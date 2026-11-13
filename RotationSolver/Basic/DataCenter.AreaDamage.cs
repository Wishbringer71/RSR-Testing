using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Config;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using RotationSolver.Basic.Configuration;
using RotationSolver.Basic.Rotations.Duties;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using Action = Lumina.Excel.Sheets.Action;
using Buddy = FFXIVClientStructs.FFXIV.Client.Game.UI.Buddy;
using CharacterManager = FFXIVClientStructs.FFXIV.Client.Game.Character.CharacterManager;
using CombatRole = RotationSolver.Basic.Data.CombatRole;

namespace RotationSolver.Basic;

internal static partial class DataCenter
{
	/// <summary>
	/// Whether the player stands above the level the current content syncs to - an unrestricted party in older content.
	/// A hit taken there is a smaller share than in sync, and every learned size - area, tankbuster, damage over time -
	/// would rate the action too small for the next synced run, where a saving decision reads it (concept 13, his test
	/// cases of 29.10.2026, A452). Content without a level sync - the open world - counts as in sync; an item level above
	/// the content's limit at the same level is not seen here.
	/// </summary>
	internal static bool AboveContentLevel
	{
		get
		{
			var sync = Territory?.ClassJobLevelSync ?? 0;
			return sync > 0 && PlayerSyncedLevel() > sync;
		}
	}

	/// <summary>
	/// Whether the running content was entered as an unrestricted party: the duty finder's last popped entry is this
	/// content and was popped so. Its item level is not synced either, so a hit is a smaller share than in a synced run,
	/// at the same level too (concept 13, A466). Content the duty finder did not bring - the open world - counts as in sync.
	/// </summary>
	internal static unsafe bool InUnrestrictedParty
	{
		get
		{
			var content = Territory?.ContentFinderConditionId ?? 0;
			var finder = ContentsFinder.Instance();
			if (content == 0 || finder == null)
			{
				return false;
			}

			var popped = finder->QueueInfo.PoppedQueueEntry;
			return finder->QueueInfo.PoppedContentIsUnrestrictedParty
				&& popped.ContentType == FFXIVClientStructs.FFXIV.Client.UI.Agent.ContentsType.Regular
				&& popped.Id == content;
		}
	}

	/// <summary>
	/// Whether a reading taken now lies outside the content's sync - above its level, or in an unrestricted party - and is
	/// therefore stored by no learned size (A452, A466).
	/// </summary>
	internal static bool OutOfContentSync => AboveContentLevel || InUnrestrictedParty;

	/// <summary>Why <see cref="OutOfContentSync"/> holds, in the words of the trace.</summary>
	internal static string OutOfContentSyncReason
		=> AboveContentLevel ? "above the level the content syncs to" : "in an unrestricted party";

	/// <summary>
	/// How many one-second samples of the party's health are kept: four minutes.
	/// </summary>
	public const int HP_RECORD_TIME = 240;

	/// <summary>
	/// The party's health, one sample a second (TargetUpdater): what a member's time to death, the heal forecast and
	/// its self-correction read (A91). Hostiles are kept by <see cref="TimeToKillTracker"/> since upstream 3c6ce9ee2,
	/// which starts over at every rise in health and so holds nothing for a member being healed (A341).
	/// </summary>
	internal static Queue<(DateTime time, Dictionary<ulong, float> hpRatios)> RecordedHP { get; } =
		new(HP_RECORD_TIME + 1);

	/// <summary>
	/// How the last measured cast of a healing action landed: the share of what it restored that met
	/// missing health, whether any target was reported a larger amount than it was missing (the packet
	/// reports the gross heal), how many targets were confirmed and how many were not.
	/// </summary>
	internal readonly record struct HealLanding(float EffectiveShare, bool GrossReported, int Confirmed, int Unconfirmed, DateTime At);

	// One cast waiting for its confirmation. The effect packet arrives with the amounts; whether the
	// health seen at that moment was still the health from before the heal is not known then - the
	// server's health update can come before or after the effect, and the heal-hp projection below
	// (GetPartyMemberHPRatio) is built for both orders. So the cast is held, and each target is
	// confirmed only when its health actually rises by what the heal can have added after the
	// effect was seen. A target whose health had already risen, or that took damage in between, is
	// never confirmed and never measured - the safe direction.
	private sealed record PendingHealTarget(ulong Id, uint Amount, uint HpAtEffect, uint MaxHp)
	{
		public bool Confirmed { get; set; }
		public uint MissingBefore => MaxHp > HpAtEffect ? MaxHp - HpAtEffect : 0;
		public uint Expected => Math.Min(Amount, MissingBefore);
	}

	private static (uint ActionId, DateTime Deadline, List<PendingHealTarget> Targets)? _pendingHeal;

	/// <summary>
	/// Holds one cast of an own healing action until its health changes confirm it; see
	/// <see cref="HealLanding"/>. <paramref name="landed"/> holds, per party member hit, the amount
	/// the effect reported and the member's health and maximum at that moment. The cast waits until
	/// the end of the effect window (EffectEndTime) - as long as the heal projection waits for the
	/// server's health update.
	/// </summary>
	internal static void RecordHealEffect(uint actionId, IReadOnlyList<(ulong Id, uint Amount, uint Hp, uint MaxHp)> landed)
	{
		ResolvePendingHeal(force: true);

		List<PendingHealTarget> targets = [];
		foreach (var (id, amount, hp, maxHp) in landed)
		{
			if (amount > 0 && maxHp > 0)
			{
				targets.Add(new PendingHealTarget(id, amount, hp, maxHp));
			}
		}

		_pendingHeal = null;
		if (targets.Count > 0)
		{
			_pendingHeal = (actionId, EffectEndTime, targets);
		}
	}

	// Called for every party member whenever the refined health is read, and before any reader of
	// the measurement. Confirms targets whose health has risen by what the heal adds; once every
	// target that could rise has, or the effect window is over, the cast is measured from its
	// confirmed targets alone.
	private static void ConfirmPendingHeal(IBattleChara member)
	{
		if (_pendingHeal is not { } pending)
		{
			return;
		}

		foreach (var target in pending.Targets)
		{
			if (!target.Confirmed && target.Id == member.GameObjectId && target.Expected > 0
				&& member.CurrentHp >= target.HpAtEffect + target.Expected)
			{
				target.Confirmed = true;
			}
		}

		ResolvePendingHeal(force: false);
	}

	private static void ResolvePendingHeal(bool force)
	{
		if (_pendingHeal is not { } pending)
		{
			return;
		}

		var open = false;
		foreach (var target in pending.Targets)
		{
			open |= !target.Confirmed && target.Expected > 0;
		}

		if (open && !force && DateTime.Now <= pending.Deadline)
		{
			return;
		}

		_pendingHeal = null;

		var confirmed = 0;
		var unconfirmed = 0;
		double restored = 0;
		double met = 0;
		var gross = false;
		foreach (var target in pending.Targets)
		{
			if (!target.Confirmed)
			{
				unconfirmed++;
				continue;
			}

			confirmed++;
			restored += target.Amount;
			met += target.Expected;
			gross |= target.Amount > target.MissingBefore;
		}

		if (confirmed > 0)
		{
			var smallest = float.MaxValue;
			foreach (var target in pending.Targets)
			{
				if (target.Confirmed && (gross || target.Amount < target.MissingBefore))
				{
					smallest = Math.Min(smallest, target.Amount);
				}
			}

			if (smallest < float.MaxValue)
			{
				_observedHealPerCast[pending.ActionId] = _observedHealPerCast.TryGetValue(pending.ActionId, out var known) && known > 0
					? Math.Min(known, smallest)
					: smallest;
			}
		}

		_lastHealLanding[pending.ActionId] = new HealLanding(
			restored > 0 ? (float)(met / restored) : 0f, gross, confirmed, unconfirmed, DateTime.Now);
	}

	/// <summary>
	/// What one cast of this action restores to one target, in health points, or 0 when it has not
	/// been observed since the last territory change. 0 means "unknown", never "heals nothing" - a
	/// caller that cannot act on an unknown value keeps its previous behaviour instead of assuming one.
	/// </summary>
	public static float GetObservedHealPerCast(uint actionId)
	{
		ResolvePendingHeal(force: false);
		return _observedHealPerCast.TryGetValue(actionId, out var known) ? known : 0f;
	}

	/// <summary>
	/// Clears the measured heals. Called on a territory change only, not with ResetAllRecords - that
	/// one runs after every fight, and a heal measured in the last pull is the right figure for the
	/// next. Item level sync is set per duty, so a figure from outside is wrong inside.
	/// </summary>
	internal static void ResetHealMeasurements()
	{
		_observedHealPerCast.Clear();
		_lastHealLanding.Clear();
		_pendingHeal = null;
	}

	/// <summary>How the last cast of this action landed, or null when none has been seen.</summary>
	internal static HealLanding? GetLastHealLanding(uint actionId)
	{
		ResolvePendingHeal(force: false);
		return _lastHealLanding.TryGetValue(actionId, out var landing) ? landing : null;
	}

	private static ulong _closingTargetId;

	private static float _closingLastDistance;

	private static float _closingSpeed;

	/// <summary>
	/// Whether the single hit that opens the single-target defence reaches the player himself - the
	/// question for the actions that protect only him, as <see cref="AreaHitReachesPlayer"/> is for the
	/// area defence (concept 13, A233).
	/// </summary>
	/// <remarks>
	/// The flag is measured by the paths it opens, and a tank's single-target path also carries help for
	/// the other tank - Intervention, Heart of Corundum on the tankbuster's target, Reprisal on the enemy -
	/// so the flag itself stays as it is. A tank's own cooldowns, though, were spent on every announced
	/// tankbuster and every cast an enemy aimed at its own target, whoever that was: Damnation and Rampart
	/// for a buster BossModReborn had marked as not hitting him (the owner's trace, 30.09.2026).
	///
	/// Sources: a tankbuster marker on the player or a listed tankbuster cast at him; for a tank also any
	/// cast an enemy aims at him as its target, and the pull rule; and an announced BossModReborn tankbuster
	/// when its mask names him. An unknown mask counts for a tank - the chance is not measured then, and
	/// safety comes first - and for anyone else only with no living tank in the party (A220).
	/// </remarks>
	public static bool SingleHitReachesPlayer
	{
		get
		{
			var player = Player.Object;
			if (!InCombat || player == null)
			{
				return false;
			}

			if (IsHostileCastingTankBusterAtMe)
			{
				return true;
			}

			var isTank = Role == JobRole.Tank;
			if (isTank && (TankPullOnPlayer || IsAnyHostileCastingAtPlayer(player.GameObjectId)))
			{
				return true;
			}

			// An enemy attacking him with no healer alive, any role: the hit is his (A361).
			if (UnhealedUnderAttack)
			{
				return true;
			}

			// A measured cast aimed at him, a non-tank, that calls for a defence (A540).
			if (!isTank && TankbusterHelp.AimedHitCallsForDefence(player, out _))
			{
				return true;
			}

			return BMRTankbusterReachesPlayer;
		}
	}

	// The tank's reading of an unlisted cast - an enemy casting at the one it is attacking - narrowed to
	// casts whose target is the player.
	private static bool IsAnyHostileCastingAtPlayer(ulong playerId)
	{
		var hostiles = AllHostileTargets;
		if (hostiles == null)
		{
			return false;
		}

		for (var i = 0; i < hostiles.Count; i++)
		{
			var h = hostiles[i];
			if (h != null && h.CastTargetObjectId == playerId && IsHostileCastingTank(h))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Whether the hostile is casting an action from the curated tankbuster list, without the
	/// target-identity fallback that <see cref="IsHostileCastingTank"/> adds on top of it.
	/// </summary>
	public static bool IsHostileCastingListedTank(IBattleChara h)
	{
		return h != null && IsHostileCastingBase(h, (act) =>
		{
			return OtherConfiguration.HostileCastingTank.Contains(act.RowId);
		});
	}

	/// <summary>
	/// Whether a tankbuster lock-on VFX currently sits on the player. Read-only counterpart to
	/// <see cref="IsCastingTankVfx"/>, which also rebuilds <see cref="TankbusterTargetIds"/> and
	/// answers for the party rather than for the player alone.
	/// </summary>
	public static bool IsTankbusterVfxOnPlayer()
	{
		var player = Player.Object;
		return player != null && IsTankbusterVfxOn(player.GameObjectId);
	}

	/// <summary>
	/// What the one tankbuster aimed at <paramref name="member"/> takes and leaves him, by the tankbuster table, as shares
	/// of his maximum HP: <paramref name="share"/> under the mitigation that still stands at the hit
	/// (<see cref="TankbusterTable.PredictedShare"/>), <paramref name="left"/> his HP now plus the barriers that still
	/// stand then, less that share. <paramref name="actionId"/> names the hit. False when this cannot be said: nothing or
	/// more than one tankbuster aimed at him, a hit the table has not measured, a marker with neither a cast nor a learned
	/// hit, a BossModReborn prediction on him, or a member who is not a tank - the table rates hits on tanks (A312).
	/// </summary>
	/// <remarks>
	/// Recognition, the footing of the tankbuster plan (concept 09, "Die Rechnung"): the highest measured sum of a cast's
	/// hits, HP now, only what stands until its last hit - the cast's rest plus one GCD plus the series' span -, no heal
	/// that might come. The hit is a cast at him, a
	/// tankbuster cast at no other member while a marker stands on him, or with a marker and no such cast the hit learned
	/// after the same marker in this area (<see cref="TankbusterMarkerWatch.LearnedHitOn"/>), counted without what lies
	/// on an enemy, as the plan counts it. For help from others: whether it is called for is the reader's question (A373).
	/// </remarks>
	/// <summary>
	/// Whether an enemy casts a measured hit at <paramref name="member"/>, a member who is no tank, within the flag's landing
	/// window (<see cref="IsHostileCastingBase"/>), and what it takes and leaves him: the share of his maximum HP the table
	/// of aimed hits forecasts under the mitigation standing now, and his forecast effective health less that share. The
	/// largest such cast counts (concept 13, "Auf einen Nicht-Tank gezielt", A540). Detection only: whether it calls for
	/// a defence is <see cref="TankbusterHelp.AimedHitCallsForDefence"/>'s question.
	/// </summary>
	/// <summary>
	/// The area table's figure for <paramref name="actionId"/>, zero when it has none: for the danger an enemy announces
	/// (<see cref="EnemyDanger"/>), a reader that only protects - a higher figure makes the caster more urgent, and none
	/// leaves it as it was (concept 13, "Stufe 2", A541).
	/// </summary>
	internal static float LearnedAreaShare(uint actionId)
	{
		return OtherConfiguration.HostileCastingAreaPotential.TryGetValue(actionId, out var share) ? share : 0f;
	}

	internal static bool AimedHitLeaves(IBattleChara member, out uint actionId, out float share, out float left,
		out IBattleChara? caster)
	{
		actionId = 0;
		share = 0f;
		left = 0f;
		caster = null;
		if (member == null || member.IsDead || member.MaxHp == 0 || member.IsJobCategory(JobRole.Tank))
		{
			return false;
		}

		var casters = AreaCasters;
		if (casters == null)
		{
			return false;
		}

		var id = member.GameObjectId;
		for (var i = 0; i < casters.Count; i++)
		{
			var h = casters[i];
			if (h == null || !h.IsValid() || h.CastTargetObjectId != id || !IsHostileCastingBase(h, _ => true))
			{
				continue;
			}

			var predicted = TankbusterTable.PredictedAimedShare(h.CastActionId, member, h);
			if (predicted > share)
			{
				share = predicted;
				actionId = h.CastActionId;
				caster = h;
			}
		}

		if (share <= 0f)
		{
			return false;
		}

		left = (member.GetForecastEffectiveHp() / (float)member.MaxHp) - share;
		return true;
	}

	public static bool IsCastingAreaVfx()
	{
		return IsCastingVfx(VfxDataQueue, s =>
		{
			if (!Player.Available || Player.Object == null)
			{
				return false;
			}

			if (string.IsNullOrEmpty(s.Path))
			{
				return false;
			}

			// Multi-hit markers (treated as area/stack mechanics), regular shared and spread markers.
			if (!StartsWithAny(s.Path, MultiHitSharedPaths) && !StartsWithAny(s.Path, SharedDamagePaths)
				&& !StartsWithAny(s.Path, SpreadDamagePaths))
			{
				return false;
			}

			// A marker on someone outside the party - another alliance group's stack - hits none of us,
			// and the flag this raises opens the party's mitigations (concept 13, A220). Asked after the path:
			// it searches the object table, and asked first it did so for every queued effect (A325).
			return IsOwnPartyMarker(s.ObjectId);
		});
	}

	public static bool IsHostileCastingTank(IBattleChara h)
	{
		return h != null && IsHostileCastingBase(h, act => OtherConfiguration.HostileCastingTank.Contains(act.RowId)
			|| h.CastTargetObjectId == h.TargetObjectId);
	}

	public static bool IsHostileCastingArea(IBattleChara h)
	{
		return IsHostileCastingBase(h, (act) =>
		{
			// Contains, not a walk over the set. All four of these lists were searched with a
			// foreach that compared every id in turn - O(n) out of a structure whose whole purpose
			// is O(1). The area list ships with 850 entries and grows in play, so the cost of the
			// question grew with the list while it could have been constant.
			//
			// Not a hot-path emergency: IsHostileCastingBase only reaches this predicate while an
			// enemy is casting something uninterruptible that is longer than a GCD and sits in a
			// one-GCD window before it lands, so a trash pull barely gets here. It is simply wrong
			// as built, and it is the reason the list cannot be allowed to grow freely - see
			// docs/rotation-flow/13-aoe-damage-classification.md.
			return OtherConfiguration.HostileCastingArea.Contains(act.RowId)
				&& AreaCastReachesParty(h, act)
				&& AreaCastIsWorthMitigating(act.RowId, h);
		});
	}

	// When a reader last passed over a figure for want of confirmation, by what it did and the action: asked every frame,
	// a note less than a GCD after the last is the same cast.
	private static readonly ConcurrentDictionary<(string What, uint ActionId), DateTime> _unconfirmedNoted = new();

	/// <summary>
	/// One trace line per cast where a reader that would leave protection out on a learned figure does not, because the
	/// running game version has not confirmed it (concept 13, "Stufe 2"). <paramref name="what"/> says what happens
	/// instead.
	/// </summary>
	internal static void NoteUnconfirmedReading(string what, uint actionId, float figure)
	{
		var now = DateTime.Now;
		var key = (what, actionId);
		var due = !_unconfirmedNoted.TryGetValue(key, out var last) || (now - last).TotalSeconds > DefaultGCDTotal;
		_unconfirmedNoted[key] = now;
		if (due)
		{
			var name = Service.GetSheet<Action>().GetRowOrDefault(actionId)?.Name.ExtractText() ?? "?";
			DefenseTrace.Line($"learned figure not confirmed in this game version: {name} #{actionId} at {figure:P0} - {what}");
		}
	}

	private static int _holdsVindicated;

	private static int _holdsWasted;

	/// <summary>
	/// How often holding a predicted mitigation was followed by a big hit, against how often it was
	/// not. Diagnostic read-out of a figure the rule already acts on by itself.
	/// </summary>
	public static (int Vindicated, int Wasted) ProactiveHoldRecord => (_holdsVindicated, _holdsWasted);

	/// <summary>
	/// Whether the hold has earned the right to keep holding.
	/// </summary>
	/// <remarks>
	/// <para>Starts open, because a rule that never fires cannot learn anything. From the first
	/// scored hold it requires that holding has been right at least as often as it was wrong - a
	/// simple majority, and no invented threshold: below parity the hold loses more mitigation than
	/// it saves, which is the exact break-even of the trade it makes.</para>
	///
	/// <para>Not persisted. A fight is the unit that matters, and carrying a verdict from one fight
	/// into another would judge one boss by another's pattern.</para>
	/// </remarks>
	public static bool ProactiveHoldIsEarningItsKeep
	{
		get
		{
			// A hold whose window ran out with nothing in it is settled here rather than waiting for
			// the next hit to settle it: if nothing lands at all, no hit ever arrives to close it,
			// and the wrong prediction would stay unscored forever. This property is read every
			// frame the rule runs, so the window closes reliably.
			if (_holdExpectsHitUntil != DateTime.MinValue && DateTime.Now > _holdExpectsHitUntil)
			{
				_holdsWasted++;
				_holdExpectsHitUntil = DateTime.MinValue;
			}

			return _holdsVindicated + _holdsWasted == 0 || _holdsVindicated >= _holdsWasted;
		}
	}

	/// <summary>
	/// Records that a predicted mitigation was held, and until when a big hit would justify it.
	/// </summary>
	internal static void NoteProactiveHold(float window)
	{
		_holdExpectsHitUntil = DateTime.Now.AddSeconds(window);
	}

	/// <summary>
	/// Called for every hit the player takes, to settle an open hold. A hit at or above the
	/// large-barrier share inside the window is what the hold was waiting for.
	/// </summary>
	internal static void ScoreProactiveHold(float incomingShare)
	{
		if (_holdExpectsHitUntil == DateTime.MinValue)
		{
			return;
		}

		if (DateTime.Now > _holdExpectsHitUntil)
		{
			// The window closed with nothing big in it - the hold gave away a mitigation for nothing.
			_holdsWasted++;
			_holdExpectsHitUntil = DateTime.MinValue;
			return;
		}

		if (incomingShare >= LargeShieldShare)
		{
			_holdsVindicated++;
			_holdExpectsHitUntil = DateTime.MinValue;
		}
	}

	// How large the announced area cast is, as a share of the weakest member's maximum health, and
	// when that was last established. This is the same figure AreaCastIsWorthMitigating already
	// looks up to decide WHETHER to answer; kept here it also answers WITH WHAT.
	//
	// Held with a timestamp rather than cleared from somewhere else: the predicate runs while an
	// enemy is casting and simply stops running when nothing is. A clearing point would have to be
	// found and kept correct in a second place; an age does not.
	private static float _announcedAreaShare;

	// Whether the running game version has confirmed the announced figure; the readers that would leave protection out
	// on it ask (concept 13, "Stufe 2").
	private static bool _announcedAreaShareConfirmed;

	// Which action the recorded share belongs to, so a probe can name it rather than counting
	// anonymously. Carried with the share and subject to the same lifetime.
	private static uint _lastAnnouncedAreaAction;

	// One GCD's worth - the player's, as the game reports it (A340). The figure describes the cast that is
	// landing now, so it must not outlive it and steer the next decision; long enough for the defensive
	// branch of the same window to read it.
	private static TimeSpan AnnouncedAreaShareLifetime => TimeSpan.FromSeconds(DefaultGCDTotal);

	/// <summary>
	/// The share of the weakest party member's maximum health carried by the area cast that is
	/// currently announced, under the mitigation standing when it lands (<see cref="AreaShareNow"/>), or 0
	/// when none is announced or its size has not been measured yet. 0 means "unknown", never "harmless".
	/// </summary>
	public static float AnnouncedAreaShare =>
		DateTime.Now - _announcedAreaShareTime <= AnnouncedAreaShareLifetime ? _announcedAreaShare : 0f;

	/// <summary>
	/// An area cast is running right now whose measured damage is small - small enough that the
	/// reactive rule would not spend a cooldown on it. False when nothing is running, when the
	/// running cast has never been measured, or when it is large.
	/// </summary>
	/// <remarks>
	/// <para>The counterpart to <see cref="AreaCastIsWorthMitigating"/> for the PROACTIVE side.
	/// BossModReborn answers when the next damage arrives, never how hard, and that is enough to
	/// spend a cooldown on the wrong one of two hits in a row: reported from Eternal Queen's
	/// opening, a small area cast followed by a big one, where the barrier is eaten by the first.</para>
	///
	/// <para>The size of a cast already on screen IS known, because it is measured per action. So a
	/// rated small cast running now is the most likely referent of a prediction pointing at the
	/// immediate future, and a proactive refresh can wait for the next one. A heuristic, and stated
	/// as one: nothing here proves the prediction means this cast.</para>
	///
	/// <para><b>The exact question - which action is the prediction pointing at, so its rating can be
	/// looked up - cannot be asked. Measured against BossmodReborn's own source on 20.09.2026, not
	/// assumed:</b></para>
	/// <list type="bullet">
	/// <item><c>Timeline.NextRaidwideIn</c> returns
	/// <c>module.StateMachine.NextTransitionWithFlag(StateHint.Raidwide)</c> - a transition in the
	/// module's state machine that its author flagged as "a raidwide happens here". There is no cast
	/// and no action id in it at all.</item>
	/// <item><c>Hints.NextRaidwideDamageIn</c> returns the activation time of the first
	/// <c>PredictedDamage</c> entry of that type, and that struct carries exactly three fields:
	/// <c>Players</c> (a bitmask), <c>Activation</c> and <c>Type</c>. No action.</item>
	/// <item>The prediction list is not exposed either - each endpoint returns the FIRST matching
	/// entry. Even with sizes available, "two are coming and the second is the big one" could not be
	/// read out, which is precisely the reported case.</item>
	/// </list>
	/// <para>So the rating cannot be looked up for a predicted event. What is running now is the only
	/// size information available at that moment, and this is what it is worth.</para>
	///
	/// <para>"Small" is the same question the reactive rule asks - would this hit push anyone to
	/// where the tree heals anyway - so the two sides cannot disagree about the same cast.</para>
	/// </remarks>
	public static bool AnnouncedHitIsSmall
	{
		get
		{
			var share = AnnouncedAreaShare;
			if (share <= 0f || share >= LargeShieldShare)
			{
				return false;
			}

			// A figure the running game version has not confirmed saves nothing: not small (concept 13, "Stufe 2").
			if (!_announcedAreaShareConfirmed)
			{
				NoteUnconfirmedReading("the proactive refresh does not wait", _lastAnnouncedAreaAction, share);
				return false;
			}

			return !AnnouncedHitDropsAnyoneBelow(Service.Config.HealthAreaSpell);
		}
	}

	/// <summary>
	/// What the effect handler received since loading: every effect set, those from enemies, those
	/// that damaged the player, and the ones it failed on.
	/// </summary>
	/// <remarks>
	/// The tally below starts only once an enemy cast has hurt the player, so on its own it cannot
	/// tell a handler that receives nothing from casts that never reached it. Zero sets means the
	/// hook delivers nothing; sets without enemy hits mean the filter before the measurement drops
	/// them; errors mean the handler stops before it measures.
	/// </remarks>
	public static int EffectSetsReceived { get; internal set; }

	/// <inheritdoc cref="EffectSetsReceived"/>
	public static int EnemyEffectSets { get; internal set; }

	/// <inheritdoc cref="EffectSetsReceived"/>
	public static int EnemyHitsOnPlayer { get; internal set; }

	/// <inheritdoc cref="EffectSetsReceived"/>
	public static int EffectHandlerErrors { get; internal set; }

	/// <summary>
	/// How often each reason decided an enemy cast that hit the player, since the plugin was loaded.
	/// </summary>
	/// <remarks>
	/// The last outcome alone cannot say why a table stays empty over a whole evening: one line shows
	/// one hit. The tally shows whether casts arrive at all and which gate stops them.
	/// </remarks>
	public static IReadOnlyDictionary<string, int> AreaMeasurementTally => _areaMeasurementTally;

	/// <summary>
	/// The tally as one line for the windows.
	/// </summary>
	public static string AreaMeasurementTallyText
	{
		get
		{
			if (_areaMeasurementTally.Count == 0)
			{
				return "no enemy cast has hit the player since loading";
			}

			var parts = new List<string>(_areaMeasurementTally.Count);
			foreach (var (reason, count) in _areaMeasurementTally)
			{
				parts.Add($"{reason}: {count}");
			}

			return string.Join(" | ", parts);
		}
	}

	/// <summary>
	/// Records the outcome of one enemy cast for the damage table: <paramref name="reason"/> is the
	/// tallied category, <paramref name="detail"/> what the last-outcome line adds to it.
	/// </summary>
	public static void RecordAreaMeasurementOutcome(uint actionId, string reason, string detail = "")
	{
		AreaMeasurementLastOutcome = $"{DateTime.Now:HH:mm:ss} #{actionId}: {reason}{detail}";
		_areaMeasurementTally[reason] = _areaMeasurementTally.TryGetValue(reason, out var count) ? count + 1 : 1;
	}

	/// <summary>
	/// The last time an area heal centred on the caster was weighed on the heal path: which action,
	/// how many hurt members stood in its radius against how many it asks for, whether one of them
	/// was under its heal ratio. Written by the targeting,
	/// shown in the diagnostics window, so a group heal that stays out can be told apart from one
	/// that was never asked.
	/// </summary>
	internal static SelfCentredHealWeighing? LastSelfCentredHeal { get; set; }

	/// <summary>One decision of a defense hold.</summary>
	internal readonly record struct DefenseHoldDecision(string Rule, bool Held, string Why, DateTime At);

	/// <summary>One weighing of an area heal centred on the caster.</summary>
	internal readonly record struct SelfCentredHealWeighing(string Action, int HurtInRadius, int Required, bool InNeed, DateTime At);

	/// <summary>
	/// The last time the movement safety check withheld an action that moves the player, and why.
	/// Without it a gap closer that stays out cannot be told apart in the fight from one that was
	/// never asked for.
	/// </summary>
	internal static MoveSafetyRefusal? LastMoveSafetyRefusal { get; set; }

	/// <summary>
	/// The last refusal the safety check gave without measuring anything, because the movement had no
	/// target to measure against.
	/// </summary>
	internal static MoveSafetyRefusal? LastMoveSafetyUnmeasured { get; set; }

	/// <summary>One refusal of the movement safety check.</summary>
	internal readonly record struct MoveSafetyRefusal(string Action, string Why, DateTime At);

	/// <summary>
	/// The action id the currently recorded area share belongs to, or 0 when none is recorded.
	/// </summary>
	public static uint AnnouncedAreaAction =>
		DateTime.Now - _announcedAreaShareTime <= AnnouncedAreaShareLifetime ? _lastAnnouncedAreaAction : 0u;

	/// <summary>
	/// Whether the announced area cast would put any living party member below the given share of
	/// their maximum health. False when nothing is announced or its size has not been measured -
	/// unknown is not "harmless", it is simply no reason to act.
	/// </summary>
	/// <remarks>
	/// <para>The mitigation side's question (<see cref="AreaCastIsWorthMitigating"/>,
	/// <see cref="AnnouncedHitIsSmall"/>): does this hit create a need to heal. Every healing threshold in
	/// the tree reads the health a member HAS; none of them reads the health he will have when the cast
	/// that is already on screen lands. Whether to heal before the hit is a different question with a
	/// different line, <see cref="AnnouncedHitCallsForHealFirst"/> (A292).</para>
	///
	/// <para>The barrier counts here, and that is not a contradiction of A85. A85 removed the
	/// barrier from the general healing threshold, because a barrier does not restore health - a
	/// tank at 40% behind a shield is still at 40% once it expires unspent. This question is a
	/// different one: does he survive THIS hit, the one being cast right now. Against that hit the
	/// barrier is spent and does absorb it, so leaving it out would call for healing that the shield
	/// has already paid for.</para>
	/// </remarks>
	public static bool AnnouncedHitDropsAnyoneBelow(float threshold)
	{
		return AnnouncedHitDropsBelow(threshold, false, false, out _);
	}

	/// <summary>
	/// <see cref="AnnouncedHitDropsAnyoneBelow"/> for the members the hit can actually take down: an
	/// invulnerable tank sits low on purpose and is not in danger from it (Hallowed Ground,
	/// Superbolide). <paramref name="who"/> names the first member found.
	/// </summary>
	internal static bool AnnouncedHitDropsUnprotectedBelow(float threshold, out string who)
	{
		return AnnouncedHitDropsBelow(threshold, true, false, out who);
	}

	/// <summary>
	/// What a large shield absorbs, as a share of maximum HP: the point from which an area hit counts
	/// as a big one regardless of how healthy the party is.
	/// </summary>
	/// <remarks>
	/// Not an invented number, and no longer a written-down one either: it is read from the effect
	/// texts. The largest barrier in the tree that states its size as a share and can be put on
	/// another party member is The Blackest Night - "absorbs damage totaling 25% of target's maximum
	/// HP" - and DefensiveValues.g.cs carries that figure along with every other, generated from the
	/// same sheets and checked against them in CI. Manaward states 30%, but only its caster carries
	/// it: it says nothing about what a shield on the member being hit would absorb.
	///
	/// It used to be a literal 0.25f with the source named in prose. That is the ageing form this
	/// file has been caught by twice already: an earlier version of this remark said "five barriers"
	/// and listed their row ids, and the generated table finds more than five. A patch that restates
	/// a barrier, or a new job action with a larger one, now moves this threshold with it instead of
	/// leaving a number behind that nothing reads as wrong.
	///
	/// The smaller barriers name 10%, 15% and 20%, which is the other end of the user's requirement:
	/// below a small shield the hit only matters to someone already low. That end needs no constant,
	/// because the buffer comparison below is exactly that question.
	///
	/// Concept 13 once dismissed the two-threshold form as "not implementable without invented
	/// numbers, and not needed, because the buffer comparison answers the same question". Both halves
	/// were wrong: the barriers state their share, and the buffer comparison answers a different
	/// question - whether healing would be needed, not whether the hit is large.
	/// </remarks>
	private static float LargeShieldShare => DefensiveValues.LargestStatedBarrierShare;

	/// <summary>
	/// Whether an incoming area cast is big enough that the party mitigation is worth its cooldown.
	/// </summary>
	/// <remarks>
	/// The user's requirement: an area action whose damage stays below what a small shield absorbs
	/// does not need to be treated as a big hit; one at or above the size of a large shield does.
	/// What is actually spent on a trivial hit is not the shield but the **cooldown** - a Reprisal
	/// laid on a two-percent tick is missing at the next real one.
	///
	/// The measure is not a threshold anybody had to invent. It is the same question the healing
	/// side already answers: **does this hit create a need to heal?** Buffer minus expected hit
	/// against the level at which the tree would heal by itself. Two percent on a full player does
	/// not; thirty does. The same action is therefore minor for a healthy tank and major for a
	/// wounded caster, which no stored category could express - and it fits the standing order that
	/// healing comes before mitigation, because mitigation goes where healing would otherwise be
	/// needed.
	///
	/// An earlier draft compared against HealthForDyingTanks (0.15). Working it through showed it
	/// would practically never mitigate: a thirty-percent hit leaves a full player at seventy. A rule
	/// that cannot fire in its own domain is no rule - the same shape C59 found on the Holy hold.
	///
	/// Unrated actions return true, which is the behaviour the tree has always had. That is what
	/// keeps the 850 shipped entries from losing their mitigation the moment this ships: an entry
	/// only enters the arithmetic once an actual hit has been measured, and in content that is
	/// repeated - an extreme trial, a savage fight in progression - that is one clear.
	/// </remarks>
	private static bool AreaCastIsWorthMitigating(uint actionId, IBattleChara caster)
	{
		RecordAnnouncedAreaShare(actionId, caster);

		if (!Service.Config.SkipMitigationForSmallAreaCasts)
		{
			return true;
		}

		// Unrated: behave exactly as before. A rating of 0 is a landing measured without damage (A366), a small cast.
		if (!AreaShareNow(actionId, caster, out var share))
		{
			return true;
		}

		if (AreaShareCallsForMitigation(share))
		{
			return true;
		}

		// Taken over from an earlier game version and not confirmed since: mitigated as unrated, the asymmetric rule he
		// decided (A451, concept 13, "Stufe 2") - a patch that made the hit harder must not take the first cast's cover.
		if (!OtherConfiguration.AreaShareConfirmed(actionId))
		{
			NoteUnconfirmedReading("the area mitigation goes as for an unrated cast", actionId, share);
			return true;
		}

		// One trace line per cast, as the BossModReborn path writes one per prediction (NoteBmrRaidwideHeld): the
		// predicate is asked every frame, so a skip less than a GCD after the last is the same cast. Without it a cast
		// the size rule let through and one the recognition never saw looked alike in the trace of 18.-22.10.2026 (A334).
		var now = DateTime.Now;
		if (!AreaMitigationSkipped.TryGetValue(actionId, out var last) || (now - last).TotalSeconds > DefaultGCDTotal)
		{
			var name = Service.GetSheet<Action>().GetRowOrDefault(actionId)?.Name.ExtractText() ?? "?";
			DefenseTrace.Line($"area defence skipped: {name} #{actionId}, expected at {share:P0} under the mitigation standing,"
				+ " which leaves everybody above the area heal line");
		}

		AreaMitigationSkipped[actionId] = now;
		return false;
	}

	/// <summary>
	/// The share of maximum HP a listed area action of <paramref name="caster"/> is expected to take when it lands: its
	/// stored unmitigated figure times what gets through the mitigation that still stands then - on the caster, and on
	/// the living party member it protects least, whoever laid it - with a raise of his maximum counted
	/// (<see cref="TankbusterTable.MitigationFactor"/>, A441). False when the action has not been measured; a rating of 0
	/// is a landing measured without damage (A366).
	/// </summary>
	/// <remarks>
	/// The table holds the hit as it lands unmitigated; read raw, a raidwide whose first landing the party took under
	/// three mitigations and a barrier stood at 25 % instead of about 34 % (Seelenüberfluss, 26.10.2026, A419), and the
	/// bell, the heal ahead and the size rule read the small figure. Read here, mitigation another player has laid for
	/// this hit makes it smaller, and one that runs out before the cast ends does not.
	/// <para>A reading stored before that form (<see cref="OtherConfiguration.AreaPotentialUnmitigated"/> without the
	/// action) is the hit as it arrived, often under mitigation; taking the mitigation standing now off it again read a
	/// raidwide stored at 25 % as 22.5 % under another player's 10 %, below the size of a large barrier, and the size rule
	/// could skip the mitigation it gave before. Such a reading is read as before until the action is measured (A446).</para>
	/// <para>A series of hits (<see cref="OtherConfiguration.AreaSeriesSpan"/>) is read under the mitigation that still
	/// stands at its last hit, not at the end of the bar: Bodyweight Exorcism's four towers land over 9 s, and a party
	/// mitigation that ended before the third had counted for all four, so the size rule could leave its own out (A460).
	/// One that covers only the first hits counts for none - the safe side.</para>
	/// </remarks>
	internal static bool AreaShareNow(uint actionId, IBattleChara? caster, out float share)
	{
		if (!OtherConfiguration.HostileCastingAreaPotential.TryGetValue(actionId, out var unmitigated))
		{
			share = 0f;
			return false;
		}

		if (!OtherConfiguration.AreaPotentialUnmitigated.ContainsKey(actionId))
		{
			share = unmitigated;
			return true;
		}

		// A series hits for its learned span after the cast lands: only what stands until its last hit counts (A460).
		var horizon = (caster != null && caster.IsCasting ? Math.Max(0f, caster.TotalCastTime - caster.CurrentCastTime) : 0f)
			+ OtherConfiguration.AreaSeriesSpan.GetValueOrDefault(actionId);
		var through = 0f;
		var party = PartyMembers;
		for (var i = 0; i < party.Count; i++)
		{
			var member = party[i];
			if (member != null && !member.IsDead && member.MaxHp > 0)
			{
				through = Math.Max(through, TankbusterTable.MitigationFactor(actionId, member, caster, horizon)
					/ (1f + TankbusterTable.MaxHpRaise(member, horizon)));
			}
		}

		share = unmitigated * (through > 0f ? through : 1f);
		return true;
	}

	/// <summary>
	/// The size rule of <c>Skip mitigation for small area casts</c>, for a measured share: whether a hit of this
	/// size calls for the party mitigation. Asked by the cast path and by the BossModReborn path (A301), so the
	/// two cannot disagree about the same cast.
	/// </summary>
	private static bool AreaShareCallsForMitigation(float share)
	{
		// Above the size of a large shield the hit is a big one, whatever the party's health - the
		// user's requirement says so in as many words, and the party's health does not change how
		// hard it lands. Without this arm the rule asks only whether healing would be needed, and a
		// healthy party answers no to almost every raidwide: at a buffer of 1.0 against the area
		// heal threshold of 0.65, mitigation would need a share above 0.35 to happen at all. That
		// is what took Addle and Radiant Aegis out of the fight on Summoner.
		if (share >= LargeShieldShare)
		{
			return true;
		}

		if (PartyMembers.Count == 0)
		{
			return true;
		}

		// Anyone the hit would push to where healing would be called for is reason enough - see
		// AnnouncedHitDropsAnyoneBelow, including why the barrier counts here and not in the general
		// healing threshold (A85). Healing before the hit asks a stricter line of its own (A292).
		return HitDropsBelow(share, Service.Config.HealthAreaSpell, false, false, out _);
	}

	/// <summary>Whether a listed area cast can reach the player (concept 13, A218).</summary>
	/// <remarks>
	/// Why reach is asked at all (6588832b9): hostiles are collected out to 48 yalms and the area list
	/// only records that an action once hit a whole party, never whether anyone is inside this instance
	/// of it - so without a reach check any listed cast anywhere in that radius opened the area defence.
	/// </remarks>
	private static bool AreaCastCanReachPlayer(IBattleChara h, Action act)
	{
		var player = Player.Object;
		return player != null && AreaCastReaches(h, act, player);
	}

	/// <summary>
	/// Whether the enemy cast <paramref name="h"/> is casting reaches the player, from the shape the game data gives it -
	/// listed or not. Read by <see cref="HitSeries"/> for the first hit of a series that is announced by a cast.
	/// </summary>
	internal static bool CastReachesPlayer(IBattleChara h)
	{
		return h != null && h.IsValid() && h.IsCasting && TryGetCastAction(h, out var act) && AreaCastCanReachPlayer(h, act);
	}

	/// <summary>
	/// Whether a listed area cast reaches the party in the sense the area-defence flag needs: the
	/// player, or at least two living party members - more than a single target (concept 13, A218).
	/// </summary>
	/// <remarks>
	/// The flag opens party mitigations - Reprisal, Divine Veil, Sacred Soil, Addle, Feint - and a flag
	/// is measured by the paths it opens, not by whom it is set for. Measured by the player alone, a
	/// healer standing clear of a cleave on the tank and the melee gave them no party mitigation. The
	/// actions that protect only the player ask <see cref="AreaHitReachesPlayer"/> instead. The sleep that
	/// stops an area cast asks the same of any cast, listed or not (SleepInterruptWatch).
	/// </remarks>
	internal static bool AreaCastReachesParty(IBattleChara h, Action act)
	{
		if (AreaCastCanReachPlayer(h, act))
		{
			return true;
		}

		var reached = 0;
		foreach (var member in PartyMembers)
		{
			if (member != null && !member.IsDead && AreaCastReaches(h, act, member) && ++reached >= 2)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Whether the current BossModReborn raidwide is the landing of listed area casts that are still being cast
	/// and that the size rule of <c>Skip mitigation for small area casts</c> leaves unmitigated (A301).
	/// <paramref name="largest"/> gives the largest of their measured shares and <paramref name="actionId"/> its
	/// action.
	/// </summary>
	/// <remarks>
	/// False whenever anything is unknown: no cast came with the prediction, one of them is not listed or not
	/// measured, one has ended - after a cast ends a later hit may stand behind its prediction - or one calls
	/// for mitigation.
	/// </remarks>
	internal static bool BmrRaidwideIsSmallListedCast(out float largest, out uint actionId)
	{
		largest = 0f;
		actionId = 0;
		var casts = _bmrRaidwideCasts;
		if (casts.Count == 0)
		{
			return false;
		}

		foreach (var cast in casts)
		{
			if (Svc.Objects.SearchById(cast.CasterId) is not IBattleChara caster || !caster.HasNativeState()
				|| !caster.IsCasting || caster.CastActionId != cast.ActionId
				|| !OtherConfiguration.HostileCastingArea.Contains(cast.ActionId)
				|| !AreaShareNow(cast.ActionId, caster, out var share)
				|| AreaShareCallsForMitigation(share))
			{
				return false;
			}

			// Small by a figure the running game version has not confirmed: not held (concept 13, "Stufe 2").
			if (!OtherConfiguration.AreaShareConfirmed(cast.ActionId))
			{
				NoteUnconfirmedReading("the BossModReborn raidwide is not held", cast.ActionId, share);
				return false;
			}

			if (share > largest)
			{
				largest = share;
				actionId = cast.ActionId;
			}
		}

		return true;
	}

	/// <summary>
	/// Records that the area defence was held for the current BossModReborn raidwide because it is the landing of
	/// a small listed cast: the probe of the cast path, and one trace line per prediction.
	/// </summary>
	internal static void NoteBmrRaidwideHeld(float share, uint actionId)
	{
		AreaMitigationSkipped[actionId] = DateTime.Now;
		if (_bmrRaidwideHeldTracedFor == _bmrRaidwideLandsAt)
		{
			return;
		}

		_bmrRaidwideHeldTracedFor = _bmrRaidwideLandsAt;
		var name = Service.GetSheet<Action>().GetRowOrDefault(actionId)?.Name.ExtractText() ?? "?";
		DefenseTrace.Line($"area defence held: BossModReborn's raidwide in {BMRNextRaidwideIn:F1} s came with"
			+ $" {name} #{actionId}, rated {share:P0}, which leaves everybody above the area heal line");
	}

	/// <summary>
	/// Whether area damage on the party is announced, from any source, without asking how hard it lands:
	/// an area marker on the party, a listed area cast reaching the party, a large measured one even when
	/// interruptible, or a BossMod raidwide inside the mitigation window. A recognition, not a decision -
	/// unlike <see cref="IsHostileCastingAOE"/>, which drops what is not worth mitigating (concept 13). The
	/// heal target's danger class 3 ranks by points while this holds, because a raidwide takes the same
	/// number from everybody (concept 07).
	/// </summary>
	public static bool AreaHitAnnounced => InCombat
		&& (IsCastingAreaVfx() || IsHostileCastingAreaUnrated || IsHostileCastingLargeArea
			|| (Service.Config.UseBmrTimeline && BMRNextRaidwideIn > 0f
				&& BMRNextRaidwideIn <= Service.Config.BMRRaidwideMitWindow));

	/// <summary>
	/// An announced area hit that has not landed yet: the area-defense signal, or a BossMod raidwide
	/// still ahead inside the mitigation window - including the last moment before it, where the
	/// signal already lets go. What the channel locks of Passage of Arms and Collective Unconscious
	/// hold for ("during AOE mitigations").
	/// </summary>
	public static bool AreaHitPending => MergedStatus.HasFlag(AutoStatus.DefenseArea)
		|| (InCombat && Service.Config.UseBmrTimeline && BMRNextRaidwideIn > 0f
			&& BMRNextRaidwideIn <= Service.Config.BMRRaidwideMitWindow);

	/// <summary>
	/// Who the next predicted damage event hits, as BossModReborn's party bitmask. 0 when nothing is
	/// predicted.
	/// </summary>
	/// <remarks>
	/// Reads the same entry as <see cref="BMRNextDamageIn"/> and <see cref="BMRNextDamageType"/> -
	/// BossMod sorts its prediction list by activation time and all three endpoints take the first
	/// element, so the three describe one event. The type-specific endpoints behind
	/// <see cref="BMRNextRaidwideIn"/> and <see cref="BMRNextTankbusterIn"/> search for the first
	/// entry of THEIR type instead, which can be a different one - this mask does not belong to
	/// those and must not be read alongside them.
	/// </remarks>
	public static ulong BMRNextDamagePlayers { get; set; }

	/// <summary>
	/// Whether the next predicted damage event includes the player.
	/// </summary>
	/// <remarks>
	/// Bit 0, and that position is fixed rather than a guess: BossModReborn's <c>PartyState</c>
	/// declares <c>PlayerSlot = 0</c>, so the player is always the first slot of its party list and
	/// does not move with party order. The remaining bits - 1..7 party, 8..23 alliance, 24..63 other
	/// allies - would need a mapping from BossMod's slot order to this tree's party list, which is
	/// another unverified contract across the IPC boundary and is deliberately not made.
	/// </remarks>
	public static bool BMRNextDamageHitsPlayer { get; set; }

	/// <summary>
	/// Whether BossModReborn's imminent tankbuster hits the player: its answer when the next predicted
	/// damage entry is that tankbuster - the entry's mask names who is hit, and bit 0 is always the
	/// player (BossModReborn <c>PartyState.PlayerSlot = 0</c>) - and null when the mask belongs to
	/// another event or nothing is predicted (A220).
	/// </summary>
	public static bool? BMRTankbusterHitsPlayer
		=> BMRTankbusterImminent && BMRNextDamageType == PredictedDamageType.Tankbuster
			&& BMRNextDamageIn > 0f && BMRNextDamageIn <= Service.Config.BMRTankbusterMitWindow
			? BMRNextDamageHitsPlayer
			: null;

	/// <summary>
	/// Whether BossModReborn's imminent tankbuster reaches the player: its mask when the prediction is the next hit
	/// (<see cref="BMRTankbusterHitsPlayer"/>), otherwise for a tank, and for anyone else only when no tank is in the
	/// party (A220). <see cref="BMRTankbusterImminent"/> alone says that a tankbuster comes, not whom it hits: the
	/// emergency HP potion read it so and a White Mage at 60 % drank for a tankbuster on the tank (A284).
	/// </summary>
	internal static bool BMRTankbusterReachesPlayer
		=> BMRTankbusterImminent && (BMRTankbusterHitsPlayer ?? (Role == JobRole.Tank || PartyTank == null));

	/// <summary>
	/// BMR predicts a tankbuster inside the user's single-target mitigation window, early enough for a press made now
	/// to count (<see cref="PressReachesBeforeSnapshot"/>).
	/// </summary>
	public static bool BMRTankbusterImminent =>
		Service.Config.UseBmrTimeline && BMRNextTankbusterIn <= Service.Config.BMRTankbusterMitWindow
		&& PressReachesBeforeSnapshot(BMRNextTankbusterIn);
}

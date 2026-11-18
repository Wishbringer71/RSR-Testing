using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Config;
using ECommons;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using Action = Lumina.Excel.Sheets.Action;
using Buddy = FFXIVClientStructs.FFXIV.Client.Game.UI.Buddy;
using CharacterManager = FFXIVClientStructs.FFXIV.Client.Game.Character.CharacterManager;
using CombatRole = RotationSolver.Engine.Data.CombatRole;
using ECommons.DalamudServices;
using ECommons.Logging;

namespace RotationSolver.Engine.State;

internal static partial class DataCenter
{
	/// <summary>
	/// Concept 03, "Gegner mit Stacheln" (A571): the enemies in reach left out of <see cref="AllHostileTargets"/> because
	/// their spikes damage their attackers - candidates only of an action that removes those spikes.
	/// </summary>
	public static List<IBattleChara> SpikedHostiles { get; set; } = [];

	/// <summary>
	/// Concept 03, "Gegner, die keine Fläche treffen darf" (A572): the enemies in reach that no attack may hit, not even an
	/// area one - spiked enemies, stop-marked ones while that setting is on, and the Zu's eggs while the Zu lives.
	/// </summary>
	public static List<IBattleChara> UntouchableHostiles { get; set; } = [];

	/// <summary>The untargetable members of <see cref="AreaCasters"/>.</summary>
	public static HashSet<ulong> AreaCastHelperIds { get; set; } = [];

	/// <summary>
	/// The party members an enemy is currently aiming at - either attacking them, or casting
	/// something that will land on them. Rebuilt once per frame beside the hostile list itself in
	/// <c>TargetUpdater.UpdateLists</c>.
	/// </summary>
	/// <remarks>
	/// Both sources are read because they answer different questions: the attack target is aggro,
	/// the cast target is what is about to arrive, and they part company on a boss that beats on the
	/// tank while casting at somebody else. Only party members are recorded, so an empty set means
	/// what it says - nothing is aimed at the party - rather than "no enemy has any target at all".
	/// Empty out of combat.
	/// </remarks>
	public static HashSet<ulong> TargetedPartyMembers { get; set; } = [];

	/// <summary>
	/// Every enemy a tank should provoke now, past the provoke delay or unable to wait, in the order of the hostile list;
	/// chosen per action as <see cref="InterruptCandidates"/> (A612).
	/// </summary>
	internal static List<IBattleChara> ProvokeCandidates { get; } = [];

	private static readonly Dictionary<uint, HealLanding> _lastHealLanding = [];

	private static long _packTTKCacheTick = long.MinValue;

	// One cache per kind of heal - off-GCD and GCD look ahead by different leads (A213).
	private static readonly long[] _forecastStatsCacheTick = [long.MinValue, long.MinValue];

	private static readonly float[] _forecastAvgCache = new float[_forecastStatsCacheTick.Length];

	private static readonly float[] _forecastStdDevCache = new float[_forecastStatsCacheTick.Length];

	private static readonly float[] _forecastLowestAvgCache = new float[_forecastStatsCacheTick.Length];

	private static readonly float[] _forecastLowestStdDevCache = new float[_forecastStatsCacheTick.Length];

	private static readonly float[] _forecastMinCache = new float[_forecastStatsCacheTick.Length];

	private static readonly ConcurrentDictionary<string, byte> _markerLocksTakenOver = new();

	/// <summary>
	/// Whether the marker entry <paramref name="key"/> is locked as "no tankbuster" in the running game version (concept
	/// 13, "Stufe 4", 4a): a lock from an earlier version, or from before stage 4, is a hypothesis and does not hold - the
	/// marker counts as a tankbuster until it goes without a hit again. Nothing holds while the version cannot be read.
	/// </summary>
	internal static bool MarkerLockHolds(string key, bool trace = true)
	{
		if (!OtherConfiguration.TankbusterMarkerWithoutHit.Contains(key))
		{
			return false;
		}

		if (OtherConfiguration.GameVersion is { } game && OtherConfiguration.TankbusterMarkerWithoutHitGame.TryGetValue(key, out var seen)
			&& seen == game)
		{
			return true;
		}

		if (trace && _markerLocksTakenOver.TryAdd(key, 0))
		{
			DefenseTrace.Line($"tankbuster marker {key}: no hit seen in an earlier game version only - counted as a tankbuster"
				+ " until it goes without a hit in this one");
		}

		return false;
	}

	/// <summary>
	/// Every area action for which <see cref="AreaCastIsWorthMitigating"/> has withheld the party
	/// mitigation, and when it last did so. This is the probe for that decision, and the domain
	/// requires one: whether a rule that fires in combat fires at all cannot be settled by reading it.
	/// </summary>
	/// <remarks>
	/// It records actions and not calls on purpose. The predicate is asked once per casting enemy per
	/// frame, so a counter would report the frame rate rather than the number of hits let through -
	/// a surrogate for the effect instead of the effect. Writing the id is idempotent, so the same
	/// cast seen sixty times a second leaves one entry, and the count answers the question that
	/// matters: for how many of the rated actions has this rule actually saved a cooldown.
	///
	/// Diagnostic only, so it is deliberately not persisted: after a restart the honest answer is
	/// "not yet seen this session". The size is bounded by the number of rated actions.
	/// </remarks>
	public static readonly ConcurrentDictionary<uint, DateTime> AreaMitigationSkipped = new();

	/// <summary>
	/// Every area action the pre-heal rule has raised the healing flag for, and when it last did.
	/// </summary>
	/// <remarks>
	/// The counterpart probe to <see cref="AreaMitigationSkipped"/>, and it exists for a stated
	/// reason: the owner tests changes by switching the new option ON, so a rule that cannot be told
	/// apart from its own absence is not testable. Healing that arrives before a raidwide looks in
	/// the fight exactly like healing that arrives for any other reason.
	///
	/// Actions, not calls - the question is asked every frame, so a counter would report the frame
	/// rate. Diagnostic only and not persisted.
	/// </remarks>
	public static readonly ConcurrentDictionary<uint, DateTime> HealedAheadOfAreaCast = new();

	/// <summary>
	/// Every interruptible area action the defence was raised for because its measured share was
	/// large, and when it last happened. Same purpose as the two above.
	/// </summary>
	public static readonly ConcurrentDictionary<uint, DateTime> MitigatedInterruptibleCast = new();

	// The hold's own prediction, and whether the fight bore it out. Holding back says: "the event
	// this prediction points at is not the small cast now running - something larger follows". That
	// is a claim about the next few seconds, and the next few seconds answer it.
	//
	// This is the difference between a counter and a probe that decides. A counter would have to be
	// read by somebody and reported back, which costs a fight, a reading and a round per figure -
	// the owner's standing objection. This one scores itself: every hold is checked against what
	// actually landed, and the rule stops holding once its own record says it is wrong.
	private static DateTime _holdExpectsHitUntil = DateTime.MinValue;

	/// <summary>
	/// Every small area action a predicted mitigation was held back for, and when it last happened.
	/// Same purpose as the two above: a mitigation that waits looks exactly like one that never
	/// triggered, so without this the rule cannot be tested by switching it on.
	/// </summary>
	public static readonly ConcurrentDictionary<uint, DateTime> ProactiveMitigationHeld = new();

	private static DateTime _announcedAreaShareTime = DateTime.MinValue;

	/// <summary>
	/// Why the last enemy cast that damaged the player was or was not rated for the learned damage
	/// table, with its time and action id. Written by the effect handler; shown in the AoE list.
	/// </summary>
	/// <remarks>
	/// Casts only. Instant hits - auto-attacks above all - are never rated, and while they were
	/// reported here the next swing replaced a raidwide's reason within a second, so the line could
	/// only ever show why an auto-attack was not measured.
	/// </remarks>
	public static string AreaMeasurementLastOutcome { get; private set; } = "no enemy cast has hit the player yet";

	private static readonly Dictionary<string, int> _areaMeasurementTally = [];

	/// <summary>The first error the enemy effect handler raised since loading, for the windows.</summary>
	public static string EffectHandlerFirstError { get; internal set; } = string.Empty;

	/// <summary>
	/// Every strategic hold of a defense that was asked (CustomRotation_DefenseHold), by rule: whether
	/// it last held or yielded to a member in danger, why, and when. One entry per rule, so a rule
	/// asked after another in the same frame does not hide it. Shown in the diagnostics window,
	/// because a defense that waits cannot otherwise be told apart from one that was never asked.
	/// </summary>
	internal static readonly ConcurrentDictionary<string, DefenseHoldDecision> DefenseHolds = new();

	private static bool HitDropsBelow(float share, float threshold, bool unprotectedOnly, bool needsRoom, out string who)
	{
		who = string.Empty;
		if (share <= 0f)
		{
			return false;
		}

		var party = PartyMembers;
		for (var i = 0; i < party.Count; i++)
		{
			var member = party[i];
			if (member == null || member.IsDead || member.MaxHp == 0
				|| (unprotectedOnly && !member.IsUnprotectedForHealing())
				|| (needsRoom && member.CurrentHp >= member.MaxHp))
			{
				continue;
			}

			// The unprotected reading is the critical class's own footing (IsInCriticalClass): the
			// forecast effective health, at or below the threshold. The other keeps its original form
			// for its original readers.
			if (unprotectedOnly
				? member.GetForecastEffectiveHp() / (float)member.MaxHp - share <= threshold
				: member.GetEffectiveHp() / (float)member.MaxHp - share < threshold)
			{
				who = member.Name.TextValue;
				return true;
			}
		}

		return false;
	}

	// Kept separate from the verdict below, and called before it, for two reasons. The figure
	// describes the incoming cast, not this rule's opinion of it - and a second reader, the healing
	// flag, needs it whether or not the owner wants small casts mitigated. Recorded inside the
	// verdict it sat behind SkipMitigationForSmallAreaCasts, which would have left that reader blind
	// to every cast whenever the switch was off.
	private static void RecordAnnouncedAreaShare(uint actionId, IBattleChara caster)
	{
		// A rating of 0 is a landing measured without damage (A366), and announces exactly that.
		if (AreaShareNow(actionId, caster, out var share))
		{
			_announcedAreaShare = share;
			_announcedAreaShareConfirmed = OtherConfiguration.AreaShareConfirmed(actionId);
			_announcedAreaShareTime = DateTime.Now;
			_lastAnnouncedAreaAction = actionId;
		}
	}

	/// <summary>
	/// Whether the last landing of each listed area cast reached the player, for this session. Written
	/// by the effect handler, read by <see cref="IsAnyHostileCastingAreaReachingMe"/> under
	/// "Skip area defence for casts that missed you" (A205).
	/// </summary>
	internal static readonly ConcurrentDictionary<uint, bool> AreaCastReachedPlayer = new();

	/// <summary>
	/// A731 (concept 13, "Wen die Form erreicht"): the angle of a cone and the inner radius of a ring as the action's omen
	/// names them - "gl_fan060_1bf" a cone of 60 degrees, "gl_sircle_2010bf" a ring with the figures 20 and 10, as
	/// BossModReborn's analysis reads them. The ring's figures hold as the inner radius itself or as its share of the sheet's
	/// range, unevenly even for one omen; the smaller is taken, so more count as hit. 0 for what the omen does not name.
	/// </summary>
	internal static (float Degrees, float Inner) OmenShape(Action act)
	{
		return _omenShapes.GetOrAdd(act.RowId, _ =>
		{
			var path = act.Omen.ValueNullable?.Path.ExtractText() ?? string.Empty;
			var degrees = DigitsAfter(path, "fan", out var angle) ? angle : 0;
			var inner = 0f;
			foreach (var key in RingKeys)
			{
				if (DigitsAfter(path, key, out var figures) && figures > 0)
				{
					var digits = figures.ToString(System.Globalization.CultureInfo.InvariantCulture);
					var half = digits.Length / 2;
					if (half > 0 && int.TryParse(digits.AsSpan(0, digits.Length - half), out var outer) && int.TryParse(digits.AsSpan(digits.Length - half), out var innerFigure))
					{
						inner = outer > 0 ? Math.Min(innerFigure, act.EffectRange * innerFigure / (float)outer) : innerFigure;
					}

					break;
				}
			}

			return (degrees, inner);
		});
	}

	// A731: the omen names in which a ring's figures follow, as BossModReborn's analysis reads them.
	private static readonly string[] RingKeys = ["sircle_", "sicle_", "circle"];

	// The number that follows <key> in <path>, digits only.
	private static bool DigitsAfter(string path, string key, out int number)
	{
		number = 0;
		var start = path.IndexOf(key, StringComparison.Ordinal);
		if (start < 0)
		{
			return false;
		}

		start += key.Length;
		var end = start;
		while (end < path.Length && char.IsAsciiDigit(path[end]))
		{
			end++;
		}

		return end > start && int.TryParse(path.AsSpan(start, end - start), System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out number);
	}

	private static float HorizontalDistance(Vector3 a, Vector3 b)
	{
		var dx = a.X - b.X;
		var dz = a.Z - b.Z;
		return MathF.Sqrt((dx * dx) + (dz * dz));
	}

	/// <summary>
	/// Every source that can open the player's defence, as it stands now, for the defence trace.
	/// Reads the same predicates the flags are built from; decides nothing.
	/// </summary>
	internal static string DescribeDefenseSources()
	{
		var parts = new List<string> { $"flags {MergedStatus & (AutoStatus.DefenseArea | AutoStatus.DefenseSingle)}" };
		var player = Player.Object;
		try
		{
			foreach (var vfx in VfxDataQueue)
			{
				if (string.IsNullOrEmpty(vfx.Path))
				{
					continue;
				}

				var kind = IsTankbusterMarkerPath(vfx.Path)
					? IsFalsifiedTankbusterMarker(vfx.Path) ? "tankbuster marker (falsified)" : "tankbuster marker"
					: StartsWithAny(vfx.Path, MultiHitSharedPaths) || StartsWithAny(vfx.Path, SharedDamagePaths) ? "stack marker"
					: StartsWithAny(vfx.Path, SpreadDamagePaths) ? "spread marker"
					: null;
				if (kind == null)
				{
					continue;
				}

				var marked = Svc.Objects.SearchById(vfx.ObjectId) as IBattleChara;
				var onPlayer = player != null && vfx.ObjectId == player.GameObjectId;
				parts.Add($"{kind} {vfx.Path} on {(onPlayer ? "you" : marked?.Name.TextValue ?? "?")}"
					+ (onPlayer || marked == null || player == null ? string.Empty : $" {HorizontalDistance(player.Position, marked.Position):F1} y from you"));
			}

			var sheet = Service.GetSheet<Action>();
			var isTank = Role == JobRole.Tank;
			foreach (var h in AreaCasters)
			{
				if (h == null || !h.IsValid() || !h.IsCasting)
				{
					continue;
				}

				var id = h.CastActionId;
				var listedArea = OtherConfiguration.HostileCastingArea.Contains(id);
				var listedTank = OtherConfiguration.HostileCastingTank.Contains(id);

				// A tank also reads an unlisted cast an enemy aims at the one it is attacking as a
				// tankbuster; that source opened his single-target defence without showing here.
				if (!listedArea && !listedTank)
				{
					if (isTank && IsHostileCastingTank(h))
					{
						var aimed = Svc.Objects.SearchById(h.CastTargetObjectId);
						parts.Add($"cast {sheet?.GetRowOrDefault(id)?.Name.ExtractText() ?? "?"} #{id} by {h.Name.TextValue} at "
							+ $"{(player != null && h.CastTargetObjectId == player.GameObjectId ? "you" : aimed?.Name.TextValue ?? "?")}"
							+ $" (unlisted, cast at its target) {h.TotalCastTime - h.CurrentCastTime:F1} s left");
					}

					continue;
				}

				var act = sheet?.GetRowOrDefault(id);
				var target = Svc.Objects.SearchById(h.CastTargetObjectId);
				var text = $"cast {act?.Name.ExtractText() ?? "?"} #{id} by {h.Name.TextValue}"
					+ $"{(AreaCastHelperIds.Contains(h.GameObjectId) ? " (untargetable)" : string.Empty)} at {target?.Name.TextValue ?? "?"}"
					+ $" ({(listedArea ? "area list" : string.Empty)}{(listedArea && listedTank ? ", " : string.Empty)}{(listedTank ? "tankbuster list" : string.Empty)})"
					+ $" {h.TotalCastTime - h.CurrentCastTime:F1} s left";
				if (act is { } a)
				{
					text += $", type {a.CastType} range {a.Range} effect {a.EffectRange} width {a.XAxisModifier}"
						+ $", caster {h.DistanceToPlayer():F1} y from you";
					// Only side-effect-free reads here: the worth check and the large-cast check record
					// what they decide, and the trace must not make decisions the flags did not make.
					if (listedArea)
					{
						text += $", reaches you {AreaCastCanReachPlayer(h, a)}, reaches the party {AreaCastReachesParty(h, a)}, interruptible {h.IsCastInterruptible}"
							+ $", rated {(OtherConfiguration.HostileCastingAreaPotential.TryGetValue(id, out var share) ? share.ToString("P0") + " unmitigated" : "no")}"
							+ $", last landing reached you {(AreaCastReachedPlayer.TryGetValue(id, out var reachedLast) ? reachedLast.ToString() : "not seen")}";
					}
				}

				parts.Add(text);
			}
		}
		catch (Exception ex)
		{
			parts.Add($"scan failed: {ex.Message}");
		}

		parts.Add($"area hit reaches you {AreaHitReachesPlayer}");
		parts.Add($"single hit reaches you {SingleHitReachesPlayer}");

		if (TankPullOnPlayer)
		{
			parts.Add($"pull on you: at least {Service.Config.AutoDefenseNumber} enemies within reach attacking you");
		}

		if (UnhealedUnderAttack)
		{
			var due = TankbusterForecast.NextOnPlayerIn;
			parts.Add("no healer alive, an enemy attacks you: mitigation that is back before the next tankbuster"
				+ (float.IsPositiveInfinity(due) ? " (none due)" : $" (due in {due:F1} s)"));
		}

		if (BMRNextRaidwideIn is > 0f and < float.MaxValue)
		{
			parts.Add($"BMR raidwide in {BMRNextRaidwideIn:F1} s (window {Service.Config.BMRRaidwideMitWindow:F1},"
				+ $" {BMRActiveModuleName ?? "no module"} by {BMRNextRaidwideSource}, hits you {BMRRaidwideHitsPlayer?.ToString() ?? "unknown"})");
		}

		if (BMRNextTankbusterIn is > 0f and < float.MaxValue)
		{
			parts.Add($"BMR tankbuster in {BMRNextTankbusterIn:F1} s (window {Service.Config.BMRTankbusterMitWindow:F1}), party tank {PartyTank?.Name.TextValue ?? "none"}"
				+ $", hits you {BMRTankbusterHitsPlayer?.ToString() ?? "unknown"}");
		}

		if (IsTankbusterVfxOnPlayer())
		{
			parts.Add("tankbuster marker on you");
		}

		var party = PartyMembers;
		for (var i = 0; i < party.Count; i++)
		{
			if (party[i] is { } member && TankbusterHelp.AimedHitCallsForDefence(member, out var aimed))
			{
				parts.Add(aimed);
			}
		}

		return string.Join(" | ", parts);
	}

	// Whether a marker sits on the player or a member of his own party, Duty Support companions
	// included whatever the NPC party setting says: they share the stack as a player would.
	private static bool IsOwnPartyMarker(ulong objectId)
	{
		var player = Player.Object;
		if (player != null && objectId == player.GameObjectId)
		{
			return true;
		}

		return Svc.Objects.SearchById(objectId) is IBattleChara marked
			&& (marked.IsParty() || marked.IsNpcPartyMember());
	}

	private static bool IsAnyHostileCastingKnockback()
	{
		// Over the helpers too (A303): a listed knockback resolved by an untargetable helper is the knockback
		// "Automatically use Anti-Knockback role actions" promises to answer from the list. Only one that reaches the
		// player (A701, concept 13): the knockback is an effect on whom the action hits; Ground and Pound in a line he
		// did not stand in took Surecast for 120 s.
		KnockbackReachingPlayer = null;
		var targets = AreaCasters;
		for (var i = 0; i < targets.Count; i++)
		{
			var h = targets[i];
			if (IsHostileCastingKnockback(h) && TryGetCastAction(h, out var act) && AreaCastCanReachPlayer(h, act))
			{
				KnockbackReachingPlayer = (h, act);
				return true;
			}
		}

		return false;
	}

	private static List<BmrRaidwideCast> _bmrRaidwideCasts = [];

	private static DateTime _bmrRaidwideLandsAt = DateTime.MinValue;

	private static DateTime _bmrRaidwideReadAt = DateTime.MinValue;

	private static TimeSpan _bmrRaidwideReadFrame = TimeSpan.Zero;

	private static DateTime _bmrRaidwideHeldTracedFor = DateTime.MinValue;

	#region BossModReborn Timeline Integration
	private static readonly FrameCached<bool> _bmrEnabled = new(() => IsPluginLoaded("BossModReborn", "BossModReborn"), 1000);
	private static readonly FrameCached<bool> _autoPvpSeriesGrindEnabled = new(() => IsPluginLoaded("Auto PVP Series Grind", "AutoPvpSeriesGrind"), 1000);

	public static bool BMREnabled => _bmrEnabled.Value;

	public static bool AutoPvpSeriesGrindEnabled => _autoPvpSeriesGrindEnabled.Value;

	internal static bool IsPluginLoaded(string name, string internalName)
	{
		foreach (var x in Svc.PluginInterface.InstalledPlugins)
		{
			if ((x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) || x.InternalName.Equals(internalName, StringComparison.OrdinalIgnoreCase)) && x.IsLoaded)
			{
				return true;
			}
		}

		return false;
	}
	public static bool PvPAutomationBlocked => IsPvP && AutoPvpSeriesGrindEnabled;

	public static bool BMRHasActiveModule { get; set; }
	public static string? BMRActiveModuleName { get; set; }
	public static float BMRNextRaidwideIn { get; set; } = float.MaxValue;

	/// <summary>
	/// Which BossModReborn endpoint gave <see cref="BMRNextRaidwideIn"/>: "timeline", "hints" or "generic".
	/// For the trace: only "hints" carries a list of whom the raidwide hits.
	/// </summary>
	public static string BMRNextRaidwideSource { get; set; } = string.Empty;
	public static float BMRNextTankbusterIn { get; set; } = float.MaxValue;
	public static float BMRNextKnockbackIn { get; set; } = float.MaxValue;
	public static float BMRNextDowntimeIn { get; set; } = float.MaxValue;
	public static float BMRNextDowntimeEndIn { get; set; } = float.MaxValue;
	public static float BMRNextVulnerableIn { get; set; } = float.MaxValue;
	public static float BMRNextVulnerableEndIn { get; set; } = float.MaxValue;
	public static float BMRNextDamageIn { get; set; } = float.MaxValue;
	public static PredictedDamageType BMRNextDamageType { get; set; } = PredictedDamageType.None;
	public static float BMRSpecialModeIn { get; set; } = float.MaxValue;
	public static SpecialMode BMRSpecialModeType { get; set; } = SpecialMode.Normal;

	/// <summary>
	/// True if BossMod's boss module AI hints are requesting the current cast be cancelled.
	/// </summary>
	public static bool BMRForceCancelCast { get; set; }

	/// <summary>
	/// True if BossMod's AI controller is requesting the current cast be cancelled.
	/// </summary>
	public static bool BMRForceCancelCastAI { get; set; }

	/// <summary>
	/// The longest cast BossMod expects to finish before its AI or a mechanic moves the player (<c>Hints.MaxCastTime</c>);
	/// float.MaxValue without BossMod. A longer cast is interrupted (A628).
	/// </summary>
	public static float BMRMaxCastTime { get; set; } = float.MaxValue;

	/// <summary>
	/// True while BossMod wants a movement of the player or holds casts (<c>Movement.IsMoving</c>:
	/// <c>hints.ForcedMovement != null || hints.HoldCasts</c>, BossModReborn 2087153c2) - also while its AI does not walk.
	/// Whether the AI steers the player is <see cref="BMRIsNavigating"/> (A725).
	/// </summary>
	public static bool BMRIsMoving { get; set; }

	/// <summary>
	/// Seconds until the earliest of BossMod's forbidden zones turns active, wherever it lies
	/// (<c>Hints.ForbiddenZonesNextActivation</c>); float.MaxValue without one.
	/// </summary>
	public static float BMRForbiddenZonesNextActivation { get; set; } = float.MaxValue;
	public static float BMRDebugTimelineRaidwide { get; set; } = float.MaxValue;
	public static float BMRDebugTimelineTankbuster { get; set; } = float.MaxValue;
	public static float BMRDebugHintsRaidwide { get; set; } = float.MaxValue;
	public static float BMRDebugHintsTankbuster { get; set; } = float.MaxValue;
	public static float BMRDebugGenericDamageIn { get; set; } = float.MaxValue;
	public static int BMRDebugGenericDamageType { get; set; }
	public static bool BMRDebugTimelineRwFunc { get; set; }
	public static bool BMRDebugTimelineTbFunc { get; set; }
	public static bool BMRDebugHintsRwFunc { get; set; }
	public static bool BMRDebugHintsTbFunc { get; set; }

	/// <summary>
	/// Delegate wired up by BossModUpdater to the <c>Hints.IsPositionSafe</c> IPC endpoint.
	/// When null, BossModReborn is not available and all positions are considered safe.
	/// </summary>
	public static Func<Vector3, bool>? BMRIsPositionSafe { get; set; }

	/// <summary>
	/// Delegate wired up by BossModUpdater to the <c>Hints.IsDashSafe</c> IPC endpoint.
	/// When null, BossModReborn is not available and all dashes are considered safe.
	/// </summary>
	public static Func<Vector3, Vector3, bool>? BMRIsDashSafe { get; set; }

	/// <summary>
	/// Delegate wired up by BossModUpdater for a dash whose landing point the game fixes - a fixed distance along the
	/// facing, or a backstep from the target - with the landing point RSR computes. It asks BossModReborn's
	/// <c>Hints.IsDashSafe</c>: its own <c>Hints.IsFixedDashSafe</c> takes range and direction, not two points (A313).
	/// When null, BossModReborn is not available and all fixed dashes are considered safe.
	/// </summary>
	public static Func<Vector3, Vector3, bool>? BMRIsFixedDashSafe { get; set; }

	/// <summary>
	/// The most recently polled set of upcoming planned actions from BossMod's Cooldown Planner,
	/// wired up by BMRPlanUpdater. Empty when no plan is active or BossModReborn is unavailable.
	/// </summary>
	public static List<BMRPlannedAction> BMRPlannedActions { get; set; } = [];

	/// <summary>
	/// Returns the currently active planned action (if any) matching the given adjusted action id,
	/// i.e. one whose activation window has started (ActivationIn &lt;= 0) and not yet ended
	/// (WindowEndIn &gt; 0). Only entries resolved to a concrete game action (ActionType == 1) are considered.
	/// </summary>
	public static BMRPlannedAction? GetActivePlannedAction(uint actionId)
	{
		var actions = BMRPlannedActions;
		for (var i = 0; i < actions.Count; i++)
		{
			var action = actions[i];
			if (action.ActionId == actionId && action.ActivationIn <= 0f && action.WindowEndIn > 0f)
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"GetActivePlannedAction: Found active planned action {action.ActionId} with ActivationIn {action.ActivationIn} and WindowEndIn {action.WindowEndIn}");
				}

				return action;
			}
		}

		return null;
	}

	/// <summary>
	/// Returns true if the destination is safe to move to, or if BossModReborn IPC is unavailable.
	/// </summary>
	public static bool IsMovementDestinationSafe(Vector3 destination)
	{
		if (BMRIsPositionSafe == null)
		{
			return true;
		}

		try
		{ return BMRIsPositionSafe(destination); }
		catch { return true; }
	}

	/// <summary>
	/// Returns true if the dash from <paramref name="from"/> to <paramref name="to"/> is safe,
	/// or if BossModReborn IPC is unavailable.
	/// </summary>
	public static bool IsDashSafe(Vector3 from, Vector3 to)
	{
		if (BMRIsDashSafe == null)
		{
			return true;
		}

		try
		{ return BMRIsDashSafe(from, to); }
		catch { return true; }
	}

	/// <summary>
	/// Returns true if a fixed-distance dash (game-determined destination) from <paramref name="from"/> to
	/// <paramref name="to"/> is safe, or if BossModReborn IPC is unavailable.
	/// For FixedDistanceMoveForward, FixedDistanceMoveBackward
	/// </summary>
	public static bool IsFixedDashSafe(Vector3 from, Vector3 to)
	{
		if (BMRIsFixedDashSafe == null)
		{
			return true;
		}

		try
		{ return BMRIsFixedDashSafe(from, to); }
		catch { return true; }
	}
	public static void ResetBmrData()
	{
		BMRHasActiveModule = false;
		BMRActiveModuleName = null;
		BMRNextRaidwideIn = float.MaxValue;
		BMRNextRaidwideSource = string.Empty;
		BMRRaidwideHitsPlayer = null;
		_bmrRaidwideCasts = [];
		_bmrRaidwideLandsAt = DateTime.MinValue;
		BMRNextTankbusterIn = float.MaxValue;
		BMRNextKnockbackIn = float.MaxValue;
		BMRNextDowntimeIn = float.MaxValue;
		BMRNextDowntimeEndIn = float.MaxValue;
		BMRNextVulnerableIn = float.MaxValue;
		BMRNextVulnerableEndIn = float.MaxValue;
		BMRNextDamageIn = float.MaxValue;
		BMRNextDamageType = PredictedDamageType.None;
		BMRNextDamagePlayers = 0;
		BMRNextDamageHitsPlayer = false;
		BMRSpecialModeIn = float.MaxValue;
		BMRSpecialModeType = SpecialMode.Normal;
		BMRForceCancelCast = false;
		BMRForceCancelCastAI = false;
		BMRMaxCastTime = float.MaxValue;
		BMRMoveRequested = false;
		BMRIsMoving = false;
		BMRIsNavigating = false;
		BMRNaviTargetPos = null;
		BMRPlayerSpeed = 0f;
		BMRForbiddenZonesNextActivation = float.MaxValue;
		BMRDebugTimelineRaidwide = float.MaxValue;
		BMRDebugTimelineTankbuster = float.MaxValue;
		BMRDebugHintsRaidwide = float.MaxValue;
		BMRDebugHintsTankbuster = float.MaxValue;
		BMRDebugGenericDamageIn = float.MaxValue;
		BMRDebugGenericDamageType = 0;
		BMRDebugTimelineRwFunc = false;
		BMRDebugTimelineTbFunc = false;
		BMRDebugHintsRwFunc = false;
		BMRDebugHintsTbFunc = false;
		BMRIsPositionSafe = null;
		BMRIsDashSafe = null;
		BMRIsFixedDashSafe = null;
	}

	/// <summary>
	/// Clears any cached Cooldown Planner data (called when the feature is disabled or BMR becomes unavailable).
	/// </summary>
	public static void ResetBmrPlanData()
	{
		BMRPlannedActions = [];
	}
	#endregion
}

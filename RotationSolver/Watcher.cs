using Dalamud.Hooking;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Hooks;
using ECommons.Hooks.ActionEffectTypes;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using RotationSolver.ActionTimeline;
using RotationSolver.Basic.Configuration;
using System.Text.RegularExpressions;

namespace RotationSolver;

public static partial class Watcher
{
	private const uint CancelAbilityCategory = 15;
	private const uint EnemyDotTickCategory = 0x605;

	// A heal-over-time tick on a target, amount in the second parameter: the aggregated one with 0 in the first, a ground
	// effect's with its status there - beside the damage-over-time tick above (DeathRecap reads both so; A432).
	private const uint HotTickCategory = 0x604;

	private delegate void ActorControlDelegate(uint entityId, uint category, uint p1, uint p2, uint p3, uint p4, uint p5, uint p6, uint p7, uint p8, ulong targetId, byte replaying);
	private static Hook<ActorControlDelegate>? _actorControlHook;

	public static void Enable()
	{
		var config = Service.Config;
		DefenseTrace.Start($"version {typeof(Watcher).Assembly.GetName().Version} | commit {SourceCommit()} | area defence {config.UseAoeDefense}"
			+ $" | single defence {config.UseStDefense} | skip casts that missed you {config.SkipAreaCastsThatMissedMe}"
			+ $" | big interruptible casts {config.MitigateBigAreaCastsEvenIfInterruptible} | BMR timeline {config.UseBmrTimeline}"
			+ $" | AoE list {OtherConfiguration.HostileCastingArea.Count} | tankbuster list {OtherConfiguration.HostileCastingTank.Count}"
			+ $" | tankbuster markers without a hit {OtherConfiguration.TankbusterMarkerWithoutHit.Count} ({MarkerLocksRunning()} locked in this game version)"
			+ $" | stop enemy casts {config.SleepToStopAreaCasts}"
			+ $" | sleep knowledge {OtherConfiguration.SleepOutcomes.Count} action(s), {OtherConfiguration.SleepResistantKinds.Count} resistant kind(s),"
			+ $" {OtherConfiguration.SleepDiminishing.Count} kind limit(s) ({OtherConfiguration.SleepStoreState})"
			+ $" | sleep locks: {SleepInterruptWatch.LockSummary()}"
			+ $" | hit series {OtherConfiguration.HitSeriesOnPlayer.Count} action(s) ({OtherConfiguration.HitSeriesStoreState})"
			+ $" | heal over time against damage over time {config.HotAgainstDot},"
			+ $" damage over time learned in {OtherConfiguration.DotOnParty.Count} area(s) ({OtherConfiguration.DotStoreState})"
			+ $" | casts laying a status that damages every action {ActionPenaltyWatch.LearnedCount}, {ActionPenaltyWatch.RefutedCount} refuted"
			+ $" ({OtherConfiguration.ActionPenaltyStoreState})"
			+ $" | area series with a learned span {OtherConfiguration.AreaSeriesSpan.Count} ({OtherConfiguration.AreaSeriesSpanState})"
			+ $" | area table revisions: {OtherConfiguration.AreaRevisionState}"
			+ $" | tankbuster table revisions: {OtherConfiguration.TankbusterRevisionState}");

		ActionEffect.ActionEffectEvent += ActionFromEnemy;
		ActionEffect.ActionEffectEvent += ActionFromSelf;

		try
		{
			_actorControlHook = Svc.Hook.HookFromSignature<ActorControlDelegate>("E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64", ActorControlDetour);
			_actorControlHook.Enable();
		}
		catch (Exception ex)
		{
			PluginLog.Error($"[Watcher] Failed to hook ActorControl: {ex}");
		}
	}

	public static void Disable()
	{
		ActionEffect.ActionEffectEvent -= ActionFromEnemy;
		ActionEffect.ActionEffectEvent -= ActionFromSelf;
		DefenseTrace.Stop("effect handler unhooked");

		_actorControlHook?.Dispose();
		_actorControlHook = null;
	}

	private static void ActorControlDetour(uint entityId, uint category, uint p1, uint p2, uint p3, uint p4, uint p5, uint p6, uint p7, uint p8, ulong targetId, byte replaying)
	{
		_actorControlHook!.Original(entityId, category, p1, p2, p3, p4, p5, p6, p7, p8, targetId, replaying);

		try
		{
			if (category == EnemyDotTickCategory)
			{
				DotTickTracker.RecordTick(entityId, Environment.TickCount64);
				DotOnParty.RecordDotTick(entityId, p1, p2);

				// Concept 03 (A723): the tick of the one damage over time a member carries is its layer's damage on the party.
				if (p1 == 0 && p2 != 0 && DotOnParty.TryOnlySource(entityId, out var layer))
				{
					EnemyDanger.RecordHit(layer, p2);
				}

				// K10 of the Crucible (A722): damage over time on the player belongs to what a fight costs him.
				if (DataCenter.IsInCrucible && entityId == Player.Object?.EntityId)
				{
					CrucibleBoard.RecordTakenOverTime(p2);
				}
			}
			else if (category == HotTickCategory)
			{
				DotOnParty.RecordHotTick(entityId, p1, p2);
				HealFromOthers.RecordTick(entityId, p1);
			}
			else if (category == CancelAbilityCategory && entityId == Player.Object?.EntityId)
			{
				ActionTimelineManager.Current?.CancelInterruptedCast(DateTime.Now);
			}
			else if (category == CancelAbilityCategory && DataCenter.IsInCrucible)
			{
				// K4 probe (A557): an enemy's cast stopped on a board.
				CrucibleBoard.RecordCastStopped(entityId, p1, p2, p3, p4);
			}
		}
		catch (Exception ex)
		{
			PluginLog.Error($"Error in ActorControlDetour: {ex.Message}");
		}
	}

	public static string ShowStrSelf { get; private set; } = string.Empty;

	private static readonly Dictionary<uint, AreaSeries> _areaSeries = [];

	// The targets of one effect set the tankbuster table has taken, so one named twice counts once (A454); and the
	// statuses passed for a member who is not a tank, whom the table does not store.
	private static readonly HashSet<ulong> _tankbusterTargets = [];
	private static readonly List<uint> _noStatuses = [];

	private static readonly HashSet<ulong> _reachedTargets = [];

	// Listed area casts whose last landing reached nobody in the party, by name. Such a cast is the visible
	// wind-up; the hit, where there is one, is a same-named action right after it, often from an
	// untargetable helper (trace of 04-10 October, A272: Head-splitting Roar lands on its caster, and its
	// namesake 43579 hits the party for up to 22 % half a second later). An entry stands until the listed
	// cast lands again or the fight ends - no clock, so no window to guess.
	private static readonly Dictionary<string, uint> _emptyLandingByName = [];

	// A hit on the player from a source this handler does not read as an enemy: none at all, because the
	// object table does not hold it (ECommons resolves the source there, Svc.Objects.SearchById, and a client
	// with many figures around need not keep every helper), or a figure that is neither an enemy nor a
	// combatant. Every path below drops such a set - no "hit you", no damage intake, no measurement. Whether
	// that happens is open: twice raidwides were predicted and no hit stood in the trace, against Necrophobia
	// (10.10.2026) and Dark Flare in an Occult Crescent critical engagement (17.10.2026), both resolved by
	// helpers. This line answers it (A299).
	private static void TraceHitFromUnreadSource(ActionEffectSet set, IBattleChara player)
	{
		if (set.Action is not { } action || action.GetActionCate() == ActionCate.Autoattack)
		{
			return;
		}

		var source = set.Source;
		if (IsHostileSource(set)
			|| (source is IBattleChara known && (known.GameObjectId == player.GameObjectId || known.IsParty())))
		{
			return;
		}

		foreach (var effect in set.TargetEffects)
		{
			if (effect.TargetID == player.GameObjectId && TryGetDamageEntry(effect, out _))
			{
				DefenseTrace.Line("hit you from "
					+ (source == null ? "a source the object table does not hold"
						: $"{source.Name.TextValue}, not read as an enemy ({source.ObjectKind}"
							+ (source is IBattleChara figure ? $", {figure.GetBattleNPCSubKind()}{(figure.IsTargetable ? string.Empty : ", untargetable")}" : string.Empty)
							+ ")")
					+ $": {action.Name.ExtractText()} #{action.RowId} for {DamageShareOn(set, player.GameObjectId, Math.Max(1u, player.MaxHp)):P0}"
					+ $" of max HP, {set.TargetEffects.Length} targets");
				return;
			}
		}
	}

	private static void ActionFromEnemy(ActionEffectSet set)
	{
		try
		{
			DataCenter.EffectSetsReceived++;

			var playerObject = Player.Object;
			if (playerObject == null)
			{
				return;
			}

			// Every set, from any source: the stuns that matter come from the party, so this sits before
			// every filter below.
			ObserveStuns(set);
			ObserveSleep(set, playerObject.GameObjectId);
			// A landing that lays a status damaging every action teaches its action, from any source (A439).
			ActionPenaltyWatch.ObserveLanding(set);

			// Concept 21, K12 (A675): the roulette needle's Death, for where its beat falls before the needle turns.
			if (set.Action is { } observed)
			{
				KillOrder.ObserveAction(observed.RowId);
			}

			TraceHitFromUnreadSource(set, playerObject);

			// Healing by anyone but the player and his objects, for the rule that breaks off a single heal it made needless.
			RecordHealsByOthers(set, playerObject);

			// K1 of the Crucible (A549): the damage of the player's and his familiars' hits, for the weakness bonus.
			RecordCrucibleHits(set, playerObject);

			// A party member's Shirk on the player: his request to take the enemy he holds (TankSwapWatch).
			if (set.Source is IBattleChara shirker && shirker.GameObjectId != playerObject.GameObjectId && shirker.IsParty()
				&& set.Action is { RowId: (uint)ActionID.ShirkPvE })
			{
				foreach (var effect in set.TargetEffects)
				{
					if (effect.TargetID == playerObject.GameObjectId)
					{
						TankSwapWatch.RecordShirkOnPlayer(shirker.GameObjectId, shirker.Name.TextValue);
						break;
					}
				}
			}

			// Whom an enemy struck, and when: the provoke delay ends with its first hit on the member it turned to (A500).
			if (IsHostileSource(set) && set.Source is IBattleChara striker)
			{
				foreach (var effect in set.TargetEffects)
				{
					if (ReachedTarget(effect))
					{
						DataCenter.LastStrikeByEnemy[striker.GameObjectId] = (effect.TargetID, DateTime.Now);
						break;
					}
				}

				// How dangerous it is: its damage on the party, and its heals on other enemies (concept 03, A541).
				RecordEnemyDanger(set, striker, playerObject);

				// Concept 21, K3 (A724): the first action of an enemy that runs to another is its arrival there.
				Approach.ObserveAction(striker, ReachedYourSide(set, playerObject));
			}

			// The first auto-attack after a swap Shirk shows whom the enemy attacks now.
			if (set.Source is IBattleChara swinger && set.Action is { } swing && swing.GetActionCate() == ActionCate.Autoattack)
			{
				foreach (var effect in set.TargetEffects)
				{
					if (ReachedTarget(effect))
					{
						TankSwapWatch.RecordAutoAttack(swinger.GameObjectId, effect.TargetID);
						break;
					}
				}
			}

			// A tankbuster marker is confirmed by any enemy action that damages the marked member, from a
			// targetable enemy or from one of the invisible helpers that resolve many of them - so this
			// comes before the source filter below. Auto-attacks do not count: the tank takes them
			// anyway, and they would confirm every marker on him.
			if (IsHostileSource(set) && set.Source is IBattleChara source
				&& set.Action is { } marked && marked.GetActionCate() != ActionCate.Autoattack)
			{
				// An action on the area list has hit every party member at least once: a raidwide, not the
				// tankbuster a marker names, even when it lands on the marked member while the marker stands -
				// Feldgeschrei and Wehklagen went into the tankbuster table and to the tank swap that way (traces of
				// 22. and 23.10.2026, A337). It still confirms the marker; that error leans the safe way (concept 15).
				var partyWide = TankbusterTable.IsPartyWide(marked.RowId);
				var markedForPlayer = false;
				var tankbusterForPlayer = 0u;
				var reached = -1;
				_tankbusterTargets.Clear();
				foreach (var effect in set.TargetEffects)
				{
					// A target the set names twice counts once (A454).
					if (ReachedTarget(effect) && _tankbusterTargets.Add(effect.TargetID))
					{
						var isMarked = TankbusterMarkerWatch.RecordHit(effect.TargetID, marked.RowId, partyWide) && !partyWide;
						var isTankbuster = isMarked || OtherConfiguration.HostileCastingTank.Contains(marked.RowId);
						if (effect.TargetID == playerObject.GameObjectId)
						{
							markedForPlayer = isMarked;
						}

						// The tankbuster table learns from every player it is seen to hit, not only from the user - the
						// co-tank's busters, and other parties' in large content; it keeps the hits on tanks (A312). Besides
						// list and marker it takes the landing of a cast the plan takes for a tankbuster, also one cast only
						// at its own target (A452), and adds up what one cast's hits take on one tank, under the cast's id,
						// which the forecast reads (TankbusterTable.Record, A454). A raidwide is no tankbuster. Asked before the
						// target is looked up: every hit on a player passes.
						var counted = 0u;
						if (!partyWide && TankbusterTable.MayCount(source, effect.TargetID, marked, isTankbuster)
							&& Svc.Objects.SearchById(effect.TargetID) is Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter struck)
						{
							if (reached < 0)
							{
								reached = ReachedTargets(set);
							}

							counted = TankbusterTable.Record(marked, struck, source,
								DamageShareOn(set, effect.TargetID, Math.Max(1u, struck.MaxHp)),
								struck.IsJobCategory(JobRole.Tank) ? StatusesAppliedTo(set, effect.TargetID) : _noStatuses, isTankbuster, reached);
						}

						// Marker and tank swap read the table under the id the series lies under, the cast's where one explains
						// the hit (A454); the tank swap stays with list and marker.
						if (isMarked && counted != 0 && counted != marked.RowId)
						{
							TankbusterMarkerWatch.LearnAs(effect.TargetID, marked.RowId, counted);
						}

						if (effect.TargetID == playerObject.GameObjectId)
						{
							tankbusterForPlayer = counted;
						}
						else if (isTankbuster)
						{
							TankSwapWatch.RecordBusterOnOther(source.GameObjectId, effect.TargetID, marked.Name.ExtractText(),
								counted != 0 ? counted : marked.RowId);
						}
					}
				}

				// The other half of the defence trace: which hits reached the player, so a defence in
				// the trace can be read against whether its hit arrived.
				var hitShare = DamageShareOn(set, playerObject.GameObjectId, Math.Max(1u, playerObject.MaxHp));
				var hitPlayer = false;
				foreach (var effect in set.TargetEffects)
				{
					if (effect.TargetID == playerObject.GameObjectId && ReachedTarget(effect))
					{
						hitPlayer = true;
						break;
					}
				}

				if (hitPlayer)
				{
					DefenseTrace.Line($"hit you: {marked.Name.ExtractText()} #{marked.RowId} from {source.Name.TextValue}"
						+ $" for {hitShare:P0} of max HP, {set.TargetEffects.Length} targets"
						+ (!markedForPlayer ? string.Empty
							: DataCenter.Role != JobRole.Tank || TankbusterForecast.TookCastFor(tankbusterForPlayer != 0 ? tankbusterForPlayer : marked.RowId)
								? ", after a tankbuster marker"
							: ", after a tankbuster marker, no cast taken for it"));
					TankSwapWatch.RecordHitOnPlayer(tankbusterForPlayer != 0 ? tankbusterForPlayer : marked.RowId, marked.Name.ExtractText(),
						source.GameObjectId, hitShare, markedForPlayer, !partyWide && OtherConfiguration.HostileCastingTank.Contains(marked.RowId),
						StatusesAppliedTo(set, playerObject.GameObjectId));

					// When the next one is due, from the spacing in this fight; turned away or absorbed, it still came.
					if (markedForPlayer || (!partyWide && OtherConfiguration.HostileCastingTank.Contains(marked.RowId)))
					{
						TankbusterForecast.RecordHitOnPlayer();
					}
					HitSeries.RecordHit(marked.RowId, hitShare > 0f, DamagedPartyMembers(set) > 1);
				}

				// Before the source filter below: the follow-up often comes from an untargetable helper.
				RecordFollowUpOfEmptyLanding(set, marked, playerObject, hitPlayer);
			}

			float damageRatio = 0;
			var playerId = playerObject.GameObjectId;
			var maxHp = playerObject.MaxHp;
			var denom = Math.Max(1u, maxHp);

			// An enemy nobody can target - the invisible helpers that resolve many raidwides, the parts of a
			// boss, or a boss while it is off the field - counts as an enemy here: the area recognition reads
			// its casts (DataCenter.AreaCasters, A277, A327), so its actions are listed and measured like any other.
			if (!IsHostileSource(set) || set.Source is not IBattleChara battle)
			{
				return;
			}

			damageRatio = DamageShareOn(set, playerId, denom);

			DataCenter.EnemyEffectSets++;
			if (damageRatio > 0f)
			{
				DataCenter.EnemyHitsOnPlayer++;
			}

			DataCenter.AddDamageRec(damageRatio);

			// Settles an open hold of a predicted mitigation. The hold claims a bigger hit is still
			// coming; this is where the fight answers that, with no reading and no report in
			// between. Called for every hit, including the small ones, because a window that closes
			// without a big hit is exactly what marks the hold as wrong.
			DataCenter.ScoreProactiveHold(damageRatio);

			foreach (var effect in set.TargetEffects)
			{
				if (effect.TargetID != playerId)
				{
					continue;
				}

				if (effect.GetSpecificTypeEffect(ActionEffectType.Knockback, out var entry))
				{
					var knock = Svc.Data.GetExcelSheet<Knockback>()?.GetRow(entry.value);
					if (knock != null)
					{
						DataCenter.KnockbackStart = DateTime.Now;
						if (knock.Value.Speed > 0)
						{
							DataCenter.KnockbackFinished = DateTime.Now + TimeSpan.FromSeconds(knock.Value.Distance / (float)knock.Value.Speed);
						}

						if (set.Action.HasValue && Service.Config.RecordKnockbackies
							&& OtherConfiguration.HostileCastingKnockback.Add(set.Action.Value.RowId))
						{
							// The Add sits in the condition above: it reports whether it actually added,
							// so the membership test and the insertion are one lookup instead of a walk
							// over the set followed by an insertion. Upstream now carries that form too,
							// so the second Add this fork used to have here would always report false
							// - the id having just been inserted - and the store would never be saved.
							_ = OtherConfiguration.SaveHostileCastingKnockback();
						}
					}
					break;
				}
			}

			var partyMembers = DataCenter.PartyMembers;
			var partyMemberCount = partyMembers.Count;

			// New ids enter the AoE list only under "Record enemy AoE actions" and only from a party of at
			// least a light party, where hitting every member marks a party-wide hit. Neither condition
			// concerns measuring an id that is already listed.
			var intakeOpen = Service.Config.RecordCastingArea && partyMemberCount >= 4;

			// Why an enemy cast that hurt the player was or was not measured. The table can stay empty
			// for several different reasons, and the file ("{}") looks the same for all of them - so
			// the decision states its reason where it is taken, and the tally counts it. Casts only:
			// instant hits are never rated, and reporting them let every auto-attack overwrite the
			// reason for the raidwide before it. A cast in the AoE list is recorded further down, once
			// its reading is known.
			if (damageRatio > 0f && set.Action is { Cast100ms: > 0 } castAction)
			{
				var actionId = castAction.RowId;
				if (ActionTypeOf(set) != ActionType.Action)
				{
					DataCenter.RecordAreaMeasurementOutcome(actionId, "not measured - not a regular action",
						$" ({ActionTypeOf(set)})");
				}
				else if (castAction.GetActionCate() is not (ActionCate.Spell or ActionCate.Weaponskill or ActionCate.Ability))
				{
					DataCenter.RecordAreaMeasurementOutcome(actionId, "not measured - not a spell, weaponskill or ability",
						$" ({castAction.GetActionCate()})");
				}
				else if (!OtherConfiguration.HostileCastingArea.Contains(actionId))
				{
					DataCenter.RecordAreaMeasurementOutcome(actionId, "not measured - not in the AoE list",
						!Service.Config.RecordCastingArea
							? " (Record enemy AoE actions is off, so it is not added either)"
							: intakeOpen
								? " (added once it hits every member)"
								: $" (party counted as {partyMemberCount}, too small to add it)");
				}
			}

			// "Record enemy AoE actions" decides whether new ids enter the list - its text and its origin
			// upstream say that, and a user may switch it off to keep the curated list as shipped. It
			// does not decide whether listed actions are measured: measuring answers how hard a listed
			// cast hits, and the rules that read the answer - skipping small casts, healing ahead of a
			// large one, releasing a defensive hold - decide for themselves. Hung on this switch, the
			// measurement went dark for anyone who only wanted the list left alone, and with it all
			// three of them. The same holds for the party size: it qualifies the intake, not a reading.
			if (ActionTypeOf(set) == ActionType.Action && set.Action?.Cast100ms > 0)
			{
				var type = set.Action?.GetActionCate();
				if (type is ActionCate.Spell or ActionCate.Weaponskill or ActionCate.Ability)
				{
					// Members the action reached, for the intake: a damage entry with an amount or the flag below, or a
					// hit turned away by invulnerability, evasion or resistance. A tank under Hallowed Ground or Superbolide
					// carries no damage entry; counted by damage alone, a raidwide landing on him stayed off the list, and a
					// tankbuster a patch made a raidwide kept drawing the tankbuster plan (his second test case, A452).
					var reachedMembers = 0;

					// The members by id, because the measurement below needs each one's maximum HP and
					// barrier and this is the one place that already walks the party. The previous form
					// kept only the ids and searched them with a foreach - a linear walk over a HashSet,
					// the same defect class that was just removed from DataCenter's four action lists.
					var partyById = new Dictionary<ulong, IBattleChara>(partyMemberCount);
					foreach (var pm in partyMembers)
					{
						partyById[pm.GameObjectId] = pm;
					}

					// The hardest this action has been seen to hit anyone in this set, as a share of
					// that member's maximum HP. The share does not age with item level or content
					// sync the way an amount would, and the highest share belongs to whoever is worst
					// off against it - which is what a decision about mitigating it would ask.
					var shares = new Dictionary<ulong, float>();
					var beyondHealth = 0;
					var hardestBeyond = 0f;
					var vulnerableMeasured = false;

					// Damage entries on party members, and those of them at zero on a member who carried no barrier.
					var damageEntries = 0;
					var zeroWithoutBarrier = 0;

					foreach (var effect in set.TargetEffects)
					{
						if (!partyById.TryGetValue(effect.TargetID, out var member))
						{
							continue;
						}

						if (!TryGetDamageEntry(effect, out var damageEffect))
						{
							if (ReachedTarget(effect))
							{
								reachedMembers++;
							}

							continue;
						}

						var landed = FullAmount(damageEffect) > 0;
						if (landed || (damageEffect.param0 & 6) == 6)
						{
							reachedMembers++;
						}

						damageEntries++;
						if (!landed && member.GetObjectShield() == 0)
						{
							zeroWithoutBarrier++;
						}

						// Only a real amount measures anything. A barrier makes no zero - the figure carries what it
						// absorbed (A449) -, so a zero entry is a hit that did nothing; with no barrier on anyone reached
						// (zeroWithoutBarrier below) the action is read as dealing nothing. For the intake above a zero
						// counts with the flag only, as before. Nor does a hit beyond the member's whole health and barrier (TryAreaShare):
						// it says the member missed the mechanic, not how hard the cast hits the rest.
						if (landed && member.MaxHp > 0)
						{
							if (TryAreaShare(FullAmount(damageEffect), member, set.Action!.Value.RowId, battle, out var share))
							{
								shares[member.GameObjectId] = Math.Max(shares.GetValueOrDefault(member.GameObjectId), share);
								vulnerableMeasured |= member.CarriesVulnerabilityUp();
							}
							else
							{
								beyondHealth++;
								hardestBeyond = Math.Max(hardestBeyond, (float)FullAmount(damageEffect) / member.MaxHp);
							}
						}
					}

					if (intakeOpen && reachedMembers == partyMemberCount
						&& OtherConfiguration.HostileCastingArea.Add(set.Action!.Value.RowId))
					{
						_ = OtherConfiguration.SaveHostileCastingArea();
					}

					// A listed action is rated by its series (SeriesShare); an unlisted one is not measured at all.
					var seriesSpan = 0f;
					var highestShare = OtherConfiguration.HostileCastingArea.Contains(set.Action!.Value.RowId)
						? SeriesShare(set.Action!.Value.RowId, shares, battle, out seriesSpan) : 0f;

					// Recording how hard it hits. DataCenter.AreaCastIsWorthMitigating reads this, and
					// the store is what carries a reading past the end of a session: without it every
					// login would start from nothing and a fight progged over several evenings would
					// never leave the unrated state. In repeated content one clear rates it.
					//
					// Two conditions, and they are deliberately not the intake's. First, the id has
					// to be a known area action already: the strict "every member was hit" test is
					// right for deciding what belongs in the list and wrong for measuring one, since
					// a raidwide with a member dead, invulnerable or out of range would throw the
					// reading away - in exactly the hard fights where it matters most. Second, every
					// reading goes to the table's revisions, which give the readers the highest one
					// (concept 13, "Stufe 1"). The mitigation the effect texts rate is divided out
					// (TryAreaShare); what they do not rate leaves a reading low - a barrier does not, its part is in the figure (A449) -,
					// and the highest value ever seen is the one a hit under less of it brings.
					// The line above said "measured" before the amount was known; say what the reading
					// was, or that there was none.
					// Above the level the content syncs to or in an unrestricted party (A466), every reading is smaller than in sync and would rate the action too
					// small for the synced runs after it: none is stored, a rating of 0 neither (A452).
					var outOfContentSync = DataCenter.OutOfContentSync;
					if (damageRatio > 0f && OtherConfiguration.HostileCastingArea.Contains(set.Action!.Value.RowId))
					{
						if (highestShare > 0f && outOfContentSync)
						{
							DataCenter.RecordAreaMeasurementOutcome(set.Action!.Value.RowId,
								$"not measured - {DataCenter.OutOfContentSyncReason}", $" ({highestShare:P0} of max HP unmitigated)");
						}
						else if (highestShare > 0f)
						{
							DataCenter.RecordAreaMeasurementOutcome(set.Action!.Value.RowId, "measured",
								$" at {highestShare:P0} of max HP unmitigated");
						}
						else if (beyondHealth > 0)
						{
							DataCenter.RecordAreaMeasurementOutcome(set.Action!.Value.RowId,
								"not measured - every hit beyond max HP and barrier");
						}
						else
						{
							DataCenter.RecordAreaMeasurementOutcome(set.Action!.Value.RowId,
								"not measured - no amount read from a party member");
						}
					}

					// What the measurement left out, so a trace shows the kill mechanic beside the reading
					// the cast was given instead.
					if (beyondHealth > 0 && OtherConfiguration.HostileCastingArea.Contains(set.Action!.Value.RowId))
					{
						DefenseTrace.Line($"area cast {set.Action!.Value.Name.ExtractText()} #{set.Action!.Value.RowId}:"
							+ $" {beyondHealth} hit beyond max HP and barrier (up to {hardestBeyond:P0}) not measured,"
							+ $" measured {highestShare:P0} unmitigated");
					}

					// Whether this landing reached the player (A205). Recorded apart from the setting that
					// reads it, so the record is there when the setting is switched on. A dead player
					// proves nothing; a damage entry of any size - blocked, parried or swallowed by a
					// barrier included - and a hit turned away by invulnerability or evasion count as
					// reached.
					if (!playerObject.IsDead && OtherConfiguration.HostileCastingArea.Contains(set.Action!.Value.RowId))
					{
						var reachedPlayer = false;
						foreach (var effect in set.TargetEffects)
						{
							if (effect.TargetID == playerId && ReachedTarget(effect))
							{
								reachedPlayer = true;
								break;
							}
						}

						DataCenter.AreaCastReachedPlayer[set.Action!.Value.RowId] = reachedPlayer;

						// A landing that reached nobody in the party carried no hit; if one comes, it comes as
						// a same-named action, and that one settles both records (RecordFollowUpOfEmptyLanding).
						var damagedMembers = DamagedPartyMembers(set);
						var landedName = set.Action!.Value.Name.ExtractText();
						lock (_emptyLandingByName)
						{
							if (damagedMembers == 0 && !reachedPlayer)
							{
								_emptyLandingByName[landedName] = set.Action!.Value.RowId;
							}
							else
							{
								_ = _emptyLandingByName.Remove(landedName);
							}
						}

						// "Skip area defence for casts that missed you" can only learn from a landing that
						// arrives here; the trace shows each one, so a cast that keeps opening the defence
						// without ever being recorded can be told from one that keeps reaching him.
						DefenseTrace.Line($"area cast landed: {landedName} #{set.Action!.Value.RowId}"
							+ $" from {battle.Name.TextValue}, reached you {reachedPlayer}, {set.TargetEffects.Length} targets"
							+ $" ({damagedMembers} party members reached)");
					}

					if (highestShare > 0f && !outOfContentSync && OtherConfiguration.HostileCastingArea.Contains(set.Action!.Value.RowId))
					{
						// Every reading, not only an increase: the revisions count it in its game version and keep the
						// highest for the readers, as written here before (concept 13, "Stufe 1").
						var id = set.Action!.Value.RowId;
						OtherConfiguration.MeasureAreaShare(id, highestShare, IsCleanAreaReading(battle, vulnerableMeasured));
						RecordAreaSeriesSpan(id, seriesSpan);
						MarkMeasuredUnmitigated(id);
					}
					else if (highestShare == 0f && !outOfContentSync && beyondHealth == 0 && zeroWithoutBarrier > 1 && zeroWithoutBarrier == damageEntries
						&& OtherConfiguration.HostileCastingArea.Contains(set.Action!.Value.RowId)
						&& !OtherConfiguration.HostileCastingAreaPotential.ContainsKey(set.Action!.Value.RowId))
					{
						// A landing that dealt nothing to members nobody shielded: zero here is not a hit something
						// absorbed but an action without damage - Impakt (#36794, Eliminator) put a damage entry of
						// zero on all four, and unrated it drew Temperance at every cast (owner's log of 25.10.2026,
						// A366). Rated 0, it is a small cast; a later landing with damage raises it, as any rating.
						var id = set.Action!.Value.RowId;
						OtherConfiguration.MeasureAreaShare(id, 0f, IsCleanAreaReading(battle, false));
						MarkMeasuredUnmitigated(id);
						DataCenter.RecordAreaMeasurementOutcome(id, "measured", $" at no damage on {zeroWithoutBarrier} party members without a barrier");
						DefenseTrace.Line($"area cast {set.Action!.Value.Name.ExtractText()} #{id}: no damage on {zeroWithoutBarrier}"
							+ " party members without a barrier - rated 0 %");
					}
				}
			}
		}
		catch (Exception ex)
		{
			// Counted and kept, not only logged: an exception here ends the handler before the
			// measurement, and if it happens on every set the damage table stays empty with nothing
			// to show for it but the log.
			DataCenter.EffectHandlerErrors++;
			if (DataCenter.EffectHandlerFirstError.Length == 0)
			{
				DataCenter.EffectHandlerFirstError = $"{ex.GetType().Name}: {ex.Message}";
			}

			PluginLog.Error($"Error in ActionFromEnemy: {ex}");
		}
	}

	private static void ActionFromSelf(ActionEffectSet set)
	{
		try
		{
			var playerObject = Player.Object;
			if (set.Source == null || playerObject == null)
			{
				return;
			}

			if (set.Source.GameObjectId != playerObject.GameObjectId)
			{
				return;
			}

			if (set.Action == null)
			{
				return;
			}

			if (set.Action.Value.ActionCategory.RowId == (uint)ActionCate.Autoattack)
			{
				return;
			}

			// A damage area grades itself over its duration: did the enemies stay in it (A353). Ahead of the check below:
			// an area that hits nobody when it goes out may come with no target effects.
			if (ActionTooltip.LeavesDamageArea(set.Action.Value.RowId)
				&& ActionTooltip.TryStatedDuration(set.Action.Value.RowId, out var areaDuration)
				&& (set.Action.Value.Range > 0 ? set.Target?.Position : playerObject.Position) is { } areaCentre)
			{
				HostileMovement.AreaLaid(set.Action.Value.Name.ExtractText(), areaCentre, set.Action.Value.EffectRange,
					areaDuration, DataCenter.AllHostileTargets);
			}

			if (set.TargetEffects.Length == 0)
			{
				return;
			}

			var action = set.Action;
			var tar = set.Target;

			DataCenter.AddActionRec(action!.Value);
			TankbusterForecast.RecordOwnAction(action.Value.RowId, tar?.GameObjectId ?? 0);

			// The trace writes what the defence chose; this line says that it went out.
			DefenseTrace.Executed(action.Value.RowId, action.Value.Name.ExtractText());
			Updaters.BlueMageSetTrace.Landed(action.Value);
			ObserveOwnHits(set, action.Value);

			// What an own heal brought each member it reached: against those without a status lowering HP recovery, the
			// share that reaches its bearers (concept 07, A544).
			var healed = AmountsByTarget(set, ActionEffectType.Heal);
			if (healed.Count > 1)
			{
				_ownHeals.Clear();
				foreach (var (target, amount) in healed)
				{
					_ownHeals.Add((target, amount));
				}

				HealReduction.RecordOwnHeal(_ownHeals);
			}

			if (action.Value.RowId == (uint)ActionID.ShirkPvE)
			{
				TankSwapWatch.ShirkLanded();
			}

			if (Service.Config.InDebug)
			{
				ShowStrSelf = set.ToString();
			}

			DataCenter.HealHP = AmountsByTarget(set, ActionEffectType.Heal);
			// A heal of ours on a figure only healing defeats: counted, and one for 0 takes its kind out (A516).
			HealToDefeat.NoteOwnHeals(DataCenter.HealHP);

			// Record what this heal was actually worth in health points. HealHP above is consumed and
			// cleared as soon as the server's own health update catches up, so it answers "do not heal
			// this target twice" and nothing beyond the next few frames; a rule that wants to know how
			// far one cast reaches needs the figure to survive the cast.
			//
			// With each amount goes the target's health when the effect arrived. Whether that is still
			// the health from before the heal is not known here - the server's health update can come
			// first - so DataCenter holds the cast until the health rise confirms it
			// (DataCenter.RecordHealEffect). A target outside the party list (a chocobo, an NPC without
			// the NPC setting) has no health to measure against and is left out.
			List<(ulong Id, uint Amount, uint Hp, uint MaxHp)> healLanded = [];
			if (DataCenter.HealHP is { Count: > 0 })
			{
				foreach (var (targetId, amount) in DataCenter.HealHP)
				{
					foreach (var member in DataCenter.PartyMembers)
					{
						if (member != null && member.GameObjectId == targetId)
						{
							healLanded.Add((targetId, amount, member.CurrentHp, member.MaxHp));
							break;
						}
					}
				}
			}

			DataCenter.ApplyStatus = set.GetSpecificTypeEffect(ActionEffectType.ApplyStatusEffectTarget) ?? [];
			var sourceApply = set.GetSpecificTypeEffect(ActionEffectType.ApplyStatusEffectSource);
			if (sourceApply is { Count: > 0 })
			{
				foreach (var effect in sourceApply)
				{
					DataCenter.ApplyStatus[effect.Key] = effect.Value;
				}
			}

			uint mpGain = 0;
			var mpEffects = set.GetSpecificTypeEffect(ActionEffectType.MpGain);
			if (mpEffects != null)
			{
				foreach (var effect in mpEffects)
				{
					if (effect.Key == playerObject.GameObjectId)
					{
						mpGain += effect.Value;
					}
				}
			}
			DataCenter.MPGain = mpGain;

			DataCenter.EffectTime = DateTime.Now;
			DataCenter.EffectEndTime = DateTime.Now.AddSeconds(set.Header.AnimationLockTime + 1);

			// After the effect window is set: the cast waits for its confirmation as long as the heal
			// projection waits for the server's health update, and no longer.
			if (DataCenter.HealHP is { Count: > 0 })
			{
				DataCenter.RecordHealEffect(action!.Value.RowId, healLanded);
			}

			var attackedTargets = DataCenter.AttackedTargets;
			var attackedTargetsCount = DataCenter.AttackedTargetsCount;

			if (attackedTargetsCount > 0)
			{
				foreach (var effect in set.TargetEffects)
				{
					if (!effect.GetSpecificTypeEffect(ActionEffectType.Damage, out _))
					{
						continue;
					}

					var targetExists = false;
					foreach ((var id, var time) in attackedTargets)
					{
						if (id == effect.TargetID)
						{
							targetExists = true;
							break;
						}
					}
					if (targetExists)
					{
						continue;
					}

					while (attackedTargets.Count >= attackedTargetsCount && attackedTargets.Count > 0)
					{
						(var id, var time) = attackedTargets.Peek();
						if (id == effect.TargetID)
						{
							break;
						}
						_ = attackedTargets.Dequeue();
					}

					attackedTargets.Enqueue((effect.TargetID, DateTime.Now));
				}
			}

			var regexOptions = RegexOptions.IgnoreCase;
			var eventsList = Service.Config.Events ?? [];
			var actionName = action.Value.Name.ExtractText() ?? string.Empty;
			if (!string.IsNullOrEmpty(actionName))
			{
				foreach (var item in eventsList)
				{
					if (string.IsNullOrWhiteSpace(item.Name))
					{
						continue;
					}

					bool isMatch;
					try
					{
						isMatch = Regex.IsMatch(actionName, item.Name, regexOptions);
					}
					catch (ArgumentException ex)
					{
						PluginLog.Warning($"Invalid regex in ActionEventInfo.Name: \"{item.Name}\". {ex.Message}");
						continue;
					}

					if (!isMatch)
					{
						continue;
					}

					if (item.AddMacro(tar))
					{
						break;
					}
				}
			}
		}
		catch (Exception ex)
		{
			PluginLog.Error($"Error in ActionFromSelf: {ex}");
		}
	}
}
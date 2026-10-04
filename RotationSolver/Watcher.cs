using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Hooks;
using ECommons.Hooks.ActionEffectTypes;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using RotationSolver.Basic.Configuration;
using System.Text.RegularExpressions;

namespace RotationSolver;

public static class Watcher
{
	public static void Enable()
	{
		var config = Service.Config;
		DefenseTrace.Start($"version {typeof(Watcher).Assembly.GetName().Version} | commit {SourceCommit()} | area defence {config.UseAoeDefense}"
			+ $" | single defence {config.UseStDefense} | skip casts that missed you {config.SkipAreaCastsThatMissedMe}"
			+ $" | big interruptible casts {config.MitigateBigAreaCastsEvenIfInterruptible} | BMR timeline {config.UseBmrTimeline}"
			+ $" | AoE list {OtherConfiguration.HostileCastingArea.Count} | tankbuster list {OtherConfiguration.HostileCastingTank.Count}");

		ActionEffect.ActionEffectEvent += ActionFromEnemy;
		ActionEffect.ActionEffectEvent += ActionFromSelf;
	}

	// The commit this build was made from, so an uploaded trace can be matched to the code that wrote it:
	// the version is the same for every build on the branch, and a day carries several commits.
	// Embedded by Directory.Build.props; missing when the build had no git.
	private static string SourceCommit()
	{
		foreach (var attribute in typeof(Watcher).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
		{
			if (attribute.Key == "SourceCommit")
			{
				return attribute.Value ?? "unknown";
			}
		}
		return "unknown";
	}

	public static void Disable()
	{
		ActionEffect.ActionEffectEvent -= ActionFromEnemy;
		ActionEffect.ActionEffectEvent -= ActionFromSelf;
		DefenseTrace.Stop("effect handler unhooked");
	}

	public static string ShowStrSelf { get; private set; } = string.Empty;

	// Amounts of damage and healing are read through FullAmount, never from .value alone. The entry
	// stores an amount as a 16-bit value; a larger one carries its third byte in the byte ECommons
	// calls mult, and only then is bit 0x40 of the byte it calls flags set ("a lot of damage":
	// cactbot's LogGuide, "Ability Damage" - bytes ABCD, C = 0x40, total = D A B). So .value alone
	// wrapped every hit or heal above 65,535 points - a level 100 raidwide on a tank, a Benediction
	// on one - and the measured shares came out too small, so a big area hit could be rated small.
	// EffectEntry.Damage adds mult without looking at the flag; where the byte means something else
	// that would inflate a small hit for good, because the store only ever raises a value.
	// ActionEffectSet.GetSpecificTypeEffect returns .value too ("Is this value or Damage? IDK about
	// it." in its source), so heals are collected here instead.
	private static uint FullAmount(EffectEntry entry)
		=> (entry.flags & 0x40) != 0 ? entry.Damage : entry.value;

	private static Dictionary<ulong, uint> AmountsByTarget(ActionEffectSet set, ActionEffectType type)
	{
		var result = new Dictionary<ulong, uint>();
		foreach (var effect in set.TargetEffects)
		{
			if (effect.GetSpecificTypeEffect(type, out var entry))
			{
				result[effect.TargetID] = FullAmount(entry);
			}
		}

		return result;
	}

	// The kind of action behind an effect set, read from the one byte the game gives it.
	//
	// ECommons declares EffectHeader.ActionType at offset 0x1F with FFXIVClientStructs' ActionType,
	// and that enum's underlying type is uint. The game's field is a single byte: ClientStructs'
	// own ActionEffectHandler.Header puts Flags at 0x20 and NumTargets at 0x21. So the field read
	// four bytes, and every set that hit anyone came out as 0x00NN0001 - "Action" in the low byte,
	// the number of targets in the third. Measured in the owner's trace (A177): the low byte was 1
	// in every line, the third byte matched the targets hit. A comparison with ActionType.Action
	// therefore failed exactly for the sets that damaged someone, and no area action was ever rated.
	private static ActionType ActionTypeOf(ActionEffectSet set) => (ActionType)(byte)set.Header.ActionType;

	// Damage the game reports as blocked or parried is damage taken all the same, at a reduced amount:
	// effect types 5 and 6 beside 3 (ECommons ActionEffectType). Read as Damage only, a tankbuster a
	// paladin blocked - under Sheltron every one is blocked - counted as no hit: its marker was learned
	// away as "not a tankbuster", a listed area cast as having missed him, and the amount went missing
	// from his damage intake (re-audit of A189 and A205, 29.09.2026).
	private static bool IsDamageEntry(ActionEffectType type)
		=> type is ActionEffectType.Damage or ActionEffectType.BlockedDamage or ActionEffectType.ParriedDamage;

	private static bool TryGetDamageEntry(TargetEffect effect, out EffectEntry entry)
	{
		var found = false;
		EffectEntry first = default;
		effect.ForEach(e =>
		{
			if (!found && IsDamageEntry(e.type))
			{
				found = true;
				first = e;
			}
		});
		entry = first;
		return found;
	}

	// Whether the action reached this target at all: damage of any kind, or a hit that an
	// invulnerability, an evasion or a resistance turned away. Each of these says the action was aimed
	// at the member and connected; only the absence of all of them says it did not. A tankbuster taken
	// under Hallowed Ground or Holmgang is still a tankbuster.
	private static bool ReachedTarget(TargetEffect effect)
	{
		var reached = false;
		effect.ForEach(e => reached |= IsDamageEntry(e.type) || e.type is ActionEffectType.Invulnerable
			or ActionEffectType.PartialInvulnerable or ActionEffectType.Miss or ActionEffectType.FullResist);
		return reached;
	}

	// How many party members this set reached, for the trace line of an area landing: "missed you" with
	// party members reached says he stood out of it; with none, the set carried no hit at all and the
	// damage came another way (trace of 01.10.2026, A242).
	private static int DamagedPartyMembers(ActionEffectSet set)
	{
		var count = 0;
		var party = DataCenter.PartyMembers;
		foreach (var effect in set.TargetEffects)
		{
			if (!ReachedTarget(effect))
			{
				continue;
			}

			for (var i = 0; i < party.Count; i++)
			{
				if (party[i]?.GameObjectId == effect.TargetID)
				{
					count++;
					break;
				}
			}
		}

		return count;
	}

	private static float DamageShareOn(ActionEffectSet set, ulong targetId, uint denom)
	{
		float share = 0;
		foreach (var effect in set.TargetEffects)
		{
			if (effect.TargetID == targetId)
			{
				effect.ForEach(entry =>
				{
					if (IsDamageEntry(entry.type))
					{
						share += (float)FullAmount(entry) / denom;
					}
				});
			}
		}

		return share;
	}

	// The statuses an effect set puts on one target: the entry's value is the status id.
	private static List<uint> StatusesAppliedTo(ActionEffectSet set, ulong targetId)
	{
		List<uint> statuses = [];
		foreach (var effect in set.TargetEffects)
		{
			if (effect.TargetID != targetId)
			{
				continue;
			}

			effect.ForEach(entry =>
			{
				if (entry.type == ActionEffectType.ApplyStatusEffectTarget && entry.value != 0)
				{
					statuses.Add(entry.value);
				}
			});
		}

		return statuses;
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

			// A party member's Shirk on the player, for the trace.
			if (set.Source is IBattleChara shirker && shirker.GameObjectId != playerObject.GameObjectId && shirker.IsParty()
				&& set.Action is { RowId: (uint)ActionID.ShirkPvE })
			{
				foreach (var effect in set.TargetEffects)
				{
					if (effect.TargetID == playerObject.GameObjectId)
					{
						TankSwapWatch.RecordShirkOnPlayer(shirker.Name.TextValue);
						break;
					}
				}
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
			if (set.Source is IBattleChara source
				&& (source.IsEnemy() || source.GetBattleNPCSubKind() == Dalamud.Game.ClientState.Objects.Enums.BattleNpcSubKind.Combatant)
				&& set.Action is { } marked && marked.GetActionCate() != ActionCate.Autoattack)
			{
				var markedForPlayer = false;
				foreach (var effect in set.TargetEffects)
				{
					if (ReachedTarget(effect))
					{
						var isMarked = TankbusterMarkerWatch.RecordHit(effect.TargetID);
						if (effect.TargetID == playerObject.GameObjectId)
						{
							markedForPlayer = isMarked;
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
						+ $" for {hitShare:P0} of max HP, {set.TargetEffects.Length} targets");
					TankSwapWatch.RecordHitOnPlayer(marked.RowId, marked.Name.ExtractText(), source.GameObjectId, hitShare,
						markedForPlayer, StatusesAppliedTo(set, playerObject.GameObjectId));
				}
			}

			float damageRatio = 0;
			var playerId = playerObject.GameObjectId;
			var maxHp = playerObject.MaxHp;
			var denom = Math.Max(1u, maxHp); // avoid division by zero

			if (set.Source is not IBattleChara battle || !set.Source.IsEnemy())
			{
				// An enemy nobody can target - the invisible helpers that resolve many raidwides, or a
				// boss while it is off the field - is left out of everything below, the damage table
				// included, because the consumers only ever read casts of targetable enemies. The tally
				// says how often that happened.
				if (set.Source is IBattleChara hidden && hidden.IsValid()
					&& hidden.GetBattleNPCSubKind() == Dalamud.Game.ClientState.Objects.Enums.BattleNpcSubKind.Combatant
					&& set.Action is { Cast100ms: > 0 } hiddenAction
					&& DamageShareOn(set, playerId, denom) > 0f)
				{
					DataCenter.RecordAreaMeasurementOutcome(hiddenAction.RowId,
						"not measured - cast by an untargetable enemy");
				}

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
							_ = OtherConfiguration.Save();
						}
					}
					break;
				}
			}

			var partyMembers = DataCenter.PartyMembers;
			var partyMemberCount = partyMembers.Count;

			// New ids enter the AoE list only under "Record AOE actions" and only from a party of at
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
							? " (Record AOE actions is off, so it is not added either)"
							: intakeOpen
								? " (added once it hits every member)"
								: $" (party counted as {partyMemberCount}, too small to add it)");
				}
			}

			// "Record AOE actions" decides whether new ids enter the list - its text and its origin
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
					var damageEffectCount = 0;

					// Maximum HP per member, because the measurement below needs it and this is the
					// one place that already walks the party. The previous form kept only the ids
					// and searched them with a foreach - a linear walk over a HashSet, the same
					// defect class that was just removed from DataCenter's four action lists.
					var partyMaxHp = new Dictionary<ulong, uint>(partyMemberCount);
					foreach (var pm in partyMembers)
					{
						partyMaxHp[pm.GameObjectId] = pm.MaxHp;
					}

					// The hardest this action has been seen to hit anyone in this set, as a share of
					// that member's maximum HP. The share does not age with item level or content
					// sync the way an amount would, and the highest share belongs to whoever is worst
					// off against it - which is what a decision about mitigating it would ask.
					var highestShare = 0f;

					foreach (var effect in set.TargetEffects)
					{
						if (!partyMaxHp.TryGetValue(effect.TargetID, out var memberMaxHp))
						{
							continue;
						}

						if (!TryGetDamageEntry(effect, out var damageEffect))
						{
							continue;
						}

						var landed = FullAmount(damageEffect) > 0;
						if (landed || (damageEffect.param0 & 6) == 6)
						{
							damageEffectCount++;
						}

						// Only a real amount measures anything. A hit that arrived at zero was
						// swallowed by a barrier or blocked outright, and zero does not mean the
						// action is harmless - it means something absorbed it. Such a set is skipped
						// for the measurement while still counting for the intake above, so a
						// raidwide does not fall out of the list just because the party was shielded.
						if (landed && memberMaxHp > 0)
						{
							var share = (float)FullAmount(damageEffect) / memberMaxHp;
							if (share > highestShare)
							{
								highestShare = share;
							}
						}
					}

					// Only write the file when this is a newly recorded action, not on every raidwide cast.
					if (intakeOpen && damageEffectCount == partyMemberCount
						&& OtherConfiguration.HostileCastingArea.Add(set.Action!.Value.RowId))
					{
						_ = OtherConfiguration.SaveHostileCastingArea();
					}

					// Recording how hard it hits. DataCenter.AreaCastIsWorthMitigating reads this, and
					// the store is what carries a reading past the end of a session: without it every
					// login would start from nothing and a fight progged over several evenings would
					// never leave the unrated state. In repeated content one clear rates it.
					//
					// Two conditions, and they are deliberately not the intake's. First, the id has
					// to be a known area action already: the strict "every member was hit" test is
					// right for deciding what belongs in the list and wrong for measuring one, since
					// a raidwide with a member dead, invulnerable or out of range would throw the
					// reading away - in exactly the hard fights where it matters most. Second, only
					// an increase is written. The highest value ever seen is the one that survives a
					// well-mitigated pull, and an underrated action corrects itself: the mitigation
					// is skipped, so the next hit arrives unmitigated and measures itself.
					// The line above said "measured" before the amount was known; say what the reading
					// was, or that there was none.
					if (damageRatio > 0f && OtherConfiguration.HostileCastingArea.Contains(set.Action!.Value.RowId))
					{
						if (highestShare > 0f)
						{
							DataCenter.RecordAreaMeasurementOutcome(set.Action!.Value.RowId, "measured",
								$" at {highestShare:P0} of max HP");
						}
						else
						{
							DataCenter.RecordAreaMeasurementOutcome(set.Action!.Value.RowId,
								"not measured - no amount read from a party member");
						}
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

						// "Skip area defence for casts that missed you" can only learn from a landing that
						// arrives here; the trace shows each one, so a cast that keeps opening the defence
						// without ever being recorded can be told from one that keeps reaching him.
						DefenseTrace.Line($"area cast landed: {set.Action!.Value.Name.ExtractText()} #{set.Action!.Value.RowId}"
							+ $" from {battle.Name.TextValue}, reached you {reachedPlayer}, {set.TargetEffects.Length} targets"
							+ $" ({DamagedPartyMembers(set)} party members damaged)");
					}

					if (highestShare > 0f && OtherConfiguration.HostileCastingArea.Contains(set.Action!.Value.RowId))
					{
						var id = set.Action!.Value.RowId;
						if (!OtherConfiguration.HostileCastingAreaPotential.TryGetValue(id, out var known)
							|| highestShare > known)
						{
							OtherConfiguration.HostileCastingAreaPotential[id] = highestShare;
							_ = OtherConfiguration.SaveHostileCastingAreaPotential();
						}
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
			//PluginLog.Debug($"ActionFromSelf invoked. Source: {set.Source?.GameObjectId}, Action: {set.Action?.Name.ExtractText() ?? "null"}");

			var playerObject = Player.Object;
			if (set.Source == null || playerObject == null)
			{
				//PluginLog.Debug("ActionFromSelf: Source or playerObject is null. Exiting.");
				return;
			}

			if (set.Source.GameObjectId != playerObject.GameObjectId)
			{
				//PluginLog.Debug($"ActionFromSelf: Source.GameObjectId ({set.Source.GameObjectId}) does not match playerObject.GameObjectId ({playerObject.GameObjectId}). Exiting.");
				return;
			}

			// Only process real actions/items. Mounts and other non-action types (e.g. ActionType.Mount)
			// reuse the Action sheet's row IDs, which can collide with unrelated GCDs/abilities and
			// corrupt LastAction/LastComboAction tracking if not filtered out here.
			//if (set.Header.ActionType is not ActionType.Action and not ActionType.Item)
			//{
			//	return;
			//}

			if (set.Action == null)
			{
				//PluginLog.Debug("ActionFromSelf: set.Action is null. Exiting.");
				return;
			}

			if (set.Action.Value.ActionCategory.RowId == (uint)ActionCate.Autoattack)
			{
				//PluginLog.Debug("ActionFromSelf: ActionCategory is Autoattack. Exiting.");
				return;
			}

			if (set.TargetEffects.Length == 0)
			{
				//PluginLog.Debug("ActionFromSelf: No TargetEffects. Exiting.");
				return;
			}

			var action = set.Action;
			var tar = set.Target;

			// Record
			//PluginLog.Debug($"ActionFromSelf: ActionType is {set.Header.ActionType}.");
			DataCenter.AddActionRec(action!.Value);

			// The trace writes what the defence chose; this line says that it went out.
			DefenseTrace.Executed(action.Value.RowId, action.Value.Name.ExtractText());
			if (action.Value.RowId == (uint)ActionID.ShirkPvE)
			{
				TankSwapWatch.ShirkLanded();
			}

			// Only shown on the Debug tab; formatting the whole effect set for every action is wasted otherwise.
			if (Service.Config.InDebug)
			{
				ShowStrSelf = set.ToString();
			}

			DataCenter.HealHP = AmountsByTarget(set, ActionEffectType.Heal);

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

			// Ensure ApplyStatus dictionary is non-null, then merge source-applied effects
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

					// Check if the target is already in the attacked targets list
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

					// Ensure the current target is not dequeued
					while (attackedTargets.Count >= attackedTargetsCount && attackedTargets.Count > 0)
					{
						(var id, var time) = attackedTargets.Peek();
						if (id == effect.TargetID)
						{
							// If the oldest target is the current target, break the loop to avoid dequeuing it
							break;
						}
						_ = attackedTargets.Dequeue();
					}

					// Enqueue the new target
					attackedTargets.Enqueue((effect.TargetID, DateTime.Now));
				}
			}

			// Macro
			// Not Compiled: the static Regex cache holds only a few entries, so compiled patterns would be
			// recompiled (slowly, on the game thread) whenever more events than that are configured.
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
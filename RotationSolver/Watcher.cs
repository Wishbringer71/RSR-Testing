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
		ActionEffect.ActionEffectEvent += ActionFromEnemy;
		ActionEffect.ActionEffectEvent += ActionFromSelf;
	}

	public static void Disable()
	{
		ActionEffect.ActionEffectEvent -= ActionFromEnemy;
		ActionEffect.ActionEffectEvent -= ActionFromSelf;
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

	private static void ActionFromEnemy(ActionEffectSet set)
	{
		try
		{
			if (set.Source is not IBattleChara battle || !set.Source.IsEnemy())
			{
				return;
			}

			var playerObject = Player.Object;
			if (playerObject == null)
			{
				return;
			}

			float damageRatio = 0;
			var playerId = playerObject.GameObjectId;
			var maxHp = playerObject.MaxHp;
			var denom = Math.Max(1u, maxHp); // avoid division by zero

			foreach (var effect in set.TargetEffects)
			{
				if (effect.TargetID == playerId)
				{
					effect.ForEach(entry =>
					{
						if (entry.type == ActionEffectType.Damage)
						{
							damageRatio += (float)FullAmount(entry) / denom;
						}
					});
				}
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

			// Why the last enemy action that hurt the player was or was not measured. The table can
			// stay empty for five different reasons, and the file ("{}") looks the same for all of
			// them - so the decision states its reason where it is taken. Only actions that actually
			// damaged the player count here, so auto-attacks from trash do not overwrite a raidwide.
			if (damageRatio > 0f && set.Action.HasValue)
			{
				var actionId = set.Action.Value.RowId;
				DataCenter.AreaMeasurementLastOutcome =
					!Service.Config.RecordCastingArea
						? $"{DateTime.Now:HH:mm:ss} #{actionId}: not measured - Record AOE actions is off"
					: partyMemberCount < 4
						? $"{DateTime.Now:HH:mm:ss} #{actionId}: not measured - party counted as {partyMemberCount}, 4 needed (NPC companions only count with the NPC party-member setting)"
					: !(set.Action?.Cast100ms > 0)
						? $"{DateTime.Now:HH:mm:ss} #{actionId}: not measured - instant, only cast actions are rated"
					: set.Header.ActionType != ActionType.Action
						? $"{DateTime.Now:HH:mm:ss} #{actionId}: not measured - action type {set.Header.ActionType}, only regular actions are rated"
					: set.Action?.GetActionCate() is not (ActionCate.Spell or ActionCate.Weaponskill or ActionCate.Ability)
						? $"{DateTime.Now:HH:mm:ss} #{actionId}: not measured - category {set.Action?.GetActionCate()}, only spells, weaponskills and abilities are rated"
					: !OtherConfiguration.HostileCastingArea.Contains(actionId)
						? $"{DateTime.Now:HH:mm:ss} #{actionId}: not measured - not in the AoE list (added only once it hits every member)"
						: $"{DateTime.Now:HH:mm:ss} #{actionId}: in the AoE list, measured";
			}

			if (Service.Config.RecordCastingArea && set.Header.ActionType == ActionType.Action && partyMemberCount >= 4 && set.Action?.Cast100ms > 0)
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

						if (!effect.GetSpecificTypeEffect(ActionEffectType.Damage, out var damageEffect))
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
					if (damageEffectCount == partyMemberCount
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
						DataCenter.AreaMeasurementLastOutcome = highestShare > 0f
							? $"{DateTime.Now:HH:mm:ss} #{set.Action!.Value.RowId}: in the AoE list, measured at {highestShare:P0} of max HP"
							: $"{DateTime.Now:HH:mm:ss} #{set.Action!.Value.RowId}: in the AoE list, not measured - no amount was read from a party member";
					}

					if (highestShare > 0f && Service.Config.RecordCastingArea
						&& OtherConfiguration.HostileCastingArea.Contains(set.Action!.Value.RowId))
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
			// With each amount goes what that target was missing when it arrived: the effect is seen
			// before the server's health update applies it, so the object still carries the health
			// from before the heal. That lets the record say how much of the cast met missing health,
			// and whether the packet reports overheal at all - which nobody had checked.
			if (DataCenter.HealHP is { Count: > 0 })
			{
				List<(uint Amount, uint MissingBefore)> landed = [];
				foreach (var (targetId, amount) in DataCenter.HealHP)
				{
					uint missing = 0;
					foreach (var member in DataCenter.PartyMembers)
					{
						if (member != null && member.GameObjectId == targetId)
						{
							missing = member.MaxHp > member.CurrentHp ? member.MaxHp - member.CurrentHp : 0;
							break;
						}
					}

					landed.Add((amount, missing));
				}

				DataCenter.RecordHealEffect(action!.Value.RowId, landed);
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
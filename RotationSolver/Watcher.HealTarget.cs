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
	// What each object took of one kind of amount from one effect set. An entry the game marks as landing on the source - bit
	// 0x80 of the byte ECommons calls flags, BossModReborn's ActionEffect.AtSource - goes to the source: the self-heal of an
	// attack (Storm's Path, Bloodbath) stands among the struck enemy's effects, as Storm's Path's 104 HP on the warrior in his
	// log of 04.11.2026. Read by target alone, it was a heal on that enemy, and an own one never showed as healing in flight
	// on the player (concept 07, "Einen Heilzauber abbrechen, den andere überflüssig gemacht haben", A545).
	private static Dictionary<ulong, uint> AmountsByTarget(ActionEffectSet set, ActionEffectType type)
	{
		var result = new Dictionary<ulong, uint>();
		var sourceId = set.Source?.GameObjectId ?? 0;
		foreach (var effect in set.TargetEffects)
		{
			var targetId = effect.TargetID;
			effect.ForEach(entry =>
			{
				if (entry.type != type)
				{
					return;
				}

				var to = sourceId != 0 && ActionEffectReading.LandsOnSource(entry) ? sourceId : targetId;
				result[to] = (result.TryGetValue(to, out var sum) ? sum : 0) + FullAmount(entry);
			});
		}

		return result;
	}

	// The kind of action behind an effect set, read from the one byte the game gives it.
	//
	// The game's field is a single byte: ClientStructs' own ActionEffectHandler.Header puts Flags at
	// 0x20 and NumTargets at 0x21. Up to 3.2.1.25 ECommons declared EffectHeader.ActionType at 0x1F
	// with FFXIVClientStructs' ActionType, whose underlying type is uint, so the field read four bytes
	// and every set that hit anyone came out as 0x00NN0001 - "Action" in the low byte, the number of
	// targets in the third (the owner's trace, A177). A comparison with ActionType.Action failed exactly
	// for the sets that damaged someone. ECommons 3.2.1.26 reads the byte itself (ActionTypeByte); the
	// cast stays, so going back to an older ECommons cannot reopen it (A296).
	private static ActionType ActionTypeOf(ActionEffectSet set) => (ActionType)(byte)set.Header.ActionType;

	// Damage, blocked and parried damage (ActionEffectReading.IsDamage).
	private static bool IsDamageEntry(ActionEffectType type)
		=> ActionEffectReading.IsDamage(type);

	// Concept 07, "Einen Heilzauber abbrechen, den andere überflüssig gemacht haben" (A545): a heal on a party member from a
	// source that is neither the player nor one of his objects nor an enemy - another healer, an NPC, the member himself.
	private static void RecordHealsByOthers(ActionEffectSet set, IBattleChara player)
	{
		if (set.Source == null || IsHostileSource(set)
			|| StatusHelper.IsPlayerOrOwned(set.Source.GameObjectId, player.GameObjectId))
		{
			return;
		}

		foreach (var (targetId, amount) in AmountsByTarget(set, ActionEffectType.Heal))
		{
			foreach (var member in DataCenter.PartyMembers)
			{
				if (member != null && member.GameObjectId == targetId)
				{
					HealFromOthers.RecordEffect(member, amount);
					break;
				}
			}
		}
	}
}

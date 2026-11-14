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
	// What every Sleep or Repose did to its targets - the player's own in full, anyone's towards the halving of repeated
	// sleeps - and every other action of another source resolving - the watched cast going off - for
	// SleepInterruptWatch (concept 08, "Ein gegnerisches Wirken stoppen").
	private static void ObserveSleep(ActionEffectSet set, ulong playerId)
	{
		if (set.Action is not { } action || set.Source is not IBattleChara source)
		{
			return;
		}

		if (!SleepInterruptWatch.IsSleepAction(action.RowId))
		{
			if (source.GameObjectId != playerId)
			{
				SleepInterruptWatch.ObserveEnemyAction(source.GameObjectId, action.RowId);
			}

			return;
		}

		List<SleepInterruptWatch.Observation> found = [];
		foreach (var effect in set.TargetEffects)
		{
			bool applied = false, resisted = false, unaffected = false;
			effect.ForEach(e =>
			{
				if (e.type == ActionEffectType.ApplyStatusEffectTarget)
				{
					applied = true;
				}
				// A resisted status (52, 55) or a resisted action: the kind is immune, and that is stored.
				else if (e.type is ActionEffectType.FullResistStatus or ActionEffectType.FullResistStatus2
					or ActionEffectType.FullResist)
				{
					resisted = true;
				}
				// A target that cannot be affected right now - an invulnerable phase, not an immunity of the kind.
				else if (e.type == ActionEffectType.Invulnerable)
				{
					unaffected = true;
				}
			});
			found.Add(new SleepInterruptWatch.Observation(effect.TargetID, applied, resisted, unaffected));
		}

		// Every sleep counts towards the halving, whoever put it on (SleepInterruptWatch.CanStillBeSlept).
		SleepInterruptWatch.CountApplications(found);
		if (source.GameObjectId == playerId)
		{
			SleepInterruptWatch.ObserveOwnSleep(action.RowId, found);
		}
	}

	private static bool IsStun(uint statusId)
	{
		foreach (var stun in StatusHelper.StunStatus)
		{
			if ((uint)stun == statusId)
			{
				return true;
			}
		}

		return false;
	}
}

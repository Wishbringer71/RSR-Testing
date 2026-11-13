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
	// K1 of the Crucible (concept 21, A549): every damage of the player or an object of his on an enemy of the fight in
	// progress, for the bonus of a hit on a weakness and for the share of damage hitting each weakness.
	private static void RecordCrucibleHits(ActionEffectSet set, IBattleChara player)
	{
		if (!DataCenter.IsInCrucible || CrucibleBoard.Fight == 0 || set.Source == null || set.Action is not { } action
			|| !StatusHelper.IsPlayerOrOwned(set.Source.GameObjectId, player.GameObjectId))
		{
			return;
		}

		foreach (var effect in set.TargetEffects)
		{
			if (TryGetDamageEntry(effect, out var entry) && Svc.Objects.SearchById(effect.TargetID) is IBattleChara target
				&& target.IsEnemy())
			{
				CrucibleBoard.RecordHit(action, target, entry, FullAmount(entry), set.Source.GameObjectId != player.GameObjectId);
			}

			// K10 shop (A570): the player's own healing on himself - his actions and items -, for gear that adds to it.
			if (effect.TargetID == player.GameObjectId && set.Source.GameObjectId == player.GameObjectId
				&& effect.GetSpecificTypeEffect(ActionEffectType.Heal, out var heal))
			{
				CrucibleBoard.RecordHealed(FullAmount(heal));
			}

			// K4 probe (A557): the last own action on each target and the kinds of its entries.
			var kinds = new List<byte>();
			effect.ForEach(e =>
			{
				if (e.type != 0 && !kinds.Contains((byte)e.type))
				{
					kinds.Add((byte)e.type);
				}
			});
			CrucibleBoard.RecordOwnAction(action.RowId, (uint)effect.TargetID, kinds);
		}
	}
}

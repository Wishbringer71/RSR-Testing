using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Hooks.ActionEffectTypes;

namespace RotationSolver.Engine.Helpers;

/// <summary>
/// Concept 05, "Arenagrenzen ohne BossMod-Modul" (A935): the first runtime contact with a death wall - an enemy ring with
/// no cast and no name in the action sheet that reaches a party member. Written once per action and area, with how far the
/// member stood from the ring's source and how far the ring reaches, so the learning rule by sectors can be built on what
/// the game shows. Nothing decides on it yet.
/// </summary>
internal static class ArenaEdgeWatch
{
	private static readonly HashSet<uint> _traced = [];

	internal static void ObserveLanding(ActionEffectSet set)
	{
		if (set.Action is not { } action || action.CastType != (byte)CastType.Donut || action.Cast100ms != 0
			|| !action.Name.IsEmpty || set.Source is not IBattleChara source || !source.IsEnemySideFigure()
			|| _traced.Contains(action.RowId))
		{
			return;
		}

		foreach (var effect in set.TargetEffects)
		{
			if (Svc.Objects.SearchById(effect.TargetID) is not IBattleChara member || !member.IsParty())
			{
				continue;
			}

			_ = _traced.Add(action.RowId);
			var who = member.GameObjectId == Player.Object?.GameObjectId ? "you" : member.ClassJob.Value.Abbreviation.ExtractText();
			DefenseTrace.Line($"arena edge: nameless ring #{action.RowId} without cast reached {who} "
				+ $"{Vector3.Distance(member.Position, source.Position):F1} y from its source, which reaches {action.EffectRange} y");
			return;
		}
	}

	internal static void AreaChanged() => _traced.Clear();
}

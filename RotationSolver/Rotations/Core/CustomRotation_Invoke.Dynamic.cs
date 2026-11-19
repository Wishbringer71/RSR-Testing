using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace RotationSolver.Rotations;

public partial class CustomRotation
{
	private static uint _dynamicTraced;
	private static DateTime _dynamicTracedAt;

	/// <summary>
	/// Concept 03, "Dynamische Rotation: geht eine Aktion nicht, kommt eine Alternative" (A930), D2: the GCD has stood free in
	/// a fight for longer than a GCD and the rotation chose none - its own conditions hold every GCD, as the samurai's Kenki
	/// guards did for 28 s (A923, A925). Then the rotation's damage GCD with the highest potency its text states goes that the
	/// game takes now, asked without the rotation's own conditions but with every central check. Not during a sequence a
	/// foreign GCD breaks (mudras, Ten Chi Jin, dance steps), not for limited jobs (Final Sting and Self-destruct state the
	/// highest potency and cost the life), not in PvP, and not while a raise is due.
	/// </summary>
	private IBaseAction? DynamicFallback()
	{
		if (!DataCenter.InCombat || DataCenter.IsPvP || DataCenter.DefaultGCDTotal <= 0f
			|| GcdIdleWatch.IdleSeconds < DataCenter.DefaultGCDTotal || DataCenter.DeathTarget != null
			|| Player is not { } player || player.ClassJob.Value.IsLimitedJob
			|| StatusHelper.PlayerHasStatus(true, StatusID.Mudra, StatusID.TenChiJin, StatusID.StandardStep, StatusID.TechnicalStep))
		{
			return null;
		}

		IBaseAction? best = null;
		var bestPotency = 0f;
		IBaseAction.IgnoreRotationChecks = true;
		try
		{
			foreach (var action in AllBaseActions)
			{
				if (!action.Info.IsGeneralGCD || !ActionTooltip.TryStatedPotency(action.AdjustedID, out var potency)
					|| potency <= bestPotency || !action.CanUse(out _) || !GameTakes(action, player))
				{
					continue;
				}

				best = action;
				bestPotency = potency;
			}
		}
		finally
		{
			IBaseAction.IgnoreRotationChecks = false;
		}

		if (best != null && (best.ID != _dynamicTraced || (DateTime.Now - _dynamicTracedAt).TotalSeconds >= DataCenter.DefaultGCDTotal))
		{
			_dynamicTraced = best.ID;
			_dynamicTracedAt = DateTime.Now;
			DefenseTrace.Line($"dynamic rotation: {best.Name} instead of none - no GCD for {GcdIdleWatch.IdleSeconds:F1} s");
		}

		return best;
	}

	private static unsafe bool GameTakes(IBaseAction action, IPlayerCharacter player)
	{
		var manager = ActionManager.Instance();
		return manager != null
			&& manager->GetActionStatus(ActionType.Action, action.AdjustedID, action.Target.Target?.GameObjectId ?? player.GameObjectId) == 0;
	}
}

using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using Action = Lumina.Excel.Sheets.Action;

namespace RotationSolver.Basic.Actions;

public partial class BaseAction : IBaseAction
{
	// The paths that answer a safety call, and the player's own press: they may end a damage channel (A380).
	private static bool SafetyMayEndAChannel(uint id)
	{
		return IBaseAction.HealPathRunning || IBaseAction.AreaDefenceRunning || IBaseAction.CommandedDefenceRunning
			|| IBaseAction.SelfProtectionHitsMe != null
			|| IBaseAction.TargetOverride is TargetType.Interrupt or TargetType.Dispel or TargetType.Heal or TargetType.BeAttacked
				or TargetType.Death or TargetType.Provoke or TargetType.TankSwap
			|| DataCenter.MergedStatus.HasFlag(AutoStatus.AntiKnockback)
			|| DataCenter.CommandNextAction?.ID == id;
	}
}

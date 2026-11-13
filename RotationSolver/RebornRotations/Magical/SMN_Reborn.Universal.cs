using System.ComponentModel;

namespace RotationSolver.RebornRotations.Magical;

public sealed partial class SMN_Reborn : SummonerRotation
{
	private string PhaseBookText(SearingPhase phase) =>
		SearingPhaseHeld(phase) ? "held" : SearingPhaseSeen(phase) ? "seen once" : "free";

	private double _summonHeldSeconds;

	// The time held this fight, including a hold that is still running.
	private double SummonHeldSecondsNow => _summonHeldSeconds
		+ (_summonHeldSince == DateTime.MinValue ? 0 : (DateTime.Now - _summonHeldSince).TotalSeconds);

	private static uint Missing(IBattleChara member) =>
		member.MaxHp > member.CurrentHp ? member.MaxHp - member.CurrentHp : 0;

	private static bool StatusEndsBefore(StatusID status, float seconds)
	{
		var left = StatusHelper.PlayerStatusTime(true, status);
		return left > 0 && left < seconds;
	}
}

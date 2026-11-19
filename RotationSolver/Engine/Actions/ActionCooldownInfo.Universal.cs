namespace RotationSolver.Engine.Actions;

public readonly partial struct ActionCooldownInfo
{
	/// <summary>
	/// The part of <see cref="CooldownCheck"/> that refused last, as a fixed string <c>Cooldown:&lt;part&gt;</c>, for the reason
	/// <c>CanUse</c> reports (concept 03, "Bereit, aber nicht genutzt", A855): the animation lock after every action, no
	/// charge, charges kept for later and the recast itself all read "Cooldown" before, and the trace line of a cooldown
	/// that stood ready could not tell a lock of a frame from a hold of a minute. Game thread, like the check.
	/// </summary>
	internal static string CooldownRefusal { get; private set; } = "Cooldown";

	// Set at the start of CooldownCheck and by each named refusal.

	private static bool RefuseCooldown(string part)
	{
		CooldownRefusal = part;
		return false;
	}

	private static readonly System.Collections.Concurrent.ConcurrentDictionary<uint, float> _fightEndTraced = new();

	/// <summary>
	/// Concept 03, "F · Über den Augenblick hinaus", K2 (A916): charges kept for later go when the boss - the last enemy in
	/// range - dies by its corrected time to kill before they would cap and before the own burst is due; kept, they would
	/// be lost with the fight. Not for friendly actions, whose last charge waits for danger, nor for a fixed-distance move.
	/// </summary>
	private bool FightEndsBeforeKeptChargesGo()
	{
		if (_action.Setting.IsFriendly || _action.Setting.SpecialType != SpecialActionType.None
			|| CustomRotation.NumberOfHostilesInRange > 1 || ECommons.DalamudServices.Svc.Targets.Target is not Dalamud.Game.ClientState.Objects.Types.IBattleChara target || !target.IsBossFromTTK())
		{
			return false;
		}

		var ttk = target.GetCorrectedTTK();
		var burstIn = CustomRotation.TinctureClock.SecondsToNext;
		if (float.IsNaN(ttk) || ttk >= RecastTimeRemain || (!float.IsNaN(burstIn) && ttk >= burstIn))
		{
			return false;
		}

		var id = _action.Info.AdjustedID;
		var fight = DataCenter.CombatTimeRaw;
		if (!_fightEndTraced.TryGetValue(id, out var tracedAt) || fight < tracedAt)
		{
			_fightEndTraced[id] = fight;
			DefenseTrace.Line($"fight end: {_action.Info.Name} charges released - the boss falls in {ttk:F1} s, before they cap "
				+ $"in {RecastTimeRemain:F1} s and before the burst ({(float.IsNaN(burstIn) ? "unknown" : $"{burstIn:F0} s")})");
		}

		return true;
	}
}

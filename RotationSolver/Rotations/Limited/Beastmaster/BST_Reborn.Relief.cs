namespace RotationSolver.Rotations.Limited.Beastmaster;

/// <summary>
/// Concept 01, B6 "Wo der Ruf ausbleibt" (A975): the stretch without a familiar in combat, measured frame by frame, so its line
/// says where the call stops - no horn ready, a horn not chosen, the gate closed on it, or no call begun.
/// </summary>
public sealed partial class BST_Reborn
{
	// Seconds a horn with a familiar stood ready, seconds a horn was the next action, of those with the gate closed, and the
	// calls begun.
	private static float _reliefReady;
	private static float _reliefChosen;
	private static float _reliefGateClosed;
	private static int _reliefCasts;
	private static bool _reliefWasCasting;

	// A975: a stretch without a familiar starts afresh - also where the familiar fell instead of being sent off.
	private static void StartStretchWithoutFamiliar(DateTime since)
	{
		_noneOutSince = since;
		_reliefReady = _reliefChosen = _reliefGateClosed = 0f;
		_reliefCasts = 0;
		_reliefWasCasting = false;
		_reliefFrame = since;
	}

	// A975: one frame of a stretch without a familiar in combat; it ends without one - the fight over, the player fallen, or
	// RSR not run for more than a GCD - with its line, where a familiar coming would have written it.
	private void FollowStretchWithoutFamiliar(DateTime now)
	{
		var gap = _reliefFrame != DateTime.MinValue && (now - _reliefFrame).TotalSeconds > DataCenter.DefaultGCDTotal;
		if (_noneOutSince != DateTime.MinValue && (!InCombat || Player is not { IsDead: false } || gap))
		{
			var end = gap ? _reliefFrame : now;
			if ((end - _noneOutSince).TotalSeconds > DataCenter.DefaultGCDTotal)
			{
				DefenseTrace.Line($"horn relief: {(end - _noneOutSince).TotalSeconds:F1} s without a familiar, none came - "
					+ (gap ? "RSR did not run on" : !InCombat ? "the fight ended" : "you fell") + StretchDetail(end));
			}

			_noneOutSince = DateTime.MinValue;
		}

		if (!InCombat || Player is not { IsDead: false } player)
		{
			return;
		}

		if (_noneOutSince == DateTime.MinValue)
		{
			StartStretchWithoutFamiliar(now);
			return;
		}

		var delta = (float)(now - _reliefFrame).TotalSeconds;
		_reliefFrame = now;
		if (AnyHornReadyWithFamiliar())
		{
			_reliefReady += delta;
		}

		if (Updaters.ActionUpdater.NextAction is IBaseAction next && IsBattlehorn(next.ID))
		{
			_reliefChosen += delta;
			if (Updaters.ActionUpdater.GateClosedByCastOrLock())
			{
				_reliefGateClosed += delta;
			}
		}

		var casting = player.IsCasting && IsBattlehorn(player.CastActionId);
		if (casting && !_reliefWasCasting)
		{
			_reliefCasts++;
		}

		_reliefWasCasting = casting;
	}

	// A975: what the stretch measured, for a stretch longer than a GCD.
	private static string StretchDetail(DateTime end)
	{
		return (end - _noneOutSince).TotalSeconds > DataCenter.DefaultGCDTotal
			? $" | a horn ready {_reliefReady:F1} s, chosen {_reliefChosen:F1} s, of that with the gate closed {_reliefGateClosed:F1} s;"
				+ $" calls begun {_reliefCasts}"
			: string.Empty;
	}

	private static bool AnyHornReadyWithFamiliar()
	{
		foreach (var horn in ThreeHorns)
		{
			if (HornReady(horn) && PetOnHorn(horn) != BeastmasterPet.None)
			{
				return true;
			}
		}

		return false;
	}

	private bool IsBattlehorn(uint id)
	{
		return id == FirstBattlehornPvE.ID || id == SecondBattlehornPvE.ID || id == ThirdBattlehornPvE.ID;
	}
}

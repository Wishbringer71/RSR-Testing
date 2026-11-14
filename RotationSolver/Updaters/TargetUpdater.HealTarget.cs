using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Updaters;

internal static partial class TargetUpdater
{
	/// <summary>
	/// A healer's cleanse (concept 07, "Reinigung oder Heilung", A565): one that saves a life - a status that kills when it
	/// runs out - goes before every heal; one that turns into something worse when it runs out (A574) before every heal
	/// while no member stands in the critical class. Anything else only while no heal or defence is asked for, the listed (dangerous)
	/// first; and a damage over time borne by more than one member, while an enemy is in reach and the job has a heal over
	/// time for the party, not at all - that heal answers it (his note of 05.11.2026). The MP a raise needs is kept in the
	/// action's own check (ActionBasicInfo.LeavesMpForRaise).
	/// </summary>
	private static IBattleChara? HealerDispelTarget(List<IBattleChara> candidates, bool nothingAsked)
	{
		var spreadDotsWait = DataCenter.HasHostilesInRange
			&& DataCenter.CurrentRotation is RotationSolver.Basic.Rotations.CustomRotation { HasGroupHealOverTime: true };
		_dotBearers.Clear();
		if (spreadDotsWait)
		{
			foreach (var member in DataCenter.PartyMembers)
			{
				if (member?.StatusList == null)
				{
					continue;
				}

				foreach (var status in member.StatusList)
				{
					if (status != null && status.CanDispel() && StatusHelper.IsDamageOverTime([(StatusID)status.StatusId]))
					{
						_dotBearers[status.StatusId] = _dotBearers.GetValueOrDefault(status.StatusId) + 1;
					}
				}
			}
		}

		IBattleChara? lethal = null, worse = null, listed = null, other = null;
		float lethalDist = float.MaxValue, worseDist = float.MaxValue, listedDist = float.MaxValue, otherDist = float.MaxValue;
		foreach (var person in candidates)
		{
			var statusList = person.StatusList;
			if (statusList == null)
			{
				continue;
			}

			bool kills = false, turnsWorse = false, isListed = false, any = false;
			foreach (var status in statusList)
			{
				if (status == null || !status.CanDispel())
				{
					continue;
				}

				if (StatusHelper.EndsInDeath((StatusID)status.StatusId))
				{
					kills = true;
					break;
				}

				if (spreadDotsWait && _dotBearers.GetValueOrDefault(status.StatusId) > 1)
				{
					continue;
				}

				any = true;
				isListed |= status.IsDangerous();
				turnsWorse |= StatusHelper.TurnsWorseOnExpiry((StatusID)status.StatusId);
			}

			var dist = ObjectHelper.DistanceToPlayer(person);
			if (kills && dist < lethalDist)
			{
				lethal = person;
				lethalDist = dist;
			}
			else if (!kills && turnsWorse && dist < worseDist)
			{
				worse = person;
				worseDist = dist;
			}
			else if (!kills && isListed && dist < listedDist)
			{
				listed = person;
				listedDist = dist;
			}
			else if (!kills && any && dist < otherDist)
			{
				other = person;
				otherDist = dist;
			}
		}

		if (lethal != null)
		{
			return lethal;
		}

		// A574: the heal of a member about to die saves a life now, the cleanse only when the status runs out.
		if (worse != null)
		{
			var critical = false;
			foreach (var member in DataCenter.PartyMembers)
			{
				critical |= member != null && !member.IsDead && member.IsInCriticalClass();
			}

			if (!critical)
			{
				return worse;
			}
		}

		return nothingAsked ? listed ?? worse ?? other : null;
	}

	private static void TraceParried(List<IBattleChara> parried)
	{
		_parriedNow.Clear();
		foreach (var enemy in parried)
		{
			_ = _parriedNow.Add(enemy.GameObjectId);
			if (_parriedTraced.Add(enemy.GameObjectId))
			{
				DefenseTrace.Line($"target: {enemy.Name} passed over - its Directional Parry guards the side you stand on, "
					+ "another enemy takes your attacks");
			}
		}

		_ = _parriedTraced.RemoveWhere(id => !_parriedNow.Contains(id));
	}

	private static void TraceSpiked(IBattleChara enemy)
	{
		if (!_spikedTraced.Add(enemy.GameObjectId))
		{
			return;
		}

		if (ObjectHelper.SpikeOf(enemy) is { } status)
		{
			DefenseTrace.Line($"target: {enemy.Name} left alone - {status.GameData.Value.Name.ExtractText()} ({status.StatusId}) "
				+ $"turns {StatusHelper.CounteredAttacks(status.StatusId)} attacks on its attackers, yours are {ObjectHelper.PlayerAttackKinds}, "
				+ $"{status.RemainingTime:F0} s left");
		}
	}
}

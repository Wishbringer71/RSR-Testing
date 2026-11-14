using ECommons.ExcelServices;
using Lumina.Excel.Sheets;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// The filter for target.
/// </summary>
public static class TargetFilter
{
	private static Dictionary<JobRole, HashSet<byte>>? _roleJobs;
	private static readonly Lock _roleJobsLock = new();

	private static Dictionary<JobRole, HashSet<byte>> GetRoleMap()
	{
		var map = _roleJobs;
		if (map != null)
		{
			return map;
		}

		lock (_roleJobsLock)
		{
			if (_roleJobs != null)
			{
				return _roleJobs;
			}

			var sheet = Service.GetSheet<ClassJob>();
			var dict = new Dictionary<JobRole, HashSet<byte>>();
			if (sheet != null)
			{
				foreach (var job in sheet)
				{
					var role = job.GetJobRole();
					if (!dict.TryGetValue(role, out var set))
					{
						set = [];
						dict[role] = set;
					}
					set.Add((byte)job.RowId);
				}
			}
			_roleJobs = dict;
			return _roleJobs;
		}
	}

	#region Find one target
	/// <summary>
	/// Get the dead ones in the list.
	/// </summary>
	/// <param name="charas">The list of characters.</param>
	/// <returns>The dead characters.</returns>
	public static IEnumerable<IBattleChara> GetDeath(this IEnumerable<IBattleChara> charas)
	{
		if (charas == null)
		{
			yield break;
		}

		foreach (var item in charas)
		{
			if (item == null)
			{
				continue;
			}

			if (!item.IsDead)
			{
				_ = _deathNotes.Remove(item.GameObjectId);
				continue;
			}

			// A765: a member of the party or alliance is never an enemy; for a dead one the game's answer to Blizzard
			// flickers (concept 11, "Ein totes Mitglied ist kein Gegner").
			if (item.CurrentHp != 0 || !item.IsTargetable || item.IsTargetMoving()
				|| (!item.IsParty() && !item.IsAllianceMember() && item.IsEnemy()))
			{
				NoteDeath(item, item.CurrentHp != 0 ? "HP above 0" : !item.IsTargetable ? "not targetable" : item.IsTargetMoving() ? "moving" : "an enemy");
				continue;
			}

			var raisetype = Service.Config.RaiseType;

			if (raisetype == RaiseType.AllOutOfDuty)
			{
				if (!item.IsParty() && !item.IsOtherPlayerOutOfDuty())
				{
					continue;
				}
			}
			if (raisetype != RaiseType.AllOutOfDuty)
			{
				if (!item.IsParty() && !item.IsAllianceMember())
				{
					continue;
				}
			}
			if (item.DistanceToPlayer() > 30)
			{
				NoteDeath(item, "beyond 30 y");
				continue;
			}

			if (!item.CanSee())
			{
				NoteDeath(item, "out of sight");
				continue;
			}

			if (item.HasStatus(false, StatusHelper.RaiseStatus))
			{
				NoteDeath(item, "raised, not yet accepted");
				continue;
			}

			// Raise lands as a status only once the cast is through; while another player is still
			// casting one on him, a second raise would only race it (A293). A693: unless the player's own
			// Swiftcast already runs and the other's lands later than his next GCD and a whole GCD after it.
			if (DataCenter.RaisedByOthers.TryGetValue(item.GameObjectId, out var other)
				&& !(StatusHelper.PlayerHasStatus(true, StatusHelper.SwiftcastStatus)
					&& other.LandsAt > DateTime.Now.AddSeconds(DataCenter.DefaultGCDRemain + DataCenter.DefaultGCDTotal)))
			{
				NoteDeath(item, "left to another raise that lands first");
				continue;
			}

			if (item.HasStatus(false, StatusID.ResurrectionDenied))
			{
				NoteDeath(item, "raise denied");
				continue;
			}

			if (!Service.Config.RaiseBrinkOfDeath && item.HasStatus(false, StatusID.BrinkOfDeath))
			{
				NoteDeath(item, "brink of death, not raised by the setting");
				continue;
			}

			if (!item.CanBeRaised())
			{
				NoteDeath(item, "the game refuses a raise on him");
				continue;
			}

			NoteDeath(item, "a candidate");
			yield return item;
		}
	}

	// A693 (concept 11): for a dead party member, one trace line each time the reason it is or is not a raise target changes,
	// with the seconds since RSR first saw it dead and its distance - the measure for the time between a death and the first
	// raise step. Dropped once the member is seen alive.
	private static readonly Dictionary<ulong, (DateTime Since, string Reason)> _deathNotes = [];

	private static void NoteDeath(IBattleChara item, string reason)
	{
		if (!item.IsParty())
		{
			return;
		}

		var now = DateTime.Now;
		var note = _deathNotes.TryGetValue(item.GameObjectId, out var known) ? known : (now, string.Empty);
		if (note.Item2 == reason)
		{
			return;
		}

		_deathNotes[item.GameObjectId] = (note.Item1, reason);
		DefenseTrace.Line($"raise: {item.Name.TextValue} dead {(now - note.Item1).TotalSeconds:F1} s, {item.DistanceToPlayer():F1} y - {reason}");
	}

	/// <summary>
	/// Get the specific roles members.
	/// </summary>
	/// <param name="objects">The list of objects.</param>
	/// <param name="roles">The roles to filter by.</param>
	/// <returns>The objects that match the roles.</returns>
	public static IEnumerable<IBattleChara> GetJobCategory(this IEnumerable<IBattleChara> objects, params JobRole[] roles)
	{
		if (objects == null || roles == null || roles.Length == 0)
		{
			return [];
		}

		var map = GetRoleMap();
		HashSet<byte> validJobs = [];
		foreach (var role in roles)
		{
			if (map.TryGetValue(role, out var set) && set != null)
			{
				foreach (var j in set)
				{
					validJobs.Add(j);
				}
			}
		}

		List<IBattleChara> result = [];
		foreach (var obj in objects)
		{
			if (obj != null && obj.IsJobs(validJobs))
			{
				result.Add(obj);
			}
		}

		return result;
	}

	/// <summary>
	/// Is the target the role.
	/// </summary>
	/// <param name="role">The role to check.</param>
	/// <returns>True if the object is of the specified role, otherwise false.</returns>
	public static bool PlayerJobCategory(JobRole role)
	{
		if (ECommons.GameHelpers.Player.Object == null)
		{
			return false;
		}

		var map = GetRoleMap();
		return map.TryGetValue(role, out var set) && PlayerIsJobs(set);
	}

	/// <summary>
	/// Is the target the role.
	/// </summary>
	/// <param name="battleChara">The game object.</param>
	/// <param name="role">The role to check.</param>
	/// <returns>True if the object is of the specified role, otherwise false.</returns>
	public static bool IsJobCategory(this IBattleChara battleChara, JobRole role)
	{
		if (battleChara == null)
		{
			return false;
		}

		// A blue mage counts in the role of his Aetheric Mimicry, as DataCenter.Role does for the player: every reader asks
		// what a member does in the party, not what his job is (concept 03, A559).
		if (battleChara.IsJobs(Job.BLU))
		{
			var mimicry = battleChara.HasStatus(false, StatusID.AethericMimicryTank) ? JobRole.Tank
				: battleChara.HasStatus(false, StatusID.AethericMimicryHealer) ? JobRole.Healer
				: JobRole.RangedMagical;
			return mimicry == role;
		}

		var map = GetRoleMap();
		return map.TryGetValue(role, out var set) && battleChara.IsJobs(set);
	}

	/// <summary>
	/// Is the Player in the jobs.
	/// </summary>
	/// <param name="validJobs">The valid jobs.</param>
	/// <returns>True if the object is in the valid jobs, otherwise false.</returns>
	public static bool PlayerIsJobs(params ReadOnlySpan<Job> validJobs)
	{
		if (ECommons.GameHelpers.Player.Object == null || validJobs.Length == 0)
		{
			return false;
		}

		var jobId = (byte)ECommons.GameHelpers.Player.Object.ClassJob.RowId;
		for (var i = 0; i < validJobs.Length; i++)
		{
			if ((byte)(uint)validJobs[i] == jobId)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Is the target in the jobs.
	/// </summary>
	/// <param name="battleChara">The game object.</param>
	/// <param name="validJobs">The valid jobs.</param>
	/// <returns>True if the object is in the valid jobs, otherwise false.</returns>
	public static bool IsJobs(this IBattleChara battleChara, params ReadOnlySpan<Job> validJobs)
	{
		if (battleChara == null || validJobs.Length == 0)
		{
			return false;
		}

		var jobId = (byte)battleChara.ClassJob.RowId;
		for (var i = 0; i < validJobs.Length; i++)
		{
			if ((byte)(uint)validJobs[i] == jobId)
			{
				return true;
			}
		}
		return false;
	}

	private static bool PlayerIsJobs(HashSet<byte> validJobs)
	{
		if (ECommons.GameHelpers.Player.Object == null || validJobs == null)
		{
			return false;
		}

		if (ECommons.GameHelpers.Player.Object is IBattleChara b && validJobs != null)
		{
			return validJobs.TryGetValue((byte)b.ClassJob.RowId, out _);
		}
		return false;
	}

	private static bool IsJobs(this IBattleChara battleChara, HashSet<byte> validJobs)
	{
		if (battleChara is IBattleChara b && validJobs != null)
		{
			return validJobs.TryGetValue((byte)b.ClassJob.RowId, out _);
		}
		return false;
	}
	#endregion

	/// <summary>
	/// Get the <paramref name="objects"/> in <paramref name="radius"/>.
	/// </summary>
	/// <typeparam name="T">The type of objects.</typeparam>
	/// <param name="objects">The list of objects.</param>
	/// <param name="radius">The radius to filter by.</param>
	/// <returns>The objects within the radius.</returns>
	public static IEnumerable<T> GetObjectInRadius<T>(this IEnumerable<T> objects, float radius) where T : IBattleChara
	{
		if (objects == null)
		{
			yield break;
		}

		foreach (var obj in objects)
		{
			if (obj.DistanceToPlayer() <= radius)
			{
				yield return obj;
			}
		}
	}
}
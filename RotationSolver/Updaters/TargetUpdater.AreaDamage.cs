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
	/// The enemies whose casts the cast recognition reads: the hostile list, and every untargetable enemy casting an
	/// action on the area list (A277), on the tankbuster or knockback list, or a tankbuster the table has measured
	/// (A303, <c>DataCenter.IsListedHostileCast</c>).
	/// </summary>
	/// <remarks>
	/// Many raidwides are cast by invisible helpers - Shinryu's Meteor Impact by five light spheres, 5 s cast, each
	/// hitting the whole party (trace of 13.10.2026) - and a boss off the field casts its own. The hostile list holds
	/// targetable enemies only (<see cref="GetAllTargets"/>), so none of these opened a defence. The list is the
	/// evidence of hostility here: an area id enters it only after it damaged every party member, a knockback only
	/// after it knocked someone, a tankbuster by the shipped list or a measured hit, and an untargetable caster
	/// carries no other. Tankbusters and knockbacks of helpers went unread until A303. Range and line of sight are left to the reach check of each cast,
	/// because a helper stands where its area is, not where an enemy can be attacked. A helper is any untargetable
	/// figure of the enemy side, of each of its kinds - combatant, part of an enemy, helper, or none
	/// (ObjectHelper.IsEnemySideFigure); read as combatants only, a dungeon boss's parts cast and hit unread (A327), and
	/// without the helper kind the helpers of trials and raids (A334). A targetable enemy the hostile list leaves out -
	/// behind a status of the invincible list, out of sight, on "No hostile names" - joins while it casts a listed action,
	/// not as a helper (A566).
	/// </remarks>
	private static void UpdateAreaCasters()
	{
		// Emptied and refilled, not made anew each frame (concept 24, "Speicher"); every reader reads them within the frame.
		var hostiles = DataCenter.AllHostileTargets;
		var casters = DataCenter.AreaCasters;
		var helperIds = DataCenter.AreaCastHelperIds;
		var casting = DataCenter.CastingEnemies;
		casters.Clear();
		helperIds.Clear();
		casting.Clear();
		casters.AddRange(hostiles);
		for (var i = 0; i < hostiles.Count; i++)
		{
			if (hostiles[i] is { IsCasting: true } hostile)
			{
				casting.Add(hostile);
			}
		}

		var longestHelperCast = 0f;

		foreach (var obj in Svc.Objects)
		{
			if (obj is not IBattleChara caster || caster.ObjectKind != Dalamud.Game.ClientState.Objects.Enums.ObjectKind.BattleNpc
				|| !caster.HasNativeState() || !caster.IsCasting || !caster.IsEnemySideFigure())
			{
				continue;
			}

			// A targetable enemy the hostile list leaves out - behind a status of the invincible list, out of sight, on
			// "No hostile names" - is no target, but its listed cast hits the party all the same (concept 13, A566).
			if (caster.IsTargetable)
			{
				if (caster.IsEnemy() && !hostiles.Contains(caster))
				{
					casting.Add(caster);
					if (DataCenter.IsListedHostileCast(caster.CastActionId))
					{
						casters.Add(caster);
					}
				}

				continue;
			}

			// Every helper's cast, listed or not: the tankbuster marker watch waits for the casts that run while a
			// marker stands, and a helper casting the hit after it is the common case, its action not yet listed. What a
			// step avoids is asked of its shape alone, without the list as evidence (concept 13, A750).
			longestHelperCast = Math.Max(longestHelperCast, caster.TotalCastTime - caster.CurrentCastTime);
			casting.Add(caster);

			if (!DataCenter.IsListedHostileCast(caster.CastActionId))
			{
				continue;
			}

			if (helperIds.Add(caster.GameObjectId))
			{
				casters.Add(caster);
			}
		}

		DataCenter.LongestHelperCastRemaining = longestHelperCast;
	}

	/// <summary>
	/// Whom other players are casting a raise or an Esuna on right now (A293).
	/// </summary>
	/// <remarks>
	/// A raise shows on its target as <c>StatusID.Raise</c> only once it lands. During another healer's
	/// eight-second hard cast the corpse was still a candidate, and RSR without Swiftcast started a raise of its
	/// own on it: two casts for one corpse, and whichever finished second was lost (his report of 15.10.2026).
	/// The other player's cast and its target are on screen the whole time; BossModReborn reads them the same way
	/// to leave a member to another healer's Esuna (<c>PartyHealth.Update</c>). A raise is recognised by game data,
	/// as the player's own (<c>ActionBasicInfo.RaiseAction</c>: <c>DeadTargetBehaviour</c> 1), so every job's and
	/// every duty's raise counts. A cast that is broken off is gone at the next update and its target is a
	/// candidate again; a raise that lands is then excluded by its status. Kept per body is the raise that
	/// lands first, as a point in time, so a reader in a later frame does not read an aged remainder.
	/// </remarks>
	private static void UpdateCastsByOthers()
	{
		Dictionary<ulong, (string Caster, DateTime LandsAt)> raised = [];
		var now = DateTime.Now;
		Dictionary<ulong, int> esunas = [];
		var playerId = Player.Object?.GameObjectId ?? 0;
		var sheet = Service.GetSheet<Lumina.Excel.Sheets.Action>();

		foreach (var chara in DataCenter.AllTargets)
		{
			if (chara == null || chara.GameObjectId == playerId || chara.IsDead || !chara.IsCasting
				|| chara.CastActionType != (byte)FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action)
			{
				continue;
			}

			var targetId = chara.CastTargetObjectId;
			if (targetId == 0 || targetId == chara.GameObjectId)
			{
				continue;
			}

			if (chara.CastActionId == (uint)ActionID.EsunaPvE)
			{
				esunas[targetId] = esunas.GetValueOrDefault(targetId) + 1;
			}
			else if (sheet.GetRowOrDefault(chara.CastActionId) is { DeadTargetBehaviour: 1 })
			{
				var left = Math.Max(0f, chara.TotalCastTime - chara.CurrentCastTime);
				var landsAt = now.AddSeconds(left);
				var seen = raised.TryGetValue(targetId, out var known);
				if (seen && known.LandsAt <= landsAt)
				{
					continue;
				}

				raised[targetId] = (chara.Name.TextValue, landsAt);
				if (!seen && !DataCenter.RaisedByOthers.ContainsKey(targetId))
				{
					DefenseTrace.Line($"raise left to {chara.Name.TextValue}: casting it on "
						+ $"{Svc.Objects.SearchById(targetId)?.Name.TextValue ?? "?"}, {left:F1} s left");
				}
			}
		}

		DataCenter.RaisedByOthers = raised;
		DataCenter.EsunasByOthers = esunas;
	}

	private static ulong _dutyOnlyCorpse;

	private static bool _dutyRestrictionSeen;

	// Whether a status on the member that kills when it runs out does so within the given seconds (A500; A565: only those -
	// a status that does not kill just ends, and cleansing it then gains nothing).
	private static bool DangerousStatusEndsWithin(IBattleChara person, float seconds)
	{
		var statusList = person.StatusList;
		if (statusList == null)
		{
			return false;
		}

		for (int i = 0, n = statusList.Length; i < n; i++)
		{
			var status = statusList[i];
			if (status != null && status.CanDispel()
				&& (StatusHelper.EndsInDeath((StatusID)status.StatusId) || StatusHelper.TurnsWorseOnExpiry((StatusID)status.StatusId))
				&& status.RemainingTime > 0 && status.RemainingTime <= seconds)
			{
				return true;
			}
		}

		return false;
	}
}

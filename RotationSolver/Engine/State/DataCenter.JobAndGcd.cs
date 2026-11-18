using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Config;
using ECommons;
using ECommons.GameFunctions;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using Action = Lumina.Excel.Sheets.Action;
using Buddy = FFXIVClientStructs.FFXIV.Client.Game.UI.Buddy;
using CharacterManager = FFXIVClientStructs.FFXIV.Client.Game.Character.CharacterManager;
using CombatRole = RotationSolver.Engine.Data.CombatRole;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using RotationSolver.Rotations.Limited.BlueMage;

namespace RotationSolver.Engine.State;

internal static partial class DataCenter
{
	#region Job Info
	public static Job Job => Player.Job;

	public static RotationType PlayerRotationType => RotationType.Reborn;

	// TODO: add the two Evercold jobs once they exist in ecommons.
	private static readonly HashSet<Job> EvolvedOnlyJobs = [];

	public static bool IsRotationTypeAllowed(Job job, RotationType type)
	{
		if (job is Job.BLU or Job.BST)
		{
			return true;
		}

		return type == (EvolvedOnlyJobs.Contains(job) ? RotationType.Evolved : PlayerRotationType);
	}

	private static readonly BaseItem PhoenixDownItem = new(4570);

	/// <summary>
	/// Whether a healer in the party is alive, the player counted unless <paramref name="excludeSelf"/>. Without one
	/// nothing restores what a member loses but his own heals: the heals a tank or damage dealer holds back for the
	/// healers go (<c>CustomRotation.HoldPartyHeal</c>, <c>GcdHealsAllowed</c>), and so does the defence the player
	/// keeps (<see cref="UnhealedUnderAttack"/>, A361). A living raiser does not count here: whether he raises, and
	/// when, is another player's doing, and a Phoenix Down in someone's bag cannot be seen at all (the owner's note,
	/// 25.10.2026). One definition for the three callers that carried it as their own loop.
	/// </summary>
	/// <param name="excludeSelf">Skip the player, for a healer asking whether another healer is there.</param>
	/// <returns>True when a healer of the party is alive.</returns>
	public static bool AnyLivingHealer(bool excludeSelf)
	{
		return AnyLiving(JobRole.Healer, excludeSelf);
	}

	public static bool CanRaise()
	{
		if (IsPvP)
		{
			return false;
		}

		if (Service.Config.UsePhoenixDown && PhoenixDownItem.HasIt)
		{
			return true;
		}

		if ((Role == JobRole.Healer || Job == Job.SMN) && PlayerSyncedLevel() >= RaiseLevel)
		{
			return true;
		}

		if (Job == Job.RDM && PlayerSyncedLevel() >= VerraiseLevel)
		{
			return true;
		}

		if (DutyRotation.ChemistLevel >= 3)
		{
			return true;
		}

		if (DutyRotation.WhiteMageLevel >= 4)
		{
			return true;
		}

		if (StatusHelper.PlayerHasStatus(false, StatusID.VariantRaiseSet))
		{
			return true;
		}
		return false;
	}

	private static Job _roleJob = Job.ADV;
	private static JobRole _role = JobRole.None;

	public static JobRole Role
	{
		get
		{
			if (CurrentRotation is BlueMageRotation)
			{
				return BlueMageRotation.Role;
			}

			var job = Job;
			if (job != _roleJob || job == Job.ADV)
			{
				var classJob = Service.GetSheet<ClassJob>().GetRowOrDefault((uint)job);
				_role = classJob is { RowId: not 0 } row ? row.GetJobRole() : JobRole.None;
				_roleJob = job;
			}

			return _role;
		}
	}

	public static float JobRange
	{
		get
		{
			float radius = 25;
			if (!Player.Available)
			{
				return radius;
			}

			switch (Role)
			{
				case JobRole.Tank:
				case JobRole.Melee:
					radius = IsPvP ? 5 : 3;
					break;
			}

			return radius;
		}
	}

	public static unsafe bool SylphManagementFinished()
	{
		if (UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted(66049))
		{
			return true;
		}

		return false;
	}

	public static bool BaseClass()
	{
		if (Svc.Objects.LocalPlayer == null)
		{
			return false;
		}

		var rowId = Svc.Objects.LocalPlayer.ClassJob.RowId;
		return (rowId >= 1 && rowId <= 7) || rowId == 26 || rowId == 29;
	}
	#endregion

	#region GCD
	/// <summary>
	/// Returns the current animation lock remaining time (seconds).
	/// </summary>
	public static float AnimationLock => Player.AnimationLock;

	/// <summary>
	/// Time until the next ability relative to the next GCD window.
	/// Non-negative (clamped to 0).
	/// </summary>
	public static float NextAbilityToNextGCD => Math.Max(0f, DefaultGCDRemain - AnimationLock);

	/// <summary>
	/// Returns the total duration of the default GCD (seconds): the length of one GCD, also while none is
	/// running.
	/// </summary>
	/// <remarks>
	/// The game reports the GCD group's total only while its timer runs; between GCDs - out of range, between
	/// pulls, before the first GCD of a fight - it reads 0. Every reader asks for the length of a GCD (a lead
	/// time, a window, a throttle, "the first n GCDs of combat"), and 0 shut each of them: the defence trace of
	/// 01.10.2026 holds the same decision two and three times within 50 ms, past a throttle of one GCD (A240).
	/// So the last length the game reported stands in while the timer is idle, and before the first GCD the
	/// adjusted recast of a weaponskill does - the game's own figure for the player's speed.
	/// </remarks>
	public static float DefaultGCDTotal
	{
		get
		{
			var running = ActionManagerHelper.GetDefaultRecastTime();
			if (running > 0f)
			{
				_lastGcdTotal = running;
				return running;
			}

			return _lastGcdTotal > 0f ? _lastGcdTotal : Math.Max(0f, ActionManagerHelper.GetDefaultAdjustedRecastTime());
		}
	}

	/// <summary>
	/// Returns the remaining time for the default GCD by subtracting the elapsed time from the total recast time.
	/// Clamped to non-negative; 0 while no GCD is running.
	/// </summary>
	public static float DefaultGCDRemain => Math.Max(0f, ActionManagerHelper.GetDefaultRecastTime() - DefaultGCDElapsed);

	/// <summary>
	/// Returns the elapsed time since the start of the default GCD. Clamped to non-negative.
	/// </summary>
	public static float DefaultGCDElapsed => Math.Max(0f, ActionManagerHelper.GetDefaultRecastTimeElapsed());

	/// <summary>
	/// Calculates the action ahead time based on the default GCD total and configured multiplier.
	/// Result is clamped to non-negative.
	/// </summary>
	public static float CalculatedActionAhead => Math.Max(0f, DefaultGCDTotal * Service.Config.Action6Head);

	/// <summary>
	/// Calculates the total GCD time for a given number of GCDs and an optional offset.
	/// </summary>
	/// <param name="gcdCount">The number of GCDs.</param>
	/// <param name="offset">The optional offset.</param>
	/// <returns>The total GCD time.</returns>
	public static float GCDTime(uint gcdCount = 0, float offset = 0)
	{
		return (DefaultGCDTotal * gcdCount) + offset;
	}
	#endregion
}

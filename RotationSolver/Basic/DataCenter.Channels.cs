using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Config;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using RotationSolver.Basic.Configuration;
using RotationSolver.Basic.Rotations.Duties;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using Action = Lumina.Excel.Sheets.Action;
using Buddy = FFXIVClientStructs.FFXIV.Client.Game.UI.Buddy;
using CharacterManager = FFXIVClientStructs.FFXIV.Client.Game.Character.CharacterManager;
using CombatRole = RotationSolver.Basic.Data.CombatRole;

namespace RotationSolver.Basic;

internal static partial class DataCenter
{
	/// <summary>
	/// Concept 14, "Das Ende der Gruppe" (A591): the time until the last hostile in combat falls - their HP over their
	/// common fall per second from the health history, as if the damage went on to the others after each fall. It answers
	/// "how long is there something to hit", where <see cref="AverageTTK"/> answers "when does the average hostile fall": a
	/// hostile without a measured fall left the mean, and a boss beside a few adds averaged with them. PositiveInfinity
	/// while no hostile in combat has a measured fall.
	/// </summary>
	public static float PackTimeToKill
	{
		get
		{
			var now = Environment.TickCount64;
			if (_packTTKCacheTick != long.MinValue && now - _packTTKCacheTick < AverageTtkTtlMs)
			{
				return _packTTK;
			}

			var health = 0f;
			var fall = 0f;
			var targets = AllHostileTargets;
			for (int i = 0, n = targets.Count; i < n; i++)
			{
				var hostile = targets[i];
				if (hostile == null || hostile.CurrentHp == 0 || !hostile.InCombat())
				{
					continue;
				}

				health += hostile.CurrentHp;
				var rate = TimeToKillTracker.GetRate(hostile.GameObjectId);
				if (!float.IsNaN(rate) && rate > 0f)
				{
					fall += rate * hostile.MaxHp;
				}
			}

			_packTTKKnown = fall > 0f;
			_packTTK = _packTTKKnown ? health / fall : float.PositiveInfinity;
			_packTTKCacheTick = now;
			return _packTTK;
		}
	}

	/// <summary>False while no hostile in combat has a measured fall; <see cref="PackTimeToKill"/> is PositiveInfinity then.</summary>
	public static bool IsPackTimeToKillKnown
	{
		get
		{
			_ = PackTimeToKill;
			return _packTTKKnown;
		}
	}

	/// <summary>
	/// Whether a channel of the player runs that another action would end and that is still of use (concept 14, "Ein Kanal,
	/// den eine weitere Aktion beendet", A380, A733): Flamethrower and Apokalypsis while their status stands, he stands
	/// still and a hostile is within the action's range in the cone before him; Chelonian Gate while its status stands and
	/// he stands still. "Effect ends upon using another action or moving" (their effect texts), so whatever reads this keeps
	/// other actions back.
	/// </summary>
	/// <remarks>
	/// Recognition only. Read by every action's check, so computed once per frame. Writes one trace line when such a channel
	/// ends, with how long it ran and the last action, so a channel cut short shows.
	/// </remarks>
	internal static bool KeptChannelRunning => _keptChannelRunning.Value;

	/// <summary>A733: the follow-up a kept channel lets go - Divine Cataract out of Chelonian Gate's Auspicious Trance.</summary>
	internal static bool IsFollowUpOfKeptChannel(uint actionId)
	{
		return actionId == (uint)ActionID.DivineCataractPvE && StatusHelper.PlayerHasStatus(true, StatusID.ChelonianGate);
	}

	// A733: the kept channels - the action, its status, and whether it is of use only while an enemy stands before him.
	private static readonly (ActionID Action, StatusID Status, bool NeedsEnemyBefore)[] KeptChannels =
	[
		(ActionID.FlamethrowerPvE, StatusID.Flamethrower, true),
		(ActionID.ApokalypsisPvE, StatusID.Apokalypsis, true),
		(ActionID.ChelonianGatePvE, StatusID.ChelonianGate, false),
	];

	private static bool ReadKeptChannel()
	{
		var now = DateTime.Now;
		var kept = false;
		for (var i = 0; i < KeptChannels.Length; i++)
		{
			var (action, status, needsEnemyBefore) = KeptChannels[i];
			var channel = Player.Object != null && StatusHelper.PlayerHasStatus(true, status);
			if (channel)
			{
				if (_channelSince[i] == DateTime.MinValue)
				{
					_channelSince[i] = now;
				}

				_channelSeen[i] = now;
			}
			else if (_channelSince[i] != DateTime.MinValue)
			{
				DefenseTrace.Line($"{action} ended after {(_channelSeen[i] - _channelSince[i]).TotalSeconds:F1} s | last action {LastAction} | moving {IsMoving}");
				_channelSince[i] = DateTime.MinValue;
			}

			if (!channel || IsMoving)
			{
				continue;
			}

			var range = Service.GetSheet<Action>().GetRowOrDefault((uint)action)?.EffectRange ?? 0;
			kept |= !needsEnemyBefore || ActionTargetInfo.HostileInFacingCone(range);
		}

		return kept;
	}
}

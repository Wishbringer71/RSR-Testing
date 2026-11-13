using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using Lumina.Excel.Sheets;
using RotationSolver.Basic.Configuration;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace RotationSolver.Basic.Helpers;

public static partial class ObjectHelper
{
	// Concept 01, K12 (A572): the zu piece and its eggs (BNpcName rows; Daedalus 74659a6, in the field).
	private const uint ZuPieceNameId = 14572;

	/// <summary>
	/// Concept 01, K12 (A572): a zu egg while a zu piece lives and "Crucible of the Unbroken - Zu logic" is on - no target,
	/// and no area attack may catch it.
	/// </summary>
	internal static bool IsGuardedZuEgg(this IBattleChara battleChara)
	{
		return Service.Config.CrucibleZu && DataCenter.IsInCrucible && Array.IndexOf(ZuEggNameIds, battleChara.NameId) >= 0
			&& AnyLivingHostile(ZuPieceNameId);
	}

	/// <summary>
	/// Whether this figure stands on the enemy side without having to be on the hostile list: a battle NPC of an
	/// enemy kind - a combatant, a part of an enemy, a helper, or one without a kind - that is no party member.
	/// </summary>
	/// <remarks>
	/// The hostile list holds what can be targeted and attacked. The invisible helpers that cast and resolve many
	/// raidwides, tankbusters and knockbacks, the parts of a large boss and a boss off the field are not on it, and
	/// they come in several kinds (FFXIVClientStructs <c>BattleNpcSubKind</c>: <c>Combatant</c> "Enemies, Guards",
	/// <c>BNpcPart</c> "Weak Spots"; <see cref="HelperSubKind"/>). Read as combatants only, a dungeon boss's every hit
	/// on the party went unread, among them one of 89 % right after a tankbuster marker, and the marker was learned as
	/// followed by no hit (trace of 18.10.2026, A327); read without helpers, 127 actions of helpers in trials, raids
	/// and dungeons still did, up to 88 % on eight members (traces of 19.-22.10.2026, A334). Guards are combatants too,
	/// so the kind alone is no proof of hostility: the caller adds its own evidence - a listed hostile cast, or an
	/// action that reached a party member.
	/// </remarks>
	/// <summary>
	/// The living tank of the party who holds the aggro that weighs most (concept 01, SGE, "Kardia folgt der Aggro", A567;
	/// his rule in concept 07: the tank holding the aggro has the heal priority): per tank the measured danger per second
	/// of the enemies targeting him (<see cref="EnemyDanger.Rate"/>), before any is measured the sum of their maximum HP.
	/// Null while no enemy targets a tank, and at a tie.
	/// </summary>
	internal static IBattleChara? TankHoldingAggro()
	{
		List<(IBattleChara Tank, double Rate, double Hp)> held = [];
		var anyRate = false;
		foreach (var tank in DataCenter.PartyMembers)
		{
			if (tank == null || tank.IsDead || !tank.IsJobCategory(JobRole.Tank))
			{
				continue;
			}

			double rate = 0, hp = 0;
			foreach (var enemy in DataCenter.AllHostileTargets)
			{
				if (enemy != null && enemy.TargetObjectId == tank.GameObjectId)
				{
					rate += EnemyDanger.Rate(enemy, out _, out _);
					hp += enemy.MaxHp;
				}
			}

			if (hp > 0)
			{
				held.Add((tank, rate, hp));
				anyRate |= rate > 0;
			}
		}

		// Measured danger where there is any, else the enemies' maximum HP; a tie answers nobody.
		IBattleChara? best = null;
		var bestWeight = 0d;
		var tie = false;
		foreach (var (tank, rate, hp) in held)
		{
			var weight = anyRate ? rate : hp;
			if (weight > bestWeight)
			{
				best = tank;
				bestWeight = weight;
				tie = false;
			}
			else if (weight == bestWeight && best != null)
			{
				tie = true;
			}
		}

		return tie ? null : best;
	}

	internal static bool IsEnemySideFigure(this IBattleChara battleChara)
	{
		return battleChara != null
			&& battleChara.ObjectKind == ObjectKind.BattleNpc
			&& battleChara.GetBattleNPCSubKind() is BattleNpcSubKind.Combatant or BattleNpcSubKind.BNpcPart
				or BattleNpcSubKind.None or HelperSubKind
			&& !battleChara.IsParty();
	}

	/// <summary>
	/// The battle NPC kind of a boss's invisible helpers. FFXIVClientStructs names no kind 11; BossModReborn calls it
	/// <c>Helper</c> (<c>ActorType.Helper = 0x20B</c>, battle NPC kind 11), and every unread hit on the player in his
	/// traces of 19.-22.10.2026 came from an untargetable figure of this kind (A334).
	/// </summary>
	private const BattleNpcSubKind HelperSubKind = (BattleNpcSubKind)11;
}

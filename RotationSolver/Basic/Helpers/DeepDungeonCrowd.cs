using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Concept 23, "Tiefe Gewölbe: Gegnerkontrolle" (A834): which enemies stay out of a deep dungeon fight. A sleeping enemy
/// is spared by every area of RSR's and attacked only when no awake enemy is left, whoever put it to sleep. Whether a
/// kind can be slept is learned by <see cref="SleepInterruptWatch"/> from the game's answer to every own sleep; this class
/// only remembers the enemy a sleep was just pressed on until its status could show, and writes each decision to the trace once.
/// </summary>
internal static class DeepDungeonCrowd
{
	private static ulong _pendingId;
	private static DateTime _pendingUntil = DateTime.MinValue;
	private static ulong _chosenTraced;
	private static float _pendingSleepLeft;
	private static readonly HashSet<ulong> _keptOutTraced = [];
	private static readonly HashSet<(uint Action, ulong Enemy)> _waitTraced = [];
	private static uint _territory;

	/// <summary>The sleeping enemies of this frame's list build, kept out of the targets (G4) and renewed (G5).</summary>
	internal static List<IBattleChara> Sleepers { get; set; } = [];

	/// <summary>Whether the rule may put enemies to sleep here: the option is on, a deep dungeon, not PvP.</summary>
	internal static bool Enabled => Service.Config.DeepDungeonSleepAdds && DataCenter.IsInDeepDungeons && !DataCenter.IsPvP;

	private static readonly HashSet<ulong> _party = [];
	private static readonly HashSet<ulong> _fought = [];

	/// <summary>
	/// Once a frame before the lists are built, from this frame's figures: who belongs to the party (the player and the
	/// party members, as IsParty counts them) and which enemies are fought - the player's target and every party tank's target
	/// (his precision, 16.11.2026, A836). Only in a deep dungeon with the sleep option on.
	/// </summary>
	internal static void Update(List<IBattleChara> allTargets)
	{
		_party.Clear();
		_fought.Clear();
		if (!Enabled || Player.Object is not { } player)
		{
			return;
		}

		_ = _party.Add(player.GameObjectId);
		if (player.TargetObjectId != 0)
		{
			_ = _fought.Add(player.TargetObjectId);
		}

		foreach (var member in allTargets)
		{
			if (member == null || !member.IsParty())
			{
				continue;
			}

			_ = _party.Add(member.GameObjectId);
			if (member.TargetObjectId != 0 && member.IsJobCategory(JobRole.Tank))
			{
				_ = _fought.Add(member.TargetObjectId);
			}
		}
	}

	/// <summary>Whether <paramref name="enemy"/> attacks the party: its target is the player or a party member.</summary>
	internal static bool AttacksParty(IBattleChara enemy) => enemy.TargetObjectId != 0 && _party.Contains(enemy.TargetObjectId);

	/// <summary>Whether <paramref name="enemy"/> is fought: the player's target or a party tank's.</summary>
	internal static bool IsFought(IBattleChara enemy) => _fought.Contains(enemy.GameObjectId);

	/// <summary>
	/// Whether <paramref name="enemy"/> sleeps while the option is on in a deep dungeon: no area of RSR's may hit it, and it
	/// is attacked only when no awake enemy is left. Not one that is fought: slept along, it stays the target and the next
	/// attack wakes it (A836).
	/// </summary>
	internal static bool IsHeldAsleep(IBattleChara enemy)
	{
		return Enabled && !IsFought(enemy) && enemy.HasStatus(false, StatusID.Sleep);
	}

	/// <summary>Whether a sleep was just pressed on <paramref name="enemy"/> and its status cannot show yet.</summary>
	internal static bool IsPending(IBattleChara enemy)
	{
		return enemy.GameObjectId == _pendingId && DateTime.Now <= _pendingUntil;
	}

	/// <summary>
	/// A sleep was pressed on <paramref name="targetId"/> - by the rule or by the player: held as pending for its cast time
	/// and one GCD, the time its status needs to show, so the next GCD does not choose the enemy again. Called from
	/// <c>BaseAction.Use</c>.
	/// </summary>
	internal static void RecordPress(uint actionId, ulong targetId, float castTime)
	{
		if (!SleepInterruptWatch.IsSleepAction(actionId))
		{
			return;
		}

		_pendingId = targetId;
		_pendingUntil = DateTime.Now.AddSeconds(castTime + DataCenter.DefaultGCDTotal);
		// A renewal lands on a sleeper: it shows by its sleep growing longer, not by a sleep appearing.
		_pendingSleepLeft = Svc.Objects.SearchById(targetId) is IBattleChara enemy ? enemy.StatusTime(false, StatusID.Sleep) : 0f;
	}

	/// <summary>
	/// <paramref name="enemy"/> sleeps; if a sleep was pressed on it and its sleep is now longer than at the press - a first
	/// sleep or a renewal -, it landed: traced and no longer pending. Called by the
	/// list build for each sleeping enemy - the measure of the rule (a refusal is traced by SleepInterruptWatch).
	/// </summary>
	internal static void NoteAsleep(IBattleChara enemy)
	{
		if (enemy.GameObjectId != _pendingId || _pendingId == 0 || enemy.StatusTime(false, StatusID.Sleep) <= _pendingSleepLeft)
		{
			return;
		}

		_pendingId = 0;
		_chosenTraced = 0;
		DefenseTrace.Line($"deep dungeon: sleep landed on {enemy.Name.TextValue} (kind {enemy.BaseId})");
	}

	/// <summary>The rule chose <paramref name="sleep"/> for <paramref name="enemy"/>; traced once per enemy chosen in a row.</summary>
	internal static void NoteChosen(IBattleChara enemy, IBaseAction sleep, string why)
	{
		if (enemy.GameObjectId == _chosenTraced)
		{
			return;
		}

		_chosenTraced = enemy.GameObjectId;
		DefenseTrace.Line($"deep dungeon: sleep chosen for {enemy.Name.TextValue} (kind {enemy.BaseId}) with {sleep.Name}: {why}");
	}

	/// <summary>
	/// A heal or attack of <paramref name="actionId"/> waits because it would reach <paramref name="enemy"/>, not fighting
	/// yet. Traced once per action, enemy and floor.
	/// </summary>
	internal static void TraceWait(uint actionId, IBattleChara enemy)
	{
		if (!DataCenter.IsInDeepDungeons)
		{
			return;
		}

		NewFloor();
		if (!_waitTraced.Add((actionId, enemy.GameObjectId)))
		{
			return;
		}

		DefenseTrace.Line($"deep dungeon: {(ActionID)actionId} waits - it would reach {enemy.Name.TextValue} (kind {enemy.BaseId}), not fighting yet");
	}

	/// <summary>
	/// Whether, in a deep dungeon fight with "Deep dungeons: damage doesn't pull new enemies" on, <paramref name="enemy"/>
	/// is not fighting yet and so no new target; traced once per enemy and floor.
	/// </summary>
	internal static bool KeepsOut(IBattleChara enemy)
	{
		if (!DataCenter.InCombat || !DataCenter.IsInDeepDungeons || !Service.Config.DeepDungeonNoPullDamage
			|| !DataCenter.NotFightingYet(enemy))
		{
			return false;
		}

		NewFloor();
		if (_keptOutTraced.Add(enemy.GameObjectId))
		{
			DefenseTrace.Line($"deep dungeon: {enemy.Name.TextValue} (kind {enemy.BaseId}) is not a new target - not fighting yet");
		}

		return true;
	}

	// Each floor is its own territory; what was traced on the last one is let go.
	private static void NewFloor()
	{
		var territory = DataCenter.TerritoryID;
		if (territory != _territory)
		{
			_territory = territory;
			_keptOutTraced.Clear();
			_waitTraced.Clear();
		}
	}

	/// <summary>Whether a party member is in the heal chain's critical class: a heal that also deals damage does not wait then.</summary>
	internal static bool PartyInDanger()
	{
		foreach (var member in DataCenter.PartyMembers)
		{
			if (member != null && !member.IsDead && member.MaxHp > 0 && member.IsInCriticalClass())
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>Whether <paramref name="enemy"/> carries damage over time from anyone: its next tick would wake it.</summary>
	internal static bool TakesDamageOverTime(IBattleChara enemy)
	{
		return StatusHelper.Carries(enemy, static id => StatusHelper.IsDamageOverTime([(StatusID)id]));
	}
}

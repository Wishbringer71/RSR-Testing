using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Concept 23, "Tiefe Gewölbe: Gegnerkontrolle" (A834): the enemies RSR put to sleep in a deep dungeon so the player fights
/// one at a time, held by id and read from the object table where used (concept 03, "Dritte Invariante"), and the enemy
/// kinds a sleep did not reach in this session. A sleeper is spared by every area of RSR's and attacked only when no awake
/// enemy is left.
/// </summary>
internal static class DeepDungeonCrowd
{
	// Per enemy id: its kind, the sleep chosen for it, when, whether the player's cast on it was seen and when it ends,
	// and whether the sleep landed.
	private sealed class Sleeper
	{
		public required uint Kind { get; init; }
		public required uint ActionId { get; init; }
		public required DateTime Chosen { get; init; }
		public DateTime CastEnd { get; set; } = DateTime.MinValue;
		public bool Landed { get; set; }
	}

	private static readonly Dictionary<ulong, Sleeper> _sleepers = [];
	private static readonly List<ulong> _gone = [];
	private static readonly HashSet<uint> _unsleepable = [];
	private static uint _territory;

	/// <summary>Whether the rule may put enemies to sleep here: the option is on, a deep dungeon, not PvP.</summary>
	internal static bool Enabled => Service.Config.DeepDungeonSleepAdds && DataCenter.IsInDeepDungeons && !DataCenter.IsPvP;

	/// <summary>Whether <paramref name="enemy"/> is one RSR put to sleep and sleeps now: no area of RSR's may hit it.</summary>
	internal static bool IsHeldAsleep(IBattleChara enemy)
	{
		return _sleepers.Count > 0 && _sleepers.TryGetValue(enemy.GameObjectId, out var sleeper) && sleeper.Landed
			&& enemy.HasStatus(false, StatusID.Sleep);
	}

	/// <summary>Whether a sleep on <paramref name="enemy"/> is chosen and not judged yet, or its kind did not sleep this session.</summary>
	internal static bool IsBarred(IBattleChara enemy)
	{
		return _unsleepable.Contains(enemy.BaseId) || (_sleepers.TryGetValue(enemy.GameObjectId, out var sleeper) && !sleeper.Landed);
	}

	/// <summary>The rule chose <paramref name="action"/> to put <paramref name="enemy"/> to sleep.</summary>
	internal static void NoteChosen(IBattleChara enemy, uint action)
	{
		_sleepers[enemy.GameObjectId] = new Sleeper { Kind = enemy.BaseId, ActionId = action, Chosen = DateTime.Now };
		DefenseTrace.Line($"deep dungeon: sleep chosen for {enemy.Name.TextValue} ({enemy.BaseId}), {DataCenter.AllHostileTargets.Count} enemies listed");
	}

	/// <summary>
	/// Once a frame before the lists of targets are built: whether each chosen sleep was cast, landed or wore off. A sleep cast
	/// that shows no Sleep on the enemy a GCD after its cast ended marks its kind as not to be slept in this session
	/// (code.md, "Entscheidung zur Laufzeit"); a sleep never cast is dropped unjudged.
	/// </summary>
	internal static void Update()
	{
		var territory = DataCenter.TerritoryID;
		if (territory != _territory)
		{
			_territory = territory;
			_sleepers.Clear();
		}

		if (_sleepers.Count == 0)
		{
			return;
		}

		var now = DateTime.Now;
		var gcd = DataCenter.DefaultGCDTotal;
		var player = Player.Object;
		_gone.Clear();
		foreach (var (id, sleeper) in _sleepers)
		{
			if (Svc.Objects.SearchById(id) is not IBattleChara enemy || enemy.IsDead)
			{
				_gone.Add(id);
				continue;
			}

			var asleep = enemy.HasStatus(false, StatusID.Sleep);
			if (!sleeper.Landed && asleep)
			{
				sleeper.Landed = true;
				DefenseTrace.Line($"deep dungeon: sleep landed on {enemy.Name.TextValue} ({sleeper.Kind})");
				continue;
			}

			if (sleeper.Landed)
			{
				if (!asleep)
				{
					_gone.Add(id);
					DefenseTrace.Line($"deep dungeon: {enemy.Name.TextValue} woke after {(now - sleeper.Chosen).TotalSeconds:F0} s");
				}

				continue;
			}

			if (sleeper.CastEnd == DateTime.MinValue)
			{
				if (player != null && player.IsCasting && player.CastActionId == sleeper.ActionId && player.CastTargetObjectId == id)
				{
					sleeper.CastEnd = now.AddSeconds(Math.Max(0f, player.TotalCastTime - player.CurrentCastTime));
				}
				else if ((now - sleeper.Chosen).TotalSeconds > gcd + gcd)
				{
					_gone.Add(id);
				}

				continue;
			}

			if ((now - sleeper.CastEnd).TotalSeconds > gcd)
			{
				_ = _unsleepable.Add(sleeper.Kind);
				_gone.Add(id);
				DefenseTrace.Line($"deep dungeon: no sleep on {enemy.Name.TextValue} a GCD after the cast - its kind ({sleeper.Kind}) is not slept again this session");
			}
		}

		foreach (var id in _gone)
		{
			_ = _sleepers.Remove(id);
		}
	}
}

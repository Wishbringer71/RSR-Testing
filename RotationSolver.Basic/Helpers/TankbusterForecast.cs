using ECommons.GameHelpers;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// What a tankbuster now being cast at the player is expected to do to him, against what he has to
/// take it - the measurement behind the tankbuster plan (concept 09, "Das geringste Mittel gegen einen
/// gemessenen Tankbuster").
/// </summary>
/// <remarks>
/// <para>Detection only: what is the case, under which figures. No option is read here, and no
/// action of the rotation is chosen. Which mitigation, whether the invulnerability, and whether the rest
/// is held, is decided in <c>CustomRotation</c> (<c>UpdateTankbusterPlan</c>,
/// <c>TankbusterPlanAbility</c>, <c>HoldDefenceForTankbuster</c>) and, for healers, in
/// <c>StateUpdater.ShouldAddDefenseSingle</c>.</para>
///
/// <para>The figure needs the action: only a cast names it. A marker or a BossModReborn prediction says
/// that a tankbuster comes, not which, so there is no figure. An action the learned table
/// (<see cref="TankbusterTable"/>) has not measured has none either.</para>
///
/// <para>Without risk, as the owner asks of every means short of the invulnerability: the prediction is
/// the highest figure ever measured for the action, under only the mitigation that still stands when the
/// hit arrives - the player's own, a co-tank's Reprisal, a healer's - and the budget is the HP the player
/// has now plus the barriers that still stand then. A heal that might come before the hit is not
/// counted, being an assumption about what someone will do.</para>
/// </remarks>
internal static class TankbusterForecast
{
	/// <summary>One tankbuster being cast at the player, and the figures on it.</summary>
	/// <param name="Predicted">Share of maximum HP the hit takes under what stands at impact; zero when
	/// the action has not been measured.</param>
	/// <param name="Budget">Share of maximum HP the player has to take it: HP now plus the barriers that
	/// still stand at impact.</param>
	/// <param name="Horizon">Seconds until the hit can arrive: the cast's rest plus one GCD.</param>
	/// <param name="Serial">Tells this cast from every other, also from a later cast of the same action
	/// by the same enemy with the same cast time.</param>
	internal sealed record Cast(int Serial, IBattleChara Source, uint ActionId, uint AttackType, float Remaining,
		float Total, float Predicted, float Budget, float Horizon)
	{
		/// <summary>Measured at all: the table has a figure for the action.</summary>
		public bool Measured => Predicted > 0f;
	}

	private static readonly List<Cast> _casts = [];

	// The player's own actions as the server confirmed them: action, target, when (monotonic, so a clock
	// change does not stretch anything), and whether its status has been seen since. Kept while the
	// action's effect can still last.
	private sealed class Execution
	{
		public required uint ActionId { get; init; }
		public required ulong Target { get; init; }
		public required long Tick { get; init; }
		public bool StatusSeen { get; set; }
	}

	private static readonly List<Execution> _own = [];
	private static readonly Dictionary<(ulong Source, uint ActionId, float Total), int> _serials = [];
	private static readonly HashSet<(ulong Source, uint ActionId, float Total)> _live = [];
	private static int _nextSerial;

	/// <summary>Every known tankbuster now being cast at the player, the soonest first.</summary>
	public static IReadOnlyList<Cast> Casts => _casts;

	/// <summary>The soonest cast with a figure, or null.</summary>
	public static Cast? SoonestMeasured { get; private set; }

	/// <summary>
	/// Whether the coming hit cannot touch the player: Hallowed Ground or Superbolide lasts past the
	/// soonest tankbuster being cast at him (plus one GCD for the hit to arrive) or, with no cast to time
	/// it, for at least one more GCD - long enough to still press a mitigation once it runs low.
	/// </summary>
	public static bool PlayerImpervious { get; private set; }

	/// <summary>
	/// Whether an invulnerability on the player - Hallowed Ground, Holmgang, Superbolide, Living Dead,
	/// Walking Dead or Undead Rebirth - is still up in <paramref name="seconds"/>, so that it covers a hit
	/// landing then.
	/// </summary>
	public static bool PlayerInvulnerableThrough(float seconds)
	{
		var player = Player.Object;
		return player != null && player.InvulnerableThrough(seconds);
	}

	/// <summary>
	/// An action of the player went out, as the effect handler saw it. Called on the game thread.
	/// </summary>
	public static void RecordOwnAction(uint actionId, ulong targetId)
	{
		var now = Environment.TickCount64;
		_ = _own.RemoveAll(e => SecondsSince(e.Tick, now) > DefensiveValues.DurationOf(e.ActionId));
		if (DefensiveValues.DurationOf(actionId) > 0f)
		{
			_own.Add(new Execution { ActionId = actionId, Target = targetId, Tick = now });
		}
	}

	/// <summary>
	/// For the gap between the server confirming one of the player's own actions and its status showing:
	/// the seconds it will still last by its duration from that moment, while its status has not been seen
	/// yet; zero once it has (from then on the status itself tells, also when it ends early - a barrier
	/// broken, a mitigation dispelled) or when the action did not go out within its duration. Whether the
	/// status showed is recorded every cycle in <see cref="Update"/>. <paramref name="anyTarget"/>: the
	/// action counts whatever it was aimed at (Holmgang on an enemy, Reprisal around the player).
	/// </summary>
	public static float OwnPendingCover(uint actionId, bool anyTarget)
	{
		var player = Player.Object;
		var lasts = DefensiveValues.DurationOf(actionId);
		if (player == null || lasts <= 0f)
		{
			return 0f;
		}

		Execution? latest = null;
		foreach (var execution in _own)
		{
			if (execution.ActionId == actionId && (anyTarget || execution.Target == player.GameObjectId)
				&& (latest == null || execution.Tick > latest.Tick))
			{
				latest = execution;
			}
		}

		return latest == null || latest.StatusSeen
			? 0f
			: Math.Max(0f, lasts - SecondsSince(latest.Tick, Environment.TickCount64));
	}

	// Marks every execution whose status - one the player applied, on him or on an enemy - now shows.
	private static void RecordStatusesSeen(IBattleChara player)
	{
		foreach (var execution in _own)
		{
			if (execution.StatusSeen)
			{
				continue;
			}

			var statuses = StatusesOfAction(execution.ActionId);
			if (statuses.Length == 0)
			{
				continue;
			}

			var seen = player.HasStatus(true, statuses);
			if (!seen)
			{
				foreach (var hostile in DataCenter.AllHostileTargets)
				{
					if (hostile != null && hostile.HasStatus(true, statuses))
					{
						seen = true;
						break;
					}
				}
			}

			execution.StatusSeen = seen;
		}
	}

	// The statuses an action puts on: those its effect text is tied to, and those its setting in the
	// current rotation provides (The Blackest Night's barrier has no mitigation figure, only a status).
	private static StatusID[] StatusesOfAction(uint actionId)
	{
		List<StatusID> statuses = [];
		if (DefensiveValues.MitigatingStatusesByActionId.TryGetValue(actionId, out var tied))
		{
			foreach (var status in tied)
			{
				statuses.Add((StatusID)status);
			}
		}

		var actions = DataCenter.CurrentRotation?.AllBaseActions;
		if (actions != null)
		{
			foreach (var action in actions)
			{
				if (action == null || (action.ID != actionId && Service.GetAdjustedActionId(action.ID) != actionId))
				{
					continue;
				}

				statuses.AddRange(action.Setting.StatusProvide ?? []);
				statuses.AddRange(action.Setting.TargetStatusProvide ?? []);
			}
		}

		return [.. statuses];
	}

	/// <summary>Whether the cast with <paramref name="serial"/> is still coming at the player.</summary>
	public static bool IsRunning(int serial)
	{
		foreach (var cast in _casts)
		{
			if (cast.Serial == serial)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>Seconds between two readings of <see cref="Environment.TickCount64"/>.</summary>
	internal static float SecondsSince(long tick, long now)
	{
		return (float)TimeSpan.FromMilliseconds(now - tick).TotalSeconds;
	}

	/// <summary>Recomputes the figures. Called once per framework cycle, on the game thread.</summary>
	public static void Update()
	{
		_casts.Clear();
		SoonestMeasured = null;
		PlayerImpervious = false;

		var player = Player.Object;
		if (!DataCenter.InCombat || player == null || player.IsDead || DataCenter.Role != JobRole.Tank)
		{
			_own.Clear();
			_serials.Clear();
			return;
		}

		RecordStatusesSeen(player);

		var gcd = DataCenter.DefaultGCDTotal;
		var maxHp = (float)Math.Max(1u, player.MaxHp);
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile == null || !hostile.IsCasting || hostile.CastTargetObjectId != player.GameObjectId)
			{
				continue;
			}

			var id = hostile.CastActionId;
			if (!OtherConfiguration.HostileCastingTank.Contains(id) && !IsLearned(id))
			{
				continue;
			}

			var left = hostile.TotalCastTime - hostile.CurrentCastTime;
			var horizon = left + gcd;
			var predicted = TankbusterTable.PredictedShare(id, player, hostile, horizon);

			// The barrier counts only when it still stands then; the earliest barrier's end decides, so a
			// short one beside a long one counts none (HasSurvivingShield).
			var barrier = player.HasSurvivingShield(horizon) ? player.GetObjectShield() / maxHp : 0f;
			var attackType = Service.GetSheet<Lumina.Excel.Sheets.Action>().TryGetRow(id, out var row)
				? row.AttackType.RowId
				: 0u;
			var identity = (hostile.GameObjectId, id, hostile.TotalCastTime);
			var isNew = !_serials.TryGetValue(identity, out var serial);
			if (isNew)
			{
				serial = ++_nextSerial;
				_serials[identity] = serial;
			}

			_ = _live.Add(identity);
			var cast = new Cast(serial, hostile, id, attackType, left, hostile.TotalCastTime, predicted,
				(player.CurrentHp / maxHp) + barrier, horizon);
			_casts.Add(cast);
			if (isNew)
			{
				Trace(cast);
			}
		}

		_casts.Sort((a, b) => a.Remaining.CompareTo(b.Remaining));
		foreach (var cast in _casts)
		{
			if (cast.Measured)
			{
				SoonestMeasured = cast;
				break;
			}
		}

		// Only the casts still running keep their serial; the next cast of the same action is a new one.
		foreach (var identity in _serials.Keys)
		{
			if (!_live.Contains(identity))
			{
				_ = _serials.Remove(identity);
			}
		}

		_live.Clear();

		PlayerImpervious = player.ImperviousThrough(_casts.Count == 0 ? gcd : _casts[0].Horizon);
	}

	// One line per cast, written when it is first seen, so the file shows what the plan was built on.
	private static void Trace(Cast cast)
	{
		var name = Service.GetSheet<Lumina.Excel.Sheets.Action>().TryGetRow(cast.ActionId, out var row)
			? row.Name.ExtractText()
			: $"#{cast.ActionId}";
		DefenseTrace.Line($"tankbuster coming at you: {name} #{cast.ActionId} in {cast.Remaining:F1} s - "
			+ (!cast.Measured
				? "not in the tankbuster table"
				: $"takes {cast.Predicted:P0} of max HP under what stands at impact, you have {cast.Budget:P0}"));
	}

	private static bool IsLearned(uint actionId)
	{
		lock (OtherConfiguration.TankbusterPotentialGate)
		{
			return OtherConfiguration.TankbusterPotential.ContainsKey(actionId);
		}
	}
}

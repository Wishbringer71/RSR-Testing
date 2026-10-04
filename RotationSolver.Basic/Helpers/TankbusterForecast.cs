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
	internal sealed record Cast(IBattleChara Source, uint ActionId, uint AttackType, float Remaining, float Total,
		float Predicted, float Budget, float Horizon)
	{
		/// <summary>Measured at all: the table has a figure for the action.</summary>
		public bool Measured => Predicted > 0f;
	}

	private static readonly List<Cast> _casts = [];

	// The player's own actions as the server confirmed them: action, target, and when (monotonic, so a
	// clock change does not stretch a window). Kept while the action's effect can still last.
	private static readonly List<(uint ActionId, ulong Target, long Tick)> _own = [];
	private static readonly HashSet<(ulong Source, uint ActionId, float Total)> _written = [];

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
			_own.Add((actionId, targetId, now));
		}
	}

	/// <summary>
	/// Seconds the player's own <paramref name="actionId"/>, gone out and aimed at him, still lasts by its
	/// duration from that moment - also before its status is on him; zero if it did not go out within its
	/// duration. <paramref name="anyTarget"/>: an effect around the player that lands on enemies (Reprisal),
	/// whose recorded target is not him.
	/// </summary>
	public static float OwnCoverLeft(uint actionId, bool anyTarget)
	{
		var player = Player.Object;
		var lasts = DefensiveValues.DurationOf(actionId);
		if (player == null || lasts <= 0f)
		{
			return 0f;
		}

		var now = Environment.TickCount64;
		var best = 0f;
		foreach (var (id, target, tick) in _own)
		{
			if (id == actionId && (anyTarget || target == player.GameObjectId))
			{
				best = Math.Max(best, lasts - SecondsSince(tick, now));
			}
		}

		return best;
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
			_written.Clear();
			_own.Clear();
			return;
		}

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
			var cast = new Cast(hostile, id, attackType, left, hostile.TotalCastTime, predicted,
				(player.CurrentHp / maxHp) + barrier, horizon);
			_casts.Add(cast);
			Trace(cast);
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

		// Only the casts still running keep their line, so the set does not grow through a fight.
		_ = _written.RemoveWhere(key => !_casts.Exists(c => c.Source.GameObjectId == key.Source
			&& c.ActionId == key.ActionId && c.Total == key.Total));

		PlayerImpervious = player.ImperviousThrough(_casts.Count == 0 ? gcd : _casts[0].Horizon);
	}

	// One line per cast, so the file shows what the plan was built on.
	private static void Trace(Cast cast)
	{
		if (!_written.Add((cast.Source.GameObjectId, cast.ActionId, cast.Total)))
		{
			return;
		}

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

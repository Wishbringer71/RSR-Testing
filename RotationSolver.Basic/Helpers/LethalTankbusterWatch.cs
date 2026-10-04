using ECommons.GameHelpers;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Whether a tankbuster now being cast at the player would kill him even at full HP and with every
/// mitigation he can still add before it lands - the owner's proposal of 04.10.2026 (concept 09,
/// "Unverwundbarkeit vor einem tödlichen Tankbuster").
/// </summary>
/// <remarks>
/// <para>Detection only: what is the case, under which figures. No option, no threshold of a rule is
/// read here. The decisions - whether the invulnerability is committed to a cast, and whether the other
/// defence is held - are taken in <c>CustomRotation</c> (<c>InvulnerabilityCommitted</c>,
/// <c>TankInvulnerabilityAbility</c>, <c>HoldDefenceForInvulnerability</c>) and, for healers, in
/// <c>StateUpdater.ShouldAddDefenseSingle</c>.</para>
///
/// <para>The verdict needs the action: only a cast names it. A marker or a BossModReborn prediction says
/// that a tankbuster comes, not which, so it gives no figure to judge by and no verdict. An action the
/// learned table (<see cref="TankbusterTable"/>) has not measured gets none either - no figure, no
/// invulnerability, the owner's condition.</para>
///
/// <para>Best case on purpose, by the owner's criterion "selbst bei 100% gesundheit und debuffs auf den
/// bossgegner und gezogener mitigation": full HP plus the barrier standing now, and every own mitigation
/// that is enabled, learned and ready by the hit, together with every one already standing and every
/// own barrier. Full HP rather than the HP now, because no heal - own or a healer's - can raise HP above
/// it: an omitted heal cannot turn the verdict. Errors in it - two mitigations that cannot both be used,
/// a reading that is too low - make the verdict "survivable" more often. The one error on the other
/// side is what other players add, a healer's mitigation or barrier on the tank: it is not counted,
/// because what other players do is an assumption and never a reason (concept 09).</para>
///
/// <para>Every cast at the player is judged, not only the one that lands first: a survivable hit ahead
/// of a lethal one must not hide it.</para>
/// </remarks>
internal static class LethalTankbusterWatch
{
	/// <summary>One tankbuster being cast at the player, and the verdict on it.</summary>
	internal sealed record Cast(IBattleChara Source, uint ActionId, float Remaining, float Total,
		float Predicted, float Standing, string Detail)
	{
		/// <summary>Measured at all: the table has a figure for the action.</summary>
		public bool Measured => Predicted > 0f;

		/// <summary>The best case takes full HP and the barrier standing now.</summary>
		public bool Lethal => Measured && Predicted >= Standing;
	}

	private static readonly List<Cast> _casts = [];
	private static readonly HashSet<(ulong Source, uint ActionId, float Total, bool Lethal)> _written = [];

	/// <summary>Every known tankbuster now being cast at the player, the soonest first.</summary>
	public static IReadOnlyList<Cast> Casts => _casts;

	/// <summary>The soonest cast whose verdict is lethal, or null.</summary>
	public static Cast? SoonestLethal { get; private set; }

	/// <summary>
	/// Whether the player stands under any invulnerability now - Hallowed Ground, Holmgang, Superbolide,
	/// Living Dead, Walking Dead or Undead Rebirth.
	/// </summary>
	public static bool PlayerUnderInvulnerability { get; private set; }

	/// <summary>
	/// Whether the coming hit cannot touch the player: Hallowed Ground or Superbolide lasts past the
	/// soonest tankbuster being cast at him (plus one GCD for the hit to arrive) or, with no cast to time
	/// it, for at least one more GCD - long enough to still press a mitigation once it runs low.
	/// </summary>
	public static bool PlayerImpervious { get; private set; }

	/// <summary>The cast of <paramref name="actionId"/> by <paramref name="source"/>, if it is still running.</summary>
	public static Cast? Find(ulong source, uint actionId)
	{
		foreach (var cast in _casts)
		{
			if (cast.Source.GameObjectId == source && cast.ActionId == actionId)
			{
				return cast;
			}
		}

		return null;
	}

	/// <summary>
	/// Whether an invulnerability on the player - any of <see cref="PlayerUnderInvulnerability"/> - is
	/// still up in <paramref name="seconds"/>, so that it covers a hit landing then.
	/// </summary>
	public static bool PlayerInvulnerableThrough(float seconds)
	{
		var player = Player.Object;
		return player != null && player.InvulnerableThrough(seconds);
	}

	/// <summary>Recomputes the verdicts. Called once per framework cycle, on the game thread.</summary>
	public static void Update()
	{
		_casts.Clear();
		SoonestLethal = null;
		PlayerUnderInvulnerability = false;
		PlayerImpervious = false;

		var player = Player.Object;
		if (!DataCenter.InCombat || player == null || player.IsDead || DataCenter.Role != JobRole.Tank)
		{
			_written.Clear();
			return;
		}

		PlayerUnderInvulnerability = player.InvulnerableThrough(0f);

		var maxHp = Math.Max(1u, player.MaxHp);
		var standing = 1f + ((float)player.GetObjectShield() / maxHp);
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
			var predicted = BestCaseShare(id, player, hostile, left, out var detail);
			var cast = new Cast(hostile, id, left, hostile.TotalCastTime, predicted, standing, detail);
			_casts.Add(cast);
			Trace(cast);
		}

		_casts.Sort((a, b) => a.Remaining.CompareTo(b.Remaining));
		foreach (var cast in _casts)
		{
			if (cast.Lethal)
			{
				SoonestLethal = cast;
				break;
			}
		}

		var gcd = DataCenter.DefaultGCDTotal;
		PlayerImpervious = player.ImperviousThrough(_casts.Count == 0 ? gcd : _casts[0].Remaining + gcd);
	}

	// One line per cast and verdict, so the file shows what the verdict was built on.
	private static void Trace(Cast cast)
	{
		if (!_written.Add((cast.Source.GameObjectId, cast.ActionId, cast.Total, cast.Lethal)))
		{
			return;
		}

		var name = Service.GetSheet<Lumina.Excel.Sheets.Action>().TryGetRow(cast.ActionId, out var row)
			? row.Name.ExtractText()
			: $"#{cast.ActionId}";
		DefenseTrace.Line($"tankbuster coming at you: {name} #{cast.ActionId} in {cast.Remaining:F1} s - "
			+ (!cast.Measured
				? "not in the tankbuster table, no verdict"
				: $"best case {cast.Predicted:P0} of max HP against full HP and barrier {cast.Standing:P0}"
					+ $" ({cast.Detail}): {(cast.Lethal ? "lethal" : "survivable")}"));
	}

	private static bool IsLearned(uint actionId)
	{
		lock (OtherConfiguration.TankbusterPotentialGate)
		{
			return OtherConfiguration.TankbusterPotential.ContainsKey(actionId);
		}
	}

	/// <summary>
	/// The share of maximum HP the hit takes in the best case: the table's prediction under the mitigation
	/// standing now, times every own mitigation that could still be added before the hit, less every own
	/// barrier that could still be put up. Zero when the action is not in the table.
	/// </summary>
	private static float BestCaseShare(uint actionId, IBattleChara player, IBattleChara caster, float remaining, out string detail)
	{
		detail = string.Empty;
		var standingNow = TankbusterTable.PredictedShare(actionId, player, caster);
		if (standingNow <= 0f)
		{
			return 0f;
		}

		var rotation = DataCenter.CurrentRotation;
		if (rotation == null)
		{
			detail = "no rotation";
			return standingNow;
		}

		var attackType = Service.GetSheet<Lumina.Excel.Sheets.Action>().TryGetRow(actionId, out var row)
			? row.AttackType.RowId
			: 0u;

		// One figure per action as the game casts it: Vengeance and Damnation, Raw Intuition and
		// Bloodwhetting are one button each, and counting both halves would add mitigation that cannot
		// be had.
		Dictionary<uint, (float Factor, float Barrier, string Name)> addable = [];
		foreach (var action in rotation.AllBaseActions)
		{
			if (action == null || !action.EnoughLevel || !action.Config.IsEnabled
				|| !action.Cooldown.WillHaveOneCharge(remaining))
			{
				continue;
			}

			var value = DefensiveValues.For(action.ID);
			var canSelf = action.Action.CanTargetSelf;
			var self = canSelf ? value.Self : 0f;
			var enemy = attackType is >= (uint)AttackType.Slashing and <= (uint)AttackType.Shot
				? value.EnemyPhysical
				: attackType == (uint)AttackType.Magic
					? value.EnemyMagical
					: Math.Min(value.EnemyPhysical, value.EnemyMagical);
			var barrier = canSelf ? value.Barrier : 0f;
			if (self <= 0f && enemy <= 0f && barrier <= 0f)
			{
				continue;
			}

			// Already standing: counted in the prediction above.
			if (Stands(action.ID, player, caster))
			{
				continue;
			}

			var factor = (1f - self) * (1f - enemy);
			var key = Service.GetAdjustedActionId(action.ID);
			addable[key] = addable.TryGetValue(key, out var known)
				? (Math.Min(factor, known.Factor), Math.Max(barrier, known.Barrier), known.Name)
				: (factor, barrier, action.Name);
		}

		var best = standingNow;
		var barriers = 0f;
		List<string> names = [];
		foreach (var (factor, barrier, name) in addable.Values)
		{
			best *= factor;
			barriers += barrier;
			names.Add(name);
		}

		best = Math.Max(0f, best - barriers);
		detail = names.Count == 0 ? $"now {standingNow:P0}, nothing left to add" : $"now {standingNow:P0}, with {string.Join(", ", names)}";

		// A share of zero would read as "not in the table"; a best case that absorbs everything is
		// survivable, which the smallest positive figure keeps saying.
		return best > 0f ? best : float.Epsilon;
	}

	private static bool Stands(uint actionId, IBattleChara player, IBattleChara caster)
	{
		if (!DefensiveValues.MitigatingStatusesByActionId.TryGetValue(actionId, out var statuses))
		{
			return false;
		}

		foreach (var status in statuses)
		{
			if (player.HasStatus(false, (StatusID)status) || caster.HasStatus(false, (StatusID)status))
			{
				return true;
			}
		}

		return false;
	}
}

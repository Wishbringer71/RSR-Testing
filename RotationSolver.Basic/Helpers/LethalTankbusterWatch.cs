using ECommons.GameHelpers;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Whether a tankbuster now being cast at the player would kill him even at full HP and with every
/// mitigation he can still add before it lands - the owner's proposal of 04.10.2026 (concept 09,
/// "Unverwundbarkeit vor einem tödlichen Tankbuster").
/// </summary>
/// <remarks>
/// <para>Detection only: what is the case, under which figures. The consumers decide - the
/// invulnerability (<c>CustomRotation.TankInvulnerabilityAbility</c>) and the hold on every other
/// single defence (<c>StateUpdater.ShouldAddDefenseSingle</c>).</para>
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
/// <para>Bound per cast: once a cast is judged lethal while the invulnerability is ready, the verdict
/// stands until that cast is over and one GCD more for the hit to arrive. A barrier arriving later
/// cannot take it back, because the hold on the other mitigations was already given on its strength -
/// an invulnerability that then did not come would leave the player with less than either path. The
/// hold itself opens again in the last GCD before the hit if the invulnerability has still not gone
/// out (<see cref="HoldMitigation"/>), so a failed draw falls back to the mitigation.</para>
/// </remarks>
internal static class LethalTankbusterWatch
{
	private sealed class Committed
	{
		public required ulong Source { get; init; }
		public required uint ActionId { get; init; }
		public required DateTime Ends { get; init; }
	}

	private static Committed? _committed;
	private enum Verdict
	{
		NotMeasured,
		Survivable,
		LethalNoInvulnerability,
		LethalOptionOff,
		Committed,
	}

	private static (ulong Source, uint ActionId, float Total, Verdict Verdict) _written;

	/// <summary>The tankbuster being cast at the player, if one is, or null.</summary>
	public static IBattleChara? Source { get; private set; }

	/// <summary>Seconds until that cast lands.</summary>
	public static float Remaining { get; private set; }

	/// <summary>
	/// Whether the invulnerability is committed to the cast: judged lethal in the best case, with the
	/// invulnerability ready at the time. Holds until the cast is over.
	/// </summary>
	public static bool InvulnerabilityCommitted { get; private set; }

	/// <summary>
	/// Whether the other single mitigations are held for the committed invulnerability: committed, and
	/// either the invulnerability already stands or the hit is more than one GCD away - so that, if the
	/// draw has failed by then, the last GCD before the hit is still open for a mitigation.
	/// </summary>
	public static bool HoldMitigation { get; private set; }

	/// <summary>
	/// Whether the player stands under any invulnerability now - Hallowed Ground, Holmgang, Superbolide,
	/// Living Dead, Walking Dead or Undead Rebirth -, so that a second one is not drawn.
	/// </summary>
	public static bool PlayerUnderInvulnerability { get; private set; }

	/// <summary>
	/// Whether the coming hit cannot touch the player: Hallowed Ground or Superbolide lasts past the
	/// tankbuster being cast at him (plus one GCD for the hit to arrive) or, with no cast to time it,
	/// for at least one more GCD - long enough to still press a mitigation once it runs low.
	/// </summary>
	public static bool PlayerImpervious { get; private set; }

	/// <summary>Recomputes the verdict. Called once per framework cycle, on the game thread.</summary>
	public static void Update()
	{
		Source = null;
		Remaining = 0f;
		InvulnerabilityCommitted = false;
		PlayerUnderInvulnerability = false;
		PlayerImpervious = false;
		HoldMitigation = false;

		var player = Player.Object;
		if (!DataCenter.InCombat || player == null || player.IsDead || DataCenter.Role != JobRole.Tank)
		{
			_committed = null;
			return;
		}

		PlayerUnderInvulnerability = player.HasStatus(false, StatusHelper.NoNeedHealingStatus)
			|| player.HasStatus(false, StatusID.WalkingDead);

		IBattleChara? caster = null;
		uint actionId = 0;
		var remaining = 0f;
		var total = 0f;
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
			if (caster == null || left < remaining)
			{
				caster = hostile;
				actionId = id;
				remaining = left;
				total = hostile.TotalCastTime;
			}
		}

		var gcd = DataCenter.DefaultGCDTotal;
		PlayerImpervious = player.ImperviousThrough(caster == null ? gcd : remaining + gcd);

		if (caster == null)
		{
			// The binding outlives the cast by the one GCD the hit needs to arrive, so the hold does
			// not open in the moment between cast end and hit.
			if (_committed is { } after && DateTime.Now <= after.Ends)
			{
				InvulnerabilityCommitted = true;
				HoldMitigation = PlayerUnderInvulnerability;
			}
			else
			{
				_committed = null;
			}

			return;
		}

		Source = caster;
		Remaining = remaining;

		if (_committed is { } bound && bound.Source == caster.GameObjectId && bound.ActionId == actionId
			&& DateTime.Now <= bound.Ends)
		{
			InvulnerabilityCommitted = true;
			HoldMitigation = PlayerUnderInvulnerability || remaining > gcd;
			return;
		}

		_committed = null;
		var predicted = BestCaseShare(actionId, player, caster, remaining, out var detail);
		var maxHp = Math.Max(1u, player.MaxHp);
		var standing = 1f + ((float)player.GetObjectShield() / maxHp);
		var lethal = predicted > 0f && predicted >= standing;
		// Committed only when the consumer will act on it: with the option off, a commitment would hold
		// the other mitigation for an invulnerability that never comes.
		var enabled = Service.Config.InvulnerabilityBeforeLethalTankbuster;
		var ready = InvulnerabilityReady(out var invulnerability);

		if (lethal && ready && enabled)
		{
			_committed = new Committed
			{
				Source = caster.GameObjectId,
				ActionId = actionId,
				Ends = DateTime.Now + TimeSpan.FromSeconds(remaining + gcd),
			};
			InvulnerabilityCommitted = true;
			HoldMitigation = PlayerUnderInvulnerability || remaining > gcd;
		}

		// One line per cast and verdict, so the file shows what the verdict was built on - and a second
		// one when the invulnerability comes off cooldown during the cast and the verdict turns.
		var verdict = predicted <= 0f ? Verdict.NotMeasured : !lethal ? Verdict.Survivable
			: !ready ? Verdict.LethalNoInvulnerability : enabled ? Verdict.Committed : Verdict.LethalOptionOff;
		var key = (caster.GameObjectId, actionId, total, verdict);
		if (_written != key)
		{
			_written = key;
			var name = Service.GetSheet<Lumina.Excel.Sheets.Action>().TryGetRow(actionId, out var row)
				? row.Name.ExtractText()
				: $"#{actionId}";
			DefenseTrace.Line($"tankbuster coming at you: {name} #{actionId} in {remaining:F1} s - "
				+ (predicted <= 0f
					? "not in the tankbuster table, no verdict"
					: $"best case {predicted:P0} of max HP against full HP and barrier {standing:P0} ({detail}): "
						+ (verdict switch
						{
							Verdict.Survivable => "survivable",
							Verdict.Committed => $"lethal, {invulnerability!.Name} committed",
							Verdict.LethalOptionOff => $"lethal, {invulnerability!.Name} ready, but the option is off",
							_ => "lethal, but no invulnerability is ready",
						})));
		}
	}

	/// <summary>
	/// Whether the tank's invulnerability can be drawn before the hit: one RSR would use at all (enabled,
	/// learned, its threshold HealthForDyingTanks above zero) and off cooldown.
	/// </summary>
	public static bool InvulnerabilityReady(out IBaseAction? invulnerability)
	{
		invulnerability = DataCenter.CurrentRotation?.TankInvulnerability;
		return invulnerability != null && invulnerability.Config.IsEnabled && invulnerability.EnoughLevel
			&& Service.Config.HealthForDyingTanks > 0f && invulnerability.Cooldown.HasOneCharge;
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

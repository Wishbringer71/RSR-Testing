using ECommons.DalamudServices;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// The learned tankbuster table (<see cref="OtherConfiguration.TankbusterPotential"/>): what a tankbuster
/// does unmitigated, measured on every player it is seen to hit, and what it is expected to do to a
/// given target now. The owner's request of 04.10.2026 (concept 13, "Die Tankbuster-Tabelle").
/// </summary>
/// <remarks>
/// <para>Whether a tankbuster kills cannot be known ahead of the hit from any public source (A258);
/// it can be learned. A hit is scaled back to unmitigated by the mitigation that stood at that moment
/// - on the one hit, and on the attacker by damage type - with the figures the actions' own effect
/// texts state (<see cref="DefensiveValues.MitigationByStatusId"/>). A well-mitigated hit therefore
/// does not rate the action low, which is the weakness of the area table's raw observation.</para>
///
/// <para>What is left out errs on the low side, never on the high one: a mitigation no effect text
/// rates (a trait, a status not tied to an action) is not divided out, so the unmitigated figure is
/// at most underestimated, and every later hit can only raise it. A barrier is not divided out
/// either: whether the game's damage figure counts the part a barrier absorbed is not established
/// here. An underestimate means a defensive that does not fire - the behaviour before the table.</para>
///
/// <para>Detection only: nothing here decides. Readers are the tank swap
/// (<see cref="TankSwapWatch"/>) for "would a repeat kill", and <see cref="TankbusterForecast"/> for
/// "what does the coming hit take".</para>
/// </remarks>
internal static class TankbusterTable
{
	/// <summary>
	/// A tankbuster from <paramref name="attacker"/> reached <paramref name="target"/>, a player, for
	/// <paramref name="share"/> of the target's maximum HP. Called from the effect handler, on the game
	/// thread. <paramref name="appliedByHit"/>: the statuses this very hit put on the target - a
	/// vulnerability among them came with the hit and did not raise it, so it does not count as one the
	/// target carried (whether the game has already applied it when the effect arrives is not
	/// established).
	/// </summary>
	public static void Record(uint actionId, string name, IBattleChara target, IBattleChara? attacker, float share,
		List<uint> appliedByHit)
	{
		if (share <= 0f)
		{
			// Nothing arrived: a hit turned away by invulnerability or evasion, or wholly absorbed. Zero
			// says that something stopped it, not that the action is harmless.
			return;
		}

		// A strengthened attacker hits harder than the action does: stored, the figure would rate the
		// action too high, and the tankbuster plan would draw the invulnerability, or stack mitigation,
		// where less would have done. Not stored - the one error the table must not make (concept 09, "Das geringste Mittel gegen einen gemessenen Tankbuster").
		if (attacker != null && attacker.CarriesDamageUp())
		{
			DefenseTrace.Line($"tankbuster measured: {name} #{actionId} on {target.Name.TextValue} for {share:P0} of max HP"
				+ " - not stored, the attacker carried a damage-up status");
			return;
		}

		var factor = MitigationFactor(actionId, target, attacker);
		if (factor <= 0f)
		{
			return;
		}

		var unmitigated = share / factor;
		var vulnerable = CarriedVulnerability(target, appliedByHit);

		bool raised;
		lock (OtherConfiguration.TankbusterPotentialGate)
		{
			if (!OtherConfiguration.TankbusterPotential.TryGetValue(actionId, out var reading))
			{
				reading = new TankbusterReading();
			}

			var before = vulnerable ? reading.UnderVulnerability : reading.Unmitigated;
			raised = unmitigated > before;
			if (raised)
			{
				var updated = new TankbusterReading
				{
					Unmitigated = vulnerable ? reading.Unmitigated : unmitigated,
					UnderVulnerability = vulnerable ? unmitigated : reading.UnderVulnerability,
				};
				OtherConfiguration.TankbusterPotential[actionId] = updated;
			}
		}

		DefenseTrace.Line($"tankbuster measured: {name} #{actionId} on {target.Name.TextValue} for {share:P0} of max HP,"
			+ $" mitigation factor {factor:F2}, unmitigated {unmitigated:P0}{(vulnerable ? " under a vulnerability" : string.Empty)}"
			+ (raised ? " - stored" : " - not above the stored value"));

		if (raised)
		{
			_ = OtherConfiguration.SaveTankbusterPotential();
		}
	}

	/// <summary>
	/// The share of <paramref name="target"/>'s maximum HP the tankbuster is expected to take: its
	/// stored unmitigated figure (the one under vulnerability when the target carries one and it is
	/// known) times the mitigation standing now and still standing in <paramref name="horizon"/>
	/// seconds - whoever put it there, the player, a co-tank's Reprisal, a healer. Zero when the action
	/// has not been measured.
	/// </summary>
	public static float PredictedShare(uint actionId, IBattleChara target, IBattleChara? attacker, float horizon = 0f)
	{
		TankbusterReading? reading;
		lock (OtherConfiguration.TankbusterPotentialGate)
		{
			_ = OtherConfiguration.TankbusterPotential.TryGetValue(actionId, out reading);
		}

		if (reading == null)
		{
			return 0f;
		}

		var unmitigated = target.CarriesVulnerabilityUp() && reading.UnderVulnerability > 0f
			? reading.UnderVulnerability
			: reading.Unmitigated;
		return unmitigated * MitigationFactor(actionId, target, attacker, horizon);
	}

	/// <summary>
	/// The share of a hit that gets through the mitigation standing now: on the target, and on the
	/// attacker by the action's damage type. Where the type is neither physical nor magical, the smaller
	/// of the attacker's two reductions counts - the reading errs low rather than high. With a
	/// <paramref name="horizon"/>, a status that runs out before it does not count.
	/// </summary>
	public static float MitigationFactor(uint actionId, IBattleChara target, IBattleChara? attacker, float horizon = 0f)
	{
		var factor = 1f;
		foreach (var status in StatusesOf(target, horizon))
		{
			if (DefensiveValues.MitigationByStatusId.TryGetValue(status, out var value) && value.Self > 0f)
			{
				factor *= 1f - value.Self;
			}
		}

		if (attacker != null)
		{
			var attackType = Service.GetSheet<Lumina.Excel.Sheets.Action>().TryGetRow(actionId, out var row)
				? row.AttackType.RowId
				: 0u;
			foreach (var status in StatusesOf(attacker, horizon))
			{
				if (!DefensiveValues.MitigationByStatusId.TryGetValue(status, out var value))
				{
					continue;
				}

				var reduction = attackType is >= (uint)AttackType.Slashing and <= (uint)AttackType.Shot
					? value.EnemyPhysical
					: attackType == (uint)AttackType.Magic
						? value.EnemyMagical
						: Math.Min(value.EnemyPhysical, value.EnemyMagical);
				if (reduction > 0f)
				{
					factor *= 1f - reduction;
				}
			}
		}

		return factor;
	}

	private static bool CarriedVulnerability(IBattleChara target, List<uint> appliedByHit)
	{
		foreach (var status in StatusesOf(target))
		{
			if (StatusHelper.IsVulnerabilityUp(status) && !appliedByHit.Contains(status))
			{
				return true;
			}
		}

		return false;
	}

	// A remaining time of zero or less is a status without an end; it stands at any horizon.
	private static List<uint> StatusesOf(IBattleChara battleChara, float horizon = 0f)
	{
		List<uint> ids = [];
		var statuses = battleChara.StatusList;
		if (statuses == null)
		{
			return ids;
		}

		foreach (var status in statuses)
		{
			if (status != null && status.StatusId != 0
				&& (horizon <= 0f || status.RemainingTime <= 0f || status.RemainingTime >= horizon))
			{
				ids.Add(status.StatusId);
			}
		}

		return ids;
	}
}

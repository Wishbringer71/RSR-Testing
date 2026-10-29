using ECommons.DalamudServices;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// The learned tankbuster table (<see cref="OtherConfiguration.TankbusterPotential"/>): what a tankbuster
/// does unmitigated, measured on every tank it is seen to hit, and what it is expected to do to a
/// given target now. The owner's request of 04.10.2026 (concept 13, "Die Tankbuster-Tabelle").
/// </summary>
/// <remarks>
/// <para>Whether a tankbuster kills cannot be known ahead of the hit from any public source (A258);
/// it can be learned. A hit is scaled back to unmitigated by the mitigation that stood at that moment
/// - on the one hit, and on the attacker by damage type - with the figures the actions' own effect
/// texts state, or a trait where the text leaves the figure blank (Troubadour), given only to a status
/// whose own text says it reduces damage (<see cref="DefensiveValues.TryStatus"/>). A well-mitigated hit
/// therefore does not rate the action low, which is the weakness of the area table's raw observation.</para>
///
/// <para>What is left out errs on the low side, never on the high one: a mitigation no text rates
/// (a status not tied to an action, a trait figure below the trait's level) is not divided out, so the
/// unmitigated figure is at most underestimated, and every later hit can only raise it; and a status
/// that only shares an action's name (its heal over time) carries no figure, so nothing is divided out
/// twice. A barrier needs no dividing out: the game's damage figure carries the part it absorbed (his ACT
/// logs of 26.10.2026, A449). An underestimate means a defensive that does not fire - the behaviour before
/// the table.</para>
///
/// <para>Detection only: nothing here decides. Readers are the tank swap
/// (<see cref="TankSwapWatch"/>) for "would a repeat kill", <see cref="TankbusterForecast"/> for
/// "what does the coming hit take", and <see cref="DataCenter.AimedTankbusterLeaves"/> for the same question about
/// another member, whom help from others is for (A373).</para>
/// </remarks>
internal static class TankbusterTable
{
	/// <summary>
	/// Whether <paramref name="actionId"/> is on the area list: it has hit every party member at least once, so it is a
	/// raidwide, whatever the table holds for it. Until A337 a raidwide landing on a marked tank while the marker stood
	/// was stored here as the marker's tankbuster (Feldgeschrei, Wehklagen); the file keeps such entries, since a save
	/// merges with it, and every reader passes them over through this.
	/// </summary>
	internal static bool IsPartyWide(uint actionId) => OtherConfiguration.HostileCastingArea.Contains(actionId);

	// A cast taken for a tankbuster only by the fallback of DataCenter.IsHostileCastingTank: its name and attack type, the
	// tank it is aimed at and when its bar ends. Keyed by caster and action, so a second cast of the same enemy does not
	// push out the first before its hit lands; kept until a GCD after the bar - the hit comes after it.
	private readonly record struct AimedCast(string Name, uint AttackType, ulong TargetId, DateTime EndsAt);

	private static readonly Dictionary<(ulong Caster, uint Action), AimedCast> _aimedCasts = [];
	private static readonly List<(ulong Caster, uint Action)> _aimedDone = [];

	/// <summary>
	/// Follows the casts that only the fallback of <see cref="DataCenter.IsHostileCastingTank"/> takes for a tankbuster:
	/// a targetable enemy - not a helper, whose casts count only when listed - casting at its own target, a tank,
	/// uninterruptibly and for longer than a GCD. Neither list nor marker names them, so their hits went unmeasured, and
	/// plan and invulnerability, which read only listed or learned casts, never took a lethal one: it killed at every
	/// repeat (his first test case of 29.10.2026, concept 13, A452). With every target update, for every role - the table
	/// learns from every tank it sees hit; with RSR off and no auto-on, the targets are not updated and nothing is followed.
	/// </summary>
	internal static void UpdateAimedCasts()
	{
		var casters = DataCenter.AreaCasters;
		if (!DataCenter.InCombat || casters == null)
		{
			_aimedCasts.Clear();
			return;
		}

		var now = DateTime.Now;
		for (var i = 0; i < casters.Count; i++)
		{
			var h = casters[i];
			if (h == null || !h.IsCasting || DataCenter.AreaCastHelperIds.Contains(h.GameObjectId)
				|| OtherConfiguration.HostileCastingTank.Contains(h.CastActionId) || IsPartyWide(h.CastActionId)
				|| !DataCenter.IsHostileCastingTank(h)
				|| Svc.Objects.SearchById(h.CastTargetObjectId) is not Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter aimedAt
				|| !aimedAt.IsJobCategory(JobRole.Tank))
			{
				continue;
			}

			var key = (h.GameObjectId, h.CastActionId);
			var endsAt = now.AddSeconds(Math.Max(0f, h.TotalCastTime - h.CurrentCastTime));
			if (_aimedCasts.TryGetValue(key, out var known) && known.TargetId == aimedAt.GameObjectId)
			{
				_aimedCasts[key] = known with { EndsAt = endsAt };
				continue;
			}

			var row = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(h.CastActionId);
			_aimedCasts[key] = new AimedCast(row?.Name.ExtractText() ?? string.Empty, row?.AttackType.RowId ?? 0,
				aimedAt.GameObjectId, endsAt);
		}

		_aimedDone.Clear();
		var gcd = DataCenter.DefaultGCDTotal;
		foreach (var (key, cast) in _aimedCasts)
		{
			if (now > cast.EndsAt.AddSeconds(gcd))
			{
				_aimedDone.Add(key);
			}
		}

		foreach (var key in _aimedDone)
		{
			_ = _aimedCasts.Remove(key);
		}
	}

	/// <summary>Forgets the casts followed by <see cref="UpdateAimedCasts"/>, with the rest of the target lists.</summary>
	internal static void ClearAimedCasts() => _aimedCasts.Clear();

	/// <summary>
	/// The id a hit from <paramref name="sourceId"/> on <paramref name="targetId"/> is learned under when it is the
	/// landing of a cast followed by <see cref="UpdateAimedCasts"/>, no later than a GCD after its bar: the cast's own,
	/// which the forecast reads. The hit carries that id, or one of the same name and attack type - a cast can land as a
	/// neighbouring row (Valfodr, cast #43319, hit #43320, in his log of 26.10.2026; 84 of 85 casts of over 2.5 s at a
	/// player there landed under their own id), and the attack type decides which mitigation is divided out of the
	/// reading and laid on again in the forecast. Zero when it is no such landing.
	/// </summary>
	internal static uint AimedCastLanding(ulong sourceId, ulong targetId, Lumina.Excel.Sheets.Action hit)
	{
		if (_aimedCasts.Count == 0)
		{
			return 0u;
		}

		var latest = DateTime.Now.AddSeconds(-DataCenter.DefaultGCDTotal);
		if (_aimedCasts.TryGetValue((sourceId, hit.RowId), out var own) && own.TargetId == targetId && own.EndsAt >= latest)
		{
			return hit.RowId;
		}

		string? name = null;
		foreach (var (key, cast) in _aimedCasts)
		{
			if (key.Caster == sourceId && cast.TargetId == targetId && cast.EndsAt >= latest && cast.Name.Length > 0
				&& cast.AttackType == hit.AttackType.RowId && cast.Name == (name ??= hit.Name.ExtractText()))
			{
				return key.Action;
			}
		}

		return 0u;
	}

	/// <summary>
	/// A tankbuster from <paramref name="attacker"/> reached <paramref name="target"/>, a player, for
	/// <paramref name="share"/> of the target's maximum HP. Called from the effect handler, on the game
	/// thread. <paramref name="appliedByHit"/>: the statuses this very hit put on the target - a
	/// vulnerability among them came with the hit and did not raise it, so it does not count as one the
	/// target carried (whether the game has already applied it when the effect arrives is not
	/// established).
	/// </summary>
	/// <param name="storedAs">The id the reading is kept under when it is not <paramref name="actionId"/>: the cast whose
	/// landing this is (<see cref="AimedCastLanding"/>). Zero keeps it under the hit's own id.</param>
	public static void Record(uint actionId, string name, IBattleChara target, IBattleChara? attacker, float share,
		List<uint> appliedByHit, uint storedAs = 0)
	{
		var key = storedAs != 0 ? storedAs : actionId;
		var via = storedAs != 0 ? ", known only as a cast at its own target" : string.Empty;
		if (share <= 0f)
		{
			// Nothing arrived: a hit turned away by invulnerability or evasion. Zero says that something stopped it,
			// not that the action is harmless. A barrier makes no zero: the figure carries what it absorbed (A449).
			return;
		}

		// Every reader rates the hit for a tank - the player's plan and the tank swap. A member who is not a tank lacks
		// the tank trait's reduction, which no status carries, and has a smaller pool, so his share would raise the
		// figure for every tank, and a failed mechanic on him could take it past his whole health (A312, as A298 for
		// the area table). Not stored; the line stays for the trace.
		if (!target.IsJobCategory(JobRole.Tank))
		{
			DefenseTrace.Line($"tankbuster measured: {name} #{key} on {target.Name.TextValue} (not a tank) for {share:P0}"
				+ $" of max HP{via} - not stored, the table rates hits on tanks");
			return;
		}

		// A strengthened attacker hits harder than the action does: stored, the figure would rate the
		// action too high, and the tankbuster plan would draw the invulnerability, or stack mitigation,
		// where less would have done. Not stored - the one error the table must not make (concept 09, "Gemessener Tankbuster: das geringste Mittel").
		if (attacker != null && attacker.CarriesDamageUp())
		{
			DefenseTrace.Line($"tankbuster measured: {name} #{key} on {target.Name.TextValue} for {share:P0} of max HP{via}"
				+ " - not stored, the attacker carried a damage-up status");
			return;
		}

		// Above the level the content syncs to, the hit is a smaller share than in sync: stored, it would rate the action
		// too small for the next synced run, where the plan reads it to spend less (A452).
		if (DataCenter.AboveContentLevel)
		{
			DefenseTrace.Line($"tankbuster measured: {name} #{key} on {target.Name.TextValue} for {share:P0} of max HP{via}"
				+ " - not stored, above the level the content syncs to");
			return;
		}

		var factor = MitigationFactor(actionId, target, attacker);
		if (factor <= 0f)
		{
			return;
		}

		// A share of a raised maximum (Thrill of Battle, Great Nebula, Protraction) is a share of the larger
		// pool; scaled back to the unraised maximum, as every plan reads it (concept 13).
		var unmitigated = share * (1f + MaxHpRaise(target)) / factor;
		var vulnerable = CarriedVulnerability(target, appliedByHit);

		bool raised;
		bool changed;
		lock (OtherConfiguration.TankbusterPotentialGate)
		{
			var known = OtherConfiguration.TankbusterPotential.TryGetValue(key, out var reading);
			reading ??= new TankbusterReading();

			// Aimed only while every landing came through a cast at its target; one under list or marker ends it.
			var aimedOnly = storedAs != 0 && (!known || reading.AimedOnly);
			var before = vulnerable ? reading.UnderVulnerability : reading.Unmitigated;
			raised = unmitigated > before;
			changed = raised || (known && reading.AimedOnly != aimedOnly);
			if (changed)
			{
				var updated = new TankbusterReading
				{
					Unmitigated = vulnerable || !raised ? reading.Unmitigated : unmitigated,
					UnderVulnerability = vulnerable && raised ? unmitigated : reading.UnderVulnerability,
					AimedOnly = aimedOnly,
				};
				OtherConfiguration.TankbusterPotential[key] = updated;
			}
		}

		DefenseTrace.Line($"tankbuster measured: {name} #{key} on {target.Name.TextValue} for {share:P0} of max HP{via},"
			+ $" mitigation factor {factor:F2}, unmitigated {unmitigated:P0}{(vulnerable ? " under a vulnerability" : string.Empty)}"
			+ (raised ? " - stored" : changed ? " - not above the stored value, known as a tankbuster from now on"
				: " - not above the stored value"));

		if (changed)
		{
			_ = OtherConfiguration.SaveTankbusterPotential();
		}
	}

	/// <summary>
	/// The share of <paramref name="target"/>'s maximum HP the tankbuster is expected to take: its
	/// stored unmitigated figure (the one under vulnerability when the target carries one and it is
	/// known) times the mitigation standing now and still standing in <paramref name="horizon"/>
	/// seconds - whoever put it there, the player, a co-tank's Reprisal, a healer - as a share of the
	/// maximum HP the target has now: a raise of the maximum that still stands then makes the same hit a
	/// smaller share. Zero when the action has not been measured.
	/// </summary>
	public static float PredictedShare(uint actionId, IBattleChara target, IBattleChara? attacker, float horizon = 0f)
	{
		if (IsPartyWide(actionId))
		{
			return 0f;
		}

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
		return unmitigated * MitigationFactor(actionId, target, attacker, horizon) / (1f + MaxHpRaise(target, horizon));
	}

	/// <summary>
	/// By how much the target's maximum HP is raised (0.2 for +20 %) by statuses standing now and, with a
	/// <paramref name="horizon"/>, still standing then - read from the effect texts of the actions that
	/// put them there (<see cref="DefensiveValue.MaxHp"/>). Raises multiply.
	/// </summary>
	public static float MaxHpRaise(IBattleChara target, float horizon = 0f)
	{
		var raised = 1f;
		foreach (var status in StatusesOf(target, horizon))
		{
			if (DefensiveValues.TryStatus(status, out var value) && value.MaxHp > 0f)
			{
				raised *= 1f + value.MaxHp;
			}
		}

		return raised - 1f;
	}

	/// <summary>
	/// The share of a hit that gets through the mitigation standing now: on the target, and on the
	/// attacker by the action's damage type. Where the type is neither physical nor magical, the smaller
	/// of the attacker's two reductions counts - the reading errs low rather than high. With a
	/// <paramref name="horizon"/>, a status that runs out before it does not count.
	/// </summary>
	public static float MitigationFactor(uint actionId, IBattleChara target, IBattleChara? attacker, float horizon = 0f)
	{
		var attackType = Service.GetSheet<Lumina.Excel.Sheets.Action>().TryGetRow(actionId, out var row)
			? row.AttackType.RowId
			: 0u;

		// On the target: its own mitigation, together with a split one for this damage type (Dark Mind).
		var factor = 1f;
		foreach (var status in StatusesOf(target, horizon))
		{
			if (DefensiveValues.TryStatus(status, out var value))
			{
				var reduction = value.SelfAgainst(attackType);
				if (reduction > 0f)
				{
					factor *= 1f - reduction;
				}
			}
		}

		if (attacker != null)
		{
			foreach (var status in StatusesOf(attacker, horizon))
			{
				if (!DefensiveValues.TryStatus(status, out var value))
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

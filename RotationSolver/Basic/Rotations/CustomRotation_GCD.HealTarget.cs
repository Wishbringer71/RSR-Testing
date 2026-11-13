using ECommons.ExcelServices;
using ECommons.Logging;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Rotations;

public partial class CustomRotation
{
	// The probe of the downtime heal (concept 07, "Die Füllheilung"): a line per heal with its target, the target's
	// forecast health and the line, so an upload shows each one - the Praetorium case had to be read from ACT.
	// The GCD path asks every frame until the heal goes out; one line per action and target within a GCD.
	private static IAction? TraceDowntimeHeal(IAction? act, float line)
	{
		var target = (act as IBaseAction)?.Target.Target;
		var key = (act?.ID ?? 0u, target?.GameObjectId ?? 0ul);
		var now = DateTime.Now;
		if (key != _downtimeHealTraced || (now - _downtimeHealTracedAt).TotalSeconds >= DataCenter.DefaultGCDTotal)
		{
			_downtimeHealTraced = key;
			_downtimeHealTracedAt = now;
			var health = target?.GetForecastHealthRatio(false);
			DefenseTrace.Line($"downtime heal -> {act?.Name ?? "?"} #{key.Item1}"
				+ (target != null ? $" on {target.Name.TextValue} at {health:P0}" : string.Empty)
				+ $", line {line:P0}");
		}

		return act;
	}

	private static (uint Action, ulong Target) _downtimeHealTraced;

	private static bool CanCastHeals => !StatusHelper.PlayerHealingPunished();

	/// <summary>
	/// Is the rotation still going to spend Swiftcast on the raise?
	///
	/// The hard cast branches used to ask <c>SwiftcastPvE.Cooldown.IsCoolingDown</c>, which answers
	/// a different question and leaves a state they cannot get out of. For a healer this rotation
	/// spends Swiftcast on the raise path alone - the two other triggers in CustomRotation_Ability
	/// are gated on JobRole.RangedMagical - so with RaisePlayerBySwift off it is never spent, never
	/// enters recovery, the hard cast branch never fires, and nobody is raised at all. The setting
	/// only promises not to spend Swiftcast on raises, not to stop raising.
	///
	/// With the setting on, `!SwiftcastComingForRaise` equals the old `IsCoolingDown` in every
	/// combination; it differs only for setting-off with Swiftcast ready, which is exactly the
	/// state that raised nobody.
	///
	/// <c>EnoughLevel</c> closes the same hole one level down. <c>IsCoolingDown</c> reads the recast
	/// timer (<see cref="ActionIdHelper.IsCoolingDown"/>), and an action the player cannot cast at
	/// all has no timer running - so a raiser below Swiftcast's level, or synced under it in older
	/// content, reported "Swiftcast is ready" forever. With the setting on that shut every hard cast
	/// branch while the Swiftcast path itself failed on CanUse, and nobody was raised by either.
	/// Raise comes well before Swiftcast, so the gap is a real span of levels rather than a corner
	/// case. At full level EnoughLevel is true and nothing changes.
	/// </summary>
	private bool SwiftcastComingForRaise =>
		Service.Config.RaisePlayerBySwift
		&& SwiftcastPvE.EnoughLevel
		&& !SwiftcastPvE.Cooldown.IsCoolingDown;

	/// <summary>
	/// For the two modes that promise "cooldown is higher than raise cast time": is starting the
	/// hard cast now better than waiting for Swiftcast?
	///
	/// It is when the rotation is not spending Swiftcast on raises at all, when Swiftcast is not
	/// available to this player in the first place (same reason as in
	/// <see cref="SwiftcastComingForRaise"/>), or when the wait is longer than the cast itself.
	/// This cannot simply read <c>!SwiftcastComingForRaise</c>: that is true for any running
	/// cooldown and would drop the very comparison these two modes exist for.
	///
	/// It lives here rather than four times in the dispatch because the four copies are the same
	/// decision - the previous gap was in all four of them at once.
	/// </summary>
	private bool HardCastBeatsWaitingForSwiftcast =>
		!Service.Config.RaisePlayerBySwift
		|| !SwiftcastPvE.EnoughLevel
		|| (SwiftcastPvE.Cooldown.IsCoolingDown && Raise != null
			&& Raise.Info.CastTime < SwiftcastPvE.Cooldown.RecastTimeRemainOneCharge);

	/// <summary>
	/// Whether a raise takes the Swiftcast or Dualcast already running. With "Raise with Swiftcast or Dualcast" on, always.
	/// Off, the setting's text holds and the next spells spend it - until it would run out unused: when the next GCD is
	/// its last chance, it goes into the raise. The owner's rule (03.11.2026): what would be wasted is used first, as the
	/// bell and Lux Solaris are used before they run out (A504).
	/// </summary>
	protected bool RunningSwiftGoesToRaise => Service.Config.RaisePlayerBySwift
		|| (HasSwift && StatusHelper.PlayerStatusTime(true, StatusHelper.SwiftcastStatus) < WeaponRemain + WeaponTotal);

	/// <summary>
	/// A heal over time on a party member whom an enemy's damage over time alone takes below the single-heal threshold
	/// before a heal over time laid now would run out - the owner's proposal of 27.10.2026 as the loop left it (concept
	/// 08, "Schaden über Zeit: HoT gegen DoT", A432). Healers only, with the job's own heals over time
	/// (<see cref="HotAgainstDotByJob"/>); who needs one, and the measurement behind it, is DotOnParty's.
	/// </summary>
	private bool HotAgainstDot(out IAction? act)
	{
		act = null;
		// The master switch and the heal stop of the automatic heals hold here as for the heal flags (StateUpdater).
		return Service.Config.HotAgainstDot && Service.Config.AutoHeal && !DataCenter.IsPvP && DataCenter.InCombat
			&& DataCenter.Role == JobRole.Healer && !DataCenter.IsTyrantCastingSpecialIndicator() && DotOnParty.AnyDot
			&& HotAgainstDotByJob(out act);
	}

	/// <summary>
	/// Whether the job has, at its level, a heal over time for the party - the answer to a damage over time on several
	/// members, which a healer then does not cleanse one by one (concept 07, "Reinigung oder Heilung", A565).
	/// </summary>
	internal virtual bool HasGroupHealOverTime => false;

	/// <summary>
	/// <paramref name="group"/> where more than one member within its reach needs a heal over time against an enemy's
	/// damage over time - a single one covers exactly one member, so from the second on the group one saves a GCD -,
	/// else <paramref name="single"/> on the lowest of them who does not carry it yet. Each is asked with its own
	/// duration, read from its effect text.
	/// </summary>
	/// <remarks>
	/// Both are GCD heals and ask the rotation's own permission for them (<see cref="CanHealAreaSpell"/>,
	/// <see cref="CanHealSingleSpell"/>) - with "Use GCDs to heal" off, a healer leaves them to the other healer.
	/// </remarks>
	/// <param name="single">The job's heal over time on one target.</param>
	/// <param name="group">The job's heal over time on the party, or null.</param>
	/// <param name="act">The action to use.</param>
	/// <returns>True when one of them goes.</returns>
	protected bool TryHotAgainstDot(IBaseAction single, IBaseAction? group, out IAction? act)
	{
		act = null;
		if (group is { EnoughLevel: true } && CanHealAreaSpell)
		{
			NeedHotAgainstDot(DefensiveValues.DurationOf(group.ID), group.Info.EffectRange);
			if (DataCenter.HotAgainstDotMembers.Count > 1 && group.CanUse(out act, skipAoeCheck: true))
			{
				TraceHotAgainstDot(group, null);
				return true;
			}
		}

		if (!single.EnoughLevel || !CanHealSingleSpell)
		{
			return false;
		}

		// No reach of its own here: the target choice keeps to the members the action reaches.
		NeedHotAgainstDot(DefensiveValues.DurationOf(single.ID), float.MaxValue);
		if (DataCenter.HotAgainstDotMembers.Count == 0 || !single.CanUse(out act, targetOverride: TargetType.HotAgainstDot))
		{
			return false;
		}

		TraceHotAgainstDot(single, single.Target.Target);
		return true;
	}

	private static (uint Action, ulong Target, DateTime At) _hotAgainstDotTraced;

	// The decision over what DotOnParty sees coming: whom it takes below his single-heal threshold from above it - with
	// the exclusions and the protected threshold of the single-heal decision; one already below is the heal thresholds'
	// case, and "damage over time alone" is no longer what takes him there -, within reach, and not one whose damage over
	// time Esuna is about to clear. Lowest first, into DataCenter.HotAgainstDotMembers for the target choice.
	private static void NeedHotAgainstDot(float hotSeconds, float range)
	{
		_hotAgainstDotThresholds.Clear();
		List<IBattleChara> members = [];
		var normal = Service.Config.HealthSingleSpell;
		var esuna = DataCenter.MergedStatus.HasFlag(AutoStatus.Dispel) ? DataCenter.DispelTarget : null;
		foreach (var (member, low) in DotOnParty.Project(hotSeconds))
		{
			if (member.DistanceToPlayer() > range || !member.TryHealSingleThreshold(normal, out var threshold) || low >= threshold
				|| member.GetHealthRatio() < threshold
				|| (esuna?.GameObjectId == member.GameObjectId && DotOnParty.OnlyDispellable(member)))
			{
				continue;
			}

			members.Add(member);
			_hotAgainstDotThresholds[member.GameObjectId] = threshold;
		}

		DataCenter.HotAgainstDotMembers = members;
	}

	// One line in DefenseTrace.log per heal over time this path lays, on the target or for the party - at most once per
	// GCD for the same action and target, since the decision is asked on every update until it goes. The job stands
	// for the member, not his name.
	private static void TraceHotAgainstDot(IBaseAction action, IBattleChara? target)
	{
		var now = DateTime.Now;
		var targetId = target?.GameObjectId ?? 0;
		if (_hotAgainstDotTraced.Action == action.ID && _hotAgainstDotTraced.Target == targetId
			&& (now - _hotAgainstDotTraced.At).TotalSeconds < DataCenter.DefaultGCDTotal)
		{
			return;
		}

		_hotAgainstDotTraced = (action.ID, targetId, now);
		var text = new System.Text.StringBuilder($"heal over time against damage over time: {action.Name}");
		foreach (var member in DataCenter.HotAgainstDotMembers)
		{
			if ((target != null && member.GameObjectId != target.GameObjectId) || !DotOnParty.TryWeighed(member, out var w))
			{
				continue;
			}

			_ = text.Append($" | {DotOnParty.JobOf(member)} at {member.CurrentHp / (float)member.MaxHp:P0},"
				+ $" {w.Tick:F0} HP per tick ({(w.Measured ? "measured" : "learned in this area")}),"
				+ $" {w.Ticks} tick(s) within {w.Window:F1} s, heals over time on him {w.HotTick} HP x {w.HotTicks:F0},"
				+ $" lowest ahead {w.Low:P0} under {_hotAgainstDotThresholds.GetValueOrDefault(member.GameObjectId):P0}");
		}

		DefenseTrace.Line(text.ToString());
	}
}

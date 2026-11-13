using ECommons.DalamudServices;
using ECommons.ExcelServices;

namespace RotationSolver.Basic.Rotations;

public partial class CustomRotation
{
	/// <summary>
	/// A heal ability ahead of a due GCD for a party member in the critical class (concept 07, "Die Notfallheilung
	/// zwischen zwei Zaubern"). After a hard cast the time left to the GCD is its recast less its cast, at most half a
	/// second for every 2.0-s heal at any spell speed, and the locks in Ability and DoAction keep that half second free:
	/// a healer chaining Cure II never reached Benediction while the tank fell to 43 HP (01.11.2026, A478). Reported
	/// instead of the GCD, the ability is pressed the moment the GCD comes free - DoAction lets abilities go at none
	/// left - and the GCD follows after its animation lock. Which ability and on whom stays with the job's heal paths
	/// and their own conditions; it goes ahead only if it reaches a member in the critical class, because the delayed
	/// GCD is worth it only for the one who would otherwise fall.
	/// </summary>
	private bool EmergencyHealAheadOfGcd(IAction nextGCD, out IAction? act)
	{
		act = null;
		if (DataCenter.IsPvP)
		{
			return false;
		}

		_criticalMembers.Clear();
		foreach (var member in DataCenter.PartyMembers)
		{
			if (member != null && !member.IsDead && member.MaxHp > 0 && member.IsInCriticalClass())
			{
				_criticalMembers.Add(member);
			}
		}

		if (_criticalMembers.Count == 0)
		{
			return false;
		}

		IBaseAction.TargetOverride = TargetType.Heal;
		IBaseAction.HealPathRunning = true;
		try
		{
			// The area heal first, as in the heal path; one that misses the member leaves the single heal its turn.
			if ((AutoHealAreaAbility(nextGCD, out act) && act is IBaseAction area && ReachedCriticalMember(area) is { } byArea
					&& TraceEmergencyHeal(area, byArea, nextGCD))
				|| (AutoHealSingleAbility(nextGCD, out act) && act is IBaseAction single && ReachedCriticalMember(single) is { } bySingle
					&& TraceEmergencyHeal(single, bySingle, nextGCD)))
			{
				return true;
			}
		}
		finally
		{
			IBaseAction.TargetOverride = null;
			IBaseAction.HealPathRunning = false;
			IBaseAction.AutoHealCheck = false;
		}

		act = null;
		foreach (var member in _criticalMembers)
		{
			// A531: for the player, what each potion in the bag waits for - the configuration the upload lacks.
			TraceEmergencyHeal(null, member, nextGCD, member.GameObjectId == Player?.GameObjectId ? DescribeHpPotionsHeld() : null);
		}
		return false;
	}

	/// <summary>
	/// The member in the critical class this heal reaches, as its target or among those an area heal covers.
	/// </summary>
	private static IBattleChara? ReachedCriticalMember(IBaseAction heal)
	{
		var result = heal.Target;
		foreach (var member in _criticalMembers)
		{
			if (result.Target?.GameObjectId == member.GameObjectId)
			{
				return member;
			}

			if (result.AffectedTargets == null)
			{
				continue;
			}

			foreach (var affected in result.AffectedTargets)
			{
				if (affected?.GameObjectId == member.GameObjectId)
				{
					return member;
				}
			}
		}

		return null;
	}

	private static readonly Dictionary<(uint Action, ulong Target), DateTime> _emergencyHealTraced = [];

	private static readonly List<(uint Action, ulong Target)> _staleTraceKeys = [];

	/// <summary>
	/// Whether this job's <see cref="InterruptAbility"/> override already gates the same shared
	/// role-fallback interrupt action (e.g. LegSweep) with its own combo-safety condition. When true,
	/// <see cref="MyInterruptAbility"/> skips its generic per-role fallback for that action instead of
	/// retrying it ungated after the job's own gate declined it.
	/// </summary>
	protected virtual bool HasOwnInterruptGate => false;

	/// <summary>
	/// Whether this job's <see cref="AntiKnockbackAbility"/> override already gates the same shared
	/// role-fallback anti-knockback action (e.g. Arm's Length) with its own combo-safety condition.
	/// When true, <see cref="AntiKnockback"/> skips its generic per-role fallback for that action
	/// instead of retrying it ungated after the job's own gate declined it.
	/// </summary>
	protected virtual bool HasOwnAntiKnockbackGate => false;

	/// <summary>
	/// Whether this rotation's <see cref="DefenseSingleAbility"/> already runs
	/// <see cref="ArmsLengthSlowsPull"/> with its own option and holds. When true, the single defence
	/// does not retry Arm's Length as its last resort, which would cast it past a declined option or a
	/// hold such as the Dark Knight's barrier.
	/// </summary>
	protected virtual bool HasOwnArmsLengthPullRule => false;

	/// <summary>
	/// Swiftcast is held for this job's raise: the raise and Swiftcast are learned at the current level, and the player
	/// raises with Swiftcast ("Raise with Swiftcast or Dualcast"). The owner's rule (A503): damage comes far more often
	/// than a raise, so a Swiftcast spent on damage is rarely there when someone dies.
	/// </summary>
	protected bool SwiftcastKeptForRaise => Service.Config.RaisePlayerBySwift && Raise is { EnoughLevel: true } && SwiftcastPvE.EnoughLevel;

	/// <summary>
	/// Whether damage may take Swiftcast: where nothing is held for a raise, or where the player chose it explicitly.
	/// A rotation with its own damage switch for Swiftcast adds that switch here; every damage use of Swiftcast asks
	/// this, not <see cref="SwiftcastKeptForRaise"/> (A503).
	/// </summary>
	protected virtual bool SwiftcastForDamage => !SwiftcastKeptForRaise;

	/// <summary>
	/// Is a raise waiting, and would it actually go out once the cast time is gone?
	///
	/// Both halves matter. Without the pending check Swiftcast would be spent whenever it happens
	/// to be ready; without the castable check it would be spent on a raise that then fails on MP,
	/// range or level, and the charge is gone for nothing.
	///
	/// The target override is not decoration. Raise actions declare no target type of their own -
	/// they are merely IsFriendly - and their target comes from TargetType.Death, which only the
	/// GCD path sets. Asking CanUse here without it would search the wrong set, and CanUse assigns
	/// Target as a side effect, so a stray call would also leave the action pointing somewhere
	/// else. The previous value is restored rather than cleared, because this runs inside the
	/// ability dispatcher, which sets overrides of its own around its branches.
	/// </summary>
	private bool RaisePendingAndCastable()
	{
		if (Raise == null || !DataCenter.MergedStatus.HasFlag(AutoStatus.Raise) || HasSwift)
		{
			return false;
		}

		var previousOverride = IBaseAction.TargetOverride;
		IBaseAction.TargetOverride = TargetType.Death;
		try
		{
			return Raise.CanUse(out _);
		}
		finally
		{
			IBaseAction.TargetOverride = previousOverride;
		}
	}

	/// <summary>
	/// The tank's own invulnerability - Hallowed Ground, Holmgang, Living Dead, Superbolide - for the
	/// rules that need to know whether it can still save him. Null for every other job.
	/// </summary>
	protected virtual IBaseAction? Invulnerability => null;
}

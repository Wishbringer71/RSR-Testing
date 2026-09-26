using System.ComponentModel;

namespace RotationSolver.RebornRotations.Tank;

[Rotation("Reborn", CombatType.PvE, GameVersion = "7.55")]
[SourceCode(Path = "main/RebornRotations/Tank/DRK_Reborn.cs")]

public sealed class DRK_Reborn : DarkKnightRotation
{
	#region Config Options
	[RotationConfig(CombatType.PvE, Name = "Use provoke in opening if tank stance is on")]
	public bool UseProvokeInOpening { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Keep at least 3000 MP")]
	public bool TheBlackestNight { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use The Blackest Night on lowest HP party member during AOE scenarios")]
	public bool BlackLantern { get; set; } = false;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Target health threshold needed to use Blackest Night with above option", Parent = nameof(BlackLantern))]
	private float BlackLanternRatio { get; set; } = 0.5f;

	[RotationConfig(CombatType.PvE, Name = "Use Oblation on lowest HP party member during AOE scenarios")]
	public bool OblationLantern { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Use Oblation last stack of Oblation for party members", Parent = nameof(OblationLantern))]
	public bool OblationLanternStack { get; set; } = false;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Target health threshold needed to use Oblation with above option", Parent = nameof(OblationLantern))]
	private float OblationLanternRatio { get; set; } = 0.5f;

	[RotationConfig(CombatType.PvE, Name = "When to use The Blackest Night on yourself",
		Tooltip = "The Blackest Night costs 3000 MP and only pays it back as Dark Arts if the barrier "
			+ "is absorbed in full. The trigger that opens the defensive path is much weaker than that "
			+ "- two enemies in melee range, or any uninterruptible cast aimed at you - so on the "
			+ "default setting the barrier often goes up where nothing is about to spend it.\n"
			+ "Whenever single-target defences open: the old behaviour, barrier on every opening.\n"
			+ "Tankbuster or a big pull with no other mitigation: only where the hit is actually large, "
			+ "or where the stream is unthrottled and will spend the barrier. A big mitigation running, "
			+ "a stun chain, a slowed pack or a spent Reprisal all block it, because each of them drops "
			+ "the stream below the rate that spends the barrier.\n"
			+ "As above, plus below the health threshold: adds an emergency case that deliberately "
			+ "ignores those conditions.\n"
			+ "In a fight: on the narrower options you go into small and already-mitigated pulls "
			+ "without the barrier and keep the 3000 MP, and the barrier is there with its Dark Arts "
			+ "return for the hits that would otherwise land unabsorbed.\n"
			+ "The two values below belong to this setting: the enemy count applies to both of the "
			+ "narrower options, the health threshold only to the last one.")]
	public BlackestNightStrategy BlackestNightUsage { get; set; } = BlackestNightStrategy.WheneverDefensesOpen;

	public enum BlackestNightStrategy : byte
	{
		[Description("Whenever single-target defences open")]
		WheneverDefensesOpen,

		[Description("Tankbuster, or a big pull with no other mitigation running")]
		TankbusterOrHeavyPull,

		[Description("As above, plus below the health threshold")]
		TankbusterHeavyPullOrLowHealth,
	}

	[Range(1, 8, ConfigUnitType.None, 1)]
	[RotationConfig(CombatType.PvE, Name = "Hostiles that make a pull big enough for The Blackest Night",
		Tooltip = "How many enemies have to be in range before a pull counts as big enough to spend the "
			+ "barrier on. Applies to both of the narrower options of the setting above, and to neither "
			+ "on the default one.\n"
			+ "In a fight: the barrier is worth its 3000 MP when enough enemies are hitting you to "
			+ "absorb it in full within its duration. Lower: the barrier goes up on small groups that "
			+ "cannot spend it, and the MP is gone without the Dark Arts return. Higher: you tank a "
			+ "mid-sized pack without it, relying on Rampart and Reprisal instead.")]
	private int BlackestNightMinHostiles { get; set; } = 4;

	[RotationConfig(CombatType.PvE, Name = "Use Arm's Length on a pull for its Slow",
		Tooltip = "Arm's Length is used on a group pull for its Slow, not only as knockback "
			+ "protection - which was the only way the plugin ever used it.\n"
			+ "In a fight: the Slow +20% lands on every enemy that strikes you and delays "
			+ "auto-attacks as well as casts, so in a standing pack it throttles the whole incoming "
			+ "stream for fifteen seconds. It costs nothing but its own cooldown.\n"
			+ "It also feeds the decision above: a pull already throttled by this Slow no longer counts "
			+ "as unmitigated, so The Blackest Night is not spent into a stream that has been thinned. "
			+ "Off by default, because it changes what the action is used for.")]
	private bool UseArmsLengthOnPull { get; set; } = false;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Health threshold for The Blackest Night",
		Tooltip = "Only used by the last option of the Blackest Night setting above: below this share "
			+ "of your health the barrier goes up as an emergency, skipping every condition about pull "
			+ "size and running mitigation.\n"
			+ "In a fight: this is the case where the barrier is not an investment but a stopgap - it "
			+ "absorbs the next hits whether or not the MP is ever repaid. Higher: it fires earlier and "
			+ "more often, and the Dark Arts return becomes less likely. Lower: it is kept for genuine "
			+ "emergencies, at the risk of arriving after the hit that mattered.")]
	private float BlackestNightHealthRatio { get; set; } = 0.6f;

	[RotationConfig(CombatType.PvE, Name = "Opener action")]
	public OpenerActionStrategy OpenerActionUsage { get; set; } = OpenerActionStrategy.Unmend;

	public enum OpenerActionStrategy : byte
	{
		[Description("Use Unmend")]
		Unmend,

		[Description("Use Shadowstride")]
		Shadowstride,
	}
	#endregion

	#region Countdown Logic
	// Countdown logic to prepare for combat.
	// Includes logic for using Provoke, tank stances, and burst medicines.
	protected override IAction? CountDownAction(float remainTime)
	{
		//Provoke when has Shield.
		if (UseProvokeInOpening && remainTime <= CountDownAhead)
		{
			if (HasTankStance)
			{
				if (ProvokePvE.CanUse(out _))
				{
					return ProvokePvE;
				}
			}
		}

		if (remainTime < 1f && UseBurstMedicine(out var act))
		{
			return act;
		}

		if (remainTime <= 3f && TheBlackestNightPvE.CanUse(out act))
		{
			return act;
		}

		if (OpenerActionUsage == OpenerActionStrategy.Unmend)
		{
			if (remainTime <= 1f && UnmendPvE.CanUse(out act))
			{
				return act;
			}
		}

		if (OpenerActionUsage == OpenerActionStrategy.Shadowstride)
		{
			if (remainTime <= 1f && ShadowstridePvE.CanUse(out act))
			{
				return act;
			}
		}

		return base.CountDownAction(remainTime);
	}
	#endregion

	#region oGCD Logic
	// Decision-making for emergency abilities, focusing on Blood Weapon usage.
	protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
	{
		if (CombatElapsedLessGCD(3) && (IsLastAction(false, UnmendPvE) || IsLastAction(false, HardSlashPvE)))
		{
			if (EdgeOfShadowPvE.CanUse(out act))
			{
				return true;
			}
		}

		return base.EmergencyAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.ShadowstridePvE)]
	protected override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
	{
		if (ShadowstridePvE.CanUse(out act))
		{
			return true;
		}
		return base.MoveForwardAbility(nextGCD, out act);
	}

	protected override bool HealSingleAbility(IAction nextGCD, out IAction? act)
	{

		return base.HealSingleAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.DarkMissionaryPvE, ActionID.ReprisalPvE)]
	protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
	{
		// BlackLantern was declared and named as the threshold's parent but never read, so this
		// branch ran regardless of the switch - and the UI hides the threshold while the switch is
		// off, leaving the only adjustment invisible. The Oblation line below checks its own option,
		// and ChurinDRK checks this one on the same branch.
		// No burst hold on this line or the Oblation one below: their setting text ("Use ... on lowest
		// HP party member", with its own health threshold) names no exception for the burst, and the
		// setting text binds (A157).
		if (BlackLantern && TheBlackestNightPvE.CanUse(out act, targetOverride: TargetType.LowHP) && !TheBlackestNightPvE.Target.Target.HasStatus(false, StatusID.Transcendent) && TheBlackestNightPvE.Target.Target.GetHealthRatio() <= BlackLanternRatio)
		{
			return true;
		}

		if (!IsLastAbility(false, OblationPvE))
		{
			if (OblationLantern && OblationPvE.CanUse(out act, usedUp: OblationLanternStack, targetOverride: TargetType.LowHP) && !OblationPvE.Target.Target.HasStatus(false, StatusID.Transcendent) && OblationPvE.Target.Target.GetHealthRatio() <= OblationLanternRatio)
			{
				return true;
			}
		}

		// Dark knight special rule: the burst window keeps its weave slots and MP for damage. On the
		// universal layer, so it yields when the party is in danger (concept 08, "Die Abwehrsperren").
		var burstHold = HoldAreaDefense(InTwoMIsBurst, "Dark Knight: burst window");

		if (!burstHold && DarkMissionaryPvE.CanUse(out act))
		{
			return true;
		}

		// Held while a barrier waits to be spent - see HoldMitigationForBarrier. Reprisal takes 10%
		// off the stream that has to break The Blackest Night within its seven seconds.
		if (!burstHold && !HoldMitigationForBarrier(true)
			&& ShouldSustainMitigationDebuff(StatusHelper.ReprisalStatus)
			&& ReprisalPvE.CanUse(out act, skipAoeCheck: true, skipStatusProvideCheck: true))
		{
			return true;
		}

		if (!burstHold && !HoldMitigationForBarrier(true) && ReprisalPvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		if (!IsLastAbility(false, OblationPvE))
		{
			if (!burstHold && OblationPvE.CanUse(out act, skipStatusProvideCheck: false, targetOverride: TargetType.Self))
			{
				return true;
			}
		}

		return base.DefenseAreaAbility(nextGCD, out act);
	}

	/// <summary>
	/// Whether the self-cast of The Blackest Night is wanted in the current situation. See
	/// docs/rotation-flow/10-drk-blackest-night.md: the barrier repays its 3000 MP as Dark Arts only
	/// when it is absorbed in full, which takes 25% of maximum HP in 7 seconds - roughly 3.6% per
	/// second, and more than that once another mitigation has cut the incoming damage.
	/// <para>
	/// Two situations reach that rate, and they want opposite handling. Against a tankbuster the
	/// hit clears the threshold even under Shadow Wall, so mitigating alongside costs nothing and
	/// stacking is right. In a wall-to-wall pull the damage is a stream, and stacking wastes
	/// coverage that should be staggered - so there the barrier goes up only while no big
	/// mitigation is running, the same stagger Shadow Wall and Shadowed Vigil already keep against
	/// each other through StatusProvide.
	/// </para>
	/// </summary>
	/// <summary>
	/// How long after an area stun the hold continues, so the gap between two casts of Sanctus does
	/// not open a window. Roughly one global cooldown.
	/// </summary>
	private const float StunChainGrace = 3f;

	private static DateTime _lastGroupStunSeen = DateTime.MinValue;

	/// <summary>
	/// Whether an <b>area</b> stun is currently keeping the damage stream down.
	/// <para>
	/// Neither "any enemy is stunned" nor "every enemy is stunned" answers that. The first counts
	/// Low Blow - which this job carries itself and the interrupt path uses - where one enemy stops
	/// and the rest of the pull keeps hitting. The second fails as soon as a single straggler joins
	/// the pack unstunned, although the stream is plainly interrupted. What separates the two cases
	/// is the share: an area stun catches the pack, a single-target stun catches one of it. The rule
	/// therefore asks for at least two stunned enemies and at least half of those in range.
	/// </para>
	/// <para>
	/// The radius is the job's own reach, the same set of enemies the hostile count is measured
	/// over. Both conditions answer one question - is damage still arriving on me - so they have to
	/// look at the same enemies; a wider radius would count enemies that are not hitting anyone.
	/// </para>
	/// <para>
	/// Between two casts of Sanctus the stun lapses for about a global cooldown, so the hold
	/// continues through that gap while the enemies can still be stunned. Once they carry stun
	/// resistance there is no headroom left and the hold ends by itself - "wait until the stuns stop
	/// working" needs no counter of its own.
	/// </para>
	/// </summary>
	private bool GroupStunRunning()
	{
		var inRange = SurveyStuns(DataCenter.JobRange, out var stunned, out _, out var headroom);
		if (inRange == 0)
		{
			return false;
		}

		if (stunned >= 2 && stunned * 2 >= inRange)
		{
			_lastGroupStunSeen = DateTime.Now;
			return true;
		}

		return headroom && (DateTime.Now - _lastGroupStunSeen).TotalSeconds < StunChainGrace;
	}

	/// <summary>
	/// Whether the pack is slowed hard enough that the barrier would not be spent.
	/// </summary>
	/// <remarks>
	/// Slow is not only a caster debuff: its effect text names the auto-attack delay alongside cast
	/// and recast time, and trash enemies deal most of their damage by auto-attack. A slowed pack
	/// therefore throttles the incoming stream by about the size of the debuff - Arm's Length applies
	/// Slow +20% to every physical attacker for 15s, which is the same order as Rampart and past the
	/// line where the stream no longer spends 25% of maximum HP in seven seconds.
	///
	/// Same share rule as the stun condition, and for the same reason: one slowed enemy out of eight
	/// says nothing about the stream. The difference is the timing - a stun stops the stream and
	/// lapses in seconds, a slow thins it for fifteen, so this one has no grace window. It ends when
	/// the debuff does.
	/// </remarks>
	private static bool PackSlowed()
	{
		var inRange = SurveyHostileStatus(DataCenter.JobRange, StatusHelper.SlowStatus, out var slowed);
		return inRange > 0 && slowed >= 2 && slowed * 2 >= inRange;
	}

	/// <summary>
	/// Hold a damage mitigation because a barrier is waiting to be spent - group pulls only.
	/// </summary>
	/// <remarks>
	/// The Blackest Night absorbs 25% of maximum HP over 7s and grants Dark Arts only when that
	/// barrier is broken; a barrier that expires unbroken has cost 3000 MP and returned nothing but
	/// the absorption. What decides it is the size of the incoming stream, so every mitigation cast
	/// while the barrier stands works against it: Reprisal takes 10% off every enemy's damage,
	/// Arm's Length slows every physical attacker by 20% for 15s. The user's instruction is to hold
	/// both while the barrier is up.
	///
	/// <para>
	/// <b>Group pulls only, by the user's own qualification - in a boss fight the rule does not
	/// apply.</b> There the incoming damage arrives as scripted single hits rather than as a stream
	/// of auto-attacks: a tankbuster breaks the barrier on its own whatever Reprisal does, so
	/// holding the mitigation would give up real damage reduction for a reward that is not at risk.
	/// The pull is recognised by the same hostile count the barrier itself requires
	/// (<see cref="BlackestNightMinHostiles"/>) rather than a second number that could drift away
	/// from it.
	/// </para>
	///
	/// <para>
	/// Healing is deliberately not part of this. A barrier absorbs damage before it reaches HP, so
	/// the bearer's health does not change how fast it is spent - the only coupling runs the other
	/// way, since a bearer who dies first gets no Dark Arts at all.
	/// </para>
	///
	/// <para>
	/// The whole party is asked, not just the player: the barrier can be placed on any member
	/// (<c>"self or target party member"</c>), and Reprisal thins the stream for whoever is being
	/// hit. <c>isFromSelf: false</c> for the same reason - a second dark knight's barrier is just
	/// as real as this one's. The one-GCD lead releases the hold before the barrier lapses, so a
	/// mitigation that can no longer affect the outcome is not held back for nothing.
	/// </para>
	///
	/// <para>
	/// The hold is a Dark Arts question, rung two of concept 09, and yields on the universal layer
	/// when a member is in danger (<see cref="CustomRotation.HoldAreaDefense"/>).
	/// </para>
	/// </remarks>
	private bool HoldMitigationForBarrier(bool area)
	{
		return area
			? HoldAreaDefense(BarrierWaitsToBeSpent(), "Dark Knight: a barrier waits to be spent")
			: HoldSingleDefense(BarrierWaitsToBeSpent(), "Dark Knight: a barrier waits to be spent");
	}

	private bool BarrierWaitsToBeSpent()
	{
		if (NumberOfHostilesInRange < BlackestNightMinHostiles)
		{
			return false;
		}

		var party = DataCenter.PartyMembers;
		if (party == null)
		{
			return false;
		}

		foreach (var member in party)
		{
			if (member == null)
			{
				continue;
			}

			if (member.HasStatus(false, StatusHelper.FullAbsorbRewardStatus)
				&& !member.WillStatusEndGCD(1, 0, false, StatusHelper.FullAbsorbRewardStatus))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Whether Arm's Length is worth spending on the pull rather than kept for a knockback.
	/// </summary>
	/// <remarks>
	/// The action has two effects and the plugin read only one of them: it nullifies knockback, and
	/// it afflicts every physical attacker with Slow +20% for 15s. The slow raises auto-attack delay
	/// as well as cast and recast time, and trash enemies deal most of their damage by auto-attack,
	/// so on a standing pack it throttles the incoming stream by about the size of the debuff - the
	/// same order as Rampart, for nothing but a 120s cooldown.
	///
	/// The obvious objection - it is the job's only knockback protection - does not hold where this
	/// rule fires: knockback is a boss mechanic, and a wall-to-wall pull has none. The hostile-count
	/// condition is what keeps the two apart, so it uses the same threshold as the barrier rather
	/// than a second number that could drift away from it.
	/// </remarks>
	private bool ShouldUseArmsLengthOnPull()
		=> UseArmsLengthOnPull
			&& NumberOfHostilesInRange >= BlackestNightMinHostiles
			&& !PackSlowed()
			// The slow is the point of casting it here, and it is exactly what a barrier waiting to
			// be spent cannot afford. Only this branch is held: the central anti-knockback uses of
			// Arm's Length stay untouched, because being thrown off a platform is not a damage
			// question.
			&& !HoldMitigationForBarrier(false);

	private bool ShouldUseBlackestNightOnSelf()
	{
		// The free mitigations first. Both cost nothing but their cooldown and sit at or near the
		// end of this path, so while The Blackest Night is off cooldown every opportunity goes to
		// the 3000 MP action and the cheap ones only land once it happens to be unavailable. In a
		// pull that ordering is backwards; against a tankbuster it does not matter, which is why
		// this only gates the pull branch.
		var reprisalDone = !ReprisalPvE.EnoughLevel
			|| ReprisalPvE.Cooldown.IsCoolingDown
			|| (HostileTarget?.HasStatus(false, StatusHelper.ReprisalStatus) ?? false);

		// Arm's Length is the second one. "Done" means the same thing: either it cannot be cast, or
		// it already is - a cooling-down Arm's Length is one that has been spent on this pull.
		var armsLengthDone = !UseArmsLengthOnPull
			|| !ArmsLengthPvE.EnoughLevel
			|| ArmsLengthPvE.Cooldown.IsCoolingDown;

		// In a pull the barrier has to stand on its own: a big mitigation running alongside drops
		// the damage stream below the rate that spends it, a stun chain stops the stream outright,
		// and a slowed pack thins it for as long as the debuff lasts.
		var staggeredHeavyPull = NumberOfHostilesInRange >= BlackestNightMinHostiles
			&& !HasMajorMitigation
			&& !GroupStunRunning()
			&& !PackSlowed()
			&& reprisalDone
			&& armsLengthDone;

		return BlackestNightUsage switch
		{
			BlackestNightStrategy.TankbusterOrHeavyPull => TankbusterOnMe || staggeredHeavyPull,
			// Low health is an emergency, so it deliberately skips the stagger condition.
			BlackestNightStrategy.TankbusterHeavyPullOrLowHealth =>
				TankbusterOnMe || staggeredHeavyPull
				|| Player?.GetHealthRatio() <= BlackestNightHealthRatio,
			_ => true,
		};
	}

	[RotationDesc(ActionID.OblationPvE, ActionID.ArmsLengthPvE, ActionID.TheBlackestNightPvE, ActionID.DarkMindPvE, ActionID.ShadowWallPvE, ActionID.ShadowedVigilPvE, ActionID.RampartPvE, ActionID.ReprisalPvE)]
	protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
	{
		//10
		if (!IsLastAbility(false, OblationPvE))
		{
			if (OblationPvE.CanUse(out act, usedUp: true, skipStatusProvideCheck: false, targetOverride: TargetType.Self))
			{
				return true;
			}
		}

		// Arm's Length before the barrier, for the same reason Reprisal goes before it: it costs
		// nothing but its cooldown. Its Slow +20% lands on every enemy that strikes, and the slow
		// delays auto-attacks as well as casts, so in a standing pack it throttles the whole stream
		// for 15s. The plugin only ever used this action as knockback protection.
		if (ShouldUseArmsLengthOnPull() && ArmsLengthPvE.CanUse(out act))
		{
			return true;
		}

		// The trigger that opens this path says "defending is warranted", not "damage worth 25% of
		// maximum HP is coming" - it fires on two enemies in melee range, or on any uninterruptible
		// cast aimed at its own target. Rampart and Reprisal cost nothing but their cooldown; this
		// costs 3000 MP and only repays it as Dark Arts when the barrier is absorbed in full.
		if (ShouldUseBlackestNightOnSelf() && TheBlackestNightPvE.CanUse(out act, targetOverride: TargetType.Self))
		{
			return true;
		}
		//20
		if (DarkMindPvE.CanUse(out act))
		{
			return true;
		}

		// Predicted tankbuster takes priority over the elapsed-time stagger below.
		if (BMRShouldRefreshBefore(BMRTankbusterIn, 15f, true, null, ShadowedVigilPvE.EnoughLevel ? StatusID.ShadowedVigil : StatusID.ShadowWall)
			&& (ShadowedVigilPvE.EnoughLevel ? ShadowedVigilPvE.CanUse(out act, skipStatusProvideCheck: true) : ShadowWallPvE.CanUse(out act, skipStatusProvideCheck: true)))
		{
			return true;
		}

		if (BMRShouldRefreshBefore(BMRTankbusterIn, 20f, true, null, StatusID.Rampart) && RampartPvE.CanUse(out act, skipStatusProvideCheck: true))
		{
			return true;
		}

		//30-40
		if ((!RampartPvE.Cooldown.IsCoolingDown || RampartPvE.Cooldown.ElapsedAfter(60)) && ShadowedVigilPvE.CanUse(out act) && ShadowedVigilPvE.EnoughLevel)
		{
			return true;
		}

		if ((!RampartPvE.Cooldown.IsCoolingDown || RampartPvE.Cooldown.ElapsedAfter(60)) && ShadowWallPvE.CanUse(out act) && !ShadowedVigilPvE.EnoughLevel)
		{
			return true;
		}

		//20
		if (!ShadowWallPvE.EnoughLevel)
		{
			if (RampartPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (ShadowWallPvE.EnoughLevel && !ShadowedVigilPvE.EnoughLevel)
		{
			if (ShadowWallPvE.Cooldown.IsCoolingDown && ShadowWallPvE.Cooldown.ElapsedAfter(30) && RampartPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (ShadowedVigilPvE.EnoughLevel)
		{
			if (ShadowedVigilPvE.Cooldown.IsCoolingDown && ShadowedVigilPvE.Cooldown.ElapsedAfter(30) && RampartPvE.CanUse(out act))
			{
				return true;
			}
		}

		// Same hold as in the area path, and for the same reason: on a group pull the barrier's
		// reward depends on the stream that Reprisal would thin. In a boss fight the condition is
		// false by the hostile count, so a tankbuster keeps its mitigation.
		if (!HoldMitigationForBarrier(false)
			&& ShouldSustainMitigationDebuff(StatusHelper.ReprisalStatus)
			&& ReprisalPvE.CanUse(out act, skipAoeCheck: true, skipStatusProvideCheck: true))
		{
			return true;
		}

		if (!HoldMitigationForBarrier(false) && ReprisalPvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		return base.DefenseSingleAbility(nextGCD, out act);
	}

	protected override bool AttackAbility(IAction nextGCD, out IAction? act)
	{
		if (IsBurst)
		{
			if (InCombat && (IsLastGCD(false, SouleaterPvE) || !CombatElapsedLessGCD(4)))
			{
				if (DeliriumPvE.CanUse(out act))
				{
					return true;
				}
			}

			if (!DeliriumPvE.EnoughLevel)
			{
				if (BloodWeaponPvE.CanUse(out act))
				{
					return true;
				}
			}
			if (InCombat && (IsLastGCD(false, HardSlashPvE) || !CombatElapsedLessGCD(3)))
			{
				if (LivingShadowPvE.CanUse(out act, skipAoeCheck: true))
				{
					return true;
				}
			}
		}

		if (CombatElapsedLessGCD(4))
		{
			return base.AttackAbility(nextGCD, out act);
		}

		if (CheckDarkSide)
		{
			if (FloodOfDarknessPvE.CanUse(out act))
			{
				return true;
			}

			if (EdgeOfDarknessPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (!IsMoving && SaltedEarthPvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		if (ShadowbringerPvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		if (NumberOfHostilesInRange >= 3 && AbyssalDrainPvE.CanUse(out act))
		{
			return true;
		}

		if (CarveAndSpitPvE.CanUse(out act))
		{
			return true;
		}

		if (InTwoMIsBurst)
		{
			if (ShadowbringerPvE.CanUse(out act, usedUp: true, skipAoeCheck: true))
			{
				return true;
			}
		}

		if (SaltAndDarknessPvE.CanUse(out act))
		{
			return true;
		}

		return base.AttackAbility(nextGCD, out act);
	}
	#endregion

	#region GCD Logic
	protected override bool GeneralGCD(out IAction? act)
	{
		if (CombatElapsedLessGCD(4) && !IsLastGCD(false, SouleaterPvE) && NumberOfHostilesInRange < 2)
		{
			if (!HasDelirium && SouleaterPvE.CanUse(out act))
			{
				return true;
			}

			if (!HasDelirium && SyphonStrikePvE.CanUse(out act))
			{
				return true;
			}

			if (!HasDelirium && HardSlashPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (DisesteemPvE.CanUse(out act, skipComboCheck: true, skipAoeCheck: true))
		{
			return true;
		}

		//AOE Delirium
		if (ImpalementPvE.CanUse(out act))
		{
			return true;
		}

		if (QuietusPvE.CanUse(out act))
		{
			return true;
		}

		// Single Target Delirium
		if (TorcleaverPvE.CanUse(out act, skipComboCheck: true))
		{
			return true;
		}

		if (ComeuppancePvE.CanUse(out act, skipComboCheck: true))
		{
			return true;
		}

		if (ScarletDeliriumPvE.CanUse(out act, skipComboCheck: true))
		{
			return true;
		}

		if (BloodspillerPvE.CanUse(out act, skipComboCheck: true))
		{
			return true;
		}

		//AOE
		if (StalwartSoulPvE.CanUse(out act, skipAoeCheck: true) && NumberOfHostilesInRange > 0)
		{
			return true;
		}

		if (UnleashPvE.CanUse(out act))
		{
			return true;
		}

		//Single Target
		if (!HasDelirium && SouleaterPvE.CanUse(out act))
		{
			return true;
		}

		if (!HasDelirium && SyphonStrikePvE.CanUse(out act))
		{
			return true;
		}

		if (!HasDelirium && HardSlashPvE.CanUse(out act))
		{
			return true;
		}

		if (UnmendPvE.CanUse(out act))
		{
			return true;
		}

		return base.GeneralGCD(out act);
	}
	#endregion

	#region Extra Methods
	// Indicates whether the Dark Knight can heal using a single ability.
	public override bool CanHealSingleAbility => false;

	// Logic to determine when to use blood-based abilities.
	private bool UseBlood
	{
		get
		{
			// Conditions based on player statuses and ability cooldowns.
			if (!DeliriumPvE.EnoughLevel || !LivingShadowPvE.EnoughLevel)
			{
				return true;
			}

			if (StatusHelper.PlayerHasStatus(true, StatusID.Delirium_3836))
			{
				return true;
			}

			if ((StatusHelper.PlayerHasStatus(true, StatusID.Delirium_1972) || StatusHelper.PlayerHasStatus(true, StatusID.Delirium_3836)) && LivingShadowPvE.Cooldown.IsCoolingDown)
			{
				return true;
			}

			return (DeliriumPvE.Cooldown.WillHaveOneChargeGCD(1) && !LivingShadowPvE.Cooldown.WillHaveOneChargeGCD(3)) || (Blood >= 90 && !LivingShadowPvE.Cooldown.WillHaveOneChargeGCD(1));
		}
	}
	// Determines if currently in a burst phase based on cooldowns of key abilities.
	private bool InTwoMIsBurst => BloodWeaponPvE.Cooldown.IsCoolingDown && DeliriumPvE.Cooldown.IsCoolingDown && ((LivingShadowPvE.Cooldown.IsCoolingDown && !LivingShadowPvE.Cooldown.ElapsedAfter(15)) || !LivingShadowPvE.EnoughLevel);

	// Manages DarkSide ability based on several conditions.
	private bool CheckDarkSide
	{
		get
		{
			if (DarkSideEndAfterGCD(3))
			{
				return true;
			}

			if ((InTwoMIsBurst && HasDarkArts) || (HasDarkArts && StatusHelper.PlayerHasStatus(true, StatusID.BlackestNight)) || (HasDarkArts && DarkSideEndAfterGCD(3)))
			{
				return true;
			}

			if (InTwoMIsBurst && BloodWeaponPvE.Cooldown.IsCoolingDown && LivingShadowPvE.Cooldown.IsCoolingDown && SaltedEarthPvE.Cooldown.IsCoolingDown && ShadowbringerPvE.Cooldown.CurrentCharges == 0 && CarveAndSpitPvE.Cooldown.IsCoolingDown)
			{
				return true;
			}

			return (!TheBlackestNight || CurrentMp >= 6000) && CurrentMp >= 8500;
		}
	}
	#endregion
}
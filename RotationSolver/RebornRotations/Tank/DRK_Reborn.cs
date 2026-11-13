using System.ComponentModel;

namespace RotationSolver.RebornRotations.Tank;

[Rotation("Reborn", CombatType.PvE, GameVersion = "7.57", RotationType = RotationType.Reborn)]
[SourceCode(Path = "main/RebornRotations/Tank/DRK_Reborn.cs")]

public sealed partial class DRK_Reborn : DarkKnightRotation
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

	[RotationConfig(CombatType.PvE, Name = "Use The Blackest Night on another party member a tankbuster is aimed at",
		Tooltip = "The Blackest Night goes on the member a tankbuster is aimed at - a tankbuster marker on them, or an "
			+ "enemy casting a tankbuster at them -, most often the other tank.\n"
			+ "In a fight: the barrier takes a quarter of their maximum HP off the tankbuster. A tankbuster nearly "
			+ "always breaks it, and a broken barrier returns its 3000 MP as Dark Arts. Your own setting for The "
			+ "Blackest Night on yourself goes first.")]
	public bool BlackestNightOnTankbusterTarget { get; set; } = true;

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
	public BlackestNightStrategy BlackestNightUsage { get; set; } = BlackestNightStrategy.TankbusterHeavyPullOrLowHealth;

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
			+ "stream for fifteen seconds. It costs nothing but its own cooldown. Bosses do not count "
			+ "towards the pull - Arm's Length does not soften the hit that strikes you, so a boss's "
			+ "tankbuster gains nothing from it.\n"
			+ "Not while BossModReborn announces a knockback that lands after the barrier has run out "
			+ "and before Arm's Length is ready again - the action is kept for it.\n"
			+ "It also feeds the decision above: a pull already throttled by this Slow no longer counts "
			+ "as unmitigated, so The Blackest Night is not spent into a stream that has been thinned.")]
	private bool UseArmsLengthOnPull { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "In a pull, use The Blackest Night only once the damage you take would break it",
		Tooltip = "The barrier pays its 3000 MP back as Dark Arts only when it is absorbed in full within its "
			+ "duration - its effect text gives the share of your maximum HP and the seconds. In a pull the barrier "
			+ "now also waits until the damage you took over that many seconds, after the mitigation that stood, "
			+ "reached that share. Tankbusters and the low-health case are not affected.\n"
			+ "In a fight: a pack that hits too softly to break the barrier no longer gets it, and keeps the MP for "
			+ "Edge of Shadow; a pack that does hit that hard gets it as before, a few seconds into the pull.")]
	private bool BlackestNightWaitsForStream { get; set; } = true;

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
		if (BlackLantern && CanUseOnLowestBelow(TheBlackestNightPvE, BlackLanternRatio, false, out act))
		{
			return true;
		}

		if (!IsLastAbility(false, OblationPvE))
		{
			if (OblationLantern && CanUseOnLowestBelow(OblationPvE, OblationLanternRatio, OblationLanternStack, out act))
			{
				return true;
			}
		}

		// Dark knight special rule: the burst window keeps its weave slots and MP for damage. On the
		// universal layer, so it yields when the party is in danger (concept 08, "Die Abwehrsperren").
		var burstHold = HoldAreaDefense(InTwoMIsBurst, "Dark Knight: burst window");

		// Tanks' stage: Reprisal and the party tool take turns over a run of area attacks (concept 09, rule 9); Oblation
		// and The Blackest Night are no party tools and stay out of it.
		var stretched = TankAreaDefenseStretched(DarkMissionaryPvE);

		if (!burstHold && !stretched && DarkMissionaryPvE.CanUse(out act))
		{
			return true;
		}

		// Held while a barrier waits to be spent - see HoldMitigationForBarrier. Reprisal takes 10%
		// off the stream that has to break The Blackest Night within its seven seconds.
		if (!burstHold && !stretched && !HoldMitigationForBarrier(true)
			&& ShouldSustainMitigationDebuff(StatusHelper.ReprisalStatus)
			&& ReprisalPvE.CanUse(out act, skipAoeCheck: true, skipStatusProvideCheck: true))
		{
			return true;
		}

		if (!burstHold && !stretched && !HoldMitigationForBarrier(true) && ReprisalPvE.CanUse(out act, skipAoeCheck: true))
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
	private static DateTime _lastGroupStunSeen = DateTime.MinValue;

	/// <summary>
	/// Whether an <b>area</b> stun is currently keeping the damage stream down, or the next one of a
	/// chain is visibly on its way.
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
	/// Between two casts of Holy the stun lapses for a moment. The hold carries across that gap
	/// only while the next stun can be seen coming (concept 10, A215): for one global cooldown after
	/// the stun ended - the time the white mage needs to begin the next Holy with his next GCD - and
	/// for as long as a party member is casting Holy or Holy III with this pack in its radius. A gap
	/// estimated from earlier gaps (A195, A212) held the barrier after a chain had ended, as long as
	/// the longest or the last pause - a raise, say - while the tank took the stream. Once the fight has
	/// shown the enemies immune (StunWatch) there is no headroom left and the hold ends by itself.
	/// </para>
	/// </summary>
	private static bool GroupStunRunning()
	{
		var inRange = SurveyStuns(DataCenter.JobRange, out var stunned, out _, out var headroom);
		if (inRange == 0)
		{
			return false;
		}

		var now = DateTime.Now;
		if (stunned >= 2 && stunned * 2 >= inRange)
		{
			_lastGroupStunSeen = now;
			return true;
		}

		if (!headroom)
		{
			return false;
		}

		return (now - _lastGroupStunSeen).TotalSeconds < DataCenter.DefaultGCDTotal
			|| AreaStunBeingCastOnPack(inRange);
	}

	// A party member casting an action that stuns in a radius around them (A560: a cast time, an effect radius, not aimed at
	// a spot, and "Stun Duration" in its text - today Holy and Holy III; Occult Falcon is instant and aimed at a spot, so its
	// stun is seen only on the enemies), whose radius holds this pack by the same share the stun itself is counted with. The
	// radius is the action's own, from the game data.
	private static bool AreaStunBeingCastOnPack(int inRange)
	{
		var sheet = Service.GetSheet<Lumina.Excel.Sheets.Action>();
		foreach (var member in DataCenter.PartyMembers)
		{
			if (member == null || !member.IsCasting || sheet.GetRowOrDefault(member.CastActionId) is not { } action
				|| !StunsAroundCaster(action))
			{
				continue;
			}

			var radius = action.EffectRange;
			var reached = 0;
			foreach (var hostile in DataCenter.AllHostileTargets)
			{
				if (hostile != null && hostile.DistanceToPlayer() <= DataCenter.JobRange
					&& System.Numerics.Vector3.Distance(member.Position, hostile.Position) - hostile.HitboxRadius <= radius)
				{
					reached++;
				}
			}

			if (reached >= 2 && reached * 2 >= inRange)
			{
				return true;
			}
		}

		return false;
	}

	private static readonly System.Text.RegularExpressions.Regex StunDuration = new(@"Stun Duration: (?<value>\d+)s",
		System.Text.RegularExpressions.RegexOptions.CultureInvariant);
	private static readonly Dictionary<uint, bool> _stunsAroundCaster = [];

	private static bool StunsAroundCaster(Lumina.Excel.Sheets.Action action)
	{
		if (!_stunsAroundCaster.TryGetValue(action.RowId, out var stuns))
		{
			stuns = action.Cast100ms > 0 && action.EffectRange > 0 && !action.TargetArea
				&& ActionTooltip.TryStated(action.RowId, StunDuration, out _);
			_stunsAroundCaster[action.RowId] = stuns;
		}

		return stuns;
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

	[RotationDesc(ActionID.OblationPvE, ActionID.ArmsLengthPvE, ActionID.TheBlackestNightPvE, ActionID.DarkMindPvE, ActionID.ShadowWallPvE, ActionID.ShadowedVigilPvE, ActionID.RampartPvE, ActionID.ReprisalPvE)]
	protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
	{
		// A predicted tankbuster on the player gets its big mitigation first - or Rampart while that one is
		// spent - ahead of the short ones: the window opens a few seconds before the hit, and with the cheap ones
		// ahead the big one landed last (trace of 01.10.2026: Damnation 0.7 s before the hit, A241, A243).
		if (BMRShouldRefreshBefore(BMRTankbusterIn, DefensiveValues.DurationOf(ShadowedVigilPvE.EnoughLevel ? ShadowedVigilPvE.ID : ShadowWallPvE.ID), true, null, ShadowedVigilPvE.EnoughLevel ? StatusID.ShadowedVigil : StatusID.ShadowWall)
			&& (ShadowedVigilPvE.EnoughLevel ? ShadowedVigilPvE.CanUse(out act, skipStatusProvideCheck: true) : ShadowWallPvE.CanUse(out act, skipStatusProvideCheck: true)))
		{
			return true;
		}

		if (BMRShouldRefreshBefore(BMRTankbusterIn, DefensiveValues.DurationOf((uint)ActionID.RampartPvE), true, null, StatusID.Rampart)
			&& RampartTakesPredictedTankbuster(ShadowedVigilPvE.EnoughLevel ? ShadowedVigilPvE : ShadowWallPvE, ShadowedVigilPvE.EnoughLevel ? StatusID.ShadowedVigil : StatusID.ShadowWall)
			&& RampartPvE.CanUse(out act, skipStatusProvideCheck: true))
		{
			return true;
		}

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

		// The big mitigation and Rampart, spaced, or in a pull held until the measured fall needs them (A359).
		if (StaggeredBigMitigation(ShadowWallPvE, ShadowedVigilPvE, out act))
		{
			return true;
		}

		// Same hold as in the area path, and for the same reason: on a group pull the barrier's
		// reward depends on the stream that Reprisal would thin. In a boss fight the condition is
		// false by the hostile count, so a tankbuster keeps its mitigation.
		if (!HoldMitigationForBarrier(false)
			&& !HoldReprisalForRaidwide()
			&& ShouldSustainMitigationDebuff(StatusHelper.ReprisalStatus)
			&& ReprisalPvE.CanUse(out act, skipAoeCheck: true, skipStatusProvideCheck: true))
		{
			return true;
		}

		if (!HoldReprisalForRaidwide() && !HoldMitigationForBarrier(false) && ReprisalPvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		// On another member a tankbuster is aimed at, after his own defence and the barrier on himself (concept 09,
		// "Hilfe für das Ziel eines Tankbusters"). The Balance: a tankbuster in a raid or trial almost never fails to
		// break it, and the break refunds the MP as Dark Arts.
		if (BlackestNightOnTankbusterTarget && HelpTankbusterTarget(TheBlackestNightPvE, out act))
		{
			return true;
		}

		return base.DefenseSingleAbility(nextGCD, out act);
	}

	protected override bool AttackAbility(IAction nextGCD, out IAction? act)
	{
		// The tincture in the burst whenever it is usable and at hand; where is "Tincture usage" alone (his rule, A500). Before, only in the countdown.
		if (InTwoMIsBurst && InCombat && UseBurstMedicine(out act))
		{
			return true;
		}

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
			// Flood of Darkness, or Flood of Shadow where it has replaced it, only where it deals at least Edge on the enemies
			// it reaches - from 3 at patch 7.5 (concept 02, "Ausgaben in der Fläche erst, wo sie mindestens so viel bringen";
			// A473). Its AoE count stays the lower bound.
			if (FloodOfDarknessPvE.CanUse(out act) && AreaWorthOver(FloodOfDarknessPvE, EdgeOfDarknessPvE))
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

		// Abyssal Drain shares its recast with Carve and Spit: it goes where it out-damages it on the enemies it reaches,
		// by their tooltips (A391), its AoE count the lower bound.
		if (AbyssalDrainPvE.CanUse(out act)
			&& ActionTooltip.AreaOutdamagesSingle(AbyssalDrainPvE.ID, AbyssalDrainPvE.Target.AffectedTargets.Length, CarveAndSpitPvE.ID))
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

		// AOE Delirium and the area blood spender, each where it deals at least the single one on the enemies it reaches:
		// Impalement against Torcleaver, the strongest step of the Delirium combo, and Quietus against Bloodspiller - both
		// from 3 at patch 7.5 (A473).
		if (ImpalementPvE.CanUse(out act) && AreaWorthOver(ImpalementPvE, TorcleaverPvE))
		{
			return true;
		}

		if (UseBlood && QuietusPvE.CanUse(out act) && AreaWorthOver(QuietusPvE, BloodspillerPvE))
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

		if (UseBlood && BloodspillerPvE.CanUse(out act, skipComboCheck: true))
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

	// How much Blood to carry into Delirium. The Balance (7.5): "entering buffs (when Delirium is
	// pressed) with 70 or less Blood Gauge will ensure that you do not overcap ... it is best to have
	// the Blood Gauge as high as possible (up to 70) when entering Delirium". The job guide gives the
	// reason: Delirium grants three stacks of Blood Weapon, each adding 10 Blood (A197).
	private const byte BloodPoolLimit = 70;

	// Determines if currently in a burst phase based on cooldowns of key abilities.
	private bool InTwoMIsBurst => BloodWeaponPvE.Cooldown.IsCoolingDown && DeliriumPvE.Cooldown.IsCoolingDown && ((LivingShadowPvE.Cooldown.IsCoolingDown && !LivingShadowPvE.Cooldown.ElapsedAfter(15)) || !LivingShadowPvE.EnoughLevel);

	// Manages DarkSide ability based on several conditions.
	private bool CheckDarkSide
	{
		get
		{
			if (DarkSideEndAfterGCD(3))
			{
				return EdgeKeepsBlackestNightMp;
			}

			if ((InTwoMIsBurst && HasDarkArts) || (HasDarkArts && StatusHelper.PlayerHasStatus(true, StatusID.BlackestNight)) || (HasDarkArts && DarkSideEndAfterGCD(3)))
			{
				return true;
			}

			if (InTwoMIsBurst && BloodWeaponPvE.Cooldown.IsCoolingDown && LivingShadowPvE.Cooldown.IsCoolingDown && SaltedEarthPvE.Cooldown.IsCoolingDown && ShadowbringerPvE.Cooldown.CurrentCharges == 0 && CarveAndSpitPvE.Cooldown.IsCoolingDown
				&& EdgeKeepsBlackestNightMp)
			{
				return true;
			}

			return CurrentMp >= 8500;
		}
	}
	#endregion

}
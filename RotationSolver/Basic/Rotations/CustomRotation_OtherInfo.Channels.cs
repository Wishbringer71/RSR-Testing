using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace RotationSolver.Basic.Rotations;

public partial class CustomRotation
{
	/// <summary>
	/// A pause in the fight: in combat, and no hostile within 25 yalms - nothing to strike, the boss
	/// untargetable or gone. Read now, without BossModReborn; the predicted pause is
	/// <see cref="BMRDowntimeWithin"/>. The universal half of the pause rule (concept 14); what a job
	/// does in the pause is its own rule.
	/// </summary>
	[Description("In combat without a hostile in 25 yalms")]
	public static bool InCombatPause => InCombat && !HasHostilesInMaxRange;

	/// <summary>
	/// True in the last decision in which <paramref name="action"/> still goes off inside the window of
	/// <paramref name="statusIDs"/>. Chosen now, it goes off when the GCD rolls; the next chance comes a
	/// GCD after that, and by then the window would end before the action and its cast. For a rule that
	/// holds a window back for an alignment (burst, a debuff) and must not lose it: the universal half
	/// of "use it before it runs out" (concept 14, "Werden die Fenster genutzt?"); what the window waits
	/// for is the job's rule.
	/// </summary>
	protected static bool IsLastChanceBeforeStatusEnds(IBaseAction action, params StatusID[] statusIDs)
	{
		return StatusHelper.PlayerWillStatusEnd(DataCenter.DefaultGCDRemain + DataCenter.DefaultGCDTotal + action.Info.CastTime, true, statusIDs);
	}

	/// <summary>
	/// The party's first living tank, or null if there is none.
	/// </summary>
	protected static IBattleChara? PartyTank => DataCenter.PartyTank;

	/// <summary>
	/// The reach of the four tank gap closers (Intervene, Onslaught, Shadowstride, Trajectory), the longest of them from
	/// the action sheet - 20 yalms for each today (A340).
	/// </summary>
	private static float TankGapCloserRangeYalms => _tankGapCloserRange ??= LongestRange(
		ActionID.IntervenePvE, ActionID.OnslaughtPvE, ActionID.ShadowstridePvE, ActionID.TrajectoryPvE);

	private static float? _tankGapCloserRange;

	private static float LongestRange(params ActionID[] actions)
	{
		var sheet = Service.GetSheet<Lumina.Excel.Sheets.Action>();
		float longest = 0;
		foreach (var action in actions)
		{
			if (sheet.TryGetRow((uint)action, out var row))
			{
				longest = Math.Max(longest, row.Range);
			}
		}

		return longest;
	}

	/// <summary>
	/// Whether the party's tank is within gap-closer range of enough hostiles to be pulling them, used
	/// to time tank sustain across a dungeon wall-to-wall. Out of scope in Trials and Raids.
	/// Before combat the count is <paramref name="prePullMinimumHostileCount"/> (opening groups are
	/// small); once in combat it is <paramref name="wallToWallMinimumHostileCount"/>, which acts as the
	/// off-ramp back to the normal rotation. Both are per-job config.
	/// </summary>
	protected static bool TankApproachingMobGroup(int prePullMinimumHostileCount, int wallToWallMinimumHostileCount)
	{
		var tank = PartyTank;
		if (tank == null)
		{
			return false;
		}

		var contentType = DataCenter.Territory?.ContentType;
		if (contentType == TerritoryContentType.Trials || contentType == TerritoryContentType.Raids)
		{
			return false;
		}

		var targets = DataCenter.AllHostileTargets;
		var minimumHostileCount = DataCenter.InCombat ? wallToWallMinimumHostileCount : prePullMinimumHostileCount;

		var mobsInRange = 0;
		for (int i = 0, n = targets.Count; i < n; i++)
		{
			var hostile = targets[i];
			if (hostile != null && Vector3.Distance(tank.Position, hostile.Position) <= TankGapCloserRangeYalms)
			{
				mobsInRange++;
			}
		}
		return mobsInRange >= minimumHostileCount;
	}

	/// <summary>
	/// The hostiles in reach that Arm's Length's Slow is for: not bosses.
	/// </summary>
	/// <remarks>
	/// Arm's Length does not touch the hit that strikes it - "When you are struck by a physical attack,
	/// the striker will be afflicted with Slow" - so its only worth in a defence is slowing the attacks
	/// that follow, and that is a pack of ordinary enemies on the tank. The owner observed it cast on a
	/// boss's tankbuster to no effect (01.10.2026); counted with the pack, a boss also let two enemies -
	/// boss and one add - read as a pull. Spent there, it is missing for the knockback it is the tank's
	/// only answer to.
	/// </remarks>
	protected static int SlowableHostilesInRange()
	{
		var range = DataCenter.JobRange;
		var hostiles = DataCenter.AllHostileTargets;
		var count = 0;
		for (int i = 0, n = hostiles.Count; i < n; i++)
		{
			var hostile = hostiles[i];
			if (hostile != null && hostile.DistanceToPlayer() < range && !hostile.IsBossFromTTK() && !hostile.IsBossFromIcon())
			{
				count++;
			}
		}

		return count;
	}

	/// <summary>
	/// Whether the pack in reach is slowed - at least two enemies and at least half of them.
	/// </summary>
	/// <remarks>
	/// Slow is not only a caster debuff: its effect text names the auto-attack delay alongside cast
	/// and recast time, and trash enemies deal most of their damage by auto-attack. A slowed pack
	/// therefore throttles the incoming stream by about the size of the debuff - Arm's Length applies
	/// Slow +20% to every physical attacker for 15s, the same order as Rampart. The Dark Knight's
	/// barrier reads it (a thinned stream no longer breaks The Blackest Night), and every tank's
	/// Arm's Length pull rule does (a slowed pack needs no second slow).
	///
	/// Same share rule as the stun condition, and for the same reason: one slowed enemy out of eight
	/// says nothing about the stream. The difference is the timing - a stun stops the stream and
	/// lapses in seconds, a slow thins it for fifteen, so this one has no grace window. It ends when
	/// the debuff does.
	/// </remarks>
	protected static bool PackSlowed()
	{
		var inRange = SurveyHostileStatus(DataCenter.JobRange, StatusHelper.SlowStatus, out var slowed);
		return inRange > 0 && slowed >= 2 && slowed * 2 >= inRange;
	}

	/// <summary>
	/// Arm's Length for its Slow on a group pull - the rule for every tank, first built for the Dark
	/// Knight (A53) and lifted to the tank level (A194): each tank carries the same option and passes
	/// it in, with the hostile count that marks a pull for it.
	/// </summary>
	/// <remarks>
	/// Arm's Length is a role action of tanks and melee; this rule is for tanks, because the Slow
	/// throttles the stream of auto-attacks on whoever holds the pack. It costs nothing but the
	/// cooldown. Not while the pack is already slowed - a second Slow does not stack onto the first.
	/// Not while BossModReborn announces a knockback that lands after Arm's Length would have run out
	/// and before its cooldown is back: the rule also fires on a boss with adds, and spent on their
	/// Slow the action would be gone for the knockback it is the tank's only answer to. A knockback
	/// inside the duration is no reason to wait - cast now, it is covered (A212, A219). Bosses do not
	/// count towards the pull: the Slow is for the stream of an ordinary pack, and on a boss alone the
	/// rule cast it to no effect (A236). Duration from
	/// the effect text, cooldown from the action data. Without a module there is no announcement, and
	/// the anti-knockback use stays reactive as before.
	/// </remarks>
	protected bool ArmsLengthSlowsPull(bool enabled, int minimumHostiles)
		=> enabled
			&& DataCenter.Role == JobRole.Tank
			&& SlowableHostilesInRange() >= minimumHostiles
			&& !PackSlowed()
			&& !(Service.Config.UseBmrTimeline && BMRKnockbackIn is > 0f and < float.MaxValue
				&& BMRKnockbackIn > DefensiveValues.DurationOf((uint)ActionID.ArmsLengthPvE)
				&& BMRKnockbackIn <= ArmsLengthPvE.Cooldown.RecastTimeOneChargeRaw);

	/// <summary>
	/// A single-target barrier with charges that sits at its last charge: onto the member an enemy is attacking, where
	/// none of its barrier stands. At the cap the recharge stops, so every second there is a charge lost, and the hits on
	/// the attacked member use the barrier up - The Balance, White Mage Basic Guide (patch 7.3): "Try to not let this
	/// overcap on charges - you're almost always guaranteed to get value out of this just slapping it on the tank for
	/// autos." The healers' rule for every such barrier: Divine Benison, Celestial Intersection (A376).
	/// </summary>
	/// <param name="action">The barrier with charges.</param>
	/// <param name="act">The action aimed at the attacked member, when due.</param>
	protected static bool TryChargedBarrierAtCap(IBaseAction action, out IAction? act)
	{
		act = null;
		if (!InCombat || action.Cooldown.MaxCharges <= 1 || action.Cooldown.CurrentCharges < action.Cooldown.MaxCharges
			|| !action.CanUse(out act, targetOverride: TargetType.BeAttacked))
		{
			act = null;
			return false;
		}

		DefenseTrace.Decision("a charge at the cap, onto the attacked member", act);
		return true;
	}

	/// <summary>
	/// A healer's pull upkeep: an instant HoT or barrier kept on the tank through a dungeon pull and
	/// renewed as soon as it runs out or is used up. One rule for every healer (the white mage's Regen,
	/// the astrologian's Aspected Benefic, the sage's Eukrasian Diagnosis); only the action differs.
	/// </summary>
	/// <param name="action">The instant HoT or barrier.</param>
	/// <param name="prePullMinimumHostileCount">See <see cref="TankApproachingMobGroup"/>.</param>
	/// <param name="wallToWallMinimumHostileCount">See <see cref="TankApproachingMobGroup"/>.</param>
	/// <param name="floorHealthRatio">At or below this the tank is left to the job's emergency heals;
	/// 0 where the upkeep action is itself the better emergency GCD.</param>
	/// <param name="raise">The job's raise. The upkeep never spends the MP a raise needs: a pull runs
	/// for minutes and renews on every broken barrier, and a healer who cannot raise the one who dies
	/// has traded the party's safety for a tank who was never in danger.</param>
	/// <param name="heldBy">Statuses that already do the upkeep's job on the tank, where the action's
	/// own list is narrower (a barrier that cannot stack with a sibling). Null: the action's own list.</param>
	/// <param name="act">The action aimed at the tank, when due.</param>
	/// <remarks>
	/// The target override bypasses the candidate status check (FindTankTarget does not call
	/// CheckStatus), so the remaining duration is checked here. A barrier is removed by the game when
	/// it is used up, so the same check renews it on breaking as on running out.
	/// </remarks>
	protected static bool TryPullUpkeepOnTank(IBaseAction action, int prePullMinimumHostileCount, int wallToWallMinimumHostileCount,
		float floorHealthRatio, IBaseAction? raise, StatusID[]? heldBy, out IAction? act)
	{
		act = null;

		if (!TankApproachingMobGroup(prePullMinimumHostileCount, wallToWallMinimumHostileCount))
		{
			return false;
		}

		if (raise != null && raise.EnoughLevel && CurrentMp < action.Info.MPNeed + raise.Info.MPNeed)
		{
			return false;
		}

		if (!action.CanUse(out act, targetOverride: TargetType.Tank))
		{
			act = null;
			return false;
		}

		var tank = action.Target.Target;
		if (tank != null && tank.GetForecastHealthRatio() > floorHealthRatio
			&& tank.WillStatusEndGCD(action.Config.StatusRefreshGcdCount, 0, action.Setting.StatusFromSelf, heldBy ?? action.Setting.TargetStatusProvide ?? []))
		{
			return true;
		}

		act = null;
		return false;
	}

	/// <summary>
	/// Whether the weaponskill chosen now is the last one that lands on the current target: the one after it could begin
	/// only once the target has died - by its time to kill, corrected by how wrong the forecast has been lately, and only
	/// while it is the last hostile in range, since another one there takes the next hit - or once the downtime
	/// BossModReborn's timeline predicts has begun (<see cref="BMRDowntimeWithin"/>). Without that timeline a downtime is
	/// known only when it has begun, too late for this (A413).
	/// </summary>
	public static bool LastWeaponskillOnTarget
	{
		get
		{
			var nextBegins = WeaponRemain;
			var oneAfter = WeaponRemain + WeaponTotal;
			if (BMRDowntimeWithin(oneAfter) && BMRDowntimeIn > nextBegins)
			{
				return true;
			}

			return CurrentTarget is { } target && NumberOfHostilesInRange <= 1 && LastWeaponskillBefore(target, 0f);
		}
	}

	/// <summary>
	/// Whether the weaponskill chosen now is the last whose effect still finds <paramref name="target"/> alive
	/// <paramref name="lands"/> seconds after it goes off: by its time to kill, corrected by how wrong the forecast has
	/// been lately, it outlives that after the next weaponskill begins and would not after the one after it.
	/// </summary>
	/// <param name="target">The enemy.</param>
	/// <param name="lands">How long after the weaponskill goes off its effect has to find the enemy alive - 0 for a hit,
	/// the time to a status for one that has to carry it when it falls (Capture, A523).</param>
	public static bool LastWeaponskillBefore(IBattleChara target, float lands)
	{
		var ttk = target.GetCorrectedTTK();
		var lives = WeaponRemain + lands;
		return !float.IsNaN(ttk) && ttk > lives && ttk <= lives + WeaponTotal;
	}

	/// <summary>
	/// Whether holding <paramref name="action"/> for <paramref name="holdSeconds"/> lets all its charges stand full before the
	/// hold ends: the recharge stops there, and every second at the maximum is recast lost that pushes each later use back.
	/// Using one charge now leaves the next one back for the buff it waited for (concept 14, A604).
	/// </summary>
	/// <param name="action">The charged action that would wait.</param>
	/// <param name="holdSeconds">How long from now it would wait.</param>
	/// <returns>True when its charges would be full before the hold ends.</returns>
	public static bool HoldCapsCharges(IBaseAction action, float holdSeconds)
	{
		if (action == null || holdSeconds <= 0f)
		{
			return false;
		}

		var max = action.Cooldown.MaxCharges;
		return max > 1 && action.Cooldown.WillHaveXCharges(max, holdSeconds);
	}

	/// <summary>
	/// Whether one of the big personal mitigations is already running on the player.
	/// <para>
	/// <see cref="StatusHelper.RampartStatus"/> is what the job rotations already use as
	/// <c>StatusProvide</c> on Shadow Wall and Shadowed Vigil, so those never overlap each other.
	/// Reading the same list here extends that stagger to an action that cannot express it through
	/// <c>StatusProvide</c>, because its own status is a barrier rather than a mitigation.
	/// </para>
	/// </summary>
	/// <remarks>
	/// One entry of that list is not a throttle and is worth knowing about before a second caller
	/// appears. <c>LivingDead</c> does not reduce the incoming stream at all - it postpones death -
	/// so for a dark knight this reads true while the stream is unchanged; the answer still suits the
	/// one caller, because a barrier is the wrong thing to spend during it for a different reason.
	/// </remarks>
	protected static bool HasMajorMitigation
		=> StatusHelper.PlayerHasStatus(true, StatusHelper.RampartStatus);

	/// <summary>
	/// Whether a tankbuster is aimed at the player: a lock-on VFX or a cast from the learned list
	/// landing now, or one BMR predicts inside the mitigation window that reaches him (its mask; without one,
	/// for a tank, A220). The prediction alone does not say whom it hits (A284).
	/// <para>
	/// Deliberately not <c>IsHostileCastingToTank</c>. That one falls back to "the enemy is casting
	/// at its own target", which for a tank holding the pull matches any uninterruptible trash cast -
	/// harmless for a free mitigation, wrong for an action with a resource cost (AUDIT_LOG C10).
	/// </para>
	/// </summary>
	public static bool TankbusterOnMe
		=> DataCenter.IsHostileCastingTankBusterAtMe || DataCenter.BMRTankbusterReachesPlayer;

	/// <summary>
	/// Whether a party tool used now would miss the raidwide BossModReborn announces: the raidwide lands after
	/// the tool's effect has run out and before the tool is ready again. Detection only; the consumer decides.
	/// </summary>
	/// <param name="tool">The party tool.</param>
	/// <param name="lasts">How long its effect lasts, from the effect text.</param>
	protected static bool RaidwideAfterEffectBeforeRecast(IBaseAction tool, float lasts)
		=> Service.Config.UseBmrTimeline && BMRActive
			&& BMRRaidwideIn is > 0f and < float.MaxValue
			&& BMRRaidwideIn > lasts
			&& BMRRaidwideIn <= tool.Cooldown.RecastTimeOneChargeRaw;

	/// <summary>
	/// Whether a tank keeps Reprisal in its single-target defence for the raidwide BossModReborn announces.
	/// </summary>
	/// <remarks>
	/// Reprisal acts on the enemy: every hit it deals during the 15 s is reduced, the tankbuster and the
	/// raidwide alike (the owner's hint, 01.10.2026). Only when the raidwide lands after Reprisal has run out
	/// and before it is back does using it now cost the party its 10 % at the raidwide. Then the owner's
	/// criterion decides: who does going without put in serious danger. The tank meets its tankbuster with
	/// its own big mitigation already running - one of Rampart or the 40 % cooldown plus the short ones is the
	/// standard answer (The Balance) - while the party has no personal tool of that size; party tools belong
	/// on raid-wide damage (The Balance). So Reprisal waits when the tankbuster is on the player and he is
	/// covered. It does not wait when he is not covered, when the hit is on someone else (their cover is
	/// not known here), on a pull, or without an announcement. The universal hold yields when the player or
	/// a tank is in the critical class (A244).
	/// </remarks>
	protected bool HoldReprisalForRaidwide()
		=> HoldSingleDefense(
			DataCenter.Role == JobRole.Tank
				&& TankbusterOnMe
				&& DataCenter.SingleHitReachesPlayer
				&& StatusHelper.PlayerHasStatus(true, StatusHelper.RampartStatus)
				&& RaidwideAfterEffectBeforeRecast(ReprisalPvE, MitigationDebuffDuration),
			"Reprisal kept for the announced raidwide");

	/// <summary>
	/// Whether Rampart goes out for a predicted tankbuster: only when the job's big mitigation does not take
	/// it - one of the two per tankbuster, plus the short ones (The Balance, "one of Rampart or your 40%
	/// cooldown, plus your short cooldown"). Stacked on one tankbuster, both were gone for the next: trace of
	/// 01.10.2026, tankbusters about every 61 s, 30 % under both, then 85 % with neither ready (A243).
	/// </summary>
	/// <param name="big">The job's big mitigation at the player's level (Damnation, Guardian, Shadowed
	/// Vigil, Great Nebula or their lower forms).</param>
	/// <param name="bigStatus">Its status on the player.</param>
	protected static bool RampartTakesPredictedTankbuster(IBaseAction big, StatusID bigStatus)
		=> !big.EnoughLevel
			|| (!StatusHelper.PlayerHasStatus(true, bigStatus) && !IsLastAbility(false, big));

	/// <summary>
	/// Whether a tankbuster on the player lands within <paramref name="seconds"/> and can hurt him - the
	/// question for an own cooldown whose effect lasts that long: a lock-on VFX or a listed tankbuster
	/// cast on him, or one BossModReborn announces within that time. Where BossModReborn does not name
	/// who is hit, the one his target has targeted is taken to get it (an inference: some tankbusters
	/// hit the second in enmity). Not while an invulnerability covers him.
	/// </summary>
	protected static bool TankbusterOnMeWithin(float seconds)
	{
		var player = Player;
		if (player == null || !player.CanBeHealed())
		{
			return false;
		}

		if (DataCenter.IsHostileCastingTankBusterAtMe)
		{
			return true;
		}

		return BMRTankbusterWithin(seconds)
			&& (DataCenter.BMRTankbusterHitsPlayer ?? ObjectHelper.PlayerIsTargetOnSelf());
	}
}

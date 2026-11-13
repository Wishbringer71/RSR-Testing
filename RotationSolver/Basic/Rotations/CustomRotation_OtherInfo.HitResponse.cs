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
	/// How many more hits of a learned series on the party the player would still take - a series running, or one an enemy
	/// cast announces within the next GCD; 0 when none. Counted the way Liturgy of the Bell counts its triggers
	/// (<see cref="HitSeries"/>: within its 20 s, one hit a second, the player and another member), and read by every
	/// healer whose big tool gains from each hit of a series (concept 08, "Glockenspiel", A468).
	/// </summary>
	public static int HitSeriesHitsLeft => HitSeries.TriggersLeft();

	/// <summary>
	/// Whether a tool limited to multihit party stacks may go now: a multihit stack marker, or - each under its own switch -
	/// a learned series still hitting (<see cref="HitSeriesHitsLeft"/>) or an announced area hit that reaches the player
	/// and would put an unprotected member in danger (concept 08, A349, A468).
	/// </summary>
	/// <param name="alsoSeries">Also into a learned series.</param>
	/// <param name="alsoDanger">Also before a dangerous area hit.</param>
	/// <returns>True when the limited tool may go.</returns>
	protected static bool MultiHitAhead(bool alsoSeries, bool alsoDanger)
		=> IsCastingMultiHit || (alsoSeries && HitSeriesHitsLeft > 0) || (alsoDanger && AreaHitOnMe && AreaDefenseDanger(out _));

	/// <summary>
	/// True when a status will expire before a predicted BMR event lands, so it should be refreshed
	/// now rather than on cooldown. The 0.6s floor matches StateUpdater's own guards. Always false
	/// when BMR is inactive or UseBmrTimeline is off.
	/// </summary>
	/// <param name="predictedIn">One of the BMR*In values: seconds until the predicted event.</param>
	/// <param name="statusDuration">The status's own duration, in seconds.</param>
	/// <param name="statusFromSelf">Whether the status is read on the player rather than the target.</param>
	/// <param name="target">Null for a self status, the enemy for a debuff.</param>
	/// <param name="action">
	/// The action this refresh would spend, when the caller can name it. Read by the hold below: an
	/// action with a charge to spare, or with its next charge back before the predicted event, is not
	/// held, because the reserve A9 protects is not at risk. Troubadour, Tactician and Shield Samba pass
	/// theirs and have one charge, so their reserve is always real. Radiant Aegis has two from level 88
	/// ("Enhanced Radiant Aegis [480]: Maximum Charges: 2") and is not passed: the option's own text has
	/// it wait for the bigger hit instead of being spent on the small one.
	/// </param>
	/// <param name="statusIDs">The status or statuses whose remaining time decides the refresh.</param>
	public static bool BMRShouldRefreshBefore(float predictedIn, float statusDuration, bool statusFromSelf, IBattleChara? target, IBaseAction? action, params StatusID[] statusIDs)
	{
		if (!Service.Config.UseBmrTimeline || !BMRActive || predictedIn > statusDuration
			|| !DataCenter.PressReachesBeforeSnapshot(predictedIn))
		{
			return false;
		}

		// BossModReborn answers WHEN, never HOW HARD, and spending on the wrong one of two hits in a
		// row is the same loss as not spending at all.
		//
		// Reported from play, Eternal Queen's opening: a small area cast, then a big one. The
		// prediction fires on the first, Radiant Aegis or Tactician goes out for it, and by the time
		// the big one lands the barrier is spent or the debuff has expired. A barrier is the clearer
		// case - it absorbs points, so a small hit eats it outright.
		//
		// The size IS known for a cast that is already on screen: it is measured per action. So when
		// a rated small area cast is running right now, the event the prediction is pointing at is
		// most likely that one, and this refresh waits for the next. Where nothing is running, or
		// the running cast has never been measured, no claim is made and the behaviour is unchanged.
		//
		// This is the hole that the size rating left open: the reactive path has asked "is this hit
		// worth a cooldown" since A108, the proactive path never did.
		// Whether to spend a charge here is decided in the fight, not by reading a counter afterwards.
		// Two things are asked, both answerable right now:
		//
		// 1. Does holding still earn its keep? Every hold is scored against what actually landed
		//    (DataCenter.ScoreProactiveHold). Once it has been wrong more often than right, it stops
		//    holding. A rule that mispredicts this fight's pattern therefore corrects itself inside
		//    that fight instead of waiting for somebody to look at a number.
		//
		// 2. Is the reserve A9 protects actually at risk? The owner's decision there was not "never
		//    spend the second charge" but its consequence: "when real danger came there was none
		//    left". That is a question about availability, and it can be computed - spending now is
		//    safe when a charge remains afterwards, or when the next one returns before the predicted
		//    event. Where neither holds, the reserve is real and the hold stands.
		//
		// The A9 case itself is untouched: there was no prediction at all there, so this branch is
		// never reached for it.
		//
		// "The next one is back" counts from the moment of spending. With a charge already recharging
		// that is its remaining time; with none recharging - a single-charge action that is ready, or
		// every charge full - spending starts a full recast. The remaining time reads 0 there only
		// because nothing is recharging yet, and read as "back at once" it would have waved every
		// ready single-charge action through.
		var reserveSurvivesSpending = action != null
			&& (action.Cooldown.CurrentCharges > 1
				|| (action.Cooldown.IsCoolingDown
					? action.Cooldown.RecastTimeRemainOneCharge
					: action.Cooldown.RecastTimeOneChargeRaw) <= predictedIn);

		if (Service.Config.HoldProactiveMitigationForSmallCast
			&& DataCenter.AnnouncedHitIsSmall
			&& DataCenter.ProactiveHoldIsEarningItsKeep
			&& !reserveSurvivesSpending)
		{
			// The window the hold is betting on: the status it would have refreshed has to still be
			// worth refreshing when the real hit arrives, so that duration is the honest deadline.
			DataCenter.NoteProactiveHold(statusDuration);
			var held = DataCenter.AnnouncedAreaAction;
			if (held != 0)
			{
				DataCenter.ProactiveMitigationHeld[held] = DateTime.Now;
			}
			return false;
		}

		var chara = statusFromSelf ? Player : target;
		return chara != null && chara.WillStatusEnd(predictedIn, statusFromSelf, statusIDs);
	}

	/// <summary>
	/// As above, for callers that do not name the action they would spend.
	/// </summary>
	public static bool BMRShouldRefreshBefore(float predictedIn, float statusDuration, bool statusFromSelf, IBattleChara? target, params StatusID[] statusIDs)
	{
		return BMRShouldRefreshBefore(predictedIn, statusDuration, statusFromSelf, target, null, statusIDs);
	}

	/// <summary>
	/// As above, for an action that declares on itself which statuses already do its job on the player
	/// (<see cref="ActionSetting.StatusProvide"/>), and whether only its own count
	/// (<see cref="ActionSetting.StatusFromSelf"/>). The proactive refresh then asks the same question
	/// as the action's own status check, so the two cannot drift apart.
	/// </summary>
	/// <remarks>
	/// Written for Troubadour, Tactician and Shield Samba, each of which "cannot be stacked" with the
	/// other two. The game replaces a standing one with the new one whoever cast it (measured in the
	/// owner's logs, A592), so casting ours while another player's still covers the predicted hit
	/// gains nothing for that hit and spends the cooldown the next one needed. Their reactive path
	/// always checked the whole family; the proactive one checked only the player's own status.
	/// </remarks>
	/// <param name="predictedIn">One of the BMR*In values: seconds until the predicted event.</param>
	/// <param name="action">The action the refresh would spend; also read by the hold above.</param>
	public static bool BMRShouldRefreshBefore(float predictedIn, IBaseAction action)
	{
		var statuses = action.Setting.StatusProvide;
		if (statuses == null || statuses.Length == 0)
		{
			return false;
		}

		var fromSelf = action.Setting.StatusFromSelf;
		return BMRShouldRefreshBefore(predictedIn, DefensiveValues.DurationOf(action.ID), fromSelf, fromSelf ? null : Player, action, statuses);
	}

	/// <summary>
	/// The duration of Reprisal's debuff at the player's synced level, from the game - the figure Addle and Feint share
	/// with it today (<see cref="MitigationDebuffDurationOf"/>).
	/// </summary>
	/// <remarks>
	/// The Enhanced traits extend the three debuffs and change nothing else - not the reduction, which stays at each
	/// debuff's base figure whatever the level (the user checked the Lodestone entry after this code had briefly priced
	/// an end-game Reprisal at 15%; action 7535 states 10% and leaves only the duration blank, which is how the game
	/// data marks a trait-modified figure). The duration used to be written in here as 10 s, 15 s from level 98 - the
	/// same task <see cref="DefensiveValues.DurationOf"/> answers from the trait and the tooltip at the synced level
	/// (A338).
	/// </remarks>
	protected static float MitigationDebuffDuration => MitigationDebuffDurationOf(StatusHelper.ReprisalStatus);

	/// <summary>
	/// The duration at the player's synced level of the enemy mitigation debuff the given statuses belong to: the
	/// action that lays them (<see cref="DefensiveValues.MitigatingStatusesByActionId"/>) and its duration from the game
	/// (<see cref="DefensiveValues.DurationOf"/>). Where the game answers nothing below the trait's level, the trait's
	/// figure - a duration taken too long has a refresh come early, one of zero would hold Reprisal for a raidwide it
	/// already covers.
	/// </summary>
	/// <param name="statusIDs">The statuses of one debuff: Addle's, Feint's or Reprisal's.</param>
	/// <returns>The duration in seconds; zero when no action lays any of them.</returns>
	protected static float MitigationDebuffDurationOf(params StatusID[] statusIDs)
	{
		foreach (var (actionId, statuses) in DefensiveValues.MitigatingStatusesByActionId)
		{
			foreach (var statusId in statusIDs)
			{
				if (Array.IndexOf(statuses, (uint)statusId) < 0)
				{
					continue;
				}

				var seconds = DefensiveValues.DurationOf(actionId);
				return seconds > 0f
					? seconds
					: DefensiveValues.TraitDurationByActionId.TryGetValue(actionId, out var trait) ? trait.Seconds : 0f;
			}
		}

		return 0f;
	}

	/// <summary>
	/// Whether an enemy mitigation debuff (Addle/Feint/Reprisal) is due for a proactive refresh: either
	/// BMR predicts damage landing after the debuff would have expired, or enough hostiles are in range
	/// to keep it up without a prediction to time it against, since trash pulls usually have no active
	/// BMR module at all. Both branches look at the target's status from any source, so a debuff another
	/// party member already applied is not overwritten.
	/// </summary>
	protected static bool ShouldSustainMitigationDebuff(params StatusID[] statusIDs)
	{
		return BMRShouldRefreshBefore(BMRDamageIn, MitigationDebuffDurationOf(statusIDs), false, HostileTarget, statusIDs)
			|| (NumberOfHostilesInRange >= Service.Config.MitigationSustainHostileCount
				&& (HostileTarget?.WillStatusEndGCD(StatusRefreshGcdCountOf(statusIDs), 0, false, statusIDs) ?? false));
	}

	/// <summary>
	/// The horizon the action laying these statuses names in its own setting, "Number of GCDs before the DOT/Status
	/// effect is reapplied" - the test CanUse makes when its status check is not skipped. The count branch above skips
	/// it for the prediction branch's sake and asked a copy of the default instead, so a changed setting did not reach
	/// it (A340). Without a rotation that lays them, the default of a fresh action configuration.
	/// </summary>
	private static uint StatusRefreshGcdCountOf(StatusID[] statusIDs)
	{
		var actions = DataCenter.CurrentRotation?.AllBaseActions;
		if (actions != null)
		{
			foreach (var action in actions)
			{
				var provided = action.Setting.TargetStatusProvide;
				if (provided == null)
				{
					continue;
				}

				foreach (var status in provided)
				{
					if (Array.IndexOf(statusIDs, status) >= 0)
					{
						return action.Config.StatusRefreshGcdCount;
					}
				}
			}
		}

		return new ActionConfig().StatusRefreshGcdCount;
	}

	// How long before the pull a melee presses its dash: The Balance's Viper opener (Slither at -0.5 s) and upstream's
	// Thunderclap. No game figure gives a dash's delay before the character moves (A347).
	private const float PullDashLead = 0.5f;

	/// <summary>
	/// The melee's dash at the end of a countdown: in its last <see cref="PullDashLead"/> seconds, when the target - an
	/// enemy - stands beyond the range of <paramref name="reach"/>, the job's opening weaponskill. So the first weaponskill
	/// lands at the pull instead of after the walk there; in range, no charge and no animation lock is spent. Governed by
	/// "Dash to the target at the end of the countdown when out of melee range" (concept 04, A347).
	/// </summary>
	/// <param name="remainTime">Seconds left on the countdown.</param>
	/// <param name="dash">The job's target dash.</param>
	/// <param name="reach">An opening weaponskill whose range is melee range.</param>
	/// <param name="act">The dash.</param>
	/// <returns>True when the dash goes out.</returns>
	protected static bool PullGapCloser(float remainTime, IBaseAction dash, IBaseAction reach, out IAction? act)
	{
		act = null;
		if (!Service.Config.PullGapCloser || remainTime > PullDashLead
			|| Svc.Targets.Target is not IBattleChara target || !target.IsHostile()
			|| target.DistanceToPlayer() <= reach.Info.Range)
		{
			return false;
		}

		// A dash that may also go to a party member picks the focus target first; it has to land on the enemy.
		return dash.CanUse(out act) && dash.Target.Target?.GameObjectId == target.GameObjectId;
	}

	/// <summary>
	/// Whether a damage area laid now would hold its enemies: around <paramref name="centre"/> - or the player, where none
	/// is given - at least one enemy has been inside the area's radius throughout the last GCD, and no fewer are inside it
	/// now than, of those still alive, were inside at its start (<see cref="HostileMovement.AreaHolds"/>). Enemies moving
	/// about inside it, a swap at its edge and a death leave it open; a pack moving on closes it, and so do an enemy not
	/// yet in combat in the area or within a tank's reach and a group picked up within that GCD - a tank stopping for the
	/// next group. Owner's rule (25.10.2026): "bei allen Klassen mit Schadensflächen: erst dann, wenn Bewegung nach
	/// walltowall stoppt", with his hints of the same day on enemies moving inside an area, swaps in a big pull, deaths and
	/// the tank's stop to pick up the next group (concept 04, A350-A353).
	/// </summary>
	/// <param name="area">The action that lays the area; its radius is the game's effect range.</param>
	/// <param name="centre">Where the area is laid: the target of an area laid around it, none for one at the player's feet.</param>
	/// <returns>True when the enemies stay in the area.</returns>
	protected static bool DamageAreaSettled(IBaseAction area, IBattleChara? centre = null)
	{
		return DamageAreaSettled((centre ?? Player)?.Position, area.Info.EffectRange);
	}

	internal static bool DamageAreaSettled(Vector3? centre, float radius)
	{
		return centre is { } at && HostileMovement.AreaHolds(at, radius, DataCenter.AllHostileTargets);
	}

	/// <summary>
	/// Whether an attack that carries the player onto <paramref name="target"/> may go out: the target stands closer than
	/// "Only use damaging gap closers within this distance" and within <paramref name="rotationLimit"/>, a rotation's own
	/// limit for that attack - or the player asked for the move: a move-forward command, the countdown, or this very
	/// action queued by him (his press, intercepted, or a /rotation command). The setting's text covers every gap closer
	/// that deals damage; the leaps typed <see cref="SpecialActionType.HostileMovingAttack"/> take the regular targeting,
	/// which does not read it (A358). A rotation switch the player turns on to leap regardless of distance is checked by
	/// its rotation, not here.
	/// </summary>
	/// <param name="target">Where the attack lands.</param>
	/// <param name="rotationLimit">The rotation's own limit for this attack, if it has one.</param>
	/// <param name="pressed">The attack itself, for a check that also runs when the player queued it.</param>
	/// <returns>True when the leap does not take the player further than he allows.</returns>
	public static bool GapCloserAllowed(IBattleChara? target, float rotationLimit = float.MaxValue, ActionID pressed = default)
	{
		// Not the intercepting state: it lasts a few seconds after any press the player makes, a heal included, and
		// would open every leap in that time.
		if (DataCenter.MergedStatus.HasFlag(AutoStatus.MoveForward) || Service.CountDownTime > 0
			|| (pressed != default && DataCenter.CommandNextAction?.ID == (uint)pressed))
		{
			return true;
		}

		// Within the distance, the bound included: set to 0, a leap onto a target he already touches is only damage (his
		// rule; it never went before, as no distance is below 0, A528).
		var distance = target?.DistanceToPlayer() ?? float.MaxValue;
		return distance <= Service.Config.DistanceForMoving2 && distance <= rotationLimit;
	}
}

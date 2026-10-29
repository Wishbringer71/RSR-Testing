using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using Action = Lumina.Excel.Sheets.Action;

namespace RotationSolver.Basic.Actions;

/// <summary>
/// The base action for all actions.
/// </summary>
public class BaseAction : IBaseAction
{
	/// <summary>
	/// Gets or sets the target to use for the action.
	/// </summary>
	/// <value>
	/// A <see cref="TargetResult"/> representing the target of the action.
	/// </value>
	public TargetResult Target { get; set; } = default;

	/// <summary>
	/// Gets the target for preview purposes.
	/// </summary>
	/// <value>
	/// A nullable <see cref="TargetResult"/> representing the preview target, or <c>null</c> if no preview target is available.
	/// </value>
	public TargetResult? PreviewTarget { get; private set; } = null;

	/// <inheritdoc/>
	public Action Action { get; }

	/// <inheritdoc/>
	public ActionTargetInfo TargetInfo { get; }

	/// <inheritdoc/>
	public ActionBasicInfo Info { get; }

	/// <inheritdoc/>
	public ActionCooldownInfo Cooldown { get; }

	ICooldown IAction.Cooldown => Cooldown;

	/// <inheritdoc/>
	public uint ID => Info.ID;

	/// <inheritdoc/>
	public uint AdjustedID => Info.AdjustedID;

	/// <inheritdoc/>
	public uint SortKey => Cooldown.CoolDownGroup;

	/// <inheritdoc/>
	public uint IconID => Info.IconID;

	/// <inheritdoc/>
	public string Name => Info.Name;

	/// <inheritdoc/>
	public string Description => string.Empty;

	/// <inheritdoc/>
	public byte Level => Info.Level;

	/// <inheritdoc/>
	public bool IsEnabled
	{
		get => Config.IsEnabled;
		set => Config.IsEnabled = value;
	}

	/// <inheritdoc/>
	public bool IsIntercepted
	{
		get => Config.IsIntercepted;
		set => Config.IsIntercepted = value;
	}

	/// <summary>
	/// 
	/// </summary>
	public bool IsRestrictedDOT
	{
		get => Config.IsRestrictedDOT;
		set => Config.IsRestrictedDOT = value;
	}

	/// <inheritdoc/>
	public bool MinHPFeature
	{
		get => Config.MinHPFeature;
		set => Config.MinHPFeature = value;
	}

	/// <inheritdoc/>
	public bool SkipPositionSafetyCheck
	{
		get => Config.SkipPositionSafetyCheck;
		set => Config.SkipPositionSafetyCheck = value;
	}

	/// <inheritdoc/>
	public float MinHPPercent
	{
		get => Config.MinHPPercent;
		set => Config.MinHPPercent = value;
	}

	/// <inheritdoc/>
	public bool EnoughLevel => Info.EnoughLevel;

	/// <inheritdoc/>
	public ActionSetting Setting { get; set; }

	private ActionConfig? _defaultConfig;

	/// <summary>
	/// The rotation's own defaults for this action, built once. <see cref="Config"/> compares against
	/// them on every read to carry default changes over into stored user configs, and it is read
	/// several times per <see cref="CanUse"/> for every action the rotation evaluates each frame -
	/// rebuilding them per read allocated an <see cref="ActionConfig"/> that was thrown away again.
	/// Safe to keep: every <c>CreateConfig</c> in this repository returns literal values, and
	/// <see cref="Setting"/> is assigned once during construction.
	/// </summary>
	private ActionConfig DefaultConfig => _defaultConfig ??= Setting.CreateConfig?.Invoke() ?? new ActionConfig();

	private bool _defaultsInSync;

	/// <inheritdoc/>
	public ActionConfig Config
	{
		get
		{
			if (!Service.Config.RotationActionConfig.TryGetValue(ID, out var value) || DataCenter.ResetActionConfigs)
			{
				value = Setting.CreateConfig?.Invoke() ?? new ActionConfig();
				Service.Config.RotationActionConfig[ID] = value;
				_defaultsInSync = false;

				if (!Action.ClassJob.IsValid)
				{
					if (Service.Config.InDebug)
					{
						PluginLog.Debug($"ClassJob is not valid for Action ID: {ID}");
					}
					return value;
				}

				_ = Action.ClassJob.Value;

				if (Setting.TargetStatusProvide != null)
				{
					value.TimeToKill = 0;
				}
			}

			if (_defaultsInSync)
			{
				return value;
			}

			var changed = false;

			var defaultAoe = DefaultConfig.AoeCount;
			if (!value.AoeResetDone || value.AppliedDefaultAoeCount != defaultAoe)
			{
				value.AoeCount = defaultAoe;
				value.AppliedDefaultAoeCount = defaultAoe;
				value.AoeResetDone = true;
				changed = true;
			}

			var defaultStatusRefresh = DefaultConfig.StatusRefreshGcdCount;
			if (value.AppliedDefaultStatusRefreshGcdCount != defaultStatusRefresh)
			{
				value.StatusRefreshGcdCount = defaultStatusRefresh;
				value.AppliedDefaultStatusRefreshGcdCount = defaultStatusRefresh;
				changed = true;
			}

			if (changed)
			{
				Service.Config.RotationActionConfig[ID] = value;
			}

			_defaultsInSync = true;
			return value;
		}
	}

	/// <summary>
	/// The default constructor
	/// </summary>
	/// <param name="actionID">action id</param>
	/// <param name="isDutyAction">is this action a duty action</param>
	public BaseAction(ActionID actionID, bool isDutyAction = false)
	{
		Action = Service.GetSheet<Action>().GetRow((uint)actionID);
		TargetInfo = new(this);
		Info = new(this, isDutyAction);
		Cooldown = new(this);

		Setting = new();
	}

	/// <inheritdoc/>
	public bool CanUse(out IAction act, bool skipStatusProvideCheck = false, bool skipStatusNeed = false, bool skipTargetStatusNeedCheck = false, bool skipComboCheck = false, bool skipCastingCheck = false,
	bool usedUp = false, bool skipAoeCheck = false, bool skipTTKCheck = false, byte gcdCountForAbility = 0, bool checkActionManagerDirectly = false, TargetType targetOverride = default)
	{
		act = this;

		ActionTracer.Try(this);

		if (IBaseAction.ActionPreview)
		{
			skipCastingCheck = true;
		}
		else
		{
			Setting.EndSpecial = IBaseAction.ShouldEndSpecial;
		}

		if (IBaseAction.AllEmpty)
		{
			usedUp = true;
		}

		if (!Info.BasicCheck(skipStatusProvideCheck, skipStatusNeed, skipComboCheck, skipCastingCheck, checkActionManagerDirectly, targetOverride))
		{
			return ActionTracer.Reject(this, "BasicCheck");
		}

		if (!Cooldown.CooldownCheck(usedUp, gcdCountForAbility))
		{
			return ActionTracer.Reject(this, "Cooldown");
		}

		if (Setting.SpecialType == SpecialActionType.MeleeRangedAttack && IActionHelper.IsLastAction(IActionHelper.MovingActions))
		{
			return ActionTracer.Reject(this, "MeleeRangedAfterMove");
		}

		if (!skipTTKCheck)
		{
			if (!DataCenter.IsPvP || (DataCenter.IsPvP && !Service.Config.IgnorePvPttk))
			{
				if (!IsTimeToKillValid())
				{
					return ActionTracer.Reject(this, "TTKBelowThreshold");
				}
			}
		}
		PreviewTarget = TargetInfo.FindTarget(skipAoeCheck, skipStatusProvideCheck, skipTargetStatusNeedCheck, targetOverride);
		if (PreviewTarget == null)
		{
			return ActionTracer.Reject(this, "NoTarget");
		}

		// A defence that protects only the player is spent only on a hit that reaches him. "Only the
		// player" is read from the game data and the target: it lands on him and has no effect radius -
		// Rampart, Damnation, Arm's Length, a barrier aimed at himself - while Reprisal, Shake It Off or
		// Heart of Corundum on the other tank reach beyond him and stay free (concept 13, A233).
		if (IBaseAction.SelfProtectionHitsMe == false && Info.EffectRange == 0
			&& PreviewTarget.Value.Target is { } protectedOne && Player.Object is { } self
			&& protectedOne.GameObjectId == self.GameObjectId)
		{
			return ActionTracer.Reject(this, "ProtectsOnlyYouAndTheHitMissesYou");
		}

		// An outward mitigation in the single defence - Reprisal, which lowers what the enemies around the player deal -
		// when the hit misses him and goes only to members whose tankbuster needs nothing from him: under Hallowed Ground
		// past it, or a measured hit small enough (TankbusterHelp). Help aimed at those members stays out by the target
		// selection; this is the same decision for what aims at no member. A BossModReborn prediction keeps it (A374).
		if (IBaseAction.SelfProtectionHitsMe == false && !IBaseAction.ForceEnable && !IBaseAction.AreaDefenceRunning
			&& !IBaseAction.HealPathRunning && !IBaseAction.CommandedDefenceRunning && Info.EffectRange > 0
			&& DefensiveValues.For(ID) != default && TankbusterHelp.OnlyForMembersNeedingNothing())
		{
			return ActionTracer.Reject(this, "NoTankbusterHereNeedsIt");
		}

		// While the Flamethrower channel runs, any other action ends it (effect text). What a safety path asks for - a
		// heal, the defence, an interrupt, a cleanse, a raise, a provoke, a knockback guard - and what the player presses
		// still go: the channel is damage, and safety comes first (A380).
		if (DataCenter.FlamethrowerChannelRunning && !IBaseAction.ForceEnable && !SafetyMayEndAChannel(ID))
		{
			return ActionTracer.Reject(this, "KeepsTheFlamethrowerChannel");
		}

		// While the tankbuster plan or an invulnerability makes it unneeded, no rated defence aimed at the
		// player goes out beyond what the plan spends - own mitigation, own barrier, Reprisal around him
		// alike - from whichever path the rotation spends it. Help aimed at another member stays free, so do
		// the heal paths (Heart of Corundum heals a tank Superbolide left at 1 HP), the single defence while
		// the player commands it, and in the area defence what reaches beyond the player (concept 09, "Gemessener Tankbuster: das geringste
		// Mittel").
		if (IBaseAction.HoldDefenceOnSelf && !IBaseAction.ForceEnable
			&& !IBaseAction.HealPathRunning && !IBaseAction.CommandedDefenceRunning
			&& (Info.EffectRange == 0 || !IBaseAction.AreaDefenceRunning)
			&& IBaseAction.AllowedDefenceOnSelf?.Contains(ID) != true
			&& PreviewTarget.Value.Target is { } heldOne && Player.Object is { } holder
			&& heldOne.GameObjectId == holder.GameObjectId
			&& DefensiveValues.For(ID) != default)
		{
			return ActionTracer.Reject(this, "HeldForTheInvulnerability");
		}

		// No healer alive and nothing but an attack on the player opening the defence: a rated defence aimed at him
		// goes only once it is back before the next tankbuster on him is due - the hit takes more than the auto-attacks
		// in the same window, and what covers it covers those too (A361). The same exceptions as the hold above.
		if (IBaseAction.KeepDefenceForTankbusterIn is { } due && !IBaseAction.ForceEnable
			&& !IBaseAction.HealPathRunning && !IBaseAction.CommandedDefenceRunning
			&& (Info.EffectRange == 0 || !IBaseAction.AreaDefenceRunning)
			&& PreviewTarget.Value.Target is { } keptOne && Player.Object is { } keeper
			&& keptOne.GameObjectId == keeper.GameObjectId
			&& DefensiveValues.For(ID) != default
			&& !BackBefore(due))
		{
			return ActionTracer.Reject(this, "KeptForTheNextTankbuster");
		}

		// Help aimed at another member that takes away what the tankbuster plan cannot do without - the
		// same button, recast group or gauge - waits for the plan's member, unless the member it helps wins
		// the triage of the owner's rule: a healer in the critical class, or a tank there while the player
		// is not (concept 09, "Andere Pfade, die dem Plan ein Glied nehmen", K2-K4). A command is his decision.
		if (!IBaseAction.ForceEnable && Setting.IsFriendly
			&& PreviewTarget.Value.Target is { } helped && Player.Object is { } helper
			&& helped.GameObjectId != helper.GameObjectId
			&& CustomRotation.HelpTakesNeededMember(this) && !CustomRotation.HelpWinsTriage(helped))
		{
			return ActionTracer.Reject(this, "TakesWhatTheTankbusterPlanNeeds");
		}

		// A damage area of its own stays where it is laid; a pack still on the move walks out of it (the owner's rule,
		// 25.10.2026, concept 04, A350). Laid at the player's feet or around the target, as the action's range says. An
		// action reached only through another button - a ninjutsu, a step of Ten Chi Jin - cannot sit on a hotbar
		// (IsPlayerAction): it is decided where its sequence begins, and the rotation asks there. An area that is only the
		// additional effect of a hit is weighed by the rotation against the hit (ActionTooltip.LaysDamageArea).
		if (Service.Config.DamageAreasWhereEnemiesStay && !IBaseAction.ForceEnable && Action.IsPlayerAction
			&& ActionTooltip.LaysDamageArea(ID)
			&& !CustomRotation.DamageAreaSettled(Info.Range > 0 ? PreviewTarget.Value.Target?.Position : Player.Object?.Position,
				Info.EffectRange))
		{
			return ActionTracer.Reject(this, "EnemiesInTheAreaNotSettled");
		}

		Target = PreviewTarget.Value;

		ActionTracer.Accept(this);
		return true;
	}

	// Whether this action, pressed now, is ready again before the next tankbuster on the player needs it: a charge left
	// after the press, or one charge's recast over before the hit less what of the action's own duration must stand
	// before it (pressed later than that, it would run out before the hit). Positive infinity: no tankbuster is due.
	private bool BackBefore(float due)
	{
		if (float.IsPositiveInfinity(due) || Cooldown.CurrentCharges > 1)
		{
			return true;
		}

		var lasts = DefensiveValues.DurationOf(ID);
		var window = lasts > 0f ? Math.Max(0f, lasts - DataCenter.DefaultGCDTotal) : 0f;
		return Cooldown.RecastTimeOneChargeRaw <= due - window;
	}

	private bool IsTimeToKillValid()
	{
		return Config.TimeToKill <= 0 || !DataCenter.IsAverageTTKKnown || DataCenter.AverageTTK >= Config.TimeToKill;
	}

	/// <inheritdoc/>
	public unsafe bool Use()
	{
		if (Player.Object == null)
		{
			return false;
		}

		var actionManager = ActionManager.Instance();
		if (actionManager == null)
		{
			return false;
		}

		var target = Target;
		var adjustId = AdjustedID;
		if (TargetInfo.IsTargetArea)
		{
			if (adjustId != ID || !target.Position.HasValue)
			{
				return false;
			}

			var loc = target.Position.Value;
			return actionManager->UseActionLocation(ActionType.Action, ID, Player.Object.GameObjectId, &loc);
		}

		var targetId = target.Target?.GameObjectId ?? Player.Object.GameObjectId;
		if (targetId == 0 || Svc.Objects.SearchById(targetId) == null)
		{
			return false;
		}

		var used = actionManager->UseAction(ActionType.Action, adjustId, targetId);

		// The tankbuster plan counts its own press from this moment, before the server confirms it
		// (concept 09, "Die Zustände je Wirken"). Its round trip is measured on the request itself, when it
		// leaves (ActionManagerEx).
		if (used)
		{
			TankbusterForecast.RecordOwnPress(adjustId, targetId);
			SleepInterruptWatch.RecordPress(adjustId, Info.CastTime);
		}

		return used;
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		return Name;
	}

	// The paths that answer a safety call, and the player's own press: they may end a damage channel (A380).
	private static bool SafetyMayEndAChannel(uint id)
	{
		return IBaseAction.HealPathRunning || IBaseAction.AreaDefenceRunning || IBaseAction.CommandedDefenceRunning
			|| IBaseAction.SelfProtectionHitsMe != null
			|| IBaseAction.TargetOverride is TargetType.Interrupt or TargetType.Dispel or TargetType.Heal or TargetType.BeAttacked
				or TargetType.Death or TargetType.Provoke or TargetType.TankSwap
			|| DataCenter.MergedStatus.HasFlag(AutoStatus.AntiKnockback)
			|| DataCenter.CommandNextAction?.ID == id;
	}
}
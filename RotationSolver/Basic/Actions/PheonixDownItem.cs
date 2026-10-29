using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace RotationSolver.Basic.Actions;

internal class PhoenixDownItem : BaseItem
{
	// The feather's cast from the Item sheet: 8 s (CastTimeSeconds of item 4570, read 26.10.2026). Since patch 7.3 it can be
	// thrown in combat in four-player content, and for that whole cast nothing else of the player's goes out (A372).
	private readonly float _castTime;

	private string _lastWait = string.Empty;

	public PhoenixDownItem(Item item) : base(item)
	{
		_castTime = item.CastTimeSeconds;
		ItemCheck = () =>
		{
			var target = DataCenter.DeathTarget;
			return target != null && TargetAcceptsTheFeather(target) && CastCanFinish();
		};
	}

	/// <summary>
	/// A raise with this feather is due: the setting allows it, a feather is in the bag, and the game would take it on the
	/// body RSR picked - its recast included, which the feather shares with every medicine.
	/// </summary>
	internal bool RaisePending => CanUseThis && HasIt && DataCenter.DeathTarget is { } target && TargetAcceptsTheFeather(target);

	/// <summary>
	/// Whether the player is expected to live through the feather's cast: his health is not falling on balance, or the
	/// self-correcting forecast puts his fall past the cast (<see cref="ObjectHelper.GetCorrectedTTK"/>).
	/// </summary>
	internal bool PlayerOutlivesTheCast
	{
		get
		{
			var ttk = ECommons.GameHelpers.Player.Object?.GetCorrectedTTK() ?? float.NaN;
			return float.IsNaN(ttk) || ttk <= 0f || ttk > _castTime;
		}
	}

	/// <summary>
	/// Whether the feather's cast can run to its end. The game does not start a cast while the player moves, and refused
	/// it in every frame RSR asked; during the cast no defence of his goes out, so a tankbuster on him landing inside it
	/// would meet him without the mitigation planned for it; and a player who falls before the cast ends raises nobody.
	/// The feather waits in each case and goes once it has passed.
	/// </summary>
	private bool CastCanFinish()
	{
		if (_castTime <= 0f)
		{
			return true;
		}

		var wait = DataCenter.IsMoving
			? "you are moving"
			: DataCenter.IsHostileCastingTankBusterAtMe
				|| TankbusterForecast.NextOnPlayerIn <= _castTime + Math.Max(0f, TankbusterForecast.RoundTrip)
				? "a tankbuster on you lands within its cast"
				: !PlayerOutlivesTheCast
					? "you would fall within its cast"
					: string.Empty;

		if (wait != _lastWait)
		{
			_lastWait = wait;
			if (wait.Length > 0)
			{
				DefenseTrace.Line($"Phoenix Down waits ({_castTime:F0} s cast): {wait}");
			}
		}

		return wait.Length == 0;
	}

	/// <summary>
	/// Would the game let this feather go out on this corpse, right now?
	///
	/// This used to ask <see cref="ObjectHelper.CanBeRaised"/>, which runs CanUseActionOnTarget
	/// against the Raise spell. That is the wrong question for an item, and it is wrongest for the
	/// only people a feather is for: a tank or a damage job has no Raise. The client's action status
	/// answers for level and learning - BadStatus carries 573, "Not learned or not high enough
	/// level", for exactly that reason - so a check phrased as a spell can fail on the carrier
	/// rather than on the corpse.
	///
	/// Asking about item 4570 with the corpse as the target moves the whole question to the game:
	/// range, line of sight, whether the target is a raisable corpse at all, whether this duty
	/// forbids items, and whether a raise is already pending on them. The last one matters most in
	/// play - between the feather going out and the Raise status coming back, the corpse still
	/// reads as dead on our side, and only the game knows the difference.
	///
	/// A status of exactly 0 means usable with no qualification. The looser BadStatus list is right
	/// for the untargeted check <see cref="BaseItem.CanUse"/> makes and wrong here: a target-related
	/// refusal is not in that list, so it would read as permission.
	/// </summary>
	private unsafe bool TargetAcceptsTheFeather(IBattleChara target)
	{
		var manager = ActionManager.Instance();
		if (manager == null)
		{
			return false;
		}

		// Use() spends the HQ stack first when there is one, so ask about the same one it would use.
		var inventory = InventoryManager.Instance();
		var hq = inventory != null && inventory->GetInventoryItemCount(ID, true) > 0;

		return manager->GetActionStatus(ActionType.Item, hq ? ID + 1000000 : ID, target.GameObjectId) == 0;
	}

	protected override bool CanUseThis => Service.Config.UsePhoenixDown && ((Service.Config.UsePhoenixDownHealerLogic && !DataCenter.AnyLivingRaiser(excludeSelf: false)) || !Service.Config.UsePhoenixDownHealerLogic) && DataCenter.DeathTarget != null;
}

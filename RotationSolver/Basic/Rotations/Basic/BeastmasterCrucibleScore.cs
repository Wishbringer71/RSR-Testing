using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace RotationSolver.Basic.Rotations.Basic;

/// <summary>
/// Concept 21, K15 (A689): the bonuses of the board's score that a step of RSR would forfeit, kept while the run is safe
/// without that step. What stands is read per run from what RSR sees; the game shows the bonuses only at the end.
/// </summary>
public partial class BeastmasterRotation
{
	private static int _scoreRun = -1;
	private static int _scoreItemsHeld = -1;
	private static bool _scoreItemUsed;
	private static bool? _scoreKeepTraced;
	private static bool _scoreKeep;
	private static DateTime _scoreKeepAt = DateTime.MinValue;

	/// <summary>
	/// K15: whether no crucible item has been used in this run - the bonus "Sparsamkeit" (Economic Ethos, XBMScoreBonus row
	/// 15) still stands. A use by RSR or by hand shows as one item less in the board's inventory.
	/// </summary>
	internal static bool NoItemBonusStands => !_scoreItemUsed;

	/// <summary>K15: follows the run's inventory; called every frame the board's director is read.</summary>
	private static unsafe void FollowScore(InstanceContentCrucibleOfTheUnbroken* crucible)
	{
		if (CrucibleBoard.Run != _scoreRun)
		{
			_scoreRun = CrucibleBoard.Run;
			_scoreItemsHeld = -1;
			_scoreItemUsed = false;
			_scoreKeepTraced = null;
			_scoreKeepAt = DateTime.MinValue;
		}

		var held = 0;
		for (var i = 0; i < crucible->Inventory.Length; i++)
		{
			if (crucible->Inventory[i].ItemId != 0)
			{
				held++;
			}
		}

		if (_scoreItemsHeld >= 0 && held < _scoreItemsHeld && !_scoreItemUsed)
		{
			_scoreItemUsed = true;
			DefenseTrace.Line("crucible score: an item went from the inventory - the no-item bonus (Sparsamkeit) is gone for this run");
		}

		_scoreItemsHeld = held;
	}

	/// <summary>
	/// K15: whether the items that guard against no present danger - insurance, damage, healing nothing needs - stay in the
	/// inventory for the no-item bonus: it still stands, and the player's HP carry the heaviest way to the final fight,
	/// included, without a rest (A661's learned loads with the gear worn). Unknown loads keep nothing - the safe side.
	/// </summary>
	private static bool KeepsNoItemBonus(IBattleChara player)
	{
		if (_scoreItemUsed || player.MaxHp == 0)
		{
			return false;
		}

		// Weighed again at most once a GCD: the way ahead is walked anew each time.
		var now = DateTime.Now;
		if ((now - _scoreKeepAt).TotalSeconds < DataCenter.DefaultGCDTotal)
		{
			return _scoreKeep;
		}

		_scoreKeepAt = now;
		var load = CrucibleRoute.WorstAhead(false, LoadNow);
		var share = player.CurrentHp / (float)player.MaxHp;
		var keep = load is { } heaviest && share > heaviest;
		if (_scoreKeepTraced != keep)
		{
			_scoreKeepTraced = keep;
			DefenseTrace.Line(keep
				? $"crucible score: items kept for the no-item bonus (Sparsamkeit) - you at {share:P0} carry the heaviest way to the final fight, {load:P0}"
				: load is { } need
					? $"crucible score: items go again - the heaviest way to the final fight, {need:P0}, is more than your {share:P0}"
					: "crucible score: items go again - a fight on the way to the final fight was never measured");
		}

		_scoreKeep = keep;
		return keep;
	}
}

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

	private static int _scoreShopRun = -1;
	private static bool _scoreTokensSpent;

	/// <summary>
	/// K15: whether no territory token has been spent in this run - the bonus "Geizhals" (Coin Clutcher, XBMScoreBonus row 19)
	/// still stands. A purchase shows as a row the open shop lists as bought.
	/// </summary>
	private static bool NoSpendBonusStands(int boughtRows)
	{
		if (CrucibleBoard.Run != _scoreShopRun)
		{
			_scoreShopRun = CrucibleBoard.Run;
			_scoreTokensSpent = false;
		}

		if (boughtRows > 0 && !_scoreTokensSpent)
		{
			_scoreTokensSpent = true;
			DefenseTrace.Line("crucible score: a purchase in this run - the no-spending bonus (Geizhals) is gone");
		}

		return !_scoreTokensSpent;
	}

	/// <summary>
	/// K15: whether the player's HP now, and with <paramref name="withHeldHealing"/> the healing items held, carry the
	/// heaviest way to the final fight, included, without a rest - the learned loads with the gear worn (A661). False while a
	/// fight on the way was never measured: the safe side.
	/// </summary>
	private static bool CarriesToFinal(bool withHeldHealing, out float share, out float? load)
	{
		var player = Player;
		share = 0f;
		load = null;
		if (player == null || player.MaxHp == 0)
		{
			return false;
		}

		load = CrucibleRoute.WorstAhead(false, LoadNow);
		share = (player.CurrentHp / (float)player.MaxHp) + (withHeldHealing ? HealingHeldShare() : 0f);
		return load is { } heaviest && share > heaviest;
	}

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
		var keep = CarriesToFinal(false, out var share, out var load);
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

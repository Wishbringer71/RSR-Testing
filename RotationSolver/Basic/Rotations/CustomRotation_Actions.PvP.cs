using FFXIVClientStructs.FFXIV.Client.Game;

namespace RotationSolver.Basic.Rotations;

public partial class CustomRotation
{
	/// <summary>
	/// Whether Recuperate may go now (concept 25, "PvP: Gemeinsame Aktionen", A756): at least its healing is missing - read
	/// from the client's effect text ("Cure Potency: 16,000"; one potency is one HP in PvP), where upstream had 15,000 from
	/// before 7.5 - and it leaves the MP for one Purify, unless the player stands in the critical class. Until A756
	/// Recuperate spent the MP to the last, and a stun after it could no longer be purified. Without a readable text the
	/// heal is unknown, and Recuperate goes only in the critical class.
	/// </summary>
	private static unsafe bool RecuperatePvPAllowed()
	{
		var player = Player;
		if (player == null || player.MaxHp == 0)
		{
			return false;
		}

		var critical = player.IsInCriticalClass();
		if (!ActionTooltip.TryStatedCurePotency((uint)ActionID.RecuperatePvP, out var heal))
		{
			return critical;
		}

		if (player.MaxHp - player.CurrentHp < heal)
		{
			return false;
		}

		if (critical)
		{
			return true;
		}

		var recuperate = ActionManager.GetActionCost(ActionType.Action, (uint)ActionID.RecuperatePvP, 0, 0, 0, 0);
		var purify = ActionManager.GetActionCost(ActionType.Action, (uint)ActionID.PurifyPvP, 0, 0, 0, 0);
		return player.CurrentMp >= recuperate + purify;
	}
}

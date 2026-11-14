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
	private static bool RecuperatePvPAllowed()
	{
		var player = Player;
		if (player == null || player.MaxHp == 0)
		{
			return false;
		}

		if (!ActionTooltip.TryStatedCurePotency((uint)ActionID.RecuperatePvP, out var heal))
		{
			return player.IsInCriticalClass();
		}

		return player.MaxHp - player.CurrentHp >= heal && PvPMpLeavesPurify((uint)ActionID.RecuperatePvP, player);
	}

	/// <summary>
	/// Whether spending MP on <paramref name="actionId"/> for <paramref name="target"/> leaves enough for one Purify: in PvP
	/// Recuperate, Purify and Haelan draw on the same MP, and a heal spent to the last MP leaves a stun that nothing lifts
	/// (concept 25, "PvP: Wechselwirkungen und Synergien", A757). A target in the critical class goes first: its death is
	/// certain, the stun only possible. Both costs come from the game.
	/// </summary>
	private static unsafe bool PvPMpLeavesPurify(uint actionId, IBattleChara? target)
	{
		var player = Player;
		if (player == null)
		{
			return false;
		}

		if (target != null && target.IsInCriticalClass())
		{
			return true;
		}

		var cost = ActionManager.GetActionCost(ActionType.Action, actionId, 0, 0, 0, 0);
		var purify = ActionManager.GetActionCost(ActionType.Action, (uint)ActionID.PurifyPvP, 0, 0, 0, 0);
		return player.CurrentMp >= cost + purify;
	}
}

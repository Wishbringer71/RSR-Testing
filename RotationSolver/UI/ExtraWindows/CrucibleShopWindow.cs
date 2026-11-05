using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using RotationSolver.Basic.Rotations.Basic;

namespace RotationSolver.UI.ExtraWindows;

/// <summary>
/// K10 of the Crucible (concept 01, Laden, A569): while a shop on a board is open and the beastmaster rotation runs, what RSR
/// recommends buying, why, what it weighs the protective gear against, and a button that buys the list. Nothing is bought
/// without the click (decision C, A533).
/// </summary>
internal class CrucibleShopWindow : Window
{
	private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;

	public CrucibleShopWindow() : base("RSR - Crucible shop", WindowFlags)
	{
		IsOpen = true;
		RespectCloseHotkey = false;
	}

	public override bool DrawConditions()
	{
		return Svc.ClientState.IsLoggedIn && DataCenter.CurrentRotation is BeastmasterRotation && BeastmasterRotation.CrucibleShopOpen;
	}

	public override void Draw()
	{
		var picks = BeastmasterRotation.CrucibleShopRecommendation;
		var (tokens, left) = BeastmasterRotation.CrucibleShopTokens;
		ImGui.TextUnformatted(picks.Count > 0
			? $"Recommended ({tokens} tokens, {left} left after these):"
			: $"Nothing recommended ({tokens} tokens): nothing here adds safety within your tokens and slots.");
		foreach (var pick in picks)
		{
			ImGui.TextUnformatted($"{pick.Name} ({pick.Price}): {pick.Why}");
		}

		if (BeastmasterRotation.CrucibleShopBasis.Length > 0)
		{
			ImGui.TextUnformatted($"Gear weighed against {BeastmasterRotation.CrucibleShopBasis}.");
		}

		if (picks.Count > 0 && ImGui.Button("Buy"))
		{
			BeastmasterRotation.RequestCrucibleShop();
		}

		if (BeastmasterRotation.CrucibleShopStatus.Length > 0)
		{
			ImGui.TextUnformatted(BeastmasterRotation.CrucibleShopStatus);
		}
	}
}

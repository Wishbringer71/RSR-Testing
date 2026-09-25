using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.UI;

/// <summary>
/// The fork's diagnostic lines, where they can be read during a fight.
///
/// They used to live only in the configuration window - the AoE store and the last rated hit in the
/// list tab, the potion reason in the action detail behind Debug Mode, the rotation's own status in
/// the rotation tab - and nobody has that window open while fighting. A line that is only visible
/// when the fight is over is the later evaluation the owner ruled out; this window shows the same
/// lines while the decision is being made.
/// </summary>
internal class DiagnosticsWindow : Window
{
	private const ImGuiWindowFlags BaseFlags = ControlWindow.BaseFlags
		| ImGuiWindowFlags.AlwaysAutoResize
		| ImGuiWindowFlags.NoCollapse
		| ImGuiWindowFlags.NoResize;

	public DiagnosticsWindow()
		: base("RSR Diagnostics###" + nameof(DiagnosticsWindow), BaseFlags)
	{
	}

	public override void PreDraw()
	{
		ImGui.PushStyleColor(ImGuiCol.WindowBg, Service.Config.InfoWindowBg);

		Flags = BaseFlags;
		if (Service.Config.IsInfoWindowNoInputs)
		{
			Flags |= ImGuiWindowFlags.NoInputs;
		}
		if (Service.Config.IsInfoWindowNoMove)
		{
			Flags |= ImGuiWindowFlags.NoMove;
		}
		base.PreDraw();
	}

	public override void PostDraw()
	{
		ImGui.PopStyleColor();
		base.PostDraw();
	}

	public override void Draw()
	{
		var rotation = DataCenter.CurrentRotation;
		if (rotation != null)
		{
			ImGui.TextColored(ImGuiColors.DalamudViolet, rotation.Name);
			rotation.DisplayRotationStatus();
			ImGui.Separator();
		}

		ImGui.TextColored(ImGuiColors.DalamudViolet, "AoE damage table");
		ImGui.Text($"Damage potential recorded: {OtherConfiguration.HostileCastingAreaPotential.Count}"
			+ $" of {OtherConfiguration.HostileCastingArea.Count}");
		var store = OtherConfiguration.AreaPotentialStoreState;
		ImGui.TextColored(
			store.Contains("FAILED") || store.Contains("MISMATCH") ? ImGuiColors.DalamudRed : ImGuiColors.DalamudGrey,
			"Store: " + store);
		ImGui.TextColored(ImGuiColors.DalamudGrey, "Last hit: " + DataCenter.AreaMeasurementLastOutcome);
		ImGui.Separator();

		ImGui.TextColored(ImGuiColors.DalamudViolet, "Area heal around you");
		if (DataCenter.LastSelfCentredHeal is { } heal)
		{
			var verdict = heal.CleaveBlocked
				? "held by the Cleave setting"
				: heal.HurtInRadius < heal.Required
					? "too few hurt in the radius"
					: heal.InNeed ? "someone in the radius is under its heal ratio - it may go" : "nobody in the radius under its heal ratio";
			ImGui.Text($"{heal.Action}: {heal.HurtInRadius} hurt in the radius, {heal.Required} asked for - {verdict}"
				+ $" ({(DateTime.Now - heal.At).TotalSeconds:F0} s ago)");
		}
		else
		{
			ImGui.TextColored(ImGuiColors.DalamudGrey, "No area heal around you has been weighed for healing yet.");
		}
		ImGui.Separator();

		// The same verdict the AoE list shows, here because that list is closed during a fight.
		if (Service.Config.HoldProactiveMitigationForSmallCast)
		{
			var (vindicated, wasted) = DataCenter.ProactiveHoldRecord;
			ImGui.Text("Predicted mitigation held for a small cast, this fight: "
				+ $"{DataCenter.ProactiveMitigationHeld.Count} action(s), {vindicated} followed by a big hit, {wasted} not"
				+ (vindicated + wasted > 0 && !DataCenter.ProactiveHoldIsEarningItsKeep ? " - stood down, wrong more often than right" : string.Empty));
			ImGui.Separator();
		}

		ImGui.TextColored(ImGuiColors.DalamudViolet, "Movement safety");
		if (DataCenter.LastMoveSafetyRefusal is { } refusal)
		{
			ImGui.Text($"Last withheld: {refusal.Action} - {refusal.Why} ({(DateTime.Now - refusal.At).TotalSeconds:F0} s ago)");
		}
		else
		{
			ImGui.TextColored(ImGuiColors.DalamudGrey, Service.Config.BmrSafetyCheckAuto
				? "Nothing withheld yet. A gap closer at 0 y to its target is never withheld."
				: "The check is off: nothing is withheld.");
		}
		ImGui.Separator();

		ImGui.TextColored(ImGuiColors.DalamudViolet, "HP potions");
		var gate = !DataCenter.InCombat
			? "out of combat - potions are not offered"
			: DataCenter.IsHostileCastingTankBusterAtMe || DataCenter.BMRTankbusterImminent
				? "in combat, tankbuster: the HP threshold is dropped"
				: "in combat: the HP threshold applies";
		ImGui.Text("Trigger: " + gate);

		var anyEnabled = false;
		foreach (var potion in CustomRotation.HpPotions)
		{
			if (!potion.IsEnabled)
			{
				continue;
			}

			anyEnabled = true;
			ImGui.Text($"{potion.Name}: {potion.DescribeBlock()}");
		}

		if (!anyEnabled)
		{
			ImGui.TextColored(ImGuiColors.DalamudGrey, "No HP potion is enabled in the action list.");
		}
	}
}

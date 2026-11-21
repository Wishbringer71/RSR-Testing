using ECommons.EzIpcManager;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace RotationSolver.Integrations.IPC;

// Concept 02, "Bewegung in die Seite über BossMods Bewegungsmodul" (A928): BossMod's presets, through which RSR switches
// its "Automatic movement" on for the window a positional needs (PositionalMove).
internal static partial class BMRInfo_IPCSubscriber
{
	// Adds or replaces a preset from its serialized form; false when BossMod cannot read it.
	[EzIPC("Presets.Create", true)]
	internal static readonly Func<string, bool, bool>? PresetsCreate;

	// The name of the active preset; null without one.
	[EzIPC("Presets.GetActive", true)]
	internal static readonly Func<string?>? PresetsGetActive;

	[EzIPC("Presets.SetActive", true)]
	internal static readonly Func<string, bool>? PresetsSetActive;

	[EzIPC("Presets.ClearActive", true)]
	internal static readonly Func<bool>? PresetsClearActive;

	// Preset, module type, track, option: a value that holds until the preset is changed or cleared.
	[EzIPC("Presets.AddTransientStrategy", true)]
	internal static readonly Func<string, string, string, string, bool>? PresetsAddTransientStrategy;
}

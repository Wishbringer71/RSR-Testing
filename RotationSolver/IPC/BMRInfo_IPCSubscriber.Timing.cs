using ECommons.EzIpcManager;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace RotationSolver.IPC;

internal static partial class BMRInfo_IPCSubscriber
{
	// BossMod's configuration: read a field (two arguments) or set it without saving it (three and false); an explicit set
	// replaces a transient override of that field. RSR takes BossMod's lock tweaks over with them (concept 03, "Weben,
	// Animationssperre und Latenz", A753).
	[EzIPC("Configuration", true)]
	internal static readonly Func<List<string>, bool, List<string>>? Configuration;

	// Sets a field for the session only; the player's value is kept and comes back with an explicit set or a clear.
	[EzIPC("Configuration.SetTransient", true)]
	internal static readonly Func<List<string>, List<string>>? SetTransient;
}

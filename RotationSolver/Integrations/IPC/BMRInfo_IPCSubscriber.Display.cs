using ECommons.EzIpcManager;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace RotationSolver.Integrations.IPC;

// Concept 13, "Flächenanzeige" (A921): whether BossMod projects its module into the world is read with Configuration
// (BMRInfo_IPCSubscriber.Timing.cs) and kept until BossMod's settings change.
internal static partial class BMRInfo_IPCSubscriber
{
	// When BossMod's settings last changed.
	[EzIPC("Configuration.LastModified", true)]
	internal static readonly Func<DateTime>? ConfigurationLastModified;
}

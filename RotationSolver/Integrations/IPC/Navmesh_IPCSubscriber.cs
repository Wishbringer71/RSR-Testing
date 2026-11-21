using ECommons.EzIpcManager;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace RotationSolver.Integrations.IPC;

// Concept 05, "Arenagrenzen ohne BossMod-Modul" (A934): vnavmesh's mesh, built from the zone's collision, says whether a
// landing point has floor under it (awgil/ffxiv_navmesh 38d2a24, IPCProvider).
internal static class Navmesh_IPCSubscriber
{
	private static readonly EzIPCDisposalToken[] _disposalTokens =
		EzIPC.Init(typeof(Navmesh_IPCSubscriber), "vnavmesh", SafeWrapper.AnyException);

	internal static bool IsEnabled => IPCSubscriber_Common.IsReady("vnavmesh");

	[EzIPC("Nav.IsReady", true)]
	internal static readonly Func<bool>? IsReady;

	// Point, half extent in height, whether polygons unreachable from the start count: whether mesh lies under the point.
	[EzIPC("Query.Mesh.IsPointOnMesh", true)]
	internal static readonly Func<Vector3, float, bool, bool>? IsPointOnMesh;

	internal static void Dispose() => IPCSubscriber_Common.DisposeAll(_disposalTokens);
}

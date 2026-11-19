using ECommons.GameHelpers;
using RotationSolver.Engine.Data;
using RotationSolver.Integrations.IPC;

namespace RotationSolver.Engine.State;

internal static partial class DataCenter
{
	// Whether vnavmesh can answer for this frame: running, its mesh ready, and the player himself standing on it - read
	// once a frame, since several movement actions may ask in the same frame (concept 24). Built on first use: a static
	// initializer's order across the partial files is not fixed.
	private static FrameCached<bool>? _navmeshDescribesArena;

	private static bool NavmeshDescribesArena()
	{
		try
		{
			return Navmesh_IPCSubscriber.IsEnabled && Navmesh_IPCSubscriber.IsReady?.Invoke() == true
				&& Player.Object is { } player
				&& Navmesh_IPCSubscriber.IsPointOnMesh?.Invoke(player.Position, player.HitboxRadius, true) == true;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Concept 05, "Arenagrenzen ohne BossMod-Modul" (A934): whether vnavmesh's mesh lies under <paramref name="point"/> -
	/// floor, not an abyss or a hole in the arena. Null when nothing can say: vnavmesh is not running or has no mesh for
	/// this zone, or the player himself does not stand on the mesh, which then does not describe this arena (a platform a
	/// phase brings). Floor farther below the point than <paramref name="heightReach"/> counts as none.
	/// </summary>
	internal static bool? FloorAt(Vector3 point, float heightReach)
	{
		if (!(_navmeshDescribesArena ??= new(NavmeshDescribesArena, FrameCacheMs)).Value || Navmesh_IPCSubscriber.IsPointOnMesh is not { } onMesh)
		{
			return null;
		}

		try
		{
			return onMesh(point, heightReach, true);
		}
		catch
		{
			return null;
		}
	}
}

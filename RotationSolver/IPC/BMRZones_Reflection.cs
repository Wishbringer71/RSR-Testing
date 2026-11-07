using System.Collections;
using System.Reflection;
using ECommons.Reflection;

namespace RotationSolver.IPC;

/// <summary>
/// BossModReborn's forbidden zones with their activation, read from its hints object (concept 03, "Bewegungssperre beim
/// Wirken und die KI von BossMod", A652). The IPC names only the earliest zone anywhere
/// (<c>Hints.ForbiddenZonesNextActivation</c>), so a channel ended for a zone elsewhere; the hints carry each zone's shape
/// and activation (<c>AIHints.ForbiddenZones</c>, BossmodReborn 27be422). Any failure - BossMod absent, a field renamed, a
/// shape that throws - gives null until BossMod is loaded anew, and the caller falls back to the IPC, which only lets go
/// earlier, never later.
/// </summary>
internal static class BMRZones_Reflection
{
	private const string PluginName = "BossModReborn";

	private static object? _plugin;
	private static bool _usable;
	private static FieldInfo? _hints;
	private static FieldInfo? _zones;
	private static FieldInfo? _shape;
	private static FieldInfo? _activation;
	private static ConstructorInfo? _position;
	private static MethodInfo? _contains;

	/// <summary>
	/// Seconds until the earliest of BossMod's forbidden zones that covers <paramref name="position"/> turns active - zero or
	/// less when one is active now, float.MaxValue when none covers it; null when the zones cannot be read.
	/// </summary>
	internal static float? ActivationAt(Vector3 position)
	{
		try
		{
			if (!DalamudReflector.TryGetDalamudPlugin(PluginName, out var plugin, true, false))
			{
				return null;
			}

			if (!ReferenceEquals(plugin, _plugin))
			{
				_plugin = plugin;
				_usable = Resolve(plugin);
			}

			if (!_usable || _hints!.GetValue(plugin) is not { } hints || _zones!.GetValue(hints) is not IList zones)
			{
				return null;
			}

			object?[] at = [_position!.Invoke([position.X, position.Z])];
			var now = DateTime.Now;
			var earliest = float.MaxValue;
			for (var i = 0; i < zones.Count; i++)
			{
				var zone = zones[i];
				if (zone == null || _shape!.GetValue(zone) is not { } shape || _contains!.Invoke(shape, at) is not true)
				{
					continue;
				}

				// BossMod measures its own IPC the same way: activation minus DateTime.Now.
				var seconds = (float)(((DateTime)_activation!.GetValue(zone)!) - now).TotalSeconds;
				if (seconds < earliest)
				{
					earliest = seconds;
				}
			}

			return earliest;
		}
		catch
		{
			_usable = false;
			return null;
		}
	}

	private static bool Resolve(object plugin)
	{
		const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		_hints = plugin.GetType().GetField("_hints", Instance);
		_zones = _hints?.FieldType.GetField("ForbiddenZones", Instance);
		var zoneType = _zones?.FieldType.IsGenericType == true ? _zones.FieldType.GetGenericArguments()[0] : null;
		_shape = zoneType?.GetField("Item1");
		_activation = zoneType?.GetField("Item2");
		var assembly = plugin.GetType().Assembly;
		var positionType = assembly.GetType("BossMod.WPos");
		_position = positionType?.GetConstructor([typeof(float), typeof(float)]);
		_contains = positionType == null ? null
			: assembly.GetType("BossMod.ShapeDistance")?.GetMethod("Contains", Instance, [positionType.MakeByRefType()]);
		return _hints != null && _zones != null && _shape != null && _activation?.FieldType == typeof(DateTime)
			&& _position != null && _contains?.ReturnType == typeof(bool);
	}
}

using ECommons.GameHelpers;
using ECommons.SplatoonAPI;
using RotationSolver.Integrations.IPC;

namespace RotationSolver.Engine.Helpers;

/// <summary>
/// Concept 13, "Flächenanzeige: eine richtige Anzeige je Wirken" (A920, A921): each enemy area cast is shown by the one
/// source that knows its shape, and no second one contradicts it.
/// <list type="number">
/// <item>With a ground marker the game draws it. A ring Splatoon's projection would draw against that marker (it reads the
/// omen's digits as yalms) goes into Splatoon's projection blacklist; the game's marker stays.</item>
/// <item>A circle or line without one: BossMod while a module runs and it projects into the world; else Splatoon's
/// projection, which draws them as RSR would, except the circles it leaves out by size; else RSR, through Splatoon
/// (<see cref="DataCenter.PlainAreaShape"/>, the shape <see cref="DataCenter.CastReachesPlayer"/> tests). An action in
/// Splatoon's blacklist RSR never draws, and a fixed circle only while one can still leave it.</item>
/// <item>Cones and rings without a marker name no angle or inner circle anywhere and are left out.</item>
/// </list>
/// Nothing happens without Splatoon.
/// </summary>
internal static class AreaDisplay
{
	private const string Namespace = "RotationSolver.Areas";
	private static readonly Dictionary<ulong, (uint Action, float Total)> _drawn = [];
	private static bool? _tracedConnected;
	private static string? _tracedSource;
	private static DateTime _bossModStamp;
	private static bool _bossModRead, _bossModInWorld;

	internal static void Update()
	{
		if (!Service.Config.DrawAreasWithoutMarker)
		{
			return;
		}

		var connected = Splatoon.IsConnected();
		if (_tracedConnected != connected)
		{
			_tracedConnected = connected;
			DefenseTrace.Line($"area display: {(connected ? "connected to Splatoon" : "Splatoon is not connected - nothing is drawn")}");
		}

		var casters = DataCenter.CastingEnemies;
		if (!connected || casters == null)
		{
			_drawn.Clear();
			return;
		}

		for (var i = 0; i < casters.Count; i++)
		{
			var h = casters[i];
			if (h == null || !h.IsValid() || !h.IsCasting || h.CastActionId == 0)
			{
				continue;
			}

			var total = h.TotalCastTime;
			if (_drawn.TryGetValue(h.GameObjectId, out var drawn) && drawn.Action == h.CastActionId && drawn.Total == total)
			{
				continue;
			}

			_drawn[h.GameObjectId] = (h.CastActionId, total);
			if (Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(h.CastActionId) is not { } act || act.EffectRange == 0)
			{
				continue;
			}

			var left = Math.Max(0f, total - h.CurrentCastTime);
			if (act.Omen.RowId != 0)
			{
				BlockMisreadRing(h, act);
			}
			else if (Drawable(h, act, left) is { } shape)
			{
				Splatoon.AddDynamicElement(Namespace, Build(shape), Environment.TickCount64 + (long)TimeSpan.FromSeconds(left).TotalMilliseconds);
			}
		}

		PruneEnded(casters);
	}

	// Concept 13: a ring Splatoon's projection draws against the game's marker goes into its blacklist, while the
	// projection draws here; the marker shows the ring.
	private static void BlockMisreadRing(IBattleChara h, Lumina.Excel.Sheets.Action act)
	{
		var territory = DataCenter.TerritoryID;
		if (SplatoonProjection.MisreadRing(act) is not { } ring || !SplatoonProjection.DrawsIn(territory)
			|| !SplatoonProjection.Block(h, act.RowId, territory))
		{
			return;
		}

		DefenseTrace.Line($"area display: {act.Name.ExtractText()} #{act.RowId} blocked in Splatoon's projection - it draws {ring.Inner}-{ring.Outer} from the marker's name, the game lays the marker to {act.EffectRange}");
	}

	// The shape RSR draws for a cast without a marker; null where another source draws it or none knows it.
	private static DataCenter.AreaShape? Drawable(IBattleChara h, Lumina.Excel.Sheets.Action act, float left)
	{
		// A ring or cone without a marker reads as a plain circle in the hit test, which stays on the safe side there;
		// drawn, it would show a shape the game does not use.
		if ((CastType)act.CastType == CastType.Donut || DataCenter.IsConeCastType(act.CastType)
			|| DataCenter.PlainAreaShape(h, act) is not { } shape || SplatoonProjection.Blacklisted(act.RowId))
		{
			return null;
		}

		if (BossModDrawsInWorld())
		{
			TraceSource("BossMod projects its module into the world - casts without a marker are left to it");
			return null;
		}

		if (SplatoonProjection.DrawsIn(DataCenter.TerritoryID) && (shape.Line || act.EffectRange < SplatoonProjection.RaidwideSize))
		{
			TraceSource("Splatoon's projection draws circles and lines here - they are left to it");
			return null;
		}

		TraceSource("RSR draws circles and lines without a marker");
		return shape.Line || shape.Follows != 0 || CanLeave(shape, left) ? shape : null;
	}

	// A fixed circle is worth drawing while one can still leave it: its depth at the player's place is less than the
	// way BossMod's speed covers in the cast time left. Without a speed every circle is drawn.
	private static bool CanLeave(DataCenter.AreaShape shape, float left)
	{
		var speed = DataCenter.BMRPlayerSpeed;
		if (speed <= 0f || Player.Object is not { } player)
		{
			return true;
		}

		var dx = player.Position.X - shape.Centre.X;
		var dz = player.Position.Z - shape.Centre.Z;
		return shape.Reach - MathF.Sqrt((dx * dx) + (dz * dz)) <= speed * left;
	}

	// BossMod's "Enable projecting radar into the 3D world" draws a running module in the world; read through its settings
	// interface again only when those change.
	private static bool BossModDrawsInWorld()
	{
		if (!DataCenter.BMRHasActiveModule)
		{
			return false;
		}

		var stamp = BMRInfo_IPCSubscriber.ConfigurationLastModified?.Invoke() ?? default;
		if (!_bossModRead || stamp != _bossModStamp || stamp == default)
		{
			_bossModRead = true;
			_bossModStamp = stamp;
			var answer = BMRInfo_IPCSubscriber.Configuration?.Invoke(["BossModuleConfig", "ProjectRadarInto3DWorld"], false);
			_bossModInWorld = answer is [var value] && bool.TryParse(value, out var on) && on;
		}

		return _bossModInWorld;
	}

	private static void TraceSource(string source)
	{
		if (!ReferenceEquals(_tracedSource, source))
		{
			_tracedSource = source;
			DefenseTrace.Line($"area display: {source}");
		}
	}

	private static Element Build(DataCenter.AreaShape shape)
	{
		Element element;
		if (shape.Line)
		{
			element = new Element(ElementType.LineBetweenTwoFixedCoordinates);
			element.SetRefCoord(shape.Centre);
			element.SetOffCoord(shape.Centre + (shape.Direction * shape.Reach));
			element.radius = shape.HalfWidth;
		}
		else if (shape.Follows != 0)
		{
			element = new Element(ElementType.CircleRelativeToActorPosition)
			{
				refActorComparisonType = RefActorComparisonType.ObjectID,
				refActorObjectID = (uint)shape.Follows,
			};
			element.radius = shape.Reach;
		}
		else
		{
			element = new Element(ElementType.CircleAtFixedCoordinates);
			element.SetRefCoord(shape.Centre);
			element.radius = shape.Reach;
		}

		element.Filled = true;
		return element;
	}

	// A caster that stopped casting is forgotten, so its next cast of the same action draws again.
	private static void PruneEnded(List<IBattleChara> casters)
	{
		if (_drawn.Count == 0)
		{
			return;
		}

		List<ulong>? gone = null;
		foreach (var id in _drawn.Keys)
		{
			var stillCasting = false;
			for (var i = 0; i < casters.Count; i++)
			{
				if (casters[i] is { } c && c.GameObjectId == id && c.IsCasting)
				{
					stillCasting = true;
					break;
				}
			}

			if (!stillCasting)
			{
				(gone ??= []).Add(id);
			}
		}

		if (gone != null)
		{
			foreach (var id in gone)
			{
				_ = _drawn.Remove(id);
			}
		}
	}
}

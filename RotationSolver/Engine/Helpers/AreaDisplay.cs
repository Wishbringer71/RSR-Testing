using ECommons.SplatoonAPI;

namespace RotationSolver.Engine.Helpers;

/// <summary>
/// Concept 13, "Flächen ohne BossMod-Modul: Splatoons Anzeige als Vorbild" (A920): an enemy cast without a ground marker
/// whose shape is a circle or a line goes to Splatoon once when it begins, drawn where the game lays it
/// (<see cref="DataCenter.PlainAreaShape"/>, the shape <see cref="DataCenter.CastReachesPlayer"/> tests), and leaves with
/// the cast. Casts with a ground marker the game draws itself; cones and rings without one name no angle or inner circle
/// anywhere and are left out. Nothing happens without Splatoon.
/// </summary>
internal static class AreaDisplay
{
	private const string Namespace = "RotationSolver.Areas";
	private static readonly Dictionary<ulong, (uint Action, float Total)> _drawn = [];
	private static bool? _tracedConnected;

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
			if (Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(h.CastActionId) is not { } act
				|| act.Omen.RowId != 0 || act.EffectRange == 0 || DataCenter.PlainAreaShape(h, act) is not { } shape)
			{
				continue;
			}

			// A ring or cone without a marker reads as a plain circle in the hit test, which stays on the safe side there;
			// drawn, it would show a shape the game does not use.
			if ((CastType)act.CastType == CastType.Donut || DataCenter.IsConeCastType(act.CastType))
			{
				continue;
			}

			var left = Math.Max(0f, total - h.CurrentCastTime);
			Splatoon.AddDynamicElement(Namespace, Build(shape), Environment.TickCount64 + (long)TimeSpan.FromSeconds(left).TotalMilliseconds);
		}

		PruneEnded(casters);
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

using Lumina.Excel.Sheets;

namespace RotationSolver.Engine.Data;

// Concept 03, "Inhalt eines Gebiets" (A807): the content a territory runs, for every job at this one place.
public partial class TerritoryInfo
{
	/// <summary>
	/// The level the territory's content syncs to, from its content finder condition; zero where it syncs to none, as in
	/// the open world. Read for the learned tables: a hit taken above it is smaller than in sync (A452).
	/// </summary>
	public byte ClassJobLevelSync { get; private set; }

	/// <summary>
	/// The territory's row of the ContentFinderCondition sheet; zero where it has none, as in the open world. Matched against
	/// the duty finder's last popped entry to tell an unrestricted party (A466).
	/// </summary>
	internal uint ContentFinderConditionId { get; private set; }

	// A807: for each territory the first ContentFinderCondition that names it, built once.
	private static Dictionary<uint, uint>? _contentByTerritory;

	/// <summary>
	/// A807: the territory's ContentFinderCondition - the one its own row names, else the first that names the territory.
	/// 39 territories name none although a condition names them (xivapi, 14.11.2026): the Crucible's five boards,
	/// Heaven-on-High's ten, the Crystalline Conflict maps and Gold Saucer games; where several name one, their content type,
	/// level sync and high-end flag agree. Row 0 where none names it, as in the open world.
	/// </summary>
	private static ContentFinderCondition ContentOf(TerritoryType territory)
	{
		if (territory.ContentFinderCondition.RowId != 0)
		{
			return territory.ContentFinderCondition.Value;
		}

		var sheet = Service.GetSheet<ContentFinderCondition>();
		if (_contentByTerritory == null)
		{
			var map = new Dictionary<uint, uint>();
			foreach (var row in sheet)
			{
				if (row.TerritoryType.RowId != 0)
				{
					_ = map.TryAdd(row.TerritoryType.RowId, row.RowId);
				}
			}

			_contentByTerritory = map;
		}

		return _contentByTerritory.TryGetValue(territory.RowId, out var content) && sheet.GetRowOrDefault(content) is { } found
			? found
			: territory.ContentFinderCondition.Value;
	}
}

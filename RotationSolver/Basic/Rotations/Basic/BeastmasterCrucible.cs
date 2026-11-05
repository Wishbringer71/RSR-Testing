using Dalamud.Game;
using ECommons.DalamudServices;

namespace RotationSolver.Basic.Rotations.Basic;

/// <summary>
/// The Crucible of the Unbroken from the game's sheets (concept 01, "Crucible of the Unbroken", stage 1, A548): the
/// enemies of each fight with their weakness, the fight in progress, and the weaknesses each familiar's Trick and Tempered
/// Release hit - read by K2, which holds a horn for enemies that come later.
/// </summary>
public partial class BeastmasterRotation
{
	// One enemy of a fight: its name, which the game's objects carry as NameId, and the XBMElement row of its weakness.
	private readonly record struct CrucibleEnemy(uint NameId, uint Weakness);

	// XBMBattleDetail has one row per fight and a subrow per enemy. EXDSchema (branch latest, 05.11.2026) orders its columns
	// Name, Icon, Action1, Action2, Resist, Strength, Intelligence, PhysResistance, MagResistance, Constitution, Weakness; the
	// built Lumina reads them as Unknown0 to Unknown10 (its named Experimental sheet is marked provisional). Null until read,
	// empty when the sheet gave nothing; the line on entering a board shows a probe.
	private static List<(uint Row, CrucibleEnemy[] Enemies)>? _crucibleFights;

	// The English name of each XBMElement row, trimmed ("Fire" ... "Slashing"), by row id.
	private static readonly Dictionary<uint, string> _elementNames = [];

	// The weaknesses each familiar's Trick and Release hit, by familiar.
	private static readonly Dictionary<BeastmasterPet, HashSet<uint>> _familiarElements = [];

	// The fight in progress: its row (0 none), the enemies there when it was told, those there now; the area traced last;
	// what K2 has traced in this fight.
	private static uint _crucibleFight;
	private static CrucibleEnemy[] _crucibleEnemies = [];
	private static readonly HashSet<uint> _crucibleAtStart = [];
	private static readonly HashSet<uint> _cruciblePresent = [];
	private static uint _crucibleTracedArea;
	private static readonly HashSet<BeastmasterPet> _crucibleHeldTraced = [];

	private static List<(uint Row, CrucibleEnemy[] Enemies)> CrucibleFights => _crucibleFights ??= ReadCrucibleFights();

	private static List<(uint Row, CrucibleEnemy[] Enemies)> ReadCrucibleFights()
	{
		var fights = new List<(uint Row, CrucibleEnemy[] Enemies)>();
		var elements = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.XBMElement>(ClientLanguage.English);
		if (elements != null)
		{
			foreach (var element in elements)
			{
				var name = element.Unknown0.ExtractText().Trim();
				if (name.Length > 0)
				{
					_elementNames[element.RowId] = name;
				}
			}
		}

		var sheet = Svc.Data.GetSubrowExcelSheet<Lumina.Excel.Sheets.XBMBattleDetail>();
		if (sheet == null)
		{
			return fights;
		}

		var enemies = new List<CrucibleEnemy>();
		foreach (var row in sheet)
		{
			enemies.Clear();
			foreach (var enemy in row)
			{
				if (enemy.Unknown0 != 0)
				{
					enemies.Add(new CrucibleEnemy(enemy.Unknown0, enemy.Unknown10));
				}
			}

			if (enemies.Count > 0)
			{
				fights.Add((row.RowId, enemies.ToArray()));
			}
		}

		return fights;
	}

	/// <summary>
	/// The weaknesses (XBMElement rows) the Trick and the Tempered Release of <paramref name="profile"/> hit: an action's
	/// element (Fire to Water), else its physical attack type (Slashing, Piercing, Blunt), matched by the English names.
	/// </summary>
	internal static HashSet<uint> ElementsOf(FamiliarProfile profile)
	{
		if (_familiarElements.TryGetValue(profile.Pet, out var known))
		{
			return known;
		}

		_ = CrucibleFights;
		var hits = new HashSet<uint>();
		var actions = Service.GetSheet<Lumina.Excel.Sheets.Action>();
		foreach (var id in (ReadOnlySpan<uint>)[profile.TrickId, profile.ReleaseId])
		{
			if (id == 0 || actions?.GetRowOrDefault(id) is not { } action)
			{
				continue;
			}

			var aspect = (Aspect)action.Aspect;
			var name = aspect is >= Aspect.Fire and <= Aspect.Water
				? aspect.ToString()
				: action.AttackType.RowId is >= (uint)AttackType.Slashing and <= (uint)AttackType.Blunt
					? ((AttackType)action.AttackType.RowId).ToString()
					: string.Empty;
			foreach (var (row, element) in _elementNames)
			{
				if (name.Length > 0 && string.Equals(element, name, StringComparison.OrdinalIgnoreCase))
				{
					_ = hits.Add(row);
				}
			}
		}

		_familiarElements[profile.Pet] = hits;
		return hits;
	}

	/// <summary>
	/// Follows the Crucible every frame (stage 1, A548): on entering a board, whether the sheets were read; in combat, which
	/// fight is in progress and who is there.
	/// </summary>
	internal static void FollowCrucible()
	{
		if (!DataCenter.IsInCrucible)
		{
			_crucibleTracedArea = 0;
			EndCrucibleFight();
			return;
		}

		var fights = CrucibleFights;
		if (_crucibleTracedArea != DataCenter.TerritoryID)
		{
			_crucibleTracedArea = DataCenter.TerritoryID;
			var count = 0;
			foreach (var (_, enemies) in fights)
			{
				count += enemies.Length;
			}

			var probe = fights.Count > 0 ? fights[0].Enemies[0] : default;
			DefenseTrace.Line(fights.Count == 0 || _elementNames.Count == 0
				? "crucible: no fights read from the sheets - holding horns for enemies to come stays off"
				: $"crucible: {fights.Count} fights with {count} enemies read from the sheets, {_elementNames.Count} weaknesses; "
					+ $"first enemy {EnemyName(probe.NameId)}, weak to {ElementName(probe.Weakness)}");
		}

		if (!DataCenter.InCombat)
		{
			EndCrucibleFight();
			return;
		}

		_cruciblePresent.Clear();
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile != null && !hostile.IsDead && hostile.NameId != 0)
			{
				_ = _cruciblePresent.Add(hostile.NameId);
			}
		}

		if (_crucibleFight != 0 || _cruciblePresent.Count == 0)
		{
			return;
		}

		// The row naming every enemy here, the one with the fewest if several do; else the one naming most of them - an enemy
		// the sheet does not list (a helper with a name of its own) must not hide the fight.
		var bestOverlap = 0;
		foreach (var (row, enemies) in fights)
		{
			var names = Names(enemies);
			var overlap = 0;
			foreach (var present in _cruciblePresent)
			{
				overlap += names.Contains(present) ? 1 : 0;
			}

			if (overlap > bestOverlap
				|| (overlap == bestOverlap && overlap > 0 && enemies.Length < _crucibleEnemies.Length))
			{
				bestOverlap = overlap;
				_crucibleFight = row;
				_crucibleEnemies = enemies;
			}
		}

		if (_crucibleFight == 0)
		{
			return;
		}

		_crucibleAtStart.UnionWith(_cruciblePresent);
		var coming = new List<string>();
		foreach (var enemy in _crucibleEnemies)
		{
			if (!_crucibleAtStart.Contains(enemy.NameId))
			{
				coming.Add($"{EnemyName(enemy.NameId)} ({ElementName(enemy.Weakness)})");
			}
		}

		DefenseTrace.Line($"crucible fight {_crucibleFight}: {bestOverlap} of {_cruciblePresent.Count} kind(s) here listed, "
			+ (coming.Count == 0 ? "nobody to come" : $"to come: {string.Join(", ", coming)}"));

		static HashSet<uint> Names(CrucibleEnemy[] enemies)
		{
			var names = new HashSet<uint>();
			foreach (var enemy in enemies)
			{
				_ = names.Add(enemy.NameId);
			}

			return names;
		}
	}

	private static void EndCrucibleFight()
	{
		_crucibleFight = 0;
		_crucibleEnemies = [];
		_crucibleAtStart.Clear();
		_cruciblePresent.Clear();
		_crucibleHeldTraced.Clear();
	}

	/// <summary>
	/// K2 (A548): <paramref name="profile"/> hits the weakness of an enemy of this fight that was not there when it began and
	/// is not there yet, and the weakness of none that is here.
	/// </summary>
	internal static bool HitsOnlyEnemiesToCome(FamiliarProfile profile)
	{
		if (_crucibleFight == 0)
		{
			return false;
		}

		var hits = ElementsOf(profile);
		var coming = false;
		foreach (var enemy in _crucibleEnemies)
		{
			if (enemy.Weakness == 0 || !hits.Contains(enemy.Weakness))
			{
				continue;
			}

			if (_cruciblePresent.Contains(enemy.NameId))
			{
				return false;
			}

			coming |= !_crucibleAtStart.Contains(enemy.NameId);
		}

		if (coming && _crucibleHeldTraced.Add(profile.Pet))
		{
			DefenseTrace.Line($"crucible: {profile.Pet} held for the enemies to come - it hits their weakness, none of those here");
		}

		return coming;
	}

	/// <summary>
	/// K2 (A548): <paramref name="profile"/> hits the weakness of an enemy here now that was not there when the fight began.
	/// </summary>
	internal static bool HitsEnemyThatCame(FamiliarProfile profile)
	{
		if (_crucibleFight == 0)
		{
			return false;
		}

		var hits = ElementsOf(profile);
		foreach (var enemy in _crucibleEnemies)
		{
			if (enemy.Weakness != 0 && hits.Contains(enemy.Weakness) && _cruciblePresent.Contains(enemy.NameId)
				&& !_crucibleAtStart.Contains(enemy.NameId))
			{
				return true;
			}
		}

		return false;
	}

	private static string EnemyName(uint nameId)
	{
		return Service.GetSheet<Lumina.Excel.Sheets.BNpcName>()?.GetRowOrDefault(nameId) is { } row
			? row.Singular.ExtractText()
			: $"#{nameId}";
	}

	private static string ElementName(uint element)
	{
		return _elementNames.TryGetValue(element, out var name) ? name : "nothing";
	}
}

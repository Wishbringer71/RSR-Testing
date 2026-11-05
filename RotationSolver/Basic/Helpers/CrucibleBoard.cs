using Dalamud.Game;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// The Crucible of the Unbroken as the game's sheets and the player's hits show it (concept 01, "Crucible of the Unbroken",
/// stage 1, A548, A549): the enemies of each fight with their weakness and stats, the fight in progress, the weakness each
/// action hits, and how much more a hit on a weakness deals - measured, since no source states it.
/// </summary>
/// <remarks>
/// <para>XBMBattleDetail has one row per fight and a subrow per enemy. EXDSchema (branch latest, 05.11.2026) orders its
/// columns Name, Icon, Action1, Action2, Resist, Strength, Intelligence, PhysResistance, MagResistance, Constitution,
/// Weakness; the built Lumina reads them as Unknown0 to Unknown10 (its named Experimental sheet is marked provisional). The
/// line on entering a board shows a probe.</para>
///
/// <para>Recognition only: the rules that read it - K2 holding a horn, B3 valuing a Trick, K3 ordering the targets - decide
/// what a figure is worth.</para>
/// </remarks>
internal static class CrucibleBoard
{
	/// <summary>One enemy of a fight: its name as the game's objects carry it, its weakness, its sheet ratings.</summary>
	internal readonly record struct Enemy(uint NameId, uint Weakness, byte Strength, byte Intelligence);

	private static List<(uint Row, Enemy[] Enemies)>? _fights;
	private static readonly Dictionary<uint, string> _elementNames = [];

	// K8 (A553): the final fight of each board, and per fight the dangerous statuses its enemies' actions inflict.
	private static readonly HashSet<uint> _finalFights = [];
	private static readonly Dictionary<uint, HashSet<uint>> _fightAilments = [];
	private static readonly Dictionary<uint, HashSet<uint>> _actionElements = [];

	private static uint _fight;
	private static Enemy[] _enemies = [];
	private static readonly HashSet<uint> _atStart = [];
	private static readonly HashSet<uint> _present = [];
	private static readonly HashSet<uint> _seen = [];
	private static uint _tracedArea;

	// K1 (A549): damage per potency of the player's and his familiars' hits on an enemy of the fight, kept apart for hits on
	// its weakness and on anything else, per action, in this area; and in this fight the damage of that side in all, and of
	// the actions hitting each weakness.
	private readonly record struct Sums(double Weak, int WeakHits, double Other, int OtherHits);

	private static readonly Dictionary<uint, Sums> _perPotency = [];
	private static uint _measuredIn;
	private static double _ownDamage;
	private static readonly Dictionary<uint, double> _ownDamageByElement = [];
	private static bool _bonusTraced;

	private static List<(uint Row, Enemy[] Enemies)> Fights => _fights ??= Read();

	/// <summary>The fight in progress: its XBMBattleDetail row, 0 while none is told.</summary>
	internal static uint Fight => _fight;

	/// <summary>The enemies of the fight in progress, as the sheet lists them.</summary>
	internal static Enemy[] Enemies => _enemies;

	/// <summary>Whether an enemy of this name was here when the fight was told.</summary>
	internal static bool WasThereAtStart(uint nameId) => _atStart.Contains(nameId);

	/// <summary>Whether an enemy of this name is here now.</summary>
	internal static bool IsHere(uint nameId) => _present.Contains(nameId);

	/// <summary>
	/// Whether an enemy of this name, listed for the fight in progress, has not been seen in it yet - it is still to come. One
	/// that came and fell does not come again (A553; before, it counted as still to come once it was gone).
	/// </summary>
	internal static bool IsToCome(uint nameId) => _fight != 0 && !_seen.Contains(nameId);

	/// <summary>
	/// Whether the fight in progress is the board's final one, against its boss: subrow 0 of the board's row in
	/// XBMContentBattle (A553). The board's items are lost after the run.
	/// </summary>
	internal static bool IsFinalFight => _fight != 0 && _finalFights.Contains(_fight);

	/// <summary>
	/// The dangerous statuses the enemies of the fight in progress inflict, as the sheet lists their two actions (A553):
	/// detrimental, and taking control from their target (it cannot act or does not act on its own will) or ending in death
	/// by their English effect text. Empty while no fight is told.
	/// </summary>
	internal static IReadOnlyCollection<uint> Ailments => _fight != 0 && _fightAilments.TryGetValue(_fight, out var ailments)
		? ailments
		: [];

	/// <summary>The XBMElement row whose English name is <paramref name="name"/>, ignoring case; 0 when none is.</summary>
	internal static uint ElementNamed(string name)
	{
		_ = Fights;
		foreach (var (row, element) in _elementNames)
		{
			if (string.Equals(element, name, StringComparison.OrdinalIgnoreCase))
			{
				return row;
			}
		}

		return 0;
	}

	private static List<(uint Row, Enemy[] Enemies)> Read()
	{
		var fights = new List<(uint Row, Enemy[] Enemies)>();
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

		var actions = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.XBMBattleDetailAction>();
		var statuses = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(ClientLanguage.English);
		var enemies = new List<Enemy>();
		foreach (var row in sheet)
		{
			enemies.Clear();
			var ailments = new HashSet<uint>();
			foreach (var enemy in row)
			{
				if (enemy.Unknown0 != 0)
				{
					enemies.Add(new Enemy(enemy.Unknown0, enemy.Unknown10, enemy.Unknown5, enemy.Unknown6));
					AddAilment(enemy.Unknown2);
					AddAilment(enemy.Unknown3);
				}
			}

			if (enemies.Count > 0)
			{
				fights.Add((row.RowId, enemies.ToArray()));
				if (ailments.Count > 0)
				{
					_fightAilments[row.RowId] = ailments;
				}
			}

			// XBMBattleDetailAction: Action, Status, ActionTarget, ActionEffectType (EXDSchema latest, 05.11.2026).
			void AddAilment(ushort action)
			{
				if (action != 0 && actions?.GetRowOrDefault(action) is { } detail && detail.Unknown1 != 0
					&& statuses?.GetRowOrDefault(detail.Unknown1) is { } status && IsDangerous(status))
				{
					_ = ailments.Add(detail.Unknown1);
				}
			}
		}

		// XBMContentBattle: one row per board, a subrow per fight naming its XBMBattleDetail row; subrow 0 is the boss.
		var boards = Svc.Data.GetSubrowExcelSheet<Lumina.Excel.Sheets.XBMContentBattle>();
		if (boards != null)
		{
			foreach (var board in boards)
			{
				foreach (var fight in board)
				{
					if (fight.SubrowId == 0 && fight.Unknown0 != 0)
					{
						_ = _finalFights.Add(fight.Unknown0);
					}
				}
			}
		}

		return fights;
	}

	// Detrimental, and it takes control from its bearer or its English effect text names death (Doom, Stone Curse).
	private static bool IsDangerous(Lumina.Excel.Sheets.Status status)
	{
		return status.StatusCategory == (byte)StatusCategory.Detrimental
			&& (status.LockActions || status.LockControl
				|| status.Description.ExtractText().Contains("death", StringComparison.OrdinalIgnoreCase));
	}

	/// <summary>
	/// The weakness (XBMElement row) an action hits: its element (Fire to Water), else its physical attack type (Slashing,
	/// Piercing, Blunt), matched by the English names; none for an action without either.
	/// </summary>
	internal static HashSet<uint> ElementsOf(uint actionId)
	{
		if (_actionElements.TryGetValue(actionId, out var known))
		{
			return known;
		}

		_ = Fights;
		var hits = new HashSet<uint>();
		if (actionId != 0 && Service.GetSheet<Lumina.Excel.Sheets.Action>()?.GetRowOrDefault(actionId) is { } action)
		{
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

		_actionElements[actionId] = hits;
		return hits;
	}

	/// <summary>The weakness of an enemy of the fight in progress, 0 when none or not in the fight.</summary>
	internal static uint WeaknessOf(uint nameId)
	{
		foreach (var enemy in _enemies)
		{
			if (enemy.NameId == nameId)
			{
				return enemy.Weakness;
			}
		}

		return 0;
	}

	/// <summary>
	/// K3 before the first measurement (A549): the larger of the sheet's Strength and Intelligence of an enemy of the fight
	/// in progress, 0 when it is not listed.
	/// </summary>
	internal static float SheetDanger(uint nameId)
	{
		foreach (var enemy in _enemies)
		{
			if (enemy.NameId == nameId)
			{
				return Math.Max(enemy.Strength, enemy.Intelligence);
			}
		}

		return 0f;
	}

	/// <summary>
	/// Follows the Crucible every frame: on entering a board, whether the sheets were read; in combat, which fight is in
	/// progress and who is there.
	/// </summary>
	internal static void Follow()
	{
		if (!DataCenter.IsInCrucible)
		{
			_tracedArea = 0;
			EndFight();
			return;
		}

		var fights = Fights;
		if (_tracedArea != DataCenter.TerritoryID)
		{
			_tracedArea = DataCenter.TerritoryID;
			var count = 0;
			foreach (var (_, enemies) in fights)
			{
				count += enemies.Length;
			}

			var probe = fights.Count > 0 ? fights[0].Enemies[0] : default;
			DefenseTrace.Line(fights.Count == 0 || _elementNames.Count == 0
				? "crucible: no fights read from the sheets - the rules reading them stay idle"
				: $"crucible: {fights.Count} fights with {count} enemies read from the sheets, {_elementNames.Count} weaknesses; "
					+ $"first enemy {EnemyName(probe.NameId)}, weak to {ElementName(probe.Weakness)}, strength {probe.Strength}, "
					+ $"intelligence {probe.Intelligence}");
			TraceTeamAndField("entering the board");
		}

		if (!DataCenter.InCombat)
		{
			EndFight();
			return;
		}

		_present.Clear();
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile != null && !hostile.IsDead && hostile.NameId != 0)
			{
				_ = _present.Add(hostile.NameId);
			}
		}

		if (_fight != 0)
		{
			_seen.UnionWith(_present);
			return;
		}

		if (_present.Count == 0)
		{
			return;
		}

		// The row naming most of the enemies here, the one with the fewest if several do: a row naming all of them wins, and
		// an enemy the sheet does not list (a helper with a name of its own) does not hide the fight.
		var bestOverlap = 0;
		foreach (var (row, enemies) in fights)
		{
			var overlap = 0;
			foreach (var present in _present)
			{
				foreach (var enemy in enemies)
				{
					if (enemy.NameId == present)
					{
						overlap++;
						break;
					}
				}
			}

			if (overlap > bestOverlap || (overlap == bestOverlap && overlap > 0 && enemies.Length < _enemies.Length))
			{
				bestOverlap = overlap;
				_fight = row;
				_enemies = enemies;
			}
		}

		if (_fight == 0)
		{
			return;
		}

		_atStart.UnionWith(_present);
		_seen.UnionWith(_present);
		var coming = new List<string>();
		foreach (var enemy in _enemies)
		{
			if (!_atStart.Contains(enemy.NameId))
			{
				coming.Add($"{EnemyName(enemy.NameId)} ({ElementName(enemy.Weakness)})");
			}
		}

		DefenseTrace.Line($"crucible fight {_fight}: {bestOverlap} of {_present.Count} kind(s) here listed, "
			+ (coming.Count == 0 ? "nobody to come" : $"to come: {string.Join(", ", coming)}"));
		TraceTeamAndField($"fight {_fight} told");
	}

	/// <summary>
	/// K11 probe (A556): what the game's agents hold on a board - the team with each familiar's HP
	/// (<c>AgentXBMPetParty</c>: <c>SelectedPetIds</c>, <c>PetCurrentHealth</c>, <c>PetMaxHealth</c>) and the board's
	/// fields (<c>AgentXBMStageDetailList</c>: mode 1 while a challenge runs, its entries with their XBMBattleDetail rows,
	/// the selection). FFXIVClientStructs has both since 17.09.2026 (d44bc123, 3120c745); whether the game fills them while
	/// a board is played and the windows are closed is not stated anywhere, so the line shows it. Fields only, no calls.
	/// </summary>
	private static unsafe void TraceTeamAndField(string when)
	{
		var team = "no team agent";
		var party = AgentXBMPetParty.Instance();
		if (party != null)
		{
			var ids = party->SelectedPetIds.AsSpan();
			var current = party->PetCurrentHealth;
			var max = party->PetMaxHealth;
			var pets = new List<string>();
			for (var i = 0; i < ids.Length; i++)
			{
				var hp = i < current.Length && i < max.Length ? $" {current[i]}/{max[i]}" : string.Empty;
				pets.Add($"{(ids[i].PetId <= byte.MaxValue ? ((Rotations.Basic.BeastmasterPet)ids[i].PetId).ToString() : "#" + ids[i].PetId)}{hp}");
			}

			team = $"team mode {party->Mode}, content {party->ContentId}, {party->SelectedPets.Count} selected, {ids.Length} ids, "
				+ $"{party->CandidatePets.Count} candidates: {string.Join(", ", pets)}";
		}

		var field = "no field agent";
		var stages = AgentXBMStageDetailList.Instance();
		if (stages != null)
		{
			var entries = stages->Entries;
			var stageRows = 0;
			var battles = new List<uint>();
			for (var i = 0; i < entries.Length; i++)
			{
				ref readonly var entry = ref entries[i];
				if (entry.EntryType == 3 && entry.BattleDetailId != 0)
				{
					if (!battles.Contains(entry.BattleDetailId))
					{
						battles.Add(entry.BattleDetailId);
					}
				}
				else if (entry.StageEventId != 0 || entry.EntryType != 0)
				{
					stageRows++;
				}
			}

			var selected = new List<int>();
			var selection = stages->EntrySelection;
			for (var i = 0; i < selection.Length; i++)
			{
				if (selection[i] != 0)
				{
					selected.Add(i);
				}
			}

			field = $"field mode {stages->Mode}, content {stages->ContentId}, built {stages->AddonInitState}, {stageRows} stage rows, "
				+ $"battles {string.Join(" ", battles)}, selected {string.Join(" ", selected)}";
		}

		DefenseTrace.Line($"crucible agents ({when}): {team} | {field}");
	}

	private static void EndFight()
	{
		if (_fight != 0)
		{
			TraceTeamAndField($"fight {_fight} over");
		}

		_fight = 0;
		_enemies = [];
		_atStart.Clear();
		_present.Clear();
		_seen.Clear();
		_ownDamage = 0d;
		_ownDamageByElement.Clear();
	}

	/// <summary>
	/// K1 (A549): a hit of the player or one of his familiars on <paramref name="target"/> in a fight of the Crucible. Its
	/// damage per potency goes to its action's sums for hits on the target's weakness or on anything else; actions whose
	/// effect text states no plain potency and actions that hit no weakness are left out. Effect handler, game thread.
	/// </summary>
	internal static void RecordHit(uint actionId, IBattleChara target, uint amount)
	{
		if (_fight == 0 || amount == 0)
		{
			return;
		}

		var elements = ElementsOf(actionId);
		_ownDamage += amount;
		foreach (var element in elements)
		{
			_ownDamageByElement[element] = (_ownDamageByElement.TryGetValue(element, out var dealt) ? dealt : 0d) + amount;
		}

		if (elements.Count == 0 || !ActionTooltip.TryPlainPotency(actionId, out var potency) || potency <= 0f)
		{
			return;
		}

		if (_measuredIn != DataCenter.TerritoryID)
		{
			_measuredIn = DataCenter.TerritoryID;
			_perPotency.Clear();
			_bonusTraced = false;
		}

		var weak = WeaknessOf(target.NameId);
		var perPotency = amount / (double)potency;
		var sums = _perPotency.TryGetValue(actionId, out var known) ? known : default;
		sums = weak != 0 && elements.Contains(weak)
			? sums with { Weak = sums.Weak + perPotency, WeakHits = sums.WeakHits + 1 }
			: sums with { Other = sums.Other + perPotency, OtherHits = sums.OtherHits + 1 };
		_perPotency[actionId] = sums;

		if (!_bonusTraced && WeaknessBonus > 0f)
		{
			_bonusTraced = true;
			DefenseTrace.Line($"crucible: a hit on a weakness deals {WeaknessBonus:P0} more, first measured on action {actionId}");
		}
	}

	/// <summary>
	/// K1 (A549): how much more a hit on a weakness deals in this area, measured: per action that hit both a weakness and
	/// something else, its mean damage per potency on the weakness against the rest, weighted by the smaller of its two
	/// counts. 0 until an action has hit both - the bonus does not count before it is measured.
	/// </summary>
	internal static float WeaknessBonus
	{
		get
		{
			if (_measuredIn != DataCenter.TerritoryID)
			{
				return 0f;
			}

			double weighted = 0d;
			double weights = 0d;
			foreach (var (_, sums) in _perPotency)
			{
				if (sums.WeakHits == 0 || sums.OtherHits == 0 || sums.Other <= 0d)
				{
					continue;
				}

				var ratio = sums.Weak / sums.WeakHits / (sums.Other / sums.OtherHits);
				var weight = Math.Min(sums.WeakHits, sums.OtherHits);
				weighted += ratio * weight;
				weights += weight;
			}

			return weights > 0d ? Math.Max(0f, (float)(weighted / weights) - 1f) : 0f;
		}
	}

	/// <summary>
	/// K3 (A549): by how much the measured weakness bonus speeds the fall of an enemy of the fight in progress - one plus the
	/// bonus times the share of the player's side's damage in this fight dealt by actions hitting its weakness; 1 while
	/// nothing is measured or it has none.
	/// </summary>
	internal static float FallSpeedup(uint nameId)
	{
		var weakness = WeaknessOf(nameId);
		var bonus = WeaknessBonus;
		if (weakness == 0 || bonus <= 0f || _ownDamage <= 0d || !_ownDamageByElement.TryGetValue(weakness, out var dealt))
		{
			return 1f;
		}

		return 1f + (bonus * (float)(dealt / _ownDamage));
	}

	internal static string EnemyName(uint nameId)
	{
		return Service.GetSheet<Lumina.Excel.Sheets.BNpcName>()?.GetRowOrDefault(nameId) is { } row
			? row.Singular.ExtractText()
			: $"#{nameId}";
	}

	internal static string ElementName(uint element)
	{
		return _elementNames.TryGetValue(element, out var name) ? name : "nothing";
	}
}

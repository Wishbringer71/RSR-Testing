using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using RotationSolver.Basic.Resources;

namespace RotationSolver.Basic.Rotations.Basic;

/// <summary>One point of a stage guide as the window shows it: the spell RSR would put in, or what is missing.</summary>
/// <param name="Required">The guide's "Required Spells", else "Recommended Spells".</param>
/// <param name="Spell">The chosen spell's action id, 0 when none of the named spells is learned.</param>
/// <param name="Text">The chosen spell's name, and for whom it stands in; or the names of the spells missing.</param>
/// <param name="InSet">Whether the chosen spell is in the active set already.</param>
public readonly record struct CarnivalePick(bool Required, uint Spell, string Text, bool InSet);

public partial class BlueMageRotation
{
	// A627 (concept 22, "Satz je Stufe aus den Stufenführern"): the spells the stage guide of Blue Academy names for the stage
	// the Masked Carnivale briefing has chosen, and on the player's click the set that holds them.
	private static Dictionary<uint, uint>? _spellByNumber;
	private static readonly Dictionary<int, List<List<Lumina.Excel.Sheets.AOZContentBriefingBNpc>>> _stageEnemies = [];
	private static List<string>? _stageNames;
	private static int _briefedIndex = -1;
	private static int _stageShift;
	private static List<uint> _carnivaleQueue = [];
	private static (int Slot, uint Spell, DateTime At) _carnivaleWrite = (-1, 0, DateTime.MinValue);

	/// <summary>The stage the window shows, 0 while the briefing is closed.</summary>
	public static int CarnivaleStage { get; private set; }

	/// <summary>The stage's name in the client's language, so a wrong reading shows beside the briefing's own.</summary>
	public static string CarnivaleStageName { get; private set; } = string.Empty;

	/// <summary>The guide's points for the stage, required first.</summary>
	public static IReadOnlyList<CarnivalePick> CarnivaleProposal { get; private set; } = [];

	/// <summary>What the last click did, or why it stopped.</summary>
	public static string CarnivaleStatus { get; private set; } = string.Empty;

	/// <summary>Whether the set is being written for a stage; the improver waits meanwhile.</summary>
	public static bool CarnivaleApplying => _carnivaleQueue.Count > 0 || _carnivaleWrite.Slot >= 0;

	/// <summary>The window's buttons: another stage than the one read from the briefing.</summary>
	public static void ShiftCarnivaleStage(int by)
	{
		_stageShift += by;
	}

	/// <summary>The window's "Apply": the proposal's spells that are not in the set yet, written one slot per frame.</summary>
	public static void RequestCarnivaleSet()
	{
		_carnivaleQueue = [];
		foreach (var pick in CarnivaleProposal)
		{
			if (pick.Spell != 0 && !pick.InSet && !_carnivaleQueue.Contains(pick.Spell))
			{
				_carnivaleQueue.Add(pick.Spell);
			}
		}

		CarnivaleStatus = _carnivaleQueue.Count == 0 ? "Every learned spell of the guide is in the set." : "Writing the set...";
	}

	/// <inheritdoc/>
	protected override void UpdateServices()
	{
		base.UpdateServices();
		UpdateCarnivaleSet();
	}

	private unsafe void UpdateCarnivaleSet()
	{
		var agent = AgentAozContentBriefing.Instance();
		if (agent == null || !agent->IsAgentActive() || agent->AozContentData == null)
		{
			CarnivaleStage = 0;
			CarnivaleProposal = [];
			CarnivaleStatus = string.Empty;
			_carnivaleQueue = [];
			_carnivaleWrite = (-1, 0, DateTime.MinValue);
			_briefedIndex = -1;
			return;
		}

		var index = agent->AozContentData->SelectedContentIndex;
		if (index != _briefedIndex)
		{
			_briefedIndex = index;
			_stageShift = 0;
		}

		var names = StageNames();
		var stage = Math.Clamp(index + 1 + _stageShift, 1, Math.Max(1, names.Count));
		_stageShift = stage - index - 1;
		CarnivaleStage = stage;
		CarnivaleStageName = stage <= names.Count ? names[stage - 1] : string.Empty;
		CarnivaleProposal = Propose(stage, DataCenter.BluSlots);
		WriteCarnivaleSet();
	}

	// Per point the first learned spell it names; for a filler example nobody learned, a learned spell of the same element and
	// recast group with no longer a recast.
	private List<CarnivalePick> Propose(int stage, uint[] slots)
	{
		var picks = new List<CarnivalePick>();
		if (!CarnivaleSets.Stages.TryGetValue(stage, out var points))
		{
			return picks;
		}

		var learned = new Dictionary<uint, IBaseAction>();
		foreach (var action in AllBaseActions)
		{
			if (action != null && action.Action.ClassJob.RowId == (uint)ECommons.ExcelServices.Job.BLU && action.EnoughLevel
				&& action.Info.IsQuestUnlocked())
			{
				learned[action.ID] = action;
			}
		}

		foreach (var required in new[] { true, false })
		{
			foreach (var point in points)
			{
				if (point.Required != required)
				{
					continue;
				}

				uint chosen = 0;
				var text = string.Empty;
				var missing = new List<string>();
				foreach (var number in point.Spells)
				{
					var id = SpellOf(number);
					if (id != 0 && learned.TryGetValue(id, out var action))
					{
						chosen = id;
						text = action.Name;
						break;
					}

					missing.Add(SpellName(id, number));
				}

				if (chosen == 0 && point.Filler && point.Spells.Length > 0
					&& StandIn(SpellOf(point.Spells[0]), learned, slots) is { } standIn)
				{
					chosen = standIn.ID;
					text = $"{standIn.Name} (for {SpellName(SpellOf(point.Spells[0]), point.Spells[0])})";
				}

				if (chosen == 0)
				{
					text = $"not learned: {string.Join(" or ", missing)}" + (point.Example ? " (an example; another of its kind does too)" : string.Empty);
				}

				picks.Add(new CarnivalePick(required, chosen, text + ImmuneNote(stage, chosen),
					chosen != 0 && Array.IndexOf(slots, chosen) >= 0));
			}
		}

		AddWeaknessFillers(stage, points, learned, slots, picks);
		AddCoverageFillers(stage, learned, slots, picks);
		return picks;
	}

	// A762 (concept 22, "Jeder Gegner braucht einen Zauber, der ihn trifft"): an enemy of the stage immune to every damaging
	// spell of the proposal and the set - stage 24, the Magus immune to magic beside the Viking immune to physical spells -
	// gets a recommended point: the learned filler with the most damage per time on it. The weakness points' rule, for
	// immunity.
	private static void AddCoverageFillers(int stage, Dictionary<uint, IBaseAction> learned, uint[] slots, List<CarnivalePick> picks)
	{
		var acts = StageEnemies(stage);
		for (var act = 0; act < acts.Count; act++)
		{
			foreach (var enemy in acts[act])
			{
				var nameId = enemy.BNpcName.RowId;
				if (Hurts(nameId, picks, slots))
				{
					continue;
				}

				var best = BestFiller(learned, null, nameId);
				var why = $"hurts {EnemyName(enemy)} (act {act + 1}), immune to every damaging spell of the set";
				picks.Add(best == null
					? new CarnivalePick(false, 0, $"not learned: a spell that {why}", false)
					: new CarnivalePick(false, best.ID, $"{best.Name} - {why}", Array.IndexOf(slots, best.ID) >= 0));
			}
		}
	}

	// Whether a damaging spell of the proposal or the set is not one the enemy is immune to.
	private static bool Hurts(uint nameId, List<CarnivalePick> picks, uint[] slots)
	{
		foreach (var pick in picks)
		{
			if (pick.Spell != 0 && HurtsWith(nameId, pick.Spell))
			{
				return true;
			}
		}

		foreach (var spell in slots)
		{
			if (spell != 0 && HurtsWith(nameId, spell))
			{
				return true;
			}
		}

		return false;
	}

	private static bool HurtsWith(uint nameId, uint spell)
	{
		return ActionTooltip.TryPotencyOn(spell, 1, out var potency) && potency > 0f
			&& Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(spell) is { } row
			&& MaskedCarnivaleHelper.ResistanceTo(nameId, row) != MobResistance.Immune;
	}

	// The learned filler - a GCD without a recast of its own, with a potency in its text - with the most potency per time
	// times its factor on the enemy (none when immune, the weakness factor when weak; an enemy of 0 is no enemy, factor 1),
	// of the element when one is given.
	private static IBaseAction? BestFiller(Dictionary<uint, IBaseAction> learned, Aspect? element, uint nameId)
	{
		IBaseAction? best = null;
		var bestRate = 0f;
		foreach (var action in learned.Values)
		{
			if ((element is { } aspect && action.Action.Aspect != (byte)aspect) || !action.Info.IsGeneralGCD
				|| action.Cooldown.RecastTimeOneChargeRaw > WeaponTotal
				|| !ActionTooltip.TryPotencyOn(action.ID, 1, out var potency))
			{
				continue;
			}

			var factor = MaskedCarnivaleHelper.ResistanceTo(nameId, action.Action) switch
			{
				MobResistance.Immune => 0f,
				MobResistance.Weak => OwnHitWatch.WeaknessFactor,
				_ => 1f,
			};
			var rate = potency * factor / SpellTime(action);
			if (rate > bestRate)
			{
				best = action;
				bestRate = rate;
			}
		}

		return best;
	}

	// A647: an element an enemy of the stage is weak to and no filler point of the guide names gets a recommended point - the
	// learned filler of that element (a GCD without a recast of its own, a potency in its text) with the most potency per
	// time on one target. The guide's points come first; it knows what the sheet does not.
	private static void AddWeaknessFillers(int stage, CarnivaleSets.Pick[] points, Dictionary<uint, IBaseAction> learned,
		uint[] slots, List<CarnivalePick> picks)
	{
		var actions = Service.GetSheet<Lumina.Excel.Sheets.Action>();
		var named = new HashSet<byte>();
		foreach (var point in points)
		{
			// Only the guide's filler points name an element to fight with; Ice Spikes on stage 12 is a defence.
			if (!point.Filler)
			{
				continue;
			}

			foreach (var number in point.Spells)
			{
				if (actions.GetRowOrDefault(SpellOf(number)) is { } row)
				{
					_ = named.Add(row.Aspect);
				}
			}
		}

		var acts = StageEnemies(stage);
		foreach (var element in new[] { Aspect.Fire, Aspect.Ice, Aspect.Wind, Aspect.Earth, Aspect.Lightning, Aspect.Water })
		{
			if (named.Contains((byte)element) || WeakEnemy(acts, element) is not { } weak)
			{
				continue;
			}

			var best = BestFiller(learned, element, 0);
			var why = $"{element} filler: {weak} weak to {element.ToString().ToLowerInvariant()}";
			picks.Add(best == null
				? new CarnivalePick(false, 0, $"not learned: a {why}", false)
				: new CarnivalePick(false, best.ID, $"{best.Name} - {why}{ImmuneNote(stage, best.ID)}", Array.IndexOf(slots, best.ID) >= 0));
		}
	}

	// The first enemy of the stage weak to the element, with its act; null when none is.
	private static string? WeakEnemy(IReadOnlyList<List<Lumina.Excel.Sheets.AOZContentBriefingBNpc>> acts, Aspect element)
	{
		for (var act = 0; act < acts.Count; act++)
		{
			foreach (var enemy in acts[act])
			{
				if (MaskedCarnivaleHelper.GetAspectResistance(enemy.BNpcName.RowId, element) == MobResistance.Weak)
				{
					return $"{EnemyName(enemy)} (act {act + 1})";
				}
			}
		}

		return null;
	}

	// The enemies of the stage immune to the spell, by the sheet: the guide may name it for enemies the sheet does not list
	// (stage 28: the fire spell is for the undead that appear in the fight; Durinn is immune to fire), so it is told, not
	// dropped (A647).
	private static string ImmuneNote(int stage, uint spell)
	{
		if (spell == 0 || Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(spell) is not { } row)
		{
			return string.Empty;
		}

		var immune = new List<string>();
		var acts = StageEnemies(stage);
		for (var act = 0; act < acts.Count; act++)
		{
			foreach (var enemy in acts[act])
			{
				if (MaskedCarnivaleHelper.ResistanceTo(enemy.BNpcName.RowId, row) == MobResistance.Immune)
				{
					immune.Add($"{EnemyName(enemy)} (act {act + 1})");
				}
			}
		}

		return immune.Count == 0 ? string.Empty : $" - immune: {string.Join(", ", immune)}";
	}

	private static string EnemyName(Lumina.Excel.Sheets.AOZContentBriefingBNpc enemy)
	{
		return enemy.BNpcName.ValueNullable?.Singular.ExtractText() is { Length: > 0 } name ? name : $"enemy {enemy.BNpcName.RowId}";
	}

	/// <summary>
	/// The enemies of each act of a Masked Carnivale stage, from the sheets (concept 22, "Die Gegner einer Stufe stehen in
	/// den Blättern", A627, A647): the <c>AOZContent</c> row whose Order is the stage names per act a fight type and a row -
	/// fight type 1 a group in <c>AOZArrangement</c> (its subrows), 2 and 3 a boss in <c>AOZBoss</c> (BlueMageSetBuilder
	/// 213d3d9 checks this against the stage names: stage 3 is Zipacna, 25 Azulmagia, 32 Goldor).
	/// </summary>
	internal static IReadOnlyList<List<Lumina.Excel.Sheets.AOZContentBriefingBNpc>> StageEnemies(int stage)
	{
		if (_stageEnemies.TryGetValue(stage, out var known))
		{
			return known;
		}

		var acts = new List<List<Lumina.Excel.Sheets.AOZContentBriefingBNpc>>();
		_stageEnemies[stage] = acts;
		var arrangements = Svc.Data.GetSubrowExcelSheet<Lumina.Excel.Sheets.AOZArrangement>();
		var bosses = Service.GetSheet<Lumina.Excel.Sheets.AOZBoss>();
		foreach (var content in Service.GetSheet<Lumina.Excel.Sheets.AOZContent>())
		{
			if (content.Order != stage)
			{
				continue;
			}

			foreach (var (row, fightType) in new[] { (content.Act1.RowId, content.Act1FightType), (content.Act2.RowId, content.Act2FightType),
				(content.Act3.RowId, content.Act3FightType) })
			{
				if (row == 0)
				{
					continue;
				}

				var enemies = new List<Lumina.Excel.Sheets.AOZContentBriefingBNpc>();
				if (fightType == 1)
				{
					if (arrangements != null && arrangements.TryGetRow(row, out var group))
					{
						foreach (var member in group)
						{
							if (member.AOZContentBriefingBNpc.ValueNullable is { } enemy)
							{
								enemies.Add(enemy);
							}
						}
					}
				}
				else if (bosses.GetRowOrDefault(row)?.Boss.ValueNullable is { } boss)
				{
					enemies.Add(boss);
				}

				acts.Add(enemies);
			}

			break;
		}

		return acts;
	}

	// A learned spell of the example's element and recast group, with no longer a recast; one in the set first.
	private static IBaseAction? StandIn(uint example, Dictionary<uint, IBaseAction> learned, uint[] slots)
	{
		var row = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(example);
		if (row == null)
		{
			return null;
		}

		IBaseAction? found = null;
		foreach (var action in learned.Values)
		{
			var other = action.Action;
			if (other.Aspect != row.Value.Aspect || other.CooldownGroup != row.Value.CooldownGroup
				|| other.Recast100ms > row.Value.Recast100ms)
			{
				continue;
			}

			if (Array.IndexOf(slots, action.ID) >= 0)
			{
				return action;
			}

			found ??= action;
		}

		return found;
	}

	// One slot per frame, out of combat and not between areas, read back before the next: free slots first, then from the last
	// slot backwards the slots whose spell no point of the stage names.
	private unsafe void WriteCarnivaleSet()
	{
		var slots = DataCenter.BluSlots;
		if (_carnivaleWrite.Slot >= 0)
		{
			if (slots[_carnivaleWrite.Slot] == _carnivaleWrite.Spell)
			{
				_carnivaleWrite = (-1, 0, DateTime.MinValue);
			}
			else if ((DateTime.Now - _carnivaleWrite.At).TotalSeconds < DataCenter.DefaultGCDTotal)
			{
				return;
			}
			else
			{
				CarnivaleStatus = $"The game did not take {SpellName(_carnivaleWrite.Spell, 0)} in slot {_carnivaleWrite.Slot + 1} - stopped.";
				_carnivaleQueue = [];
				_carnivaleWrite = (-1, 0, DateTime.MinValue);
				return;
			}
		}

		if (_carnivaleQueue.Count == 0)
		{
			return;
		}

		var manager = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
		var player = Player;
		if (DataCenter.InCombat || player == null || player.IsCasting || manager == null || manager->ActionQueued
			|| Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BetweenAreas]
			|| Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BetweenAreas51])
		{
			return;
		}

		var spell = _carnivaleQueue[0];
		_carnivaleQueue.RemoveAt(0);
		if (Array.IndexOf(slots, spell) >= 0)
		{
			return;
		}

		var slot = FreeSlot(slots);
		if (slot < 0)
		{
			CarnivaleStatus = $"No slot left for {SpellName(spell, 0)} - every slot holds a spell of the guide.";
			_carnivaleQueue = [];
			return;
		}

		var old = slots[slot];
		manager->AssignBlueMageActionToSlot(slot, spell);
		_carnivaleWrite = (slot, spell, DateTime.Now);
		DefenseTrace.Line($"carnivale set: stage {CarnivaleStage}, slot {slot + 1} #{old} -> {SpellName(spell, 0)} #{spell} (on the player's click)");
		if (_carnivaleQueue.Count == 0)
		{
			CarnivaleStatus = $"Set written for stage {CarnivaleStage}.";
		}
	}

	private static int FreeSlot(uint[] slots)
	{
		var empty = Array.IndexOf(slots, 0u);
		if (empty >= 0)
		{
			return empty;
		}

		var named = new HashSet<uint>();
		if (CarnivaleSets.Stages.TryGetValue(CarnivaleStage, out var points))
		{
			foreach (var point in points)
			{
				foreach (var number in point.Spells)
				{
					_ = named.Add(SpellOf(number));
				}
			}
		}

		foreach (var pick in CarnivaleProposal)
		{
			_ = named.Add(pick.Spell);
		}

		for (var i = slots.Length - 1; i >= 0; i--)
		{
			if (!named.Contains(slots[i]))
			{
				return i;
			}
		}

		return -1;
	}

	// Spellbook number -> action, through AozActionTransient (Number) and AozAction (Action), which share their rows.
	private static uint SpellOf(uint number)
	{
		if (_spellByNumber == null)
		{
			_spellByNumber = [];
			var actions = Service.GetSheet<Lumina.Excel.Sheets.AozAction>();
			foreach (var row in Service.GetSheet<Lumina.Excel.Sheets.AozActionTransient>())
			{
				if (row.Number != 0 && actions.GetRowOrDefault(row.RowId) is { } aoz)
				{
					_spellByNumber[row.Number] = aoz.Action.RowId;
				}
			}
		}

		return _spellByNumber.TryGetValue(number, out var id) ? id : 0;
	}

	private static string SpellName(uint id, uint number)
	{
		return Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(id)?.Name.ExtractText() is { Length: > 0 } name
			? name
			: $"spell {number}";
	}

	// The stage names in the client's language: the Masked Carnivale's duties, ordered by their content.
	private static List<string> StageNames()
	{
		if (_stageNames != null)
		{
			return _stageNames;
		}

		var duties = new List<(uint Content, string Name)>();
		foreach (var row in Service.GetSheet<Lumina.Excel.Sheets.ContentFinderCondition>())
		{
			if (row.ContentType.RowId == (uint)TerritoryContentType.TheMaskedCarnivale)
			{
				duties.Add((row.Content.RowId, row.Name.ExtractText()));
			}
		}

		duties.Sort((a, b) => a.Content.CompareTo(b.Content));
		_stageNames = [];
		foreach (var duty in duties)
		{
			_stageNames.Add(duty.Name);
		}

		return _stageNames;
	}
}

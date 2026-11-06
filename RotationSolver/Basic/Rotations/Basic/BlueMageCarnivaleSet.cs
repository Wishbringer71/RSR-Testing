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
	// A627 (concept 01, "Satz je Stufe aus den Stufenführern"): the spells the stage guide of Blue Academy names for the stage
	// the Masked Carnivale briefing has chosen, and on the player's click the set that holds them.
	private static Dictionary<uint, uint>? _spellByNumber;
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

				picks.Add(new CarnivalePick(required, chosen, text, chosen != 0 && Array.IndexOf(slots, chosen) >= 0));
			}
		}

		return picks;
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

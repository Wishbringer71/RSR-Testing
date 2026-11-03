using ECommons.ExcelServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using System.Text;

namespace RotationSolver.Updaters;

/// <summary>
/// Writes the Blue Mage's active set to <c>DefenseTrace.log</c> whenever it changes, and every own Blue Mage spell that
/// lands although it is not in the set read that frame (concept 01, "Blaumagier", stage 0, A506).
/// </summary>
/// <remarks>
/// The owner's report of 03.11.2026: 1000 Needles is not in his set and is cast all the same. RSR's basic check refuses
/// every Blue Mage spell outside the set it reads (<c>ActionBasicInfo.IsOnSlot</c>), and nothing in the repository
/// writes the set. Which of the three readings holds - the active set does hold it, the read gives something else, or a
/// path goes past the check - only his client can show; these two lines show it.
/// </remarks>
internal static class BlueMageSetTrace
{
	private static readonly uint[] _lastSet = new uint[DataCenter.BluSlots.Length];

	/// <summary>Called after the slots were read; writes the set when it differs from the last one written.</summary>
	public static void SlotsRead(uint[] slots)
	{
		var changed = false;
		for (var i = 0; i < slots.Length && i < _lastSet.Length; i++)
		{
			if (slots[i] != _lastSet[i])
			{
				changed = true;
				_lastSet[i] = slots[i];
			}
		}

		if (!changed)
		{
			return;
		}

		var text = new StringBuilder("blue mage set:");
		for (var i = 0; i < slots.Length; i++)
		{
			text.Append(i == 0 ? " " : ", ").Append(i + 1).Append(' ').Append(Name(slots[i])).Append(" #").Append(slots[i]);
		}

		DefenseTrace.Line(text.ToString());
	}

	/// <summary>
	/// Called when an own action lands. A Blue Mage spell not in the set is written, unless a set spell turns into it
	/// right now (White Death on Cold Fog, Divine Cataract on Chelonian Gate) - read through the game's adjusted id.
	/// </summary>
	public static unsafe void Landed(Lumina.Excel.Sheets.Action action)
	{
		if (DataCenter.Job != Job.BLU || action.ClassJob.RowId != (uint)Job.BLU)
		{
			return;
		}

		var manager = ActionManager.Instance();
		var slots = DataCenter.BluSlots;
		for (var i = 0; i < slots.Length; i++)
		{
			if (slots[i] == 0)
			{
				continue;
			}

			if (slots[i] == action.RowId || (manager != null && manager->GetAdjustedActionId(slots[i]) == action.RowId))
			{
				return;
			}
		}

		DefenseTrace.Line($"blue mage spell outside the active set: {action.Name.ExtractText()} #{action.RowId}"
			+ " (a follow-up whose set spell has already turned back shows here as well)");
	}

	private static int _briefedStage = -1;

	/// <summary>
	/// Writes the stage the Masked Carnivale briefing shows and, per act, the enemy ids it holds with the briefing sheet's
	/// name for each, once per stage chosen (concept 01, "Masked Carnivale", A510). What these ids are is not documented:
	/// FFXIVClientStructs names them enemies of an arrangement, nothing more. The line shows whether they resolve to the
	/// stage's enemies in <c>AOZContentBriefingBNpc</c> - the condition for choosing a set per stage by their resistances.
	/// </summary>
	public static unsafe void BriefingRead()
	{
		var agent = FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentAozContentBriefing.Instance();
		if (agent == null || !agent->IsAgentActive() || agent->AozContentData == null)
		{
			_briefedStage = -1;
			return;
		}

		var data = agent->AozContentData;
		var stage = data->SelectedContentIndex;
		if (stage == _briefedStage)
		{
			return;
		}

		_briefedStage = stage;
		var sheet = Service.GetSheet<Lumina.Excel.Sheets.AOZContentBriefingBNpc>();
		var text = new StringBuilder($"carnivale briefing: stage index {stage}");
		var act = 0;
		foreach (ref readonly var arrangement in data->Arrangements)
		{
			act++;
			text.Append($"; act {act}:");
			for (var i = 0; i < arrangement.Count && i < arrangement.Enemies.Length; i++)
			{
				var id = arrangement.Enemies[i];
				var name = sheet.GetRowOrDefault(id) is { } row ? row.BNpcName.Value.Singular.ExtractText() : "not in the briefing sheet";
				text.Append($" #{id} {name}");
			}
		}

		DefenseTrace.Line(text.ToString());
	}

	private static string Name(uint id)
	{
		if (id == 0)
		{
			return "empty";
		}

		return Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(id)?.Name.ExtractText() ?? "unknown";
	}
}

using ECommons;
using ECommons.DalamudServices;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace RotationSolver.Rotations.Limited.Beastmaster;

public partial class BeastmasterRotation
{
	// Concept 21, K7 "Kader stimmt" (A821, A846): the game starts a board from the board choice, measured in his trace of
	// 16.11.2026 twice - a ButtonClick with parameter 9 there, then the duty confirmation. RSR sends exactly that event to
	// exactly that window, as a mouse click does, and nothing else.
	private const string BoardChoiceWindow = "XBMStageDetailList";
	private const int BoardStartParam = 9;
	private const string DutyConfirmWindow = "ContentsFinderConfirm";

	private enum StartStep : byte
	{
		None,
		Pressed,
		Commenced,
	}

	private static StartStep _startStep;
	private static uint _startTerritory;
	private static bool _startRefused;

	/// <summary>
	/// K7: whether RSR's "Start" can start the board now - the board choice open, ready, and holding a visible, enabled button
	/// with the measured start event. <paramref name="why"/> says what is missing otherwise.
	/// </summary>
	public static unsafe bool CanStartCrucibleBoard(out string why)
	{
		if (_startRefused)
		{
			why = "a start from here did not reach the board this session - start it in the game's window";
			return false;
		}

		if (_startStep != StartStep.None)
		{
			why = "starting...";
			return false;
		}

		var choice = (AtkUnitBase*)Svc.GameGui.GetAddonByName(BoardChoiceWindow).Address;
		if (choice == null || !choice->IsVisible || !GenericHelpers.IsAddonReady(choice))
		{
			why = "start the board in the game's window";
			return false;
		}

		if (FindBoardStart(choice) == null)
		{
			why = "the game's start button is not ready - start the board in the game's window";
			return false;
		}

		why = string.Empty;
		return true;
	}

	/// <summary>K7: presses the board choice's start button as a mouse click does; the duty confirmation follows by itself.</summary>
	public static unsafe void StartCrucibleBoard()
	{
		var choice = (AtkUnitBase*)Svc.GameGui.GetAddonByName(BoardChoiceWindow).Address;
		if (choice == null || !choice->IsVisible || !GenericHelpers.IsAddonReady(choice))
		{
			return;
		}

		var start = FindBoardStart(choice);
		if (start == null)
		{
			return;
		}

		_startStep = StartStep.Pressed;
		_startTerritory = DataCenter.TerritoryID;
		DefenseTrace.Line($"crucible start: pressed the board choice's start (event {start->State.EventType}, param {start->Param})");
		choice->ReceiveEvent(start->State.EventType, (int)start->Param, start);
	}

	/// <summary>
	/// K7, every update of the beastmaster rotation: after RSR's start, "Commence" once the duty confirmation shows; the board
	/// reached, or the windows closed without it, ends the start - a failed one is not offered again this session.
	/// </summary>
	internal static unsafe void FollowCrucibleStart()
	{
		if (_startStep == StartStep.None)
		{
			return;
		}

		if (DataCenter.TerritoryID != _startTerritory)
		{
			var reached = DataCenter.IsInCrucible;
			_startRefused |= !reached;
			DefenseTrace.Line(reached ? "crucible start: on the board" : "crucible start: the territory changed, but not to a board - not offered again this session");
			_startStep = StartStep.None;
			return;
		}

		var confirm = (AtkUnitBase*)Svc.GameGui.GetAddonByName(DutyConfirmWindow).Address;
		var confirmShown = confirm != null && confirm->IsVisible && GenericHelpers.IsAddonReady(confirm);
		var choice = (AtkUnitBase*)Svc.GameGui.GetAddonByName(BoardChoiceWindow).Address;
		var choiceShown = choice != null && choice->IsVisible;
		if (_startStep == StartStep.Pressed && confirmShown)
		{
			// Commence once its button takes a click; until then the next update asks again.
			var button = ((AddonContentsFinderConfirm*)confirm)->CommenceButton;
			if (button == null || !button->IsEnabled)
			{
				return;
			}

			_startStep = StartStep.Commenced;
			DefenseTrace.Line("crucible start: the duty confirmation shows - Commence");
			new AddonMaster.ContentsFinderConfirm(confirm).Commence();
			return;
		}

		if (_startStep == StartStep.Pressed && !choiceShown)
		{
			_startRefused = true;
			_startStep = StartStep.None;
			DefenseTrace.Line("crucible start: the board choice closed without a duty confirmation - not offered again this session");
		}
		else if (_startStep == StartStep.Commenced && !confirmShown && choiceShown)
		{
			// The confirmation closed and the board choice is still there: withdrawn, by him or the game - nothing failed.
			_startStep = StartStep.None;
			DefenseTrace.Line("crucible start: the duty confirmation closed without entering - start again when ready");
		}
	}

	// The board choice's button that carries the measured start event, when it is visible and enabled; its event list is read,
	// not its text or place, which the trace does not give.
	private static unsafe AtkEvent* FindBoardStart(AtkUnitBase* choice)
	{
		var manager = &choice->UldManager;
		for (var i = 0; i < manager->NodeListCount; i++)
		{
			var node = manager->NodeList[i];
			var component = node == null ? null : node->GetComponent();
			if (component == null || component->GetComponentType() != ComponentType.Button)
			{
				continue;
			}

			var button = (AtkComponentButton*)component;
			if (!button->IsEnabled || !node->IsVisible())
			{
				continue;
			}

			for (var atkEvent = node->AtkEventManager.Event; atkEvent != null; atkEvent = atkEvent->NextEvent)
			{
				if (atkEvent->State.EventType == AtkEventType.ButtonClick && atkEvent->Param == BoardStartParam)
				{
					return atkEvent;
				}
			}
		}

		return null;
	}
}

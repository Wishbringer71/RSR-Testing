using ECommons.GameHelpers;
using RotationSolver.Integrations.IPC;
using RotationSolver.Updaters;

namespace RotationSolver.Engine.Helpers;

/// <summary>
/// Concept 02, "Bewegung in die Seite über BossMods Bewegungsmodul" (A928): while the next GCD of a melee needs a side the
/// player is not on and True North does not cover it, BossMod's "Automatic movement" with "Goes to specified positional"
/// walks him into that side - BossMod's pathfinding, danger map, special modes and input handling, no hook of RSR's own.
/// RSR switches its own preset on only for that window and off when the GCD is pressed or a condition falls, and never
/// while BossMod's AI steers or another preset is active. It judges itself: a press on the side that the game still judges
/// a miss, or an enemy hit on the player while BossMod walks him, stops it for that target, a second one for the area.
/// </summary>
internal static class PositionalMove
{
	private const string PresetName = "RSR Positionals";
	private const string PositionalModule = "BossMod.Autorotation.MiscAI.GoToPositional";

	// BossMod's preset format: module type names, then track and option names (BossModReborn 8d5ff8e21, JsonPresetConverter).
	// "Pathfind" moves by BossMod's pathfinding to the best spot of the goal zones; "Small" keeps 0.5 y inside the side.
	private const string PresetJson = "{\"Name\":\"" + PresetName + "\",\"Modules\":{"
		+ "\"BossMod.Autorotation.MiscAI.NormalMovement\":[{\"Track\":\"Destination\",\"Option\":\"Pathfind\"}],"
		+ "\"" + PositionalModule + "\":[{\"Track\":\"EdgeBuffer\",\"Option\":\"Small\"}]}}";

	private static bool _created, _refused, _active, _leftoverChecked;
	private static uint _windowAction;
	private static ulong _windowTarget;
	private static EnemyPositional _windowSide;
	private static string _windowName = string.Empty;

	// The window that ended with its GCD pressed, judged when the game's answer arrives (PositionalWatch.Landed).
	private static uint _judgeAction;
	private static ulong _judgeTarget;

	private static readonly HashSet<ulong> _stoppedTargets = [];

	// Concept 02 (A948): the facing each enemy had when first seen in this area. One that holds the player as its target
	// and has turned since turns with him; a training dummy holds him and never turns. Exact comparison: a facing nobody
	// changes is the same float frame after frame.
	private static readonly Dictionary<ulong, float> _firstFacing = [];

	// code.md, "Entscheidung zur Laufzeit": the first failure stops the move for its target, the next for the area.
	private static bool _failedInArea, _offInArea;
	private static string? _tracedWhyNot;

	internal static void Update()
	{
		// RSR's own preset left active by a session that ended without closing it (a crash) is switched off once.
		if (!_leftoverChecked && BMRInfo_IPCSubscriber.IsEnabled)
		{
			_leftoverChecked = true;
			if (!_active && BMRInfo_IPCSubscriber.PresetsGetActive?.Invoke() == PresetName)
			{
				_ = BMRInfo_IPCSubscriber.PresetsClearActive?.Invoke();
				DefenseTrace.Line($"positional move: {PresetName} was still active from an earlier session - switched off");
			}
		}

		if (Wanted(out var why) is { } window)
		{
			Open(window);
		}
		else
		{
			Close(why);
		}
	}

	/// <summary>The game judged a positional press: on the side by <paramref name="margin"/> radians, hit or missed.</summary>
	internal static void Judged(uint actionId, ulong targetId, float margin, bool hit, bool miss, bool trueNorth)
	{
		if (actionId != _judgeAction || targetId != _judgeTarget || _judgeAction == 0)
		{
			return;
		}

		_judgeAction = 0;
		var verdict = hit == miss ? "unknown" : hit ? "hit" : "miss";
		DefenseTrace.Line($"positional move: {_windowName} pressed {float.RadiansToDegrees(margin):F1}° inside its side, "
			+ $"true north {(trueNorth ? "yes" : "no")} - {verdict}");

		if (!trueNorth && margin >= 0f && miss && !hit)
		{
			Fail(targetId, "on the side the game still judged a miss");
		}
	}

	/// <summary>An enemy action reached the player: while BossMod walks him for a positional, the move stops.</summary>
	internal static void HitOnPlayer(string action)
	{
		if (_active)
		{
			Fail(_windowTarget, $"{action} hit you while BossMod walked you");
		}
	}

	internal static void AreaChanged()
	{
		Close("area changed");
		_stoppedTargets.Clear();
		_firstFacing.Clear();
		_failedInArea = _offInArea = false;
		_judgeAction = 0;
		_tracedWhyNot = null;
	}

	internal static void Dispose() => Close("RSR unloads");

	private readonly record struct Window(uint Action, ulong Target, EnemyPositional Side, string Name);

	private static Window? Wanted(out string why)
	{
		why = string.Empty;
		var side = ActionUpdater.DesiredPositional;
		if (!Service.Config.MoveIntoPositionals || !DataCenter.IsActivated() || !DataCenter.InCombat || DataCenter.IsPvP
			|| DataCenter.Role != JobRole.Melee || side is not (EnemyPositional.Rear or EnemyPositional.Flank)
			|| ActionUpdater.NextGCDAction is not { } gcd || gcd.Target.Target is not { } target || Player.Object is not { } player)
		{
			why = "no side needed";
			return null;
		}

		if (!_firstFacing.ContainsKey(target.GameObjectId))
		{
			_firstFacing[target.GameObjectId] = target.Rotation;
		}

		var keep = _active && gcd.ID == _windowAction && target.GameObjectId == _windowTarget && side == _windowSide;
		if (!keep && target.FindEnemyPositional() == side)
		{
			why = "on the side";
			return null;
		}

		// A positional ability woven before this GCD - Trick Attack - needs where the player stands now.
		if (ActionUpdater.NextAction is IBaseAction next && next != gcd && ConfigurationHelper.ActionPositional.ContainsKey((ActionID)next.ID))
		{
			why = $"{next.Name} first";
			return null;
		}

		why = _offInArea ? "a second failure in this area"
			: _stoppedTargets.Contains(target.GameObjectId) ? "a failure at this target"
			: TurnsWithPlayer(target, player) ? "the target turns with you"
			: !BMRInfo_IPCSubscriber.IsEnabled ? "BossMod is not running"
			: _refused ? "BossMod refused the preset"
			: DataCenter.BMRIsNavigating ? "BossMod's AI walks you"
			: GazeWatch.AnyPending() ? "a gaze is due"
			: ForeignPreset() is { } other ? $"your BossMod preset {other} is active"
			: string.Empty;
		if (why.Length > 0)
		{
			TraceWhyNot(why);
			return null;
		}

		return new Window(gcd.ID, target.GameObjectId, side, gcd.Name);
	}

	private static bool TurnsWithPlayer(IBattleChara target, IGameObject player)
	{
		return target.TargetObjectId == player.GameObjectId
			&& _firstFacing.TryGetValue(target.GameObjectId, out var first) && target.Rotation != first;
	}

	private static string? ForeignPreset()
	{
		var active = BMRInfo_IPCSubscriber.PresetsGetActive?.Invoke();
		return string.IsNullOrEmpty(active) || active == PresetName ? null : active;
	}

	private static void Open(Window window)
	{
		if (!_created)
		{
			// Without BossMod's preset interface (an older BossMod) nothing is decided; it is asked again next time.
			if (BMRInfo_IPCSubscriber.PresetsCreate?.Invoke(PresetJson, true) is not { } created)
			{
				TraceWhyNot("BossMod has no preset interface");
				return;
			}

			_created = true;
			_refused = !created;
			if (_refused)
			{
				DefenseTrace.Line($"positional move: BossMod refused the preset {PresetName} - its modules or options have other names now");
				return;
			}
		}

		if (_active && window.Action == _windowAction && window.Target == _windowTarget && window.Side == _windowSide)
		{
			return;
		}

		if (BMRInfo_IPCSubscriber.PresetsAddTransientStrategy?.Invoke(PresetName, PositionalModule, "Positional", window.Side.ToString()) != true
			|| (!_active && BMRInfo_IPCSubscriber.PresetsSetActive?.Invoke(PresetName) != true))
		{
			TraceWhyNot("BossMod did not take the side");
			return;
		}

		_active = true;
		(_windowAction, _windowTarget, _windowSide, _windowName) = (window.Action, window.Target, window.Side, window.Name);
		_tracedWhyNot = null;
		DefenseTrace.Line($"positional move: BossMod walks you to the {window.Side.ToString().ToLowerInvariant()} for {window.Name}");
	}

	private static void Close(string why)
	{
		if (!_active)
		{
			return;
		}

		_active = false;
		if (BMRInfo_IPCSubscriber.PresetsGetActive?.Invoke() == PresetName)
		{
			_ = BMRInfo_IPCSubscriber.PresetsClearActive?.Invoke();
		}

		// Pressed: the next GCD is another one now; its answer judges the move.
		if (ActionUpdater.NextGCDAction?.ID != _windowAction)
		{
			(_judgeAction, _judgeTarget) = (_windowAction, _windowTarget);
		}

		DefenseTrace.Line($"positional move: off for {_windowName} - {why}");
	}

	private static void Fail(ulong targetId, string why)
	{
		Close(why);
		_ = _stoppedTargets.Add(targetId);
		_offInArea = _failedInArea;
		_failedInArea = true;
		DefenseTrace.Line(_offInArea
			? $"positional move: stopped for this area - {why}, the second time"
			: $"positional move: stopped for this target - {why}");
	}

	private static void TraceWhyNot(string why)
	{
		if (_tracedWhyNot != why)
		{
			_tracedWhyNot = why;
			DefenseTrace.Line($"positional move: not now - {why}");
		}
	}
}

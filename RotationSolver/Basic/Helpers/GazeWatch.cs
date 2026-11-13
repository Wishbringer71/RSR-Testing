using ECommons.Logging;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using Framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using RotationSolver.Basic.Resources;
using RotationSolver.Basic.Tweaks;
using System.Collections.Concurrent;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Gaze casts that reach the player, whether an action would look into one, and turning away before one lands (concept 13,
/// "Nicht in einen Blick drehen" and "Zeitraum und Wegdrehen", A770, A772, A774). An action that needs to face its target
/// turns the player towards it when pressed, and the player keeps that heading; a gaze hits whoever looks at its caster as
/// it lands, an inverted gaze whoever looks away. In the World of Darkness Angra Mainyu casts Mortal Gaze twice at once,
/// #3281 with the gaze mark of the sheet and #3499 from a helper without it; the Doom came after #3499 (the owner's
/// recording of 13.11.2026). Beatrice's Death Foreseen carries no mark at all, which BossModReborn's fight modules name
/// (GazeActions). It works without BossModReborn running (his rule of 13.11.2026).
/// </summary>
internal static class GazeWatch
{
	// BossMod's half width of a forbidden direction for a gaze (AIHintsBuilder, ForbiddenDirections); the game names no
	// angle (A770).
	private const float DangerHalfAngleDegrees = 45f;

	// The sheet's cast VFX of a gaze (ActionCastVFX 25), as BossMod and BossModReborn read it.
	private const uint GazeCastVfx = 25;

	private enum Kind : byte
	{
		None,
		Gaze,
		Inverted,
	}

	private readonly record struct Gazer(IBattleChara Caster, bool Inverted, float Left);

	private static readonly List<Gazer> _gazers = [];
	private static readonly ConcurrentDictionary<uint, Kind> _kinds = new();
	private static readonly HashSet<ulong> _traced = [];
	private static readonly HashSet<ulong> _turnTraced = [];
	private static readonly List<(float Start, float End)> _forbidden = [];
	private static long _gazersTick = long.MinValue;
	private static volatile HashSet<string>? _gazeNames;
	private static int _namesRequested;

	/// <summary>
	/// The lead before a gaze lands from which RSR holds actions turning into it and turns the player away: the longest
	/// delay to the server's answer measured in this zone plus a frame. Without a measure, or with turning off, the whole
	/// cast - an action pressed early would then keep the player looking at the caster when it lands.
	/// </summary>
	private static float Lead()
	{
		var most = ActionManagerEx.CurrentLockTweak?.DelayMost ?? 0f;
		return Service.Config.TurnAwayFromGazes && most > 0f ? most + AnimationLockTweak.MeanFrameTime : float.PositiveInfinity;
	}

	/// <summary>
	/// Whether an action of <paramref name="castTime"/> seconds that faces <paramref name="towards"/> would still face into a
	/// gaze within the lead before it lands: looking at the caster of a gaze, or away from the caster of an inverted one.
	/// </summary>
	internal static bool TurnsIntoGaze(Vector3 towards, float castTime)
	{
		var player = Player.Object;
		if (player == null)
		{
			return false;
		}

		var gazers = Gazers();
		if (gazers.Count == 0)
		{
			return false;
		}

		var facing = towards - player.Position;
		facing.Y = 0;
		if (facing.LengthSquared() <= 0f)
		{
			return false;
		}

		var lead = Lead();
		var rotation = MathF.Atan2(facing.X, facing.Z);
		foreach (var gazer in gazers)
		{
			// An instant turns the player now, and RSR turns the player away at the lead; a cast holds the heading to its end.
			if (gazer.Left - Math.Max(0f, castTime) > lead || !Unsafe(rotation, player, gazer))
			{
				continue;
			}

			if (_traced.Add(gazer.Caster.GameObjectId ^ gazer.Caster.CastActionId))
			{
				DefenseTrace.Line($"gaze: holding actions that turn {(gazer.Inverted ? "away from" : "towards")} "
					+ $"#{gazer.Caster.CastActionId} of {gazer.Caster.Name}, {gazer.Left:F1} s left, lead {(float.IsInfinity(lead) ? "the whole cast" : $"{lead:F2} s")}");
			}

			return true;
		}

		return false;
	}

	/// <summary>
	/// Every frame in combat with RSR on: within the lead before a gaze, a player who faces into one is turned to the middle
	/// of the nearest safe gap - away from a gaze, towards an inverted one. Off with "Turn away from gaze attacks".
	/// </summary>
	internal static unsafe void TurnAway()
	{
		if (!Service.Config.TurnAwayFromGazes || !DataCenter.InCombat)
		{
			return;
		}

		var player = Player.Object;
		var gazers = Gazers();
		if (player == null || gazers.Count == 0)
		{
			return;
		}

		var lead = Lead();
		var current = player.Rotation;
		var anyDue = false;
		var unsafeNow = false;
		foreach (var gazer in gazers)
		{
			if (gazer.Left > lead)
			{
				continue;
			}

			anyDue = true;
			unsafeNow |= Unsafe(current, player, gazer);
		}

		if (!anyDue || !unsafeNow || SafeRotation(player, current, lead) is not { } target)
		{
			return;
		}

		var manager = ActionManager.Instance();
		var framework = Framework.Instance();
		if (manager == null || framework == null)
		{
			return;
		}

		// As BossModReborn turns (ActionManagerEx.FaceDirection): the game's own facing call, with "Auto face target" on for
		// the call and set back after it.
		var option = framework->SystemConfig.GetConfigOption((uint)ConfigOption.AutoFaceTargetOnAction);
		var original = option != null ? option->Value.UInt : 0u;
		if (option != null)
		{
			option->Value.UInt = 1;
		}

		var point = player.Position + new Vector3(MathF.Sin(target), 0f, MathF.Cos(target));
		manager->AutoFaceTargetPosition(&point);
		if (option != null)
		{
			option->Value.UInt = original;
		}

		foreach (var gazer in gazers)
		{
			if (gazer.Left <= lead && _turnTraced.Add(gazer.Caster.GameObjectId ^ gazer.Caster.CastActionId))
			{
				DefenseTrace.Line($"gaze: turned {float.RadiansToDegrees(target - current):F0}° {(gazer.Inverted ? "towards" : "away from")} "
					+ $"#{gazer.Caster.CastActionId} of {gazer.Caster.Name}, {gazer.Left:F2} s before it lands");
			}
		}
	}

	// Whether facing <rotation> looks into <gazer>: at its caster for a gaze, away from it for an inverted one.
	private static bool Unsafe(float rotation, IBattleChara player, Gazer gazer)
	{
		var toGazer = gazer.Caster.Position - player.Position;
		toGazer.Y = 0;
		if (toGazer.LengthSquared() <= 0f)
		{
			return !gazer.Inverted;
		}

		var looksAt = MathF.Abs(Difference(rotation, MathF.Atan2(toGazer.X, toGazer.Z))) <= float.DegreesToRadians(DangerHalfAngleDegrees);
		return looksAt != gazer.Inverted;
	}

	// The middle of the safe gap nearest to <current> among the gazes within <lead>; null when no direction is safe.
	private static float? SafeRotation(IBattleChara player, float current, float lead)
	{
		var half = float.DegreesToRadians(DangerHalfAngleDegrees);
		_forbidden.Clear();
		foreach (var gazer in _gazers)
		{
			if (gazer.Left > lead)
			{
				continue;
			}

			var toGazer = gazer.Caster.Position - player.Position;
			toGazer.Y = 0;
			var at = MathF.Atan2(toGazer.X, toGazer.Z);
			if (gazer.Inverted)
			{
				AddArc(at + half, at + MathF.Tau - half);
			}
			else
			{
				AddArc(at - half, at + half);
			}
		}

		_forbidden.Sort(static (a, b) => a.Start.CompareTo(b.Start));
		float? best = null;
		var bestDistance = float.MaxValue;
		var count = _forbidden.Count;
		var end = count == 0 ? 0f : _forbidden[0].End;
		for (var i = 0; i < count; i++)
		{
			var next = i + 1 < count ? _forbidden[i + 1].Start : _forbidden[0].Start + MathF.Tau;
			end = Math.Max(end, _forbidden[i].End);
			if (next <= end)
			{
				continue;
			}

			// A gap of no width between touching arcs leaves its middle on a boundary, which still looks into a gaze.
			var middle = 0.5f * (end + next);
			var distance = MathF.Abs(Difference(middle, current));
			if (distance < bestDistance && SafeFor(middle, player, lead))
			{
				best = middle;
				bestDistance = distance;
			}
		}

		return best;
	}

	private static bool SafeFor(float rotation, IBattleChara player, float lead)
	{
		foreach (var gazer in _gazers)
		{
			if (gazer.Left <= lead && Unsafe(rotation, player, gazer))
			{
				return false;
			}
		}

		return true;
	}

	// Adds the arc from <start> to <end> (shorter than a full turn) on [0, 2π), split where it wraps.
	private static void AddArc(float start, float end)
	{
		var full = MathF.Tau;
		var length = end - start;
		start = ((start % full) + full) % full;
		var stop = start + length;
		if (stop <= full)
		{
			_forbidden.Add((start, stop));
		}
		else
		{
			_forbidden.Add((start, full));
			_forbidden.Add((0f, stop - full));
		}
	}

	// The signed difference a - b, folded into (-π, π].
	private static float Difference(float a, float b)
	{
		var full = MathF.Tau;
		var d = ((a - b) % full + full) % full;
		return d > MathF.PI ? d - full : d;
	}

	// The casters of a gaze that reaches the player, with the seconds left, once per frame.
	private static List<Gazer> Gazers()
	{
		var now = Environment.TickCount64;
		if (_gazersTick != long.MinValue && now - _gazersTick < DataCenter.FrameCacheMs)
		{
			return _gazers;
		}

		_gazersTick = now;
		_gazers.Clear();
		if (!DataCenter.InCombat)
		{
			_traced.Clear();
			_turnTraced.Clear();
			return _gazers;
		}

		foreach (var caster in DataCenter.CastingEnemies)
		{
			if (caster == null || !caster.IsCasting)
			{
				continue;
			}

			var kind = KindOf(caster.CastActionId);
			if (kind != Kind.None && DataCenter.CastReachesPlayer(caster))
			{
				_gazers.Add(new Gazer(caster, kind == Kind.Inverted, Math.Max(0f, caster.TotalCastTime - caster.CurrentCastTime)));
			}
		}

		return _gazers;
	}

	/// <summary>
	/// What the action is: an inverted gaze that BossModReborn's modules name; a gaze they name, one with the sheet's gaze
	/// mark, or an enemy action with the name of either (#3499, the helper's Mortal Gaze); else none. The names are read once
	/// in the background - until then the lists and the mark count alone - and leave out the names of inverted gazes, which
	/// a normal gaze shares ("Cat's Eye").
	/// </summary>
	private static Kind KindOf(uint actionId)
	{
		if (_kinds.TryGetValue(actionId, out var known))
		{
			return known;
		}

		if (GazeActions.Inverted.Contains(actionId))
		{
			return _kinds[actionId] = Kind.Inverted;
		}

		if (GazeActions.Gazes.Contains(actionId))
		{
			return _kinds[actionId] = Kind.Gaze;
		}

		var names = GazeNames();
		var row = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(actionId);
		var gaze = row is { } action && !action.IsPlayerAction
			&& (action.VFX.RowId == GazeCastVfx || (names != null && names.Contains(action.Name.ExtractText())));
		var kind = gaze ? Kind.Gaze : Kind.None;
		if (names != null || gaze)
		{
			_kinds[actionId] = kind;
		}

		return kind;
	}

	private static HashSet<string>? GazeNames()
	{
		if (_gazeNames == null && Interlocked.Exchange(ref _namesRequested, 1) == 0)
		{
			_ = System.Threading.Tasks.Task.Run(static () =>
			{
				try
				{
					var sheet = Service.GetSheet<Lumina.Excel.Sheets.Action>();
					var names = new HashSet<string>(StringComparer.Ordinal);
					foreach (var action in sheet)
					{
						if (!action.IsPlayerAction && (action.VFX.RowId == GazeCastVfx || GazeActions.Gazes.Contains(action.RowId))
							&& action.Name.ExtractText() is { Length: > 0 } name)
						{
							_ = names.Add(name);
						}
					}

					foreach (var id in GazeActions.Inverted)
					{
						if (sheet.GetRowOrDefault(id)?.Name.ExtractText() is { Length: > 0 } name)
						{
							_ = names.Remove(name);
						}
					}

					_gazeNames = names;
				}
				catch (Exception ex)
				{
					PluginLog.Warning($"Could not read the gaze names: {ex.Message}");
				}
			});
		}

		return _gazeNames;
	}
}

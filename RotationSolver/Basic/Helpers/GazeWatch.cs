using ECommons.Logging;
using ECommons.GameHelpers;
using RotationSolver.Basic.Resources;
using System.Collections.Concurrent;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Gaze casts that reach the player, and whether turning towards a point would look into one (concept 13, "Nicht in einen
/// Blick drehen", A770, A772). An action that needs to face its target turns the player towards it when pressed, and the
/// player keeps that heading; a gaze hits whoever looks at its caster as it lands, an inverted gaze whoever looks away. In
/// the World of Darkness Angra Mainyu casts Mortal Gaze twice at once, #3281 with the gaze mark of the sheet and #3499 from
/// a helper without it; the Doom came after #3499 (the owner's recording of 13.11.2026). Beatrice's Death Foreseen carries
/// no mark at all, which BossModReborn's fight modules name (GazeActions).
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

	private static readonly List<(IBattleChara Caster, bool Inverted)> _gazers = [];
	private static readonly ConcurrentDictionary<uint, Kind> _kinds = new();
	private static readonly HashSet<ulong> _traced = [];
	private static long _gazersTick = long.MinValue;
	private static volatile HashSet<string>? _gazeNames;
	private static int _namesRequested;

	/// <summary>
	/// Whether facing <paramref name="towards"/> would look at the caster of a gaze that reaches the player, or away from the
	/// caster of an inverted one.
	/// </summary>
	internal static bool TurnsIntoGaze(Vector3 towards)
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

		facing = Vector3.Normalize(facing);
		var limit = MathF.Cos(float.DegreesToRadians(DangerHalfAngleDegrees));
		foreach (var (gazer, inverted) in gazers)
		{
			var toGazer = gazer.Position - player.Position;
			toGazer.Y = 0;
			var looksAt = toGazer.LengthSquared() <= 0f || Vector3.Dot(facing, Vector3.Normalize(toGazer)) >= limit;
			if (looksAt != inverted)
			{
				if (_traced.Add(gazer.GameObjectId ^ gazer.CastActionId))
				{
					DefenseTrace.Line($"gaze: holding actions that turn {(inverted ? "away from" : "towards")} #{gazer.CastActionId} "
						+ $"of {gazer.Name}, {gazer.TotalCastTime - gazer.CurrentCastTime:F1} s left");
				}

				return true;
			}
		}

		return false;
	}

	// The casters of a gaze that reaches the player, once per frame.
	private static List<(IBattleChara Caster, bool Inverted)> Gazers()
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
				_gazers.Add((caster, kind == Kind.Inverted));
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
			_ = Task.Run(static () =>
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

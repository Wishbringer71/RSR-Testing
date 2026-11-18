using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace RotationSolver.Engine.Helpers;

/// <summary>
/// Positionals as the game judges them (concept 02, "Positionals: wo stehen, wer bewegt, wann True North", A819). The
/// sectors are quarter turns around the target's facing, measured from its centre to the player's; a player on the
/// boundary between rear and flank is a fraction of a step from either side, and there a small turn of the target between
/// press and judgement flips the side. The game's damage entry carries the bonus percent of the hit (combo and positional
/// together) and no flag of its own, so the values that mean a hit are learned here per action, combo state and level:
/// under True North, or standing deeper in the needed side than its boundary, the positional landed; standing deep in the
/// wrong side without True North, it did not. Where the game then judges against RSR's measure, the boundary is less
/// certain than it looks, and a band of that width makes RSR use True North there while its charges are full.
/// </summary>
internal static class PositionalWatch
{
	// Front, flank and rear meet half a quarter turn off the target's front and back.
	private static readonly float HalfQuarter = MathF.Atan(1f);

	private readonly record struct Key(uint Action, bool Comboed, int Level);

	private readonly record struct Press(Key Key, ulong TargetId, EnemyPositional Needed, float Margin, bool Deep, bool DeepWrong,
		bool TrueNorth);

	private static Press? _pending;
	private static readonly Dictionary<Key, HashSet<byte>> _hits = [];
	private static readonly Dictionary<Key, HashSet<byte>> _misses = [];

	/// <summary>
	/// Radians from the boundary within which the game has judged against RSR's measure in this area; zero while it never
	/// has.
	/// </summary>
	internal static float Band { get; private set; }

	/// <summary>
	/// Set by the positional flag when it is raised only because the player stands within <see cref="Band"/> of the
	/// boundary on the needed side; True North then waits for full charges.
	/// </summary>
	internal static bool UncertainOnly { get; set; }

	/// <summary>
	/// Whether the player's True North still lasts <paramref name="time"/> seconds from now.
	/// </summary>
	internal static bool TrueNorthCovers(float time)
	{
		return StatusHelper.PlayerHasStatus(true, StatusID.TrueNorth) && !StatusHelper.PlayerWillStatusEnd(time, true, StatusID.TrueNorth);
	}

	/// <summary>
	/// Whether the player stands on the needed side, but within the band where the game has judged otherwise before.
	/// </summary>
	internal static bool InBand(IBattleChara target, EnemyPositional needed)
	{
		return Band > 0 && FromFront(target) is { } angle && SideMargin(angle, needed) is var margin && margin >= 0 && margin < Band;
	}

	/// <summary>
	/// A positional action left for the server (<c>ActionManagerEx.UseActionLocationDetour</c>): where the player stood
	/// against the side it needs, kept until its effect arrives.
	/// </summary>
	internal static unsafe void Pressed(ActionManager* manager, uint actionId, ulong targetId)
	{
		var adjusted = manager->GetAdjustedActionId(actionId);
		if (!ConfigurationHelper.ActionPositional.TryGetValue((ActionID)adjusted, out var needed)
			|| Svc.Objects.SearchById(targetId) is not IBattleChara target
			|| !target.HasPositional()
			|| FromFront(target) is not { } angle)
		{
			return;
		}

		var comboFrom = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(adjusted)?.ActionCombo.RowId ?? 0;
		var comboed = comboFrom != 0 && manager->Combo.Action == comboFrom;
		var side = SideAt(angle);
		_pending = new Press(new Key(adjusted, comboed, DataCenter.PlayerSyncedLevel()), targetId, needed,
			SideMargin(angle, needed), side == needed && DeepIn(angle, side), side != needed && DeepIn(angle, side),
			StatusHelper.PlayerHasStatus(true, StatusID.TrueNorth));
	}

	/// <summary>
	/// The effect of the pressed positional action reached its target with this bonus percent: learn what it means and
	/// judge it.
	/// </summary>
	internal static void Landed(uint actionId, ulong targetId, byte bonus)
	{
		if (_pending is not { } press || press.Key.Action != actionId || press.TargetId != targetId)
		{
			return;
		}

		_pending = null;
		if (press.TrueNorth || press.Deep)
		{
			Add(_hits, press.Key, bonus);
		}
		else if (press.DeepWrong)
		{
			Add(_misses, press.Key, bonus);
		}

		var hit = _hits.TryGetValue(press.Key, out var hits) && hits.Contains(bonus);
		var miss = _misses.TryGetValue(press.Key, out var misses) && misses.Contains(bonus);
		if (!press.TrueNorth && hit != miss && (hit ? press.Margin < 0 : press.Margin >= 0))
		{
			Band = MathF.Max(Band, MathF.Abs(press.Margin));
		}

		var name = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(actionId)?.Name.ExtractText() ?? actionId.ToString();
		DefenseTrace.Line($"positional: {name} needs {press.Needed}{(press.Key.Comboed ? " in combo" : string.Empty)}, "
			+ $"{float.RadiansToDegrees(press.Margin):F1}° inside its side, true north {(press.TrueNorth ? "yes" : "no")} - "
			+ $"{(hit == miss ? "unknown" : hit ? "hit" : "miss")} (bonus {bonus}%), band {float.RadiansToDegrees(Band):F1}°");
	}

	/// <summary>
	/// A new area: another fight's turns, the band starts over. What a bonus means stays, it belongs to the action.
	/// </summary>
	internal static void AreaChanged()
	{
		_pending = null;
		Band = 0;
		UncertainOnly = false;
	}

	private static void Add(Dictionary<Key, HashSet<byte>> learned, Key key, byte bonus)
	{
		if (!learned.TryGetValue(key, out var values))
		{
			learned[key] = values = [];
		}

		values.Add(bonus);
	}

	// Radians from the target's front to the player, 0 in front and pi behind; null with the player on its centre.
	private static float? FromFront(IBattleChara target)
	{
		if (Player.Object is not { } player)
		{
			return null;
		}

		var face = target.GetFaceVector();
		var offset = player.Position - target.Position;
		var direction = new Vector2(offset.X, offset.Z);
		var facing = new Vector2(face.X, face.Z);
		if (direction.LengthSquared() <= float.Epsilon || facing.LengthSquared() <= float.Epsilon)
		{
			return null;
		}

		return MathF.Acos(Math.Clamp(Vector2.Dot(Vector2.Normalize(direction), Vector2.Normalize(facing)), -1f, 1f));
	}

	private static EnemyPositional SideAt(float angle)
	{
		return angle < HalfQuarter ? EnemyPositional.Front : angle > MathF.PI - HalfQuarter ? EnemyPositional.Rear : EnemyPositional.Flank;
	}

	// How far inside the side the player stands, negative outside it.
	private static float SideMargin(float angle, EnemyPositional side)
	{
		return side switch
		{
			EnemyPositional.Front => HalfQuarter - angle,
			EnemyPositional.Rear => angle - (MathF.PI - HalfQuarter),
			_ => MathF.Min(angle - HalfQuarter, MathF.PI - HalfQuarter - angle),
		};
	}

	// Deeper in the side than at its boundary: nearer to the side's centre line than to its edge.
	private static bool DeepIn(float angle, EnemyPositional side)
	{
		var centre = side switch
		{
			EnemyPositional.Front => angle,
			EnemyPositional.Rear => MathF.PI - angle,
			_ => MathF.Abs(angle - HalfQuarter - HalfQuarter),
		};
		return SideMargin(angle, side) > centre;
	}
}

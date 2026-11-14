using Dalamud.Game;
using ECommons.Logging;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using Framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using RotationSolver.Basic.Resources;
using RotationSolver.Basic.Tweaks;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Gazes that reach the player, whether an action would look into one, and turning away before one lands (concept 13,
/// "Nicht in einen Blick drehen", "Zeitraum und Wegdrehen" and "Umkehrung und Auge zur Laufzeit", A770, A772, A774, A776).
/// An action that needs to face its target turns the player towards it when pressed, and the player keeps that heading; a
/// gaze hits whoever looks at its eye as it lands, an inverted gaze whoever looks away. Every gaze is an eye with a place,
/// a sense and the seconds left, from four sources:
/// <list type="bullet">
/// <item>casts: the sheet's gaze mark, BossModReborn's lists (GazeActions) and the names of either, the eye at the cast's
/// location as BossModReborn places it, else at the caster;</item>
/// <item>statuses on the player whose text names the enemy to look at or away from (Nymeia's Arcane Attraction);</item>
/// <item>"look" and "no look" markers on the player (the Windurst alliance raid's last boss), the eye at the largest
/// enemies casting;</item>
/// <item>a party member whose status makes them the eye when it expires (Glassy-eyed).</item>
/// </list>
/// It works without BossModReborn running (his rule of 13.11.2026).
/// </summary>
internal static partial class GazeWatch
{
	// BossMod's half width of a forbidden direction for a gaze (AIHintsBuilder, ForbiddenDirections); the game names no
	// angle (A770).
	private const float DangerHalfAngleDegrees = 45f;

	// The sheet's cast VFX of a gaze (ActionCastVFX 25), as BossMod and BossModReborn read it.
	private const uint GazeCastVfx = 25;

	// How far a running cast's target may lie off the facing until the game has the spell (BossModReborn,
	// ActionManagerEx, empirical; A777). Between this and the gaze's half width the cast lands and the gaze misses.
	private const float CastHalfAngleDegrees = 75f;

	// How far an oriented action's target may lie off the facing for the action to go off (BossModReborn,
	// ActionManagerEx, empirical; A778). The game's "Auto face target" turns the player straight at the target instead.
	private const float ActionHalfAngleDegrees = 45f;

	private enum Kind : byte
	{
		None,
		Gaze,
		Inverted,
	}

	private enum Origin : byte
	{
		Cast,
		PlayerStatus,
		MemberStatus,
		Marker,
	}

	// What the eye came from, kept as values so a frame builds no text; Describe writes it for the trace.
	private readonly record struct Gazer(Vector3 Eye, ulong Key, bool Inverted, float Left, Origin From, uint Id, IGameObject? Who);

	private sealed class Texts
	{
		public required HashSet<string> GazeNames { get; init; }
		public required HashSet<string> InvertedNames { get; init; }
		public required Dictionary<uint, (string Enemy, bool Inverted)> PlayerStatuses { get; init; }
		public required HashSet<uint> MemberEyes { get; init; }
	}

	private static readonly List<Gazer> _gazers = [];
	private static readonly ConcurrentDictionary<uint, Kind> _kinds = new();
	private static readonly Dictionary<uint, string> _englishNames = [];
	private static readonly HashSet<ulong> _traced = [];
	private static readonly HashSet<ulong> _turnTraced = [];
	private static readonly List<(float Start, float End)> _forbidden = [];
	private static readonly List<(DateTime Seen, bool Inverted, float Duration)> _markers = [];
	private static readonly List<IBattleChara> _largest = [];
	private static long _gazersTick = long.MinValue;
	private static volatile Texts? _texts;
	private static int _textsRequested;

	[GeneratedRegex(@"if (?:gazing at|looking at|facing) (?<name>.+?) when this effect expires", RegexOptions.IgnoreCase)]
	private static partial Regex LookAtText();

	[GeneratedRegex(@"if (?:looking|turning|facing) away from (?<name>.+?) when this effect expires", RegexOptions.IgnoreCase)]
	private static partial Regex LookAwayText();

	[GeneratedRegex(@"petrifying attack in all directions", RegexOptions.IgnoreCase)]
	private static partial Regex MemberEyeText();

	// A head marker's effect: vfx/lockon/eff/<name>.avfx; "_no_lk_" is the marker to look away, "_lk_" to look at, and
	// "_<n>s_" its seconds (Lockon 680 and 681, the only two the sheet names so).
	[GeneratedRegex(@"^vfx/lockon/eff/.*?_(?<no>no_)?lk_(?<seconds>\d+)s_", RegexOptions.IgnoreCase)]
	private static partial Regex LookMarker();

	/// <summary>
	/// The lead before a gaze lands from which RSR holds actions turning into it and turns the player away: the longest
	/// delay to the server's answer measured in this zone plus a frame. Without a measure, or with turning off, the whole
	/// cast - an action pressed early would then keep the player looking at the eye when it lands.
	/// </summary>
	private static float Lead()
	{
		var most = ActionManagerEx.CurrentLockTweak?.DelayMost ?? 0f;
		return Service.Config.TurnAwayFromGazes && most > 0f ? most + AnimationLockTweak.MeanFrameTime : float.PositiveInfinity;
	}

	/// <summary>
	/// Whether an action of <paramref name="castTime"/> seconds that faces <paramref name="towards"/> would still face into a
	/// gaze within the lead before it lands: looking at the eye of a gaze, or away from the eye of an inverted one.
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

		// A778: with turning on, RSR presses the action facing a safe way within the action's cone around the target
		// (PrepareFacing), and a cast keeps a safe way within its wider limit until the game has it (TurnAway). It waits
		// only where neither exists - above all on the eye's own caster.
		var held = false;
		if (!float.IsInfinity(lead))
		{
			held = SafeNear(player, rotation, float.DegreesToRadians(ActionHalfAngleDegrees), lead) == null
				|| (castTime > 0f && SafeNear(player, rotation, float.DegreesToRadians(CastHalfAngleDegrees), castTime + lead) == null);
		}

		if (!float.IsInfinity(lead) && !held)
		{
			return false;
		}

		foreach (var gazer in gazers)
		{
			// Without turning: an instant turns the player straight at the target, a cast holds that heading to its end.
			// With turning, the hold names the first gaze that lands while the action needs its facing.
			if (gazer.Left - Math.Max(0f, castTime) > lead || (float.IsInfinity(lead) && !Unsafe(rotation, player, gazer)))
			{
				continue;
			}

			if (_traced.Add(gazer.Key))
			{
				DefenseTrace.Line($"gaze: holding actions that turn {(gazer.Inverted ? "away from" : "towards")} {Describe(gazer)}, "
					+ $"{gazer.Left:F1} s left, lead {(float.IsInfinity(lead) ? "the whole cast" : $"{lead:F2} s")}");
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

		if (!anyDue || !unsafeNow)
		{
			return;
		}

		// A777: a running cast the game does not have yet keeps going when the new facing stays within its limit around
		// the cast's target; only where no safe direction is left there does the turn break it.
		var near = CastAim(player, out var aim) ? SafeNear(player, aim, float.DegreesToRadians(CastHalfAngleDegrees), lead)
			: TargetAim(player, out aim) ? SafeNear(player, aim, float.DegreesToRadians(ActionHalfAngleDegrees), lead)
			: null;
		if ((near ?? SafeRotation(player, current, lead)) is not { } target)
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
			if (gazer.Left <= lead && _turnTraced.Add(gazer.Key))
			{
				DefenseTrace.Line($"gaze: turned {float.RadiansToDegrees(Difference(target, current)):F0}° {(gazer.Inverted ? "towards" : "away from")} "
					+ $"{Describe(gazer)}, {gazer.Left:F2} s before it lands");
			}
		}
	}

	/// <summary>
	/// Right before an oriented action goes off while a gaze is within the lead (A778, as BossModReborn's smart
	/// orientation): the facing stays when the target lies within the action's cone and it looks into no gaze, else
	/// RSR turns to the middle of the widest safe stretch of that cone; either way "Auto face target" is off for the
	/// call, so the game does not turn the player straight at the target. Returns the option's value to put back.
	/// </summary>
	internal static unsafe uint? PrepareFacing(Vector3 towards)
	{
		var player = Player.Object;
		if (!Service.Config.TurnAwayFromGazes || !DataCenter.InCombat || player == null)
		{
			return null;
		}

		var lead = Lead();
		var gazers = Gazers();
		var due = false;
		foreach (var gazer in gazers)
		{
			due |= gazer.Left <= lead;
		}

		var facing = towards - player.Position;
		facing.Y = 0;
		if (!due || facing.LengthSquared() <= 0f)
		{
			return null;
		}

		var aim = MathF.Atan2(facing.X, facing.Z);
		var half = float.DegreesToRadians(ActionHalfAngleDegrees);
		var current = player.Rotation;
		float? turnTo = null;
		if (MathF.Abs(Difference(current, aim)) > half || !SafeFor(current, player, lead))
		{
			turnTo = SafeNear(player, aim, half, lead);
			if (turnTo == null)
			{
				return null;
			}
		}

		var manager = ActionManager.Instance();
		var framework = Framework.Instance();
		var option = framework == null ? null : framework->SystemConfig.GetConfigOption((uint)ConfigOption.AutoFaceTargetOnAction);
		if (manager == null || option == null)
		{
			return null;
		}

		var original = option->Value.UInt;
		if (turnTo is { } target)
		{
			option->Value.UInt = 1;
			var point = player.Position + new Vector3(MathF.Sin(target), 0f, MathF.Cos(target));
			manager->AutoFaceTargetPosition(&point);
			foreach (var gazer in gazers)
			{
				if (gazer.Left <= lead && _turnTraced.Add(gazer.Key))
				{
					DefenseTrace.Line($"gaze: turned {float.RadiansToDegrees(Difference(target, current)):F0}° for an action, "
						+ $"{float.RadiansToDegrees(Difference(target, aim)):F0}° off its target, {(gazer.Inverted ? "towards" : "away from")} "
						+ $"{Describe(gazer)}, {gazer.Left:F2} s before it lands");
				}
			}
		}

		option->Value.UInt = 0;
		return original;
	}

	/// <summary>Puts "Auto face target" back after <see cref="PrepareFacing"/>.</summary>
	internal static unsafe void RestoreFacing(uint? original)
	{
		var framework = Framework.Instance();
		if (original is not { } value || framework == null)
		{
			return;
		}

		var option = framework->SystemConfig.GetConfigOption((uint)ConfigOption.AutoFaceTargetOnAction);
		if (option != null)
		{
			option->Value.UInt = value;
		}
	}

	// The direction of the player's current enemy target, which the next attacks face.
	private static bool TargetAim(IBattleChara player, out float aim)
	{
		aim = 0f;
		if (Svc.Targets.Target is not IBattleChara target || target.GameObjectId == player.GameObjectId || !target.IsEnemy())
		{
			return false;
		}

		var facing = target.Position - player.Position;
		facing.Y = 0;
		if (facing.LengthSquared() <= 0f)
		{
			return false;
		}

		aim = MathF.Atan2(facing.X, facing.Z);
		return true;
	}

	private static string Describe(Gazer gazer) => gazer.From switch
	{
		Origin.Cast => $"#{gazer.Id} of {gazer.Who?.Name}",
		Origin.PlayerStatus => $"{gazer.Who?.Name} by status {gazer.Id} on the player",
		Origin.MemberStatus => $"a party member with status {gazer.Id}",
		_ => $"{gazer.Who?.Name} by a marker to look {(gazer.Inverted ? "at" : "away from")}",
	};

	// Whether facing <rotation> looks into <gazer>: at its eye for a gaze, away from it for an inverted one.
	private static bool Unsafe(float rotation, IBattleChara player, Gazer gazer)
	{
		var toEye = gazer.Eye - player.Position;
		toEye.Y = 0;
		if (toEye.LengthSquared() <= 0f)
		{
			return !gazer.Inverted;
		}

		var looksAt = MathF.Abs(Difference(rotation, MathF.Atan2(toEye.X, toEye.Z))) <= float.DegreesToRadians(DangerHalfAngleDegrees);
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

			var toEye = gazer.Eye - player.Position;
			toEye.Y = 0;
			var at = MathF.Atan2(toEye.X, toEye.Z);
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

	// The middle of the widest safe stretch within <half> around <center>, against the gazes landing within <maxLeft>;
	// null when the stretch holds none. Arcs are laid out around <center> + π, so the stretch never wraps.
	private static float? SafeNear(IBattleChara player, float center, float half, float maxLeft)
	{
		var gazeHalf = float.DegreesToRadians(DangerHalfAngleDegrees);
		_forbidden.Clear();
		foreach (var gazer in _gazers)
		{
			if (gazer.Left > maxLeft)
			{
				continue;
			}

			var toEye = gazer.Eye - player.Position;
			toEye.Y = 0;
			var at = Difference(MathF.Atan2(toEye.X, toEye.Z), center) + MathF.PI;
			if (gazer.Inverted)
			{
				AddArc(at + gazeHalf, at + MathF.Tau - gazeHalf);
			}
			else
			{
				AddArc(at - gazeHalf, at + gazeHalf);
			}
		}

		_forbidden.Sort(static (a, b) => a.Start.CompareTo(b.Start));
		float? best = null;
		var bestWidth = 0f;
		var end = MathF.PI - half;
		var stop = MathF.PI + half;
		for (var i = 0; i <= _forbidden.Count; i++)
		{
			var next = i < _forbidden.Count ? Math.Min(_forbidden[i].Start, stop) : stop;
			if (next > end)
			{
				var middle = 0.5f * (end + next);
				var rotation = center + middle - MathF.PI;
				if (next - end > bestWidth && SafeFor(rotation, player, maxLeft))
				{
					best = rotation;
					bestWidth = next - end;
				}
			}

			if (i < _forbidden.Count)
			{
				end = Math.Max(end, _forbidden[i].End);
			}
		}

		return best;
	}

	// The direction of the player's own cast while it still needs it: an oriented spell the game has not taken yet
	// (ResponseSourceSequence, as MovementLock reads it), towards its target or its place.
	private static unsafe bool CastAim(IBattleChara player, out float aim)
	{
		aim = 0f;
		var info = ((FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)player.Address)->GetCastInfo();
		if (info == null || !info->IsCasting || (ActionType)info->ActionType != ActionType.Action
			|| (info->SourceSequence != 0 && info->ResponseSourceSequence == info->SourceSequence)
			|| Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(info->ActionId) is not { NeedToFaceTarget: true, Range: > 0 } action)
		{
			return false;
		}

		Vector3 towards;
		if (action.TargetArea)
		{
			towards = new Vector3(info->TargetLocation.X, info->TargetLocation.Y, info->TargetLocation.Z);
		}
		else if ((ulong)info->TargetId is var id && id != player.GameObjectId && Svc.Objects.SearchById(id) is { } target)
		{
			towards = target.Position;
		}
		else
		{
			return false;
		}

		var facing = towards - player.Position;
		facing.Y = 0;
		if (facing.LengthSquared() <= 0f)
		{
			return false;
		}

		aim = MathF.Atan2(facing.X, facing.Z);
		return true;
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

	// Every gaze on the player with its eye and the seconds left, once per frame.
	private static List<Gazer> Gazers()
	{
		var now = Environment.TickCount64;
		if (_gazersTick != long.MinValue && now - _gazersTick < DataCenter.FrameCacheMs)
		{
			return _gazers;
		}

		_gazersTick = now;
		_gazers.Clear();
		var player = Player.Object;
		if (!DataCenter.InCombat || player == null)
		{
			_traced.Clear();
			_turnTraced.Clear();
			_markers.Clear();
			return _gazers;
		}

		AddCasts();
		var texts = Texts_();
		if (texts != null)
		{
			AddPlayerStatuses(player, texts);
			AddMemberEyes(player, texts);
		}

		AddMarkers(player);
		return _gazers;
	}

	private static void AddCasts()
	{
		foreach (var caster in DataCenter.CastingEnemies)
		{
			if (caster == null || !caster.IsCasting)
			{
				continue;
			}

			var kind = KindOf(caster.CastActionId);
			if (kind != Kind.None && DataCenter.CastReachesPlayer(caster))
			{
				var eye = DataCenter.CastLocation(caster) ?? caster.Position;
				_gazers.Add(new Gazer(eye, caster.GameObjectId ^ caster.CastActionId, kind == Kind.Inverted,
					Math.Max(0f, caster.TotalCastTime - caster.CurrentCastTime), Origin.Cast, caster.CastActionId, caster));
			}
		}
	}

	// A status on the player whose text names the enemy to look at or away from when it expires (A776).
	private static void AddPlayerStatuses(IBattleChara player, Texts texts)
	{
		var statuses = player.StatusList;
		for (var i = 0; i < statuses.Length; i++)
		{
			var status = statuses[i];
			if (status == null || status.StatusId == 0 || !texts.PlayerStatuses.TryGetValue(status.StatusId, out var gaze))
			{
				continue;
			}

			if (NearestNamed(player, gaze.Enemy) is { } enemy)
			{
				// The text says what petrifies: looking at the enemy, so the gaze is normal; looking away, so it is inverted.
				_gazers.Add(new Gazer(enemy.Position, (ulong)(uint)HashCode.Combine(status.StatusId, status.SourceId), gaze.Inverted, status.RemainingTime,
					Origin.PlayerStatus, status.StatusId, enemy));
			}
		}
	}

	// A party member whose status makes them the eye for everyone else when it expires (A776).
	private static void AddMemberEyes(IBattleChara player, Texts texts)
	{
		foreach (var member in DataCenter.PartyMembers)
		{
			if (member == null || member.GameObjectId == player.GameObjectId || member.IsDead)
			{
				continue;
			}

			var statuses = member.StatusList;
			for (var i = 0; i < statuses.Length; i++)
			{
				var status = statuses[i];
				if (status != null && status.StatusId != 0 && texts.MemberEyes.Contains(status.StatusId))
				{
					_gazers.Add(new Gazer(member.Position, member.GameObjectId ^ status.StatusId, false, status.RemainingTime,
						Origin.MemberStatus, status.StatusId, member));
				}
			}
		}
	}

	// "Look" and "no look" markers on the player; the eyes are the largest enemies casting (A776).
	private static void AddMarkers(IBattleChara player)
	{
		// The queue keeps a marker 5 s (MajorUpdater) and these last 7 s, so each is remembered when first seen.
		if (!DataCenter.VfxDataQueue.IsEmpty)
		{
			foreach (var vfx in DataCenter.VfxDataQueue)
			{
				if (vfx.ObjectId != player.GameObjectId || string.IsNullOrEmpty(vfx.Path) || LookMarker().Match(vfx.Path) is not { Success: true } match
					|| !float.TryParse(match.Groups["seconds"].Value, System.Globalization.CultureInfo.InvariantCulture, out var seconds))
				{
					continue;
				}

				var known = false;
				foreach (var marker in _markers)
				{
					known |= marker.Seen == vfx.Time;
				}

				if (!known)
				{
					_markers.Add((vfx.Time, !match.Groups["no"].Success, seconds));
				}
			}
		}

		var now = DateTime.Now;
		for (var i = _markers.Count - 1; i >= 0; i--)
		{
			if ((now - _markers[i].Seen).TotalSeconds > _markers[i].Duration)
			{
				_markers.RemoveAt(i);
			}
		}

		if (_markers.Count == 0)
		{
			return;
		}

		_largest.Clear();
		var radius = 0f;
		foreach (var caster in DataCenter.CastingEnemies)
		{
			if (caster == null || !caster.IsCasting)
			{
				continue;
			}

			if (caster.HitboxRadius > radius)
			{
				radius = caster.HitboxRadius;
				_largest.Clear();
			}

			if (caster.HitboxRadius == radius)
			{
				_largest.Add(caster);
			}
		}

		foreach (var marker in _markers)
		{
			var left = marker.Duration - (float)(now - marker.Seen).TotalSeconds;
			foreach (var eye in _largest)
			{
				_gazers.Add(new Gazer(eye.Position, eye.GameObjectId ^ (ulong)marker.Seen.Ticks, marker.Inverted, Math.Max(0f, left),
					Origin.Marker, 0, eye));
			}
		}
	}

	private static IBattleChara? NearestNamed(IBattleChara player, string englishName)
	{
		IBattleChara? nearest = null;
		var best = float.MaxValue;
		foreach (var obj in Svc.Objects)
		{
			if (obj is not IBattleChara chara || chara.IsDead || !chara.IsEnemySideFigure()
				|| !string.Equals(EnglishName(chara.NameId), englishName, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			var distance = Vector3.DistanceSquared(chara.Position, player.Position);
			if (distance < best)
			{
				best = distance;
				nearest = chara;
			}
		}

		return nearest;
	}

	private static string EnglishName(uint nameId)
	{
		if (!_englishNames.TryGetValue(nameId, out var name))
		{
			name = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.BNpcName>(ClientLanguage.English)?.GetRowOrDefault(nameId)?.Singular.ExtractText() ?? string.Empty;
			_englishNames[nameId] = name;
		}

		return name;
	}

	/// <summary>
	/// What the action is. BossModReborn's runtime-built gazes first (no gaze: the eye or the sense is decided elsewhere),
	/// then its inverted and normal lists, the sheet's gaze mark, and the name of a gaze or of an inverted gaze; a name
	/// both carry counts as neither. The names are read once in the background; until then the lists and the mark count.
	/// </summary>
	private static Kind KindOf(uint actionId)
	{
		if (_kinds.TryGetValue(actionId, out var known))
		{
			return known;
		}

		if (GazeActions.Unsure.Contains(actionId))
		{
			return _kinds[actionId] = Kind.None;
		}

		if (GazeActions.Inverted.Contains(actionId))
		{
			return _kinds[actionId] = Kind.Inverted;
		}

		if (GazeActions.Gazes.Contains(actionId))
		{
			return _kinds[actionId] = Kind.Gaze;
		}

		var texts = Texts_();
		var row = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(actionId);
		var kind = Kind.None;
		if (row is { } action && !action.IsPlayerAction)
		{
			var name = action.Name.ExtractText();
			kind = action.VFX.RowId == GazeCastVfx || (texts != null && texts.GazeNames.Contains(name)) ? Kind.Gaze
				: texts != null && texts.InvertedNames.Contains(name) ? Kind.Inverted
				: Kind.None;
		}

		if (texts != null || kind != Kind.None)
		{
			_kinds[actionId] = kind;
		}

		return kind;
	}

	private static Texts? Texts_()
	{
		if (_texts == null && Interlocked.Exchange(ref _textsRequested, 1) == 0)
		{
			_ = System.Threading.Tasks.Task.Run(static () =>
			{
				try
				{
					_texts = ReadTexts();
				}
				catch (Exception ex)
				{
					PluginLog.Warning($"Could not read the gaze texts: {ex.Message}");
				}
			});
		}

		return _texts;
	}

	private static Texts ReadTexts()
	{
		var actions = Service.GetSheet<Lumina.Excel.Sheets.Action>();
		var gazeNames = new HashSet<string>(StringComparer.Ordinal);
		var invertedNames = new HashSet<string>(StringComparer.Ordinal);
		foreach (var action in actions)
		{
			if (action.IsPlayerAction || GazeActions.Unsure.Contains(action.RowId) || action.Name.ExtractText() is not { Length: > 0 } name)
			{
				continue;
			}

			if (GazeActions.Inverted.Contains(action.RowId))
			{
				_ = invertedNames.Add(name);
			}
			else if (action.VFX.RowId == GazeCastVfx || GazeActions.Gazes.Contains(action.RowId))
			{
				_ = gazeNames.Add(name);
			}
		}

		// A name both kinds carry ("Cat's Eye") tells neither.
		foreach (var name in invertedNames)
		{
			_ = gazeNames.Remove(name);
		}

		invertedNames.ExceptWith(GazeNamesOfBoth(actions, invertedNames));

		// Statuses read in English: the text names the enemy, the language of the client does not matter.
		var playerStatuses = new Dictionary<uint, (string Enemy, bool Inverted)>();
		var memberEyes = new HashSet<uint>();

		foreach (var status in Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(ClientLanguage.English)!)
		{
			var text = status.Description.ExtractText().Replace('\n', ' ');
			if (LookAwayText().Match(text) is { Success: true } away)
			{
				playerStatuses[status.RowId] = (away.Groups["name"].Value.Trim(), true);
			}
			else if (LookAtText().Match(text) is { Success: true } at)
			{
				playerStatuses[status.RowId] = (at.Groups["name"].Value.Trim(), false);
			}

			if (MemberEyeText().IsMatch(text))
			{
				_ = memberEyes.Add(status.RowId);
			}
		}

		return new Texts { GazeNames = gazeNames, InvertedNames = invertedNames, PlayerStatuses = playerStatuses, MemberEyes = memberEyes };
	}

	// Inverted names that a normal gaze also carries.
	private static HashSet<string> GazeNamesOfBoth(Lumina.Excel.ExcelSheet<Lumina.Excel.Sheets.Action> actions, HashSet<string> invertedNames)
	{
		var both = new HashSet<string>(StringComparer.Ordinal);
		foreach (var action in actions)
		{
			if (!action.IsPlayerAction && !GazeActions.Inverted.Contains(action.RowId) && !GazeActions.Unsure.Contains(action.RowId)
				&& (action.VFX.RowId == GazeCastVfx || GazeActions.Gazes.Contains(action.RowId))
				&& action.Name.ExtractText() is { Length: > 0 } name && invertedNames.Contains(name))
			{
				_ = both.Add(name);
			}
		}

		return both;
	}
}

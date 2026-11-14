using Dalamud.Game;
using ECommons.Logging;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Hooks;
using ECommons.Hooks.ActionEffectTypes;
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

	// A809: the room RSR keeps to the gaze's edge in its own facing - the edge lies between the last hit (44.2°) and the
	// first spared (45.1°) of 117 measured targets (#3499, #44355, #48153), and ACT gives a facing to 0.57°.
	private const float MeasuredEdgeDegrees = 1f;

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

	// What the eye came from, kept as values so a frame builds no text; Describe writes it for the trace. Trusted is
	// false for a gaze that hit despite RSR's facing in this build (A786, A809): it still holds actions over the whole cast,
	// and RSR no longer turns for it - a facing chosen for another gaze still keeps clear of it.
	// Margin (A789) widens the gaze's half width by the angle the player's and the eye's movement makes until it resolves.
	private readonly record struct Gazer(Vector3 Eye, ulong Key, bool Inverted, float Left, Origin From, uint Id, IGameObject? Who,
		bool Trusted = true, float Margin = 0f);

	// A783, A786, A809: a gaze RSR acted on, until a packet judges it (Learn). Source and Action name a cast gaze's own
	// cast, whose packet resolves it; Action 0 for a marker, status or member gaze, which any enemy packet from its Due on
	// judges, Due moving on with the gaze while it is seen (CloseAvoided).
	private readonly record struct Pending(ulong Key, ulong Learn, string? Name, ulong Source, uint Action, DateTime Due, string Text);

	// A786: a cast gaze whose bar ended: the server resolves it about 0.3 s later, so it stays due until its packet.
	private readonly record struct Landing(Gazer Gazer, DateTime Ended);

	private sealed class Texts
	{
		public required HashSet<string> GazeNames { get; init; }
		public required HashSet<string> InvertedNames { get; init; }
		public required HashSet<string> InvertedAll { get; init; }
		public required Dictionary<uint, (string Enemy, bool Inverted)> PlayerStatuses { get; init; }
		public required HashSet<uint> MemberEyes { get; init; }
	}

	private static readonly List<Gazer> _gazers = [];
	private static readonly ConcurrentDictionary<uint, Kind> _kinds = new();
	private static readonly Dictionary<uint, string> _englishNames = [];
	private static readonly HashSet<ulong> _traced = [];
	private static readonly HashSet<ulong> _turnTraced = [];
	private static readonly HashSet<ulong> _refusedTraced = [];
	private static readonly List<(float Start, float End)> _forbidden = [];
	// A "look" or "no look" marker and the eyes it was matched to, kept after their cast ends until it resolves (A789).
	private sealed class MarkerWatch
	{
		public DateTime Seen;
		public bool Inverted;
		public float Duration;
		public bool NoEyeTraced;
		public readonly List<IBattleChara> Eyes = [];
	}

	private static readonly List<MarkerWatch> _markers = [];
	private static readonly Dictionary<ulong, Vector3> _eyesBefore = [];
	private static Vector3 _playerBefore;
	private static long _movedTick = long.MinValue;
	private static readonly HashSet<ulong> _heldFacing = [];
	private static float _updateGap;
	private static DateTime _lastOrientedAction = DateTime.MinValue;
	private static readonly List<IBattleChara> _largest = [];
	private static long _gazersTick = long.MinValue;
	private static readonly List<Pending> _pending = [];
	// A789: the gazes that hit despite RSR's facing, by gaze (A809: no longer by zone), until RSR is loaded again; true where the packet
	// bore out RSR's eye and sense (only the turning ends), false where it did not (RSR leaves the gaze alone).
	private static readonly Dictionary<ulong, bool> _distrusted = [];

	// A809: when each learned gaze failed first - a packet of the same mechanic shortly after is no second hit.
	private static readonly Dictionary<ulong, DateTime> _failedAt = [];
	private static readonly List<Gazer> _casts = [];
	private static readonly List<Gazer> _castsBefore = [];
	private static readonly List<Landing> _landing = [];
	private static readonly Dictionary<ulong, (uint Action, DateTime BarEnd)> _barEnds = [];
	private static readonly Dictionary<uint, bool> _areaTargeted = [];
	private static float _landingMost;
	private static (float Rotation, Vector3 Position, ulong Key)? _turnCheck;
	private static readonly Dictionary<uint, string> _actionNames = [];
	private static uint _zoneTerritory;
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
		// A791: plus the measured time between two updates, which "Minimum update interval" or a stall can make longer
		// than a frame.
		return Service.Config.TurnAwayFromGazes && most > 0f ? most + Math.Max(AnimationLockTweak.MeanFrameTime, _updateGap) : float.PositiveInfinity;
	}

	/// <summary>
	/// Whether an action of <paramref name="castTime"/> seconds that faces <paramref name="towards"/> and goes off in
	/// <paramref name="startsIn"/> seconds would still face into a gaze within the lead before it lands: looking at the eye
	/// of a gaze, or away from the eye of an inverted one. A809: the server keeps an action's facing for
	/// <paramref name="keeps"/> seconds after it goes off (its lock), so a gaze landing then counts too - the same while the
	/// facing at the press is chosen and the gaze is watched.
	/// </summary>
	internal static bool TurnsIntoGaze(Vector3 towards, float castTime, float startsIn, bool moves = false, float keeps = 0f)
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
		var turning = !float.IsInfinity(lead);
		var rotation = MathF.Atan2(facing.X, facing.Z);
		var span = Math.Max(0f, startsIn) + Math.Max(0f, castTime);
		var reach = lead + Math.Max(0f, keeps);

		// A778: with turning on, RSR presses the action facing a safe way within the action's cone around the target
		// (PrepareFacing), and a cast keeps a safe way within its wider limit until the game has it (TurnAway). It waits
		// only where neither exists - above all on the eye's own caster. A786: counted from when the game runs the action,
		// which may be a GCD or a lock later than the press.
		var held = turning && (SafeNear(player, rotation, float.DegreesToRadians(ActionHalfAngleDegrees), Math.Max(0f, startsIn) + reach) == null
			|| (castTime > 0f && SafeNear(player, rotation, float.DegreesToRadians(CastHalfAngleDegrees), span + reach) == null));

		foreach (var gazer in gazers)
		{
			// Without turning, and for a gaze that hit despite RSR's facing (A786): the game turns the player straight at the
			// target, and a cast holds that heading to its end, so the whole cast counts. With turning, the hold names the
			// first gaze that lands while the action needs its facing. A789: an action that moves the player keeps facing the
			// target until it lands, so the exact direction counts until then; and after the game undid RSR's turn for a gaze,
			// every action that faces waits until its packet, so no further turn of the game starts.
			var due = gazer.Left - span <= (turning ? reach : float.PositiveInfinity);
			var holds = _heldFacing.Contains(gazer.Key) ? due
				: moves ? (turning && gazer.Trusted ? due : true) && Unsafe(rotation, player, gazer)
				: !turning || !gazer.Trusted ? Unsafe(rotation, player, gazer)
				: held && due;
			if (!holds)
			{
				continue;
			}

			if (_traced.Add(gazer.Key))
			{
				DefenseTrace.Line($"gaze: holding actions that turn {(gazer.Inverted ? "away from" : "towards")} {Describe(gazer)}, "
					+ $"{gazer.Left:F1} s left, lead {(turning && gazer.Trusted ? $"{lead:F2} s" : "the whole cast")}");
			}

			// A809 (sixth review, M6): a gaze RSR keeps a target out of the choice for is learned too - also without a measured
			// lead or with turning off, where a wrong sense would otherwise keep the target towards the real eye out each cast.
			Watch(gazer);
			return true;
		}

		return false;
	}

	/// <summary>
	/// Seconds until the game runs an action pressed now: the rest of the animation lock, for a GCD also the rest of the
	/// GCD - pressed earlier, the game queues it (A786).
	/// </summary>
	internal static unsafe float StartsIn(bool gcd)
	{
		var manager = ActionManager.Instance();
		var locked = manager == null ? 0f : Math.Max(0f, manager->AnimationLock);
		return gcd ? Math.Max(locked, DataCenter.DefaultGCDRemain) : locked;
	}

	/// <summary>
	/// Every frame in combat with RSR on: within the lead before a gaze, a player who faces into one is turned to the middle
	/// of the widest safe gap - away from a gaze, towards an inverted one. Off with "Turn away from gaze attacks", and
	/// without a measured lead (A786).
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
			_turnCheck = null;
			return;
		}

		var lead = Lead();
		CheckTurnHeld(player, lead);
		if (float.IsInfinity(lead))
		{
			return;
		}

		var current = player.Rotation;
		var unsafeNow = false;
		foreach (var gazer in gazers)
		{
			if (gazer.Trusted && gazer.Left <= lead)
			{
				unsafeNow |= Unsafe(current, player, gazer);
			}
		}

		if (!unsafeNow)
		{
			return;
		}

		// A777: a running cast the game does not have yet keeps going when the new facing stays within its limit around
		// the cast's target; only where no safe direction is left there does the turn break it.
		var near = CastAim(player, out var aim) ? SafeNear(player, aim, float.DegreesToRadians(CastHalfAngleDegrees), lead)
			: TargetAim(player, out aim) ? SafeNear(player, aim, float.DegreesToRadians(ActionHalfAngleDegrees), lead)
			: null;
		if ((near ?? SafeRotation(player, lead)) is not { } target || !Face(target))
		{
			return;
		}

		ulong first = 0;
		foreach (var gazer in gazers)
		{
			if (gazer.Trusted && gazer.Left <= lead)
			{
				first = first == 0 ? gazer.Key : first;
				if (_turnTraced.Add(gazer.Key))
				{
					DefenseTrace.Line($"gaze: turned {float.RadiansToDegrees(Difference(target, current)):F0}° {(gazer.Inverted ? "towards" : "away from")} "
						+ $"{Describe(gazer)}, {gazer.Left:F2} s before it lands");
				}
			}
		}

		// M1 of the review (A786): the game may turn the player back the next frame while it interpolates a turn
		// (BossModReborn sets DesiredRotation for that, at offsets of its own); RSR measures it instead.
		_turnCheck = (player.Rotation, player.Position, first);
		Watch(gazers, lead);
	}

	// As BossModReborn turns (ActionManagerEx.FaceDirection): the game's own facing call, with "Auto face target" on for
	// the call and set back after it.
	private static unsafe bool Face(float rotation)
	{
		var player = Player.Object;
		var manager = ActionManager.Instance();
		var framework = Framework.Instance();
		if (player == null || manager == null || framework == null)
		{
			return false;
		}

		var option = framework->SystemConfig.GetConfigOption((uint)ConfigOption.AutoFaceTargetOnAction);
		var original = option != null ? option->Value.UInt : 0u;
		if (option != null)
		{
			option->Value.UInt = 1;
		}

		var point = player.Position + new Vector3(MathF.Sin(rotation), 0f, MathF.Cos(rotation));
		manager->AutoFaceTargetPosition(&point);
		if (option != null)
		{
			option->Value.UInt = original;
		}

		return true;
	}

	// Whether the facing RSR set last frame still stands while the player did not move; once per gaze in the trace.
	private static void CheckTurnHeld(IBattleChara player, float lead)
	{
		if (_turnCheck is not { } check)
		{
			return;
		}

		_turnCheck = null;
		// A791: only a facing that looks into a gaze again counts; another safe one (BossModReborn's) is no harm.
		if (player.Position == check.Position && player.Rotation != check.Rotation && !SafeFor(player.Rotation, player, lead)
			&& _heldFacing.Add(check.Key))
		{
			DefenseTrace.Line($"gaze: turn undone the next frame, by the game or another tool, {float.RadiansToDegrees(Difference(player.Rotation, check.Rotation)):F0}° "
				+ "- RSR turns again each frame and holds every action that faces until the gaze resolves");
		}
	}

	/// <summary>
	/// Where the game runs an action (ActionManagerEx.UseActionLocation, A786): RSR's own press, the game's queue and the
	/// player's own press alike. For an action that faces its target, <see cref="PrepareFacing"/>; null otherwise.
	/// </summary>
	internal static unsafe uint? PrepareFacingFor(uint actionId, ulong targetId, Vector3* location, out bool refuse)
	{
		refuse = false;
		var player = Player.Object;
		if (player == null || !Service.Config.TurnAwayFromGazes || !DataCenter.InCombat
			|| Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(actionId) is not { NeedToFaceTarget: true, Range: not 0 } action)
		{
			return null;
		}

		// A791: the server keeps an action's facing for a while after it (concept 13, "Richtung einer Aktion"), so the safe
		// facing is chosen against every gaze landing within the lead plus this action's lock.
		_lastOrientedAction = DateTime.Now;
		var horizon = ActionManagerEx.CurrentLockTweak?.AbilityWindow(actionId) ?? 0f;
		if (action.TargetArea)
		{
			return location == null ? null : PrepareFacing(*location, horizon, out refuse);
		}

		return targetId != player.GameObjectId && Svc.Objects.SearchById(targetId) is { } target ? PrepareFacing(target.Position, horizon, out refuse) : null;
	}

	/// <summary>
	/// Right before an oriented action goes off while a gaze is within the lead (A778, as BossModReborn's smart
	/// orientation): the facing stays when the target lies within the action's cone and it looks into no gaze, else
	/// RSR turns to the middle of the widest safe stretch of that cone, and where there is none it does not turn - the
	/// game refuses a target outside the cone. Either way "Auto face target" is off for the call, so the game does not
	/// turn the player straight at the target. Returns the option's value to put back. A809: where the facing now is not
	/// safe and no safe one lies within the cone, <paramref name="refuse"/> - the action does not go.
	/// </summary>
	internal static unsafe uint? PrepareFacing(Vector3 towards, float horizon, out bool refuse)
	{
		refuse = false;
		var player = Player.Object;
		if (!Service.Config.TurnAwayFromGazes || !DataCenter.InCombat || player == null)
		{
			return null;
		}

		var lead = Lead();
		if (float.IsInfinity(lead))
		{
			return null;
		}

		var gazers = Gazers();
		var reach = lead + Math.Max(0f, horizon);
		var due = false;
		foreach (var gazer in gazers)
		{
			due |= gazer.Trusted && gazer.Left <= reach;
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
		var needsTurn = MathF.Abs(Difference(current, aim)) > half || !SafeFor(current, player, reach);
		var turnTo = needsTurn ? SafeNear(player, aim, half, reach) : null;
		if (needsTurn && turnTo == null)
		{
			refuse = true;
			foreach (var gazer in gazers)
			{
				if (gazer.Left <= reach && Unsafe(aim, player, gazer) && _refusedTraced.Add(gazer.Key))
				{
					DefenseTrace.Line($"gaze: refused an action - no safe facing within its cone while {Describe(gazer)} lands in {gazer.Left:F2} s");
				}
			}

			Watch(gazers, reach);
			return null;
		}

		// A809: every gaze within the facing's reach is watched - the server judges by this facing (vierte Prüfung, H2);
		// before the option is touched, so nothing here can leave "Auto face target" off.
		Watch(gazers, reach);
		var framework = Framework.Instance();
		var option = framework == null ? null : framework->SystemConfig.GetConfigOption((uint)ConfigOption.AutoFaceTargetOnAction);
		if (option == null)
		{
			return null;
		}

		var original = option->Value.UInt;
		if (turnTo is { } target && Face(target))
		{
			// RSR's own turn this frame is no turn the game undid (CheckTurnHeld).
			_turnCheck = null;
			foreach (var gazer in gazers)
			{
				if (gazer.Trusted && gazer.Left <= reach && _turnTraced.Add(gazer.Key))
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

	// A809: a hit counts as the second only after the first failure's mechanic has resolved - within the longest delay to
	// the packet plus the lead after it, a packet belongs to the same mechanic (#3281 and #3499; two eyes of a marker).
	private static bool SecondHit(ulong learned)
	{
		var lead = Lead();
		var window = _landingMost + (float.IsInfinity(lead) ? AnimationLockTweak.MeanFrameTime : lead);
		return !_failedAt.TryGetValue(learned, out var first) || (DateTime.Now - first).TotalSeconds > window;
	}

	// A783 (concept 13, "Lernen und gestufter Rückfall"): every gaze within the reach when RSR turned, chose a facing or
	// refused an action.
	private static void Watch(List<Gazer> gazers, float reach)
	{
		foreach (var gazer in gazers)
		{
			if (gazer.Left <= reach)
			{
				Watch(gazer);
			}
		}
	}

	// A809: a gaze RSR acted on waits for its judgement, whatever its origin and whether it is trusted or only holds -
	// once while it is pending, so a status that comes again in the fight is watched again.
	private static void Watch(Gazer gazer)
	{
		foreach (var pending in _pending)
		{
			if (pending.Key == gazer.Key)
			{
				return;
			}
		}

		var cast = gazer.From == Origin.Cast;
		_pending.Add(new Pending(gazer.Key, LearnKey(gazer), cast ? ActionName(gazer.Id) : null, gazer.Who?.GameObjectId ?? 0,
			cast ? gazer.Id : 0u, DateTime.Now.AddSeconds(gazer.Left), Describe(gazer)));
	}

	/// <summary>
	/// An enemy's effect packet (A783, A786). It measures how long after its bar a cast resolves, ends a gaze that waited
	/// for it, writes who it hit, and judges every gaze RSR acted on (Related): harm on the player is a failure, learned by
	/// the one rule in Learn - hold only, or leave the gaze, until RSR is loaded again; the gaze's own packet without harm
	/// is a gaze avoided.
	/// </summary>
	internal static void ObserveLanding(ActionEffectSet set)
	{
		var player = Player.Object;
		if (player == null || set.Source is not IBattleChara source || !source.IsEnemySideFigure() || set.Action is not { } action
			|| action.ActionCategory.RowId == (uint)ActionCate.Autoattack)
		{
			return;
		}

		var sourceId = source.GameObjectId;
		var actionId = action.RowId;
		NoteLandingDelay(sourceId, actionId);
		var landed = Landed(set, sourceId, actionId);
		if (_pending.Count == 0)
		{
			return;
		}

		var name = action.Name.ExtractText();
		if (!Harmed(set, player.GameObjectId))
		{
			for (var i = _pending.Count - 1; i >= 0; i--)
			{
				if (_pending[i].Action != 0 && _pending[i].Source == sourceId && _pending[i].Action == actionId)
				{
					Survived(_pending[i], string.Empty);
					_pending.RemoveAt(i);
				}
			}

			return;
		}

		// A809 (sixth review, M2): whether the packet refutes RSR's eye and sense is judged once, against the gaze whose
		// packet it is - not against each pending gaze of the same name, whose eye may stand elsewhere.
		var now = DateTime.Now;
		var landedLearn = landed is { } own ? LearnKey(own) : 0UL;
		var landedRefuted = landed is { } judged && Refuted(set, judged, player.GameObjectId);
		var othersRefuted = OthersRefuted(set, player.GameObjectId, sourceId, actionId, name, now, landed != null, landedLearn);
		for (var i = _pending.Count - 1; i >= 0; i--)
		{
			var pending = _pending[i];
			if (Related(pending, sourceId, actionId, name, now))
			{
				Learn(pending, name, landed != null && pending.Learn == landedLearn ? landedRefuted : othersRefuted);
				_pending.RemoveAt(i);
			}
		}
	}

	// A809: a packet belongs to a pending gaze when it is the gaze's own cast, a cast of the same name (a helper resolving
	// it), or - for a marker, status or member gaze - any enemy packet from its deadline on, a frame early (sixth review,
	// M4: before it an unrelated "Vulnerability Up" read as the gaze failing).
	private static bool Related(Pending pending, ulong sourceId, uint actionId, string name, DateTime now)
	{
		return pending.Action != 0
			? (pending.Source == sourceId && pending.Action == actionId)
				|| (!string.IsNullOrEmpty(name) && string.Equals(pending.Name, name, StringComparison.Ordinal))
			: now.AddSeconds(AnimationLockTweak.MeanFrameTime) >= pending.Due;
	}

	// A809: for the related gazes the packet is not the own packet of, it refutes only where it refutes every one of them
	// that is still seen - the marker's eyes may stand apart.
	private static bool OthersRefuted(ActionEffectSet set, ulong playerId, ulong sourceId, uint actionId, string name, DateTime now,
		bool hasLanded, ulong landedLearn)
	{
		var any = false;
		foreach (var pending in _pending)
		{
			if ((hasLanded && pending.Learn == landedLearn) || !Related(pending, sourceId, actionId, name, now) || Find(pending.Key) is not { } gazer)
			{
				continue;
			}

			if (!Refuted(set, gazer, playerId))
			{
				return false;
			}

			any = true;
		}

		return any;
	}

	/// <summary>
	/// The one rule of learning (A809, concept 13, "Lernen und gestufter Rückfall"), per gaze for the build: nothing learned
	/// yet - RSR only holds for it, or leaves it where the packet refutes RSR's eye and sense; only holding - a second hit
	/// after the first's mechanic leaves it; left - stays left. What is learned is read here, never from the pending.
	/// </summary>
	private static void Learn(Pending pending, string name, bool refuted)
	{
		if (!_distrusted.TryGetValue(pending.Learn, out var holdOnly))
		{
			_distrusted[pending.Learn] = !refuted;
			_failedAt[pending.Learn] = DateTime.Now;
			DefenseTrace.Line($"gaze: hit despite RSR's facing by {pending.Text} ({name}) - "
				+ (refuted ? "others it hit faced away from the eye as RSR places it, so RSR leaves this gaze to you until it is loaded again"
					: "the packet does not refute RSR's eye and sense, so RSR only holds actions for it until it is loaded again"));
		}
		else if (holdOnly && SecondHit(pending.Learn))
		{
			_distrusted[pending.Learn] = false;
			DefenseTrace.Line($"gaze: hit again by {pending.Text} while RSR only held actions - RSR leaves this gaze to you until it is loaded again");
		}
	}

	// A gaze RSR acted on that passed without harm: avoided where RSR turned for it, held where it only held actions.
	private static void Survived(Pending pending, string why)
	{
		var held = _distrusted.TryGetValue(pending.Learn, out var holdOnly) && holdOnly;
		DefenseTrace.Line($"gaze: {(held ? "held" : "avoided")} {pending.Text}{why}");
	}

	// A harmful status on <targetId> in the packet - a gaze works through its status (Doom, Stone, Paralysis). A801: damage
	// alone does not count; Mortal Ray (#48143) dealt 1 % to him facing away, and counting it would have read every gaze
	// that also damages as failed and, with the members facing away hit too, as refuted.
	private static bool Harmed(ActionEffectSet set, ulong targetId)
	{
		var harmed = false;
		foreach (var effect in set.TargetEffects)
		{
			if (effect.TargetID != targetId)
			{
				continue;
			}

			effect.ForEach(entry =>
			{
				harmed |= entry.type == ActionEffectType.ApplyStatusEffectTarget && entry.value != 0
					&& Service.GetSheet<Lumina.Excel.Sheets.Status>().GetRowOrDefault(entry.value) is { StatusCategory: (byte)StatusCategory.Detrimental };
			});
		}

		return harmed;
	}

	// A786: the seconds from a cast's bar end to its packet, the longest in this zone.
	private static void NoteLandingDelay(ulong sourceId, uint actionId)
	{
		if (_barEnds.Remove(sourceId, out var cast) && cast.Action == actionId)
		{
			var delay = (float)(DateTime.Now - cast.BarEnd).TotalSeconds;
			if (delay >= 0f && delay > _landingMost)
			{
				_landingMost = delay;
			}
		}
	}

	// A786: a cast gaze's own packet ends its wait, and RSR writes who it hit and whether they faced the eye as RSR places
	// it - a hit on someone who faced away says the sense or the eye is wrong, before it costs the player.
	private static Gazer? Landed(ActionEffectSet set, ulong sourceId, uint actionId)
	{
		Gazer? found = null;
		for (var i = _landing.Count - 1; i >= 0; i--)
		{
			var gazer = _landing[i].Gazer;
			if (gazer.Who?.GameObjectId == sourceId && gazer.Id == actionId)
			{
				found = gazer;
				_landing.RemoveAt(i);
			}
		}

		if (found is not { } landed)
		{
			return null;
		}

		_ = _heldFacing.Remove(landed.Key);

		int hitFacing = 0, hitAway = 0, sparedFacing = 0, sparedAway = 0;
		foreach (var effect in set.TargetEffects)
		{
			if (Svc.Objects.SearchById(effect.TargetID) is not IBattleChara member || member.IsEnemySideFigure())
			{
				continue;
			}

			var facing = Unsafe(member.Rotation, member, landed, false);
			var hit = Harmed(set, effect.TargetID);
			if (hit)
			{
				hitFacing += facing ? 1 : 0;
				hitAway += facing ? 0 : 1;
			}
			else
			{
				sparedFacing += facing ? 1 : 0;
				sparedAway += facing ? 0 : 1;
			}
		}

		DefenseTrace.Line($"gaze: landed {Describe(landed)}: hit {hitFacing + hitAway} ({hitAway} looking the safe way by RSR's eye), "
			+ $"spared {sparedFacing + sparedAway} ({sparedFacing} looking the wrong way); RSR's last oriented action "
			+ (_lastOrientedAction == DateTime.MinValue ? "none" : $"{(DateTime.Now - _lastOrientedAction).TotalSeconds:F2} s before"));
		return landed;
	}

	// A791: whether the packet refutes RSR's eye and sense: among the others it hit, more faced the safe way as RSR places
	// it than into it. Without others hit nothing refutes it.
	private static bool Refuted(ActionEffectSet set, Gazer gazer, ulong playerId)
	{
		int facing = 0, away = 0;
		foreach (var effect in set.TargetEffects)
		{
			if (effect.TargetID == playerId || Svc.Objects.SearchById(effect.TargetID) is not IBattleChara member || member.IsEnemySideFigure()
				|| !Harmed(set, effect.TargetID))
			{
				continue;
			}

			if (Unsafe(member.Rotation, member, gazer, false))
			{
				facing++;
			}
			else
			{
				away++;
			}
		}

		return away > facing;
	}

	// The gaze with this key among the current, the ended and last frame's.
	private static Gazer? Find(ulong key)
	{
		foreach (var gazer in _gazers)
		{
			if (gazer.Key == key)
			{
				return gazer;
			}
		}

		foreach (var landing in _landing)
		{
			if (landing.Gazer.Key == key)
			{
				return landing.Gazer;
			}
		}

		return null;
	}

	// Marker, status and member gazes RSR acted on: while the gaze is seen its deadline moves on with it; once it is gone,
	// without harm from any enemy within the longest delay measured in this zone plus the lead, it passed (A809, sixth
	// review, M5: a marker's eye may resolve after the marker's time). A cast gaze waits for its own packet until the fight
	// ends (A801) - early in a zone the longest delay measured was shorter than this one's.
	private static void CloseAvoided(float lead)
	{
		// A809 (seventh review, M3): without a delay measured in this zone yet, the lead alone, else a frame - left open, any
		// later harmful packet in the fight read as the gaze failing.
		var now = DateTime.Now;
		var window = _landingMost + (float.IsInfinity(lead) ? AnimationLockTweak.MeanFrameTime : lead);
		for (var i = _pending.Count - 1; i >= 0; i--)
		{
			var pending = _pending[i];
			if (pending.Action != 0)
			{
				continue;
			}

			if (Find(pending.Key) is { } gazer)
			{
				var due = now.AddSeconds(gazer.Left);
				if (due > pending.Due)
				{
					_pending[i] = pending with { Due = due };
				}
			}
			else if ((now - pending.Due).TotalSeconds > window)
			{
				Survived(pending, ", no harm seen from any enemy");
				_pending.RemoveAt(i);
			}
		}

		// A809: a held facing ends with its gaze - a marker, status or member gaze has no packet of its own (Landed), and a
		// status that came again held every facing action, a safe one too.
		if (_heldFacing.Count > 0)
		{
			_ = _heldFacing.RemoveWhere(static key => Find(key) == null);
		}
	}

	// What a failure is learned under: the gaze itself - a cast's row, a marker's kind, a status's id. A809 (seventh
	// review, M1): not the cast's name - Angra Mainyu, the Crucible's Ahriman and Destrudo share "Mortal Gaze", and a
	// failure on one released the others. A helper's twin is judged by the same packet and learns under its own row.
	private static ulong LearnKey(Gazer gazer) => (ulong)(uint)HashCode.Combine(gazer.From, gazer.Id);

	private static string ActionName(uint actionId)
	{
		if (!_actionNames.TryGetValue(actionId, out var name))
		{
			name = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(actionId)?.Name.ExtractText() ?? string.Empty;
			_actionNames[actionId] = name;
		}

		return name;
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

	// Whether facing <rotation> looks into <gazer>: at its eye for a gaze, away from it for an inverted one. With
	// <margin>, the half width grows by the gaze's margin for movement (the player's own facing; not for other members).
	private static bool Unsafe(float rotation, IBattleChara player, Gazer gazer, bool margin = true)
	{
		var toEye = gazer.Eye - player.Position;
		toEye.Y = 0;
		if (toEye.LengthSquared() <= 0f)
		{
			return !gazer.Inverted;
		}

		var room = margin ? gazer.Margin + float.DegreesToRadians(MeasuredEdgeDegrees) : 0f;
		var half = float.DegreesToRadians(DangerHalfAngleDegrees) + room;
		var off = MathF.Abs(Difference(rotation, MathF.Atan2(toEye.X, toEye.Z)));
		// An inverted gaze is safe only well within the cone: the margin narrows it.
		return gazer.Inverted ? off > float.DegreesToRadians(DangerHalfAngleDegrees) - room : off <= half;
	}

	// The middle of the widest safe gap among the gazes within <lead>, as BossModReborn: it keeps the most room to every
	// eye (A786; nearest until then, which saves turning but nothing else without a target); null when none is safe.
	private static float? SafeRotation(IBattleChara player, float lead)
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
			var room = gazer.Margin + float.DegreesToRadians(MeasuredEdgeDegrees);
			if (gazer.Inverted)
			{
				AddArc(at + half - room, at + MathF.Tau - half + room);
			}
			else
			{
				AddArc(at - half - room, at + half + room);
			}
		}

		_forbidden.Sort(static (a, b) => a.Start.CompareTo(b.Start));
		float? best = null;
		var bestWidth = 0f;
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
			if (next - end > bestWidth && SafeFor(middle, player, lead))
			{
				best = middle;
				bestWidth = next - end;
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
			var room = gazer.Margin + float.DegreesToRadians(MeasuredEdgeDegrees);
			if (gazer.Inverted)
			{
				AddArc(at + gazeHalf - room, at + MathF.Tau - gazeHalf + room);
			}
			else
			{
				AddArc(at - gazeHalf - room, at + gazeHalf + room);
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
			|| Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(info->ActionId) is not { NeedToFaceTarget: true, Range: not 0 } action)
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
	/// <summary>
	/// What a fight registered ends with it (A809, seventh review, M2): called where RSR measures the fight's end every
	/// frame (ActionUpdater), whether RSR is on or not - after a wipe RSR switches itself off, and an open registration was
	/// read as failed by the next pull's first harmful status. What was learned stays until RSR is loaded again.
	/// </summary>
	internal static void EndFight()
	{
		foreach (var pending in _pending)
		{
			DefenseTrace.Line($"gaze: unknown {pending.Text}, the fight ended before its packet");
		}

		_pending.Clear();
		_landing.Clear();
		_casts.Clear();
		_castsBefore.Clear();
		_traced.Clear();
		_turnTraced.Clear();
		_refusedTraced.Clear();
		_heldFacing.Clear();
		_markers.Clear();
		_eyesBefore.Clear();
		_movedTick = long.MinValue;
	}

	private static List<Gazer> Gazers()
	{
		var now = Environment.TickCount64;
		if (_gazersTick != long.MinValue && now - _gazersTick < DataCenter.FrameCacheMs)
		{
			return _gazers;
		}

		_gazersTick = now;
		_gazers.Clear();

		// A789: distrust lasts until RSR is loaded again - a new build proves itself again, the same build does not die
		// again in the next run of the same duty. A zone change starts the measured delays anew.
		var territory = DataCenter.Territory?.Id ?? 0;
		if (territory != _zoneTerritory)
		{
			_zoneTerritory = territory;
			_pending.Clear();
			_barEnds.Clear();
			_landingMost = 0f;
			if (_texts == null)
			{
				_ = Interlocked.Exchange(ref _textsRequested, 0);
			}
		}

		var player = Player.Object;
		if (!DataCenter.InCombat || player == null)
		{
			EndFight();
			return _gazers;
		}

		var lead = Lead();
		_castsBefore.Clear();
		_castsBefore.AddRange(_casts);
		_casts.Clear();
		AddCasts();
		AddLandings(lead);
		var texts = Texts_();
		if (texts != null)
		{
			AddPlayerStatuses(player, texts);
			AddMemberEyes(player, texts);
		}

		AddMarkers(lead);
		CloseAvoided(lead);

		// A789: a gaze that hit despite RSR's facing keeps holding actions over the whole cast where its packet bore out the
		// eye and sense, else RSR leaves it alone.
		if (_distrusted.Count > 0)
		{
			for (var i = _gazers.Count - 1; i >= 0; i--)
			{
				// A809: learned per gaze for the build, not per zone - the Pilgrim's Traverse changes zone every ten floors.
				if (_distrusted.TryGetValue(LearnKey(_gazers[i]), out var holds))
				{
					if (holds)
					{
						_gazers[i] = _gazers[i] with { Trusted = false };
					}
					else
					{
						_gazers.RemoveAt(i);
					}
				}
			}
		}

		AddMargins(player, lead);
		return _gazers;
	}

	// A789: the angle by which the player's and each eye's movement over the last frame, carried over the lead and the
	// zone's longest delay to the packet, can shift a gaze's edge; 0 while both stand.
	private static void AddMargins(IBattleChara player, float lead)
	{
		var now = Environment.TickCount64;
		// A793: the start value is checked before it is subtracted. now - long.MinValue overflowed, TimeSpan threw, and from
		// the game's action hook that closed the game at the first action of every fight (14.11.2026, 13:58).
		var known = _movedTick != long.MinValue && now > _movedTick;
		var seconds = known ? (float)TimeSpan.FromMilliseconds(now - _movedTick).TotalSeconds : 0f;
		_updateGap = known ? seconds : 0f;
		var ahead = (float.IsInfinity(lead) ? 0f : lead) + _landingMost;
		var playerSpeed = known ? Vector3.Distance(player.Position, _playerBefore) / seconds : 0f;
		for (var i = 0; i < _gazers.Count; i++)
		{
			var gazer = _gazers[i];
			var eyeSpeed = known && _eyesBefore.TryGetValue(gazer.Key, out var before) ? Vector3.Distance(gazer.Eye, before) / seconds : 0f;
			var distance = Vector3.Distance(gazer.Eye, player.Position);
			var drift = (playerSpeed + eyeSpeed) * ahead;
			if (drift > 0f && distance > 0f)
			{
				_gazers[i] = gazer with { Margin = MathF.Atan2(drift, distance) };
			}
		}

		_eyesBefore.Clear();
		foreach (var gazer in _gazers)
		{
			_eyesBefore[gazer.Key] = gazer.Eye;
		}

		_playerBefore = player.Position;
		_movedTick = now;
	}

	private static void AddCasts()
	{
		var at = DateTime.Now;
		foreach (var caster in DataCenter.CastingEnemies)
		{
			if (caster == null || !caster.IsCasting)
			{
				continue;
			}

			var left = Math.Max(0f, caster.TotalCastTime - caster.CurrentCastTime);
			_barEnds[caster.GameObjectId] = (caster.CastActionId, at.AddSeconds(left));
			var kind = KindOf(caster.CastActionId);
			if (kind != Kind.None && DataCenter.CastReachesPlayer(caster))
			{
				// A786: the client keeps a cast's place only for a ground-targeted action; any other may show an old one.
				var eye = AreaTargeted(caster.CastActionId) ? DataCenter.CastLocation(caster) ?? caster.Position : caster.Position;
				var gazer = new Gazer(eye, (ulong)(uint)HashCode.Combine(caster.GameObjectId, caster.CastActionId), kind == Kind.Inverted, left, Origin.Cast, caster.CastActionId, caster);
				_gazers.Add(gazer);
				_casts.Add(gazer);
			}
		}
	}

	// A786: a cast gaze whose bar ended stays due, at no seconds left, until its packet (Landed), its caster is gone, or
	// the longest delay measured in this zone plus the lead has passed. A bar that stopped with more than the lead left
	// was cut off, not finished.
	private static void AddLandings(float lead)
	{
		var now = DateTime.Now;
		// A791: plus the time since the last update - with a longer update interval the last bar seen had more left.
		var finished = (float.IsInfinity(lead) ? AnimationLockTweak.MeanFrameTime : lead) + _updateGap;
		foreach (var before in _castsBefore)
		{
			var running = false;
			foreach (var cast in _casts)
			{
				running |= cast.Key == before.Key;
			}

			var known = false;
			foreach (var landing in _landing)
			{
				known |= landing.Gazer.Key == before.Key;
			}

			if (!running && !known && before.Left <= finished)
			{
				_landing.Add(new Landing(before with { Left = 0f }, now));
			}
		}

		for (var i = _landing.Count - 1; i >= 0; i--)
		{
			var landing = _landing[i];
			var expired = _landingMost > 0f && (now - landing.Ended).TotalSeconds > _landingMost + (float.IsInfinity(lead) ? 0f : lead);
			if (expired || landing.Gazer.Who is not IBattleChara who || !who.IsValid() || who.IsDead
				|| (who.IsCasting && who.CastActionId != landing.Gazer.Id))
			{
				_landing.RemoveAt(i);
				continue;
			}

			_gazers.Add(landing.Gazer);
		}
	}

	private static bool AreaTargeted(uint actionId)
	{
		if (!_areaTargeted.TryGetValue(actionId, out var area))
		{
			area = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(actionId)?.TargetArea ?? false;
			_areaTargeted[actionId] = area;
		}

		return area;
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
					_gazers.Add(new Gazer(member.Position, (ulong)(uint)HashCode.Combine(member.GameObjectId, status.StatusId), false, status.RemainingTime,
						Origin.MemberStatus, status.StatusId, member));
				}
			}
		}
	}

	// "Look" and "no look" markers on the player (A776). The eyes are the enemies whose cast ends with the marker - within
	// the lead of each other - and among them the largest (A786: until then the largest casting, whatever it cast; with
	// "Look" a wrong eye turns the player away from the real one). Without a measured lead every cast counts, and RSR
	// only holds then.
	/// <summary>
	/// A visual effect created on a figure (Service.ActorVfxCreateDetour): a "look" or "no look" marker on the player is
	/// remembered here, so no frame walks the effect queue (A789).
	/// </summary>
	internal static void NoteVfx(VfxNewData vfx)
	{
		if (Player.Object is not { } player || vfx.ObjectId != player.GameObjectId || string.IsNullOrEmpty(vfx.Path)
			|| LookMarker().Match(vfx.Path) is not { Success: true } match
			|| !float.TryParse(match.Groups["seconds"].Value, System.Globalization.CultureInfo.InvariantCulture, out var seconds))
		{
			return;
		}

		_markers.Add(new MarkerWatch { Seen = DateTime.Now, Inverted = !match.Groups["no"].Success, Duration = seconds });
	}

	private static void AddMarkers(float lead)
	{
		var now = DateTime.Now;
		var finite = float.IsInfinity(lead) ? 0f : lead;

		// A789: a marker stays due past its duration by the zone's longest delay to the packet, as its cast's bar ends
		// 0.3 s before the server resolves it.
		for (var i = _markers.Count - 1; i >= 0; i--)
		{
			if ((now - _markers[i].Seen).TotalSeconds > _markers[i].Duration + _landingMost + finite)
			{
				_markers.RemoveAt(i);
			}
		}

		// The eye's cast ends with the marker: its bar ends up to the zone's delay to the packet, plus a lead or a frame,
		// before it. Without that delay measured every cast counts (A791: independent of the lead).
		var tolerance = _landingMost > 0f ? _landingMost + (float.IsInfinity(lead) ? AnimationLockTweak.MeanFrameTime : lead) : float.PositiveInfinity;
		foreach (var marker in _markers)
		{
			var left = Math.Max(0f, marker.Duration - (float)(now - marker.Seen).TotalSeconds);

			// A791: eyes once matched stay. In the last stretch the real eye no longer casts, and any enemy whose cast just
			// ends would match - a helper with an area. They are matched anew only while none of them casts and the marker
			// is not in that stretch yet.
			var anyCasting = false;
			foreach (var eye in marker.Eyes)
			{
				anyCasting |= eye.IsValid() && eye.IsCasting;
			}

			if (marker.Eyes.Count == 0 || (!anyCasting && left > tolerance))
			{
				_largest.Clear();
				var radius = 0f;
				foreach (var caster in DataCenter.CastingEnemies)
				{
					if (caster == null || !caster.IsCasting || MathF.Abs(caster.TotalCastTime - caster.CurrentCastTime - left) > tolerance)
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

				if (_largest.Count > 0)
				{
					marker.Eyes.Clear();
					marker.Eyes.AddRange(_largest);
					DefenseTrace.Line($"gaze: marker to look {(marker.Inverted ? "at" : "away from")} {left:F1} s before it resolves - eyes: {EyeNames(marker.Eyes)}");
				}
			}

			// A809 (T8): a marker that comes to resolve without an eye - then the game turns as without the rule.
			if (marker.Eyes.Count == 0 && !marker.NoEyeTraced && left <= finite)
			{
				marker.NoEyeTraced = true;
				DefenseTrace.Line($"gaze: marker to look {(marker.Inverted ? "at" : "away from")} {left:F1} s before it resolves - no eye found, no cast ends with it");
			}

			foreach (var eye in marker.Eyes)
			{
				if (eye.IsValid() && !eye.IsDead)
				{
					_gazers.Add(new Gazer(eye.Position, eye.GameObjectId ^ (ulong)marker.Seen.Ticks, marker.Inverted, left,
						Origin.Marker, marker.Inverted ? 1u : 0u, eye));
				}
			}
		}
	}

	private static string EyeNames(List<IBattleChara> eyes)
	{
		var text = new System.Text.StringBuilder();
		foreach (var eye in eyes)
		{
			text.Append(text.Length == 0 ? string.Empty : ", ").Append(eye.Name.TextValue).Append(" (").Append(eye.CastActionId).Append(')');
		}

		return text.ToString();
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

		// Names compared on the English sheet, where the 77 namesakes were counted; the client's language parts them
		// otherwise (A786). The gaze mark counts unless an inverted gaze carries the name: it does not tell the sense
		// (#27363 and #27369, "Avert Your Eyes", both marked, the second inverted).
		var texts = Texts_();
		var row = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>(ClientLanguage.English)?.GetRowOrDefault(actionId);
		var kind = Kind.None;
		if (row is { } action && !action.IsPlayerAction)
		{
			var name = action.Name.ExtractText();
			kind = (action.VFX.RowId == GazeCastVfx && (texts == null || !texts.InvertedAll.Contains(name)))
				|| (texts != null && texts.GazeNames.Contains(name)) ? Kind.Gaze
				: texts != null && texts.InvertedNames.Contains(name) ? Kind.Inverted
				: Kind.None;
		}

		if (texts != null)
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
					// Read again after the next zone change (Gazers), not left out for the session (A786).
					PluginLog.Warning($"Could not read the gaze texts: {ex.Message}");
				}
			});
		}

		return _texts;
	}

	private static Texts ReadTexts()
	{
		var actions = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>(ClientLanguage.English)!;
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

		var invertedAll = new HashSet<string>(invertedNames, StringComparer.Ordinal);

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

		return new Texts { GazeNames = gazeNames, InvertedNames = invertedNames, InvertedAll = invertedAll, PlayerStatuses = playerStatuses, MemberEyes = memberEyes };
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

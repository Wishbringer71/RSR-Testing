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
	// false for a gaze that hit despite RSR's facing in this zone (A786): it still holds actions over the whole cast,
	// but RSR no longer turns or chooses a facing for it.
	private readonly record struct Gazer(Vector3 Eye, ulong Key, bool Inverted, float Left, Origin From, uint Id, IGameObject? Who,
		bool Trusted = true);

	// A783, A786: a gaze RSR turned for, until its own effect packet judges it. Source and Action name the cast whose
	// packet resolves it (the eye's caster and its cast; for a marker the cast that ends with it); Action 0 for a status
	// or member gaze, which a packet of the eye judges until the deadline.
	private readonly record struct Pending(ulong Key, ulong Learn, string? Name, bool Cast, ulong Source, uint Action, DateTime Due,
		string Text);

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
	private static readonly List<(float Start, float End)> _forbidden = [];
	private static readonly List<(DateTime Seen, bool Inverted, float Duration)> _markers = [];
	private static readonly List<IBattleChara> _largest = [];
	private static long _gazersTick = long.MinValue;
	private static readonly List<Pending> _pending = [];
	private static readonly HashSet<ulong> _distrusted = [];
	private static readonly List<Gazer> _casts = [];
	private static readonly List<Gazer> _castsBefore = [];
	private static readonly List<Landing> _landing = [];
	private static readonly Dictionary<ulong, (uint Action, DateTime BarEnd)> _barEnds = [];
	private static readonly Dictionary<uint, bool> _areaTargeted = [];
	private static float _landingMost;
	private static (float Rotation, Vector3 Position, ulong Key)? _turnCheck;
	private static readonly HashSet<ulong> _undoneTraced = [];
	private static readonly Dictionary<uint, string> _actionNames = [];
	private static uint _distrustTerritory;
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
	/// Whether an action of <paramref name="castTime"/> seconds that faces <paramref name="towards"/> and goes off in
	/// <paramref name="startsIn"/> seconds would still face into a gaze within the lead before it lands: looking at the eye
	/// of a gaze, or away from the eye of an inverted one.
	/// </summary>
	internal static bool TurnsIntoGaze(Vector3 towards, float castTime, float startsIn)
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

		// A778: with turning on, RSR presses the action facing a safe way within the action's cone around the target
		// (PrepareFacing), and a cast keeps a safe way within its wider limit until the game has it (TurnAway). It waits
		// only where neither exists - above all on the eye's own caster. A786: counted from when the game runs the action,
		// which may be a GCD or a lock later than the press.
		var held = turning && (SafeNear(player, rotation, float.DegreesToRadians(ActionHalfAngleDegrees), Math.Max(0f, startsIn) + lead) == null
			|| (castTime > 0f && SafeNear(player, rotation, float.DegreesToRadians(CastHalfAngleDegrees), span + lead) == null));

		foreach (var gazer in gazers)
		{
			// Without turning, and for a gaze that hit despite RSR's facing (A786): the game turns the player straight at the
			// target, and a cast holds that heading to its end, so the whole cast counts. With turning, the hold names the
			// first gaze that lands while the action needs its facing.
			if (!turning || !gazer.Trusted ? !Unsafe(rotation, player, gazer) : !held || gazer.Left - span > lead)
			{
				continue;
			}

			if (_traced.Add(gazer.Key))
			{
				DefenseTrace.Line($"gaze: holding actions that turn {(gazer.Inverted ? "away from" : "towards")} {Describe(gazer)}, "
					+ $"{gazer.Left:F1} s left, lead {(turning && gazer.Trusted ? $"{lead:F2} s" : "the whole cast")}");
			}

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

		CheckTurnHeld(player);
		var lead = Lead();
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
	private static void CheckTurnHeld(IBattleChara player)
	{
		if (_turnCheck is not { } check)
		{
			return;
		}

		_turnCheck = null;
		if (player.Position == check.Position && player.Rotation != check.Rotation && _undoneTraced.Add(check.Key))
		{
			DefenseTrace.Line($"gaze: turn undone by the game the next frame, {float.RadiansToDegrees(Difference(player.Rotation, check.Rotation)):F0}° "
				+ "- RSR turns again each frame while the gaze is due");
		}
	}

	/// <summary>
	/// Where the game runs an action (ActionManagerEx.UseActionLocation, A786): RSR's own press, the game's queue and the
	/// player's own press alike. For an action that faces its target, <see cref="PrepareFacing"/>; null otherwise.
	/// </summary>
	internal static unsafe uint? PrepareFacingFor(uint actionId, ulong targetId, Vector3* location)
	{
		var player = Player.Object;
		if (player == null || !Service.Config.TurnAwayFromGazes || !DataCenter.InCombat
			|| Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(actionId) is not { NeedToFaceTarget: true, Range: > 0 } action)
		{
			return null;
		}

		if (action.TargetArea)
		{
			return location == null ? null : PrepareFacing(*location);
		}

		return targetId != player.GameObjectId && Svc.Objects.SearchById(targetId) is { } target ? PrepareFacing(target.Position) : null;
	}

	/// <summary>
	/// Right before an oriented action goes off while a gaze is within the lead (A778, as BossModReborn's smart
	/// orientation): the facing stays when the target lies within the action's cone and it looks into no gaze, else
	/// RSR turns to the middle of the widest safe stretch of that cone, and where there is none it does not turn - the
	/// game refuses a target outside the cone. Either way "Auto face target" is off for the call, so the game does not
	/// turn the player straight at the target. Returns the option's value to put back.
	/// </summary>
	internal static unsafe uint? PrepareFacing(Vector3 towards)
	{
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
		var due = false;
		foreach (var gazer in gazers)
		{
			due |= gazer.Trusted && gazer.Left <= lead;
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
		var turnTo = MathF.Abs(Difference(current, aim)) > half || !SafeFor(current, player, lead) ? SafeNear(player, aim, half, lead) : null;

		var framework = Framework.Instance();
		var option = framework == null ? null : framework->SystemConfig.GetConfigOption((uint)ConfigOption.AutoFaceTargetOnAction);
		if (option == null)
		{
			return null;
		}

		var original = option->Value.UInt;
		if (turnTo is { } target && Face(target))
		{
			foreach (var gazer in gazers)
			{
				if (gazer.Trusted && gazer.Left <= lead && _turnTraced.Add(gazer.Key))
				{
					DefenseTrace.Line($"gaze: turned {float.RadiansToDegrees(Difference(target, current)):F0}° for an action, "
						+ $"{float.RadiansToDegrees(Difference(target, aim)):F0}° off its target, {(gazer.Inverted ? "towards" : "away from")} "
						+ $"{Describe(gazer)}, {gazer.Left:F2} s before it lands");
				}
			}
		}

		option->Value.UInt = 0;
		Watch(gazers, lead);
		return original;
	}

	// A783 (concept 13, "Im Spiel auswerten und lernen"): every gaze RSR turned for or chose the facing against waits for
	// its own effect packet; ObserveLanding judges it.
	private static void Watch(List<Gazer> gazers, float lead)
	{
		var now = DateTime.Now;
		foreach (var gazer in gazers)
		{
			if (!gazer.Trusted || gazer.Left > lead)
			{
				continue;
			}

			var known = false;
			foreach (var pending in _pending)
			{
				known |= pending.Key == gazer.Key;
			}

			if (!known)
			{
				var action = gazer.From == Origin.Cast ? gazer.Id
					: gazer.From == Origin.Marker && gazer.Who is IBattleChara eye ? eye.CastActionId
					: 0u;
				_pending.Add(new Pending(gazer.Key, LearnKey(gazer), gazer.From == Origin.Cast ? ActionName(gazer.Id) : null, gazer.From == Origin.Cast,
					gazer.Who?.GameObjectId ?? 0, action, now.AddSeconds(gazer.Left), Describe(gazer)));
			}
		}
	}

	/// <summary>
	/// An enemy's effect packet (A783, A786). It measures how long after its bar a cast resolves, ends a gaze that waited
	/// for it, writes who it hit, and judges every gaze RSR acted on: harm on the player from the gaze's own cast, from a
	/// cast of the same name, or for a status or member gaze from the eye, is a failure - RSR then only holds for that gaze
	/// in this zone, as A770; the gaze's own packet without harm is a gaze avoided.
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
		Landed(set, sourceId, actionId);
		if (_pending.Count == 0)
		{
			return;
		}

		var harmed = Harmed(set, player.GameObjectId);
		var name = action.Name.ExtractText();
		for (var i = _pending.Count - 1; i >= 0; i--)
		{
			var pending = _pending[i];
			var resolves = pending.Action != 0 && pending.Source == sourceId && pending.Action == actionId;
			var related = resolves || (pending.Cast && string.Equals(pending.Name, name, StringComparison.Ordinal))
				|| (pending.Action == 0 && pending.Source == sourceId);
			if (harmed && related)
			{
				_ = _distrusted.Add(pending.Learn);
				DefenseTrace.Line($"gaze: hit despite RSR's facing by {pending.Text} ({name}) - RSR only holds actions for this gaze in this zone");
				_pending.RemoveAt(i);
			}
			else if (resolves)
			{
				DefenseTrace.Line($"gaze: avoided {pending.Text}");
				_pending.RemoveAt(i);
			}
		}
	}

	// Damage or a harmful status on <targetId> in the packet.
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
				harmed |= ActionEffectReading.DamageOnTarget(entry) > 0
					|| (entry.type == ActionEffectType.ApplyStatusEffectTarget && entry.value != 0
						&& Service.GetSheet<Lumina.Excel.Sheets.Status>().GetRowOrDefault(entry.value) is { StatusCategory: (byte)StatusCategory.Detrimental });
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
	private static void Landed(ActionEffectSet set, ulong sourceId, uint actionId)
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
			return;
		}

		int hitFacing = 0, hitAway = 0, sparedFacing = 0, sparedAway = 0;
		foreach (var effect in set.TargetEffects)
		{
			if (Svc.Objects.SearchById(effect.TargetID) is not IBattleChara member || member.IsEnemySideFigure())
			{
				continue;
			}

			var facing = Unsafe(member.Rotation, member, landed);
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
			+ $"spared {sparedFacing + sparedAway} ({sparedFacing} looking the wrong way)");
	}

	// Gazes RSR acted on whose packet did not come within the longest delay measured in this zone: a status or member
	// gaze without harm from its eye is avoided; a cast gaze is unknown, its packet unseen.
	private static void CloseAvoided(float lead)
	{
		if (_landingMost <= 0f)
		{
			return;
		}

		var deadline = DateTime.Now.AddSeconds(-(_landingMost + (float.IsInfinity(lead) ? 0f : lead)));
		for (var i = _pending.Count - 1; i >= 0; i--)
		{
			if (_pending[i].Due < deadline)
			{
				DefenseTrace.Line(_pending[i].Action == 0 ? $"gaze: avoided {_pending[i].Text}" : $"gaze: unknown {_pending[i].Text}, its packet unseen");
				_pending.RemoveAt(i);
			}
		}
	}

	// What a failure is learned under: the cast's name (so a helper's twin counts too), else the source and its id.
	private static ulong LearnKey(Gazer gazer) => gazer.From == Origin.Cast
		? (ulong)(uint)StringComparer.Ordinal.GetHashCode(ActionName(gazer.Id))
		: (ulong)(uint)HashCode.Combine(gazer.From, gazer.Id);

	private static bool Distrusted(Gazer gazer) => _distrusted.Count > 0 && _distrusted.Contains(LearnKey(gazer));

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

		// A783: a zone change starts trust anew, so a fixed build proves itself again; the measured delays start anew too.
		var territory = DataCenter.Territory?.Id ?? 0;
		if (territory != _distrustTerritory)
		{
			_distrustTerritory = territory;
			_distrusted.Clear();
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
			_undoneTraced.Clear();
			_markers.Clear();
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

		AddMarkers(player, lead);
		CloseAvoided(lead);

		// A786: a gaze that hit despite RSR's facing keeps holding actions over the whole cast; only the turning ends.
		if (_distrusted.Count > 0)
		{
			for (var i = 0; i < _gazers.Count; i++)
			{
				if (Distrusted(_gazers[i]))
				{
					_gazers[i] = _gazers[i] with { Trusted = false };
				}
			}
		}

		return _gazers;
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
				var gazer = new Gazer(eye, caster.GameObjectId ^ caster.CastActionId, kind == Kind.Inverted, left, Origin.Cast, caster.CastActionId, caster);
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
		var finished = float.IsInfinity(lead) ? AnimationLockTweak.MeanFrameTime : lead;
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
					_gazers.Add(new Gazer(member.Position, member.GameObjectId ^ status.StatusId, false, status.RemainingTime,
						Origin.MemberStatus, status.StatusId, member));
				}
			}
		}
	}

	// "Look" and "no look" markers on the player (A776). The eyes are the enemies whose cast ends with the marker - within
	// the lead of each other - and among them the largest (A786: until then the largest casting, whatever it cast; with
	// "Look" a wrong eye turns the player away from the real one). Without a measured lead every cast counts, and RSR
	// only holds then.
	private static void AddMarkers(IBattleChara player, float lead)
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

		foreach (var marker in _markers)
		{
			var left = Math.Max(0f, marker.Duration - (float)(now - marker.Seen).TotalSeconds);
			_largest.Clear();
			var radius = 0f;
			foreach (var caster in DataCenter.CastingEnemies)
			{
				if (caster == null || !caster.IsCasting
					|| (!float.IsInfinity(lead) && MathF.Abs(caster.TotalCastTime - caster.CurrentCastTime - left) > lead))
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

			foreach (var eye in _largest)
			{
				_gazers.Add(new Gazer(eye.Position, eye.GameObjectId ^ (ulong)marker.Seen.Ticks, marker.Inverted, left,
					Origin.Marker, marker.Inverted ? 1u : 0u, eye));
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

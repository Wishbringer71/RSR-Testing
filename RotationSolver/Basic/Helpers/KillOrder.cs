using Dalamud.Game.ClientState.Objects.Enums;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Concept 03, "Tötungsregeln", rules 2 and 3 (A588), with the fights in concept 21, K12: enemies that must fall together,
/// after another, only in a place, or only at a time. Worked out once per frame from every battle character before the target lists are
/// built; a held enemy is no target, and where its fall itself harms, no area attack may catch it.
/// </summary>
internal static class KillOrder
{
	// Groups whose survivors begin an action when a member falls: an enrage that clears the arena, or a raise. BNpcName rows
	// and Action rows (xivapi v2, 05.11.2026; BossModReborn 27be42256).
	private static readonly (uint[] Members, uint SurvivorAction)[] FallTogether =
	[
		([14556, 14555], 48214), // younger and elder tablitaur piece: Endless Slashes, then the arena is hit
		([14661], 49287), // lamia piece: Raise on a dead one
	];

	// An enemy whose fall harms while another lives: no target then; NoArea, its fall itself harms, so no area either.
	private static readonly (uint Enemy, uint While, bool NoArea)[] NotBefore =
	[
		(14656, 14655, true), // morpho piece while an abaddon piece lives: its death confuses you
		(14655, 14656, false), // abaddon piece while a morpho piece lives: it must eat them first
	];

	// The myconid piece falls only in a puddle of Void Water III while the mindflayer piece lives: outside, its death spills
	// spores (Spore Spill). The puddle is an event object; spent, it stands in event state 7 (BossModReborn 27be42256).
	private const uint MyconidPiece = 14634;
	private const uint MindflayerPiece = 14633;
	private const uint WaterPuddle = 2005400;
	private const uint VoidWaterIII = 49200;
	private const byte SpentPuddle = 7;

	// Concept 03, rule 3 "nur zu einer Zeit", and concept 21, K12 "Königs-Ahriman" (A675): the King Ahriman piece's Roulette.
	// Breaking the final hourglass makes the needle's next beat fire Death along its heading, and Death lays Doom, which
	// kills; unbroken, Death fires when its cast ends. BNpcName, BNpcBase, Action and Status rows (xivapi v2, 07.11.2026;
	// GluttonyCombo cb5bf32).
	private const uint FinalHourglass = 14689;
	private const uint RouletteNeedle = 19738;
	private const uint NeedleDeath = 49422;
	private const uint DeathDoom = 5421;

	// Until the needle shows its own: the beat and how long before the needle's turn it falls (GluttonyCombo's recorded runs:
	// 16 Death casts one beat apart, each turn 0.49 s after its beat).
	private const double BeatFallback = 1.5605;
	private const double BeatBeforeTurnFallback = 0.49;

	// The needle points along the four axes (GluttonyCombo: every recorded heading): its heading in quarter turns.
	private const int Quarters = 4;

	// Half the survivor's window for the reserve; the other half is the way to the survivor and a stream that sinks at the end
	// (concept 03, "Tötungsregeln", rule 2).
	private const float ReserveShareOfWindow = 0.5f;

	private static readonly HashSet<ulong> _held = [];
	private static readonly HashSet<ulong> _noArea = [];
	private static readonly Dictionary<ulong, string> _reasons = [];
	private static readonly Dictionary<ulong, (string Name, DateTime At)> _traced = [];
	private static readonly List<IBattleChara> _members = [];
	private static readonly List<IBattleChara> _living = [];
	private static readonly List<System.Numerics.Vector3> _puddles = [];
	private static readonly HashSet<ulong> _first = [];

	// A687: every figure that has been targetable in this area - a living enemy counts only once it has been one; the helpers
	// that carry a group's name and cast its spells never are (six with 44 HP in each tablitaur fight on 11.11.2026).
	private static readonly HashSet<ulong> _seenTargetable = [];
	private static readonly HashSet<ulong> _present = [];
	private static readonly List<ulong> _gone = [];
	private static uint _territory;

	// A687: the highest stream measured on each group of FallTogether while a member lives - a history begins anew when its
	// enemy regains health, and a fresh fit over few samples shows almost nothing.
	private static readonly float[] _streamPeak = new float[FallTogether.Length];

	// A675: the needle as last read - its object, its heading in quarter turns, the sense and moment of its last turn - and the
	// beat it showed; a Death seen, for how long before a turn the beat falls.
	private static ulong _needleId;
	private static int _needleQuarter = -1;
	private static int _needleSense;
	private static DateTime _needleTurned = DateTime.MinValue;
	private static double _beat;
	private static double _beatBeforeTurn;
	private static DateTime _deathSeen = DateTime.MinValue;
	private static bool _needleMissTraced;
	private static string? _doomName;

	/// <summary>Whether a kill rule holds this enemy back: no target of any action.</summary>
	internal static bool IsHeld(IBattleChara battleChara)
	{
		return _held.Contains(battleChara.GameObjectId);
	}

	/// <summary>
	/// Whether a kill rule puts this enemy before every other: its fall is wanted now, because a later one harms as much as
	/// one at the wrong time (concept 03, rule 3, A675).
	/// </summary>
	internal static bool IsFirst(IBattleChara battleChara)
	{
		return _first.Contains(battleChara.GameObjectId);
	}

	/// <summary>An action's effect seen, from any source: the needle's Death, for the beat's place before its turn (A675).</summary>
	internal static void ObserveAction(uint actionId)
	{
		if (actionId == NeedleDeath)
		{
			_deathSeen = DateTime.Now;
		}
	}

	/// <summary>Whether a kill rule also keeps every area attack off this enemy.</summary>
	internal static bool MustNotBeHit(IBattleChara battleChara)
	{
		return _noArea.Contains(battleChara.GameObjectId);
	}

	/// <summary>
	/// Works out the held enemies; called once per frame by the target update before it builds the lists of targets.
	/// <paramref name="all"/> holds the targetable battle characters only; whether an enemy lives is read from the object
	/// table instead (concept 03, "Tötungsregeln", "Wer lebt"; A683).
	/// </summary>
	internal static void Update(List<IBattleChara> all)
	{
		_held.Clear();
		_noArea.Clear();
		_reasons.Clear();
		_first.Clear();
		var territory = (uint)Svc.ClientState.TerritoryType;
		if (territory != _territory)
		{
			_territory = territory;
			_seenTargetable.Clear();
			Array.Clear(_streamPeak);
		}

		foreach (var battleChara in all)
		{
			_ = _seenTargetable.Add(battleChara.GameObjectId);
		}

		var rules = Service.Config.KillRules && !DataCenter.IsPvP && all.Count > 0;
		if (rules || _traced.Count > 0)
		{
			CollectLiving();
		}

		if (rules)
		{
			for (var group = 0; group < FallTogether.Length; group++)
			{
				HoldTogether(group);
			}

			foreach (var (enemy, others, noArea) in NotBefore)
			{
				if (AnyLiving(others))
				{
					Hold(all, enemy, noArea, $"its fall harms while a {NameOf(others)} lives");
				}
			}

			HoldOutsidePuddles(all);
			HoldAgainstTheNeedle(all);
		}

		Trace();
	}

	// A683: every enemy of the object table with HP left, targetable or not. An enemy that cannot be targeted for a while - the
	// elder tablitaur through its Endless Swing, 26 s on 08.11.2026 - still begins its action when another falls, and the
	// target list, which holds the targetable ones only, would count it as gone. A687: only one that has been targetable in
	// this area; a figure that never was is a helper (concept 03, "Wer lebt").
	private static void CollectLiving()
	{
		_living.Clear();
		_present.Clear();
		foreach (var obj in Svc.Objects)
		{
			if (obj is not IBattleChara battleChara)
			{
				continue;
			}

			_ = _present.Add(battleChara.GameObjectId);
			if (battleChara.CurrentHp > 0 && battleChara.IsEnemySideFigure() && _seenTargetable.Contains(battleChara.GameObjectId))
			{
				_living.Add(battleChara);
			}
		}

		// Ids gone from the object table leave the set, so a later figure under a reused id starts unseen.
		_gone.Clear();
		foreach (var id in _seenTargetable)
		{
			if (!_present.Contains(id))
			{
				_gone.Add(id);
			}
		}

		foreach (var id in _gone)
		{
			_ = _seenTargetable.Remove(id);
		}
	}

	// Rule 2: every member at or below the reserve waits while another living member stands above it.
	private static void HoldTogether(int group)
	{
		var (nameIds, survivorAction) = FallTogether[group];
		_members.Clear();
		foreach (var battleChara in _living)
		{
			if (Array.IndexOf(nameIds, battleChara.NameId) >= 0)
			{
				_members.Add(battleChara);
			}
		}

		if (_members.Count == 0)
		{
			_streamPeak[group] = 0f;
		}

		// Your stream in HP per second: the largest fall among the members, from their health history. You attack one after
		// the other, and the one under attack shows your stream. Its highest value stands while a member lives (A687): a
		// history begins anew when its enemy regains health, and holding one must not empty the stream - a sum over the
		// attackable ones would, and the reserve with it.
		foreach (var member in _members)
		{
			var rate = TimeToKillTracker.GetRate(member.GameObjectId);
			if (!float.IsNaN(rate) && rate * member.MaxHp > _streamPeak[group])
			{
				_streamPeak[group] = rate * member.MaxHp;
			}
		}

		if (_members.Count <= 1 || Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>()?.GetRowOrDefault(survivorAction) is not { } action)
		{
			return;
		}

		var window = action.CastSeconds();
		var stream = _streamPeak[group];
		if (window <= 0f || stream <= 0f)
		{
			return;
		}

		var reserve = stream * window * ReserveShareOfWindow;
		foreach (var member in _members)
		{
			if (member.CurrentHp > reserve)
			{
				continue;
			}

			foreach (var other in _members)
			{
				// A683: also while the other cannot be targeted or hit - it begins its action when this one falls all the same.
				if (other.GameObjectId != member.GameObjectId && other.CurrentHp > reserve)
				{
					Add(member, true, $"must fall with {other.Name.TextValue} ({other.CurrentHp} HP): held at {member.CurrentHp} HP, "
						+ $"reserve {reserve:F0} HP = {stream:F0} HP/s over half of {window:F1} s");
					break;
				}
			}
		}
	}

	// Rule 3, a place: a myconid piece outside every live water puddle, while the mindflayer piece lives.
	private static unsafe void HoldOutsidePuddles(List<IBattleChara> all)
	{
		if (!AnyLiving(MindflayerPiece) || !AnyLiving(MyconidPiece)
			|| Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>()?.GetRowOrDefault(VoidWaterIII) is not { } water)
		{
			return;
		}

		_puddles.Clear();
		foreach (var obj in Svc.Objects)
		{
			if (obj is { ObjectKind: ObjectKind.EventObj } && obj.BaseId == WaterPuddle && obj.Address != IntPtr.Zero
				&& ((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)obj.Address)->EventState != SpentPuddle)
			{
				_puddles.Add(obj.Position);
			}
		}

		float radius = water.EffectRange;
		foreach (var battleChara in all)
		{
			if (battleChara.NameId != MyconidPiece || battleChara.CurrentHp == 0 || !battleChara.IsEnemy())
			{
				continue;
			}

			var inside = false;
			foreach (var puddle in _puddles)
			{
				if (System.Numerics.Vector2.Distance(new(puddle.X, puddle.Z), new(battleChara.Position.X, battleChara.Position.Z)) <= radius)
				{
					inside = true;
					break;
				}
			}

			if (!inside)
			{
				Add(battleChara, true, $"outside the water puddles ({_puddles.Count} live, {radius:F0} y): its fall would spill spores");
			}
		}
	}

	// Rule 3, only at a time (A675): the final hourglass is no target, and no area catches it, while a press now could break
	// it on a beat that points Death at the player; otherwise it comes first.
	private static void HoldAgainstTheNeedle(List<IBattleChara> all)
	{
		IBattleChara? hourglass = null;
		foreach (var battleChara in all)
		{
			if (battleChara.NameId == FinalHourglass && battleChara.CurrentHp > 0 && battleChara.IsEnemy())
			{
				hourglass = battleChara;
				break;
			}
		}

		if (hourglass == null)
		{
			_needleMissTraced = false;
			return;
		}

		var needle = FollowNeedle();
		var player = Player.Object;
		if (player != null && hourglass.CurrentHp < hourglass.MaxHp && !DoomResisted())
		{
			if (needle == null || _needleTurned == DateTime.MinValue)
			{
				if (!_needleMissTraced)
				{
					_needleMissTraced = true;
					DefenseTrace.Line($"kill rule: {hourglass.Name.TextValue} - the roulette needle is {(needle == null ? "not found" : "not yet seen to turn")}, nothing held");
				}
			}
			else if (Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>()?.GetRowOrDefault(NeedleDeath) is { } death)
			{
				var lag = Math.Max(0f, TankbusterForecast.RoundTrip);
				var reach = death.EffectRange + player.HitboxRadius;
				var wait = WaitForSafePress(lag, needle.Position, player.Position, reach);
				var castLeft = hourglass.IsCasting ? hourglass.TotalCastTime - hourglass.CurrentCastTime : float.MaxValue;
				if (wait > 0d && !double.IsPositiveInfinity(wait) && wait + lag + Beat < castLeft)
				{
					Add(hourglass, true, $"a break now could fire Death along the needle at you (beat {Beat:F2} s, round trip {lag:F2} s); safe in {wait:F1} s");
					return;
				}
			}
		}

		_ = _first.Add(hourglass.GameObjectId);
	}

	private static double Beat => _beat > 0d ? _beat : BeatFallback;

	private static double BeatBeforeTurn => _beatBeforeTurn > 0d ? _beatBeforeTurn : BeatBeforeTurnFallback;

	// Reads the needle from the whole object table - it is no target - and follows its turns: the beat between two, the
	// sense of the turn, and how long after a seen Death the turn came.
	private static IGameObject? FollowNeedle()
	{
		IGameObject? needle = null;
		foreach (var obj in Svc.Objects)
		{
			if (obj != null && obj.BaseId == RouletteNeedle)
			{
				needle = obj;
				break;
			}
		}

		if (needle == null)
		{
			_needleId = 0;
			_needleQuarter = -1;
			_needleTurned = DateTime.MinValue;
			return null;
		}

		var quarter = QuarterOf(needle.Rotation);
		var now = DateTime.Now;
		if (needle.GameObjectId != _needleId || _needleQuarter < 0)
		{
			_needleId = needle.GameObjectId;
			_needleQuarter = quarter;
			_needleTurned = DateTime.MinValue;
			return needle;
		}

		if (quarter == _needleQuarter)
		{
			return needle;
		}

		var steps = (quarter - _needleQuarter + Quarters) % Quarters;
		var sense = steps == 1 ? 1 : steps == Quarters - 1 ? -1 : 0;
		if (_needleTurned != DateTime.MinValue && sense != 0 && sense == _needleSense)
		{
			_beat = (now - _needleTurned).TotalSeconds;
			if (_deathSeen > _needleTurned)
			{
				var before = (now - _deathSeen).TotalSeconds;
				if (before > 0d && before < _beat)
				{
					_beatBeforeTurn = before;
				}
			}
		}

		_needleSense = sense;
		_needleQuarter = quarter;
		_needleTurned = sense == 0 ? DateTime.MinValue : now;
		return needle;
	}

	private static int QuarterOf(float rotation)
	{
		var quarter = (int)Math.Round(rotation / (Math.Tau / Quarters));
		return ((quarter % Quarters) + Quarters) % Quarters;
	}

	// Seconds from now until a press can no longer break the hourglass on a beat that points Death at the player: 0 when a
	// press now cannot, infinity when no beat in a full turn of the needle is safe. The beats a press can reach change only
	// where its earliest break crosses a beat, so the time splits into spans of one beat with one answer each.
	private static double WaitForSafePress(float lag, Vector3 needle, Vector3 player, float reach)
	{
		if (!PressAims(0d, lag, needle, player, reach))
		{
			return 0d;
		}

		var since = (DateTime.Now - _needleTurned).TotalSeconds;
		for (var k = 0; k <= Quarters + 1; k++)
		{
			var start = -since - BeatBeforeTurn + (k * Beat) - lag;
			if (start <= 0d)
			{
				continue;
			}

			if (!PressAims(start + (Beat / 2d), lag, needle, player, reach))
			{
				return start;
			}
		}

		return double.PositiveInfinity;
	}

	// Whether a press x seconds from now could break the hourglass on a beat that points Death at the player. The break
	// lands one round trip after the press at the earliest and one beat later at the latest; Death fires on the first beat
	// after it. Beat k (k >= 1) falls at -since - before + k * beat and fires with the heading the needle took at the turn
	// after beat k - 1.
	private static bool PressAims(double x, float lag, Vector3 needle, Vector3 player, float reach)
	{
		var beat = Beat;
		var since = (DateTime.Now - _needleTurned).TotalSeconds;
		var earliest = x + lag;
		var latest = earliest + beat;
		for (var k = 1; k <= Quarters * Quarters; k++)
		{
			var at = -since - BeatBeforeTurn + (k * beat);
			if (at - beat >= latest)
			{
				break;
			}

			if (at >= earliest && Faces(_needleQuarter + ((k - 1) * _needleSense), needle, player, reach))
			{
				return true;
			}
		}

		return false;
	}

	// Whether the needle at this heading points into the player's half: no cone narrower than 180 degrees reaches him from
	// the side or behind, and the sheet gives Death no angle (concept 21, K12).
	private static bool Faces(int quarter, Vector3 needle, Vector3 player, float reach)
	{
		var dx = player.X - needle.X;
		var dz = player.Z - needle.Z;
		if ((dx * dx) + (dz * dz) > reach * reach)
		{
			return false;
		}

		var heading = quarter * (Math.Tau / Quarters);
		return (Math.Sin(heading) * dx) + (Math.Cos(heading) * dz) > 0d;
	}

	// Gear that makes the player immune to Death's Doom: the hourglass then falls as soon as it can.
	private static bool DoomResisted()
	{
		_doomName ??= Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(Dalamud.Game.ClientLanguage.English)?.GetRowOrDefault(DeathDoom)?.Name.ExtractText() ?? string.Empty;
		return _doomName.Length > 0 && BeastmasterRotation.WornResists(_doomName);
	}

	private static void Hold(List<IBattleChara> all, uint nameId, bool noArea, string reason)
	{
		foreach (var battleChara in all)
		{
			if (battleChara.NameId == nameId && battleChara.CurrentHp > 0 && battleChara.IsEnemy())
			{
				Add(battleChara, noArea, reason);
			}
		}
	}

	private static void Add(IBattleChara battleChara, bool noArea, string reason)
	{
		_ = _held.Add(battleChara.GameObjectId);
		if (noArea)
		{
			_ = _noArea.Add(battleChara.GameObjectId);
		}

		_reasons[battleChara.GameObjectId] = reason;
	}

	private static bool AnyLiving(uint nameId)
	{
		foreach (var battleChara in _living)
		{
			if (battleChara.NameId == nameId)
			{
				return true;
			}
		}

		return false;
	}

	private static string NameOf(uint nameId)
	{
		foreach (var battleChara in _living)
		{
			if (battleChara.NameId == nameId)
			{
				return battleChara.Name.TextValue;
			}
		}

		return nameId.ToString();
	}

	// One line when an enemy is held and one when it is released, at most once a GCD per enemy.
	private static void Trace()
	{
		var now = DateTime.Now;
		foreach (var (id, reason) in _reasons)
		{
			if (_traced.ContainsKey(id))
			{
				continue;
			}

			var name = Svc.Objects.SearchById(id)?.Name.TextValue ?? id.ToString("X");
			_traced[id] = (name, now);
			DefenseTrace.Line($"kill rule: {name} held - {reason}");
		}

		List<ulong>? released = null;
		foreach (var (id, traced) in _traced)
		{
			if (!_held.Contains(id) && (now - traced.At).TotalSeconds >= DataCenter.DefaultGCDTotal)
			{
				(released ??= []).Add(id);
			}
		}

		if (released == null)
		{
			return;
		}

		foreach (var id in released)
		{
			var name = _traced[id].Name;
			_ = _traced.Remove(id);
			var alive = false;
			foreach (var battleChara in _living)
			{
				if (battleChara.GameObjectId == id)
				{
					alive = true;
					break;
				}
			}

			DefenseTrace.Line($"kill rule: {name} released - {(alive ? "its rule no longer holds it" : "it is gone")}");
		}
	}
}

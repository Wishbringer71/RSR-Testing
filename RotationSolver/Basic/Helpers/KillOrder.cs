using Dalamud.Game.ClientState.Objects.Enums;
using ECommons.DalamudServices;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Concept 03, "Tötungsregeln", rules 2 and 3 (A588), with the fights in concept 01, K12: enemies that must fall together,
/// after another, or only in a place. Worked out once per frame from every battle character before the target lists are
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

	// Half the survivor's window for the reserve; the other half is the way to the survivor and a stream that sinks at the end
	// (concept 03, "Tötungsregeln", rule 2).
	private const float ReserveShareOfWindow = 0.5f;

	private static readonly HashSet<ulong> _held = [];
	private static readonly HashSet<ulong> _noArea = [];
	private static readonly Dictionary<ulong, string> _reasons = [];
	private static readonly Dictionary<ulong, (string Name, DateTime At)> _traced = [];
	private static readonly List<IBattleChara> _members = [];
	private static readonly List<System.Numerics.Vector3> _puddles = [];

	/// <summary>Whether a kill rule holds this enemy back: no target of any action.</summary>
	internal static bool IsHeld(IBattleChara battleChara)
	{
		return _held.Contains(battleChara.GameObjectId);
	}

	/// <summary>Whether a kill rule also keeps every area attack off this enemy.</summary>
	internal static bool MustNotBeHit(IBattleChara battleChara)
	{
		return _noArea.Contains(battleChara.GameObjectId);
	}

	/// <summary>
	/// Works out the held enemies from every battle character in reach of the game; called once per frame by the target
	/// update before it builds the lists of targets.
	/// </summary>
	internal static void Update(List<IBattleChara> all)
	{
		_held.Clear();
		_noArea.Clear();
		_reasons.Clear();
		if (Service.Config.KillRules && !DataCenter.IsPvP && all.Count > 0)
		{
			foreach (var (members, survivorAction) in FallTogether)
			{
				HoldTogether(all, members, survivorAction);
			}

			foreach (var (enemy, others, noArea) in NotBefore)
			{
				if (AnyLiving(all, others))
				{
					Hold(all, enemy, noArea, $"its fall harms while a {NameOf(all, others)} lives");
				}
			}

			HoldOutsidePuddles(all);
		}

		Trace(all);
	}

	// Rule 2: every member at or below the reserve waits while another attackable member stands above it.
	private static void HoldTogether(List<IBattleChara> all, uint[] nameIds, uint survivorAction)
	{
		_members.Clear();
		foreach (var battleChara in all)
		{
			if (battleChara.CurrentHp > 0 && Array.IndexOf(nameIds, battleChara.NameId) >= 0 && battleChara.IsEnemy())
			{
				_members.Add(battleChara);
			}
		}

		if (_members.Count <= 1 || Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>()?.GetRowOrDefault(survivorAction) is not { } action)
		{
			return;
		}

		var window = action.CastSeconds();

		// Your stream in HP per second: the largest fall among the members, from their health history. You attack one after
		// the other, and the one under attack shows your stream; a held member's last value stands until its history lapses
		// (30 s), so holding one does not empty the stream - a sum over the attackable ones would, and the reserve with it.
		var stream = 0f;
		foreach (var member in _members)
		{
			var rate = TimeToKillTracker.GetRate(member.GameObjectId);
			if (!float.IsNaN(rate) && rate * member.MaxHp > stream)
			{
				stream = rate * member.MaxHp;
			}
		}

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
				if (other.GameObjectId != member.GameObjectId && other.CurrentHp > reserve && other.IsTargetable && !Invincible(other))
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
		if (!AnyLiving(all, MindflayerPiece) || !AnyLiving(all, MyconidPiece)
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

	private static bool AnyLiving(List<IBattleChara> all, uint nameId)
	{
		foreach (var battleChara in all)
		{
			if (battleChara.NameId == nameId && battleChara.CurrentHp > 0 && battleChara.IsEnemy())
			{
				return true;
			}
		}

		return false;
	}

	private static string NameOf(List<IBattleChara> all, uint nameId)
	{
		foreach (var battleChara in all)
		{
			if (battleChara.NameId == nameId)
			{
				return battleChara.Name.TextValue;
			}
		}

		return nameId.ToString();
	}

	private static bool Invincible(IBattleChara battleChara)
	{
		foreach (var status in battleChara.StatusList)
		{
			if (status != null && status.StatusId != 0 && status.IsInvincible())
			{
				return true;
			}
		}

		return false;
	}

	// One line when an enemy is held and one when it is released, at most once a GCD per enemy.
	private static void Trace(List<IBattleChara> all)
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
			foreach (var battleChara in all)
			{
				if (battleChara.GameObjectId == id && battleChara.CurrentHp > 0)
				{
					alive = true;
					break;
				}
			}

			DefenseTrace.Line($"kill rule: {name} released - {(alive ? "its rule no longer holds it" : "it is gone")}");
		}
	}
}

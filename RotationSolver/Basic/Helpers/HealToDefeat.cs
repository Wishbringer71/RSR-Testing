using Dalamud.Game.ClientState.Objects.Enums;
using ECommons.GameFunctions;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Figures that only single-target healing defeats: friendly battle figures outside the party that the fight brings in
/// to be healed to full - the Reiryu of Shinryu (Unreal), which appear at 1 HP, tether a player each and fly off at the
/// end of their cast unless healed to full first (concept 07, "Heilziele außerhalb der Gruppe", A516).
/// </summary>
/// <remarks>
/// <para>Recognition, per frame after the target lists: a battle figure that is neither a party nor an alliance member,
/// no pet, chocobo or NPC party member, that the game does not let the player attack, alive and below full health -
/// and that carries one of two signs: it was first seen at 1 HP, the least a living figure holds, or it tethers a party
/// member (the tether sits in its own effect list). A heal of the player's that lands for 0 takes the figure's kind out
/// for the session. The list is the figures' own: party values, area thresholds and mitigation do not read it.</para>
///
/// <para>The deadline for non-healers: the remaining time of a cast on a figure; the need, the health the figures still
/// miss; the rate, the health they gained since the wave began over the time it has run. Behind when the rate does not
/// cover the need within the deadline. Before the first cast there is no deadline and nobody is behind.</para>
/// </remarks>
internal static class HealToDefeat
{
	private sealed class Figure
	{
		public required string Name { get; init; }
		public required uint Kind { get; init; }
		public required uint FirstHp { get; init; }
		public uint LastHp { get; set; }
		public uint MaxHp { get; set; }
		public bool Present { get; set; }
		public DateTime CastEnds { get; set; }
	}

	private static readonly Dictionary<ulong, Figure> _figures = [];
	private static readonly HashSet<ulong> _seen = [];
	private static readonly HashSet<uint> _refusedKinds = [];
	private static uint _territory;
	private static DateTime _waveStart;
	private static int _waveFigures;
	private static int _waveHeals;
	private static double _waveGained;
	private static bool _behindTraced;

	/// <summary>Whether the healing the figures receive does not finish them within the deadline of a cast on one.</summary>
	internal static bool Behind { get; private set; }

	/// <summary>Whether this player, not a healer, is to help: the option is on and the healers are behind.</summary>
	internal static bool CallsForNonHealer => Behind && Service.Config.HealToDefeatAsNonHealer
		&& DataCenter.Role != JobRole.Healer;

	/// <summary>Whether the player is to heal the figures now: a healer whenever one stands, anyone else when called.</summary>
	internal static bool CallsForPlayer => DataCenter.HealToDefeatTargets.Count > 0
		&& (DataCenter.Role == JobRole.Healer || CallsForNonHealer);

	/// <summary>Whether <paramref name="chara"/> is one of the figures standing now.</summary>
	internal static bool IsFigure(IBattleChara? chara)
		=> chara != null && _figures.TryGetValue(chara.GameObjectId, out var figure) && figure.Present;

	/// <summary>Brings the figures up to date. Called once a frame after the target lists.</summary>
	internal static void Update()
	{
		var list = DataCenter.HealToDefeatTargets;
		list.Clear();

		if (DataCenter.TerritoryID != _territory)
		{
			_territory = DataCenter.TerritoryID;
			_figures.Clear();
			_seen.Clear();
			_refusedKinds.Clear();
			ResetWave();
		}

		foreach (var figure in _figures.Values)
		{
			figure.Present = false;
		}

		if (!DataCenter.IsPvP && DataCenter.InCombat && DataCenter.AllTargets != null)
		{
			foreach (var chara in DataCenter.AllTargets)
			{
				Observe(chara, list);
			}
		}

		EndFiguresGone();
		UpdateDeadline(list);
	}

	private static void Observe(IBattleChara chara, List<IBattleChara> list)
	{
		if (!IsFriendlyFigure(chara))
		{
			return;
		}

		// First sight is the first frame in combat the figure is seen at all, at whatever health: a friendly figure that
		// is brought down to 1 HP later did not appear that way.
		var id = chara.GameObjectId;
		var firstSight = _seen.Add(id);
		if (chara.IsDead || chara.CurrentHp == 0 || chara.CurrentHp >= chara.MaxHp)
		{
			return;
		}

		if (!_figures.TryGetValue(id, out var figure))
		{
			string? sign = firstSight && chara.CurrentHp == 1 ? "appeared at 1 HP" : null;
			if (sign == null && TetheredMember(chara) is { } member)
			{
				sign = $"tethers {member.Name.TextValue}";
			}

			if (sign == null)
			{
				return;
			}

			figure = new Figure { Name = chara.Name.TextValue, Kind = chara.BaseId, FirstHp = chara.CurrentHp };
			_figures[id] = figure;
			if (_waveFigures == 0)
			{
				_waveStart = DateTime.Now;
			}

			_waveFigures++;
			DefenseTrace.Line($"figure only healing defeats: {figure.Name} (kind {figure.Kind}) at {chara.CurrentHp}/{chara.MaxHp} HP, "
				+ $"{sign} - single heals go to it after a member about to die");
		}

		figure.Present = true;
		figure.LastHp = chara.CurrentHp;
		figure.MaxHp = chara.MaxHp;
		if (chara.IsCasting)
		{
			figure.CastEnds = DateTime.Now.AddSeconds(Math.Max(0f, chara.TotalCastTime - chara.CurrentCastTime));
		}

		list.Add(chara);
	}

	// A battle figure on the party's side that is not in it: no party or alliance member, pet, chocobo or NPC party member,
	// and the game does not let the player attack it.
	private static bool IsFriendlyFigure(IBattleChara chara)
	{
		if (chara == null || chara.ObjectKind != ObjectKind.BattleNpc || _refusedKinds.Contains(chara.BaseId))
		{
			return false;
		}

		if (chara.GetBattleNPCSubKind() is BattleNpcSubKind.Pet or BattleNpcSubKind.Buddy or BattleNpcSubKind.NpcPartyMember)
		{
			return false;
		}

		return !chara.IsParty() && !chara.IsAllianceMember() && !chara.IsEnemy();
	}

	/// <summary>The party member a tether of the figure's own reaches, or null.</summary>
	// Read from its effect list: the game keeps a tether on its
	// source (Reiryu -> player, tether 1 in BossModReborn's Un2Shinryu).
	internal static unsafe IBattleChara? TetheredMember(IBattleChara chara)
	{
		var tethers = chara.Struct()->Vfx.Tethers;
		for (var i = 0; i < tethers.Length; i++)
		{
			if (tethers[i].Id == 0)
			{
				continue;
			}

			ulong target = tethers[i].TargetId;
			foreach (var member in DataCenter.PartyMembers)
			{
				if (member.GameObjectId == target)
				{
					return member;
				}
			}
		}

		return null;
	}

	// A figure no longer in the table has died - healed to full - or flown off at the end of its cast. Gone after its cast
	// had ended, it was not healed in time. When the last one of a wave is gone, one line says how the wave went.
	private static void EndFiguresGone()
	{
		List<ulong>? gone = null;
		foreach (var (id, figure) in _figures)
		{
			if (!figure.Present)
			{
				(gone ??= []).Add(id);
			}
		}

		if (gone == null)
		{
			return;
		}

		List<string>? unhealed = null;
		var now = DateTime.Now;
		foreach (var id in gone)
		{
			var figure = _figures[id];
			var outlivedCast = figure.CastEnds != default && now >= figure.CastEnds;
			if (outlivedCast && figure.MaxHp > 0)
			{
				(unhealed ??= []).Add($"{figure.Name} at {(float)figure.LastHp / figure.MaxHp:P0}");
			}

			// Its gain stays in the wave's rate: gone before its cast ended it was healed to full - all it missed -, else
			// what it had gained when last seen (A726).
			var reached = outlivedCast ? figure.LastHp : figure.MaxHp;
			_waveGained += reached > figure.FirstHp ? reached - figure.FirstHp : 0;
			_ = _figures.Remove(id);
		}

		if (_figures.Count == 0 && _waveFigures > 0)
		{
			DefenseTrace.Line($"figures only healing defeats: {_waveFigures} gone after {(DateTime.Now - _waveStart).TotalSeconds:F1} s, "
				+ $"{_waveHeals} heal(s) of yours on them, "
				+ (unhealed == null ? "none outlived its cast" : $"outlived its cast, unhealed: {string.Join(", ", unhealed)}"));
			ResetWave();
		}
	}

	private static void UpdateDeadline(List<IBattleChara> list)
	{
		var deadline = float.MaxValue;
		uint missing = 0;
		foreach (var chara in list)
		{
			missing += chara.MaxHp - chara.CurrentHp;
			if (chara.IsCasting)
			{
				deadline = Math.Min(deadline, Math.Max(0f, chara.TotalCastTime - chara.CurrentCastTime));
			}
		}

		if (list.Count == 0 || deadline == float.MaxValue)
		{
			Behind = false;
			return;
		}

		// What the wave's figures have gained since it began, those already healed to full included.
		var gained = _waveGained;
		foreach (var figure in _figures.Values)
		{
			gained += figure.LastHp > figure.FirstHp ? figure.LastHp - figure.FirstHp : 0;
		}

		var elapsed = (DateTime.Now - _waveStart).TotalSeconds;
		var rate = elapsed > 0 ? gained / elapsed : 0;
		Behind = rate <= 0 || missing > rate * deadline;
		if (Behind && !_behindTraced)
		{
			_behindTraced = true;
			DefenseTrace.Line($"figures only healing defeats: behind - {missing} HP missing, {rate:F0} HP/s so far, {deadline:F1} s left "
				+ $"of the cast; non-healers {(Service.Config.HealToDefeatAsNonHealer ? "help" : "do not help (option off)")}");
		}
	}

	/// <summary>
	/// Records the heals of one own action: a heal on a figure counts for the wave's line, and a heal that lands on one
	/// for 0 takes its kind out for the session - it takes no healing, whatever it looked like.
	/// </summary>
	internal static void NoteOwnHeals(IReadOnlyDictionary<ulong, uint>? amounts)
	{
		if (amounts == null || amounts.Count == 0 || _figures.Count == 0)
		{
			return;
		}

		foreach (var (id, amount) in amounts)
		{
			if (!_figures.TryGetValue(id, out var figure))
			{
				continue;
			}

			if (amount > 0)
			{
				_waveHeals++;
				continue;
			}

			if (_refusedKinds.Add(figure.Kind))
			{
				DefenseTrace.Line($"figure only healing defeats: a heal of yours did nothing to {figure.Name} (kind {figure.Kind}) "
					+ "- no more single heals on this kind in this area");
			}
		}
	}

	private static void ResetWave()
	{
		_waveFigures = 0;
		_waveHeals = 0;
		_waveGained = 0;
		_behindTraced = false;
		Behind = false;
	}
}

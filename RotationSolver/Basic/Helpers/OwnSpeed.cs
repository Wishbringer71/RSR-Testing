using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using System.Text;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// The player's own speed, determined instead of assumed (concept 03, "Die eigene Geschwindigkeit", A872; his words of
/// 17.11.2026: "da du immer igendwelche sekundenwerte für GCDs ANNIMMST. Also ist das eine methode zur FESTSTELLUNG"). At the
/// start of a fight: skill speed, spell speed and haste from gear, materia and the meal (gear cannot change in a fight, so
/// once), the meal buff checked for existence, effect and duration (his precept: "es ist ein buff, also muss der geprüft
/// werden auf bestand, effekt und dauer"), and the recast and cast time the game gives each GCD of the rotation at that speed.
/// When the meal's time runs out in the fight, it is read again - eaten again, the new end counts; gone, the line names the
/// values and the GCD after it. It decides nothing - every time RSR uses comes from the game with all of this in it - and it
/// does not eat (his precision of 17.11.2026).
/// </summary>
internal static class OwnSpeed
{
	// BaseParam rows (sheet BaseParam; PlayerState.Attributes is indexed by these rows).
	private const int SkillSpeedParam = 45;
	private const int SpellSpeedParam = 46;
	private const int HasteParam = 47;

	// The meal status carries the ItemFood row, plus this offset for a high quality meal: 0x2966 = 10598 is row 598, high
	// quality, in his ACT logs of 15.11.2026 (FFXIVClientStructs Status.Param: "ID of the food/potion in the ItemFood sheet").
	private const ushort HighQualityParam = 10000;

	// The two speeds and the values of the meal, as they stood while it was on (kept across fights). Haste is written, not
	// watched: what the client keeps in its row is unverified, and a haste buff could move it while the meal ends.
	private static readonly List<(int Param, int Start)> _watched = [];

	private static DateTime _endsAt = DateTime.MaxValue;
	private static DateTime _combatStart;
	private static DateTime _endedAt;
	private static bool _endPending;
	private static float _gcdAtStart;

	/// <summary>
	/// The start of a fight: speeds, haste and meal, and the GCD table, as the part of the "combat started" line after the
	/// ordinary GCD. Remembers when the meal ends.
	/// </summary>
	internal static string CombatStarted(DateTime now, float ordinaryGcd)
	{
		_combatStart = now;
		_endPending = false;
		_endsAt = DateTime.MaxValue;
		_gcdAtStart = ordinaryGcd;
		_watched.Clear();
		Watch(SkillSpeedParam);
		Watch(SpellSpeedParam);

		var text = new StringBuilder();
		_ = text.Append($"skill speed {Attribute(SkillSpeedParam)}, spell speed {Attribute(SpellSpeedParam)}, haste "
			+ $"{Attribute(HasteParam)}; ");
		AppendMeal(text, now);
		AppendGcdTable(text);
		return text.ToString();
	}

	/// <summary>
	/// Once per frame in a fight. With a meal on: a time comparison, until its remembered end; then the status is read again -
	/// eaten again, it sets the new end; gone, the values the meal gave are awaited and written with the GCD after it. Without
	/// one: the player's statuses are read, so a meal eaten in the fight is named with its effect and duration.
	/// </summary>
	internal static void Frame(DateTime now)
	{
		if (!DataCenter.InCombat)
		{
			return;
		}

		if (!_endPending && _endsAt == DateTime.MaxValue)
		{
			if (TryReadMeal(out _, out _))
			{
				var text = new StringBuilder($"food eaten in combat {(now - _combatStart).TotalSeconds:F0} s into the fight; ");
				_watched.Clear();
				Watch(SkillSpeedParam);
				Watch(SpellSpeedParam);
				AppendMeal(text, now);
				DefenseTrace.Line(text.ToString());
			}

			return;
		}

		if (!_endPending)
		{
			if (now < _endsAt)
			{
				return;
			}

			if (TryReadMeal(out _, out var remaining))
			{
				_endsAt = now.AddSeconds(remaining);
				return;
			}

			_endPending = true;
			_endedAt = now;
		}
		else if (TryReadMeal(out _, out var again))
		{
			// Eaten again before the values moved: no end to report, the new meal's end is the next one.
			_endPending = false;
			_endsAt = now.AddSeconds(again);
			return;
		}

		// The game recalculates the values after the status is gone; the line waits for the first one that moved.
		if (AnyWatchedMoved())
		{
			WriteEnd();
		}
	}

	/// <summary>The fight ended: a meal that ended in it without a moved value is written now.</summary>
	internal static void CombatEnded()
	{
		if (_endPending)
		{
			WriteEnd();
		}

		_endsAt = DateTime.MaxValue;
	}

	private static void AppendMeal(StringBuilder text, DateTime now)
	{
		if (!TryReadMeal(out var param, out var remaining))
		{
			_ = text.Append("no food");
			return;
		}

		_endsAt = now.AddSeconds(remaining);
		var highQuality = param >= HighQualityParam;
		var row = (uint)(highQuality ? param - HighQualityParam : param);
		_ = text.Append($"food {row}{(highQuality ? " HQ" : string.Empty)}, {remaining:F0} s left");
		if (Service.GetSheet<ItemFood>().GetRowOrDefault(row) is not { } food)
		{
			_ = text.Append(" (no ItemFood row)");
			return;
		}

		foreach (var value in food.Params)
		{
			if (value.BaseParam.RowId == 0)
			{
				continue;
			}

			var share = highQuality ? value.ValueHQ : value.Value;
			var cap = highQuality ? value.MaxHQ : value.Max;
			_ = text.Append($", {value.BaseParam.Value.Name.ExtractText()} +{share}{(value.IsRelative ? $" % (max {cap})" : string.Empty)}");
			Watch((int)value.BaseParam.RowId);
		}
	}

	// Every GCD of the rotation the player has, enabled: recast and cast time as the game adjusts them now, grouped by the pair
	// so the line names each timing once ("2.47/0.00 s: A, B; 2.47/1.48 s: C").
	private static unsafe void AppendGcdTable(StringBuilder text)
	{
		var actions = DataCenter.CurrentRotation?.AllBaseActions;
		if (actions == null)
		{
			return;
		}

		var groups = new Dictionary<(int Recast, int Cast), StringBuilder>();
		var seen = new HashSet<uint>();
		var pvp = DataCenter.IsPvP;
		foreach (var action in actions)
		{
			// The current mode only, and each action the game resolves to once: a combo step and its replacement are one.
			if (!action.Info.IsRealGCD || !action.EnoughLevel || !action.IsEnabled || action.Action.IsPvP != pvp
				|| !seen.Add(action.AdjustedID))
			{
				continue;
			}

			var recast = ActionManager.GetAdjustedRecastTime(ActionType.Action, action.AdjustedID);
			var cast = ActionManager.GetAdjustedCastTime(ActionType.Action, action.AdjustedID);
			if (!groups.TryGetValue((recast, cast), out var names))
			{
				names = new StringBuilder();
				groups[(recast, cast)] = names;
			}

			_ = names.Append(names.Length == 0 ? string.Empty : ", ").Append(action.Name);
		}

		_ = text.Append("; GCDs recast/cast");
		foreach (var (key, names) in groups)
		{
			_ = text.Append($"; {TimeSpan.FromMilliseconds(key.Recast).TotalSeconds:F2}/"
				+ $"{TimeSpan.FromMilliseconds(key.Cast).TotalSeconds:F2} s: {names}");
		}
	}

	private static void WriteEnd()
	{
		_endPending = false;
		_endsAt = DateTime.MaxValue;
		var text = new StringBuilder($"food ended in combat {(_endedAt - _combatStart).TotalSeconds:F0} s into the fight");
		foreach (var (param, start) in _watched)
		{
			var after = Attribute(param);
			if (after == start)
			{
				continue;
			}

			var name = Service.GetSheet<BaseParam>().GetRowOrDefault((uint)param) is { } row ? row.Name.ExtractText() : param.ToString();
			_ = text.Append($"; {name} {start} -> {after}");
		}

		_ = text.Append($"; ordinary GCD {_gcdAtStart:F2} -> {ActionManagerHelper.GetDefaultAdjustedRecastTime():F2} s");
		DefenseTrace.Line(text.ToString());
	}

	private static bool AnyWatchedMoved()
	{
		foreach (var (param, start) in _watched)
		{
			if (Attribute(param) != start)
			{
				return true;
			}
		}

		return false;
	}

	private static void Watch(int param)
	{
		foreach (var (watched, _) in _watched)
		{
			if (watched == param)
			{
				return;
			}
		}

		_watched.Add((param, Attribute(param)));
	}

	private static bool TryReadMeal(out ushort param, out float remaining)
	{
		foreach (ref readonly var status in Player.Object.GetStatuses())
		{
			if (status.StatusId == (ushort)StatusID.WellFed && status.RemainingTime > 0f)
			{
				param = status.Param;
				remaining = status.RemainingTime;
				return true;
			}
		}

		param = 0;
		remaining = 0f;
		return false;
	}

	private static unsafe int Attribute(int param)
	{
		var state = PlayerState.Instance();
		return state == null || param < 0 || param >= state->Attributes.Length ? 0 : state->Attributes[param];
	}
}

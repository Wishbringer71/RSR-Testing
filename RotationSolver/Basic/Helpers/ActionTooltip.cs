using Dalamud.Game;
using ECommons.DalamudServices;
using Lumina.Excel.Sheets;
using System.Globalization;
using System.Text.RegularExpressions;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// An action's effect text as its tooltip shows it at the player's synced level: the client's effect text
/// (ActionTransient, English sheet) evaluated by Dalamud's SeString evaluator. The evaluation fills in the figures a
/// level or trait decides, which the raw text - and ActionId.resx made from it - leaves blank ("a potency of .",
/// "Duration: s"). English whatever the client language, so a German client reads the same figures (A316, A342).
/// </summary>
/// <remarks>
/// What the game gives at runtime weighs more than a table fetched beforehand (the owner's rule, 12.10.2026): it shows
/// the patch that is running. Whether the evaluation follows a level sync is not documented; a reader whose answer
/// depends on it says how it guards against a figure of the unsynced level.
/// </remarks>
internal static class ActionTooltip
{
	private static readonly Dictionary<(uint ActionId, int Level), string> _texts = [];
	private static readonly object _gate = new();

	private static readonly Regex StatedPotency = new(@"potency of (?<value>\d[\d,]*)", RegexOptions.Compiled);
	private static readonly HashSet<(uint ActionId, int Level)> _unreadPotency = [];
	private static readonly Regex StatedFalloff = new(@"(?<value>\d{1,2})% less for all remaining enemies", RegexOptions.Compiled);
	private const string DamageAreaText = "enemies who enter";
	private static readonly Regex AdditionalDamageArea = new(@"Additional Effect: Creates [^.]*enemies who enter", RegexOptions.Compiled);
	private static readonly Regex StatedDuration = new(@"Duration: (?<value>\d{1,3})s", RegexOptions.Compiled);
	private static readonly Regex PotencyWord = new(@"[Pp]otenc", RegexOptions.Compiled);
	private static readonly Regex ConditionalWording = new(@"\b(when|while|increase[sd]?|over time|fold|each hit|[Cc]ombo|rear|flank|front|[Bb]onus|[Cc]ure)\b", RegexOptions.Compiled);
	private static readonly Regex StatedFixedDamage = new(@"fixed (?<value>\d[\d,]*) points of", RegexOptions.Compiled);
	private static readonly Regex StatedPercent = new(@"(?<value>\d{1,3})%", RegexOptions.Compiled);
	private static readonly Regex StatedHealthShare = new(@"(?:dealing|equal to) (?<shares>[^.|\n]*?) of (?:their|target's) (?:current )?HP", RegexOptions.Compiled);
	private static readonly Regex StatedDamageUp = new(@"increasing damage dealt by (?<value>\d{1,3})%", RegexOptions.Compiled);
	private const string IgnoresHigherLevelText = "no effect on enemies whose level is higher than your own";
	private static readonly Regex StatedLevelMultiple = new(@"level must be a multiple of (?<value>\d{1,3})", RegexOptions.Compiled);
	private static readonly Regex StatedUndeadPotency = new(@"[Pp]otency increases to (?<value>\d[\d,]*) when used against undead", RegexOptions.Compiled);
	private const string DiminishingText = "Repeated use";

	/// <summary>
	/// The evaluated effect text of <paramref name="actionId"/>, empty when the sheet has no row for it or the
	/// evaluation fails.
	/// </summary>
	public static string Text(uint actionId)
	{
		var level = DataCenter.PlayerSyncedLevel();
		lock (_gate)
		{
			if (_texts.TryGetValue((actionId, level), out var known))
			{
				return known;
			}

			var text = string.Empty;
			try
			{
				var sheet = Svc.Data.GetExcelSheet<ActionTransient>(ClientLanguage.English);
				if (sheet != null && sheet.TryGetRow(actionId, out var row))
				{
					text = Svc.SeStringEvaluator.Evaluate(row.Description, default, ClientLanguage.English).ExtractText();
				}
			}
			catch (Exception)
			{
				// A sheet or an evaluation that fails answers nothing; each reader keeps its own fallback.
				text = string.Empty;
			}

			_texts[(actionId, level)] = text;
			return text;
		}
	}

	/// <summary>
	/// The figure the first match of <paramref name="pattern"/> captures as "value" in the text of
	/// <paramref name="actionId"/>; false when the text states none.
	/// </summary>
	public static bool TryStated(uint actionId, Regex pattern, out float value)
	{
		value = 0f;
		var match = pattern.Match(Text(actionId));
		return match.Success && float.TryParse(match.Groups["value"].Value.Replace(",", string.Empty),
			NumberStyles.Float, CultureInfo.InvariantCulture, out value);
	}

	/// <summary>
	/// The potency one use of <paramref name="actionId"/> deals over <paramref name="targets"/> enemies, as its tooltip
	/// states it: the first enemy's potency, every further one less by the stated falloff ("30% less for all remaining
	/// enemies"), every one alike where the text states none. False when the text states no potency.
	/// </summary>
	/// <remarks>
	/// Only the first stated potency counts: a bonus under a status ("Meisui Bonus: Potency is increased to 550") and a
	/// damage over time are left out, and a reader whose comparison hinges on them weighs them itself.
	/// </remarks>
	public static bool TryPotencyOn(uint actionId, int targets, out float total)
	{
		total = 0f;
		if (targets < 1)
		{
			return false;
		}

		if (!TryStated(actionId, StatedPotency, out var first))
		{
			// Once per action and level: a comparison that falls back for want of a figure is otherwise silent.
			var level = DataCenter.PlayerSyncedLevel();
			bool firstTime;
			lock (_gate)
			{
				firstTime = _unreadPotency.Add((actionId, level));
			}

			if (firstTime)
			{
				DefenseTrace.Line($"Tooltip of action {actionId} at level {level} states no potency - the comparison that asked falls back");
			}

			return false;
		}

		var rest = TryStated(actionId, StatedFalloff, out var falloff) ? first * (1f - (falloff / 100f)) : first;
		total = first + (rest * (targets - 1));
		return true;
	}

	/// <summary>
	/// Whether <paramref name="actionId"/> lays a damage area as its own effect: an area that stays where it is laid and
	/// damages "any enemies who enter" it - Salted Earth, Doton - and not as the additional effect of a hit, as Slipstream's
	/// windstorm (concept 04, "Schadensflächen erst, wo die Gegner bleiben", A350).
	/// </summary>
	public static bool LaysDamageArea(uint actionId)
	{
		var text = Text(actionId);
		return text.Contains(DamageAreaText, StringComparison.Ordinal) && !AdditionalDamageArea.IsMatch(text);
	}

	/// <summary>
	/// Whether <paramref name="actionId"/> leaves a damage area behind, as its own effect or as the additional effect of a
	/// hit - Salted Earth, Doton, Slipstream's windstorm.
	/// </summary>
	public static bool LeavesDamageArea(uint actionId)
	{
		return Text(actionId).Contains(DamageAreaText, StringComparison.Ordinal);
	}

	/// <summary>
	/// The shape before the player that the text of <paramref name="actionId"/> names - "in a cone before you", "in a
	/// straight line before you" - for an action the sheet types as aimed at the caster, as Flamethrower and Apokalypsis
	/// are; null when it names neither (A380).
	/// </summary>
	public static CastType? StatedShapeBeforeYou(uint actionId)
	{
		var text = Text(actionId);
		return text.Contains("in a cone before you", StringComparison.Ordinal) ? CastType.Cone
			: text.Contains("in a straight line before you", StringComparison.Ordinal) ? CastType.StraightLine
			: null;
	}

	/// <summary>
	/// The first "Duration" the text of <paramref name="actionId"/> states, in seconds; false when it states none.
	/// </summary>
	public static bool TryStatedDuration(uint actionId, out float seconds)
	{
		return TryStated(actionId, StatedDuration, out seconds);
	}

	/// <summary>
	/// The second "Duration" the text of <paramref name="actionId"/> states, in seconds: the effect that follows the first one,
	/// as Moon Flute's Waning Nocturne follows its Waxing Nocturne; false when the text states fewer than two.
	/// </summary>
	public static bool TrySecondStatedDuration(uint actionId, out float seconds)
	{
		seconds = 0f;
		var first = StatedDuration.Match(Text(actionId));
		var second = first.Success ? first.NextMatch() : first;
		return second.Success && float.TryParse(second.Groups["value"].Value, NumberStyles.Float,
			CultureInfo.InvariantCulture, out seconds);
	}

	/// <summary>
	/// The potency of <paramref name="actionId"/> when its text states exactly one and no condition on it - no "when",
	/// "increased", damage over time, multiple hits, combo, positional or bonus wording - so that one hit on the main target
	/// deals that potency and nothing else; false otherwise. The measure of damage per potency counts only such hits
	/// (<see cref="OwnHitWatch"/>, A507).
	/// </summary>
	public static bool TryPlainPotency(uint actionId, out float potency)
	{
		potency = 0f;
		var text = Text(actionId);
		return text.Length > 0 && PotencyWord.Matches(text).Count == 1 && !ConditionalWording.IsMatch(text)
			&& TryStated(actionId, StatedPotency, out potency);
	}

	/// <summary>
	/// The fixed damage the text of <paramref name="actionId"/> states ("Deals a fixed 1,000 points of physical damage"),
	/// false when it states none.
	/// </summary>
	public static bool TryFixedDamage(uint actionId, out float damage)
	{
		return TryStated(actionId, StatedFixedDamage, out damage);
	}

	/// <summary>
	/// The share of each enemy's health the text of <paramref name="actionId"/> states it deals, as a fraction: "damage equal
	/// to 30% of their current HP" gives 0.3, and "randomly dealing 50%, 30%, 20%, or 10% of their HP" the mean of the
	/// four. False when the text states no such share.
	/// </summary>
	public static bool TryHealthShare(uint actionId, out float share)
	{
		share = 0f;
		var match = StatedHealthShare.Match(Text(actionId));
		if (!match.Success)
		{
			return false;
		}

		var sum = 0f;
		var count = 0;
		foreach (Match percent in StatedPercent.Matches(match.Groups["shares"].Value))
		{
			if (float.TryParse(percent.Groups["value"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
			{
				sum += value;
				count++;
			}
		}

		if (count == 0)
		{
			return false;
		}

		share = sum / count / 100f;
		return true;
	}

	/// <summary>
	/// The damage increase in percent a buff of <paramref name="actionId"/> states ("increasing damage dealt by 50%"),
	/// false when it states none.
	/// </summary>
	public static bool TryStatedDamageUp(uint actionId, out float percent)
	{
		return TryStated(actionId, StatedDamageUp, out percent);
	}

	/// <summary>
	/// The number an enemy's level must be a multiple of for <paramref name="actionId"/> to affect it ("Enemy level must be a
	/// multiple of 5", Level 5 Death and Level 5 Petrify); false when the text states none.
	/// </summary>
	public static bool TryLevelMultiple(uint actionId, out int multiple)
	{
		multiple = 0;
		if (!TryStated(actionId, StatedLevelMultiple, out var value) || value < 1f)
		{
			return false;
		}

		multiple = (int)value;
		return true;
	}

	/// <summary>
	/// The potency <paramref name="actionId"/> deals against undead enemies, beside its first stated potency ("Potency
	/// increases to 500 when used against undead enemies", Saintly Beam); false when the text states none.
	/// </summary>
	public static bool TryUndeadPotency(uint actionId, out float potency)
	{
		return TryStated(actionId, StatedUndeadPotency, out potency);
	}

	/// <summary>
	/// Whether the text of <paramref name="actionId"/> says its status wears off with repeated use until targets become
	/// immune for a while (Peripheral Synthesis' Lightheaded): a resisted status then says nothing about the enemy's kind.
	/// </summary>
	public static bool StatusDiminishes(uint actionId)
	{
		return Text(actionId).Contains(DiminishingText, StringComparison.Ordinal);
	}

	/// <summary>
	/// Whether the text of <paramref name="actionId"/> says it has no effect on enemies of higher level than the player:
	/// the Blue Mage's class of fixed, percentage and lethal spells (Missile, Launcher, Dimensional Shift, Tail Screw, Doom,
	/// Level 5 Death, Ultravibration).
	/// </summary>
	public static bool IgnoresHigherLevel(uint actionId)
	{
		return Text(actionId).Contains(IgnoresHigherLevelText, StringComparison.Ordinal);
	}

	/// <summary>
	/// Whether <paramref name="area"/> on <paramref name="targets"/> enemies deals more than <paramref name="single"/>
	/// on one, by the potencies their tooltips state (<see cref="TryPotencyOn"/>). True when either cannot be read, so
	/// a caller falls back to what it did without the comparison.
	/// </summary>
	public static bool AreaOutdamagesSingle(uint area, int targets, uint single)
	{
		return CompareAreaToSingle(area, targets, single, tieToArea: false);
	}

	/// <summary>
	/// <see cref="AreaOutdamagesSingle"/> with a tie going to the area: for a choice where the area spreads the same
	/// damage over every enemy it reaches, as a tank's spender does with enmity (A473).
	/// </summary>
	public static bool AreaAtLeastSingle(uint area, int targets, uint single)
	{
		return CompareAreaToSingle(area, targets, single, tieToArea: true);
	}

	private static bool CompareAreaToSingle(uint area, int targets, uint single, bool tieToArea)
	{
		if (!TryPotencyOn(area, targets, out var areaTotal) || !TryPotencyOn(single, 1, out var singleTotal))
		{
			return true;
		}

		return tieToArea ? areaTotal >= singleTotal : areaTotal > singleTotal;
	}
}

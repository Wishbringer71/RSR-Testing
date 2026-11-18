using Dalamud.Game.ClientState.Statuses;
using ECommons.Automation;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
namespace RotationSolver.Engine.Helpers;

public static partial class StatusHelper
{
	/// <summary>Whether <paramref name="battleChara"/> carries a status for which <paramref name="isOfKind"/> holds; the one loop for such
	/// questions, used by tank protection, healing targets, raise, PvP and deep dungeons (concept 24).</summary>
	internal static bool Carries(IBattleChara battleChara, Func<uint, bool> isOfKind)
	{
		foreach (ref readonly var status in battleChara.GetStatuses())
		{
			if (status.StatusId != 0 && isOfKind(status.StatusId))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>The name of a status in the client's language, empty without a row (A864: the sheet's row read safely, as every
	/// other status reader does).</summary>
	internal static string StatusNameOf(uint statusId)
		=> TryGetStatusRow(statusId, out var row) ? row.Name.ExtractText() : string.Empty;

	/// <summary>
	/// Which attacks this status turns back on whoever makes them, by its own text in the English status sheet: spikes,
	/// counters and reflections (concept 03, "Gegner mit Stacheln", A571, A588). "physical" in the text narrows it to
	/// physical attacks, "magic" or "-aspected" to magic ones, "ranged" to attacks from range; otherwise every attack. None
	/// for any other status, and when the sheet cannot be read.
	/// </summary>
	internal static AttackKinds CounteredAttacks(uint statusId)
	{
		return _counteredAttacks.GetOrAdd(statusId, static key =>
		{
			try
			{
				var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(Dalamud.Game.ClientLanguage.English);
				if (sheet == null || !sheet.TryGetRow(key, out var row))
				{
					return AttackKinds.None;
				}

				var text = row.Description.ExtractText();
				if (!HitsAttackers.IsMatch(text) && !(Reflects.IsMatch(text) && ReflectsAttacks.IsMatch(text)))
				{
					return AttackKinds.None;
				}

				var kinds = AttackKinds.None;
				if (text.Contains("physical", StringComparison.OrdinalIgnoreCase))
				{
					kinds |= AttackKinds.Physical;
				}

				if (text.Contains("magic", StringComparison.OrdinalIgnoreCase) || text.Contains("-aspected", StringComparison.OrdinalIgnoreCase))
				{
					kinds |= AttackKinds.Magic;
				}

				if (text.Contains("ranged", StringComparison.OrdinalIgnoreCase))
				{
					kinds |= AttackKinds.Ranged;
				}

				return kinds == AttackKinds.None ? AttackKinds.Any : kinds;
			}
			catch (Exception)
			{
				return AttackKinds.None;
			}
		});
	}

	/// <summary>
	/// Whether the next damage kills the bearer of this status, by its own text in the English status sheet: "Turned into
	/// stone. Taking damage results in instant death." (Stone Curse 437, 1417, 4537, 5058; xivapi, 05.11.2026). Concept 03,
	/// "Tötungsregeln" (A588). A sheet that cannot be read answers no.
	/// </summary>
	internal static bool DiesToAnyHit(uint statusId)
	{
		return _diesToAnyHit.GetOrAdd(statusId, static key =>
		{
			try
			{
				var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(Dalamud.Game.ClientLanguage.English);
				return sheet != null && sheet.TryGetRow(key, out var row)
					&& row.Description.ExtractText().Contains("Taking damage results in instant death", StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception)
			{
				return false;
			}
		});
	}

	// Status lists this part reads; they read no other static at start, so no order binds them to another file (concept 24).
	private static readonly System.Collections.Concurrent.ConcurrentDictionary<uint, AttackKinds> _counteredAttacks = new();

	private static readonly System.Collections.Concurrent.ConcurrentDictionary<uint, bool> _diesToAnyHit = new();

	// Concept 03, "Gegner mit Stacheln" (A571, A588), the English status texts (xivapi, 05.11.2026): spikes "dealing fire
	// damage to attackers", "dealing ice damage to and slowing down attackers"; counters "Countering any physical attacks",
	// "Damage taken is reduced while countering attacks", "Delivering counter damage every time an attack is suffered";
	// reflections "All physical attacks are reflected back at the dealer", "Reflecting magic back on its caster",
	// "Magic attacks are being absorbed and reflected". "Certain death when counter reaches zero" (Doom) is none of them.
	private static readonly System.Text.RegularExpressions.Regex HitsAttackers = new(
		@"damage to (?:and [^.]*?)?attackers|\bcountering\b|\bcounter damage\b|damage back to its source",
		System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant);

	private static readonly System.Text.RegularExpressions.Regex Reflects = new(@"\breflect",
		System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant);

	private static readonly System.Text.RegularExpressions.Regex ReflectsAttacks = new(@"attack|magic|caster|dealer",
		System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant);

	/// <summary>
	/// Every id the game files under the display name Slow, all of them carrying the same effect:
	/// "weaponskill cast time and recast time, spell cast time and recast time, and auto-attack
	/// delay are increased".
	/// <para>
	/// The auto-attack half is what makes this a mitigation and not a caster nuisance. Trash enemies
	/// deal most of their damage by auto-attack, so a slowed pack throttles the incoming stream by
	/// roughly the size of the debuff - the same order as Rampart, and enough to stop a barrier that
	/// only pays off when it is spent from being spent at all.
	/// </para>
	/// <para>
	/// Read as a group, like <see cref="StunStatus"/> and for the same reason: which id a given
	/// action applies is not decidable from the data, and the source does not matter. Arm's Length
	/// is the one a tank brings; Blue Mage and the Occult Crescent phantom jobs bring others.
	/// Slow+ (427, 1568) is the stronger grade of the same effect and belongs here too.
	/// </para>
	/// </summary>
	public static StatusID[] SlowStatus { get; } =
	[
		StatusID.Slow,
		StatusID.Slow_10,
		StatusID.Slow_193,
		StatusID.Slow_427,
		StatusID.Slow_442,
		StatusID.Slow_561,
		StatusID.Slow_1346,
		StatusID.Slow_1509,
		StatusID.Slow_1568,
		StatusID.Slow_2246,
		StatusID.Slow_3464,
		StatusID.Slow_3493,
	];

	/// <summary>
	/// Barriers that pay a reward only when they are absorbed in full, so letting one expire unspent
	/// wastes its cost rather than merely leaving protection unused.
	/// <para>
	/// The Blackest Night is the only one in the game: it grants Dark Arts when its barrier - 25% of
	/// maximum HP over 7s - is consumed completely (action 7393), and nothing at all when it is not.
	/// Every other barrier in <see cref="ShieldStatus"/> is pure protection, where an unspent barrier
	/// is a good outcome. That difference is why this list exists separately instead of asking
	/// whether any shield is running.
	/// </para>
	/// <para>
	/// Read by rules that would otherwise stop the damage stream while such a barrier is up - the
	/// white mage's Holy stun is the case this was written for.
	/// </para>
	/// </summary>
	public static StatusID[] FullAbsorbRewardStatus { get; } =
	[
		StatusID.BlackestNight,
		StatusID.BlackestNight_1308,
	];

	/// <summary>
	/// Stun Resistance, "Immune to stun effects": the permanent immunity of certain enemies (39 is
	/// marked permanent in the game data). It is not the halving of repeated stuns - the game shows
	/// that as no status, and <see cref="StunWatch"/> reads it from the fight instead (A272).
	/// </summary>
	public static StatusID[] StunResistanceStatus { get; } =
	[
		StatusID.StunResistance,
		StatusID.StunResistance_1349
	];
}

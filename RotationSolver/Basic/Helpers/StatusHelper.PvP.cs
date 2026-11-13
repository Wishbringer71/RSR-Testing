namespace RotationSolver.Basic.Helpers;

public static partial class StatusHelper
{
	/// <summary>
	/// Guard in PvP: two rows of the status sheet, both named Guard with the same text ("Damage taken is reduced ..."), 3054
	/// and 3673. Until A756 only 3054 counted, and a target under 3673 took the full burst (concept 25, "PvP: Ziele unter
	/// Guard und Unverwundbarkeit").
	/// </summary>
	public static StatusID[] PvPGuardStatuses { get; } =
	[
		StatusID.Guard,
		StatusID.Guard_3673,
	];

	/// <summary>
	/// The player's own states under which Guard is wasted or cannot go: invulnerable (Hallowed Ground), held above 1 HP
	/// (Undead Redemption), out of reach (Sky High), already holding still under reduced damage (Hardened Scales), "Cannot
	/// execute Guard" (Inner Release, Unguarded), and on a machina ("beneficial effects conferred by actions are
	/// nullified"). Guard holds every action for its 4 s, so a Guard under one of these lost them for nothing (concept 25,
	/// "PvP: Gemeinsame Aktionen", A756).
	/// </summary>
	public static StatusID[] GuardWastedStatuses { get; } =
	[
		StatusID.HallowedGround_1302,
		StatusID.UndeadRedemption,
		StatusID.SkyHigh,
		StatusID.HardenedScales,
		StatusID.InnerRelease_1303,
		StatusID.Unguarded,
		StatusID.Mounted,
	];

	private static readonly System.Collections.Concurrent.ConcurrentDictionary<uint, bool> _nullifiesCrowdControl = new();

	private static readonly System.Text.RegularExpressions.Regex Nullifies = new(@"nullif|immun",
		System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant);

	private static readonly System.Text.RegularExpressions.Regex CrowdControlNamed = new(@"\bStun\b|Purify",
		System.Text.RegularExpressions.RegexOptions.CultureInvariant);

	/// <summary>
	/// Whether the status nullifies the crowd control Purify lifts, read from its English text: Guard and Swift ("All Stun,
	/// Heavy, Bind, Silence ... are nullified"), Resilience ("Nullifying status afflictions that can be removed by Purify"),
	/// Inner Release, Hardened Scales and any later state worded alike (concept 25, "PvP: Wechselwirkungen und Synergien",
	/// A757).
	/// </summary>
	internal static bool NullifiesCrowdControl(uint statusId)
	{
		return _nullifiesCrowdControl.GetOrAdd(statusId, static key =>
		{
			try
			{
				var sheet = ECommons.DalamudServices.Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(Dalamud.Game.ClientLanguage.English);
				if (sheet == null || !sheet.TryGetRow(key, out var row))
				{
					return false;
				}

				var text = row.Description.ExtractText();
				return Nullifies.IsMatch(text) && CrowdControlNamed.IsMatch(text);
			}
			catch (Exception)
			{
				return false;
			}
		});
	}

	/// <summary>Whether the character carries a status that nullifies the crowd control Purify lifts.</summary>
	internal static bool IsCrowdControlImmune(this IBattleChara battleChara)
	{
		foreach (var status in battleChara.StatusList)
		{
			if (status != null && status.StatusId != 0 && NullifiesCrowdControl(status.StatusId))
			{
				return true;
			}
		}

		return false;
	}
}

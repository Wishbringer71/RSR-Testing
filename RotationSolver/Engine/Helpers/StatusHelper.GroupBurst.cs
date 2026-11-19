using ECommons.DalamudServices;

namespace RotationSolver.Engine.Helpers;

public static partial class StatusHelper
{
	// Concept 03, "F · Über den Augenblick hinaus", K3 (A904): the texts of the raid buffs of every job in the English status
	// sheet (xivapi v2, 19.11.2026) - on a member "Damage dealt is increased." (Divination, Brotherhood, Embolden, Technical
	// Finish, Searing Light, Arcane Circle, Radiant Finale, Starry Muse), "Critical hit rate is increased." (Battle Litany),
	// "Direct hit rate is increased." (Battle Voice), "Critical hit rate and direct hit rate are increased." (Devilment); on
	// the enemy "Damage taken is increased." (Dokumori) and "Rate at which critical hits are taken is increased." (Chain
	// Stratagem).
	private static readonly System.Collections.Concurrent.ConcurrentDictionary<uint, bool> _raidBuffOnMember = new();
	private static readonly System.Collections.Concurrent.ConcurrentDictionary<uint, bool> _raidBuffOnEnemy = new();

	/// <summary>
	/// K3 (A904): whether this status raises a member's damage as a raid buff does, by its own text in the English status
	/// sheet - damage dealt, critical hit rate or direct hit rate increased. Own buffs say the same; a raid buff is one that
	/// another member lays. A sheet that cannot be read answers no.
	/// </summary>
	internal static bool RaisesDamageDealt(StatusID statusId)
	{
		return _raidBuffOnMember.GetOrAdd((uint)statusId, static key => TextSaysAny(key, _onMember));
	}

	/// <summary>
	/// K3 (A904): whether this status on an enemy raises the damage it takes - "Damage taken is increased.", "Rate at which
	/// critical hits are taken is increased." - by its own text in the English status sheet.
	/// </summary>
	internal static bool RaisesDamageTaken(StatusID statusId)
	{
		return _raidBuffOnEnemy.GetOrAdd((uint)statusId, static key => TextSaysAny(key, _onEnemy));
	}

	private static readonly string[] _onMember =
		["damage dealt is increased", "critical hit rate is increased", "direct hit rate is increased",
		"critical hit rate and direct hit rate are increased"];

	private static readonly string[] _onEnemy = ["damage taken is increased", "critical hits are taken is increased"];

	// One read of the status's English text against several phrases; TextSays caches one phrase per status.
	private static bool TextSaysAny(uint statusId, string[] phrases)
	{
		try
		{
			var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(Dalamud.Game.ClientLanguage.English);
			if (sheet == null || !sheet.TryGetRow(statusId, out var row))
			{
				return false;
			}

			var text = row.Description.ExtractText();
			foreach (var phrase in phrases)
			{
				if (text.Contains(phrase, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}
		catch (Exception)
		{
			return false;
		}
	}
}

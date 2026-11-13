using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Config;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using RotationSolver.Basic.Configuration;
using RotationSolver.Basic.Rotations.Duties;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using Action = Lumina.Excel.Sheets.Action;
using Buddy = FFXIVClientStructs.FFXIV.Client.Game.UI.Buddy;
using CharacterManager = FFXIVClientStructs.FFXIV.Client.Game.Character.CharacterManager;
using CombatRole = RotationSolver.Basic.Data.CombatRole;

namespace RotationSolver.Basic;

internal static partial class DataCenter
{
	/// <summary>
	/// Whether to heal before the announced area hit lands: it would leave an unprotected member in the critical
	/// class (<see cref="AnnouncedHitDropsUnprotectedBelow"/> at <c>HealthForDyingTanks</c>), and that member has
	/// health a heal can restore. <paramref name="who"/> names the first such member.
	/// </summary>
	/// <remarks>
	/// <para>The owner's second stage: "die aktuelle hp liegt unter dem schadenswert. dann wäre aber eine heilung
	/// sinnvoll bis max maxhp." A heal comes first where the hit reaches the health a member has; concept 08's table
	/// answers a hit below it with cover alone. Asked on the critical class's footing rather than at zero, as the
	/// barrier of the holds is (A159, A283): a member the hit leaves at a few percent is lost to the next tick, so
	/// the safe direction is to heal him first. The member is then missing at least one minus the hit's share minus
	/// that line of his health - forty percent before a 45 % hit at the default line - so the heal lands on health
	/// he lacks. Only a hit near his whole health leaves little to restore.</para>
	///
	/// <para>This was asked at the area heal threshold before. A 45 % raidwide crosses 65 % from full health, so
	/// every area heal went out before it on a full party and healed nobody, direct heals included (A292).</para>
	///
	/// <para>A member at full health gains nothing from a heal before the hit. Barrier and mitigation are the
	/// answer for him, and the barrier of the holds opens them for the same hit.</para>
	/// </remarks>
	internal static bool AnnouncedHitCallsForHealFirst(out string who)
	{
		return AnnouncedHitDropsBelow(Service.Config.HealthForDyingTanks, true, true, out who);
	}

	/// <summary>
	/// How many party members the measured announced area hit takes to zero by the figures - the forecast
	/// effective health against its share, the invulnerable left out (the critical class's footing) - and
	/// whether a healer is among them.
	/// </summary>
	internal static int AnnouncedHitKills(out bool healer)
	{
		healer = false;
		var share = AnnouncedAreaShare;
		if (share <= 0f)
		{
			return 0;
		}

		var killed = 0;
		var party = PartyMembers;
		for (var i = 0; i < party.Count; i++)
		{
			var member = party[i];
			if (member == null || member.IsDead || member.MaxHp == 0 || !member.IsUnprotectedForHealing())
			{
				continue;
			}

			if (member.GetForecastEffectiveHp() / (float)member.MaxHp - share <= 0f)
			{
				killed++;
				healer |= member.IsJobCategory(JobRole.Healer);
			}
		}

		return killed;
	}

	private static bool AnnouncedHitDropsBelow(float threshold, bool unprotectedOnly, bool needsRoom, out string who)
	{
		return HitDropsBelow(AnnouncedAreaShare, threshold, unprotectedOnly, needsRoom, out who);
	}
}

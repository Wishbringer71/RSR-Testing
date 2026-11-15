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

	private static bool HitDropsBelow(float share, float threshold, bool unprotectedOnly, bool needsRoom, out string who)
	{
		who = string.Empty;
		if (share <= 0f)
		{
			return false;
		}

		var party = PartyMembers;
		for (var i = 0; i < party.Count; i++)
		{
			var member = party[i];
			if (member == null || member.IsDead || member.MaxHp == 0
				|| (unprotectedOnly && !member.IsUnprotectedForHealing())
				|| (needsRoom && member.CurrentHp >= member.MaxHp))
			{
				continue;
			}

			// The unprotected reading is the critical class's own footing (IsInCriticalClass): the
			// forecast effective health, at or below the threshold. The other keeps its original form
			// for its original readers.
			if (unprotectedOnly
				? member.GetForecastEffectiveHp() / (float)member.MaxHp - share <= threshold
				: member.GetEffectiveHp() / (float)member.MaxHp - share < threshold)
			{
				who = member.Name.TextValue;
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Every source that can open the player's defence, as it stands now, for the defence trace.
	/// Reads the same predicates the flags are built from; decides nothing.
	/// </summary>
	internal static string DescribeDefenseSources()
	{
		var parts = new List<string> { $"flags {MergedStatus & (AutoStatus.DefenseArea | AutoStatus.DefenseSingle)}" };
		var player = Player.Object;
		try
		{
			foreach (var vfx in VfxDataQueue)
			{
				if (string.IsNullOrEmpty(vfx.Path))
				{
					continue;
				}

				var kind = IsTankbusterMarkerPath(vfx.Path)
					? IsFalsifiedTankbusterMarker(vfx.Path) ? "tankbuster marker (falsified)" : "tankbuster marker"
					: StartsWithAny(vfx.Path, MultiHitSharedPaths) || StartsWithAny(vfx.Path, SharedDamagePaths) ? "stack marker"
					: StartsWithAny(vfx.Path, SpreadDamagePaths) ? "spread marker"
					: null;
				if (kind == null)
				{
					continue;
				}

				var marked = Svc.Objects.SearchById(vfx.ObjectId) as IBattleChara;
				var onPlayer = player != null && vfx.ObjectId == player.GameObjectId;
				parts.Add($"{kind} {vfx.Path} on {(onPlayer ? "you" : marked?.Name.TextValue ?? "?")}"
					+ (onPlayer || marked == null || player == null ? string.Empty : $" {HorizontalDistance(player.Position, marked.Position):F1} y from you"));
			}

			var sheet = Service.GetSheet<Action>();
			var isTank = Role == JobRole.Tank;
			foreach (var h in AreaCasters)
			{
				if (h == null || !h.IsValid() || !h.IsCasting)
				{
					continue;
				}

				var id = h.CastActionId;
				var listedArea = OtherConfiguration.HostileCastingArea.Contains(id);
				var listedTank = OtherConfiguration.HostileCastingTank.Contains(id);

				// A tank also reads an unlisted cast an enemy aims at the one it is attacking as a
				// tankbuster; that source opened his single-target defence without showing here.
				if (!listedArea && !listedTank)
				{
					if (isTank && IsHostileCastingTank(h))
					{
						var aimed = Svc.Objects.SearchById(h.CastTargetObjectId);
						parts.Add($"cast {sheet?.GetRowOrDefault(id)?.Name.ExtractText() ?? "?"} #{id} by {h.Name.TextValue} at "
							+ $"{(player != null && h.CastTargetObjectId == player.GameObjectId ? "you" : aimed?.Name.TextValue ?? "?")}"
							+ $" (unlisted, cast at its target) {h.TotalCastTime - h.CurrentCastTime:F1} s left");
					}

					continue;
				}

				var act = sheet?.GetRowOrDefault(id);
				var target = Svc.Objects.SearchById(h.CastTargetObjectId);
				var text = $"cast {act?.Name.ExtractText() ?? "?"} #{id} by {h.Name.TextValue}"
					+ $"{(AreaCastHelperIds.Contains(h.GameObjectId) ? " (untargetable)" : string.Empty)} at {target?.Name.TextValue ?? "?"}"
					+ $" ({(listedArea ? "area list" : string.Empty)}{(listedArea && listedTank ? ", " : string.Empty)}{(listedTank ? "tankbuster list" : string.Empty)})"
					+ $" {h.TotalCastTime - h.CurrentCastTime:F1} s left";
				if (act is { } a)
				{
					text += $", type {a.CastType} range {a.Range} effect {a.EffectRange} width {a.XAxisModifier}"
						+ $", caster {h.DistanceToPlayer():F1} y from you";
					// Only side-effect-free reads here: the worth check and the large-cast check record
					// what they decide, and the trace must not make decisions the flags did not make.
					if (listedArea)
					{
						text += $", reaches you {AreaCastCanReachPlayer(h, a)}, reaches the party {AreaCastReachesParty(h, a)}, interruptible {h.IsCastInterruptible}"
							+ $", rated {(OtherConfiguration.HostileCastingAreaPotential.TryGetValue(id, out var share) ? share.ToString("P0") + " unmitigated" : "no")}"
							+ $", last landing reached you {(AreaCastReachedPlayer.TryGetValue(id, out var reachedLast) ? reachedLast.ToString() : "not seen")}";
					}
				}

				parts.Add(text);
			}
		}
		catch (Exception ex)
		{
			parts.Add($"scan failed: {ex.Message}");
		}

		parts.Add($"area hit reaches you {AreaHitReachesPlayer}");
		parts.Add($"single hit reaches you {SingleHitReachesPlayer}");

		if (TankPullOnPlayer)
		{
			parts.Add($"pull on you: at least {Service.Config.AutoDefenseNumber} enemies within reach attacking you");
		}

		if (UnhealedUnderAttack)
		{
			var due = TankbusterForecast.NextOnPlayerIn;
			parts.Add("no healer alive, an enemy attacks you: mitigation that is back before the next tankbuster"
				+ (float.IsPositiveInfinity(due) ? " (none due)" : $" (due in {due:F1} s)"));
		}

		if (BMRNextRaidwideIn is > 0f and < float.MaxValue)
		{
			parts.Add($"BMR raidwide in {BMRNextRaidwideIn:F1} s (window {Service.Config.BMRRaidwideMitWindow:F1},"
				+ $" {BMRActiveModuleName ?? "no module"} by {BMRNextRaidwideSource}, hits you {BMRRaidwideHitsPlayer?.ToString() ?? "unknown"})");
		}

		if (BMRNextTankbusterIn is > 0f and < float.MaxValue)
		{
			parts.Add($"BMR tankbuster in {BMRNextTankbusterIn:F1} s (window {Service.Config.BMRTankbusterMitWindow:F1}), party tank {PartyTank?.Name.TextValue ?? "none"}"
				+ $", hits you {BMRTankbusterHitsPlayer?.ToString() ?? "unknown"}");
		}

		if (IsTankbusterVfxOnPlayer())
		{
			parts.Add("tankbuster marker on you");
		}

		var party = PartyMembers;
		for (var i = 0; i < party.Count; i++)
		{
			if (party[i] is { } member && TankbusterHelp.AimedHitCallsForDefence(member, out var aimed))
			{
				parts.Add(aimed);
			}
		}

		return string.Join(" | ", parts);
	}
}

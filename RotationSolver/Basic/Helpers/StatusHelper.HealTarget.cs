using Dalamud.Game.ClientState.Statuses;
using ECommons.Automation;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Helpers;

public static partial class StatusHelper
{
	/// <summary>
	/// Concept 07, "Reinigung oder Heilung" (A574): whether this status turns into something worse when it runs out, by its
	/// own text in the English status sheet - a Deep Freeze, a Sleep, a detonation. A status that ends in death answers no
	/// here; <see cref="EndsInDeath"/> holds it. A sheet that cannot be read answers no.
	/// </summary>
	internal static bool TurnsWorseOnExpiry(StatusID statusId)
	{
		return !EndsInDeath(statusId) && _turnsWorse.GetOrAdd((uint)statusId, static key =>
		{
			try
			{
				var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(Dalamud.Game.ClientLanguage.English);
				return sheet != null && sheet.TryGetRow(key, out var row) && WorseOnExpiry.IsMatch(row.Description.ExtractText());
			}
			catch (Exception)
			{
				return false;
			}
		});
	}

	/// <summary>
	/// Whether this status kills its bearer when it runs out, by its own text in the English status sheet: "Certain death
	/// when counter reaches zero." (Doom, six rows), "... decay will result in KO." (Necrosis), "KO is imminent."
	/// (Throttle) - the cleansable statuses that say so (xivapi, 05.11.2026). A cleanse of one of these saves a life;
	/// concept 07, "Reinigung oder Heilung" (A565). A sheet that cannot be read answers no.
	/// </summary>
	internal static bool EndsInDeath(StatusID statusId)
	{
		return _endsInDeath.GetOrAdd((uint)statusId, static key =>
		{
			try
			{
				var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(Dalamud.Game.ClientLanguage.English);
				if (sheet == null || !sheet.TryGetRow(key, out var row))
				{
					return false;
				}

				// "KO" as the game writes it, in capitals; "death" in any case.
				var text = row.Description.ExtractText();
				return text.Contains("KO", StringComparison.Ordinal) || text.Contains("death", StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception)
			{
				return false;
			}
		});
	}

	/// <summary>
	/// Whether any of these statuses deals damage over time, by its own text in the English status sheet
	/// ("Sustaining damage over time.", "Toxins are causing damage over time.", "Sustaining lightning damage over
	/// time."). Read from the game, so every job's damage over time counts, including those no rotation flags
	/// (A295). A sheet that cannot be read answers no.
	/// </summary>
	internal static bool IsDamageOverTime(ReadOnlySpan<StatusID> statusIDs)
	{
		foreach (var id in statusIDs)
		{
			if (TextSays(_damageOverTime, id, "damage over time"))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Whether this status heals over time, by its own text in the English status sheet ("Regenerating HP over time.",
	/// "Self and nearby party members are regenerating HP over time.") - 84 statuses of every job, the healers' and the
	/// tanks' own (xivapi, 27.10.2026; A432). A sheet that cannot be read answers no.
	/// </summary>
	internal static bool IsHealOverTime(StatusID statusId)
	{
		return TextSays(_healOverTime, statusId, "regenerating HP over time");
	}

	/// <summary>
	/// Whether this status heals over time for the heal thresholds: by its own text (<see cref="IsHealOverTime"/>), or
	/// as one of <see cref="AreaHots"/> and <see cref="SingleHots"/> - kept as the fallback for a status whose text says
	/// it another way and for a sheet that cannot be read.
	/// </summary>
	internal static bool IsAnyHealOverTime(StatusID statusId)
	{
		return IsHealOverTime(statusId) || Array.IndexOf(AreaHots, statusId) >= 0 || Array.IndexOf(SingleHots, statusId) >= 0;
	}

	/// <summary>
	/// How long the heal over time running longest on <paramref name="battleChara"/> lasts, from any source - the healer's,
	/// the co-healer's, a tank's own (Aurora), a ground effect's (Kerachole); <see cref="float.MaxValue"/> while an action
	/// that lays one is on its way to it, or for one without an end; 0 when none runs.
	/// </summary>
	/// <remarks>
	/// The lists alone left out Kerachole's and Celestial Opposition's regeneration and the tanks' own, and the heal
	/// thresholds treated a member under them as if nothing ticked (A442). The longest, not the earliest to end: a member
	/// keeps regenerating for as long as one still runs.
	/// </remarks>
	internal static float HealOverTimeLeft(this IBattleChara battleChara)
	{
		if (DataCenter.InEffectTime && DataCenter.ApplyStatus.TryGetValue(battleChara.GameObjectId, out var pending)
			&& IsAnyHealOverTime((StatusID)pending))
		{
			return float.MaxValue;
		}

		if (!TryGetStatusList(battleChara, out var statusList))
		{
			return 0f;
		}

		var longest = 0f;
		for (var i = 0; i < statusList!.Length; i++)
		{
			var status = statusList[i];
			if (status != null && status.StatusId != 0 && IsAnyHealOverTime((StatusID)status.StatusId))
			{
				longest = Math.Max(longest, status.RemainingTime == 0f ? float.MaxValue : status.RemainingTime);
			}
		}

		return longest;
	}

	// Whether the status's own text in the English sheet holds the phrase, once per status and phrase.
	private static bool TextSays(System.Collections.Concurrent.ConcurrentDictionary<uint, bool> known, StatusID statusId, string phrase)
	{
		return known.GetOrAdd((uint)statusId, static (key, phrase) =>
		{
			try
			{
				var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(Dalamud.Game.ClientLanguage.English);
				return sheet != null && sheet.TryGetRow(key, out var row)
					&& row.Description.ExtractText().Contains(phrase, StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception)
			{
				return false;
			}
		}, phrase);
	}

	/// <summary>
	/// Whether <paramref name="battleChara"/> stands under an <see cref="InvulnerabilityStatus"/> that is
	/// still up in <paramref name="seconds"/>. The earliest expiry across the list counts, so a short
	/// second status beside a long one errs towards "not covered".
	/// </summary>
	internal static bool InvulnerableThrough(this IBattleChara battleChara, float seconds)
	{
		return battleChara.HasStatus(false, InvulnerabilityStatus)
			&& !battleChara.WillStatusEnd(seconds, false, InvulnerabilityStatus);
	}

	/// <summary>
	/// Whether <paramref name="battleChara"/> stands under an <see cref="ImperviousStatus"/> that is
	/// still up in <paramref name="seconds"/>.
	/// </summary>
	internal static bool ImperviousThrough(this IBattleChara battleChara, float seconds)
	{
		return battleChara.HasStatus(false, ImperviousStatus)
			&& !battleChara.WillStatusEnd(seconds, false, ImperviousStatus);
	}

	private static StatusID[] Without(StatusID[] list, StatusID[] removed)
	{
		List<StatusID> kept = [];
		foreach (var status in list)
		{
			if (Array.IndexOf(removed, status) < 0)
			{
				kept.Add(status);
			}
		}

		return [.. kept];
	}

	/// <summary>
	/// Whether <paramref name="battleChara"/> is inside a death-triggered window that still has room
	/// for the death to arrive - that is, <see cref="DeathTriggeredStatus"/> is up and not about to
	/// expire.
	/// <para>
	/// The clock has to be read on this list alone. <see cref="CanBeHealed"/> measures the
	/// *earliest* expiry across all of <see cref="NoNeedHealingStatus"/>, so an unrelated short
	/// invulnerability on the same target - a Phantom Oracle's, say - would report the window as
	/// ending while Living Dead still had most of its duration left.
	/// </para>
	/// <para>
	/// Two GCDs of lead time, and they are measured to the *decision*, not to the heal landing:
	/// worst case the current GCD has to run out and a cast has to finish on top of it, which is two
	/// GCDs on its own, so anything shorter lands the heal after the window has already closed and
	/// the bearer is mortal again. Against Living Dead's ten seconds (ActionId.resx, action 3638:
	/// "Living Dead Duration: 10s") that is roughly half the window. The GCD itself is the player's
	/// real one - <see cref="DataCenter.DefaultGCDTotal"/> reads the recast the game reports, so
	/// spell speed shortens both the lead time and the runway it buys.
	/// </para>
	/// <para>
	/// The lead time is suspended while the death is still reachable, see
	/// <see cref="DeathStillLikely"/>. Without that, the runway is bought with the very outcome the
	/// hold exists to produce: a bearer deep enough to fall would be healed back over the line and
	/// Living Dead would lapse unused, having cost the tank his invulnerability for nothing.
	/// </para>
	/// <para>
	/// An instant heal would land immediately and make a shorter lead time safe, but which heal is
	/// about to be cast is a rotation decision that this layer cannot see, let alone enforce.
	/// </para>
	/// </summary>
	public static bool InDeathTriggerWindow(this IBattleChara battleChara)
	{
		return battleChara.HasStatus(false, DeathTriggeredStatus)
			&& (DeathStillLikely(battleChara.GetHealthRatio())
				|| !battleChara.WillStatusEndGCD(DeathTriggerLeadGCDs, 0, false, DeathTriggeredStatus));
	}

	/// <inheritdoc cref="InDeathTriggerWindow(IBattleChara)"/>
	public static bool PlayerInDeathTriggerWindow()
	{
		return PlayerHasStatus(false, DeathTriggeredStatus)
			&& (DeathStillLikely(ObjectHelper.GetPlayerHealthRatio())
				|| !PlayerWillStatusEndGCD(DeathTriggerLeadGCDs, 0, false, DeathTriggeredStatus));
	}

	/// <summary>
	/// Whether a heal from the player would be punished or wasted right now: Scalebound (1495:
	/// "unable to heal wounds via any method save mega potions"), or Shackled Healing (4564: "Use of
	/// HP-restoring actions will inflict Shackles of Penitence on those nearby") while anyone else is
	/// near. The same test the heal dispatch in CustomRotation_Ability and CustomRotation_GCD applies,
	/// including its 21 yalms; here so that a heal asked from outside that dispatch - Lux Solaris,
	/// Rekindle - obeys it too.
	/// </summary>
	internal static bool PlayerHealingPunished()
	{
		return PlayerHasStatus(false, StatusID.Scalebound)
			|| (PlayerHasStatus(false, StatusID.ShackledHealing) && DataCenter.NumberOfPartyMembersInRangeOf(21) != 1);
	}

	/// <summary>How early the death-trigger hold releases; see <see cref="InDeathTriggerWindow"/>.</summary>
	private const uint DeathTriggerLeadGCDs = 2;

	/// <summary>
	/// Concept 07, "Reinigung oder Heilung" (A573): whether this status ends only once its bearer is fully healed, by its
	/// own text in the English status sheet ("Effect dissipates once fully healed": Doom 1769 and 5473, Gradual
	/// Petrification 1628, Accretion 1604; xivapi, 05.11.2026). A sheet that cannot be read answers no.
	/// </summary>
	internal static bool EndsWhenFullyHealed(uint statusId)
	{
		return _endsWhenFullyHealed.GetOrAdd(statusId, static key =>
		{
			try
			{
				var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(Dalamud.Game.ClientLanguage.English);
				return sheet != null && sheet.TryGetRow(key, out var row)
					&& row.Description.ExtractText().Contains("dissipates once fully healed", StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception)
			{
				return false;
			}
		});
	}

	/// <summary>
	/// Whether <paramref name="battleChara"/> has an active shield (see <see cref="ShieldStatus"/>) that will
	/// still be up in <paramref name="horizon"/> seconds. A shield about to expire shouldn't be credited toward
	/// the target's effective health, since it won't be there to absorb the damage that matters.
	/// </summary>
	public static bool HasSurvivingShield(this IBattleChara battleChara, float horizon)
	{
		return battleChara.GetObjectShield() > 0 && !battleChara.WillStatusEnd(horizon, false, ShieldStatus);
	}

	private static ulong _playerOwnedFor;

	private static uint _playerOwnedIn;

	/// <summary>
	/// Whether <paramref name="objectId"/> is the player or an object the player owns or has owned in this area - pet,
	/// summons, companions -, read from the same ids as the "from self" test of the statuses.
	/// </summary>
	internal static bool IsPlayerOrOwned(ulong objectId, ulong playerId)
	{
		if (objectId == playerId)
		{
			return true;
		}

		if (_playerOwnedTick == long.MinValue || Environment.TickCount64 - _playerOwnedTick >= DataCenter.FrameCacheMs
			|| _playerOwnedFor != playerId || _playerOwnedIn != DataCenter.TerritoryID)
		{
			NoteOwnedObjects();
		}

		return _playerOwned.Contains(objectId);
	}
}

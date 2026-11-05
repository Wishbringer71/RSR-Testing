using ECommons.GameHelpers;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// When healing by anyone but the player and his objects last landed on a party member (concept 07, "Einen Heilzauber
/// abbrechen, den andere überflüssig gemacht haben", A545).
/// </summary>
/// <remarks>
/// <para>An action's effect arrives before the health it brings - in his log of 04.11.2026 a Cure's 1.0 s and Storm's
/// Path's 1.5 s before the rise. So a heal is held from its effect until the member's health has risen by it, counted
/// from the lowest health since; that moment is its landing, and a heal reported before a cast began still lands during
/// it. A heal over time arrives as one tick for all of them, without its sources (cactbot LogGuide, line 24): it lands
/// with the tick, and counts as another's while the member carries a heal over time that is not the player's. Natural
/// regeneration - 1 % of maximum HP per server tick in combat, measured in the same log - comes with neither, and the
/// player's own heals and his objects' are never recorded.</para>
///
/// <para>This answers what landed; whether that ends a heal of the player's is the decision's, in
/// <c>CancelCastUpdater</c>.</para>
/// </remarks>
internal static class HealFromOthers
{
	// A heal reported and not yet seen in the health: how much it can add, at most what was missing, and the lowest health
	// since its effect, from which its rise is counted.
	private readonly record struct Held(uint Expected, uint Lowest);

	private static readonly Dictionary<ulong, Held> _held = [];
	private static readonly Dictionary<ulong, DateTime> _landed = [];

	/// <summary>
	/// An action of another reported <paramref name="amount"/> of healing on <paramref name="member"/>. Effect handler,
	/// game thread.
	/// </summary>
	internal static void RecordEffect(IBattleChara member, uint amount)
	{
		var hp = member.CurrentHp;
		var max = member.MaxHp;
		var expected = Math.Min(amount, max > hp ? max - hp : 0u);
		if (expected == 0)
		{
			return;
		}

		var id = member.GameObjectId;
		if (_held.TryGetValue(id, out var held))
		{
			// A second heal before the first showed: both rise from the lowest health, together at most to the maximum.
			var lowest = Math.Min(held.Lowest, hp);
			_held[id] = new Held(Math.Min(held.Expected + expected, max > lowest ? max - lowest : 0u), lowest);
		}
		else
		{
			_held[id] = new Held(expected, hp);
		}
	}

	/// <summary>
	/// A heal-over-time tick on the party member <paramref name="entityId"/> (ActorControl 0x604): the aggregated one with
	/// <paramref name="statusId"/> 0, a ground effect's with its status. It lands now, as another's while he carries such a
	/// heal that is not the player's.
	/// </summary>
	internal static void RecordTick(uint entityId, uint statusId)
	{
		var playerId = Player.Object?.GameObjectId ?? 0;
		if (playerId == 0)
		{
			return;
		}

		foreach (var member in DataCenter.PartyMembers)
		{
			if (member == null || member.EntityId != entityId)
			{
				continue;
			}

			if (CarriesHealOverTimeOfOthers(member, statusId, playerId))
			{
				_landed[member.GameObjectId] = DateTime.Now;
			}

			return;
		}
	}

	private static bool CarriesHealOverTimeOfOthers(IBattleChara member, uint statusId, ulong playerId)
	{
		if (member.StatusList == null)
		{
			return false;
		}

		foreach (var status in member.StatusList)
		{
			if (status == null || status.StatusId == 0)
			{
				continue;
			}

			var matches = statusId != 0
				? status.StatusId == statusId
				: StatusHelper.IsAnyHealOverTime((StatusID)status.StatusId);
			if (matches && !StatusHelper.IsPlayerOrOwned(status.SourceId, playerId))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Follows <paramref name="member"/>'s health: a held heal lands once his health has risen by it since its effect. Called
	/// wherever party health is read (DataCenter.RefinedHP) and by the rule that asks.
	/// </summary>
	internal static void Follow(IBattleChara member)
	{
		if (_held.Count == 0 || !_held.TryGetValue(member.GameObjectId, out var held))
		{
			return;
		}

		var hp = member.CurrentHp;
		if (hp == 0)
		{
			// He fell before it showed: it will not land, and the raise would read as its rise.
			_ = _held.Remove(member.GameObjectId);
			return;
		}

		var lowest = held.Lowest;
		if (HealPrediction.HasLanded(hp, ref lowest, held.Expected))
		{
			_ = _held.Remove(member.GameObjectId);
			_landed[member.GameObjectId] = DateTime.Now;
		}
		else if (lowest != held.Lowest)
		{
			_held[member.GameObjectId] = held with { Lowest = lowest };
		}
	}

	/// <summary>Whether healing by another landed on the member <paramref name="id"/> at or after <paramref name="since"/>.</summary>
	internal static bool LandedSince(ulong id, DateTime since)
	{
		return _landed.TryGetValue(id, out var at) && at >= since;
	}

	/// <summary>Forgets every held and landed heal: at the end of a fight and on a change of area.</summary>
	internal static void Clear()
	{
		_held.Clear();
		_landed.Clear();
	}
}

using ECommons.GameHelpers;
using Action = Lumina.Excel.Sheets.Action;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// An announced hit on the player: what it is, and in how many seconds it lands.
/// </summary>
internal readonly record struct AnnouncedHitOnPlayer(string Name, float LandsIn);

/// <summary>
/// What the raise protection (Transcendent, status 418) is worth right now: which announced hit on the player lands
/// while it lasts, and which lands after it (concept 11, "Nach der eigenen Wiederbelebung", A539).
/// </summary>
/// <remarks>
/// <para>The protection ends with the first action the player begins - a cast at its start, an instant, an item, the mount
/// (his logs of 25.10. to 03.11.2026, 208 windows) - so it is worth something only against hits that land before he
/// acts. In 31 of those windows an enemy action reached the bearer while it lasted, 24 of them announced by a cast of the
/// same action.</para>
///
/// <para>Detection only: no option, no threshold. Whether the protection is spent is the dispatch's decision.</para>
/// </remarks>
internal static class RaiseProtection
{
	/// <summary>Seconds the protection has left; zero when the player does not carry it.</summary>
	internal static float Remaining
	{
		get
		{
			var player = Player.Object;
			return player != null && player.HasStatus(false, StatusID.Transcendent)
				? player.StatusTime(false, StatusID.Transcendent)
				: 0f;
		}
	}

	/// <summary>
	/// The first announced hit that can land on the player within <paramref name="remaining"/> seconds: a cast at him or
	/// reaching him that ends by then, a tankbuster or area marker on him - its moment is not known, so it counts as
	/// landing within -, or a BossModReborn raidwide or tankbuster on him predicted by then. Null when none.
	/// </summary>
	internal static AnnouncedHitOnPlayer? FirstWithin(float remaining)
	{
		if (DataCenter.IsTankbusterVfxOnPlayer())
		{
			return new("a tankbuster marker on you", 0f);
		}

		if (DataCenter.IsAreaVfxReachingPlayer())
		{
			return new("a stack or hit series marker on you", 0f);
		}

		if (FirstCast(remaining, within: true) is { } cast)
		{
			return cast;
		}

		if (Service.Config.UseBmrTimeline)
		{
			if (RaidwideHitsPlayer && DataCenter.BMRNextRaidwideIn <= remaining)
			{
				return new("a BossModReborn raidwide", DataCenter.BMRNextRaidwideIn);
			}

			if (TankbusterHitsPlayer && DataCenter.BMRNextTankbusterIn <= remaining)
			{
				return new("a BossModReborn tankbuster", DataCenter.BMRNextTankbusterIn);
			}
		}

		return null;
	}

	/// <summary>
	/// The first announced hit on the player that lands after <paramref name="remaining"/> seconds: a cast at him or
	/// reaching him that ends later, or a BossModReborn raidwide or tankbuster on him predicted later but within the
	/// window the caller reads for it. Null when none.
	/// </summary>
	internal static AnnouncedHitOnPlayer? FirstAfter(float remaining, float raidwideWindow, float tankbusterWindow)
	{
		if (FirstCast(remaining, within: false) is { } cast)
		{
			return cast;
		}

		if (Service.Config.UseBmrTimeline)
		{
			var raidwide = DataCenter.BMRNextRaidwideIn;
			if (RaidwideHitsPlayer && raidwide > remaining && raidwide <= remaining + raidwideWindow)
			{
				return new("a BossModReborn raidwide", raidwide);
			}

			var tankbuster = DataCenter.BMRNextTankbusterIn;
			if (TankbusterHitsPlayer && tankbuster > remaining && tankbuster <= remaining + tankbusterWindow)
			{
				return new("a BossModReborn tankbuster", tankbuster);
			}
		}

		return null;
	}

	/// <summary>
	/// The first announced hit within <paramref name="remaining"/> seconds the player could only avoid by moving: a stack or hit
	/// series marker on him, or an enemy cast whose shape reaches him, that is not aimed at him and is no listed area cast. A
	/// raidwide or a cast at him is no such hit - no step avoids it. Null when none (concept 03, "Limit Break allein", A630).
	/// </summary>
	/// <remarks>The area list does not tell a raidwide from an area one can leave (concept 13); a listed one counts as unavoidable.</remarks>
	internal static AnnouncedHitOnPlayer? FirstDodgeableWithin(float remaining)
	{
		if (DataCenter.IsAreaVfxReachingPlayer())
		{
			return new("a stack or hit series marker on you", 0f);
		}

		// The shared walk over the casters (A739, concept 24): a static condition with its values as state, no allocation.
		return FirstCastWhere(remaining, static (h, player, left, remaining) => left <= remaining && h.CastTargetObjectId != player
			&& !OtherConfiguration.HostileCastingArea.Contains(h.CastActionId) && DataCenter.CastReachesPlayer(h));
	}

	// Unless the prediction names whom it hits and leaves him out (A273).
	private static bool RaidwideHitsPlayer => DataCenter.BMRRaidwideHitsPlayer ?? true;

	// As everywhere a BossModReborn tankbuster is read for the player (A220).
	private static bool TankbusterHitsPlayer => DataCenter.BMRTankbusterHitsPlayer
		?? (DataCenter.Role == JobRole.Tank || DataCenter.PartyTank == null);

	// A cast of an enemy aimed at the player, or one whose shape reaches him, listed or not. A cast that ends while the
	// protection lasts counts as landing within it, although its effect comes a moment after the bar: acted on, the
	// protection would be gone when it lands, which is the costly side of the doubt.
	private static AnnouncedHitOnPlayer? FirstCast(float remaining, bool within)
	{
		return FirstCastWhere((remaining, within), static (h, player, left, s) => (left <= s.remaining) == s.within
			&& (h.CastTargetObjectId == player || DataCenter.CastReachesPlayer(h)));
	}

	// The first enemy cast `wanted` takes - given the caster, the player's id and the seconds left - named for the trace.
	private static AnnouncedHitOnPlayer? FirstCastWhere<T>(T state, Func<IBattleChara, ulong, float, T, bool> wanted)
	{
		var player = Player.Object;
		var hostiles = DataCenter.AllHostileTargets;
		if (player == null || hostiles == null)
		{
			return null;
		}

		for (var i = 0; i < hostiles.Count; i++)
		{
			var h = hostiles[i];
			if (h == null || !h.IsValid() || !h.IsCasting)
			{
				continue;
			}

			var left = Math.Max(0f, h.TotalCastTime - h.CurrentCastTime);
			if (!wanted(h, player.GameObjectId, left, state))
			{
				continue;
			}

			var action = Service.GetSheet<Action>().GetRowOrDefault(h.CastActionId);
			var name = action.HasValue ? action.Value.Name.ExtractText() : string.Empty;
			return new($"{name} #{h.CastActionId} from {h.Name.TextValue}", left);
		}

		return null;
	}
}

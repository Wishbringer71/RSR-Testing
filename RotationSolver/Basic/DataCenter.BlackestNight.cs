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
	/// The share of the player's maximum HP that enemy actions took within the last <paramref name="seconds"/>, after
	/// the mitigation that stood, from the effect handler's damage records - what a barrier pressed then would have had
	/// to absorb. For The Blackest Night, whose reward needs its barrier spent within its duration (concept 10, A340).
	/// </summary>
	internal static float DamageTakenWithin(float seconds)
	{
		var now = DateTime.Now;
		float taken = 0;
		foreach (var rec in _damages)
		{
			if ((now - rec.ReceiveTime).TotalSeconds < seconds)
			{
				taken += rec.Ratio;
			}
		}

		return taken;
	}

	/// <summary>
	/// When the player last used the action with either id, by the record of his last actions; null when it is not in
	/// the record, which holds the last <c>QUEUECAPACITY</c> actions and starts over after a fight.
	/// </summary>
	internal static DateTime? LastUseOf(uint id, uint adjustedId)
	{
		DateTime? last = null;
		foreach (var rec in _actions)
		{
			if (rec.Action.RowId == id || rec.Action.RowId == adjustedId)
			{
				last = rec.UsedTime;
			}
		}

		return last;
	}

	/// <summary>Whether a fight in this area has shown this marker path not to be followed by a hit.</summary>
	internal static bool IsFalsifiedTankbusterMarker(string path) => MarkerLockHolds(TankbusterMarkerKey(path));

	/// <summary>
	/// The entry of a marker in <see cref="OtherConfiguration.TankbusterMarkerWithoutHit"/>: the area and the path. One
	/// path serves many fights - `tank_lockon02k1` came before five different tankbusters on 23.10.2026 - and a fight in
	/// which it announces no hit on the marked member would otherwise take it from all the others (A338, A340). An entry
	/// of the path alone, from before, matches no area and is passed over: the marker counts as a tankbuster again until
	/// a fight in the area shows otherwise.
	/// </summary>
	internal static string TankbusterMarkerKey(string path) => $"{TerritoryID}:{path.ToLowerInvariant()}";

	public static bool IsHostileCastingAOE => _isHostileCastingAoe.Value;

	public static bool IsHostileCastingToTank => _isHostileCastingToTank.Value;

	public static bool IsHostileCastingStop => _isHostileCastingStop.Value;

	public static bool AreHostilesCastingKnockback => _areHostilesCastingKnockback.Value;

	internal static bool IsAreaVfxReachingPlayer()
	{
		var player = Player.Object;
		if (player == null || VfxDataQueue == null || VfxDataQueue.IsEmpty)
		{
			return false;
		}

		foreach (var vfx in VfxDataQueue)
		{
			if (string.IsNullOrEmpty(vfx.Path))
			{
				continue;
			}

			if ((StartsWithAny(vfx.Path, MultiHitSharedPaths) || StartsWithAny(vfx.Path, SharedDamagePaths))
				&& IsOwnPartyMarker(vfx.ObjectId))
			{
				return true;
			}

			if (vfx.ObjectId == player.GameObjectId && StartsWithAny(vfx.Path, SpreadDamagePaths))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// <see cref="IsHostileCastingAOE"/> for the player's own area defence: under "Skip area defence for
	/// casts that missed you", a listed cast whose last landing left the living player untouched does
	/// not count (A205). Only the defence flag reads this; healing ahead and the threat check keep the
	/// party's view.
	/// </summary>
	public static bool IsHostileCastingAOEForMyDefense =>
		!Service.Config.SkipAreaCastsThatMissedMe
			? IsHostileCastingAOE
			: InCombat && (IsCastingAreaVfx() || (AllHostileTargets != null && IsAnyHostileCastingAreaReachingMe()));

	private static bool IsAnyHostileCastingAreaReachingMe()
	{
		var playerId = Player.Object?.GameObjectId ?? 0;
		var casters = AreaCasters;
		for (var i = 0; i < casters.Count; i++)
		{
			var h = casters[i];
			if (!IsHostileCastingArea(h))
			{
				continue;
			}

			// A cast aimed at the player reaches him by definition, whatever happened last time.
			if (h.CastTargetObjectId == playerId
				|| !AreaCastReachedPlayer.TryGetValue(h.CastActionId, out var reached) || reached)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// An enemy is casting an action whose measured area damage is large, that will reach the player,
	/// and that lands within about one GCD.
	/// </summary>
	/// <remarks>
	/// <para><b>Why this exists beside <see cref="IsHostileCastingAOE"/>.</b> That one routes through
	/// <see cref="IsHostileCastingBase"/>, which drops every INTERRUPTIBLE cast. The reasoning behind
	/// that is sound - an interruptible cast is meant to be interrupted, and mitigating it would spend
	/// a cooldown on something that never lands. It holds only while somebody actually interrupts.
	/// A Summoner has no interrupt; in a four-player dungeon with a tank who does not use Interject,
	/// the cast lands anyway, and nothing answered it. That is the reported picture: "sometimes Radiant
	/// Aegis and Addle go out on area casts, sometimes not".</para>
	///
	/// <para><b>Why it is not a return to what A9 removed.</b> A9 took out a fallback that raised the
	/// defence from the ENEMY COUNT - no evidence of danger at all, and the owner reported it firing
	/// with nothing happening. This asks the opposite: it requires a figure measured from an actual
	/// landing, at or above what the largest barrier in the game absorbs. A cast nobody has been hit
	/// by carries no figure and opens nothing.</para>
	///
	/// <para><b>What makes it possible now and not then.</b> The per-action damage share did not exist
	/// when that fallback was removed, so coarseness had to be handled in the pre-filter. With the
	/// size measured, the filter no longer has to carry that job alone.</para>
	///
	/// <para>Deliberately a separate property rather than a loosening of
	/// <see cref="IsHostileCastingAOE"/>: that one also feeds <c>ObjectHelper.IsUnderThreat</c> and
	/// through it the White Mage's Benediction guard. Widening it would move two paths at once.</para>
	///
	/// <para><b>This answers what is being cast, not what anyone wants done about it</b>, so the
	/// setting that governs the mitigation decision is checked by its reader and not here. Built the
	/// other way round first, and it was wrong twice over: the healing rule reads the same question
	/// and would have been switched off by a setting about mitigating, and the recorded share -
	/// which the healing rule needs - would never have been written while that setting was off. The
	/// same coupling had already been found and removed from AreaCastIsWorthMitigating in the same
	/// session; putting it straight back in a new property makes it a class, not a slip.</para>
	/// </remarks>
	public static bool IsHostileCastingLargeArea
	{
		get
		{
			if (!InCombat)
			{
				return false;
			}

			var targets = AreaCasters;
			if (targets == null)
			{
				return false;
			}

			var actionSheet = Service.GetSheet<Action>();
			if (actionSheet == null)
			{
				return false;
			}

			for (var i = 0; i < targets.Count; i++)
			{
				var h = targets[i];
				try
				{
					if (h == null || !h.IsValid() || h.GameObjectId == 0 || !h.IsCasting || !IsAreaCastEnemy(h))
					{
						continue;
					}

					// The same one-GCD window IsHostileCastingBase uses at its far end, and for the
					// same reason: answer shortly before the hit, not at the start of the cast. It
					// also leaves the interrupt its chance - by this point it has happened or it will
					// not happen at all.
					var remaining = h.TotalCastTime - h.CurrentCastTime;
					if (remaining <= 0f || remaining > GCDTime(1))
					{
						continue;
					}

					if (!OtherConfiguration.HostileCastingArea.Contains(h.CastActionId))
					{
						continue;
					}

					if (!AreaShareNow(h.CastActionId, h, out var share) || share < LargeShieldShare)
					{
						continue;
					}

					var action = actionSheet.GetRowOrDefault(h.CastActionId) ?? default;
					if (action.RowId == 0 || !AreaCastReachesParty(h, action))
					{
						continue;
					}

					// Recorded here as well, and not only in AreaCastIsWorthMitigating, because this
					// path never passes through it. Without this the healing rule would be blind to
					// exactly the casts this property exists to catch: the defence would open and
					// the heal would not - and the standing order is healing before mitigation.
					_announcedAreaShare = share;
					_announcedAreaShareConfirmed = OtherConfiguration.AreaShareConfirmed(h.CastActionId);
					_announcedAreaShareTime = DateTime.Now;
					_lastAnnouncedAreaAction = h.CastActionId;

					return true;
				}
				catch (AccessViolationException ex)
				{
					PluginLog.Warning($"AccessViolation in IsHostileCastingLargeArea: {ex.Message}");
				}
			}

			return false;
		}
	}

	/// <summary>
	/// An area cast is announced that will reach the player - <b>without</b> asking whether it is
	/// worth mitigating. Records its measured share on the way.
	/// </summary>
	/// <remarks>
	/// <para><see cref="IsHostileCastingAOE"/> carries a verdict inside it: <c>AreaCastIsWorthMitigating</c>
	/// drops a cast that leaves everybody above <c>HealthAreaSpell</c>. That is the right question
	/// for spending a cooldown, and the wrong one for every other reader, because it is taken at the
	/// MITIGATION threshold.</para>
	///
	/// <para>The healing rule asks at its own, higher threshold (<c>HealthAreaAbility</c>, 0.75
	/// against 0.65 in the shipped defaults). Reading the mitigation verdict would therefore lose it
	/// exactly the band between the two: a hit that puts the party at 70% is "too small to mitigate"
	/// and would have been too small to heal ahead of as well - although the heal flag itself would
	/// have raised at that health. The rule would have been switched off by a decision that is not
	/// its own.</para>
	///
	/// <para>Same shape as the two couplings already removed in this session - the recorded share
	/// sitting behind <c>SkipMitigationForSmallAreaCasts</c>, and the mitigation setting sitting
	/// inside <see cref="IsHostileCastingLargeArea"/>. Three of them is a class: <b>a recognition
	/// must not contain a decision.</b></para>
	/// </remarks>
	public static bool IsHostileCastingAreaUnrated
	{
		get
		{
			var targets = AreaCasters;
			if (!InCombat || targets == null)
			{
				return false;
			}

			for (var i = 0; i < targets.Count; i++)
			{
				var h = targets[i];
				if (h == null)
				{
					continue;
				}

				if (IsHostileCastingBase(h, act =>
				{
					if (!OtherConfiguration.HostileCastingArea.Contains(act.RowId)
						|| !AreaCastReachesParty(h, act))
					{
						return false;
					}

					RecordAnnouncedAreaShare(act.RowId, h);
					return true;
				}))
				{
					return true;
				}
			}

			return false;
		}
	}

	private static bool IsAnyHostileCastingArea()
	{
		var casters = AreaCasters;
		if (casters == null)
		{
			return false;
		}

		for (var i = 0; i < casters.Count; i++)
		{
			if (IsHostileCastingArea(casters[i]))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsAnyHostileCastingTank()
	{
		// The helpers too (A303): a tankbuster resolved by an untargetable helper is cast at the tank by it. For a
		// helper only a listed one counts - the fallback "casting at its own target" reads a targetable enemy's
		// attack on whoever it fights, which a helper has no part in.
		var casters = AreaCasters;
		if (casters == null)
		{
			return false;
		}

		for (var i = 0; i < casters.Count; i++)
		{
			var h = casters[i];
			if (AreaCastHelperIds.Contains(h.GameObjectId) ? IsHostileCastingListedTank(h) : IsHostileCastingTank(h))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// True if a hostile is casting a tankbuster at the player specifically, whatever their role.
	/// Counts only casts actually known to be tankbusters: an action on the curated
	/// <see cref="OtherConfiguration.HostileCastingTank"/> list, or one that placed a tankbuster
	/// lock-on VFX on the player. The "casting at its current target" fallback inside
	/// <see cref="IsHostileCastingTank"/> is deliberately not used here. That fallback holds for
	/// practically every uninterruptible cast a hostile aims at whoever it is attacking, which for
	/// a tank is a fair guess at a tankbuster but for anyone else is an ordinary trash-mob attack -
	/// and it made every such cast open the whole single-target defensive chain.
	/// </summary>
	public static bool IsHostileCastingTankBusterAtMe =>
		InCombat && Player.Object != null
		&& (IsTankbusterVfxOnPlayer() || (AllHostileTargets != null && IsAnyHostileCastingListedTankBusterAtMe()));

	private static bool IsAnyHostileCastingListedTankBusterAtMe()
	{
		// Over the helpers too (A303).
		var casters = AreaCasters;
		if (casters == null || Player.Object == null)
		{
			return false;
		}

		var playerId = Player.Object.GameObjectId;
		for (var i = 0; i < casters.Count; i++)
		{
			var h = casters[i];
			if (h != null && h.CastTargetObjectId == playerId && IsHostileCastingListedTank(h))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Whether the tank's pull rule opened the single-target defence in this update: enough enemies
	/// within reach on the player and attacking him. Written by the state updater where the rule is
	/// decided, so nothing reads a second copy of it.
	/// </summary>
	public static bool TankPullOnPlayer { get; internal set; }
}

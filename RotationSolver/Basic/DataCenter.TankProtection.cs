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
	/// Whether a tankbuster lock-on VFX that is not on the learned list of markers without a hit sits on
	/// <paramref name="objectId"/>. A marker fixes whom its tankbuster hits, whatever the enmity (concept 09,
	/// "Wann eine Herausforderung den Treffer noch umlenkt").
	/// </summary>
	internal static bool IsTankbusterVfxOn(ulong objectId)
	{
		if (VfxDataQueue == null || VfxDataQueue.IsEmpty)
		{
			return false;
		}

		foreach (var s in VfxDataQueue)
		{
			try
			{
				if (s.ObjectId != objectId || string.IsNullOrEmpty(s.Path))
				{
					continue;
				}

				if (IsTankbusterMarkerPath(s.Path) && !IsFalsifiedTankbusterMarker(s.Path))
				{
					return true;
				}
			}
			catch (AccessViolationException ex)
			{
				PluginLog.Warning($"AccessViolation in IsTankbusterVfxOn while scanning VFX: {ex.Message}");
			}
		}
		return false;
	}

	public static bool IsHostileStop(IBattleChara h)
	{
		return IsHostileCastingStopBase(h, static act => OtherConfiguration.HostileCastingStop.Contains(act.RowId));
	}

	private static bool TryGetCastAction(IBattleChara h, out Action action)
	{
		var row = Service.GetSheet<Action>().GetRowOrDefault(h.CastActionId);
		action = row ?? default;
		return row is { RowId: not 0 };
	}

	public static bool IsCastingVfx(ConcurrentQueue<VfxNewData> vfxData, Func<VfxNewData, bool> isVfx)
	{
		if (vfxData == null || vfxData.IsEmpty)
		{
			return false;
		}

		foreach (var data in vfxData)
		{
			if (isVfx(data))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsCastingMultiHit()
	{
		return IsCastingVfx(VfxDataQueue, s =>
		{
			if (!Player.Available || Player.Object == null)
			{
				return false;
			}

			if (string.IsNullOrEmpty(s.Path))
			{
				return false;
			}

			foreach (var p in MultiHitSharedPaths)
			{
				if (s.Path.StartsWith(p, PathCmp))
				{
					return true;
				}
			}

			return false;
		});
	}

	/// <summary>
	/// Whether a tankbuster marker stands on a member of the party (<see cref="TankbusterTargetIds"/>).
	/// </summary>
	/// <remarks>
	/// For every role, a tank too: his single-target defence also carries his help for the marked member -
	/// Intervention, Nascent Flash, The Blackest Night, Heart of Corundum, Reprisal - and what protects only himself
	/// asks <see cref="SingleHitReachesPlayer"/> (A233). Until A328 a tank counted only a marker on himself, a
	/// condition from the time his single-target defence was his own mitigation alone (upstream a89f402d0), and a
	/// marker on the co-tank opened nothing. A marker on someone outside the party - another alliance group's tank -
	/// hits none of us, as for the area markers (A220).
	/// </remarks>
	public static bool IsCastingTankVfx()
	{
		lock (_tankbusterLock)
		{
			return TankbusterTargetIds.Count > 0;
		}
	}

	/// <summary>
	/// Rebuilds <see cref="TankbusterTargetIds"/>: every living party member a tankbuster marker stands on that is not
	/// on the learned list of markers without a hit. Once per frame, after the party list (TargetUpdater).
	/// </summary>
	/// <remarks>
	/// Recognition only - whom the marker names, for every role. Until A328 the list was filled as a side effect of
	/// <see cref="IsCastingTankVfx"/> under its condition that a tank counts only a marker on himself (upstream
	/// e34a801fe built the list into that answer): a tank's list never held the co-tank, so Heart of Corundum and
	/// Aurora "on tankbuster targets" never reached him. It was also rebuilt only when the state updater asked that
	/// answer - not for a damage dealer, not with single-target defence off, not after the pull rule had opened the
	/// flag - and could name a marker that had gone.
	/// </remarks>
	internal static void UpdateTankbusterTargets()
	{
		lock (_tankbusterLock)
		{
			TankbusterTargetIds.Clear();
			var party = PartyMembers;
			if (VfxDataQueue == null || VfxDataQueue.IsEmpty || party == null || party.Count == 0)
			{
				return;
			}

			foreach (var s in VfxDataQueue)
			{
				try
				{
					// The path first: the learned list is looked up in lower case, a new string per effect.
					if (string.IsNullOrEmpty(s.Path) || !IsTankbusterMarkerPath(s.Path) || IsFalsifiedTankbusterMarker(s.Path))
					{
						continue;
					}

					// Against the party, a handful of members, not the object table, which is searched in full for
					// every id (A325).
					for (var i = 0; i < party.Count; i++)
					{
						var member = party[i];
						if (member.GameObjectId == s.ObjectId)
						{
							if (!member.IsDead && !TankbusterTargetIds.Contains(member.GameObjectId))
							{
								TankbusterTargetIds.Add(member.GameObjectId);
							}

							break;
						}
					}
				}
				catch (AccessViolationException ex)
				{
					PluginLog.Warning($"AccessViolation in UpdateTankbusterTargets while scanning VFX: {ex.Message}");
				}
			}
		}
	}

	/// <summary>
	/// Whether a known tankbuster is aimed at <paramref name="member"/>: a tankbuster marker on him
	/// (<see cref="TankbusterTargetIds"/>), or an enemy casting at him an action on the tankbuster list or one the
	/// tankbuster table has measured; for the player also a BossModReborn tankbuster whose mask names him.
	/// </summary>
	/// <remarks>
	/// Recognition: whom the tankbuster is aimed at, decided by no option and no threshold. A targeted cast keeps
	/// the target it had at its start (concept 09, "Wann eine Herausforderung den Treffer noch umlenkt"); it counts
	/// in the window of every cast-based tankbuster (<see cref="IsHostileCastingBase"/>, the GCD before the last), so
	/// a press made in it reaches the server before the snapshot. Not the unlisted reading that opens a tank's
	/// single-target defence - an enemy casting at the one it attacks: it matches every uninterruptible trash cast at
	/// the tank holding a pull, harmless for his free mitigation, wrong for help that costs MP or gauge (C10,
	/// <c>CustomRotation.TankbusterOnMe</c>). A measured tankbuster is known from its second cast on.
	/// BossModReborn's mask is read for the player only: bit 0 is fixed to him, the other bits would need a mapping of
	/// its party slots to this tree's list, an unverified contract (<see cref="BMRNextDamageHitsPlayer"/>).
	/// </remarks>
	internal static bool IsTankbusterAimedAt(IBattleChara member)
	{
		if (member == null)
		{
			return false;
		}

		var id = member.GameObjectId;
		lock (_tankbusterLock)
		{
			if (TankbusterTargetIds.Contains(id))
			{
				return true;
			}
		}

		var casters = AreaCasters;
		if (casters != null)
		{
			for (var i = 0; i < casters.Count; i++)
			{
				if (IsKnownTankbusterCastAt(casters[i], id))
				{
					return true;
				}
			}
		}

		return Player.Object is { } player && player.GameObjectId == id && BMRTankbusterHitsPlayer == true;
	}

	/// <summary>
	/// Whether every tankbuster aimed at <paramref name="member"/> lands while he stands under Hallowed Ground or
	/// Superbolide: each is a cast whose hit is timed, and the invulnerability lasts past it. False with a marker or a
	/// BossModReborn prediction on him - neither states a landing time here -, and when nothing is aimed at him.
	/// </summary>
	/// <remarks>
	/// For the decision under "hold mitigation under invulnerability" (his proposal of 04.10.2026: a tank the hit
	/// cannot touch gets nothing from a mitigation on him), as the healer's own hold reads it (StateUpdater): the
	/// end of the cast plus one GCD for the hit to arrive.
	/// </remarks>
	internal static bool ImperviousPastAimedTankbuster(IBattleChara member)
	{
		if (member == null)
		{
			return false;
		}

		var id = member.GameObjectId;
		lock (_tankbusterLock)
		{
			if (TankbusterTargetIds.Contains(id))
			{
				return false;
			}
		}

		if (Player.Object is { } player && player.GameObjectId == id && BMRTankbusterHitsPlayer == true)
		{
			return false;
		}

		var latest = -1f;
		var casters = AreaCasters;
		if (casters != null)
		{
			for (var i = 0; i < casters.Count; i++)
			{
				var h = casters[i];
				if (IsKnownTankbusterCastAt(h, id))
				{
					latest = Math.Max(latest, h.TotalCastTime - h.CurrentCastTime + DefaultGCDTotal);
				}
			}
		}

		return latest >= 0f && member.ImperviousThrough(latest);
	}

	internal static bool AimedTankbusterLeaves(IBattleChara member, out uint actionId, out float share, out float left)
	{
		actionId = 0;
		share = 0f;
		left = 0f;
		if (member == null || member.IsDead || member.MaxHp == 0 || !member.IsJobCategory(JobRole.Tank)
			|| (Player.Object is { } player && player.GameObjectId == member.GameObjectId && BMRTankbusterHitsPlayer == true))
		{
			return false;
		}

		var id = member.GameObjectId;
		bool marked;
		lock (_tankbusterLock)
		{
			marked = TankbusterTargetIds.Contains(id);
		}

		IBattleChara? attacker = null;
		var remaining = 0f;
		var hits = 0;
		var casters = AreaCasters;
		if (casters != null)
		{
			for (var i = 0; i < casters.Count; i++)
			{
				var h = casters[i];
				if (h == null || !h.IsValid() || !h.IsCasting
					|| (!OtherConfiguration.HostileCastingTank.Contains(h.CastActionId) && !TankbusterForecast.IsLearned(h.CastActionId))
					|| (h.CastTargetObjectId != id && !(marked && !IsOtherPartyMember(h.CastTargetObjectId, id))))
				{
					continue;
				}

				hits++;
				actionId = h.CastActionId;
				attacker = h;
				remaining = h.TotalCastTime - h.CurrentCastTime;
			}
		}

		if (hits == 0 && marked && TankbusterMarkerWatch.LearnedHitOn(id, out var learned, out var learnedIn, out _) && learnedIn > 0f)
		{
			hits = 1;
			actionId = learned;
			remaining = learnedIn;
		}

		if (hits != 1)
		{
			return false;
		}

		// What mitigates the hit has to stand until the series' last hit (concept 09, "H_S", A454).
		var horizon = remaining + DefaultGCDTotal + TankbusterTable.SeriesSpan(actionId);
		share = TankbusterTable.PredictedShare(actionId, member, attacker, horizon);
		if (share <= 0f)
		{
			return false;
		}

		var maxHp = (float)member.MaxHp;
		left = (member.CurrentHp / maxHp) + TankbusterForecast.BarrierAt(member, horizon, maxHp) - share;
		return true;
	}

	// Whether this object is a party member other than the one with exceptId: a cast aimed at him is his.
	private static bool IsOtherPartyMember(ulong objectId, ulong exceptId)
	{
		if (objectId == 0 || objectId == exceptId)
		{
			return false;
		}

		var party = PartyMembers;
		for (var i = 0; i < party.Count; i++)
		{
			if (party[i] is { } m && m.GameObjectId == objectId)
			{
				return true;
			}
		}

		return false;
	}

	// When help for a member was last held back for a small measured tankbuster, per member and action: one trace line
	// per hit, as the area skip writes one per cast (A334).
	private static readonly ConcurrentDictionary<(ulong, uint), DateTime> _tankbusterHelpHeld = new();

	/// <summary>
	/// Writes once per hit that help for <paramref name="member"/> is held back: the measured tankbuster
	/// <paramref name="actionId"/> takes <paramref name="share"/> and leaves him <paramref name="left"/> of his maximum HP.
	/// </summary>
	internal static void NoteTankbusterHelpHeld(IBattleChara member, uint actionId, float share, float left, float line)
	{
		var key = (member.GameObjectId, actionId);
		var now = DateTime.Now;
		if (_tankbusterHelpHeld.TryGetValue(key, out var last) && (now - last).TotalSeconds <= DefaultGCDTotal)
		{
			_tankbusterHelpHeld[key] = now;
			return;
		}

		_tankbusterHelpHeld[key] = now;

		// A739 (concept 24, "Speicher"): an entry is read only within a GCD; the older ones go when a new hit is written,
		// so the store holds the hits of the last moments, not every enemy and action of the session.
		foreach (var (heldKey, at) in _tankbusterHelpHeld)
		{
			if ((now - at).TotalSeconds > DefaultGCDTotal)
			{
				_ = _tankbusterHelpHeld.TryRemove(heldKey, out _);
			}
		}

		var name = Service.GetSheet<Action>().GetRowOrDefault(actionId)?.Name.ExtractText() ?? "?";
		DefenseTrace.Line($"tankbuster help held: {name} #{actionId} on {member.Name.TextValue} takes {share:P0} of max HP"
			+ $" and leaves {left:P0}, above the single heal line {line:P0} and under a large shield");
	}

	// An enemy - a helper too - casting at the member with this id an action on the tankbuster list or one the
	// tankbuster table has measured, in the window of the cast recognition.
	private static bool IsKnownTankbusterCastAt(IBattleChara? h, ulong id)
	{
		if (h == null || h.CastTargetObjectId != id)
		{
			return false;
		}

		return IsHostileCastingBase(h, act => OtherConfiguration.HostileCastingTank.Contains(act.RowId)
			|| TankbusterForecast.IsLearned(act.RowId));
	}

	/// <summary>
	/// Would a press made now reach the server before a hit BossModReborn predicts <paramref name="hitIn"/> seconds
	/// ahead? Its time is the end of the cast or the moment the action goes off (<c>CastFinishAt</c> in its components,
	/// the transition of the flagged state in its timeline; BossModReborn be8ff77) - the snapshot, where a mitigation
	/// has to stand. A press goes out once the animation lock, an own cast or a block on every ability ends, and takes
	/// the measured round trip to arrive (concept 09, "Spielerweite Sperre und Frist", the deadline of the tankbuster
	/// plan). Before the first measurement only the time until it can go out counts.
	/// </summary>
	internal static bool PressReachesBeforeSnapshot(float hitIn)
	{
		if (hitIn is <= 0f or >= float.MaxValue)
		{
			return false;
		}

		var free = ObjectHelper.GetInstantHealLeadTime();
		if (StatusHelper.PlayerAbilitiesLocked(out var lockedFor))
		{
			free = Math.Max(free, lockedFor);
		}

		return hitIn - Math.Max(0f, TankbusterForecast.RoundTrip) > free;
	}
}

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
	/// Whether a Walking Dead bearer is still carried by his own attacks, and if not, why.
	/// <para>
	/// Owner's rule (concept 09): under Walking Dead the dark knight does not fall below 1 HP from
	/// most attacks, every weaponskill or spell he lands restores HP, and only if the restored total
	/// has not reached his maximum HP when the timer runs out is he KO'd (Living Dead's effect text,
	/// ActionId.resx 3638). So at first the party trusts him to heal himself by fighting and supports
	/// him only lightly, with a HoT. Full support comes once the timer is running out, or once it is
	/// clear he will not make it in the time left - no enemy in reach, or an event ahead during which
	/// he cannot attack.
	/// </para>
	/// <para>
	/// Every condition comes from the game or from the fight, none from a number of its own:
	/// <list type="bullet">
	/// <item>The lead time is the Living Dead hold's, for the same reason: a full heal decided now
	/// must still land before the window closes.</item>
	/// <item>Reach is the game's range for Hard Slash, his basic weaponskill, hitbox to hitbox.</item>
	/// <item>The event is BossModReborn's next downtime. Without a module it reads as none, and an
	/// untargetable phase is seen only once it has begun, through the reach check.</item>
	/// <item>A tank limit break on the party releases too: "most attacks" leaves room for hits that
	/// do take him below 1 HP, and by the owner's reading those are the rare raidwides that need one.</item>
	/// <item>The course is his health since the window was first seen, carried forward over the
	/// time left. Health is net of the damage he takes, so it understates what he has restored and
	/// the release comes early rather than late. For the first GCD there is nothing to measure yet,
	/// and he is trusted, as the rule says.</item>
	/// </list>
	/// </para>
	/// </summary>
	internal static bool WalkingDeadCarriedBySelfHeal(this IBattleChara battleChara, out string why)
	{
		var id = battleChara.GameObjectId;
		if (!battleChara.HasStatus(false, StatusID.WalkingDead))
		{
			_ = WalkingDeadSeen.Remove(id);
			why = "not under Walking Dead";
			return false;
		}

		var now = DateTime.Now;
		var remaining = battleChara.StatusTime(false, StatusID.WalkingDead);
		var ratio = battleChara.GetHealthRatio();

		// A later window reads more time left than the one on record: start over.
		if (!WalkingDeadSeen.TryGetValue(id, out var seen) || remaining > seen.Remaining)
		{
			seen = (now, ratio, remaining);
			WalkingDeadSeen[id] = seen;
		}

		if (battleChara.WillStatusEndGCD(DeathTriggerLeadGCDs, 0, false, StatusID.WalkingDead))
		{
			why = "the timer is running out - full support";
			return false;
		}

		var reach = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetActionRange((uint)ActionID.HardSlashPvE);
		var enemyInReach = false;
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile != null && EdgeDistance(battleChara, hostile) <= reach)
			{
				enemyInReach = true;
				break;
			}
		}

		if (!enemyInReach)
		{
			why = "no enemy in reach of his weaponskills - full support";
			return false;
		}

		if (DataCenter.BMRNextDowntimeIn < remaining)
		{
			why = "a downtime is announced before the timer ends - full support";
			return false;
		}

		// "Most attacks" will not lower him below 1 HP - not all. The owner's reading: the exception
		// is the rare raidwide the party only survives with a tank's limit break (his examples: the
		// Alexander raids, the Warrior of Light trial). Those are few, so an ordinary raidwide must
		// not release the hold - that would undo the trust the rule asks for at every one of them.
		// The mark of the rare one is the limit break itself: once a tank's limit break status is up
		// on the party, the hit it was used for is coming, and at 1 HP he would stand in front of it
		// unprotected. Read from the party's statuses, so it needs no BossModReborn.
		foreach (var member in DataCenter.PartyMembers)
		{
			if (member != null && member.HasStatus(false, TankLimitBreakStatus))
			{
				why = "a tank limit break is up - full support before the hit it was used for";
				return false;
			}
		}

		var span = (float)(now - seen.At).TotalSeconds;
		if (span < DataCenter.DefaultGCDTotal)
		{
			why = "just started, nothing to measure yet - HoT only";
			return true;
		}

		var projected = ratio + ((ratio - seen.Ratio) / span * remaining);
		if (projected < 1f)
		{
			why = $"his own course reaches {projected:P0} by the end - full support";
			return false;
		}

		why = $"carried by his own attacks, course {projected:P0} - HoT only";
		return true;
	}

	/// <summary>When each bearer's Walking Dead was first seen, with his health and the time left then.</summary>
	private static readonly Dictionary<ulong, (DateTime At, float Ratio, float Remaining)> WalkingDeadSeen = [];

	/// <summary>Hitbox-to-hitbox distance on the ground plane, as the game measures action range.</summary>
	private static float EdgeDistance(IBattleChara a, IBattleChara b)
	{
		var dx = a.Position.X - b.Position.X;
		var dz = a.Position.Z - b.Position.Z;
		return MathF.Max(0f, MathF.Sqrt((dx * dx) + (dz * dz)) - a.HitboxRadius - b.HitboxRadius);
	}

	/// <summary>
	/// Is the fall to zero still the likely continuation for a bearer at this health?
	///
	/// The lead time above buys the heal enough runway to land, and it pays for that with the one
	/// thing the hold exists to protect: it can cancel a death that would still have arrived inside
	/// the window. That price is only acceptable where the death was not coming anyway, and the
	/// user's requirement is exactly that - the lead time must never heal the bearer out of a death
	/// he was going to reach.
	///
	/// So the lead time is suspended while the bearer stands at or below
	/// <c>HealthForDyingTanks</c>: that is the tree's own expression of "this tank is about to fall"
	/// - the value at which the tank rotations fire the invulnerability in the first place - and at
	/// that depth under a running stream the zero arrives before the window closes. Above it the
	/// death is not coming, the buff is going to lapse unused, and the heal should be under way
	/// before the bearer is mortal again.
	///
	/// Nothing is lost in the suspended case: once Living Dead does lapse, the hold is gone with it
	/// and the bearer is an ordinary heal target again. What he cannot do is be healed over the
	/// threshold while the trigger is still reachable.
	/// </summary>
	private static bool DeathStillLikely(float healthRatio)
	{
		return Service.Config != null && healthRatio <= Service.Config.HealthForDyingTanks;
	}

	private static HashSet<uint>? _amnesiaIds;

	/// <summary>
	/// Whether the player can use no ability right now, and for how much longer: a status whose row in the
	/// client's status sheet locks actions (a stun, sleep, petrification and some 260 others), or Amnesia, which
	/// blocks every ability without that flag ("Unable to use abilities"; rows 5, 1092 and 4210, recognised by the
	/// generator's names from the English sheet, so the client language does not matter).
	/// <paramref name="rest"/> is the longest remaining time among them, 0 when none names an end. Recognition
	/// only: the readers decide (concept 09, "Spielerweite Sperre und Frist").
	/// </summary>
	internal static bool PlayerAbilitiesLocked(out float rest)
	{
		rest = 0f;
		var list = Player.Object?.StatusList;
		if (list == null)
		{
			return false;
		}

		_amnesiaIds ??= IdsByNamePrefix("Amnesia");
		var locked = false;
		foreach (var status in list)
		{
			if (status == null || status.StatusId == 0)
			{
				continue;
			}

			if (_amnesiaIds.Contains(status.StatusId) || (status.GameData.ValueNullable?.LockActions ?? false))
			{
				locked = true;
				rest = Math.Max(rest, status.RemainingTime);
			}
		}

		return locked;
	}

	private static HashSet<uint>? _vulnerabilityUpIds;

	/// <summary>
	/// The game's "damage taken is increased" debuffs, recognised by the names the generator gives them
	/// from the status sheet (Vulnerability Up, Physical and Magic Vulnerability Up) rather than by a
	/// hand-kept id list. The names are the English sheet names, so the client language does not matter.
	/// </summary>
	internal static bool IsVulnerabilityUp(uint statusId)
	{
		_vulnerabilityUpIds ??= IdsByNamePrefix("VulnerabilityUp", "PhysicalVulnerabilityUp", "MagicVulnerabilityUp");
		return _vulnerabilityUpIds.Contains(statusId);
	}

	private static HashSet<uint> IdsByNamePrefix(params string[] prefixes)
	{
		HashSet<uint> ids = [];
		foreach (var value in Enum.GetValues<StatusID>())
		{
			var name = value.ToString();
			foreach (var prefix in prefixes)
			{
				if (name.StartsWith(prefix, StringComparison.Ordinal))
				{
					_ = ids.Add((uint)value);
					break;
				}
			}
		}

		return ids;
	}

	/// <summary>Whether <paramref name="battleChara"/> carries any vulnerability debuff.</summary>
	internal static bool CarriesVulnerabilityUp(this IBattleChara battleChara)
	{
		return Carries(battleChara, IsVulnerabilityUp);
	}

	/// <summary>
	/// Seconds the longest vulnerability debuff on <paramref name="battleChara"/> still runs: 0 without one,
	/// <see cref="float.MaxValue"/> for one that names no end.
	/// </summary>
	internal static float VulnerabilityUpRemaining(this IBattleChara battleChara)
	{
		var statuses = battleChara.StatusList;
		if (statuses == null)
		{
			return 0f;
		}

		var longest = 0f;
		foreach (var status in statuses)
		{
			if (status == null || status.StatusId == 0 || !IsVulnerabilityUp(status.StatusId))
			{
				continue;
			}

			longest = Math.Max(longest, status.RemainingTime > 0f ? status.RemainingTime : float.MaxValue);
		}

		return longest;
	}

	private static HashSet<uint>? _damageUpIds;

	/// <summary>
	/// The game's "damage dealt is increased" statuses - Damage Up, Physical and Magic Damage Up - by the
	/// generator's names from the status sheet, as <see cref="IsVulnerabilityUp"/> does for the debuffs.
	/// </summary>
	internal static bool IsDamageUp(uint statusId)
	{
		_damageUpIds ??= IdsByNamePrefix("DamageUp", "PhysicalDamageUp", "MagicDamageUp");
		return _damageUpIds.Contains(statusId);
	}

	/// <summary>Whether <paramref name="battleChara"/> carries any damage-up status.</summary>
	internal static bool CarriesDamageUp(this IBattleChara battleChara)
	{
		return Carries(battleChara, IsDamageUp);
	}

	private static bool Carries(IBattleChara battleChara, Func<uint, bool> isOfKind)
	{
		var statuses = battleChara.StatusList;
		if (statuses == null)
		{
			return false;
		}

		foreach (var status in statuses)
		{
			if (status != null && status.StatusId != 0 && isOfKind(status.StatusId))
			{
				return true;
			}
		}

		return false;
	}
}

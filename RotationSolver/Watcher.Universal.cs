using Dalamud.Hooking;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Hooks;
using ECommons.Hooks.ActionEffectTypes;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using RotationSolver.ActionTimeline;
using RotationSolver.Basic.Configuration;
using System.Text.RegularExpressions;

namespace RotationSolver;

public static partial class Watcher
{
	// Concept 03, "Zielart ohne Tank und Heiler" (A541, A582): an enemy's damage on the player, the party members and the
	// player's own objects, and its heals on other enemies, into its danger per second.
	private static void RecordEnemyDanger(ActionEffectSet set, IBattleChara source, IBattleChara player)
	{
		foreach (var effect in set.TargetEffects)
		{
			if (TryGetDamageEntry(effect, out var damage))
			{
				// A582: and on the player's own objects - the beastmaster's familiar takes the hits meant for him under Snarl.
				if (IsOnYourSide(effect.TargetID, player))
				{
					EnemyDanger.RecordHit(source.GameObjectId, FullAmount(damage));
				}

				// K10 shop (A569): what the player takes in the Crucible, physical or magic, for the gear that guards it.
				if (effect.TargetID == player.GameObjectId && DataCenter.IsInCrucible && set.Action is { } action)
				{
					CrucibleBoard.RecordTaken(action.AttackType.RowId, FullAmount(damage));
				}
			}
			else if (effect.GetSpecificTypeEffect(ActionEffectType.Heal, out var heal)
				&& DataCenter.AllHostileTargetIds.Contains(effect.TargetID)
				&& Svc.Objects.SearchById(effect.TargetID) is IBattleChara healed)
			{
				EnemyDanger.RecordHeal(source.GameObjectId, healed, FullAmount(heal));
			}
		}
	}

	// The player, a party member, or an object the player owns.
	private static bool IsOnYourSide(ulong id, IBattleChara player)
	{
		if (StatusHelper.IsPlayerOrOwned(id, player.GameObjectId))
		{
			return true;
		}

		var party = DataCenter.PartyMembers;
		for (var i = 0; i < party.Count; i++)
		{
			if (party[i]?.GameObjectId == id)
			{
				return true;
			}
		}

		return false;
	}

	// Whether the action reached the player, a party member or an object of the player's.
	private static bool ReachedYourSide(ActionEffectSet set, IBattleChara player)
	{
		foreach (var effect in set.TargetEffects)
		{
			if (ReachedTarget(effect) && IsOnYourSide(effect.TargetID, player))
			{
				return true;
			}
		}

		return false;
	}

	private static bool TryGetDamageEntry(TargetEffect effect, out EffectEntry entry)
	{
		var found = false;
		EffectEntry first = default;
		effect.ForEach(e =>
		{
			if (!found && IsDamageEntry(e.type))
			{
				found = true;
				first = e;
			}
		});
		entry = first;
		return found;
	}

	// Whether the action reached this target at all: damage of any kind, or a hit that an
	// invulnerability, an evasion or a resistance turned away. Each of these says the action was aimed
	// at the member and connected; only the absence of all of them says it did not. A tankbuster taken
	// under Hallowed Ground or Holmgang is still a tankbuster.
	private static bool ReachedTarget(TargetEffect effect)
	{
		var reached = false;
		effect.ForEach(e => reached |= IsDamageEntry(e.type) || e.type is ActionEffectType.Invulnerable
			or ActionEffectType.PartialInvulnerable or ActionEffectType.Miss or ActionEffectType.FullResist);
		return reached;
	}

	// Whether a set comes from the enemy side: an enemy on the hostile list, or a figure of the enemy side
	// (ObjectHelper.IsEnemySideFigure) that nobody can target - the helpers that resolve casts, a boss off the
	// field - or whose action reached a party member. A guard is a combatant as well, and is told apart by the
	// second: it never reaches the party. Read as untargetable combatants only, the parts and helpers of other
	// kinds went unread everywhere below (trace of 18.10.2026, A327); without the helper kind, the helpers of trials
	// and raids still did (A334).
	private static bool IsHostileSource(ActionEffectSet set)
	{
		return set.Source is IBattleChara source
			&& (source.IsEnemy()
				|| (source.IsEnemySideFigure() && (!source.IsTargetable || DamagedPartyMembers(set) > 0)));
	}

	// How many targets the action reached, party or not, each once.
	private static int ReachedTargets(ActionEffectSet set)
	{
		_reachedTargets.Clear();
		foreach (var effect in set.TargetEffects)
		{
			if (ReachedTarget(effect))
			{
				_ = _reachedTargets.Add(effect.TargetID);
			}
		}

		return _reachedTargets.Count;
	}

	// How many party members this set reached, for the trace line of an area landing: "missed you" with
	// party members reached says he stood out of it; with none, the set carried no hit at all and the
	// damage came another way (trace of 01.10.2026, A242).
	private static int DamagedPartyMembers(ActionEffectSet set)
	{
		var count = 0;
		var party = DataCenter.PartyMembers;
		foreach (var effect in set.TargetEffects)
		{
			if (!ReachedTarget(effect))
			{
				continue;
			}

			for (var i = 0; i < party.Count; i++)
			{
				if (party[i]?.GameObjectId == effect.TargetID)
				{
					count++;
					break;
				}
			}
		}

		return count;
	}

	// What a set did to stuns, target by target, for StunWatch: a stun put on, a stun the game reports as
	// fully resisted, and damage - an action that stunned before and now only damages has met immunity.
	private static void ObserveStuns(ActionEffectSet set)
	{
		if (set.Action is not { } action || set.TargetEffects.Length == 0)
		{
			return;
		}

		List<StunWatch.Observation>? found = null;
		foreach (var effect in set.TargetEffects)
		{
			bool applied = false, resisted = false, damaged = false;
			effect.ForEach(e =>
			{
				if (e.type == ActionEffectType.ApplyStatusEffectTarget && IsStun(e.value))
				{
					applied = true;
				}
				// ECommons names two effect types for a fully resisted status (52 and 55); WrathCombo's table
				// gives 55 for a resisted Low Blow stun. Either one counts.
				else if (e.type is ActionEffectType.FullResistStatus or ActionEffectType.FullResistStatus2
					&& IsStun(e.value))
				{
					resisted = true;
				}
				else if (IsDamageEntry(e.type))
				{
					damaged = true;
				}
			});

			if (applied || resisted || damaged)
			{
				(found ??= []).Add(new StunWatch.Observation(effect.TargetID, applied, resisted, damaged));
			}
		}

		if (found != null)
		{
			StunWatch.Observe(action.RowId, found);
		}
	}
}

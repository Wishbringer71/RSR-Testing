using ECommons.ExcelServices;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace RotationSolver.Engine.Actions;

public readonly partial struct ActionBasicInfo
{
	/// <summary>
	/// How long a status the action needs has to last from now: until the action goes off and its cast
	/// ends. A GCD is chosen ahead of time (<see cref="DataCenter.CalculatedActionAhead"/>) and goes off
	/// when the GCD rolls, so the time left on it counts; the cast time is the adjusted one, zero under
	/// Swiftcast or Dualcast.
	/// </summary>
	public readonly float NeededStatusMargin => CastTime + (IsRealGCD ? DataCenter.DefaultGCDRemain : 0f);

	/// <summary>
	/// Whether a cast of this action could begin now as far as its cast time decides: the player is not walking without the
	/// movement lock, no danger forbids the spot under it, BossMod's cast limit and a landing penalty allow it. For a rule
	/// that asks before the action itself can be used (the Beastmaster's relief, concept 01, B6).
	/// </summary>
	internal bool CastCouldStartNow => !NeedsCasting(false);

	/// <summary>
	/// Whether BossMod needs the player to move before a cast of this length could end (<c>Hints.MaxCastTime</c>: "maximal
	/// time we can spend casting before we need to move"). It is set from the AI's leeway on its path by BossMod's movement
	/// module, and to zero while a knockback is pending, while casts are held, or under an instants-only cast strategy; a
	/// longer cast would be broken by the move, or, under the movement lock, hold the player past the leeway. While the
	/// player holds a movement key, BossMod's zero only reflects that key, and the movement lock decides as the player set it
	/// (concept 03, "Bewegungssperre beim Wirken und die KI von BossMod"). One trace line per episode.
	/// </summary>
	internal static bool ExceedsBossModCastLimit(float castTime, string name)
	{
		var limit = DataCenter.BMRMaxCastTime;
		if (!BossModCastLimitRefuses(castTime))
		{
			if (limit == float.MaxValue)
			{
				_tracedCastLimit = float.MaxValue;
			}

			return false;
		}

		if (_tracedCastLimit == float.MaxValue)
		{
			_tracedCastLimit = limit;
			DefenseTrace.Line($"BossMod's cast limit of {limit:F2} s holds longer casts; first held: {name}, {castTime:F2} s");
		}

		return true;
	}

	/// <summary>The test of <see cref="ExceedsBossModCastLimit"/>, without its trace line.</summary>
	private static bool BossModCastLimitRefuses(float castTime)
		=> castTime > DataCenter.BMRMaxCastTime && !DataCenter.BMRMoveRequested;

	/// <summary>
	/// Whether no spell of the current rotation can start now for a reason that depends on its cast time: the shortest cast
	/// the job has now is longer than BossMod's cast limit, would still be under way when a status that damages every
	/// action lands (A439), the player walks under the movement lock while danger reaches the spot before it ends (A651), or
	/// a hit a step certainly avoids lands before it ends (A652).
	/// Then no cast can start, as with <see cref="CastStartBlockedNow"/>, and a caster's instant GCD is due (concept 03,
	/// "C6", A650).
	/// </summary>
	internal static bool NoCastFitsNow()
	{
		var limited = DataCenter.BMRMaxCastTime < float.MaxValue && !DataCenter.BMRMoveRequested;
		var landing = ActionPenaltyWatch.LandsOnPlayerIn() < float.MaxValue;
		var walkingLocked = !DataCenter.NoPoslock && DataCenter.IsMoving;
		var certainHit = MovementLock.CertainDodgeableWithin(float.MaxValue);
		var shortest = ShortestCastNow();
		// Concept 03, A880: no spell fits before the player steps off by his measured lead, or in his pause.
		var forecast = DataCenter.NoPoslock && shortest < float.MaxValue && MoveForecast.Refuses(shortest);
		if (!limited && !landing && !walkingLocked && certainHit is null && !forecast)
		{
			return false;
		}

		return shortest < float.MaxValue
			&& ((limited && BossModCastLimitRefuses(shortest)) || (landing && ActionPenaltyWatch.WouldMeetLanding(shortest))
				|| (walkingLocked && MovementLock.SpotInDanger(shortest)) || (certainHit is { } hit && shortest >= hit.LandsIn)
				|| forecast);
	}

	// The game frame ShortestCastNow last answered for, and its answer: CastStartBlocked is read several times a frame.
	private static DateTime _shortestCastFrame;

	// The shortest cast time among the rotation's enabled GCDs at the player's level, as the game adjusts it now (a proc that
	// makes a spell instant takes it out); float.MaxValue when the rotation has none. Once per game frame.
	private static float ShortestCastNow()
	{
		var frame = Svc.Framework.LastUpdate;
		if (frame == _shortestCastFrame)
		{
			return _shortestCast;
		}

		_shortestCastFrame = frame;
		_shortestCast = float.MaxValue;
		var actions = DataCenter.CurrentRotation?.AllBaseActions;
		if (actions == null)
		{
			return _shortestCast;
		}

		var shortest = float.MaxValue;

		foreach (var action in actions)
		{
			if (!action.Info.IsGeneralGCD || !action.EnoughLevel || !action.IsEnabled)
			{
				continue;
			}

			var cast = action.Info.CastTime;
			if (cast > 0f && cast < shortest)
			{
				shortest = cast;
			}
		}

		_shortestCast = shortest;
		return shortest;
	}

	/// <summary>
	/// Whether no spell with a cast time may start now, whatever the spell: no-casting is commanded, a knockback runs, the
	/// player moves without the movement lock, or BossMod's AI moves or cancels. The Swiftcast-type statuses are checked by
	/// the caller (concept 03, "C6").
	/// </summary>
	internal static bool CastStartBlockedNow(bool skipCastingCheck)
		=> DataCenter.SpecialType == SpecialCommandType.NoCasting
			|| (DateTime.Now > DataCenter.KnockbackStart && DateTime.Now < DataCenter.KnockbackFinished)
			|| (DataCenter.NoPoslock && (DataCenter.IsMoving || DataCenter.BMRMoveRequested) && !skipCastingCheck)
			|| DataCenter.BMRForceCancelCast || DataCenter.BMRIsMoving;

	/// <summary>
	/// The part of <see cref="BasicCheck"/> that refused last, as a fixed string <c>BasicCheck:&lt;part&gt;</c>, for the reason
	/// <c>CanUse</c> reports (concept 03, "Bereit, aber nicht genutzt", A855): "BasicCheck" alone covers some twenty checks,
	/// and the trace line of a cooldown that stood ready could not say which one held it. Game thread, like the check.
	/// </summary>
	internal static string BasicRefusal { get; private set; } = "BasicCheck";

	// Set at the start of BasicCheck and by each named refusal.

	private static bool RefuseBasic(string part)
	{
		BasicRefusal = part;
		return false;
	}

	private static readonly System.Collections.Concurrent.ConcurrentDictionary<uint, bool> _ownDamageBuff = new();
	private static readonly System.Collections.Concurrent.ConcurrentDictionary<uint, DateTime> _downtimeHoldTracedUntil = new();

	/// <summary>
	/// Concept 03, "F · Über den Augenblick hinaus", K1 (A914): an own damage buff waits while BossMod's timeline predicts a
	/// downtime inside its window and knows its end, if the wait costs no use before the target dies. Alone at any point of
	/// the window - the whole window after the downtime hits more than a part before it; in a group only within the first
	/// GCD, the safe assumption that the group presses on cooldown until K3 measures it.
	/// </summary>
	private bool HeldForDowntime()
	{
		var downtimeIn = DataCenter.BMRNextDowntimeIn;
		var downtimeEndIn = DataCenter.BMRNextDowntimeEndIn;
		if (downtimeIn == float.MaxValue || downtimeEndIn == float.MaxValue || downtimeEndIn <= downtimeIn || !DataCenter.InCombat
			|| !IsOwnDamageBuff() || !ActionTooltip.TryStatedDuration(AdjustedID, out var window) || downtimeIn >= window)
		{
			return false;
		}

		var alone = DataCenter.PartyMembers.Count <= 1;
		if ((!alone && downtimeIn >= DataCenter.DefaultGCDTotal) || CustomRotation.HoldCostsUse(_action, downtimeEndIn))
		{
			return false;
		}

		var now = DateTime.Now;
		if (!_downtimeHoldTracedUntil.TryGetValue(AdjustedID, out var until) || now > until)
		{
			_downtimeHoldTracedUntil[AdjustedID] = now.AddSeconds(downtimeEndIn);
			DefenseTrace.Line($"downtime hold: {Name} waits - pause in {downtimeIn:F1} s for {downtimeEndIn - downtimeIn:F1} s, "
				+ $"its window is {window:F0} s, {(alone ? "alone" : "in a group")}");
		}

		return true;
	}

	// A status the action gives that raises your damage, critical or direct hit rate, or the damage the target takes - by
	// its English text, as the group's raid buffs are known (K3).
	private bool IsOwnDamageBuff()
	{
		if (_ownDamageBuff.TryGetValue(AdjustedID, out var known))
		{
			return known;
		}

		var setting = _action.Setting;
		var buff = AnyRaises(setting.StatusProvide, false) || AnyRaises(setting.TargetStatusProvide, true);
		_ownDamageBuff[AdjustedID] = buff;
		return buff;

		static bool AnyRaises(StatusID[]? statuses, bool onEnemy)
		{
			if (statuses == null)
			{
				return false;
			}

			foreach (var status in statuses)
			{
				if (onEnemy ? StatusHelper.RaisesDamageTaken(status) : StatusHelper.RaisesDamageDealt(status))
				{
					return true;
				}
			}

			return false;
		}
	}
}

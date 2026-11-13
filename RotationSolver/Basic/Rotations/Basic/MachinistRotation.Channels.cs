using Dalamud.Interface.Colors;
using System.Text.RegularExpressions;

namespace RotationSolver.Basic.Rotations.Basic;

public partial class MachinistRotation
{
	// Concept 14, "Das Ende der Gruppe" (A591): whether the Battery stays from the end of one fight to the start of the next
	// in the same area - stated by no text, measured here: null until the first such pair is seen.
	private static bool? _batteryKept;

	private static bool _batteryWasInCombat;

	private static byte _batteryAtEnd;

	private static uint _batteryArea;

	/// <summary>
	/// Whether the Battery has been seen to stay between two fights in the same area (concept 14, A591). Only then is
	/// holding the Automaton Queen back from a pack about to fall worth it; while it is unknown, or was seen lost, a part of
	/// her damage beats none.
	/// </summary>
	public static bool BatteryKeptBetweenFights => _batteryKept == true;

	/// <inheritdoc/>
	protected override void UpdateServices()
	{
		base.UpdateServices();
		var inCombat = InCombat;
		if (_batteryWasInCombat && !inCombat)
		{
			_batteryAtEnd = Battery;
			_batteryArea = DataCenter.TerritoryID;
		}
		else if (!_batteryWasInCombat && inCombat && _batteryKept == null && _batteryAtEnd > 0
			&& _batteryArea == DataCenter.TerritoryID)
		{
			_batteryKept = Battery >= _batteryAtEnd;
			DefenseTrace.Line($"machinist: Battery {_batteryAtEnd} at the end of the last fight, {Battery} at the start of this one "
				+ $"- it {(_batteryKept == true ? "stays" : "does not stay")} between fights");
		}

		_batteryWasInCombat = inCombat;
	}

	/// <summary>
	/// Whether Flamethrower beats the GCD it replaces and nothing waits on the channel. The Balance, Machinist Basic Guide:
	/// "If gauge is not usable, beats filler on 2+" - the action's AoE count, checked by its targeting, and Hypercharge
	/// neither usable nor coming ready in the channel; "Never beats Hypercharge, Chain Saw/Excavator, Drill/Bio"; "Ensure
	/// that none of your other major cooldowns (Barrel Stabilizer, Wildfire, Chainsaw, and Air Anchor) will become
	/// available for the 11 seconds you plan to channel." The channel lasts
	/// its tooltip's "Duration" from the press; Drill, with its charges, counts once it would fill up, since only then the
	/// channel costs it time. Reassemble, Double Check and Checkmate are not waited for: the guide does not name them, and a
	/// third of a charge lost at the cap weighs less than the channel gains on two enemies. The channel binds the player to his place and facing, so the pack stands
	/// (<see cref="CustomRotation.DamageAreaSettled(IBaseAction, IBattleChara?)"/>), no hostile casts an area attack, and the
	/// enemies live through the channel where their time to kill is known (A380).
	/// </summary>
	/// <param name="wildfireHeld">The rotation keeps Wildfire back here regardless, so a ready Wildfire waits on nothing.</param>
	/// <returns>True when Flamethrower may go out now.</returns>
	protected bool FlamethrowerFits(bool wildfireHeld = false)
	{
		if (!InCombat || IsOverheated || HasExcavatorReady || HasFullMetalMachinist || DataCenter.IsHostileCastingAOE
			|| !ActionTooltip.TryStatedDuration(FlamethrowerPvE.ID, out var duration)
			|| (DataCenter.IsPackTimeToKillKnown && DataCenter.PackTimeToKill < duration) || !DamageAreaSettled(FlamethrowerPvE))
		{
			return false;
		}

		// Hypercharge's own condition - Heat for its cost, or Hypercharged - and its recast within the channel.
		var channel = DataCenter.DefaultGCDRemain + duration;
		return !(HyperchargePvE.Setting.ActionCheck?.Invoke() == true && ComesReady(HyperchargePvE, channel))
			&& !ComesReady(AirAnchorPvE.EnoughLevel ? AirAnchorPvE : HotShotPvE, channel)
			&& !ComesReady(ChainSawPvE, channel)
			&& !(IsBurst && ComesReady(BarrelStabilizerPvE, channel))
			&& !(IsBurst && !wildfireHeld && ComesReady(WildfirePvE, channel))
			&& !(DrillPvE.EnoughLevel && DrillPvE.Cooldown.WillHaveXCharges(DrillPvE.Cooldown.MaxCharges, channel));

		static bool ComesReady(IBaseAction action, float within)
		{
			return action.EnoughLevel && action.Cooldown.WillHaveOneCharge(within);
		}
	}

	/// <summary>
	/// Whether Auto Crossbow on <paramref name="coneTargets"/> enemies deals more than Blazing Shot (Heat Blast below
	/// its level) with what it brings: its own hit, raised by Overheated, and the recast it takes off both Double Check
	/// and Checkmate (Gauss Round and Ricochet below their level), whose hits reach the enemies around their target.
	/// Auto Crossbow takes nothing off them. The Balance (Machinist Basic Guide, 7.3): "Blazing Shot on 1-5, Auto
	/// Crossbow on 6+" while every target stands in their 5 y, on 4+ with one outside, on 2+ with two or more outside.
	/// </summary>
	/// <remarks>
	/// The potencies, the falloff, the recast cut and the Overheated bonus are read from the tooltips at the synced level
	/// (<see cref="ActionTooltip"/>), the recasts from the game, how many enemies the cut actions reach from their own
	/// target selection - so the breakpoint follows the patch and where the enemies stand (concept 14, "Vergleich mit The
	/// Balance"). A figure that cannot be read answers true: Auto Crossbow then goes by its AoE count, as before.
	/// DefenseTrace.log notes once per level what was read.
	/// </remarks>
	/// <param name="coneTargets">The enemies Auto Crossbow would hit.</param>
	/// <returns>True when Auto Crossbow deals more, or the figures are not known.</returns>
	protected bool AutoCrossbowOutdamagesBlazingShot(int coneTargets)
	{
		var shotAction = BlazingShotPvE.EnoughLevel ? BlazingShotPvE : HeatBlastPvE;
		IBaseAction[] cut = DoubleCheckPvE.EnoughLevel ? [DoubleCheckPvE, CheckmatePvE] : [GaussRoundPvE, RicochetPvE];

		var level = DataCenter.PlayerSyncedLevel();
		var trace = level != _crossbowTracedLevel;
		_crossbowTracedLevel = level;

		if (!ActionTooltip.TryPotencyOn(AutoCrossbowPvE.ID, coneTargets, out var crossbow)
			|| !ActionTooltip.TryPotencyOn(shotAction.ID, 1, out var shot)
			|| !ActionTooltip.TryStated(shotAction.ID, StatedRecastCut, out var recastCut))
		{
			if (trace)
			{
				DefenseTrace.Line($"Machinist, level {level}: the potencies of Auto Crossbow or {shotAction.Name} could not be read from the tooltips - Auto Crossbow goes by its AoE count");
			}

			return true;
		}

		_ = ActionTooltip.TryStated(HyperchargePvE.ID, StatedOverheatBonus, out var overheat);
		var withShot = shot + overheat;
		var read = $"Auto Crossbow {crossbow} on {coneTargets}, {shotAction.Name} {shot} +{overheat}, recast cut {recastCut} s";

		foreach (var action in cut)
		{
			if (!action.EnoughLevel)
			{
				continue;
			}

			// How many enemies it reaches: its own target selection, for an action that hits around its target.
			var reached = 1;
			if (action.Info.EffectRange > 0 && action.TargetInfo.FindTarget(true, true, true, default) is { } hit)
			{
				reached = Math.Max(1, hit.AffectedTargets.Length);
			}

			var recast = action.Cooldown.RecastTimeOneChargeRaw;
			if (recast <= 0f || !ActionTooltip.TryPotencyOn(action.ID, reached, out var hits))
			{
				if (trace)
				{
					DefenseTrace.Line($"Machinist, level {level}: {read}; {action.Name} could not be read - Auto Crossbow goes by its AoE count");
				}

				return true;
			}

			withShot += Math.Min(1f, recastCut / recast) * hits;
			read += $", {action.Name} {hits} on {reached} per {recast} s";
		}

		if (trace)
		{
			DefenseTrace.Line($"Machinist, level {level}: {read}");
		}

		return crossbow > withShot;
	}
}

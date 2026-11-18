using System.Globalization;
using System.Text.RegularExpressions;

namespace RotationSolver.Rotations.Limited.Beastmaster;

/// <summary>
/// Concept 01, "Ketten: Rally und Rallying Cheer tragen die Kette weiter" (A886): in a chain, Rally or Rallying Cheer
/// refills the side whose instinctual skill then completes the next combo; outside a chain they wait while their stacks
/// can still grow.
/// </summary>
public sealed partial class BST_Reborn
{
	[RotationConfig(CombatType.PvE, Name = "Keep Rally and Rallying Cheer to carry a combo chain on",
		Tooltip = "In a fight: outside a chain, Rally and Rallying Cheer wait while their stacks can still grow. In a chain, when neither "
			+ "Trick nor an axe could follow in time, the one whose skill can then follow refills, so that skill completes "
			+ "the next combo, which deals more the later it comes in the chain. Off: both refill whenever their TP fit.")]
	public bool KeepRefillsForChains { get; set; } = true;

	/// <summary>
	/// Whether Rally or Rallying Cheer waits outside a chain: the setting is on and a further combo could still add a
	/// stack (<paramref name="stacks"/> below full). With full stacks the next combo's stack would be lost, so it fills now.
	/// </summary>
	private bool RefillWaitsForChain(int stacks)
	{
		return KeepRefillsForChains && stacks < FullInstinctStacks && ChainTimeLeft <= DataCenter.DefaultGCDTotal;
	}

	// The most stacks of Mastered or Natural Instinct the gauge holds (the gauge's three notches each).
	private const int FullInstinctStacks = 3;

	/// <summary>
	/// In a chain that stands longer than a GCD, when neither Trick nor an axe can follow before it ends: Rallying Cheer
	/// when its TP lift the familiar to what Trick needs and Trick is ready in time, else Rally when its TP lift yours to
	/// what an axe needs and the axes are ready in time.
	/// </summary>
	private bool CarryChain(out IAction? act)
	{
		act = null;
		var chain = ChainTimeLeft;
		if (!KeepRefillsForChains || chain <= DataCenter.DefaultGCDTotal)
		{
			return false;
		}

		var before = chain - DataCenter.DefaultGCDTotal;
		var trickInTime = TrickPvE.EnoughLevel && TrickPvE.Cooldown.RecastTimeRemainOneCharge < before;
		var axeInTime = AvalancheAxePvE.Cooldown.RecastTimeRemainOneCharge < before;
		if ((trickInTime && PetTPCount >= TrickTPNeed) || (axeInTime && TPCount >= AxeTPNeed))
		{
			return false;
		}

		if (trickInTime && PetTPCount + RallyingCheerTPRestore >= TrickTPNeed && RallyingCheerPvE.CanUse(out act))
		{
			TraceHorn($"bst chain: Rallying Cheer carries chain {ChainCount} on, {chain:F1} s left | familiar TP {PetTPCount} + {RallyingCheerTPRestore}");
			return true;
		}

		if (axeInTime && TPCount + RallyTPRestore >= AxeTPNeed && RallyPvE.CanUse(out act))
		{
			TraceHorn($"bst chain: Rally carries chain {ChainCount} on, {chain:F1} s left | TP {TPCount} + {RallyTPRestore}");
			return true;
		}

		return false;
	}

	[RotationConfig(CombatType.PvE, Name = "Hold the axes for a finisher on a pack while it is worth more",
		Tooltip = "In a fight: on a pack that lives long enough for your TP to reach 250, the axes wait and Brutal Rage or "
			+ "its kin hits every enemy around your target, when that is worth more than the axes and their combos - "
			+ "weighed with your measured combo damage. Off: an axe goes whenever 100 TP stand.")]
	public bool HoldAxesForFinisher { get; set; } = true;

	// Concept 01, "Die Rotation als Ganzes" (A889): the combos, whose texts state no potency - their worth is measured.
	private static readonly uint[] ComboActions = [44910, 44911, 44912, 44913, 44914, 44915, 44916];
	private static readonly Regex StatedPotency = new(@"potency of (?<value>\d[\d,]*)", RegexOptions.Compiled);

	// The TP the player gains per second in combat, measured from the gauge.
	private int _tpSeen = -1;
	private DateTime _tpSeenAt;
	private double _tpGained;
	private double _tpSeconds;
	private bool _finisherHoldWritten;

	private float TPPerSecond
	{
		get
		{
			var now = DateTime.Now;
			var tp = (int)TPCount;
			if (InCombat && _tpSeen >= 0)
			{
				var seconds = (now - _tpSeenAt).TotalSeconds;
				// A gap longer than a GCD is no sample of the gain (out of combat, another job, a loading screen).
				if (seconds <= DataCenter.DefaultGCDTotal)
				{
					_tpSeconds += seconds;
					if (tp > _tpSeen)
					{
						_tpGained += tp - _tpSeen;
					}
				}
			}

			_tpSeen = tp;
			_tpSeenAt = now;
			return _tpSeconds > 0d && _tpGained > 0d ? (float)(_tpGained / _tpSeconds) : float.NaN;
		}
	}

	/// <summary>
	/// Whether the axes wait for the finisher at a full gauge: from level 50, on a pack whose enemies around the target
	/// the finisher reaches (n), where F·n + K &gt; a·Full + (Full/Need)·K - F the finisher's potency, a an axe's potency
	/// per TP, K a combo's measured worth in potency - and the pack lives longer than the TP need to fill.
	/// </summary>
	private bool HoldsAxesForFinisher()
	{
		var tpRate = TPPerSecond;
		if (!HoldAxesForFinisher || !InCombat || !InstinctualMasteryTrait.EnoughLevel || HostileTarget is not { } target
			|| !DataCenter.IsPackTimeToKillKnown || float.IsNaN(tpRate))
		{
			_finisherHoldWritten = false;
			return false;
		}

		var combo = OwnHitWatch.MeasuredPotency(ComboActions);
		var finisher = PotencyOf(ActionID.BrutalRagePvE);
		var axe = PotencyOf(ActionID.AvalancheAxePvE);
		var full = GaugeFullTP;
		var need = AxeTPNeed;
		if (float.IsNaN(combo) || finisher <= 0f || axe <= 0f || full <= need || need <= 0)
		{
			_finisherHoldWritten = false;
			return false;
		}

		var enemies = EnemiesAround(target, BrutalRagePvE.Info.EffectRange);
		var withFinisher = (finisher * enemies) + combo;
		var withAxes = (axe / need * full) + ((float)full / need * combo);
		var fillIn = (full - TPCount) / tpRate;
		if (withFinisher <= withAxes || fillIn >= DataCenter.PackTimeToKill)
		{
			_finisherHoldWritten = false;
			return false;
		}

		if (!_finisherHoldWritten)
		{
			_finisherHoldWritten = true;
			TraceHorn($"bst finisher: axes held for {enemies} enemies | {withFinisher:F0} against {withAxes:F0} potency, combo {combo:F0}"
				+ $" | TP {TPCount} full in {fillIn:F0} s, pack falls in {DataCenter.PackTimeToKill:F0} s");
		}

		return true;
	}

	// The first potency the effect text states (an axe: at the least TP; a finisher: on each enemy), 0 without one.
	private static float PotencyOf(ActionID action)
	{
		var match = StatedPotency.Match(ActionTooltip.Text((uint)action));
		return match.Success && float.TryParse(match.Groups["value"].Value.Replace(",", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
			? value
			: 0f;
	}

	// Enemies the finisher's circle around the target reaches: the target and every hostile within the radius of it,
	// counted to the hitbox's edge as the game counts an area.
	private static int EnemiesAround(IBattleChara target, float radius)
	{
		var count = 0;
		var hostiles = DataCenter.AllHostileTargets;
		for (int i = 0, n = hostiles.Count; i < n; i++)
		{
			var hostile = hostiles[i];
			var dx = hostile.Position.X - target.Position.X;
			var dz = hostile.Position.Z - target.Position.Z;
			if (hostile.GameObjectId == target.GameObjectId || MathF.Sqrt((dx * dx) + (dz * dz)) - hostile.HitboxRadius <= radius)
			{
				count++;
			}
		}

		return count;
	}
}

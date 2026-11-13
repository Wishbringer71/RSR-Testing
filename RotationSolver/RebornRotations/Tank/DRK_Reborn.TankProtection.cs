using System.ComponentModel;

namespace RotationSolver.RebornRotations.Tank;

public sealed partial class DRK_Reborn : DarkKnightRotation
{
	/// <summary>
	/// Hold a damage mitigation because a barrier is waiting to be spent - group pulls only.
	/// </summary>
	/// <remarks>
	/// The Blackest Night absorbs 25% of maximum HP over 7s and grants Dark Arts only when that
	/// barrier is broken; a barrier that expires unbroken has cost 3000 MP and returned nothing but
	/// the absorption. What decides it is the size of the incoming stream, so every mitigation cast
	/// while the barrier stands works against it: Reprisal takes 10% off every enemy's damage,
	/// Arm's Length slows every physical attacker by 20% for 15s. The user's instruction is to hold
	/// both while the barrier is up.
	///
	/// <para>
	/// <b>Group pulls only, by the user's own qualification - in a boss fight the rule does not
	/// apply.</b> There the incoming damage arrives as scripted single hits rather than as a stream
	/// of auto-attacks: a tankbuster breaks the barrier on its own whatever Reprisal does, so
	/// holding the mitigation would give up real damage reduction for a reward that is not at risk.
	/// The pull is recognised by the same hostile count the barrier itself requires
	/// (<see cref="BlackestNightMinHostiles"/>) rather than a second number that could drift away
	/// from it.
	/// </para>
	///
	/// <para>
	/// Healing is deliberately not part of this. A barrier absorbs damage before it reaches HP, so
	/// the bearer's health does not change how fast it is spent - the only coupling runs the other
	/// way, since a bearer who dies first gets no Dark Arts at all.
	/// </para>
	///
	/// <para>
	/// The whole party is asked, not just the player: the barrier can be placed on any member
	/// (<c>"self or target party member"</c>), and Reprisal thins the stream for whoever is being
	/// hit. <c>isFromSelf: false</c> for the same reason - a second dark knight's barrier is just
	/// as real as this one's. The one-GCD lead releases the hold before the barrier lapses, so a
	/// mitigation that can no longer affect the outcome is not held back for nothing.
	/// </para>
	///
	/// <para>
	/// The hold is a Dark Arts question, rung two of concept 09, and yields on the universal layer
	/// when a member is in danger (<see cref="CustomRotation.HoldAreaDefense"/>).
	/// </para>
	/// </remarks>
	private bool HoldMitigationForBarrier(bool area)
	{
		return area
			? HoldAreaDefense(BarrierWaitsToBeSpent(), "Dark Knight: a barrier waits to be spent")
			: HoldSingleDefense(BarrierWaitsToBeSpent(), "Dark Knight: a barrier waits to be spent");
	}

	/// <summary>
	/// Whether Arm's Length is worth spending on the pull rather than kept for a knockback.
	/// </summary>
	/// <remarks>
	/// The action has two effects and the plugin read only one of them: it nullifies knockback, and
	/// it afflicts every physical attacker with Slow +20% for 15s. The slow raises auto-attack delay
	/// as well as cast and recast time, and trash enemies deal most of their damage by auto-attack,
	/// so on a standing pack it throttles the incoming stream by about the size of the debuff - the
	/// same order as Rampart, for nothing but a 120s cooldown.
	///
	/// The obvious objection - it is the job's only knockback protection - does not hold where this
	/// rule fires: knockback is a boss mechanic, and a wall-to-wall pull has none. The hostile-count
	/// condition is what keeps the two apart, so it uses the same threshold as the barrier rather
	/// than a second number that could drift away from it.
	/// </remarks>
	private bool ShouldUseArmsLengthOnPull()
		=> ArmsLengthSlowsPull(UseArmsLengthOnPull, BlackestNightMinHostiles)
			// The slow is the point of casting it here, and it is exactly what a barrier waiting to
			// be spent cannot afford. Only this branch is held: the central anti-knockback uses of
			// Arm's Length stay untouched, because being thrown off a platform is not a damage
			// question.
			&& !HoldMitigationForBarrier(false);

	public enum BlackestNightStrategy : byte
	{
		[Description("Whenever single-target defences open")]
		WheneverDefensesOpen,

		[Description("Tankbuster, or a big pull with no other mitigation running")]
		TankbusterOrHeavyPull,

		[Description("As above, plus below the health threshold")]
		TankbusterHeavyPullOrLowHealth,
	}

	protected override bool HasOwnArmsLengthPullRule => true;
}

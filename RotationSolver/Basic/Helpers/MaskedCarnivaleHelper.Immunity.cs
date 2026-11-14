namespace RotationSolver.Basic.Helpers;

/// <summary>A control effect the Masked Carnivale briefing lists per enemy (the sheet <c>AOZContentBriefingBNpc</c>).</summary>
public enum CarnivaleControl : byte
{
	/// <summary>Slow.</summary>
	Slow,

	/// <summary>Petrification.</summary>
	Petrification,

	/// <summary>Paralysis.</summary>
	Paralysis,

	/// <summary>Interruption of a cast.</summary>
	Interruption,

	/// <summary>Blind.</summary>
	Blind,

	/// <summary>Stun.</summary>
	Stun,

	/// <summary>Sleep.</summary>
	Sleep,

	/// <summary>Bind.</summary>
	Bind,

	/// <summary>Heavy.</summary>
	Heavy,

	/// <summary>Flat damage or instant death (Missile, Tail Screw, Level 5 Death, Doom).</summary>
	FlatOrDeath,
}

public static partial class MaskedCarnivaleHelper
{
	/// <summary>
	/// Whether the briefing lists the enemy as immune to the effect. The sheet's ten fields are named "...Vuln" (EXDSchema)
	/// but are set where the enemy is immune (concept 22, "Die Kontrollfelder bedeuten Immunität", A762): the stage guides
	/// interrupt 17 of 18 bosses whose Interruption field is clear, and of the Forefather, whose field is set, they write
	/// "Burst cannot be interrupted"; Auric's guide names 19 enemies for death, Heavy and Sleep, all matching this reading,
	/// none the literal one. Until A762 RSR read them literally, and its ten Carnivale blocks held back exactly the
	/// enemies an effect works on. False for an enemy the sheet does not list - an add such as the Shabti of stage 15.
	/// </summary>
	public static bool IsImmuneTo(IBattleChara? target, CarnivaleControl control)
	{
		return target != null && MobDataByNameId.TryGetValue(target.NameId, out var data) && ImmuneFlag(data, control);
	}

	// Listed by the briefing and not immune: what the IsVulnerableTo... methods have meant to answer.
	private static bool IsListedAndNotImmune(IBattleChara? target, CarnivaleControl control)
	{
		return target != null && MobDataByNameId.TryGetValue(target.NameId, out var data) && !ImmuneFlag(data, control);
	}

	private static bool ImmuneFlag(MobData data, CarnivaleControl control)
	{
		return control switch
		{
			CarnivaleControl.Slow => data.SlowVuln,
			CarnivaleControl.Petrification => data.PetrificationVuln,
			CarnivaleControl.Paralysis => data.ParalysisVuln,
			CarnivaleControl.Interruption => data.InterruptionVuln,
			CarnivaleControl.Blind => data.BlindVuln,
			CarnivaleControl.Stun => data.StunVuln,
			CarnivaleControl.Sleep => data.SleepVuln,
			CarnivaleControl.Bind => data.BindVuln,
			CarnivaleControl.Heavy => data.HeavyVuln,
			CarnivaleControl.FlatOrDeath => data.FlatOrDeathVuln,
			_ => false,
		};
	}
}

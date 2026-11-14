namespace RotationSolver.Basic.Configuration;

internal partial class OtherConfiguration
{
	/// <summary>K7 of the Crucible (concept 21, "Leveln", A799): writes the familiars' true ranks.</summary>
	internal static Task SaveCrucibleFamiliarRank() => SaveList(nameof(CrucibleFamiliarRank));
}

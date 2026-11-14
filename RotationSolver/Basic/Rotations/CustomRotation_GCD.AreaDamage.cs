using ECommons.ExcelServices;
using ECommons.Logging;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Rotations;

public partial class CustomRotation
{
	/// <summary>
	/// Whether a try on a sleep lock taken over from an earlier game version may take this GCD (concept 13, rule 6): no heal
	/// spell is due and nobody stands in the critical class. The sleep comes before the heals in the GCD path, and a try on a
	/// kind that most likely still resists must not cost a heal - for healers and casters alike, both heal with the GCD.
	/// </summary>
	/// <param name="busy">What the GCD is needed for, when it is.</param>
	/// <returns>True when the try may go.</returns>
	private static bool SleepTryAllowed(out string busy)
	{
		if ((DataCenter.MergedStatus & (AutoStatus.HealSingleSpell | AutoStatus.HealAreaSpell)) != 0)
		{
			busy = "a heal spell is due";
			return false;
		}

		if (Player is { IsDead: false } self && self.IsInCriticalClass())
		{
			busy = "you are in the critical class";
			return false;
		}

		foreach (var member in DataCenter.PartyMembers)
		{
			if (member != null && !member.IsDead && member.IsInCriticalClass())
			{
				busy = "a party member is in the critical class";
				return false;
			}
		}

		busy = string.Empty;
		return true;
	}
}

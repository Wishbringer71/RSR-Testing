using FFXIVClientStructs.FFXIV.Client.Game;

namespace RotationSolver.Engine.Helpers
{
	internal static partial class ActionManagerHelper
	{
		/// <summary>
		/// The length of one ordinary GCD of the player's kind at the player's speed as the game computes it now, whether or
		/// not a GCD is running: a spell (Cure, recast 2.5 s) for healers and casters, whose GCD runs on spell speed, a
		/// weaponskill otherwise (concept 03, "Die eigene Geschwindigkeit", A865). Gear, materia, food while its status lasts
		/// and every haste buff are in the game's figure.
		/// </summary>
		public static float GetDefaultAdjustedRecastTime()
		{
			var reference = DataCenter.Role is JobRole.Healer or JobRole.RangedMagical
				? (uint)ActionID.CurePvE
				: (uint)ActionID.HeatedSplitShotPvE;
			return ActionManager.GetAdjustedRecastTime(ActionType.Action, reference) / 1000f;
		}
	}
}

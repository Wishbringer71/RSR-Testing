namespace RotationSolver.Basic.Rotations.Duties;

/// <summary>
/// The deep dungeons: Palace of the Dead, Heaven-on-High, Eureka Orthos, Pilgrim's Traverse - every territory with the
/// intended use deep dungeon (TerritoryType, intended use 31, xivapi 06.11.2026). Concept 03, "Tiefe Gewölbe:
/// Verwandlung" (A614).
/// </summary>
[DutyTerritory(561, 562, 563, 564, 565, 593, 594, 595, 596, 597, 598, 599, 600, 601, 602, 603, 604, 605, 606, 607,
	770, 771, 772, 773, 774, 775, 782, 783, 784, 785,
	1099, 1100, 1101, 1102, 1103, 1104, 1105, 1106, 1107, 1108,
	1281, 1282, 1283, 1284, 1285, 1286, 1287, 1288, 1289, 1290, 1311, 1333)]
public abstract class DeepDungeonRotation : DutyRotation
{
}

public partial class DutyRotation
{
	// The attacks of the deep dungeon forms (concept 03, "Tiefe Gewölbe: Verwandlung", A614): Manticore, Succubus and
	// Kuribu from the Transformation sheet, Dreadnaught, Bomb and Mudball from BossMod's deep dungeon module. Each stands on
	// no duty slot; the game allows the one of the form the player has.

	static partial void ModifyPummelPvE(ref ActionSetting setting)
	{
		setting.IsTransformationAction = true;
	}

	static partial void ModifyVoidFireIiPvE(ref ActionSetting setting)
	{
		setting.IsTransformationAction = true;
	}

	static partial void ModifyHeavenlyJudgePvE(ref ActionSetting setting)
	{
		setting.IsTransformationAction = true;
	}

	static partial void ModifyRotosmashPvE(ref ActionSetting setting)
	{
		setting.IsTransformationAction = true;
	}

	static partial void ModifyWreckingBallPvE(ref ActionSetting setting)
	{
		setting.IsTransformationAction = true;
	}

	static partial void ModifyBigBurstPvE(ref ActionSetting setting)
	{
		setting.IsTransformationAction = true;
	}

	static partial void ModifyRockyRollPvE(ref ActionSetting setting)
	{
		setting.IsTransformationAction = true;
		// "Quickly dash 25 yalms forward, dealing damage to targets in your path" (effect text): a fixed dash like En
		// Avant, its landing point checked, the distance the action's own reach in the sheet.
		setting.TargetType = TargetType.Move;
		setting.IsFriendly = true;
		setting.SpecialType = SpecialActionType.FixedDistanceMoveForward;
		setting.BackstepDistance = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault((uint)ActionID.RockyRollPvE)?.EffectRange ?? 0;
	}
}

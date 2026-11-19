using Action = Lumina.Excel.Sheets.Action;

namespace RotationSolver.Engine.Actions;

public partial interface IBaseAction : IAction
{
	// The line AutoHealCheck filters at in place of each action's own heal ratio; null keeps the action's. The downtime
	// heal sets its own, "When the lowest party HP is below": it heals the members under the line set for it, and with
	// the action's ratio a line set higher would filter out the very member who opened it (concept 07, "Die
	// Füllheilung", A477).
	internal static float? HealRatioOverride { get; set; }

	// Set while the rotation's countdown path is asked before the first GCD of a pull without a countdown: only an action
	// that states no damage and does not move the player may answer (concept 03, "Den Kampf eröffnen", pull without a countdown, A924).
	internal static bool PullPreparationOnly { get; set; }

	// Set while the heal path asks its GCDs only for a heal that reaches a member in the critical class: any other target
	// is refused (concept 07, "Der Heil-GCD für ein Mitglied der kritischen Klasse", A929).
	internal static List<IBattleChara>? CriticalHealTargets { get; set; }

	// Set while the way out of a GCD stall asks the rotation's damage GCDs without the rotation's own conditions (ActionCheck,
	// RotationCheck); every central check stays (concept 03, "Dynamische Rotation", D2, A930).
	internal static bool IgnoreRotationChecks { get; set; }

	// Set by the defence dispatch while a defence path runs: whether the hit that opened it reaches the
	// player. False rejects the actions that protect only him (concept 13, A233); null outside the paths.
	internal static bool? SelfProtectionHitsMe { get; set; }

	// Set for a whole cycle of the rotation while a measured tankbuster's plan, or an invulnerability that
	// keeps the hit off, makes the player's own defence unneeded: true rejects every rated defence aimed at
	// him outside the heal paths, except the ids in AllowedDefenceOnSelf (concept 09).
	internal static bool HoldDefenceOnSelf { get; set; }

	// The planned mitigations that stay free while HoldDefenceOnSelf holds the rest; null holds all.
	internal static HashSet<uint>? AllowedDefenceOnSelf { get; set; }

	// Set for a cycle while the single defence is open only because no healer is alive and an enemy attacks the
	// player: seconds until the next tankbuster on him is due (positive infinity: none). A rated defence aimed at him
	// goes only once it is back before then (A361). Null otherwise.
	internal static float? KeepDefenceForTankbusterIn { get; set; }

	// Set while the area defence path runs: the hold above then spares what reaches beyond the player.
	internal static bool AreaDefenceRunning { get; set; }

	// Set while a heal path runs: the hold above spares it (a tank Superbolide left at 1 HP needs the heal
	// in Heart of Corundum). Its own flag, because TargetOverride can outlive its path (TODO.md).
	internal static bool HealPathRunning { get; set; }

	// Set only while the Limit Break path of a party without other players runs (concept 03, "Limit Break allein", A628):
	// upstream's block of every Limit Break in the basic check (54fba5714) stays for all other paths.
	internal static bool LimitBreakPathRunning { get; set; }

	// Set while the single defence runs under the player's command: the hold above spares that path, and only
	// that path - he commanded a defence, not every one (concept 09).
	internal static bool CommandedDefenceRunning { get; set; }

	// Whether the path running now heals or defends: the heal paths, the area defence, the single defence.
	internal static bool ProtectivePathRunning => HealPathRunning || AreaDefenceRunning
		|| TargetOverride is TargetType.Heal or TargetType.BeAttacked;
}

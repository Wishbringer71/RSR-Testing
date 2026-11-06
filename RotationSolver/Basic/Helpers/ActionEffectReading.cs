using ECommons.Hooks.ActionEffectTypes;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// How one entry of an action effect is read - its amount, whether it is damage, whether it lands on the source. One place
/// for the protocol bits, read by the effect handler (<c>Watcher</c>) and by the client's list of reported effects
/// (<see cref="ReportedDamage"/>, A617).
/// </summary>
internal static class ActionEffectReading
{
	// Amounts of damage and healing are read through FullAmount, never from .value alone. The entry
	// stores an amount as a 16-bit value; a larger one carries its third byte in the byte ECommons
	// calls mult, and only then is bit 0x40 of the byte it calls flags set ("a lot of damage":
	// cactbot's LogGuide, "Ability Damage" - bytes ABCD, C = 0x40, total = D A B). So .value alone
	// wrapped every hit or heal above 65,535 points - a level 100 raidwide on a tank, a Benediction
	// on one - and the measured shares came out too small, so a big area hit could be rated small.
	// EffectEntry.Damage adds mult without looking at the flag; where the byte means something else
	// that would inflate a small hit for good, because the store only ever raises a value.
	// ActionEffectSet.GetSpecificTypeEffect returns .value too ("Is this value or Damage? IDK about
	// it." in its source), so heals are collected here instead.
	internal static uint FullAmount(EffectEntry entry)
		=> (entry.flags & 0x40) != 0 ? entry.Damage : entry.value;

	// An entry the game marks as landing on the source - bit 0x80 of the byte ECommons calls flags, BossModReborn's
	// ActionEffect.AtSource: the self-heal of an attack (Storm's Path), the damage a spiked enemy returns.
	internal static bool LandsOnSource(EffectEntry entry)
		=> (entry.flags & 0x80) != 0;

	// Damage the game reports as blocked or parried is damage taken all the same, at a reduced amount:
	// effect types 5 and 6 beside 3 (ECommons ActionEffectType). Read as Damage only, a tankbuster a
	// paladin blocked - under Sheltron every one is blocked - counted as no hit: its marker was learned
	// away as "not a tankbuster", a listed area cast as having missed him, and the amount went missing
	// from his damage intake (re-audit of A189 and A205, 29.09.2026).
	internal static bool IsDamage(ActionEffectType type)
		=> type is ActionEffectType.Damage or ActionEffectType.BlockedDamage or ActionEffectType.ParriedDamage;

	/// <summary>
	/// The damage this entry does to the target it stands under: its full amount for damage, blocked and parried damage,
	/// nothing for any other effect or for a part that lands on the source.
	/// </summary>
	internal static uint DamageOnTarget(EffectEntry entry)
		=> IsDamage(entry.type) && !LandsOnSource(entry) ? FullAmount(entry) : 0;
}

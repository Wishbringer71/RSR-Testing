using Dalamud.Game.ClientState.Statuses;
using ECommons.Automation;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
using Lumina.Excel;
using GameStatus = FFXIVClientStructs.FFXIV.Client.Game.Status;
using StatusRow = Lumina.Excel.Sheets.Status;
namespace RotationSolver.Engine.Helpers;

public static partial class StatusHelper
{

	/// <summary>
	/// 
	/// </summary>
	public static readonly Dictionary<uint, StatusID[]> NorthHornWeaknessByNameId = new()
	{
		{ 13876, [StatusID.LightningWeakness] },
		{ 13939, [StatusID.IceWeakness] },
		{ 13940, [StatusID.WindWeakness] },
		{ 13941, [StatusID.FireWeakness] },
		{ 13942, [StatusID.LightningWeakness] },
		{ 14490, [StatusID.WindWeakness] },
		{ 14491, [StatusID.FireWeakness] },
		{ 14503, [StatusID.IceWeakness] },
		{ 14505, [StatusID.LightningWeakness] },
		{ 14508, [StatusID.LightningWeakness] },
		{ 14509, [StatusID.LightningWeakness] },
		{ 14511, [StatusID.WindWeakness] },
		{ 14512, [StatusID.WindWeakness] },
		{ 14517, [StatusID.WindWeakness] },
		{ 14520, [StatusID.FireWeakness] },
		{ 14523, [StatusID.IceWeakness] },
		{ 14714, [StatusID.FireWeakness] },
		{ 14717, [StatusID.FireWeakness, StatusID.WindWeakness] },
		{ 14719, [StatusID.IceWeakness] },
		{ 14720, [StatusID.IceWeakness] },
		{ 14726, [StatusID.FireWeakness] },
		{ 14728, [StatusID.LightningWeakness] },
		{ 14735, [StatusID.FireWeakness] },
		{ 14736, [StatusID.IceWeakness] },
		{ 14738, [StatusID.FireWeakness] },
		{ 14747, [StatusID.FireWeakness] },
		{ 14762, [StatusID.FireWeakness] },
		{ 14764, [StatusID.LightningWeakness, StatusID.WindWeakness] },
		{ 14765, [StatusID.FireWeakness] },
		{ 14767, [StatusID.WindWeakness] },
		{ 14771, [StatusID.FireWeakness] },
		{ 14772, [StatusID.IceWeakness] },
		{ 14774, [StatusID.LightningWeakness] },
		{ 14775, [StatusID.LightningWeakness] },
		{ 14776, [StatusID.FireWeakness] },
		{ 14785, [StatusID.FireWeakness] },
		{ 14787, [StatusID.FireWeakness] },
		{ 14789, [StatusID.FireWeakness] },
		{ 14790, [StatusID.FireWeakness] },
		{ 14791, [StatusID.IceWeakness] },
		{ 14795, [StatusID.LightningWeakness] },
		{ 14799, [StatusID.LightningWeakness] },
		{ 14800, [StatusID.LightningWeakness] },
		{ 14801, [StatusID.WindWeakness] },
		{ 14802, [StatusID.LightningWeakness] },
		{ 14804, [StatusID.LightningWeakness] },
		{ 14805, [StatusID.IceWeakness] },
		{ 14806, [StatusID.LightningWeakness] },
		{ 14809, [StatusID.LightningWeakness] },
		{ 14817, [StatusID.LightningWeakness] },
		{ 14820, [StatusID.LightningWeakness] },
		{ 14840, [StatusID.IceWeakness] },
		{ 14841, [StatusID.IceWeakness] },
		{ 14857, [StatusID.LightningWeakness] },
		{ 14858, [StatusID.LightningWeakness] },
		{ 14859, [StatusID.LightningWeakness] },
		{ 14860, [StatusID.FireWeakness] },
		{ 14861, [StatusID.FireWeakness] },
		{ 14862, [StatusID.FireWeakness] },
		{ 14863, [StatusID.IceWeakness] },
		{ 14864, [StatusID.WindWeakness] },
		{ 14865, [StatusID.FireWeakness] },
		{ 14866, [StatusID.FireWeakness] },
		{ 14867, [StatusID.IceWeakness] },
		{ 14868, [StatusID.LightningWeakness] },
		{ 14869, [StatusID.FireWeakness, StatusID.IceWeakness] },
		{ 14870, [StatusID.FireWeakness] },
		{ 14871, [StatusID.FireWeakness] },
		{ 14872, [StatusID.WindWeakness] },
		{ 14873, [StatusID.WindWeakness] },
		{ 14874, [StatusID.LightningWeakness] },
		{ 14875, [StatusID.LightningWeakness, StatusID.IceWeakness] },
		{ 14876, [StatusID.LightningWeakness] },
		{ 14877, [StatusID.LightningWeakness] },
		{ 14878, [StatusID.FireWeakness, StatusID.WindWeakness] },
		{ 14879, [StatusID.IceWeakness] },
		{ 14880, [StatusID.FireWeakness] },
		{ 14881, [StatusID.IceWeakness] },
		{ 14882, [StatusID.FireWeakness] },
		{ 14883, [StatusID.IceWeakness, StatusID.WindWeakness] },
		{ 14884, [StatusID.WindWeakness] },
		{ 14885, [StatusID.FireWeakness] },
		{ 14886, [StatusID.FireWeakness] },
		{ 14887, [StatusID.FireWeakness] },
		{ 14888, [StatusID.FireWeakness] },
		{ 14889, [StatusID.IceWeakness] },
		{ 14890, [StatusID.LightningWeakness] },
		{ 14891, [StatusID.LightningWeakness] },
		{ 14892, [StatusID.FireWeakness] },
		{ 14893, [StatusID.FireWeakness] },
		{ 14894, [StatusID.FireWeakness] },
		{ 14895, [StatusID.IceWeakness] },
		{ 14896, [StatusID.WindWeakness] },
		{ 14897, [StatusID.IceWeakness] },
		{ 14898, [StatusID.IceWeakness] },
		{ 14899, [StatusID.WindWeakness] },
		{ 14900, [StatusID.LightningWeakness] },
		{ 14901, [StatusID.LightningWeakness] },
		{ 14902, [StatusID.IceWeakness] },
		{ 14903, [StatusID.WindWeakness] },
		{ 14904, [StatusID.WindWeakness] },
		{ 14905, [StatusID.LightningWeakness] },
		{ 14906, [StatusID.IceWeakness] },
		{ 14907, [StatusID.WindWeakness] },
		{ 14908, [StatusID.IceWeakness, StatusID.WindWeakness] },
		{ 14909, [StatusID.FireWeakness] },
		{ 14910, [StatusID.WindWeakness] },
		{ 14911, [StatusID.WindWeakness] },
		{ 14912, [StatusID.WindWeakness] },
		{ 14913, [StatusID.FireWeakness] },
		{ 14914, [StatusID.IceWeakness] },
		{ 14915, [StatusID.FireWeakness] },
		{ 14917, [StatusID.WindWeakness] },
		{ 14918, [StatusID.WindWeakness] },
		{ 14919, [StatusID.FireWeakness] },
		{ 14920, [StatusID.IceWeakness] },
		{ 14921, [StatusID.WindWeakness] },
		{ 14922, [StatusID.LightningWeakness] },
		{ 14923, [StatusID.WindWeakness] },
		{ 14929, [StatusID.FireWeakness] },
		{ 14930, [StatusID.WindWeakness] },
		{ 14931, [StatusID.FireWeakness] },
		{ 14932, [StatusID.IceWeakness] },
	};

	/// <summary>
	/// 
	/// </summary>
	public static readonly Dictionary<uint, StatusID[]> SouthHornWeaknessByNameId = new()
	{
		{ 13638, [StatusID.FireWeakness] },
		{ 13646, [StatusID.FireWeakness] },
		{ 13656, [StatusID.IceWeakness] },
		{ 13666, [StatusID.IceWeakness] },
		{ 13668, [StatusID.IceWeakness] },
		{ 13703, [StatusID.FireWeakness] },
		{ 13717, [StatusID.LightningWeakness] },
		{ 13718, [StatusID.LightningWeakness] },
		{ 13726, [StatusID.LightningWeakness] },
		{ 13728, [StatusID.LightningWeakness] },
		{ 13729, [StatusID.LightningWeakness] },
		{ 13740, [StatusID.FireWeakness] },
		{ 13747, [StatusID.LightningWeakness] },
		{ 13748, [StatusID.LightningWeakness] },
		{ 13853, [StatusID.WindWeakness] },
		{ 13855, [StatusID.WindWeakness] },
		{ 13871, [StatusID.FireWeakness] },
		{ 13872, [StatusID.WindWeakness] },
		{ 13873, [StatusID.IceWeakness] },
		{ 13874, [StatusID.FireWeakness] },
		{ 13875, [StatusID.FireWeakness] },
		{ 13877, [StatusID.FireWeakness] },
		{ 13878, [StatusID.FireWeakness] },
		{ 13879, [StatusID.LightningWeakness] },
		{ 13880, [StatusID.IceWeakness] },
		{ 13881, [StatusID.LightningWeakness] },
		{ 13882, [StatusID.IceWeakness] },
		{ 13883, [StatusID.LightningWeakness] },
		{ 13884, [StatusID.FireWeakness] },
		{ 13885, [StatusID.IceWeakness] },
		{ 13886, [StatusID.FireWeakness] },
		{ 13887, [StatusID.FireWeakness] },
		{ 13888, [StatusID.IceWeakness] },
		{ 13890, [StatusID.FireWeakness] },
		{ 13891, [StatusID.LightningWeakness] },
		{ 13892, [StatusID.WindWeakness] },
		{ 13893, [StatusID.FireWeakness] },
		{ 13894, [StatusID.FireWeakness] },
		{ 13895, [StatusID.FireWeakness] },
		{ 13896, [StatusID.IceWeakness] },
		{ 13897, [StatusID.IceWeakness] },
		{ 13898, [StatusID.WindWeakness] },
		{ 13900, [StatusID.FireWeakness] },
		{ 13901, [StatusID.WindWeakness] },
		{ 13902, [StatusID.IceWeakness] },
		{ 13903, [StatusID.LightningWeakness] },
		{ 13904, [StatusID.IceWeakness] },
		{ 13906, [StatusID.FireWeakness] },
		{ 13907, [StatusID.IceWeakness] },
		{ 13908, [StatusID.IceWeakness] },
		{ 13909, [StatusID.IceWeakness] },
		{ 13911, [StatusID.FireWeakness] },
		{ 13912, [StatusID.LightningWeakness] },
		{ 13913, [StatusID.LightningWeakness] },
		{ 13915, [StatusID.LightningWeakness] },
		{ 13916, [StatusID.IceWeakness] },
		{ 13917, [StatusID.LightningWeakness] },
		{ 13918, [StatusID.IceWeakness] },
		{ 13919, [StatusID.FireWeakness] },
		{ 13922, [StatusID.WindWeakness] },
		{ 13924, [StatusID.LightningWeakness] },
		{ 13925, [StatusID.FireWeakness] },
		{ 13926, [StatusID.FireWeakness] },
		{ 13928, [StatusID.LightningWeakness] },
		{ 13930, [StatusID.IceWeakness] },
		{ 13931, [StatusID.FireWeakness] },
		{ 13932, [StatusID.FireWeakness] },
		{ 13933, [StatusID.LightningWeakness] },
		{ 13934, [StatusID.LightningWeakness] },
		{ 13935, [StatusID.WindWeakness] },
		{ 13936, [StatusID.LightningWeakness] },
		{ 13937, [StatusID.LightningWeakness] },
		{ 13938, [StatusID.IceWeakness] },
	};

	/// <summary>
	/// The elemental weakness statuses used in Occult Crescent
	/// </summary>
	public static StatusID[] OccultWeaknessStatuses { get; } =
	[
		StatusID.LightningWeakness,
		StatusID.FireWeakness,
		StatusID.IceWeakness,
		StatusID.WindWeakness,
	];

	/// <summary>
	/// 
	/// </summary>
	public static bool IsOccultWeaknessStatus(StatusID status)
	{
		return Array.IndexOf(OccultWeaknessStatuses, status) >= 0;
	}

	/// <summary>
	/// 
	/// </summary>
	public static bool HasKnownOccultWeakness(uint nameId)
	{
		if (DataCenter.IsInNorthHorn)
		{
			if (nameId != 0 && NorthHornWeaknessByNameId.ContainsKey(nameId))
			{
				return true;
			}
		}

		if (DataCenter.IsInSouthHorn)
		{
			if (nameId != 0 && SouthHornWeaknessByNameId.ContainsKey(nameId))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// 
	/// </summary>
	public static bool HasKnownOccultWeakness(uint nameId, StatusID status)
	{
		if (DataCenter.IsInNorthHorn)
		{
			if (nameId != 0 && NorthHornWeaknessByNameId.TryGetValue(nameId, out var known) && known != null && Array.IndexOf(known, status) >= 0)
			{
				return true;
			}
		}

		if (DataCenter.IsInSouthHorn)
		{
			if (nameId != 0 && SouthHornWeaknessByNameId.TryGetValue(nameId, out var known) && known != null && Array.IndexOf(known, status) >= 0)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// 
	/// </summary>
	public static StatusID[] RangePhysicalDefense { get; } =
	[
		StatusID.Troubadour,
		StatusID.Tactician_1951,
		StatusID.Tactician_2177,
		StatusID.ShieldSamba,
	];

	/// <summary>
	/// 
	/// </summary>
	public static StatusID[] PhysicalResistance { get; } =
	[
		StatusID.IceSpikes_1720,
		StatusID.IceSpikes_1307
	];

	/// <summary>
	/// 
	/// </summary>
	public static StatusID[] PhysicalRangedResistance { get; } =
	[
		StatusID.RangedResistance,
	];

	/// <summary>
	/// 
	/// </summary>
	public static StatusID[] MagicResistance { get; } =
	[
		StatusID.MagicResistance,
		StatusID.MagicResistance_3621,
		StatusID.RepellingSpray_556,
		StatusID.MagitekField_2166,
	];

	/// <summary>
	/// 
	/// </summary>
	public static StatusID[] AreaHots { get; } =
	[
		StatusID.AspectedHelios,
		StatusID.HeliosConjunction,
		StatusID.MedicaIi,
		StatusID.TrueMedicaIi,
		StatusID.PhysisIi,
		StatusID.Physis,
		StatusID.SacredSoil_1944,
		StatusID.WhisperingDawn,
		StatusID.AngelsWhisper,
		StatusID.Seraphism_3885,
		StatusID.Asylum_1911,
		StatusID.DivineAura,
		StatusID.MedicaIii_3986,
		StatusID.MedicaIii,

		// Summoner's Phoenix phase: Summon Phoenix grants the party regen Everlasting Flight. Without
		// it here the area thresholds healed under a running regen as if nothing were ticking.
		StatusID.EverlastingFlight,
	];

	/// <summary>
	/// 
	/// </summary>
	public static StatusID[] SingleHots { get; } =
	[
		StatusID.AspectedBenefic,
		StatusID.Regen,
		StatusID.Regen_897,
		StatusID.Regen_1330,
		StatusID.TheEwer_3891,

		// Rekindle's follow-up regen, triggered below 75% or on expiry (effect text of Rekindle).
		StatusID.UndyingFlame,
	];

	/// <summary>
	/// Shields (absorb effects) that have their own tracked duration, as opposed to instant HP-based
	/// mitigation. Used to decide whether a shield will still be up when it matters, e.g. for
	/// shield-aware heal prioritization.
	/// <para>
	/// A missing id here does not merely lose precision, it inverts the answer:
	/// <see cref="HasSurvivingShield"/> reads <c>GetObjectShield() &gt; 0 &amp;&amp;
	/// !WillStatusEnd(..., ShieldStatus)</c>, and <c>WillStatusEnd</c> reports an *absent* status as
	/// ending. A real barrier whose id is not listed therefore counts as no barrier at all, and its
	/// bearer looks more hurt than they are.
	/// </para>
	/// <para>
	/// The game gives most barriers several ids under one display name - one per version of the
	/// ability, plus PvP forms - so naming one and omitting the siblings ages the list silently.
	/// The ids below were collected with <c>.github/scripts/audit/scan13.py</c>, which decides
	/// membership on the effect text rather than the name: a status counts when its text says a
	/// barrier is nullifying or preventing damage. That excludes the neighbours that only reduce
	/// damage (<c>Catalyze_3088</c>) and those that merely set a barrier up for later
	/// (<c>DivineVeil</c> 726).
	/// </para>
	/// <para>
	/// Which form of an ability an id belongs to cannot be read off the status: the PvE and the PvP
	/// version share the display name and the effect text alike. The scan therefore matches each
	/// candidate against the action of the same name, and only ids whose PvE action actually creates
	/// a barrier are listed here. Aquaveil (3086) and Holy Sheltron (3026) are the counter-examples
	/// that show why - both PvE actions only reduce damage taken, so those barrier ids are PvP forms.
	/// </para>
	/// <para>
	/// Still absent, deliberately: 46 ids in 44 groups with no representative here, none of which
	/// sits behind a PvE action that creates a barrier - PvP forms, the astrologian's removed
	/// nocturnal sect statuses, and duty or item effects with no player action behind them. The scan
	/// fails the build if that number ever stops being zero.
	/// </para>
	/// </summary>
	public static StatusID[] ShieldStatus { get; } =
	[
		StatusID.Galvanize,
		StatusID.Galvanize_1331,
		StatusID.Galvanize_3087,
		StatusID.Catalyze,
		StatusID.Consolation,
		StatusID.SeraphicVeil,
		StatusID.SeraphicVeil_2040,
		StatusID.SeraphicVeil_3097,
		StatusID.EukrasianDiagnosis,
		StatusID.EukrasianDiagnosis_2865,
		StatusID.EukrasianDiagnosis_3109,
		StatusID.DifferentialDiagnosis,
		StatusID.EukrasianPrognosis,
		StatusID.EukrasianPrognosis_2866,
		StatusID.Haima,
		StatusID.Haima_2869,
		StatusID.Haima_3110,
		StatusID.Haimatinon,
		StatusID.Panhaima,
		StatusID.Panhaimatinon,
		StatusID.Holosakos,
		StatusID.DivineBenison,
		StatusID.DivineBenison_1404,
		StatusID.DivineVeil_1362,
		StatusID.DivineVeil_727,
		StatusID.DivineVeil_2168,
		StatusID.DivineVeil_2169,
		StatusID.DivineCaress,
		StatusID.Intersection,
		StatusID.Intersection_4040,
		// The barrier Neutral Sect puts on the receiver of an aspected spell, not the astrologian's
		// own healing buff of the same display name - that one reads "increases healing magic potency".
		StatusID.NeutralSect_1921,
		StatusID.NeutralSect_3988,
		StatusID.TheSpire_3892,
		StatusID.BlackestNight,
		StatusID.BlackestNight_1308,
		StatusID.BrutalShell,
		StatusID.BrutalShell_1997,
		StatusID.StemTheTide,
		StatusID.StemTheTide_3031,
		StatusID.ShakeItOff,
		StatusID.ShakeItOff_1993,
		StatusID.ShadeShift,
		StatusID.ShadeShift_2011,
		StatusID.CrestOfTimeBorrowed,
		StatusID.CrestOfTimeBorrowed_2597,
		StatusID.CrestOfTimeBorrowed_2861,
		StatusID.Manaward,
		StatusID.Manaward_1989,
		StatusID.RadiantAegis,
		StatusID.RadiantAegis_3224,
		StatusID.TemperaCoat,
		StatusID.TemperaCoat_4114,
		StatusID.TemperaGrassa,
		StatusID.TemperaGrassa_4115,
		StatusID.ImprovisedFinish,
		// Occult Crescent. Phantom job and duty actions, all of them party barriers, so they land on
		// the same targets a healer is judging.
		StatusID.OccultUnicorn,
		StatusID.BlessedRain,
		StatusID.MagicShell,
		StatusID.SteadfastStance,
		StatusID.Lance,
	];

	/// <summary>
	/// 
	/// </summary>
	public static StatusID[] TankStanceStatus { get; } =
	[
		StatusID.Grit,
		StatusID.RoyalGuard_1833,
		StatusID.IronWill,
		StatusID.Defiance,
		StatusID.Defiance_3124,
	];

	/// <summary>
	/// 
	/// </summary>
	/// <summary>
	/// Statuses under which the bearer cannot be killed, so healing them is less urgent than
	/// healing anyone else - not unnecessary. The window is the safest moment to heal, and when it
	/// closes the target keeps whatever health it had; Superbolide leaves them at 1 HP on purpose.
	/// <para>
	/// Only genuine invulnerabilities belong here. Two entries were removed because they are a
	/// different thing and were being read as if they were the same: <c>Mounted</c> nullifies HP
	/// recovery outright, which is an exclusion and lives in <see cref="HealingIneffectiveStatus"/>;
	/// <c>HpRecoveryDown</c> only reduces incoming healing, which is a reason to heal harder, not
	/// later. Both are still handled where they actually matter - StateUpdater checks them for
	/// their own duties, and the target selection excludes what healing cannot reach.
	/// </para>
	/// <para>
	/// <c>WalkingDead</c> stays commented out deliberately: there, healing is the survival
	/// condition rather than a courtesy.
	/// </para>
	/// </summary>
	public static StatusID[] NoNeedHealingStatus { get; } =
	[
		StatusID.Holmgang_409,
		StatusID.LivingDead,
		//StatusID.WalkingDead,
		StatusID.UndeadRebirth,
		StatusID.Superbolide,
		StatusID.HallowedGround,
		StatusID.HallowedGround_1302,
		StatusID.Invulnerability,
	];

	/// <summary>
	/// Every status under which a hit cannot kill the bearer: <see cref="NoNeedHealingStatus"/> and
	/// Walking Dead, which that list leaves out for the heal decision's sake.
	/// </summary>
	internal static StatusID[] InvulnerabilityStatus { get; } = [.. NoNeedHealingStatus, StatusID.WalkingDead];

	/// <summary>
	/// The subset of <see cref="NoNeedHealingStatus"/> whose trigger is the bearer's own death.
	/// Living Dead is the only one: the dark knight spends it expecting to be killed, and the kill
	/// is what converts it into Walking Dead and its self-healing. Healing the bearer above zero
	/// while it is up does not merely waste a cast - it removes the very event the ability is
	/// waiting for, which is why these get a full hold rather than the lowered threshold the other
	/// invulnerabilities get.
	/// <para>
	/// The hold is not open-ended. Callers reach this list through
	/// <see cref="InDeathTriggerWindow(IBattleChara)"/>, which releases it while the status still
	/// has lead time left - so once the death can no longer arrive in time, the normal threshold
	/// returns and the bearer is healed like anyone else.
	/// </para>
	/// </summary>
	public static StatusID[] DeathTriggeredStatus { get; } =
	[
		StatusID.LivingDead,
	];

	/// <summary>
	/// <see cref="NoNeedHealingStatus"/> without <see cref="DeathTriggeredStatus"/>: the protection the heal
	/// decision reads while the death-triggered hold is switched off (ObjectHelper.IsUnprotectedForHealing).
	/// Declared after both lists, which it is built from.
	/// </summary>
	internal static StatusID[] NoNeedHealingStatusBesidesDeathTrigger { get; } = Without(NoNeedHealingStatus, DeathTriggeredStatus);

	/// <summary>
	/// 
	/// </summary>
	public static StatusID[] SwiftcastStatus { get; } =
	[
		StatusID.Swiftcast,
		StatusID.Triplecast,
		StatusID.Dualcast,
		StatusID.Dualcast_5438,
		StatusID.OccultQuick,
		StatusID.LostChainspell
	];

	/// <summary>
	/// 
	/// </summary>
	public static StatusID[] AstCardStatus { get; } =
	[
		StatusID.TheBalance_3887,
		StatusID.TheSpear_3889,
		StatusID.Weakness,
		StatusID.BrinkOfDeath,
	];

	/// <summary>
	/// The big personal mitigations and the invulnerabilities, read in two places that both depend
	/// on it being complete: the job rotations put it on Shadow Wall and Shadowed Vigil as
	/// <c>StatusProvide</c> so those never overlap, and <c>CustomRotation.HasMajorMitigation</c>
	/// reads it to keep a barrier from being spent while one of them is up.
	/// <para>
	/// Membership is decided on the effect text, not on the display name. Both directions have a
	/// case here: <c>Rampart_1978</c> is the form a tank from level 94 actually carries - "damage
	/// taken is reduced while HP recovered via healing actions is increased", scoped PLD WAR DRK GNB
	/// - and its absence made the whole list miss the most common mitigation in the game. In the
	/// other direction, <c>Nebula_3051</c> ("inflicting a portion of sustained damage back to its
	/// source") and <c>Bloodwhetting_3030</c> ("weaponskills generate HP") share a name with a
	/// mitigation but are the reflect and lifesteal halves, so they stay out. So do the Holmgang ids
	/// 88 and 1305, which sit on the target rather than the tank (AUDIT_LOG C15). Bloodwhetting itself
	/// (2678) is the warrior's short cooldown - 10% for eight seconds, every 25 - as Heart of Corundum,
	/// Holy Sheltron and The Blackest Night are for the others, none of them listed; listed, it held
	/// Rampart and Damnation back at the start of every pull and on every tankbuster (A238).
	/// </para>
	/// </summary>
	public static StatusID[] RampartStatus { get; } =
	[
		StatusID.Rampart,
		StatusID.Rampart_1191,
		StatusID.Rampart_1978,
		StatusID.Rampart_4168,
		StatusID.Bulwark,

		StatusID.Vengeance,
		StatusID.Damnation,

		StatusID.Sentinel,
		StatusID.Guardian,

		StatusID.ShadowWall,
		StatusID.ShadowedVigil,

		StatusID.Nebula,
		StatusID.GreatNebula,

		StatusID.Superbolide,
		StatusID.HallowedGround,
		StatusID.HallowedGround_1302,
		StatusID.Holmgang_409,
		StatusID.LivingDead,
	];

	/// <summary>
	/// 
	/// </summary>
	public static StatusID[] NoPositionalStatus { get; } =
	[
		StatusID.TrueNorth,
	];

	/// <summary>
	/// 
	/// </summary>
	public static StatusID[] DoomHealStatus { get; } =
	[
		StatusID.Doom_1769,
		StatusID.Doom_5473
	];

	/// <summary>
	/// M9S Hell in a Cell statuses on the player. Each one pairs with the <see cref="HeelOfTheCellStatus"/> at the same index.
	/// </summary>
	public static StatusID[] HellInACellStatus { get; } =
	[
		StatusID.HellInACell,
		StatusID.HellInACell_4732,
		StatusID.HellInACell_4733,
		StatusID.HellInACell_4734,
		StatusID.HellInACell_4735,
		StatusID.HellInACell_4736,
		StatusID.HellInACell_4737,
		StatusID.HellInACell_4738,
	];

	/// <summary>
	/// M9S Heel in a Cell statuses on the Charnel Cells. Each one pairs with the <see cref="HellInACellStatus"/> at the same index.
	/// </summary>
	public static StatusID[] HeelOfTheCellStatus { get; } =
	[
		StatusID.HeelOfTheCell,
		StatusID.HeelOfTheCell_4740,
		StatusID.HeelOfTheCell_4741,
		StatusID.HeelOfTheCell_4742,
		StatusID.HeelOfTheCell_4743,
		StatusID.HeelOfTheCell_4744,
		StatusID.HeelOfTheCell_4745,
		StatusID.HeelOfTheCell_4746,
	];

	/// <summary>
	/// Statuses for the Phantom Oracle spell PredictPvE.
	/// </summary>
	public static StatusID[] OracleStatuses { get; } =
	[
		StatusID.PredictionOfCleansing,
		StatusID.PredictionOfStarfall,
		StatusID.PredictionOfJudgment,
		StatusID.PredictionOfBlessing
	];

	/// <summary>
	/// Statuses that can be dispelled by Occult Dispel.
	/// </summary>
	public static StatusID[] PhantomDispellable { get; } =
	[
		StatusID.DamageUp_1161,
		StatusID.DamageUp,
		StatusID.DarkDefenses,
		StatusID.MagicDamageUp_2556,
		StatusID.EvasionUp_1706
        //StatusID.Invincibility_4539 maybe this, need verification, seems to be on guardian knight in Rooms & Lockwards part of forked tower
    ];

	/// <summary>
	/// 
	/// </summary>
	public static StatusID[] PurifyPvPStatuses { get; } =
	[
		StatusID.Stun_1343,
		StatusID.Heavy_1344,
		StatusID.Bind_1345,
		StatusID.Silence_1347,
		StatusID.DeepFreeze_3219,
		StatusID.MiracleOfNature,
	];

	/// <summary>
	/// 
	/// </summary>
	public static StatusID[] RotationLockoutStatus { get; } =
	[
		StatusID.Reawakened,
		StatusID.Overheated,
		StatusID.InnerRelease,
		StatusID.Eukrasia,
		StatusID.Mudra,
		StatusID.TenChiJin,
		StatusID.FullMetalMachinist
	];

	/// <summary>
	/// Every stun the game knows, bundled because which id a given action applies cannot be
	/// determined from the name alone. A stun stops the target from acting at all, so for the
	/// duration it is stronger than any damage reduction - who applied it does not matter.
	/// </summary>
	public static StatusID[] StunStatus { get; } =
	[
		StatusID.Stun,
		StatusID.Stun_142,
		StatusID.Stun_149,
		StatusID.Stun_201,
		StatusID.Stun_1343,
		StatusID.Stun_1513,
		StatusID.Stun_1521,
		StatusID.Stun_1522,
		StatusID.Stun_2656,
		StatusID.Stun_2953,
		StatusID.Stun_3408,
		StatusID.Stun_3465,
		StatusID.Stun_4163,
		StatusID.Stun_4374,
		StatusID.Stun_4378,
		StatusID.Stun_4433,
		StatusID.Stun_5043,
		StatusID.Stun_5411,
		StatusID.Stun_5803
	];

}

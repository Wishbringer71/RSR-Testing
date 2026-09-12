using Dalamud.Game.ClientState.Statuses;
using ECommons.Automation;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// The helper for the status.
/// </summary>
public static class StatusHelper
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
		StatusID.MedicaIii
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
	];

	/// <summary>
	/// Every status the game data lists under the name Reprisal with the same effect text. Which of
	/// them the role action applies at a given level is not verifiable from the data alone, so every
	/// Reprisal check looks for any of them.
	/// <para>
	/// <c>Reprisal_2101</c> is scoped to PLD WAR DRK GNB rather than to the shared role, which is the
	/// signature of the form a trait upgrades into - Enhanced Reprisal at level 98 extends the
	/// duration to 15s. It leaves the reduction at 10%: an earlier version of this note claimed 15%
	/// without naming a source, and the Lodestone entry says otherwise. Its absence made every reader of this list blind to
	/// the version an end-game tank actually applies: <c>ReprisalPvE</c> carries this list as
	/// <c>TargetStatusProvide</c>, so the guard against re-applying it never saw the debuff, and the
	/// mitigation surveys that read it counted a reprised pull as unmitigated.
	/// </para>
	/// <para>
	/// Kept out: no id under this name carries a different effect text, so the list is complete as
	/// long as no new one appears. <c>.github/scripts/audit/scan14.py</c> reports one that does.
	/// </para>
	/// </summary>
	public static StatusID[] ReprisalStatus { get; } =
	[
		StatusID.Reprisal,
		StatusID.Reprisal_1193,
		StatusID.Reprisal_2101,
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
	/// Every id the game files under the display name Slow, all of them carrying the same effect:
	/// "weaponskill cast time and recast time, spell cast time and recast time, and auto-attack
	/// delay are increased".
	/// <para>
	/// The auto-attack half is what makes this a mitigation and not a caster nuisance. Trash enemies
	/// deal most of their damage by auto-attack, so a slowed pack throttles the incoming stream by
	/// roughly the size of the debuff - the same order as Rampart, and enough to stop a barrier that
	/// only pays off when it is spent from being spent at all.
	/// </para>
	/// <para>
	/// Read as a group, like <see cref="StunStatus"/> and for the same reason: which id a given
	/// action applies is not decidable from the data, and the source does not matter. Arm's Length
	/// is the one a tank brings; Blue Mage and the Occult Crescent phantom jobs bring others.
	/// Slow+ (427, 1568) is the stronger grade of the same effect and belongs here too.
	/// </para>
	/// </summary>
	public static StatusID[] SlowStatus { get; } =
	[
		StatusID.Slow,
		StatusID.Slow_10,
		StatusID.Slow_193,
		StatusID.Slow_427,
		StatusID.Slow_442,
		StatusID.Slow_561,
		StatusID.Slow_1346,
		StatusID.Slow_1509,
		StatusID.Slow_1568,
		StatusID.Slow_2246,
		StatusID.Slow_3464,
		StatusID.Slow_3493,
	];

	/// <summary>
	/// Barriers that pay a reward only when they are absorbed in full, so letting one expire unspent
	/// wastes its cost rather than merely leaving protection unused.
	/// <para>
	/// The Blackest Night is the only one in the game: it grants Dark Arts when its barrier - 25% of
	/// maximum HP over 7s - is consumed completely (action 7393), and nothing at all when it is not.
	/// Every other barrier in <see cref="ShieldStatus"/> is pure protection, where an unspent barrier
	/// is a good outcome. That difference is why this list exists separately instead of asking
	/// whether any shield is running.
	/// </para>
	/// <para>
	/// Read by rules that would otherwise stop the damage stream while such a barrier is up - the
	/// white mage's Holy stun is the case this was written for.
	/// </para>
	/// </summary>
	public static StatusID[] FullAbsorbRewardStatus { get; } =
	[
		StatusID.BlackestNight,
		StatusID.BlackestNight_1308,
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
	/// Whether <paramref name="battleChara"/> is inside a death-triggered window that still has room
	/// for the death to arrive - that is, <see cref="DeathTriggeredStatus"/> is up and not about to
	/// expire.
	/// <para>
	/// The clock has to be read on this list alone. <see cref="NoNeedHealingInvuln"/> measures the
	/// *earliest* expiry across all of <see cref="NoNeedHealingStatus"/>, so an unrelated short
	/// invulnerability on the same target - a Phantom Oracle's, say - would report the window as
	/// ending while Living Dead still had most of its duration left.
	/// </para>
	/// <para>
	/// Two GCDs of lead time. The tempting optimisation is to shorten it: the hold's one real cost
	/// is that it can cancel a death that would still have arrived in time, and that cost is exactly
	/// the lead time - against Living Dead's ten seconds, two GCDs give away half the window. But
	/// the lead time is measured to the *decision*, not to the heal landing. Worst case the current
	/// GCD has to run out and a cast has to finish on top of it, which is two GCDs on its own, so
	/// anything shorter lands the heal after the window has already closed and the bearer is
	/// unprotected. Half the window is the price of the heal actually arriving.
	/// </para>
	/// <para>
	/// An instant heal would land immediately and make a shorter lead time safe, but which heal is
	/// about to be cast is a rotation decision that this layer cannot see, let alone enforce.
	/// </para>
	/// </summary>
	public static bool InDeathTriggerWindow(this IBattleChara battleChara)
	{
		return battleChara.HasStatus(false, DeathTriggeredStatus)
			&& !battleChara.WillStatusEndGCD(DeathTriggerLeadGCDs, 0, false, DeathTriggeredStatus);
	}

	/// <inheritdoc cref="InDeathTriggerWindow(IBattleChara)"/>
	public static bool PlayerInDeathTriggerWindow()
	{
		return PlayerHasStatus(false, DeathTriggeredStatus)
			&& !PlayerWillStatusEndGCD(DeathTriggerLeadGCDs, 0, false, DeathTriggeredStatus);
	}

	/// <summary>How early the death-trigger hold releases; see <see cref="InDeathTriggerWindow"/>.</summary>
	private const uint DeathTriggerLeadGCDs = 2;

	/// <summary>
	/// Statuses under which a heal lands for nothing at all, as opposed to merely being less urgent.
	/// A target carrying one of these is not a healing candidate: the cast would be spent and the
	/// target left exactly as it was. Mounted's own description says HP recovery is "nullified".
	/// <para>
	/// Deliberately narrower than <see cref="NoNeedHealingStatus"/>, which mixes this case in with
	/// genuine invulnerabilities. Those postpone the need for healing, they do not remove it - a
	/// Superbolide gunbreaker sits at 1 HP and needs healing more than anyone, just not this second.
	/// Only a status that nullifies healing outright belongs here; HpRecoveryDown, which merely
	/// reduces it, does not.
	/// </para>
	/// </summary>
	/// <remarks>
	/// Only <see cref="StatusID.Mounted"/> (1420) qualifies on the evidence: its description says
	/// "HP recovery and beneficial effects conferred by actions are nullified". Mounted_1520 is
	/// "Riding atop a Rathalos" and says nothing about healing, so it is deliberately absent - a
	/// shared identifier is not evidence of a shared effect.
	/// </remarks>
	public static StatusID[] HealingIneffectiveStatus { get; } =
	[
		StatusID.Mounted,
	];

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
	/// 88 and 1305, which sit on the target rather than the tank (AUDIT_LOG C15).
	/// </para>
	/// </summary>
	public static StatusID[] RampartStatus { get; } =
	[
		StatusID.Rampart,
		StatusID.Rampart_1191,
		StatusID.Rampart_1978,
		StatusID.Rampart_4168,
		StatusID.Bulwark,
		StatusID.Bloodwhetting,

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
		StatusID.Stun_5411
	];

	/// <summary>
	/// The resistance a target builds up against being stunned again. The game tracks it as a
	/// status of its own, so no application has to be counted: while this sits on a target, a
	/// further stun is shorter or does not land at all, and when it falls off the budget is back.
	/// </summary>
	public static StatusID[] StunResistanceStatus { get; } =
	[
		StatusID.StunResistance,
		StatusID.StunResistance_1349
	];

	/// <summary>
	/// Determines if the specified battle character has reached the maximum number of status effects.
	/// </summary>
	/// <param name="battleChara">The battle character to check.</param>
	/// <returns>
	/// <c>true</c> if the character's status list is at the cap (30 for players, 60 for NPCs); otherwise, <c>false</c>.
	/// </returns>
	public unsafe static bool IsStatusCapped(IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return false;
		}

		try
		{
			if (battleChara.StatusList == null)
			{
				return false;
			}
		}
		catch
		{
			return false;
		}

		if (battleChara.IsValid())
		{
			var count = 0;
			foreach (var x in battleChara.StatusList)
			{
				if (x.StatusId != 0)
				{
					count++;
				}
			}
			if (count == battleChara.Struct()->StatusManager.NumValidStatuses)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Check whether the Player needs to be healing.
	/// </summary>
	/// <returns></returns>
	public static bool PlayerNoNeedHealingInvuln()
	{
		if (Player.Object == null)
		{
			return false;
		}

		return PlayerWillStatusEndGCD(2, 0, false, NoNeedHealingStatus);
	}

	/// <summary>
	/// Check whether the target needs to be healing.
	/// </summary>
	/// <param name="Invulnp"></param>
	/// <returns></returns>
	public static bool NoNeedHealingInvuln(this IBattleChara Invulnp)
	{
		return Invulnp.WillStatusEndGCD(2, 0, false, NoNeedHealingStatus);
	}

	/// <summary>
	/// Check if the Player needs to be healed because of Doomed To Heal status.
	/// </summary>
	/// <returns></returns>
	public static bool PlayerDoomNeedHealing()
	{
		if (Player.Object == null)
		{
			return false;
		}

		return PlayerHasStatus(false, DoomHealStatus);
	}

	/// <summary>
	/// Check if the target needs to be healed because of Doomed To Heal status.
	/// </summary>
	/// <param name="Doomp"></param>
	/// <returns></returns>
	public static bool DoomNeedHealing(this IBattleChara Doomp)
	{
		if (Doomp == null)
		{
			return false;
		}

		if (!Doomp.IsValid())
		{
			return false;
		}

		// Extra defensive checks to shrink the window between the IsValid()
		// check above and the native StatusList access below. Objects can be
		// invalidated (despawned/removed from the object table) between the
		// two, and some entity kinds do not expose a usable StatusManager.
		if (Doomp.Address == nint.Zero || Doomp.EntityId == 0)
		{
			return false;
		}

		try
		{
			if (!Doomp.IsValid())
			{
				return false;
			}

			if (Doomp.StatusList == null)
			{
				return false;
			}

			if (HasStatus(Doomp, false, DoomHealStatus))
			{
				return true;
			}
		}
		catch (Exception)
		{
			return false;
		}

		return false;
	}

	/// <summary>
	/// Will any of <paramref name="statusIDs"/> end after <paramref name="gcdCount"/> GCDs plus <paramref name="offset"/> seconds?
	/// </summary>
	/// <param name="gcdCount"></param>
	/// <param name="offset"></param>
	/// <param name="isFromSelf"></param>
	/// <param name="statusIDs"></param>
	/// <returns></returns>
	public static bool PlayerWillStatusEndGCD(uint gcdCount = 0, float offset = 0, bool isFromSelf = true, params StatusID[] statusIDs)
	{
		if (Player.Object == null)
		{
			return false;
		}

		return PlayerWillStatusEnd(DataCenter.GCDTime(gcdCount, offset), isFromSelf, statusIDs);
	}

	/// <summary>
	/// Will any of <paramref name="statusIDs"/> end after <paramref name="gcdCount"/> GCDs plus <paramref name="offset"/> seconds?
	/// </summary>
	/// <param name="battleChara"></param>
	/// <param name="gcdCount"></param>
	/// <param name="offset"></param>
	/// <param name="isFromSelf"></param>
	/// <param name="statusIDs"></param>
	/// <returns></returns>
	public static bool WillStatusEndGCD(this IBattleChara battleChara, uint gcdCount = 0, float offset = 0, bool isFromSelf = true, params StatusID[] statusIDs)
	{
		return WillStatusEnd(battleChara, DataCenter.GCDTime(gcdCount, offset), isFromSelf, statusIDs);
	}

	/// <summary>
	/// Will any of <paramref name="statusIDs"/> end after <paramref name="time"/> seconds?
	/// </summary>
	/// <param name="time"></param>
	/// <param name="isFromSelf"></param>
	/// <param name="statusIDs"></param>
	/// <returns></returns>
	public static bool PlayerWillStatusEnd(float time, bool isFromSelf = true, params StatusID[] statusIDs)
	{
		if (PlayerHasApplyStatus(statusIDs))
		{
			return false;
		}

		var statusTime = PlayerStatusTime(isFromSelf, statusIDs);
		return (statusTime >= 0f || !PlayerHasStatus(isFromSelf, statusIDs)) && statusTime <= time;
	}

	/// <summary>
	/// Will any of <paramref name="statusIDs"/> end after <paramref name="time"/> seconds?
	/// </summary>
	/// <param name="battleChara"></param>
	/// <param name="time"></param>
	/// <param name="isFromSelf"></param>
	/// <param name="statusIDs"></param>
	/// <returns></returns>
	public static bool WillStatusEnd(this IBattleChara battleChara, float time, bool isFromSelf = true, params StatusID[] statusIDs)
	{
		if (HasApplyStatus(battleChara, statusIDs))
		{
			return false;
		}

		var statusTime = battleChara.StatusTime(isFromSelf, statusIDs);
		return (statusTime >= 0f || !battleChara.HasStatus(isFromSelf, statusIDs)) && statusTime <= time;
	}

	/// <summary>
	/// Whether <paramref name="battleChara"/> has an active shield (see <see cref="ShieldStatus"/>) that will
	/// still be up in <paramref name="horizon"/> seconds. A shield about to expire shouldn't be credited toward
	/// the target's effective health, since it won't be there to absorb the damage that matters.
	/// </summary>
	public static bool HasSurvivingShield(this IBattleChara battleChara, float horizon)
	{
		return battleChara.GetObjectShield() > 0 && !battleChara.WillStatusEnd(horizon, false, ShieldStatus);
	}

	/// <summary>
	/// Get the remaining time of the status (raw remaining time of the earliest matching status). Returns 0 if none.
	/// NOTE: Previously this subtracted DefaultGCDRemain which caused premature refresh decisions.
	/// </summary>
	public static float PlayerStatusTime(bool isFromSelf, params StatusID[] statusIDs)
	{
		if (Player.Object == null)
		{
			return 0f;
		}

		try
		{
			if (PlayerHasApplyStatus(statusIDs))
			{
				return float.MaxValue;
			}

			var times = PlayerStatusTimes(isFromSelf, statusIDs);
			var min = float.MaxValue;
			var found = false;
			foreach (var t in times)
			{
				if (t < min)
				{
					min = t;
				}
				found = true;
			}
			// Return 0 when not found (legacy behaviour expected by callers), otherwise raw remaining time.
			return !found ? 0f : min;
		}
		catch (Exception ex)
		{
			PluginLog.Error($"Failed to get status time: {ex.Message}");
			return 0f;
		}
	}

	/// <summary>
	/// Get the remaining time of the status (raw remaining time of the earliest matching status). Returns 0 if none.
	/// NOTE: Previously this subtracted DefaultGCDRemain which caused premature refresh decisions.
	/// </summary>
	public static float StatusTime(this IBattleChara battleChara, bool isFromSelf, params StatusID[] statusIDs)
	{
		try
		{
			if (HasApplyStatus(battleChara, statusIDs))
			{
				return float.MaxValue;
			}

			var times = battleChara.StatusTimes(isFromSelf, statusIDs);
			var min = float.MaxValue;
			var found = false;
			foreach (var t in times)
			{
				if (t < min)
				{
					min = t;
				}
				found = true;
			}
			// Return 0 when not found (legacy behaviour expected by callers), otherwise raw remaining time.
			return !found ? 0f : min;
		}
		catch (Exception ex)
		{
			PluginLog.Error($"Failed to get status time: {ex.Message}");
			return 0f;
		}
	}

	internal static IEnumerable<float> PlayerStatusTimes(bool isFromSelf, params StatusID[] statusIDs)
	{
		if (Player.Object == null)
		{
			yield break;
		}

		foreach (var status in Player.Object.GetStatus(isFromSelf, statusIDs))
		{
			yield return status.RemainingTime == 0f ? float.MaxValue : status.RemainingTime;
		}
	}

	internal static IEnumerable<float> StatusTimes(this IBattleChara battleChara, bool isFromSelf, params StatusID[] statusIDs)
	{
		foreach (var status in battleChara.GetStatus(isFromSelf, statusIDs))
		{
			yield return status.RemainingTime == 0f ? float.MaxValue : status.RemainingTime;
		}
	}

	/// <summary>
	/// Get the stack count of the status.
	/// </summary>
	/// <param name="isFromSelf"></param>
	/// <param name="statusIDs"></param>
	/// <returns></returns>
	public static byte PlayerStatusStack(bool isFromSelf, params StatusID[] statusIDs)
	{
		if (Player.Object == null)
		{
			return 0;
		}

		if (PlayerHasApplyStatus(statusIDs))
		{
			return byte.MaxValue;
		}

		var stacks = PlayerStatusStacks(isFromSelf, statusIDs);
		var min = byte.MaxValue;
		var found = false;
		foreach (var s in stacks)
		{
			if (s < min)
			{
				min = s;
			}

			found = true;
		}
		return found ? min : (byte)0;
	}

	/// <summary>
	/// Get the stack count of the status.
	/// </summary>
	/// <param name="battleChara"></param>
	/// <param name="isFromSelf"></param>
	/// <param name="statusIDs"></param>
	/// <returns></returns>
	public static byte StatusStack(this IBattleChara battleChara, bool isFromSelf, params StatusID[] statusIDs)
	{
		if (HasApplyStatus(battleChara, statusIDs))
		{
			return byte.MaxValue;
		}

		var stacks = battleChara.StatusStacks(isFromSelf, statusIDs);
		var min = byte.MaxValue;
		var found = false;
		foreach (var s in stacks)
		{
			if (s < min)
			{
				min = s;
			}

			found = true;
		}
		return found ? min : (byte)0;
	}

	private static IEnumerable<byte> PlayerStatusStacks(bool isFromSelf, params StatusID[] statusIDs)
	{
		if (Player.Object == null)
		{
			yield break;
		}

		foreach (var status in PlayerGetStatus(isFromSelf, statusIDs))
		{
			yield return (byte)(status.Param == 0 ? byte.MaxValue : status.Param);
		}
	}

	private static IEnumerable<byte> StatusStacks(this IBattleChara battleChara, bool isFromSelf, params StatusID[] statusIDs)
	{
		foreach (var status in battleChara.GetStatus(isFromSelf, statusIDs))
		{
			yield return (byte)(status.Param == 0 ? byte.MaxValue : status.Param);
		}
	}

	/// <summary>
	/// Check if the player object has any of the specified statuses.
	/// </summary>
	/// <param name="isFromSelf"></param>
	/// <param name="statusIDs"></param>
	/// <returns></returns>
	public static bool PlayerHasStatus(bool isFromSelf, params StatusID[] statusIDs)
	{
		if (Player.Object == null)
		{
			return false;
		}

		if (Player.Object.StatusList == null)
		{
			return false;
		}

		if (PlayerHasApplyStatus(statusIDs))
		{
			return true;
		}

		foreach (var _ in PlayerGetStatus(isFromSelf, statusIDs))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Check if the object has any of the specified statuses.
	/// </summary>
	/// <param name="battleChara"></param>
	/// <param name="isFromSelf"></param>
	/// <param name="statusIDs"></param>
	/// <returns></returns>
	public static bool HasStatus(this IBattleChara battleChara, bool isFromSelf, params StatusID[] statusIDs)
	{
		try
		{
			if (!DataCenter.PlayerAvailable())
			{
				return false;
			}

			if (Player.Object == null)
			{
				return false;
			}

			if (Player.Object.StatusList == null)
			{
				return false;
			}

			if (battleChara == null)
			{
				return false;
			}

			if (!battleChara.IsValid())
			{
				return false;
			}

			if (battleChara.StatusList == null)
			{
				return false;
			}

			if (HasApplyStatus(battleChara, statusIDs))
			{
				return true;
			}

			foreach (var _ in battleChara.GetStatus(isFromSelf, statusIDs))
			{
				return true;
			}
		}
		catch
		{
			// StatusList threw, treat as unavailable
			return false;
		}

		return false;
	}

	/// <summary>
	/// Checks if the player currently has any of the statuses being applied,
	/// as tracked by <c>DataCenter.ApplyStatus</c> during effect time.
	/// </summary>
	/// <param name="statusIDs">An array of status IDs to check against the applied status.</param>
	/// <returns>
	/// <c>true</c> if any of the specified statuses are currently being applied to the character; otherwise, <c>false</c>.
	/// </returns>
	public static bool PlayerHasApplyStatus(StatusID[] statusIDs)
	{
		if (!DataCenter.PlayerAvailable())
		{
			return false;
		}

		if (Player.Object == null)
		{
			return false;
		}

		try
		{
			if (Player.Object.StatusList == null)
			{
				return false;
			}
		}
		catch
		{
			// StatusList threw, treat as unavailable
			return false;
		}

		if (DataCenter.InEffectTime && DataCenter.ApplyStatus.TryGetValue(Player.Object.GameObjectId, out var statusId))
		{
			foreach (var s in statusIDs)
			{
				if ((uint)s == statusId)
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Checks if the specified <paramref name="battleChara"/> currently has any of the statuses being applied,
	/// as tracked by <c>DataCenter.ApplyStatus</c> during effect time.
	/// </summary>
	/// <param name="battleChara">The battle character to check for applied statuses.</param>
	/// <param name="statusIDs">An array of status IDs to check against the applied status.</param>
	/// <returns>
	/// <c>true</c> if any of the specified statuses are currently being applied to the character; otherwise, <c>false</c>.
	/// </returns>
	public static bool HasApplyStatus(this IBattleChara battleChara, StatusID[] statusIDs)
	{
		try
		{
			if (battleChara.StatusList == null)
			{
				return false;
			}
		}
		catch
		{
			// StatusList threw, treat as unavailable
			return false;
		}

		if (DataCenter.InEffectTime && DataCenter.ApplyStatus.TryGetValue(battleChara.GameObjectId, out var statusId))
		{
			foreach (var s in statusIDs)
			{
				if ((uint)s == statusId)
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Remove the specified status.
	/// </summary>
	/// <param name="status"></param>
	public static void StatusOff(StatusID status)
	{
		if (!DataCenter.IsActivated())
		{
			return;
		}

		if (!PlayerHasStatus(false, status))
		{
			return;
		}

		try
		{
			Chat.SendMessage($"/statusoff \"{GetStatusName(status)}\"");
			PluginLog.Information($"Status {GetStatusName(status)} removed successfully.");
		}
		catch (Exception ex)
		{
			PluginLog.Error($"Failed to remove status {GetStatusName(status)}: {ex.Message}");
		}
	}

	/// <summary>
	/// Get the name of the specified status.
	/// </summary>
	/// <param name="id">The status ID.</param>
	/// <returns>The name of the status.</returns>
	internal static string GetStatusName(StatusID id)
	{
		var sheet = Service.GetSheet<Lumina.Excel.Sheets.Status>();
		if (sheet == null)
		{
			return string.Empty;
		}

		var statusRow = sheet.GetRow((uint)id);
		return statusRow.RowId == 0 ? string.Empty : statusRow.Name.ToString() ?? string.Empty;
	}

	/// <summary>
	/// Get the statuses of the specified object.
	/// </summary>
	/// <param name="battleChara">The object to get the statuses from.</param>
	/// <param name="isFromSelf">Whether the statuses are from self.</param>
	/// <param name="statusIDs">The status IDs to look for.</param>
	/// <returns>An enumerable of statuses.</returns>
	private static IEnumerable<IStatus> GetStatus(this IBattleChara battleChara, bool isFromSelf, params StatusID[] statusIDs)
	{
		if (battleChara == null)
		{
			yield break;
		}

		StatusList statusList;
		try
		{
			statusList = battleChara.StatusList;
			if (statusList == null)
			{
				yield break;
			}
		}
		catch
		{
			// StatusList threw, treat as unavailable
			yield break;
		}

		// Linear membership check to avoid HashSet allocation (statusIDs is small in practice)
		static bool ContainsId(uint id, StatusID[] ids)
		{
			for (var i = 0; i < ids.Length; i++)
			{
				if ((uint)ids[i] == id)
				{
					return true;
				}
			}
			return false;
		}

		var playerId = Player.Object?.GameObjectId ?? 0;

		for (var i = 0; i < statusList.Length; i++)
		{
			var status = statusList[i];
			if (status == null)
			{
				continue;
			}

			if (status.StatusId == 0)
			{
				continue;
			}

			if (isFromSelf)
			{
				if (status.SourceId != playerId && status.SourceObject?.OwnerId != playerId)
				{
					continue;
				}
			}

			if (ContainsId(status.StatusId, statusIDs))
			{
				yield return status;
			}
		}
	}

	/// <summary>
	/// Get the statuses of the Player.
	/// </summary>
	/// <param name="isFromSelf">Whether the statuses are from self.</param>
	/// <param name="statusIDs">The status IDs to look for.</param>
	/// <returns>An enumerable of statuses.</returns>
	private static IEnumerable<IStatus> PlayerGetStatus(bool isFromSelf, params StatusID[] statusIDs)
	{
		if (Player.Object == null)
		{
			yield break;
		}

		StatusList statusList;
		try
		{
			statusList = Player.Object.StatusList;
			if (statusList == null)
			{
				yield break;
			}
		}
		catch
		{
			// StatusList threw, treat as unavailable
			yield break;
		}

		// Linear membership check to avoid HashSet allocation (statusIDs is small in practice)
		static bool ContainsId(uint id, StatusID[] ids)
		{
			for (var i = 0; i < ids.Length; i++)
			{
				if ((uint)ids[i] == id)
				{
					return true;
				}
			}
			return false;
		}

		var playerId = Player.Object?.GameObjectId ?? 0;

		for (var i = 0; i < statusList.Length; i++)
		{
			var status = statusList[i];
			if (status == null || status.StatusId == 0)
			{
				continue;
			}

			if (isFromSelf)
			{
				if (status.SourceId != playerId && status.SourceObject?.OwnerId != playerId)
				{
					continue;
				}
			}

			if (ContainsId(status.StatusId, statusIDs))
			{
				yield return status;
			}
		}
	}


	/// <summary>
	/// Check if the status is invincible.
	/// </summary>
	/// <param name="status">The status to check.</param>
	/// <returns>True if the status is invincible, otherwise false.</returns>
	public static bool IsInvincible(this IStatus status)
	{
		if (status == null)
		{
			return false;
		}

		if (status.GameData.Value.Icon == 15024)
		{
			return true;
		}

		if (OtherConfiguration.InvincibleStatus == null)
		{
			return false;
		}

		foreach (var id in OtherConfiguration.InvincibleStatus)
		{
			if (id == status.StatusId)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Check if the status is a priority.
	/// </summary>
	/// <param name="status">The status to check.</param>
	/// <returns>True if the status is a priority, otherwise false.</returns>
	public static bool IsPriority(this IStatus status)
	{
		if (status == null)
		{
			return false;
		}

		if (OtherConfiguration.PriorityStatus == null)
		{
			return false;
		}

		foreach (var id in OtherConfiguration.PriorityStatus)
		{
			if (id == status.StatusId)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Check if the status needs to be dispelled immediately.
	/// </summary>
	/// <param name="status">The status to check.</param>
	/// <returns>True if the status needs to be dispelled, otherwise false.</returns>
	public static bool IsDangerous(this IStatus status)
	{
		if (status == null)
		{
			return false;
		}

		if (!status.CanDispel())
		{
			return false;
		}

		// Catch all doom statuses that use the Doom icon
		if (status.GameData.Value.Icon == 215503 && status.RemainingTime > 3)
		{
			return true;
		}

		if (status.Param > 2)
		{
			return true;
		}

		if (status.RemainingTime > 20)
		{
			return true;
		}

		if (OtherConfiguration.DangerousStatus == null)
		{
			return false;
		}

		foreach (var id in OtherConfiguration.DangerousStatus)
		{
			if (id == status.StatusId)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Check if the status can be dispelled.
	/// </summary>
	/// <param name="status">The status to check.</param>
	/// <returns>True if the status can be dispelled, otherwise false.</returns>
	public static bool CanDispel(this IStatus status)
	{
		return status != null && status.GameData.Value.CanDispel == true && status.RemainingTime > 1 + DataCenter.DefaultGCDRemain;
	}

	/// <summary>
	/// 
	/// </summary>
	public static bool CanStatusOff(this IStatus status)
	{
		return status != null && status.GameData.Value.CanStatusOff == true && status.RemainingTime > 1 + DataCenter.DefaultGCDRemain;
	}

	/// <summary>
	/// 
	/// </summary>
	public static bool LockActions(this IStatus status)
	{
		return status != null && status.GameData.Value.LockActions == true && status.RemainingTime > 1 + DataCenter.DefaultGCDRemain;
	}

	/// <summary>
	/// 
	/// </summary>
	public static bool LockMovement(this IStatus status)
	{
		return status != null && status.GameData.Value.LockMovement == true && status.RemainingTime > 1 + DataCenter.DefaultGCDRemain;
	}

	/// <summary>
	/// 
	/// </summary>
	public static bool LockControl(this IStatus status)
	{
		return status != null && status.GameData.Value.LockControl == true && status.RemainingTime > 1 + DataCenter.DefaultGCDRemain;
	}

	/// <summary>
	/// Unknown3 is used to determine if the status indicates a tether.
	/// </summary>
	public static bool IsTether(this IStatus status)
	{
		return status != null && status.GameData.Value.Unknown3 == true && status.RemainingTime > 1 + DataCenter.DefaultGCDRemain;
	}
}

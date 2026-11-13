using Dalamud.Interface.Colors;

namespace RotationSolver.Basic.Rotations.Basic;

public partial class SummonerRotation
{
	/// <inheritdoc/>
	public override MedicineType MedicineType => MedicineType.Intelligence;

	private protected sealed override IBaseAction Raise => ResurrectionPvE;

	#region JobGauge

	/// <summary>
	/// 
	/// </summary>
	public static ushort SummonTimerRemaining => JobGauge.SummonTimerRemaining;

	/// <summary>
	/// 
	/// </summary>
	public static ushort AttunementTimerRemaining => JobGauge.AttunementTimerRemaining;

	/// <summary>
	/// 
	/// </summary>
	public static SummonPet ReturnSummon => JobGauge.ReturnSummon;

	/// <summary>
	/// 
	/// </summary>
	public static bool RubyAttunement => JobGauge.AttunementType == SummonAttunement.Ifrit && JobGauge.AttunementCount > 0;

	/// <summary>
	///
	/// </summary>
	public static bool TopazAttunement => JobGauge.AttunementType == SummonAttunement.Titan && JobGauge.AttunementCount > 0;

	/// <summary>
	///
	/// </summary>
	public static bool EmeraldAttunement => JobGauge.AttunementType == SummonAttunement.Garuda && JobGauge.AttunementCount > 0;

	/// <summary>
	/// 
	/// </summary>
	public static byte AttunementCount => JobGauge.AttunementCount;

	/// <summary>
	/// 
	/// </summary>
	public static SummonAttunement AttunementType => JobGauge.AttunementType;

	/// <summary>
	/// 
	/// </summary>
	public static bool GarudaActive => JobGauge.AttunementType == SummonAttunement.Garuda;

	/// <summary>
	/// 
	/// </summary>
	public static bool IfritActive => JobGauge.AttunementType == SummonAttunement.Ifrit;

	/// <summary>
	/// 
	/// </summary>
	public static bool TitanActive => JobGauge.AttunementType == SummonAttunement.Titan;

	/// <summary>
	/// 
	/// </summary>
	public static AetherFlags AetherFlags => JobGauge.AetherFlags;

	/// <summary>
	/// Does the gauge name Demi-Bahamut as the next big summon? Dalamud's own answer only reads the
	/// Phoenix bit and so says yes while the gauge names Solar Bahamut ahead of Demi-Bahamut.
	/// </summary>
	public static bool IsBahamutReady => !JobGauge.IsPhoenixReady && !IsSolarBahamutReady;

	/// <summary>
	/// Does the gauge name Demi-Phoenix as the next big summon? The Phoenix bit is also set while the
	/// gauge names the Solar Bahamut ahead of Phoenix, which Dalamud's own answer counts as Phoenix.
	/// </summary>
	public static bool IsPhoenixReady => JobGauge.IsPhoenixReady && !IsSolarBahamutReady;

	/// <summary>
	/// 
	/// </summary>
	public static bool IsIfritReady => JobGauge.IsIfritReady;

	/// <summary>
	/// 
	/// </summary>
	public static bool IsTitanReady => JobGauge.IsTitanReady;

	/// <summary>
	/// 
	/// </summary>
	public static bool IsGarudaReady => JobGauge.IsGarudaReady;

	/// <summary>
	/// 
	/// </summary>
	public static bool HasAetherflowStacks => JobGauge.HasAetherflowStacks;

	/// <summary>
	/// 
	/// </summary>
	public static byte AetherflowStacks => JobGauge.AetherflowStacks;

	/// <summary>
	/// 
	/// </summary>
	public static bool IsSolarBahamutReady => JobGauge.AetherFlags.HasFlag((AetherFlags)8) || JobGauge.AetherFlags.HasFlag((AetherFlags)12);

	/// <summary>
	/// 
	/// </summary>
	public static bool NoElementalSummon => JobGauge.AttunementType == SummonAttunement.None && !InPhoenix && !InBahamut && !InSolarBahamut;

	/// <summary>
	/// 
	/// </summary>
	public static float SummonTimeRaw => JobGauge.SummonTimerRemaining / 1000f;

	/// <summary>
	/// 
	/// </summary>
	public static float SummonTime => SummonTimeRaw - DataCenter.DefaultGCDRemain;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="time"></param>
	/// <returns></returns>
	protected static bool SummonTimeEndAfter(float time)
	{
		return SummonTime <= time;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="gcdCount"></param>
	/// <param name="offset"></param>
	/// <returns></returns>
	protected static bool SummonTimeEndAfterGCD(uint gcdCount = 0, float offset = 0)
	{
		return SummonTimeEndAfter(GCDTime(gcdCount, offset));
	}

	private static float AttunmentTimeRaw => JobGauge.AttunementTimerRemaining / 1000f;

	/// <summary>
	/// 
	/// </summary>
	public static float AttunmentTime => AttunmentTimeRaw - DataCenter.DefaultGCDRemain;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="time"></param>
	/// <returns></returns>
	protected static bool AttunmentTimeEndAfter(float time)
	{
		return AttunmentTime <= time;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="gcdCount"></param>
	/// <param name="offset"></param>
	/// <returns></returns>
	protected static bool AttunmentTimeEndAfterGCD(uint gcdCount = 0, float offset = 0)
	{
		return AttunmentTimeEndAfter(GCDTime(gcdCount, offset));
	}

	/// <summary>
	/// 
	/// </summary>
	public static bool HasSummon => DataCenter.HasPet() && SummonTimeEndAfterGCD();

	/// <summary>
	/// 
	/// </summary>
	public bool CanBurst => MergedStatus.HasFlag(AutoStatus.Burst) && SearingLightPvE.IsEnabled;

	/// <summary>
	/// 
	/// </summary>
	public bool InBigSummon => !SummonBahamutPvE.EnoughLevel || InBahamut || InPhoenix || InSolarBahamut;

	/// <summary>
	/// 
	/// </summary>
	public static bool NoPrimalReady => !IsIfritReady && !IsGarudaReady && !IsTitanReady;

	/// <summary>
	/// 
	/// </summary>
	public static bool AnyPrimalReady => IsIfritReady || IsGarudaReady || IsTitanReady;

	/// <summary>
	/// 
	/// </summary>
	public static bool HasAnyFavor => HasGarudaFavor || HasIfritFavor || HasTitanFavor;

	/// <summary>
	/// 
	/// </summary>
	public static bool HasAnyAttunement => EmeraldAttunement || RubyAttunement || TopazAttunement;

	/// <summary>
	/// 
	/// </summary>
	public static bool NoAttunement => !RubyAttunement && !EmeraldAttunement && !TopazAttunement;

	/// <summary>
	/// 
	/// </summary>
	public static bool InSolar => DataCenter.PlayerSyncedLevel() == 100 ? !InBahamut && !InPhoenix && InSolarBahamut : InBahamut && !InPhoenix;

	/// <summary>
	/// 
	/// </summary>
	public bool BahamutBurst => ((SummonSolarBahamutPvE.EnoughLevel && InSolarBahamut)
	|| (SummonSolarBahamutPvE.EnoughLevel && (InBahamut || InPhoenix))
	|| (!SummonSolarBahamutPvE.EnoughLevel && InBahamut)
	|| !SummonBahamutPvE.EnoughLevel) && CanBurst;
	#endregion

	#region Status
	/// <summary>
	/// 
	/// </summary>
	public static bool HasFurtherRuin => StatusHelper.PlayerHasStatus(true, StatusID.FurtherRuin_2701);

	/// <summary>
	/// 
	/// </summary>
	public static bool HasCrimsonStrike => StatusHelper.PlayerHasStatus(true, StatusID.CrimsonStrikeReady_4403);

	/// <summary>
	/// 
	/// </summary>
	public static bool HasRadiantAegis => StatusHelper.PlayerHasStatus(true, StatusID.RadiantAegis);

	/// <summary>
	/// 
	/// </summary>
	public static bool HasGarudaFavor => StatusHelper.PlayerHasStatus(true, StatusID.GarudasFavor);

	/// <summary>
	/// 
	/// </summary>
	public static bool HasIfritFavor => StatusHelper.PlayerHasStatus(true, StatusID.IfritsFavor);

	/// <summary>
	/// 
	/// </summary>
	public static bool HasTitanFavor => StatusHelper.PlayerHasStatus(true, StatusID.TitansFavor);

	/// <summary>
	/// 
	/// </summary>
	public static bool HasSearingLight => StatusHelper.PlayerHasStatus(true, StatusID.SearingLight);

	// Per phase, two answers from the last two entries: was it found occupied by a foreign Searing
	// Light last time, and is it held - occupied last time AND this time. The owner's rule: one
	// Summoner getting there first is chance and must not cost a phase; "erst wenn derselbe
	// Beschwörer nach seiner Wiederholzeit erneut dort steht" is a pattern. "Again" is the whole
	// threshold, so no count is kept.
	private readonly bool[] _searingPhaseSeen = new bool[Enum.GetValues<SearingPhase>().Length];
	private readonly bool[] _searingPhaseHeld = new bool[Enum.GetValues<SearingPhase>().Length];
	private SearingPhase _lastSearingPhase = SearingPhase.None;

	#endregion

	#region PvE Actions Unassignable Status

	/// <summary>
	/// 
	/// </summary>
	public static bool InBahamut => Service.GetAdjustedActionId(ActionID.AstralFlowPvE) == ActionID.DeathflarePvE;
	/// <summary>
	/// 
	/// </summary>
	public static bool SummonPhoenixPvEReady => Service.GetAdjustedActionId(ActionID.SummonBahamutPvE) == ActionID.SummonPhoenixPvE;
	/// <summary>
	/// Is the next big summon the burst one - Solar Bahamut where it exists, Demi-Bahamut below?
	/// A cooldown cannot answer this: every big summon comes round on the same 60 s beat.
	///
	/// The game names the next demi twice: on the gauge ("The type of Demi is also indicated and
	/// changes upon summon", The Balance) and on the summon button ("Summon Bahamut changes to Summon
	/// Solar Bahamut when requirements for execution are met"). Neither source says whether the name
	/// is current in the slot ahead of the summon, while its cooldown still runs - and that is where
	/// Searing Light is decided. So each is held against the demi that actually came
	/// (<see cref="UpdateSearingPhaseBook"/>), and one that named the wrong demi there is not asked
	/// again this session: gauge first, then button, then the order (the burst summon follows every
	/// other one, and a fight starts with it).
	///
	/// Either reading used to be enough, so the order overrode the game. It cannot see the game set the
	/// order back, nor a demi summoned while the rotation was not running, and a wrong "yes" is the
	/// costly error: Searing Light goes ahead of a weaker demi and is still cooling down when the burst
	/// summon comes a minute later, which then goes without it. A wrong "no" only lets the buff follow
	/// in the burst phase itself (burstInSolar), a GCD late.
	/// </summary>
	protected bool NextBigSummonIsBurst
	{
		get
		{
			SearingPhase burst;
			if (SummonSolarBahamutPvE.EnoughLevel)
			{
				burst = SearingPhase.Solar;
			}
			else if (SummonPhoenixPvE.EnoughLevel)
			{
				burst = SearingPhase.Bahamut;
			}
			else
			{
				return true;
			}

			var next = !_gaugeMisnamedDemi ? BigSummonOnGauge
				: !_buttonMisnamedDemi ? BigSummonOnButton
				: SearingPhase.None;
			return next != SearingPhase.None ? next == burst : _lastBigSummon != burst;
		}
	}

	private SearingPhase _lastBigSummon = SearingPhase.None;

	// What gauge and button named when the summon first came within the slot ahead of it, and whether
	// either has named another demi than the one that came. How the game keeps both is the same in
	// every fight, so a miss holds for the session.
	private SearingPhase _gaugeNamed = SearingPhase.None;
	private SearingPhase _buttonNamed = SearingPhase.None;

	/// <summary>
	/// 
	/// </summary>
	public static bool InPhoenix => Service.GetAdjustedActionId(ActionID.AstralFlowPvE) == ActionID.RekindlePvE;
	/// <summary>
	/// 
	/// </summary>
	public static bool EnkindlePhoenixPvEReady => Service.GetAdjustedActionId(ActionID.EnkindleBahamutPvE) == ActionID.EnkindlePhoenixPvE;
	/// <summary>
	/// 
	/// </summary>
	public static bool InSolarBahamut => Service.GetAdjustedActionId(ActionID.AstralFlowPvE) == ActionID.SunflarePvE;
	/// <summary>
	/// 
	/// </summary>
	public static bool MountainBusterPvEReady => Service.GetAdjustedActionId(ActionID.AstralFlowPvE) == ActionID.MountainBusterPvE_25836;
	#endregion

	#region Draw Debug

	/// <inheritdoc/>
	public override void DisplayBaseStatus()
	{
		ImGui.Text("ReturnSummon: " + ReturnSummon.ToString());
		ImGui.Text("SummonTime: " + SummonTime.ToString());
		ImGui.Text("HasSummon: " + HasSummon.ToString());
		ImGui.Text("HasPet: " + DataCenter.HasPet().ToString());
		ImGui.Spacing();
		ImGui.Text("HasAetherflowStacks: " + HasAetherflowStacks.ToString());
		ImGui.Text("AetherflowStacks: " + AetherflowStacks.ToString());
		ImGui.Spacing();
		ImGui.TextColored(RubyAttunement ? ImGuiColors.HealerGreen : ImGuiColors.DalamudWhite, "RubyAttunement: " + RubyAttunement.ToString());
		ImGui.TextColored(EmeraldAttunement ? ImGuiColors.HealerGreen : ImGuiColors.DalamudWhite, "EmeraldAttunement: " + EmeraldAttunement.ToString());
		ImGui.TextColored(TopazAttunement ? ImGuiColors.HealerGreen : ImGuiColors.DalamudWhite, "TopazAttunement: " + TopazAttunement.ToString());
		ImGui.Text("AttunementCount: " + AttunementCount.ToString());
		ImGui.Text("AttunmentTime: " + AttunmentTime.ToString());
		ImGui.Spacing();
		ImGui.TextColored(IfritActive ? ImGuiColors.HealerGreen : ImGuiColors.DalamudWhite, "IfritActive: " + IfritActive.ToString());
		ImGui.TextColored(GarudaActive ? ImGuiColors.HealerGreen : ImGuiColors.DalamudWhite, "GarudaActive: " + GarudaActive.ToString());
		ImGui.TextColored(TitanActive ? ImGuiColors.HealerGreen : ImGuiColors.DalamudWhite, "TitanActive: " + TitanActive.ToString());
		ImGui.Text("AttunementType: " + AttunementType.ToString());
		ImGui.Spacing();
		ImGui.TextColored(IsIfritReady ? ImGuiColors.HealerGreen : ImGuiColors.DalamudWhite, "IsIfritReady: " + IsIfritReady.ToString());
		ImGui.TextColored(IsGarudaReady ? ImGuiColors.HealerGreen : ImGuiColors.DalamudWhite, "IsGarudaReady: " + IsGarudaReady.ToString());
		ImGui.TextColored(IsTitanReady ? ImGuiColors.HealerGreen : ImGuiColors.DalamudWhite, "IsTitanReady: " + IsTitanReady.ToString());
		ImGui.Spacing();
		ImGui.TextColored(IsSolarBahamutReady ? ImGuiColors.HealerGreen : ImGuiColors.DalamudWhite, "IsSolarBahamutReady: " + IsSolarBahamutReady.ToString());
		ImGui.TextColored(IsBahamutReady ? ImGuiColors.HealerGreen : ImGuiColors.DalamudWhite, "IsBahamutReady: " + IsBahamutReady.ToString());
		ImGui.TextColored(IsPhoenixReady ? ImGuiColors.HealerGreen : ImGuiColors.DalamudWhite, "IsPhoenixReady: " + IsPhoenixReady.ToString());
		ImGui.Spacing();
		ImGui.TextColored(InSolarBahamut ? ImGuiColors.HealerGreen : ImGuiColors.DalamudWhite, "InSolarBahamut: " + InSolarBahamut.ToString());
		ImGui.TextColored(InBahamut ? ImGuiColors.HealerGreen : ImGuiColors.DalamudWhite, "InBahamut: " + InBahamut.ToString());
		ImGui.TextColored(InPhoenix ? ImGuiColors.HealerGreen : ImGuiColors.DalamudWhite, "InPhoenix: " + InPhoenix.ToString());
		ImGui.Spacing();
		ImGui.Text("Can Heal Single Spell: " + CanHealSingleSpell.ToString());
		ImGui.TextColored(ImGuiColors.DalamudViolet, "PvE Actions");
		ImGui.TextColored(SummonPhoenixPvEReady ? ImGuiColors.HealerGreen : ImGuiColors.DalamudWhite, "SummonPhoenixPvEReady: " + SummonPhoenixPvEReady.ToString());
		ImGui.TextColored(EnkindlePhoenixPvEReady ? ImGuiColors.HealerGreen : ImGuiColors.DalamudWhite, "EnkindlePhoenixPvEReady: " + EnkindlePhoenixPvEReady.ToString());
	}
	#endregion

	#region PvE Actions

	//Class Actions
	static partial void ModifyRuinPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => !InBahamut && !InPhoenix;
	}

	private static RandomDelay _carbuncleDelay = new(() => (2, 2));
	static partial void ModifySummonCarbunclePvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => _carbuncleDelay.Delay(!DataCenter.HasPet() && AttunmentTimeRaw == 0 && SummonTimeRaw == 0) && DataCenter.LastGCD is not ActionID.SummonCarbunclePvE;
	}

	static partial void ModifyRadiantAegisPvE(ref ActionSetting setting)
	{
		// "Can only be executed while Carbuncle is summoned" (effect text), and a demi stands in Carbuncle's
		// place for its 15 s (the owner's hint, A129). The pet check alone let the area defence pick the
		// shield throughout a demi: the game refused it, and Addle behind it in the chain never came (A277).
		// The same gate the demi summons carry.
		setting.ActionCheck = () => DataCenter.HasPet() && SummonTimeRaw <= 0f;
		setting.StatusProvide = [StatusID.RadiantAegis];
		setting.IsFriendly = true;
	}

	static partial void ModifyPhysickPvE(ref ActionSetting setting)
	{
		setting.GCDSingleHeal = true;
	}

	static partial void ModifyAetherchargePvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => InCombat && DataCenter.HasPet();
	}

	static partial void ModifySummonRubyPvE(ref ActionSetting setting)
	{
		setting.StatusProvide = [StatusID.IfritsFavor];
		setting.ActionCheck = () => SummonTime <= WeaponRemain && IsIfritReady;
	}

	static partial void ModifyGemshinePvE(ref ActionSetting setting)
	{

	}

	static partial void ModifyFesterPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => AetherflowStacks > 0;
	}

	static partial void ModifyEnergyDrainPvE(ref ActionSetting setting)
	{
		setting.StatusProvide = [StatusID.FurtherRuin];
		setting.ActionCheck = () => !HasAetherflowStacks;
	}

	static partial void ModifyResurrectionPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => Player?.CurrentMp >= RaiseMPMinimum;
	}

	static partial void ModifySummonTopazPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => SummonTime <= WeaponRemain && IsTitanReady;
		setting.UnlockedByQuestID = 66639;
	}

	static partial void ModifySummonEmeraldPvE(ref ActionSetting setting)
	{
		setting.StatusProvide = [StatusID.GarudasFavor];
		setting.ActionCheck = () => SummonTime <= WeaponRemain && IsGarudaReady;
	}

	static partial void ModifyOutburstPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => !InBahamut && !InPhoenix;
	}

	static partial void ModifyRuinIiPvE(ref ActionSetting setting)
	{
		setting.UnlockedByQuestID = 65997;
		setting.ActionCheck = () => !InBahamut && !InPhoenix;
	}

	// Job Actions

	static partial void ModifySummonIfritPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => SummonTime <= WeaponRemain && IsIfritReady;
		setting.UnlockedByQuestID = 66627;
	}

	static partial void ModifySummonTitanPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => SummonTime <= WeaponRemain && IsTitanReady;
		setting.UnlockedByQuestID = 66628;
	}

	static partial void ModifyPainflarePvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => HasAetherflowStacks;
		setting.UnlockedByQuestID = 66629;
	}

	static partial void ModifySummonGarudaPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => SummonTime <= WeaponRemain && IsGarudaReady;
		setting.UnlockedByQuestID = 66631;
	}

	static partial void ModifyEnergySiphonPvE(ref ActionSetting setting)
	{
		setting.StatusProvide = [StatusID.FurtherRuin];
		setting.ActionCheck = () => !HasAetherflowStacks;
		setting.UnlockedByQuestID = 67637;
	}

	static partial void ModifyRuinIiiPvE(ref ActionSetting setting)
	{
		setting.UnlockedByQuestID = 67638;
		setting.ActionCheck = () => !InBahamut && !InPhoenix;
	}

	static partial void ModifyDreadwyrmTrancePvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => InCombat && DataCenter.HasPet() && SummonTime <= WeaponRemain;
		setting.UnlockedByQuestID = 67640;
	}

	static partial void ModifyAstralFlowPvE(ref ActionSetting setting)
	{
		setting.UnlockedByQuestID = 67641;
	}

	static partial void ModifyRuinIvPvE(ref ActionSetting setting)
	{
		setting.StatusNeed = [StatusID.FurtherRuin_2701];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifySearingLightPvE(ref ActionSetting setting)
	{
		setting.StatusProvide = [StatusID.SearingLight];
		setting.StatusFromSelf = false;
		setting.TargetType = TargetType.Self;
		setting.ActionCheck = () => InCombat;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifySummonBahamutPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => InCombat && DataCenter.HasPet() && SummonTime <= WeaponRemain;
		setting.UnlockedByQuestID = 68165;
	}

	static partial void ModifyEnkindleBahamutPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => InBahamut || InPhoenix;
	}

	static partial void ModifyTridisasterPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => !InBahamut && !InPhoenix;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
	}

	static partial void ModifySummonIfritIiPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => SummonTime <= WeaponRemain && IsIfritReady;
	}

	static partial void ModifySummonTitanIiPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => SummonTime <= WeaponRemain && IsTitanReady;
	}

	static partial void ModifySummonGarudaIiPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => SummonTime <= WeaponRemain && IsGarudaReady;
	}

	static partial void ModifyNecrotizePvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => AetherflowStacks > 0;
	}

	static partial void ModifySearingFlashPvE(ref ActionSetting setting)
	{
		setting.StatusNeed = [StatusID.RubysGlimmer];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyLuxSolarisPvE(ref ActionSetting setting)
	{
		setting.StatusNeed = [StatusID.RefulgentLux];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}
	#endregion

	#region PvE Actions Unassignable
	static partial void ModifyAstralImpulsePvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => InBahamut;
	}

	static partial void ModifyAstralFlarePvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => InBahamut;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
	}

	static partial void ModifyDeathflarePvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => InBahamut;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyWyrmwavePvE(ref ActionSetting setting)
	{

	}

	static partial void ModifyAkhMornPvE(ref ActionSetting setting)
	{
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyRubyRuinPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => AttunementCount > 0 && !AttunmentTimeEndAfter(ActionID.RubyRuinPvE.GetCastTime()) && RubyAttunement;
	}

	static partial void ModifyEmeraldRuinPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => EmeraldAttunement;
	}

	static partial void ModifyTopazRuinPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => TopazAttunement;
	}

	static partial void ModifyRubyRuinIiPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => AttunementCount > 0 && !AttunmentTimeEndAfter(ActionID.RubyRuinIiPvE.GetCastTime()) && RubyAttunement;
	}

	static partial void ModifyEmeraldRuinIiPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => EmeraldAttunement;
	}

	static partial void ModifyTopazRuinIiPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => TopazAttunement;
	}

	static partial void ModifyRubyRuinIiiPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => AttunementCount > 0 && !AttunmentTimeEndAfter(ActionID.RubyRuinIiiPvE.GetCastTime()) && RubyAttunement;
	}

	static partial void ModifyEmeraldRuinIiiPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => EmeraldAttunement;
	}

	static partial void ModifyTopazRuinIiiPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => AttunementCount > 0 && !AttunmentTimeEndAfter(ActionID.TopazRuinIiiPvE.GetCastTime()) && TopazAttunement;
	}

	static partial void ModifyRubyRitePvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => AttunementCount > 0 && !AttunmentTimeEndAfter(ActionID.RubyRitePvE.GetCastTime()) && RubyAttunement;
	}

	static partial void ModifyEmeraldRitePvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => AttunementCount > 0 && !AttunmentTimeEndAfter(ActionID.EmeraldRitePvE.GetCastTime()) && EmeraldAttunement;
	}

	static partial void ModifyTopazRitePvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => AttunementCount > 0 && !AttunmentTimeEndAfter(ActionID.TopazRitePvE.GetCastTime()) && TopazAttunement;
	}

	static partial void ModifyRubyOutburstPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => AttunementCount > 0 && !AttunmentTimeEndAfter(ActionID.RubyOutburstPvE.GetCastTime()) && RubyAttunement;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
	}

	static partial void ModifyEmeraldOutburstPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => AttunementCount > 0 && !AttunmentTimeEndAfter(ActionID.EmeraldOutburstPvE.GetCastTime()) && EmeraldAttunement;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
	}

	static partial void ModifyTopazOutburstPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => AttunementCount > 0 && !AttunmentTimeEndAfter(ActionID.TopazOutburstPvE.GetCastTime()) && TopazAttunement;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
	}

	static partial void ModifyRubyDisasterPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => AttunementCount > 0 && !AttunmentTimeEndAfter(ActionID.RubyDisasterPvE.GetCastTime()) && RubyAttunement;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
	}

	static partial void ModifyEmeraldDisasterPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => AttunementCount > 0 && !AttunmentTimeEndAfter(ActionID.EmeraldDisasterPvE.GetCastTime()) && EmeraldAttunement;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
	}

	static partial void ModifyTopazDisasterPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => AttunementCount > 0 && !AttunmentTimeEndAfter(ActionID.TopazDisasterPvE.GetCastTime()) && TopazAttunement;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
	}

	static partial void ModifyRubyCatastrophePvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => AttunementCount > 0 && !AttunmentTimeEndAfter(ActionID.RubyCatastrophePvE.GetCastTime()) && RubyAttunement;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
	}

	static partial void ModifyEmeraldCatastrophePvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => AttunementCount > 0 && !AttunmentTimeEndAfter(ActionID.EmeraldCatastrophePvE.GetCastTime()) && EmeraldAttunement;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
	}

	static partial void ModifyTopazCatastrophePvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => AttunementCount > 0 && !AttunmentTimeEndAfter(ActionID.TopazCatastrophePvE.GetCastTime()) && TopazAttunement;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
	}

	static partial void ModifySummonPhoenixPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => SummonPhoenixPvEReady;
	}

	static partial void ModifyFountainOfFirePvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => InPhoenix;
	}

	static partial void ModifyBrandOfPurgatoryPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => InPhoenix;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
	}

	static partial void ModifyRekindlePvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => InPhoenix;
	}

	static partial void ModifyEnkindlePhoenixPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => EnkindlePhoenixPvEReady;
	}

	static partial void ModifyEverlastingFlightPvE(ref ActionSetting setting)
	{
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyScarletFlamePvE(ref ActionSetting setting)
	{

	}

	static partial void ModifyRevelationPvE(ref ActionSetting setting)
	{
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyCrimsonCyclonePvE(ref ActionSetting setting)
	{
		setting.SpecialType = SpecialActionType.HostileMovingAttack;
		setting.StatusProvide = [StatusID.CrimsonStrikeReady_4403];
		setting.StatusNeed = [StatusID.IfritsFavor];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyCrimsonStrikePvE(ref ActionSetting setting)
	{
		setting.StatusNeed = [StatusID.CrimsonStrikeReady_4403];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyMountainBusterPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => MountainBusterPvEReady;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifySlipstreamPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => !HasSwift && !StatusHelper.PlayerWillStatusEnd(ActionID.SlipstreamPvE.GetCastTime(), true, StatusID.GarudasFavor)
									|| HasSwift && !StatusHelper.PlayerWillStatusEndGCD(0, 0, true, StatusID.GarudasFavor);
		setting.StatusNeed = [StatusID.GarudasFavor];
		setting.StatusProvide = [StatusID.Slipstream];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifySummonSolarBahamutPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => IsSolarBahamutReady && InCombat && SummonTime <= WeaponRemain;
		setting.UnlockedByQuestID = 68165;

	}

	static partial void ModifyUmbralImpulsePvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => InSolarBahamut;
	}

	static partial void ModifyUmbralFlarePvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => InSolarBahamut;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
	}

	static partial void ModifySunflarePvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => InSolarBahamut;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyEnkindleSolarBahamutPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => InSolarBahamut;
	}

	static partial void ModifyLuxwavePvE(ref ActionSetting setting)
	{

	}

	static partial void ModifyExodusPvE(ref ActionSetting setting)
	{
		setting.ActionCheck = () => InSolarBahamut;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}
	#endregion

	#region PvP Actions
	static partial void ModifyRuinIiiPvP(ref ActionSetting setting)
	{

	}

	static partial void ModifyRuinIvPvP(ref ActionSetting setting)
	{
		setting.ActionCheck = () => Service.GetAdjustedActionId(ActionID.RuinIiiPvP) == ActionID.RuinIvPvP;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyMountainBusterPvP(ref ActionSetting setting)
	{
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
		setting.TargetStatusProvide = [StatusID.Stun_1343];
		setting.StatusProvide = [StatusID.MountainBuster];
	}

	static partial void ModifySlipstreamPvP(ref ActionSetting setting)
	{
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
		setting.TargetStatusProvide = [StatusID.Slipping];
	}

	static partial void ModifyCrimsonCyclonePvP(ref ActionSetting setting)
	{
		setting.SpecialType = SpecialActionType.HostileMovingAttack;
		setting.StatusProvide = [StatusID.CrimsonStrikeReady_4403];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyCrimsonStrikePvP(ref ActionSetting setting)
	{
		setting.ActionCheck = () => Service.GetAdjustedActionId(ActionID.CrimsonCyclonePvP) == ActionID.CrimsonStrikePvP;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyRadiantAegisPvP(ref ActionSetting setting)
	{
		setting.StatusProvide = [StatusID.RadiantAegis_3224];
	}

	static partial void ModifyNecrotizePvP(ref ActionSetting setting)
	{
		setting.StatusProvide = [StatusID.FurtherRuin_4399];
	}

	static partial void ModifyDeathflarePvP(ref ActionSetting setting)
	{
		setting.StatusNeed = [StatusID.DreadwyrmTrance_3228];
		setting.MPOverride = () => 0;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyAstralImpulsePvP(ref ActionSetting setting)
	{
		setting.ActionCheck = () => Service.GetAdjustedActionId(ActionID.RuinIiiPvP) == ActionID.AstralImpulsePvP;
	}

	static partial void ModifyBrandOfPurgatoryPvP(ref ActionSetting setting)
	{
		setting.StatusNeed = [StatusID.FirebirdTrance];
		setting.MPOverride = () => 0;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyFountainOfFirePvP(ref ActionSetting setting)
	{
		setting.ActionCheck = () => Service.GetAdjustedActionId(ActionID.RuinIiiPvP) == ActionID.FountainOfFirePvP;
	}

	static partial void ModifyMegaflarePvP(ref ActionSetting setting)
	{
		setting.CreateConfig = () => new ActionConfig()
		{
			IsEnabled = false,
		};
	}

	static partial void ModifyEverlastingFlightPvP(ref ActionSetting setting)
	{
		setting.CreateConfig = () => new ActionConfig()
		{
			IsEnabled = false,
		};
	}
	#endregion

}

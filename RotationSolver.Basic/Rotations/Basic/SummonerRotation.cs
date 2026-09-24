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
	public static byte Attunement => JobGauge.Attunement;

	/// <summary>
	/// 
	/// </summary>
	public static bool RubyAttunement => JobGauge.Attunement == 5 || JobGauge.Attunement == 9;

	/// <summary>
	/// 
	/// </summary>
	public static bool TopazAttunement => JobGauge.Attunement == 6 || JobGauge.Attunement == 10 || JobGauge.Attunement == 14 || JobGauge.Attunement == 18;

	/// <summary>
	/// 
	/// </summary>
	public static bool EmeraldAttunement => JobGauge.Attunement == 7 || JobGauge.Attunement == 11 || JobGauge.Attunement == 15 || JobGauge.Attunement == 19;

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
	/// 
	/// </summary>
	public static bool IsBahamutReady => JobGauge.IsBahamutReady;

	/// <summary>
	/// 
	/// </summary>
	public static bool IsPhoenixReady => JobGauge.IsPhoenixReady;

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
	public static bool NoElementalSummon => JobGauge.Attunement == 0 && !InPhoenix && !InBahamut && !InSolarBahamut;

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

	/// <summary>
	/// Is a Searing Light on the player at all, no matter who cast it?
	///
	/// Searing Light does not stack, it overwrites, and it raises damage by the same 5% whoever it
	/// came from. <see cref="HasSearingLight"/> counts the player's own buff alone, which is the
	/// right question for "may I cast it" and the wrong one for "am I standing in a buff window".
	/// </summary>
	public static bool HasAnySearingLight => StatusHelper.PlayerHasStatus(false, StatusID.SearingLight);

	/// <summary>
	/// Which burst phase the Summoner is standing in, if any. Kinds, not positions: Solar occupies
	/// two of the four demi slots in a cycle, and whoever holds it holds both.
	/// </summary>
	protected enum SearingPhase
	{
		/// <summary>Outside every demi window.</summary>
		None,

		/// <summary>Solar Bahamut.</summary>
		Solar,

		/// <summary>Demi-Bahamut.</summary>
		Bahamut,

		/// <summary>Demi-Phoenix.</summary>
		Phoenix,
	}

	/// <inheritdoc cref="SearingPhase"/>
	protected static SearingPhase CurrentSearingPhase =>
		InPhoenix ? SearingPhase.Phoenix
		: InBahamut ? SearingPhase.Bahamut
		: InSolarBahamut ? SearingPhase.Solar
		: SearingPhase.None;

	// Per phase, two answers from the last two entries: was it found occupied by a foreign Searing
	// Light last time, and is it held - occupied last time AND this time. The owner's rule: one
	// Summoner getting there first is chance and must not cost a phase; "erst wenn derselbe
	// Beschwörer nach seiner Wiederholzeit erneut dort steht" is a pattern. "Again" is the whole
	// threshold, so no count is kept.
	private readonly bool[] _searingPhaseSeen = new bool[Enum.GetValues<SearingPhase>().Length];
	private readonly bool[] _searingPhaseHeld = new bool[Enum.GetValues<SearingPhase>().Length];
	private SearingPhase _lastSearingPhase = SearingPhase.None;
	private bool _searingPhaseBooked;

	/// <summary>
	/// Is every burst phase held by somebody who keeps coming back?
	///
	/// This is the only question the book answers, and the distinction matters: it does **not** say
	/// which phase to aim at. Holding back applies to the phase at hand, not as a matter of
	/// principle - so every free burst phase is fair game whenever no buff is running, and only
	/// when all three are spoken for is there a reason to look outside them at all.
	///
	/// Only the phases this level has are asked. Solar Bahamut does not exist below its level and
	/// Phoenix not below its own, so requiring them made the answer "no" in every level-synced duty,
	/// and the fallback into a primal block could never be reached there.
	/// </summary>
	protected bool AllSearingPhasesHeld
	{
		get
		{
			if (!SummonBahamutPvE.EnoughLevel)
			{
				return false;
			}

			if (!SearingPhaseHeld(SearingPhase.Bahamut))
			{
				return false;
			}

			if (SummonSolarBahamutPvE.EnoughLevel)
			{
				return SearingPhaseHeld(SearingPhase.Solar);
			}

			return !SummonPhoenixPvE.EnoughLevel || SearingPhaseHeld(SearingPhase.Phoenix);
		}
	}

	/// <summary>
	/// Was this phase found occupied by a foreign Searing Light on the last two entries?
	/// </summary>
	protected bool SearingPhaseHeld(SearingPhase phase) => _searingPhaseHeld[(int)BookSlot(phase)];

	/// <summary>
	/// Was this phase found occupied on the last entry - one sighting, not yet a pattern? For the
	/// rotation status, so the tester can see what the rule decides from.
	/// </summary>
	protected bool SearingPhaseSeen(SearingPhase phase) => _searingPhaseSeen[(int)BookSlot(phase)];

	/// <summary>
	/// Where a phase is booked. With Solar Bahamut in the cycle, Demi-Bahamut and Demi-Phoenix take
	/// the windows between two Solars in turn, and Searing Light's 120 s recast is two windows - so a
	/// Summoner seen in one of them returns in the other, and both are spoken for (concept 12,
	/// "Die Buchführung"). Booking them apart needed two sightings of each, every 240 s, before
	/// either counted as held. Below Solar's level the two alternate every window and a caster
	/// returns to the same one, so they are booked apart.
	/// </summary>
	private SearingPhase BookSlot(SearingPhase phase) =>
		phase == SearingPhase.Phoenix && SummonSolarBahamutPvE.EnoughLevel
			? SearingPhase.Bahamut
			: phase;

	/// <summary>
	/// Keeps the phase book, once per window entered rather than once per frame.
	///
	/// Entering a burst phase while somebody else's Searing Light is running marks it seen, and held
	/// if it was seen the time before as well; entering it and finding no foreign buff clears both. That is
	/// what makes the book self-healing without a clock: a Summoner who stops casting - died,
	/// left, switched job - stops being found there, and his phase comes back on the next pass. No
	/// grace period has to be guessed, and no reset point beyond leaving combat is needed.
	/// </summary>
	protected void UpdateSearingPhaseBook()
	{
		if (!DataCenter.InCombat)
		{
			Array.Clear(_searingPhaseSeen, 0, _searingPhaseSeen.Length);
			Array.Clear(_searingPhaseHeld, 0, _searingPhaseHeld.Length);
			_lastSearingPhase = SearingPhase.None;
			_lastBigSummon = SearingPhase.None;
			_searingPhaseBooked = false;
			return;
		}

		var phase = CurrentSearingPhase;
		if (phase != _lastSearingPhase)
		{
			_lastSearingPhase = phase;
			_searingPhaseBooked = false;
			if (phase != SearingPhase.None)
			{
				_lastBigSummon = phase;
			}
		}

		if (phase == SearingPhase.None || _searingPhaseBooked)
		{
			return;
		}

		// The player's own buff says nothing about whether anyone else holds this phase, so it
		// neither books nor clears - the window is simply left unjudged and asked again later in the
		// same phase. Without this a charge spent in Solar would clear the Bahamut entry on entry,
		// on the strength of a buff the player cast himself.
		if (HasSearingLight)
		{
			return;
		}

		_searingPhaseBooked = true;
		var slot = (int)BookSlot(phase);
		var occupied = HasAnySearingLight;
		_searingPhaseHeld[slot] = occupied && _searingPhaseSeen[slot];
		_searingPhaseSeen[slot] = occupied;
	}

	/// <inheritdoc/>
	protected override void UpdateInfo()
	{
		base.UpdateInfo();
		UpdateSearingPhaseBook();
	}

	/// <summary>
	/// Is there another Summoner in the party who could cast Searing Light?
	///
	/// Alliance members are not asked: Searing Light reaches nearby party members, so a Summoner in
	/// another alliance party never buffs this player. The dead are not asked either - they cast
	/// nothing. The level comes from the action's own data rather than a literal, and it is applied
	/// to the other player for the same reason it is applied here: a Summoner below it has no
	/// Searing Light to give, and counting him would hold this one back for a buff that cannot come.
	/// That is the mistake <see cref="DataCenter.AnyLivingRaiser(bool)"/> made with Red Mage.
	///
	/// What this cannot answer is whether the other Summoner runs this rotation, runs any plugin at
	/// all, or plays the job well. It answers "can a second Searing Light exist here", which is the
	/// question the firing window turns on.
	/// </summary>
	protected bool AnotherSummonerInParty
	{
		get
		{
			var members = DataCenter.PartyMembers;
			if (members == null)
			{
				return false;
			}

			foreach (var member in members)
			{
				if (member == null || member.IsDead || member.IsPlayer())
				{
					continue;
				}

				if (member.IsJobs(ECommons.ExcelServices.Job.SMN)
					&& member.Level >= SearingLightPvE.Level)
				{
					return true;
				}
			}

			return false;
		}
	}

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
	/// Read from the summon button itself, which the game turns into the summon that comes next
	/// ("Summon Bahamut changes to Summon Solar Bahamut when requirements for execution are met").
	/// A cooldown cannot answer this: every big summon comes round on the same 60 s beat.
	///
	/// Two readings, either is enough. The button is the game's own answer, but whether it turns
	/// before the summon's cooldown has run out is not stated anywhere - and the slot ahead of the
	/// summon is exactly that moment. So the order is read as well: the burst summon follows every
	/// other one (Solar, Bahamut, Solar, Phoenix; below Solar, Bahamut and Phoenix in turn), and a
	/// fight starts with it. A wrong "yes" only lets Searing Light go ahead of a weaker demi, as it did
	/// before this was asked at all; a wrong "no" would hold the summon for a buff that never comes.
	/// </summary>
	protected bool NextBigSummonIsBurst
	{
		get
		{
			var next = Service.GetAdjustedActionId(ActionID.SummonBahamutPvE);
			if (SummonSolarBahamutPvE.EnoughLevel)
			{
				return next == ActionID.SummonSolarBahamutPvE || _lastBigSummon != SearingPhase.Solar;
			}

			if (SummonPhoenixPvE.EnoughLevel)
			{
				return next == ActionID.SummonBahamutPvE || _lastBigSummon != SearingPhase.Bahamut;
			}

			return true;
		}
	}

	private SearingPhase _lastBigSummon = SearingPhase.None;
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
		ImGui.Text("Attunement: " + Attunement.ToString());
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
		setting.ActionCheck = () => DataCenter.HasPet();
		setting.StatusProvide = [StatusID.RadiantAegis];
		setting.IsFriendly = true;
	}

	static partial void ModifyPhysickPvE(ref ActionSetting setting)
	{
		setting.CreateConfig = () => new ActionConfig()
		{
			GCDSingleHeal = true,
		};
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

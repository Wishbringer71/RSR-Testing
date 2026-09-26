# BRD — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `BardRotation`
- Rotation: `RotationSolver/RebornRotations/Ranged/BRD_Reborn.cs`
- Matrix als Tabelle: `BRD.csv`

## Nutzung

direkt: 39 · ungenutzt: 2

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Fernkämpfer | Arm's Length (`ArmsLengthPvE`) | 7548 | Ability | direkt |
| Fernkämpfer | Foot Graze (`FootGrazePvE`) | 7553 | Ability | ungenutzt |
| Fernkämpfer | Head Graze (`HeadGrazePvE`) | 7551 | Ability | direkt |
| Fernkämpfer | Leg Graze (`LegGrazePvE`) | 7554 | Ability | ungenutzt |
| Fernkämpfer | Peloton (`PelotonPvE`) | 7557 | Ability | direkt |
| Fernkämpfer | Second Wind (`SecondWindPvE`) | 7541 | Ability | direkt |
| Job | Apex Arrow (`ApexArrowPvE`) | 16496 | Weaponskill | direkt |
| Job | Army's Paeon (`ArmysPaeonPvE`) | 116 | Ability | direkt |
| Job | Barrage (`BarragePvE`) | 107 | Ability | direkt |
| Job | Battle Voice (`BattleVoicePvE`) | 118 | Ability | direkt |
| Job | Blast Arrow (`BlastArrowPvE`) | 25784 | Weaponskill | direkt |
| Job | Bloodletter (`BloodletterPvE`) | 110 | Ability | direkt |
| Job | Burst Shot (`BurstShotPvE`) | 16495 | Weaponskill | direkt |
| Job | Caustic Bite (`CausticBitePvE`) | 7406 | Weaponskill | direkt |
| Job | Empyreal Arrow (`EmpyrealArrowPvE`) | 3558 | Ability | direkt |
| Job | Heartbreak Shot (`HeartbreakShotPvE`) | 36975 | Ability | direkt |
| Job | Heavy Shot (`HeavyShotPvE`) | 97 | Weaponskill | direkt |
| Job | Iron Jaws (`IronJawsPvE`) | 3560 | Weaponskill | direkt |
| Job | Ladonsbite (`LadonsbitePvE`) | 25783 | Weaponskill | direkt |
| Job | Mage's Ballad (`MagesBalladPvE`) | 114 | Ability | direkt |
| Job | Nature's Minne (`NaturesMinnePvE`) | 7408 | Ability | direkt |
| Job | Pitch Perfect (`PitchPerfectPvE`) | 7404 | Ability | direkt |
| Job | Quick Nock (`QuickNockPvE`) | 106 | Weaponskill | direkt |
| Job | Radiant Encore (`RadiantEncorePvE`) | 36977 | Weaponskill | direkt |
| Job | Radiant Finale (`RadiantFinalePvE`) | 25785 | Ability | direkt |
| Job | Raging Strikes (`RagingStrikesPvE`) | 101 | Ability | direkt |
| Job | Rain of Death (`RainOfDeathPvE`) | 117 | Ability | direkt |
| Job | Refulgent Arrow (`RefulgentArrowPvE`) | 7409 | Weaponskill | direkt |
| Job | Repelling Shot (`RepellingShotPvE`) | 112 | Ability | direkt |
| Job | Resonant Arrow (`ResonantArrowPvE`) | 36976 | Weaponskill | direkt |
| Job | Shadowbite (`ShadowbitePvE`) | 16494 | Weaponskill | direkt |
| Job | Sidewinder (`SidewinderPvE`) | 3562 | Ability | direkt |
| Job | Stormbite (`StormbitePvE`) | 7407 | Weaponskill | direkt |
| Job | Straight Shot (`StraightShotPvE`) | 98 | Weaponskill | direkt |
| Job | Troubadour (`TroubadourPvE`) | 7405 | Ability | direkt |
| Job | Venomous Bite (`VenomousBitePvE`) | 100 | Weaponskill | direkt |
| Job | Wide Volley (`WideVolleyPvE`) | 36974 | Weaponskill | direkt |
| Job | Windbite (`WindbitePvE`) | 113 | Weaponskill | direkt |
| Job | the Wanderer's Minuet (`TheWanderersMinuetPvE`) | 3559 | Ability | direkt |
| Job | the Warden's Paean (`TheWardensPaeanPvE`) | 3561 | Ability | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Apex Arrow | Knopf wird zu | Blast Arrow |
| Apex Arrow | kostet | Soul Voice |
| Army's Paeon | braucht (Erzeuger nicht im Text) | combat |
| Blast Arrow | braucht Blast Arrow Ready | Eigenschaft Enhanced Apex Arrow |
| Bloodletter | Ausbau (Bloodletter Mastery) | Heartbreak Shot |
| Heartbreak Shot | gemeinsame Abklingzeit | Rain of Death |
| Heavy Shot | Ausbau (Heavy Shot Mastery) | Burst Shot |
| Mage's Ballad | braucht (Erzeuger nicht im Text) | combat |
| Quick Nock | Ausbau (Quick Nock Mastery) | Ladonsbite |
| Radiant Encore | braucht Radiant Encore Ready | Eigenschaft Enhanced Radiant Finale |
| Refulgent Arrow | braucht Barrage | Barrage |
| Refulgent Arrow | braucht (Erzeuger nicht im Text) | Hawk's Eye |
| Resonant Arrow | braucht Resonant Arrow Ready | Eigenschaft Enhanced Barrage |
| Shadowbite | braucht Barrage | Barrage |
| Shadowbite | braucht (Erzeuger nicht im Text) | Hawk's Eye |
| Straight Shot | braucht (Erzeuger nicht im Text) | Hawk's Eye |
| Straight Shot | Ausbau (Straight Shot Mastery) | Refulgent Arrow |
| the Wanderer's Minuet | braucht (Erzeuger nicht im Text) | combat |
| Venomous Bite | Ausbau (Heavy Shot</strong></see> <i>PvE</i> (ARC BRD) [97] [Weaponskill]
    /// </summary>
    static partial void ModifyHeavyShotPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/97"><strong>Heavy Shot</strong></see> <i>PvE</i> (ARC BRD) [97] [Weaponskill]
    /// <para>Delivers an attack with a potency of 160.</para>
    /// </summary>
    
    public IBaseAction HeavyShotPvE => _HeavyShotPvECreator.Value;
    private readonly Lazy<IBaseAction> _StraightShotPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)98, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyStraightShotPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/98"><strong>Straight Shot</strong></see> <i>PvE</i> (ARC BRD) [98] [Weaponskill]
    /// </summary>
    static partial void ModifyStraightShotPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/98"><strong>Straight Shot</strong></see> <i>PvE</i> (ARC BRD) [98] [Weaponskill]
    /// <para>Delivers an attack with a potency of 200. Can only be executed when under the effect of Hawk's Eye.</para>
    /// </summary>
    
    public IBaseAction StraightShotPvE => _StraightShotPvECreator.Value;
    private readonly Lazy<IBaseAction> _VenomousBitePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)100, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyVenomousBitePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/100"><strong>Venomous Bite</strong></see> <i>PvE</i> (ARC BRD) [100] [Weaponskill]
    /// </summary>
    static partial void ModifyVenomousBitePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/100"><strong>Venomous Bite</strong></see> <i>PvE</i> (ARC BRD) [100] [Weaponskill]
    /// <para>Delivers an attack with a potency of 100. Additional Effect: Venom Potency: 15 Duration: 45s</para>
    /// </summary>
    
    public IBaseAction VenomousBitePvE => _VenomousBitePvECreator.Value;
    private readonly Lazy<IBaseAction> _RagingStrikesPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)101, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRagingStrikesPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/101"><strong>Raging Strikes</strong></see> <i>PvE</i> (ARC BRD) [101] [Ability]
    /// </summary>
    static partial void ModifyRagingStrikesPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/101"><strong>Raging Strikes</strong></see> <i>PvE</i> (ARC BRD) [101] [Ability]
    /// <para>Increases damage dealt by 15%. Duration: 20s</para>
    /// </summary>
    
    public IBaseAction RagingStrikesPvE => _RagingStrikesPvECreator.Value;
    private readonly Lazy<IBaseAction> _QuickNockPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)106, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyQuickNockPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/106"><strong>Quick Nock</strong></see> <i>PvE</i> (ARC BRD) [106] [Weaponskill]
    /// </summary>
    static partial void ModifyQuickNockPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/106"><strong>Quick Nock</strong></see> <i>PvE</i> (ARC BRD) [106] [Weaponskill]
    /// <para>Delivers an attack with a potency of 110 to all enemies in a cone before you.</para>
    /// </summary>
    
    public IBaseAction QuickNockPvE => _QuickNockPvECreator.Value;
    private readonly Lazy<IBaseAction> _BarragePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)107, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBarragePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/107"><strong>Barrage</strong></see> <i>PvE</i> (ARC BRD) [107] [Ability]
    /// </summary>
    static partial void ModifyBarragePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/107"><strong>Barrage</strong></see> <i>PvE</i> (ARC BRD) [107] [Ability]
    /// <para>Grants Barrage allowing the use of and . Upon execution will strike the selected target three times while 's potency will be increased to . Duration: 10s</para>
    /// </summary>
    
    public IBaseAction BarragePvE => _BarragePvECreator.Value;
    private readonly Lazy<IBaseAction> _BloodletterPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)110, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBloodletterPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/110"><strong>Bloodletter</strong></see> <i>PvE</i> (ARC BRD) [110] [Ability]
    /// </summary>
    static partial void ModifyBloodletterPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/110"><strong>Bloodletter</strong></see> <i>PvE</i> (ARC BRD) [110] [Ability]
    /// <para>Delivers an attack with a potency of 130. Maximum Charges:</para>
    /// </summary>
    
    public IBaseAction BloodletterPvE => _BloodletterPvECreator.Value;
    private readonly Lazy<IBaseAction> _RepellingShotPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)112, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRepellingShotPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/112"><strong>Repelling Shot</strong></see> <i>PvE</i> (ARC BRD) [112] [Ability]
    /// </summary>
    static partial void ModifyRepellingShotPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/112"><strong>Repelling Shot</strong></see> <i>PvE</i> (ARC BRD) [112] [Ability]
    /// <para>Jump 10 yalms away from current target. Cannot be executed while bound.</para>
    /// </summary>
    
    public IBaseAction RepellingShotPvE => _RepellingShotPvECreator.Value;
    private readonly Lazy<IBaseAction> _WindbitePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)113, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyWindbitePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/113"><strong>Windbite</strong></see> <i>PvE</i> (ARC BRD) [113] [Weaponskill]
    /// </summary>
    static partial void ModifyWindbitePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/113"><strong>Windbite</strong></see> <i>PvE</i> (ARC BRD) [113] [Weaponskill]
    /// <para>Deals wind damage with a potency of 60. Additional Effect: Wind damage over time Potency: 20 Duration: 45s</para>
    /// </summary>
    
    public IBaseAction WindbitePvE => _WindbitePvECreator.Value;
    private readonly Lazy<IBaseAction> _MagesBalladPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)114, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyMagesBalladPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/114"><strong>Mage's Ballad</strong></see> <i>PvE</i> (BRD) [114] [Ability]
    /// </summary>
    static partial void ModifyMagesBalladPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/114"><strong>Mage's Ballad</strong></see> <i>PvE</i> (BRD) [114] [Ability]
    /// <para>Grants Mage's Ballad to self and all party members within 50 yalms increasing damage dealt by 1%. Duration: 45s Additional Effect: 80% chance to grant Repertoire This effect can trigger repeatedly while singing the Mage's Ballad. Repertoire Effect: Reduces the recast time of by 7.5s Can only be executed while in combat.</para>
    /// </summary>
    
    public IBaseAction MagesBalladPvE => _MagesBalladPvECreator.Value;
    private readonly Lazy<IBaseAction> _ArmysPaeonPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)116, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyArmysPaeonPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/116"><strong>Army's Paeon</strong></see> <i>PvE</i> (BRD) [116] [Ability]
    /// </summary>
    static partial void ModifyArmysPaeonPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/116"><strong>Army's Paeon</strong></see> <i>PvE</i> (BRD) [116] [Ability]
    /// <para>Grants Army's Paeon to self and all party members within 50 yalms increasing direct hit rate by 3%. Duration: 45s Additional Effect: 80% chance to grant Repertoire This effect can trigger repeatedly while singing the Army's Paeon. Repertoire Effect: Reduces weaponskill cast time and recast time spell cast time and recast time and auto-attack delay by 4% Can be stacked up to 4 times. Can only be executed while in combat.</para>
    /// </summary>
    
    public IBaseAction ArmysPaeonPvE => _ArmysPaeonPvECreator.Value;
    private readonly Lazy<IBaseAction> _RainOfDeathPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)117, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRainOfDeathPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/117"><strong>Rain of Death</strong></see> <i>PvE</i> (BRD) [117] [Ability]
    /// </summary>
    static partial void ModifyRainOfDeathPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/117"><strong>Rain of Death</strong></see> <i>PvE</i> (BRD) [117] [Ability]
    /// <para>Delivers an attack with a potency of 100 to target and all enemies nearby it. Maximum Charges: Shares a recast timer with .</para>
    /// </summary>
    
    public IBaseAction RainOfDeathPvE => _RainOfDeathPvECreator.Value;
    private readonly Lazy<IBaseAction> _BattleVoicePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)118, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBattleVoicePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/118"><strong>Battle Voice</strong></see> <i>PvE</i> (BRD) [118] [Ability]
    /// </summary>
    static partial void ModifyBattleVoicePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/118"><strong>Battle Voice</strong></see> <i>PvE</i> (BRD) [118] [Ability]
    /// <para>Increases direct hit rate of self and all nearby party members by 20%. Duration: 20s</para>
    /// </summary>
    
    public IBaseAction BattleVoicePvE => _BattleVoicePvECreator.Value;
    private readonly Lazy<IBaseAction> _EmpyrealArrowPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3558, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEmpyrealArrowPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3558"><strong>Empyreal Arrow</strong></see> <i>PvE</i> (BRD) [3558] [Ability]
    /// </summary>
    static partial void ModifyEmpyrealArrowPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3558"><strong>Empyreal Arrow</strong></see> <i>PvE</i> (BRD) [3558] [Ability]
    /// <para>Delivers an attack with a potency of .</para>
    /// </summary>
    
    public IBaseAction EmpyrealArrowPvE => _EmpyrealArrowPvECreator.Value;
    private readonly Lazy<IBaseAction> _TheWanderersMinuetPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3559, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyTheWanderersMinuetPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3559"><strong>the Wanderer's Minuet</strong></see> <i>PvE</i> (BRD) [3559] [Ability]
    /// </summary>
    static partial void ModifyTheWanderersMinuetPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3559"><strong>the Wanderer's Minuet</strong></see> <i>PvE</i> (BRD) [3559] [Ability]
    /// <para>Grants the Wanderer's Minuet to self and all party members within 50 yalms increasing critical hit rate by 2%. Duration: 45s Additional Effect: 80% chance to grant Repertoire This effect can trigger repeatedly while singing the Wanderer's Minuet. Repertoire Effect: Allows execution of Pitch Perfect Can be stacked up to 3 times. Can only be executed while in combat.</para>
    /// </summary>
    
    public IBaseAction TheWanderersMinuetPvE => _TheWanderersMinuetPvECreator.Value;
    private readonly Lazy<IBaseAction> _IronJawsPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3560, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyIronJawsPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3560"><strong>Iron Jaws</strong></see> <i>PvE</i> (BRD) [3560] [Weaponskill]
    /// </summary>
    static partial void ModifyIronJawsPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3560"><strong>Iron Jaws</strong></see> <i>PvE</i> (BRD) [3560] [Weaponskill]
    /// <para>Delivers an attack with a potency of 100. Additional Effect: If the target is suffering from a effect inflicted by you the effect timer is reset</para>
    /// </summary>
    
    public IBaseAction IronJawsPvE => _IronJawsPvECreator.Value;
    private readonly Lazy<IBaseAction> _TheWardensPaeanPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3561, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyTheWardensPaeanPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3561"><strong>the Warden's Paean</strong></see> <i>PvE</i> (BRD) [3561] [Ability]
    /// </summary>
    static partial void ModifyTheWardensPaeanPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3561"><strong>the Warden's Paean</strong></see> <i>PvE</i> (BRD) [3561] [Ability]
    /// <para>Removes one select detrimental effect from self or target party member. If the target is not enfeebled a barrier is created nullifying the target's next detrimental effect suffered. Duration: 30s</para>
    /// </summary>
    
    public IBaseAction TheWardensPaeanPvE => _TheWardensPaeanPvECreator.Value;
    private readonly Lazy<IBaseAction> _SidewinderPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3562, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifySidewinderPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3562"><strong>Sidewinder</strong></see> <i>PvE</i> (BRD) [3562] [Ability]
    /// </summary>
    static partial void ModifySidewinderPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3562"><strong>Sidewinder</strong></see> <i>PvE</i> (BRD) [3562] [Ability]
    /// <para>Delivers an attack with a potency of .</para>
    /// </summary>
    
    public IBaseAction SidewinderPvE => _SidewinderPvECreator.Value;
    private readonly Lazy<IBaseAction> _PitchPerfectPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7404, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyPitchPerfectPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7404"><strong>Pitch Perfect</strong></see> <i>PvE</i> (BRD) [7404] [Ability]
    /// </summary>
    static partial void ModifyPitchPerfectPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7404"><strong>Pitch Perfect</strong></see> <i>PvE</i> (BRD) [7404] [Ability]
    /// <para>Delivers an attack to the target and all enemies nearby it. Potency varies with number of Repertoire stacks dealing full damage for the first enemy and 50% less for all remaining enemies. 1 Repertoire Stack: 100 2 Repertoire Stacks: 220 3 Repertoire Stacks: 360 Can only be executed when the Wanderer's Minuet is active and you have at least one stack of Repertoire.</para>
    /// </summary>
    
    public IBaseAction PitchPerfectPvE => _PitchPerfectPvECreator.Value;
    private readonly Lazy<IBaseAction> _TroubadourPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7405, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyTroubadourPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7405"><strong>Troubadour</strong></see> <i>PvE</i> (BRD) [7405] [Ability]
    /// </summary>
    static partial void ModifyTroubadourPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7405"><strong>Troubadour</strong></see> <i>PvE</i> (BRD) [7405] [Ability]
    /// <para>Reduces damage taken by self and nearby party members by %. Duration: 15s Effect cannot be stacked with machinist's Tactician or dancer's Shield Samba.</para>
    /// </summary>
    
    public IBaseAction TroubadourPvE => _TroubadourPvECreator.Value;
    private readonly Lazy<IBaseAction> _CausticBitePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7406, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyCausticBitePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7406"><strong>Caustic Bite</strong></see> <i>PvE</i> (BRD) [7406] [Weaponskill]
    /// </summary>
    static partial void ModifyCausticBitePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7406"><strong>Caustic Bite</strong></see> <i>PvE</i> (BRD) [7406] [Weaponskill]
    /// <para>Delivers an attack with a potency of 150. Additional Effect: Poison Potency: 20 Duration: 45s</para>
    /// </summary>
    
    public IBaseAction CausticBitePvE => _CausticBitePvECreator.Value;
    private readonly Lazy<IBaseAction> _StormbitePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7407, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyStormbitePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7407"><strong>Stormbite</strong></see> <i>PvE</i> (BRD) [7407] [Weaponskill]
    /// </summary>
    static partial void ModifyStormbitePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7407"><strong>Stormbite</strong></see> <i>PvE</i> (BRD) [7407] [Weaponskill]
    /// <para>Deals wind damage with a potency of 100. Additional Effect: Wind damage over time Potency: 25 Duration: 45s</para>
    /// </summary>
    
    public IBaseAction StormbitePvE => _StormbitePvECreator.Value;
    private readonly Lazy<IBaseAction> _NaturesMinnePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7408, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyNaturesMinnePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7408"><strong>Nature's Minne</strong></see> <i>PvE</i> (BRD) [7408] [Ability]
    /// </summary>
    static partial void ModifyNaturesMinnePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7408"><strong>Nature's Minne</strong></see> <i>PvE</i> (BRD) [7408] [Ability]
    /// <para>Increases HP recovery via healing actions by 15% for self and nearby party members. Duration: 15s</para>
    /// </summary>
    
    public IBaseAction NaturesMinnePvE => _NaturesMinnePvECreator.Value;
    private readonly Lazy<IBaseAction> _RefulgentArrowPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7409, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRefulgentArrowPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7409"><strong>Refulgent Arrow</strong></see> <i>PvE</i> (BRD) [7409] [Weaponskill]
    /// </summary>
    static partial void ModifyRefulgentArrowPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7409"><strong>Refulgent Arrow</strong></see> <i>PvE</i> (BRD) [7409] [Weaponskill]
    /// <para>Delivers an attack with a potency of . Can only be executed when under the effect of Hawk's Eye or Barrage.</para>
    /// </summary>
    
    public IBaseAction RefulgentArrowPvE => _RefulgentArrowPvECreator.Value;
    private readonly Lazy<IBaseAction> _ShadowbitePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16494, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyShadowbitePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16494"><strong>Shadowbite</strong></see> <i>PvE</i> (BRD) [16494] [Weaponskill]
    /// </summary>
    static partial void ModifyShadowbitePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16494"><strong>Shadowbite</strong></see> <i>PvE</i> (BRD) [16494] [Weaponskill]
    /// <para>Delivers an attack with a potency of 200 to target and all enemies nearby it. Barrage Potency: 300 Can only be executed when under the effect of Hawk's Eye or Barrage.</para>
    /// </summary>
    
    public IBaseAction ShadowbitePvE => _ShadowbitePvECreator.Value;
    private readonly Lazy<IBaseAction> _BurstShotPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16495, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBurstShotPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16495"><strong>Burst Shot</strong></see> <i>PvE</i> (BRD) [16495] [Weaponskill]
    /// </summary>
    static partial void ModifyBurstShotPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16495"><strong>Burst Shot</strong></see> <i>PvE</i> (BRD) [16495] [Weaponskill]
    /// <para>Delivers an attack with a potency of . Additional Effect: 35% chance of granting Hawk's Eye Duration: 30s</para>
    /// </summary>
    
    public IBaseAction BurstShotPvE => _BurstShotPvECreator.Value;
    private readonly Lazy<IBaseAction> _ApexArrowPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16496, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyApexArrowPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16496"><strong>Apex Arrow</strong></see> <i>PvE</i> (BRD) [16496] [Weaponskill]
    /// </summary>
    static partial void ModifyApexArrowPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16496"><strong>Apex Arrow</strong></see> <i>PvE</i> (BRD) [16496] [Weaponskill]
    /// <para>Delivers an attack with a potency of to all enemies in a straight line before you. Soul Voice Gauge Cost: 20 Potency increases up to as Soul Voice Gauge exceeds minimum cost. Consumes Soul Voice Gauge upon execution.</para>
    /// </summary>
    
    public IBaseAction ApexArrowPvE => _ApexArrowPvECreator.Value;
    private readonly Lazy<IBaseAction> _LadonsbitePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25783, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyLadonsbitePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25783"><strong>Ladonsbite</strong></see> <i>PvE</i> (BRD) [25783] [Weaponskill]
    /// </summary>
    static partial void ModifyLadonsbitePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25783"><strong>Ladonsbite</strong></see> <i>PvE</i> (BRD) [25783] [Weaponskill]
    /// <para>Delivers an attack with a potency of 140 to all enemies in a cone before you. Additional Effect: 35% chance of granting Hawk's Eye Duration: 30s</para>
    /// </summary>
    
    public IBaseAction LadonsbitePvE => _LadonsbitePvECreator.Value;
    private readonly Lazy<IBaseAction> _BlastArrowPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25784, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBlastArrowPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25784"><strong>Blast Arrow</strong></see> <i>PvE</i> (BRD) [25784] [Weaponskill]
    /// </summary>
    static partial void ModifyBlastArrowPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25784"><strong>Blast Arrow</strong></see> <i>PvE</i> (BRD) [25784] [Weaponskill]
    /// <para>Delivers an attack to all enemies in a straight line before you with a potency of 700 for the first enemy and 50% less for all remaining enemies. Can only be executed while under the effect of Blast Arrow Ready. ※This action cannot be assigned to a hotbar. ※Apex Arrow changes to Blast Arrow when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction BlastArrowPvE => _BlastArrowPvECreator.Value;
    private readonly Lazy<IBaseAction> _RadiantFinalePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25785, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRadiantFinalePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25785"><strong>Radiant Finale</strong></see> <i>PvE</i> (BRD) [25785] [Ability]
    /// </summary>
    static partial void ModifyRadiantFinalePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25785"><strong>Radiant Finale</strong></see> <i>PvE</i> (BRD) [25785] [Ability]
    /// <para>Increases damage dealt by self and nearby party members. Duration: 20s Effectiveness is determined by the number of different Coda active in the Song Gauge. 1 Coda: 2% 2 Coda: 4% 3 Coda: 6% Can only be executed when at least 1 coda is active.</para>
    /// </summary>
    
    public IBaseAction RadiantFinalePvE => _RadiantFinalePvECreator.Value;
    private readonly Lazy<IBaseAction> _PowerfulShotPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29391, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyPowerfulShotPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29391"><strong>Powerful Shot</strong></see> <i>PvP</i> (BRD) [29391] [Weaponskill]
    /// </summary>
    static partial void ModifyPowerfulShotPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29391"><strong>Powerful Shot</strong></see> <i>PvP</i> (BRD) [29391] [Weaponskill]
    /// <para>Delivers a ranged attack with a potency of 6,000. Additional Effect: Reduces the recast time of Harmonic Arrow by 4s Requires casting time to execute. However it is possible to walk while casting.</para>
    /// </summary>
    
    public IBaseAction PowerfulShotPvP => _PowerfulShotPvPCreator.Value;
    private readonly Lazy<IBaseAction> _PitchPerfectPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29392, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyPitchPerfectPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29392"><strong>Pitch Perfect</strong></see> <i>PvP</i> (BRD) [29392] [Weaponskill]
    /// </summary>
    static partial void ModifyPitchPerfectPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29392"><strong>Pitch Perfect</strong></see> <i>PvP</i> (BRD) [29392] [Weaponskill]
    /// <para>Delivers an attack to target and all enemies nearby it with a potency of 9,000 for the first enemy and 4,500 for all remaining enemies. Additional Effect: Reduces the recast time of Harmonic Arrow by 4s Can only be executed while under the effect of Repertoire. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction PitchPerfectPvP => _PitchPerfectPvPCreator.Value;
    private readonly Lazy<IBaseAction> _ApexArrowPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29393, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyApexArrowPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29393"><strong>Apex Arrow</strong></see> <i>PvP</i> (BRD) [29393] [Weaponskill]
    /// </summary>
    static partial void ModifyApexArrowPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29393"><strong>Apex Arrow</strong></see> <i>PvP</i> (BRD) [29393] [Weaponskill]
    /// <para>Delivers an attack with a potency of 8,000 to all enemies in a straight line before you. Additional Effect: Grants Frontliner's March Frontliner's March Effect: Reduces your weaponskill cast time and recast time by 15% and increases damage dealt by self and all party members within a radius of 30 yalms by 5% Duration: 30s Additional Effect: Grants Blast Arrow Ready Duration: 20s This action's effects extend through obstructions. This weaponskill does not share a recast timer with any other actions. ※Action changes to Blast Arrow while under the effect of Blast Arrow Ready.</para>
    /// </summary>
    
    public IBaseAction ApexArrowPvP => _ApexArrowPvPCreator.Value;
    private readonly Lazy<IBaseAction> _BlastArrowPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29394, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBlastArrowPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29394"><strong>Blast Arrow</strong></see> <i>PvP</i> (BRD) [29394] [Weaponskill]
    /// </summary>
    static partial void ModifyBlastArrowPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29394"><strong>Blast Arrow</strong></see> <i>PvP</i> (BRD) [29394] [Weaponskill]
    /// <para>Delivers an attack with a potency of 10,000 to all enemies in a straight line before you. Additional Effect: 10-yalm knockback Can only be executed while under the effect of Blast Arrow Ready. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction BlastArrowPvP => _BlastArrowPvPCreator.Value;
    private readonly Lazy<IBaseAction> _SilentNocturnePvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29395, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifySilentNocturnePvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29395"><strong>Silent Nocturne</strong></see> <i>PvP</i> (BRD) [29395] [Ability]
    /// </summary>
    static partial void ModifySilentNocturnePvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29395"><strong>Silent Nocturne</strong></see> <i>PvP</i> (BRD) [29395] [Ability]
    /// <para>Silences target. Duration: 2s Additional Effect: Grants Repertoire Duration: 10s ※Powerful Shot changes to Pitch Perfect while under the effect of Repertoire.</para>
    /// </summary>
    
    public IBaseAction SilentNocturnePvP => _SilentNocturnePvPCreator.Value;
    private readonly Lazy<IBaseAction> _RepellingShotPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29399, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRepellingShotPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29399"><strong>Repelling Shot</strong></see> <i>PvP</i> (BRD) [29399] [Ability]
    /// </summary>
    static partial void ModifyRepellingShotPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29399"><strong>Repelling Shot</strong></see> <i>PvP</i> (BRD) [29399] [Ability]
    /// <para>Delivers an attack with a potency of 4,500. Additional Effect: 10-yalm backstep Additional Effect: Bind Duration: 3s Additional Effect: Grants Repertoire Duration: 10s Cannot be executed while bound. ※Powerful Shot changes to Pitch Perfect while under the effect of Repertoire.</para>
    /// </summary>
    
    public IBaseAction RepellingShotPvP => _RepellingShotPvPCreator.Value;
    private readonly Lazy<IBaseAction> _TheWardensPaeanPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29400, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyTheWardensPaeanPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29400"><strong>the Warden's Paean</strong></see> <i>PvP</i> (BRD) [29400] [Ability]
    /// </summary>
    static partial void ModifyTheWardensPaeanPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29400"><strong>the Warden's Paean</strong></see> <i>PvP</i> (BRD) [29400] [Ability]
    /// <para>Removes one status affliction from self or target party member that can be removed by Purify. If a status affliction cannot be removed creates a barrier that will nullify the next status affliction that can be removed by Purify. Duration: 10s Grants Warden's Grace upon successfully removing or nullifying a status affliction. Warden's Grace Effect: Reduces damage taken by 25% while increasing HP recovered by healing actions and movement speed by 25% Duration: 5s</para>
    /// </summary>
    
    public IBaseAction TheWardensPaeanPvP => _TheWardensPaeanPvPCreator.Value;
    private readonly Lazy<IBaseAction> _WideVolleyPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)36974, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyWideVolleyPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/36974"><strong>Wide Volley</strong></see> <i>PvE</i> (ARC BRD) [36974] [Weaponskill]
    /// </summary>
    static partial void ModifyWideVolleyPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/36974"><strong>Wide Volley</strong></see> <i>PvE</i> (ARC BRD) [36974] [Weaponskill]
    /// <para>Delivers an attack with a potency of 140 to target and all enemies nearby it. Can only be executed while under the effect of Hawk's Eye.</para>
    /// </summary>
    
    public IBaseAction WideVolleyPvE => _WideVolleyPvECreator.Value;
    private readonly Lazy<IBaseAction> _HeartbreakShotPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)36975, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHeartbreakShotPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/36975"><strong>Heartbreak Shot</strong></see> <i>PvE</i> (BRD) [36975] [Ability]
    /// </summary>
    static partial void ModifyHeartbreakShotPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/36975"><strong>Heartbreak Shot</strong></see> <i>PvE</i> (BRD) [36975] [Ability]
    /// <para>Delivers an attack with a potency of 180. Maximum Charges: 3 Shares a recast timer with Rain of Death.</para>
    /// </summary>
    
    public IBaseAction HeartbreakShotPvE => _HeartbreakShotPvECreator.Value;
    private readonly Lazy<IBaseAction> _ResonantArrowPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)36976, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyResonantArrowPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/36976"><strong>Resonant Arrow</strong></see> <i>PvE</i> (BRD) [36976] [Weaponskill]
    /// </summary>
    static partial void ModifyResonantArrowPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/36976"><strong>Resonant Arrow</strong></see> <i>PvE</i> (BRD) [36976] [Weaponskill]
    /// <para>Delivers an attack to target and all enemies nearby it with a potency of 640 for the first enemy and 50% less for all remaining enemies. Can only be executed while under the effect of Resonant Arrow Ready.</para>
    /// </summary>
    
    public IBaseAction ResonantArrowPvE => _ResonantArrowPvECreator.Value;
    private readonly Lazy<IBaseAction> _RadiantEncorePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)36977, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRadiantEncorePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/36977"><strong>Radiant Encore</strong></see> <i>PvE</i> (BRD) [36977] [Weaponskill]
    /// </summary>
    static partial void ModifyRadiantEncorePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/36977"><strong>Radiant Encore</strong></see> <i>PvE</i> (BRD) [36977] [Weaponskill]
    /// <para>Delivers an attack to target and all enemies nearby it. Potency is determined by the number of different Coda consumed in the Radiant Finale executed prior dealing full damage for the first enemy and 50% less for all remaining enemies. 1 Coda: 700 2 Coda: 800 3 Coda: 1,100 Can only be executed while under the effect of Radiant Encore Ready.</para>
    /// </summary>
    
    public IBaseAction RadiantEncorePvE => _RadiantEncorePvECreator.Value;
    private readonly Lazy<IBaseAction> _HarmonicArrowPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41464, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHarmonicArrowPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41464"><strong>Harmonic Arrow</strong></see> <i>PvP</i> (BRD) [41464] [Weaponskill]
    /// </summary>
    static partial void ModifyHarmonicArrowPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41464"><strong>Harmonic Arrow</strong></see> <i>PvP</i> (BRD) [41464] [Weaponskill]
    /// <para>Delivers a ranged attack with a potency of 9,000. Consumes all charges upon execution each charge adding an additional strike to the attack while lowering potency. 2 Charge Potency: 6,000 per strike 3 Charge Potency: 5,000 per strike 4 Charge Potency: 4,500 per strike Maximum Charges: 4</para>
    /// </summary>
    
    public IBaseAction HarmonicArrowPvP => _HarmonicArrowPvPCreator.Value;
    private readonly Lazy<IBaseAction> _HarmonicArrowPvP_41465Creator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41465, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHarmonicArrowPvP_41465(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41465"><strong>Harmonic Arrow</strong></see> <i>PvP</i> (BRD) [41465] [Weaponskill]
    /// </summary>
    static partial void ModifyHarmonicArrowPvP_41465(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41465"><strong>Harmonic Arrow</strong></see> <i>PvP</i> (BRD) [41465] [Weaponskill]
    /// <para>Delivers a ranged attack with a potency of 9,000. Consumes all charges upon execution each charge adding an additional strike to the attack while lowering potency. 2 Charge Potency: 6,000 per strike 3 Charge Potency: 5,000 per strike 4 Charge Potency: 4,500 per strike Maximum Charges: 4</para>
    /// </summary>
    
    public IBaseAction HarmonicArrowPvP_41465 => _HarmonicArrowPvP_41465Creator.Value;
    private readonly Lazy<IBaseAction> _HarmonicArrowPvP_41466Creator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41466, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHarmonicArrowPvP_41466(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41466"><strong>Harmonic Arrow</strong></see> <i>PvP</i> (BRD) [41466] [Weaponskill]
    /// </summary>
    static partial void ModifyHarmonicArrowPvP_41466(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41466"><strong>Harmonic Arrow</strong></see> <i>PvP</i> (BRD) [41466] [Weaponskill]
    /// <para>Delivers a ranged attack with a potency of 9,000. Consumes all charges upon execution each charge adding an additional strike to the attack while lowering potency. 2 Charge Potency: 6,000 per strike 3 Charge Potency: 5,000 per strike 4 Charge Potency: 4,500 per strike Maximum Charges: 4</para>
    /// </summary>
    
    public IBaseAction HarmonicArrowPvP_41466 => _HarmonicArrowPvP_41466Creator.Value;
    private readonly Lazy<IBaseAction> _EncoreOfLightPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41467, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEncoreOfLightPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41467"><strong>Encore of Light</strong></see> <i>PvP</i> (BRD) [41467] [Ability]
    /// </summary>
    static partial void ModifyEncoreOfLightPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41467"><strong>Encore of Light</strong></see> <i>PvP</i> (BRD) [41467] [Ability]
    /// <para>Delivers a ranged attack with a potency of 10,000 to target and all enemies nearby it. Additional Effect: Reduces target's MP by 4,000 Effect cannot be applied to players riding machina or non-player combatants. Can only be executed while under the effect of Encore of Light Ready. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction EncoreOfLightPvP => _EncoreOfLightPvPCreator.Value;
    private readonly Lazy<IBaseAction> _HarmonicArrowPvP_41964Creator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41964, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHarmonicArrowPvP_41964(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41964"><strong>Harmonic Arrow</strong></see> <i>PvP</i> (BRD) [41964] [Weaponskill]
    /// </summary>
    static partial void ModifyHarmonicArrowPvP_41964(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41964"><strong>Harmonic Arrow</strong></see> <i>PvP</i> (BRD) [41964] [Weaponskill]
    /// <para>Delivers a ranged attack with a potency of 9,000. Consumes all charges upon execution each charge adding an additional strike to the attack while lowering potency. 2 Charge Potency: 6,000 per strike 3 Charge Potency: 5,000 per strike 4 Charge Potency: 4,500 per strike Maximum Charges: 4</para>
    /// </summary>
    
    public IBaseAction HarmonicArrowPvP_41964 => _HarmonicArrowPvP_41964Creator.Value;

    private IBaseAction[] _AllBaseActions = null;
    
    /// <inheritdoc/>
    public override IBaseAction[] AllBaseActions => _AllBaseActions ??= [
    	HeavyShotPvE, StraightShotPvE, VenomousBitePvE, RagingStrikesPvE, QuickNockPvE, BarragePvE, BloodletterPvE, RepellingShotPvE, WindbitePvE, MagesBalladPvE, ArmysPaeonPvE, RainOfDeathPvE, BattleVoicePvE, EmpyrealArrowPvE, TheWanderersMinuetPvE, IronJawsPvE, TheWardensPaeanPvE, SidewinderPvE, PitchPerfectPvE, TroubadourPvE, CausticBitePvE, StormbitePvE, NaturesMinnePvE, RefulgentArrowPvE, ShadowbitePvE, BurstShotPvE, ApexArrowPvE, LadonsbitePvE, BlastArrowPvE, RadiantFinalePvE, PowerfulShotPvP, PitchPerfectPvP, ApexArrowPvP, BlastArrowPvP, SilentNocturnePvP, RepellingShotPvP, TheWardensPaeanPvP, WideVolleyPvE, HeartbreakShotPvE, ResonantArrowPvE, RadiantEncorePvE, HarmonicArrowPvP, EncoreOfLightPvP,
    	..base.AllBaseActions,
    ];

private readonly Lazy<IBaseAction> _BigShotPvECreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)4238, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifyBigShotPvE(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/4238"><strong>Big Shot</strong></see> <i>PvE</i> (All Classes) [4238] [Limit Break]
/// </summary>
static partial void ModifyBigShotPvE(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/4238"><strong>Big Shot</strong></see> <i>PvE</i> (All Classes) [4238] [Limit Break]
/// <para></para>
/// </summary>

public IBaseAction BigShotPvE => _BigShotPvECreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/4238"><strong>Big Shot</strong></see> <i>PvE</i> (All Classes) [4238] [Limit Break]
/// <para></para>
/// </summary>
private sealed protected override IBaseAction LimitBreak1 => BigShotPvE;
private readonly Lazy<IBaseAction> _DesperadoPvECreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)4239, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifyDesperadoPvE(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/4239"><strong>Desperado</strong></see> <i>PvE</i> (All Classes) [4239] [Limit Break]
/// </summary>
static partial void ModifyDesperadoPvE(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/4239"><strong>Desperado</strong></see> <i>PvE</i> (All Classes) [4239] [Limit Break]
/// <para></para>
/// </summary>

public IBaseAction DesperadoPvE => _DesperadoPvECreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/4239"><strong>Desperado</strong></see> <i>PvE</i> (All Classes) [4239] [Limit Break]
/// <para></para>
/// </summary>
private sealed protected override IBaseAction LimitBreak2 => DesperadoPvE;
private readonly Lazy<IBaseAction> _SagittariusArrowPvECreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)4244, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifySagittariusArrowPvE(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/4244"><strong>Sagittarius Arrow</strong></see> <i>PvE</i> (All Classes) [4244] [Limit Break]
/// </summary>
static partial void ModifySagittariusArrowPvE(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/4244"><strong>Sagittarius Arrow</strong></see> <i>PvE</i> (All Classes) [4244] [Limit Break]
/// <para></para>
/// </summary>

public IBaseAction SagittariusArrowPvE => _SagittariusArrowPvECreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/4244"><strong>Sagittarius Arrow</strong></see> <i>PvE</i> (All Classes) [4244] [Limit Break]
/// <para></para>
/// </summary>
private sealed protected override IBaseAction LimitBreak3 => SagittariusArrowPvE;
private readonly Lazy<IBaseAction> _FinalFantasiaPvPCreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)29401, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifyFinalFantasiaPvP(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/29401"><strong>Final Fantasia</strong></see> <i>PvP</i> (BRD) [29401] [Limit Break]
/// </summary>
static partial void ModifyFinalFantasiaPvP(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/29401"><strong>Final Fantasia</strong></see> <i>PvP</i> (BRD) [29401] [Limit Break]
/// <para>Grants Final Fantasia. Final Fantasia Effect: Reduces your weaponskill cast time and recast time by 15% while also increasing damage dealt by self and all party members within 30 yalms by 10% and movement speed by 25% Effect also gradually fills the limit gauge when targets are in combat. Duration: 30s Additional Effect: Grants Encore of Light Ready Duration: 30s Can only be executed when the limit gauge is full. Gauge Charge Time: 120s This action's effects extend through obstructions. ※Action changes to Encore of Light upon execution.</para>
/// </summary>

private IBaseAction FinalFantasiaPvP => _FinalFantasiaPvPCreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/29401"><strong>Final Fantasia</strong></see> <i>PvP</i> (BRD) [29401] [Limit Break]
/// <para>Grants Final Fantasia. Final Fantasia Effect: Reduces your weaponskill cast time and recast time by 15% while also increasing damage dealt by self and all party members within 30 yalms by 10% and movement speed by 25% Effect also gradually fills the limit gauge when targets are in combat. Duration: 30s Additional Effect: Grants Encore of Light Ready Duration: 30s Can only be executed when the limit gauge is full. Gauge Charge Time: 120s This action's effects extend through obstructions. ※Action changes to Encore of Light upon execution.</para>
/// </summary>
private sealed protected override IBaseAction LimitBreakPvP => FinalFantasiaPvP;

#endregion

#region Traits

    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/50168"><strong>Bite Mastery) | Caustic Bite |
| Wide Volley | braucht (Erzeuger nicht im Text) | Hawk's Eye |
| Wide Volley | Ausbau (Wide Volley Mastery) | Shadowbite |
| Windbite | Ausbau (Heavy Shot</strong></see> <i>PvE</i> (ARC BRD) [97] [Weaponskill]
    /// </summary>
    static partial void ModifyHeavyShotPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/97"><strong>Heavy Shot</strong></see> <i>PvE</i> (ARC BRD) [97] [Weaponskill]
    /// <para>Delivers an attack with a potency of 160.</para>
    /// </summary>
    
    public IBaseAction HeavyShotPvE => _HeavyShotPvECreator.Value;
    private readonly Lazy<IBaseAction> _StraightShotPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)98, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyStraightShotPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/98"><strong>Straight Shot</strong></see> <i>PvE</i> (ARC BRD) [98] [Weaponskill]
    /// </summary>
    static partial void ModifyStraightShotPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/98"><strong>Straight Shot</strong></see> <i>PvE</i> (ARC BRD) [98] [Weaponskill]
    /// <para>Delivers an attack with a potency of 200. Can only be executed when under the effect of Hawk's Eye.</para>
    /// </summary>
    
    public IBaseAction StraightShotPvE => _StraightShotPvECreator.Value;
    private readonly Lazy<IBaseAction> _VenomousBitePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)100, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyVenomousBitePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/100"><strong>Venomous Bite</strong></see> <i>PvE</i> (ARC BRD) [100] [Weaponskill]
    /// </summary>
    static partial void ModifyVenomousBitePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/100"><strong>Venomous Bite</strong></see> <i>PvE</i> (ARC BRD) [100] [Weaponskill]
    /// <para>Delivers an attack with a potency of 100. Additional Effect: Venom Potency: 15 Duration: 45s</para>
    /// </summary>
    
    public IBaseAction VenomousBitePvE => _VenomousBitePvECreator.Value;
    private readonly Lazy<IBaseAction> _RagingStrikesPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)101, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRagingStrikesPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/101"><strong>Raging Strikes</strong></see> <i>PvE</i> (ARC BRD) [101] [Ability]
    /// </summary>
    static partial void ModifyRagingStrikesPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/101"><strong>Raging Strikes</strong></see> <i>PvE</i> (ARC BRD) [101] [Ability]
    /// <para>Increases damage dealt by 15%. Duration: 20s</para>
    /// </summary>
    
    public IBaseAction RagingStrikesPvE => _RagingStrikesPvECreator.Value;
    private readonly Lazy<IBaseAction> _QuickNockPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)106, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyQuickNockPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/106"><strong>Quick Nock</strong></see> <i>PvE</i> (ARC BRD) [106] [Weaponskill]
    /// </summary>
    static partial void ModifyQuickNockPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/106"><strong>Quick Nock</strong></see> <i>PvE</i> (ARC BRD) [106] [Weaponskill]
    /// <para>Delivers an attack with a potency of 110 to all enemies in a cone before you.</para>
    /// </summary>
    
    public IBaseAction QuickNockPvE => _QuickNockPvECreator.Value;
    private readonly Lazy<IBaseAction> _BarragePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)107, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBarragePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/107"><strong>Barrage</strong></see> <i>PvE</i> (ARC BRD) [107] [Ability]
    /// </summary>
    static partial void ModifyBarragePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/107"><strong>Barrage</strong></see> <i>PvE</i> (ARC BRD) [107] [Ability]
    /// <para>Grants Barrage allowing the use of and . Upon execution will strike the selected target three times while 's potency will be increased to . Duration: 10s</para>
    /// </summary>
    
    public IBaseAction BarragePvE => _BarragePvECreator.Value;
    private readonly Lazy<IBaseAction> _BloodletterPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)110, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBloodletterPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/110"><strong>Bloodletter</strong></see> <i>PvE</i> (ARC BRD) [110] [Ability]
    /// </summary>
    static partial void ModifyBloodletterPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/110"><strong>Bloodletter</strong></see> <i>PvE</i> (ARC BRD) [110] [Ability]
    /// <para>Delivers an attack with a potency of 130. Maximum Charges:</para>
    /// </summary>
    
    public IBaseAction BloodletterPvE => _BloodletterPvECreator.Value;
    private readonly Lazy<IBaseAction> _RepellingShotPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)112, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRepellingShotPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/112"><strong>Repelling Shot</strong></see> <i>PvE</i> (ARC BRD) [112] [Ability]
    /// </summary>
    static partial void ModifyRepellingShotPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/112"><strong>Repelling Shot</strong></see> <i>PvE</i> (ARC BRD) [112] [Ability]
    /// <para>Jump 10 yalms away from current target. Cannot be executed while bound.</para>
    /// </summary>
    
    public IBaseAction RepellingShotPvE => _RepellingShotPvECreator.Value;
    private readonly Lazy<IBaseAction> _WindbitePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)113, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyWindbitePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/113"><strong>Windbite</strong></see> <i>PvE</i> (ARC BRD) [113] [Weaponskill]
    /// </summary>
    static partial void ModifyWindbitePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/113"><strong>Windbite</strong></see> <i>PvE</i> (ARC BRD) [113] [Weaponskill]
    /// <para>Deals wind damage with a potency of 60. Additional Effect: Wind damage over time Potency: 20 Duration: 45s</para>
    /// </summary>
    
    public IBaseAction WindbitePvE => _WindbitePvECreator.Value;
    private readonly Lazy<IBaseAction> _MagesBalladPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)114, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyMagesBalladPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/114"><strong>Mage's Ballad</strong></see> <i>PvE</i> (BRD) [114] [Ability]
    /// </summary>
    static partial void ModifyMagesBalladPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/114"><strong>Mage's Ballad</strong></see> <i>PvE</i> (BRD) [114] [Ability]
    /// <para>Grants Mage's Ballad to self and all party members within 50 yalms increasing damage dealt by 1%. Duration: 45s Additional Effect: 80% chance to grant Repertoire This effect can trigger repeatedly while singing the Mage's Ballad. Repertoire Effect: Reduces the recast time of by 7.5s Can only be executed while in combat.</para>
    /// </summary>
    
    public IBaseAction MagesBalladPvE => _MagesBalladPvECreator.Value;
    private readonly Lazy<IBaseAction> _ArmysPaeonPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)116, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyArmysPaeonPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/116"><strong>Army's Paeon</strong></see> <i>PvE</i> (BRD) [116] [Ability]
    /// </summary>
    static partial void ModifyArmysPaeonPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/116"><strong>Army's Paeon</strong></see> <i>PvE</i> (BRD) [116] [Ability]
    /// <para>Grants Army's Paeon to self and all party members within 50 yalms increasing direct hit rate by 3%. Duration: 45s Additional Effect: 80% chance to grant Repertoire This effect can trigger repeatedly while singing the Army's Paeon. Repertoire Effect: Reduces weaponskill cast time and recast time spell cast time and recast time and auto-attack delay by 4% Can be stacked up to 4 times. Can only be executed while in combat.</para>
    /// </summary>
    
    public IBaseAction ArmysPaeonPvE => _ArmysPaeonPvECreator.Value;
    private readonly Lazy<IBaseAction> _RainOfDeathPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)117, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRainOfDeathPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/117"><strong>Rain of Death</strong></see> <i>PvE</i> (BRD) [117] [Ability]
    /// </summary>
    static partial void ModifyRainOfDeathPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/117"><strong>Rain of Death</strong></see> <i>PvE</i> (BRD) [117] [Ability]
    /// <para>Delivers an attack with a potency of 100 to target and all enemies nearby it. Maximum Charges: Shares a recast timer with .</para>
    /// </summary>
    
    public IBaseAction RainOfDeathPvE => _RainOfDeathPvECreator.Value;
    private readonly Lazy<IBaseAction> _BattleVoicePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)118, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBattleVoicePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/118"><strong>Battle Voice</strong></see> <i>PvE</i> (BRD) [118] [Ability]
    /// </summary>
    static partial void ModifyBattleVoicePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/118"><strong>Battle Voice</strong></see> <i>PvE</i> (BRD) [118] [Ability]
    /// <para>Increases direct hit rate of self and all nearby party members by 20%. Duration: 20s</para>
    /// </summary>
    
    public IBaseAction BattleVoicePvE => _BattleVoicePvECreator.Value;
    private readonly Lazy<IBaseAction> _EmpyrealArrowPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3558, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEmpyrealArrowPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3558"><strong>Empyreal Arrow</strong></see> <i>PvE</i> (BRD) [3558] [Ability]
    /// </summary>
    static partial void ModifyEmpyrealArrowPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3558"><strong>Empyreal Arrow</strong></see> <i>PvE</i> (BRD) [3558] [Ability]
    /// <para>Delivers an attack with a potency of .</para>
    /// </summary>
    
    public IBaseAction EmpyrealArrowPvE => _EmpyrealArrowPvECreator.Value;
    private readonly Lazy<IBaseAction> _TheWanderersMinuetPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3559, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyTheWanderersMinuetPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3559"><strong>the Wanderer's Minuet</strong></see> <i>PvE</i> (BRD) [3559] [Ability]
    /// </summary>
    static partial void ModifyTheWanderersMinuetPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3559"><strong>the Wanderer's Minuet</strong></see> <i>PvE</i> (BRD) [3559] [Ability]
    /// <para>Grants the Wanderer's Minuet to self and all party members within 50 yalms increasing critical hit rate by 2%. Duration: 45s Additional Effect: 80% chance to grant Repertoire This effect can trigger repeatedly while singing the Wanderer's Minuet. Repertoire Effect: Allows execution of Pitch Perfect Can be stacked up to 3 times. Can only be executed while in combat.</para>
    /// </summary>
    
    public IBaseAction TheWanderersMinuetPvE => _TheWanderersMinuetPvECreator.Value;
    private readonly Lazy<IBaseAction> _IronJawsPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3560, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyIronJawsPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3560"><strong>Iron Jaws</strong></see> <i>PvE</i> (BRD) [3560] [Weaponskill]
    /// </summary>
    static partial void ModifyIronJawsPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3560"><strong>Iron Jaws</strong></see> <i>PvE</i> (BRD) [3560] [Weaponskill]
    /// <para>Delivers an attack with a potency of 100. Additional Effect: If the target is suffering from a effect inflicted by you the effect timer is reset</para>
    /// </summary>
    
    public IBaseAction IronJawsPvE => _IronJawsPvECreator.Value;
    private readonly Lazy<IBaseAction> _TheWardensPaeanPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3561, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyTheWardensPaeanPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3561"><strong>the Warden's Paean</strong></see> <i>PvE</i> (BRD) [3561] [Ability]
    /// </summary>
    static partial void ModifyTheWardensPaeanPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3561"><strong>the Warden's Paean</strong></see> <i>PvE</i> (BRD) [3561] [Ability]
    /// <para>Removes one select detrimental effect from self or target party member. If the target is not enfeebled a barrier is created nullifying the target's next detrimental effect suffered. Duration: 30s</para>
    /// </summary>
    
    public IBaseAction TheWardensPaeanPvE => _TheWardensPaeanPvECreator.Value;
    private readonly Lazy<IBaseAction> _SidewinderPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3562, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifySidewinderPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3562"><strong>Sidewinder</strong></see> <i>PvE</i> (BRD) [3562] [Ability]
    /// </summary>
    static partial void ModifySidewinderPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3562"><strong>Sidewinder</strong></see> <i>PvE</i> (BRD) [3562] [Ability]
    /// <para>Delivers an attack with a potency of .</para>
    /// </summary>
    
    public IBaseAction SidewinderPvE => _SidewinderPvECreator.Value;
    private readonly Lazy<IBaseAction> _PitchPerfectPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7404, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyPitchPerfectPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7404"><strong>Pitch Perfect</strong></see> <i>PvE</i> (BRD) [7404] [Ability]
    /// </summary>
    static partial void ModifyPitchPerfectPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7404"><strong>Pitch Perfect</strong></see> <i>PvE</i> (BRD) [7404] [Ability]
    /// <para>Delivers an attack to the target and all enemies nearby it. Potency varies with number of Repertoire stacks dealing full damage for the first enemy and 50% less for all remaining enemies. 1 Repertoire Stack: 100 2 Repertoire Stacks: 220 3 Repertoire Stacks: 360 Can only be executed when the Wanderer's Minuet is active and you have at least one stack of Repertoire.</para>
    /// </summary>
    
    public IBaseAction PitchPerfectPvE => _PitchPerfectPvECreator.Value;
    private readonly Lazy<IBaseAction> _TroubadourPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7405, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyTroubadourPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7405"><strong>Troubadour</strong></see> <i>PvE</i> (BRD) [7405] [Ability]
    /// </summary>
    static partial void ModifyTroubadourPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7405"><strong>Troubadour</strong></see> <i>PvE</i> (BRD) [7405] [Ability]
    /// <para>Reduces damage taken by self and nearby party members by %. Duration: 15s Effect cannot be stacked with machinist's Tactician or dancer's Shield Samba.</para>
    /// </summary>
    
    public IBaseAction TroubadourPvE => _TroubadourPvECreator.Value;
    private readonly Lazy<IBaseAction> _CausticBitePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7406, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyCausticBitePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7406"><strong>Caustic Bite</strong></see> <i>PvE</i> (BRD) [7406] [Weaponskill]
    /// </summary>
    static partial void ModifyCausticBitePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7406"><strong>Caustic Bite</strong></see> <i>PvE</i> (BRD) [7406] [Weaponskill]
    /// <para>Delivers an attack with a potency of 150. Additional Effect: Poison Potency: 20 Duration: 45s</para>
    /// </summary>
    
    public IBaseAction CausticBitePvE => _CausticBitePvECreator.Value;
    private readonly Lazy<IBaseAction> _StormbitePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7407, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyStormbitePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7407"><strong>Stormbite</strong></see> <i>PvE</i> (BRD) [7407] [Weaponskill]
    /// </summary>
    static partial void ModifyStormbitePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7407"><strong>Stormbite</strong></see> <i>PvE</i> (BRD) [7407] [Weaponskill]
    /// <para>Deals wind damage with a potency of 100. Additional Effect: Wind damage over time Potency: 25 Duration: 45s</para>
    /// </summary>
    
    public IBaseAction StormbitePvE => _StormbitePvECreator.Value;
    private readonly Lazy<IBaseAction> _NaturesMinnePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7408, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyNaturesMinnePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7408"><strong>Nature's Minne</strong></see> <i>PvE</i> (BRD) [7408] [Ability]
    /// </summary>
    static partial void ModifyNaturesMinnePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7408"><strong>Nature's Minne</strong></see> <i>PvE</i> (BRD) [7408] [Ability]
    /// <para>Increases HP recovery via healing actions by 15% for self and nearby party members. Duration: 15s</para>
    /// </summary>
    
    public IBaseAction NaturesMinnePvE => _NaturesMinnePvECreator.Value;
    private readonly Lazy<IBaseAction> _RefulgentArrowPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7409, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRefulgentArrowPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7409"><strong>Refulgent Arrow</strong></see> <i>PvE</i> (BRD) [7409] [Weaponskill]
    /// </summary>
    static partial void ModifyRefulgentArrowPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7409"><strong>Refulgent Arrow</strong></see> <i>PvE</i> (BRD) [7409] [Weaponskill]
    /// <para>Delivers an attack with a potency of . Can only be executed when under the effect of Hawk's Eye or Barrage.</para>
    /// </summary>
    
    public IBaseAction RefulgentArrowPvE => _RefulgentArrowPvECreator.Value;
    private readonly Lazy<IBaseAction> _ShadowbitePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16494, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyShadowbitePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16494"><strong>Shadowbite</strong></see> <i>PvE</i> (BRD) [16494] [Weaponskill]
    /// </summary>
    static partial void ModifyShadowbitePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16494"><strong>Shadowbite</strong></see> <i>PvE</i> (BRD) [16494] [Weaponskill]
    /// <para>Delivers an attack with a potency of 200 to target and all enemies nearby it. Barrage Potency: 300 Can only be executed when under the effect of Hawk's Eye or Barrage.</para>
    /// </summary>
    
    public IBaseAction ShadowbitePvE => _ShadowbitePvECreator.Value;
    private readonly Lazy<IBaseAction> _BurstShotPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16495, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBurstShotPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16495"><strong>Burst Shot</strong></see> <i>PvE</i> (BRD) [16495] [Weaponskill]
    /// </summary>
    static partial void ModifyBurstShotPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16495"><strong>Burst Shot</strong></see> <i>PvE</i> (BRD) [16495] [Weaponskill]
    /// <para>Delivers an attack with a potency of . Additional Effect: 35% chance of granting Hawk's Eye Duration: 30s</para>
    /// </summary>
    
    public IBaseAction BurstShotPvE => _BurstShotPvECreator.Value;
    private readonly Lazy<IBaseAction> _ApexArrowPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16496, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyApexArrowPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16496"><strong>Apex Arrow</strong></see> <i>PvE</i> (BRD) [16496] [Weaponskill]
    /// </summary>
    static partial void ModifyApexArrowPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16496"><strong>Apex Arrow</strong></see> <i>PvE</i> (BRD) [16496] [Weaponskill]
    /// <para>Delivers an attack with a potency of to all enemies in a straight line before you. Soul Voice Gauge Cost: 20 Potency increases up to as Soul Voice Gauge exceeds minimum cost. Consumes Soul Voice Gauge upon execution.</para>
    /// </summary>
    
    public IBaseAction ApexArrowPvE => _ApexArrowPvECreator.Value;
    private readonly Lazy<IBaseAction> _LadonsbitePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25783, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyLadonsbitePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25783"><strong>Ladonsbite</strong></see> <i>PvE</i> (BRD) [25783] [Weaponskill]
    /// </summary>
    static partial void ModifyLadonsbitePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25783"><strong>Ladonsbite</strong></see> <i>PvE</i> (BRD) [25783] [Weaponskill]
    /// <para>Delivers an attack with a potency of 140 to all enemies in a cone before you. Additional Effect: 35% chance of granting Hawk's Eye Duration: 30s</para>
    /// </summary>
    
    public IBaseAction LadonsbitePvE => _LadonsbitePvECreator.Value;
    private readonly Lazy<IBaseAction> _BlastArrowPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25784, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBlastArrowPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25784"><strong>Blast Arrow</strong></see> <i>PvE</i> (BRD) [25784] [Weaponskill]
    /// </summary>
    static partial void ModifyBlastArrowPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25784"><strong>Blast Arrow</strong></see> <i>PvE</i> (BRD) [25784] [Weaponskill]
    /// <para>Delivers an attack to all enemies in a straight line before you with a potency of 700 for the first enemy and 50% less for all remaining enemies. Can only be executed while under the effect of Blast Arrow Ready. ※This action cannot be assigned to a hotbar. ※Apex Arrow changes to Blast Arrow when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction BlastArrowPvE => _BlastArrowPvECreator.Value;
    private readonly Lazy<IBaseAction> _RadiantFinalePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25785, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRadiantFinalePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25785"><strong>Radiant Finale</strong></see> <i>PvE</i> (BRD) [25785] [Ability]
    /// </summary>
    static partial void ModifyRadiantFinalePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25785"><strong>Radiant Finale</strong></see> <i>PvE</i> (BRD) [25785] [Ability]
    /// <para>Increases damage dealt by self and nearby party members. Duration: 20s Effectiveness is determined by the number of different Coda active in the Song Gauge. 1 Coda: 2% 2 Coda: 4% 3 Coda: 6% Can only be executed when at least 1 coda is active.</para>
    /// </summary>
    
    public IBaseAction RadiantFinalePvE => _RadiantFinalePvECreator.Value;
    private readonly Lazy<IBaseAction> _PowerfulShotPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29391, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyPowerfulShotPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29391"><strong>Powerful Shot</strong></see> <i>PvP</i> (BRD) [29391] [Weaponskill]
    /// </summary>
    static partial void ModifyPowerfulShotPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29391"><strong>Powerful Shot</strong></see> <i>PvP</i> (BRD) [29391] [Weaponskill]
    /// <para>Delivers a ranged attack with a potency of 6,000. Additional Effect: Reduces the recast time of Harmonic Arrow by 4s Requires casting time to execute. However it is possible to walk while casting.</para>
    /// </summary>
    
    public IBaseAction PowerfulShotPvP => _PowerfulShotPvPCreator.Value;
    private readonly Lazy<IBaseAction> _PitchPerfectPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29392, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyPitchPerfectPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29392"><strong>Pitch Perfect</strong></see> <i>PvP</i> (BRD) [29392] [Weaponskill]
    /// </summary>
    static partial void ModifyPitchPerfectPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29392"><strong>Pitch Perfect</strong></see> <i>PvP</i> (BRD) [29392] [Weaponskill]
    /// <para>Delivers an attack to target and all enemies nearby it with a potency of 9,000 for the first enemy and 4,500 for all remaining enemies. Additional Effect: Reduces the recast time of Harmonic Arrow by 4s Can only be executed while under the effect of Repertoire. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction PitchPerfectPvP => _PitchPerfectPvPCreator.Value;
    private readonly Lazy<IBaseAction> _ApexArrowPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29393, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyApexArrowPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29393"><strong>Apex Arrow</strong></see> <i>PvP</i> (BRD) [29393] [Weaponskill]
    /// </summary>
    static partial void ModifyApexArrowPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29393"><strong>Apex Arrow</strong></see> <i>PvP</i> (BRD) [29393] [Weaponskill]
    /// <para>Delivers an attack with a potency of 8,000 to all enemies in a straight line before you. Additional Effect: Grants Frontliner's March Frontliner's March Effect: Reduces your weaponskill cast time and recast time by 15% and increases damage dealt by self and all party members within a radius of 30 yalms by 5% Duration: 30s Additional Effect: Grants Blast Arrow Ready Duration: 20s This action's effects extend through obstructions. This weaponskill does not share a recast timer with any other actions. ※Action changes to Blast Arrow while under the effect of Blast Arrow Ready.</para>
    /// </summary>
    
    public IBaseAction ApexArrowPvP => _ApexArrowPvPCreator.Value;
    private readonly Lazy<IBaseAction> _BlastArrowPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29394, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBlastArrowPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29394"><strong>Blast Arrow</strong></see> <i>PvP</i> (BRD) [29394] [Weaponskill]
    /// </summary>
    static partial void ModifyBlastArrowPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29394"><strong>Blast Arrow</strong></see> <i>PvP</i> (BRD) [29394] [Weaponskill]
    /// <para>Delivers an attack with a potency of 10,000 to all enemies in a straight line before you. Additional Effect: 10-yalm knockback Can only be executed while under the effect of Blast Arrow Ready. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction BlastArrowPvP => _BlastArrowPvPCreator.Value;
    private readonly Lazy<IBaseAction> _SilentNocturnePvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29395, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifySilentNocturnePvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29395"><strong>Silent Nocturne</strong></see> <i>PvP</i> (BRD) [29395] [Ability]
    /// </summary>
    static partial void ModifySilentNocturnePvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29395"><strong>Silent Nocturne</strong></see> <i>PvP</i> (BRD) [29395] [Ability]
    /// <para>Silences target. Duration: 2s Additional Effect: Grants Repertoire Duration: 10s ※Powerful Shot changes to Pitch Perfect while under the effect of Repertoire.</para>
    /// </summary>
    
    public IBaseAction SilentNocturnePvP => _SilentNocturnePvPCreator.Value;
    private readonly Lazy<IBaseAction> _RepellingShotPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29399, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRepellingShotPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29399"><strong>Repelling Shot</strong></see> <i>PvP</i> (BRD) [29399] [Ability]
    /// </summary>
    static partial void ModifyRepellingShotPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29399"><strong>Repelling Shot</strong></see> <i>PvP</i> (BRD) [29399] [Ability]
    /// <para>Delivers an attack with a potency of 4,500. Additional Effect: 10-yalm backstep Additional Effect: Bind Duration: 3s Additional Effect: Grants Repertoire Duration: 10s Cannot be executed while bound. ※Powerful Shot changes to Pitch Perfect while under the effect of Repertoire.</para>
    /// </summary>
    
    public IBaseAction RepellingShotPvP => _RepellingShotPvPCreator.Value;
    private readonly Lazy<IBaseAction> _TheWardensPaeanPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29400, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyTheWardensPaeanPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29400"><strong>the Warden's Paean</strong></see> <i>PvP</i> (BRD) [29400] [Ability]
    /// </summary>
    static partial void ModifyTheWardensPaeanPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29400"><strong>the Warden's Paean</strong></see> <i>PvP</i> (BRD) [29400] [Ability]
    /// <para>Removes one status affliction from self or target party member that can be removed by Purify. If a status affliction cannot be removed creates a barrier that will nullify the next status affliction that can be removed by Purify. Duration: 10s Grants Warden's Grace upon successfully removing or nullifying a status affliction. Warden's Grace Effect: Reduces damage taken by 25% while increasing HP recovered by healing actions and movement speed by 25% Duration: 5s</para>
    /// </summary>
    
    public IBaseAction TheWardensPaeanPvP => _TheWardensPaeanPvPCreator.Value;
    private readonly Lazy<IBaseAction> _WideVolleyPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)36974, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyWideVolleyPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/36974"><strong>Wide Volley</strong></see> <i>PvE</i> (ARC BRD) [36974] [Weaponskill]
    /// </summary>
    static partial void ModifyWideVolleyPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/36974"><strong>Wide Volley</strong></see> <i>PvE</i> (ARC BRD) [36974] [Weaponskill]
    /// <para>Delivers an attack with a potency of 140 to target and all enemies nearby it. Can only be executed while under the effect of Hawk's Eye.</para>
    /// </summary>
    
    public IBaseAction WideVolleyPvE => _WideVolleyPvECreator.Value;
    private readonly Lazy<IBaseAction> _HeartbreakShotPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)36975, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHeartbreakShotPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/36975"><strong>Heartbreak Shot</strong></see> <i>PvE</i> (BRD) [36975] [Ability]
    /// </summary>
    static partial void ModifyHeartbreakShotPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/36975"><strong>Heartbreak Shot</strong></see> <i>PvE</i> (BRD) [36975] [Ability]
    /// <para>Delivers an attack with a potency of 180. Maximum Charges: 3 Shares a recast timer with Rain of Death.</para>
    /// </summary>
    
    public IBaseAction HeartbreakShotPvE => _HeartbreakShotPvECreator.Value;
    private readonly Lazy<IBaseAction> _ResonantArrowPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)36976, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyResonantArrowPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/36976"><strong>Resonant Arrow</strong></see> <i>PvE</i> (BRD) [36976] [Weaponskill]
    /// </summary>
    static partial void ModifyResonantArrowPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/36976"><strong>Resonant Arrow</strong></see> <i>PvE</i> (BRD) [36976] [Weaponskill]
    /// <para>Delivers an attack to target and all enemies nearby it with a potency of 640 for the first enemy and 50% less for all remaining enemies. Can only be executed while under the effect of Resonant Arrow Ready.</para>
    /// </summary>
    
    public IBaseAction ResonantArrowPvE => _ResonantArrowPvECreator.Value;
    private readonly Lazy<IBaseAction> _RadiantEncorePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)36977, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRadiantEncorePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/36977"><strong>Radiant Encore</strong></see> <i>PvE</i> (BRD) [36977] [Weaponskill]
    /// </summary>
    static partial void ModifyRadiantEncorePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/36977"><strong>Radiant Encore</strong></see> <i>PvE</i> (BRD) [36977] [Weaponskill]
    /// <para>Delivers an attack to target and all enemies nearby it. Potency is determined by the number of different Coda consumed in the Radiant Finale executed prior dealing full damage for the first enemy and 50% less for all remaining enemies. 1 Coda: 700 2 Coda: 800 3 Coda: 1,100 Can only be executed while under the effect of Radiant Encore Ready.</para>
    /// </summary>
    
    public IBaseAction RadiantEncorePvE => _RadiantEncorePvECreator.Value;
    private readonly Lazy<IBaseAction> _HarmonicArrowPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41464, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHarmonicArrowPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41464"><strong>Harmonic Arrow</strong></see> <i>PvP</i> (BRD) [41464] [Weaponskill]
    /// </summary>
    static partial void ModifyHarmonicArrowPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41464"><strong>Harmonic Arrow</strong></see> <i>PvP</i> (BRD) [41464] [Weaponskill]
    /// <para>Delivers a ranged attack with a potency of 9,000. Consumes all charges upon execution each charge adding an additional strike to the attack while lowering potency. 2 Charge Potency: 6,000 per strike 3 Charge Potency: 5,000 per strike 4 Charge Potency: 4,500 per strike Maximum Charges: 4</para>
    /// </summary>
    
    public IBaseAction HarmonicArrowPvP => _HarmonicArrowPvPCreator.Value;
    private readonly Lazy<IBaseAction> _HarmonicArrowPvP_41465Creator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41465, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHarmonicArrowPvP_41465(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41465"><strong>Harmonic Arrow</strong></see> <i>PvP</i> (BRD) [41465] [Weaponskill]
    /// </summary>
    static partial void ModifyHarmonicArrowPvP_41465(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41465"><strong>Harmonic Arrow</strong></see> <i>PvP</i> (BRD) [41465] [Weaponskill]
    /// <para>Delivers a ranged attack with a potency of 9,000. Consumes all charges upon execution each charge adding an additional strike to the attack while lowering potency. 2 Charge Potency: 6,000 per strike 3 Charge Potency: 5,000 per strike 4 Charge Potency: 4,500 per strike Maximum Charges: 4</para>
    /// </summary>
    
    public IBaseAction HarmonicArrowPvP_41465 => _HarmonicArrowPvP_41465Creator.Value;
    private readonly Lazy<IBaseAction> _HarmonicArrowPvP_41466Creator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41466, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHarmonicArrowPvP_41466(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41466"><strong>Harmonic Arrow</strong></see> <i>PvP</i> (BRD) [41466] [Weaponskill]
    /// </summary>
    static partial void ModifyHarmonicArrowPvP_41466(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41466"><strong>Harmonic Arrow</strong></see> <i>PvP</i> (BRD) [41466] [Weaponskill]
    /// <para>Delivers a ranged attack with a potency of 9,000. Consumes all charges upon execution each charge adding an additional strike to the attack while lowering potency. 2 Charge Potency: 6,000 per strike 3 Charge Potency: 5,000 per strike 4 Charge Potency: 4,500 per strike Maximum Charges: 4</para>
    /// </summary>
    
    public IBaseAction HarmonicArrowPvP_41466 => _HarmonicArrowPvP_41466Creator.Value;
    private readonly Lazy<IBaseAction> _EncoreOfLightPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41467, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEncoreOfLightPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41467"><strong>Encore of Light</strong></see> <i>PvP</i> (BRD) [41467] [Ability]
    /// </summary>
    static partial void ModifyEncoreOfLightPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41467"><strong>Encore of Light</strong></see> <i>PvP</i> (BRD) [41467] [Ability]
    /// <para>Delivers a ranged attack with a potency of 10,000 to target and all enemies nearby it. Additional Effect: Reduces target's MP by 4,000 Effect cannot be applied to players riding machina or non-player combatants. Can only be executed while under the effect of Encore of Light Ready. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction EncoreOfLightPvP => _EncoreOfLightPvPCreator.Value;
    private readonly Lazy<IBaseAction> _HarmonicArrowPvP_41964Creator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41964, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHarmonicArrowPvP_41964(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41964"><strong>Harmonic Arrow</strong></see> <i>PvP</i> (BRD) [41964] [Weaponskill]
    /// </summary>
    static partial void ModifyHarmonicArrowPvP_41964(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41964"><strong>Harmonic Arrow</strong></see> <i>PvP</i> (BRD) [41964] [Weaponskill]
    /// <para>Delivers a ranged attack with a potency of 9,000. Consumes all charges upon execution each charge adding an additional strike to the attack while lowering potency. 2 Charge Potency: 6,000 per strike 3 Charge Potency: 5,000 per strike 4 Charge Potency: 4,500 per strike Maximum Charges: 4</para>
    /// </summary>
    
    public IBaseAction HarmonicArrowPvP_41964 => _HarmonicArrowPvP_41964Creator.Value;

    private IBaseAction[] _AllBaseActions = null;
    
    /// <inheritdoc/>
    public override IBaseAction[] AllBaseActions => _AllBaseActions ??= [
    	HeavyShotPvE, StraightShotPvE, VenomousBitePvE, RagingStrikesPvE, QuickNockPvE, BarragePvE, BloodletterPvE, RepellingShotPvE, WindbitePvE, MagesBalladPvE, ArmysPaeonPvE, RainOfDeathPvE, BattleVoicePvE, EmpyrealArrowPvE, TheWanderersMinuetPvE, IronJawsPvE, TheWardensPaeanPvE, SidewinderPvE, PitchPerfectPvE, TroubadourPvE, CausticBitePvE, StormbitePvE, NaturesMinnePvE, RefulgentArrowPvE, ShadowbitePvE, BurstShotPvE, ApexArrowPvE, LadonsbitePvE, BlastArrowPvE, RadiantFinalePvE, PowerfulShotPvP, PitchPerfectPvP, ApexArrowPvP, BlastArrowPvP, SilentNocturnePvP, RepellingShotPvP, TheWardensPaeanPvP, WideVolleyPvE, HeartbreakShotPvE, ResonantArrowPvE, RadiantEncorePvE, HarmonicArrowPvP, EncoreOfLightPvP,
    	..base.AllBaseActions,
    ];

private readonly Lazy<IBaseAction> _BigShotPvECreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)4238, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifyBigShotPvE(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/4238"><strong>Big Shot</strong></see> <i>PvE</i> (All Classes) [4238] [Limit Break]
/// </summary>
static partial void ModifyBigShotPvE(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/4238"><strong>Big Shot</strong></see> <i>PvE</i> (All Classes) [4238] [Limit Break]
/// <para></para>
/// </summary>

public IBaseAction BigShotPvE => _BigShotPvECreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/4238"><strong>Big Shot</strong></see> <i>PvE</i> (All Classes) [4238] [Limit Break]
/// <para></para>
/// </summary>
private sealed protected override IBaseAction LimitBreak1 => BigShotPvE;
private readonly Lazy<IBaseAction> _DesperadoPvECreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)4239, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifyDesperadoPvE(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/4239"><strong>Desperado</strong></see> <i>PvE</i> (All Classes) [4239] [Limit Break]
/// </summary>
static partial void ModifyDesperadoPvE(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/4239"><strong>Desperado</strong></see> <i>PvE</i> (All Classes) [4239] [Limit Break]
/// <para></para>
/// </summary>

public IBaseAction DesperadoPvE => _DesperadoPvECreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/4239"><strong>Desperado</strong></see> <i>PvE</i> (All Classes) [4239] [Limit Break]
/// <para></para>
/// </summary>
private sealed protected override IBaseAction LimitBreak2 => DesperadoPvE;
private readonly Lazy<IBaseAction> _SagittariusArrowPvECreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)4244, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifySagittariusArrowPvE(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/4244"><strong>Sagittarius Arrow</strong></see> <i>PvE</i> (All Classes) [4244] [Limit Break]
/// </summary>
static partial void ModifySagittariusArrowPvE(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/4244"><strong>Sagittarius Arrow</strong></see> <i>PvE</i> (All Classes) [4244] [Limit Break]
/// <para></para>
/// </summary>

public IBaseAction SagittariusArrowPvE => _SagittariusArrowPvECreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/4244"><strong>Sagittarius Arrow</strong></see> <i>PvE</i> (All Classes) [4244] [Limit Break]
/// <para></para>
/// </summary>
private sealed protected override IBaseAction LimitBreak3 => SagittariusArrowPvE;
private readonly Lazy<IBaseAction> _FinalFantasiaPvPCreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)29401, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifyFinalFantasiaPvP(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/29401"><strong>Final Fantasia</strong></see> <i>PvP</i> (BRD) [29401] [Limit Break]
/// </summary>
static partial void ModifyFinalFantasiaPvP(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/29401"><strong>Final Fantasia</strong></see> <i>PvP</i> (BRD) [29401] [Limit Break]
/// <para>Grants Final Fantasia. Final Fantasia Effect: Reduces your weaponskill cast time and recast time by 15% while also increasing damage dealt by self and all party members within 30 yalms by 10% and movement speed by 25% Effect also gradually fills the limit gauge when targets are in combat. Duration: 30s Additional Effect: Grants Encore of Light Ready Duration: 30s Can only be executed when the limit gauge is full. Gauge Charge Time: 120s This action's effects extend through obstructions. ※Action changes to Encore of Light upon execution.</para>
/// </summary>

private IBaseAction FinalFantasiaPvP => _FinalFantasiaPvPCreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/29401"><strong>Final Fantasia</strong></see> <i>PvP</i> (BRD) [29401] [Limit Break]
/// <para>Grants Final Fantasia. Final Fantasia Effect: Reduces your weaponskill cast time and recast time by 15% while also increasing damage dealt by self and all party members within 30 yalms by 10% and movement speed by 25% Effect also gradually fills the limit gauge when targets are in combat. Duration: 30s Additional Effect: Grants Encore of Light Ready Duration: 30s Can only be executed when the limit gauge is full. Gauge Charge Time: 120s This action's effects extend through obstructions. ※Action changes to Encore of Light upon execution.</para>
/// </summary>
private sealed protected override IBaseAction LimitBreakPvP => FinalFantasiaPvP;

#endregion

#region Traits

    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/50168"><strong>Bite Mastery) | Stormbite |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Barrage | Regel prüft | Empyreal Arrow |
| Barrage | Regel sperrt vorher | Iron Jaws |
| Barrage | Regel prüft | Raging Strikes |
| Barrage | Regel sperrt vorher | Straight Shot |
| Barrage | Regel sperrt vorher | Venomous Bite |
| Barrage | Regel sperrt vorher | Windbite |
| Battle Voice | Regel prüft | Mage's Ballad |
| Battle Voice | Regel prüft | Radiant Finale |
| Battle Voice | Regel prüft | Raging Strikes |
| Bloodletter | Regel sperrt vorher | Battle Voice |
| Bloodletter | Regel sperrt vorher | Empyreal Arrow |
| Bloodletter | Regel sperrt vorher | Radiant Finale |
| Burst Shot | Regel sperrt vorher | Barrage |
| Burst Shot | Regel sperrt vorher | Blast Arrow |
| Burst Shot | Regel sperrt vorher | Caustic Bite |
| Burst Shot | Regel sperrt vorher | Iron Jaws |
| Burst Shot | Regel sperrt vorher | Stormbite |
| Burst Shot | Regel sperrt vorher | Venomous Bite |
| Burst Shot | Regel sperrt vorher | Windbite |
| Caustic Bite | Regel sperrt vorher | Barrage |
| Caustic Bite | Regel sperrt vorher | Blast Arrow |
| Caustic Bite | Regel sperrt vorher | Stormbite |
| Empyreal Arrow | Regel sperrt vorher | Battle Voice |
| Empyreal Arrow | Regel sperrt vorher | Radiant Finale |
| Empyreal Arrow | Regel prüft | Raging Strikes |
| Heartbreak Shot | Regel sperrt vorher | Battle Voice |
| Heartbreak Shot | Regel prüft | Bloodletter |
| Heartbreak Shot | Regel sperrt vorher | Empyreal Arrow |
| Heartbreak Shot | Regel sperrt vorher | Radiant Finale |
| Heavy Shot | Regel sperrt vorher | Barrage |
| Heavy Shot | Regel sperrt vorher | Blast Arrow |
| Heavy Shot | Regel sperrt vorher | Caustic Bite |
| Heavy Shot | Regel sperrt vorher | Iron Jaws |
| Heavy Shot | Regel sperrt vorher | Stormbite |
| Heavy Shot | Regel sperrt vorher | Venomous Bite |
| Heavy Shot | Regel sperrt vorher | Windbite |
| Ladonsbite | Regel sperrt vorher | Barrage |
| Ladonsbite | Regel sperrt vorher | Blast Arrow |
| Ladonsbite | Regel sperrt vorher | Quick Nock |
| Pitch Perfect | Regel sperrt vorher | Battle Voice |
| Pitch Perfect | Regel sperrt vorher | Radiant Finale |
| Quick Nock | Regel sperrt vorher | Barrage |
| Quick Nock | Regel sperrt vorher | Blast Arrow |
| Radiant Encore | StatusNeed RadiantEncoreReady | Radiant Finale |
| Radiant Finale | Regel prüft | Mage's Ballad |
| Raging Strikes | Regel prüft | Battle Voice |
| Raging Strikes | Regel prüft | Mage's Ballad |
| Raging Strikes | Regel prüft | Radiant Finale |
| Raging Strikes | Regel prüft | Straight Shot |
| Rain of Death | Regel sperrt vorher | Battle Voice |
| Rain of Death | Regel prüft | Bloodletter |
| Rain of Death | Regel sperrt vorher | Empyreal Arrow |
| Rain of Death | Regel sperrt vorher | Radiant Finale |
| Refulgent Arrow | Regel sperrt vorher | Barrage |
| Refulgent Arrow | StatusNeed Barrage | Barrage |
| Refulgent Arrow | Regel sperrt vorher | Blast Arrow |
| Refulgent Arrow | StatusNeed HawksEye_3861 | Burst Shot |
| Refulgent Arrow | Regel sperrt vorher | Caustic Bite |
| Refulgent Arrow | StatusNeed HawksEye_3861 | Caustic Bite |
| Refulgent Arrow | StatusNeed HawksEye_3861 | Heavy Shot |
| Refulgent Arrow | Regel sperrt vorher | Iron Jaws |
| Refulgent Arrow | StatusNeed HawksEye_3861 | Iron Jaws |
| Refulgent Arrow | StatusNeed HawksEye_3861 | Ladonsbite |
| Refulgent Arrow | StatusNeed HawksEye_3861 | Quick Nock |
| Refulgent Arrow | Regel sperrt vorher | Stormbite |
| Refulgent Arrow | StatusNeed HawksEye_3861 | Stormbite |
| Refulgent Arrow | Regel sperrt vorher | Venomous Bite |
| Refulgent Arrow | Regel sperrt vorher | Windbite |
| Resonant Arrow | StatusNeed ResonantArrowReady | Barrage |
| Shadowbite | Regel sperrt vorher | Barrage |
| Shadowbite | StatusNeed Barrage | Barrage |
| Shadowbite | Regel sperrt vorher | Blast Arrow |
| Shadowbite | StatusNeed HawksEye_3861 | Burst Shot |
| Shadowbite | StatusNeed HawksEye_3861 | Caustic Bite |
| Shadowbite | StatusNeed HawksEye_3861 | Heavy Shot |
| Shadowbite | StatusNeed HawksEye_3861 | Iron Jaws |
| Shadowbite | StatusNeed HawksEye_3861 | Ladonsbite |
| Shadowbite | StatusNeed HawksEye_3861 | Quick Nock |
| Shadowbite | StatusNeed HawksEye_3861 | Stormbite |
| Sidewinder | Regel prüft | Battle Voice |
| Sidewinder | Regel sperrt vorher | Battle Voice |
| Sidewinder | Regel sperrt vorher | Empyreal Arrow |
| Sidewinder | Regel prüft | Radiant Finale |
| Sidewinder | Regel sperrt vorher | Radiant Finale |
| Sidewinder | Regel prüft | Raging Strikes |
| Stormbite | Regel sperrt vorher | Barrage |
| Stormbite | Regel sperrt vorher | Blast Arrow |
| Straight Shot | Regel sperrt vorher | Barrage |
| Straight Shot | StatusNeed Barrage | Barrage |
| Straight Shot | Regel sperrt vorher | Blast Arrow |
| Straight Shot | StatusNeed HawksEye_3861 | Burst Shot |
| Straight Shot | Regel sperrt vorher | Caustic Bite |
| Straight Shot | StatusNeed HawksEye_3861 | Caustic Bite |
| Straight Shot | StatusNeed HawksEye_3861 | Heavy Shot |
| Straight Shot | Regel sperrt vorher | Iron Jaws |
| Straight Shot | StatusNeed HawksEye_3861 | Iron Jaws |
| Straight Shot | StatusNeed HawksEye_3861 | Ladonsbite |
| Straight Shot | StatusNeed HawksEye_3861 | Quick Nock |
| Straight Shot | Regel prüft | Refulgent Arrow |
| Straight Shot | Regel sperrt vorher | Stormbite |
| Straight Shot | StatusNeed HawksEye_3861 | Stormbite |
| Straight Shot | Regel sperrt vorher | Venomous Bite |
| Straight Shot | Regel sperrt vorher | Windbite |
| Venomous Bite | Regel sperrt vorher | Barrage |
| Venomous Bite | Regel sperrt vorher | Blast Arrow |
| Venomous Bite | Regel prüft | Caustic Bite |
| Venomous Bite | Regel sperrt vorher | Caustic Bite |
| Venomous Bite | Regel sperrt vorher | Iron Jaws |
| Venomous Bite | Regel sperrt vorher | Stormbite |
| Venomous Bite | Regel sperrt vorher | Windbite |
| Wide Volley | Regel sperrt vorher | Barrage |
| Wide Volley | Regel sperrt vorher | Blast Arrow |
| Wide Volley | StatusNeed HawksEye_3861 | Burst Shot |
| Wide Volley | StatusNeed HawksEye_3861 | Caustic Bite |
| Wide Volley | StatusNeed HawksEye_3861 | Heavy Shot |
| Wide Volley | StatusNeed HawksEye_3861 | Iron Jaws |
| Wide Volley | StatusNeed HawksEye_3861 | Ladonsbite |
| Wide Volley | StatusNeed HawksEye_3861 | Quick Nock |
| Wide Volley | StatusNeed HawksEye_3861 | Stormbite |
| Windbite | Regel sperrt vorher | Barrage |
| Windbite | Regel sperrt vorher | Blast Arrow |
| Windbite | Regel sperrt vorher | Caustic Bite |
| Windbite | Regel prüft | Stormbite |
| Windbite | Regel sperrt vorher | Stormbite |

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `BigShotPvE`, `DesperadoPvE`, `SagittariusArrowPvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Foot Graze (`FootGrazePvE`, Ability): ungenutzt
- Leg Graze (`LegGrazePvE`, Ability): ungenutzt

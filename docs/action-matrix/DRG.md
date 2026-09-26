# DRG — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `DragoonRotation`
- Rotation: `RotationSolver/RebornRotations/Melee/DRG_Reborn.cs`
- Matrix als Tabelle: `DRG.csv`

## Nutzung

direkt: 40

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Nahkämpfer | Arm's Length (`ArmsLengthPvE`) | 7548 | Ability | direkt |
| Nahkämpfer | Bloodbath (`BloodbathPvE`) | 7542 | Ability | direkt |
| Nahkämpfer | Feint (`FeintPvE`) | 7549 | Ability | direkt |
| Nahkämpfer | Leg Sweep (`LegSweepPvE`) | 7863 | Ability | direkt |
| Nahkämpfer | Second Wind (`SecondWindPvE`) | 7541 | Ability | direkt |
| Nahkämpfer | True North (`TrueNorthPvE`) | 7546 | Ability | direkt |
| Job | Battle Litany (`BattleLitanyPvE`) | 3557 | Ability | direkt |
| Job | Chaos Thrust (`ChaosThrustPvE`) | 88 | Weaponskill | direkt |
| Job | Chaotic Spring (`ChaoticSpringPvE`) | 25772 | Weaponskill | direkt |
| Job | Coerthan Torment (`CoerthanTormentPvE`) | 16477 | Weaponskill | direkt |
| Job | Disembowel (`DisembowelPvE`) | 87 | Weaponskill | direkt |
| Job | Doom Spike (`DoomSpikePvE`) | 86 | Weaponskill | direkt |
| Job | Draconian Fury (`DraconianFuryPvE`) | 25770 | Weaponskill | direkt |
| Job | Dragonfire Dive (`DragonfireDivePvE`) | 96 | Ability | direkt |
| Job | Drakesbane (`DrakesbanePvE`) | 36952 | Weaponskill | direkt |
| Job | Elusive Jump (`ElusiveJumpPvE`) | 94 | Ability | direkt |
| Job | Fang and Claw (`FangAndClawPvE`) | 3554 | Weaponskill | direkt |
| Job | Full Thrust (`FullThrustPvE`) | 84 | Weaponskill | direkt |
| Job | Geirskogul (`GeirskogulPvE`) | 3555 | Ability | direkt |
| Job | Heavens' Thrust (`HeavensThrustPvE`) | 25771 | Weaponskill | direkt |
| Job | High Jump (`HighJumpPvE`) | 16478 | Ability | direkt |
| Job | Jump (`JumpPvE`) | 92 | Ability | direkt |
| Job | Lance Barrage (`LanceBarragePvE`) | 36954 | Weaponskill | direkt |
| Job | Lance Charge (`LanceChargePvE`) | 85 | Ability | direkt |
| Job | Life Surge (`LifeSurgePvE`) | 83 | Ability | direkt |
| Job | Mirage Dive (`MirageDivePvE`) | 7399 | Ability | direkt |
| Job | Nastrond (`NastrondPvE`) | 7400 | Ability | direkt |
| Job | Piercing Talon (`PiercingTalonPvE`) | 90 | Weaponskill | direkt |
| Job | Raiden Thrust (`RaidenThrustPvE`) | 16479 | Weaponskill | direkt |
| Job | Rise of the Dragon (`RiseOfTheDragonPvE`) | 36953 | Ability | direkt |
| Job | Sonic Thrust (`SonicThrustPvE`) | 7397 | Weaponskill | direkt |
| Job | Spiral Blow (`SpiralBlowPvE`) | 36955 | Weaponskill | direkt |
| Job | Starcross (`StarcrossPvE`) | 36956 | Ability | direkt |
| Job | Stardiver (`StardiverPvE`) | 16480 | Ability | direkt |
| Job | True Thrust (`TrueThrustPvE`) | 75 | Weaponskill | direkt |
| Job | Vorpal Thrust (`VorpalThrustPvE`) | 78 | Weaponskill | direkt |
| Job | Wheeling Thrust (`WheelingThrustPvE`) | 3556 | Weaponskill | direkt |
| Job | Winged Glide (`WingedGlidePvE`) | 36951 | Ability | direkt |
| Job | Wyrmwind Thrust (`WyrmwindThrustPvE`) | 25773 | Ability | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Chaos Thrust | Ausbau (Lance Mastery II) | Chaotic Spring |
| Chaos Thrust | Combo nach | Disembowel |
| Coerthan Torment | Combo nach | Sonic Thrust |
| Disembowel | Ausbau (Lance Mastery IV) | Spiral Blow |
| Doom Spike | Ausbau (Enhanced Coerthan Torment) | Draconian Fury |
| Doom Spike | Knopf wird zu | Draconian Fury |
| Draconian Fury | braucht Draconian Fire | Eigenschaft Enhanced Coerthan Torment |
| Draconian Fury | braucht Draconian Fire | Eigenschaft Lance Mastery |
| Drakesbane | Combo nach | Fang and Claw |
| Drakesbane | Combo nach | Wheeling Thrust |
| Fang and Claw | Knopf wird zu | Drakesbane |
| Full Thrust | Ausbau (Lance Mastery II) | Heavens' Thrust |
| Full Thrust | Combo nach | Vorpal Thrust |
| Jump | Ausbau (Jump Mastery) | High Jump |
| Lance Barrage | Combo nach | Raiden Thrust |
| Lance Barrage | Combo nach | True Thrust |
| Mirage Dive | braucht (Erzeuger nicht im Text) | Dive Ready |
| Nastrond | braucht Nastrond Ready | Eigenschaft True Thrust</strong></see> <i>PvE</i> (LNC DRG) [75] [Weaponskill]
    /// </summary>
    static partial void ModifyTrueThrustPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/75"><strong>True Thrust</strong></see> <i>PvE</i> (LNC DRG) [75] [Weaponskill]
    /// <para>Delivers an attack with a potency of .</para>
    /// </summary>
    
    public IBaseAction TrueThrustPvE => _TrueThrustPvECreator.Value;
    private readonly Lazy<IBaseAction> _VorpalThrustPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)78, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyVorpalThrustPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/78"><strong>Vorpal Thrust</strong></see> <i>PvE</i> (LNC DRG) [78] [Weaponskill]
    /// </summary>
    static partial void ModifyVorpalThrustPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/78"><strong>Vorpal Thrust</strong></see> <i>PvE</i> (LNC DRG) [78] [Weaponskill]
    /// <para>Delivers an attack with a potency of . Combo Action: Combo Potency:</para>
    /// </summary>
    
    public IBaseAction VorpalThrustPvE => _VorpalThrustPvECreator.Value;
    private readonly Lazy<IBaseAction> _LifeSurgePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)83, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyLifeSurgePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/83"><strong>Life Surge</strong></see> <i>PvE</i> (LNC DRG) [83] [Ability]
    /// </summary>
    static partial void ModifyLifeSurgePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/83"><strong>Life Surge</strong></see> <i>PvE</i> (LNC DRG) [83] [Ability]
    /// <para>Ensures critical damage for first weaponskill used while Life Surge is active. Duration: 5s Increases damage dealt when under an effect that raises critical hit rate. Effect cannot be applied to damage over time. Additional Effect: Absorbs a portion of damage dealt as HP</para>
    /// </summary>
    
    public IBaseAction LifeSurgePvE => _LifeSurgePvECreator.Value;
    private readonly Lazy<IBaseAction> _FullThrustPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)84, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyFullThrustPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/84"><strong>Full Thrust</strong></see> <i>PvE</i> (LNC DRG) [84] [Weaponskill]
    /// </summary>
    static partial void ModifyFullThrustPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/84"><strong>Full Thrust</strong></see> <i>PvE</i> (LNC DRG) [84] [Weaponskill]
    /// <para>Delivers an attack with a potency of 100. Combo Action: Vorpal Thrust Combo Potency: 380</para>
    /// </summary>
    
    public IBaseAction FullThrustPvE => _FullThrustPvECreator.Value;
    private readonly Lazy<IBaseAction> _LanceChargePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)85, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyLanceChargePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/85"><strong>Lance Charge</strong></see> <i>PvE</i> (LNC DRG) [85] [Ability]
    /// </summary>
    static partial void ModifyLanceChargePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/85"><strong>Lance Charge</strong></see> <i>PvE</i> (LNC DRG) [85] [Ability]
    /// <para>Increases damage dealt by 10%. Duration: 20s</para>
    /// </summary>
    
    public IBaseAction LanceChargePvE => _LanceChargePvECreator.Value;
    private readonly Lazy<IBaseAction> _DoomSpikePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)86, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyDoomSpikePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/86"><strong>Doom Spike</strong></see> <i>PvE</i> (DRG) [86] [Weaponskill]
    /// </summary>
    static partial void ModifyDoomSpikePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/86"><strong>Doom Spike</strong></see> <i>PvE</i> (DRG) [86] [Weaponskill]
    /// <para>Delivers an attack with a potency of 110 to all enemies in a straight line before you.</para>
    /// </summary>
    
    public IBaseAction DoomSpikePvE => _DoomSpikePvECreator.Value;
    private readonly Lazy<IBaseAction> _DisembowelPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)87, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyDisembowelPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/87"><strong>Disembowel</strong></see> <i>PvE</i> (LNC DRG) [87] [Weaponskill]
    /// </summary>
    static partial void ModifyDisembowelPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/87"><strong>Disembowel</strong></see> <i>PvE</i> (LNC DRG) [87] [Weaponskill]
    /// <para>Delivers an attack with a potency of . Combo Action: Combo Potency: Combo Bonus: Grants Power Surge Power Surge Effect: Increases damage dealt by 10% Duration: 30s</para>
    /// </summary>
    
    public IBaseAction DisembowelPvE => _DisembowelPvECreator.Value;
    private readonly Lazy<IBaseAction> _ChaosThrustPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)88, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyChaosThrustPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/88"><strong>Chaos Thrust</strong></see> <i>PvE</i> (LNC DRG) [88] [Weaponskill]
    /// </summary>
    static partial void ModifyChaosThrustPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/88"><strong>Chaos Thrust</strong></see> <i>PvE</i> (LNC DRG) [88] [Weaponskill]
    /// <para>Delivers an attack with a potency of 100. 140 when executed from a target's rear. Combo Action: Disembowel Combo Potency: 220 Rear Combo Potency: 260 Combo Bonus: Damage over time Potency: 40 Duration: 24s</para>
    /// </summary>
    
    public IBaseAction ChaosThrustPvE => _ChaosThrustPvECreator.Value;
    private readonly Lazy<IBaseAction> _PiercingTalonPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)90, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyPiercingTalonPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/90"><strong>Piercing Talon</strong></see> <i>PvE</i> (LNC DRG) [90] [Weaponskill]
    /// </summary>
    static partial void ModifyPiercingTalonPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/90"><strong>Piercing Talon</strong></see> <i>PvE</i> (LNC DRG) [90] [Weaponskill]
    /// <para>Delivers a ranged attack with a potency of .</para>
    /// </summary>
    
    public IBaseAction PiercingTalonPvE => _PiercingTalonPvECreator.Value;
    private readonly Lazy<IBaseAction> _JumpPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)92, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyJumpPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/92"><strong>Jump</strong></see> <i>PvE</i> (DRG) [92] [Ability]
    /// </summary>
    static partial void ModifyJumpPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/92"><strong>Jump</strong></see> <i>PvE</i> (DRG) [92] [Ability]
    /// <para>Delivers a jumping attack with a potency of . Returns you to your original position after the attack is made.</para>
    /// </summary>
    
    public IBaseAction JumpPvE => _JumpPvECreator.Value;
    private readonly Lazy<IBaseAction> _ElusiveJumpPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)94, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyElusiveJumpPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/94"><strong>Elusive Jump</strong></see> <i>PvE</i> (DRG) [94] [Ability]
    /// </summary>
    static partial void ModifyElusiveJumpPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/94"><strong>Elusive Jump</strong></see> <i>PvE</i> (DRG) [94] [Ability]
    /// <para>Executes a jump to a location 15 yalms behind you. Additional Effect: Grants Enhanced Piercing Talon Duration: 15s Cannot be executed while bound.</para>
    /// </summary>
    
    public IBaseAction ElusiveJumpPvE => _ElusiveJumpPvECreator.Value;
    private readonly Lazy<IBaseAction> _DragonfireDivePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)96, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyDragonfireDivePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/96"><strong>Dragonfire Dive</strong></see> <i>PvE</i> (DRG) [96] [Ability]
    /// </summary>
    static partial void ModifyDragonfireDivePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/96"><strong>Dragonfire Dive</strong></see> <i>PvE</i> (DRG) [96] [Ability]
    /// <para>Delivers a jumping fire-based attack to target and all enemies nearby it with a potency of 500 for the first enemy and 50% less for all remaining enemies. Cannot be executed while bound.</para>
    /// </summary>
    
    public IBaseAction DragonfireDivePvE => _DragonfireDivePvECreator.Value;
    private readonly Lazy<IBaseAction> _FangAndClawPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3554, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyFangAndClawPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3554"><strong>Fang and Claw</strong></see> <i>PvE</i> (DRG) [3554] [Weaponskill]
    /// </summary>
    static partial void ModifyFangAndClawPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3554"><strong>Fang and Claw</strong></see> <i>PvE</i> (DRG) [3554] [Weaponskill]
    /// <para>Delivers an attack with a potency of . when executed from a target's flank. Combo Action: Combo Potency: Flank Combo Potency:</para>
    /// </summary>
    
    public IBaseAction FangAndClawPvE => _FangAndClawPvECreator.Value;
    private readonly Lazy<IBaseAction> _GeirskogulPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3555, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyGeirskogulPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3555"><strong>Geirskogul</strong></see> <i>PvE</i> (DRG) [3555] [Ability]
    /// </summary>
    static partial void ModifyGeirskogulPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3555"><strong>Geirskogul</strong></see> <i>PvE</i> (DRG) [3555] [Ability]
    /// <para>Delivers an attack to all enemies in a straight line before you with a potency of for the first enemy and 50% less for all remaining enemies.</para>
    /// </summary>
    
    public IBaseAction GeirskogulPvE => _GeirskogulPvECreator.Value;
    private readonly Lazy<IBaseAction> _WheelingThrustPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3556, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyWheelingThrustPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3556"><strong>Wheeling Thrust</strong></see> <i>PvE</i> (DRG) [3556] [Weaponskill]
    /// </summary>
    static partial void ModifyWheelingThrustPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3556"><strong>Wheeling Thrust</strong></see> <i>PvE</i> (DRG) [3556] [Weaponskill]
    /// <para>Delivers an attack with a potency of . when executed from a target's rear. Combo Action: Combo Potency: Rear Combo Potency:</para>
    /// </summary>
    
    public IBaseAction WheelingThrustPvE => _WheelingThrustPvECreator.Value;
    private readonly Lazy<IBaseAction> _BattleLitanyPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3557, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBattleLitanyPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3557"><strong>Battle Litany</strong></see> <i>PvE</i> (DRG) [3557] [Ability]
    /// </summary>
    static partial void ModifyBattleLitanyPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3557"><strong>Battle Litany</strong></see> <i>PvE</i> (DRG) [3557] [Ability]
    /// <para>Increases critical hit rate of self and nearby party members by 10%. Duration: 20s</para>
    /// </summary>
    
    public IBaseAction BattleLitanyPvE => _BattleLitanyPvECreator.Value;
    private readonly Lazy<IBaseAction> _SonicThrustPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7397, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifySonicThrustPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7397"><strong>Sonic Thrust</strong></see> <i>PvE</i> (DRG) [7397] [Weaponskill]
    /// </summary>
    static partial void ModifySonicThrustPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7397"><strong>Sonic Thrust</strong></see> <i>PvE</i> (DRG) [7397] [Weaponskill]
    /// <para>Delivers an attack with a potency of 100 to all enemies in a straight line before you. Combo Action: Combo Potency: 120 Combo Bonus: Grants Power Surge Power Surge Effect: Increases damage dealt by 10% Duration: 30s</para>
    /// </summary>
    
    public IBaseAction SonicThrustPvE => _SonicThrustPvECreator.Value;
    private readonly Lazy<IBaseAction> _MirageDivePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7399, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyMirageDivePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7399"><strong>Mirage Dive</strong></see> <i>PvE</i> (DRG) [7399] [Ability]
    /// </summary>
    static partial void ModifyMirageDivePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7399"><strong>Mirage Dive</strong></see> <i>PvE</i> (DRG) [7399] [Ability]
    /// <para>Delivers an attack with a potency of 380. Can only be executed when Dive Ready.</para>
    /// </summary>
    
    public IBaseAction MirageDivePvE => _MirageDivePvECreator.Value;
    private readonly Lazy<IBaseAction> _NastrondPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7400, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyNastrondPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7400"><strong>Nastrond</strong></see> <i>PvE</i> (DRG) [7400] [Ability]
    /// </summary>
    static partial void ModifyNastrondPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7400"><strong>Nastrond</strong></see> <i>PvE</i> (DRG) [7400] [Ability]
    /// <para>Delivers an attack to all enemies in a straight line before you with a potency of for the first enemy and 50% less for all remaining enemies. Can only be executed while Nastrond Ready.</para>
    /// </summary>
    
    public IBaseAction NastrondPvE => _NastrondPvECreator.Value;
    private readonly Lazy<IBaseAction> _CoerthanTormentPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16477, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyCoerthanTormentPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16477"><strong>Coerthan Torment</strong></see> <i>PvE</i> (DRG) [16477] [Weaponskill]
    /// </summary>
    static partial void ModifyCoerthanTormentPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16477"><strong>Coerthan Torment</strong></see> <i>PvE</i> (DRG) [16477] [Weaponskill]
    /// <para>Delivers an attack with a potency of 100 to all enemies in a straight line before you. Combo Action: Sonic Thrust Combo Potency: 150</para>
    /// </summary>
    
    public IBaseAction CoerthanTormentPvE => _CoerthanTormentPvECreator.Value;
    private readonly Lazy<IBaseAction> _HighJumpPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16478, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHighJumpPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16478"><strong>High Jump</strong></see> <i>PvE</i> (DRG) [16478] [Ability]
    /// </summary>
    static partial void ModifyHighJumpPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16478"><strong>High Jump</strong></see> <i>PvE</i> (DRG) [16478] [Ability]
    /// <para>Delivers a jumping attack with a potency of 400. Returns you to your original position after the attack is made. Additional Effect: Grants Dive Ready Duration: 15s</para>
    /// </summary>
    
    public IBaseAction HighJumpPvE => _HighJumpPvECreator.Value;
    private readonly Lazy<IBaseAction> _RaidenThrustPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16479, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRaidenThrustPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16479"><strong>Raiden Thrust</strong></see> <i>PvE</i> (DRG) [16479] [Weaponskill]
    /// </summary>
    static partial void ModifyRaidenThrustPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16479"><strong>Raiden Thrust</strong></see> <i>PvE</i> (DRG) [16479] [Weaponskill]
    /// <para>Delivers an attack with a potency of . Can only be executed while under the effect of Draconian Fire. ※This action cannot be assigned to a hotbar. ※True Thrust changes to Raiden Thrust when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction RaidenThrustPvE => _RaidenThrustPvECreator.Value;
    private readonly Lazy<IBaseAction> _StardiverPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16480, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyStardiverPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16480"><strong>Stardiver</strong></see> <i>PvE</i> (DRG) [16480] [Ability]
    /// </summary>
    static partial void ModifyStardiverPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16480"><strong>Stardiver</strong></see> <i>PvE</i> (DRG) [16480] [Ability]
    /// <para>Delivers a jumping fire-based attack to target and all enemies nearby it with a potency of for the first enemy and 40% less for all remaining enemies. Can only be executed while under the effect of Life of the Dragon. Cannot be executed while bound.</para>
    /// </summary>
    
    public IBaseAction StardiverPvE => _StardiverPvECreator.Value;
    private readonly Lazy<IBaseAction> _DraconianFuryPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25770, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyDraconianFuryPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25770"><strong>Draconian Fury</strong></see> <i>PvE</i> (DRG) [25770] [Weaponskill]
    /// </summary>
    static partial void ModifyDraconianFuryPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25770"><strong>Draconian Fury</strong></see> <i>PvE</i> (DRG) [25770] [Weaponskill]
    /// <para>Delivers an attack with a potency of 130 to all enemies in a straight line before you. Can only be executed while under the effect of Draconian Fire. ※This action cannot be assigned to a hotbar. ※Doom Spike changes to Draconian Fury when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction DraconianFuryPvE => _DraconianFuryPvECreator.Value;
    private readonly Lazy<IBaseAction> _HeavensThrustPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25771, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHeavensThrustPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25771"><strong>Heavens' Thrust</strong></see> <i>PvE</i> (DRG) [25771] [Weaponskill]
    /// </summary>
    static partial void ModifyHeavensThrustPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25771"><strong>Heavens' Thrust</strong></see> <i>PvE</i> (DRG) [25771] [Weaponskill]
    /// <para>Delivers an attack with a potency of . Combo Action: Combo Potency:</para>
    /// </summary>
    
    public IBaseAction HeavensThrustPvE => _HeavensThrustPvECreator.Value;
    private readonly Lazy<IBaseAction> _ChaoticSpringPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25772, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyChaoticSpringPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25772"><strong>Chaotic Spring</strong></see> <i>PvE</i> (DRG) [25772] [Weaponskill]
    /// </summary>
    static partial void ModifyChaoticSpringPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25772"><strong>Chaotic Spring</strong></see> <i>PvE</i> (DRG) [25772] [Weaponskill]
    /// <para>Delivers an attack with a potency of . when executed from a target's rear. Combo Action: Combo Potency: Rear Combo Potency: Combo Bonus: Damage over time Potency: 45 Duration: 24s</para>
    /// </summary>
    
    public IBaseAction ChaoticSpringPvE => _ChaoticSpringPvECreator.Value;
    private readonly Lazy<IBaseAction> _WyrmwindThrustPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25773, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyWyrmwindThrustPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25773"><strong>Wyrmwind Thrust</strong></see> <i>PvE</i> (DRG) [25773] [Ability]
    /// </summary>
    static partial void ModifyWyrmwindThrustPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25773"><strong>Wyrmwind Thrust</strong></see> <i>PvE</i> (DRG) [25773] [Ability]
    /// <para>Delivers an attack to all enemies in a straight line before you with a potency of for the first enemy and 50% less for all remaining enemies. Firstminds' Focus Cost: 2</para>
    /// </summary>
    
    public IBaseAction WyrmwindThrustPvE => _WyrmwindThrustPvECreator.Value;
    private readonly Lazy<IBaseAction> _RaidenThrustPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29486, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRaidenThrustPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29486"><strong>Raiden Thrust</strong></see> <i>PvP</i> (DRG) [29486] [Weaponskill]
    /// </summary>
    static partial void ModifyRaidenThrustPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29486"><strong>Raiden Thrust</strong></see> <i>PvP</i> (DRG) [29486] [Weaponskill]
    /// <para>Delivers an attack with a potency of 5,000. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction RaidenThrustPvP => _RaidenThrustPvPCreator.Value;
    private readonly Lazy<IBaseAction> _FangAndClawPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29487, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyFangAndClawPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29487"><strong>Fang and Claw</strong></see> <i>PvP</i> (DRG) [29487] [Weaponskill]
    /// </summary>
    static partial void ModifyFangAndClawPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29487"><strong>Fang and Claw</strong></see> <i>PvP</i> (DRG) [29487] [Weaponskill]
    /// <para>Delivers an attack with a potency of 6,000. Combo Action: Raiden Thrust ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction FangAndClawPvP => _FangAndClawPvPCreator.Value;
    private readonly Lazy<IBaseAction> _WheelingThrustPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29488, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyWheelingThrustPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29488"><strong>Wheeling Thrust</strong></see> <i>PvP</i> (DRG) [29488] [Weaponskill]
    /// </summary>
    static partial void ModifyWheelingThrustPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29488"><strong>Wheeling Thrust</strong></see> <i>PvP</i> (DRG) [29488] [Weaponskill]
    /// <para>Delivers an attack with a potency of 7,000. Combo Action: Fang and Claw ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction WheelingThrustPvP => _WheelingThrustPvPCreator.Value;
    private readonly Lazy<IBaseAction> _HeavensThrustPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29489, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHeavensThrustPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29489"><strong>Heavens' Thrust</strong></see> <i>PvP</i> (DRG) [29489] [Weaponskill]
    /// </summary>
    static partial void ModifyHeavensThrustPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29489"><strong>Heavens' Thrust</strong></see> <i>PvP</i> (DRG) [29489] [Weaponskill]
    /// <para>Delivers an attack with a potency of 10,000. Can only be executed while under the effect of Heavensent. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction HeavensThrustPvP => _HeavensThrustPvPCreator.Value;
    private readonly Lazy<IBaseAction> _ChaoticSpringPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29490, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyChaoticSpringPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29490"><strong>Chaotic Spring</strong></see> <i>PvP</i> (DRG) [29490] [Weaponskill]
    /// </summary>
    static partial void ModifyChaoticSpringPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29490"><strong>Chaotic Spring</strong></see> <i>PvP</i> (DRG) [29490] [Weaponskill]
    /// <para>Delivers an attack with a potency of 9,000. Ignores the effects of Guard when dealing damage. Additional Effect: Absorbs 200% of damage dealt as HP This weaponskill does not share a recast timer with any other actions.</para>
    /// </summary>
    
    public IBaseAction ChaoticSpringPvP => _ChaoticSpringPvPCreator.Value;
    private readonly Lazy<IBaseAction> _GeirskogulPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29491, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyGeirskogulPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29491"><strong>Geirskogul</strong></see> <i>PvP</i> (DRG) [29491] [Ability]
    /// </summary>
    static partial void ModifyGeirskogulPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29491"><strong>Geirskogul</strong></see> <i>PvP</i> (DRG) [29491] [Ability]
    /// <para>Delivers an attack with a potency of 5,000 to all enemies in a straight line before you. Additional Effect: Grants Life of the Dragon Life of the Dragon Effect: Increases damage dealt by 25% and damage suffered by 15% Duration: 10s Additional Effect: Grants Nastrond Ready Duration: 10s Additional Effect: Grants Starcross Ready Duration: 10s ※Action changes to Nastrond while under the effect of Nastrond Ready. ※Wheeling Thrust Combo changes to Starcross while under the effect of Starcross Ready.</para>
    /// </summary>
    
    public IBaseAction GeirskogulPvP => _GeirskogulPvPCreator.Value;
    private readonly Lazy<IBaseAction> _NastrondPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29492, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyNastrondPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29492"><strong>Nastrond</strong></see> <i>PvP</i> (DRG) [29492] [Ability]
    /// </summary>
    static partial void ModifyNastrondPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29492"><strong>Nastrond</strong></see> <i>PvP</i> (DRG) [29492] [Ability]
    /// <para>Delivers an attack with a potency of 5,000 to all enemies in a straight line before you. Potency increases up to 10,000 as the target's HP decreases reaching its maximum value when the target has 50% HP or less. Life of the Dragon effect fades upon execution. Can only be executed while under the effect of Nastrond Ready. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction NastrondPvP => _NastrondPvPCreator.Value;
    private readonly Lazy<IBaseAction> _HighJumpPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29493, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHighJumpPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29493"><strong>High Jump</strong></see> <i>PvP</i> (DRG) [29493] [Ability]
    /// </summary>
    static partial void ModifyHighJumpPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29493"><strong>High Jump</strong></see> <i>PvP</i> (DRG) [29493] [Ability]
    /// <para>Delivers a jumping attack with a potency of 6,000. Additional Effect: Grants Heavensent Duration: 10s Cannot be executed while bound. ※Wheeling Thrust Combo changes to Heavens' Thrust while under the effect of Heavensent.</para>
    /// </summary>
    
    public IBaseAction HighJumpPvP => _HighJumpPvPCreator.Value;
    private readonly Lazy<IBaseAction> _ElusiveJumpPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29494, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyElusiveJumpPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29494"><strong>Elusive Jump</strong></see> <i>PvP</i> (DRG) [29494] [Ability]
    /// </summary>
    static partial void ModifyElusiveJumpPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29494"><strong>Elusive Jump</strong></see> <i>PvP</i> (DRG) [29494] [Ability]
    /// <para>Executes a jump to a location 15 yalms behind you. Additional Effect: Removes Heavy and Bind Additional Effect: Increases movement speed by 25% Duration: 5s Additional Effect: Grants Firstminds' Focus Duration: 10s ※Action changes to Wyrmwind Thrust upon execution.</para>
    /// </summary>
    
    public IBaseAction ElusiveJumpPvP => _ElusiveJumpPvPCreator.Value;
    private readonly Lazy<IBaseAction> _WyrmwindThrustPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29495, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyWyrmwindThrustPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29495"><strong>Wyrmwind Thrust</strong></see> <i>PvP</i> (DRG) [29495] [Weaponskill]
    /// </summary>
    static partial void ModifyWyrmwindThrustPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29495"><strong>Wyrmwind Thrust</strong></see> <i>PvP</i> (DRG) [29495] [Weaponskill]
    /// <para>Delivers an attack with a potency of 8,000 to all enemies in a straight line before you. Potency increases up to 16,000 the farther away you are from the target reaching its maximum value when the target is 15 yalms away. Can only be executed while under the effect of Firstminds' Focus. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction WyrmwindThrustPvP => _WyrmwindThrustPvPCreator.Value;
    private readonly Lazy<IBaseAction> _HorridRoarPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29496, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHorridRoarPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29496"><strong>Horrid Roar</strong></see> <i>PvP</i> (DRG) [29496] [Ability]
    /// </summary>
    static partial void ModifyHorridRoarPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29496"><strong>Horrid Roar</strong></see> <i>PvP</i> (DRG) [29496] [Ability]
    /// <para>Deals unaspected damage with a potency of 3,000 to all nearby enemies. Additional Effect: Reduces damage targets deal to you by 50% Duration: 10s</para>
    /// </summary>
    
    public IBaseAction HorridRoarPvP => _HorridRoarPvPCreator.Value;
    private readonly Lazy<IBaseAction> _WingedGlidePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)36951, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyWingedGlidePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/36951"><strong>Winged Glide</strong></see> <i>PvE</i> (DRG) [36951] [Ability]
    /// </summary>
    static partial void ModifyWingedGlidePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/36951"><strong>Winged Glide</strong></see> <i>PvE</i> (DRG) [36951] [Ability]
    /// <para>Rush to a targeted enemy's location. Cannot be executed while bound.</para>
    /// </summary>
    
    public IBaseAction WingedGlidePvE => _WingedGlidePvECreator.Value;
    private readonly Lazy<IBaseAction> _DrakesbanePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)36952, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyDrakesbanePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/36952"><strong>Drakesbane</strong></see> <i>PvE</i> (DRG) [36952] [Weaponskill]
    /// </summary>
    static partial void ModifyDrakesbanePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/36952"><strong>Drakesbane</strong></see> <i>PvE</i> (DRG) [36952] [Weaponskill]
    /// <para>Delivers an attack with a potency of . Combo Action: Wheeling Thrust or Fang and Claw Can only be executed after successfully landing Wheeling Thrust or Fang and Claw as a combo action. ※This action cannot be assigned to a hotbar. ※Wheeling Thrust and Fang and Claw change to Drakesbane when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction DrakesbanePvE => _DrakesbanePvECreator.Value;
    private readonly Lazy<IBaseAction> _RiseOfTheDragonPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)36953, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRiseOfTheDragonPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/36953"><strong>Rise of the Dragon</strong></see> <i>PvE</i> (DRG) [36953] [Ability]
    /// </summary>
    static partial void ModifyRiseOfTheDragonPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/36953"><strong>Rise of the Dragon</strong></see> <i>PvE</i> (DRG) [36953] [Ability]
    /// <para>Deals physical damage to target and all enemies nearby it with a potency of 550 for the first enemy and 50% less for all remaining enemies. Can only be executed while under the effect of Dragon's Flight.</para>
    /// </summary>
    
    public IBaseAction RiseOfTheDragonPvE => _RiseOfTheDragonPvECreator.Value;
    private readonly Lazy<IBaseAction> _LanceBarragePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)36954, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyLanceBarragePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/36954"><strong>Lance Barrage</strong></see> <i>PvE</i> (DRG) [36954] [Weaponskill]
    /// </summary>
    static partial void ModifyLanceBarragePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/36954"><strong>Lance Barrage</strong></see> <i>PvE</i> (DRG) [36954] [Weaponskill]
    /// <para>Delivers an attack with a potency of 130. Combo Action: True Thrust or Raiden Thrust Combo Potency: 340</para>
    /// </summary>
    
    public IBaseAction LanceBarragePvE => _LanceBarragePvECreator.Value;
    private readonly Lazy<IBaseAction> _SpiralBlowPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)36955, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifySpiralBlowPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/36955"><strong>Spiral Blow</strong></see> <i>PvE</i> (DRG) [36955] [Weaponskill]
    /// </summary>
    static partial void ModifySpiralBlowPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/36955"><strong>Spiral Blow</strong></see> <i>PvE</i> (DRG) [36955] [Weaponskill]
    /// <para>Delivers an attack with a potency of 140. Combo Action: True Thrust or Raiden Thrust Combo Potency: 300 Combo Bonus: Grants Power Surge Power Surge Effect: Increases damage dealt by 10% Duration: 30s</para>
    /// </summary>
    
    public IBaseAction SpiralBlowPvE => _SpiralBlowPvECreator.Value;
    private readonly Lazy<IBaseAction> _StarcrossPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)36956, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyStarcrossPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/36956"><strong>Starcross</strong></see> <i>PvE</i> (DRG) [36956] [Ability]
    /// </summary>
    static partial void ModifyStarcrossPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/36956"><strong>Starcross</strong></see> <i>PvE</i> (DRG) [36956] [Ability]
    /// <para>Deals physical damage to target and all enemies nearby it with a potency of 1,000 for the first enemy and 40% less for all remaining enemies. Can only be executed while under the effect of Starcross Ready.</para>
    /// </summary>
    
    public IBaseAction StarcrossPvE => _StarcrossPvECreator.Value;
    private readonly Lazy<IBaseAction> _DrakesbanePvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41449, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyDrakesbanePvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41449"><strong>Drakesbane</strong></see> <i>PvP</i> (DRG) [41449] [Weaponskill]
    /// </summary>
    static partial void ModifyDrakesbanePvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41449"><strong>Drakesbane</strong></see> <i>PvP</i> (DRG) [41449] [Weaponskill]
    /// <para>Delivers an attack with a potency of 8,000. Combo Action: Wheeling Thrust ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction DrakesbanePvP => _DrakesbanePvPCreator.Value;
    private readonly Lazy<IBaseAction> _StarcrossPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41450, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyStarcrossPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41450"><strong>Starcross</strong></see> <i>PvP</i> (DRG) [41450] [Weaponskill]
    /// </summary>
    static partial void ModifyStarcrossPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41450"><strong>Starcross</strong></see> <i>PvP</i> (DRG) [41450] [Weaponskill]
    /// <para>Delivers an attack with a potency of 12,000 to target and all enemies nearby it. Can only be executed while under the effect of Starcross Ready. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction StarcrossPvP => _StarcrossPvPCreator.Value;

    private IBaseAction[] _AllBaseActions = null;
    
    /// <inheritdoc/>
    public override IBaseAction[] AllBaseActions => _AllBaseActions ??= [
    	TrueThrustPvE, VorpalThrustPvE, LifeSurgePvE, FullThrustPvE, LanceChargePvE, DoomSpikePvE, DisembowelPvE, ChaosThrustPvE, PiercingTalonPvE, JumpPvE, ElusiveJumpPvE, DragonfireDivePvE, FangAndClawPvE, GeirskogulPvE, WheelingThrustPvE, BattleLitanyPvE, SonicThrustPvE, MirageDivePvE, NastrondPvE, CoerthanTormentPvE, HighJumpPvE, RaidenThrustPvE, StardiverPvE, DraconianFuryPvE, HeavensThrustPvE, ChaoticSpringPvE, WyrmwindThrustPvE, RaidenThrustPvP, FangAndClawPvP, WheelingThrustPvP, HeavensThrustPvP, ChaoticSpringPvP, GeirskogulPvP, NastrondPvP, HighJumpPvP, ElusiveJumpPvP, WyrmwindThrustPvP, HorridRoarPvP, WingedGlidePvE, DrakesbanePvE, RiseOfTheDragonPvE, LanceBarragePvE, SpiralBlowPvE, StarcrossPvE, DrakesbanePvP, StarcrossPvP,
    	..base.AllBaseActions,
    ];

private readonly Lazy<IBaseAction> _BraverPvECreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)200, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifyBraverPvE(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/200"><strong>Braver</strong></see> <i>PvE</i> (All Classes) [200] [Limit Break]
/// </summary>
static partial void ModifyBraverPvE(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/200"><strong>Braver</strong></see> <i>PvE</i> (All Classes) [200] [Limit Break]
/// <para>Delivers an attack with a potency of 2,400.</para>
/// </summary>

public IBaseAction BraverPvE => _BraverPvECreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/200"><strong>Braver</strong></see> <i>PvE</i> (All Classes) [200] [Limit Break]
/// <para>Delivers an attack with a potency of 2,400.</para>
/// </summary>
private sealed protected override IBaseAction LimitBreak1 => BraverPvE;
private readonly Lazy<IBaseAction> _BladedancePvECreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)201, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifyBladedancePvE(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/201"><strong>Bladedance</strong></see> <i>PvE</i> (All Classes) [201] [Limit Break]
/// </summary>
static partial void ModifyBladedancePvE(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/201"><strong>Bladedance</strong></see> <i>PvE</i> (All Classes) [201] [Limit Break]
/// <para>Delivers an attack with a potency of 5,250.</para>
/// </summary>

public IBaseAction BladedancePvE => _BladedancePvECreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/201"><strong>Bladedance</strong></see> <i>PvE</i> (All Classes) [201] [Limit Break]
/// <para>Delivers an attack with a potency of 5,250.</para>
/// </summary>
private sealed protected override IBaseAction LimitBreak2 => BladedancePvE;
private readonly Lazy<IBaseAction> _DragonsongDivePvECreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)4242, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifyDragonsongDivePvE(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/4242"><strong>Dragonsong Dive</strong></see> <i>PvE</i> (All Classes) [4242] [Limit Break]
/// </summary>
static partial void ModifyDragonsongDivePvE(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/4242"><strong>Dragonsong Dive</strong></see> <i>PvE</i> (All Classes) [4242] [Limit Break]
/// <para></para>
/// </summary>

public IBaseAction DragonsongDivePvE => _DragonsongDivePvECreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/4242"><strong>Dragonsong Dive</strong></see> <i>PvE</i> (All Classes) [4242] [Limit Break]
/// <para></para>
/// </summary>
private sealed protected override IBaseAction LimitBreak3 => DragonsongDivePvE;
private readonly Lazy<IBaseAction> _SkyHighPvPCreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)29497, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifySkyHighPvP(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/29497"><strong>Sky High</strong></see> <i>PvP</i> (DRG) [29497] [Limit Break]
/// </summary>
static partial void ModifySkyHighPvP(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/29497"><strong>Sky High</strong></see> <i>PvP</i> (DRG) [29497] [Limit Break]
/// <para>Jump high into the air preventing enemies from targeting you. Movement is still possible before landing and speed is increased by 50%. Duration: 4s Additional Effect: Removes Heavy and Bind Executes Sky Shatter automatically when effect duration expires. Can only be executed when the limit gauge is full. Gauge Charge Time: 90s ※Action changes to Sky Shatter upon execution.</para>
/// </summary>

private IBaseAction SkyHighPvP => _SkyHighPvPCreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/29497"><strong>Sky High</strong></see> <i>PvP</i> (DRG) [29497] [Limit Break]
/// <para>Jump high into the air preventing enemies from targeting you. Movement is still possible before landing and speed is increased by 50%. Duration: 4s Additional Effect: Removes Heavy and Bind Executes Sky Shatter automatically when effect duration expires. Can only be executed when the limit gauge is full. Gauge Charge Time: 90s ※Action changes to Sky Shatter upon execution.</para>
/// </summary>
private sealed protected override IBaseAction LimitBreakPvP => SkyHighPvP;

#endregion

#region Traits

    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/50163"><strong>Life of the Dragon |
| Raiden Thrust | braucht Draconian Fire | Eigenschaft Enhanced Coerthan Torment |
| Raiden Thrust | braucht Draconian Fire | Eigenschaft Lance Mastery |
| Rise of the Dragon | braucht Dragon's Flight | Eigenschaft Enhanced Dragonfire Dive |
| Spiral Blow | Combo nach | Raiden Thrust |
| Spiral Blow | Combo nach | True Thrust |
| Starcross | braucht Starcross Ready | Eigenschaft Enhanced Stardiver |
| Stardiver | braucht (Erzeuger nicht im Text) | Life of the Dragon |
| True Thrust | Ausbau (Lance Mastery) | Raiden Thrust |
| True Thrust | Knopf wird zu | Raiden Thrust |
| Vorpal Thrust | Ausbau (Lance Mastery IV) | Lance Barrage |
| Wheeling Thrust | Knopf wird zu | Drakesbane |
| Wyrmwind Thrust | kostet | Firstminds' Focus |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Battle Litany | Regel sperrt vorher | Disembowel |
| Battle Litany | Regel sperrt vorher | Stardiver |
| Bloodbath | Regel sperrt vorher | Stardiver |
| Chaos Thrust | ComboIds | Disembowel |
| Chaos Thrust | ComboIds | Spiral Blow |
| Chaotic Spring | ComboIds | Spiral Blow |
| Coerthan Torment | ComboIds | Sonic Thrust |
| Disembowel | ComboIds | Raiden Thrust |
| Disembowel | ComboIds | True Thrust |
| Dragonfire Dive | Regel prüft | Battle Litany |
| Dragonfire Dive | Regel sperrt vorher | Disembowel |
| Dragonfire Dive | Regel sperrt vorher | Stardiver |
| Drakesbane | ComboIds | Fang and Claw |
| Drakesbane | ComboIds | Wheeling Thrust |
| Elusive Jump | Regel sperrt vorher | Stardiver |
| Fang and Claw | ComboIds | Heavens' Thrust |
| Feint | Regel sperrt vorher | Stardiver |
| Full Thrust | ComboIds | Lance Barrage |
| Full Thrust | ComboIds | Vorpal Thrust |
| Geirskogul | Regel sperrt vorher | Disembowel |
| Geirskogul | Regel sperrt vorher | Stardiver |
| Heavens' Thrust | ComboIds | Lance Barrage |
| Heavens' Thrust | ComboIds | Vorpal Thrust |
| High Jump | Regel sperrt vorher | Disembowel |
| High Jump | Regel sperrt vorher | Dragonfire Dive |
| High Jump | Regel prüft | Geirskogul |
| High Jump | Regel sperrt vorher | Stardiver |
| Jump | Regel sperrt vorher | Disembowel |
| Jump | Regel sperrt vorher | Dragonfire Dive |
| Jump | Regel prüft | High Jump |
| Jump | Regel prüft | Lance Charge |
| Jump | Regel sperrt vorher | Stardiver |
| Lance Barrage | ComboIds | Raiden Thrust |
| Lance Barrage | ComboIds | True Thrust |
| Lance Charge | Regel prüft | Battle Litany |
| Lance Charge | Regel sperrt vorher | Disembowel |
| Lance Charge | Regel sperrt vorher | Stardiver |
| Life Surge | Regel sperrt vorher | Disembowel |
| Life Surge | Regel sperrt vorher | Stardiver |
| Mirage Dive | Regel sperrt vorher | Disembowel |
| Mirage Dive | Regel sperrt vorher | Dragonfire Dive |
| Mirage Dive | StatusNeed DiveReady | High Jump |
| Mirage Dive | StatusNeed DiveReady | Jump |
| Mirage Dive | Regel sperrt vorher | Stardiver |
| Nastrond | Regel sperrt vorher | Disembowel |
| Nastrond | Regel sperrt vorher | Dragonfire Dive |
| Nastrond | StatusNeed NastrondReady | Geirskogul |
| Nastrond | Regel sperrt vorher | Stardiver |
| Piercing Talon | Regel prüft | Winged Glide |
| Rise of the Dragon | Regel sperrt vorher | Disembowel |
| Rise of the Dragon | Regel sperrt vorher | Dragonfire Dive |
| Rise of the Dragon | StatusNeed DragonsFlight | Dragonfire Dive |
| Rise of the Dragon | Regel sperrt vorher | Stardiver |
| Second Wind | Regel sperrt vorher | Stardiver |
| Sonic Thrust | ComboIds | Doom Spike |
| Sonic Thrust | ComboIds | Draconian Fury |
| Spiral Blow | ComboIds | Raiden Thrust |
| Spiral Blow | ComboIds | True Thrust |
| Starcross | Regel sperrt vorher | Disembowel |
| Starcross | Regel sperrt vorher | Dragonfire Dive |
| Starcross | Regel sperrt vorher | Stardiver |
| Starcross | StatusNeed StarcrossReady | Stardiver |
| Vorpal Thrust | ComboIds | Raiden Thrust |
| Vorpal Thrust | ComboIds | True Thrust |
| Wheeling Thrust | ComboIds | Chaotic Spring |
| Winged Glide | Regel sperrt vorher | Stardiver |
| Wyrmwind Thrust | Regel sperrt vorher | Disembowel |
| Wyrmwind Thrust | Regel prüft | Draconian Fury |
| Wyrmwind Thrust | Regel sperrt vorher | Dragonfire Dive |
| Wyrmwind Thrust | Regel prüft | Raiden Thrust |
| Wyrmwind Thrust | Regel sperrt vorher | Stardiver |

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `BladedancePvE`, `BraverPvE`, `DragonsongDivePvE`

## Nicht direkt genutzt

keine

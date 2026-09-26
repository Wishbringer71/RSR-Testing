# WHM — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `WhiteMageRotation`
- Rotation: `RotationSolver/RebornRotations/Healer/WHM_Reborn.cs`
- Matrix als Tabelle: `WHM.csv`

## Nutzung

direkt: 41 · ungenutzt: 2

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Heiler | Esuna (`EsunaPvE`) | 7568 | Spell | direkt |
| Heiler | Lucid Dreaming (`LucidDreamingPvE`) | 7562 | Ability | direkt |
| Heiler | Repose (`ReposePvE`) | 16560 | Spell | ungenutzt |
| Heiler | Rescue (`RescuePvE`) | 7571 | Ability | ungenutzt |
| Heiler | Surecast (`SurecastPvE`) | 7559 | Ability | direkt |
| Heiler | Swiftcast (`SwiftcastPvE`) | 7561 | Ability | direkt |
| Job | Aero (`AeroPvE`) | 121 | Spell | direkt |
| Job | Aero II (`AeroIiPvE`) | 132 | Spell | direkt |
| Job | Aetherial Shift (`AetherialShiftPvE`) | 37008 | Ability | direkt |
| Job | Afflatus Misery (`AfflatusMiseryPvE`) | 16535 | Spell | direkt |
| Job | Afflatus Rapture (`AfflatusRapturePvE`) | 16534 | Spell | direkt |
| Job | Afflatus Solace (`AfflatusSolacePvE`) | 16531 | Spell | direkt |
| Job | Aquaveil (`AquaveilPvE`) | 25861 | Ability | direkt |
| Job | Assize (`AssizePvE`) | 3571 | Ability | direkt |
| Job | Asylum (`AsylumPvE`) | 3569 | Ability | direkt |
| Job | Benediction (`BenedictionPvE`) | 140 | Ability | direkt |
| Job | Cure (`CurePvE`) | 120 | Spell | direkt |
| Job | Cure II (`CureIiPvE`) | 135 | Spell | direkt |
| Job | Cure III (`CureIiiPvE`) | 131 | Spell | direkt |
| Job | Dia (`DiaPvE`) | 16532 | Spell | direkt |
| Job | Divine Benison (`DivineBenisonPvE`) | 7432 | Ability | direkt |
| Job | Divine Caress (`DivineCaressPvE`) | 37011 | Ability | direkt |
| Job | Glare (`GlarePvE`) | 16533 | Spell | direkt |
| Job | Glare III (`GlareIiiPvE`) | 25859 | Spell | direkt |
| Job | Glare IV (`GlareIvPvE`) | 37009 | Spell | direkt |
| Job | Holy (`HolyPvE`) | 139 | Spell | direkt |
| Job | Holy III (`HolyIiiPvE`) | 25860 | Spell | direkt |
| Job | Liturgy of the Bell (`LiturgyOfTheBellPvE`) | 25862 | Ability | direkt |
| Job | Medica (`MedicaPvE`) | 124 | Spell | direkt |
| Job | Medica II (`MedicaIiPvE`) | 133 | Spell | direkt |
| Job | Medica III (`MedicaIiiPvE`) | 37010 | Spell | direkt |
| Job | Plenary Indulgence (`PlenaryIndulgencePvE`) | 7433 | Ability | direkt |
| Job | Presence of Mind (`PresenceOfMindPvE`) | 136 | Ability | direkt |
| Job | Raise (`RaisePvE`) | 125 | Spell | direkt |
| Job | Regen (`RegenPvE`) | 137 | Spell | direkt |
| Job | Stone (`StonePvE`) | 119 | Spell | direkt |
| Job | Stone II (`StoneIiPvE`) | 127 | Spell | direkt |
| Job | Stone III (`StoneIiiPvE`) | 3568 | Spell | direkt |
| Job | Stone IV (`StoneIvPvE`) | 7431 | Spell | direkt |
| Job | Temperance (`TemperancePvE`) | 16536 | Ability | direkt |
| Job | Tetragrammaton (`TetragrammatonPvE`) | 3570 | Ability | direkt |
| Job | Thin Air (`ThinAirPvE`) | 7430 | Ability | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Aero II | Ausbau (Aero Mastery II) | Dia |
| Afflatus Misery | braucht (Erzeuger nicht im Text) | the Blood Lily is in full bloom |
| Afflatus Rapture | kostet | Blood Lily Healing |
| Afflatus Solace | kostet | Healing |
| Divine Caress | braucht Divine Grace | Eigenschaft Enhanced Temperance |
| Glare IV | braucht Sacred Sight | Eigenschaft Enhanced Presence of Mind |
| Glare | Ausbau (Glare Mastery) | Glare III |
| Holy | Ausbau (Holy Mastery) | Holy III |
| Medica II | Ausbau (Medica Mastery) | Medica III |
| Stone II | Ausbau (Stone</strong></see> <i>PvE</i> (CNJ WHM) [119] [Spell]
    /// </summary>
    static partial void ModifyStonePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/119"><strong>Stone</strong></see> <i>PvE</i> (CNJ WHM) [119] [Spell]
    /// <para>Deals earth damage with a potency of 140.</para>
    /// </summary>
    
    public IBaseAction StonePvE => _StonePvECreator.Value;
    private readonly Lazy<IBaseAction> _CurePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)120, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyCurePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/120"><strong>Cure</strong></see> <i>PvE</i> (CNJ WHM) [120] [Spell]
    /// </summary>
    static partial void ModifyCurePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/120"><strong>Cure</strong></see> <i>PvE</i> (CNJ WHM) [120] [Spell]
    /// <para>Restores target's HP. Cure Potency:</para>
    /// </summary>
    
    public IBaseAction CurePvE => _CurePvECreator.Value;
    private readonly Lazy<IBaseAction> _AeroPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)121, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyAeroPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/121"><strong>Aero</strong></see> <i>PvE</i> (CNJ WHM) [121] [Spell]
    /// </summary>
    static partial void ModifyAeroPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/121"><strong>Aero</strong></see> <i>PvE</i> (CNJ WHM) [121] [Spell]
    /// <para>Deals wind damage with a potency of 50. Additional Effect: Wind damage over time Potency: 30 Duration: 30s</para>
    /// </summary>
    
    public IBaseAction AeroPvE => _AeroPvECreator.Value;
    private readonly Lazy<IBaseAction> _MedicaPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)124, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyMedicaPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/124"><strong>Medica</strong></see> <i>PvE</i> (CNJ WHM) [124] [Spell]
    /// </summary>
    static partial void ModifyMedicaPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/124"><strong>Medica</strong></see> <i>PvE</i> (CNJ WHM) [124] [Spell]
    /// <para>Restores own HP and the HP of all nearby party members. Cure Potency:</para>
    /// </summary>
    
    public IBaseAction MedicaPvE => _MedicaPvECreator.Value;
    private readonly Lazy<IBaseAction> _RaisePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)125, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRaisePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/125"><strong>Raise</strong></see> <i>PvE</i> (CNJ WHM) [125] [Spell]
    /// </summary>
    static partial void ModifyRaisePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/125"><strong>Raise</strong></see> <i>PvE</i> (CNJ WHM) [125] [Spell]
    /// <para>Resurrects target to a weakened state.</para>
    /// </summary>
    
    public IBaseAction RaisePvE => _RaisePvECreator.Value;
    private readonly Lazy<IBaseAction> _StoneIiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)127, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyStoneIiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/127"><strong>Stone II</strong></see> <i>PvE</i> (CNJ WHM) [127] [Spell]
    /// </summary>
    static partial void ModifyStoneIiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/127"><strong>Stone II</strong></see> <i>PvE</i> (CNJ WHM) [127] [Spell]
    /// <para>Deals earth damage with a potency of 190.</para>
    /// </summary>
    
    public IBaseAction StoneIiPvE => _StoneIiPvECreator.Value;
    private readonly Lazy<IBaseAction> _CureIiiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)131, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyCureIiiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/131"><strong>Cure III</strong></see> <i>PvE</i> (WHM) [131] [Spell]
    /// </summary>
    static partial void ModifyCureIiiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/131"><strong>Cure III</strong></see> <i>PvE</i> (WHM) [131] [Spell]
    /// <para>Restores own or target party member's HP and all party members nearby target. Cure Potency:</para>
    /// </summary>
    
    public IBaseAction CureIiiPvE => _CureIiiPvECreator.Value;
    private readonly Lazy<IBaseAction> _AeroIiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)132, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyAeroIiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/132"><strong>Aero II</strong></see> <i>PvE</i> (CNJ WHM) [132] [Spell]
    /// </summary>
    static partial void ModifyAeroIiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/132"><strong>Aero II</strong></see> <i>PvE</i> (CNJ WHM) [132] [Spell]
    /// <para>Deals wind damage with a potency of 50. Additional Effect: Wind damage over time Potency: 50 Duration: 30s</para>
    /// </summary>
    
    public IBaseAction AeroIiPvE => _AeroIiPvECreator.Value;
    private readonly Lazy<IBaseAction> _MedicaIiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)133, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyMedicaIiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/133"><strong>Medica II</strong></see> <i>PvE</i> (CNJ WHM) [133] [Spell]
    /// </summary>
    static partial void ModifyMedicaIiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/133"><strong>Medica II</strong></see> <i>PvE</i> (CNJ WHM) [133] [Spell]
    /// <para>Restores own HP and the HP of all nearby party members. Cure Potency: Additional Effect: Regen Cure Potency: Duration: 15s</para>
    /// </summary>
    
    public IBaseAction MedicaIiPvE => _MedicaIiPvECreator.Value;
    private readonly Lazy<IBaseAction> _CureIiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)135, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyCureIiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/135"><strong>Cure II</strong></see> <i>PvE</i> (CNJ WHM) [135] [Spell]
    /// </summary>
    static partial void ModifyCureIiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/135"><strong>Cure II</strong></see> <i>PvE</i> (CNJ WHM) [135] [Spell]
    /// <para>Restores target's HP. Cure Potency:</para>
    /// </summary>
    
    public IBaseAction CureIiPvE => _CureIiPvECreator.Value;
    private readonly Lazy<IBaseAction> _PresenceOfMindPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)136, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyPresenceOfMindPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/136"><strong>Presence of Mind</strong></see> <i>PvE</i> (WHM) [136] [Ability]
    /// </summary>
    static partial void ModifyPresenceOfMindPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/136"><strong>Presence of Mind</strong></see> <i>PvE</i> (WHM) [136] [Ability]
    /// <para>Reduces spell cast time and recast time and auto-attack delay by 20%. Duration: 15s</para>
    /// </summary>
    
    public IBaseAction PresenceOfMindPvE => _PresenceOfMindPvECreator.Value;
    private readonly Lazy<IBaseAction> _RegenPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)137, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRegenPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/137"><strong>Regen</strong></see> <i>PvE</i> (WHM) [137] [Spell]
    /// </summary>
    static partial void ModifyRegenPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/137"><strong>Regen</strong></see> <i>PvE</i> (WHM) [137] [Spell]
    /// <para>Grants healing over time effect to target. Cure Potency: Duration: 18s</para>
    /// </summary>
    
    public IBaseAction RegenPvE => _RegenPvECreator.Value;
    private readonly Lazy<IBaseAction> _HolyPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)139, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHolyPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/139"><strong>Holy</strong></see> <i>PvE</i> (WHM) [139] [Spell]
    /// </summary>
    static partial void ModifyHolyPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/139"><strong>Holy</strong></see> <i>PvE</i> (WHM) [139] [Spell]
    /// <para>Deals unaspected damage with a potency of 140 to all nearby enemies. Additional Effect: Stun Duration: 4s</para>
    /// </summary>
    
    public IBaseAction HolyPvE => _HolyPvECreator.Value;
    private readonly Lazy<IBaseAction> _BenedictionPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)140, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBenedictionPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/140"><strong>Benediction</strong></see> <i>PvE</i> (WHM) [140] [Ability]
    /// </summary>
    static partial void ModifyBenedictionPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/140"><strong>Benediction</strong></see> <i>PvE</i> (WHM) [140] [Ability]
    /// <para>Restores all of a target's HP.</para>
    /// </summary>
    
    public IBaseAction BenedictionPvE => _BenedictionPvECreator.Value;
    private readonly Lazy<IBaseAction> _StoneIiiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3568, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyStoneIiiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3568"><strong>Stone III</strong></see> <i>PvE</i> (WHM) [3568] [Spell]
    /// </summary>
    static partial void ModifyStoneIiiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3568"><strong>Stone III</strong></see> <i>PvE</i> (WHM) [3568] [Spell]
    /// <para>Deals earth damage with a potency of 220.</para>
    /// </summary>
    
    public IBaseAction StoneIiiPvE => _StoneIiiPvECreator.Value;
    private readonly Lazy<IBaseAction> _AsylumPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3569, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyAsylumPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3569"><strong>Asylum</strong></see> <i>PvE</i> (WHM) [3569] [Ability]
    /// </summary>
    static partial void ModifyAsylumPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3569"><strong>Asylum</strong></see> <i>PvE</i> (WHM) [3569] [Ability]
    /// <para>Envelops a designated area in a veil of succor granting healing over time to self and any party members who enter. Cure Potency: 100 Duration: 24s</para>
    /// </summary>
    
    public IBaseAction AsylumPvE => _AsylumPvECreator.Value;
    private readonly Lazy<IBaseAction> _TetragrammatonPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3570, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyTetragrammatonPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3570"><strong>Tetragrammaton</strong></see> <i>PvE</i> (WHM) [3570] [Ability]
    /// </summary>
    static partial void ModifyTetragrammatonPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3570"><strong>Tetragrammaton</strong></see> <i>PvE</i> (WHM) [3570] [Ability]
    /// <para>Restores target's HP. Cure Potency: 700</para>
    /// </summary>
    
    public IBaseAction TetragrammatonPvE => _TetragrammatonPvECreator.Value;
    private readonly Lazy<IBaseAction> _AssizePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3571, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyAssizePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3571"><strong>Assize</strong></see> <i>PvE</i> (WHM) [3571] [Ability]
    /// </summary>
    static partial void ModifyAssizePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3571"><strong>Assize</strong></see> <i>PvE</i> (WHM) [3571] [Ability]
    /// <para>Deals unaspected damage with a potency of 400 to all nearby enemies. Additional Effect: Restores own HP and the HP of nearby party members Cure Potency: 400 Additional Effect: Restores 5% of maximum MP</para>
    /// </summary>
    
    public IBaseAction AssizePvE => _AssizePvECreator.Value;
    private readonly Lazy<IBaseAction> _ThinAirPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7430, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyThinAirPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7430"><strong>Thin Air</strong></see> <i>PvE</i> (WHM) [7430] [Ability]
    /// </summary>
    static partial void ModifyThinAirPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7430"><strong>Thin Air</strong></see> <i>PvE</i> (WHM) [7430] [Ability]
    /// <para>Next action is executed without MP cost. Duration: 12s Maximum Charges: 2</para>
    /// </summary>
    
    public IBaseAction ThinAirPvE => _ThinAirPvECreator.Value;
    private readonly Lazy<IBaseAction> _StoneIvPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7431, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyStoneIvPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7431"><strong>Stone IV</strong></see> <i>PvE</i> (WHM) [7431] [Spell]
    /// </summary>
    static partial void ModifyStoneIvPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7431"><strong>Stone IV</strong></see> <i>PvE</i> (WHM) [7431] [Spell]
    /// <para>Deals earth damage with a potency of 260.</para>
    /// </summary>
    
    public IBaseAction StoneIvPvE => _StoneIvPvECreator.Value;
    private readonly Lazy<IBaseAction> _DivineBenisonPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7432, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyDivineBenisonPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7432"><strong>Divine Benison</strong></see> <i>PvE</i> (WHM) [7432] [Ability]
    /// </summary>
    static partial void ModifyDivineBenisonPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7432"><strong>Divine Benison</strong></see> <i>PvE</i> (WHM) [7432] [Ability]
    /// <para>Creates a barrier around self or target party member that absorbs damage equivalent to a heal of 500 potency. Duration: 15s</para>
    /// </summary>
    
    public IBaseAction DivineBenisonPvE => _DivineBenisonPvECreator.Value;
    private readonly Lazy<IBaseAction> _PlenaryIndulgencePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7433, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyPlenaryIndulgencePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7433"><strong>Plenary Indulgence</strong></see> <i>PvE</i> (WHM) [7433] [Ability]
    /// </summary>
    static partial void ModifyPlenaryIndulgencePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7433"><strong>Plenary Indulgence</strong></see> <i>PvE</i> (WHM) [7433] [Ability]
    /// <para>Grants Confession to self and nearby party members reducing damage taken by 10%. Duration: 10s Party members under the effect of Confession will receive additional healing upon receiving HP recovery via Medica . Cure Potency: 200</para>
    /// </summary>
    
    public IBaseAction PlenaryIndulgencePvE => _PlenaryIndulgencePvECreator.Value;
    private readonly Lazy<IBaseAction> _AfflatusSolacePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16531, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyAfflatusSolacePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16531"><strong>Afflatus Solace</strong></see> <i>PvE</i> (WHM) [16531] [Spell]
    /// </summary>
    static partial void ModifyAfflatusSolacePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16531"><strong>Afflatus Solace</strong></see> <i>PvE</i> (WHM) [16531] [Spell]
    /// <para>Restores target's HP. Cure Potency: Healing Gauge Cost: 1 Lily</para>
    /// </summary>
    
    public IBaseAction AfflatusSolacePvE => _AfflatusSolacePvECreator.Value;
    private readonly Lazy<IBaseAction> _DiaPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16532, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyDiaPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16532"><strong>Dia</strong></see> <i>PvE</i> (WHM) [16532] [Spell]
    /// </summary>
    static partial void ModifyDiaPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16532"><strong>Dia</strong></see> <i>PvE</i> (WHM) [16532] [Spell]
    /// <para>Deals unaspected damage with a potency of . Additional Effect: Unaspected damage over time Potency: Duration: 30s</para>
    /// </summary>
    
    public IBaseAction DiaPvE => _DiaPvECreator.Value;
    private readonly Lazy<IBaseAction> _GlarePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16533, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyGlarePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16533"><strong>Glare</strong></see> <i>PvE</i> (WHM) [16533] [Spell]
    /// </summary>
    static partial void ModifyGlarePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16533"><strong>Glare</strong></see> <i>PvE</i> (WHM) [16533] [Spell]
    /// <para>Deals unaspected damage with a potency of 290.</para>
    /// </summary>
    
    public IBaseAction GlarePvE => _GlarePvECreator.Value;
    private readonly Lazy<IBaseAction> _AfflatusRapturePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16534, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyAfflatusRapturePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16534"><strong>Afflatus Rapture</strong></see> <i>PvE</i> (WHM) [16534] [Spell]
    /// </summary>
    static partial void ModifyAfflatusRapturePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16534"><strong>Afflatus Rapture</strong></see> <i>PvE</i> (WHM) [16534] [Spell]
    /// <para>Restores own HP and the HP of all nearby party members. Cure Potency: Additional Effect: Nourishes the Blood Lily Healing Gauge Cost: 1 Lily</para>
    /// </summary>
    
    public IBaseAction AfflatusRapturePvE => _AfflatusRapturePvECreator.Value;
    private readonly Lazy<IBaseAction> _AfflatusMiseryPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16535, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyAfflatusMiseryPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16535"><strong>Afflatus Misery</strong></see> <i>PvE</i> (WHM) [16535] [Spell]
    /// </summary>
    static partial void ModifyAfflatusMiseryPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16535"><strong>Afflatus Misery</strong></see> <i>PvE</i> (WHM) [16535] [Spell]
    /// <para>Deals unaspected damage to target and all enemies nearby it with a potency of for the first enemy and 50% less for all remaining enemies. Can only be executed when the Blood Lily is in full bloom.</para>
    /// </summary>
    
    public IBaseAction AfflatusMiseryPvE => _AfflatusMiseryPvECreator.Value;
    private readonly Lazy<IBaseAction> _TemperancePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16536, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyTemperancePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16536"><strong>Temperance</strong></see> <i>PvE</i> (WHM) [16536] [Ability]
    /// </summary>
    static partial void ModifyTemperancePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16536"><strong>Temperance</strong></see> <i>PvE</i> (WHM) [16536] [Ability]
    /// <para>Increases healing magic potency by 20% while reducing damage taken by self and all party members within a radius of 50 yalms by 10%. Duration: 20s</para>
    /// </summary>
    
    public IBaseAction TemperancePvE => _TemperancePvECreator.Value;
    private readonly Lazy<IBaseAction> _GlareIiiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25859, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyGlareIiiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25859"><strong>Glare III</strong></see> <i>PvE</i> (WHM) [25859] [Spell]
    /// </summary>
    static partial void ModifyGlareIiiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25859"><strong>Glare III</strong></see> <i>PvE</i> (WHM) [25859] [Spell]
    /// <para>Deals unaspected damage with a potency of .</para>
    /// </summary>
    
    public IBaseAction GlareIiiPvE => _GlareIiiPvECreator.Value;
    private readonly Lazy<IBaseAction> _HolyIiiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25860, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHolyIiiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25860"><strong>Holy III</strong></see> <i>PvE</i> (WHM) [25860] [Spell]
    /// </summary>
    static partial void ModifyHolyIiiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25860"><strong>Holy III</strong></see> <i>PvE</i> (WHM) [25860] [Spell]
    /// <para>Deals unaspected damage with a potency of 150 to all nearby enemies. Additional Effect: Stun Duration: 4s</para>
    /// </summary>
    
    public IBaseAction HolyIiiPvE => _HolyIiiPvECreator.Value;
    private readonly Lazy<IBaseAction> _AquaveilPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25861, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyAquaveilPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25861"><strong>Aquaveil</strong></see> <i>PvE</i> (WHM) [25861] [Ability]
    /// </summary>
    static partial void ModifyAquaveilPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25861"><strong>Aquaveil</strong></see> <i>PvE</i> (WHM) [25861] [Ability]
    /// <para>Reduces damage taken by a party member or self by 15%. Duration: 8s</para>
    /// </summary>
    
    public IBaseAction AquaveilPvE => _AquaveilPvECreator.Value;
    private readonly Lazy<IBaseAction> _LiturgyOfTheBellPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25862, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyLiturgyOfTheBellPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25862"><strong>Liturgy of the Bell</strong></see> <i>PvE</i> (WHM) [25862] [Ability]
    /// </summary>
    static partial void ModifyLiturgyOfTheBellPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25862"><strong>Liturgy of the Bell</strong></see> <i>PvE</i> (WHM) [25862] [Ability]
    /// <para>Places a healing blossom at the designated location and grants 5 stacks of Liturgy of the Bell to self. Duration: 20s Taking damage will expend 1 stack of Liturgy of the Bell to heal self and all party members within a radius of 20 yalms. Cure Potency: 400 The effect of this action can only be triggered once per second. Any remaining stacks of Liturgy of the Bell will trigger an additional healing effect when time expires or upon executing this action a second time. Cure Potency: 200 for every remaining stack of Liturgy of the Bell This action does not share a recast timer with any other actions.</para>
    /// </summary>
    
    public IBaseAction LiturgyOfTheBellPvE => _LiturgyOfTheBellPvECreator.Value;
    private readonly Lazy<IBaseAction> _LiturgyOfTheBellPvE_28509Creator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)28509, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyLiturgyOfTheBellPvE_28509(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/28509"><strong>Liturgy of the Bell</strong></see> <i>PvE</i> (WHM) [28509] [Ability]
    /// </summary>
    static partial void ModifyLiturgyOfTheBellPvE_28509(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/28509"><strong>Liturgy of the Bell</strong></see> <i>PvE</i> (WHM) [28509] [Ability]
    /// <para>Places a healing blossom at the designated location and grants 5 stacks of Liturgy of the Bell to self. Duration: 20s Taking damage will expend 1 stack of Liturgy of the Bell to heal self and all party members within a radius of 20 yalms. Cure Potency: 400 The effect of this action can only be triggered once per second. Any remaining stacks of Liturgy of the Bell will trigger an additional healing effect when time expires or upon executing this action a second time. Cure Potency: 200 for every remaining stack of Liturgy of the Bell This action does not share a recast timer with any other actions.</para>
    /// </summary>
    
    public IBaseAction LiturgyOfTheBellPvE_28509 => _LiturgyOfTheBellPvE_28509Creator.Value;
    private readonly Lazy<IBaseAction> _GlareIiiPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29223, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyGlareIiiPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29223"><strong>Glare III</strong></see> <i>PvP</i> (WHM) [29223] [Spell]
    /// </summary>
    static partial void ModifyGlareIiiPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29223"><strong>Glare III</strong></see> <i>PvP</i> (WHM) [29223] [Spell]
    /// <para>Deals unaspected damage with a potency of 6,000. ※Action changes to Glare IV when under the effect of Sacred Sight.</para>
    /// </summary>
    
    public IBaseAction GlareIiiPvP => _GlareIiiPvPCreator.Value;
    private readonly Lazy<IBaseAction> _CureIiPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29224, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyCureIiPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29224"><strong>Cure II</strong></see> <i>PvP</i> (WHM) [29224] [Spell]
    /// </summary>
    static partial void ModifyCureIiPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29224"><strong>Cure II</strong></see> <i>PvP</i> (WHM) [29224] [Spell]
    /// <para>Restores target's HP. Cure Potency: 12,000 Maximum Charges: 2 This action does not share a recast timer with any other actions. ※Action changes to Cure III when under the effect of Cure III Ready.</para>
    /// </summary>
    
    public IBaseAction CureIiPvP => _CureIiPvPCreator.Value;
    private readonly Lazy<IBaseAction> _CureIiiPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29225, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyCureIiiPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29225"><strong>Cure III</strong></see> <i>PvP</i> (WHM) [29225] [Spell]
    /// </summary>
    static partial void ModifyCureIiiPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29225"><strong>Cure III</strong></see> <i>PvP</i> (WHM) [29225] [Spell]
    /// <para>Restores own or target party member's HP and all party members nearby target. Cure Potency: 16,000 Can only be executed while under the effect of Cure III Ready. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction CureIiiPvP => _CureIiiPvPCreator.Value;
    private readonly Lazy<IBaseAction> _AfflatusMiseryPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29226, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyAfflatusMiseryPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29226"><strong>Afflatus Misery</strong></see> <i>PvP</i> (WHM) [29226] [Spell]
    /// </summary>
    static partial void ModifyAfflatusMiseryPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29226"><strong>Afflatus Misery</strong></see> <i>PvP</i> (WHM) [29226] [Spell]
    /// <para>Deals unaspected damage with a potency of 12,000 to target and all enemies nearby it. This action does not share a recast timer with any other actions.</para>
    /// </summary>
    
    public IBaseAction AfflatusMiseryPvP => _AfflatusMiseryPvPCreator.Value;
    private readonly Lazy<IBaseAction> _AquaveilPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29227, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyAquaveilPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29227"><strong>Aquaveil</strong></see> <i>PvP</i> (WHM) [29227] [Ability]
    /// </summary>
    static partial void ModifyAquaveilPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29227"><strong>Aquaveil</strong></see> <i>PvP</i> (WHM) [29227] [Ability]
    /// <para>Creates a barrier around self or target party member that absorbs damage equivalent to a heal of 10,000 potency. Duration: 10s Additional Effect: Nullifies one status affliction that can be removed by Purify Barrier potency is doubled when successfully nullifying a status affliction.</para>
    /// </summary>
    
    public IBaseAction AquaveilPvP => _AquaveilPvPCreator.Value;
    private readonly Lazy<IBaseAction> _MiracleOfNaturePvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29228, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyMiracleOfNaturePvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29228"><strong>Miracle of Nature</strong></see> <i>PvP</i> (WHM) [29228] [Ability]
    /// </summary>
    static partial void ModifyMiracleOfNaturePvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29228"><strong>Miracle of Nature</strong></see> <i>PvP</i> (WHM) [29228] [Ability]
    /// <para>Forcibly transforms target into a diminutive creature preventing them from using actions other than Purify. Duration: 2s Additional Effect: Increases target's damage taken by 20% Has no effect on players under an effect that nullifies status afflictions that can be removed by Purify Relentless Rush Honing Dance players riding machina or non-player combatants.</para>
    /// </summary>
    
    public IBaseAction MiracleOfNaturePvP => _MiracleOfNaturePvPCreator.Value;
    private readonly Lazy<IBaseAction> _SeraphStrikePvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29229, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifySeraphStrikePvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29229"><strong>Seraph Strike</strong></see> <i>PvP</i> (WHM) [29229] [Ability]
    /// </summary>
    static partial void ModifySeraphStrikePvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29229"><strong>Seraph Strike</strong></see> <i>PvP</i> (WHM) [29229] [Ability]
    /// <para>Delivers a jumping attack that deals unaspected damage to target and all enemies nearby it with a potency of 6,000. Additional Effect: Grants Protect to self and nearby party members reducing damage taken by 10% Duration: 10s Additional Effect: Grants 3 stacks of Sacred Sight Duration: 20s Additional Effect: Grants Cure III Ready Duration: 20s Cannot be executed while bound. ※Glare III changes to Glare IV while under the effect of Sacred Sight. ※Cure II changes to Cure III while under the effect of Cure III Ready.</para>
    /// </summary>
    
    public IBaseAction SeraphStrikePvP => _SeraphStrikePvPCreator.Value;
    private readonly Lazy<IBaseAction> _AetherialShiftPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)37008, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyAetherialShiftPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/37008"><strong>Aetherial Shift</strong></see> <i>PvE</i> (WHM) [37008] [Ability]
    /// </summary>
    static partial void ModifyAetherialShiftPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/37008"><strong>Aetherial Shift</strong></see> <i>PvE</i> (WHM) [37008] [Ability]
    /// <para>Quickly dash 15 yalms forward. Cannot be executed while bound.</para>
    /// </summary>
    
    public IBaseAction AetherialShiftPvE => _AetherialShiftPvECreator.Value;
    private readonly Lazy<IBaseAction> _GlareIvPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)37009, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyGlareIvPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/37009"><strong>Glare IV</strong></see> <i>PvE</i> (WHM) [37009] [Spell]
    /// </summary>
    static partial void ModifyGlareIvPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/37009"><strong>Glare IV</strong></see> <i>PvE</i> (WHM) [37009] [Spell]
    /// <para>Deals unaspected damage to target and all enemies nearby it with a potency of 640 for the first enemy and 40% less for all remaining enemies. Can only be executed while Sacred Sight is active.</para>
    /// </summary>
    
    public IBaseAction GlareIvPvE => _GlareIvPvECreator.Value;
    private readonly Lazy<IBaseAction> _MedicaIiiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)37010, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyMedicaIiiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/37010"><strong>Medica III</strong></see> <i>PvE</i> (WHM) [37010] [Spell]
    /// </summary>
    static partial void ModifyMedicaIiiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/37010"><strong>Medica III</strong></see> <i>PvE</i> (WHM) [37010] [Spell]
    /// <para>Restores own HP and the HP of all nearby party members. Cure Potency: 250 Additional Effect: Regen Cure Potency: 175 Duration: 15s</para>
    /// </summary>
    
    public IBaseAction MedicaIiiPvE => _MedicaIiiPvECreator.Value;
    private readonly Lazy<IBaseAction> _DivineCaressPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)37011, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyDivineCaressPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/37011"><strong>Divine Caress</strong></see> <i>PvE</i> (WHM) [37011] [Ability]
    /// </summary>
    static partial void ModifyDivineCaressPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/37011"><strong>Divine Caress</strong></see> <i>PvE</i> (WHM) [37011] [Ability]
    /// <para>Creates a barrier around self and all party members near you that absorbs damage equivalent to a heal of 400 potency. Duration: 10s Additional Effect: Grants Divine Aura when barrier effect expires Divine Aura Effect: Healing over time Cure Potency: 200 Duration: 15s Can only be executed while Divine Grace is active.</para>
    /// </summary>
    
    public IBaseAction DivineCaressPvE => _DivineCaressPvECreator.Value;
    private readonly Lazy<IBaseAction> _GlareIvPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41499, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyGlareIvPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41499"><strong>Glare IV</strong></see> <i>PvP</i> (WHM) [41499] [Spell]
    /// </summary>
    static partial void ModifyGlareIvPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41499"><strong>Glare IV</strong></see> <i>PvP</i> (WHM) [41499] [Spell]
    /// <para>Deals unaspected damage with a potency of 9,000 to target and all enemies nearby it. Can only be executed while under the effect of Sacred Sight. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction GlareIvPvP => _GlareIvPvPCreator.Value;

    private IBaseAction[] _AllBaseActions = null;
    
    /// <inheritdoc/>
    public override IBaseAction[] AllBaseActions => _AllBaseActions ??= [
    	StonePvE, CurePvE, AeroPvE, MedicaPvE, RaisePvE, StoneIiPvE, CureIiiPvE, AeroIiPvE, MedicaIiPvE, CureIiPvE, PresenceOfMindPvE, RegenPvE, HolyPvE, BenedictionPvE, StoneIiiPvE, AsylumPvE, TetragrammatonPvE, AssizePvE, ThinAirPvE, StoneIvPvE, DivineBenisonPvE, PlenaryIndulgencePvE, AfflatusSolacePvE, DiaPvE, GlarePvE, AfflatusRapturePvE, AfflatusMiseryPvE, TemperancePvE, GlareIiiPvE, HolyIiiPvE, AquaveilPvE, LiturgyOfTheBellPvE, GlareIiiPvP, CureIiPvP, CureIiiPvP, AfflatusMiseryPvP, AquaveilPvP, MiracleOfNaturePvP, SeraphStrikePvP, AetherialShiftPvE, GlareIvPvE, MedicaIiiPvE, DivineCaressPvE, GlareIvPvP,
    	..base.AllBaseActions,
    ];

private readonly Lazy<IBaseAction> _HealingWindPvECreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)206, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifyHealingWindPvE(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/206"><strong>Healing Wind</strong></see> <i>PvE</i> (All Classes) [206] [Limit Break]
/// </summary>
static partial void ModifyHealingWindPvE(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/206"><strong>Healing Wind</strong></see> <i>PvE</i> (All Classes) [206] [Limit Break]
/// <para>Restores 25% of own HP and the HP of all nearby party members.</para>
/// </summary>

public IBaseAction HealingWindPvE => _HealingWindPvECreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/206"><strong>Healing Wind</strong></see> <i>PvE</i> (All Classes) [206] [Limit Break]
/// <para>Restores 25% of own HP and the HP of all nearby party members.</para>
/// </summary>
private sealed protected override IBaseAction LimitBreak1 => HealingWindPvE;
private readonly Lazy<IBaseAction> _BreathOfTheEarthPvECreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)207, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifyBreathOfTheEarthPvE(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/207"><strong>Breath of the Earth</strong></see> <i>PvE</i> (All Classes) [207] [Limit Break]
/// </summary>
static partial void ModifyBreathOfTheEarthPvE(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/207"><strong>Breath of the Earth</strong></see> <i>PvE</i> (All Classes) [207] [Limit Break]
/// <para>Restores 60% of own HP and the HP of all nearby party members.</para>
/// </summary>

public IBaseAction BreathOfTheEarthPvE => _BreathOfTheEarthPvECreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/207"><strong>Breath of the Earth</strong></see> <i>PvE</i> (All Classes) [207] [Limit Break]
/// <para>Restores 60% of own HP and the HP of all nearby party members.</para>
/// </summary>
private sealed protected override IBaseAction LimitBreak2 => BreathOfTheEarthPvE;
private readonly Lazy<IBaseAction> _PulseOfLifePvECreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)208, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifyPulseOfLifePvE(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/208"><strong>Pulse of Life</strong></see> <i>PvE</i> (All Classes) [208] [Limit Break]
/// </summary>
static partial void ModifyPulseOfLifePvE(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/208"><strong>Pulse of Life</strong></see> <i>PvE</i> (All Classes) [208] [Limit Break]
/// <para>Restores 100% of own HP and the HP of all nearby party members including ones KO'd.</para>
/// </summary>

public IBaseAction PulseOfLifePvE => _PulseOfLifePvECreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/208"><strong>Pulse of Life</strong></see> <i>PvE</i> (All Classes) [208] [Limit Break]
/// <para>Restores 100% of own HP and the HP of all nearby party members including ones KO'd.</para>
/// </summary>
private sealed protected override IBaseAction LimitBreak3 => PulseOfLifePvE;
private readonly Lazy<IBaseAction> _AfflatusPurgationPvPCreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)29230, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifyAfflatusPurgationPvP(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/29230"><strong>Afflatus Purgation</strong></see> <i>PvP</i> (WHM) [29230] [Limit Break]
/// </summary>
static partial void ModifyAfflatusPurgationPvP(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/29230"><strong>Afflatus Purgation</strong></see> <i>PvP</i> (WHM) [29230] [Limit Break]
/// <para>Deals unaspected damage with a potency of 18,000 to all enemies in a straight line before you. Additional Effect: Stun Duration: 2s Additional Effect: Grants Temperance Temperance Effect: Grants Regen to self and nearby party members within 30 yalms Cure Potency: 4,000 Duration: 15s Can only be executed when the limit gauge is full. Gauge Charge Time: 60s This action's effects extend through obstructions.</para>
/// </summary>

private IBaseAction AfflatusPurgationPvP => _AfflatusPurgationPvPCreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/29230"><strong>Afflatus Purgation</strong></see> <i>PvP</i> (WHM) [29230] [Limit Break]
/// <para>Deals unaspected damage with a potency of 18,000 to all enemies in a straight line before you. Additional Effect: Stun Duration: 2s Additional Effect: Grants Temperance Temperance Effect: Grants Regen to self and nearby party members within 30 yalms Cure Potency: 4,000 Duration: 15s Can only be executed when the limit gauge is full. Gauge Charge Time: 60s This action's effects extend through obstructions.</para>
/// </summary>
private sealed protected override IBaseAction LimitBreakPvP => AfflatusPurgationPvP;

#endregion

#region Traits

    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/50181"><strong>Stone Mastery II) | Stone III |
| Stone III | Ausbau (Stone Mastery III) | Stone IV |
| Stone IV | Ausbau (Stone Mastery IV) | Glare |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Aero II | Regel prüft | Aero |
| Aero II | Regel prüft | Dia |
| Aero | Regel prüft | Aero II |
| Afflatus Rapture | Regel prüft | Afflatus Misery |
| Afflatus Solace | Regel prüft | Afflatus Misery |
| Aquaveil | Regel sperrt vorher | Divine Benison |
| Asylum | Regel sperrt vorher | Benediction |
| Dia | Regel prüft | Aero |
| Divine Benison | Regel sperrt vorher | Aquaveil |
| Divine Benison | Regel sperrt vorher | Benediction |
| Divine Benison | Regel sperrt vorher | Stone |
| Divine Caress | Regel sperrt vorher | Liturgy of the Bell |
| Divine Caress | Regel sperrt vorher | Temperance |
| Divine Caress | StatusNeed DivineGrace | Temperance |
| Glare | Regel prüft | Glare III |
| Holy III | Regel prüft | Holy |
| Holy | Regel prüft | Holy III |
| Liturgy of the Bell | Regel sperrt vorher | Temperance |
| Medica II | Regel prüft | Medica III |
| Medica III | Regel prüft | Medica II |
| Plenary Indulgence | Regel prüft | Afflatus Rapture |
| Plenary Indulgence | Regel prüft | Cure III |
| Plenary Indulgence | Regel sperrt vorher | Liturgy of the Bell |
| Plenary Indulgence | Regel prüft | Medica II |
| Plenary Indulgence | Regel prüft | Medica |
| Plenary Indulgence | Regel sperrt vorher | Presence of Mind |
| Plenary Indulgence | Regel sperrt vorher | Temperance |
| Regen | Regel sperrt vorher | Stone |
| Stone II | Regel prüft | Stone III |
| Stone III | Regel prüft | Stone IV |
| Stone IV | Regel prüft | Glare |
| Stone | Regel prüft | Stone II |
| Temperance | Regel sperrt vorher | Liturgy of the Bell |
| Tetragrammaton | Regel sperrt vorher | Benediction |
| Thin Air | Regel prüft | Raise |

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `BreathOfTheEarthPvE`, `HealingWindPvE`, `PulseOfLifePvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Repose (`ReposePvE`, Spell): ungenutzt
- Rescue (`RescuePvE`, Ability): ungenutzt

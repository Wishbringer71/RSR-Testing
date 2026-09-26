# RDM — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `RedMageRotation`
- Rotation: `RotationSolver/RebornRotations/Magical/RDM_Reborn.cs`
- Matrix als Tabelle: `RDM.csv`

## Nutzung

direkt: 41 · nur gelesen: 5 · ungenutzt: 1 · über anderen Knopf: 2

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Magier | Addle (`AddlePvE`) | 7560 | Ability | direkt |
| Magier | Lucid Dreaming (`LucidDreamingPvE`) | 7562 | Ability | direkt |
| Magier | Sleep (`SleepPvE`) | 25880 | Spell | ungenutzt |
| Magier | Surecast (`SurecastPvE`) | 7559 | Ability | direkt |
| Magier | Swiftcast (`SwiftcastPvE`) | 7561 | Ability | direkt |
| Job | Acceleration (`AccelerationPvE`) | 7518 | Ability | direkt |
| Job | Contre Sixte (`ContreSixtePvE`) | 7519 | Ability | direkt |
| Job | Corps-a-corps (`CorpsacorpsPvE`) | 7506 | Ability | direkt |
| Job | Displacement (`DisplacementPvE`) | 7515 | Ability | direkt |
| Job | Embolden (`EmboldenPvE`) | 7520 | Ability | direkt |
| Job | Enchanted Moulinet (`EnchantedMoulinetPvE`) | 7530 | Weaponskill | direkt |
| Job | Enchanted Moulinet Deux (`EnchantedMoulinetDeuxPvE`) | 37002 | Weaponskill | direkt |
| Job | Enchanted Moulinet Trois (`EnchantedMoulinetTroisPvE`) | 37003 | Weaponskill | direkt |
| Job | Enchanted Redoublement (`EnchantedRedoublementPvE`) | 7529 | Weaponskill | direkt |
| Job | Enchanted Reprise (`EnchantedReprisePvE`) | 16528 | Weaponskill | direkt |
| Job | Enchanted Riposte (`EnchantedRipostePvE`) | 7527 | Weaponskill | direkt |
| Job | Enchanted Zwerchhau (`EnchantedZwerchhauPvE`) | 7528 | Weaponskill | direkt |
| Job | Engagement (`EngagementPvE`) | 16527 | Ability | direkt |
| Job | Fleche (`FlechePvE`) | 7517 | Ability | direkt |
| Job | Grand Impact (`GrandImpactPvE`) | 37006 | Spell | direkt |
| Job | Impact (`ImpactPvE`) | 16526 | Spell | direkt |
| Job | Jolt (`JoltPvE`) | 7503 | Spell | direkt |
| Job | Jolt II (`JoltIiPvE`) | 7524 | Spell | über Jolt |
| Job | Jolt III (`JoltIiiPvE`) | 37004 | Spell | über Jolt II |
| Job | Magick Barrier (`MagickBarrierPvE`) | 25857 | Ability | direkt |
| Job | Manafication (`ManaficationPvE`) | 7521 | Ability | direkt |
| Job | Moulinet (`MoulinetPvE`) | 7513 | Weaponskill | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Prefulgence (`PrefulgencePvE`) | 37007 | Ability | direkt |
| Job | Redoublement (`RedoublementPvE`) | 7516 | Weaponskill | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Reprise (`ReprisePvE`) | 16529 | Weaponskill | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Resolution (`ResolutionPvE`) | 25858 | Spell | direkt |
| Job | Riposte (`RipostePvE`) | 7504 | Weaponskill | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Scatter (`ScatterPvE`) | 7509 | Spell | direkt |
| Job | Scorch (`ScorchPvE`) | 16530 | Spell | direkt |
| Job | Veraero (`VeraeroPvE`) | 7507 | Spell | direkt |
| Job | Veraero II (`VeraeroIiPvE`) | 16525 | Spell | direkt |
| Job | Veraero III (`VeraeroIiiPvE`) | 25856 | Spell | direkt |
| Job | Vercure (`VercurePvE`) | 7514 | Spell | direkt |
| Job | Verfire (`VerfirePvE`) | 7510 | Spell | direkt |
| Job | Verflare (`VerflarePvE`) | 7525 | Spell | direkt |
| Job | Verholy (`VerholyPvE`) | 7526 | Spell | direkt |
| Job | Verraise (`VerraisePvE`) | 7523 | Spell | direkt |
| Job | Verstone (`VerstonePvE`) | 7511 | Spell | direkt |
| Job | Verthunder (`VerthunderPvE`) | 7505 | Spell | direkt |
| Job | Verthunder II (`VerthunderIiPvE`) | 16524 | Spell | direkt |
| Job | Verthunder III (`VerthunderIiiPvE`) | 25855 | Spell | direkt |
| Job | Vice of Thorns (`ViceOfThornsPvE`) | 37005 | Ability | direkt |
| Job | Zwerchhau (`ZwerchhauPvE`) | 7512 | Weaponskill | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Displacement | gemeinsame Abklingzeit | Engagement |
| Enchanted Moulinet Deux | kostet | Black Mana Balance |
| Enchanted Moulinet Deux | kostet | Enchanted Moulinet Balance |
| Enchanted Moulinet Deux | Combo nach | Enchanted Moulinet |
| Enchanted Moulinet Deux | Knopf wird zu | Enchanted Moulinet Trois |
| Enchanted Moulinet | kostet | Balance |
| Enchanted Moulinet | kostet | Black Mana Balance |
| Enchanted Moulinet | Knopf wird zu | Enchanted Moulinet Deux |
| Enchanted Moulinet Trois | kostet | Black Mana Balance |
| Enchanted Moulinet Trois | kostet | Enchanted Moulinet Deux Balance |
| Enchanted Moulinet Trois | Combo nach | Enchanted Moulinet Deux |
| Enchanted Redoublement | kostet | Balance |
| Enchanted Redoublement | kostet | Black Mana Balance |
| Enchanted Redoublement | Combo nach | Enchanted Zwerchhau |
| Enchanted Reprise | kostet | Balance |
| Enchanted Reprise | kostet | Black Mana Balance |
| Enchanted Riposte | kostet | Balance |
| Enchanted Riposte | kostet | Black Mana Balance |
| Enchanted Zwerchhau | kostet | Balance |
| Enchanted Zwerchhau | kostet | Black Mana Balance |
| Enchanted Zwerchhau | Combo nach | Enchanted Riposte |
| Enchanted Zwerchhau | Combo nach | Riposte |
| Engagement | gemeinsame Abklingzeit | Displacement |
| Grand Impact | braucht Grand Impact Ready | Eigenschaft Enhanced Acceleration II |
| Impact | Knopf wird zu | Grand Impact |
| Impact | Knopf wird zu | Scorch |
| Jolt II | Ausbau (Red Magic Mastery III) | Jolt III |
| Jolt III | Knopf wird zu | Grand Impact |
| Jolt | Ausbau (Jolt</strong></see> <i>PvE</i> (RDM) [7503] [Spell]
    /// </summary>
    static partial void ModifyJoltPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7503"><strong>Jolt</strong></see> <i>PvE</i> (RDM) [7503] [Spell]
    /// <para>Deals unaspected damage with a potency of 170. Additional Effect: Increases both Black Mana and White Mana by 2</para>
    /// </summary>
    
    public IBaseAction JoltPvE => _JoltPvECreator.Value;
    private readonly Lazy<IBaseAction> _RipostePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7504, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRipostePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7504"><strong>Riposte</strong></see> <i>PvE</i> (RDM) [7504] [Weaponskill]
    /// </summary>
    static partial void ModifyRipostePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7504"><strong>Riposte</strong></see> <i>PvE</i> (RDM) [7504] [Weaponskill]
    /// <para>Delivers an attack with a potency of 130.</para>
    /// </summary>
    
    public IBaseAction RipostePvE => _RipostePvECreator.Value;
    private readonly Lazy<IBaseAction> _VerthunderPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7505, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyVerthunderPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7505"><strong>Verthunder</strong></see> <i>PvE</i> (RDM) [7505] [Spell]
    /// </summary>
    static partial void ModifyVerthunderPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7505"><strong>Verthunder</strong></see> <i>PvE</i> (RDM) [7505] [Spell]
    /// <para>Deals lightning damage with a potency of . Additional Effect: Increases Black Mana by 6</para>
    /// </summary>
    
    public IBaseAction VerthunderPvE => _VerthunderPvECreator.Value;
    private readonly Lazy<IBaseAction> _CorpsacorpsPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7506, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyCorpsacorpsPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7506"><strong>Corps-a-corps</strong></see> <i>PvE</i> (RDM) [7506] [Ability]
    /// </summary>
    static partial void ModifyCorpsacorpsPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7506"><strong>Corps-a-corps</strong></see> <i>PvE</i> (RDM) [7506] [Ability]
    /// <para>Rushes target and delivers an attack with a potency of 130. Maximum Charges: 2 Cannot be executed while bound.</para>
    /// </summary>
    
    public IBaseAction CorpsacorpsPvE => _CorpsacorpsPvECreator.Value;
    private readonly Lazy<IBaseAction> _VeraeroPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7507, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyVeraeroPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7507"><strong>Veraero</strong></see> <i>PvE</i> (RDM) [7507] [Spell]
    /// </summary>
    static partial void ModifyVeraeroPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7507"><strong>Veraero</strong></see> <i>PvE</i> (RDM) [7507] [Spell]
    /// <para>Deals wind damage with a potency of . Additional Effect: Increases White Mana by 6</para>
    /// </summary>
    
    public IBaseAction VeraeroPvE => _VeraeroPvECreator.Value;
    private readonly Lazy<IBaseAction> _ScatterPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7509, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyScatterPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7509"><strong>Scatter</strong></see> <i>PvE</i> (RDM) [7509] [Spell]
    /// </summary>
    static partial void ModifyScatterPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7509"><strong>Scatter</strong></see> <i>PvE</i> (RDM) [7509] [Spell]
    /// <para>Deals unaspected damage with a potency of 120 to target and all enemies nearby it. Additional Effect: Increases both Black Mana and White Mana by 3</para>
    /// </summary>
    
    public IBaseAction ScatterPvE => _ScatterPvECreator.Value;
    private readonly Lazy<IBaseAction> _VerfirePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7510, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyVerfirePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7510"><strong>Verfire</strong></see> <i>PvE</i> (RDM) [7510] [Spell]
    /// </summary>
    static partial void ModifyVerfirePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7510"><strong>Verfire</strong></see> <i>PvE</i> (RDM) [7510] [Spell]
    /// <para>Deals fire damage with a potency of . Additional Effect: Increases Black Mana by 5 Can only be executed while Verfire Ready is active.</para>
    /// </summary>
    
    public IBaseAction VerfirePvE => _VerfirePvECreator.Value;
    private readonly Lazy<IBaseAction> _VerstonePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7511, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyVerstonePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7511"><strong>Verstone</strong></see> <i>PvE</i> (RDM) [7511] [Spell]
    /// </summary>
    static partial void ModifyVerstonePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7511"><strong>Verstone</strong></see> <i>PvE</i> (RDM) [7511] [Spell]
    /// <para>Deals earth damage with a potency of . Additional Effect: Increases White Mana by 5 Can only be executed while Verstone Ready is active.</para>
    /// </summary>
    
    public IBaseAction VerstonePvE => _VerstonePvECreator.Value;
    private readonly Lazy<IBaseAction> _ZwerchhauPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7512, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyZwerchhauPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7512"><strong>Zwerchhau</strong></see> <i>PvE</i> (RDM) [7512] [Weaponskill]
    /// </summary>
    static partial void ModifyZwerchhauPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7512"><strong>Zwerchhau</strong></see> <i>PvE</i> (RDM) [7512] [Weaponskill]
    /// <para>Delivers an attack with a potency of 100. Combo Action: Riposte or Enchanted Riposte Combo Potency: 150 Action upgraded to Enchanted Zwerchhau if both Black Mana and White Mana are at 15 or more.</para>
    /// </summary>
    
    public IBaseAction ZwerchhauPvE => _ZwerchhauPvECreator.Value;
    private readonly Lazy<IBaseAction> _MoulinetPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7513, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyMoulinetPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7513"><strong>Moulinet</strong></see> <i>PvE</i> (RDM) [7513] [Weaponskill]
    /// </summary>
    static partial void ModifyMoulinetPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7513"><strong>Moulinet</strong></see> <i>PvE</i> (RDM) [7513] [Weaponskill]
    /// <para>Delivers an attack with a potency of 60 to all enemies in a cone before you. Action upgraded to Enchanted Moulinet if both Black Mana and White Mana are at 20 or more.</para>
    /// </summary>
    
    public IBaseAction MoulinetPvE => _MoulinetPvECreator.Value;
    private readonly Lazy<IBaseAction> _VercurePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7514, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyVercurePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7514"><strong>Vercure</strong></see> <i>PvE</i> (RDM) [7514] [Spell]
    /// </summary>
    static partial void ModifyVercurePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7514"><strong>Vercure</strong></see> <i>PvE</i> (RDM) [7514] [Spell]
    /// <para>Restores target's HP. Cure Potency: 350</para>
    /// </summary>
    
    public IBaseAction VercurePvE => _VercurePvECreator.Value;
    private readonly Lazy<IBaseAction> _DisplacementPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7515, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyDisplacementPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7515"><strong>Displacement</strong></see> <i>PvE</i> (RDM) [7515] [Ability]
    /// </summary>
    static partial void ModifyDisplacementPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7515"><strong>Displacement</strong></see> <i>PvE</i> (RDM) [7515] [Ability]
    /// <para>Delivers an attack with a potency of . Additional Effect: 15-yalm backstep Maximum Charges: 2 Cannot be executed while bound. Shares a recast timer with Engagement.</para>
    /// </summary>
    
    public IBaseAction DisplacementPvE => _DisplacementPvECreator.Value;
    private readonly Lazy<IBaseAction> _RedoublementPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7516, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRedoublementPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7516"><strong>Redoublement</strong></see> <i>PvE</i> (RDM) [7516] [Weaponskill]
    /// </summary>
    static partial void ModifyRedoublementPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7516"><strong>Redoublement</strong></see> <i>PvE</i> (RDM) [7516] [Weaponskill]
    /// <para>Delivers an attack with a potency of 100. Combo Action: Zwerchhau or Enchanted Zwerchhau Combo Potency: 230 Action upgraded to Enchanted Redoublement if both Black Mana and White Mana are at 15 or more.</para>
    /// </summary>
    
    public IBaseAction RedoublementPvE => _RedoublementPvECreator.Value;
    private readonly Lazy<IBaseAction> _FlechePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7517, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyFlechePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7517"><strong>Fleche</strong></see> <i>PvE</i> (RDM) [7517] [Ability]
    /// </summary>
    static partial void ModifyFlechePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7517"><strong>Fleche</strong></see> <i>PvE</i> (RDM) [7517] [Ability]
    /// <para>Delivers an attack with a potency of .</para>
    /// </summary>
    
    public IBaseAction FlechePvE => _FlechePvECreator.Value;
    private readonly Lazy<IBaseAction> _AccelerationPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7518, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyAccelerationPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7518"><strong>Acceleration</strong></see> <i>PvE</i> (RDM) [7518] [Ability]
    /// </summary>
    static partial void ModifyAccelerationPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7518"><strong>Acceleration</strong></see> <i>PvE</i> (RDM) [7518] [Ability]
    /// <para>Ensures the next or can be cast immediately. Duration: 20s Additional Effect: Increases the potency of by 50 Additional Effect: Ensures trigger Verfire Ready or Verstone Ready respectively</para>
    /// </summary>
    
    public IBaseAction AccelerationPvE => _AccelerationPvECreator.Value;
    private readonly Lazy<IBaseAction> _ContreSixtePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7519, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyContreSixtePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7519"><strong>Contre Sixte</strong></see> <i>PvE</i> (RDM) [7519] [Ability]
    /// </summary>
    static partial void ModifyContreSixtePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7519"><strong>Contre Sixte</strong></see> <i>PvE</i> (RDM) [7519] [Ability]
    /// <para>Delivers an attack with a potency of to target and all enemies nearby it.</para>
    /// </summary>
    
    public IBaseAction ContreSixtePvE => _ContreSixtePvECreator.Value;
    private readonly Lazy<IBaseAction> _EmboldenPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7520, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEmboldenPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7520"><strong>Embolden</strong></see> <i>PvE</i> (RDM) [7520] [Ability]
    /// </summary>
    static partial void ModifyEmboldenPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7520"><strong>Embolden</strong></see> <i>PvE</i> (RDM) [7520] [Ability]
    /// <para>Increases own magic damage dealt by 10% and damage dealt by nearby party members by 5%. Duration: 20s</para>
    /// </summary>
    
    public IBaseAction EmboldenPvE => _EmboldenPvECreator.Value;
    private readonly Lazy<IBaseAction> _ManaficationPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7521, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyManaficationPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7521"><strong>Manafication</strong></see> <i>PvE</i> (RDM) [7521] [Ability]
    /// </summary>
    static partial void ModifyManaficationPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7521"><strong>Manafication</strong></see> <i>PvE</i> (RDM) [7521] [Ability]
    /// <para>Temporarily increases the ranges of Enchanted Riposte Enchanted Zwerchhau and Enchanted Redoublement to 25 yalms. Duration: 30s Additional Effect: Grants 3 stacks of Magicked Swordplay each stack allowing the use of Enchanted Riposte Enchanted Zwerchhau Enchanted Redoublement Enchanted Moulinet Enchanted Moulinet Deux or Enchanted Moulinet Trois without cost Duration: 30s All combos are canceled upon execution of Manafication. Can only be executed while in combat.</para>
    /// </summary>
    
    public IBaseAction ManaficationPvE => _ManaficationPvECreator.Value;
    private readonly Lazy<IBaseAction> _VerraisePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7523, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyVerraisePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7523"><strong>Verraise</strong></see> <i>PvE</i> (RDM) [7523] [Spell]
    /// </summary>
    static partial void ModifyVerraisePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7523"><strong>Verraise</strong></see> <i>PvE</i> (RDM) [7523] [Spell]
    /// <para>Resurrects target to a weakened state.</para>
    /// </summary>
    
    public IBaseAction VerraisePvE => _VerraisePvECreator.Value;
    private readonly Lazy<IBaseAction> _JoltIiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7524, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyJoltIiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7524"><strong>Jolt II</strong></see> <i>PvE</i> (RDM) [7524] [Spell]
    /// </summary>
    static partial void ModifyJoltIiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7524"><strong>Jolt II</strong></see> <i>PvE</i> (RDM) [7524] [Spell]
    /// <para>Deals unaspected damage with a potency of 280. Additional Effect: Increases both Black Mana and White Mana by 2</para>
    /// </summary>
    
    public IBaseAction JoltIiPvE => _JoltIiPvECreator.Value;
    private readonly Lazy<IBaseAction> _VerflarePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7525, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyVerflarePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7525"><strong>Verflare</strong></see> <i>PvE</i> (RDM) [7525] [Spell]
    /// </summary>
    static partial void ModifyVerflarePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7525"><strong>Verflare</strong></see> <i>PvE</i> (RDM) [7525] [Spell]
    /// <para>Deals fire damage to target and all enemies nearby it with a potency of for the first enemy and 55% less for all remaining enemies. Additional Effect: Increases Black Mana by 11 Additional Effect: 20% chance of becoming Verfire Ready Duration: 30s Chance to become Verfire Ready increases to 100% if White Mana is higher than Black Mana at time of execution. Mana Stack Cost: 3 ※This action cannot be assigned to a hotbar. ※ and Verthunder II change to Verflare when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction VerflarePvE => _VerflarePvECreator.Value;
    private readonly Lazy<IBaseAction> _VerholyPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7526, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyVerholyPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7526"><strong>Verholy</strong></see> <i>PvE</i> (RDM) [7526] [Spell]
    /// </summary>
    static partial void ModifyVerholyPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7526"><strong>Verholy</strong></see> <i>PvE</i> (RDM) [7526] [Spell]
    /// <para>Deals unaspected damage to target and all enemies nearby it with a potency of for the first enemy and 55% less for all remaining enemies. Additional Effect: Increases White Mana by 11 Additional Effect: 20% chance of becoming Verstone Ready Duration: 30s Chance to become Verstone Ready increases to 100% if Black Mana is higher than White Mana at time of execution. Mana Stack Cost: 3 ※This action cannot be assigned to a hotbar. ※ and Veraero II change to Verholy when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction VerholyPvE => _VerholyPvECreator.Value;
    private readonly Lazy<IBaseAction> _EnchantedRipostePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7527, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEnchantedRipostePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7527"><strong>Enchanted Riposte</strong></see> <i>PvE</i> (RDM) [7527] [Weaponskill]
    /// </summary>
    static partial void ModifyEnchantedRipostePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7527"><strong>Enchanted Riposte</strong></see> <i>PvE</i> (RDM) [7527] [Weaponskill]
    /// <para>Deals unaspected damage with a potency of . Balance Gauge Cost: 20 Black Mana Balance Gauge Cost: 20 White Mana ※This action cannot be assigned to a hotbar. ※Riposte changes to Enchanted Riposte when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction EnchantedRipostePvE => _EnchantedRipostePvECreator.Value;
    private readonly Lazy<IBaseAction> _EnchantedZwerchhauPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7528, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEnchantedZwerchhauPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7528"><strong>Enchanted Zwerchhau</strong></see> <i>PvE</i> (RDM) [7528] [Weaponskill]
    /// </summary>
    static partial void ModifyEnchantedZwerchhauPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7528"><strong>Enchanted Zwerchhau</strong></see> <i>PvE</i> (RDM) [7528] [Weaponskill]
    /// <para>Deals unaspected damage with a potency of . Combo Action: Riposte or Enchanted Riposte Combo Potency: Balance Gauge Cost: 15 Black Mana Balance Gauge Cost: 15 White Mana ※This action cannot be assigned to a hotbar. ※Zwerchhau changes to Enchanted Zwerchhau when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction EnchantedZwerchhauPvE => _EnchantedZwerchhauPvECreator.Value;
    private readonly Lazy<IBaseAction> _EnchantedRedoublementPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7529, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEnchantedRedoublementPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7529"><strong>Enchanted Redoublement</strong></see> <i>PvE</i> (RDM) [7529] [Weaponskill]
    /// </summary>
    static partial void ModifyEnchantedRedoublementPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7529"><strong>Enchanted Redoublement</strong></see> <i>PvE</i> (RDM) [7529] [Weaponskill]
    /// <para>Deals unaspected damage with a potency of . Combo Action: Enchanted Zwerchhau Combo Potency: Balance Gauge Cost: 15 Black Mana Balance Gauge Cost: 15 White Mana ※This action cannot be assigned to a hotbar. ※Redoublement changes to Enchanted Redoublement when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction EnchantedRedoublementPvE => _EnchantedRedoublementPvECreator.Value;
    private readonly Lazy<IBaseAction> _EnchantedMoulinetPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7530, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEnchantedMoulinetPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7530"><strong>Enchanted Moulinet</strong></see> <i>PvE</i> (RDM) [7530] [Weaponskill]
    /// </summary>
    static partial void ModifyEnchantedMoulinetPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7530"><strong>Enchanted Moulinet</strong></see> <i>PvE</i> (RDM) [7530] [Weaponskill]
    /// <para>Deals unaspected damage with a potency of 130 to all enemies in a cone before you. Balance Gauge Cost: 20 Black Mana Balance Gauge Cost: 20 White Mana ※This action cannot be assigned to a hotbar. ※Moulinet changes to Enchanted Moulinet when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction EnchantedMoulinetPvE => _EnchantedMoulinetPvECreator.Value;
    private readonly Lazy<IBaseAction> _VerthunderIiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16524, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyVerthunderIiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16524"><strong>Verthunder II</strong></see> <i>PvE</i> (RDM) [16524] [Spell]
    /// </summary>
    static partial void ModifyVerthunderIiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16524"><strong>Verthunder II</strong></see> <i>PvE</i> (RDM) [16524] [Spell]
    /// <para>Deals lightning damage with a potency of to target and all enemies nearby it. Additional Effect: Increases Black Mana by 7</para>
    /// </summary>
    
    public IBaseAction VerthunderIiPvE => _VerthunderIiPvECreator.Value;
    private readonly Lazy<IBaseAction> _VeraeroIiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16525, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyVeraeroIiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16525"><strong>Veraero II</strong></see> <i>PvE</i> (RDM) [16525] [Spell]
    /// </summary>
    static partial void ModifyVeraeroIiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16525"><strong>Veraero II</strong></see> <i>PvE</i> (RDM) [16525] [Spell]
    /// <para>Deals wind damage with a potency of to target and all enemies nearby it. Additional Effect: Increases White Mana by 7</para>
    /// </summary>
    
    public IBaseAction VeraeroIiPvE => _VeraeroIiPvECreator.Value;
    private readonly Lazy<IBaseAction> _ImpactPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16526, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyImpactPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16526"><strong>Impact</strong></see> <i>PvE</i> (RDM) [16526] [Spell]
    /// </summary>
    static partial void ModifyImpactPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16526"><strong>Impact</strong></see> <i>PvE</i> (RDM) [16526] [Spell]
    /// <para>Deals unaspected damage with a potency of to target and all enemies nearby it. Acceleration Potency: Additional Effect: Increases both Black Mana and White Mana by 3</para>
    /// </summary>
    
    public IBaseAction ImpactPvE => _ImpactPvECreator.Value;
    private readonly Lazy<IBaseAction> _EngagementPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16527, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEngagementPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16527"><strong>Engagement</strong></see> <i>PvE</i> (RDM) [16527] [Ability]
    /// </summary>
    static partial void ModifyEngagementPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16527"><strong>Engagement</strong></see> <i>PvE</i> (RDM) [16527] [Ability]
    /// <para>Delivers an attack with a potency of . Maximum Charges: 2 Shares a recast timer with Displacement.</para>
    /// </summary>
    
    public IBaseAction EngagementPvE => _EngagementPvECreator.Value;
    private readonly Lazy<IBaseAction> _EnchantedReprisePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16528, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEnchantedReprisePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16528"><strong>Enchanted Reprise</strong></see> <i>PvE</i> (RDM) [16528] [Weaponskill]
    /// </summary>
    static partial void ModifyEnchantedReprisePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16528"><strong>Enchanted Reprise</strong></see> <i>PvE</i> (RDM) [16528] [Weaponskill]
    /// <para>Deals unaspected damage with a potency of . Balance Gauge Cost: 5 Black Mana Balance Gauge Cost: 5 White Mana ※This action cannot be assigned to a hotbar. ※Reprise changes to Enchanted Reprise when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction EnchantedReprisePvE => _EnchantedReprisePvECreator.Value;
    private readonly Lazy<IBaseAction> _ReprisePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16529, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyReprisePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16529"><strong>Reprise</strong></see> <i>PvE</i> (RDM) [16529] [Weaponskill]
    /// </summary>
    static partial void ModifyReprisePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16529"><strong>Reprise</strong></see> <i>PvE</i> (RDM) [16529] [Weaponskill]
    /// <para>Delivers an attack with a potency of 100. Action upgraded to Enchanted Reprise if both Black Mana and White Mana are at 5 or more.</para>
    /// </summary>
    
    public IBaseAction ReprisePvE => _ReprisePvECreator.Value;
    private readonly Lazy<IBaseAction> _ScorchPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16530, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyScorchPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16530"><strong>Scorch</strong></see> <i>PvE</i> (RDM) [16530] [Spell]
    /// </summary>
    static partial void ModifyScorchPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16530"><strong>Scorch</strong></see> <i>PvE</i> (RDM) [16530] [Spell]
    /// <para>Deals unaspected damage to target and all enemies nearby it with a potency of for the first enemy and 55% less for all remaining enemies. Combo Action: Verflare or Verholy Additional Effect: Increases both Black Mana and White Mana by 4 Can only be executed after successfully landing Verflare or Verholy as a combo action. ※This action cannot be assigned to a hotbar. ※ and Impact change to Scorch when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction ScorchPvE => _ScorchPvECreator.Value;
    private readonly Lazy<IBaseAction> _VerthunderIiiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25855, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyVerthunderIiiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25855"><strong>Verthunder III</strong></see> <i>PvE</i> (RDM) [25855] [Spell]
    /// </summary>
    static partial void ModifyVerthunderIiiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25855"><strong>Verthunder III</strong></see> <i>PvE</i> (RDM) [25855] [Spell]
    /// <para>Deals lightning damage with a potency of . Additional Effect: Increases Black Mana by 6 Additional Effect: 50% chance of becoming Verfire Ready Duration: 30s</para>
    /// </summary>
    
    public IBaseAction VerthunderIiiPvE => _VerthunderIiiPvECreator.Value;
    private readonly Lazy<IBaseAction> _VeraeroIiiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25856, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyVeraeroIiiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25856"><strong>Veraero III</strong></see> <i>PvE</i> (RDM) [25856] [Spell]
    /// </summary>
    static partial void ModifyVeraeroIiiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25856"><strong>Veraero III</strong></see> <i>PvE</i> (RDM) [25856] [Spell]
    /// <para>Deals wind damage with a potency of . Additional Effect: Increases White Mana by 6 Additional Effect: 50% chance of becoming Verstone Ready Duration: 30s</para>
    /// </summary>
    
    public IBaseAction VeraeroIiiPvE => _VeraeroIiiPvECreator.Value;
    private readonly Lazy<IBaseAction> _MagickBarrierPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25857, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyMagickBarrierPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25857"><strong>Magick Barrier</strong></see> <i>PvE</i> (RDM) [25857] [Ability]
    /// </summary>
    static partial void ModifyMagickBarrierPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25857"><strong>Magick Barrier</strong></see> <i>PvE</i> (RDM) [25857] [Ability]
    /// <para>Reduces magic damage taken by self and nearby party members by 10% while increasing HP recovered by healing actions by 5%. Duration: 10s</para>
    /// </summary>
    
    public IBaseAction MagickBarrierPvE => _MagickBarrierPvECreator.Value;
    private readonly Lazy<IBaseAction> _ResolutionPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25858, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyResolutionPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25858"><strong>Resolution</strong></see> <i>PvE</i> (RDM) [25858] [Spell]
    /// </summary>
    static partial void ModifyResolutionPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25858"><strong>Resolution</strong></see> <i>PvE</i> (RDM) [25858] [Spell]
    /// <para>Deals unaspected damage to all enemies in a straight line before you with a potency of for the first enemy and 55% less for all remaining enemies. Combo Action: Scorch Additional Effect: Increases both Black Mana and White Mana by 4 Can only be executed after successfully landing Scorch as a combo action. ※This action cannot be assigned to a hotbar. ※Scorch changes to Resolution when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction ResolutionPvE => _ResolutionPvECreator.Value;
    private readonly Lazy<IBaseAction> _CorpsacorpsPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29699, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyCorpsacorpsPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29699"><strong>Corps-a-corps</strong></see> <i>PvP</i> (RDM) [29699] [Ability]
    /// </summary>
    static partial void ModifyCorpsacorpsPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29699"><strong>Corps-a-corps</strong></see> <i>PvP</i> (RDM) [29699] [Ability]
    /// <para>Rushes target and delivers an attack with a potency of 3,000. Additional Effect: Afflicts target with Monomachy Monomachy Effect: Increases damage dealt to target by 15% while lowering damage taken from target by 15% Duration: 8s Maximum Charges: 2 Cannot be executed while bound.</para>
    /// </summary>
    
    public IBaseAction CorpsacorpsPvP => _CorpsacorpsPvPCreator.Value;
    private readonly Lazy<IBaseAction> _DisplacementPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29700, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyDisplacementPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29700"><strong>Displacement</strong></see> <i>PvP</i> (RDM) [29700] [Ability]
    /// </summary>
    static partial void ModifyDisplacementPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29700"><strong>Displacement</strong></see> <i>PvP</i> (RDM) [29700] [Ability]
    /// <para>Delivers an attack with a potency of 3,000. Additional Effect: 15-yalm backstep Additional Effect: Increases damage and healing potency of next spell cast by 15% Duration: 10s Maximum Charges: 2 Cannot be executed while bound.</para>
    /// </summary>
    
    public IBaseAction DisplacementPvP => _DisplacementPvPCreator.Value;
    private readonly Lazy<IBaseAction> _EnchantedMoulinetDeuxPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)37002, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEnchantedMoulinetDeuxPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/37002"><strong>Enchanted Moulinet Deux</strong></see> <i>PvE</i> (RDM) [37002] [Weaponskill]
    /// </summary>
    static partial void ModifyEnchantedMoulinetDeuxPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/37002"><strong>Enchanted Moulinet Deux</strong></see> <i>PvE</i> (RDM) [37002] [Weaponskill]
    /// <para>Deals unaspected damage with a potency of 140 to all enemies in a cone before you. Combo Action: Enchanted Moulinet Balance Gauge Cost: 15 Black Mana Balance Gauge Cost: 15 White Mana ※This action cannot be assigned to a hotbar. ※Enchanted Moulinet changes to Enchanted Moulinet Deux when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction EnchantedMoulinetDeuxPvE => _EnchantedMoulinetDeuxPvECreator.Value;
    private readonly Lazy<IBaseAction> _EnchantedMoulinetTroisPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)37003, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEnchantedMoulinetTroisPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/37003"><strong>Enchanted Moulinet Trois</strong></see> <i>PvE</i> (RDM) [37003] [Weaponskill]
    /// </summary>
    static partial void ModifyEnchantedMoulinetTroisPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/37003"><strong>Enchanted Moulinet Trois</strong></see> <i>PvE</i> (RDM) [37003] [Weaponskill]
    /// <para>Deals unaspected damage with a potency of 150 to all enemies in a cone before you. Combo Action: Enchanted Moulinet Deux Balance Gauge Cost: 15 Black Mana Balance Gauge Cost: 15 White Mana ※This action cannot be assigned to a hotbar. ※Enchanted Moulinet Deux changes to Enchanted Moulinet Trois when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction EnchantedMoulinetTroisPvE => _EnchantedMoulinetTroisPvECreator.Value;
    private readonly Lazy<IBaseAction> _JoltIiiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)37004, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyJoltIiiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/37004"><strong>Jolt III</strong></see> <i>PvE</i> (RDM) [37004] [Spell]
    /// </summary>
    static partial void ModifyJoltIiiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/37004"><strong>Jolt III</strong></see> <i>PvE</i> (RDM) [37004] [Spell]
    /// <para>Deals unaspected damage with a potency of 360. Additional Effect: Increases both Black Mana and White Mana by 2</para>
    /// </summary>
    
    public IBaseAction JoltIiiPvE => _JoltIiiPvECreator.Value;
    private readonly Lazy<IBaseAction> _ViceOfThornsPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)37005, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyViceOfThornsPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/37005"><strong>Vice of Thorns</strong></see> <i>PvE</i> (RDM) [37005] [Ability]
    /// </summary>
    static partial void ModifyViceOfThornsPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/37005"><strong>Vice of Thorns</strong></see> <i>PvE</i> (RDM) [37005] [Ability]
    /// <para>Deals unaspected damage to target and all enemies nearby it with a potency of 950 for the first enemy and 55% less for all remaining enemies. Can only be executed while under the effect of Thorned Flourish.</para>
    /// </summary>
    
    public IBaseAction ViceOfThornsPvE => _ViceOfThornsPvECreator.Value;
    private readonly Lazy<IBaseAction> _GrandImpactPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)37006, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyGrandImpactPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/37006"><strong>Grand Impact</strong></see> <i>PvE</i> (RDM) [37006] [Spell]
    /// </summary>
    static partial void ModifyGrandImpactPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/37006"><strong>Grand Impact</strong></see> <i>PvE</i> (RDM) [37006] [Spell]
    /// <para>Deals unaspected damage to target and all enemies nearby it with a potency of 600 for the first enemy and 55% less for all remaining enemies. Additional Effect: Increases both Black Mana and White Mana by 3 Can only be executed while under the effect of Grand Impact Ready. ※This action cannot be assigned to a hotbar. ※Jolt III and Impact change to Grand Impact when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction GrandImpactPvE => _GrandImpactPvECreator.Value;
    private readonly Lazy<IBaseAction> _PrefulgencePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)37007, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyPrefulgencePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/37007"><strong>Prefulgence</strong></see> <i>PvE</i> (RDM) [37007] [Ability]
    /// </summary>
    static partial void ModifyPrefulgencePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/37007"><strong>Prefulgence</strong></see> <i>PvE</i> (RDM) [37007] [Ability]
    /// <para>Deals unaspected damage to target and all enemies nearby it with a potency of 1,200 for the first enemy and 55% less for all remaining enemies. Can only be executed while under the effect of Prefulgence Ready.</para>
    /// </summary>
    
    public IBaseAction PrefulgencePvE => _PrefulgencePvECreator.Value;
    private readonly Lazy<IBaseAction> _JoltIiiPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41486, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyJoltIiiPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41486"><strong>Jolt III</strong></see> <i>PvP</i> (RDM) [41486] [Spell]
    /// </summary>
    static partial void ModifyJoltIiiPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41486"><strong>Jolt III</strong></see> <i>PvP</i> (RDM) [41486] [Spell]
    /// <para>Deals unaspected damage with a potency of 6,000. Additional Effect: Grants Dualcast Duration: 15s ※Action changes to Grand Impact upon execution.</para>
    /// </summary>
    
    public IBaseAction JoltIiiPvP => _JoltIiiPvPCreator.Value;
    private readonly Lazy<IBaseAction> _GrandImpactPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41487, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyGrandImpactPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41487"><strong>Grand Impact</strong></see> <i>PvP</i> (RDM) [41487] [Spell]
    /// </summary>
    static partial void ModifyGrandImpactPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41487"><strong>Grand Impact</strong></see> <i>PvP</i> (RDM) [41487] [Spell]
    /// <para>Deals unaspected damage to target and all enemies nearby it with a potency of 6,000 for the first enemy and 4,000 for all remaining enemies. Can only be executed while under the effect of Dualcast. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction GrandImpactPvP => _GrandImpactPvPCreator.Value;
    private readonly Lazy<IBaseAction> _EnchantedRipostePvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41488, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEnchantedRipostePvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41488"><strong>Enchanted Riposte</strong></see> <i>PvP</i> (RDM) [41488] [Weaponskill]
    /// </summary>
    static partial void ModifyEnchantedRipostePvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41488"><strong>Enchanted Riposte</strong></see> <i>PvP</i> (RDM) [41488] [Weaponskill]
    /// <para>Deals unaspected damage with a potency of 5,000. Ignores the effects of Guard when dealing damage. Additional Effect: Creates a barrier around self that absorbs damage equivalent to a heal of 4,000 potency Duration: 6s ※Action changes to Enchanted Zwerchhau upon execution.</para>
    /// </summary>
    
    public IBaseAction EnchantedRipostePvP => _EnchantedRipostePvPCreator.Value;
    private readonly Lazy<IBaseAction> _EnchantedZwerchhauPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41489, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEnchantedZwerchhauPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41489"><strong>Enchanted Zwerchhau</strong></see> <i>PvP</i> (RDM) [41489] [Weaponskill]
    /// </summary>
    static partial void ModifyEnchantedZwerchhauPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41489"><strong>Enchanted Zwerchhau</strong></see> <i>PvP</i> (RDM) [41489] [Weaponskill]
    /// <para>Deals unaspected damage with a potency of 6,000. Ignores the effects of Guard when dealing damage. Combo Action: Enchanted Riposte Additional Effect: Creates a barrier around self that absorbs damage equivalent to a heal of 4,000 potency Duration: 6s ※This action cannot be assigned to a hotbar. ※Action changes to Enchanted Redoublement upon execution.</para>
    /// </summary>
    
    public IBaseAction EnchantedZwerchhauPvP => _EnchantedZwerchhauPvPCreator.Value;
    private readonly Lazy<IBaseAction> _EnchantedRedoublementPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41490, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEnchantedRedoublementPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41490"><strong>Enchanted Redoublement</strong></see> <i>PvP</i> (RDM) [41490] [Weaponskill]
    /// </summary>
    static partial void ModifyEnchantedRedoublementPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41490"><strong>Enchanted Redoublement</strong></see> <i>PvP</i> (RDM) [41490] [Weaponskill]
    /// <para>Deals unaspected damage with a potency of 7,000. Ignores the effects of Guard when dealing damage. Combo Action: Enchanted Zwerchhau Additional Effect: Creates a barrier around self that absorbs damage equivalent to a heal of 4,000 potency Duration: 6s ※This action cannot be assigned to a hotbar. ※Action changes to Scorch upon execution.</para>
    /// </summary>
    
    public IBaseAction EnchantedRedoublementPvP => _EnchantedRedoublementPvPCreator.Value;
    private readonly Lazy<IBaseAction> _ScorchPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41491, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyScorchPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41491"><strong>Scorch</strong></see> <i>PvP</i> (RDM) [41491] [Spell]
    /// </summary>
    static partial void ModifyScorchPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41491"><strong>Scorch</strong></see> <i>PvP</i> (RDM) [41491] [Spell]
    /// <para>Deals unaspected damage with a potency of 8,000 to target and all enemies nearby it. Combo Action: Enchanted Redoublement Additional Effect: Damage over time Potency: 4,000 Duration: 6s ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction ScorchPvP => _ScorchPvPCreator.Value;
    private readonly Lazy<IBaseAction> _ResolutionPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41492, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyResolutionPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41492"><strong>Resolution</strong></see> <i>PvP</i> (RDM) [41492] [Spell]
    /// </summary>
    static partial void ModifyResolutionPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41492"><strong>Resolution</strong></see> <i>PvP</i> (RDM) [41492] [Spell]
    /// <para>Deals unaspected damage with a potency of 8,000 to all enemies in a straight line. Additional Effect: Silence Duration: 2s This action does not share a recast timer with any other actions.</para>
    /// </summary>
    
    public IBaseAction ResolutionPvP => _ResolutionPvPCreator.Value;
    private readonly Lazy<IBaseAction> _ViceOfThornsPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41493, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyViceOfThornsPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41493"><strong>Vice of Thorns</strong></see> <i>PvP</i> (RDM) [41493] [Ability]
    /// </summary>
    static partial void ModifyViceOfThornsPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41493"><strong>Vice of Thorns</strong></see> <i>PvP</i> (RDM) [41493] [Ability]
    /// <para>Deals unaspected damage with a potency of 4,000 to target and all enemies nearby it. Additional Effect: Stun Duration: 2s Can only be executed while under the effect of Thorned Flourish. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction ViceOfThornsPvP => _ViceOfThornsPvPCreator.Value;
    private readonly Lazy<IBaseAction> _EmboldenPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41494, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEmboldenPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41494"><strong>Embolden</strong></see> <i>PvP</i> (RDM) [41494] [Ability]
    /// </summary>
    static partial void ModifyEmboldenPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41494"><strong>Embolden</strong></see> <i>PvP</i> (RDM) [41494] [Ability]
    /// <para>Increases damage dealt by self and nearby party members by 8% while reducing damage taken by 8%. Duration: 8s Additional Effect: Grants Prefulgence Ready Duration: 15s This action's effects extend through obstructions. ※Action changes to Prefulgence upon execution.</para>
    /// </summary>
    
    public IBaseAction EmboldenPvP => _EmboldenPvPCreator.Value;
    private readonly Lazy<IBaseAction> _PrefulgencePvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41495, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyPrefulgencePvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41495"><strong>Prefulgence</strong></see> <i>PvP</i> (RDM) [41495] [Spell]
    /// </summary>
    static partial void ModifyPrefulgencePvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41495"><strong>Prefulgence</strong></see> <i>PvP</i> (RDM) [41495] [Spell]
    /// <para>Deals unaspected damage with a potency of 10,000 to target and all enemies nearby it. Additional Effect: Restores own HP Cure Potency: 10,000 Additional Effect: Restores HP of party members near target Cure Potency: 10,000 Can only be executed while under the effect of Prefulgence Ready. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction PrefulgencePvP => _PrefulgencePvPCreator.Value;
    private readonly Lazy<IBaseAction> _FortePvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41496, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyFortePvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41496"><strong>Forte</strong></see> <i>PvP</i> (RDM) [41496] [Ability]
    /// </summary>
    static partial void ModifyFortePvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41496"><strong>Forte</strong></see> <i>PvP</i> (RDM) [41496] [Ability]
    /// <para>Creates a barrier around self that reduces damage taken by 50% and absorbs damage equivalent to a heal of 4,000 potency. Duration: 5s Grants Thorned Flourish when barrier is completely absorbed. Duration: 10s ※Action changes to Vice of Thorns while under the effect of Thorned Flourish.</para>
    /// </summary>
    
    public IBaseAction FortePvP => _FortePvPCreator.Value;
    private readonly Lazy<IBaseAction> _EnchantedRipostePvE_45960Creator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)45960, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEnchantedRipostePvE_45960(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/45960"><strong>Enchanted Riposte</strong></see> <i>PvE</i> (RDM) [45960] [Weaponskill]
    /// </summary>
    static partial void ModifyEnchantedRipostePvE_45960(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/45960"><strong>Enchanted Riposte</strong></see> <i>PvE</i> (RDM) [45960] [Weaponskill]
    /// <para>Deals unaspected damage with a potency of . Balance Gauge Cost: 20 Black Mana Balance Gauge Cost: 20 White Mana ※This action cannot be assigned to a hotbar. ※Riposte changes to Enchanted Riposte when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction EnchantedRipostePvE_45960 => _EnchantedRipostePvE_45960Creator.Value;
    private readonly Lazy<IBaseAction> _EnchantedZwerchhauPvE_45961Creator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)45961, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEnchantedZwerchhauPvE_45961(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/45961"><strong>Enchanted Zwerchhau</strong></see> <i>PvE</i> (RDM) [45961] [Weaponskill]
    /// </summary>
    static partial void ModifyEnchantedZwerchhauPvE_45961(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/45961"><strong>Enchanted Zwerchhau</strong></see> <i>PvE</i> (RDM) [45961] [Weaponskill]
    /// <para>Deals unaspected damage with a potency of . Combo Action: Riposte or Enchanted Riposte Combo Potency: Balance Gauge Cost: 15 Black Mana Balance Gauge Cost: 15 White Mana ※This action cannot be assigned to a hotbar. ※Zwerchhau changes to Enchanted Zwerchhau when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction EnchantedZwerchhauPvE_45961 => _EnchantedZwerchhauPvE_45961Creator.Value;
    private readonly Lazy<IBaseAction> _EnchantedRedoublementPvE_45962Creator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)45962, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEnchantedRedoublementPvE_45962(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/45962"><strong>Enchanted Redoublement</strong></see> <i>PvE</i> (RDM) [45962] [Weaponskill]
    /// </summary>
    static partial void ModifyEnchantedRedoublementPvE_45962(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/45962"><strong>Enchanted Redoublement</strong></see> <i>PvE</i> (RDM) [45962] [Weaponskill]
    /// <para>Deals unaspected damage with a potency of . Combo Action: Enchanted Zwerchhau Combo Potency: Balance Gauge Cost: 15 Black Mana Balance Gauge Cost: 15 White Mana ※This action cannot be assigned to a hotbar. ※Redoublement changes to Enchanted Redoublement when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction EnchantedRedoublementPvE_45962 => _EnchantedRedoublementPvE_45962Creator.Value;

    private IBaseAction[] _AllBaseActions = null;
    
    /// <inheritdoc/>
    public override IBaseAction[] AllBaseActions => _AllBaseActions ??= [
    	JoltPvE, RipostePvE, VerthunderPvE, CorpsacorpsPvE, VeraeroPvE, ScatterPvE, VerfirePvE, VerstonePvE, ZwerchhauPvE, MoulinetPvE, VercurePvE, DisplacementPvE, RedoublementPvE, FlechePvE, AccelerationPvE, ContreSixtePvE, EmboldenPvE, ManaficationPvE, VerraisePvE, JoltIiPvE, VerflarePvE, VerholyPvE, EnchantedRipostePvE, EnchantedZwerchhauPvE, EnchantedRedoublementPvE, EnchantedMoulinetPvE, VerthunderIiPvE, VeraeroIiPvE, ImpactPvE, EngagementPvE, EnchantedReprisePvE, ReprisePvE, ScorchPvE, VerthunderIiiPvE, VeraeroIiiPvE, MagickBarrierPvE, ResolutionPvE, CorpsacorpsPvP, DisplacementPvP, EnchantedMoulinetDeuxPvE, EnchantedMoulinetTroisPvE, JoltIiiPvE, ViceOfThornsPvE, GrandImpactPvE, PrefulgencePvE, JoltIiiPvP, GrandImpactPvP, EnchantedRipostePvP, EnchantedZwerchhauPvP, EnchantedRedoublementPvP, ScorchPvP, ResolutionPvP, ViceOfThornsPvP, EmboldenPvP, PrefulgencePvP, FortePvP,
    	..base.AllBaseActions,
    ];

private readonly Lazy<IBaseAction> _SkyshardPvECreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)203, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifySkyshardPvE(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/203"><strong>Skyshard</strong></see> <i>PvE</i> (All Classes) [203] [Limit Break]
/// </summary>
static partial void ModifySkyshardPvE(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/203"><strong>Skyshard</strong></see> <i>PvE</i> (All Classes) [203] [Limit Break]
/// <para>Deals unaspected damage with a potency of 1,650 to all enemies near point of impact.</para>
/// </summary>

public IBaseAction SkyshardPvE => _SkyshardPvECreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/203"><strong>Skyshard</strong></see> <i>PvE</i> (All Classes) [203] [Limit Break]
/// <para>Deals unaspected damage with a potency of 1,650 to all enemies near point of impact.</para>
/// </summary>
private sealed protected override IBaseAction LimitBreak1 => SkyshardPvE;
private readonly Lazy<IBaseAction> _StarstormPvECreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)204, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifyStarstormPvE(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/204"><strong>Starstorm</strong></see> <i>PvE</i> (All Classes) [204] [Limit Break]
/// </summary>
static partial void ModifyStarstormPvE(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/204"><strong>Starstorm</strong></see> <i>PvE</i> (All Classes) [204] [Limit Break]
/// <para>Deals unaspected damage with a potency of 3,600 to all enemies near point of impact.</para>
/// </summary>

public IBaseAction StarstormPvE => _StarstormPvECreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/204"><strong>Starstorm</strong></see> <i>PvE</i> (All Classes) [204] [Limit Break]
/// <para>Deals unaspected damage with a potency of 3,600 to all enemies near point of impact.</para>
/// </summary>
private sealed protected override IBaseAction LimitBreak2 => StarstormPvE;
private readonly Lazy<IBaseAction> _VermilionScourgePvECreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)7862, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifyVermilionScourgePvE(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/7862"><strong>Vermilion Scourge</strong></see> <i>PvE</i> (All Classes) [7862] [Limit Break]
/// </summary>
static partial void ModifyVermilionScourgePvE(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/7862"><strong>Vermilion Scourge</strong></see> <i>PvE</i> (All Classes) [7862] [Limit Break]
/// <para></para>
/// </summary>

public IBaseAction VermilionScourgePvE => _VermilionScourgePvECreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/7862"><strong>Vermilion Scourge</strong></see> <i>PvE</i> (All Classes) [7862] [Limit Break]
/// <para></para>
/// </summary>
private sealed protected override IBaseAction LimitBreak3 => VermilionScourgePvE;
private readonly Lazy<IBaseAction> _SouthernCrossPvPCreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)41498, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifySouthernCrossPvP(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/41498"><strong>Southern Cross</strong></see> <i>PvP</i> (RDM) [41498] [Limit Break]
/// </summary>
static partial void ModifySouthernCrossPvP(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/41498"><strong>Southern Cross</strong></see> <i>PvP</i> (RDM) [41498] [Limit Break]
/// <para>Emblazons a grand cross on the ground beneath a party member or enemy restoring HP of party members within range while damaging enemies. Cure Potency: 12,000 Damage Potency: 12,000 Targets standing at the cross's center will receive the effects of this action twice. Can only be executed when the limit gauge is full. Gauge Charge Time: 90s</para>
/// </summary>

private IBaseAction SouthernCrossPvP => _SouthernCrossPvPCreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/41498"><strong>Southern Cross</strong></see> <i>PvP</i> (RDM) [41498] [Limit Break]
/// <para>Emblazons a grand cross on the ground beneath a party member or enemy restoring HP of party members within range while damaging enemies. Cure Potency: 12,000 Damage Potency: 12,000 Targets standing at the cross's center will receive the effects of this action twice. Can only be executed when the limit gauge is full. Gauge Charge Time: 90s</para>
/// </summary>
private sealed protected override IBaseAction LimitBreakPvP => SouthernCrossPvP;

#endregion

#region Traits

    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/50195"><strong>Enhanced Jolt) | Jolt II |
| Manafication | braucht (Erzeuger nicht im Text) | combat |
| Moulinet | Knopf wird zu | Enchanted Moulinet |
| Prefulgence | braucht Prefulgence Ready | Eigenschaft Enhanced Manafication |
| Redoublement | Knopf wird zu | Enchanted Redoublement |
| Redoublement | Combo nach | Enchanted Zwerchhau |
| Redoublement | Combo nach | Zwerchhau |
| Reprise | Knopf wird zu | Enchanted Reprise |
| Resolution | Combo nach | Scorch |
| Riposte | Knopf wird zu | Enchanted Riposte |
| Scatter | Ausbau (Scatter Mastery) | Impact |
| Scorch | Knopf wird zu | Resolution |
| Scorch | Combo nach | Verflare |
| Scorch | Combo nach | Verholy |
| Veraero II | Ausbau (Mana Stack) | Verholy |
| Veraero II | Knopf wird zu | Verholy |
| Veraero | Ausbau (Red Magic Mastery II) | Veraero III |
| Veraero | Ausbau (Mana Stack) | Verholy |
| Verfire | braucht Verfire Ready | Verfire |
| Verflare | kostet | Mana Stack |
| Verholy | kostet | Mana Stack |
| Verstone | braucht Verstone Ready | Verstone |
| Verthunder II | Knopf wird zu | Verflare |
| Verthunder | Ausbau (Red Magic Mastery II) | Verthunder III |
| Vice of Thorns | braucht Thorned Flourish | Eigenschaft Enhanced Embolden |
| Zwerchhau | Combo nach | Enchanted Riposte |
| Zwerchhau | Knopf wird zu | Enchanted Zwerchhau |
| Zwerchhau | Combo nach | Riposte |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Acceleration | Regel prüft | Embolden |
| Acceleration | Regel prüft | Enchanted Reprise |
| Acceleration | Regel prüft | Grand Impact |
| Acceleration | Regel prüft | Swiftcast |
| Corps-a-corps | Regel prüft | Embolden |
| Displacement | Regel prüft | Engagement |
| Enchanted Moulinet Deux | ComboIds | Enchanted Moulinet |
| Enchanted Moulinet Trois | ComboIds | Enchanted Moulinet Deux |
| Enchanted Redoublement | ComboIds | Enchanted Zwerchhau |
| Enchanted Reprise | Regel prüft | Acceleration |
| Enchanted Reprise | Regel prüft | Grand Impact |
| Enchanted Reprise | Regel prüft | Swiftcast |
| Enchanted Zwerchhau | ComboIds | Enchanted Riposte |
| Engagement | Regel prüft | Embolden |
| Grand Impact | Regel prüft | Acceleration |
| Grand Impact | Regel prüft | Enchanted Reprise |
| Grand Impact | StatusNeed GrandImpactReady | Status GrandImpactReady |
| Grand Impact | Regel prüft | Swiftcast |
| Manafication | Regel prüft | Embolden |
| Prefulgence | StatusNeed PrefulgenceReady | Status PrefulgenceReady |
| Redoublement | ComboIds | Zwerchhau |
| Resolution | Regel prüft | Riposte |
| Resolution | ComboIds | Scorch |
| Resolution | Regel prüft | Scorch |
| Resolution | Regel prüft | Zwerchhau |
| Scatter | Regel prüft | Impact |
| Scorch | Regel prüft | Resolution |
| Scorch | Regel prüft | Riposte |
| Scorch | ComboIds | Verfire |
| Scorch | ComboIds | Verholy |
| Scorch | Regel prüft | Zwerchhau |
| Swiftcast | Regel prüft | Acceleration |
| Swiftcast | Regel prüft | Embolden |
| Swiftcast | Regel prüft | Enchanted Reprise |
| Swiftcast | Regel prüft | Grand Impact |
| Swiftcast | Regel prüft | Veraero III |
| Swiftcast | Regel prüft | Veraero |
| Swiftcast | Regel prüft | Verthunder III |
| Swiftcast | Regel prüft | Verthunder |
| Veraero | Regel prüft | Veraero III |
| Vercure | Regel sperrt vorher | Resolution |
| Vercure | Regel sperrt vorher | Riposte |
| Vercure | Regel sperrt vorher | Scorch |
| Vercure | Regel sperrt vorher | Zwerchhau |
| Verfire | Regel prüft | Verstone |
| Verfire | StatusNeed VerfireReady | Verthunder III |
| Verfire | StatusNeed VerfireReady | Verthunder |
| Verflare | ComboIds | Enchanted Moulinet Trois |
| Verflare | ComboIds | Enchanted Redoublement |
| Verflare | ComboIds | Redoublement |
| Verholy | ComboIds | Enchanted Moulinet Trois |
| Verholy | ComboIds | Enchanted Redoublement |
| Verholy | ComboIds | Redoublement |
| Verraise | StatusNeed Dualcast | Jolt |
| Verraise | Regel sperrt vorher | Resolution |
| Verraise | Regel sperrt vorher | Riposte |
| Verraise | Regel sperrt vorher | Scorch |
| Verraise | StatusNeed Dualcast | Vercure |
| Verraise | Regel sperrt vorher | Zwerchhau |
| Verstone | StatusNeed VerstoneReady | Veraero III |
| Verstone | StatusNeed VerstoneReady | Veraero |
| Verthunder | Regel prüft | Verthunder III |
| Vice of Thorns | StatusNeed ThornedFlourish | Status ThornedFlourish |
| Zwerchhau | ComboIds | Riposte |

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `SkyshardPvE`, `StarstormPvE`, `VermilionScourgePvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Moulinet (`MoulinetPvE`, Weaponskill): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen
- Redoublement (`RedoublementPvE`, Weaponskill): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen
- Reprise (`ReprisePvE`, Weaponskill): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen
- Riposte (`RipostePvE`, Weaponskill): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen
- Sleep (`SleepPvE`, Spell): ungenutzt
- Zwerchhau (`ZwerchhauPvE`, Weaponskill): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen

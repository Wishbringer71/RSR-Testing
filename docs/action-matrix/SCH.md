# SCH — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `ScholarRotation`
- Rotation: `RotationSolver/RebornRotations/Healer/SCH_Reborn.cs`
- Matrix als Tabelle: `SCH.csv`

## Nutzung

direkt: 44 · ungenutzt: 5 · über anderen Knopf: 1

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Heiler | Esuna (`EsunaPvE`) | 7568 | Spell | direkt |
| Heiler | Lucid Dreaming (`LucidDreamingPvE`) | 7562 | Ability | direkt |
| Heiler | Repose (`ReposePvE`) | 16560 | Spell | ungenutzt |
| Heiler | Rescue (`RescuePvE`) | 7571 | Ability | ungenutzt |
| Heiler | Surecast (`SurecastPvE`) | 7559 | Ability | direkt |
| Heiler | Swiftcast (`SwiftcastPvE`) | 7561 | Ability | direkt |
| Job | Accession (`AccessionPvE`) | 37016 | Spell | direkt |
| Job | Adloquium (`AdloquiumPvE`) | 185 | Spell | direkt |
| Job | Aetherflow (`AetherflowPvE`) | 166 | Ability | direkt |
| Job | Aetherpact (`AetherpactPvE`) | 7437 | Ability | direkt |
| Job | Art of War (`ArtOfWarPvE`) | 16539 | Spell | direkt |
| Job | Art of War II (`ArtOfWarIiPvE`) | 25866 | Spell | über Art of War |
| Job | Baneful Impaction (`BanefulImpactionPvE`) | 37012 | Ability | direkt |
| Job | Bio (`BioPvE`) | 17864 | Spell | direkt |
| Job | Bio II (`BioIiPvE`) | 17865 | Spell | direkt |
| Job | Biolysis (`BiolysisPvE`) | 16540 | Spell | direkt |
| Job | Broil (`BroilPvE`) | 3584 | Spell | direkt |
| Job | Broil II (`BroilIiPvE`) | 7435 | Spell | direkt |
| Job | Broil III (`BroilIiiPvE`) | 16541 | Spell | direkt |
| Job | Broil IV (`BroilIvPvE`) | 25865 | Spell | direkt |
| Job | Chain Stratagem (`ChainStratagemPvE`) | 7436 | Ability | direkt |
| Job | Concitation (`ConcitationPvE`) | 37013 | Spell | direkt |
| Job | Consolation (`ConsolationPvE`) | 16546 | Ability | direkt |
| Job | Deployment Tactics (`DeploymentTacticsPvE`) | 3585 | Ability | direkt |
| Job | Dissipation (`DissipationPvE`) | 3587 | Ability | direkt |
| Job | Dissolve Union (`DissolveUnionPvE`) | 7869 | Ability | ungenutzt |
| Job | Embrace (`EmbracePvE`) | 802 | Spell | ungenutzt — nicht zuweisbar: Begleiter oder Automatik |
| Job | Emergency Tactics (`EmergencyTacticsPvE`) | 3586 | Ability | direkt |
| Job | Energy Drain (`EnergyDrainPvE`) | 167 | Ability | direkt |
| Job | Excogitation (`ExcogitationPvE`) | 7434 | Ability | direkt |
| Job | Expedient (`ExpedientPvE`) | 25868 | Ability | direkt |
| Job | Fey Blessing (`FeyBlessingPvE`) | 16543 | Ability | direkt |
| Job | Fey Illumination (`FeyIlluminationPvE`) | 16538 | Ability | direkt |
| Job | Indomitability (`IndomitabilityPvE`) | 3583 | Ability | direkt |
| Job | Lustrate (`LustratePvE`) | 189 | Ability | direkt |
| Job | Manifestation (`ManifestationPvE`) | 37015 | Spell | direkt |
| Job | Physick (`PhysickPvE`) | 190 | Spell | direkt |
| Job | Protraction (`ProtractionPvE`) | 25867 | Ability | direkt |
| Job | Recitation (`RecitationPvE`) | 16542 | Ability | direkt |
| Job | Resurrection (`ResurrectionPvE`) | 173 | Spell | direkt |
| Job | Ruin (`RuinPvE`) | 163 | Spell | direkt |
| Job | Ruin II (`RuinIiPvE`) | 172 | Spell | direkt |
| Job | Sacred Soil (`SacredSoilPvE`) | 188 | Ability | direkt |
| Job | Seraphic Veil (`SeraphicVeilPvE`) | 16548 | Spell | ungenutzt — nicht zuweisbar: Begleiter oder Automatik |
| Job | Seraphism (`SeraphismPvE`) | 37014 | Ability | direkt |
| Job | Succor (`SuccorPvE`) | 186 | Spell | direkt |
| Job | Summon Eos (`SummonEosPvE`) | 17215 | Spell | direkt |
| Job | Summon Seraph (`SummonSeraphPvE`) | 16545 | Ability | direkt |
| Job | Whispering Dawn (`WhisperingDawnPvE`) | 16537 | Ability | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Accession | braucht Seraphism | Seraphism |
| Adloquium | Knopf wird zu | Manifestation |
| Aetherflow | braucht (Erzeuger nicht im Text) | combat |
| Aetherpact | kostet | Faerie |
| Art of War | Ausbau (Art of War Mastery) | Art of War II |
| Baneful Impaction | braucht Impact Imminent | Eigenschaft Enhanced Chain Stratagem |
| Bio II | Ausbau (Corruption Mastery II) | Biolysis |
| Bio | Ausbau (Corruption Mastery) | Bio II |
| Broil II | Ausbau (Broil Mastery III) | Broil III |
| Broil III | Ausbau (Broil Mastery IV) | Broil IV |
| Broil | Ausbau (Aetherflow</strong></see> <i>PvE</i> (SCH) [166] [Ability]
    /// </summary>
    static partial void ModifyAetherflowPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/166"><strong>Aetherflow</strong></see> <i>PvE</i> (SCH) [166] [Ability]
    /// <para>Restores 20% of maximum MP. Additional Effect: Aetherflow III Can only be executed while in combat.</para>
    /// </summary>
    
    public IBaseAction AetherflowPvE => _AetherflowPvECreator.Value;
    private readonly Lazy<IBaseAction> _EnergyDrainPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)167, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEnergyDrainPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/167"><strong>Energy Drain</strong></see> <i>PvE</i> (SCH) [167] [Ability]
    /// </summary>
    static partial void ModifyEnergyDrainPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/167"><strong>Energy Drain</strong></see> <i>PvE</i> (SCH) [167] [Ability]
    /// <para>Deals unaspected damage with a potency of 100. Additional Effect: Absorbs a portion of damage dealt as HP Aetherflow Gauge Cost: 1</para>
    /// </summary>
    
    public IBaseAction EnergyDrainPvE => _EnergyDrainPvECreator.Value;
    private readonly Lazy<IBaseAction> _ResurrectionPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)173, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyResurrectionPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/173"><strong>Resurrection</strong></see> <i>PvE</i> (ACN SMN SCH) [173] [Spell]
    /// </summary>
    static partial void ModifyResurrectionPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/173"><strong>Resurrection</strong></see> <i>PvE</i> (ACN SMN SCH) [173] [Spell]
    /// <para>Resurrects target to a weakened state.</para>
    /// </summary>
    
    public IBaseAction ResurrectionPvE => _ResurrectionPvECreator.Value;
    private readonly Lazy<IBaseAction> _AdloquiumPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)185, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyAdloquiumPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/185"><strong>Adloquium</strong></see> <i>PvE</i> (SCH) [185] [Spell]
    /// </summary>
    static partial void ModifyAdloquiumPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/185"><strong>Adloquium</strong></see> <i>PvE</i> (SCH) [185] [Spell]
    /// <para>Restores target's HP. Cure Potency: 300 Additional Effect: Grants Galvanize to target nullifying damage equaling % of the amount of HP restored. When critical HP is restored also grants Catalyze nullifying damage equaling % the amount of HP restored. Duration: 30s Effect cannot be stacked with certain sage barrier effects.</para>
    /// </summary>
    
    public IBaseAction AdloquiumPvE => _AdloquiumPvECreator.Value;
    private readonly Lazy<IBaseAction> _SuccorPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)186, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifySuccorPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/186"><strong>Succor</strong></see> <i>PvE</i> (SCH) [186] [Spell]
    /// </summary>
    static partial void ModifySuccorPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/186"><strong>Succor</strong></see> <i>PvE</i> (SCH) [186] [Spell]
    /// <para>Restores own HP and the HP of all nearby party members. Cure Potency: 200 Additional Effect: Erects a magicked barrier which nullifies damage equaling % of the amount of HP restored Duration: 30s Effect cannot be stacked with certain sage barrier effects.</para>
    /// </summary>
    
    public IBaseAction SuccorPvE => _SuccorPvECreator.Value;
    private readonly Lazy<IBaseAction> _SacredSoilPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)188, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifySacredSoilPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/188"><strong>Sacred Soil</strong></see> <i>PvE</i> (SCH) [188] [Ability]
    /// </summary>
    static partial void ModifySacredSoilPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/188"><strong>Sacred Soil</strong></see> <i>PvE</i> (SCH) [188] [Ability]
    /// <para>Creates a designated area in which party members will only suffer 90% of all damage inflicted. Duration: 15s Aetherflow Gauge Cost: 1</para>
    /// </summary>
    
    public IBaseAction SacredSoilPvE => _SacredSoilPvECreator.Value;
    private readonly Lazy<IBaseAction> _LustratePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)189, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyLustratePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/189"><strong>Lustrate</strong></see> <i>PvE</i> (SCH) [189] [Ability]
    /// </summary>
    static partial void ModifyLustratePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/189"><strong>Lustrate</strong></see> <i>PvE</i> (SCH) [189] [Ability]
    /// <para>Restores target's HP. Cure Potency: 600 Aetherflow Gauge Cost: 1</para>
    /// </summary>
    
    public IBaseAction LustratePvE => _LustratePvECreator.Value;
    private readonly Lazy<IBaseAction> _PhysickPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)190, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyPhysickPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/190"><strong>Physick</strong></see> <i>PvE</i> (SCH) [190] [Spell]
    /// </summary>
    static partial void ModifyPhysickPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/190"><strong>Physick</strong></see> <i>PvE</i> (SCH) [190] [Spell]
    /// <para>Restores target's HP. Cure Potency:</para>
    /// </summary>
    
    public IBaseAction PhysickPvE => _PhysickPvECreator.Value;
    private readonly Lazy<IBaseAction> _EmbracePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)802, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEmbracePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/802"><strong>Embrace</strong></see> <i>PvE</i> (SCH) [802] [Spell]
    /// </summary>
    static partial void ModifyEmbracePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/802"><strong>Embrace</strong></see> <i>PvE</i> (SCH) [802] [Spell]
    /// <para>Restores target's HP. Cure Potency: ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction EmbracePvE => _EmbracePvECreator.Value;
    private readonly Lazy<IBaseAction> _IndomitabilityPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3583, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyIndomitabilityPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3583"><strong>Indomitability</strong></see> <i>PvE</i> (SCH) [3583] [Ability]
    /// </summary>
    static partial void ModifyIndomitabilityPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3583"><strong>Indomitability</strong></see> <i>PvE</i> (SCH) [3583] [Ability]
    /// <para>Restores own HP and the HP of all nearby party members. Cure Potency: 400 Aetherflow Gauge Cost: 1</para>
    /// </summary>
    
    public IBaseAction IndomitabilityPvE => _IndomitabilityPvECreator.Value;
    private readonly Lazy<IBaseAction> _BroilPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3584, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBroilPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3584"><strong>Broil</strong></see> <i>PvE</i> (SCH) [3584] [Spell]
    /// </summary>
    static partial void ModifyBroilPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3584"><strong>Broil</strong></see> <i>PvE</i> (SCH) [3584] [Spell]
    /// <para>Deals unaspected damage with a potency of 220.</para>
    /// </summary>
    
    public IBaseAction BroilPvE => _BroilPvECreator.Value;
    private readonly Lazy<IBaseAction> _DeploymentTacticsPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3585, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyDeploymentTacticsPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3585"><strong>Deployment Tactics</strong></see> <i>PvE</i> (SCH) [3585] [Ability]
    /// </summary>
    static partial void ModifyDeploymentTacticsPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3585"><strong>Deployment Tactics</strong></see> <i>PvE</i> (SCH) [3585] [Ability]
    /// <para>Extends Galvanize effect cast on self or target party member to other nearby party members. Duration: Time remaining on original effect No effect when target is not under the effect of Galvanize.</para>
    /// </summary>
    
    public IBaseAction DeploymentTacticsPvE => _DeploymentTacticsPvECreator.Value;
    private readonly Lazy<IBaseAction> _EmergencyTacticsPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3586, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEmergencyTacticsPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3586"><strong>Emergency Tactics</strong></see> <i>PvE</i> (SCH) [3586] [Ability]
    /// </summary>
    static partial void ModifyEmergencyTacticsPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3586"><strong>Emergency Tactics</strong></see> <i>PvE</i> (SCH) [3586] [Ability]
    /// <para>Transforms the next Galvanize and Catalyze statuses into HP recovery equaling the amount of damage reduction intended for the barrier. Duration: 15s</para>
    /// </summary>
    
    public IBaseAction EmergencyTacticsPvE => _EmergencyTacticsPvECreator.Value;
    private readonly Lazy<IBaseAction> _DissipationPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3587, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyDissipationPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3587"><strong>Dissipation</strong></see> <i>PvE</i> (SCH) [3587] [Ability]
    /// </summary>
    static partial void ModifyDissipationPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3587"><strong>Dissipation</strong></see> <i>PvE</i> (SCH) [3587] [Ability]
    /// <para>Orders your faerie away while granting you a full Aetherflow stack. Also increases healing magic potency by 20%. Duration: 30s Current faerie will return once the effect expires. Summon Eos cannot be executed while under the effect of Dissipation. Can only be executed while a faerie is summoned and you are in combat.</para>
    /// </summary>
    
    public IBaseAction DissipationPvE => _DissipationPvECreator.Value;
    private readonly Lazy<IBaseAction> _ExcogitationPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7434, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyExcogitationPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7434"><strong>Excogitation</strong></see> <i>PvE</i> (SCH) [7434] [Ability]
    /// </summary>
    static partial void ModifyExcogitationPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7434"><strong>Excogitation</strong></see> <i>PvE</i> (SCH) [7434] [Ability]
    /// <para>Grants self or target party member the effect of Excogitation restoring HP when member's HP falls to 50% or below or upon effect duration expiration. Cure Potency: 800 Duration: 45s Aetherflow Gauge Cost: 1</para>
    /// </summary>
    
    public IBaseAction ExcogitationPvE => _ExcogitationPvECreator.Value;
    private readonly Lazy<IBaseAction> _BroilIiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7435, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBroilIiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7435"><strong>Broil II</strong></see> <i>PvE</i> (SCH) [7435] [Spell]
    /// </summary>
    static partial void ModifyBroilIiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7435"><strong>Broil II</strong></see> <i>PvE</i> (SCH) [7435] [Spell]
    /// <para>Deals unaspected damage with a potency of 240.</para>
    /// </summary>
    
    public IBaseAction BroilIiPvE => _BroilIiPvECreator.Value;
    private readonly Lazy<IBaseAction> _ChainStratagemPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7436, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyChainStratagemPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7436"><strong>Chain Stratagem</strong></see> <i>PvE</i> (SCH) [7436] [Ability]
    /// </summary>
    static partial void ModifyChainStratagemPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7436"><strong>Chain Stratagem</strong></see> <i>PvE</i> (SCH) [7436] [Ability]
    /// <para>Increases rate at which target takes critical hits by 10%. Duration: 20s</para>
    /// </summary>
    
    public IBaseAction ChainStratagemPvE => _ChainStratagemPvECreator.Value;
    private readonly Lazy<IBaseAction> _AetherpactPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7437, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyAetherpactPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7437"><strong>Aetherpact</strong></see> <i>PvE</i> (SCH) [7437] [Ability]
    /// </summary>
    static partial void ModifyAetherpactPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7437"><strong>Aetherpact</strong></see> <i>PvE</i> (SCH) [7437] [Ability]
    /// <para>Orders faerie to execute Fey Union with target party member. Effect ends upon reuse. Faerie Gauge Cost: 10 Fey Union Effect: Gradually restores HP of party member with which faerie has a Fey Union. Cure Potency: 300 Faerie Gauge is depleted by 10 periodically while HP is restored. Fey Union effect fades upon execution of other faerie actions or when party member moves from within 30 yalms of the faerie. The Faerie Gauge increases when is summoned and an Aetherflow action is successfully executed while in combat.</para>
    /// </summary>
    
    public IBaseAction AetherpactPvE => _AetherpactPvECreator.Value;
    private readonly Lazy<IBaseAction> _DissolveUnionPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7869, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyDissolveUnionPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7869"><strong>Dissolve Union</strong></see> <i>PvE</i> (SCH) [7869] [Ability]
    /// </summary>
    static partial void ModifyDissolveUnionPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7869"><strong>Dissolve Union</strong></see> <i>PvE</i> (SCH) [7869] [Ability]
    /// <para>Dissolves current Fey Union.</para>
    /// </summary>
    
    public IBaseAction DissolveUnionPvE => _DissolveUnionPvECreator.Value;
    private readonly Lazy<IBaseAction> _WhisperingDawnPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16537, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyWhisperingDawnPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16537"><strong>Whispering Dawn</strong></see> <i>PvE</i> (SCH) [16537] [Ability]
    /// </summary>
    static partial void ModifyWhisperingDawnPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16537"><strong>Whispering Dawn</strong></see> <i>PvE</i> (SCH) [16537] [Ability]
    /// <para>Orders faerie to execute Whispering Dawn. Whispering Dawn Effect: Gradually restores the HP of all nearby party members Cure Potency: 80 Duration: 21s</para>
    /// </summary>
    
    public IBaseAction WhisperingDawnPvE => _WhisperingDawnPvECreator.Value;
    private readonly Lazy<IBaseAction> _FeyIlluminationPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16538, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyFeyIlluminationPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16538"><strong>Fey Illumination</strong></see> <i>PvE</i> (SCH) [16538] [Ability]
    /// </summary>
    static partial void ModifyFeyIlluminationPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16538"><strong>Fey Illumination</strong></see> <i>PvE</i> (SCH) [16538] [Ability]
    /// <para>Orders faerie to execute Fey Illumination. Fey Illumination Effect: Increases healing magic potency of all nearby party members by 10% while reducing magic damage taken by all nearby party members by 5% Duration: 20s</para>
    /// </summary>
    
    public IBaseAction FeyIlluminationPvE => _FeyIlluminationPvECreator.Value;
    private readonly Lazy<IBaseAction> _ArtOfWarPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16539, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyArtOfWarPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16539"><strong>Art of War</strong></see> <i>PvE</i> (SCH) [16539] [Spell]
    /// </summary>
    static partial void ModifyArtOfWarPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16539"><strong>Art of War</strong></see> <i>PvE</i> (SCH) [16539] [Spell]
    /// <para>Deals unaspected damage with a potency of to all nearby enemies.</para>
    /// </summary>
    
    public IBaseAction ArtOfWarPvE => _ArtOfWarPvECreator.Value;
    private readonly Lazy<IBaseAction> _BiolysisPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16540, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBiolysisPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16540"><strong>Biolysis</strong></see> <i>PvE</i> (SCH) [16540] [Spell]
    /// </summary>
    static partial void ModifyBiolysisPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16540"><strong>Biolysis</strong></see> <i>PvE</i> (SCH) [16540] [Spell]
    /// <para>Deals unaspected damage over time. Potency: Duration: 30s</para>
    /// </summary>
    
    public IBaseAction BiolysisPvE => _BiolysisPvECreator.Value;
    private readonly Lazy<IBaseAction> _BroilIiiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16541, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBroilIiiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16541"><strong>Broil III</strong></see> <i>PvE</i> (SCH) [16541] [Spell]
    /// </summary>
    static partial void ModifyBroilIiiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16541"><strong>Broil III</strong></see> <i>PvE</i> (SCH) [16541] [Spell]
    /// <para>Deals unaspected damage with a potency of 255.</para>
    /// </summary>
    
    public IBaseAction BroilIiiPvE => _BroilIiiPvECreator.Value;
    private readonly Lazy<IBaseAction> _RecitationPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16542, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRecitationPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16542"><strong>Recitation</strong></see> <i>PvE</i> (SCH) [16542] [Ability]
    /// </summary>
    static partial void ModifyRecitationPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16542"><strong>Recitation</strong></see> <i>PvE</i> (SCH) [16542] [Ability]
    /// <para>Allows the execution of Adloquium Indomitability or Excogitation without consuming resources while also ensuring critical HP is restored. Duration: 15s</para>
    /// </summary>
    
    public IBaseAction RecitationPvE => _RecitationPvECreator.Value;
    private readonly Lazy<IBaseAction> _FeyBlessingPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16543, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyFeyBlessingPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16543"><strong>Fey Blessing</strong></see> <i>PvE</i> (SCH) [16543] [Ability]
    /// </summary>
    static partial void ModifyFeyBlessingPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16543"><strong>Fey Blessing</strong></see> <i>PvE</i> (SCH) [16543] [Ability]
    /// <para>Orders faerie to execute Fey Blessing. Fey Blessing Effect: Restores the HP of all nearby party members Cure Potency: 320</para>
    /// </summary>
    
    public IBaseAction FeyBlessingPvE => _FeyBlessingPvECreator.Value;
    private readonly Lazy<IBaseAction> _SummonSeraphPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16545, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifySummonSeraphPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16545"><strong>Summon Seraph</strong></see> <i>PvE</i> (SCH) [16545] [Ability]
    /// </summary>
    static partial void ModifySummonSeraphPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16545"><strong>Summon Seraph</strong></see> <i>PvE</i> (SCH) [16545] [Ability]
    /// <para>Summons Seraph to fight at your side. When set to guard automatically casts Seraphic Veil on party members who suffer damage. Cannot summon Seraph unless a pet is already summoned. Current pet will leave the battlefield while Seraph is present and return once gone. Duration: 22s</para>
    /// </summary>
    
    public IBaseAction SummonSeraphPvE => _SummonSeraphPvECreator.Value;
    private readonly Lazy<IBaseAction> _ConsolationPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16546, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyConsolationPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16546"><strong>Consolation</strong></see> <i>PvE</i> (SCH) [16546] [Ability]
    /// </summary>
    static partial void ModifyConsolationPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16546"><strong>Consolation</strong></see> <i>PvE</i> (SCH) [16546] [Ability]
    /// <para>Orders Seraph to execute Consolation. Consolation Effect: Restores the HP of all nearby party members Cure Potency: 250 Additional Effect: Erects a magicked barrier which nullifies damage equaling the amount of HP restored Duration: 30s Maximum Charges: 2</para>
    /// </summary>
    
    public IBaseAction ConsolationPvE => _ConsolationPvECreator.Value;
    private readonly Lazy<IBaseAction> _SeraphicVeilPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16548, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifySeraphicVeilPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16548"><strong>Seraphic Veil</strong></see> <i>PvE</i> (SCH) [16548] [Spell]
    /// </summary>
    static partial void ModifySeraphicVeilPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16548"><strong>Seraphic Veil</strong></see> <i>PvE</i> (SCH) [16548] [Spell]
    /// <para>Restores target's HP. Cure Potency: Additional Effect: Erects a magicked barrier which nullifies damage equaling the amount of HP restored Duration: 30s ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction SeraphicVeilPvE => _SeraphicVeilPvECreator.Value;
    private readonly Lazy<IBaseAction> _SummonEosPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)17215, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifySummonEosPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/17215"><strong>Summon Eos</strong></see> <i>PvE</i> (SCH) [17215] [Spell]
    /// </summary>
    static partial void ModifySummonEosPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/17215"><strong>Summon Eos</strong></see> <i>PvE</i> (SCH) [17215] [Spell]
    /// <para>Summons the faerie Eos to fight at your side. When set to guard automatically casts Embrace on party members who suffer damage.</para>
    /// </summary>
    
    public IBaseAction SummonEosPvE => _SummonEosPvECreator.Value;
    private readonly Lazy<IBaseAction> _BioPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)17864, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBioPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/17864"><strong>Bio</strong></see> <i>PvE</i> (SCH) [17864] [Spell]
    /// </summary>
    static partial void ModifyBioPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/17864"><strong>Bio</strong></see> <i>PvE</i> (SCH) [17864] [Spell]
    /// <para>Deals unaspected damage over time. Potency: 20 Duration: 30s</para>
    /// </summary>
    
    public IBaseAction BioPvE => _BioPvECreator.Value;
    private readonly Lazy<IBaseAction> _BioIiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)17865, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBioIiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/17865"><strong>Bio II</strong></see> <i>PvE</i> (SCH) [17865] [Spell]
    /// </summary>
    static partial void ModifyBioIiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/17865"><strong>Bio II</strong></see> <i>PvE</i> (SCH) [17865] [Spell]
    /// <para>Deals unaspected damage over time. Potency: 40 Duration: 30s</para>
    /// </summary>
    
    public IBaseAction BioIiPvE => _BioIiPvECreator.Value;
    private readonly Lazy<IBaseAction> _RuinPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)17869, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRuinPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/17869"><strong>Ruin</strong></see> <i>PvE</i> (SCH) [17869] [Spell]
    /// </summary>
    static partial void ModifyRuinPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/17869"><strong>Ruin</strong></see> <i>PvE</i> (SCH) [17869] [Spell]
    /// <para>Deals unaspected damage with a potency of 150.</para>
    /// </summary>
    
    public IBaseAction RuinPvE => _RuinPvECreator.Value;
    private readonly Lazy<IBaseAction> _RuinIiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)17870, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRuinIiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/17870"><strong>Ruin II</strong></see> <i>PvE</i> (SCH) [17870] [Spell]
    /// </summary>
    static partial void ModifyRuinIiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/17870"><strong>Ruin II</strong></see> <i>PvE</i> (SCH) [17870] [Spell]
    /// <para>Deals unaspected damage with a potency of .</para>
    /// </summary>
    
    public IBaseAction RuinIiPvE => _RuinIiPvECreator.Value;
    private readonly Lazy<IBaseAction> _BroilIvPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25865, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBroilIvPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25865"><strong>Broil IV</strong></see> <i>PvE</i> (SCH) [25865] [Spell]
    /// </summary>
    static partial void ModifyBroilIvPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25865"><strong>Broil IV</strong></see> <i>PvE</i> (SCH) [25865] [Spell]
    /// <para>Deals unaspected damage with a potency of .</para>
    /// </summary>
    
    public IBaseAction BroilIvPvE => _BroilIvPvECreator.Value;
    private readonly Lazy<IBaseAction> _ArtOfWarIiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25866, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyArtOfWarIiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25866"><strong>Art of War II</strong></see> <i>PvE</i> (SCH) [25866] [Spell]
    /// </summary>
    static partial void ModifyArtOfWarIiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25866"><strong>Art of War II</strong></see> <i>PvE</i> (SCH) [25866] [Spell]
    /// <para>Deals unaspected damage with a potency of 180 to all nearby enemies.</para>
    /// </summary>
    
    public IBaseAction ArtOfWarIiPvE => _ArtOfWarIiPvECreator.Value;
    private readonly Lazy<IBaseAction> _ProtractionPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25867, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyProtractionPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25867"><strong>Protraction</strong></see> <i>PvE</i> (SCH) [25867] [Ability]
    /// </summary>
    static partial void ModifyProtractionPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25867"><strong>Protraction</strong></see> <i>PvE</i> (SCH) [25867] [Ability]
    /// <para>Increases maximum HP of a party member or self by 10% and restores the amount increased. Additional Effect: Increases HP recovery via healing actions by 10% Duration: 10s</para>
    /// </summary>
    
    public IBaseAction ProtractionPvE => _ProtractionPvECreator.Value;
    private readonly Lazy<IBaseAction> _ExpedientPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25868, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyExpedientPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25868"><strong>Expedient</strong></see> <i>PvE</i> (SCH) [25868] [Ability]
    /// </summary>
    static partial void ModifyExpedientPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25868"><strong>Expedient</strong></see> <i>PvE</i> (SCH) [25868] [Ability]
    /// <para>Grants Expedience and Desperate Measures to all nearby party members. Expedience Effect: Increases movement speed Duration: 10s Desperate Measures Effect: Reduces damage taken by 10% Duration: 20s</para>
    /// </summary>
    
    public IBaseAction ExpedientPvE => _ExpedientPvECreator.Value;
    private readonly Lazy<IBaseAction> _BroilIvPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29231, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBroilIvPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29231"><strong>Broil IV</strong></see> <i>PvP</i> (SCH) [29231] [Spell]
    /// </summary>
    static partial void ModifyBroilIvPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29231"><strong>Broil IV</strong></see> <i>PvP</i> (SCH) [29231] [Spell]
    /// <para>Deals unaspected damage with a potency of 6,000.</para>
    /// </summary>
    
    public IBaseAction BroilIvPvP => _BroilIvPvPCreator.Value;
    private readonly Lazy<IBaseAction> _AdloquiumPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29232, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyAdloquiumPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29232"><strong>Adloquium</strong></see> <i>PvP</i> (SCH) [29232] [Spell]
    /// </summary>
    static partial void ModifyAdloquiumPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29232"><strong>Adloquium</strong></see> <i>PvP</i> (SCH) [29232] [Spell]
    /// <para>Restores target's HP. Cure Potency: 4,000 Additional Effect: Grants Galvanize and Catalyze to target Galvanize Effect: Absorbs damage equivalent to a heal of 4,000 potency Duration: 12s Barrier potency is doubled when you are under the effect of Recitation. Catalyze Effect: Reduces damage taken by 10% Duration: 12s Maximum Charges: 2 This action does not share a recast timer with any other actions.</para>
    /// </summary>
    
    public IBaseAction AdloquiumPvP => _AdloquiumPvPCreator.Value;
    private readonly Lazy<IBaseAction> _BiolysisPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29233, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBiolysisPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29233"><strong>Biolysis</strong></see> <i>PvP</i> (SCH) [29233] [Spell]
    /// </summary>
    static partial void ModifyBiolysisPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29233"><strong>Biolysis</strong></see> <i>PvP</i> (SCH) [29233] [Spell]
    /// <para>Afflicts target with Biolysis and Biolytic. Biolysis Effect: Damage over time Potency: 3,000 Duration: 12s Potency is increased by 50% when you are under the effect of Recitation. Biolytic Effect: Reduces target's HP recovered by healing actions by 15% Duration: 12s This action does not share a recast timer with any other actions.</para>
    /// </summary>
    
    public IBaseAction BiolysisPvP => _BiolysisPvPCreator.Value;
    private readonly Lazy<IBaseAction> _DeploymentTacticsPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29234, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyDeploymentTacticsPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29234"><strong>Deployment Tactics</strong></see> <i>PvP</i> (SCH) [29234] [Ability]
    /// </summary>
    static partial void ModifyDeploymentTacticsPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29234"><strong>Deployment Tactics</strong></see> <i>PvP</i> (SCH) [29234] [Ability]
    /// <para>Extends Galvanize and Catalyze effects cast on self or target to nearby party members. When targeting an enemy extends Biolysis and Biolytic effects to other nearby enemies. Duration: Time remaining on original effect No effect on targets not under effects applied by you players riding machina or non-player combatants. Maximum Charges: 2</para>
    /// </summary>
    
    public IBaseAction DeploymentTacticsPvP => _DeploymentTacticsPvPCreator.Value;
    private readonly Lazy<IBaseAction> _ExpedientPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29236, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyExpedientPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29236"><strong>Expedient</strong></see> <i>PvP</i> (SCH) [29236] [Ability]
    /// </summary>
    static partial void ModifyExpedientPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29236"><strong>Expedient</strong></see> <i>PvP</i> (SCH) [29236] [Ability]
    /// <para>Grants Expedience and Desperate Measures to self and all nearby party members. Expedience Effect: Increases movement speed by 25% Duration: 10s Desperate Measures Effect: Increases damage dealt by 10% Duration: 10s Additional Effect: Grants Recitation Recitation Effect: Increases the potency of Galvanize Biolysis and Biolytic effects Duration: 20s This action's effects extend through obstructions.</para>
    /// </summary>
    
    public IBaseAction ExpedientPvP => _ExpedientPvPCreator.Value;
    private readonly Lazy<IBaseAction> _SummonSeraphPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29237, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifySummonSeraphPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29237"><strong>Summon Seraph</strong></see> <i>PvP</i> (SCH) [29237] [Ability]
    /// </summary>
    static partial void ModifySummonSeraphPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29237"><strong>Summon Seraph</strong></see> <i>PvP</i> (SCH) [29237] [Ability]
    /// <para>Summons Seraph to fight at a designated location. Duration: 20s Automatically casts Seraphic Veil on party members within 30 yalms. Automatically grants Seraphic Illumination to party members within 15 yalms. Seraphic Illumination Effect: Increases HP recovery via healing actions on self by 10%</para>
    /// </summary>
    
    public IBaseAction SummonSeraphPvP => _SummonSeraphPvPCreator.Value;
    private readonly Lazy<IBaseAction> _SeraphicVeilPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29240, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifySeraphicVeilPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29240"><strong>Seraphic Veil</strong></see> <i>PvP</i> (SCH) [29240] [Spell]
    /// </summary>
    static partial void ModifySeraphicVeilPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29240"><strong>Seraphic Veil</strong></see> <i>PvP</i> (SCH) [29240] [Spell]
    /// <para>Restores target's HP. Cure Potency: 6,000 Additional Effect: Erects a magicked barrier which nullifies damage equivalent to a heal of 6,000 potency Duration: 10s ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction SeraphicVeilPvP => _SeraphicVeilPvPCreator.Value;
    private readonly Lazy<IBaseAction> _ChainStratagemPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29716, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyChainStratagemPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29716"><strong>Chain Stratagem</strong></see> <i>PvP</i> (SCH) [29716] [Ability]
    /// </summary>
    static partial void ModifyChainStratagemPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29716"><strong>Chain Stratagem</strong></see> <i>PvP</i> (SCH) [29716] [Ability]
    /// <para>Increases target's damage taken by 10%. Duration: 10s Halves the defensive bonus of Guard instead when targeting enemies under its effect.</para>
    /// </summary>
    
    public IBaseAction ChainStratagemPvP => _ChainStratagemPvPCreator.Value;
    private readonly Lazy<IBaseAction> _BanefulImpactionPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)37012, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBanefulImpactionPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/37012"><strong>Baneful Impaction</strong></see> <i>PvE</i> (SCH) [37012] [Ability]
    /// </summary>
    static partial void ModifyBanefulImpactionPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/37012"><strong>Baneful Impaction</strong></see> <i>PvE</i> (SCH) [37012] [Ability]
    /// <para>Deals unaspected damage over time to target and all enemies nearby it. Potency: 140 Duration: 15s Can only be executed while under the effect of Impact Imminent.</para>
    /// </summary>
    
    public IBaseAction BanefulImpactionPvE => _BanefulImpactionPvECreator.Value;
    private readonly Lazy<IBaseAction> _ConcitationPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)37013, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyConcitationPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/37013"><strong>Concitation</strong></see> <i>PvE</i> (SCH) [37013] [Spell]
    /// </summary>
    static partial void ModifyConcitationPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/37013"><strong>Concitation</strong></see> <i>PvE</i> (SCH) [37013] [Spell]
    /// <para>Restores own HP and the HP of all nearby party members. Cure Potency: 200 Additional Effect: Erects a magicked barrier which nullifies damage equaling 180% of the amount of HP restored Duration: 30s Effect cannot be stacked with certain sage barrier effects.</para>
    /// </summary>
    
    public IBaseAction ConcitationPvE => _ConcitationPvECreator.Value;
    private readonly Lazy<IBaseAction> _SeraphismPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)37014, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifySeraphismPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/37014"><strong>Seraphism</strong></see> <i>PvE</i> (SCH) [37014] [Ability]
    /// </summary>
    static partial void ModifySeraphismPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/37014"><strong>Seraphism</strong></see> <i>PvE</i> (SCH) [37014] [Ability]
    /// <para>Gradually restores the HP of self and all party members within a radius of 50 yalms. Cure Potency: 100 Additional Effect: Changes Adloquium to Manifestation and Concitation to Accession Additional Effect: Resets Emergency Tactics recast timer and reduces its recast timer to 1s Duration: 20s Effect cannot be stacked with Dissipation. Can only be executed while a faerie or Seraph is summoned and you are in combat.</para>
    /// </summary>
    
    public IBaseAction SeraphismPvE => _SeraphismPvECreator.Value;
    private readonly Lazy<IBaseAction> _ManifestationPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)37015, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyManifestationPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/37015"><strong>Manifestation</strong></see> <i>PvE</i> (SCH) [37015] [Spell]
    /// </summary>
    static partial void ModifyManifestationPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/37015"><strong>Manifestation</strong></see> <i>PvE</i> (SCH) [37015] [Spell]
    /// <para>Restores target's HP. Cure Potency: 360 Additional Effect: Grants Galvanize to target nullifying damage equaling 180% of the amount of HP restored. When critical HP is restored also grants Catalyze nullifying damage equaling 180% the amount of HP restored. Duration: 30s Effect cannot be stacked with certain sage barrier effects. Can only be executed while under the effect of Seraphism. ※This action cannot be assigned to a hotbar. ※Adloquium changes to Manifestation when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction ManifestationPvE => _ManifestationPvECreator.Value;
    private readonly Lazy<IBaseAction> _AccessionPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)37016, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyAccessionPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/37016"><strong>Accession</strong></see> <i>PvE</i> (SCH) [37016] [Spell]
    /// </summary>
    static partial void ModifyAccessionPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/37016"><strong>Accession</strong></see> <i>PvE</i> (SCH) [37016] [Spell]
    /// <para>Restores own HP and the HP of all nearby party members. Cure Potency: 240 Additional Effect: Erects a magicked barrier which nullifies damage equaling 180% of the amount of HP restored Duration: 30s Effect cannot be stacked with certain sage barrier effects. Can only be executed while under the effect of Seraphism. ※This action cannot be assigned to a hotbar. ※Concitation changes to Accession when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction AccessionPvE => _AccessionPvECreator.Value;
    private readonly Lazy<IBaseAction> _EmergencyTacticsPvE_37037Creator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)37037, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyEmergencyTacticsPvE_37037(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/37037"><strong>Emergency Tactics</strong></see> <i>PvE</i> (SCH) [37037] [Ability]
    /// </summary>
    static partial void ModifyEmergencyTacticsPvE_37037(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/37037"><strong>Emergency Tactics</strong></see> <i>PvE</i> (SCH) [37037] [Ability]
    /// <para>Transforms the next Galvanize and Catalyze statuses into HP recovery equaling the amount of damage reduction intended for the barrier. Duration: 15s</para>
    /// </summary>
    
    public IBaseAction EmergencyTacticsPvE_37037 => _EmergencyTacticsPvE_37037Creator.Value;
    private readonly Lazy<IBaseAction> _SeraphicHaloPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41500, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifySeraphicHaloPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41500"><strong>Seraphic Halo</strong></see> <i>PvP</i> (SCH) [41500] [Spell]
    /// </summary>
    static partial void ModifySeraphicHaloPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41500"><strong>Seraphic Halo</strong></see> <i>PvP</i> (SCH) [41500] [Spell]
    /// <para>Deals unaspected damage with a potency of 9,000 to target and all enemies nearby it. Can only be executed while under the effect of Seraphism. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction SeraphicHaloPvP => _SeraphicHaloPvPCreator.Value;
    private readonly Lazy<IBaseAction> _AccessionPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41501, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyAccessionPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41501"><strong>Accession</strong></see> <i>PvP</i> (SCH) [41501] [Spell]
    /// </summary>
    static partial void ModifyAccessionPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41501"><strong>Accession</strong></see> <i>PvP</i> (SCH) [41501] [Spell]
    /// <para>Restores own HP and the HP of all nearby party members. Cure Potency: 8,000 Additional Effect: Grants Consolation Consolation Effect: Creates a barrier that absorbs damage equivalent to a heal of 8,000 potency Duration: 20s This action's effects extend through obstructions. Can only be executed while under the effect of Seraphism. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction AccessionPvP => _AccessionPvPCreator.Value;

    private IBaseAction[] _AllBaseActions = null;
    
    /// <inheritdoc/>
    public override IBaseAction[] AllBaseActions => _AllBaseActions ??= [
    	AetherflowPvE, EnergyDrainPvE, ResurrectionPvE, AdloquiumPvE, SuccorPvE, SacredSoilPvE, LustratePvE, PhysickPvE, EmbracePvE, IndomitabilityPvE, BroilPvE, DeploymentTacticsPvE, EmergencyTacticsPvE, DissipationPvE, ExcogitationPvE, BroilIiPvE, ChainStratagemPvE, AetherpactPvE, DissolveUnionPvE, WhisperingDawnPvE, FeyIlluminationPvE, ArtOfWarPvE, BiolysisPvE, BroilIiiPvE, RecitationPvE, FeyBlessingPvE, SummonSeraphPvE, ConsolationPvE, SeraphicVeilPvE, SummonEosPvE, BioPvE, BioIiPvE, RuinPvE, RuinIiPvE, BroilIvPvE, ArtOfWarIiPvE, ProtractionPvE, ExpedientPvE, BroilIvPvP, AdloquiumPvP, BiolysisPvP, DeploymentTacticsPvP, ExpedientPvP, SummonSeraphPvP, SeraphicVeilPvP, ChainStratagemPvP, BanefulImpactionPvE, ConcitationPvE, SeraphismPvE, ManifestationPvE, AccessionPvE, SeraphicHaloPvP, AccessionPvP,
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
private readonly Lazy<IBaseAction> _AngelFeathersPvECreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)4247, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifyAngelFeathersPvE(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/4247"><strong>Angel Feathers</strong></see> <i>PvE</i> (All Classes) [4247] [Limit Break]
/// </summary>
static partial void ModifyAngelFeathersPvE(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/4247"><strong>Angel Feathers</strong></see> <i>PvE</i> (All Classes) [4247] [Limit Break]
/// <para></para>
/// </summary>

public IBaseAction AngelFeathersPvE => _AngelFeathersPvECreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/4247"><strong>Angel Feathers</strong></see> <i>PvE</i> (All Classes) [4247] [Limit Break]
/// <para></para>
/// </summary>
private sealed protected override IBaseAction LimitBreak3 => AngelFeathersPvE;
private readonly Lazy<IBaseAction> _SeraphismPvPCreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)41502, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifySeraphismPvP(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/41502"><strong>Seraphism</strong></see> <i>PvP</i> (SCH) [41502] [Limit Break]
/// </summary>
static partial void ModifySeraphismPvP(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/41502"><strong>Seraphism</strong></see> <i>PvP</i> (SCH) [41502] [Limit Break]
/// <para>Grants Seraphism and Recitation. Seraphism Effect: Broil IV is upgraded to Seraphic Halo and Seraphism is upgraded to Accession Duration: 20s Recitation Effect: Increases the potency of Adloquium and Biolysis effects Duration: 20s Additional Effect: Removes one status affliction from self and nearby party members that can be removed by Purify. If a status affliction cannot be removed creates a barrier that will nullify the next status affliction that can be removed by Purify Duration: 20s Can only be executed when the limit gauge is full. Gauge Charge Time: 90s This action's effects extend through obstructions. ※Action changes to Accession upon execution.</para>
/// </summary>

private IBaseAction SeraphismPvP => _SeraphismPvPCreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/41502"><strong>Seraphism</strong></see> <i>PvP</i> (SCH) [41502] [Limit Break]
/// <para>Grants Seraphism and Recitation. Seraphism Effect: Broil IV is upgraded to Seraphic Halo and Seraphism is upgraded to Accession Duration: 20s Recitation Effect: Increases the potency of Adloquium and Biolysis effects Duration: 20s Additional Effect: Removes one status affliction from self and nearby party members that can be removed by Purify. If a status affliction cannot be removed creates a barrier that will nullify the next status affliction that can be removed by Purify Duration: 20s Can only be executed when the limit gauge is full. Gauge Charge Time: 90s This action's effects extend through obstructions. ※Action changes to Accession upon execution.</para>
/// </summary>
private sealed protected override IBaseAction LimitBreakPvP => SeraphismPvP;

#endregion

#region Traits

    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/50184"><strong>Broil Mastery II) | Broil II |
| Concitation | Knopf wird zu | Accession |
| Dissipation | braucht (Erzeuger nicht im Text) | a faerie is summoned |
| Dissipation | braucht (Erzeuger nicht im Text) | you are in combat |
| Energy Drain | kostet | HP Aetherflow |
| Excogitation | kostet | Aetherflow |
| Indomitability | kostet | Aetherflow |
| Lustrate | kostet | Aetherflow |
| Manifestation | braucht Seraphism | Seraphism |
| Ruin | Ausbau (Broil Mastery) | Broil |
| Sacred Soil | kostet | Aetherflow |
| Seraphism | braucht (Erzeuger nicht im Text) | Seraph is summoned |
| Seraphism | braucht (Erzeuger nicht im Text) | a faerie |
| Seraphism | braucht (Erzeuger nicht im Text) | you are in combat |
| Succor | Ausbau (Succor Mastery) | Concitation |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Adloquium | Regel sperrt vorher | Summon Eos |
| Aetherpact | Regel sperrt vorher | Excogitation |
| Art of War | Regel prüft | Broil III |
| Art of War | Regel prüft | Broil IV |
| Baneful Impaction | StatusNeed ImpactImminent | Status ImpactImminent |
| Bio II | Regel prüft | Art of War |
| Bio II | Regel prüft | Broil III |
| Bio | Regel prüft | Bio II |
| Biolysis | Regel prüft | Broil IV |
| Broil II | Regel prüft | Broil III |
| Broil III | Regel prüft | Broil IV |
| Deployment Tactics | Regel prüft | Recitation |
| Deployment Tactics | Regel sperrt vorher | Summon Eos |
| Dissipation | Regel prüft | Aetherflow |
| Dissipation | Regel prüft | Fey Blessing |
| Dissipation | Regel prüft | Summon Seraph |
| Dissipation | Regel prüft | Whispering Dawn |
| Emergency Tactics | Regel prüft | Accession |
| Emergency Tactics | Regel prüft | Concitation |
| Emergency Tactics | Regel prüft | Succor |
| Energy Drain | Regel prüft | Aetherflow |
| Energy Drain | Regel prüft | Dissipation |
| Excogitation | Regel prüft | Recitation |
| Fey Blessing | Regel sperrt vorher | Excogitation |
| Lustrate | Regel sperrt vorher | Excogitation |
| Recitation | Regel sperrt vorher | Summon Eos |
| Ruin | Regel prüft | Art of War |
| Ruin | Regel prüft | Bio II |
| Ruin | Regel sperrt vorher | Summon Eos |
| Seraphism | Regel prüft | Summon Seraph |
| Summon Seraph | Regel prüft | Fey Blessing |
| Summon Seraph | Regel prüft | Whispering Dawn |
| Whispering Dawn | Regel sperrt vorher | Excogitation |

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `AngelFeathersPvE`, `BreathOfTheEarthPvE`, `HealingWindPvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Dissolve Union (`DissolveUnionPvE`, Ability): ungenutzt
- Embrace (`EmbracePvE`, Spell): ungenutzt — nicht zuweisbar: Begleiter oder Automatik
- Repose (`ReposePvE`, Spell): ungenutzt
- Rescue (`RescuePvE`, Ability): ungenutzt
- Seraphic Veil (`SeraphicVeilPvE`, Spell): ungenutzt — nicht zuweisbar: Begleiter oder Automatik

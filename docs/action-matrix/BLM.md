# BLM — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `BlackMageRotation`
- Rotation: `RotationSolver/RebornRotations/Magical/BLM_Default.cs`, `RotationSolver/RebornRotations/Magical/BLM_RP.cs`
- Matrix als Tabelle: `BLM.csv`

## Nutzung

direkt: 37 · ungenutzt: 1 · über anderen Knopf: 2

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Magier | Addle (`AddlePvE`) | 7560 | Ability | direkt |
| Magier | Lucid Dreaming (`LucidDreamingPvE`) | 7562 | Ability | direkt |
| Magier | Sleep (`SleepPvE`) | 25880 | Spell | ungenutzt |
| Magier | Surecast (`SurecastPvE`) | 7559 | Ability | direkt |
| Magier | Swiftcast (`SwiftcastPvE`) | 7561 | Ability | direkt |
| Job | Aetherial Manipulation (`AetherialManipulationPvE`) | 155 | Ability | direkt |
| Job | Amplifier (`AmplifierPvE`) | 25796 | Ability | direkt |
| Job | Between the Lines (`BetweenTheLinesPvE`) | 7419 | Ability | direkt |
| Job | Blizzard (`BlizzardPvE`) | 142 | Spell | direkt |
| Job | Blizzard II (`BlizzardIiPvE`) | 25793 | Spell | direkt |
| Job | Blizzard III (`BlizzardIiiPvE`) | 154 | Spell | direkt |
| Job | Blizzard IV (`BlizzardIvPvE`) | 3576 | Spell | direkt |
| Job | Despair (`DespairPvE`) | 16505 | Spell | direkt |
| Job | Fire (`FirePvE`) | 141 | Spell | direkt |
| Job | Fire II (`FireIiPvE`) | 147 | Spell | direkt |
| Job | Fire III (`FireIiiPvE`) | 152 | Spell | direkt |
| Job | Fire IV (`FireIvPvE`) | 3577 | Spell | direkt |
| Job | Flare (`FlarePvE`) | 162 | Spell | direkt |
| Job | Flare Star (`FlareStarPvE`) | 36989 | Spell | direkt |
| Job | Foul (`FoulPvE`) | 7422 | Spell | direkt |
| Job | Freeze (`FreezePvE`) | 159 | Spell | direkt |
| Job | High Blizzard II (`HighBlizzardIiPvE`) | 25795 | Spell | über Blizzard II |
| Job | High Fire II (`HighFireIiPvE`) | 25794 | Spell | über Fire II |
| Job | High Thunder (`HighThunderPvE`) | 36986 | Spell | direkt |
| Job | High Thunder II (`HighThunderIiPvE`) | 36987 | Spell | direkt |
| Job | Ley Lines (`LeyLinesPvE`) | 3573 | Ability | direkt |
| Job | Manafont (`ManafontPvE`) | 158 | Ability | direkt |
| Job | Manaward (`ManawardPvE`) | 157 | Ability | direkt |
| Job | Paradox (`ParadoxPvE`) | 25797 | Spell | direkt |
| Job | Retrace (`RetracePvE`) | 36988 | Ability | direkt |
| Job | Scathe (`ScathePvE`) | 156 | Spell | direkt |
| Job | Thunder (`ThunderPvE`) | 144 | Spell | direkt |
| Job | Thunder II (`ThunderIiPvE`) | 7447 | Spell | direkt |
| Job | Thunder III (`ThunderIiiPvE`) | 153 | Spell | direkt |
| Job | Thunder IV (`ThunderIvPvE`) | 7420 | Spell | direkt |
| Job | Transpose (`TransposePvE`) | 149 | Ability | direkt |
| Job | Triplecast (`TriplecastPvE`) | 7421 | Ability | direkt |
| Job | Umbral Soul (`UmbralSoulPvE`) | 16506 | Spell | direkt |
| Job | Xenoglossy (`XenoglossyPvE`) | 16507 | Spell | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Amplifier | braucht Umbral Ice | Blizzard |
| Amplifier | braucht Astral Fire | Fire |
| Blizzard II | Ausbau (Aspect Mastery IV) | High Blizzard II |
| Blizzard IV | braucht Umbral Ice | Blizzard |
| Blizzard | Knopf wird zu | Paradox |
| Despair | braucht Astral Fire | Fire |
| Fire II | Ausbau (Aspect Mastery IV) | High Fire II |
| Fire IV | braucht Astral Fire | Fire |
| Fire | Knopf wird zu | Paradox |
| Flare | braucht Astral Fire | Fire |
| Flare Star | braucht (Erzeuger nicht im Text) | the Astral Gauge is full |
| Foul | kostet | Polyglot |
| Freeze | braucht Umbral Ice | Blizzard |
| Manafont | braucht Astral Fire | Fire |
| Paradox | braucht Paradox | Paradox |
| Retrace | braucht Ley Lines | Ley Lines |
| Retrace | braucht (Erzeuger nicht im Text) | the effect duration will not be reset |
| Thunder II | Ausbau (Thunder Mastery II) | Thunder IV |
| Thunder III | Ausbau (Thunder Mastery III) | High Thunder |
| Thunder IV | Ausbau (Thunder Mastery III) | High Thunder II |
| Thunder | Ausbau (Fire</strong></see> <i>PvE</i> (THM BLM) [141] [Spell]
    /// </summary>
    static partial void ModifyFirePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/141"><strong>Fire</strong></see> <i>PvE</i> (THM BLM) [141] [Spell]
    /// <para>Deals fire damage with a potency of 180. Additional Effect: Grants Astral Fire or removes Umbral Ice</para>
    /// </summary>
    
    public IBaseAction FirePvE => _FirePvECreator.Value;
    private readonly Lazy<IBaseAction> _BlizzardPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)142, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBlizzardPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/142"><strong>Blizzard</strong></see> <i>PvE</i> (THM BLM) [142] [Spell]
    /// </summary>
    static partial void ModifyBlizzardPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/142"><strong>Blizzard</strong></see> <i>PvE</i> (THM BLM) [142] [Spell]
    /// <para>Deals ice damage with a potency of 180. Additional Effect: Grants Umbral Ice or removes Astral Fire</para>
    /// </summary>
    
    public IBaseAction BlizzardPvE => _BlizzardPvECreator.Value;
    private readonly Lazy<IBaseAction> _ThunderPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)144, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyThunderPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/144"><strong>Thunder</strong></see> <i>PvE</i> (THM BLM) [144] [Spell]
    /// </summary>
    static partial void ModifyThunderPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/144"><strong>Thunder</strong></see> <i>PvE</i> (THM BLM) [144] [Spell]
    /// <para>Deals lightning damage with a potency of 100. Additional Effect: Lightning damage over time Potency: 45 Duration: 24s Can only be cast while under the effect of Thunderhead granted when gaining Astral Fire or Umbral Ice from an unaspected state or changing between their influences. Only one Thunder spell-induced damage over time effect per caster can be inflicted upon a single target.</para>
    /// </summary>
    
    public IBaseAction ThunderPvE => _ThunderPvECreator.Value;
    private readonly Lazy<IBaseAction> _FireIiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)147, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyFireIiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/147"><strong>Fire II</strong></see> <i>PvE</i> (THM BLM) [147] [Spell]
    /// </summary>
    static partial void ModifyFireIiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/147"><strong>Fire II</strong></see> <i>PvE</i> (THM BLM) [147] [Spell]
    /// <para>Deals fire damage with a potency of 80 to target and all enemies nearby it. Additional Effect:</para>
    /// </summary>
    
    public IBaseAction FireIiPvE => _FireIiPvECreator.Value;
    private readonly Lazy<IBaseAction> _TransposePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)149, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyTransposePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/149"><strong>Transpose</strong></see> <i>PvE</i> (THM BLM) [149] [Ability]
    /// </summary>
    static partial void ModifyTransposePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/149"><strong>Transpose</strong></see> <i>PvE</i> (THM BLM) [149] [Ability]
    /// <para>Swaps Astral Fire with a single Umbral Ice or Umbral Ice with a single Astral Fire.</para>
    /// </summary>
    
    public IBaseAction TransposePvE => _TransposePvECreator.Value;
    private readonly Lazy<IBaseAction> _FireIiiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)152, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyFireIiiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/152"><strong>Fire III</strong></see> <i>PvE</i> (THM BLM) [152] [Spell]
    /// </summary>
    static partial void ModifyFireIiiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/152"><strong>Fire III</strong></see> <i>PvE</i> (THM BLM) [152] [Spell]
    /// <para>Deals fire damage with a potency of 290. Additional Effect: Grants Astral Fire III and removes Umbral Ice</para>
    /// </summary>
    
    public IBaseAction FireIiiPvE => _FireIiiPvECreator.Value;
    private readonly Lazy<IBaseAction> _ThunderIiiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)153, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyThunderIiiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/153"><strong>Thunder III</strong></see> <i>PvE</i> (BLM) [153] [Spell]
    /// </summary>
    static partial void ModifyThunderIiiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/153"><strong>Thunder III</strong></see> <i>PvE</i> (BLM) [153] [Spell]
    /// <para>Deals lightning damage with a potency of 120. Additional Effect: Lightning damage over time Potency: 50 Duration: 27s Can only be cast while under the effect of Thunderhead granted when gaining Astral Fire or Umbral Ice from an unaspected state or changing between their influences. Only one Thunder spell-induced damage over time effect per caster can be inflicted upon a single target.</para>
    /// </summary>
    
    public IBaseAction ThunderIiiPvE => _ThunderIiiPvECreator.Value;
    private readonly Lazy<IBaseAction> _BlizzardIiiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)154, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBlizzardIiiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/154"><strong>Blizzard III</strong></see> <i>PvE</i> (BLM) [154] [Spell]
    /// </summary>
    static partial void ModifyBlizzardIiiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/154"><strong>Blizzard III</strong></see> <i>PvE</i> (BLM) [154] [Spell]
    /// <para>Deals ice damage with a potency of 290. Additional Effect: Grants Umbral Ice III and removes Astral Fire</para>
    /// </summary>
    
    public IBaseAction BlizzardIiiPvE => _BlizzardIiiPvECreator.Value;
    private readonly Lazy<IBaseAction> _AetherialManipulationPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)155, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyAetherialManipulationPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/155"><strong>Aetherial Manipulation</strong></see> <i>PvE</i> (THM BLM) [155] [Ability]
    /// </summary>
    static partial void ModifyAetherialManipulationPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/155"><strong>Aetherial Manipulation</strong></see> <i>PvE</i> (THM BLM) [155] [Ability]
    /// <para>Rush to a target party member's side. Unable to cast if bound.</para>
    /// </summary>
    
    public IBaseAction AetherialManipulationPvE => _AetherialManipulationPvECreator.Value;
    private readonly Lazy<IBaseAction> _ScathePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)156, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyScathePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/156"><strong>Scathe</strong></see> <i>PvE</i> (THM BLM) [156] [Spell]
    /// </summary>
    static partial void ModifyScathePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/156"><strong>Scathe</strong></see> <i>PvE</i> (THM BLM) [156] [Spell]
    /// <para>Deals unaspected damage with a potency of 100. Additional Effect: 20% chance potency will double</para>
    /// </summary>
    
    public IBaseAction ScathePvE => _ScathePvECreator.Value;
    private readonly Lazy<IBaseAction> _ManawardPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)157, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyManawardPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/157"><strong>Manaward</strong></see> <i>PvE</i> (THM BLM) [157] [Ability]
    /// </summary>
    static partial void ModifyManawardPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/157"><strong>Manaward</strong></see> <i>PvE</i> (THM BLM) [157] [Ability]
    /// <para>Creates a barrier that nullifies damage totaling up to 30% of maximum HP. Duration: 20s</para>
    /// </summary>
    
    public IBaseAction ManawardPvE => _ManawardPvECreator.Value;
    private readonly Lazy<IBaseAction> _ManafontPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)158, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyManafontPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/158"><strong>Manafont</strong></see> <i>PvE</i> (BLM) [158] [Ability]
    /// </summary>
    static partial void ModifyManafontPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/158"><strong>Manafont</strong></see> <i>PvE</i> (BLM) [158] [Ability]
    /// <para>Fully restores MP. Additional Effect: Grants Astral Fire III Additional Effect: Grants Thunderhead Can only be executed while under the effect of Astral Fire.</para>
    /// </summary>
    
    public IBaseAction ManafontPvE => _ManafontPvECreator.Value;
    private readonly Lazy<IBaseAction> _FreezePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)159, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyFreezePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/159"><strong>Freeze</strong></see> <i>PvE</i> (BLM) [159] [Spell]
    /// </summary>
    static partial void ModifyFreezePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/159"><strong>Freeze</strong></see> <i>PvE</i> (BLM) [159] [Spell]
    /// <para>Deals ice damage with a potency of 120 to target and all enemies nearby it. Can only be executed while under the effect of Umbral Ice.</para>
    /// </summary>
    
    public IBaseAction FreezePvE => _FreezePvECreator.Value;
    private readonly Lazy<IBaseAction> _FlarePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)162, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyFlarePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/162"><strong>Flare</strong></see> <i>PvE</i> (BLM) [162] [Spell]
    /// </summary>
    static partial void ModifyFlarePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/162"><strong>Flare</strong></see> <i>PvE</i> (BLM) [162] [Spell]
    /// <para>Deals fire damage to target and all enemies nearby it with a potency of 240 for the first enemy and 30% less for all remaining enemies. Additional Effect: Grants Astral Fire III Can only be executed while under the effect of Astral Fire.</para>
    /// </summary>
    
    public IBaseAction FlarePvE => _FlarePvECreator.Value;
    private readonly Lazy<IBaseAction> _LeyLinesPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3573, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyLeyLinesPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3573"><strong>Ley Lines</strong></see> <i>PvE</i> (BLM) [3573] [Ability]
    /// </summary>
    static partial void ModifyLeyLinesPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3573"><strong>Ley Lines</strong></see> <i>PvE</i> (BLM) [3573] [Ability]
    /// <para>Connects naturally occurring ley lines to create a circle of power which while standing within it reduces spell cast time and recast time and auto-attack delay by 15%. Duration: 20s Maximum Charges: 2 Cannot be executed while under the effect of Ley Lines.</para>
    /// </summary>
    
    public IBaseAction LeyLinesPvE => _LeyLinesPvECreator.Value;
    private readonly Lazy<IBaseAction> _BlizzardIvPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3576, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBlizzardIvPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3576"><strong>Blizzard IV</strong></see> <i>PvE</i> (BLM) [3576] [Spell]
    /// </summary>
    static partial void ModifyBlizzardIvPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3576"><strong>Blizzard IV</strong></see> <i>PvE</i> (BLM) [3576] [Spell]
    /// <para>Deals ice damage with a potency of 300. Additional Effect: Grants 3 Umbral Hearts Umbral Heart Bonus: Nullifies Astral Fire's MP cost increase for Fire spells and reduces MP cost for Flare by one-third Can only be executed while under the effect of Umbral Ice.</para>
    /// </summary>
    
    public IBaseAction BlizzardIvPvE => _BlizzardIvPvECreator.Value;
    private readonly Lazy<IBaseAction> _FireIvPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)3577, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyFireIvPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/3577"><strong>Fire IV</strong></see> <i>PvE</i> (BLM) [3577] [Spell]
    /// </summary>
    static partial void ModifyFireIvPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/3577"><strong>Fire IV</strong></see> <i>PvE</i> (BLM) [3577] [Spell]
    /// <para>Deals fire damage with a potency of 300. Can only be executed while under the effect of Astral Fire.</para>
    /// </summary>
    
    public IBaseAction FireIvPvE => _FireIvPvECreator.Value;
    private readonly Lazy<IBaseAction> _BetweenTheLinesPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7419, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBetweenTheLinesPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7419"><strong>Between the Lines</strong></see> <i>PvE</i> (BLM) [7419] [Ability]
    /// </summary>
    static partial void ModifyBetweenTheLinesPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7419"><strong>Between the Lines</strong></see> <i>PvE</i> (BLM) [7419] [Ability]
    /// <para>Move instantly to Ley Lines drawn by you. Cannot be executed while bound.</para>
    /// </summary>
    
    public IBaseAction BetweenTheLinesPvE => _BetweenTheLinesPvECreator.Value;
    private readonly Lazy<IBaseAction> _ThunderIvPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7420, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyThunderIvPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7420"><strong>Thunder IV</strong></see> <i>PvE</i> (BLM) [7420] [Spell]
    /// </summary>
    static partial void ModifyThunderIvPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7420"><strong>Thunder IV</strong></see> <i>PvE</i> (BLM) [7420] [Spell]
    /// <para>Deals lightning damage with a potency of 80 to target and all enemies nearby it. Additional Effect: Lightning damage over time Potency: 35 Duration: 21s Can only be cast while under the effect of Thunderhead granted when gaining Astral Fire or Umbral Ice from an unaspected state or changing between their influences. Only one Thunder spell-induced damage over time effect per caster can be inflicted upon a single target.</para>
    /// </summary>
    
    public IBaseAction ThunderIvPvE => _ThunderIvPvECreator.Value;
    private readonly Lazy<IBaseAction> _TriplecastPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7421, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyTriplecastPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7421"><strong>Triplecast</strong></see> <i>PvE</i> (BLM) [7421] [Ability]
    /// </summary>
    static partial void ModifyTriplecastPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7421"><strong>Triplecast</strong></see> <i>PvE</i> (BLM) [7421] [Ability]
    /// <para>The next three spells will require no cast time. Duration: 15s Maximum Charges: 2</para>
    /// </summary>
    
    public IBaseAction TriplecastPvE => _TriplecastPvECreator.Value;
    private readonly Lazy<IBaseAction> _FoulPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7422, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyFoulPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7422"><strong>Foul</strong></see> <i>PvE</i> (BLM) [7422] [Spell]
    /// </summary>
    static partial void ModifyFoulPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7422"><strong>Foul</strong></see> <i>PvE</i> (BLM) [7422] [Spell]
    /// <para>Deals unaspected damage to target and all enemies nearby it with a potency of 600 for the first enemy and 25% less for all remaining enemies. Polyglot Cost: 1</para>
    /// </summary>
    
    public IBaseAction FoulPvE => _FoulPvECreator.Value;
    private readonly Lazy<IBaseAction> _ThunderIiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)7447, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyThunderIiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/7447"><strong>Thunder II</strong></see> <i>PvE</i> (THM BLM) [7447] [Spell]
    /// </summary>
    static partial void ModifyThunderIiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/7447"><strong>Thunder II</strong></see> <i>PvE</i> (THM BLM) [7447] [Spell]
    /// <para>Deals lightning damage with a potency of 60 to target and all enemies nearby it. Additional Effect: Lightning damage over time Potency: 30 Duration: 18s Can only be cast while under the effect of Thunderhead granted when gaining Astral Fire or Umbral Ice from an unaspected state or changing between their influences. Only one Thunder spell-induced damage over time effect per caster can be inflicted upon a single target.</para>
    /// </summary>
    
    public IBaseAction ThunderIiPvE => _ThunderIiPvECreator.Value;
    private readonly Lazy<IBaseAction> _DespairPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16505, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyDespairPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16505"><strong>Despair</strong></see> <i>PvE</i> (BLM) [16505] [Spell]
    /// </summary>
    static partial void ModifyDespairPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16505"><strong>Despair</strong></see> <i>PvE</i> (BLM) [16505] [Spell]
    /// <para>Deals fire damage with a potency of 350. Additional Effect: Grants Astral Fire III Can only be executed while under the effect of Astral Fire.</para>
    /// </summary>
    
    public IBaseAction DespairPvE => _DespairPvECreator.Value;
    private readonly Lazy<IBaseAction> _UmbralSoulPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16506, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyUmbralSoulPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16506"><strong>Umbral Soul</strong></see> <i>PvE</i> (BLM) [16506] [Spell]
    /// </summary>
    static partial void ModifyUmbralSoulPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16506"><strong>Umbral Soul</strong></see> <i>PvE</i> (BLM) [16506] [Spell]
    /// <para>Additional Effect: Restores an amount of MP commensurate with your stacks of Umbral Ice Umbral Ice I: 2,500 MP Umbral Ice II: 5,000 MP Umbral Ice III: 10,000 MP Can only be executed while under the effect of Umbral Ice.</para>
    /// </summary>
    
    public IBaseAction UmbralSoulPvE => _UmbralSoulPvECreator.Value;
    private readonly Lazy<IBaseAction> _XenoglossyPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)16507, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyXenoglossyPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/16507"><strong>Xenoglossy</strong></see> <i>PvE</i> (BLM) [16507] [Spell]
    /// </summary>
    static partial void ModifyXenoglossyPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/16507"><strong>Xenoglossy</strong></see> <i>PvE</i> (BLM) [16507] [Spell]
    /// <para>Deals unaspected damage with a potency of 890. Polyglot Cost: 1</para>
    /// </summary>
    
    public IBaseAction XenoglossyPvE => _XenoglossyPvECreator.Value;
    private readonly Lazy<IBaseAction> _BlizzardIiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25793, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBlizzardIiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25793"><strong>Blizzard II</strong></see> <i>PvE</i> (THM BLM) [25793] [Spell]
    /// </summary>
    static partial void ModifyBlizzardIiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25793"><strong>Blizzard II</strong></see> <i>PvE</i> (THM BLM) [25793] [Spell]
    /// <para>Deals ice damage with a potency of 80 to target and all enemies nearby it. Additional Effect: removes Astral Fire</para>
    /// </summary>
    
    public IBaseAction BlizzardIiPvE => _BlizzardIiPvECreator.Value;
    private readonly Lazy<IBaseAction> _HighFireIiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25794, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHighFireIiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25794"><strong>High Fire II</strong></see> <i>PvE</i> (BLM) [25794] [Spell]
    /// </summary>
    static partial void ModifyHighFireIiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25794"><strong>High Fire II</strong></see> <i>PvE</i> (BLM) [25794] [Spell]
    /// <para>Deals fire damage with a potency of 100 to target and all enemies nearby it. Additional Effect: Grants Astral Fire III and removes Umbral Ice</para>
    /// </summary>
    
    public IBaseAction HighFireIiPvE => _HighFireIiPvECreator.Value;
    private readonly Lazy<IBaseAction> _HighBlizzardIiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25795, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHighBlizzardIiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25795"><strong>High Blizzard II</strong></see> <i>PvE</i> (BLM) [25795] [Spell]
    /// </summary>
    static partial void ModifyHighBlizzardIiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25795"><strong>High Blizzard II</strong></see> <i>PvE</i> (BLM) [25795] [Spell]
    /// <para>Deals ice damage with a potency of 100 to target and all enemies nearby it. Additional Effect: Grants Umbral Ice III and removes Astral Fire</para>
    /// </summary>
    
    public IBaseAction HighBlizzardIiPvE => _HighBlizzardIiPvECreator.Value;
    private readonly Lazy<IBaseAction> _AmplifierPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25796, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyAmplifierPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25796"><strong>Amplifier</strong></see> <i>PvE</i> (BLM) [25796] [Ability]
    /// </summary>
    static partial void ModifyAmplifierPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25796"><strong>Amplifier</strong></see> <i>PvE</i> (BLM) [25796] [Ability]
    /// <para>Grants Polyglot. Can only be executed while under the effect of Astral Fire or Umbral Ice.</para>
    /// </summary>
    
    public IBaseAction AmplifierPvE => _AmplifierPvECreator.Value;
    private readonly Lazy<IBaseAction> _ParadoxPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)25797, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyParadoxPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/25797"><strong>Paradox</strong></see> <i>PvE</i> (BLM) [25797] [Spell]
    /// </summary>
    static partial void ModifyParadoxPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/25797"><strong>Paradox</strong></see> <i>PvE</i> (BLM) [25797] [Spell]
    /// <para>Deals unaspected damage with a potency of 540. Astral Fire Bonus: Grants Firestarter Firestarter Effect: Next Fire III will require no time to cast and cost no MP Umbral Ice Bonus: Requires no MP to cast Can only be executed while under the effect of Paradox. ※This action cannot be assigned to a hotbar. ※Fire and Blizzard change to Paradox when requirements for execution are met.</para>
    /// </summary>
    
    public IBaseAction ParadoxPvE => _ParadoxPvECreator.Value;
    private readonly Lazy<IBaseAction> _FirePvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29649, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyFirePvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29649"><strong>Fire</strong></see> <i>PvP</i> (BLM) [29649] [Spell]
    /// </summary>
    static partial void ModifyFirePvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29649"><strong>Fire</strong></see> <i>PvP</i> (BLM) [29649] [Spell]
    /// <para>Deals fire damage with a potency of 6,750. Additional Effect: Grants Astral Fire Duration: 15s Effect cannot be stacked with Umbral Ice. Additional Effect: Grants Paradox Duration: 15s ※Action changes to Fire III upon execution.</para>
    /// </summary>
    
    public IBaseAction FirePvP => _FirePvPCreator.Value;
    private readonly Lazy<IBaseAction> _FireIvPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29650, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyFireIvPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29650"><strong>Fire IV</strong></see> <i>PvP</i> (BLM) [29650] [Spell]
    /// </summary>
    static partial void ModifyFireIvPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29650"><strong>Fire IV</strong></see> <i>PvP</i> (BLM) [29650] [Spell]
    /// <para>Deals fire damage with a potency of 11,250. Additional Effect: Grants Astral Fire III Duration: 15s Effect cannot be stacked with Umbral Ice. Can only be executed while under the effect of Astral Fire II. ※Action changes to High Fire II upon execution. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction FireIvPvP => _FireIvPvPCreator.Value;
    private readonly Lazy<IBaseAction> _FlarePvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29651, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyFlarePvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29651"><strong>Flare</strong></see> <i>PvP</i> (BLM) [29651] [Spell]
    /// </summary>
    static partial void ModifyFlarePvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29651"><strong>Flare</strong></see> <i>PvP</i> (BLM) [29651] [Spell]
    /// <para>Deals fire damage with a potency of 15,000 to target and all enemies nearby it. Additional Effect: Grants Astral Fire III Duration: 15s Effect cannot be stacked with Umbral Ice. Can only be executed while under the effect of Soul Resonance. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction FlarePvP => _FlarePvPCreator.Value;
    private readonly Lazy<IBaseAction> _BlizzardPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29653, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBlizzardPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29653"><strong>Blizzard</strong></see> <i>PvP</i> (BLM) [29653] [Spell]
    /// </summary>
    static partial void ModifyBlizzardPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29653"><strong>Blizzard</strong></see> <i>PvP</i> (BLM) [29653] [Spell]
    /// <para>Deals ice damage with a potency of 4,500. Additional Effect: Grants Umbral Ice Duration: 15s Effect cannot be stacked with Astral Fire. Additional Effect: Grants Paradox Duration: 15s ※Action changes to Blizzard III upon execution.</para>
    /// </summary>
    
    public IBaseAction BlizzardPvP => _BlizzardPvPCreator.Value;
    private readonly Lazy<IBaseAction> _BlizzardIvPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29654, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBlizzardIvPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29654"><strong>Blizzard IV</strong></see> <i>PvP</i> (BLM) [29654] [Spell]
    /// </summary>
    static partial void ModifyBlizzardIvPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29654"><strong>Blizzard IV</strong></see> <i>PvP</i> (BLM) [29654] [Spell]
    /// <para>Deals ice damage with a potency of 7,500. Additional Effect: Grants Umbral Ice III Duration: 15s Effect cannot be stacked with Astral Fire. Can only be executed while under the effect of Umbral Ice II. ※Action changes to High Blizzard II upon execution. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction BlizzardIvPvP => _BlizzardIvPvPCreator.Value;
    private readonly Lazy<IBaseAction> _FreezePvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29655, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyFreezePvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29655"><strong>Freeze</strong></see> <i>PvP</i> (BLM) [29655] [Spell]
    /// </summary>
    static partial void ModifyFreezePvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29655"><strong>Freeze</strong></see> <i>PvP</i> (BLM) [29655] [Spell]
    /// <para>Deals ice damage with a potency of 10,000 to target and all enemies nearby it. Additional Effect: Grants Umbral Ice III Duration: 15s Effect cannot be stacked with Astral Fire. Can only be executed while under the effect of Soul Resonance. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction FreezePvP => _FreezePvPCreator.Value;
    private readonly Lazy<IBaseAction> _BurstPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29657, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBurstPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29657"><strong>Burst</strong></see> <i>PvP</i> (BLM) [29657] [Spell]
    /// </summary>
    static partial void ModifyBurstPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29657"><strong>Burst</strong></see> <i>PvP</i> (BLM) [29657] [Spell]
    /// <para>Deals lightning damage to all nearby enemies with a potency of 15,000. Additional Effect: Creates a barrier around self that absorbs damage equivalent to a heal of 15,000 potency Duration: 10s This action does not share a recast timer with any other actions.</para>
    /// </summary>
    
    public IBaseAction BurstPvP => _BurstPvPCreator.Value;
    private readonly Lazy<IBaseAction> _XenoglossyPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29658, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyXenoglossyPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29658"><strong>Xenoglossy</strong></see> <i>PvP</i> (BLM) [29658] [Spell]
    /// </summary>
    static partial void ModifyXenoglossyPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29658"><strong>Xenoglossy</strong></see> <i>PvP</i> (BLM) [29658] [Spell]
    /// <para>Deals unaspected damage with a potency of 12,000. Additional Effect: Absorbs 50% of damage dealt as HP Potency is reduced to 8,000 when your HP is below 50% but absorbs 200% of damage dealt as HP. Maximum Charges: 2 This action does not share a recast timer with any other actions.</para>
    /// </summary>
    
    public IBaseAction XenoglossyPvP => _XenoglossyPvPCreator.Value;
    private readonly Lazy<IBaseAction> _AetherialManipulationPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29660, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyAetherialManipulationPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29660"><strong>Aetherial Manipulation</strong></see> <i>PvP</i> (BLM) [29660] [Ability]
    /// </summary>
    static partial void ModifyAetherialManipulationPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29660"><strong>Aetherial Manipulation</strong></see> <i>PvP</i> (BLM) [29660] [Ability]
    /// <para>Rush to a target's side. Cannot be executed while bound.</para>
    /// </summary>
    
    public IBaseAction AetherialManipulationPvP => _AetherialManipulationPvPCreator.Value;
    private readonly Lazy<IBaseAction> _ParadoxPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)29663, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyParadoxPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/29663"><strong>Paradox</strong></see> <i>PvP</i> (BLM) [29663] [Spell]
    /// </summary>
    static partial void ModifyParadoxPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/29663"><strong>Paradox</strong></see> <i>PvP</i> (BLM) [29663] [Spell]
    /// <para>Deals unaspected damage with a potency of 9,000. Additional Effect: Increases your stacks of Astral Fire or Umbral Ice to maximum Duration: 15s Can only be executed while under the effect of Paradox. This action does not share a recast timer with any other actions.</para>
    /// </summary>
    
    public IBaseAction ParadoxPvP => _ParadoxPvPCreator.Value;
    private readonly Lazy<IBaseAction> _FireIiiPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)30896, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyFireIiiPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/30896"><strong>Fire III</strong></see> <i>PvP</i> (BLM) [30896] [Spell]
    /// </summary>
    static partial void ModifyFireIiiPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/30896"><strong>Fire III</strong></see> <i>PvP</i> (BLM) [30896] [Spell]
    /// <para>Deals fire damage with a potency of 9,000. Additional Effect: Grants Astral Fire II Duration: 15s Effect cannot be stacked with Umbral Ice. Can only be executed while under the effect of Astral Fire. ※Action changes to Fire IV upon execution. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction FireIiiPvP => _FireIiiPvPCreator.Value;
    private readonly Lazy<IBaseAction> _BlizzardIiiPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)30897, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyBlizzardIiiPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/30897"><strong>Blizzard III</strong></see> <i>PvP</i> (BLM) [30897] [Spell]
    /// </summary>
    static partial void ModifyBlizzardIiiPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/30897"><strong>Blizzard III</strong></see> <i>PvP</i> (BLM) [30897] [Spell]
    /// <para>Deals ice damage with a potency of 6,000. Additional Effect: Grants Umbral Ice II Duration: 15s Effect cannot be stacked with Astral Fire. Can only be executed while under the effect of Umbral Ice. ※Action changes to Blizzard IV upon execution. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction BlizzardIiiPvP => _BlizzardIiiPvPCreator.Value;
    private readonly Lazy<IBaseAction> _HighThunderPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)36986, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHighThunderPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/36986"><strong>High Thunder</strong></see> <i>PvE</i> (BLM) [36986] [Spell]
    /// </summary>
    static partial void ModifyHighThunderPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/36986"><strong>High Thunder</strong></see> <i>PvE</i> (BLM) [36986] [Spell]
    /// <para>Deals lightning damage with a potency of 150. Additional Effect: Lightning damage over time Potency: 60 Duration: 30s Can only be cast while under the effect of Thunderhead granted when gaining Astral Fire or Umbral Ice from an unaspected state or changing between their influences. Only one Thunder spell-induced damage over time effect per caster can be inflicted upon a single target.</para>
    /// </summary>
    
    public IBaseAction HighThunderPvE => _HighThunderPvECreator.Value;
    private readonly Lazy<IBaseAction> _HighThunderIiPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)36987, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHighThunderIiPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/36987"><strong>High Thunder II</strong></see> <i>PvE</i> (BLM) [36987] [Spell]
    /// </summary>
    static partial void ModifyHighThunderIiPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/36987"><strong>High Thunder II</strong></see> <i>PvE</i> (BLM) [36987] [Spell]
    /// <para>Deals lightning damage with a potency of 100 to target and all enemies nearby it. Additional Effect: Lightning damage over time Potency: 40 Duration: 24s Can only be cast while under the effect of Thunderhead granted when gaining Astral Fire or Umbral Ice from an unaspected state or changing between their influences. Only one Thunder spell-induced damage over time effect per caster can be inflicted upon a single target.</para>
    /// </summary>
    
    public IBaseAction HighThunderIiPvE => _HighThunderIiPvECreator.Value;
    private readonly Lazy<IBaseAction> _RetracePvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)36988, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyRetracePvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/36988"><strong>Retrace</strong></see> <i>PvE</i> (BLM) [36988] [Ability]
    /// </summary>
    static partial void ModifyRetracePvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/36988"><strong>Retrace</strong></see> <i>PvE</i> (BLM) [36988] [Ability]
    /// <para>Weave ley lines anew setting your circle of power at a new location. Can only be executed while under the effect of Ley Lines and the effect duration will not be reset.</para>
    /// </summary>
    
    public IBaseAction RetracePvE => _RetracePvECreator.Value;
    private readonly Lazy<IBaseAction> _FlareStarPvECreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)36989, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyFlareStarPvE(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/36989"><strong>Flare Star</strong></see> <i>PvE</i> (BLM) [36989] [Spell]
    /// </summary>
    static partial void ModifyFlareStarPvE(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/36989"><strong>Flare Star</strong></see> <i>PvE</i> (BLM) [36989] [Spell]
    /// <para>Deals fire damage to target and all enemies nearby it with a potency of 500 for the first enemy and 65% less for all remaining enemies. Can only be executed when the Astral Gauge is full.</para>
    /// </summary>
    
    public IBaseAction FlareStarPvE => _FlareStarPvECreator.Value;
    private readonly Lazy<IBaseAction> _HighFireIiPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41473, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHighFireIiPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41473"><strong>High Fire II</strong></see> <i>PvP</i> (BLM) [41473] [Spell]
    /// </summary>
    static partial void ModifyHighFireIiPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41473"><strong>High Fire II</strong></see> <i>PvP</i> (BLM) [41473] [Spell]
    /// <para>Deals fire damage to target and all enemies nearby it with a potency of 13,500 for the first enemy and 9,000 for all remaining enemies. Additional Effect: Grants Astral Fire Duration: 15s Effect cannot be stacked with Umbral Ice Can only be executed while under the effect of Astral Fire III. ※Action changes to Fire III upon execution. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction HighFireIiPvP => _HighFireIiPvPCreator.Value;
    private readonly Lazy<IBaseAction> _HighBlizzardIiPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41474, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyHighBlizzardIiPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41474"><strong>High Blizzard II</strong></see> <i>PvP</i> (BLM) [41474] [Spell]
    /// </summary>
    static partial void ModifyHighBlizzardIiPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41474"><strong>High Blizzard II</strong></see> <i>PvP</i> (BLM) [41474] [Spell]
    /// <para>Deals ice damage to target and all enemies nearby it with a potency of 9,000 for the first enemy and 6,000 for all remaining enemies. Additional Effect: Grants Umbral Ice Duration: 15s Effect cannot be stacked with Astral Fire. Can only be executed while under the effect of Umbral Ice III. ※Action changes to Blizzard III upon execution. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction HighBlizzardIiPvP => _HighBlizzardIiPvPCreator.Value;
    private readonly Lazy<IBaseAction> _ElementalWeavePvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41475, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyElementalWeavePvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41475"><strong>Elemental Weave</strong></see> <i>PvP</i> (BLM) [41475] [Ability]
    /// </summary>
    static partial void ModifyElementalWeavePvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41475"><strong>Elemental Weave</strong></see> <i>PvP</i> (BLM) [41475] [Ability]
    /// <para>Changes to Wreath of Fire while under the effect of Astral Fire or Wreath of Ice while under the effect of Umbral Ice.</para>
    /// </summary>
    
    public IBaseAction ElementalWeavePvP => _ElementalWeavePvPCreator.Value;
    private readonly Lazy<IBaseAction> _WreathOfFirePvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41476, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyWreathOfFirePvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41476"><strong>Wreath of Fire</strong></see> <i>PvP</i> (BLM) [41476] [Ability]
    /// </summary>
    static partial void ModifyWreathOfFirePvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41476"><strong>Wreath of Fire</strong></see> <i>PvP</i> (BLM) [41476] [Ability]
    /// <para>Grants Wreath of Fire. Wreath of Fire Effect: Deals additional fire damage with a potency of 4,500 to target and all enemies within 5 yalms when casting spells Ignores the effects of Guard when dealing damage. Duration: 10s Can only be executed while under the effect of Astral Fire. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction WreathOfFirePvP => _WreathOfFirePvPCreator.Value;
    private readonly Lazy<IBaseAction> _WreathOfIcePvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41478, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyWreathOfIcePvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41478"><strong>Wreath of Ice</strong></see> <i>PvP</i> (BLM) [41478] [Ability]
    /// </summary>
    static partial void ModifyWreathOfIcePvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41478"><strong>Wreath of Ice</strong></see> <i>PvP</i> (BLM) [41478] [Ability]
    /// <para>Grants Wreath of Ice. Wreath of Ice Effect: Reduces damage taken by 20% and delivers an ice attack with a potency of 3,000 every time you suffer damage Duration: 10s Can only be executed while under the effect of Umbral Ice. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction WreathOfIcePvP => _WreathOfIcePvPCreator.Value;
    private readonly Lazy<IBaseAction> _FlareStarPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41480, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyFlareStarPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41480"><strong>Flare Star</strong></see> <i>PvP</i> (BLM) [41480] [Spell]
    /// </summary>
    static partial void ModifyFlareStarPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41480"><strong>Flare Star</strong></see> <i>PvP</i> (BLM) [41480] [Spell]
    /// <para>Deals fire damage with a potency of 20,000 to target and all enemies nearby it. Additional Effect: Afflicts target with Burns dealing fire damage over time Potency: 5,000 Duration: 12s Can only be executed while under the effect of Astral Fire and Elemental Star. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction FlareStarPvP => _FlareStarPvPCreator.Value;
    private readonly Lazy<IBaseAction> _FrostStarPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41481, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyFrostStarPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41481"><strong>Frost Star</strong></see> <i>PvP</i> (BLM) [41481] [Spell]
    /// </summary>
    static partial void ModifyFrostStarPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41481"><strong>Frost Star</strong></see> <i>PvP</i> (BLM) [41481] [Spell]
    /// <para>Deals ice damage with a potency of 20,000 to target and all enemies nearby it. Additional Effect: Afflicts target with Deep Freeze Deep Freeze Effect: Target is temporarily unable to act Duration: 3s Can only be executed while under the effect of Umbral Ice and Elemental Star. ※This action cannot be assigned to a hotbar.</para>
    /// </summary>
    
    public IBaseAction FrostStarPvP => _FrostStarPvPCreator.Value;
    private readonly Lazy<IBaseAction> _LethargyPvPCreator = new(() => 
    {
        IBaseAction action = new BaseAction((ActionID)41510, false);
        CustomRotation.LoadActionSetting(ref action);
    
        var setting = action.Setting;
        ModifyLethargyPvP(ref setting);
        action.Setting = setting;
    
        return action;
    });
    
    /// <summary>
    /// Modify <see href="https://garlandtools.org/db/#action/41510"><strong>Lethargy</strong></see> <i>PvP</i> (BLM) [41510] [Ability]
    /// </summary>
    static partial void ModifyLethargyPvP(ref ActionSetting setting);
    
    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/41510"><strong>Lethargy</strong></see> <i>PvP</i> (BLM) [41510] [Ability]
    /// <para>Lowers target's damage dealt by 33%. Duration: 5s Additional Effect: Heavy +75% Duration: 3s</para>
    /// </summary>
    
    public IBaseAction LethargyPvP => _LethargyPvPCreator.Value;

    private IBaseAction[] _AllBaseActions = null;
    
    /// <inheritdoc/>
    public override IBaseAction[] AllBaseActions => _AllBaseActions ??= [
    	FirePvE, BlizzardPvE, ThunderPvE, FireIiPvE, TransposePvE, FireIiiPvE, ThunderIiiPvE, BlizzardIiiPvE, AetherialManipulationPvE, ScathePvE, ManawardPvE, ManafontPvE, FreezePvE, FlarePvE, LeyLinesPvE, BlizzardIvPvE, FireIvPvE, BetweenTheLinesPvE, ThunderIvPvE, TriplecastPvE, FoulPvE, ThunderIiPvE, DespairPvE, UmbralSoulPvE, XenoglossyPvE, BlizzardIiPvE, HighFireIiPvE, HighBlizzardIiPvE, AmplifierPvE, ParadoxPvE, FirePvP, FireIvPvP, FlarePvP, BlizzardPvP, BlizzardIvPvP, FreezePvP, BurstPvP, XenoglossyPvP, AetherialManipulationPvP, ParadoxPvP, FireIiiPvP, BlizzardIiiPvP, HighThunderPvE, HighThunderIiPvE, RetracePvE, FlareStarPvE, HighFireIiPvP, HighBlizzardIiPvP, ElementalWeavePvP, WreathOfFirePvP, WreathOfIcePvP, FlareStarPvP, FrostStarPvP, LethargyPvP,
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
private readonly Lazy<IBaseAction> _MeteorPvECreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)205, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifyMeteorPvE(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/205"><strong>Meteor</strong></see> <i>PvE</i> (All Classes) [205] [Limit Break]
/// </summary>
static partial void ModifyMeteorPvE(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/205"><strong>Meteor</strong></see> <i>PvE</i> (All Classes) [205] [Limit Break]
/// <para>Deals unaspected damage with a potency of 6,150 to all enemies near point of impact.</para>
/// </summary>

public IBaseAction MeteorPvE => _MeteorPvECreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/205"><strong>Meteor</strong></see> <i>PvE</i> (All Classes) [205] [Limit Break]
/// <para>Deals unaspected damage with a potency of 6,150 to all enemies near point of impact.</para>
/// </summary>
private sealed protected override IBaseAction LimitBreak3 => MeteorPvE;
private readonly Lazy<IBaseAction> _SoulResonancePvPCreator = new(() => 
{
    IBaseAction action = new BaseAction((ActionID)29662, false);
    CustomRotation.LoadActionSetting(ref action);

    var setting = action.Setting;
    ModifySoulResonancePvP(ref setting);
    action.Setting = setting;

    return action;
});

/// <summary>
/// Modify <see href="https://garlandtools.org/db/#action/29662"><strong>Soul Resonance</strong></see> <i>PvP</i> (BLM) [29662] [Limit Break]
/// </summary>
static partial void ModifySoulResonancePvP(ref ActionSetting setting);

/// <summary>
/// <see href="https://garlandtools.org/db/#action/29662"><strong>Soul Resonance</strong></see> <i>PvP</i> (BLM) [29662] [Limit Break]
/// <para>Grants 6 stacks of Soul Resonance upgrading Fire to Flare and Blizzard to Freeze. Duration: 30s Additional Effect: Grants Elemental Star Elemental Star Effect: Action changes to Flare Star while under the effect of Astral Fire or Frost Star while under the effect of Umbral Ice Duration: 60s Can only be executed when the limit gauge is full. Gauge Charge Time: 60s</para>
/// </summary>

private IBaseAction SoulResonancePvP => _SoulResonancePvPCreator.Value;
/// <summary>
/// <see href="https://garlandtools.org/db/#action/29662"><strong>Soul Resonance</strong></see> <i>PvP</i> (BLM) [29662] [Limit Break]
/// <para>Grants 6 stacks of Soul Resonance upgrading Fire to Flare and Blizzard to Freeze. Duration: 30s Additional Effect: Grants Elemental Star Elemental Star Effect: Action changes to Flare Star while under the effect of Astral Fire or Frost Star while under the effect of Umbral Ice Duration: 60s Can only be executed when the limit gauge is full. Gauge Charge Time: 60s</para>
/// </summary>
private sealed protected override IBaseAction LimitBreakPvP => SoulResonancePvP;

#endregion

#region Traits

    /// <summary>
    /// <see href="https://garlandtools.org/db/#action/50171"><strong>Thunder Mastery) | Thunder III |
| Umbral Soul | braucht Umbral Ice | Blizzard |
| Xenoglossy | kostet | Polyglot |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Blizzard II | Regel sperrt vorher | Manafont |
| Blizzard III | Regel sperrt vorher | Manafont |
| Blizzard IV | Regel prüft | Freeze |
| Blizzard | Regel sperrt vorher | Manafont |
| Blizzard | Regel prüft | Transpose |
| Blizzard | Regel prüft | Umbral Soul |
| Fire III | Regel prüft | Fire |
| Fire IV | Regel prüft | Fire |
| Foul | Regel prüft | Amplifier |
| Foul | Regel prüft | Xenoglossy |
| Freeze | Regel prüft | Blizzard IV |
| Retrace | Regel prüft | Ley Lines |
| Retrace | StatusNeed LeyLines | Ley Lines |
| Swiftcast | Regel prüft | Paradox |
| Thunder II | Regel sperrt vorher | Thunder III |
| Thunder II | Regel sperrt vorher | Thunder IV |
| Thunder II | Regel sperrt vorher | Thunder |
| Thunder III | Regel sperrt vorher | Thunder II |
| Thunder III | Regel sperrt vorher | Thunder IV |
| Thunder III | Regel sperrt vorher | Thunder |
| Thunder | Regel sperrt vorher | Thunder II |
| Thunder | Regel prüft | Thunder III |
| Thunder | Regel sperrt vorher | Thunder III |
| Thunder | Regel sperrt vorher | Thunder IV |
| Transpose | Regel prüft | Fire III |
| Transpose | Regel sperrt vorher | Manafont |
| Transpose | Regel prüft | Paradox |
| Triplecast | Regel prüft | Paradox |
| Triplecast | Regel sperrt vorher | Paradox |
| Triplecast | Regel prüft | Swiftcast |
| Triplecast | Regel sperrt vorher | Swiftcast |
| Xenoglossy | Regel prüft | Amplifier |

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `MeteorPvE`, `SkyshardPvE`, `StarstormPvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Sleep (`SleepPvE`, Spell): ungenutzt

## Abgleich Wirktext ↔ Code

Der Wirktext nennt eine Bedingung, die Basisrotation führt dafür weder `StatusNeed` noch `ActionCheck`. RSR verlässt sich dann auf die Nutzbarkeitsauskunft des Spiels, die `BasicCheck` nur für eine feste Liste von Ablehnungscodes liest. Kandidaten, keine Befunde.

- Manafont (`ManafontPvE`): Astral Fire

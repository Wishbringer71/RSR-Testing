# What this fork does differently in a fight

Against upstream **7.5.6.9**. Settings are named as they appear in the configuration, and
where a change sits behind a switch its default is given; without that note it takes effect
immediately. Two changes are confirmed in play — the raise dispatch and the tank pre-pull
HoT. Everything else is established in the code and compiled in CI, which says that a chain
closes, not that it is right at the target dummy.

## Check this setting first — a known defect in this build

`Skip mitigation for small area casts` is **on by default**, and with that default party
mitigation is withheld in almost every ordinary case. The condition only mitigates above an
impact of roughly 35 % of maximum health, which an ordinary raidwide does not reach, and the
impact is only known after the first hit of that action. In the fight: on Summoner neither
Addle nor Radiant Aegis has gone out against area damage since 17 September, and every
party-wide mitigation on that chain is affected.

**Switch the setting off** to get the previous behaviour. A new default in code would not
reach you — a configuration already in use keeps its stored value.

## Healing — when it lands

- **A pull no longer starts without healing.** For roughly the first 2.5 seconds of every
  pull there was no automatic healing at all, because the time-to-kill estimate read as "the
  fight is about to end" while it was still empty.
- **The white mage heals again while a pack is dying.** At the end of every pack all heal
  flags went out: tank below 20 %, no heal attempted, Holy instead of Cure.
- **Healing ahead of the hit instead of after it** — `Heal ahead of incoming damage`, **off
  by default**. Every healing threshold and the heal target choice read the health a member
  is heading for by the time a heal started now would land. A tank dropping fast gets his
  heal about one GCD earlier, and someone falling quickly is picked ahead of someone sitting
  lower but steady. The rate is net of mitigation, barrier and healing, and each prediction
  is held against the actual course every second and corrected.
- **The emergency full heal waits for a reason** — `Benediction needs a reason`, **on by
  default**. Benediction now also requires that the target is being attacked, that an area
  cast is announced, or that their health is measurably falling. Before that, a player who
  had just been raised read as the most urgent member in the party while nothing was
  happening to him — and the full heal was gone for ninety seconds.
- **The tank walks into the pull with a HoT already ticking** — `UsePreRegen` (White Mage),
  `UsePreAspectedBenefic` (Astrologian), each with two enemy-count thresholds. The first hits
  land on a tank who already has something running. Both actions are instant, so nothing is
  lost while moving. **Confirmed in play.**

## Healing — who it picks

- **Whoever is about to die comes before the role short-cuts.** A damage dealer at 10 % was
  passed over as soon as the tank sat at 44 %; a critical rank now ranks ahead of the fixed
  role thresholds.
- **A party member behind the camera can be healed again** — the "only targets in view"
  filter applied to heal targets as well.
- **A target under invulnerability** gets healing at a lowered threshold and the normal one
  shortly before the status ends, instead of none at all.

## Shields and barriers

- **The shield list did not know the most common barriers** — fifteen were missing, among
  them Divine Benison and The Blackest Night. Wherever the tree asked whether anything was
  still up on a target, the answer was no for most tank shields.
- **A shield is no longer credited against the healing threshold.** A shield prevents damage,
  it does not restore health: a tank at 40 % stands at 40 % whether a barrier is running or
  not, and healing withheld for a barrier that expires unspent is thrown away.

## Raising

- **It happens at once instead of after a long delay.** Swiftcast was only spent on a raise
  once the raise was already reported as the next GCD — which it could not be without
  Swiftcast. It now also falls when a raise is pending and would be castable. **Confirmed in
  play.**
- **Phoenix Down** is wired up and checks target eligibility. **Off by default.**
- **The healer-only hard-cast modes** hold the Swiftcast reservation their text promises:
  with Swiftcast ready, nothing is hard-cast any more.
- Swiftcast is not spent for damage in the Summoner rotation; it stays reserved for raises.

## Mitigation and defensives

- **Two tanks no longer stack Reprisal**, and the second one keeps the 60 seconds of cooldown
  it used to throw away. The same applied to Addle and Feint with two casters or two melees.
- **Paladin and Warrior now give Reprisal on raidwides too** — it sat only in the
  single-target path, although the Warrior's own description promised it for area damage.
- **The mitigation debuffs are sustained rather than applied once**, with the duration
  following the level 98 trait.
- **A buster aimed at a damage dealer gets an answer.** Twelve jobs use their existing
  reactive line — Feint, Addle, Troubadour, Tactician, Shield Samba, Third Eye — when a cast
  is actually aimed at them. Single-target defence for damage dealers was not staffed at all.
- **Self-healing for damage dealers** (Second Wind, Bloodbath) across ten jobs, where the
  role actions were declared and never used.
- **The BossModReborn timeline is only read when it is switched on.**
- **Area actions are measured by how hard they hit**, not only recognised as area actions;
  the list window shows the share of maximum health per action.

## Tank self-protection

- **The Blackest Night** repays its 3000 MP only when the barrier is fully absorbed, and the
  original trigger is far too weak for that. `BlackestNightUsage` offers narrower readings
  with a minimum enemy count and an emergency share. **The default is the old behaviour.**
  The other side of it: the white mage can hold Holy while a tank carries the barrier
  (`HoldHolyForBlackestNight`), because the stun stops exactly the hits that would spend it.
- **Living Dead and Walking Dead.** While more than two GCDs of Living Dead remain, a lowered
  healing threshold applies so the death effect can occur; within two GCDs the normal
  threshold returns, so healing resumes shortly before the status expires. The lead-in is
  suspended while zero would arrive before the window closes, since it would otherwise
  prevent the very death the rule exists for. `WithholdHealingForLivingDead` sharpens the
  first part and is **off by default**.
- **Arm's Length on a group pull** (`UseArmsLengthOnPull`) for its Slow, not only as
  knockback protection: +20 % on every enemy that strikes you throttles the whole incoming
  stream for fifteen seconds.
- **The co-tank Provoke** no longer pulls the boss off a tank standing under Superbolide,
  Living Dead or Holmgang.

## Damage and rotation

- **Summoner.** Searing Light is tied to the burst phase — Solar Bahamut, or Bahamut at lower
  levels; with a second Summoner in the party it falls back to the big summon, and across all
  established phases to Ifrit. `PreferTitanWhileMoving` (**off by default**) brings Titan
  forward while you are moving, because Topaz Rite and its follow-ups are instant while
  Garuda and Ifrit lose GCDs on the move. Titan is only brought forward, never skipped.
- **White mage, Holy.** Three separately switchable rules: do not overwrite the stun while it
  is still running (`StretchHolyStun`, **off by default**); hold Holy while the dark knight's
  barrier is meant to be filled; and hold Holy while more than half the enemies in radius are
  slowed, since the stun is worth more once the Slow has run out.
- **Thin Air** is only spent on an expensive spell under real MP pressure that Lucid Dreaming
  cannot answer (`ThinAirOnMpPressureOnly`). A raise still takes a charge regardless.

**Defects of the original that were costing something in the fight:**

| Site | What happened in the fight |
|---|---|
| Black mage, Thunder | From level sync 92 a fresh area DoT was cut short on every cast |
| Red mage, Impact | Impact never fired |
| Reaper and Viper | Leg Sweep and Arm's Length were given up to the combo gate, and the role default then took them ungated anyway |
| Nine dispatch overrides | Defensive and healing calls silently continued in the wrong chain |
| Restricted DoT guard | Targets excluded from DoTs got them anyway |
| Movement abilities | Double call, in the wrong order against the duty rotation |

# TODO / Offene Punkte (persistent — siehe CLAUDE.md REGEL, Persistenz-Klausel)

Diese Datei existiert, damit offene Konzepte und Findings eine
Kontextkomprimierung überleben. Bei Sitzungsbeginn lesen. Neue Findings
während der Arbeit hier ergänzen, nicht nur im Chat/Task-Tool belassen.

Nur offene Arbeit steht hier — ausnahmslos. Ein Punkt mit Status
GEFIXT/ABGESCHLOSSEN/VERWORFEN gehört NICHT mehr in diese Datei, sondern
wird nach `AUDIT_LOG.md` (Beleg-Archiv) verschoben, sobald der Status
feststeht — nicht hier mit erledigtem Status stehen gelassen. Der
vollständige Beleg-Trail (alle abgeschlossenen Batch- und
Einzelcommit-Prüfungen, Fork vs. Upstream, plus die komplette Herleitung
aller bisherigen Feature-/Aggro-Management-Arbeit) liegt in `AUDIT_LOG.md`
— dort nachsehen, bevor ein Commit/Bereich erneut geprüft oder ein
scheinbar neues Thema begonnen wird, um Doppelarbeit zu vermeiden.

## Offene Konzepte / Fixes (noch nicht umgesetzt)

### #54 WHM-Heilsuppression nach oGCD-Erschöpfung — Root Cause weiterhin NICHT gefunden, alle bekannten Kandidaten ausgeschlossen

Nutzer-Meldung, wörtlich verifizierte Fakten (keine Interpretation):
Normales Dungeon (Klyteum), echte Mitspieler (keine Duty-Support-NPCs).
Schwellen: alle acht Werte (Ability+Spell, Single+Area, je mit/ohne HoT)
konsistent auf >70% (ohne HoT) / 55-65% (mit HoT) angehoben. oGCD-Heilung
funktionierte zu Beginn, dann oGCDs aufgebraucht. Danach feuerten
manaverbrauchende GCD-Heilsprüche (Cure/Cure II) trotz vollem Mana NICHT —
kein Castversuch, kein Balken (nicht "abgebrochen", sondern nie versucht).
Tank fiel bis unter 20% HP, kein Schild/Barriere aktiv. Gegner (>3, kein
Mitheiler) waren unter 50% HP, aber weit von Tod entfernt, Ausgang unklar.
Heiler castete in dieser Zeit aktiv eine AoE-Stun-Aktion (Holy, CC bei 3+
Gegnern) — selbst gewählt von RSRs Automatik, nicht manuell vom Nutzer
gedrückt. Kein Stun auf den Heiler selbst.

Der bereits gefixte `AverageTTK`-Nullfallback (siehe AUDIT_LOG.md) erklärt
dies NICHT vollständig (nur ~2.5s-Fenster am Pull-Start).

**Vollständig ausgeschlossene Kandidaten (mit Beleg):**
- `CancelCastUpdater.shouldStopHealing` (Configs.cs:713, Default `false`) —
  setzt bereits laufenden Cast voraus, gab es laut Nutzer nicht.
- `DataCenter.IsTyrantCastingSpecialIndicator()` (nur `IsInM11S`) — normales
  Dungeon, nicht M11S.
- Schild-Credit auf Effective-HP (`ShieldCreditAllowed`/`HasSurvivingShield`
  in `ShouldHealSingle`) — kein Schild/Barriere vorhanden.
- Getrennte Schwellenpaare `HealthSingleAbility(Hot)` vs.
  `HealthSingleSpell(Hot)` — alle acht Werte konsistent angehoben, nicht nur
  eine Zeile.
- Per-Action `ActionConfig.AutoHealRatio` (Default 0.8) — laut Nutzer nie
  verändert.
- `EmergencyGCD` (`CustomRotation_GCD.cs:72-79`) vor Heal-Branches — WHM
  PvE hat keine Override, Basisklasse macht in PvE nichts, keine
  `CurrentDutyRotation` für normales Dungeon.
- Manuelles Overriding durch den Spieler — Nutzer hat nichts gedrückt,
  RSR-Automatik hat Holy selbst gewählt.
- Upstream-Issue #1351 (NPC-Duty-Support-HP-Lesefehler) — laut Melder
  explizit nicht bei echten Mitspieler-Partys, hier echte Mitspieler.
- Keine weiteren passenden Upstream-Issues gefunden (durchsucht: heal, cure,
  "won't heal", "not healing", "GCD heal", holy — nur altes, 2024
  geschlossenes #70 mit gegenteiligem Symptom, nicht relevant).

**Noch nicht geprüft / fehlende Daten:** WHM
`HealSingleGCD`-Swiftcast+Raise-Kurzschluss (WHM_Reborn.cs ~336-364, nur
relevant wenn Rez ansteht — bei diesem Vorfall nicht erwähnt, daher
nachrangig). Kein Zugriff auf Live-Diagnosedaten (RSR-Debug-Statusfenster
zeigt `AutoStatus`/`MergedStatus`-Flags und tatsächlich verwendete
HP-Werte live) — ohne das keine weitere Eingrenzung per Code-Lektüre
möglich, da alle bekannten Codepfad-Kandidaten durchgeprüft sind.

**Nächster Schritt:** Beim nächsten Auftreten das RSR-Debug-Statusfenster
offen halten/Werte notieren (insbesondere ob `HealSingleSpell`-Flag gesetzt
war) — das ist der einzige noch verbleibende Weg, den Suchraum weiter
einzugrenzen.

KEIN Fix umsetzen, bevor eine Ursache tatsächlich belegt ist — Stand jetzt
ist nichts bestätigt.

### #55 `_lastHp` in `DataCenter.GetPartyMemberHPRatio` toter Code — Heil-Prädiktions-Cleanup greift nie

`RotationSolver.Basic/DataCenter.cs:1096/1115`: `_lastHp` wird deklariert und
per `TryGetValue` gelesen, aber NIRGENDS im Repo beschrieben
(`_lastHp[...] = ...` existiert nicht). Dadurch ist `lastHp` in
`GetPartyMemberHPRatio` immer `0`, und die Bedingung
`currentHp - lastHp == healedHp` (Zeile 1117, soll erkennen "die eigene
Heilung ist im echten HP-Wert angekommen, prädiktiven Eintrag entfernen")
kann praktisch nie wie beabsichtigt zutreffen. Folge: der prädiktive
HP-Ratio-Ausgleich (`Math.Min(1, (healedHp + currentHp) / maxHp)`, Zeile
1123) wird nicht wie vorgesehen durch echten HP-Abgleich beendet, sondern
nur dadurch, dass `DataCenter.HealHP` beim nächsten Self-Action-Effekt
(`Watcher.cs:211`) komplett neu zugewiesen wird — faktisch harmlos in der
Praxis (Fenster ohnehin nur `EffectTime`..`EffectEndTime`, typ. ~1.6-1.8s),
aber die vorgesehene Abgleichlogik ist funktional tot. Eigenständiger, von
der WHM-Heilsuppression unabhängiger Fund — nicht Ursache von #54 (Fenster
zu kurz für das gemeldete Muster), aber echter Bug, der bei Gelegenheit
bereinigt werden sollte (entweder `_lastHp` korrekt pflegen oder toten
Zweig entfernen).

### #57 Heilschwellen-Multiplikator `* 1.5f` global, hartcodiert, ohne Opt-out

`RotationSolver/Updaters/StateUpdater.cs` (`ShouldHealSelf`,
`ShouldHealSingle`): bei Weakness/Brink of Death wird die Heilschwelle mit
fest codiertem `1.5f` multipliziert. Das ändert das Heilverhalten JEDER
Klasse, ist weder Config noch benannte Konstante, und der Nutzer kann es
nicht abschalten. Gleiches gilt für `ShieldSurvivalHorizon`s `3f`-Fallback
und `25` (Co-Tank-HP-Schwelle in `CanProvoke`). Repo-Konvention für
Stellschrauben ist `Service.Config.X` bzw. `[RotationConfig]`.

### #58 25× dupliziertes `PlayerSyncedLevel() >= 98 ? 15f : 10f` über 14 Dateien

Die Dauer von Feint/Addle/Reprisal (Trait-Verlängerung 10s→15s) ist als
Ternary in 25 Aufrufen über 14 Dateien kopiert. Eine benannte Property in
`CustomRotation_OtherInfo.cs` würde alle 25 ersetzen und die Spielannahme
an EINER prüfbaren Stelle festhalten statt 25-fach verstreut. Analog:
`NumberOfHostilesInRange >= 4` 26× hartcodiert — inkonsistent dazu, dass
die entsprechende Healer-Schwelle inzwischen UI-konfigurierbar ist.

### #59 Helper-Muster nur in 1 von 15 Dateien angewandt

`SMN_Reborn.cs:154` kapselt die proaktive Bedingung in
`private bool TryAddleBeforeDamage(out IAction? act)` und ruft sie aus
DefenseArea- UND DefenseSingleAbility auf — das ist das richtige Muster.
Die anderen 14 Dateien duplizieren dieselbe Bedingung inline 2–4× pro
Datei (38 `BMRShouldRefreshBefore`-Aufrufstellen gesamt). Das Muster
existiert also im eigenen Patch-Satz und wurde nicht durchgezogen.

### #60 WHM/AST/SGE: drei Kopien mit bereits auseinandergelaufener Semantik

`WHM_Reborn.cs:338` (HealAreaGCD) und `:373` (HealSingleGCD) enthalten die
`GetHealthRatio() > RegenHeal`-Sicherung und `Target != null &&`;
`:426` (GeneralGCD) enthält BEIDES NICHT und nutzt stattdessen `?? true`.
Bei Null-Ziel verhalten sich die Pfade damit gegensätzlich, und der
GeneralGCD-Pfad castet Regen auch unterhalb der `RegenHeal`-Schwelle. Ob
das gewollt ist, ist nirgends dokumentiert — es ist der klassische
Copy-Paste-Drift (eine spätere Absicherung landete in 2 von 3 Kopien).
AST/SGE haben je ebenfalls 3 Kopien, auf denselben Drift prüfen.

### #61 Drei Implementierungen von "lebender Tank in der Party"

`CustomRotation_OtherInfo.PartyTank` (neu), `StateUpdater.AnyLivingTankInParty()`
(neu), `ActionTargetInfo.FindTankTarget()` (upstream) prüfen alle
`IsJobCategory(JobRole.Tank) && !IsDead` über `DataCenter.PartyMembers`.
Assembly-Grenze erklärt die Trennung nur teilweise — ein gemeinsamer
Helper in `DataCenter`/`TargetFilter` wäre möglich. Zusätzlich:
`CanProvoke` ruft `Svc.Objects.SearchById(target.TargetObjectId)` zweimal
für dieselbe ID in einem Per-Frame-/Per-Hostile-Pfad auf.

### #62 Kommentardichte 3,5× über Repo-Norm, teils Änderungshistorie im Quelltext

Gemessen: Repo-Baseline in den betroffenen Dateien 11,0 % Kommentarzeilen
(3517/31717 nicht-leere Zeilen); die Fork-Ergänzungen liegen bei 38,6 %
(487 Kommentar- zu 774 Codezeilen). Inhaltlich enthalten mehrere
Kommentare Änderungsbegründung/-historie statt Code-Erklärung (z.B.
StateUpdater: "a prior attempt at this without the job-scoped property did
exactly that and was reverted"; diverse Rotationen: "same dual-placement
pattern already used for DRK/GNB Reprisal and SMN/RDM/PCT/BLM Addle").
Das gehört in Commit-Message/AUDIT_LOG, nicht in den Quelltext — dort
veraltet es und ist für Upstream-Leser Rauschen.

## Wichtig für zukünftige Sessions

Diese Dateien (TODO.md, AUDIT_LOG.md) existieren nur auf dem Branch, auf
dem sie committet wurden. Falls sie fehlen, obwohl an diesem Repo
gearbeitet wird: das dem Nutzer explizit melden (siehe CLAUDE.md), nicht
stillschweigend neu anfangen. Beide Dateien bei Sitzungsbeginn lesen —
nicht nur TODO.md.

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

### #63 Sustain-HP-Boden: Asymmetrie WHM/AST/SGE und umgekehrte Vergleichsrichtung bei AST

Aus dem Critic-Loop zur Vereinheitlichung in `6b40600`. Zwei unwiderlegte
Punkte, beide nur im Spiel entscheidbar:

1. **Asymmetrie**: der proaktive Sustain hat bei WHM einen HP-Boden von 0.3
   (`RegenHeal`), bei AST 0.4 (`AspectedBeneficHeal`), bei SGE **keinen**.
   Entweder gehoert der Boden ueberall hin (dann fehlt er SGE), oder
   nirgends (dann sind WHM/AST zu restriktiv). Fuer SGE gibt es keine
   bestehende Einstellung zum Wiederverwenden — eine neue zu erfinden waere
   Scope Creep ohne Belegt, deshalb bewusst offen gelassen.

2. **Vergleichsrichtung bei AST**: derselbe Wert `AspectedBeneficHeal` wird
   im reaktiven Zweig als `GetHealthRatio() < AspectedBeneficHeal`
   (AST_Reborn.cs:537, "heile ihn, er ist verletzt") und im proaktiven
   Helfer als `>` ("pflege den HoT, er ist gesund") verwendet. Das ist
   nicht falsch — die Schwelle trennt sinnvoll beide Faelle —, aber fuer
   den Nutzer nicht erkennbar: wer den Wert hochzieht, bekommt mehr
   reaktive Heilung UND weniger proaktiven Sustain. WHM ist konsistent
   (beide Zweige `>`).

**Pruefbar im Spiel**: waehrend eines Wall-to-Wall beobachten, ob der
HoT-Refresh aussetzt, sobald der Tank unter die Schwelle faellt, und ob
das stoert. Relevant vor allem in Inhalten mit zwei lebenden Heilern, wo
`CanHealSingleSpell` wegen `GCDHeal == false` (ASTs Default!) falsch ist
und `GeneralGCD` deshalb auch mit verletztem Tank erreicht wird.

## Wichtig für zukünftige Sessions

Diese Dateien (TODO.md, AUDIT_LOG.md) existieren nur auf dem Branch, auf
dem sie committet wurden. Falls sie fehlen, obwohl an diesem Repo
gearbeitet wird: das dem Nutzer explizit melden (siehe CLAUDE.md), nicht
stillschweigend neu anfangen. Beide Dateien bei Sitzungsbeginn lesen —
nicht nur TODO.md.

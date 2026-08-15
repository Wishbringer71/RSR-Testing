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

### #54 WHM-Heilsuppression nach oGCD-Erschöpfung — Ursache NICHT bestätigt, Recherche läuft

Nutzer-Meldung, wörtlich verifizierte Fakten (keine Interpretation):
Schwellen konfiguriert auf >70% (ohne HoT) und >55% (HoT-Schwellenwert,
zweiter Config-Wert — KEIN in-Game-Ereignis, kein Mitheiler erwähnt).
oGCD-Heilung funktioniert zu Beginn, dann oGCDs aufgebraucht. Danach
feuern manaverbrauchende GCD-Heilsprüche trotz vollem Mana NICHT —
Partymitglieder fallen unter 50%, nicht nur kurzfristig. Tritt meist auf,
wenn keine oGCDs verfügbar sind.

Der bereits gefixte `AverageTTK`-Nullfallback (siehe AUDIT_LOG.md) erklärt
dies NICHT vollständig (nur ~2.5s-Fenster am Pull-Start, betrifft oGCD und
GCD gleichermaßen).

Bisher untersuchter, NICHT bestätigter Kandidat (Vorsicht — frühere Fassung
dieses Eintrags hatte hier fälschlich einen Mitheiler/HoT-Auslöser
unterstellt, den der Nutzer nie genannt hat — das war Fabrikation und ist
gestrichen):
`RotationSolver/Updaters/CancelCastUpdater.cs:70-77` (`shouldStopHealing`,
hinter `Service.Config.StopHealingAfterThresholdExperimental2`, Default
`false`, Configs.cs:713-714) bricht einen BEREITS LAUFENDEN GCD-Single-Heal
ab, wenn das Heilbedarf-Flag während des Casts wegfällt. Das erklärt nur ein
Szenario mit sichtbar startendem und dann abbrechendem Cast — nicht
zwangsläufig "Spell wird nie genutzt" (könnte auch bedeuten: Spell wird von
der Auswahllogik nie ausgewählt, anderer Codepfad in `StateUpdater`/
`CustomRotation_GCD`-Dispatch). Ob dieses Szenario überhaupt zutrifft, ist
ungeklärt.

Offene Rückfrage an Nutzer, bevor weitere Analyse sinnvoll ist: Wurde ein
Cast-Balken von Cure/Cure II/Regen beobachtet, der beginnt und dann
abbricht — oder hat WHM gar nicht erst versucht zu casten (z. B. nur
Auto-Attacke/Filler weitergenutzt, während HP fiel)? Diese Unterscheidung
entscheidet den weiteren Suchraum (Cast-Abbruch-Logik vs.
Aktionsauswahl-Logik) und darf nicht angenommen werden.

Noch nicht abschließend geprüfte Nebenkandidaten aus dieser Recherche
(keiner bestätigt, keiner verworfen): getrennte Schwellenpaare
`HealthSingleSpell`/`HealthSingleSpellHot` vs.
`HealthSingleAbility`/`HealthSingleAbilityHot` (StateUpdater.cs); per-Action
`ActionConfig.AutoHealRatio` (Default 0.8, ActionConfig.cs:106) als
zusätzlicher Ziel-Eligibility-Filter unabhängig von Job-Schwellen; WHM
`HealSingleGCD`-Swiftcast+Raise-Kurzschluss (WHM_Reborn.cs ~336-364, nur
relevant wenn ein Rez ansteht).

KEIN Fix umsetzen, bevor eine dieser Ursachen tatsächlich belegt ist —
Stand jetzt ist alles Kandidat, nichts bestätigt.

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

## Wichtig für zukünftige Sessions

Diese Dateien (TODO.md, AUDIT_LOG.md) existieren nur auf dem Branch, auf
dem sie committet wurden. Falls sie fehlen, obwohl an diesem Repo
gearbeitet wird: das dem Nutzer explizit melden (siehe CLAUDE.md), nicht
stillschweigend neu anfangen. Beide Dateien bei Sitzungsbeginn lesen —
nicht nur TODO.md.

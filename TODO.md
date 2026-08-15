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

### #54 WHM-Heilsuppression nach oGCD-Erschöpfung — Root Cause identifiziert, Rückfrage an Nutzer offen

Nutzer-Meldung: oGCD-Heilung funktioniert zu Beginn, aber sobald oGCDs
verbraucht sind, feuern manaverbrauchende GCD-Heilsprüche (Cure/Cure II)
trotz vollem Mana und korrekt hochgesetzter Schwellen NICHT — Partymitglieder
fallen unter 50% ohne Heilung. Der bereits gefixte `AverageTTK`-Nullfallback
(siehe AUDIT_LOG.md) erklärt dies NICHT vollständig (nur ~2.5s-Fenster am
Pull-Start, betrifft oGCD und GCD gleichermaßen).

Konkreter, code-verifizierter Mechanismus gefunden:
`RotationSolver/Updaters/CancelCastUpdater.cs:70-77` (`shouldStopHealing`) —
bricht einen laufenden GCD-Single-Heal-Cast (`ActionConfig.GCDSingleHeal`,
gesetzt bei WHM CurePvE/CureIiPvE, `WhiteMageRotation.cs:120,160`) sofort ab
(`uiState->Hotbar.CancelCast()`), wenn während des Casts das
`HealSingleSpell`/`HealAreaSpell`-Flag in `DataCenter.MergedStatus`
wegfällt — z. B. weil ein Mitheiler oder ein HoT-Tick das Ziel zwischenzeitlich
knapp über die Schwelle gebracht hat. Passt exakt zum gemeldeten Muster
(Mana bleibt voll, weil der Cast vor Abschluss/Mana-Abzug gecancelt wird,
nicht weil der Cast nie beginnt).

Einschränkung: Der Mechanismus hängt an
`Service.Config.StopHealingAfterThresholdExperimental2`
(`RotationSolver.Basic/Configuration/Configs.cs:713-714`) — Default `false`,
UI-Label "Stop single target GCD healing after reaching threshold. (EXTREMELY
Experimental)". Ob dieser Schalter beim Nutzer aktiv ist, ist aus dem Code
allein nicht verifizierbar — noch nicht bestätigt, nur als Kandidat mit
exakter Symptom-Übereinstimmung identifiziert. Rückfrage an Nutzer nötig,
bevor ein Fix konzipiert wird (keine voreilige Umsetzung gegen unbestätigte
Ursache).

Weitere, noch nicht abschließend geprüfte/verworfene Nebenkandidaten aus
derselben Recherche (niedrigere Priorität, nur bei Bedarf weiterverfolgen):
getrennte Schwellenpaare `HealthSingleSpell`/`HealthSingleSpellHot` vs.
`HealthSingleAbility`/`HealthSingleAbilityHot` (StateUpdater.cs); per-Action
`ActionConfig.AutoHealRatio` (Default 0.8, ActionConfig.cs:106) als
zusätzlicher Ziel-Eligibility-Filter unabhängig von Job-Schwellen; WHM
`HealSingleGCD`-Swiftcast+Raise-Kurzschluss (WHM_Reborn.cs ~336-364, nur
relevant wenn ein Rez ansteht).

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

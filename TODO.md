# TODO — nur offene Arbeit

Getrennt nach Defekt (Abweichung vom beabsichtigten Verhalten), technischer Schuld (bewusst eingegangener Kompromiss mit Auflösungsbedingung) und offener Arbeit. Je Eintrag der betroffene Personenkreis: **N** Endnutzer des Plugins · **R** Autoren abgeleiteter Rotationen, die `RotationSolver.Basic` als Paket beziehen · **U** Upstream-Pflege.

## Defekte

### Der Schadenseingang wird rechnerisch nur auf der Gegnerseite erfasst · N

**Vorgabe des Auftraggebers, vollständig in `docs/rotation-flow/08-mitigation-synergy.md`:** Der Schadenseingang einschließlich eingerechneter Schadensreduktion **und Mitigation** soll zu jedem Zeitpunkt bestimmte Grenzwerte nicht überschreiten. Dazu dienen Reflexion, The Blackest Night und die übrigen Minderungen des Tanks ebenso wie die Verlangsamung.

**Gemessen wird davon die Hälfte.** `HostileOutputPercent` rechnet die gegnerseitigen Drosselungen. Die persönlichen Minderungen des Tanks — Rampart, Bollwerk, Sentinel, Schattenwall, Vengeance, Bloodwhetting — gehen **nirgends** ein: `StatusHelper.RampartStatus` führt die Ids, wird aber nur als `StatusProvide` benutzt. `GetCurrentMitigationPercent` deckt die gruppenweiten Minderungen ab, ist aber für einen einzelnen bevorstehenden Treffer gebaut, nicht für den Dauerstrom.

**Was zum Bauen fehlt:** je Status ein Minderungssatz, belegbar aus den Wirktexten in `Action.resx`, und die Entscheidung, gegen welchen Grenzwert gemessen wird. Beides ist Voraussetzung, nicht Beiwerk.

**Die Barriere gehört in den Zähler, nicht in den Nenner** (Vorgabe des Auftraggebers, Konzept 08): Sie drosselt die Rate nicht, bewertet aber, ob der Tank überlebt und ob genug Zeit zum Heilen bleibt. Die gemeinte Größe ist Puffer geteilt durch Rate. **Beide stehen inzwischen:** der Puffer einschließlich Barriere in `GetEffectiveHp`, die Rate in `GetCorrectedTTK` — und genau ihr Quotient ist es, den `GetForecastSurvivingShare` seit A93 bildet und die Heilkette liest. Offen an diesem Punkt bleibt die **Restzeit der Barriere** (`HasSurvivingShield`, Defekt s. u.): Eine Barriere, die vor der Heilung ausläuft, kauft keine Zeit, und die Vorausschau skaliert den Schild heute mit, statt seine eigene Laufzeit zu prüfen.

**Der Weg ohne jede statische Vorgabe, und er ist der kleinste Eingriff:** `DataCenter.RecordedHP` führt eine Zeitreihe von Gesundheitsanteilen je Objekt-Id (1 Hz, 240 Einträge), und `ObjectHelper.GetTTK` wertet sie für **jede** Id aus — die Methode ist generisch. Gefüllt wird die Reihe nur aus `AllHostileTargets` (`TargetUpdater.UpdateTimeToKill`), deshalb liefert sie für Gruppenmitglieder `NaN`. Nimmt man die Gruppe mit auf, ist die Rate je Mitglied da, **netto nach allem** — Minderung, Mitigation, Barriere und Heilung eingerechnet, ohne eine einzige Liste. Der Grenzwert wird damit relativ: „Ist die Restzeit kürzer als die Zeit, die meine Heilung braucht?" Beide Seiten sind zur Laufzeit bekannt.

**Die Größe ist präventiv** (Richtigstellung C60): Bei 90 % Gesundheit meldet `GetTTK` den Tod acht Sekunden im Voraus — das ist der Raum für den vorbeugenden Eingriff, den der Auftraggeber verlangt. **Grenzen, gemessen:** blind für den **ersten** Treffer (`CheckSpan` 2,5 s, Abtastung 1 Hz — dafür bleibt die BossModReborn-Vorhersage zuständig); Trägheit (`GetTTK` mittelt über den ganzen Kampf, nicht über die letzten Sekunden — inzwischen nicht mehr nur benannt, sondern von `ScoreTtkForecast` gemessen und von `GetCorrectedTTK` herausgeteilt); keine Zuordnung (der Verlauf sagt, *dass* die Gesundheit fällt, nicht *warum*).

**`DataCenter.DPSTaken` ist die schwächere Alternative:** Sie misst ebenfalls netto, trägt aber **kein Ziel** (`DamageRec` hat nur Zeitpunkt und Anteil) und hat ein Fenster von fünf Millisekunden gegen ein Bild von rund sechzehn — sie sieht fast immer nichts. Upstream-Code, einziger Leser ist die Diagnoseanzeige.

**Die Richtung der Reaktion steht bereits fest: Heilung vor Minderung** (Vorgabe des Auftraggebers, Konzepte 08 und 10). Eine Grenzwertüberschreitung löst also zuerst Heilung aus; gemindert wird, wo die Heilung nicht reicht. Im Dispatch ist diese Reihenfolge in beiden Pfaden bereits gegeben.

**Die Beobachtung ist umgesetzt** (A91): Die Gruppe steht in `RecordedHP`, `GetTTK` antwortet für Gruppenmitglieder, und der Wert ist netto nach allem — Minderung, Mitigation, Barriere und Heilung eingerechnet, ohne Pflegeliste.

**Offen bleibt die Lage vor dem ersten Treffer** — hybride Lösung nach Vorgabe des Auftraggebers: Beobachtung trägt den laufenden Kampf, etwas anderes den Eröffnungsmoment (`GetTTK` liefert vor 2,5 s `NaN`). **Der Weg dorthin hat sich geändert:** Bisher stand hier die Hochrechnung aus Statussätzen, je Status ein Satz aus `Action.resx` — eine gepflegte Tabelle mit der bekannten Alterung. Konzept `13-aoe-damage-classification.md` erreicht dasselbe aus **beobachteten Einschlägen**: Ein angekündigter Cast mit bekanntem Schadenspotential sagt den ersten Treffer voraus, bevor er fällt. Die Statussatz-Tabelle ist damit nicht mehr die erste Wahl.

**Zweiter Teil, gemessen statt vermutet: die Auswertung.** `GetTTK` mittelt über den ganzen Kampf statt über die letzten Sekunden — für einen Gegner richtig, für ein Gruppenmitglied träge, und der Fehler geht in die gefährliche Richtung: Die gemeldete Restzeit ist zu lang, eine Regel darauf griffe zu spät. Dafür braucht es keinen externen Beobachter (A92): `ObjectHelper.ScoreTtkForecast` hält jede Sekunde die vorige Vorhersage gegen den tatsächlichen Verlauf, `GetCorrectedTTK` teilt den Fehler heraus.

**Der Verbraucher besteht (A93):** Alle Heilentscheidungen lesen die **vorausberechnete** Gesundheit — `GetForecastSurvivingShare` und die drei davon abgeleiteten Getter, hinter `HealAheadOfDamage`, Standard aus. Offen bleibt allein die Beobachtung im Spiel: ob der Fehlerfaktor überhaupt von 1 abweicht und ob der Vorab-Eingriff den Tank hält. Beides steht in der Diagnoseanzeige (Gesundheit jetzt → prognostiziert, Rohzeit, korrigierte Zeit, Faktor).

### Die Notfallheilungen der übrigen Heiler prüfen die Gefahr nicht · N

**Konzept:** `docs/rotation-flow/07-heal-target-priority.md`
Erfasst, nicht bearbeitet (A94). Die Vorgabe des Auftraggebers — eine Notfallmaßnahme nur bei Gefahr, sonst genügen HoT und kleinere Heilungen — ist bisher allein am Weißmagier umgesetzt (`WHM_Reborn`, `BenedictionNeedsThreat`). Dieselbe Bauform „`CanUse` **und** Ziel unter Schwelle" ohne jede Gefahrenprüfung tragen:

| Job | Aktion | Bemerkung |
|---|---|---|
| SGE | `TaurocholePvE` gegen `TaurocholeHeal` | kürzere Abklingzeit als Benediction, Verlust entsprechend kleiner |
| SCH | `ExcogitationPvE` gegen `ExcogHeal` | zusätzlich an `Recitation` gebunden, das den Verlust verteuert |
| AST | `EssentialDignityPvE` gegen drei gestaffelte Schwellen | trägt Ladungen, der Einzelverlust wiegt weniger |

**Warum nicht mitbearbeitet:** Nach der Prioritätsregel folgt die Bearbeitung dem Nutzungsprofil des Auftraggebers, nicht der Fundlage. Belegt gespielt sind Weißmagier und Dunkelritter; für die drei übrigen Heiler liegt weder eine Meldung noch eine Beobachtung vor. `ObjectHelper.IsUnderThreat` ist allgemein gebaut und von jeder dieser Stellen lesbar — die Übertragung ist je Aktion eine Zeile plus Einstellung.

**Auflösungsbedingung:** eine Spielbeobachtung am Weißmagier, dass die Regel trägt, oder die Freigabe des Auftraggebers für die übrigen Heiler. Dabei ist je Aktion neu zu bewerten, ob die Abklingzeit den Vorbehalt überhaupt rechtfertigt — bei Essential Dignity mit Ladungen ist das offen.

**Zweiter offener Punkt derselben Familie:** `IsUnderThreat` ist `internal`. Rotationen im Baum lesen es, abgeleitete Rotationen aus dem Paket `RotationSolver.Basic` (Betroffenenkreis R) nicht. Ob es öffentlich werden soll, ist erst zu entscheiden, wenn die Größe im Spiel bestätigt ist — eine öffentliche Signatur ist danach ein Vertrag.

### Die Flächenheilung entscheidet weiter nach Pegel statt nach Rate · N

**Konzept:** `docs/rotation-flow/07-heal-target-priority.md`, `docs/rotation-flow/08-mitigation-synergy.md`
Erfasst, nicht bearbeitet (A93). `HealthAreaAbility`/`HealthAreaSpell` werden gegen `DataCenter.PartyMembersAverHP` und `LowestPartyMembersAverHP` verglichen — dieselbe Verwechslung von Stand und Zufluss, die für die Einzelheilung mit der Vorausschau behoben ist. Die Flächenheilung fällt daher weiterhin zu spät, wenn die Gruppe schnell fällt.

**Warum nicht mitbehoben:** Die Größen stammen aus `DataCenter.ComputePartyHpStats` und speisen fünf öffentliche Eigenschaften mit **83 Lesern außerhalb der Heilkette**, darunter Schwellen in fremden `ExtraRotations` (Beiruta, Churin), die auf den heutigen Wert eingestellt sind. Eine Vorausschau dort hinein zu legen änderte still das Verhalten aller 83 Stellen und wäre nicht mehr der kleinste wirksame Eingriff.

**Auflösungsbedingung:** aufzugreifen, sobald die Einzelheilung im Spiel beurteilt ist. Dann ist der Zuschnitt zu wählen, der die fremden Leser nicht trifft — eine eigene, vorausberechnete Kenngröße neben den bestehenden, gelesen allein von den beiden Flächenschwellen.


### `searing_light_coverage.py` misst über das Fenster hinaus, das es zu messen vorgibt · —

`simulate(…, window=(lo, hi))` soll die Abdeckung **innerhalb** eines Zeitfensters messen. Der Zähler wird aber auch außerhalb hochgezählt — der `elif buff_until > t: covered += STEP` neben dem Fensterzweig —, geteilt wird dagegen durch die Fensterlänge `(hi - lo)`. Das Ergebnis ist die Gesamtabdeckung des Kampfes, gestreckt um das Verhältnis Kampflänge zu Fensterlänge. Sichtbar an der Ausgabe selbst: Die Einschwingtabelle meldet 332 %, die Ausfalltabelle 210 bis 542 % — Abdeckungsanteile über 100 % sind nicht deutbar.

**Betroffen sind drei Auswertungen, alle mit `window=`:** die Einschwingtabelle (Anfang gegen Ende des Kampfes), die Ausfalltabelle (ein Beschwörer fällt drei Minuten aus) und der Selbsttest, der prüft, dass die abschreibende Fassung unter Ausfall nie schlechter ist als die buchführende. Der Selbsttest bleibt gültig, weil beide Seiten gleich verzerrt sind — aber er prüft nicht, was sein Kommentar sagt: verglichen wird die Abdeckung über den ganzen Kampf, nicht die im Ausfallfenster. Genau die Bauform „Test misst ein Surrogat statt der gemeinten Eigenschaft".

**Nicht betroffen ist jede Zahl, die in einem Dokument steht.** Alle Tabellen in `docs/rotation-flow/12-searing-light-stacking.md` stammen aus Aufrufen ohne `window`; dort ist `lo, hi = 0, fight`, und der fehlerhafte Zweig kann nicht greifen. Nachgerechnet: Die zwanzig Werte der beiden Abdeckungstabellen des Konzepts sind heute Ziffer für Ziffer reproduzierbar, einschließlich der V4-Spalte, die der Bericht nicht mehr druckt (Modus `anytime`). Die Einschwingzahlen sind in keinem Dokument verwendet.

**Behebung:** den `elif`-Zweig streichen, damit außerhalb des Fensters nicht gezählt wird; der Selbsttest ist danach auf das Ausfallfenster zu schärfen, sonst deckt er die Rückkehr des Fehlers nicht ab. **Empfehlung: beheben** — es ist ein Prüfmittel, und ein Prüfmittel, das eine undeutbare Zahl druckt, entwertet auch seine richtigen.

### Fänge von `AccessViolationException`, die im gemeinten Fall nicht greifen · N, U

`DataCenter.cs`, unter anderem `:1316`, `:1402`, `:1534`, `:1803`. Muster überall gleich: ein nativer Lesezugriff über ein Dalamud-Objekt steht in einem `try`, dessen `catch (AccessViolationException)` den Absturz abfangen soll. Die Überschrift nannte zuvor 19 Fänge; gezählt waren die Zeilen, die den Ausnahmenamen **erwähnen**, nicht die Fänge selbst — ein Surrogat, das drei Kommentarzeilen mitzählte. `grep -c "catch (AccessViolationException"` beziffert den Bestand jederzeit.

**Der Fang greift genau dann nicht, wenn er gebraucht wird.** Microsoft dokumentiert für .NET Core: „corrupted-process-state exceptions cannot be handled by managed code", die Laufzeit liefert sie nicht an verwaltete Handler aus, und `HandleProcessCorruptedStateExceptionsAttribute` ist obsolet und wird ignoriert. Die Doku zu `AccessViolationException` präzisiert, dass der `catch` nur greift, solange die Verletzung **innerhalb** des von der Laufzeit reservierten Speichers auftritt — bei einem freigegebenen Spielobjekt ist sie das nicht.

Upstream hat dieselbe Klasse in 7.5.6.3 an vier Stellen aufgelöst (`ObjectHelper.IsEnemy`, `FindEnemyPositional`, `GetFaceVector`, dazu `RSCommands_Actions` und `StateUpdater`) und dort `IsValid()` sowie `Address != nint.Zero` **vor** den nativen Zugriff gesetzt. Die Stellen in `DataCenter.cs` sind dabei nicht mitgegangen.

**Empfehlung:** dasselbe Muster nachziehen, nicht die Fänge entfernen — ein `catch`, der nie feuert, ist harmlos, der fehlende Vorab-Test ist es nicht. Vorher zu klären: ob `PartyMembers` und die Feindlisten überhaupt freigegebene Objekte führen können oder ob sie je Rahmen neu erhoben werden; trifft Letzteres zu, ist die Klasse hier gegenstandslos und die Fänge sind der eigentliche Befund.

### `UseSummonsAndTrances`: eine der beiden Searing-Light-Kopplungen ist unerreichbar · N

**Konzept:** `docs/rotation-flow/12-searing-light-stacking.md`
`SMN_Reborn.cs:478` ruft `SummonBahamutPvE.CanUse(out act)` ohne Vorbedingung. `:487` ruft denselben Ausdruck mit einer zusätzlichen Bedingung davor und ist damit eine strikte Teilmenge — **beweisbar toter Code**. `:491` steuert die Solar-Beschwörung über `!SearingLightPvE.Cooldown.IsCoolingDown` und ist nur erreichbar, wenn `CanUse` in derselben Lage falsch liefert.

Welche der beiden Zeilen tot ist, hängt daran, ob Summon Bahamut auf Stufe 100 spielseitig zu Summon Solar Bahamut umgewandelt wird; RSR ruft über `AdjustedID`, die Antwort steht nicht im Repository. Das ist kein Aufräumfall: Wird `:478` entfernt, wird die Kopplung aus `:491` wirksam, und dann greift die Zündregel aus A78 in die Beschwörungswahl ein. Erst messen, dann anfassen — Beobachtungspunkt ist, ob Solar Bahamut alle 120 Sekunden kommt.

### Wiederbelebung: vier Eingriffe des Zweigs sind weiter ungemessen · N, R

**Konzept:** `docs/rotation-flow/11-raise-dispatch.md`
**Der Kerndefekt ist behoben und im Spiel bestätigt** — Beobachtung, Erklärung und Nachweis in
`AUDIT_LOG.md` A73. Dieser Punkt führt nur noch, was der Spieltest nicht abdecken konnte.

Vier Eingriffe auf `claude/raise-swiftcast-weave-2` sind nur unter Einstellungen wirksam,
die abseits der Vorgabe liegen, und wurden deshalb mit dem Kernpfad nicht mitgetestet:

- **Phönixfeder** samt Zieleignung über den Item-Status und Stufenprüfung der Rezzereigenschaft
  (A59, A60). `UsePhoenixDown` ist ab Werk **aus**; ohne Einschalten passiert nichts.
- **Hartwirk-Korrektur** (A56). Greift nur bei abgeschaltetem `RaisePlayerBySwift` — dem Fall, in dem
  vorher überhaupt nicht wiederbelebt wurde.
- **Bezugsmenge der Nur-Heiler-Modi** (A58). Nur bei `HardCastOnlyHealer` und
  `HardCastOnlyHealerSwiftCooldown`.
- **Spontanitäts-Vorbehalt in `HardCastOnlyHealer`** (A84). Derselbe Modus wie der Punkt darüber,
  aber in einer Beobachtung davon trennbar: Der Vorbehalt zeigt sich daran, dass bei **bereiter**
  Spontanität nicht mehr hartgewirkt wird, die Bezugsmenge daran, **welche** Heiler zählen.

**Auflösungsbedingung:** je Eingriff eine Beobachtung unter der zugehörigen Einstellung. Für die
Phönixfeder zusätzlich eine Gruppe ohne lebenden Rezzer, weil die Bedingung sonst nicht greift.

**Bewertung:** kein Defektverdacht, sondern offener Nachweis. Die Wirkketten sind im Code
nachvollzogen; was fehlt, ist die Bestätigung im Spiel.

### `H2` bleibt im Modus `PartyAndAllianceHealers` wirkungslos · N

**Konzept:** `docs/rotation-flow/07-heal-target-priority.md`, `docs/rotation-flow/11-raise-dispatch.md`
`TargetUpdater.GetPriorityDeathTarget`. Der Sonderfall `if (raiseType == RaiseType.PartyAndAllianceHealers && deathHealers.Count > 0) return deathHealers[0];` steht **vor** der Umkehrung der vier Listen durch `Service.Config.H2`. In allen anderen Modi dreht diese Einstellung die Reihenfolge, in diesem einen nicht.

Ohne Wirkung auf die Frage, *ob* wiederbelebt wird — nur darauf, *welcher* von mehreren toten Heilern zuerst drankommt. **Auflösung:** den Sonderfall hinter die Umkehrung ziehen. **Nicht im laufenden Vorgang behoben,** weil der Zweig bereits mehrere ungemessene Eingriffe am Wiederbelebungspfad trägt (Punkt oben); ein weiterer verschlechtert die Auswertbarkeit des Spieltests, ohne dass diesem Punkt Dringlichkeit zukäme.

### Die Aufzählung der Wiederbelebungsaktionen im Einschiebezweig veraltet · N, R

**Konzept:** `docs/rotation-flow/11-raise-dispatch.md`
`CustomRotation_Ability.cs` prüft `nextGCD.IsTheSameTo(true, RaisePvE, EgeiroPvE, ResurrectionPvE, AscendPvE)`. Verraise des Rotmagiers und Angel Whisper des Blaumagiers fehlen, obwohl beide Rotationen `Raise` setzen — dieselbe Alterungsursache wie die Hauptursache des Wiederbelebungsdefekts: eine handgepflegte Liste statt der vorhandenen Fähigkeitsprüfung.

**Derzeit folgenlos,** weil der zweite Zweig derselben Bedingung (`RaisePendingAndCastable`) die Liste nicht braucht und über `Raise` geht. Der Punkt bleibt, weil die Liste beim nächsten Rezzer-Job erneut still falsch wird. **Auflösung:** den Vergleich gegen `Raise` führen statt gegen die Aufzählung.

### `SwiftcastBuffer` hat keinen Leser, und ihre Absicht ist überholt · N

`Configs.cs:1005` definiert die Einstellung (0,6 s, eigene Oberfläche, eigene Dokumentation „how early before next GCD should RSR use swiftcast for raise"). Eine Volltextsuche über den Baum findet genau diese eine Fundstelle: Sie wird nirgends gelesen.

Sie ist nicht nur unverbunden, sondern in ihrer dokumentierten Bedeutung unerfüllbar geworden. Sie besagt, Spontanität solle erst fallen, wenn nur noch `SwiftcastBuffer` Restzeit auf dem GCD liegt — bei 0,6 s liegt dieses Fenster fast vollständig, bei 0 vollständig in dem Bereich, den `RSCommands_Actions.DoAction` für Fähigkeiten sperrt. Sie zu verdrahten hieße, den in `docs/rotation-flow/11-raise-dispatch.md` behobenen Defekt an einer zweiten Stelle neu zu bauen; deshalb ist sie bei der dortigen Behebung bewusst unangetastet geblieben.

**Auflösung:** entweder entfernen — dann ist zu prüfen, ob der Name in gespeicherter Nutzerkonfiguration liegt und ein Migrationspfad nötig ist — oder als **Untergrenze** im Einschiebefenster neu definieren, also „wie weit oberhalb der Sperre darf Spontanität frühestens fallen". Die zweite Lesart erhält die Absicht des Autors und ist mit der Sperre vereinbar. Beides ist eine Entscheidung über Nutzerkonfiguration und gehört nicht in den Behebungsvorgang.

### `InterruptDelay` und `ProvokeDelay` haben keinen Leser · N

`Configs.cs:1057` und `:1061`. Beide sind `Vector2` mit Vorgabe `(0,5 s; 1 s)`, eigener Beschriftung und Wertebereich — und versprechen damit eine Zufallsverzögerung vor dem Unterbrechen beziehungsweise vor Provoke. Gelesen werden sie nirgends.

Die Klasse ist belegt, weil die beiden Geschwister derselben Bauart **gelesen** werden: `RaiseDelay2` und `EsunaDelay` speisen die `ObjectListDelay`-Instanzen in `TargetUpdater.cs:13-15`. Für Provoke- und Unterbrechungsziele gibt es keine solche Instanz; `TargetUpdater.cs:43-46` ermittelt beide ohne jede Verzögerung.

**Nicht verdrahtet, und das ist keine offene Entscheidung, sondern das Ergebnis der Regeln.** Drei Gründe, die zusammen nur einen Schluss zulassen:

1. *Es gibt keinen Migrationsweg.* `Configs.Migrate` ist keine Migration, sondern ein Zurücksetzen: bei abweichender Version `return new Configs()`. Eine geänderte Vorgabe erreicht deshalb nur Neuinstallationen; wer das Plugin schon benutzt, hat `(0,5; 1)` in seiner gespeicherten Konfiguration stehen und bekäme die Verzögerung beim Verdrahten tatsächlich eingeschaltet.
2. *Damit verletzt jede Verdrahtung die Feature-Toggle-Regel*, die für eine Änderung ohne Nachweismöglichkeit das bisherige Standardverhalten als Vorgabe verlangt. Das bisherige Verhalten ist „keine Verzögerung", und es lässt sich ohne Migration nicht erhalten.
3. *Der Nutzen liegt außerhalb der Zielrichtung.* Die Verzögerung dient der Tarnung, nicht der Kampfwirkung. Bei der Unterbrechung wirkt sie sogar gegen den Zweck der Aktion: bis zu eine Sekunde kann das Fenster des Zaubers verbrauchen, der gestoppt werden soll.

**Auflösungsbedingung:** Sobald ein echter Migrationsmechanismus existiert — einer, der einzelne Felder umstellt, statt die Datei zu verwerfen —, ist zu verdrahten und die Vorgabe zugleich auf `(0; 0)` zu ziehen. Vorher ist die wirkungslose Einstellung das kleinere Übel gegenüber einer ungefragt eingeschalteten Verzögerung.

### `TargetColor` wird nicht gelesen, und ihr Elternverweis zeigt auf sie selbst · N

`Configs.cs:1254-1255` trägt `[UI("Target color", Parent = nameof(TargetColor))]` — die Eigenschaft nennt sich selbst als Elternschalter. Die Zeile darüber, `TeachingModeColor`, zeigt die richtige Bauart mit `Parent = nameof(TeachingMode)`; Kennzeichen eines Klons ohne Anpassung (Parnas, *Ignorant Surgery*).

**Der Elternverweis ist folgenlos, entgegen der ersten Einschätzung.** `SearchableCollection.cs:45` nimmt ausschließlich `CheckBoxSearch` in die Elternliste auf, also boolesche Einstellungen. `TargetColor` ist ein `Vector4`, landet nie darin, der `TryGetValue` schlägt fehl, und der Eintrag wird auf oberster Ebene einsortiert. Kein Absturz, keine Rekursion in `GetParent`, nur eine Einrückung, die fehlt.

**Der wirkliche Befund ist der fehlende Leser:** Die Farbe wird im gesamten Baum nicht gelesen, wirkt also ohnehin nicht.

**Nicht behoben,** weil die Behebung voraussetzt zu wissen, wo die Farbe gezeichnet werden sollte und welcher Schalter der gemeinte Elternteil ist. Beides geht aus dem Code nicht hervor, und eine erfundene Zuordnung wäre schlechter als der sichtbare Rest. Technische Schuld, kein Defekt mit Wirkung.

### `IBaseAction.IgnoreClipping` wird geschrieben und nirgends gelesen · N, R

`IBaseAction.cs:14` definiert das Flag, `CustomRotation_Invoke.cs` setzt es an sechs Stellen (`:212`, `:228`, `:238`, `:243`, `:257`). Kein einziger Leser im gesamten Baum. Der Name benennt genau den Mechanismus, der beim Wiederbelebungsdefekt gefehlt hat: die Anti-Clipping-Sperre für einen Einzelfall aufheben.

Nicht behoben, weil der Wirkungsbereich den Vorgang sprengt. Das Flag wird in `Invoke` breit gesetzt, auch für Fälle ohne Bezug zur Wiederbelebung; ein Leser in `RSCommands_Actions.DoAction` würde die Sperre praktisch überall aushebeln und damit das Verhalten jeder Rotation ändern. Die Behebung verlangt zuerst eine Entscheidung, für welche Aktionen das Flag gelten soll.

**Auflösungsbedingung:** eine Erhebung, welche der sechs Setzstellen eine Ausnahme rechtfertigen, und eine Engführung des Flags auf diese.

### Beschwörer: Searing Light bei mehreren Beschwörern — im Spiel zu bestätigen · N

Umgesetzt und in `AUDIT_LOG.md` A78 und A89 nachgewiesen, soweit statisch möglich; Konzept in `docs/rotation-flow/12-searing-light-stacking.md`. Der Stand im Code ist V8: `mayFireSearingLight` fordert die Burstphase in Solar Bahamut oder — bei einem zweiten Beschwörer — die große Beschwörung, und weicht bei **allen** belegten Phasen auf Ifrit aus (`SMN_Reborn.cs:227-229`). V7 ist damit zurückgebaut; das frühere `|| !HasAnySearingLight` steht nicht mehr in der Zündbedingung. Offen sind zwei Beobachtungen, die nur im Spiel zu machen sind, beide mit einer Gruppe aus mindestens zwei Beschwörern:

**Kommt Solar Bahamut weiterhin alle 120 Sekunden?** Das entscheidet die Kopplungsfrage aus dem Defekt zu `UseSummonsAndTrances` weiter oben. Rutscht der Takt, trägt `burstInSolar` nicht mehr, und die Zündbedingung fällt auf den Zweig für den zweiten Beschwörer zurück.

**Zünden mehrere Beschwörer beim Buffende gleichzeitig?** Das Modell schreibt sequenziell zu und bildet das nicht ab. Der Fall besteht heute schon und sollte seltener werden, nicht häufiger; belegt ist das nicht.

**Erfasst, nicht bearbeitet:** `ChurinSMN.cs:1015` trägt denselben V1-Befund; beim Zündfenster ist die fremde Rotation bereits weiter (`:948` nutzt `BahamutBurst`), allerdings ohne Gruppenprüfung.


### ChurinDNC wertet die BMR-Downtime ohne Vorzeichenprüfung aus · N, U

`ChurinDNC.cs:777-843` (Upstream) liest `BMRNextDowntimeIn`/`-EndIn` ohne Vorzeichenprüfung. BossModReborn liefert diese Werte als `(Aktivierung − jetzt)`, sie sind während einer laufenden Downtime also negativ, und die Rotation kann „Downtime läuft" nicht von „Downtime kommt gleich" unterscheiden: `if (BMRNextDowntimeIn >= 15f) return;` kehrt dann nicht zurück, und die folgende `<`-Bedingung ist immer erfüllt. Die Normalisierung der Schadensvorhersagen ist erledigt (AUDIT_LOG A11); hier wäre ein Filter falsch, weil das Vorzeichen die Information trägt.

Nicht behoben, weil die Absicht dieser fremden Rotation ohne ihren Autor nicht belegbar ist und eine Änderung ohne Spieltest nicht abzusichern wäre. Auflösung: Rückfrage an den Upstream-Autor oder Laufzeitbeobachtung.

**Empfehlung: nicht bearbeiten.** Fremde Rotationsdatei, und die Behebung verlangt genau die Richtungsentscheidung, die ohne den Autor nicht zu treffen ist. Der Punkt bleibt erfasst; Adressat ist der Upstream.

### Status-Einstellungen auf der falschen Seite der Aktion · N, U

`ActionSetting` liest `StatusProvide` und `StatusNeed` gegen `Player.Object`, `TargetStatusProvide` und `TargetStatusNeed` gegen das Ziel. Eine ID, die der Spieler nie tragen kann, macht die erste Sperre wirkungslos und die zweite dauerhaft blockierend. `scan11.py` erhebt die Klasse; nach der Behebung der beiden Blaumagier-Stellen (A32) bleiben vier Fundstellen, deren richtige Auflösung PvP- beziehungsweise Bozja-Verhalten voraussetzt.

- `RedMageRotation.cs:756` und `:763` (PvP): `StatusProvide` trägt 3238 und 3239, die Schadenswirkungen auf dem **Ziel**; die Barrieren auf dem Spieler sind 3235 und 3236, und die Nachbarzeile `:749` greift mit 3234 auf diese Hälfte zu. Die Feldwahl ist damit belegbar falsch — welche Behebung richtig ist, aber nicht: Nach dem Blaumagier-Befund ist ebenso denkbar, dass die **greifende** Zeile (Riposte) der Defekt ist, weil eine Barrieren-Sperre die feste Kombo Riposte → Zwerchhau → Redoublement → Scorch unterbrechen kann. Beide Lesarten sind ohne PvP-Beobachtung nicht zu trennen.
- `ScholarRotation.cs:497` (PvP, Deployment Tactics): `StatusProvide` trägt `Biolysis_3089`, wirkungslos. Als `TargetStatusProvide` gelesen widerspräche es dem `TargetStatusNeed` derselben ID zwei Zeilen darüber und machte die Aktion unbenutzbar; ersatzloses Streichen ist daher die wahrscheinliche Auflösung. `SCH_Default.PVP.cs:30` hat sich mit einer eigenen `!IsLastAction`-Sperre beholfen.
- `BozjaRotation.cs:368` (Lost Paralyze III): `StatusProvide` trägt `Paralysis`, den Ziel-Debuff. Hier ist die Sperre fachlich plausibel — der Zauber hat keinen nennenswerten anderen Zweck und Lost Actions haben begrenzte Ladungen —, aber die Verschiebung aktiviert eine Sperre, die nie gegriffen hat.
- `BozjaRotation.cs:140` (Lost Excellence): `StatusNeed = [Weakness]`. Anders als die übrigen wirkt diese Prüfung, weil der Spieler `Weakness` tragen kann; sie beschränkt Lost Excellence auf den geschwächten Zustand. Ein Zusammenhang zwischen beiden ist nicht erkennbar — fachliche Frage, kein Seitenfehler.

**Auflösungsbedingung:** je Fundstelle eine Beobachtung im betreffenden Inhalt oder eine Rückfrage an den Upstream-Autor. Alle vier stammen aus Upstream und sind dort unverändert.

**Empfehlung: liegen lassen.** Alle vier verbliebenen Fundstellen liegen in PvP oder Bozja, also außerhalb des Nutzungsprofils, und jede verlangt eine Richtungsentscheidung, die eine Beobachtung im jeweiligen Inhalt voraussetzt. Die Klasse ist vollständig erhoben und durch `scan11.py` gegen Rückfall gesichert — das ist der Zweck der Erfassung, die Bearbeitung ist es hier nicht.

### Die erhöhte Heilwirkung unter Schutzwall wird nirgends gelesen · N

**Konzept:** `docs/rotation-flow/07-heal-target-priority.md`
Belegt: `Status.resx` führt `Rampart_1978` — die Form, die ein Tank ab Stufe 94 trägt, eingegrenzt auf PLD WAR DRK GNB — mit „Damage taken is reduced **while HP recovered via healing actions is increased**". Die Grundformen 71 und 1191 sagen nur „Damage taken is reduced". Der Auftraggeber gibt die Erhöhung mit 15 % an; die Spieldaten nennen keine Zahl, wie bei jedem merkmalsabhängigen Wert.

**Gelesen wird die Wirkung nirgends.** `Rampart_1978` steht allein in `StatusHelper.RampartStatus`, und deren zwei Leser — `StatusProvide` der Tank-Rotationen und `HasMajorMitigation` — fragen nach Überlappung, nicht nach Heilwirkung. Die Gegenrichtung ist dagegen bekannt: `HpRecoveryDown` wird an drei Stellen im `StateUpdater` geprüft, allerdings nur im Sonderfall `IsInWindurst`.

**Kandidat, kein Defekt.** Es gibt derzeit keine Entscheidung im Baum, die davon abhinge: RSR entscheidet über Heilung an HP-Schwellen, nicht an Heilmengen, und eine um 15 % stärkere Heilung ändert nicht, **ob** geheilt werden muss.

**Was sie ändert, ist die Menge, nicht die Dringlichkeit** — und die frühere Fassung dieses Eintrags hat beides verwechselt. Sie schloss, ein Tank unter Schutzwall brauche die Heilung „weniger dringend". Das ist derselbe Fehlschluss wie bei der Schildanrechnung: Ein Tank bei 40 % steht bei 40 %, ob Schutzwall läuft oder nicht. Die stärkere Wirkung heißt, dass **eine Heilung** ihn weiter hochbringt, nicht dass er sie später braucht. Der einzige Fall, in dem die Kenntnis etwas einbrächte, ist die Wahl **welcher** Heilung: Unter Schutzwall genügt vielleicht das billigere oGCD, wo sonst der Zauber nötig wäre. Eine Schwelle zu verschieben ist es nicht.

**Auflösungsbedingung:** die Entscheidung über die Schildanrechnung. Fällt sie für eine Anrechnung des Schutzzustands aus, gehört die Heilverstärkung in dieselbe Rechnung; bleibt es beim Upstream-Verhalten, ist dieser Punkt gegenstandslos. Vorher zu klären wäre die Herkunft der 15 %.

### Der erzeugte Merkmalssatz enthält keine Rollenmerkmale · N, R

`TraitRotationGetter.AddToList` verwirft jedes Merkmal mit `item.ClassJob.RowId == 0`. Rollenmerkmale sind keiner einzelnen Klasse zugeordnet und fallen damit sämtlich heraus: Im erzeugten `Rotation.resx` findet sich kein `EnhancedReprisalTrait`, `EnhancedRampartTrait`, `EnhancedSecondWindTrait`, `EnhancedSwiftcastTrait`, `EnhancedFeintTrait`, `EnhancedAddleTrait` — null Treffer für jedes davon.

**Wirkung:** Wo eine Rollenaktion durch ein Merkmal aufgewertet wird, muss die Stufe als Zahl im Code stehen, statt vom Merkmalsobjekt gelesen zu werden. Zwei Stellen tun das bereits, beide mit 98 für dieselbe Aufwertung (`MitigationDebuffDuration`, `EnhancedReprisalLevel`), inzwischen über eine gemeinsame Konstante.

**Nicht sofort behoben:** Die Änderung liegt im Generator, der nur mit installiertem Spiel läuft, und sie vergrößert den erzeugten Satz um eine ganze Kategorie — Wirkungsbereich und Nutzen sind vor dem Eingriff zu erheben. Zu klären ist außerdem, ob `ClassJob.RowId == 0` tatsächlich das Kennzeichen von Rollenmerkmalen ist oder nur eines von mehreren Merkmalen ohne Klassenbezug.

### `CanUse` als Prüfung, nicht als Wahl — mit Zuweisung als Nebenwirkung · N, R

**Konzept:** `docs/rotation-flow/03-universal.md`
`ShouldStretchHolyStun` und `ShouldHoldHolyWhilePackSlowed` fragen beide `DiaPvE.CanUse(out _) || AeroIiPvE.CanUse(out _) || AeroPvE.CanUse(out _)`, um die Ersatzgarantie zu prüfen. `CanUse` weist dabei `Target` zu — dieselbe Nebenwirkung, die beim Wiederbelebungspfad eine eigene Vorkehrung nötig gemacht hat (`RaisePendingAndCastable` sichert und stellt den Zielüberschreiber wieder her, A56).

**Hier bislang folgenlos:** Wird der DoT anschließend tatsächlich gewirkt, ruft der Schadenszweig `CanUse` erneut und setzt das Ziel neu; wird Sanctus gewirkt, bleibt ein Ziel an einer Aktion stehen, die niemand liest. Belegt ist die Folgenlosigkeit allerdings nicht.

**Umfang, erhoben statt geschätzt: 120 Fundstellen in 25 Dateien** — `BeirutaRDM` 16, `SGE_Reborn` 11, `NIN_Reborn` 10, `BeirutaSGE` 10, dazu `CustomRotation_Ability`, `CustomRotation_GCD`, `CustomRotation_Items` und die UI. Das Muster ist damit nicht die Ausnahme, die diese Rotation eingeführt hat, sondern die Hausform des gesamten Baums; die beiden Sanctus-Stellen sind zwei von 120. Das ändert die Frage: Nicht „sollen diese zwei Stellen anders gebaut werden", sondern „hat der Baum eine seiteneffektfreie Prüfung nötig".

**Vor einer Änderung zu klären:** ob RSR eine seiteneffektfreie Prüfung anbietet. Gibt es keine, ist die Frage, ob eine solche eingeführt werden soll — mit einem Wirkungsbereich über alle Rotationen, die `CanUse` als Prüfung benutzen, und damit auch über die abgeleiteten Rotationen als Paketnutzer.

### Betäubungsstreckung von Sanctus: Voreinstellung aus, Wirkung unbeobachtet · N

**Konzept:** `docs/rotation-flow/08-mitigation-synergy.md`
`StretchHolyStun` ist voreingestellt aus, weil die Wirkung ohne Laufzeitbeobachtung nicht zu belegen war. Der **Mitigationsgrund** derselben Regel ist umgesetzt und voreingestellt an (`ShouldHoldHolyWhilePackSlowed`); ihre heutige Fassung ist die Anteilsregel des Auftraggebers — mehr als die Hälfte der Gegner im Wirkbereich verlangsamt **und** mindestens `HoldHolyMinSlowedHostiles` betroffen, dazu Betäubungsspielraum und eine Schranke für den Restausstoß (A90, C59; die frühere Leistungsrechnung aus A79 ist damit abgelöst). Der **Betäubungsgrund** — Sanctus einen GCD aussetzen, solange die eigene Betäubung noch läuft, statt sie zu überschreiben — wartet weiter auf die Beobachtung, ob die Streckung im Spiel eintritt.

**Auflösungsbedingung:** eine Beobachtung, ob Sanctus in eine laufende Betäubung hinein gewirkt wird und ob die Streckung die vom Modell gerechneten 5,5 auf 7,0 Sekunden bringt. Der Auftraggeber hat die Einstellung eingeschaltet, um überhaupt testen zu können; offen ist allein, ob die Streckung messbar eintritt — und danach, ob die **Voreinstellung** im Code folgen soll.

### Walking Dead: zwei Stellen wirken gegen die gestaffelte Unterstützung · N

Die Regel steht in `docs/rotation-flow/09-tank-selfprotection.md`, Abschnitt „Living Dead ist ein Zeitproblem": am Anfang der Phase leicht mit einem HoT unterstützen, am Ende die Lücke zu 100 % schließen, falls die Selbstheilung nicht reicht. Dieser Punkt führt nur, was im Code dagegen steht.

- **Der HoT ist gesperrt:** `WHM_Reborn.HealSingleGCD` verlangt `GetHealthRatio() > RegenHeal` (0,30), der Träger liegt bei 1 HP.
- **Die Vollheilung feuert am Anfang:** `BenedictionPvE` zündet unter `BenedictionHeal` (0,30), also sofort, mit 90 s Abklingzeit.

**Fall 4 braucht keine Prognose:** Die leichte Unterstützung gilt unabhängig vom Kurs, die HoT-Freigabe braucht also nur die Feststellung, dass Walking Dead liegt.

**Fall 4a braucht eine Prognose, und die besteht seit A91 bis A93** — die frühere Begründung „der Messbaustein ist begründet verworfen" ist damit überholt. Vollständig zu heilen ist nur richtig, wenn der Kurs nicht trägt; der Gesundheitsstand ist dafür kein Ersatz, weil Angriffe den Träger wieder auf 1 drücken, ohne die aufgenommene Heilung zu mindern. **Die vorhandene Prognose beantwortet allerdings die Nachbarfrage, nicht diese:** `GetCorrectedTTK` misst den Weg zur Null, Walking Dead fragt nach dem Weg zur **aufgenommenen Heilmenge in Höhe der Maximalgesundheit**. Die Datenquelle taugt für beides — der Gesundheitsverlauf steigt, wenn geheilt wird —, die Auswertung ist eine andere und noch nicht gebaut.

**Empfehlung: die HoT-Sperre aufheben, den Benediction-Vorrang offen lassen.** Der erste Teil ist eine belegte Behebung mit einer Bedingung; für den zweiten ist jetzt die Auswertung zu entwerfen, nicht mehr die Messgrundlage. Gekoppelt an `WithholdHealingForLivingDead`, weil wer Phase eins einschaltet den Tod als Auslöser will — das hält das heutige Verhalten für alle anderen unverändert.

### Living Dead: der Hebel ist die Option, nicht der HP-Grenzwert · N

**Konzept:** `docs/rotation-flow/09-tank-selfprotection.md`
`StateUpdater.ShouldHealSingle`: `threshold = target.NoNeedHealingInvuln() ? normal : Math.Min(normal, Service.Config.HealthProtectedRatio)`. `NoNeedHealingInvuln` ist `WillStatusEndGCD(2, …)` über `NoNeedHealingStatus`, und `LivingDead` steht in dieser Liste.

**Wirkung, in zwei Abschnitten — und genau so, wie der Auftraggeber die Regel gefasst hat:** Solange Living Dead noch **mehr als zwei GCDs** Restzeit hat, liegt die Schwelle bei `HealthProtectedRatio` 0,15; der Todeseffekt kann also eintreten. Läuft der Status in zwei GCDs oder weniger ab, liefert `NoNeedHealingInvuln` wahr und die **normale** Schwelle kehrt zurück — kurz vor Ablauf wird geheilt. Die Vorlaufzeit ist bis zur **Entscheidung** gemessen, nicht bis zum Landen der Heilung, weshalb zwei GCDs und nicht weniger.

**Der Vorlauf setzt aus, solange der Tod noch erreichbar ist** (`StatusHelper.DeathStillLikely`, A88, auf ausdrückliche Vorgabe des Auftraggebers): Steht der Träger auf oder unter `HealthForDyingTanks`, greift die Freigabe nicht, weil die Null vor dem Fensterende ankommt. Ohne das hätte der Vorlauf den Tod verhindert, für den die Regel da ist.

Die Dauer ist belegt, nicht erinnert: `ActionId.resx`, Aktion 3638, „Living Dead Duration: 10s". Bei rund 2,5 s Gießzeit sind zwei GCDs damit etwa die halbe Restzeit.

**`WithholdHealingForLivingDead` verschärft nur den ersten Abschnitt** — aus 0,15 wird „gar nicht", der zweite Abschnitt bleibt unverändert. Voreingestellt aus.

**Kein Fork-Rückschritt, im Gegenteil:** Upstream gibt einem Ziel unter Invulnerabilität überhaupt keine Heilung (`if (h == 0 || !target.NoNeedHealingInvuln()) return false;`). Die Absenkung auf einen Grenzwert ist die mildere Fassung.

**Der Fall, der sie trotzdem zum Problem macht,** ist der falsch gesetzte Invulnerabilitätsschub: Living Dead bei 70 % im Wall-to-Wall gezündet, wie vom Auftraggeber beobachtet. Die Konstruktion unterstellt, dass die Invulnerabilität gegen einen tödlichen Schlag gesetzt wird; wird sie zu früh gesetzt, kostet sie zehn Sekunden automatische Heilung, ohne dass der Anlass je eintritt.

**Der richtige Stellhebel ist die Option, nicht der Grenzwert.** `HealthProtectedRatio` anzuheben würde beide Abschnitte verschieben und damit gerade den Tod verhindern, auf den die Regel wartet — ein höherer Wert heilt **früher** im Fenster. Wer den Todeseffekt will, schaltet `WithholdHealingForLivingDead` ein; wer ihn nicht will, lässt beides, wie es ist. Der Grenzwert ist nur für die **anderen** Invulnerabilitäten der Liste maßgeblich (Holmgang, Superbolide, Hallowed Ground), bei denen kein Tod gewollt ist.

**Vollständige Erhebung der Defensivfähigkeiten des Dunkelritters gegen die Heilentscheidung** — nur zwei greifen ein:

| Fähigkeit | Pfad | Wirkung auf die Heilschwelle |
|---|---|---|
| The Blackest Night | `ShieldStatus` → Schildanrechnung | effektiv −25 Prozentpunkte |
| Living Dead | `NoNeedHealingStatus` → `HealthProtectedRatio` | 0,15 statt 0,65 |
| Walking Dead | in `NoNeedHealingStatus` **auskommentiert** | keine — richtig, dort ist Heilung überlebensnotwendig |
| Shadow Wall, Rampart | `RampartStatus` | **keine**: gelesen nur als `StatusProvide` der Tank-Rotationen und von `HasMajorMitigation`, und das fragt `PlayerHasStatus(true, …)`, also allein den eigenen Charakter |
| Dark Mind, Oblation, Dark Missionary | in keiner heilrelevanten Liste | keine |
| Reprisal | `ReprisalStatus` | keine — Debuff am Gegner |

Schadensreduktion wirkt also in keinem Fall auf die Heilentscheidung; nur Barriere und Invulnerabilität tun es.

### Die Zielwahl der Heilung misst nicht die Sterbegefährdung · N, U

**Vorgabe des Auftraggebers, vollständig in `docs/rotation-flow/07-heal-target-priority.md`:** Oberste Priorität hat das gesamtheitliche Überleben der Gruppe, ansonsten Triage. Maßgeblich ist, wer wie stark gefährdet ist zu sterben — Tank mit Aggro hat Priorität, Heiler mit Aggro muss überleben, bei mehreren Betroffenen entscheidet die Schadensrate, und ein Schadensausteiler ohne Aggro bei 10 % stirbt an der nächsten Flächenaktion. **Bei gleicher Gefährdung: Heiler vor Tank vor Schadensausteiler**, begründet mit Ersetzbarkeit — der Tank besteht eine Weile ohne Heiler, heilen kann nur der Heiler.

**Heute entscheidet allein der Prozentsatz** (`ActionTargetInfo.FindHealTarget`, Rang 4), davor zwei Rollenabkürzungen mit festen Schwellen. Weder Aggro noch Barriere noch absoluter Lebenspuffer gehen ein. Die Sortierung stammt aus dem Upstream; Fork-Arbeit ist allein die Behandlung der Unverwundbaren.

**Die Prüfreihenfolge stellt den Heiler vor den Tank, die Schwellen kehren das um:** Tank ≤ 45 %, Heiler ≤ 40 %. Im Band 40–45 % bekommt der Tank die Heilung, obwohl der Heiler gleich tief steht; spielt der Auftraggeber selbst den Heiler, trifft ihn derselbe Fall über `HealthSelfRatio` (ebenfalls 0,40). Die Differenz vertritt die fehlende Schadensrate — für den Regelfall „Tank hält die Aggro" richtig, für den zweiten Fall der Vorgabe „Heiler hält die Aggro" falsch, weil das Surrogat rollenfest statt lagefest ist. **Alle drei Werte sind identisch mit `upstream/main`**, also kein Fork-Defekt, und es sind Voreinstellungen im Code — die Konfiguration des Auftraggebers ist von hier nicht messbar.

**Drei der vier Größen sind verfügbar und zwei davon gelesen:** der effektive Puffer in absoluten Punkten (`GetEffectiveHp`, gelesen), die Schadensrate je Mitglied (`GetCorrectedTTK` aus `RecordedHP`, gelesen — A91 bis A93), die Aggro (`TargetObject`, wie in `CanProvoke` aufgelöst — **nicht** gelesen) und der angekündigte Flächenschaden (`IsHostileCastingAOE`, BMR-Vorhersage — **nicht** gelesen).

**Der schwerste Einzelfall ist ein Kurzschluss, nicht ein Maß:** `tankTars[0]` unter `HealthTankRatio` beendet die Suche sofort, also wird ein Schadensausteiler bei 10 % übergangen, sobald der Tank bei 44 % steht — der von der Vorgabe ausdrücklich genannte Fall.

**Der Entwurf steht vollständig in `docs/rotation-flow/07-heal-target-priority.md`:** drei Gefährdungsklassen statt einer Kette von Kurzschlüssen — kritisch (unter `HealthForDyingTanks`, geordnet nach absoluten Punkten), unter Beschuss (Aggro oder Tankhaltung, geordnet nach Prozentsatz), übrige (absolute Punkte bei angekündigtem Flächenschaden, sonst Prozentsatz); Rolle nur als Gleichstandsregel. Jedes Maß wirkt dort, wo es das Richtige misst, keines wird gewichtet, keine Zahl erfunden — BossModReborn nennt Art und Zeitpunkt des nächsten Einschlags, nicht seine Höhe.

**Umgesetzt sind Stufe 1 und Stufe 3.** Stufe 1 (A89): Klasse 1 steht vor allen drei Kurzschlüssen. Stufe 3 (A93): die Rate je Mitglied — allerdings nicht als zusätzliches Ordnungsmerkmal innerhalb einer Klasse, wie ursprünglich entworfen, sondern als **Ersatz der gelesenen Gesundheit** durch die vorausberechnete, hinter `HealAheadOfDamage` mit Standard aus. Damit erben alle vier Entscheidungen der Methode die Vorausschau, Klasse 1 eingeschlossen; die Begründung der Entwurfsänderung steht in Konzept 07.

**Offen bleibt Stufe 2:** Klassen 2 und 3, mit einem Aggro-Set, das `TargetUpdater.UpdateLists` einmal je Bild aus den `TargetObjectId` der Gegner aufbaut. Verhaltensänderung ohne Nachweismöglichkeit → hinter eine Einstellung mit beibehaltener Voreinstellung. **Empfehlung: erst nach einer Spielbeobachtung der Vorausschau**, weil beide dieselbe Rangstufe betreffen und sich sonst nicht auseinanderhalten lassen.

**Was der Entwurf nicht löst:** Die Schwellendifferenz 45/40 ist entweder wirksam — dann kehrt sie im Band die Rangfolge um — oder unwirksam, dann ist eine Nutzereinstellung stillgelegt. Die Klassenordnung entschärft sie, beseitigt sie nicht. Ob die Werte vereinheitlicht werden, ist eine Wertentscheidung über eine Konfiguration und liegt beim Auftraggeber.

### `HasSurvivingShield` misst die **kürzeste** Schildrestzeit, nicht die längste · N, R

**Konzept:** `docs/rotation-flow/09-tank-selfprotection.md`, `docs/rotation-flow/13-aoe-damage-classification.md`
`StatusHelper.cs:942`: `GetObjectShield() > 0 && !WillStatusEnd(horizon, false, ShieldStatus)`. `WillStatusEnd` stützt sich auf `StatusTime`, und das liefert das **Minimum** über alle vorhandenen gelisteten Status (`StatusHelper.cs:990-1011`). Beantwortet wird damit „laufen **alle** Barrieren noch?", während der Doku-Kommentar derselben Methode „has an active shield that will still be up" sagt — Singular.

**Wirkung:** Ein Ziel mit mehreren Barrieren — der Regelfall in einer Gruppe mit Heiler, etwa Galvanize plus Eukrasian Diagnosis plus eine Gruppenbarriere — gilt als ungeschützt, sobald die **kürzeste** unter den Horizont fällt, obwohl `GetObjectShield()` weiterhin einen Wert meldet. Der Fehler zeigt in dieselbe Richtung wie der behobene (A43): zu viel Heilung, nie zu wenig. Deshalb hat die Erweiterung der Liste ihn nicht verschlimmert, sondern nur häufiger sichtbar gemacht.

**Seit der Entfernung der Schildanrechnung (A85) hat die Methode keinen Leser mehr im Baum.** Sie bleibt trotzdem: `public static` in `RotationSolver.Basic`, also Teil der Paketschnittstelle für abgeleitete Rotationen — ihre Entfernung wäre ein Signaturbruch für den Betroffenenkreis R, und die Methode ist für ihre eigene Frage („hält die Barriere noch") richtig gebaut. Der Befund unten betrifft ihre Genauigkeit, nicht ihre Berechtigung.

**Warum nicht sofort behoben:** Die naheliegende Umkehr auf das Maximum tauscht den Fehler nur aus. Sie würde ein Ziel als geschützt werten, dessen große Barriere gerade ausläuft, solange irgendeine kleine bleibt — und `GetObjectShield()` liefert nur den Gesamtwert, nicht die Aufteilung je Status. Eine Unterschätzung kostet eine überflüssige Heilung, eine Überschätzung kostet einen Tod.

**Auflösungsbedingung:** Laufzeitbeobachtung, welche der beiden Abweichungen tatsächlich auftritt, oder eine Quelle für die Aufteilung des Schildwerts auf die einzelnen Status. **Empfehlung bis dahin: belassen** — die heutige Richtung ist die sichere.

### Statuslisten und Einzelprüfungen ohne die Geschwister-Ids ihrer Wirkung · N, R

Das Spiel führt jede Wirkung unter mehreren Status-Ids desselben Anzeigenamens — eine je Fassung der Fähigkeit, dazu PvP-Formen und die Fassungen, in die eine Trait aufwertet. Eine handgepflegte Aufzählung, die eine Id nennt und die Geschwister auslässt, antwortet für den Träger der ausgelassenen Id **falsch**, nicht nur ungenau. `scan14.py` erhebt die Klasse über alle Statuslisten in `StatusHelper.cs` (A49) und nennt die jeweils aktuelle Zahl; sie steht hier bewusst nicht, weil sie mit jeder Ergänzung wächst. Genau das ist nach der Behebung von `RampartStatus` und `ReprisalStatus` geschehen: Je mehr Ids geführt sind, desto mehr Gruppen haben einen Vertreter, und desto mehr Geschwister werden überhaupt sichtbar. Eine steigende Zahl ist hier also Fortschritt, nicht Verfall.

**Was davon kein Defekt ist:** 114 Treffer stammen aus `PhantomDispellable` und `PurifyPvPStatuses`, die bewusst Teilmengen sind. Weitere Treffer sind andere Wirkungen unter geteiltem Namen — `Nebula_3051` (Reflexion), `Bloodwhetting_3030` (Lebensraub), `Holmgang` 88 und 1305 auf dem Ziel statt dem Träger (C15). Der Scan druckt zu jedem Kandidaten die Wirkbeschreibung und die Marke `same opening`/`differs`; entscheiden muss ein Leser.

**Zwei benannte Fundstellen, an denen die Zuordnung offen ist:**

- `TankStanceStatus` führt `IronWill` (79) und `RoyalGuard_1833`, nicht aber `IronWill_393`, `IronWill_2843` und `RoyalGuard` (392) — gleicher Anzeigename, gleicher Wirktext „Enmity is increased." Die Liste entscheidet, wen die Zielwahl für einen Tank hält (`ActionTargetInfo.cs`, sieben Stellen, darunter `FindTankTarget` und `FindKardia`) und ob `HasTankStance` für den Spieler greift. `Defiance_1396` und `Grit_1397` („Damage dealt and taken are reduced.") sind die Fassungen vor Shadowbringers und gehören **nicht** hinein.
- `GetCurrentMitigationPercent` (`CustomRotation_OtherInfo`) liest `StatusID.Addle` und `StatusID.Feint` als **einzelne** Id. `Addle_1988` (Geltungsbereich BLM SMN RDM BLU PCT, keine PvP-Aktion in `ActionId.resx`) und `Feint_2185` existieren; die Minderungsbilanz zählt eine vorhandene Schwächung dann als nicht vorhanden. Einzelprüfungen dieser Art sieht `scan14.py` nicht — er erhebt nur Ids in Listen.

**Warum nicht behoben:** Welche Id das Spiel je Stufe tatsächlich setzt, ist aus den Daten nicht zu entscheiden. Bei `Reprisal_2101` trug der Beleg — genau eine PvE-Aktion, Geltungsbereich auf die vier Jobs verengt, belegter Trait-Stufenwert; bei den Tankhaltungen tut er das nicht, und eine falsch aufgenommene Id kehrt die Antwort in die andere Richtung um. **Auflösungsbedingung:** Laufzeitbeobachtung, welche Id ein Tank beziehungsweise ein Zauberer im Ziel trägt, oder eine Quelle für die Id-Zuordnung je Stufe.

**Empfehlung: erfassen, nicht bearbeiten.** Behoben ist, was eine Wirkkette im Code liest und wo der Beleg trägt. Der Rest ist eine Klasse ohne Schranke: Ein Rückgabewert, den nur Wegsehen grün hält, wäre schlechter als keiner, und `scan14.py` hält die Liste jederzeit wieder abrufbar.

### Im Vorschaulauf liefert `CanUse` wahr, ohne ein Ziel zu setzen · N, R

**Konzept:** `docs/rotation-flow/03-universal.md`
`BaseAction.CanUse` schreibt das gefundene Ziel nur außerhalb der Vorschau: `Target = PreviewTarget.Value` steht unter `if (!IBaseAction.ActionPreview)` (`BaseAction.cs:264`). Zurückgegeben wird trotzdem `true`. Wer also im Vorschaulauf nach einem erfolgreichen `CanUse` auf `X.Target.Target` zugreift, liest entweder ein **veraltetes** Ziel aus einem früheren echten Lauf oder — solange die Aktion noch nie erfolgreich gewählt wurde — `default(TargetResult)`, dessen `Target` trotz nicht-nullbarer Deklaration **null** ist (ein Struct umgeht die Nullability-Garantie).

**Die Wirkkette ist geschlossen**, nicht vermutet: `CustomRotation_Invoke.TryInvoke` setzt `ActionPreview = true` (nur bei `DataCenter.DrawingActions`), ruft darunter `UpdateActions`, und das ruft die echten Dispatch-Methoden der Heilung und Verteidigung — Fläche und Einzelziel, GCD und Fähigkeit — samt Dispel-, Wiederbelebungs-, Positional- und Bewegungspfad.

Genau dort steht das Muster, sechsmal im Heilerbestand:

- `AST_Reborn`, Essential Dignity in der Einzelheilung (drei Schwellenstufen)
- `AST_Reborn`, Aspected Benefic
- `SCH_Reborn`, Excogitation
- `ScholarRotation`, Excogitation — die Basisrotation, also Paketoberfläche

**Kein Kampffehler.** Der eigentliche `Invoke` läuft nach `ActionPreview = false`, dort wird das Ziel gesetzt. Die Folge trifft die Anzeige: `UpdateHealingActions` fängt jede `Exception`, setzt die vier Heilanzeigen auf `null` und schreibt in den PluginLog — die Vorschau zeigt dann keine Heilaktion, obwohl eine anstünde. `UpdateDefenseActions` fängt nur `MissingMethodException`, eine Nullreferenz propagiert von dort also weiter nach `TryInvoke`.

**Die Angriffspfade sind nicht betroffen**, weil `UpdateActions` sie nicht aufruft: die 58 Stellen in `SMN_Reborn`, `BRD_Reborn`, `MCH_Reborn`, `DRG_Reborn` und `PhantomDefault` liegen in `AttackAbility` und `GeneralGCD`. Von den 87 Fundstellen des Musters im eigenen Baum sind damit sechs erreichbar.

**Zu entscheiden ist die Richtung**, deshalb nicht behoben: Dass die Vorschau `Target` nicht überschreibt, ist eine ausdrückliche Entscheidung im Code — sie soll den echten Zustand nicht verändern. Falsch ist folglich nicht die Nicht-Zuweisung, sondern der Zugriff auf `Target` im Vorschaulauf. Drei Wege, alle mit Wirkungsbereich über sämtliche Rotationen und damit auch über die Paketnutzer: die Rotationen auf `PreviewTarget ?? Target` umstellen (viele Stellen, dauerhaft), `Target` in der Vorschau in ein Schattenfeld schreiben und die Leser dorthin lenken (eine Stelle, aber neue Zustandshaltung), oder `CanUse` in der Vorschau `false` liefern lassen, sobald ein Ziel nötig ist (kleinster Eingriff, verändert aber, was die Vorschau anzeigt).

**Empfehlung: erfassen, entscheiden, dann bauen.** Der Schweregrad ist gering — Anzeige und Lograuschen, kein Kampfeffekt —, der Wirkungsbereich jeder Behebung dagegen groß, und keiner der sechs erreichbaren Punkte liegt in einem Job des Nutzungsprofils.

### `PredictedDamageType` ist an ein fremdes Enum gebunden, und die Zuordnung ist ungeprüft · N, R

**Konzept:** `docs/rotation-flow/08-mitigation-synergy.md`
BossModReborn liefert `Hints.NextDamageType` als `(int)predicted[0].Type` — das Ordinal **seines** Enums —, und `BossModUpdater` castet den Wert direkt in unser `PredictedDamageType`. Die Nummerierung ist damit Schnittstellenvertrag, nicht innere Angelegenheit.

**Beim Schwesterfall derselben Datei war genau diese Zuordnung falsch.** `SpecialMode` überschreitet dieselbe Grenze; die Angleichung (`8dc2bd658`) stellte fest, dass unser `Freezing` auf BossModReborns `Misdirection` zeigte und unser `Misdirection` auf nichts. Dort wurde der Vertrag danach mit expliziten Werten und einem Kommentar festgeschrieben — `PredictedDamageType` stand daneben und wurde nicht mitgeprüft. Das ist die Klassenlücke: dieselbe Bauform, eine Stelle gesichert, die Nachbarstelle nicht.

**Umgesetzt ist nur, was verhaltensneutral ist:** Die heutigen impliziten Werte 0–3 sind jetzt ausgeschrieben, der Vertrag steht als Kommentar am Enum, und die beiden Rücksetzungen in `ResetBmrData` nennen `PredictedDamageType.None` und `SpecialMode.Normal` statt des Literals `0`. Damit fällt eine Einfügung an falscher Stelle beim Lesen auf, statt die Bedeutung des Fremdwerts still zu verschieben.

**Was offen bleibt, ist die Zuordnung selbst.** Ob BossModReborn `None, Tankbuster, Raidwide, Shared` in dieser Reihenfolge führt, ist nicht belegt: Das Enum liegt in `AIHints`, und dessen Pfad ist ohne Code-Suche nicht auffindbar — `api.github.com` ist für diese Sitzung auf die freigegebenen Repositories beschränkt, `raw.githubusercontent.com` liefert nur, was man benennen kann (`IPCProvider.cs` ist so belegt worden).

**Heute folgenlos, und das ist gemessen:** `BMRNextDamageType` wird geschrieben, als `CustomRotation.BMRDamageType` weitergegeben und von **keinem** Entscheidungspfad gelesen — dieselbe Lage wie bei `SpecialMode` zum Zeitpunkt seiner Angleichung. `SMN_Reborn` erwähnt das Paar im Kommentar ausdrücklich als das, was man **nicht** benutzt, weil es nicht sagt, wen der Tankbuster trifft.

**Auflösungsbedingung:** der Pfad zu BossModReborns `AIHints.SpecialMode`/`PredictedDamage.Type` oder eine Laufzeitbeobachtung der gelieferten Zahlen. **Empfehlung: vor dem ersten Verbraucher auflösen** — Konzept 08 sieht die Schadensart als künftige Grundlage der Minderungswahl, und dann wäre eine vertauschte Zuordnung keine Randnotiz mehr.

### Die Minderungsbilanz kennt zwei Schadensarten, die Datenquelle drei · N, R

**Konzept:** `docs/rotation-flow/08-mitigation-synergy.md`
`GetCurrentMitigationPercent` gewichtet sechs Faktoren binär nach `incomingMagical ? 0.90f : 0.95f` — Addle, Feint, Fey Illumination, Magick Barrier und zwei weitere. Der Wert kommt aus `DataCenter.IsMagicalDamageIncoming()`, das `AttackType.RowId == 5` prüft. Das Blatt kennt aber mehr Werte als 5 und 7; und wenn gerade **niemand** wirkt, ist `CastActionId` überall 0 und die Antwort ebenfalls `false`. Beide Fälle rechnet die Bilanz als **physisch** — mit vertauschten Vorzeichen: Addle zählt dann −5 % statt −10 %, Feint −10 % statt −5 %.

**Der Baustein, der das trennen würde, ist vorhanden und nicht verdrahtet.** `IsPhysicalDamageIncoming()` (`AttackType.RowId == 7`) hat im ganzen Baum **keinen Leser** — kein Kampfpfad, nicht einmal die Diagnoseanzeige, die ihr magisches Gegenstück zeigt. Das ist die Bauform, die `CLAUDE.md` als fehlende Verdrahtung statt tote Stelle führt (Beleg `ResetAvailabilityCheck`): Mit beiden Prädikaten ließe sich „unbekannt" von „physisch" unterscheiden, statt es stillschweigend zusammenzulegen.

**Wirkung: klein, und das ist belegt, nicht vermutet.** `GetCurrentMitigationPercent` hat genau **einen** Verbraucher, die Diagnosezeile in `RotationConfigWindow` (`0.0–0.95`). Kein Kampfpfad liest sie; in meiner eigenen Arbeit dieser Sitzung ist sie als **Vorbild** zitiert, nicht als Aufrufziel. Der Fehler zeigt sich also derzeit nur in einer Anzeige — aber die Bilanz ist öffentlich und damit für abgeleitete Rotationen lesbar, und Konzept 08 sieht sie als künftige Entscheidungsgrundlage.

**Zwei weitere Klon-Reste an derselben Stelle**, ohne eigene Wirkung: Die beiden Methoden sind Kopien mit geänderter Konstante, und die `<remarks>` der physischen Fassung sagt „Returns early on the first confirmed **magical** cast". Der Kommentar bleibt stehen, bis die Stelle bearbeitet wird — ihn allein anzugleichen würde den Beleg der Entstehung tilgen.

**Auflösungsbedingung:** die Zuordnung der `AttackType`-Zeilen. Dass 5 magisch und 7 physisch ist, steht im Code ausdrücklich als Deutung („interpreted as"), nicht als Beleg; welche weiteren Zeilen vorkommen und wie häufig, ist ohne die Spieldaten nicht zu entscheiden — `RotationSolver.GameData` könnte das Blatt ausgeben, läuft aber nur beim Auftraggeber.

**Empfehlung: erfassen.** Solange nur eine Anzeige betroffen ist, wäre eine Umstellung auf geratener Zuordnung teurer als der Fehler. Wird die Bilanz zur Entscheidungsgrundlage, ist sie vorher aufzulösen — dann gehört auch der dritte Fall benannt, statt ihn als physisch zu führen.

### NIN: Der GCD-Vorbehalt vor der Ninjutsu-Ausführung ist konstant wahr · N, U

`NIN_Reborn.cs:977` und `BeirutaNIN.cs:1003` tragen beide

```
if (_ninActionAim != null && GCDTime() == 0f)
```

und `GCDTime(uint gcdCount = 0, float offset = 0)` liefert `(DefaultGCDTotal * 0) + 0`, also **konstant 0**. Der Vergleich ist damit zur Übersetzungszeit entschieden, und die Bedingung reduziert sich auf `_ninActionAim != null`. Der Block dahinter führt die Ninjutsu-Aktionen aus (`DoGokaMekkyaku`, `DoHuton`, `DoDoton`).

**Die Absicht ist erkennbar und nicht umgesetzt:** Ein Aufruf von `GCDTime()` an dieser Stelle kann nur einen Zeitvergleich gemeint haben — vermutlich „der GCD ist frei", also `DefaultGCDRemain == 0f`, oder ein Fenster von einem GCD. Welcher der beiden, sagt der Code nicht; beides zu raten hieße, die Rotation auf Verdacht zu ändern.

**Klasse und Abgrenzung:** `scan9.py` erfasst das Muster jetzt. Von 14 Methoden im Baum, deren argumentloser Aufruf durch die Standardwerte konstant ist, sind genau diese zwei Stellen Treffer — der konstante Wert allein ist **kein** Befund: `SongEndAfterGCD()` heißt „endet der Status jetzt" und gibt seine 0 sinnvoll an eine weitere Prüfung weiter. Zum Defekt wird es erst, wenn der konstante Wert **selbst** die Antwort ist und gegen ein Literal verglichen wird.

**Empfehlung: erfassen, nicht bearbeiten.** Ninja steht nicht im Nutzungsprofil, und die zweite Fundstelle liegt in einer fremden Rotation. **Auflösungsbedingung:** eine Angabe, welcher Zeitvergleich gemeint war — oder eine Beobachtung, ob das Ninjutsu-Timing im Spiel auffällt.

### Die Aquapolis fehlt in der Zielpriorisierung der Schatzkarten-Dungeons · N, U

`ObjectHelper.TreasureDungeonPrio` zählt neun Schatzkarten-Dungeons auf und nennt zu jedem die NPCs, die Vorrang haben — Namazu Stickywhisker in den Lost Canals, Alpaca of Fortune in Cenote Ja Ja Gural, Vaultkeeper in Vault Oneiron. `DataCenter.IsInTheAquapolis` ist der zehnte und **einzige** ohne Zweig; von den elf Flags der Region `Treasure Hunt` ist es das einzige ohne jeden Leser im Baum.

**Entstehung belegt:** Konstante und Aufzählung stammen aus demselben Commit (`4ee849eca`, 14.05.2026, „Add Treasure Dungeon support"). Die Stelle war also nie vollständig — *Ignorant Surgery* in Parnas' Sinn, nicht das Altern einer zuvor richtigen Aufzählung.

**Kandidat, nicht Befund.** Die Gegenhypothese ist nicht widerlegt: Hat die Aquapolis gar keinen Vorrang-NPC dieser Art, dann ist ihre Auslassung aus der Aufzählung richtig und der Befund verschiebt sich auf die Konstante, die dann keinen Verwender braucht. Das ist eine Spieltatsache, und sie ist von hier aus nicht belegbar — das Enum `NPCName` ist aus den Spieldaten erzeugt, führt aber keinen Territoriumsbezug, und die Wikis sind vom Egress gesperrt.

**Auflösungsbedingung:** eine Angabe des Auftraggebers oder eine Spielbeobachtung, welche Gegner in der Aquapolis Vorrang haben sollen. **Empfehlung: erfassen, nicht bearbeiten** — Schatzkarten stehen nicht im benannten Nutzungsprofil, und ohne die Namen wäre jeder Zweig geraten.

### Vier Vorrangregeln, die nichts entscheiden, weil derselbe Aufruf unbedingt folgt · N, U

**Konzept:** `docs/rotation-flow/03-universal.md`
`scan.py`, Prüfung (f). Vier Stellen wickeln einen Aktionsaufruf in eine Bedingung und wiederholen denselben Aufruf unmittelbar danach **ohne** Bedingung. Da der innere Zweig zurückkehrt, ist die Bedingung wirkungslos: Sie trifft keine Wahl, die der unbedingte Aufruf nicht ohnehin träfe.

- `VPR_Reborn.cs:518` — `VicepitPvE` unter „letzte Ladung und Wiederholzeit unter 10 s", direkt gefolgt vom unbedingten Aufruf.
- `VPR_Reborn.cs:835` — dasselbe für `VicewinderPvE`.
- `PCT_Reborn.cs:199` — dasselbe für `RetributionOfTheMadeenPvE`.
- `RDM_Reborn.cs:140` — hier ohne erkennbare Absicht: dieselbe Bedingung steht wortgleich in sich selbst geschachtelt (`InCombat && HasHostilesInMaxRange && ManaficationPvE.CanUse(out act)`).

**Warum das ein Defekt und nicht nur Stil ist:** Die Bedingung ist ein Beleg der Entwurfsabsicht — bei den drei Ladungsfällen „gib der Aktion Vorrang, bevor eine Ladung überläuft". Diese Absicht ist nicht umgesetzt. Die Entstehungsform ist bei VPR belegbar an der Versionsgeschichte: Der äußere Zweig wurde in `acebc4537` („fix for VPR weirdness") nachgeschärft, der innere blieb stehen — *Ignorant Surgery* in Parnas' Sinn, kein Altern einer Prämisse.

**Bewusst nicht gelöscht**, aus demselben Grund wie beim leeren VPR-Zweig (A11): Die Entfernung wäre verhaltensneutral, würde aber die einzige Spur der nicht umgesetzten Vorrangregel tilgen. Zu entscheiden ist, ob die Regel gemeint war — dann muss der unbedingte Aufruf nach hinten oder unter eine Gegenbedingung — oder ob sie fallen soll.

**Empfehlung: erfassen, nicht bearbeiten.** Keiner der vier Jobs steht im Nutzungsprofil des Auftraggebers, und die Entscheidung „Vorrang gemeint oder nicht" gehört zum Autor der Rotation; Adressat ist der Upstream.

## Technische Schuld

### Zustandsabfragen, die bei jedem Lesen neu über Gruppe oder Gegner laufen · N, R

**Konzept:** `docs/rotation-flow/03-universal.md`
`DataCenter` führt **15 öffentliche statische Eigenschaften, deren Getter iteriert** — darunter genau die, die in den heißen Pfaden stehen: `PartyTank`, `RefinedHP`, `PartyMembersHP`, `NumberOfHostilesInRange`, `NumberOfHostilesInMaxRange`, `AverageTTK`, `DPSTaken`, `AreHostilesCastingKnockback`. Dazu die Helfer derselben Bauform: `SurveyHostileOutput` fragt je Gegner im Radius sechs Status ab, `GetCurrentMitigationPercent` vier, `SurveyStuns` einen.

**Der schwerste Fall war `RefinedHP`, und Upstream hat ihn behoben.** `5ffd22056` (Upstream, 12.09.2026, mit 7.5.6.6 eingezogen) legt einen Cache mit 15 ms Lebensdauer davor, ebenso für `AverageTTK` und die Gruppen-HP-Kennzahlen; `ResetAllRecords` verwirft ihn. Der folgende Absatz beschreibt den Stand davor und bleibt als Begründung stehen, weil das Muster in den übrigen Eigenschaften unverändert vorliegt: Es baute bei **jedem** Lesen ein neues `Dictionary` über alle Gruppenmitglieder, mit einem `try/catch` je Mitglied — und es ist die Nachschlagetabelle, aus der `ObjectHelper.GetHealthRatio` und `GetPlayerHealthRatio` ihren Wert holen. Eine Tabelle, die bei jedem Nachschlagen neu gebaut wird, ist unabhängig von der Häufigkeit eine verkehrte Konstruktion. `GetHealthRatio()` hat 129 Aufrufstellen im Baum.

**Die Kosten sind geschätzt, nicht gemessen.** Aufrufstellen sind kein Maß für Aufrufe je Frame: die meisten liegen in Rotationen, die nie zugleich laufen. Ein Profiler ist von hier aus nicht verfügbar, und statische Prüfung reicht dafür nicht — die Zahl ist deshalb ausdrücklich als Surrogat gekennzeichnet und nicht als Wirkung.

**Warum trotzdem kein Defekt:** Das Verhalten ist richtig, nur der Aufwand ist es vielleicht nicht. `PartyMembers` ist auf die eigene Gruppe begrenzt (Allianzmitglieder gehen in `AllianceMembers`, geprüft an `TargetUpdater`), es geht also um maximal acht Einträge je Aufbau.

**Auflösungsbedingung:** eine Laufzeitbeobachtung, ob es spürbar ist. **Auflösungsweg, falls ja:** Das Muster liegt im Baum bereits vor — `TargetUpdater` schreibt `PartyMembers`, `AllianceMembers` und `AllHostileTargets` einmal je Frame in `DataCenter`. `RefinedHP` gehört auf denselben Weg. Zu bedenken ist, dass `InEffectTime` zeitbasiert ist: ein Wert je Frame ist damit nicht bitgleich zum Wert je Lesen, also eine Verhaltensänderung ohne Nachweismöglichkeit und nach Projektregel nur mit beibehaltener Voreinstellung zu bauen.

**Empfehlung: erfassen.** Ohne Messung wäre jede Umstellung eine Verbesserung auf Verdacht, und der Betroffenenkreis umfasst die Paketnutzer: `RefinedHP` und die übrigen Eigenschaften sind öffentlich, ihr Aufrufverhalten ist Teil des Vertrags.


### `Configs.Migrate` ist kein Migrationspfad, sondern ein Zurücksetzen · N, R

`Configs.Migrate` lautet vollständig: weicht die gespeicherte `Version` von `CurrentVersion` ab, wird `new Configs()` zurückgegeben — **die gesamte Nutzerkonfiguration fällt auf die Vorgaben zurück**. Der Kommentar „Implement migration logic if needed" weist die Stelle als bewusst offenen Platzhalter aus, nicht als Fehler; sie ist damit technische Schuld und kein Defekt.

**Kosten, und sie sind höher als sie aussehen.** Die Schuld ist nicht nur ein fehlendes Bequemlichkeitsmerkmal, sie **blockiert andere Behebungen**: Jede Korrektur, die einen Vorgabewert ändern muss, um das bisherige Verhalten zu erhalten, ist ohne Feldmigration nicht durchführbar. Der Eintrag zu `InterruptDelay`/`ProvokeDelay` weiter oben ist genau daran gescheitert. Dieselbe Sperre trifft künftig jede Einstellung, deren Vorgabe sich als falsch erweist.

Hinzu kommt die unmittelbare Wirkung für den Auftraggeber: Erhöht Upstream `CurrentVersion` — etwa weil dort ein Feld hinzukommt —, sind beim nächsten Start **alle** eigenen Einstellungen weg. Ein Sicherungsstand lässt sich über `Backup()` anlegen; `Restore()` verweigert allerdings genau dann, wenn die Version abweicht, also im einzigen Fall, in dem man ihn bräuchte.

**Auflösungsbedingung:** eine feldweise Migration, die die gespeicherte Version liest und nur die geänderten Felder umstellt, statt die Datei zu verwerfen. Erst danach sind Vorgabewert-Korrekturen überhaupt möglich.

### `CanEarlyWeave` steht auf dem beobachteten statt auf dem geschriebenen Verhalten · N, R

**Konzept:** `docs/rotation-flow/03-universal.md`
`CanEarlyWeave` wurde in `0246bea5` als `(!HasWeaved() || WeaponRemain > LateWeaveWindow) && CanWeave` eingeführt, im selben Commit wie ein `HasWeaved()`, das nicht `false` liefern konnte. Die erste Hälfte der Disjunktion hat deshalb nie beigetragen; sämtliche Verbraucher — alle in `ExtraRotations` — sind gegen die zweite Hälfte allein geschrieben und eingestellt worden.

`HasWeaved()` ist behoben (A30). Die Disjunktion mitzuwecken hätte aber keine Absicht wiederhergestellt, sondern das Verhalten verschoben: Sie macht `CanEarlyWeave` auch im späten Fenster wahr, sobald noch nichts geweavt wurde, und `ChurinMNK.TryUseRiddleOfFire` (`ChurinMNK.cs:804`) liest `CanEarlyWeave` **ausschließend** gegen `CanLateWeave`. Im Einzel-Weave-Fall — spätes Fenster, nichts geweavt — fiele Riddle of Fire damit ganz aus. `ChurinBRD` liest `CanEarlyWeave` an drei Stellen einschließend (`:698`, `:810`, `:837`), davon einmal als benutzergewählte Zeitpunktoption `WandererWeave.Early`, deren Bedeutung sich mit der Ausweitung verwischt.

**Kosten des Kompromisses:** `CanEarlyWeave` heißt jetzt, was es tut — „erste Hälfte der Wiederholzeit" —, und ist damit exakt das Komplement von `CanLateWeave`. Die Frage, ob der ursprüngliche Autor eine Disjunktion oder eine Konjunktion (`!HasWeaved() && WeaponRemain > LateWeaveWindow`) meinte, bleibt offen; beide Lesarten hätten je eigene Verschiebungen in den beiden fremden Rotationen zur Folge. Die Konjunktion erzeugt zusätzlich eine Lücke: Ein zweiter Weave in der frühen Hälfte wäre dann weder früh noch spät.

**Auflösungsbedingung:** Entscheidbar nur über die Absicht der beiden fremden Autoren oder über Laufzeitbeobachtung der Weave-Zeitpunkte in ChurinMNK und ChurinBRD. Statische Prüfung reicht nicht aus, und ohne Beleg gilt die Regel, das bisherige Standardverhalten beizubehalten.

**Empfehlung: belassen.** Der heutige Ausdruck beschreibt genau das Verhalten, gegen das alle Verwender eingestellt sind. Jede der beiden denkbaren Wiederherstellungen verschiebt das Verhalten fremder Rotationen — die Disjunktion nimmt ChurinMNK im Einzel-Weave-Fall Riddle of Fire, die Konjunktion erzeugt einen Weave, der weder früh noch spät ist. Ein Eingriff lohnt erst, wenn eine Laufzeitbeobachtung vorliegt.

### Doppelte Zustandswahl in den Zustandskommandos · N

Die Zustandswahl liegt an zwei Orten: implizit in `AdjustStateType`, wo `/rotation Auto` über `UpdateTargetingIndex` selbst durch die Zielarten schaltet, sofern `ToggleAuto` aus ist; explizit in den fünf `Cycle*`-Methoden, die dieselbe Aufgabe erneut lösen und über `CycleType` bzw. `DTRType` am Chatkommando und am Leistenklick hängen. Da die `Cycle*` ebenfalls `DoStateCommandType` rufen, greift `AdjustStateType` auch dort; die Toggle-Optionen wirken dadurch als Krücken für fehlende Übergänge, statt als unabhängige Achse.

**Kosten:** `DTRAllAuto` kollabiert mit aktivem `ToggleAuto` auf Off ↔ Auto(0), die Zielarten-Rotation ist dann tot. Umgekehrt ist `ToggleAuto` bei `DTRManualAuto` der einzige Ausschaltweg über die Leiste — ein pauschales Umgehen der Toggle-Auswertung würde ihn beseitigen.

**Empfehlung: belassen.** Die Kosten treffen zwei Bedienvarianten, nicht das Kampfverhalten, und jeder Umbau berührt einen Zustandsautomaten, dessen Übergänge statisch nicht abzusichern sind — der einzige bisher geprüfte Eingriff hätte einer Variante den einzigen Ausschaltweg genommen.

**Auflösungsbedingung:** erst mit einer Möglichkeit zur Laufzeitbeobachtung; ein Zustandsautomat mit acht Zuständen, fünf Zykluswegen und zwei Schaltern ist statisch nicht abzusichern. Bei einem Eingriff ist der Persistenzvertrag zu beachten: `DTRType` und `CycleType` liegen als Ordinalzahlen in der Nutzerkonfiguration, ihre Reihenfolge ist nicht frei änderbar.

Geprüfte Nicht-Fehlstellen: `DTRManualAuto` bildet den vom Enum-Text beschriebenen Zwei-Zustands-Zyklus ab (kein Fehler, AUDIT_LOG A14); ein zu großer `TargetingIndex` kann keinen Indexfehler auslösen, `DataCenter.TargetingType` rechnet `% Count`.

### Searing Light wird erlaubt, aber nicht vorgezogen · N

**Konzept:** `docs/rotation-flow/12-searing-light-stacking.md`
**Spielbeobachtung des Auftraggebers, 4er-Instanz, einziger Beschwörer:** Searing Light fiel nicht zu Beginn der Solar-Bahamut-Phase, sondern irgendwann darin. Damit ist die Aussage „bei einem Beschwörer ist der Ablauf richtig" aus Konzept 12 widerlegt; das Konzept ist fortgeschrieben.

**Ursache, am Code belegt:** `mayFireSearingLight` ist bei einem Beschwörer über `burstInSolar` während der **gesamten** Phase wahr (`SMN_Reborn.AttackAbility`). Die Bedingung sagt **ob**, nicht **wann**. Verpasst die Aktion den ersten Einschiebeplatz — belegter Platz, kurz kein Ziel in Reichweite, die Ausführungssperre kurz vor dem nächsten GCD —, fällt sie beim nächsten freien Platz, und nichts holt das nach oder priorisiert sie. Searing Light steht zwar an erster Stelle **innerhalb** von `AttackAbility`, aber der Fähigkeitenpfad erreicht `AttackAbility` erst nach Notfall-, Verteidigungs- und Heilzweigen.

**Kosten, aus den Wirktexten** (`ActionId.resx`): Searing Light wirkt 20 s, Summon Solar Bahamut dauert 15 s. Zu Beginn gezündet deckt der Buff die ganze Phase und läuft 5 s über — der Überhang ist eingeplant. Jede Sekunde Verzug tauscht eine gebuffte Sekunde in der Phase gegen eine danach: Demi-GCDs tragen 947–1217 Potenz, Zwischenblöcke höchstens 632, die 5 % wirken auf den darunterliegenden Wert. Zwei bis drei GCDs Verzug verschieben damit rund 30 Potenz je GCD von der starken in die schwache Phase, und zwar auch für die nahen Gruppenmitglieder.

**Vor einer Umsetzung zu klären:** Welcher Zweig den ersten Einschiebeplatz tatsächlich belegt, ist statisch nicht zu bestimmen — das ist eine Laufzeitfrage. Ein Eingriff in die Reihenfolge des Fähigkeitenpfads ist zudem genau die Bauform, die in C37 im Spiel schlechter war als der Defekt. **Empfehlung:** zuerst messen, welcher Platz vergeben wird, dann entscheiden; das Messmittel gehört nach der Cynefin-Regel mitgeliefert.

### Beim Beschwörer bleibt in der 4er-Instanz nur ein einziger Weg zu Radiant Aegis und Addle · N

**Konzept:** `docs/rotation-flow/12-searing-light-stacking.md`, `docs/rotation-flow/13-aoe-damage-classification.md`
**Spielbeobachtung des Auftraggebers, 4er-Instanz:** kein Addle und kein Radiant Aegis trotz Flächenschaden. Beide zusammen, was auf eine gemeinsame Ursache deutet — und die gibt es.

**Vollständig erhobene Kette.** Beide Aktionen stehen beim Beschwörer in `DefenseAreaAbility` und `DefenseSingleAbility`; Radiant Aegis zusätzlich in `GeneralAbility`. Was diese drei Wege öffnet:

| Weg | Bedingung | Stand |
|---|---|---|
| `GeneralAbility`, Radiant Aegis | `BMRShouldRefreshBefore(BMRRaidwideIn, …)` | **ab Werk tot**: der Helfer gibt sofort `false` zurück, wenn `UseBmrTimeline` aus ist, und `_useBMRTimeline` ist auf `false` voreingestellt |
| `DefenseSingleAbility` | für RangedMagical nur `IsHostileCastingTankBusterAtMe`, oder BMR-Tankbuster **ohne lebenden Tank** | greift im Gruppenpull praktisch nie; die Einschränkung auf gesicherte Tankbuster stammt aus A9/C10 — seiner eigenen Meldung, dass es zu oft feuerte |
| `DefenseAreaAbility` | `AutoStatus.DefenseArea` | einziger verbliebener Weg |

`AutoStatus.DefenseArea` wiederum hat zwei Zweige: die BMR-Timeline (ab Werk aus, siehe oben) und `IsHostileCastingAOE`. Letzterer verlangt **kumulativ**: ein Gegner castet, der Cast ist **nicht unterbrechbar**, er dauert länger als ein GCD, seine Restzeit liegt zwischen einem und zwei GCDs, die Aktions-Id steht in `HostileCastingArea`, der Effekt erreicht den Spieler — und seit A101 zusätzlich, dass das gemessene Schadenspotential niemanden unter die Heilschwelle drückt.

**Damit ist der Befund kein einzelner Fehler, sondern eine Pendelbewegung.** A9 hat zwei Auslöser entfernt, die dauerhaft anstanden (der Gegnerzahl-Fallback in `ShouldAddDefenseArea`, Radiant Aegis bedingungslos in `GeneralAbility`) — beides zu Recht, beides auf seine Meldung „zu oft, obwohl keine Gefahr vorliegt". Übrig blieb ein Pfad, der bei Dungeon-Trash kaum je auslöst, weil dessen Casts überwiegend unterbrechbar und kurz sind. A9 vermerkte als Rückfall „bleibt über die BMR-Raidwide-Vorhersage und die Defense-Pfade"; die BMR-Vorhersage ist ab Werk abgeschaltet, und die Defense-Pfade sind der eben beschriebene Engpass. Der Rückfall trug also nicht.

**Nicht ohne den Auftraggeber zu behebende Frage:** Welche der dokumentierten Entscheidungen fallen soll. Seine Beobachtung belegt, **dass** nichts kommt, nicht **welche** Schraube zu drehen ist — der Gegnerzahl-Fallback, die Tankbuster-Einschränkung, die Voreinstellung von `UseBmrTimeline` oder die Schärfe des Cast-Vorfilters. Vorlage mit Optionen und Empfehlung steht aus.

**Der wahrscheinliche Auslöser des gemeldeten Bildes ist behoben** (A108): Die Bewertung aus A101 fragte allein, ob der Treffer Heilbedarf erzeugt, und verneinte das bei gesunder Gruppe für jeden Anteil unter 0,35. Seit der belegten Obergrenze — ab 0,25 der Maximalgesundheit ist die Fläche groß, unabhängig vom Zustand der Gruppe — greift sie wieder. Was davon im Spiel ankommt, ist offen: Die Sonde `AreaMitigationSkipped` in der Listenverwaltung zeigt je Aktion, ob die Regel noch etwas verwirft.

**Offen bleibt die Pendelbewegung selbst.** Auch mit greifender Bewertung führt der Weg zu Radiant Aegis und Addle beim Beschwörer allein über `AutoStatus.DefenseArea` und dessen Cast-Vorfilter (nicht unterbrechbar, länger als ein GCD, Restzeit zwischen einem und zwei GCDs). Welche der dokumentierten Entscheidungen zu lockern ist — der Gegnerzahl-Fallback, die Tankbuster-Einschränkung, die Voreinstellung von `UseBmrTimeline` oder die Schärfe des Vorfilters —, ist ohne den Auftraggeber nicht zu entscheiden; seine Beobachtung belegt, **dass** nichts kommt, nicht **welche** Schraube zu drehen ist.

**Der Wirkungsbereich reicht über die Minderung hinaus und ist vor jeder Korrektur zu erheben:** `IsHostileCastingAOE` speist auch `ObjectHelper.IsUnderThreat`, und darüber hängt die Gefahrenprüfung der Notfallheilung am Weißmagier (`BenedictionNeedsThreat`) an derselben gefilterten Größe. Eine Änderung am Filter verschiebt beide Pfade zugleich; die Bewertung je Pfad steht aus.

**Zeitliche Einordnung:** A66 (11.09.) führt seine Meldung „schimmerschild klappt bislang", die Bewertung stammt vom 17.09. (`1bc46119a`). Das Fenster deckt sich mit der Beobachtung.

### Die Aufnahme in die AoE-Liste unterscheidet Raidwide und ausweichbare Fläche nicht · N

`Watcher.ActionFromEnemy` nimmt eine Gegneraktion dauerhaft in `HostileCastingArea` auf, wenn die Gruppe mindestens vier Mitglieder hat, die Aktion eine Wirkzeit besitzt, zur Kategorie Spell/Weaponskill/Ability gehört und **jedes** Gruppenmitglied im selben Effektsatz Schaden genommen hat. „Record AOE actions" ist ab Werk an.

**Was daran offen ist, ist die Aufnahme, nicht mehr die Bewertung.** Die Größenordnung ist seit A99 bis A102 gemessen, gespeichert und wird beim Verbrauch verrechnet — vollständiger Stand in `docs/rotation-flow/13-aoe-damage-classification.md`. Damit kostet eine zu klein bewertete Aktion keine Abklingzeit mehr. Die Aufnahme selbst bleibt grob: Ob sich echte Raidwides beim Lernen von ausweichbaren Flächen unterscheiden lassen, ist ohne Spieldaten nicht entscheidbar (Kandidaten: `CastType`, `EffectRange`), und die Reichweitenprüfung `AreaCastCanReachPlayer` entschärft den Fall nur für den Spieler selbst.

**Bewertung: technische Schuld, kein Defekt.** Eine Verschärfung der Aufnahmebedingung wäre eine Verhaltensänderung ohne Nachweismöglichkeit und gehörte deshalb hinter eine eigene Option. Geprüfte Nicht-Fehlstelle: das Speichern läuft asynchron, kein blockierendes Schreiben im Kampfpfad.

**Drei benannte Grenzen der Bewertung, alle erfasst und keine behoben:**

- **Serien kleiner Einschläge.** Mehrere kleine Treffer kurz hintereinander summieren sich; jeder einzeln unter jeder Schwelle, zusammen tödlich. Eine Einzelwertprüfung sieht das nicht. Die Ablage trägt einen Anteil je Aktion, keine Folge über die Zeit.
- **Der VFX-Zweig umgeht die Rechnung.** `IsCastingAreaVfx` erkennt Stack- und Spread-Marker über Effektpfade statt über Aktions-Ids; für die gibt es kein Potential, also greift die Bewertung dort nicht. Konsistent mit „unbewertet heißt mindern", aber diese Auslöser bleiben grob.
- **Die Rückrechnung um die wirkende Minderung ist verworfen, nicht vergessen.** `GetCurrentMitigationPercent` ist eine Aufzählung bekannter Status, also im Zweifel unvollständig — und eine Näherung, die den Wert **erhöht**, ist gefährlicher als eine Beobachtung, die ihn zu niedrig ansetzt und sich beim nächsten ungeminderten Treffer selbst korrigiert (Begründung in Konzept 13, Abschnitt Falsifikation).

**Auflösungsbedingung:** eine Spielbeobachtung, die die Aufnahme ausweichbarer Flächen als Kostenfaktor belegt. Die Sonde in der Listenverwaltung liefert dafür jetzt die Grundlage — sie nennt je Eintrag den gemessenen Anteil.

### Dieselbe Frage steht bei Tankbustern, Rückstoß und Unterbrechung offen · N

**Konzept:** `docs/rotation-flow/08-mitigation-synergy.md`, `docs/rotation-flow/13-aoe-damage-classification.md`
`HostileCastingTank` trägt sie wörtlich — wie hart schlägt dieser zu —, `HostileCastingKnockback` und `HostileCastingStop` dieselbe Struktur. Der Messpfad im Effekt-Handler ist derselbe; was fehlt, ist je Liste ein eigener Speicher und die passende Rechnung. Beim Tankbuster ist der Vergleichspartner nicht der Gruppendurchschnitt, sondern der Puffer **des Tanks**, und die Frage lautet „übersteht er ihn ohne Minderung".

**Erfasst, nicht bearbeitet.** Die Übertragung verlangt je Liste eine eigene Entscheidung darüber, gegen wessen Puffer gerechnet wird; die Flächenfassung ist zuerst im Spiel zu beurteilen.

### `InitOne` lädt eine unlesbare kuratierte Liste nicht erneut herunter · N, U

Upstream-Verhalten, alle vier gelernten Listen betreffend. Der Ladepfad prüft auf **Existenz** der Datei, nicht auf Lesbarkeit: Ist sie vorhanden und unlesbar, wird mit einer leeren Liste begonnen, und der Download bleibt aus. Für eine kuratierte Liste kostet das einen Knopfdruck, für die gemessenen Potentiale kostete es die gesammelten Erfahrungswerte.

**Die Ursache ist weitgehend entfernt** (A100): Geschrieben wird seit dem über eine temporäre Datei und einen Move, und eine unlesbare Datei wird als `.corrupt` beiseitegelegt und gemeldet, statt still verworfen. Was bleibt, ist der fehlende Neu-Download.

**Nicht behoben, weil der Zweig eine eigene Frage aufwirft:** Was soll geschehen, wenn kein Netz da ist? Ein blockierender Versuch im Startpfad ist keine Option, ein stiller Fehlschlag wäre der heutige Zustand mit mehr Code. **Empfehlung: erfassen, Adressat ist der Upstream.**

### Die globalen Einstellungen zeigen kein Symbol für ihre Erklärung · N, U

Gefunden bei der Erhebung der Fork-Einstellungen (A103). Die beiden Tooltip-Wege verhalten sich verschieden, und der Unterschied trifft die Sichtbarkeit:

| | Rotationseinstellungen (`RotationConfig.Tooltip`) | Globale Einstellungen (`UI.Description`) |
|---|---|---|
| Symbol | `(?)` neben der Zeile, gezeichnet **nur** bei vorhandenem Tooltip | keines |
| Auslöser | Überfahren des Symbols | Überfahren der Einstellung |
| Schalter „Show tooltips" | wirkt **nicht**, das Symbol nutzt ImGui direkt | wirkt — bei Aus erscheint nichts |

**Wirkung:** Bei einer globalen Einstellung ist nicht erkennbar, dass eine Erklärung vorliegt; wer nicht zufällig darüberfährt, findet sie nie. Und wer „Show tooltips" abgeschaltet hat, bekommt sie überhaupt nicht, während die Rotations-Erklärungen weiter erscheinen — derselbe Nutzer, zwei verschiedene Antworten.

**Nicht behoben, weil der Wirkungsbereich den Auftrag überschreitet.** Das Symbol nachzuziehen heißt, `Searchable.ShowTooltip` beziehungsweise die sieben Zeichner in `RotationSolver/UI/SearchableConfigs/` anzufassen; das trifft **alle** Upstream-Einstellungen mit Beschreibung, nicht nur die des Forks, und erzeugt Merge-Aufwand bei jeder Upstream-Änderung an diesen Dateien. Der Auftrag betraf die Erklärungen der Fork-Einstellungen.

**Auflösungsbedingung:** Freigabe des Auftraggebers für den Eingriff in den gemeinsamen Zeichenpfad. Die kleinere Variante wäre, das Symbol nur dort zu zeichnen, wo eine Beschreibung vorliegt — das ist genau die Bedingung, die der Rotationspfad schon benutzt, und damit kein neues Verhalten, sondern die Übernahme des vorhandenen.

### Zwei Werte des Dunkelritters hängen nicht an der Strategie, für die sie gelten · N

**Konzept:** `docs/rotation-flow/10-drk-blackest-night.md`
`DRK_Reborn.BlackestNightMinHostiles` und `BlackestNightHealthRatio` gelten nur für die engeren Optionen von `BlackestNightUsage` — die Gegnerzahl für zwei davon, die Gesundheitsschwelle für eine. Beide tragen kein `Parent`, erscheinen also immer und unverändert eingerückt, auch wenn die eingestellte Strategie sie gar nicht liest. Ihre Labels verwiesen zusätzlich auf „die Option oben", obwohl zwischen ihnen und `BlackestNightUsage` andere Einstellungen stehen; das ist mit A103 berichtigt, die Kopplung nicht.

**Warum nicht mitbehoben:** `ShouldShowRotationConfigInternal` vergleicht `ParentValue` gegen **einen** Wert. Für die Gesundheitsschwelle wäre die Kopplung damit korrekt möglich, für die Gegnerzahl nicht — sie gilt für zwei Strategien. Eine halbe Kopplung wäre schlechter als keine: Sie ließe den Nutzer glauben, die sichtbaren Werte seien genau die wirksamen.

**Auflösung:** entweder `ParentValue` auf mehrere zulässige Werte erweitern — Wirkungsbereich ist der gemeinsame Zeichenpfad aller Rotationen, Betroffenenkreis R und U — oder es beim Tooltip belassen, der jetzt sagt, für welche Option jeder Wert gilt. **Empfehlung: beim Tooltip belassen**, solange kein zweiter Fall dieser Art auftritt; der Nutzen ist eine Einrückung, die Kosten sind eine Signaturerweiterung im Upstream-Pfad.

### Die Holy-Vorbehalte des Weißmagiers entscheiden ohne jede Sonde · N

**Konzept:** `docs/rotation-flow/08-mitigation-synergy.md`
Gefunden bei der Erhebung der Defektklasse „Regel entscheidet im Kampf, niemand kann sehen, ob sie greift" (A102). Drei Vorbehalte halten Sanctus zurück — `ShouldStretchHolyStun`, `ShouldHoldHolyForBarrier`, `ShouldHoldHolyWhilePackSlowed` —, und keiner von ihnen hinterlässt eine Spur. Am Bildschirm ist ein zurückgehaltenes Sanctus nicht von einem unterscheidbar, das aus einem anderen Grund ausblieb.

**Das ist dieselbe Klasse, die bei der Flächenbewertung behoben wurde**, und sie ist dort wie hier durch die Cynefin-Regel gefordert: In der komplexen Domäne liegt die Antwort im Handeln, also ist das Messmittel mitzuliefern. Der Bedarf ist hier belegt und nicht vermutet — für `StretchHolyStun` steht als offener Punkt genau die Beobachtung aus, ob die Streckung im Spiel eintritt, und ohne Sonde ist sie nicht zu machen.

**Erfasst, nicht bearbeitet:** Der laufende Auftrag betraf die Flächenbewertung; ein Eingriff in den Sanctus-Pfad ist eine eigene Sache. **Auflösung:** je Vorbehalt die Aktions-Id und der Grund des Rückhalts in der Diagnoseanzeige, nach derselben Bauform wie `DataCenter.AreaMitigationSkipped` — Vermerk an der Entscheidungsstelle, Anzeige im Debug-Fenster.

### `SpreadDamagePaths` enthält keinen Spread-Marker · N

**Konzept:** `docs/rotation-flow/13-aoe-damage-classification.md`
`DataCenter.SpreadDamagePaths`. Zwei der vier Pfade stehen wortgleich in `SharedDamagePaths`, die anderen beiden sind laut eigenem Kommentar „AOE share markers", also ebenfalls Stack-Marker. Ohne Fehlwirkung, weil `IsCastingAreaVfx` alle drei Listen prüft. **Kosten:** eine Kategorie, die etwas anderes verspricht, als sie enthält. Nebenbefund: `SharedDamagePaths` führt `vfx/lockon/eff/com_trg01_0c` zweimal (2022 und 2024), im `FrozenSet` folgenlos.

**Entstehung belegt** (A41): eingeführt in `33e6acb1` vom 07.05.2026, einem Sammel-Refactoring („Refactor for var usage, safety checks, and plugin compat"), und zwar bereits mit den beiden Duplikaten und dem übernommenen Kommentar. Das ist *Ignorant Surgery* nach Parnas — Klon einer Nachbarliste ohne Anpassung —, kein späteres Veralten. Die Historie war dafür zu vertiefen; im flachen Klon lag der Einführungs-Commit vor dem Anfang.

**Empfehlung: Nullvariante, Punkt offen führen.** Für RSR ist der Unterschied ohne Verbraucher — Spread wie Stack lösen dieselbe Gruppenmitigation aus, eine Positionierung leitet das Plugin nicht. Die Kategorie zu füllen setzt Spieldaten voraus, die hier fehlen: Eine Websuche nach den beiden einzigartigen Pfaden (`x6fd_loc04m_5s1v`, `m0922tar_a0w`) bleibt ohne Treffer, ihre Zuordnung ist damit unbelegt. Die Liste zu streichen ist nur dann verhaltensneutral, wenn genau diese beiden nach `SharedDamagePaths` übernommen werden — und tilgt dann das Signal, das den offenen Punkt sichtbar hält. Wer dennoch aufräumen will: die kleinste verhaltensneutrale Fassung ist das Entfernen der beiden Duplikate `x6r9_loc01_t0a1` und `x6r9_loc02_t0a1` aus dieser Liste.

### Release-Paket enthält vermeidbaren Ballast · N, R

**Am Artefakt belegt** (`latest.zip` von 7.5.5.41+wsh1, 5,35 MB): Nutzlast sind `RotationSolver.dll`, `RotationSolver.Basic.dll`, `ECommons.dll` und `RotationSolver.json`; dazu kommen `RotationSolver.Basic.xml` (7,52 MB), der Analyzer samt Symbolen (5,59 MB), das NuGet-Paket (1,54 MB) und `RotationSolver.Basic.pdb` (1,27 MB).

**Kein Fork-Defekt, sondern Upstream-Verhalten** (A40): Die Upstream-Releases 7.5.5.41 und 7.5.6.0 enthalten dieselben zwölf Dateien in denselben Rollen, 5,34 MB gegenüber 5,35 MB. Der Fork weicht im Dateibestand nicht ab; auch der Abhängigkeitsgraph beider `deps.json` ist deckungsgleich. Eine Bereinigung wäre damit eine eigene Abweichung im Verpackungspfad.

**Ursache** ist weder die Prune-Regel — `PruneOutputDlls` arbeitet auf `ReferenceCopyLocalPaths` und erfasst nichts davon — noch das `OutputPath` der Projektdatei: `publish.yaml:42` baut mit `--output .\build`, wodurch die Ausgaben aller beteiligten Projekte in einem Verzeichnis landen, das DalamudPackager packt. `GeneratePackageOnBuild` bedient dabei bewusst die Autoren abgeleiteter Rotationen.

**Kosten:** 5,35 MB Download statt rund 1,8 MB, funktional folgenlos.

**Auflösungsbedingung:** Der naheliegende Weg trägt nicht — der Standard-Target von DalamudPackager reicht `Exclude` nicht durch und läuft nur, solange keine eigene `DalamudPackager.targets` im Projektverzeichnis liegt; diese müsste den vollständigen Task-Aufruf samt aller Manifest-Felder nachbauen. `Exclude` vergleicht exakt über `List.Contains` (im Paket DalamudPackager, nicht in diesem Baum), kennt also keine Muster, und das NuGet-Paket trägt die Version im Dateinamen. Der Build-Workflow kompiliert nur und prüft das Paket nicht. Aufgreifen erst, wenn der Veröffentlichungspfad prüfbar ist. Geprüft: die XML-Dokumentation wird zur Laufzeit nicht gelesen.

**Empfehlung: nicht aufgreifen.** Der Auftraggeber hat die Spielbarkeit des Release-ZIPs zur Anforderung gemacht. Ein Eingriff in den Verpackungspfad ist genau die Klasse von Änderung, deren Ergebnis erst am fertigen Release sichtbar wird — und dieser Pfad läuft nur auf einen Tag, wird von keiner Prüfung abgedeckt und weicht dann zusätzlich vom Upstream ab. 3,5 MB Download stehen gegen das Risiko, ein nicht ladbares Paket zu bauen.

### DRK: Die Zeitpunktwahl für The Blackest Night steht auf dem alten Verhalten · N, U

`BlackestNightUsage` (`DRK_Reborn.cs:35`) bietet drei Stufen für den Selbstschutz-Zweig: wie bisher; Tankbuster oder großer Pull ohne laufende große Minderung; dasselbe plus Gesundheitsschwelle. Voreingestellt ist **die erste**, also das Verhalten, das der Auftraggeber beanstandet hat (A44/A45, Konzept `docs/rotation-flow/10-drk-blackest-night.md`).

**Kosten des Kompromisses:** Wer nichts umstellt, behält die Fehlausgabe — 3000 MP für eine Barriere, die bei mäßigem Schaden nicht aufgezehrt wird und deshalb kein Dark Arts auslöst. Für den Auftraggeber ist das mit einem Klick erledigt; für andere Nutzer des Forks bleibt es der Standard.

**Warum trotzdem so:** Die Beobachtung liegt für ein Nutzungsprofil vor, nicht allgemein. Zwei Größen der engeren Stufen sind zudem Annahmen: dass vier Gegner die Verbrauchsrate von 3,6 % der maximalen Gesundheit pro Sekunde erreichen, und dass ein Tankbuster die unter Minderung angehobene Schwelle mitnimmt.

**Umgesetzt nach Vorgabe des Auftraggebers, Wall-to-Wall.** Steht The Blackest Night, wirkt der Dunkelritter auf einem Gruppenpull weder Reflexion noch Abtausch, und der Weißmagier betäubt nicht mit Sanctus; im Bosskampf gilt die Regel nicht. `HoldMitigationForBarrier()` sperrt die beiden Reflexion-Zweige und den Abtausch-Zweig auf dem Pull, `ShouldHoldHolyForBarrier()` die Betäubungsseite — jetzt **voreingestellt an** und ebenfalls auf den Gruppenpull begrenzt. Begründung und Abgrenzung stehen in `docs/rotation-flow/10-drk-blackest-night.md`. **Offen bleibt allein die Beobachtung im Spiel**, ob die Barriere damit tatsächlich bricht; erreichter Prüfgrad ist statische Prüfung und Compile.

**Die Verlangsamung durch Dritte ist messbar und praktisch vernachlässigbar.** Die Behauptung, sie sei von hier aus nicht zu erfassen, war falsch: Der Debuff steht auf den **Gegnern**, und `PackSlowed` misst ihn bereits genau so — `SurveyHostileStatus(JobRange, SlowStatus, out slowed)`, ohne zu fragen, wer ihn gelegt hat. Ein fremder Abtausch ist damit sichtbar wie der eigene.

Er fällt trotzdem kaum ins Gewicht, und der Grund liegt in der Wirkweise der Aktion: Abtausch verlangsamt nur Gegner, die den **Träger** treffen. Der Tank zieht die Aggro, also schlagen die Gegner ihn und nicht den Schadensausteiler; dessen Abtausch müsste erst Aggro und Treffer abbekommen, bevor überhaupt eine Verlangsamung entsteht. Die Wirkrichtung ist außerdem einseitig: Ein fremder Slow kann das **Zünden** der Barriere verzögern, weil `PackSlowed` es sperrt — die stehende Barriere hat umgekehrt keinen Einfluss darauf, ob der andere Spieler seine Aktion wirkt.

**Empfehlung: nicht bearbeiten.** Eine eigene Regel dafür hätte keinen belegbaren Anwendungsfall.

**Was hier offen bleibt, ist allein die Zündrichtung.** `BlackestNightUsage` entscheidet, *wann* die Barriere fällt; die Sperre oben entscheidet, was danach unterbleibt. Beide Seiten greifen unabhängig voneinander: Auch in der Voreinstellung `WheneverDefensesOpen` hält die neue Sperre die Minderungen zurück, sobald die Barriere steht. Die Voreinstellung der Zündrichtung bleibt beim Upstream-Verhalten, weil die Beobachtung dafür nur für ein Nutzungsprofil vorliegt.

**Auflösungsbedingung:** eine Spielbeobachtung über mehrere Kämpfe — wird die Barriere unter `TankbusterOrHeavyPull` regelmäßig aufgezehrt, und fehlt sie nie dort, wo sie gebraucht wurde, ist der Standard umzustellen. Für die Heilerseite ist zusätzlich zu beobachten, was die Rückhaltung kostet: Sanctus ist der einzige Flächenzauber des Jobs, ein zurückgehaltener GCD fällt auf Einzelzielschaden zurück. Zu beobachten ist dabei auch die Gegnerschwelle: Kommt The Blackest Night im Wall-to-Wall zu selten, ist `BlackestNightMinHostiles` (Vorgabe 4) zu hoch angesetzt. Sie ist bewusst eine **eigene** Option der Rotation und nicht der globale `MitigationSustainHostileCount`, weil die Frage hier eine andere ist: nicht „lohnt eine Minderung", sondern „reicht der Schadensstrom, um 25 % der maximalen Gesundheit in sieben Sekunden aufzuzehren".

### VPR: leerer Zweig einer Struktur, die anderswo eine Entscheidung trägt · U

`VPR_Reborn.cs:591-597` und `975-981`. Das Muster `!HasHunterAndSwift` kommt viermal vor; der Vorspann `!IsHunter && !IsSwift` trägt nur an der Coil-Stelle (751-807) Inhalt, an der Den-Stelle (424-493) fehlt er ganz. Weder ein fehlender Inhalt noch dessen Entbehrlichkeit ist belegbar. **Bewusst nicht gelöscht** (AUDIT_LOG A11): Die Entfernung wäre verhaltensneutral, würde aber die Asymmetrie verdecken, die den Befund sichtbar macht. **Auflösung:** Adressat ist der Upstream. **Empfehlung: belassen** — der leere Zweig ist der einzige Hinweis auf die Lücke, und ohne den Autor ist nicht zu entscheiden, ob Inhalt fehlt oder der Vorspann überflüssig ist.

### `AutodutyUpdateState` dupliziert `UpdateState` · U

`RSCommands_StateSpecialCommand.cs`: rund 100 wortgleiche Zeilen, abweichend nur die Fälle `TargetOnly` und `AutoDuty` (`TargetingTypeOverride = targetingType` statt `null`) und der Zustandstext. **Kosten:** jede künftige Änderung am Zustandsautomaten muss an zwei Stellen erfolgen. **Auflösung:** über einen optionalen `TargetingType?`-Parameter zusammenführen, sobald an dieser Stelle ohnehin gearbeitet wird. **Empfehlung: nicht eigens angehen** — die Duplizierung kostet erst bei der nächsten Änderung am Zustandsautomaten etwas, und genau die ist nach dem Punkt oben ohne Laufzeitbeobachtung nicht zu empfehlen. Beide Punkte lösen sich gemeinsam oder gar nicht.

### Zwei entfernte öffentliche Member seit dem letzten Release · R

`scan7.py` misst die Paketoberfläche von `RotationSolver.Basic` gegen den Tag `7.5.5.41+wsh1`: `CustomRotation_BasicInfo.HasHostileCountAoeMitigation` (`public virtual`) und `ActionConfig.ShouldCheckTargetStatus` sind seither ersatzlos entfallen. Beide Entfernungen sind sachlich belegt — das Flag öffnete die gesamte Defensivkette, die Option las niemand —, aber sie waren im ausgelieferten Paket enthalten.

**Kosten:** Eine abgeleitete Rotation, die das Flag überschreibt oder die Option liest, kompiliert gegen die nächste Paketversion nicht mehr. Für die gespeicherte Nutzerkonfiguration folgenlos, weil Dalamud fehlende Member beim Deserialisieren überspringt.

**Zur Versionsnummer selbst** (A52): Upstream führt im Quellbaum gar keine — `publish.yaml` leitet sie dort erst beim Veröffentlichen aus dem Git-Tag ab. Die drei Zeilen in unserer `Directory.Build.props` sind eine Fork-Ergänzung, die **nichts automatisch nachzieht**; `check_fork_version.py` misst, ob sie noch auf dem Upstream-Release steht, das in unserer Historie liegt.

**Auflösungsbedingung:** Der Ausweis erfolgt in `CHANGELOG.md`, weil die Versionsnummer ihn nicht tragen kann: ihr numerischer Teil folgt dem Upstream-Release, nicht der Kompatibilität dieses Forks, und das Prerelease-Label `-wsh1` kennzeichnet nur die Herkunft (A38). Offen bleibt allein, den Abschnitt „Unreleased" bei der nächsten Versionsvergabe auf die dann vergebene Version zu setzen. Der Zeitpunkt der Veröffentlichung liegt beim Auftraggeber.

**Empfehlung:** mit dem nächsten Tag erledigen, nicht davor — der Abschnitt kann erst auf eine Nummer gesetzt werden, die vergeben ist. Nach dem Upstream-Merge steht der Baum auf `7.5.6.1`; der Tag wäre also `7.5.6.1+wsh1`.

### Plugin-Identität ist unverändert die des Upstreams · N

`manifest.json` ist gegenüber `upstream/main` unverändert: `InternalName: RotationSolver`, `RepoUrl` und `IconUrl` verweisen auf FFXIV-CombatReborn, `AcceptsFeedback` ist gesetzt. Ein Fork-Build meldet sich damit unter derselben Plugin-Identität an wie das Upstream-Plugin. Bisher mitgeführt, nicht als Entscheidung erfasst; aufgefallen beim Abgleich der README gegen den Ist-Stand (A39).

**Kosten — Inferenz, nicht am Artefakt belegt:** Dalamud adressiert Plugin-Verzeichnis, Konfigurationsablage und Aktualisierung über den `InternalName`. Daraus folgte, dass Fork- und Upstream-Installation einander verdrängen, dieselbe gespeicherte Konfiguration benutzen und ein Feedback-Weg beim Upstream endet. Belegt ist allein der Manifest-Inhalt; das Installationsverhalten ist in dieser Umgebung nicht prüfbar.

**Gegenposition:** Die gemeinsame Identität ist zugleich der Nutzen — die Nutzerkonfiguration überdauert einen Wechsel zwischen Upstream und Fork, und die Abweichung zum Upstream bleibt null (Betroffenengruppe U).

**Auflösungsbedingung:** Zu entscheiden erst, wenn beide Installationen nebeneinander gebraucht werden. Ein eigener `InternalName` trennt dann auch die gespeicherte Konfiguration; ohne Migrationspfad verlöre der Nutzer seine Einstellungen. Die Freigabe liegt beim Auftraggeber.

**Empfehlung: belassen.** Die geteilte Identität kostet nur, wenn beide Fassungen gleichzeitig installiert sein sollen; sie nützt bei jedem Wechsel zwischen ihnen, weil die Nutzerkonfiguration erhalten bleibt.

### Übertragung der Mitigations-Synergie auf weitere Doppelnutzen-Aktionen · N

Schritt 3 aus `docs/rotation-flow/08-mitigation-synergy.md`. Die Schritte 1 und 2 — Messung und Aussetzbedingung für Sanctus — sind umgesetzt (AUDIT_LOG A20). Offen ist die Übertragung auf weitere Aktionen, die Schaden erzeugen und zugleich Schaden vermeiden.

**Zur Führung dieses Punktes:** Die Einzelheiten stehen im Konzeptdokument, nicht hier. `TODO.md` führt die offene Arbeit und verweist; eine zweite Beschreibung derselben Sache würde mit der ersten auseinanderlaufen. Was hier stehen muss, ist allein, dass noch etwas offen ist und wo es beschrieben wird.

**Die Kandidatensuche ist erledigt** und lief, wie hier gefordert, über ein Prüfskript statt über Erinnerung: `scan16.py` erhebt jede PvE-Aktion mit Kontroll- oder Minderungswirkung auf Gegner und prüft, ob der Baum den zugehörigen Status liest. Im Tank- und Heilerprofil bleibt genau eine Aktion, deren zweite Wirkung eine Entscheidung ändern würde — Rückstoß (Arm’s Length), eigener Eintrag oben. Offen ist damit nur noch die **Übertragung** selbst.

**Auflösungsbedingung:** erst nach Beobachtung der Schritte 1 und 2 im Spiel.

**Empfehlung: warten.** Schritt 3 überträgt eine Regel, deren Nutzen in den Schritten 1 und 2 noch nicht beobachtet ist; eine Übertragung vor dem Nachweis vervielfacht einen möglichen Fehler, statt einen Nutzen zu vervielfachen.

## Offene Arbeit

### Das Abwehrmittel nach der Größe des Treffers wählen · N, R

**Vorgabe des Auftraggebers:** „man könnte es auch so anpassen, dass die geeignete schadensverringerung bzw. das geeignete schild bei dem eintreffenden schaden gewählt wird." Mit seiner Rechenregel: „wenn schaden nur 10% auf spieler mit geringster maxhp verursacht, dann reicht ein schild, was 10% blockiert. oder sogar weniger bis kein schild. wenn ein schaden 70% verursacht von maxhp des geringsten spielers, dann sollte das schild möglichst hoch sein, optimal 70%."

**Die Regel ist dreistufig** (seine Ergänzung): Liegt der Treffer unter der **aktuellen** Gesundheit des schwächsten Mitglieds, wird in Höhe des Treffers gedeckt — bei kleinen Werten auch gar nicht. Erreicht er sie, wird **zuerst geheilt**, Ziel ist die Maximalgesundheit. Übersteigt er auch die maximale Gesundheit, reicht Heilung nicht: Dann sind Barriere **und** Minderung zusätzlich zu setzen, bis der Rest darunter liegt. Das folgt seiner stehenden Ordnung Heilung vor Minderung.

**Ausgangslage:** Die Größe des eintreffenden Treffers liegt als Anteil je Aktions-Id vor (Konzept 13), und zwar als höchster Anteil im Effektsatz — das ist der Spieler mit der geringsten Maximalgesundheit, also genau seine Bezugsgröße. Heute entscheidet dieser Wert nur, **ob** die Abwehrkette geöffnet wird; die Reihenfolge innerhalb der Kette ist je Job fest verdrahtet.

**Derselbe Baustein schließt vier offene Punkte** — Vorausschau vor dem ersten Treffer, Wahl des Mittels, die Minderungsbilanz ohne Betäubung und Verlangsamung, und Rückstoß als Minderungswerkzeug. Aufstellung in `docs/rotation-flow/08-mitigation-synergy.md`, Abschnitt „Was ein Baustein mehrfach trägt“.

**Was zum Bauen fehlt:** je Abwehraktion ein belegter Wert — für Minderungen der Prozentsatz aus dem eigenen Wirktext, für Barrieren der Anteil (25 %, 15 %, 10 % sind belegt). Erzeugbar aus den Ressourcen, nicht handzuführen.

**Blast Radius und Auflage:** Der Eingriff berührt `DefenseAreaAbility` und `DefenseSingleAbility` jedes Jobs und mittelbar die fremden Rotationen unter `ExtraRotations`. Er gehört deshalb hinter eine eigene Option mit dem bisherigen Verhalten als Voreinstellung, und die Sonde je Entscheidung ist mitzuliefern — sonst ist im Kampf nicht unterscheidbar, ob ein schwächeres Mittel gewählt wurde oder gar keines. Vollständige Bewertung in `docs/rotation-flow/08-mitigation-synergy.md`, Abschnitt „Die Antwort auf einen eingehenden Treffer“; die Messung der Treffergröße steht in `13-aoe-damage-classification.md`.

### Nachprüfung der 73 Commits vom 11. und 12. September 2026 · N, R, U

Der Auftraggeber hat die Arbeit dieser beiden Tage als nicht belastbar zurückgewiesen und angeordnet, sie zur Nachprüfung vorzumerken. Die Liste steht vollständig in `AUDIT_LOG.md` B2, Prüfstand **ZWEIFELHAFT**: 73 eigene Commits ohne Merges und 5 eigene Merge-Commits, 72 davon nur auf `claude/raise-swiftcast-weave-2` und dort noch änderbar.

**Was die Vormerkung besagt:** nichts über den Inhalt. Kein Commit ist damit widerlegt. Zweifelhaft ist der **Prüfstand** — die Belege zu diesen Commits in Teil A stammen aus denselben beiden Tagen und sind Selbstauskunft, also Gegenstand der Nachprüfung und nicht ihre Grundlage.

**Reihenfolge**, nach Wirkung auf den Betroffenenkreis:

1. **Code** — **erledigt** (A86 für die Wiederbelebungs-Commits, A87 für die übrigen); in B2 als NACHGEPRÜFT eingetragen.
2. **Generator und CI** — offen. Falsch erhobene Namen und ein falsch messendes Prüfmittel vergiften jede spätere Aussage, die sich darauf stützt.
3. **Prüfmittel** — offen. Ein Skript, das das Surrogat statt der Wirkung misst, erzeugt stille Nullbefunde; genau diese Fehlerklasse war der Anlass ihrer Überarbeitung.
4. **Doku und sonstiges** — offen, zuletzt, weil eine falsche Aussage dort nur mitträgt, was der Code ohnehin zeigt.

Wie viele Commits in jeder Gruppe noch auf ZWEIFELHAFT stehen, zählt die Tabelle in B2 aus; eine Zahl hier wäre bei der nächsten Nachprüfung überholt, ohne dass etwas fehlschlüge.

**Prüfmaßstab je Commit:** Belegt der Diff, was die Nachricht behauptet? Ist die zugehörige Wirkkette im Code nachvollzogen oder nur erzählt? Welcher Prüfgrad war tatsächlich erreicht — statisch, Prüfskript, Compile, Spiel? Ergebnis je Commit als **KEIN FEHLER**, **KORRIGIERT** oder **VERWORFEN** in B2 eintragen; ZWEIFELHAFT bleibt stehen, bis geprüft.

**Die Fortsetzung beginnt auf Freigabe des Auftraggebers**, nicht aus eigenem Antrieb — er hat die Code-Gruppe ausdrücklich beauftragt, die übrigen drei nicht.

### Rückstoß im Pull nur beim Dunkelritter, nicht bei den übrigen Tanks · N, U

**Konzept:** `docs/rotation-flow/08-mitigation-synergy.md`
Arm's Length (deutsch Rückstoß) ist eine **Rollenaktion**: Paladin, Krieger, Dunkelritter, Revolverklinge und die Nahkämpfer tragen sie alle. Gewirkt wird sie für ihre Verlangsamung bisher nur im Dunkelritter (`DRK_Reborn.ShouldUseArmsLengthOnPull`, A53), weil dort die Pull-Bedingung schon steht und der Auftraggeber diesen Job spielt.

**Warum nicht gleich zentral:** Eine gemeinsame Zeile in `CustomRotation_Ability` träfe jeden Tank und jeden Nahkämpfer auf einmal. Genau diese Bauform hat schon einmal die gesamte Defensivkette geöffnet, statt die eine gemeinte Zeile zu bedienen (C9). Die Übertragung ist deshalb Job für Job zu machen, mit je eigener Schwelle.

**Auflösungsbedingung:** eine Beobachtung beim Dunkelritter, dass die Regel trägt — dann PLD, WAR und GNB nach demselben Muster.

**Empfehlung: warten.** Erst die Wirkung an einem Job sehen, dann übertragen; die Reihenfolge ist dieselbe wie bei der Mitigations-Synergie.

### DRK: Die Betäubungsregel prüft die Tatsache, nicht die Prognose · N

**Konzept:** `docs/rotation-flow/10-drk-blackest-night.md`
Der Pull-Zweig unterbleibt, solange eine **Gruppenbetäubung** läuft: mindestens zwei betäubte Gegner und mindestens die Hälfte der Gegner in Jobreichweite, dazu ein Nachlauffenster von drei Sekunden, solange noch Betäubungsspielraum besteht (`GroupStunRunning`, A48). Was die Regel **nicht** prüft: ob der Heiler gleich betäuben wird. Der Auftraggeber hatte ursprünglich auf „solange der Weißmagier seine drei Betäubungen noch nicht abgearbeitet hat" gezielt — das wäre eine Aussage über den nächsten Zauber eines anderen Spielers.

**Kosten des Kompromisses:** Das Nachlauffenster überbrückt die Lücke zwischen zwei Anwendungen nur pauschal. Ist der Abstand größer als drei Sekunden, kann die Barriere dazwischen fallen und wird von der nächsten Betäubung unterbrochen; ist er kleiner und die Kette endet dort, wartet die Barriere drei Sekunden zu lang.

**Auflösungsbedingung:** eine Beobachtung, wie oft beides im Spiel vorkommt. Fällt der erste Fall auf, ist `headroom` aus `SurveyStuns` die vorhandene Größe für eine schärfere Fassung: Die Sperre gälte dann bis zur Betäubungsimmunität der Gegner (`StunResistance`) statt bis zum Ablauf des Fensters. Zu bedenken ist, dass die Streckung von Sanctus eine Regel **dieses** Plugins ist (`WHM_Reborn.ShouldStretchHolyStun`) — bei einem fremden Heiler greift sie nicht.

### Reihenfolge im Verteidigungspfad des Dunkelritters: teuer vor billig · N, U

**Konzept:** `docs/rotation-flow/10-drk-blackest-night.md`
`DefenseSingleAbility` gibt je Gelegenheit eine Aktion zurück und führt The Blackest Night (Priorität 20) weit vor Reprisal (`:296`, `:301`). Weil die Barriere nur 15 s Abklingzeit hat, gewinnt sie fast jede Gelegenheit; die kostenlose, gruppenweite Minderung landet erst, wenn sie gerade nicht verfügbar ist.

**Behandelt, aber nicht behoben:** Der Pull-Zweig der Option `BlackestNightUsage` verlangt jetzt, dass Reprisal zuerst liegt (A46). Das wirkt nur für den, der die Option umstellt.

**Kosten des Kompromisses:** In der Voreinstellung bleibt die Rangfolge, wie sie ist. **Auflösung:** die Reprisal-Zeilen im Pfad vor The Blackest Night ziehen. Das ist der direktere Weg und trifft alle Lagen — deshalb erst nach einer Beobachtung, ob die Bedingung im Zweig ausreicht.

### Audit + Code-Review der gesamten Codebasis

Umfang: `RotationSolver.Basic` (48k Zeilen) · RebornRotations (21k) · ExtraRotations (15k) · Updaters (4k) · UI (11k) · Commands/IPC/Data (3k). Der ganze Baum, Upstream-Code eingeschlossen. Phasen 1 bis 4 sind abgeschlossen (AUDIT_LOG A8, A10).

- **Kern tief lesen:** Rest von `DataCenter`; `StateUpdater`, `TargetUpdater`, `ActionTargetInfo`, `BaseAction`/`ActionBasicInfo`, `CustomRotation_Ability`/`GCD`, `Watcher`, `MajorUpdater`, `ObjectHelper`/`StatusHelper` sind gelesen.
- **Rotationen je Job:** Dispatch-Reihenfolge, Gates, Status-IDs, Zielwahl; bisher nur über die Scanner abgedeckt, nicht Datei für Datei.
- **`RotationSolver/UI`** jenseits der Paar- und Totcode-Scans.
- **Dokumentation** in `docs/rotation-flow/14-codebase-audit.md` — **noch nicht angelegt**. Die früher hier genannte Nummer 07 ist seit `07-heal-target-priority.md` vergeben; der Verweis zeigte damit auf ein Dokument, das es unter diesem Namen nie geben konnte.

Der zweite Durchgang mit allen Prüfmitteln ist geführt (A82) und hat die Skripte selbst instand gesetzt; als Messlage für den Rest des Blocks ist er damit verbraucht.

**Empfehlung: der nächste Arbeitsblock, sobald die Nachprüfung der vorgemerkten Commits durch ist.** Sie geht vor, weil sie den Bestand betrifft, auf dem jeder weitere Durchgang aufsetzt — ein Prüfmittel aus dem zweifelhaften Fenster misst sonst den Rest des Baums. Von den übrigen offenen Punkten ist dieser hier der einzige, der weder auf eine Entscheidung noch auf eine Spielbeobachtung wartet — die übrigen sind entweder erfasste Fremdbefunde oder brauchen Laufzeitdaten. Sinnvoller Einstieg ist jetzt das Lesen, nicht das Messen: Die Skripte erfassen Muster, die schon einmal aufgefallen sind, und der verbliebene Bestand — der Rest von `DataCenter`, die Rotationen Datei für Datei, die UI — ist genau der Teil, den kein Muster abdeckt.

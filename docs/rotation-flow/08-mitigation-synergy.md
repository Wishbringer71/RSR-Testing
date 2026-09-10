# 08 · Synergie von Schadensvermeidung und Schadenserzeugung

Entwurfsdokument nach ADR-Struktur. Es stellt den geltenden Stand dar; die
Prüfhistorie steht in `AUDIT_LOG.md` (A20).

## Ergebnis

RSR trifft Mitigationsentscheidungen **je Werkzeug und reaktiv**. Es gibt keine
Stelle, an der beantwortet wird, wie viel Schadensvermeidung gerade anliegt und ob
ein weiteres Werkzeug daran noch etwas ändert. Jedes Werkzeug, das redundant fällt,
kostet zugleich einen GCD oder Weave-Slot, der Schaden erzeugt hätte — **die
Vermeidung von Überlappung dient beiden Zielen gleichzeitig**, weshalb das Thema als
Synergiefrage geführt wird.

Gewählt ist eine **zentrale Bremse bei dezentraler Auslösung**: Die vorhandenen
Auslöser bleiben, hinzu kommt eine Prüfung, die *zurückhält*. Fällt sie aus,
verhält sich RSR wie zuvor. Umgesetzt ist das zuerst am Beispiel Sanctus beim Weißmagier:
Der Zauber wird ausgesetzt, wenn seine Betäubung dadurch gestreckt statt
überschrieben wird und ein Cast mit eigenem Wert bereitsteht. Inzwischen liegt
eine zweite Anwendung derselben Bremse in der Gegenrichtung — der Weißmagier
hält Sanctus zurück, solange die Barriere eines Dunkelritters aufgezehrt werden
muss (Konzept 10) — und eine dritte für die Verlangsamung, die bis dahin
überhaupt nicht gelesen wurde.

| Baustein | Stand |
|---|---|
| Messung: `SurveyStuns`, `SurveyHostileStatus`, `StatusHelper.StunStatus` und `SlowStatus` | umgesetzt in `CustomRotation_OtherInfo` und `StatusHelper` |
| Aussetzbedingung am Sanctus-Block, hinter `StretchHolyStun` (Standard aus) | umgesetzt (`WHM_Reborn.ShouldStretchHolyStun`) |
| Erhebung der übrigen Doppelnutzen-Aktionen | umgesetzt als `scan16.py`; ein Fund im Tank-/Heilerprofil (Armlänge) |
| Zweite Aktion nach ihrer stillen Wirkung geregelt: Armlänge verlangsamt | umgesetzt in der Barrierenregel (Konzept 10, `DRK_Reborn.PackSlowed`) |
| Armlänge auch **als** Minderungswerkzeug wirken | offen, siehe `TODO.md` — Zielkonflikt mit ihrer Rolle als einziger Rückstoßschutz |
| Wirksamkeitsmessung im Spiel | offen, Voraussetzung für weitere Übertragungen |

## Warum die Streckung richtig ist

### Die Größenordnung

Die Betäubungsressource ist **einmalig je Pull**, nicht wiederkehrend: Erste
Anwendung 4 s, zweite 2 s, dritte 1 s, danach 45 s Immunität. Ein stehender
Gruppenpull ist selten länger als diese 45 s, das Zurücksetzen der Resistenz fällt
also praktisch nicht mehr in den Kampf. Zu verteilen sind damit genau **7 Sekunden
Betäubung**, und die Frage lautet nicht, wie viele Zyklen man unterbringt, sondern
wie man diese sieben Sekunden legt.

**Posten 1 — Ausnutzung der Dauern.** Bei einem GCD von rund 2,5 s deckt
ununterbrochenes Nachcasten 0–4,5 s und 5,0–6,0 s ab, zusammen etwa **5,5 s**: Die
zweite Anwendung fällt, während die erste noch läuft, und ihre kürzere Dauer verfällt
teilweise. Wird jeweils erst nach Ablauf nachgecastet, stehen die vollen **7 s** zur
Verfügung.

**Posten 2 — Entzerrung gegen fremde Mitigation.** Reprisal senkt den Gegnerschaden
um 10 % für 10 s. Liegt die Betäubung darin, ist ihr Reprisal-Anteil verloren:
7 s × 10 % = **0,7 s** je Gegner.

| | überlappend | entzerrt |
|---|---|---|
| Betäubungsdauer genutzt | 5,5 s | 7,0 s |
| Reprisal-Anteil erhalten | 0,3 | 1,0 |
| **Summe je Gegner** | **5,8** | **8,0** |

Posten 1 ist damit mehr als doppelt so schwer wie Posten 2: Die Streckung trägt den
Nutzen, die Abstimmung mit fremder Mitigation ist ein Zusatz. Modell, keine Messung —
Annahmen: gleichmäßiger Schaden, stehender Kampf, alle Werkzeuge verfügbar,
Nachcasten im GCD-Takt.

### Die Gegnerzahl gibt den Ausschlag nicht

Naheliegend ist, der Einschub lohne mit steigender Gegnerzahl zunehmend. Das trifft
für den absoluten Betrag zu, für die Entscheidung nicht: **Der Preis skaliert mit
derselben Zahl.** Betäubungsgewinn und Sanctus-Verlust wachsen beide linear mit der
Gegnerzahl, ihr Verhältnis bleibt konstant. Nicht mitskaliert nur der DoT-Anteil: Ein
Einzelziel-DoT bringt unabhängig von der Gegnerzahl denselben Betrag, während sein
Preis mit ihr steigt.

| Posten | Nutzen skaliert mit n | Preis skaliert mit n | Folge |
|---|---|---|---|
| Betäubungsstreckung | ja | ja | gegnerzahl-neutral |
| DoT-Einschub | nein | ja | lohnt bei **wenigen** Zielen |

Die Gegnerzahl bleibt deshalb als **Untergrenze** sinnvoll — unterhalb von drei Zielen
fällt Sanctus wegen `AoeCount` ohnehin nicht, und Einzelziele sind meist
betäubungsimmun. Sie ist ein Filter, kein Gewicht.

### Vermiedener und erzeugter Schaden sind nicht verrechenbar

Die Aufgabe eines Heilers ist, Tank und Gruppe am Leben zu halten; Schaden ist
wichtig, steht aber im Rang darunter. Die beiden Größen bilden eine **Rangordnung,
keine Verrechnung**: erst Überleben sichern, dann Schaden maximieren.

Daraus folgt eine Umkehrung der Beweislast. Die gestreckte Betäubung liefert mehr
Deckung als das Nachcasten im GCD-Takt — 7 s gegen 5,5 s — und ist damit in einer
Rangordnung mit Schadensvermeidung an erster Stelle **immer** die richtige Wahl,
sobald sie anwendbar ist. Nicht die Streckung braucht eine Rechtfertigung, sondern
ihr Unterlassen.

Zwei Einschränkungen stehen dagegen und dürfen nicht aufgelöst werden:

- **Mitigation hat keinen linearen Wert.** Sie ist folgenlos, solange niemand stirbt,
  und entscheidend, wenn sie einen Tod verhindert. Im gut gehaltenen Trash-Pull
  vermeidet die Streckung Schaden, den niemand gespürt hätte — die Rangordnung greift
  dort ins Leere, schadet aber auch nicht.
- **Schaden ist selbst eine Form der Schadensvermeidung.** Ein schneller Kill verkürzt
  den Kampf. Der Anteil des Heilers am Gruppenschaden ist allerdings klein, weshalb
  dieser Rückkopplungseffekt den Vorrang nicht umkehrt.

**Warum die Architektur das nicht von selbst löst.** Der Dispatch prüft Heilung und
Verteidigung vor dem Schadenszweig, setzt die Rangordnung also bereits um — aber nur
**reaktiv**: Ein Heilflag entsteht, wenn HP fehlen. Eine Betäubung ist Prävention und
wirkt, bevor Heilbedarf sichtbar wird. Für Prävention hat die Architektur keinen Ort,
und deshalb liegt die Betäubung im Schadenszweig — nicht weil sie nachrangig wäre,
sondern weil sie dort mangels Alternative gelandet ist. Das ist der eigentliche Befund
hinter der gesamten Frage.

## Die Lücke

**Aktionen mit doppelter Wirkung sind nur nach einer ihrer Wirkungen eingeordnet.**
Sanctus steht im Schadenszweig; dass es betäubt, ist im Entscheidungsmodell nicht
vorhanden. Assize steht im Angriffs-oGCD (`WHM_Reborn.cs:308`); dass es heilt,
ebenfalls nicht. Umgekehrt kennt `GetCurrentMitigationPercent` die Wirkung von
Reprisal, aber weder Betäubung noch Verlangsamung, obwohl beide wie
Schadensreduktion wirken. Beide sind inzwischen an der Stelle gelesen, an der sie
eine Entscheidung ändern — der Barrierenregel des Dunkelritters —, in der
Minderungsbilanz selbst aber weiterhin nicht.

Die Erhebung dazu ist geführt und liegt als `scan16.py` im Repository: Sie nimmt
jede PvE-Aktion, deren Wirktext eine Kontroll- oder Minderungswirkung auf Gegner
nennt, und fragt, ob der Baum den zugehörigen Status irgendwo liest. Im
Tank- und Heilerprofil bleibt **eine** Aktion übrig, deren zweite Wirkung
nirgends gelesen wurde: **Armlänge**. Sie ist als Rückstoßschutz eingeordnet
(`CustomRotation_Ability.AntiKnockbackAbility`), legt aber zugleich
Verlangsamung +20 % auf jeden physischen Angreifer für 15 Sekunden. Der Wirktext
der Verlangsamung nennt ausdrücklich die Verzögerung der **Automatikangriffe**,
aus denen Trash-Gegner den Großteil ihres Schadens liefern — die Wirkung liegt
damit in derselben Größenordnung wie Rampart. Die übrigen Treffer der Erhebung
liegen in Bozja und den Tiefen Gewölben, also außerhalb des Nutzungsprofils, und
sind erfasst statt bearbeitet.

Zweite, unabhängig belegte Fundstelle: Im Schadenszweig steht der Sanctus-Block
**vor** dem DoT-Block. Sobald `AoeCount = 3` erfüllt ist, greift Sanctus und der DoT
wird nie gesetzt — bei einem Boss mit zwei Adds läuft also nie Dia. Dass das nicht
gewollt ist, zeigt der DoT selbst: `ModifyDiaPvE` (`WhiteMageRotation.cs:300-311`)
setzt `TargetStatusProvide` gegen Nachlegen und `IsRestrictedDOT` gegen ungeeignete
Ziele — beide Vorkehrungen laufen bei AoE ins Leere, weil der Zweig nicht erreicht
wird.

## Die Regel

Drei Gesichtspunkte sind eine einzige Frage: **Wann lohnt es sich, einen GCD nicht in
Sanctus zu stecken?**

- **Der Grund:** Der DoT fehlt oder läuft aus, der eingeschobene Cast hat eigenen Wert.
- **Das Timing:** Läuft die Betäubung noch, streckt der Einschub sie, statt sie zu
  überschreiben. Ist das Ziel bereits resistent oder immun, ist die Betäubung ohnehin
  kein Argument mehr.
- **Der zweite Grund für dasselbe Timing:** Läuft eine fremde Mitigation, ist ein Stun
  jetzt weniger wert als später.

> Ein Nicht-Sanctus-GCD wird eingeschoben, wenn er eigenen Wert hat **und** die
> Betäubung dadurch nicht verloren geht — weil sie noch läuft, weil sie ohnehin nicht
> mehr wirkt, oder weil gerade eine stärkere Mitigation trägt.

### Ein einziger Einschub genügt

Bei einem GCD von 2,5 s und Stundauern von 4 / 2 / 1 s:

| Verlauf | Betäubungsdeckung | Intervalle |
|---|---|---|
| ohne Einschub, Casts bei 0 / 2,5 / 5,0 | **5,5 s** | 0–4,5 · 5,0–6,0 |
| ein Einschub, Casts bei 0 / 5,0 / 7,5 | **7,0 s** | 0–4,0 · 5,0–7,0 · 7,5–8,5 |
| zwei Einschübe, Casts bei 0 / 5,0 / 10,0 | **7,0 s** | 0–4,0 · 5,0–7,0 · 10,0–11,0 |

Der zweite Einschub bringt nichts mehr und kostet einen weiteren GCD. Der DoT reicht
also exakt aus. Die Zahlen stammen aus `.github/scripts/audit/stun_coverage.py`, das
die Regel GCD für GCD simuliert und im Repository liegt; bei verkürztem GCD (2,0 s
unter Presence of Mind) liefert die Regel sogar lückenlose Deckung von 0 bis 7 s.

### Warum die Bedingung so lautet, wie sie lautet

**Nicht über eine Restzeit, sondern über zwei Wahrheitswerte.** Eine Restzeit
beschreibt einen einzelnen Gegner. Im Pull kommen laufend ungestunnte Gegner hinzu;
dann sagt die Restzeit über die bereits Betäubten nichts über die Neuzugänge, und die
Regel würde strecken, obwohl ein Cast die Neuen mit **voller** Dauer erwischt hätte —
die Resistenz zählt je Gegner. Gestreckt wird deshalb, wenn **alle** Gegner im Radius
betäubt sind, oder wenn **keiner** von ihnen noch betäubt werden kann — in
beiden Fällen zusätzlich nur, solange überhaupt **eine Betäubung läuft**. Ohne
diesen Zusatz griff die zweite Hälfte auch nach der abgearbeiteten
Betäubungskette, wenn alle Gegner immun und keiner mehr betäubt ist: Dort gibt es
nichts zu schützen, und die Regel tauschte für den Rest des Pulls einen
Flächenzauber gegen einen Einzelzielzauber (A50). Bei einem einzelnen Gegner ist
die Bedingung gleichbedeutend mit der Restzeit-Bedingung.

Eine Schwelle der Form `Restzeit > ein GCD` wäre zusätzlich falsch gewesen: Nach dem
ersten Sanctus beträgt die Restzeit beim nächsten GCD noch 1,5 s, die Regel hätte den
einen nötigen Einschub gerade verhindert.

**Über den Wirkradius, nicht über die Jobreichweite.** `SurveyStuns` bekommt den
Wirkradius der Aktion (`HolyIiiPvE.Info.EffectRange`), nicht `DataCenter.JobRange` —
für einen Heiler sind das 25 Yalms Angriffsreichweite, nicht der Wirkradius von
Sanctus. Über die Jobreichweite gemessen hätten ferne, nie betäubte Gegner die Deckung
dauerhaft unvollständig erscheinen lassen und die Regel nie greifen lassen.
Nebenwirkung dieser Wahl: Die Messung gehört zur Aktion, nicht in einen
jobunabhängigen Updater — die Einhängung in den `MajorUpdater` und die dafür nötige
Änderung der Aktualisierungsreihenfolge entfallen.

**Über eine Statusgruppe, nicht über eine einzelne Id.** Die Spieldaten führen `Stun`
und rund ein Dutzend Varianten mit Zahlensuffix; welche davon Sanctus anlegt, ist
offline nicht bestimmbar. `StatusHelper.StunStatus` fasst sie nach dem im Projekt
etablierten Muster zusammen. Damit ist die Frage gegenstandslos, und fremde
Betäubungen zählen mit — was erwünscht ist, weil auch sie Schaden verhindern.

**Mit Ersatzgarantie.** Sanctus wird nur ausgesetzt, wenn ein Cast mit eigenem Wert
bereitsteht (`DiaPvE`, `AeroIiPvE`, `AeroPvE`). Fehlt er, ist Sanctus die richtige
Wahl; ein Ausweichen auf Glare wäre ein reiner Verlust.

## Vorhandene Bausteine

Der Entwurf erfindet wenig; das meiste lag im Baum und war nur nicht verbunden.

| Baustein | Fundstelle | Zustand |
|---|---|---|
| Mitigationsmessung: Gegner-Debuffs und Party-Buffs, verrechnet zu einem Schadensfaktor | `CustomRotation_OtherInfo.cs:534` | Gelesen nur zur Anzeige (`RotationConfigWindow.cs:4863`) |
| Betäubung, Verlangsamung und **deren Resistenzen** als Statuseffekte | `StatusID.Stun`, `.StunResistance`, `.Slow`, `.SlowResistance`, `.ArmsLength` | In den Spieldaten vorhanden; die Resistenzstufe ist damit direkt lesbar, eine eigene Buchführung über den Ereignisstrom ist **nicht** nötig |
| Statusabfragen mit Restzeit und Stapelzahl | `StatusHelper.HasStatus`, `.StatusTime`, `.StatusStack` | In Betrieb |
| Vorhersagefenster aus der BossModReborn-Timeline | `Configs.cs:742`, `:747`, ausgewertet in `StateUpdater.cs:185` | In Betrieb |
| Zentralisierte Nachzieh-Regel für Gegner-Debuffs | `CustomRotation_OtherInfo.cs:1327` | In Betrieb, 27 Aufrufstellen — Beleg, dass eine gemeinsame Regel über viele Jobs trägt |
| Gegnerzahl-Schwelle als etabliertes Muster | `Configs.MitigationSustainHostileCount` gegen `NumberOfHostilesInRange` | In Betrieb |
| Trennung von Mitigation und Schaden im Dispatch | `CustomRotation_GCD.cs`: HealArea 240, HealSingle 282, DefenseArea 322, DefenseSingle 337, GeneralGCD erst 449 | In Betrieb |

## Was ausgeschlossen wurde und warum

**Nullvariante.** Ausgeschlossen: Die bezifferte Überlappung bliebe, der DoT bliebe
bei AoE unerreichbar, `GetCurrentMitigationPercent` bliebe Anzeige.

**Je Rotation einzeln umsetzen.** Ausgeschlossen: Verstößt gegen die
Defektklassen-Regel; dasselbe Muster altert je Datei getrennt.

**Zentrale Auslösung — ein gemeinsamer Trigger, der Mitigation anfordert.**
Ausgeschlossen am belegten Rückbau in diesem Fork: `HasHostileCountAoeMitigation`
setzte `AutoStatus.DefenseArea` und öffnete damit die gesamte Defensivkette statt der
einen gemeinten Zeile. Ausschlaggebend ist das Ausfallverhalten — eine Bremse, die
nicht greift, führt zum heutigen Verhalten zurück; ein Öffner, der fälschlich feuert,
löst die ganze Kette aus.

**Den DoT-Block vor den Sanctus-Block ziehen.** Ausgeschlossen, weil dann der
**erste** Stun einen GCD später fiele — eine Verzögerung der Schadensvermeidung
zugunsten von Schaden, also die Umkehrung des Vorrangs, der dieses Konzept trägt.
Zusätzlich wäre die Verschiebung der teuerste Teil im Upstream-Merge. Der
Sanctus-Block bleibt, wo er ist, und bekommt eine Aussetzbedingung; der vorhandene
DoT-Block darunter fängt den freigewordenen GCD auf. Damit begrenzt sich der
Merge-Aufwand auf eine Zeile.

**Sanctus in den Verteidigungszweig verschieben.** Ausgeschlossen aus demselben
Grund: durch die Aussetzbedingung überflüssig.

**Ein HP- und ein BMR-Notfallvorbehalt an der Aussetzbedingung.** Ausgeschlossen,
weil beide vor nichts schützen: Der Dispatcher ruft sämtliche Heil- und
Verteidigungszweige **vor** `GeneralGCD` auf, ein kritischer Gruppenzustand erreicht
diesen Code also gar nicht; und ein vorhergesagter Raidwide kommt vom Boss, nicht von
den betäubbaren Trash-Gegnern. Ein Vorbehalt, der nachweislich nichts abfängt, ist
toter Code.

## Falsifikation

**Es liegt kein Defekt vor.** Widerlegt durch die bezifferte Größenordnung und durch
die Existenz von `GetCurrentMitigationPercent` — die Entwurfsabsicht ist vorhanden,
die Verdrahtung fehlt. Für den DoT-Fall zusätzlich durch die beiden Vorkehrungen an
`ModifyDiaPvE`, die bei AoE nie zur Wirkung kommen.

**Die gewählte Option ist falsch.** Widerlegt für die Bremse, nicht für einen
Auslöser — siehe den belegten Rückbau oben. Für die Übertragung auf weitere Aktionen
hält der Einwand teilweise stand, weshalb sie von Beobachtung abhängig gemacht ist.

**Der Einschub ist im mittleren Gegnerzahlbereich falsch.** **Hält stand.** Bei etwa
fünf bis sieben Zielen ist weder der DoT-Wert noch der Betäubungswert groß, der
entgangene Sanctus aber schon spürbar. Die Regel ist deshalb nicht als „immer"
formuliert, sondern an `StretchHolyMinHostiles` gebunden; dieser Bereich ist der
schwächste Teil.

**Nicht widerlegt:** dass der Nutzen im Spiel eintritt. Die Rechnung ist ein Modell.

## Nachweisbarkeit

### Wirksamkeitsmessung im Spiel

Ein Nachweis, dass die Rotation besser spielt, ist **nicht** unmöglich: RSR sieht den
Ereignisstrom und kann sich selbst messen. `Watcher.ActionFromEnemy` wertet jeden
gegnerischen Treffer aus, summiert die Schadensanteile und legt sie über
`DataCenter.AddDamageRec` als `DamageRec(ReceiveTime, Ratio)` in eine Warteschlange
(`DataCenter.cs:1294`, `:1403-1411`). Der erlittene Schaden über die Zeit ist damit
bereits erfasst — gebraucht wird nur eine Auswertung je Kampf statt eines gleitenden
Fensters.

| Kennzahl | Quelle | Aussage |
|---|---|---|
| Erlittener Schadensanteil je Pull | `_damages`, summiert zwischen Kampfbeginn und -ende | Das Zielkriterium |
| Genutzte Betäubungsdauer | `StunCoverage` über die Zeit integriert | Ob die 7 s ausgeschöpft wurden |
| Überlappungsanteil | Anteil der Betäubungszeit mit bereits erhöhter `MitigationFraction` | Ob Posten 2 greift |
| DoT-Laufzeitanteil | Zeit mit aktivem DoT geteilt durch Kampfdauer | Ob der DoT-Grund wirkt |

**Versuchsanordnung.** Dieselbe Instanz, derselbe Pull, Option abwechselnd an und aus,
mehrere Durchläufe. Weil alle vier Kennzahlen innerhalb des Plugins anfallen, genügt
eine Anzeige im Einstellungsfenster; ein externes Werkzeug ist nicht nötig.

**Was die Messung nicht leistet.** Sie ist nicht kontrolliert: Gegnerzahl,
Tankverhalten und Gruppenzusammensetzung schwanken zwischen Durchläufen und überdecken
einen Effekt in der Größenordnung weniger Prozent leicht. Sie taugt daher, um eine
**Verschlechterung** zu erkennen und die Größenordnung einzugrenzen, nicht um einen
kleinen Gewinn zu beweisen.

### Übrige Ebenen

| Ebene | Möglich |
|---|---|
| Statisch | `stun_coverage.py` simuliert die Regel GCD für GCD und trägt seinen Selbsttest; für die Übertragung zusätzlich ein Skript, das Aktionen mit Mitigationswirkung im Schadenszweig findet |
| Kompilierung | CI |

Solange die Wirksamkeitsmessung nicht vorliegt, gilt die Feature-Toggle-Regel
unverändert: **jeder Schritt hinter eine eigene Option, Standard aus.** Die Messung
ist der Weg, diese Vorsichtsmaßnahme später begründet aufzuheben — sie gehört deshalb
vor die Übertragung auf weitere Aktionen.

## Konsequenzen

**Endnutzer.** Bei eingeschalteter Option fallen Mitigationswerkzeuge seltener, aber
gezielter; die frei werdenden GCDs gehen in Schaden, und die DoT-Uptime steigt.
Spürbar im großen Flächenpull, folgenlos im Einzelzielkampf — was durch die
Gegnerzahl-Schwelle so gewollt ist.

**Autoren abgeleiteter Rotationen.** Die Messung ergänzt die öffentliche Oberfläche
von `RotationSolver.Basic` additiv; keine Signatur bricht. Wer die neuen Prüfungen
nicht aufruft, merkt nichts.

**Upstream-Pflege.** Der Eingriff liegt in `CustomRotation_OtherInfo` und
`WHM_Reborn`, beides Dateien mit regelmäßiger Upstream-Aktivität. Neue Teile liegen in
eigenen Regionen; kein Block wandert.

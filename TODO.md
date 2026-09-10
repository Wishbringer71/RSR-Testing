# TODO — nur offene Arbeit

Getrennt nach Defekt (Abweichung vom beabsichtigten Verhalten), technischer Schuld (bewusst eingegangener Kompromiss mit Auflösungsbedingung) und offener Arbeit. Je Eintrag der betroffene Personenkreis: **N** Endnutzer des Plugins · **R** Autoren abgeleiteter Rotationen, die `RotationSolver.Basic` als Paket beziehen · **U** Upstream-Pflege.

## Defekte

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

### `HasSurvivingShield` misst die **kürzeste** Schildrestzeit, nicht die längste · N, R

`StatusHelper.cs:942`: `GetObjectShield() > 0 && !WillStatusEnd(horizon, false, ShieldStatus)`. `WillStatusEnd` stützt sich auf `StatusTime`, und das liefert das **Minimum** über alle vorhandenen gelisteten Status (`StatusHelper.cs:990-1011`). Beantwortet wird damit „laufen **alle** Barrieren noch?", während der Doku-Kommentar derselben Methode „has an active shield that will still be up" sagt — Singular.

**Wirkung:** Ein Ziel mit mehreren Barrieren — der Regelfall in einer Gruppe mit Heiler, etwa Galvanize plus Eukrasian Diagnosis plus eine Gruppenbarriere — gilt als ungeschützt, sobald die **kürzeste** unter den Horizont fällt, obwohl `GetObjectShield()` weiterhin einen Wert meldet. Der Fehler zeigt in dieselbe Richtung wie der behobene (A43): zu viel Heilung, nie zu wenig. Deshalb hat die Erweiterung der Liste ihn nicht verschlimmert, sondern nur häufiger sichtbar gemacht.

**Warum nicht sofort behoben:** Die naheliegende Umkehr auf das Maximum tauscht den Fehler nur aus. Sie würde ein Ziel als geschützt werten, dessen große Barriere gerade ausläuft, solange irgendeine kleine bleibt — und `GetObjectShield()` liefert nur den Gesamtwert, nicht die Aufteilung je Status. Eine Unterschätzung kostet eine überflüssige Heilung, eine Überschätzung kostet einen Tod.

**Auflösungsbedingung:** Laufzeitbeobachtung, welche der beiden Abweichungen tatsächlich auftritt, oder eine Quelle für die Aufteilung des Schildwerts auf die einzelnen Status. **Empfehlung bis dahin: belassen** — die heutige Richtung ist die sichere.

### Statuslisten und Einzelprüfungen ohne die Geschwister-Ids ihrer Wirkung · N, R

Das Spiel führt jede Wirkung unter mehreren Status-Ids desselben Anzeigenamens — eine je Fassung der Fähigkeit, dazu PvP-Formen und die Fassungen, in die eine Trait aufwertet. Eine handgepflegte Aufzählung, die eine Id nennt und die Geschwister auslässt, antwortet für den Träger der ausgelassenen Id **falsch**, nicht nur ungenau. `scan14.py` erhebt die Klasse über alle 24 Listen in `StatusHelper.cs` (A49): **186 fehlende Geschwister in 16 Listen** nach der Behebung von `RampartStatus` und `ReprisalStatus`.

**Was davon kein Defekt ist:** 114 Treffer stammen aus `PhantomDispellable` und `PurifyPvPStatuses`, die bewusst Teilmengen sind. Weitere Treffer sind andere Wirkungen unter geteiltem Namen — `Nebula_3051` (Reflexion), `Bloodwhetting_3030` (Lebensraub), `Holmgang` 88 und 1305 auf dem Ziel statt dem Träger (C15). Der Scan druckt zu jedem Kandidaten die Wirkbeschreibung und die Marke `same opening`/`differs`; entscheiden muss ein Leser.

**Zwei benannte Fundstellen, an denen die Zuordnung offen ist:**

- `TankStanceStatus` führt `IronWill` (79) und `RoyalGuard_1833`, nicht aber `IronWill_393`, `IronWill_2843` und `RoyalGuard` (392) — gleicher Anzeigename, gleicher Wirktext „Enmity is increased." Die Liste entscheidet, wen die Zielwahl für einen Tank hält (`ActionTargetInfo.cs`, sieben Stellen, darunter `FindTankTarget` und `FindKardia`) und ob `HasTankStance` für den Spieler greift. `Defiance_1396` und `Grit_1397` („Damage dealt and taken are reduced.") sind die Fassungen vor Shadowbringers und gehören **nicht** hinein.
- `GetCurrentMitigationPercent` (`CustomRotation_OtherInfo.cs:627`, `:633`) liest `StatusID.Addle` und `StatusID.Feint` als **einzelne** Id. `Addle_1988` (Geltungsbereich BLM SMN RDM BLU PCT, keine PvP-Aktion in `ActionId.resx`) und `Feint_2185` existieren; die Minderungsbilanz zählt eine vorhandene Schwächung dann als nicht vorhanden. Einzelprüfungen dieser Art sieht `scan14.py` nicht — er erhebt nur Ids in Listen.

**Warum nicht behoben:** Welche Id das Spiel je Stufe tatsächlich setzt, ist aus den Daten nicht zu entscheiden. Bei `Reprisal_2101` trug der Beleg — genau eine PvE-Aktion, Geltungsbereich auf die vier Jobs verengt, belegter Trait-Stufenwert; bei den Tankhaltungen tut er das nicht, und eine falsch aufgenommene Id kehrt die Antwort in die andere Richtung um. **Auflösungsbedingung:** Laufzeitbeobachtung, welche Id ein Tank beziehungsweise ein Zauberer im Ziel trägt, oder eine Quelle für die Id-Zuordnung je Stufe.

**Empfehlung: erfassen, nicht bearbeiten.** Behoben ist, was eine Wirkkette im Code liest und wo der Beleg trägt. Der Rest ist eine Klasse ohne Schranke: Ein Rückgabewert, den nur Wegsehen grün hält, wäre schlechter als keiner, und `scan14.py` hält die Liste jederzeit wieder abrufbar.

## Technische Schuld

### `CanEarlyWeave` steht auf dem beobachteten statt auf dem geschriebenen Verhalten · N, R

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

Geprüfte Nicht-Fehlstellen: `DTRManualAuto` bildet den vom Enum-Text beschriebenen Zwei-Zustands-Zyklus ab (kein Fehler, AUDIT_LOG A14); ein zu großer `TargetingIndex` kann keinen Indexfehler auslösen, `DataCenter.TargetingType` rechnet `% Count` (`DataCenter.cs:284-302`).

### Selbstlernende AoE-Liste wächst ohne fachliche Schranke · N

`Watcher.ActionFromEnemy:111-148` nimmt eine Gegner-Aktion dauerhaft in `HostileCastingArea` auf, wenn die Party mindestens vier Mitglieder hat, die Aktion eine Wirkzeit besitzt, zur Kategorie Spell/Weaponskill/Ability gehört und **jedes** Party-Mitglied im selben Effektsatz Schaden genommen hat. „Record AOE actions" ist standardmäßig an.

**Korrektur einer früheren Aussage:** Der Eintrag behauptete, ein gelernter Eintrag lasse sich nur durch Editieren der Datei zurücknehmen. Das ist zweifach widerlegt — `RotationConfigWindow.cs:3745` bietet „Reset and Update AOE List" (`ResetHostileCastingArea`, lädt die gepflegte Liste neu), und `DrawActionsList` erlaubt das Entfernen einzelner Einträge über Kontextmenü und Entf-Taste. Die Codedokumentation empfiehlt den Reset ausdrücklich nach jedem Patch. Damit ist dies kein Defekt, sondern eine Automatik mit vorhandenen Korrekturwerkzeugen.

**Kosten:** Zwischen einer falsch gelernten Aktion und der nächsten Nutzerkorrektur mitigiert RSR auf einen ausweichbaren Effekt. Durch die Reichweitenprüfung in `IsHostileCastingArea` entschärft.

**Auflösungsbedingung:** ob sich echte Raidwides beim Lernen von ausweichbaren Flächen unterscheiden lassen (Kandidat: `CastType`/`EffectRange`), ist ohne Spieldaten nicht entscheidbar. Eine Verschärfung wäre eine Verhaltensänderung ohne Nachweismöglichkeit und gehörte deshalb hinter eine eigene Option, nicht in den Standardpfad. Geprüfte Nicht-Fehlstelle: das Speichern läuft asynchron, kein blockierendes Schreiben im Kampfpfad.

#### Geprüfter Vorschlag: Schadenshöhe als Aufnahmekriterium

Vorschlag des Auftraggebers: Liegt der Schaden unterhalb dessen, was der Schild ohnehin auffängt, braucht die Aktion nicht in die Liste — der Schild wäre verschwendet. Auftrag war, das zu widerlegen oder modifiziert aufzunehmen. Ergebnis: **der Kerngedanke trifft eine echte Lücke, die vorgeschlagene Form trägt nicht, eine modifizierte Form ist umsetzbar.**

**Die Lücke ist real.** `HostileCastingArea` ist ein `HashSet<uint>` (`OtherConfiguration.cs:28`) — eine Aktion ist drin oder nicht. Eine Fläche, die zwei Prozent der Gesundheit nimmt, steht gleichberechtigt neben einer, die sechzig nimmt, und löst über `IsHostileCastingAOE` → `ShouldAddDefenseArea` (`StateUpdater.cs:178`) dieselbe Gruppenmitigation aus. Die Kostenseite ist ebenfalls real, nur anders benannt als im Vorschlag: Was verbraucht wird, ist nicht „der Schild", sondern der **Cooldown** — eine auf eine Bagatelle gelegte Reprisal fehlt beim nächsten großen Einschlag.

**Die Datenquelle liegt bereits an der richtigen Stelle.** `Watcher.cs:137` liest beim Lernen `damageEffect.value`, prüft davon aber nur `> 0`. Der Betrag ist da und wird verworfen.

**Zeitpunkt der Aufnahme — geprüft, und er ist bereits der richtige.** Gelernt wird **nach dem Effekt**, nicht bei Cast-Beginn: `Watcher.Enable` (`Watcher.cs:17`) hängt `ActionFromEnemy` an `ActionEffect.ActionEffectEvent`, das Effektpaket-Ereignis. Dass dort `set.TargetEffects` mit Schadenswerten ausgewertet wird, ist der Beleg — Schadenswerte entstehen erst beim Treffer. Die Bedingung `set.Action?.Cast100ms > 0` prüft nur, ob die Aktion überhaupt eine Wirkzeit *besitzt*, nicht ob gerade gewirkt wird.

Die Größenordnung steht beim Lernen also fest. **Verwendet** wird die Liste dagegen bei Cast-Beginn: `IsHostileCastingArea` (`DataCenter.cs:2445`) prüft laufende Casts gegen die gelernten Ids. Aus beidem zusammen ergibt sich die eigentliche Konstruktion — beim **ersten** Vorkommen einer Aktion wird nie mitigiert, ab dem zweiten schon. Das ist stimmig und nicht zu ändern.

**Vier Einwände gegen die einfache Form (Filter beim Lernen):**

1. **Zirkelschluss.** Gemessen wird der Schaden *nach* der damals wirkenden Mitigation. Hat die Gruppe beim ersten Vorkommen gut mitigiert, fällt der Wert klein aus, die Aktion wird ausgeschlossen — und künftig wird nicht mehr mitigiert, wodurch der Schaden groß wird. Die Regel würde ihre eigene Voraussetzung zerstören. *(Weitgehend behoben durch die Rückrechnung, siehe unten; ein Restfehler bleibt.)*
2. **Alterung.** Ein absoluter Betrag oder ein an einem Schildwert gemessener Schwellwert veraltet mit Item-Level, Content-Sync und Vulnerability-Stapeln. Das ist dasselbe *Lack of Movement*-Muster wie bei den Statusaufzählungen: heute richtig, nach der nächsten Erweiterung still falsch.
3. **„Der Schild" ist der falsche Maßstab.** Was `DefenseArea` auslöst, ist überwiegend prozentuale Schadensminderung (Reprisal, Addle, Feint, Kerachole), nicht Absorption. Prozentuale Minderung wird nicht „verschwendet" — sie skaliert mit dem Schaden, ihr Nutzen ist bei kleinem Schaden nur klein. Und welcher Schild gemeint wäre, hängt an Job und Level; ein gruppenweiter Schwellwert daraus ist nicht ableitbar.
4. **Serien.** Mehrere kleine Einschläge kurz hintereinander summieren sich. Jeder einzeln unter der Schwelle, zusammen tödlich — eine Einzelwertprüfung sieht das nicht.

**Modifizierte Form, die die vier Einwände umgeht:** nicht beim Lernen filtern, sondern die Größenordnung **mitspeichern** und erst beim Verbrauch entscheiden.

- `HashSet<uint>` → Zuordnung Aktion auf **höchsten je beobachteten Schadensanteil**, gemessen als `value / MaxHp` des getroffenen Mitglieds und **um die zum Trefferzeitpunkt wirkende Minderung zurückgerechnet** (siehe unten).

##### Warum Anteil an der Maximalgesundheit und nicht Potenz

Der Auftraggeber schlägt statt eines absoluten Werts die **Potenz** vor, analog zu Heilzaubern und Schilden. Das Ziel dahinter — eine Größe, die nicht mit Level und Item-Level altert — ist richtig und wird von der hier gewählten Form erreicht; die Wahl fällt trotzdem auf den Anteil, aus vier Gründen. Der Vorschlag hat dabei einen echten Vorteil, der zuerst zu nennen ist:

**Was für die Potenz spricht:** Sie ist **mitigationsfrei**. Der beobachtete Schaden enthält immer die zufällig gerade wirkende Mitigation — genau der Zirkelschluss aus Einwand 1. Wäre die Potenz ablesbar, entfiele er vollständig. Das ist ein sachlicher Vorteil und kein Nebenpunkt.

**Was dagegen spricht:**

1. **Verfügbarkeit ist nicht belegt.** Im gesamten Baum wird keine Potenz gelesen — weder `Potency` noch ein `Power`-Feld kommt in einer `.cs`-Datei vor. In `ActionId.resx` erscheint „Potency" 465-mal, aber ausschließlich als **Freitext** innerhalb der Beschreibung von Spieleraktionen, nicht als auswertbares Feld. Ob das Datenblatt des Spiels für **Gegner**-Aktionen eine numerische Potenz führt, ist aus diesem Repository nicht zu klären und hier als unbelegt zu führen. Der beobachtete Schadensbetrag ist demgegenüber nachweislich vorhanden.

2. **Potenz altert ebenfalls, nur an einer anderen Achse.** Sie ist gegenüber der Ausrüstung der Gruppe invariant, aber nicht gegenüber dem Inhalt: Dieselbe Potenz ist in Level-50-Inhalten tödlich und in Level-100-Inhalten belanglos. Der Anteil an der Maximalgesundheit ist gegenüber **beiden** Achsen stabil, weil er Schaden und Gesundheit gemeinsam skaliert.

3. **Potenz ist keine Gefahrenaussage, sondern eine Eingangsgröße.** Sie ist der erste Faktor einer Formel, deren übrige Faktoren — Gegnerstufe, Inhaltssynchronisation, Verwundbarkeitsstapel, Maximalgesundheit der Gruppe — erst bestimmen, was den Heiler interessiert. Der Anteil misst das Ergebnis unmittelbar.

4. **Der Vergleichspartner liegt bereits in dieser Einheit vor.** Der ursprüngliche Gedanke war, Schadensgröße gegen Schildkapazität zu halten. Das Spiel selbst führt Schilde als **Prozent der maximalen Gesundheit**: `ICharacter.ShieldPercentage`, in `ObjectHelper.GetObjectShield` (`ObjectHelper.cs:3372`) als `MaxHp * ShieldPercentage / 100` gelesen. Beide Seiten des Vergleichs stehen damit in derselben Einheit; die Potenz wäre eine dritte, in die erst umzurechnen wäre.

##### Rückrechnung statt Vorhersage — Korrektur einer zu absoluten Aussage

Eine frühere Fassung dieses Abschnitts schloss: „Eine Formel braucht nicht, wer das Ergebnis hat." **Das war zu absolut.** Der Auftraggeber weist zu Recht darauf hin, dass sich der Rechenweg umkehren lässt: Wer den absoluten Wert beobachtet hat, kann die Störfaktoren herausrechnen, statt den Wert hinzunehmen. Der Gedanke ist richtig und führt zu einer besseren Größe als der bloß beobachtete Anteil.

**Und das Werkzeug dafür ist bereits vorhanden.** `CustomRotation_OtherInfo.GetCurrentMitigationPercent()` (`:585`) rechnet die wirkenden Minderungen multiplikativ zu einem Schadensfaktor zusammen — Addle, Feint, Dismantle, Reprisal und die gruppenweiten Minderungsstatus, mit getrennter Skalierung nach physisch und magisch. Aufgerufen im Effekt-Handler, also **zum Trefferzeitpunkt**, liefert sie genau den Divisor:

```
Rohanteil = (damageEffect.value / MaxHp) / Schadensfaktor
```

Damit entfällt der Zirkelschluss aus Einwand 1 weitgehend, und die Höchstwertregel sinkt von der Notlösung zur Absicherung gegen den Restfehler.

**Die Rückrechnung endet allerdings einen Schritt vor der Potenz.** Bereinigen lässt sich um das, was der Client kennt: die gruppenweite Minderung, die Verwundbarkeitsstapel des Getroffenen, dessen persönliche Minderungsstatus. Nicht bekannt sind die **Angriffswerte des Gegners** und die Verteidigungswerte des Getroffenen. Ohne sie kommt man von „ungemindertem Schaden an Spieler X" nicht zur „Potenz der Aktion" — die Formel für Gegnerschaden enthält Größen, die außerhalb des Clients liegen.

**Gebraucht wird die Potenz dann aber auch nicht.** Das Ziel — eine minderungsfreie, nicht alternde Größe — ist beim ungeminderten Anteil erreicht. Der letzte Schritt zur Potenz würde die Größe zusätzlich von der Gruppe lösen und damit gerade die Information entfernen, auf die es ankommt: wie gefährlich die Aktion für *diese* Gruppe in *diesem* Inhalt ist.

**Grenzen der Rückrechnung, die nicht zu übergehen sind:**

1. `GetCurrentMitigationPercent` ist eine **Aufzählung** bekannter Status. Sie altert nach demselben *Lack of Movement*-Muster wie die Statuslisten und ist damit im Zweifel unvollständig — eine unvollständige Bereinigung unterschätzt den Rohwert.
2. Sie ist **parameterlos und gruppenbezogen**, nicht auf ein einzelnes Ziel bezogen. Für Raidwides — den Gegenstand dieser Liste — ist das die passende Bezugsgröße; die persönliche Minderung eines einzelnen Getroffenen verzerrt dessen Einzelwert und ist getrennt zu berücksichtigen.
3. Verwundbarkeitsstapel wirken in die **Gegenrichtung** und sind dort nicht enthalten.
4. Der Deckel bei 0,95 verfälscht die Rückrechnung bei extremer Stapelung.

Der Restfehler rechtfertigt die Höchstwertregel weiterhin, aber als Absicherung, nicht als Ersatz für die Bereinigung.
- *Anteil statt Betrag* entschärft Einwand 2 — ein Anteil altert nicht mit dem Item-Level.
- *Höchstwert statt letztem Wert* entschärft Einwand 1 — eine einzige ungemitigierte Beobachtung setzt den wahren Wert, und spätere gut mitigierte Vorkommen senken ihn nicht wieder.
- *Entscheidung beim Verbrauch* entschärft Einwand 3 und 4 — die Schwelle ist eine Nutzeroption, keine feste Zahl, und sie kann später um eine Serienbetrachtung ergänzt werden, ohne die gelernten Daten neu zu erheben.
- **Standard 0**, also unverändertes Verhalten, bis der Nutzer eine Schwelle setzt. Die Wirkung ist statisch nicht belegbar, gehört also nach der Projektregel hinter eine Option.

**Kosten dieser Form, die nicht zu verschweigen sind:** `HostileCastingArea` ist gespeicherte Nutzerkonfiguration und damit ein Persistenzvertrag. Ein Typwechsel von `HashSet<uint>` auf eine Zuordnung bricht die vorhandene Datei; nötig wäre ein Migrationspfad, der bestehende Einträge mit unbekanntem Anteil übernimmt und sie bis zur ersten Neubeobachtung wie heute behandelt. Zudem ist der gemessene Anteil je Gruppenmitglied verschieden (verschiedene Maximalgesundheit, verschiedene Mitigation) — festzulegen wäre, ob der höchste oder der mittlere Anteil des Effektsatzes zählt.

##### Entscheidung nach eigenem Loop: konzipiert, nicht gebaut

Geprüft mit demselben Maßstab, der den Messbaustein verworfen hat. Ergebnis: **Nullvariante**, und die Begründung ist die Kostenseite, nicht der Gedanke.

Was die Umsetzung verlangt, vollständig erhoben:

| Kostenpunkt | Umfang |
|---|---|
| Persistenzvertrag | `HostileCastingArea` ist ein `HashSet<uint>` in gespeicherter Nutzerkonfiguration. Ein Typwechsel bricht die Datei; ein Parallelspeicher vermeidet das, verdoppelt aber die Ablage |
| **UI-Kopplung, erst hier gefunden** | `RotationConfigWindow.DrawActionsList(string, HashSet<uint>)` bedient **vier** Listen mit einer Signatur — `HostileCastingTank`, `HostileCastingArea`, `HostileCastingKnockback`, `HostileCastingStop`. Der Typwechsel einer davon erzwingt eine Überladung oder den Umbau aller vier |
| Rückrechnung | `GetCurrentMitigationPercent` ist eine Aufzählung bekannter Status und damit unvollständig; die Bereinigung bleibt eine Näherung |
| Die Schwelle selbst | Ohne Spielbeobachtung ist nicht belegbar, welcher Anteil „zu klein für eine Mitigation" ist. Standard 0 bedeutet: keine Wirkung, bis jemand rät |

Dem steht als Ertrag eine Nutzeroption gegenüber, deren Wirkung unbelegt ist und die im Auslieferungszustand nichts tut. **Das Verhältnis trägt nicht.**

**Kehrtwende gegenüber der früheren Bewertung, offen benannt:** Der Vorschlag wurde zuvor als „modifiziert aufnehmbar" eingestuft, und das bleibt er inhaltlich — die vier Einwände sind entkräftet, die Konstruktion oben ist tragfähig. Was fehlte, war die Erhebung der Umsetzungskosten; die UI-Kopplung über vier Listen war nicht gesehen. Ein Vorschlag kann konzeptionell richtig und trotzdem nicht umsetzungswert sein.

**Auflösungsbedingung:** aufzugreifen, sobald eine der drei Voraussetzungen entfällt — eine Spielbeobachtung, die eine Schwelle belegt; ein ohnehin anstehender Umbau von `DrawActionsList`; oder ein zweiter Verbraucher für gespeicherte Schadensanteile, der die Ablage für sich rechtfertigt.

**Bewertung:** technische Schuld, kein Defekt — die heutige Grobheit ist eine bewusste Vereinfachung, keine Fehlfunktion. Die Auflösung ist an dieselbe Bedingung gebunden wie der Rest dieses Eintrags: Ohne Spielbeobachtung ist nicht belegbar, dass die Schwelle mehr nützt als schadet.

### `SpreadDamagePaths` enthält keinen Spread-Marker · N

`DataCenter.cs:2036-2043`. Zwei der vier Pfade stehen wortgleich in `SharedDamagePaths` (2025-2026), die anderen beiden sind laut eigenem Kommentar „AOE share markers", also ebenfalls Stack-Marker. Ohne Fehlwirkung, weil `IsCastingAreaVfx` alle drei Listen prüft. **Kosten:** eine Kategorie, die etwas anderes verspricht, als sie enthält. Nebenbefund: `SharedDamagePaths` führt `vfx/lockon/eff/com_trg01_0c` zweimal (2022 und 2024), im `FrozenSet` folgenlos.

**Entstehung belegt** (A41): eingeführt in `33e6acb1` vom 07.05.2026, einem Sammel-Refactoring („Refactor for var usage, safety checks, and plugin compat"), und zwar bereits mit den beiden Duplikaten und dem übernommenen Kommentar. Das ist *Ignorant Surgery* nach Parnas — Klon einer Nachbarliste ohne Anpassung —, kein späteres Veralten. Die Historie war dafür zu vertiefen; im flachen Klon lag der Einführungs-Commit vor dem Anfang.

**Empfehlung: Nullvariante, Punkt offen führen.** Für RSR ist der Unterschied ohne Verbraucher — Spread wie Stack lösen dieselbe Gruppenmitigation aus, eine Positionierung leitet das Plugin nicht. Die Kategorie zu füllen setzt Spieldaten voraus, die hier fehlen: Eine Websuche nach den beiden einzigartigen Pfaden (`x6fd_loc04m_5s1v`, `m0922tar_a0w`) bleibt ohne Treffer, ihre Zuordnung ist damit unbelegt. Die Liste zu streichen ist nur dann verhaltensneutral, wenn genau diese beiden nach `SharedDamagePaths` übernommen werden — und tilgt dann das Signal, das den offenen Punkt sichtbar hält. Wer dennoch aufräumen will: die kleinste verhaltensneutrale Fassung ist das Entfernen der beiden Duplikate `x6r9_loc01_t0a1` und `x6r9_loc02_t0a1` aus dieser Liste.

### Release-Paket enthält vermeidbaren Ballast · N, R

**Am Artefakt belegt** (`latest.zip` von 7.5.5.41+wsh1, 5,35 MB): Nutzlast sind `RotationSolver.dll`, `RotationSolver.Basic.dll`, `ECommons.dll` und `RotationSolver.json`; dazu kommen `RotationSolver.Basic.xml` (7,52 MB), der Analyzer samt Symbolen (5,59 MB), das NuGet-Paket (1,54 MB) und `RotationSolver.Basic.pdb` (1,27 MB).

**Kein Fork-Defekt, sondern Upstream-Verhalten** (A40): Die Upstream-Releases 7.5.5.41 und 7.5.6.0 enthalten dieselben zwölf Dateien in denselben Rollen, 5,34 MB gegenüber 5,35 MB. Der Fork weicht im Dateibestand nicht ab; auch der Abhängigkeitsgraph beider `deps.json` ist deckungsgleich. Eine Bereinigung wäre damit eine eigene Abweichung im Verpackungspfad.

**Ursache** ist weder die Prune-Regel — `PruneOutputDlls` arbeitet auf `ReferenceCopyLocalPaths` und erfasst nichts davon — noch das `OutputPath` der Projektdatei: `publish.yaml:42` baut mit `--output .\build`, wodurch die Ausgaben aller beteiligten Projekte in einem Verzeichnis landen, das DalamudPackager packt. `GeneratePackageOnBuild` bedient dabei bewusst die Autoren abgeleiteter Rotationen.

**Kosten:** 5,35 MB Download statt rund 1,8 MB, funktional folgenlos.

**Auflösungsbedingung:** Der naheliegende Weg trägt nicht — der Standard-Target von DalamudPackager reicht `Exclude` nicht durch und läuft nur, solange keine eigene `DalamudPackager.targets` im Projektverzeichnis liegt; diese müsste den vollständigen Task-Aufruf samt aller Manifest-Felder nachbauen. `Exclude` vergleicht exakt über `List.Contains` (`DalamudPackager.cs:187`), kennt also keine Muster, und das NuGet-Paket trägt die Version im Dateinamen. Der Build-Workflow kompiliert nur und prüft das Paket nicht. Aufgreifen erst, wenn der Veröffentlichungspfad prüfbar ist. Geprüft: die XML-Dokumentation wird zur Laufzeit nicht gelesen.

**Empfehlung: nicht aufgreifen.** Der Auftraggeber hat die Spielbarkeit des Release-ZIPs zur Anforderung gemacht. Ein Eingriff in den Verpackungspfad ist genau die Klasse von Änderung, deren Ergebnis erst am fertigen Release sichtbar wird — und dieser Pfad läuft nur auf einen Tag, wird von keiner Prüfung abgedeckt und weicht dann zusätzlich vom Upstream ab. 3,5 MB Download stehen gegen das Risiko, ein nicht ladbares Paket zu bauen.

### DRK: Die Zeitpunktwahl für The Blackest Night steht auf dem alten Verhalten · N, U

`BlackestNightUsage` (`DRK_Reborn.cs:35`) bietet drei Stufen für den Selbstschutz-Zweig: wie bisher; Tankbuster oder großer Pull ohne laufende große Minderung; dasselbe plus Gesundheitsschwelle. Voreingestellt ist **die erste**, also das Verhalten, das der Auftraggeber beanstandet hat (A44/A45, Konzept `docs/rotation-flow/10-drk-blackest-night.md`).

**Kosten des Kompromisses:** Wer nichts umstellt, behält die Fehlausgabe — 3000 MP für eine Barriere, die bei mäßigem Schaden nicht aufgezehrt wird und deshalb kein Dark Arts auslöst. Für den Auftraggeber ist das mit einem Klick erledigt; für andere Nutzer des Forks bleibt es der Standard.

**Warum trotzdem so:** Die Beobachtung liegt für ein Nutzungsprofil vor, nicht allgemein. Zwei Größen der engeren Stufen sind zudem Annahmen: dass vier Gegner die Verbrauchsrate von 3,6 % der maximalen Gesundheit pro Sekunde erreichen, und dass ein Tankbuster die unter Minderung angehobene Schwelle mitnimmt.

**Auflösungsbedingung:** eine Spielbeobachtung über mehrere Kämpfe — wird die Barriere unter `TankbusterOrHeavyPull` regelmäßig aufgezehrt, und fehlt sie nie dort, wo sie gebraucht wurde, ist der Standard umzustellen. Zu beobachten ist dabei auch die Gegnerschwelle: Kommt The Blackest Night im Wall-to-Wall zu selten, ist `BlackestNightMinHostiles` (Vorgabe 4) zu hoch angesetzt. Sie ist bewusst eine **eigene** Option der Rotation und nicht der globale `MitigationSustainHostileCount`, weil die Frage hier eine andere ist: nicht „lohnt eine Minderung", sondern „reicht der Schadensstrom, um 25 % der maximalen Gesundheit in sieben Sekunden aufzuzehren".

### PR-Prüfung folgt dem Staging-Kanal von Dalamud · U

`publish.yaml` bezieht Dalamud seit dem Upstream-Release 7.5.6.0 aus `dalamud-distrib/stg/latest.zip` — das ist die **einzige** Änderung, die dieses Release gegenüber 7.5.5.41 trägt. `build.yaml` ist dieser Zeile nachgezogen (A40), damit die PR-Prüfung gegen dieselben Assemblies kompiliert, gegen die das Release gebaut wird. Upstream hat nur `publish.yaml` umgestellt; die Zeile in `build.yaml` ist deshalb eine Fork-Abweichung.

**Kosten:** eine Zeile Merge-Fläche. Zudem hängt die PR-Prüfung nun an einem Kanal, der nach der Dalamud-Dokumentation häufiger bricht als der Release-Kanal — ein Bruch dort blockiert die Prüfung, hätte aber ebenso den Release-Build blockiert, nur später.

**Auflösungsbedingung:** Stellt Upstream `publish.yaml` auf den Release-Kanal zurück, ist `build.yaml` mitzuziehen. Beide Zeilen sind bewusst gleich zu halten; laufen sie auseinander, misst die Prüfung nicht mehr, was ausgeliefert wird.

### VPR: leerer Zweig einer Struktur, die anderswo eine Entscheidung trägt · U

`VPR_Reborn.cs:591-597` und `975-981`. Das Muster `!HasHunterAndSwift` kommt viermal vor; der Vorspann `!IsHunter && !IsSwift` trägt nur an der Coil-Stelle (751-807) Inhalt, an der Den-Stelle (424-493) fehlt er ganz. Weder ein fehlender Inhalt noch dessen Entbehrlichkeit ist belegbar. **Bewusst nicht gelöscht** (AUDIT_LOG A11): Die Entfernung wäre verhaltensneutral, würde aber die Asymmetrie verdecken, die den Befund sichtbar macht. **Auflösung:** Adressat ist der Upstream. **Empfehlung: belassen** — der leere Zweig ist der einzige Hinweis auf die Lücke, und ohne den Autor ist nicht zu entscheiden, ob Inhalt fehlt oder der Vorspann überflüssig ist.

### `AutodutyUpdateState` dupliziert `UpdateState` · U

`RSCommands_StateSpecialCommand.cs`: rund 100 wortgleiche Zeilen, abweichend nur die Fälle `TargetOnly` und `AutoDuty` (`TargetingTypeOverride = targetingType` statt `null`) und der Zustandstext. **Kosten:** jede künftige Änderung am Zustandsautomaten muss an zwei Stellen erfolgen. **Auflösung:** über einen optionalen `TargetingType?`-Parameter zusammenführen, sobald an dieser Stelle ohnehin gearbeitet wird. **Empfehlung: nicht eigens angehen** — die Duplizierung kostet erst bei der nächsten Änderung am Zustandsautomaten etwas, und genau die ist nach dem Punkt oben ohne Laufzeitbeobachtung nicht zu empfehlen. Beide Punkte lösen sich gemeinsam oder gar nicht.

### Zwei entfernte öffentliche Member seit dem letzten Release · R

`scan7.py` misst die Paketoberfläche von `RotationSolver.Basic` gegen den Tag `7.5.5.41+wsh1`: `CustomRotation_BasicInfo.HasHostileCountAoeMitigation` (`public virtual`) und `ActionConfig.ShouldCheckTargetStatus` sind seither ersatzlos entfallen. Beide Entfernungen sind sachlich belegt — das Flag öffnete die gesamte Defensivkette, die Option las niemand —, aber sie waren im ausgelieferten Paket enthalten.

**Kosten:** Eine abgeleitete Rotation, die das Flag überschreibt oder die Option liest, kompiliert gegen die nächste Paketversion nicht mehr. Für die gespeicherte Nutzerkonfiguration folgenlos, weil Dalamud fehlende Member beim Deserialisieren überspringt.

**Auflösungsbedingung:** Der Ausweis erfolgt in `CHANGELOG.md`, weil die Versionsnummer ihn nicht tragen kann: ihr numerischer Teil folgt dem Upstream-Release, nicht der Kompatibilität dieses Forks, und das Prerelease-Label `-wsh1` kennzeichnet nur die Herkunft (A38). Offen bleibt allein, den Abschnitt „Unreleased" bei der nächsten Versionsvergabe auf die dann vergebene Version zu setzen. Der Zeitpunkt der Veröffentlichung liegt beim Auftraggeber.

**Empfehlung:** mit dem nächsten Tag erledigen, nicht davor — der Abschnitt kann erst auf eine Nummer gesetzt werden, die vergeben ist. Nach dem Upstream-Merge (A40) wäre das `7.5.6.0+wsh1`.

### Plugin-Identität ist unverändert die des Upstreams · N

`manifest.json` ist gegenüber `upstream/main` unverändert: `InternalName: RotationSolver`, `RepoUrl` und `IconUrl` verweisen auf FFXIV-CombatReborn, `AcceptsFeedback` ist gesetzt. Ein Fork-Build meldet sich damit unter derselben Plugin-Identität an wie das Upstream-Plugin. Bisher mitgeführt, nicht als Entscheidung erfasst; aufgefallen beim Abgleich der README gegen den Ist-Stand (A39).

**Kosten — Inferenz, nicht am Artefakt belegt:** Dalamud adressiert Plugin-Verzeichnis, Konfigurationsablage und Aktualisierung über den `InternalName`. Daraus folgte, dass Fork- und Upstream-Installation einander verdrängen, dieselbe gespeicherte Konfiguration benutzen und ein Feedback-Weg beim Upstream endet. Belegt ist allein der Manifest-Inhalt; das Installationsverhalten ist in dieser Umgebung nicht prüfbar.

**Gegenposition:** Die gemeinsame Identität ist zugleich der Nutzen — die Nutzerkonfiguration überdauert einen Wechsel zwischen Upstream und Fork, und die Abweichung zum Upstream bleibt null (Betroffenengruppe U).

**Auflösungsbedingung:** Zu entscheiden erst, wenn beide Installationen nebeneinander gebraucht werden. Ein eigener `InternalName` trennt dann auch die gespeicherte Konfiguration; ohne Migrationspfad verlöre der Nutzer seine Einstellungen. Die Freigabe liegt beim Auftraggeber.

**Empfehlung: belassen.** Die geteilte Identität kostet nur, wenn beide Fassungen gleichzeitig installiert sein sollen; sie nützt bei jedem Wechsel zwischen ihnen, weil die Nutzerkonfiguration erhalten bleibt.

### Übertragung der Mitigations-Synergie auf weitere Doppelnutzen-Aktionen · N

Schritt 3 aus `docs/rotation-flow/08-mitigation-synergy.md`. Die Schritte 1 und 2 — Messung und Aussetzbedingung für Sanctus — sind umgesetzt (AUDIT_LOG A20). Offen ist die Übertragung auf weitere Aktionen, die Schaden erzeugen und zugleich Schaden vermeiden.

**Zur Führung dieses Punktes:** Die Einzelheiten stehen im Konzeptdokument, nicht hier. `TODO.md` führt die offene Arbeit und verweist; eine zweite Beschreibung derselben Sache würde mit der ersten auseinanderlaufen. Was hier stehen muss, ist allein, dass noch etwas offen ist und wo es beschrieben wird.

**Auflösungsbedingung:** erst nach Beobachtung der Schritte 1 und 2 im Spiel. Kandidatensuche über ein Prüfskript, nicht über Erinnerung.

**Empfehlung: warten.** Schritt 3 überträgt eine Regel, deren Nutzen in den Schritten 1 und 2 noch nicht beobachtet ist; eine Übertragung vor dem Nachweis vervielfacht einen möglichen Fehler, statt einen Nutzen zu vervielfachen.

### Keine Messgrundlage für Schadens- und Heilungsraten auf Gruppenmitglieder · —

**Geprüft und bewusst nicht gebaut.** Der Befund selbst besteht fort: Für Gruppenmitglieder gibt es keine Rate. `DataCenter.RecordedHP` (`DataCenter.cs:197`) wird in `TargetUpdater.cs:513-535` ausschließlich aus `AllHostileTargets` gefüllt, weshalb `GetTTK` für eine Party-Id `NaN` liefert; die Abtastrate ist 1 Hz (`TargetUpdater.cs:19`); und ein Heilpaket eines fremden Heilers passiert beide Watcher-Filter (`Watcher.cs:17-18`) ungelesen. Es fehlt der Aufnehmer, nicht die Quelle.

**Warum er trotzdem nicht gebaut wird:** Er hatte genau einen vorgesehenen Verbraucher — die Hochrechnung, ob der Tod eines Dunkelritters noch vor Ablauf von Living Dead eintritt. Diese Frage ist inzwischen anders beantwortet: `StatusHelper.InDeathTriggerWindow` misst die Restzeit des Status selbst und gibt den Halt einen GCD vor Ablauf frei. Damit gibt es im gesamten Baum keinen Verbraucher mehr, und ein Baustein ohne Verbraucher ist Vorratsarbeit.

Die Kostenseite bliebe dagegen bestehen: Ein Ringpuffer über alle Gruppenmitglieder und ein dritter Effekt-Handler laufen in jedem Kampf für jeden Nutzer, auch für die, die nie einen Dunkelritter sehen. Nutzen bei einem Job in einer Fähigkeit, Kosten bei allen — und der Nutzen wäre statisch nicht belegbar.

**Der Restfehler, den er verkleinert hätte, ist stattdessen direkt verkleinert worden.** Die Uhrregel kann einen Tod verhindern, der noch rechtzeitig gekommen wäre; dieser Fehler ist genau so groß wie der Vorlauf. Zwei GCDs hätten bei zehn Sekunden Fenster die halbe Phase verschenkt, ein GCD verschenkt ein Viertel. Das ist die billige Abhilfe; der Messbaustein wäre die teure gewesen.

**Wieder aufzugreifen, wenn** ein konkreter Verbraucher entsteht — etwa eine gestaffelte Phase-2-Unterstützung, die den Heilungskurs gegen die Restzeit prüft. Bis dahin ist der Befund dokumentiert und die Entscheidung begründet, nicht offen.

## Offene Arbeit

### DRK: Die Betäubungsregel prüft die Tatsache, nicht die Prognose · N

Der Pull-Zweig unterbleibt, solange eine **Gruppenbetäubung** läuft: mindestens zwei betäubte Gegner und mindestens die Hälfte der Gegner in Jobreichweite, dazu ein Nachlauffenster von drei Sekunden, solange noch Betäubungsspielraum besteht (`GroupStunRunning`, A48). Was die Regel **nicht** prüft: ob der Heiler gleich betäuben wird. Der Auftraggeber hatte ursprünglich auf „solange der Weißmagier seine drei Betäubungen noch nicht abgearbeitet hat" gezielt — das wäre eine Aussage über den nächsten Zauber eines anderen Spielers.

**Kosten des Kompromisses:** Das Nachlauffenster überbrückt die Lücke zwischen zwei Anwendungen nur pauschal. Ist der Abstand größer als drei Sekunden, kann die Barriere dazwischen fallen und wird von der nächsten Betäubung unterbrochen; ist er kleiner und die Kette endet dort, wartet die Barriere drei Sekunden zu lang.

**Auflösungsbedingung:** eine Beobachtung, wie oft beides im Spiel vorkommt. Fällt der erste Fall auf, ist `headroom` aus `SurveyStuns` die vorhandene Größe für eine schärfere Fassung: Die Sperre gälte dann bis zur Betäubungsimmunität der Gegner (`StunResistance`) statt bis zum Ablauf des Fensters. Zu bedenken ist, dass die Streckung von Sanctus eine Regel **dieses** Plugins ist (`WHM_Reborn.cs:498`) — bei einem fremden Heiler greift sie nicht.

### Reihenfolge im Verteidigungspfad des Dunkelritters: teuer vor billig · N, U

`DefenseSingleAbility` gibt je Gelegenheit eine Aktion zurück und führt The Blackest Night (Priorität 20) weit vor Reprisal (`:296`, `:301`). Weil die Barriere nur 15 s Abklingzeit hat, gewinnt sie fast jede Gelegenheit; die kostenlose, gruppenweite Minderung landet erst, wenn sie gerade nicht verfügbar ist.

**Behandelt, aber nicht behoben:** Der Pull-Zweig der Option `BlackestNightUsage` verlangt jetzt, dass Reprisal zuerst liegt (A46). Das wirkt nur für den, der die Option umstellt.

**Kosten des Kompromisses:** In der Voreinstellung bleibt die Rangfolge, wie sie ist. **Auflösung:** die Reprisal-Zeilen im Pfad vor The Blackest Night ziehen. Das ist der direktere Weg und trifft alle Lagen — deshalb erst nach einer Beobachtung, ob die Bedingung im Zweig ausreicht.

### Audit + Code-Review der gesamten Codebasis

Umfang: `RotationSolver.Basic` (48k Zeilen) · RebornRotations (21k) · ExtraRotations (15k) · Updaters (4k) · UI (11k) · Commands/IPC/Data (3k). Der ganze Baum, Upstream-Code eingeschlossen. Phasen 1 bis 4 sind abgeschlossen (AUDIT_LOG A8, A10).

- **Kern tief lesen:** Rest von `DataCenter`; `StateUpdater`, `TargetUpdater`, `ActionTargetInfo`, `BaseAction`/`ActionBasicInfo`, `CustomRotation_Ability`/`GCD`, `Watcher`, `MajorUpdater`, `ObjectHelper`/`StatusHelper` sind gelesen.
- **Rotationen je Job:** Dispatch-Reihenfolge, Gates, Status-IDs, Zielwahl; bisher nur über die Scanner abgedeckt, nicht Datei für Datei.
- **`RotationSolver/UI`** jenseits der Paar- und Totcode-Scans.
- **Zweiter Durchgang** mit den Skripten aus `.github/scripts/audit/` über den bereinigten Baum, nach dem Nachrüsten der fehlenden Selbsttests.
- **Dokumentation** in `docs/rotation-flow/07-codebase-audit.md`.

**Empfehlung: das ist der nächste Arbeitsblock.** Von allen offenen Punkten ist er der einzige, der weder auf eine Entscheidung noch auf eine Spielbeobachtung wartet — die übrigen sind entweder erfasste Fremdbefunde oder brauchen Laufzeitdaten. Sinnvoller Einstieg ist der zweite Durchgang mit den dreizehn Skripten über den bereinigten Baum, weil er ohne neue Lesearbeit misst, was die bisherigen Eingriffe hinterlassen haben.

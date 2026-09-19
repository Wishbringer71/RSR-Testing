# 13 · Schadenspotential der Flächenaktionen

Entwurfsdokument nach ADR-Struktur. Es stellt den geltenden Sachstand dar; die Prüfhistorie und die
zurückgenommenen Aussagen stehen in `AUDIT_LOG.md` (A96, A98–A102).

## Ergebnis

**Die gelernte Liste gegnerischer Flächenaktionen kannte nur „drin oder nicht drin", und das ist zu
grob.** Eine Aktion, die zwei Prozent der Gesundheit nimmt, löste dieselbe Gruppenminderung aus wie
eine, die sechzig nimmt. Verbraucht wird dabei nicht der Schild, sondern die **Abklingzeit**: Eine auf
eine Bagatelle gelegte Reflexion fehlt beim nächsten großen Einschlag.

**Vorgabe des Auftraggebers:** Das Schadenspotential jeder eingehenden Flächenaktion ist zu prüfen und
**mitzuspeichern**. Was unterhalb eines geringen Schildes liegt, ist keine große Fläche, sondern eine
geringe, die nur bei Gruppenmitgliedern mit wenig Gesundheit etwas auslöst; was oberhalb eines großen
Schildes liegt, ist eine große. Weil der beobachtete Wert durch Minderung und Schild schwankt, wird
über weitere Durchläufe neu bewertet. Bestehende Einträge ohne Potential sollen nachträglich bewertet
werden können, **ohne** dabei als gering zu gelten — sein eigener Nachtrag, und der schwerste Punkt
des Entwurfs: Der ausgelieferte Bestand führt **850 Einträge**, und sie pauschal als gering zu
behandeln wäre ein Totalausfall der Gruppenminderung.

**Drei Entscheidungen tragen die Umsetzung.**

1. **„Unbewertet" ist eine eigene Kategorie und verhält sich wie heute.** Eine Aktion tritt erst in
   die neue Rechnung ein, wenn ein Einschlag gemessen wurde. Damit ist der Umstieg verhaltensneutral,
   und es braucht keinen Stichtag. Der Auftraggeber hat dieselbe Bauform unabhängig vorgeschlagen —
   „einträge ohne potential wie bisher behandeln und nur einträge mit potential nach neuer struktur".
2. **„Gering" und „groß" werden nicht gespeichert, sondern beim Verbrauch gerechnet.** Abgelegt ist
   allein der gemessene Anteil; ob er gering oder groß ist, hängt am Puffer dessen, der getroffen
   wird. Dieselbe Aktion ist damit für den vollen Tank belanglos und für den angeschlagenen Magier
   nicht — was eine gespeicherte Kategorie nie ausdrücken könnte —, und die Einordnung kann nicht
   veralten.
3. **Gemessen und gerechnet wird im Spiel, nicht hier.** Der Anteil entsteht aus beobachteten
   Treffern, wird zur Laufzeit gegen den Puffer verrechnet und wirkt sofort. Die Ablage ist deshalb
   kein Beiwerk, sondern der Baustein: Sie allein trägt eine Messung über das Ende einer Sitzung, und
   ohne sie begänne jeder Start bei null.

**Die Rechnung, die alles ersetzt, was sonst gesetzt werden müsste:**

> **Erste Stufe:** Liegt der gespeicherte Anteil bei **0,25 oder darüber**, ist die Fläche groß, und
> es wird gemindert — ohne Blick auf die Gesundheit der Gruppe. Der Wert ist die Deckung von The
> Blackest Night aus ihrem eigenen Wirktext, also ein großer Schild.
>
> **Zweite Stufe, darunter:** **Effektiver Puffer** des Mitglieds (Gesundheit einschließlich Barriere)
> **minus** dem gespeicherten Anteil seiner Maximalgesundheit. Bleibt das über der Schwelle, ab der
> der Baum von sich aus heilen würde, ist nichts zu tun; unterschreitet es sie, wird gemindert.

**Beide Stufen zusammen sind nötig, und die erste ist die später nachgerüstete** (A108, C67): Allein
mit der zweiten fragt die Regel nur, ob **Heilbedarf** entstünde, und bei gesunder Gruppe lautet die
Antwort für fast jeden Raidwide nein. Gemindert werden soll aber, **bevor** Heilbedarf entsteht. Die
zweite Stufe bleibt gleichwohl richtig für alles darunter: Dort ist die Rangregel *Heilung vor
Minderung* maßgeblich — gemindert wird, wo sonst geheilt werden müsste — und sie bringt seine
Formulierung wörtlich hervor: Zwei Prozent drücken nur den unter die Schwelle, der ohnehin fast dort
steht.

**Die Barriere zählt in dieser Rechnung mit, und das steht nicht im Widerspruch zu A85.** Dort wurde
sie aus der **Heilschwelle** entfernt, weil sie keine Gesundheit herstellt — ein Tank bei 40 % steht
bei 40 %, ob eine Barriere läuft oder nicht. Hier wird eine andere Frage gestellt: ob **dieser**
angekündigte Treffer durchschlägt. Genau das verhindert eine laufende Barriere, also gehört sie in
den Puffer. Dieselbe Größe, zwei Fragen, zwei Antworten — nachzulesen in
`07-heal-target-priority.md`, Abschnitt „Abgrenzung zur Schildanrechnung".

| Baustein | Stand |
|---|---|
| Aufnahme einer Flächenaktion in die Liste | vorhanden, Upstream (`Watcher.ActionFromEnemy`) |
| Messung: höchster Anteil an der Maximalgesundheit je Effektsatz | umgesetzt |
| Ablage je Aktion (`HostileCastingAreaPotential`, Parallelspeicher) | umgesetzt |
| Höchstwert-Fortschreibung, auch für bekannte Ids, mit gelockerter Bedingung | umgesetzt |
| Entscheidung beim Verbrauch (`DataCenter.AreaCastIsWorthMitigating`) | umgesetzt, hinter `SkipMitigationForSmallAreaCasts`, Standard an |
| Schutz der Werte gegen Zurücksetzen und gegen Absturz beim Schreiben | umgesetzt |
| Sonde: Bestand und härtester Einschlag in der Listenverwaltung | umgesetzt |
| Sonde: Anteil je Eintrag, und die Aktionen, deren Minderung unterblieb | umgesetzt |
| Übertragung auf die Tankbuster-Liste (dieselbe Frage, dieselbe Struktur) | offen |

**Die Sonde zählt Aktionen, nicht Aufrufe, und das ist keine Feinheit.** `IsHostileCastingArea` wird
je castendem Gegner je Bild gefragt; ein Zähler hätte die Bildrate gemeldet statt der Zahl
durchgelassener Einschläge — ein Surrogat des Wirkungsbereichs an der Stelle, an der der
Wirkungsbereich selbst zu messen ist. Vermerkt wird deshalb die **Aktions-Id** der unterbliebenen
Minderung; derselbe Cast sechzigmal in der Sekunde gesehen hinterlässt einen Eintrag, und die Zahl
antwortet auf die Frage, die zählt: für wie viele der bewerteten Aktionen hat diese Regel tatsächlich
eine Abklingzeit gespart. Der Vermerk ist Diagnose und wird bewusst **nicht** gespeichert — nach einem
Neustart lautet die ehrliche Antwort „in dieser Sitzung noch nicht gesehen".

**Null ist zwei verschiedene Antworten**, und deshalb nennt die Anzeige den Schalter mit: Ist
`SkipMitigationForSmallAreaCasts` aus, *kann* die Regel nicht greifen; ist er an, war jede bewertete
Aktion der bisher gespielten Inhalte ihre Minderung wert. Ohne diese Unterscheidung hätte die Sonde
einen abgeschalteten Baustein als unwirksamen gemeldet.

## Wen die Unterdrückung erreicht

Erhoben, nicht geschätzt: `AreaCastIsWorthMitigating` sitzt in `IsHostileCastingArea`, und diese Frage
hat genau **vier** Leser — zwei, die etwas bewirken, und zwei Anzeigen.

| Leser | Wirkung der Unterdrückung |
|---|---|
| `StateUpdater` → `AutoStatus.DefenseArea` (hinter `UseAoeDefense`) | die gemeinte: keine Gruppenminderung auf eine Bagatelle |
| `ObjectHelper.IsUnderThreat`, zweiter Arm | die Bagatellfläche hält die Notfall-Vollheilung nicht mehr frei — **gewollt**, es ist der Fall aus der Rezz-Meldung |
| Diagnosezeile der Gruppe, Feld `IsHostileCastingAOE` | keine, Anzeige |

**Kein unerwarteter Verbraucher, und der zweite ist der Grund, warum diese Frage gestellt werden
musste:** Ein gelernter Flächencast, der zwei Prozent nimmt, hielt Benediction genauso frei wie einer,
der sechzig nimmt. `IsUnderThreat` behält daneben seine beiden anderen Arme, Aggro und fallende
Gesundheit — zurückgehalten wird die Vollheilung also nur bei einem Ziel, das weder beschossen wird
noch fällt.

**Was dabei ungelöst bleibt, ist benannt und kein neuer Befund:** Die Flächenfrage wird gruppenweit
beantwortet („bringt der Einschlag irgendwen unter die Schwelle"), `IsUnderThreat` fragt aber für ein
**bestimmtes** Mitglied. Steht der Tank knapp über der Schwelle, gilt die Gefahr auch für den
Gerezzten. Das ist das Verhalten von vorher und durch diesen Baustein nicht verschärft; die
mitgliedsgenaue Fassung steht unten als Chance.

## Was im Kampf anders wird

| Lage | Vorher | Nachher |
|---|---|---|
| Gelernte Fläche, noch nie gemessen | volle Gruppenminderung | **unverändert** |
| Gemessene Bagatelle (2 %), Gruppe gesund | volle Gruppenminderung, Abklingzeit weg | keine Minderung, Abklingzeit bleibt |
| Dieselbe Bagatelle, ein Mitglied knapp über der Heilschwelle | volle Gruppenminderung | volle Gruppenminderung |
| Gemessene Fläche ab 25 %, Gruppe gesund | volle Gruppenminderung | **unverändert** — die Obergrenze entscheidet ohne Blick auf die Gesundheit |
| Gemessene Fläche zwischen 10 % und 25 %, Gruppe gesund | volle Gruppenminderung | keine Minderung, solange der Treffer niemanden unter die Heilschwelle drückt |
| Savage-Training, zweiter Versuch | jede Fläche gleich behandelt | die kleinen kosten nichts mehr |
| Nach dem Kampf, Blick in die Listenverwaltung | nichts zu sehen | je Aktion der gemessene Anteil, und welche davon eine Minderung gespart hat |

**Die letzte Zeile ist der eigentliche Gewinn**, und sie stammt aus einer Vorgabe des Auftraggebers:
„der effekt bei der umsetzung ergibt sich sofort bei regelmäßigen wiederholungen von inhalten.
beispiel training in extreme trials und savage raids." Ein solcher Kampf führt wenige Flächenaktionen,
und sie wiederholen sich in jedem Versuch — nach **einem** Durchlauf ist der Bestand für diesen Kampf
bewertet. Und es ist genau der Inhalt, in dem Minderungen geplant werden: Eine Abklingzeit, die an
eine Bagatelle geht, fehlt am nächsten harten Einschlag. Im Roulette fällt dieselbe Verschwendung
niemandem auf; in einem Savage-Kampf ist sie der Unterschied zwischen Durchkommen und Wipe.

## Was gemessen wird, und was nicht

**Der höchste Anteil im Effektsatz, nicht der mittlere.** Ein Effektsatz trifft acht Mitglieder mit
verschiedener Maximalgesundheit und verschiedener eigener Minderung; der höchste Anteil gehört
typischerweise dem Stoffträger und sagt, wie hart die Aktion den am stärksten Betroffenen trifft.
Genau danach fragt die Entscheidung.

**Fortgeschrieben wird als Höchstwert über alle Durchläufe.** Der Wert kann nur steigen, und jeder
Anstieg wirkt beim nächsten Verbrauch sofort.

**Ein vollständig absorbierter Treffer wird übersprungen, nicht als null gewertet.** Der Lernpfad
prüft `damageEffect.value > 0 || (damageEffect.param0 & 6) == 6`; die zweite Hälfte zählt einen
Treffer, bei dem kein Schaden ankam. *Die genaue Bedeutung dieses Flags ist hier nicht belegbar* und
wird nicht behauptet — sicher ist nur, dass `value` dann nicht der Einschlag ist. Für die **Messung**
ist ein solcher Satz wertlos: Null heißt nicht harmlos, sondern absorbiert. Für die **Aufnahme** zählt
er weiter, sonst fiele ein Raidwide aus der Liste, nur weil die Gruppe gut geschildet war.

**Aufnahme und Fortschreibung haben verschiedene Bedingungen.** Aufgenommen wird nur, wenn **jedes**
Gruppenmitglied im selben Effektsatz getroffen wurde — richtig, denn das ist der Grund, warum die
Liste Raidwides führt und keine örtlichen Flächen. Für die **Fortschreibung** einer bereits
anerkannten Id ist dieselbe Bedingung falsch: Ein Raidwide, bei dem ein Mitglied tot, unverwundbar
oder außer Reichweite war, träfe sie nicht und würde die Messung verwerfen — ausgerechnet in den
harten Kämpfen. Dort genügt ein beobachteter Treffer auf ein Gruppenmitglied. Ebenso greift die
Fortschreibung bei **bekannten** Ids: `HashSet.Add` fasst eine vorhandene Id nicht mehr an, und ohne
diesen Zusatz bekäme kein Alteintrag je ein Potential — die ganze hybride Form wäre für die 850
vorhandenen Einträge wirkungslos.

## Der Maßstab: der gemessene Anteil, mit belegter Obergrenze

**Ab einem Anteil von 0,25 der Maximalgesundheit ist die Fläche groß, unabhängig vom Zustand der
Gruppe.** Darunter entscheidet der Vergleich mit dem Puffer. Damit ist die Zwei-Schwellen-Form der
Vorgabe umgesetzt, und zwar ohne eine einzige gesetzte Zahl: Die Obergrenze ist der größte Schild,
der seine Größe im eigenen Wirktext als Anteil nennt.

| Barriere | Angabe im Wirktext | verwendbar? |
|---|---|---|
| The Blackest Night (`ActionId.resx` 1234) | „absorbs damage totaling **25 % of target's maximum HP**" | **ja** — der Maßstab für „großer Schild" |
| Shake It Off (`ActionId.resx` 1209), drei Duty-Aktionen (`DutyAction.resx` 1908, 4484, 6715) | 15 %, 10 %, 15 %, 10 % der Maximalgesundheit | **ja** — das untere Ende, siehe unten |
| Divine Benison (1404) | „absorbs damage equivalent to a heal of **500 potency**" | nein — Potenz, ohne Heilattribut nicht umrechenbar |
| Adloquium, Succor, Eukrasian Diagnosis/Prognosis | „nullifies damage equaling **% of the amount of HP restored**" | nein — der Prozentsatz fehlt im Text, der geheilte Betrag hängt am Heilattribut |

**Das untere Ende braucht keine eigene Konstante.** „Was unterhalb eines geringen Schildes liegt,
löst nur bei Gruppenmitgliedern mit wenig Gesundheit etwas aus" — das ist wörtlich die Frage, die der
Puffer-Vergleich stellt, und er stellt sie mitgliedsgenau statt an einem Trennwert. Eine zweite
Konstante träfe deshalb keine Entscheidung, die nicht ohnehin fiele.

**Warum die Obergrenze nicht entbehrlich ist — der Beleg stammt aus dem Spiel.** Ohne sie fragt die
Regel allein, ob durch den Treffer **Heilbedarf** entstünde. Das ist eine andere Frage als die nach
der Größe des Treffers, und bei gesunder Gruppe lautet ihre Antwort fast immer nein: Bei einem Puffer
von 1,0 gegen die Flächenheilschwelle von 0,65 wird erst oberhalb eines Anteils von 0,35 gemindert,
den ein gewöhnlicher Raidwide nicht erreicht. Der Auftraggeber hat die Folge gemeldet — beim
Beschwörer fielen Addle und Schimmerschild mal, mal nicht, abhängig davon, ob die Aktion schon
bewertet war. Minderung soll den Schadensstrom drosseln, **bevor** Heilbedarf entsteht; sie an das
Entstehen von Heilbedarf zu knüpfen, kehrt ihren Zweck um.

## Warum der Anteil und nicht die Potenz

Der Auftraggeber hat die **Potenz** als Maß vorgeschlagen, analog zu Heilzaubern und Schilden, und das
Ziel dahinter ist richtig: eine Größe, die nicht mit Stufe und Gegenstandsstufe altert. Erreicht wird
es auch vom Anteil an der Maximalgesundheit, und der ist der gewählte Maßstab (Erhebung in
`AUDIT_LOG.md` A24).

**Was für die Potenz spricht, und nicht kleinzureden ist: sie wäre minderungsfrei.** Der beobachtete
Schaden trägt immer die zufällig gerade wirkende Minderung mit; die Potenz täte das nicht und würde
den Zirkelschluss vollständig beseitigen, den die Höchstwertregel nur abfedert. Findet sich eine
auswertbare Potenz für Gegneraktionen, gehört sie **neben** den Anteil, nicht an seine Stelle.

**Was dagegen spricht, in der Reihenfolge des Gewichts:**

1. **Verfügbarkeit für Gegneraktionen ist unbelegt.** Im gesamten Baum wird keine Potenz gelesen; die
   465 Vorkommen in `ActionId.resx` sind Freitext in den Beschreibungen von **Spieler**aktionen. Der
   beobachtete Schadensbetrag ist demgegenüber nachweislich vorhanden.
2. **Die Potenz altert an der anderen Achse.** Sie ist gegenüber der Ausrüstung stabil, nicht
   gegenüber dem Inhalt — dieselbe Potenz ist auf Stufe 50 tödlich und auf Stufe 100 belanglos. Der
   Anteil ist gegen beide Achsen stabil, weil er Schaden und Gesundheit gemeinsam skaliert.
3. **Die Potenz ist eine Eingangsgröße, keine Gefahrenaussage.** Sie ist der erste Faktor einer
   Formel, deren übrige Faktoren — Gegnerstufe, Inhaltssynchronisation, Verwundbarkeitsstapel,
   Maximalgesundheit der Gruppe — erst bestimmen, was den Heiler betrifft.
4. **Der Vergleichspartner liegt schon in dieser Einheit vor.** Das Spiel führt Schilde als Prozent
   der Maximalgesundheit (`ICharacter.ShieldPercentage`, gelesen in `ObjectHelper.GetObjectShield`);
   die Potenz wäre eine dritte Einheit, in die erst umzurechnen wäre.

**Der Unterschied zur Heilung ist Vorhersage gegen Messung.** Bei einem Heilzauber ist die Potenz die
Rechengröße, weil der Ausgabewert **vor** dem Wirken zu bestimmen ist. Hier ist er bereits eingetreten
und beobachtet.

## Die Werte und ihr Schutz

Die Ablage ist der Baustein, nicht sein Beiwerk. Entsprechend sind alle Wege erhoben, auf denen sie
verschwinden kann.

| Weg | Wirkung |
|---|---|
| „Reset and Update AOE List" (lädt die kuratierte Liste vom Server) | Werte bleiben |
| „Reset RSR Plugin Settings" (globaler Knopf) | Werte bleiben — setzt nur `Service.Config` zurück |
| „Forget recorded damage potential" | löscht sie, und das ist sein Zweck |
| Unlesbare Datei beim Start | Werte bleiben, die Datei wird als `.corrupt` beiseitegelegt und gemeldet |

**Das Zurücksetzen der Liste lässt die Werte stehen** — Vorgabe des Auftraggebers: „es wäre schade,
wenn dann auch die Erfahrungswerte weg wären." Die kuratierte Liste neu zu laden ist ein Download, die
Messungen kosten Spielzeit; sie mit dem Listen-Reset zu verwerfen hieße, nach jedem Patch bei null
anzufangen, wegen der wenigen Aktionen, die sich tatsächlich geändert haben. Ein Wert, der zu einer
nicht mehr gelisteten Id stehenbleibt, kostet nichts: Jede Leseroute geht zuerst über die Liste.

**Einen eigenen Knopf braucht es trotzdem, weil die Höchstwert-Regel einseitig ist.** Sie hebt nur.
Eine zu niedrig bewertete Aktion korrigiert sich selbst — die Minderung unterbleibt, der nächste
Treffer kommt ungemildert an und misst sich. Eine **abgeschwächte** Aktion behält ihren zu hohen Wert
dagegen für immer; die Folge ist Minderung, wo sie nicht mehr nötig wäre, also sicher, aber falsch.
Der Ausweg ist das gezielte Verwerfen durch den Nutzer und kein automatischer Verfall — Verfall würde
genau die Eigenschaft aufheben, die eine einzelne ungemilderte Beobachtung wertvoll macht.

**Geschrieben wird über eine temporäre Datei.** `File.WriteAllText` kürzt zuerst und füllt danach; ein
Absturz dazwischen hinterlässt unlesbares JSON, und der Ladepfad beantwortet das damit, leer
anzufangen — **ohne** neu herunterzuladen, weil die Datei existiert. Für die kuratierten Listen kostet
das einen Knopfdruck, für die Erfahrungswerte alles. Und geschrieben wird dieser Speicher **im
Kampf**, bei jedem neuen Höchstwert, also genau dann, wenn ein Absturz am wahrscheinlichsten ist.

## Falsifikation

**Der Höchstwert konvergiert nicht, wenn immer gemindert wird.** Nicht widerlegt, aber entschärft:
Minderungen haben Abklingzeiten, sind verschieden stark, und die **erste** Beobachtung einer Aktion
ist immer ohne RSR-Minderung. Der Höchstwert konvergiert gegen das Maximum über die vorgekommenen
Minderungszustände, und das liegt nahe am ungemilderten Wert. Eine Rückrechnung über
`GetCurrentMitigationPercent` wäre der scheinbar sauberere Weg und ist verworfen: Diese Größe ist eine
Aufzählung bekannter Status und damit unvollständig — und eine Näherung, die den Wert **erhöht**, ist
gefährlicher als eine Beobachtung, die ihn zu niedrig ansetzt und sich selbst korrigiert.

**Eine unterschätzte Aktion wird nicht gemindert, und in einem Savage-Kampf ist das ein Wipe.** Der
ernsteste Einwand, und durchgerechnet trägt er kaum: Der Messwert ist Schaden **nach** Minderung,
Gruppenminderung liegt bei zehn bis zwanzig Prozent, ein Raidwide mit vierzig Prozent Potential misst
sich also bei zweiunddreißig bis sechsunddreißig — weit über jeder Bagatelle. Eine Verwechslung
verlangte eine Minderung von neunzig Prozent, die es nicht gibt. Fehlklassifikation bleibt auf einen
schmalen Grenzbereich beschränkt, und dort sind beide Antworten vertretbar.

**Verwundbarkeitsstapel verfälschen nach oben.** Zutreffend, und die Fehlerrichtung ist die sichere:
Überschätzung kostet eine Abklingzeit, Unterschätzung ein Gruppenmitglied.

**Die Rechnung misst am falschen Mittel.** Was `DefenseArea` auslöst, ist überwiegend prozentuale
Minderung und nicht Absorption, während „Puffer minus Einschlag" Absorption beschreibt. Zutreffend und
unauflösbar — aber folgenlos: Die Rechnung sagt nicht die Wirkung der Minderung voraus, sondern
beantwortet, **ob** dieser Einschlag jemanden in Gefahr bringt. Prozentuale Minderung wirkt zudem bei
großem Schaden am stärksten, also dort, wo die Rechnung sie auslöst.

**Serien kleiner Einschläge bleiben ungelöst.** Mehrere kleine Treffer kurz hintereinander summieren
sich; jeder einzeln unter jeder Schwelle, zusammen tödlich. Eine Einzelwertprüfung sieht das nicht.
Benannte Grenze, nicht behoben.

**Der VFX-Zweig umgeht die Rechnung.** `IsHostileCastingAOE` erkennt auch Stack- und Spread-Marker
über Effektpfade statt über Aktions-Ids; für die gibt es kein Potential, also greift die Rechnung dort
nicht. Konsistent mit „unbewertet heißt mindern", aber benannt: Diese Auslöser bleiben grob.

**Premortem — ausgeliefert, und es ändert sich nichts. Warum?** Vier Gründe, drei davon in der
Umsetzung behandelt: Alteinträge bekommen kein Potential (behoben durch Fortschreibung bei bekannten
Ids), die Fortschreibung greift in harten Kämpfen nicht (behoben durch die gelockerte Bedingung), und
die Rechnung löst nie aus (sie verglich zunächst gegen `HealthForDyingTanks` 0,15, was einen vollen
Spieler selbst bei dreißig Prozent Einschlag nicht unterschreitet — dieselbe Fehlerform wie C59, und
korrigiert auf die Heilschwelle). **Der vierte war der gefährlichste, weil er nach dem Bauen nicht
mehr auffällt:** Niemand kann sehen, ob es wirkt. Er ist mit der Sonde behoben — und die Form, in der
er sich zeigte, ist der Beweis für ihre Notwendigkeit: Der Vergleich gegen 0,15 konnte nie feuern und
wurde durch Nachdenken gefunden, nicht durch eine Messung. Dieselbe Fehlerform noch einmal, und die
Regel wäre stumm geblieben, während der Speicher sich weiter füllt.

## Was der Baustein eröffnet

**Er schließt die letzte benannte Lücke der Laufzeitbeobachtung.** Konzept 08 führt als verbliebene
Grenze die Blindheit vor dem **ersten** Treffer eines Pulls und sieht dafür eine Hochrechnung aus
Statussätzen vor — eine gepflegte Tabelle mit genau der Alterung, die dieses Projekt sonst vermeidet.
Ein angekündigter Cast mit bekanntem Potential **ist** die Vorausschau auf den ersten Treffer, und sie
kommt ohne Statussätze aus.

**Er macht die Gefahrenfrage quantitativ.** `ObjectHelper.IsUnderThreat` fragt binär, ob irgendwo eine
Flächenaktion läuft; mit dem Potential wird daraus „bringt dieser Einschlag **dieses** Mitglied unter
die Heilschwelle". Damit ist auch die Bagatellfläche erledigt, die heute die Notfall-Vollheilung
blockiert.

**Wer den Verbraucher baut, löst mehr als eine Frage.** Welche das sind und welcher Baustein wie viele
offene Punkte zugleich schließt, steht in `08-mitigation-synergy.md`, Abschnitt „Was ein Baustein
mehrfach trägt“.

**Dieselbe Frage stellt sich bei den Tankbustern.** `HostileCastingTank` trägt sie wörtlich — wie hart
schlägt dieser zu —, und `HostileCastingKnockback` und `HostileCastingStop` dieselbe Struktur. Der
Messpfad ist derselbe; was fehlt, ist je Liste ein eigener Speicher und die passende Rechnung.

**Er erlaubt die Wahl des Mittels, nicht nur die Entscheidung über das Ob — Vorgabe des
Auftraggebers:** „man könnte es auch so anpassen, dass die geeignete schadensverringerung bzw. das
geeignete schild bei dem eintreffenden schaden gewählt wird." Heute ist die Reihenfolge der
Abwehraktionen je Job fest verdrahtet, und die gemessene Größe entscheidet allein, ob diese Kette
überhaupt geöffnet wird. Mit einem Wert auf beiden Seiten ließe sich stattdessen zuordnen: der
Zehn-Prozent-Tick zieht das billige Mittel, der Vierzig-Prozent-Raidwide das stärkste verfügbare.

**Die Messgröße dieses Konzepts ist zugleich die Eingangsgröße für die Wahl des Mittels.** Abgelegt
wird der **höchste** Anteil im Effektsatz, und das ist bei gleichem absolutem Schaden der Spieler mit
der geringsten Maximalgesundheit — genau die Bezugsgröße, die die Vorgabe des Auftraggebers zur
Deckung nennt. Für die Wahl muss die Messung also nicht geändert werden, nur ihr Verbraucher.

**Die Entscheidungsordnung selbst steht in `08-mitigation-synergy.md`**, Vorgabe 5 und der Abschnitt
„Die Antwort auf einen eingehenden Treffer": Deckung in Höhe des Treffers, Heilung zuerst, sobald die
aktuelle Gesundheit nicht reicht, und Barriere samt Minderung zusätzlich, wo auch die volle nicht
reicht. Sie gilt nicht nur für Flächen — dieselbe Frage stellt sich beim Tankbuster —, deshalb steht
sie dort und nicht hier.

## Konsequenzen

**Endnutzer.** Nach dem Umstieg zunächst kein Unterschied — alle vorhandenen Einträge sind unbewertet.
Mit jedem beobachteten Einschlag wird eine Aktion bewertet; Bagatellflächen hören auf, Abklingzeiten
zu verbrauchen, und diese stehen beim nächsten großen Einschlag zur Verfügung. Der Weg dorthin ist
graduell und ohne Stichtag. In wiederholten Inhalten ist er einen Durchlauf lang.

**Autoren abgeleiteter Rotationen.** `HostileCastingArea` bleibt unverändert — Typ, Format und
Signatur. Die Ergänzung ist additiv; wer sie nicht liest, merkt nichts.

**Upstream-Pflege.** Die Eingriffe liegen in `Watcher.ActionFromEnemy` und
`DataCenter.IsHostileCastingArea`, beides Upstream-Code mit regelmäßiger Aktivität. Beide sind klein
und stehen als eigene Blöcke; `check_emergency_heal_threat.py` meldet in der CI, wenn ein Merge die
Rechnung oder ihren Rückfall auf „mindern" entfernt.

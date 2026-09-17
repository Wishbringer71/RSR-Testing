# 13 · Schadenspotential der Flächenaktionen

Entwurfsdokument nach ADR-Struktur. Es stellt den geltenden Sachstand dar; die Prüfhistorie steht in
`AUDIT_LOG.md`, der offene Punkt in `TODO.md`.

## Ergebnis

**Die Liste der gegnerischen Flächenaktionen kennt nur „drin oder nicht drin", und das ist zu grob.**
Eine Aktion, die zwei Prozent der Gesundheit nimmt, löst dieselbe Gruppenminderung aus wie eine, die
sechzig nimmt. Verbraucht wird dabei nicht „der Schild", sondern die **Abklingzeit**: Eine auf eine
Bagatelle gelegte Reflexion fehlt beim nächsten großen Einschlag.

**Vorgabe des Auftraggebers:** Das Schadenspotential jeder eingehenden Flächenaktion ist zu prüfen
und **mitzuspeichern**. Liegt es unterhalb eines geringen Schildes, gehört die Aktion nicht als
großer Schaden in die Liste, sondern als geringe Fläche, die nur bei Gruppenmitgliedern mit wenig
Gesundheit überhaupt etwas auslöst. Liegt es oberhalb eines großen Schildes oder beim Potential von
zwei oder mehr Schilden, ist sie als große Fläche zu führen. Weil der beobachtete Wert durch
Minderung und Schild schwankt, erfolgt die Aufnahme zunächst zurückhaltend; steigen die Werte über
weitere Durchläufe, wird neu bewertet, sobald ein Schwellwert überschritten ist. Bestehende Einträge
ohne Schadenspotential lassen sich so nachträglich anpassen, ohne dass sie obsolet werden.

**Sein eigener Nachtrag ist der schwerste Punkt des Entwurfs und ändert die Bauform:** Bestehende
Einträge ohne gemessenes Potential dürfen **nicht** als geringe Fläche gewertet werden, obwohl sie
mehr Schaden verursachen könnten. Gemessen am ausgelieferten Bestand — `Resources/HostileCastingArea.json`
führt **850 Einträge** — wäre das ein Totalausfall der Gruppenminderung, bis jeder einzelne Raidwide
einmal neu beobachtet wurde.

**Daraus folgt die tragende Entscheidung: „unbewertet" ist eine eigene Kategorie und verhält sich wie
heute.** Erst wenn eine Messung vorliegt, tritt eine Einstufung in Kraft. Der Umstieg ist damit
verhaltensneutral, und der Fehler beim Lernen geht in die sichere Richtung.

**Zweite Entscheidung: „gering" und „groß" werden nicht gespeichert, sondern beim Verbrauch
gerechnet.** In der Ablage steht nur der gemessene Anteil; ob er gering oder groß ist, hängt vom
Puffer dessen ab, der getroffen wird. Damit ist seine Formulierung „geringe Fläche, die nur bei
Mitgliedern mit wenig Gesundheit Effekte hat" nicht mehr eine Kategorie, die jemand setzen muss,
sondern das Ergebnis der Rechnung — und sie kann nicht veralten.

| Baustein | Stand |
|---|---|
| Aufnahme einer Flächenaktion in die Liste | **vorhanden**, Upstream (`Watcher.ActionFromEnemy`) |
| Schadensbetrag beim Lernen | **umgesetzt** (A99): höchster Anteil an der Maximalgesundheit je Effektsatz |
| Ablage mit Wert je Aktion | **umgesetzt** als Parallelspeicher `HostileCastingAreaPotential`; `HostileCastingArea` bleibt unverändert |
| Höchstwert-Fortschreibung über Durchläufe | **umgesetzt**, auch für bereits bekannte Ids und mit gelockerter Bedingung |
| Sonde: wie viel des Bestands ist bewertet | **umgesetzt** in der Listenverwaltung |
| Kategorie „unbewertet" mit heutigem Verhalten | **erfüllt, weil nichts liest** — Vorbedingung für alles Weitere |
| Entscheidung beim Verbrauch statt gespeicherter Kategorie | **umgesetzt** (A101): `DataCenter.AreaCastIsWorthMitigating`, hinter `SkipMitigationForSmallAreaCasts`, Standard an |
| **Nebenbefund:** die Liste wurde linear durchsucht, obwohl sie ein `HashSet` ist | **behoben** (A98): `Contains` an allen fünf Stellen, `check_set_lookups.py` hält es |

**Stand: gebaut.** Gemessen und gespeichert wird seit A99, gelesen und angewandt seit A101. Beides
gehört zusammen, und die Trennung in zwei Auslieferungen beruhte auf einem Denkfehler.

**Der Denkfehler, benannt vom Auftraggeber:** „die daten entstehen im spiel, werden im spiel
ausgewertet und dann genutzt und entsprechend angewendet. wann liefert also schritt 1 daten, die
schritt 2 auswerten kann? und wann schritt 2 sie nur auswerten, wenn er vorhanden ist."

Die Antwort ist: **sofort — und nie.** Sofort, weil ein Wert ab dem ersten gemessenen Einschlag da
ist. Nie, weil ihn ohne den zweiten Schritt nichts liest. Meine Aufteilung unterstellte, die
Auswertung finde **hier** statt: Werte sammeln, sie mir berichten lassen, dann entscheiden. Das ist
dreifach falsch. Die Werte liegen auf seinem Rechner und sind von hier nicht einsehbar; sie mir
berichten zu lassen wäre die Prüfaufgabe an den Nutzer, die dieses Projekt ausdrücklich ausschließt;
und die Rechnung braucht die konkreten Zahlen gar nicht, weil sie Puffer minus Einschlag gegen eine
Schwelle vergleicht und mit jedem Wert arbeitet.

**Die Dreizustandsform ist bereits die Sicherung, die ich mit der Aufteilung ein zweites Mal bauen
wollte.** Unbewertet heißt heutiges Verhalten. Liefert man beides zusammen aus, ist der Anfang überall
das alte Verhalten, und die Wirkung wächst mit jedem gemessenen Einschlag hinein — ohne Stichtag.
Getrennt ausgeliefert hätte dieselbe Sicherung gegolten, aber die Wirkung wäre um die ganze Zeit
zwischen beiden Auslieferungen verzögert worden. Bei wiederholten Inhalten ist das der Unterschied
zwischen „ab dem zweiten Versuch" und „irgendwann".

**Die Speicherung ist keine Nebensache, sondern die Bedingung dafür, dass das überhaupt trägt.** Eine
frühere Fassung dieses Dokuments beschrieb den Parallelspeicher vor allem als billig — als etwas, das
nichts kostet und deshalb keinen Widerstand verdient. Das verfehlt seinen Zweck. Der gemessene Anteil
wird **im Spiel** erhoben, **im Spiel** ausgewertet und **im Spiel** angewandt; was ihn über das
Beenden des Spiels hinweg trägt, ist allein die Datei. Ohne sie begänne jeder Start bei null, und
„nach einem Durchlauf ist der Bestand bewertet" gölte nur bis zum Ausloggen — bei einem Training über
mehrere Abende also nie. Die Ablage ist deshalb der Baustein, nicht sein Beiwerk, und alles, was sie
gefährdet — Zurücksetzen, ein Absturz beim Schreiben — ist entsprechend ernst zu nehmen.

**Die frühere Bewertung war eine Nullvariante aus Kostengründen.** Drei ihrer vier Kostenpunkte sind
durch diese Vorgabe entfallen; **ein** Punkt besteht fort, und er ist rein technisch — siehe „Was
übrig bleibt". **Und er ist zugleich der Hebel, nicht nur die Last.** Der gespeicherte Einschlag beantwortet mehr
als die Frage, aus der er entstanden ist: Er schließt die letzte benannte Lücke der
Laufzeitbeobachtung — die Blindheit vor dem **ersten** Treffer eines Pulls —, und zwar ohne die
Statussatz-Tabelle, die Konzept 08 dafür bisher vorsieht. Siehe „Was der Baustein eröffnet".

## Die Vorgabe löst drei alte Einwände auf

Der Vorschlag wurde schon einmal geprüft und mit vier Einwänden versehen. Die Vorgabe des
Auftraggebers entkräftet drei davon, und zwar nicht durch Zugeständnisse, sondern durch eine bessere
Konstruktion.

**Einwand 1 war der Zirkelschluss.** Gemessen wird der Schaden **nach** der damals wirkenden
Minderung. Hat die Gruppe beim ersten Vorkommen gut gemindert, fällt der Wert klein aus, die Aktion
gilt als harmlos, künftig wird nicht mehr gemindert — die Regel zerstörte ihre eigene Voraussetzung.

*Aufgelöst durch die Höchstwert-Fortschreibung.* Gespeichert wird nicht der letzte, sondern der
**höchste je beobachtete** Anteil. Eine einzige ungemilderte Beobachtung setzt den wahren Wert, und
spätere gut geminderte Vorkommen senken ihn nicht wieder. Der Regelkreis läuft damit in die richtige
Richtung: Wird eine Aktion unterschätzt, unterbleibt die Minderung, der nächste Einschlag kommt
ungemildert an — und wird genau dadurch richtig gemessen. Der Preis ist ein Durchlauf ohne Minderung,
und den zahlt die heutige Fassung ohnehin: Beim **ersten** Vorkommen einer Aktion steht sie noch
nicht in der Liste, also wird nie gemindert.

**Einwand 2 war die Alterung.** Ein absoluter Schadensbetrag veraltet mit Gegenstandsstufe,
Inhaltsanpassung und Verwundbarkeitsstapeln — dasselbe *Lack of Movement*-Muster wie bei den
Statusaufzählungen: heute richtig, nach der nächsten Erweiterung still falsch.

*Aufgelöst durch den Anteil.* Gemessen wird `Schaden / Maximalgesundheit` des getroffenen Mitglieds.
Der Anteil skaliert mit dem Charakter und altert deshalb nicht.

**Einwand 3 hieß, ein eigener Messbaustein rechne sich nicht.** Er ist gegenstandslos: Der Betrag
liegt bereits an der Stelle, an der gelernt wird. `Watcher` liest `damageEffect.value` und prüft
davon nur das Vorzeichen. **Der Wert ist da und wird weggeworfen.**

**Einwand 4 steht: Serien.** Mehrere kleine Einschläge kurz hintereinander summieren sich; jeder
einzeln unterhalb jeder Schwelle, zusammen tödlich. Eine Einzelwertprüfung sieht das nicht. Dieser
Einwand ist durch die Vorgabe **nicht** entkräftet und bleibt als benannte Grenze bestehen.

## Der Maßstab: was „geringer" und „großer Schild" belegbar heißen

Der Auftraggeber misst in Schilden. Das ist der richtige Gedanke — ein Schild ist die Menge, die ein
Einschlag ohne Wirkung überstehen kann —, aber die Übersetzung in eine messbare Größe ist nur über
**einen** belegten Anker möglich, und das ist beim Schreiben dieses Konzepts geprüft worden.

| Barriere | Angabe im Wirktext | umrechenbar? |
|---|---|---|
| The Blackest Night (1234) | „absorbs damage totaling **25 % of target's maximum HP**" | **ja**, unmittelbar ein Anteil |
| Divine Benison (1404) | „absorbs damage equivalent to a heal of **500 potency**" | nein — Potenz, ohne Heilattribut nicht in Gesundheit umrechenbar |
| Adloquium, Succor, Eukrasian Diagnosis/Prognosis | „nullifies damage equaling **% of the amount of HP restored**" | nein — der Prozentsatz fehlt im Text, und der geheilte Betrag hängt am Heilattribut |

**Folge:** Genau eine Barriere im Spiel nennt ihre Größe als Anteil der Maximalgesundheit, und das ist
The Blackest Night mit 25 %. Jeder weitere „Schild" als Maßstab wäre eine Setzung, kein Messergebnis
— und das Heilattribut des Heilers ist von hier nicht auslesbar und je Spieler verschieden.

Damit ist die Zwei-Schwellen-Form der Vorgabe (`unterhalb eines geringen Schildes` / `oberhalb eines
großen`) nicht ohne erfundene Zahlen umsetzbar. Sie wird deshalb nicht so gebaut, sondern ersetzt.

## Die Kategorie ist das Ergebnis einer Rechnung, nicht ihre Eingabe

**Die Schwellen werden für die Kampfentscheidung gar nicht gebraucht.** Was eine Minderung
rechtfertigt, ist nicht „die Aktion ist groß", sondern „dieser Einschlag bringt jemanden in Gefahr".
Und das ist eine Subtraktion aus Größen, die der Baum bereits führt:

> **Effektiver Puffer des Mitglieds** — Gesundheit einschließlich Barriere, `GetEffectiveHp` —
> **minus** dem gespeicherten Anteil mal seiner Maximalgesundheit. Bleibt das Ergebnis über der
> Schwelle, ab der der Baum von sich aus heilen würde, ist nichts zu tun. Unterschreitet es sie,
> wird gemindert.

**Die Schwelle ist die Heilschwelle, nicht die Sterbeschwelle** — und das ist eine Korrektur an
diesem Konzept selbst, gefunden durch das Premortem der Falsifikationsstufe. Die erste Fassung
verglich gegen `HealthForDyingTanks` (0,15). Durchgerechnet mindert das so gut wie nie: Ein
Einschlag mit dreißig Prozent Potential drückt einen vollen Spieler auf siebzig Prozent, und nur
wer bereits unter fünfundvierzig steht, käme darunter. Eine Regel, die im maßgeblichen Bereich nie
eintritt, ist keine Regel — dieselbe Fehlerform, die C59 an der Sanctus-Aussetzregel belegt hat.

Richtig ist die Schwelle, ab der ohnehin geheilt würde (`HealthSingleSpell`, `HealthAreaSpell`).
Dann lautet die Frage: **Erzeugt dieser Einschlag Heilbedarf?** Zwei Prozent auf einen vollen
Spieler tun das nicht, dreißig schon. Das ist zugleich stimmig mit der Rangregel des Auftraggebers
„Heilung vor Minderung": Gemindert wird dort, wo sonst geheilt werden müsste.

Daraus folgt alles, was die Vorgabe verlangt, ohne eine einzige gesetzte Zahl:

- **Eine geringe Fläche wirkt nur bei Mitgliedern mit wenig Gesundheit** — wörtlich die Formulierung
  des Auftraggebers, hier als Ergebnis: Zwei Prozent drücken nur den unter die Schwelle, der ohnehin
  fast dort steht.
- **Eine große Fläche wirkt immer** — sechzig Prozent unterschreiten die Schwelle bei jedem, dessen
  Puffer nicht nahezu voll ist.
- **Dieselbe Aktion ist lageabhängig gering oder groß.** Für den vollen Tank belanglos, für den
  angeschlagenen Magier nicht. Eine gespeicherte Kategorie könnte das nie ausdrücken.

**Und es löst den Grund auf, an dem die frühere Bewertung gescheitert ist.** Dort hieß es, ohne
Spielbeobachtung sei nicht belegbar, welcher Anteil „zu klein für eine Minderung" ist. Diese Frage
wird nicht mehr gestellt: Gebraucht wird kein Trennwert, sondern ein Vergleich mit dem Puffer — und
beide Seiten sind zur Laufzeit bekannt.

Die Kategorien **gering** und **groß** bleiben dennoch nützlich, aber nur als **Anzeige** in der
Listenverwaltung: Sie machen dem Auftraggeber sichtbar, was gemessen wurde. Gespeichert wird allein
der Anteil.

## Unbewertet ist kein Synonym für gering

**Der Einwand des Auftraggebers gegen seinen eigenen Punkt, und er ist entscheidend:** Bestehende
Einträge tragen keinen Wert. Würden sie als geringe Fläche behandelt, verlören 850 ausgelieferte
Einträge auf einen Schlag ihre Minderung, obwohl unter ihnen die schwersten Raidwides des Spiels
sind.

**Deshalb drei Zustände statt zwei:**

| Zustand | Bedeutung | Verhalten im Kampf |
|---|---|---|
| **unbewertet** | kein Anteil gemessen | **wie heute**: volle Gruppenminderung |
| **bewertet** | höchster beobachteter Anteil liegt vor | Rechnung Puffer minus Einschlag |
| *(gering / groß)* | Anzeige, abgeleitet aus Anteil und Lage | — nicht gespeichert |

**Die Erfahrungswerte überleben das Zurücksetzen der Liste** — Vorgabe des Auftraggebers: „wichtig
wäre aber, dass die alte liste überschrieben, geresetted werden kann. es wäre schade, wenn dann auch
die Erfahrungswerte weg wären." Die erste Umsetzung löschte sie mit; das war der teurere Fehler. Die
kuratierte Liste neu zu laden ist ein Download, die Messungen kosten Spielzeit — sie mit dem Listen-
Reset zu verwerfen hieße, nach jedem Patch bei null anzufangen, wegen der wenigen Aktionen, die sich
tatsächlich geändert haben. Ein Wert, der zu einer Id stehenbleibt, die die Liste nicht mehr führt,
kostet nichts: Jede Leseroute geht zuerst über die Liste.

**Alle Wege erhoben, auf denen die Werte verschwinden könnten** — die Frage des Auftraggebers zielte
ausdrücklich auf das Zurücksetzen und Neuladen vom Server in den Einstellungen:

| Weg | Wirkung auf die Erfahrungswerte |
|---|---|
| „Reset and Update AOE List" (lädt die kuratierte Liste vom Server) | **bleiben erhalten** — fasst den Parallelspeicher nicht an |
| „Reset RSR Plugin Settings" (globaler Knopf) | **bleiben erhalten** — setzt nur `Service.Config` zurück, nicht die Listendateien |
| „Forget recorded damage potential" | löscht sie, und das ist sein Zweck |
| **Unlesbare Datei beim Start** | **löschte sie still** — behoben, siehe unten |

**Die Einordnung „gering oder groß" kann gar nicht verlorengehen**, weil sie nicht gespeichert wird.
Sie entsteht beim Verbrauch aus dem Anteil und dem Puffer dessen, der getroffen wird — verloren gehen
kann nur der Anteil, aus dem sie folgt. Das war der Grund, sie nicht abzulegen, und er zahlt sich hier
ein zweites Mal aus.

**Der vierte Weg war real und ist behoben.** `SavePath` schrieb mit `File.WriteAllText`, das zuerst
kürzt und dann füllt; ein Absturz dazwischen hinterlässt JSON, das nicht mehr parst. Und `InitOne`
beantwortet eine unlesbare Datei damit, still von vorn anzufangen — **ohne** neu herunterzuladen, weil
die Datei ja existiert. Für die kuratierten Listen kostet das einen Knopfdruck; für die
Erfahrungswerte kostet es alles, und geschrieben wird dieser Speicher **im Kampf**, bei jedem neuen
Höchstwert — genau dann, wenn ein Absturz am wahrscheinlichsten ist. Geschrieben wird jetzt über eine
temporäre Datei und ein Ersetzen, und eine unlesbare Datei wird beiseitegelegt statt überschrieben,
mit Warnung. Der Verlust ist damit unwahrscheinlich und, wenn er doch eintritt, nicht mehr still.

**Wofür es dennoch einen eigenen Knopf gibt.** Die Höchstwert-Regel ist **einseitig**: Sie hebt nur.
Eine zu niedrig bewertete Aktion korrigiert sich selbst — die Minderung unterbleibt, der nächste
Treffer kommt ungemildert an und misst sich. Eine **abgeschwächte** Aktion behält ihren zu hohen Wert
dagegen für immer; die Folge ist Minderung, wo sie nicht mehr nötig wäre — sicher, aber falsch. Der
Ausweg ist das gezielte Verwerfen durch den Nutzer, nicht ein automatischer Verfall: Verfall würde
genau die Eigenschaft aufheben, die eine einzelne ungemilderte Beobachtung wertvoll macht.

Alle heutigen Einträge starten als **unbewertet**. Der Umstieg ändert damit kein einziges Verhalten,
und jede Aktion wechselt erst dann in die Rechnung, wenn sie tatsächlich beobachtet wurde. Das ist
zugleich die Antwort auf die Projektregel für Verhaltensänderungen ohne Nachweis: Der Standard bleibt
das bisherige Verhalten, ohne dass es dafür eine Option bräuchte.

**Der Auftraggeber hat dieselbe Bauform unabhängig vorgeschlagen** — „einträge ohne potential wie
bisher behandeln und nur einträge mit potential nach neuer struktur", von ihm als hybride Variante
eingeordnet. Sie ist es: zwei Verhaltensweisen nebeneinander, und was gilt, entscheidet nicht eine
Einstellung, sondern der Kenntnisstand über die einzelne Aktion. Dass die Konstruktion von beiden
Seiten unabhängig gefunden wurde, ist kein Beweis ihrer Richtigkeit, nimmt ihr aber die Willkür.

**Auch neu gelernte Aktionen gehören zunächst dorthin.** Der Auftraggeber schlägt vor, sie
zurückhaltend als geringe Fläche aufzunehmen; das trägt dieselbe Gefahr im Kleinen. Der erste
gemessene Wert ist zwar häufig der beste — beim ersten Vorkommen steht die Aktion noch nicht in der
Liste, also wirkte keine von RSR ausgelöste Minderung —, aber garantiert ist das nicht: Ein fremder
Heiler, ein Tank-Cooldown oder eine Vorhersage von BossModReborn können unabhängig gemindert haben.
Eine Aktion wird deshalb erst dann aus **unbewertet** entlassen, wenn ein Anteil vorliegt; die
Einstufung als gering ergibt sich danach aus der Rechnung und nicht aus dem Aufnahmezeitpunkt.

## Was gemessen wird

**Ein vollständig absorbierter Treffer wird übersprungen, nicht als null gewertet.** Der Lernpfad
prüft heute `damageEffect.value > 0 || (damageEffect.param0 & 6) == 6` — die zweite Hälfte zählt
einen Treffer, bei dem kein Schaden ankam, für die Aufnahme trotzdem als Treffer. *Die genaue
Bedeutung dieses Flags ist hier nicht belegbar* und wird deshalb nicht behauptet; sicher ist nur,
dass `value` in diesem Fall nicht der Einschlag ist. Für die **Messung** ist ein solcher Satz
wertlos: Null sagt nicht, dass die Aktion harmlos ist, sondern dass eine Barriere sie geschluckt hat.
Er wird übersprungen. Für die **Aufnahme** bleibt er zählend, wie bisher — sonst fiele ein Raidwide
aus der Liste, nur weil die Gruppe gut geschildet war.

**Der höchste Anteil im Effektsatz, nicht der mittlere.** Ein Effektsatz trifft acht Mitglieder mit
verschiedener Maximalgesundheit und verschiedener eigener Minderung; der höchste Anteil gehört
typischerweise dem Stoffträger und sagt, wie hart die Aktion den am stärksten Betroffenen trifft.
Genau danach fragt die Entscheidung.

**Fortgeschrieben wird ebenfalls als Höchstwert**, über alle Durchläufe. Das ist die Mechanik, die
Einwand 1 auflöst, und es ist die Form, in der die Neubewertung des Auftraggebers stattfindet: Der
Wert kann nur steigen, und jeder Anstieg wirkt beim nächsten Verbrauch sofort.

### Aufnahme und Fortschreibung sind zwei verschiedene Bedingungen

Zwei Punkte am heutigen Lernpfad stehen der Fortschreibung entgegen, und beide sind erst beim
Durchdenken der hybriden Form aufgefallen:

**Ein bekannter Eintrag wird heute nicht mehr angefasst.** `Watcher` ruft `HashSet.Add`; steht die Id
bereits drin, geschieht nichts weiter. Damit bekäme ein Alteintrag **nie** ein Potential und bliebe
dauerhaft unbewertet — die hybride Form wäre für die 850 vorhandenen Einträge wirkungslos, also genau
dort, wo sie gebraucht wird. Der Lernpfad muss den Wert auch bei bereits bekannter Id fortschreiben.

**Die Aufnahmebedingung ist für die Fortschreibung zu streng.** Aufgenommen wird nur, wenn **jedes**
Gruppenmitglied im selben Effektsatz getroffen wurde. Für die Aufnahme ist das richtig und der Grund,
warum die Liste Raidwides führt und keine örtlichen Flächen. Für die **Aktualisierung** eines bereits
bekannten Eintrags ist es falsch: Ein Raidwide, bei dem ein Mitglied gerade tot, unverwundbar oder
außer Reichweite war, träfe die Bedingung nicht und würde die Messung verwerfen — und das sind
ausgerechnet die harten Kämpfe, in denen der Wert am meisten zählt. Für eine Id, die bereits als
Fläche anerkannt ist, genügt ein beobachteter Treffer auf ein Gruppenmitglied.

## Falsifikation

**Hypothese: Der Höchstwert konvergiert nicht.** Wenn RSR jede gelistete Aktion mindert, sobald sie
bekannt ist, wird der ungemilderte Wert nie gemessen — der Höchstwert bliebe beim gemilderten Stand
stehen und unterschätzte die Aktion dauerhaft.

*Nicht widerlegt, aber entschärft, und die Entschärfung ist zu benennen.* Minderungen haben
Abklingzeiten und sind nicht immer verfügbar; sie sind verschieden stark; und die **erste**
Beobachtung einer Aktion ist immer ohne RSR-Minderung. Der Höchstwert konvergiert deshalb gegen das
Maximum über die tatsächlich vorgekommenen Minderungszustände, und das liegt nahe am ungemilderten
Wert. Eine Rückrechnung über `GetCurrentMitigationPercent` wäre der scheinbar sauberere Weg und ist
verworfen: Diese Größe ist eine Aufzählung bekannter Status und damit unvollständig, die Rückrechnung
bliebe eine Näherung — und eine Näherung, die den Wert **erhöht**, ist gefährlicher als eine
Beobachtung, die ihn zu niedrig ansetzt und sich selbst korrigiert.

**Hypothese: Verwundbarkeitsstapel verfälschen nach oben.** Ein Mitglied mit Verwundbarkeit nimmt
mehr Schaden; der Höchstwert wird zu groß und die Aktion dauerhaft überschätzt.

*Zutreffend, und die Fehlerrichtung ist die sichere.* Überschätzung kostet eine Abklingzeit, die
vielleicht nicht nötig war; Unterschätzung kostet ein Gruppenmitglied. Der Auftraggeber stellt die
Sicherheit der Gruppe vor den Schadensausstoß, damit ist die Richtung entschieden. Benannt bleibt sie
trotzdem.

**Hypothese: Die Rechnung misst am falschen Mittel.** Was `DefenseArea` auslöst, ist überwiegend
**prozentuale Minderung** (Reflexion, Verwirrung, Feint, Kerachole) und nicht Absorption. Der
Vergleich „Puffer minus Einschlag" beschreibt Absorption.

*Zutreffend und unauflösbar, aber folgenlos für die Entscheidung.* Die Rechnung dient nicht dazu, die
Wirkung der Minderung vorherzusagen, sondern die Frage zu beantworten, **ob** dieser Einschlag jemanden
in Gefahr bringt. Wie er dann gemindert wird, entscheidet die vorhandene Kette. Der Maßstab bleibt
insofern eine Näherung, und die Näherung liegt auf der sicheren Seite: Prozentuale Minderung wirkt
gerade bei großem Schaden am stärksten, also dort, wo die Rechnung sie auslöst.

**Premortem: Der Baustein ist gebaut und ausgeliefert, und es ändert sich nichts. Warum?** Die
prospektive Rückschau fragt nicht nach der Richtigkeit der Analyse, sondern nach der Wirkung der
Umsetzung — und sie hat hier vier Gründe gefunden, von denen drei bereits behoben sind und einer die
Rechnung selbst betraf:

1. *Alteinträge bekommen nie ein Potential*, weil `HashSet.Add` eine bekannte Id nicht anfasst —
   behandelt unter „Aufnahme und Fortschreibung".
2. *Die Fortschreibung greift in den harten Kämpfen nicht*, weil die Aufnahmebedingung jedes
   Gruppenmitglied verlangt — ebenda behandelt.
3. *Die Rechnung löst so gut wie nie aus*, weil sie gegen die Sterbeschwelle verglich statt gegen die
   Heilschwelle — oben korrigiert. **Dieser Punkt wäre ohne das Premortem im Konzept geblieben:** Die
   beiden vorhandenen Hypothesen der Falsifikationsstufe fragen, ob ein Defekt vorliegt und ob die
   Option falsch ist; beide waren mit Ja und Nein richtig beantwortet, während die Konstruktion
   wirkungslos gewesen wäre.
4. *Niemand kann sehen, ob es wirkt.* Siehe unten.

**Hypothese: Die Nullvariante ist weiterhin richtig.** Widerlegt, aber nur teilweise — siehe unten.
Der inhaltliche Grund der früheren Ablehnung ist entfallen; der technische besteht fort.

## Die Sonde gehört mitgeliefert

Diese Frage liegt in der **komplexen** Domäne: Ob eine Minderung zu Recht unterblieb, zeigt sich im
Kampf und nicht in der Analyse. Die Projektregel verlangt für diesen Fall, das Messmittel mitzubauen
statt hinterher „im Spiel nicht beobachtet" zu vermerken. Konkret sind das zwei Dinge, und beide sind
klein:

- **In der Listenverwaltung** je Eintrag der gemessene Anteil und, wo keiner vorliegt, „unbewertet".
  Damit ist sichtbar, wie schnell der Bestand aus dem unbewerteten Zustand herauswächst — die Frage,
  an der die ganze hybride Form hängt.
- **In der Diagnoseanzeige** ein Zähler, wie oft eine Minderung wegen zu kleinen Potentials
  unterblieben ist. Bleibt er über einen Kampf bei null, greift die Regel nicht, und das ist dann
  belegt statt vermutet.

Ohne diese beiden ist die Wirkung dieses Bausteins nach dem Bauen genauso unbekannt wie vorher.

## Was der Baustein eröffnet

Diese Bewertung ist nachgetragen: Die erste Fassung dieses Konzepts hat den Vorschlag gegen Kosten
und Risiko geprüft und **nicht** gegen das, was er möglich macht. Genau diese Lücke im Loop ist
inzwischen als vierte Querschnittsanforderung geschlossen (`docs/method/01-loop-evaluation-methods.md`).
Angewandt ergibt sie drei Befunde, und der erste kehrt die Bewertung um.

**Er schließt die letzte benannte Lücke der Laufzeitbeobachtung.** Konzept 08 führt als verbliebene
Grenze: blind für den **ersten** Treffer eines Pulls, weil vor 2,5 Sekunden Beobachtung keine Rate
existiert. Und es beschreibt den Weg dorthin als „Hochrechnung aus Statussätzen, je Status ein Satz
aus `Action.resx`" — eine gepflegte Liste, die genau der Alterung unterliegt, die dieses Projekt
sonst überall vermeidet. **Ein angekündigter Cast mit bekanntem Potential ist die Vorausschau auf den
ersten Treffer**, und sie kommt ohne Statussätze aus: Sie entsteht aus beobachteten Einschlägen. Das
ist der zweite Teil der hybriden Lösung, die der Auftraggeber verlangt hat — Beobachtung trägt den
laufenden Kampf, und der Eröffnungsmoment wird ebenfalls beobachtet statt gerechnet.

**Er macht die Gefahrenfrage quantitativ.** `ObjectHelper.IsUnderThreat` fragt heute binär, ob
irgendwo eine Flächenaktion läuft. Mit gespeichertem Potential wird daraus „bringt dieser Einschlag
**dieses** Mitglied unter die Heilschwelle" — dieselbe Rechnung, die die Minderung auslöst, an einer
zweiten Entscheidung. Damit ist auch die Bagatellfläche erledigt, die heute die Notfall-Vollheilung
blockiert.

**Und der einzige verbliebene Kostenpunkt ist zugleich der Hebel.** Dass `DrawActionsList` mit einer
Signatur **vier** Listen bedient, steht unten als Hindernis. Von der Möglichkeitsseite gelesen ist es
das Gegenteil: `HostileCastingTank` trägt dieselbe Frage — wie hart schlägt dieser Tankbuster zu —
und `HostileCastingKnockback` und `HostileCastingStop` dieselbe Struktur. **Ein Umbau bedient vier
Fragen.** Die Kostenrechnung der früheren Bewertung hat den Aufwand einmal gezählt und den Ertrag
einmal; richtig ist einmal Aufwand gegen vier Erträge.

*Einordnung nach Kano:* Das ist kein Basismerkmal — nichts ist kaputt, und niemand vermisst es. Es
ist auch kein Leistungsmerkmal, das ein vorhandenes Verhalten besser macht. „Die Rotation weiß, wie
hart der nächste angekündigte Einschlag trifft, und mindert nur dann" ist ein Begeisterungsmerkmal.
Der Loop erzeugt solche nicht von allein; er springt auf Defekte an.

## Was übrig bleibt

Von den vier Kostenpunkten der früheren Bewertung sind drei entfallen:

| Kostenpunkt | Stand |
|---|---|
| Persistenzvertrag: Typwechsel bricht die gespeicherte Datei | **entfallen** — der Zustand „unbewertet" ist genau der Migrationspfad. Alte Einträge bleiben gültig und verhalten sich wie bisher |
| Rückrechnung der Minderung ist eine Näherung | **entfallen** — die Höchstwert-Fortschreibung braucht keine Rückrechnung |
| Die Schwelle selbst ist ohne Spielbeobachtung nicht belegbar | **entfallen** — es gibt keine Schwelle mehr, nur den Vergleich mit dem Puffer |
| **UI-Kopplung über vier Listen** | **entfällt für den ersten Schritt** — siehe unten; für die spätere Zusammenführung besteht sie fort, und dort ist sie zugleich der Hebel |

**Der letzte Kostenpunkt entfällt, wenn die Ablage danebengestellt wird statt ersetzt.** Der Umbau von
`DrawActionsList` wird nur nötig, wenn `HostileCastingArea` selbst seinen Typ ändert. Eine eigene
Zuordnung Id → höchster Anteil, in eigener Datei, lässt die vorhandene Liste unangetastet: Die
Oberfläche bleibt, das gespeicherte Format bleibt, die Signatur bleibt. Der frühere Einwand gegen
diesen Weg war „verdoppelt die Ablage" — richtig, und gemessen an einer zusätzlichen JSON-Datei mit
einigen hundert Zahlen ist das kein Preis. Damit ist der **erste Schritt vollständig hindernisfrei**:
kein Persistenzbruch, keine UI-Änderung, keine Verhaltensänderung.

Die Zusammenführung beider Ablagen in eine bleibt möglich und ist dann der Umbau, der vier Listen
zugleich bedient — aber sie ist kein Eintrittspreis mehr.

`RotationConfigWindow.DrawActionsList(string, HashSet<uint>)` bedient mit **einer** Signatur vier
Listen — `HostileCastingTank`, `HostileCastingArea`, `HostileCastingKnockback`, `HostileCastingStop`.
Den Typ einer davon zu ändern erzwingt eine Überladung oder den Umbau aller vier. Das ist der einzige
verbliebene Einwand, und er ist rein technisch: kein fachlicher Grund, sondern Aufwand an einer
Stelle, die mit der Sache nichts zu tun hat.

**Empfehlung: umsetzen, im Zuschnitt C — erst messen, später entscheiden.** Die erste Fassung dieses
Konzepts empfahl „umsetzen, aber nicht als Erstes", gestützt auf eine Kostenrechnung, die den Umbau
von `DrawActionsList` einmal als Aufwand zählte. Gegen den Ertrag gerechnet steht derselbe Umbau für
vier Listen und schließt zugleich die letzte benannte Lücke der Laufzeitbeobachtung; er ist damit
kein Hindernis, sondern der Einstieg.

Der Nebenbefund unten ist inzwischen erledigt und war nie eine Entscheidung: Er hing an nichts, was
der Auftraggeber zu wählen hatte.

## Der Zuschnitt der Umsetzung: zusammen, nicht nacheinander

Die Frage „wird gebaut" ist eine andere als „in welchem Umfang", und sie ist gesondert durch den Loop
geführt worden. Die erste Antwort war falsch und ist hier korrigiert.

| Zuschnitt | Was entsteht | Bewertung |
|---|---|---|
| **A** nicht bauen | — | Die fachlichen Gründe sind entfallen; bleibt ohne Begründung |
| **B** Messung und Rechnung zusammen | volle Wirkung, sobald ein Wert vorliegt | **gewählt** |
| **C** erst Messung ausliefern, Rechnung später | dieselbe Endwirkung, aber um die Zeit zwischen beiden Auslieferungen verzögert | **verworfen** — siehe Denkfehler oben |
| **D** nur die Flächenliste, die drei anderen später | ein Viertel des Ertrags | Verschenkt den Hebel, den der Umbau darstellt |

**Warum B und nicht C.** C wurde mit dem Argument gewählt, der Bestand müsse erst volllaufen, damit
die Rechnung nicht ins Leere greift. Das trifft zu — und es ist genau das, was die Kategorie
*unbewertet* ohnehin leistet: Solange kein Wert vorliegt, wird gemindert wie bisher. Die zeitliche
Trennung fügt dieser Sicherung nichts hinzu und kostet die Wirkung in der Zwischenzeit. Bei
wiederholten Inhalten, wo der Bestand nach einem Durchlauf steht, ist das der ganze Nutzen.

**Was von C bleibt:** die Sonde. Sie ist vor der Rechnung entstanden und gehört dorthin — sie zeigt
den Bestand, unabhängig davon, ob eine Regel ihn liest.

### Die Anlaufzeit hängt am Inhalt, und im wichtigsten Fall ist sie ein Durchlauf

Vorgabe des Auftraggebers: „der effekt bei der umsetzung ergibt sich sofort bei regelmäßigen
wiederholungen von inhalten. beispiel training in extreme trials und savage raids."

**Das korrigiert die Einschätzung der Anlaufzeit, und zwar dort, wo es am meisten zählt.** Ein
einzelner Extreme- oder Savage-Kampf führt eine überschaubare Zahl von Flächenaktionen, und sie
wiederholen sich in **jedem** Versuch. Nach einem Durchlauf ist der Bestand für diesen Kampf
bewertet; ab dem zweiten wirkt die Rechnung vollständig. Die Rede von „Wochen" trifft nur auf
Zufallsinhalte zu, in denen selten dieselbe Aktion zweimal vorkommt.

**Und genau dort ist der Nutzen am größten.** Wer einen Kampf trainiert, plant seine Minderungen: Auf
welchen Einschlag liegt was. Eine Abklingzeit, die an eine Bagatellfläche verloren geht, fehlt am
nächsten harten Einschlag — in einem Savage-Kampf ist das der Unterschied zwischen Durchkommen und
Wipe, und es ist der Inhalt, in dem Spieler auf genau diese Planung achten. Im Roulette fällt
dieselbe Verschwendung niemandem auf.

**Folge für die Reihenfolge:** Sie bleibt. Zuschnitt C wird durch dieses Argument nicht schwächer,
sondern stärker — wer trainiert, hat nach dem ersten Versuch die Daten und kann den zweiten Schritt
unmittelbar danach nutzen, statt auf einen Bestand zu warten.

Das ist zugleich die Reihenfolge, die die Sonde verlangt: Der gemessene Anteil steht in der
Listenverwaltung, **bevor** eine Entscheidung auf ihm aufsetzt. Ob die Werte plausibel sind — ob ein
bekannter Raidwide tatsächlich als großer Anteil erscheint und eine Bagatelle als kleiner —, ist
damit im Spiel prüfbar, bevor irgendein Kampfverhalten davon abhängt.

**Falsifikation dieses Zuschnitts.** Der Einwand liegt nahe, dass eine Messung ohne Verbraucher
Vorratsarbeit ist — derselbe Satz, mit dem dieses Vorhaben schon einmal abgelehnt wurde. Er trägt
hier nicht: Der Verbraucher ist die Anzeige, der Zweck ist die Anlaufzeit, und beides ist benannt
statt erhofft. Und das Premortem: „Die Messung läuft, und die Werte sind unbrauchbar" — die beiden
Gründe dafür (Alteinträge werden nicht angefasst, die Aufnahmebedingung ist zu streng) sind oben
behandelt und gehören genau deshalb in **diesen** Schritt, nicht in den späteren.

## Nebenbefund: die Sorge ums Wachstum trifft zu, aus einem anderen Grund

Der Auftraggeber beginnt mit der Feststellung, die Liste könne ohne Limit wachsen. Das ist richtig,
aber die Ablage ist nicht das Problem — ein `HashSet` ist dafür gebaut, groß zu sein. Das Problem ist
ihre **Benutzung**:

```
foreach (var id in OtherConfiguration.HostileCastingArea)
    if (id == act.RowId) …
```

Ein `HashSet` wird hier **linear durchlaufen** statt mit `Contains` befragt — O(n) statt O(1), an
einer Stelle, die je Gegner und je Bild läuft. Bei den ausgelieferten 850 Einträgen und einem
Wall-to-Wall-Pull sind das Hunderttausende Vergleiche je Sekunde für eine einzige Frage. Dieselbe
Bauform steht an **fünf** Stellen in `DataCenter` und betrifft alle vier Listen; vier davon stammen
unverändert aus Upstream.

**Das ist die eigentliche Antwort auf sein Bedenken:** Die Liste darf wachsen, sobald sie richtig
befragt wird. **Behoben** (A98) — `Contains` an allen fünf Stellen, gehalten von
`check_set_lookups.py`, das eine zurückkehrende Schleife in der CI meldet.

*Zur Größenordnung, gemessen statt überschlagen:* Die erste Fassung dieses Abschnitts sprach von
Hunderttausenden Vergleichen je Sekunde im Wall-to-Wall-Pull. Das war überzeichnet — der Vorfilter
`IsHostileCastingBase` war nicht mitgeprüft. Er lässt das Prädikat nur durch, während ein Gegner
etwas Nicht-Unterbrechbares wirkt, das länger als ein GCD dauert und dessen Restzeit gerade im
Fenster zwischen einem und zwei GCDs liegt. Trash-Gegner erreichen das kaum; im Bosskampf sind es bei
850 Einträgen rund 50.000 Vergleiche je Sekunde für **einen** Gegner. Falsch gebaut war es trotzdem,
und der Aufwand wuchs linear mit einer Liste, die wachsen soll.

## Konsequenzen

**Endnutzer.** Nach dem Umstieg zunächst kein Unterschied — alle vorhandenen Einträge sind
unbewertet und verhalten sich wie heute. Mit jedem beobachteten Einschlag wird eine Aktion bewertet;
Bagatellflächen hören auf, Abklingzeiten zu verbrauchen, und diese stehen beim nächsten großen
Einschlag zur Verfügung. Der Weg dorthin ist graduell und ohne Stichtag.

**Autoren abgeleiteter Rotationen.** `HostileCastingArea` ist öffentlich. Ein Typwechsel bricht deren
Signatur — hier ist ein Parallelspeicher oder eine additive Ergänzung vorzusehen, und die Entscheidung
darüber gehört in den Umsetzungsvorgang, nicht in dieses Konzept.

**Upstream-Pflege.** `Watcher.ActionFromEnemy` und `DataCenter.IsHostileCastingArea` sind
Upstream-Code mit regelmäßiger Aktivität. Der Eingriff ist an beiden Stellen klein, liegt aber
mitten im fremden Fluss; eine eigene Region hält den Merge-Aufwand niedrig.

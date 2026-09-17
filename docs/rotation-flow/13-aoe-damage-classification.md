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
| Schadensbetrag beim Lernen | **verworfen**: `damageEffect.value` wird gelesen und nur gegen `> 0` geprüft |
| Ablage mit Wert je Aktion | offen — `HostileCastingArea` ist ein `HashSet<uint>` |
| Kategorie „unbewertet" mit heutigem Verhalten | offen, und Vorbedingung für alles Weitere |
| Entscheidung beim Verbrauch statt gespeicherter Kategorie | offen |
| Höchstwert-Fortschreibung über Durchläufe | offen |
| **Nebenbefund:** die Liste wird linear durchsucht, obwohl sie ein `HashSet` ist | offen, siehe `TODO.md` — trifft fünf Stellen und ist unabhängig hiervon zu beheben |

**Stand: konzipiert, nicht gebaut.** Die frühere Bewertung war eine Nullvariante aus Kostengründen.
Drei ihrer vier Kostenpunkte sind durch diese Vorgabe entfallen; **ein** Punkt besteht fort, und er
ist rein technisch — siehe „Was übrig bleibt".

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
> Schwelle, an der der Baum selbst „dieser Spieler fällt gleich" sagt (`HealthForDyingTanks`), ist
> nichts zu tun. Unterschreitet es sie, wird gemindert.

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

**Hypothese: Die Nullvariante ist weiterhin richtig.** Widerlegt, aber nur teilweise — siehe unten.
Der inhaltliche Grund der früheren Ablehnung ist entfallen; der technische besteht fort.

## Was übrig bleibt

Von den vier Kostenpunkten der früheren Bewertung sind drei entfallen:

| Kostenpunkt | Stand |
|---|---|
| Persistenzvertrag: Typwechsel bricht die gespeicherte Datei | **entfallen** — der Zustand „unbewertet" ist genau der Migrationspfad. Alte Einträge bleiben gültig und verhalten sich wie bisher |
| Rückrechnung der Minderung ist eine Näherung | **entfallen** — die Höchstwert-Fortschreibung braucht keine Rückrechnung |
| Die Schwelle selbst ist ohne Spielbeobachtung nicht belegbar | **entfallen** — es gibt keine Schwelle mehr, nur den Vergleich mit dem Puffer |
| **UI-Kopplung über vier Listen** | **besteht fort** |

`RotationConfigWindow.DrawActionsList(string, HashSet<uint>)` bedient mit **einer** Signatur vier
Listen — `HostileCastingTank`, `HostileCastingArea`, `HostileCastingKnockback`, `HostileCastingStop`.
Den Typ einer davon zu ändern erzwingt eine Überladung oder den Umbau aller vier. Das ist der einzige
verbliebene Einwand, und er ist rein technisch: kein fachlicher Grund, sondern Aufwand an einer
Stelle, die mit der Sache nichts zu tun hat.

**Empfehlung: umsetzen, aber nicht als Erstes.** Die fachliche Konstruktion trägt jetzt; was fehlt,
ist ein Umbau von `DrawActionsList`, der ohnehin ansteht. Vorher zu erledigen ist der Nebenbefund
unten, weil er unabhängig davon wirkt und die Grundlage des Bedenkens entschärft, mit dem der
Auftraggeber begonnen hat.

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
befragt wird. Der Punkt ist unabhängig von allem Übrigen in diesem Konzept und steht in `TODO.md`.

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

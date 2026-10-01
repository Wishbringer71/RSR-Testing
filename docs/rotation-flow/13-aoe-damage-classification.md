# 13 · Schadenspotential der Flächenaktionen

Entwurfsdokument nach ADR-Struktur. Es stellt den geltenden Sachstand dar; die Prüfhistorie und die
zurückgenommenen Aussagen stehen in `AUDIT_LOG.md` (A96, A98–A102).

## Ergebnis

**Die gelernte Liste gegnerischer Flächenaktionen kannte nur „drin oder nicht drin", und das ist zu
grob.** Eine Aktion, die zwei Prozent der Gesundheit nimmt, löste dieselbe Gruppenminderung aus wie
eine, die sechzig nimmt. Verbraucht wird dabei nicht der Schild, sondern die **Abklingzeit**: Eine auf
eine Bagatelle gelegte Reflexion fehlt beim nächsten großen Einschlag.

**Warum diese Liste überhaupt geführt wird — seine Begründung, wörtlich:** „bossmod liefert nicht für
jeden boss werte, sondern nur für unterstützte module. und da ist der abdeckungsgrad in bossmod auch
unterschiedlich. sich auf bossmod zu 100% zu verlassen ist fahrlässig. […] daher die eigene liste mit
dem aoe-schadensausmaß.“

**Die eigene Messung ist damit kein Zusatz zu BossModReborn, sondern die Grundlage**, auf die
zurückzufallen ist, wo das fremde Plugin nichts weiß — und sie beantwortet eine Frage, die BMR
überhaupt nicht stellt: **wie hart** der nächste Treffer ist. BMR nennt nur den Zeitpunkt. Was daraus
für die proaktive Ebene folgt, steht in `08-mitigation-synergy.md`.

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

## Wen ein gelisteter Flächencast erreicht

**Zwei Fragen, zwei Leser (A218).** Die Flächenabwehr-Flagge öffnet Gruppenminderungen — Reflexion,
Divine Veil, Sacred Soil, Addle, Feint, Troubadour, Tactician, Shield Samba — und bei Schadensausteilern
zusätzlich die ganze Einzelabwehr sowie wenige Selbstschilde im Flächenpfad (Radiant Aegis, Tempera Coat,
Tengentsu, Third Eye). Gemessen wird ein Flag an den Pfaden, die es öffnet (CLAUDE.md, Kausalität):

- **Die Flagge fragt nach der Gruppe:** Sie steht, wenn der gelistete Cast den Spieler erreicht oder
  mindestens zwei lebende Gruppenmitglieder — mehr als ein Einzelziel. Ein Stack auf einem Mitglied trifft
  alle, ein Rundumschlag am Boss Tank und Nahkämpfer. Ein Tankbuster-Kreis auf dem Tank trifft nur einen und
  öffnet sie nicht. „Die Hälfte der Gruppe" ist verworfen: Im Achterteam träfe ein Rundumschlag auf Tank und
  zwei Nahkämpfer drei von acht und bekäme keine Gruppenminderung.
- **Selbstschutz fragt nach dem Spieler** (`DataCenter.AreaHitReachesPlayer`, in den Rotationen
  `AreaHitOnMe`): Radiant Aegis, Tempera Coat, Tengentsu und Third Eye im Flächenpfad, und der Aufruf der
  Einzelabwehr eines Schadensausteilers unter der Flächenflagge (Dispatch) fallen nur, wenn der Treffer ihn
  erreicht. Das ist die Antwort auf seine Meldung zu Radiant Aegis.

**Wen die Form erreicht, aus den Spieldaten** (`DataCenter.AreaCastReaches`, je Mitglied):

- Einzelzielaktion mit Reichweite (`CastType` 1, `Range` > 0): nur das Ziel.
- Effektreichweite 0 und ein auf das Mitglied gewirkter Cast: erreicht es.
- Kreis auf ein Ziel (`CastType` 2, `Range` > 0, nicht auf sich selbst, nicht auf den Boden): um das Ziel.
  BossModReborn beschreibt dieselben Aktionen als „Boss->players, range 6 circle, stack" (Pyric Blast 25742,
  Clawful 37693).
- Linie (`CastType` 4 und 12, mit Breite): Rechteck vom Wirkenden zum Ziel, ohne Ziel in Blickrichtung;
  `XAxisModifier` ist die volle Breite (Heavy Blast Cannon 37345: Spieldaten Breite 8, BossModReborn „width 8
  rect", halbe Breite 4).
- Kreis und Kegel um den Wirkenden: Abstand vom **Mittelpunkt** des Wirkenden, ohne dessen Trefferkreis.
  BossModReborn setzt keine seiner 3494 Kreisformen um den Trefferkreis des Wirkenden größer
  (`AOEShapeCircle`, geprüft 29.09.2026); gemessen am Trefferkreis verlängerte ein großer Boss jeden
  Rundumschlag um seinen Trefferkreis — 10 y reichten bei 8 y Trefferkreis bis 18 y. Der Kegel ohne Winkel
  (steht in keinem Blatt) zählt wie ein Kreis.
- Bodenkreise (`TargetArea`): Ihr Mittelpunkt steht nur in den nativen Castdaten; weiter Abstand zum Wirkenden
  mit beiden Trefferkreisen, also eher zu weit.
- Ansturm (`CastType` 8, Effektreichweite 0): erreicht jeden.

Der Trefferkreis des Mitglieds zählt überall zugunsten des Treffers.

**Durchgerechnet an der ausgelieferten Liste** (850 Einträge, Spieldaten über xivapi, 29.09.2026): 550 Kreise
um den Wirkenden, davon 509 größer als 30 y — die erreichen im Kampf ohnehin jeden, der Trefferkreis ändert
dort nichts; 41 bis 30 y, bei denen er entschied. 152 Kreise mit Reichweite (70 auf Spieler gerichtet,
54 Bodenkreise, 26 um den Wirkenden) und 5 Linien auf ein Ziel.

**Anlass, seine Beobachtung (28.09.2026, erneut 29.09.2026):** Radiant Aegis fällt bei Tankbustern auf den Tank,
auch wenn er weit weg steht. Ob die Form oder eine andere Quelle sein Fall war, zeigt der Code nicht. Möglich
sind außerdem Marker (Stack- und Spread-Marker ohne Abstand) und BossModReborn (ein Modul, das einen
Tankbuster als Raidwide meldet). Welche Quelle in seinen Kämpfen die Abwehr öffnet, schreibt
`DefenseTrace.log` (unten); die Zeilen nennen jetzt auch, ob der Treffer ihn erreicht.

**Marker:** Es zählen nur Marker auf ihm oder einem Mitglied seiner eigenen Gruppe, Duty-Support-Begleiter
eingeschlossen, gleich wie „Heal and raise Party NPCs" steht (A220). Das gilt für die Flagge
(`IsCastingAreaVfx`, eine Upstream-Erkennung, die bis dahin jeden Marker in Reichweite las) und für den
Selbstschutz. Ein Stack auf einem Mitglied einer anderen Allianzgruppe traf keinen von ihnen und öffnete doch
ihre Gruppenminderungen und seinen Selbstschutz. Für den Selbstschutz zählt ein Stack-Marker in der eigenen
Gruppe als „erreicht ihn" (wer stackt, wird getroffen); ein Spread-Marker zählt nur auf ihm selbst. Ein
angekündigter BossModReborn-Raidwide trifft jeden.

**Tankbuster und die Einzelabwehr der Schadensausteiler (A220):** Ein angekündigter BossModReborn-Tankbuster
öffnet sie, wenn er ihn trifft. Ist der nächste vorhergesagte Treffer dieser Tankbuster, sagt BossModReborn das
selbst: Die Maske des Eintrags nennt die Getroffenen, Bit 0 ist immer der Spieler (`PartyState.PlayerSlot = 0`,
IPC `Hints.PredictedDamagePlayers`; die Liste ist nach Zeitpunkt sortiert, BossModReborn-Quelle 29.09.2026).
Gehört die Maske zu einem anderen Ereignis, gilt die bisherige Näherung: kein lebender Tank in der Gruppe. Allein
diese Näherung öffnete Radiant Aegis für einen Tankbuster auf einem Tank, den RSR nicht als Gruppenmitglied
zählte — ein Duty-Support-Tank bei ausgeschaltetem „Heal and raise Party NPCs". Ob RSR Duty-Support-Tanks über
ihren `ClassJob` überhaupt als Tank erkennt, ist nicht belegt. WrathCombo liest den Job solcher NPCs aus
`InfoProxyPartyMember` statt aus `ClassJob`; das ist ein Hinweis, keine Spielquelle. Mit der Maske hängt die
Entscheidung daran nicht mehr.

**Selbstschutz in jedem Abwehrpfad, zentral (A233):** Eine Aktion, die nur den Spieler schützt, fällt in der
Flächen- und in der Einzelabwehr nur, wenn der Treffer, der den Pfad geöffnet hat, ihn erreicht. „Nur den Spieler"
liest `BaseAction.CanUse` aus Spieldaten und Zielwahl: Das aufgelöste Ziel ist er selbst, und die Aktion hat keinen
Wirkradius. Darunter fallen Schutzwall, Verdammnis/Vengeance, Urinstinkt/Bloodwhetting auf sich, Abtausch,
Sentinel/Guardian, Bulwark, Shadow Wall, Nebula, Camouflage, Holmgang und The Blackest Night oder Heart of
Corundum mit Ziel „Self". Frei bleiben, was andere oder den Gegner trifft: Reflexion (Wirkradius 5), Abschütteln,
Divine Veil, Passage of Arms, Dark Missionary, Heart of Light, Intervention, Heart of Corundum auf das
Tankbuster-Ziel, Nascent Flash. Ein Befehl des Spielers (Makro „Defense") wird nicht geprüft.

- *Flächenpfad:* „erreicht ihn" ist `AreaHitReachesPlayer` — dieselbe Frage, die Radiant Aegis, Tengentsu und Third
  Eye bisher je Rotation stellten; die Abfragen dort bleiben und sind jetzt doppelt.
- *Einzelpfad:* „erreicht ihn" ist `SingleHitReachesPlayer`: ein Tankbuster-Marker auf ihm oder ein gelisteter
  Tankbuster auf ihn; beim Tank zusätzlich ein Cast, den ein Gegner auf den Spieler als sein Ziel wirkt, und die
  Pull-Regel (mindestens `AutoDefenseNumber` Gegner bis 3 y auf ihm, die ihn angreifen, Gesundheit unter
  `HealthForAutoDefense`); ein angekündigter BossModReborn-Tankbuster, wenn seine Maske ihn trifft. Ist die Maske
  unbekannt, zählt sie beim Tank als Treffer — die Wahrscheinlichkeit ist dann nicht gemessen, und Sicherheit geht
  vor —, bei allen anderen nur ohne lebenden Tank (wie A220).
- *Warum nicht die Flagge sperren:* Die Einzelabwehr eines Tanks trägt auch die Hilfe am anderen Tank —
  Intervention („Use Intervention on CoTank during tankbusters"), Heart of Corundum mit Ziel `Tankbuster`,
  Reflexion auf den Gegner. Ein Flag wird an den Pfaden gemessen, die es öffnet; gesperrt wird deshalb die
  Selbstschutz-Aktion, nicht der Pfad.

**Anlass, sein Protokoll vom 30.09.2026 (Krieger, Build ad73a6a5c):** Bei angekündigten Tankbustern auf Josy Akuma
und Lyx Parsingreen, die BossModReborn mit „hits you False" meldete, fielen Verdammnis und Schutzwall (19:47:08)
sowie Schutzwall und Abtausch (19:45:03); Verdammnis und Schutzwall auch bei „hits you unknown" vier Sekunden vor
dem Buster auf Josy Akuma (19:43:11). Im Kampf: Verdammnis (120 s) und Schutzwall (90 s) waren für den Buster auf
ihn selbst verbraucht. Ursache: Der Tankzweig von `ShouldAddDefenseSingle` öffnet die Einzelabwehr für jeden
angekündigten Tankbuster und für jeden Cast eines Gegners auf sein eigenes Ziel, gleich wen er trifft; der Zweig
der Schadensausteiler fragt seit A220 die Maske.

**Selbst gemessen, ob ein gelisteter Cast den Spieler erreicht (A205, Option „Skip area defence for casts that
missed you", ab Werk an):** Der Effekt-Handler hält je gelisteter Aktion fest, ob ihre letzte Landung dem
lebenden Spieler einen Treffer brachte: Schaden jeder Höhe, auch geblockt, pariert oder von einer Barriere
geschluckt, oder einen Treffer, den Unverwundbarkeit, Ausweichen oder Widerstand abwies (Effektarten 1–3, 5–7 und
teilweise Unverwundbarkeit; bis A211 nur Art 3). Mit der Option zählt ein
Cast, der ihn zuletzt verfehlte, nicht für seine Flächenabwehr — ausgewichen oder auf jemand anderen zentriert —,
bis er ihn wieder trifft; ein auf ihn gewirkter Cast zählt immer. Nur die Abwehrflagge liest es
(`IsHostileCastingAOEForMyDefense`); Vorab-Heilung und Gefährdungsprüfung behalten die Sicht der Gruppe. Der
Messwert gilt je Sitzung.

**Protokoll der Abwehrentscheidungen (`DefenseTrace.log` im Konfigurationsordner, über Sitzungen und Builds fortgeschrieben, jede Sitzung mit Datum, Uhrzeit und Commit des Builds, A208, A232):** Jede
Wahl der Abwehrkette — Flächen- und Einzelabwehr im Dispatch für alle Jobs, beim Beschwörer auch Radiant Aegis vor
einem BossModReborn-Raidwide — mit allen Quellen, die in diesem Moment stehen: Marker mit Pfad, Träger und Abstand,
gelistete Casts mit Form, Abständen, „reaches you" und dem Ergebnis ihrer letzten Landung, beim Tank auch
ungelistete Casts eines Gegners auf sein Ziel, die Pull-Regel, BossModReborn-Raidwide und -Tankbuster samt erkanntem
Tank, und je Pfad, ob der Treffer ihn erreicht („area hit reaches you", „single hit reaches you", A233). Daneben
jeder gegnerische Treffer auf ihn (ohne Auto-Attacken) und jede Landung eines gelisteten Flächencasts mit
„reached you" — ohne diese Zeile blieb offen, warum „Ätherschub" am 30.09. sechsmal die Flächenabwehr öffnete,
obwohl er ihn nie traf: Eine Landung, aus der „Skip area defence for casts that missed you" lernt, kam nicht an. So steht in der Datei, ob der Treffer, für den die
Abwehr fiel, ankam. Aufgelöst wird das Protokoll, sobald eine Datei seiner Kämpfe die Quelle zeigt und sie behoben ist.

## Wen die Unterdrückung erreicht

Erhoben, nicht geschätzt (Lauf vom 20.09.2026): `AreaCastIsWorthMitigating` sitzt in
`IsHostileCastingArea`, und über `IsHostileCastingAOE` hat diese Frage **zwei** Leser, die etwas
bewirken, und zwei Anzeigen.

| Leser | Wirkung der Unterdrückung |
|---|---|
| `StateUpdater` → `AutoStatus.DefenseArea` (hinter `UseAoeDefense`) | die gemeinte: keine Gruppenminderung auf eine Bagatelle |
| `ObjectHelper.IsUnderThreat`, zweiter Arm | die Bagatellfläche hält die Notfall-Vollheilung nicht mehr frei — **gewollt**, es ist der Fall aus der Rezz-Meldung |
| Diagnosezeile der Gruppe, Feld `IsHostileCastingAOE` | keine, Anzeige |
| Listenverwaltung, Spalte je Aktion | keine, Anzeige |

**Ein dritter wirksamer Leser wurde erwogen und wieder abgetrennt, und das ist der Grund für
`IsHostileCastingAreaUnrated`.** Die Vorausheilung sollte zunächst dieselbe Frage lesen. Sie arbeitet
aber an ihrer **eigenen**, höheren Schwelle — `HealthAreaAbility` (0,75 im Code) gegen
`HealthAreaSpell` (0,65), mit dem die Unterdrückung urteilt. Ein Treffer, der die Gruppe auf 70 %
bringt, gilt damit als „zu klein zum Mindern"; die Heilflagge hätte bei genau diesem Stand aber
ausgelöst. Die Heilung hätte also eine Entscheidung geerbt, die nicht ihre ist.

**Daraus die allgemeine Form: eine Erkennung darf keine Entscheidung enthalten.** `IsHostileCastingAOE`
trägt das Minderungsurteil in sich und ist deshalb nur für die Minderung die richtige Frage. Wer
etwas anderes entscheidet, fragt die unbewertete Erkennung und legt seine eigene Schwelle an.

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

## Der Vorfilter davor: der unterbrechbare Cast

**Bevor die Messung überhaupt gefragt wird, hat der Cast einen Filter zu passieren, und der ist der
Engpass.** `IsHostileCastingBase` verlangt kumulativ: ein Gegner wirkt, der Cast ist **nicht
unterbrechbar**, seine Gesamtzeit übersteigt einen GCD, und seine Restzeit liegt zwischen einem und
zwei GCDs. Erst danach kommt die Id-Prüfung und die Größenbewertung.

**Der Unterbrechbarkeitsfilter ist richtig gedacht und trägt nur unter einer Bedingung.** Ein
unterbrechbarer Cast soll unterbrochen werden; ihn zu mindern gäbe eine Abklingzeit für etwas aus,
das gar nicht einschlägt. Das gilt, **solange jemand unterbricht**. Der Beschwörer hat keinen
Interrupt; in einer Viererinstanz mit einem Tank, der Interject nicht einsetzt, schlägt der Cast ein,
und nichts hat geantwortet. Das ist die Spielbeobachtung des Auftraggebers — „mal wird Schimmerschild
und Addle gecastet, mal nicht" —, und sie ist damit am Code erklärt.

**Der BMR-Weg deckt einen Teil der Bosse ab, und nur einen Teil.** `BMRShouldRefreshBefore` verlangt
`BMRActive` = `BMRHasActiveModule`. BossModReborn führt Module für die Kämpfe, für die jemand eines
geschrieben hat, und deren Tiefe ist verschieden — **ein aktives Modul sagt nicht, dass es Raidwides
führt.** Beim Auftraggeber ist `UseBmrTimeline` eingeschaltet (seine Angabe); Schimmerschild ist
damit dort proaktiv gedeckt, wo ein Modul diese Ereignisart liefert, und sonst nicht. Bei Trash gibt
es ohnehin kein Modul.

**Beide Ausfälle sind still:** Sie kommen als `float.MaxValue` an, und jede Prüfung gegen ein
Zeitfenster liest das als „es kommt nichts". Die Diagnoseseite „BMR Data" nennt deshalb jetzt das
aktive Modul und je Ereignisart, ob überhaupt eine Vorhersage vorliegt. Die vollständige Erhebung,
welche Regeln des Baums so hängen, steht in `08-mitigation-synergy.md`.

**Die Antwort ist ein zweiter, eigener Weg** (`DataCenter.IsHostileCastingLargeArea`, hinter
`Mitigate a big area cast even when it is interruptible`, **Vorgabewert an**): Er fragt nicht nach
Unterbrechbarkeit und nicht nach Mindestlänge, sondern nach dem **gemessenen** Anteil — mindestens
das, was die größte Barriere des Spiels absorbiert — und nach demselben Ein-GCD-Fenster vor dem
Einschlag.

**Warum das kein Rückbau der Entscheidung aus A9 ist.** A9 hat auf seine Meldung hin einen Rückfall
entfernt, der die Verteidigung aus der **Gegnerzahl** heraus hob: kein Gefahrenbeleg, und er stand
dauernd an. Dieser Weg verlangt das Gegenteil — eine Zahl, die aus einem tatsächlichen Einschlag
stammt. Eine Aktion, von der noch niemand getroffen wurde, trägt keine Zahl und öffnet nichts.

**Und er war damals nicht baubar:** Den Anteil je Aktion gab es nicht, als der Rückfall entfernt
wurde. Die Grobheit musste deshalb der Vorfilter allein tragen. Das ist der Grund, warum dieselbe
Frage heute anders zu beantworten ist als im September.

**Getrennt gehalten statt gelockert, und das ist Absicht:** `IsHostileCastingAOE` speist auch
`ObjectHelper.IsUnderThreat` und darüber die Notfallheilung des Weißmagiers. Ein Lockern dort
verschöbe zwei Pfade auf einmal. Der neue Weg hängt allein an der Verteidigungsflagge.

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
der seine Größe im eigenen Wirktext als Anteil nennt **und auf ein anderes Gruppenmitglied gelegt werden
kann** — laut Wirktext „barrier around self or target party member" oder „… all nearby party members".
Seine Vorgabe fragt, ob ein Treffer über dem liegt, was ein großer Schild auf dem Getroffenen auffinge;
ein Schild, den nur sein Wirkender trägt, beantwortet das für niemanden sonst (A140).

| Barriere | Angabe im Wirktext | verwendbar? |
|---|---|---|
| The Blackest Night (`ActionId.resx` 1234) | „absorbs damage totaling **25 % of target's maximum HP**" | **ja** — der Maßstab für „großer Schild" |
| Manaward (157) | „nullifies damage totaling **up to 30 % of maximum HP**" | im Generator erfasst, **nicht** für die Obergrenze — nur die Schwarzmagierin selbst trägt ihn |
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
| Speichern, während der Speicher weniger hält als die Datei (Laden gescheitert, Tabelle aus irgendeinem Grund leer begonnen) | Werte bleiben — jedes Speichern führt Speicher und Datei zusammen und nimmt je Aktion den höheren Wert |
| Datei vorhanden, aber beim Speichern nicht lesbar (gesperrt, beschädigt) | Werte bleiben — das Speichern unterbleibt, „Store:" meldet es rot, der Messwert geht mit dem nächsten Speichern hinaus |
| Laden beim Start abgebrochen oder gescheitert, danach Entladen des Plugins | Werte bleiben — ein Speicher, dessen Laden nicht zu Ende lief, wird nicht geschrieben; das gilt für alle Listen, nicht nur für diese Tabelle |
| „Reset and Update AOE List", wenn der Download scheitert | die bisherige Liste bleibt; vorher wurde sie durch eine leere ersetzt, und ohne Liste wird nichts gemessen |
| Unlesbare Datei beim Start | Werte bleiben, die Datei wird als `.corrupt` beiseitegelegt und gemeldet |
| Kampfende, Zustandswechsel, Laden und Entladen (`DataCenter.ResetAllRecords`) | Werte bleiben — die Methode räumt das Laufzeitgedächtnis eines Kampfes ab und fasst keinen Speicher an |
| **Speicher wird beim Start nicht geladen** | **Totalverlust**, still — behoben, s. u. |
| Speichern während einer neuen Messung | Der Vorgang ging verloren, still; behoben — gespeichert wird ein Schnappschuss, und nur ein Schreiber zur Zeit |
| Gesamtspeichern (`OtherConfiguration.Save`, etwa nach einer neuen Knockback-Aktion im Kampf) während einer neuen Messung | Bis A224 zog es den Schnappschuss erst auf dem Pool-Thread; warf die Kopie, fielen **alle** folgenden Listen dieses Durchgangs aus, still. Behoben: Die vier im Spiel beschriebenen Speicher (Flächenliste, Schadenstabelle, Knockback-Liste, Marker-Negativliste) ziehen ihre Kopie auf dem aufrufenden Thread, auch im Gesamtspeichern |
| Messung zwischen letztem Speichern und Entladen | Verlust dieser einen Messung; behoben — der Effekt-Handler wird vor dem letzten Speichern abgehängt |

**Ablesbar ist das jetzt im Listenfenster unter „Store:".** Das Laden meldet, ob es eine Datei fand, keine fand oder eine unlesbare beiseitelegte; jedes Speichern liest die Datei zurück und meldet Erfolg nur, wenn dort so viele Einträge stehen wie geschrieben wurden. Die Zahl „Damage potential recorded" darüber ist die Tabelle im Speicher — sie sieht gleich aus, ob die Werte die Platte erreicht haben oder nicht.

**Was eine Datei mit `{}` sagt, und was nicht.** Sie entsteht auf genau zwei Wegen: beim ersten Start ohne Datei (das Laden legt die leere Tabelle an) oder in der Sitzung nach einer unlesbaren Datei (die liegt dann als `.corrupt` daneben). Jedes andere Speichern schreibt mindestens einen Eintrag, weil die Tabelle von selbst nur wächst. Seit dem Ladefix heißt `{}` also: **in keiner Sitzung seither wurde ein Wert gemessen.** Die Datei sagt nicht, warum — das sagt nur die Messstelle.

**Die Messung hat sechs Tore**, jedes einzeln hinreichend, um sie zu verhindern: der Effekt-Hook liefert überhaupt Treffer; die Quelle ist ein anvisierbarer Gegner; eine Aktion mit Wirkzeit; eine reguläre Aktion der Kategorie Zauber, Waffenfertigkeit oder Fähigkeit; ihre Id in der Flächenliste; ein Betrag über null bei einem Gruppenmitglied.

**`Record AOE actions` und die Gruppengröße gelten nur der Aufnahme in die Liste, nicht der Messung.** Die Option stammt von Upstream (2023) und entscheidet, ob neue Ids in die Liste kommen; wer sie ausschaltet, will die kuratierte Liste so behalten, wie sie geliefert wurde. Die Mindestgröße von vier ist die kleinste Gruppenzusammensetzung des Spiels (`ContentMemberType`, Zeile 2: je ein Tank, Heiler, Nahkämpfer, Fernkämpfer); darunter trifft auch ein Kegel oder ein kleiner Kreis alle, und „alle getroffen" belegt keinen Gruppentreffer mehr. Für eine bereits gelistete Id beantwortet die Messung nur, wie hart sie trifft — das hängt an keiner der beiden Bedingungen. Solange die Messung dahinter stand, fiel sie für jeden aus, der die Option abschaltete oder mit Duty Support ohne NPC-Gruppenoption spielte, und mit ihr alle drei Leser: das Auslassen kleiner Casts, das Heilen vor einem großen und das Aufheben einer Zurückhaltung. Ein Wert aus einem Lauf ohne Stufensynchronisation fällt zu klein aus; er korrigiert sich wie jede Unterschätzung beim nächsten ungemilderten Treffer, und die Höchstwert-Regel lässt ihn einen höheren Stand nie senken. Das Listenfenster und die Diagnoseanzeige zählen seit dem Laden **je Grund**, wie oft ein gegnerischer Cast den Spieler traf und woran er hängenblieb („Casts this session"). Steht dort nach einem Abend mit Raidwides kein einziger Eintrag, traf ihn kein Cast eines anvisierbaren oder unsichtbaren Gegners, oder der Hook liefert nichts; stehen dort nur Gründe ohne „measured", nennt die Zeile das Tor. Davor steht, was der Effekt-Handler überhaupt erhält („Effect handler": Effektsätze, davon von Gegnern, davon mit Schaden am Spieler, dazu Fehler und der erste Fehlertext). Null Sätze heißt: Der Hook liefert nichts. Sätze ohne Gegnertreffer heißen: Der Filter vor der Messung verwirft sie. Fehler heißen: Der Handler bricht vor der Messung ab. **Die Ursache der leeren Tabelle: die Aktionsart wurde vier Bytes breit gelesen** (A177, belegt an seinem Protokoll vom 27.09.2026). ECommons führt `EffectHeader.ActionType` an Offset 0x1F mit dem Enum `ActionType` der ClientStructs, und dessen zugrunde liegender Typ ist `uint` (aus der Assembly gelesen). Das Spielfeld ist ein Byte; ClientStructs legt `Flags` an 0x20 und `NumTargets` an 0x21. Gelesen wurde also `0x00NN0001`: im niedrigen Byte die Aktionsart, im dritten die Zahl der Ziele. In allen 422 Protokollzeilen war das niedrige Byte 1, das dritte die Zielzahl. Der Vergleich mit `ActionType.Action` schlug damit genau für jeden Satz fehl, der jemanden traf — die Messung konnte nie greifen, und ebenso wenig die Aufnahme neuer Ids, auch bei Upstream. Die Messstelle liest jetzt nur das eine Byte. Ob ECommons selbst betroffen ist: Sein `ActionEffectSet` wählt mit demselben Feld zwischen Aktion, Gegenstand und Reittier; mit Zielen fällt jeder Gegenstand in den Zweig „Aktion" (offen in `TODO.md`).

**Folge der leeren Tabelle im Kampf, an seiner Beobachtung (A181):** „es scheint, als ob addle immer zusammen mit schimmerschild gecasted wird beim beschwörer. ich habe addle alleine bislang nicht gesehen, wenn schimmerschild nicht verfügbar war beim aoe." Beide stehen im selben Verteidigungspfad, erst Radiant Aegis, im nächsten Einschiebeplatz Addle; ausgelöst durch die Verteidigungsflagge, die für einen gelisteten Cast nur fällt, wenn er als „lohnt Minderung" gilt. Unbewertet heißt „lohnt", und solange nichts gemessen wurde, war jeder gelistete Flächencast unbewertet — also jeder ein Anlass für beide. Radiant Aegis trägt zwei Ladungen je 60 s, Addle 90 s Abklingzeit (Job-Guide): Nach dem ersten Paar lädt Radiant Aegis nach, Addle nicht; ist Radiant Aegis später leer, ist Addle fast immer noch in der Abklingzeit. Ein eigener Defekt der Paarung liegt nicht vor — die beiden schützen Verschiedenes (Radiant Aegis nur den Beschwörer, 20 % seiner Maximalgesundheit; Addle die ganze Gruppe gegen diesen Gegner, 10 % magisch, 5 % physisch), und bei einem großen Treffer sind beide richtig. Mit laufender Messung (A177) lösen als klein bewertete Casts die Flagge nicht mehr aus, und Addle bleibt für die großen. Grenze: Der Weg über erkannte Flächen-VFX liest die Tabelle nicht.

**Messung und Speicherung sind belegt (A185):** Seine Datei vom 28.09.2026 enthält sechs bewertete Flächenaktionen (Augen auf, Müllentsorger, Immersion, Störender Schweif, Blitzender Boden, Fluchstimme; Namen über xivapi), beim Start geladen, beim Entladen geschrieben und zurückgelesen. Die Protokolldatei, die nach seiner Vorgabe die Ursache finden sollte, ist damit entfernt; die Zählung je Grund bleibt.

**Seine Angabe (27.09.2026):** Nach frischem Kompilieren und vier Instanzen mit Flächenschaden blieb die Tabelle leer; eine veraltete lokale Flächenliste schließt er aus, und seine hochgeladene `HostileCastingArea.json` belegt es: 850 Einträge, identisch mit der gelieferten Liste. Sofortaktionen zählen nicht mit: Sie werden nie bewertet, und als sie in „Last hit" standen, überschrieb der nächste Auto-Attack den Grund des Raidwides binnen einer Sekunde — die Zeile zeigte praktisch nur Auto-Attacks.

**Eine leere Flächenliste misst nichts**, gleich wie der Kampf verläuft; beide Fenster melden sie rot. Sie entsteht, wenn der Download beim ersten Start scheitert. Seit A196 bleibt sie auf diese Sitzung beschränkt: `InitOne` schreibt dann keine leere Datei mehr, die Liste gilt als nicht geladen (kein Speichern), und der nächste Start lädt erneut; eine unlesbare Liste wird beiseitegelegt und ebenfalls neu geladen. Downloads beim Laden sind an das Ladezeitlimit des Plugins gebunden.

**Grenze, keine Ursache:** Viele Raidwides löst ein unsichtbarer Helfer aus, oft mit einer anderen Id als der sichtbare Cast des Bosses. Solche Treffer kommen nicht an: Die Messung nimmt nur anvisierbare Quellen, und die Verbraucher lesen ohnehin nur deren Casts. Der sichtbare Cast bleibt dann unbewertet, also beim Verhalten ohne Tabelle. Wie häufig das ist, ist nicht belegt; die Zählung weist es als „cast by an untargetable enemy" aus.

**Das Zurücksetzen der Liste lässt die Werte stehen** — Vorgabe des Auftraggebers: „es wäre schade,
wenn dann auch die Erfahrungswerte weg wären." Die kuratierte Liste neu zu laden ist ein Download, die
Messungen kosten Spielzeit; sie mit dem Listen-Reset zu verwerfen hieße, nach jedem Patch bei null
anzufangen, wegen der wenigen Aktionen, die sich tatsächlich geändert haben. Ein Wert, der zu einer
nicht mehr gelisteten Id stehenbleibt, kostet nichts: Jede Leseroute geht zuerst über die Liste.

**Verworfen wird die Tabelle von Hand, nicht im Spiel** — seine Vorgabe: „wenn ich aufgrund eines
gamepatches merke, dass die alte tabelle nicht mehr funktioniert, kann ich datei auch händisch
löschen. dazu brauch ich keinen ingame-button. unnötige funktionen erzeugen auch unnötige
fehlerursachen." Der Anlass bleibt, dass die Höchstwert-Regel einseitig ist: Eine zu niedrig
bewertete Aktion korrigiert sich selbst — die Minderung unterbleibt, der nächste Treffer kommt
ungemildert an und misst sich. Eine **abgeschwächte** Aktion behält ihren zu hohen Wert dagegen für
immer; die Folge ist Minderung, wo sie nicht mehr nötig wäre, also sicher, aber falsch. Gelöscht wird
`HostileCastingAreaPotential.json` **bei geschlossenem Spiel**: Solange das Plugin läuft, hält der
Speicher die Werte, und das nächste Speichern führt sie mit der dann fehlenden Datei zusammen, also
schreibt es sie zurück. Kein automatischer Verfall — er würde genau die Eigenschaft aufheben, die eine
einzelne ungemilderte Beobachtung wertvoll macht.

**Selbstkorrektur nach einem Patch: nicht gebaut.** Eine Historie über Tage bräuchte sie nicht.
Denkbar wären zwei Formen: je Eintrag die Spielversion der Messung (nach einem Patch ersetzt die erste
neue Messung den alten Wert), oder ein gleitendes Maximum über die letzten Messungen je Aktion. Beide
tragen dasselbe Risiko: Ist die erste oder sind die letzten Messungen gemildert, sinkt der Wert zu
tief, und der nächste Treffer dieser Aktion kommt ungemildert an — ein Sicherheitsverlust, um einen
Fall zu lösen, der selten ist (ein Patch, der eine alte Aktion abschwächt) und dessen Folge nur
überflüssige Minderung ist. Dazu braucht die zweite Form eine neue feste Zahl. Das Löschen von Hand
deckt den Fall ohne dieses Risiko.

**Seit A172 kann kein Speichern mehr verlieren, was die Datei hält.** Die Tabelle wächst von selbst nur; also ist jeder Stand der Datei eine Untergrenze, und ein Speichern nimmt je Aktion das Höhere aus Speicher und Datei. Damit ist die Fehlerklasse geschlossen, nicht der Einzelfall: Gleich aus welchem Grund der Speicher leer oder unvollständig ist — Laden gescheitert, abgebrochen, ein künftiger Defekt —, die Datei behält ihren Stand. Dazu wird ein Speicher, dessen Laden in dieser Sitzung nicht zu Ende lief, gar nicht geschrieben: Das Entladen schreibt alle Listen, und ein abgebrochener Start hätte sie sonst alle geleert.

**Der schwerste Weg war keiner der erhobenen, sondern das Ausbleiben des Ladens** (A121). Der Speicher
stand nur in `OtherConfiguration.Init()`, und die ruft niemand; gerufen wird `InitAsync`. Die Tabelle
begann damit jede Sitzung leer, und weil ein Speichervorgang **die ganze** Tabelle schreibt, legte die
erste Messung des Abends — spätestens das Entladen des Plugins — die leere Fassung über den
gespeicherten Stand. Im Kampf war das Bild identisch mit „noch nichts gemessen": jede Aktion
unbewertet, beide Stufen der Rechnung wirkungslos. **Beide Einstiegspunkte teilen sich jetzt eine
einzige Ladeliste**, und `check_config_store_roundtrip.py` hält in der CI fest, dass kein Speicher
einen Schreibweg ohne erreichbaren Leseweg hat. Die Lehre daran ist allgemeiner als der Fall: Ein
Speicher, der geschrieben, aber nicht gelesen wird, ist gefährlicher als gar keiner — er ersetzt den
Bestand durch das Nichts, das er für richtig hält.

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

**Die Güte der eigenen Schätzung wird hier nicht gemessen — und die Bauform dafür steht schon.**
Der abgelegte Anteil ist eine Vorhersage: „so hart schlägt diese Aktion beim nächsten Mal“. Ob sie
zutrifft, prüft niemand; die Höchstwert-Fortschreibung korrigiert nur nach oben und nie nach
unten. `08-mitigation-synergy.md` löst dieselbe Frage für die Restzeitschätzung bereits:
`ScoreTtkForecast` hält jede Vorhersage gegen den tatsächlichen Verlauf, `GetCorrectedTTK` teilt
den Fehler heraus. Auf diese Messung übertragen heißt das: beim nächsten Einschlag derselben
Aktion den gespeicherten Anteil gegen den beobachteten halten und den Fehler **selbst herausrechnen** —
nicht anzeigen und auf eine Auswertung warten. Vorgabe des Auftraggebers: Eine Sonde, deren Auswertung
über das Modell läuft, kostet je Messwert einen Kampf, einen Bericht und eine Runde; zulässig ist nur,
was sich selbst nachsteuert. Das ist
**nicht gebaut**, und der Nutzen ist nicht bloß Diagnose: Ein Anteil, der durch eine zufällig
laufende Minderung zu niedrig gemessen wurde, bleibt heute zu niedrig, bis ein ungeminderter
Treffer ihn anhebt.

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

## Offene Punkte zu diesem Konzept

Sie stehen in `TODO.md` und sind dort unter der Überschrift des Eintrags mit **Konzept:** auf dieses
Dokument gekennzeichnet — an **einer** Stelle statt in zweien, damit keine Kopie altert.
`.github/scripts/audit/check_concept_links.py` listet sie je Konzept und nennt zugleich, wie viele
Einträge überhaupt keinem Konzept zugeordnet sind.

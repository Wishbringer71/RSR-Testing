# 13 · Schadenspotential der Flächenaktionen

**Leitfrage:** Wie hart schlägt eine angekündigte Flächenaktion – oder ein Tankbuster –, und woher weiß der Baum das?

Das Dokument gibt den geltenden Stand; die Prüfhistorie und die zurückgenommenen Aussagen stehen in `AUDIT_LOG.md`
(A96, A98–A102, A260 und folgende).

## Antwort

**Eine Liste gegnerischer Flächenaktionen, die nur „drin oder nicht drin" kennt, ist zu grob.** Eine Aktion, die zwei Prozent der Gesundheit nimmt, löste dieselbe Gruppenminderung aus wie
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
werden können, **ohne** dabei als gering zu gelten — sein eigener Nachtrag: Der ausgelieferte Bestand führte bei
der Einführung **850 Einträge** (A96), und sie pauschal als gering zu behandeln wäre ein Totalausfall der
Gruppenminderung.

**Drei Entscheidungen tragen die Umsetzung.**

1. **„Unbewertet" ist eine eigene Kategorie und verhält sich wie ohne Bewertung.** Eine Aktion tritt erst in
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

> **Erwarteter Anteil** (`DataCenter.AreaShareNow`, A441): der gespeicherte ungeminderte Anteil mal dem, was die
> Minderung durchlässt, die beim Einschlag noch steht – auf dem Wirker und auf dem Mitglied, das sie am wenigsten
> schützt, gleich wer sie legte –, umgerechnet auf seine maximalen LP samt Erhöhung.
>
> **Erste Stufe:** Liegt der erwartete Anteil bei **0,25 oder darüber**, ist die Fläche groß, und
> es wird gemindert — ohne Blick auf die Gesundheit der Gruppe. Der Wert ist die Deckung von The
> Blackest Night aus ihrem eigenen Wirktext, also ein großer Schild.
>
> **Zweite Stufe, darunter:** **Effektiver Puffer** des Mitglieds (Gesundheit einschließlich Barriere)
> **minus** dem erwarteten Anteil seiner Maximalgesundheit. Bleibt das über der Schwelle, ab der
> der Baum von sich aus heilen würde, ist nichts zu tun; unterschreitet es sie, wird gemindert.

**Beide Stufen zusammen sind nötig** (A108, C67): Allein mit der zweiten fragte die Regel nur, ob **Heilbedarf** entstünde, und bei gesunder Gruppe lautet die
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
| Messung: höchster Anteil an der Maximalgesundheit je Effektsatz, eine Serie je Mitglied summiert | umgesetzt (Serie: A349, A480) |
| Messung ungemindert zurückgerechnet, Leser unter der beim Einschlag stehenden Minderung | umgesetzt (A441) |
| Treffer über der ganzen Gesundheit samt Barriere nicht gemessen, Ablage höchstens die ganze Maximalgesundheit | umgesetzt (A298) |
| Ablage je Aktion (`HostileCastingAreaPotential`, Parallelspeicher) | umgesetzt |
| Höchstwert-Fortschreibung, auch für bekannte Ids, mit gelockerter Bedingung | umgesetzt |
| Entscheidung beim Verbrauch (`DataCenter.AreaCastIsWorthMitigating`) | umgesetzt, hinter `SkipMitigationForSmallAreaCasts`, Standard an |
| Schutz der Werte gegen Zurücksetzen und gegen Absturz beim Schreiben | umgesetzt |
| Sonde: Bestand und härtester Einschlag in der Listenverwaltung | umgesetzt |
| Sonde: Anteil je Eintrag, und die Aktionen, deren Minderung unterblieb | umgesetzt |
| Übertragung auf die Tankbuster-Liste (dieselbe Frage, dieselbe Struktur) | umgesetzt (A259), Abschnitt „Die Tankbuster-Tabelle" |

**Die Sonde zählt Aktionen, nicht Aufrufe, und das ist keine Feinheit.** `IsHostileCastingArea` wird
je castendem Gegner je Bild gefragt; ein Zähler hätte die Bildrate gemeldet statt der Zahl
durchgelassener Einschläge — ein Surrogat des Wirkungsbereichs an der Stelle, an der der
Wirkungsbereich selbst zu messen ist. Vermerkt wird deshalb die **Aktions-Id** der unterbliebenen
Minderung; derselbe Cast sechzigmal in der Sekunde gesehen hinterlässt einen Eintrag, und die Zahl
antwortet auf die Frage, die zählt: für wie viele der bewerteten Aktionen hat diese Regel tatsächlich
eine Abklingzeit gespart. Der Vermerk ist Diagnose und wird bewusst **nicht** gespeichert — nach einem
Neustart lautet die ehrliche Antwort „in dieser Sitzung noch nicht gesehen".

**Jedes Auslassen steht in `DefenseTrace.log`**, eine Zeile je Cast („area defence skipped: … expected at … under the
mitigation standing, which leaves everybody above the area heal line"), wie der BossModReborn-Pfad je Vorhersage eine schreibt („area defence held").
Ohne sie sahen in seinen Protokollen vom 18.–22.10.2026 ein Cast, den die Größenregel durchließ, und einer, den die
Erkennung nie sah, gleich aus: Schriller Kreischer traf zweimal mit 32 und 24 % ohne Zeile, beim dritten Mal öffnete
die Abwehr mit dem nachgelernten Anteil von 37 % (A334). Die Regel steuert sich selbst nach – die nächste ungeminderte
Landung hebt den Anteil –, die Zeile macht das Auslassen nur lesbar.

**Null ist zwei verschiedene Antworten**, und deshalb nennt die Anzeige den Schalter mit: Ist
`SkipMitigationForSmallAreaCasts` aus, *kann* die Regel nicht greifen; ist er an, war jede bewertete
Aktion der bisher gespielten Inhalte ihre Minderung wert. Ohne diese Unterscheidung hätte die Sonde
einen abgeschalteten Baustein als unwirksamen gemeldet.

## Die mitgelieferten Listen: Upstream und cactbot

**Die eingebaute Flächen- und Tankbuster-Liste enthält neben Upstreams Einträgen (`ActionLists`) die Casts, die cactbot
als Raidwide oder Tankbuster ansagt** (`CactbotCasts.g.cs`, A620). Beide zusammen sind die Vorgabe, auf der die
Abweichung des Nutzers liegt (`OtherConfiguration.SetList`: gespeichert wird nur, was er hinzufügt oder entfernt), also
erreichen neue Einträge auch bestehende Konfigurationen.
- *Warum:* Ein Cast kommt sonst erst nach seinem ersten Treffer in die Liste – ein Flächencast unter „Record enemy AoE
  actions" erst, wenn er alle Mitglieder einer Gruppe ab vier traf, ein Tankbuster ohne Marker erst über die
  Tankbuster-Tabelle nach seinem ersten Wirken auf den Tank. Bis dahin öffnet nur eine Vorhersage von BossModReborn die
  Abwehr, und die fehlt, wo kein Modul läuft oder eines die Art nicht vorhersagt (seine Begründung der eigenen Liste,
  oben). In Kreszentia, wo er meist ohne Gruppe spielt, kam so nie ein Cast in die Liste (A299).
- *Quelle:* `OverlayPlugin/cactbot`, `ui/raidboss/data`, Trigger vom Typ „StartsUsing" mit der Ansage `Responses.aoe`
  oder `bigAoe` (Fläche) und `tankBuster`, `tankBusterSwap`, `sharedTankBuster`, `tankCleave` (Tankbuster); die Id ist
  die des Casts, wie RSR sie am Wirkenden liest. Werkzeugquelle, Apache-2.0; übernommen sind Aktions-Ids, kein Code. Am
  Stand e79ea7f (08.10.2026): 705 Flächencasts, davon 263 schon bei Upstream, und 409 Tankbuster, davon 14 bei Upstream;
  unter den fehlenden 15 Flächencasts aus Kreszentia Süd, 10 aus Windurst, 4 Flächencasts und 3 Tankbuster aus Mistwake.
  Ids, die Upstream aus seiner Liste genommen hat (`retired` in `OtherConfiguration`), bleiben draußen.
- *Erzeugt, nicht geschrieben:* `.github/scripts/audit/cactbot_casts.py generate <cactbot>` schreibt die Datei aus einem
  cactbot-Stand und nennt Commit und Datum im Kopf; die CI prüft mit `cactbot_casts.py --check`, dass jede Id eine Zeile
  des Action-Blatts ist und keine Spieleraktion (xivapi, übersprungen, wenn es nicht erreichbar ist), und der Selbsttest
  hält die Lesart der Trigger.
- *Im Kampf:* Der erste Raidwide eines neuen Kampfes öffnet Gruppenminderung und Heilung vorab wie jeder gelistete und
  unbewertete Cast (Abschnitt „Antwort", erste Entscheidung); sein Treffer wird gemessen, und von da an zählt der
  Anteil. Der erste Tankbuster öffnet die Einzelabwehr des Tanks und die Hilfe der Heiler wie ein gelisteter.
- *Preis:* Wo cactbot einen Cast als Raidwide ansagt, der nur wenig nimmt, kostet sein erstes Auftreten eine
  Gruppenminderung; danach lässt die Größenregel ihn aus. Wer „Record enemy AoE actions" ausschaltet, um die gelieferte
  Liste zu behalten, bekommt die cactbot-Einträge als Teil dieser Lieferung.
- *Grenzen:* Neue Inhalte erst, wenn cactbot sie führt und die Datei neu erzeugt wird; Casts, deren Schaden von einer
  anderen Id kommt, misst RSR wie bisher an ihrer Landung.

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
- **Selbstschutz fragt nach dem Spieler** (`DataCenter.AreaHitReachesPlayer`): Radiant Aegis, Tempera Coat,
  Tengentsu und Third Eye im Flächenpfad fallen nur, wenn der Treffer ihn erreicht — gefragt zentral für jede
  Aktion, die nur ihn schützt (unten, A233) —, ebenso der Aufruf der Einzelabwehr eines Schadensausteilers unter
  der Flächenflagge (Dispatch). Das ist die Antwort auf seine Meldung zu Radiant Aegis.

**Wen die Form erreicht, aus den Spieldaten** (`DataCenter.AreaCastReaches`, je Mitglied):

- Einzelzielaktion mit Reichweite (`CastType` 1, `Range` > 0): nur das Ziel.
- Effektreichweite 0 und ein auf das Mitglied gewirkter Cast: erreicht es.
- Kreis auf ein Ziel (`CastType` 2, `Range` > 0, nicht auf sich selbst, nicht auf den Boden): um das Ziel.
  BossModReborn beschreibt dieselben Aktionen als „Boss->players, range 6 circle, stack" (Pyric Blast 25742,
  Clawful 37693).
- Linie (`CastType` 4 und 12, mit Breite): Rechteck vom Wirkenden in Wirkrichtung (`Character.CastRotation`, A766);
  `XAxisModifier` ist die volle Breite (Heavy Blast Cannon 37345: Spieldaten Breite 8, BossModReborn „width 8
  rect", halbe Breite 4). Art 4 reicht um den Trefferkreis des Wirkenden weiter, Art 12 nicht (unten, A731).
- Kreis um den Wirkenden: Art 2 vom **Mittelpunkt** des Wirkenden, Art 5 um seinen Trefferkreis weiter (A731).
- Kegel (Art 3 und 13): Winkel aus dem Omen der Aktion, Reichweite wie beim Kreis – Art 3 um den Trefferkreis des
  Wirkenden weiter, Art 13 nicht –, in Wirkrichtung (`Character.CastRotation`, A766; bis dahin zum Ziel des Zaubers).
  Ohne Winkel im Omen zählt der Kegel wie ein Kreis.
- Ring (Art 10): Außenradius die Effektreichweite, Innenradius aus dem Omen; wer ganz im Innenkreis steht, mit seinem
  Trefferkreis, bleibt unberührt (A731). Ein Ring mit Kegelwinkel im Omen zählt als Kegel ohne Innenkreis, ein Ring
  ohne Innenradius im Omen als Kreis.

**Mehrteilige Angriffe: die gleichnamigen Treffer zählen mit** (A765). Manche Gegner wirken eine Aktion und treffen
mit einer zweiten, gleichnamigen Aktion ohne Wirkzeit, die das Blatt direkt daneben führt. Ihre Form kann eine andere
sein. Pilgerpfad, 12.11.2026, 20:13: Der Wallfahrt-Cama wirkt „Tritt und Schwanz“ (#43132, Art 12, Linie 8 × 3 Yalm
nach vorn, 2,7 s). Getroffen hat #43131, gleichnamig, Art 13, Kegel 10 Yalm, ohne Omen. Der Treffer machte 262 % der
maximalen LP aus, und der Weißmagier starb. RSR las nur die Linie, schrieb „area hit reaches you False“ und ließ ihn
weiterwirken; die Sperre zum Ausweichen griff nicht.
- *Jetzt:* Erreicht die Form der gewirkten Aktion ein Mitglied nicht, fragt `AreaCastReaches` die Formen der
  gleichnamigen Zeilen ohne Wirkzeit und mit Effektreichweite, die im Blatt lückenlos an sie anschließen. Ein Kegel
  ohne Winkel im Omen zählt dort wie überall als Kreis (oben). Eine Zeile ohne Effektreichweite zählt nicht: Für sich
  gelesen hieße sie „erreicht jeden“, und jedes Wirken mit einer solchen Anzeige-Zeile gälte für die ganze Gruppe.
- *Im Kampf:* Wer im Bereich eines solchen zweiten Treffers steht, gilt als getroffen. Die Sperre beim Wirken hält
  einen Zauberbeginn zurück, damit er hinausgehen kann. Selbstschutz und Flächenabwehr gehen wie bei jedem Treffer, der
  ihn erreicht.
- *Preis:* Ohne Winkel im Omen zählt der Kegel als Kreis. Wer seitlich oder vorn außerhalb der Linie in 10 Yalm steht,
  gilt ebenfalls als getroffen und verliert für bis zu 2,7 s einen Zauberbeginn. Gewichtet mit einem Treffer über
  100 % der LP wiegt das wenig.
- *Gegenthese 1 (kein Defekt, er stand einfach falsch):* RSR bewegt ihn nicht. Aber die Sperre beim Wirken soll ihm
  den Schritt freihalten. Mit „reaches you False“ hielt sie nichts frei.
- *Gegenthese 2 (falsche Option, alle Aktionen eines Gegners zusammenfassen):* Ein Gegner hat viele Aktionen mit
  verschiedenen Wirkungen. Gleicher Name in lückenloser Folge trennt den Teil eines Angriffs von einem anderen Angriff.
- *Gegenthese 3 (ausgeliefert, und nichts ändert sich):* Ohne gleichnamige Nachbarzeile bleibt alles wie bisher.
  Ein Treffer ganz ohne Wirken, etwa „Tail Drive“ des Dahak am selben Abend (418 %), kündigt sich nicht an; dagegen
  hilft nur der Stand.

**Blickangriffe und die Form** (seine Frage, 13.11.2026, A769: ob die Formprüfung mit einem Blick kollidiert, bei
dem man nicht hinsehen darf).
- *Was ein Blick ist:* Er trifft, wer zum Wirkenden blickt, wenn er landet. Gefährlich wird eine eigene Aktion, weil
  das Spiel die Figur beim Drücken zum Ziel dreht, wenn die Aktion das verlangt (`NeedToFaceTarget`). Wo man steht,
  zählt nur über die Reichweite des Blicks.
- *Keine Wechselwirkung mit A765, A766 und A768:* Die Formprüfung beantwortet nur, ob eine Fläche den Spieler
  erreicht. Danach richten sich Flächenabwehr, Selbstschutz und die Sperre beim Wirken. Keiner dieser Pfade dreht die
  Figur, und keiner drückt eine Aktion auf den Wirkenden. Die Sperre hält nur einen Zauberbeginn zurück. Ein Blick
  mit großem Kreis erreicht die ganze Gruppe, deshalb ist er nicht „sicher ausweichbar“, und die Sperre greift nicht.
- *Was heute schützt:* BossModReborn erkennt einen Blick am Spielblatt (`Action.VFX` = 25, auch ohne Modul,
  `AIHintsBuilder`), und „Smart character orientation“ mit „Automatically avoid gazes“ (ab Werk an) dreht die Figur
  ab 0,5 s vor dem Blick weg. Lässt sich das Ziel nicht treffen, ohne hinzusehen, sperrt es Zauber und Angriffe. Das
  gilt auch für Aktionen, die RSR drückt (`SmartRotationTweak`, `ActionManagerEx`; 8ad17f97f, Werkzeugquelle). RSR
  selbst hat nur Upstreams Liste „Gaze/Stop“ mit vier Aktionen hinter „Stop acting while an enemy casts a Gaze/Stop
  action“ (ab Werk aus), und sie sperrt nur Zauber.
- *Lücke, auch mit BossModReborn* (sein Hinweis, 13.11.2026: „In Welt der Dunkelheit besteht das Risiko Doom zu
  bekommen … bmr hat da anscheinend auch keinen Schutz“). Belegt an seinem Mitschnitt vom 13.11.2026, 18:26 bis
  18:28: Angra Mainyu wirkt „Tödlicher Blick“ zweimal zugleich.
  - #3281 dauert 3 s und trägt das Blick-Kennzeichen (`VFX` 25). Ein Helfer wirkt #3499, 4,5 s, ohne Kennzeichen.
  - Verhängnis (#210, 10 s) kam beide Male 0,6 s nach #3499, zuerst auf 2, dann auf 9 Spieler.
  - BossModReborns Modul (`A31AngraMainyu`, Stand ef3da084b) kennt nur #3281, seine Automatik erkennt #3499 nicht.
  - Nach #3281 dreht BossModReborn nicht mehr weg. Eine Aktion auf den Boss dreht die Figur in den zweiten Blick.
  - Verhängnis lässt sich hier nur auf einer leuchtenden Glyphe aufheben (consolegameswiki, „Doom (Status Effect)“),
    also durch Bewegung, und Esuna wirkt nicht (`CanDispel` false). RSR bewegt nicht; schützen kann nur das Nicht-Hinsehen.

**Nicht in einen Blick drehen** (A770, alle Jobs; seine Vorgabe, 13.11.2026: „Die Rsr Funktion soll wirken, wenn bmr
nicht aktiv ist oder selbst Fehlern bei der Beachtung hat“). Die Regel liest BossModReborn zur Laufzeit nicht. Sie
wirkt mit ihm, ohne ihn und dort, wo er einen Blick nicht kennt; die Liste aus seinen Modulen ist nur eine Datenquelle
neben Blatt und Name. Pilgerpfad, 12.11.2026: Für die gerufenen Gegner hat BossModReborn kein Modul. Der einzige Blick
war „Tödlicher Blick“ des Gerufenen Destrudo (#44355, 9 Wirken, Kennzeichen 25). Er traf zweimal einen Spieler mit
Verhängnis und blieb 14-mal ohne Wirkung. „Starren“ (#44354), „Honigsüße Front/Linke/Rechte“ (#44356 bis #44358, Kegel
30 Yalm) und „Vielsagender Glanz“ (#44783, Kreis 6 Yalm) sind Flächen mit Omen, keine Blicke (A773). Solange ein Gegner einen Blick wirkt, der den Spieler erreicht, wählt
eine Aktion, die zum Ziel dreht (`NeedToFaceTarget`, Reichweite über 0), kein Ziel, zu dem sich der Spieler in den Blick
drehen müsste.
- *Was ein Blick ist:* eine Aktion mit dem Blick-Kennzeichen des Blatts (`VFX` 25, wie BossMod), eine Aktion, die
  BossModReborns Kampfmodule als Blick führen (A772, unten), oder eine gegnerische Aktion mit demselben Namen wie eine
  dieser beiden. Am Blatt gemessen (xivapi v2, 13.11.2026): 230 Aktionen tragen das
  Kennzeichen, keine davon eine Spieleraktion. 77 gegnerische Wirken heißen gleich und tragen es nicht, darunter die
  Helferfassungen von „Mortal Gaze“, „Hex Eye“, „Petrifaction“ und „Evil Eye“. Die Namen liest RSR einmal im
  Hintergrund; bis dahin zählt das Kennzeichen. Was BossModReborn nur in selbst gebauten Blicken führt, zählt nicht;
  Status, Kopfmarker und umgekehrte Namen ergänzen die Quellen (A776, „Umkehrung und Auge zur Laufzeit“).
- *In den Blick drehen:* Mit „Auto face target“ dreht das Spiel die Figur beim Auslösen genau aufs Ziel. Liegt das Auge
  dann innerhalb von 45°, blickt die Figur es an. Mit „Turn away from gaze attacks“ ersetzt RSR dieses Drehen im
  Vorlauf durch eine sichere Richtung im Kegel der Aktion („Richtig stehen und weiter treffen“, A778). Das Auge eines
  Wirkens liegt an dessen Ort, ohne Ort beim Wirkenden (A776). Die 45° sind BossMods halbe Breite einer verbotenen Richtung (`AIHintsBuilder`,
  `ForbiddenDirections`; Werkzeugquelle). Das Spiel nennt keinen Winkel.
- *Zeitraum und Wegdrehen* (A774, seine Frage vom 13.11.2026 nach anderen Plugins, die wegdrehen): RSR dreht die Figur
  selbst weg und hält Aktionen nur im letzten Stück zurück.
  - *Vorlauf L:* die größte Verzögerung bis zur Antwort des Servers im Gebiet plus ein Bild (gemessen von der
    Animationssperre, A753). Bis L vor dem Blick handelt RSR wie sonst, auch auf den Wirkenden. Ab L vor dem Blick
    nimmt eine Aktion, die dreht, kein Ziel mehr in Richtung des Blicks. Zeigt die Figur hinein, dreht RSR sie in
    die Mitte der nächstgelegenen sicheren Lücke. Bei einem Blick heißt das genau weg vom Wirkenden; bei Blicken aus
    West und Ost nach Nord oder Süd; bei einem umgekehrten Blick zum Wirkenden hin.
  - *Zauber mit Wirkzeit:* Während des Wirkens schaut die Figur zum Ziel. Ein Zauber zum Wirkenden des Blicks beginnt
    nur, wenn er vor L vor dem Blick endet.
  - *Ohne Messung:* Solange im Gebiet keine Antwort gemessen ist, gilt das ganze Wirken als Zeitraum, wie vor A774.
    Ebenso, wenn er „Turn away from gaze attacks“ abschaltet. Dann dreht RSR nicht, und eine frühe Aktion ließe ihn am
    Ende noch hinsehen.
  - *Drehen:* über die Spielfunktion, die eine Aktion zum Ziel dreht (`ActionManager.AutoFaceTargetPosition`), mit der
    Spieloption „Auto face target“ für diesen Aufruf eingeschaltet und danach zurückgesetzt, wie BossModReborn
    (`ActionManagerEx.FaceDirection`). Ein Wirken im Gang bricht dabei womöglich ab. Gewogen gegen Versteinerung oder
    Verhängnis ist das der kleinere Preis. Gedreht wird nur im Kampf mit eingeschaltetem RSR. Die Position ändert sich
    nicht; das ist keine Bewegung im Sinn seiner Vorgabe.
  - *Erhebung fremder Werkzeuge (13.11.2026):*
    - BossModReborn 8ad17f97f (`SmartRotationTweak`, ab Werk an): Es kennt Blicke nur aus Modulen und `VFX` 25. Es dreht
      ab 0,5 s vor dem Blick in die breiteste sichere Lücke nahe der Zielrichtung und sperrt Zauber und Angriffe, wenn
      das Ziel nicht ohne Blick zu treffen ist. Für einen Zauber im Gang erlaubt es 75° statt 45° (empirisch).
    - BossMod (Original) 6a8f5f304: derselbe Kern.
    - „Don't Look Back“ (Atomoxide 1.1.0.0, nur gebaut, kein Quelltext; gelesen aus den Metadaten): Es dreht die Figur
      nach jeder eigenen Aktion auf die Kamerarichtung zurück (`GameObject.SetRotation`, Kamera-Matrix) und kennt
      keine Blicke. Man schaltet es von Hand ein. Verworfen als Vorbild: Es dreht auch ohne Blick und richtet sich
      nach der Kamera, nicht nach den Wirkenden.
  - *Optionen:* (0) A770 lassen: Es hält Aktionen auf den Wirkenden das ganze Wirken zurück, bei Beatrice bis 8 s, also
    zwei bis drei GCDs. (1) Wie BossModReborn mit festen 0,5 s. (2) Vorlauf aus der Messung, Drehen in die
    nächstgelegene Lücke. *Gewählt:* (2). Es spart gegen (0) fast den ganzen Ausfall, braucht keinen festen Wert und
    dreht bei einer Verbindung mit großer Verzögerung früher.
  - *Gegenthese 1 (Option falsch, der Server sieht die Drehung zu spät):* L ist die größte gemessene Verzögerung bis
    zur Antwort, also Hin- und Rückweg. Die Drehung braucht nur den Hinweg. Mit Schwankungen größer als alles Gemessene
    bleibt ein Rest. Den hält die Zeile „gaze: …“ fest, und das Verhängnis zeigt sich im Mitschnitt.
  - *Gegenthese 2 (Doppel mit BossModReborn):* Beide drehen nur, solange die Richtung unsicher ist. Wer zuerst dreht,
    lässt dem anderen eine sichere Richtung, und keiner dreht zurück.
  - *Gegenthese 3 (ausgeliefert, und nichts ändert sich):* mit abgeschalteter Option oder ohne Messung. Dann gilt das
    Zurückhalten über das ganze Wirken.
  - *Lauf im Vorlauf:* Läuft er im Vorlauf, dreht das Spiel die Figur in die Laufrichtung, Bild für Bild. Läuft er vom
    Wirkenden weg, ist die Richtung sicher, und RSR tut nichts. Läuft er auf ihn zu, dreht RSR ihn jedes Bild weg, und
    das Spiel dreht ihn mit dem nächsten Schritt zurück. RSR greift nicht in die Bewegung ein; BossModReborn tut es
    (`PlayerMove`). Das wäre Bewegung im Sinn seiner Vorgabe. Der Preis ist ein Flackern der Figur für höchstens L.
    Ein Blick, auf den er zuläuft, trifft ihn, wie ohne RSR.
  - *Lücke ohne Breite:* Berühren sich zwei gesperrte Bögen, liegt die Mitte der Lücke auf einer Grenze und sieht noch
    hinein. RSR prüft jede Mitte gegen alle Blicke, bevor es dreht; gibt es keine, dreht es nicht.
- *Richtig stehen und weiter treffen* (A777, A778; seine Vorgabe vom 14.11.2026: „wenn man bereits richtig rum steht, den
  Gegner trifft, braucht nicht gedreht werden und der Kampf angehalten“; sein Hinweis dazu: „Spieler selbst kennen ja
  die Kämpfe auch und können sich selbst ja so positionieren, dass sie bedenkenlos weiter kämpfen können“; seine Rüge:
  die BossModReborn-Mechanik nicht ganz angesehen). Einordnung: Vorgabe und Hinweis.
  - *Spielmechanik (BossModReborn `ActionManagerEx`, empirisch, Werkzeugquelle):*
    - Eine Aktion mit Richtung geht nur, wenn ihr Ziel innerhalb von 45° vor der Figur liegt.
    - Ein Zauber bricht erst ab, wenn sein Ziel zu Beginn des Slidecast-Fensters mehr als 75° seitlich liegt. Ab der
      Antwort des Spiels (`CastInfo.ResponseSourceSequence`, wie `MovementLock`) zählt die Richtung nicht mehr.
    - Mit der Spieloption „Auto face target“ dreht das Spiel die Figur beim Auslösen genau aufs Ziel.
  - *Was BossModReborn tut (`SmartRotationTweak`, ab Werk an):* Es ersetzt dieses Drehen. Beim Auslösen schaltet es die
    Spieloption ab und dreht nur, wenn das Ziel nicht im Kegel vor der Figur liegt oder die Richtung in einen Blick
    sieht. Dann dreht es in das breiteste sichere Stück innerhalb des Kegels um das Ziel: 45° beim Auslösen, 75° während
    eines Zaubers. Gibt es dort keines, sperrt es.
  - *Befunde an RSR:*
    - A770–A776: RSR ließ das Spiel genau aufs Ziel drehen. Deshalb sperrte es jedes Ziel, in dessen genauer Richtung ein
      Auge lag, auch wenn eine Richtung innerhalb von 45° um das Ziel sicher war. Wer sich so stellte, dass das Ziel vorn
      und das Auge seitlich liegt, wurde trotzdem angehalten oder vom Spiel ins Auge gedreht. Ein Beispiel sind die
      Ahriman-Ebenbilder am Rand des zweiten Crucible-Bretts.
    - A774: Im Vorlauf drehte RSR in die Mitte der nächsten Lücke, bis zu 180°, also oft weg vom Ziel.
  - *Regel (A777, A778):*
    - *Beim Auslösen* einer Aktion mit Richtung, während ein Blick im Vorlauf liegt (`GazeWatch.PrepareFacing`, aus
      `BaseAction.Use`):
      - Liegt das Ziel im 45°-Kegel vor der Figur und sieht sie in keinen Blick, bleibt die Richtung.
      - Sonst dreht RSR in die Mitte des breitesten sicheren Stücks innerhalb von 45° um das Ziel.
      - In beiden Fällen ist „Auto face target“ für diesen Aufruf aus und danach wieder wie zuvor.
    - *Sperre:* Eine Aktion wartet nur, wenn es innerhalb von 45° um ihr Ziel keine sichere Richtung gibt. Für einen
      Zauber gilt zusätzlich: innerhalb von 75° bis zu seinem Ende plus Vorlauf. Das trifft vor allem den Wirkenden selbst,
      wenn sein Auge bei ihm liegt: Dort trennt keine Richtung Treffen und Blick.
    - *Wegdrehen im Vorlauf:*
      - Läuft ein eigener Zauber, den das Spiel noch nicht hat, dreht RSR innerhalb von 75° um dessen Ziel.
      - Sonst dreht es innerhalb von 45° um das aktuelle Ziel des Spielers, damit der Kampf weiterläuft.
      - Erst wenn dort nichts sicher ist, dreht es in die nächste Lücke. Sicherheit geht vor dem Treffer.
    - Ohne „Turn away from gaze attacks“ oder ohne Messung gilt die Sperre über das ganze Wirken, und RSR dreht nicht.
  - *Optionen:* (0) A776 lassen. (1) Wie BossModReborn mit festen 0,5 s. (2) Die Regel oben mit dem gemessenen Vorlauf.
    *Gewählt:* (2).
  - *Im Kampf:*
    - *Crucible, zweites Brett:* Die Ebenbilder blicken von West und Ost, der Taurus steht nördlich. RSR greift den Taurus
      ohne Pause an, denn er liegt im Kegel und kein Auge davor. Bisher hätte das Spiel bei jedem Druck genau zum Taurus
      gedreht; RSR sperrte nur, wenn ein Ebenbild in dieser genauen Richtung stand.
    - *Ein Auge 30° neben dem Ziel:* Bisher wartete jede Sofortaktion auf das Ziel. Jetzt dreht RSR 30° zur anderen Seite:
      Das Ziel liegt im Kegel, das Auge 60° seitlich.
    - *Angra Mainyu, Zauber von 2,5 s, Blick nach 1,5 s:* Der Zauber beginnt; kurz vor dem Blick dreht RSR 60° weg. Er
      trifft, der Blick nicht.
    - *Sofortaktion auf den Wirkenden selbst, Auge bei ihm:* Sie wartet im Vorlauf, die gemessene Antwortzeit plus ein
      Bild lang.
  - *Gegenthese 1 (kein Defekt, er stellt sich selbst):* Gerade dann drehte das Spiel ihn beim nächsten Druck genau aufs
    Ziel. Nur RSR kann das beim Auslösen verhindern, und BossModReborn tut es nur, wenn es selbst auslöst.
  - *Gegenthese 2 (die 45° oder 75° gelten nicht):* Beide Werte sind empirisch. Ist der Kegel enger, lehnt das Spiel
    den Druck ab, und RSR versucht es im nächsten Bild. Ein Schaden für ihn entsteht nicht, denn die
    Richtung ist sicher.
  - *Gegenthese 3 (ausgeliefert, und nichts ändert sich):* Drückt er selbst, dreht das Spiel ihn wie immer. RSR fängt
    fremde Drücke nicht ab.
  - *Preis:* Während eines Vorlaufs schaut die Figur bis zu 45° neben das Ziel. Bei Angriffen von hinten oder der Seite
    zählt der Standort, nicht die Blickrichtung; Positionals ändern sich nicht.
- *Im Kampf:* Während der 4,5 s von #3499 kämpft RSR bis L vor dem Blick weiter, auch auf Angra Mainyu. Ab dann drückt
  es nichts mehr auf ihn und nichts auf ein Ziel in seiner Richtung, und es dreht die Figur von ihm weg. Der Ausfall schrumpft so von zwei GCDs auf
  die gemessene Verzögerung plus ein Bild. Der Heiler heilt weiter, denn Heilungen
  drehen nicht. Ein Ziel in einer anderen Richtung bleibt wählbar. Ohne Messung im Gebiet hält RSR das ganze Wirken
  zurück; dann setzen Nahkämpfer ein bis zwei GCDs aus, wie es die Mechanik von Spielern verlangt.
- *Mehrere Blicke zugleich* (sein Hinweis, 13.11.2026: auf dem zweiten Crucible-Brett „von den Seiten Ringe …
  mehrere Punkte, von denen man sich wegdrehen muss“). Am Mitschnitt vom 11.11.2026 (15:42 und 15:58):
  - Der Taurus ruft zwei Ahriman-Ebenbilder an die Ränder, bei x 502 und 538 (der Mittelpunkt liegt bei 520), also
    West und Ost.
  - Jedes wirkt „Tödlicher Blick“ selbst (#48152, ohne Fläche) und über einen Helfer an derselben Stelle (#48153,
    Kreis 50 Yalm), je 4,5 bis 5 s. Zwei solche Wellen folgen 4 s nacheinander.
  - Keine der beiden Aktionen trägt das Blick-Kennzeichen; beide heißen wie #3281 und zählen deshalb als Blick.
  - Jeder Wirkende sperrt seine eigene Richtung. Mit Blicken aus West und Ost bleiben Ziele nach Nord und Süd wählbar,
    je 90°. Steht der Spieler westlich der Mitte, liegt der Taurus im Osten und bleibt bis zum Ende beider Wellen
    gesperrt. Sperren die Blicke jede Richtung, drückt RSR keine Aktion, die dreht.
  - BossModReborn führt #48153 im Modul (`TaurusPiece`, `CastGaze`, höchstens zwei).
- *Blicke ohne Kennzeichen, aus BossModReborns Modulen* (A772; sein Hinweis, 13.11.2026: „Einen ähnlichen Effekt hatte
  beatrice aus Troja“).
  - Beatrice (Fell Court of Troia) wirkt „Tödliche Vorsehung“ über Helfer: #29821 (5 s, ein Blick) und #29828 (8 s,
    zwei zugleich). Danach folgt Verhängnis #3364; Esuna wirkt hier (`CanDispel` true).
  - Beide tragen nicht das Kennzeichen 25, sondern die Wirk-Effekte 295 und 319 (`z5d1_magannoganko`, nur dieser Kampf).
    Das Blatt markiert Blicke also nicht einheitlich. Das Kennzeichen und der Name hätten beide übersehen.
  - BossModReborn führt Blicke je Kampf in seinen Modulen (`Components.CastGaze`, `CastGazes`; 80 Stellen in
    8ad17f97f, BSD-3, Werkzeugquelle). `gaze_actions.py` liest daraus nur die Ids und ob der Blick umgekehrt ist
    (`inverted`). Es schreibt sie nach `GazeActions.g.cs` und prüft sie in der CI gegen das Aktionsblatt. Eigene
    Blickformen eines Moduls (`GenericGaze`, 24 Stellen) bleiben außen vor; ihre Ids stehen nicht im Aufruf.
  - *Umgekehrter Blick:* Dort trifft, wer wegsieht. Für diese Ids gilt die Regel umgekehrt. Eine Aktion, die dreht,
    nimmt kein Ziel außerhalb von 45° um die Richtung des Wirkenden. Die gleichnamige Erweiterung gilt nicht für
    umgekehrte Blicke, weil ein Name dort zwei Bedeutungen haben kann.
  - *Gegenthese (veraltet):* Ein neuer Kampf kommt erst mit dem nächsten Lauf des Generators. Bis dahin gelten das
    Kennzeichen und der Name.
- *Optionen:* (0) nichts. (1) Upstreams Liste „Gaze/Stop“ füllen: eine Id je Kampf, und sie sperrt nur Zauber.
  (2) BossModReborn fragen: Seine verbotenen Richtungen gibt es nicht heraus (A575), und #3499 kennt es nicht. (3) Die
  Regel oben. *Gewählt:* (3). Sie deckt jeden Kampf aus dem Blatt, auch ohne BossModReborn, und mit ihm wirkt sie in
  dieselbe Richtung, ohne Doppel.
- *Gegenthese 1 (kein Defekt, er dreht sich selbst weg):* Das tut er, und RSR drehte ihn mit der nächsten Aktion
  zurück. Am Mitschnitt: Das Verhängnis traf 9 von 24, alle nach #3499.
- *Gegenthese 2 (Option falsch, gleicher Name ist kein Blick):* Einige der 77 könnten Begleitaktionen ohne Blick sein.
  *Preis:* Für ein Wirken hält RSR Aktionen in einer Richtung zurück, ein bis zwei GCDs. Gewogen gegen Versteinerung
  oder Verhängnis wiegt das wenig.
- *Gegenthese 3 (ausgeliefert, und nichts ändert sich):* Steht er schon mit Blick zum Wirkenden still, dreht RSR ihn
  nicht weg; das kann nur er oder BossModReborn. Bodenzauber drehen zum Ort, nicht zum Ziel, und bleiben hier
  ungeprüft.

- *Umkehrung und Auge zur Laufzeit* (A776; seine Hinweise vom 13. und 14.11.2026: „Bei kefka … wird ein Fragezeichen
  beigepackt. Hinsehen ist da dann wegsehen und umgekehrt. Solche Sonderfälle gibt es auch woanders“; „beim endboss des
  letzten ally raids windurst scheint bmr einige Dinge falsch zu interpretieren“). Einordnung: Hinweise.
  - *Erhebung:* BossModReborn 8ad17f97f, alle 24 Klassen, die einen Blick selbst bauen (`GenericGaze`), dazu die Blätter
    `Lockon` (726 Zeilen) und `Status` (5.851 Zeilen, englisch). Auslöser und Ort des Auges:
    - *Eigene Aktion für die Täuschung:* Kefka in O8S wirkt „Indolent Will“ (#10468, lila Auge, wegsehen) oder „Ave
      Maria“ (#10467, gelbes Auge mit Fragezeichen, hinsehen; consolegameswiki, O8S). Das Fragezeichen ist eine eigene
      Aktion. Bisher galt #10467 als kein Blick: Sein Name war entfernt, weil O8Ns „Ave Maria“ (#10539) umgekehrt ist.
    - *Kopfmarker:* Der Windurst-Endboss (BossModReborn `A35ShinryuParadox`) setzt je Spieler „Look“ (`Lockon` 681,
      `z6r3_b4_lock_lk_7s_c0k2`) oder „No Look“ (680, `…_no_lk_7s…`). Aufgelöst wird beides mit einem Wirken von 7 s:
      in Phase 1 von Shinryu (#49121), in Phase 2 vom Hollow King (#49161). BossModReborn setzt das Auge in beiden
      Phasen auf Shinryu (`PrimaryActor`). In Phase 2 wirkt aber der Hollow King. *Unbelegt, Schluss:* Das ist der
      Fehler, den er sieht. Im ganzen Blatt tragen nur diese zwei Marker „lk“ im Namen.
    - *Status auf dem Spieler:* „Arcane Attraction“ (3385) und „Attraction Reversed“ (3386) bei Althyk und Nymeia
      (Euphrosyne). Der Text sagt es selbst: „Seduced if gazing at Nymeia“ und „… if looking away from Nymeia when this
      effect expires“. Sonst trägt kein Status diese Form.
    - *Ein Mitglied wird zum Auge:* „Glassy-eyed“ (3511, Lapis Manalis: „unleash a petrifying attack in all
      directions“). Wer den Status trägt, wird beim Ablauf zum Auge für alle anderen. „Spell-in-Waiting: Shadoweye“
      (1812, 2456) sagt nur „Designated target of Shadoweye“; Shadoweye trägt weder das Kennzeichen noch den Namen
      eines Blicks, und die Id im Ultimate führt BossModReborn nur im selbst gebauten Blick.
    - *Auge woanders als beim Wirkenden:* BossModReborns `CastGaze` setzt das Auge an den Ort des Wirkens
      (`spell.LocXZ`, aus `CastInfo.TargetLocation`). RSR nahm den Wirkenden. Bei Thordan (#5257, `VFX` 25) liegt das
      Auge am Ort; bei Merciful Moon (#23212, `VFX` 25) an einer erzeugten Kugel; in P8S (#31019, gleichnamig) bei den
      Gorgonen.
    - *Umkehrung, die RSR nicht lesen kann:* Kefkas Statuen im Ultimate (Animation einer Figur), Nier „Distortion“
      (Animation), Owain (Verbindung), „Cursed Shriek“ im Ultimate (echt oder falsch nach einem Status des Bosses mit
      Zusatzwert), Bozja CE34 (das Wirken des Bosses kehrt die Augen um). Ebenso Lala (Ausrichtung aus mehreren Status)
      und das Erosive Eye (Reihenfolge der Wirken). Shadoweye (siehe oben) und „petrifying light upon those nearby“
      („Crown of the Gorgon“, „Authority's Gaze“): Der Text sagt Nähe, nicht Blick, und trennt beides nicht.
  - *Regel:* RSR führt jeden Blick als Auge mit Ort, Richtung (normal oder umgekehrt) und Restzeit. Geprüft an allen
    Zeilen (14.11.2026): Die Statusregel trifft genau 3385 und 3386, die Markerregel genau 680 und 681, die Regel für
    ein Mitglied genau 3511. Die Quellen:
    - Wirken: wie bisher; dazu gilt ein Name, den nur umgekehrte Blicke tragen, als umgekehrt (#10467). Ein Name, den
      beide Arten tragen, zählt nicht. Das Auge liegt am Ort des Wirkens, ohne Ort am Wirkenden.
    - Status auf dem Spieler nach seinem Text: „if gazing at / looking at <Name> when this effect expires“ ist ein Blick,
      „if looking away from <Name> …“ ein umgekehrter. Das Auge ist der nächste Gegner mit diesem englischen Namen,
      die Restzeit die des Status.
    - Kopfmarker auf dem Spieler, deren Effektpfad „_no_lk_“ (wegsehen) oder „_lk_“ (hinsehen) trägt, mit der Dauer
      aus dem Pfad („7s“). Augen sind die wirkenden Gegner mit dem größten Trefferkreis: in Phase 1 Shinryu und sein
      Teil „ShinryuGroin“ (beide 25 y, beide wirken #49121/#49122), in Phase 2 der Hollow King (25 y). Liegen zwei
      Augen in verschiedenen Richtungen, gilt jedes; bei „Look“ bleibt nur eine Richtung, die beide ansieht.
      *Schluss, unbelegt am Mitschnitt:* Die Zeile „gaze: marker …“ nennt die gewählten Augen.
    - Mitglied als Auge nach dem Statustext („petrifying attack in all directions“), ein normaler Blick vom Träger aus,
      solange der Träger nicht der Spieler ist und lebt.
    - *Unsicher:* Ids, die BossModReborn nur in selbst gebauten Blicken führt, zählen nicht als Blick, auch mit
      Kennzeichen oder Namen. Ausgenommen sind Klassen ohne Umkehrung, die das Auge beim Wirken an dessen Ort setzen,
      ohne Einstellung des Moduls (Thordan, Cloud of Darkness „Break IV“). Diese Ids führt `gaze_actions.py` als Blick.
  - *Optionen:* (0) nichts: RSR dreht bei Ave Maria in O8S und bei „Look“ in Windurst in den Tod, wo es angreift, und
    bei Merciful Moon und P8S vom falschen Punkt weg. (1) Nur die unsicheren Ids herausnehmen: Das beseitigt die falsche
    Richtung, schützt aber bei Kefka, Nymeia und Windurst nicht. (2) Die Regel oben. *Gewählt:* (2).
  - *Gegenthese 1 (kein Defekt, BossModReborn regelt es):* Er will Schutz ohne BossModReborn (A773). Und in Windurst,
    Phase 2, irrt BossModReborn nach seinem Modul.
  - *Gegenthese 2 (Option falsch, der größte Wirkende ist nicht das Auge):* Wirkt beim Marker ein größerer Gegner etwas
    anderes, dreht RSR vom falschen Punkt. In Windurst haben neben Shinryu, seinem Teil und dem Hollow King nur Helfer
    (0,5 y), Sphären (1 y), Gulool Ja Ja (5 y) und eine unbenannte Figur (2 y) einen Kreis. *Preis:* in einem anderen Kampf mit Marker eine falsche Richtung für höchstens die Markerdauer. Bisher gibt es nur
    diesen einen Kampf.
  - *Gegenthese 3 (ausgeliefert, und nichts ändert sich):* Kefkas Statuen, Distortion und Owain sieht RSR nicht. Dort
    tut RSR nichts, wie ohne die Regel, und der Spieler oder BossModReborn dreht.
  - *Gegenthese 4 (falsche Zuordnung des Namens):* Ein Gegner mit dem Namen aus dem Statustext muss nicht das Auge
    sein, wenn mehrere so heißen. Gewählt wird der nächste. Bei Nymeia gibt es eine.
  - *Im Kampf:*
    - O8S, Ave Maria: RSR dreht im Vorlauf zur Statue hin und nimmt kein Ziel mehr, das ihn wegdreht.
    - Windurst mit „Look“: Er schaut den Boss an. RSR greift weiter an, denn der Angriff dreht zum Boss.
    - Windurst mit „No Look“: RSR dreht ihn im Vorlauf weg.
    - Euphrosyne: Mit „Attraction Reversed“ hält RSR Ziele, die von Nymeia wegdrehen, und dreht zu ihr.

**Warum die Form so gemessen wird (A731).** Welche Form um den Trefferkreis des Wirkenden weiter reicht, sagt die Art:
BossModReborns Analysewerkzeug beschreibt Art 3, 4 und 5 als „range X+R", Art 2, 12 und 13 als „range X"
(`Replay/Analysis/AbilityInfo.DescribeShape`, 2087153c2, Werkzeugquelle), und seine Module bauen die Formen danach:
Wing Cutter (#331, Art 3, Reichweite 6, Trefferkreis 0,9) als Kegel von 6,9 y und 60° (`D260NorthernBateleur`).
Kegelwinkel und Innenradius stehen im Pfad des Omens (Blatt `Omen`): `gl_fan060_1bf` ist ein Kegel von 60°,
`gl_sircle_2010bf` ein Ring mit den Zahlen 20 und 10. Abgeglichen an allen Aktionen, deren Form BossModReborn im
Kommentar nennt (ffxiv-datamining 52a72b3, 12.11.2026): Bei 945 von 953 Kegeln stimmen Winkel und Reichweite; die acht
übrigen nennen eine Reichweite mit eingerechnetem Trefferkreis oder einen halben Winkel bei einem Omen `er_gl_fan…`.
Beim Ring ist nicht einheitlich, wie die Zahlen gelten: Wo der Pfad dieselbe Außenreichweite nennt wie das Blatt,
stimmen 221 Ringe; wo nicht, passt 32-mal der Innenradius im Verhältnis (Zahl mal Reichweite durch Pfadreichweite) und
10-mal die Zahl selbst, auch beim selben Omen (`gl_sircle_1005bf`: #32841 innen 6, #46903 innen 5). RSR nimmt den
kleineren der beiden: Er zählt mehr Mitglieder als getroffen, der Fehler liegt auf der Seite des Schutzes. Ebenso
zählt der größere Winkel bei `er_gl_fan…`.
- *Im Kampf:* Wer hinter einem Frontalkegel oder im Innenkreis eines Rings steht, gilt nicht mehr als getroffen. Die
  Flächenabwehr, der Selbstschutz (Radiant Aegis, Tengentsu, Third Eye), Surecast und Abtausch (A701) und die Heilung
  vor dem Treffer gehen dort nicht mehr; ein Zauber- oder Kanalbeginn wird für einen solchen Zauber gesperrt, wenn er
  den Spieler sicher erreicht und ein Schritt hinausführt (`CastCertainlyDodgeable`). Wer vor einem Kegel der Art 3, an
  einer Linie der Art 4 oder in einem Kreis der Art 5 steht, gilt jetzt auch im Saum des Trefferkreises als getroffen;
  dort fehlte bisher der Schutz.
- *Gegenthese 1 – Das Omen zeigt nicht die Trefferform:* Die Anzeige und der Treffer des Servers können abweichen.
  Teilweise widerlegt: Der Abgleich mit BossModReborn, dessen Module aus Mitschnitten gebaut sind, stimmt bei fast allen
  Kegeln; beim Ring nimmt RSR den kleineren Innenradius. *Preis:* Wo der Server weiter trifft als das Omen, fehlt der
  Schutz für den Rand; dort bleibt der gelernte Treffer (`HitSeries`, die Flächentabelle) unberührt.
- *Gegenthese 2 – Der Kegel dreht sich mit dem Wirkenden:* Ein Gegner kann sich während des Wirkens drehen. Gefragt wird
  jedes Bild mit der Richtung dieses Bildes, wie bei der Linie.
- *Gegenthese 3 (ausgeliefert, und nichts ändert sich):* Viele Kegel tragen kein Omen (904 der Art 3, 1.242 der Art
  13); sie zählen weiter als Kreis. Ein Ring ohne Zahlen im Omen ebenso.
- Bodenkreise (`TargetArea`): um den Ort, den das Spiel für das Wirken führt (`CastInfo.TargetLocation`, A766).
  Fehlt er, gilt wie bisher der Abstand zum Wirkenden mit beiden Trefferkreisen, also eher zu weit.
- Ein Kegel oder eine Linie gilt nicht schon deshalb als treffend, weil das Mitglied das genannte Ziel ist. Es zählt
  allein die Form in Wirkrichtung (A766). Ein Kreis oder Ring auf ein Ziel trifft sein Ziel wie bisher.

**Wirkrichtung und Ort aus dem Spiel** (A766; TODO-Punkt „Kegel und Linien nach der Wirkrichtung des Spiels
ausrichten“, Anlass BossMod 93b0afd30).
- *Befund:* RSR richtete Kegel und Linien zum Ziel des Zaubers aus und rechnete jedes Bild neu. Ein Ziel, das während
  des Wirkens aus dem Kegel läuft, zog den Kegel mit. Das Omen und der Treffer folgen ihm nicht. Wer das Ziel war,
  galt außerdem immer als getroffen. „Wooly Inspiration“ (#12218, Kegel 60 Yalm) nennt den Spieler als Ziel, trifft aber
  in Wirkrichtung; BossMod führt die Aktion deshalb einzeln als täuschend. Bodenkreise maß RSR vom Wirkenden aus, weil
  der Ort im Blatt fehlt.
- *Quelle:* Der Client führt je Figur die Wirkrichtung (`Character.CastRotation`) und für Bodenzauber den Ort
  (`CastInfo.TargetLocation`; FFXIVClientStructs der Dalamud-Version, gemessen am 13.11.2026). BossMod baut daraus jede
  automatische Form (`WorldStateGameSync`: `Rotation = chr->CastRotation`, `Location` aus dem Paket, sonst
  `TargetLocation`; `AIHintsBuilder`: jede Form in dieser Richtung; awgil 93b0afd30, Werkzeugquelle).
- *Optionen:* (0) nichts. (1) Wie BossMod #12218 einzeln als täuschend führen. (2) Richtung immer aus `CastRotation`,
  Ort des Bodenkreises aus `TargetLocation`, und das genannte Ziel zählt bei Kegel und Linie nicht als Beweis.
- *Abwägung im Kampf:* (0) lässt den Kegel dem fliehenden Ziel folgen. Wer schon draußen steht, gilt als getroffen:
  Selbstschutz und Flächenabwehr gehen umsonst, und die Sperre beim Wirken hält einen Zauber zurück, ohne dass es nötig
  ist. Wer im wahren Kegel steht und nicht Ziel ist, kann als sicher gelten, etwa hinter dem Ziel aus Sicht des
  Wirkenden, wenn das Ziel seitlich ausweicht. (1) hilft nur bei dieser einen Aktion und veraltet mit jedem Patch. (2)
  misst, was das Spiel zeichnet, für jede Aktion. Ein Bodenkreis trifft dann, wer in ihm steht, nicht jeder nahe am
  Wirkenden: Weniger Abwehr geht ins Leere, und wer im Kreis steht, aber weit vom Wirkenden, gilt jetzt als getroffen.
  *Gewählt:* (2).
- *Gegenthese 1 (kein Defekt, ein Kegel auf ein Ziel dreht sich mit):* Ein Wirkender ist beim Wirken festgelegt; das
  Omen zeigt in eine feste Richtung. BossMods automatische Formen, gebaut aus Mitschnitten, rechnen genau so, und
  Module, die einem Ziel folgen, führen das eigens. Wo eine Aktion am Ende doch zum Ziel schnappt, lernt die
  Flächentabelle den Treffer weiter.
- *Gegenthese 2 (falsche Option, ein Ziel im Kegel ist immer getroffen):* Läuft das Ziel hinaus, trifft der Kegel es
  nicht mehr; so war es gemeint, als das Ziel auswich. Steht es darin, liefert die Form dasselbe Urteil wie vorher.
- *Gegenthese 3 (ausgeliefert, und nichts ändert sich):* Ohne Bewegung des Ziels und ohne Bodenkreis zeigen Richtung zum
  Ziel und Wirkrichtung dasselbe. Liefert das Spiel keinen Ort (0), bleibt der Bodenkreis beim alten Maß.
- *Preis:* keiner im Kampf. Beim Lesen kosten Richtung und Ort einen Zugriff auf die Figur je Prüfung.
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
`DefenseTrace.log` (unten); die Zeilen nennen auch, ob der Treffer ihn erreicht.

**Marker:** Es zählen nur Marker auf ihm oder einem Mitglied seiner eigenen Gruppe, Duty-Support-Begleiter
eingeschlossen, gleich wie „Heal and raise party NPCs" steht (A220). Das gilt für die Flagge
(`IsCastingAreaVfx`, eine Upstream-Erkennung, die bis dahin jeden Marker in Reichweite las) und für den
Selbstschutz. Ein Stack auf einem Mitglied einer anderen Allianzgruppe traf keinen von ihnen und öffnete doch
ihre Gruppenminderungen und seinen Selbstschutz. Für den Selbstschutz zählt ein Stack-Marker in der eigenen
Gruppe als „erreicht ihn" (wer stackt, wird getroffen); ein Spread-Marker zählt nur auf ihm selbst. Deshalb gehört
jeder Markerpfad genau einer Art an – Tankbuster, Stack oder Spread –, sonst gilt er als die zuerst gefragte:
Upstream führte im M9 den Spread von Aetherletting (`x6r9_loc01_t0a1`, Lockon 655) auch als Stack, und ein Spread
auf einem anderen Mitglied öffnete seinen Selbstschutz (A394). Die Arten sind am Lockon-Blatt und an den Icon-Namen
in BossMod belegt; `check_marker_lists.py` hält sie in der CI getrennt.

**BossModReborn-Raidwide (A273):** Was BossModReborn „Raidwide" nennt, trifft nicht jeden — derselbe Typ steht für
Spreads, Türme, Köder und Sturmangriffe auf einzelne Spieler (Konzept 08, „Wann eine BossModReborn-Vorhersage ihn
betrifft").
Ist der nächste vorhergesagte Treffer dieser Raidwide, nennt seine Maske die Getroffenen: Erreicht er weder ihn
noch mehr als ein Gruppenmitglied, öffnet er nichts; erreicht er die Gruppe, aber nicht ihn, öffnet er die
Gruppenminderung, nicht den Selbstschutz. Gehört die Maske zu einem anderen Ereignis oder kommt die Vorhersage aus
der Zeitleiste, gilt er wie bisher als Treffer auf jeden.

**Tankbuster und die Einzelabwehr der Schadensausteiler (A220):** Ein angekündigter BossModReborn-Tankbuster
öffnet sie, wenn er ihn trifft. Ist der nächste vorhergesagte Treffer dieser Tankbuster, sagt BossModReborn das
selbst: Die Maske des Eintrags nennt die Getroffenen, Bit 0 ist immer der Spieler (`PartyState.PlayerSlot = 0`,
IPC `Hints.PredictedDamagePlayers`; die Liste ist nach Zeitpunkt sortiert, BossModReborn-Quelle 29.09.2026).
Gehört die Maske zu einem anderen Ereignis, gilt die bisherige Näherung: kein lebender Tank in der Gruppe. Allein
diese Näherung öffnete Radiant Aegis für einen Tankbuster auf einem Tank, den RSR nicht als Gruppenmitglied
zählte — ein Duty-Support-Tank bei ausgeschaltetem „Heal and raise party NPCs". Ob RSR Duty-Support-Tanks über
ihren `ClassJob` überhaupt als Tank erkennt, ist nicht belegt. WrathCombo liest den Job solcher NPCs aus
`InfoProxyPartyMember` (FFXIVClientStructs) statt aus `ClassJob`; das ist ein Hinweis, keine Spielquelle. Mit der Maske hängt die
Entscheidung daran nicht mehr.

**Selbstschutz in jedem Abwehrpfad, zentral (A233):** Eine Aktion, die nur den Spieler schützt, fällt in der
Flächen- und in der Einzelabwehr nur, wenn der Treffer, der den Pfad geöffnet hat, ihn erreicht. „Nur den Spieler"
liest `BaseAction.CanUse` aus Spieldaten und Zielwahl: Das aufgelöste Ziel ist er selbst, und die Aktion hat keinen
Wirkradius. Darunter fallen Schutzwall, Verdammnis/Rachsucht (Damnation/Vengeance), Urimpuls/Urinstinkt (Bloodwhetting/Raw Intuition) auf sich, Abtausch,
Sentinel/Guardian, Bulwark, Shadow Wall, Nebula, Camouflage, Holmgang und The Blackest Night oder Heart of
Corundum mit Ziel „Self". Frei bleiben, was andere oder den Gegner trifft: Reflexion (Wirkradius 5), Abschütteln,
Divine Veil, Passage of Arms, Dark Missionary, Heart of Light, Intervention, Heart of Corundum auf das
Tankbuster-Ziel, Nascent Flash. Ein Befehl des Spielers (Makro „Defense") wird nicht geprüft.

- *Flächenpfad:* „erreicht ihn" ist `AreaHitReachesPlayer`. Die Rotationen fragen es nicht ein zweites Mal: Eine
  eigene Abfrage in `SAM_Reborn` und `SMN_Reborn` galt auch unter seinem Befehl und ließ Tengentsu, Third Eye und
  Radiant Aegis dort aus (entfernt, A271). Außerhalb des Abwehrpfads fragt `SMN_Reborn.RadiantAegisDueBeforeDemi`
  selbst, weil dort keine zentrale Prüfung läuft.
- *Einzelpfad:* „erreicht ihn" ist `SingleHitReachesPlayer`: ein Tankbuster-Marker auf ihm oder ein gelisteter
  Tankbuster auf ihn; beim Tank zusätzlich ein Cast, den ein Gegner auf den Spieler als sein Ziel wirkt, und die
  Pull-Regel (mindestens `AutoDefenseNumber` Gegner bis 3 y auf ihm, die ihn angreifen, Gesundheit unter
  `HealthForAutoDefense`); ein angekündigter BossModReborn-Tankbuster, wenn seine Maske ihn trifft. Ist die Maske
  unbekannt, zählt sie beim Tank als Treffer — die Wahrscheinlichkeit ist dann nicht gemessen, und Sicherheit geht
  vor —, bei allen anderen nur ohne lebenden Tank (wie A220).
  Beim Nicht-Tank zusätzlich ein gemessener Zauber, den ein Gegner gezielt auf ihn wirkt und der Minderung lohnt
  (A540, Absatz „Auf einen Nicht-Tank gezielt").
- *Warum nicht die Flagge sperren:* Die Einzelabwehr eines Tanks trägt auch die Hilfe am anderen Tank —
  Intervention, Urflackern, The Blackest Night und Herz des Korunds auf das Ziel eines Tankbusters (Konzept 09,
  „Hilfe für das Ziel eines Tankbusters", A328), Reflexion auf den Gegner. Ein Flag wird an den Pfaden gemessen, die
  es öffnet; gesperrt wird deshalb die Selbstschutz-Aktion, nicht der Pfad.

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

**Eine Landung, die niemanden aus der Gruppe traf, trägt keinen Treffer** (A272). Zehn gelistete Aktionen landen
auf dem Wirkenden selbst (`CastType` 1, `EffectRange` 0; „1 targets, 0 party members reached", bis A366 „damaged" geschrieben, gezählt wird jeder erreichte). Bei Head-splitting
Roar (43578) trifft eine halbe Sekunde später die gleichnamige 43579 die Gruppe mit 1–22 %; bei den übrigen neun
trifft niemanden etwas. Eine solche Landung steht deshalb zunächst als „verfehlt"; folgt vor der nächsten Landung
derselben Aktion und vor Kampfende eine gleichnamige Aktion mit Treffer auf die Gruppe, gilt sie als ihre Landung:
Erreicht sie ihn, steht „erreicht", und ihr Anteil misst die gelistete Aktion (`Watcher.RecordFollowUpOfEmptyLanding`,
Protokollzeile „follow-up of"). Im Kampf: Der Folgetreffer bekommt wieder die Flächenabwehr, und der Cast wird
bewertet; ein Wind-up ohne Folgetreffer bleibt ausgelassen, wie bisher. Verknüpft wird über den Namen, nicht über
den Wirkenden, weil der Folgetreffer von einer anderen Figur kommt; eine Frist gibt es nicht, die nächste Landung
und das Kampfende begrenzen die Zuordnung. Grenze: Ein Folgetreffer unter anderem Namen bleibt unverknüpft.

**Als „trifft niemanden" gespeichert wird eine leere Landung nicht** (A275; sein Hinweis vom 13.10.2026, die
Tabelle decke solche Flächen ab). Die Tabelle führt nur Anteile über null, weil ein Treffer ohne Schaden sagt, dass
etwas ihn aufhielt, nicht, dass er harmlos ist. Für die neun Wind-ups ohne Folgetreffer lässt „Skip area defence for
casts that missed you" seine Flächenabwehr ab der zweiten Landung je Sitzung ohnehin weg. Gespeichert gewönne das
eine Abwehr je Sitzung und Aktion; es kostete dauerhaft die Abwehr dort, wo der Folgetreffer unter anderem Namen
kommt und unverknüpft bleibt.

**Protokoll der Abwehrentscheidungen (`DefenseTrace.log` im Konfigurationsordner, über Sitzungen und Builds fortgeschrieben, jede Sitzung mit Datum, Uhrzeit und Commit des Builds, A208, A232):** Jede
Wahl der Abwehrkette — Flächen- und Einzelabwehr im Dispatch für alle Jobs, beim Beschwörer auch Radiant Aegis vor
einem BossModReborn-Raidwide — mit allen Quellen, die in diesem Moment stehen: Marker mit Pfad, Träger und Abstand,
gelistete Casts mit Form, Abständen, „reaches you" und dem Ergebnis ihrer letzten Landung, beim Tank auch
ungelistete Casts eines Gegners auf sein Ziel, die Pull-Regel, BossModReborn-Raidwide und -Tankbuster samt erkanntem
Tank, und je Pfad, ob der Treffer ihn erreicht („area hit reaches you", „single hit reaches you", A233). Kam ein offener
Pfad leer heraus – jede gefragte Aktion abgelehnt oder keine da –, steht „area defence open, nothing chosen" bzw.
„single defence open, nothing chosen" mit denselben Quellen, höchstens einmal je GCD (A265): Sonst sähen ein Pfad,
der nie öffnete, und einer, der offen nichts fand, in der Datei gleich aus. Daneben
jeder gegnerische Treffer auf ihn (ohne Auto-Attacken) und jede Landung eines gelisteten Flächencasts mit
„reached you" — ohne diese Zeile blieb offen, warum „Ätherschub" am 30.09. sechsmal die Flächenabwehr öffnete,
obwohl er ihn nie traf: Eine Landung, aus der „Skip area defence for casts that missed you" lernt, kam nicht an. So steht in der Datei, ob der Treffer, für den die
Abwehr fiel, ankam. Seit A255 bis A257 kommen die Zeilen des Tankwechsels dazu: jeder erkannte Tankbuster auf ihn
(„tankbuster on you", mit der Verwundbarkeit, die er legte) und nach einem Wechsel auf den Halter, warum ein
Wechsel oder das Zurückholen wartet, die Wahl, ihr Ausgang und ein Geteiltes Leid eines Gruppenmitglieds auf ihn
(Konzept 09). Aufgelöst wird das Protokoll, sobald eine Datei seiner Kämpfe die Quelle zeigt und sie behoben ist.
Jeder Gebietswechsel, Kampfbeginn und jedes Kampfende stehen darin, unabhängig von den übrigen Pfaden (A417). Ein
gescheiterter Schreibversuch öffnet die Datei neu, nennt Fehlerart und Meldung in ihr („trace reopened: a write failed
…") und schreibt die Zeile nach; ein Zeichen, das sich nicht kodieren lässt, steht als „�". Scheitert auch das
Neuöffnen, bleibt die Datei bis zum nächsten Gebietswechsel zu. Vorher machte ein einziger gescheiterter Schreibversuch
die Datei für die ganze Sitzung stumm – am 26.10.2026 zwei Kämpfe ohne eine Zeile (A418).

## Wen die Unterdrückung erreicht

Erhoben am Code (12.10.2026): `AreaCastIsWorthMitigating` sitzt in `IsHostileCastingArea`, und über
`IsHostileCastingAOE` hat diese Frage **zwei** Leser, die etwas bewirken, und zwei Anzeigen.

| Leser | Wirkung der Unterdrückung |
|---|---|
| `StateUpdater` → `AutoStatus.DefenseArea` (hinter `UseAoeDefense`, über `IsHostileCastingAOEForMyDefense`) | die gemeinte: keine Gruppenminderung auf eine Bagatelle |
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

**Die Heilzielwahl fragt deshalb die unbewertete Erkennung.** Gefährdungsklasse 3 ordnet nach absoluten
effektiven Punkten, solange `DataCenter.AreaHitAnnounced` steht: Flächenmarker auf der Gruppe, ein gelisteter
Cast, ein großer gemessener auch unterbrechbar, ein BossModReborn-Raidwide im Minderungsfenster (Konzept 07,
A271). Mit `IsHostileCastingAOE` sah sie weder den unterbrechbaren großen Cast noch den nur von BossModReborn
angekündigten Raidwide; vor einem großen Rundumschlag, den niemand unterbricht, heilte der Heiler zuerst den
Tank mit dem niedrigsten Prozentwert statt des Schadensausteilers mit den wenigsten Punkten.

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
Zeitfenster liest das als „es kommt nichts". Die Diagnoseseite „BMR Data" nennt das aktive Modul und je
Ereignisart, ob überhaupt eine Vorhersage vorliegt. Die vollständige Erhebung,
welche Regeln des Baums so hängen, steht in `08-mitigation-synergy.md`.

**Die Antwort ist ein zweiter, eigener Weg** (`DataCenter.IsHostileCastingLargeArea`, hinter
`Mitigate a big area cast even when it is interruptible`, **Vorgabewert an**): Er fragt nicht nach
Unterbrechbarkeit und nicht nach Mindestlänge, sondern nach dem **gemessenen** Anteil — mindestens
das, was die größte Barriere des Spiels absorbiert — und nach dem letzten GCD vor dem Einschlag (Restzeit
bis ein GCD; der Vorfilter fragt eins bis zwei GCDs vorher). Ein Unterbrechen hat bis dahin stattgefunden
oder findet nicht mehr statt.

**Warum das kein Rückbau der Entscheidung aus A9 ist.** A9 hat auf seine Meldung hin einen Rückfall
entfernt, der die Verteidigung aus der **Gegnerzahl** heraus hob: kein Gefahrenbeleg, und er stand
dauernd an. Dieser Weg verlangt das Gegenteil — eine Zahl, die aus einem tatsächlichen Einschlag
stammt. Eine Aktion, von der noch niemand getroffen wurde, trägt keine Zahl und öffnet nichts.

**Und er war damals nicht baubar:** Den Anteil je Aktion gab es nicht, als der Rückfall entfernt
wurde. Die Grobheit musste deshalb der Vorfilter allein tragen. Das ist der Grund, warum dieselbe
Frage heute anders zu beantworten ist als im September.

**Das Zeitfenster des Vorfilters ist nicht der Engpass, gemessen an seinen Protokollen (A283).** Ein gelisteter
Cast von höchstens einem GCD Gesamtzeit öffnet über diesen Weg nie. Ausgezählt wurden die Flächentreffer auf ihn
ab 5 % mit mindestens zwei Zielen und ob in den zehn Sekunden davor eine Flächenabwehr-Entscheidung stand: im
Protokoll 04.–10.10.2026 285 Treffer, 181 ohne Entscheidung — 3 davon gelistet mit kurzer Wirkzeit, 42 ohne
Wirkzeit (keinem Cast vorhersehbar, nur BossModReborn oder ein Marker kündigt sie an), 41 nicht auf der
ausgelieferten Liste, 62 gelistet mit langer Wirkzeit, aus älteren Builds, deren Ursachen A272 und A277 behoben
haben (leere Landungen, Helfer); im Protokoll vom 13.10.2026 30 Treffer, 14 ohne Entscheidung, davon 10
Meteoreinschlag von Helfern (A277), kein kurzer Cast. Das Fenster zu weiten träfe alle Jobs und die Notfallheilung
über `IsUnderThreat` für keinen gemessenen Fall; es bleibt. Ebenso bleibt die Tankbuster-Einschränkung für
Magier aus A9/C10, seiner Meldung, sie feuere zu oft.

**Getrennt gehalten statt gelockert, und das ist Absicht:** `IsHostileCastingAOE` speist auch
`ObjectHelper.IsUnderThreat` und darüber die Notfallheilung des Weißmagiers. Ein Lockern dort
verschöbe zwei Pfade auf einmal. Der neue Weg hängt allein an der Verteidigungsflagge.

## Was die Bewertung im Kampf ändert

| Lage | ohne Bewertung | mit Bewertung |
|---|---|---|
| Gelernte Fläche, noch nie gemessen | volle Gruppenminderung | **unverändert** |
| Gemessene Bagatelle (2 %), Gruppe gesund | volle Gruppenminderung, Abklingzeit weg | keine Minderung, Abklingzeit bleibt |
| Dieselbe Bagatelle, ein Mitglied knapp über der Heilschwelle | volle Gruppenminderung | volle Gruppenminderung |
| Gemessene Fläche ab 25 %, Gruppe gesund | volle Gruppenminderung | **unverändert** — die Obergrenze entscheidet ohne Blick auf die Gesundheit |
| Gemessene Fläche zwischen 10 % und 25 %, Gruppe gesund | volle Gruppenminderung | keine Minderung, solange der Treffer niemanden unter die Heilschwelle drückt |
| Erste Landung unter drei Minderungen gemessen (A419: 25 %) | – | zurückgerechnet, rund 34–36 %: der nächste Cast gilt als groß |
| Gemessene Fläche von 30 %, ein anderer Spieler hat schon 20 % Minderung gelegt, Gruppe gesund | volle Gruppenminderung | erwartet rund 24 %: keine weitere, solange der Treffer niemanden unter die Heilschwelle drückt |
| Savage-Training, zweiter Versuch | jede Fläche gleich behandelt | die kleinen kosten nichts mehr |
| Nach dem Kampf, Blick in die Listenverwaltung | nichts zu sehen | je Aktion der gemessene Anteil, und welche davon eine Minderung gespart hat |

**Die Zeile zum Savage-Training ist der eigentliche Gewinn**, und sie stammt aus einer Vorgabe des Auftraggebers:
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

**Eine Serie zählt als ein Angriff** (A349, A480, `Watcher.SeriesShare`). Landet eine gelistete Aktion mehrmals, folgt
jede Landung der vorigen also schneller als ihre Wirkzeit laut Aktionsblatt, werden die Anteile je Mitglied addiert und der
höchste Summenanteil abgelegt, höchstens die ganze Maximalgesundheit. Ersticken des Fleisches (6 s Wirkzeit) traf ihn am
19.10.2026 zweimal je viermal im Abstand von 3 s, mit bis zu 24 % je Treffer und zusammen 49 und 54 %: einzeln ein
Treffer, der ihn über der Flächenheilschwelle lässt, zusammen nicht. Der Text von „Skip mitigation for small area casts"
mindert voll, wenn nach dem Treffer Heilung fällig wird; mit dem Einzeltreffer hätte die Regel die Minderung gegen die
ganze Serie gespart.

**Gleich welcher Gegner wirkt.** Die Serie gilt dem, was die Gruppe nacheinander trifft, nicht dem Wirkenden. Die Leser
der Tabelle bewerten je angekündigtem Cast nur diesen einen (`DataCenter.AnnouncedAreaShare`); ohne die Summe sähen sie
beim zweiten und dritten Wirken jeweils einen Treffer. Zwei Arten kommen in seinen Mitschnitten vor (25.10. bis
01.11.2026, alle gemessenen Flächenaktionen nachgerechnet, `area_series_model.py`):
- *Mechanikobjekte,* nie angegriffen, je ein Wirken: Ersticken des Fleisches (vier Türme in 9 s), Chemiebombe (vier in
  9 s), Explosion (drei in 5 s), Verfluchter Blick (acht Verfluchte Augäpfel, nicht anvisierbar, gemeinsam erschienen und
  entfernt, alle 3 s ein Blick, 21 s); dazu gleichzeitige Gruppen (Großer Meteorit, Dunkler Regen, Kritzelei). Eine
  Mechanik; ihre Summe ist ihr Schaden.
- *Ein Rudel angreifbarer Gegner:* Schockwelle (drei Gegner in 2,9 s), Augen auf mich (zwei in 3,6 s), Schlangenpeitsche
  (zwei in 2,0 s). Die Treffer kommen, bevor eine Heilung dazwischen landet. Wirkt ein Gegner allein, bewertet die Summe
  ihn zu hoch – Fehlerrichtung sicher.

Die 38 % von Verfluchter Blick sind zwei Blicke zu je 18 bis 19 % auf ein Mitglied, das sich nicht abwandte; die übrigen
66 von 78 Zielen traf er nicht. Das ist der verfehlte, aber überlebte Treffer („Ein verfehlter, aber überlebbarer Treffer
bewertet den Cast zu hoch", unten), keine falsche Verkettung (C142).

**Ein neues Wirken desselben angreifbaren Gegners beginnt eine neue Serie** (A480). Landet ein anvisierbarer Gegner, der
in der laufenden Serie schon traf, wieder, und liegt seine eigene vorige Landung darin mindestens eine Wirkzeit zurück, hat
er neu gewirkt: Die Runde des Rudels ist vorbei, die Landung beginnt eine neue Serie. Liegt sie kürzer zurück, ist es ein
weiterer Treffer desselben Wirkens und bleibt in der Serie.
- *Warum:* Ohne diese Grenze läuft die Serie in einem großen Pull weiter, solange irgendein Gegner die Lücke füllt, und die
  Summe wächst mit der Dauer des Pulls statt mit dem Angriff. Abgelegt wird der höchste Wert, der Fehler bliebe also.
  Nachgerechnet über 200 Pulls zu 45 s (`area_series_model.py`): Drei Gegner, die alle 8 s dieselbe Fläche zu 8 % wirken,
  stünden mit 64 % statt 24 %, vier bis sechs mit 100 %; bei 12 s ab fünf Gegnern 80 % statt 40 %. Jedes Wirken des
  Rudels zählte dann als fast tödlich: Minderung und Vorausheilung bei jedem, „Skip mitigation for small area casts" spart
  nie, und die mitgelernte Dauer der Serie lässt keine Minderung mehr zählen.
- *Mechanikobjekte bleiben ausgenommen:* Ein Helfer, der in derselben Mechanik zweimal wirkt, teilte sie sonst, und nur ein
  Teil der Türme wäre bewertet – die unsichere Richtung. Gemessen wird `IsTargetable` des Wirkenden bei der Landung; ein
  Boss, der vor ihr unanvisierbar wird, zählt als Mechanikobjekt, und die Serie läuft wie bisher.
- *In seinen Mitschnitten* (sechs Abende, 160 Serien) landet kein Gegner zweimal in derselben Serie; die Regel ändert dort
  nichts. Was sie teilt, schreibt eine Zeile ins `DefenseTrace.log`.
- *Fehlerrichtung zu kurz:* Ein anvisierbarer Gegner, dessen eines Wirken dieselbe Aktions-Id in größerem Abstand als
  seiner Wirkzeit treffen lässt, während dazwischen andere landen, würde geteilt; kommt in seinen Mitschnitten nicht vor.
- *Bestand:* Die Serie wird nicht gespeichert, nur ihre Summe; ein schon aufgelaufener Wert lässt sich nicht nachträglich
  zerlegen. Sein letzter Upload zeigt keinen: Die gelernten Serienspannen (sechs, seit A460 geführt) liegen außer bei
  Verfluchter Blick bei höchstens 1 s, und die längste Serie angreifbarer Gegner in seinen Mitschnitten sind drei Landungen
  in 2,9 s. Einen älteren, unerkannten senkt erst die Revision über einen Spielpatch.
- *Verworfen:*
  - Je Wirkendem trennen und nur gleichzeitige Landungen zusammennehmen: zerlegte Ersticken des Fleisches, Chemiebombe
    und Verfluchter Blick in Einzeltreffer, und die Minderung fehlte gegen die ganze Mechanik.
  - Die Serie auf eine Wirkzeit ab ihrer ersten Landung begrenzen: teilte Ersticken des Fleisches (9 s bei 5,7 s).
  - Eine Höchstzahl von Landungen: eine erfundene Zahl.
- *Gegenhypothesen:*
  - Kein Defekt: In seinen Mitschnitten läuft keine Serie auf. Der Höchstwert hält aber einen einzigen Lauf fest, und große
    Pulls gleicher Gegner gibt es in jedem Dungeon (Modell oben).
  - Ausgeliefert, und nichts ändert sich – warum? Weil die Lage in seinen Inhalten bisher fehlt (null von 160 Serien), und
    weil ein schon aufgelaufener Wert bleibt (Bestand).

**Gemindert ist eine Serie nur so weit, wie die Minderung bis zu ihrem letzten Treffer steht** (A460). Die Flächenserie
lernt wie die Tankbuster-Serie (A454) ihre Dauer vom ersten bis zum letzten Treffer, als höchsten Wert je Aktion
(`OtherConfiguration.AreaSeriesSpan`), und `DataCenter.AreaShareNow` zählt nur Minderung und Erhöhung der
Maximalgesundheit, die über die Restzeit des Casts und diese Dauer hinaus stehen. Für alle Leser der Tabelle zugleich:
die Größenregel, die Vorausheilung, den großen unterbrechbaren Cast und den gehaltenen BossModReborn-Raidwide.
- *Warum:* Ersticken des Fleisches – vier Türme aus je eigenem 5,7-s-Cast eines Helfers, 3 s versetzt – trifft bis
  9 s nach dem ersten Turm (seine Protokolle vom 25. und 29.10.2026). Eine Gruppenminderung von 10 s, 3 s vor dem
  Ende des ersten Balkens gelegt, endet vor dem dritten Turm; die Größenregel rechnete sie für die ganze Summe, konnte
  die eigene Minderung auslassen, und die letzten Türme kamen ungemindert.
- *Preis:* Eine Minderung, die nur die ersten Treffer deckt, zählt gar nicht – gerechnet wird, als träfe die ganze
  Summe ungemindert. RSR mindert dann eher einmal zu viel; nach seiner Spielweise der sichere Fehler, weil die
  Wahrscheinlichkeit des Treffers bekannt ist (er ist angekündigt). Eine Mechanik, die länger trifft, als eine Minderung
  steht – Verfluchter Blick über 21 s –, zählt ungemindert; die Serie eines Rudels endet mit seiner Runde (oben).
- *Ausgeschlossen:* den schon gelandeten Teil einer offenen Serie von der Summe abziehen, wenn ihr nächster Cast
  angekündigt wird (sein Protokoll vom 29.10.2026: vor jedem Turm „hit 99 %"). Läuft die Serie heute länger als die
  gespeicherte, rechnete der Abzug den nächsten Cast zu klein; und die Vorausheilung nach dem ersten Turm ist meist
  ohnehin begründet – drei Türme zu 19 % stehen gegen ein Mitglied bei 80 %. Ebenso ausgeschlossen: die Minderung je
  Treffer zeitlich aufzuteilen; die Treffer einer Serie kennt die Tabelle nicht einzeln.

**Fortgeschrieben wird als Höchstwert über alle Durchläufe.** Der Wert kann nur steigen, und jeder
Anstieg wirkt beim nächsten Verbrauch sofort.

**Gemessen wird nur ein Treffer, den das Mitglied überleben kann; abgelegt wird höchstens seine ganze
Maximalgesundheit** (A298, `Watcher.TryAreaShare`). Ein Treffer, der die ganze Gesundheit samt der Barriere
erreicht, die das Mitglied beim Treffer trägt, tötet aus voller Gesundheit, was immer Heilung und Minderung tun.
Er sagt nicht, wie hart der Cast die Gruppe trifft, sondern dass dieses Mitglied die Mechanik verfehlt hat.
Belege aus seinen Protokollen: Megatod (#6090, Forgall) traf ihn ohne Debuff mit 941 % der Maximalgesundheit und
mit Debuff 41 Sekunden später mit 10 % (DefenseTrace cf840808); Nekrotische Gefahr (#40646, Forschungsleiter
Kanilokka, Schaden fällt zum Rand hin ab) traf ihn am Rand mit 6 %, gespeichert waren 588 %. Sein Hinweis
(16.10.2026): „megatod tritt mit tot ein, wenn man nicht einen debuff auf sich hat, der durch betreten einer
schadensfläche entsteht. heilung dagegen ist unmöglich." Gespeichert hielt der eine Fehlschlag den Wert für immer,
und jeder Leser nahm den Cast für einen tödlichen Raidwide: Vorausheilung, sobald jemandem Gesundheit fehlte,
weichende Rückhaltungen, Minderung, und der Tankbuster-Plan zählte die Gruppe als tot. Ein solcher Treffer zählt
weiter für die Aufnahme in die Liste und für „hat dich erreicht", nicht für den Anteil; `DefenseTrace.log` nennt
ihn („hit beyond max HP and barrier … not measured"). Ein Treffer, den das Mitglied nur hinter seiner Barriere
überlebt, wird als ganze Maximalgesundheit abgelegt: Er nimmt ihm alles, was Heilung herstellen kann, und weil
kein abgelegter Wert darüber liegt, erkennt das Laden einen älteren.

- *Barriere:* gelesen, wenn der Effekt eintrifft. Das ist das Einrasten der Aktion; angerechnet wird der Schaden
  danach (cactbot LogGuide, Zeile 37). Eine Barriere zählt, wenn sie in diesem Moment steht, eine danach gelegte
  nicht (Community-Anleitungen, keine Spielquelle). Der Betrag des Spiels enthält den von ihr aufgefangenen Teil:
  In seinen ACT-Mitschnitten vom 26.10.2026 tragen vier Treffer, die ganz in einer Barriere aufgingen, ihren vollen
  Betrag, während die LP des Getroffenen gleich blieben und die Barriere um ihn fiel (Zeile 21 gegen Zeile 37 derselben
  Sequenz; dazu teilweise aufgefangene mit einem Betrag über dem LP-Verlust, A449). Die Grenze ist damit genau.
- *Ein Cast, den niemand überlebt,* bleibt unbewertet, und unbewertet heißt mindern wie vor der Tabelle. Eine
  Kennzeichnung „Mechanik, nicht mindern" ist verworfen: Ein Raidwide, der alle nur mangels Minderung tötete,
  würde danach nicht mehr gemindert, tötete im nächsten Versuch wieder und würde nie gemessen.
- *Ältere Werte:* Was über der ganzen Maximalgesundheit steht, lässt das Laden aus, und das Speichern holt es
  nicht aus der Datei zurück (`OtherConfiguration.IsWithinWholeHealth`) – sonst höbe es jede neue Messung derselben
  Aktion wieder an. In seiner Datei vom 16.10.2026 betrifft das Megatod, Nekrotische Gefahr, Bestialisches
  Brüllen (#40610, 169 %) und Voller Schwinger (#7378, 109 %). Bis zu ihrer nächsten Landung sind sie unbewertet.
  War Voller Schwinger ein Raidwide, den jemand hinter seiner Barriere überlebte, steht er danach wieder bei der
  ganzen Maximalgesundheit; war es ein Fehlschlag, beim Treffer der Überlebenden.
- *Die Tankbuster-Tabelle* hat diese Grenze bewusst nicht. Dort ist ein Treffer über der Maximalgesundheit die
  Auskunft, die der Plan braucht – ungemindert tödlich, also Minderung oder Unverwundbarkeit –, und abgelegt wird
  ohnehin der ungeminderte Wert.

**Ein Schadenseintrag ohne Betrag wird nicht gemessen.** Der Lernpfad (`Watcher`) zählt einen Treffer für die
Aufnahme, wenn `FullAmount(damageEffect) > 0 || (damageEffect.param0 & 6) == 6`; die zweite Hälfte zählt einen
Treffer, bei dem kein Betrag ankam. Gemessen wird nur, wo ein Betrag ankam. *Die genaue Bedeutung dieses Flags ist hier
nicht belegbar* und wird nicht behauptet — sicher ist nur, dass `value` dann nicht der Einschlag ist. Ein Treffer, den
eine Barriere ganz auffängt, ist kein solcher Fall: Er trägt seinen vollen Betrag (Absatz „Gemessen wird nur ein
Treffer, den das Mitglied überleben kann", Punkt „Barriere", A449) und wird gemessen wie jeder andere. Für die
**Aufnahme** zählt ein Eintrag ohne Betrag weiter, wie Upstream es führt: mit dem Flag. Ein Mitglied unter
Unverwundbarkeit trägt gar keinen Schadenseintrag, sondern die Effektart „unverwundbar"; es zählt für die Aufnahme als
erreicht, ebenso Ausweichen und Widerstand (`Watcher.ReachedTarget`, A452). Bis dahin kam ein Raidwide, der landete,
während ein Tank unter Hallowed Ground oder Superbolide stand, bei dieser Landung nicht in die Liste – ein Tankbuster,
den ein Patch zur Fläche machte, bekam dann beim nächsten Cast noch einmal den Tankbuster-Plan (sein zweiter Prüffall).

**Null ohne Barriere ist kein Schaden** (A366). Trägt jedes erreichte Mitglied einen Schadenseintrag von null und keines
eine Barriere (`GetObjectShield`), hat nichts den Treffer geschluckt: Die Aktion macht keinen Schaden. Eine gelistete,
noch unbewertete Aktion bekommt dann den Wert 0 – ab zwei erreichten Mitgliedern, damit ein Einzelner nicht für die
Gruppe spricht. Ungeschützt geschluckt wird nichts; Unverwundbarkeit, Ausweichen und Widerstand kommen als eigene
Effektarten, nicht als Schadenseintrag. Auch eine Barriere macht aus einem Treffer keine Null (A449); die Bedingung
„keine Barriere" lässt eine solche Aktion nur unbewertet, also gemindert, wo eine stand – die sichere Richtung. Anlass: Impakt (#36794, Eliminator in Alexandria) traf am 25.10.2026 alle vier
mit null und blieb unbewertet; „unbewertet heißt mindern" gab bei jedem Cast Linderung aus. Mit 0 ist es ein kleiner
Cast, den „Skip mitigation for small area casts" auslässt; trifft er später mit Schaden, hebt die Höchstwertregel den
Wert wie jeden anderen. Die Leser unterscheiden seitdem „nicht bewertet" (kein Eintrag) von 0.

**Aufnahme und Fortschreibung haben verschiedene Bedingungen.** Aufgenommen wird nur, wenn **jedes**
Gruppenmitglied im selben Effektsatz getroffen wurde — richtig, denn das ist der Grund, warum die
Liste Raidwides führt und keine örtlichen Flächen. Für die **Fortschreibung** einer bereits
anerkannten Id ist dieselbe Bedingung falsch: Ein Raidwide, bei dem ein Mitglied tot, unverwundbar
oder außer Reichweite war, träfe sie nicht und würde die Messung verwerfen — ausgerechnet in den
harten Kämpfen. Dort genügt ein beobachteter Treffer auf ein Gruppenmitglied. Ebenso greift die
Fortschreibung bei **bekannten** Ids: `HashSet.Add` fasst eine vorhandene Id nicht mehr an, und ohne
diesen Zusatz bekäme kein Alteintrag je ein Potential — die ganze hybride Form wäre für die 850
vorhandenen Einträge wirkungslos.

**Raidwide oder ausweichbare Fläche: Die Aufnahme trennt nicht, die Anwendung trennt (A750).** Aufgenommen wird, was
die ganze Gruppe getroffen hat. Dazu gehören neben Raidwides auch Stack, Turm, Streuung, Rückstoß über die ganze Fläche
und Schaden mit Abfall zum Rand, denn diese Treffer erreichen alle, und eine Gruppenminderung hilft dort wie beim
Raidwide. Ob eine gelistete Fläche im laufenden Kampf jemanden erreicht und ob ein Schritt hinausführt, entscheidet
jeder Leser erst im Moment des Drückens, an der Form und den Positionen:
- Die Flächenflagge und der Selbstschutz prüfen die Reichweite erst im letzten GCD vor dem Treffer (`IsHostileCastingBase`,
  `AreaCastReachesParty`, `AreaCastCanReachPlayer`). Wer bis dahin hinausgegangen ist, bekommt keine Minderung mehr.
- Die Sperre vor einem Zauberbeginn (`CertainDodgeableWithin`) und die Sperre vor Limit Break, langem Kanal und KI-Weg
  (`FirstDodgeableWithin`, Konzept 03, „Ausweichen in der Sperre") lassen eine gelistete Fläche als ausweichbar gelten,
  wenn ihre gemessene Form den Spieler erreicht und ein lebendes Mitglied draußen lässt (`CastCertainlyDodgeable`). Ein
  Raidwide erreicht alle und bleibt unausweichlich.

*Gemessen an seinen 63 aufgenommenen Ids (Upload vom 13.11.2026):* BossModReborn (7550528, 13.11.2026) führt 58 davon
in einem Modul (Werkzeugquelle, aus Mitschnitten gebaut); die Einordnung steht in dessen Kommentaren und Komponenten. Danach sind es Raidwides, Schaden mit Abfall,
Rückstöße, Stacks (Reichweite 6 auf einem Spieler), Türme (4 und 8) und ein Köder-Treffer
(#49966, im Blatt Kreis 40 um das Ziel). Ausweichbar sind nach den Modulen nur der Kreis „the
Dragon's Voice" (#3344, `SimpleAOEs` 30) und zwei Blicke in einer FATE (#47148, #47152, `CastGaze`).

*Verworfen, ein Merkmal aus den Spieldaten bei der Aufnahme (`CastType`, `EffectRange`, Omen):* An diesen 63 Ids
trennt es falsch herum. 18 haben ein Omen. 15 davon sind nach ihrem Modul Gruppentreffer: Abfall-Kreise, Rückstöße
über die Fläche, Türme. Die drei ohne Modul tragen dieselben Omen (27 wie die Abfall-Kreise, 348 wie der Rückstoß
„Leaping Cleave“); dass auch sie Gruppentreffer sind, ist daraus geschlossen und nicht belegt. Die kleinen Reichweiten (4 bis 8) sind Stacks und Türme. Ein Filter „Omen" oder „kleine Reichweite" hätte also
rund 25 richtige Aufnahmen verworfen, mit ihnen die Gruppenminderung vor diesen Treffern. Die drei ausweichbaren Ids
dagegen hat er nicht erkannt: Kreis 30 ohne Omen, Blicke Kreis 30 und 50 ohne Omen, alle der Art 2 wie die
Raidwides. Was ein Blick ist, steht in keinem Feld der Aktion; BossModReborn erkennt ihn am Modul.
- *Im Kampf:* Vor dem Kreis der Hunde in Dun Scaith geht eine Gruppenminderung nur, wenn im letzten GCD der Spieler
  oder zwei Mitglieder darin stehen. Steht der Heiler darin, startet er dort kein Limit Break mehr, solange ein anderes Mitglied
  draußen steht. Vor einem Blick bleibt die Minderung: Seine Form erreicht alle. Wer hinsieht, wird getroffen, und ob
  jemand hinsieht, weiß RSR nicht.
- *Preis:* Ein Raidwide, dessen Form ein lebendes Mitglied nicht erreicht, etwa einen Fernkämpfer jenseits von
  Reichweite 30, gilt in den Sperren als ausweichbar. Limit Break und langer Kanal warten dann bis nach dem Treffer, so
  wie heute schon bei jedem ungelisteten Cast, der den Spieler erreicht. Unwahrscheinlich, denn Raidwides reichen 30 bis
  100 Yalm und eine Arena selten über 20.

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
Schaden trägt die gerade wirkende Minderung mit; zurückgerechnet wird seit A441, was die Wirktexte beziffern, eine
Barriere und eine unbezifferte Minderung nicht. Die Potenz wäre davon ganz frei. Findet sich eine
auswertbare Potenz für Gegneraktionen, gehört sie **neben** den Anteil, nicht an seine Stelle.

**Was dagegen spricht, in der Reihenfolge des Gewichts:**

1. **Verfügbarkeit für Gegneraktionen ist unbelegt.** Im gesamten Baum wird keine Potenz gelesen; die
   Vorkommen in `ActionId.resx` sind Freitext in den Beschreibungen von **Spieler**aktionen. Der
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
| „Reset AOE List" (setzt die Liste auf die eingebaute zurück) | Werte bleiben |
| „Reset RSR Plugin Settings" (globaler Knopf) | Werte bleiben — setzt nur `Service.Config` zurück |
| Speichern, während der Speicher weniger hält als die Datei (Laden gescheitert, Tabelle aus irgendeinem Grund leer begonnen) | Werte bleiben — jedes Speichern führt Speicher und Datei zusammen und nimmt je Aktion den höheren Wert |
| Datei vorhanden, aber beim Speichern nicht lesbar (gesperrt, beschädigt) | Werte bleiben — das Speichern unterbleibt, „Store:" meldet es rot, der Messwert geht mit dem nächsten Speichern hinaus |
| Laden beim Start abgebrochen oder gescheitert, danach Entladen des Plugins | Werte bleiben — ein Speicher, dessen Laden nicht zu Ende lief, wird nicht geschrieben; das gilt für alle Listen, nicht nur für diese Tabelle |
| Unlesbare Datei beim Start | Werte bleiben, die Datei wird als `.corrupt` beiseitegelegt und gemeldet |
| Kampfende, Zustandswechsel, Laden und Entladen (`DataCenter.ResetAllRecords`) | Werte bleiben — die Methode räumt das Laufzeitgedächtnis eines Kampfes ab und fasst keinen Speicher an |
| **Speicher wird beim Start nicht geladen** | **Totalverlust**, still — behoben, s. u. |
| Speichern während einer neuen Messung | Der Vorgang ging verloren, still; behoben — gespeichert wird ein Schnappschuss, und nur ein Schreiber zur Zeit |
| Gesamtspeichern (`OtherConfiguration.Save`, etwa nach einer neuen Knockback-Aktion im Kampf) während einer neuen Messung | Bis A224 zog es den Schnappschuss erst auf dem Pool-Thread; warf die Kopie, fielen **alle** folgenden Listen dieses Durchgangs aus, still. Behoben: Die vier im Spiel beschriebenen Speicher (Flächenliste, Schadenstabelle, Knockback-Liste, Marker-Negativliste) ziehen ihre Kopie auf dem aufrufenden Thread, auch im Gesamtspeichern |
| Messung zwischen letztem Speichern und Entladen | Verlust dieser einen Messung; behoben — der Effekt-Handler wird vor dem letzten Speichern abgehängt |

**Das Listenfenster führt das unter „Store:".** Das Laden meldet, ob es eine Datei fand, keine fand oder eine unlesbare beiseitelegte; jedes Speichern liest die Datei zurück und meldet Erfolg nur, wenn dort so viele Einträge stehen wie geschrieben wurden. Die Zahl „Damage potential recorded" darüber ist die Tabelle im Speicher — sie sieht gleich aus, ob die Werte die Platte erreicht haben oder nicht.

**Was eine Datei mit `{}` sagt, und was nicht.** Sie entsteht auf genau zwei Wegen: beim ersten Start ohne Datei (das Laden legt die leere Tabelle an) oder in der Sitzung nach einer unlesbaren Datei (die liegt dann als `.corrupt` daneben). Jedes andere Speichern schreibt mindestens einen Eintrag, weil die Tabelle von selbst nur wächst. Seit dem Ladefix heißt `{}` also: **in keiner Sitzung seither wurde ein Wert gemessen.** Die Datei sagt nicht, warum — das sagt nur die Messstelle.

**Die Messung hat sechs Tore**, jedes einzeln hinreichend, um sie zu verhindern: der Effekt-Hook liefert überhaupt Treffer; die Quelle ist ein Gegner, anvisierbar oder ein nicht anvisierbarer Kämpfer (Helfer, Boss außerhalb des Felds, A277); eine Aktion mit Wirkzeit; eine reguläre Aktion der Kategorie Zauber, Waffenfertigkeit oder Fähigkeit; ihre Id in der Flächenliste; ein Betrag über null bei einem Gruppenmitglied.

**`Record enemy AoE actions` und die Gruppengröße gelten nur der Aufnahme in die Liste, nicht der Messung.** Die Option stammt von Upstream (2023) und entscheidet, ob neue Ids in die Liste kommen; wer sie ausschaltet, will die kuratierte Liste so behalten, wie sie geliefert wurde. Die Mindestgröße von vier ist die kleinste Gruppenzusammensetzung des Spiels (`ContentMemberType`, Zeile 2: je ein Tank, Heiler, Nahkämpfer, Fernkämpfer); darunter trifft auch ein Kegel oder ein kleiner Kreis alle, und „alle getroffen" belegt keinen Gruppentreffer mehr. Für eine bereits gelistete Id beantwortet die Messung nur, wie hart sie trifft — das hängt an keiner der beiden Bedingungen. Stünde die Messung dahinter, fiele sie für jeden aus, der die Option abschaltet oder mit Duty Support ohne NPC-Gruppenoption spielt, und mit ihr alle drei Leser: das Auslassen kleiner Casts, das Heilen vor einem großen und das Aufheben einer Zurückhaltung. Ein Wert aus einem Lauf ohne Stufensynchronisation fällt zu klein aus; er korrigiert sich wie jede Unterschätzung beim nächsten ungemilderten Treffer, und die Höchstwert-Regel lässt ihn einen höheren Stand nie senken. Aus Occult Crescent stand in keinem seiner Protokolle bis zum 17.10.2026 ein Cast in der Liste (A299); ob es an der Gruppengröße lag, nennt seit A299 die Zeile am Ende jedes Kampfes („party of …").

**Woran die Messung hängenbleibt, zählt das Listenfenster** seit dem Laden je Grund: wie oft ein gegnerischer Cast den Spieler traf und an welchem Tor er hängenblieb („Casts this session"), und davor, was der Effekt-Handler überhaupt erhält („Effect handler": Effektsätze, davon von Gegnern, davon mit Schaden am Spieler, dazu Fehler und der erste Fehlertext). Null Sätze heißt: Der Hook liefert nichts. Sätze ohne Gegnertreffer heißen: Der Filter vor der Messung verwirft sie. Fehler heißen: Der Handler bricht vor der Messung ab.

**Die Aktionsart wird als ein Byte gelesen** (A177, belegt an seinem Protokoll vom 27.09.2026). ECommons führt `EffectHeader.ActionType` an Offset 0x1F mit dem Enum `ActionType` der ClientStructs, und dessen zugrunde liegender Typ ist `uint` (aus der Assembly gelesen). Das Spielfeld ist ein Byte; ClientStructs legt `Flags` an 0x20 und `NumTargets` an 0x21. Vier Bytes breit gelesen ergab sich `0x00NN0001`: im niedrigen Byte die Aktionsart, im dritten die Zahl der Ziele — in allen 422 Protokollzeilen. Der Vergleich mit `ActionType.Action` schlug damit genau für jeden Satz fehl, der jemanden traf: Die Tabelle blieb leer, und ebenso wenig wurden neue Ids aufgenommen, auch bei Upstream. ECommons las das Feld bis 3.2.1.25 so; sein `ActionEffectSet` wählte damit zwischen Aktion, Gegenstand und Reittier, und mit Zielen fiel jeder Gegenstand in den Zweig „Aktion". ECommons 3.2.1.26 liest das Byte selbst (`ActionTypeByte`, Commit 26d30b9, 15.10.2026), und RSR bindet 3.2.1.26 ein (A296); RSRs eigene Lesung über das niedrige Byte bleibt und ist damit gleichwertig.

**Addle und Radiant Aegis beim Beschwörer** (seine Beobachtung, A181: „es scheint, als ob addle immer zusammen mit schimmerschild gecasted wird beim beschwörer"): Beide stehen im selben Verteidigungspfad, erst Radiant Aegis, im nächsten Einschiebeplatz Addle; ausgelöst durch die Verteidigungsflagge, die für einen gelisteten Cast nur fällt, wenn er als „lohnt Minderung" gilt. Unbewertet heißt „lohnt" — solange die Tabelle leer war, war also jeder gelistete Flächencast ein Anlass für beide. Radiant Aegis trägt zwei Ladungen je 60 s, Addle 90 s Abklingzeit (Job-Guide): Nach dem ersten Paar lädt Radiant Aegis nach, Addle nicht. Ein eigener Defekt der Paarung liegt nicht vor — die beiden schützen Verschiedenes (Radiant Aegis nur den Beschwörer, 20 % seiner Maximalgesundheit; Addle die ganze Gruppe gegen diesen Gegner, 10 % magisch, 5 % physisch), und bei einem großen Treffer sind beide richtig. Mit laufender Messung lösen als klein bewertete Casts die Flagge nicht aus, und Addle bleibt für die großen. Grenze: Der Weg über erkannte Flächen-VFX liest die Tabelle nicht.

**Messung und Speicherung sind belegt (A185):** Seine Datei vom 28.09.2026 enthält sechs bewertete Flächenaktionen (Augen auf, Müllentsorger, Immersion, Störender Schweif, Blitzender Boden, Fluchstimme; Namen über xivapi), beim Start geladen, beim Entladen geschrieben und zurückgelesen. Die Protokolldatei, die nach seiner Vorgabe die Ursache finden sollte, ist damit entfernt; die Zählung je Grund bleibt.

**Sofortaktionen zählen nicht mit:** Sie werden nie bewertet und erscheinen nicht in der Zählung je Grund — sonst überschriebe jeder Auto-Attack binnen einer Sekunde den Grund eines Raidwides.

**Eine leere Flächenliste misst nichts**, gleich wie der Kampf verläuft; beide Fenster melden sie rot. Seit Upstream 3c6ce9ee2 ist die kuratierte Liste ins Plugin eingebaut (`ActionLists.HostileCastingArea`), und die Datei `ListChanges/HostileCastingArea.json` hält nur seine Ergänzungen und Entfernungen; leer wird die Liste nur, wenn er alles entfernt oder das Laden abbricht. Ein abgebrochenes Laden wird nicht gespeichert (A196, A341): Die leere Liste ergäbe sonst „alles entfernt“ in der Änderungsdatei, und jeder spätere Start läse die Liste leer. Eine unlesbare Änderungsdatei wird als `.corrupt` beiseitegelegt; dann gilt die eingebaute Liste. Beim ersten Start mit dieser Fassung wird die alte Datei mit der ganzen Liste einmal übernommen: Seine Ergänzungen (auch aufgezeichnete Ids) bleiben, Entfernungen nicht, und die alte Datei wird gelöscht (Upstreams Migration).

**Casts unsichtbarer Helfer zählen wie die sichtbarer Gegner** (A277). Viele Raidwides löst ein nicht
anvisierbarer Helfer aus, oft mit einer anderen Id als der sichtbare Cast des Bosses (Akhmorning, „Raiding
Fundamentals“, 02.10.2026: bei Zaubern, deren Wirkung später eintritt oder weiterläuft, sind „often invisible,
untargetable entities responsible“; ein Debuff auf dem Boss wie Reflexion mindert deren Schaden „usually“ nicht —
Folge für die Gegner-Debuffs in Konzept 08). Ebenso wirkt ein Boss außerhalb des Felds seine Raidwides nicht
anvisierbar. Die Flächenerkennung liest deshalb neben der Gegnerliste jede nicht anvisierbare Figur der
Gegnerseite – Kämpfer (`BattleNpcSubKind.Combatant`), Teil eines Gegners (`BNpcPart`), Helfer (Unterart 11, in
FFXIVClientStructs ohne Namen, bei BossModReborn `Helper`) oder ohne Unterart, und kein Gruppenmitglied
(`ObjectHelper.IsEnemySideFigure`, A327, A334) –, solange sie eine Aktion der Flächenliste wirkt (`DataCenter.AreaCasters`, gebaut in
`TargetUpdater.UpdateAreaCasters`); alle Leser gehen darüber: die Flächenflagge, „erreicht ihn", der große Cast, die
unbewertete Erkennung, die eigene Flächenabwehr und das Protokoll („(untargetable)"). Der Effekt-Handler lässt
dieselben Quellen durch und dazu jede anvisierbare Figur der Gegnerseite, deren Aktion ein Gruppenmitglied erreicht
(eine Wache ist auch ein Kämpfer, trifft aber nie die Gruppe); also wird ihre Aktion aufgenommen, sobald sie jedes
Gruppenmitglied trifft, und gemessen wie jede andere. Bis A327 las beides nur Kämpfer: Im Protokoll vom 18.10.2026
kamen alle 47 Treffer des Dungeonbosses Hammerschädel aus Figuren, die keine Kämpfer waren – keiner wurde gemessen,
Impakt und Bebende Walze kamen nie in die Liste, und der Tankbuster-Marker vor Hitzesublimierung (89 % seiner LP)
galt als Marker ohne Treffer. Im Kampf, sein Protokoll vom 13.10.2026: Shinryus Meteoreinschlag (#50283, 5 s Wirkzeit, Kreis
60 y) wirken fünf Lichtsphären, jede trifft die ganze Gruppe; ihn trafen je Welle bis zu 65 % zusammen, und keine
Abwehr öffnete, weil die Sphären nicht anvisierbar sind und es für Shinryu Unreal kein BossModReborn-Modul gibt.
Jetzt kommt die Aktion unter „Record enemy AoE actions" beim ersten Treffer auf alle in die Liste, und ab der nächsten
Welle öffnet der Cast die Abwehr. Als Beleg der Feindseligkeit dient die Liste selbst: Eine Id steht dort erst, wenn sie die ganze Gruppe
beschädigt hat; Reichweite und Sicht prüft die Reichweitenprüfung des Casts, weil ein Helfer dort steht, wo seine
Fläche liegt.

**Grenzen:** Ein Helfertreffer ohne eigene Wirkzeit (Schaden „no cast", etwa Hail of Hellflares bei Necrophobia)
bleibt unsichtbar für die Casterkennung; sein sichtbarer Cast bleibt unbewertet, die Zählung weist ihn aus. Der
Messwert gilt je Aktion, nicht je Welle: Treffen mehrere Helfer zugleich mit derselben Id, steht der höchste
Einzelanteil in der Tabelle, nicht ihre Summe.

**Ein anvisierbarer Gegner, den die Gegnerliste ausschließt, zählt ebenso** (A566). Die Gegnerliste
(`AllHostileTargets`, Upstream) lässt einen Gegner aus, der einen Status der Unverwundbarkeitsliste trägt (oder das
Symbol 15024), außer Sicht oder weiter als 48 y steht oder auf „No hostile names" steht: Er ist kein Ziel für Angriffe.
Seine Casts treffen die Gruppe trotzdem, und `AreaCasters` nahm bis A566 nur nicht anvisierbare Figuren dazu – ein
Boss hinter einer Unverwundbarkeit, der einen gelisteten Flächenangriff oder Tankbuster wirkt, öffnete keine Abwehr
und keine Vorausheilung. Belegt ist die Lage in seinem Mitschnitt vom 26.10.2026 (Ultima-Waffe: Irdene Ewigkeit 32,7 s
unter Unverwundbar, Klingenwind 27,7 s unter Sturmwall; beide brachen mit der Phase ab, Schaden entstand dort keiner).
Jetzt nimmt `AreaCasters` auch jede anvisierbare Figur der Gegnerseite auf, die das Spiel als feindlich führt
(`IsEnemy`), nicht in der Gegnerliste steht und eine gelistete Aktion wirkt; sie zählt nicht als Helfer
(`AreaCastHelperIds`). Angegriffen wird sie weiterhin nicht. *Gegenthese:* Ein Phasen-Enrage, der abbricht, sobald die
Adds fallen, öffnet die Abwehr in seinem Minderungsfenster umsonst. Preis benannt: nur wenn er gelistet ist und die
Phase erst in den letzten Sekunden fällt; landet er, ist die Abwehr das Einzige, was noch zählt. *Gegenthese 2:* Ein
Gegner außer Sicht wird gelesen. Widerlegt: Reichweite und Sicht prüft die Reichweitenprüfung des Casts, wie bei
Helfern.

**Tankbuster und Rückstöße von Helfern zählen ebenso** (A303). `AreaCasters` nimmt einen nicht anvisierbaren Kämpfer
auch auf, solange er eine Aktion der Tankbuster- oder der Rückstoßliste wirkt oder einen Tankbuster, den die
Tankbuster-Tabelle gemessen hat (`DataCenter.IsListedHostileCast`); auch hier ist die Liste der Beleg der
Feindseligkeit. Darüber lesen die Tankbuster-Erkennung (`IsHostileCastingToTank`, `IsHostileCastingTankBusterAtMe`),
die Vorhersage des Tankbuster-Plans (`TankbusterForecast`) und die Rückstoßerkennung (`AreHostilesCastingKnockback`).
Für einen Helfer zählt nur ein gelisteter Tankbuster: Der Rückfall „ein Gegner wirkt auf sein eigenes Ziel" liest den
Angriff eines anvisierbaren Gegners auf den, den er bekämpft, und ein Helfer hat daran keinen Teil. Den Rückstoß
beantwortet RSR damit, wie der Text von „Automatically use Anti-Knockback role actions" zusagt, nach der Liste; der Weg
über BossModReborn tat das für angekündigte Rückstöße von Helfern schon vorher. Ein Rückstoß, den die Mechanik
braucht, ist durch die Liste so wenig geschützt wie bei einem anvisierbaren Gegner; die Liste ist von Hand
bearbeitbar.

**Was nur ein Schritt vermeidet, liest jeden wirkenden Gegner, gelistet oder nicht** (A750). Neben `AreaCasters` führt
`TargetUpdater.UpdateAreaCasters` im selben Durchlauf `DataCenter.CastingEnemies`: jeden Gegner der Gegnerliste, der
wirkt, und jede andere wirkende Figur der Gegnerseite (`IsEnemySideFigure`), anvisierbar oder nicht, ohne die Liste als
Beleg. Darüber lesen die Fragen, die nur die Form und den Zeitpunkt eines Casts brauchen: der Treffer, dem ein Schritt
entgeht, vor Limit Break, Kanal und KI-Weg (`RaiseProtection.FirstDodgeableWithin`) und vor einem Zauberbeginn
(`MovementLock.CertainDodgeableWithin`), und das Fenster um einen Treffer auf ihn (`RaiseProtection.FirstWithin`,
`FirstAfter`). Bis A750 gingen sie nur über die Gegnerliste. Die meisten Flächen eines Bosskampfs wirken aber
Helfer, und ausweichbare kommen nie in die Liste, weil ihnen jemand ausweicht. Ohne Modul von BossMod blieben sie
deshalb allen vier Fragen unsichtbar.
- *Im Kampf:* Ein Heiler, der ohne Modul allein in der Kreisfläche eines Helfers steht, startet dort kein Limit Break
  mehr. Mit „Lock movement while casting" hält RSR ihn dort nicht mehr fest. Ein Zauber, den ein Schritt aus der
  Fläche bräche, beginnt nicht, solange er geht.
- *Warum ohne Listenbeleg:* Hier entscheidet die Asymmetrie anders als bei der Abwehr. Ein Helfer-Cast, den RSR
  fälschlich liest, verzögert ein Limit Break oder einen Kanal bis nach dem Treffer, oder er gibt die Bewegung frei.
  Ein übersehener lässt den Spieler in der Fläche stehen. Die Abwehrflaggen bleiben bei `AreaCasters`: Dort kostet
  ein Fehlgriff eine Minderung, die später fehlt.
- *Preis:* Eine Figur der Gegnerseite ohne Gruppenzugehörigkeit, deren Cast den Spieler erreicht, aber nicht trifft,
  etwa ein Verbündeter in einer Soloinstanz, zählt mit. Das Limit Break wartet dann bis zum Ende ihres Casts. Belegt
  ist kein solcher Fall.
- *Gegenthese – ein ungelisteter Raidwide eines Helfers sperrt das Limit Break:* Das gilt bis zu seinem ersten
  Treffer. Für einen ungelisteten Cast eines anvisierbaren Bosses gilt es heute schon. Danach steht er in der Liste,
  und die Form sagt „erreicht alle“; er sperrt nicht mehr (Abschnitt „Raidwide oder ausweichbare Fläche“).
- *Kosten:* Der Durchlauf über die Objekttabelle läuft schon jedes Bild. Neu ist ein Eintrag je wirkender Figur. Die
  drei Sammlungen (`AreaCasters`, `AreaCastHelperIds`, `CastingEnemies`) werden je Bild geleert und wiederverwendet;
  bis A750 legte der Durchlauf zwei davon jedes Bild neu an (Konzept 24, „Speicher“).

**Ein gelisteter Rückstoß zählt nur, wenn er den Spieler erreicht** (A701, alle Rollen: Surecast und Arm's Length,
120 s Abklingzeit). *Sachstand bis A701:* `IsAnyHostileCastingKnockback` fragte nur, ob ein gelisteter Zauber im
Landefenster gewirkt wird; die Flächenabwehr fragt seit A218 auch, ob seine Form den Spieler erreicht. *Seine Meldung*
(11.11.2026): „es wird antirückstoß verwendet, obwohl kein rückstoß da war (whm)". *Gemessen* an seinen Mitschnitten vom
10. und 11.11.2026, zwölf Surecasts: neun galten einem Rückstoß oder Zug, der ihn erreichte – Zugstrahl (zweimal),
Eald'narches Rückstoß #44823, Flutwelle (zweimal, BossMods Zeitleiste), Ätherknall (zweimal, Zeitleiste), Aetherial
Pull in Babil (zweimal, Zeitleiste; ein Zug von 30 y auf einen Spieler, BossModReborn `D023Anima`, 2087153c2,
11.11.2026: „pull 30 between centers", mit dem Hinweis auf Rückstoßschutz). Drei nicht: Ground and Pound in Babil
(#25322, Linie 6 × 40 y, er stand nicht darin) und Tornado bei Eald'narche (#44395, traf ihn nicht), beide über die
Liste; einer in Windurst ohne gelisteten Zauber (Winde oder Zeitleiste – welcher Weg, sagt kein Protokoll).
- *Optionen:* (a) Nullvariante. (b) Die Reichweite wie bei der Flächenabwehr (`AreaCastReaches`): Kreis und Linie
  genau, Kegel und Ring als voller Kreis, ein auf ein Ziel gewirkter Zauber nur, wenn er auf ihn zielt. (c) Aus dem
  letzten Ausgang lernen, ob der Zauber ihn erreichte (wie „Skip area defence for casts that missed you"). (d) Nur
  BossMods Zeitleiste.
- *Abwägung im Kampf:* (a) Ground and Pound nimmt Surecast für 120 s; kommt in dieser Zeit ein Rückstoß, der ihn trifft
  – in Babil Aetherial Pull im nächsten Kampf –, fliegt er; an einem tödlichen Rand stirbt er. (b) *Stärken:* Ground and
  Pound und jede Linie oder jeder Kreis, in dem er nicht steht, lassen Surecast stehen; Aetherial Pull auf ihn zählt
  weiter, auf einen anderen nicht – ein Zug auf einen anderen bewegt ihn nicht. *Schwächen:* Kegel und Ring zählen als
  voller Kreis – seit A731 gemessen, soweit das Omen Winkel und Innenradius nennt (oben, „Wen die Form erreicht"). *Chancen:* dieselbe Prüfung, die die Flächenabwehr seit A218 trägt. *Bedrohungen:* Er tritt
  erst nach der Prüfung in die Form; gefragt wird jedes Bild im Landefenster (letzter GCD vor dem Treffer), tritt er
  später hinein, kommt Surecast zu spät – wie heute bei jedem Rückstoß, der erst spät angekündigt ist. (c) gelernt aus
  einem Ausgang, der von seiner Position abhing – bei einer Linie, der er einmal auswich, bliebe Surecast beim nächsten
  Mal aus, auch wenn er darin steht: schlechter als (b). (d) ohne Modul nichts (Regel „BossModReborn", `code.md`).
- *Gewählt:* (b). Die Zeitleiste von BossMod bleibt daneben; sie nennt keine Getroffenen (nur die Schadensvorhersage
  führt eine Maske), und in seinen Mitschnitten traf jeder ihrer Rückstöße ihn. *Preis:* Ein Rückstoß der Zeitleiste,
  der ihn nicht erreicht, nimmt Surecast weiter.
- *Wechselwirkungen:* Die Flagge öffnet neben Surecast und Arm's Length (und den Job-Ersatzaktionen) auch die
  Sicherheitswege an einem gehaltenen Kanal (`BaseAction.SafetyMayEndAChannel`): Ein Rückstoß, der ihn nicht erreicht,
  beendet den Flammenwerfer nicht mehr. `CastStartBlockedNow` liest den laufenden Rückstoß, nicht die Ankündigung:
  unberührt. `IsHostileCastingKnockback` (öffentlich) bleibt die Frage „wirkt dieser Gegner einen gelisteten Rückstoß".
- *Messmittel:* Jedes Öffnen der Flagge schreibt eine Zeile in `DefenseTrace.log` mit dem Weg – gelisteter Zauber
  (Name, Nummer, Wirkender), BossMods Zeitleiste (in wie vielen Sekunden) oder die Winde in Windurst –, damit jeder
  weitere Fall zuzuordnen ist.
- *Falsifikation:* *Kein Defekt:* widerlegt an Ground and Pound. *Option falsch:* Ein Rückstoß wirkt weiter als seine
  Schadensform – der Rückstoß ist ein Effekt der Aktion auf die Getroffenen (`ActionEffectType.Knockback` im
  Effekteintrag des Getroffenen, `Watcher`), wer außerhalb steht, wird nicht bewegt. *Ausgeliefert, und nichts ändert
  sich:* Der dritte Fall (Windurst) lief nicht über die Liste; die Messzeile zeigt den Weg beim nächsten Mal.

**Das Zurücksetzen der Liste lässt die Werte stehen** — Vorgabe des Auftraggebers: „es wäre schade,
wenn dann auch die Erfahrungswerte weg wären." Die kuratierte Liste zurückzusetzen kostet nichts, die
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

**Selbstkorrektur nach einem Patch: entschieden, nicht gebaut** (Abschnitt „Gelernte Tabellen über Spielversionen",
A447–A451). Die beiden einfachen Formen – die erste neue Messung ersetzt den alten Wert, oder ein gleitendes Maximum über
die letzten Messungen – tragen das Risiko, dass eine kleinere erste oder letzte Messung den Wert zu tief setzt und der
nächste Treffer ohne Schutz ankommt; die zweite braucht dazu eine feste Zahl. Die Revision dort widerlegt einen alten
Wert erst mit so vielen sauberen Beobachtungen, wie er selbst hatte, oder nach einem Abfall, den kein Schadenswurf
erklärt. Bis zu ihrem Bau deckt das Löschen von Hand den Fall – seit Stufe 1 beide Dateien des Speichers (Abschnitt
„Stufe 1: Speicher, Übernahme, Sicherung").

**Seit A172 kann kein Speichern mehr verlieren, was die Datei hält.** Die Tabelle wächst von selbst nur; also ist jeder Stand der Datei eine Untergrenze, und ein Speichern nimmt je Aktion das Höhere aus Speicher und Datei. Damit ist die Fehlerklasse geschlossen, nicht der Einzelfall: Gleich aus welchem Grund der Speicher leer oder unvollständig ist — Laden gescheitert, abgebrochen, ein künftiger Defekt —, die Datei behält ihren Stand. Dazu wird ein Speicher, dessen Laden in dieser Sitzung nicht zu Ende lief, gar nicht geschrieben: Das Entladen schreibt alle Listen, und ein abgebrochener Start hätte sie sonst alle geleert. Ausgenommen ist nur ein Wert, den die Messung selbst nicht ablegt – über der ganzen Maximalgesundheit (A298, Abschnitt „Was gemessen wird, und was nicht"), nicht endlich oder negativ –; die Revisionsdatei hält ihn in Quarantäne (Abschnitt „Stufe 1: Speicher, Übernahme, Sicherung").

**Der schwerste Weg war keiner der erhobenen, sondern das Ausbleiben des Ladens** (A121). Der Speicher
stand nur in `OtherConfiguration.Init()`, und die ruft niemand; gerufen wird `InitAsync`. Die Tabelle
begann damit jede Sitzung leer, und weil ein Speichervorgang **die ganze** Tabelle schreibt, legte die
erste Messung des Abends — spätestens das Entladen des Plugins — die leere Fassung über den
gespeicherten Stand. Im Kampf war das Bild identisch mit „noch nichts gemessen": jede Aktion
unbewertet, beide Stufen der Rechnung wirkungslos. **Laden und Speichern laufen über eine einzige Liste
aller Speicher** (`OtherConfiguration.Files`, seit upstream 7.5.7.5, A331): `InitAsync` lädt genau ihre
Einträge, `Save()` und `SaveList` schreiben genau sie, und `check_config_store_roundtrip.py` hält in der CI
fest, dass kein Speicher an ihr vorbei geschrieben wird. Die Lehre daran ist allgemeiner als der Fall: Ein
Speicher, der geschrieben, aber nicht gelesen wird, ist gefährlicher als gar keiner — er ersetzt den
Bestand durch das Nichts, das er für richtig hält.

**Geschrieben wird über eine temporäre Datei.** `File.WriteAllText` kürzt zuerst und füllt danach; ein
Absturz dazwischen hinterlässt unlesbares JSON, und der Ladepfad beantwortet das damit, leer
anzufangen — **ohne** neu herunterzuladen, weil die Datei existiert. Für die kuratierten Listen kostet
das einen Knopfdruck, für die Erfahrungswerte alles. Und geschrieben wird dieser Speicher **im
Kampf**, bei jedem neuen Höchstwert, also genau dann, wenn ein Absturz am wahrscheinlichsten ist.

## Die Tankbuster-Tabelle

**Sein Auftrag (04.10.2026):** „erstelle eine solche liste mit ingame auswertung ähnlich der liste für aoes. mach
da aber beim speichern nicht die gleichen fehler." Dazu seine Frage: „kannst du auch gesehene tankbuster auf andere
spieler auswerten?" Anlass ist sein Hinweis, dass sich ohne den erwarteten Schaden nicht sagen lässt, ob ein
Tankbuster tödlich ist. Eine öffentliche Quelle dafür gibt es nicht (A258).

**Gebaut (A259, A454):** `OtherConfiguration.TankbusterPotential`, je Aktion zwei Werte als Anteil an den maximalen LP
des Getroffenen: der ungeminderte Schaden ohne Verwundbarkeit und der unter Verwundbarkeit – jeweils, was ein Tank aus
einem Cast zusammen nimmt, alle seine Treffer summiert –, dazu die Dauer dieser Serie vom ersten bis zum letzten Treffer
in Sekunden (`TankbusterReading.Span`). Alle drei steigen nur. Messung in `TankbusterTable`, Ablage über denselben
gesicherten Weg wie die Flächentabelle.

### Was gemessen wird

- **Welche Treffer:** jeder Treffer einer gegnerischen Aktion, die in der Tankbuster-Liste steht oder deren
  Tankbuster-Marker bestätigt wurde (`TankbusterMarkerWatch.RecordHit`), und jeder Treffer eines Casts, den der Plan als
  Tankbuster nimmt, samt dessen Folgetreffern (Absatz „Ein Tankbuster ist der Cast samt seiner Treffer"), auf **jeden
  Tank** unter den Spielern. Das schließt den Co-Tank ein und in großen Inhalten Tanks anderer Gruppen. Die LP und Status anderer Spieler liegen vor.
  Ein Treffer auf einen Nicht-Tank wird nicht abgelegt (A312): Jeder Leser rechnet für einen Tank – der Plan des
  Spielers, der Tankwechsel –, und dem Nicht-Tank fehlen die Minderung des Tank-Merkmals, die kein Status trägt, und
  die größere Maximalgesundheit; ein Fehlschlag an ihm höbe den Wert für jeden Tank, im Extremfall über die ganze
  Gesundheit hinaus. Das Protokoll schreibt ihn mit „(not a tank) … not stored". Keiner der Leser liest die Tabelle für
  einen Nicht-Tank, also fehlt dort nichts.
- **Ein Tankbuster ist der Cast samt seiner Treffer** (A454). Abgelegt wird, was ein Tank aus einem Cast zusammen nimmt,
  unter der Id, die der Plan liest: der des Casts.
  - *Warum:* Bei jedem erhobenen mehrfach einschlagenden Tankbuster mit Cast kommt ein Teil der Treffer unter anderen
    Ids als der Cast, oft von einem Helfer. Lead Hook (Criterion, 4 s): der erste Treffer unter dem Cast, der zweite und
    dritte unter zwei anderen Zeilen, je 1,1 s später. Crush Helm (P12S, 5 s): Cast des Bosses auf sich selbst, vier
    kleine Treffer eines Helfers, die eine Verwundbarkeit stapeln, dann der große 6,1 s nach dem Balken. Void Meteor
    (Golbez, Extreme): vier Kometen eines Helfers ab dem Balken im Abstand von 1 s, der Meteor 4,1 s nach dem Balken.
    Bitter Whirlwind (Zoraal Ja, Extreme): drei Treffer im Abstand von 3,3 und 3,1 s, zwischen ihnen ein Tankwechsel
    oder eine Unverwundbarkeit (BossModReborn ae64e33 vom 20.10.2026, Werkzeugquelle; Namen, Wirkzeiten und
    Angriffstypen aus den Spielblättern, xivapi v2). In seinen Mitschnitten vom 25. bis 28.10.2026: Knallregen
    (Alexander – The Arm of the Son, 2,0 s laut Blatt) drei bis sechs Treffer im Abstand von 2,14 bis 2,18 s, ab dem
    zweiten unter der Nachbarzeile; Valfodr (Pilgrim's Traverse, 5 s) drei bis vier Treffer bis 7 s nach dem Balken;
    Quadrupel-Triade (5 s) zwei Treffer im Abstand von 2,6 bis 2,8 s. Bis A454 lag jeder Treffer einzeln als Höchstwert
    unter seiner eigenen Id. Der Plan las nur den ersten Treffer (Lead Hook, Knallregen unter der Id des Casts; Crush
    Helm über den Marker den ersten kleinen) oder nichts (Void Meteor), das Bündel reichte für einen Bruchteil, und eine
    kurze Minderung, die den ersten Treffer deckte, war beim großen schon abgelaufen. In seinen Abwehrprotokollen steht
    keine Serie: Die Mehrfachtreffer dort nannte weder Liste noch Marker; seine Mitschnitte belegen Einzelfälle, nicht
    ihr Fehlen.
  - *Zuordnung:* `TankbusterTable` verfolgt jedes Wirken der Gegner, die RSR liest (`DataCenter.AreaCasters`), mit
    Wirker, Id, Name, Angriffstyp, Ziel und Ende des Balkens; jeder Cast ist eine eigene Instanz. Ein Treffer auf einen
    Tank gehört zu dem Cast mit seiner Id, sonst zu dem mit seinem Namen – Cast und Treffer heißen in allen erhobenen
    Fällen gleich, ausgenommen die Kometen von Void Meteor („Void Comet") –, dessen Balken höchstens ein Serienfenster
    vor dem Treffer endete und der auf diesen Tank oder auf kein Gruppenmitglied zielte (auf sich selbst, den Boden).
    Unter mehreren gilt der Cast des Wirkers, der trifft; der eines anderen Wirkers nur, wenn ein nicht anvisierbarer
    Helfer trifft. Bei mehreren Instanzen gilt die, deren Balken zuletzt vor dem Treffer endete. Ein Cast mit anderer
    Schadensart als der Treffer – physisch gegen magisch – zählt nicht: Die Vorhersage legt die Minderung nach der Art
    des Casts auf. Ein Cast ohne Angriffstyp (die sichtbaren Casts der Bosse) zählt, die Vorhersage nimmt dann die
    kleinere der beiden Minderungen und rechnet eher zu viel.
  - *Wann ein Treffer zählt:* wenn Liste oder Marker ihn nennen (wie bisher); wenn er zu einem Cast gehört, den der Plan
    als Tankbuster nimmt – gelistet, gemessen oder als Wirken auf das eigene Ziel (Absatz „Auch ohne Liste und Marker");
    oder wenn er die offene Serie seines Namens auf diesem Tank fortsetzt (die Folgezeilen, die keine Liste nennt).
    Einen Treffer, den Liste oder Marker nennen und den kein Cast seiner Id oder seines Namens erklärt, rechnet die
    Zuordnung dem Tankbuster-Cast zu, dessen Fenster auf diesem Tank offen ist – gelistet oder gemessen, auf diesen Tank
    oder auf kein Mitglied gezielt, von derselben Schadensart oder ohne eine –, sonst der offenen Serie eines solchen Casts: Die Kometen von Void Meteor treffen
    ab dem Balken, vor dem Meteor, und gehören so zu seiner Serie.
  - *Die Serie:* Ein Treffer setzt sie fort, solange er dem vorigen früher folgt – der erste dem Ende des Balkens – als
    das Serienfenster: die Wirkzeit des Casts laut Blatt, mindestens ein GCD (`DataCenter.DefaultGCDTotal`). Die
    Wirkzeit, weil ein neuer Cast derselben Aktion mindestens so lange braucht; einen neuen Balken trennt die Zuordnung
    ohnehin. Der GCD für Treffer ohne Cast (Bahamut's Claw, UCoB: fünf Treffer in 3,2 s) und weil binnen eines GCD keine
    Heilung zwischen zwei Treffer kommt. Alle erhobenen Abstände liegen im Fenster, Knallregen (2,14 s gegen 2,0 s
    Wirkzeit) nur durch den GCD. Mit einem Tankwechsel zwischen den Treffern hat jeder Tank seine eigene Serie.
  - *Ohne Cast:* Treffer, die kein Cast erklärt – ein Marker ohne Wirken –, bilden die Serie unter der Id ihres ersten
    Treffers, derselben, die der Marker lernt (Konzept 09, „Ein Marker ohne Wirken"); ihr Fenster ist die Wirkzeit
    dieser Zeile, mindestens ein GCD.
  - *Abgelegt* wird, wenn die Serie endet – ihr Fenster ist ohne weiteren Treffer verstrichen, oder die Ziellisten
    werden außerhalb des Kampfes geleert –, einmal je Serie; je Treffer schreibt das Protokoll den Zwischenstand. Bis
    zum Ende kann ein späterer Treffer die Serie noch unbrauchbar machen; je Treffer abgelegt, bliebe der Zwischenstand
    stehen, weil die Tabelle nur steigt. Abgelegt werden die Summe der ungeminderten Anteile – jeder Treffer mit der
    Minderung zurückgerechnet, die bei ihm stand – und die Dauer vom ersten bis zum letzten Treffer, beide als
    Höchstwert, die Dauer auch ohne höhere Summe. Ob unter Verwundbarkeit, entscheidet der erste Treffer: Eine
    Verwundbarkeit, die ein früherer Treffer derselben Serie legte (Crush Helm), gehört zum Tankbuster und kommt mit ihm
    wieder. Nicht abgelegt wird eine Serie mit einem Treffer von einem Angreifer mit Schadensverstärkung, bei einem
    Wirken auf das eigene Ziel mit einem Treffer, der mehr als ein Ziel erreicht, und mit einem Treffer ohne Schaden
    (Unverwundbarkeit, Ausweichen): Ihre Summe wäre nur eine Untergrenze, und der Plan hielte für sie die übrige Abwehr
    zurück. Vor A454 lag in diesem Fall der ungedeckte Rest als Einzeltreffer in der Tabelle. Ein Effektsatz zählt je
    Tank einmal. Serien und verfolgte Casts verfallen mit ihrem Fenster und mit den Ziellisten außerhalb des Kampfes.
  - *Wer liest:* der Plan die Summe als P und die Dauer für den Horizont (Konzept 09, „Die Rechnung"); die Hilfe für das
    Ziel eines Tankbusters ebenso (`DataCenter.AimedTankbusterLeaves`); der Tankwechsel die Summe („eine Wiederholung
    bringt dich um") unter der Id, unter der die Serie liegt. Der Marker lernt als seinen Treffer ebendiese Id – die des
    Casts, wenn einer ihn erklärt (Konzept 09, „Ein Marker ohne Wirken"); steht er vor dem Balken, plant RSR so schon
    mit der Serie. Für einen Tankbuster aus einem Treffer ändert sich nichts: Summe und Treffer sind gleich, die Dauer
    ist 0.
  - *Fehlerrichtung:* Zu kurz – die Serie zerfällt in Einzeltreffer wie vor A454 – wird sie, wenn der Beobachter einen
    kürzeren GCD hat als der Abstand (Knallregen bei einem GCD unter 2,14 s); der Höchstwert behält die längere Serie
    eines anderen Beobachters. Zu lang wird sie nur ohne neuen Cast: Zwei Einsätze ohne Balken binnen eines Fensters
    verschmelzen, und ein gleichnamiger Treffer, der nicht zum Tankbuster gehört, erhöht die Summe – beides mehr Abwehr,
    die sichere Richtung (Stufe 3). Heilung zwischen den Treffern zählt nicht: Die Summe ist, was der Tank ohne Heilung
    nimmt, wie der Plan rechnet (Konzept 09, „Ohne Risiko"); bei langen Serien kleiner Treffer (Knallregen: sechs
    Treffer zusammen 85 % der maximalen LP über 11 s, gemessen unter der Minderung, die stand) gibt der Plan damit mehr
    Abwehr aus, als die Heiler nötig machten.
  - *Grenzen:* Einen Treffer anderen Namens, den weder Liste noch Marker nennen, führt nichts mit seinem Cast zusammen.
    Wirkt ein Helfer eine ungelistete, ungemessene Aktion, liest RSR seinen Cast nicht; sein Treffer geht an den
    sichtbaren Cast gleichen Namens, wenn der Boss einen wirkt, sonst bleibt er unter seiner eigenen Id. Endet das
    Plugin, bevor eine Serie endet, geht sie verloren.
- **Auch ohne Liste und Marker** (A452): der Treffer eines Casts, den RSR nur als Wirken eines Gegners auf sein eigenes
  Ziel für einen Tankbuster nimmt (`DataCenter.IsHostileCastingTank`, Rückfall) – ein anvisierbarer Gegner, kein Helfer,
  nicht unterbrechbar, länger als ein GCD, auf einen Tank. Seine Landung erkennt die Zuordnung des vorigen Absatzes; in
  seinem Mitschnitt vom 26.10.2026 landeten 84 von 85 gezielten Casts über 2,5 s unter ihrer eigenen Id, einer – Valfodr
  – unter der Nachbarzeile. Abgelegt wird unter der Id des Casts, die die Vorhersage liest, mit dem Kennzeichen
  `TankbusterReading.AimedOnly`, solange kein Treffer der Serie unter Liste oder Marker kam. Ein Treffer, der mehr als
  ein Ziel erreicht – auch in einer anderen Allianzgruppe –, ist ein Cleave oder Raidwide und wird so nicht gelernt.
  - *Wer ihn liest:* nur der Tankbuster-Plan, für einen Cast auf den Spieler, und er hält für ihn nichts weiter zurück
    (Konzept 09). Für die Leser, die die Abwehr öffnen oder Hilfe rufen (`IsLearned`: `IsKnownTankbusterCastAt`,
    `AimedTankbusterLeaves`, Helfererkennung), ist er kein bekannter Tankbuster: Im Pull ist er jeder Trash-Zauber auf
    den Tank, und mit ihm kehrte die Staffel zurück, die A395 im Pull abgestellt hat. Der Tankwechsel bleibt bei Liste
    und Marker. Eine Landung unter Liste oder Marker hebt das Kennzeichen für immer auf.
  - *Grenzen:* Einen Treffer, den ein unsichtbarer Helfer statt des Wirkers bringt, erkennt die Zuordnung nur unter dem
    Namen des Casts; bei ausgeschaltetem RSR ohne Auto-On aktualisiert RSR die Ziele nicht und verfolgt nichts.
  - *Ob der Rückfall ihn nimmt, liest RSR, solange der Cast läuft* (A540). Der Rückfall fragt im Landefenster des
    Flags, ein bis zwei GCDs vor dem Ende des Casts (`IsHostileCastingBase`). Bis A540 wurde er nur einmal gefragt, beim
    ersten Sehen des Casts; ein Cast, der dann noch länger als zwei GCDs lief, galt nie als gezielt und wurde nie
    gelernt, auch nicht bei jeder Wiederholung. Alle acht so gelernten Tankbuster seiner Tabelle vom 04.11.2026 wirken 3
    bis 5 s. Jetzt gilt der Cast als gezielt, sobald der Rückfall in seinem Lauf greift; Ziel und Tank-Sein bleiben beim
    ersten Sehen festgelegt.
- **Auf einen Nicht-Tank gezielt: eine eigene Tabelle** (A540, `OtherConfiguration.AimedHitPotential`). Gelernt wird
  der Cast eines Gegners auf einen Spieler, der kein Tank ist, gelistet oder nicht, unterbrechbar oder nicht, auch von
  einem unsichtbaren Helfer: Das Ziel nennt der Cast selbst, anders als beim Rückfall für Tanks, der es aus dem Ziel des
  Wirkers liest. In M4 kam jede Zellexplosion von einer eigenen unsichtbaren Kopie Lindwurms, eine je Spieler. Ob er
  Minderung lohnt, entscheidet seine Größe, nicht seine Art. Seine Treffer werden wie beim Tank zur
  Serie zusammengezählt und auf ungemindert zurückgerechnet (Minderung auf dem Getroffenen und am Gegner, vergrößerte
  Maximal-LP), mit denselben Ausschlüssen: Schadenserhöhung am Gegner, Messung über der Stufe des Inhalts, ein Treffer,
  der mehr als ein Ziel erreicht, ein abgewehrter Treffer. Abgelegt wird in der eigenen Tabelle, nicht in der
  Tankbuster-Tabelle – deren Leser rechnen für Tanks (A312 bleibt). Form (`TankbusterReading`), Revision über
  Spielversionen und Anzeige („Aimed hits on non-tanks") wie bei der Tankbuster-Tabelle.
  - *Gemessen, warum die Größe entscheidet:* In seinen Mitschnitten vom 25.10. bis 03.11.2026 wirkten Gegner 239
    Zauber gezielt auf ihn: 166 nahmen unter 10 % seiner LP (meist Blitz, Feuer und Eis von Trash), 38 bis 25 %, 28 bis
    40 %, 7 mehr (Zellexplosion 59 und 60 %, Blitz von Angra Mainyu bis 64 %). Bei jedem gezielten Zauber zu mindern
    kostete in sieben von zehn Fällen eine Abklingzeit für einen Treffer, den die Heilung ohnehin auffängt; fehlt sie
    dann beim nächsten Raidwide, nimmt er dort mehr.
  - *Wer sie liest:* `DataCenter.AimedHitLeaves` für ein Mitglied, das kein Tank ist und auf das ein Gegner einen
    gemessenen Zauber wirkt: vorhergesagter Anteil = gelernter Wert (unter Verwundbarkeit, wenn er eine trägt und sie
    gemessen ist) mal der jetzt stehenden Minderung, geteilt durch die Vergrößerung der Maximal-LP; was bleibt =
    vorhergesagte effektive LP minus Anteil. Mindern lohnt nach demselben Urteil wie die Hilfe für einen gemessenen
    Tankbuster (`TankbusterHelp.MeasuredHitCallsForNoHelp`): ab dem Anteil eines großen Schildes
    (`LargestStatedBarrierShare`), oder wenn er unter der Einzelheilungslinie (`HealthSingleSpell`) bliebe. Ein Wert,
    den die laufende Spielversion noch nicht bestätigt hat, zählt als lohnend, wie dort. Im Landefenster des Flags
    (`IsHostileCastingBase`):
    - Trifft er den Spieler selbst, öffnet sich seine Einzelabwehr, und „single hit reaches you" ist wahr; seine
      Selbstschutz-Aktionen gehen (A233).
    - Trifft er ein anderes Mitglied, öffnet sich die Einzelabwehr eines Heilers, und seine Hilfe geht an dieses
      Mitglied (`FindBeAttackedTarget`), nicht an den Tank, den der Boss angreift.
  - *Bilanz im Kampf:* M4 (Konzept 11, „Nach der eigenen Wiederbelebung"): Um 18:34:00 landete Zellexplosion auf jedem
    der acht Spieler, auf vier Nicht-Tanks sauber mit je einem Ziel, 54 bis 71 % ihrer LP; ihn trafen zwei zugleich, das
    wird nicht gelernt. Die Serien enden 4,7 s später und gehen in die Tabelle, sobald RSR wieder Ziele aktualisiert –
    nach seinem Tod also spätestens mit dem Wiedereinschalten nach der Wiederbelebung um 18:34:29. Als um 18:34:29,8 die
    nächste Zellexplosion auf ihn begann, stand ihr Wert damit fest: Ab einem GCD vor dem Einschlag öffnet sich seine
    Einzelabwehr, und weil der Treffer erst nach seiner Unverwundbarkeit landet, darf sie diese beenden. Ein Heiler
    neben ihm mindert ihn statt des Tanks. Kosten: Abklingzeiten nur für Treffer ab einem großen Schild oder unter die
    Einzelheilungslinie, also für die 15 % seiner gezielten Treffer ab 25 % und für kleinere nur, wenn er schon tief
    steht.
  - *Verworfen:* jeden gezielten Zauber auf einen Nicht-Tank als Treffer nehmen, wie beim Tank – sieben von zehn
    Abklingzeiten auf Trash-Blitze; nur innerhalb des Kampfes messen – derselbe Bau ohne Gedächtnis, und der erste Treffer
    jedes Abends bliebe ungemindert; die Werte in die Tankbuster-Tabelle legen – A312.
  - *Grenzen:* Beim ersten Mal ist die Größe unbekannt, und nichts öffnet; ab der ersten sauberen Landung gilt sie,
    auch im selben Kampf. Die Tabelle hält je Aktion den höchsten Anteil über alle Nicht-Tanks; ein Wert vom Magier mit
    kleinerem LP-Vorrat gilt auch für den Nahkämpfer – das überschätzt dort, in die sichere Richtung. Zwei Zauber auf
    ihn zugleich (in M4 um 18:34:00, je zwei Ziele) werden nicht gelernt. Ein unterbrechbarer Zauber öffnet nichts, wie überall
    im Flag (`IsHostileCastingBase`); gelernt wird er trotzdem.
  - *Falsifikation:* Kein Defekt – widerlegt durch M4 (60 % gezielt, keine Minderung). Option falsch – die stärkste
    Gegenposition ist die Überschätzung über alle Nicht-Tanks; sie kostet eine Abklingzeit bei einem Treffer knapp unter
    dem Schild-Anteil, kein Leben, und hält deshalb nicht. Ausgeliefert, und nichts ändert sich – (1) beim ersten Treffer
    eines Zaubers, der in keiner Datei steht; (2) bei einem Job ohne Einzelabwehr für sich (dann bleibt die Hilfe des
    Heilers); (3) wenn RSR aus ist – der Effekt-Handler misst trotzdem, nur die Zuordnung zum Cast fehlt dann, und der
    Treffer bleibt ungelernt (wie beim Tank).
- **Kein Raidwide:** Eine Aktion der Flächenliste hat schon einmal jedes Mitglied getroffen; sie ist nicht der
  Tankbuster eines Markers, auch wenn sie das markierte Mitglied trifft, solange der Marker steht
  (`TankbusterTable.IsPartyWide`). Bis A337 zählte jede Aktion in diesem Fenster: In seinen Protokollen vom 22. und
  23.10.2026 kamen Feldgeschrei (zwei Fassungen, bis 25 Ziele) und Wehklagen so in die Tabelle, zusammen mit dem
  echten Tankbuster, der im selben Augenblick traf, und ebenso als Tankbuster zum Tankwechsel. Den Marker bestätigt ein
  Raidwide weiter (Konzept 15, der Fehler liegt auf der sicheren Seite). Was die Datei davon schon hält, bleibt dort,
  weil ein Speichern mit ihr zusammenführt und nie entfernt; jeder Leser übergeht es (`IsLearned`, `PredictedShare`).
  Ein Raidwide, der noch nicht in der Liste steht, kommt beim ersten Mal noch hinein und wird übergangen, sobald die
  Liste ihn aufnimmt.
- **Zurückgerechnet auf ungemindert:** Der gemessene Anteil wird durch die Minderung geteilt, die beim Einschlag
  stand.
  - Auf dem Getroffenen zählt sein eigener Anteil, auf dem Angreifer der Anteil nach Schadensart: physisch für
    `AttackType` 1–4, magisch für 5, sonst die kleinere der beiden Minderungen.
  - Die Werte kommen aus den Wirktexten der Aktionen, die den Status legen (`DefensiveValues.MitigationByStatusId`).
    Der Generator verknüpft die Status über den gemeinsamen Namen oder über die Statusangabe im Code.
    Sammellisten wie die von Schutzwall, die die Sperrgruppe aller großen Minderungen sind, bleiben dabei außen vor.
  - Er liest auch die Formen, die den Träger vor „damage taken" nennen („Reduces target party member's damage taken by
    10%", Intervention; „Reduces target's damage taken", Taurochole), die Form für den nächsten Treffer (Third Eye) und
    eine Minderung nur einer Schadensart („Reduces magic damage taken by …", Magick Barrier, Fey Illumination). Die
    zweite Art steht dann auf 0; bei unbekannter Schadensart zählt die kleinere, also 0. Bis A286 fehlten diese Werte,
    und ein Treffer unter ihnen wurde zu klein gelernt.
  - Setzt ein Merkmal die Zahl und lässt der Wirktext sie leer („Improves Troubadour's damage reduction effect to 15%",
    Troubadour, Tactician, Shield Samba ab Stufe 98), gilt der Anteil des Merkmals — zur Laufzeit aus dem Spiel
    (`TraitDurations`), sonst aus `trait_durations.json` (`DefensiveValues.TryStatus`).
  - Einen Wert trägt nur ein Status, dessen eigener Text eine Minderung oder einen größeren Lebenspool nennt. Ein Status,
    der nur den Namen der Aktion teilt — der HoT von Temperance, die Barriere von Aquaveil, die Rückstrahlung von Nebula,
    eine ältere Form derselben Aktion —, trägt keinen; sonst würde dieselbe Minderung doppelt herausgerechnet.
  - Damit drückt ein gut geminderter Treffer die Aktion nicht nach unten; die Flächentabelle rechnet seit A441
    ebenso zurück (Abschnitt „Grenzen der Messung").
- **Unter Verwundbarkeit getrennt:** Trägt der Getroffene eine Verwundbarkeit, geht der Wert in die zweite Zahl.
  Ausgenommen ist eine, die genau dieser Treffer legt (Effektsatz): Sie kam mit ihm und hat ihn nicht verstärkt. In
  einer Serie entscheidet ihr erster Treffer, und was ihre Treffer legen, gehört zum Tankbuster (A454).
  Ob das Spiel sie beim Eintreffen des Effekts schon gesetzt hat, ist nicht belegt; die Ausnahme macht die Frage
  gegenstandslos.
  Sonst würde der zweite Treffer einer Wechselmechanik die Aktion selbst als so hart einstufen.
- **Null zählt nicht:** Ein Treffer ohne Schaden – Unverwundbarkeit, Ausweichen – sagt, dass etwas ihn aufhielt,
  nicht, dass er harmlos ist; eine Serie mit ihm wird nicht abgelegt (Absatz „Ein Tankbuster ist der Cast samt seiner
  Treffer"). Eine Barriere gehört nicht dazu: Ein Treffer, den sie ganz auffängt, trägt seinen vollen Betrag (A449).

**Fehler nur nach unten:**
- Eine Minderung, die kein Text beziffert (unverknüpfter Status, Merkmalswert unterhalb der Merkmalsstufe), wird nicht herausgerechnet.
- Treffer unsichtbarer Helfer tragen die Debuffs des Bosses nicht.
- Eine Messung bei gleicher Stufe ohne die Gegenstandsstufengrenze des Inhalts in einem Inhalt, der nicht über die
  Inhaltssuche kam: Der Anteil fällt kleiner aus und wird abgelegt. Erkannt werden eine Stufe über der des Inhalts und
  eine Gruppe ohne Beschränkung aus der Inhaltssuche (Absatz „Ohne Stufensynchronisation").

Eine Barriere verfälscht nichts: Die Schadenszahl des Spiels enthält, was sie auffing (A449); herauszurechnen ist
nichts.

**Ohne Stufensynchronisation: nicht abgelegt** (A452, A466; `DataCenter.OutOfContentSync`). Steht der Spieler über der
Stufe, auf die der Inhalt synchronisiert (`DataCenter.AboveContentLevel`: `PlayerSyncedLevel` gegen `ClassJobLevelSync` der
`ContentFinderCondition` des Gebiets, gelesen mit `TerritoryInfo` beim Gebietswechsel), zeigt jeder Treffer weniger als im synchronisierten
Lauf – wie viel, hängt vom Abstand der Stufen ab und ist nicht gemessen. Abgelegt, bewertete er eine noch unbewertete
Aktion zu klein, und im nächsten synchronisierten Lauf sparte die Größenregel die Minderung oder der Plan das Bündel.
Deshalb legt keiner der drei Speicher, die eine Größe lernen, eine solche Messung ab: Tankbuster-Tabelle
(„not stored, above the level the content syncs to"), Flächentabelle samt der Bewertung mit 0 (A366) und der DoT-Speicher
(Konzept 08). Ein Inhalt ohne Synchronisationsstufe – die offene Welt – gilt als synchron, wie bisher. Ebenso die
Tiefen Gewölbe: Ihre `ContentFinderCondition` führt keine Synchronisationsstufe (`ClassJobLevelSync` 0 für Palast der
Toten, Himmelssäule, Eureka Orthos und Pilgers Pfad, xivapi 06.11.2026), also lernt RSR dort – die Flächentabelle
führt Valfodr aus dem Pilgerpfad (oben). Ob die Ausrüstungsstärke, die das Gewölbe selbst führt, die Treffer
verschiebt, ist nicht gemessen; eine schwächere zeigt größere Treffer, die sichere Richtung. Dasselbe gilt
in einer Gruppe ohne Beschränkung bei gleicher Stufe: Dort fehlt die Gegenstandsstufengrenze, und jeder Treffer fällt
ebenso kleiner aus. Erkannt wird sie an der Inhaltssuche (`DataCenter.InUnrestrictedParty`, A466):
`ContentsFinderQueueInfo.PoppedContentIsUnrestrictedParty` beschreibt den zuletzt aus der Warteschlange gekommenen
Inhalt, `PoppedQueueEntry` nennt ihn (`ContentsType.Regular` mit der Zeile der `ContentFinderCondition`); nur wenn
das die `ContentFinderCondition` des laufenden Gebiets ist, gilt das Merkmal für ihn. Ein Gebiet ohne eigene Zeile –
die offene Welt – oder ein anderer Inhalt als der zuletzt gekommene lassen das Lernen an. Die Felder sind durch
Reverse Engineering gewonnen (FFXIVClientStructs, Werkzeugquelle); falsch gelesen, schaltete ein Merkmal das Lernen
nur für diesen einen Inhalt ab, und jede nicht abgelegte Messung steht mit ihrem Grund im Protokoll.

Herausgerechnet werden dagegen eine Minderung je Schadensart auf dem Getroffenen (Dark Mind: physisch 10 %, magisch
20 %; `MitigationFactor` über `SelfAgainst`) und eine Erhöhung der maximalen LP, die beim Treffer steht (Kampfrausch,
Great Nebula; `MaxHpRaise`): Der Anteil wird an den aktuellen maximalen LP gemessen (`Watcher.DamageShareOn`) und auf
die maximalen LP ohne Erhöhung umgerechnet.

In allen diesen Fällen fällt der ungeminderte Wert zu klein aus, und jeder spätere Treffer kann ihn nur anheben. Ein
zu kleiner Wert heißt **seit dem Tankbuster-Plan nicht mehr** „das Verhalten ohne Tabelle": Der Plan wählt ein zu
kleines Bündel und hält mit „Spend only …" den Rest zurück – der Tank nimmt den Treffer mit weniger, als die alte
Abwehr gedrückt hätte (Konzept 09, „Gemessener Tankbuster: das geringste Mittel", Grenzen). Unter derselben Option
bleibt die Hilfe von Heilern und anderen Tanks aus, wo der Wert unter einem großen Schild liegt und ihn über der
Einzelheil-Schwelle lässt (Konzept 09, „Hilfe für das Ziel eines Tankbusters", A373). Der erste Treffer einer Aktion hat
keinen Wert.

**Nach oben ausgeschlossen:** ein Treffer eines gestärkten Angreifers. Trägt er `DamageUp`, `PhysicalDamageUp` oder
`MagicDamageUp` (Bezeichner aus dem Statusblatt, `StatusHelper.IsDamageUp`), wird nichts gespeichert, und das
Protokoll schreibt „not stored, the attacker carried a damage-up status". Ein solcher Wert stufte die Aktion zu hoch
ein, und auf ihm zöge die Regel aus Konzept 09 eine Unverwundbarkeit. Stärkungen unter anderem Namen bleiben
unerkannt.

**Nach oben ausgeschlossen, seit A312:** ein Tankbuster auf einen Nicht-Tank (Abschnitt „Was gemessen wird"). Werte,
die vorher an einem Nicht-Tank entstanden, sind in der Datei nicht als solche erkennbar und bleiben; in seinen
Protokollen vom 13. bis 17.10.2026 ist keiner belegt (nur das vom 17.10. trug die Kennzeichnung).

### Wofür

- **Tankwechsel (Konzept 09):** „eine Wiederholung bringt dich um" rechnet mit dem Höheren aus dem härtesten
  Treffer dieses Kampfes auf dich und der Vorhersage der Tabelle: ungeminderter Wert mal der Minderung, die jetzt
  auf dir und auf dem Boss liegt. Gleiches gilt für die Prüfung, ob der Empfänger selbst in Gefahr ist. Damit
  greift der Tankwechsel schon beim ersten Tankbuster eines späteren Abends und mit Messungen vom Co-Tank.
- **Der Tankbuster-Plan (Konzept 09, „Gemessener Tankbuster: das geringste Mittel"):** Für einen gewirkten
  Tankbuster wählt er das billigste Bündel eigener Abwehr, das ihn übersteht, die Unverwundbarkeit nur, wenn keines
  reicht, und hält den Rest zurück. Hier entscheidet die Tabelle über Abklingzeiten und darüber, ob die
  Unverwundbarkeit fällt. Dafür rechnet `PredictedShare` mit einem Horizont: Eine Minderung, die vor dem Einschlag
  endet, zählt nicht. Sein Vorschlag vom 04.10.2026, bei sicher tödlichem Tankbuster nur die Unverwundbarkeit zu
  nehmen, ist dort Regel; die Gegenrechnung steht dort unter „Grenzen".

**Ablesbar:** Die Liste zeigt neben der Tankbuster-Liste je Eintrag den ungeminderten Anteil, die Zahl der
bewerteten Tankbuster und „Store:" in derselben Form wie die Flächentabelle. `DefenseTrace.log` schreibt jede
Messung („tankbuster measured: … mitigation factor …, unmitigated … - stored").

### Speichern: welche Fehler der Flächentabelle hier ausgeschlossen sind

Gemeinsamer Weg, nicht kopiert: ein Eintrag `LearnedFile` in der Liste aller Speicher (`Files`), Laden über
`ReadFile` mit einem Bericht für das Listenfenster, Speichern über `SaveTracked<T>` mit einer Zusammenführung je
Speicher. Die Flächentabelle nutzt denselben Weg.

| Fehler der Flächentabelle | Hier |
|---|---|
| Nicht geladen, weil nur in `Init` eingetragen (A121) | in der einen Liste `Files`, die Laden und Speichern beide durchlaufen; `check_config_store_roundtrip.py` prüft, dass nichts an ihr vorbei gespeichert wird |
| Speichern überschreibt die Datei mit weniger (A172) | jedes Speichern führt Datei und Speicher zusammen, je Wert das Höhere |
| Datei beim Speichern unlesbar | nicht überschrieben, „NOT SAVED" |
| Laden nicht zu Ende gelaufen, dann Entladen | nicht geschrieben |
| Unlesbare Datei beim Start | als `.corrupt` beiseitegelegt, im Protokoll gemeldet, im Listenfenster „LOAD FAILED" |
| Halb geschriebene Datei nach Absturz | über eine temporäre Datei geschrieben |
| Zwei Speicherungen gleichzeitig auf dieselbe `.tmp` | eine Sperre je Speicher |
| Kopie auf dem Pool-Thread, während der Effekt-Handler schreibt (A224) | Kopie auf dem aufrufenden Thread, unter der Sperre der Tabelle, auch im Gesamtspeichern |
| Messung zwischen letztem Speichern und Entladen | Effekt-Handler wird vor dem letzten Speichern abgehängt (gilt für alle Speicher) |
| Erfolg ohne Beleg | die Datei wird zurückgelesen, „SAVE MISMATCH" bei Abweichung |
| Neu: Lesen und Schreiben im Speicher aus zwei Threads | jeder Zugriff unter `TankbusterPotentialGate` |

Verworfen wird die Tabelle von Hand bei geschlossenem Spiel, wie die Flächentabelle (seine Vorgabe, keine
Schaltfläche).

## Grenzen der Messung

**Gemessen unter Minderung: zurückgerechnet, soweit die Wirktexte sie beziffern** (A441, `Watcher.TryAreaShare`). Der
Anteil eines Mitglieds wird durch das geteilt, was die Minderung beim Einschlag durchließ – auf ihm und auf dem Wirker
nach Schadensart, mit den Werten der Tankbuster-Tabelle (`TankbusterTable.MitigationFactor`, `DefensiveValues`) –, auf
seine maximalen LP ohne Erhöhung umgerechnet und höchstens als ganze Maximalgesundheit abgelegt. Gelesen wird er unter der
Minderung, die beim Einschlag noch steht (`DataCenter.AreaShareNow`): auf dem Mitglied, das sie am wenigsten schützt,
und auf dem Wirker; eine, die vor dem Ende des Casts ausläuft – bei einer Serie vor ihrem letzten Treffer –, zählt
nicht. Beleg: Seelenüberfluss (#37707) traf ihn am
26.10.2026 mit 25 % unter Vollkommener Ablass, Mäßigung und Taktiker; zurückgerechnet rund 34 % (Taktiker 10 %) oder
36 % (ab Stufe 98 15 %), und Glockenspiel blieb mit dem kleinen Wert liegen (A419). Bis dahin war der Messwert Schaden
nach Minderung, und die Höchstwertregel federte das nur ab. Die Rückrechnung geht auf seinen Einwand zurück (A26:
„wer den absoluten Wert hat, kann die Störfaktoren herausrechnen"); verworfen war sie, solange nur
`GetCurrentMitigationPercent` bereitstand, eine Aufzählung von vier Status. Die Wirktextwerte der Tankbuster-Tabelle
haben diesen Grund aufgehoben.

Was bleibt, korrigiert sich: Eine Minderung, die kein Wirktext beziffert, wird nicht herausgerechnet; der Wert fällt
zu klein aus, und der nächste Treffer unter weniger davon hebt ihn. Eine Barriere braucht keine Rückrechnung: Die
Schadenszahl enthält, was sie auffing (A449). Eine Messung über der Stufe des Inhalts wird nicht abgelegt (Abschnitt
„Die Tankbuster-Tabelle", „Ohne Stufensynchronisation"). Ein Wert, der vor A441 abgelegt wurde, ist der Treffer, wie er ankam, oft unter Minderung; er wird gelesen wie
damals, ohne die jetzt stehende Minderung ein zweites Mal abzuziehen, bis die Aktion neu gemessen ist
(`OtherConfiguration.AreaPotentialUnmitigated`, A446). Sonst las RSR Seelenüberfluss mit 25 % unter 10 % fremder
Minderung als 22,5 %, unter der Größe einer großen Barriere, und die Größenregel konnte die Minderung auslassen, die sie
vorher gab.
Eine Minderung anderer Spieler, die beim Einschlag steht, macht den erwarteten Treffer kleiner, und die Größenregel
spart dann die eigene, solange der Rest niemanden unter die Heilschwelle drückt und unter einem großen Schild bleibt;
eine eigene, eben gelegte Minderung ebenso. Das ist die Frage der Einstellung („whose measured damage would leave
everybody above …"), am Treffer, wie er landet.

**Eine unterschätzte Aktion wird nicht gemindert, und in einem Savage-Kampf ist das ein Wipe.** Der
ernsteste Einwand, und für synchronisierte Messungen trägt er kaum: Die bezifferte Minderung ist herausgerechnet
(Abschnitt „Grenzen der Messung", erster Absatz), eine Barriere verfälscht nichts (A449); was bleibt, ist eine
unbezifferte Minderung. Selbst ungerechnet läge ein Raidwide mit vierzig Prozent Potential unter zehn bis zwanzig
Prozent Gruppenminderung bei zweiunddreißig bis sechsunddreißig — weit über jeder Bagatelle. Eine Verwechslung verlangte
eine Minderung von neunzig Prozent, die es nicht gibt. Für eine Messung über der Stufe des Inhalts hielte der Einwand;
sie wird deshalb nicht abgelegt (A452), ebenso eine Messung in einer Gruppe ohne Beschränkung (A466).

**Verwundbarkeitsstapel verfälschen nach oben.** Zutreffend, und die Fehlerrichtung ist die sichere:
Überschätzung kostet eine Abklingzeit, Unterschätzung ein Gruppenmitglied.

**Die Rechnung misst am falschen Mittel.** Was `DefenseArea` auslöst, ist überwiegend prozentuale
Minderung und nicht Absorption, während „Puffer minus Einschlag" Absorption beschreibt. Zutreffend und
unauflösbar — aber folgenlos: Die Rechnung sagt nicht die Wirkung der Minderung voraus, sondern
beantwortet, **ob** dieser Einschlag jemanden in Gefahr bringt. Prozentuale Minderung wirkt zudem bei
großem Schaden am stärksten, also dort, wo die Rechnung sie auslöst.

**Ein verfehlter, aber überlebbarer Treffer bewertet den Cast zu hoch.** Seit A298 bleibt nur der Treffer außen
vor, der die ganze Gesundheit samt Barriere erreicht. Wer eine Mechanik verfehlt und überlebt – zu weit innen
bei Nekrotische Gefahr, deren Schaden zum Rand hin abfällt, oder zu nah am Einschlag von Bestialisches Brüllen, das
BossModReborn als Raidwide und zugleich als Fläche von 25 Yalm um den Einschlag führt –, setzt den Anteil mit seinem
Treffer; die Gruppe bekommt dann Vorausheilung und Minderung für
einen Treffer, den ein gut Stehender nicht braucht. Fehlerrichtung sicher. Ein Abschlag ließe sich nicht ohne
erfundene Zahl bilden, und welcher Treffer der richtig stehende war, sagt das Spiel nicht.

**Serien kleiner Einschläge:** Eine gelistete Aktion mit Wirkzeit wird als Serie gemessen („Eine Serie zählt als ein
Angriff"). Die meisten Serien in seinen Kämpfen sind aber Sofortaktionen ohne Wirkzeit (Tosende Hyperkanone,
Pechschwarzer Pfad, Dimension Null; Aktionsblatt, Cast100ms 0): Die misst die Tabelle gar nicht, sie kommen nicht in
die Liste und bleiben unbewertet, also gemindert wie vor der Tabelle. Angekündigt werden sie nur über
BossModReborn. Was die Glocke des Weißmagiers aus ihnen macht, zählt `HitSeries` getrennt (Konzept 08).

**Der VFX-Zweig umgeht die Rechnung.** `IsHostileCastingAOE` erkennt auch Stack- und Spread-Marker
über Effektpfade statt über Aktions-Ids; für die gibt es kein Potential, also greift die Rechnung dort
nicht. Konsistent mit „unbewertet heißt mindern", aber benannt: Diese Auslöser bleiben grob.

**Ob die Rechnung im Kampf auslöst,** zeigt die Sonde in der Listenverwaltung (Antwort, Tabelle): Bestand,
härtester Einschlag, Anteil je Eintrag und die Aktionen, deren Minderung unterblieb. Eine Rechnung, die nie feuert,
fällt nach dem Bauen sonst nicht mehr auf.

## Was der Baustein eröffnet

**Er schließt die letzte benannte Lücke der Laufzeitbeobachtung.** Konzept 08 führt als verbliebene
Grenze die Blindheit vor dem **ersten** Treffer eines Pulls und sieht dafür eine Hochrechnung aus
Statussätzen vor — eine gepflegte Tabelle mit genau der Alterung, die dieses Projekt sonst vermeidet.
Ein angekündigter Cast mit bekanntem Potential **ist** die Vorausschau auf den ersten Treffer, und sie
kommt ohne Statussätze aus.

**Er macht die Gefahrenfrage mitgliedsgenau.** `ObjectHelper.IsUnderThreat` fragt über
`IsHostileCastingAOE`, ob ein unbewerteter oder ein als lohnend bewerteter Flächencast läuft — eine Antwort
für die ganze Gruppe („bringt er irgendwen unter die Schwelle"). Mit dem Potential wird daraus „bringt dieser
Einschlag **dieses** Mitglied unter die Heilschwelle"; dann hielte ein Treffer, der nur den Gerezzten
gefährdet, die Notfall-Vollheilung nicht für den gesunden Tank frei. Nicht gebaut.

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
**nicht gebaut**, und der Nutzen ist nicht bloß Diagnose: Ein Anteil, den eine Barriere oder eine unbezifferte
Minderung zu niedrig hielt, bleibt zu niedrig, bis ein Treffer unter weniger davon ihn anhebt; ein Anteil, den ein
Fehlschlag oder ein gestärkter Wirker zu hoch setzte, bleibt zu hoch. Die bezifferte Minderung rechnet die Messung seit
A441 heraus.

**Wer den Verbraucher baut, löst mehr als eine Frage.** Welche das sind und welcher Baustein wie viele
offene Punkte zugleich schließt, steht in `08-mitigation-synergy.md`, Abschnitt „Was ein Baustein
mehrfach trägt“.

**Für die Tankbuster ist dieselbe Frage beantwortet** (Abschnitt „Die Tankbuster-Tabelle").
`HostileCastingKnockback` und `HostileCastingStop` haben dieselbe Struktur, aber keinen eigenen Speicher und keine
Rechnung, und sie brauchen keine (A340): Die Rückstoßliste öffnet nur den Rückstoßschutz (`AutoStatus.AntiKnockback`),
die Stoppliste nur das Innehalten (`AutoStatus.NoCasting`). Beide antworten auf die Mechanik – verschoben werden,
während eines Blicks oder Stopps handeln –, nicht auf den Schaden; ein Rückstoß verschiebt, ob er viel oder wenig
Schaden macht. Keine Schadensabwehr liest die beiden Listen. Macht ein Rückstoß auch Schaden an der Gruppe, steht er
auf der Flächenliste und wird dort gemessen.

**Die Wahl des Mittels nach der Treffergröße — Vorgabe des Auftraggebers:** „man könnte es auch so
anpassen, dass die geeignete schadensverringerung bzw. das geeignete schild bei dem eintreffenden schaden
gewählt wird." Für Flächen ist die Zuordnung „kleiner Treffer, billiges Mittel; großer Treffer, starkes"
geprüft und widerlegt (A118): Minderungen kennen kein „zu groß", sie skalieren mit dem Treffer, und kein
Job hält zwei Anteilsbarrieren zur Wahl. Gebaut ist die zweite Stufe, Heilung vor dem angekündigten
Treffer; die dritte, Barriere und Minderung zusätzlich bei Treffern über der Maximalgesundheit, trägt seit A159 die
Schranke: Sie lässt jede Rückhaltung weichen, sobald der gemessene Treffer ein ungeschütztes Mitglied in Klasse 1
brächte, und ein Treffer über der Maximalgesundheit tut das immer (Konzept 08). Für Tankbuster wählt der Tankbuster-Plan das geringste Bündel, das den gemessenen Treffer
übersteht (Konzept 09). Die Messgröße dieses Konzepts trägt beides: Abgelegt wird der **höchste** Anteil
im Effektsatz, bei gleichem absolutem Schaden der des Spielers mit der geringsten Maximalgesundheit —
die Bezugsgröße, die seine Vorgabe zur Deckung nennt.

**Die Entscheidungsordnung selbst steht in `08-mitigation-synergy.md`**, Vorgabe 5 und der Abschnitt
„Die Antwort auf einen eingehenden Treffer": Deckung in Höhe des Treffers, Heilung zuerst, sobald die
aktuelle Gesundheit nicht reicht, und Barriere samt Minderung zusätzlich, wo auch die volle nicht
reicht. Sie gilt nicht nur für Flächen — dieselbe Frage stellt sich beim Tankbuster —, deshalb steht
sie dort und nicht hier.

## Gelernte Tabellen über Spielversionen: Revision, Absicherung, Anzeige (entschieden, gebaut)

**Seine Aufträge vom 28.10.2026.** Revision: „sobald ein neuer patch von ffxiv rauskommt, könnte sich der wert der
listen aber ändern, weil sich mechaniken im spiel ändern. […] nach patch werden die bestehenden listen als gegeben
angesehen, aber noch nicht als verifiziert. also solange anwendung der bisherigen erkenntnisse, bis durch neue
erkenntnisse (also eine neue revision der liste) diese bestätigt werden oder eben falsifiziert werden und in der neuen
liste als ungültig markiert bzw. in der alten gelöscht werden. das neue revisionssystem muss sicher aufgebaut sein, dass
nicht irrtümlich erkenntnisse gelöscht werden, bzw. alte daten dauerhaft unwidersprochen und ungeprüft nach einem
versionssprung des spiels beibehalten werden." Anzeige: „prüfen, ob es sinnvoll ist, die durch den fork selbst
erstellten lernlisten […] auch in den settings anzeigbar und editierbar zu machen mit den dazugehörigen werten wie name,
gegner, schaden, effekt etc." Seine Rüge vom 29.10.2026 an der ersten Fassung (A443): „was ist mit der vorgabe, dass das
überleben zählt? was ist mit failsafe und hardening?" Seine Prüffälle vom selben Tag: „ein vorher harmloser schlag wird
zum tödlichen onehit umgebaut, der nur mit invul gegegnet werden kann. erkenntnis erst nach tod, bzw. einschlag" und „ein
tödlicher onehit, der bislang invul benötigt hat, ist nun ein harmloser 10% aoe. auch erst bekannt nach einschlag. da
ändert sich sogar die art und ein grppenschild ist fast nicht nötig"; dazu sein Hinweis: „manchmal ist etwas nach
einschlag klar erkennbar." Gebaut wird nach seiner Freigabe: „umsetzung der beiden konzepte erfolgt nach
ausführlicher präsentation und begründung wie das ziel erreicht werden soll." Die eine offene Frage hat er am 29.10.2026
entschieden („asymmetrisch, wie empfohlen", Abschnitt „Seine Entscheidung: asymmetrisch") und danach die drei Defekte
aus seinen Prüffällen beauftragt.

**Berührte Entscheidungen:** seine Vorgabe A173 – Löschen der Datei von Hand, „dazu brauch ich keinen ingame-button.
unnötige funktionen erzeugen auch unnötige fehlerursachen"; meine Absage an eine Selbstkorrektur nach einem Patch
(Abschnitt „Die Werte und ihr Schutz"). Die Revision braucht keinen Knopf; eine Bearbeitung in den Einstellungen wäre
einer, und darüber entscheidet er.

### Maßstab: wofür ein gelernter Wert gelesen wird

Jeder gelernte Wert wird in zwei Richtungen gelesen. **Schützend:** Ein hoher Wert öffnet Minderung, Vorausheilung,
die Glocke, die Unverwundbarkeit, eine Haltesperre. **Einsparend:** Ein niedriger Wert lässt Schutz aus – „Skip
mitigation for small area casts", der Tankbuster-Plan mit „Spend only the mitigation a measured tankbuster needs", die
Hilfe für den anderen Tank, die resistente Gegnerart ohne Schlafversuch, der Marker ohne Treffer. Seine Spielweise –
Sicherheit vor Schaden, gewichtet mit der Wahrscheinlichkeit – gewichtet die beiden Fehler ungleich:

- Ein **zu hoher** Wert kostet Abklingzeit und GCDs, also Schaden. Zweiter Ordnung kostet er Überleben: Eine
  Unverwundbarkeit oder ein großes Bündel ohne Not fehlt dem nächsten Treffer, der es braucht. Wählt der Plan wegen eines
  zu hohen Werts Living Dead statt eines Bündels, das gereicht hätte, und tötet der Treffer ungemindert noch, führt er
  in Walking Dead, und die Heiler müssen die ganze Gesundheit binnen seiner Dauer herstellen (Optionstext „Use the
  invulnerability before a tankbuster nothing less survives"). Ein Schlafversuch steht in der GCD-Folge vor der Heilung
  (`CustomRotation_GCD`, nach Notfall und Unterbrechung).
- Ein **zu niedriger** Wert in einer einsparenden Entscheidung nimmt einem Treffer den Schutz, den er braucht – der
  gefährliche Fehler. Der Tankbuster-Plan mit zu kleinem Wert wählt ein zu kleines Bündel, und der Tank fällt.

Daraus der tragende Grundsatz: **Ein Wert, den die laufende Spielversion und das laufende Messverfahren nicht bestätigt
haben, darf Schutz auslösen, aber keinen einsparen.** Einsparend gelesen gilt er als unbewertet, und unbewertet heißt
überall schon heute: schützen („unbewertet heißt mindern"; „Spend only the mitigation a measured tankbuster needs" gilt
laut Text nur „against a tankbuster the table has measured"; die Hilfe für den anderen Tank geht). Schützend gelesen
gilt er wie bisher – das ist „solange anwendung der bisherigen erkenntnisse" in der Richtung, in der ein Irrtum nur
Schaden kostet. In der einsparenden Richtung weicht der Grundsatz vom Wortlaut seines Auftrags ab; so hat er es
entschieden (Abschnitt „Seine Entscheidung: asymmetrisch"). Alles Übrige ist aus seinen Regeln entschieden und unten
begründet.

### Bestand

| Speicher (Datei) | Leser | schützend gelesen | einsparend gelesen | Fehler, der Leben kostet |
|---|---|---|---|---|
| `HostileCastingAreaPotential` (+ `AreaPotentialUnmitigated`, A446) | `DataCenter.AreaShareNow` | Minderung ab großer Barriere, Vorausheilung, Glocke, Weichen der Rückhaltungen | Größenregel, „angekündigter Treffer klein", BossModReborn-Raidwide gehalten | zu niedrig: Minderung ausgelassen |
| `TankbusterPotential` | `TankbusterTable.PredictedShare` | Bündel, Unverwundbarkeit, Tankwechsel | „nur so viel, wie nötig", Hilfe für den anderen Tank ausgelassen | zu niedrig: zu kleines Bündel; zu hoch: Unverwundbarkeit fehlt dem nächsten tödlichen Treffer |
| `HitSeriesOnPlayer` | `HitSeries` | Glocke auf eine Serie | – | zu niedrig: Glocke fehlt einer Serie |
| `AreaSeriesSpan` (A460) | `DataCenter.AreaShareNow` | nur Minderung, die bis zum letzten Treffer der Serie steht, zählt | – | zu kurz: eine Minderung, die vor dem letzten Treffer endet, zählt für die ganze Summe |
| `DotOnParty` | `DotOnParty.Learned` | HoT gegen DoT, Fortschreibung eines DoT ohne Ende | – | zu niedrig: HoT zu spät |
| `SleepOutcomes` | `SleepInterruptWatch` | „ein Schlaf hat es schon gestoppt" öffnet den Versuch | „Schlaf stoppt es nicht" (`IsUnstoppable`) | zu niedrig/als unstoppbar: Wirken landet |
| `SleepResistantKinds` | `SleepInterruptWatch.LocksOn` | – | kein Versuch auf die Art | Wirken landet; seit Stufe 4 je Version einmal geprüft |
| `SleepDiminishing` | ebenda | – | Versuche über der Grenze unterbleiben | Wirken landet |
| `ActionPenaltyCasts` (A439) | `ActionPenaltyWatch` | kein Zauber in die Landung | – | fehlender oder falsch widerlegter Eintrag: der Tod aus A427 |
| `TankbusterMarkerWithoutHit` | Tankbuster-Erkennung | – | Marker gilt nicht | Tankbuster ohne Abwehr (prüft sich je Auftreten nach, A340) |
| `TankbusterMarkerHits` | Tankbuster-Plan ohne Cast | frühester Zeitpunkt | – | zu spät: Abwehr nach dem Treffer |

Zwei Speicher sind reine Sperren (resistente Art, Schlafgrenze): Sie verhindern die Beobachtung, die sie widerlegen
könnte – der Fall „dauerhaft unwidersprochen und ungeprüft" seines Auftrags. Bis Stufe 4 wurde der Mimik aus A427 nach
einem Widerstand nie wieder versucht, auch wenn ein Patch ihn schlafbar machte; seit Stufe 4 bekommt er in jeder neuen
Spielversion einen Versuch (Abschnitt „Stufe 4", 4b).

### Was einen gelernten Wert entwertet

1. *Eine neue Spielversion* (Patch, Hotfix): Mechanik, Schaden, Immunität, Art des Treffers können sich ändern.
2. *Ein neues Messverfahren von RSR:* A298 (Treffer über die ganze Gesundheit), A312 (Tankbuster nur an Tanks), A337
   (Raidwides aus der Tankbuster-Tabelle), A441 (Fläche ungemindert) änderten die Bedeutung gespeicherter Werte. A446
   zeigt den Preis, wenn das nicht geführt wird: Alte Flächenwerte wurden ein zweites Mal gemindert gelesen.
   A454 (die Treffer eines Casts als Summe statt jedes Treffers einzeln) ändert die Bedeutung ohne Entwertung: Ein
   alter Wert unter der Id eines Casts ist einer seiner Treffer, also eine Untergrenze, und die erste Serie hebt ihn.
3. *Eine Änderung am Server ohne neue Clientversion:* von hier nicht erkennbar. Gedeckt nur in der Richtung, in der der
   Höchstwert sich selbst hebt.
4. *Messfehler in einer Version, nach unten:* ein Stack mit mehr Teilenden als sonst, eine Minderung, die kein
   Wirktext beziffert. Eine Messung über der Stufe des Inhalts oder in einer Gruppe ohne Beschränkung wird nicht
   abgelegt (A452, A466). Eine Barriere gehört nicht dazu: Die Schadenszahl enthält, was
   sie auffing (A449).
5. *Messfehler in einer Version, nach oben:* Schadensplus des Gegners (in der Tankbuster-Tabelle nicht abgelegt),
   Verwundbarkeit (dort getrennt geführt), eine verfehlte, aber überlebte Mechanik. Ein Treffer über der ganzen
   Gesundheit ist in der Flächentabelle ein Fehlschlag (A298), in der Tankbuster-Tabelle die gesuchte Auskunft.

### Seine Prüffälle: was vor dem Einschlag erkennbar ist und was danach

**Vor dem ersten Einschlag** zeigt eine umgebaute Aktion nur, was ihr Cast zeigt: Id, Ziel, Wirkzeit, einen Marker,
eine Vorhersage von BossModReborn. Ihren Schaden rechnet der Server; er steht in keinem Blatt (A258). Kein Verfahren
der Revision ordnet den ersten Treffer nach einem Patch richtig ein. Entscheiden kann es nur, ob ein alter Wert ihn
ungeprüft einsparen darf (Regel 4), und wie schnell der Einschlag den alten Wert ersetzt (Regeln 5 und 5b).

**Sein Beispiel 1: ein harmloser Schlag (15 %) wird ein One-Shot (150 %), den nur die Unverwundbarkeit übersteht.**
- *Erster Cast:* Der alte Wert nennt den Treffer überlebbar, und gegen eine Aktion, die nicht als tödlich gemessen ist,
  gibt RSR keine Unverwundbarkeit („A BossModReborn prediction, an action not yet measured and a marker seen for the
  first time get nothing"). Der Tank stirbt – unter jeder Regel, auch unter dem Grundsatz: Er verbietet dem alten Wert
  das Einsparen, und die Abwehr ohne ihn sind Minderungen, die einen One-Shot nicht abfangen. Gerechnet ein Tod je
  Regel (`learned_revision_model.py`, zweiter Teil). Verhindern ließe ihn nur eine Unverwundbarkeit auf Verdacht
  (Abschnitt „Verworfen").
- *Der Einschlag lehrt:* `TankbusterTable.Record` legt den Treffer ab, auch über der ganzen Gesundheit und auch, wenn
  der Tank an ihm stirbt – es prüft weder das eine noch das andere, und die Grenze aus A298 gilt nur der
  Flächentabelle; dass das Spiel den vollen Betrag eines tödlichen Treffers meldet, zeigt Megatod mit 941 % in seinem
  Protokoll (A298). Der Wert steigt sofort. Ab dem nächsten Cast nimmt die Vorhersage die Aktion als gelernt
  (`TankbusterForecast`), kein Bündel reicht für 150 %, also geht die Unverwundbarkeit; mit zwei Tanks rechnet der
  Tankwechsel mit dem Wert („eine Wiederholung bringt dich um"), wo Liste oder Marker die Aktion nennen. Gerechnet
  danach kein Tod mehr. In der Revision
  steht der Wert in der laufenden Schicht und gilt in beide Richtungen.
- *Ohne Liste und Marker:* Einen umgebauten Schlag ohne beides erkennt RSR nur als Wirken eines Gegners auf sein
  eigenes Ziel. Bis A452 lernte die Tabelle ihn nie, und er tötete bei jeder Wiederholung; jetzt lernt sie ihn mit dem
  Kennzeichen „nur als Wirken auf das Ziel", und der Plan nimmt ihn ab dem zweiten Cast – mit der Unverwundbarkeit, aber
  ohne den Rest zurückzuhalten (Abschnitt „Die Tankbuster-Tabelle", „Auch ohne Liste und Marker"). Bringt ein
  unsichtbarer Helfer den Treffer, lernt sie ihn, wenn er den Namen des Casts trägt (A454).
- *Quarantäne:* Sie gilt je Speicher nach dessen Grenze (Regel 7). Über der ganzen Gesundheit ist ein Tankbuster-Wert
  gültig; sonst bliebe Beispiel 1 auf Dauer tödlich.

**Sein Beispiel 2: ein One-Shot (150 %) wird eine harmlose Fläche von 10 %.**
- *Erster Cast:* Zielt er auf den Tank oder steht ein Tankbuster-Marker auf ihm, nimmt RSR ihn für den alten
  Tankbuster – gelistet oder gelernt (`TankbusterForecast`) – und plant mit 150 %: Die Unverwundbarkeit geht ohne Not.
  Vermeidbar wäre das nur, wenn der alte Wert vor dem Einschlag nicht mehr schützen dürfte – dann stürbe jeder Tank,
  dessen One-Shot ein Patch nicht entschärft hat (Abschnitt „Verworfen"). Zielt der Cast nicht auf ihn und steht kein
  Marker, plant RSR nicht für ihn.
- *Die neue Art ist am Einschlag klar erkennbar:* Der Treffer erreicht jedes Mitglied. Die Aktion kommt in die
  Flächenliste („Record enemy AoE actions", ab Werk an, ab vier Mitgliedern), und von da an übergeht jeder
  Tankbuster-Leser ihren alten Wert (`TankbusterTable.IsPartyWide`) – unter jeder Regel, auch heute. Dieselbe Landung
  misst die 10 % für die Flächentabelle; ab dem zweiten Cast lässt „Skip mitigation for small area casts" die
  Gruppenminderung aus, bei gesunder Gruppe. Das ist sein „ein grppenschild ist fast nicht nötig". In der Revision
  ist die Messung eine der laufenden Version und spart sofort (Regel 4).
- *Unter Unverwundbarkeit:* Steht der Tank beim Einschlag unter Hallowed Ground oder Superbolide, trägt sein Eintrag
  keinen Schaden, sondern die Effektart „unverwundbar". Seit A452 zählt er für die Aufnahme als erreicht, und die erste
  Landung genügt auch dann; bis dahin plante RSR beim zweiten Cast noch einmal das größte Bündel. Unter Holmgang und
  Living Dead nimmt der Tank Schaden.
- *Ohne Aufnahme* – weniger als vier Mitglieder, die Option aus, oder der Treffer bleibt auf dem Tank – senkt den
  alten Wert nur die Revision. Heute nie: Jede bereite Unverwundbarkeit geht auf einen Treffer von 10 %, gerechnet 3 in
  12 Casts (Unverwundbarkeit jeden vierten Cast zurück, Annahme), dazwischen das größte Bündel. Mit der Zählregel
  (Regel 5) erst, wenn die neue Version so viele saubere Messungen hat wie die alte: bei 3 alten Messungen 1, bei 10
  alten weiter 3. Mit dem klaren Abfall (Regel 5b) nach einer Messung: 1 – die erste, unvermeidbare.
- *Die Falle der Unverwundbarkeit:* Unter Hallowed Ground und Superbolide liest RSR keinen Betrag; ein Wert, der die
  Unverwundbarkeit zieht, verhindert so seine eigene Widerlegung. Gebrochen wird die Falle von ihrer Abklingzeit: Der
  nächste Cast trifft gemindert und wird gemessen.

**Was nach dem Einschlag klar erkennbar ist, und was daraus folgt:**

| Am Einschlag erkennbar | woran | Folge | Regel |
|---|---|---|---|
| härter als bekannt | ein Treffer über dem Wert | sofort übernommen, auch über der ganzen Gesundheit | Höchstwert (heute und Regel 5) |
| eine neue Art: trifft alle | jedes Mitglied erreicht, auch unter Unverwundbarkeit | Flächenliste, der alte Tankbuster-Wert wird übergangen | Aufnahme (A452) |
| viel schwächer als ein fester Wert | eine saubere Messung mehr als einen Schadenswurf unter alten, die selbst nicht mehr als einen Wurf streuen | alter Wert widerlegt | 5b |
| schwächer als ein streuender Wert: *nicht* an einem Einschlag | ein Stack, den einmal nur vier teilten, sieht von acht geteilt klein aus | gesenkt erst nach so vielen sauberen Messungen wie die alten | 5 |

### Entwurf

**1. Revisionsmerkmale.** Die Spielversion des Clients samt den Versionen der Erweiterungen (`Framework.GameVersionString`
und `Framework.ExVersions`, FFXIVClientStructs b72bc1f7 vom 25.10.2026, Werkzeugquelle; Dalamud gibt sie Plugins nicht,
1e51469 vom 25.10.2026; Lumina liest die Version der Spieldateien, `Repository.ParseVersion`, öffentlich nicht
dokumentiert) und je Speicher eine Nummer des Messverfahrens im Code, erhöht mit jeder Änderung, die die Bedeutung seiner
Werte ändert. Eine Revision ist das Paar aus beiden. Jeder Hotfix beginnt eine; sie widerlegt nichts, die Werte werden
mit der nächsten sauberen Messung bestätigt. Die Erweiterungen gehören dazu, weil ein Patch auch nur ihre Dateien
bearbeiten kann: Eine Revision zu viel kostet je Eintrag eine Bestätigung, eine zu wenig ließe ab Stufe 2 einen alten
Wert Schutz einsparen.

**2. Eintrag.** Je Eintrag zwei Schichten, die laufende und die übernommene, je mit: Revision, höchster Messung, Zahl der
Messungen, Zahl der sauberen Messungen, niedrigster und höchster sauberer Messung. *Sauber* heißt: ohne Schadensplus am
Gegner und ohne Verwundbarkeit auf dem Gemessenen – beides verfälscht nach oben und weitet sonst die Streuung, an der
Regel 5 die Varianten einer Aktion erkennt. Eine Barriere macht eine Messung nicht unsauber (A449); eine Messung ohne
Stufensynchronisation wird gar nicht abgelegt (Regel 8). Dazu ein dritter Platz für einen widerlegten Wert (gelöscht
wird er nicht, nur nicht mehr gelesen) und der Name des Wirkenden (Blattzeile, für die Anzeige). Mehr Schichten
entstehen nicht: Ein zweiter Versionssprung ohne neue Messung lässt die übernommene Schicht, wie sie ist. Jeder Platz
kommt mit der Stufe, die ihn liest (Regel 12): die beiden Schichten und die Quarantäne mit Stufe 1, der widerlegte Wert
mit Stufe 3, der Name mit Stufe 5 – jeweils als zusätzliches Feld, das eine ältere Datei leer liest (Abschnitt „Stufe 1:
Speicher, Übernahme, Sicherung").

**3. Zustände.** *Übernommen:* Messungen nur aus einer früheren Revision. *Bestätigt:* mindestens eine saubere Messung in
der laufenden Revision. *Gesenkt:* die übernommene Schicht widerlegt (Regeln 5 und 5b). *Quarantäne:* ein ungültiger
Wert (Regel 7).

**4. Anwendung.** Schützend gilt immer das Höhere aus beiden Schichten. Einsparend gilt ein Eintrag nur bestätigt, dann
mit dem Höheren aus beiden Schichten, nach einer Senkung mit der laufenden. Ein übernommener Eintrag gilt einsparend als
unbewertet: Er schützt wie vor der Tabelle, bis eine saubere Messung ihn bestätigt.

**5. Senken nach Zahl: erst wenn die neue Revision mindestens so viel weiß wie die alte.** Die übernommene Schicht ist
widerlegt, wenn die laufende
- mindestens so viele saubere Messungen hat wie die übernommene und mindestens zwei (die wenigsten, die eine Streuung
  zeigen),
- ihre sauberen Messungen mindestens so stark streuen wie die übernommenen, bis auf einen Schadenswurf, und
- ihr Höchstwert um mehr als einen Schadenswurf unter dem übernommenen liegt.

Der Schadenswurf ist ±5 % auf jeden erlittenen Treffer, vor den Minderungen (Allagan Studies, „How to be a Math Wizard",
Ausgabe Shadowbringers, Community-Quelle; für Dawntrail nicht eigens belegt). Was die Regel ausschließt: eine Senkung
nur um den Wurf; eine Senkung durch neue Messungen, die die Varianten der alten noch nicht getroffen haben (ein Stack,
den einmal nur vier teilten); eine Senkung auf eine einzelne Messung einer streuenden Aktion. Ein alter Eintrag ohne
Streuungsangabe zählt als eine Messung ohne Streuung.

**Für die Einträge aus der alten Datei gilt nur der gleichbleibende Abfall** (A475, A482). Ihr Wert ist der Höchstwert
aller Messungen vor Stufe 1, deren Zahl und Streuung unbekannt sind; als eine Messung ohne Streuung übernommen, senkten
ihn die Regeln 5 und 5b schon nach zwei neuen Messungen, die die harte Variante nur noch nicht getroffen haben. Im Upload
vom 01.11.2026 (Prätorium, Nebeltrift) hätten sie vier von 16 neu gemessenen Einträgen gesenkt, ohne dass sich an diesen
Aktionen etwas geändert haben muss: Donnerfunke (#45291) von 56 auf 44 %, Erdbeben (#43327) von 50 auf 41 %, Verstärkte
Sprengung (#28477) von 46 auf 36 %, den Tankbuster Goldene Klauen (#45305) von 44 auf 6 %. Wie weit eine Aktion streut,
zeigen dieselben Kämpfe: Donnerfunke traf das am härtesten getroffene Mitglied mit 4 und dann mit 40 % seiner
Maximalgesundheit, Erdbeben mit 16 und mit 37 %.
- *Was gilt:* Eine übernommene Schicht aus der alten Datei (Verfahren 0) ist widerlegt, wenn die laufende Version
  mindestens drei saubere Messungen hat, alle innerhalb eines Schadenswurfs voneinander und alle um mehr als einen Wurf
  unter dem alten Wert; dann gilt ihr Höchstwert. Eine einzige saubere Messung außerhalb des Wurfs zeigt eine streuende
  Aktion und hält den alten Wert für diese Version. Die Regeln 5 und 5b gelten für solche Schichten nicht.
- *Gerechnet* (`learned_oldfile_model.py`, Annahmen dort benannt, alte Datei aus 3, 10 oder 30 Messungen): Bei einer
  unveränderten, streuenden Aktion lässt die Regel Schutz höchstens einen halben Punkt öfter falsch aus als nie zu senken;
  Regel 5 in der bisherigen Fassung bis 4,4 Punkte öfter, zwei Messungen statt drei bis 1,2 Punkte. Einen abgeschwächten
  festen Treffer gibt sie nach drei Landungen frei; nie zu senken bände seinen Schutz für immer.
- *Warum drei:* die wenigsten, bei denen keine unveränderte Lage des Modells mehr als einen halben Punkt hinter „nie
  senken" zurückfällt.
- *Seine beiden Anforderungen* (28.10.2026): „nicht irrtümlich erkenntnisse gelöscht" – gesenkt wird nur bei
  gleichbleibendem Abfall, und gelöscht wird nichts (der widerlegte Wert bleibt auf seinem Platz, Regel 2); „alte daten
  [nicht] dauerhaft unwidersprochen" – ein fester Treffer wird nach drei Landungen widerlegt. Ein streuender bleibt, solange
  er streut: Das ist der Teil, der sich ohne falsches Senken nicht prüfen lässt.
- *Preis:* Ein streuender Treffer, den ein Patch abschwächte, behält in dieser Version seinen alten Wert (gerechnet in 91
  bis 96 % der Casts Schutz, den er nicht mehr braucht). Ein falsch gesenkter Wert steigt mit dem ersten härteren Treffer
  wieder (Höchstwert); dieser eine kommt ohne die Minderung, die die Größenregel ihm sonst gäbe.
- *Verworfen:* Regel 5 wie für gezählte Einträge; der klare Abfall aus Regel 5b mit zwei Messungen (gerechnet bis 2,5
  Punkte öfter als nie senken); nie senken (gegen „dauerhaft unwidersprochen").

Der Maßstab unten gilt für Einträge, deren alte Messungen gezählt sind.

**5b. Senken nach klarem Abfall: sein „manchmal ist etwas nach einschlag klar erkennbar".** Die übernommene Schicht ist
auch widerlegt, wenn ihre sauberen Messungen – mindestens zwei – nicht mehr als einen Schadenswurf auseinanderliegen und
eine saubere Messung der laufenden Revision um mehr als einen Wurf unter der niedrigsten von ihnen liegt. Ein fester
Treffer, der um mehr fällt, als der Wurf erklärt, ist geändert; mehr Messungen sagen darüber nichts Neues. Bei einer
einzigen alten Messung ist nicht erkennbar, ob die Aktion streut: Dann braucht es zwei neue, die nicht mehr als einen
Wurf auseinanderliegen und um mehr als einen Wurf unter ihr liegen.

Gerechnet (`learned_revision_model.py`, 20 000 Läufe je Fall, Schwelle 25 %, 12 Casts nach dem Wechsel; Annahmen dort
benannt): falsch ausgelassener Schutz je Cast

| Lage | heute | erste Messung ersetzt | gleiche Zahl ersetzt | Regel 5 | Regeln 5 und 5b |
|---|---|---|---|---|---|
| unverändert, 22 %, nur Wurf | 0 % | 0 % | 0 % | 0 % (senkt nie) | 0 % |
| unverändert, Stack 24 % (8, 6 oder 4 Teilende) | 2,2 % | 6,1 % | 3,8 % | 1,4 % | 1,5 % |
| unverändert, 30 %, halb ohne Synchronisation gemessen, ein Drittel kleiner | 2,0 % | 9,1 % | 2,0 % | 0 % | 0 % |
| Patch hebt 18 → 30 % | 8,3 % | 8,3 % | 8,3 % | 0 % | 0 % |
| Patch senkt 30 → 18 % | 0 % (spart nie) | 0 %, spart ab Cast 1 | 0 %, ab Cast 3 | 0 %, ab Cast 3 | 0 %, ab Cast 1 |
| Patch senkt, Stack | 0 % (spart nie) | 5,5 % | 3,8 % | 2,4 % | 3,0 % |

Maßstab der Senkregeln, für Einträge mit gezählten alten Messungen (die übernommenen der alten Datei: oben, A475): Wo
der Treffer nicht schwächer wurde, lassen sie nie öfter Schutz falsch aus als heute; wo ein
Patch ihn schwächte, nie öfter als heute bei jeder neuen Aktion (gerechnet 5,5 % im Stack-Fall) – ein geschwächter
Treffer ist für die Tabelle eine neue Aktion. Regel 5b bleibt in beiden Grenzen. Ihr Preis ist der Stack, dessen alte
Messungen zufällig alle von gleich vielen geteilt wurden: Er sieht fest aus, und nach einer Abschwächung senkt eine
Messung ihn, bevor die seltene Variante wieder getroffen ist – 3,0 % statt 2,4 %. Ihr Gewinn ist die Abklingzeit, die
ein entschärfter Treffer nicht mehr bindet: In Beispiel 2 ohne Aufnahme bei zehn alten Messungen eine Unverwundbarkeit
ohne Not statt drei, und jede gesparte fehlt nicht dem nächsten Treffer, der sie braucht. Der Gewinn aus Regel 4: Nach
einer Erhöhung schützt der erste Cast.

**6. Sperren.** Eine übernommene Sperre gilt als Hypothese und wird gelöst, wo das Lösen Schutz hinzufügt:
- *Marker ohne Treffer:* gilt bis zur nächsten Beobachtung in der laufenden Revision wieder als Tankbuster. Preis: eine
  Abwehr je Marker und Version.
- *Resistente Gegnerart, Schlafgrenze:* ein Versuch je Art und Version, aber nur, wo kein Heilen fällig ist und niemand
  in Gefährdungsklasse 1 steht – der Schlaf steht in der GCD-Folge vor der Heilung, und ein Versuch auf eine
  wahrscheinlich noch immune Art darf keinen Heil-GCD kosten. Widerstand bestätigt, ein gelandeter Schlaf widerlegt.
- *Schlaf stoppt es nicht* (`IsUnstoppable`): übernommen gilt die Aktion als stoppbar, unter denselben Bedingungen.
- *Pyretisch-Wirken:* hält weiter Zauber zurück. Widerlegt erst, wenn die laufende Revision das Wirken öfter auf einem
  getroffenen Mitglied ohne den Status landen sah, als die übernommene es mit ihm sah, mindestens zweimal, und nie mit
  ihm. Ein falsch gelöschter Eintrag ist der Tod aus A427; eine Landung ohne Status kann auch heißen, dass das Wirken
  den Status nur seinem Ziel legt, deshalb zählt nur eine Landung auf dem Ziel des Wirkens.

**7. Absicherung: jeder Fehler endet im schützenden Zustand.**

| Fehler | Zustand danach |
|---|---|
| Spielversion nicht lesbar (Feld verschoben, Client nicht bereit) | Kein Eintrag gilt einsparend als bestätigt; keine laufende Schicht wird verschoben, Messungen gehen in die übernommene – sie schützen und bestätigen nichts; Zeile im Protokoll und im Speicherstand; Versuch bei jeder Messung |
| Inhalt ohne Synchronisationsstufe im Blatt (offene Welt) | gilt als synchron, wie vor A452; Stufe des Spielers und Blatt sind sonst immer lesbar |
| Datei unlesbar | letzte rückgelesene Sicherung (`.bak`, geschrieben nach jedem bestätigten Speichern); erst wenn auch sie fehlt, leer – „nichts gelernt"; die unlesbare bleibt als `.corrupt`. Fehlt die Datei, gilt das: Er hat sie gelöscht, und die Sicherung bringt sie nicht zurück |
| Erster Start mit dem neuen Format | neues Format in neuer Datei; die alte schreibt RSR auf dem bisherigen Weg weiter, als Höchstwert aller gültigen Messungen; das neue Format wird beim ersten Speichern zurückgelesen; misslingt es, bleibt die alte vollständig |
| Ältere RSR-Fassung nach der neuen | liest die alte Datei mit allem Gelernten; was sie dazulernt, übernimmt die neue beim nächsten Start nur als übernommene Schicht, also nur schützend |
| Ungültiger Wert | Quarantäne: bleibt in der Datei, wird nicht gelesen, wird gezählt. Ungültig ist je Speicher, was er nicht ablegen kann: NaN, unendlich, negativ; in der Flächentabelle dazu über der ganzen Gesundheit (A298). Die Tankbuster-Tabelle behält Werte über der ganzen Gesundheit – dort sind sie die gesuchte Auskunft, und Beispiel 1 lebt von ihnen |
| Speichern verlöre einen Eintrag, eine Schicht, eine Messung oder senkte einen Wert ohne Senkung nach Regel 5 oder 5b | nicht gespeichert, „SAVE INVARIANT" im Speicherstand und im Protokoll, der Speicher bleibt im Gedächtnis |
| Absturz beim Schreiben, zwei Schreiber | wie heute: temporäre Datei, Sperre je Speicher, Rücklesen |

**8. Härtung innerhalb einer Version: keine Messung über der Stufe des Inhalts – gebaut (A452).** Eine solche Messung
zeigt weniger als der Treffer, um den es geht, und bewertete eine noch unbewertete Aktion: Im nächsten synchronisierten
Lauf spart die Größenregel die Minderung oder der Plan das Bündel. Gerechnet bei der Hälfte der Landungen ohne
Synchronisation, ein Drittel kleiner (Annahme): 8,3 % falsch ausgelassener Schutz je Cast nach der ersten Messung einer
neuen Aktion, ohne Ablage 0 %. Entschieden nach seiner Spielweise: Nicht abgelegt wird sie, nicht nur einsparend
übergangen – ihr einziger Nutzen wäre ein Wert, den der erste synchronisierte Treffer ohnehin liefert, und eine
Kennzeichnung je Messung kostete Format. Gebaut unabhängig von der Revision, in allen drei Speichern, die eine Größe
lernen (Abschnitt „Die Tankbuster-Tabelle", „Ohne Stufensynchronisation"); ebenso eine Gruppe ohne Beschränkung bei
gleicher Stufe, erkannt an der Inhaltssuche (A466).

**9. Messmittel im Code.** Jeder Übergang – übernommen, bestätigt, gesenkt, Quarantäne, Versuch auf eine Sperre – eine
Zeile im `DefenseTrace.log`; die Kopfzeile zählt je Speicher die Zustände. Die Regel steuert sich selbst; sie wartet auf
keine Ablesung.

**10. Anzeige: ja; Bearbeiten: nein.** Im Listenfenster neben Upstreams Listen je Speicher eine Tabelle: Aktion im
Client-Namen, Wirkender, Gebiet, Wert (Anteil der Maximalgesundheit, Treffer je Serie, Status, Schlafergebnis), Zustand
mit Revision und Zahl der sauberen Messungen, bei einem Tankbuster-Eintrag, den die Flächenliste übergeht, „jetzt
Fläche". Suche und Kopieren wie Upstreams Listen; gezeichnet nur die sichtbaren Zeilen (ImGui-Clipper), Blattnamen
zwischengespeichert, kein Schreiben aus der Oberfläche. Ergebnis seiner Prüffrage, ob Anzeige und Bearbeitung sinnvoll
sind: Die Anzeige ja – er testet, und ohne sie sieht er nicht, warum RSR eine Aktion mindert oder auslässt. Die
Bearbeitung nein, seine Vorgabe A173 bleibt: Den Anlass, für den er die Datei von Hand löscht – ein Patch –, deckt die
Revision selbst. Entfernen nähme bei Pyretisch, Serie und DoT Schutz weg, ein zu niedrig eingetippter Flächen- oder
Tankbusterwert ebenso, ohne dass eine Beobachtung ihn widerlegt hätte; ein Knopf „erneut prüfen" fügte nur Schutz
hinzu, hat aber keinen Fall, den die Revision nicht schon abdeckt, und wäre eine Funktion ohne Bedarf.

**11. Prüfmittel.** Das Modell in der CI (läuft, wenn es sich ändert). Ein Prüfskript nach dem Muster von
`check_emergency_heal_threat.py`: Jeder einsparende Leser geht über den Zugriff „bestätigt", jeder schützende über
„angewandt"; ein direkter Zugriff auf den Speicher außerhalb davon ist ein Befund. Die Invarianten aus Regel 7 im Code,
geprüft bei jedem Speichern.

**12. Bauabfolge, jede Stufe rückbaubar.** (0) Die drei Defekte aus seinen Prüffällen – ungelisteter Tankbuster,
Aufnahme unter Unverwundbarkeit, Messung ohne Synchronisation –: unabhängig von der Revision, gebaut (A452). (1) Format, Übernahme der alten Dateien, Sicherung, Quarantäne, Invarianten – ohne
Verhaltensänderung, die alte Datei bleibt (Abschnitt „Stufe 1: Speicher, Übernahme, Sicherung"). (2) Regel 4 für Fläche
und Tankbuster – gebaut (A481, Abschnitt „Stufe 2"). (3) Regeln 5 und 5b – gebaut (A492, Abschnitt „Stufe 3"). (4) Sperren nach Regel 6 – gebaut (A493, A494, Abschnitt „Stufe 4"). (5) Anzeige – gebaut (A496, Abschnitt „Stufe 5"). Rückbau
einer Stufe: ihr Messverfahren zurück; die Werte bleiben in beiden Dateien.

### Stufe 1: Speicher, Übernahme, Sicherung (gebaut)

**Im Kampf ändert Stufe 1 nichts:** Jeder Leser bekommt denselben Wert wie vorher. Sie legt ab ihrem Einbau jede Messung
unter ihrer Spielversion ab, sodass nach dem nächsten Patch feststeht, was die neue Version schon bestätigt hat – die
Grundlage von Stufe 2 –, und sie prüft den Speicherweg, bevor eine Abwehr von ihm abhängt. Schon jetzt wirkt die
Sicherung: Eine unlesbare Datei startet heute leer, und jede Aktion gilt in dieser Sitzung als unbewertet – Minderung auf
jede Fläche, kein Tankbuster gemessen, Schlafwissen weg; mit der Sicherung gilt der letzte gespeicherte Stand.

**Umfang.** Schichten bekommen die beiden Speicher, deren Werte die Stufen 2 und 3 einsparend lesen: die Flächentabelle
(`HostileCastingAreaPotential`) und die Tankbuster-Tabelle (`TankbusterPotential`). Die Sperrspeicher (Schlaf,
Pyretisch, Marker ohne Treffer) bekommen statt Schichten mit Stufe 4 einen Nebenspeicher mit der Spielversion je Sperre,
weil erst Regel 6 festlegt, was dort eine Beobachtung ist (Abschnitt „Stufe 4");
die nur schützend gelesenen (Serie, Dauer der Flächenserie, DoT, Markertreffer) mit der Regel, die sie einmal senkt –
bis dahin gelten sie wie heute. Das kostet nichts: Ein Speicher, der seine Schichten später bekommt, übernimmt seine Werte dann als übernommen,
und übernommen ist der schützende Zustand; der Preis ist eine Bestätigung je Eintrag mehr. Die Sicherung bekommen alle
gelernten Speicher mit dem Speicherweg `LearnedFile`; die Marker- und Sperrspeicher (`LocalFile`) mit Stufe 4.
`AreaPotentialUnmitigated` bleibt ohne Schichten: Es sagt, wie ein Wert gemessen wurde, nicht, was das Spiel tut, und
ein Patch ändert daran nichts.

**Dateien.** Je Speicher mit Schichten eine neue Datei `<Name>.rev.json`. Die alte `<Name>.json` schreibt RSR auf dem
bisherigen Weg weiter: als Höchstwert aller gültigen Messungen, zusammengeführt mit ihrem Inhalt, zurückgelesen. Eine
ältere RSR-Fassung findet darin alles Gelernte; fallen die neue Datei und ihre Sicherung aus, ist die alte der zweite
Bestand. Ihr Schreibweg nimmt nur Höheres auf, ein Fehler im neuen Weg kann sie also heben, nicht senken. Zurücksetzen
von Hand heißt deshalb: beide Dateien des Speichers löschen; eine allein stellt die andere wieder her.

**Format.** `{"Format": 1, "Entries": {"<Id>": {"Current": …, "Inherited": …, "Quarantined": …}}}`, ein fehlender Platz
ist leer. Jede Schicht: `Game` (Spielversion), `Method` (Messverfahren), `Value` (der Wert in der Form der alten
Datei), `Count` (Messungen), `Clean` (saubere Messungen), `Low` und `High` (niedrigste und höchste saubere, bedeutsam
erst ab einer sauberen). Die Plätze späterer Stufen kommen als zusätzliche Felder (Regel 2); `Format` steigt nur, wenn
eine Datei mit dem neuen Feld von einer älteren Fassung falsch gelesen würde.

**Revision.** Spielversion: der Text aus `GameVersionString` und den Erweiterungen aus `ExVersions` (Heavensward bis
Dawntrail und folgende), verbunden; lesbar nur, wenn der Hauptteil aus Ziffern und Punkten besteht. Das schließt ein
Feld aus, das nach einer Verschiebung Fremdes liest – es hielte sonst jeden Patch für keinen. Ein Platz der
Erweiterungen, der keine Version hält, bleibt außen vor: Der Hauptteil zeigt einen Patch auch allein, und ein
ungenutzter Platz darf die Version nicht auf Dauer unlesbar machen.
Messverfahren je Speicher: 1 ab Stufe 1. Werte der alten Datei tragen die Revision „unbekannt" (leere Spielversion,
Verfahren 0).

**Messung: jede, nicht nur eine Erhöhung.** Bisher schrieb jeder Schreiber nur, was den Höchstwert hob. Jetzt meldet er
jede Messung, die er ablegen würde, mit ihrem Wert und ob sie sauber ist; Höchstwert und Zählung führt der Speicher.
Ohne das bliebe nach einem Patch jede unveränderte Aktion übernommen, bis ein Treffer ihren alten Höchstwert überböte.
- *Fläche:* jede Landung, die einen Anteil über 0 misst, und die Bewertung 0 einer Aktion ohne Wert (A366) – dieselben
  Bedingungen wie bisher, ohne „nur höher". Sauber: der Wirkende ohne Schadensplus und kein gemessenes Mitglied mit
  Verwundbarkeit; die Minderung, die `MitigationFactor` herausrechnet, kennt beide nicht. Eine Verwundbarkeit, die erst
  der Treffer legt, macht die Messung unsauber, wo sie beim Eintreffen schon steht – das zählt eine saubere zu wenig und
  hemmt Regel 5 nur. Die Bewertung 0 einer schadenlosen Aktion ist eine saubere Messung von 0 – anders als ein
  Tankbuster-Treffer, den eine Unverwundbarkeit aufhielt („Null zählt nicht") –, und sie kommt nur als erste: Folgt ihr
  doch Schaden, weitet sie die Streuung und hemmt die Senkung. Einheit ist die Landung, bei einer Serie (A349) also die
  laufende Summe: Teilsummen weiten `Low`
  und erhöhen `Count`, beides hemmt die Senkung in Regel 5 und 5b nur. Stufe 3 behält die Landung als Einheit (A492,
  Abschnitt „Stufe 3", Lagen).
- *Tankbuster:* jede abgelegte Serie (A454) mit ihrer Summe, ihrer Dauer und „nur als Wirken auf das Ziel". Sauber, wenn
  sie nicht unter einer Verwundbarkeit lag; eine unter Schadensplus wird gar nicht abgelegt. `Low` und `High` aus der
  Summe.

**Übergang.** Trägt die laufende Schicht eines Eintrags eine andere Revision als die laufende, wird sie in die
übernommene eingerechnet: der höhere Wert nach der Höchstwertregel des Speichers, die Zählungen addiert, `Low` das
niedrigere, `High` das höhere; die übernommene trägt danach die jüngere Revision. Beim Laden für jeden Eintrag, sonst bei
jeder Berührung und vor jedem Speichern. Ein Versionssprung ohne Messung dazwischen ändert die übernommene Schicht nicht.

**Spielversion nicht lesbar.** Kein Übergang, und jede Messung geht in die übernommene Schicht: Sie schützt sofort,
bestätigt nichts und schiebt keine laufende Schicht beiseite, die vielleicht doch zur laufenden Version gehört. Gelesen
wird bei jeder Messung erneut; wird die Version lesbar, gilt von da an der Übergang.

**Übernahme der alten Datei.** Beim Laden geht jeder Eintrag der alten Datei, der dem Bestand etwas hinzufügt – kein
Platz kennt ihn, oder sein Wert hebt das Höhere aus allen Plätzen –, in die übernommene Schicht, als eine saubere
Messung ohne Streuung (`Low` = `High` = Wert, Regel 5). Beim ersten Start sind das alle; danach nur, was eine ältere
Fassung dazwischen gelernt hat. Der eigene Schreibweg fügt der alten Datei nichts hinzu, was der Bestand nicht kennt –
die Übernahme wiederholt sich also nicht und zählt nichts doppelt.

**Quarantäne.** Ungültig ist: nicht endlich oder negativ; in der Flächentabelle zudem über der ganzen Gesundheit (A298,
bisher beim Laden ausgelassen und aus der alten Datei entfernt). In der Tankbuster-Tabelle gilt das für jede Zahl des
Werts. Ein ungültiger Wert – aus einer Datei oder einer Messung – geht auf den Platz `Quarantined` (letzter Wert,
Zählung), wird nicht gelesen und in der Kopfzeile gezählt. Die alte Datei lässt ihn aus, wie bisher einen Flächenwert
über der ganzen Gesundheit.

**Was die Leser sehen.** Die bisherigen Tabellen bleiben die einzige Leseschnittstelle; ihr Wert je Eintrag ist das
Höhere aus laufender und übernommener Schicht, also der Höchstwert aller gültigen Messungen wie bisher. Kein Schreiber
schreibt sie mehr selbst: Jeder meldet seine Messung dem Speicher, und der setzt den Wert. Zwei Quellen, die
auseinanderlaufen können, gibt es damit nicht; ein Prüfskript hält in der CI fest, dass keine Stelle außerhalb des
Speichers in die beiden Tabellen schreibt, und ein Tankbuster-Wert lässt sich nach seinem Anlegen nicht mehr ändern –
die Schichten, ihre Sicherung und die Tabelle teilen ihn.

**Speichern.** Wie bisher: nur nach vollendetem Laden, zusammengeführt mit der Datei, über eine temporäre Datei,
zurückgelesen, einer zur Zeit. Zusammengeführt wird nach dem Übergang beider Seiten, je Platz: gleiche Revision – der
höhere Wert, je Zählung die höhere, denn beide Seiten stammen aus derselben Geschichte, und eine Summe zählte doppelt;
verschiedene Revision in der übernommenen Schicht – Werte nach der Höchstwertregel, Zählungen addiert, denn eine Zählung
zu viel in der übernommenen hemmt Regel 5 nur, eine zu wenig lockerte sie. Danach die Invarianten gegen die Datei vor
dem Schreiben: Kein Eintrag fehlt, kein belegter Platz ist leer, das Höhere aus allen Plätzen sinkt nicht, die Summe der
Zählungen sinkt nicht. Verletzt eine davon, wird nicht geschrieben (Regel 7). Nach einem Schreiben, dessen Rücklesen
dieselbe Zahl Einträge ergab, eine Kopie als `<Datei>.bak`.

**Laden.** Lesbare Datei – sie. Vorhanden, aber unlesbar – zur Seite als `.corrupt`, dann die `.bak`, erst ohne sie
leer. Fehlt die Datei, gilt das, die `.bak` bleibt ungelesen: Er hat sie gelöscht. Danach die Übernahme der alten Datei,
der Übergang, die Quarantäne, zuletzt die Tabellen für die Leser. Für die alten Dateien aller `LearnedFile`-Speicher gilt
dieselbe Regel für `.corrupt` und `.bak`.

**Protokoll.** Die Kopfzeile des `DefenseTrace.log` nennt je Speicher mit Schichten die Spielversion, die Einträge mit
laufender Schicht, die nur übernommenen, die in Quarantäne und den Ausgang des Ladens samt Übergängen. Eine Zeile je
Eintrag, wenn er bestätigt wird (die erste saubere Messung der laufenden Revision bei vorhandener übernommener Schicht)
und wenn ein Wert in Quarantäne geht; der Übergang beim Laden als Anzahl in der Kopfzeile, nicht je Eintrag – nach einem
Patch wären es Hunderte Zeilen.

**Rückbau.** Die neue Datei wird nicht mehr gelesen und nicht mehr geschrieben; die alte ist vollständig, weil sie
durchgehend geschrieben wurde.

**Prüfmittel.** `learned_store_model.py` spielt zufällige Folgen von Messungen, Speichern, Laden, Versionssprüngen,
unlesbaren Versionen und Dateien, gelöschten Dateien und Läufen einer älteren Fassung und hält: Der gelesene Wert ist der
Höchstwert aller gültigen Messungen (keine Verhaltensänderung); keine Messung geht verloren oder zählt doppelt; die
laufende Schicht enthält genau die Messungen der laufenden Revision; ein gelöschter Speicher bleibt gelöscht; ein
ungültiger Wert wird nie gelesen. Sein Selbsttest baut die Fehler ein, gegen die der Entwurf gebaut ist, und jeder
wird erkannt. Nicht gedeckt ist eine alte Datei, die zuletzt eine ältere Fassung schrieb: Sie legt keine Sicherung an,
und was nur sie hielt, geht mit ihr verloren wie heute. Den Code selbst spielt `revision_harness/run.sh` außerhalb
des Spiels durch dieselben Läufe, samt einem JSON-Rundlauf des Tankbuster-Formats und eingebauten Fehlern als
Selbsttest; nicht in der CI, deren Prüfrechner weder .NET noch Dalamud hat.

**Im Spiel belegt** (sein Upload vom 01.11.2026, Builds d8451be59 und 124fdcb17, A475): Die Spielversion ist lesbar,
sobald er eingeloggt ist; in den Kopfzeilen von Sitzungen, die vor dem Login laden, steht „NOT READABLE", und Messungen
danach tragen die Version doch (zwölf Flächen- und vier Tankbuster-Einträge mit laufender Schicht). Die alte Datei
gleicht in jedem Eintrag dem Höheren aus beiden Schichten, die Sicherungen gleichen ihren Dateien, keine Invariante
schlug an, nichts ging in Quarantäne.

**Was dagegen spricht.**
- *„Eine Stufe ohne Wirkung im Kampf ist Vorratsarbeit."* Nein: Ohne sie hätte Stufe 2 beim ersten Patch nach ihrem
  Einbau keine bestätigten Einträge, und jeder Eintrag zöge noch einmal Schutz, den die Version schon bestätigt hatte;
  die Sicherung wirkt sofort; und der Speicherweg läuft in seinem Spiel, bevor eine Abwehr von ihm abhängt. Bleibt: mehr
  Schreiben – jede gemessene Landung und jede abgelegte Serie speichert beide Dateien, bisher nur eine Erhöhung; bei
  seinen 220 Flächeneinträgen (Stand 25.10.2026) ist die neue Datei mit beiden Schichten je Eintrag rund 124 KB groß,
  geschrieben auf einem Hilfsthread. Seltener zu speichern wäre billiger, verlöre aber bei einem Absturz die Zählungen
  des Kampfes, und Abstürze hatte er allein am 29.10.2026 fünf (A455).
- *„Zwei Dateien je Speicher verdoppeln die Fehlerstellen."* Die alte läuft über den unveränderten Weg und kann nur
  steigen. Die neue prüfen Invarianten bei jedem Speichern und das Modell. Bleibt: Er setzt einen Speicher nur zurück,
  wenn er beide Dateien löscht.
- *„Die Landung als Einheit misst bei Serien Teilsummen."* Wirkt in Stufe 1 nicht; in den Regeln 5 und 5b nur hemmend,
  also im Preis von Abklingzeiten, nicht von Leben.
- *„Zwei Spielclients mit demselben Konfigurationsordner zählen gegeneinander."* Die Zusammenführung nimmt je Zählung
  die höhere, eine Zählung des anderen Clients kann so fehlen: in der laufenden Schicht hemmt das die Senkung, in der
  übernommenen lockert es sie um wenige Messungen. Von hier nicht zu unterscheiden; selten.

**Verworfen.**
- *Das alte Format an Ort und Stelle erweitern:* Eine ältere Fassung läse es nicht, legte die Datei als `.corrupt` ab
  und begänne leer.
- *Die alte Datei nach der Übernahme nicht mehr schreiben:* Eine ältere Fassung verlöre alles seither Gelernte, und ohne
  die neue Datei bliebe nur der Stand vor Stufe 1.
- *Je Eintrag nur eine Versionsmarke statt zweier Schichten:* Der laufende Wert ließe sich nicht vom übernommenen
  trennen; Regel 5 hätte nichts zu vergleichen.
- *Alle Speicher in Stufe 1:* Für Zähler (Schlafausgänge, DoT-Ticks) und Speicher, deren letzte Beobachtung gilt
  (Marker), legt erst die Regel, die ihre Schichten liest, fest, was eine Messung ist; vorher gebaut, legte sie eine
  Bedeutung fest, die Regel 6 noch nicht entschieden hat.
- *Bei unlesbarer Version eine eigene Revision „unbekannt":* Sie schöbe jede laufende Schicht in die übernommene, auch
  wo nur das Feld beim Laden noch leer war, und kein späteres Lesen holte das zurück.
- *Die Sicherung auch bei fehlender Datei laden:* Sie höbe sein Löschen von Hand auf.

**Gegenhypothesen.** *Kein Defekt:* Ohne Revision gilt nach einem Patch jeder alte Wert in beide Richtungen, die
Lage, die er mit „alte daten dauerhaft unwidersprochen" ausschließt. *Option falsch:* geprüft an den Verworfenen;
keine liefert dieselben Daten mit weniger Risiko für den Bestand. *Ausgeliefert, und nichts ändert sich – warum?* So
gewollt im Kampf. Was nicht wirken könnte: Bleibt die Spielversion unlesbar, landet jede Messung in der übernommenen
Schicht, und Stufe 2 bestätigte nie etwas – schützend, und in der Kopfzeile sichtbar.

### Stufe 2: Regel 4 für Fläche und Tankbuster

**Im Kampf.** Ein Wert, den die laufende Spielversion noch nicht sauber gemessen hat, spart keinen Schutz mehr: Eine
kleine Fläche wird gemindert wie eine unbewertete, ein kleiner Tankbuster hält die übrige Abwehr nicht zurück, und
Mitspieler helfen dem Tank wie bei einem ungemessenen. Schützend gilt jeder Wert weiter: ein großer gelernter Raidwide,
ein tödlicher Tankbuster, eine lange Serie. Mit der ersten sauberen Messung der laufenden Version spart er wieder. Seine
Entscheidung (A451, „asymmetrisch, wie empfohlen"); die Rechnung dazu: „Patch hebt 18 → 30 %" sinkt von 8,3 % falsch
ausgelassenem Schutz je Cast auf 0 % (`learned_revision_model.py`, Tabelle unter Regel 5b).

**Bestätigt** heißt: Die laufende Schicht des Eintrags trägt die laufende Spielversion und das laufende Messverfahren und
hat mindestens eine saubere Messung. Ist die Spielversion nicht lesbar, ist nichts bestätigt; gelesen wird sie beim Fragen
erneut, solange sie fehlt. Ein Eintrag ohne laufende Schicht – nur aus der alten Datei oder aus einer früheren Version –
ist übernommen.

**Die Leser, je nach Richtung** (alle Stellen, die eine der Tabellen lesen, A481; die Tabelle gezielter Treffer auf
Nicht-Tanks seit A540):

| Leser | Richtung | übernommen, unbestätigt |
|---|---|---|
| `DataCenter.AreaCastIsWorthMitigating` (Größenregel, „Skip mitigation for small area casts") | spart | wie unbewertet: gemindert |
| `DataCenter.BmrRaidwideIsSmallListedCast` (gehaltener BossModReborn-Raidwide) | spart | nicht klein: nicht gehalten |
| `DataCenter.AnnouncedHitIsSmall` (proaktive Erneuerung wartet) | spart | nicht klein: wartet nicht |
| Rückhalt beim Tankbuster-Plan (`HoldDefenceForTankbuster`, „Spend only the mitigation a measured tankbuster needs") | spart | nichts neben dem Plan gehalten |
| `TankbusterHelp.MeasuredHitCallsForNoHelp` (Hilfe für den Tank bleibt aus) | spart | Hilfe wie bei einem ungemessenen |
| großer unterbrechbarer Cast, große Fläche (`AreaShareNow` gegen die Größe einer großen Barriere) | schützt | gilt |
| angekündigter Anteil: Vorausheilung, Halt für den Treffer, wen er tötet (`AnnouncedAreaShare`) | schützt | gilt |
| Tankbuster-Plan: Bündel, Unverwundbarkeit, „alles" (`TankbusterTable.PredictedShare`) | schützt | gilt |
| Tankwechsel (`TankSwapWatch`), bekannter Tankbuster (`IsLearned`), Seriendauern | schützt | gilt |
| Abwehr gegen einen gezielten Treffer auf einen Nicht-Tank (`TankbusterHelp.AimedHitCallsForDefence`, A540) | spart, wo der Wert klein ist | lohnend: die Abwehr öffnet |
| Gefahr eines Gegners für die Zielwahl allein (`DataCenter.LearnedAreaShare`, `EnemyDanger`, A541) | schützt: ein höherer Wert macht den Wirker nur dringender | gilt |
| Anzeige im Listenfenster (`MainWindow_Learned`, Stufe 5) | zeigt nur | als übernommen benannt |

Ein schützender Leser ist mit einem kleinen übernommenen Wert nie weniger schützend als ohne Wert: Er reagiert erst ab
einer Größe, und ohne Wert reagiert er gar nicht. Deshalb genügt es, die sparenden Leser auf „bestätigt" zu stellen.

**Messmittel.** Lässt ein sparender Leser einen Wert aus, weil er unbestätigt ist, schreibt er eine Zeile ins
`DefenseTrace.log`, einmal je Cast: Aktion, übernommener Wert, „not confirmed in this game version". Die Kopfzeile zählt
je Speicher die bestätigten Einträge neben den laufenden und den übernommenen.

**Prüfmittel** (Regel 11). `check_learned_readers.py` hält in der CI fest: Die Tabellen werden nur in den Dateien gelesen,
deren Leser oben eingeordnet sind; jeder sparende Leser fragt „bestätigt". Ein neuer Leser in einer anderen Datei ist ein
Befund, bis er eingeordnet ist. Selbsttest gegen einen sparenden Leser ohne die Frage und einen Leser in einer neuen Datei.
Den Speicher spielt `revision_harness/run.sh` durch: Nach dem Laden in lesbarer Version ist ein Eintrag genau dann
bestätigt, wenn diese Version eine saubere Messung von ihm hat, und eine saubere Messung bestätigt sofort; ein Speicher,
der schon eine unsaubere Messung als Bestätigung nimmt, fällt im Selbsttest auf.

**Lagen.**
- *Erster Start mit Stufe 2:* Bestätigt ist, was seit Stufe 1 in der laufenden Version sauber gemessen wurde (im Upload vom
  01.11.2026 zwölf Flächen- und vier Tankbuster-Einträge); alles aus der alten Datei ist übernommen, bis es wieder
  landet. Das ist der Preis aus Stufe 1, „eine Bestätigung je Eintrag mehr".
- *Patch oder Hotfix:* Jeder Eintrag ist übernommen, bis er wieder landet; sein Beispiel „Hotfix ohne Änderung" kostet je
  kleinem Raidwide einmal eine Gruppenminderung.
- *Ohne Stufensynchronisation* wird nichts abgelegt (Regel 8), also auch nichts bestätigt: In übersynchronisiertem Inhalt
  bleibt ein übernommener Wert unbestätigt und spart nicht.
- *Unsaubere Landungen* (Schadensplus am Gegner, Verwundbarkeit) bestätigen nicht; ein Gegner, der stets mit Schadensplus
  wirkt, bleibt unbestätigt – schützend.
- *Spielversion nicht lesbar:* nichts bestätigt, alles schützt; die Kopfzeile sagt es.

**Preis.** Je Version und kleiner Aktion beim ersten Auftreten eine Abklingzeit mehr; beim Tank die alte Abwehr neben dem
Plan, und Hilfe eines Mitspielers, die sonst ausbliebe. Bewertet in seiner Entscheidung (A451): Der Preis ist sicher, aber
klein; der Schaden des wörtlichen Wegs selten, aber im Tankbuster-Fall tödlich.

**Verworfen.**
- *Bestätigt erst ab zwei sauberen Messungen:* verdoppelte den Preis und schützt nur gegen eine erste Messung, die zu klein
  ausfällt – das deckt Regel 5 mit Zahl und Streuung, nicht Regel 4.
- *Die schützenden Leser ebenfalls auf „bestätigt" stellen:* nähme einem übernommenen tödlichen Tankbuster die
  Unverwundbarkeit – gegen „schützend gilt immer das Höhere".

**Gegenhypothesen.** *Kein Defekt:* Ohne Stufe 2 spart nach einem Patch jeder alte Wert weiter, auch der eines verstärkten
Treffers; das schließt sein Auftrag aus. *Option falsch:* die Verworfenen. *Ausgeliefert, und nichts ändert sich –
warum?* Bleibt die Spielversion unlesbar, ist nichts bestätigt und nichts spart (schützend, sichtbar in der Kopfzeile);
landet eine Aktion nie sauber, spart sie nie – ebenfalls schützend.

### Stufe 3: Senken nach Regel 5, 5b und gleichbleibendem Abfall

**Im Kampf.** Bis Stufe 3 behielt ein Eintrag nach einem Patch, der seinen Treffer abschwächte, den alten Wert. Er
schützt damit weiter und spart, sobald bestätigt, ebenfalls mit dem Höheren aus beiden Schichten (Regel 4). Ein kleiner
gewordener Raidwide zog dann jedes Mal eine Gruppenminderung, ein entschärfter Tankbuster weiter das große Bündel oder
die Unverwundbarkeit. Mit Stufe 3 widerlegt die laufende Version die übernommene Schicht, sobald eine der Regeln unten
hält. Ab dann gilt der neue Wert, schützend und sparend. Ein Treffer, der wieder härter landet, hebt ihn sofort
(Höchstwert).

**Die Regeln, wie gerechnet** (`learned_revision_model.py`, `learned_oldfile_model.py`). „Ein Schadenswurf" zwischen zwei
Messungen ist das Verhältnis 1,05 / 0,95. Die Streuung einer Schicht ist ihre höchste saubere Messung durch ihre
niedrigste; bei weniger als zwei sauberen ist sie 1. Verglichen werden die Anteile der Maximalgesundheit, beim
Tankbuster der ungeminderte.
- *Schicht aus der alten Datei* (Regel 5, Abschnitt „Für die Einträge aus der alten Datei"): widerlegt, wenn die laufende
  mindestens drei saubere Messungen hat, die höchstens einen Wurf streuen, und ihre höchste saubere um mehr als einen
  Wurf unter dem alten Wert liegt.
  - Als solche gilt jede übernommene Schicht, die einen Wert der alten Datei enthält. Das Kennzeichen geht bei jedem
    Zusammenlegen mit. Beim ersten Laden mit Stufe 3 trägt es jede übernommene Schicht des Verfahrens 0 oder ohne
    Spielversion; die zweite Art kann Messungen bei unlesbarer Version enthalten, die gezählt wären. Die strengere Regel
    ist die sichere Richtung.
- *Gezählte Schicht, Regel 5:* widerlegt, wenn die laufende mindestens so viele saubere Messungen hat wie die
  übernommene und mindestens zwei; wenn ihre Streuung mal einem Wurf mindestens die der übernommenen erreicht; und wenn
  ihr Höchstwert mal einem Wurf unter dem übernommenen liegt.
- *Gezählte Schicht, Regel 5b:* Hat die übernommene mindestens zwei saubere Messungen und streut sie höchstens einen
  Wurf, genügt es, dass die höchste saubere der laufenden mal einem Wurf unter der niedrigsten übernommenen liegt. Mit
  nur einer übernommenen sauberen braucht die laufende zwei, die höchstens einen Wurf streuen und mal einem Wurf unter
  ihr liegen.
- Geprüft wird nach jeder sauberen Messung der laufenden Version und beim Laden. Was vorher schon gemessen war, kann so
  beim ersten Start mit Stufe 3 senken.

**Wohin der widerlegte Wert geht** (Regel 2, der dritte Platz). Die übernommene Schicht geht als Ganzes auf den Platz
„widerlegt": nicht gelöscht, nicht mehr gelesen. Eine spätere Widerlegung kommt dazu.
- *Die alte Datei* hält weiter den Höchstwert aller gültigen Messungen. Geschrieben wird sie aus der Tabelle der Leser,
  angehoben auf alles, was der Eintrag kennt, den widerlegten Platz eingeschlossen. Ohne das schriebe RSR nach dem Verlust
  der alten Datei den gesenkten Wert, und fiele danach auch die Revisionsdatei samt Sicherung aus, wäre der alte Wert
  ganz verloren.
- *Beim Laden* übernimmt RSR den Wert der alten Datei nur, wenn er über allem liegt, was der Eintrag kennt, den
  widerlegten Platz eingeschlossen. Sonst brächte jeder Start den widerlegten Wert zurück.
- *Beim Speichern* geht eine übernommene Schicht der Datei, die dieselbe Geschichte trägt wie die widerlegte, in den
  widerlegten Platz, nicht zurück unter die gelesenen. Der widerlegte Platz der Datei und der des Speichers sind
  derselbe Platz, zweimal gesehen; er wächst nur, also zählt beim Zusammenlegen die höhere Zahl, nicht die Summe.
- *Invarianten* (Regel 7): Der angezeigte Wert darf nur fallen, wenn der widerlegte Platz den alten hält. Keine Messung
  geht verloren; der widerlegte Platz zählt mit.
- *Format 2:* Eine ältere RSR-Fassung ließe den widerlegten Platz beim Schreiben fallen; das Format hält sie davon ab. Sie
  lässt die Revisionsdatei, wie sie ist, und liest die alte Datei (Regel 7, „Ältere RSR-Fassung nach der neuen").

**Messmittel.** Jede Widerlegung schreibt eine Zeile ins `DefenseTrace.log`: Eintrag, Regel, alter Wert, saubere
Messungen der laufenden Version mit ihrer Spanne, der Wert danach. Die Kopfzeile zählt je Speicher die gesenkten
Einträge.

**Prüfmittel.** `revision_harness/run.sh` spielt das gegen den Code:
- Der angezeigte Wert ist der Höchstwert der nicht widerlegten Messungen.
- Keine Messung geht verloren, die widerlegten mitgezählt.
- Ein widerlegter Wert kommt nie aus der alten Datei zurück.
- Vier neue Mutanten muss der Selbsttest fangen: ein Laden, das den widerlegten Platz übersieht; eine Invariante, die ihn
  übersieht; ein Zusammenlegen, das ihn zurückholt; eine alte Datei, die nur schreibt, was die Leser sehen.
- Ein Eintrag wird in den Läufen von einem Patch abgeschwächt, damit Widerlegungen überhaupt vorkommen (300 Läufe, rund
  250 Widerlegungen).

**Lagen.**
- *Kein Patch:* Ein unveränderter fester Treffer streut nur im Wurf und wird nie gesenkt. Ein unveränderter streuender
  (Stack) kann nach Regel 5 sinken. Gerechnet sind das 1,5 % falsch ausgelassener Schutz je Cast statt 2,2 % ohne die
  Revision (Tabelle unter Regel 5b); der nächste harte Treffer hebt ihn wieder.
- *Patch schwächt:* fester Treffer nach einer Landung (5b) oder drei (alte Datei); streuender nach so vielen sauberen
  Messungen wie die alten (5); einer aus der alten Datei, der streut, nie in dieser Version (Preis unten).
- *Unlesbare Spielversion:* Keine Messung kommt in die laufende Schicht, also senkt nichts.
- *Ohne Stufensynchronisation* wird nichts abgelegt (Regel 8), also senkt nichts.
- *Flächenserien* (A349): Abgelegt wird jede Landung mit der laufenden Summe, die Teilsummen weiten die Streuung. Regel
  5b und der gleichbleibende Abfall senken eine Serie deshalb nie. Regel 5 senkt sie erst, wenn die laufende Version so
  viele Landungen sah wie die übernommene und ihre volle Summe um mehr als einen Wurf darunter liegt. Die Serie als
  Einheit war für diese Stufe geplant und ist verworfen: Sie braucht ein Serienende, das das Spiel nicht meldet, und ein
  neues Messverfahren. Dann wäre jede Flächenzahl bis zu ihrer nächsten Landung unbestätigt und spart nicht – je
  kleinem Raidwide eine Gruppenminderung mehr. Der Preis der Landung als Einheit liegt in der sicheren Richtung: eine
  abgeschwächte Serie bleibt länger beim alten Wert.

**Preis.** Ein streuender Treffer aus der alten Datei, den ein Patch abschwächte, behält in dieser Version seinen alten
Wert: gerechnet in 91 bis 96 % der Casts Schutz, den er nicht mehr braucht. Der Stack, dessen alte Messungen zufällig
fest aussahen, sinkt nach einer Abschwächung vor seiner seltenen harten Variante: 3,0 % statt 2,4 % falsch ausgelassener
Schutz je Cast. Beide Zahlen stehen unter Regel 5 und 5b.

**Gegenhypothesen.**
- *Kein Defekt:* Ohne Stufe 3 bliebe ein abgeschwächter Treffer für immer beim alten Wert. Das schließt sein „alte daten
  [nicht] dauerhaft unwidersprochen" aus.
- *Option falsch:* Ein falsch gesenkter Wert steigt mit dem ersten härteren Treffer wieder. Dieser eine Treffer kommt
  ohne die Minderung, die die Größenregel ihm sonst gäbe.
- *Ausgeliefert, und nichts ändert sich – warum?* Hat kein Patch einen Treffer abgeschwächt, gibt es nichts zu senken.
  Die streuenden Einträge der alten Datei senkt der gleichbleibende Abfall nie. Beides ist gewollt; die Kopfzeile zählt,
  was gesenkt wurde.

### Stufe 4: Sperren nach Regel 6

Regel 6 stellt die Sperren unter die Spielversion, in drei Teilstufen, alle gebaut: 4a der Marker ohne Treffer
(Lebensgefahr für den Tank, wenn ein Patch den Marker zum Tankbuster macht), 4b die Schlafsperren (resistente Art,
Schlafgrenze, „Schlaf stoppt es nicht"), 4c das Pyretisch-Wirken. Ihre Speicher sind verschieden gebaut; gemeinsam ist
ihnen ein Nebenspeicher, der je Sperre die Spielversion der letzten entscheidenden Beobachtung hält. Die Schlaf- und
Pyretisch-Dateien werden beim Speichern mit dem Höchst- oder Mindestwert der Datei zusammengelegt, eine Sperre aus ihnen
lässt sich in ihnen also nicht lösen; „widerlegt" steht deshalb im Nebenspeicher. Eine Sperre ohne Eintrag dort stammt
aus der Zeit vor Stufe 4 und gilt als übernommen.

**4a – Marker ohne Treffer (gebaut, A493).**
- *Im Kampf:* Ein Marker, dem in einer früheren Spielversion kein Treffer folgte, zählt nach einem Patch wieder als
  Tankbuster. Das gilt, bis er in der laufenden Version erneut ohne Treffer bleibt; erst dann ist er wieder gesperrt.
  Folgt ihm ein Treffer, verschwindet die Sperre wie bisher.
- *Woran:* Ein Nebenspeicher hält je gesperrtem Marker die Spielversion der letzten Beobachtung ohne Treffer
  (`TankbusterMarkerWithoutHitGame`). Eine Sperre ohne Eintrag dort stammt aus der Zeit vor Stufe 4 und gilt als
  übernommen. Ist die Spielversion nicht lesbar, gilt keine Sperre als laufend (Regel 7), und jeder Marker zählt.
- *Preis:* je Marker und Version einmal die Abwehr, die ein Tankbuster bekommt. Beim ersten Start mit 4a gilt das auch
  ohne Patch: Alle bisherigen Sperren sind übernommen.
- *Messmittel:* je Marker und Sitzung eine Zeile, wenn eine übernommene Sperre nicht greift; die Kopfzeile zählt
  laufende und übernommene Sperren.
- *Gegenhypothese – ausgeliefert, und nichts ändert sich:* Kommt kein Patch und hat er keine Sperren, ändert sich
  nichts. Das ist gewollt.

**4b – Schlafsperren (gebaut, A494).**
- *Im Kampf:* Eine Gegnerart, die einen Schlaf in einer früheren Spielversion oder vor Stufe 4 abwies, bekommt in der
  laufenden einen Versuch: Ein Heiler oder Magier legt einmal einen Schlaf auf ein Wirken dieser Art, das zu stoppen
  lohnt (Konzept 08, „Ein gegnerisches Wirken stoppen"). Weist sie ihn ab, gilt die Sperre bis zum nächsten
  Versionssprung; landet er, ist sie widerlegt, und ihr Wirken wird von da an eingeschläfert wie das jeder Art ohne
  Sperre. Ebenso die Schlafgrenze (ein Schlaf über die gelernte Zahl hinaus) und eine Aktion, deren Wirken ein Schlaf
  nicht stoppte (ein Schlaf auf ihr Wirken).
- *Nur auf einem GCD, den keine Heilung braucht:* Kein Heilzauber ist fällig (Einzel- oder Flächenheilung), und niemand
  steht in Gefährdungsklasse 1 – für Heiler und Magier gleich, denn beide heilen mit dem GCD, und der Schlaf steht in der
  GCD-Folge vor der Heilung. Sonst hält die übernommene Sperre, und das Protokoll nennt den Grund.
- *Seine Vorgabe* „wo Schlaf nicht wirkt, soll er nicht immer wieder versucht werden" (Konzept 08, „Gespeichertes
  Wissen") bleibt gewahrt: ein Versuch je Spielversion, nicht je Kampf oder Sitzung.
- *Ein Versuch je Sperre und Version*, verbraucht mit dem Druck des Schlafs, denn dann ist der GCD ausgegeben: Ein
  Schlaf, der ohne Antwort bleibt (der Gegner fällt, ist gerade unverwundbar, das Wirken endet vorher), verbraucht ihn
  auch. Ein Schlaf auf mehrere übernommene Sperren zugleich (Art und Aktion) verbraucht den Versuch jeder.
- *Was bestätigt, was widerlegt* – jede Beobachtung, nicht nur der Versuch:
  - Art: Abweisung beim ersten Schlaf im Kampf bestätigt. Ein gelandeter Schlaf, gleich von wem, widerlegt, auch in der
    laufenden Version.
  - Grenze: Abweisung nach Schlafen in diesem Kampf bestätigt; in einer Version gilt die kleinste Zahl. Ein Schlaf, der
    auf einem Gegner landet, der in diesem Kampf schon so oft eingeschläfert wurde, wie die Grenze sagt, widerlegt sie:
    Sie liegt höher. Bis zur nächsten Abweisung hat die Art dann keine eigene Grenze; die niedrigste der anderen Arten
    gilt für sie nur, wo sie über der widerlegten liegt, und ihr Zustand ist der ihrer Quelle.
  - Aktion: Ein Wirken, das trotz eines rechtzeitigen Schlafs ausgeht, bestätigt; eines, das ein Schlaf stoppt,
    widerlegt, wie bisher (gestoppt heißt stoppbar).
- *Woran:* Nebenspeicher `SleepLocks` mit je einer Liste für Arten, Grenzen und Aktionen: Spielversion und Ergebnis der
  letzten entscheidenden Beobachtung, bei der Art die Zahl der Abweisungen in der Datei zu diesem Zeitpunkt, bei der
  Grenze ihr Wert (widerlegt: die Zahl der Schlafe, nach denen einer landete), und die Spielversion des Versuchs. Zählt
  die Datei für eine widerlegte Art mehr Abweisungen als bei der Widerlegung – geschrieben von einem zweiten Client oder
  einer älteren RSR-Fassung –, gilt sie wieder als übernommen.
- *Spielversion nicht lesbar:* Eine übernommene Sperre hält wie vor Stufe 4; „ein Versuch je Version" ließe sich nicht
  halten. Im Kampf ist die Version gelesen (A475).
- *Preis:* je Sperre und Version ein GCD auf einen Gegner, der wahrscheinlich noch abweist – Schaden, keine Heilung.
  Beim ersten Start mit 4b gilt das für alle bisherigen Sperren: in seinem Upload vom 02.11.2026 16 Arten, keine Grenze,
  keine Aktion. Eine Art, die nur in manchen Phasen abweist, wechselt zwischen widerlegt und bestätigt; jeder Wechsel
  kostet höchstens einen GCD.
- *Messmittel:* eine Zeile je Versuch (welche Sperren er prüft), je Bestätigung und je Widerlegung; der Grund, wenn eine
  übernommene Sperre hält; die Kopfzeile zählt je Sperrart, wie viele in dieser Version gelten und wie viele widerlegt
  sind.
- *Prüfmittel:* `revision_harness/run.sh` spielt die Zustandslogik (`LearnedLocks.cs`) gegen ein Orakel, das nur die
  Geschichte der Beobachtungen kennt: zufällige Folgen aus Abweisungen, Landungen, Versuchen, Versionssprüngen,
  unlesbaren Versionen und Speichern; mit Mutanten im Selbsttest.
- *Gegenhypothese – ausgeliefert, und nichts ändert sich:* Er trifft die gesperrten Arten selten wieder, und in vollen
  Pulls ist oft Heilung fällig; dann hält die Sperre wie vorher. Das ist der sichere Zustand, und die Kopfzeile zeigt,
  wie viele Sperren laufen.
- *Verworfen:*
  - Alle Schlafsperren bei einem Versionssprung löschen: Jede Art bekäme ihren Versuch ohne die Bedingungen, also auch
    dann, wenn Heilung fällig ist.
  - Ein Versuch je Sitzung statt je Version: ein GCD je Art und Sitzung ohne neue Erkenntnis.
  - Die Datei selbst senken: Das Zusammenlegen mit der Datei holte den alten Wert beim nächsten Speichern zurück.

**4c – Pyretisch-Wirken (gebaut, A494).**
- *Im Kampf:* Ein gelerntes Wirken hält weiter Zauber zurück, auch nach einem Patch. Landet es in der laufenden Version
  mindestens zweimal auf seinem Ziel, ohne den Status zu legen, öfter, als es ihn in früheren Versionen legte, und legt
  es ihn in dieser Version nie, hält es keine Zauber mehr zurück. Legt es ihn danach wieder, gilt es sofort wieder.
- *Was zählt:*
  - Mit dem Status: Die Landung legt einem Mitglied der Gruppe einen Status, der jede Aktion bestraft (der Lernweg,
    A439).
  - Ohne: Das Ziel des Wirkens, wie der Wirkbalken es zeigte, ist ein Mitglied, und die Landung desselben Wirkenden trifft
    es mit Schaden. Kein Mitglied bekommt einen solchen Status, das Ziel trug den gelernten nicht schon, und kein Eintrag
    der Landung auf dem Ziel zeigt Widerstand, Unverwundbarkeit oder einen Status ohne Wirkung.
  - Ein Wirken ohne Ziel in der Gruppe – eine Fläche um den Wirkenden – zählt nie und bleibt gesperrt.
  - Früher gesehen: die Landungen mit Status aller früheren Versionen; für ein Wirken von vor Stufe 4, dessen Zahl nicht
    geführt wurde, eine.
- *Woran:* Nebenspeicher `ActionPenaltySightings` je Aktion: Spielversion, Landungen mit und ohne Status in ihr, die
  früheren mit Status, widerlegt. Ohne lesbare Spielversion zählt nichts, also widerlegt nichts; eine Landung mit Status
  hebt eine Widerlegung auch dann auf.
- *Restrisiko:* Legt ein Wirken den Status nur unter einer Bedingung, die seine Landung nicht zeigt (etwa nur, wer in
  etwas steht), sehen zwei Landungen ohne ihn wie eine Änderung aus, und die nächste mit ihm trifft einen laufenden
  Zauber: der Treffer aus A427, der dann das Wirken sofort wieder lernt. Begrenzt dadurch, dass die laufende Version es
  nie mit dem Status sah, dass mehr Landungen ohne ihn nötig sind, als früher mit ihm gezählt wurden, und dass die
  Ankündigung von BossModReborn (Sondermodus Pyretic) unabhängig davon hält. Wie oft ein Status so gelegt wird, ist
  unbelegt; in seinen Uploads ist noch kein Wirken gelernt (Datei vom 02.11.2026 leer).
- *Preis:* Ein entschärftes Wirken hält bis zur Widerlegung Zauber zurück wie bisher, mindestens zwei Landungen lang.
- *Messmittel:* eine Zeile je gezählter Landung ohne Status mit dem Stand, je Widerlegung und je Aufhebung; die
  Kopfzeile zählt die widerlegten Wirken.
- *Gegenhypothese – ausgeliefert, und nichts ändert sich:* Seine Datei ist leer; bis ein Wirken gelernt ist, ändert 4c
  nichts. Gewollt.

**Sicherung der Sperrspeicher (gebaut, A494).** Die Markerspeicher und die Nebenspeicher der Sperren
(`LocalFile`) legen nach jedem Speichern, das sich zurücklesen lässt, eine Sicherung an (`.bak`) und fallen beim Laden
einer unlesbaren Datei auf sie zurück; eine fehlende Datei holt sie nicht zurück (Regel 7). Ohne sie ginge mit einer
unlesbaren `TankbusterMarkerHits.json` der gemessene Treffer nach einem Marker verloren, und der Plan begegnete einem
tödlichen Marker wie einem ungemessenen. Die übrigen enden auch ohne Sicherung im schützenden Zustand: Jeder Marker
zählt, jede Schlafsperre ist übernommen, jedes Pyretisch-Wirken hält.

**Gegenhypothesen für Stufe 4.**
- *Kein Defekt:* Ohne Stufe 4 bliebe jede Sperre für immer: Die Schlafdateien wachsen nur, und eine gesperrte Art wird
  nie wieder versucht, also nie widerlegt. Das schließt sein „alte daten [nicht] dauerhaft unwidersprochen" aus.
- *Option falsch:* geprüft an den Verworfenen unter 4b; für 4c ist die strenge Widerlegung die Regel 6, wie entschieden,
  und ihr Preis steht oben.
- *Ausgeliefert, und nichts ändert sich – warum?* Ohne Patch und ohne Begegnung mit einer gesperrten Art oder einem
  gelernten Wirken ändert sich nichts. Die Kopfzeile zählt, was läuft, was übernommen und was widerlegt ist.

### Stufe 5: Anzeige (Regel 10, gebaut)

**Im Kampf** ändert die Anzeige nichts. Sie zeigt ihm, warum RSR eine Aktion mindert oder auslässt, einen Gegner nicht
einschläfert, einen Marker übergeht oder einen Zauber vor einer Landung zurückhält: je Eintrag den Wert, den die Leser
sehen, und seinen Zustand unter der Spielversion. Bearbeiten kann er nichts (Regel 10, seine Vorgabe A173).

**Wo:** im Listenfenster ein eigener Abschnitt „Learned tables" neben Upstreams Listen, immer sichtbar, auch wenn die
Abwehr aus ist – Schlaf, Pyretisch und DoT hängen nicht an ihr. Eine Reiterleiste, ein Reiter je Speicher; oben ein
Suchfeld (Name oder Id) und „Copy", das die Zeilen des offenen Reiters als Text mit Tabulatoren in die Zwischenablage
legt, Kopfzeile zuerst.

**Was je Reiter** – gezeigt wird, was der Speicher führt; Wirkender und Gebiet führen die Aktionsspeicher nicht, ihr
Schlüssel ist die Aktion:

| Reiter | Speicher | Spalten |
|---|---|---|
| Area hits | `HostileCastingAreaPotential` mit Revision | Aktion (Client-Name, Id), Anteil, Zustand, Messungen dieser Version |
| Tankbusters | `TankbusterPotential` mit Revision | Aktion, ungemindert, unter Verwundbarkeit, Seriendauer, „aimed only", Zustand, Messungen; „in the AoE list", wenn die Flächenliste ihn führt (A337) |
| Area series | `AreaSeriesSpan` | Aktion, Dauer der Serie |
| Hit series | `HitSeriesOnPlayer` | Aktion, Treffer in Folge |
| Sleep outcomes | `SleepOutcomes` mit `SleepLocks` | Aktion, gestoppt (längste Zeit bis zum Abbruch), ausgegangen (größter Rest), Sperre „Schlaf stoppt es nicht" mit Zustand |
| Sleep kinds | `SleepResistantKinds` mit `SleepLocks` | Gegnerart (Id, Name eines Gegners dieser Art im Umkreis, falls einer da ist), Abweisungen, Zustand |
| Sleep limits | `SleepDiminishing` mit `SleepLocks` | Gegnerart, Grenze, Zustand |
| Damage over time | `DotOnParty` | Gebiet (Client-Name), Status (Client-Name), Ticks, Betrag je Tick, Enden, Laufzeit |
| Penalty casts | `ActionPenaltyCasts` mit `ActionPenaltySightings` | Aktion, Status, Zustand, Landungen mit und ohne in dieser Version, mit vorher |
| Tankbuster markers | `TankbusterMarkerWithoutHit` mit Spielversion, `TankbusterMarkerHits` | Gebiet, Pfad, Sperre „kein Tankbuster" mit Zustand, Treffer nach dem Marker (Aktion, Verzug) |

**Zustand** heißt in den Reitern mit Revision: „confirmed in this game version" (eine saubere Messung der laufenden
Version, Regel 4), „measured in this version, not cleanly yet", „taken over from game version …" oder „taken over from
the file before the revisions", dazu „refuted: was …", wenn eine Widerlegung (Stufe 3) den Wert senkte, und
„quarantined", wenn ein ungültiger Wert ruht (Regel 7). In den Sperren: „holds in this game version", „taken over, one
try left", „taken over, try spent", „refuted"; beim Pyretisch-Wirken „holds spells" oder „refuted". Spielversion nicht
lesbar: Der Kopf des Abschnitts sagt es, und jeder Zustand folgt Regel 7.

**Wie, ohne den Kampf zu bremsen:** Die Zeilen entstehen aus einer Kopie jedes Speichers unter seinem Schloss, höchstens
einmal je Sekunde und nur, solange der Abschnitt offen ist; Aktions-, Status- und Gebietsnamen aus den Blättern werden
zwischengespeichert. Gezeichnet werden nur die sichtbaren Zeilen (ImGui-Clipper). Geschrieben wird nichts.

**Prüfmittel:** `check_learned_display.py` (in der CI): Jeder gelernte Speicher in `OtherConfiguration.Files` hat
einen Reiter; ein neuer Speicher ohne Reiter ist ein Befund. Selbsttest gegen einen Speicher ohne Reiter.

**Verworfen:**
- *Die Werte in Upstreams Aktionslisten anhängen* (wie heute die Anteile der Fläche): Dort stehen nur gelistete
  Aktionen; gelernte ohne Listeneintrag (Tankbuster aus A452, Serien, Schlaf) fehlten, und Upstreams Listencode bekäme
  mehr Fork-Zeilen.
- *Eine Zeile je Messung:* Die Schichten halten Zählungen und Spannen, keine Einzelmessungen; die Einzelnen stehen im
  `DefenseTrace.log`.
- *Bearbeiten, Entfernen, „erneut prüfen":* Regel 10.

**Gegenhypothesen.** *Kein Defekt:* Ohne die Anzeige sieht er die Speicher nur in den Dateien, ohne Namen und ohne
Zustand – seine Frage, warum RSR etwas tut, bliebe auf den Upload angewiesen. *Option falsch:* die Verworfenen.
*Ausgeliefert, und nichts ändert sich – warum?* Im Kampf ändert sich gewollt nichts; nutzt er das Fenster nicht, bleibt
das Protokoll die Quelle.

### Seine Entscheidung: asymmetrisch

**Entschieden am 29.10.2026** (seine Antwort: „asymmetrisch, wie empfohlen"), eine Präzisierung seines Auftrags vom
28.10.2026 für die einsparende Richtung: Ein übernommener Wert gilt schützend wie bisher; einsparend gilt er bis zur
ersten sauberen Messung der neuen Version als unbewertet, also schützend. Die Grundlage, wie sie ihm vorlag:

**Kontext.** Sein Auftrag sagt: nach einem Patch „solange anwendung der bisherigen erkenntnisse, bis […] diese
bestätigt werden oder eben falsifiziert". Seine Spielweise sagt: Sicherheit vor Schaden, gewichtet mit der
Wahrscheinlichkeit. In der einsparenden Richtung widersprechen sich beide, wenn ein Patch eine Aktion verstärkt.
*Betroffene Stellen:* `DataCenter.AreaCastIsWorthMitigating` (Größenregel), `AnnouncedHitIsSmall`,
`BmrRaidwideIsSmallListedCast`, der Tankbuster-Plan unter „Spend only the mitigation a measured tankbuster needs",
`TankbusterHelp.MeasuredHitCallsForNoHelp`, `SleepInterruptWatch.IsUnstoppable`.

**Mechanismus.** *Wörtlich:* Ein übernommener Wert gilt bis zur Bestätigung in beiden Richtungen. *Asymmetrisch
(Empfehlung):* Er gilt schützend; einsparend gilt er bis zur ersten sauberen Messung der neuen Version als unbewertet,
also schützend. Keiner der beiden Wege weiß vor dem Einschlag, was der Patch geändert hat; sie unterscheiden sich nur
darin, ob der alte Wert in dieser Zeit Schutz auslassen darf.

**Kampfbeispiele, ohne Vorwissen.**
- *Sein Beispiel 1, wie er es stellt (15 % → One-Shot von 150 %):* In beiden Wegen gleich. Der erste Cast tötet den Tank,
  weil keine Abwehr außer der Unverwundbarkeit ihn abfängt und RSR sie gegen einen als harmlos bekannten Treffer nicht
  zieht; ab dem zweiten geht sie. Die Frage entscheidet diesen Fall nicht.
- *Beispiel 1 mit einem Treffer, den Minderung abfängt (15 % → 80 % ungemindert, der Tank bei 75 %):* Wörtlich liest der
  Plan die alten 15 %: überlebbar, kein Mittel, und „Spend only …" hält die Einzelabwehr zurück. Der Treffer nimmt 80 %
  von 75 %; der Tank fällt, sofern ihn kein Heiler vorher hochgeheilt hat. Asymmetrisch gilt der alte Wert einsparend
  nicht, und die Einzelabwehr läuft wie bei jedem ungemessenen Tankbuster; ein Mittel von 20 % lässt ihn mit rund 11 %
  stehen, wenn eines bereitsteht. Die Landung misst 80 %; ab dann plant RSR mit ihnen, in beiden Wegen.
- *Raidwide, ein Patch hebt ihn von 18 % auf 30 %:* Wörtlich lässt die Größenregel bei gesunder Gruppe die
  Gruppenminderung aus, alle nehmen 30 %. Asymmetrisch mindert RSR den ersten Cast; gerechnet sinkt das falsche
  Auslassen von 8,3 % je Cast auf 0 %.
- *Sein Beispiel 2 (One-Shot → Fläche von 10 %):* In beiden Wegen gleich: Der alte Wert schützt, die erste
  Unverwundbarkeit geht ohne Not, und die Landung beendet es (Aufnahme oder Regel 5b).
- *Hotfix ohne Änderung, ein Raidwide von 15 %:* Wörtlich spart RSR wie vorher. Asymmetrisch geht beim ersten Cast nach
  dem Hotfix einmal eine Gruppenminderung, danach spart RSR wieder.

**Konsequenzen.** Besser asymmetrisch: Kein Patch, der eine Aktion verstärkt, nimmt ihrem ersten Treffer den Schutz,
den RSR einem unbekannten Treffer gibt. Schlechter asymmetrisch: je Version und kleiner Aktion eine Abklingzeit mehr,
beim ersten Auftreten. Es überwiegt nach seiner Spielweise das Überleben: Der Preis ist sicher, aber klein; der Schaden
des wörtlichen Wegs ist selten (wie oft ein Patch alte Aktionen verstärkt, ist unbelegt), aber im Tankbuster-Fall
tödlich, und dort, wo eine Minderung ihn abgefangen hätte.

**Drei Antithesen gegen die Empfehlung, und was von ihnen bleibt.**
1. *„Sein Auftrag sagt es anders."* Hielt – deshalb war es seine Frage, und er hat sie entschieden. Der Zweck des Wortlauts, gelernte Werte über einen
   Patch hinweg nicht zu verlieren, bleibt gewahrt: Gelöscht wird nichts, schützend gilt alles weiter, und einsparend
   gilt der Wert nach einer sauberen Messung wieder.
2. *„Mitigationspläne verteilen die starken Mittel auf die schweren Treffer zuerst: ‚Do not start at the top of the
   fight and spend chronologically; you will run out of good tools before the hard part.' (Guild Order, ‚writing a
   mit plan')."* Trifft den Preis: Eine Abklingzeit auf einem kleinen Treffer kann einem großen danach fehlen. Bleibt
   begrenzt: nur beim ersten Auftreten je Version, nur für Treffer, die die Größenregel klein nennt; die Abstandsregeln
   (A340) und die Reprisal-Rückhaltung für einen angekündigten Raidwide gelten weiter.
3. *„Unbewertet heißt beim Tank die alte Abwehr – die kann Unverwundbarkeit oder Living Dead ziehen."* Widerlegt am
   Optionstext: Die Unverwundbarkeit gegen einen gemessenen Tankbuster gibt einer ungemessenen Aktion nichts („an action
   not yet measured … gets nothing"); die alte Abwehr nimmt Minderungen.

### Verworfen

- *Die erste neue Messung ersetzt den alten Wert:* gerechnet 6–9 % falsch ausgelassener Schutz bei Stacks und Messungen
  ohne Synchronisation.
- *Gleiche Zahl neuer Messungen ersetzt:* bei Stacks schlechter als heute (3,8 % gegen 2,2 %).
- *Nur Regel 5, ohne klaren Abfall:* lässt einen entschärften festen Treffer so lange Abklingzeiten binden, wie die alte
  Version Messungen hatte – in Beispiel 2 ohne Aufnahme bei zehn alten drei Unverwundbarkeiten ohne Not statt einer.
- *Nie senken, nur bestätigen:* sicher, aber gegen seine Bedingung, dass alte Daten nicht dauerhaft unwidersprochen
  bleiben; ein abgeschwächter Treffer spart dann nie wieder eine Abklingzeit.
- *Gleitendes Maximum, Verfall nach Zeit:* feste Zahlen, und ein selten gespielter Inhalt verlöre seine Werte ohne
  Gegenbeobachtung.
- *Nach einem Patch alles unbewertet, auch schützend:* In Beispiel 2 spräche das alte 150 % nicht mehr für die
  Unverwundbarkeit – richtig für die entschärfte Aktion, tödlich für jede, die der Patch nicht änderte; kostet bei jedem
  Hotfix jede Minderung neu, und das „Anwenden der bisherigen Erkenntnisse" seines Auftrags fiele ganz weg.
- *Unverwundbarkeit auf Verdacht für einen unbekannten oder geänderten Cast, damit Beispiel 1 keinen Tod kostet:* Sie
  ginge auf jeden neuen Cast und fehlte dem echten One-Shot danach; der Optionstext nimmt ungemessene Aktionen deshalb
  aus.
- *Ein Abdruck der Blattzeile (Wirkzeit, Form, Reichweite), der eine Änderung vor dem Einschlag zeigt:* Er zeigt eine
  Bearbeitung, nicht ihre Richtung. Einsparend ist nach einem Versionssprung ohnehin alles übernommen (Regel 4);
  schützend darf er nichts wegnehmen (Beispiel 2, die erste Unverwundbarkeit); hinzufügen könnte er nur eine
  Unverwundbarkeit auf Verdacht.
- *Sauber heißt ohne Barriere:* widerlegt, die Schadenszahl enthält, was die Barriere auffing (A449).
- *Messung ohne Synchronisation ablegen und nur einsparend übergehen:* Begründung in Regel 8.
- *Revision je Gebiet:* Das Spiel sagt nicht, welchen Inhalt ein Patch berührt.
- *Ein Knopf „nach Patch neu prüfen":* Die Revision erkennt den Patch selbst (A173).
- *Entfernen und Eintippen in der Anzeige:* Begründung in Regel 10.

### Was gegen den Entwurf spricht

- *„Regel 4 kostet nach jedem Hotfix Abklingzeiten."* Je kleiner Aktion einmal, bis zur ersten sauberen Messung –
  dasselbe, was RSR vor der Tabelle bei jeder Aktion tat. Dagegen steht der erste Cast nach einem Patch, der eine Aktion
  stärker macht: gerechnet 8,3 % falsch ausgelassener Schutz heute, 0 % mit Regel 4. Die Antithese hält für den Schaden,
  nicht für das Überleben.
- *„Kein Verfahren rettet den Tank in Beispiel 1."* Zutreffend für den ersten Cast; der Entwurf sagt nichts anderes. Er
  sorgt dafür, dass es bei einem Tod bleibt; seit A452 auch ohne Liste und Marker, solange der Wirkende selbst trifft.
- *„Regel 5b senkt einen Stack, der nur zufällig fest aussah."* Zutreffend; gerechnet 3,0 % statt 2,4 % falsch
  ausgelassener Schutz je Cast nach einer Abschwächung, unter den 5,5 %, die RSR heute jeder neuen Aktion zumutet.
- *„Ein übernommener hoher Tankbusterwert zieht die Unverwundbarkeit ohne Not."* Beim ersten Cast, unvermeidbar (Beispiel
  2); danach beenden es die Aufnahme oder Regel 5b nach einer Landung, die nicht unter Hallowed Ground oder Superbolide
  liegt.
- *„Die Spielversion ist eine Werkzeugquelle; verschiebt ein Patch das Feld, liest RSR Unsinn."* Eine leere oder
  unveränderliche Zeichenkette ist kein Sprung (Regel 7, nicht lesbar: schützend); eine zufällig wechselnde wäre ein
  Sprung je Start und hält alles im Zustand übernommen – schützend, und im Protokoll sichtbar. Belegt beim Bau: das
  Feld vor dem Laden der Tabellen lesbar, sonst verzögert.
- *„Der Beleg zur Barriere sind vier Treffer aus einem Abend."* Zutreffend; sie sind eindeutig – die LP blieben
  unverändert, der gemeldete Betrag war nicht null –, und die teilweise aufgefangenen zeigen dasselbe. Irrt der
  Beleg, fiele eine Messung unter Barriere zu klein aus: Das ist der Fehler, den Regel 5 mit ihrer Zahl und Streuung
  ohnehin abfängt, und der Höchstwert hebt ihn mit dem nächsten Treffer ohne Barriere.
- *„Ein Schlafversuch auf eine immune Art kostet einen Heil-GCD."* Nur wo kein Heilen fällig ist und niemand in
  Gefährdungsklasse 1 steht; sonst kostet er Schaden, nicht Leben.
- *„Zehn Speicher mit eigenem Format sind viel Code; jeder Fehler darin gefährdet alle gelernten Werte."* Zutreffend; daher
  die Stufe (1) ohne Verhaltensänderung, die unberührte alte Datei, die Invarianten beim Speichern und das Prüfskript.

**Gegenhypothesen.** *Kein Defekt:* widerlegt – die Sperren verhindern ihre eigene Prüfung, A446 zeigt einen Schaden aus
einem nicht geführten Messverfahren, und seine Prüffälle legten drei Defekte des heutigen Codes frei. *Option falsch:*
geprüft an den Alternativen oben, gerechnet am Modell; keine verworfene Regel ist in einer Lage besser für das
Überleben. *Ausgeliefert, und nichts ändert sich:* hält für den ersten Treffer jeder umgebauten Aktion (Beispiel 1), für
Änderungen am Server ohne neue Clientversion und für Einträge, die nie wieder gespielt werden; alle drei wirken nur, wo
gespielt wird, und dort misst RSR. Für einen ungelisteten Tankbuster ohne Marker hält sie noch, wo ein unsichtbarer
Helfer den Treffer bringt.

### Am Code geprüft

Aussage für Aussage, statisch (Selbstprüfung, kein unabhängiges Audit):
- Lade- und Speicherweg je Speicher über `OtherConfiguration.Files` (`LearnedFile`; die Marker- und Sperrspeicher als
  `LocalFile` ohne Zusammenführung, seit Stufe 4 mit Sicherung).
- Leser je Speicher wie in der Tabelle: Fläche über `AreaShareNow` (A441, A446), Tankbuster über `PredictedShare`, Schlaf
  über `SleepInterruptWatch`, DoT über `DotOnParty.Learned`, Serie über `HitSeries`, Pyretisch über `ActionPenaltyWatch`.
- „Unbewertet schützt" je einsparendem Leser: Größenregel (`AreaCastIsWorthMitigating`: ohne Wert `true`),
  BossModReborn-Raidwide (`BmrRaidwideIsSmallListedCast`: ohne Wert nicht gehalten), angekündigter Treffer klein (ohne
  Wert `false`), Tankbuster-Plan (Text von „Spend only …": „against a tankbuster the table has measured"; die
  Unverwundbarkeit ebenso: „an action not yet measured … gets nothing"), Hilfe für den anderen Tank
  (`MeasuredHitCallsForNoHelp` nur über `AimedTankbusterLeaves` mit Messwert).
- Beispiel 1: `TankbusterTable.Record` prüft weder die ganze Gesundheit noch, ob der Getroffene lebt; der Wert steigt
  nur (`raised`). `TankbusterForecast` nimmt einen Cast, der gelistet oder gelernt ist (`IsLearned`), seit A452 auch
  einen, der nur als Wirken auf das Ziel gelernt ist (`TankbusterReading.AimedOnly`), dann ohne Zurückhalten
  (`Announced`).
- Beispiel 2: Aufnahme bei `RecordCastingArea` und mindestens vier Mitgliedern, wenn jedes erreicht ist
  (`reachedMembers == partyMemberCount`, seit A452 auch unter Unverwundbarkeit über `ReachedTarget`). Gemessen wird in
  derselben Landung (`Contains` nach `Add`). `PredictedShare` und `IsLearned` geben für eine Aktion der Flächenliste 0
  und `false` zurück.
- Stufensynchronisation: `DataCenter.OutOfContentSync` (über der Stufe des Inhalts oder Gruppe ohne Beschränkung) an
  allen Messstellen, die eine Größe ablegen (A452, A466).
- Sauberkeit: Schadensplus am Gegner (`CarriesDamageUp`) und Verwundbarkeit (`CarriesVulnerabilityUp`) an beiden
  Messstellen lesbar; die Barriere (`GetObjectShield`) braucht sie nicht mehr, die Grenze aus A298 liest sie weiter. Die
  Kommentare, die die Frage noch offen nannten, sind mit Stufe (0) berichtigt.
- Schlaf vor Heilung in der GCD-Folge (`CustomRotation_GCD`: Notfall, Unterbrechung, `StopEnemyCast`, dann Heilung).
- Stufe 1 (A456): `OtherConfiguration.RevisedStore` – Laden mit `.bak`, Übernahme, Übergang, Messung,
  Zusammenführung, Invarianten, Schreiben mit Rücklesen; `LearnedFile` mit Revision und `.bak` für alle gelernten
  Speicher; die Schreiber über `MeasureAreaShare` (drei Stellen in `Watcher`) und `MeasureTankbuster`
  (`TankbusterTable.Store`); `check_learned_writes.py` hält fest, dass niemand sonst die beiden Tabellen schreibt.
  Zählungen gibt es bei Fläche und Tankbuster ab Stufe 1, bei Serie und Pyretisch mit ihren Stufen; alte Einträge
  zählen als eine Messung.
- Offen bis zur Beobachtung im Spiel: ob das Feld der Spielversion beim Laden schon lesbar ist. Ist es das nicht,
  gehen die Messungen bis zur ersten lesbaren in die übernommene Schicht, und die Kopfzeile sagt es.

Prüfgrad: statisch am Code und an den Quellen, Modell gerechnet, die Barriere an seinen ACT-Mitschnitten gemessen;
Stufen (0) und (1) gebaut und kompiliert, der Code von Stufe 1 außerhalb des Spiels gegen die Läufe des Modells geprüft
(`revision_harness/run.sh`), im Spiel nicht beobachtet; die übrigen Stufen nicht gebaut.

## Betroffene

**Endnutzer.** Mit jedem beobachteten Einschlag wird eine Aktion bewertet; Bagatellflächen verbrauchen keine
Abklingzeiten, die beim nächsten großen Einschlag fehlten. Eine Aktion ohne Messung gilt weiter als lohnend. In
wiederholten Inhalten ist der Weg dorthin einen Durchlauf lang.

**Autoren abgeleiteter Rotationen.** `HostileCastingArea` bleibt unverändert — Typ, Format und
Signatur. Die Ergänzung ist additiv; wer sie nicht liest, merkt nichts.

**Upstream-Pflege.** Die Eingriffe liegen in `Watcher.ActionFromEnemy` und
`DataCenter.IsHostileCastingArea`, beides Upstream-Code mit regelmäßiger Aktivität. Beide sind klein
und stehen als eigene Blöcke; `check_emergency_heal_threat.py` meldet in der CI, wenn ein Merge die
Rechnung oder ihren Rückfall auf „mindern" entfernt.

## Offene Punkte

Sie stehen in `TODO.md` und sind dort unter der Überschrift des Eintrags mit **Konzept:** auf dieses
Dokument gekennzeichnet — an **einer** Stelle statt in zweien, damit keine Kopie altert.
`.github/scripts/audit/check_concept_links.py` listet sie je Konzept und nennt zugleich, wie viele
Einträge überhaupt keinem Konzept zugeordnet sind.

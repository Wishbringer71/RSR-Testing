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
| Messung: höchster Anteil an der Maximalgesundheit je Effektsatz, eine Serie je Mitglied summiert | umgesetzt (Serie: A349) |
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
- Linie (`CastType` 4 und 12, mit Breite): Rechteck vom Wirkenden zum Ziel, ohne Ziel in Blickrichtung;
  `XAxisModifier` ist die volle Breite (Heavy Blast Cannon 37345: Spieldaten Breite 8, BossModReborn „width 8
  rect", halbe Breite 4).
- Kreis und Kegel um den Wirkenden: Abstand vom **Mittelpunkt** des Wirkenden, ohne dessen Trefferkreis.
  BossModReborn setzt keine seiner 3494 Kreisformen um den Trefferkreis des Wirkenden größer
  (BossModReborn `AOEShapeCircle`, geprüft 29.09.2026); gemessen am Trefferkreis verlängerte ein großer Boss jeden
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

**Eine Serie zählt als ein Angriff** (A349, `Watcher.SeriesShare`). Landet eine gelistete Aktion mehrmals, folgt jede
Landung der vorigen also schneller, als der Gegner sie noch einmal wirken könnte (ihre Wirkzeit laut Aktionsblatt),
werden die Anteile je Mitglied addiert und der höchste Summenanteil abgelegt, höchstens die ganze Maximalgesundheit.
Ersticken des Fleisches (6 s Wirkzeit) traf ihn am 19.10.2026 zweimal je viermal im Abstand von 3 s, mit bis zu 24 %
je Treffer und zusammen 49 und 54 %: einzeln ein Treffer, der ihn über der Flächenheilschwelle lässt, zusammen nicht. Der Text von „Skip mitigation for small area casts"
mindert voll, wenn nach dem Treffer Heilung fällig wird; mit dem Einzeltreffer hätte die Regel die Minderung gegen die
ganze Serie gespart. Zwei Gegner, die dieselbe Aktion im selben Moment wirken, zählen ebenso zusammen – sie treffen die
Gruppe zusammen. *Offen* (TODO „Mehrfach treffender Raidwide: Minderung über die ganze Serie"): Die Flächenserie kennt
ihre Dauer nicht, und die Größenregel rechnet die Minderung, die beim Ende des Balkens steht, für die ganze Summe; die
Tankbuster-Serie lernt ihre Dauer seit A454 (Konzept 09, „H_S").

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
erklärt. Bis zu ihrem Bau deckt das Löschen von Hand den Fall.

**Seit A172 kann kein Speichern mehr verlieren, was die Datei hält.** Die Tabelle wächst von selbst nur; also ist jeder Stand der Datei eine Untergrenze, und ein Speichern nimmt je Aktion das Höhere aus Speicher und Datei. Damit ist die Fehlerklasse geschlossen, nicht der Einzelfall: Gleich aus welchem Grund der Speicher leer oder unvollständig ist — Laden gescheitert, abgebrochen, ein künftiger Defekt —, die Datei behält ihren Stand. Dazu wird ein Speicher, dessen Laden in dieser Sitzung nicht zu Ende lief, gar nicht geschrieben: Das Entladen schreibt alle Listen, und ein abgebrochener Start hätte sie sonst alle geleert. Ausgenommen ist nur ein Wert, den die Messung selbst nicht ablegt, über der ganzen Maximalgesundheit (A298, Abschnitt „Was gemessen wird, und was nicht").

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
- Eine Messung bei gleicher Stufe ohne die Gegenstandsstufengrenze des Inhalts (Gruppe ohne Beschränkung): Der
  Anteil fällt kleiner aus und wird abgelegt. Erkannt wird nur eine Stufe über der des Inhalts (Absatz „Ohne
  Stufensynchronisation").

Eine Barriere verfälscht nichts: Die Schadenszahl des Spiels enthält, was sie auffing (A449); herauszurechnen ist
nichts.

**Ohne Stufensynchronisation: nicht abgelegt** (A452). Steht der Spieler über der Stufe, auf die der Inhalt
synchronisiert (`DataCenter.AboveContentLevel`: `PlayerSyncedLevel` gegen `ClassJobLevelSync` der
`ContentFinderCondition` des Gebiets, gelesen mit `TerritoryInfo` beim Gebietswechsel), zeigt jeder Treffer weniger als im synchronisierten
Lauf – wie viel, hängt vom Abstand der Stufen ab und ist nicht gemessen. Abgelegt, bewertete er eine noch unbewertete
Aktion zu klein, und im nächsten synchronisierten Lauf sparte die Größenregel die Minderung oder der Plan das Bündel.
Deshalb legt keiner der drei Speicher, die eine Größe lernen, eine solche Messung ab: Tankbuster-Tabelle
(„not stored, above the level the content syncs to"), Flächentabelle samt der Bewertung mit 0 (A366) und der DoT-Speicher
(Konzept 08). Ein Inhalt ohne Synchronisationsstufe – die offene Welt – gilt als synchron, wie bisher. Verworfen ist das
Flag der Gruppe ohne Beschränkung (`ContentsFinderQueueInfo.PoppedContentIsUnrestrictedParty`, FFXIVClientStructs):
Es beschreibt den zuletzt gestarteten Inhalt, nicht belegt auch den laufenden, und falsch gelesen schaltete es das
Lernen still ab; Stufe und Blatt sind dagegen Spieldaten des laufenden Inhalts.

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
und auf dem Wirker; eine, die vor dem Ende des Casts ausläuft, zählt nicht. Beleg: Seelenüberfluss (#37707) traf ihn am
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
sie wird deshalb nicht abgelegt (A452). Offen bleibt die gleiche Stufe ohne Gegenstandsstufengrenze.

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

## Gelernte Tabellen über Spielversionen: Revision, Absicherung, Anzeige (entschieden, nicht gebaut)

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
| `DotOnParty` | `DotOnParty.Learned` | HoT gegen DoT, Fortschreibung eines DoT ohne Ende | – | zu niedrig: HoT zu spät |
| `SleepOutcomes` | `SleepInterruptWatch` | „ein Schlaf hat es schon gestoppt" öffnet den Versuch | „Schlaf stoppt es nicht" (`IsUnstoppable`) | zu niedrig/als unstoppbar: Wirken landet |
| `SleepResistantKinds` | `SleepInterruptWatch.CanStillBeSlept` | – | kein Versuch auf die Art | Wirken landet, nie wieder geprüft |
| `SleepDiminishing` | ebenda | – | Versuche über der Grenze unterbleiben | Wirken landet |
| `ActionPenaltyCasts` (A439) | `ActionPenaltyWatch` | kein Zauber in die Landung | – | fehlender Eintrag: der Tod aus A427 |
| `TankbusterMarkerWithoutHit` | Tankbuster-Erkennung | – | Marker gilt nicht | Tankbuster ohne Abwehr (prüft sich je Auftreten nach, A340) |
| `TankbusterMarkerHits` | Tankbuster-Plan ohne Cast | frühester Zeitpunkt | – | zu spät: Abwehr nach dem Treffer |

Zwei Speicher sind reine Sperren (resistente Art, Schlafgrenze): Sie verhindern die Beobachtung, die sie widerlegen
könnte – der Fall „dauerhaft unwidersprochen und ungeprüft" seines Auftrags. Der Mimik aus A427 wird nach einem Widerstand
nie wieder versucht, auch wenn ein Patch ihn schlafbar machte.

### Was einen gelernten Wert entwertet

1. *Eine neue Spielversion* (Patch, Hotfix): Mechanik, Schaden, Immunität, Art des Treffers können sich ändern.
2. *Ein neues Messverfahren von RSR:* A298 (Treffer über die ganze Gesundheit), A312 (Tankbuster nur an Tanks), A337
   (Raidwides aus der Tankbuster-Tabelle), A441 (Fläche ungemindert) änderten die Bedeutung gespeicherter Werte. A446
   zeigt den Preis, wenn das nicht geführt wird: Alte Flächenwerte wurden ein zweites Mal gemindert gelesen.
   A454 (die Treffer eines Casts als Summe statt jedes Treffers einzeln) ändert die Bedeutung ohne Entwertung: Ein
   alter Wert unter der Id eines Casts ist einer seiner Treffer, also eine Untergrenze, und die erste Serie hebt ihn.
3. *Eine Änderung am Server ohne neue Clientversion:* von hier nicht erkennbar. Gedeckt nur in der Richtung, in der der
   Höchstwert sich selbst hebt.
4. *Messfehler in einer Version, nach unten:* eine Messung bei gleicher Stufe ohne die Gegenstandsstufengrenze des
   Inhalts, ein Stack mit mehr Teilenden als sonst, eine Minderung, die kein Wirktext beziffert. Eine Messung über der
   Stufe des Inhalts wird seit A452 nicht abgelegt. Eine Barriere gehört nicht dazu: Die Schadenszahl enthält, was
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

Maßstab der Senkregeln: Wo der Treffer nicht schwächer wurde, lassen sie nie öfter Schutz falsch aus als heute; wo ein
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
lernen (Abschnitt „Die Tankbuster-Tabelle", „Ohne Stufensynchronisation"). Nicht erkannt wird die gleiche Stufe ohne die
Gegenstandsstufengrenze des Inhalts: Das Flag der Gruppe ohne Beschränkung beschreibt den zuletzt gestarteten Inhalt,
nicht belegt den laufenden.

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
und Tankbuster. (3) Regeln 5 und 5b. (4) Sperren nach Regel 6, mit den Schichten der Sperrspeicher. (5) Anzeige. Rückbau
einer Stufe: ihr Messverfahren zurück; die Werte bleiben in beiden Dateien.

### Stufe 1: Speicher, Übernahme, Sicherung (entschieden, nicht gebaut)

**Im Kampf ändert Stufe 1 nichts:** Jeder Leser bekommt denselben Wert wie vorher. Sie legt ab ihrem Einbau jede Messung
unter ihrer Spielversion ab, sodass nach dem nächsten Patch feststeht, was die neue Version schon bestätigt hat – die
Grundlage von Stufe 2 –, und sie prüft den Speicherweg, bevor eine Abwehr von ihm abhängt. Schon jetzt wirkt die
Sicherung: Eine unlesbare Datei startet heute leer, und jede Aktion gilt in dieser Sitzung als unbewertet – Minderung auf
jede Fläche, kein Tankbuster gemessen, Schlafwissen weg; mit der Sicherung gilt der letzte gespeicherte Stand.

**Umfang.** Schichten bekommen die beiden Speicher, deren Werte die Stufen 2 und 3 einsparend lesen: die Flächentabelle
(`HostileCastingAreaPotential`) und die Tankbuster-Tabelle (`TankbusterPotential`). Die Sperrspeicher (Schlaf,
Pyretisch, Marker ohne Treffer) bekommen sie mit Stufe 4, weil erst Regel 6 festlegt, was dort eine Beobachtung ist;
die nur schützend gelesenen (Serie, DoT, Markertreffer) mit der Regel, die sie einmal senkt – bis dahin gelten sie wie
heute. Das kostet nichts: Ein Speicher, der seine Schichten später bekommt, übernimmt seine Werte dann als übernommen,
und übernommen ist der schützende Zustand; der Preis ist eine Bestätigung je Eintrag mehr. Die Sicherung bekommen alle
gelernten Speicher mit dem Speicherweg `LearnedFile`; die beiden Markerspeicher (`LocalFile`) mit Stufe 4.
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
Dawntrail und folgende), verbunden; lesbar nur, wenn jeder Teil aus Ziffern und Punkten besteht und der Hauptteil nicht
leer ist. Das schließt ein Feld aus, das nach einer Verschiebung Fremdes liest – es hielte sonst jeden Patch für keinen.
Messverfahren je Speicher: 1 ab Stufe 1. Werte der alten Datei tragen die Revision „unbekannt" (leere Spielversion,
Verfahren 0).

**Messung: jede, nicht nur eine Erhöhung.** Bisher schrieb jeder Schreiber nur, was den Höchstwert hob. Jetzt meldet er
jede Messung, die er ablegen würde, mit ihrem Wert und ob sie sauber ist; Höchstwert und Zählung führt der Speicher.
Ohne das bliebe nach einem Patch jede unveränderte Aktion übernommen, bis ein Treffer ihren alten Höchstwert überböte.
- *Fläche:* jede Landung, die einen Anteil über 0 misst, und die Bewertung 0 einer Aktion ohne Wert (A366) – dieselben
  Bedingungen wie bisher, ohne „nur höher". Sauber: der Wirkende ohne Schadensplus und kein gemessenes Mitglied mit
  Verwundbarkeit; die Minderung, die `MitigationFactor` herausrechnet, kennt beide nicht. Eine Verwundbarkeit, die erst
  der Treffer legt, macht die Messung unsauber, wo sie beim Eintreffen schon steht – das zählt eine saubere zu wenig und
  hemmt Regel 5 nur. Einheit ist die Landung, bei einer Serie (A349) also die laufende Summe: Teilsummen weiten `Low`
  und erhöhen `Count`, beides hemmt die Senkung in Regel 5 und 5b nur. Die Serie als Einheit kommt mit dem offenen
  Punkt „Mehrfach treffender Raidwide" und erhöht das Messverfahren.
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
Speichers in die beiden Tabellen schreibt.

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
und was nur sie hielt, geht mit ihr verloren wie heute.

**Was dagegen spricht.**
- *„Eine Stufe ohne Wirkung im Kampf ist Vorratsarbeit."* Nein: Ohne sie hätte Stufe 2 beim ersten Patch nach ihrem
  Einbau keine bestätigten Einträge, und jeder Eintrag zöge noch einmal Schutz, den die Version schon bestätigt hatte;
  die Sicherung wirkt sofort; und der Speicherweg läuft in seinem Spiel, bevor eine Abwehr von ihm abhängt. Bleibt: mehr
  Schreiben – jede gemessene Landung speichert, bisher nur eine Erhöhung; Dateien von einigen hundert Kilobyte, auf
  einem Hilfsthread.
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
- Lade- und Speicherweg je Speicher über `OtherConfiguration.Files` (`LearnedFile`; die Markerspeicher als `LocalFile`
  ohne Zusammenführung – sie bekommen sie mit Stufe 4).
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
- Stufensynchronisation: `DataCenter.AboveContentLevel` an allen Messstellen, die eine Größe ablegen (A452).
- Sauberkeit: Schadensplus am Gegner (`CarriesDamageUp`) und Verwundbarkeit (`CarriesVulnerabilityUp`) an beiden
  Messstellen lesbar; die Barriere (`GetObjectShield`) braucht sie nicht mehr, die Grenze aus A298 liest sie weiter. Die
  Kommentare, die die Frage noch offen nannten, sind mit Stufe (0) berichtigt.
- Schlaf vor Heilung in der GCD-Folge (`CustomRotation_GCD`: Notfall, Unterbrechung, `StopEnemyCast`, dann Heilung).
- Offen bis zum Bau: das Feld der Spielversion vor dem Laden lesbar; die Zahl der Messungen fehlt heute bei Fläche,
  Tankbuster, Serie und Pyretisch und beginnt mit Stufe 1 – alte Einträge zählen als eine Messung.

Prüfgrad: statisch am Code und an den Quellen, Modell gerechnet, die Barriere an seinen ACT-Mitschnitten gemessen; die
Revision nicht gebaut, Stufe (0) gebaut und kompiliert, im Spiel nicht beobachtet.

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

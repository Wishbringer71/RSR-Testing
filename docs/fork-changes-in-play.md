# Was dieser Fork im Kampf anders macht

Gegenstand ist der Abstand zu `upstream/main` auf dem Stand **7.5.6.9** (`83033ed79`),
gemessen an `f47d2a40a`. Gegliedert ist er nach dem, was im Kampf geschieht — wer wann
wie viel Schaden nimmt, welche Aktion früher oder später fällt, wer überlebt. Die
Herkunftssicht (was war ein Fehler des Originals, was eine Erweiterung, was ein eigener
Fehler) steht in `docs/rotation-flow/06-fork-audit.md`; sie beschreibt den ersten
Durchgang und ist für die spätere Arbeit nicht fortgeschrieben.

**Umfang:** 532 Commits. An C#-Quellen 74 Dateien, +5102/−814 Zeilen
(`git diff --shortstat upstream/main...HEAD -- '*.cs'`); Dokumentation und Prüfskripte
kommen obendrauf und tun im Spiel nichts (§ 9).

---

## Zum Prüfgrad, vorweg

Nur zwei Änderungen sind **im Spiel bestätigt**: die Wiederbelebung (A73, seine Beobachtung
„rezz klappt bislang automatisch") und der Tank-Sustain-HoT (A2). Alles andere ist am Code
belegt und in der CI kompiliert — das sagt, **dass** eine Wirkkette schließt, nicht **ob
sie im Spiel richtig ist**. Wo eine Änderung ab Werk hinter einem Schalter liegt, steht es
dabei; ohne Vermerk wirkt sie sofort.

**Ein belegter Defekt ist in diesem Stand enthalten.** Er steht in § 5.3 und ist der
Grund, warum seit dem 17.09. beim Beschwörer weder Addle noch Schimmerschild bei
Flächenschaden fallen.

---

## 1 · Heilung — wann sie fällt

**Der Pull beginnt nicht mehr ohne Heilung.** `DataCenter.AverageTTK` lieferte im Original
`0f`, solange kein Gegner eine Schätzung trug. Jeder Verbraucher las daraus „der Kampf ist
gleich vorbei" und schaltete ab — in den ersten rund 2,5 Sekunden jedes Pulls gab es
deshalb keine automatische Heilung. Behoben.

**Der Weißmagier heilt wieder, wenn ein Pack stirbt.** `StateUpdater.CanUseHealAction`
wandte das TTK-Gate `AutoHealTimeToKill` (8 s) auch auf Heiler an, obwohl die Option unter
`UseHealWhenNotAHealer` hängt und Nicht-Heiler meint. Sobald der Mittelwert der
Gegner-Restzeit unter acht Sekunden fiel — also am Ende jedes Packs —, gingen **alle**
Heilflags aus: Tank unter 20 %, kein Heilversuch, Sanctus statt Cure. Das war die
gemeldete Ursache (#54).

**Heilung vor dem Einschlag statt danach** — `Heal ahead of incoming damage`, **ab Werk
aus**. Jede Heilschwelle und die Zielwahl lesen die Gesundheit, auf die ein Mitglied
zuläuft, bis eine jetzt begonnene Heilung landet, statt den Stand von jetzt. Im Kampf: Ein
schnell fallender Tank bekommt seine Heilung rund einen GCD früher, und wer schnell fällt
wird vor jemandem bedient, der tiefer, aber stabil steht. Die Rate je Gruppenmitglied
stammt aus der vorhandenen Gesundheitshistorie (`RecordedHP`, 1 Hz über vier Minuten), die
der Fork auch für die Gruppe füllt statt nur für Gegner; sie ist damit netto nach allem —
Minderung, Barriere und Heilung eingerechnet. Weil die Schätzung über den ganzen Kampf
mittelt und damit träge ist, hält `ScoreTtkForecast` jede Sekunde die vorige Vorhersage
gegen den tatsächlichen Verlauf und `GetCorrectedTTK` teilt den Fehler heraus.

**Die Notfall-Vollheilung wartet auf einen Grund** — `Benediction needs a reason`,
**ab Werk an**. Benediction verlangt jetzt neben dem niedrigen Stand, dass das Ziel
angegriffen wird, dass eine Flächenaktion angekündigt ist oder dass seine Gesundheit
messbar fällt. Im Kampf: Ein gerade Wiederbelebter hält ein paar Prozent, hat keine Aggro
und nimmt keinen Schaden — die Schwelle las ihn als dringendsten Fall der Gruppe, und die
Vollheilung war für neunzig Sekunden weg, während nichts passierte.

**Der Tank läuft mit einem tickenden HoT in den Pull** — `UsePreRegen` (WHM),
`UsePreAspectedBenefic` (AST), je mit zwei Gegnerzahl-Schwellen. Vor dem Kampf geht der
Regen auf den Tank, sobald genug Gegner in Ansprungweite stehen, und wird gehalten,
solange das Pack groß genug ist; darunter fällt die Heilung auf die Schwellen zurück. Im
Kampf: Die ersten Treffer landen auf einem Tank, auf dem schon etwas läuft, statt auf
einem, der bereits tief steht. Beide Aktionen sind Sofortzauber, unterwegs geht nichts
verloren. **Im Spiel bestätigt.** Der Countdown-Zweig des Originals für Trials und Raids
ist dabei wiederhergestellt — der Fork hatte ihn zunächst gelöscht.

---

## 2 · Heilung — wen sie trifft

**Wer sterben wird, kommt vor die Rollenabkürzungen.** `ActionTargetInfo.FindHealTarget`
sortierte im Original nach Rolle mit festen Schwellen (Tank ≤ 45 %, Heiler ≤ 40 %) und
brach bei der ersten Abkürzung ab. Ein Schadensausteiler bei 10 % wurde damit übergangen,
sobald der Tank bei 44 % stand. Der Fork setzt einen kritischen Rang davor. Die Reihenfolge
ist in der CI verriegelt (`check_heal_target_order.py`), weil ein Upstream-Merge sie sonst
lautlos zurückdrehen würde.

**Ein Mitspieler hinter der Kamera ist wieder heilbar.** Der Filter „nur Ziele im Sichtfeld
angreifen" galt im Original auch für Heilziele.

**Unverwundbare Ziele** werden gesondert behandelt statt gar nicht: Upstream gibt einem Ziel
unter Invulnerabilität überhaupt keine Heilung; der Fork senkt die Schwelle auf
`HealthProtectedRatio` und lässt kurz vor Ablauf des Status wieder die normale gelten
(§ 6.2).

---

## 3 · Schilde und Barrieren

**Die Schildliste kannte die häufigsten Barrieren nicht.** `ShieldStatus` führte weder
Divine Benison noch The Blackest Night; fünfzehn Barrieren fehlten insgesamt. Wo der Baum
fragt „steht auf diesem Ziel noch etwas", war die Antwort für die meisten Tankschilde nein.
Ergänzt und gegen Rückfall gesichert.

**Die Anrechnung des Schildes auf die Heilschwelle ist wieder draußen** — auf seine
Entscheidung (A85). Der Grund ist der richtige: Ein Schild verhindert Schaden, er stellt
keine Gesundheit her. Ein Tank bei 40 % steht bei 40 %, ob eine Barriere läuft oder nicht;
läuft sie ab, ohne verbraucht zu werden, war die zurückgehaltene Heilung verschenkt.

**Offen und bewusst so belassen:** `HasSurvivingShield` misst die **kürzeste** Restzeit über
alle Barrieren, nicht die längste — ein Ziel mit drei Barrieren gilt als ungeschützt, sobald
die kleinste ausläuft. Die Fehlerrichtung ist „zu viel Heilung", nie „zu wenig"; die
naheliegende Umkehr tauscht den Fehler gegen die gefährlichere Richtung. Steht in `TODO.md`.

---

## 4 · Wiederbelebung

**Sie fällt jetzt sofort statt nach langer Verzögerung** — die einzige Änderung mit
Spielbestätigung (A73). Ursache war ein Henne-Ei-Problem: Der Fähigkeitspfad gab Spontanität
für eine Wiederbelebung nur aus, wenn die Wiederbelebung bereits als nächster GCD gemeldet
war — was sie nicht konnte, weil diese Meldung selbst Spontanität voraussetzte. Im
Handbetrieb mit hart anvisierter Leiche ging es, weil dort der GCD frei blieb; das war der
Beweis. Der Fork setzt den fehlenden Auslöser: Spontanität fällt auch, wenn eine
Wiederbelebung ansteht und wirkbar wäre. Der GCD-Pfad bleibt unangetastet — der erste
Versuch hatte ihn umgeschrieben und damit unter anderem den Schimmerschild des Beschwörers
stillgelegt; er wurde zurückgenommen (C37).

**Die Phönixfeder** ist verdrahtet, prüft die Zieleignung über den Gegenstandsstatus und die
Stufe der Wiederbelebungseigenschaft. **Ab Werk aus.**

**Die Nur-Heiler-Hartwirkmodi** messen die richtige Menge (welche Heiler zählen) und halten
den Spontanitäts-Vorbehalt, den ihr eigener Text verspricht: Bei bereiter Spontanität wird
nicht mehr hartgewirkt.

Swiftcast wird in der Beschwörer-Rotation nicht für Schaden verbraucht (A77) — deine Vorgabe,
Spontanität bleibt für Wiederbelebungen.

---

## 5 · Minderung und Verteidigung

### 5.1 Was sicher besser ist

**Zwei Tanks legen Reprisal nicht mehr übereinander.** Im Original fehlte Reprisal das
`TargetStatusProvide`, das Addle und Feint haben; der zweite Tank verschenkte 60 Sekunden
Abklingzeit. Dasselbe galt für die Gegenzahl-Zweige von Addle und Feint bei zwei Castern
oder zwei Nahkämpfern.

**Paladin und Krieger geben Reprisal jetzt auch bei Raidwides** — es stand nur im
Einzelzielpfad, obwohl die eigene Beschreibung des Kriegers es für Flächen versprach.
Dunkelritter und Haudegen hatten es richtig.

**Die Minderungsschwächungen werden aufrechterhalten statt einmal gesetzt**
(`ShouldSustainMitigationDebuff`, ein Helfer statt 25 Kopien). Die Dauer folgt der
Aufwertung auf Stufe 98.

**Ein Buster auf einen Schadensausteiler bekommt eine Antwort.** Zwölf Jobs setzen ihre
vorhandene reaktive Zeile — Feint, Addle, Troubadour, Tactician, Shield Samba, beim Samurai
zusätzlich Third Eye —, wenn ein Cast tatsächlich auf sie zielt. Vorher war der
Einzelzielschutz für Schadensausteiler überhaupt nicht besetzt.

**Selbstheilung der Schadensausteiler** (Second Wind, Bloodbath) bei zehn Jobs: Die
Rollenaktionen waren deklariert und wurden nie benutzt, `HealSingleAbility` war leer.

**Die BossModReborn-Zeitleiste wird nur gelesen, wenn sie eingeschaltet ist.** Vier Helfer
reagierten darauf, obwohl die Option aus war.

### 5.2 Was die Flächenerkennung heute tut

Der Baum lernt Flächenaktionen mit: Trifft eine Gegneraktion in einer Gruppe ab vier
Spielern jeden im selben Effektsatz, wird ihre Id dauerhaft vermerkt. Der Fork misst
zusätzlich, **wie hart** sie trifft — der Anteil der Maximalgesundheit, den der Einschlag
gekostet hat, wird je Aktion gespeichert. Die Listenverwaltung zeigt beides an.

### 5.3 Der Defekt, der in diesem Stand steckt

`Skip mitigation for small area casts` ist **ab Werk an**, und in dieser Voreinstellung
unterbleibt die Gruppenminderung in fast jedem gewöhnlichen Fall:

- Die Bedingung lautet „Puffer minus gemessener Anteil unter der Heilschwelle (0,65)".
  Bei voller Gruppe ist der Puffer 1,0 — gemindert wird also erst ab einem Anteil über
  **35 %** der Maximalgesundheit. Ein gewöhnlicher Raidwide erreicht das nicht.
- Der Anteil wird erst **nach dem ersten Einschlag** gespeichert. Der erste Cast einer
  Aktion mindert daher noch, jeder weitere nicht.

Im Kampf heißt das: Beim Beschwörer fällt seit dem 17.09. weder Addle noch Schimmerschild
bei Flächenschaden, weil `DefenseArea` nicht mehr gesetzt wird. Betroffen ist jede
gruppenweite Minderung, die an dieser Kette hängt, und über `IsUnderThreat` auch die
Gefahrenprüfung der Notfall-Vollheilung (§ 1).

**Handgriff:** Die Einstellung ausschalten stellt das Verhalten von vor dem 17.09. her.
Die Voreinstellung im Code zu ändern genügt nicht — eine bereits benutzte Konfiguration
trägt den gespeicherten Wert, und `Configs.Migrate` stellt einzelne Felder nicht um.
Die Entscheidung, wie die Bewertung selbst zu korrigieren ist, steht in `TODO.md`.

**Woher der Defekt kommt:** Die Bewertung wurde mit Voreinstellung **an** eingeführt. Das
verstößt gegen die eigene Regel, dass eine Verhaltensänderung ohne Nachweismöglichkeit das
bisherige Standardverhalten behält und das neue hinter einem Schalter anbietet.

---

## 6 · Tank-Selbstschutz

### 6.1 The Blackest Night

Die Barriere kostet 3000 MP und zahlt sie nur als Dark Arts zurück, wenn sie **vollständig
aufgebraucht** wird. Der Auslöser des Originals ist dafür viel zu schwach — zwei Gegner in
Nahkampfreichweite oder irgendein nicht unterbrechbarer Cast auf dich. `BlackestNightUsage`
bietet engere Fassungen samt Mindestgegnerzahl und einem Notfallanteil, unter dem die
Barriere ohne jede Bedingung fällt. **Die Voreinstellung ist das alte Verhalten.**

Dazu die Gegenseite: Der Weißmagier kann Sanctus zurückhalten, solange ein Tank die Barriere
trägt (`HoldHolyForBlackestNight`) — die Betäubung stoppt genau die Treffer, die die Barriere
aufbrauchen würden, und ohne sie verfällt sie ungenutzt.

### 6.2 Living Dead und Walking Dead

Die Staffelung folgt deiner Vorgabe: Solange Living Dead mehr als zwei GCDs Restzeit hat,
gilt die abgesenkte Schwelle `HealthProtectedRatio` (0,15) — der Todeseffekt kann also
eintreten. Läuft der Status in zwei GCDs oder weniger ab, kehrt die normale Schwelle zurück,
es wird also kurz vor Ablauf geheilt. Der Vorlauf setzt aus, solange die Null vor dem
Fensterende ankommt (`DeathStillLikely`), sonst hätte er genau den Tod verhindert, für den
die Regel da ist. `WithholdHealingForLivingDead` verschärft nur den ersten Abschnitt und ist
**ab Werk aus**.

Offen und in `TODO.md` erfasst: In der Walking-Dead-Phase ist der HoT durch die
`RegenHeal`-Schwelle gesperrt (der Träger liegt bei 1 HP), und Benediction feuert am Anfang
der Phase statt am Ende.

### 6.3 Rückstoß (Arm's Length) und Provoke

`UseArmsLengthOnPull` benutzt die Aktion auf einem Gruppenpull für ihren Slow, nicht nur als
Rückstoßschutz — das war bis dahin die einzige Verwendung. Der Slow von +20 % trifft jeden
Gegner, der zuschlägt, und drosselt fünfzehn Sekunden lang den gesamten eingehenden Strom.

Der Co-Tank-Provoke zieht den Boss nicht mehr von einem Tank weg, der gerade unter
Superbolide, Living Dead oder Holmgang steht.

---

## 7 · Schaden und Rotation

**Beschwörer.** Das Zündfenster von Searing Light ist an die Burstphase gebunden: in Solar
Bahamut, bei niedrigerer Stufe in Bahamut. Bei einem zweiten Beschwörer in der Gruppe weicht
die Regel auf die große Beschwörung aus, und bei allen belegten Phasen auf Ifrit.
`PreferTitanWhileMoving` (**ab Werk aus**) zieht Titan vor, solange du dich bewegst — Topaz
Rite und seine Folgeaktionen sind Sofortzauber, während Garuda und Ifrit Stillstand
verlangen und unterwegs GCDs verlieren. Titan wird nur vorgezogen, nie übersprungen.

**Weißmagier, Sanctus.** Drei Regeln, jede einzeln abschaltbar: die Betäubung nicht
überschreiben, solange sie noch läuft (`StretchHolyStun`, **ab Werk aus**); Sanctus
zurückhalten, solange die Barriere des Dunkelritters gefüllt werden soll; und Sanctus
zurückhalten, solange mehr als die Hälfte der Gegner im Wirkbereich verlangsamt ist und
mindestens die eingestellte Zahl den Slow trägt — Slow und Betäubung drosseln denselben
Strom, und die Betäubung ist mehr wert, wenn der Slow abgelaufen ist. Eine Schranke für den
Restausstoß der Gegner begrenzt das Sparen.

**Thin Air** wird nur noch auf einen teuren Zauber gelegt, wenn der MP-Druck tatsächlich da
ist und Lucid Dreaming ihn nicht beantworten kann (`ThinAirOnMpPressureOnly`). Eine
Wiederbelebung nimmt weiterhin immer eine Ladung.

**Behobene Fehler des Originals mit Kampfwirkung:**

| Stelle | Was falsch war | Was im Kampf geschah |
|---|---|---|
| Schwarzmagier, Thunder | Das Refresh-Gate kannte `HighThunder` nicht | Ab Stufensync 92 wurde ein frischer Flächen-DoT bei jedem Cast abgeschnitten |
| Rotmagier, Impact | `!Impact.EnoughLevel && Impact.CanUse` | Zweig nie erreichbar, Impact fiel nie |
| Neun `base.X`-Aufrufe | Overrides riefen die falsche Basismethode, z. B. `DefenseSingleGCD` → `base.DefenseAreaGCD` | Die Dispatchkette lief still an der falschen Stelle weiter; kompiliert sauber, im Diff unsichtbar |
| Unterbrechung / Rückstoßschutz | Der Rollen-Standard lief **vor** dem Job-Override | Schnitter und Viper gaben Leg Sweep bzw. Arm's Length wegen ihres Combo-Gates ab, der Standard nahm sie ungegatet trotzdem |
| Phantom-Job-Zweig | `out _` statt `out act` | Aktion wurde erkannt und nie zurückgegeben |
| `MoveBackAbility` | Im `if`-Kopf **und** im Rumpf aufgerufen | Doppelaufruf, falsche Reihenfolge gegenüber der Duty-Rotation |
| Restricted-DoT-Sperre | `continue` in der inneren statt der äußeren Schleife | Gesperrte Ziele wurden doch mit DoTs belegt |
| `CalculateDamageFactor` | `foreach` über die Gruppe ohne Rumpf | Toter Code |

Drei dieser Klassen sind per CI ausgeschlossen (`check_base_calls.py`), damit ein Merge sie
nicht zurückbringt.

**Bewegungsslots** für Haudegen, Weißmagier, Barde und Samurai (je acht Zeilen): Sie laufen
ausschließlich unter `MoveForward`/`MoveBack` und können die Schadensrotation nicht erreichen.

**Lange GCD-Ketten in benannte Stufen zerlegt** (Blaumagier, Phantom, Piktomant, Samurai,
Beschwörer): nur Methodengrenzen eingefügt, keine Zeile verschoben, Reihenfolge und
Verhalten unverändert.

---

## 8 · Was der Fork an eigenen Fehlern zurückgebaut hat

- **Sustain des Weisen** lief über den ganzen Pull: Die Bedingung prüfte den Schildstatus
  selbst, und ein Schild platzt im Wall-to-Wall in Sekunden — zwei GCDs pro Platzer, vor
  allem Schaden. Entfernt; bei Weißmagier und Astrologe ticken HoTs ihre Dauer ab, dort
  bleibt der Helfer.
- **Weakness-Schwellenfaktor** heilte praktisch immer: Die Multiplikation mit 1,5 klemmte die
  Schwelle auf 1,0, ein geschwächter Spieler galt bei jeder Gesundheit unter voll als
  heilbedürftig — nach jedem Rezz hundert bis dreihundert Sekunden lang. Entfernt.
- **DoT-Schutz des Weißmagiers** prüfte das Ziel des **vorigen** Casts, weil `Target` erst in
  `CanUse` gesetzt wird. Auf Upstream-Form zurückgebaut.
- **Ein Upstream-Feature war gelöscht**: der Countdown-Regen für Trials und Raids.
  Wiederhergestellt.
- **Der erste Wiederbelebungsversuch** schrieb `nextGCD` um — 447 Leser, darunter der
  Schimmerschild des Beschwörers, der daraufhin verstummte. Zurückgenommen (C37), der zweite
  Versuch fasst den GCD-Pfad nicht an.
- **Ein `[WSH 16/18]`-Marker** im Fenstertitel täuschte eine Versionierung vor. Entfernt.

---

## 9 · Was im Spiel nichts tut

Rund die Hälfte des Diffs: `TODO.md`, `AUDIT_LOG.md` und dreizehn Konzeptdokumente; das
deutsche Namensregister und sein Erzeuger (`GermanNameIndex`, läuft nur mit installiertem
Spiel); und die Prüfskripte samt CI-Job. Letztere sind kein Beiwerk — sie verriegeln
Fehlerklassen, die schon einmal aufgetreten sind. Dreizehn davon laufen bei jedem Lauf:
falsches `base.`-Ziel und widersprüchliches Stufenprädikat (`check_base_calls`), strukturell
zerbrochene C#-Dateien, verlorene Heilzielreihenfolge, Zeilenverweise in Dokumenten,
deutsche Namenszuordnung, Fork-Versionsbezeichnung, MSBuild-XML, Mengen-Nachschlagen,
fehlende Erklärungstexte, die Gefahrenprüfung der Notfallheilung und drei Musterscans.
Die übrigen — darunter die Sync-Messung — sind für den Einsatz von Hand gebaut.

Die Paketidentität trägt ein Prerelease-Label statt Build-Metadaten (`-wsh1` statt `+wsh1`),
weil NuGet die Metadaten wegnormalisiert und das ausgelieferte Paket sonst nicht von der
Upstream-Fassung zu unterscheiden war.

---

## 10 · Was offen ist

Vollständig in `TODO.md`, getrennt nach Defekt und technischer Schuld. Die Punkte mit
Kampfwirkung, kurz:

- **Die Flächenbewertung aus § 5.3** — Entscheidung steht aus.
- **Die Flächenheilung entscheidet nach Pegel statt nach Rate**, anders als die
  Einzelheilung. Nicht mitbehoben, weil dieselben Größen 83 Leser außerhalb der Heilkette
  haben, darunter fremde Rotationen.
- **Die Notfallheilungen von Weise, Gelehrter und Astrologe** prüfen die Gefahr nicht, anders
  als der Weißmagier. Erfasst, nicht bearbeitet — außerhalb deines Nutzungsprofils.
- **Walking Dead**: HoT gesperrt, Benediction am falschen Ende der Phase.
- **Searing Light fällt nicht am Anfang der Burstphase**, wie von dir gemeldet. Die Ursache
  ist nicht gefunden: Die Zündbedingung ist für einen einzelnen Beschwörer mit Upstream
  verhaltensgleich und sagt nur **ob**, nie **wann**. Verpasst die Aktion den ersten
  Einschiebeplatz, fällt sie am nächsten freien — nichts zieht sie vor.
- **`HasSurvivingShield`** misst die kürzeste statt der längsten Barrierenrestzeit (§ 3).

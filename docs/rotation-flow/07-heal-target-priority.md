# 07 · Zielwahl der Heilung

**Leitfrage:** Wer wird zuerst geheilt, wenn nicht alle zugleich versorgt werden können?

## Vorgabe des Auftraggebers

**Oberste Priorität hat das gesamtheitliche Überleben der Gruppe.** Es sollen alle überleben; wo das
nicht für alle zugleich geht, gilt Triage.

**Maßgeblich ist dann, wer wie stark gefährdet ist zu sterben.** Nicht der Prozentsatz, nicht die
Rolle für sich, nicht die Barriere für sich — die Gefährdung.

**Bei gleicher Gefährdung entscheidet die Rolle, und zwar Heiler vor Tank vor Schadensausteiler.**
Seine Begründung ist keine Rangordnung nach Wichtigkeit, sondern nach Ersetzbarkeit: Der Tank kann
einige Zeit ohne Heiler bestehen — er trägt Minderungen, einen großen Lebenspool und eigene
Selbstheilung. Heilen kann dagegen nur der Heiler. Fällt er, fällt die Gruppe mit ihm, und zwar
verzögert, aber unvermeidlich. Die Rolle ist hier also selbst eine Gefährdungsgröße: Der Tod des
Heilers gefährdet zusätzlich alle anderen.

Daraus folgen die weiteren Regeln, die er genannt hat:

- **Hält der Tank die Aggro, hat er die Heilpriorität.** Er bekommt den Schaden, also bekommt er die
  Heilung.
- **Hält der Heiler die Aggro, muss der Heiler überleben.** Fällt er, fällt die Gruppe mit ihm, und
  er trägt weder die Minderungen noch den Lebenspool eines Tanks.
- **Sind mehrere zugleich betroffen — Tank und Heiler, oder auch Schadensausteiler —, entscheidet
  die Schadensrate.** Dann reicht „wer wird angegriffen" nicht mehr; es zählt, wie schnell die
  Gesundheit fällt.
- **Ein Schadensausteiler ohne Aggro bei 10 % Leben kann an einer Flächenaktion sterben.** Aggro ist
  also keine Bedingung für Gefährdung, sondern eine ihrer Ursachen.

### Das Mittel richtet sich nach der Gefahr, nicht nach dem Gesundheitsstand

Aus seiner Meldung, dass unmittelbar nach einer Wiederbelebung Benediction auf den Wiederbelebten
fällt, und seiner Präzisierung dazu — im Wortlaut:

> „falls gefahr bevorsteht, z.b. goßer heftiger aoe ist diese notfallmaßnahme gerechtfertigt. wenn
> der spieler aber keine aggro hat, kein aoe ansteht, oder kein sonstiger schaden ansteht, würde
> doch hot oder kleinere heals bzw. beides reichen"

**Die Vorgabe trennt zwei Fragen, die eine einzelne Schwelle zu einer verschmilzt:** *Wer* braucht
Heilung — das entscheidet der Gesundheitsstand — und *welches Mittel* er bekommt — das entscheidet
die Gefahr. Ein Wiederbelebter steht bei wenigen Prozent und ist damit nach jeder Schwelle der
dringendste Fall, obwohl ihm gerade nichts geschieht: Er trägt keine Aggro, und sein
Gesundheitsverlauf steigt. Die einmalige Vollheilung dort auszugeben heißt, sie fehlt beim nächsten
Tankschaden.

**Die Vorgabe gilt nicht der Wiederbelebung, sondern der Gefahr.** Der Wiederbelebte ist nur der
auffälligste Fall; ein Schadensausteiler, der gerade aus einer Flächenaktion herausgelaufen ist,
steht genauso da. Eine Regel „nicht auf frisch Wiederbelebte" träfe deshalb den Wortlaut und nicht
die Sache — und sie würde mit dem nächsten vergleichbaren Fall erneut fällig.

**Drei Gefahrenquellen, alle drei im Baum vorhanden:** Aggro (ein Gegner zielt auf das Mitglied),
angekündigter Flächenschaden (`DataCenter.IsHostileCastingAOE`) und tatsächlich ankommender Schaden
(der Gesundheitsverlauf hat eine endliche Restzeit). Liegt keine davon vor, genügen HoT und die
kleineren Heilungen — und die kommen von selbst, weil der Zweig ohne die Vollheilung zu Asylum,
Divine Benison und Tetragrammaton weiterläuft und der GCD-Pfad Regen und Cure II behält. Das Ziel
wird also nicht übergangen, nur die teuerste Antwort darauf.

**Gilt für jeden Heiler, nicht nur den Weißmagier** (A180, sein Profil umfasst alle Kampfjobs). Dieselbe Prüfung (`ObjectHelper.IsUnderThreat`) steht vor der teuersten Einzelheilung jedes Heilers, jeweils hinter einer eigenen Einstellung, ab Werk an: Benediction (Weißmagier), Excogitation als Heilung (Gelehrter, auch unter Recitation; der Auslöser von Recitation für Excogitation fragt dasselbe, sonst ginge Recitation für einen Tank ohne Gefahr raus und verfiele, A280), Taurochole als Heilung (Weiser) und die **letzte** Ladung Essential Dignity (Astrologe). Bei Essential Dignity nur die letzte: Die ersten Ladungen laden hintereinander nach, eine davon auszugeben lässt den Tank nicht ohne Antwort; die letzte schon. Hält die Strategie „Prioritize Essential Dignity over single target GCD heals when available" die GCD-Heilungen für Essential Dignity zurück, dann nur, solange es jetzt gehen kann: Wartet die letzte Ladung auf Gefahr, bekommt das Ziel die kleineren Heilungen, die der Text der Gefahrenbindung verspricht (A403). Ebenso beim Weisen: Druochole wartet auf Taurochole nur, solange Taurochole gehen kann, nicht, während es auf Gefahr wartet (A405). Die Verteidigungspfade (Excogitation, Taurochole, Haima) bleiben unberührt — sie laufen nur, wenn eine Abwehr angefordert ist, also bei Gefahr.

**Auch die Heilschwellen der Jobs lesen die Vorausschau** (A191): Regen, Essential Dignity, Synastry,
Aspected Benefic, Taurochole, Krasis, Soteria, Pneuma, Kardia, Benediction, Excogitation, Emergency Tactics,
Aetherpact, Clemency, Bloodwhetting, Thrill of Battle, Equilibrium, Nascent Flash, Second Wind, Bloodbath und
die Untergrenze der Pull-Pflege; ebenso die Heilungen der Inhaltsrotationen (A519): in Kreszentia Occult Potion,
Occult Elixir, Occult Chakra, Occult Resuscitation, Blessing und Phantom Judgment, im Monster-Hunter-Kampf Mega Potion.
Bis A519 lasen diese den Stand, weil der Prüfer die Inhaltsrotationen als außerhalb seines Profils ausließ und seine
Liste der Heilnamen ihre Namen nicht kannte. Abwehrschwellen (sterbender Tank, Intervention, Cover, The Blackest Night)
sind keine Heilschwellen und bleiben am Stand. `check_heal_threshold_forecast.py` hält das in der CI fest.
Wie weit voraus, hängt von der Heilart ab (A213, Konzept 08): Eine oGCD-Heilung blickt nur so weit voraus, wie die
laufende Ausführungssperre reicht, eine GCD-Heilung um den Rest des GCD und ihren Wirkvorgang. Maßgeblich ist, wann
die Heilung landet: Synastry, Krasis, Soteria und Emergency Tactics heilen erst mit dem nächsten GCD und lesen wie
die GCD-Heilungen und die Untergrenze der Pull-Pflege den GCD-Vorlauf, bei Schwelle und Zielwahl (A221); die übrigen
oGCD-Schwellen den kurzen.

**Auch die Flächenschwellen lesen die Vorausschau** (A182). Der Text von „Heal ahead of incoming damage" sagt: „Every healing threshold and the heal target choice read the health a member is heading for". Die Flächenschwellen (`HealthAreaAbility`, `HealthAreaSpell`) lesen eigene, vorausberechnete Gruppenwerte (`DataCenter.ComputeForecastAreaStats`: Mittel und Streuung, bei mehr als vier Mitgliedern über die vier niedrigsten). Die bisherigen Gruppenwerte bleiben unverändert, weil sie viele Leser außerhalb der Heilkette haben, fremde Rotationen darunter. Mit ausgeschalteter Einstellung ist die Vorausschau 1 und die Werte gleich den bisherigen. Gezählt werden dieselben Mitglieder wie bei den bisherigen Werten (gültig, mit Id, lebend; A210). Im Kampf: Fällt die Gruppe schnell, kommt eine Flächenheilung per GCD etwa einen GCD früher. Die oGCD-Schwelle blickt nur bis zum Ende der Ausführungssperre voraus (A213).

## Was Gefährdung heißt

**Aggro sagt nicht, ob jemand Schaden bekommt — nur, ob er *gerichteten* Schaden bekommt.** Eine
Flächenaktion trifft ohne Rücksicht darauf, und wer wenig Puffer hat, stirbt daran, ob er angegriffen
wurde oder nicht. Vier Größen also, und keine davon genügt allein:

| Größe | Beantwortet | Im Baum vorhanden |
|---|---|---|
| **Effektive Gesundheit, absolut** | Wie viele Punkte liegen zwischen ihm und dem Tod? | ja — `GetEffectiveHp` (Gesundheit plus Barriere); die Zielwahl liest sie vorausberechnet in Klasse 1 und, bei angekündigtem Flächenschaden, in Klasse 3 |
| **Aggro** | Bekommt er gerichteten Schaden — Auto-Angriffe, Tankbuster? | ja — `DataCenter.TargetedPartyMembers`, je Bild aus den Zielen der Gegner; die Zielwahl liest sie in Klasse 2 |
| **Angekündigter Flächenschaden** | Kommt Schaden, der ihn ohne Aggro trifft? | ja — die Zielwahl liest in Klasse 3 `DataCenter.AreaHitAnnounced`: Flächenmarker auf der Gruppe, gelisteter Cast, großer gemessener auch unterbrechbar, BossModReborn-Raidwide im Minderungsfenster — ohne das Minderungsurteil aus `13-aoe-damage-classification.md` (A271). `IsUnderThreat` liest weiter `IsHostileCastingAOE` mit Urteil (Abschnitt „Vorgabe des Auftraggebers“) |
| **Eingehende Schadensrate** | Wie schnell schwindet der Puffer? | **ja, seit A91** — `RecordedHP` trägt die Gruppe mit, `GetTTK` antwortet für Mitglieder, `GetCorrectedTTK` teilt den Schätzfehler heraus. Die Zielwahl liest sie über die vorausberechnete Gesundheit (Stufe 3) |

Aus den ersten beiden folgt der Puffer, aus allen vieren die **Zeit bis zum Tod**. Sie ist **vorhanden**:
`08-mitigation-synergy.md` hat sie gebaut — die Gesundheitsreihe `RecordedHP` nimmt die Gruppe seit A91
mit auf, also antwortet `GetTTK` auch für Mitglieder, und `GetCorrectedTTK` hält jede Vorhersage gegen
den tatsächlichen Verlauf. Die Zielwahl liest sie über die vorausberechnete Gesundheit (Stufe 3).

**Der kleine Puffer ist damit für sich gefährlich.** Wer bei 10 % steht, braucht keine Aggro, um an
der nächsten Flächenaktion zu sterben; die Aggro entscheidet nur, ob er auch ohne Mechanik fällt. Ein
Maß, das erst bei Aggro anschlägt, verfehlt genau diesen Fall.

**Prozentsatz und absolute Punkte sind beide Surrogate, und jedes bricht in einer anderen Lage.** Der
Prozentsatz normiert stillschweigend auf die erwartete Schadensrate: Ein Tank hat den größeren
Lebenspool und nimmt auch den größeren Schaden, ein Schadensausteiler beides kleiner — deshalb
vergleicht der Prozentsatz bei **gerichtetem** Schaden ungefähr die richtige Größe. Bei
**ungerichtetem** Flächenschaden bricht er, denn der trifft alle mit derselben absoluten Zahl, und
dort zählen die Punkte. Die absoluten Punkte brechen genau umgekehrt: Sie halten den Tank bei 30 %
für sicherer als den Schadensausteiler bei 45 %, obwohl der Tank den Dauerschaden nimmt.

Das Maß ist also nicht zu **tauschen**, sondern von der Lage abhängig zu machen — und die Lage ist
lesbar, weil angekündigter Flächenschaden im Baum steht.

## Sachstand

**Die Zielwahl ordnet nach Gefährdung** (`ActionTargetInfo.GeneralHealTarget`, Stand 02.10.2026). Alle
Gesundheitswerte darin sind vorausberechnet (`GetForecast*`, hinter `HealAheadOfDamage`, ab Werk an):

| Rang | Bedingung | Ordnung |
|---|---|---|
| 1 | Gefährdungsklasse 1: ungeschützt, effektive Gesundheit ≤ `HealthForDyingTanks` | absolute effektive Punkte, bei Gleichstand Heiler vor Tank vor Schadensausteiler |
| 2 | der Spieler selbst | ≤ `HealthSelfRatio` (0,40) |
| 3a | mit `HealTargetByDanger` (ab Werk an): Klassen 2 und 3 (`DangerClassTarget`) | siehe „Der Entwurf“ |
| 3b | ohne: Heiler ≤ `HealthHealerRatio` (0,40), dann Tank ≤ `HealthTankRatio` (0,45), dann niedrigster Prozentsatz | — |

Geschützte (Unverwundbare) stehen in jeder Stufe zuletzt. Die beiden Abschnitte nach dem folgenden begründen die
Rangfolge an den Fällen, die sie entscheidet; was davon offen ist, steht jeweils am Ende.

**Die Schwellen sind Zielwahlschwellen, keine Heilschwellen.** Ob überhaupt geheilt wird, entscheiden
`HealthSingleAbility` und `HealthSingleSpell` weiter oben; `FindHealTarget` bekommt bereits ein durch
`healRatio` gefiltertes Feld und beantwortet nur noch, **wen** die ohnehin fallende Heilung trifft.
Eine dieser Rollenschwellen zu heben erzeugt deshalb keine zusätzliche Heilung und keine
Überheilung — es verschiebt die Reihenfolge.

### Flächenheilungen um den Wirkenden messen den Bedarf an den Getroffenen

**Sachstand (A137):** Eine Flächenheilung, die um den Wirkenden herum wirkt — Reichweite 0, ein
Wirkradius, kein Bodenziel: Medica, Helios, Succor, Lux Solaris und rund dreißig weitere —, hat den
Wirkenden als Anker, und ihr Bedarf wird an den Mitgliedern **im Wirkradius** gemessen: mindestens
`AoeCount` darin, die sie aufnehmen können (verletzt, lebend, nicht heilungsunfähig, ohne ihren
bereitgestellten Status), und bei eingeschalteter Heilprüfung mindestens eines davon mit
vorausberechneter Gesundheit unter der Heilschwelle der Aktion (`AutoHealRatio`). Ein Mitglied, das
für einen Todesauslöser zurückgehalten wird, wird mitgeheilt, wenn es im Radius steht, zählt aber nie
als Grund.

**Im Kampf:** Der Heiler steht voll und einige Yalm neben der Gruppe, ein Cleave hat die Nahkämpfer
getroffen, die Flächenheilflagge steht. Vorher fiel die Heilung nicht, weil der allgemeine Pfad ein
Heil**ziel** in Reichweite 0 suchte — also den Heiler selbst und wer ihn berührt — und dieses Ziel unter
der Heilschwelle verlangte. Jetzt fällt sie, sobald im Radius genug Verletzte stehen.

**Grenzen und Bedingungen:**
- Gilt nur, wenn das Spiel für die Aktion Reichweite 0 meldet (`ActionManager.GetActionRange`). Meldet es
  etwas anderes, greift der Zweig nicht, und alles bleibt wie vorher.
  **Hinweis des Auftraggebers:** Lux Solaris ist eine Point-Blank-Fläche vom Wirkenden aus, wie Holy beim
  Weißmagier — die Prämisse Reichweite 0 trägt. Lux Solaris selbst läuft seit A154 nicht mehr durch
  diesen Zweig, sondern folgt ihrer eigenen Regel (Konzept 08) und fehlt deshalb in der Anzeige unten.
- Gilt nur für die Zielart Heilung und nicht, wenn ein Aufrufer den Wirkenden ausdrücklich als Ziel
  nennt (`TargetType.Self`); dort gibt der allgemeine Pfad den Wirkenden ohne Bedarfsprüfung zurück.
  Andere freundliche Aktionen mit Reichweite 0 bleiben auf dem allgemeinen Pfad.
- Am Anker entfallen die Prüfungen, die der allgemeine Pfad am Heilziel stellte: `CanTarget`,
  `CanUseTo` (Abfrage beim Spiel), `MinHPFeature` der Aktion. Für keine freundliche Heilung mit
  Reichweite 0 ist ein `CanTarget` gesetzt (erhoben A137). `NoNewHostiles`, das im allgemeinen Pfad
  die Trefferzahl auch für freundliche Mitglieder ohne Ziel auf 0 setzte, wirkt hier nicht. **Hinweis
  des Auftraggebers:** Heilungen erzeugen Feindschaft, wie Schaden auch — bestätigt im consolegameswiki,
  „Enmity" (02.10.2026: „healing effects used on allies generate enmity"); den Umfang nennt keine der
  geprüften Quellen (consolegameswiki, Akhmorning „Raiding Fundamentals").
  Ob eine Gruppenheilung dabei mehr oder andere Gegner erreicht als eine Einzelheilung, ist ebenfalls
  nicht belegt — `NoNewHostiles` bleibt deshalb eine Regel für Angriffe, und das ist eine offene
  Annahme, keine belegte Tatsache.
- Die AoE-Einstellung (Full/Cleave/Off) gilt nur Angriffen, seine Lesart (A147): Unter „Cleave" und
  „Off" gehen Gruppenheilungen und Gruppenminderungen wie unter „Full".
- Ob überhaupt geheilt wird, entscheidet weiter die Flagge. Deren Streuungsbedingung hält die Flagge
  unten, wenn ein einzelner Spieler getroffen ist — für den teuren Flächenzauber eines Heilers richtig,
  kein Defekt (A117).

**Betroffene:** alle Heiler dieses Baums einschließlich der fremden Rotationen, die dieselben Aktionen
über den Heilpfad rufen; die Autoren abgeleiteter Rotationen, weil `CanUse` dieser Aktionen
anders antwortet als im Original; die Upstream-Pflege durch eine weitere Abweichung in `ActionTargetInfo.FindTarget`.
Ein belegter Defekt, keine Verbesserung auf Verdacht — deshalb ohne eigene Option.

### Bodenflächen dort, wo die Gruppe steht

**Sachstand (A398):** Eine freundliche Fläche mit Bodenziel – Sacred Soil, Asylum, Earthly Star, Liturgy of the
Bell – geht nach den eigenen Wahlen des Spielers („Always place them on yourself", „Place them on the party tank")
an die übliche Stelle von Upstream: einen gespeicherten Punkt des Gebiets, in Prüfungen und Raids zu acht ohne
gespeicherten Punkt die Arenamitte (0/0 oder 100/100, der nähere), sonst den Boss, sonst einen Punkt beim
angegriffenen Mitglied, sonst den Spieler. Diese Stelle verlangte nur, dass irgendwer im Kreis steht. Mit „Place
them where they cover the most party members" (ab Werk an) wird sie mit der Stelle an jedem Mitglied in Reichweite
verglichen: Deckt eine davon mehr Mitglieder ab (bei Heilungen: mehr Verletzte), geht die Fläche dorthin; bei
Gleichstand bleibt die übliche Stelle, unter mehreren gleich guten die, um die die Getroffenen am dichtesten stehen.

**Im Kampf:** Prüfung, Boss in der Mitte bei 100/100, die Gruppe sammelt sich am Südrand, 20 Yalm von der Mitte, für
einen Stack, dann kommt der Raidwide. Vorher landete Sacred Soil (Wirkradius 15 Yalm laut Blatt) in der Mitte und
deckte Tank und Nahkämpfer, die Gruppe am Stack nahm den Treffer ohne die 10 % Minderung. Jetzt liegt sie am Stack, wenn dort mehr stehen. Steht die Gruppe in
der Mitte, ändert sich nichts.

**Warum hinter einer Option:** Gemessen wird, wo die Gruppe beim Wirken steht. Ob sie dort bleibt, misst keiner der
beiden Wege; die Arenamitte ist eine Annahme von Upstream über künftige Positionen, die Abdeckung eine Messung der
jetzigen. Dass die Fläche nie weniger Mitglieder deckt als die übliche Stelle, folgt aus dem Vergleich; ob das im
Kampf mehr nützt, wenn die Gruppe danach weiterläuft, ist nicht belegt.

**Grenzen:** Kandidaten sind die Standorte der Mitglieder (auf die Reichweite gekürzt), nicht jeder Punkt der Arena –
ein Punkt zwischen zwei Gruppen, der beide deckt, wird nicht gefunden. Earthly Star schadet beim Auslösen auch
Gegnern; an der Gruppe statt am Boss geht dieser Schaden verloren, wenn die Gruppe fern vom Boss steht – Sicherheit
vor Schaden.

**Betroffene:** alle Heiler mit Bodenflächen, fremde Rotationen eingeschlossen; Autoren abgeleiteter Rotationen,
weil die Zielwahl dieser Aktionen anders antwortet (CHANGELOG).

### Klasse 1 vor den Rollen-Kurzschlüssen

**Die Rollenabkürzungen kehren sofort zurück, sobald ihre Schwelle erfüllt ist, und sehen dabei
niemanden sonst an; deshalb steht Klasse 1 vor ihnen.** `healerTars[0]` unter `HealthHealerRatio` beendet die Suche, `tankTars[0]`
unter `HealthTankRatio` ebenso — der Prozentvergleich in Rang 4 wird gar nicht mehr erreicht.

Ohne Klasse 1 davor wurde **ein Schadensausteiler bei 10 % übergangen, sobald der Tank bei 44 % stand.** Das
ist genau der Fall, den die Vorgabe nennt — „auch ein Damagedealer ohne Aggro mit 10 % Leben kann bei einem AoE
sterben". Der Tank bei 44 % hinter
Minderungen und einem großen Lebenspool ist nicht gefährdeter als ein Schadensausteiler bei 10 %; er
ist nur früher in der Reihenfolge.

**Das ist der schwerere der beiden Fälle**, weil er ohne jede neue Messung zu beheben war: Wer
unter der Schwelle steht, an der der Baum selbst „dieser Spieler fällt gleich" sagt
(`HealthForDyingTanks`), gehört vor jeden Rollen-Kurzschluss. Die Rangfolge der Vorgabe — Heiler vor
Tank vor Schadensausteiler — gilt bei **gleicher** Gefährdung, und 10 % gegen 44 % ist keine gleiche
Gefährdung.

### Die Schwellen 45/40 und die Rangfolge in Klasse 2

Die Prüfreihenfolge stellt den Heiler vor den Tank, wie die Vorgabe es verlangt. Die Zahlen tun es
nicht: Der Tank löst bei ≤ 45 % aus, der Heiler erst bei ≤ 40 %. **Im Band zwischen 40 % und 45 %
gewinnt deshalb der Tank, obwohl der Heiler gleich tief oder tiefer steht.**

Im Kampf sieht das so aus: Heiler und Tank stehen beide bei 44 %. Der Heiler-Zweig greift nicht
(44 > 40), der Tank-Zweig greift (44 ≤ 45) — die Heilung geht an den Tank. Der Heiler bekommt sie
erst, wenn er weitere vier Prozentpunkte verloren hat. Spielt der Auftraggeber selbst den Heiler,
trifft ihn derselbe Fall über `HealthSelfRatio`, das ebenfalls auf 0,40 steht: Er wird bei 44 %
zugunsten eines gleich stehenden Tanks übergangen.

**Die Verzerrung wirkt in dieselbe Richtung wie der Prozentvergleich.** Der Tank hat den größeren
Lebenspool, 45 % davon sind mehr Punkte als 40 % eines Heilerpools — die niedrigere Schwelle trifft
also ausgerechnet den, der pro Prozentpunkt weniger Puffer hat. Beide Effekte addieren sich statt
sich auszugleichen.

**Was für die Umkehr spricht, und wie weit es trägt:** Die höhere Tankschwelle bildet keine Priorität
ab, sondern die fehlende Schadensrate. Der Tank hält die Aggro, nimmt Auto-Angriffe und Tankbuster
und fällt deshalb schneller; ein Heiler bei 44 % steht im Regelfall still, ein Tank bei 44 % steht in
fünf Sekunden bei 20 %. Als Näherung der Rate ist die Differenz sinnvoll — und für den Regelfall, den
die Vorgabe mit „hält der Tank die Aggro, hat er die Heilpriorität" selbst nennt, ist sie richtig.

**Sie trägt nur nicht in dem Fall, für den die Vorgabe gemacht ist.** Das Surrogat ist rollenfest,
nicht lagefest: Es fragt, welche Rolle jemand hat, nicht, wer gerade Schaden bekommt. Hält der Heiler
die Aggro — der zweite Fall der Vorgabe —, dreht sich die Rate um, die Schwellen bleiben stehen. Dann
nimmt der Heiler den gerichteten Schaden, ohne Tankminderungen, ohne Tankpool, und wird gleichwohl
fünf Prozentpunkte später versorgt als der Tank, der in diesem Moment gar nichts abbekommt. Dasselbe
gilt für den Wall-to-Wall-Pull vor dem Einsammeln und für jeden ungerichteten Flächenschaden, der
beide gleich trifft.

**Heute gilt davon:** Mit `HealTargetByDanger` stand die Umkehr nur noch, wenn Heiler und Tank beide unter
Beschuss standen und beide im Band lagen — und genau dort trägt die Begründung der Differenz nicht, denn
Klasse 2 verlangt den Beschuss für beide. Seit A283 kommt deshalb in Klasse 2 zum gewählten Kandidaten jeder
beschossene Heiler oder Tank hinzu, der gleich tief oder tiefer steht; dann entscheidet die niedrigste
vorausberechnete Gesundheit, bei Gleichstand der Heiler. Die Schwellen bleiben, was ihr Text sagt: hinreichende
Bedingung, dass eine Rolle zuerst kommt, kein Verbot für ein tieferes Mitglied der anderen Rolle. Ohne
`HealTargetByDanger` entscheiden die Rollenschwellen weiter allein, wie ihr Text es sagt.

**Damit ist das keine falsche Zahl, sondern eine fehlende Bedingung.** Die Schwellendifferenz
ersetzt eine Messung, die es nicht gibt. Solange die Rate je Mitglied fehlt, ist jede feste Zahl an
dieser Stelle in der einen Lage richtig und in der anderen falsch.

**Kein Fork-Defekt:** `HealthTankRatio` 0,45, `HealthHealerRatio` 0,40 und `HealthSelfRatio` 0,40
stehen im Fork auf denselben Werten wie in `upstream/main`. Es sind die **Voreinstellungen im Code**;
welche Werte in der Konfiguration des Auftraggebers stehen, ist von hier aus nicht messbar.

**Zwei Verzerrungen im Prozentvergleich, beide entscheiden über Leben:** Ein Tank bei 50 % hinter
einer Barriere über 25 % seiner maximalen Gesundheit steht effektiv bei 75 %. Und 60 % eines kleinen
Lebenspools sind weniger Punkte als 50 % eines großen — derselbe Treffer tötet den mit den wenigeren
Punkten.

## Gefährdungsklassen statt einer Kette von Kurzschlüssen

**Keines der Einzelmaße trägt allein, und der Grund ist derselbe bei allen: Jedes ist ein Surrogat,
und jedes bricht in einer anderen Lage.** Die Lösung ist deshalb nicht, eines davon zu wählen,
sondern jedes dort einzusetzen, wo es das Richtige misst.

| Maß | Sein Vorteil | Wo es bricht |
|---|---|---|
| Prozentsatz | normiert stillschweigend auf die erwartete Rate — wer mehr Pool hat, nimmt auch mehr Schaden | bei ungerichtetem Flächenschaden, der alle mit derselben absoluten Zahl trifft |
| absolute effektive Punkte | genau dort richtig: sie sagen, wer den nächsten Einschlag nicht überlebt | hält den Tank bei 30 % für sicherer als den Schadensausteiler bei 45 %, obwohl der Tank den Dauerschaden nimmt |
| Rollenvorrang | bildet die Ersetzbarkeit ab, die die Vorgabe verlangt | überholt als Kurzschluss den, der tatsächlich stirbt |
| Rollenschwellen 45/40 | vertreten die fehlende Schadensrate | rollenfest statt lagefest: falsch, sobald der Heiler die Aggro hält |
| Aggro | misst, wer gerichteten Schaden bekommt, statt es aus der Rolle zu schließen | sagt nichts über Flächenschaden |

**Die Zielwahl ordnet die Kandidaten in drei Gefährdungsklassen. Innerhalb einer Klasse entscheidet
das Maß, das für diese Lage das richtige ist; bei Gleichstand die Rolle.**

| Klasse | Wer hineinfällt | Ordnung darin | Warum dieses Maß |
|---|---|---|---|
| **1 — kritisch** | effektive Gesundheit ≤ `HealthForDyingTanks` (Vorgabe 0,15) | niedrigste **absolute** effektive Punkte zuerst | Wer hier steht, stirbt am nächsten Treffer, und ein Treffer ist eine absolute Zahl |
| **2 — Rollenvorrang, bedingt** | Heiler ≤ `HealthHealerRatio` oder Tank ≤ `HealthTankRatio`, **und** unter Beschuss | niedrigster **Prozentsatz** zuerst, bei Gleichstand Heiler vor Tank | Hier ist die Rate rollenproportional, und genau dafür ist der Prozentsatz das brauchbare Surrogat |
| **3 — übrige** | alle anderen | absolute Punkte, solange ein Flächenschaden angekündigt ist (`AreaHitAnnounced`, gleich wie groß); sonst Prozentsatz | Ein Raidwide trifft alle mit derselben Zahl; ohne ihn gilt wieder die Rollenproportionalität. Ohne Größenschwelle, weil gemessene Anteile nur nach unten irren: Ein unterschätzter Treffer ließe gerade im gefährlichen Fall nach Prozent ordnen; bei einem bekannt kleinen kostet die Punkte-Ordnung höchstens die Reihenfolge zweier Verletzter, die der Treffer beide nicht gefährdet |

**Über allen Klassen steht weiterhin die Unterscheidung geschützt/ungeschützt**, und zwar
unverändert: Wer eine Unverwundbarkeit trägt, wird nach hinten gestellt, nicht ausgeschlossen. Ohne
diesen äußersten Schlüssel kehrt sich Klasse 1 gegen sich selbst — ein Krieger der Dunkelheit unter
Superbolide steht auf 1 Trefferpunkt, hätte also die wenigsten absoluten Punkte von allen und stünde
vor jedem echten Notfall. Die Klassenordnung wirkt innerhalb der Ungeschützten. Living Dead mit abgeschalteter
Rückhaltung zählt dabei nicht als Unverwundbarkeit (`ObjectHelper.IsUnprotectedForHealing`, Konzept 09, A306): Sein
Fall auf 0 ist Walking Dead, kein verschonter Treffer.

**Die Gesundheitsschranke in Klasse 2 ist nicht schmückendes Beiwerk, sondern trägt die Klasse.**
Ohne sie wäre jeder Tank dauerhaft in Klasse 2, denn er trägt seine Tankhaltung immer — und ein Tank
bei 65 % stünde vor einem Schadensausteiler bei 30 % in Klasse 3. Das Kandidatenfeld fängt das nicht
ab: `healRatio` filtert bei 0,70, beide sind darin. Die beiden Rollenschwellen behalten damit genau
ihre Bedeutung und bekommen zusätzlich die Bedingung, die ihnen fehlte.

**Gleichstandsregel in jeder Klasse: Heiler vor Tank vor Schadensausteiler** — die Triage der
Vorgabe, und nur dort, wo sie hingehört, nämlich bei gleicher Gefährdung.

**Was die Klassenordnung von jedem Maß nimmt und was sie ablegt:**

- Der Rollenvorrang bleibt erhalten, **überholt aber niemanden mehr**, der tiefer steht: Er wirkt
  nur noch innerhalb einer Klasse. Der Schadensausteiler bei 10 % steht in Klasse 1, der Tank bei
  44 % in Klasse 2 — die Reihenfolge ist damit entschieden, bevor die Rolle überhaupt gefragt wird.
- Der Prozentsatz behält die Lage, in der er richtig ist, und verliert die, in der er blind ist.
- Die absoluten Punkte bekommen genau die zwei Lagen, für die sie gebaut sind, und keine weitere.
- Die Aggro tritt **neben** die Rollenschwellen, statt sie zu ersetzen: Die Schwelle sagt weiterhin
  „tief genug", die Aggro „und er bekommt tatsächlich Schaden". Damit ist die Umkehr der Schwellen
  entschärft — hält der Heiler die Aggro und der Tank nicht, steht der Heiler in Klasse 2 und der
  Tank in Klasse 3, obwohl die Zahlen unverändert sind.
- Auch der Selbst-Kurzschluss rückt hinter Klasse 1. Er trägt denselben Überholfehler wie die beiden
  Rollenzweige: Vor dem Umbau wurde ein Schadensausteiler bei 10 % übergangen, sobald der Spieler selbst bei
  39 % stand.
- **Kein Maß wird gewichtet und keine Zahl erfunden.** Die Klassen sind Ja/Nein-Fragen an
  vorhandene Größen; innerhalb einer Klasse wird verglichen, nicht verrechnet. Das ist der
  Unterschied zu einer gemeinsamen Gefährdungszahl, die einen Nenner bräuchte, den es nicht gibt:
  BossModReborn nennt Art und Zeitpunkt des nächsten Einschlags (`BMRNextDamageType`,
  `BMRNextDamageIn`), **nicht seine Höhe**.

**Die Schwellendifferenz 45/40 in Klasse 2 (A283):** Die Werte bleiben wirksam und unverändert — sie sagen,
ab wann eine Rolle zuerst kommt. Die Umkehr im Band ist trotzdem weg: Steht ein beschossener Heiler gleich tief
oder tiefer als der Tank, den seine Schwelle in Klasse 2 bringt, kommt er dazu und gewinnt nach der Gesundheit.
Ausgeschlossen: eine gemeinsame Schwelle für beide Rollen (sie legte eine Einstellung des Nutzers still) und
eine neue Voreinstellung für den Heiler (globale Einstellungen bleiben in einer gespeicherten Konfiguration
stehen, sie erreichte ihn nicht).

**Gebaut (A183):** Klassen 2 und 3 hinter `HealTargetByDanger` („Choose the heal target by danger"), Vorgabewert an (seine Regel für Voreinstellungen, 29.09.2026), in `ActionTargetInfo.DangerClassTarget`. „Unter Beschuss" liest `DataCenter.TargetedPartyMembers` (je Bild aus den Zielen der Gegner). Klasse 1 und der Selbst-Kurzschluss stehen unverändert davor; die Geschützten bleiben zuletzt. Klasse 3 reiht nur Verletzte: Nach Punkten gereiht gewann sonst ein Unverletzter mit kleinem Pool, und niemand wurde geheilt (A210).

**Die Rate steht.** `DataCenter.RecordedHP` führt die Gruppe seit A91 mit, `GetTTK` antwortet damit
für jede Gruppen-Id, `ScoreTtkForecast` korrigiert die Schätzung gegen ihren eigenen Fehler (A92),
und `GetForecastSurvivingShare` macht daraus die Größe, die alle Heilentscheidungen lesen (A93). Sie
ist kein Ordnungsmerkmal innerhalb von Klasse 2, sondern die vorausberechnete Gesundheit selbst —
siehe „Die Rate geht über die vorausberechnete Gesundheit ein".

**Die schwächere Alternative ist verworfen und bleibt es:** `DataCenter.DPSTaken` misst die Gruppe
als Ganzes, `DamageRec` trägt Zeitpunkt und Anteil, **kein Ziel**. Ihr Fenster umfasst seit upstream 7.5.7.5
fünf Sekunden (vorher fünf Millisekunden, A331); am Grund ändert das nichts, denn eine Rate der Gruppe sagt nicht,
wen der Strom trifft.

**Die Aggro steht ebenfalls.** `DataCenter.TargetedPartyMembers` wird in `TargetUpdater.UpdateLists` einmal
je Bild aus den `TargetObjectId` der Gegner gefüllt — ein Durchlauf über die Gegner, danach ist „wird
angegriffen" eine Nachschlageoperation. Gelesen wird sie von `ObjectHelper.IsUnderThreat` (Wahl des **Mittels**)
und von `DangerClassTarget` (Klasse 2 der Zielwahl).

**Kosten der Erhebung, gemessen am Ort:** Die Aggro braucht keinen Vergleich je Mitglied. In
`TargetUpdater.UpdateLists`, wo `AllHostileTargets` ohnehin einmal je Bild aufgebaut wird, sammelt
ein Durchlauf über die Gegner deren `TargetObjectId` in ein Set — danach ist „wird angegriffen" eine
Nachschlageoperation. Der Aufwand wächst mit der Zahl der Gegner, nicht mit ihrem Produkt.

## Die Bausteine

| Baustein | Inhalt | Stand |
|---|---|---|
| **1** | Klasse 1 vor alle drei Kurzschlüsse ziehen | **umgesetzt** in `ActionTargetInfo.GeneralHealTarget`: ungeschützte Kandidaten auf oder unter `HealthForDyingTanks` (effektive Gesundheit, wie `CanProvoke` sie liest) werden vor Selbst-, Heiler- und Tankzweig zurückgegeben, geordnet nach absoluten effektiven Punkten, bei Gleichstand Heiler vor Tank vor Schadensausteiler |
| **2** | Klassen 2 und 3 mit Aggro und lageabhängigem Maß | **umgesetzt** (A183) hinter `HealTargetByDanger`, ab Werk an |
| **3** | Rate je Mitglied | **umgesetzt** als vorausberechnete Gesundheit — siehe unten |

### Die Rate geht über die vorausberechnete Gesundheit ein

Die Rate ist kein **zusätzliches** Ordnungsmerkmal innerhalb von Klasse 2; die **gelesene Gesundheit selbst**
ist vorausberechnet (A93). Jede der vier Entscheidungen dieser Methode — die Kandidatenordnung, die Schwelle
der kritischen Klasse, ihre Ordnung nach absoluten Punkten und der Selbstkurzschluss — liest
`ObjectHelper.GetForecast*` statt der aktuellen Größe.

**Warum so:** Ein zusätzliches Merkmal hätte eine zweite Rangregel erzeugt, die mit der
ersten auseinanderläuft, sobald eine von beiden angefasst wird. Die Ersetzung ändert **eine** Größe,
und alle vorhandenen Klassen, Schwellen und Kurzschlüsse erben die Vorausschau, ohne umgebaut zu
werden. Sie wirkt zudem auf Klasse 1 mit — und gerade dort entscheidet sich, wer stirbt.

**Was sie im Kampf ändert:** Ein Schwarzmagier bei 48 %, dessen Gesundheit in vier Sekunden
aufgebraucht ist, steht prognostiziert bei 3 % und fällt damit in Klasse 1 — vor den Tank-Kurzschluss,
der ihn bei 44 % des Tanks vor dem Umbau überholte. Rechnung und Grenzfälle in Konzept 08, Abschnitt „Der
Verbraucher".

Hinter `HealAheadOfDamage`, Standard an (seine Regel für Voreinstellungen, 29.09.2026); ausgeschaltet liefern alle vier Getter die Werte ohne Vorausschau.
Die Vorausschau steuert sich selbst nach: `ScoreTtkForecast` hält jede Vorhersage gegen den Verlauf und rechnet den
Fehler heraus.

## Die Zielüberschreibung hebt die ganze Rangfolge auf

**Alles oben Beschriebene gilt nur, solange eine Aktion `FindHealTarget` überhaupt erreicht.** Ein
`targetOverride` ersetzt die Rangliste durch **eine einzige Sortierung** — und dabei fällt auch der
Bedarfsfilter weg, den `FindHealTarget` an seinem Eingang führt (`GetForecastHealthRatio() <
healRatio`, `AutoHealRatio` 0,80 als Vorgabewert im Code). Eine überschriebene Zielwahl kann deshalb
auf einen Unverletzten zeigen; die Rangfolge kann das nicht.

**Im Heilpfad gilt diese Rangfolge für jede Heilaktion, auch für eine mit eigener Zielart.** Vor den Heilungen setzt
der GCD-Pfad die Zielart auf `TargetType.Heal` und schaltet den Bedarfsfilter ein (`CustomRotation_GCD`,
`IBaseAction.TargetOverride`, `AutoHealCheck`); `ActionSetting.TargetType` liest die Überschreibung vor der eigenen
Zielart. Die eigene Zielart einer Heilaktion – Regen des Weißmagiers trägt `TargetType.LowHPPercent` – greift nur
außerhalb davon. Ein Mitglied, das den HoT schon trägt, scheidet für ihn aus (`TargetStatusProvide`); die Heilung fällt
dann auf das nächstgefährdete Mitglied mit Bedarf. Am Prätorium (Stufe 50, sein Upload vom 01.11.2026, A476) gingen so
fünf Regen an den Schwarzmagier, jedes Mal bei 46 bis 70 %, unter dem Filter und nach Klasse 3: kein Gegner zielte auf
ihn, nichts war angekündigt, er stand nach jedem Gruppentreffer nach Anteil und nach Punkten am tiefsten, und der Tank
trug Regen schon. Das Mittel folgt der Gefahr (oben): HoT. Vor dem nächsten Gruppentreffer war er jedes Mal voll;
Veni Vidi Vici, ohne Wirkbalken, ließ ihn mit 34 % statt rechnerisch etwa 8 % stehen. Wirksam waren 33 bis 63 % der
Ticks, wo Medica II mitlief, samt ihren – auf dem kleinsten Lebenspool füllen drei Ticks auf, wie Resedra (48 %
Überheilung, Konzept 18) nicht die Ausnahme.

### Die Notfallheilung zwischen zwei Zaubern

**Was geschah** (Große Gubal-Bibliothek, sein Upload vom 01.11.2026, A478). Ein ganzes Pack schlug auf den Tank ein,
der keine eigene Minderung nahm; er fiel in 3 s von 82 % auf 43 LP. Der Weißmagier wirkte viermal Cure II hintereinander
(17:27:19 bis 17:27:26) – den ersten auf sich selbst, der bei etwa 50 % selbst angegriffen wurde (Rang 2, ein Schluss aus
dem Verlauf; der Trace nennt die Wahl nicht), dann drei auf den Tank. Benediktion war im ganzen Dungeon unbenutzt, also
bereit, der Tank unter Bedrohung und unter ihrer Schwelle – sie ging nicht hinaus. Die erste Fähigkeit nach der Kette war
Assise, nach einem sofort wirkenden Zauber (17:27:27,7).

**Warum es keine Lücke gab.** Zwei Sperren halten Fähigkeiten aus der letzten halben Sekunde vor dem GCD heraus: die Wahl
(`CustomRotation.Ability`: `WeaponRemain <= 0.5f`, und `Invoke` meldet ab dem Vorlauf „Action ahead", ab Werk einem
Viertel des GCD, nur noch den GCD) und das Drücken (`RSCommands.DoAction`: keine Fähigkeit bei `0 < DefaultGCDRemain <= 0.5`).
Nach einem hart gewirkten Zauber bleibt bis zum nächsten GCD dessen Abklingzeit minus Wirkzeit, beide im selben Maß
verkürzt durch Zaubertempo und Geistesgegenwart. Cure II wirkt 2,0 s bei 2,5 s Abklingzeit (Spielblatt, xivapi,
02.11.2026); es bleiben höchstens 0,5 s, an jenem Abend unter Geistesgegenwart 1,53 s Wirkzeit in einem GCD von 1,87 bis
1,96 s, also 0,35 bis 0,4 s. Zwischen zwei Cure II geht damit keine Fähigkeit – nicht als Zufall, sondern bei jedem
Zaubertempo. Dasselbe gilt für jeden Heilzauber mit 2,0 s Wirkzeit: Medica II und Cure III (Weißmagier), Adloquium und
Succor (Gelehrter), Prognosis (Weiser). Nach 1,5 s Wirkzeit – Benefic II, Helios, Helios Conjunction (Astrologe),
Diagnosis (Weiser), Glare III und Holy III – bleibt rund eine Sekunde, abzüglich der Sperre ein Fenster von 0,27 bis
0,5 s je nach Tempo (`emergency_weave_model.py`).

**Was gilt** (A479). Steht ein ungeschütztes Mitglied der Gruppe in Klasse 1 – seine vorausberechnete effektive Gesundheit
bei oder unter `HealthForDyingTanks`, ab Werk 15 % –, fragt `Invoke` die automatischen Heilfähigkeiten des Jobs auch dann,
wenn der GCD schon ansteht, und meldet eine, die hinausgehen kann, vor dem GCD. Gedrückt wird sie, sobald der GCD frei
ist (die Sperre in `DoAction` bleibt; bei 0 s Rest lässt sie Fähigkeiten zu), und der nächste GCD folgt nach ihrer
Ausführungssperre. Was die Fähigkeit ist und auf wen sie geht, entscheiden wie bisher die Heilpfade des Jobs mit ihren
eigenen Schwellen und Bedingungen – Benediktion etwa weiter erst unter „Minimum health threshold party member needs to be
to use Benediction" und unter Bedrohung (oben, „Das Mittel richtet sich nach der Gefahr"); die Rangfolge stellt das
Mitglied in Klasse 1 an die Spitze. Geändert wird allein der Zeitpunkt, und nur für eine Fähigkeit, die ein Mitglied in
Klasse 1 erreicht – als ihr Ziel oder unter den Getroffenen einer Flächenheilung. Wählt der Pfad etwas anderes, etwa
Second Wind für einen Schadensausteiler bei 40 %, während der Tank in Klasse 1 steht, wartet es wie bisher auf ein
Fenster; den GCD um eine Ausführungssperre zu schieben, lohnt nur für den, der sonst fällt. Die allgemeinen Sperren des
Fähigkeitenpfads gelten weiter: nichts während eines Zaubers, unter `NoCasting`, mit abgeschalteten Fähigkeiten, während
Mudra, unter Fähigkeitssperre (Shackled Abilities) oder solange eine Kanalisierung gehalten wird (Passage of Arms,
Collective Unconscious, Phantom Flurry nach seinen Einstellungen). Ein Befehl („next action“) und ein Gegenstand werden
nicht vorgezogen. Andere Fähigkeiten – Angriff, Unterbrechung, Abwehr – bleiben hinter den Sperren. Jede so vorgezogene
Heilung schreibt eine Zeile ins `DefenseTrace.log`; steht ein Mitglied in Klasse 1 und geht keine Heilfähigkeit für es,
einmal je Mitglied und GCD eine Zeile mit diesem Befund.

*Benediktion bleibt der absolute Notfall* (seine Worte, 02.11.2026: „benediktion soll ja erst als absoluter notfall
gelten“ – seine Vorgabe von oben, „Das Mittel richtet sich nach der Gefahr“, nicht eine neue Linie). Die Regel macht sie
nicht früher, sie macht den Notfall erreichbar: Vorgezogen wird nur in Klasse 1, also wenn das Mitglied fiele, bevor
eine jetzt begonnene GCD-Heilung landet – die engste Fassung von „absolut“, die sich am Kampf messen lässt. Absolut ist
ein Notfall dabei am Augenblick der Entscheidung gemessen, nicht an der Anzeige: Für einen Heiler, der hart wirkt, ist der
freie GCD der letzte Augenblick vor 1,5 bis 2 s ohne jede Fähigkeit. Wer erst bei 10 % auf dem Bildschirm drücken wollte,
stünde dann mitten im Zauber. Außerhalb von Klasse 1 gelten für Benediktion unverändert ihre Schwelle und die Gefahr
(oben); für einen Tank ist die Gefahr durch seine Aggro stets erfüllt, dort entscheidet ihre Schwelle.

*Woran die Wahrscheinlichkeit gemessen wird* (seine Vorgabe für jede Sicherheitsregel): an Klasse 1 – der gemessenen
Schadensrate, über die sich selbst korrigierende Restzeit (`GetCorrectedTTK`) bis zur Landung einer jetzt begonnenen
GCD-Heilung fortgeschrieben, gegen die Linie, die er für den sterbenden Tank setzt.

*Im Kampf, am Fall vom 01.11.:* Als der GCD um 17:27:21,3 frei wurde (der erste Cure II hatte um 20,9 geendet), stand der
Tank bei 24 % und verlor 2 300 bis 2 900 LP je Sekunde (über die letzte und die letzten zwei Sekunden gemessen); tot wäre
er in 1,2 bis 1,5 s gewesen, ein dann begonnener Cure II landet nach 2,1 s – Klasse 1. Benediktions eigene Bedingungen
hielten ebenso: Sie liest die Gesundheit bis zum Ende der laufenden Sperre, beim freien GCD also die jetzige (24 % unter
ihrer Schwelle ab Werk von 30 %), und der Tank stand unter Angriff. Benediktion wäre gemeldet und gedrückt worden. Später
ging es nicht: Der zweite Cure II begann in diesem Augenblick; nach seiner Wirkzeit (22,8) stand der Tank bei 712 LP, und
bis zum GCD danach hielt die Sperre jede Fähigkeit zurück. Eine sofort wirkende Heilung ändert die LP 0,53 s nach ihrer
Zeile im Mitschnitt (26 Heilungen jenes Abends gemessen); mit 0 bis 0,3 s vom Drücken bis zu dieser Zeile (angenommen)
fällt der Tank bis zur Landung weiter auf 10 bis 22 % – kein Mangel, sondern die Lage, für die sie da ist – und stünde um
17:27:22,8, nach dem Schaden, der tatsächlich kam, bei 78 bis 90 % statt bei 43 LP (`emergency_weave_model.py`,
Nachrechnung aus dem Mitschnitt). Der zweite Cure II begänne um die Ausführungssperre der Fähigkeit später.

*Lagen:* Ein Heiler, der nur harte Zauber kettet, kommt in jeder Notlage an seine Notfallheilung, nicht erst nach dem
nächsten sofortigen Zauber. Mit Lilien oder Eukrasie ist die Lücke meist ohnehin da; dann ändert sich nichts. Unter
Stufe 50 hat der Weißmagier keine Benediktion, die übrigen Heilfähigkeiten folgen ihren Stufen. Gilt für jeden Job, dessen
Heilpfade Fähigkeiten liefern (universell), im PvE; ein Paladin oder Krieger hat dort Selbstheilungen, die ebenso nur
greifen, wo ihre Bedingungen es erlauben. BossModReborn liest die Regel nicht.

**Preis und Gegenwirkung.** Je vorgezogener Fähigkeit rückt der nächste GCD um ihre Ausführungssperre nach hinten (in
keinem Spielblatt; der Server nennt sie im Effektpaket, `AnimationLockTime`; in Community-Angaben um 0,6 s). Mehrere
Fähigkeiten hintereinander, solange das Mitglied in Klasse 1 bleibt, verschieben ihn mehrfach – jede heilt sofort, also
endet Klasse 1 mit der ersten, die genug heilt. Eine zweite allein deshalb, weil die LP der ersten erst 0,53 s später
steigen, gibt es nicht: Vor dem Ende ihrer Ausführungssperre drückt `DoAction` nichts, und sobald ihr Effektpaket da ist,
rechnet `RefinedHP` ihre Heilung vorab an (`HealHP`, bis zur Sperre plus 1 s; `Watcher`, `DataCenter.GetPartyMemberHPRatio`).
Eine anstehende Wiederbelebung bleibt, wo sie ist: Spontanität im Notfallzweig des Fähigkeitenpfads, der Zauber im GCD –
die Heilfähigkeit geht vor diesem GCD.
Ein falscher Alarm der Vorhersage (ein Ratensprung, der abebbt) kostet dieselbe Verschiebung; ob die Fähigkeit selbst
geht, entscheiden weiter ihre eigenen Bedingungen, eine Benediktion geht also nicht auf jemanden ohne Gefahr. Rest: Hat
`DoAction` den GCD schon in die Warteschlange des Spiels gegeben, bevor das Mitglied in Klasse 1 rückt, beginnt dieser
GCD, und die Fähigkeit wartet bis nach ihm – wie heute.

**Verworfen.**
- *Die Sperre senken* (0,5 s auf weniger): verschiebt den Fundort, nicht die Zuständigkeit (Konzept 11, „Entstehung"),
  und clippte jeden GCD ohne Not.
- *In der Not sofort wirkende GCD-Heilungen vorziehen*, damit eine Lücke entsteht: jobweise verschieden und nicht immer da –
  an jenem Abend waren die Lilien gerade ausgegeben (dreimal Afflatus solatii bis 17:27:13).
- *Den laufenden Zauber abbrechen*, um die Fähigkeit einzuschieben: kostet die Wirkzeit, und der Zauber, der läuft, heilt
  in der Not meist das gefährdete Mitglied.
- *Den Leser in `DoAction` öffnen:* hebelte die Sperre für jede Fähigkeit aus (Konzept 11, A388).
- *Nichts ändern:* Ein Heiler, der in der Not Cure II kettet, kommt nie an Benediktion – genau die Lage, für die sie da ist.

**Gegenhypothesen.** *Kein Defekt:* Der Tank überlebte mit 43 LP, durch den dritten Cure II und eine Minderung, die er
bei 5 % selbst nahm; die Notfallheilung, die für diese Lage gebaut ist, war bereit und ging nicht. *Option falsch:* die
Verworfenen oben; Annahme „die Sperre in `DoAction` lässt bei 0 s Rest zu" – am Code belegt (`DefaultGCDRemain > 0f`);
stimmte sie nicht, ginge die Fähigkeit nach dem GCD statt davor, wie heute. *Ausgeliefert, und nichts ändert sich –
warum?* Kommt die Vorhersage nicht rechtzeitig in Klasse 1 – der Sprung kommt schneller als ihre Rate –, oder gehen die
Heilpfade des Jobs ihrer eigenen Bedingungen wegen nicht – etwa weil seine Schwelle für Benediktion unter dem Stand liegt,
den der Tank beim freien GCD hat (im Fall vom 01.11. unter 24 %); dann kommen die übrigen Heilfähigkeiten des Pfads nach
ihren Bedingungen vor, und Benediktion wartet, wie er es eingestellt hat. Beides zeigt die Trace-Zeile.

### Die Füllheilung: heilen, wenn nichts anderes zu tun ist

**Was sie tut.** „Use GCD heals when there's nothing else to do" (ab Werk an) läuft am Ende des GCD-Pfads, nach Heilung,
Abwehr, Wiederbelebung und Schaden – also nur, wenn kein anderer GCD geht –, im Kampf, nie unter `NoCasting` (der
GCD-Pfad endet davor), nach einer zufälligen Verzögerung („Downtime healing delay range", ab Werk 0,5 bis 1 s).
Auslöser ist das vorausberechnete Minimum der Gruppe (`ForecastMinHp`) unter „When the lowest party HP is below" (ab
Werk 80 %). Dann setzt sie die Zielart auf die Rangfolge (`TargetType.Heal`) samt Bedarfsfilter an ihrer Linie und ruft
die Flächenheilung, wenn mehr als zwei Mitglieder unter der Linie stehen und die Streuung unter „AoE heal HP spread"
liegt, sonst die Einzelheilung – zuerst die der Spezialaktionen, dann die des Jobs (die Regel unten, „Was gilt").

Sie läuft in Pausen ohne angreifbares Ziel – ein Boss wird unanvisierbar, eine Welle ist tot und die nächste noch nicht
da, die Ziele stehen außer Reichweite – und bei einem Zaubernden in Bewegung ohne sofort wirkenden Angriff. Sie gilt
für jeden Job mit einer GCD-Heilung: die vier Heiler, den Paladin (Clemency, hinter seinen eigenen Schwellen), den
Beschwörer (Physick, mit „Healbot" oder bis Stufe 30), den Rotmagier (Vercure), den Blaumagier und die Spezialaktionen
(Okkulte Sichel, Bozja, Variante, Hardboiled). Einen Schadens-GCD kostet sie nie, es gab keinen; sie kostet MP, beim
Weißmagier auch Lilien (Afflatus solatii). Die Abbruchregel schont sie: Eine Füllheilung wird nicht abgebrochen, wenn ihr
Ziel während des Wirkens hochgeheilt wird (`CancelCastUpdater`, `HealingWhileDoingNothing`). Was denselben GCD sonst
nimmt, läuft vorher und bleibt unberührt: die regulären Heilpfade mit Filter, die Wiederbelebung, HoT gegen DoT, die
Pull-Pflege mit Regen auf den Tank (im allgemeinen GCD-Zweig). Eine Vorhersage von BossModReborn liest sie nicht.

**Der Defekt bis A477.** Sie schaltete den Bedarfsfilter (`AutoHealCheck`) nicht ein und heilte so Mitglieder über
ihrer eigenen Linie. Am Prätorium (Stufe 50, sein Upload vom 01.11.2026, A476): Gaius wurde unanvisierbar, der Tank stand
bei 75 % und trug Regen, und die Füllheilung ging als Regen auf den Schwarzmagier bei 95 %. Das widersprach dem Eingang
dieses Konzepts („`FindHealTarget` bekommt bereits ein durch `healRatio` gefiltertes Feld") und seiner Vorgabe für die
Heilung („es soll ja nicht großartig überheilt werden", Konzept 18). Dieselbe Lücke hatte ihre Flächenheilung, zweifach:
Drei Mitglieder bei 95 % zählten als „nicht voll" und öffneten sie, und gewirkt wurde sie, sobald jemand in Reichweite
stand – auch wenn der eine unter der Linie außer Reichweite stand.

**Was gilt.** Die Füllheilung heilt, wer unter ihrer Linie steht:
- Der Bedarfsfilter gilt in ihr, mit der Linie aus „When the lowest party HP is below" statt der Heilquote der Aktion –
  für das Ziel der Einzelheilung und für die Frage, ob eine Flächenheilung um den Wirkenden jemanden mit Bedarf
  erreicht. Unter den Gefilterten ordnet die Rangfolge wie überall.
- Die Flächenheilung öffnet sie, wenn mehr als zwei Mitglieder unter der Linie stehen (vorausberechnet), nicht mehr,
  wenn mehr als zwei nicht voll sind; die Streuung bleibt die zweite Bedingung.
- Das auslösende Mitglied besteht den Filter immer: Auslöser und Filter lesen denselben Anteil
  (`GetPartyMemberHPRatio`, samt der eigenen Heilung im Flug, beim Filter über `RefinedHP`) mit derselben Vorausschau
  für eine GCD-Heilung, dieselben Mitglieder (die Zielwahl einer freundlichen Aktion zieht aus `PartyMembers`,
  `TargetHelper.GetTargetsByRange`) und vergleichen beide mit „kleiner als". Einzige Abweichung: Ein Mitglied unter
  Verhängnis liest die Zielwahl mit 1 %, es besteht den Filter also immer. Ein Mitglied über der Linie besteht ihn nie;
  Tote zählen weder beim Auslöser noch bei der Flächenheilung.
- Jede Füllheilung schreibt eine Zeile ins `DefenseTrace.log`: Aktion, Ziel, dessen vorausberechnete Gesundheit, die
  Linie. Am Prätorium musste sie aus dem ACT-Mitschnitt erschlossen werden; mit der Zeile zeigt sein Upload jede.

*Im Kampf, Lage für Lage:*
- Boss unanvisierbar, Tank unter der Linie ohne HoT: wie bisher, Regen oder Cure II auf ihn.
- Tank unter der Linie mit HoT, alle anderen darüber (der Fall vom Prätorium): keine Regen auf den Vollen. Regen findet
  kein Ziel, Cure II geht auf den Tank – bisher kam Cure II erst im nächsten Pausen-GCD, nach der Regen auf den Vollen.
  Preis: Cure II überheilt um das, was der HoT bis zur Landung bringt.
- Auslöser außer Reichweite: keine Füllheilung, wo bisher ein Mitglied in Reichweite geheilt wurde, das sie nicht
  brauchte. Für den Auslöser ändert sich nichts.
- Ein, zwei Mitglieder unter der Linie, der Rest knapp darüber: Einzelheilung statt einer Flächenheilung über alle.
- Seine Linie auf 100 %: Der Filter lässt jeden durch, der nicht voll ist. Die Rangfolge nahm schon bisher nie einen
  Vollen (ihr letzter Rückfall verlangt weniger als volle Gesundheit); die Flächenheilung zählt, wer nach der Vorausschau
  nicht voll ist, bisher nach dem Stand – ein Mitglied, das gerade fällt, zählt jetzt mit.
- Paladin, Beschwörer, Rotmagier, Blaumagier, Spezialaktionen: dieselbe Grenze, vor ihren eigenen Schwellen.
- Stufensynchron, allein mit Chocobo („Treat your chocobo as a party member"), Achtergruppe, Okkulte Sichel ohne
  Gruppe: nichts Eigenes; Auslöser und Ziele sind die Mitglieder, die `PartyMembers` führt.

**Verworfen.**
- *Der Filter mit der Heilquote der Aktion* (`AutoHealRatio`, ab Werk wie die Linie 80 %): Setzt er die Linie höher,
  etwa 90 %, fiele der Auslöser bei 85 % aus dem Filter, und die Einstellung täte nicht, was ihr Text sagt – der Text
  einer Einstellung bindet (seine Vorgabe).
- *Nur den Auslöser heilen:* überginge die Rangfolge nach Gefahr – ein Heiler bei 79 % unter Aggro käme nach einem
  Damage Dealer bei 75 % ohne.
- *Die HoT-Spalte auf die Linie anwenden* (ein voller HoT senkt die Schwelle, Konzept 18): Die Füllheilung hat keine
  HoT-Einstellung, ihr Text nennt eine Linie; die Senkung wäre eine Regel ohne Einstellung. Ihr Preis, die Überheilung
  des Tanks mit HoT, fällt in eine Pause ohne Schadensverlust und kostet MP.
- *Die Schwellen der regulären Einzelheilung statt der Linie:* Dann heilte sie nur, wo die reguläre ohnehin heilt, und
  die Linie, die er für Pausen eigens setzt, verlöre ihren Zweck.
- *Nichts ändern:* lässt die Heilung auf Mitglieder über der Linie, gegen seine Vorgabe und gegen den Filter, den dieses
  Konzept voraussetzt.

**Gegenhypothesen.** *Kein Defekt:* Eine Heilung auf ein Mitglied über der Linie nützt dem Auslöser nichts und ist
Überheilung; dass sie keinen Schadens-GCD kostet, macht sie nicht nützlich. *Option falsch:* geprüft an den
Verworfenen; dazu die Annahmen – sehen Auslöser und Filter dieselben Mitglieder und dieselbe Vorausschau? Ja, am Code
belegt (oben); wäre es nicht so, fiele höchstens eine Füllheilung aus, nie eine auf einen Vollen. Kostet eine Pause nie
Schaden? Sie läuft nur, wenn kein anderer GCD ging. *Ausgeliefert, und nichts ändert sich – warum?* Steht seine Linie
auf 100 % oder kommt in seinen Kämpfen keine Pause mit jemandem unter der Linie, wirkt die Änderung nicht; die
Trace-Zeile zeigt beides.

**Gerechnet** (`filler_heal_model.py`; zufällige Pausen, Gruppe von 4 oder 8, ein Drittel voll, ein Viertel mit HoT,
jedes achte Mitglied außer Reichweite, Weißmagier unter Stufe 52; Annahmen dort benannt; je Pause, die die Füllheilung
öffnet):

| Linie | heute: Heilung über der Linie | Heilquote der Aktion: über der Linie / Auslöser verfehlt | eigene Linie |
|---|---|---|---|
| 70 % | 8,2 % | 3,2 % / 0 % | 0 / 0 |
| 80 % | 5,2 % | 0 / 0 | 0 / 0 |
| 90 % | 2,5 % | 0 / 1,7 % | 0 / 0 |
| 100 % | 0 | 0 / 2,5 % | 0 / 0 |

Eine Flächenheilung, die niemanden unter der Linie erreicht, kam heute in bis zu 1,2 % der Pausen vor, mit der eigenen
Linie nie. Das Modell prüft die Entscheidung, nicht Heilmengen.

**Welche Sortierung richtig ist, hängt an der Aktion, und beide Antworten kommen vor:**

| Maß | Beantwortet | Richtig für |
|---|---|---|
| `TargetType.LowHP` — aktuelle Gesundheit in **Punkten** | Wer überlebt den nächsten Einschlag nicht? | eine Barriere gegen **einen** angekündigten Treffer; das ist die Ordnung der Gefährdungsklasse 1 |
| `TargetType.LowHPPercent` — **Anteil** der Maximalgesundheit | Wer ist am weitesten vom sicheren Stand entfernt? | Heilung, und jede Aktion, deren eigener Wirktext eine Prozentschwelle nennt |

**Vorgabe des Auftraggebers zu Rekindle** (Beschwörer, Phönix-Phase): „Auf wen geht der Single hot
bei Phoenix? Am besten auf den mit der geringsten prozentualen hp, ansonsten auf den Caster selbst.“

**Der Code sortierte nach Punkten, und das ist hier belegbar falsch** — nicht nur abweichend von der
Vorgabe. Die Lebenspools der Rollen unterscheiden sich so stark, dass ein Magier bei voller
Gesundheit weniger Punkte tragen kann als ein Tank bei der Hälfte; die Sortierung gab dann den
Vollen zurück, während der Tank weiter fiel, und die Ladung war ausgegeben. Der Beleg gegen das
Punktemaß steht im Wirktext der Aktion selbst: Rekindle bewaffnet seine Nachheilung „when HP falls
below 75%“ — **das Spiel misst diese Aktion in Anteilen.** Ein nach Punkten gewähltes Ziel kann
damit eines sein, bei dem die Nachwirkung nie auslöst.

**Der Rückfall auf den Wirkenden ist keine Förmlichkeit.** Rekindle besteht nur während der
Phönix-Phase; ein Aufruf ohne Ziel ist mit der Phase verloren, und 400 Potenz auf sich selbst sind
mehr als nichts.

**Der Rückfall fällt kurz vor Phasenende, gemessen an der Jobleiste (A137).** Die Phase liest
`SMN_Reborn` aus `InPhoenix` und der Beschwörungszeit (`SummonTimeEndAfterGCD`), nicht aus dem Status
Firebird Trance (3229). Dieser Status macht nach seinem Wirktext Fountain of Fire und Brand of
Purgatory wirkbar und wird in den Basisrotationen sonst nur von `ModifyBrandOfPurgatoryPvP` gelesen. Dass das
Spiel ihn im PvE setzt, ist belegt: Die
Status-Tabelle des Spiels (xivapi, 02.10.2026) kennt genau einen Status dieses Namens, 3229, Job SMN, mit
dem Wirktext „Able to execute Fountain of Fire and Brand of Purgatory" — beides PvE-Aktionen. Die
Jobleiste bleibt die Quelle der Phase, weil sie auch die Restzeit liefert; der Status wäre ein
gleichwertiger Zeuge. Der Vorlauf von drei GCDs ist ein
fester Wert ohne eigenen Loop (`fixed_values.json`, offen).

**Geprüft wird das maschinell:**
`.github/scripts/audit/check_heal_target_measure.py` erhebt aus `ActionId.resx` jede Aktion, deren
Wirktext eine eigene Prozentschwelle nennt, und meldet jede Stelle, die genau eine solche Aktion
nach Punkten sortiert. Es prüft das Maß, nicht die Zielwahl im Ganzen — eine Barriere nach Punkten
zu wählen bleibt zulässig und wird nicht gemeldet.

**Nascent Flash (Krieger) hat seit A226 eine vierte Zielwahl „By danger", die keine Sortierung ersetzt,
sondern diese Rangfolge selbst anfordert** (`TargetType.Heal`): Nascent Flash ist Heilung, Minderung und Barriere
zugleich, und seine Vorgabe dazu — Heiler vor Schadensausteiler, geprüft, was beim Ziel schon liegt — ist die
Ordnung dieses Konzepts. Die drei Upstream-Einstellungen (niedrigster Prozentsatz, Heiler zuerst, nur Heiler)
bleiben, weil ihr Text bindet. Konzept 09, Abschnitt zum Krieger.

**Die übrigen Fundstellen derselben Bauform sind erhoben und bleiben unbearbeitet**, weil bei ihnen
das Punktemaß nach der Tabelle oben das richtige sein kann: The Blackest Night und Oblation beim
Dunkelritter, Heart of Corundum, Heart of Stone und Aurora beim Revolverklinge. Beim Dunkelritter
hängt eine zweite Prüfung an derselben Wahl — das gewählte Ziel muss zusätzlich unter
`BlackLanternRatio` stehen —, sodass eine Fehlwahl die Aktion nicht nur verschiebt, sondern ganz
ausfallen lässt. Die Entscheidung darüber berührt `10-drk-blackest-night.md` und gehört dem
Auftraggeber. Die Fundstellen in den PvP-Rotationen liegen außerhalb seines Nutzungsprofils.

## Heilziele außerhalb der Gruppe: Figuren, die nur Einzelheilung besiegt

**Hinweis des Auftraggebers (03.11.2026):** „bei unreal shinryu erscheinen mehrmals in den phasen adds, welche mit
spielern getetherd sind. diese mobs kann man nicht bekämpfen, um sie zu vernichten muss man sie heilen. hier greifen
keine aoes, sondern nur direkte heilungen mit ihnen als target." Dazu sein zweiter Hinweis (03.11.2026): „die vier
mobs heißen Reiryu oder ähnlich, sind nicht rot als böse gefärbt, sondern namen sind grün und sie sind meist mit den
damagedealern getethered. einer pro spieler". Sein dritter (03.11.2026): „man muss sie voll heilen, sonst töten sie
irgendwann die getetherten spieler, sie starten mit low health, wahrscheinlich 1HP." Er nutzt BossModReborn, nicht
ffxiv_bossmod (seine Angabe, 03.11.2026).

**Die Mechanik, am Spiel geprüft.**
- *Inhalt und Figur:* Traumprüfung - Heldenlied von Shinryu (ContentFinderCondition 1118, Stufe 100, Patch 7.5). Die
  Figuren heißen Reiryu (BNpcName 6856, BNpcBase 19945 = 0x4DE9), vier je Welle.
- *Seite:* Das Spielblatt führt Reiryu im Battalion 0, dem der Spieler, und Battalion 0 ist sich selbst nicht feind
  (`IsEnemyTo[0]` falsch). Alle übrigen Figuren des Kampfes (BNpcBase 19940 bis 19947 außer 19945) stehen im
  Battalion 4, das Battalion 0 feind ist (xivapi v2, 03.11.2026). Das Spiel stellt Reiryu also auf die Seite der
  Gruppe: Angreifen lässt es sich nicht, heilen wie ein Verbündeter.
- *Ablauf:* Die vier Reiryu erscheinen, je einer leint sich an einen Spieler, meist an die Schadensausteiler (Tether 1,
  Reiryu → Spieler; das Wiki nennt alle vier Schadensausteiler, er sah „meist"), und sie wirken Drachenflug (30 s).
  Für die Erkennung trägt die Rolle des Angeleinten nichts. Sie erscheinen mit 1 LP und
  sterben, sobald sie direkt voll geheilt sind; Flächenheilung wirkt weder auf sie noch auf die Angeleinten. Läuft
  Drachenflug durch oder stirbt ein Angeleinter, fliegen sie auf: Schaden und 60 s Schaden-runter auf die ganze Gruppe
  (Wiki); ungeheilt töten sie mit der Zeit die Angeleinten (sein Hinweis). In Phase 1 einmal (Zeitleiste von
  BossModReborn: Erscheinen 214 s, Ende Drachenflug 251,5 s nach dem ersten Cast), in Phase 2 einmal je Rotation, bis
  zu zweimal. Quellen: BossModReborn, Modul `Un2Shinryu` (Commit 89387f4 vom 20.10.2026, aus zwei Mitschnitten des
  Autors; Klon auf 577675d vom 31.10.2026), Werkzeugquelle; consolegameswiki, Seite „Shinryu's Domain (Unreal)",
  zuletzt bearbeitet am 01.07.2026, Gemeinschaftsquelle. Die Folgen des Scheiterns und Phase 2 stehen nur im Wiki.
- *Gemessen in seinem Mitschnitt* (ACT, 03.11.2026, zwei Läufe, vier Wellen, 16 Reiryu; er spielte Beschwörer, die
  beiden Heiler waren andere Spieler):
  - Jede Reiryu erscheint mit 1 von 105 448 LP (ACT-Zeile 03, BNpcBase 19945).
  - Leine und Drachenflug beginnen 7,8 bis 8,4 s nach dem Erscheinen; Drachenflug wirkt 29,7 s. Vom Erscheinen bis zum
    Ende des Zaubers bleiben also rund 38 s.
  - Die Heiler begannen 3 bis 5,5 s nach dem Erscheinen, vor der Leine. Die Reiryu, die vor der Leine voll war, leinte
    niemanden an: In jeder Welle gab es drei Leinen und drei Flüche, nicht vier. Je früher die Heilung, desto weniger
    Spieler tragen den Fluch.
  - Eine Benediction heilte eine Reiryu in einem Zug voll. Sonst brauchte jede drei Heilungen: Tetragrammaton, Afflatus
    Solace, Cure II, Druochole und Diagnosis brachten je 19 000 bis 50 000 LP.
  - Alle 16 starben 5,6 bis 27,3 s nach dem Erscheinen (ACT-Zeile 25 ohne Quelle). Kein Drachenflug lief durch, kein
    Angeleinter starb an ihnen.
  - Der Fluch endet mit dem Tod seiner Reiryu (ACT-Zeile 30).
- *Fluch des Drachen (Status 1419), den die Leine auf den Angeleinten legt:* „Fluch des Drachen verursacht schrittweise
  Schaden und LP-Regeneration ist angehalten. Regeneration wird kurzzeitig aktiv, wenn selbst eine Attacke ausgeführt
  wird." „LP-Regeneration" heißt in den deutschen Texten jede Erholung: Chaoserde (Status 1605) „Jede Form von
  LP-Regeneration ist wirkungslos", englisch „all HP recovery is nullified". Natürliche Erholung gibt es im Kampf
  nicht, der Satz meint also Heilung (Schluss; das Wiki sagt „preventing healing").

**Was RSR tut (A516).** Die Reiryu heilt jeder Heiler unter RSR, nach einem Notfall in der Gruppe und vor allem
anderen; Nichtheiler springen ein, wenn die Frist ohne sie nicht reicht. Auf die Angeleinten heilt RSR nicht mehr.
Angegriffen werden die Reiryu nicht: `ObjectHelper.IsEnemy` fragt das Spiel, ob Blizzard auf die Figur ginge
(`ActionManager.CanUseActionOnTarget`), und auf eine Figur des Battalion 0 geht er nicht.

- *Erkennung* (`HealToDefeat`, zentral für alle Jobs, je Frame nach den Zielen, nicht im PvP und nur im Kampf): eine
  Kampffigur (BattleNpc), die kein Gruppen- oder Allianzmitglied ist, kein Begleiter, kein Chocobo und kein
  NPC-Gruppenmitglied; die das Spiel nicht angreifen lässt; die lebt und unter voller Gesundheit steht; und die eines
  von zwei Zeichen trägt. Entweder wurde sie mit 1 LP gesehen, dem wenigsten, was ein Lebender hat; so erscheinen die
  Reiryu. Oder sie leint ein Gruppenmitglied an: Leine in ihrer eigenen Effektliste (`Vfx.Tethers`), Ziel ein
  Mitglied. Die Erkennung behält die Figur, bis sie verschwindet; ein Gebietswechsel leert sie. Liefert eine eigene
  Heilung auf sie 0 LP, fällt ihre Art für dieses Gebiet heraus. Diese Selbstkorrektur greift bei einer Figur, die so
  aussieht, aber keine Heilung annimmt.
- *Kandidaten:* Die Figuren stehen in einer eigenen Liste (`DataCenter.HealToDefeatTargets`), nicht in `PartyMembers`.
  Gruppenwerte, Flächenschwellen und Minderung lesen sie nicht. Nur die Zielwahl einer Einzelheilung nimmt sie auf: Die
  Aktion ist freundlich, hat ein Ziel ohne Wirkungsradius und ohne Bodenziel, gibt keinen HoT (`GrantsSingleHot`), und
  das Spiel erlaubt sie auf die Figur (`CanUseActionOnTarget` je Aktion). Damit fallen Flächenheilungen heraus (sie
  wirken nicht), Regen und die anderen HoTs, und die Schilde und Minderungen, die nur Gruppenmitglieder treffen.
- *Flaggen:* Steht eine Figur, setzt `StateUpdater` die Einzelheilflaggen (Fähigkeit und Zauber), für Heiler immer, für
  Nichtheiler nur unter „Help heal figures only healing defeats as a non-healer" (ab Werk an) und nur, wenn die Frist
  ohne sie nicht reicht.
- *Rangfolge* (`FindHealTarget`):
  1. ein Mitglied in Klasse 1, das gleich stirbt;
  2. die Figuren;
  3. die übrigen Klassen.
  
  Unter den Figuren geht zuerst die, die ein Mitglied anleint, das selbst nicht mehr heilbar ist und am tiefsten
  steht: Den Verfluchten rettet nur der Tod seiner Reiryu. Sonst geht die mit dem höchsten Anteil zuerst, damit eine
  nach der anderen fertig wird; jede fertige beendet einen Fluch oder verhindert ihn. Die Schranke `healRatio` gilt für
  Figuren nicht: Eine Reiryu bei 94 % braucht die letzte Heilung so dringend wie die erste, sonst läuft Drachenflug aus.
- *Benediction* bleibt an die Gefahr gebunden (Abschnitt „Das Mittel richtet sich nach der Gefahr"); die Reiryu ist
  kein bedrohtes Mitglied. Gerechnet am Mitschnitt reicht ein Weißmagier ohne sie: 10 bis 12 Heilungen zu 31 000 bis
  45 000 LP (Afflatus Solace, Tetragrammaton, Cure II) für vier Reiryu, rund 30 s nach dem Erscheinen; die Frist
  endet nach rund 38 s. Mit Benediction schaffte es der Weißmagier seiner Gruppe in 17 bis 23 s.
- *Frist für Nichtheiler* (`HealToDefeat.Behind`): Läuft auf einer Figur ein Zauber, ist seine Restzeit die Frist
  (Drachenflug 29,7 s). Der Bedarf ist die Summe der fehlenden LP. Die Rate ist die Summe der gewonnenen LP aller
  Figuren seit dem ersten Sehen, geteilt durch die vergangene Zeit; sie folgt den tatsächlichen Heilern. Reicht Rate mal
  Frist nicht für den Bedarf, springen Nichtheiler ein: Rotmagier mit Vercure, Paladin mit Clemency, Beschwörer mit
  Physick. Ihre Job-Schalter für Heilungen (Healbot, Prevent heal) gelten dann nicht. Vor dem ersten Zauber gibt es
  keine Frist, und die Nichtheiler warten; die Heiler heilen ab dem Erscheinen.
- *Die Angeleinten:* Fluch des Drachen (1419) und Chaoserde (1605) stehen in `StatusHelper.HealingIneffectiveStatus`.
  Beleg im Mitschnitt: Lux Solaris heilte die Verfluchten dreimal um 0 LP. Wer einen davon trägt, ist kein Ziel einer
  Einzelheilung, zählt nicht für die Einzelheilflaggen (`TryHealSingleThreshold`) und nicht für eine Flächenheilung um
  den Wirkenden. Die Gruppenmittel der Flächenschwellen bleiben, wie sie sind: Die Verfluchten hielten sich durch
  Lebensraub hoch (sein Beschwörer 88 000 bis 112 650 von 112 650 LP), und ein Ausschluss änderte viele Leser ohne Nutzen.
- *Unabhängig von BossModReborn:* Erkennung, Frist und Rangfolge lesen nur das Spiel. BossModReborn zeigt „Heal Reiryu
  to full before the cast ends", färbt die Reiryu und sperrt sie für seine eigene KI als Angriffsziel
  (`AIHints.Enemy.PriorityForbidden`); heilen tut es nicht.
- *Messmittel* (`DefenseTrace.log`):
  - eine Zeile je erkannter Figur mit Name, Art, LP und Zeichen;
  - eine Zeile, wenn Nichtheiler einspringen, mit Frist, Bedarf und Rate;
  - eine Zeile, wenn die letzte einer Welle fort ist: Zahl, Dauer, wie oft RSR geheilt hat und welche ungeheilt
    verschwand.

**Bilanz im Kampf.** Besser: Spielt er Heiler, heilt RSR die Reiryu ab dem Erscheinen. Eine, die vor ihrer Leine voll
ist, verflucht niemanden. Ohne die Regel blieb das am menschlichen Mitheiler, und allein schafft der es knapp. Spielt
er Schadensausteiler, kostet die Regel nichts, solange die Heiler in der Frist liegen. Schlechter: Solange Reiryu
stehen, warten Mitglieder der Klassen 2 und 3 (ein beschossener Tank über der Notfallschwelle) bis zu 38 s auf die
Heilungen dieses Heilers; Klasse 1 kommt weiter zuerst, und der Mitheiler heilt weiter. Bedrohung: Eine freundliche
Figur in einem anderen Inhalt, die mit 1 LP erscheint und nicht geheilt werden soll, bekäme Heilungen; nimmt sie sie
nicht an, korrigiert die 0-LP-Regel das. Eine, deren LP das Spiel festhält, ohne 0 zu melden, bekäme sie weiter. Das
ist nicht belegt, der Preis wären Heilungen im Leeren. Überwiegt: das Bessere. Ein Scheitern trifft alle acht, und die
Angeleinten sterben (sein Hinweis).

**Ausgeschlossen:**
- *Jede freundliche Figur unter voller Gesundheit:* Das ist der Bereich des Schalters für Solo-NPCs (Upstream:
  experimentell, ab Werk aus), mit FATE-NPCs und Verbündeten in Solo-Inhalten.
- *Nur die Leine als Zeichen:* Sie kommt 8 s zu spät, und genau die frühe Heilung verhindert den Fluch.
- *Nur 1 LP als Zeichen:* Eine Figur, die RSR erst nach der ersten Heilung sieht (RSR eingeschaltet mitten in der
  Welle), bliebe unerkannt; die Leine fängt sie.
- *Die Figuren in `PartyMembers`* (der Schalter für Solo-NPCs): Vier Figuren bei 0 % verfälschten jeden Gruppenwert.
- *Die Gruppenmittel ohne die Verfluchten:* siehe oben.

**Falsifikation.** Kein Defekt: widerlegt. RSR heilte die Reiryu nicht, keine Liste führte sie. Option falsch: Die
Rangfolge vor Klasse 2 lässt einen beschossenen Tank warten. Der Tank fällt aber in Klasse 1, sobald er gleich stirbt,
und die Welle dauert höchstens 38 s; dagegen steht ein sicheres Scheitern ohne die Heilung. Ausgeliefert, und nichts
ändert sich, warum?
- *Die Reiryu sind kein Ziel:* Sie sind anvisierbar, die Heiler seiner Gruppe zielten auf sie.
- *Das Spiel weist die Heilung ab:* Cure II, Tetragrammaton, Afflatus Solace, Druochole und Diagnosis gingen auf sie,
  dieselbe Prüfung, die der Client für die Taste nutzt (Schluss).
- *Eine Job-Schranke auf den Anteil des Ziels hält die letzte Heilung zurück:* Essential Dignity fragt den Anteil, die
  GCD-Heilungen dahinter nicht.
- *Die Erkennung sieht 1 LP nicht,* weil eine Heilung im selben Frame landet: Dann trägt die Leine.

*Prüfgrad:* Mechanik an Spielblättern, am Quelltext von RSR, BossModReborn und ECommons, am Wiki und an seinem
Mitschnitt vom 03.11.2026 (zwei Läufe: LP, Zeiten, Heilungen, Leinen, Flüche, 0-LP-Heilungen auf Verfluchte). Die Regel
ist kompiliert und geprüft, im Spiel noch nicht beobachtet; die Zeilen oben zeigen es im nächsten Protokoll.

## Heilung, die nur geschwächt ankommt

**Geltender Stand** (A544, `HealReduction`): Eine Flächenheilung zählt bei einem Mitglied, das einen Status trägt, der
die LP-Erholung senkt, nur das Defizit, das Heilung bei ihm noch schließen kann: Fehl-LP mal dem gemessenen Anteil, der
bei ihm ankommt. Das gilt für die Gruppenmittel der Flächenschwellen (`ComputePartyHpStats`,
`ComputeForecastAreaStats`, damit auch `PartyMembersAverHP` und die übrigen Gruppenwerte für Heilungen), für die
Zahl der Mitglieder unter der Einzelschwelle, ab der eine Flächenheilung kommt (mehr als zwei), und für den Bedarf im
Radius einer Flächenheilung um den Wirkenden. Eine gezielte Flächenheilung wie Vitaga folgt der Flächenflagge. Einzelheilung,
Notfallheilung und Klasse 1 bleiben, wie sie sind: Ein Mitglied, bei dem weniger ankommt, braucht mehr Heilung, nicht
später (`StatusHelper`, Abgrenzung zu `HealingIneffectiveStatus`).
- *Welche Status:* Jeder, dessen Wirktext im Statusblatt sagt, dass die LP-Erholung sinkt oder aufgehoben ist („HP
  recovery … reduced", „reducing HP recovery", „nullified"), beim ersten Gebrauch aus dem Blatt gelesen – aus dem
  englischen, gleich welche Sprache der Client spricht. Ohne Angabe liefert Dalamud die Sprache des Clients; mit seinem
  deutschen Client fand die erste Fassung keinen Status, und die Flächenheilung zählte jeden Träger ganz (A547;
  `check_sheet_language.py` hält das in der CI). Die Blätter
  nennen über 30 solcher Status, die meisten ohne Stärke (Heilung -, Infirmity, Healing Magic Down, Biolysis …); die
  Stärke misst RSR selbst.
- *Messung* (Effekt-Handler, je eigener Heilung auf mehrere): Was beim Träger ankommt, geteilt durch die kleinste Menge,
  die dieselbe Heilung im selben Wurf einem Mitglied ohne solchen Status brachte; je Status gemittelt über die Würfe der
  Sitzung, höchstens 1. Die kleinste, nicht der Median: Ein kritischer Treffer bei einem Mitglied ohne Status ließe den
  Durchlass kleiner erscheinen, als er ist; mit der kleinsten erscheint er eher größer – die Seite, auf der die
  Flächenheilung wie bisher geht. Ungemessen gilt 1 – wie bisher. Die erste Flächenheilung auf einen Träger misst also, die
  folgenden lesen.
- *Spur:* eine Zeile je Status beim ersten Messwert und wenn er sich deutlich ändert: „heal reduction: <Status> … lets
  <Anteil> of own heals through".

**Befund** (Windurst, Die dritte Etappe, 03.11.2026): Himmlischer Pfad setzte drei Mitglieder auf 10 % und legte 20 s
„Heilung -" (2852). RSR wirkte zweimal Afflatus laetitiae (16534), Resega (37010), Vitaga (131) und Assise (3571); bei
den Getroffenen kamen je 1 bis 2 % an, gemessen rund 98 % weniger. Die Einzelheilung schloss sie schon aus (`TryHealSingleThreshold`, Ausnahme für 2852
in Windurst); die Flächenheilung zählte sie mit, weil das Mittel der vier Schwächsten bei drei Mitgliedern auf 10 % unter
jeder Schwelle liegt.

**Bilanz im Kampf.** Besser: Nach der ersten Flächenheilung zählen die Träger mit ihrem schließbaren Defizit – bei 2 %
Durchlass fast keins –, und die vier weiteren GCDs gehen in Schaden; MP bleibt. Bei einer schwachen Senkung (Durchlass
80 %) zählt ein Träger auf 30 % wie einer auf 44 %: Die Flächenheilung kommt etwas seltener für ihn allein, seine
Einzelheilung unverändert. Schlechter: Die erste Flächenheilung auf Träger geht wie bisher (sie misst). Ein Status, der
nur Heilmagie senkt, bei einer Fähigkeit gemessen, wird als so schwach gelesen wie bei dieser – beim nächsten Zauber
korrigiert die Messung.

**Verworfen:** die Windurst-Ausnahme auf Flächenheilungen ausdehnen (eine Inhaltsregel für eine Lage, die überall
vorkommt); die Stärke aus dem Wirktext lesen (die meisten nennen keine); Träger aus den Mitteln ganz streichen (ein
Durchlass von 80 % ist Heilung, die er braucht).

**Falsifikation:** Kein Defekt – widerlegt durch die fünf GCDs in Windurst. Option falsch – die stärkste Gegenposition
ist das Kritrauschen einer Flächenheilung (ein kritischer Treffer je Ziel); der Bezug auf die kleinste Menge ohne Status
lässt es nur nach oben wirken, und bei 98 % Senkung ist es bedeutungslos. Ausgeliefert, und nichts
ändert sich – wenn die Spieldaten eine überheilte Menge als ganze Heilung melden, misst ein volles Mitglied ohne Status
gleich viel wie sonst, der Träger aber weniger; liefern sie nur die gewirkte Menge, sinkt der Nenner bei vollen Mitgliedern
und der Durchlass wird überschätzt – das geht in die sichere Richtung (es heilt wie bisher).

## Einen Heilzauber abbrechen, den andere überflüssig gemacht haben

**Geltender Stand** („Stop single-target GCD heals once the target is above the threshold (Experimental)", ab Werk
aus; `CancelCastUpdater.ShouldStopHealing`, A332, A545). Der Text der Option bindet: „If another healer's healing puts
the target above the healing threshold while you're casting, the cast is interrupted so you don't waste MP." Ein
Einzelheilzauber (`GCDSingleHeal`) bricht im Kampf ab, wenn alle vier Bedingungen gelten:
- Das Ziel brauchte die Heilung, als der Zauber begann – dieselbe Entscheidung wie die Heilflagge
  (`StateUpdater.NeedsSingleHealSpell`).
- Seitdem ist Heilung eines anderen auf ihm gelandet (`HealFromOthers`). Das ist zum einen eine Aktion, deren
  LP-Anstieg während des Wirkens ankam; als Quelle zählt jeder außer dem Spieler und seinen Objekten, also ein anderer
  Heiler, ein NPC oder das Ziel selbst (Sturmkeil, Blutbad). Ihre Wirkung darf schon vor dem Beginn gemeldet sein, es
  zählt die Landung. Zum anderen zählt ein Tick über Zeit, während das Ziel eine Heilung über Zeit eines anderen trägt.
  Nicht zählen die eigene Heilung, die des eigenen Begleiters (Eos, Selene, Mondstein-Karfunkel; Blatt `Pet`, Zeilen 6,
  7, 16) und die natürliche Regeneration.
- Seine Gesundheit liegt über dem Stand bei Beginn.
- Es braucht die Heilung jetzt nicht mehr.

Drei Ausnahmen gelten. Die Füllheilung bricht nicht ab (`HealingWhileDoingNothing`, „Die Füllheilung"). Ein Befehl des
Spielers für genau diesen Zauber bricht nicht ab. Und ein Zauber, dessen eigene Wirkung schon eingetroffen ist, wird
nicht mehr beurteilt: Der Server hat ihn aufgelöst, ein Abbruch ändert nichts mehr. Jeder Abbruch schreibt eine Zeile
ins `DefenseTrace.log` mit Ziel, Anstieg und der abgelaufenen Wirkzeit, also dem Preis des Abbruchs. Hebt nur eigene
Heilung oder die Regeneration das Ziel über die Schwelle, schreibt der Zauber einmal „heal kept". Diese Zeile misst, wie
oft der Fall eintritt, den die frühere Fassung abbrach.

**Mechanik** (gemessen an seinem Mitschnitt vom 04.11.2026, Toto-Rak, 46 Vita):
- Der Server löst einen Zauber vor dem Ende des Wirkbalkens auf. Die Wirkung der Vita kam 0,98 s nach Beginn eines
  1,49-s-Wirkens. Ein Abbruch danach bleibt ohne Folge: Es gab keine Abbruchzeile, und die Heilung landete voll.
- Den LP-Anstieg zeigt der Client erst mit dem Ergebnis der Wirkung: bei der Vita 1,0 s nach ihr, beim Sturmkeil 1,5 s.
  Bis dahin rechnet RSR die eigene Heilung im Flug auf (`RefinedHP`); das Ziel braucht in dieser Zeit nichts mehr.
- Im Kampf kommt je Servertick (3 s) 1 % der maximalen LP hinzu (abgerundet), ohne Wirkung und ohne Tick einer Heilung.
  Das zeigt Zeile 39 des Mitschnitts, zehnmal von zehn: 912 → +9, 1013 → +10, 804 → +8. Das ist die natürliche
  Regeneration. Eine Spielquelle dazu ist nicht gesucht; der Satz steht auf der Messung.
- Ein Abbruch gibt den GCD frei. Nach dem Abbruch um 09:22:30 begann 0,18 s später Steinra. Verloren ist die
  abgelaufene Wirkzeit, nicht der GCD.
- Heilungen über Zeit meldet das Spiel je Ziel als einen gesammelten Tick ohne Quelle (cactbot LogGuide, Zeile 24;
  Werkzeugquelle). Ein Tick zählt deshalb als fremd, wenn das Ziel eine Heilung über Zeit eines anderen trägt. Trägt
  es dazu eine eigene, zählt er trotzdem ganz.
- Die Selbstheilung einer Angriffsaktion steht im Wirkungspaket beim getroffenen Gegner, gekennzeichnet als „an der
  Quelle": Bit 0x80 im Byte, das ECommons `flags` nennt. Belege: BossModReborn, `ActionEffect.AtSource`, Commit
  27be42256 vom 05.11.2026 (Werkzeugquelle), und im Mitschnitt der Sturmkeil, der den Tank um 104 LP heilte. Der
  Effekt-Handler ordnet solche Heilung der Quelle zu (`Watcher.AmountsByTarget`), für fremde wie für eigene Aktionen.
  Unter einem eigenen Blutbad rechnet die Vorhersage die eigene Heilung im Flug seitdem beim Spieler auf, nicht mehr
  beim Gegner.

**Befund** (Toto-Rak, Weißmagier, Build ad1013fb8): Bei 46 Vita standen elf Zeilen „heal stopped" im Protokoll; einen
Abbruch führte das Spiel aber nur einmal aus.
- *Zehn Fälle:* Die Vita war schon aufgelöst, ihre Wirkung 0,8 s vor der Zeile eingetroffen. Der Tick der natürlichen
  Regeneration hob die Gesundheit über den Stand bei Beginn, und die eigene, schon gemeldete Heilung nahm den Bedarf. Die
  Zeile behauptete einen Abbruch, den es nicht gab; jede dieser Vita landete voll.
- *Der eine Abbruch (09:22:30):* Der Sturmkeil des Tanks hob ihn 0,13 s nach Beginn von 66 auf 77 %. 0,18 s danach begann
  Steinra, und 3 s später endete der Kampf.

Die Annahme im TODO und in A521 ist damit widerlegt (C160): Dort hieß es, ein Regen-Tick habe achtmal eine Heilung
abgebrochen und die Wirkzeit gekostet. Verloren ging keine Heilung.

**Bilanz im Kampf.**
- *Besser:* Die Regel bricht nur ab, wenn Heilung eines anderen das Ziel über die Schwelle gehoben hat. Hebt die
  Regeneration oder ein eigener Regen-Tick ein Ziel knapp unter der Schwelle darüber, läuft der Zauber weiter und heilt
  noch voll. Nach der früheren Fassung brach er ab und begann erst eine Wirkzeit später neu. Die Spur nennt nur noch
  Abbrüche, die das Spiel ausführt, mit ihrem Preis.
- *Schlechter:* Hebt Eos oder Selene das Ziel über die Schwelle, läuft der Zauber weiter, mit Überheilung und MP
  wie ohne die Option. Ein gesammelter Tick aus eigenem und fremdem HoT zählt ganz als fremd.
- *Was die Option selbst kostet, bleibt:* Ein abgebrochener Zauber fehlt dem Ziel als Polster. Kommt ein Treffer ohne
  Vorwarnung, steht es um die Heilmenge tiefer, als wenn der Zauber gelandet wäre. Deshalb bleibt sie ab Werk aus. Das
  ist meine Ableitung aus „Sicherheit vor Schaden" (CLAUDE.md, „Spielweise"); er hat sie an, wie die Zeilen in seinem
  Protokoll zeigen.

**Verworfen.**
- *Abbrechen erst, wenn die Heilung zum größten Teil überheilte* (der Vorschlag im TODO): Das widerspricht dem Text
  („once the target is above the threshold"), und der Text bindet. Die Sorge dahinter, ein Tick knapp über die Schwelle,
  trägt die Messung nicht: Keiner der elf Fälle war ein solcher Abbruch.
- *Einen Anstieg über die Regeneration hinaus verlangen* (mehr als 1 %): eine feste Zahl, und ein eigener Regen-Tick
  zählte weiter.
- *Nur die eigene Heilung im Flug ausschließen:* Dann hebt die Regeneration weiter ein Ziel knapp unter der Schwelle
  darüber. Am 04.11. ist das nicht beobachtet, möglich ist es bei jedem Zauber, in dessen Wirkzeit ein Servertick fällt.
  Der Text nennt zudem ausdrücklich Heilung eines anderen.

**Falsifikation.**
- *Kein Defekt:* widerlegt. Zehn von elf Zeilen behaupten einen Abbruch, den das Spiel nicht ausführte, und die Regel
  zählte, was ihr Text ausschließt.
- *Option falsch:* Die stärkste Gegenposition ist, dass die Zuordnung fremde Heilung verfehlt und die Option dann nichts
  mehr abbricht. Die Wirkung einer Aktion kommt vor ihrem Anstieg (im Mitschnitt 1,0 und 1,5 s), die Landung wird also
  gesehen. Landet fremde Heilung vor dem Beginn, ist sie im Stand bei Beginn enthalten. Die Selbstheilung trägt ihre
  Kennung im Mitschnitt (Sturmkeil). Einen Tick über Zeit sieht RSR schon heute (`DotOnParty`).
- *Ausgeliefert, und nichts ändert sich:* An den elf Fällen ändert sich im Kampf fast nichts, denn zehn waren ohne
  Wirkung. Was sich ändert, ist die Spur, und es bleibt der seltene Fall der Regeneration über die Schwelle. Das ist der
  Umfang dieser Behebung. Wie oft dieser Fall eintritt, zählt „heal kept".

## Reinigung oder Heilung

**Leitfrage:** Wann geht eine Reinigung vor einer Heilung?

**Wen es betrifft:** die Heiler (Weißmagier, Gelehrter, Astrologe, Weiser, Druide): Ihre Reinigung Esuna ist ein GCD
und kostet 400 MP, sie steht also gegen eine Heilung und gegen deren Mana. Der Barde reinigt mit Warden's Paean, einer
Fähigkeit ohne Mana, die keinem GCD den Platz nimmt; für ihn bleibt alles wie bisher – die Gründe unten treffen ihn nicht.

**Was gilt (A565):** Eine Reinigung geht nur dann vor jeder Heilung, wenn sie selbst ein Leben rettet. Alles andere
reinigt RSR erst, wenn weder Heilung noch Abwehr ansteht, und nicht mit dem Mana, das die Wiederbelebung braucht. Drei
Arten reinigbarer Zustände, jede an den eigenen Daten des Spiels erkannt:

| Art | Erkannt an | Beispiele (Standardliste) | Wann RSR reinigt |
|---|---|---|---|
| **tötet am Ende** | Symbol von Verhängnis (215503, sechs Doom-Zeilen) oder Wirktext mit „KO" (Necrosis 2965, Throttle 700; xivapi, 05.11.2026) | Doom (910, 3364, 4594), Necrosis (2965) | sofort, vor jeder Heilung – sie rettet ein Leben |
| **Schaden über Zeit** | Wirktext „damage over time" (derselbe Erkenner wie „HoT gegen DoT", Konzept 08) | Bleeding (273), Poison (2104), Concussion (3521) | liegt er, solange ein Gegner in Reichweite ist, auf mehr als einem Mitglied, nie – dann antwortet die Heilung über Zeit für die Gruppe (Weißmagier, Astrologe: „HoT gegen DoT", Konzept 08; Gelehrter, Weiser: ihre Fähigkeiten über die Heilschwellen); auf einem einzigen wie die übrigen |
| **übrige** | alles andere auf der Liste „Dispellable debuffs" | Stun, Sleep, Silence, Paralysis, Bind, Slow (561 u. a.), Blind | nur ohne Heil- und Abwehranlass und nur mit Mana über der Rücklage der Wiederbelebung |

*Was keine Reinigung nimmt, nimmt volle Heilung (A573):* Sagt der englische Wirktext „Effect dissipates once fully
healed", endet der Zustand erst bei vollen LP und ist nicht reinigbar (`CanDispel` nein). RSR heilt seinen Träger, als
stünde er bei 1 %, bis er voll ist (`DoomNeedHealing`: Gesundheitsanteil 0,01, Heilflaggen) – bisher nur für Upstreams
Liste Doom 1769 und 5473, jetzt für jeden Zustand mit diesem Text: auch Gradual Petrification 1628 (wird zu Stein) und
Accretion 1604 (senkt jede Heilung; xivapi v2, 05.11.2026). Daedalus 74659a6 führt dieselbe Regel als Liste („Doomed
(it clears only at 100%)"; ein Spieler starb zweimal bei 83 bis 85 % an seinem Verhängnis, während der Heiler Schaden
wirkte). Seine Liste nennt dazu Doom 910 und 2976: 910 ist reinigbar (erste Art oben), für 2976 sagt das Blatt nichts
dergleichen – unbelegt, darum ohne Regel.

Zustände außerhalb der Liste reinigt der Heiler nach denen auf der Liste und ebenso nur, wenn nichts ansteht; bisher
auch bei Heilbedarf, sobald kein Gegner in Reichweite stand. „Dispel every dispellable debuff" nimmt sie wie bisher alle
dazu, vor die Heilung stellt es keine. Die Rücklage ist die der Einstellung „Keep the MP a raise needs" (ab Werk an,
A469); ihr Text nennt jetzt auch die Reinigung. Die Dringlichkeit aus A500 – ein Zustand, der innerhalb eines GCD
abläuft, geht ohne Verzögerung – gilt für jeden Reiniger, auch den Barden, nur noch für die erste Art: Was nicht tötet,
endet nur. „Gruppen-HoT des Jobs" heißt auf seiner Stufe gelernt: Medica II oder III (Weißmagier), Aspected Helios oder
Helios Conjunction (Astrologe), Whispering Dawn mit der Fee (Gelehrter), Physis oder Physis II (Weiser).

**Belegt am Odin-Kampf (sein Upload vom 05.11.2026, Weißmagier, stufensynchron 50, acht Mitglieder):**
Odin legt mit Einherjar (39 bis 41 % auf alle acht) Bleeding für 30 s auf jeden, mit anderen Angriffen Slow für 10 s.
Beide stehen auf Upstreams Standardliste gefährlicher Zustände (Slow seit 30.04.2023, Bleeding seit 07.01.2025; seit
24.10.2026 als eingebaute Liste), und alles auf der Liste ging vor jede Heilung (Slot 6 vor 10 und 11, Konzept 03). RSR
warf 22-mal Esuna – 13-mal auf Bleeding, 9-mal auf Slow –, 400 MP je Wurf, zusammen 8.800 MP. Nach dem ersten
Einherjar gingen drei Reinigungen hinaus, während ein Mitglied bei 0 % Vorausschau stand; ein anderes fiel 7 s nach
Einherjar an Sanngetall bei 36 % – ob eine Heilung statt der Reinigung es gehalten hätte, zeigt der Mitschnitt nicht. Ab
39:00 lag das Mana unter 2.600, um 39:42 bei 268: zu wenig für Heilungen und, unter „Keep the MP a raise needs" (ab
Werk an; deine Stellung kenne ich nicht), für jeden Schadenszauber – darum stand der Heiler in den ruhigen Strecken still (40:26 bis 40:48 kein GCD).
Mit dieser Regel: keine der 13 Blutungsreinigungen (acht Mitglieder, der Gruppen-HoT antwortet), die Slow-Reinigungen
nur, solange keine Heilung ansteht und das Mana über 2.400 plus 400 liegt; mindestens 5.200 MP bleiben für
Heilungen.

**Begründung.** Seine Vorgabe oben: Oberste Priorität hat das Überleben der Gruppe, und das Mittel richtet sich nach der
Gefahr. Eine Reinigung, die kein Leben rettet, ist kein Mittel gegen die Gefahr, die gerade tötet. Sein Hinweis vom
05.11.2026: „Blutung ist eher etwas, was man mit einem HoT entgegenwirkt" – auf acht Mitgliedern kostet die Reinigung
acht GCD und 3.200 MP, ein Gruppen-HoT einen. Die Rücklage folgt A469 („die Lebenden gehen vor"): Eine Reinigung, die
kein Leben rettet, ist dort wie Schaden zu behandeln, nicht wie eine Heilung.

**Verworfen.** (a) Bleeding und Slow aus der eingebauten Liste nehmen: Die Liste bleibt seine (über „Removed" in den
Einstellungen); falsch war nicht, dass sie reinigenswert sind, sondern dass alles darauf vor jede Heilung ging. (b) Jeden
Schaden über Zeit nie reinigen: Auf einem einzigen Mitglied nimmt eine Reinigung den ganzen restlichen Schaden für
denselben GCD und dasselbe Mana wie ein HoT, der nur einen Teil heilt. (c) Die Restzeit- und Stapelregel von Upstream
(über 20 s, mehr als zwei Stapel) als Maß der Gefahr: Sie sagt nichts darüber, ob der Zustand tötet; sie bleibt nur als
Merkmal „reinigenswert" für Zustände außerhalb der Liste.

**Gegenthesen.**
- *Ein betäubter Tank wartet einen GCD auf seine Reinigung, wenn zugleich jemand Heilung braucht.* Nicht widerlegt,
  Preis benannt: Ein Mitglied unter der Schwelle stirbt am nächsten Treffer sicher, der betäubte Tank nur, wenn in
  diesem GCD ein Tankbuster landet – dann steht ohnehin die Abwehr an, und für die gilt dasselbe. Die Heilung geht vor.
- *Ein Schaden über Zeit auf zwei Mitgliedern, und der Job hat (auf dieser Stufe) keinen Gruppen-HoT:* Zwei
  Reinigungen wären billiger als das Durchheilen. Aufgenommen: Ohne Gruppen-HoT gilt der DoT wie die übrigen und wird
  gereinigt, sobald nichts ansteht. Ebenso ohne Gegner in Reichweite, wo kein GCD gegen Schaden steht.
- *Unter Mana-Not fällt die Reinigung einer Betäubung oder eines Schlafs bei einem anderen Mitglied weg.* Nicht
  widerlegt, Preis benannt: Die Rücklage hält das Mana, mit dem RSR dieses Mitglied wiederbelebt, falls es fällt; eine
  Betäubung endet von selbst, ein Schlaf mit dem nächsten Treffer. Wer sie dennoch gereinigt haben will, schaltet „Keep
  the MP a raise needs" aus.

## Abgrenzung zur Schildanrechnung

Die entfernte Schildanrechnung (`AUDIT_LOG.md` A85) hat dieselbe Größe an der falschen Stelle
verwendet: Sie rechnete die Barriere auf die **Heilschwelle** und verzögerte damit die Heilung
überhaupt. Gesundheit und Barriere addieren sich aber — ein vollgeheilter Tank mit Barriere ist
besser geschützt als ein geschildeter mit wenig Gesundheit. Für die **Reihenfolge zwischen Zielen**
ist die Barriere dagegen eine zulässige Größe: Dort wird nicht gefragt, ob geheilt wird, sondern wer
zuerst. Der einführende Commit der Anrechnung (`27c7b6942`) nannte im Titel genau diese Frage und
änderte dann die Schwelle.

## Abgrenzung zur Wiederbelebung

Dieses Konzept ordnet die **Lebenden**. Wer bereits tot ist, fällt nicht unter die Gefährdung,
sondern unter die Wiederbelebung — deren Auslösung, Reihenfolge und der Spontanitäts-Vorbehalt
stehen in `11-raise-dispatch.md`. Die Berührung der beiden ist real und in Abschnitt 1 belegt: Der
frisch Wiederbelebte wechselt in dem Moment aus dem einen Konzept in das andere, und genau dort
hat die Notfall-Vollheilung ihn früher als dringendsten Fall gelesen.

## Abgrenzung zur Wahl des Mittels

Dieses Konzept beantwortet **wer** zuerst geheilt wird. Ob überhaupt geheilt wird oder stattdessen
gedeckt — Barriere, Minderung —, entscheidet die Ordnung in `08-mitigation-synergy.md` („Die Antwort
auf einen eingehenden Treffer"): Heilung geht vor, sobald der angekündigte Treffer die **aktuelle**
Gesundheit des schwächsten Mitglieds erreicht, und zwar bis zur Maximalgesundheit; darunter genügt
Deckung in Höhe des Treffers. Beide Konzepte greifen also nacheinander und nicht ineinander — erst
die Frage nach dem Mittel, dann die nach dem Ziel.

## Grenzen des Nachweises

Am Quelltext belegt (02.10.2026, für Klasse 2 am 14.10.2026): die Rangfolge samt Vorgabewerten, die
Klassenordnung, die Gleichheit der drei Werte mit `upstream/main`; die Umkehr im Band 40–45 % unter Beschuss
ist seit A283 aufgehoben. Der Prüfgrad ist statische
Selbstprüfung am Quelltext.

Nicht belegbar: wie oft das Band 40–45 % mit einem gleich tief stehenden, beschossenen Heiler überhaupt
erreicht wird. Ebenfalls nicht messbar:
die tatsächliche Konfiguration des Auftraggebers.

## Offene Punkte

Sie stehen in `TODO.md` und sind dort unter der Überschrift des Eintrags mit **Konzept:** auf dieses
Dokument gekennzeichnet — an **einer** Stelle statt in zweien, damit keine Kopie altert.
`.github/scripts/audit/check_concept_links.py` listet sie je Konzept und nennt zugleich, wie viele
Einträge überhaupt keinem Konzept zugeordnet sind.

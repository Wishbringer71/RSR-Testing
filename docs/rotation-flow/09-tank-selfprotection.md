# 09 · Tank-Selbstschutz und das Verhalten des Heilers

Entwurfsdokument nach ADR-Struktur, mit Umsetzungskonzept und dessen Prüfung.
Beschreibt einen Mechanismus, kein umgesetztes Verhalten.

## Kontext

Ausgangspunkt war die Beobachtung, dass Heilung eines Dunkelritters unter Living
Dead den Tod verhindern kann, den dieser gerade herbeiführen will. Die Prüfung
zeigte, dass das kein Einzelfall ist: Mehrere Tank-Fähigkeiten haben einen
**Auslöser**, den fremde Heilung oder ein fremder Schild verhindern kann.

RSR kennt heute nur eine einzige dieser Wechselwirkungen, und die ist fehlerhaft
umgesetzt (`TODO.md`, zwei Defekte). Die übrigen sind ihm unbekannt.

## Prüfmaßstab — die Rangordnung

Alles Folgende ist an dieser Ordnung zu messen. Sie ist **lexikographisch**: Eine
nachrangige Stufe darf eine vorrangige nie aufwiegen, gleich wie groß ihr Betrag
wäre.

1. **Das Überleben des Tanks.**
2. **Die Mitnahme positiver Effekte** — Dark Arts, Catharsis, die Selbstheilung von
   Walking Dead.
3. **Die Vermeidung unnötiger Aktionen** — verbrauchte oGCDs, MP, Cooldowns.

**Befund aus der Anwendung dieses Maßstabs auf die erste Fassung dieses Dokuments:
Alle drei Rückhaltefälle verletzten die Ordnung.** Sie hielten Heilung oder Schild
zurück, um einen Effekt der Stufe 2 zu sichern, ohne vorher Stufe 1 zu prüfen. Am
schwersten bei Living Dead: Der Verzicht auf Heilung führt planmäßig in Walking
Dead, wo das Überleben an einer kumulativen Heilung in Höhe der **vollen maximalen
HP** hängt. Kann der Heiler die nicht aufbringen — weil MP fehlen, Cooldowns liegen
oder andere verletzt sind —, tauscht der Verzicht ein sicheres Überleben gegen ein
unsicheres. Das ist genau die verbotene Aufwiegung.

Die Beseitigung ist keine Randausnahme, sondern eine **Vorbedingung**: Jede
Rückhaltung wird erst geprüft, wenn Stufe 1 gesichert ist.

**Eine Lesart bleibt offen und ist hier festzuhalten.** „Überleben des Tanks" ist
oben als Vorrang *innerhalb* der Frage verstanden, ob eine Tank-Schutzmechanik
respektiert wird — nicht als genereller Vorrang des Tanks vor der übrigen Gruppe.
RSR führt für diesen anderen Fall bereits eine eigene Rangfolge (Selbst → Heiler →
Tank → niedrigste Gesundheit, `ActionTargetInfo.cs:3566-3583`). Sollte die Ordnung
auch dort gelten, wäre das eine zweite, größere Änderung; sie ist hier nicht
unterstellt.

## Research — Taxonomie nach Auslöser

Entscheidend ist nicht, ob eine Fähigkeit schützt, sondern **ob sie auf ein
Ereignis wartet**, das der Heiler abfangen kann.

### Klasse A — auslöserbehaftet: Heilung oder Schild kann den Effekt verhindern

| Job | Fähigkeit | Auslöser | Auslöser ist… | Was fremde Heilung bewirkt | Was ein fremder Schild bewirkt |
|---|---|---|---|---|---|
| DRK | Living Dead | HP fallen auf 0 | **der Tod selbst** | verhindert den Tod, also den Übergang in Walking Dead samt dessen Selbstheilung | dasselbe, über abgefangenen Schaden |
| DRK | The Blackest Night | Barriere wird **vollständig** absorbiert | ein Schadensereignis unterhalb des Todes | indirekt: hebt die HP, sodass weniger Schaden gegen die Barriere läuft | unbelegt, siehe Klasse A+ |
| SCH | Excogitation | HP-Schwelle auf dem Ziel | ein Schadensereignis unterhalb des Todes | vorzeitige Heilung entwertet die eigene, bereits gesetzte Fähigkeit | dasselbe |

**Die vierte Spalte trennt die Klasse in zwei, und diese Trennung entscheidet über
alles Weitere.** Sie fehlte in allen bisherigen Fassungen und hat dort zu einer
falschen Regel geführt.

**A-tödlich — der Auslöser *ist* der Tod.** Living Dead ist der einzige Fall. Hier
ist das Ereignis, das der Heiler sonst um jeden Preis verhindert, genau das
erwünschte. Jede Regel, die sonst gilt — „bei niedriger Gesundheit heilen", „bei
drohendem Tankbuster heilen" —, ist hier **nicht** bloß unnötig, sondern
kontraproduktiv: Sie zielt auf die Verhinderung dessen, worauf die Fähigkeit wartet.

**A-nichttödlich — der Auslöser liegt unterhalb des Todes.** The Blackest Night und
Excogitation. Hier ist der Tod weiterhin die Katastrophe und nicht das Ziel; die
üblichen Aufhebungsregeln gelten unverändert.

**Gunbreaker gehört nicht mehr hierher.** Die erste Fassung führte Catharsis of
Corundum als auslöserbehaftet. Das ist am Artefakt widerlegt:
`Status.resx` beschreibt Status 2685 mit „HP will be restored automatically upon
falling below a certain level **or expiration of effect duration**". Der Heilstoß
kommt in jedem Fall — spätestens beim Ablauf. Der Heiler kann ihn nicht rauben, es
gibt nichts zurückzuhalten. Nebenbefund derselben Prüfung: Der auslösertragende
Bezeichner heißt `CatharsisOfCorundum` (2685, in Dawntrail zusätzlich 4296), nicht
`ClarityOfCorundum` (2684) — letzterer ist reine Schadensreduktion und gehört nach
Klasse C. Die frühere Fassung nannte den falschen.

### Klasse A+ — der Auslöser ist nicht nur eine Belohnung, sondern eine Rückzahlung

The Blackest Night ist der schwerste Fall und verdient eine eigene Einordnung. Die
Fähigkeit kostet **3000 MP** und legt eine Barriere über 25 % der maximalen HP für
7 Sekunden. Dark Arts — und damit ein kostenloser Edge of Shadow oder Flood of
Shadow — wird nur gewährt, wenn die Barriere **vollständig** absorbiert wird.

Wird sie es nicht, sind die 3000 MP ersatzlos ausgegeben. Das ist kein entgangener
Bonus, sondern ein Verlust mit Folgen: Edge of Shadow kostet seinerseits 3000 MP
und verlängert Darkside um 30 Sekunden. Ein verpuffter Blackest Night nimmt dem
Dunkelritter also einen Schadensskill **und** einen Beitrag zur Darkside-Laufzeit,
deren Abriss zehn Prozent Schaden kostet.

Die erste Fassung schloss daraus, ein Heilerschild verschlimmere das doppelt, weil
er den Gesamtpuffer erhöhe. **Dieser Schluss wird zurückgenommen.** Er stand auf
zwei Annahmen, die beide nicht belegt sind:

*Erstens die Aufteilung.* Die erste Fassung behauptete, mehr Gesamtpuffer bedeute
„in jedem Aufteilungsmodell" geringere Verbrauchswahrscheinlichkeit. Das gilt gerade
nicht in jedem Modell. Für Heilerschilde untereinander ist überliefert, dass sie sich
nicht addieren, sondern der stärkere den schwächeren verdrängt — Divine Benison als
dokumentierte Ausnahme. Trifft dieselbe Regel auf The Blackest Night zu, dann
*ersetzt* ein größerer Heilerschild die Barriere, statt sie zu schonen; sie wäre
sofort fort statt langsamer verbraucht. Beide Wirkungen sind dem Dunkelritter
abträglich, aber sie verlangen entgegengesetzte Gegenmaßnahmen. Quellenstatus:
Spielerforum, keine offizielle Dokumentation — als unbelegt zu führen.

*Zweitens die Messbarkeit.* Der Client führt je Charakter genau einen
Schildwert: `ICharacter.ShieldPercentage`, den `ObjectHelper.GetObjectShield`
(`ObjectHelper.cs:3372`) in absolute Punkte umrechnet. Es gibt keine Buchführung je
Quelle. Ein Plugin kann daher weder sehen, wie viel des Puffers von The Blackest
Night stammt, noch ob die Barriere vollständig aufgezehrt wurde. Der Auslöser ist
**nicht beobachtbar** — weder zur Laufzeit noch im Nachhinein.

Damit steht Fall 5 anders da als gedacht: Nicht als Abwägung zwischen belegtem
Nutzen und Kosten, sondern als Regel, deren Wirkung mit den verfügbaren Mitteln
nicht nachweisbar ist und deren Mechanik unbelegt bleibt. Nach der Projektregel zu
Verhaltensänderungen ohne Nachweismöglichkeit gehört sie hinter eine Option mit
Standard aus — und sie ist der schwächste, nicht der stärkste Teil des Vorhabens.

### Klasse B — Unverwundbarkeit: Heilung während der Phase wirkt nicht auf den Schutz, wohl aber auf das, was danach kommt

| Job | Fähigkeit | Wirkung auf die HP | Lage nach Ablauf |
|---|---|---|---|
| PLD | Hallowed Ground | unverändert | so niedrig wie beim Zünden |
| WAR | Holmgang | fallen nicht unter 1 | möglicherweise 1 HP |
| GNB | Superbolide | **sofort auf 1** | 1 HP, sofern nicht geheilt wurde |
| DRK | Undead Rebirth | fallen nicht unter 1 | Erfolgszustand, siehe unten |
| DRK | Living Dead / Walking Dead | Sonderfall, siehe Klasse A und unten | — |

**Living Dead hat drei Phasen, nicht zwei.** Die Aktionsbeschreibung im Repository
(`ActionId.resx`, Aktion 3638) ist die Primärquelle und benennt sie vollständig:

1. **Living Dead**, 10 s. Fällt die Gesundheit in dieser Zeit auf 0, tritt kein Tod
   ein, sondern der Wechsel nach Walking Dead. Die Selbstheilung — 1500 Potenz je
   getroffenem Waffenskill oder gewirktem Zauber — hängt bereits an *dieser* Phase,
   nicht erst an der zweiten.
2. **Walking Dead**, 10 s. Angriffe drücken die Gesundheit nicht unter 1. Wird bis
   zum Ablauf kumuliert Heilung in Höhe der **maximalen** Gesundheit aufgenommen,
   folgt Phase 3; wird sie es nicht, tritt der Tod ein. Die Quelle sagt „an amount of
   HP totaling your maximum HP is restored" ohne Einschränkung auf die Quelle —
   fremde Heilung zählt mit. Bei heutigen Tank-Gesundheitswerten trägt die
   Selbstheilung davon nur einen Bruchteil.
3. **Undead Rebirth**, Restlaufzeit von Walking Dead. Angriffe drücken die
   Gesundheit nicht unter 1. Hier ist Heilung tatsächlich entbehrlich — der
   Dunkelritter kann nicht sterben, und der Auslöser ist bereits eingelöst.

Diese dritte Phase fehlte in der ersten Fassung des Konzepts vollständig. Sie ist
der einzige Zustand der ganzen Taxonomie, in dem Zurückhaltung weder Stufe 1 noch
Stufe 2 berührt, sondern rein Stufe 3 bedient.

### Living Dead ist ein Zeitproblem, kein Zustandsproblem

Alle bisherigen Fassungen dieses Konzepts fragten: *Liegt der Status?* Das ist die
falsche Frage, und sie führt in beiden Phasen zu falschem Verhalten. Die richtige
Frage ist in beiden Fällen eine über **Rate und Restzeit**:

**Phase 1 — reicht der eingehende Schaden, um vor Ablauf auf 0 zu kommen?**
Living Dead nützt nur, wenn der Tod **innerhalb** der zehn Sekunden eintritt. Zeichnet
sich ab, dass der eingehende Schaden dafür nicht ausreicht — der Tod also erst nach
Ablauf käme —, dann war die Rückhaltung nicht nur nutzlos, sondern hat den Tank zehn
Sekunden lang ungeheilt gelassen und steht nun mit niedriger Gesundheit ohne Schutz
da. In diesem Fall ist **im letzten Augenblick** zu handeln: in der Ablaufsekunde
oder unmittelbar danach, und zwar ausreichend, nicht symbolisch.

Das kehrt die Logik um. Zurückgehalten wird nicht, *weil* der Status liegt, sondern
solange die Hochrechnung sagt, dass der Tod noch rechtzeitig kommt. Kippt die
Hochrechnung, kippt die Entscheidung — mitten in der Phase.

**Phase 2 — heilt er sich schnell genug selbst?** Walking Dead verlangt kumuliert
eine volle Maximalgesundheit an Heilung in zehn Sekunden. Die Selbstheilung liefert
1500 Potenz je Waffenskill oder Zauber; bei rund 2,4 s GCD sind das etwa vier
Auslösungen. Ob das reicht, hängt an der Maximalgesundheit des Dunkelritters und
daran, ob er überhaupt zum Angreifen kommt. Auch hier ist die Frage eine Rate:
*Liegt die bisher aufgenommene Heilung auf einem Kurs, der bis zum Ablauf die volle
Maximalgesundheit erreicht?*

Daraus folgt ein gestaffeltes Verhalten statt eines einzigen Schalters:

| Zeitpunkt in Phase 2 | Verhalten | Begründung |
|---|---|---|
| Anfang | **leicht unterstützen** — Regeneration, ein günstiges HoT, kein teurer Notfallzauber | Der Beitrag zählt voll gegen die geforderte Summe, kostet aber wenig. Nichts zu tun verschenkt die billigste Hilfe |
| Mitte | Kurs prüfen, weiter leicht unterstützen | Solange der Kurs trägt, ist ein großer Zauber vergeudet |
| Kurs reicht nicht | **eingreifen, in voller Höhe** | Die Alternative ist der Tod am Phasenende |
| Kurs trägt bis zum Ende | nichts weiter | Der Rest ist Überheilung |

Die frühere Fassung kannte nur „Phase 2: ja, zwingend". Das ist im Ergebnis nicht
falsch, aber es verschenkt die Unterscheidung zwischen billiger Frühunterstützung
und teurem Späteingriff — und es lässt offen, wann der teure Eingriff fällig ist.

### Was diese Ratenbetrachtung an Messmitteln voraussetzt — und was davon fehlt

Die Anforderung ist berechtigt; die Frage ist, ob sie mit den vorhandenen Mitteln zu
beantworten ist. Erhebung am Artefakt:

| Größe | Vorhanden? | Beleg |
|---|---|---|
| Restzeit des Status | **ja** | `StatusHelper.StatusTime(…)` liefert die verbleibende Sekundenzahl |
| Historie der Gesundheit über die Zeit | **nur für Gegner** | `DataCenter.RecordedHP` (`DataCenter.cs:197`) wird in `TargetUpdater.cs:513-535` ausschließlich aus `AllHostileTargets` gefüllt. Party-Mitglieder stehen nicht darin |
| Zeit bis zum Tod eines Ziels | **auf Party-Mitglieder nicht anwendbar** | `GetTTK` (`ObjectHelper.cs:3468`) liest genau diese Historie; für eine Party-Id findet es keinen Eintrag, `startTime` bleibt `DateTime.MinValue` und die Funktion liefert `NaN` |
| Abtastrate der Historie | **1 Hz** | `TimeToKillUpdateInterval = TimeSpan.FromSeconds(1)` (`TargetUpdater.cs:19`) |
| Eingehende Heilung auf ein Party-Mitglied | **nicht ausgewertet, aber verfügbar** | `ActionEffect.ActionEffectEvent` liefert jedes Effektpaket mit Betrag und Ziel; `Watcher.cs:17-18` hängt daran nur zwei Handler, gefiltert auf Quelle = Gegner beziehungsweise Quelle = Spieler. Ein Paket eines fremden Heilers passiert beide Filter, wird also von niemandem gelesen |
| Schadensbetrag eines Gegnertreffers | **verfügbar und heute schon gelesen** | `Watcher.cs:137` wertet `damageEffect.value` aus, prüft aber nur `> 0` |

**Der Befund ist damit nicht „nicht messbar", sondern „der Aufnehmer fehlt".** Die
Datenquelle liegt an, sie wird nur nicht auf Party-Mitglieder angewandt. Das ist ein
wesentlicher Unterschied für die Umsetzbarkeit — und es benennt genau, was zu bauen
wäre.

Zwei Genauigkeitsgrenzen bleiben auch nach einem solchen Umbau und sind zu benennen,
nicht wegzuformulieren:

1. **Die 1-Hz-Abtastung ist für ein Zehn-Sekunden-Fenster zu grob.** Zehn Stützstellen
   für eine Kursprognose, deren Entscheidung in der letzten Sekunde fällt, sind zu
   wenig; die Prognose wäre in der entscheidenden Sekunde eine Sekunde alt. Für diesen
   Zweck ist eine eigene, feinere Aufzeichnung nötig, nicht die vorhandene mitbenutzt.
2. **Ein Gesundheitsdelta ist ein Surrogat für die kumulierte Heilung.** Fallen
   Heilung und Schaden in dasselbe Abtastintervall, heben sie sich im Delta auf,
   obwohl die Heilung gegen die geforderte Summe zählt. Die Effektpakete zu lesen
   misst die Größe selbst; das Delta misst nur ihre Nettowirkung. Nach der
   Projektregel zur Surrogatmessung ist die Paketauswertung der zulässige Weg und
   das Delta bestenfalls eine Rückfallebene.

### Klasse C — reine Schadensreduktion: keine Wechselwirkung

Rampart, Sentinel, Shadow Wall, Nebula, Sheltron und Holy Sheltron, Bulwark,
Camouflage, Oblation, Heart of Corundum und Clarity of Corundum (die
Reduktionsanteile), Reprisal. Heilung und Schilde sind hier weder schädlich noch
besonders dringend.

### Klasse D — Selbstheilung ohne Auslöser: Heilung ist redundant, nicht schädlich

Bloodwhetting und Raw Intuition (heilen bei Waffenskill-Treffern), Aurora,
Clemency, Nascent Flash. Fremde Heilung addiert sich, verhindert aber nichts.

## Warum die Überlebensprüfung in ihrer bisherigen Form nicht trägt

`TankSurvivesWithoutMe` sollte die Rückhaltung absichern: erst prüfen, ob der Tank
ohne mich überlebt, dann zurückhalten. Drei Einwände aus der Spielpraxis zeigen, dass
die vorgesehene Konstruktion diese Zusage nicht einlösen kann.

**Erstens: Tankbuster wechseln das Ziel mitten in der Sequenz.** In Begegnungen mit
zwei Tanks ist der übliche Ablauf, dass der erste Tank einen Buster nimmt, der
Offtank danach die Aggro übernimmt und den zweiten auf sich zieht. Die vorhandene
Vorhersage bildet das nicht ab: `DataCenter.BMRNextTankbusterIn` ist **eine einzige
Zahl** — die Zeit bis zum nächsten Tankbuster, ohne Angabe, auf wen. Und
`IsHostileCastingTankBusterAtMe` ist ausdrücklich spielerzentriert; der Kommentar bei
`DataCenter.cs:2087` begründet das ausführlich. Für die Frage „welcher der beiden
Tanks bekommt den nächsten Schlag" gibt es keine Größe.

**Zweitens: Ein Tankbuster ist nicht ein Einschlag.** Mehrfach einschlagende Buster —
der Auftraggeber nennt Unreal Shinryu und Arkh Monh — sind eine Folge von Treffern in
kurzem Abstand. `BMRNextTankbusterIn` nennt den Beginn, nicht die Anzahl und nicht die
Gesamtsumme. Eine Prüfung „steht gerade ein Tankbuster an?" beantwortet also nicht,
ob der Tank *diese Sequenz* übersteht.

**Drittens, und das wiegt am schwersten: Manche Buster töten auch einen vollgeheilten
und geschildeten Tank.** Wenn der Tank seine eigene Notfallfähigkeit nicht zündet,
ändern Vollheilung und Schild vorab nichts am Ausgang. Die stillschweigende Annahme
hinter jeder b-Zeile — „wenn ich rechtzeitig heile, überlebt er" — trifft für diese
Fälle nicht zu.

**Was daraus folgt, ist nicht, die Rückhaltung freizugeben.** Der dritte Einwand
belegt, dass Heilung manchmal nichts nützt; er belegt nicht, dass Zurückhaltung dann
besser wäre. Sie ist in diesem Fall nur nicht schlechter — und welcher Fall vorliegt,
ist vorab nicht erkennbar. Nach der Rangordnung entscheidet bei Gleichstand auf Stufe
1 die Stufe 2, aber eben nur bei belegtem Gleichstand, und der ist hier nicht
herstellbar.

Die tragfähige Folgerung lautet für **A-nichttödliche** Auslöser:

> **Jedes erkannte Tankbuster-Fenster hebt die Rückhaltung auf, unabhängig vom
> Gesundheitsstand.** Nicht erst, wenn der Tank nach HP „in Gefahr" ist.

Denn genau in diesem Fenster ist die Prognose unmöglich: Zielwechsel, Trefferzahl und
Gesamtschaden sind allesamt unbekannt. Eine Regel, die in einem Zustand
zurückhält, den sie nicht beurteilen kann, ist eine Wette, keine Entscheidung. Die
HP-Schwelle bleibt als zweite, unabhängige Aufhebung bestehen — sie fängt den
Dauerschaden ab, das Buster-Fenster den Einzelschlag.

### Für Living Dead gilt das Gegenteil, und das ist keine Ausnahme, sondern die Regel

Eine frühere Fassung dieses Abschnitts erklärte die Buster-Aufhebung für **alle**
Auslöser. Das ist für Living Dead falsch, und zwar grundlegend:

> **In Phase 1 ist der Tod das Ziel.** Ein Tankbuster ist dort nicht die Gefahr, vor
> der zu retten wäre — er ist das Ereignis, auf das die Fähigkeit wartet.

Beide üblichen Aufhebungen kehren sich damit um:

| Aufhebung | Bei A-nichttödlich | Bei Living Dead Phase 1 |
|---|---|---|
| Niedrige Gesundheit | hebt auf — der Tank droht zu sterben | **hebt nicht auf** — niedrige Gesundheit ist der erwünschte Zustand, sie bringt den Auslöser näher |
| Tankbuster-Fenster | hebt auf — die Sequenz ist nicht beurteilbar | **hebt nicht auf** — der Buster liefert den Auslöser |

Der Auftraggeber benennt die beiden Weisen, auf die eine Vollheilung bei zehn Prozent
Gesundheit in Phase 1 danebengeht, und beide treffen zu:

1. **Sie rettet nicht.** Tötet der Buster den Tank auch vollgeheilt, war die Heilung
   wirkungslos — genau der Einwand aus dem vorigen Abschnitt, hier gegen die Heilung
   gewendet statt für sie.
2. **Sie schadet.** Rettet sie ihn, hat sie den Übergang in Phase 2 verhindert und
   damit die Fähigkeit entwertet, die der Tank für genau diesen Moment gezündet hat.

Es gibt keinen dritten Ausgang: Eine Heilung, die den Tod in Phase 1 verhindert,
verhindert per Konstruktion den Auslöser. Das ist keine Abwägung, sondern eine
Identität.

**Damit bleibt für Living Dead Phase 1 genau eine Aufhebung übrig** — und sie hat
nichts mit Gefahr zu tun, sondern mit der Uhr:

> Die Rückhaltung wird aufgehoben, **wenn der Tod nicht mehr vor dem Ende der ersten
> Phase eintreten würde.** Dann und nur dann.

Dazu kommt die Vorbedingung aus Fall 1b, die vor der Phase liegt und keine Aufhebung
im Verlauf ist: Wer die Heilmenge für Phase 2 gar nicht aufbringen kann, darf den Weg
über den Tod nicht einschlagen.

## Fallvarianten

Vollständig über die Dimensionen Fähigkeitsklasse × Gesundheitsstand ×
Heileraktion. „Richtig" meint jeweils die Handlung, die dem Vorrang der
Überlebenssicherung folgt.

| # | Lage | Heilung richtig? | Schild richtig? | Begründung |
|---|---|---|---|---|
| 1 | DRK, Living Dead aktiv, **Hochrechnung: Tod tritt vor Ablauf ein**, Heilkapazität für Phase 2 gesichert | **nein** | **nein** | Stufe 1 ist gesichert, also darf Stufe 2 entscheiden: beides verhindert den Tod, den die Fähigkeit einplant |
| 1b | dieselbe Lage, **Heilkapazität nicht gesichert** | **ja** | ja | Stufe 1 schlägt Stufe 2. Ohne die Kapazität für die volle Heilmenge in Walking Dead ist der Verzicht ein Tausch von sicherem gegen unsicheres Überleben |
| 1c | DRK, Living Dead aktiv, **Tod würde nicht mehr vor Phasenende eintreten** | **ja, rechtzeitig vor Ablauf** | ja | Die **einzige** Aufhebung in Phase 1. Der Schutz verpufft sonst und lässt den Tank ungeheilt und ungeschützt zurück |
| 1d | DRK, Living Dead aktiv, **Tankbuster-Fenster erkannt** | **nein** | **nein** | Kehrseite der allgemeinen Regel: Der Buster liefert den Auslöser. Ihn abzufangen verhindert entweder nichts (er tötet ohnehin) oder das Gewollte (Phase 2) |
| 1e | DRK, Living Dead aktiv, **HP sehr niedrig**, Tod aber noch vor Ablauf zu erwarten | **nein** | **nein** | Niedrige Gesundheit ist hier der erwünschte Zustand, keine Gefahrenmeldung. Eine Vollheilung rettet nicht oder entwertet die Fähigkeit — ein dritter Ausgang existiert nicht |
| 2 | DRK, Living Dead aktiv, HP niedrig, Ablauf fern | wie 1 / 1b / 1e | dito | Der Gesundheitsstand entscheidet in dieser Phase überhaupt nicht; entscheidend sind allein die Uhr und die Kapazität für Phase 2 |
| 3 | DRK, Living Dead läuft ab und der Tod ist nicht eingetreten | **ja, dringend** | ja | Ausführungsform von 1c: Der Eingriff muss so früh beginnen, dass er vor dem Ablauf wirkt — bei einem Heilzauber mit Wirkzeit also mit Vorlauf, bei einer Fähigkeit ohne Wirkzeit erst in der Ablaufsekunde |
| 4 | DRK, Walking Dead aktiv, **Kurs trägt bis zum Phasenende** | leicht unterstützen (HoT, Regeneration) | nein, wirkungslos bei 1 HP | Billige Beiträge zählen voll gegen die geforderte Summe; ein teurer Notfallzauber wäre hier Überheilung |
| 4a | DRK, Walking Dead aktiv, **Kurs reicht nicht** | **ja, in voller Höhe** | nein | Die Alternative ist der Tod am Phasenende. Kein Rückhaltefall mehr |
| 4b | DRK, **Undead Rebirth** aktiv | nein, nachrangig | nein | Die Bedingung ist erfüllt, der Tank kann nicht sterben. Reine Stufe 3, ohne Berührung von Stufe 1 oder 2 |
| 5 | DRK, TBN aktiv, Schaden läuft, **Tank nicht in Gefahr** | zurückhaltend | zurückhaltend | Stufe 1 gesichert. Die Wirkung eines fremden Schildes auf die Barriere ist **unbelegt** und für das Plugin **nicht beobachtbar** (ein einziger `ShieldPercentage`-Wert je Charakter). Der Fall trägt keine Regel ohne Option |
| 5b | dieselbe Lage, **Tank in Gefahr oder Tankbuster-Fenster** | **ja** | **ja** | Stufe 1 schlägt Stufe 2. Ein möglicherweise verlorener Dark Arts wiegt keinen toten Tank auf |
| 6 | GNB, Superbolide aktiv | **ja** | ja | HP stehen auf 1; das Fenster ist die einzige gefahrlose Gelegenheit |
| 7 | WAR, Holmgang aktiv, HP heruntergedrückt | **ja** | ja | wie 6 |
| 8 | PLD, Hallowed Ground aktiv, beim Zünden wenig HP | **ja** | ja | Die HP bleiben unverändert; nach Ablauf steht er, wo er stand |
| 9 | PLD, Hallowed Ground aktiv, beim Zünden viel HP | nein, nachrangig | nein | Einziger Fall echter Entbehrlichkeit in Klasse B |
| 10 | GNB, HP knapp über 50 %, Heart of Corundum liegt | **nach normaler Regel** | nach normaler Regel | **Kein Rückhaltefall.** Der Catharsis-Stoß löst auch bei Ablauf aus; es gibt nichts zu verschenken |
| 11 | GNB, HP bereits unter 50 %, Catharsis ausgelöst | **ja** | ja | Der Auslöser ist verbraucht, nichts steht mehr im Weg |
| 12 | Beliebig, Klasse C aktiv | nach normaler Regel | nach normaler Regel | keine Wechselwirkung |
| 13 | Beliebig, Klasse D aktiv | nachrangig | nach normaler Regel | Die Selbstheilung trägt einen Teil, ersetzt sie aber nicht |
| 14 | Mehrere Gruppenmitglieder brauchen Heilung, Tank geschützt | Tank **nachrangig**, andere zuerst | dito | Der Schutz verschiebt die Dringlichkeit, hebt den Bedarf nicht auf |
| 15 | Nur der geschützte Tank braucht Heilung, kein Auslöser betroffen | **ja** | ja | Nichts spricht dagegen; das Fenster ist die sicherste Gelegenheit |

Zeile 14 und 15 sind der Kern: **Der Schutz verschiebt die Reihenfolge, er hebt den
Bedarf nicht auf.** Handeln schadet nur in den Zeilen 1, 2 und 5 — und dort
ausschließlich, solange Stufe 1 gesichert ist. Die b-Zeilen sind nicht Ausnahmen am
Rand, sondern der Regelfall, sobald der Tank in Gefahr gerät: **Jede Rückhaltung
steht unter dem Vorbehalt des Überlebens.**

Die Prüfung am Artefakt und die Einarbeitung der Spielpraxis haben das Verhältnis
weiter zu Ungunsten der Rückhaltung verschoben. Von neunzehn Lagen bleiben **zwei**
echte Rückhaltefälle (1, 5); ihnen stehen fünf Aufhebungen gegenüber (1b, 1c, 1d, 4a,
5b), einer ist reine Ressourcenschonung ohne Risiko (4b), und der Rest verlangt
normales, leicht unterstützendes oder nachrangiges Handeln. Der Gunbreaker-Fall ist
ganz entfallen, und von den zwei verbliebenen Rückhaltefällen ruht einer (5) auf
unbelegter Mechanik.

Bemerkenswert ist, dass die Zahl der Aufhebungen die der Rückhaltefälle nun um mehr
als das Doppelte übersteigt. Eine Bedingung, die häufiger nicht gilt als gilt, ist
kein Leitmotiv. **Der belastbare Kern des Vorhabens ist die Richtigstellung der
Zielwahl; die Rückhaltung ist ein eng umgrenzter Sonderfall mit vielen Ausnahmen.**

## Was RSR heute tut

| Mechanik | Kenntnis | Bewertung |
|---|---|---|
| Unverwundbarkeiten | `StatusHelper.NoNeedHealingStatus` (`StatusHelper.cs:421`) unterdrückt Heilung, Freigabe zwei GCDs vor Ablauf | Konstruktion falsch: prüft den Gesundheitsstand nicht, siehe `TODO.md` |
| Inhalt dieser Liste | `Holmgang_409`, `LivingDead`, `Superbolide`, `Invulnerability`, `HpRecoveryDown`, `Mounted`; `WalkingDead` auskommentiert | `Holmgang_409` ist **belegt richtig** (Status 409: „Most attacks cannot reduce your HP to less than 1"; 88 und 1305 sind der Bewegungs-Debuff, 1304 die PvP-Selbstform). Die Auskommentierung von `WalkingDead` ist **ebenfalls richtig** und ein Beleg verstandener Entwurfsabsicht. **`UndeadRebirth` fehlt** — Lücke, siehe unten. `HallowedGround` fehlt in **jeder** Form |
| Heilzielauswahl | `ActionTargetInfo.cs:3536` liest dieselbe Prüfung invertiert | Defekt, Wirkung schwerer als zunächst bewertet — siehe unten |
| Excogitation | `SCH_Reborn.cs:830` liest die Prüfung **ebenfalls invertiert** | **Neuer Fundort derselben Defektklasse.** Excogitation geht nur auf Ziele, die gerade unverwundbar sind |
| Co-Tank-Aggrorücknahme | `ObjectHelper.cs:125` liest die Prüfung richtig, mit erklärendem Kommentar | Beleg der gemeinten Semantik, unabhängig von `StateUpdater` |
| The Blackest Night | Nur als Eintrag in `ShieldStatus` (`StatusHelper.cs:395`), also als Beitrag zur effektiven Gesundheit | Der Bruchmechanismus ist nicht beobachtbar, siehe Klasse A+ |
| Catharsis of Corundum | Kommt einmal vor, als `StatusProvide` der Gunbreaker-Aktion | Kein Handlungsbedarf: Der Auslöser ist nicht raubbar |
| Heart of Corundum, Excogitation als Auslöser | Keine Fundstelle in einer Entscheidung | unbekannt, und für Heart of Corundum nun auch gegenstandslos |
| Klasse C und D | keine Sonderbehandlung | richtig so |

### Der Zielwahl-Defekt wiegt schwerer als zuerst bewertet

`NoNeedHealingInvuln()` ruft `WillStatusEndGCD(2, 0, false, …)` auf. Die Kette
`WillStatusEnd` → `StatusTime` (`StatusHelper.cs:781` und `:845`) liefert bei
**fehlendem** Status die Zeit `0f`, und `(0 >= 0 || !HasStatus) && 0 <= time` ergibt
`true`. Der Rückgabewert bedeutet also: *kein schützender Status aktiv, Heilung ist
zuzulassen.* Der Name sagt das Gegenteil, was die Defektklasse erklärt.

`GeneralHealTarget` sammelt mit `if (!o.NoNeedHealingInvuln())` folglich genau die
Gruppenmitglieder, die **einen aktiven Schutz tragen**. Im Regelfall — niemand ist
unverwundbar — bleibt `healingNeededObjs` **leer**. Damit laufen alle darauf
aufbauenden Stufen ins Leere: die Heiler-Vorrangprüfung, die Tank-Vorrangprüfung
und die Auswahl des am schwersten Verletzten. Die Funktion liefert `null`, und der
Aufrufer (`ActionTargetInfo.cs:3497 ff.`) fällt auf den Träger der Tank-Haltung
zurück, sonst auf `partyMembers[0]`.

Die praktische Folge ist nicht bloß eine falsche Reihenfolge, sondern: **Die
generische Heilzielwahl ist blind für den am schwersten verletzten Gruppenteil und
wählt faktisch immer den Tank.** Einzig die Selbstprüfung gegen
`Service.Config.HealthSelfRatio` funktioniert noch, weil sie die Liste nicht
benutzt. Und in dem einen Fall, in dem die Liste gefüllt ist — jemand trägt einen
Invuln —, wird bevorzugt der Unverwundbare geheilt.

Damit gehört dieser Defekt zu **Rangstufe 1**, nicht zu Stufe 3.

## Optionen

**O0 — Nullvariante.** Nichts ändern. Die beiden erfassten Defekte bleiben; die
Klasse-A-Wechselwirkungen bleiben unbekannt.

**O1 — Nur die Defekte beheben.** Invertierte Prüfung korrigieren, Konstruktion
von Ausschluss auf Priorität umstellen. Klasse A bleibt bis auf Living Dead
unbehandelt.

**O2 — Klasse A vollständig behandeln.** Zusätzlich TBN, Catharsis und
Excogitation berücksichtigen.

**O3 — Nur Living Dead behandeln, Rest verwerfen.** Der Fall mit dem größten
Einzelnutzen, ohne die übrigen.

## Abwägung

| Option | Nutzen | Aufwand | Blast Radius | Risiko |
|---|---|---|---|---|
| O0 | keiner | keiner | keiner | Defekte bleiben |
| O1 | Behebt zwei belegte Defekte und stellt Fall 3, 6, 7, 8, 14, 15 richtig | mittel | **groß** — zentrale Heilzielwahl aller Jobs | Priorisierung greift erstmals; Verhalten ändert sich breit |
| O2 | zusätzlich Fall 5 und 10 | hoch | groß plus vier neue Sonderfälle | Jeder Sonderfall kann falsch liegen; die Auslöser sind statisch nicht prüfbar |
| O3 | Fall 1, 2, 4 | klein | klein | Lässt die belegten Defekte stehen |

**Gewählt: O1, dann O2 stufenweise.** O1 behebt Belegtes und richtet die
Konstruktion; O2 setzt darauf auf. O3 scheidet aus, weil es den größeren, bereits
belegten Fehler stehen ließe, um einen Sonderfall zu bedienen.

## Falsifikation

**Hypothese 1: Es liegt kein Defekt vor.** Die heutige Unterdrückung sei eine
bewusste Ressourcenschonung. Widerlegt für die invertierte Prüfung, und zwar nun
dreifach statt einfach: `ActionTargetInfo.cs:3536` und `SCH_Reborn.cs:830` werten
dieselbe Funktion mit dem entgegengesetzten Vorzeichen aus wie
`StateUpdater.cs:726`/`:771` und `ObjectHelper.cs:125`. Der Kommentar bei
`ObjectHelper.cs:120` — „unless they are riding an invulnerability (Superbolide
leaves them at 1 HP on purpose)" — benennt die gemeinte Semantik ausdrücklich und
entscheidet, welche Seite richtig ist. Der Rückgabewert der Kette wurde zusätzlich
am Artefakt nachgerechnet (`StatusHelper.cs:781`, `:845`).

Ebenfalls widerlegt für die fehlende Gesundheitsprüfung — aber **nicht mit dem
Paladin.** Die erste Fassung führte hier an, ein Paladin, der bei fünf Prozent
zünde, stehe nach zehn Sekunden bei fünf Prozent. Das Beispiel trägt nicht:
`HallowedGround` steht in keiner Form in `NoNeedHealingStatus`, der Paladin löst
also gar keine Unterdrückung aus. Der tragfähige Beleg ist **Superbolide**: Es steht
in der Liste, setzt die Gesundheit sofort auf 1 und unterdrückt die Heilung
anschließend zehn Sekunden lang, ohne diesen Stand je zu prüfen. Die Freigabe kommt
erst zwei GCDs vor Ablauf.

**Hypothese 2: Die Prioritätsstufe ist die falsche Lösung.** Alternative wäre eine
Schwelle: unterdrücken nur oberhalb eines Gesundheitswertes. Hält teilweise
stand — sie wäre einfacher, träfe aber Fall 14 nicht, weil sie den Vergleich mit
anderen Gruppenmitgliedern nicht kennt. Die Prioritätsstufe leistet beides, ist
aber der größere Eingriff. Für O1 wird die Stufe gewählt, weil `GeneralHealTarget`
die Rangfolge bereits besitzt und nur eine Ebene fehlt.

**Hypothese 3: Klasse A ist nicht behandelbar.** Die Auslöser sind Ereignisse, die
RSR nicht vorhersehen kann. Hält für Excogitation stand: Ob der Schaden kommt, der
die Schwelle unterschreitet, ist nicht bekannt. **Excogitation wird deshalb aus O2
herausgenommen.** Für Catharsis ist die Hypothese gegenstandslos geworden — es gibt
keinen raubbaren Auslöser. Für Living Dead hält sie nicht: Der Status ist sichtbar,
und die Regel muss nur zurückhalten, solange er liegt.

**Für The Blackest Night hält sie nun ebenfalls stand**, anders als in der ersten
Fassung angenommen. Sichtbar ist dort der *Status*, nicht der *Auslöser*: Ob die
Barriere vollständig verzehrt wurde, ist aus einem einzigen `ShieldPercentage`-Wert
nicht ableitbar. Eine Regel kann also nur „solange TBN liegt" bedeuten — und ob das
dem Dunkelritter nützt, hängt an einer unbelegten Stapelmechanik. Der Fall
verbleibt in O2, aber ausdrücklich als nicht nachweisbare Verbesserung hinter einer
Option, nicht als Defektbehebung.

**Was nicht widerlegt ist:** dass die Prioritätsstufe im Spiel besser spielt. Für
die *Richtigstellung* der Zielwahl ist das anders zu bewerten als in der ersten
Fassung — dass die heutige Auswahl den am schwersten Verletzten übergeht, ist am
Artefakt belegt, und ihre Behebung ist eine Defektbehebung, keine Wette. Offen
bleibt nur, wie sich die erstmals wirksame *Rangfolge innerhalb* der Behebung
spielt.

## Umsetzungskonzept

Drei Schritte. Der erste behebt, die beiden anderen erweitern.

### Schritt 1 — Die invertierte Prüfung korrigieren, an beiden Fundorten

`ActionTargetInfo.cs:3536`: `if (!o.NoNeedHealingInvuln())` wird zu
`if (o.NoNeedHealingInvuln())`. Eine Zeile, aber die Wirkung ist groß: Erst danach
enthält `healingNeededObjs` überhaupt die normalen Gruppenmitglieder, und die
Rangfolge Selbst → Heiler → Tank → niedrigste Gesundheit greift zum ersten Mal.

`SCH_Reborn.cs:830` trägt dieselbe Invertierung:
`member.GetHealthRatio() <= ExcogHeal && !member.NoNeedHealingInvuln()` legt
Excogitation genau auf jene Ziele, die gerade unverwundbar sind. Die Negation
entfällt ebenso. Der Blast Radius ist hier klein — eine Rotation, eine Fähigkeit —,
weshalb dieser Teilschritt für sich genommen unkritisch ist.

Die Defektklasse ist damit als solche belegt und nicht als Einzelfall zu behandeln.
Ihre Wiederholbarkeit hat eine benennbare Ursache: `NoNeedHealingInvuln` liefert
`true`, wenn *kein* Schutz vorliegt — der Name legt das Gegenteil nahe. Solange er
so heißt, entsteht der Fehler bei jeder neuen Aufrufstelle erneut. Das Prüfskript
aus dem Abschnitt „Nachweisbarkeit" adressiert die Wiederholbarkeit; eine Umbenennung
wäre die eigentliche Behebung, berührt aber die Paketoberfläche und ist daher
getrennt zu entscheiden.

### Schritt 2 — Vom Ausschluss zur Prioritätsstufe

`NoNeedHealingStatus` wird nicht mehr als Filter verwendet, sondern als
Sortierkriterium: geschützte Ziele wandern ans Ende von `healingNeededObjs`,
statt herauszufallen. Fall 14 und 15 sind damit beide richtig — andere zuerst,
der Tank danach.

Die Zwei-GCD-Freigabe bleibt als Beschleuniger: Läuft der Schutz aus, verliert das
Ziel die Herabstufung und rückt nach vorn.

Damit entfällt zugleich die Frage nach den fehlenden Status-Ids: Ihre Ergänzung ist
erst nach diesem Schritt unschädlich, weil sie dann nur noch die Reihenfolge
beeinflusst, nicht mehr das Ob. Die Prüfung am Artefakt hat die Liste dabei
präzisiert:

- **`HallowedGround`** (82) und **`HallowedGround_1302`** fehlen und sind zu
  ergänzen. Beide sind „Impervious to most attacks" — echte Unverwundbarkeiten.
- **`UndeadRebirth`** (3255) fehlt und ist zu ergänzen: die dritte Living-Dead-Phase,
  in der der Dunkelritter nicht sterben kann. Heute wird er dort weitergeheilt.
- **`Holmgang_409`** ist bereits richtig. Die übrigen Holmgang-Ids sind **nicht**
  zu ergänzen: 88 und 1305 beschreiben Bewegungsunfähigkeit, nicht Schutz, und
  1304 ist die PvP-Selbstform. Die erste Fassung forderte hier „drei von vier"
  fehlende Ids — das war ein Nullbefund über den bloßen Namen, ohne Prüfung der
  Wirkbeschreibung.

`UndeadRebirth` ist ein Lehrstück für Parnas' *Lack of Movement*: Die Aufzählung war
bei ihrer Entstehung vollständig und wurde durch eine spätere Spielerweiterung
unrichtig, ohne dass etwas fehlschlug. Die Umstellung auf die Prioritätsstufe
behebt den Einzelfall; die Wiederholbarkeit bleibt, solange eine Aufzählung dort
steht, wo eine Fähigkeitsprüfung stehen müsste. Ein Nachfolgebefund ist damit bei
der nächsten Erweiterung zu erwarten und in `TODO.md` als technische Schuld zu
führen.

### Schritt 3 — Klasse A als Rückhalteliste, unter dem Überlebensvorbehalt

Eine kleine, begründete Liste für die Fälle, in denen Handeln tatsächlich schadet —
jeder Eintrag steht für einen belegten Auslöser, nicht für „ist gerade geschützt":

```
StatusHelper.TriggerBearingStatus = [ LivingDead, BlackestNight ]
```

`WalkingDead` gehört ausdrücklich **nicht** hinein: Dort ist Heilung die
Überlebensbedingung, und fremde Heilung zählt gegen die geforderte Gesamtmenge.
`UndeadRebirth` gehört ebenfalls nicht hinein, sondern in `NoNeedHealingStatus` —
es ist kein raubbarer Auslöser, sondern ein erreichter Endzustand.

Die Corundum-Frage ist **erledigt**: Weder `ClarityOfCorundum` noch
`CatharsisOfCorundum` kommen in diese Liste. Der Catharsis-Heilstoß löst auch bei
Ablauf der Wirkdauer aus, ist also nicht raubbar; `ClarityOfCorundum` ist reine
Schadensreduktion.

**Die Rückhaltung greift nur, wenn Stufe 1 gesichert ist.** Das ist keine
Ausnahme am Ende der Bedingung, sondern die erste Prüfung. Nach Einarbeitung der
Spielpraxis kommt eine zweite Vorbedingung hinzu — die Hochrechnung — und die
Buster-Aufhebung wandert aus der Gefahrenprüfung heraus in eine eigene Bedingung:

**Es gibt nicht eine Bedingung, sondern zwei — je Auslöserklasse eine.** Eine
gemeinsame Bedingung war der Fehler der vorigen Fassung: Was bei A-nichttödlichen
Auslösern aufhebt, muss bei Living Dead gerade nicht aufheben.

```
protected bool MayWithholdForTrigger(IBattleChara tank) =>
    WithholdForTriggersEnabled && (
        tank.HasStatus(false, StatusID.LivingDead)
            ? MayWithholdForDeathTrigger(tank)
            : tank.HasStatus(false, StatusHelper.NonLethalTriggerStatus)
              && MayWithholdForNonLethalTrigger(tank));

// A-tödlich: der Tod IST der Auslöser. Weder niedrige Gesundheit noch ein
// Tankbuster heben auf - beide bringen den Auslöser näher.
protected bool MayWithholdForDeathTrigger(IBattleChara tank) =>
       DeathStillArrivesInTime(tank)     // einzige Aufhebung im Verlauf
    && CanSustainPhaseTwo();             // Vorbedingung, vor der Phase

// A-nichttödlich: der Tod ist die Katastrophe, nicht das Ziel.
protected bool MayWithholdForNonLethalTrigger(IBattleChara tank) =>
       !TankbusterWindowOpen()
    && TankSurvivesWithoutMe(tank);
```

`TankbusterWindowOpen()` fasst `DataCenter.BMRTankbusterImminent` und
`DataCenter.IsHostileCastingToTank` zusammen. Es steht bei den nichttödlichen
Auslösern **vor** der Gesundheitsprüfung, weil es aus einem anderen Grund aufhebt:
nicht weil der Tank in Gefahr *ist*, sondern weil in diesem Fenster nicht beurteilbar
ist, ob er es wird.

`TankSurvivesWithoutMe` prüft `tank.GetEffectiveHpPercent()` gegen
`Service.Config.HealthForDyingTanks` (`ObjectHelper.cs:126` nutzt dieselbe Schwelle
bereits für sterbende Tanks). **Es kommt im tödlichen Zweig nicht vor.**

`CanSustainPhaseTwo()` ist Fall 1b und die einzige Vorbedingung des tödlichen
Zweiges: Wer die volle Heilmenge für Walking Dead nicht aufbringen kann, darf den Weg
über den Tod nicht einschlagen. Ohne belastbares Maß konservativ genähert —
`DataCenter.CurrentMp` über einer Schwelle und kein weiteres Gruppenmitglied unter
`Service.Config.HealthTankRatio`. Sie wird **vor** dem Eintritt geprüft und nicht
laufend, damit sie nicht mitten in der Phase kippt und den Weg auf halber Strecke
abbricht.

`DeathStillArrivesInTime(tank)` trägt die Zeitbetrachtung und ist die einzige
Aufhebung im Verlauf. Sie ist in zwei Ausbaustufen zu bauen, und **die erste kommt
ohne den Messbaustein aus** — was die Aufwandsdarstellung des vorigen Durchgangs
korrigiert:

**Stufe 1 — reine Uhr, ohne jede Messung.** Zurückgehalten wird, solange
`StatusTime(LivingDead)` größer ist als der Vorlauf, den das vorgesehene Heilmittel
braucht. Unterschreitet die Restzeit ihn, wird gehandelt. Mehr verlangt der Kern der
Anforderung nicht: „im letzten Augenblick retten" ist eine Aussage über die Uhr, nicht
über eine Rate. Damit ist Schritt 0 für Living Dead **keine Voraussetzung**, sondern
eine Verbesserung.

**Der Vorlauf ist die entscheidende Größe und hängt am Heilmittel:**

| Heilmittel | Vorlauf | Folge |
|---|---|---|
| Fähigkeit ohne Wirkzeit (Benediction, Tetragrammaton) | praktisch nur die Verzögerung zum Server | Das Rettungsfenster liegt in der **letzten Sekunde**. Der Zeitraum, in dem fälschlich ein noch rechtzeitiger Tod verhindert wird, ist minimal |
| Zauber mit Wirkzeit | Wirkzeit plus Puffer, also ein bis zwei GCDs | Das Fenster beginnt früher und der Fehler wird entsprechend größer |

Daraus folgt eine Vorgabe für die Umsetzung: **In dieser Phase ist einer Fähigkeit
ohne Wirkzeit der Vorzug zu geben**, und zwar nicht aus Effizienz, sondern weil sie
den einzigen echten Fehler der Uhrregel klein hält. Benediction ist für diesen Zweck
das genaue Werkzeug — Vollheilung, sofort, in der letzten Sekunde.

**Stufe 2 — mit Messung, früher und genauer.** Zeigt die beobachtete Schadensrate,
dass die Gesundheit die Null bis zum Ablauf ohnehin nicht mehr erreicht, kann früher
aufgehoben werden. Das verlängert das Rettungsfenster und erlaubt auch ein Heilmittel
mit Wirkzeit.

**Der eine Fehler, den die Uhrregel behält, ist zu benennen:** Sie kann in den
letzten Sekunden einen Tod verhindern, der noch rechtzeitig gekommen wäre — der
Auslöser geht dann verloren. Genau diesen Fehler verkleinert der knappe Vorlauf, und
genau ihn beseitigt die Messung. Er ist der Preis dafür, ohne Messbaustein
auszukommen, und in dieser Richtung der sichere: Ein verlorener Auslöser kostet einen
Cooldown, ein zu spät geretteter Tank kostet den Kampf.

**Was ohne Messung ausdrücklich nicht gilt:** Die frühere Fassung ließ
`TriggerStillReachable` ohne Messbaustein `false` liefern, also nie zurückhalten. Für
Living Dead wäre das falsch — „nicht zurückhalten" heißt dort heilen, und Heilung
entwertet die Fähigkeit. Der Rückfall ohne Messung ist die Uhrregel, nicht die
Freigabe.

### Schritt 0 — der fehlende Messbaustein (nachrangig, nicht vorausgesetzt)

Der vorige Durchgang stellte diesen Schritt allen anderen voran, weil die
Hochrechnung ohne ihn nicht ausführbar sei. **Das war zu weit gegriffen.** Die
Kernanforderung — im letzten Augenblick retten — ist eine Uhrregel und braucht keine
Rate. Der Messbaustein verkleinert nur den verbleibenden Fehler: dass die Uhrregel
gelegentlich einen Tod verhindert, der noch rechtzeitig gekommen wäre.

Er bleibt sinnvoll und wird hier vollständig beschrieben, rutscht aber ans Ende der
Reihenfolge. Er entscheidet nichts, er misst nur, und ist deshalb für sich allein
nutzbar.

**Was zu bauen ist:**

1. **Gesundheitshistorie für Party-Mitglieder.** Nicht durch Erweiterung von
   `DataCenter.RecordedHP` — dessen 1-Hz-Takt und Vier-Minuten-Fenster sind auf die
   Gegner-Lebensdauer zugeschnitten, für ein Zehn-Sekunden-Fenster zu grob und für
   acht zusätzliche Objekte über vier Minuten unnötig teuer. Stattdessen ein eigener
   Ringpuffer über ein kurzes Fenster mit deutlich feinerem Takt, gefüllt aus dem
   bestehenden Updater-Pfad.
2. **Ein dritter Effekt-Handler.** `Watcher.cs:17-18` hängt heute zwei Handler an
   `ActionEffect.ActionEffectEvent`, gefiltert auf Quelle = Gegner und Quelle =
   Spieler. Ein Paket eines fremden Heilers auf ein Party-Mitglied passiert beide.
   Ein `ActionOnPartyMember`-Handler liest daraus `ActionEffectType.Heal` mit Betrag
   und Ziel — und misst damit die kumulierte Heilung selbst statt ihrer Nettowirkung.
   Das ist der Unterschied zwischen Messung und Surrogat.
3. **Zwei abgeleitete Größen**, beide als reine Lesefunktionen:
   `IncomingDamageRate(member)` in Gesundheitsanteil je Sekunde, und
   `HealingReceivedSince(member, seconds)` als absolute Summe.

**Betroffenenkreis.** Punkt 1 und 2 laufen in jedem Kampf für jedes Party-Mitglied
mit, also auch bei Nutzern, die nie einen Dunkelritter sehen. Die Kosten sind damit
allgemein, der Nutzen ist es zunächst nicht — das ist bei der Freigabe zu
berücksichtigen und spricht dafür, den Aufnehmer nur zu betreiben, solange ihn
jemand liest.

Für `BlackestNight` gilt die Rückhaltung **vorrangig dem Schild**, nicht der
Heilung: Die Barriere wird von Schaden verzehrt, nicht von fehlender Heilung, und
ein zusätzlicher Schild ist die direkte Störung. Heilung wirkt nur mittelbar, indem
sie den Schadensdruck senkt. Wo beides zur Wahl steht, ist der Schild zurückzuhalten
und die Heilung nur herabzustufen.

**Einschränkung nach der Artefaktprüfung:** Für `BlackestNight` ruht diese Regel auf
zwei unbelegten Stücken — der Stapelmechanik zweier Schilde und der Annahme, dass
Zurückhalten dem Verbrauch nützt. Beobachtbar ist keines von beiden. Sie ist damit
der schwächste Teil des Vorhabens und wäre ein vertretbarer Streichkandidat, wenn
der Umfang zu begrenzen ist. `LivingDead` ist davon unberührt: Dort ist der
Auslöser — Gesundheit auf 0 — am Statusübergang beobachtbar.

Stufe 3 — verbrauchte oGCDs und MP — kommt in dieser Bedingung **nicht** vor. Sie
darf keine Rückhaltung begründen, sondern wirkt allein über die Herabstufung aus
Schritt 2, die ohnehin verhindert, dass Ressourcen auf ein geschütztes Ziel gehen,
solange ein anderes sie braucht.

## Audit des Umsetzungskonzepts

Selbstprüfung, kein Vier-Augen-Prinzip — der erreichte Prüfgrad ist statische
Selbstkontrolle. Die erste Fassung dieses Audits prüfte auf Lauffähigkeit und
Vollständigkeit; diese zweite prüft zusätzlich gegen die Rangordnung.

| Prüffrage | Ergebnis |
|---|---|
| Ist Schritt 1 für sich lauffähig? | Nein. Sobald die Liste normale Ziele enthält, fallen die geschützten heraus — der heutige Zufallszustand kippt in den entgegengesetzten. Schritt 1 und 2 gehören in **einen** Eingriff |
| Kann Schritt 2 ohne Schritt 1? | Nein; er sortierte die falsche Menge |
| Stellt eine Regel Stufe 2 über Stufe 1? | **Behoben.** Alle drei Rückhaltefälle standen ohne Überlebensprüfung da; sie ist jetzt erste Bedingung, nicht Randausnahme |
| Stellt eine Regel Stufe 3 über Stufe 1 oder 2? | Nein — Stufe 3 begründet keine Rückhaltung mehr, sondern wirkt nur über die Herabstufung |
| Ist der Living-Dead-Fall vollständig? | **Nur konservativ.** Ob die Heilkapazität für Phase 2 reicht, ist nicht exakt bestimmbar; die Ersatzbedingung (MP-Schwelle, keine weiteren Verletzten) ist gröber als die Frage. Das ist im Zweifel zu wenig Rückhaltung — die sichere Richtung |
| Deckt Schritt 3 Fall 10 ab? | **Gegenstandslos.** Fall 10 ist nach der Artefaktprüfung kein Rückhaltefall mehr |
| Wirkungsbereich benannt? | Ja: Die Liste wirkt nur auf Ziele in der Tankrolle |
| Was passiert bei zwei Tanks? | **Neue Lücke.** Die Prüfung gilt je Ziel — richtig, aber die MP-Ersatzbedingung ist global und würde die Rückhaltung für beide gleichzeitig aufheben. Konservativ und damit hinnehmbar. *Nach dem fünften Audit sitzt diese Bedingung in `CanSustainPhaseTwo`, nicht mehr in `TankSurvivesWithoutMe`; die Lücke bleibt dieselbe* |

## Drittes Audit — gegen die Artefaktprüfung

Anlass war die Abarbeitung der drei Punkte, die das zweite Audit als ungeprüft
stehen ließ. Sie hat mehr widerlegt als bestätigt. Prüfgrad weiterhin: statische
Selbstprüfung an Repository-Artefakten (`Status.resx`, `ActionId.resx`,
`StatusHelper.cs`, `ActionTargetInfo.cs`, `ObjectHelper.cs`) plus eine Websuche zur
Schildmechanik; keine Laufzeitbeobachtung, kein Vier-Augen-Prinzip.

| Prüffrage | Ergebnis |
|---|---|
| Bildet `ClarityOfCorundum` den Catharsis-Auslöser ab? | **Nein, und der Fall entfällt ganz.** Falscher Bezeichner (der Auslöser ist `CatharsisOfCorundum`), und der Auslöser ist nicht raubbar, weil er auch bei Ablauf feuert |
| Wie teilen zwei gleichzeitige Schilde ihren Verbrauch auf? | **Nicht beantwortbar und nicht beobachtbar.** Ein `ShieldPercentage`-Wert je Charakter. Der frühere Schluss „gilt in jedem Aufteilungsmodell" war unbelegt und ist zurückgenommen |
| Welche Holmgang-Id setzt das Spiel? | **409, bereits im Code.** Die Forderung nach drei weiteren Ids war ein Nullbefund über den bloßen Namen |
| Hat die Prüfung eigene Vorbefunde widerlegt? | **Ja, drei:** der Gunbreaker-Rückhaltefall, die Schild-Aufteilungsaussage und die Holmgang-Lückenmeldung. Zusätzlich trug das Paladin-Beispiel in der Falsifikation nicht |
| Hat sie Neues gefunden? | **Ja, drei:** `SCH_Reborn.cs:830` als zweiter Fundort derselben Invertierung, die fehlende `UndeadRebirth`-Id, und dass der Zielwahl-Defekt die Priorisierung im Regelfall vollständig ausschaltet |
| Ändert sich dadurch die gewählte Option? | **Die Gewichtung ja, die Wahl nein.** O1 wird schwerer (Rangstufe 1 statt 3), O2 leichter (nur noch `LivingDead` belastbar, `BlackestNight` unbelegt). Die Reihenfolge „erst O1, dann O2" wird dadurch bestätigt, nicht in Frage gestellt |
| Wurde eine Aussage geglättet, statt sie zurückzunehmen? | Zu prüfen war das für die Schild-Aussage. Sie ist ausdrücklich zurückgenommen und der Widerspruch benannt, nicht durch Umformulierung getilgt |

## Viertes Audit — gegen die Spielpraxis

Anlass waren drei Einwände des Auftraggebers, die sich nicht am Code, sondern an der
Begegnungswirklichkeit stoßen. Sie haben die Konstruktion stärker verändert als die
Artefaktprüfung.

| Prüffrage | Ergebnis |
|---|---|
| Fragt das Konzept nach dem Zustand oder nach der Zeit? | **Nach dem Zustand — und das war falsch.** Living Dead nützt nur, wenn der Tod *vor* Ablauf eintritt; Walking Dead nur, wenn die Heilung *bis* zum Ablauf reicht. Beide Male ist die Frage eine Rate, keine Statusabfrage |
| Konnte die bisherige Fassung „im letzten Augenblick retten"? | **Nein.** Sie kannte keinen Ablaufzeitpunkt als Auslöser, nur die Zwei-GCD-Freigabe, die aus einem anderen Grund existiert. Fall 1c war nicht abgedeckt |
| Kannte sie die Staffelung in Phase 2? | **Nein.** „ja, zwingend" verschenkt die billige Frühunterstützung und benennt nicht, wann der teure Eingriff fällig ist |
| Trägt `TankSurvivesWithoutMe` die ihm zugedachte Zusage? | **Nein.** Es prüft den Gesundheitsstand, aber der entscheidet nicht über einen Mehrfachbuster, einen Zielwechsel zwischen zwei Tanks oder einen Buster, der auch vollgeheilt tötet. Die Zusage „er überlebt ohne mich" ist aus den vorhandenen Größen nicht herstellbar |
| Folgt daraus, die Rückhaltung freizugeben? | **Nein**, und diese Gegenposition ist zu benennen: Dass Heilung manchmal nichts nützt, macht Zurückhaltung nicht besser, nur nicht schlechter. Welcher Fall vorliegt, ist vorab unbekannt. Die Folgerung ist strenger, nicht laxer — jedes Buster-Fenster hebt auf, HP-unabhängig |
| Ist die geforderte Hochrechnung überhaupt messbar? | **Heute nicht, aber baubar.** `RecordedHP` führt nur Gegner (`TargetUpdater.cs:513-535`), also liefert `GetTTK` auf ein Party-Mitglied `NaN`. Die Effektpakete fremder Heiler passieren beide Watcher-Filter. Beides sind fehlende Aufnehmer, keine fehlenden Quellen |
| Wurde die Grenze der Messung benannt statt geglättet? | Ja: 1 Hz ist für ein 10-s-Fenster zu grob, und ein Gesundheitsdelta ist ein Surrogat für kumulierte Heilung. Deshalb eigener Puffer und Paketauswertung statt Mitbenutzung |
| Was, wenn die Messung fehlt oder unsicher ist? | `TriggerStillReachable` liefert dann `false`, also **keine** Rückhaltung. Fehlende Messung darf nie Rückhaltung begründen — *im fünften Audit für Living Dead widerrufen: „nicht zurückhalten" heißt dort heilen und entwertet die Fähigkeit. Der Rückfall ist die Uhrregel. Für A-nichttödliche Auslöser gilt die Zeile weiter* |
| Ist der Aufwand noch verhältnismäßig? | **Offen, und ehrlich zu benennen.** Schritt 3 bedient einen Job in einer Fähigkeit, Schritt 0 kostet bei allen Nutzern. Das gehört in die Vorlage, nicht in eine Fußnote |

**Nicht geprüft, weil außerhalb der Artefaktlage:** die konkreten Trefferzahlen und
Schadenswerte der genannten Begegnungen (Unreal Shinryu, Arkh Monh). Sie sind als
Beleg dafür genommen, dass mehrfach einschlagende Buster existieren — das genügt für
die Konstruktion. Ein Nullbefund über den englischen Bossnamen wäre nach der
Sprachregel ohnehin kein Beleg gewesen.

## Fünftes Audit — die Aufhebungen waren für den Hauptfall verkehrt herum

Anlass war ein Einwand des Auftraggebers gegen die im vierten Durchgang eingeführte
Buster-Aufhebung. Er trifft, und er trifft mehr als nur diese eine Regel.

| Prüffrage | Ergebnis |
|---|---|
| Gilt die Buster-Aufhebung für alle Auslöser? | **Nein, und die Verallgemeinerung war der Fehler.** Bei Living Dead liefert der Tankbuster den Auslöser. Ihn abzufangen verhindert entweder nichts oder das Gewollte |
| Gilt die Gesundheitsschwelle für alle Auslöser? | **Ebenfalls nein**, aus demselben Grund. Niedrige Gesundheit ist in Phase 1 der erwünschte Zustand, keine Gefahrenmeldung. Diese Aufhebung stand seit dem dritten Durchgang darin und war die ganze Zeit falsch |
| Gibt es einen Fall, in dem Heilung in Phase 1 rettet **und** Phase 2 erhält? | **Nein.** Eine Heilung, die den Tod verhindert, verhindert per Konstruktion den Auslöser. Das ist keine Abwägung, sondern eine Identität — die Gegenhypothese ist nicht bloß schwach, sie ist nicht formulierbar |
| Was bleibt dann als Aufhebung in Phase 1? | Genau eine: **der Tod käme nicht mehr vor Phasenende.** Dazu die Vorbedingung aus 1b, die vor dem Eintritt liegt und keine laufende Aufhebung ist |
| Braucht diese Aufhebung den Messbaustein? | **Nein** — das war die zweite Fehleinschätzung des vierten Durchgangs. „Im letzten Augenblick retten" ist eine Uhrregel. Die Messung verkleinert nur den Restfehler |
| Welcher Restfehler bleibt? | Die Uhrregel kann in den letzten Sekunden einen Tod verhindern, der noch rechtzeitig gekommen wäre. Klein zu halten über einen knappen Vorlauf, also über ein Heilmittel **ohne Wirkzeit** |
| Ist die Fehlerrichtung vertretbar? | Ja, und das ist zu begründen statt zu behaupten: Ein verlorener Auslöser kostet einen Cooldown, ein zu spät geretteter Tank den Kampf. Die Uhrregel irrt in die billigere Richtung |
| War der frühere Rückfall „ohne Messung nie zurückhalten" richtig? | **Nein.** Für Living Dead heißt „nicht zurückhalten" heilen, und Heilung entwertet die Fähigkeit. Der Rückfall ist die Uhrregel, nicht die Freigabe. Die Regel „fehlende Messung darf nie Rückhaltung begründen" gilt weiter für A-nichttödliche Auslöser, wo sie herkam |
| Bleibt die Kapazitätsprüfung laufend oder einmalig? | **Einmalig vor dem Eintritt.** Eine laufende Prüfung könnte mitten in der Phase kippen und den Weg auf halber Strecke abbrechen — das wäre der schlechteste aller Ausgänge: kein Auslöser und trotzdem eine ausgegebene Heilung |

## Verbesserung nach dem zweiten Audit

**Schritt 1 und 2 bleiben zusammengelegt** — Ergebnis des ersten Audits, bestätigt.

**Die Überlebensprüfung wird zur ersten Bedingung** statt zur Ausnahme. Das ist die
Beseitigung des Hauptmissstands: Ohne sie hätte das Konzept in drei von siebzehn
Lagen einen Effekt der Stufe 2 über das Überleben gestellt.

**Stufe 3 verliert jede eigenständige Wirkung.** Ressourcenschonung darf nichts
zurückhalten; sie ergibt sich als Nebenwirkung der Herabstufung.

**Ergebnis nach dem fünften Audit: vier Schritte, nach Beleglage geordnet.**

1. **Excogitation-Invertierung** (`SCH_Reborn.cs:830`). Belegte Defektbehebung,
   kleiner Blast Radius: eine Rotation, eine Fähigkeit. Ohne Option. Für sich
   lauffähig und unabhängig von den übrigen Schritten — deshalb zuerst.
2. **Zielwahl richten und herabstufen** (`ActionTargetInfo.cs:3536` plus Umbau vom
   Filter zur Prioritätsstufe, in **einem** Eingriff). Belegte Defektbehebung,
   Rangstufe 1, aber großer Blast Radius: die generische Heilzielwahl aller Jobs.
   Ohne Option — ein Schalter würde den fehlerhaften Zustand konservieren. Danach
   die fehlenden Ids ergänzen: `HallowedGround`, `HallowedGround_1302`,
   `UndeadRebirth`. Keine weiteren Holmgang-Ids.
3. **Auslöser-Rückhaltung**, hinter einer Option mit Standard aus, gebunden an die
   Tankrolle — **mit zwei getrennten Bedingungen je Auslöserklasse**. Für Living Dead
   (Auslöser = Tod): Uhrregel mit knappem Vorlauf, vorzugsweise über eine Fähigkeit
   ohne Wirkzeit; **keine** Aufhebung durch niedrige Gesundheit oder Tankbuster;
   Vorbedingung ist allein die Kapazität für Phase 2. Für The Blackest Night (Auslöser
   unterhalb des Todes): Buster-Fenster und Gesundheitsschwelle heben auf.
   Dazu die gestaffelte Phase-2-Unterstützung — leicht am Anfang, voll bei nicht
   tragendem Kurs. `LivingDead` ist belegt und trägt; `BlackestNight` ruht auf
   unbelegter Mechanik und ist ein vertretbarer Streichkandidat.
   `CatharsisOfCorundum` und `Excogitation` sind ausgeschieden.

4. **Messbaustein** (Schritt 0 dieses Dokuments), nachrangig. Verkleinert den
   verbleibenden Fehler der Uhrregel und erlaubt Heilmittel mit Wirkzeit. Kosten
   fallen bei allen Nutzern an, Nutzen zunächst nur beim Dunkelritter — als letzter
   Schritt getrennt zu entscheiden.

Die Umstellung gegenüber der zweiten Fassung ist nicht kosmetisch: Was dort als
Kern galt — die Rückhaltung —, ist nach der Artefaktprüfung der schwächste Teil,
und was dort als Nebenschritt geführt wurde, betrifft das Überleben der Gruppe.

**Der vierte Durchgang hatte den Aufwand von Schritt 3 zu hoch angesetzt, der fünfte
korrigiert das nach unten.** Die Kernanforderung ist eine Uhrregel — zurückhalten,
bis die Restzeit den Vorlauf des Heilmittels unterschreitet — und die kommt ohne
Messbaustein aus. Was bleibt, ist eine Statusprüfung, eine Restzeitprüfung und eine
Vorbedingung; der Messbaustein rutscht ans Ende und wird getrennt entschieden.

**Zugleich hat der fünfte Durchgang die Konstruktion an einer Stelle grundlegend
geändert:** Es gibt nicht eine Rückhaltebedingung, sondern zwei — und was in der
einen aufhebt, darf in der anderen gerade nicht aufheben. Diese Trennung nach der
Art des Auslösers fehlte in allen vorigen Fassungen und hat dort zu einer Regel
geführt, die dem Dunkelritter genau das genommen hätte, wofür er seine Fähigkeit
zündet.

## Nachweisbarkeit

| Ebene | Möglich | Nicht möglich |
|---|---|---|
| Statisch | Prüfskript: Liest jede Aufrufstelle von `NoNeedHealingInvuln` und meldet, wenn zwei Aufrufer dieselbe Funktion mit entgegengesetztem Vorzeichen auswerten — genau die Defektklasse, die hier gefunden wurde | — |
| Kompilierung | CI | — |
| Laufzeit | Beobachtung, ob ein Dunkelritter unter Living Dead noch geheilt wird | Beweis, dass die neue Rangfolge besser spielt |

Weil der Nutzen von Schritt 2 nicht belegbar ist, steht er hinter einer Option.
Schritt 1 ist eine Defektbehebung und bekommt keine — ein Schalter würde den
fehlerhaften Zustand konservieren.

## Konsequenzen

**Endnutzer.** Nach Schritt 2 ändert sich die Heilzielwahl aller Jobs spürbar, weil
die Rangfolge erstmals greift. Konkret: Der am schwersten verletzte DPS wird
überhaupt erst wählbar, wo heute regelmäßig der Tank gewählt wird. Das ist
beabsichtigt und behebt einen Defekt der Rangstufe 1, bleibt aber die größte
Verhaltensänderung dieses Vorhabens — und die einzige, für die eine
Laufzeitbeobachtung wirklich wünschenswert wäre.

**Autoren abgeleiteter Rotationen.** `NoNeedHealingStatus` bleibt öffentlich; die
neue Liste kommt additiv hinzu. Die Aufnahme von `HallowedGround`,
`HallowedGround_1302` und `UndeadRebirth` ändert allerdings das Verhalten *jeder*
abgeleiteten Rotation, die die Liste liest — nach Schritt 2 nur noch in der
Reihenfolge, davor im Ob. Deshalb gehört die Ergänzung zwingend hinter den Umbau,
nicht davor.

Wer die Prüfung selbst aufruft — `BeirutaWHM`, `BeirutaSCH`, `BeirutaAST`,
`BeirutaSGE` — behält sein Verhalten, folgt damit aber weiter der alten
Konstruktion. Nebenbefund derselben Erhebung, nicht Teil dieses Vorhabens: Die
Beiruta-Rotationen prüfen `StatusID.Holmgang` (88), also den Bewegungs-Debuff auf
dem *Ziel*, statt `Holmgang_409` auf dem Krieger. Diese Prüfungen greifen nie. In
`TODO.md` zu erfassen.

**Upstream.** Der Eingriff liegt in `ActionTargetInfo` und `StatusHelper`, beide
mit regelmäßiger Upstream-Aktivität.

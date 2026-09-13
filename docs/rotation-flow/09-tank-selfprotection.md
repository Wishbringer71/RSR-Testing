# 09 · Tank-Selbstschutz und das Verhalten des Heilers

Entwurfsdokument nach ADR-Struktur. Es stellt den geltenden Stand dar; die
Prüfhistorie steht in `AUDIT_LOG.md` (A21–A23, A27–A29), zurückgenommene Aussagen
dort in Abschnitt C.

## Ergebnis

Mehrere Tank-Fähigkeiten warten auf ein **Ereignis**, das fremde Heilung abfangen
kann. RSR kannte davon eine einzige Wechselwirkung und setzte sie fehlerhaft um.

Der belastbare Kern des Vorhabens ist **nicht** die Rückhaltung, sondern die
Richtigstellung der Heilzielwahl: `NoNeedHealingInvuln()` liefert `true`, wenn
*kein* Schutzstatus liegt, und zwei Aufrufer werteten sie mit dem entgegengesetzten
Vorzeichen aus. Die generische Zielwahl sammelte dadurch genau die geschützten
Gruppenmitglieder, war im Regelfall leer und fiel auf den Tank zurück — sie war
blind für den am schwersten Verletzten. Das ist ein Defekt der Rangstufe 1 und
umgesetzt.

Die Rückhaltung selbst ist ein eng umgrenzter Sonderfall: Von neunzehn geprüften
Lagen bleiben **zwei** echte Rückhaltefälle, denen fünf Aufhebungen gegenüberstehen.
Sie ist für Living Dead als **Uhrregel** umgesetzt — zurückhalten, solange der Tod
noch rechtzeitig kommt — hinter einer Option mit Standard aus.

Für The Blackest Night ist **keine eigene Rückhalteregel** richtig: Heilung berührt den
Auslöser nicht, und ein Heilerschild wird erst nach der TBN-Barriere aufgezehrt, kann sie
also weder verzögern noch verdrängen. Was RSR stattdessen tut, ist allerdings nicht die
Nachrangigkeit, als die dieses Konzept es zunächst geführt hat: `BlackestNight` steht in
`StatusHelper.ShieldStatus`, und die Anrechnung über `GetEffectiveHpPercent` hebt die
Gesundheitsquote des Trägers — sie verschiebt damit nicht seinen **Rang** unter den
Heilzielen, sondern die **Schwelle**, ab der überhaupt geheilt wird. Bei einer Barriere
über 25 % der maximalen HP sind das 25 Prozentpunkte: Die oGCD-Heilung setzt erst bei
real rund 40 % ein statt bei 65 %. Ob das richtig bemessen ist, ist offen und steht in
`TODO.md`; entschieden ist hier nur, dass eine **zusätzliche** TBN-Regel nichts beiträgt.

*Abgrenzung, weil dieser Satz sonst zu weit gelesen wird:* Er gilt für Heilung und
Schild. Für den **Schadensstrom** gilt das Gegenteil, und dort liegt inzwischen eine
Sonderregel — der Weißmagier hält Sanctus zurück, solange ein Tank die Barriere trägt,
weil dessen Betäubung genau den Schaden anhält, an dem die Barriere aufgezehrt werden
müsste. Das ist keine Frage der Heilzielwahl und steht deshalb in Konzept 10, nicht
hier.

| Baustein | Stand |
|---|---|
| Invertierte Prüfung an beiden Fundorten korrigiert | umgesetzt |
| Zielwahl vom Ausschluss zur Prioritätsstufe | umgesetzt |
| Fehlende Ids (`HallowedGround`, `HallowedGround_1302`, `UndeadRebirth`) | umgesetzt |
| Schutzstatus senkt die Heilschwelle, statt das Flag zu unterdrücken | umgesetzt |
| Living-Dead-Rückhaltung als Uhrregel, hinter Option | umgesetzt |
| Sonderbehandlung für The Blackest Night | **nicht nötig** — die vorhandene Schildanrechnung erfasst den Fall |
| Bemessung dieser Anrechnung | **offen** — sie senkt die Schwelle um den vollen Barrierenwert, nicht nur den Rang (`TODO.md`) |
| Messbaustein für Heilraten auf Gruppenmitglieder | verworfen, kein Verbraucher |

## Prüfmaßstab — die Rangordnung

Alles Folgende ist an dieser Ordnung zu messen. Sie ist **lexikographisch**: Eine
nachrangige Stufe darf eine vorrangige nie aufwiegen, gleich wie groß ihr Betrag
wäre.

1. **Das Überleben des Tanks.**
2. **Die Mitnahme positiver Effekte** — Dark Arts, Catharsis, die Selbstheilung von
   Walking Dead.
3. **Die Vermeidung unnötiger Aktionen** — verbrauchte oGCDs, MP, Cooldowns.

Daraus folgt die tragende Vorbedingung: **Jede Rückhaltung wird erst geprüft, wenn
Stufe 1 gesichert ist.** Eine Rückhaltung, die einen Effekt der Stufe 2 sichern
will, ohne vorher Stufe 1 zu prüfen, ist die verbotene Aufwiegung. Am schwersten
bei Living Dead: Der Verzicht auf Heilung führt planmäßig in Walking Dead, wo das
Überleben an kumulierter Heilung in Höhe der **vollen maximalen HP** hängt. Kann
der Heiler die nicht aufbringen, tauscht der Verzicht sicheres gegen unsicheres
Überleben.

Eine Lesart bleibt bewusst ausgeklammert: „Überleben des Tanks" gilt hier als
Vorrang *innerhalb* der Frage, ob eine Tank-Schutzmechanik respektiert wird — nicht
als genereller Vorrang des Tanks vor der Gruppe. Für diesen anderen Fall führt RSR
bereits eine eigene Rangfolge (Selbst → Heiler → Tank → niedrigste Gesundheit,
`ActionTargetInfo.cs:3566-3583`). Sie hier ebenfalls umzustellen wäre eine zweite,
größere Änderung.

## Taxonomie nach Auslöser

Entscheidend ist nicht, ob eine Fähigkeit schützt, sondern **ob sie auf ein Ereignis
wartet**, das der Heiler abfangen kann.

### Klasse A — auslöserbehaftet

| Job | Fähigkeit | Auslöser | Auslöser ist… | Was fremde Heilung bewirkt |
|---|---|---|---|---|
| DRK | Living Dead | HP fallen auf 0 | **der Tod selbst** | verhindert den Tod, also den Übergang in Walking Dead samt dessen Selbstheilung |
| DRK | The Blackest Night | Barriere wird **vollständig** absorbiert | ein Schadensereignis unterhalb des Todes | **nichts.** Eine Barriere absorbiert vor den HP; ihr Verbrauch hängt am eingehenden Schaden, nicht am Gesundheitsstand. Heilung wirkt hier sogar *für* den Auslöser, weil sie den Tod verhindert, der ihn vereiteln würde |
| SCH | Excogitation | HP-Schwelle auf dem Ziel | ein Schadensereignis unterhalb des Todes | vorzeitige Heilung entwertet die eigene, bereits gesetzte Fähigkeit |

**Die vierte Spalte trennt die Klasse in zwei, und diese Trennung entscheidet über
alles Weitere.**

**A-tödlich — der Auslöser *ist* der Tod.** Living Dead ist der einzige Fall. Das
Ereignis, das der Heiler sonst um jeden Preis verhindert, ist hier das erwünschte.
Jede sonst geltende Regel — „bei niedriger Gesundheit heilen", „bei drohendem
Tankbuster heilen" — ist hier nicht bloß unnötig, sondern **kontraproduktiv**: Sie
zielt auf die Verhinderung dessen, worauf die Fähigkeit wartet.

**A-nichttödlich — der Auslöser liegt unterhalb des Todes.** The Blackest Night und
Excogitation. Der Tod bleibt die Katastrophe; die üblichen Aufhebungsregeln gelten
unverändert.

### Klasse A+ — The Blackest Night: eine Frage der Werkzeugwahl, nicht der Rückhaltung

Die Fähigkeit kostet **3000 MP** und legt eine Barriere über 25 % der maximalen HP
für 7 Sekunden. Dark Arts — und damit ein kostenloser Edge oder Flood of Shadow —
wird nur gewährt, wenn die Barriere **vollständig** absorbiert wird. Wird sie es
nicht, sind die 3000 MP ersatzlos ausgegeben: Edge of Shadow kostet seinerseits
3000 MP und verlängert Darkside um 30 Sekunden, deren Abriss zehn Prozent Schaden
kostet.

**Heilung und HoT berühren diesen Auslöser nicht.** Eine Barriere absorbiert
eingehenden Schaden, bevor er die HP erreicht; wie schnell sie aufgezehrt wird, hängt
allein am eingehenden Schaden. Der Gesundheitsstand des Trägers ändert daran nichts.
Die einzige Kopplung läuft in die **andere** Richtung: Stirbt der Träger, bevor die
Barriere aufgebraucht ist, entfällt Dark Arts — Heilung wirkt also für den Auslöser,
nicht gegen ihn. *Schluss aus der Wirkbeschreibung („nullifying damage") und der
Auslösebedingung („completely absorbed"), nicht wörtlich belegt.*

**Ein zweiter Schild schadet ebenfalls nicht.** Mehrere Barrieren auf demselben
Charakter werden in einer festen **Verbrauchsreihenfolge** aufgezehrt, nicht anteilig,
und The Blackest Night steht in dieser Reihenfolge **vor** den Schilden, die ein Heiler
auf einen Tank legen kann:

| Barriere | Verbrauchsrang |
|---|---|
| The Blackest Night | 3 |
| Eukrasian Diagnosis (SGE) | 4 |
| Divine Benison (WHM) | 12 |

Der niedrigere Rang wird zuerst aufgezehrt. Ein Heilerschild verzögert die Absorption
von TBN also nicht — Dark Arts wird davon nicht berührt. Er geht dabei auch nicht
verloren: Er bleibt liegen und absorbiert, sobald TBN aufgebraucht ist. Zurückstellen
spart deshalb nichts, es verschiebt nur.

*Quellenstatus:* Spielerdokumentation (eine nummerierte Prioritätsliste in einem
Lodestone-Blog, wiedergegeben über die Suche), keine offizielle Beschreibung. Die
Primärseite selbst und das Consolegames-Wiki sind vom Egress dieser Umgebung nach
Organisationsrichtlinie gesperrt; das ist keine Fehlkonfiguration und nicht zu
umgehen. **Ein Quellenkonflikt bleibt offen:** Ein Job-Guide führt Eukrasian Diagnosis
als vorrangig gegenüber TBN, die Liste ordnet sie dahinter. Betroffen wäre allein der
Weise. Für Weißmagier, Gelehrten und Astrologen sagen beide Quellen dasselbe, weil
deren Schilde deutlich hinter TBN liegen.

**Der Träger ist nicht zwingend der Dunkelritter.** `ActionId.resx` (Aktion 7393)
beschreibt TBN als „Creates a barrier around **self or target party member**" — die
Barriere kann auf jedem Gruppenmitglied liegen, und der Party-Zweig in `DRK_Reborn` nutzt das mit
`targetOverride: TargetType.LowHP`. Damit ist die ebenfalls genannte Radiant Aegis
**nicht** gegenstandslos: Sie ist zwar ein Selbstschild des Beschwörers
(`Status.resx`: **(SMN)**), aber ein Beschwörer kann sie tragen **und** zusätzlich TBN
vom Dunkelritter bekommen. Steht sie in der Reihenfolge vor TBN, verzögert sie dessen
Absorption. Das ist ein realer Fall — nur keiner, den ein Heiler beeinflussen kann,
denn Radiant Aegis wirft der Beschwörer selbst.

**Damit bleibt kein Grund, den Schild zurückzustellen.** Was bleibt, ist die gewöhnliche
Dringlichkeitsfrage: Ein Träger mit TBN ist bereits geschützt und deshalb weniger dringend
zu versorgen als ein ungeschütztes Gruppenmitglied. Eine eigene TBN-Regel fügt dem nichts
hinzu, denn `StatusID.BlackestNight` steht in `StatusHelper.ShieldStatus` und geht über
`GetEffectiveHpPercent` in die Heilentscheidung ein.

**Womit dieser Mechanismus allerdings nicht das tut, was der Absatz von ihm verlangt.**
Die Dringlichkeitsfrage ist eine Frage des Rangs — wer von mehreren Verwundeten zuerst
versorgt wird. Die Anrechnung hebt dagegen die Gesundheitsquote und verschiebt damit die
**Schwelle**, ab der überhaupt geheilt wird; sie wirkt auch dann, wenn der Träger der
einzige Verwundete ist und es gar nichts zu priorisieren gibt. Das ist derselbe
Kategorienfehler, den dieses Projekt bei `HasHostileCountAoeMitigation` schon einmal
gemacht hat: Ein Mechanismus wurde an seinem Geltungsbereich beurteilt statt an dem, was
er auslöst. Die Anrechnung ist deshalb hier nicht mehr als erledigt geführt, sondern als
offene Bemessungsfrage in `TODO.md` — einschließlich des Falls, für den sie am
schlechtesten gebaut ist: eine Barriere, die zu spät oder unnötig gesetzt wurde, wird
voll angerechnet, ohne je Schaden abzufangen.

### Klasse B — Unverwundbarkeit

Heilung während der Phase wirkt nicht auf den Schutz, wohl aber auf das, was danach
kommt.

| Job | Fähigkeit | Wirkung auf die HP | Lage nach Ablauf |
|---|---|---|---|
| PLD | Hallowed Ground | unverändert | so niedrig wie beim Zünden |
| WAR | Holmgang | fallen nicht unter 1 | möglicherweise 1 HP |
| GNB | Superbolide | **sofort auf 1** | 1 HP, sofern nicht geheilt wurde |
| DRK | Undead Rebirth | fallen nicht unter 1 | Erfolgszustand |

**Living Dead hat drei Phasen.** Die Aktionsbeschreibung im Repository
(`ActionId.resx`, Aktion 3638) ist die Primärquelle:

1. **Living Dead**, 10 s. Fällt die Gesundheit auf 0, tritt kein Tod ein, sondern
   der Wechsel nach Walking Dead. Die Selbstheilung — 1500 Potenz je getroffenem
   Waffenskill oder gewirktem Zauber — hängt bereits an *dieser* Phase.
2. **Walking Dead**, 10 s. Angriffe drücken die Gesundheit nicht unter 1. Wird bis
   zum Ablauf kumuliert Heilung in Höhe der **maximalen** Gesundheit aufgenommen,
   folgt Phase 3; sonst tritt der Tod ein. Die Quelle sagt „an amount of HP totaling
   your maximum HP is restored" ohne Einschränkung auf die Quelle — fremde Heilung
   zählt mit, und bei heutigen Tank-Gesundheitswerten trägt die Selbstheilung davon
   nur einen Bruchteil.
3. **Undead Rebirth**, Restlaufzeit von Walking Dead. Der Dunkelritter kann nicht
   sterben, und der Auslöser ist eingelöst. Der einzige Zustand der ganzen Taxonomie,
   in dem Zurückhaltung weder Stufe 1 noch Stufe 2 berührt, sondern rein Stufe 3
   bedient.

### Klasse C — reine Schadensreduktion

Rampart, Sentinel, Shadow Wall, Nebula, Sheltron und Holy Sheltron, Bulwark,
Camouflage, Oblation, Heart of Corundum und Clarity of Corundum (die
Reduktionsanteile), Reprisal. Keine Wechselwirkung.

### Klasse D — Selbstheilung ohne Auslöser

Bloodwhetting und Raw Intuition, Aurora, Clemency, Nascent Flash. Fremde Heilung
addiert sich, verhindert aber nichts.

## Living Dead ist ein Zeitproblem, kein Zustandsproblem

Die Frage lautet nicht *liegt der Status?*, sondern in beiden Phasen eine über
**Rate und Restzeit**.

**Phase 1 — reicht der eingehende Schaden, um vor Ablauf auf 0 zu kommen?** Living
Dead nützt nur, wenn der Tod **innerhalb** der zehn Sekunden eintritt. Zeichnet sich
ab, dass der Tod erst nach Ablauf käme, war die Rückhaltung nicht nur nutzlos — sie
hat den Tank zehn Sekunden ungeheilt gelassen, der nun mit niedriger Gesundheit ohne
Schutz dasteht. Dann ist **im letzten Augenblick** zu handeln, ausreichend und nicht
symbolisch.

Zurückgehalten wird also nicht, *weil* der Status liegt, sondern solange der Tod
noch rechtzeitig kommt.

**Phase 2 — heilt er sich schnell genug selbst?** Walking Dead verlangt kumuliert
eine volle Maximalgesundheit in zehn Sekunden. Die Selbstheilung liefert 1500 Potenz
je Waffenskill oder Zauber, bei rund 2,4 s GCD also etwa vier Auslösungen. Daraus
folgt ein gestaffeltes Verhalten statt eines Schalters:

| Zeitpunkt in Phase 2 | Verhalten | Begründung |
|---|---|---|
| Anfang | **leicht unterstützen** — Regeneration, ein günstiges HoT | Der Beitrag zählt voll gegen die geforderte Summe, kostet aber wenig |
| Mitte | Kurs prüfen, weiter leicht unterstützen | Solange der Kurs trägt, ist ein großer Zauber vergeudet |
| Kurs reicht nicht | **eingreifen, in voller Höhe** | Die Alternative ist der Tod am Phasenende |
| Kurs trägt bis zum Ende | nichts weiter | Der Rest ist Überheilung |

### Die Aufhebungen kehren sich für Living Dead um

| Aufhebung | Bei A-nichttödlich | Bei Living Dead Phase 1 |
|---|---|---|
| Niedrige Gesundheit | hebt auf — der Tank droht zu sterben | **hebt nicht auf** — niedrige Gesundheit ist der erwünschte Zustand, sie bringt den Auslöser näher |
| Tankbuster-Fenster | hebt auf — die Sequenz ist nicht beurteilbar | **hebt nicht auf** — der Buster liefert den Auslöser |

Eine Vollheilung bei zehn Prozent Gesundheit in Phase 1 geht auf zwei Weisen daneben,
und beide treffen zu: **Sie rettet nicht**, wenn der Buster auch vollgeheilt tötet;
und **sie schadet**, wenn sie rettet, weil sie dann den Übergang in Phase 2 verhindert
und die Fähigkeit entwertet, die der Tank für genau diesen Moment gezündet hat. Einen
dritten Ausgang gibt es nicht: Eine Heilung, die den Tod in Phase 1 verhindert,
verhindert per Konstruktion den Auslöser. Das ist keine Abwägung, sondern eine
Identität.

**Für Phase 1 bleibt damit genau eine Aufhebung** — und sie hat nichts mit Gefahr zu
tun, sondern mit der Uhr:

> Die Rückhaltung wird aufgehoben, **wenn der Tod nicht mehr vor dem Ende der ersten
> Phase eintreten würde.**

Dazu die Vorbedingung aus Fall 1b, die vor der Phase liegt: Wer die Heilmenge für
Phase 2 gar nicht aufbringen kann, darf den Weg über den Tod nicht einschlagen.

### Warum eine Überlebensprüfung diese Zusage nicht tragen kann

Eine Prüfung der Form „erst feststellen, ob der Tank ohne mich überlebt, dann
zurückhalten" ist aus den vorhandenen Größen **nicht herstellbar**:

- **Tankbuster wechseln das Ziel mitten in der Sequenz.** `DataCenter.BMRNextTankbusterIn`
  ist eine einzige Zahl ohne Angabe, auf wen; `IsHostileCastingTankBusterAtMe` ist
  ausdrücklich spielerzentriert (`DataCenter.IsHostileCastingTankBusterAtMe`).
- **Ein Tankbuster ist nicht ein Einschlag.** Mehrfach einschlagende Buster sind eine
  Folge von Treffern; die Vorhersage nennt den Beginn, nicht die Anzahl und nicht die
  Gesamtsumme.
- **Manche Buster töten auch einen vollgeheilten und geschildeten Tank.** Die
  stillschweigende Annahme „wenn ich rechtzeitig heile, überlebt er" trifft dort nicht
  zu.

Daraus folgt **nicht**, die Rückhaltung freizugeben. Dass Heilung manchmal nichts
nützt, macht Zurückhaltung nicht besser, nur nicht schlechter — und welcher Fall
vorliegt, ist vorab nicht erkennbar. Die Folgerung ist strenger: Für
**A-nichttödliche** Auslöser hebt **jedes** erkannte Tankbuster-Fenster die
Rückhaltung auf, HP-unabhängig, weil die Prognose dort unmöglich ist. Die
HP-Schwelle bleibt als zweite, unabhängige Aufhebung: sie fängt den Dauerschaden ab,
das Buster-Fenster den Einzelschlag.

## Fallvarianten

Vollständig über die Dimensionen Fähigkeitsklasse × Gesundheitsstand × Heileraktion.
„Richtig" meint die Handlung, die dem Vorrang der Überlebenssicherung folgt.

| # | Lage | Heilung richtig? | Schild richtig? | Begründung |
|---|---|---|---|---|
| 1 | DRK, Living Dead aktiv, **Tod tritt vor Ablauf ein**, Heilkapazität für Phase 2 gesichert | **nein** | **nein** | Stufe 1 ist gesichert, also darf Stufe 2 entscheiden |
| 1b | dieselbe Lage, **Heilkapazität nicht gesichert** | **ja** | ja | Stufe 1 schlägt Stufe 2: sicheres gegen unsicheres Überleben zu tauschen ist verboten |
| 1c | DRK, Living Dead aktiv, **Tod würde nicht mehr vor Phasenende eintreten** | **ja, rechtzeitig vor Ablauf** | ja | Die **einzige** Aufhebung in Phase 1 |
| 1d | DRK, Living Dead aktiv, **Tankbuster-Fenster erkannt** | **nein** | **nein** | Der Buster liefert den Auslöser |
| 1e | DRK, Living Dead aktiv, **HP sehr niedrig**, Tod noch vor Ablauf zu erwarten | **nein** | **nein** | Niedrige Gesundheit ist hier erwünschter Zustand, keine Gefahrenmeldung |
| 2 | DRK, Living Dead aktiv, HP niedrig, Ablauf fern | wie 1 / 1b / 1e | dito | Der Gesundheitsstand entscheidet in dieser Phase nicht |
| 3 | DRK, Living Dead läuft ab, Tod nicht eingetreten | **ja, dringend** | ja | Ausführungsform von 1c: mit Vorlauf, damit die Heilung vor dem Ablauf wirkt |
| 4 | DRK, Walking Dead aktiv, **Kurs trägt** | leicht unterstützen | nein, wirkungslos bei 1 HP | Billige Beiträge zählen voll gegen die Summe |
| 4a | DRK, Walking Dead aktiv, **Kurs reicht nicht** | **ja, in voller Höhe** | nein | Die Alternative ist der Tod am Phasenende |
| 4b | DRK, **Undead Rebirth** aktiv | nein, nachrangig | nein | Bedingung erfüllt, reine Stufe 3 |
| 5 | DRK, TBN aktiv | **nach normaler Regel** | **nach normaler Regel** | **Kein Sonderfall.** Heilung berührt den Auslöser nicht, und ein Heilerschild wird erst nach TBN aufgezehrt. Die angerechnete Barriere macht den Träger über `GetEffectiveHpPercent` ohnehin nachrangig |
| 6 | GNB, Superbolide aktiv | **ja** | ja | HP stehen auf 1; das Fenster ist die einzige gefahrlose Gelegenheit |
| 7 | WAR, Holmgang aktiv, HP heruntergedrückt | **ja** | ja | wie 6 |
| 8 | PLD, Hallowed Ground aktiv, beim Zünden wenig HP | **ja** | ja | Die HP bleiben unverändert; nach Ablauf steht er, wo er stand |
| 9 | PLD, Hallowed Ground aktiv, beim Zünden viel HP | nein, nachrangig | nein | Einziger Fall echter Entbehrlichkeit in Klasse B |
| 10 | GNB, Heart of Corundum liegt | **nach normaler Regel** | nach normaler Regel | **Kein Rückhaltefall** — der Catharsis-Stoß löst auch bei Ablauf aus |
| 11 | GNB, Catharsis ausgelöst | **ja** | ja | Der Auslöser ist verbraucht |
| 12 | Klasse C aktiv | nach normaler Regel | nach normaler Regel | keine Wechselwirkung |
| 13 | Klasse D aktiv | nachrangig | nach normaler Regel | Die Selbstheilung trägt einen Teil, ersetzt sie nicht |
| 14 | Mehrere brauchen Heilung, Tank geschützt | Tank **nachrangig**, andere zuerst | dito | Der Schutz verschiebt die Dringlichkeit, hebt den Bedarf nicht auf |
| 15 | Nur der geschützte Tank braucht Heilung | **ja** | ja | Das Fenster ist die sicherste Gelegenheit |

Zeile 14 und 15 sind der Kern: **Der Schutz verschiebt die Reihenfolge, er hebt den
Bedarf nicht auf.** Handeln schadet nur in den Zeilen 1 und 2 — und dort nur, solange
Stufe 1 gesichert ist. Von achtzehn Lagen bleibt damit **ein** echter Rückhaltefall
(1), dem vier Aufhebungen (1b, 1c, 1d, 4a) gegenüberstehen; einer ist reine
Ressourcenschonung ohne Risiko (4b). Eine Bedingung, die häufiger nicht gilt als gilt,
ist kein Leitmotiv: Der belastbare Kern bleibt die Richtigstellung der Zielwahl, und
der einzige verbliebene Sonderfall ist Living Dead.

## Was gebaut ist

### Die Zielwahl (Rangstufe 1)

`NoNeedHealingInvuln()` ruft `WillStatusEndGCD(2, 0, false, …)`. Die Kette
`WillStatusEnd` → `StatusTime` (beide in `StatusHelper.cs`) liefert bei
**fehlendem** Status die Zeit `0f`, und `(0 >= 0 || !HasStatus) && 0 <= time` ergibt
`true`. Der Rückgabewert bedeutet also: *kein schützender Status aktiv, Heilung ist
zuzulassen* — der Name sagt das Gegenteil, was die ganze Defektklasse erklärt.

`GeneralHealTarget` sammelte mit `if (!o.NoNeedHealingInvuln())` folglich genau die
Gruppenmitglieder mit **aktivem Schutz**. Im Regelfall blieb die Liste leer, alle
darauf aufbauenden Stufen liefen ins Leere, und der Aufrufer fiel auf den Träger der
Tank-Haltung zurück. Praktisch: **Die generische Heilzielwahl war blind für den am
schwersten verletzten Gruppenteil und wählte faktisch immer den Tank.** War die Liste
einmal gefüllt, wurde bevorzugt der Unverwundbare geheilt. Derselbe Fehler stand ein
zweites Mal in `SCH_Reborn.cs:830`, wo Excogitation nur auf gerade unverwundbare
Ziele ging.

Umgesetzt ist beides zusammen, denn getrennt wäre der erste Schritt schädlich: Sobald
die Liste normale Ziele enthält, fielen die geschützten heraus und der Zufallszustand
kippte in den entgegengesetzten. Geschützte Ziele sind seither **nachrangig, nicht
ausgeschlossen**, und die fehlenden Ids `HallowedGround`, `HallowedGround_1302` und
`UndeadRebirth` sind ergänzt — zwingend nach dem Umbau, weil sie davor das *Ob* und
danach nur noch die *Reihenfolge* ändern.

Im `StateUpdater` senkt ein Schutzstatus seither die Heilschwelle auf
`HealthProtectedRatio`, statt das Heilflag zu unterdrücken. Ohne diese Umstellung
erreicht die Herabstufung ihren Zweck nicht: Ohne Flag ruft der Dispatcher
`HealSingleGCD` gar nicht auf.

### Die Living-Dead-Rückhaltung

Sie liegt hinter `WithholdHealingForLivingDead` (Standard **aus**) und benutzt
`StatusHelper.InDeathTriggerWindow` auf einer eigenen Liste `DeathTriggeredStatus`,
die genau den einen Status führt, dessen Auslöser der eigene Tod ist. Die Paarung aus
Einstellung und Fenster steht als `ObjectHelper.IsHeldForDeathTrigger` an **einem**
Ort; die Bedingung selbst kommt im Baum kein zweites Mal vor.

**Ein Halt muss auf jedem Weg halten, auf dem eine Heilung den Träger erreicht.** Eine
einzelne Lücke schwächt ihn nicht, sie hebt ihn auf — der Träger wird aus dem Auslöser
herausgeheilt und die Fähigkeit ist umsonst gezündet. Erfasst sind deshalb:

| Weg | Ort |
|---|---|
| Heilflag für ein Gruppenmitglied | `StateUpdater.ShouldHealSingle` |
| Heilflag für den Spieler selbst | `StateUpdater.ShouldHealSelf` |
| Kandidatenliste, Rollenpässe, Auswahl des am schwersten Verletzten, Tank-Haltungs-Fallback und `partyMembers[0]`-Fallback | Eingangsfilter von `ActionTargetInfo.FindHealTarget` |
| Selbstabkürzung, die die Kandidatenliste umgeht | eigene Prüfung in `GeneralHealTarget` |

Der Eingangsfilter ist der tragende Teil: Das Heilflag kann von einem **anderen**
Gruppenmitglied gesetzt worden sein, und liegt dieses außerhalb der Reichweite der
gewählten Heilaktion, bleibt der Träger als einziger Kandidat übrig. Ohne den Filter
holt ihn spätestens der Tank-Haltungs-Fallback zurück — ein Dunkelritter trägt Grit.

Zwei Wege bleiben bewusst offen. **Flächenheilung** trifft den Träger als
Nebenwirkung; sie deswegen zu unterlassen hieße, die Gruppe für den Auslöser zu
opfern, und verstieße gegen die Rangordnung. **Fremde Rotationen, die ihr Heilziel
selbst setzen** (`targetOverride: TargetType.Tank`, in den Beiruta-Heilern) umgehen
die zentrale Zielwahl; sie halten allerdings über ihre eigene Sperrliste ohnehin
zurück, dort sogar unabhängig von der Einstellung.

Die Uhrregel musste nicht gebaut werden: `WillStatusEndGCD` meldet einen Status vor
seinem Ablauf als endend. Sie braucht aber eine **eigene** Liste — `StatusTime`
liefert das Minimum über die ganze übergebene Liste, sodass eine fremde kurze
Unverwundbarkeit das Fenster als endend gemeldet hätte.

Die Option ist nötig, weil RSR Living Dead selbst als Notrettung bei
`HealthForDyingTanks` zündet. Dort ist der Tod die Katastrophe, und Walking Dead
verlangt danach eine volle Maximalgesundheit an Heilung in zehn Sekunden.

### Zwei Lücken in der Schildanrechnung, die diese Prüfung nebenbei fand — beide geschlossen

| Befund | Wirkung | Stand |
|---|---|---|
| `ModifyDivineBenisonPvE` setzte `StatusProvide` statt `TargetStatusProvide` | Divine Benison geht auf **fremde** Ziele. Die Doppelbelegungssperre prüfte damit den Spieler statt das Ziel und griff nie; zugleich sperrte sie die Aktion ganz, sobald der Weißmagier den Schild selbst trug. Die drei Geschwister (`AdloquiumPvE`, `EukrasianDiagnosisPvE`, `CelestialIntersectionPvE`) nutzen `TargetStatusProvide` | behoben, beide Ids auf der Zielseite |
| `StatusID.Intersection` (1889) und `Intersection_4040` fehlten in `StatusHelper.ShieldStatus` | Beide sind als „A magicked barrier is nullifying damage" ausgewiesen. Der Schild eines Astrologen zählte nicht zur effektiven Gesundheit; sein Ziel erschien verletzter, als es ist | behoben (A43) |


## Was ausgeschlossen wurde und warum

**Nullvariante** — nichts ändern. Ausgeschlossen: zwei belegte Defekte der Rangstufe 1
blieben stehen.

**Nur Living Dead behandeln, Zielwahl unangetastet.** Ausgeschlossen: Es hätte den
größeren, bereits belegten Fehler stehen gelassen, um einen Sonderfall zu bedienen.

**Eine Rückhaltung von Heilung für The Blackest Night oder Excogitation.**
Ausgeschlossen für **Excogitation**, weil sein Auslöser nicht vorhersehbar ist: Ob der
Schaden kommt, der die Schwelle unterschreitet, ist unbekannt. Ausgeschlossen für
**The Blackest Night**, weil Heilung dessen Auslöser gar nicht berührt — eine Barriere
absorbiert vor den HP. Was dort bleibt, ist keine Rückhaltung, sondern die
Nachrangigkeit eines **zweiten Schildes**; siehe Klasse A+.

**Gunbreaker Catharsis of Corundum als Rückhaltefall.** Ausgeschlossen am Artefakt:
`Status.resx` beschreibt Status 2685 mit „HP will be restored automatically upon
falling below a certain level **or expiration of effect duration**". Der Heilstoß
kommt in jedem Fall; es gibt nichts zurückzuhalten. Der auslösertragende Bezeichner
heißt `CatharsisOfCorundum` (2685, in Dawntrail zusätzlich 4296); `ClarityOfCorundum`
(2684) ist reine Schadensreduktion und gehört nach Klasse C.

**Eine Gesundheitsschwelle statt der Prioritätsstufe.** Ausgeschlossen, weil sie
Fall 14 nicht trifft: Sie kennt den Vergleich mit anderen Gruppenmitgliedern nicht.
Die Prioritätsstufe leistet beides, und `GeneralHealTarget` besitzt die Rangfolge
bereits — es fehlte nur eine Ebene.

**Ein Messbaustein für Heilraten auf Gruppenmitglieder.** Ausgeschlossen, weil die
Uhrregel seinen einzigen vorgesehenen Verbraucher ersetzt. Er hätte bei allen Nutzern
in jedem Kampf Laufzeit gekostet, ohne einen Verbraucher im Baum zu haben. Die
Erhebung dazu bleibt als Befund gültig und ist keine Frage der Messbarkeit, sondern
des fehlenden Aufnehmers:

| Größe | Vorhanden? | Beleg |
|---|---|---|
| Restzeit des Status | **ja** | `StatusHelper.StatusTime(…)` |
| Historie der Gesundheit über die Zeit | **nur für Gegner** | `DataCenter.RecordedHP` wird in `TargetUpdater.cs:513-535` ausschließlich aus `AllHostileTargets` gefüllt |
| Zeit bis zum Tod eines Ziels | **auf Party-Mitglieder nicht anwendbar** | `GetTTK` (`ObjectHelper.cs:3468`) liest diese Historie; für eine Party-Id liefert es `NaN` |
| Abtastrate der Historie | **1 Hz** | `TimeToKillUpdateInterval` (`TargetUpdater.cs:19`) |
| Eingehende Heilung auf ein Party-Mitglied | **nicht ausgewertet, aber verfügbar** | `ActionEffect.ActionEffectEvent` liefert jedes Effektpaket; `Watcher.cs:17-18` filtert auf Quelle = Gegner beziehungsweise Spieler, ein fremder Heiler passiert beide Filter |
| Schadensbetrag eines Gegnertreffers | **verfügbar und gelesen** | `Watcher.cs:137` wertet `damageEffect.value` aus, prüft aber nur `> 0` |

Zwei Genauigkeitsgrenzen blieben auch nach einem Umbau bestehen: Die 1-Hz-Abtastung
ist für ein Zehn-Sekunden-Fenster zu grob, und ein Gesundheitsdelta ist ein Surrogat
für kumulierte Heilung — fallen Heilung und Schaden in dasselbe Intervall, heben sie
sich auf, obwohl die Heilung gegen die geforderte Summe zählt.

**`HpRecoveryDown` und `Mounted` in der Schwellensenkung.** Ausgeschlossen: `Mounted`
nullifiziert Heilung und gehört in einen Ausschluss, nicht in eine Herabstufung;
`HpRecoveryDown` mindert Heilung nur und ist damit ein Grund, **härter** zu heilen.

## Was offen bleibt

**Der Vorlauf der Uhrregel misst bis zur Entscheidung, nicht bis zum Landen der
Heilung.** Im ungünstigsten Fall muss der laufende GCD auslaufen und ein Zauber mit
Wirkzeit darauf fertig werden — zusammen zwei GCDs. Der Vorlauf muss daher mindestens
zwei GCDs betragen, und genau diese zwei GCDs sind der Preis: Bei einem
Zehn-Sekunden-Fenster wird die halbe Phase verschenkt, in der ein noch rechtzeitiger
Tod abgefangen werden kann. Ein kürzerer Vorlauf senkt den Preis, lässt die Heilung
aber nach Ablauf landen, wenn der Tank bereits ungeschützt ist. Eine Fähigkeit ohne
Wirkzeit würde beides lösen — welche Heilung gleich fällt, ist jedoch eine
Rotationsentscheidung, die die zentrale Schicht weder kennt noch erzwingen kann.

**Die gestaffelte Phase-2-Unterstützung** (Fälle 4 und 4a) ist nicht gebaut. Sie
verlangt eine Kursprognose und damit den verworfenen Messbaustein. Welche Stellen im
Code ihr entgegenstehen, führt `TODO.md`.

**Eine einzige Frage zum Weisen.** Die Verbrauchsreihenfolge ordnet Eukrasian Diagnosis
hinter The Blackest Night ein, ein Job-Guide davor. Träfe Letzteres zu, könnte der
Einzelschild des Weisen die TBN-Absorption verzögern und Dark Arts kosten. Die beiden
Primärquellen, die das klären würden, sind vom Egress nach Organisationsrichtlinie
gesperrt. Für Weißmagier, Gelehrten und Astrologen besteht die Frage nicht — deren
Schilde liegen nach beiden Quellen deutlich hinter TBN.

## Nachweisbarkeit

| Ebene | Möglich | Nicht möglich |
|---|---|---|
| Statisch | `.github/scripts/audit/scan8.py` meldet Prädikate mit negierendem Namen, die im Baum mit beiden Polaritäten gelesen werden — genau die hier gefundene Defektklasse | — |
| Kompilierung | CI | — |
| Laufzeit | Beobachtung, ob ein Dunkelritter unter Living Dead noch geheilt wird | Beweis, dass die neue Rangfolge besser spielt |

Erreichter Prüfgrad: statische Selbstprüfung, Prüfskript, Fallmatrizen, CI-Kompilierung.
Keine Laufzeitbeobachtung, kein Vier-Augen-Prinzip. Dass die Rückhaltung besser spielt,
ist deshalb **nicht** belegt und steht hinter einer Option; die Richtigstellung der
Zielwahl ist eine belegte Defektbehebung und bekommt keine — ein Schalter würde den
fehlerhaften Zustand konservieren.

## Konsequenzen

**Endnutzer.** Die Heilzielwahl aller Jobs ändert sich spürbar, weil die Rangfolge
erstmals greift: Der am schwersten verletzte DPS wird überhaupt erst wählbar, wo
zuvor regelmäßig der Tank gewählt wurde. Das ist die größte Verhaltensänderung des
Vorhabens und zugleich die einzige, für die eine Laufzeitbeobachtung wirklich
wünschenswert wäre.

**Autoren abgeleiteter Rotationen.** `NoNeedHealingStatus` behält seine Signatur,
ändert aber seinen Inhalt — eine Verhaltensänderung ohne Compilerfehler für jede
abgeleitete Rotation, die die Liste liest. Im `CHANGELOG` vermerkt.

**Upstream.** Der Eingriff liegt in `ActionTargetInfo`, `StatusHelper` und
`StateUpdater`, alle mit regelmäßiger Upstream-Aktivität.

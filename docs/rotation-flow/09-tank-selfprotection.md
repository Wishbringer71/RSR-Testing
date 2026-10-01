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
| Sonderbehandlung für The Blackest Night | **nicht nötig**, aber aus dem umgekehrten Grund: Eine Barriere ist kein Grund, später zu heilen — s. u. |
| Schildanrechnung auf die Heilschwelle | **entfernt** (A85). Gesundheit und Schild addieren sich, sie ersetzen einander nicht |
| Messbaustein für Raten auf Gruppenmitglieder | **gebaut** (A91–A93): `RecordedHP` führt die Gruppe mit, `GetCorrectedTTK` liefert die Restzeit je Mitglied. Die frühere Verwerfung ist überholt |

## Prüfmaßstab — die Rangordnung

Alles Folgende ist an dieser Ordnung zu messen. Sie ist **lexikographisch**: Eine
nachrangige Stufe darf eine vorrangige nie aufwiegen, gleich wie groß ihr Betrag
wäre.

1. **Das Überleben des Tanks.**
2. **Die Mitnahme positiver Effekte** — Dark Arts, Catharsis, die Selbstheilung von
   Walking Dead.
3. **Die Vermeidung unnötiger Aktionen** — verbrauchte oGCDs, MP, Cooldowns.

**Das Maß für „unnötig" auf Stufe 3 ist die Größe des Treffers, nicht das Gefühl.** Vorgabe des
Auftraggebers, ausgeschrieben in `08-mitigation-synergy.md` („Die Antwort auf einen eingehenden
Treffer"): Gesucht ist Deckung in Höhe des Treffers, gemessen am schwächsten Mitglied. Ein Zehn-
Prozent-Tick rechtfertigt keine große Barriere, ein Treffer oberhalb der vollen Gesundheit dagegen
Barriere **und** Minderung zusammen. Stufe 1 bleibt davon unberührt: Wo das Überleben in Frage steht,
entscheidet nicht die Sparsamkeit.

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
bereits eine eigene Rangfolge in `ActionTargetInfo.GeneralHealTarget`: zuerst wer gleich fällt
(kritische Klasse, Konzept 07), dann Selbst → Heiler → Tank → niedrigste Gesundheit; mit „Choose
the heal target by danger" statt der Rollen-Kurzwege die Gefährdungsklassen 2 und 3 (Stand 29.09.2026).
Sie hier ebenfalls umzustellen wäre eine zweite, größere Änderung.

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

**Umgesetzt ist das in `StatusHelper.InDeathTriggerWindow` als zwei Bedingungen, nicht
als eine Uhr.** Die Zurückhaltung endet einen Vorlauf von zwei GCDs vor Ablauf, damit
die Heilung noch landen kann — **aber nur, solange der Träger über
`HealthForDyingTanks` steht.** Darunter läuft sie bis zum Ablauf durch, weil dort der
Fall auf 0 die wahrscheinliche Fortsetzung ist und der Vorlauf sonst genau den Auslöser
wegheilen würde, für den die ganze Regel da ist. Die Uhr allein hätte die Fälle 1c und
1e der Tabelle oben nicht mehr unterschieden: Sie beantwortet „wie viel Zeit bleibt",
nicht „kommt der Tod noch".

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

### Umsetzung für Phase 2 (A147)

**Vorgabe des Auftraggebers (25.09.2026):** „walking dead läßt solange es läuft den darkknight sich
selbst durch angriffe heilen. […] also vertraut man am anfang (bei eben den 1hp) darauf, dass der tank
sich selbst heilt, indem er bei gegnern schaden verursacht. das soll nur leicht mit einem hot
unterstützt werden. erst wenn der timer des effektes ausläuft bzw. klar ist, dass der darkknight sich
in der verbleibenden zeit nicht selbst durch angriff (vollständig) heilen kann, soll unterstützt werden.
das kann z.b. durch fehlende gegnerzahlen oder durch ein anstehendes event passieren, wo der darkknight
nicht mehr angreifen kann." Der Wirktext bestätigt die Mechanik (Living Dead, `ActionId.resx` 3638).

**Sachstand:** `StatusHelper.WalkingDeadCarriedBySelfHeal` sagt, ob der Träger noch von seinen eigenen
Angriffen getragen wird. Solange das gilt, nimmt ihn keine Heilaktion als Ziel, die keinen HoT aus
`SingleHots` verleiht (`ActionTargetInfo.FindTarget`), und er zählt nicht als Grund für eine
Flächenheilung um den Wirkenden. Regen des Weißmagiers ist unter Walking Dead von der
`RegenHeal`-Sperre ausgenommen. Die volle Unterstützung setzt ein, sobald einer dieser Fälle eintritt:

| Auslöser | Maß, aus dem Spiel |
|---|---|
| Timer läuft aus | derselbe Vorlauf wie bei der Living-Dead-Sperre (zwei GCDs bis zur Entscheidung) |
| kein Gegner in Reichweite | Spielreichweite von Hard Slash, Trefferfläche zu Trefferfläche |
| angekündigtes Ereignis | BossModReborn-Auszeit vor Ablauf; ohne Modul erst reaktiv über die Reichweite |
| Tank-Limitbruch auf der Gruppe | Status Last Bastion, Land Waker, Dark Force oder Gunmetal Soul |
| er schafft es nicht | Gesundheit seit Beginn des Fensters, auf die Restzeit hochgerechnet, bleibt unter 100 % |

Der Kurs ist netto: Schaden zieht ab, er unterschätzt also die kumulierte Heilung, und die Freigabe
kommt eher zu früh als zu spät. Im ersten GCD gibt es noch nichts zu messen; dann wird ihm vertraut.

**Hinweis des Auftraggebers zu „most attacks":** Es gibt Raidwides, die alle nur mit dem Limitbruch
eines Tanks überleben; das ist wahrscheinlich die Ausnahme, die der Wirktext meint (seine Deutung).
Es sind sehr wenige, seine Beispiele: die Alexander-Raids, die Prüfung gegen den Krieger des Lichts.
Deshalb gibt nicht jeder Raidwide die volle Unterstützung frei — das höbe das Vertrauen am Anfang bei
jedem Raidwide auf —, sondern erst der Tank-Limitbruch auf der Gruppe: Er wird für genau diesen
Treffer gezogen, und bei 1 HP stünde der Träger schutzlos davor.

**Grenzen:** Rotationen, die ihr Heilziel selbst wählen statt über `FindTarget` (fremde Rotationen,
direkte Aufrufe von `FindTargetByType`), sehen die Sperre nicht. Die Flächenheilflagge rechnet den
Träger bei 1 HP weiter in ihre Mittelwerte ein. Ein solcher Treffer ohne Tank-Limitbruch wird nicht
erkannt. Der Limitbruch selbst wird im selben Bild erkannt, in dem sein Status erscheint; die Heilung
kommt aber nur vor dem Einschlag an, wenn dazwischen noch ein Einschiebeplatz (Benediction) oder ein
GCD mit Wirkzeit (Cure II) liegt. Wie viel Zeit zwischen Limitbruch und Einschlag liegt, entscheidet der
Tank, der ihn zieht.

**Im Kampf ablesbar:** Das Diagnosefenster zeigt, solange jemand unter Walking Dead steht, ob er
getragen wird oder welcher Auslöser die volle Unterstützung freigegeben hat, mit dem hochgerechneten
Kurs.

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


**Der Hebel ist die Option, nicht der Grenzwert.** Zwei Mechanismen, je nach Stellung von
`WithholdHealingForLivingDead`:
- **Option aus:** `StateUpdater.ShouldHealSingle` senkt die Schwelle unter einem Schutzstatus auf
  `HealthProtectedRatio` (0,15), solange Living Dead mehr als zwei GCDs Restzeit hat; danach kehrt die
  normale Schwelle zurück, unabhängig von der Gesundheit.
- **Option an:** Der Träger wird gar nicht geheilt, solange das Todesfenster läuft
  (`IsHeldForDeathTrigger`). Das Fenster endet zwei GCDs vor Ablauf, außer der Träger steht auf oder
  unter `HealthForDyingTanks` (`DeathStillLikely`, A88) — dann läuft es bis zum Ablauf.

`HealthProtectedRatio` anzuheben verschöbe den ersten Mechanismus und heilte **früher** im Fenster,
also gerade den Tod weg, auf den die Regel wartet. Wer den Todeseffekt will, schaltet
`WithholdHealingForLivingDead` ein; der Grenzwert ist nur für die
übrigen Invulnerabilitäten der Liste maßgeblich (Holmgang, Superbolide, Hallowed Ground), bei denen
kein Tod gewollt ist. Upstream heilt ein Ziel unter Invulnerabilität gar nicht; die Absenkung ist die
mildere Fassung. Ein zu früh gesetzter Living Dead (vom Auftraggeber bei 70 % im Wall-to-Wall
beobachtet) kostet damit zehn Sekunden automatischer Heilung ohne Anlass.

**Welche Abwehr des Dunkelritters die Heilentscheidung berührt** (erhoben A141):

| Fähigkeit | Pfad | Wirkung auf die Heilschwelle |
|---|---|---|
| Living Dead | `NoNeedHealingStatus` → `HealthProtectedRatio` | 0,15 statt der normalen Schwelle, wie oben |
| Walking Dead | in `NoNeedHealingStatus` auskommentiert | keine — richtig, dort ist Heilung überlebensnotwendig |
| The Blackest Night | Schildanteil des Spiels (`ShieldPercentage`) im effektiven Puffer | keine auf die Schwelle, seit die Schildanrechnung entfernt ist (A85); der Schild zählt im Puffer der Vorausschau und der Sterbegefährdung (`GetEffectiveHp`) |
| Shadow Wall, Rampart | `RampartStatus` | keine: gelesen als `StatusProvide` und von `HasMajorMitigation` für den eigenen Charakter |
| Dark Mind, Oblation, Dark Missionary, Reprisal | in keiner heilrelevanten Liste bzw. am Gegner | keine |

Schadensreduktion und Barriere wirken auf keine Heilschwelle; nur die Invulnerabilität tut es. Die Rate, die aus
Minderung folgt, geht über die Vorausschau ein (`GetForecastSurvivingShare`), nicht über Listen.
### Die Barriere senkt den Heilbedarf nicht

**Ein Schild verhindert Schaden, er stellt keine Gesundheit her.** Ein vollgeheilter Tank **mit**
Barriere ist besser geschützt als ein geschildeter Tank mit wenig Gesundheit; beide Größen addieren
sich, sie ersetzen einander nicht. Eine laufende Barriere ist damit die Gelegenheit, das Polster zu
vergrößern, nicht der Grund, es kleiner zu lassen. Läuft sie ungenutzt ab, steht der Träger
unverändert tief; fängt sie den Treffer, ist sie verbraucht und er steht ebenso tief — verändert hat
sich nur die verbleibende Zeit.

Die frühere Anrechnung (`CreditShieldToEffectiveHp`) rechnete den Schild auf die Gesundheitsquote und
verzögerte damit die Einzelziel-Heilung um die Barrierengröße — bei The Blackest Night 25
Prozentpunkte. Sie ist entfernt (A85); das Verhalten entspricht wieder dem Upstream.

**Heilung steht dem Aufzehren der Barriere nicht entgegen**, und darin liegt der Unterschied zur
Minderungssperre: Wie schnell eine Barriere verbraucht wird, hängt allein am eingehenden Schaden. Die
einzige Kopplung läuft umgekehrt — stirbt der Träger vorher, entfällt Dark Arts. Was den Verbrauch
verhindert, ist Schadensminderung, und dagegen steht die Sperre in Konzept 10.

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

**Ein eigener Messbaustein für Heilraten auf Gruppenmitglieder.** Ausgeschlossen — und
inzwischen auch überflüssig. Ausgeschlossen war er, weil die Uhrregel seinen einzigen
vorgesehenen Verbraucher ersetzt; überflüssig ist er, weil die Größe ohne neuen
Baustein entstanden ist: `DataCenter.RecordedHP` nimmt die Gruppenmitglieder seit A91
mit auf, und `GetTTK` wertet sie seitdem für jede Gruppen-Id aus. Der Eingriff war eine
Schleife neben der bestehenden, kein Ringpuffer und kein dritter Effekt-Handler.

| Größe | Vorhanden? | Beleg |
|---|---|---|
| Restzeit des Status | **ja** | `StatusHelper.StatusTime(…)` |
| Historie der Gesundheit über die Zeit | **für Gegner und Gruppe** | `DataCenter.RecordedHP`, gefüllt in `TargetUpdater.UpdateTimeToKill` aus `AllHostileTargets` **und** `PartyMembers` |
| Zeit bis zum Tod eines Gruppenmitglieds | **ja** | `ObjectHelper.GetCorrectedTTK`, gegen den eigenen Vorhersagefehler kalibriert |
| Abtastrate der Historie | **1 Hz** | `TimeToKillUpdateInterval` |
| Eingehende Heilung auf ein Party-Mitglied | **nicht gesondert ausgewertet, und nicht nötig** | Der Gesundheitsverlauf ist bereits netto: Eine fremde Heilung zeigt sich als steigender Anteil, `GetTTK` antwortet dann `NaN` |
| Schadensbetrag eines Gegnertreffers | **verfügbar und gelesen** | `Watcher.FullAmount`, voller Betrag auch über 65.535 Punkte (A140, A144) |

Zwei Genauigkeitsgrenzen bestehen fort: Die 1-Hz-Abtastung ist für ein
Zehn-Sekunden-Fenster grob, und ein Gesundheitsdelta ist ein Surrogat für kumulierte
Heilung — fallen Heilung und Schaden in dasselbe Intervall, heben sie sich auf, obwohl
die Heilung gegen die von Walking Dead geforderte Summe zählt. Für den Weg **zur Null**
ist das unerheblich, für den Weg **zur aufgenommenen Heilmenge** nicht; deshalb ist
Fall 4a mit der vorhandenen Auswertung noch nicht beantwortet.

**`HpRecoveryDown` und `Mounted` in der Schwellensenkung.** Ausgeschlossen: `Mounted`
nullifiziert Heilung und gehört in einen Ausschluss, nicht in eine Herabstufung;
`HpRecoveryDown` mindert Heilung nur und ist damit ein Grund, **härter** zu heilen.

## Krieger: Nascent Flash für einen anderen oder Bloodwhetting für sich

**Seine Vorgabe (29.09.2026), im Wortlaut:** „nascent flash dahingehend im vollständigen loop bewerten: ist der
tankbuster tödlich? braucht der tank nach dem tankbuster viel heilung? ist der tankbuster mit bestehenden
schilden, minderung und bestehenden hot oder sonstiger verfügbarer heilung so gut wegsteckbar, dass nascent flash
auf andere gecasted werden kann. auf wen soll nascent flash denn gecasted werden? auf heiler? heiler haben eine
höhere priorität als normale damagedealer. auch da dann prüfen: was ist beim ziel vorhanden? schilde, bestehender
hot, bestehende minderung. haben die ziele debuffs, hat der tank debuffs? bewertungstriage = muss auf jemanden
durch tod verzichtet werden und wer ist am ehestens entbehrlich?" Einordnung: Vorgabe mit Kriterien für die
vorgelegte Entscheidung (TODO, A191). Der Satz „Heiler haben eine höhere Priorität als normale Damagedealer"
bestätigt die Rollenordnung aus Konzept 07; dort gilt sie bei gleicher Gefährdung.

### Die Mechanik (Wirktexte, Job-Guide und Spieldaten, abgerufen 29.09.2026)

| | Bloodwhetting (82, auf sich) | Nascent Flash (76, auf ein Mitglied, nicht auf sich) |
|---|---|---|
| Minderung | −10 % für 8 s, dazu Stem the Flow −10 % für 4 s | dieselbe auf dem **Ziel** (Nascent Glint, Stem the Flow) |
| Barriere | Stem the Tide, 400 Potenz, 20 s | dieselbe auf dem **Ziel** |
| Heilung | 400 Potenz je Waffenfertigkeit, 8 s, auf den Krieger | 400 Potenz je Waffenfertigkeit auf den **Krieger** (Nascent Flash) **und** 100 % davon auf das Ziel |
| Abklingzeit | 25 s, gemeinsam (Abklingzeitgruppe 7, auch Raw Intuition) | |

**Daraus folgt die tragende Feststellung: Nascent Flash auf einen anderen kostet den Krieger keine Heilung.** Er
heilt sich damit genauso wie mit Bloodwhetting. Er verliert Minderung und Barriere: in den ersten 4 s rund 19 %
weniger Schaden (0,9 × 0,9, sofern Minderungen multiplizieren — Spielregel, hier nicht am Artefakt belegt), danach
bis 8 s 10 %, und 400 Potenz Barriere. Die zweite seiner Fragen, ob der Tank nach dem Tankbuster viel Heilung
braucht, trägt deshalb keinen Grund, Nascent Flash zurückzuhalten.

### Sachstand im Code

- **Defekt, behoben (A226): Nascent Flash ging nur an Unverwundbare.** Der Zielfilter las
  `!t.NoNeedHealingInvuln()`. Die Funktion ist wahr, solange **keine** Unverwundbarkeit liegt; die Negation ließ
  also nur Geschützte durch — einen Tank unter Holmgang, Superbolide, Hallowed Ground oder Living Dead, sonst
  niemanden. Seit Upstream c3fac720b heilte der Krieger damit praktisch nie ein anderes Mitglied; der in A191
  vorgelegte Konflikt am Tankbuster trat so gar nicht auf. Dieselbe Verwechslung ist die dritte ihrer Art im Baum
  (Heilzielwahl, Excogitation); `check_invuln_polarity.py` hält sie jetzt in der CI.
- Nascent Flash liegt im Heilpfad der Fähigkeiten (`HealSingleAbility`, Upstream c3fac720b). Dieser läuft im
  Dispatch vor der Einzelabwehr und vor dem allgemeinen Pfad. Ziel ist ein Mitglied unter
  `Nascent Flash Heal Threshold` (0,6, Vorausschau), nicht unverwundbar, sortiert nach Einstellung „Nascent Flash
  target priority": niedrigster **Prozentsatz der aktuellen Gesundheit** (ohne Vorausschau, ohne Barriere),
  Heiler zuerst oder nur Heiler.
- Bloodwhetting fällt für sich
  - in der Einzelabwehr (Tankbuster, Beschuss) mit „Use Bloodwhetting/Raw intuition on single enemies" oder
    bei mehr als zwei Gegnern in Reichweite, und nur, solange der Gegner den Krieger anvisiert;
  - im allgemeinen Pfad reaktiv unter „Bloodwhetting/Raw intuition heal threshold" (0,7).
  „single enemies" steht ab Werk **an** (A237, seine Regel für Voreinstellungen); bis dahin stand es aus, und vor
  einem Boss-Tankbuster fiel Bloodwhetting gar nicht, erst danach reaktiv. Mit der Einstellung aus gilt das weiter.

### Bewertung nach seinen Kriterien

**Ist der Tankbuster tödlich?** Das weiß RSR nicht, und das ist kein Mangel der Regel, sondern der Datenlage.
BossModReborn meldet Art, Zeitpunkt und Getroffene, **keine Höhe** (Konzept 07). Der Effekt-Handler könnte die Höhe
messen: Der Messweg der Flächen (Konzept 13) sieht jeden Treffer. Er misst aber **nach** Minderung und Barriere,
und ein Tank mindert Tankbuster fast immer — gemessen würde also systematisch zu niedrig, und „zu niedrig" ist
hier die gefährliche Richtung (ein tödlicher Treffer läse sich als harmlos). Das Herausrechnen bräuchte je Treffer
die wirkenden Minderungen mit ihren Prozentsätzen, die Schadensart der Aktion (Feint mindert physisch, Addle
magisch) und die Antwort, ob der gemeldete Schadenswert eine aufgezehrte Barriere enthält — die letzte ist nicht
belegt. Das ist ein eigenes Vorhaben (TODO). **Bis dahin gilt ein angekündigter Tankbuster auf den Krieger als
möglicherweise tödlich**: Er ist angekündigt, also wahrscheinlich, und seine Höhe ist unbekannt — nach seiner
Spielweise geht dann die Sicherheit vor.

**Ist er mit Bestehendem wegsteckbar?** Ohne Höhe nur in einem Fall entscheidbar, und der ist vollständig: Liegt
eine Unverwundbarkeit (Holmgang, oder jede andere aus `NoNeedHealingStatus`) auf dem Krieger, während der
Tankbuster auf ihn gewirkt wird, kann er ihn nicht töten. Schilde, laufende Minderungen, HoTs und die Heiler senken
den Treffer oder füllen nach; ob das genügt, hängt an der Höhe, die fehlt.

**Wird Bloodwhetting für den Tankbuster überhaupt gewirkt?** Das ist die Frage, die der Code beantworten kann, und
sie entscheidet, ob eine Zurückhaltung etwas bewirkt. Fällt Bloodwhetting nicht vor dem Tankbuster (ab Werk bei
einem einzelnen Boss), hält eine Zurückhaltung Nascent Flash vom Mitglied fern, ohne dass der Krieger am Treffer
etwas davon hat; seine reaktive Nutzung danach ersetzt Nascent Flash bis auf Minderung und Barriere, und die Heilung
bekommt er ohnehin. Die Zurückhaltung für den Tankbuster gilt deshalb nur, wenn die Einzelabwehr Bloodwhetting
wirken würde — dieselbe Bedingung an derselben Stelle (`BloodwhettingForDefense`), nicht kopiert.

**Wann ein Tankbuster „auf ihn und vor der Abklingzeit" kommt:**
- ein gelisteter Tankbuster-Zauber auf ihn oder ein Tankbuster-Marker auf ihm (`IsHostileCastingTankBusterAtMe`,
  wenige Sekunden voraus);
- BossModReborn sagt einen Tankbuster innerhalb der Abklingzeit von Nascent Flash voraus (25 s, aus den
  Spieldaten). Wen er trifft, sagt BossModReborn nur, solange er der nächste vorhergesagte Treffer im
  Minderungsfenster ist (`BMRTankbusterHitsPlayer`). Weiter voraus gilt als Näherung: auf ihn, solange sein
  Ziel ihn anvisiert (Schluss, nicht belegt — Tankbuster auf den zweiten in der Feindseligkeit gibt es).
- In allen drei Fällen nur, solange sein Ziel ihn anvisiert: Das ist die eigene Prüfung von Raw
  Intuition/Bloodwhetting (`PlayerIsTargetOnSelf`). Ein Off-Tank mit Tankbuster-Marker bekommt Bloodwhetting aus
  der Einzelabwehr nicht, die Zurückhaltung hielte dort umsonst. Dass Bloodwhetting dann fehlt, ist eine
  Upstream-Bauform der Aktion, nicht dieser Regel (TODO).
Ohne BossModReborn-Modul bleibt nur der sichtbare Zauber oder Marker; die Zurückhaltung greift dann nur Sekunden
vorher. Das ist eine Grenze, kein stiller Ausfall: Die Regel hält dann seltener, nie falsch.

**Auf wen?** Nascent Flash ist eine Heilung samt Minderung und Barriere. Für die Zielwahl der Heilung gilt seine
Vorgabe aus Konzept 07: **wer am stärksten gefährdet ist zu sterben; bei gleicher Gefährdung Heiler vor Tank vor
Schadensausteiler.** Genau das tut die Heilzielwahl des Baums (`FindHealTarget`): die kritische Klasse zuerst, nach
absoluten effektiven Punkten; mit „Choose the heal target by danger" die Klassen 2 und 3; Gleichstand nach Rolle.
Die drei vorhandenen Einstellungen tun es nicht: „Lowest HP party member" sortiert nach aktuellem Prozentsatz und
übergeht Barriere und Verlauf; „Healers first" zieht jeden Heiler unter der Schwelle einem sterbenden
Schadensausteiler vor, was die Rollenordnung über die Gefährdung stellt; „Healers only" lässt alle anderen aus.
**Deshalb eine vierte Einstellung: „By danger" — die Heilzielwahl.** Die drei bestehenden behalten ihre Bedeutung,
weil ihr Text bindet.

**Was beim Ziel vorhanden ist:**
- *Schilde* zählen in der kritischen Klasse (effektive Gesundheit). Wer hinter einer Barriere steht, fällt nicht in
  sie. Für die Heilschwelle zählen sie nicht, nach seiner Entscheidung A85: Gesundheit und Schild ersetzen einander
  nicht.
- *HoTs und Minderungen* wirken über den gemessenen Verlauf: Er ist netto aller Heilung und Minderung, die
  Vorausschau liest ihn (mit „Heal ahead of incoming damage" an).
- *Ein Nascent Glint eines anderen Kriegers* sperrt das Ziel (`TargetStatusProvide`).
- *Unverwundbare* sind ausgeschlossen (Filter), *Heilung wirkungslos* (`HealingIneffectiveStatus`, Mounted) jetzt
  ebenfalls — die Heilzielwahl schloss sie schon aus, die Sortierung nach Prozentsatz nicht.

**Debuffs:**
- *Beim Ziel:* Doom zählt als 1 % Gesundheit und steht damit vorn — Nascent Flash heilt nicht voll, aber Minderung
  und Heilung helfen, und die volle Heilung kommt aus dem Heilpfad der Heiler. `HpRecoveryDown` mindert nur die
  Heilung; Minderung und Barriere wirken voll, das Ziel bleibt Kandidat. Eine Verwundbarkeit erhöht den Eingang;
  der Verlauf zeigt ihn.
- *Beim Krieger:* Eine Verwundbarkeit macht den nächsten Tankbuster härter. Ohne Höhe ändert das an der Regel
  nichts, sie ist bereits vorsichtig. Heilstrafen (Scalebound, Shackled Healing) sperren den ganzen Heilpfad
  schon (`PlayerHealingPunished`).

### Die Triage: wer verzichtet, wenn beide es brauchen

Sein Bedarf an Bloodwhetting besteht, wenn **(a)** ein Tankbuster auf ihn vor der Abklingzeit kommt und Bloodwhetting
dafür gewirkt würde, oder **(b)** er selbst in der kritischen Klasse steht. Dann geht Nascent Flash nur an ein
Mitglied, das die Triage gegen ihn gewinnt, und das folgt seiner Rollenordnung nach Ersetzbarkeit (Konzept 07) und
dem Grundsatz „sicher vor möglich":

| Mitglied | gewinnt gegen seinen Bedarf? | Grund |
|---|---|---|
| nicht kritisch, gleich welche Rolle | nein | es stirbt nicht am nächsten Treffer; die Heiler decken es |
| **Heiler**, kritisch | **ja** | fällt der Heiler, fällt die Gruppe mit ihm (seine Begründung) |
| **Tank**, kritisch | ja, solange der Krieger selbst nicht kritisch ist | gleiche Rolle; sein Tod ist sicher, der Tankbuster möglich |
| **Schadensausteiler**, kritisch | **nein** | am ehesten entbehrlich: wiederbelebbar, während ein toter Tank meist den Kampf kostet (Konzept 08) |

**Gegenposition, geprüft und nicht widerlegt:** Bloodwhetting ist am Tankbuster ein kleiner Anteil — rund 19 % in
4 s und eine Barriere — und oft nicht der Unterschied zwischen Leben und Tod. Nascent Flash auf einen kritischen
Schadensausteiler kann dagegen genau diesen Unterschied machen. Ohne die Höhe des Tankbusters opfert die Regel im
Zweifel einen Schadensausteiler für einen Treffer, den der Krieger auch ohne Bloodwhetting überlebt hätte. Sie ist
nicht widerlegbar, solange die Höhe fehlt; die Regel folgt seiner Ordnung, und mit der Messung aus dem TODO wird
aus „möglicherweise tödlich" eine Zahl.

### Antithesen

- **Kein Defekt:** Mit „single enemies" aus fällt Bloodwhetting vor dem Tankbuster eines einzelnen Bosses nicht —
  dann gibt es den Konflikt am Tankbuster nicht, und die Regel hält dort auch nicht (Bedingung (a)). Er besteht mit
  der Einstellung an (ab Werk seit A237) und bei mehr als zwei Gegnern, und Bedingung (b) besteht immer. Die Zielwahl nach Prozentsatz
  wählt einen Schadensausteiler bei 10 % hinter Barriere vor einem bei 12 % ohne, der am nächsten Treffer stirbt.
- **Option falsch:** Die Näherung „wen sein Ziel anvisiert, der bekommt den Tankbuster" kann irren; dann hält der Krieger
  Nascent Flash für einen Treffer, der den anderen Tank trifft — und gerade der könnte es brauchen. Der Fehler geht
  in die vorsichtige Richtung und endet, sobald der Tankbuster im Minderungsfenster steht und die Maske ihn richtig
  zuordnet. Die Triage kann einen Schadensausteiler kosten (Gegenposition oben).
- **Ausgeliefert, und nichts ändert sich:** Ohne Tankbuster-Signal greift nur (b). Ohne Bloodwhetting in der
  Einzelabwehr greift nur (b) — richtig so, siehe oben. Mit „By danger" ändert sich die Wahl nur, wo kritische
  Klasse, Barriere oder Verlauf vom Prozentsatz abweichen; sonst wählt sie denselben.

### Umsetzung (A226)

- `WAR_Reborn.BloodwhettingForDefense`: die Bedingung der Einzelabwehr, gelesen von ihr und von der Zurückhaltung.
- Einstellung **„Keep Bloodwhetting for yourself when you need it"**, ab Werk **an** (seine Regel für Voreinstellungen,
  29.09.2026): hält Nascent Flash nach (a)/(b) und der Triage zurück.
- Vierte Zielwahl **„By danger"**, ab Werk gewählt (angehängt; Rotationseinstellungen speichern den Namen).
- Heilung-wirkungslos-Ausschluss in allen vier Zielwahlen.
- Der Zielfilter liest `NoNeedHealingInvuln()` mit der richtigen Polarität (belegter Defekt, ohne Schalter).
- **Nicht gebaut:** Nascent Flash als Minderung für den **anderen Tank vor dessen Tankbuster** — der Anwendungsfall,
  den The Balance mit „Nascent Flash goes on a friend" meint, ist hier ein Schluss, keine Quelle. Das ist neues
  Verhalten und steht als Vorschlag im TODO. Ebenso die Messung der Tankbuster-Höhe.

## Krieger: die Abwehr im Ganzen

**Seine Fragen und Aufträge (01.10.2026):** „abtausch auf tankbuster durch boss bringt nichts, wird aber gecasted"
(Boss allein in der Arena, Krieger); „ist kampfrausch vor tankbuster nicht sinnvoll? warum nicht genutzt?"; „evtl.
auch bei gruppenpulls wall to wall sinnvoll. alle defskills warrior im vollen loop prüfen, bewerten, schauen, wie
bislang genutzt"; „in die bestehenden konzepte einarbeiten im vollen loop, kritisch alles bewerten". Einordnung:
Beobachtung mit Hinweis auf die Lage, Frage, Prüfauftrag.

**Maßstab:** seine Spielweise — Sicherheit vor Schaden, gewichtet mit der Wahrscheinlichkeit des Treffers. Ein
angekündigter Tankbuster auf ihn gilt als wahrscheinlich und, ohne gemessene Höhe, als möglicherweise tödlich
(Abschnitt Nascent Flash). Für einen Pull misst die Wahrscheinlichkeit der gemessene Gesundheitsverlauf. Keine
der Abwehrfähigkeiten kostet Schaden; ihr Preis ist allein die Abklingzeit, also ob sie beim nächsten Bedarf fehlt.
Referenz für Rotationen: The Balance, Warrior Basic Guide, „Staying Alive" (abgerufen 01.10.2026).

### Die Fähigkeiten (Job-Guide deutsch und englisch, abgerufen 01.10.2026)

| Fähigkeit | Stufe | Abklingzeit | Wirkung |
|---|---|---|---|
| Schutzwall (Rampart) | 8 | 90 s | −20 % für 20 s, Heilung auf ihn +15 % |
| Tiefschlag (Low Blow), Zwischenruf (Interject) | 12, 18 | 25 s, 30 s | Betäubung, Unterbrechung |
| Reflexion (Reprisal) | 22 | 60 s | Gegner im Umkreis von 5 Yalm −10 % Schaden für 15 s |
| Kampfrausch (Thrill of Battle) | 30 | 90 s | maximale Gesundheit +20 % und aufgefüllt, Heilung auf ihn +20 %, 10 s |
| Abtausch (Arm's Length) | 32 | 120 s | Rückstoßschutz 6 s; wer ihn physisch trifft, bekommt Gemach +20 % für 15 s |
| Rachsucht (Vengeance) → Verdammnis (Damnation) | 38 → 92 | 120 s | −30 % → −40 % für 15 s; Verdammnis danach Regeneration (Status Primeval Impulse) |
| Holmgang | 42 | 240 s | Gesundheit fällt 10 s lang nicht unter 1 |
| Urinstinkt (Raw Intuition) → Urimpuls (Bloodwhetting) | 56 → 82 | 25 s, geteilt mit Urflackern | −10 % (6 s → 8 s), Heilung je Waffenfertigkeit; Urimpuls dazu −10 % für 4 s und eine Barriere |
| Äquilibrium (Equilibrium) | 58 | 60 s | Heilung 1200 Potenz und Regeneration |
| Abschütteln (Shake It Off) | 68 | 90 s | Gruppenbarriere 15 % der Maximalgesundheit; hebt Kampfrausch, Verdammnis und Urimpuls auf, +2 % je aufgehobenem Effekt |
| Urflackern (Nascent Flash) | 76 | 25 s, geteilt | Urimpuls-Wirkung auf ein Mitglied, Heilung auf ihn |

The Balance führt sie als Stapel für Tankbuster, von „Reprisal | Thrill | Rampart | Bloodwhetting" bis zum „Kitchen
Sink" aus allen fünf, mit Holmgang als letzter Stufe. Urimpuls heilt je Treffer, „very powerful in dungeons";
Äquilibrium ist „great when used with Thrill of Battle, Rampart or both". Abschütteln bemisst seine Barriere an der
durch Kampfrausch erhöhten Maximalgesundheit („Big Value"). Reflexion auf einen Tankbuster ist „very situational",
weil sie für den nächsten Raidwide fehlen kann.

### Wie RSR sie nutzt, und was davon richtig ist

Die Einzelabwehr des Kriegers (`WAR_Reborn.DefenseSingleAbility`) öffnet zweimal: für einen Tankbuster oder Zauber
auf ihn (Marker, gelisteter Zauber, Vorhersage innerhalb von „Seconds before tankbuster to use single mitigation",
ab Werk 3 s) und für einen Pull (`TankPullOnPlayer`: mindestens „Number of hostiles" Gegner auf ihm in 3 Yalm, er wird
getroffen). Je Einwebeplatz fällt die erste passende Fähigkeit in dieser Reihenfolge:

1. **Abtausch** auf einem Rudel gewöhnlicher Gegner für den Slow, Bosse nicht mitgezählt (A236). *Richtig:* Er mindert
   den Treffer nicht, der ihn auslöst. Die zentrale Rückfallstufe, die ihn auf jeden Tankbuster warf, ist geschlossen.
2. **Urimpuls/Urinstinkt** mit „single enemies" (ab Werk an, A237) oder mehr als zwei Gegnern. *Richtig:* Er ist der
   Kern jedes Stapels und heilt im Pull je Treffer.
3. **Kampfrausch** vor einem Tankbuster auf ihn innerhalb seiner Wirkdauer (A237).
4. **Verdammnis/Rachsucht** und **Schutzwall** für einen vorhergesagten Tankbuster, unabhängig voneinander. Darunter die
   Staffelung: Verdammnis, wenn Schutzwall bereit ist oder vor mehr als 60 s fiel; Schutzwall, wenn Verdammnis vor
   mehr als 30 s fiel. Beide schließen einander über `StatusHelper.RampartStatus` aus.
5. **Reflexion**, nachgeführt oder sobald sie fehlt.

Außerhalb der Einzelabwehr:
- **Kampfrausch** und **Äquilibrium** reaktiv unter je 0,6 Gesundheit (Vorausschau bis zur Landung); Kampfrausch steht
  davor, also wirkt Äquilibrium unter ihm wie von The Balance empfohlen.
- **Kampfrausch** zusätzlich, wenn die Gesundheit beim gemessenen Verlauf **innerhalb seiner Wirkdauer** unter diese
  Schwelle fiele (A239).
- **Urimpuls** reaktiv unter 0,7, solo immer.
- **Holmgang** als Notfall bei „Health of dying tank" (ab Werk 15 %).
- **Abschütteln** als Gruppenheilung und Flächenabwehr.
- **Urflackern**: Abschnitt Nascent Flash.
- **Tiefschlag** und **Zwischenruf**: zentral, nicht auf Bosse.

### Befunde

**1. Urimpuls hielt Verdammnis, Schutzwall und Reflexion zurück — behoben (A238).** Zwei Sperren taten dasselbe: Die
Einzelabwehr brach ab, solange Urimpuls oder Urinstinkt lief (Upstream, seit 141f9b27a), und Urimpuls stand in
`RampartStatus`, der Liste der *großen* Minderungen, die Schutzwall und Verdammnis als Doppelbelegungssperre tragen.
Im Kampf hieß das:
- Am **Pullbeginn**, wenn das Rudel vollzählig ist, kamen Verdammnis und Reflexion erst nach den acht Sekunden von
  Urimpuls.
- Bei **jedem Tankbuster**, für den Urimpuls fiel, kamen Verdammnis und Schutzwall gar nicht. Seit „single enemies" ab
  Werk an ist (A237), wäre das jeder Tankbuster eines einzelnen Bosses gewesen. Die Sperre hätte die Änderung von A237
  ins Gegenteil verkehrt.

Die Liste beschreibt sich selbst als „the big personal mitigations"; Urimpuls ist die kurze Abklingzeit des Kriegers wie
Heart of Corundum, Holy Sheltron und The Blackest Night bei den anderen, und von denen steht keine darin. Der Eintrag
widersprach also dem erklärten Zweck der Liste, und der Code folgt jetzt dem Kommentar. Beide Sperren sind entfernt;
Schutzwall und Verdammnis staffeln weiter gegeneinander. Andere Leser der Liste: `HasMajorMitigation` (nur Dunkelritter,
der nie Urimpuls trägt). Gegenposition: Ohne Sperre überlappen Urimpuls und Verdammnis. Das ist der Stapel „Damnation +
Bloodwhetting" der Referenz und kostet nichts, weil Urimpuls nach 25 s wieder bereit ist.

**2. Kampfrausch fiel nie vor dem Treffer — gebaut (A237, A239).**
- *Tankbuster:* Er fällt vor einem Tankbuster auf ihn, innerhalb seiner Wirkdauer (`TankbusterOnMeWithin`, zentral für
  jeden Tank; Dauer aus den Wirktexten). „Auf ihn" heißt: Marker oder gelisteter Tankbuster-Zauber auf ihm, oder eine
  Vorhersage, die ihn nennt oder, wo sie niemanden nennt, wen sein Ziel anvisiert (Schluss). Nicht unter
  Unverwundbarkeit. Option „Use Thrill of Battle before a tankbuster on you", ab Werk an.
- *Pull (sein Hinweis):* Er fällt, sobald die Gesundheit beim gemessenen Verlauf innerhalb der zehn Sekunden seiner
  Wirkung unter „Thrill Of Battle Heal Threshold" fiele (`GetHealthRatioIn`, die selbstkorrigierte Zeit bis zum Tod).
  Damit liegen die 20 % Gesundheit und die verstärkte Heilung an, solange das Rudel vollzählig zuschlägt, und sie
  tragen die Heilung von Urimpuls und Äquilibrium. Halten die Heiler ihn stabil, ist der Verlauf nicht fallend, und er
  fällt nicht. Keine neue Zahl: Schwelle seine, Dauer aus den Wirktexten. Option „Use Thrill of Battle when your health
  will fall below its threshold within its duration", ab Werk an.
- *Gegenposition, geprüft:* Früher eingesetzt, fehlt er 90 s als Notheilung. Für den Notfall bleiben Äquilibrium (60 s),
  Urimpuls (25 s) und Holmgang. Und reaktiv füllt er dieselben 20 % erst auf, wenn der Treffer gelandet ist; im Pull
  verliert er nichts, wenn er früher kommt, weil der Verlauf den Bedarf schon belegt.
- *Grenze:* Der Verlauf ist linear fortgeschrieben. Ein Rudel, das stirbt, fällt langsamer als vorhergesagt; dann fiel
  Kampfrausch etwas zu früh, in die vorsichtige Richtung.

**3. Urimpuls vor dem Tankbuster eines einzelnen Bosses — umgestellt (A237).** „single enemies" stand ab Werk aus, also
fiel er gegen einen einzelnen Boss erst nach dem Treffer. Nach seiner Regel für Voreinstellungen (A227) steht er an.
*Restrisiko:* Die Einzelabwehr öffnet auch für einen ungelisteten Zauber eines Gegners auf ihn. Dann kann Urimpuls beim
eigentlichen Tankbuster noch abklingen (25 s).

**4. Abtausch auf einem Boss-Tankbuster — behoben (A236).**

**5. Schutzwall erst 30 s nach Verdammnis — zur Entscheidung.** Verdammnis wirkt 15 s; die Staffelung lässt
Schutzwall erst 30 s nach ihr zu. Dazwischen liegen 15 s, in denen keine der beiden wirkt. Umgekehrt wartet Verdammnis bis
60 s nach Schutzwall, der nur 20 s wirkt. Dieselbe Bauform tragen alle vier Tanks (Upstream abe6132d3, „Rampart usage
consistency", ohne Begründung).
- *Im Pull:* Gedeckt sind 0–15 s und 30–50 s; direkt aneinander wären es 0–35 s ohne Lücke, während das Rudel am
  stärksten ist.
- *Am Boss:* Über zwei Minuten ist die gedeckte Zeit gleich, nur verteilt; für Tankbuster gibt es die eigenen Pfade.
- *Empfehlung:* Für alle vier Tanks im Pull (`TankPullOnPlayer`) die zweite große Minderung direkt nach Ablauf der
  ersten; am Boss wie bisher.

**6. Abschütteln hebt Kampfrausch, Verdammnis und Urimpuls auf — Entscheidung offen (TODO), mit neuer Grundlage.**
The Balance wertet die Aufhebung von Kampfrausch als Gewinn: Die Barriere bemisst sich an der erhöhten
Maximalgesundheit, dazu +2 %. Verdammnis aufzuheben kostet bis zu 40 % Minderung für den Rest ihrer 15 s, Urimpuls die
Minderung und die Heilung je Treffer. Seit A237 und A239 liegt Kampfrausch öfter. *Empfehlung:* Abschütteln zurückhalten,
solange Verdammnis/Rachsucht oder Urimpuls läuft, mit Kampfrausch dagegen frei. Die Rückhaltung weicht, wenn die
Gruppe in Gefahr ist (`HoldAreaDefense`).

**7. Reflexion auf einen Tankbuster kann dem Raidwide fehlen — zur Entscheidung, alle Tanks.** Die Einzelabwehr wirkt
Reflexion bei jeder Öffnung, also auch für einen Tankbuster, der nur ihn trifft. Sagt BossModReborn einen Raidwide
innerhalb ihrer Abklingzeit (60 s) voraus, fehlt sie dort der ganzen Gruppe. *Empfehlung:* In der Einzelabwehr
zurückhalten, wenn ein Raidwide innerhalb ihrer Abklingzeit angekündigt ist. Die Rückhaltung weicht, wenn er in Gefahr
ist (`HoldSingleDefense`). Ohne Modul gibt es keine Ankündigung; dann bleibt es wie heute.

**Ohne Befund:**
- *Äquilibrium* (reaktiv, unter Kampfrausch), *Holmgang* (Notfall bei 15 %; vorbeugend nur bei bekannter Höhe des
  Treffers sinnvoll, die fehlt — Abschnitt Nascent Flash).
- *Tiefschlag/Zwischenruf* (zentral) und *Urflackern* (A226).

### Antithesen

- **Kein Defekt:**
  - Für Befund 1 widerlegt: Die Sperre stand im Code, und sie hält Verdammnis bei Pullbeginn um die Laufzeit von
    Urimpuls zurück.
  - Für Befund 2 widerlegt: Kampfrausch fiel nur nach dem Treffer.
- **Option falsch:**
  - Die Näherung „wen sein Ziel anvisiert" kann bei Tankbustern auf den zweiten in der Feindseligkeit irren. Dann geht
    Kampfrausch umsonst, in die vorsichtige Richtung.
  - Mehr Überlappung am Pullbeginn kann bei einem langen Pull hinten Deckung kosten. Gemindert durch die Staffelung von
    Schutzwall und Verdammnis, die bleibt, und durch Urimpuls alle 25 s.
- **Ausgeliefert, und nichts ändert sich:**
  - Ohne Modul, Marker oder gelisteten Zauber gibt es kein Tankbuster-Signal; Kampfrausch fällt dann nach dem Verlauf
    oder reaktiv.
  - Fehlt im Fenster der Einzelabwehr (ab Werk 3 s) ein Einwebeplatz, fällt keine ihrer Minderungen. Das gilt für alle
    Tanks und ist bestehende Bauform.
  - Hat er „single enemies" selbst auf aus gestellt, bleibt Urimpuls gegen den Boss reaktiv.
  - Mit „Heal ahead of incoming damage" aus bleibt die Pull-Regel von Kampfrausch wirksam; sie liest den Verlauf
    unabhängig davon.

## Was offen bleibt

**Der Vorlauf der Uhrregel misst bis zur Entscheidung, nicht bis zum Landen der
Heilung.** Im ungünstigsten Fall muss der laufende GCD auslaufen und ein Zauber mit
Wirkzeit darauf fertig werden — zusammen zwei GCDs. Der Vorlauf muss daher mindestens
zwei GCDs betragen. Wie viele Sekunden das sind, steht nicht fest: Gemessen wird die
**tatsächliche** Erholzeit des Spielers, die das Spiel meldet
(`DataCenter.DefaultGCDTotal` über `ActionManagerHelper.GetDefaultRecastTime`), also
verkürzt Zaubertempo den Vorlauf mit. Gegen die zehn Sekunden von Living Dead
(`ActionId.resx`, Aktion 3638) ist es rund die halbe Phase.

**Der Vorlauf greift nicht, solange der Tod noch erreichbar ist** — das ist Fall 1e der
Tabelle oben, und er ist die Bedingung, unter der der Preis des Vorlaufs überhaupt
tragbar ist. Ohne ihn wird die Startbahn für die Heilung mit genau dem Ergebnis
bezahlt, das die Zurückhaltung erzeugen soll: Ein Träger, der tief genug für den Fall
auf 0 steht, würde in der Vorlaufzeit über die Linie zurückgeheilt, Living Dead liefe
ungenutzt ab, und der Tank hätte seine Unverwundbarkeit für nichts ausgegeben. Die
Grenze ist `HealthForDyingTanks` — der Wert, bei dem die Tank-Rotationen die
Unverwundbarkeit selbst zünden, also die Aussage des Baums über „dieser Tank fällt
gleich". Darüber kommt der Tod nicht mehr, und die Heilung soll laufen, bevor der
Träger wieder sterblich ist; darunter kommt er, und nichts darf ihn aufhalten.

**Verloren geht dabei nichts:** Läuft Living Dead ab, endet die Zurückhaltung mit ihm,
und der Träger ist wieder gewöhnliches Heilziel. Was er nicht mehr kann, ist über die
Schwelle geheilt zu werden, solange der Auslöser noch erreichbar ist.

Ein kürzerer Vorlauf senkt den Preis weiter, lässt die Heilung aber nach Ablauf landen,
wenn der Tank bereits ungeschützt ist. Eine Fähigkeit ohne Wirkzeit würde beides lösen
— welche Heilung gleich fällt, ist jedoch eine Rotationsentscheidung, die die zentrale
Schicht weder kennt noch erzwingen kann.

**Die gestaffelte Phase-2-Unterstützung** (Fälle 4 und 4a) ist nicht gebaut. Fall 4 —
die leichte Unterstützung mit einem HoT — braucht keine Prognose, sondern nur die
Feststellung, dass Walking Dead liegt; ihm steht eine Gesundheitsschwelle entgegen, die
der Träger bei 1 HP nie erfüllt. Fall 4a verlangt eine Kursprognose: Die Datenquelle
steht seit A91, die **Auswertung** zur aufgenommenen Heilmenge ist eine andere als die
zur Restzeit und noch nicht gebaut. Welche Stellen im Code beidem entgegenstehen, führt
`TODO.md`.

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

## Offene Punkte zu diesem Konzept

Sie stehen in `TODO.md` und sind dort unter der Überschrift des Eintrags mit **Konzept:** auf dieses
Dokument gekennzeichnet — an **einer** Stelle statt in zweien, damit keine Kopie altert.
`.github/scripts/audit/check_concept_links.py` listet sie je Konzept und nennt zugleich, wie viele
Einträge überhaupt keinem Konzept zugeordnet sind.

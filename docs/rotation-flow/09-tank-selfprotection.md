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
also weder verzögern noch verdrängen. Die Barriere verschiebt auch nicht mehr die Heilschwelle: Die
Schildanrechnung auf die Heilschwelle ist entfernt (A85). In die Zielwahl geht sie über die effektive
Gesundheit ein (Gefährdungsklasse 1, Konzept 07).

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
| Tankwechsel nach einem Tankbuster (sein Vorschlag): Geteiltes Leid auf den Co-Tank, Zurückprovozieren nach dessen Tankbuster | **gebaut**, Option ab Werk an (A255–A257); Abschnitt „Tankwechsel nach einem Tankbuster" |
| Das geringste Mittel gegen einen gemessenen Tankbuster: Minderung nach Bedarf, gestapelt wo nötig; Unverwundbarkeit nur, wenn nichts Geringeres reicht; der Rest zurückgehalten (seine Vorgabe) | **gebaut**, zwei Optionen ab Werk an (A260, A261); Abschnitt „Das geringste Mittel gegen einen gemessenen Tankbuster" |

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

*Quellenstatus:* Gemeinschaftsquelle — die Tabelle „Barrier Consumption Priority" im consolegameswiki
(abgerufen 02.10.2026), die sich auf einen Lodestone-Blog und einen Beitrag auf X stützt; keine offizielle
Beschreibung. Sie führt The Blackest Night auf Rang 3 und Eukrasian Diagnosis auf Rang 4 und, mit
Differential Diagnosis, auf Rang 24 — beide hinter TBN. Der früher offene Quellenkonflikt (ein Job-Guide
habe Eukrasian Diagnosis vor TBN geführt) ist damit für TBN aufgelöst: Kein Heilerschild verzögert
seine Absorption. Weitere Ränge derselben Tabelle, die andere Regeln berühren: Stem the Tide aus
Bloodwhetting und Nascent Flash Rang 8, Divine Benison 12, Radiant Aegis 18, Shake It Off 19, Divine
Veil 20.

**Der Träger ist nicht zwingend der Dunkelritter.** `ActionId.resx` (Aktion 7393)
beschreibt TBN als „Creates a barrier around **self or target party member**" — die
Barriere kann auf jedem Gruppenmitglied liegen, und der Party-Zweig in `DRK_Reborn` nutzt das mit
`targetOverride: TargetType.LowHP`. Ein Beschwörer kann also Radiant Aegis tragen und zusätzlich TBN
bekommen. Radiant Aegis steht auf Rang 18, also hinter TBN (Rang 3): Sie verzögert die Absorption von TBN
nicht. Was vor TBN steht, sind nur Crest of Time Borrowed und die Tempera-Schilde (Ränge 1 und 2) — eigene
Schilde von Schnitter (Arcane Crest, ab Stufe 84) und Piktomant, die ein Heiler ebenso wenig steuert.

**Damit bleibt kein Grund, den Schild zurückzustellen.** Was bleibt, ist die gewöhnliche
Dringlichkeitsfrage: Ein Träger mit TBN ist bereits geschützt und deshalb weniger dringend
zu versorgen als ein ungeschütztes Gruppenmitglied. Das beantwortet die Zielwahl: `StatusID.BlackestNight` steht in
`StatusHelper.ShieldStatus`, die effektive Gesundheit (`GetEffectiveHp`) zählt die Barriere mit, und die
Gefährdungsklasse 1 ordnet nach ihr (Konzept 07). Die Heilschwelle liest die Barriere nicht; eine frühere
Anrechnung dort verschob, **ob** überhaupt geheilt wird, statt **wen** zuerst, und ist entfernt (A85).

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
| 5 | DRK, TBN aktiv | **nach normaler Regel** | **nach normaler Regel** | **Kein Sonderfall.** Heilung berührt den Auslöser nicht, und ein Heilerschild wird erst nach TBN aufgezehrt. Die Barriere zählt in der effektiven Gesundheit der Zielwahl (Konzept 07), nicht in der Heilschwelle (A85) |
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
| Shadow Wall, Rampart | `RampartStatus` | keine: gelesen als `StatusProvide`, von `HasMajorMitigation` und von `HoldReprisalForRaidwide`, jeweils für den eigenen Charakter |
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
ist das unerheblich, für den Weg **zur aufgenommenen Heilmenge** nicht; Fall 4a beantwortet A147 deshalb nur
näherungsweise, über den Gesundheitsanstieg seit Beginn des Fensters.

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
weniger Schaden (0,9 × 0,9: Minderungen multiplizieren sich, The Balance rechnet Bloodwhetting genauso, „100 x 0.9 x 0.9 = 81“), danach
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
- **Nicht gebaut, jetzt mit Quelle:** Nascent Flash als Minderung für den **anderen Tank vor dessen Tankbuster**.
  The Balance, Warrior Basic Guide, Abschnitt Makros (abgerufen 02.10.2026): „Nascent Flash goes on a friend. Most
  often on the co-tank of your eight-person party." Damit ist der Einsatz am anderen Tank die Regelanwendung der
  Referenz, kein Schluss mehr. Gebaut ist er nicht: Er braucht die Erkennung „Tankbuster auf dem anderen Tank"
  (Marker oder Vorhersage mit Zielangabe) und die Abwägung gegen den eigenen Bedarf, die die Triage oben schon
  führt. Steht im TODO als offene Arbeit. Die Messung der Tankbuster-Höhe bleibt ein eigenes Vorhaben.

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

0. Unter **Holmgang** und unter 30 % Gesundheit fällt nichts weiter (Upstream).
1. **Verdammnis/Rachsucht**, dann **Schutzwall** für einen von BossModReborn vorhergesagten Tankbuster, vor allen
   kurzen (A241). Schutzwall nur, wenn die große Minderung nicht läuft und nicht eben fiel (A243): je Tankbuster eine
   der beiden.
2. **Abtausch** auf einem Rudel gewöhnlicher Gegner für den Slow, Bosse nicht mitgezählt (A236). *Richtig:* Er mindert
   den Treffer nicht, der ihn auslöst. Die zentrale Rückfallstufe, die ihn auf jeden Tankbuster warf, ist geschlossen.
3. **Urimpuls/Urinstinkt** mit „single enemies" (ab Werk an, A237) oder mehr als zwei Gegnern. *Richtig:* Er ist der
   Kern jedes Stapels und heilt im Pull je Treffer.
4. **Kampfrausch** vor einem Tankbuster auf ihn innerhalb seiner Wirkdauer (A237).
5. **Verdammnis/Rachsucht** und **Schutzwall** ohne Vorhersage, gestaffelt: Verdammnis, wenn Schutzwall bereit ist
   oder vor mehr als 60 s fiel; Schutzwall, wenn Verdammnis vor mehr als 30 s fiel (unterhalb der Stufe von Rachsucht
   ohne Bedingung). Beide schließen einander über `StatusHelper.RampartStatus` aus.
6. **Reflexion**, nachgeführt oder sobald sie fehlt; am Tankbuster zurückgehalten, wenn er gedeckt ist und ein
   Raidwide nach ihrem Ende ansteht (`HoldReprisalForRaidwide`, A244).

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
der nie Urimpuls trägt) und seit A244 `HoldReprisalForRaidwide` (alle Tanks, für den eigenen Charakter). Gegenposition: Ohne Sperre überlappen Urimpuls und Verdammnis. Das ist der Stapel „Damnation +
Bloodwhetting" der Referenz und kostet nichts, weil Urimpuls nach 25 s wieder bereit ist.

**2. Kampfrausch fiel nie vor dem Treffer — gebaut (A237, A239).**
- *Tankbuster:* Er fällt vor einem Tankbuster auf ihn, innerhalb seiner Wirkdauer (`TankbusterOnMeWithin`, zentral für
  jeden Tank; Dauer aus den Wirktexten). „Auf ihn" heißt: Marker oder gelisteter Tankbuster-Zauber auf ihm, oder eine
  Vorhersage, die ihn nennt oder, wo sie niemanden nennt, wen sein Ziel anvisiert (Schluss). Nicht unter
  Unverwundbarkeit. Option „Use Thrill of Battle before a tankbuster on you", ab Werk an. Er fällt auch auf einen
  Tankbuster, den Verdammnis schon nimmt. *Verworfen (A248):* ihn dort zurückzuhalten, damit er auf den schwächer
  gedeckten Tankbuster unter Schutzwall fällt, wie WrathCombo es tut („align with Rampart", Stand 25.09.2026,
  keine Spielquelle). Das hilft nur, wenn der nächste Tankbuster binnen seiner 90 s Abklingzeit kommt. Kommen sie
  im Abstand von zwei Minuten, nimmt Verdammnis jeden, und Kampfrausch fiele auf keinen. Den Abstand zum übernächsten
  Tankbuster liefert keine Quelle; BossModReborn sagt nur den nächsten an. The Balance führt „Damnation + Thrill"
  und „Rampart + Thrill + Bloodwhetting" als gleich starke Stapel („The 60s"); eine Zuordnung zu Schutzwall
  nennt sie nicht.
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

**5. Ein vorhergesagter Tankbuster bekam große Minderung und Schutzwall zugleich — behoben (A243).** Beide Zweige
für einen vorhergesagten Tankbuster prüften nur, ob ihr eigener Status bis zum Treffer abläuft, und übergingen die
gegenseitige Sperre. Lagen beide bereit, fielen beide auf denselben Tankbuster.

*Beleg, sein Protokoll vom 01.10.2026, Restaurierter Löwe:* Tankbuster etwa alle 61 s.
- 20:13:43: Verdammnis und Schutzwall, Treffer 30 %.
- 20:14:45: keine von beiden bereit, Treffer 85 %.
- 20:16:02: beide, Treffer 31 %.
- 20:17:05: keine, Treffer 85 %.

Bei Neo Garula dasselbe: 27 % unter beiden, 49 s später 65 % ohne. Abwechselnd eingesetzt (Verdammnis 120 s, Schutzwall
90 s, Job-Guide) wären alle fünf Tankbuster beim Löwen gedeckt gewesen, je mit einer der beiden.

*Referenz:* The Balance empfiehlt je Tankbuster „one of Rampart or your 40 % cooldown, plus your short cooldown".

*Jetzt:* Schutzwall fällt für einen vorhergesagten Tankbuster nur, wenn die große Minderung ihn nicht nimmt — sie läuft
nicht und fiel nicht eben erst (`RampartTakesPredictedTankbuster`, zentral). Das gilt für alle vier Tanks. Die kurzen
Minderungen (Urimpuls, Kampfrausch, Heart of Corundum, Oblation, Sheltron, The Blackest Night) kommen wie bisher dazu.
Das Fenster für Schutzwall ist jetzt seine Dauer aus dem Wirktext statt der Zahl 20.

*Antithesen:*
- *Kein Defekt:* widerlegt durch das Protokoll.
- *Option falsch, ein Tankbuster braucht beide:* möglich bei einem Treffer weit über der Maximalgesundheit. Für diesen
  Fall bleiben Holmgang (bei „Health of dying tank") und die kurzen Minderungen. Ein Tankbuster gleich nach dem ersten
  bekäme mit beiden auf dem ersten gar nichts mehr, wie im Protokoll.
- *Ausgeliefert, nichts ändert sich:* Ohne Vorhersage greifen diese Zweige nicht. Dann staffelt die Zeitregel darunter,
  die beide schon nicht zugleich zulässt.

**Die Zeitregel ohne Vorhersage** (Schutzwall 30 s nach der großen, die große 60 s nach Schutzwall, Upstream
abe6132d3) bleibt unverändert. Durchgerechnet mit `tank_mitigation_stagger_model.py`: Direkt aneinander schützt einen
einzelnen Pull um 2–7 % besser. In einer Wall-to-Wall-Folge verschiebt es nur, welcher Pull ganz ohne beide bleibt; im
Mittel liegen beide Varianten höchstens 3 Prozentpunkte auseinander, mal die eine, mal die andere vorn. Am Boss ohne
Vorhersage ändert die Reihenfolge nichts. Ohne einen Unterschied im Kampf gibt es keinen Grund für einen Eingriff. Die
Zahlen 30 und 60 bleiben als offene feste Werte geführt.

**6. Abschütteln gegen die eigenen Status und gegen den Raidwide — gebaut (A245).** Abschütteln hat zwei Wege, und beide
wirkten es als Erstes, ohne einen eigenen Status zu prüfen:
- *Einzelheilung* (`HealSingleAbility`): Sie öffnet, sobald irgendein Mitglied in der Vorausschau unter
  „HealthSingleAbility" fällt (ab Werk 0,7).
- *Flächenabwehr:* Sie öffnet bei einem erkannten Flächenzauber oder einem angesagten Raidwide.

Wirkung (Job-Guide): Barriere 15 % der Maximalgesundheit je Mitglied, 30 s; Heilung 300 Potenz und Regeneration;
Abklingzeit 90 s. Es hebt Kampfrausch, Verdammnis und Urimpuls auf, je +2 % Barriere. Die Barriere des Kriegers bemisst
sich an seiner Maximalgesundheit vor der Aufhebung (The Balance). The Balance führt Abschütteln unter den
Gruppenwerkzeugen, die auf raidweiten Schaden gehören.

| Fall | Gewinn | Kosten | Jetzt |
|---|---|---|---|
| Einzelheilung, Verdammnis/Rachsucht oder Urimpuls läuft | +2 % Barriere, Heilung für ein Mitglied unter 70 % | bis 15 s −40 % (−30 %) auf ihn, oder Urimpuls' Minderung und Heilung | **wartet**, bis sie enden |
| Einzelheilung, Raidwide nach Ende der Barriere und vor neuer Bereitschaft angesagt | Barriere für eine Einzellage | fehlt der Gruppe am Raidwide | **wartet** |
| Einzelheilung, Kampfrausch läuft | Barriere an der erhöhten Gesundheit, +2 % | Rest von Kampfrausch | frei |
| Raidwide, Verdammnis läuft, kein Tankbuster vor ihrem Ende | Barriere für alle | Rest von Verdammnis | frei |
| Raidwide, Verdammnis läuft **für einen Tankbuster, der vor ihrem Ende landet** | Barriere für alle | 40 % am Tankbuster | **wartet** bis nach dem Tankbuster |
| ein Mitglied in Gefährdungsklasse 1, oder der gemessene Raidwide brächte eines dorthin | — | — | jede Rückhaltung weicht (`HoldAreaDefense`) |

Das folgt seinem Kriterium: Wen bringt der Verzicht in ernste Bedrängnis? Am Raidwide verliert die Gruppe die Barriere,
er den Rest seiner Minderung, also bekommt die Gruppe sie. Steht ihm ein Tankbuster unter Verdammnis bevor, verlöre er
40 % an einem möglicherweise tödlichen Treffer; die Gruppe verliert eine Barriere, und gerät sie dadurch in Gefahr,
weicht die Rückhaltung.

*Antithesen:*
- *Kein Defekt:* widerlegt; die Wege prüften keinen Status.
- *Option falsch:* In der Einzelheilung wartet die Heilung für ein Mitglied unter 70 % bis zu 15 s. Ein Mitglied in
  Gefahr bekommt sie trotzdem (die Rückhaltung weicht); darüber heilen die Heiler.
- *Ausgeliefert, nichts ändert sich:* Ohne Modul gibt es keine Raidwide-Ansage, dann entfällt dieser Teil. Ohne
  sichtbaren Tankbuster entfällt der Teil am Raidwide. Die Statusprüfung wirkt immer.

**7. Reflexion am Tankbuster oder am Raidwide — gebaut (A244).** Reflexion liegt auf dem Gegner und schützt jeden, den er
während ihrer 15 s trifft (sein Hinweis). Eine Wahl gibt es nur, wenn der Raidwide nach ihrem Ende und vor ihrer neuen
Bereitschaft landet. Dann wartet sie, wenn der Tank seinen Tankbuster schon mit eigener großer Minderung oder Schutzwall
nimmt. Fälle, Quellen und Grenzen in Konzept 08, „Gegner-Debuffs am Tankbuster und am Raidwide", weil die Regel dort für
alle Rollen sitzt.

**8. Die große Minderung kam beim Tankbuster zuletzt — behoben (A241).** Sein Protokoll, 21:11:58: Marker auf ihm, dann
Urimpuls, Kampfrausch und Verdammnis erst 0,7 s vor dem Treffer. Das Fenster der Einzelabwehr öffnet bei einer
Vorhersage wenige Sekunden vorher, und die kleinen Minderungen standen vorn. Jetzt stehen die Zweige für einen
vorhergesagten Tankbuster bei allen vier Tanks am Anfang. Zusammen mit Befund 5 fällt dort je Tankbuster eine große
Minderung oder Schutzwall, dann die kurzen.

**Ohne Befund:**
- *Äquilibrium* (reaktiv, unter Kampfrausch), *Holmgang* (Notfall bei 15 %; vorbeugend nur bei bekannter Höhe des
  Treffers sinnvoll, die fehlt — Abschnitt Nascent Flash).
- *Tiefschlag/Zwischenruf* (zentral) und *Urflackern* (A226).

### Abgleich mit den Referenzen (02.10.2026)

| Frage | The Balance | WrathCombo (Stand 25.09.2026, keine Spielquelle) | xivanalysis | RSR jetzt |
|---|---|---|---|---|
| Große Minderung und Schutzwall auf einen Tankbuster | je Tankbuster eine von beiden plus kurze | Vengeance nicht innerhalb 20 s nach Rampart und umgekehrt („Prevent double big mits") | — | eine je Tankbuster (A243) |
| Urimpuls vor dem Tankbuster | Teil jedes Stapels | auf angekündigten Tankbuster | zählt nur die Nutzung | ja, „single enemies" an (A237) |
| Kampfrausch vor dem Tankbuster | in den Stapeln, mit Verdammnis wie mit Rampart („The 60s“) | nur ohne andere große oder mit Rampart, sonst Notfall | zählt nur die Nutzung | auf jeden Tankbuster, für den er bereit ist (A237); WrathCombos Ausrichtung verworfen (A248) |
| Reflexion am Tankbuster | Gruppenwerkzeuge auf Raidwides | im Bosskampf nur bei angesagtem Gruppenschaden | — | wartet, wenn der Tank gedeckt ist (A244) |
| Abschütteln | Barriere an erhöhter Gesundheit | im Bosskampf bei Gruppenschaden, nicht innerhalb 10 s nach Reflexion; außerhalb nur ohne Kampfrausch, Verdammnis, Vengeance, Urimpuls | zählt nur die Nutzung | wartet bei Verdammnis/Urimpuls (A245) |
| Reflexion und Abschütteln auf demselben Raidwide | mal stapeln, mal verteilen, je nach Größe | verteilt (je 10 s Sperre gegeneinander) | — | **beide auf denselben** — offen (TODO) |

*xivanalysis* bewertet Abwehr nur danach, ob sie genutzt wurde („find helpful times"), ohne Zeitpunktregel. *FFLogs*
liefert die Kampfdaten, keine Regeln; ein Report von ihm würde zeigen, welche Minderung auf welchem Treffer lag.

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

## Tankwechsel nach einem Tankbuster: Geteiltes Leid auf den anderen Tank

**Sein Vorschlag (04.10.2026), als Option geprüft:** „bei inhalten mit mehr als einem tank (also keine instanzen
mit wall to wall), wenn gerade ein tankbuster auf dich erfolgt ist und du danach kritische gesundheit hat, bzw. einen
debuff, der einen zweiten tankbuster auf dich tödlich enden läßt (also auch solche spezialeffekte wie holmgang,
totenerweckung etc. auf cooldown sind), dann geteiltes leid auf den tank casten, welcher am wenigsten aggro hat bzw.
die höchste gesundheit."

**Seine Angaben dazu (04.10.2026):**
- *Präzisierung:* „wenn unverwundbarkeit bereit ist, dann braucht aggro nicht gewechselt werden."
- *Hinweis:* „es kann ja umgekehrt sein, dass der andere tank sterben könnte, weil seine unverwundbarkeit nicht bereit
  ist, und er ebenfalls geteiltes leid auf mich gewirkt hat."
- *Vorgabe:* „weiterhin sollte erst dann mit provozieren wieder aggro aufgebaut werden, wenn debuff und kritischer
  zustand abgelaufen sind."
- *Präzisierung dazu:* „am besten, wenn auch unverwundbarkeit wieder bereitsteht."
- *Präzisierung (als Frage gestellt, geprüft und übernommen):* „müsste das zurückprovozieren nicht erst dann
  erfolgen, wenn auch der andere tank einen tankbuster bekommen hat?"

**Gebaut (A255–A257), Option „Shirk the co-tank after a tankbuster that leaves you in danger", ab Werk an:**
Erkennung in `TankSwapWatch`, Entscheidung in `CustomRotation.TankSwapAbility`, für alle Tanks an einer Stelle (Stufe
Tanks; Geteiltes Leid ist eine Rollenaktion, für keinen Tank-Job gilt etwas anderes). Geteiltes Leid geht auf den
anderen Tank, wenn alles zutrifft:

1. **Ein Tankbuster hat dich getroffen.** Tankbuster heißt: die Aktion steht in der Tankbuster-Liste, oder ein
   Tankbuster-Marker stand auf dir, der nicht auf der gelernten Liste der Marker ohne Treffer steht. Gemessen werden
   der Schadensanteil an deinen maximalen LP und die Status, die genau dieser Treffer auf dich legt (Effektsatz).
2. **Ein zweiter Treffer wäre tödlich.** Entweder trägst du noch eine Verwundbarkeit, die der Tankbuster gelegt hat
   (`VulnerabilityUp`, `PhysicalVulnerabilityUp`, `MagicVulnerabilityUp` – erkannt an den Bezeichnern, die der
   Generator aus dem englischen Statusblatt bildet, also unabhängig von der Client-Sprache). Oder LP und Schild zusammen liegen nicht über dem
   härtesten Anteil, der für diese Aktion in diesem Kampf auf dir gemessen wurde, oder der Vorhersage der
   Tankbuster-Tabelle für dich unter der jetzt stehenden Minderung, je nachdem, was höher ist (Konzept 13, „Die
   Tankbuster-Tabelle"; „eine Wiederholung bringt dich um").
   Das zweite Kriterium erlischt von selbst, sobald du über diese Linie geheilt bist.
3. **Deine Unverwundbarkeit ist nicht verfügbar.** Verfügbar heißt hier: aktiviert, erlernt, abgeklungen und mit
   einer Schwelle über 0 (`HealthForDyingTanks`). Unter einer laufenden Unverwundbarkeit, Living Dead oder Walking
   Dead geschieht nichts.
4. **Der andere Tank behält den Gegner danach.** Das ist gemessen, nicht angenommen; siehe Mechanik.
5. **Der andere Tank ist nicht selbst in Gefahr** (sein Hinweis). Er trägt keine Verwundbarkeit, und seine LP mit
   Schild liegen über dem, was dieser Tankbuster ihn erwartbar kostet: dem härtesten gemessenen Anteil oder der
   Vorhersage der Tankbuster-Tabelle unter seiner jetzt stehenden Minderung, gerechnet auf seine maximalen LP. Ob
   *seine* Unverwundbarkeit bereit ist, kann RSR nicht lesen: Die Abklingzeiten anderer Spieler liegen nicht vor.
   Gemessen wird deshalb, ob ein Treffer ihn umbrächte, nicht, ob er sich retten könnte. Hat er dir den Gegner mit
   Geteiltem Leid gegeben, weil er in Gefahr ist, zeigt genau diese Messung das, und du gibst ihn nicht zurück.
   Seine Geste selbst schreibt das Protokoll nur mit („… shirked to you").

**Zurückholen (seine Vorgabe und Präzisierungen):** Hat dein Geteiltes Leid den Gegner bewegt, provoziert RSR ihn
zurück, sobald
- der Tank, der ihn jetzt hält, den nächsten Tankbuster dieses Gegners genommen hat,
- die Verwundbarkeit abgelaufen ist und LP mit Schild über dem liegen, was eine Wiederholung erwartbar kostet
  (wie oben), und
- deine Unverwundbarkeit wieder bereit ist. Wo RSR sie gar nicht einsetzen würde (auf dieser Stufe nicht erlernt,
  abgeschaltet, Schwelle 0), wartet es darauf nicht.

Warum erst nach seinem Tankbuster: Ein Wechsel folgt dem Takt der Tankbuster. Früher zurückgeholt, nähmst du den
nächsten selbst, und die Abklingzeiten, die der Co-Tank dafür bereithält, blieben ungenutzt; der Gegner drehte
einmal mehr als nötig. Danach wechseln sich die Treffer ab, und trägt er nun selbst eine Verwundbarkeit, ist das
genau der Moment, in dem er dich braucht. Kommt lange kein Tankbuster, behält er den Gegner. Das ist ungefährlich,
denn er ist ein Tank, und fällt er unter die Sterbe-Schwelle, nimmt die automatische Herausforderung ihm den Gegner
ab, sofern du nicht selbst in Gefahr bist. Trifft sein Tankbuster, solange dein Debuff noch läuft, wartet das
Zurückholen auf deine übrigen Bedingungen.

Greift der Gegner dich schon wieder an, ist nichts zurückzuholen. Zurückgeholt wird nach jedem Wechsel, bei dem
dein Geteiltes Leid ausgeführt wurde und der Gegner danach den Empfänger angreift – auch wenn der Co-Tank vorher
provoziert hatte. Hat er ihn ohne dein Geteiltes Leid übernommen, holt RSR ihn nicht zurück: Das war ein geplanter
Wechsel.

**Kein Zurückholen, solange du in Gefahr bist:** Die automatische Herausforderung nimmt einem Co-Tank den Gegner ab,
der unter `HealthForDyingTanks` fällt (`ObjectHelper.CanProvoke`). Solange du selbst nach einem Tankbuster in Gefahr
bist, tut sie das nicht mehr. Ohne diese Sperre hätte sie dir den Gegner zurückgeholt, während dein Debuff noch
läuft – genau das, was seine Vorgabe ausschließt. Diese Sperre gilt unabhängig von der Option, weil sie eine
bestehende Regel an seine Vorgabe bindet.

**Die Zielwahl:** unter den anderen Tanks der Gruppe die, die lebend, anvisierbar, in Reichweite sind, selbst keine
Verwundbarkeit tragen und den Gegner nach der Übertragung behalten würden. Davon der, den die wenigsten Gegner
angreifen, bei Gleichstand der mit den meisten LP. „Am wenigsten Aggro" lese ich als „am wenigsten belastet" (meine
Ableitung, nicht seine Regel): Wörtlich – die geringste Feindseligkeit auf diesem Gegner – wäre es genau der Tank, bei
dem Geteiltes Leid am wenigsten bewirkt. Die Frage stellt sich nur bei drei und mehr Tanks in einer Gruppe; in Achter-
Inhalten gibt es genau einen anderen.

### Mechanik (Belege, abgerufen 04.10.2026)

- **Geteiltes Leid** (Shirk, #7537): „Du überträgst 25 % deiner Feindseligkeit auf das ausgewählte Gruppenmitglied",
  St. 48, 120 s, 25 y (Job-Guide deutsch und englisch). Nicht auf dich selbst, nicht auf Allianzmitglieder (xivapi:
  `CanTargetSelf`, `CanTargetAlliance` falsch). In Allianzraids ist der andere Tank also nie erreichbar.
- **Herausforderung** (Provoke, #7533) setzt den Wirker an die Spitze der Feindseligkeitsliste – das Werkzeug der
  *anderen* Seite eines Tankwechsels.
- **Wann Geteiltes Leid allein den Gegner bewegt:** Danach hältst du (1 − s) deiner Feindseligkeit, der Empfänger
  gewinnt s davon. Er wird zum Ziel, wenn er danach vor dir und vor allen anderen liegt; von dir aus gerechnet
  braucht er vorher mehr als 1 − 2s, bei s = 25 % also mehr als die Hälfte deiner Feindseligkeit. s kommt aus dem
  Wirktext über den Generator (`DefensiveValues.EnmityTransferOf`), nicht aus dem Code.
- **Die Feindseligkeit der Gruppe** auf dein aktuelles Ziel liest das Spiel selbst (`UIState.Hate`, je Mitglied 0 bis
  100 relativ zur Spitze – so in FFXIVClientStructs dokumentiert, Gemeinschaftsquelle, und so liest sie WrathCombo).
  Ist dein Ziel nicht die Quelle des Tankbusters, ist die Zahl nicht lesbar, und die Regel hält.
- **The Balance** führt den Tankwechsel als letzte Stufe nach der Unverwundbarkeit („It Still Kills Me: Holmgang
  (invulnerability), Tank Swap (mechanical requirement)", Krieger-Leitfaden) und ein Makro für Geteiltes Leid auf
  den Co-Tank. WrathCombo kennt Geteiltes Leid nur als Zielumlenkung beim manuellen Drücken, keine eigene Auslösung.

### Wo die Umsetzung von seinem Wortlaut abweicht

**Verengung, mit Grund:** Liegt der andere Tank bei höchstens der Hälfte deiner Feindseligkeit, wirkt die Regel
nicht, obwohl seine Bedingungen erfüllt sind. Geteiltes Leid würde dort nichts daran ändern, wer den nächsten Treffer
nimmt. Es kostete aber die Aktion für 120 s und fehlte beim geplanten Wechsel, wenn der Co-Tank provoziert und du
mit Geteiltem Leid nachlegen sollst. Hat der Co-Tank schon provoziert, liegt er an der Spitze; dann geht Geteiltes
Leid sofort und festigt den Wechsel, wie es die Referenz vorsieht.

### Wechselwirkungen und Lagen

- **Zwei RSR-Tanks:** Nur der Getroffene wirkt. Der Empfänger gibt dir den Gegner nicht zurück, solange du eine
  Verwundbarkeit trägst oder eine Wiederholung nicht überlebst – ein Tank in Gefahr ist kein Empfänger. Sind beide
  in Gefahr, wirkt keiner, und keiner provoziert.
- **Haltung:** RSR schaltet die Haltung nur ein, wenn kein anderer Tank sie trägt. Ein RSR-Co-Tank liegt daher
  meist weit unter der Hälfte, und die Regel hält. Das ist richtig: Ohne Haltung behielte er den Gegner nicht.
- **Automatische Herausforderung** (`ObjectHelper.CanProvoke`, bei zwei Tanks nur mit „Auto provoke when there is
  another tank in party") greift bei Gegnern auf Nicht-Tanks und nimmt einem Co-Tank unter `HealthForDyingTanks`
  den Gegner ab. Den zweiten Fall sperrt die Regel, solange du selbst in Gefahr bist (oben, „Kein Zurückholen").
- **Allein, Vierer-Instanz, Allianzraid:** Es gibt keinen anderen Tank in der Gruppe, die Regel ist still.
- **BossMod ohne Modul:** Die Regel liest keine Vorhersage; Erkennung über Liste, Marker und Effektsatz.
- **Stufensynchron:** Geteiltes Leid erst ab St. 48 (`EnoughLevel`), darunter still.

### Antithesen

1. **Kein Bedarf, der Co-Tank provoziert ohnehin.** Bei geplanten Wechseln ja; dann festigt die Regel nur, was
   ohnehin geschieht. Den Ausschlag gibt der ungeplante Fall: Tankbuster in Unterzahl der Abklingzeiten. Seine
   Kämpfe vom 01.10.2026 zeigen „Schramme" zweimal mit 85 %, beide Male ohne vorher gewählte Abwehr; die 30 bis
   39 % davor und danach kamen nach Verdammnis und Schutzwall. Dass beim 85-%-Treffer keine Minderung bereit war,
   ist daraus geschlossen, nicht gemessen. Ob dort ein zweiter Tank in der Gruppe war, zeigt das Protokoll nicht. Entkräftet, aber die Häufigkeit ist offen.
2. **Falsches Werkzeug, Herausforderung des Co-Tanks wirkt sicher.** Stimmt, aber die drückt der andere Spieler.
   Die Gegenseite gibt es zum Teil schon: Fällt der Co-Tank unter `HealthForDyingTanks`, provoziert RSR. Fehlt
   noch der Fall, dass er nach einem Tankbuster eine Verwundbarkeit trägt, aber nicht tief steht. Das steht als
   eigener Eintrag in `TODO.md`.
3. **Ausgeliefert, und nichts ändert sich.** Bei Spielern, deren Co-Tank ohne Haltung spielt, wirkt die Regel nie.
   Das ist so gebaut, weil sie dort nichts bewirken könnte. Damit das von außen unterscheidbar bleibt, schreibt das
   Protokoll je Tankbuster einen Grund. Eine falsche Prognose zieht die Regel selbst nach (unten).
4. **Das Zurückholen kommt zu früh oder unnötig.** Ohne Bindung an den Tankbuster des Co-Tanks zog es einen
   geplanten Rückwechsel vor; diese Antithese hielt und hat die Bedingung geändert (seine Frage). Mit der Bindung
   folgt es dem Takt der Treffer. Dass der Wechsel bei bereiter Unverwundbarkeit hält, trägt seit A260 auch gegen
   einen tödlichen Treffer von oben: Sie geht vor einem gewirkten Tankbuster aus, den die Tabelle selbst im besten
   Fall für tödlich hält (Abschnitt „Das geringste Mittel gegen einen gemessenen Tankbuster"). Offen bleibt das für
   Tankbuster, die nur ein Marker oder BossModReborn ankündigt, und für Aktionen ohne Messung; dort zündet sie weiter
   erst unter der Sterbe-Schwelle.

### Messmittel und Nachsteuerung

`DefenseTrace.log` schreibt je erkanntem Tankbuster „tankbuster on you: … vulnerability …". Hält die Regel, schreibt
sie einmal je Tankbuster „tank swap held: …" mit dem Grund (Unverwundbarkeit bereit, kein anderer Tank, Ziel nicht
die Quelle, Feindseligkeit zu gering, Geteiltes Leid im Abklingen). Wählt sie, schreibt sie „tank swap (…) ->
Shirk" mit den Feindseligkeitswerten; ist die Aktion ausgeführt, folgt die „used"-Zeile. Den Ausgang misst der erste
Auto-Angriff der Quelle danach: „moved the enemy" oder „did not move". Im zweiten Fall verlangt jeder weitere
Wechsel in diesem Kampf mehr als das Verhältnis, das versagt hat. Gezählt wird erst ab der Ausführung, eine Wahl
ohne Ausführung verschiebt nichts. Den Erfolg zeigt schon das Ziel des Gegners: Wechselt es auf den Empfänger, gilt
der Wechsel sofort als gelungen. Wartet das Zurückholen, steht einmal je Wechsel „tank swap back waits: …"
(der Co-Tank hat noch keinen Tankbuster genommen, Unverwundbarkeit noch nicht bereit, Herausforderung im
Abklingen oder außer Reichweite); trifft der Tankbuster den Co-Tank, steht „tankbuster on …; holt RSR zurück, steht
„tank swap back … -> Provoke". Ob die Geste des Co-Tanks (Geteiltes Leid auf dich) als eigener Effektsatz bei dir
ankommt, ist nicht belegt; fehlt die Zeile, ändert das nichts an der Entscheidung.

**Prüfgrad:** statisch, Prüfskripte, Compile über die CI; im Spiel nicht beobachtet.

## Das geringste Mittel gegen einen gemessenen Tankbuster

**Seine Angaben (04.10.2026):**
- *Hinweis:* Ob ein Tankbuster tödlich ist, lässt sich ohne den erwarteten Schaden nicht sagen – erst nach dem
  Einschlag oder aus Erfahrungswerten. Eine Unverwundbarkeit bei jedem Tankbuster wäre schädlich. Die Erfahrungswerte
  liefert die Tankbuster-Tabelle (Konzept 13).
- *Vorschlag, als Prüfaufgabe übernommen:* „rein ökonomisch … für einen tankbuster, welcher 100% tödlich ist, nur
  noch invul nimmt und alle anderen schilde, debuffs und mitigations wegläßt". Bestätigt: „das war der sinn der
  idee."
- *Präzisierung:* „ich habe nie gesagt, dass bei tankbustern nur eine große minderung erlaubt ist." Die Staffelung
  aus A243 war eine Abwägung ohne Kenntnis der Stärke, keine Regel von ihm.
- *Vorgabe:* „wenn klar ist, dass man den tankbuster nur mit zwei aktuell verfügbaren (also nicht auf cooldown)
  minderungen überleben kann, dann sollte man diese auch bei verfügbarkeit nutzen. gerade dann, wenn invul
  aufgebraucht ist. und selbst wenn invul verfügbar ist, gilt immer das geringste notwendige mittel und invul ist
  ‚mit kanonen auf spatzen schießen'. manchmal absolut notwendig, manchmal das mittel zweiter wahl, wenn es anders
  auch geht (aber dann ohne risiko)."
- *Hinweis:* Externe Effekte sind mitzubewerten – ein Debuff eines anderen auf dem Boss, der beim Einschlag noch
  wirkt; ein besonders großer Heilerschild auf dem Tank, der beim Einschlag noch steht.
- *Präzisierung zu „alles":* „nicht invul und dann noch zusätzlich buffs, sondern alles raushauen, wenn invul nicht
  verfügbar ist, alle buffs zusammen aber eigentlich nicht reichen, aber man hofft, dass irgendjemand anderes noch
  einen debuff oder ein schild raushaut."

**Gebaut (A260, A261), zwei Optionen, ab Werk an:**
- „Spend only the mitigation a measured tankbuster needs" (Planung, Stapeln, Zurückhalten; Tanks und Heiler).
- „Use the invulnerability before a tankbuster nothing less survives" (nur Tanks).

Erkennung in `TankbusterForecast`: je Wirken auf dich der erwartete Anteil beim Einschlag und was du dagegen hast;
sie liest keine Option und wählt keine Aktion. Entscheidung in `CustomRotation_Tankbuster.cs`:
`UpdateTankbusterPlan` (je Durchlauf), `TankbusterPlanAbility` (Ausführung, Fähigkeiten-Slot 9) und
`HoldDefenceForTankbuster` (Zurückhalten, je Durchlauf in `IBaseAction.HoldDefenceOnSelf`, Gatter in
`BaseAction.CanUse`, Ablehnung „HeldForTheInvulnerability"). Für Heiler `StateUpdater.ShouldAddDefenseSingle`.
Stufe Tanks: Die Rechnung ist für alle vier gleich, nur die Aktionen unterscheiden sich, und die liest sie aus den
Wirktexten. Der Heiler-Teil sitzt auf der Stufe Heiler; Damage Dealer betrifft die Regel nicht.

### Die Rechnung

Für den frühesten gemessenen Tankbuster, der auf dich gewirkt wird:

1. **Was er nimmt:** der höchste je gemessene ungeminderte Wert der Aktion (Konzept 13), mal jede Minderung, die beim
   Einschlag noch steht – deine, die Reflexion des Co-Tanks, ein Zermürben oder Stumpfsinn auf dem Boss, eine Minderung
   eines Heilers auf dir. „Beim Einschlag" heißt: Restzeit des Wirkens plus ein GCD für das Eintreffen; ein Status,
   der vorher endet, zählt nicht (`TankbusterTable.PredictedShare` mit Horizont).
2. **Was du hast:** deine LP jetzt plus die Barrieren, die beim Einschlag noch stehen. Läuft eine Barriere vorher ab,
   zählt keine (`HasSurvivingShield`: die früheste Barriere entscheidet). Ein großer Heilerschild, der hält, zählt
   also voll – sein Hinweis.
3. **Was du noch tun kannst:** jede eigene Minderung, die erlernt, aktiviert und nach ihren eigenen Prüfungen
   nutzbar ist (Ressourcen eingeschlossen), bis einen GCD vor dem Einschlag abgeklungen ist, eine bekannte Wirkdauer
   von mindestens einem GCD hat (Reflexion und Schiltron nennen in der Tabelle keine, `TODO.md`), nach Wirktext gegen diese Schadensart mindert oder eine Barriere legt und beim Einschlag nicht schon steht.
   Ein Knopf zählt einmal (Rachsucht/Verdammnis, Urinstinkt/Urimpuls). In dem Moment zwischen dem
   Effektpaket des Servers (die eigene Minderung ging auf dich hinaus) und dem ersten Erscheinen ihres Status zählt
   sie schon als stehend, nach ihrer Wirkdauer ab der Ausführung (`TankbusterForecast.OwnPendingCover`, aus dem
   Effekt-Handler mit Ziel und Zeitpunkt). Ist der Status einmal gesehen, sagt nur noch er, ob sie steht – auch wenn
   sie vorzeitig endet (Barriere gebrochen, Abschütteln). Weder ein `CanUse`-Treffer noch eine Abklingzeit taugt
   dafür: Der erste ist noch kein Druck, die zweite sagt weder, wer drückte, noch auf wen (The Blackest Night auf dem
   Co-Tank), und Rachsucht/Verdammnis teilen sie. Ob der neue Status erschienen ist, erhebt die Erkennung in jedem
   Durchlauf aus der Statusliste selbst (geschützt gelesen, Quelle der Spieler; nicht der Status, den das
   Bestätigungspaket vorhersagt): eine eigene Kopie dessen, was die Aktion ihrem Nutzer gibt, auf dir (Holmgang,
   worauf es auch gerichtet war), oder dessen, was sie dort legt, wo sie landet – auf ihrem Ziel oder bei Wirkung um
   dich (Wirkradius laut Spieldaten) auf einem beliebigen Gegner –, die länger läuft als eine eigene Kopie dort beim
   Hinausgehen lief; bei mehreren Status einer Aktion zählt der längste. Gruppenwerkzeuge werden nicht verfolgt. Eine
   Erneuerung zählt so erst, wenn die erneuerte Kopie liegt; lag sie beim Bestätigen schon (mehr als Wirkdauer weniger
   ein GCD übrig), gilt sie sofort als gesehen. Reflexion wird überbrückt, bis ihr
   Debuff auf einem Gegner erscheint; erreichte sie diesen nicht, sagt danach sein Fehlen. **Nicht** dabei sind Gruppenwerkzeuge – Barriere oder
   Minderung über die Gruppe (Abschütteln, Divine Veil, Dark Missionary, Heart of Light): Sie gehören der
   Flächenabwehr, und Abschütteln hebt Verdammnis und Urimpuls des Kriegers selbst auf. Reflexion, ein Debuff auf dem
   Gegner, ist dabei.
4. **Das geringste Mittel:** unter allen Bündeln dieser Minderungen das billigste, mit dem der Treffer weniger nimmt,
   als du hast. Billig nach der Abklingzeit des Verbrauchten, bei Gleichstand das kleinere Bündel – was am wenigsten
   Abklingzeit kostet, lässt am meisten für den nächsten Tankbuster (meine Lesart von „geringstes notwendiges
   Mittel", nicht seine Regel). Reicht der Treffer schon ohne alles nicht zum Tod, ist das Bündel leer.
5. **Die Unverwundbarkeit** nur, wenn kein Bündel reicht – und nur, wenn sie bis einen GCD vor dem Einschlag bereit
   ist. „Mit kanonen auf spatzen schießen" sonst.
6. **Alles,** nur wenn kein Bündel reicht und die Unverwundbarkeit nicht zu haben ist – auf Abklingzeit,
   abgeschaltet oder vom Spiel verweigert (seine Präzisierung). Nie die Unverwundbarkeit und die Minderungen dazu.
   Nach den Zahlen überlebst du dann nicht, aber die Zahlen sind ein Höchstwert, und vielleicht legt ein anderer noch
   einen Debuff oder Schild.

Hält schon eine Unverwundbarkeit über den Einschlag – vom Plan gezogen oder an ihrer Sterbe-Schwelle; ihr Status
liegt, oder der Server hat den Druck bestätigt, der Status ist noch nicht erschienen und ihre Wirkdauer reicht –, ist
der Plan „die Unverwundbarkeit, nichts sonst". Holmgang schützt seinen Nutzer, worauf es auch gerichtet war.

**Ohne Risiko, wie er es für jedes Mittel unterhalb der Unverwundbarkeit verlangt:** Gerechnet wird mit dem höchsten
je gemessenen Treffer, den LP jetzt und nur dem, was beim Einschlag noch steht. Eine Heilung, die bis dahin kommen
könnte, zählt nicht – sie ist eine Annahme über das, was jemand tun wird. Was schon steht, ist keine Annahme: Das
unterscheidet die Fremdeffekte seines Hinweises von einer erhofften Hilfe.

Der Plan wird je Durchlauf neu gerechnet. Heilt ein Heiler dich vor dem Einschlag hoch oder legt der Co-Tank
Reflexion, schrumpft er; fallen deine LP durch Auto-Angriffe, wächst er. Bereits gedrückte Minderungen stehen dann
und zählen unter 1. mit.

### Das Zustandsmodell je Wirken

Maßgeblich für Code und Prüfung; die Abschnitte davor und danach beschreiben Teile davon und dürfen ihm nicht
widersprechen. Gerechnet wird in jedem Durchlauf neu, nur für Tanks und nur, wenn mindestens eine der beiden Optionen
an ist.

**Die Tatsachen (Erkennung, `TankbusterForecast`, liest keine Option):**
- je Wirken auf dich: Nummer, Restzeit R, Horizont H = R + ein GCD, erwarteter Anteil P unter dem, was über H steht,
  und was du hast, B = LP jetzt plus Barrieren, die über H stehen;
- Status nur aus der Statusliste selbst – nie der Status, den ein Bestätigungspaket vorhersagt (`DataCenter.ApplyStatus`
  meldet ihn mit unendlicher Restzeit und hielte so alles für „steht über H");
- die eigenen Drücke (RSR hat die Aktion ausgeführt, `BaseAction.Use`) und die eigenen Ausführungen, die der Server
  bestätigt hat, je mit Ziel, Zeitpunkt und ob ihr Status seither erschienen ist – unabhängig davon, ob die Aktion
  gerade Kandidat sein könnte (abgeschaltet, verweigert, per Befehl gedrückt).

**Wann eine eigene Aktion als stehend zählt, ohne dass ihr Status schon liegt:** ab dem eigenen Druck höchstens einen
GCD lang, solange keine Bestätigung kam; ab der Bestätigung bis ihr Status erscheint, solange ihre Wirkdauer über H
reicht. Danach sagt nur der Status. So gibt es keinen Augenblick, in dem eine eben gedrückte Minderung weder Kandidat
noch stehend ist.

**Die Zustände** für das früheste gemessene Wirken c:

| Zustand | Wann | Was gedrückt wird | Was zurückgehalten wird |
|---|---|---|---|
| Z0 kein Plan | kein gemessenes Wirken auf dich | nichts | nichts; Heiliger Boden/Meteoritenfall über den nächsten GCD: alles |
| Z1 gedeckt | eine Unverwundbarkeit steht über H (Status oder eigener Druck, siehe oben) | nichts | alles Bewertete auf dich |
| Z2 Bündel S | das billigste Bündel S übersteht c; S darf leer sein | jedes Glied von S in seinem Fenster | alles Bewertete auf dich außer S; ein Glied von S erst in seinem Fenster, aus jedem Pfad; im letzten GCD nichts, wenn ein Glied fehlt |
| Z3 Unverwundbarkeit | kein Bündel übersteht c, sie ist einen GCD vor dem Einschlag bereit, für dieses Wirken nicht ausgeschlossen | sie, in ihrem Fenster | alles Bewertete auf dich, solange R > GCD |
| Z4 alles | kein Bündel, keine Unverwundbarkeit | jedes verfügbare Glied in seinem Fenster | nichts |

**Die Optionen:** „Spend only the mitigation a measured tankbuster needs" trägt die Minderungsdrücke in Z2 und Z4
und jedes Zurückhalten; ohne sie drückt der Plan keine Minderung und hält nichts zurück. „Use the invulnerability
before a tankbuster nothing less survives" trägt Z3; ohne sie wird aus Z3 Z4.

**Wann zurückgehalten wird:** nur, wenn c das einzige bekannte Wirken auf dich ist – kein zweites Wirken auf dich,
gemessen oder nicht, und keine BossModReborn-Vorhersage vor c (bei aktivem „Use BMR timeline", gleich wie weit
voraus). Ein Tankbuster-Marker auf dir, während c läuft, gilt als der von c. Z1 hält auch neben einem zweiten Wirken
zurück, wenn die Deckung über jedes Wirken reicht; deckt sie nur c, hält nichts zurück. Frei bleiben immer: Hilfe für
andere, die Heilpfade, nach außen wirkende Flächenabwehr, jeder Befehl.

**Die Übergänge** – jeder Durchlauf rechnet den Zustand aus den Tatsachen; gespeichert wird nur:
- *Verweigerung:* Lehnt eine Aktion ab, ohne dass Animationssperre oder eigener Zauber das erklärt, wird sie einen GCD
  lang nicht gedrückt. Für die Frage, ob ein Bündel reicht, zählt sie weiter, solange bis zum letzten GCD vor dem
  Einschlag noch ein neuer Versuch bleibt (R > zwei GCD); bei der Wahl werden Bündel ohne verweigerte Glieder
  vorgezogen. Eine einmalige Verweigerung führt so nicht zur Unverwundbarkeit; eine, die bis zuletzt anhält, schon.
  Ist die Unverwundbarkeit selbst verweigert, wird aus Z3 für diesen GCD Z4.
- *„Alles" ausgeschöpft:* Ging ein Druck aus Z4 hinaus (vom Ausführungsprotokoll bestätigt), ist Z3 für dieses Wirken
  ausgeschlossen (offene Entscheidung D1a).
- Alles Übrige ergibt sich aus den Tatsachen: Heilung hebt B (Z3 → Z2 möglich, solange die Unverwundbarkeit nicht
  gedrückt ist), Auto-Angriffe senken B (größeres S, Z3), eine fremde Reflexion senkt P, ein gedrücktes Glied von S
  steht und fällt aus den Kandidaten.

**Invarianten:**
1. Die Unverwundbarkeit wird nie geplant, solange ein Bündel reicht, dessen Glieder bis zum letzten GCD noch gedrückt
   werden können – auch eines mit einem eben verweigerten Glied.
2. Steht eine Unverwundbarkeit über H oder ist sie für c gedrückt, geht für c keine Minderung mehr hinaus, aus keinem
   Pfad (Z1). Holmgang und Totenerweckung eingeschlossen: Sie sichern das Überleben; wie viele LP nach ihrem Ende
   bleiben, ist die offene Reserve-Frage (`TODO.md`).
3. Das Zurückhalten trifft nie Heilung, Hilfe für andere, nach außen wirkende Flächenabwehr oder einen Befehl.
4. Ein Glied von S geht aus keinem Pfad vor seinem Fenster hinaus; fehlt im letzten GCD eines, endet das Zurückhalten.
5. Ein Kandidat braucht eine bekannte Wirkdauer von mindestens einem GCD.
6. Nach einem Druck gibt es keinen Durchlauf, in dem die Aktion weder Kandidat noch stehend ist.

**Offene Entscheidung D1 (`TODO.md`):** die Unverwundbarkeit, nachdem für dasselbe Wirken schon Minderungen
hinausgingen – (a) aus Z4, weil sie verweigert oder abklingend war (gebaut: nicht mehr, aus seiner Präzisierung
abgeleitet); (b) aus Z2, wenn danach die LP fallen und kein Bündel mehr reicht (gebaut: ja).

**Grenzen des Modells:** zwei Wirken binnen eines GCD bekommen einen Minderungsplan nur für das erste; ein Treffer über
dem bisher gemessenen Höchstwert wird unterschätzt; keine Reserve nach einem knapp überlebten Treffer (`TODO.md`).

### Die Ausführung

- **Zeitpunkt:** jede geplante Minderung, sobald der Einschlag nicht weiter entfernt ist als ihre Wirkdauer weniger
  einem GCD (Wirktexte über den Generator, `DefensiveValues.DurationOf`). Früher liefe sie vor dem Einschlag ab.
  Schutzwall (20 s) geht bei den meisten Wirken sofort, Urimpuls (8 s) etwa 5,5 s vorher (GCD 2,5 s). Die
  Unverwundbarkeit (10 s) ebenso.
- **Die Sperrgruppe des Spiels** (eine große Minderung zur Zeit, `RampartStatus` als Statusangabe) wird für einen
  geplanten Druck übergangen: Ob die Minderung beim Einschlag schon steht, hat der Plan selbst gefragt, und die
  Staffelung ist genau das, was der Plan für einen gemessenen Treffer ersetzt (seine Präzisierung).
- **Ziel:** jeder geplante Druck geht auf dich (`TargetType.Self`); The Blackest Night suchte sich sonst ein eigenes Ziel.
- **Ein abgelehnter Druck:** Während einer Animationssperre oder eines eigenen Zaubers lehnt `CanUse` ab; das ist
  keine Antwort, der Druck wird im nächsten Durchlauf neu versucht. Jede andere Ablehnung – Reflexion außer Reichweite,
  zu wenig MP, ein Stun, eine Prüfung der Rotation – sperrt den Druck für einen GCD; was daraus für Bündel und
  Unverwundbarkeit folgt, regelt das Zustandsmodell („Verweigerung", „‚Alles' ausgeschöpft"). Ablehnungen gelten je
  Wirken (eigene Nummer je Wirken, auch bei zwei gleichen Wirken ohne Pause, erkannt am Zurückspringen der Wirkzeit).

### Das Zurückhalten

Wann und was, regelt das Zustandsmodell. Abgelehnt wird jede Aktion mit Wirktextwert (`DefensiveValues`), die auf dich
selbst zielt und nicht freigegeben ist – eigene Minderung, eigene Barriere, Reflexion um dich herum –, aus jedem Pfad
der Rotation: Einzelabwehr, Notfall, allgemeine Fähigkeiten (der Krieger wählt Urimpuls auch nach der
Gesundheitsprognose, der Paladin Schiltron außerhalb der Abwehr). Freigegeben ist in Z2 ein Glied von S ab seinem
Fenster.
- **Frei bleiben** Hilfe für ein anderes Mitglied (Intervention, Herz des Korunds auf dem Co-Tank, Urflackern), die
  Heilpfade (Herz des Korunds heilt einen Tank, den Meteoritenfall auf 1 LP setzte; erkannt an einem eigenen Schalter,
  nicht an der Zielüberschreibung, die über ihren Pfad hinaus stehen bleiben kann), in der Flächenabwehr, was über
  dich hinaus wirkt (Reflexion, Abschütteln für einen Raidwide), und jeder Befehl von dir.

**Holmgang und Totenerweckung:** Heiliger Boden und Meteoritenfall: „Impervious to most attacks" – kein Schaden.
Holmgang, Undead Rebirth: „Most attacks cannot reduce your HP to less than 1"; Totenerweckung wandelt den Tod in
Walking Dead. Unter allen sichert die Unverwundbarkeit das Überleben des Treffers, und für ihn geht keine Minderung
mehr hinaus (Invariante 2, seine Präzisierung „nicht invul und dann noch zusätzlich buffs"). Unter Holmgang und
Totenerweckung nimmt ein Treffer LP bis 1; wie viele nach ihrem Ende bleiben, ist die offene Reserve-Frage.

**Heiler:** keine Einzelabwehr für einen gewirkten Tankbuster, wenn jedes seiner Ziele unter Heiligem Boden oder
Meteoritenfall über den Einschlag hinaus steht. Eine BossModReborn-Vorhersage öffnet sie weiterhin: Sie sagt nicht, wen
sie trifft, und ein zweiter Tankbuster auf den Co-Tank sähe genauso aus. Steht ein Tankbuster-Marker, hält nichts: Er
nennt keinen Einschlagszeitpunkt. Den Plan des Tanks kennt der Heiler nicht; seine Minderung auf einem Tank unter
Holmgang oder Totenerweckung bleibt also, wie sie war.

### Mechanik (Belege, abgerufen 04.10.2026)

- Statustexte (xivapi, Blatt `Status`): Heiliger Boden 82 und 1302, Meteoritenfall (Superbolide) 1836 „Impervious to
  most attacks"; Holmgang 409, Undead Rebirth 3255, Invulnerability 4275 „Most attacks cannot reduce your HP to less
  than 1". Im Code: `StatusHelper.ImperviousStatus`, `StatusHelper.InvulnerabilityStatus`.
- Living Dead (Job-Guide englisch): Fällt die Gesundheit unter Living Dead auf 0, folgt Walking Dead; wird bis zu dessen
  Ablauf Heilung in Höhe der maximalen LP aufgenommen, folgt Undead Rebirth, „If this amount is not restored, you will
  be KO'd." Die geplante Totenerweckung verlagert den Tod also auf eine Heilaufgabe, wie die reaktive schon heute; die
  Uhrregel und die Phase-2-Unterstützung (Abschnitt „Living Dead ist ein Zeitproblem") gelten unverändert.
- Wirkdauern und Werte aus den Wirktexten (Generator): Schutzwall 20 %/20 s, Verdammnis 40 %/15 s, Reflexion
  10 %/15 s, Urimpuls 10 %/8 s, Heiliges Schiltron 15 %/8 s, Herz des Korunds 15 %/8 s, The Blackest Night 25 % der
  maximalen LP als Barriere/7 s. Der Generator zählt bei Urimpuls und Schiltron nur die Grundminderung, nicht die
  kurze Zusatzwirkung der ersten Sekunden – das unterschätzt sie, die sichere Richtung.
- Unverwundbarkeiten: 10 s; Abklingzeiten Heiliger Boden 420 s, Holmgang 240 s, Totenerweckung 300 s, Meteoritenfall
  360 s (A259).

### In welche Richtung die Rechnung irrt

- **Zu viel Mittel** (mehr verbraucht als nötig): Der Höchstwert liegt über dem typischen Treffer; eine Heilung vor dem
  Einschlag zählt nicht; Zusatzwirkungen (Urimpuls, Schiltron) sind nicht gezählt. Alles drei kostet Abklingzeit,
  nicht Leben.
- **Zu wenig Mittel** (der Tank fällt mit Plan):
  - Ein Treffer über dem bisher gemessenen Höchstwert, etwa unter einer Stärkung des Bosses bei der Vorhersage. Ein
    Treffer unter Damage Up wird nicht gespeichert (A260), aber auch nicht hochgerechnet: Die Wirktexte nennen keine
    Stärke. Stärkungen unter anderem Namen bleiben unerkannt.
  - Ein Wert unter Verwundbarkeit aus weniger Stapeln als jetzt.
  - Auto-Angriffe zwischen Plan und Einschlag: Der Plan wächst mit, solange Zeit ist.
  - Nach dem Einschlag zählt die Rechnung nicht weiter: Ein Tank, den ein knapp überlebter Tankbuster tief lässt,
    hat danach die zurückgehaltenen Minderungen wieder frei (das Zurückhalten endet mit dem Wirken), und die reaktive
    Unverwundbarkeit und die Heiler übernehmen. Ob „knapp" eine Reserve braucht, ist offen (`TODO.md`).
- **Unverwundbarkeit zu oft:** nur, wenn kein Bündel reicht – bei LP jetzt. Ein Tank, der tief in den Tankbuster
  geht, bekommt sie eher, auch wenn ein Heiler ihn noch hochgeheilt hätte. Das ist der Preis von „ohne Risiko"; die
  Neurechnung je Durchlauf gibt sie zurück, wenn die Heilung vor dem Ziehen kommt.

### Lagen

- **Allein, Vierer-Instanz:** wirkt; die Regel braucht keinen Co-Tank.
- **Zwei RSR-Tanks:** Jeder plant nur für sich, über die Tankbuster auf ihn. Die Reflexion des anderen zählt, sobald
  sie steht.
- **BossMod ohne Modul:** unverändert; die Regel liest keine Vorhersage.
- **Stufensynchron:** Unverwundbarkeit und Minderungen zählen nur, wenn erlernt. Eine Gegneraktion gehört zu ihrem
  Inhalt und wird dort stets auf dessen Stufe getroffen; dass die Anteile dadurch vergleichbar bleiben, ist ein
  Schluss, nicht gemessen.
- **Burst aus:** unberührt.
- **Mehrere Wirken auf dich:** geplant wird für das früheste gemessene; zurückgehalten wird dann nichts, weil der Plan
  das zweite nicht kennt. Ein späteres bekommt seinen Plan, sobald das erste endet (Grenze: liegen beide in einem GCD,
  deckt ein Minderungsplan nur das erste).
- **Ein ungemessenes Wirken oder eine frühere BossModReborn-Vorhersage** neben einem gemessenen: Der Plan drückt,
  hält aber nichts zurück. Ein Tankbuster-Marker auf dir während des gemessenen Wirkens gilt als dessen.
- **Tankwechsel:** hält bei bereiter Unverwundbarkeit, die RSR für jeden nächsten Treffer nutzen würde – an ihrer
  Schwelle (`CustomRotation.InvulnerabilityUsable`). Der Plan zählt dort nicht: Er zieht sie nur vor einem gemessenen
  Wirken, nicht vor einem Marker oder einer Vorhersage. Die Schwelle `HealthForDyingTanks` gehört der reaktiven
  Unverwundbarkeit; wer sie auf 0 setzt, schaltet für den Plan nichts ab, der Tankwechsel wechselt dann aber auch bei
  bereiter Unverwundbarkeit.
- **Reaktive Unverwundbarkeit** (`EmergencyAbility` unter `HealthForDyingTanks`): unverändert, läuft vor dieser Regel.
  Hält sie über den Einschlag, zählt sie; läuft sie vorher ab, hält nichts zurück.
- **Staffelung ohne Messung** (A243, `RampartTakesPredictedTankbuster`, Zeitregel): bleibt für Tankbuster ohne
  Messwert. Für einen gemessenen ersetzt der Plan sie; die Staffelungszweige sehen den Plan nur über das Gatter.

### Antithesen

1. **Kein Defekt, die alte Abwehr genügt.** Sie gibt je Tankbuster eine große Minderung. Ein gemessener Treffer, der
   zwei braucht, tötet damit – seine Präzisierung nennt genau das. Und die reaktive Unverwundbarkeit zündet unter
   der Sterbe-Schwelle, die ein Treffer von voller Gesundheit in einem Schritt überspringt. Widerlegt.
2. **Die Option ist falsch, das billigste Bündel ist zu knapp.** Die Rechnung nimmt den Höchstwert, die LP jetzt
   und nur, was beim Einschlag steht; jede dieser Größen irrt zur sicheren Seite. Nicht widerlegt ist ein Treffer
   über allem bisher Gemessenen; dagegen hilft keine Rechnung, und die Tabelle hebt den Wert danach an.
3. **Ausgeliefert, und nichts ändert sich – warum?**
   - Die Aktion ist noch nicht gemessen: Das erste Auftreten bleibt ohne Plan. Die Tabelle misst jeden Spieler und
     speichert über Sitzungen.
   - Der Tankbuster kommt nur als Marker oder BossModReborn-Vorhersage: keine Aktion, kein Plan.
   - Der Boss trägt den ganzen Kampf ein Damage Up: kein Treffer wird gespeichert.

   Das Protokoll schreibt je Wirken die Zahlen und den Plan, sodass jeder dieser Fälle ablesbar ist.
4. **Das Zurückhalten kostet einen Treffer, wenn der Plan nicht aufgeht.** Abgefangen: Fehlt eine geplante Minderung
   im letzten GCD, öffnet es; reicht nichts und kommt keine Unverwundbarkeit, hält es gar nicht.
5. **Die Rotation wählt dieselbe Minderung anderswo.** Das Gatter gilt für jeden Pfad eines Durchlaufs. Widerlegt.
6. **Gegen seine eigene frühere Lage:** Die Staffelung (A243) kam aus dem Protokoll vom 01.10.2026: zwei große auf
   einen Tankbuster, nichts auf den nächsten. Der Plan stapelt nur, wo der Treffer es verlangt, und für einen
   Treffer, den eine reicht, gibt er eine – das Muster von damals entsteht nur, wenn jeder Tankbuster zwei braucht,
   und dann ist der zweite Tankbuster ohne Minderung, aber mit der Unverwundbarkeit geplant oder mit allem.

### Messmittel und Nachsteuerung

`DefenseTrace.log` schreibt je Wirken auf dich „tankbuster coming at you: … takes …% of max HP under what stands at
impact, you have …%", je Änderung des Plans „tankbuster plan for … in … s: Rampart + Damnation, leaving …% against
…%" (oder „survivable as it stands, nothing spent", „nothing less survives it: the invulnerability", „nothing survives
it, everything goes"), je Druck „tankbuster plan for … -> Rampart" mit der „used"-Zeile. Nachsteuerung: Jeder Treffer
hebt den Tabellenwert, wo er höher ausfällt, und der Plan wird je Durchlauf neu gerechnet.

**Prüfgrad:** statisch, Prüfskripte, Compile über die CI, zwei unabhängige Code-Reviews; im Spiel nicht beobachtet.

## Was offen bleibt

**Der Vorlauf der Uhrregel misst bis zur Entscheidung, nicht bis zum Landen der
Heilung.** Im ungünstigsten Fall muss der laufende GCD auslaufen und ein Zauber mit
Wirkzeit darauf fertig werden — zusammen zwei GCDs. Der Vorlauf muss daher mindestens
zwei GCDs betragen. Wie viele Sekunden das sind, steht nicht fest: Gemessen wird die
**tatsächliche** Erholzeit des Spielers, die das Spiel meldet
(`DataCenter.DefaultGCDTotal`; läuft kein GCD, die Länge, die das Spiel für einen Waffenskill rechnet,
`ActionManagerHelper.GetDefaultAdjustedRecastTime`, A240), also
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

**Die gestaffelte Phase-2-Unterstützung** (Fälle 4 und 4a) ist gebaut (A147, Abschnitt „Umsetzung für Phase 2“):
`WalkingDeadCarriedBySelfHeal` lässt nur HoTs an den Träger, solange er sich selbst trägt, und gibt die volle
Unterstützung bei den dort genannten Auslösern frei. Die Kursprognose rechnet die Gesundheit seit Beginn des Fensters
auf die Restzeit hoch, nicht die aufgenommene Heilmenge; fallen Heilung und Schaden in dieselbe Abtastung, unterschätzt
sie den Kurs (Grenze unter „Was ausgeschlossen wurde“).

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

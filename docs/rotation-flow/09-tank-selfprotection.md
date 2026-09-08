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
noch rechtzeitig kommt — hinter einer Option mit Standard aus. Für The Blackest
Night ist sie nicht gebaut, weil ihr Auslöser nicht beobachtbar ist.

| Baustein | Stand |
|---|---|
| Invertierte Prüfung an beiden Fundorten korrigiert | umgesetzt |
| Zielwahl vom Ausschluss zur Prioritätsstufe | umgesetzt |
| Fehlende Ids (`HallowedGround`, `HallowedGround_1302`, `UndeadRebirth`) | umgesetzt |
| Schutzstatus senkt die Heilschwelle, statt das Flag zu unterdrücken | umgesetzt |
| Living-Dead-Rückhaltung als Uhrregel, hinter Option | umgesetzt |
| The-Blackest-Night-Rückhaltung | nicht gebaut, Auslöser nicht beobachtbar |
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
| DRK | The Blackest Night | Barriere wird **vollständig** absorbiert | ein Schadensereignis unterhalb des Todes | indirekt: hebt die HP, sodass weniger Schaden gegen die Barriere läuft |
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

### Klasse A+ — The Blackest Night, ein Auslöser mit Rückzahlung

Die Fähigkeit kostet **3000 MP** und legt eine Barriere über 25 % der maximalen HP
für 7 Sekunden. Dark Arts — und damit ein kostenloser Edge oder Flood of Shadow —
wird nur gewährt, wenn die Barriere **vollständig** absorbiert wird. Wird sie es
nicht, sind die 3000 MP ersatzlos ausgegeben: Edge of Shadow kostet seinerseits
3000 MP und verlängert Darkside um 30 Sekunden, deren Abriss zehn Prozent Schaden
kostet.

Ob ein fremder Schild das verschlimmert, ist **nicht entscheidbar**, und zwar aus
zwei unabhängigen Gründen:

*Die Aufteilung ist unbelegt.* Für Heilerschilde untereinander ist überliefert, dass
sie sich nicht addieren, sondern der stärkere den schwächeren verdrängt — Divine
Benison als dokumentierte Ausnahme. Trifft das auf The Blackest Night zu, *ersetzt*
ein größerer Heilerschild die Barriere, statt sie zu schonen; sie wäre sofort fort
statt langsamer verbraucht. Beide Wirkungen schaden dem Dunkelritter, verlangen aber
entgegengesetzte Gegenmaßnahmen. Quellenstatus: Spielerforum, keine offizielle
Dokumentation — unbelegt.

*Der Auslöser ist nicht beobachtbar.* Der Client führt je Charakter genau einen
Schildwert: `ICharacter.ShieldPercentage`, den `ObjectHelper.GetObjectShield`
(`ObjectHelper.cs:3372`) in absolute Punkte umrechnet. Es gibt keine Buchführung je
Quelle. Ein Plugin kann weder sehen, wie viel des Puffers von The Blackest Night
stammt, noch ob die Barriere vollständig aufgezehrt wurde — weder zur Laufzeit noch
im Nachhinein.

Damit ist Fall 5 keine Abwägung zwischen belegtem Nutzen und Kosten, sondern eine
Regel ohne Nachweismöglichkeit auf unbelegter Mechanik. Sie ist deshalb nicht
gebaut.

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
  ausdrücklich spielerzentriert (`DataCenter.cs:2087`).
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
| 5 | DRK, TBN aktiv, **Tank nicht in Gefahr** | zurückhaltend | zurückhaltend | Wirkung unbelegt und nicht beobachtbar; trägt keine Regel |
| 5b | dieselbe Lage, **Tank in Gefahr oder Buster-Fenster** | **ja** | **ja** | Ein möglicherweise verlorener Dark Arts wiegt keinen toten Tank auf |
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
Bedarf nicht auf.** Handeln schadet nur in den Zeilen 1, 2 und 5 — und dort nur,
solange Stufe 1 gesichert ist. Von neunzehn Lagen bleiben **zwei** Rückhaltefälle
(1, 5), denen fünf Aufhebungen (1b, 1c, 1d, 4a, 5b) gegenüberstehen; einer ist reine
Ressourcenschonung ohne Risiko (4b). Eine Bedingung, die häufiger nicht gilt als
gilt, ist kein Leitmotiv.

## Was gebaut ist

### Die Zielwahl (Rangstufe 1)

`NoNeedHealingInvuln()` ruft `WillStatusEndGCD(2, 0, false, …)`. Die Kette
`WillStatusEnd` → `StatusTime` (`StatusHelper.cs:781`, `:845`) liefert bei
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

## Was ausgeschlossen wurde und warum

**Nullvariante** — nichts ändern. Ausgeschlossen: zwei belegte Defekte der Rangstufe 1
blieben stehen.

**Nur Living Dead behandeln, Zielwahl unangetastet.** Ausgeschlossen: Es hätte den
größeren, bereits belegten Fehler stehen gelassen, um einen Sonderfall zu bedienen.

**Klasse A vollständig, einschließlich The Blackest Night und Excogitation.**
Ausgeschlossen für **Excogitation**, weil sein Auslöser nicht vorhersehbar ist: Ob der
Schaden kommt, der die Schwelle unterschreitet, ist unbekannt. Ausgeschlossen für
**The Blackest Night**, weil dort der *Status* sichtbar ist, nicht der *Auslöser* —
aus einem einzigen `ShieldPercentage`-Wert ist nicht ableitbar, ob die Barriere
vollständig verzehrt wurde, und die zugrunde liegende Stapelmechanik ist unbelegt.

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
verlangt eine Kursprognose und damit den verworfenen Messbaustein.

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

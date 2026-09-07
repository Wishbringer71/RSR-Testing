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

| Job | Fähigkeit | Auslöser | Was fremde Heilung bewirkt | Was ein fremder Schild bewirkt |
|---|---|---|---|---|
| DRK | Living Dead | HP fallen auf 0 | verhindert den Tod, also den Übergang in Walking Dead samt dessen Selbstheilung | dasselbe, über abgefangenen Schaden |
| DRK | The Blackest Night | Barriere wird **vollständig** absorbiert | indirekt: hebt die HP, sodass weniger Schaden gegen die Barriere läuft | direkt: absorbiert Schaden, der die Barriere gebrochen hätte — kein Dark Arts |
| GNB | Catharsis of Corundum | HP fallen auf 50 % oder darunter | hält über der Schwelle, der Nachheilstoß entfällt | dasselbe, über abgefangenen Schaden |
| SCH | Excogitation | HP-Schwelle auf dem Ziel | vorzeitige Heilung entwertet die eigene, bereits gesetzte Fähigkeit | dasselbe |

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

Ein Heilerschild verschlimmert das doppelt: Er erhöht den Gesamtpuffer, gegen den
der Schaden läuft, und macht damit unwahrscheinlicher, dass die Barriere aufgezehrt
wird. Er nimmt dem Tank also nicht nur den Nutzen, sondern verwandelt eine bezahlte
Fähigkeit in einen reinen Verlust.

**Offen und hier nicht belegt:** wie zwei gleichzeitig liegende Schilde ihren
Verbrauch aufteilen. Der Schluss hängt davon nicht ab — mehr Gesamtpuffer heißt in
jedem Aufteilungsmodell geringere Wahrscheinlichkeit, dass die Barriere vollständig
verbraucht wird.

### Klasse B — Unverwundbarkeit: Heilung während der Phase wirkt nicht auf den Schutz, wohl aber auf das, was danach kommt

| Job | Fähigkeit | Wirkung auf die HP | Lage nach Ablauf |
|---|---|---|---|
| PLD | Hallowed Ground | unverändert | so niedrig wie beim Zünden |
| WAR | Holmgang | fallen nicht unter 1 | möglicherweise 1 HP |
| GNB | Superbolide | **sofort auf 1** | 1 HP, sofern nicht geheilt wurde |
| DRK | Living Dead / Walking Dead | Sonderfall, siehe Klasse A und unten | — |

### Klasse C — reine Schadensreduktion: keine Wechselwirkung

Rampart, Sentinel, Shadow Wall, Nebula, Sheltron und Holy Sheltron, Bulwark,
Camouflage, Oblation, Heart of Corundum (der Reduktionsanteil), Reprisal. Heilung
und Schilde sind hier weder schädlich noch besonders dringend.

### Klasse D — Selbstheilung ohne Auslöser: Heilung ist redundant, nicht schädlich

Bloodwhetting und Raw Intuition (heilen bei Waffenskill-Treffern), Aurora,
Clemency, Nascent Flash. Fremde Heilung addiert sich, verhindert aber nichts.

## Fallvarianten

Vollständig über die Dimensionen Fähigkeitsklasse × Gesundheitsstand ×
Heileraktion. „Richtig" meint jeweils die Handlung, die dem Vorrang der
Überlebenssicherung folgt.

| # | Lage | Heilung richtig? | Schild richtig? | Begründung |
|---|---|---|---|---|
| 1 | DRK, Living Dead aktiv, HP hoch, **Heilkapazität für Phase 2 gesichert** | **nein** | **nein** | Stufe 1 ist gesichert, also darf Stufe 2 entscheiden: beides verhindert den Tod, den die Fähigkeit einplant |
| 1b | dieselbe Lage, **Heilkapazität nicht gesichert** | **ja** | ja | Stufe 1 schlägt Stufe 2. Ohne die Kapazität für die volle Heilmenge in Walking Dead ist der Verzicht ein Tausch von sicherem gegen unsicheres Überleben |
| 2 | DRK, Living Dead aktiv, HP niedrig, Ablauf fern | wie 1 / 1b | wie 1 / 1b | Der Gesundheitsstand allein ändert nichts; entscheidend bleibt die Kapazität für Phase 2 |
| 3 | DRK, Living Dead läuft in ≤ 2 GCDs ab, HP niedrig | **ja, dringend** | ja | Der Schutz endet, ohne dass der Tod eintrat — danach ist er ungeschützt |
| 4 | DRK, Walking Dead aktiv | **ja, zwingend** | nein, wirkungslos bei 1 HP | Überlebensbedingung ist kumulative Heilung in Höhe der maximalen HP |
| 5 | DRK, TBN aktiv, Schaden läuft, **Tank nicht in Gefahr** | zurückhaltend | **nein** | Stufe 1 gesichert, also entscheidet Stufe 2: Ein Schild vergrößert den Gesamtpuffer und verhindert womöglich den vollständigen Verbrauch. Dann sind 3000 MP verloren, samt dem Edge of Shadow und dessen Beitrag zur Darkside-Laufzeit |
| 5b | dieselbe Lage, **Tank in Gefahr** | **ja** | **ja** | Stufe 1 schlägt Stufe 2. Ein verlorener Dark Arts wiegt keinen toten Tank auf |
| 6 | GNB, Superbolide aktiv | **ja** | ja | HP stehen auf 1; das Fenster ist die einzige gefahrlose Gelegenheit |
| 7 | WAR, Holmgang aktiv, HP heruntergedrückt | **ja** | ja | wie 6 |
| 8 | PLD, Hallowed Ground aktiv, beim Zünden wenig HP | **ja** | ja | Die HP bleiben unverändert; nach Ablauf steht er, wo er stand |
| 9 | PLD, Hallowed Ground aktiv, beim Zünden viel HP | nein, nachrangig | nein | Einziger Fall echter Entbehrlichkeit in Klasse B |
| 10 | GNB, HP knapp über 50 %, Auslöser noch offen, **Tank nicht in Gefahr** | zurückhaltend | zurückhaltend | Stufe 1 gesichert; über der Schwelle zu halten verschenkt den Catharsis-Stoß |
| 10b | dieselbe Lage, **Tank in Gefahr** | **ja** | ja | Stufe 1 schlägt Stufe 2. Den Tank absichtlich unter 50 % fallen zu lassen, ist eine Wette auf den nächsten Schlag |
| 11 | GNB, HP bereits unter 50 %, Catharsis ausgelöst | **ja** | ja | Der Auslöser ist verbraucht, nichts steht mehr im Weg |
| 12 | Beliebig, Klasse C aktiv | nach normaler Regel | nach normaler Regel | keine Wechselwirkung |
| 13 | Beliebig, Klasse D aktiv | nachrangig | nach normaler Regel | Die Selbstheilung trägt einen Teil, ersetzt sie aber nicht |
| 14 | Mehrere Gruppenmitglieder brauchen Heilung, Tank geschützt | Tank **nachrangig**, andere zuerst | dito | Der Schutz verschiebt die Dringlichkeit, hebt den Bedarf nicht auf |
| 15 | Nur der geschützte Tank braucht Heilung, kein Auslöser betroffen | **ja** | ja | Nichts spricht dagegen; das Fenster ist die sicherste Gelegenheit |

Zeile 14 und 15 sind der Kern: **Der Schutz verschiebt die Reihenfolge, er hebt den
Bedarf nicht auf.** Handeln schadet nur in den Zeilen 1, 2, 5 und 10 — und dort
ausschließlich, solange Stufe 1 gesichert ist. Die b-Zeilen sind nicht Ausnahmen am
Rand, sondern der Regelfall, sobald der Tank in Gefahr gerät: **Jede Rückhaltung
steht unter dem Vorbehalt des Überlebens.**

Bemerkenswert ist die Verteilung: Von siebzehn Lagen sind drei Rückhaltefälle, drei
ihre Gegenstücke unter Gefahr, und elf verlangen normales oder nachrangiges
Handeln. Ein Entwurf, der die Rückhaltung zum Leitmotiv macht, hätte das Verhältnis
verfehlt.

## Was RSR heute tut

| Mechanik | Kenntnis | Bewertung |
|---|---|---|
| Unverwundbarkeiten | `StatusHelper.NoNeedHealingStatus` unterdrückt Heilung, Freigabe zwei GCDs vor Ablauf | Konstruktion falsch: prüft den Gesundheitsstand nicht, siehe `TODO.md`. Zusätzlich fehlen alle vier `HallowedGround`-Ids und drei von vier `Holmgang`-Ids |
| Heilzielauswahl | `ActionTargetInfo.cs:3536` liest dieselbe Prüfung invertiert | Defekt, siehe `TODO.md` |
| The Blackest Night | Nur als Eintrag in `ShieldStatus` (`StatusHelper.cs:395`), also als Beitrag zur effektiven Gesundheit | Der Bruchmechanismus ist unbekannt; ein Heilerschild darüber wird nicht verhindert |
| Catharsis of Corundum | Kommt einmal vor, als `StatusProvide` der Gunbreaker-Aktion (`GunbreakerRotation.cs:430`) | Für die Heilentscheidung unbekannt |
| Heart of Corundum, Excogitation | Keine Fundstelle in einer Entscheidung | unbekannt |
| Klasse C und D | keine Sonderbehandlung | richtig so |

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
bewusste Ressourcenschonung. Widerlegt für die invertierte Prüfung
(`ActionTargetInfo.cs:3536` widerspricht `StateUpdater.cs:726` bei identischer
Funktion — eine der beiden muss falsch sein) und für die fehlende
Gesundheitsprüfung (ein Paladin, der bei fünf Prozent zündet, steht nach zehn
Sekunden bei fünf Prozent).

**Hypothese 2: Die Prioritätsstufe ist die falsche Lösung.** Alternative wäre eine
Schwelle: unterdrücken nur oberhalb eines Gesundheitswertes. Hält teilweise
stand — sie wäre einfacher, träfe aber Fall 14 nicht, weil sie den Vergleich mit
anderen Gruppenmitgliedern nicht kennt. Die Prioritätsstufe leistet beides, ist
aber der größere Eingriff. Für O1 wird die Stufe gewählt, weil `GeneralHealTarget`
die Rangfolge bereits besitzt und nur eine Ebene fehlt.

**Hypothese 3: Klasse A ist nicht behandelbar.** Die Auslöser sind Ereignisse, die
RSR nicht vorhersehen kann. Hält für Excogitation stand: Ob der Schaden kommt, der
die Schwelle unterschreitet, ist nicht bekannt. Für TBN und Catharsis hält sie
nicht — der Status ist sichtbar, und die Regel muss nur zurückhalten, solange er
liegt. **Excogitation wird deshalb aus O2 herausgenommen.**

**Was nicht widerlegt ist:** dass die Prioritätsstufe im Spiel besser spielt. Die
Rangfolge ist heute faktisch wirkungslos, ihre erstmalige Wirksamkeit ist eine
Verhaltensänderung ohne Nachweismöglichkeit.

## Umsetzungskonzept

Drei Schritte. Der erste behebt, die beiden anderen erweitern.

### Schritt 1 — Die invertierte Prüfung korrigieren

`ActionTargetInfo.cs:3536`: `if (!o.NoNeedHealingInvuln())` wird zu
`if (o.NoNeedHealingInvuln())`. Eine Zeile, aber die Wirkung ist groß: Erst danach
enthält `healingNeededObjs` überhaupt die normalen Gruppenmitglieder, und die
Rangfolge Selbst → Heiler → Tank → niedrigste Gesundheit greift zum ersten Mal.

Getrennt zu committen, damit die Wirkung dieser einen Zeile im Verlauf sichtbar
bleibt.

### Schritt 2 — Vom Ausschluss zur Prioritätsstufe

`NoNeedHealingStatus` wird nicht mehr als Filter verwendet, sondern als
Sortierkriterium: geschützte Ziele wandern ans Ende von `healingNeededObjs`,
statt herauszufallen. Fall 14 und 15 sind damit beide richtig — andere zuerst,
der Tank danach.

Die Zwei-GCD-Freigabe bleibt als Beschleuniger: Läuft der Schutz aus, verliert das
Ziel die Herabstufung und rückt nach vorn.

Damit entfällt zugleich die Frage nach den fehlenden `HallowedGround`- und
`Holmgang`-Ids: Ihre Ergänzung ist erst nach diesem Schritt unschädlich, weil sie
dann nur noch die Reihenfolge beeinflusst, nicht mehr das Ob.

### Schritt 3 — Klasse A als Rückhalteliste, unter dem Überlebensvorbehalt

Eine kleine, begründete Liste für die Fälle, in denen Handeln tatsächlich schadet —
jeder Eintrag steht für einen belegten Auslöser, nicht für „ist gerade geschützt":

```
StatusHelper.TriggerBearingStatus = [ LivingDead, BlackestNight ]
```

`WalkingDead` gehört ausdrücklich **nicht** hinein: Dort ist Heilung die
Überlebensbedingung. `ClarityOfCorundum` erst nach Prüfung am Artefakt.

**Die Rückhaltung greift nur, wenn Stufe 1 gesichert ist.** Das ist keine
Ausnahme am Ende der Bedingung, sondern die erste Prüfung:

```
protected bool MayWithholdForTrigger(IBattleChara tank) =>
       TankSurvivesWithoutMe(tank)                       // Stufe 1
    && tank.HasStatus(false, StatusHelper.TriggerBearingStatus)   // Stufe 2
    && WithholdForTriggersEnabled;
```

`TankSurvivesWithoutMe` ist konservativ zu bauen, aus vorhandenen Größen:

- `tank.GetEffectiveHpPercent()` über `Service.Config.HealthForDyingTanks`
  (`ObjectHelper.cs:126` nutzt dieselbe Schwelle bereits für sterbende Tanks),
- kein `DataCenter.IsHostileCastingToTank` und kein
  `DataCenter.BMRTankbusterImminent`,
- **für Living Dead zusätzlich:** genug eigene Kapazität für Phase 2. Ohne
  belastbares Maß dafür wird konservativ gefordert, dass `DataCenter.CurrentMp`
  über einer Schwelle liegt und kein weiteres Gruppenmitglied unter
  `Service.Config.HealthTankRatio` steht. Ist eines davon nicht erfüllt, wird nicht
  zurückgehalten.

Für `BlackestNight` gilt die Rückhaltung **vorrangig dem Schild**, nicht der
Heilung: Die Barriere wird von Schaden verzehrt, nicht von fehlender Heilung, und
ein zusätzlicher Schild ist die direkte Störung. Heilung wirkt nur mittelbar, indem
sie den Schadensdruck senkt. Wo beides zur Wahl steht, ist der Schild zurückzuhalten
und die Heilung nur herabzustufen.

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
| Deckt Schritt 3 Fall 10 ab? | Nein, und bewusst nicht: `HeartOfCorundum` liegt über den Auslöser hinaus, `ClarityOfCorundum` ist ungeprüft. Fall 10 bleibt offen |
| Wirkungsbereich benannt? | Ja: Die Liste wirkt nur auf Ziele in der Tankrolle |
| Was passiert bei zwei Tanks? | **Neue Lücke.** `TankSurvivesWithoutMe` prüft ein Ziel. Trägt der Zweittank denselben Status, gilt die Prüfung je Ziel — richtig, aber die MP-Ersatzbedingung ist global und würde die Rückhaltung für beide gleichzeitig aufheben. Konservativ und damit hinnehmbar |

## Verbesserung nach dem zweiten Audit

**Schritt 1 und 2 bleiben zusammengelegt** — Ergebnis des ersten Audits, bestätigt.

**Die Überlebensprüfung wird zur ersten Bedingung** statt zur Ausnahme. Das ist die
Beseitigung des Hauptmissstands: Ohne sie hätte das Konzept in drei von siebzehn
Lagen einen Effekt der Stufe 2 über das Überleben gestellt.

**Stufe 3 verliert jede eigenständige Wirkung.** Ressourcenschonung darf nichts
zurückhalten; sie ergibt sich als Nebenwirkung der Herabstufung.

**Ergebnis: zwei Schritte.**

1. **Zielwahl richten und herabstufen** — die invertierte Prüfung korrigieren und im
   selben Eingriff vom Filter auf die Prioritätsstufe umstellen. Danach die
   fehlenden `HallowedGround`- und `Holmgang`-Ids ergänzen, deren Aufnahme dann
   unschädlich ist. Defektbehebung, ohne Option.
2. **Auslöser-Rückhalteliste** mit `LivingDead` und `BlackestNight`, gebunden an die
   Tankrolle, mit dem Überlebensvorbehalt als erster Bedingung, hinter einer Option
   mit Standard aus. `ClarityOfCorundum` und damit Fall 10 erst nach Prüfung am
   Artefakt.

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

**Endnutzer.** Nach Schritt 1 ändert sich die Heilzielwahl aller Jobs spürbar,
weil die Rangfolge erstmals greift. Das ist beabsichtigt, aber es ist die größte
Verhaltensänderung dieses Vorhabens.

**Autoren abgeleiteter Rotationen.** `NoNeedHealingStatus` bleibt öffentlich und
unverändert; die neue Liste kommt additiv hinzu. Wer die Prüfung selbst aufruft —
`BeirutaWHM`, `BeirutaSCH`, `BeirutaAST`, `BeirutaSGE`, `SCH_Reborn` — behält sein
Verhalten, folgt damit aber weiter der alten Konstruktion. Das ist zu benennen,
nicht stillschweigend zu ändern.

**Upstream.** Der Eingriff liegt in `ActionTargetInfo` und `StatusHelper`, beide
mit regelmäßiger Upstream-Aktivität.

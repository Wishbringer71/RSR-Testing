# 12 · Searing Light bei einem und bei mehreren Beschwörern

**Leitfrage:** Wann zündet der Beschwörer Searing Light, allein und mit weiteren Beschwörern in der Gruppe?

*Zum Namen: Der Auftraggeber nennt die Aktion „Gleißender Schein". Die Zuordnung zu **Searing Light**
ist belegt und in `.github/scripts/audit/action_names_de.json` geführt; Quelle ist seine eigene
Angabe. Das Dokument benutzt den englischen Bezeichner, weil der Code ihn trägt.*

Die Prüfhistorie steht in `AUDIT_LOG.md` (A78, A89, A112–A115, A126, A127, A129, A132, A138, A206, A247).

## Antwort

- **Ein Beschwörer:** Searing Light fällt im Einschiebeplatz **vor** der Burst-Beschwörung (Solar Bahamut,
  unterhalb seiner Stufe Demi-Bahamut), und die Beschwörung wartet dafür, wenn der Buff bereit ist oder in den
  Platz des Warte-GCDs zurückkehrt. So steht der Buff vor dem ersten Burstschaden, und sein Takt bleibt an
  Solar gebunden.
- **Mehrere Beschwörer:** Jede große Beschwörung ist ein Zündfenster. Sind alle Burstphasen dauerhaft von
  anderen belegt, weicht die Ladung in den Titan-Block aus, in den Ifrit-Block nur, wenn der Spieler ohnehin
  am Ziel steht. Die Beschwörung wartet dann auf keinen abkühlenden Buff.
- **Immer:** Ein fremder Searing Light zählt als Buff-Fenster für die eigenen Aetherflow-Ausgaben. Über einen
  laufenden Buff gleich welcher Herkunft zündet RSR nicht, außer in seinen letzten GCDs. Ein fälliger
  Schimmerschild geht vor jeder Demi vor, und vor einer Wiederbelebung oder Heilung als nächstem GCD
  wartet der Buff.

## Seine Vorgabe: eine Regel, die sich der Lage anpasst

**Die Regel hat nicht fest zu sein, sondern sich optimal auf die bestehende Situation
auszurichten.** Sie darf dabei auf Annahmen aufbauen und diese Schritt für Schritt korrigieren,
sobald die Beobachtung ihnen widerspricht:

1. **Annahme: Ich bin der Erste.** Der Kampf beginnt mit Solar Bahamut, und Searing Light wird dort
   gezündet.
2. **Kommt mir ein anderer Beschwörer zuvor**, halte ich meine Ladung für diese Solar-Phase zurück —
   ein zweiter Buff auf einen laufenden ersten wäre doppelt gezündet und damit verschwendet. Das
   Zurückhalten gilt nur für diese Phase, nicht grundsätzlich.
3. **Nächster Versuch in der nächsten anstehenden Burstphase** (Bahamut oder Phoenix), wieder unter
   der Annahme, dass ich dort der Erste bin.
4. **Kommt mir auch dort jemand zuvor**, rückt der Versuch auf die darauffolgende Burstphase weiter —
   die übriggebliebene von Phoenix beziehungsweise Bahamut, erneut unter derselben Annahme.
5. **Dabei ist zu beachten**, dass der Beschwörer, der beim ersten Solar Bahamut gezündet hat, beim
   nächsten Solar Bahamut wieder dran sein kann. Auch hier gilt zunächst die Annahme, dass ich als
   Erster zünde.
6. **Trifft auch das nicht zu** und er kommt erneut zuvor, ist von der Wahrscheinlichkeit auszugehen,
   dass es bei den übrigen ebenso läuft — bei vier Beschwörern in der Gruppe also durchgehend. Dann
   gehört das eigene Searing Light in eine **Zwischenphase**, und zwar in die, in der der eigene
   Schaden am größten ist: Ifrit oder Titan.

**Die Bauform ist ein hybrides, dynamisches Modell:** eine Regel, die sich an die Lage anpasst und
bei Bedarf zwischen Strategien **wechselt**, statt einer festen Wahl zu folgen. Die Messung stützt
das — keine der geprüften Einzelregeln gewinnt überall: Bei auseinandergelaufenen Rotationen ist die
reine Solar-Regel bereits optimal, weil jeder Beschwörer seinen Buff in seiner stärksten Phase
setzt und die Streuung die Lücken von selbst schließt; bei synchronem Pull mit fremden Beschwörern
gewinnt die Buchführung; folgen alle derselben Regel, gewinnt das reine Lückenfüllen. Eine feste
Regel muss daher in mindestens einer dieser Lagen unterlegen sein.

**Die Buchführung ist genau dafür da.** Sie beantwortet die Frage, in welcher Lage man sich befindet:
Wer belegt welche Phase, und tut er es wiederholt? Erst aus der Wiederholung folgt, dass eine Phase
dauerhaft vergeben ist — ein einmaliges Zuvorkommen ist Zufall und darf keine Phase kosten.

**Gruppengrößen, auf die die Anpassung auszurichten ist:** In einer Vierergruppe sind regulär bis zu
zwei Beschwörer möglich, in einer Achtergruppe bis zu fünf. Eine Vierergruppe aus vier und eine
Achtergruppe aus acht Beschwörern sind nicht auszuschließen, haben bei der Optimierung aber
nachrangig zu gelten: **Vorrang haben die regulären Gruppen.**

**Ausgeschlossen ist damit** die Annahme, ab zwei Beschwörern fielen die Gelegenheiten ohnehin
zusammen. Spielstile und RSR-Einstellungen unterscheiden sich, und schon der Zeitpunkt des
Kampfeintritts streut die Zyklen. Der synchrone Fall ist ein Randfall, kein Regelfall — und eine
Regel, die nur ihn behandelt, behandelt den seltensten Fall.

**Seine Vorgabe zum Zeitpunkt (A115):** „Somit muss ja searing light aktiv sein, bevor der erste
burstschaden entsteht."

**Sein Hinweis auf Schild und Addle:** „schimmerschild kann man nicht im burst um einen gcd nach hinten
verschieben, muss vor demi bei bedarf gecasted werden. addle könnte verschoben werden, solange sicher ist,
dass er rechtzeitig vor castende des gegners draussen ist." Beides sind Tatsachen des Spiels, keine
Festlegungen; eingearbeitet unter „Der Schimmerschild geht vor" und „Ausgeschlossen".

**Sein Prüfvorschlag zu mehreren Beschwörern:** „die prüfung der abklingzeit darf aber nicht dazu führen,
dass alle demis verzögert werden (siehe mehrere Beschwörer in gruppe), da erfolgt ein ausweichen auf den
nächsten demi bzw. im negativfall auf den stärksten primal." Geprüft und so umgesetzt (unten, „Die
Beschwörung wartet").

## Die Regel

### Ein Beschwörer: Zündung vor der Burst-Beschwörung

1. **Zündung.** Im Einschiebeplatz **vor** der Burst-Beschwörung — Solar Bahamut, unterhalb seiner
   Stufe Demi-Bahamut —, sobald deren Abklingzeit bis zum nächsten GCD abläuft (`burstAboutToStart`:
   Burst an, Beschwörung bis zum nächsten GCD bereit, und sie ist die Burst-Demi laut
   `NextBigSummonIsBurst`); sonst in der Phase (`burstInSolar`).
2. **Die Beschwörung wartet** (`searingSettled`), wenn der Buff bereit ist oder so früh zurückkehrt, dass
   er noch in den Einschiebeplatz des GCDs passt, den das Warten kostet. Sie wartet nicht bei laufendem
   Buff gleich welcher Herkunft, bei ausgeschaltetem Burst oder Searing Light, vor einer Beschwörung, die
   nicht die Burst-Demi ist, und unterhalb der Stufe von Searing Light.
3. **Nicht vor einer Wiederbelebung oder Heilung als nächstem GCD** (`higherGcdFirst`: Resurrection,
   Physick; A206). Beide stehen im Dispatch vor der Beschwörung, und eine hart gewirkte Wiederbelebung
   dauert acht Sekunden: Ein jetzt gezündeter Buff liefe, während die Beschwörung wartet, und die letzten
   GCDs der Solar-Phase fielen aus seinen 20 s. Die Wiederbelebung behält ihren Vorrang (seine
   Sicherheitsregel), und der Buff bleibt bei Solar. Die Regel fragt, was der nächste GCD **nicht** ist,
   und ist darum nicht das Henne-Ei-Problem unten.

**Im Kampf:** Searing Light steht vor dem ersten Umbral Impulse jeder Solar-Phase. Kann es vor der
Beschwörung nicht fallen — etwa weil davor Ruby Rite gewirkt wird —, kommt Solar einen GCD später, und
Searing Light geht in den Platz dieses GCDs. Solar und der Buff verschieben sich dabei gemeinsam; der
Burst bleibt geschlossen, weil der einzige Beschwörer ihn selbst setzt.

**Gelesen wird die Bereitschaft der Beschwörung, nicht der nächste GCD.** Andernfalls entstünde dasselbe
Henne-Ei-Problem wie bei der Wiederbelebung (Konzept 11): Der Buff wartete darauf, angekündigt zu
werden, und die Ankündigung auf den Buff.

**„Bereit bis zum nächsten GCD", nicht „bereit".** Die Abklingzeit der Beschwörung endet auf dem
GCD-Raster, weil die vorige Beschwörung selbst ein GCD war und 60 s ein Vielfaches des GCDs sind. Fragte
die Regel nach abgelaufener Abklingzeit, wäre der Platz davor schon vorbei (A126).

**Keine Sonde:** Die Entscheidung fällt im Code, aus der Bereitschaft der Beschwörung und des Buffs, die
beide hier und jetzt lesbar sind. Eine Messung, deren Auswertung über das Modell liefe, kostete je Wert
einen Kampf, eine Ablesung, einen Bericht und eine Runde (seine Vorgabe).

### Der Schimmerschild geht vor

**Schimmerschild (Radiant Aegis) geht vor jeder Demi, wenn er fällig ist** (`RadiantAegisDueBeforeDemi`).
Der Wirktext begründet es: „Can only be executed while Carbuncle is summoned" — die Demi ersetzt Carbuncle
für 15 Sekunden. Fällig heißt, was die Rotation ohnehin fragt: ein von BossModReborn angekündigter Raidwide
innerhalb der Wirkdauer des Schilds (aus seinem Wirktext, `DefensiveValues.DurationOf`) oder die stehende
Verteidigungsflagge ohne eigenen Schild, wenn der Treffer den Beschwörer selbst erreicht (`AreaHitOnMe`,
Konzept 13: die Flagge fragt nach der Gruppe, der Schild schützt nur ihn). Wirkbar heißt Stufe, Option,
Carbuncle und eine Ladung. Der Schild nimmt den Platz vor der Beschwörung **vor** Searing Light, und jede
Beschwörung wartet, solange er fällig und wirkbar ist — auch eine, die nicht die Burst-Demi ist, und bei
ausgeschaltetem Burst. Ohne BossModReborn-Modul bleibt nur die Flagge, also ein Cast, der schon läuft;
das ist die Grenze des reaktiven Wegs.

**Warum so:** Der angekündigte Schild steht in `GeneralAbility`, das die Kette erst **nach** dem
Angriffszweig fragt. Ohne den Vorrang nähme Searing Light den einzigen Platz vor der Demi, und der Schild
bliebe die ganze Phase aus.

### Mehrere Beschwörer: das Zündfenster

**Das Zündfenster** (`SMN_Reborn.AttackAbility`):

`burstInSolar || burstAboutToStart || (AnotherSummonerInParty && (inBigInvocation || (AllSearingPhasesHeld && (TitanActive || (IfritActive && am Ziel stehend)) && !HasAnySearingLight)))`

- **Gruppenprüfung als Schalter** (`AnotherSummonerInParty`): ein lebender Beschwörer in der Gruppe, der
  die Stufe von Searing Light hat (`SearingLightPvE.Level`, aus den Aktionsdaten). Tote zählen nicht, denn
  sie zünden nichts; ein Beschwörer unter der Stufe hat keinen Buff zu geben, und ihn mitzuzählen hielte
  diesen hier für einen Buff zurück, der nicht kommen kann — derselbe Fehler, den `AnyLivingRaiser` beim
  Rotmagier gemacht hatte. Die Allianz wird nicht gefragt: Searing Light erreicht „nearby party members",
  ein Beschwörer in einer anderen Allianzgruppe verstärkt diesen Spieler nie.
- **Jede große Beschwörung** öffnet das Fenster, und `burstAboutToStart` öffnet den Platz vor jeder.
- **Ausweichen** (Punkt 6 seiner Vorgabe): außerhalb einer Burstphase nur, wenn **alle** Phasenarten
  dauerhaft belegt sind (`AllSearingPhasesHeld`), dann in den Titan-Block, in den Ifrit-Block nur am Ziel
  stehend, und nur auf einen vollständig abgelaufenen Buff.

**Was die Prüfung nicht leisten kann:** Sie sieht nicht, ob der andere Beschwörer RSR benutzt, ob seine
Rotation dieselbe ist oder ob er von Hand spielt. Sie beantwortet nur „kann überhaupt ein zweiter Searing
Light kommen" — und genau das ist die Frage, auf die es ankommt.

**Welcher Block beim Ausweichen.** Ifrit ist auf dem Papier der stärkste (632 Potenz je GCD gegen Titans
464), aber die Zahl enthält Crimson Cyclone, einen Gapcloser in den Nahkampf. Ohne Anlauf kehrt sich die
Rangfolge im Buff um: Titan drei Attacken für 1300, Ifrit eine für 800 (Begründung, „Was in die Restzeit
von Searing Light noch hineingeht"). Ifrit gilt darum nur, wo seine Voraussetzung ohnehin erfüllt ist:
**Am Ziel steht, wer 0 Yalm Abstand hat** — seine Präzisierung: „wenn der beschwörer bereits beim boss steht
(0 yalm), dann wäre der gapcloser nur noch damage und kein risiko." Gemessen wird von Trefferfläche zu
Trefferfläche (`ActionTargetInfo.StandsAtTarget`, A138): Die Trefferflächen berühren sich. Ob das Spiel
diesen Abstand ebenso misst und ganze Yalm abrundet — dann zeigte es „0" schon bis unter 1 Yalm —, ist
unbelegt. Dasselbe Maß liest die Sicherheitsprüfung der Gapcloser: Bei 0 Yalm verweigert sie Crimson Cyclone
nicht, auch wenn am Boss eine Gefahrenzone liegt, in der er ohnehin steht. Wo das Spiel den Sprung beendet
(Ringrand oder Berührung), ist unbelegt; bei 0 Yalm bleibt in beiden Fällen höchstens die eigene
Trefferfläche, und die fällt unter seine Grenze. `CrimsonCycloneDistance` bleibt die eigene Grenze des
Spielers für den Anlauf selbst. Auf einen fremden Ifrit-Block zu warten kostet nichts: Die Ladung bleibt
liegen, ihre Wiederholzeit beginnt erst mit der Zündung.

**Warum nur auf einen abgelaufenen Buff:** Die Sperre lässt eine Ladung in den letzten GCDs eines laufenden
Buffs zu. In einer Burstphase lohnt das, außerhalb verschenkt es den Rest eines fremden Buffs; ohne diese
Bedingung gäbe der Ausweichblock bei drei Beschwörern 1,4 Punkte Schaden unter Buff ab
(`searing_light_coverage.py`, „guard").

### Das Phasenbuch

**Das Buch beantwortet genau eine Frage:** Sind **alle** Phasenarten dauerhaft belegt? Es sagt **nicht**,
welche Phase anzustreben ist — angestrebt wird immer jede freie Burstphase, die gerade steht. Das ist der
Unterschied zwischen „für diese Phase zurückhalten" und „diese Phase aufgeben"; die Vorgabe sagt
ausdrücklich das Erste, und ihr Punkt 5 versucht Solar in der nächsten Runde erneut.

**Wie es bucht** (`SummonerRotation.UpdateSearingPhaseBook`, je Phasenwechsel einmal, fortgeschrieben in
`UpdateInfo`):
- Beim Betreten einer Phase mit fremdem Searing Light ist sie *gesehen*; war sie es schon beim letzten
  Betreten, ist sie *belegt*. Ein Betreten ohne fremden Buff löscht beides.
- Läuft der eigene Buff, bleibt das Fenster ungewertet und wird später in derselben Phase erneut gefragt —
  sonst löschte eine in Solar gesetzte Ladung beim Betreten von Bahamut dessen Eintrag auf die Kraft eines
  selbst gewirkten Buffs hin.
- **Bahamut und Phoenix buchen gemeinsam, wo Solar existiert** (`BookSlot`). Searing Light kommt nach
  120 Sekunden wieder, ein Beschwörungsfenster alle 60 — ein Zünder trifft also stets Fenster gleicher
  Parität. Ein fremder Cast in einem Solar-Fenster heißt: Er nimmt dauerhaft Solar; einer in Bahamut oder
  Phoenix: Er wechselt zwischen beiden, beide sind vergeben. Unterhalb von Solar wechseln Bahamut und
  Phoenix jedes Fenster, und ein Zünder kehrt in dasselbe zurück; dort buchen sie getrennt.
- **„Alle belegt" fragt nur die Phasen, die die Stufe hat.** Unterhalb von Solar oder Phoenix wäre die
  Klausel sonst in jedem stufensynchronisierten Inhalt unerreichbar.
- **Außerhalb des Kampfes wird das Buch geleert.** Ein neuer Kampf beginnt beim Verhalten ohne Ausweichen
  und erreicht die Klausel erst, wenn jede Phase zweimal belegt angetroffen wurde — auf Stufe 100
  frühestens nach vier Minuten.

**Warum ohne Zeitwert und ohne Urheber:** Die Rücksetzung geschieht durch die Beobachtung selbst — wer
aufhört zu zünden (gestorben, gegangen, Job gewechselt), wird beim nächsten Durchgang nicht mehr
angetroffen, und seine Phase kommt zurück. Teilen sich zwei fremde Beschwörer eine Phase, ist sie für
den eigenen Beschwörer gleichwohl verloren; der Merker ohne Urheber beantwortet die Frage, die zählt: Ist
diese Phase für mich zu haben? Was der Merker braucht, beobachtet der Client ohnehin: `IStatus.SourceId`
nennt den Urheber des laufenden Buffs. Abfragen fremder Abklingzeiten, die es nicht gibt, braucht die
Regel nicht.

### Die Beschwörung wartet nur, wo sie den Burst nicht verschiebt

Mit einem zweiten Beschwörer wartet die Beschwörung auf keinen abkühlenden Buff, nur auf einen, der bis
zum nächsten GCD bereit ist, und bei einem laufenden fremden Buff auf gar keinen. Wer außerhalb des
Solar-Fensters zündet, setzt Searing Light zu einem anderen Zeitpunkt auf Abklingzeit; wartete die
Solar-Beschwörung dann auf den Buff, verschöbe die Ausweichregel die teuerste Phase des Zyklus — eine
Verschiebung um ein Fenster kostete 1600 Potenz, mehr als der gesamte Zugewinn an Buffzeit. Das ist sein
Prüfvorschlag: kein Verzögern aller Demis, stattdessen Ausweichen auf die nächste Demi oder den stärksten
Primal.

**Die drei großen Beschwörungen teilen eine Abklingzeit.** Ihre Wirktexte sagen „does not share a recast
timer with any other actions"; die Action-Tabelle des Spiels führt alle drei samt Aethercharge in
derselben Abklingzeitgruppe 10, dazu die GCD-Gruppe als zweite (xivapi, `CooldownGroup` und
`AdditionalCooldownGroup`, 12.10.2026). `burstAboutToStart` liest auf Stufe 100 die Abklingzeit von Solar
und damit die gemeinsame.

### Der fremde Buff als Buff-Fenster

Painflare, Necrotize und Fester fallen in der Solar-Phase unter `inSolarUnique && HasAnySearingLight`
(`SMN_Reborn.AttackAbility`, dreimal). `HasAnySearingLight` fragt `PlayerHasStatus(false, …)`, also den
Buff gleich welcher Herkunft; `HasSearingLight` zählt nur den eigenen. Im Kampf: Ab zwei Beschwörern
landen die Aetherflow-Ausgaben im laufenden fremden Fenster statt daneben; bei einem Beschwörer fallen
beide Prüfungen zusammen. Ein Nachteil ist nicht erkennbar: Der Buff wirkt multiplikativ, unabhängig von
seiner Herkunft. Eine Defektbehebung: Die Bedingung soll „im Buff-Fenster" heißen und sagte „in meinem
Buff-Fenster"; der Auftraggeber hat das bestätigt.

**Die Burst-Medizin bleibt beim eigenen Buff** (`UseBurstMedicine` unter `HasSearingLight`): Sie ist eine
Fünfzehn-Minuten-Ressource und gehört in das eigene Solar-Fenster, das stärkste des Zyklus; an einen
fremden Buff gehängt landete sie irgendwo.

### Der Schutz gegen Überschreiben

Ein zweiter Searing Light auf einen laufenden ersten stapelt nicht, er **ersetzt** ihn — die Wirkung
bleibt bei 5 %, die Restzeit springt zurück auf 20 Sekunden (Angabe des Auftraggebers). Der Wert einer
Zündung hängt damit vom Zeitpunkt ab:

| Zündung von Beschwörer B, nachdem A gezündet hat | Nettogewinn |
|---|---|
| sofort (Restzeit 20 s) | 0 s — die Ladung ist vollständig verloren |
| nach 10 s (Restzeit 10 s) | 10 s |
| nach 19 s (Restzeit 1 s) | 19 s |
| nach Ablauf | 20 s, volle Wirkung |

`IsStatusProvided` (`ActionBasicInfo.cs:704`) sperrt die Zündung, solange der Status nicht innerhalb von
`StatusRefreshGcdCount` GCDs endet (`ActionConfig.cs:66`, ab Werk 2), also etwa fünf Sekunden vor Ablauf.
Entscheidend dafür ist `StatusFromSelf = false` in `ModifySearingLightPvE`: `PlayerHasStatus`
(`StatusHelper.PlayerHasStatus`, Quellenfilter in `AnyStatusMatches`) filtert nur bei `isFromSelf` auf die
eigene Quelle, hier zählt also jeder fremde Buff. Der Schutz verhindert die teuren Fälle und erlaubt die
billigen. **Er ist das einzige Abstimmungsmittel zwischen Clients, die einander nicht kennen**, und jede
Erweiterung des Zündfensters ist nur deshalb ungefährlich, weil er dahinter steht: Mehr Gelegenheiten
führen nicht zu mehr Überschreibungen, sondern zu mehr genutzten Lücken.

### Code-Stellen und Betroffene

| Ort | Was dort steht |
|---|---|
| `SummonerRotation.cs` | `HasAnySearingLight` (Buff gleich welcher Herkunft), `AnotherSummonerInParty`, `NextBigSummonIsBurst` (nächste große Beschwörung ist die Burst-Demi, aus der Beschwörungstaste und der Reihenfolge), Phasenbuch (`UpdateSearingPhaseBook`, `AllSearingPhasesHeld`, `BookSlot`) |
| `SMN_Reborn.AttackAbility` | Zündfenster, Platz vor der Beschwörung (`burstAboutToStart`), Schild vor der Demi, Rückhalt vor Wiederbelebung und Heilung (`higherGcdFirst`), Aetherflow-Ausgaben unter `HasAnySearingLight` |
| `SMN_Reborn.UseSummonsAndTrances` | Warten der Beschwörung (`searingSettled`, `RadiantAegisDueBeforeDemi`) |

Betroffen sind Endnutzer als Beschwörer und, über die geschützten Eigenschaften der Basisklasse, Autoren
abgeleiteter Beschwörer-Rotationen — additiv, ohne geänderte Signatur. Die Aktionseinstellungen und der
Sperrmechanismus bleiben unberührt.

## Begründung

### Der erste Burstschaden kommt nach der Beschwörung

`Summon Solar Bahamut` (`ActionId.resx`, 36992) nennt **keine Potenz**: „Enters Lightwyrm Trance and summons
Solar Bahamut to fight your target. Solar Bahamut will execute Luxwave automatically on the targets
attacked by you after summoning.“ Der Beschwörungs-GCD richtet also nichts aus; Luxwave (160) folgt den
**eigenen** Angriffen. Der erste Schaden der Phase ist der erste GCD danach — Umbral Impulse (640) samt
automatischem Luxwave, zusammen 800 Potenz.

**Der Platz hinter der Beschwörung ist umkämpft, der davor nicht.** Der Wirktext der Beschwörung gewährt
selbst Refulgent Lux („Additional Effect: Grants Refulgent Lux Duration: 30s“). Sobald sie aufgeht, wird Lux
Solaris wirkbar, und `HealAreaAbility` fragt die Kette **vor** `AttackAbility` (in `CustomRotation.Ability`
stehen die Heilzweige vor dem Angriffszweig); er kann den Platz dahinter nehmen, sobald die
Flächenheilungsflagge steht und die Zündregel von Lux Solaris sie freigibt (`LuxSolarisDecision`, Konzept 08,
„Wann Lux Solaris zuendet“). Dazu kommen dort Addle und ein Trank. Schluss aus Wirktext und
Zweigreihenfolge, keine Spielbeobachtung.

**Vor der Beschwörung kommen nur Schild und Addle in Betracht.** Die Zweigkette des Fähigkeitenpfads
(Konzept 03) stellt Notfall, Unterbrechung, Reinigung, Rettungsrückgriff, Haltung, Rückstoßschutz,
Positionierung, Flächen- und Einzelheilung, Tempo, Spott, Flächen- und Einzelverteidigung, Bewegung,
Trank und Phönixfeder vor den Angriffszweig, in dem Searing Light an erster Stelle steht. „Heilung oder
Verteidigung“ heißt beim Beschwörer Schimmerschild und Addle; seine einzige nennenswerte Heilung ist die
Flächenheilung aus einer laufenden Demi, und die kann **vor** der Beschwörung nicht feuern:
`ModifyLuxSolarisPvE` setzt `StatusNeed = [StatusID.RefulgentLux]`, `ModifyRekindlePvE` prüft `InPhoenix`
— beides entsteht **aus** der Phase. Addle fällt bei gesetzter Verteidigungsflagge; der Schild geht dort
absichtlich vor. Nimmt einer davon den Platz, wartet die Beschwörung einen GCD, und der Buff fällt in dessen
Platz. Eine Absicherung über `CanUse` als Prüfung scheidet aus; das ist eine Defektklasse aus `TODO.md`.

**Warum die Beschwörung wartet: sonst bleibt ein Rückstand für immer.** Gemeldet: „cooldown von searing
light ist später nicht fertig, wenn burst phase läuft." Die Abklingzeiten der Beschwörung und des Buffs
laufen ab ihrer **Nutzung**; fällt der Buff einmal hinter die Beschwörung, ist er in der nächsten
Solar-Phase um denselben Betrag zu spät bereit, und in jeder weiteren.
- **Ohne Warten:** Sind beide Plätze hinter der Beschwörung belegt, fällt der Buff hinter den ersten
  Umbral Impulse, und weil nichts ihn zurückholt, bleibt er dort in jeder folgenden Solar-Phase: 15 bis
  23 Potenz eigener Schaden je Zwei-Minuten-Zyklus, mehr mit jedem weiteren solchen Fall.
- **Mit Warten** wird der Rückstand jede Solar-Phase eingeholt. Die Beschwörung kommt einen GCD später,
  Searing Light geht in den Platz dieses GCDs. Der einzige Beschwörer setzt den Burst selbst: Solar und
  der Buff verschieben sich gemeinsam, und im Kampf ändert sich dadurch nur der Zeitplan der späteren
  Demis — spürbar erst am Kampfende, wo die letzte Demi-Phase knapper ausfallen kann.

*Dass hinter Ruby Rite kein Platz bleibt, folgt aus Code und Spieldaten: 2,8 s Gießzeit, 3,0 s
Wiederholzeit (xivapi, 12.10.2026).*

**Auch das Warten innerhalb der Demi ist teurer als sein Ertrag.** Hält man statt der Beschwörung den
ersten GCD der Phase an, bis die offenen Fähigkeiten gewoben sind, verschiebt sich keine Demi — die
Abklingzeit läuft ab der Beschwörung —, aber jeder folgende GCD. Der Zyklus verliert dadurch den
billigsten GCD anteilig: 0,2 bis 0,7 Sekunden kosten 32 bis 112 Potenz, gerechnet mit Ruin III
(400 je 2,5 s). Bei einem GCD unter 2,5 Sekunden kommt ein zweiter Verlust hinzu:
`smn_phase_potency.py` zählt dann sieben statt sechs Würfe in der Phase, der letzte Umbral Impulse
liegt nur 0,12 bis 0,6 Sekunden vor dem Ende der 15 Sekunden, und ein Clip darüber kostet ihn, rund
400 Potenz. Der Ertrag ist, was ein verpasster erster Platz überhaupt kostet: Der Buff fällt dann einen GCD
später, der erste Umbral Impulse (800 Potenz samt Luxwave) läuft ohne die 5 %, und das Buffende
rückt um denselben GCD nach hinten und trägt einen Primal-GCD mehr — mit Titan zuerst Topaz Rite 340,
mit Mountain Buster 500. Netto sind das 15 bis 23 Potenz eigener Schaden, 0,05 bis 0,08 Prozent des
Zwei-Minuten-Zyklus; die Aetherflow-Ausgaben der Phase warten ohnehin auf den Buff und verlieren
nichts. Für die Gruppe verschiebt sich das Fenster um denselben GCD; das ist nicht beziffert. Die übrigen
Fähigkeiten der Phase — Energy Drain, zweimal Necrotize, Exodus, Sunflare, Searing Flash, Lux
Solaris — brauchen kein Warten: rund acht auf zwölf Einschiebeplätze. *Clipdauer aus Ausführungssperre
und Latenz, beide nicht im Repository, der Code liest sie erst zur Laufzeit.*

**Warum Verzug Schaden kostet** (`ActionId.resx`, beides dort wörtlich): Searing Light wirkt
**20 Sekunden**, Summon Solar Bahamut dauert **15 Sekunden**. Zu Beginn gezündet deckt der Buff die ganze
Phase ab und läuft fünf Sekunden darüber hinaus — dieser Überhang ist eingeplant und harmlos. Jede
Sekunde Verzug tauscht dagegen eine gebuffte Sekunde **innerhalb** der Phase gegen eine **danach**: Die
Demi-GCDs tragen 947 bis 1217 Potenz, die Zwischenblöcke höchstens 632, und die fünf Prozent wirken auf
den jeweils darunterliegenden Wert. Ein Verzug von zwei bis drei GCDs verschiebt damit rund 30 Potenz je
GCD von der starken in die schwache Phase. Der Verlust trifft zudem nicht ihn allein: Der Buff gilt für
die nahen Gruppenmitglieder mit.

### Die Zeitstruktur

Die Größen außerhalb des Quelltextes stammen aus dem offiziellen Job-Guide und der Action-Tabelle des
Spiels (siehe „Grenzen").

| Größe | Wert |
|---|---|
| Searing Light, Wirkdauer | 20 s |
| Searing Light, Wiederholzeit | 120 s |
| Wirkung | +5 % Schaden für die ganze Gruppe |
| Große Beschwörung, Standzeit | 15 s |
| Große Beschwörung, Wiederholzeit | 60 s, gemeinsam für alle drei |
| Reihenfolge der großen Beschwörungen | Solar Bahamut → Bahamut → Solar Bahamut → Phoenix |

Daraus folgt die entscheidende Asymmetrie: **Pro 120 Sekunden gibt es zwei Beschwörungsfenster, aber
nur eines davon ist Solar Bahamut.** Hinge die Zündung allein an `burstInSolar` — die Ausgangslage
„nur Solar" aller Messungen unten —, hätte ein Beschwörer genau **eine** Gelegenheit pro Wiederholzeit,
und sie läge bei allen Beschwörern einer synchron gestarteten Gruppe zur selben Zeit. Für lückenlose
Abdeckung wären sechs Zündungen pro 120 Sekunden nötig (6 × 20 s).

**Ab zwei Beschwörern reicht das Solar-Fenster allein deshalb nicht:** Treffen zwei Beschwörer darin
aufeinander, verfällt eine Ladung. **Der begrenzende Faktor ist nicht die Wiederholzeit, sondern das
Zündfenster:** Sechs Beschwörer haben zusammen genug Ladungen für lückenlose Abdeckung; dass sie nicht
ankommt, liegt allein daran, wann gezündet werden darf.

**Die Rechnung des Auftraggebers geht auf, mit zwei Korrekturen.** Searing Light wirkt 20 Sekunden,
nicht 15 — die 15 sind die Standzeit der Beschwörung. Und die Wiederholzeit läuft ab der Zündung,
nicht ab Buff-Ende; ein Intervall von 135 Sekunden gibt es nicht, der Zyklus ist 120. Damit liegt die
Schwelle für rechnerisch lückenlose Abdeckung bei **sechs** Beschwörern, nicht erst bei acht. Die
Schlussfolgerung — bei acht Beschwörern wäre der Buff nahezu dauerhaft — ist richtig und wird von der
Messung bestätigt, allerdings nur unter den Bedingungen unten.

### Was eine Phase wert ist

**Ein Solar-Bahamut-Fenster trägt 7300 Potenz, ein Bahamut- oder Phoenix-Fenster 78 Prozent davon und
ein gleich langes Primalfenster 41 Prozent.** Der Zwei-Minuten-Zyklus ist damit nicht annähernd
gleichmäßig: Auf ein Viertel der Zeit entfällt gut die Hälfte des Schadens. Die Zahlen stammen aus
`.github/scripts/audit/smn_phase_potency.py`; Aufbau der Phasen aus der Dispatch-Reihenfolge in
`SMN_Reborn.cs`, Dauer 15 s aus dem Tooltip-Text der drei Beschwörungen in `ActionId.resx`, Einzelziel,
Stufe 100, ohne Searing Light.

| Fenster (15 s) | Potenz | je GCD | gegen Solar | gegen Primalfenster |
|---|---|---|---|---|
| Solar Bahamut | 7300 | 1217 | 100 % | 2,46× |
| Bahamut | 5700 | 950 | 78 % | 1,92× |
| Phoenix | 5680 | 947 | 78 % | 1,91× |
| Primal (Mittel) | 2970 | 495 | 41 % | 1,00× |

**Bahamut und Phoenix sind gleichwertig**, obwohl sie verschieden aussehen: Phoenix' stärkerer Füller
(580 gegen 500) gleicht genau aus, dass seine Astral-Flow-Aktion Rekindle heilt statt zu schaden,
während Bahamut dort Deathflare mit 500 Potenz hat. Der Vorsprung von Solar Bahamut kommt aus drei
Quellen zugleich: stärkerer Füller (640), stärkerer Begleitangriff (Luxwave 160 gegen 150) und vor
allem die beiden Abschlüsse Sunflare 1000 und Exodus 1500 gegen Deathflare 500 und Akh Morn 1300.

**Der Abstand zur Zwischenphase ist noch größer als die Tabelle zeigt.** RSR legt zusätzlich Energy
Drain, zweimal Necrotize und Searing Flash ins Solar-Fenster — Necrotize und Fester unter
`inSolarUnique && HasAnySearingLight`, Searing Flash, weil es erst durch Searing Light entsteht. Das sind
1800 Potenz obendrauf, 25 Prozent mehr; sie stehen nicht in der Tabelle, weil der Vergleich ohne Searing
Light geführt ist.

**Die Beschwörung selbst kostet einen GCD.** Die Action-Tabelle führt bei den Demi-Beschwörungen die
GCD-Gruppe als zweite Abklingzeitgruppe (A247), und RSR ruft sie aus `GeneralGCD` auf. Das Skript rechnet
damit; die Rangfolge wäre in beiden Lesarten dieselbe.

**Die Sonderaktion der drei Primals: Ifrit hat die stärkste, in jedem der drei sinnvollen Maße.** Neben
den beiden wiederholbaren Gemshine-Formen für Einzel- und Gruppenschaden gewährt jeder Primal genau eine
Sonderaktion; sie unterscheiden sich nicht nur in der Potenz, sondern auch darin, ob sie einen
GCD-Platz kosten.

| Sonderaktion | Potenz | GCDs | je GCD | Zugewinn gegenüber dem Füller |
|---|---|---|---|---|
| Ifrit — Crimson Cyclone + Crimson Strike | 1120 | 2 | 560 | +320 |
| Titan — Mountain Buster | 160 | 0 | — | +160 |
| Garuda — Slipstream | 520 | 1 | 520 | +120 |

Die letzte Spalte ist das eigentliche Maß: Eine Sonderaktion, die einen GCD belegt, verdrängt einen
Füller (Ruin III, 400) und ist nur die Differenz wert. Mountain Buster kostet keinen GCD und behält
deshalb seinen vollen Wert, bleibt aber absolut der kleinste Beitrag. Dieselbe Rangfolge ergibt sich
für die ganzen Blöcke: Ifrit 632 Potenz je GCD, Titan 464, Garuda 407.

### Was in die Restzeit von Searing Light noch hineingeht

**Zwei bis drei Attacken, zusammen 800 bis 1360 Potenz — und welche es sind, entscheidet nicht die
Potenz, sondern die Gießzeit.** Searing Light deckt 20 Sekunden, das Solar-Fenster 15; nach dem
letzten Beschwörungs-GCD bleiben bei 2,50 s Wiederholzeit 5,0 Sekunden Buff, also zwei GCD-Plätze und
die daran gewebten Fähigkeiten. Gezählt wird nicht nach Plätzen, sondern auf der Uhr: Ein Zauber, der
innerhalb des Buffs beginnt und nach seinem Ende fertig wird, bekommt ihn nicht.

| Zuerst gerufen | Attacken im Buff | Potenz | was hineingeht |
|---|---|---|---|
| **Ifrit** | 2 | **1360** | Inferno 800, Crimson Cyclone 560 |
| Titan (Voreinstellung) | 3 | 1300 | Earthen Fury 800, Topaz Rite 340, Mountain Buster 160 |
| Garuda | 1 | 800 | Aerial Blast 800 — **Slipstream fällt heraus** |

**Der Ausreißer ist Garuda, nicht Ifrit.** Slipstream beginnt auf dem zweiten Platz bei 17,5 Sekunden
und ist mit seiner Gießzeit erst nach dem Buffende fertig; der Buff greift beim Fertigwerden, nicht
beim Anfangen. Rechnerisch käme Slipstream mit Swiftcast auf 1320, und RSR hat dafür die Option
`AddSwiftcastOnGaruda` — **diese Möglichkeit steht hier nicht offen**, weil der Auftraggeber Swiftcast für
Wiederbelebungen zurückhält. Titan verliert dagegen nichts: Seine GCDs sind sofort wirksam, und Mountain
Buster wird gewebt — deshalb liefert Titan die meisten Attacken bei fast derselben Potenz.

**Bei schnellerer Wiederholzeit ändert sich die Rangfolge nicht.** Bei 2,45 s und 2,40 s trägt das
Beschwörungsfenster sieben statt sechs Zauber, die Restzeit schrumpft auf 2,85 beziehungsweise 3,20
Sekunden — es bleiben dieselben zwei Plätze und dieselben Werte.

**Wer nicht heranspringt, verliert bei Ifrit doppelt.** Crimson Strike entsteht erst aus Crimson
Cyclone („Grants Crimson Strike Ready", `ActionId.resx`); wird der Anlauf aus Sicherheitsgründen
ausgelassen, fällt der Ifrit-Block von 3160 Potenz über fünf GCDs auf 2040 über drei, und die beiden
frei werdenden Plätze gehen an den Füller zurück — 568 Potenz je GCD statt 632. Im Bufffenster ist der
Rückschlag größer, weil der Ersatz auf dem zweiten Platz Ruby Rite ist und Ruby Rite 2,8 s Gießzeit hat:
Bei 17,5 s begonnen, landet es nach dem Buffende.

| Zuerst gerufen, ohne Anlauf | Attacken im Buff | Potenz |
|---|---|---|
| **Titan** | 3 | **1300** |
| Ifrit | 1 | 800 |
| Garuda | 1 | 800 |

**Titan ist der einzige Block, dessen Wert nicht an der Position hängt.** The Balance empfiehlt Titan nach
der Beschwörung ebenso: „Titan first after the demi-primal phase should generally be prioritized" (Summoner
Basic Guide, abgerufen 02.10.2026).

**Die Reihenfolge der Primals bleibt, und das ist die Entscheidung des Auftraggebers.** Zwischen der besten
und der schlechtesten Reihenfolge liegen 560 Potenz innerhalb des Buffs; der Buff steigert um 5 Prozent,
also 28 Potenz gegen 28 990 Potenz Zyklusleistung — 0,1 Prozent. Zwischen Ifrit und der Voreinstellung
Titan sind es 60 Potenz, drei Potenz Schaden, 0,01 Prozent. Liegt der Buff ganz außerhalb eines
Beschwörungsfensters — im Ausweichblock —, füllen ihn 20 s Primalblock: Ifrit zuerst 4800 Potenz, Titan
zuerst 3920, Garuda zuerst 4420; zwischen bester und schlechtester Reihenfolge liegen dann 880 Potenz,
44 Potenz Buffwert, 0,15 Prozent des Zyklus. Ifrit zuerst lohnt nur, wenn man ohnehin am Ziel steht; der
Anlauf von Crimson Cyclone in eine Burstphase hinein ist ein Positionsrisiko, das 0,01 Prozent Schaden
nicht rechtfertigen. Titan ist sicher, erlaubt Bewegung und kostet drei Potenz Schaden je Zyklus.

**Zwei Einstellungen stützen diese Wahl.** `PreferTitanWhileMoving` zieht in `SummonPrimals` Titan bei
Bewegung vor, unabhängig von der eingestellten Reihenfolge; ab Werk an (seine Regel für
Voreinstellungen). `AddCrimsonCyclone`, ab Werk **aus**, überspringt laut seinem Optionstext und der
Bedingung vor Crimson Cyclone in `SMN_Reborn` — `AddCrimsonCyclone ||
CrimsonCyclonePvE.Target.Target.DistanceToPlayer() <= CrimsonCycloneDistance` — die Distanzprüfung: Mit ihr
springt RSR aus beliebiger Entfernung heran. Aus greifen die drei Yalm aus `CrimsonCycloneDistance`.

**Gieß- und Wiederholzeiten** (Action-Tabelle über xivapi, 12.10.2026, und Job-Guide, 02.10.2026): Ruby Rite
2,8 s / 3,0 s, Slipstream 3,0 s / 3,5 s, Ruin III 1,5 s / 2,5 s; Emerald Rite, Topaz Rite und Ruin IV
sofort, Emerald Rite mit 1,5 s Wiederholzeit. Die Demi-Beschwörungen kosten einen GCD (oben). Das Modell
rechnet damit (`smn_phase_potency.py`).

**Woher die Potenzen kommen.** Ohne Merkmalsaufwertung aus `ActionId.resx` in diesem Repository: Umbral
Impulse 640, Luxwave 160, Wyrmwave 150, Scarlet Flame 150, Sunflare 1000, Exodus 1500, Deathflare 500, Akh
Morn 1300, Revelation 1300, Necrotize 500, Searing Flash 700, Energy Drain 100. Wo ein Merkmal die Zahl im
Tooltip überschreibt, steht in `ActionId.resx` „with a potency of .“; diese vierzehn Werte stammen aus dem
offiziellen Job-Guide (Stufe 100, abgerufen 02.10.2026): Astral Impulse 500, Fountain of Fire 580, Ruin III
400, Ruin IV 520, Ruby Rite 620, Topaz Rite 340, Emerald Rite 280, Crimson Cyclone 560, Crimson Strike 560,
Slipstream 520, Mountain Buster 160, Inferno 800, Earthen Fury 800, Aerial Blast 800.

**The Balance** (Summoner Basic Guide, 02.10.2026): „Searing Light should be used on cooldown to align with
all other two minute party buffs, unless a specific buff delay is agreed upon by the entire party. Aligning
with other party buffs should be prioritized over aligning with your own burst phases." Für einen einzelnen
Beschwörer trifft RSR das: Die Abklingzeit von 120 s ist genau ein Demi-Zyklus, und die Zündung im
Beschwörungsfenster fällt mit „on cooldown" zusammen. Mehrere Beschwörer behandelt The Balance nicht; dort
gilt seine Vorgabe. Swiftcast auf Slipstream nennt The Balance als situativen Gewinn; nach seiner Spielweise
bleibt Swiftcast für Wiederbelebungen.

### Gemessen: was im Kampf ankommt

**Das Maß ist der Anteil des eigenen Schadens, der unter einem Buff fällt** — nicht die Zahl der
Sekunden mit Buff. Searing Light hebt den Schaden um 5 %, eine Sekunde ist also wert, was sie
produziert, und eine Solar-Sekunde trägt 1217 Potenz je GCD gegen 407 bis 632 in einem Primal-Block.
Sekunden zu zählen beantwortet eine andere Frage als „wo zahlt sich die Ladung aus"; die
Abdeckungstabellen weiter unten sind Zwischengröße.

**Die Gruppe, die tatsächlich vorkommt, ist die gemischte:** Ein Beschwörer folgt dieser Regel, die
anderen sind fremde Spieler mit eigener Rotation. Gemessen mit `searing_light_coverage.py`
(Abschnitt „V8"), drei Beschwörer, über alle Reihenfolgen gemittelt, wer bei gleichzeitiger
Gelegenheit zuerst zündet. Zwischen den Demis laufen die drei Primals in der voreingestellten
Reihenfolge Titan, Garuda, Ifrit; ausgewichen wird in den Titan-Block. „V2" ist die Regel ohne
Ausweichklausel (jede große Beschwörung mit zweitem Beschwörer), „V8" die gebaute mit ihr; „V8 (Plugin)"
ist das Buch, das `SummonerRotation` führt, „V8 (Modellbuch)" das Buch, aus dem die Regel hergeleitet ist.

**Gegenspieler, die nur Solar nutzen** — Bahamut und Phoenix bleiben frei, die Klausel greift nie:

| Lage | nur Solar | V2 | V8 (Plugin) | V8 (Modellbuch) |
|---|---|---|---|---|
| synchroner Pull | 26,4 % | 46,1 % | 46,1 % | 46,1 % |
| halb versetzt | 44,4 % | 44,4 % | 44,4 % | 44,4 % |
| voll versetzt | 59,4 % | 59,4 % | 59,4 % | 59,4 % |

**Gegenspieler, die jede Phasenart belegen** — die Lage, für die Punkt 6 gemacht ist:

| Lage | nur Solar | V2 | **V8 (Plugin)** | V8 (Modellbuch) |
|---|---|---|---|---|
| synchroner Pull | 47,6 % | 47,6 % | **53,6 %** | 57,1 % |
| halb versetzt | 44,4 % | 44,4 % | 44,4 % | 44,4 % |
| voll versetzt | 59,4 % | 59,4 % | 59,4 % | 59,4 % |

(Bei vier und fünf Beschwörern liegt V8 im Plugin bei 56,3 und 56,6 %, das Modellbuch bei 57,7 und
57,9 %; bei zwei greift die Klausel mangels Belegung nicht.)

**V8 ist nirgends schlechter als V2** und in der einen Lage, für die seine Klausel gebaut ist, um sechs
Prozentpunkte besser. Das ist die Bedingung, die eine Anpassung erfüllen muss — sie darf keine Lage
verschlechtern, um eine andere zu gewinnen; der Selbsttest des Modells hält sie für das Buch des
Plugins fest. **Sobald die Rotationen auseinanderlaufen, ist schon die reine Solar-Regel optimal** — jeder
Beschwörer setzt seinen Buff in seine stärkste Phase, und die Streuung schließt die Lücken von selbst.
Alle Unterschiede entstehen im synchronen Fall.

**Der Abstand zwischen Plugin und Modellbuch ist gemessen und zerlegt:**

| Unterschied | Wirkung bei drei Beschwörern | Stand |
|---|---|---|
| Bahamut und Phoenix als ein Paar buchen | +3,2 Punkte | gebaut |
| Ausweichen nur auf einen vollständig abgelaufenen Buff | +1,4 Punkte | gebaut |
| Beim Betreten der Phase buchen statt während des ganzen Fensters | Beobachtung über das ganze Fenster **kostet** 2,7 Punkte | Plugin bucht beim Betreten |
| Keine Rücksetzung, wenn eine Phase ohne fremden Buff betreten wird | +2,1 Punkte | nicht übernommen: Die Rücksetzung ist die Selbstheilung ohne Uhr — wer ausfällt, gibt seine Phase beim nächsten Durchgang frei |

Ohne die beiden gebauten Punkte erreichte das Plugin 49,0 % — anderthalb Punkte über V2. Das Modellbuch
führt je Phasenart den zuletzt beobachteten fremden Zünder samt Zeitstempel und lässt den Eintrag nach
Wiederholzeit plus Nachfrist verfallen; das Plugin bleibt beim Merker ohne Uhr und Urheber (Begründung
unter „Das Phasenbuch").

**Warum die Klausel nicht früher greifen darf, ist die Potenzdichte.** Solar trägt 1217 je GCD,
Bahamut 950, Phoenix 947; ein Primal-Block 407 bis 632. Eine ausgelassene Gelegenheit kostet dabei
wenig: Die Ladung bleibt liegen, ihre Wiederholzeit beginnt erst mit der Zündung, und die nächste
Burstphase kommt in höchstens 60 Sekunden. Erst wenn **keine** Burstphase mehr zu bekommen ist, ist
der Primal-Block besser als gar nichts — und dann ist er es deutlich.

**Der maßgebliche Bereich ist eins bis fünf.** Eine reguläre Achtergruppe trägt vier bis fünf
Schadensklassen, eine Vierergruppe zwei. Sechs und mehr Beschwörer sind Sondergruppen außerhalb des
regulären Spiels; sie bleiben im Modell abrufbar (`--all`), bestimmen aber keine Entscheidung.

### Abdeckung in Sekunden, ein bis fünf Beschwörer

Aus `.github/scripts/audit/searing_light_coverage.py`, das die Regeln durchrechnet: 20 s Wirkung, 120 s
Wiederholzeit ab Zündung, Überschreiben statt Stapeln, Beschwörungen 15 s alle 60 s in der Reihenfolge
Solar, Bahamut, Solar, Phoenix, und die Sperre, die fünf Sekunden vor Buff-Ende öffnet. Gemessen wird der
Anteil der Kampfzeit mit laufendem Buff. Die Spalte „Obergrenze" ist, was die Ladungen überhaupt hergeben:
*n* × 20 s je 120 s. V4 und V7 sind ausgeschlossene Regeln (unten).

**Synchrone Rotationen — der saubere Pull:**

| Beschwörer | nur Solar | V2 | V4 | V7 | Obergrenze |
|---|---|---|---|---|---|
| 1 | 17 % | 17 % | 17 % | 17 % | 17 % |
| 2 | 17 % | 33 % | 29 % | 33 % | 33 % |
| 3 | 17 % | 33 % | 42 % | 50 % | 50 % |
| 4 | 17 % | 33 % | 54 % | 66 % | 67 % |
| 5 | 17 % | 33 % | 67 % | 83 % | 83 % |

**Voll auseinandergelaufene Rotationen:**

| Beschwörer | nur Solar | V2 | V4 | V7 | Obergrenze |
|---|---|---|---|---|---|
| 1 | 17 % | 17 % | 17 % | 17 % | 17 % |
| 2 | 33 % | 33 % | 33 % | 33 % | 33 % |
| 3 | 50 % | 50 % | 50 % | 50 % | 50 % |
| 4 | 66 % | 66 % | 66 % | 66 % | 67 % |
| 5 | 67 % | 67 % | 67 % | 67 % | 83 % |

**Versatz hilft nur der reinen Solar-Regel.** In der zweiten Tabelle liegen alle Regeln gleichauf. Wo die
Beschwörungsfenster ohnehin gestreut sind, trifft schon die enge Regel die Lücken; die Erweiterungen
finden nichts mehr vor.

**Der Versatz ist der stärkste einzelne Hebel.** Der Auftraggeber nennt Tod, Bewegung und Betäubung;
sie sind kein Problem, sondern der Verbündete der Erweiterung:

| Ursache | Wirkung auf den Zyklus |
|---|---|
| Tod und Wiederbelebung | Beschwörungen und Wiederholzeiten laufen weiter, die Rotation setzt aber versetzt wieder ein; zusätzlich kostet die Schwäche nach der Wiederbelebung Schaden |
| Bewegung | Wirkzeitgebundene Zauber entfallen, die Beschwörungskette verschiebt sich um GCDs |
| Betäubung, Stille, Bewegungsunfähigkeit | dasselbe, in Stufen |
| Phasenwechsel ohne Ziel | die Beschwörung wird nicht gestartet, das Fenster verschiebt sich um bis zu 60 s |

Alle vier streuen die Beschwörungsfenster über die Zeit. Bei synchronen Rotationen liegen zwei
Fenster je 120 Sekunden; bei versetzten liegen bis zu *2n* Fenster verteilt, und jedes davon ist eine
Gelegenheit, die der Sperrmechanismus korrekt filtert. Bei acht Beschwörern und reiner Solar-Regel hebt der
Versatz die Abdeckung allein von 17 % auf 67 %.

**Mehr Beschwörer heißt nicht immer mehr Abdeckung.** Außerhalb des regulären Bereichs liefert V2 bei
halbem Versatz mit sieben Beschwörern 84 % und mit acht 83 %, und V5 bricht bei sieben von 99 % auf 60 %
ein — gierige Zuteilung: Wer zuerst in einem Fenster steht, zündet, und kann jemandem zuvorkommen, dessen
Wiederholzeit eine spätere Lücke gedeckt hätte. Für die Entscheidung ohne Belang; der Selbsttest des
Skripts verlangt deshalb keine Monotonie zwischen Beschwörerzahl und Abdeckung.

### Abdeckung ist nicht Schaden — die Gegenprobe

Raidverstärkungen und Abklingzeiten der ganzen Gruppe sind auf das Burst-Fenster gebündelt, sodass dort ein
Anteil des Schadens liegt, der weit über dem Anteil an der Zeit liegt. Eine Regel, die viel Abdeckung an
den falschen Stellen erzeugt, wäre deshalb möglicherweise schlechter als eine, die wenig Abdeckung genau
im Burst liefert.

**Gemessen ist sie es nicht.** Geprüft an V7, der ausgeschlossenen Regel mit der meisten Buffzeit außerhalb
des Bursts, also am strengsten Fall; für V8 in der Fassung des Plugins hält der Selbsttest dieselbe
Invariante fest. Das Modell gewichtet jede Sekunde mit der Schadensdichte. Wie hoch der Burst-Anteil
tatsächlich ist, hängt von der Gruppenzusammensetzung ab und ist aus diesem Repository nicht zu klären —
deshalb steht er als Parameter. Die Spalte „bestmöglich" ist die optimale Platzierung derselben Ladungen:
erst das Burst-Fenster, dann der Rest.

**Burst-Anteil 30 % (die mittlere der geprüften Annahmen):**

| Beschwörer | nur Solar | V2 | V7 | bestmöglich |
|---|---|---|---|---|
| 1 | 30 % | 30 % | 30 % | 30 % |
| 2 | 30 % | 44 % | 44 % | 44 % |
| 3 | 30 % | 44 % | 58 % | 58 % |
| 4 | 30 % | 44 % | 71 % | 72 % |
| 5 | 30 % | 44 % | 85 % | 86 % |

**Burst-Anteil 45 % — die für V7 ungünstigste der geprüften Annahmen**, weil sie den einen gut
platzierten Buff der reinen Solar-Regel am stärksten aufwertet:

| Beschwörer | nur Solar | V2 | V7 | bestmöglich |
|---|---|---|---|---|
| 1 | 45 % | 45 % | 45 % | 45 % |
| 2 | 45 % | 55 % | 55 % | 56 % |
| 3 | 45 % | 56 % | 66 % | 67 % |
| 4 | 45 % | 56 % | 77 % | 78 % |
| 5 | 45 % | 56 % | 88 % | 89 % |

**In allen geprüften Annahmen liegt die Regel mit der meisten Buffzeit innerhalb eines Prozentpunkts am
Optimum,** weil sie das Burst-Fenster nicht aufgibt: Der erste Zünder steht in seinem Solar-Fenster, das
mit dem Burst der Gruppe zusammenfällt; die übrigen füllen nur die Zeit danach. Wer den Burst gedeckt hat,
ist genau 120 Sekunden später — zum nächsten Burst — wieder bereit. **Die reine Solar-Regel verschenkt
umso mehr, je stärker gebündelt wird:** Bei 45 % Burst-Anteil und fünf Beschwörern deckt sie 45 % des
Schadens ab, möglich wären 89 %.

**Die Burst-Abdeckung wird getrennt gemessen**, weil eine gewichtete Gesamtzahl eine Regression verstecken
kann (Einwand des Auftraggebers): Ein Buff im Burst steigert einen Anteil des Burst-Schadens, ein Buff in
der Zwischenphase denselben Anteil eines viel kleineren Schadens. Bei jeder Regel, jeder Beschwörerzahl und
jeder Versatzstufe bleibt sie bei 99 bis 100 Prozent — **es wandert nichts aus dem Burst heraus**, weil die
Ladung, die den Burst deckt, dauerhaft an ihn gebunden bleibt. Damit gilt seine Folgerung: Sekunden zählen
hier genauso wie Schaden, solange der Burst gedeckt bleibt; der Selbsttest hält fest, dass keine
Erweiterung die Burst-Abdeckung senkt.

**Zwei Eigenschaften der Messung tragen das, und beide stehen als Invariante im Selbsttest:** Steht die
Burst-Abdeckung durchgehend auf 0 %, ist 0 nicht kleiner als 0, und der Vergleich bliebe still — der
Selbsttest verlangt deshalb zusätzlich, dass ein einzelner Beschwörer seinen eigenen Burst tatsächlich
deckt. Und die Toleranz ist begründet gesetzt: Der einzige gemessene Rückgang — 0,8 Prozentpunkte bei zwei
Beschwörern — schrumpft mit zehnfach feinerem Zeitraster auf 0,14; er skaliert mit der Rasterweite und ist
Diskretisierung, kein Verlust.

**Phasen ohne Ziel sind keine Modellgrenze.** Eine Ladung auf einen Abschnitt ohne Gegner wäre reiner
Verlust, aber der Pfad gibt das nicht her: `AttackAbility` wird in `CustomRotation.Ability` nur unter
`HasHostilesInRange` aufgerufen.

## Ausgeschlossen

- **V4 — Zündung ganz von der Beschwörung lösen** (zünden, sobald kein Buff steht, die eigene Wiederholzeit
  frei ist und ein Ziel im Kampf vorliegt; nach diesem Ansatz jenseits der Beschwörungsfenster hat der
  Auftraggeber ausdrücklich gefragt). Im synchronen Fall die höchste Abdeckung erst bei vielen Beschwörern
  (100 % bei acht gegen 33 % mit V2), bei auseinandergelaufenen Rotationen fast gleichauf. **Im
  tatsächlichen Nutzungsprofil schlechter:** Bei zwei Beschwörern synchron 29 % gegen 33 % — es zündet
  früher und bringt die Wiederholzeiten in eine ungünstigere Lage; bei einem Beschwörer verliert es die
  Bündelung mit dem stärksten eigenen Fenster. Es gewinnt erst ab drei, deutlich erst ab sechs Beschwörern —
  Gruppen, die es im Spiel praktisch nicht gibt. Spielt er je in einer Gruppe mit sechs oder mehr
  Beschwörern, liegen die Zahlen hier vor.
- **V7 — außerhalb eines Fensters zünden, sobald der Buff abgelaufen ist, ohne Buchführung.** In Sekunden
  trifft es beim synchronen Pull die Obergrenze; im eigenen Schaden gewinnt es eine Lage und verliert zwei:
  gegen Gegenspieler, die jede Phase belegen, vorn, gegen Gegenspieler auf Solar und bei zwei Beschwörern
  auf derselben Regel mit vollem Versatz unter V2 — es gibt die eigene Burstphase auf, ohne dass eine
  Kollision vorliegt. V8 entscheidet dasselbe Ausweichen aus der Lage statt blind; beides nebeneinander
  hieße, dass die blinde Bedingung die überlegte jedes Mal überholt. Aus V7 übernommen ist die Bedingung
  „vollständig abgelaufen".
- **V5 und V6 — Buch über die Wiederholzeiten der anderen**, mit Zurückstehen, wenn ein anderer die Lücke
  decken könnte (V6 mit Verfallsdatum). Die Information ist verfügbar (`IStatus.SourceId`), beantwortet
  aber keine Frage, die für die eigene Entscheidung zählt: Ein anderer Beschwörer kann ohnehin nicht zünden,
  bevor seine Wiederholzeit frei ist; kann er, zündet er, und die Sperre verhindert die Verschwendung. Im
  maßgeblichen Bereich liefern beide dieselben Werte wie die Fassung ohne Buch. Bei einem dreiminütigen
  Ausfall eines Beschwörers kostet das Buch ohne Verfallsdatum die Hälfte (11, 11, 22, 33 % gegen 11, 22,
  44, 66 % bei zwei bis fünf Beschwörern): Wer Buch führt, hält seine Ladung zurück, weil der Ausgefallene
  „dran" wäre, und der zündet nie. Das Verfallsdatum heilt nur diesen selbst verursachten Schaden. Bei
  sieben und acht Beschwörern fällt V5 auf 46 % gegen 100 %. Ebenso ausgeschlossen ist das Buchen schon
  beim ersten Zuvorkommen: Es gab Phasen auf, die niemand hielt — bei fünf Beschwörern und vollem Versatz
  53 gegen 78 Prozent.
- **Einem Beschwörer, der in der Gruppe steht, aber nie zündet, die stärkste Phase überlassen.** Gemessen
  beim synchronen Pull ein Prozentpunkt Gewinn, bei halbem Versatz zwei Punkte Verlust: Die Regel kann
  „durch mich blockiert" nicht von „diesen Zyklus still" trennen.
- **`BahamutBurst` der Basisklasse statt des eigenen Zündausdrucks.** Die kleinste Textänderung, aber mit
  zweiter Verhaltensänderung: `BahamutBurst` ist an `CanBurst` gebunden, also an `AutoStatus.Burst`, der nur
  gesetzt ist, wenn der Nutzer den Burst-Befehl gibt oder `Service.Config.AutoBurst` an ist
  (`StatusFromCmdOrCondition` in `StateUpdater.cs`); `burstInSolar` prüft das nicht. `SMN_Reborn` benutzt
  `BahamutBurst` nicht.
- **Nullvariante (nur Solar).** Für einen Beschwörer richtig und so gebaut: Die Gruppenprüfung lässt ihn
  bei Solar. Ab zwei nicht tragfähig, weil zwei Beschwörer in einer Gruppe gewöhnlich sind und die Ladungen
  dann kollidieren.
- **Addle hinter Searing Light zurücktreten lassen**, solange er vor dem Castende des Gegners liegt (sein
  Hinweis: sein Wert hängt nur daran, dass er vor dem Treffer steht). Nicht gebaut: Es beträfe nur den
  seltenen Fall, dass beide Plätze hinter der Beschwörung belegt sind, und gewönne dort 15 bis 23 Potenz;
  dafür bräuchte die Verteidigungsregel das Castende als Bedingung.

## Grenzen

**Am Quelltext belegt:** die Kette von der Zündbedingung über die Aktionseinstellung und die Sperrlogik bis
zu den Vorgabewerten und der Filterung nach Statusquelle.

**Am offiziellen Job-Guide belegt** (02.10.2026): Searing Light +5 % für 20 s, 120 s Abklingzeit, und es
gewährt Ruby's Glimmer für 30 s bei der eigenen Ausführung; die großen Beschwörungen haben 60 s Abklingzeit.
Die gemeinsame Abklingzeit und der GCD der Beschwörungen stehen in der Action-Tabelle (xivapi). Die
Reihenfolge Solar Bahamut, dann Bahamut oder Phoenix im Wechsel, nennt The Balance. Dass ein zweiter Searing
Light überschreibt statt zu stapeln, ist die Angabe des Auftraggebers; eine schriftliche Quelle dafür ist
nicht gefunden.

**Die Abdeckungszahlen sind eine Messung am Modell, keine am Spiel**
(`.github/scripts/audit/searing_light_coverage.py`, in der CI mit Selbsttest). Das Modell:
- kennt keinen Standort: Der Ausweichblock ist immer Titan, der Ifrit-Block am Ziel ist nicht abgebildet;
- zählt Sekunden mit Buff, nicht Schaden, außer in den potenzgewichteten Tabellen;
- teilt Fenster gierig zu: Wer zuerst darf, zündet. Real entscheidet der Zufall des Sekundenbruchteils;
  zünden zwei Beschwörer im selben Augenblick, verschwendet einer, und das Modell bildet das nicht ab. Die
  erweiterten Fenster verteilen die Gelegenheiten und machen die Kollision seltener, nicht häufiger;
- modelliert den Versatz gleichmäßig und über den ganzen Lauf fest. Real wächst er mit der Kampfdauer;
  das Modell zeigt, dass keine der Regeln eine Anlaufzeit braucht, aber nicht, wie schnell eine Gruppe von
  der synchronen in die versetzte Tabelle wandert. Die Richtung steht fest — sie verbessert die Lage —,
  ihr Tempo nicht;
- startet alle Beschwörer mit freier Wiederholzeit. Den Fall, in dem die weite Fassung des Zündfensters
  von einer engen („ausweichen nur nach tatsächlicher Blockade") abweicht, kann es darum nicht erzeugen
  (offen, `TODO.md`).

**Der Burst-Anteil ist eine Annahme;** der Schaden außerhalb des Bursts ist als gleichmäßig modelliert, was
er nicht ist — die Beschwörungsfenster sind selbst Spitzen (das Zwei- bis Zweieinhalbfache eines gleich
langen Primalfensters). Weil die Regel gerade diese Fenster mit abdeckt, wird ihr Vorsprung unterschätzt,
nicht überschätzt.

**Zweitverwendung des Modells für die Streckung der Minderung (Konzept 08) trägt nicht ohne
Verallgemeinerung:** Es kennt nur eine Aktion (`BUFF` 20 s, `RECAST` 120 s als Konstanten), die Streckung hat
ungleiche Quellen; und es rechnet mit Überschreiben (`buff_until = t + BUFF   # overwrite, never stack`),
Minderungen stapeln dagegen multiplikativ — „Strecken statt stapeln" ist dort eine Vorgabe des
Auftraggebers, keine Spielmechanik. Übertragbar ist die Zeitschritt-Simulation mit Quellen, Dauer,
Wiederholzeit und Zündregel, samt der Trennung „Abdeckung ist nicht Schaden"; Quellenliste und
Stapelverhalten müssten dafür Parameter werden.

## Offene Punkte

Sie stehen in `TODO.md`, dort je Eintrag mit **Konzept:** auf dieses Dokument gekennzeichnet;
`.github/scripts/audit/check_concept_links.py` listet sie je Konzept.

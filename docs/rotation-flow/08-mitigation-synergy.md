# 08 · Synergie von Schadensvermeidung und Schadenserzeugung

Entwurfsdokument nach ADR-Struktur. Beschreibt einen Mechanismus, kein umgesetztes
Verhalten: Stand dieses Dokuments ist ein Konzept ohne Code.

## Kontext

Ausgangspunkt war die Frage, wie Sanctus (Holy) eingesetzt wird und ob seine
Betäubungswirkung berücksichtigt wird. Die Prüfung ergab: sie wird nicht
berücksichtigt, und die Frage öffnet ein allgemeineres Thema. RSR trifft
Mitigationsentscheidungen heute **je Werkzeug und reaktiv**. Es gibt keine Stelle,
an der die Frage beantwortet wird, wie viel Schadensvermeidung gerade anliegt und
ob ein weiteres Werkzeug daran noch etwas ändert.

Das ist nicht nur eine Frage der Mitigation. Jedes Werkzeug, das redundant fällt,
kostet einen GCD oder einen Weave-Slot, der Schaden hätte erzeugen können. **Die
Vermeidung von Überlappung dient beiden Zielen gleichzeitig** — das ist der
Leitgedanke dieses Konzepts und der Grund, warum es nicht als reine
Mitigationsoptimierung geführt wird.

### Rechenbeispiel als Größenordnung

In Sekunden-Äquivalenten verhinderten Schadens, bei gleichmäßigem Gegnerschaden.
Reprisal senkt den Gegnerschaden um 10 % für 10 s; die Betäubung durch Sanctus
wirkt 4 s, bei Wiederholung 2 s und 1 s, danach ist das Ziel 45 s immun
(Resistenzstufen, Fremdquelle).

| | Betäubung im Reprisal-Fenster | Betäubung außerhalb |
|---|---|---|
| Betäubungsanteil | 4 s × 100 % = 4,0 | 4 s × 100 % = 4,0 |
| Reprisal-Anteil | 6 s × 10 % = 0,6 | 10 s × 10 % = 1,0 |
| **Summe** | **4,6** | **5,0** |

Über einen 90-Sekunden-Pull mit zwei Reprisal-Anwendungen und zwei
Resistenzzyklen: getrennt 2,0 + 14,0 = **16,0**, vollständig überlappt
14,0 + 0,6 = **14,6**. Rund ein Zehntel der gesamten Mitigation geht an der
Überlappung verloren, dazu die GCDs, die dafür aufgewendet wurden.

Die Zahlen sind ein Modell, keine Messung. Annahmen: gleichmäßiger Schaden, keine
Bewegung, alle Werkzeuge verfügbar. Sie begründen die Größenordnung des Themas,
nicht die Auslegung einer einzelnen Bedingung.

### Skalierung mit der Gegneranzahl

Die Tabelle rechnet pro Gegner. Der absolute Nutzen ist mit der Zahl der
betroffenen Gegner zu multiplizieren, weil jede der drei Quellen flächig wirkt:
die Betäubung auf alle Ziele im Wirkbereich von Sanctus, Reprisal auf alle nahen
Gegner, die Verlangsamung auf alle, die den Träger angreifen. Aus 1,4
Sekunden-Äquivalenten je Gegner über einen 90-Sekunden-Pull werden bei fünfzehn
Gegnern einundzwanzig.

Daraus folgt eine Entwurfsvorgabe, keine bloße Beobachtung: **Die Regeln dieses
Konzepts sind an eine Gegnerzahl-Schwelle zu binden.** Im Einzelzielkampf ist der
Gewinn vernachlässigbar und die zusätzliche Bedingung reines Risiko; im großen Pull
— dem Fall, in dem die Heilerlast überhaupt kritisch wird — ist er entscheidend.

Für die Betäubung verschärft sich das noch: Sanctus fällt nach `AoeCount = 3`
ohnehin erst ab drei getroffenen Zielen, und Bossgegner sind in aller Regel
betäubungsimmun. Die Frage stellt sich also von vornherein nur im Flächenkampf.

Das Projekt hat für solche Schwellen bereits ein Muster:
`Service.Config.MitigationSustainHostileCount` (`Configs.cs:757-759`, Bereich 1–8,
Standard 4) steuert, ab wie vielen Gegnern in Reichweite ein Mitigations-Debuff
ohne Vorhersage nachgezogen wird. Dieselbe Größe — `NumberOfHostilesInRange` — ist
hier zu verwenden, statt eine zweite Zählweise einzuführen.

## Vorhandene Bausteine

Der Entwurf erfindet wenig; das meiste liegt bereits im Baum und ist nur nicht
verbunden.

| Baustein | Fundstelle | Zustand |
|---|---|---|
| Vollständige Mitigationsmessung: Gegner-Debuffs (Addle, Feint, Dismantle, Reprisal) und Party-Buffs (Tank-LB3, Sacred Soil, Temperance, Kerachole, Troubadour-Familie, Dark Missionary u. a.), verrechnet zu einem Schadensfaktor | `CustomRotation_OtherInfo.cs:534` | Wird an genau einer Stelle gelesen: `RotationConfigWindow.cs:4863`, also **nur zur Anzeige**. Keine Entscheidung liest sie |
| Vorhersagefenster aus der BossModReborn-Timeline | `Configs.cs:742` (`BMRRaidwideMitWindow`, 5 s), `:747` (`BMRTankbusterMitWindow`, 3 s), ausgewertet in `StateUpdater.cs:185` und `DataCenter.cs:2640` | In Betrieb, treibt die `AutoStatus`-Flags |
| Zentralisierte Nachzieh-Regel für Gegner-Debuffs | `CustomRotation_OtherInfo.cs:1327` (`ShouldSustainMitigationDebuff`) | In Betrieb, 27 Aufrufstellen. Beispiel dafür, dass eine gemeinsame Regel für viele Jobs tragfähig ist |
| Ereignisstrom eigener Aktionen samt angewandter Statuseffekte je Ziel | `Watcher.ActionFromSelf` → `DataCenter.AddActionRec`, `DataCenter.ApplyStatus`, `DataCenter.AttackedTargets` | In Betrieb. Trägt die Daten, die eine Zählung von Resistenzstufen bräuchte |
| Trennung von Mitigation und Schaden im Dispatch | `CustomRotation_GCD.cs`: HealArea 240, HealSingle 282, DefenseArea 322, DefenseSingle 337, GeneralGCD erst 449 | In Betrieb. Mitigation und Heilung haben Vorrang; der Schadenszweig läuft nur, wenn kein Flag gesetzt ist |

## Die Lücke

**Aktionen mit doppelter Wirkung sind nur nach einer ihrer Wirkungen eingeordnet.**
Sanctus steht im Schadenszweig (`WHM_Reborn.cs:518-527`); dass es betäubt, ist im
Entscheidungsmodell nicht vorhanden. Assize steht im Angriffs-oGCD
(`WHM_Reborn.cs:308`); dass es heilt, ebenfalls nicht. Umgekehrt kennt
`GetCurrentMitigationPercent` die Wirkung von Reprisal, aber keine Betäubung und
keine Verlangsamung, weil beide keine Schadensreduktion im engeren Sinn sind,
obwohl sie so wirken.

Daraus folgen zwei beobachtbare Effekte:

1. Mitigationsquellen überlappen unkontrolliert, weil niemand fragt, was schon
   anliegt.
2. Die Doppelwirkung wird nicht genutzt: Der Schadenszweig weiß nicht, dass eine
   seiner Aktionen zugleich Schaden vermeidet, und der Verteidigungszweig weiß
   nicht, dass eine Schadensaktion seine Aufgabe teilweise erledigt.

## Optionen

**O0 — Nullvariante.** Verhalten belassen. Kosten: die oben bezifferte Überlappung
bleibt, `GetCurrentMitigationPercent` bleibt eine reine Anzeige.

**O1 — Je Rotation einzeln lösen.** Jede Jobdatei bekommt ihre eigenen
Bedingungen. Erfüllt die Anforderung, verstößt aber gegen die
Defektklassen-Regel: dasselbe Muster würde für jeden Job neu geschrieben und
altert je Datei getrennt.

**O2 — Zentrale Auslösung.** Ein gemeinsamer Trigger, der Mitigation anfordert,
wenn zu wenig anliegt. Genau diese Bauform wurde in diesem Fork schon einmal
gebaut und zurückgebaut: `HasHostileCountAoeMitigation` setzte `AutoStatus.DefenseArea`
und öffnete damit die gesamte Defensivkette des Jobs statt der einen gemeinten
Zeile. Ein zentraler Öffner hat im Fehlerfall den größten Wirkungsbereich.

**O3 — Zentrale Bremse, dezentrale Auslösung.** Die vorhandenen Auslöser bleiben
unverändert; hinzu kommt nur eine Prüfung, die ein Werkzeug **zurückhält**, solange
eine gleichwertige oder stärkere Quelle bereits wirkt. Fällt die Prüfung aus,
verhält sich RSR wie heute.

**O4 — Rückbau.** `GetCurrentMitigationPercent` aus der UI entfernen, Thema
schließen. Verwirft eine funktionsfähige Messung, die für dieses Thema die
Grundlage ist.

## Abwägung

| Option | Schweregrad des gelösten Problems | Aufwand | Blast Radius | Folgekosten |
|---|---|---|---|---|
| O0 | — | keiner | keiner | Überlappung bleibt, rund ein Zehntel der Mitigation |
| O1 | mittel | hoch, je Job | je Datei klein | Wartung an vielen Stellen, Defektklasse bleibt offen |
| O2 | mittel | mittel | **sehr groß** — ein Flag öffnet ganze Ketten | belegter Rückbau als Präzedenz |
| O3 | mittel | mittel | klein je Fundstelle, additiv | eine zusätzliche zentrale Funktion |
| O4 | — | klein | klein | Messung verloren |

**Gewählt: O3.** Ausschlaggebend ist das Ausfallverhalten. Eine Bremse, die
irrtümlich nicht greift, führt zum heutigen Verhalten zurück. Ein Öffner, der
irrtümlich greift, feuert die gesamte Defensivkette — das ist der Unterschied
zwischen einem Rückschritt und einem Defekt, und der Fork hat für die zweite
Variante bereits bezahlt.

## Entwurf

Drei Schichten, die einzeln nutzbar und einzeln abschaltbar sind. Jede Schicht ist
für sich lieferbar; keine setzt die folgende voraus.

### Schicht 1 — Messung sichtbar machen

`GetCurrentMitigationPercent()` von der Anzeige in die Entscheidung heben. Dazu
gehört, die Erhebung um die beiden Träger zu ergänzen, die heute fehlen, obwohl sie
wie Schadensreduktion wirken:

- **Betäubung** auf Gegnern — Wirkung 100 % für ihre Restdauer, aber nur auf den
  betäubten Zielen, nicht auf der Gruppe.
- **Verlangsamung** durch Arm's Length — wirkt nur auf Gegner, die den Träger
  angreifen, und erhöht zusätzlich deren Castzeiten.

Beide sind zielbezogen, nicht gruppenbezogen. Das erzwingt eine Erweiterung des
Rückgabewerts: ein einzelner Faktor für die Gruppe reicht nicht mehr. Vorschlag:
die bestehende Funktion unverändert lassen und eine zweite danebenstellen, die je
Ziel antwortet. Damit bleibt die UI-Anzeige stabil.

### Schicht 2 — Nicht-Überlappung als Rückhalteregel

Eine gemeinsame Prüfung nach dem Muster des vorhandenen
`ShouldSustainMitigationDebuff`: Ein Werkzeug wird zurückgehalten, wenn eine
Quelle mit **mindestens gleicher Wirkung** auf demselben Ziel bereits läuft und
deren Restdauer die eigene Wirkung überdeckt.

Drei Eigenschaften sind entwurfsentscheidend:

- **Nur zurückhalten, nie auslösen.** Die Regel darf kein `AutoStatus`-Flag setzen.
- **Vorbehalt bei Gefahr.** Unterhalb einer HP-Schwelle oder bei vorhergesagtem
  Schaden innerhalb des Mitigationsfensters wird nicht zurückgehalten. Eine
  Effizienzoptimierung von rund zehn Prozent darf keinen Tank kosten. Die
  vorhandenen Fenster `BMRRaidwideMitWindow` und `BMRTankbusterMitWindow` sind
  dafür die natürlichen Schwellen.
- **Erst ab einer Gegnerzahl.** Unterhalb der Schwelle greift die Regel nicht,
  weil der Gewinn dort im Rundungsbereich liegt, das Risiko einer
  zurückgehaltenen Mitigation aber unverändert besteht. Maß ist
  `NumberOfHostilesInRange`, Vorbild ist `MitigationSustainHostileCount`.

### Schicht 3 — Doppelnutzen abbilden

Aktionen, die Schaden erzeugen und zugleich Schaden vermeiden, bleiben in ihrem
bisherigen Dispatch-Zweig. Neu ist allein, dass ihre Mitigationswirkung in Schicht 1
sichtbar wird und Schicht 2 sie berücksichtigt. Für Sanctus heißt das: Die
Rotation castet es weiterhin im Schadenszweig, aber die Betäubung wird als
Mitigationsquelle geführt, und ein Werkzeug, das dieselbe Wirkung schwächer
liefert, wird währenddessen zurückgehalten.

Der umgekehrte Weg — Sanctus in den Verteidigungszweig zu verschieben — wird
verworfen: Er würde den Schadensfiller vom Mitigationsflag abhängig machen und
damit den DPS-Anteil der Rotation an eine Bedingung hängen, die dafür nicht gebaut
ist.

### Voraussetzung, die noch fehlt

Die Resistenzstufe einer Betäubung ist aus dem Statuseffekt nicht ablesbar. Sie
muss aus dem Ereignisstrom mitgezählt werden: `Watcher.ActionFromSelf` liefert je
Aktion die angewandten Statuseffekte je Ziel (`DataCenter.ApplyStatus`), daraus ist
je Gegner-Id eine Zählung mit Zeitstempel und 45-Sekunden-Verfall aufzubauen. Ohne
diese Zählung kann Schicht 1 die Betäubung nur als „liegt an / liegt nicht an"
führen, nicht als „wirkt noch". Das genügt für Schicht 2, nicht für eine gezielte
Streckung der Betäubungen.

## Falsifikation

**Hypothese 1: Es liegt kein Defekt vor.** Vertreten mit dem Argument, dass die
Überlappung eine kleine Ineffizienz ist und RSR reaktiv korrekt handelt.
Widerlegt durch zweierlei: die bezifferte Größenordnung von rund einem Zehntel der
Gesamtmitigation, und die Existenz von `GetCurrentMitigationPercent`, das die
gemeinte Größe bereits berechnet und nur nicht verwendet — die Entwurfsabsicht ist
also vorhanden, die Verdrahtung fehlt.

**Hypothese 2: Die gewählte Option ist falsch.** Vertreten mit dem Argument, dass
eine zentrale Regel den Blast Radius unnötig vergrößert und O1 (je Rotation) sicherer
wäre. Widerlegt für die Bremse, nicht für einen Auslöser: Eine Rückhalteregel kann
im Fehlerfall nur zum heutigen Verhalten zurückführen, während der bereits
zurückgebaute zentrale Öffner das Gegenteil zeigte. Für Schicht 3 hält der Einwand
teilweise stand, weil Doppelnutzen je Job unterschiedlich aussieht; deshalb ist
Schicht 3 auf die Sichtbarmachung beschränkt und ändert keine Dispatch-Zuordnung.

**Was nicht widerlegt ist:** dass der Nutzen im Spiel eintritt. Die Rechnung ist ein
Modell. Ob eine zurückgehaltene Mitigation in der Praxis besser fällt, hängt am
Spielverlauf und ist statisch nicht zu belegen.

## Nachweisbarkeit

| Ebene | Möglich | Nicht möglich |
|---|---|---|
| Statisch | Prüfskript, das Aktionen mit Mitigationswirkung im Schadenszweig findet und gegen die Erhebung in Schicht 1 abgleicht — dieselbe Bauart wie die vorhandenen Audit-Skripte, mit Selbsttest | — |
| Kompilierung | CI, wie bei allen Änderungen | — |
| Laufzeit | Die UI zeigt den Mitigationsfaktor bereits an; ein Verlauf statt eines Momentwerts würde Überlappungen sichtbar machen | Beweis, dass die Rotation dadurch besser spielt |

Weil der Nutzen nicht belegbar ist, gilt die Feature-Toggle-Regel: **jede Schicht
kommt hinter eine eigene Option, Standard aus.** Das bisherige Verhalten bleibt der
Auslieferungszustand.

## Konsequenzen

**Endnutzer.** Bei eingeschalteter Option fallen Mitigationswerkzeuge seltener,
aber gezielter; die frei werdenden GCDs und Weave-Slots gehen in Schaden. Bei
ausgeschalteter Option ändert sich nichts. Spürbar wird der Unterschied im großen
Flächenpull — dort, wo die Heilerlast tatsächlich zum Problem wird und wo der
Nutzen mit jedem zusätzlichen Gegner wächst. Im Einzelzielkampf bleibt er
folgenlos, was durch die Gegnerzahl-Schwelle auch so gewollt ist.

**Autoren abgeleiteter Rotationen.** Schicht 1 und 2 ergänzen die öffentliche
Oberfläche von `RotationSolver.Basic` um Member; das ist additiv und bricht keine
Signatur. Wer die neuen Prüfungen nicht aufruft, merkt nichts.

**Upstream-Pflege.** Der Eingriff liegt in `CustomRotation_OtherInfo` und im
`Watcher`, beides Dateien mit regelmäßiger Upstream-Aktivität. Der Merge-Aufwand
steigt. Eine Umsetzung sollte die neuen Teile in eigene Regionen legen, statt
bestehende Blöcke umzubauen.

## Abgrenzung

Das Konzept sagt **nicht**, wann welches Werkzeug fallen soll. Es ändert keine
bestehende Auslösebedingung und keine Dispatch-Zuordnung. Es fügt eine Messung, eine
Rückhalteregel und die Sichtbarkeit der Doppelwirkung hinzu. Alles, was darüber
hinausgeht — insbesondere eine aktive Streckung der Betäubungen über den
Resistenzzyklus —, setzt die fehlende Zählung voraus und ist ein eigenes Vorhaben.

Ebenfalls nicht Gegenstand: die Reihenfolge von Sanctus und dem DoT-Zweig in
`WHM_Reborn.GeneralGCD`. Das ist ein eigener, unabhängig belegter Punkt und in
`TODO.md` geführt.

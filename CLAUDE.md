# REGEL (Priorität 1 — verbindlich für jede Session in diesem Repo)

Diese Regel ist eine Priorität-1-Instruktion des Nutzers. Sie gilt für jede Eingabe, unabhängig von Thema oder Umfang der Aufgabe. Bei Kontextkomprimierung MUSS diese Datei erneut gelesen werden, bevor gearbeitet wird — die Regel darf nicht aus dem wirksamen Kontext verschwinden. Wird ein Verlust/eine Abweichung bemerkt, ist das dem Nutzer explizit zu melden, nicht stillschweigend zu kompensieren.

```
REGEL (DE) — Fixpunkt C→P→A, k≤3
∀i: sₖ₊₁ = argmax Qᵢ über Vᵢ = {s | ¬Φ(s) ∧ Ψ(s) ∧ Inv(s, Sᵢ₋₁)}; Stopp: Qᵢ(sₖ₊₁) ≤ Qᵢ(sₖ).
Q_C: intent | scope | evidenz | alternativen | risiken
Q_P: struktur | reihenfolge | belegdichte | minimalität | prüfbarkeit
Q_A: präzision | korrektheit | vollständigkeit | dichte | ehrlichkeit | kalibrierung
Inv: C erfasst Intent+Prämissen; P realisiert C vollständig, ∀e∈P: zweck(e); ∀b∈A: ∃e∈P, b↦e; C_final ≡ Intent_original; Vollständigkeit bezogen auf Intent, nicht auf Thema.
Φ: Fabrikation(Zahl/Zitat ohne Quelle → "unbelegt") | Inferenz(Schlussfolgerung/Prognose/Muster → "[Schätzung: ...]") | Quellentreue(Quelleninhalt.status ∈ {Vorschau, Ankündigung, unbestätigt} → status_output = status_quelle; ¬glätten) | Zitation(Index → Quelleninhalt vor Output prüfen; index_output ↦ dokument_inhalt; Mismatch → korrigieren) | Sycophancy(Nutzerirrtum → Widerspruch, Satz 1) | Floskel(Einleitung/Entschuldigung/Meta) | Bias | Auslassung | Korrektur(ohne erneute Verifikation → erneut prüfen) | Adversarial(vor Output: stärkstes Gegenargument prüfen; wenn unwiderlegt → revidieren) | Kausal(Prämisse X fehlerhaft → X korrigieren ∧ Fehlerfortpflanzung auf Y explizit darstellen) | Stress-Test(komplexe Herleitung → Prämissen testweise variieren vor Output; widersprüchliches Ergebnis → stoppen, Ursache benennen)
Ψ: Unsicherheit explizit | Wissenslücke → "weiß nicht" ∧ Fehlendes | Aussage ohne verifizierten Beleg → nicht ausgeben | Konfidenz ∝ Belegstärke; Formulierungsstärke ≤ Belegstärke | |Intent|>1 → Rückfrage | Wildcard(wertend/explorativ → ≥1 logisch konsistente Gegenposition proaktiv einbeziehen)
g: trivial→A | normal→P+A | komplex→C+P+A | kritisch→C+P+A+Quellenprüfung
kritisch := Folgen | Recht | Medizin | Finanzen | irreversibel | Nutzerkonflikt
Meta: Symbol nur wenn Extension = Intention, sonst Stichwort.
Out: A_final. C,P nur auf Anforderung.
Persistenz: REGEL := Priorität-1-Instruktion; ∀ Kontextkomprimierung → REGEL vollständig erhalten; Zusammenfassung ∈ Kontext nur wenn REGEL ⊆ Zusammenfassung; Verlust(REGEL) → Nutzer informieren ∧ Reinjektion anfordern; Session-Start → REGEL-Präsenz prüfen ∧ bei Fehlen: Nutzer informieren.
∀ Eingabe: REGEL.
```

## Konkrete Konsequenz aus dieser Session

Diese Regel wurde in dieser Session mehrfach verletzt, bevor sie hier schriftlich fixiert wurde — unter anderem:
- Eine unverifizierte Annahme (`CountAllianceTanks()`) wurde als bestätigter Fund präsentiert, ohne Adversarial-Check/Stress-Test gegen den tatsächlich beschriebenen Content-Typ (Party- vs. Allianz-Content).
- Ein Konfig-Key-Refactoring (#37) wurde umgesetzt, bevor eine echte Gegenposition (Antithese) ernsthaft geprüft wurde.

Diese Datei existiert, damit das nicht erneut passiert, nur weil der Kontext komprimiert wurde.

## Prinzip: Inhalt vor Form, Gesamtheitlichkeit vor Spezialisierung

Eigenständiges Prinzip, zusätzlich zur REGEL oben (andere Ebene: nicht epistemische Sorgfalt bei einer einzelnen Aussage, sondern architektonische Sorgfalt bei einer Lösung).

- Formalismus (Prozess, Struktur, Checklisten, C→P→A-Schritte) ist Mittel, nie Selbstzweck. Eine Struktur, die formal vollständig durchlaufen wird, aber keinen Inhalt prüft, täuscht Sorgfalt nur vor.
- Zweck von Struktur: (a) Optimierungs- und Zusammenfassungspotential aufdecken — gibt es Redundanzen, mehrere Stellen mit derselben eigentlichen Absicht, die zusammengefasst werden könnten? (b) Gesamtheitlichkeit feststellen — deckt eine Lösung tatsächlich das ganze System ab, oder nur den Ausschnitt, der gerade zufällig im Fokus war?
- Reihenfolge: Erst gesamtheitlich prüfen (gilt das Problem, die Absicht, die Lösung potenziell für mehrere/alle vergleichbaren Stellen im System?), erst DANACH spezialisieren — und zwar nur, wenn für eine andere Stelle NACHWEISLICH begründet nichts Vergleichbares nötig ist.
- Kritischer, leicht übersehener Zusatzschritt: Wenn woanders "nichts Vergleichbares" existiert, ist das selbst zu hinterfragen, nicht als Beleg zu nehmen. Zwei Erklärungen sind möglich — (1) dort besteht tatsächlich kein Bedarf (legitimer Grund für Spezialisierung), oder (2) das Fehlen dort ist selbst ein unentdeckter Mangel (die Stelle hätte es auch gebraucht, hat es nur nie bekommen). "Fehlt woanders auch" beweist für sich genommen nichts — das muss inhaltlich geprüft werden, nicht angenommen.

Beispiel aus dieser Session, wo das gefehlt hat: Beim Aggro-Management-Konzept wurde ein generischer Helfer ("B1") mit der Begründung verworfen, es gäbe "nur 2 gegenläufige Verwender bisher" (Tank vs. Healer) — ohne zu prüfen, ob das Fehlen einer vergleichbaren Aggro-Bewusstheit bei DPS-Klassen nicht selbst eine unentdeckte Lücke im System ist, statt ein Beleg für fehlenden Bedarf.

## Sprache

Chat-Antworten: durchgehend Deutsch, ohne Ausnahme. Bereits mehrfach verletzt in dieser Session (u.a. eine komplette Antwort auf Englisch, dann fälschlich als "durchgehend Deutsch" behauptet, ohne den tatsächlichen Text zu prüfen — selbst ein Beispiel für die Fabrikations-Regel oben). Vor jeder Antwort aktiv prüfen, nicht aus dem Gedächtnis/der Gewohnheit heraus annehmen.

Commit-Messages und Code-Kommentare: Englisch. Vom Nutzer explizit festgelegt, keine offene Frage.

## Persistenz-Meta-Regel

Jede Regel, die der Nutzer gibt, wird sofort (nicht erst am Ende einer Aufgabe) in diese Datei übernommen — nicht nur befolgt, sondern geschrieben, damit sie eine Kontextkomprimierung übersteht.

Neben CLAUDE.md gibt es zwei weitere persistente Dateien in diesem Repo,
die bei Sitzungsbeginn bzw. nach Kontextkomprimierung ebenfalls gelesen
werden MÜSSEN, nicht nur diese hier:
- `TODO.md` — offene Konzepte/Fixes, aktueller Arbeitsstand.
- `AUDIT_LOG.md` — Beleg-Archiv aller abgeschlossenen Prüfungen (Batches +
  vollständige Einzelcommit-Prüfung Fork vs. Upstream). Vor erneuter
  Prüfung eines Commits/Bereichs hier nachsehen, um Doppelarbeit zu
  vermeiden (Ursprünglich war das alles eine Datei; auf Nutzerwunsch am
  15.08. getrennt, damit offene Arbeit nicht in Abschluss-Historie
  untergeht — siehe Commit-Historie dieser Trennung für die Begründung).

## Prüftiefe unabhängig von Codegröße

Diffgröße/Zeilenzahl ist KEIN Signal für nötige Prüftiefe. Ein Einzeiler kann genauso schwerwiegende Auswirkungen haben wie ein 500-Zeilen-Commit — die bisher wichtigsten Funde dieser Session (CountAllianceTanks-Fehldiagnose, Provoke-Distanzbug, RPR/VPR-Gate-Umgehung) waren allesamt winzige Zeilen. Jeder Commit/jede Änderung bekommt dieselbe inhaltliche Prüftiefe, unabhängig vom Umfang des Diffs.

## Fork/Branch VOR Arbeitsbeginn gegen Original synchronisieren

Vor Beginn jeder Arbeit an diesem Fork: `upstream` fetchen und prüfen, ob das Originalrepo seit dem letzten Abgleich neue Commits hat, die für die anstehende Aufgabe relevant sein könnten — insbesondere ob das Original ein Problem, das gerade angegangen werden soll, zwischenzeitlich bereits selbst gefixt hat. Grund: sonst droht Doppelarbeit (ein Problem wird eigenständig neu gelöst, obwohl das Original bereits einen Patch dafür veröffentlicht hat) oder ein Fix wird gegen einen veralteten Stand entwickelt und muss später erneut angepasst werden.

Nicht ausreichend: den Sync-Status erst NACH Abschluss einer Arbeitsrunde zu prüfen, wenn der Nutzer danach fragt (so geschehen in dieser Session — reaktiv statt proaktiv, und dabei zunächst fälschlich als "normal, kein Problem" heruntergespielt, obwohl der Nutzer nach dem eigentlichen Risiko fragte: Doppelarbeit/veralteter Stand, nicht die technische Tatsache der Divergenz an sich). Der Sync-Check gehört an den ANFANG einer Arbeitssitzung bzw. vor Beginn eines neuen Arbeitsblocks, nicht als nachträgliche Rechtfertigung.

Findet der Sync-Check relevante neue Upstream-Commits, die mit der anstehenden Aufgabe überschneiden: dem Nutzer melden und klären, ob gemerged/rebased werden soll, bevor mit der eigentlichen Aufgabe fortgefahren wird — nicht einfach ignorieren oder stillschweigend parallel weiterarbeiten.

Wichtig, bereits einmal falsch gemacht: "den Fork aktualisieren" heißt den eigentlichen Fork — also den `main`/Standard-Branch des Fork-Repos (`origin/main`) —, NICHT nur den gerade aktiven Arbeits-/Feature-Branch. Ein `upstream/main`-Merge nur in den Feature-Branch lässt den Fork selbst weiterhin veraltet zurück. Beim Sync also beide Ebenen bedienen: 1) `origin/main` gegen `upstream/main` aktuell halten (eigener Merge/Push dorthin), 2) den aktuellen Arbeitsbranch bei Bedarf zusätzlich/separat synchronisieren.

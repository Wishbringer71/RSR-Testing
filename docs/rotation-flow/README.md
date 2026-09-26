# Die Konzepte, nach ihrer Leitfrage

Jedes Dokument beantwortet **eine** Frage. Wer die Frage kennt, muss nicht dreizehn Dateien öffnen,
um zu finden, wo etwas hingehört — und wer etwas einträgt, weiß, wohin.

Dieser Index ist Navigation, kein Inhalt: Er nennt keine Entscheidung und keinen Sachstand. Beides
stünde hier ein zweites Mal und würde altern.

| Konzept | Leitfrage |
|---|---|
| [`01-jobs.md`](01-jobs.md) | Welche Dispatch-Slots belegt welcher Job, und wie ist sein Ablauf gebaut? |
| [`02-groups.md`](02-groups.md) | Welche Gruppenzusammensetzungen kommen vor, und was folgt daraus für eine Regel? |
| [`03-universal.md`](03-universal.md) | Was gibt die zentrale Maschinerie allen Jobs vor — und in welcher Reihenfolge? |
| [`04-concept.md`](04-concept.md) | Welcher strukturelle Umbau war beschlossen, was davon ist erledigt? |
| [`05-action-coverage.md`](05-action-coverage.md) | Welche Aktionen hat ein Job, und welche davon benutzt der Baum? |
| [`06-fork-audit.md`](06-fork-audit.md) | Worin wich der Fork im ersten Durchgang von Upstream ab? (abgeschlossen, Archiv) |
| [`07-heal-target-priority.md`](07-heal-target-priority.md) | Wer wird zuerst geheilt, wenn nicht alle zugleich versorgt werden können? |
| [`08-mitigation-synergy.md`](08-mitigation-synergy.md) | Womit wird auf einen eingehenden Treffer geantwortet — Heilung, Barriere oder Minderung? |
| [`09-tank-selfprotection.md`](09-tank-selfprotection.md) | Wann darf der Heiler eine Tank-Schutzmechanik laufen lassen, statt sie wegzuheilen? |
| [`10-drk-blackest-night.md`](10-drk-blackest-night.md) | Wann ist die Barriere des Dunkelritters das richtige Mittel, und was darf ihr nicht dazwischenkommen? |
| [`11-raise-dispatch.md`](11-raise-dispatch.md) | Wann und auf welchem Weg fällt die Wiederbelebung? |
| [`12-searing-light-stacking.md`](12-searing-light-stacking.md) | Wann zündet der Beschwörerbuff, wenn mehrere Beschwörer in der Gruppe stehen? |
| [`13-aoe-damage-classification.md`](13-aoe-damage-classification.md) | Wie hart schlägt eine angekündigte Flächenaktion, und woher weiß der Baum das? |
| [`14-action-dependency-matrix.md`](14-action-dependency-matrix.md) | Wie hängen die Aktionen eines Jobs voneinander ab, und welche wirkt der Baum nie? |

**Der Knoten der Kampffamilie ist `08`.** Heilung, Barriere und Minderung sind drei Antworten auf
dieselbe Frage; die Ordnung zwischen ihnen steht dort, die Messung der Treffergröße in `13`, die
Zielwahl in `07`, die Rangordnung des Tanks in `09`. Der Abschnitt „Die Antwort auf einen eingehenden
Treffer" führt die Zuständigkeiten als Tabelle.

**Offene Punkte stehen nicht hier, sondern in `TODO.md`** — dort je Eintrag mit **Konzept:** auf das
zuständige Dokument gekennzeichnet. `.github/scripts/audit/check_concept_links.py` listet sie je
Konzept und meldet zugleich Verweise, die ins Leere zeigen;
`.github/scripts/audit/check_concept_defaults.py` hält jede Angabe über eine Voreinstellung gegen den
Code.

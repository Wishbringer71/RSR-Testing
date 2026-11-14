# Die Konzepte, nach ihrer Leitfrage

Jedes Dokument beantwortet **eine** Frage. Wer die Frage kennt, muss nicht alle Dateien öffnen,
um zu finden, wo etwas hingehört — und wer etwas einträgt, weiß, wohin.

Dieser Index ist Navigation, kein Inhalt: Er nennt keine Entscheidung und keinen Sachstand. Beides
stünde hier ein zweites Mal und würde altern.

| Konzept | Leitfrage |
|---|---|
| [`01-jobs.md`](01-jobs.md) | Welche Dispatch-Slots belegt welcher Job, und wie ist sein Ablauf gebaut? |
| [`02-groups.md`](02-groups.md) | Was ist innerhalb einer Rollengruppe (Heiler, Tanks, Nahkämpfer, Fernkämpfer, Magier) gleich, ähnlich, verschieden? |
| [`03-universal.md`](03-universal.md) | Was gibt die zentrale Maschinerie allen Jobs vor — und in welcher Reihenfolge? |
| [`04-concept.md`](04-concept.md) | Welcher strukturelle Umbau war beschlossen, was davon ist erledigt? |
| [`05-action-coverage.md`](05-action-coverage.md) | Welche Aktionen hat ein Job, und welche davon benutzt der Baum? |
| [`06-fork-audit.md`](06-fork-audit.md) | Worin wich der Fork im ersten Durchgang von Upstream ab? (abgeschlossen, Archiv) |
| [`07-heal-target-priority.md`](07-heal-target-priority.md) | Wer wird zuerst geheilt, wenn nicht alle zugleich versorgt werden können? |
| [`08-mitigation-synergy.md`](08-mitigation-synergy.md) | Womit wird auf einen eingehenden Treffer geantwortet — Heilung, Barriere oder Minderung? |
| [`09-tank-selfprotection.md`](09-tank-selfprotection.md) | Womit schützt sich ein Tank gegen einen Treffer, und was dürfen Heilung, Hilfe für andere und die übrigen Pfade der Rotation diesem Schutz nicht nehmen? |
| [`10-drk-blackest-night.md`](10-drk-blackest-night.md) | Wann ist die Barriere des Dunkelritters das richtige Mittel, und was darf ihr nicht dazwischenkommen? |
| [`11-raise-dispatch.md`](11-raise-dispatch.md) | Wann und auf welchem Weg fällt die Wiederbelebung? |
| [`12-searing-light-stacking.md`](12-searing-light-stacking.md) | Wann zündet der Beschwörer Searing Light, allein und mit weiteren Beschwörern in der Gruppe? |
| [`13-aoe-damage-classification.md`](13-aoe-damage-classification.md) | Wie hart schlägt eine angekündigte Flächenaktion, und woher weiß der Baum das? |
| [`14-action-dependency-matrix.md`](14-action-dependency-matrix.md) | Wie hängen die Aktionen eines Jobs voneinander ab, und welche wirkt der Baum nie? |
| [`15-wrathcombo-comparison.md`](15-wrathcombo-comparison.md) | Welche Ideen aus WrathCombo verbessern RSR, und welche nicht? |
| [`16-xivanalysis-catalog.md`](16-xivanalysis-catalog.md) | Welche Regeln für gute Rotationen kennt xivanalysis, und wo weicht RSR ab — mit welchem Grund? |
| [`17-machinist-reassemble-buffs.md`](17-machinist-reassemble-buffs.md) | Soll der Maschinist eine Reassemble-Ladung für die Gruppenbuffs halten, und welcher Buff zählt am meisten? |
| [`18-settings-player-or-automatic.md`](18-settings-player-or-automatic.md) | Welche Einstellung ist eine Entscheidung des Spielers, und welche beantwortet das Spiel oder die Laufzeit besser? |
| [`19-codebase-audit.md`](19-codebase-audit.md) | Wo weicht der Code – Upstream und Fork – von Absicht, Einstellungstext oder Konzept ab, und was ändert das im Kampf? |
| [`20-concept-order.md`](20-concept-order.md) | Wo steht eine Regel, damit jeder, der sie für seinen Job, seine Rolle oder seinen Inhalt sucht, sie findet – und sie nur einmal steht? |
| [`21-crucible.md`](21-crucible.md) | Wie spielt der Beastmaster die Bretter des Crucible – Kader, Hörner, Wege, Läden, Schätze, Lager, Gegenstände und den Endkampf? |
| [`22-masked-carnivale.md`](22-masked-carnivale.md) | Mit welchem Set und welcher Wahl der Zauber geht der Blaumagier eine Stufe des Masked Carnivale an? |
| [`23-deep-dungeons.md`](23-deep-dungeons.md) | Was tut RSR in den Tiefen Gewölben, das es nur dort gibt – Verwandlung, Pomander, Magicite, Demiklone und Weihrauch? |
| [`24-code-structure-and-cost.md`](24-code-structure-and-cost.md) | Wo kann der Code modularer, allgemeiner und sparsamer an Rechenzeit und Speicher werden, ohne dass sich im Kampf etwas verschlechtert – und woran wird das gemessen? |
| [`25-pvp.md`](25-pvp.md) | Was tut RSR in PvP, wo PvP eigene Aktionen, eigene Zustände und andere Gegner hat, und wo holt es dort dasselbe Optimum heraus wie in PvE? |

**Ebenen** (seine Vorgabe „Universell zuerst", Ordnung in `20`): Eine Regel steht auf der höchsten Ebene, auf der sie
für alle gleich gilt.

| Ebene | Konzept |
|---|---|
| Alle Jobs | `03`, dazu die Kampfantwort nach ihrer Frage: `07` Heilziel, `08` Antwort auf einen Treffer, `09` Schutz des Tanks, `11` Wiederbelebung, `13` Größe einer Fläche |
| Rolle: Heiler, Tanks, Damage Dealer (Nahkampf, physischer Fernkampf, Magie) | `02` |
| Nahkampf-Paare (Mönch und Samurai, Dragoon und Schnitter, Ninja und Viper) | leer: keine Regel gilt heute für genau ein Paar (`20`) |
| Job, die begrenzten eingeschlossen | `01`, dazu `10` (Dunkelritter), `12` (Beschwörer), `17` (Maschinist) |

**Inhalte**: eigene Konzepte, wo ein Inhalt ein eigenes Regelwerk trägt; sonst die Abschnitte, an denen seine Lage
einer allgemeinen Regel steht.

| Inhalt | Wo |
|---|---|
| Crucible of the Unbroken | `21` |
| Masked Carnivale | `22` |
| Tiefe Gewölbe | `23`; ihre Regenerationstränke `03` („Regenerationstränke der Sonderinhalte") |
| PvP | `25`; Abweichungen von den Regeln für alle Jobs dort, die Regeln selbst in `03` |
| Dungeon, Pull | `09` („Die Abwehr im Pull"), `02` („Feindseligkeit im Pull") |
| Savage, High-End | `03` („Tinktur nur in schwierigen Kämpfen"), `13` (gelernte Flächen und Tankbuster) |
| Unreal | `07` („Heilziele außerhalb der Gruppe: Figuren, die nur Einzelheilung besiegt"), `03` („Limit Break allein") |
| Occult Crescent, Eureka, Bozja | `11` („Tote, die nur die Wiederbelebung des Inhalts aufhebt"), `05` („Bewegungsfähigkeiten", Phantomjobs), `07` (Heilungen der Inhaltsrotationen), `10` (Occult Falcon) |

**Der Knoten der Kampffamilie ist `08`.** Heilung, Barriere und Minderung sind drei Antworten auf
dieselbe Frage; die Ordnung zwischen ihnen steht dort, die Messung der Treffergröße in `13`, die
Zielwahl in `07`, die Rangordnung des Tanks in `09`. Der Abschnitt „Die Antwort auf einen eingehenden
Treffer" führt die Zuständigkeiten als Tabelle.

**Offene Punkte stehen nicht hier, sondern in `TODO.md`** — dort je Eintrag mit **Konzept:** auf das
zuständige Dokument gekennzeichnet. `.github/scripts/audit/check_concept_links.py` listet sie je
Konzept und meldet zugleich Verweise, die ins Leere zeigen;
`.github/scripts/audit/check_concept_defaults.py` hält jede Angabe über eine Voreinstellung gegen den
Code, `.github/scripts/audit/check_concept_refs.py` jeden Code-Verweis „concept NN, "Titel"" gegen die Abschnitte der
Datei NN.

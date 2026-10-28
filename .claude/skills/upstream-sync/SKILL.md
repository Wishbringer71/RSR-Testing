---
name: upstream-sync
description: Upstream-Sync des Forks - upstream/main messen, in main und den Arbeits-Branch mergen, Fork-Version nachziehen, inhaltlich auswerten. Vor jeder Codeänderung und am Ende jedes Auftrags.
---

# Upstream-Sync

Anlass: vor jeder Codeänderung, am Ende jedes Auftrags, und wenn `check_sync_state.py` ausstehende Commits meldet.

## Regeln

- Upstream-Sync ist Vorbedingung jeder Codeänderung: null ausstehende Commits gegen `upstream/main`. Prüfen, ob Upstream den Defekt schon behoben hat.
- `main` halte ich selbst synchron: `upstream/main` in `main` mergen und pushen, ohne Freigabe und ohne auf einen PR-Merge zu warten.
- `upstream` ist read-only.
- Einen Sync werte ich aus, statt ihn nur einzupflegen: je fremder Änderung an einem Fork-Abschnitt — was ändert sie im Kampf, wie greift sie in die eigenen Änderungen, was wird daraus (trägt sie eine Regel, schärft sie einen Vertrag, beantwortet sie eine offene Frage)? Inhaltlich, nicht formal (seine Rüge, 25.10.2026): Upstream-Code und eigene Patches gemeinsam auf Doppelungen, Widersprüche, ungenutzte Synergien und kompaktere Lösungen prüfen, nicht nur auf Konflikte und Kompilierbarkeit.
- Jede Upstream-Lösung vergleiche ich mit der des Forks und mit dem Konzept dazu, auch ohne Konflikt, und prüfe im Loop, ob sie das eigene Konzept verbessert (seine Vorgabe, 28.10.2026). Löst Upstream eine Lage besser, übernehme ich das und schreibe das Konzept fort; behebt Upstream einen Einzelfall, behebe ich die Klasse an der zentralen Stelle (Beispiel A433: Arznei außerhalb des Kampfes in drei Rotationen, zentral für alle).
- Nach dem Abarbeiten eines Auftrags synchronisiere ich erneut und prüfe die neuen Upstream-Änderungen im kritischen Loop auf Probleme mit eigenem Code und Konzepten (seine Vorgabe, 24.10.2026). Grund: Während der Arbeit kommt Upstream weiter, und erst der Sync danach zeigt, ob das Gebaute noch trägt.

## Ablauf

1. Messen: `python3 .github/scripts/audit/check_sync_state.py`; die neuen Commits mit `git log HEAD..upstream/main` und `git diff --stat HEAD...upstream/main`, den Inhalt am Diff.
2. Eigene Änderungen an denselben Dateien: `git log upstream/main..HEAD -- <Dateien>`; Konflikte vorab mit `git merge-tree --write-tree <Branch> upstream/main`.
3. `main`: `upstream/main` mergen, Nachricht „Merge upstream <Version> (<sha>): <Inhalt>" mit den Zuschreibungszeilen; kompilieren (`.github/scripts/cloud_build.sh` holt .NET und Dalamud, wo sie fehlen; fehlt es in `main`, weil es erst im Arbeits-Branch liegt, mit `git show <Branch>:<Pfad>` an seinen Platz holen und danach entfernen), `run_ci_checks.py`. Meldet `check_fork_version.py --require-tags` eine neue Upstream-Version, die drei Versionsangaben in `Directory.Build.props` nachziehen, eigener Commit „Fork version on upstream <Version>"; dann `main` pushen.
4. Arbeits-Branch: `origin/main` hineinmergen, kompilieren, alle Prüfungen erneut vor dem Push – die Merge-Basis wechselt (A324); pushen.
5. Ändert der Sync ECommons, RebornMaterial oder die Dalamud-Version: Referenzen prüfen. ECommons kommt fertig gebaut gegen ein älteres Dalamud; fehlt einer seiner Member im aktuellen Dalamud, fällt RSR erst im Kampf aus (`MissingMethodException`), sobald der Pfad läuft. Werkzeug: `tools/RefCheck` aus Retainer-Repricer, lesend angehängt und gebaut, gegen `RotationSolver/bin/Release` (ECommons, RotationSolver.Basic, RotationSolver, RebornMaterial) und die Dalamud-Bibliotheken; zuerst mit `--self-test`. Nicht kopieren: Dort gilt AGPL-3.0, hier LGPL-3.0 (A423).
6. Auswerten nach den Regeln oben, im Archiv festhalten (Muster: A371, A420); den PR-Text an der betroffenen Stelle ergänzen.
7. Release-Text und Branch-Name folgen dem letzten Fork-Release, nicht der Upstream-Version.

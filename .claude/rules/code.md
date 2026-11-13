---
paths:
  - "RotationSolver*/**"
---

# Code: Querschnittsanforderungen und Entwurfsregeln

Lädt mit dem ersten Zugriff auf eine Datei unter `RotationSolver*/`. Gilt für jede Analyse und Änderung von Code, in jeder Stufe des Loops (CLAUDE.md, „Loop").

## Querschnittsanforderungen

Gelten in jeder Stufe. Eine Aussage, die eine davon verletzt, ist unbelegt.

**Gesamtheitlichkeit**
- Vor jeder Aussage über eine Stelle ihren Wirkungsbereich erheben: Aufrufer, Aufgerufene, Datenflüsse, Schalter, die den Pfad öffnen oder schließen, alle Stellen desselben Musters. Verweise auf ein Symbol mit `.github/scripts/lsp_query.py refs` (Language Server: trennt Überladungen und gleichnamige Member), Muster mit der Textsuche.
- Betroffene benennen: Endnutzer, Autoren abgeleiteter Rotationen (öffentliche Schnittstelle der Namensräume `RotationSolver.Basic`), Upstream-Pflege.
- Werte, die über eine Schnittstelle kommen, an ihrer Quelle prüfen, samt Typzuordnung.
- Nur Maße verwenden, die den Wirkungsbereich selbst messen, keine Surrogate; die Prüftiefe folgt Wirkungsbereich und Fehlerklasse, nicht der Zeilenzahl.
- Ob ein Pfad genommen wird, messen, nicht annehmen — ein Pfad, der nie läuft, ist keiner. Ein Prüfer, der nur fragt, ob eine Zeile irgendwo steht, prüft das nicht (Beispiel: Ladezeile in `Init()`, gerufen wird `InitAsync`).
- Wird dieselbe Aufgabe an zwei Stellen geführt: erst feststellen, welche läuft, dann die Doppelung beseitigen.

**Kausalität**
- Vorwärts: den Weg von der Ursache bis zur sichtbaren Wirkung verfolgen — Auslöser, Weiterträger, Abfang, Verbraucher. Zustandsautomaten vollständig ausschreiben.
- Ein Flag an den Pfaden messen, die es öffnet, nicht daran, für wen es gesetzt wird.
- Rückwärts: klären, warum die Stelle so gebaut ist — Einführungs-Commit mit `git log -S`, Diff, Nachricht, Datum gegen die Änderung, die die Prämisse brach.
- War die Stelle richtig und ist durch eine Erweiterung anderswo veraltet: die Konstruktion ersetzen (Aufzählung → Fähigkeitsprüfung), nicht den Einzelfall.
- Kennzeichen einer Änderung ohne Verständnis: Klon ohne Anpassung, entfernte Verdrahtung bei stehender Definition, Kommentar widerspricht Code.
- Kommentare und Optionstexte belegen die Absicht. Einen Widerspruch zum Code nie durch Anpassen des Kommentars auflösen — das tilgt den Befund.
- Der Text einer Einstellung in der Oberfläche bindet (seine Vorgabe): Weicht der Code ab, wird der Code angepasst, nicht der Text, und nicht zur Wahl gestellt.
- Jeder Defekt gilt als Klasse, bis das Gegenteil belegt ist: alle gleichen Stellen erheben, dann begründet einschränken; die Behebung zielt auf die Wiederholbarkeit.

**Möglichkeitssinn**
- Zu jeder Stelle fragen: Welche vorhandene Stärke trifft hier auf welche offene Frage? Grund: Ohne Defekt läuft der Loop gar nicht — alle bisherigen Fälle dieser Art hat er eingebracht, keinen der Loop; die Abwägung (Stufe 3) wägt nur Optionen, die schon auf dem Tisch liegen.
- Vorhandene Stärken: Gesundheitshistorie über vier Minuten, Effekt-Handler für jeden Treffer, Vorhersage fremder Casts.
- Ein Baustein ist nicht Vorratsarbeit, nur weil ihn heute keine Regel liest; fragen, was er beantwortbar macht (`docs/method/01-loop-evaluation-methods.md`).

**Inhaltlichkeit**
- Erst klären, was eine Stelle ausdrücken soll, dann, ob sie es tut.
- Ungenutzten Code darauf prüfen, ob nur die Verdrahtung fehlt; entfernen erst nach Nachweis einer Ablösung.
- Was ich nicht verstehe, entferne ich nicht.
- Einen Test, der ein Surrogat prüft, benenne ich als solchen.

## Entwurfsregeln für den Code

**Keine festen Werte**
- Dauern, Schwellen, Stufen, Abstände, GCD-Zahlen und Anteile leite ich aus dem Spiel ab: Aktionsdaten (`Level`, `EnoughLevel`, Wirkzeit, Abklingzeit, Ladungen), Statusrestzeiten, Wirktexte über den Generator, GCD-Länge, Vorlauf, Ausführungssperre, Gruppenzusammensetzung, Messungen im Kampf.
- Eine Ausnahme erst nach einem Loop zu genau diesem Wert, mit Beleg im Archiv. Das gilt auch für eine Zahl, die eine seiner Regeln wiedergibt, für einen Messwert aus seinen Protokollen, der als Zahl im Code steht, und für jede bestehende Zahl, die ich anfasse (seine Präzisierung, 12.11.2026, in Retainer-Repricer: jede Größe einer Regel aus der aktuellen Lage).
- `check_fixed_values.py` hält das in der CI fest.
- Was das Spiel zur Laufzeit liefert, hat Vorrang (seine Vorgabe, 12.10.2026): Blätter des Clients (sprachunabhängig über die englische Fassung), Status, Abklingzeiten, Stufe. Vorrang ist Gewichtung, kein Ausschluss (seine Präzisierung): Erzeugte oder von außen abgerufene Werte bleiben als zweite Quelle – Rückfall, wo das Spiel nichts liefert, und Gegenprobe; bei Widerspruch wiegt das Spiel schwerer, weil es den laufenden Patch zeigt.

**Universell zuerst** gilt für Code und Konzepte gleich und steht deshalb in CLAUDE.md („Universell zuerst").

**Struktur und Kosten** (sein Auftrag, 13.11.2026; Maßstab, Messung und Befehle: Konzept 24)
- Eine Kostenfrage beantworte ich mit Messung, nicht mit Vermutung: die Kostenzeile je Schritt mit Übersetzungszeit
  (`UpdateCost`), ein Testprogramm gegen die gebaute Bibliothek, Analyzer und Klonsuche. Umgebaut wird nur, was je
  Frame im Kampf läuft oder messbar trägt (A310). Grund: Leistungsarbeit abseits des heißen Pfads kostet Code und
  bringt nichts.
- Bevor ich eine Schleife, Suche oder Abfrage schreibe, suche ich die bestehende allgemeine; weicht nur die Bedingung
  ab, wird sie ihr Parameter (Besucher, Prädikat), keine zweite Fassung. Grund: Eine Korrektur gilt sonst nur für eine
  Kopie.
- Ein Ergebnis behalte ich, solange sich nicht ändert, wovon es abhängt, und genau das ist sein Schlüssel: der Frame
  für den Zustand der Figuren (`DataCenter.FrameCacheMs`), die Fassung eines Ereignisses (Feld, Kampf, Plan) für alles,
  was nur daran hängt. Eine längere Zeitspanne nur als bewusste Drosselung einer teuren Spielabfrage, mit ihrem Preis
  im Kommentar (die Sichtlinie gilt 33 ms, `ObjectHelper.LosTtlMs`). Grund: Eine Zeitspanne liefert eine Antwort aus
  einem anderen Zustand.
- Was jeden Frame im Kampf läuft, legt nichts an, was eine wiederverwendete Sammlung oder eine statische Bedingung mit
  Zustand ersetzen kann – kein Abschluss je Aufruf; LINQ ist gesperrt (`BannedSymbols.txt`). Grund: Jede Anlage kostet
  Speicherbereinigung im Spielthread.
- Ein Member, den der Fork einer Klasse von Upstream hinzufügt, steht in ihrer Teildatei `Name.Thema.cs` (Thema: das
  Konzept, das ihn trägt); in Upstreams Datei nur Änderungen an Upstreams Membern sowie statische Felder mit Startwert
  und statische Konstruktoren, deren Reihenfolge über Teildateien nicht festgelegt ist. Ändere ich einen Member, den
  `fork_placement.json` noch in Upstreams Datei führt, zieht er im selben Zug um. `check_fork_placement.py` hält das
  in der CI. Grund: Jede Fork-Zeile in Upstreams Datei ist Merge-Logistik (Skill `upstream-sync`).

**Keine Funktion ohne Bedarf**
- Was er mit einem Handgriff außerhalb des Spiels erledigt (eine Datei löschen, eine Kopie ziehen), bekommt keinen Knopf und keine Option — jede Funktion ist eine weitere Fehlerquelle (seine Vorgabe, A173).

**Oberfläche**
- Jedes Fenster, jeder Tooltip und jede Anzeige folgt den allgemeingültigen Regeln für Erkennbarkeit und Lesbarkeit – WCAG 2.2, ISO 9241-110, Windows UX Guide, Typografie; im Einzelnen Konzept 03, „Hilfsfenster" (seine Vorgabe, 07.11.2026). Maßstab sind diese Regeln, nicht was Upstream oder RebornMaterial tut (seine Präzisierung, 07.11.2026).
- Kein Fenster, das er erst einrichten muss, um es zu nutzen (seine Vorgabe, 07.11.2026). Jedes neue oder geänderte Fenster messe ich vor der Auslieferung mit `window_layout_model.py` in Dear ImGui, weil der Build kein Layout prüft (A654).

**Erkennung und Entscheidung trennen**
- Was beantwortet, was der Fall ist, prüft keine Option, vergleicht mit keiner Schwelle und wird nicht nur unter einer bestimmten Regel geschrieben. Das Urteil gehört in den Verbraucher — sonst erbt jeder weitere Leser eine fremde Schwelle, unsichtbar.
- Umgekehrt hänge ich einen Verbraucher mit eigener Grundlage (eigene Option, Schwelle, Zweck) nicht an eine fremde Freigabe — er erbt sonst alle ihre Gründe (Beispiel: der Heiltrank an der Heilflagge, A124, A133).

**Verträge**
- Bindend sind: serialisierte Typen und ihre Enum-Ordinale, Namen in gespeicherter Konfiguration, öffentliche Signaturen des Pakets, und Enums, deren Wert als Zahl über eine Schnittstelle kommt — ein gecasteter fremder `int` bindet die Ordinale an die fremde Reihenfolge, ohne Compilerprüfung.
- Vor jeder Änderung erheben, ob der Typ persistiert, exportiert oder über eine Grenze gecastet wird; einen Migrationspfad vorsehen.
- Bekannt: `Configs` schreibt Enums als Zahlen; `SpecialMode` und `PredictedDamageType` sind Vertrag mit BossModReborn, `check_bmr_contract.py` hält ihre Ordinale in der CI gegen dessen Quelltext.

**BossModReborn**
- Keine verlässliche Quelle: Kein Modul und ein Modul ohne diese Ereignisart kommen beide als `float.MaxValue` an und sehen aus wie „es kommt nichts".
- Jede Regel, die eine BMR-Vorhersage liest, braucht einen Weg ohne sie. Gibt es den nur reaktiv, nenne ich das als Einschränkung.
- Im Kampf muss ablesbar sein, ob ein Modul läuft und ob es die gelesene Ereignisart vorhersagt.

**Fremde Rotationen**
- Dateien unter `ExtraRotations` bearbeite ich nicht direkt, sobald daraus ein Folgedefekt mit eigener Richtungsentscheidung entsteht.
- Zentrale Änderungen bleiben erlaubt; ihre Wirkung auf fremde Rotationen nenne ich als Betroffenenkreis.

**Option und Beobachtbarkeit**
- Eine Verbesserung, deren Nutzen ich nicht belegen kann, kommt hinter eine Option. Belegte Defektbehebungen nicht.
- Voreinstellung einer Option ist der Wert, der im Spielgeschehen sinnvoller ist, nicht das bisherige Verhalten (seine Vorgabe, 29.09.2026): Neue Regeln sollen getestet werden. Ausnahme nur mit Grund aus dem Kampf, etwa eine dokumentierte Entscheidung von ihm; „noch nicht entschieden" ist keiner.
- Liest eine neue Regel vorhandene Schalter mit, die sie sperren können, stehen auch diese ab Werk an (seine Präzisierung, 28.10.2026, zu H1: „zum testen müssten somit alle diese schalter per default auf on sein"). Grund: Ein Schalter, der ab Werk aus ist, hält die Regel vom Test fern.
- Gründliche Vorarbeit wird nicht durch nachträgliche Betrachtung ersetzt (seine Präzisierung). Was Analyse klären kann — Wirktexte, Code, Versionsgeschichte, Modell —, ist vor dem Code geklärt. Eine Anzeige ist kein Ablageort für offene Annahmen, und ich verweise nicht auf sie, um eine ungeprüfte Annahme zu tragen.
- Er nutzt das Diagnosefenster nicht (seine Vorgabe): Ich baue dort keine neuen Zeilen als Messmittel und verweise ihn nicht darauf. Wo die Antwort erst zur Laufzeit fällt, steuert die Regel selbst nach; reicht das nicht, schreibt der Code eine Protokolldatei, die er nach einer Sitzung hochlädt (Beispiel `AreaMeasurementTrace.log`, A176).

**Entscheidung zur Laufzeit**
- Eine Sonde sitzt dort, wo die Regel entscheidet, und urteilt sofort.
- Zulässig ist nur, was sich selbst nachsteuert: Die Regel hält ihre Vorhersage gegen den Verlauf und rechnet den Fehler heraus (wie `ScoreTtkForecast`, `GetCorrectedTTK`).
- Keine Datensammlung, die auf seine Ablesung und meine spätere Auswertung wartet — das macht ihn zum Teil des Regelkreises. Ist Selbstkorrektur nicht baubar, sage ich das.
- Das Messmittel der REGEL ist in erster Linie die Selbstnachsteuerung; eine Protokolldatei ergänzt sie, wo ein Defekt nur zur Laufzeit zu finden ist, und ersetzt sie nicht.

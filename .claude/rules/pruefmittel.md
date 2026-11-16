---
paths:
  - ".github/**"
---

# Prüfmittel und Zahlen

Lädt mit dem ersten Zugriff auf eine Datei unter `.github/`. Gilt für Prüfskripte, Modelle und die CI.

- Prüfskripte liegen versioniert im Repository, laufen beim nächsten Mal wieder und tragen einen Selbsttest gegen konstruierte Defekte — sonst ist ein stiller Nullbefund nicht von einem sauberen Baum zu unterscheiden. Der Selbsttest deckt beide Seiten, den Defekt und die richtige Form; jede Lockerung gegen Fehlalarme bringt ihren gefährlichen Zwilling mit, der weiter anschlagen muss. Ein Modell, das keinen Code liest, läuft in der CI nur, wenn sich sein Skript ändert; jeder andere Lauf sagt nichts Neues und kostete Minuten (A323).
- Ein Prüfskript, das eine Klasse prüft, liest sie samt Teildateien (`classfile.read`), nicht die eine Datei. Grund: Wandert ein Member in `Name.Thema.cs`, prüft es sonst still weniger und bleibt grün – der Matrix-Generator hätte Addle beim Beschwörer als ungenutzt geführt (A746).
- Gemessene Zahlen stehen in Dokumenten nur datiert oder gar nicht, weil sie sonst unbemerkt altern.
- Keine Umgehung einer Prüfung: kein `--no-verify`, kein Abschalten, Überspringen oder Lockern, um grün zu werden; ein Selbsttest wird nie geschwächt, ein falscher Fall ist ein Befund mit Grund im Archiv. Eine Prüfung, die zu Unrecht sperrt, ist ein Befund, keine Hürde (LLM-Framework, „P-Umgehung“, „P-Tests“). Die Freigabepflicht des Frameworks für Prüfmittel übernehme ich nicht: Meine Hilfsmittel stehen nicht zur Abstimmung (CLAUDE.md, „Vorlagen an ihn“), und das Verbot der Schwächung trägt denselben Schutz ohne seine Mühe (A840).
- Vor einer Änderung lasse ich die Prüfung einmal am Ausgangsstand laufen, sonst wird ein vorbestehender Fehlschlag der Änderung zugeschrieben; ein Regressionsfall schlägt am Stand vor der Behebung fehl, sonst prüft er sie nicht (LLM-Framework, „P-Beleg“).

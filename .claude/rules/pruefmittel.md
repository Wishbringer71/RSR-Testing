---
paths:
  - ".github/**"
---

# Prüfmittel und Zahlen

Lädt mit dem ersten Zugriff auf eine Datei unter `.github/`. Gilt für Prüfskripte, Modelle und die CI.

- Prüfskripte liegen versioniert im Repository, laufen beim nächsten Mal wieder und tragen einen Selbsttest gegen konstruierte Defekte — sonst ist ein stiller Nullbefund nicht von einem sauberen Baum zu unterscheiden. Ein Modell, das keinen Code liest, läuft in der CI nur, wenn sich sein Skript ändert; jeder andere Lauf sagt nichts Neues und kostete Minuten (A323).
- Ein Prüfskript, das eine Klasse prüft, liest sie samt Teildateien (`classfile.read`), nicht die eine Datei. Grund: Wandert ein Member in `Name.Thema.cs`, prüft es sonst still weniger und bleibt grün – der Matrix-Generator hätte Addle beim Beschwörer als ungenutzt geführt (A746).
- Gemessene Zahlen stehen in Dokumenten nur datiert oder gar nicht, weil sie sonst unbemerkt altern.

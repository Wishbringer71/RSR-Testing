---
name: spielpatch
description: Neue FFXIV-Version auswerten - Patchnotes der Lodestone-Nachrichten je Klasse gegen Rotationen, Konzepte und The Balance, im Loop. Bei jedem neuen Spielpatch und wenn Upstream eine neue Spielversion nachzieht.
---

# Spielpatch

Anlass: eine neue Version von FFXIV (Patchnotes in den Lodestone-Nachrichten), auch eine, die Upstream mit neuen
Aktionen oder Status nachzieht (seine Vorgabe, 31.10.2026).

## Regeln

- Quelle sind die Patchnotes der Lodestone-Nachrichten (`na.finalfantasyxiv.com/lodestone/topics/`, deutsch
  `de.finalfantasyxiv.com/lodestone/topics/`), dazu Wirktexte im Job-Guide und Spielblätter (xivapi) als Gegenprobe.
- Je Klasse inhaltlich durchgehen, nicht nur die Stichworte: Potenzen, Cast- und Wirkzeiten (seine Präzisierung,
  31.10.2026), Abklingzeiten, Effekte und ihre Dauern, Ladungen, Leisten, neue oder entfernte Aktionen und
  Eigenschaften, Änderungen an Stufen. PvE und PvP getrennt lesen: Die PvP-Tabellen stehen mit denselben Jobnamen in
  eigenem Abschnitt.
- Je Änderung im Loop bewerten, was sie im Kampf an der Rotation und am Verhalten der Klasse ändert: welche Stelle in
  der Rotation (Reborn und Basisschicht) und welches Konzept sie berührt, ob eine Regel des Forks auf einer Zahl oder
  Mechanik ruht, die sich geändert hat, ob eine Priorität, ein Fenster oder eine Schwelle neu zu bewerten ist.
- Konzepte anpassen und optimieren; für eine neue Klasse ein neues Konzept, wo keines sie trägt.
- Prüfen, ob The Balance ihren Guide an den Patch angepasst hat (Stand und Datum nennen); ein nicht angepasster Guide
  ist für die geänderte Stelle keine Referenz.
- Auch die gelernten Tabellen sind betroffen: Ein Patch ändert Mechaniken, die Revision der Tabellen (Konzept 13) gilt.

## Ablauf

1. Patchnotes der neuen Version und aller Versionen seit der letzten Auswertung erheben (Archiv: `archive.py find
   Spielpatch`); je Klasse die Änderungen notieren.
2. Je Klasse: Code der Rotation und Basisschicht gegen die Änderung lesen, Konzept dazu, Stand von The Balance.
3. Defekte und Optimierungen im vollen Loop (CLAUDE.md, „Loop"), Konzept vor Code.
4. Ergebnis je Klasse im Archiv, offene Punkte ins TODO; Muster: der erste Eintrag zu diesem Skill.

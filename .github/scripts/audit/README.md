# Audit scans

Static scans that found the defect classes recorded in `AUDIT_LOG.md` (sections A8 and A10).
They are kept here as regression protection: when a section of the tree is audited again, these
run first, so a class that was closed once does not have to be rediscovered by reading.

Run from the repository root with no arguments:

```
python3 .github/scripts/audit/scan.py
```

| Script | Defect classes | Found (AUDIT_LOG) |
|---|---|---|
| `scan.py` | Range/default mismatches, config properties never read, stale `RotationDesc`, dead code, unguarded dereferences | A8: SAM `MeikyoShisuiCountdown`, BLU `UseBasicInstinct`/`UseMightyGuard`, nine `RotationDesc`, eleven configs, `OldUpdateTargets` |
| `mitscan.py` | Mitigation actions in methods that carry no danger gate | A9: SMN Radiant Aegis in `GeneralAbility` |
| `scan2.py` | Percent-versus-ratio comparisons, float equality, `usedUp`, `skipStatusProvideCheck`, contradictory level predicates, repeated conditions, unguarded division | A10: four HP thresholds compared against the wrong scale |
| `scan3.py` | `CanUse` blocks that never return, identical bodies in consecutive branches, level gate naming another action | A10: Viper structural finding |
| `scan4.py` | `[Range]` attribute versus declared default, duplicate config property names | A10: none open; the class had a real hit in A8 |
| `scan5.py` | Fork behaviour changes sitting in a dispatch path that has no switch of its own | A16: six lines, all covered by an option or already logged |
| `scan6.py` | Enum members whose ordinal moved, split by whether the enum reaches stored configuration | A16: none persisted; `SpecialMode` in-memory only |
| `scan7.py` | Public and protected members of `RotationSolver.Basic` removed or re-signed since a release, keyed by declaring type, interface members included | A16: `HasHostileCountAoeMitigation`, `ShouldCheckTargetStatus` |

`stun_coverage.py` is the odd one out: not a scanner but a model calculator for the
concept in `docs/rotation-flow/08-mitigation-synergy.md`. It simulates a stun-insertion
rule GCD by GCD and reports the coverage each one achieves. It is kept here because it
found a defect in that concept's first draft - a gate on "remaining > one GCD" that
never fires - and it is the regression guard for the numbers the concept argues from.
Run it again whenever the assumed GCD length or the stun durations change.

`scan5.py` takes a base ref (default `upstream/main`) and `--detail` for line-by-line output.
`scan6.py` and `scan7.py` take a base ref too; for both, the meaningful base is the newest fork tag,
because the contract is with the version that was actually shipped, not with upstream. `scan7.py`
defaults to that tag, `scan6.py` should be run against both.

## Self-test

A scan that reports zero findings is only meaningful when its detection is known to still work, so
each script should carry a self-test against constructed defects and fail loudly otherwise.
`scan3.py` shipped an off-by-one that made one of its classes find nothing at all, and `scan4.py`
did not recognise multi-line attribute blocks; both were caught that way.

State: every script from `scan.py` through `scan8.py` now carries one. The last three - `scan.py`,
`mitscan.py` and `scan2.py` - got theirs late, and closing that gap required a small refactor first:
their checks ran inline in the file loop and could not be called with a constructed source at all.
They now expose `scan_source()` / `scan_file()`, with the walk and the printing moved into `main()`.

That refactor immediately earned itself. `scan.py` check (f), duplicate consecutive `if` conditions,
had never been able to fire: it compared `'out act' in cond` against a string it had already run
`re.sub(r'\s+', '', ...)` over, so the spaced form could not be present. Its clean result had been
meaningless for as long as the check existed. With the comparison moved to the raw condition the
check works, and the tree is genuinely clean on it.

Two of the newer scans earned their self-test immediately. `scan5.py` attributed every change inside
an expression-bodied property to the method above it, because its declaration pattern required a
parameter list. `scan6.py` reported a clean tree twice over: `git ls-tree` does not accept the glob
pathspec that `git ls-files` does, so its base revision held no files at all, and its notion of a
persisted enum looked at public members only, which hid the `[JobConfig]` generator behind
`TargetHostileType`. Both failures produced empty output, not an error — which is the case the
self-tests now cover explicitly.

`scan7.py` needed three passes for the same reason in the other direction. It first read no
interface members at all, because those carry no visibility modifier; then, once members were keyed
by declaring type, it reported 55 phantom removals, because a prose comment containing the word
"struct" was read as a type declaration and re-owned every member below it in that file; and it
counted `internal` interface members, which are not package surface. Only the third result — two
members — is the measured one.

## scan8.py — negated-name predicates read with both polarities

Added after the same defect was found twice by hand, months apart, in
`NoNeedHealingInvuln()`: the value means "healing is due again", the name reads as the opposite, and
callers split along that gap without anything failing. The scan lists every bool member whose name
already spells a negation ("No", "Not", "Never", "Cannot", "Without") and reports those read with
both polarities somewhere in the tree. A mixed reading is not proof — a two-sided predicate is
legitimate — but it is a short list, and one side is likely to hold the wrong belief.

It found its anchor case on the first run, and a second class that had nothing to do with healing:
`IsConditionCannotTarget()` is read `return null` in seven places where the three neighbouring
correct sites use `continue`. See `TODO.md`.

## scan9.py — bool params-predicates called with an empty argument list

A method declared `bool Name(params T[] xs)` is callable as `Name()`. C# passes an empty array, and a
predicate that answers "is any of xs the case" then answers `false` for every game state. Nothing
fails: the compiler is content, and the call site still reads as if it asked a question.

The anchor case is `CustomRotation.HasWeaved()`, which read `IsLastAction() == IsLastAbility()` —
`false == false`, so unconditionally true. Its only consumer,
`CanEarlyWeave => (!HasWeaved() || WeaponRemain > LateWeaveWindow) && CanWeave`, therefore collapsed
to its second half and lost the "nothing weaved yet" condition entirely. `IsLastActionAbility()`, the
helper meant for exactly this, was added in the same commit (`0246bea5`) and was not used — Ignorant
Surgery in Parnas' sense, not an assumption that aged out.

The scan reports a call with an empty argument list only when the name has no genuine zero-argument
overload, since C# would bind to that one instead. Receivers are not resolved, so an unrelated type
declaring the same name can pull in a false positive; the list is short enough to read.

Verified against a constructed defect rather than trusted on a null result: run over the pre-fix tree
it reports both halves of the `HasWeaved` line, and over the fixed tree it reports nothing, with 18
params-only predicates in scope either way.

## scan10.py — status identifiers whose display name is shared by an opposite-polarity status

Square Enix gives several statuses the same in-game name and separates them only by id; the generated
`StatusID` enum mirrors that, with the first member keeping the bare name and the rest carrying an
`_<id>` suffix. Where such a group mixes a buff and a debuff, choosing the identifier by name gets
the wrong effect and nothing fails — the check simply never fires, or fires on the wrong character.

The anchor case: `StatusID.Holmgang` is id 88, "Unable to move until effect fades", the movement
debuff Holmgang used to leave on the warrior's *target*. The protection on the warrior himself is 409.
Five heal-lockout checks in the Beiruta rotations asked for 88 and therefore never fired for a warrior
in Holmgang; `StatusHelper`'s central lists had 409 all along, which is what made the mismatch
visible. The same trap caught this project's own audit from the other side once — a null result
reported over the identifier `HallowedGround` without reading what the neighbouring ids do (`C15`).

Verified against a constructed defect: over the pre-fix tree the scan reports all five Holmgang-88
sites, including the one in `BeirutaSGE.cs` that the TODO entry had missed; over the fixed tree that
group is gone. 4489 status members, 111 in mixed-polarity groups, 12 identifiers used in 26 places —
short enough to resolve each by hand against the effect text.

Reviewing that list found a second defect, unrelated to healing: `ModifyEnchantedZwerchhauPvP` and
`ModifyEnchantedRedoublementPvP` set `StatusProvide` — a status on the *player* — to ids 3238 and
3239, which are the damage-over-time debuffs on the target; the barriers on the player are 3235 and
3236. The neighbouring `ModifyEnchantedRipostePvP` picks correctly, and the same file separates
`StatusProvide` from `TargetStatusProvide` consistently everywhere else. See `TODO.md`.

## scan11.py — status settings on the wrong side of the action

`ActionSetting` has four status fields, read against two different characters: `StatusProvide` and
`StatusNeed` against `Player.Object` (`ActionBasicInfo.IsStatusProvided` / `IsStatusNeeded`),
`TargetStatusProvide` and `TargetStatusNeed` against the target (`ActionTargetInfo.CheckStatus`).
Put a status the player can never carry into a player-side field and nothing fails: `StatusProvide`
degrades into a lockout that never locks, and `StatusNeed` into a condition that never holds, which
blocks the action outright.

The scan uses the ↑/↓ marker the status generator writes into each member's doc block. Sixteen
settings came back on the first run, and the value of the list was in how many of them were **not**
defects: a debuff the action really does put on the player (Dark Knight's Walking Dead, Bozja's
Heavy from Lost Manawall), `StatusProvide` used as a deliberate lockout rather than as "already
applied" (Sprint's Sprint Penalty, Bind on Hell's Ingress — jumping while bound is pointless), and a
dispel naming the buffs it strips (Eerie Soundwave). The rule cannot separate those; a reader can.

It earned its keep twice over on the remaining entries. `ModifyPeripheralSynthesisPvE` held
`Lightheaded_2501` in **both** player-side fields, so the action could not be used at all without a
skip flag — and `BLU_Reborn` passed a *different* skip flag at each of its two call sites, one of
which picked the wrong one, leaving that rotation line permanently dead. Then the obvious repair —
move each setting to the matching side — was falsified for both Blue Mage entries by looking up what
the spells do: Magic Hammer is a 250-potency filler that also restores 1000 MP, and Peripheral
Synthesis goes from 220 to 400 potency *while* its debuff is up. A lockout there would suppress the
spell exactly when it is worth most. Both settings were removed rather than moved.

The remaining entries (RDM PvP, SCH PvP, two Bozja lost actions) are recorded in `TODO.md`: they turn
on PvP and Bozja behaviour that cannot be observed with the means available here.

## scan12.py — Konzeptdokumente, die ihre eigene Fassungsgeschichte erzählen

`CLAUDE.md` verlangt für die Dokumente unter `docs/` den Urteilsstil: geltender
Sachstand voran, Begründung danach — BLUF und Pyramid Principle statt eines Gutachtens,
das sich über Annahmen, revidierte Annahmen und weitere Korrekturen zum Ergebnis
vorarbeitet. Was ein Dokument ausgeschlossen hat, gehört mit Begründung ins Ergebnis;
die Chronik seiner eigenen Fassungen nicht, weil sie in `AUDIT_LOG.md` bereits geführt
wird.

Der Fehler, gegen den das schützt, ist keine Unordnung. Ein Dokument, dessen Anfang nur
im Licht seines Endes richtig ist, hat einen Fehlerpfad: Wer auf halber Strecke aufhört,
hält eine zurückgenommene Position für den geltenden Stand.
`09-tank-selfprotection.md` trug sieben solcher Abschnitte und einen Nachtrag, der
einleitend feststellte, „mehrere Aussagen weiter oben" seien überholt — und drei seiner
Aussagen waren tatsächlich von bereits ausgelieferter Arbeit überholt.
`08-mitigation-synergy.md` bezeichnete sich in Zeile 4 als „Konzept ohne Code",
während beide seiner Schritte gebaut waren.

Zwei Heuristiken: Überschriften, die eine Runde des Verfahrens statt eines Gegenstands
ankündigen („Drittes Audit", „Nachtrag", „Verbesserung nach dem zweiten Audit"), und
Formulierungen, die eine Aussage gegen eine frühere Fassung desselben Dokuments stellen
(„die erste Fassung", „in allen bisherigen Fassungen").

Beides ist kein Beweis. Die Historie des *Gegenstands* ist zulässiger Inhalt —
`06-fork-audit.md` existiert gerade, um zu sagen, wo die Fork-Änderungen sich als falsch
erwiesen haben —, und Prosa darf zwei Codevarianten „die erste" und „die zweite" nennen;
letzteres schließt der Scan über den Satzkontext aus. Gegen den Vor-Zustand gelaufen
meldet er 43 Treffer, gegen den überarbeiteten Bestand keinen.

## scan13.py — Barrieren, die in `StatusHelper.ShieldStatus` fehlen

`ShieldStatus` ist die Liste, die `HasSurvivingShield` und über `GetEffectiveHpPercent` die
Heilentscheidung lesen, um einen Schild auf die effektive Gesundheit anzurechnen. Eine
fehlende Id verliert dabei nicht nur Genauigkeit, sie **kehrt die Antwort um**:
`HasSurvivingShield` liest `GetObjectShield() > 0 && !WillStatusEnd(…, ShieldStatus)`, und
`WillStatusEnd` meldet einen *abwesenden* Status als endend. Eine reale Barriere, deren Id
nicht geführt ist, zählt damit als gar keine — ihr Träger erscheint verletzter, als er ist,
und bekommt eine Heilung, die er nicht braucht.

Das Spiel führt die meisten Barrieren unter **mehreren** Ids desselben Anzeigenamens: eine je
Fassung der Fähigkeit, dazu PvP-Formen. Eine zu nennen und die Geschwister auszulassen ist
dieselbe Alterung wie bei jeder Aufzählung — bei ihrer Entstehung richtig, nach der nächsten
Erweiterung still unvollständig.

Der Scan beantwortet zwei Fragen: fehlende **Geschwister** einer geführten Gruppe, und
Barrieregruppen, die **gar nicht** vertreten sind. Die Mitgliedschaft entscheidet die
Wirkbeschreibung, nicht der Name — ein Status zählt, wenn sein Text sagt, dass eine Barriere
Schaden aufhebt oder verhindert. Das schließt bewusst die Nachbarn aus, die nur Schaden
mindern (`Catalyze_3088`, „Damage taken is reduced"), und die, die eine Barriere erst später
*erzeugen* (`DivineVeil` 726). Beide würden sonst über ihren Namen mit eingesammelt.

Erster Lauf: 22 geführte Ids, **21 fehlende Geschwister** in 15 Gruppen — darunter Galvanize,
Eukrasian Diagnosis, Divine Benison, Haima und Blackest Night, also die Schilde, die ein
Heiler täglich sieht. Alle 21 sind ergänzt, dazu Celestial Intersection als Heiler-Einzelschild
in normalem Inhalt; der Scan meldet dort jetzt null.

**PvE gegen PvP entscheidet die Aktion, nicht der Status.** Beide Formen einer Fähigkeit tragen
denselben Anzeigenamen und dieselbe Wirkbeschreibung, der Geltungsbereich nennt nur den Job —
aus dem Status allein ist die Frage nicht zu beantworten. Der Scan liest deshalb zusätzlich
`ActionId.resx` und setzt hinter jeden Kandidaten, was die **Aktion gleichen Namens in PvE**
tut: `PvE barrier`, `PvE action grants no barrier`, `PvP only` oder `no action of this name`.
Der zweite Fall ist der, für den die Prüfung existiert: Aquaveil und Holy Sheltron senken in
PvE nur den erlittenen Schaden, ihre Barriere-Ids 3086 und 3026 gehören zu den PvP-Formen —
beide standen vorher als PvE-Kandidaten in `TODO.md` (AUDIT_LOG C28). Der Selbsttest deckt alle
vier Fälle an konstruierten Aktionen ab, den Aquaveil-Fall eingeschlossen.

Von den 55 Gruppen ohne jeden Vertreter standen 15 Ids hinter einer PvE-Barriereaktion — zehn
Jobbarrieren (Shake It Off, Seraphic Veil, Neutral Sect, The Spire, Improvised Finish, Divine
Caress) und fünf aus dem Occult Crescent, den der Auftraggeber spielt. Alle 15 sind aufgenommen
(A43); `ShieldStatus` führt jetzt 60 Ids, der Scan meldet dort null. Die verbliebenen 46 Ids in
44 Gruppen sind PvP-Formen, entfernte Alt-Status oder Duty-Effekte ohne Spieleraktion.

**Dieser Scan ist zugleich eine Schranke in der CI.** Er endet mit Rückgabewert 1, sobald ein
Kandidat mit dem Label `PvE barrier` ungelistet ist, und läuft im `DispatchChain`-Job von
`build.yaml` — dem Job ohne .NET, der in Sekunden antwortet (der Scan selbst braucht 0,1 s).
Das ist die Antwort auf die Alterung, nicht auf den Einzelfall: Die Liste war korrekt, als sie
geschrieben wurde, und wurde durch Erweiterungen anderswo unvollständig, ohne dass etwas
fehlschlug. Gegenprobe am konstruierten Defekt: eine Id entfernt → Rückgabewert 1 und die Id
wird benannt; wieder eingefügt → 0.

## scan14.py — fehlende Geschwister in **allen** Statuslisten

`scan13.py` beantwortet die Geschwisterfrage für `ShieldStatus`. Die Alterung, gegen die er
schützt, ist aber keine Eigenheit der Barrierenliste, sondern der Bauform: Jede handgepflegte
Aufzählung von Status-Ids ist bei ihrer Entstehung vollständig und wird durch eine Erweiterung
anderswo still unvollständig — Parnas' *Lack of Movement*. Dieser Scan stellt dieselbe Frage an
jede der 24 Listen in `StatusHelper.cs`.

**Er meldet, er entscheidet nicht**, und der Grund steht im Ergebnis: Ein gemeinsamer Anzeigename
macht zwei Ids nicht zur selben Wirkung. Damit die Beurteilung billig bleibt, wird zu jedem
fehlenden Geschwister die eigene Wirkbeschreibung gedruckt, dazu die Marke `same opening` oder
`differs` — ob der Text so beginnt wie der der bereits geführten Id. Ein gemeinsamer Anfang ist
die Signatur einer Trait-Aufwertung und gehört meist hinein; ein anderer Anfang ist eine andere
Wirkung unter geteiltem Namen und gehört meist nicht.

Erster Lauf: **187 fehlende Geschwister über 17 Listen**. Zwei davon waren Defekte im Sinn der
umgekehrten Antwort und sind behoben:

- **`RampartStatus`** führte `Rampart` (71), nicht aber `Rampart_1978` — die Fassung, die ein Tank
  ab Stufe 94 trägt (Geltungsbereich PLD WAR DRK GNB, Wirktext um die Heilaufwertung der Trait
  ergänzt). Die häufigste Minderung des Spiels war damit für jeden Leser der Liste unsichtbar:
  `HasMajorMitigation` blind, und die vorhandene `StatusProvide`-Staffelung von Shadow Wall und
  Shadowed Vigil löchrig. Ergänzt wurden `Rampart_1191`, `Rampart_1978`, `Rampart_4168` und
  `HallowedGround_1302`.
- **`ReprisalStatus`** führte 753 und 1193, nicht aber `Reprisal_2101`. Der Geltungsbereich
  PLD WAR DRK GNB statt der geteilten Rolle ist hier die Signatur der Trait-Fassung — *Enhanced
  Reprisal* hebt auf Stufe 98 die Minderung auf 15 % und die Dauer auf 15 s. `ReprisalPvE` trägt
  die Liste als `TargetStatusProvide`, die Sperre gegen erneutes Anwenden sah die Schwächung
  eines Endstufen-Tanks also nie.

Die Gegenprobe steht im Scan selbst: `Nebula_3051` („inflicting a portion of sustained damage back
to its source") und `Bloodwhetting_3030` („weaponskills generate HP equal to the amount of damage
dealt") teilen ihren Namen mit Minderungen, sind aber deren Reflexions- und Lebensraubhälfte, und
`Holmgang` 88 und 1305 sitzen auf dem *Ziel* der Unverwundbarkeit (AUDIT_LOG C15). Alle fünf
bleiben draußen und werden weiter gemeldet — die verbleibenden Treffer in `RampartStatus` und
`ShieldStatus` sind genau diese bewussten Ausschlüsse.

**Keine CI-Schranke.** Von den 186 verbliebenen Treffern sind 114 Rauschen aus zwei Listen, die
bewusst Teilmengen sind (`PhantomDispellable`, `PurifyPvPStatuses`), und der Rest verlangt je einen
Blick in die Wirkbeschreibung. Ein Rückgabewert, den man nur durch Wegsehen grün hält, wäre
schlechter als keiner; die Schranke bleibt bei `scan13.py`, wo die Mitgliedschaft aus der Aktion
maschinell entscheidbar ist. Der offene Rest ist in `TODO.md` als Defektklasse geführt.

*Grenze des Scans:* Er sieht nur Ids **in den Listen**. Ein Prüfpunkt, der eine einzelne Id direkt
nennt — `e.HasStatus(false, StatusID.Addle)` in `GetCurrentMitigationPercent` — altert genauso und
wird nicht erfasst. Auch das steht in `TODO.md`.

Selbsttest: gegen einen konstruierten Rampart-Fall (Basis-Id geführt, Trait-Fassung fehlt) meldet
er das Geschwister, und die Gegenprobe stellt sicher, dass Reflexionstext und Minderungstext als
verschiedene Anfänge gelten.

## scan15.py — Codeverweise in den Dokumenten, die ihr Ziel verloren haben

Die Konzepte zitieren den Baum nach Datei und Zeile — `WHM_Reborn.cs:566`,
`StatusHelper.cs:781` —, und jeder Commit an diesen Dateien verschiebt das Ziel, ohne das
Zitat anzufassen. Das Zitat sieht weiterhin richtig aus; wer ihm folgt, landet auf fremdem
Code oder, schlimmer, auf Code, der sich plausibel wie der Gegenstand liest. Das ist dieselbe
Alterung wie bei den Statuslisten, eine Ebene höher: bei der Niederschrift richtig, nach der
nächsten Änderung anderswo still falsch.

Prüfbar ist eine Zeilennummer für sich nicht — jede Zeile existiert. Prüfbar ist, ob **das,
wovon der Satz spricht**, dort steht, wo der Satz es behauptet. Der Scan paart deshalb jedes
Zitat mit den Bezeichnern in Backticks daneben und meldet `ok`, `moved` (Bezeichner in der
Datei, aber anderswo — mit Fundstelle), `gone` (Bezeichner gar nicht mehr da) oder
`no anchor` (kein prüfbarer Bezeichner; kein Befund, aber ausgewiesen, damit die Abdeckung
des Laufs sichtbar bleibt).

**`AUDIT_LOG.md` ist ausgenommen und wird getrennt ausgewiesen.** Das Archiv hält fest, was
zum Prüfzeitpunkt galt; eine seither verschobene Zeile datiert den Befund, sie entwertet ihn
nicht. Nur Dokumente, die den **geltenden** Stand behaupten, müssen stimmen.

Erster Lauf: 122 Zitate, 26 Befunde. Zwei Fehler des Scans selbst kamen dabei heraus und sind
behoben — in einer Tabellenzeile mit zwei Zitaten prüfte er jeden Bezeichner gegen beide
Dateien, was als „Code ist weg" gelesen wurde, und das Abschneiden am Nachbarzitat zerlegte
die Backtick-Paarung, sodass Prosa als Bezeichner gelesen wurde. Der Selbsttest deckt beides
ab. Nach der Korrektur blieben zehn echte Befunde in den geltenden Dokumenten.

**Behoben wurde nicht die Nummer, sondern die Bauform.** Wo ein eindeutiger Bezeichner
existiert, steht jetzt er statt der Zeile — `WHM_Reborn.ShouldStretchHolyStun` statt
`WHM_Reborn.cs:498`. Eine Zeilennummer altert bei jedem Commit, ein Bezeichner erst bei einer
Umbenennung, und die fällt beim Kompilieren auf.

## scan16.py — Aktionen, deren zweite Wirkung niemand liest

Konzept 08 benennt die Bauform: Eine Aktion mit zwei Wirkungen ist nur nach einer von ihnen
eingeordnet. Sanctus ist ein Schadenszauber, der auch betäubt — die Betäubung war bis zu
diesem Fork Teil keiner Entscheidung. Assize ist ein Angriffs-oGCD, der auch heilt. Armlänge
gilt als Rückstoßschutz, und ihre Verlangsamung +20 % auf jeden Angreifer wurde **nirgends**
gelesen.

Der Scan nimmt den Wirktext jeder PvE-Aktion, zieht die Kontroll- und Minderungswirkungen auf
Gegner heraus (Slow, Stun, Heavy, Bind, Blind, Silence, Paralysis, „reduces damage dealt by")
und fragt, ob der Baum den zugehörigen Status je liest — als Mitglied einer `StatusHelper`-Liste
oder wenigstens als einzelne `StatusID`. Die Ausgabe trennt Tank und Heiler vom Rest, weil die
Erhebung vollständig zu führen ist, die Bearbeitung aber dem Nutzungsprofil folgt.

Ergebnis: 1457 PvE-Aktionen, 52 mit einer solchen Wirkung, **ein** Fund im Tank- und
Heilerprofil, der eine Entscheidung ändert — Armlänge. Der Wirktext der Verlangsamung nennt
ausdrücklich die Verzögerung der **Automatikangriffe**, aus denen Trash-Gegner den Großteil
ihres Schadens liefern; die Drosselung liegt damit in der Größenordnung von Rampart. Genutzt
ist der Befund in der Barrierenregel des Dunkelritters (`PackSlowed`); die Frage, ob Armlänge
auch **als** Minderungswerkzeug gewirkt werden soll, steht in `TODO.md`, weil sie mit ihrer
Rolle als einzigem Rückstoßschutz kollidiert. Die übrigen Treffer liegen in Bozja und den
Tiefen Gewölben und sind erfasst, nicht bearbeitet.

Der Selbsttest deckt die Unterscheidung ab, die den Scan trägt: Ein Kontrolleffekt wird
erkannt, eine reine Heilung nicht, und die Rollenzuordnung trennt Tank, Heiler und den Rest.

## Bekannte Fehlanzeigen der älteren Skripte

Beim zweiten Durchgang über den Heiler- und Tankbestand (A52) meldeten drei Skripte Treffer, die
keine sind. Sie stehen hier, damit der nächste Durchgang sie nicht erneut aufrollt:

- **`scan.py`, „RotationDesc nennt X, Rumpf benutzt X nie"** liest nur den unmittelbaren
  Methodenrumpf. Wo eine Rotation die Aktion in eine Hilfsmethode auslagert, meldet der Scan sie als
  ungenutzt, obwohl sie gewirkt wird — belegt an `SCH_Reborn` (Sacred Soil, in einer Hilfsmethode),
  `PLD_Reborn` (Sheltron/Holy Sheltron, in `DefenseSingleAbility` weiter unten) und `SGE_Reborn`
  (Eukrasian Prognosis II über `SetEukrasia`). Die `RotationDesc` ist zudem eine Anzeigeliste; selbst
  ein echter Treffer wäre ein Oberflächen-, kein Kampffehler.
- **`scan2.py`, „wiederholte Bedingung in derselben Methode"** trennt keine Rollenzweige.
  `StateUpdater.ShouldAddDefenseSingle` prüft `IsHostileCastingTankBusterAtMe` und
  `BMRTankbusterImminent` je zweimal, aber in verschiedenen Rollenzweigen mit verschiedener
  Bedeutung — der Zweig für Schadensklassen trägt zusätzlich `PartyTank == null` (A6).
- **`scan11.py`, `ModifyLivingDeadPvE`** ist der in seinem eigenen Abschnitt bereits beschriebene
  Fall: ein Debuff, den die Aktion tatsächlich auf den Spieler legt (Walking Dead).

## check_fork_version.py — steht die Fork-Versionsnummer noch auf dem Upstream-Release?

**Warum es diese Prüfung braucht, ist selbst der Befund:** Upstream führt im Quellbaum **gar keine**
Version. Sein `Directory.Build.props` hat keine `<Version>`-Zeile; `publish.yaml` leitet
`AssemblyVersion`, `FileVersion`, `PackageVersion` und `InformationalVersion` erst beim
Veröffentlichen aus dem **Git-Tag** ab, der den Lauf ausgelöst hat. Die drei Versionszeilen in
unserer Fassung sind eine Fork-Ergänzung — sonst meldete die Assembly 1.0.0, und das Paket trüge die
nackte Upstream-Identität.

Daraus folgt: **Ein Upstream-Merge kann die Zahl nicht mitbringen, weil dort nichts ist, was
mitkäme.** Sie ist eine handgepflegte Zahl, die eine Tatsache anderswo spiegelt — den höchsten
Upstream-Tag in der eigenen Historie. Dieselbe Alterungsform wie eine handgepflegte Statusliste, und
sie ist genauso gealtert: nach dem Sync auf 7.5.6.0 gesetzt, während Upstream längst 7.5.6.1
getaggt hatte.

Der Vergleich läuft bewusst gegen die Tags, die **Vorfahr von HEAD** sind, nicht gegen alle Tags des
Upstreams: Die Zahl behauptet, auf welchem Release der Fork *steht*, nicht welches es gibt. Ein Tag,
den wir noch nicht gemergt haben, ist deshalb kein Befund.

Rückgabewert 1, wenn die Zahl zurückliegt — als Schranke tauglich, aber **nicht** in `build.yaml`
eingehängt: Das ist eine Entscheidung über den Veröffentlichungspfad und liegt beim Auftraggeber.
Bis dahin gehört das Skript in den Sync-Ablauf, gleich nach `git fetch --prune --tags upstream`.

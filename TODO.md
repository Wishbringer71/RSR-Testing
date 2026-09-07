# TODO — nur offene Arbeit

Getrennt nach Defekt (Abweichung vom beabsichtigten Verhalten), technischer Schuld (bewusst eingegangener Kompromiss mit Auflösungsbedingung) und offener Arbeit. Je Eintrag der betroffene Personenkreis: **N** Endnutzer des Plugins · **R** Autoren abgeleiteter Rotationen, die `RotationSolver.Basic` als Paket beziehen · **U** Upstream-Pflege.

## Defekte

### ChurinDNC wertet die BMR-Downtime ohne Vorzeichenprüfung aus · N, U

`ChurinDNC.cs:777-843` (Upstream) liest `BMRNextDowntimeIn`/`-EndIn` ohne Vorzeichenprüfung. BossModReborn liefert diese Werte als `(Aktivierung − jetzt)`, sie sind während einer laufenden Downtime also negativ, und die Rotation kann „Downtime läuft" nicht von „Downtime kommt gleich" unterscheiden: `if (BMRNextDowntimeIn >= 15f) return;` kehrt dann nicht zurück, und die folgende `<`-Bedingung ist immer erfüllt. Die Normalisierung der Schadensvorhersagen ist erledigt (AUDIT_LOG A11); hier wäre ein Filter falsch, weil das Vorzeichen die Information trägt.

Nicht behoben, weil die Absicht dieser fremden Rotation ohne ihren Autor nicht belegbar ist und eine Änderung ohne Spieltest nicht abzusichern wäre. Auflösung: Rückfrage an den Upstream-Autor oder Laufzeitbeobachtung.

### Das Fork-NuGet-Paket trägt die Identität des Upstream-Pakets · R

`RotationSolver.Basic.csproj` setzt `PackageId` auf `RotationSolverReborn.Basic` — denselben Bezeichner, unter dem Upstream veröffentlicht. `publish.yaml:41` übergibt `PackageVersion=${{ env.numericVersion }}`, also die Version **ohne** das Fork-Suffix. Das im Release-ZIP mitgelieferte `.nupkg` heißt damit `RotationSolverReborn.Basic 7.5.5.41` und ist von der gleichnamigen Upstream-Fassung nicht zu unterscheiden, obwohl es einen anderen Inhalt hat.

Das Suffix nachzureichen behebt es nicht: NuGet entfernt SemVer-2.0-Build-Metadaten bei der Normalisierung — `1.0.7+r3456` wird als `1.0.7` behandelt ([Package versioning](https://learn.microsoft.com/nuget/concepts/package-versioning#normalized-version-numbers)) —, und dieselbe Quelle hält fest, dass ein Repository zwei Pakete gleicher normalisierter Version nicht als verschiedene Pakete führen soll. Ein unterscheidendes Merkmal wäre nur ein eigener `PackageId` oder ein Prerelease-Label (`-wsh1`), das in die Identität eingeht.

**Kosten:** Wer das Paket aus dem Release-ZIP in einen lokalen Feed legt, kann es nicht vom Upstream-Paket unterscheiden; NuGet löst je Identität einmal auf und cacht. Genau in diesem Fall werden die entfernten Member (siehe unten) als unerklärlicher Compilefehler sichtbar. Kein Fehlverhalten zur Laufzeit des Plugins.

**Auflösungsbedingung:** Die Wahl zwischen eigenem `PackageId`, Prerelease-Label und dem Verzicht auf die Paketauslieferung trifft der Auftraggeber; alle drei berühren die Autoren abgeleiteter Rotationen unterschiedlich. Zusammen mit dem Eintrag zum Release-Ballast zu entscheiden, der dasselbe `.nupkg` betrifft.

### Argumentlose `IsLastAction()`-Vergleiche sind konstant wahr · N, U

`IsLastAction()`, `IsLastGCD()` und `IsLastAbility()` sind `params ActionID[]`-Überladungen. Ohne Argument prüft `IsActionID` eine leere Liste und liefert `false` (`IActionHelper.cs:196-211`). Der Ausdruck `IsLastAction() == IsLastGCD()` ist damit `false == false` und immer wahr; für `IsLastAction() == IsLastAbility()` gilt dasselbe. Die gemeinte Prüfung existiert bereits als `IActionHelper.IsLastActionGCD()` (`DataCenter.LastAction == DataCenter.LastGCD`) und wird in `CustomRotation_Ability.cs:688`, `697` und `705` korrekt verwendet.

Fünf Fundstellen, alle aus Upstream übernommen und dort unverändert vorhanden:

- `WHM_Reborn.cs:135` (zweimal) und `BeirutaWHM.cs:401-402` (zweimal) — sollen Thin Air auf das Weave-Fenster unmittelbar nach einem GCD beschränken.
- `DRG_Reborn.cs:131` — dieselbe Absicht vor Stardiver.
- `CustomRotation_OtherInfo.cs:1798` in `HasWeaved()`. Der Kommentar darüber schreibt die gemeinte Prüfung wörtlich aus, der Code führt sie nicht aus — nach der Auslegungsregel ist der Widerspruch der Befund, nicht der Kommentar.

**Wirkung:** In den vier Rotationsstellen entfällt eine Einschränkung, die nie gegriffen hat; Thin Air und Stardiver können damit in jedem Weave-Slot statt nur im ersten fallen. Schwerer wiegt `HasWeaved()`: über `CanEarlyWeave` (`CustomRotation_OtherInfo.cs:1353`) kollabiert `(!HasWeaved() || WeaponRemain > LateWeaveWindow)` auf die zweite Hälfte, die „noch nicht geweavt"-Bedingung fällt also weg. Verbraucher sind `ChurinMNK` (drei Stellen) und `ChurinBRD` (vier); die Reborn-Rotationen nutzen `CanEarlyWeave` nicht.

**Auflösung:** Ersetzung durch `IsLastActionGCD()` beziehungsweise durch eine gleichwertige Prüfung für `HasWeaved()`. Für die beiden Churin-Rotationen ist die Absicht des fremden Autors zu berücksichtigen, weil die Behebung deren Weave-Zeitpunkte tatsächlich verschiebt.

### Die Heilzielauswahl legt die Invulnerabilitätsprüfung invertiert aus · N

`NoNeedHealingInvuln()` (`StatusHelper.cs:653`) ist `WillStatusEndGCD(2, 0, false, NoNeedHealingStatus)` und liefert **true**, wenn der Schutzstatus fehlt oder binnen zwei GCDs endet — also „heilen ist wieder sinnvoll". Der Name sagt das Gegenteil, und die beiden Aufrufstellen folgen verschiedenen Lesarten:

- `StateUpdater.cs:726` und `:771` folgen der Implementierung: `if (h == 0 || !NoNeedHealingInvuln()) return false;` — kein Heilflag, solange der Schutz läuft. **Richtig.**
- `ObjectHelper.cs:125` folgt ihr ebenfalls, mit erklärendem Kommentar darüber („unless they are riding an invulnerability (Superbolide leaves them at 1 HP on purpose)"). **Richtig**, und der Beleg dafür, welche Lesart gemeint ist.
- `ActionTargetInfo.cs:3536` in `GeneralHealTarget` folgt dem Namen: `if (!o.NoNeedHealingInvuln()) healingNeededObjs.Add(o);` — aufgenommen wird, wessen Schutz **noch läuft**. **Invertiert.**
- `SCH_Reborn.cs:830` ebenso: `member.GetHealthRatio() <= ExcogHeal && !member.NoNeedHealingInvuln()`. Excogitation geht nur auf Ziele, die gerade unverwundbar sind. **Invertiert.** Zweiter Fundort derselben Defektklasse, erst bei der Erhebung zu `docs/rotation-flow/09-tank-selfprotection.md` gefunden.

**Wirkung, gegenüber der ersten Fassung dieses Eintrags nach oben korrigiert:** In die priorisierte Zielliste kommen nur Spieler mit laufendem Schutzstatus; alle übrigen fallen heraus. Normalerweise ist die Liste damit leer — und dann laufen **alle** darauf aufbauenden Stufen ins Leere: die Heiler-Vorrangprüfung, die Tank-Vorrangprüfung und die Auswahl des am schwersten Verletzten. `GeneralHealTarget` liefert `null`, und der Aufrufer greift zuerst auf den Träger der Tank-Haltung zurück, sonst auf `partyMembers[0]` (`ActionTargetInfo.cs:3497-3512`). Praktisch heißt das: **Die generische Heilzielwahl ist blind für den am schwersten Verletzten und wählt faktisch immer den Tank.** Einzig die Selbstprüfung gegen `HealthSelfRatio` funktioniert noch, weil sie die Liste nicht benutzt.

Trägt dagegen ein Tank gerade einen Schutzstatus und braucht ein anderer Spieler Heilung, wird **der geschützte Tank bevorzugt** — das Gegenteil der Absicht, und beim Dunkelritter verhindert es womöglich den Tod, der den Übergang nach Walking Dead erst auslöst.

Damit ist dies kein Effizienz-, sondern ein Überlebensdefekt.

**Auflösung:** `if (o.NoNeedHealingInvuln())` an beiden Stellen. `SCH_Reborn.cs:830` ist für sich unkritisch — eine Rotation, eine Fähigkeit — und kann getrennt vorgezogen werden. `ActionTargetInfo.cs:3536` betrifft dagegen die zentrale Heilzielwahl aller Jobs: Nach der Korrektur greift die Priorisierung erstmals wirklich, was das Heilverhalten breit verändert. Freigabepflichtig, nicht nebenbei zu ändern, und gemeinsam mit dem Umbau vom Filter zur Prioritätsstufe (siehe nächster Punkt).

Der irreführende Name ist die Ursache der Wiederholbarkeit und getrennt zu behandeln: Solange `NoNeedHealingInvuln` „true bei fehlendem Schutz" bedeutet, entsteht der Fehler bei jeder neuen Aufrufstelle erneut. Ihn anzupassen, ohne die Aufrufstellen zu prüfen, würde den Beleg tilgen; er steht zudem in der Paketoberfläche.

### Die Heilunterdrückung bei Unverwundbarkeit prüft den Gesundheitsstand nicht · N

`NoNeedHealingStatus` unterdrückt Heilung, solange ein Schutzstatus läuft. Die dahinterliegende Annahme — wer nicht sterben kann, braucht keine Heilung — gilt nur, wenn der Tank den **Ablauf** des Schutzes überlebt. Genau das prüft die Regel nicht.

**Korrektur des Belegs:** Die erste Fassung führte hier den Paladin an — er zünde Hallowed Ground bei fünf Prozent und stehe zehn Sekunden später bei denselben fünf Prozent. Das Beispiel trägt nicht: `HallowedGround` steht in **keiner** Form in `NoNeedHealingStatus`, der Paladin löst also gar keine Unterdrückung aus. Der tragfähige Beleg ist **Superbolide**: Es steht in der Liste, setzt die Gesundheit sofort auf 1 und unterdrückt die Heilung anschließend zehn Sekunden lang, ohne diesen Stand je zu prüfen. Für Holmgang gilt dasselbe, sobald der Krieger heruntergedrückt wurde. Am Sachverhalt ändert die Korrektur nichts, nur am Beleg — und der Paladin-Fall wird real, sobald die fehlenden Ids ergänzt werden.

**Die Unverwundbarkeit ist damit nicht der Grund, Heilung auszusetzen, sondern die beste Gelegenheit, sie anzubringen.** Die Zwei-GCD-Freigabe vor Ablauf mildert das, reicht aber nicht: Einen Tank von wenigen Prozent auf sicher zu bringen, dauert länger als zwei GCDs.

| Job | Fähigkeit | Wirkung auf die HP | Heilung im Schutzfenster |
|---|---|---|---|
| PLD | Hallowed Ground | unverändert, kein Schaden geht durch | nötig, wenn beim Zünden wenig HP standen |
| WAR | Holmgang | fallen nicht unter 1 | nötig, sobald er heruntergedrückt wurde |
| GNB | Superbolide | **sofort auf 1** | zwingend |
| DRK | Living Dead | Tod wird in Walking Dead umgewandelt | Phase 1 **schädlich**, Phase 2 erforderlich |

**Zwei frühere Aussagen dieses Eintrags sind damit falsch** und hier ersetzt: dass ein Schild auf Holmgang oder Superbolide wirkungslos sei (er überdauert die Phase und liegt, wenn der Tank am verwundbarsten ist), und dass bei Hallowed Ground Heilung wirklich unnötig sei (sie ist es nur, wenn der Paladin mit hohen HP gezündet hat — was die Regel nicht wissen kann).

**Richtige Konstruktion:** nicht ausschließen, sondern **in der Priorität herabstufen**. Ein geschützter Tank soll hinter jedem ungeschützten Gruppenmitglied stehen, aber geheilt werden, wenn sonst niemand Bedarf hat — im Schutzfenster, wo es am sichersten ist. `GeneralHealTarget` besitzt bereits eine Rangfolge (Selbst → Heiler → Tank → niedrigste Gesundheit); nötig wäre eine zusätzliche Stufe, keine Ausschlussliste. Einzige echte Ausnahme bleibt der Dunkelritter in Phase 1.

**Abdeckung der Liste, am Artefakt geprüft.** Die erste Fassung meldete pauschal vier fehlende `HallowedGround`- und drei fehlende `Holmgang`-Ids. Das war ein Nullbefund über den bloßen Bezeichner, ohne Prüfung der Wirkbeschreibung in `Status.resx`. Tatsächlich gilt:

| Status | Beschreibung | Bewertung |
|---|---|---|
| `HallowedGround` (82), `HallowedGround_1302` | „Impervious to most attacks" | **fehlen, sind zu ergänzen** |
| `UndeadRebirth` (3255) | „Most attacks cannot reduce your HP to less than 1" | **fehlt, ist zu ergänzen** — dritte Living-Dead-Phase, siehe eigener Eintrag |
| `Holmgang_409` | „Most attacks cannot reduce your HP to less than 1" | bereits vorhanden, **richtig** |
| `Holmgang` (88), `Holmgang_1305` | „Unable to move until effect fades" | **nicht** ergänzen — Bewegungs-Debuff auf dem Ziel, kein Schutz |
| `Holmgang_1304` | Bewegungsunfähigkeit **und** HP-Schutz | PvP-Selbstform; nur relevant, wenn PvP-Rotationen die Liste lesen |
| `WalkingDead` (811) | Heilung ist dort Überlebensbedingung | auskommentiert, **richtig so** — Beleg verstandener Entwurfsabsicht |

Solange die Regel in ihrer heutigen Form falsch ist, wäre das Ergänzen der fehlenden Ids eine Verschlimmerung — erst die Konstruktion, dann die Abdeckung.

**Auflösungsbedingung:** gemeinsam mit dem vorigen Punkt, weil beide dieselbe Prüfung betreffen. Beide sind in `docs/rotation-flow/09-tank-selfprotection.md` als Teil eines umfassenderen Bildes eingeordnet: Dort sind sämtliche Tank-Selbstschutzmechaniken danach getrennt, ob sie einen Auslöser haben, den fremde Heilung oder ein fremder Schild abfangen kann. Die Einzelheiten und der Umsetzungsplan stehen dort, nicht hier.

### Die dritte Living-Dead-Phase ist der Heilentscheidung unbekannt · N

Die Aktionsbeschreibung zu Living Dead (`ActionId.resx`, Aktion 3638) benennt drei Phasen: **Living Dead** (10 s, Tod wird umgewandelt), **Walking Dead** (10 s, kumulierte Heilung in Höhe der maximalen HP entscheidet über Leben und Tod) und — bei Erfolg — **Undead Rebirth** über die Restlaufzeit, in der Angriffe die Gesundheit nicht unter 1 drücken.

`StatusID.UndeadRebirth` (3255) kommt im gesamten Baum genau einmal vor: als `StatusProvide` von `ModifyLivingDeadPvE` (`DarkKnightRotation.cs:267`). In `NoNeedHealingStatus` fehlt der Status. Der Heiler heilt einen Dunkelritter in dieser Phase also weiter, obwohl dieser nicht sterben kann und die Heilbedingung bereits erfüllt ist.

**Wirkung:** verschwendete GCDs und MP in genau dem Moment, in dem der Rest der Gruppe die Heilung braucht — der Dunkelritter hat gerade eine volle Maximalgesundheit an Heilung aufgenommen. Das Überleben des Tanks ist nicht berührt; betroffen ist allein die Ressourcenschonung.

**Entstehung** nach Parnas' *Lack of Movement*: Die Aufzählung war bei ihrer Entstehung vollständig und wurde durch eine spätere Spielerweiterung unrichtig, ohne dass etwas fehlschlug. Das macht den Fund zu einer Defektklasse: Solange dort eine Aufzählung steht, wo eine Fähigkeitsprüfung stehen müsste, ist ein Nachfolgebefund bei der nächsten Erweiterung zu erwarten.

**Auflösung:** Aufnahme in `NoNeedHealingStatus`, aber erst **nach** dem Umbau vom Filter zur Prioritätsstufe — davor würde sie das Ob beeinflussen statt der Reihenfolge. Damit an denselben Freigabezeitpunkt gebunden wie die beiden vorigen Punkte.

### Ein Gruppenmitglied in einer Zwischensequenz bricht vier Zielsuchen ganz ab · N, U

`ObjectHelper.IsConditionCannotTarget()` (`ObjectHelper.cs:593`) liest `obj.OnlineStatus.RowId` und liefert `true` für die Werte 15 und 5 — eine Eigenschaft **des geprüften Ziels**, nicht des Spielers oder der Gruppe. Ein einzelnes Gruppenmitglied kann sie erfüllen, etwa während einer Zwischensequenz.

An sieben Stellen in `ActionTargetInfo.cs` steht darauf `return null` statt `continue`, jeweils nach demselben Bauplan:

```csharp
if (m.IsConditionCannotTarget())
{
    PluginLog.Debug($"FindTankTarget 1: {m.Name} is a tank with TankStanceStatus.");
    return null;              // bricht die gesamte Suche ab
}
if (!m.IsConditionCannotTarget())
{
    PluginLog.Debug($"FindTankTarget 1: {m.Name} is a tank with TankStanceStatus.");
    return m;
}
```

Betroffen sind `FindDancePartner` (2699, 2730), `FindKardia` (3154, 3184, 3212) und `FindTankTarget` (4021, 4045). Ein nicht anvisierbarer Kandidat beendet damit die Funktion, statt übersprungen zu werden — samt aller nachgelagerten Ausweichschleifen und Endfallbacks. `FindTankTarget()` (deklariert `ActionTargetInfo.cs:4007`) hat zwei solcher Schleifen und einen abschließenden `RandomPickByJobs(…, JobRole.Tank)`; keiner davon wird erreicht.

**Wirkung:** Solange ein Gruppenmitglied in der Zwischensequenz steht — beim Ersteintritt in Story-Dungeons und Raids der Regelfall —, findet der Tänzer keinen Tanzpartner, der Weiser kein Kardion-Ziel und `TargetType.Tank` **kein Tankziel**. Der letzte Punkt trifft die Zielwahl tank-gerichteter Heilung und Mitigation und ist damit ein Überlebensdefekt, kein Komfortmangel.

**Beleg für die Entwurfsabsicht:** In derselben Datei stehen drei strukturgleiche Stellen mit dem richtigen `continue` (4182, 4198, 4221). Die Debug-Meldung ist in beiden Zweigen jeweils **wortgleich** und beschreibt durchweg einen gefundenen Kandidaten, obwohl der eine Zweig abbricht — Widerspruch zwischen Meldung und Code. Beides zusammen ist das Kennzeichen von *Ignorant Surgery* nach Parnas: Klon einer Nachbarstelle, bei dem Bedingung und Rückgabe geändert, die Meldung aber stehen gelassen wurde.

**Geprüfte Gegenhypothese:** `return null` könnte gewollt sein („lieber warten als einen schlechteren Partner dauerhaft binden"). Widerlegt durch die drei `continue`-Nachbarstellen, durch die wortgleichen Meldungen und dadurch, dass die vorhandenen Ausweichschleifen unter dieser Lesart nie erreicht werden — eine Ausweichlogik, die nie greift, war nicht so gemeint.

**Fundweise:** `.github/scripts/audit/scan8.py`, beim ersten Lauf. Nicht Teil des laufenden Auftrags, deshalb nur erfasst.

**Auflösung:** `return null` → `continue` an den sieben Stellen; die Debug-Meldungen sind dabei an das anzupassen, was der Zweig tut. Der Blast Radius von `FindTankTarget` ist erheblich (jede Aktion mit `TargetType.Tank`), deshalb freigabepflichtig. Der Code stammt aus Upstream und betrifft ihn ebenso.

### Die Beiruta-Rotationen prüfen den falschen Holmgang-Status · N

`BeirutaAST.cs:387`, `BeirutaSCH.cs:1102` und `:1118` sowie `BeirutaWHM.cs:236` prüfen `StatusID.Holmgang` (88). Status 88 ist laut `Status.resx` „Unable to move until effect fades" — der Bewegungs-Debuff, den Holmgang auf dem **Ziel** des Kriegers hinterlässt. Der Schutzstatus auf dem Krieger selbst ist Status 409 („Most attacks cannot reduce your HP to less than 1"), im Code als `Holmgang_409` geführt und an allen zentralen Stellen richtig verwendet.

**Wirkung:** Die betroffenen Prüfungen erkennen einen Krieger unter Holmgang nie. Da es sich um Ausschlussprüfungen handelt, ist die Folge zusätzliche statt fehlender Heilung — die sichere Richtung, aber nicht die gemeinte.

**Betroffenenkreis:** nur Nutzer der Beiruta-Rotationen; der Auftraggeber nutzt sie nach eigener Angabe nicht. Der Code stammt aus Upstream.

**Auflösung:** `Holmgang_409` statt `Holmgang`, oder besser der Verweis auf `StatusHelper.NoNeedHealingStatus`, damit die Fundstellen nicht erneut hinter der Statusliste zurückbleiben. Fremde Rotation, deshalb mit derselben Zurückhaltung zu behandeln wie die übrigen Upstream-Befunde: Adressat ist der Upstream-Autor.

## Technische Schuld

### Doppelte Zustandswahl in den Zustandskommandos · N

Die Zustandswahl liegt an zwei Orten: implizit in `AdjustStateType`, wo `/rotation Auto` über `UpdateTargetingIndex` selbst durch die Zielarten schaltet, sofern `ToggleAuto` aus ist; explizit in den fünf `Cycle*`-Methoden, die dieselbe Aufgabe erneut lösen und über `CycleType` bzw. `DTRType` am Chatkommando und am Leistenklick hängen. Da die `Cycle*` ebenfalls `DoStateCommandType` rufen, greift `AdjustStateType` auch dort; die Toggle-Optionen wirken dadurch als Krücken für fehlende Übergänge, statt als unabhängige Achse.

**Kosten:** `DTRAllAuto` kollabiert mit aktivem `ToggleAuto` auf Off ↔ Auto(0), die Zielarten-Rotation ist dann tot. Umgekehrt ist `ToggleAuto` bei `DTRManualAuto` der einzige Ausschaltweg über die Leiste — ein pauschales Umgehen der Toggle-Auswertung würde ihn beseitigen.

**Auflösungsbedingung:** erst mit einer Möglichkeit zur Laufzeitbeobachtung; ein Zustandsautomat mit acht Zuständen, fünf Zykluswegen und zwei Schaltern ist statisch nicht abzusichern. Bei einem Eingriff ist der Persistenzvertrag zu beachten: `DTRType` und `CycleType` liegen als Ordinalzahlen in der Nutzerkonfiguration, ihre Reihenfolge ist nicht frei änderbar.

Geprüfte Nicht-Fehlstellen: `DTRManualAuto` bildet den vom Enum-Text beschriebenen Zwei-Zustands-Zyklus ab (kein Fehler, AUDIT_LOG A14); ein zu großer `TargetingIndex` kann keinen Indexfehler auslösen, `DataCenter.TargetingType` rechnet `% Count` (`DataCenter.cs:284-302`).

### Selbstlernende AoE-Liste wächst ohne fachliche Schranke · N

`Watcher.ActionFromEnemy:111-148` nimmt eine Gegner-Aktion dauerhaft in `HostileCastingArea` auf, wenn die Party mindestens vier Mitglieder hat, die Aktion eine Wirkzeit besitzt, zur Kategorie Spell/Weaponskill/Ability gehört und **jedes** Party-Mitglied im selben Effektsatz Schaden genommen hat. „Record AOE actions" ist standardmäßig an.

**Korrektur einer früheren Aussage:** Der Eintrag behauptete, ein gelernter Eintrag lasse sich nur durch Editieren der Datei zurücknehmen. Das ist zweifach widerlegt — `RotationConfigWindow.cs:3745` bietet „Reset and Update AOE List" (`ResetHostileCastingArea`, lädt die gepflegte Liste neu), und `DrawActionsList` erlaubt das Entfernen einzelner Einträge über Kontextmenü und Entf-Taste. Die Codedokumentation empfiehlt den Reset ausdrücklich nach jedem Patch. Damit ist dies kein Defekt, sondern eine Automatik mit vorhandenen Korrekturwerkzeugen.

**Kosten:** Zwischen einer falsch gelernten Aktion und der nächsten Nutzerkorrektur mitigiert RSR auf einen ausweichbaren Effekt. Durch die Reichweitenprüfung in `IsHostileCastingArea` entschärft.

**Auflösungsbedingung:** ob sich echte Raidwides beim Lernen von ausweichbaren Flächen unterscheiden lassen (Kandidat: `CastType`/`EffectRange`), ist ohne Spieldaten nicht entscheidbar. Eine Verschärfung wäre eine Verhaltensänderung ohne Nachweismöglichkeit und gehörte deshalb hinter eine eigene Option, nicht in den Standardpfad. Geprüfte Nicht-Fehlstelle: das Speichern läuft asynchron, kein blockierendes Schreiben im Kampfpfad.

#### Geprüfter Vorschlag: Schadenshöhe als Aufnahmekriterium

Vorschlag des Auftraggebers: Liegt der Schaden unterhalb dessen, was der Schild ohnehin auffängt, braucht die Aktion nicht in die Liste — der Schild wäre verschwendet. Auftrag war, das zu widerlegen oder modifiziert aufzunehmen. Ergebnis: **der Kerngedanke trifft eine echte Lücke, die vorgeschlagene Form trägt nicht, eine modifizierte Form ist umsetzbar.**

**Die Lücke ist real.** `HostileCastingArea` ist ein `HashSet<uint>` (`OtherConfiguration.cs:28`) — eine Aktion ist drin oder nicht. Eine Fläche, die zwei Prozent der Gesundheit nimmt, steht gleichberechtigt neben einer, die sechzig nimmt, und löst über `IsHostileCastingAOE` → `ShouldAddDefenseArea` (`StateUpdater.cs:178`) dieselbe Gruppenmitigation aus. Die Kostenseite ist ebenfalls real, nur anders benannt als im Vorschlag: Was verbraucht wird, ist nicht „der Schild", sondern der **Cooldown** — eine auf eine Bagatelle gelegte Reprisal fehlt beim nächsten großen Einschlag.

**Die Datenquelle liegt bereits an der richtigen Stelle.** `Watcher.cs:137` liest beim Lernen `damageEffect.value`, prüft davon aber nur `> 0`. Der Betrag ist da und wird verworfen.

**Vier Einwände gegen die einfache Form (Filter beim Lernen):**

1. **Zirkelschluss.** Gemessen wird der Schaden *nach* der damals wirkenden Mitigation. Hat die Gruppe beim ersten Vorkommen gut mitigiert, fällt der Wert klein aus, die Aktion wird ausgeschlossen — und künftig wird nicht mehr mitigiert, wodurch der Schaden groß wird. Die Regel würde ihre eigene Voraussetzung zerstören.
2. **Alterung.** Ein absoluter Betrag oder ein an einem Schildwert gemessener Schwellwert veraltet mit Item-Level, Content-Sync und Vulnerability-Stapeln. Das ist dasselbe *Lack of Movement*-Muster wie bei den Statusaufzählungen: heute richtig, nach der nächsten Erweiterung still falsch.
3. **„Der Schild" ist der falsche Maßstab.** Was `DefenseArea` auslöst, ist überwiegend prozentuale Schadensminderung (Reprisal, Addle, Feint, Kerachole), nicht Absorption. Prozentuale Minderung wird nicht „verschwendet" — sie skaliert mit dem Schaden, ihr Nutzen ist bei kleinem Schaden nur klein. Und welcher Schild gemeint wäre, hängt an Job und Level; ein gruppenweiter Schwellwert daraus ist nicht ableitbar.
4. **Serien.** Mehrere kleine Einschläge kurz hintereinander summieren sich. Jeder einzeln unter der Schwelle, zusammen tödlich — eine Einzelwertprüfung sieht das nicht.

**Modifizierte Form, die die vier Einwände umgeht:** nicht beim Lernen filtern, sondern die Größenordnung **mitspeichern** und erst beim Verbrauch entscheiden.

- `HashSet<uint>` → Zuordnung Aktion auf **höchsten je beobachteten Schadensanteil**, gemessen als `value / MaxHp` des getroffenen Mitglieds.
- *Anteil statt Betrag* entschärft Einwand 2 — ein Anteil altert nicht mit dem Item-Level.
- *Höchstwert statt letztem Wert* entschärft Einwand 1 — eine einzige ungemitigierte Beobachtung setzt den wahren Wert, und spätere gut mitigierte Vorkommen senken ihn nicht wieder.
- *Entscheidung beim Verbrauch* entschärft Einwand 3 und 4 — die Schwelle ist eine Nutzeroption, keine feste Zahl, und sie kann später um eine Serienbetrachtung ergänzt werden, ohne die gelernten Daten neu zu erheben.
- **Standard 0**, also unverändertes Verhalten, bis der Nutzer eine Schwelle setzt. Die Wirkung ist statisch nicht belegbar, gehört also nach der Projektregel hinter eine Option.

**Kosten dieser Form, die nicht zu verschweigen sind:** `HostileCastingArea` ist gespeicherte Nutzerkonfiguration und damit ein Persistenzvertrag. Ein Typwechsel von `HashSet<uint>` auf eine Zuordnung bricht die vorhandene Datei; nötig wäre ein Migrationspfad, der bestehende Einträge mit unbekanntem Anteil übernimmt und sie bis zur ersten Neubeobachtung wie heute behandelt. Zudem ist der gemessene Anteil je Gruppenmitglied verschieden (verschiedene Maximalgesundheit, verschiedene Mitigation) — festzulegen wäre, ob der höchste oder der mittlere Anteil des Effektsatzes zählt.

**Bewertung:** technische Schuld, kein Defekt — die heutige Grobheit ist eine bewusste Vereinfachung, keine Fehlfunktion. Die Auflösung ist an dieselbe Bedingung gebunden wie der Rest dieses Eintrags: Ohne Spielbeobachtung ist nicht belegbar, dass die Schwelle mehr nützt als schadet.

### `SpreadDamagePaths` enthält keinen Spread-Marker · N

`DataCenter.cs:2036-2043`. Zwei der vier Pfade stehen wortgleich in `SharedDamagePaths` (2025-2026), die anderen beiden sind laut eigenem Kommentar „AOE share markers", also ebenfalls Stack-Marker. Ohne Fehlwirkung, weil `IsCastingAreaVfx` alle drei Listen prüft. **Kosten:** eine Kategorie, die etwas anderes verspricht, als sie enthält. **Auflösung:** entweder echte Spread-Marker ergänzen oder die Liste streichen — beides erfordert Spieldaten, die offline nicht vorliegen. Nebenbefund: `SharedDamagePaths` führt `vfx/lockon/eff/com_trg01_0c` zweimal (2022 und 2024), im `FrozenSet` folgenlos.

### Release-Paket enthält vermeidbaren Ballast · N, R

**Am Artefakt belegt** (`latest.zip` von 7.5.5.41+wsh1, 5,35 MB): Nutzlast sind `RotationSolver.dll`, `RotationSolver.Basic.dll`, `ECommons.dll` und `RotationSolver.json`; dazu kommen `RotationSolver.Basic.xml` (7,52 MB), der Analyzer samt Symbolen (5,59 MB), das NuGet-Paket (1,54 MB) und `RotationSolver.Basic.pdb` (1,27 MB).

**Ursache** ist weder die Prune-Regel — `PruneOutputDlls` arbeitet auf `ReferenceCopyLocalPaths` und erfasst nichts davon — noch das `OutputPath` der Projektdatei: `publish.yaml:42` baut mit `--output .\build`, wodurch die Ausgaben aller beteiligten Projekte in einem Verzeichnis landen, das DalamudPackager packt. `GeneratePackageOnBuild` bedient dabei bewusst die Autoren abgeleiteter Rotationen.

**Kosten:** 5,35 MB Download statt rund 1,8 MB, funktional folgenlos.

**Auflösungsbedingung:** Der naheliegende Weg trägt nicht — der Standard-Target von DalamudPackager reicht `Exclude` nicht durch und läuft nur, solange keine eigene `DalamudPackager.targets` im Projektverzeichnis liegt; diese müsste den vollständigen Task-Aufruf samt aller Manifest-Felder nachbauen. `Exclude` vergleicht exakt über `List.Contains` (`DalamudPackager.cs:187`), kennt also keine Muster, und das NuGet-Paket trägt die Version im Dateinamen. Der Build-Workflow kompiliert nur und prüft das Paket nicht. Aufgreifen erst, wenn der Veröffentlichungspfad prüfbar ist. Geprüft: die XML-Dokumentation wird zur Laufzeit nicht gelesen.

### VPR: leerer Zweig einer Struktur, die anderswo eine Entscheidung trägt · U

`VPR_Reborn.cs:591-597` und `975-981`. Das Muster `!HasHunterAndSwift` kommt viermal vor; der Vorspann `!IsHunter && !IsSwift` trägt nur an der Coil-Stelle (751-807) Inhalt, an der Den-Stelle (424-493) fehlt er ganz. Weder ein fehlender Inhalt noch dessen Entbehrlichkeit ist belegbar. **Bewusst nicht gelöscht** (AUDIT_LOG A11): Die Entfernung wäre verhaltensneutral, würde aber die Asymmetrie verdecken, die den Befund sichtbar macht. **Auflösung:** Adressat ist der Upstream.

### `AutodutyUpdateState` dupliziert `UpdateState` · U

`RSCommands_StateSpecialCommand.cs`: rund 100 wortgleiche Zeilen, abweichend nur die Fälle `TargetOnly` und `AutoDuty` (`TargetingTypeOverride = targetingType` statt `null`) und der Zustandstext. **Kosten:** jede künftige Änderung am Zustandsautomaten muss an zwei Stellen erfolgen. **Auflösung:** über einen optionalen `TargetingType?`-Parameter zusammenführen, sobald an dieser Stelle ohnehin gearbeitet wird.

### Zwei entfernte öffentliche Member seit dem letzten Release · R

`scan7.py` misst die Paketoberfläche von `RotationSolver.Basic` gegen den Tag `7.5.5.41+wsh1`: `CustomRotation_BasicInfo.HasHostileCountAoeMitigation` (`public virtual`) und `ActionConfig.ShouldCheckTargetStatus` sind seither ersatzlos entfallen. Beide Entfernungen sind sachlich belegt — das Flag öffnete die gesamte Defensivkette, die Option las niemand —, aber sie waren im ausgelieferten Paket enthalten.

**Kosten:** Eine abgeleitete Rotation, die das Flag überschreibt oder die Option liest, kompiliert gegen die nächste Paketversion nicht mehr. Für die gespeicherte Nutzerkonfiguration folgenlos, weil Dalamud fehlende Member beim Deserialisieren überspringt.

**Auflösungsbedingung:** Der Ausweis erfolgt in `CHANGELOG.md`, weil die Versionsnummer ihn nicht tragen kann: sie folgt der Upstream-Version, und das Fork-Suffix ist SemVer-Build-Metadatum ohne Präzedenz. Offen bleibt allein, den Abschnitt „Unreleased" bei der nächsten Versionsvergabe auf die dann vergebene Version zu setzen. Der Zeitpunkt der Veröffentlichung liegt beim Auftraggeber.

### Übertragung der Mitigations-Synergie auf weitere Doppelnutzen-Aktionen · N

Schritt 3 aus `docs/rotation-flow/08-mitigation-synergy.md`. Die Schritte 1 und 2 — Messung und Aussetzbedingung für Sanctus — sind umgesetzt (AUDIT_LOG A20). Offen ist die Übertragung auf weitere Aktionen, die Schaden erzeugen und zugleich Schaden vermeiden.

**Zur Führung dieses Punktes:** Die Einzelheiten stehen im Konzeptdokument, nicht hier. `TODO.md` führt die offene Arbeit und verweist; eine zweite Beschreibung derselben Sache würde mit der ersten auseinanderlaufen. Was hier stehen muss, ist allein, dass noch etwas offen ist und wo es beschrieben wird.

**Auflösungsbedingung:** erst nach Beobachtung der Schritte 1 und 2 im Spiel. Kandidatensuche über ein Prüfskript, nicht über Erinnerung.

### Keine Messgrundlage für Schadens- und Heilungsraten auf Gruppenmitglieder · N

Jede Entscheidung der Form „reicht das, was gerade passiert, bis zum Ablauf einer Frist" braucht eine Rate. Für Gruppenmitglieder gibt es sie nicht, obwohl die Datenquellen anliegen:

| Größe | Lage | Beleg |
|---|---|---|
| Gesundheitshistorie | nur Gegner | `DataCenter.RecordedHP` (`DataCenter.cs:197`) wird in `TargetUpdater.cs:513-535` ausschließlich aus `AllHostileTargets` gefüllt |
| Zeit bis zum Tod | auf Gruppenmitglieder nicht anwendbar | `GetTTK` (`ObjectHelper.cs:3468`) liest genau diese Historie; für eine Party-Id bleibt `startTime` auf `DateTime.MinValue`, Rückgabe `NaN` |
| Abtastrate | 1 Hz | `TimeToKillUpdateInterval` (`TargetUpdater.cs:19`) |
| Eingehende Heilung | nicht ausgewertet | `Watcher.cs:17-18` hängt zwei Handler an `ActionEffect.ActionEffectEvent`, gefiltert auf Quelle = Gegner beziehungsweise Quelle = Spieler. Ein Paket eines fremden Heilers auf ein Gruppenmitglied passiert beide Filter |

**Das ist kein fehlendes Datum, sondern ein fehlender Aufnehmer.** Die Ereignisse liegen an, sie werden für diese Objektmenge nur nicht gelesen.

**Kosten:** Ohne diese Grundlage bleibt jede Regel über Living Dead, Walking Dead oder eine Tankbuster-Sequenz eine Statusabfrage — sie kann „liegt der Effekt" beantworten, aber nicht „reicht die Zeit". Dieselbe Lücke trifft künftige Regeln derselben Bauart.

**Zwei Genauigkeitsgrenzen, die beim Bau zu beachten sind:** 1 Hz ist für ein Zehn-Sekunden-Fenster zu grob (zehn Stützstellen, Entscheidung in der letzten Sekunde), weshalb ein eigener kurzer Ringpuffer mit feinerem Takt der richtige Weg ist statt der Erweiterung von `RecordedHP`. Und ein Gesundheitsdelta ist ein **Surrogat** für kumulierte Heilung: fallen Heilung und Schaden in dasselbe Intervall, heben sie sich auf, obwohl die Heilung zählt. Die Effektpakete zu lesen misst die Größe selbst.

**Auflösungsbedingung:** Der Aufwand fällt bei allen Nutzern an, der Nutzen zunächst nur beim Dunkelritter. Freigabe und Umfang sind deshalb gemeinsam mit Schritt 3 aus `docs/rotation-flow/09-tank-selfprotection.md` zu entscheiden, wo Konstruktion und Verwendung beschrieben sind. Sinnvoll ist, den Aufnehmer nur zu betreiben, solange ihn jemand liest.

### Drei Prüfskripte ohne Selbsttest · —

`.github/scripts/audit/scan.py`, `mitscan.py` und `scan2.py` haben keinen Selbsttest gegen konstruierte Defekte, `scan3.py` bis `scan8.py` schon — dort deckte er bisher vier Erkennungsfehler auf, die sonst als sauberer Baum durchgegangen wären, zuletzt zwei in `scan6.py` (AUDIT_LOG A16). **Kosten:** ein Nullbefund dieser drei ist nicht belastbar. Derzeit liefern alle drei Treffer, die Lücke hat also nichts verdeckt. **Auflösung:** vor dem zweiten Audit-Durchgang nachrüsten.

## Offene Arbeit

### Audit + Code-Review der gesamten Codebasis

Umfang: `RotationSolver.Basic` (48k Zeilen) · RebornRotations (21k) · ExtraRotations (15k) · Updaters (4k) · UI (11k) · Commands/IPC/Data (3k). Der ganze Baum, Upstream-Code eingeschlossen. Phasen 1 bis 4 sind abgeschlossen (AUDIT_LOG A8, A10).

- **Kern tief lesen:** Rest von `DataCenter`; `StateUpdater`, `TargetUpdater`, `ActionTargetInfo`, `BaseAction`/`ActionBasicInfo`, `CustomRotation_Ability`/`GCD`, `Watcher`, `MajorUpdater`, `ObjectHelper`/`StatusHelper` sind gelesen.
- **Rotationen je Job:** Dispatch-Reihenfolge, Gates, Status-IDs, Zielwahl; bisher nur über die Scanner abgedeckt, nicht Datei für Datei.
- **`RotationSolver/UI`** jenseits der Paar- und Totcode-Scans.
- **Zweiter Durchgang** mit den Skripten aus `.github/scripts/audit/` über den bereinigten Baum, nach dem Nachrüsten der fehlenden Selbsttests.
- **Dokumentation** in `docs/rotation-flow/07-codebase-audit.md`.

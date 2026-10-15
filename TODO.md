# TODO — nur offene Arbeit

Getrennt nach Defekt (Abweichung vom beabsichtigten Verhalten), technischer Schuld (bewusst eingegangener Kompromiss mit Auflösungsbedingung) und offener Arbeit. Je Eintrag der betroffene Personenkreis: **N** Endnutzer des Plugins · **R** Autoren abgeleiteter Rotationen, die gegen `RotationSolver.Basic` kompilieren (bis 7.5.6.13 als Paket, seither als DLL aus `latest.zip`) · **U** Upstream-Pflege.

## Defekte

### Sprungprüfung fester Sprünge: Signatur passt nicht zu BossModReborn · N, U

`BMRTimeline_IPCSubscriber.IsFixedDashSafe` abonniert `Hints.IsFixedDashSafe` als `Func<Vector3, Vector3, bool>`
(Upstream, 33e6acb1f, 07.05.2026); BossModReborn registriert den Endpunkt als `(float range, bool backwards)`
(b6698a19f, 05.05.2026; unverändert in be8ff77). EzIPC mit `SafeWrapper.AnyException` gibt bei einer Ausnahme den
Vorgabewert zurück, also `false` (ECommons `SafeWrapperAny`), und Dalamud wirft: `CallGateChannel.CheckAndConvertArgs`
versucht einen `Vector3` per JSON in `float` und `bool` zu wandeln und meldet `IpcTypeMismatchError` (Dalamud 896d831,
gelesen 13.10.2026). Bei laufendem BossModReborn gilt damit jeder feste Sprung und jeder Rückwärtssprung als
gefährlich: `CheckMovementSafety` lehnt Displacement (Rotmagier) und Hissatsu: Yaten (Samurai) ab, seit 17aba32af
(29.09.2026) über die Rückwärtssprung-Prüfung, dazu Repelling Shot und die übrigen `FixedDistanceMove`-Aktionen. Im
Kampf vermutlich: der Rotmagier nimmt Engagement statt Displacement (gefolgert, nicht am Code verfolgt). Offen: die
Abbildung von Start und Ziel auf Weite und Richtung — BossModReborn rechnet das Ziel aus der aktuellen Blickrichtung,
RSR den Rückwärtssprung vom Ziel weg.

### Gelehrter: Recitation ohne Rückfall vor Ablauf · N

`ShouldUseRecitation` drückt Recitation aus vier Anlässen, jeder mit seinem Verbraucher: ein Tank unter
`ExcogHeal` in Gefahr → Excogitation als Heilung; die Einzelabwehr-Flagge → Excogitation im Abwehrpfad (Upstream
7.5.7.1, A278); mittlere Gruppengesundheit unter `ReciteIndomitability` → Indomitability im Flächenheilpfad; ein
Adloquium oder Succor als nächster GCD → dieser GCD. Auslöser und Verbraucher fragen seit A280 dasselbe. Offen
bleibt, was geschieht, wenn der Anlass zwischen Drücken und Verbrauch wegfällt (ein anderer heilt den Tank, die
Flagge fällt) oder Excogitation sein Heilziel nicht beim Tank findet (die Heilzielwahl nach Gefahr nimmt einen
gefährdeteren Schadensausteiler): Dann nimmt kein Verbraucher Recitation, und es verfällt nach 15 s. Zu prüfen
(Konzept 14, „Rückfälle vor Ablauf, je Job"), ob eine andere Heilung, die Recitation trägt (Indomitability,
Adloquium, Concitation), das Fenster in der letzten Gelegenheit nutzen soll. Im Kampf: höchstens ein
verschenkter kritischer Heilschub je Minute.

**Konzept:** `docs/rotation-flow/14-action-dependency-matrix.md`

### Weißmagier: bereite Abwehr bleibt aus, Grund unbekannt · N

Protokoll vom 14.10.2026 (Build 43488aa24, A284): In vier Flächenlagen war Plenary Indulgence nach den Zeilen
„used" seit über 60 s bereit und wurde nicht gewählt; um 10:42:33 ging Temperance allein hinaus, obwohl Plenary
davor steht. Liturgy of the Bell kam in der ganzen Sitzung nie, auch in Lagen, in denen Plenary und Temperance
abklangen und die Stufe für Temperance reichte. In vier Einzelabwehrlagen auf Tankbuster gegen den Tank blieben
Divine Benison und Aquaveil aus, obwohl bereit. Statisch ausgeschlossen: Abklingzeit, Stufe, die Streckung, die
Zielwahl `BeAttacked` ohne Einzelabwehr-Flagge, die Freund-Einstufung (Liturgy über die überschriebene
Effektweite). Nicht messbar von hier: seine Einstellungen je Aktion. *Messmittel:* `DefenseTrace.log` schreibt
jetzt zu jeder Abwehrentscheidung die abgelehnten Aktionen mit dem Grund aus `CanUse` und die stehenden
Rückhaltungen; das nächste Protokoll eines Weißmagiers nennt die Ursache.

### Rückstoß- und Tankbuster-Casts unsichtbarer Helfer werden nicht gelesen · N

Seit A277 liest die Flächenerkennung Casts nicht anvisierbarer Kämpfer, die eine Aktion der Flächenliste wirken
(`DataCenter.AreaCasters`). Dieselbe Lücke haben die Rückstoßerkennung (`AreHostilesCastingKnockback`) und die
Tankbuster-Erkennung (`IsAnyHostileCastingTank`, `IsAnyHostileCastingListedTankBusterAtMe`): Sie gehen über
`AllHostileTargets`, also nur über anvisierbare Gegner. Der Effekt-Handler nimmt Rückstöße von Helfern seit A277
in die Rückstoßliste auf, gelesen werden sie nicht. Nicht mitgebaut, weil beide Regeln eigene Entscheidungen
tragen (Abtausch und Rückstoßschutz in Konzept 08, der Tankbuster-Plan in Konzept 09) und ein Rückstoß, den die
Mechanik braucht, nicht abgewehrt werden darf; die Erweiterung braucht je Regel einen eigenen Durchgang.

**Konzept:** `docs/rotation-flow/13-aoe-damage-classification.md`, „Casts unsichtbarer Helfer zählen wie die sichtbarer Gegner"

### BossModReborn: Ein FATE-Modul in Occult Crescent läuft nach seinem Weggang weiter · N

Am Quelltext gefolgert (BossModReborn be8ff77, 13.10.2026), im Spiel nicht beobachtet. `OpenWorldFate` will sich
zurücksetzen, sobald der Spieler 40 y von der Arenamitte weg ist (`CheckReset`). Die Mitte setzt aber
`SimpleBossModule.UpdateModule` in jedem Durchlauf auf die Position des Spielers, also greift die Entfernung nie.
In Inhalten mit Inhaltsnummer, Occult Crescent eingeschlossen, lädt `BossModuleManager` ein aktives Modul auch
nicht nach Entfernung ab (seit be21ea6a2, 01.10.2026). Das Modul eines FATE, an dem er teilnahm, bleibt deshalb aktiv,
bis dessen Boss den Kampf verlässt. Ein später aktives Modul löst es nicht ab, weil die Ablösung eine höhere
Priorität verlangt und beide aktiv sind. Im Kampf: RSR liest die Vorhersagen des verlassenen FATE (ein Raidwide-Cast
nennt jedes Gruppenmitglied als getroffen), und für das Critical Engagement, in das er weitergeht, kommt keine. Über
die Schnittstelle nicht erkennbar: Die Arena des FATE-Moduls wandert mit dem Spieler, und BossModReborn nennt den
Namen des Bosses, nicht die Figur. Adressat ist BossModReborn.

**Konzept:** `docs/rotation-flow/08-mitigation-synergy.md`, „Wann eine BossModReborn-Vorhersage ihn betrifft"

### ECommons liest die Aktionsart eines Effektsatzes vier Bytes breit · N, U

`EffectHeader.ActionType` (ECommons) nutzt das `uint`-Enum der ClientStructs für ein Ein-Byte-Feld und liest `Flags` und `NumTargets` mit (A177). RSR liest in `Watcher` jetzt das niedrige Byte. Offen: ECommons' eigenes `ActionEffectSet` wählt mit diesem Feld, ob die Id eine Aktion, ein Gegenstand oder ein Reittier ist; sobald der Satz Ziele hat, fällt jeder Gegenstand in den Zweig „Aktion" — `set.Action`, Name und Symbol eines Heiltranks sind dann die einer Aktion gleicher Nummer. Adressat ist ECommons; im Fork betroffen, was `set.Action` für Gegenstände liest (`Watcher.ActionFromSelf`, Aktions-Zeitleiste).

### Samurai nutzt Feint und Tengentsu im Kampf nicht (fremde Meldung) · N

**Konzept:** `docs/rotation-flow/13-aoe-damage-classification.md`
Gemeldet unter PR #11 (05.10.2026, ein Nutzer des Releases 7.5.6.13+wsh1, von ihm beantwortet): Seit 7.5.6.13+wsh1
setze der Samurai im Kampf weder Feint noch Tengentsu ein. Unbestätigt; kein Protokoll. *Statisch geprüft (A265):*
Zwischen 7.5.6.10+wsh1 und 7.5.6.13+wsh1 kam für den Samurai genau ein neues Gatter dazu: Tengentsu und Third Eye
im Flächenpfad nur, wenn der Treffer ihn selbst erreicht (A218, beabsichtigt; seit A233 zentral geprüft, seit A271
nicht mehr zusätzlich in der Rotation). Feint hat im Flächen-
und Einzelpfad kein neues Gatter; die Flächenflagge wurde im selben Fenster erweitert (Gruppe statt Spieler, A218),
nicht verengt. Die Hypothese, die Einzelziel-Regel (eb3fce2eb) nehme Raidwides die Flagge, ist an den Spieldaten
widerlegt: Von 850 Listeneinträgen ist einer ein Einzelziel mit Wirkreichweite (Holy Bladedance, ein Tankbuster).
Unverändert seit früher: beide nur ohne Zanshin Ready (`!HasZanshinReady`, Upstream), und die Bewertung „lohnt
Minderung" (`Skip mitigation for small area casts`). *Messmittel:* `DefenseTrace.log` schreibt jetzt auch „area
defence open, nothing chosen" mit den Quellen; zusammen mit „hit you" trennt eine Aufzeichnung eines Samurai-Kampfs
„Flagge nie offen" von „offen, alles abgelehnt". Auflösung: ein solches Protokoll.

### Tänzer: Improvised Finish — Stapel aufbauen bei angekündigtem Treffer · N

Seit A202 wird Improvised Finish sofort gewirkt (5 %). Offen als Verbesserung: bei angekündigtem Flächentreffer Rising Rhythm aufbauen (bis 4 Stapel, 10 %), je Stapel drei Sekunden ohne Aktion. Meine offene Arbeit, keine Frage an ihn: Der Loop muss rechnen, ob die zusätzliche Barriere einen Tod verhindert, den die 5 % nicht verhindern (gemessener Treffer gegen vorausberechnete Gesundheit, dieselbe Rechnung wie die Schranke), und ob die Zeit bis zum Treffer überhaupt einen Stapel zulässt; erst danach steht fest, ob die Regel ohne Option gebaut wird oder entfällt. Konzept 14, „Wechselwirkungen und Zeit".

### Krieger: Nascent Flash — offene Bausteine · N, U

Nach seinen Kriterien (29.09.2026) bewertet und gebaut (Konzept 09, „Krieger: Urimpuls für sich oder Urflackern für andere"; A226). Offen:
- *Offen, jetzt mit Quelle:* Nascent Flash als Minderung für den anderen Tank vor dessen Tankbuster. The Balance (Warrior Basic Guide, Makros, 02.10.2026): „Most often on the co-tank of your eight-person party.“ Braucht die Erkennung des Tankbusters auf dem anderen Tank und die Abwägung über die vorhandene Triage.
- *Upstream-Bauform:* Raw Intuition/Bloodwhetting fällt nur, solange das Ziel des Kriegers ihn anvisiert (`PlayerIsTargetOnSelf`). Ein Off-Tank mit Tankbuster-Marker bekommt es aus der Einzelabwehr nicht.

### Rotationen gegen The Balance abgleichen · N

Seit 27.09.2026 ist die Referenz erreichbar. Offen: je Job die Standardrotation (Opener, Burstausrichtung, Prioritäten) gegen den Guide prüfen — „werden die optimalen Kombinationen genutzt" über die Fensterprüfung hinaus (Konzept 14). Dazu: die von `ActionId.resx` ausgeblendeten Werte (Dauern, Potenzen, Statusnamen) aus dem Job-Guide in den Generator übernehmen, und die deutschen Namen aus dem deutschen Job-Guide in `action_names_de.json`.

### `NextBigSummonIsBurst`: die Geschichte kann das Urteil des Spiels überstimmen · N

Die Eigenschaft antwortet „ja", wenn das Spiel Solar als nächste Demi anzeigt **oder** die letzte Demi nicht Solar war. Zeigt das Spiel Bahamut an, während die eigene Geschichte etwas anderes sagt — etwa nach einem Tod, wenn das Spiel die Reihenfolge zurücksetzt —, gewinnt die Geschichte. Im Kampf hieße das: Searing Light fiele vor Bahamut statt vor Solar. Nicht belegt ist, wann das Spiel die Demi-Reihenfolge zurücksetzt und wann die umgestellte Id nicht lesbar ist; beides entscheidet, ob die Geschichte nur Rückfall sein darf.

### Tank-Unverwundbarkeit vor Tankbustern ohne Aktionsangabe oder ohne Messung weiter nur reaktiv · N

**Stand (A260):** Vor einem gewirkten Tankbuster, den die Tankbuster-Tabelle selbst bei vollen LP und mit allen
eigenen Minderungen für tödlich hält, geht die Unverwundbarkeit vorher aus, und die übrige Abwehr auf den Tank wird
zurückgehalten (Konzept 09, „Gemessener Tankbuster: das geringste Mittel"). Weiter nur reaktiv, also erst unter
`HealthForDyingTanks`, bleibt sie
- bei Tankbustern, die nur ein Marker oder BossModReborn ankündigt: Beide nennen die Aktion nicht, also gibt es
  keinen Tabellenwert;
- beim ersten Auftreten einer Aktion, solange kein Spieler davon getroffen wurde.

**Offen:** Ob sich ein Marker einer Aktion zuordnen lässt – etwa über das Wirken, das während des Markers beginnt,
wie es `TankbusterMarkerWatch` für die Bestätigung schon tut –, ist ungeprüft. Voller Loop vor jedem Bau.
Ebenfalls offen: Stärkungen des Gegners, die nicht als Damage Up benannt sind, werden beim Messen nicht erkannt;
wie oft es sie gibt, ist nicht erhoben. Umgekehrt bleibt ein Gegner, der den ganzen Kampf ein Damage Up trägt, ohne
Messung, und eine Vorhersage unter Damage Up unterschätzt (sichere Richtung; die Wirktexte nennen keine Stärke, mit
der sich hochrechnen ließe).

**Konzept:** `docs/rotation-flow/09-tank-selfprotection.md`

### Wirkdauer unterhalb der Merkmalsstufe unbekannt (Reflexion, Schiltron, Urflackern, Intervention) · N, R

**Befund (A266):** Wo der Wirktext die Dauer leer lässt („Duration: s"), liest der Plan sie aus dem Merkmal, das sie
setzt – im Spiel (`TraitDurations`), sonst aus `trait_durations.json`. Unterhalb der Merkmalsstufe (Reflexion,
Feint, Addle 98; Schiltron 74; Urflackern, Intervention 82) nennt kein Text die Dauer; die Aktion ist dann kein
Kandidat. Im Kampf, stufensynchron darunter: Der Plan wählt ein teureres Bündel oder die Unverwundbarkeit (Richtung:
zu viel Mittel, nicht zu wenig). **Offen:** ob die Grunddauer irgendwo im Spiel steht (Status-Blatt, Job-Guide), voller
Loop vor jedem Bau.

**Konzept:** `docs/rotation-flow/09-tank-selfprotection.md`

### Tankbuster-Plan nimmt Reflexion, die für einen angesagten Raidwide gehalten würde · N

**Befund (12.10.2026, beim Umbau der Konzepte):** Die Einzelabwehr hält Reflexion am Tankbuster zurück, wenn der Tank
selbst gedeckt ist und ein Raidwide nach ihrem Ende und vor ihrer neuen Bereitschaft angesagt ist
(`HoldReprisalForRaidwide`, A244, Konzept 08, „Gegner-Debuffs am Tankbuster und am Raidwide"). Der Tankbuster-Plan
drückt über `TankbusterPlanAbility` und kennt diese Regel nicht; er wählt das Bündel mit der geringsten Abklingzeit,
und Reflexion (60 s) ist oft das billigste Glied. *Im Kampf:* Vor einem gemessenen Tankbuster geht Reflexion auf den
Tank, obwohl Schutzwall oder die große Minderung ihn getragen hätte; am folgenden Raidwide fehlen jedem Mitglied 10 %.
Gilt für alle vier Tanks. **Offen:** voller Loop – etwa Reflexion wie ein Glied, das eine andere Regel braucht, nur
nehmen, wenn kein Bündel ohne sie reicht (dieselbe Form wie K2–K4).

**Konzept:** `docs/rotation-flow/09-tank-selfprotection.md`

### Tankbuster-Plan: Stun sperrt ein Glied einen GCD; Druck nach dem Snapshot möglich · N

**Konzept (geplant, 12.10.2026):** `docs/rotation-flow/09-tank-selfprotection.md`, „Spielerweite Sperre und Frist".
- *Defekt 1:* Eine Ablehnung unter Stun oder Amnesie sperrt eine Minderung einen GCD statt bis zum Ende der Sperre.
- *Defekt 2:* Der Plan drückt bis zum Ende des Wirkbalkens. Ein Druck, der nach dem Snapshot ankommt, ist verloren –
  seine Hinweise: „unverwundbarkeit nach tod bringt nichts, "alles" nach schadenseingang bringt auch nix".
- *Im Kampf:* Nach einem kurzen Stun gehen die Glieder bis zu einen GCD zu spät; der Plan kann dann zu Unrecht die
  Unverwundbarkeit oder „alles" ziehen. Ein später Druck kostet eine Abklingzeit, ohne den Treffer zu ändern.
- *Vor dem Code offen:*
  - Ob die Totenerweckung, Heiliger Boden und Meteoritenfall am Snapshot oder beim Zufügen zählen.
  - Ob die Zeitangabe von BossModReborn den Snapshot oder das Zufügen meint (für die Klasse über den Plan hinaus).
  - Die Modellerweiterung O9/O10.

### Tankbuster-Plan: der GCD als Ersatz für Zeiten, die das Spiel anders bestimmt · N

**Befund (A269):** Der Plan setzt an mehreren Stellen einen GCD ein, ohne dass die Zeit am Spiel geprüft wurde. Gemeint
sind je verschiedene Größen:
- *Horizont H = R + GCD* („Eintreffen des Treffers"): Der Snapshot liegt am Ende des Wirkbalkens (Community-Beleg),
  für Holmgang zählt das Zufügen. Eine Minderung muss also bis zum Snapshot stehen, nicht einen GCD länger.
  - Daran hängen: Fenster („Wirkdauer weniger ein GCD"), Invariante 5 (Wirkdauer über zwei GCD) und die
    Erneuerungsschwelle („mehr als Wirkdauer weniger ein GCD übrig").
- *Letzter GCD als Reserve für den Rückfall* (Z2: „im letzten GCD nichts, wenn ein Glied fehlt"; Zurückhalten bis
  R > GCD): Gebraucht werden Druckplätze bis zur Frist, also Animationssperre je Druck und Laufzeit (Konzept 09,
  „Spielerweite Sperre und Frist", Regel 5).
- *Bestätigung verfällt nach einem GCD:* Gemeint ist die Laufzeit von Druck bis Bestätigung, die gemessen werden kann.
- *Verweigerung sperrt einen GCD:* Gemeint ist das Ende der Ursache, unter Stun deren Restzeit (eigener Eintrag oben).

Ausgenommen ist die Living-Dead-Uhr (zwei GCD Vorlauf): Dort ist der GCD die Zeit, die eine Heilung braucht, also die
gemeinte Größe.
- *Im Kampf:* Fenster öffnen bis zu einen GCD zu spät, und Glieder mit kurzer Wirkdauer fallen ganz heraus – zu viel
  Mittel. Der Rückfall „alles" beginnt einen GCD vor dem Treffer statt bei den letzten nötigen Druckplätzen.
- *Offen:* ein Loop über alle Stellen zusammen, mit Modell. Erst Konzept, dann Code.

### Living Dead: Die Rückhaltung liest die gemessene Tankbuster-Höhe nicht · N

**Befund (12.10.2026, Konzept 09, „Warum eine Überlebensprüfung diese Zusage nicht tragen kann"):** Die Uhrregel hält
Heilung unter Living Dead zurück, solange der Tod noch rechtzeitig kommt, gemessen an `HealthForDyingTanks` und der
Restzeit. Für einen gewirkten, gemessenen Tankbuster kennt die Tankbuster-Tabelle inzwischen den Höchstwert; ob der
Treffer den Träger vor Ablauf auf 0 bringt, wäre damit rechenbar. *Im Kampf:* Heute kann die Regel einen Träger
zurückhalten, den der kommende Tankbuster nicht auf 0 bringt, oder ihn vor dem Tankbuster heilen, der ihn sonst in
Walking Dead brächte. **Offen:** voller Loop, ob die Regel den Wert lesen soll.

**Konzept:** `docs/rotation-flow/09-tank-selfprotection.md`

### Tankbuster-Plan: keine Reserve nach einem knapp überlebten Treffer · N

**Befund (A261):** Der Plan rechnet bis zum Einschlag: Er wählt das billigste Bündel, mit dem der gemessene
Tankbuster weniger nimmt, als der Tank hat. Ein Treffer, der ihn knapp über 0 lässt, gilt als überlebt. Danach
enden Plan und Zurückhalten, die Minderungen sind wieder frei, und die reaktive Unverwundbarkeit und die Heiler
übernehmen – aber ein Auto-Angriff unmittelbar danach trifft einen Tank fast ohne LP. **Offen:** voller Loop, ob die
Rechnung eine Reserve verlangen soll und woraus sie sich ableitet (gemessener Schadenseingang des Tanks über einen
GCD, Konzept 08), ohne feste Zahl. **Im Kampf:** wie oft ein Plan so knapp ausgeht, schreibt das Protokoll mit
(„leaving …% against …%").

**Konzept:** `docs/rotation-flow/09-tank-selfprotection.md`

### Zielüberschreibung aus dem GCD-Lauf gilt in den Fähigkeiten weiter · N, R

**Befund (A260, am Code):** `CustomRotation.Invoke` setzt `IBaseAction.TargetOverride` vor `GCD()` auf null, aber
nicht vor `Ability()`. Kehrt `GCD()` mitten aus einem Pfad zurück, der die Überschreibung gesetzt hat (Heilung,
Abwehr `BeAttacked`, Wiederbelebung `Death`, `Provoke`), laufen Befehlsaktion, `EmergencyAbility` und alle
Fähigkeiten bis zur nächsten Zuweisung mit dieser Überschreibung (`ActionSetting.TargetType` liest sie vor dem
eigenen Typ). Eine Notfallaktion ohne eigene `targetOverride` sucht ihr Ziel dann nach fremder Art.
`TankInvulnerabilityAbility` setzt sie deshalb für ihren Aufruf zurück.

**Im Kampf:** unbelegt, ob es eine Aktion trifft. Fehlbild wäre eine Notfallfähigkeit, die kein Ziel findet oder
auf das falsche geht. **Offen:** alle `EmergencyAbility`-Überschreibungen auf Aktionen ohne eigene Zielart prüfen;
ob eine Rotation sich auf die geerbte Überschreibung verlässt, vor einem zentralen Zurücksetzen erheben.

### Tankwechsel von der anderen Seite: Herausforderung, wenn der Haupttank eine Verwundbarkeit trägt · N

**Meine Ableitung, nicht seine Vorgabe (A255, A256):** Geteiltes Leid bewegt den Gegner nur, wenn der Co-Tank schon
über der Hälfte der Feindseligkeit liegt. Herausforderung des Co-Tanks wirkt immer. Teilweise besteht diese Seite
schon: Fällt der Haupttank unter `HealthForDyingTanks`, provoziert RSR (`ObjectHelper.CanProvoke`, seit A256 nicht
mehr, solange der Spieler selbst in Gefahr ist). Fehlt der Fall, dass der Haupttank nach einem Tankbuster eine
Verwundbarkeit trägt oder eine Wiederholung nicht überlebt, ohne tief zu stehen. Dafür braucht es dieselbe Erkennung für ein anderes Gruppenmitglied
(Effektsatz auf ihn, nicht auf dich). **Offen:** voller Loop, dann ihm vorlegen – die Lehre aus `451d9e90` (der
Co-Tank-Provoke zog den Boss von einem Tank unter Unverwundbarkeit) gehört in die Bedingungen.

**Konzept:** `docs/rotation-flow/09-tank-selfprotection.md`

### Astrologe: Synastry wählt ihr Ziel nach Trefferfläche und aktueller Gesundheit, nicht nach der Heilung · N, U

`AST_Reborn.CanCastSynastry` verlangt, dass Synastry und die folgende Einzelheilung (Aspected Benefic, Benefic II, Benefic) dasselbe Ziel haben. Laut Wirktext zählt jede Einzelheilung, „on yourself or a party member", die Gleichheit ist also kein Fehler. Synastry läuft aber in `EmergencyAbility` vor jedem Target-Override (Notfallslot in `CustomRotation.Ability`) und setzt keinen `TargetType`. Gewählt wird deshalb nach `Big`: größte Trefferfläche, bei Gleichstand, und Spieler haben gleiche Trefferflächen, das Mitglied mit der **meisten** aktuellen Gesundheit (`BigHp` aus, ab Werk; seit A225 liest der Zweig diese Einstellung). Die Heilung dagegen geht an den Bedürftigsten. Im Kampf (Schluss aus dem Code, nicht beobachtet): Synastry fällt nur, wenn der Geheilte zugleich der Gesündeste ist. Das trifft vor allem dann nicht zu, wenn der Tank unter die volle Gesundheit eines anderen Mitglieds gefallen ist, also genau im Bedarfsfall. Upstream-Bauform, gefunden in der Tiefenprüfung A221. Zu bearbeiten im vollen Loop: Zielwahl über das Heilziel (Override) oder Prüfung gegen das Ziel der nächsten GCD.

### Die proaktive Minderung aller Jobs hat BossModReborn als einzige Quelle · N, R

**Konzept:** `docs/rotation-flow/08-mitigation-synergy.md`

**Vorgabe des Auftraggebers:** „bossmod liefert nicht für jeden boss werte, sondern nur für unterstützte module. und da ist der abdeckungsgrad in bossmod auch unterschiedlich. sich auf bossmod zu 100% zu verlassen ist fahrlässig."

**Erhoben:** Jede Stelle, die **vor** dem Einschlag mindert, liest `BMRShouldRefreshBefore` — alle vier Tanks (Nebula/Guardian/Damnation/Shadowed Vigil und Rampart), Barde, Maschinist und Tänzer (Troubadour, Tactician, Shield Samba) und der Beschwörer (Schimmerschild). Fällt BMR aus, fällt die ganze proaktive Ebene aus; es bleibt der reaktive Weg, der erst greift, wenn der Cast läuft und den Vorfilter passiert.

**Zwei stille Ausfallarten:** kein Modul für diesen Kampf, oder ein Modul, das diese Ereignisart nicht führt. Beide kommen als `float.MaxValue` an und lesen sich wie „es kommt nichts". Die Diagnoseseite nennt jetzt Modul und Vorhersagelage je Ereignisart, sodass der Ausfall wenigstens ablesbar ist.

**Der Weg dahin ist gebaut, aber nur an einer Stelle:** `IsHostileCastingLargeArea` nutzt den gemessenen Anteil je Aktion — die einzige BMR-unabhängige Vorhersagequelle im Baum. Dieselbe Quelle könnte die proaktive Schicht der übrigen Jobs tragen.

**Seine Richtung ist benannt:** „daher die eigene liste mit dem aoe-schadensausmaß“ — die Messung ist als Ersatz für die fehlende Verlässlichkeit gedacht, nicht als Zusatz. Der Umbau der proaktiven Schicht auf diese Quelle ist damit keine offene Richtungsfrage mehr, sondern eine Frage von Umfang und Reihenfolge.

**Nicht bearbeitet:** Der Umbau trifft acht Rotationsdateien plus die gemeinsamen Helfer, alle in seinem Profil (alle Kampfjobs). Er braucht einen eigenen Loop über die Frage, welche Vorlaufzeit eine gemessene Größe ohne angekündigten Zeitpunkt trägt; eine Spielbeobachtung als Voraussetzung ist nicht seine Aufgabe.

### Dieselbe Frage steht bei Tankbustern, Rückstoß und Unterbrechung offen · N

**Konzept:** `docs/rotation-flow/08-mitigation-synergy.md`, `docs/rotation-flow/13-aoe-damage-classification.md`
Für Tankbuster ist sie beantwortet: Die Tankbuster-Tabelle misst jeden Treffer ungemindert auf jedem Spieler und trägt den Tankbuster-Plan (A259, Konzept 09). Offen bleiben `HostileCastingKnockback` und `HostileCastingStop` mit derselben Struktur: Der Messpfad im Effekt-Handler ist derselbe; was fehlt, ist je Liste ein eigener Speicher und die passende Rechnung, und je Liste die Frage, was die Größe dort entscheidet (ein Rückstoß schadet oft nicht, er verschiebt).

### DRK: Ob The Blackest Night auf dem Pull bricht, misst niemand · N, U

`BlackestNightUsage` (`DRK_Reborn.cs`) bietet drei Stufen für den Selbstschutz-Zweig: wie bisher; Tankbuster oder großer Pull ohne laufende große Minderung; dasselbe plus Gesundheitsschwelle. Voreingestellt ist seit A227 (29.09.2026) **die dritte** (`TankbusterHeavyPullOrLowHealth`). Zwei Größen der engeren Stufen sind Annahmen: dass vier Gegner die Verbrauchsrate von 3,6 % der maximalen Gesundheit pro Sekunde erreichen, und dass ein Tankbuster die unter Minderung angehobene Schwelle mitnimmt.

**Umgesetzt nach Vorgabe des Auftraggebers, Wall-to-Wall.** Steht The Blackest Night, wirkt der Dunkelritter auf einem Gruppenpull weder Reflexion noch Abtausch, und der Weißmagier betäubt nicht mit Sanctus; im Bosskampf gilt die Regel nicht. `HoldMitigationForBarrier()` sperrt die beiden Reflexion-Zweige und den Abtausch-Zweig auf dem Pull, `ShouldHoldHolyForBarrier()` die Betäubungsseite — jetzt **voreingestellt an** und ebenfalls auf den Gruppenpull begrenzt. Begründung und Abgrenzung stehen in `docs/rotation-flow/10-drk-blackest-night.md`. Prüfgrad: statisch und Compile.

**Die Verlangsamung durch Dritte ist messbar und praktisch vernachlässigbar.** Die Behauptung, sie sei von hier aus nicht zu erfassen, war falsch: Der Debuff steht auf den **Gegnern**, und `PackSlowed` misst ihn bereits genau so — `SurveyHostileStatus(JobRange, SlowStatus, out slowed)`, ohne zu fragen, wer ihn gelegt hat. Ein fremder Abtausch ist damit sichtbar wie der eigene.

Er fällt trotzdem kaum ins Gewicht, und der Grund liegt in der Wirkweise der Aktion: Abtausch verlangsamt nur Gegner, die den **Träger** treffen. Der Tank zieht die Aggro, also schlagen die Gegner ihn und nicht den Schadensausteiler; dessen Abtausch müsste erst Aggro und Treffer abbekommen, bevor überhaupt eine Verlangsamung entsteht. Die Wirkrichtung ist außerdem einseitig: Ein fremder Slow kann das **Zünden** der Barriere verzögern, weil `PackSlowed` es sperrt — die stehende Barriere hat umgekehrt keinen Einfluss darauf, ob der andere Spieler seine Aktion wirkt.

**Empfehlung: nicht bearbeiten.** Eine eigene Regel dafür hätte keinen belegbaren Anwendungsfall.

**Was offen bleibt, ist die Messung.** Ob die Barriere unter den engeren Stufen tatsächlich bricht, beobachtet niemand, und eine Spielbeobachtung ist nicht seine Aufgabe (CLAUDE.md, „Entscheidung zur Laufzeit"): Die Regel müsste ihren Bruch selbst zählen (Dark Arts nach der Barriere) und die Gegnerschwelle nachsteuern. Voller Loop vor dem Bau. Dieselbe Zählung beantwortet zwei Nebenfragen: was die Rückhaltung des Weißmagiers kostet (Sanctus ist sein einziger Flächenzauber, ein zurückgehaltener GCD fällt auf Einzelzielschaden zurück) und ob `BlackestNightMinHostiles` (Vorgabe 4) zu hoch liegt, sodass The Blackest Night im Wall-to-Wall zu selten kommt. Sie ist bewusst eine **eigene** Option der Rotation und nicht der globale `MitigationSustainHostileCount`, weil die Frage hier eine andere ist: nicht „lohnt eine Minderung", sondern „reicht der Schadensstrom, um 25 % der maximalen Gesundheit in sieben Sekunden aufzuzehren".

### `IsValid()` prüft nicht, ob ein Objekt noch lebt · N, U

33 Prüfungen im Plugin (ohne `ExtraRotations`, Stand 15.10.2026) setzen `IsValid()` vor einen nativen Zugriff, damit ein Objekt, das seit dem Aufbau der Liste verschwunden ist, übersprungen wird (Upstream 6baba6a94, Fork A199). Dalamuds `GameObject.IsValid` prüft aber nur `PlayerState.IsLoaded`, also ob der Spieler geladen ist (C111). Dazu kommt, dass Dalamud je Tabellenplatz und Objektart **eine** Instanz wiederverwendet und ihre Adresse bei jeder Aufzählung neu setzt (`ObjectTable.CachedEntry.Update`, für alle Plugins gemeinsam). Eine gespeicherte Instanz zeigt also nach dem Verschwinden ihres Objekts auf freigegebenen Speicher und, sobald der Platz neu belegt und erneut aufgezählt wird, auf ein anderes Objekt.

Betroffen sind Leser, die eine Liste außerhalb des Zyklus lesen, in dem sie gebaut wurde: Fenster, Effekt-Hooks und mit `MinUpdatingTime` > 0 übersprungene Zyklen (A199). Im Kampf hieße das: ein Zyklus mit Werten eines verschwundenen oder eines anderen Gegners, im ungünstigsten Fall ein nativer Absturz bei einer virtuellen Funktion aus freigegebenem Speicher. Beobachtet ist davon nichts. Ob das Spiel Objekte zwischen Dalamuds Framework-Aktualisierung und dem Zeichnen im selben Frame entfernt, ist unbelegt.

Offen: voller Loop. Zu klären sind ein Lebendigkeitsmaß, das ohne Lesen des möglicherweise freigegebenen Objekts auskommt (etwa Tabellenplatz und Adresse gegen `IObjectTable.GetObjectAddress`, dazu die `GameObjectId`), welche der 33 Stellen außerhalb ihres Zyklus lesen und ob Upstream dasselbe Muster pflegt.

## Technische Schuld

Bewusst eingegangene Kompromisse und erfasste Befunde ohne Wirkung im Kampf, je mit Kosten und Auflösungsbedingung.

### Searing-Light-Abdeckungsmodell bildet die Phasenbuchung des Plugins von Hand nach · R

**Konzept:** `docs/rotation-flow/12-searing-light-stacking.md`
Der Modus „plugin" in `searing_light_coverage.py` rechnet die Phasenbuchung von `SummonerRotation.UpdateSearingPhaseBook` nach (gehalten = zwei bewertete Fenster in Folge besetzt, eigener Buff bewertet nicht) — am 14.10.2026 gleich (A286, K5, P1). Die CI hält die Invarianten des Modells, nicht seine Übereinstimmung mit dem Code.

**Kompromiss:** Eine Verknüpfung über Textmuster im C#-Code prüfte Schreibweisen, nicht Verhalten (ein Surrogat); das Modell aus dem Code zu erzeugen, hieße einen C#-Interpreter für einen Teil der Rotation zu bauen.
**Kosten:** Ändert sich die Buchung im Code, liefert das Modell weiter Zahlen für die alte Regel, ohne dass etwas fehlschlägt — und Konzept 12 argumentiert mit ihnen.
**Auflösungsbedingung:** jede Änderung an `UpdateSearingPhaseBook`, `AllSearingPhasesHeld` oder `BurstPhaseHeldByAnother` trägt im selben Commit die Anpassung des Modus „plugin"; die Prüfung dafür ist der Diff dieser drei Stellen beim Review.

### `SwiftcastBuffer` hat keinen Leser, und ihre Absicht ist überholt · N

`Configs.cs`, `SwiftcastBuffer`, definiert die Einstellung (0,6 s, eigene Oberfläche, eigene Dokumentation „how early before next GCD should RSR use swiftcast for raise"). Eine Volltextsuche über den Baum findet genau diese eine Fundstelle: Sie wird nirgends gelesen.

Sie ist nicht nur unverbunden, sondern in ihrer dokumentierten Bedeutung unerfüllbar geworden. Sie besagt, Spontanität solle erst fallen, wenn nur noch `SwiftcastBuffer` Restzeit auf dem GCD liegt — bei 0,6 s liegt dieses Fenster fast vollständig, bei 0 vollständig in dem Bereich, den `RSCommands_Actions.DoAction` für Fähigkeiten sperrt. Sie zu verdrahten hieße, den in `docs/rotation-flow/11-raise-dispatch.md` behobenen Defekt an einer zweiten Stelle neu zu bauen; deshalb ist sie bei der dortigen Behebung bewusst unangetastet geblieben.

**Auflösung:** entweder entfernen — dann ist zu prüfen, ob der Name in gespeicherter Nutzerkonfiguration liegt und ein Migrationspfad nötig ist — oder als **Untergrenze** im Einschiebefenster neu definieren, also „wie weit oberhalb der Sperre darf Spontanität frühestens fallen". Die zweite Lesart erhält die Absicht des Autors und ist mit der Sperre vereinbar. Beides ist eine Entscheidung über Nutzerkonfiguration und gehört nicht in den Behebungsvorgang.

### `InterruptDelay` und `ProvokeDelay` haben keinen Leser · N

`Configs.cs`, `InterruptDelay` und `ProvokeDelay`. Beide sind `Vector2` mit Vorgabe `(0,5 s; 1 s)`, eigener Beschriftung und Wertebereich — und versprechen damit eine Zufallsverzögerung vor dem Unterbrechen beziehungsweise vor Provoke. Gelesen werden sie nirgends.

Die Klasse ist belegt, weil die beiden Geschwister derselben Bauart **gelesen** werden: `RaiseDelay2` und `EsunaDelay` speisen die `ObjectListDelay`-Instanzen in `TargetUpdater` (`_raisePartyTargets` und Geschwister). Für Provoke- und Unterbrechungsziele gibt es keine solche Instanz; `TargetUpdater` setzt `DataCenter.ProvokeTarget` und `DataCenter.InterruptTarget` ohne jede Verzögerung.

**Nicht verdrahtet, und das ist keine offene Entscheidung, sondern das Ergebnis der Regeln.** Drei Gründe, die zusammen nur einen Schluss zulassen:

1. *Es gibt keinen Migrationsweg.* `Configs.Migrate` ist keine Migration, sondern ein Zurücksetzen: bei abweichender Version `return new Configs()`. Eine geänderte Vorgabe erreicht deshalb nur Neuinstallationen; wer das Plugin schon benutzt, hat `(0,5; 1)` in seiner gespeicherten Konfiguration stehen und bekäme die Verzögerung beim Verdrahten tatsächlich eingeschaltet.
2. *Damit verletzt jede Verdrahtung die Feature-Toggle-Regel*, die für eine Änderung ohne Nachweismöglichkeit das bisherige Standardverhalten als Vorgabe verlangt. Das bisherige Verhalten ist „keine Verzögerung", und es lässt sich ohne Migration nicht erhalten.
3. *Der Nutzen liegt außerhalb der Zielrichtung.* Die Verzögerung dient der Tarnung, nicht der Kampfwirkung. Bei der Unterbrechung wirkt sie sogar gegen den Zweck der Aktion: bis zu eine Sekunde kann das Fenster des Zaubers verbrauchen, der gestoppt werden soll.

**Auflösungsbedingung:** Sobald ein echter Migrationsmechanismus existiert — einer, der einzelne Felder umstellt, statt die Datei zu verwerfen —, ist zu verdrahten und die Vorgabe zugleich auf `(0; 0)` zu ziehen. Vorher ist die wirkungslose Einstellung das kleinere Übel gegenüber einer ungefragt eingeschalteten Verzögerung.

### `TargetColor` wird nicht gelesen, und ihr Elternverweis zeigt auf sie selbst · N

`TargetColor` in `Configs.cs` trägt `[UI("Target color", Parent = nameof(TargetColor))]` — die Eigenschaft nennt sich selbst als Elternschalter. Die Zeile darüber, `TeachingModeColor`, zeigt die richtige Bauart mit `Parent = nameof(TeachingMode)`; Kennzeichen eines Klons ohne Anpassung (Parnas, *Ignorant Surgery*).

**Der Elternverweis ist folgenlos, entgegen der ersten Einschätzung.** `SearchableCollection.cs:39` nimmt ausschließlich `CheckBoxSearch` in die Elternliste auf, also boolesche Einstellungen. `TargetColor` ist ein `Vector4`, landet nie darin, der `TryGetValue` schlägt fehl, und der Eintrag wird auf oberster Ebene einsortiert. Kein Absturz, keine Rekursion in `GetParent`, nur eine Einrückung, die fehlt.

**Der wirkliche Befund ist der fehlende Leser:** Die Farbe wird im gesamten Baum nicht gelesen, wirkt also ohnehin nicht.

**Nicht behoben,** weil die Behebung voraussetzt zu wissen, wo die Farbe gezeichnet werden sollte und welcher Schalter der gemeinte Elternteil ist. Beides geht aus dem Code nicht hervor, und eine erfundene Zuordnung wäre schlechter als der sichtbare Rest. Technische Schuld, kein Defekt mit Wirkung.

### `IBaseAction.IgnoreClipping` wird geschrieben und nirgends gelesen · N, R

`IBaseAction.IgnoreClipping` ist das Flag; `CustomRotation_Invoke.cs` setzt es an fünf Stellen (gezählt 12.10.2026). Kein einziger Leser im gesamten Baum. Der Name benennt genau den Mechanismus, der beim Wiederbelebungsdefekt gefehlt hat: die Anti-Clipping-Sperre für einen Einzelfall aufheben.

Nicht behoben, weil der Wirkungsbereich den Vorgang sprengt. Das Flag wird in `Invoke` breit gesetzt, auch für Fälle ohne Bezug zur Wiederbelebung; ein Leser in `RSCommands_Actions.DoAction` würde die Sperre praktisch überall aushebeln und damit das Verhalten jeder Rotation ändern. Die Behebung verlangt zuerst eine Entscheidung, für welche Aktionen das Flag gelten soll.

**Auflösungsbedingung:** eine Erhebung, welche der sechs Setzstellen eine Ausnahme rechtfertigen, und eine Engführung des Flags auf diese.

### Status-Einstellungen auf der falschen Seite der Aktion · N, U

`ActionSetting` liest `StatusProvide` und `StatusNeed` gegen `Player.Object`, `TargetStatusProvide` und `TargetStatusNeed` gegen das Ziel. Eine ID, die der Spieler nie tragen kann, macht die erste Sperre wirkungslos und die zweite dauerhaft blockierend. `scan11.py` erhebt die Klasse; nach der Behebung der beiden Blaumagier-Stellen (A32) bleiben vier Fundstellen, deren richtige Auflösung PvP- beziehungsweise Bozja-Verhalten voraussetzt.

- `RedMageRotation.ModifyEnchantedZwerchhauPvP` und `ModifyEnchantedRedoublementPvP` (PvP): `StatusProvide` trägt 3238 und 3239, die Schadenswirkungen auf dem **Ziel**; die Barrieren auf dem Spieler sind 3235 und 3236, und die Nachbarin `ModifyEnchantedRipostePvP` greift mit 3234 auf diese Hälfte zu. Die Feldwahl ist damit belegbar falsch — welche Behebung richtig ist, aber nicht: Nach dem Blaumagier-Befund ist ebenso denkbar, dass die **greifende** Zeile (Riposte) der Defekt ist, weil eine Barrieren-Sperre die feste Kombo Riposte → Zwerchhau → Redoublement → Scorch unterbrechen kann. Beide Lesarten sind ohne PvP-Beobachtung nicht zu trennen.
- `ScholarRotation.ModifyDeploymentTacticsPvP` (PvP): `StatusProvide` trägt `Biolysis_3089`, wirkungslos. Als `TargetStatusProvide` gelesen widerspräche es dem `TargetStatusNeed` derselben ID zwei Zeilen darüber und machte die Aktion unbenutzbar; ersatzloses Streichen ist daher die wahrscheinliche Auflösung. `SCH_Default.PVP.cs` hat sich mit einer eigenen `!IsLastAction`-Sperre beholfen.
- `BozjaRotation.cs:368` (Lost Paralyze III): `StatusProvide` trägt `Paralysis`, den Ziel-Debuff. Hier ist die Sperre fachlich plausibel — der Zauber hat keinen nennenswerten anderen Zweck und Lost Actions haben begrenzte Ladungen —, aber die Verschiebung aktiviert eine Sperre, die nie gegriffen hat.
- `BozjaRotation.cs:140` (Lost Excellence): `StatusNeed = [Weakness]`. Anders als die übrigen wirkt diese Prüfung, weil der Spieler `Weakness` tragen kann; sie beschränkt Lost Excellence auf den geschwächten Zustand. Ein Zusammenhang zwischen beiden ist nicht erkennbar — fachliche Frage, kein Seitenfehler.

**Auflösungsbedingung:** je Fundstelle eine Beobachtung im betreffenden Inhalt oder eine Rückfrage an den Upstream-Autor. Alle vier stammen aus Upstream und sind dort unverändert.

**Empfehlung: liegen lassen.** Alle vier verbliebenen Fundstellen liegen in PvP oder Bozja, also außerhalb des Nutzungsprofils, und jede verlangt eine Richtungsentscheidung, die eine Beobachtung im jeweiligen Inhalt voraussetzt. Die Klasse ist vollständig erhoben und durch `scan11.py` gegen Rückfall gesichert — das ist der Zweck der Erfassung, die Bearbeitung ist es hier nicht.

### Der erzeugte Merkmalssatz enthält keine Rollenmerkmale · N, R

`TraitRotationGetter.AddToList` verwirft jedes Merkmal mit `item.ClassJob.RowId == 0`. Rollenmerkmale sind keiner einzelnen Klasse zugeordnet und fallen damit sämtlich heraus: Im erzeugten `Rotation.resx` findet sich kein `EnhancedReprisalTrait`, `EnhancedRampartTrait`, `EnhancedSecondWindTrait`, `EnhancedSwiftcastTrait`, `EnhancedFeintTrait`, `EnhancedAddleTrait` — null Treffer für jedes davon.

**Wirkung:** Wo eine Rollenaktion durch ein Merkmal aufgewertet wird, muss die Stufe als Zahl im Code stehen, statt vom Merkmalsobjekt gelesen zu werden. Zwei Stellen tun das bereits, beide mit 98 für dieselbe Aufwertung (`MitigationDebuffDuration`, `EnhancedReprisalLevel`), inzwischen über eine gemeinsame Konstante.

**Nicht sofort behoben:** Die Änderung liegt im Generator, der nur mit installiertem Spiel läuft, und sie vergrößert den erzeugten Satz um eine ganze Kategorie — Wirkungsbereich und Nutzen sind vor dem Eingriff zu erheben. Zu klären ist außerdem, ob `ClassJob.RowId == 0` tatsächlich das Kennzeichen von Rollenmerkmalen ist oder nur eines von mehreren Merkmalen ohne Klassenbezug.

### `CanUse` als Prüfung, nicht als Wahl — mit Zuweisung als Nebenwirkung · N, R

**Konzept:** `docs/rotation-flow/03-universal.md`
`CanUse` als bloße Prüfung weist `Target` zu. Anlass waren die beiden Sanctus-Regeln, die mit `DiaPvE.CanUse(out _) || AeroIiPvE.CanUse(out _) || AeroPvE.CanUse(out _)` eine Ersatzgarantie prüften; beide tun es nicht mehr (A178, A231). Es ist dieselbe Nebenwirkung, die beim Wiederbelebungspfad eine eigene Vorkehrung nötig gemacht hat (`RaisePendingAndCastable` sichert und stellt den Zielüberschreiber wieder her, A56).

**Umfang, erhoben statt geschätzt: 120 Fundstellen in 25 Dateien** — `BeirutaRDM` 16, `SGE_Reborn` 11, `NIN_Reborn` 10, `BeirutaSGE` 10, dazu `CustomRotation_Ability`, `CustomRotation_GCD`, `CustomRotation_Items` und die UI. Das Muster ist damit nicht die Ausnahme, die diese Rotation eingeführt hat, sondern die Hausform des gesamten Baums. Das ändert die Frage: Nicht „sollen diese zwei Stellen anders gebaut werden", sondern „hat der Baum eine seiteneffektfreie Prüfung nötig".

**Die Vorfrage, am Code beantwortet (14.10.2026, A286 H1):** RSR bietet keine seiteneffektfreie Prüfung. `BaseAction.CanUse` setzt bei Erfolg `PreviewTarget` und `Target`, schreibt außerhalb der Vorschau `Setting.EndSpecial` und protokolliert über `ActionTracer` — seit A284 erscheint eine abgelehnte Prüfung im Abwehrpfad deshalb auch als „refused before" in `DefenseTrace`. Eine abgelehnte Prüfung setzt kein Ziel; die Zuweisung steht hinter allen Prüfungen.

**Folgen hätte die Nebenwirkung nur über einen zweiten Leser:** Code, der `X.Target` liest, ohne `X.CanUse` in derselben Entscheidung gerufen zu haben, liest das Ziel des letzten erfolgreichen Aufrufs, Prüfung oder nicht. Erhoben: 55 solche Lesestellen außerhalb der Churin-Dateien (kein `X.CanUse` in den drei Zeilen davor), überwiegend in Beiruta-Rotationen und Upstream-Code der Reborn-Rotationen (Barde, Viper, Ninja, Duty). **Offen, meine Arbeit:** je Stelle klären, ob das gelesene Ziel aus derselben Entscheidung stammt; erst wenn eine Stelle ein fremdes Ziel liest, ist eine seiteneffektfreie Prüfung (Wirkungsbereich: alle Rotationen und die Paketnutzer) überhaupt begründet.

### `HasSurvivingShield` misst die **kürzeste** Schildrestzeit, nicht die längste · N, R

**Konzept:** `docs/rotation-flow/09-tank-selfprotection.md`, `docs/rotation-flow/13-aoe-damage-classification.md`
`StatusHelper.HasSurvivingShield`: `GetObjectShield() > 0 && !WillStatusEnd(horizon, false, ShieldStatus)`. `WillStatusEnd` stützt sich auf `StatusTime`, und das liefert das **Minimum** über alle vorhandenen gelisteten Status (`MinStatusRemainingTime`). Beantwortet wird damit „laufen **alle** Barrieren noch?", während der Doku-Kommentar derselben Methode „has an active shield that will still be up" sagt — Singular.

**Wirkung:** Ein Ziel mit mehreren Barrieren — der Regelfall in einer Gruppe mit Heiler, etwa Galvanize plus Eukrasian Diagnosis plus eine Gruppenbarriere — gilt als ungeschützt, sobald die **kürzeste** unter den Horizont fällt, obwohl `GetObjectShield()` weiterhin einen Wert meldet. Der Fehler zeigt in dieselbe Richtung wie der behobene (A43): zu viel Heilung, nie zu wenig. Deshalb hat die Erweiterung der Liste ihn nicht verschlimmert, sondern nur häufiger sichtbar gemacht.

**Seit der Entfernung der Schildanrechnung (A85) hat die Methode keinen Leser mehr im Baum**; der Tankbuster-Plan
liest dieselbe Regel (früheste Barriere entscheidet) aus der Statusliste selbst nach
(`TankbusterForecast.BarrierStandsThrough`, A263), damit kein vom Bestätigungspaket vorhergesagter Status als stehend
gilt; der Befund unten gilt für beide. Sie bleibt trotzdem: `public static` in `RotationSolver.Basic`, also Teil der Paketschnittstelle für abgeleitete Rotationen — ihre Entfernung wäre ein Signaturbruch für den Betroffenenkreis R, und die Methode ist für ihre eigene Frage („hält die Barriere noch") richtig gebaut. Der Befund unten betrifft ihre Genauigkeit, nicht ihre Berechtigung.

**Warum nicht sofort behoben:** Die naheliegende Umkehr auf das Maximum tauscht den Fehler nur aus. Sie würde ein Ziel als geschützt werten, dessen große Barriere gerade ausläuft, solange irgendeine kleine bleibt — und `GetObjectShield()` liefert nur den Gesamtwert, nicht die Aufteilung je Status. Eine Unterschätzung kostet eine überflüssige Heilung, eine Überschätzung kostet einen Tod.

**Auflösungsbedingung:** Laufzeitbeobachtung, welche der beiden Abweichungen tatsächlich auftritt, oder eine Quelle für die Aufteilung des Schildwerts auf die einzelnen Status. **Empfehlung bis dahin: belassen** — die heutige Richtung ist die sichere.

### Statuslisten und Einzelprüfungen ohne die Geschwister-Ids ihrer Wirkung · N, R

**Konzept:** `docs/rotation-flow/03-universal.md`
Das Spiel führt jede Wirkung unter mehreren Status-Ids desselben Anzeigenamens — eine je Fassung der Fähigkeit, dazu PvP-Formen und die Fassungen, in die eine Trait aufwertet. Eine handgepflegte Aufzählung, die eine Id nennt und die Geschwister auslässt, antwortet für den Träger der ausgelassenen Id **falsch**, nicht nur ungenau. `scan14.py` erhebt die Klasse über alle Statuslisten in `StatusHelper.cs` (A49) und nennt die jeweils aktuelle Zahl; sie steht hier bewusst nicht, weil sie mit jeder Ergänzung wächst. Genau das ist nach der Behebung von `RampartStatus` und `ReprisalStatus` geschehen: Je mehr Ids geführt sind, desto mehr Gruppen haben einen Vertreter, und desto mehr Geschwister werden überhaupt sichtbar. Eine steigende Zahl ist hier also Fortschritt, nicht Verfall.

**Was davon kein Defekt ist:** 114 Treffer stammen aus `PhantomDispellable` und `PurifyPvPStatuses`, die bewusst Teilmengen sind. Weitere Treffer sind andere Wirkungen unter geteiltem Namen — `Nebula_3051` (Reflexion), `Bloodwhetting_3030` (Lebensraub), `Holmgang` 88 und 1305 auf dem Ziel statt dem Träger (C15). Der Scan druckt zu jedem Kandidaten die Wirkbeschreibung und die Marke `same opening`/`differs`; entscheiden muss ein Leser.

**Zwei benannte Fundstellen, an denen die Zuordnung offen ist:**

- `TankStanceStatus` führt `IronWill` (79) und `RoyalGuard_1833`, nicht aber `IronWill_393`, `IronWill_2843` und `RoyalGuard` (392) — gleicher Anzeigename, gleicher Wirktext „Enmity is increased." Die Liste entscheidet, wen die Zielwahl für einen Tank hält (`ActionTargetInfo.cs`, sieben Stellen, darunter `FindTankTarget` und `FindKardia`) und ob `HasTankStance` für den Spieler greift. `Defiance_1396` und `Grit_1397` („Damage dealt and taken are reduced.") sind die Fassungen vor Shadowbringers und gehören **nicht** hinein.
- `GetCurrentMitigationPercent` (`CustomRotation_OtherInfo`) liest `StatusID.Addle` und `StatusID.Feint` als **einzelne** Id. `Addle_1988` (Geltungsbereich BLM SMN RDM BLU PCT, keine PvP-Aktion in `ActionId.resx`) und `Feint_2185` existieren; die Minderungsbilanz zählt eine vorhandene Schwächung dann als nicht vorhanden. Einzelprüfungen dieser Art sieht `scan14.py` nicht — er erhebt nur Ids in Listen.

**Stand der beiden Fundstellen (28.09.2026):** WrathCombo, das dieselben Abfragen im Spiel stellt, prüft die Tankhaltungen mit Iron Will 79, Defiance 91, Grit 743, Royal Guard 1833 und die Schwächung mit Addle 1203 — genau die Ids, die RSR führt (Status: anderes Werkzeug, keine Spielquelle). Ein Beleg für einen Defekt an diesen Stellen liegt damit nicht vor; offen bleibt die Klasse.

**Warum nicht behoben:** Welche Id das Spiel je Stufe tatsächlich setzt, ist aus den Daten nicht zu entscheiden. Bei `Reprisal_2101` trug der Beleg — genau eine PvE-Aktion, Geltungsbereich auf die vier Jobs verengt, belegter Trait-Stufenwert; bei den Tankhaltungen tut er das nicht, und eine falsch aufgenommene Id kehrt die Antwort in die andere Richtung um. **Auflösungsbedingung:** Laufzeitbeobachtung, welche Id ein Tank beziehungsweise ein Zauberer im Ziel trägt, oder eine Quelle für die Id-Zuordnung je Stufe.

**Empfehlung: erfassen, nicht bearbeiten.** Behoben ist, was eine Wirkkette im Code liest und wo der Beleg trägt. Der Rest ist eine Klasse ohne Schranke: Ein Rückgabewert, den nur Wegsehen grün hält, wäre schlechter als keiner, und `scan14.py` hält die Liste jederzeit wieder abrufbar.

### Die Minderungsbilanz kennt zwei Schadensarten, die Datenquelle drei · N, R

**Konzept:** `docs/rotation-flow/08-mitigation-synergy.md`
Confession und die 15 % von Troubadour, Tactician und Shield Samba sind seit A179 eingerechnet; `IsPhysicalDamageIncoming` prüft seit A179 die physischen Zeilen 1–4.

`GetCurrentMitigationPercent` gewichtet sechs Faktoren binär nach `incomingMagical ? 0.90f : 0.95f` — Addle, Feint, Fey Illumination, Magick Barrier und zwei weitere. Der Wert kommt aus `DataCenter.IsMagicalDamageIncoming()`, das `AttackType.RowId == 5` prüft. Das Blatt kennt aber mehr Werte als 5 und 7; und wenn gerade **niemand** wirkt, ist `CastActionId` überall 0 und die Antwort ebenfalls `false`. Beide Fälle rechnet die Bilanz als **physisch** — mit vertauschten Vorzeichen: Addle zählt dann −5 % statt −10 %, Feint −10 % statt −5 %.

**Der Baustein, der das trennen würde, ist vorhanden und nicht verdrahtet.** `IsPhysicalDamageIncoming()` (`AttackType.RowId == 7`) hat im ganzen Baum **keinen Leser** — kein Kampfpfad, nicht einmal die Diagnoseanzeige, die ihr magisches Gegenstück zeigt. Das ist die Bauform, die `CLAUDE.md` als fehlende Verdrahtung statt tote Stelle führt (Beleg `ResetAvailabilityCheck`): Mit beiden Prädikaten ließe sich „unbekannt" von „physisch" unterscheiden, statt es stillschweigend zusammenzulegen.

**Wirkung: klein, und das ist belegt, nicht vermutet.** `GetCurrentMitigationPercent` hat genau **einen** Verbraucher, die Diagnosezeile im Debug-Reiter (`MainWindow_Debug`, `0.0–0.95`). Kein Kampfpfad liest sie; in meiner eigenen Arbeit dieser Sitzung ist sie als **Vorbild** zitiert, nicht als Aufrufziel. Der Fehler zeigt sich also derzeit nur in einer Anzeige — aber die Bilanz ist öffentlich und damit für abgeleitete Rotationen lesbar, und Konzept 08 sieht sie als künftige Entscheidungsgrundlage.

**Zwei weitere Klon-Reste an derselben Stelle**, ohne eigene Wirkung: Die beiden Methoden sind Kopien mit geänderter Konstante, und die `<remarks>` der physischen Fassung sagt „Returns early on the first confirmed **magical** cast". Der Kommentar bleibt stehen, bis die Stelle bearbeitet wird — ihn allein anzugleichen würde den Beleg der Entstehung tilgen.

**Zuordnung belegt (A179, `v2.xivapi.com`):** 1 斬 slashing, 2 突 piercing, 3 打 blunt, 4 射 shot, 5 魔法 magic, 6 ブレス breath, 7 音波 sound, 8 limit break (das Blatt führt nur japanische Namen). Offen bleibt die Bilanz selbst: Sie rechnet „kein Cast", breath und sound als physisch. **Auflösungsbedingung:** ein Verbraucher im Kampf; dann die Fälle „unbekannt" und „weder noch" eigens führen.

**Empfehlung: erfassen.** Solange nur eine Anzeige betroffen ist, wäre eine Umstellung auf geratener Zuordnung teurer als der Fehler. Wird die Bilanz zur Entscheidungsgrundlage, ist sie vorher aufzulösen — dann gehört auch der dritte Fall benannt, statt ihn als physisch zu führen.

### Die Aquapolis fehlt in der Zielpriorisierung der Schatzkarten-Dungeons · N, U

`ObjectHelper.TreasureDungeonPrio` zählt neun Schatzkarten-Dungeons auf und nennt zu jedem die NPCs, die Vorrang haben — Namazu Stickywhisker in den Lost Canals, Alpaca of Fortune in Cenote Ja Ja Gural, Vaultkeeper in Vault Oneiron. `DataCenter.IsInTheAquapolis` ist der zehnte und **einzige** ohne Zweig; von den elf Flags der Region `Treasure Hunt` ist es das einzige ohne jeden Leser im Baum.

**Entstehung belegt:** Konstante und Aufzählung stammen aus demselben Commit (`4ee849eca`, 14.05.2026, „Add Treasure Dungeon support"). Die Stelle war also nie vollständig — *Ignorant Surgery* in Parnas' Sinn, nicht das Altern einer zuvor richtigen Aufzählung.

**Kandidat, nicht Befund.** Die Gegenhypothese ist nicht widerlegt: Hat die Aquapolis gar keinen Vorrang-NPC dieser Art, dann ist ihre Auslassung aus der Aufzählung richtig und der Befund verschiebt sich auf die Konstante, die dann keinen Verwender braucht. Das ist eine Spieltatsache, und sie ist von hier aus nicht belegbar — das Enum `NPCName` ist aus den Spieldaten erzeugt, führt aber keinen Territoriumsbezug, und die Wikis sind vom Egress gesperrt.

**Auflösungsbedingung:** welche Gegner in der Aquapolis Vorrang haben — zuerst aus den Spieldaten (BNpcName des Gebiets) und Referenzquellen, wie bei den übrigen neun; seine Angabe nur, wo die Quellen schweigen (A286, H3). **Empfehlung: erfassen, nicht bearbeiten** — Schatzkarten stehen nicht im benannten Nutzungsprofil, und ohne die Namen wäre jeder Zweig geraten.

### Zustandsabfragen, die bei jedem Lesen neu über Gruppe oder Gegner laufen · N, R

**Konzept:** `docs/rotation-flow/03-universal.md`
`DataCenter` führt **15 öffentliche statische Eigenschaften, deren Getter iteriert** — darunter genau die, die in den heißen Pfaden stehen: `PartyTank`, `RefinedHP`, `PartyMembersHP`, `NumberOfHostilesInRange`, `NumberOfHostilesInMaxRange`, `AverageTTK`, `DPSTaken`, `AreHostilesCastingKnockback`. Dazu die Helfer derselben Bauform: `SurveyHostileOutput` fragt je Gegner im Radius sechs Status ab, `GetCurrentMitigationPercent` vier, `SurveyStuns` einen.

**Der schwerste Fall war `RefinedHP`, und Upstream hat ihn behoben.** `5ffd22056` (Upstream, 12.09.2026, mit 7.5.6.6 eingezogen) legt einen Cache mit 15 ms Lebensdauer davor, ebenso für `AverageTTK` und die Gruppen-HP-Kennzahlen; `ResetAllRecords` verwirft ihn. Der folgende Absatz beschreibt den Stand davor und bleibt als Begründung stehen, weil das Muster in den übrigen Eigenschaften unverändert vorliegt: Es baute bei **jedem** Lesen ein neues `Dictionary` über alle Gruppenmitglieder, mit einem `try/catch` je Mitglied — und es ist die Nachschlagetabelle, aus der `ObjectHelper.GetHealthRatio` und `GetPlayerHealthRatio` ihren Wert holen. Eine Tabelle, die bei jedem Nachschlagen neu gebaut wird, ist unabhängig von der Häufigkeit eine verkehrte Konstruktion. `GetHealthRatio()` hat 129 Aufrufstellen im Baum.

**Die Kosten misst der Code jetzt selbst (A286, H2).** Aufrufstellen sind kein Maß für Aufrufe je Frame: die meisten liegen in Rotationen, die nie zugleich laufen. `UpdateCost` misst jeden RSR-Aktualisierungsschritt vom ersten bis zum letzten Handler desselben Frames und schreibt nach jedem Kampf eine Zeile in `DefenseTrace.log`: Schritte, Mittel und Höchstwert in ms und den Anteil an der Framezeit der Frames, in denen RSR lief.

**Warum trotzdem kein Defekt:** Das Verhalten ist richtig, nur der Aufwand ist es vielleicht nicht. `PartyMembers` ist auf die eigene Gruppe begrenzt (Allianzmitglieder gehen in `AllianceMembers`, geprüft an `TargetUpdater`), es geht also um maximal acht Einträge je Aufbau.

**Auflösungsbedingung:** die Messzeile im nächsten Protokoll; trägt RSR einen spürbaren Anteil der Framezeit oder ragen einzelne Schritte heraus, folgt der Umbau, sonst wird der Eintrag geschlossen. **Auflösungsweg, falls ja:** Das Muster liegt im Baum bereits vor — `TargetUpdater` schreibt `PartyMembers`, `AllianceMembers` und `AllHostileTargets` einmal je Frame in `DataCenter`. `RefinedHP` gehört auf denselben Weg. Zu bedenken ist, dass `InEffectTime` zeitbasiert ist: ein Wert je Frame ist damit nicht bitgleich zum Wert je Lesen, also eine Verhaltensänderung ohne Nachweismöglichkeit und nach Projektregel nur mit beibehaltener Voreinstellung zu bauen.

**Empfehlung: erfassen.** Ohne Messung wäre jede Umstellung eine Verbesserung auf Verdacht, und der Betroffenenkreis umfasst die Paketnutzer: `RefinedHP` und die übrigen Eigenschaften sind öffentlich, ihr Aufrufverhalten ist Teil des Vertrags.

### `Configs.Migrate` ist kein Migrationspfad, sondern ein Zurücksetzen · N, R

`Configs.Migrate` lautet vollständig: weicht die gespeicherte `Version` von `CurrentVersion` ab, wird `new Configs()` zurückgegeben — **die gesamte Nutzerkonfiguration fällt auf die Vorgaben zurück**. Der Kommentar „Implement migration logic if needed" weist die Stelle als bewusst offenen Platzhalter aus, nicht als Fehler; sie ist damit technische Schuld und kein Defekt.

**Kosten, und sie sind höher als sie aussehen.** Die Schuld ist nicht nur ein fehlendes Bequemlichkeitsmerkmal, sie **blockiert andere Behebungen**: Jede Korrektur, die einen Vorgabewert ändern muss, um das bisherige Verhalten zu erhalten, ist ohne Feldmigration nicht durchführbar. Der Eintrag zu `InterruptDelay`/`ProvokeDelay` weiter oben ist genau daran gescheitert. Dieselbe Sperre trifft künftig jede Einstellung, deren Vorgabe sich als falsch erweist.

Hinzu kommt die unmittelbare Wirkung für den Auftraggeber: Erhöht Upstream `CurrentVersion` — etwa weil dort ein Feld hinzukommt —, sind beim nächsten Start **alle** eigenen Einstellungen weg. Ein Sicherungsstand lässt sich über `Backup()` anlegen; `Restore()` verweigert allerdings genau dann, wenn die Version abweicht, also im einzigen Fall, in dem man ihn bräuchte.

**Auflösungsbedingung:** eine feldweise Migration, die die gespeicherte Version liest und nur die geänderten Felder umstellt, statt die Datei zu verwerfen. Erst danach sind Vorgabewert-Korrekturen überhaupt möglich.

### Doppelte Zustandswahl in den Zustandskommandos · N

**Konzept:** `docs/rotation-flow/03-universal.md`
Die Zustandswahl liegt an zwei Orten: implizit in `AdjustStateType`, wo `/rotation Auto` über `UpdateTargetingIndex` selbst durch die Zielarten schaltet, sofern `ToggleAuto` aus ist; explizit in den fünf `Cycle*`-Methoden, die dieselbe Aufgabe erneut lösen und über `CycleType` bzw. `DTRType` am Chatkommando und am Leistenklick hängen. Da die `Cycle*` ebenfalls `DoStateCommandType` rufen, greift `AdjustStateType` auch dort; die Toggle-Optionen wirken dadurch als Krücken für fehlende Übergänge, statt als unabhängige Achse.

**Kosten:** `DTRAllAuto` kollabiert mit aktivem `ToggleAuto` auf Off ↔ Auto(0), die Zielarten-Rotation ist dann tot. Umgekehrt ist `ToggleAuto` bei `DTRManualAuto` der einzige Ausschaltweg über die Leiste — ein pauschales Umgehen der Toggle-Auswertung würde ihn beseitigen.

**Empfehlung: belassen.** Die Kosten treffen zwei Bedienvarianten, nicht das Kampfverhalten, und jeder Umbau berührt einen Zustandsautomaten, dessen Übergänge statisch nicht abzusichern sind — der einzige bisher geprüfte Eingriff hätte einer Variante den einzigen Ausschaltweg genommen.

**Auflösungsbedingung:** erst mit einer Möglichkeit zur Laufzeitbeobachtung; ein Zustandsautomat mit acht Zuständen, fünf Zykluswegen und zwei Schaltern ist statisch nicht abzusichern. Bei einem Eingriff ist der Persistenzvertrag zu beachten: `DTRType` und `CycleType` liegen als Ordinalzahlen in der Nutzerkonfiguration, ihre Reihenfolge ist nicht frei änderbar.

Geprüfte Nicht-Fehlstellen: `DTRManualAuto` bildet den vom Enum-Text beschriebenen Zwei-Zustands-Zyklus ab (kein Fehler, AUDIT_LOG A14); ein zu großer `TargetingIndex` kann keinen Indexfehler auslösen, `DataCenter.TargetingType` rechnet `% Count`.

### Die Aufnahme in die AoE-Liste unterscheidet Raidwide und ausweichbare Fläche nicht · N

`Watcher.ActionFromEnemy` nimmt eine Gegneraktion dauerhaft in `HostileCastingArea` auf, wenn die Gruppe mindestens vier Mitglieder hat, die Aktion eine Wirkzeit besitzt, zur Kategorie Spell/Weaponskill/Ability gehört und **jedes** Gruppenmitglied im selben Effektsatz Schaden genommen hat. „Record AOE actions" ist ab Werk an.

**Was daran offen ist, ist die Aufnahme, nicht mehr die Bewertung.** Die Größenordnung ist seit A99 bis A102 gemessen, gespeichert und wird beim Verbrauch verrechnet — vollständiger Stand in `docs/rotation-flow/13-aoe-damage-classification.md`. Damit kostet eine zu klein bewertete Aktion keine Abklingzeit mehr. Die Aufnahme selbst bleibt grob: Ob sich echte Raidwides beim Lernen von ausweichbaren Flächen unterscheiden lassen, ist ohne Spieldaten nicht entscheidbar (Kandidaten: `CastType`, `EffectRange`), und die Reichweitenprüfung `AreaCastCanReachPlayer` entschärft den Fall nur für den Spieler selbst.

**Bewertung: technische Schuld, kein Defekt.** Eine Verschärfung der Aufnahmebedingung wäre eine Verhaltensänderung ohne Nachweismöglichkeit und gehörte deshalb hinter eine eigene Option. Geprüfte Nicht-Fehlstelle: das Speichern läuft asynchron, kein blockierendes Schreiben im Kampfpfad.

**Drei benannte Grenzen der Bewertung, alle erfasst und keine behoben:**

- **Serien kleiner Einschläge.** Mehrere kleine Treffer kurz hintereinander summieren sich; jeder einzeln unter jeder Schwelle, zusammen tödlich. Eine Einzelwertprüfung sieht das nicht. Die Ablage trägt einen Anteil je Aktion, keine Folge über die Zeit.
- **Der VFX-Zweig umgeht die Rechnung.** `IsCastingAreaVfx` erkennt Stack- und Spread-Marker über Effektpfade statt über Aktions-Ids; für die gibt es kein Potential, also greift die Bewertung dort nicht. Konsistent mit „unbewertet heißt mindern", aber diese Auslöser bleiben grob.
- **Die Rückrechnung um die wirkende Minderung ist verworfen, nicht vergessen.** `GetCurrentMitigationPercent` ist eine Aufzählung bekannter Status, also im Zweifel unvollständig — und eine Näherung, die den Wert **erhöht**, ist gefährlicher als eine Beobachtung, die ihn zu niedrig ansetzt und sich beim nächsten ungeminderten Treffer selbst korrigiert (Begründung in Konzept 13, Abschnitt Falsifikation).

**Statt auf eine Beobachtung von ihm zu warten, misst die Regel selbst (A205):** `AreaCastReachedPlayer` hält je gelisteter Aktion fest, ob ihre letzte Landung den lebenden Spieler traf; unter „Skip area defence for casts that missed you" (ab Werk an, Nutzen nicht belegt) öffnet ein Cast, der ihn zuletzt verfehlte, seine Flächenabwehr nicht, bis ein Treffer das widerlegt. Offen bleibt die Aufnahme selbst.

### Zwei Werte des Dunkelritters hängen nicht an der Strategie, für die sie gelten · N

**Konzept:** `docs/rotation-flow/10-drk-blackest-night.md`
`DRK_Reborn.BlackestNightMinHostiles` und `BlackestNightHealthRatio` gelten nur für die engeren Optionen von `BlackestNightUsage` — die Gegnerzahl für zwei davon, die Gesundheitsschwelle für eine. Beide tragen kein `Parent`, erscheinen also immer und unverändert eingerückt, auch wenn die eingestellte Strategie sie gar nicht liest. Ihre Labels verwiesen zusätzlich auf „die Option oben", obwohl zwischen ihnen und `BlackestNightUsage` andere Einstellungen stehen; das ist mit A103 berichtigt, die Kopplung nicht.

**Warum nicht mitbehoben:** `ShouldShowRotationConfigInternal` vergleicht `ParentValue` gegen **einen** Wert. Für die Gesundheitsschwelle wäre die Kopplung damit korrekt möglich, für die Gegnerzahl nicht — sie gilt für zwei Strategien. Eine halbe Kopplung wäre schlechter als keine: Sie ließe den Nutzer glauben, die sichtbaren Werte seien genau die wirksamen.

**Auflösung:** entweder `ParentValue` auf mehrere zulässige Werte erweitern — Wirkungsbereich ist der gemeinsame Zeichenpfad aller Rotationen, Betroffenenkreis R und U — oder es beim Tooltip belassen, der jetzt sagt, für welche Option jeder Wert gilt. **Empfehlung: beim Tooltip belassen**, solange kein zweiter Fall dieser Art auftritt; der Nutzen ist eine Einrückung, die Kosten sind eine Signaturerweiterung im Upstream-Pfad.

### `SpreadDamagePaths` enthält keinen Spread-Marker · N

**Konzept:** `docs/rotation-flow/13-aoe-damage-classification.md`
`DataCenter.SpreadDamagePaths`. Zwei der vier Pfade stehen wortgleich in `SharedDamagePaths`, die anderen beiden sind laut eigenem Kommentar „AOE share markers", also ebenfalls Stack-Marker. Ohne Fehlwirkung, weil `IsCastingAreaVfx` alle drei Listen prüft. **Kosten:** eine Kategorie, die etwas anderes verspricht, als sie enthält. Nebenbefund: `SharedDamagePaths` führt `vfx/lockon/eff/com_trg01_0c` zweimal (2022 und 2024), im `FrozenSet` folgenlos.

**Entstehung belegt** (A41): eingeführt in `33e6acb1` vom 07.05.2026, einem Sammel-Refactoring („Refactor for var usage, safety checks, and plugin compat"), und zwar bereits mit den beiden Duplikaten und dem übernommenen Kommentar. Das ist *Ignorant Surgery* nach Parnas — Klon einer Nachbarliste ohne Anpassung —, kein späteres Veralten. Die Historie war dafür zu vertiefen; im flachen Klon lag der Einführungs-Commit vor dem Anfang.

**Empfehlung: Nullvariante, Punkt offen führen.** Für RSR ist der Unterschied ohne Verbraucher — Spread wie Stack lösen dieselbe Gruppenmitigation aus, eine Positionierung leitet das Plugin nicht. Die Kategorie zu füllen setzt Spieldaten voraus, die hier fehlen: Eine Websuche nach den beiden einzigartigen Pfaden (`x6fd_loc04m_5s1v`, `m0922tar_a0w`) bleibt ohne Treffer, ihre Zuordnung ist damit unbelegt. Die Liste zu streichen ist nur dann verhaltensneutral, wenn genau diese beiden nach `SharedDamagePaths` übernommen werden — und tilgt dann das Signal, das den offenen Punkt sichtbar hält. Wer dennoch aufräumen will: die kleinste verhaltensneutrale Fassung ist das Entfernen der beiden Duplikate `x6r9_loc01_t0a1` und `x6r9_loc02_t0a1` aus dieser Liste.

### Release-Paket enthält vermeidbaren Ballast · N, R

**Am Artefakt belegt** (`latest.zip` von 7.5.7.1+wsh1, 4,67 MB, gemessen 14.10.2026, A279): Nutzlast sind `RotationSolver.dll`, `RotationSolver.Basic.dll`, `ECommons.dll`, `RebornMaterial.dll` (die Oberflächenbibliothek des Upstream-Umbaus, A253) und `RotationSolver.json` samt den beiden `deps.json`, gepackt 2,28 MB; dazu kommen `RotationSolver.Basic.xml` (7,99 MB, gepackt 0,73 MB), der Analyzer samt Symbolen (5,73 MB, gepackt 0,74 MB) und `RotationSolver.Basic.pdb` (1,35 MB, gepackt 0,93 MB). Das NuGet-Paket ist seit dem Upstream-Umbau (`IsPackable=false`) tatsächlich nicht mehr im ZIP.

**Kein Fork-Defekt, sondern Upstream-Verhalten** (A40, A279): Das Upstream-Release 7.5.7.1 enthält dieselben zwölf Dateien in denselben Rollen, 4,50 MB gegenüber 4,67 MB (gemessen 14.10.2026); so schon 7.5.5.41 und 7.5.6.0. Der Fork weicht im Dateibestand nicht ab; auch der Abhängigkeitsgraph beider `deps.json` ist deckungsgleich. Eine Bereinigung wäre damit eine eigene Abweichung im Verpackungspfad.

**Ursache** ist weder die Prune-Regel — `PruneOutputDlls` arbeitet auf `ReferenceCopyLocalPaths` und erfasst nichts davon — noch das `OutputPath` der Projektdatei: der Bauschritt in `publish.yaml` baut mit `--output .\build`, wodurch die Ausgaben aller beteiligten Projekte in einem Verzeichnis landen, das DalamudPackager packt. `GeneratePackageOnBuild` bediente dabei die Autoren abgeleiteter Rotationen; Upstream hat es mit dem UI-Umbau abgeschaltet (A253).

**Kosten:** 4,67 MB Download statt rund 2,3 MB (Stand 14.10.2026), funktional folgenlos.

**Auflösungsbedingung:** Der naheliegende Weg trägt nicht — der Standard-Target von DalamudPackager reicht `Exclude` nicht durch und läuft nur, solange keine eigene `DalamudPackager.targets` im Projektverzeichnis liegt; diese müsste den vollständigen Task-Aufruf samt aller Manifest-Felder nachbauen. `Exclude` vergleicht exakt über `List.Contains` (im Paket DalamudPackager, nicht in diesem Baum), kennt also keine Muster, und das NuGet-Paket trägt die Version im Dateinamen. Der Build-Workflow kompiliert nur und prüft das Paket nicht. Aufgreifen erst, wenn der Veröffentlichungspfad prüfbar ist. Geprüft: die XML-Dokumentation wird zur Laufzeit nicht gelesen.

**Empfehlung: nicht aufgreifen.** Der Auftraggeber hat die Spielbarkeit des Release-ZIPs zur Anforderung gemacht. Ein Eingriff in den Verpackungspfad ist genau die Klasse von Änderung, deren Ergebnis erst am fertigen Release sichtbar wird — und dieser Pfad läuft nur auf einen Tag, wird von keiner Prüfung abgedeckt und weicht dann zusätzlich vom Upstream ab. 2,4 MB Download stehen gegen das Risiko, ein nicht ladbares Paket zu bauen.

### VPR: leerer Zweig einer Struktur, die anderswo eine Entscheidung trägt · U

`VPR_Reborn.GeneralGCD`, die Stellen von Swiftskin's Bite und Swiftskin's Sting. Das Muster `!HasHunterAndSwift` kommt viermal vor; der Vorspann `!IsHunter && !IsSwift` trägt nur an der Coil-Stelle (Hunter's und Swiftskin's Coil, Wahl nach Position) eine eigene Entscheidung, an Bite und Sting wiederholt er die folgende Regel, an der Den-Stelle fehlt er ganz. Weder ein fehlender Inhalt noch dessen Entbehrlichkeit ist belegbar. **Bewusst nicht gelöscht** (AUDIT_LOG A11): Die Entfernung wäre verhaltensneutral, würde aber die Asymmetrie verdecken, die den Befund sichtbar macht. **Auflösung:** Adressat ist der Upstream. **Empfehlung: belassen** — der leere Zweig ist der einzige Hinweis auf die Lücke, und ohne den Autor ist nicht zu entscheiden, ob Inhalt fehlt oder der Vorspann überflüssig ist.

### `AutodutyUpdateState` dupliziert `UpdateState` · U

`RSCommands_StateSpecialCommand.cs`: rund 100 wortgleiche Zeilen, abweichend nur die Fälle `TargetOnly` und `AutoDuty` (`TargetingTypeOverride = targetingType` statt `null`) und der Zustandstext. **Kosten:** jede künftige Änderung am Zustandsautomaten muss an zwei Stellen erfolgen. **Auflösung:** über einen optionalen `TargetingType?`-Parameter zusammenführen, sobald an dieser Stelle ohnehin gearbeitet wird. **Empfehlung: nicht eigens angehen** — die Duplizierung kostet erst bei der nächsten Änderung am Zustandsautomaten etwas, und genau die ist nach dem Punkt oben ohne Laufzeitbeobachtung nicht zu empfehlen. Beide Punkte lösen sich gemeinsam oder gar nicht.

### Das Aktionsfenster rechnet die Bewegungssicherheit ein zweites Mal · U

**Befund (A212):** `MainWindow.GetMovementSafetyStatus` (bis zum Upstream-Umbau vom 04.10.2026 in `RotationConfigWindow`) wiederholt `ActionTargetInfo.CheckMovementSafety` in eigener Form. Die beiden laufen schon auseinander: Die Regel „am Ziel stehend ist ein Sprint nur Schaden" fehlt im Fenster. **Kosten:** Das Fenster zeigt eine andere Antwort als die, nach der die Regel entscheidet; im Kampf ändert es nichts. **Auflösung:** `CheckMovementSafety` gibt Ergebnis und Grund heraus, das Fenster zeigt sie an.

### Plugin-Identität ist unverändert die des Upstreams · N

`manifest.json` ist gegenüber `upstream/main` unverändert: `InternalName: RotationSolver`, `RepoUrl` und `IconUrl` verweisen auf FFXIV-CombatReborn, `AcceptsFeedback` ist gesetzt. Ein Fork-Build meldet sich damit unter derselben Plugin-Identität an wie das Upstream-Plugin. Bisher mitgeführt, nicht als Entscheidung erfasst; aufgefallen beim Abgleich der README gegen den Ist-Stand (A39).

**Kosten — Inferenz, nicht am Artefakt belegt:** Dalamud adressiert Plugin-Verzeichnis, Konfigurationsablage und Aktualisierung über den `InternalName`. Daraus folgte, dass Fork- und Upstream-Installation einander verdrängen, dieselbe gespeicherte Konfiguration benutzen und ein Feedback-Weg beim Upstream endet. Belegt ist allein der Manifest-Inhalt; das Installationsverhalten ist in dieser Umgebung nicht prüfbar.

**Gegenposition:** Die gemeinsame Identität ist zugleich der Nutzen — die Nutzerkonfiguration überdauert einen Wechsel zwischen Upstream und Fork, und die Abweichung zum Upstream bleibt null (Betroffenengruppe U).

**Auflösungsbedingung:** Zu entscheiden erst, wenn beide Installationen nebeneinander gebraucht werden. Ein eigener `InternalName` trennt dann auch die gespeicherte Konfiguration; ohne Migrationspfad verlöre der Nutzer seine Einstellungen. Die Freigabe liegt beim Auftraggeber.

**Empfehlung: belassen.** Die geteilte Identität kostet nur, wenn beide Fassungen gleichzeitig installiert sein sollen; sie nützt bei jedem Wechsel zwischen ihnen, weil die Nutzerkonfiguration erhalten bleibt.

## Offene Arbeit

### Tanks: Reflexion und Gruppenbarriere auf demselben Raidwide · N

Seine Protokolle vom 01.10.2026 zeigen Abschütteln und Reflexion fast immer auf demselben Raidwide, eine Sekunde auseinander. WrathCombo verteilt beide auf aufeinanderfolgende Raidwides (je 10 s Sperre gegeneinander); The Balance: stapeln für große Treffer, verteilen über mehrere. RSR kennt die gemessene Größe je Raidwide (`HostileCastingAreaPotential`), Stufe 2 der Abwehr nach Treffergröße (Konzept 08) wäre der Ort. Offen: Fallarbeit für alle vier Tanks (Divine Veil, Dark Missionary, Heart of Light). Konzept 09, „Krieger: die übrige Abwehr", Referenzen.

### Flächenheilung um den Wirkenden: Radius gegen Trefferfläche statt Mittelpunkt · N

Ob ein Spieler in einer Fläche steht, entscheidet der Mittelpunkt seines Modells (Akhmorning, „Raiding Fundamentals", 02.10.2026). `GetCanAffects` rechnet Trefferfläche zu Trefferfläche und zählt ein Mitglied am Rand um seinen Trefferkreis zu großzügig mit. Im Kampf: Lux Solaris und andere Flächenheilungen um den Wirkenden können eine verlangte Anzahl erreicht sehen, die die Heilung nicht trifft. Erhebung aller Leser von `GetCanAffects` für freundliche Flächen offen; Konzept 08, „Wann Lux Solaris zündet".

### Six-sided Star und Flamethrower: Regeln belegt, nicht gebaut · N

Quellen gefunden (Job-Guide und The Balance, 02.10.2026; Konzept 14, „Pausen und Phasenenden"):
- *Six-sided Star (Monk):* 780 + 80 je Chakra, 4 s Wiederholzeit; Einsatz als letzter GCD vor Pause, Rückzug aus einer Fläche oder Tod des Ziels, wenn nur einer passt. Braucht die vorhergesagte Pause (BossModReborn) oder die selbstkorrigierte Zeit bis zum Tod; ohne Modul nur über die Zeit bis zum Tod.
- *Flamethrower (Machinist):* Schwellen von The Balance (ohne Gauge ab 2 Zielen über dem Füller, ab 6 über Air Anchor; nie vor Hypercharge, Chain Saw/Excavator, Drill/Bio; keine große Abklingzeit in den 11 s). Bindet 11 s an Ort und Blickrichtung — gegen seine Spielweise nur bei stehendem Rudel ohne angesagte Fläche zulässig.

### Vorlauf: Dragoon und Viper ohne `CountDownAction` · N

Gemessen am 02.10.2026 (Konzept 04, „Was offen bleibt"). Ohne `CountDownAction` wirkt ein Job im Countdown nichts (`CustomRotation_Invoke.cs`, Basis liefert `null`), auch keinen Trank. Zu klären an den Openern von The Balance und an den Wirktexten, ob dort eine Vorlaufaktion fällig ist. Der Trank im Vorlauf ist dabei die Gesamtfrage: `UseBurstMedicine` im Vorlauf rufen nur die vier Heiler, Barde, Maschinist, Dunkelritter und Revolverklinge auf.

### Vorausschau vor dem ersten Treffer: eigene Minderungen rechnerisch · N

Die Vorausschau liest den gemessenen Gesundheitsverlauf; der ist netto und braucht keine Minderungssätze, setzt
aber erst nach dem ersten Treffer ein. Vorher könnte sie die eigenen laufenden Minderungen aus
`DefensiveValues.g.cs` einrechnen (Konzept 08, Tabelle „Ergebnis“). Zu klären: ob der gemessene Anteil eines
angekündigten Treffers (Konzept 13) diese Lücke schon schließt, weil er die beim Messen liegende Minderung enthält.

### Feste Werte im Fork: jeder offene Wert braucht seinen Loop · N, R

**Vorgabe des Auftraggebers:** keine festen Werte im Code, alles im Spiel ableitbar, eine Ausnahme erst nach einem vollständigen Loop zu genau diesem Wert (CLAUDE.md). Die Liste führt `.github/scripts/audit/fixed_values.json`, und `check_fixed_values.py` beziffert die offenen Werte bei jedem Lauf.

**Was im Kampf daran hängt:** Jede dieser Zahlen ist eine Annahme über das Spiel, die niemand nachprüft. Dazu gehören die Vorlaufzeiten der vorausschauenden Abwehr bei Barde, Tänzer, Maschinist und den Tanks (15 und 20 Sekunden statt der Wirkdauer der jeweiligen Aktion), die Stufe, ab der Addle, Feint und Reprisal länger wirken, die Gruppengröße, ab der die Schadenstabelle misst, und die GCD-Zahlen, ab denen Lux Solaris und Tempera Grassa vor dem Ablauf ihres Status fallen. Ändert ein Patch die Größe, bleibt der Code bei der alten Zahl, und nichts schlägt fehl.

**Reihenfolge nach seinem Nutzungsprofil:** zuerst die Werte in den Jobs, die er spielt (Beschwörer, Weißmagier, Tanks, die zentrale Abwehr in `CustomRotation_OtherInfo`), dann der Rest. Die Vorlaufzeiten der Abwehr sind vermutlich über `DefensiveValues.DurationOf` ableitbar; wo ein Merkmal die Dauer im Wirktext leer lässt, braucht es einen eigenen Weg.

### Audit + Code-Review der gesamten Codebasis

Umfang: `RotationSolver.Basic` (48k Zeilen) · RebornRotations (21k) · ExtraRotations (15k) · Updaters (4k) · UI (11k) · Commands/IPC/Data (3k). Der ganze Baum, Upstream-Code eingeschlossen. Phasen 1 bis 4 sind abgeschlossen (AUDIT_LOG A8, A10).

- **Kern tief lesen:** Rest von `DataCenter`; `StateUpdater`, `TargetUpdater`, `ActionTargetInfo`, `BaseAction`/`ActionBasicInfo`, `CustomRotation_Ability`/`GCD`, `Watcher`, `MajorUpdater`, `ObjectHelper`/`StatusHelper` sind gelesen.
- **Rotationen je Job:** Dispatch-Reihenfolge, Gates, Status-IDs, Zielwahl; bisher nur über die Scanner abgedeckt, nicht Datei für Datei.
- **`RotationSolver/UI`** jenseits der Paar- und Totcode-Scans.
- **Dokumentation** in `docs/rotation-flow/14-codebase-audit.md` — **noch nicht angelegt**. Die früher hier genannte Nummer 07 ist seit `07-heal-target-priority.md` vergeben; der Verweis zeigte damit auf ein Dokument, das es unter diesem Namen nie geben konnte.

Der zweite Durchgang mit allen Prüfmitteln ist geführt (A82) und hat die Skripte selbst instand gesetzt; als Messlage für den Rest des Blocks ist er damit verbraucht.

**Stand:** Die Nachprüfung der vorgemerkten Commits, die vorgehen musste, ist abgeschlossen (A286–A290); die Prüfmittel aus jenem Fenster sind nachgeprüft. Von den übrigen offenen Punkten ist dieser hier der einzige, der weder auf eine Entscheidung noch auf eine Spielbeobachtung wartet — die übrigen sind entweder erfasste Fremdbefunde oder brauchen Laufzeitdaten. Sinnvoller Einstieg ist jetzt das Lesen, nicht das Messen: Die Skripte erfassen Muster, die schon einmal aufgefallen sind, und der verbliebene Bestand — der Rest von `DataCenter`, die Rotationen Datei für Datei, die UI — ist genau der Teil, den kein Muster abdeckt.

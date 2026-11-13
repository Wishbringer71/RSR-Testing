# 25 · PvP

**Leitfrage:** Was tut RSR in PvP, wo PvP eigene Aktionen, eigene Zustände und andere Gegner hat, und wo holt es dort
dasselbe Optimum heraus wie in PvE?

Gilt für jeden Job, der in PvP spielbar ist (21 Jobs; Blaumagier und Beastmaster haben kein PvP). Ordnung der Konzepte:
`20-concept-order.md`. Die Regeln für alle Jobs stehen in `03-universal.md`; dieses Konzept nennt, wo PvP von ihnen
abweicht und warum.

*Seine Vorgaben (13.11.2026):* In PvP dasselbe Optimum wie in PvE, zum eigenen Vorteil, obwohl er PvP äußerst selten
spielt. Eine Einschränkung, ob von mir, von Upstream oder aus einem anderen Werkzeug, bleibt nur bei technischer
Notwendigkeit. *Sein Hinweis (13.11.2026):* In PvP nutzt man andere Fähigkeiten; sie sind nicht mit den normalen zu
verwechseln.

## PvP: Grundlagen

*Was gilt:* Jeder Job hat in PvP einen eigenen Satz Aktionen (`Action.IsPvP`), getrennt von seinen PvE-Aktionen. RSR
lädt in einem PvP-Gebiet (`TerritoryType`, `IsPvpZone`) die PvP-Rotation des Jobs (`CombatType.PvP`) und lässt nur
PvP-Aktionen zu (`ActionBasicInfo`: Aktion und Gebiet müssen beide PvP oder beide PvE sein). Eine PvE-Regel, die eine
PvE-Aktion drückt, greift in PvP deshalb ins Leere. Das ist eine technische Grenze, keine Einschränkung; ob es dafür ein
PvP-Gegenstück gibt, steht beim jeweiligen Abschnitt.

*Gemeinsame Aktionen aller Jobs* (Job-Guide, `na.finalfantasyxiv.com/jobguide/`, abgerufen 13.11.2026):
- Guard: 99 % weniger Schaden und Immunität gegen Stun, Heavy, Bind, Silence, Deep Freeze, Rückstoß und Heranziehen,
  4 s, 30 s Abklingzeit. Endet mit jeder anderen Aktion.
- Purify: entfernt Stun, Heavy, Bind, Silence, Deep Freeze und Miracle of Nature, gewährt 2 s Resilience, 2000 MP.
- Recuperate: heilt dich mit 16.000 Potenz, 2000 MP.
- Standard-issue Elixir: LP und MP voll, 4,5 s Wirkzeit, bricht bei Schaden ab.
- Sprint: 50 % schneller, endet mit jeder anderen Aktion.

*Rollenaktionen* (eine je Rolle wählbar): Tanks Rampage, Rampart, Full Swing; Heiler Haelan, Stoneskin II, Diabrosis;
Nahkampf Bloodbath, Swift, Smite; physischer Fernkampf Dervish, Bravery, Eagle Eye Shot; Magie Comet, Phantom Dart,
Rust.

## PvP: Einschränkungen

*Was gilt (A755):* Jede Bedingung im Code, die in PvP etwas anders macht als in PvE, ist einer von drei Fällen. Die
Bestandsaufnahme umfasst Fork, Upstream und alles, was aus anderen Werkzeugen übernommen wurde.

1. *Technisch nötig, bleibt:*
   - Auswahl von Rotation und Aktionen nach PvP oder PvE (`RotationUpdater`, `ActionBasicInfo`, Befehle und
     Oberfläche).
   - Regeln, die PvE-Aktionen, PvE-Zustände oder PvE-Gegner lesen und in PvP nichts finden: Wiederbelebung
     (`CanRaise`; in PvP gibt es keine), Heilung gegen Schaden über Zeit (`HotAgainstDot`, PvE-Heilungen), Schlaf gegen
     Flächen (`SleepToStopAreaCasts`), Limit Break allein (PvE-Limit Break, PvP kennt eine Gruppe immer),
     Figuren zum Heilen (`HealToDefeat`), Tötungsregeln (`KillOrder`), Sperrzustände (`PlayerHasLockActions`) und
     Upstreams Sperre für liegende Strafzustände (`NoCastingStatus`; alle 31 Zustände stammen aus PvE-Bosskämpfen).
   - Guard: Jede Aktion beendet ihn (Wirktext). „PvP Guard control" hält deshalb alle Aktionen, solange er steht.
   - Das Elixier wirkt 4,5 s, länger als jeder GCD. Die Grenze für Fähigkeiten mit Wirkzeit (`01-jobs.md`, W1) hielte
     es für immer zurück. Sie gilt deshalb nur für Wirkzeiten unter dem GCD, in PvE und PvP gleich, statt PvP
     auszunehmen.
2. *Ohne technischen Grund, aufgehoben:*
   - Verkürzung der Animationssperre und Ausgleich der Bildrate (`03-universal.md`, „Weben, Animationssperre und
     Latenz"): meine Ableitung aus Fairness (A753).
   - Wache gegen Strafzustände bei der Landung (`ActionPenaltyWatch`, `03-universal.md`): von Upstreams Sperre
     übernommen, ohne Wirkung in PvP.
   - Gegner mit Stacheln (`03-universal.md`, „Gegner mit Stacheln"): Nebula des Revolverklingen-Kämpfers (3051) trifft
     in PvP jeden Angreifer mit 4.000 Potenz je Angriff. RSR meidet ihn wie jeden Gegner mit Stacheln.
   - Stopp-Markierung: Der Text der Einstellung „Never attack targets with stop markers" bindet, auch in PvP (Upstream
     nahm PvP aus, ohne Grund im Commit).
   - Notfallheilung vor dem GCD (`07-heal-target-priority.md`, „Die Notfallheilung zwischen zwei Zaubern"): Die
     PvP-Rotationen von Astrologe, Gelehrtem und Tänzer haben Heilfähigkeiten, die dort genauso früher gehen.
   - Abfangen eigener Tastendrücke („Intercept your action presses", bis A756 mit „(PvE only)", Upstream 11.08.2025, ohne Grund im
     Commit): gilt in PvP wie in PvE, außer für die gemeinsamen Aktionen (unten, „PvP: Abfangen eigener
     Tastendrücke").
3. *Nicht aufgehoben, meine Grenze:* Upstream sperrt in PvP die Steuerung von RSR durch andere Plugins über IPC, schaltet
   RSR in PvP ab, solange „Auto PvP Series Grind" geladen ist, und wählt in PvP kein Ziel von selbst, wenn keines gewählt
   ist (Upstream 26.07.2025: „Enhanced target logic for botters"). Diese drei Sperren verhindern, dass RSR ohne
   anwesenden Spieler PvP-Partien spielt. Das ginge zulasten der anderen Spieler einer Partie, die gegen oder mit einem
   unbesetzten Platz spielen. Sie sind keine technische Notwendigkeit; ich hebe sie trotzdem nicht auf. Das Spiel eines
   anwesenden Spielers berührt keine von ihnen: Er wählt sein Ziel selbst, und RSR wählt für jede Aktion ihr Ziel wie
   in PvE.

*Im Kampf:* In PvP kommt nach jeder Sofortaktion die nächste Aktion um den gekürzten Netzrest früher. Gegen einen
Revolverklingen-Kämpfer unter Nebula greift RSR für deren 8 s ein anderes Ziel an, statt je Treffer 4.000 Potenz
zurückzunehmen. Ein Heiler mit Heilfähigkeit drückt sie vor dem GCD, wenn ein Mitglied zu fallen droht.

## PvP: Gemeinsame Aktionen

*Was gilt (A756):* Die gemeinsamen Aktionen gehen für jeden Job über einen Pfad (`CustomRotation.TryPvPEmergency`):
Purify, dann Guard, dann Recuperate oder Elixier. Weißmagier und Barde lösen Bannzustände mit eigener Aktion
(Aquaveil, The Warden's Paean) und eigener Purify-Zeile.
- *Guard* geht, wenn deine vorhergesagte Gesundheit bis zur Landung einer Aktion, die du jetzt beginnst, unter
  „Use Guard below this HP" fällt (`GetForecastPlayerHealthRatio`, dieselbe selbst nachgesteuerte Vorhersage wie für die
  Heilziele in `07-heal-target-priority.md`). Bisher las die Regel die aktuelle Gesundheit: Wer unter Beschuss von drei
  Gegnern bei 30 % stand, bekam Guard erst bei 15 % und oft nicht mehr rechtzeitig. Kein Guard, solange er nichts
  bringt oder nicht geht: unter Hallowed Ground (1302), Undead Redemption (3039), Sky High (3180, nicht anvisierbar),
  Hardened Scales (4096), Inner Release (1303, „Cannot execute Guard"), Unguarded (3021) und auf einer Maschine
  (Mounted, 1420). Bisher waren nur Undead Redemption und Inner Release ausgenommen; unter Hallowed Ground hielt Guard
  danach 4 s lang jede Aktion zurück („PvP Guard control"), ohne Schaden zu verhindern.
- *Recuperate* heilt 16.000 LP (Job-Guide, seit 7.5; 1 Potenz ist in PvP 1 LP) für 2000 MP. Es geht, wenn dir mindestens
  seine Heilung fehlt; die Menge liest RSR aus dem Wirktext des Clients („Cure Potency"), nicht aus einer Zahl im Code.
  Bisher stand dort 15.000, die Heilung vor 7.5. Es lässt das MP für ein Purify übrig (beide Kosten aus dem Aktionsblatt),
  außer du stehst in der kritischen Klasse. Bisher verbrauchte Recuperate das MP bis zuletzt. Ein Stun danach ließ sich
  nicht mehr lösen, und ein Spieler ohne Purify stirbt im Stun.
- *„Hold GCDs for heals below 50% HP"* hält die GCDs nur, solange Recuperate auch gehen kann. Mit der Reserve wäre
  sonst bei 2000 bis 4000 MP jeder GCD gehalten worden, ohne dass eine Heilung kam.
- *Purify* löst Stun, Silence, Deep Freeze und Miracle of Nature ab Werk, Heavy und Bind nicht (Upstreams Voreinstellung;
  beide halten keine Aktion auf und kosten dieselben 2000 MP wie eine Heilung).
- *Elixier* (4,5 s, bricht bei Schaden ab): nur ohne Gegner in Reichweite und unter einem Drittel LP oder MP, wie
  bisher.

*Optionen, Guard:* (a) Nullvariante, aktuelle Gesundheit; (b) Zahl der Gegner, die dich anvisieren, wie BossModReborn
(„Auto": zwei Gegner unter 75 %, sonst 33 %); (c) Vorhersage. (b) zählt Absichten, keinen Schaden: Ein Heiler, der dich
anvisiert, zählt mit. (c) misst den Schaden selbst und steuert ihre Vorhersage gegen den Verlauf nach. Gewählt: (c), weil
sie die Lage aus (b) mit erfasst, sobald der Schaden fließt. *Preis:* Ein Burst aus voller Gesundheit, der keinen
Verlauf hat, bleibt unvorhergesagt; dort greift Guard wie bisher erst an der Schwelle.

*Falsifikation:* „Ausgeliefert, und nichts ändert sich": wenn die Vorhersage in PvP schlechter wäre als in PvE. Sie
liest den Gesundheitsverlauf, der in PvP genauso entsteht, und rechnet ihren Fehler je Figur heraus
(`ScoreTtkForecast`). „Option falsch": Guard hält 4 s jede Aktion; geht es früher als bisher, verliert der Spieler diese
4 s öfter. Gewichtet: Es geht nur, wo die Vorhersage ihn unter die Schwelle bringt, also wo er ohne Guard fiele.

## PvP: Ziele unter Guard und Unverwundbarkeit

*Was gilt (A756):* Ein Gegner unter Guard nimmt 99 % weniger Schaden. RSR greift ihn nur mit Aktionen an, die Guard
übergehen („Ignores the effects of Guard", `IgnoreGuard`), solange Guard die Aktion überdauert; „Cancel casts when the
target gains Guard" bricht ein Wirken auf ihn ab. Guard hat zwei Zeilen im Statusblatt, 3054 und 3673, beide
„Guard" mit demselben Text. RSR prüfte nur 3054. Jetzt gelten beide.

Ein Ziel unter Guard darf eine Aktion wählen, deren Wirktext sagt, dass sie mit Guard umgeht: „Ignores the effects
of Guard" (Smite, Eagle Eye Shot, Drill, Seiton Tenchu, Zantetsuken …), „Removes … Guard" (Full Swing, Meteodrive) oder
„defensive bonus of Guard" (Shield Smite, Chain Stratagem halbieren ihn). RSR liest das aus dem englischen Text des
Clients (A757); das Flag `IgnoreGuard` je Aktion gilt daneben weiter. Bisher fehlte es Seiton Tenchu, Zantetsuken,
Meteodrive und Shield Smite: RSR ließ für sie gerade die Ziele aus, gegen die sie am meisten bringen. Der Gelehrte wirkt
Chain Stratagem nur auf ein Ziel unter Guard.

*Unbelegt:* wann das Spiel 3673 statt 3054 vergibt. *Preis, wenn 3673 schwächer schützt:* RSR hielte einen Angriff auf
ein Ziel zurück, das ihn größtenteils nähme. Der Text beider Zeilen ist gleich; ohne Beleg für einen Unterschied
gelten beide als Guard.

## PvP: Bewegung

*Was gilt (A756):* Jede PvP-Aktion, die dich bewegt (`AffectsPosition` im Aktionsblatt), trägt ihre Bewegungsart wie in
PvE (`05-action-coverage.md`, „Bewegungsfähigkeiten"). Damit gilt „Only use damaging gap closers within this distance"
(ab Werk 3 Yalm) auch in PvP, und mit „Check movement actions with BossModReborn (Experimental)" wird der Weg geprüft.
- *Sprung auf das Ziel mit Schaden* (`HostileMovingAttack`): Onslaught, Primal Rend, Plunge, Rough Divide, High Jump,
  Forked Raiju, Fleeting Raiju, Hissatsu: Soten. Dazu wie bisher Intervene, Seraph Strike und Crimson Cyclone.
- *Sprung zu einem Gruppenmitglied* (`FriendlyMovingForward`): Guardian.
- *Zu Freund oder Feind* (`HostileFriendlyMovingForward`): Thunderclap, Slither, Icarus.
- *Fester Sprung nach vorn* (`FixedDistanceMoveForward`, Weite aus dem Wirktext): En Avant 10 y, Smudge 15 y.
- *Rückwärtsschritt mit Angriff* (`HostileAttackBackstep`): Displacement 15 y, Repelling Shot 10 y. Repelling Shot trug
  bisher `HostileMovingAttack`: RSR prüfte den Punkt am Ziel statt 10 y hinter dir.
- *Ort am Boden* (`GroundTargetMove`): Shukuchi.

*Wie die Grenze greift:* Der Typ allein begrenzt nicht; die Zielwahl für `HostileMovingAttack` filtert nur Lebende
(A358). Die Grenze sitzt deshalb wie bei Intervene in PvE in der Basis jeder dieser Aktionen
(`setting.CanTarget = GapCloserAllowed(…)`), für jede PvP-Rotation und jeden Pfad. Ausnahme Hissatsu: Soten: Die
Samurai-Rotation hat den Schalter „Allow Hissatsu Soten to be used on any target regardless of distance", und der Text
der Grenze nimmt solche Schalter aus; dort prüft die Rotation die Grenze an ihrer übrigen Aufrufstelle. Seraph Strike
und Crimson Cyclone trugen den Typ schon, aber nicht die Grenze: Der Weißmagier sprang aus 20 y in die gegnerische
Gruppe. Limit Breaks, die dich auf ihr Ziel tragen (Meteodrive, Seiton Tenchu, Zantetsuken, World-swallower; erkannt am
Wirktext: „Rushes target", „charges forward", „jumping … attack"), gehen ebenso nur innerhalb der Grenze.
`check_movement_types.py` misst seit A756 auch PvP.

*Im Kampf:* Bisher sprangen Krieger, Dunkelritter, Revolverklingen-Kämpfer, Weißmagier, Dragoon, Ninja und Samurai aus bis zu 20 y
auf ihr Ziel, sobald die Aktion bereit war – mitten in die gegnerische Gruppe, ohne Prüfung. Jetzt gehen diese Sprünge
aus der Nähe (ab Werk unter 3 y) mit ihrem Schaden, auf „Move Forward" oder auf deinen Tastendruck.
*Preis:* Ein Nahkämpfer, der weiter weg steht, verliert den Schaden des Sprungs, bis er herangeht; Forked Raiju und
Fleeting Raiju bringen ihren Stun erst aus der Nähe. Er wählt die Grenze selbst. *Grund:* seine Spielweise, „Nichts
empfehlen, was ihn für Schaden aus einer sicheren Position holt"; sie gilt in PvP wie in PvE.

## PvP: Limit Break

*Was gilt (A756):* In PvP ist der Limit Break persönlich: jeder Job hat einen eigenen, die Leiste füllt sich je
Spieler (Job-Guide: „Gauge Charge Time" 60 bis 135 s). Bisher drückte RSR ihn nie: Die Basisklasse jedes Jobs führt ihn
(`LimitBreakPvP`), aber nichts las ihn. Jetzt fragt der Fähigkeitspfad in PvP den Limit Break des Jobs, sobald die Leiste
voll ist (das Spiel lässt ihn vorher nicht zu), mit einer Bedingung je Art:
- *Hinrichtung eines Ziels:* Marksman's Spite (40.000), Meteodrive (12.000 und 12.000 nach 3 s): nur, wenn die Potenz das
  Ziel tötet, gemessen an seinen LP samt Barriere; die Potenz aus dem Wirktext des Clients. Seiton Tenchu: auf ein Ziel
  unter 50 % LP („incapacitating foes whose HP is below 50%"). Zantetsuken: auf ein Ziel mit Kuzushi („deals damage equal
  to 100% of their maximum HP"), sonst nicht.
- *Auf ein Ziel gerichtet* (das Blatt erlaubt ein feindliches Ziel mit Reichweite): World-swallower, Afflatus
  Purgation, Southern Cross, Primal Scream, Zantetsuken ohne Kuzushi nicht: auf ein Ziel, das sie in jedem Fall trifft.
- *Kreis um dich:* Relentless Rush (6 y), Tenebrae Lemurum (10 y), Contradance (15 y), Celestial River (15 y),
  Seraphism (30 y), Sky High (über Sky Shatter, 10 y): wenn ein Gegner im Radius des Blatts steht.
- *Linie, Kegel oder Ort am Boden von dir aus:* Eventide (Linie 30 y; die LP fallen auf 1, Undead Redemption hält sie
  10 s darüber), Summon Bahamut, Advent of Chocobastion: mit der Gegnerzahl ihrer Einstellung („AoE Count", ab Werk 3,
  Chocobastion 1), weil sie nur treffen, was in der Richtung oder am Ort steht.
- *Verstärkung nur für dich* (kein Radius im Blatt): Final Fantasia, Soul Resonance: im Kampf mit einem Gegner in
  Reichweite der Rotation.
- *Schutz:* Phalanx, Mesotes: wenn du in der kritischen Klasse stehst (oben, Guard).
Die Aktionen, die ein Limit Break freischaltet (Blade of Faith, Disesteem, Star Prism, Flare Star …), führen die
Rotationen schon.

*Optionen:* (a) Nullvariante, der Spieler drückt ihn selbst; (b) sobald bereit, ohne Bedingung; (c) Bedingung je Art.
(b) verschwendet die Hinrichtungen auf volle Ziele und die Flächen ins Leere. (a) ist für ihn, der PvP selten spielt,
der Verlust des stärksten Knopfs. Gewählt: (c). Drückt er ihn selbst, fängt RSR ihn nicht ab (Upstreams Sperre für
Limit Breaks im Abfangen, A629).

*Gemessen statt angenommen (A759):* Die Potenz einer Hinrichtung wird mit dem Schaden je Potenz verrechnet, den deine
eigenen schlichten Treffer im Gebiet gezeigt haben (`OwnHitWatch.DamagePerPotency`, gemessen für jeden Job seit A507).
Darin stecken die Minderungen der Gegner und deine Verstärkungen. Treffer auf ein Ziel unter Guard zählen in PvP nicht
mit: 99 % weniger für 4 s sind kein Maß für die übrigen Ziele. Vor dem ersten gemessenen Treffer gilt 1 Potenz = 1 LP.
*Preis:* Der Faktor ist ein Mittel über alle Ziele des Gebiets; ein Ziel mit eigener starker Minderung (Rampart −50 %)
überlebt eine Hinrichtung, die nach dem Mittel tötet. Die Hinrichtung geht dann trotzdem und trifft mit voller Potenz.

## PvP: Abfangen eigener Tastendrücke

*Was gilt (A756):* „Intercept your action presses" gilt in PvP wie in PvE: Ein gedrückter Jobknopf geht in die
Warteschlange, und RSR setzt ihn an der nächsten passenden Stelle ein. Ausgenommen sind die gemeinsamen Aktionen Guard,
Purify, Recuperate, Elixier und Sprint: Sie gehen, wie du sie drückst. In der Warteschlange warteten sie auf das
nächste Webfenster, ein Guard gegen einen angekündigten Burst um bis zu einen GCD. Upstream nahm PvP als Ganzes aus,
ohne Grund im Commit.

## PvP: Jobs

Befunde je Job (Rotationen `RebornRotations/PVPRotations`, gegen den Job-Guide vom 13.11.2026). Die Rotationen sind
Prioritätslisten von Upstream; was hier nicht steht, ist geprüft und ohne Befund.
- *Gelehrter:* Deployment Tactics trug Biolysis (3089) zusätzlich als Zustand des Spielers; ohne Wirkung, entfernt.
  Die Prüfung auf Biolysis am Ziel bleibt.
- *Rotmagier:* Enchanted Zwerchhau und Enchanted Redoublement trugen die Schadenszustände auf dem Ziel (3238, 3239) als
  Zustand des Spielers (`StatusProvide`). Ohne Wirkung, weil der Spieler sie nie trägt (scan11, README des Prüfordners).
  Entfernt, nicht auf die Seite des Ziels verschoben: Dort hielte die Prüfung die Kombo an, solange der Schaden des
  vorigen Durchgangs noch läuft.
- *Paladin:* Holy Sheltron ist eine Barriere (8.000) für 4 s; bleibt sie stehen, schlägt sie mit 150 % ihres Rests auf
  Gegner in der Nähe und gibt 15 % Schaden. Jede Seite des Ausgangs nützt. Bisher ging sie nur, wenn RSR Abwehr
  verlangte. Jetzt geht sie im Kampf, sobald ein Gegner in Nahkampfreichweite steht (der Wirkradius des Ausbruchs steht
  in keinem Text; „all nearby enemies"). Die Rotation führt Smite, eine
  Rollenaktion der Nahkämpfer; für den Paladin bleibt sie wirkungslos (`StatusNeed` der Rollenwahl).
- *Weißmagier, Barde:* lösen Bannzustände mit eigener Aktion vor Purify; ohne Befund.
- *Alle Nahkämpfer:* Smite bei höchstens 25 % LP des Ziels: Das ist die Grenze aus dem Wirktext („reaching its maximum
  value when the target has 25% HP or less"), keine eigene Zahl. Swift geht bei Bereitschaft (4 s Immunität und Tempo);
  ohne Beleg für einen besseren Zeitpunkt unverändert.

## PvP: Wechselwirkungen und Synergien

*Was gilt (A757):* Die Wechselwirkungen sind je Ebene erhoben (`CLAUDE.md`, „Universell zuerst"): alle Jobs, Rolle,
Untergruppe, Job, dazu zwischen den Spielern einer Gruppe. Je Paar: dieselbe Ressource, derselbe Zeitpunkt, dasselbe
Ziel. Erhoben an den Wirktexten aller 21 Jobs (Job-Guide) und den PvP-Rotationen; die Spalte „Stand" sagt, was RSR tut.

*Alle Jobs*
| Wechselwirkung | Wirkung im Kampf | Stand |
|---|---|---|
| MP: Recuperate, Purify, Haelan (je 2000) | Ein Heilen bis zum letzten MP lässt einen Stun ohne Purify | Recuperate und Haelan lassen das MP für ein Purify, außer in der kritischen Klasse (Haelan: des Ziels) |
| Guard endet mit jeder Aktion | Jede Aktion, auch Recuperate, ein Limit Break oder ein abgefangener Tastendruck, beendet ihn | „PvP Guard control" hält alles; Guard steht vor Recuperate |
| Guard und Schutz-Limit-Break | Phalanx (10 s unverwundbar, Gruppe −33 %), Mesotes (Schaden aus der Zone genullt), Sky High (4 s unerreichbar, dann 24.000 Barriere) schützen länger oder mehr als Guard (4 s, nur du) | Wo Guard fällig ist (oben), geht ein bereiter Schutz-Limit-Break vor Guard; Phalanx und Mesotes gehen außerdem in der kritischen Klasse; danach lässt die Liste `GuardWastedStatuses` Guard unter Hallowed Ground und Sky High aus |
| Guard am Ziel und Aktionen, die ihn übergehen, entfernen oder halbieren | Seiton Tenchu, Zantetsuken, Smite, Eagle Eye Shot, Drill übergehen ihn; Full Swing und Meteodrive entfernen ihn; Shield Smite und Chain Stratagem halbieren ihn | Ob eine Aktion ein Ziel unter Guard wählen darf, liest RSR aus ihrem Wirktext („Ignores the effects of Guard", „Removes … Guard", „defensive bonus of Guard"); bisher nur über ein Flag je Aktion, das Seiton Tenchu, Zantetsuken, Meteodrive und Shield Smite fehlte |
| Resilience, Guard und andere Immunität am Ziel gegen Bannzustände | Ein Stun verpufft, der Schaden der Aktion nicht | Eine Aktion, deren Wirktext einen Bannzustand setzt („Additional Effect: Stun/Bind/Heavy/Silence/Deep Freeze", Miracle of Nature), wählt unter ihren Kandidaten zuerst einen ohne Immunität; nur ohne ihn trifft sie ein immunes Ziel für ihren Schaden (A757). Immun ist ein Ziel unter einem Zustand, dessen Text die Bannzustände aufhebt („nullified", „immunity" zu Stun oder zu den Zuständen von Purify): Resilience, Guard, Inner Release, Hardened Scales, Swift und weitere, ohne Liste |
| Unverwundbarkeit am Ziel (Hallowed Ground, Undead Redemption) | Schaden geht ins Leere | „Ignore invincibility in PvP" (ab Werk aus): RSR lässt solche Ziele aus |
| Rückgabe am Ziel (Nebula) | 4.000 je Angriff zurück | Gegner mit Stacheln, `03-universal.md` |
| Hinrichtung und Tötung | Smite lädt nach einem Kill neu, Seiton Tenchu darf erneut, Sole Survivor gibt 15 % LP und MP | Das Spiel lässt die Aktion wieder zu; RSR prüft sie jeden Frame und drückt sie, sobald sie geht |
| Sprint und Hidden enden mit jeder Aktion | Der Spieler verliert Tempo oder Tarnung | Sprint geht außerhalb des Kampfes oder ohne Ziel („Sprint with no target"); die Ninja-Rotation drückt unter Hidden nur Assassinate |

*Rolle*
- *Tanks:* Rampage (+25 % Schaden genommen, 10 s, um dich) und Full Swing (entfernt Guard) bereiten den Schaden der Gruppe
  vor; beide stehen früh in der Fähigkeitsreihe jeder Tank-Rotation. Full Swing darf jetzt ein Ziel unter Guard wählen (oben).
- *Heiler:* Haelan teilt das MP mit Purify und Recuperate (oben). Diabrosis (−33 % Heilung am Ziel) und Stoneskin II
  (Barriere für die Gruppe) ohne Wechselwirkung mit einer anderen Regel.
- *Nahkampf:* Smite übergeht Guard und lädt nach Kill oder Assist neu. Die Sprünge stehen unter der Grenze für Sprünge
  (oben, „PvP: Bewegung").
- *Physischer Fernkampf:* Bravery geht auf dich (`TargetType.Self`); auf ein anderes Mitglied brächte es dessen Schaden,
  nicht deinen (seine Vorgabe: eigener Vorteil). Eagle Eye Shot übergeht Guard.
- *Magie:* Phantom Dart (+25 % Schaden genommen, 5 s) geht als zweite Fähigkeit nach Rust, vor den übrigen; Rust (−33 % Schaden und Heilung des
  Ziels) ohne Wechselwirkung.

*Untergruppen:* Keine Regel gilt für genau eine Untergruppe. Was Nahkämpfer teilen (Sprünge), steht bei der Rolle.

*Zwischen den Spielern einer Gruppe:* Schutz und Heilung, die auch auf ein Mitglied gehen (Heart of Corundum, The
Blackest Night, Aquaveil, Haelan, Adloquium, Aspected Benefic, Cure II), wählen ihr Ziel über die allgemeine Zielwahl
wie in PvE: Heilungen das Mitglied mit dem größten Bedarf (`07-heal-target-priority.md`), Schutz das Mitglied, das Gegner
angreifen (`FindBeAttackedTarget`), Aquaveil das Mitglied mit einem Bannzustand. Keine PvP-eigene Regel nötig.

Die feindliche Zielwahl folgt in PvP der Zielart des Spielers („Targeting Type", ab Werk
zuerst die wenigsten LP). Geprüft und nicht geändert: ein eigener PvP-Fokus nach effektiver Gesundheit und Verwundbarkeit.
*Gründe:* Die Einstellung bindet; die Voreinstellung wählt schon das Ziel, das am schnellsten fällt; die Verwundbarkeit
nennt das Statusblatt ohne Zahl („Damage taken is increased"), ein Gewicht wäre erfunden. Ein Ziel unter Guard wählen
Aktionen, die ihn nicht übergehen, ohnehin nicht.

*Falsifikation:* „Ausgeliefert, und nichts ändert sich": für Guard-Aktionen, wenn der Text im Client anders lautet als im
Job-Guide. RSR liest den englischen Text des Clients, wie für die Potenz; die Gegenprobe ist die Liste der Aktionen mit
Flag, die alle den Satz tragen. „Option falsch": Ein Schutz-Limit-Break vor Guard verbraucht die Leiste für einen
Notfall, den Guard allein überstünde. Gewichtet: Er geht nur in der kritischen Klasse, wo die Vorhersage den Tod zeigt,
und schützt bei Phalanx und Mesotes die Gruppe mit.

## PvP: Prüfmittel

Die Prüfskripte nahmen PvP aus, weil PvP außerhalb seines Profils lag. Seit A756:
- `check_movement_types.py` misst auch die PvP-Aktionen mit `AffectsPosition`.
- `check_setting_tooltips.py` prüft auch die Einstellungen der PvP-Rotationen.
Die übrigen (`check_channel_locks.py`, `check_heal_threshold_forecast.py`, `generate_action_matrix.py`) bleiben bei
PvE: Kanäle gibt es in PvP nicht (alle PvP-Wirkzeiten liegen unter 2,5 s oder sind das Elixier), die Heilschwellen der
PvP-Rotationen sind Teil der offenen Rotationsarbeit, und die Matrix vergleicht PvE-Rotationen mit The Balance.

## PvP: Quellen

Job-Guide (`na.finalfantasyxiv.com/jobguide/<job>/`, PvP-Abschnitte, abgerufen 13.11.2026, Stand 7.55); xivapi v2
(Aktionen mit `IsPvP`, Status); consolegameswiki „PvP" (1 Potenz = 1 LP). Werkzeugquellen, je Commit:
WrathCombo a457838 (09.11.2026, PvP aller Jobs, rechnet noch mit Recuperate 15.000 und 2500 MP), BossModReborn 8ad17f97f
(`RolePvPUtility`: Guard nach Zahl der Angreifer, Guard früh beenden; vier PvP-Rotationen). Gefunden, ohne Regel für
RSR: priestkos/FrontlineAdvisor 301a95e (12.11.2026; „Fokus" ist dort die Großkompanie, die in Frontline zuerst
aufzuhalten ist, eine Strategieanzeige, keine Zielwahl je Aktion); nur nach ihrer Beschreibung erfasst: wrath16/PvpStats
(Statistik), Petra105/PvPTopPlates (Anzeige), exatrines/AutoFrontline (Automatisierung ohne Spieler; nicht übernommen).
*Unbelegt:* XeldarAlz/FFXIV-AutoPVPLimitBreak – eine Websuche beschrieb Zielwahl nach niedrigster effektiver
Gesundheit; das Repository war am 13.11.2026 nicht erreichbar (Klon abgewiesen, Seite 404).

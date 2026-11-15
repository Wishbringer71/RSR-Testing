# Changes since 7.5.8.2+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## Gaze attacks

- **RSR turns away from a gaze just before it lands.** Until now RSR knew no gaze at all and pressed into them like
  any other cast. A gaze is now an action with the sheet's gaze mark, an enemy action of the same name (Angra Mainyu's
  second Mortal Gaze in the World of Darkness comes from a helper without the mark), or one of the 78 gazes
  BossModReborn's fight modules name (Beatrice's Death Foreseen among them, 22 without the mark, 3 inverted). RSR keeps
  fighting until the measured server delay plus a frame before the gaze lands, then turns you to the middle of the
  widest safe stretch through the game's own facing call, as BossModReborn does. Setting
  `Turn away from gaze attacks`, **on by default**; without a measured delay or with it off, actions that would turn
  you into the eye wait for the whole cast instead.
- **Where the eye is and which way is safe come from the game.** The eye sits at the cast's place for ground-targeted
  gazes, otherwise on the caster. Inverted gazes (look at the eye) are read from their names (Kefka's Ave Maria), from
  statuses whose text says to look at or away from an enemy (Nymeia), from "look" and "no look" markers on you (the
  Windurst alliance raid's last boss) and from a party member whose status makes them the eye (Glassy-eyed).
- **You keep hitting while you look away.** An action whose target lies within 45 degrees keeps your facing when it is
  safe, or turns you within that cone; the game's own option to face the target is switched off for that one press. A spell still casting
  turns only within 75 degrees of its target, the limit BossModReborn measured, so it lands and the gaze misses. Every
  action the game runs passes this guard – RSR's press, the game's queue and your own press. An action waits only when
  no safe facing reaches its target, above all on the eye's own caster. Dashes and leaps are checked on the exact
  direction to where they land.
- **RSR learns in the fight whether turning helped.** Each gaze is judged by its own effect packet. A harmful status
  on you after RSR turned is a failure: from then on that gaze only holds actions back; after a second failure RSR
  leaves it alone, per cast row and until RSR is loaded again. Where other members hit by the packet faced the safe way
  and none faced the eye as RSR placed it, the model counts as refuted and RSR leaves the gaze at once.
  `DefenseTrace.log` writes "gaze: avoided" and "gaze: hit despite" for each one.

## Animation lock and weaving

- **RSR replaces XivAlexander, NoClippy and BossMod's action tweaks, without a value to set.** The delay to each of
  your own actions' answers is measured always, per zone. The animation lock is cut by the zone's lowest measured
  round trip plus a controller's extra that rises with each answer and falls with each request the server leaves
  unanswered; it never takes off more than the zone's longest delay. The weave window is the action's server lock
  plus what the cut leaves, and abilities that still fit go after "Action ahead" too. The frame rate adjustment is
  capped at the zone's mean frame time. The three fixed settings for this are gone; the rest are on by default.
- **Other tools are taken over, not fought.** BossMod's two lock tweaks are switched off while RSR runs and given back
  as they were; NoClippy only if it was on. While XivAlexander runs, or another tool changes the lock between packet
  and game, RSR holds its own cut until the next zone and leaves your setting as it is.
- **A trace line per fight** names delay, rest, cut, weave window, frame time, weaves per GCD, GCDs a lock held back,
  idle GCD time and requests without an answer. Only instant requests RSR saw count as unanswered; sequences the game
  sends on its own had held the cut at 0 to 1 ms.

## Stability

- **The game no longer closes at the first action of a fight.** With "Turn away from gaze attacks" on, a start value
  was checked after use and every fight's first action closed the game. Every hook RSR places now runs only the
  game's original outside a guard that catches every fault, and reports a fault once per place ("hook fault").
- **No crash while a Crucible window opens.** The window trace read texts and the value array of a board window
  directly from memory; a value pointing to unmapped memory closed the game. Both are read through Dalamud's
  SafeMemory now.
- **A board whose way leads back no longer overflows the stack**, and taking over the other tools' lock functions runs
  on the next frame instead of inside the game's action update.
- **Every exception in RSR's code is reported once per place,** caught or not, in `DefenseTrace.log` and with its
  stack in the Dalamud log. Of 154 places that catch one, 60 only wrote a warning your log level does not show.

## Every job

- **No defence at a training dummy.** A dummy targets you and never hits, and RSR counted that as being attacked:
  every role kept its single-target defence open, and a tank alone pressed its mitigation on cooldown. An enemy now
  counts as attacking only once it hit you in this fight. The Warrior's Bloodwhetting alone in the party follows the
  same rule: on cooldown while an enemy attacks and no healer lives, never at a dummy.
- **Area casts of helpers are dodged and counted.** The dodge checks, the hit window around you and the cast block
  before a certain dodge saw only targetable enemies; many raidwides and lines come from helpers. They read every
  casting enemy figure now, and a listed area cast blocks only where it reaches you and leaves a living member
  outside.
- **Cones and lines point where the game casts them.** They were turned towards the cast's target each frame and
  counted that target as hit; they now keep the direction the game stores for the cast, and ground circles sit on the
  cast's location instead of the caster. A same-named row without a cast time counts with its own shape where it has
  an effect range: Claw and Tail is cast as a line ahead and hits as a cone behind.
- **Tinctures by measured damage, not by habit.** FFLogs' best pulls of all 21 jobs on three current savage bosses
  were measured with the tincture taken out. In a duty not yet cleared, the first burst's tincture waits unless the
  last pull there reached the second burst, so a wipe while learning costs no tincture. Reaper, Viper, Bard, Dragoon
  and Dancer carry more damage in their two-minute burst than in the opener: their first tincture waits for it where
  that gives as many tinctures over the expected fight. The Reaper's own potion options from upstream asked the same
  again and are removed.
- **Positionals.** While True North covers the next GCD, or the target can be hit fully from anywhere, RSR names no
  side to BossModReborn's AI and Avarice, so they no longer walk you for nothing. RSR learns from the game's damage
  entry which side each positional press really hit, and where the game judged against RSR's angle it keeps a band in
  that area where True North goes only at full charges.
- **Every check sees the same answer on a dead member who is still moving.** The movement of a body was answered only
  to the first question per frame; the raise target choice and the trace asked later and read it as standing.
- **Cooldowns that stood ready are traced.** One line per fight names every cooldown that stood ready longer than a GCD
  while RSR played, how long, and why the rotation refused it, or "not asked".

## Jobs

- **Black Mage: Manafont no longer waits for Thunderhead.** It was refused for the 30 s Thunderhead runs after every
  Fire III: two uses in seven minutes instead of four.
- **Reaper:** the opener tincture follows the tincture rule above for every job.

## PvP

- **Restrictions without a technical need are lifted.** The animation lock cut, the frame rate adjustment, the penalty
  landing watch, the spiked-enemy avoidance, the stop marker filter and the emergency heal ahead of the GCD apply as in
  PvE. Guards against unattended play stay.
- **Limit Break.** Every job's PvP Limit Break goes, with a condition per kind: finishing blows only where the stated
  potency kills (Marksman's Spite and Meteodrive by your measured damage per potency; Zantetsuken on a target with
  Kuzushi), shields in danger and before Guard (Phalanx, Mesotes, Sky High, Eventide), area ones by the enemies they
  reach. Follow-up actions on the button (Terminal Trigger, Sky Shatter) are left to their rotation.
- **Guard, Recuperate, Purify.** Guard goes on the health forecast and not where it is wasted (Hallowed Ground, Undead
  Redemption and others). Recuperate and Haelan keep the MP for one Purify unless you are in danger. Actions that
  ignore, remove or halve Guard may pick a guarded target; crowd control prefers targets without immunity.
- **Leaps and intercept** carry their movement type and the gap closer limit; Guard, Purify, Recuperate, Elixir and
  Sprint pass through as you press them.

## Blue Mage: Masked Carnivale

- **Control effects go where they work.** The stage data marks enemies *immune* to a control effect, and RSR read it
  the other way round, holding back exactly the enemies it works on. Interrupts and the Vibe Check are read from the
  stage guides, and the set adds a filler for an enemy no damaging spell can hurt.

## Beastmaster and the Occult Crucible

- **Goal of a run: Level, Safe win, Score.** The team window offers the goals that hold on this board and stores your
  choice (`Goal of a Crucible run`). Level fills every place below the board's cap, or the next board's recommended
  rank where higher, lowest rank first; Safe win takes the lowest team that passes safely; Score the strongest, as few
  as are not endangered. On the board every goal plays safe, then for points. Level is offered only while a familiar
  of known rank stands below that limit, Score without a familiar on the cap only while its team is measured not
  endangered.
- **The true rank of each familiar is read.** The game function took two arguments more than documented, so every
  rank read 0 and no familiar ever lagged behind. RSR now reads it correctly and also learns each familiar's rank from
  the board's result window, rising only.
- **The team.** Roles go to familiars the horn choice sends in; against danger a swap takes the lowest familiar that
  fights stronger, measured by HP at its fighting rank. The board's content row is found again, so board bonuses,
  team rank and losses per board count on the board.
- **Shop and items: only what gets used.** Healing and protection count only the part of the way's demand that would
  take you below the potion line, smoke bombs only what the route would flee from; gear you already wear, from any
  window that lists it, is no longer recommended. Damage, TP and buff items wait while an item bonus stands, except
  what the final fight's last GCD buys. A fight never measured counts as the board's heaviest.
- **The next horn comes first** after a familiar leaves, ahead of other abilities.
- **The way shows when it is settled**: only once no window of the board is open (shop, treasure, loot after a fight,
  campsite, result, team screen) and you can move.
- **"Team ready"** replaces Apply when the team already is the recommended one.

## Seeing what the rules decide

- **Recommendation windows fit the screen.** The team window before board 2 ran past the screen height and its button
  could only be reached by scrolling. No window is taller than the screen now, the action and its status stay below
  the scrolled list, and long lists spread over columns.
- **Code structure.** The fork's own members of 27 more upstream classes moved into partial files per concept; no
  signature changed.

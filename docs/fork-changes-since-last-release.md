# Changes since 7.5.7.3+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## Hits from a boss's helpers and parts are read

Many bosses hit through untargetable figures - helpers, or parts of the boss. RSR read only one kind of them, and in a
dungeon boss fight every hit on the party came from another kind: none was measured, its raidwides never joined the
AoE list, and a tankbuster marker before a hit of 89 % of your HP was learned as one that no hit follows. All enemy
kinds are read now, casts and hits alike. A marker learned wrongly this way is taken off that list by the next hit
after it.


## Help for the one a tankbuster is aimed at

When a tankbuster marker sits on another party member - most often the other tank - or an enemy casts a known
tankbuster at them, every tank now puts its help for others on them before the hit: Intervention (Paladin), Nascent
Flash (Warrior), The Blackest Night (Dark Knight), Heart of Corundum (Gunbreaker). Healers put their single-target
mitigation on the same member instead of whoever the boss is attacking. Before, Heart of Corundum and Aurora set to
"Only use on tankbuster targets prioritizing self" never reached the other tank, and reached you only with a marker;
Intervention went to whoever the boss was attacking, also on a pull, and only to a tank in stance. New options for the
Warrior and the Dark Knight, and "Use Intervention on CoTank during tankbusters", are on by default. Help is not given
to a member who takes the hit under Hallowed Ground or Superbolide while "Hold mitigation under invulnerability" is on.

## Sleep against an enemy's area attack (experimental)

Healers and casters have no interrupt. With "Use Sleep or Repose to stop an enemy's area attack (experimental)", on
by default, they put Repose or Sleep on an enemy that is not a boss while it casts an area attack reaching you or two
party members, if enough of the cast is left for the sleep to land. No game text says that a sleep interrupts;
community sources say it does, and the sleep status locks actions as a stun does. Each attempt costs one GCD. An enemy
kind that resists, and an attack that goes off although the sleep landed, are not tried again in that session.
Swiftcast is never spent on it. DefenseTrace.log records every attempt and whether the attack stopped.

## Healers: a single-target heal stops only once its target has been healed

"Stop single target GCD healing after reaching threshold. (EXTREMELY Experimental)", off by default, had no effect
before; since upstream 7.5.7.5 it breaks off casts. It now does so only in the case its text names: the target of the
heal needed it when the cast began, someone healed them during the cast, and they no longer need it. A heal begun for
a falling trend keeps going when the trend levels out with nobody healed, and a barrier cast on a tank without need -
the Adloquium before Recitation runs out - is no longer broken off and pressed again every frame. When the healed
target was not the only one in need, the next GCD goes to the one still in need. DefenseTrace.log records each stop.

## Raise All counts the alliance's raisers

Under "Raise All." an alliance member's raiser who is alive now counts the way it does under "Raise All In Duty.": the
hard-cast modes that wait for other healers and the Phoenix Down wait while one of them lives. Strangers outside a
duty still never count.

## From upstream 7.5.7.4 to 7.5.7.6

- *Tanks: provoke and stance count your own party.* "Auto provoke when there is another tank in party", on by
  default, now does what it says: switched off, with a second tank in your party, an enemy on a non-tank is left to
  the other tank. Before, the count never found the other tank and provoked anyway. Taking the enemy from a co-tank in
  danger keeps its own setting. "Automatically activate tank stance" no longer turns your stance on while another
  living tank of your party holds one; in an alliance raid, a tank of another party no longer holds yours back.
- *"Stop casting if the target dies."*, off by default, works again: a cast on a dead enemy is broken off after
  "Random range of delay for RSR to stop attacking when the target is dead or immune to damage." (0.5-1 s by
  default).
- *"Auto turn on auto mode when alliance is in combat."*, off by default, works whatever the raise setting.
- *Abilities the game refuses* - a prerequisite missing, not learned - are no longer tried.
- *A heal that lands while its target is also hit* is recognized as landed; before, the target counted as healed
  until the prediction ran out.
- *Combat state* is updated before each frame's decision rather than after it.
- *"Remove extra lag-induced animation lock delay from instant casts"* and *"Remove extra framerate-induced cooldown
  delay"*, both off by default, now act on every action, your own presses included.
- *"Clicking actions random delay range."* is drawn once per press across the whole range; before, it stayed close
  to the lower end.
- *"Auto-use duty actions"* is new, on by default; switched off, duty actions are no longer used outside quest
  battles.

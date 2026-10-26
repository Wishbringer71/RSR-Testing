# Changes since 7.5.7.8+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## Phoenix Down waits for a cast it can finish, and comes before the HP potion

Since patch 7.3 a Phoenix Down can be thrown in combat in four-player content, but it is an 8-second cast during which
none of your actions goes out, mitigation included, and it shares its recast with every medicine. With "Use Phoenix Down
(Experimental)" on, it now waits while you move, while a tankbuster on you would land within its cast, and while the
heal forecast says you would fall before it ends - and goes once that has passed. While a raise with it is due and you
live through its cast, your HP potion waits, as a potion would keep the feather off for 75 seconds; a potion with a
tankbuster on its way to you still goes.

## Healers keep their single-target mitigation for the tankbuster that needs it

With "Spend only the mitigation a measured tankbuster needs" on, a healer's single-target mitigation - Aquaveil, Divine
Benison, Exaltation and the like - and a tank's help for another member no longer go onto a tank whose tankbuster has
been measured to take less than a large shield absorbs and to leave him at or above your HP threshold for single-target
healing GCDs. They stay ready for the next hit that does need them. A BossModReborn prediction beside a tankbuster no
longer sends that help onto the tank anyway, which also put it on a tank under Hallowed Ground or Superbolide. Reprisal
and the party mitigation some jobs spend in the single-target defence stay back the same way while such a tankbuster on
someone else is all that is coming and BossModReborn predicts none. The first time a tankbuster is met nothing changes:
it is measured with that hit.

## Summoner: the odd-minute Necrotizes wait for the two-minute window

The two Necrotizes from the odd-minute Energy Drain used to go out a few seconds before Solar Bahamut and Searing
Light, because Energy Drain comes back at the same moment as the burst summon. They now wait for the window when the
burst summon and Searing Light are back no later than Energy Drain, and Energy Drain follows them by a weave - as The
Balance's Summoner guide describes.

## White Mage and Astrologian: no barrier charge left at the cap

Divine Benison and Celestial Intersection used to wait at both charges while nobody was in danger, with their recharge
standing still. A charge at the cap now goes onto the member an enemy is attacking, where none of its barrier stands;
the second charge stays for the next danger. The Balance's White Mage guide recommends exactly this.

## Machinist: Flamethrower on a pack that stands still

RSR never used Flamethrower: its targeting took the action for a single-target one aimed at nobody. With "Use
Flamethrower on a pack that stands still" (on by default) it now goes in place of the filler when at least two enemies
stand in the cone before you (its AoE count, which the action settings now show), the pack has stopped moving, no enemy
casts an area attack, Hypercharge is not available, and no tool, Wildfire or Barrel Stabilizer comes ready during the
ten seconds - The Balance's rule. While it channels, RSR keeps its own actions and medicine back so as not to end it;
heals, defence, interrupts, cleanses, raises, provokes and what you press still go. Moving or turning ends it as before.

## Black Mage: Ley Lines by default

"Use Leylines in combat when standing still" is now on by default, so RSR places Ley Lines - before, it never did unless
you turned this on. It goes down once you stand still in combat with an enemy in range, keeps the second charge for you
unless "Use both stacks of Leylines automatically" is on, and waits while an enemy casts an area attack you may have to
leave the circle for. The Balance uses Ley Lines roughly on cooldown. If you place it yourself, turn the setting off.

## Scholar: Bio by the measured time to kill

The ballpark estimate behind "Enable ballpark DoT time-to-kill estimator" now holds Bio back only until a time to kill
is measured for the enemy. Before, it also held Bio on an enemy that had been measured to live long enough, whenever
its health was below what the party was assumed to deal in 12 seconds.

## Abilities when no GCD is coming

When RSR finds no weaponskill or spell to cast - while you move as a caster, say - your abilities no longer wait out
the last half second of the GCD timer. That half second only keeps an ability from delaying a GCD, and none is coming.

## Samurai and Dark Knight: AoE decisions count what the AoE hits

The Samurai's switch to the AoE Iaijutsu and Hagakure now counts the enemies within its AoE combo's radius (5 yalms
for Fuko) against that combo's AoE count, instead of those within melee range - so it agrees with the AoE combo it
already uses; Ogi Namikiri on the AoE path goes when it hits more than one enemy. The Dark Knight's Abyssal Drain
takes Carve and Spit's place where it out-damages it on the enemies it reaches, by their tooltips (from three), instead
of needing three enemies around you on top of its own AoE count.

## Tanks: pull mitigation counts what attacks you

"Use tank mitigation when this many enemies target you" now opens the tank's pull defence whenever that many enemies
within 3 yalms target you and you have attacked one of them. Before, more than 30 % of all enemies within 25 yalms also
had to stand at you, so unpulled packs nearby, casters at range or the other tank's adds kept it shut - holding three
enemies beside two unpulled packs of four, say. The big cooldowns still wait for your measured health to need them, and
now keep waiting when a trash mob casts a spell at you: only a known tankbuster on a tank - a marker, a cast from the
tankbuster list or one RSR has measured - lets them go at once.

## M9: a spread marker on someone else no longer counts as a hit on you

In M9 (Vamp Fatale, normal and Savage), the Aetherletting spread marker was also listed as a stack marker, so a spread
on another party member counted as a hit on you and spent your self-only protection - Radiant Aegis, Third Eye and the
like - on a hit that did not reach you. It now counts only when it sits on you; party mitigation reads it as before.

## Viper and Monk: "just used" no longer means "ready"

A check for whether an action was used in the last few seconds answered yes for an action that was simply ready. The
Viper's Uncoiled Fury rule for the window after Serpent's Ire therefore spent coils while Serpent's Ire lay ready and
unused - all fight long with burst off; now it waits for Serpent's Ire, and the overcap rule still spends coils at the
cap. The Monk's odd-minute Perfect Balance, which waits 20 seconds after the last one, was kept back while both
charges were full, and Phantom Rush counted a ready Riddle of Fire as used.

## Healers: ground areas where the party stands

Sacred Soil, Asylum, Earthly Star and Liturgy of the Bell went to a stored point, the arena centre, the boss or a point
by the tank, as long as anyone at all stood in the area. With "Place them where they cover the most party members"
(on by default) they go to the spot at a party member that covers more of them whenever there is one: when the party
stacks at the south edge, 20 yalms from the boss, for a marker before a raidwide, Sacred Soil lands on the stack instead of the empty
middle. With the party at the centre nothing changes.

## Line attacks count the enemies on the way to their target

Doom Spike, Draconian Fury and Coerthan Torment, Chain Saw, Apex Arrow and Blast Arrow, Shadowbringer and
Enlightenment hit everything in a line towards their target, but RSR counted the enemies along the way you happened
to face. A Dragoon looking away from the pack stayed on the single-target combo with four enemies in reach, or went
into the AoE combo towards a target with nobody behind it. They are now counted towards the target, and with several
targets in reach the one whose line hits the most is taken.

## White Mage: Thin Air no longer overrides "Hard-cast raise"

With Thin Air up and someone dead, the White Mage hard cast Raise - eight seconds without heals or damage - even with
"Hard-cast raise" set to "Do not hard cast Raise" or to a mode that waits for the other healers, and while moving it
chose no GCD at all, not even Dia or a Lily. Whether to hard cast is now decided only by that setting, as for every
other healer; Thin Air still makes the raise cost no MP.

## Summoner: Swiftcast on Resurrection follows "Raise with Swiftcast or Dualcast"

With "Raise with Swiftcast or Dualcast" off, a Summoner still swiftcast Resurrection - through the rule that puts
Swiftcast on any long cast, and through the Summoner's own option for Swiftcast on Resurrection, whose off switch
changed nothing. The setting, which is kept per job, now decides alone; the Summoner option is gone.

## Astrologian: Essential Dignity priority no longer stops single-target healing

With "Prioritize Essential Dignity over single target GCD heals when available" on "Any charges" or "When capped", the
Astrologian held every single-target GCD heal while a charge was left - also when that charge was the last one, which
waits for a target in danger. A member left at 30 % after a raidwide, with nothing hitting him, then got no heal from
the Astrologian at all. GCD heals now wait only while Essential Dignity can actually go out.

With "Prioritize Microcosmos over all other healing when available" on, Oracle and the potion before Divination were
held together with the heals while Macrocosmos ran, and the potion missed that burst. Only heals wait now.

## Sage: Druochole when Taurochole waits for danger

With "Taurochole as a heal only on a target in danger" on (the default), a low member with nothing hitting him got
neither Taurochole nor Druochole, because Druochole only came once Taurochole was on cooldown. Druochole now goes
instead, as the option describes.

## Dark Knight: "Keep at least 3000 MP" also when Darkside runs out

With Darkside about to run out, Edge of Shadow spent the last 3000 MP regardless of "Keep at least 3000 MP", and The
Blackest Night was missing for the next tankbuster. Edge now keeps what The Blackest Night costs, unless Dark Arts
makes it free; with little MP, Darkside may lapse until there is enough.

## "Reset to Default Value." restores the default

The right-click entry "Reset to Default Value." now restores the default value. For plain and per-job switches it used
to turn them off instead: "Heal ahead of incoming damage" or "Raise with Swiftcast or Dualcast", both on by default,
ended up off.

## Samurai: Feint stays for the hits the party takes

Below "Health threshold needed to use Tengentsu/ThirdEye outside of AOE mit scenarios." the Samurai also spent Feint,
with no hit announced, and it was then missing for the next raidwide. That threshold now uses Tengentsu and Third Eye
only, as its text says; Feint goes out for announced or measured hits, as for every other job.

## Reaper, Viper, Samurai: protection no longer waits for the job's own weaves

Reaper held Second Wind, Bloodbath, Feint, Arcane Crest, Arm's Length and Leg Sweep while Soul Reaver, Enshroud or
Executioner ran; Viper held the same while a follow-up ability was ready; Samurai held Feint, Tengentsu and Third Eye
while Zanshin was ready. A Reaper in Enshroud took an announced knockback without Arm's Length. These holds now give way
when the party, the player or a tank is in danger, and knockback protection and interrupts wait only for a weave that
does not clip.

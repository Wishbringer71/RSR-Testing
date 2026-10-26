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
enemies beside two unpulled packs of four, say. The big cooldowns still wait for your measured health to need them.

## M9: a spread marker on someone else no longer counts as a hit on you

In M9 (Vamp Fatale, normal and Savage), the Aetherletting spread marker was also listed as a stack marker, so a spread
on another party member counted as a hit on you and spent your self-only protection - Radiant Aegis, Third Eye and the
like - on a hit that did not reach you. It now counts only when it sits on you; party mitigation reads it as before.

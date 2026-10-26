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

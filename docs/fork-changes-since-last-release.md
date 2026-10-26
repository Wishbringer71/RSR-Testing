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
longer sends that help onto the tank anyway, which also put it on a tank under Hallowed Ground or Superbolide. The
first time a tankbuster is met nothing changes: it is measured with that hit.

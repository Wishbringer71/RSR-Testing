# Changes since 7.5.8.5+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## Occult Crescent

- **Phantom attacks wait for your burst on every job.** "Save Phantom Attacks for class specific damage bonus?" held
  them only for jobs whose rotation states a burst window of its own; for 13 of 21 jobs it never held. For those jobs
  the ready phantom abilities now wait for the burst RSR reads from where your rotation asks for the tincture, when none
  of them loses a use over the time your target has left. The trace writes "phantom attacks: held for the burst ...".

## Beastmaster

- **A finisher on a pack.** From level 50, on a pack that lives long enough for your TP to reach 250, the axes now wait
  and Brutal Rage or its kin hits every enemy around your target, when that is worth more than the axes and their
  combos. An axe hits one enemy with 4 potency per TP, a finisher every enemy within 6 yalms of the target with 1,200,
  and each combo is weighed with the damage your own combos dealt. Trick waits with the axes outside a chain, so it pairs
  with the finisher. In your runs no finisher came at all. Setting: "Hold the axes for a finisher on a pack while it is
  worth more", on by default. The trace writes "bst finisher: axes held ...".

## Crucible

- **Spoils after a fight.** A window "RSR - Crucible spoils" now shows which spoils to take and why. It says "Take all"
  when everything fits and nothing costs more than it brings. It names a piece to leave when your slots are full, when
  you already wear it, when it would harm you, or when gear would cost the bonus Mit leeren Händen while you carry the
  way without it. You take the pieces in the game's window. Setting: "Recommend what to take from a Crucible treasure
  or spoils".

- **Leveling takes the whole roster up to the board's cap.** Under the goal "Level", every familiar up to the cap of
  the board comes along, the lowest first; above the cap only while the team is endangered. Each familiar's experience
  depends on its rank against the cap and is divided by the team size, so a familiar above the cap takes a share from
  every one still below it. When no familiar on this board is below the cap, the team window names the next board that
  levels them and how many there are, and the trace writes why the goal is not played.
- **Items used where they count.** Before the next fight RSR uses Merchant's Eye (before the final fight only where the
  board awards Talerregen) and Thief's Eye (never before the final fight); both wait while an item bonus is at stake.
  Without an item bonus, the top-off at the end of the final fight starts one GCD per healing item ahead, plus one,
  instead of in the last GCD, which came too late in three of your runs; the largest heal that reaches your familiar
  waits for it while Unversehrte Bestien can still be earned. In the final fight, an enemy attacking you without an
  announced hit now counts as a physical auto-attack, so Tome of the Impervious goes instead of staying unused.
- **The board starts as it should.** RSR waits for the server's confirmation after pressing Start in the board
  choice instead of counting the closing window as a failed start.
- **Recommendation windows come back.** A recommendation window you close (team, shop, campsite, route, treasure,
  spoils, Masked Carnivale set) stays closed for that occasion only and opens again with the next one; before, it stayed
  closed until the plugin was reloaded.

## Rotations

- **Reaper waits for Radiant Finale again.** "Perfectio on raid buffs" no longer missed the bard's Radiant Finale on
  the party.
- **Beiruta healers heal a Dark Knight under Walking Dead.** Astrologian, Scholar and Sage of Beiruta no longer hold
  healing on a Dark Knight in Walking Dead, who dies unless healed to full before it ends.

## Settings

- **Item settings in one place.** The fork's potion and item settings stand with the game items under Item Usage.

- **Shorter explanations.** Every setting this fork adds now explains itself in at most three short lines: what RSR does
  in a fight, the one limit to know, and what Off or a value does. They were up to 2,500 characters long; the full
  reasoning stays in the documentation.

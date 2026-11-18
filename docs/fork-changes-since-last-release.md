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

## Settings

- **Shorter explanations.** Every setting this fork adds now explains itself in at most three short lines: what RSR does
  in a fight, the one limit to know, and what Off or a value does. They were up to 2,500 characters long; the full
  reasoning stays in the documentation.

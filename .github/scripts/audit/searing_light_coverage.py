#!/usr/bin/env python3
"""How much Searing Light uptime does a party of n Summoners actually get?

The concept in docs/rotation-flow/12-searing-light-stacking.md carried percentages that were
arithmetic done in the head, and one of them was wrong in a way the head does not catch: it said
coverage stays at 33% with the widened cast window, while the section right above it explained that
drift between rotations scatters the windows - which raises coverage. Both cannot be true. This
model settles it by running the rules instead of estimating their outcome.

What it models, all of it taken from the code or from the sources the concept cites:

- Searing Light: 20s effect, 120s recast, does not stack - a second cast replaces the first.
- Demi summons: 15s standing, one every 60s, in the order Solar, Bahamut, Solar, Phoenix. Solar
  therefore comes around every 120s.
- Cast window: today only while Solar stands (SMN_Reborn.cs:203); widened, any demi.
- The guard: a cast is refused while a Searing Light is running and more than `guard_lead` seconds
  remain on it - ActionBasicInfo.IsStatusProvided with StatusRefreshGcdCount = 2, about 5s at a
  2.5s GCD.
- Drift: each Summoner's cycle is offset by a fixed amount, spread over `drift` seconds. Drift 0 is
  a clean synchronised pull; drift 60 is a fight where deaths, movement and stuns have pulled the
  rotations fully apart.

What it does not model: that a buff landing outside the two-minute burst window is worth less than
one inside it. Coverage is not damage. The concept says so; this script only counts seconds.

Usage: python3 .github/scripts/audit/searing_light_coverage.py [--csv]
"""
import sys

BUFF = 20.0
RECAST = 120.0
DEMI_STAND = 15.0
DEMI_EVERY = 60.0
GUARD_LEAD = 5.0
FIGHT = 1200.0          # a normal raid fight; --long runs 40 minutes instead
STEP = 0.1
# Comparisons need a tolerance in the order of the time grid: one step is STEP/FIGHT of the result,
# and a boundary condition can land on either side of a step. Anything tighter tests the grid, not
# the rules - the first version used 1e-9 and failed on a 0.02 percentage point difference.
TOL = 0.002
# How long past a Summoner's earliest return the adaptive rule keeps counting on him before writing
# him off. One buff length: long enough to absorb a late cast, short enough to notice a dropout.
STALE_AFTER = 20.0
# Damage is not spread evenly over the two-minute cycle: raid buffs and cooldowns are bundled into
# the burst window, so a share of the fight's damage well above its share of the time falls there.
# The exact figure is a party-composition question and cannot be settled from this repository, so it
# is a parameter and the output shows several values. BURST_WINDOW is the 20s at the top of each
# cycle, the one every job aligns to.
BURST_WINDOW = 20.0
# The order the user's rule works through, by position in the four-demi cycle: Solar first (it sits
# on positions 0 and 2), then Bahamut, then Phoenix. A charge moves on only when somebody else has
# taken the phase it was aiming at.
PHASE_AIM = ((0, 2), (1,), (3,))
# Books are kept per phase *kind*, not per position in the cycle. Solar occupies positions 0 and 2,
# and Searing Light's 120s recast is exactly two demi windows - so a Summoner who owns Solar shows
# up alternately at 0 and at 2. Booking positions therefore never saw a repetition, and the rule
# could not tell a held phase from a contested one.
PHASE_KIND = {0: 'solar', 2: 'solar', 1: 'bahamut', 3: 'phoenix'}
# How long until a phase kind comes round again. Solar sits on two of the four positions, so it
# returns every 120s - exactly Searing Light's recast, which is why one Summoner can hold it every
# cycle. Bahamut and Phoenix return only every 240s, so a Summoner who takes one of them alternates
# between the two. Judging all three by the same window made a held Bahamut look contested.
KIND_PERIOD = {'solar': 2 * DEMI_EVERY, 'bahamut': 4 * DEMI_EVERY, 'phoenix': 4 * DEMI_EVERY}

# Potency per GCD of each phase, from smn_phase_potency.py (summon off the GCD, level 100, single
# target). Seconds with a buff are a surrogate: the buff raises damage by 5%, so what it is worth
# depends on how much damage the second carries. Solar is worth 2.46 times a primal second, which is
# why "cover as many seconds as possible" and "put the buff where it pays" are different questions -
# and the user's rule answers the second one.
PHASE_POTENCY = {
    0: 1217.0,   # Solar Bahamut
    1: 950.0,    # Bahamut
    2: 1217.0,   # Solar Bahamut again
    3: 947.0,    # Phoenix
}
# Between the demis the primal block runs. The rotation's order is Ifrit, Titan, Garuda, and their
# per-GCD potencies differ enough to matter for where a charge lands: this is the "intermediate
# phase with the most damage" the user's rule aims at once the burst phases are taken.
PRIMAL_POTENCY = (632.0, 464.0, 407.0)


def own_potency(t, offset):
    """Potency per GCD this Summoner is producing at time t.

    A demi contributes its own figure; between demis the primal block runs, and which of the three
    it is follows the rotation's order. This is what makes a buffed second worth what it is worth,
    and it is deliberately the *own* rotation: Searing Light raises the whole party's damage, but
    the decision the rule makes is where to spend a charge of one's own.
    """
    local = t - offset
    if local < 0:
        return PRIMAL_POTENCY[0]
    cycle = int(local // DEMI_EVERY)
    if local % DEMI_EVERY < DEMI_STAND:
        return PHASE_POTENCY[cycle % 4]
    return PRIMAL_POTENCY[cycle % len(PRIMAL_POTENCY)]


def demi_index(t, offset):
    """Which demi is standing for this Summoner right now, or None between them.

    0 Solar, 1 Bahamut, 2 Solar, 3 Phoenix, then repeating - the order the concept records. The
    user's rule aims at these by position, so it needs the index and not merely "a demi stands".
    """
    local = t - offset
    if local < 0:
        return None
    if local % DEMI_EVERY >= DEMI_STAND:
        return None
    return int(local // DEMI_EVERY) % 4


def in_window(t, offset, mode):
    """Is this Summoner allowed to cast at time t?

    mode 'solar'  - only while Solar Bahamut stands, which is what SMN_Reborn does today
    mode 'demi'   - while any demi stands (V2)
    mode 'anytime' - whenever a target is up, i.e. the demi tie is dropped entirely (V4)
    mode 'informed' - any demi, plus outside one when no other known Summoner can cover the gap (V5)
    mode 'simple'  - any demi, plus outside one whenever the buff has fully expired (V7). No books
                     about anyone else at all - the user's objection: the others cannot cast before
                     their own recast is up anyway, so what is there to defer to?
    """
    if mode == 'anytime':
        return t >= offset
    if mode in ('informed', 'adaptive', 'simple'):
        return t >= offset  # the caller decides; window handling lives in simulate()
    local = t - offset
    if local < 0:
        return False
    # Which demi are we in, if any?
    since = local % DEMI_EVERY
    if since >= DEMI_STAND:
        return False
    index = int(local // DEMI_EVERY)  # 0 Solar, 1 Bahamut, 2 Solar, 3 Phoenix, ...
    if mode == 'demi':
        return True
    return index % 2 == 0  # Solar sits on every second demi



def best_possible(n, burst_share):
    """The damage-weighted ceiling: n charges of 20s per 120s cycle, placed as well as possible.

    Placement is not free - a charge covers 20 contiguous seconds - but the optimum is easy to state
    because the burst window is exactly one buff long: put the first charge on the burst, where the
    damage density is highest, and spread the rest over the remainder, where density is uniform. Any
    other placement moves buff time from a denser second to a thinner one.

    This is what the rules are measured against. "Better than the alternatives" is not the same as
    "as good as it gets", and without this line the difference cannot be seen.
    """
    if n <= 0:
        return 0.0
    covered_burst = min(BURST_WINDOW, n * BUFF)
    rest_charges = max(0.0, n * BUFF - BURST_WINDOW)
    rest_span = RECAST - BURST_WINDOW
    covered_rest = min(rest_span, rest_charges)
    return (burst_share * covered_burst / BURST_WINDOW
            + (1.0 - burst_share) * covered_rest / rest_span)


def simulate(n, mode, drift, fight=None, window=None, dropout=None, burst_share=None,
             burst_only=False, modes=None, mine=None, by_potency=False, first=0):
    """Return the fraction of the fight covered by a Searing Light.

    `window` restricts the measurement to (start, end) seconds without changing the run, which is
    how the settling behaviour is read: compare the first two minutes against the last two.

    For 'informed' (V5) each Summoner also keeps what every client can actually observe: the status
    carries its source, so once another Summoner has cast, his earliest possible return is known -
    that cast plus the recast. The rule is to hold to the demi windows as in V2, and to cast outside
    one only when nobody else can possibly cover the coming gap.

    `modes` gives each Summoner his own rule, which is what a real party looks like: the other
    Summoners are other players, running their own rotation or none. Passing one mode for everybody
    answers a question nobody is in - it was the model's unstated assumption and the reason its
    recommendation rested on the rarest case. `mine` names the index whose own burst is measured
    when burst_only is asked for.
    """
    fight = FIGHT if fight is None else fight
    weighting = burst_share is not None or by_potency
    burst_share = 0.0 if burst_share is None else burst_share
    lo, hi = window if window else (0.0, fight)
    per = list(modes) if modes else [mode] * n
    offsets = [0.0] * n if n == 1 or drift == 0 else [drift * i / (n - 1) for i in range(n)]
    ready = [0.0] * n          # earliest time each Summoner may cast again
    # The user's rule, per Summoner: which burst phase he is currently aiming at. 0 Solar, 1 the
    # next demi, 2 the one after, 3 means all three were taken and the charge goes into a primal
    # phase instead. It only ever moves forward when somebody else got there first.
    phase_target = [0] * n
    buff_until = -1.0          # when the running buff expires
    buff_owner = None          # who cast the running buff - the user's rule needs "somebody else"
    covered = 0.0
    # Which instance of a demi phase a Summoner has already conceded, so the aim advances once per
    # phase and not once per time step.
    conceded = [None] * n
    # The books, kept per Summoner: when somebody else last cast while this Summoner stood in each
    # of his own four phase positions. The status carries its source, so this is what a client can
    # actually see. A holder returns after his own recast - 120s, which is two demi windows, so he
    # comes back to the same position in the cycle. That is what makes the position, and not the
    # single occasion, the thing worth recording.
    taken = [dict() for _ in range(n)]
    # The run-up needs no counting of its own: whoever casts first takes the strongest phase, and
    # everybody else finds it held in the books and moves to the next. The direction is set by the
    # first cast, which is what every client sees anyway.

    last_seen = [None] * n     # when each Summoner was last observed casting
    # What the party has observed: earliest possible return per Summoner, or None if never seen.
    # Before a Summoner's first cast nobody knows he exists as a caster - that is the honest state.
    seen_ready = [None] * n

    t = 0.0
    weighted = 0.0            # damage-weighted coverage
    weight_total = 0.0
    while t < fight:
        if lo <= t < hi:
            # Weight of this instant: burst share concentrated in the first BURST_WINDOW of each
            # 120s cycle, the rest spread evenly over the remainder.
            in_burst = (t % RECAST) < BURST_WINDOW
            # burst_only measures the burst window alone: every instant inside it counts, everything
            # else counts zero. A rule that raises total coverage while lowering this one has moved
            # buff time out of the party's burst - a regression the combined figure would hide.
            if by_potency:
                # Weight every instant by the damage actually being produced in it, so the figure
                # answers "how much of my damage was buffed" instead of "how many seconds carried a
                # buff". The two differ by a factor of 2.46 between a Solar second and a primal one.
                w = own_potency(t, offsets[mine or 0])
            elif burst_only:
                w = 1.0 if in_burst else 0.0
            else:
                w = (burst_share / BURST_WINDOW) if in_burst \
                    else ((1.0 - burst_share) / (RECAST - BURST_WINDOW))
            weight_total += w
            if buff_until > t:
                weighted += w
                covered += STEP
        # Nothing is counted outside the window. `covered` is divided by the window length, so
        # counting the rest of the fight into it made the result the whole fight's coverage
        # stretched by fight/window - the settling table printed 332%, the dropout table up to
        # 542%. Both are meant to answer "how well is this stretch covered", and that question
        # only takes the instants inside it.

        remaining = buff_until - t
        blocked = remaining > GUARD_LEAD

        # Conceding a phase is an observation, not a cast attempt, so it has to happen even while
        # the guard is shut - and the guard is shut precisely when somebody else has just cast,
        # which is the case the rule is about. Running this inside the casting loop below left the
        # aim stuck on Solar forever: the charge was blocked, the aim never moved, and the rule
        # measured exactly like the unchanged code.
        for i in range(n):
            if per[i] not in ('phased', 'booked', 'hybrid'):
                continue
            if buff_until <= t or buff_owner is None or buff_owner == i:
                continue
            # Book the position first: this is the observation both variants make, and the booked
            # variant needs it even once its aim has settled.
            local_b = t - offsets[i]
            if local_b >= 0 and local_b % DEMI_EVERY < DEMI_STAND:
                pos = PHASE_KIND[int(local_b // DEMI_EVERY) % 4]
                prev = taken[i].get(pos)
                # A position counts as held only when the *same* Summoner takes it again after his
                # own recast - the user's wording: assume you are first, and only once he really
                # comes back to it conclude that he owns it. Booking on a single occasion is what
                # made the first version give up positions that were never contested, which cost
                # more than it saved wherever rotations had drifted apart.
                if prev and prev[0] == buff_owner \
                        and t - prev[1] <= KIND_PERIOD[pos] + STALE_AFTER:
                    taken[i][pos] = (buff_owner, t, prev[2] + 1)
                else:
                    taken[i][pos] = (buff_owner, t, 1)
            if per[i] != 'phased' or phase_target[i] >= 3:
                continue
            local = t - offsets[i]
            if local < 0 or local % DEMI_EVERY >= DEMI_STAND:
                continue
            instance = int(local // DEMI_EVERY)
            if conceded[i] == instance:
                continue
            if (instance % 4) in PHASE_AIM[phase_target[i]]:
                phase_target[i] += 1
                conceded[i] = instance

        if not blocked:
            # Who gets the tie is decided by a fraction of a second in the real fight, not by the
            # order of a loop. Iterating from index 0 every time handed Summoner 0 every contested
            # window, which measured the loop and not the rule - the caller varies `first` and
            # averages over the starts.
            for step in range(n):
                i = (step + first) % n
                if ready[i] > t:
                    continue

                if dropout and i == dropout[0] and dropout[1] <= t < dropout[2]:
                    continue   # this Summoner is out and casts nothing

                allowed = False
                mine_mode = per[i]
                if mine_mode == 'hybrid':
                    # The user's hybrid: books decide which phase is worth aiming at, and when no
                    # phase of one's own is coming up in time, the charge fills the gap instead of
                    # waiting. Neither half wins everywhere - the books win a synchronised pull
                    # among strangers, gap-filling wins when everyone runs the same rule, and the
                    # unchanged narrow rule already wins once rotations have drifted apart. So the
                    # rule switches rather than choosing once.
                    idx = demi_index(t, offsets[i])
                    # Free phase *kinds*, strongest first. Kinds, not positions: Solar occupies two
                    # positions in the cycle and a Summoner who owns it takes both, so picking a
                    # single position would make him skip every second Solar - which is what the
                    # first version of this did, and it halved a lone Summoner's uptime.
                    kinds_free = [k for k in ('solar', 'bahamut', 'phoenix')
                                  if not (taken[i].get(k) is not None
                                          and taken[i][k][2] >= 2
                                          and t - taken[i][k][1] <= KIND_PERIOD[k] + STALE_AFTER)]
                    # Always aim at the strongest phase still free - the assumption is "I cast
                    # first", and it is given up only when somebody actually gets there first. An
                    # earlier version ranked by how many others had been seen casting and stepped
                    # aside before even trying; that hands the phase away on a guess, and a
                    # Summoner who would have won it ends up in a weaker one for the whole fight.
                    # The staggering comes out of the books instead: whoever holds a phase keeps
                    # showing up in it, so the others find it taken and move on by themselves.
                    mine_kind = kinds_free[0] if kinds_free else None
                    if buff_until <= t:
                        if mine_kind is not None:
                            # A phase of one's own is still to be had. Wait for it rather than
                            # filling a gap: a Solar second is worth 2.46 primal seconds, and the
                            # recast is two windows long, so a charge spent outside does not come
                            # back in time for the position it was meant for.
                            allowed = idx is not None and PHASE_KIND[idx] == mine_kind
                        else:
                            # Every position is held by somebody who keeps coming back. Waiting
                            # buys nothing now, so the rule switches to filling whatever gap is
                            # open - which is what wins when several Summoners share this rule and
                            # crowd each other out of the same phases.
                            allowed = True
                elif mine_mode == 'booked':
                    # The user's rule with books. Rather than only stepping forward, pick the
                    # strongest phase position nobody else is holding - and a holder is somebody
                    # seen casting there within his own recast plus a grace. Two Solar positions,
                    # then Bahamut, then Phoenix, by what a second of each is worth. This is what
                    # the step-forward variant cannot do: it gives up a position for good, so with
                    # several Summoners on the same rule they all walk away from the same phases
                    # together.
                    idx = demi_index(t, offsets[i])
                    free = []
                    for pos in (0, 2, 1, 3):
                        book = taken[i].get(PHASE_KIND[pos])
                        held = (book is not None and book[2] >= 2
                                and t - book[1] <= KIND_PERIOD[PHASE_KIND[pos]] + STALE_AFTER)
                        if not held:
                            free.append(pos)
                    if idx is not None and buff_until <= t:
                        allowed = idx in free
                    elif idx is None and not free and buff_until <= t:
                        local = t - offsets[i]
                        block = int(local // DEMI_EVERY) % len(PRIMAL_POTENCY) if local >= 0 else 0
                        best = max(range(len(PRIMAL_POTENCY)), key=lambda k: PRIMAL_POTENCY[k])
                        allowed = block == best
                elif mine_mode == 'phased':
                    # The user's rule. Aim at one burst phase at a time, and only move on when
                    # somebody else got there first - a second cast onto a running buff is the one
                    # thing that is certainly wasted.
                    idx = demi_index(t, offsets[i])
                    if idx is not None:
                        # The aim moves Solar -> Bahamut -> Phoenix. Solar occupies two of the four
                        # positions in the cycle (0 and 2), which is why this is a lookup and not a
                        # comparison against the index - the first version compared them directly
                        # and the rule then skipped every second Solar.
                        wanted = PHASE_AIM[phase_target[i]] if phase_target[i] < 3 else ()
                        if phase_target[i] >= 3:
                            allowed = False          # the burst phases are spoken for
                        elif idx in wanted and buff_until <= t:
                            allowed = True
                    elif phase_target[i] >= 3 and buff_until <= t:
                        # All three burst phases taken every cycle, so the charge goes where this
                        # Summoner's own damage is highest instead - and that is a specific primal
                        # block, not merely "outside a demi". Ifrit carries 632 potency per GCD
                        # against Titan's 464 and Garuda's 407, so aiming at the strongest one is
                        # the difference between spending the charge well and spending it anywhere.
                        # It waits at most two primal blocks, and the recast is longer than that.
                        local = t - offsets[i]
                        block = int(local // DEMI_EVERY) % len(PRIMAL_POTENCY) if local >= 0 else 0
                        best = max(range(len(PRIMAL_POTENCY)), key=lambda k: PRIMAL_POTENCY[k])
                        allowed = block == best
                elif mine_mode == 'simple':
                    allowed = in_window(t, offsets[i], 'demi') or buff_until <= t
                elif mine_mode in ('informed', 'adaptive'):
                    if in_window(t, offsets[i], 'demi'):
                        allowed = True
                    else:
                        # Outside a demi, two conditions, and the second was learned from the model
                        # rather than assumed. First: no other observed Summoner can possibly take
                        # this gap. Second: the buff must be fully expired, not merely inside the
                        # guard's five-second grace. Casting into those last seconds is what the
                        # guard permits for a refresh, but out here it burns a whole charge for a
                        # few seconds of gain - the first version of this rule did exactly that and
                        # came out *below* V2 at two Summoners.
                        others = []
                        for j in range(n):
                            if j == i or seen_ready[j] is None:
                                continue
                            if mine_mode == 'adaptive' and last_seen[j] is not None \
                                    and t - last_seen[j] > RECAST + STALE_AFTER:
                                # Adaptive: he was due back and did not come. Stop counting on him.
                                continue
                            others.append(seen_ready[j])
                        nobody_else = all(r > t + GUARD_LEAD for r in others) if others else False
                        allowed = nobody_else and buff_until <= t
                elif in_window(t, offsets[i], mine_mode):
                    allowed = True

                if allowed:
                    buff_until = t + BUFF   # overwrite, never stack
                    buff_owner = i
                    ready[i] = t + RECAST
                    seen_ready[i] = t + RECAST   # every client sees the source of the status
                    last_seen[i] = t
                    break

        t += STEP

    if weighting or burst_only:
        return weighted / weight_total if weight_total else 0.0
    return covered / (hi - lo)


def self_test():
    # One Summoner, no drift: one cast per recast, 20s of buff each - 20/120.
    lone = simulate(1, 'solar', drift=0)
    if not 0.15 < lone < 0.19:
        raise AssertionError('a single Summoner should land near 20/120, got %.3f' % lone)

    # Each widening step must never lower coverage, at any party size or drift.
    for n in range(1, 9):
        for drift in (0, 30, 60):
            narrow = simulate(n, 'solar', drift=drift)
            wide = simulate(n, 'demi', drift=drift)
            if wide < narrow - TOL:
                raise AssertionError('widening lowered coverage at n=%d drift=%d' % (n, drift))

    # V5 must never do worse than V2: it is V2 plus an extra permission.
    for n in range(1, 9):
        for drift in (0, 30, 60):
            if simulate(n, 'informed', drift) < simulate(n, 'demi', drift) - TOL:
                raise AssertionError('informed fell below demi at n=%d drift=%d' % (n, drift))

    # The finding that made two whole variants unnecessary, kept as a test so it cannot quietly stop
    # being true: keeping books on the other Summoners buys nothing. V7 asks only whether the buff
    # has expired and whether its own recast is up; V5 additionally tracks when every other observed
    # Summoner could return. They come out equal everywhere, and the reason is the user's: nobody can
    # cast before their own recast is up, so there is nothing to defer to - and the existing guard
    # already prevents the waste that deferring was meant to avoid.
    for n in range(1, 9):
        for drift in (0, 30, 60):
            simple = simulate(n, 'simple', drift)
            informed = simulate(n, 'informed', drift)
            if simple < informed - TOL:
                raise AssertionError('the simple rule fell below the informed one at n=%d drift=%d'
                                     % (n, drift))
            # Inside the range that decides anything they are not merely comparable, they are equal.
            if n <= 5 and abs(simple - informed) > TOL:
                raise AssertionError('simple and informed diverged at n=%d drift=%d' % (n, drift))

    # No rule may beat the optimal placement - that would mean the model is cheating somewhere.
    for n in range(1, 6):
        for bs in (0.17, 0.30, 0.45):
            best = best_possible(n, bs)
            for mode in ('solar', 'demi', 'simple'):
                got = simulate(n, mode, 0, burst_share=bs)
                if got > best + TOL:
                    raise AssertionError('%s beat the optimum at n=%d burst=%.2f: %.3f > %.3f'
                                         % (mode, n, bs, got, best))

    # Before comparing burst coverage, check the measurement returns anything at all. The first
    # version of it put a `continue` before the casting logic, so nobody ever cast and every figure
    # came out 0% - and the comparison below passed, because 0 is not less than 0. A test that only
    # relates two numbers cannot notice that both are broken.
    lone_burst = simulate(1, 'solar', 0, burst_only=True)
    if lone_burst < 0.9:
        raise AssertionError('a single Summoner casting in Solar must cover his own burst window, '
                             'got %.3f' % lone_burst)

    # The regression the user warned about: no widening may lower burst coverage. Total coverage
    # going up while the burst goes down would mean buff time was moved out of the party's burst,
    # and the combined figure would hide it.
    #
    # The tolerance here is larger than TOL and that is not a loosening to make the test pass. The
    # burst window is 20s of each 120s cycle, so a cast landing one grid step later costs STEP of
    # those 20 seconds - a relative error of STEP/BURST_WINDOW, five times coarser than for a
    # full-cycle figure. Checked rather than assumed: at STEP 0.1 the largest gap between the rules
    # is 0.8 percentage points, at STEP 0.01 it is 0.14 - it scales with the grid, so it is
    # discretisation and not a real loss.
    burst_tol = STEP / BURST_WINDOW * 2
    for n in range(1, 6):
        for drift in (0, 30, 60):
            base = simulate(n, 'solar', drift, burst_only=True)
            for mode in ('demi', 'simple'):
                got = simulate(n, mode, drift, burst_only=True)
                if got < base - burst_tol:
                    raise AssertionError('%s lowered burst coverage at n=%d drift=%d: %.3f < %.3f'
                                         % (mode, n, drift, got, base))

    # A windowed measurement is a share of that window and cannot exceed it. This is the defect the
    # review found: coverage was accumulated over the whole fight and divided by the window length,
    # so the settling and dropout tables printed 332% and up to 542%. Both tables read as plain
    # percentages, so nothing failed and nobody could tell the figure was not the one asked for.
    for n in (1, 3, 5):
        for w in ((0.0, 120.0), (300.0, 480.0), (FIGHT - 120.0, FIGHT)):
            got = simulate(n, 'simple', 0, FIGHT, window=w)
            if not 0.0 <= got <= 1.0 + TOL:
                raise AssertionError('windowed coverage outside 0..1 at n=%d window=%s: %.3f'
                                     % (n, w, got))
        whole = simulate(n, 'simple', 0, FIGHT)
        full_window = simulate(n, 'simple', 0, FIGHT, window=(0.0, FIGHT))
        if abs(whole - full_window) > TOL:
            raise AssertionError('a window spanning the fight must equal the unwindowed run at n=%d'
                                 % n)

    # Writing off a Summoner who stopped casting can never do worse than keeping him in the books.
    for n in range(2, 6):
        out = (0, 300.0, 480.0)
        v5 = simulate(n, 'informed', 0, window=(300.0, 480.0), dropout=out)
        v6 = simulate(n, 'adaptive', 0, window=(300.0, 480.0), dropout=out)
        if v6 < v5 - TOL:
            raise AssertionError('adaptive fell below informed under dropout at n=%d' % n)

    # The user's rule has to reach a phase it aims at. Beaten to Solar, it moves to Bahamut; beaten
    # there, to Phoenix; beaten there too, to the strongest primal block. If the concession step
    # ever stops working the rule collapses into today's behaviour without anything failing - the
    # first version did exactly that, because conceding was attempted only while the guard was open
    # and the guard is shut precisely when somebody else has just cast.
    beaten = simulate(3, None, 0, modes=['phased', 'solar', 'solar'])
    today = simulate(3, 'solar', 0)
    if beaten <= today + TOL:
        raise AssertionError('the phased rule no longer beats today in a mixed party: %.3f vs %.3f'
                             % (beaten, today))

    # The hybrid must be at least as good as today's rule everywhere in the party sizes that decide
    # anything, in the party that actually occurs: one Summoner on this rule, the rest on their own.
    # A rule that wins one situation by losing another is not an adaptation.
    for drift in (0, 30, 60):
        for n in range(2, 6):
            mixed = ['hybrid'] + ['solar'] * (n - 1)
            got = simulate(n, None, drift, modes=mixed, mine=0, by_potency=True)
            base = simulate(n, None, drift, modes=['solar'] * n, mine=0, by_potency=True)
            if got < base - TOL:
                raise AssertionError('hybrid fell below today at n=%d drift=%d: %.3f < %.3f'
                                     % (n, drift, got, base))

    # Books are kept per phase kind. Booking by position could never see a repetition, because the
    # 120s recast moves a Solar holder between the two Solar positions - the defect that made the
    # rule unable to tell a held phase from a contested one.
    if PHASE_KIND[0] != PHASE_KIND[2]:
        raise AssertionError('the two Solar positions must book as one phase')
    if KIND_PERIOD['solar'] >= KIND_PERIOD['bahamut']:
        raise AssertionError('Solar returns twice as often as Bahamut; the periods say otherwise')

    # Potency weighting must separate a Solar second from a primal one, or it is measuring seconds
    # again under another name.
    if own_potency(1.0, 0.0) <= own_potency(20.0, 0.0):
        raise AssertionError('a Solar second is not weighted above a primal one')

    # What the repaired measurement shows, kept so it cannot quietly stop being true: while one
    # Summoner is out, keeping books on him costs half the coverage. V5 holds its charge waiting
    # for a caster who never returns; V7 asks only whether the buff is running and fires. At three
    # Summoners and up that is a factor of two - which is the cost of bookkeeping in the one
    # situation bookkeeping was supposed to be good at, and the reason the simple rule is not
    # merely equal to the informed one but safer.
    for n in range(3, 6):
        out = (0, 300.0, 480.0)
        v5 = simulate(n, 'informed', 0, window=(300.0, 480.0), dropout=out)
        v7 = simulate(n, 'simple', 0, window=(300.0, 480.0), dropout=out)
        if v7 < v5 + TOL:
            raise AssertionError('the simple rule no longer beats the informed one under dropout '
                                 'at n=%d: %.3f vs %.3f' % (n, v7, v5))

    # Dropping the demi tie entirely can never do worse than only casting in Solar.
    for n in range(1, 9):
        for drift in (0, 30, 60):
            if simulate(n, 'anytime', drift) < simulate(n, 'solar', drift) - TOL:
                raise AssertionError('anytime fell below solar at n=%d drift=%d' % (n, drift))

    # Deliberately NOT asserted: that more Summoners mean more coverage. They do not, and an earlier
    # version of this test claimed they did. At half drift the widened window gives 84% with seven
    # and 83% with eight, and a weaker "large parties never fall below small ones" fails as well.
    # The cause is greedy allocation and it is real, not a modelling artefact: whoever sits in a
    # window first casts, and no client can see another's cooldowns in order to defer to them. So an
    # extra caster can take a window moments before someone whose own cooldown would have covered a
    # later gap. Any monotonicity test here would have to be loosened until it tested nothing.

    # The ceiling: coverage can never exceed what the charges allow, n * 20s per 120s.
    for n in range(1, 7):
        ceiling = min(1.0, n * BUFF / RECAST)
        for drift in (0, 30, 60):
            for mode in ('solar', 'demi', 'anytime', 'informed'):
                got = simulate(n, mode, drift)
                if got > ceiling + 0.02:
                    raise AssertionError('n=%d exceeded its charge ceiling: %.3f > %.3f'
                                         % (n, got, ceiling))

    print('self-test ok: single Summoner near 20/120, each widening step never lowers coverage, '
          'charge ceiling respected, bookkeeping buys nothing,\n'
          '  a windowed measurement stays a share of its window, and under dropout the simple '
          'rule beats the informed one\n')


def main():
    self_test()

    long_fight = '--long' in sys.argv
    fight = 2400.0 if long_fight else FIGHT
    as_csv = '--csv' in sys.argv
    upto = 9 if '--all' in sys.argv else 6   # regular parties hold at most five Summoners

    if as_csv:
        print('summoners,drift,solar,demi,anytime,informed,simple')

    print('Fight length: %.0f minutes%s\n'
          % (fight / 60, '' if long_fight else '  (--long for 40 minutes)'))

    for drift, label in ((0, 'synchronised (clean pull)'),
                         (30, 'half drifted'),
                         (60, 'fully drifted (deaths, movement, stuns)')):
        if not as_csv:
            print('%s' % label)
            print('  %-11s %-13s %-15s %-15s %-15s %s'
                  % ('Summoners', 'Solar only', 'any demi (V2)', 'informed (V5)', 'simple (V7)',
                     'ceiling'))
        for n in range(1, upto):
            narrow = simulate(n, 'solar', drift, fight)
            wide = simulate(n, 'demi', drift, fight)
            free = simulate(n, 'anytime', drift, fight)
            informed = simulate(n, 'informed', drift, fight)
            simple = simulate(n, 'simple', drift, fight)
            ceiling = min(1.0, n * BUFF / RECAST)
            if as_csv:
                print('%d,%d,%.3f,%.3f,%.3f,%.3f,%.3f'
                      % (n, drift, narrow, wide, free, informed, simple))
            else:
                print('  %-11d %-13s %-15s %-15s %-15s %s'
                      % (n, '%.0f%%' % (narrow * 100), '%.0f%%' % (wide * 100),
                         '%.0f%%' % (informed * 100), '%.0f%%' % (simple * 100),
                         '%.0f%%' % (ceiling * 100)))
        if not as_csv:
            print()

    if as_csv:
        return 0

    # Settling: a long fight can afford an untidy start if the pattern improves.
    print('Settling - first two minutes against the last two, synchronised pull')
    print('  %-11s %-22s %s' % ('Summoners', 'any demi (V2)', 'informed (V5)'))
    for n in range(2, upto):
        early_v2 = simulate(n, 'demi', 0, fight, window=(0.0, 120.0))
        late_v2 = simulate(n, 'demi', 0, fight, window=(fight - 120.0, fight))
        early_v5 = simulate(n, 'informed', 0, fight, window=(0.0, 120.0))
        late_v5 = simulate(n, 'informed', 0, fight, window=(fight - 120.0, fight))
        print('  %-11d %-22s %s'
              % (n, '%.0f%% -> %.0f%%' % (early_v2 * 100, late_v2 * 100),
                 '%.0f%% -> %.0f%%' % (early_v5 * 100, late_v5 * 100)))

    print()
    print('Damage-weighted, synchronised pull: share of the fight\'s DAMAGE that falls under a buff')
    print('(burst share = how much of the damage lands in the 20s burst window of each cycle)')
    for bs in (0.17, 0.30, 0.45):
        print('  burst share %.0f%%' % (bs * 100))
        print('    %-11s %-13s %-15s %-11s %s'
              % ('Summoners', 'Solar only', 'any demi (V2)', 'V7', 'best possible'))
        for n in range(1, upto):
            a = simulate(n, 'solar', 0, fight, burst_share=bs)
            b = simulate(n, 'demi', 0, fight, burst_share=bs)
            c = simulate(n, 'simple', 0, fight, burst_share=bs)
            best = best_possible(n, bs)
            print('    %-11d %-13s %-15s %-11s %s'
                  % (n, '%.0f%%' % (a * 100), '%.0f%%' % (b * 100), '%.0f%%' % (c * 100),
                     '%.0f%%' % (best * 100)))
    print()
    print('The burst window alone - is it still covered? A rule that raises the total while')
    print('lowering this has moved buff time out of the burst, which would be a regression.')
    for drift, label in ((0, 'synchronised'), (60, 'fully drifted')):
        print('  %s' % label)
        print('    %-11s %-13s %-15s %s' % ('Summoners', 'Solar only', 'any demi (V2)', 'V7'))
        for n in range(1, upto):
            a = simulate(n, 'solar', drift, fight, burst_only=True)
            b = simulate(n, 'demi', drift, fight, burst_only=True)
            c = simulate(n, 'simple', drift, fight, burst_only=True)
            print('    %-11d %-13s %-15s %s'
                  % (n, '%.0f%%' % (a * 100), '%.0f%%' % (b * 100), '%.0f%%' % (c * 100)))

    print()
    print('One Summoner drops out for three minutes, synchronised pull, measured over that window')
    print('  %-11s %-16s %-16s %s'
          % ('Summoners', 'informed (V5)', 'adaptive (V6)', 'simple (V7)'))
    for n in range(2, upto):
        out = (0, 300.0, 480.0)
        v5 = simulate(n, 'informed', 0, fight, window=(300.0, 480.0), dropout=out)
        v6 = simulate(n, 'adaptive', 0, fight, window=(300.0, 480.0), dropout=out)
        v7 = simulate(n, 'simple', 0, fight, window=(300.0, 480.0), dropout=out)
        print('  %-11d %-16s %-16s %s'
              % (n, '%.0f%%' % (v5 * 100), '%.0f%%' % (v6 * 100), '%.0f%%' % (v7 * 100)))

    print()
    print('Ceiling: n x 20s per 120s. A regular eight-man party holds four to five damage jobs, so')
    print('five Summoners is the practical maximum; six and above need --all.')
    print('Coverage is not damage - a buff outside the two-minute window buffs less of it.')
    return 0


if __name__ == '__main__':
    sys.exit(main())

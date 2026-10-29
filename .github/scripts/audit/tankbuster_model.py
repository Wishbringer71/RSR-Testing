"""Executable model of the tankbuster plan (concept 09, "Die Zustände je Wirken").

It implements the concept's rules - not the C# code - and runs them against a simulated world
with the timing the game has: frames of varying length with hitches, a forecast that reads once
per frame after the rotation, a server round trip between press, confirmation and status, a
client that may show the status before or after the confirmation, refusals, and the free paths
that can take a planned member away (Shake It Off, Nascent Flash on a party member, an early
press from another path).

Checked per run, against the true world state:
  O1  a plan that said "survivable" is: at impact the hit under what truly stands is below HP
      plus barriers (end to end; catches every way a planned member is lost);
  O2  nothing rated goes out on the player after an invulnerability was pressed for the cast;
  O3  the invulnerability is never pressed while a bundle of members that can still be pressed
      before the last GCD survives;
  O4  every member the plan pressed still stands at impact (pressed inside its window);
  O5  from the press until the hit, the plan never loses track of a pressed member;
  O8  "everything" (Z4) is planned only while the invulnerability is not available (option, cooldown),
      never because it was refused: a stun or amnesia blocks every other ability alike (his note);
  O9  a planned bundle leaves the tank above the invulnerability line at impact - at or below it the tank's
      own emergency rule draws the invulnerability right after the hit, and both are spent (A304);
  O10 no mitigation the plan presses for the cast reaches the server after the snapshot (the end of the cast
      there) - "alles nach schadenseingang bringt auch nix" (his note, 12.10.2026);
  O11 a stun that ends with a press left before the end of the cast and the invulnerability available leaves
      the tank alive: no bundle that cannot be pressed in time is planned instead;
  O12 a tankbuster of several hits (A454): a plan that said "survivable" (a bundle), took the cast as covered by an
      invulnerability (Z1) or drew one survives every hit of the series under what truly stands at it - the bundle
      sized for the sum of the series and standing until H_S = H + its span, the invulnerability pressed late enough
      to stand until H_S where it can and counted as covering only until H_I (concept 09, "H_S").

Impact follows the snapshot: a mitigation counts when the server has applied it by the end of the cast there,
which the client sees half a round trip later; Holmgang's floor counts when the damage is dealt, after that. A later
hit of a series takes its own snapshot, the same time after the end of the cast as the hit.

The self-test switches on, one at a time, the defects review rounds found in the built code
(A263) and the refusal rule his note rejected (A267): each has to be caught by at least one check. A model that cannot find them would not
have found them before the code either.

Run from the repository root:  python3 .github/scripts/audit/tankbuster_model.py
"""
import itertools
import random
import sys
from dataclasses import dataclass, field

GCD = 2.5
LOCK = 0.6            # animation lock after an own press
TICK_RES = 0.0156     # coarse clock resolution a defect variant may use


@dataclass(frozen=True)
class Act:
    name: str
    factor: float = 1.0     # damage multiplier while it stands
    barrier: float = 0.0    # share of max HP added to the budget (barrier, max HP increase)
    duration: float = 0.0
    recast: float = 0.0
    invul: bool = False
    dispelled_by_shake: bool = False


RAMPART = Act('Rampart', 0.8, 0, 20, 90)
DAMNATION = Act('Damnation', 0.6, 0, 15, 120, dispelled_by_shake=True)
BLOODWHETTING = Act('Bloodwhetting', 0.9, 0, 8, 25, dispelled_by_shake=True)
THRILL = Act('Thrill', 1.0, 0.2, 10, 90, dispelled_by_shake=True)
HOLMGANG = Act('Holmgang', 1.0, 0, 10, 240, invul=True)
KIT = [RAMPART, DAMNATION, BLOODWHETTING, THRILL]


@dataclass
class Record:
    act: Act
    press: float
    covered: bool          # duration reached the horizon at the press (window rule)
    prior_left: float      # own copy of THIS action's status left at the press
    confirmed_at: float
    seen: bool = False


@dataclass
class Status:
    act: Act
    applied: float
    shown: float

    def left(self, t):
        return self.act.duration - (t - self.applied)


@dataclass
class Defects:
    clock: bool = False           # records compared with the horizon on another, coarse clock
    order: bool = False           # status list asked before the record
    shared_status: bool = False   # renewal judged on a shared status list
    refusal_fixed: bool = False   # refused member counts only while R > two GCD
    shake_free: bool = False      # Shake It Off ignores pressed/needed members
    help_free: bool = False       # Nascent Flash on a party member ignores the plan
    hold_off: bool = False        # the hold ignores the rotation's other paths
    invul_refusal_everything: bool = False  # a refusal of the invulnerability with no retry left makes Z4
    line_ignored: bool = False    # a bundle counts as surviving down to zero, not down to the invulnerability line
    stun_noted: bool = False      # a refusal under a stun leaves the member out for a GCD, as one for MP or range
    no_deadline: bool = False     # mitigation is pressed until the cast ends, not until the measured round trip before
    no_slots: bool = False        # a bundle is chosen whether or not its members still fit before the deadline
    per_hit: bool = False         # the table rates a series by its largest hit (before A454)
    no_span: bool = False         # the series' sum without its span: members count that run out before its last hit
    invul_early: bool = False     # the invulnerability pressed once it covers the first hit, its window without the span
    z1_first_hit: bool = False    # an invulnerability counts as covering the cast once it lasts until the first hit
    z1_series_only: bool = False  # ... only once it lasts until the series' last hit, even where none can reach that far


@dataclass
class World:
    seed: int
    cast_time: float
    hit: float                    # true (= measured highest) share of max HP, unmitigated
    hp: float
    latency: float
    show_delta: float
    frames: list
    refuse_at: float | None
    refuse_twice: bool
    stun: float                   # length of the block on every ability that refuses the invulnerability
    shake_at: float | None
    help_at: float | None
    early_press: bool
    invul_ready: bool
    opt_a: bool = True            # "Use the invulnerability before a tankbuster nothing less survives"
    opt_b: bool = True            # "Spend only the mitigation a measured tankbuster needs"
    other: bool = False           # other paths of the rotation try to press rated defence on their own
    statuses: list = field(default_factory=list)
    records: list = field(default_factory=list)
    pressed: list = field(default_factory=list)     # (time, act, by)
    cooldown_until: dict = field(default_factory=dict)
    lock_until: float = 0.0
    refused_until: dict = field(default_factory=dict)
    refused_by_stun: dict = field(default_factory=dict)
    invul_pressed: bool = False
    line: float = 0.0             # "The HP% for tank to use invulnerability" (0 = no line)
    stun_at: float | None = None  # a block on every ability (stun, amnesia), from every path
    stun_len: float = 0.0
    apply_delay: float = 0.6      # from the snapshot to the damage being dealt (Holmgang counts then)
    rtt_known: bool = True        # a round trip press -> confirmation has been measured
    spacing_known: bool = True    # the time a press keeps the next one back has been measured
    offsets: list = field(default_factory=lambda: [0.0])   # each hit's snapshot after the cast's (series, A454);
                                                           # its damage follows apply_delay later
    shares: list = field(default_factory=lambda: [1.0])    # each hit's part of the series' sum

    @property
    def span(self):
        return self.offsets[-1] - self.offsets[0]


def own_left(w, act, t):
    left = -1.0
    for s in w.statuses:
        if s.act is act and t >= s.shown:
            left = max(left, s.left(t))
    return left


def shared_left(w, t):
    # a defect variant: the big mitigations share one lock-out list
    return max([s.left(t) for s in w.statuses if t >= s.shown and s.act in (RAMPART, DAMNATION)] or [-1.0])


def press(w, act, t, by, horizon_now):
    w.pressed.append((t, act, by))
    w.cooldown_until[act.name] = t + act.recast
    w.lock_until = t + LOCK
    applied = t + w.latency / 2
    w.statuses.append(Status(act, applied, applied + w.show_delta))
    w.records.append(Record(act, t, act.duration >= horizon_now, own_left(w, act, t),
                            t + w.latency))
    if act.invul:
        w.invul_pressed = True


def blocked(w, t):
    return w.stun_at is not None and w.stun_at <= t < w.stun_at + w.stun_len


def snapshot(w):
    """The end of the cast on the server, on the client's clock: the client sees it half a round trip late."""
    return w.cast_time - w.latency / 2


def counts_at_impact(w, s, offset=0.0):
    """Whether status s counts for the hit <offset> seconds after the end of the cast: applied on the server by that
    hit's snapshot (Holmgang: by its damage) and still standing then."""
    at = snapshot(w) + offset + (w.apply_delay if s.act.invul else 0.0)
    return s.applied <= at and s.left(at) >= 0


def ready_by(w, act, t, seconds):
    return w.cooldown_until.get(act.name, -1e9) <= t + seconds


def span_of(w, d):
    """The span of the series the table holds, from its first hit to its last (none in the defect variant)."""
    return 0.0 if d.no_span else w.span


def predicted_of(w, d):
    """What the table holds for the series: its sum, or its largest hit in the defect variant (before A454)."""
    return w.hit * (max(w.shares) if d.per_hit else 1.0)


def invul_horizon(w, d, R, H, span):
    """H_I: what an invulnerability has to last until to cover the cast - the series' last hit, or where none can reach
    that far, as far as one pressed at the last GCD reaches (concept 09, A454)."""
    if d.z1_first_hit:
        return H - span
    if d.z1_series_only:
        return H
    return min(H, R + HOLMGANG.duration - GCD)


def invul_window(w, d, act):
    """The rest of the cast at which the invulnerability goes out: late enough to stand until H_S, at the latest at the
    last GCD, where Z3 stops holding the rest (concept 09, A454)."""
    span = 0.0 if d.invul_early else w.span
    return min(act.duration - GCD, max(act.duration - GCD - span, GCD))


def plan(w, d, t, read_t, R_read, H_read, P_read, B_read):
    """The concept's decision at rotation time t, from the forecast's reading at read_t."""
    lag = t - read_t
    if d.clock:
        lag = round(lag / TICK_RES) * TICK_RES - TICK_RES
    R = max(0.0, R_read - lag)
    H = max(0.0, H_read - lag)
    span = span_of(w, d)
    HI = invul_horizon(w, d, R, H, span)

    plan.counted = set()
    plan.double = set()
    # Z1: an invulnerability over H_I - status, or a record that stands
    for r in w.records:
        if r.act.invul and not r.seen and (r.covered or r.act.duration - (t - r.press) >= HI):
            return 'Z1', [], R, H
    if any(s.act.invul and t >= s.shown and s.left(t) >= HI for s in w.statuses):
        return 'Z1', [], R, H

    pending, standing_live = [], set()
    for act in KIT:
        rec = next((r for r in reversed(w.records) if r.act is act), None)
        rec_stands = False
        if rec and not rec.seen and (t - rec.press <= GCD or t >= rec.confirmed_at):
            prior = shared_left(w, rec.press) if d.shared_status else rec.prior_left
            prior_now = prior - (t - rec.press)
            cover = act.duration - (t - rec.press)
            if d.clock:
                cover = act.duration - (round((t - rec.press) / TICK_RES) * TICK_RES + TICK_RES)
            if prior_now < H and (cover >= H or (rec.covered and not d.clock)):
                rec_stands = True
        live = own_left(w, act, t) >= H
        if d.order:
            if live:
                standing_live.add(act.name)
                continue
            if rec_stands:
                pending.append(act)
        else:
            if rec_stands:
                pending.append(act)
                continue
            if live:
                standing_live.add(act.name)

    predicted = P_read
    budget = B_read
    for a in pending:
        predicted *= a.factor
        budget += a.barrier

    cands = []
    for act in KIT:
        if act in pending or act.name in standing_live:
            continue
        if act.duration - GCD - span <= GCD or not ready_by(w, act, t, max(0.0, R - GCD)):
            continue
        refused_left = max(0.0, w.refused_until.get(act.name, -1) - t)
        limit = (R > 2 * GCD) if d.refusal_fixed else (R - refused_left > GCD)
        if refused_left > 0 and not limit:
            continue
        cands.append((act, refused_left > 0))

    if not w.opt_b:
        cands = []

    # The deadline for a mitigation: the cast's end less the measured round trip (none before a measurement);
    # the presses that fit until then, from the end of the animation lock or the stun, whose rest the status names.
    rtt = w.latency if w.rtt_known else 0.0
    D = R if d.no_deadline else R - rtt
    free = max(0.0, w.lock_until - t)
    if blocked(w, t):
        free = max(free, w.stun_at + w.stun_len - t)
    plan.free = free
    if not w.spacing_known or d.no_slots:
        slots = len(KIT)
    else:
        slots = 0 if D <= free else int(-(-(D - free) // LOCK))
    # a block past the end of the cast: nothing to do for it, and nothing held for it
    if free >= R and not d.no_slots:
        return 'Z0', [], R, H

    best = None
    for k in range(min(len(cands), slots) + 1):
        for combo in itertools.combinations(cands, k):
            f = 1.0
            b = budget
            for a, _ in combo:
                f *= a.factor
                b += a.barrier
            if predicted * f < b - (0.0 if d.line_ignored else w.line):
                cost = sum(a.recast for a, _ in combo)
                refused = any(x for _, x in combo)
                key = (refused, cost, len(combo))
                if best is None or key < best[0]:
                    best = (key, [a for a, _ in combo])
    plan.counted = {a.name for a in pending}
    plan.double = {a.name for a in pending} & plan.in_reading
    if best is not None:
        return 'Z2', best[1], R, H
    if w.opt_a and w.invul_ready and ready_by(w, HOLMGANG, t, max(0.0, R - GCD)) \
            and not (d.invul_refusal_everything and not invul_retry_left(w, t, R)):
        return 'Z3', [HOLMGANG], R, H
    # everything: the most valuable member first
    return 'Z4', sorted((a for a, _ in cands), key=lambda a: -(P_read * (1 - a.factor) + a.barrier)), R, H


def held(w, state, members, act, R, span=0.0):
    """The concept's hold for a press from another path (no command, one known cast)."""
    if not w.opt_b:
        return False
    if state in ('Z1',):
        return True
    if state in ('Z0', 'Z4') or R - (w.latency if w.rtt_known else 0.0) <= 0:
        return False
    if state == 'Z3':
        return R > GCD
    if state == 'Z2':
        if R <= GCD and any(ready_by(w, m, 0, 0) and m.name not in [a.name for _, a, _ in w.pressed] for m in members):
            return False
        return not (act in members and R <= act.duration - GCD - span)
    return False


def invul_retry_left(w, t, R):
    """The rule his note rejected (defect variant only): a refusal of the invulnerability with no retry left
    before the last GCD made Z4."""
    left = max(0.0, w.refused_until.get(HOLMGANG.name, -1) - t)
    return left <= 0 or R - left > GCD


def bundle_exists_true(w, t):
    """Oracle for O3 (invariant 1): a bundle that survives, of members that can still be pressed before
    the last GCD - off cooldown by then, and not refused with the refusal running into the last GCD
    (a member that can only be retried inside it is no means "ohne Risiko")."""
    R = w.cast_time - t
    acts = [a for a in KIT if ready_by(w, a, t, max(0.0, R - GCD)) and a.duration - GCD - w.span > GCD
            and own_left(w, a, w.cast_time + GCD + w.span) < 0
            and not (w.refused_until.get(a.name, -1) > t and w.cast_time - w.refused_until[a.name] <= GCD
                     and not w.refused_by_stun.get(a.name))]
    if not w.opt_b:
        acts = []   # invariant 1: without "Spend only ..." nothing presses a bundle, only the empty one counts
    # the presses that truly fit before the true deadline (a round trip before the end of the cast)
    free = max(0.0, w.lock_until - t)
    if blocked(w, t):
        free = max(free, w.stun_at + w.stun_len - t)
    D = R - w.latency
    fit = 0 if D <= free else int(-(-(D - free) // LOCK))
    # the concept's horizon: the first hit lands at most one GCD after the cast ends, the last its span later
    # ("ohne Risiko", A454)
    standing = [s.act for s in w.statuses if s.left(w.cast_time + GCD + w.span) >= 0 and not s.act.invul]
    for k in range(min(len(acts), fit) + 1):
        for combo in itertools.combinations(acts, k):
            f, b = 1.0, w.hp
            for a in list(combo) + standing:
                f *= a.factor
                b += a.barrier
            if w.hit * f < b - w.line:
                return True
    return False


def run(w, d):
    t = 0.0
    read_t = 0.0
    R_read = w.cast_time
    span = span_of(w, d)
    H_read = R_read + GCD + span
    P_read, B_read = predicted_of(w, d), w.hp
    tracked = {}
    violations = []
    last_state = None
    rng = random.Random(w.seed)
    plan.in_reading = set()
    run.slot_after_stun = False
    for i, dt in enumerate(w.frames):
        t += dt
        next_dt = w.frames[i + 1] if i + 1 < len(w.frames) else dt
        if t >= w.cast_time:
            break
        state, members, R, H = plan(w, d, t, read_t, R_read, H_read, P_read, B_read)
        last_state = state

        # O8: "everything" only while the invulnerability is not available (option, cooldown) - never because it
        # was refused: what refuses it in the game blocks every other ability alike (his note, 12.10.2026)
        Rt = w.cast_time - t
        if state == 'Z4' and w.opt_a and w.invul_ready \
                and ready_by(w, HOLMGANG, t, max(0.0, Rt - GCD)) and not bundle_exists_true(w, t):
            violations.append(('O8', w.seed, t, 'everything planned while the invulnerability is available'))

        # O5: a pressed plan member that truly stands at impact is in the plan's arithmetic every frame -
        # through the forecast's reading or as a record. O6: never through both.
        if state in ('Z2', 'Z4'):
            counted = plan.counted | plan.in_reading
            for name, act in tracked.items():
                truly = any(s.act is act and s.left(w.cast_time) >= 0 for s in w.statuses)
                if truly and name not in counted:
                    violations.append(('O5', w.seed, t, name))
            for name in plan.double:
                violations.append(('O6', w.seed, t, name))

        # O11 (checked at the end): a press left after a stun with the invulnerability available
        if blocked(w, t) and not blocked(w, t + dt) and w.opt_a and w.invul_ready \
                and w.stun_at + w.stun_len < w.cast_time and ready_by(w, HOLMGANG, t, 0):
            run.slot_after_stun = True

        # the plan's presses; under a stun nothing goes out from any path, and the refusal is no answer about
        # the member (it is noted for a GCD only in the defect variant)
        if t >= w.lock_until and blocked(w, t):
            if d.stun_noted:
                for act in members:
                    if not act.invul and act.name not in w.refused_until:
                        w.refused_until[act.name] = t + GCD
                        w.refused_by_stun[act.name] = True
        elif t >= w.lock_until:
            for act in members:
                if act.invul and state == 'Z3':
                    if R <= invul_window(w, d, act):
                        if w.refuse_at is not None and abs(t - w.refuse_at) < dt and act.name not in w.refused_until:
                            # in the game only a block on every ability refuses it (stun, amnesia): nothing goes
                            # out from any path until it ends; RSR notes the refusal for a GCD
                            w.refused_until[act.name] = t + GCD
                            w.lock_until = t + w.stun
                            break
                        if d.invul_refusal_everything and w.refused_until.get(act.name, -1) > t:
                            continue   # the rejected rule: no retry for a GCD after a refusal
                        if bundle_exists_true(w, t):
                            violations.append(('O3', w.seed, t, act.name))
                        press(w, act, t, 'plan', invul_horizon(w, d, R, H, span))
                        break
                    continue
                if not w.opt_b:
                    continue
                if not d.no_deadline and R - (w.latency if w.rtt_known else 0.0) <= 0:
                    continue   # after the deadline a press for this cast is lost
                if any(a is act for _, a, _ in w.pressed[-1:]) and t - w.pressed[-1][0] < LOCK:
                    continue
                if R > act.duration - GCD - span or not ready_by(w, act, t, 0):
                    continue
                if w.refused_until.get(act.name, -1) > t:
                    continue
                if w.refuse_at is not None and w.refuse_at <= t < w.refuse_at + dt + 0.001 \
                        and act.name not in w.refused_until:
                    w.refused_until[act.name] = t + GCD
                    if w.refuse_twice:
                        w.refuse_at = t + GCD + 0.01
                    break
                press(w, act, t, 'plan', H)
                tracked[act.name] = act
                break

        # free paths
        if w.other and t >= w.lock_until and not blocked(w, t) and rng.random() < 0.05:
            want = rng.choice(KIT)
            if ready_by(w, want, t, 0) and (d.hold_off or not held(w, state, members, want, R, span)):
                press(w, want, t, 'other', H)
        if w.shake_at is not None and abs(t - w.shake_at) < dt:
            needed = any(a.dispelled_by_shake and a.name in tracked for a in KIT)
            if d.shake_free or not needed:
                w.statuses = [s for s in w.statuses if not s.act.dispelled_by_shake]
            w.shake_at = None
        if w.help_at is not None and abs(t - w.help_at) < dt:
            needed = BLOODWHETTING in members or BLOODWHETTING.name in tracked
            if (d.help_free or not needed) and ready_by(w, BLOODWHETTING, t, 0):
                w.cooldown_until[BLOODWHETTING.name] = t + BLOODWHETTING.recast
            w.help_at = None

        # O2: nothing rated after the invulnerability
        if w.invul_pressed and w.opt_b:
            later = [a for (tp, a, by) in w.pressed if not a.invul and tp > min(tp2 for tp2, a2, _ in w.pressed if a2.invul)]
            if later:
                violations.append(('O2', w.seed, t, later[-1].name))

        # the forecast reads after the rotation
        # within this frame: before the next one starts
        gap = rng.uniform(0.0, next_dt * 0.3)
        read_t = t + gap
        R_read = w.cast_time - read_t
        H_read = R_read + GCD + span
        P_read, B_read = predicted_of(w, d), w.hp
        plan.in_reading = set()
        for s in w.statuses:
            if read_t >= s.shown and s.left(read_t) >= H_read and not s.act.invul:
                P_read *= s.act.factor
                B_read += s.act.barrier
                plan.in_reading.add(s.act.name)
        for r in w.records:
            if not r.seen and own_left(w, r.act, read_t) > r.prior_left:
                r.seen = True

    # O1 and O4 at impact: what the server applied by each hit's snapshot (Holmgang: by the damage); a series takes
    # every hit under what stands at it, and its damage adds up (A454)
    taken, survived, margin, invul_holds = 0.0, True, 0.0, False
    for offset, part in zip(w.offsets, w.shares):
        f, b = 1.0, w.hp
        for s in w.statuses:
            if not s.act.invul and counts_at_impact(w, s, offset):
                f *= s.act.factor
                b += s.act.barrier
        invul_holds = any(s.act.invul and counts_at_impact(w, s, offset) for s in w.statuses)
        if not invul_holds:
            taken += w.hit * part * f
            if taken >= b:
                survived = False
        margin = b - taken
    series = len(w.offsets) > 1
    if last_state == 'Z2' and not survived:
        violations.append(('O12' if series else 'O1', w.seed, w.cast_time, 'survivable plan did not survive'))
    if series and (last_state == 'Z1' or (w.invul_pressed and last_state == 'Z3')) and not survived:
        violations.append(('O12', w.seed, w.cast_time, 'the invulnerability ran out before the series was over'))
    if last_state == 'Z2' and not invul_holds and margin <= w.line:
        violations.append(('O9', w.seed, w.cast_time, 'the planned bundle left the tank at the invulnerability line'))
    for (tp, act, by) in w.pressed:
        if by.startswith('plan') and not act.invul and \
                not any(s.act is act and s.left(w.cast_time) >= 0 for s in w.statuses) and \
                not (w.shake_at is None and act.dispelled_by_shake and False):
            if not any(s.act is act for s in w.statuses):
                violations.append(('O4', w.seed, tp, act.name))
        # O10: a mitigation the plan pressed for this cast reached the server after the snapshot
        if by.startswith('plan') and not act.invul and tp + w.latency / 2 > snapshot(w):
            violations.append(('O10', w.seed, tp, act.name + ' pressed after the deadline'))
    if run.slot_after_stun and not survived and w.opt_b:
        violations.append(('O11', w.seed, w.cast_time, 'a press was left after the stun and the tank died'))
    run.survived = survived
    return violations


def world(seed, scenario):
    rng = random.Random(seed)
    frames = []
    for _ in range(4000):
        dt = rng.choice([1 / 144, 1 / 60, 1 / 30])
        if rng.random() < 0.02:
            dt += rng.uniform(0.03, 0.2)
        frames.append(dt)
    hp = rng.uniform(0.5, 1.0)
    hit = rng.uniform(0.6, 1.8)
    if scenario.get('tight'):
        # every member needed: all four survive it, any three do not (Bloodwhetting the weakest)
        hit = (hp + THRILL.barrier) / (RAMPART.factor * DAMNATION.factor * BLOODWHETTING.factor) * rng.uniform(0.92, 0.99)
    late = scenario.get('late_refusal')
    if late:
        # a short stun at the first press of the invulnerability, the refusal's GCD running into the last GCD;
        # nothing less survives
        hit = rng.uniform(3.0, 4.0)
    cast_time = rng.choice([4.0, 4.5]) if late else rng.choice([6.0, 8.0, 10.0, 12.0]) if scenario.get('tight') \
        else rng.choice([4.0, 5.0, 6.0, 8.0, 10.0, 12.0])
    stun_at, stun_len = None, 0.0
    if scenario.get('stun'):
        stun_at, stun_len = rng.uniform(0.0, cast_time), rng.uniform(0.3, 2.5)
    if scenario.get('stun_late'):
        # a stun that ends shortly before the end of the cast: few presses or none left
        end = cast_time - rng.uniform(0.05, 1.5)
        stun_len = rng.uniform(0.5, 2.5)
        stun_at = max(0.0, end - stun_len)
    if scenario.get('stun_long'):
        # stunned from the start of the cast until shortly before its end: every press falls into the last moment
        stun_at, stun_len = 0.0, cast_time - rng.uniform(0.02, 1.2)
    # a tankbuster of several hits (A454), shaped after the series concept 13 names: Lead Hook (three hits 1.1 s apart),
    # Crush Helm (four small hits, the large one 6.1 s after the cast), Bitter Whirlwind (three hits 3.3 and 3.1 s
    # apart), or any series up to 6.4 s long
    offsets, shares = [0.0], [1.0]
    kind = scenario.get('series')
    if kind == 'lead_hook':
        offsets, shares = [0.0, 1.1, 2.2], [1 / 3] * 3
    elif kind == 'crush_helm':
        # the large hit's snapshot 5.8 s after the cast's, its damage apply_delay later: 6.2 s or more after the cast
        offsets, shares = [0.0, 1.0, 2.0, 3.0, 5.8], [0.04] * 4 + [0.84]
    elif kind == 'whirlwind':
        offsets, shares = [0.0, 3.3, 6.4], [1 / 3] * 3
    elif kind == 'random':
        offsets = [0.0]
        for _ in range(rng.randint(1, 4)):
            nxt = offsets[-1] + rng.uniform(0.8, 3.3)
            if nxt > 6.4:
                break
            offsets.append(nxt)
        weights = [rng.uniform(0.1, 1.0) for _ in offsets]
        shares = [x / sum(weights) for x in weights]
    statuses = []
    cooldown = {DAMNATION.name: 1e9} if scenario.get('no_damnation') else {}
    if scenario.get('invul_standing'):
        # pressed by hand before the cast, ending during the series; on cooldown for this cast
        applied = cast_time - rng.uniform(4.0, 8.0)
        statuses.append(Status(HOLMGANG, applied, applied))
        cooldown[HOLMGANG.name] = 1e9
    return World(
        seed=seed,
        cast_time=cast_time,
        stun_at=stun_at,
        stun_len=stun_len,
        apply_delay=rng.uniform(0.4, 1.0),
        rtt_known=not scenario.get('no_rtt'),
        spacing_known=not scenario.get('no_spacing'),
        hit=hit,
        hp=hp,
        latency=rng.uniform(0.03, 0.3),
        show_delta=rng.uniform(-0.05, 0.12),
        frames=frames,
        refuse_at=frames[0] / 2 if late else rng.uniform(0.0, 6.0) if scenario.get('refuse') else None,
        refuse_twice=bool(scenario.get('refuse_twice')),
        stun=rng.uniform(0.3, 1.5),
        shake_at=rng.uniform(0.5, 7.0) if scenario.get('shake') else None,
        help_at=rng.uniform(0.5, 7.0) if scenario.get('help') else None,
        early_press=bool(scenario.get('early')),
        invul_ready=scenario.get('invul', True),
        opt_a=scenario.get('opt_a', True),
        opt_b=scenario.get('opt_b', True),
        other=bool(scenario.get('other')),
        cooldown_until=cooldown,
        statuses=statuses,
        line=scenario.get('line', 0.0),
        offsets=offsets,
        shares=shares,
    )


SCENARIOS = [
    {}, {'refuse': True}, {'refuse': True, 'refuse_twice': True}, {'shake': True}, {'help': True},
    {'invul': False}, {'no_damnation': True}, {'refuse': True, 'invul': False},
    {'shake': True, 'no_damnation': True}, {'help': True, 'invul': False},
    {'tight': True}, {'tight': True, 'refuse': True}, {'tight': True, 'help': True}, {'tight': True, 'shake': True},
    {'tight': True, 'help': True, 'invul': False},
    {'other': True}, {'other': True, 'refuse': True}, {'other': True, 'tight': True}, {'other': True, 'invul': False},
    {'opt_b': False}, {'opt_a': False}, {'opt_a': False, 'tight': True}, {'opt_b': False, 'other': True},
    {'late_refusal': True}, {'late_refusal': True, 'other': True},
    # the invulnerability line (A304): a bundle has to leave the tank above it
    {'line': 0.15}, {'line': 0.3}, {'line': 0.15, 'tight': True}, {'line': 0.15, 'refuse': True},
    {'line': 0.3, 'shake': True}, {'line': 0.15, 'help': True}, {'line': 0.3, 'invul': False},
    {'line': 0.15, 'other': True},
    # a stun that blocks every path, the deadline before the snapshot and the presses that fit until then
    {'stun': True}, {'stun': True, 'tight': True}, {'stun': True, 'invul': False}, {'stun': True, 'other': True},
    {'stun_late': True}, {'stun_late': True, 'tight': True}, {'stun_late': True, 'invul': False},
    {'stun_late': True, 'line': 0.15}, {'stun': True, 'refuse': True},
    {'no_rtt': True}, {'no_spacing': True}, {'no_rtt': True, 'stun_late': True},
    {'stun_long': True}, {'stun_long': True, 'tight': True}, {'stun_long': True, 'invul': False},
    {'stun_long': True, 'line': 0.15},
    # a tankbuster of several hits: its sum, its span, the invulnerability until its last hit (A454)
    {'series': 'lead_hook'}, {'series': 'crush_helm'}, {'series': 'whirlwind'}, {'series': 'random'},
    {'series': 'crush_helm', 'invul': False}, {'series': 'whirlwind', 'invul': False}, {'series': 'random', 'invul': False},
    {'series': 'random', 'tight': True}, {'series': 'random', 'line': 0.15}, {'series': 'random', 'other': True},
    {'series': 'crush_helm', 'stun': True}, {'series': 'whirlwind', 'other': True}, {'series': 'random', 'shake': True},
    {'series': 'lead_hook', 'no_rtt': True},
    {'series': 'whirlwind', 'invul_standing': True}, {'series': 'crush_helm', 'invul_standing': True},
    {'series': 'random', 'invul_standing': True},
    # Left out: a long stun before the spacing of presses has been measured. Until then the plan does not count
    # presses, as before the deadline was built, and a set that no longer fits after the stun kills (O1, O11). The
    # measurement comes from any instant press without a lock running, so it stands from the first press after the
    # plugin loads; a stun on a tankbuster before that is not a state of play (concept 09, "Grenzen").
]


def check(defects, runs_per_scenario=300, seed0=1, first=False):
    """Every violation over all scenarios; with first, stop at the first one (enough for the self-test)."""
    found = {}
    for si, sc in enumerate(SCENARIOS):
        for i in range(runs_per_scenario):
            if first and found:
                return found
            w = world(seed0 + si * 100000 + i, sc)
            for v in run(w, defects):
                found.setdefault(v[0], []).append((sc, v))
            # O7: a free path must not turn a hit the plan alone survived into a death
            # - nor force the invulnerability where the plan alone survived with a bundle (concept 09, K1-K4:
            # the invulnerability is no replacement for a member a free path took)
            if sc.get('shake') or sc.get('help'):
                with_free, invul_with = run.survived, w.invul_pressed
                base = world(seed0 + si * 100000 + i, {k: v for k, v in sc.items() if k not in ('shake', 'help')})
                run(base, defects)
                if run.survived and not base.invul_pressed and (not with_free or invul_with):
                    found.setdefault('O7', []).append((sc, ('O7', base.seed, 0.0,
                                                            'a free path cost the tank or forced the invulnerability')))
    return found


def self_test():
    variants = {
        'clock': Defects(clock=True),
        'order': Defects(order=True),
        'shared_status': Defects(shared_status=True),
        'refusal_fixed': Defects(refusal_fixed=True),
        'shake_free': Defects(shake_free=True),
        'help_free': Defects(help_free=True),
        'hold_off': Defects(hold_off=True),
        'invul_refusal_everything': Defects(invul_refusal_everything=True),
        'line_ignored': Defects(line_ignored=True),
        'stun_noted': Defects(stun_noted=True),
        'no_deadline': Defects(no_deadline=True),
        'no_slots': Defects(no_slots=True),
        'per_hit': Defects(per_hit=True),
        'no_span': Defects(no_span=True),
        'invul_early': Defects(invul_early=True),
        'z1_first_hit': Defects(z1_first_hit=True),
        'z1_series_only': Defects(z1_series_only=True),
    }
    missed = []
    for name, d in variants.items():
        if not check(d, runs_per_scenario=120, first=True):
            missed.append(name)
    if missed:
        print('SELF-TEST FAILED: the model does not catch the constructed defect(s):', ', '.join(missed))
        return False
    print(f'self-test ok: each of {len(variants)} defects (found by review, rejected by his note, or planned against) '
          f'is caught ({", ".join(variants)})')
    return True


def main():
    if not self_test():
        return 2
    found = check(Defects())
    if found:
        print('the concept\'s rules violate:')
        for key, items in sorted(found.items()):
            sc, v = items[0]
            print(f'  {key}: {len(items)} run(s), first: scenario {sc}, seed {v[1]}, t={v[2]:.3f}, {v[3]}')
        return 1
    print(f'{len(SCENARIOS)} scenario(s) x 300 runs: no check violated by the concept\'s rules')
    return 0


if __name__ == '__main__':
    sys.exit(main())

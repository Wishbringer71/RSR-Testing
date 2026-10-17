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
      own emergency rule draws the invulnerability right after the hit, and both are spent (A304).

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
    invul_pressed: bool = False
    line: float = 0.0             # "The HP% for tank to use invulnerability" (0 = no line)


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


def ready_by(w, act, t, seconds):
    return w.cooldown_until.get(act.name, -1e9) <= t + seconds


def plan(w, d, t, read_t, R_read, H_read, P_read, B_read):
    """The concept's decision at rotation time t, from the forecast's reading at read_t."""
    lag = t - read_t
    if d.clock:
        lag = round(lag / TICK_RES) * TICK_RES - TICK_RES
    R = max(0.0, R_read - lag)
    H = max(0.0, H_read - lag)

    plan.counted = set()
    plan.double = set()
    # Z1: an invulnerability over the horizon - status, or a record that stands
    for r in w.records:
        if r.act.invul and not r.seen and (r.covered or r.act.duration - (t - r.press) >= H):
            return 'Z1', [], R, H
    if any(s.act.invul and t >= s.shown and s.left(t) >= H for s in w.statuses):
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
        if act.duration - GCD <= GCD or not ready_by(w, act, t, max(0.0, R - GCD)):
            continue
        refused_left = max(0.0, w.refused_until.get(act.name, -1) - t)
        limit = (R > 2 * GCD) if d.refusal_fixed else (R - refused_left > GCD)
        if refused_left > 0 and not limit:
            continue
        cands.append((act, refused_left > 0))

    if not w.opt_b:
        cands = []
    best = None
    for k in range(len(cands) + 1):
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
    return 'Z4', [a for a, _ in cands], R, H


def held(w, state, members, act, R):
    """The concept's hold for a press from another path (no command, one known cast)."""
    if not w.opt_b:
        return False
    if state in ('Z1',):
        return True
    if state == 'Z3':
        return R > GCD
    if state == 'Z2':
        if R <= GCD and any(ready_by(w, m, 0, 0) and m.name not in [a.name for _, a, _ in w.pressed] for m in members):
            return False
        return not (act in members and R <= act.duration - GCD)
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
    acts = [a for a in KIT if ready_by(w, a, t, max(0.0, R - GCD)) and a.duration - GCD > GCD
            and own_left(w, a, w.cast_time + GCD) < 0
            and not (w.refused_until.get(a.name, -1) > t and w.cast_time - w.refused_until[a.name] <= GCD)]
    if not w.opt_b:
        acts = []   # invariant 1: without "Spend only ..." nothing presses a bundle, only the empty one counts
    # the concept's horizon: the hit lands at most one GCD after the cast ends ("ohne Risiko")
    standing = [s.act for s in w.statuses if s.left(w.cast_time + GCD) >= 0 and not s.act.invul]
    for k in range(len(acts) + 1):
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
    H_read = R_read + GCD
    P_read, B_read = w.hit, w.hp
    tracked = {}
    violations = []
    last_state = None
    rng = random.Random(w.seed)
    plan.in_reading = set()
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

        # the plan's presses
        if t >= w.lock_until:
            for act in members:
                if act.invul and state == 'Z3':
                    if R <= act.duration - GCD:
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
                        press(w, act, t, 'plan', H)
                        break
                    continue
                if not w.opt_b:
                    continue
                if any(a is act for _, a, _ in w.pressed[-1:]) and t - w.pressed[-1][0] < LOCK:
                    continue
                if R > act.duration - GCD or not ready_by(w, act, t, 0):
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
        if w.other and t >= w.lock_until and rng.random() < 0.05:
            want = rng.choice(KIT)
            if ready_by(w, want, t, 0) and (d.hold_off or not held(w, state, members, want, R)):
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
        H_read = R_read + GCD
        P_read, B_read = w.hit, w.hp
        plan.in_reading = set()
        for s in w.statuses:
            if read_t >= s.shown and s.left(read_t) >= H_read and not s.act.invul:
                P_read *= s.act.factor
                B_read += s.act.barrier
                plan.in_reading.add(s.act.name)
        for r in w.records:
            if not r.seen and own_left(w, r.act, read_t) > r.prior_left:
                r.seen = True

    # O1 and O4 at impact
    f, b = 1.0, w.hp
    for s in w.statuses:
        if s.left(w.cast_time) >= 0:
            f *= s.act.factor
            b += s.act.barrier
    survived = any(s.act.invul and s.left(w.cast_time) >= 0 for s in w.statuses) or w.hit * f < b
    if last_state == 'Z2' and not survived:
        violations.append(('O1', w.seed, w.cast_time, 'survivable plan did not survive'))
    if last_state == 'Z2' and not any(s.act.invul and s.left(w.cast_time) >= 0 for s in w.statuses) \
            and b - w.hit * f <= w.line:
        violations.append(('O9', w.seed, w.cast_time, 'the planned bundle left the tank at the invulnerability line'))
    for (tp, act, by) in w.pressed:
        if by.startswith('plan') and not act.invul and \
                not any(s.act is act and s.left(w.cast_time) >= 0 for s in w.statuses) and \
                not (w.shake_at is None and act.dispelled_by_shake and False):
            if not any(s.act is act for s in w.statuses):
                violations.append(('O4', w.seed, tp, act.name))
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
    return World(
        seed=seed,
        cast_time=rng.choice([4.0, 4.5]) if late else rng.choice([6.0, 8.0, 10.0, 12.0]) if scenario.get('tight')
        else rng.choice([4.0, 5.0, 6.0, 8.0, 10.0, 12.0]),
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
        cooldown_until={DAMNATION.name: 1e9} if scenario.get('no_damnation') else {},
        line=scenario.get('line', 0.0),
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
]


def check(defects, runs_per_scenario=300, seed0=1):
    found = {}
    for si, sc in enumerate(SCENARIOS):
        for i in range(runs_per_scenario):
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
    }
    missed = []
    for name, d in variants.items():
        if not check(d, runs_per_scenario=120):
            missed.append(name)
    if missed:
        print('SELF-TEST FAILED: the model does not catch the constructed defect(s):', ', '.join(missed))
        return False
    print(f'self-test ok: each of {len(variants)} defects (found by review, or rejected by his note) is caught '
          f'({", ".join(variants)})')
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

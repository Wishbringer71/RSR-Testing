#!/usr/bin/env python3
"""How the tanks' two personal mitigations are spread over a pull and over a boss (concept 09, Warrior).

Not a code scanner - a model calculator for the decision the owner asked to be worked through: the
second big mitigation of every tank waits behind the first (Upstream abe6132d3). Rampart goes out
only 30 s after the big one (Damnation, Guardian, Shadowed Vigil, Great Nebula and their lower
forms), the big one only while Rampart is ready or 60 s after it; both lock each other out while
either status runs (StatusProvide = RampartStatus).

Game data (job guide, 01.10.2026; DefensiveValues): Rampart -20 % for 20 s, recast 90 s; the big
one -40 % for 15 s (-30 % below level 92 and its counterparts), recast 120 s. Mitigations multiply.

Policies:
  P0  today: big when Rampart is ready or 60 s gone, Rampart when the big one is 30 s gone
  P1  status only: each goes out as soon as it is ready and the other is not running
  P3  both at once at the first chance (no stagger at all)

Situations:
  - one pull of length L, both ready at its start; the pack's output falls linearly to zero as it
    dies (ASSUMPTION; a constant stream is reported beside it), damage taken relative to none
  - wall-to-wall: pulls back to back with a short gap, cooldowns carried over
  - a boss without BossModReborn: the defence opens only when a tankbuster cast shows, every T
    seconds; reported is the share of tankbusters that land under at least one of the two

The decision cadence is one second (a weave slot); a cast lands in the same second. The self-test
checks the arithmetic on hand-computed cases.

Usage: python3 .github/scripts/audit/tank_mitigation_stagger_model.py
"""

RAMPART = (0.20, 20.0, 90.0)
BIG = (0.40, 15.0, 120.0)


class Tank:
    def __init__(self, big=BIG):
        self.big = big
        self.ready = {'big': 0.0, 'ramp': 0.0}
        self.used = {'big': None, 'ramp': None}

    def running(self, t):
        out = []
        for key, (_, dur, _) in (('big', self.big), ('ramp', RAMPART)):
            u = self.used[key]
            if u is not None and u <= t < u + dur:
                out.append(key)
        return out

    def factor(self, t):
        f = 1.0
        for key in self.running(t):
            f *= 1.0 - (self.big[0] if key == 'big' else RAMPART[0])
        return f

    def elapsed(self, key, t):
        u = self.used[key]
        return None if u is None else t - u

    def is_ready(self, key, t):
        return self.ready[key] <= t

    def cast(self, key, t):
        self.used[key] = t
        self.ready[key] = t + (self.big[2] if key == 'big' else RAMPART[2])

    def decide(self, policy, t):
        """One weave slot while the defence is open; returns what went out."""
        locked = bool(self.running(t))
        if policy == 'P3':
            for key in ('big', 'ramp'):
                if self.is_ready(key, t):
                    self.cast(key, t)
                    return key
            return None
        if locked:
            return None
        if policy == 'P1':
            for key in ('big', 'ramp'):
                if self.is_ready(key, t):
                    self.cast(key, t)
                    return key
            return None
        # P0
        er = self.elapsed('ramp', t)
        eb = self.elapsed('big', t)
        if self.is_ready('big', t) and (self.is_ready('ramp', t) or (er is not None and er >= 60)):
            self.cast('big', t)
            return 'big'
        if self.is_ready('ramp', t) and eb is not None and eb >= 30:
            self.cast('ramp', t)
            return 'ramp'
        return None


def pull_damage(policy, length, falling=True, tank=None, start=0.0):
    """Damage taken over one pull relative to an unmitigated one; the tank carries its cooldowns."""
    tank = tank or Tank()
    taken = whole = 0.0
    t = start
    while t < start + length:
        tank.decide(policy, t)
        intake = (1.0 - (t - start) / length) if falling else 1.0
        taken += intake * tank.factor(t)
        whole += intake
        t += 1.0
    return taken / whole


def wall_to_wall(policy, pulls, length, gap, falling=True):
    tank = Tank()
    shares = []
    t = 0.0
    for _ in range(pulls):
        shares.append(pull_damage(policy, length, falling, tank, t))
        t += length + gap
    return shares


def boss_busters(policy, every, fight=600.0, lead=4.0):
    """Share of tankbusters landing under at least one of the two, the defence opening at the cast."""
    tank = Tank()
    covered = total = 0
    hit = every
    while hit < fight:
        cast_at = hit - lead
        t = cast_at
        while t <= hit - 1.0:
            tank.decide(policy, t)
            t += 1.0
        total += 1
        covered += tank.factor(hit) < 1.0
        hit += every
    return covered, total


def self_test():
    # P0 on a fresh tank: big at 0, Rampart at 30 - nothing from 15 to 30.
    tk = Tank()
    for t in range(0, 40):
        tk.decide('P0', float(t))
    assert tk.used == {'big': 0.0, 'ramp': 30.0}, tk.used
    # P1: big at 0, Rampart the second the big one ends.
    tk = Tank()
    for t in range(0, 40):
        tk.decide('P1', float(t))
    assert tk.used == {'big': 0.0, 'ramp': 15.0}, tk.used
    # P3: both at once.
    tk = Tank()
    tk.decide('P3', 0.0)
    tk.decide('P3', 0.0)
    assert tk.used == {'big': 0.0, 'ramp': 0.0}, tk.used
    assert abs(tk.factor(0.0) - 0.6 * 0.8) < 1e-9
    # Constant stream, 35 s pull: P1 covers 15 s at 0.6 and 20 s at 0.8 -> (9 + 16) / 35.
    assert abs(pull_damage('P1', 35, falling=False) - 25.0 / 35.0) < 1e-9
    # P0 on the same pull: 15 s at 0.6, 15 s bare, 5 s at 0.8 -> (9 + 15 + 4) / 35.
    assert abs(pull_damage('P0', 35, falling=False) - 28.0 / 35.0) < 1e-9
    print('self-test ok: P0, P1 and P3 cast where hand-computed; pull arithmetic matches\n')


def report():
    print('One pull, both ready at its start - damage taken relative to none (lower is better):')
    print(f'  {"length":>6}  {"falling stream":^26}  {"constant stream":^26}')
    print(f'  {"":>6}  {"P0":>7} {"P1":>7} {"P3":>7}     {"P0":>7} {"P1":>7} {"P3":>7}')
    for length in (20, 30, 45, 60, 90):
        f = [pull_damage(p, length) for p in ('P0', 'P1', 'P3')]
        c = [pull_damage(p, length, falling=False) for p in ('P0', 'P1', 'P3')]
        print(f'  {length:>4} s  ' + ' '.join(f'{x:7.3f}' for x in f) + '     ' + ' '.join(f'{x:7.3f}' for x in c))
    print('\nWall-to-wall, cooldowns carried over - damage taken per pull (falling stream):')
    for pulls, length, gap in ((4, 30, 10), (4, 45, 10), (3, 60, 15), (5, 25, 5), (3, 40, 30), (2, 90, 20)):
        print(f'  {pulls} pulls of {length} s, {gap} s apart')
        for p in ('P0', 'P1', 'P3'):
            shares = wall_to_wall(p, pulls, length, gap)
            print(f'    {p}: ' + ' '.join(f'{s:.3f}' for s in shares)
                  + f'   mean {sum(shares) / len(shares):.3f}  worst {max(shares):.3f}')
    print('\nBoss without BossModReborn - tankbusters landing under Rampart or the big one (10 minutes):')
    for every in (20, 30, 40, 45, 60, 90):
        row = '  '.join(f'{p} {c}/{n}' for p, (c, n) in ((p, boss_busters(p, every)) for p in ('P0', 'P1', 'P3')))
        print(f'  every {every:>3} s: {row}')


if __name__ == '__main__':
    self_test()
    report()

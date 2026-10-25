#!/usr/bin/env python3
"""Damage areas and moving enemies: when a placed area should go out (concept 04, "Schadensflächen erst, wenn die
Gegner stehen", A350).

Not a code scanner - a model calculator. The owner's rule (25.10.2026): "bei allen Klassen mit Schadensflächen: erst
dann, wenn Bewegung nach walltowall stoppt", and his correction the same day: "gegner [koennen sich] innerhalb der
schadensflaeche bewegen ..., ohne diese zu verlassen". An area stays where it is laid (Salted Earth, Doton: around the
player's feet, 5 y; Slipstream: around the target); what wastes it is enemies leaving it, not enemies moving in it.

Four rules for "lay it now", compared over constructed pulls:
  OLD    the player stands (RSR before A350)
  STILL  at least one enemy in the radius, none of them moved within the last GCD (A350, first version)
  STAYS  at least one enemy has been inside the radius throughout the last GCD, and none that was inside during that
         GCD is outside now (A350, after his correction): moving about inside counts as staying, newcomers do not
         hold it up, a pack passing through or moving on does
  ALL    every enemy inside now has been inside throughout the last GCD (stricter variant of STAYS)

Measured per rule: when it first lays the area after the pull starts, and the damage the area does - enemy ticks inside
it over its duration (a tick every 3 s, as a server tick). ASSUMPTIONS, not from a source: run speed 6 y/s, enemies
follow the tank to a slot 1-3 y from him and shift to a new slot every 1-2 s (the movement inside the area), GCD
2.5 s, area 5 y for 15 s (Salted Earth's job guide figures).

The self-test checks the geometry and the four rules on hand-built tracks: an enemy pacing inside the circle stays, one
walking through it leaves, one arriving does not block STAYS, a pack that moves on blocks it.

Usage: python3 .github/scripts/audit/damage_area_model.py
"""
import math
import random

DT = 0.05
GCD = 2.5
RADIUS = 5.0
DURATION = 15.0
TICK = 3.0
SPEED = 6.0


def dist(a, b):
    return math.hypot(a[0] - b[0], a[1] - b[1])


def inside(pos, centre, hitbox):
    return dist(pos, centre) - hitbox <= RADIUS


class Enemy:
    def __init__(self, pos, hitbox, rng, delay=0.0):
        self.pos = pos
        self.hitbox = hitbox
        self.rng = rng
        self.slot = (0.0, 0.0)
        self.next_shift = 0.0
        self.delay = delay      # seconds before it starts following (a late arrival)
        self.track = [(0.0, pos)]

    def step(self, t, tank):
        if t < self.delay:
            self.track.append((t, self.pos))
            return
        if t >= self.next_shift:
            a = self.rng.random() * 2 * math.pi
            r = self.rng.uniform(1.0, 3.0)
            self.slot = (math.cos(a) * r, math.sin(a) * r)
            self.next_shift = t + self.rng.uniform(1.0, 2.0)
        goal = (tank[0] + self.slot[0], tank[1] + self.slot[1])
        d = dist(self.pos, goal)
        if d > 1e-6:
            move = min(d, SPEED * DT)
            self.pos = (self.pos[0] + (goal[0] - self.pos[0]) / d * move, self.pos[1] + (goal[1] - self.pos[1]) / d * move)
        self.track.append((t, self.pos))


def window(track, t):
    """Positions held within [t - GCD, t]: the last one before the window start, then every later one."""
    start = t - GCD
    base = None
    later = []
    for at, pos in track:
        if at <= start:
            base = pos
        elif at <= t:
            later.append(pos)
    return base, later


def rule(name, enemies, centre, t, player_moving):
    if name == "OLD":
        return not player_moving
    now_in = [e for e in enemies if inside(e.track[-1][1], centre, e.hitbox)]
    if name == "STILL":
        if not now_in:
            return False
        for e in now_in:
            base, later = window(e.track, t)
            if base is None or any(p != base for p in later):
                return False
        return True
    stayed = False
    for e in enemies:
        base, later = window(e.track, t)
        positions = ([base] if base is not None else []) + later
        was_in = any(inside(p, centre, e.hitbox) for p in positions)
        now = inside(e.track[-1][1], centre, e.hitbox)
        if was_in and not now:
            return False
        full = base is not None and all(inside(p, centre, e.hitbox) for p in positions)
        if name == "ALL" and now and not full:
            return False
        stayed |= full
    return stayed


def pull(tank_path, n=5, seed=3, delays=None):
    """tank_path: list of (time, (x, y)) waypoints, linear in between. Returns per rule (lay time, enemy ticks)."""
    rng = random.Random(seed)
    enemies = [Enemy((rng.uniform(-4, 4), rng.uniform(-4, 4)), rng.uniform(0.5, 1.5), rng,
                     (delays[i] if delays else 0.0)) for i in range(n)]
    end = tank_path[-1][0] + DURATION + GCD * 2

    def tank_at(t):
        for (t0, p0), (t1, p1) in zip(tank_path, tank_path[1:]):
            if t0 <= t <= t1:
                f = 0 if t1 == t0 else (t - t0) / (t1 - t0)
                return (p0[0] + (p1[0] - p0[0]) * f, p0[1] + (p1[1] - p0[1]) * f)
        return tank_path[-1][1]

    results = {}
    history = []
    t = 0.0
    while t < end:
        tank = tank_at(t)
        prev = tank_at(max(0.0, t - DT))
        moving = dist(tank, prev) > 1e-9
        for e in enemies:
            e.step(t, tank)
        history.append((t, tank))
        for name in ("OLD", "STILL", "STAYS", "ALL"):
            if name not in results and t >= GCD and rule(name, enemies, tank, t, moving):
                results[name] = (t, tank)
        t += DT

    out = {}
    for name in ("OLD", "STILL", "STAYS", "ALL"):
        if name not in results:
            out[name] = (None, 0)
            continue
        laid, centre = results[name]
        ticks = 0
        k = 1
        while k * TICK <= DURATION:
            at = laid + k * TICK
            for e in enemies:
                pos = min(e.track, key=lambda s: abs(s[0] - at))[1]
                if inside(pos, centre, e.hitbox):
                    ticks += 1
            k += 1
        out[name] = (laid, ticks)
    return out


SCENARIOS = {
    "pause 1.5 s, then on to the wall": [(0, (0, 0)), (2, (12, 0)), (3.5, (12, 0)), (6.5, (30, 0)), (60, (30, 0))],
    "pause 4 s (longer than a GCD), then on": [(0, (0, 0)), (2, (12, 0)), (6, (12, 0)), (9, (30, 0)), (60, (30, 0))],
    "straight to the wall, pack shuffles there": [(0, (0, 0)), (3, (18, 0)), (60, (18, 0))],
}


def self_test():
    rng = random.Random(0)
    # Geometry: hitbox counts.
    assert inside((6.0, 0.0), (0.0, 0.0), 1.0) and not inside((6.2, 0.0), (0.0, 0.0), 1.0)
    centre = (0.0, 0.0)
    # An enemy pacing inside the circle for a GCD: STILL says no, STAYS says yes.
    pacer = Enemy((1.0, 0.0), 0.5, rng)
    pacer.track = [(t / 10, (math.cos(t / 3), math.sin(t / 3))) for t in range(0, 31)]
    t = 3.0
    assert not rule("STILL", [pacer], centre, t, False)
    assert rule("STAYS", [pacer], centre, t, False) and rule("ALL", [pacer], centre, t, False)
    # An enemy walking through the circle and out: STAYS says no.
    walker = Enemy((-8.0, 0.0), 0.5, rng)
    walker.track = [(t / 10, (-8.0 + t * 0.6, 0.0)) for t in range(0, 31)]
    assert not rule("STAYS", [pacer, walker], centre, t, False)
    # A newcomer arriving into a settled pack: STAYS lays, ALL waits.
    newcomer = Enemy((9.0, 0.0), 0.5, rng)
    newcomer.track = [(t / 10, (max(2.0, 9.0 - t * 0.6), 0.0)) for t in range(0, 31)]
    assert rule("STAYS", [pacer, newcomer], centre, t, False)
    assert not rule("ALL", [pacer, newcomer], centre, t, False)
    # Nobody there long enough: no rule but OLD lays it.
    fresh = Enemy((1.0, 1.0), 0.5, rng)
    fresh.track = [(2.0, (1.0, 1.0)), (3.0, (1.0, 1.0))]
    assert not rule("STAYS", [fresh], centre, t, False) and not rule("STILL", [fresh], centre, t, False)
    print("self-test ok: hitbox geometry, pacing inside, walking through, a newcomer and a fresh enemy each get their verdict")


if __name__ == "__main__":
    self_test()
    print("lay time after the pull starts (s) and enemy ticks the area deals; 5 enemies, area 5 y for 15 s")
    for title, path in SCENARIOS.items():
        print(f"  {title}:")
        for seed in (3, 11):
            res = pull(path, seed=seed)
            print("    seed %2d  " % seed + "  ".join(
                f"{n} {('-' if r[0] is None else f'{r[0]:4.1f}s')}/{r[1]:2d}" for n, r in res.items()))
    print("  late arrivals (two enemies start following 3 and 5 s in), straight to the wall:")
    res = pull([(0, (0, 0)), (3, (18, 0)), (60, (18, 0))], seed=5, delays=[0, 0, 0, 3, 5])
    print("    " + "  ".join(f"{n} {('-' if r[0] is None else f'{r[0]:4.1f}s')}/{r[1]:2d}" for n, r in res.items()))

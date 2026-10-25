#!/usr/bin/env python3
"""Damage areas and moving enemies: when a placed area should go out (concept 04, "Schadensflächen erst, wo die
Gegner bleiben", A350-A353).

Not a code scanner - a model calculator. The owner's rule (25.10.2026): "bei allen Klassen mit Schadensflaechen: erst
dann, wenn Bewegung nach walltowall stoppt", and his two hints the same day: enemies move about inside an area without
leaving it; and in a big pull not every enemy fits into the area, one pushing in pushes another out and takes its
place, and enemies die over time - "es kann nicht einfach nur ein zaehler der gegner innerhalb der schadensflaeche
sein". An area stays where it is laid (Salted Earth, Doton: around the player's feet, 5 y; Slipstream: around the
target); what wastes it is the pack moving on, not an enemy shifting inside it, a swap at its edge or a death. His
further hint: a tank stops on the way to pick up the next group with an area attack that draws enmity, and since his
rotation may be elsewhere just then, he waits until it has gone out before he moves on. That group stood idle where it
was - it has "stayed" by its track - and the pack he brought settles around him while he waits.

Rules for "lay it now", compared over constructed pulls:
  OLD    the player stands (RSR before A350)
  STILL  at least one enemy in the radius, none of them moved within the last GCD (A350, withdrawn: C126)
  STAYS  at least one enemy has been inside throughout the last GCD, and none that was inside during that GCD is
         outside now (A351, withdrawn: C127) - an enemy pushed out at the edge blocks it
  FLOW   at least one enemy has been inside throughout the last GCD, and no fewer enemies are inside now than of those
         still alive were inside at the start of that GCD (A352): a swap at the edge leaves the number, a death leaves
         both sides, a pack moving on takes it down
  DRIFT  like FLOW's first half, and the centre of those still alive that were inside at the start of the GCD is
         inside the area now (considered: always open while a big pull stays, slow to close on a pack moving on)
  MOST   like FLOW's first half, and more than half of those still alive that were inside at the start of the GCD are
         inside now (considered: as DRIFT)
  ALL    every enemy inside now has been inside throughout the last GCD (stricter variant)
  GATHER FLOW, and no enemy inside the area is out of combat or came into combat within the last GCD: an idle group
         in it is one the tank is about to pick up, and a group just picked up is one he moves on with
  GATHT  GATHER, and the same for the enemies within the tank's area attack reach (5 y around him): the group he
         stopped for may stand outside the player's area

Measured besides: the share of the time a pack stays at the wall in which a rule would lay the area (how soon a rotation
that misses one moment finds the next), and the share of the GCD after the tank drags the pack on in which it still
would (a waste, for a player who stands, as a Ninja or a Summoner may).

Measured per rule: when it first lays the area after the pull starts, and the damage the area does - living enemy ticks
inside it over its duration (a tick every 3 s, as a server tick). ASSUMPTIONS, not from a source: run speed 6 y/s;
enemies follow the tank to a slot around him and take a new slot every 1-2 s - 1-3 y in a small pack, 1-7 y in a big
pull, where many do not fit into 5 y and cross its edge both ways; GCD 2.5 s; area 5 y for 15 s (Salted Earth's job
guide figures). At an aggro stop the tank waits 0.8-3.2 s for his area attack (what is left of his GCD and a weave),
then 0.3 s more before he runs on; a melee player follows him 0.6 s behind and 2 y back.

The self-test checks the geometry and the rules on hand-built tracks: an enemy pacing inside the circle stays, one
walking through it leaves, a newcomer does not block STAYS or FLOW, a swap at the edge blocks STAYS but not FLOW, a
death blocks neither, a pack that moves on blocks both; an idle enemy in the area and a fresh pickup block GATHER but
not FLOW, an idle enemy by the tank only GATHT.

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
TANK_REACH = 5.0
RULES = ("OLD", "STILL", "STAYS", "FLOW", "DRIFT", "MOST", "ALL", "GATHER", "GATHT")


def dist(a, b):
    return math.hypot(a[0] - b[0], a[1] - b[1])


def inside(pos, centre, hitbox):
    return dist(pos, centre) - hitbox <= RADIUS


class Enemy:
    def __init__(self, pos, hitbox, rng, delay=0.0, slots=(1.0, 3.0), dies=None, follows=True, joins=0.0):
        self.pos = pos
        self.hitbox = hitbox
        self.rng = rng
        self.slot = (0.0, 0.0)
        self.next_shift = 0.0
        self.delay = delay      # seconds before it starts following (a late arrival)
        self.slots = slots
        self.dies = dies        # time it dies, None for never
        self.follows = follows  # False: it stands where it is (a caster that does not follow the tank)
        self.joins = joins      # time it enters combat; before that it stands idle where it is
        self.track = [(0.0, pos)]

    def alive(self, t):
        return self.dies is None or t < self.dies

    def engaged(self, t):
        return t >= self.joins

    def step(self, t, tank):
        if t < self.delay or t < self.joins or not self.follows:
            self.track.append((t, self.pos))
            return
        if t >= self.next_shift:
            a = self.rng.random() * 2 * math.pi
            r = self.rng.uniform(*self.slots)
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


def rule(name, enemies, centre, t, player_moving, tank=None):
    """Whether rule `name` lays the area now; `enemies` are the living ones, as the hostile list holds them; `tank` is
    where the tank stands."""
    if name == "OLD":
        return not player_moving
    now_in = [e for e in enemies if inside(e.track[-1][1], centre, e.hitbox)]
    if name in ("GATHER", "GATHT"):
        def at_tank(e):
            return name == "GATHT" and tank is not None and dist(e.track[-1][1], tank) - e.hitbox <= TANK_REACH
        for e in enemies:
            here = inside(e.track[-1][1], centre, e.hitbox) or at_tank(e)
            if here and (not e.engaged(t) or t - GCD < e.joins <= t):
                return False
        name = "FLOW"
    if name == "STILL":
        if not now_in:
            return False
        for e in now_in:
            base, later = window(e.track, t)
            if base is None or any(p != base for p in later):
                return False
        return True
    stayed = False
    at_start = 0
    starters = []
    for e in enemies:
        base, later = window(e.track, t)
        positions = ([base] if base is not None else []) + later
        was_in = any(inside(p, centre, e.hitbox) for p in positions)
        now = inside(e.track[-1][1], centre, e.hitbox)
        full = base is not None and all(inside(p, centre, e.hitbox) for p in positions)
        if name == "STAYS" and was_in and not now:
            return False
        if name == "ALL" and now and not full:
            return False
        if base is not None and inside(base, centre, e.hitbox):
            at_start += 1
            starters.append(e.track[-1][1])
        stayed |= full
    if name == "FLOW":
        return stayed and len(now_in) >= at_start
    if name == "MOST":
        kept = sum(1 for p in starters if any(inside(p, centre, e.hitbox) for e in enemies if e.track[-1][1] == p))
        return stayed and (not starters or kept * 2 > len(starters))
    if name == "DRIFT":
        if not stayed or not starters:
            return stayed
        cx = sum(p[0] for p in starters) / len(starters)
        cy = sum(p[1] for p in starters) / len(starters)
        return dist((cx, cy), centre) <= RADIUS
    return stayed


def pull(tank_path, n=5, seed=3, delays=None, slots=(1.0, 3.0), deaths=None, player_path=None, extra=None, settle=None,
         leave=None):
    """tank_path and player_path: lists of (time, (x, y)) waypoints, linear in between; the player is the tank unless
    player_path is given. Returns per rule (lay time, living enemy ticks inside the area, share of the settled phase -
    from `settle` on, for the area's duration - in which the rule would lay it: how often a rotation that misses one
    moment finds the next). With `leave`, the share is taken over the GCD after the tank moves on instead: how often the
    rule would still lay it on a pack that is leaving."""
    rng = random.Random(seed)
    enemies = [Enemy((rng.uniform(-4, 4), rng.uniform(-4, 4)), rng.uniform(0.5, 1.5), rng,
                     (delays[i] if delays else 0.0), slots, (deaths[i] if deaths else None)) for i in range(n)]
    enemies += extra or []
    end = tank_path[-1][0] + DURATION + GCD * 2

    def at(path, t):
        for (t0, p0), (t1, p1) in zip(path, path[1:]):
            if t0 <= t <= t1:
                f = 0 if t1 == t0 else (t - t0) / (t1 - t0)
                return (p0[0] + (p1[0] - p0[0]) * f, p0[1] + (p1[1] - p0[1]) * f)
        return path[-1][1]

    path = player_path or tank_path
    results = {}
    open_steps = {name: 0 for name in RULES}
    phase_steps = 0
    t = 0.0
    while t < end:
        tank = at(tank_path, t)
        player = at(path, t)
        moving = dist(player, at(path, max(0.0, t - DT))) > 1e-9
        living = [e for e in enemies if e.alive(t)]
        for e in living:
            e.step(t, tank)
        settled_phase = ((settle is not None and settle <= t < settle + DURATION)
                         or (leave is not None and leave <= t < leave + GCD))
        phase_steps += settled_phase
        for name in RULES:
            # Every rotation that lays one asks the player to stand first (Salted Earth, Doton), or casts it (Slipstream).
            if (name not in results or settled_phase) and t >= GCD and not moving and rule(name, living, player, t,
                                                                                            moving, tank):
                results.setdefault(name, (t, player))
                open_steps[name] += settled_phase
        t += DT

    out = {}
    for name in RULES:
        share = open_steps[name] / phase_steps if phase_steps else None
        if name not in results:
            out[name] = (None, 0, share)
            continue
        laid, centre = results[name]
        ticks = 0
        k = 1
        while k * TICK <= DURATION:
            when = laid + k * TICK
            for e in enemies:
                if e.alive(when):
                    pos = min(e.track, key=lambda s: abs(s[0] - when))[1]
                    if inside(pos, centre, e.hitbox):
                        ticks += 1
            k += 1
        out[name] = (laid, ticks, share)
    return out


def show(res):
    def one(n, r):
        text = f"{n} {('   -  ' if r[0] is None else f'{r[0]:4.1f}s')}/{r[1]:3d}"
        return text + ("" if r[2] is None else f"/{r[2]:4.0%}")
    return "  ".join(one(n, r) for n, r in res.items())


WALL = [(0, (0, 0)), (3, (18, 0)), (60, (18, 0))]
SCENARIOS = [
    ("pause 1.5 s, then on to the wall", dict(tank_path=[(0, (0, 0)), (2, (12, 0)), (3.5, (12, 0)), (6.5, (30, 0)), (60, (30, 0))])),
    ("pause 4 s (longer than a GCD), then on", dict(tank_path=[(0, (0, 0)), (2, (12, 0)), (6, (12, 0)), (9, (30, 0)), (60, (30, 0))])),
    ("straight to the wall, pack shuffles there", dict(tank_path=WALL, settle=3 + GCD)),
    ("late arrivals (two start following 3 and 5 s in)", dict(tank_path=WALL, delays=[0, 0, 0, 3, 5], settle=5 + GCD)),
    ("big pull, 16 enemies on 1-7 y, swaps at the edge", dict(tank_path=WALL, n=16, slots=(1.0, 7.0), settle=3 + GCD)),
    ("big pull, one dying every 1.5 s from 5 s on", dict(tank_path=WALL, n=16, slots=(1.0, 7.0),
                                                         deaths=[5 + 1.5 * i for i in range(16)], settle=3 + GCD)),
    ("big pull, then on again after 6 s at the wall", dict(tank_path=[(0, (0, 0)), (3, (18, 0)), (9, (18, 0)), (12, (36, 0)),
                                                                      (60, (36, 0))], n=16, slots=(1.0, 7.0))),
]


def aggro_stop(wait, seed, melee, stays=False, far=False):
    """The tank runs with a pack of 5 to an idle group of 4, waits `wait` seconds for his area attack to go out, and -
    unless he `stays` there - runs on to the wall 0.3 s after it lands. `melee`: the player follows him 0.6 s behind and
    2 y back (a Ninja); otherwise the player is the tank (a Dark Knight). `far`: the group stands 5 y beyond the tank,
    within his reach but out of the area of a melee 3 y behind him."""
    lands = 2 + wait
    tank_path = ([(0, (0, 0)), (2, (12, 0)), (60, (12, 0))] if stays else
                 [(0, (0, 0)), (2, (12, 0)), (lands + 0.3, (12, 0)), (lands + 3.3, (30, 0)), (60, (30, 0))])
    rng = random.Random(seed + 100)
    gx, spread, back = (17, (1.0, 2.0), 3) if far else (15, (1.5, 2.5), 2)
    group = [Enemy((gx + rng.uniform(-spread[0], spread[0]), rng.uniform(-spread[1], spread[1])), rng.uniform(0.5, 1.5),
                   rng, joins=lands) for _ in range(4)]
    player_path = None
    if melee:
        player_path = [(0, (-back, 0))] + [(t + 0.6, (x - back, y)) for t, (x, y) in tank_path]
    return pull(tank_path, seed=seed, extra=group, player_path=player_path)


def self_test():
    rng = random.Random(0)
    # Geometry: hitbox counts.
    assert inside((6.0, 0.0), (0.0, 0.0), 1.0) and not inside((6.2, 0.0), (0.0, 0.0), 1.0)
    centre = (0.0, 0.0)
    t = 3.0

    def track(points):
        e = Enemy(points[0][1], 0.5, rng)
        e.track = points
        return e

    pacer = track([(k / 10, (math.cos(k / 3), math.sin(k / 3))) for k in range(0, 31)])
    # Pacing inside: STILL says no, STAYS, FLOW and ALL say yes.
    assert not rule("STILL", [pacer], centre, t, False)
    assert all(rule(n, [pacer], centre, t, False) for n in ("STAYS", "FLOW", "ALL"))
    # Walking through and out: STAYS and FLOW say no.
    walker = track([(k / 10, (-8.0 + k * 0.6, 0.0)) for k in range(0, 31)])
    assert not rule("STAYS", [pacer, walker], centre, t, False)
    # FLOW: the walker was inside at the start of the GCD and is outside now, and nobody took its place - it blocks.
    assert not rule("FLOW", [pacer, walker], centre, t, False)
    # A newcomer: STAYS and FLOW lay, ALL waits.
    newcomer = track([(k / 10, (max(2.0, 9.0 - k * 0.6), 0.0)) for k in range(0, 31)])
    assert rule("STAYS", [pacer, newcomer], centre, t, False) and rule("FLOW", [pacer, newcomer], centre, t, False)
    assert not rule("ALL", [pacer, newcomer], centre, t, False)
    # A swap at the edge: one inside at the start pushed out, the newcomer in its place - STAYS blocks, FLOW lays.
    pushed = track([(k / 10, (4.0 + k * 0.1, 0.0)) for k in range(0, 31)])
    assert not rule("STAYS", [pacer, pushed, newcomer], centre, t, False)
    assert rule("FLOW", [pacer, pushed, newcomer], centre, t, False)
    # A death: the dead one is not in the list; nothing blocks.
    assert rule("FLOW", [pacer], centre, t, False) and rule("STAYS", [pacer], centre, t, False)
    # The pack moving on: two that were inside are outside now, nobody came in - both block.
    leaving = [track([(k / 10, (1.0 + k * 0.3, 0.5 * j)) for k in range(0, 31)]) for j in range(2)]
    assert not rule("STAYS", [pacer] + leaving, centre, t, False)
    assert not rule("FLOW", [pacer] + leaving, centre, t, False)
    # Nobody there long enough: no rule but OLD lays it.
    fresh = track([(2.0, (1.0, 1.0)), (3.0, (1.0, 1.0))])
    assert not any(rule(n, [fresh], centre, t, False) for n in ("STILL", "STAYS", "FLOW", "ALL"))
    # An idle enemy in the area has stayed by its track: FLOW lays, GATHER waits for the tank to pick it up.
    idle = track([(0.0, (2.0, 0.0)), (3.0, (2.0, 0.0))])
    idle.joins = 99.0
    assert rule("FLOW", [idle], centre, t, False) and not rule("GATHER", [idle], centre, t, False)
    # Picked up within the GCD: GATHER waits a GCD from the pickup, then lays as FLOW does.
    idle.joins = 2.0
    assert not rule("GATHER", [idle], centre, t, False) and rule("GATHER", [idle], centre, t + GCD, False)
    # An idle group by the tank, outside the player's area: only GATHT waits.
    by_tank = track([(0.0, (9.0, 0.0)), (3.0, (9.0, 0.0))])
    by_tank.joins = 99.0
    assert rule("GATHER", [pacer, by_tank], centre, t, False, tank=(8.0, 0.0))
    assert not rule("GATHT", [pacer, by_tank], centre, t, False, tank=(8.0, 0.0))
    print("self-test ok: hitbox geometry, pacing inside, walking through, a newcomer, a swap at the edge, a death,"
          " a pack moving on, a fresh enemy, an idle enemy, a fresh pickup and an idle group by the tank each get"
          " their verdict")


if __name__ == "__main__":
    self_test()
    print("lay time after the pull starts (s) / living enemy ticks the area deals / share of the settled phase in which")
    print("the rule would lay it (where the pack stays at the wall); area 5 y for 15 s")
    for title, kwargs in SCENARIOS:
        print(f"  {title}:")
        for seed in (3, 11):
            print("    seed %2d  " % seed + show(pull(seed=seed, **kwargs)))
    print("  the player stands at the wall; the tank holds the big pull there 6 s, then drags it on - share of the GCD")
    print("  after he moves on in which the rule would still lay the area (a waste):")
    for seed in (3, 11):
        print("    seed %2d  " % seed + show(pull([(0, (0, 0)), (3, (18, 0)), (9, (18, 0)), (12, (36, 0)), (60, (36, 0))],
                                             seed=seed, n=16, slots=(1.0, 7.0), player_path=[(0, (18, 0)), (60, (18, 0))],
                                             leave=9)))
    stationary = Enemy((22.0, 2.0), 1.0, random.Random(1), follows=False)
    print("  the player stands by a caster that does not follow; the tank drags 5 enemies past and on:")
    print("    " + show(pull([(0, (0, 0)), (8, (48, 0)), (60, (48, 0))], player_path=[(0, (22, 0)), (60, (22, 0))],
                             extra=[stationary])))
    print("  aggro stop: the tank brings 5, stops by an idle group of 4 at 2 s, waits for his area attack, runs on to the")
    print("  wall 0.3 s after it lands (there from about 3.3 s later; a lay before that is a waste):")
    for who, melee in (("player is the tank", False), ("player is a melee behind him", True)):
        for wait in (0.8, 1.6, 2.4, 3.2):
            for seed in (3, 11):
                print(f"    {who}, wait {wait:.1f} s, seed {seed:2d}  " + show(aggro_stop(wait, seed, melee)))
    print("  the same, the idle group 5 y beyond the tank - in his reach, out of the area of a melee 3 y behind him:")
    for wait in (1.6, 2.4, 3.2):
        for seed in (3, 11):
            print(f"    wait {wait:.1f} s, seed {seed:2d}  " + show(aggro_stop(wait, seed, True, far=True)))
    print("  the same, but the tank stays where he picked them up (the cost of waiting):")
    for who, melee in (("player is the tank", False), ("player is a melee behind him", True)):
        for seed in (3, 11):
            print(f"    {who}, wait 1.6 s, seed {seed:2d}  " + show(aggro_stop(1.6, seed, melee, stays=True)))

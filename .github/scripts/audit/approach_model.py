#!/usr/bin/env python3
"""Which enemy runs to another enemy, and when it arrives (concept 21, K3 "Gegner, die zu einem Verbündeten laufen";
A660, A724).

Not a code scanner - a model calculator. The Ogre of board 1 stands untargetable while its wisps march from four
corners to it and blow up there; with the familiar's Cover up the blast lands on the familiar, so the beastmaster
uses Snarl before the first one arrives. The model replays four scenes of the owner's recording of 11.11.2026
through the recognition as it was and as it is, and a constructed march for the timing:
  OLD  (A660) the goal is the targetable enemy with the most HP; an enemy runs to it once it has closed in by more
       than its own size; where one of its kind left the list without being seen dead, its kind arrives; the closing
       rate is measured since it was first seen. Snarl is asked for when the first arrival is within a GCD.
  NEW  (A724) the goal is the enemy with the most HP that could be fought in this combat, and stays it while it lives,
       targetable or not; an enemy runs to it when it has a target of its enmity, has looked at nobody since it was
       first seen, has closed in by more than its own size and has been watched for longer than a GCD; its kind
       arrives where one running first acted, and that action tells whether the arrival reaches one of yours; the
       closing rate is that of the last one to two GCDs. Snarl is asked for when one runs whose kind is not known to
       arrive without reaching one of yours, and its arrival distance is not learned yet or its arrival is within a
       GCD.

The scenes are the enemies' paths (linear between the recorded positions), the moment each took a target of its
enmity (Character.TargetId; OverlayPlugin's NPCTargetID) and a gaze target (Character.LookAt; PCTargetID), when it
could be targeted, when it fell or left, and its actions with whether they reached the player's side - read from
his ACT log of 11.11.2026 (OverlayPlugin 43afdd5, line 261), without any name:
  ogre     three small and one great wisp, a second wave 16 s later; one wisp arrives and blows up, the others fall
  mites    fight 4: three mites come 20 s in, look at the player at once and run at him; the fight ends 6 s later
           and they leave with full HP. Played twice, as two runs of one visit.
  ahriman  fight 8: four casters at the corners, untargetable at first, then they look at the player and close in
  zombie   fight 10: one zombie turns targetable, walks at the familiar next to the main enemy and looks at it
           0.7 s after its first step

ASSUMPTION, named: the plugin sees every enemy every 0.1 s; the log gives a position every 1 to 7 s.

    approach_model.py            the figures
    approach_model.py --test     self-test: three constructed defects of NEW must each fail a check
"""
import math
import sys

GCD = 2.5
STEP = 0.1

# Per enemy: kind (BNpcName row), max HP, hitbox radius, path [(s, x, y)], gaze target taken at (s), target of its
# enmity taken at (s), targetability changes [(s, untargetable)], fell at (s), left the object table at (s), actions
# [(s, reached the player's side)]. Seconds from the start of the scene.
SCENES = {'ogre': {'main': (14538, 12074, 2.08,
                   [(0.0, 136.0, -420.0), (3.46, 130.43, -420.0), (3.7, 120.2, -420.0), (4.8, 119.98, -420.0),
                    (45.21, 120.47, -420.77)],
                   0.0, 0.0, [(8.67, True), (43.43, False)], None, None, []),
          'add1': (14539, 171, 0.8,
                   [(8.46, 134.85, -434.85), (15.81, 131.29, -431.31), (20.76, 127.74, -427.77),
                    (25.58, 124.24, -424.26)],
                   None, 8.46, [], 25.58, 30.27, []),
          'add2': (14539, 171, 0.8,
                   [(8.46, 134.85, -405.15), (15.81, 131.29, -408.7), (20.76, 127.74, -412.27),
                    (25.71, 124.19, -415.8), (28.92, 122.45, -417.53)],
                   None, 8.46, [(28.31, True)], 28.85, 33.9, [(28.35, True)]),
          'add3': (14539, 171, 0.8, [(8.46, 105.15, -405.15), (15.81, 108.68, -408.71), (16.93, 109.48, -409.5)],
                   None, 8.46, [], 16.93, 21.53, []),
          'add4': (14748, 1575, 1.6,
                   [(8.58, 105.15, -434.85), (15.81, 108.7, -431.29), (20.76, 112.24, -427.77),
                    (22.61, 113.55, -426.45), (23.51, 114.15, -425.85)],
                   None, 8.58, [], 23.51, 27.81, []),
          'add5': (14539, 171, 0.8,
                   [(24.71, 134.85, -434.85), (31.68, 131.29, -431.32), (36.73, 127.75, -427.77),
                    (41.75, 124.21, -424.24), (43.0, 123.31, -423.35), (43.76, 123.03, -423.06)],
                   None, 24.71, [], 43.76, 47.95, []),
          'add6': (14539, 171, 0.8, [(24.71, 134.85, -405.15), (31.68, 131.29, -408.7), (34.14, 129.59, -410.41)],
                   None, 24.71, [], 34.14, 38.79, []),
          'add7': (14539, 171, 0.8,
                   [(24.71, 105.15, -434.85), (31.68, 108.7, -431.3), (36.73, 112.26, -427.75),
                    (40.55, 114.9, -425.1)],
                   None, 24.71, [], 40.55, 45.5, []),
          'add8': (14748, 1575, 1.6,
                   [(24.71, 105.15, -405.15), (31.68, 108.68, -408.7), (36.51, 112.11, -412.14),
                    (37.76, 112.92, -412.94)],
                   None, 24.71, [], 37.76, 42.33, [])},
 'mites': {'main': (14536, 13634, 3.0, [(0.0, 119.98, -430.01)], 0.0, 0.0, [], 15.5, None, []),
           'add1': (14537, 522, 1.8, [(6.95, 130.61, -409.39), (12.57, 127.91, -413.95), (13.25, 125.27, -418.39)],
                    7.07, 6.95, [], None, 13.46, []),
           'add2': (14537, 522, 1.8, [(6.95, 120.0, -405.0), (12.57, 119.72, -410.29), (13.25, 119.45, -415.44)],
                    7.07, 6.95, [], None, 13.46, []),
           'add3': (14537, 522, 1.8, [(6.95, 109.39, -409.39), (12.57, 111.61, -414.21), (13.25, 113.81, -418.86)],
                    7.07, 6.95, [], None, 13.46, [])},
 'ahriman': {'main': (14546, 21325, 2.24, [(0.0, 519.77, -0.99), (31.25, 515.65, -3.94), (32.26, 512.93, -5.87)],
                      0.0, 0.0, [], None, None, []),
             'add1': (14548, 512, 0.9,
                      [(5.76, 538.0, 13.0), (25.45, 533.9, 9.96), (26.0, 529.87, 6.97), (26.62, 525.67, 3.85),
                       (27.22, 521.51, 0.74), (27.75, 517.46, -2.26), (28.32, 513.43, -5.3), (28.83, 509.41, -8.32),
                       (29.5, 505.37, -11.4)],
                      24.24, 24.24, [(20.82, False)], 31.81, 34.3, [(18.77, False), (23.8, True), (26.82, True)]),
             'add2': (14548, 512, 0.9,
                      [(5.76, 502.0, 13.0), (24.24, 501.98, 12.99), (25.45, 504.72, 8.77), (26.0, 506.87, 4.22),
                       (26.62, 507.77, -0.72), (27.22, 506.98, -5.79), (27.87, 505.11, -10.45),
                       (29.2, 504.78, -11.25)],
                      24.24, 24.24, [(20.82, False)], 31.63, 33.84, [(18.77, False), (23.8, True), (26.82, True)]),
             'add3': (14548, 512, 0.9, [(5.76, 502.0, -13.0), (25.55, 506.22, -10.12), (26.98, 507.56, -9.2)],
                      24.24, 24.24, [(20.82, False)], 29.36, 31.45, [(14.77, False), (23.8, True), (26.82, True)]),
             'add4': (14548, 512, 0.9,
                      [(5.76, 538.0, -13.0), (24.24, 537.99, -13.02), (25.55, 533.14, -11.42),
                       (26.12, 528.26, -10.21), (26.62, 523.25, -9.73), (27.22, 518.07, -9.95),
                       (27.75, 512.96, -10.69), (28.43, 507.99, -11.96), (29.4, 505.61, -12.65)],
                      24.24, 24.24, [(20.82, False)], 31.76, 33.84,
                      [(14.77, False), (23.8, True), (26.82, True)])},
 'zombie': {'add0': (14553, 1344, 0.75,
                     [(0.0, 102.5, 3.5), (7.8, 104.0, 2.32), (8.46, 108.1, -0.85), (9.0, 112.19, -4.01),
                      (9.54, 116.41, -7.26), (9.86, 117.98, -8.48)],
                     8.46, 7.8, [(7.61, False)], None, 11.41, [(10.68, True)]),
            'main': (14552, 16010, 2.34, [(0.0, 119.98, -10.03)], 0.0, 0.0, [], None, None, [])}}


class Enemy:
    def __init__(self, label, row):
        (self.kind, self.max_hp, self.radius, self.path, self.gaze, self.engaged, self.changes, self.dead,
         self.removed, self.acts) = row
        self.label = label
        # A first change to targetable means it began untargetable.
        self.starts_targetable = not self.changes or self.changes[0][1]

    def present(self, t):
        return t >= self.path[0][0] and (self.removed is None or t < self.removed)

    def alive(self, t):
        return self.dead is None or t < self.dead

    def targetable(self, t):
        state = self.starts_targetable
        for at, untargetable in self.changes:
            if t >= at:
                state = not untargetable
        return state

    def position(self, t):
        path = self.path
        if t <= path[0][0]:
            return path[0][1:]
        for (t0, x0, y0), (t1, x1, y1) in zip(path, path[1:]):
            if t <= t1:
                f = (t - t0) / (t1 - t0) if t1 > t0 else 1.0
                return (x0 + (x1 - x0) * f, y0 + (y1 - y0) * f)
        return path[-1][1:]

    def looks_at_someone(self, t):
        return self.gaze is not None and t >= self.gaze

    def has_enmity_target(self, t):
        return self.engaged is not None and t >= self.engaged


def distance(a, b, t):
    (ax, ay), (bx, by) = a.position(t), b.position(t)
    return math.hypot(ax - bx, ay - by)


class Track:
    def __init__(self, enemy, goal, t):
        self.enemy = enemy
        self.since = t
        self.start = self.now = distance(enemy, goal, t)
        self.at = t
        self.reach = goal.radius + enemy.radius
        self.mark = (t, self.now)
        self.before = None
        self.looked = False
        self.engaged = False
        self.acted = False


class Old:
    """A660, as the code stood until A724."""

    def __init__(self):
        self.tracks = {}
        self.arrival = {}
        self.goal = None

    def end_of_combat(self):
        self.tracks = {}

    def update(self, enemies, t):
        goal = None
        for e in enemies:
            if e.present(t) and e.alive(t) and e.targetable(t) and e.max_hp > (goal.max_hp if goal else 0):
                goal = e
        self.goal = goal
        if goal is None:
            self.tracks = {}
            return
        seen = set()
        for e in enemies:
            if not (e.present(t) and e.targetable(t)) or e is goal or not e.alive(t):
                continue
            seen.add(e.label)
            if e.label not in self.tracks:
                self.tracks[e.label] = Track(e, goal, t)
                continue
            track = self.tracks[e.label]
            track.at, track.now = t, distance(e, goal, t)
        for label in [label for label in self.tracks if label not in seen]:
            track = self.tracks.pop(label)
            seen_dead = track.enemy.present(t) and not track.enemy.alive(t)
            if self.running(track) and not seen_dead and track.now < self.arrival.get(track.enemy.kind, math.inf):
                self.arrival[track.enemy.kind] = track.now

    def observe(self, enemy, reached_yours, t):
        pass

    @staticmethod
    def running(track):
        return track.start - track.now > track.enemy.radius

    def eta(self, track):
        seconds = track.at - track.since
        if not self.running(track) or seconds <= 0:
            return None
        speed = (track.start - track.now) / seconds
        return max(0.0, track.now - self.arrival.get(track.enemy.kind, track.reach)) / speed

    def runners(self):
        return [track.enemy.label for track in self.tracks.values() if self.running(track)]

    def asks_snarl(self):
        etas = [eta for eta in map(self.eta, self.tracks.values()) if eta is not None]
        return bool(etas) and min(etas) <= GCD


class New:
    """A724. The three switches are the constructed defects of the self-test."""

    def __init__(self, gaze=True, latch=True, patience=True):
        self.use_gaze, self.use_latch, self.use_patience = gaze, latch, patience
        self.tracks = {}
        self.arrival = {}
        self.reaches = set()
        self.spares = set()
        self.goal = None

    def end_of_combat(self):
        self.tracks = {}
        self.goal = None

    def update(self, enemies, t):
        hostiles = [e for e in enemies if e.present(t) and e.alive(t) and e.targetable(t)]
        goal = None
        for e in hostiles:
            if e.max_hp > (goal.max_hp if goal else 0):
                goal = e
        held = self.goal
        if (self.use_latch and held is not None and held.present(t) and held.alive(t)
                and (held in hostiles or not held.targetable(t)) and (goal is None or goal.max_hp <= held.max_hp)):
            goal = held
        if goal is not self.goal:
            self.tracks = {}
            self.goal = goal
        if goal is None:
            return
        seen = set()
        for e in hostiles:
            if e is goal:
                continue
            seen.add(e.label)
            if e.label not in self.tracks:
                self.tracks[e.label] = Track(e, goal, t)
            self.follow(self.tracks[e.label], goal, t)
        for label in [label for label in self.tracks if label not in seen]:
            e = self.tracks[label].enemy
            # One that can no longer be targeted but stands and lives is still on its way: a wisp turns untargetable
            # a moment before it blows up.
            if e.present(t) and e.alive(t) and not e.targetable(t):
                self.follow(self.tracks[label], goal, t)
            else:
                del self.tracks[label]

    def follow(self, track, goal, t):
        e = track.enemy
        track.at, track.now = t, distance(e, goal, t)
        track.engaged = e.has_enmity_target(t)
        track.looked = track.looked or e.looks_at_someone(t)
        if t - track.mark[0] >= GCD:
            track.before, track.mark = track.mark, (t, track.now)

    def observe(self, enemy, reached_yours, t):
        track = self.tracks.get(enemy.label)
        if track is None or not self.running(track):
            return
        track.acted = True
        kind = enemy.kind
        if track.now > self.arrival.get(kind, -math.inf):
            self.arrival[kind] = track.now
        if reached_yours:
            self.reaches.add(kind)
            self.spares.discard(kind)
        elif kind not in self.reaches:
            self.spares.add(kind)

    def running(self, track):
        return (not track.acted and track.engaged and not (self.use_gaze and track.looked)
                and track.start - track.now > track.enemy.radius
                and (not self.use_patience or track.at - track.since > GCD))

    def eta(self, track):
        """Seconds to the arrival; None while it runs to none; math.inf while its kind's arrival is not learned."""
        if not self.running(track):
            return None
        kind = track.enemy.kind
        if kind not in self.arrival:
            return math.inf
        since, start = track.before if track.before else (track.since, track.start)
        seconds = track.at - since
        speed = (start - track.now) / seconds if seconds > 0 else 0.0
        return max(0.0, track.now - self.arrival[kind]) / speed if speed > 0 else None

    def runners(self):
        return [track.enemy.label for track in self.tracks.values() if self.running(track)]

    def asks_snarl(self):
        for track in self.tracks.values():
            eta = self.eta(track)
            if eta is not None and track.enemy.kind not in self.spares and (eta == math.inf or eta <= GCD):
                return True
        return False


def play(rule, scene, result):
    """Runs one scene; notes when the rule first asks for Snarl, whom it took as running, and its goal."""
    enemies = [Enemy(label, row) for label, row in scene.items()]
    main = next(e for e in enemies if e.label == "main")
    end = max(max(p[0] for p in e.path) for e in enemies) + 1.0
    pending = sorted(((at, e, reached) for e in enemies for at, reached in e.acts), key=lambda act: act[0])
    t = 0.0
    while t <= end:
        rule.update(enemies, t)
        while pending and pending[0][0] <= t:
            _, e, reached = pending.pop(0)
            rule.observe(e, reached, t)
        if main.present(t) and main.alive(t) and rule.goal is not None and rule.goal is not main:
            result.setdefault("other goal", round(t, 1))
        result.setdefault("runners", set()).update(rule.runners())
        if rule.asks_snarl():
            result.setdefault("asks", round(t, 1))
        t = round(t + STEP, 2)
    rule.end_of_combat()
    return result


def march(stand, speed, start, arrive):
    """A constructed wisp: stands, then walks to the main enemy and acts where its kind arrives."""
    walk = (start - arrive) / speed
    return (1, 100, 0.8, [(0.0, start, 0.0), (stand, start, 0.0), (stand + walk, arrive, 0.0)], None, 0.0,
            [(stand + walk, True)], stand + walk + 0.5, stand + walk + 5.0, [(stand + walk, True)])


def lead(rule_type):
    """Seconds before the arrival at which the rule asks, once the kind's arrival is learned."""
    main = (2, 10000, 2.08, [(0.0, 0.0, 0.0)], 0.0, 0.0, [], None, None, [])
    rule = rule_type()
    # The measured march of 11.11.2026: about 2 s standing, 1 y per second, from 21 y to 3.84 y.
    wisp = march(2.2, 1.0, 21.0, 3.84)
    play(rule, {"main": main, "first": wisp}, {})
    second = play(rule, {"main": main, "second": wisp}, {})
    arrival = wisp[9][0][0]
    return round(arrival - second["asks"], 1) if "asks" in second else None


def figures(new_type=New):
    out = {}
    for name, rule_type in (("OLD", Old), ("NEW", new_type)):
        rule = rule_type()
        rows = {}
        for scene in ("ogre", "mites", "mites", "ahriman", "zombie"):
            key = scene if scene not in rows else scene + " again"
            rows[key] = play(rule, SCENES[scene], {})
        out[name] = (rows, dict(rule.arrival), lead(rule_type))
    return out


def checks(out):
    """The claims of the concept; each returns its failures."""
    failed = []
    old, old_arrival, old_lead = out["OLD"]
    new, new_arrival, new_lead = out["NEW"]
    wisps = {label for label in SCENES["ogre"] if label != "main"}
    first_blast = min(at for row in SCENES["ogre"].values() for at, reached in row[9] if reached)
    if "other goal" in new["ogre"]:
        failed.append("NEW loses the Ogre as the goal while it cannot be targeted")
    if new["ogre"]["runners"] != wisps:
        failed.append("NEW does not take exactly the wisps as running")
    if not ("asks" in new["ogre"] and new["ogre"]["asks"] <= first_blast - GCD):
        failed.append("NEW asks for Snarl later than a GCD before the first blast")
    for scene in ("mites", "mites again", "ahriman", "zombie"):
        if new[scene]["runners"] or "asks" in new[scene]:
            failed.append(f"NEW takes an attacker as running in {scene}")
    if not any(abs(d - 3.84) < 0.2 for d in new_arrival.values()):
        failed.append("NEW does not learn where the wisps arrive")
    if new_lead is None or abs(new_lead - GCD) > 2 * STEP:
        failed.append(f"NEW asks {new_lead} s before a learned arrival, not a GCD")
    return failed


def self_test():
    out = figures()
    failed = checks(out)
    assert not failed, failed
    old = out["OLD"][0]
    assert "other goal" in old["ogre"], "OLD should lose the Ogre as the goal"
    assert all("asks" in old[s] for s in ("mites again", "ahriman", "zombie")), "OLD should ask where his log shows it"
    for name, switches in (("no gaze", {"gaze": False}), ("no latch", {"latch": False}),
                           ("no patience", {"patience": False})):
        mutant = checks(figures(lambda: New(**switches)))
        assert mutant, f"the constructed defect '{name}' passes every check"
    print("self-test ok: OLD loses the goal and asks in fights 4, 8 and 10 as in his log; NEW passes; three "
          "constructed defects of NEW each fail a check")


def main():
    if "--test" in sys.argv:
        self_test()
        return 0
    self_test()
    out = figures()
    for name in ("OLD", "NEW"):
        rows, arrival, ahead = out[name]
        print(f"\n{name}")
        for scene, row in rows.items():
            print(f"  {scene:14} runs: {', '.join(sorted(row.get('runners', []))) or 'none':46} "
                  f"Snarl asked at {row.get('asks', '-'):>5} s   goal other than the main enemy from "
                  f"{row.get('other goal', '-')} s")
        print("  arrival learned per kind:", {k: round(v, 2) for k, v in arrival.items()})
        print(f"  constructed march, arrival learned: asks {ahead} s before the arrival (a GCD is {GCD} s)")
    return 0


if __name__ == "__main__":
    sys.exit(main())

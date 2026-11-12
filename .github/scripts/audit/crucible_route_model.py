#!/usr/bin/env python3
"""Where a Crucible run stands on its board (concept 21, K10 "Weg", "Wo du stehst"; A632, A715).

Not a code scanner - a model calculator. RSR reads no position on a board: it follows the run by what the game shows -
the horn screen names the battle of a fight space, a campsite, shop or treasure window opens on its space - against the
board's spaces and ways from the sheets (CrucibleRoute). This model walks every way of every board from the start to
the boss, with every outcome of every random space, feeds the rule those events and compares where the rule says the run
stands with where it stands:
  OLD  (until A715) a battle is the first space of any board whose own battle it is; a window is a space of its kind
       among the direct successors of the space the run stands on, unless that space is of the kind already.
  NEW  (A715) a battle is the first space on the ways on that holds it - its own, or a random space that can become
       it -, else the first on any board; a window is the first space on the ways on that is of its kind or, a random
       space, can become it, never past a fixed fight; standing on a space of that kind, only a window that opened anew
       moves the run, and only to the very next space; where several spaces fit - a fork whose ways both begin with
       that kind - the run stays on the fork until the next fight tells.
Each visit of a space gives two frames with its window, the first one marked as newly opened, as the plugin sees it.
Where the rule stands elsewhere than the run, it stands BEHIND (on an earlier space of the walk: every way ahead still
counts, the heavier side decides), behind with a FIGHT between (a fight already fought counts as ahead), or ASTRAY (on
a space the run never came to: what lies ahead of it is another way).

The boards below are the sheets XBMContentStageEvent, XBMContentStageEventMap, XBMContentBattle,
XBMContentRandomStageEvent and XBMRandomStageEvent (xivapi/ffxiv-datamining 52a72b3, 10.11.2026), one tuple per space:
its kind, its battle (XBMBattleDetail row), what a random space can become (kind, battle), and the spaces its ways lead
to. Kinds: 1 start, 2 enemy, 3 elite, 4 boss, 5 shop, 6 campsite, 7 treasure, 8 random.

ASSUMPTION, named: every window is seen (RSR runs the whole run). The model also plays each walk with one window
unseen, to show how each rule finds the run again.

    crucible_route_model.py            the figures
    crucible_route_model.py --test     self-test: OLD must lose the run where a random space becomes a fight
"""
import sys

START, ENEMY, ELITE, BOSS, SHOP, CAMP, TREASURE, RANDOM = range(1, 9)
FIGHTS = (ENEMY, ELITE, BOSS)

BOARDS = {
    1: [
        (1, 0, (), (1,)),
        (2, 1, (), (3, 2)),
        (2, 2, (), (4,)),
        (2, 3, (), (5,)),
        (6, 0, (), (6,)),
        (7, 0, (), (6,)),
        (2, 4, (), (7,)),
        (7, 0, (), (8,)),
        (3, 5, (), (10, 9)),
        (7, 0, (), (11,)),
        (6, 0, (), (11,)),
        (5, 0, (), (12,)),
        (4, 6, (), ()),
    ],
    2: [
        (1, 0, (), (1,)),
        (2, 7, (), (2,)),
        (5, 0, (), (4, 3)),
        (3, 8, (), (5,)),
        (2, 9, (), (6,)),
        (7, 0, (), (7,)),
        (6, 0, (), (7,)),
        (2, 10, (), (8,)),
        (7, 0, (), (9,)),
        (3, 11, (), (11, 10)),
        (8, 0, ((2, 12), (6, 0)), (12,)),
        (7, 0, (), (12,)),
        (5, 0, (), (13,)),
        (4, 13, (), ()),
    ],
    3: [
        (1, 0, (), (1,)),
        (2, 14, (), (2,)),
        (2, 15, (), (3,)),
        (5, 0, (), (5, 4)),
        (3, 16, (), (6,)),
        (2, 17, (), (7,)),
        (7, 0, (), (8,)),
        (6, 0, (), (8,)),
        (2, 18, (), (10, 9)),
        (7, 0, (), (11,)),
        (5, 0, (), (11,)),
        (3, 19, (), (12,)),
        (7, 0, (), (14, 13)),
        (8, 0, ((2, 20), (7, 0)), (15,)),
        (6, 0, (), (15,)),
        (5, 0, (), (16,)),
        (4, 21, (), ()),
    ],
    4: [
        (1, 0, (), (1,)),
        (2, 22, (), (3, 2)),
        (2, 23, (), (4,)),
        (8, 0, ((2, 24), (7, 0)), (4,)),
        (5, 0, (), (6, 5)),
        (6, 0, (), (7,)),
        (8, 0, ((2, 25), (7, 0)), (7,)),
        (3, 26, (), (9, 8)),
        (8, 0, ((2, 27), (7, 0), (6, 0)), (10,)),
        (7, 0, (), (11,)),
        (3, 28, (), (12,)),
        (2, 29, (), (13,)),
        (8, 0, ((2, 30), (5, 0)), (14,)),
        (5, 0, (), (14,)),
        (4, 31, (), ()),
    ],
    5: [
        (1, 0, (), (1,)),
        (2, 32, (), (3, 2)),
        (3, 33, (), (5, 4)),
        (5, 0, (), (6, 5)),
        (6, 0, (), (7,)),
        (7, 0, (), (8,)),
        (8, 0, ((2, 34), (7, 0)), (9,)),
        (8, 0, ((2, 35), (5, 0)), (10,)),
        (2, 36, (), (10,)),
        (7, 0, (), (10,)),
        (3, 37, (), (11,)),
        (6, 0, (), (13, 12)),
        (8, 0, ((3, 38), (7, 0)), (14,)),
        (5, 0, (), (15,)),
        (7, 0, (), (16,)),
        (7, 0, (), (17,)),
        (3, 39, (), (18,)),
        (2, 40, (), (18,)),
        (2, 41, (), (19,)),
        (5, 0, (), (22, 21, 20)),
        (8, 0, ((2, 42), (7, 0)), (23,)),
        (6, 0, (), (25,)),
        (7, 0, (), (24,)),
        (8, 0, ((3, 43), (5, 0)), (25,)),
        (3, 44, (), (25,)),
        (4, 45, (), ()),
    ],
}


def can_be(node, kind):
    return node[0] == kind or (node[0] == RANDOM and any(option[0] == kind for option in node[2]))


def holds(node, battle):
    return node[1] == battle or (node[0] == RANDOM and any(option[1] == battle for option in node[2]))


class Old:
    """CrucibleRoute until A715."""

    def __init__(self):
        self.board, self.at, self.last = 0, -1, (0, -1)

    def battle(self, battle):
        for board, nodes in BOARDS.items():
            for index, node in enumerate(nodes):
                if node[1] == battle and self.last != (board, index):
                    self.board, self.at, self.last = board, index, (board, index)
                    return

    def window(self, kind, opened):
        if self.board == 0 or self.at < 0:
            return
        nodes = BOARDS[self.board]
        if nodes[self.at][0] == kind:
            return
        for successor in nodes[self.at][3]:
            if can_be(nodes[successor], kind):
                self.at = successor
                return


class New:
    """CrucibleRoute since A715."""

    def __init__(self):
        self.board, self.at, self.last, self.stands_as = 0, -1, (0, -1), 0

    def ahead(self, wanted):
        nodes = BOARDS[self.board]
        found, seen, queue = [], set(), list(nodes[self.at][3])
        while queue:
            index = queue.pop(0)
            if index in seen:
                continue
            seen.add(index)
            if wanted(nodes[index]):
                found.append(index)
                continue
            if nodes[index][0] in FIGHTS:
                continue
            queue.extend(nodes[index][3])
        return found

    def battle(self, battle):
        if self.board != 0 and self.at >= 0:
            found = self.ahead(lambda node: holds(node, battle))
            if found:
                self.at, self.last, self.stands_as = found[0], (self.board, found[0]), ENEMY
                return
        for board, nodes in BOARDS.items():
            for index, node in enumerate(nodes):
                if holds(node, battle) and self.last != (board, index):
                    self.board, self.at, self.last, self.stands_as = board, index, (board, index), ENEMY
                    return

    def window(self, kind, opened):
        if self.board == 0 or self.at < 0:
            return
        nodes = BOARDS[self.board]
        if self.stands_as != kind:
            found = self.ahead(lambda node: can_be(node, kind))
        elif opened:
            found = [s for s in dict.fromkeys(nodes[self.at][3]) if can_be(nodes[s], kind)]
        else:
            return
        if len(found) > 1:
            self.stands_as = 0
        elif found:
            self.at, self.stands_as = found[0], kind


def walks(board):
    """Every way from the start to the boss, each random space with every outcome: lists of (space, kind, battle)."""
    nodes = BOARDS[board]

    def on(index):
        kind, battle, options, ways = nodes[index]
        outcomes = [(index, kind, battle)] if kind != RANDOM else [(index, k, b) for k, b in options]
        for outcome in outcomes:
            if kind == BOSS or not ways:
                yield [outcome]
            else:
                for way in ways:
                    for rest in on(way):
                        yield [outcome] + rest

    return list(on(0))


def play(rule, board, walk, unseen=-1):
    """How often the rule stood behind, behind with a fight between, and astray, after the first fight told it the board."""
    tracker = rule()
    behind = fight_between = astray = 0
    visited = [index for index, _, _ in walk]
    for step, (index, kind, battle) in enumerate(walk):
        if kind in FIGHTS:
            tracker.battle(battle)
        elif kind in (SHOP, CAMP, TREASURE) and step != unseen:
            tracker.window(kind, True)
            tracker.window(kind, False)
        else:
            continue
        if tracker.board == 0 or (tracker.board, tracker.at) == (board, index):
            continue
        if tracker.board == board and tracker.at in visited[:step]:
            stood = visited.index(tracker.at)
            if any(k in FIGHTS for _, k, _ in walk[stood + 1:step + 1]):
                fight_between += 1
            else:
                behind += 1
        else:
            astray += 1
    return behind, fight_between, astray


def figures(rule, unseen=False):
    """Per board: the cases played, and how often the rule stood behind, behind with a fight between, and astray."""
    result = {}
    for board in BOARDS:
        cases, totals = 0, [0, 0, 0]
        for walk in walks(board):
            steps = [step for step, (_, kind, _) in enumerate(walk) if kind in (SHOP, CAMP, TREASURE)] if unseen else [-1]
            for step in steps:
                cases += 1
                for i, n in enumerate(play(rule, board, walk, step)):
                    totals[i] += n
        result[board] = (cases, *totals)
    return result


def self_test():
    old, new = figures(Old), figures(New)
    assert old[1][1:] == (0, 0, 0), "board 1 has no random space and no two spaces of a kind in a row: OLD follows it"
    assert all(old[b][2] > 0 for b in (2, 3, 4, 5)), "OLD stays behind a fight on every board with a random space"
    assert all(new[b][2] == 0 and new[b][3] == 0 for b in BOARDS), f"NEW never stands behind a fight nor astray: {new}"
    assert all(new[b][1] == 0 for b in (1, 2, 3)), "NEW follows boards 1 to 3 exactly"
    # A constructed board: a treasure, then a random space that becomes a treasure or a fight, then the boss.
    BOARDS[99] = [(START, 0, (), (1,)), (ENEMY, 901, (), (2,)), (TREASURE, 0, (), (3,)),
                  (RANDOM, 0, ((ENEMY, 902), (TREASURE, 0)), (4,)), (BOSS, 903, (), ())]
    try:
        assert sum(figures(Old)[99][1:]) > 0 and sum(figures(New)[99][1:]) == 0, "two treasures in a row and a random fight"
    finally:
        del BOARDS[99]
    print("self-test ok: OLD stays behind a fight where a random space becomes one, NEW never does and never goes astray")


def main():
    if "--test" in sys.argv[1:]:
        self_test()
        return 0

    self_test()
    for unseen, title in ((False, "every window seen"), (True, "one window unseen, each in turn")):
        for name, rule in (("OLD (until A715)", Old), ("NEW (A715)", New)):
            print(f"\n{name}, {title}: every walk from the start to the boss, every outcome of every random space")
            for board, (cases, behind, fight, astray) in figures(rule, unseen).items():
                print(f"  board {board}: {cases:5d} walks, stood behind {behind:5d}, behind with a fight between {fight:5d}, "
                      f"astray {astray:5d}")
    return 0


if __name__ == "__main__":
    sys.exit(main())

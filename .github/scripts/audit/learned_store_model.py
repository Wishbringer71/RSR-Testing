#!/usr/bin/env python3
"""The learned tables' revision store, stage 1: does the design lose, double or misplace a reading?

Not a code scanner - a model of the design in concept 13, "Stufe 1: Speicher, Übernahme, Sicherung" (the owner's order
of 28.10.2026: "das neue revisionssystem muss sicher aufgebaut sein, dass nicht irrtümlich erkenntnisse gelöscht
werden, bzw. alte daten dauerhaft unwidersprochen und ungeprüft nach einem versionssprung des spiels beibehalten
werden").

One store of the form of the area table: a share of a member's maximum HP per action id, valid when finite and within
[0, 1]. Each entry has a running layer (Current), a taken-over one (Inherited) and a quarantine; each layer carries its
revision (game version, measuring method), its highest value, its count, its clean count and its lowest and highest
clean value. The readers see one table: per entry the higher of the two layers. Beside the new file the old one goes on
being written as before - the highest valid value per id, merged with what the file holds.

Random runs of sessions. A session reads the game version - known from the load, known only after some readings, or
never -, loads, takes readings (some invalid, some not clean) and saves after each, as the code does. Between sessions:
a patch (new game version), a new measuring method, a file turned unreadable (the new one, its backup too, the old one
as the new RSR left it - an older RSR writes no backup, and what it alone held is lost with its file, as before the
revision), both files deleted by hand, a session of an older RSR that knows only the old file, a load that did not
finish.

Checked after every load and every reading:
  READ     the table the readers see holds, per id, the highest valid reading ever stored - the old behaviour, so
           stage 1 changes nothing in a fight - and never an invalid one
  LOST     no reading the new RSR stored is missing from the counts (unless the new file and its backup both broke)
  DOUBLED  the counts hold no more than the readings stored plus one per value the old file brought in
  CURRENT  with the version known, the running layer carries the running revision and exactly the readings taken
           under it; a reading taken while the version was unreadable is in the taken-over layer
  DELETED  a store whose two files he deleted holds nothing from before

Usage: python3 .github/scripts/audit/learned_store_model.py [--self-test]
"""
import copy
import math
import random
import sys

RUNS = 400
SESSIONS = 14
READINGS = 6
IDS = (1, 2, 3)
GARBAGE = 'garbage'

# The defects the design is built against, each built in on purpose by the self-test.
DEFECTS = {
    'transition_drops': 'a version change drops the running layer instead of adding it to the taken-over one',
    'transition_max_counts': 'a version change keeps the higher count instead of adding both',
    'merge_sums_same': 'the save adds the counts of memory and file for the same revision',
    'legacy_every_load': 'the old file is taken over at every load, whether it adds anything or not',
    'backup_on_missing': 'the backup is loaded when the file is missing, not only when it is unreadable',
    'unknown_is_revision': 'an unreadable version counts as a revision of its own and moves the running layers',
    'quarantine_read': 'an invalid value is read like a valid one',
    'old_file_dropped': 'the old file is no longer written once the new one exists',
}


def valid(value):
    return math.isfinite(value) and 0.0 <= value <= 1.0


def same(first, second):
    return repr(first) == repr(second)


class Layer:
    def __init__(self, game, method):
        self.game = game
        self.method = method
        self.value = None
        self.count = 0
        self.clean = 0
        self.low = 0.0
        self.high = 0.0

    def add(self, value, clean):
        self.value = value if self.value is None else max(self.value, value)
        self.count += 1
        if clean:
            self.low = value if self.clean == 0 else min(self.low, value)
            self.high = value if self.clean == 0 else max(self.high, value)
            self.clean += 1


def spread(into, other):
    if other.clean == 0:
        return
    if into.clean == 0:
        into.low, into.high = other.low, other.high
    else:
        into.low, into.high = min(into.low, other.low), max(into.high, other.high)


def combine(into, other, defects, label_from_other=True):
    """Adds the readings of another history: counts summed."""
    if into is None:
        return copy.deepcopy(other)
    result = copy.deepcopy(into)
    result.value = other.value if result.value is None else max(result.value, other.value)
    if 'transition_max_counts' in defects:
        result.count, result.clean = max(result.count, other.count), max(result.clean, other.clean)
    else:
        result.count += other.count
        result.clean += other.clean
    spread(result, other)
    if label_from_other:
        result.game, result.method = other.game, other.method
    return result


def merge_same(first, second, defects):
    """The same history from two sides - memory and file: the higher of each count."""
    result = copy.deepcopy(first)
    result.value = max(first.value, second.value)
    if 'merge_sums_same' in defects:
        result.count, result.clean = first.count + second.count, first.clean + second.clean
    else:
        result.count, result.clean = max(first.count, second.count), max(first.clean, second.clean)
    spread(result, second)
    return result


class Entry:
    def __init__(self):
        self.current = None
        self.inherited = None
        self.quarantined = None


def projection(entry, defects):
    values = [layer.value for layer in (entry.current, entry.inherited) if layer is not None]
    if 'quarantine_read' in defects and entry.quarantined is not None:
        values.append(entry.quarantined.value)
    return max(values) if values else None


def transition(entry, running, defects):
    if running is None or entry.current is None:
        return
    if (entry.current.game, entry.current.method) == running:
        return
    if 'transition_drops' not in defects:
        entry.inherited = combine(entry.inherited, entry.current, defects)
    entry.current = None


def merge_entries(memory, disk, defects):
    merged = copy.deepcopy(memory)
    for key, on_disk in disk.items():
        mine = merged.setdefault(key, Entry())
        for slot in ('current', 'inherited'):
            theirs = getattr(on_disk, slot)
            if theirs is None:
                continue
            ours = getattr(mine, slot)
            if ours is None:
                setattr(mine, slot, copy.deepcopy(theirs))
            elif (ours.game, ours.method) == (theirs.game, theirs.method):
                setattr(mine, slot, merge_same(ours, theirs, defects))
            elif slot == 'inherited':
                setattr(mine, slot, combine(ours, theirs, defects, label_from_other=False))
            else:
                # Two running layers of different revisions cannot both be running: the older goes to the taken-over.
                mine.inherited = combine(mine.inherited, theirs, defects, label_from_other=False)
        if on_disk.quarantined is not None:
            if mine.quarantined is None:
                mine.quarantined = copy.deepcopy(on_disk.quarantined)
            else:
                mine.quarantined.count = max(mine.quarantined.count, on_disk.quarantined.count)
    return merged


def everything(entry):
    values = [layer.value for layer in (entry.current, entry.inherited) if layer is not None]
    return max(values) if values else None


def total(entry):
    return sum(layer.count for layer in (entry.current, entry.inherited) if layer is not None)


def invariant_holds(merged, disk):
    for key, on_disk in disk.items():
        if key not in merged:
            return False
        mine = merged[key]
        if (on_disk.current or on_disk.inherited) and not (mine.current or mine.inherited):
            return False
        if on_disk.quarantined is not None and mine.quarantined is None:
            return False
        before, after = everything(on_disk), everything(mine)
        if before is not None and (after is None or after < before):
            return False
        if total(mine) < total(on_disk):
            return False
    return True


def read(disk, name, defects):
    """A file: readable - its content; unreadable - set aside, then the backup; missing - nothing."""
    content = disk.get(name)
    if content is None:
        if 'backup_on_missing' in defects and isinstance(disk.get(name + '.bak'), dict):
            return copy.deepcopy(disk[name + '.bak'])
        return None
    if content == GARBAGE:
        disk[name] = None
        backup = disk.get(name + '.bak')
        return copy.deepcopy(backup) if isinstance(backup, dict) else None
    return copy.deepcopy(content)


class Store:
    def __init__(self, defects):
        self.defects = defects
        self.entries = {}
        self.table = {}
        self.loaded = False
        self.running = None

    def project(self, key):
        value = projection(self.entries[key], self.defects)
        if value is None:
            self.table.pop(key, None)
        else:
            self.table[key] = value

    def load(self, disk, running, finishes=True):
        self.running = running
        self.entries, self.table = {}, {}
        if not finishes:
            self.loaded = False
            return
        old = read(disk, 'old', self.defects) or {}
        self.entries = read(disk, 'rev', self.defects) or {}
        for key, value in old.items():
            entry = self.entries.setdefault(key, Entry())
            if not valid(value):
                if entry.quarantined is None or not same(entry.quarantined.value, value) \
                        or 'legacy_every_load' in self.defects:
                    entry.quarantined = entry.quarantined or Layer('', 0)
                    entry.quarantined.value = value
                    entry.quarantined.count += 1
                continue
            known = everything(entry)
            if known is None or value > known or 'legacy_every_load' in self.defects:
                taken = Layer('', 0)
                taken.add(value, True)
                entry.inherited = combine(entry.inherited, taken, self.defects, label_from_other=entry.inherited is None)
        for key, entry in self.entries.items():
            transition(entry, self.unknown_as_revision(running), self.defects)
            self.project(key)
        self.loaded = True

    def unknown_as_revision(self, running):
        if running is None and 'unknown_is_revision' in self.defects:
            return ('', -1)
        return running

    def measure(self, key, value, clean, running):
        self.running = running
        entry = self.entries.setdefault(key, Entry())
        if not valid(value):
            entry.quarantined = entry.quarantined or Layer('', 0)
            entry.quarantined.value = value
            entry.quarantined.count += 1
            self.project(key)
            return
        revision = self.unknown_as_revision(running)
        if revision is not None:
            transition(entry, revision, self.defects)
            entry.current = entry.current or Layer(*revision)
            entry.current.add(value, clean)
        else:
            entry.inherited = entry.inherited or Layer('', 0)
            entry.inherited.add(value, clean)
        self.project(key)

    def save(self, disk, write_fails=False):
        if not self.loaded:
            return False
        revision = self.unknown_as_revision(self.running)
        snapshot = copy.deepcopy(self.entries)
        for entry in snapshot.values():
            transition(entry, revision, self.defects)
        on_disk = disk.get('rev')
        if on_disk == GARBAGE:
            return False
        on_disk = copy.deepcopy(on_disk) or {}
        for entry in on_disk.values():
            transition(entry, revision, self.defects)
        merged = merge_entries(snapshot, on_disk, self.defects)
        if not invariant_holds(merged, on_disk):
            return False
        if write_fails:
            return False
        disk['rev'] = copy.deepcopy(merged)
        disk['rev.bak'] = copy.deepcopy(merged)
        if 'old_file_dropped' in self.defects and disk.get('rev') is not None:
            return True
        old = disk.get('old')
        if old == GARBAGE:
            return True
        old = dict(old or {})
        old = {key: value for key, value in old.items() if valid(value)}
        for key, value in self.table.items():
            if valid(value):
                old[key] = max(value, old.get(key, value))
        disk['old'] = old
        disk['old.bak'] = dict(old)
        return True


class Truth:
    """What was really stored, kept apart from the store's own bookkeeping."""

    def __init__(self, legacy):
        self.values = {key: [value] for key, value in legacy.items() if valid(value)}
        self.stored = {key: 0 for key in IDS}
        self.brought_in = {key: (1 if key in legacy else 0) for key in IDS}
        self.by_revision = {}
        self.counts_lost = False

    def highest(self, key):
        values = self.values.get(key)
        return max(values) if values else None


def check(store, truth, running, where, found, pending=()):
    """Readings of this session not yet in a file are in memory: counted with the stored ones."""
    for key in IDS:
        unsaved = [value for key_pending, value, _ in pending if key_pending == key]
        stored_values = truth.values.get(key, [])
        expected = max(stored_values + unsaved) if stored_values or unsaved else None
        seen = store.table.get(key)
        if seen is not None and not valid(seen):
            found.add('READ')
        if (expected is None) != (seen is None) or (expected is not None and expected != seen):
            found.add('DELETED' if expected is None else 'READ')
        entry = store.entries.get(key)
        counted = total(entry) if entry else 0
        if not truth.counts_lost and counted < truth.stored[key] + len(unsaved):
            found.add('LOST')
        if counted > truth.stored[key] + len(unsaved) + truth.brought_in[key]:
            found.add('DOUBLED')
        if running is not None and not truth.counts_lost:
            under = truth.by_revision.get((key, running), 0) \
                + sum(1 for key_pending, _, at in pending if key_pending == key and at == running)
            current = entry.current if entry else None
            if under > 0 and (current is None or (current.game, current.method) != running or current.count != under):
                found.add('CURRENT')
            if under == 0 and current is not None and current.count > 0 and where == 'load':
                found.add('CURRENT')


def run(seed, defects):
    rng = random.Random(seed)
    legacy = {key: round(rng.uniform(0.05, 0.6), 3) for key in IDS if rng.random() < 0.7}
    if rng.random() < 0.3:
        legacy[rng.choice(IDS)] = rng.choice([9.41, -0.2])
    disk = {'old': dict(legacy)}
    truth = Truth(legacy)
    game, method = 1, 1
    found = set()
    for _ in range(SESSIONS):
        between = rng.random()
        if between < 0.15:
            game += 1
        elif between < 0.2:
            method += 1
        elif between < 0.25 and isinstance(disk.get('rev'), dict):
            disk['rev'] = GARBAGE
            if rng.random() < 0.3:
                disk['rev.bak'] = GARBAGE
                truth.counts_lost = True
        elif between < 0.28 and isinstance(disk.get('old'), dict) and disk.get('old') == disk.get('old.bak'):
            disk['old'] = GARBAGE
        elif between < 0.31:
            disk['rev'] = None
            disk['old'] = None
            truth = Truth({})
        elif between < 0.36:
            # An older RSR: the old file alone, no backup, the highest value only, nothing invalid stored.
            old = disk.get('old')
            old = dict(old) if isinstance(old, dict) else {}
            old = {key: value for key, value in old.items() if valid(value)}
            for _ in range(READINGS):
                key, value = rng.choice(IDS), round(rng.uniform(0.05, 0.8), 3)
                if value > old.get(key, -1.0):
                    if value > (truth.highest(key) if truth.highest(key) is not None else -1.0):
                        truth.brought_in[key] += 1
                    old[key] = value
                    truth.values.setdefault(key, []).append(value)
            disk['old'] = old
            continue

        readable = rng.random()
        known_from = 0 if readable < 0.8 else (rng.randint(1, READINGS) if readable < 0.9 else READINGS + 1)
        finishes = rng.random() > 0.05
        store = Store(defects)
        store.load(disk, (game, method) if known_from == 0 else None, finishes)
        if finishes:
            check(store, truth, (game, method) if known_from == 0 else None, 'load', found)
        session = []
        for index in range(READINGS):
            running = (game, method) if index + 1 >= known_from else None
            key = rng.choice(IDS)
            value = rng.choice([float('nan'), 1.7, -0.1]) if rng.random() < 0.08 else round(rng.uniform(0.05, 0.8), 3)
            clean = rng.random() > 0.2
            store.measure(key, value, clean, running)
            if valid(value):
                session.append((key, value, running))
            session = commit(store, disk, truth, session, rng.random() < 0.05)
            if finishes:
                check(store, truth, running, 'reading', found, session)
        # A session whose load did not finish writes nothing: its readings are lost, as before the revision.
        if session:
            commit(store, disk, truth, session, False)
    return found


def commit(store, disk, truth, session, write_fails):
    if not store.save(disk, write_fails):
        return session
    for key, value, running in session:
        truth.values.setdefault(key, []).append(value)
        truth.stored[key] += 1
        if running is not None:
            truth.by_revision[(key, running)] = truth.by_revision.get((key, running), 0) + 1
    return []


def model(defects=frozenset(), first=False):
    """The checks violated over all runs; with first, as soon as one is."""
    found = set()
    for seed in range(RUNS):
        found |= run(seed, defects)
        if first and found:
            break
    return found


def self_test():
    clean = model()
    assert not clean, f'the design breaks its own checks: {sorted(clean)}'
    missed = []
    for defect in DEFECTS:
        if not model(frozenset([defect]), first=True):
            missed.append(defect)
    assert not missed, f'defects not caught: {missed}'
    print(f'self-test ok: each of {len(DEFECTS)} defects is caught ({", ".join(DEFECTS)})')


def main():
    try:
        self_test()
    except AssertionError as error:
        print(f'FAILED: {error}')
        return 1
    if '--self-test' in sys.argv:
        return 0
    print(f'{RUNS} runs x {SESSIONS} sessions x {READINGS} readings: READ, LOST, DOUBLED, CURRENT, DELETED hold')
    return 0


if __name__ == '__main__':
    sys.exit(main())

#!/usr/bin/env python3
"""Weaving against latency, counted in the server's time: how many oGCDs the server processes between two GCDs, how
late the GCD comes there, and how many requests reach the server before its animation lock ends - for the ways the
client's lock can be handled, the two ways RSR can decide a weave, and three ways the server may treat an early
request (concept 03, "Weben, Animationssperre und Latenz", A753).

The game, from the sources named in concept 03:
- An instant action sets a client lock of 0.5 s on the press. The server's answer replaces it with the action's lock
  (mostly 0.6 s), counted from the answer, so the next press waits lock + delay; the delay is the way up, the wait for
  the server's tick, the way down and the frame the answer is read in.
- The server processes on a tick: in his ACT log of 11.11.2026 every own result arrived on a grid of 44.5 ms.
- The client sends only on a frame. Without a framerate tweak each action loses the rest of its frame; BossModReborn's
  and RSR's "Remove framerate-induced cooldown delay" credits that rest back to the next lock and recast.
- The GCD recast runs on the client from the press, on the server from its processing.

What the server does with a request that reaches it before the lock of the previous one ended is not known (concept
03); the model runs all three: 'queue' (held until the lock ends, processed at the next tick), 'reject' (no answer;
the client presses again once its own 0.5 s lock ends), 'ignore' (processed at the next tick as if nothing were
pending). A weave the client sees is worth only what the server processes.

Lock handling ("variant"), each as its source states it:
- game:      no tweak.
- fixed:     BossModReborn and RSR before A753: the delay is cut to 20 ms when longer; framerate tweak on.
- xivalex:   XivAlexander: the network round trip of this request comes off, the server's part stays (it measures the
             socket; the model gives it the true value); its own cooldown handling, counted as the framerate tweak.
- noclippy:  NoClippy: 40 ms simulated round trip plus a share for jitter above the average; no framerate tweak.
- measured:  A753 without a set value: the least delay of the earlier answers in the zone less their mean deviation
             comes off; framerate tweak on.
- adaptive:  A753, the controller built: the measured cut plus an extra learned per answer. Each answer adds r frames,
             each request without an answer takes (1 - r) frames off, r = frame time / client lock on a press: the share
             of refused requests settles at r. A late answer is no loss - the server held the request and processed it -
             and does not count. Framerate tweak.

Weave decision ("policy"): 'fixed' - RSR before A753, an oGCD while more than max(0.5 s, 25 % of the GCD) is left;
'dynamic' - while the GCD left is at least the lock plus the rest the client measures after it (mean plus deviation).

Assumptions, named: oGCDs come free at once with probability `ready_now`, else at a uniform time in the rest of the
window; GCDs are instant; network up and down are half the round trip each, with independent jitter.

    python3 .github/scripts/audit/action_timing_model.py              # tables
    python3 .github/scripts/audit/action_timing_model.py --self-test
"""
import math
import random
import sys

TICK = 0.0445          # measured: his ACT log of 11.11.2026 (A753)
LOCK = 0.6             # the server's lock of most instant actions
CLIENT_LOCK = 0.5      # the client's lock on the press, before the answer
ACTION_AHEAD = 0.25    # RSR's default of "Action ahead"
CLIP_WINDOW = 0.5      # the fixed window before the GCD (ActionHelper, before A753)
FIXED_KEEP = 0.020     # BossModReborn's default "Animation lock max. simulated delay"
NOCLIPPY_RTT = 0.04    # NoClippy's simulatedRTT
NOCLIPPY_WEIGHT = 0.1  # NoClippy's weight of a new delay in its average
READY_NOW = 0.5        # assumed share of oGCDs ready the moment the lock ends
FRAMERATE_TWEAK = {'game': False, 'fixed': True, 'xivalex': True, 'noclippy': False, 'measured': True, 'adaptive': True}


class Run:
    def __init__(self, gcd, rtt, jitter, variant, policy, fps, server, seed, ready_now):
        self.rng = random.Random(seed)
        self.gcd, self.rtt, self.jitter = gcd, rtt, jitter
        self.variant, self.policy, self.server, self.ready_now = variant, policy, server, ready_now
        self.frame = 1 / fps
        self.tweak = FRAMERATE_TWEAK[variant]
        self.state = {}
        self.kept_mean, self.kept_dev, self.kept_n = 0.0, 0.0, 0
        self.server_lock_end = -1.0
        self.server_recast_end = -1.0
        self.early = self.requests = 0
        self.held = 0.0

    def on_frame(self, t):
        return math.ceil(t / self.frame - 1e-9) * self.frame

    @staticmethod
    def tick_after(t):
        return math.ceil(t / TICK - 1e-9) * TICK

    def send(self, ideal, gcd):
        """A request wanted at `ideal` (client time). Returns (when the server processed it, or None if it rejected
        it; when the client can act next, as the lock counts it)."""
        rng = self.rng
        sent = self.on_frame(ideal)
        base = ideal if self.tweak else sent              # the framerate tweak counts the lock from the ideal time
        up = max(0.0025, rng.gauss(self.rtt / 2, self.jitter / 2))
        down = max(0.0025, rng.gauss(self.rtt / 2, self.jitter / 2))
        arrival = sent + up
        self.requests += 1
        # Only the lock: the server takes a GCD that reaches it a tick wait before its own recast ends, or every GCD
        # would fall a tick further behind the client's (inference from the absence of such a drift, unproven).
        pending = self.server_lock_end
        self.held = 0.0
        if arrival < pending:
            self.early += 1
            if self.server == 'queue':
                processed = self.tick_after(pending)
                self.held = processed - self.tick_after(arrival)
            elif self.server == 'reject':
                self.state['missing'] = True
                return None, base + CLIENT_LOCK
            else:
                processed = self.tick_after(arrival)
        else:
            processed = self.tick_after(arrival)
        self.server_lock_end = processed + LOCK
        if gcd:
            self.server_recast_end = processed + self.gcd
        delay = processed + down + rng.random() * self.frame - sent
        kept = self.keep(delay, up + down)
        self.learn(kept)
        return processed, max(base + LOCK + kept, base + CLIENT_LOCK)

    def keep(self, delay, network):
        """What remains of the delay after the variant's cut."""
        v, s = self.variant, self.state
        if v == 'game':
            return delay
        if v == 'fixed':
            return min(delay, FIXED_KEEP)
        if v == 'xivalex':
            return max(0.0, delay - network)
        if v == 'noclippy':
            if delay <= NOCLIPPY_RTT:
                return delay
            average = s.get('avg', delay)
            s['avg'] = average * (1 - NOCLIPPY_WEIGHT) + delay * NOCLIPPY_WEIGHT
            return NOCLIPPY_RTT * max(delay / average, 1)
        n = s.get('n', 0)
        if v == 'adaptive' and n >= 2:
            # Each answer adds r frames to the extra, each missing one takes (1 - r) frames off: the share of requests
            # without an answer settles at r, the frame time over the client's lock on a press - where what a refused
            # request costs (a new lock of 0.5 s) weighs what a frame of cut gains. A late answer is no loss (the server
            # held the request and still processed it) and does not count.
            r = self.frame / CLIENT_LOCK
            if s.pop('missing', False):
                s['extra'] = max(0.0, s.get('extra', 0.0) - self.frame * (1 - r))
            else:
                # no more than the longest delay seen can come off
                envelope = max(0.0, s['min'] - s['dev'])
                s['extra'] = min(s.get('extra', 0.0) + self.frame * r, max(0.0, s.get('max', 0.0) - envelope))
        cut = max(0.0, s['min'] - s['dev']) + s.get('extra', 0.0) if n >= 2 else 0.0
        s['cut'] = cut
        mean = s.get('mean', delay)
        s['dev'] = (s.get('dev', 0.0) * n + abs(delay - mean)) / (n + 1)
        s['mean'] = (mean * n + delay) / (n + 1)
        s['min'] = min(s.get('min', delay), delay)
        s['max'] = max(s.get('max', delay), delay)
        s['n'] = n + 1
        s.setdefault('delays', []).append(delay)
        return delay - min(cut, delay)

    def kept_estimate(self):
        """The rest the dynamic policy expects after the lock, as RSR's KeptEstimate: with RSR cutting, what the current
        cut leaves of the mean delay plus its deviation; else the mean and deviation of what was kept."""
        s = self.state
        if self.variant in ('measured', 'adaptive') and s.get('n', 0) >= 2:
            # What the current cut leaves of each delay the zone saw: its mean plus its mean deviation.
            cut = s.get('cut', 0.0)
            rests = [max(0.0, d - cut) for d in s['delays']]
            mean = sum(rests) / len(rests)
            return mean + sum(abs(x - mean) for x in rests) / len(rests)
        return self.kept_mean + self.kept_dev if self.kept_n else 0.1

    def learn(self, kept):
        n = self.kept_n
        self.kept_dev = (self.kept_dev * n + abs(kept - self.kept_mean)) / (n + 1) if n else 0.0
        self.kept_mean = (self.kept_mean * n + kept) / (n + 1)
        self.kept_n = n + 1

    def run(self, cycles):
        rng = self.rng
        weaves = late = 0.0
        late_count = triples = 0
        client_free = 0.0
        client_recast_end = 0.0
        last_gcd = None
        for _ in range(cycles):
            # The GCD: queued, it goes at the first moment recast and lock are both done.
            processed = None
            came_free = client_recast_end
            while processed is None:
                ideal = max(client_free, client_recast_end)
                processed, client_free = self.send(ideal, gcd=True)
            client_recast_end = (ideal if self.tweak else self.on_frame(ideal)) + self.gcd
            if last_gcd is not None:
                # How long a lock kept the GCD back after it came free on the client, and the server held it.
                behind = ideal - came_free + self.held
                if behind > 1e-6:
                    late += behind
                    late_count += 1
            last_gcd = processed
            woven = 0
            while True:
                start = client_free if rng.random() < self.ready_now \
                    else client_free + rng.random() * max(0.0, client_recast_end - client_free)
                left = client_recast_end - start
                if self.policy == 'fixed':
                    threshold = max(CLIP_WINDOW, ACTION_AHEAD * self.gcd)
                else:
                    threshold = LOCK + self.kept_estimate()
                if left <= threshold:
                    break
                done, client_free = self.send(start, gcd=False)
                if done is not None:
                    woven += 1
            weaves += woven
            triples += woven >= 3
        return {'weaves': weaves / cycles, 'late_ms': late / cycles * 1000, 'late_share': late_count / cycles,
                'early': self.early / max(self.requests, 1), 'triples': triples / cycles}


def simulate(gcd, rtt, jitter, variant, policy, fps=60, server='queue', cycles=1500, seed=7, ready_now=READY_NOW):
    return Run(gcd, rtt, jitter, variant, policy, fps, server, seed, ready_now).run(cycles)


def self_test():
    # Without delay and with the framerate tweak, three weaves fit in 2.5 s when every oGCD is ready.
    r = simulate(2.5, 0.0, 0.0, 'xivalex', 'dynamic', fps=144, ready_now=1.0, cycles=200)
    assert r['triples'] > 0.5 and r['late_ms'] < 5, r
    # With a long round trip and no tweak, the fixed rule makes the GCD late on the server; the dynamic one does not.
    a = simulate(2.5, 0.2, 0.02, 'game', 'fixed')
    b = simulate(2.5, 0.2, 0.02, 'game', 'dynamic')
    assert a['late_ms'] > b['late_ms'] + 1, (a, b)
    # The game's own handling never sends a request before the server's lock ends.
    g = simulate(2.5, 0.1, 0.02, 'game', 'dynamic')
    assert g['early'] == 0.0, g
    # A cut to 20 ms sends early often; the measured cut almost never.
    f = simulate(2.5, 0.1, 0.02, 'fixed', 'dynamic', server='queue')
    m = simulate(2.5, 0.1, 0.02, 'measured', 'dynamic', server='queue')
    assert f['early'] > 0.2 and m['early'] < 0.02, (f, m)
    # The controller: where the server takes early requests it comes close to the 20 ms cut; where it refuses them, the
    # share of refused requests stays near r (frame time over the client lock) and below the 20 ms cut's.
    burst = dict(fps=60, ready_now=1.0, cycles=600)
    fixed_ignore = simulate(2.5, 0.1, 0.02, 'fixed', 'dynamic', server='ignore', **burst)
    adaptive_ignore = simulate(2.5, 0.1, 0.02, 'adaptive', 'dynamic', server='ignore', **burst)
    assert adaptive_ignore['weaves'] >= fixed_ignore['weaves'] - 0.1, (adaptive_ignore, fixed_ignore)
    fixed_reject = simulate(2.5, 0.1, 0.02, 'fixed', 'dynamic', server='reject', **burst)
    adaptive_reject = simulate(2.5, 0.1, 0.02, 'adaptive', 'dynamic', server='reject', **burst)
    assert adaptive_reject['early'] <= 2 * (1 / 60) / CLIENT_LOCK and adaptive_reject['weaves'] > fixed_reject['weaves'], \
        (adaptive_reject, fixed_reject)
    print('self-test ok: three weaves fit in 2.5 s without delay; the fixed rule makes the GCD late on a long round '
          'trip and the dynamic one does not; the game never sends early; a 20 ms cut sends early, the measured cut not; '
          'the controller nears the 20 ms cut where the server takes early requests and keeps refusals near r where not')


def main(argv):
    self_test()
    if '--self-test' in argv:
        return 0
    variants = ('game', 'fixed', 'xivalex', 'noclippy', 'measured', 'adaptive')
    for title, ready in (('Mixed availability (half the oGCDs ready at once)', READY_NOW), ('Burst (every oGCD ready)', 1.0)):
        print()
        print(title + '. Per server behaviour (queue | reject | ignore): weaves per GCD the server processed, GCD late on')
        print('the server in ms per GCD, share of cycles with three weaves, share of requests early at the server.')
        for gcd in (2.5, 2.0):
            for fps in (60, 144):
                for rtt in (0.03, 0.1, 0.25):
                    for variant in variants:
                        for policy in ('fixed', 'dynamic'):
                            cells = []
                            for server in ('queue', 'reject', 'ignore'):
                                r = simulate(gcd, rtt, rtt * 0.2, variant, policy, fps=fps, server=server,
                                             ready_now=ready, cycles=800)
                                cells.append(f"{r['weaves']:4.2f} {r['late_ms']:5.1f} {r['triples']:4.0%} {r['early']:4.0%}")
                            print(f'{gcd:3.1f} {fps:3d}fps {rtt:4.2f}  {variant:8s} {policy:7s} ' + ' | '.join(cells))
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))

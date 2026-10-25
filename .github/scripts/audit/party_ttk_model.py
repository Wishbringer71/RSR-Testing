#!/usr/bin/env python3
"""A party member's time to death: the fork's own history against upstream's regression (concept 08, "Der Grenzwert
gilt für den gesamten Schadenseingang", A354).

Not a code scanner - a model calculator. The heal forecast reads a member's time to death (GetCorrectedTTK) and
carries his health forward by the heal's lead time: share = 1 - lead / ttk, so the health expected when the heal
lands is ratio - lead * rate. Two estimators of the rate, both on the member's health ratio sampled once a second and
net of every heal, both scored and corrected the same way afterwards (ScoreTtkForecast: bias smoothed 3:1, bounded
0.25-4):
  SINCE  the fork's ComputeTTK (upstream's estimator before 64413ccef, kept for the party in A341): the fall from the
         first sample below full health in the last 240 s to the mean of the last five such samples, over the time
         since; no answer within 2.5 s of that first sample, and none while the mean is not below it
  REG    upstream's TimeToKillTracker since 64413ccef: a least-squares line through the last 30 s, at least three
         samples, the history dropped at full health - and, for a member, not started over when health rises
  REG0   REG without the correction
  MAX    the larger rate of SINCE and REG, then corrected - whichever sees the steeper fall
  ANCHOR SINCE whose first sample moves on to any later one at or above it - the fall since health last stood that
         high, so a level above the first damaged sample no longer blinds it

Measured per estimator over constructed fights, in the seconds of danger - the health at the landing below 60 % and
more than 10 points below now: blind seconds, where the estimator gives no rate and the forecast sees nothing, and the
mean optimism, expected minus actual health at the landing (positive is the dangerous direction); and false alarms -
seconds the forecast puts the member 20 points lower than he will be while he does not fall. Open loop: the heals in a trace do
not react to the forecast. ASSUMPTIONS, not from a source: lead 3 s (rest of a GCD and a cast); the traces below.

Usage: python3 .github/scripts/audit/party_ttk_model.py
"""
import math

LEAD = 3.0
HISTORY = 240
SINCE_START = 2.5
SINCE_WINDOW = 5
REG_WINDOW = 30
REG_MIN = 3
NOISE = 0.03
ALARM = 0.20
DANGER_LEVEL = 0.6
DANGER_FALL = 0.10


def since_rate(hist, t):
    """hist: list of (time, ratio), oldest first, the last HISTORY seconds."""
    recent = [(at, r) for at, r in hist if t - at < HISTORY]
    damaged = [(at, r) for at, r in recent if r != 1.0]
    if not damaged:
        return math.nan
    start, first = damaged[0]
    if t - start < SINCE_START:
        return math.nan
    window = [r for _, r in damaged[-SINCE_WINDOW:]]
    diff = first - sum(window) / len(window)
    if diff <= 0:
        return math.nan
    return diff / (t - start)


def anchor_rate(hist, t):
    recent = [(at, r) for at, r in hist if t - at < HISTORY]
    damaged = [(at, r) for at, r in recent if r != 1.0]
    if not damaged:
        return math.nan
    start, first = damaged[0]
    for at, r in damaged[1:]:
        if r >= first:
            start, first = at, r
    if t - start < SINCE_START:
        return math.nan
    window = [r for _, r in damaged[-SINCE_WINDOW:]]
    diff = first - sum(window) / len(window)
    if diff <= 0:
        return math.nan
    return diff / (t - start)


def reg_rate(samples):
    if len(samples) < REG_MIN:
        return math.nan
    mt = sum(a for a, _ in samples) / len(samples)
    mr = sum(r for _, r in samples) / len(samples)
    cov = sum((a - mt) * (r - mr) for a, r in samples)
    var = sum((a - mt) ** 2 for a, _ in samples)
    rate = -cov / var if var > 0 else 0.0
    return rate if rate > 0 else math.nan


def run(trace, estimator):
    """trace: ratio per second. Returns (blind seconds, falling seconds, mean optimism, false alarms)."""
    hist = []
    reg = []
    bias = 1.0
    last = None
    forecasts = []
    for t, ratio in enumerate(trace):
        hist.append((t, ratio))
        if ratio >= 1.0:
            reg = []
        else:
            reg.append((t, ratio))
            reg = [(a, r) for a, r in reg if t - a <= REG_WINDOW]
        if estimator == "SINCE":
            rate = since_rate(hist, t)
        elif estimator == "ANCHOR":
            rate = anchor_rate(hist, t)
        elif estimator == "MAX":
            rates = [x for x in (since_rate(hist, t), reg_rate(reg)) if x == x]
            rate = max(rates) if rates else math.nan
        else:
            rate = reg_rate(reg)
        ttk = ratio / rate if rate == rate and rate > 0 else math.nan
        # Scoring the forecast filed a second ago against the fall since.
        if estimator != "REG0" and last is not None:
            l_ratio, l_ttk = last
            fall = l_ratio - ratio
            if l_ttk == l_ttk and l_ttk > 0 and fall > 0:
                expected = l_ratio / l_ttk
                if expected > 0:
                    bias = min(4.0, max(0.25, (bias * 3 + fall / expected) / 4))
        last = (ratio, ttk)
        corrected = ttk / bias if ttk == ttk and estimator != "REG0" else ttk
        expected_rate = ratio / corrected if corrected == corrected and corrected > 0 else 0.0
        forecasts.append(ratio - LEAD * expected_rate)

    blind = falling = alarms = 0
    optimism = []
    for t in range(len(trace) - int(LEAD)):
        actual = trace[t + int(LEAD)]
        if actual < DANGER_LEVEL and trace[t] - actual > DANGER_FALL:
            falling += 1
            if forecasts[t] >= trace[t]:
                blind += 1
            optimism.append(forecasts[t] - actual)
        elif trace[t] - actual <= NOISE and forecasts[t] < actual - ALARM:
            alarms += 1
    return blind, falling, (sum(optimism) / len(optimism) if optimism else 0.0), alarms


def clamp(r):
    return max(0.0, min(1.0, r))


def tank_pull():
    """A dungeon pull: a hard start (to 60 % in 3 s), healed back; steady autos 4 %/s against 12 % heals every 3 s; at
    60 s a second pack doubles the damage for 8 s; after it the healers catch up."""
    r, out = 1.0, []
    for t in range(150):
        if t < 3:
            r -= 0.13
        elif 60 <= t < 68:
            r -= 0.09
        else:
            r -= 0.04
        if t >= 3 and t % 3 == 0:
            r += 0.12 if not (60 <= t < 68) else 0.12
        if 68 <= t < 76:
            r += 0.06
        r = clamp(r)
        out.append(round(r, 4))
    return out


def tank_boss():
    """A boss: autos 3 %/s, a 12 % heal every 4 s; a tankbuster of 45 % at 90 s and 150 s, healed back after."""
    r, out = 1.0, []
    for t in range(200):
        r -= 0.03
        if t % 4 == 0:
            r += 0.12
        if t in (90, 150):
            r -= 0.45
        if t in (92, 93, 152, 153):
            r += 0.2
        r = clamp(r)
        out.append(round(r, 4))
    return out


def dps_raidwides():
    """A damage dealer: a raidwide of 35 % every 30 s, healed back to full within 6 s; a bleed of 2 %/s from 100 s."""
    r, out = 1.0, []
    for t in range(180):
        if t % 30 == 10:
            r -= 0.35
        elif t % 30 in range(11, 17):
            r += 0.07
        if t >= 100:
            r -= 0.02
        r = clamp(r)
        out.append(round(r, 4))
    return out


def tank_falls_late():
    """The pull that blinds SINCE: the first hit takes 40 %, healers then hold him at 85-95 %, and 70 s in the heals
    stop for 10 s (the healer is dead or moving) while autos go on at 5 %/s."""
    r, out = 1.0, []
    for t in range(140):
        if t == 1:
            r -= 0.40
        r -= 0.05
        if t >= 2 and not (70 <= t < 80) and t % 2 == 0:
            r += 0.12 if r < 0.85 else 0.08
        r = clamp(r)
        out.append(round(r, 4))
    return out


def tank_hard_first_hit():
    """The pull that blinds SINCE: the first second takes 35 % (five enemies on him at once); healers then hold him at
    90-100 %; 60 s in a second pack lands while a healer moves - 9 %/s against 6 %/s for 10 s, then a pause of the
    heals for 4 s."""
    r, out = 1.0, []
    for t in range(140):
        if t == 0:
            r -= 0.35
        elif 60 <= t < 74:
            r -= 0.09
        else:
            r -= 0.04
        if 1 <= t < 60 or t >= 74:
            r += 0.06 if r < 0.97 else 0.0
        elif 60 <= t < 70:
            r += 0.06
        r = clamp(r)
        out.append(round(r, 4))
    return out


SCENARIOS = [
    ("tank, dungeon pull, second pack at 60 s", tank_pull),
    ("tank, boss, tankbusters at 90 and 150 s", tank_boss),
    ("damage dealer, raidwides, bleed from 100 s", dps_raidwides),
    ("tank, heals stop at 70 s", tank_falls_late),
    ("tank, hard first second, second pack at 60 s", tank_hard_first_hit),
]


def self_test():
    # A steady fall of 2 %/s from full: both see it, at the right rate once started.
    trace = [1.0 - 0.02 * t for t in range(30)]
    hist = [(t, r) for t, r in enumerate(trace)]
    assert abs(since_rate(hist, 29) - 0.02) < 0.004
    assert abs(reg_rate(hist[1:]) - 0.02) < 1e-9
    # SINCE's blind spot: health above the first damaged sample gives no rate, though it falls now.
    hist = [(0, 0.6), (1, 0.7), (2, 0.95), (3, 0.93), (4, 0.91), (5, 0.89), (6, 0.87), (7, 0.85)]
    assert math.isnan(since_rate(hist, 7))
    assert reg_rate(hist[2:]) > 0 and anchor_rate(hist, 7) > 0
    # Rising health gives no rate to either.
    assert math.isnan(reg_rate([(0, 0.5), (1, 0.6), (2, 0.7)]))
    print("self-test ok: a steady fall, SINCE's blind spot above the first damaged sample, and a rise each get"
          " their verdict")


if __name__ == "__main__":
    self_test()
    print(f"lead {LEAD:.0f} s; per estimator, in the seconds of danger: blind / all, mean optimism (points, positive =")
    print("forecast higher than the health at the landing); then false alarms")
    for title, make in SCENARIOS:
        trace = make()
        parts = []
        for name in ("SINCE", "REG", "REG0", "MAX", "ANCHOR"):
            blind, falling, opt, alarms = run(trace, name)
            parts.append(f"{name:5s} {blind:3d}/{falling:3d} {opt * 100:+5.1f} {alarms:3d}")
        print(f"  {title}:")
        print("    " + "   ".join(parts))

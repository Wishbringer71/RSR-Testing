# Audit scans

Static scans that found the defect classes recorded in `AUDIT_LOG.md` (sections A8 and A10).
They are kept here as regression protection: when a section of the tree is audited again, these
run first, so a class that was closed once does not have to be rediscovered by reading.

Run from the repository root with no arguments:

```
python3 .github/scripts/audit/scan.py
```

| Script | Defect classes | Found (AUDIT_LOG) |
|---|---|---|
| `scan.py` | Range/default mismatches, config properties never read, stale `RotationDesc`, dead code, unguarded dereferences | A8: SAM `MeikyoShisuiCountdown`, BLU `UseBasicInstinct`/`UseMightyGuard`, nine `RotationDesc`, eleven configs, `OldUpdateTargets` |
| `mitscan.py` | Mitigation actions in methods that carry no danger gate | A9: SMN Radiant Aegis in `GeneralAbility` |
| `scan2.py` | Percent-versus-ratio comparisons, float equality, `usedUp`, `skipStatusProvideCheck`, contradictory level predicates, repeated conditions, unguarded division | A10: four HP thresholds compared against the wrong scale |
| `scan3.py` | `CanUse` blocks that never return, identical bodies in consecutive branches, level gate naming another action | A10: Viper structural finding |
| `scan4.py` | `[Range]` attribute versus declared default, duplicate config property names | A10: none open; the class had a real hit in A8 |
| `scan5.py` | Fork behaviour changes sitting in a dispatch path that has no switch of its own | A16: six lines, all covered by an option or already logged |
| `scan6.py` | Enum members whose ordinal moved, split by whether the enum reaches stored configuration | A16: none persisted; `SpecialMode` in-memory only |
| `scan7.py` | Public and protected members of `RotationSolver.Basic` removed or re-signed since a release, keyed by declaring type, interface members included | A16: `HasHostileCountAoeMitigation`, `ShouldCheckTargetStatus` |

`stun_coverage.py` is the odd one out: not a scanner but a model calculator for the
concept in `docs/rotation-flow/08-mitigation-synergy.md`. It simulates a stun-insertion
rule GCD by GCD and reports the coverage each one achieves. It is kept here because it
found a defect in that concept's first draft - a gate on "remaining > one GCD" that
never fires - and it is the regression guard for the numbers the concept argues from.
Run it again whenever the assumed GCD length or the stun durations change.

`scan5.py` takes a base ref (default `upstream/main`) and `--detail` for line-by-line output.
`scan6.py` and `scan7.py` take a base ref too; for both, the meaningful base is the newest fork tag,
because the contract is with the version that was actually shipped, not with upstream. `scan7.py`
defaults to that tag, `scan6.py` should be run against both.

## Self-test

A scan that reports zero findings is only meaningful when its detection is known to still work, so
each script should carry a self-test against constructed defects and fail loudly otherwise.
`scan3.py` shipped an off-by-one that made one of its classes find nothing at all, and `scan4.py`
did not recognise multi-line attribute blocks; both were caught that way.

State: every script from `scan.py` through `scan8.py` now carries one. The last three - `scan.py`,
`mitscan.py` and `scan2.py` - got theirs late, and closing that gap required a small refactor first:
their checks ran inline in the file loop and could not be called with a constructed source at all.
They now expose `scan_source()` / `scan_file()`, with the walk and the printing moved into `main()`.

That refactor immediately earned itself. `scan.py` check (f), duplicate consecutive `if` conditions,
had never been able to fire: it compared `'out act' in cond` against a string it had already run
`re.sub(r'\s+', '', ...)` over, so the spaced form could not be present. Its clean result had been
meaningless for as long as the check existed. With the comparison moved to the raw condition the
check works, and the tree is genuinely clean on it.

Two of the newer scans earned their self-test immediately. `scan5.py` attributed every change inside
an expression-bodied property to the method above it, because its declaration pattern required a
parameter list. `scan6.py` reported a clean tree twice over: `git ls-tree` does not accept the glob
pathspec that `git ls-files` does, so its base revision held no files at all, and its notion of a
persisted enum looked at public members only, which hid the `[JobConfig]` generator behind
`TargetHostileType`. Both failures produced empty output, not an error — which is the case the
self-tests now cover explicitly.

`scan7.py` needed three passes for the same reason in the other direction. It first read no
interface members at all, because those carry no visibility modifier; then, once members were keyed
by declaring type, it reported 55 phantom removals, because a prose comment containing the word
"struct" was read as a type declaration and re-owned every member below it in that file; and it
counted `internal` interface members, which are not package surface. Only the third result — two
members — is the measured one.

## scan8.py — negated-name predicates read with both polarities

Added after the same defect was found twice by hand, months apart, in
`NoNeedHealingInvuln()`: the value means "healing is due again", the name reads as the opposite, and
callers split along that gap without anything failing. The scan lists every bool member whose name
already spells a negation ("No", "Not", "Never", "Cannot", "Without") and reports those read with
both polarities somewhere in the tree. A mixed reading is not proof — a two-sided predicate is
legitimate — but it is a short list, and one side is likely to hold the wrong belief.

It found its anchor case on the first run, and a second class that had nothing to do with healing:
`IsConditionCannotTarget()` is read `return null` in seven places where the three neighbouring
correct sites use `continue`. See `TODO.md`.

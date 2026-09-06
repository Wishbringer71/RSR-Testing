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

`scan5.py` takes a base ref (default `upstream/main`) and `--detail` for line-by-line output.
`scan6.py` and `scan7.py` take a base ref too; for both, the meaningful base is the newest fork tag,
because the contract is with the version that was actually shipped, not with upstream. `scan7.py`
defaults to that tag, `scan6.py` should be run against both.

## Self-test

A scan that reports zero findings is only meaningful when its detection is known to still work, so
each script should carry a self-test against constructed defects and fail loudly otherwise.
`scan3.py` shipped an off-by-one that made one of its classes find nothing at all, and `scan4.py`
did not recognise multi-line attribute blocks; both were caught that way.

State as of the last audit round: `scan3.py` through `scan7.py` have such a self-test, `scan.py`,
`mitscan.py` and `scan2.py` do not. All three currently produce non-empty output, so the gap has not
masked anything yet, but a zero result from them carries no weight until it is closed. Tracked in
`TODO.md`.

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

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

## Self-test

A scan that reports zero findings is only meaningful when its detection is known to still work, so
each script should carry a self-test against constructed defects and fail loudly otherwise.
`scan3.py` shipped an off-by-one that made one of its classes find nothing at all, and `scan4.py`
did not recognise multi-line attribute blocks; both were caught that way.

State as of the last audit round: `scan3.py` and `scan4.py` have such a self-test, `scan.py`,
`mitscan.py` and `scan2.py` do not. All three currently produce non-empty output, so the gap has not
masked anything yet, but a zero result from them carries no weight until it is closed. Tracked in
`TODO.md`.

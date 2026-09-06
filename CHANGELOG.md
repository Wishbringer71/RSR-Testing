# Changelog

Changes in this fork that a consumer has to act on, kept because the version number cannot carry
them.

`Directory.Build.props` ties `<Version>` to the upstream release the fork is based on and marks the
fork with the build-metadata suffix `+wsh1`. NuGet strips build metadata when it normalises a
version — `1.0.7+r3456` is treated as `1.0.7`
([Package versioning](https://learn.microsoft.com/nuget/concepts/package-versioning#normalized-version-numbers))
— and `publish.yaml` passes `PackageVersion` without the suffix in any case. A breaking change to
the package surface therefore has nowhere to show up in the number under Semantic Versioning's
rules, so it is written down here instead.

Entries below the unreleased section start with the first release that carries one; earlier releases
are not reconstructed here.

## Unreleased

### Removed from RotationSolver.Basic

Both members were part of the shipped `7.5.5.41+wsh1` package. Code that overrides or reads them no
longer compiles; stored user configuration is unaffected, because Dalamud skips members it does not
recognise when deserialising.

- `CustomRotation.HasHostileCountAoeMitigation` (`public virtual bool`). Setting it kept
  `AutoStatus.DefenseArea` raised for the whole of any pull with four or more enemies in range, and
  that flag opens a job's entire defensive chain rather than the one sustain line it was named for.
  Mitigation now needs a detected area cast or a predicted raidwide again, which left the flag
  without readers. There is no replacement to migrate to. The declaration in `ICustomRotation` went
  with it, but that one was `internal` and never part of the package surface.
- `ActionConfig.ShouldCheckTargetStatus`. It had no UI and no setter, so it stayed at its default of
  `true` and made `ShouldCheckStatus` — the checkbox users actually see — unable to take effect. The
  per-action status check now runs off `ShouldCheckStatus` on both the player and the target side.

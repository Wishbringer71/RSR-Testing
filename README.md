# RotationSolverReborn — `wsh` fork

This repository is a fork of
[FFXIV-CombatReborn/RotationSolverReborn](https://github.com/FFXIV-CombatReborn/RotationSolverReborn).
It tracks upstream `main` and carries patches that were not offered upstream. **Everything
below the horizontal rule is the upstream README, taken over unchanged** — its badges, its
install instructions and its release links all refer to upstream, not to this repository.

**Releases** are tagged `<upstream version>+wsh<n>`, currently `7.5.5.41+wsh1`, and publish a
`latest.zip` asset. This repository ships no plugin repository manifest of its own, so the
install instructions below add the *upstream* plugin. `manifest.json` is unchanged as well —
`InternalName: RotationSolver` and the upstream `RepoUrl` included — so a fork build presents
itself to Dalamud under the same plugin identity as upstream.

**The `RotationSolverReborn.Basic` package** keeps upstream's `PackageId` and marks the fork
with the pre-release label `-wsh1`, so the published package is `7.5.5.41-wsh1`. Consuming it
means allowing pre-release versions or pinning the exact version: a version without a suffix
sorts higher, so in a feed holding both, a consumer that asks for neither still resolves to
upstream. Changes to the package surface are listed in [CHANGELOG.md](CHANGELOG.md), because
the version number cannot express them — its numeric part follows the upstream release, not
this fork's own compatibility.

**Where the fork documents itself**

| | |
|---|---|
| [`CHANGELOG.md`](CHANGELOG.md) | changes a consumer of `RotationSolver.Basic` has to act on |
| [`docs/rotation-flow/`](docs/rotation-flow) | design documents for the areas this fork changes |
| [`AUDIT_LOG.md`](AUDIT_LOG.md) | archive of completed reviews, including the claims that were withdrawn |
| [`TODO.md`](TODO.md) | open work only, separated into defects, technical debt and planned work |
| [`.github/scripts/audit/`](.github/scripts/audit) | the static scans that found the recorded defect classes, kept as regression protection |
| [`CLAUDE.md`](CLAUDE.md) | the working rules this fork is developed under |

Documents addressed to consumers are written in English; the working documents (`AUDIT_LOG.md`,
`TODO.md`, `docs/`) are in German.

---

# [![](https://raw.githubusercontent.com/FFXIV-CombatReborn/RebornAssets/main/IconAssets/RSR_Icon.png)](https://github.com/FFXIV-CombatReborn/RotationSolverReborn)

**RotationSolverReborn**

![Github Latest Releases](https://img.shields.io/github/downloads/FFXIV-CombatReborn/RotationSolverReborn/latest/total.svg?style=for-the-badge)
![Github License](https://img.shields.io/github/license/FFXIV-CombatReborn/RotationSolverReborn.svg?label=License&style=for-the-badge)
[![](https://dcbadge.limes.pink/api/server/p54TZMPnC9)](https://discord.gg/p54TZMPnC9)

This tool is designed to enhance your gameplay experience by performing your rotation as optimally as possible, including heals, interrupts, mitigations, and MP management.

## Features

- **Full Autorotation**: Able to fully execute combat rotation, including specialized logic for healing, mitigations, duty actions, and mechanic specific behaviour.
- **Dynamic Rotation Guidance aka Training Mode**: Offers real-time suggestions for skill rotations, tailored to your current in-game situation.
- **Customizable Settings**: Allows users to adjust the rotations based on personal preference, encounter type, and specific boss mechanics.
- **Comprehensive Database**: Includes an extensive database of class abilities to ensure accurate and effective rotation.
- **Regular Updates**: The plugin is regularly updated to reflect the latest game patches, class changes, and user feedback, ensuring it remains relevant and useful.

## Installing
- Enter `/xlsettings` in the chat window and go to the Experimental tab in the opening window.
- **Skip below the DevPlugins section to the Custom Plugin Repositories section.**
- Copy and paste the repo.json link into the first free text input field.
```
https://raw.githubusercontent.com/FFXIV-CombatReborn/CombatRebornRepo/main/pluginmaster.json
```
- Click on the + button and make sure the checkmark beside the new field is set afterwards.
- **Click on the Save-icon in the bottom right.**

Following these steps, you should be able to see all contained plugins in the Available Plugins tab in the Dalamud Plugin Installer.
No Plugins will be installed, you have just made them available. You can now select which of these plugins you actually want to install.

## Links

The rotations definitions are [here](https://github.com/FFXIV-CombatReborn/RotationSolverReborn/tree/main/RotationSolver/RebornRotations).

## Latest version of RSR for each FFXIV version
7.41
https://github.com/FFXIV-CombatReborn/RotationSolverReborn/releases/tag/7.4.1.10

7.45
https://github.com/FFXIV-CombatReborn/RotationSolverReborn/releases/tag/7.4.5.35

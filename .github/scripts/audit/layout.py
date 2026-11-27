#!/usr/bin/env python3
"""Where the code of each kind lives since upstream's restructure c170c9cd5 (18.11.2026, A878).

One place for every audit script, so that the next move of a folder is one edit, not ten. Before, each script built its
own paths (RotationSolver/Basic/Rotations/Basic, RotationSolver/RebornRotations, ...); after the move two of them read no
file at all and still passed, as their glob found nothing (A878).

The layout: RotationSolver/Rotations/<role>/<job>/ holds the job's base class (<Job>Rotation.cs and its parts), the
rotations shipped with the plugin (<JOB>_Reborn*.cs, <JOB>_Default*.cs) and third-party rotations (any other
file with a [Rotation(...)] attribute: Beiruta*, Rabbs*, Churin*). Rotations/Core holds CustomRotation, Rotations/Duties
the duty rotations and their base classes.

Usage: layout.py --self-test
"""
import os
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
CODE = ROOT / "RotationSolver"
ROTATIONS = CODE / "Rotations"
CORE = ROTATIONS / "Core"
DUTIES = ROTATIONS / "Duties"
ROLES = ("Healer", "Tank", "Melee", "Ranged", "Magical", "Limited")
DATA = CODE / "Engine" / "Data"
CONFIGS = CODE / "Configuration"

SHIPPED_NAME = re.compile(r"^[A-Z]{3}_")
ROTATION_ATTRIBUTE = re.compile(r"^\s*\[Rotation\(", re.M)


def job_dirs(rotations=ROTATIONS):
    """Every job folder, sorted."""
    found = []
    for role in ROLES:
        base = rotations / role
        if base.is_dir():
            found.extend(sorted(p for p in base.iterdir() if p.is_dir()))
    return found


def is_shipped(path):
    """A rotation shipped with the plugin: the job code, then an underscore (WHM_Reborn.cs, WHM_Reborn.Universal.cs, MCH_Default.PvP.cs)."""
    return bool(SHIPPED_NAME.match(Path(path).name))


def is_third_party(path, text=None):
    """A rotation of another author in a job folder: a [Rotation(...)] attribute, but no job code in front of the name."""
    path = Path(path)
    if is_shipped(path) or path.parent.parent.name not in ROLES:
        return False
    if text is None:
        text = path.read_text(encoding="utf-8-sig")
    return bool(ROTATION_ATTRIBUTE.search(text))


def job_files(rotations=ROTATIONS):
    """Every .cs file in the job folders."""
    return [p for d in job_dirs(rotations) for p in sorted(d.glob("*.cs"))]


def shipped_rotation_files(rotations=ROTATIONS):
    """The rotations shipped with the plugin, their parts included, in the job folders."""
    return [p for p in job_files(rotations) if is_shipped(p)]


def base_rotation_files(rotations=ROTATIONS):
    """The job base classes and their parts (everything in a job folder that is neither shipped nor third-party)."""
    return [p for p in job_files(rotations) if not is_shipped(p) and not is_third_party(p)]


def third_party_files(rotations=ROTATIONS):
    return [p for p in job_files(rotations) if is_third_party(p)]


def self_test():
    import tempfile
    with tempfile.TemporaryDirectory() as tmp:
        rot = Path(tmp) / "Rotations"
        job = rot / "Healer" / "WhiteMage"
        job.mkdir(parents=True)
        (job / "WhiteMageRotation.cs").write_text("public partial class WhiteMageRotation : CustomRotation { }")
        (job / "WhiteMageRotation.HitResponse.cs").write_text("partial class WhiteMageRotation { }")
        (job / "WHM_Reborn.cs").write_text('[Rotation("Reborn", CombatType.PvE)]\npublic sealed class WHM_Reborn { }')
        (job / "WHM_Reborn.Universal.cs").write_text("partial class WHM_Reborn { }")
        (job / "WHM_Default.PVP.cs").write_text('[Rotation("Default PvP", CombatType.PvP)]\nclass WHM_DefaultPvP { }')
        (job / "BeirutaWHM.cs").write_text('[Rotation("BeirutaWHM", CombatType.PvE)]\npublic sealed class BeirutaWHM { }')
        lim = rot / "Limited" / "Beastmaster"
        lim.mkdir(parents=True)
        (lim / "BeastmasterCrucible.cs").write_text("partial class BeastmasterRotation { }")
        (rot / "Core").mkdir()
        (rot / "Core" / "CustomRotation.cs").write_text("[Rotation(\"x\")] class CustomRotation { }")
        names = lambda ps: sorted(p.name for p in ps)
        assert names(shipped_rotation_files(rot)) == ["WHM_Default.PVP.cs", "WHM_Reborn.Universal.cs", "WHM_Reborn.cs"]
        assert names(third_party_files(rot)) == ["BeirutaWHM.cs"], "a rotation without a job code is third-party"
        assert names(base_rotation_files(rot)) == ["BeastmasterCrucible.cs", "WhiteMageRotation.HitResponse.cs",
                                                     "WhiteMageRotation.cs"], "a part without [Rotation] is the base class's"
        assert not is_third_party(rot / "Core" / "CustomRotation.cs"), "only job folders hold third-party rotations"
    # the repository itself: the folders exist and every kind is found, so a further move cannot leave a reader empty
    assert CORE.is_dir() and DUTIES.is_dir(), "Rotations/Core and Rotations/Duties exist"
    assert len(job_dirs()) >= 22, f"one folder per job, {len(job_dirs())} found"
    assert any(p.name == "WHM_Reborn.cs" for p in shipped_rotation_files()), "a shipped rotation is found"
    assert any(p.name == "WhiteMageRotation.cs" for p in base_rotation_files()), "a base class is found"
    assert any(p.name.startswith("Churin") for p in third_party_files()), "a third-party rotation is found"
    assert (DATA / "DefensiveValues.g.cs").is_file() and (CONFIGS / "Configs.cs").is_file()
    print("self-test ok: shipped, base and third-party files are told apart; the repository's folders and one file of "
          "each kind are found")
    return 0


if __name__ == "__main__":
    sys.exit(self_test() if sys.argv[1:] == ["--self-test"] else 0)

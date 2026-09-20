#!/usr/bin/env python3
"""Generate the table of what each defensive action covers, from the effect texts themselves.

The owner's rule for choosing a defensive needs one figure per action: how much of an incoming hit
it takes off. That figure is written in the action's own effect text and has never been read:

  Rampart          "Reduces damage taken by 20%."
  Addle            "Lowers target's physical damage dealt by 5% and magic damage dealt by 10%."
  Radiant Aegis    "...absorbs damage totaling 20% of your maximum HP"

Three different shapes, three different meanings, and the difference decides the fight:

  * mitigation on the bearer  - takes a SHARE OF THE HIT off, whatever its size
  * mitigation on the enemy   - the same, but through the attacker, and split by damage type
  * barrier                   - absorbs a fixed number of points, expressed as a share of MAXIMUM
                                health; against a small hit it is more than enough and against a
                                large one it runs out

They are kept apart in the output for exactly that reason. A consumer that treats a 20% barrier as
a 20% mitigation is wrong in both directions depending on the size of the hit.

The generated file is checked in and CI verifies it still matches the effect texts
(check_defensive_values.py), so this is a generated artefact under supervision, not a hand-kept
list that ages silently. The hand-kept list this replaces is the 35-name MIT array in mitscan.py,
which had no way of noticing a new job action.

Usage:
    generate_defensive_values.py           write the C# file
    generate_defensive_values.py --check   report differences and exit 1 if any (used by CI)
"""

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
PROPERTIES = ROOT / "RotationSolver.SourceGenerators" / "Properties"
SOURCES = [PROPERTIES / "ActionId.resx", PROPERTIES / "DutyAction.resx"]
OUTPUT = ROOT / "RotationSolver.Basic" / "Data" / "DefensiveValues.g.cs"

# "Reduces damage taken by 20%" - the bearer takes less.
SELF_MITIGATION = re.compile(r"[Rr]educ(?:es|ing) damage taken by (\d{1,2})\s*%")
# "Reduces damage dealt by nearby enemies by 10%", "Lowers target's damage dealt by 10%"
ENEMY_MITIGATION = re.compile(r"(?:[Rr]educes|[Ll]owers)[^.]{0,60}?damage dealt[^.]{0,40}? by (\d{1,2})\s*%")
# Addle's split form, physical and magical named separately in one sentence.
SPLIT_MITIGATION = re.compile(
    r"physical damage dealt by (\d{1,2})\s*%[^.]{0,20}?and magic(?:al)? damage dealt by (\d{1,2})\s*%"
)
# "absorbs damage totaling 20% of your maximum HP"
BARRIER_SHARE = re.compile(r"absorb(?:s|ing) damage totaling (\d{1,3})\s*% of [^.]{0,30}?maximum HP")

PARA = re.compile(r"&lt;para&gt;(.*?)&lt;/para&gt;")
ASSIGNMENT = re.compile(r"^(\w+PvE)\s*=\s*(\d+),")


def unescape(text):
    return (
        text.replace("&lt;", "<")
        .replace("&gt;", ">")
        .replace("&amp;", "&")
        .replace("&quot;", '"')
        .replace("&apos;", "'")
    )


def parse(path):
    """Yield (identifier, row id, values) for every action whose effect text names a figure."""
    if not path.exists():
        return

    pending = None
    for line in path.read_text(encoding="utf-8", errors="replace").splitlines():
        para = PARA.search(line)
        if para:
            pending = unescape(para.group(1))
            continue

        assignment = ASSIGNMENT.match(line.strip())
        if not assignment:
            continue

        identifier, row = assignment.group(1), int(assignment.group(2))
        text, pending = pending, None
        if not text:
            continue

        values = extract(text)
        if values:
            yield identifier, row, values


def extract(text):
    """Pull the figures out of one effect text. Percentages become shares of 1."""
    values = {}

    split = SPLIT_MITIGATION.search(text)
    if split:
        # Two figures for one action. The lower one is what a physical hit keeps, the higher one
        # what a magical hit keeps; a consumer that cannot tell the damage type has to assume the
        # lower, so both are carried rather than averaged into a figure that is true for neither.
        values["EnemyPhysical"] = int(split.group(1)) / 100
        values["EnemyMagical"] = int(split.group(2)) / 100
    else:
        enemy = ENEMY_MITIGATION.search(text)
        if enemy:
            share = int(enemy.group(1)) / 100
            values["EnemyPhysical"] = share
            values["EnemyMagical"] = share

    own = SELF_MITIGATION.search(text)
    if own:
        values["Self"] = int(own.group(1)) / 100

    barrier = BARRIER_SHARE.search(text)
    if barrier:
        values["Barrier"] = int(barrier.group(1)) / 100

    return values


def collect():
    found = {}
    for path in SOURCES:
        for identifier, row, values in parse(path):
            # An id can appear in more than one sheet; the first sheet wins and the second is only
            # allowed to agree, so a silent disagreement cannot pass unnoticed.
            if row in found and found[row][1] != values:
                print(
                    f"warning: {identifier} ({row}) states different figures in two sheets: "
                    f"{found[row][1]} vs {values}"
                )
                continue
            found[row] = (identifier, values)
    return dict(sorted(found.items()))


def render(table):
    lines = [
        "// <auto-generated>",
        "//     Generated by .github/scripts/audit/generate_defensive_values.py from the effect",
        "//     texts in RotationSolver.SourceGenerators/Properties. Do not edit by hand - CI",
        "//     regenerates this file and fails when it no longer matches the effect texts.",
        "//",
        "//     What each figure means, because the three are NOT interchangeable:",
        "//       Self             - the bearer takes this share less of any hit",
        "//       EnemyPhysical /  - the attacker deals this share less; two figures because Addle",
        "//       EnemyMagical       and Feint split by damage type",
        "//       Barrier          - absorbs this share of the target's MAXIMUM health, in points:",
        "//                          more than enough against a small hit, spent against a big one",
        "// </auto-generated>",
        "",
        "namespace RotationSolver.Basic.Data;",
        "",
        "/// <summary>",
        "/// What one defensive action takes off an incoming hit, as stated in its own effect text.",
        "/// A value of 0 means the effect text does not state that kind of figure - never that the",
        "/// action does nothing.",
        "/// </summary>",
        "public readonly record struct DefensiveValue(",
        "\tfloat Self,",
        "\tfloat EnemyPhysical,",
        "\tfloat EnemyMagical,",
        "\tfloat Barrier);",
        "",
        # The type is public and the assembly ships a documentation file, so a missing summary is a
        # build warning (CS1591) in every consumer's build as well as ours - generated code is not
        # exempt from that.
        "/// <summary>",
        "/// The stated figures of every defensive action, keyed by action row id, generated from the",
        "/// effect texts. Read through <see cref=\"For\"/> rather than the dictionary, so an action",
        "/// the texts do not rate answers with zeroes instead of throwing.",
        "/// </summary>",
        "public static class DefensiveValues",
        "{",
        "\t/// <summary>Action row id to the figures its effect text states.</summary>",
        "\tpublic static readonly Dictionary<uint, DefensiveValue> ByActionId = new()",
        "\t{",
    ]

    largest_barrier = 0.0
    for row, (identifier, values) in table.items():
        figures = ", ".join(
            f"{values.get(key, 0.0):g}f"
            for key in ("Self", "EnemyPhysical", "EnemyMagical", "Barrier")
        )
        lines.append(f"\t\t[{row}] = new({figures}), // {identifier}")
        largest_barrier = max(largest_barrier, values.get("Barrier", 0.0))

    lines.extend(
        [
            "\t};",
            "",
            "\t/// <summary>The figures for this action, or all zeroes when it states none.</summary>",
            "\tpublic static DefensiveValue For(uint actionId)",
            "\t{",
            "\t\treturn ByActionId.TryGetValue(actionId, out var value) ? value : default;",
            "\t}",
            "",
            "\t/// <summary>",
            "\t/// The largest barrier share any action in the game states in its own effect text.",
            "\t/// This is the measure of \"a big hit\": an area action that costs at least this much of",
            "\t/// maximum health is more than the strongest single barrier can absorb.",
            "\t/// </summary>",
            "\t/// <remarks>",
            "\t/// Computed from the table rather than written down, so a patch that restates a barrier",
            "\t/// or a new job action with a larger one moves the threshold with it. The figure is",
            "\t/// regenerated from the effect texts and checked against them in CI.",
            "\t/// </remarks>",
            f"\tpublic const float LargestStatedBarrierShare = {largest_barrier:g}f;",
            "}",
            "",
        ]
    )
    return "\n".join(lines)


def self_test():
    """Drive the extraction against constructed effect texts.

    Without this a changed sheet format would produce an empty or half-filled table and nothing
    would fail - the generated file would simply stop carrying the actions it used to.
    """
    cases = [
        ("Reduces damage taken by 20%. Duration: 20s", {"Self": 0.2}),
        (
            "Lowers target's physical damage dealt by 5% and magic damage dealt by 10%.",
            {"EnemyPhysical": 0.05, "EnemyMagical": 0.1},
        ),
        (
            "Lowers target's physical damage dealt by 10% and magic damage dealt by 5%.",
            {"EnemyPhysical": 0.1, "EnemyMagical": 0.05},
        ),
        ("Reduces damage dealt by nearby enemies by 10%.", {"EnemyPhysical": 0.1, "EnemyMagical": 0.1}),
        (
            "Creates a barrier around self that absorbs damage totaling 20% of your maximum HP",
            {"Barrier": 0.2},
        ),
        (
            "Reduces damage taken by 20%. Additional Effect: Creates a barrier around self that "
            "absorbs damage totaling 10% of target's maximum HP",
            {"Self": 0.2, "Barrier": 0.1},
        ),
        # A barrier stated as potency carries no share, and must not be mistaken for one.
        ("absorbs damage equivalent to a heal of 21,000 potency", {}),
        # Healing is not mitigation.
        ("Restores own HP and the HP of all nearby party members. Cure Potency: 500", {}),
    ]

    for text, expected in cases:
        got = extract(text)
        if got != expected:
            return f"{text!r} produced {got}, expected {expected}"
    return None


def main():
    failure = self_test()
    if failure:
        print(f"self-test failed: {failure}")
        return 1
    print(
        "self-test ok: the three shapes are told apart, the split form keeps both figures, and a "
        "potency barrier as well as a heal are left out."
    )

    table = collect()
    if not table:
        print("No effect text stated a defensive figure - the parser or the sheets have changed.")
        return 1

    rendered = render(table)
    checking = "--check" in sys.argv

    if checking:
        if not OUTPUT.exists():
            print(f"{OUTPUT.relative_to(ROOT)} is missing; run generate_defensive_values.py.")
            return 1
        if OUTPUT.read_text(encoding="utf-8") != rendered:
            print(
                f"{OUTPUT.relative_to(ROOT)} no longer matches the effect texts. "
                "Run .github/scripts/audit/generate_defensive_values.py and commit the result."
            )
            return 1
        print(f"{len(table)} defensive action(s) - the generated table matches the effect texts.")
        return 0

    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT.write_text(rendered, encoding="utf-8")
    print(f"{OUTPUT.relative_to(ROOT)}: {len(table)} defensive action(s) written.")
    return 0


if __name__ == "__main__":
    sys.exit(main())

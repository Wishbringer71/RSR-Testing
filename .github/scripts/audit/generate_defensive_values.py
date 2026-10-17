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
STATUS_SHEET = PROPERTIES / "Status.resx"
TRAIT_DURATIONS = Path(__file__).resolve().with_name("trait_durations.json")
ROTATION_SOURCES = ROOT / "RotationSolver.Basic" / "Rotations"
STATUS_ASSIGNMENT = re.compile(r"^(\w+)\s*=\s*(\d+),", re.M)
MODIFY_BODY = re.compile(r"Modify(\w+PvE)\(ref ActionSetting setting\)\s*\{(.*?)\n\t\}", re.S)
PROVIDE = re.compile(r"(Target)?StatusProvide\s*=\s*([^;]+);")

# "Reduces damage taken by 20%" - the bearer takes less. The game also names the bearers between
# "taken" and the figure: "reducing damage taken by self and all party members within a radius of
# 50 yalms by 10%" (Temperance), "Reduces damage taken by a party member or self by 15%" (Aquaveil),
# "by self and nearby party members by 10%" (Kerachole). Matching only the short form left every
# party mitigation of that shape out of the table.
# The bearer can also be named ahead of "damage taken": "Reduces target party member's damage taken
# by 10%" (Intervention), "Reduces target's damage taken by 10%" (Taurochole), and the one-hit form
# "Reduces the amount of damage taken by the next attack by 10%" (Third Eye). Without these the
# table carried no figure for them, and a tankbuster that landed under one was learned as smaller
# than it is (A286, G6).
SELF_MITIGATION = re.compile(
    r"[Rr]educ(?:es|ing) (?:the amount of )?(?:target(?: party member)?'s )?damage taken by "
    r"(?:[^%.]{0,80}? by )?(\d{1,2})\s*%"
)
# "Reduces damage dealt by nearby enemies by 10%", "Lowers target's damage dealt by 10%"
ENEMY_MITIGATION = re.compile(r"(?:[Rr]educes|[Ll]owers)[^.]{0,60}?damage dealt[^.]{0,40}? by (\d{1,2})\s*%")
# A self mitigation split by damage type: "Reduces physical damage taken by 10% and magic damage
# taken by 20%" (Dark Mind). SELF_MITIGATION needs "damage taken" right after the verb and missed it,
# so Dark Mind was neither a means of the tankbuster plan nor held under an invulnerability (A266).
SELF_SPLIT_MITIGATION = re.compile(
    r"[Rr]educ(?:es|ing) physical damage taken by (\d{1,2})\s*% and magic(?:al)? damage taken by (\d{1,2})\s*%"
)
# One damage type alone: "Reduces magic damage taken by self and nearby party members by 10%"
# (Magick Barrier), "reducing magic damage taken by all nearby party members by 5%" (Fey
# Illumination). Carried as the split form with the other type at 0, so a reader that cannot tell the
# damage type takes the smaller figure, 0, and errs low (A286, G6).
SELF_TYPED_MITIGATION = re.compile(
    r"[Rr]educ(?:es|ing) (physical|magic(?:al)?) damage taken by (?:[^%.]{0,80}? by )?(\d{1,2})\s*%"
)
# "Increases maximum HP by 20% and restores the amount increased" (Thrill of Battle), "Increases
# maximum HP of a party member or self by 10%": the hit stays as large, the bearer's pool grows. For a
# plan the raise counts like a barrier of that share of the old maximum; a hit measured under it is a
# share of the RAISED maximum and has to be scaled back (concept 13).
MAX_HP_UP = re.compile(r"[Ii]ncreases maximum HP (?:of [^.%]{0,40}? )?by (\d{1,2})\s*%")
# "Block incoming attacks." (Bulwark): protects the bearer by a share the game does not state. No
# figure, but it is a defence on the user - held under an invulnerability like any other (A266).
GUARDS = re.compile(r"\bBlock incoming attacks\b")
# Addle's split form, physical and magical named separately in one sentence.
SPLIT_MITIGATION = re.compile(
    r"physical damage dealt by (\d{1,2})\s*%[^.]{0,20}?and magic(?:al)? damage dealt by (\d{1,2})\s*%"
)
# "absorbs damage totaling 20% of your maximum HP", and the other phrasing the game uses for the
# same thing, "nullifies damage totaling up to 30% of maximum HP" (Manaward). Matching only the
# first left Manaward out of the table without a trace.
# "absorbs damage equivalent to 10% of your maximum HP" (Divine Veil) and "equal to" state the same
# shape; "equivalent to a heal of N potency" and "equaling % of the amount of HP restored" do not
# name a share of maximum HP and stay out.
BARRIER_SHARE = re.compile(
    r"(?:absorb(?:s|ing)|nullif(?:ies|ying)) damage (?:totaling|equivalent to|equal to) (?:up to )?"
    r"(\d{1,3})\s*% of [^.]{0,30}?maximum HP"
)
# Whether the barrier can be put on somebody else: "a barrier around self or target party member",
# "around self and all nearby party members", "around target", "a barrier to self or target player"
# (Lost Stoneskin). Manaward's "Creates a barrier that nullifies..." and Radiant Aegis's "a barrier
# around self that..." cannot.
BARRIER_REACHES_PARTY = re.compile(
    r"barrier (?:around|to) (?:self (?:or|and) )?(?:all )?(?:target|nearby party|party)"
)

# "Diverts 25% of enmity to target party member." (Shirk) - the share of the user's enmity that moves.
# Whether a transfer makes the receiver the enemy's target depends on that figure alone: after it the
# giver holds (1 - s) of his enmity and the receiver gains s of it, so a tank swap from one side needs
# the receiver at more than 1 - 2s of the giver beforehand.
ENMITY_TRANSFER = re.compile(r"Diverts (\d{1,3})\s*% of enmity")

# "Duration: 30s" - how long the effect stands. A trait that changes the figure leaves the text
# with an empty number ("Duration: s"), and then nothing is stated and nothing is carried.
DURATION = re.compile(r"Duration: (\d{1,3})s")

# Whether a status's own text says it reduces damage or raises the pool. A status that only shares the
# action's name - Temperance's heal over time, Aquaveil's barrier, Nebula's reflection, an older form
# of the same action - does not carry the action's figure: given it, a hit under both would have the
# figure divided out twice, and a heal over time would read as the mitigation standing (A286, G6).
STATUS_REDUCES = re.compile(
    r"damage[^.]{0,40}(?:reduc|lower|decreas|diminish)|(?:reduc|lower|decreas|diminish)\w*[^.]{0,40}damage"
    r"|defense[^.]{0,60}increas|maximum hp[^.]{0,60}increas",
    re.I,
)

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
    """Yield (identifier, row id, values, duration) for every action with an effect text.

    values is empty when the text names no defensive figure; the duration is still carried, because
    a rule that waits for an effect to run out needs it whatever the effect is.
    """
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

        yield identifier, row, extract(text), duration_of(text, figure_end(text))


def figure_end(text):
    """Where the first defensive figure in the text ends, or 0 when it states none."""
    ends = [
        match.end()
        for pattern in (SPLIT_MITIGATION, ENEMY_MITIGATION, SELF_MITIGATION, SELF_SPLIT_MITIGATION,
                        SELF_TYPED_MITIGATION, BARRIER_SHARE, MAX_HP_UP, GUARDS)
        for match in [pattern.search(text)]
        if match
    ]
    return min(ends) if ends else 0


def duration_of(text, start=0):
    """The duration the effect text states for its figure, in seconds, or None when none is stated.

    A text with several effects states several durations - Collective Unconscious gives its ring 18s
    and the mitigation it applies 10s. The duration that belongs to the defensive figure is the first
    one stated after it; a text without a figure, or with no duration after it, falls back to the
    first duration anywhere.
    """
    match = DURATION.search(text, start) or DURATION.search(text)
    return int(match.group(1)) if match else None


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

    own_split = SELF_SPLIT_MITIGATION.search(text)
    if own_split:
        values["SelfPhysical"] = int(own_split.group(1)) / 100
        values["SelfMagical"] = int(own_split.group(2)) / 100
    else:
        typed = SELF_TYPED_MITIGATION.search(text)
        if typed:
            physical = typed.group(1) == "physical"
            values["SelfPhysical"] = int(typed.group(2)) / 100 if physical else 0.0
            values["SelfMagical"] = 0.0 if physical else int(typed.group(2)) / 100

    raised = MAX_HP_UP.search(text)
    if raised:
        values["MaxHp"] = int(raised.group(1)) / 100

    if GUARDS.search(text):
        values["Guards"] = True

    barrier = BARRIER_SHARE.search(text)
    if barrier:
        values["Barrier"] = int(barrier.group(1)) / 100
        if BARRIER_REACHES_PARTY.search(text):
            values["BarrierReachesParty"] = True

    return values


def transfers():
    """Action row id to (identifier, share of enmity the effect text says it moves)."""
    found = {}
    for path in SOURCES:
        if not path.exists():
            continue
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
            match = ENMITY_TRANSFER.search(text or "")
            if match and row not in found:
                found[row] = (identifier, int(match.group(1)) / 100)
    return dict(sorted(found.items()))


def status_ids():
    """Status identifier (as the generator names it from the sheet) to row id."""
    if not STATUS_SHEET.exists():
        return {}
    return {m.group(1): int(m.group(2)) for m in STATUS_ASSIGNMENT.finditer(STATUS_SHEET.read_text(encoding="utf-8"))}


def provided_statuses():
    """Action identifier to the statuses its Modify method names directly - small lists only.

    A list that goes through a StatusHelper group (Rampart's is every big mitigation, to keep them
    from overlapping) says what blocks the action, not what it grants, and is left out.
    """
    provided = {}
    if not ROTATION_SOURCES.exists():
        return provided
    for path in sorted(ROTATION_SOURCES.rglob("*.cs")):
        text = path.read_text(encoding="utf-8-sig", errors="replace")
        for match in MODIFY_BODY.finditer(text):
            identifier, body = match.group(1), match.group(2)
            for provide in PROVIDE.finditer(body):
                expression = provide.group(2)
                if "StatusHelper." in expression:
                    continue
                names = re.findall(r"StatusID\.(\w+)", expression)
                if 0 < len(names) <= 2:
                    provided.setdefault(identifier, names)
    return provided


def status_mitigation(table):
    """Status row id to (identifier, action identifier, values) for every rated action's status.

    The status the action puts on its bearer or on the enemy is found by name first - the generator
    names a status and an action from the same English sheet name (RampartPvE, Rampart, Rampart_1191)
    - and, where the names differ, from the statuses the action's own Modify method names (Dismantle
    puts Dismantled on the enemy). Barriers are left out: a barrier absorbs points, it does not scale
    the hit. A status two actions claim keeps the first action's figures in row order.
    """
    ids = status_ids()
    texts = status_texts()
    provided = provided_statuses()
    result = {}
    for row, (identifier, values) in table.items():
        figures = {key: values.get(key, 0.0)
                   for key in ("Self", "EnemyPhysical", "EnemyMagical", "SelfPhysical", "SelfMagical", "MaxHp")}
        if not any(figures.values()):
            continue
        base = identifier[:-3] if identifier.endswith("PvE") else identifier
        names = sorted(n for n in ids if n == base or n.startswith(base + "_"))
        if not names:
            names = [n for n in provided.get(identifier, []) if n in ids]
        for name in names:
            if STATUS_REDUCES.search(texts.get(ids[name], "")):
                result.setdefault(ids[name], (name, identifier, figures))
    return dict(sorted(result.items()))


def trait_durations(identifiers):
    """Row id to (identifier, seconds, level) for every action a trait extends - only where the effect
    text leaves the number blank. The trait's own text names the action ("Extends the duration of
    Reprisal to 15 seconds"); fetch_trait_durations.py reads them from the game data."""
    if not TRAIT_DURATIONS.exists():
        return {}
    import json
    found = {}
    for trait in json.loads(TRAIT_DURATIONS.read_text(encoding="utf-8"))["traits"]:
        identifier = re.sub(r"\W", "", trait["action"]) + "PvE"
        for row, known in identifiers.items():
            if known == identifier:
                current = found.get(row)
                # a later trait for the same action supersedes an earlier one from its level on
                if current is None or trait["level"] > current[2]:
                    found[row] = (identifier, trait["seconds"], trait["level"])
    return dict(sorted(found.items()))


def trait_values(identifiers):
    """Row id to (identifier, share, level) for every action whose damage reduction a trait raises and
    whose effect text leaves the figure blank - "Improves Troubadour's damage reduction effect to 15%"
    (fetch_trait_durations.py, "values"). A later trait for the same action supersedes an earlier one."""
    if not TRAIT_DURATIONS.exists():
        return {}
    import json
    found = {}
    for trait in json.loads(TRAIT_DURATIONS.read_text(encoding="utf-8")).get("values", []):
        identifier = re.sub(r"\W", "", trait["action"]) + "PvE"
        for row, known in identifiers.items():
            if known == identifier:
                current = found.get(row)
                if current is None or trait["level"] > current[2]:
                    found[row] = (identifier, trait["percent"] / 100, trait["level"])
    return dict(sorted(found.items()))


def trait_value_statuses(values):
    """Status row id to (status name, action row, action identifier) for the actions trait_values()
    rates, found by name as in status_mitigation()."""
    ids = status_ids()
    texts = status_texts()
    result = {}
    for row, (identifier, _, _) in values.items():
        base = identifier[:-3]
        for name in sorted(n for n in ids if n == base or n.startswith(base + "_")):
            # A status of the same name that does not reduce damage is another effect of an older
            # form (Tactician 1197 regenerates TP) and must not carry the share.
            if STATUS_REDUCES.search(texts.get(ids[name], "")):
                result.setdefault(ids[name], (name, row, identifier))
    return dict(sorted(result.items()))


def status_texts():
    """Status row id to its effect text from Status.resx."""
    if not STATUS_SHEET.exists():
        return {}
    texts, pending = {}, None
    for line in STATUS_SHEET.read_text(encoding="utf-8", errors="replace").splitlines():
        para = PARA.search(line)
        if para:
            pending = unescape(para.group(1))
            continue
        assignment = STATUS_ASSIGNMENT.match(line.strip())
        if assignment:
            texts[int(assignment.group(2))] = pending or ""
            pending = None
    return texts


def collect():
    found = {}
    durations = {}
    collect.identifiers = {}
    for path in SOURCES:
        for identifier, row, values, duration in parse(path):
            collect.identifiers.setdefault(row, identifier)
            if duration is not None and row not in durations:
                durations[row] = (identifier, duration)
            if not values:
                continue
            # An id can appear in more than one sheet; the first sheet wins and the second is only
            # allowed to agree, so a silent disagreement cannot pass unnoticed.
            if row in found and found[row][1] != values:
                print(
                    f"warning: {identifier} ({row}) states different figures in two sheets: "
                    f"{found[row][1]} vs {values}"
                )
                continue
            found[row] = (identifier, values)
    return dict(sorted(found.items())), dict(sorted(durations.items()))


def extras(values):
    """The non-positional figures as an object initializer, or nothing when the action has none."""
    parts = [f"{key} = {values[key]:g}f" for key in ("SelfPhysical", "SelfMagical", "MaxHp") if values.get(key)]
    if values.get("Guards"):
        parts.append("Guards = true")
    return " { " + ", ".join(parts) + " }" if parts else ""


def render(table, durations, enmity=None, statuses=None, traits=None, trait_shares=None):
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
        "\tfloat Barrier)",
        "{",
        "\t/// <summary>The bearer takes this share less of a physical hit only (Dark Mind).</summary>",
        "\tpublic float SelfPhysical { get; init; }",
        "",
        "\t/// <summary>The bearer takes this share less of a magical hit only (Dark Mind).</summary>",
        "\tpublic float SelfMagical { get; init; }",
        "",
        "\t/// <summary>",
        "\t/// The bearer's maximum HP rises by this share (Thrill of Battle). The hit stays as large, so",
        "\t/// against it the raise works like a barrier of this share of the old maximum.",
        "\t/// </summary>",
        "\tpublic float MaxHp { get; init; }",
        "",
        "\t/// <summary>Protects the bearer by an unstated share - blocking (Bulwark).</summary>",
        "\tpublic bool Guards { get; init; }",
        "",
        "\t/// <summary>",
        "\t/// The share the bearer takes less of a hit of <paramref name=\"attackType\"/> (the action",
        "\t/// sheet's AttackType): <see cref=\"Self\"/> together with the split figure for the type;",
        "\t/// neither physical nor magical counts the smaller split figure.",
        "\t/// </summary>",
        "\tpublic float SelfAgainst(uint attackType)",
        "\t{",
        "\t\tvar split = attackType is >= (uint)AttackType.Slashing and <= (uint)AttackType.Shot ? SelfPhysical",
        "\t\t\t: attackType == (uint)AttackType.Magic ? SelfMagical",
        "\t\t\t: Math.Min(SelfPhysical, SelfMagical);",
        "\t\treturn 1f - ((1f - Self) * (1f - split));",
        "\t}",
        "",
        "\t/// <summary>Whether it protects its bearer at all: mitigation, barrier, raised maximum HP or block.</summary>",
        "\tpublic bool ProtectsBearer => Self > 0f || SelfPhysical > 0f || SelfMagical > 0f || Barrier > 0f || MaxHp > 0f || Guards;",
        "}",
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
        lines.append(f"\t\t[{row}] = new({figures}){extras(values)}, // {identifier}")
        # The owner's measure for a big area hit is "above a large shield": more than a large
        # shield on the member being hit would absorb. A barrier only its caster can carry
        # (Manaward, 30%) answers that for nobody else, so it does not set the measure.
        if values.get("BarrierReachesParty"):
            largest_barrier = max(largest_barrier, values["Barrier"])

    lines.extend(
        [
            "\t};",
            "",
            "\t/// <summary>",
            "\t/// The figures for this action, or all zeroes when it states none. Where a trait raises a",
            "\t/// damage reduction the effect text leaves blank (Troubadour), the trait's share once the",
            "\t/// player's synced level has it (<see cref=\"TraitShareOf\"/>).",
            "\t/// </summary>",
            "\tpublic static DefensiveValue For(uint actionId)",
            "\t{",
            "\t\tif (ByActionId.TryGetValue(actionId, out var value))",
            "\t\t{",
            "\t\t\treturn value;",
            "\t\t}",
            "",
            "\t\tvar share = TraitShareOf(actionId);",
            "\t\treturn share > 0f ? new(share, 0f, 0f, 0f) : default;",
            "\t}",
            "",
            "\t/// <summary>",
            "\t/// The largest barrier share an action states in its own effect text, among barriers",
            "\t/// that can be put on another party member. This is the measure of \"a big hit\": an",
            "\t/// area action that costs at least this much of maximum health is more than a large",
            "\t/// shield on the member hit can absorb. A barrier only its caster carries is left out.",
            "\t/// </summary>",
            "\t/// <remarks>",
            "\t/// Computed from the table rather than written down, so a patch that restates a barrier",
            "\t/// or a new job action with a larger one moves the threshold with it. The figure is",
            "\t/// regenerated from the effect texts and checked against them in CI.",
            "\t/// </remarks>",
            f"\tpublic const float LargestStatedBarrierShare = {largest_barrier:g}f;",
            "",
            "\t/// <summary>",
            "\t/// How long each action's effect stands, in seconds - the first duration its effect text",
            "\t/// states. Every action with a stated duration, not only the rated ones: a rule that waits",
            "\t/// for an effect to run out needs the figure whatever the effect is. Missing where the text",
            "\t/// states no number - a trait that changes the duration leaves the text blank - so a",
            "\t/// caller never reads a figure the game did not give.",
            "\t/// </summary>",
            "\tpublic static readonly Dictionary<uint, float> DurationByActionId = new()",
            "\t{",
        ]
    )
    for row, (identifier, seconds) in durations.items():
        lines.append(f"\t\t[{row}] = {seconds}f, // {identifier}")
    lines.extend(
        [
            "\t};",
            "",
            "\t/// <summary>",
            "\t/// Durations a trait sets where the effect text leaves the number blank (\"Duration: s\"), with the",
            "\t/// level the trait is learned at - the trait's own text (\"Extends the duration of Reprisal to",
            "\t/// 15 seconds\"), fetched from the game data (fetch_trait_durations.py). The second source: in the",
            "\t/// game the trait is read from the client itself (<see cref=\"TraitDurations\"/>), which weighs",
            "\t/// more. Below the trait's level the duration is not stated anywhere, so it stays unknown.",
            "\t/// </summary>",
            "\tpublic static readonly Dictionary<uint, (float Seconds, int Level)> TraitDurationByActionId = new()",
            "\t{",
        ]
    )
    for row, (identifier, seconds, level) in (traits or {}).items():
        if row not in durations:
            lines.append(f"\t\t[{row}] = ({seconds}f, {level}), // {identifier}")
    lines.extend(
        [
            "\t};",
            "",
            "\t/// <summary>",
            "\t/// The stated duration in seconds: from the effect text, or - where a trait sets it and the text",
            "\t/// leaves the number blank - from that trait once the player's synced level has it, read in the",
            "\t/// game first and from <see cref=\"TraitDurationByActionId\"/> where the game gives nothing. Below",
            "\t/// the trait's level, the duration the action's own tooltip states at that level, read in the game",
            "\t/// (<see cref=\"TraitDurations.TryBelowTrait\"/>, A316); 0 when none states one.",
            "\t/// </summary>",
            "\tpublic static float DurationOf(uint actionId)",
            "\t{",
            "\t\tif (DurationByActionId.TryGetValue(actionId, out var seconds))",
            "\t\t{",
            "\t\t\treturn seconds;",
            "\t\t}",
            "",
            "\t\tif (!TraitDurations.TryInGame(actionId, out var trait) && !TraitDurationByActionId.TryGetValue(actionId, out trait))",
            "\t\t{",
            "\t\t\treturn 0f;",
            "\t\t}",
            "",
            "\t\treturn DataCenter.PlayerSyncedLevel() >= trait.Level",
            "\t\t\t? trait.Seconds",
            "\t\t\t: TraitDurations.TryBelowTrait(actionId, trait.Seconds, out var stated) ? stated : 0f;",
            "\t}",
            "",
            "\t/// <summary>",
            "\t/// Damage reductions a trait raises where the effect text leaves the figure blank (\"by %\"), with",
            "\t/// the level the trait is learned at - \"Improves Troubadour's damage reduction effect to 15%\"",
            "\t/// (fetch_trait_durations.py). Read in the game first (<see cref=\"TraitDurations\"/>); below the",
            "\t/// trait's level the figure is not stated anywhere and stays unknown (A286, G6).",
            "\t/// </summary>",
            "\tpublic static readonly Dictionary<uint, (float Share, int Level)> TraitMitigationByActionId = new()",
            "\t{",
            "\t\t@@TRAIT_VALUES@@",
            "\t};",
            "",
            "\t/// <summary>The statuses the actions of <see cref=\"TraitMitigationByActionId\"/> put on their bearers.</summary>",
            "\tpublic static readonly Dictionary<uint, uint> TraitMitigatingActionByStatusId = new()",
            "\t{",
            "\t\t@@TRAIT_STATUSES@@",
            "\t};",
            "",
            "\t/// <summary>",
            "\t/// The damage reduction a trait sets for this action once the player's synced level has it: read",
            "\t/// in the game first, from <see cref=\"TraitMitigationByActionId\"/> where the game gives nothing;",
            "\t/// 0 when no trait states one.",
            "\t/// </summary>",
            "\tpublic static float TraitShareOf(uint actionId)",
            "\t{",
            "\t\treturn (TraitDurations.TryShareInGame(actionId, out var trait) || TraitMitigationByActionId.TryGetValue(actionId, out trait))",
            "\t\t\t&& DataCenter.PlayerSyncedLevel() >= trait.Level",
            "\t\t\t? trait.Share",
            "\t\t\t: 0f;",
            "\t}",
            "",
            "\t/// <summary>",
            "\t/// What a status takes off a hit: the figures of <see cref=\"MitigationByStatusId\"/>, or for a",
            "\t/// status whose action a trait rates (Troubadour), the trait's share. False when neither states one.",
            "\t/// </summary>",
            "\tpublic static bool TryStatus(uint statusId, out DefensiveValue value)",
            "\t{",
            "\t\tif (MitigationByStatusId.TryGetValue(statusId, out value))",
            "\t\t{",
            "\t\t\treturn true;",
            "\t\t}",
            "",
            "\t\tvar share = TraitMitigatingActionByStatusId.TryGetValue(statusId, out var actionId) ? TraitShareOf(actionId) : 0f;",
            "\t\tvalue = share > 0f ? new(share, 0f, 0f, 0f) : default;",
            "\t\treturn share > 0f;",
            "\t}",
            "",
            "\t/// <summary>",
            "\t/// The share of the user's enmity an action moves to its target, as its effect text states",
            "\t/// it (\"Diverts 25% of enmity to target party member.\"). After the transfer the user keeps",
            "\t/// 1 - s and the receiver gains s, so the receiver becomes the enemy's target from one side",
            "\t/// alone only if it held more than 1 - 2s of the user's enmity beforehand.",
            "\t/// </summary>",
            "\tpublic static readonly Dictionary<uint, float> EnmityTransferByActionId = new()",
            "\t{",
        ]
    )
    for row, (identifier, share) in (enmity or {}).items():
        lines.append(f"\t\t[{row}] = {share:g}f, // {identifier}")
    lines.extend(
        [
            "\t};",
            "",
            "\t/// <summary>The stated enmity share moved, or 0 when the effect text states none.</summary>",
            "\tpublic static float EnmityTransferOf(uint actionId)",
            "\t{",
            "\t\treturn EnmityTransferByActionId.TryGetValue(actionId, out var share) ? share : 0f;",
            "\t}",
            "",
            "\t/// <summary>",
            "\t/// What a status takes off a hit, from the effect text of the action that puts it there:",
            "\t/// Self on the one hit, the Enemy figures on the attacker (split by damage type). Lets a",
            "\t/// measured hit be scaled back to what it would have done unmitigated, whoever's",
            "\t/// mitigation it was. Barriers are not here: they absorb points rather than scale a hit.",
            "\t/// </summary>",
            "\tpublic static readonly Dictionary<uint, DefensiveValue> MitigationByStatusId = new()",
            "\t{",
        ]
    )
    for row, (name, identifier, figures) in (statuses or {}).items():
        values = ", ".join(f"{figures[key]:g}f" for key in ("Self", "EnemyPhysical", "EnemyMagical"))
        lines.append(f"\t\t[{row}] = new({values}, 0f){extras(figures)}, // {name} from {identifier}")

    # The same pairs the other way round: which statuses mean an action's mitigation already stands, so
    # a reader that asks "what could still be added" does not count it twice.
    by_action = {}
    identifiers = {identifier: row for row, (identifier, _) in table.items()}
    for status_row, (name, identifier, _) in (statuses or {}).items():
        by_action.setdefault(identifiers[identifier], (identifier, []))[1].append(status_row)
    lines.extend(
        [
            "\t};",
            "",
            "\t/// <summary>",
            "\t/// The statuses each rated action's mitigation stands as - the pairs of",
            "\t/// <see cref=\"MitigationByStatusId\"/> keyed by the action instead.",
            "\t/// </summary>",
            "\tpublic static readonly Dictionary<uint, uint[]> MitigatingStatusesByActionId = new()",
            "\t{",
        ]
    )
    for row in sorted(by_action):
        identifier, rows = by_action[row]
        lines.append(f"\t\t[{row}] = [{', '.join(str(r) for r in sorted(rows))}], // {identifier}")
    lines.extend(
        [
            "\t};",
            "}",
            "",
        ]
    )
    value_lines = [f"\t\t[{row}] = ({share:g}f, {level}), // {identifier}"
                   for row, (identifier, share, level) in (trait_shares or {}).items()]
    status_lines = [f"\t\t[{status}] = {row}, // {name} from {identifier}"
                    for status, (name, row, identifier) in trait_value_statuses(trait_shares or {}).items()]
    text = "\n".join(lines)
    text = text.replace("\t\t@@TRAIT_VALUES@@\n", "".join(line + "\n" for line in value_lines))
    text = text.replace("\t\t@@TRAIT_STATUSES@@\n", "".join(line + "\n" for line in status_lines))
    return text


def self_test():
    """Drive the extraction against constructed effect texts.

    Without this a changed sheet format would produce an empty or half-filled table and nothing
    would fail - the generated file would simply stop carrying the actions it used to.
    """
    cases = [
        ("Reduces damage taken by 20%. Duration: 20s", {"Self": 0.2}),
        # The bearers named between "taken" and the figure.
        (
            "Increases healing magic potency by 20% while reducing damage taken by self and all party "
            "members within a radius of 50 yalms by 10%. Duration: 20s",
            {"Self": 0.1},
        ),
        ("Reduces damage taken by a party member or self by 15%. Duration: 8s", {"Self": 0.15}),
        # A figure in the next sentence is not this one's.
        ("Reduces damage taken by the target. Heals by 10% of maximum HP.", {}),
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
        (
            "Creates a barrier around self or target party member that absorbs damage totaling 25% "
            "of target's maximum HP.",
            {"Barrier": 0.25, "BarrierReachesParty": True},
        ),
        (
            "Creates a barrier around self and all nearby party members that absorbs damage "
            "equivalent to 10% of your maximum HP.",
            {"Barrier": 0.1, "BarrierReachesParty": True},
        ),
        (
            "Applies a barrier to self or target player that absorbs damage totaling 15% of target's "
            "maximum HP",
            {"Barrier": 0.15, "BarrierReachesParty": True},
        ),
        (
            "Creates a barrier around self that absorbs damage totaling 20% of maximum HP. Can only "
            "be executed on a target party member",
            {"Barrier": 0.2},
        ),
        # The second phrasing, and a barrier only its caster carries.
        (
            "Creates a barrier that nullifies damage totaling up to 30% of maximum HP. Duration: 20s",
            {"Barrier": 0.3},
        ),
        # Dark Mind: a self mitigation split by damage type (A266).
        (
            "Reduces physical damage taken by 10% and magic damage taken by 20%. Duration: 10s",
            {"SelfPhysical": 0.1, "SelfMagical": 0.2},
        ),
        # The bearer named ahead of "damage taken", and the one-hit form (A286, G6).
        (
            "Reduces target party member's damage taken by 10%. Duration: 8s Additional Effect: Increases "
            "damage reduction by an additional 10% if Rampart or are active",
            {"Self": 0.1},
        ),
        ("Reduces target's damage taken by 10%. Duration: 15s", {"Self": 0.1}),
        ("Reduces the amount of damage taken by the next attack by 10%. Duration: 4s", {"Self": 0.1}),
        # One damage type alone; the other stays 0.
        (
            "Reduces magic damage taken by self and nearby party members by 10%. Duration: 10s",
            {"SelfPhysical": 0.0, "SelfMagical": 0.1},
        ),
        (
            "Summons a dragon, reducing magic damage taken by all nearby party members by 5%.",
            {"SelfPhysical": 0.0, "SelfMagical": 0.05},
        ),
        ("Reduces physical damage taken by 22%. Duration: 30m", {"SelfPhysical": 0.22, "SelfMagical": 0.0}),
        # A raise of damage taken is not a mitigation.
        ("Reduces damage dealt by 10% and increases damage taken by 5%.", {"EnemyPhysical": 0.1, "EnemyMagical": 0.1}),
        # Thrill of Battle and a raise on a party member: the pool grows, the hit does not shrink.
        (
            "Increases maximum HP by 20% and restores the amount increased. Additional Effect: Increases HP "
            "recovery via healing actions on self by 20% Duration: 10s",
            {"MaxHp": 0.2},
        ),
        ("Increases maximum HP of a party member or self by 10% and restores the amount increased.", {"MaxHp": 0.1}),
        # Bulwark: a defence on the bearer without a figure.
        ("Block incoming attacks. Duration: 10s", {"Guards": True}),
        # "reduced" maximum HP is no raise.
        ("Evasion is increased while maximum HP is reduced.", {}),
        # A barrier stated as potency carries no share, and must not be mistaken for one.
        ("absorbs damage equivalent to a heal of 21,000 potency", {}),
        # Healing is not mitigation.
        ("Restores own HP and the HP of all nearby party members. Cure Potency: 500", {}),
    ]

    for text, expected in cases:
        got = extract(text)
        if got != expected:
            return f"{text!r} produced {got}, expected {expected}"

    # The duration that belongs to the figure, not the first one in the text.
    ring = (
        "Creates a celestial ring 8 yalms around the caster. Duration: 18s Additional Effect: Reduces "
        "damage taken by 10% for self and any party members within 30 yalms Duration: 10s"
    )
    if duration_of(ring, figure_end(ring)) != 10:
        return f"duration of the figure in {ring!r} read as {duration_of(ring, figure_end(ring))}, expected 10"
    blossom = "Places a healing blossom at the designated location. Duration: 20s"
    if duration_of(blossom, figure_end(blossom)) != 20:
        return f"duration of {blossom!r} without a figure read wrong, expected 20"

    # A blank number is a trait override and must read as "not stated", not as a duration.
    for text, expected in (
        ("absorbs damage totaling 20% of your maximum HP Duration: 30s", 30),
        ("Lowers target's physical damage dealt by 5% and magic damage dealt by 10%. Duration: s", None),
        ("Reduces damage taken by 20%.", None),
    ):
        got = duration_of(text)
        if got != expected:
            return f"duration of {text!r} read as {got}, expected {expected}"

    # A provide list through a StatusHelper group says what blocks the action, not what it grants.
    for body, expected in (
        ("setting.TargetStatusProvide = [StatusID.Dismantled];", ["Dismantled"]),
        ("setting.StatusProvide = StatusHelper.RampartStatus;", None),
    ):
        provide = PROVIDE.search(body)
        expression = provide.group(2)
        got = None if "StatusHelper." in expression else re.findall(r"StatusID\.(\w+)", expression)
        if got != expected:
            return f"provided statuses of {body!r} read as {got}, expected {expected}"

    # The enmity transfer: Shirk's sentence is read, a text about enmity generation is not.
    for text, expected in (
        ("Diverts 25% of enmity to target party member.", "25"),
        ("Significantly increases enmity generation. Effect ends upon reuse.", None),
    ):
        match = ENMITY_TRANSFER.search(text)
        got = match.group(1) if match else None
        if got != expected:
            return f"enmity transfer of {text!r} read as {got}, expected {expected}"

    # A status carries its action's figure only when its own text reduces damage or raises the pool
    # (A286, G6): the texts below are the ones the game gives, the first five reduce, the rest do not.
    for text, expected in (
        ("Damage taken is reduced.", True),
        ("Physical and magic damage are reduced.", True),
        ("Magic defense and healing magic potency are increased.", True),
        ("Sins are confessed partially diminishing damage accrued.", True),
        ("Maximum HP and HP recovery via healing actions are increased.", True),
        ("Regenerating HP over time.", False),
        ("A magicked barrier is nullifying damage.", False),
        ("Inflicting a portion of sustained damage back to its source.", False),
        ("Damage dealt is increased.", False),
        ("Sustaining increased damage from target who executed Feint.", False),
    ):
        if bool(STATUS_REDUCES.search(text)) != expected:
            return f"status text {text!r} read as {'reducing' if not expected else 'not reducing'}"
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

    table, durations = collect()
    if not table:
        print("No effect text stated a defensive figure - the parser or the sheets have changed.")
        return 1

    enmity = transfers()
    if not enmity:
        print("No effect text states an enmity transfer - Shirk's sentence or the sheets have changed.")
        return 1

    statuses = status_mitigation(table)
    if not statuses:
        print("No status could be tied to a rated action - the status sheet or the names have changed.")
        return 1

    traits = trait_durations(collect.identifiers)
    if TRAIT_DURATIONS.exists() and not traits:
        print("No trait in trait_durations.json names an action of the sheets - the names have changed.")
        return 1

    trait_shares = trait_values(collect.identifiers)
    if TRAIT_DURATIONS.exists() and not trait_shares:
        print("No reduction trait in trait_durations.json names an action of the sheets - the names have changed.")
        return 1
    rendered = render(table, durations, enmity, statuses, traits, trait_shares)
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

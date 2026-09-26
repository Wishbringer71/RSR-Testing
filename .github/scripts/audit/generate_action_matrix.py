#!/usr/bin/env python3
"""Generate the dependency matrix of every job's actions, and which of them the rotation uses.

The owner's request (concept 14): for each job, how its spells, abilities and weaponskills depend on
each other, and whether any of them is never used. Two layers are read and kept apart, because they
answer different questions and a disagreement between them is itself a finding:

  game  - what the effect texts state (RotationSolver.SourceGenerators/Properties/ActionId.resx and
          the trait texts in Rotation.resx): combo predecessor, "X changes to Y", shared recast,
          a status the action needs, a status or resource it grants, what it costs, trait upgrades.
  RSR   - what the code does with them: the base job rotation's settings (StatusNeed, StatusProvide,
          ComboIds, ActionCheck) and the job's default rotation (every `X.CanUse(` and the other
          actions its condition reads).

Actions are placed on the owner's levels (CLAUDE.md, "Universell zuerst"): all jobs, the role groups,
damage dealers as a whole, their three kinds, the melee pairs, and only then the job.

What this cannot see, and says so in its output rather than guessing:
  * levels - the sheets carry none, so a use that only fails under level sync is not detectable;
  * conditions spread over nested ifs or helper methods - the rule edges are read per condition;
  * relations the effect texts do not name (gauge producers live largely in trait texts or nowhere).

Usage:
    generate_action_matrix.py          write docs/action-matrix/
    generate_action_matrix.py --check  exit 1 when the written files are out of date
"""

import csv
import functools
import html
import io
import re
import sys
from datetime import date
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
PROPERTIES = ROOT / "RotationSolver.SourceGenerators" / "Properties"
BASIC = ROOT / "RotationSolver.Basic" / "Rotations" / "Basic"
CENTRAL = ROOT / "RotationSolver.Basic" / "Rotations"
REBORN = ROOT / "RotationSolver" / "RebornRotations"
OUTPUT = ROOT / "docs" / "action-matrix"

# The owner's levels. Job codes as the effect texts print them; a class code (GLA, CNJ, ...) counts
# for the job it grows into.
CLASS_OF = {
    "GLA": "PLD", "MRD": "WAR", "PGL": "MNK", "LNC": "DRG", "ARC": "BRD", "CNJ": "WHM",
    "THM": "BLM", "ACN": "SMN", "ROG": "NIN",
}
GROUPS = [
    ("alle", ["PLD", "WAR", "DRK", "GNB", "WHM", "SCH", "AST", "SGE", "MNK", "DRG", "NIN", "SAM",
              "RPR", "VPR", "BRD", "MCH", "DNC", "BLM", "SMN", "RDM", "PCT"]),
    ("Tanks", ["PLD", "WAR", "DRK", "GNB"]),
    ("Heiler", ["WHM", "SCH", "AST", "SGE"]),
    ("Damage Dealer", ["MNK", "DRG", "NIN", "SAM", "RPR", "VPR", "BRD", "MCH", "DNC", "BLM", "SMN",
                       "RDM", "PCT"]),
    ("Nahkämpfer", ["MNK", "DRG", "NIN", "SAM", "RPR", "VPR"]),
    ("Fernkämpfer", ["BRD", "MCH", "DNC"]),
    ("Magier", ["BLM", "SMN", "RDM", "PCT"]),
    ("Monk und Samurai", ["MNK", "SAM"]),
    ("Dragoon und Schnitter", ["DRG", "RPR"]),
    ("Ninja und Viper", ["NIN", "VPR"]),
]
LIMITED = {"BLU", "BST"}

ROTATION_FILES = {
    "PLD": ["Tank/PLD_Reborn.cs"], "WAR": ["Tank/WAR_Reborn.cs"], "DRK": ["Tank/DRK_Reborn.cs"],
    "GNB": ["Tank/GNB_Reborn.cs"], "WHM": ["Healer/WHM_Reborn.cs"], "SCH": ["Healer/SCH_Reborn.cs"],
    "AST": ["Healer/AST_Reborn.cs"], "SGE": ["Healer/SGE_Reborn.cs"], "MNK": ["Melee/MNK_Reborn.cs"],
    "DRG": ["Melee/DRG_Reborn.cs"], "NIN": ["Melee/NIN_Reborn.cs"], "SAM": ["Melee/SAM_Reborn.cs"],
    "RPR": ["Melee/RPR_Reborn.cs"], "VPR": ["Melee/VPR_Reborn.cs"], "BRD": ["Ranged/BRD_Reborn.cs"],
    "MCH": ["Ranged/MCH_Reborn.cs"], "DNC": ["Ranged/DNC_Reborn.cs"],
    "BLM": ["Magical/BLM_Default.cs", "Magical/BLM_RP.cs"], "SMN": ["Magical/SMN_Reborn.cs"],
    "RDM": ["Magical/RDM_Reborn.cs"], "PCT": ["Magical/PCT_Reborn.cs"],
    "BLU": ["Limited Jobs/BLU_Reborn.cs"], "BST": ["Limited Jobs/BST_Reborn.cs"],
}

ENTRY = re.compile(
    r"<strong>(.*?)</strong></see> <i>(PvE|PvP)</i> \((.*?)\) \[(\d+)\] \[([^\]]*)\]\s*\n(.*?)\n(\w+) = (\d+),",
    re.S,
)
JOB_CLASS = re.compile(r"\[Jobs\((.*?)\)\]\s*public abstract partial class (\w+)")
PROPERTY = re.compile(r"public IBaseAction (\w+PvE(?:_\d+)?) =>")
TRAIT = re.compile(
    r"<strong>([^<]*)</strong></see> \((\w+)\) \[\d+\]\s*\n\s*/// <para>([^\n]*?)</para>\s*\n\s*/// </summary>\s*\n"
    r"\s*public static IBaseTrait (\w+)",
    re.S,
)

# Relation phrases in the effect texts.
# The name may be empty: a trait-dependent predecessor is left out of the text, like a potency.
COMBO = re.compile(
    r"Combo Action:\s*(.*?)(?=\s*(?:Combo Potency|Combo Bonus|Additional Effect|Can only|※|Balance Gauge)|\s*$)"
)
CHANGES = re.compile(r"※([^※]+?) (?:changes|change) to ([^※]+?)(?= when| while| upon| after| if|\.|$)")
SHARES = re.compile(r"Shares a recast timer with ([^.]+?)(?:\.|$)")
NEEDS = re.compile(
    r"Can only be executed (?:while|when) (?:under the effect of |in )?([^.※]+?)(?:\.|※|$)"
)
NEEDS_ACTIVE = re.compile(r"Can only be executed while ([A-Z][^.※]+?) is active")
GRANTS = re.compile(
    r"[Gg]rants (?:the effect of |\d+ stacks? of |an? )?([A-Z][\w'’-]*(?: [A-Z][\w'’-]*)*)"
)
# Words that open the next field of an effect text, never part of a status name.
FIELD_WORDS = {"Duration", "Additional", "Effect", "Can", "Cure", "Potency", "Maximum", "Combo",
               "Shares", "This", "Increases", "Reduces", "Restores", "Deals", "Grants", "Stack",
               "Stacks"}
# A trait that grants a status when an action is executed: the action is the producer.
GRANTS_AFTER = re.compile(r"[Gg]rants the effect of ([A-Z][\w'’ -]*?) after executing ([A-Z][\w'’ -]*?)(?:\.| Duration)")
UPGRADE = re.compile(r"[Uu]pgrades (.+?) to (.+?)(?: respectively| when|\.|$)")
# A property of the base rotation that names one action, cast by the central dispatch through the
# property: TankStance => GritPvE, Raise => AscendPvE.
PROPERTY_ALIAS = re.compile(r"IBaseAction\??\s+(\w+)\s*=>\s*(\w+PvE(?:_\d+)?)\s*;")
ALIAS_CANUSE = re.compile(r"\b(\w+)\??\s*\.\s*CanUse\s*\(")

CODE_COMMENT = re.compile(r"//.*?$|/\*.*?\*/", re.S | re.M)
CODE_STRING = re.compile(r'\$?@?"(?:[^"\\]|\\.)*"')
CANUSE = re.compile(r"\b(\w+PvE(?:_\d+)?)\s*\.\s*CanUse\s*\(")
# A cast: CanUse with a real out target (out act, out var act, out IAction? x), or the action
# returned or assigned as the one to perform. CanUse(out _) is a probe - a condition, not a use.
CAST = re.compile(r"\b(\w+PvE(?:_\d+)?)\s*\.\s*CanUse\s*\(\s*out\s+(?!_\s*[,)])")
RETURNED = re.compile(r"(?:\breturn|\bact\s*=)\s+(\w+PvE(?:_\d+)?)\s*;")
IDENT = re.compile(r"\b(\w+PvE(?:_\d+)?)\b")
MODIFY = re.compile(r"static partial void Modify(\w+PvE(?:_\d+)?)\s*\(\s*ref ActionSetting setting\s*\)\s*\{")
STATUS_ID = re.compile(r"StatusID\.(\w+)")


def clean_text(raw):
    text = re.sub(r"<[^>]+>", " ", raw).replace("///", " ")
    return " ".join(text.split())


def split_names(names):
    """'Hard Slash or Unleash', 'A and B', 'A, B and C' -> list of names."""
    parts = re.split(r",\s*|\s+or\s+|\s+and\s+", names.strip())
    return [p.strip(" .") for p in parts if p.strip(" .")]


def parse_texts(text):
    """Every relation one effect text states, as (kind, value) pairs."""
    found = []
    for m in COMBO.finditer(text):
        if m.group(1).strip():
            found.append(("combo", m.group(1).strip()))
    for m in CHANGES.finditer(text):
        found.append(("changes", (m.group(1).strip(), m.group(2).strip())))
    for m in SHARES.finditer(text):
        found.append(("shares", m.group(1).strip()))
    for m in NEEDS_ACTIVE.finditer(text):
        found.append(("needs", m.group(1).strip()))
    for m in NEEDS.finditer(text):
        phrase = m.group(1).strip()
        if " is active" in phrase:
            continue
        if phrase.startswith("not "):
            found.append(("needs_not", re.sub(r"^not (?:under the effect of )?", "", phrase)))
        elif phrase[:1].islower():
            found.append(("condition", phrase))
        else:
            found.append(("needs", phrase))
    for m in GRANTS.finditer(text):
        words = []
        for word in m.group(1).split():
            if word in FIELD_WORDS:
                break
            words.append(word)
        if words:
            found.append(("grants", " ".join(words)))
    for resource in costs_of(text):
        found.append(("costs", resource))
    return found


# A name in an effect text: capitalised words, joined by "of" or "the" (Thrill of Battle, Wheel of
# Fortune), never by "and" - "Delirium and Blood Weapon" are two.
NAME = r"[A-Z][\w'’]*(?: (?:of |the )?[A-Z][\w'’]*)*"
# Interplay and time. What a text says about an effect being ended, blocking the GCD, being barred,
# extended, stacked, removed, or producing a resource.
CHANNEL = re.compile(r"[Ee]ffect ends upon using another action or moving")
TOGGLE = re.compile(r"[Ee]ffect ends upon reuse")
GCD_BLOCK = re.compile(r"Triggers the cooldown of weaponskills(?: mudra and Ninjutsu)? upon execution")
CANCELS_AUTO = re.compile(r"Cancels auto-attack")
CANNOT_WHILE = re.compile(r"Cannot be executed while under the effect of (" + NAME + ")")
EXTENDS = re.compile(r"Extends (?:duration of )?(" + NAME + r"?)(?: duration)? by (\d+)s to a maximum of (\d+)s")
STACKS = re.compile(r"(\d+) stacks? of (" + NAME + ")")
REMOVES = re.compile(r"(?:Dispels|Removes|Ends the effect of) (" + NAME + ")")
MP_GAIN = re.compile(r"Restores (?:\d+% of maximum |an amount of )?MP")
GAUGE_GAIN = re.compile(r"(?:[Ii]ncreas(?:es|ing)|[Aa]dds)(?: both| the)? ([A-Z][\w'’]*(?: [A-Z][\w'’]*)*?) (?:Gauge )?by (\d+)")
CARTRIDGE_GAIN = re.compile(r"Adds (?:a|\d+) Cartridges? to your Powder Gauge")
# A value the text leaves out because a trait or the level sets it: "potency of .", "potency of for",
# "Potency: Duration", "Duration: s", and a status name left out: "Grants 2 stacks of Duration: 30s".
BLANK = re.compile(r"(?:[Pp]otency of|Potency:|Cure Potency:) (?=\.|for |[A-Z])|Duration: s\b|\b(?:of|Grants) (?=Duration:)"
                   r"|\bDispels (?:[A-Z][\w'’]* (?:of |the )?)+and (?=[a-z]+ing\b)")
# An enemy's damage lowered is defense; "Increases damage dealt" is a party buff and is not.
DEFENSIVE = re.compile(r"barrier|[Rr]educ\w* damage taken|impervious|cannot be reduced|preventing most attacks|"
                       r"(?:[Rr]educ|[Ll]ower)\w*[^.]{0,40}damage dealt|block rate|only suffer \d+%")
HEALING = re.compile(r"Restores (?:own |target's |the |an amount of )?HP|Cure Potency|[Hh]ealing over time|Regen")
OFFENSIVE = re.compile(r"Deals |Delivers (?:an attack|damage)|damage with a potency|[Ii]ncreases damage dealt|"
                       r"critical hit rate|direct hit rate")


def trim_name(name):
    """A captured name without the field words that follow it in the text ("Requiescat Duration")."""
    words = []
    for word in name.split():
        if word in FIELD_WORDS or word == "Effect":
            break
        words.append(word)
    # "Kazematoi Kazematoi", "Fire Attunement Fire Attunement": the name repeated as the heading of
    # its own effect.
    half = len(words) // 2
    if half and len(words) % 2 == 0 and words[:half] == words[half:]:
        words = words[:half]
    return " ".join(words)


def blocked_by_provide(text, provided_ids):
    """The status a changed action needs that its base action already provides, or None."""
    provided = {re.sub(r"_\d+$", "", sid).lower() for sid in provided_ids}
    for k, v in parse_texts(text):
        if k == "needs" and re.sub(r"[^a-z]", "", v.lower()) in provided:
            return v
    return None


def find_loops(parsed, facts, produce, consume, names):
    """Self-sustaining candidates: an action that needs a status and grants or extends that same
    status, one that costs a resource it also produces, two actions each spending what the other
    makes, and two actions each granting the status the other needs. A null result is only as good
    as the texts: a gain the text leaves out cannot close a cycle."""
    loops = []
    grants_of, needs_of = {}, {}
    for i, found in parsed.items():
        needs_of[i] = {v.lower() for k, v in found if k == "needs"}
        grants_of[i] = {v.lower() for k, v in found if k == "grants"}
        extended = grants_of[i] | {v[0].lower() for k, v in facts.get(i, []) if k == "extends"}
        for status in sorted(needs_of[i] & extended):
            loops.append((i, f"braucht und erneuert {status}"))
    for resource in sorted(set(produce) & set(consume)):
        for i in sorted(produce[resource] & consume[resource]):
            loops.append((i, f"kostet und erzeugt {resource}"))
    for r1 in sorted(consume):
        for r2 in sorted(consume):
            if r1 >= r2:
                continue
            for a in sorted(consume[r1] & produce.get(r2, set())):
                for b in sorted(consume[r2] & produce.get(r1, set())):
                    if a != b:
                        loops.append((a, f"Kreislauf {r1} → {r2} mit {names.get(b, b)}"))
    for a in sorted(parsed):
        for b in sorted(parsed):
            if a < b and (grants_of[a] & needs_of[b]) and (grants_of[b] & needs_of[a]):
                loops.append((a, f"Status-Kreislauf mit {names.get(b, b)}"))
    return loops


def gated_windows(code, windows):
    """Actions whose window can run out: every cast of the action in the rotation code carries a
    condition of its own - none is a plain "if (X.CanUse(out act…)) { return true;" - so a lasting
    condition lets the window pass unused. `windows` maps each action to the names of its window
    (statuses, "…Ready"). For each: the number of casts and whether one of them also fires on the
    window running out - a WillStatusEnd in the same if (condition or body) that names the window.
    Candidates, read by hand: a condition can be the point (burst alignment) or a safety choice."""
    found = []
    spans = method_spans(code)

    def top_level(pos):
        """True when `pos` sits directly in a method body, inside no other block."""
        for start, end in spans:
            if start < pos < end:
                depth = 0
                for ch in code[start:pos]:
                    depth += ch == "{"
                    depth -= ch == "}"
                return depth == 1
        return False

    for ident in sorted(windows):
        casts = list(re.finditer(r"\b" + ident + r"\s*\.\s*CanUse\s*\(\s*out\s+(?:var\s+)?act\b", code))
        if not casts:
            continue
        plain = False
        scopes = []
        for m in casts:
            scope = None
            for head in reversed(list(re.finditer(r"\bif\s*\(", code[:m.start()]))):
                paren = head.end() - 1
                cond = balanced(code, paren)
                cond_end = paren + len(cond) + 2
                body_end = block_end(code, cond_end)
                if not head.start() < m.start() < body_end:
                    continue  # an if that closed before the cast
                if scope is None:
                    # The innermost if: its condition and body, where the fallback can sit.
                    scope = code[head.start():body_end]
                    call = re.match(r"\s*" + ident + r"\s*\.\s*CanUse\s*", cond)
                    if call and len(balanced(cond, call.end())) + 2 + call.end() == len(cond.rstrip()) \
                            and re.match(r"\s*(?:\{\s*)?return\s+true\s*;", code[cond_end:body_end]) \
                            and top_level(head.start()):
                        plain = True
                else:
                    # An enclosing if: its condition can be the fallback (if (ending) { if (X.CanUse…) }).
                    scope += "\n" + cond
            if scope is None:
                line_start = code.rfind("\n", 0, m.start()) + 1
                scope = code[line_start:code.find(";", m.end()) + 1]
            scopes.append(scope)
        if plain:
            continue
        names = windows[ident]
        fallback = any("WillStatusEnd" in sc and any(n in sc for n in names) for sc in scopes)
        found.append((ident, len(casts), fallback))
    return found


def interplay_of(text):
    """What one text says about time and interplay, as (kind, value) pairs."""
    found = []
    if CHANNEL.search(text):
        found.append(("channel", None))
    if TOGGLE.search(text):
        found.append(("toggle", None))
    if GCD_BLOCK.search(text):
        found.append(("gcd", None))
    if CANCELS_AUTO.search(text):
        found.append(("auto", None))
    for m in CANNOT_WHILE.finditer(text):
        found.append(("barred_by", trim_name(m.group(1))))
    for m in EXTENDS.finditer(text):
        found.append(("extends", (trim_name(m.group(1)), int(m.group(2)), int(m.group(3)))))
    for m in STACKS.finditer(text):
        # An empty name is a blank in the text ("2 stacks of Duration"), counted there.
        if trim_name(m.group(2)):
            found.append(("stacks", (int(m.group(1)), trim_name(m.group(2)))))
    for m in REMOVES.finditer(text):
        found.append(("removes", trim_name(m.group(1))))
    if MP_GAIN.search(text):
        found.append(("produces", "MP"))
    for m in GAUGE_GAIN.finditer(text):
        name = m.group(1)
        if name.startswith("both "):
            name = name[5:]
        found.append(("produces", name.replace(" Gauge", "")))
    if CARTRIDGE_GAIN.search(text):
        found.append(("produces", "Cartridge"))
    return found


def kind_of(text, rated):
    """Defensive, heal, offensive or other - the first that fits, in that order. An attack whose heal
    is an additional effect (Souleater, Dosis) is an attack: the text opens with the damage."""
    if rated or DEFENSIVE.search(text):
        return "Abwehr"
    if re.match(r"(?:Deals|Delivers) ", text):
        return "Angriff"
    if HEALING.search(text):
        return "Heilung"
    if OFFENSIVE.search(text):
        return "Angriff"
    return "sonstige"


LOCK_LINE = re.compile(r"if \(Service\.Config\.(\w+) && DataCenter\.Job == (?:[\w.]+\.)?Job\.\w+(.*?)\)\s*$", re.M)
LAST_ACTION = re.compile(r"IsLastAction\(ActionID\.(\w+PvE)\)")
OPTION_DEFAULT = re.compile(r"private static readonly bool _(\w+) = (true|false);")
MOVE_LOCK = re.compile(r"Action = ActionID\.(\w+PvE), Parent = nameof\(PoslockCasting\)\)\]\s*"
                       r"public bool (\w+) \{ get; set; \} = (true|false);")


@functools.lru_cache(maxsize=None)
def central_channel_locks():
    return channel_locks((CENTRAL / "CustomRotation_GCD.cs").read_text(encoding="utf-8"),
                         (CENTRAL / "CustomRotation_Ability.cs").read_text(encoding="utf-8"),
                         (ROOT / "RotationSolver.Basic" / "Configuration" / "Configs.cs").read_text(encoding="utf-8"))


def channel_locks(gcd_code, ability_code, configs_code):
    """What holds a channel open in RSR: {action: [(option, default, path, only while an area hit is
    announced)]} from the action locks at the top of the GCD and ability choice, plus the movement
    locks. A channel without an entry is ended by RSR's next action."""
    defaults = {m.group(1)[:1].upper() + m.group(1)[1:]: m.group(2) == "true"
                for m in OPTION_DEFAULT.finditer(configs_code)}
    locks = {}
    for code, path in ((gcd_code, "GCD"), (ability_code, "Fähigkeit")):
        for m in LOCK_LINE.finditer(code):
            for action in LAST_ACTION.findall(m.group(2)):
                locks.setdefault(action, []).append(
                    (m.group(1), defaults.get(m.group(1)), path, "AutoStatus.DefenseArea" in m.group(2)))
    for m in MOVE_LOCK.finditer(configs_code):
        locks.setdefault(m.group(1), []).append((m.group(2), m.group(3) == "true", "Bewegung", False))
    return locks


@functools.lru_cache(maxsize=None)
def rated_ids():
    """Action ids DefensiveValues rates - the same table the defense rules read."""
    path = ROOT / "RotationSolver.Basic" / "Data" / "DefensiveValues.g.cs"
    text = path.read_text(encoding="utf-8")
    head = text.split("DurationByActionId", 1)[0]
    return {int(x) for x in re.findall(r"\[(\d+)\] = new\(", head)}


# Resources whose name has two words; every other resource is the one word before "Gauge Cost"
# or "Cost". The effect texts run the previous field into the name ("...of maximum MP Addersgall
# Cost"), so only these known pairs are taken as two words.
TWO_WORD_RESOURCES = {"Soul Voice", "Lemure Shroud", "Void Shroud", "Rattling Coil", "White Paint",
                      "Anguine Tribute", "Immortal Sacrifice", "Mana Stack", "Beast Chakra"}


def costs_of(text):
    """The resources an effect text states a cost in."""
    found = []
    for m in re.finditer(r"Balance Gauge Cost: \d+ (Black|White) Mana", text):
        found.append(f"{m.group(1)} Mana")
    for m in re.finditer(r"Cost: \d+", text):
        words = text[:m.start()].split()
        if words and words[-1] == "Gauge":
            words = words[:-1]
        if not words or words[-1] == "Balance" or not words[-1][:1].isupper():
            continue
        pair = " ".join(words[-2:])
        found.append(pair if pair in TWO_WORD_RESOURCES else words[-1])
    return found


def upgrade_pairs(source, target):
    """'A and B' to 'C and D' -> [(A, C), (B, D)]; one target takes every source."""
    sources, targets = split_names(source), split_names(target)
    if len(sources) == len(targets):
        return list(zip(sources, targets))
    if len(targets) == 1:
        return [(s, targets[0]) for s in sources]
    return []


def upgrade_walk(segment, names):
    """Pairs in 'A to B C to D and E to F': the known names in order, each followed by ' to '.

    The trait texts list several upgrades in one sentence without separators; only the action names
    mark where one pair ends. `names` are the lower-case names of this job's actions.
    """
    lower = segment.lower()
    found = []
    for name in sorted(names, key=len, reverse=True):
        start = 0
        while True:
            i = lower.find(name, start)
            if i < 0:
                break
            end = i + len(name)
            boundary = (i == 0 or not lower[i - 1].isalnum()) and (end == len(lower) or not lower[end].isalnum())
            if boundary and not any(a <= i < b or a < end <= b for a, b, _ in found):
                found.append((i, end, name))
            start = end
    found.sort()
    pairs = []
    k = 0
    while k + 1 < len(found):
        between = lower[found[k][1]:found[k + 1][0]]
        if between.strip() == "to":
            pairs.append((found[k][2], found[k + 1][2]))
            k += 2
        else:
            k += 1
    return pairs


def strip_code(code):
    return CODE_STRING.sub('""', CODE_COMMENT.sub("", code))


def balanced(code, start):
    """The text inside the parenthesis that opens at `start`."""
    depth = 0
    for i in range(start, len(code)):
        if code[i] == "(":
            depth += 1
        elif code[i] == ")":
            depth -= 1
            if depth == 0:
                return code[start + 1:i]
    return code[start + 1:]


def block_end(code, start):
    """End of the statement or brace block that starts at `start` (after whitespace)."""
    i = start
    while i < len(code) and code[i].isspace():
        i += 1
    if i < len(code) and code[i] == "{":
        depth = 0
        for j in range(i, len(code)):
            if code[j] == "{":
                depth += 1
            elif code[j] == "}":
                depth -= 1
                if depth == 0:
                    return j + 1
        return len(code)
    j = code.find(";", i)
    return len(code) if j < 0 else j + 1


METHOD = re.compile(r"\b(?:protected|private|public|internal)\b[^;{}=]*?\([^;{}]*\)\s*\{")


def method_spans(code):
    spans = []
    for m in METHOD.finditer(code):
        spans.append((m.end() - 1, block_end(code, m.end() - 1)))
    return spans


def rule_edges(code):
    """(user, read, kind) for every action cast under a condition that reads another action.

    Three shapes are read: the condition of the same if ("Regel prüft"), an if whose block encloses
    the cast ("Regel prüft"), and an earlier if in the same method whose block returns - a guard
    that keeps the cast from being reached ("Regel sperrt vorher"). Conditions read through a
    property or a helper method are not followed.
    """
    edges = set()
    ifs = []
    for m in re.finditer(r"\bif\s*\(", code):
        open_at = m.end() - 1
        cond = balanced(code, open_at)
        cond_end = open_at + len(cond) + 2
        body_end = block_end(code, cond_end)
        body = code[cond_end:body_end]
        # A guard returns without casting: an if that casts in its own condition - CanUse(out act),
        # CanUse(out var act), a helper filling `out act` - and returns is the priority order, not a
        # guard over what follows.
        casts = re.search(r"\(\s*out\s+(?:var\s+|IAction\??\s+)?act\b", cond) is not None
        ifs.append((open_at, cond_end, body_end, set(IDENT.findall(cond)),
                    "return" in body and not CANUSE.search(body) and not casts))
    spans = method_spans(code)
    for m in CANUSE.finditer(code):
        user, pos = m.group(1), m.start()
        method = next(((a, b) for a, b in spans if a <= pos < b), (0, len(code)))
        for open_at, cond_end, body_end, reads, guard in ifs:
            if open_at <= pos < cond_end or cond_end <= pos < body_end:
                for r in reads - {user}:
                    edges.add((user, r, "Regel prüft"))
            elif guard and method[0] <= open_at and body_end <= pos:
                for r in reads - {user}:
                    edges.add((user, r, "Regel sperrt vorher"))
    return edges


def modify_spans(code):
    """(start, end) of every Modify method, header to closing brace."""
    spans = []
    for m in MODIFY.finditer(code):
        spans.append((m.start(), block_end(code, m.end() - 1)))
    return spans


def modify_bodies(code):
    """identifier -> body of its Modify method in the base job rotation."""
    bodies = {}
    for m in MODIFY.finditer(code):
        i = m.end() - 1
        depth = 0
        for j in range(i, len(code)):
            if code[j] == "{":
                depth += 1
            elif code[j] == "}":
                depth -= 1
                if depth == 0:
                    bodies[m.group(1)] = code[i + 1:j]
                    break
    return bodies


def settings_of(body):
    """What one Modify body says about needs, provides, combo and checks."""
    result = {"StatusNeed": [], "StatusProvide": [], "ComboIds": [], "reads": [],
              "ActionCheck": "setting.ActionCheck" in body, "window": [], "has_reads": []}
    # A window: an own status the action needs (StatusNeed, not TargetStatusNeed), or a "…Ready"
    # the ActionCheck reads - something granted for a time and lost when it runs out.
    for m in re.finditer(r"setting\.StatusNeed\s*=\s*(.*?);", body, re.S):
        result["window"] += STATUS_ID.findall(m.group(1))
    for m in re.finditer(r"setting\.ActionCheck\s*=\s*(.*?);", body, re.S):
        # Negated reads and button states ("!DetonatorPvEReady", "IsGarudaReady", "StarryMusePvEReady")
        # are no window.
        result["window"] += [re.sub(r"^Has", "", n) for n in re.findall(r"(?<![!\w])(\w*Ready\w*)\b", m.group(1))
                             if not n.startswith("Is") and not n.endswith("PvEReady")]
        result["has_reads"] = re.findall(r"(?<![!\w])(Has\w+)\b", m.group(1))
    for field in ("StatusNeed", "StatusProvide", "TargetStatusProvide", "TargetStatusNeed"):
        for m in re.finditer(r"setting\." + field + r"\s*=\s*(.*?);", body, re.S):
            key = "StatusNeed" if "Need" in field else "StatusProvide"
            result[key] += STATUS_ID.findall(m.group(1))
    for m in re.finditer(r"setting\.ComboIds\s*=\s*(.*?);", body, re.S):
        result["ComboIds"] += re.findall(r"ActionID\.(\w+PvE(?:_\d+)?)", m.group(1))
    for m in re.finditer(r"setting\.ActionCheck\s*=\s*(.*?);\s*(?:setting\.|$)", body, re.S):
        result["reads"] += IDENT.findall(m.group(1))
    return result


def load():
    ids = html.unescape((PROPERTIES / "ActionId.resx").read_text(encoding="utf-8"))
    actions = {}
    for name, kind, jobs, _row, category, raw, ident, row in ENTRY.findall(ids):
        if kind != "PvE":
            continue
        codes = set()
        for code in jobs.replace(",", " ").split():
            codes.add(CLASS_OF.get(code, code))
        actions[ident] = {
            "id": int(row), "name": name, "category": category, "jobs": codes,
            "text": clean_text(raw), "all": jobs.strip() == "All Classes",
        }

    rot = html.unescape((PROPERTIES / "Rotation.resx").read_text(encoding="utf-8"))
    job_actions = {}
    traits = {}
    limit_breaks = {}
    classes = list(JOB_CLASS.finditer(rot))
    for i, m in enumerate(classes):
        body = rot[m.end(): classes[i + 1].start() if i + 1 < len(classes) else len(rot)]
        job = m.group(1).split(",")[0].replace("Job.", "").strip()
        props = PROPERTY.findall(body)
        # The PvE limit breaks have no entry in ActionId.resx; they are named by the rotation's
        # LimitBreak1..3 properties. Anything else missing is a change in the sheets and stops the run,
        # instead of dropping the action from the matrix without a word.
        lbs = set(re.findall(r"IBaseAction LimitBreak[123] => (\w+PvE(?:_\d+)?);", body))
        missing = [p for p in props if p not in actions and p not in lbs]
        if missing:
            raise SystemExit(f"{job}: actions without an effect text entry: {', '.join(missing)}")
        job_actions[job] = (m.group(2), [p for p in props if p in actions])
        limit_breaks[job] = sorted(p for p in props if p in lbs)
        traits[job] = [(n, clean_text(t)) for n, _j, t, _i in TRAIT.findall(body)]

    general = html.unescape((PROPERTIES / "Action.resx").read_text(encoding="utf-8"))
    general_actions = [p for p in PROPERTY.findall(general) if p in actions]
    return actions, job_actions, traits, general_actions, limit_breaks


def level_of(ident, actions, job):
    """The highest of the owner's levels on which every member job has this action."""
    info = actions[ident]
    for group, members in GROUPS:
        if job in members and (info["all"] or all(member in info["jobs"] for member in members)):
            return group
    return "Job"


def central_code():
    return "\n".join(strip_code(p.read_text(encoding="utf-8")) for p in CENTRAL.glob("CustomRotation*.cs"))


def central_uses(code):
    """Actions the central dispatch casts for every job (role actions, Sprint, ...)."""
    return set(CAST.findall(code)) | set(RETURNED.findall(code))


def analyse(job, actions, job_actions, traits, general_actions, central, central_src, limit_breaks=None):
    class_name, own = job_actions[job]
    # General actions of category System (Teleport, Return, island and field utilities) are no part
    # of a fight; only those the central dispatch casts (Sprint) are kept.
    role = [g for g in general_actions
            if (job in actions[g]["jobs"] or actions[g]["all"])
            and (actions[g]["category"] != "System" or g in central)]
    nodes = list(dict.fromkeys(own + role))
    by_name = {}
    for ident in nodes:
        by_name.setdefault(actions[ident]["name"].lower(), []).append(ident)

    def resolve(name):
        """The job's actions a phrase names: the whole phrase, else every known name inside it.

        Names contain "and" (Fang and Claw, Carve and Spit), so a phrase cannot be split on it; the
        known names are found in it instead, longest first, without overlap.
        """
        whole = by_name.get(name.lower().strip(" ."), [])
        if whole:
            return whole
        lower = name.lower()
        taken = []
        result = []
        for known in sorted(by_name, key=len, reverse=True):
            start = 0
            while True:
                i = lower.find(known, start)
                if i < 0:
                    break
                end = i + len(known)
                boundary = (i == 0 or not lower[i - 1].isalnum()) and (end == len(lower) or not lower[end].isalnum())
                if boundary and not any(a < end and i < b for a, b in taken):
                    taken.append((i, end))
                    result += by_name[known]
                start = end
        return result

    base_path = BASIC / f"{class_name}.cs"
    base_code = strip_code(base_path.read_text(encoding="utf-8")) if base_path.exists() else ""
    bodies = modify_bodies(base_code)
    # What the base rotation casts outside its Modify methods. A CanUse inside a Modify body is part
    # of another action's ActionCheck - a condition, not a use - and is read as such below.
    base_outside = base_code
    for start, end in reversed(modify_spans(base_code)):
        base_outside = base_outside[:start] + base_outside[end:]

    files = [REBORN / f for f in ROTATION_FILES.get(job, [])]
    rotation_code = "\n".join(strip_code(p.read_text(encoding="utf-8")) for p in files if p.exists())

    direct = set(CAST.findall(rotation_code)) | set(CAST.findall(base_outside))
    direct |= set(RETURNED.findall(rotation_code)) | set(RETURNED.findall(base_outside))
    probed = (set(CANUSE.findall(rotation_code)) | set(CANUSE.findall(base_outside))) - direct
    direct |= {a for a in central if a in nodes}
    # Through a property the base rotation points at one action and the dispatch casts.
    alias_calls = set(ALIAS_CANUSE.findall(central_src)) | set(ALIAS_CANUSE.findall(rotation_code))
    for prop, target in PROPERTY_ALIAS.findall(base_code):
        if prop in alias_calls:
            direct.add(target)
    referenced = set(IDENT.findall(rotation_code)) | set(IDENT.findall(base_outside))

    edges = []  # (source, target, kind, layer)
    status_grants = {}
    unresolved = []
    for ident in nodes:
        for kind, value in parse_texts(actions[ident]["text"]):
            if kind == "combo":
                for target in resolve(value):
                    edges.append((ident, target, "Combo nach", "Spiel"))
                if not resolve(value):
                    unresolved.append((ident, "Combo", value))
            elif kind == "changes":
                source, target = value
                for s in resolve(source):
                    for t in resolve(target):
                        edges.append((s, t, "Knopf wird zu", "Spiel"))
            elif kind == "shares":
                for target in resolve(value):
                    if target != ident:
                        edges.append((ident, target, "gemeinsame Abklingzeit", "Spiel"))
            elif kind == "grants":
                status_grants.setdefault(value.lower(), set()).add(ident)
            elif kind == "costs":
                edges.append((ident, value, "kostet", "Spiel"))
    for trait_name, text in traits.get(job, []):
        for m in re.finditer(r"[Uu]pgrades ([^.]+)", text):
            segment = m.group(1)
            # One " to ": two lists paired in order ("A and B to C and D", with or without
            # "respectively"). Several: pairs strung together without separators.
            if segment.count(" to ") == 1:
                left, right = segment.split(" to ", 1)
                pairs = upgrade_pairs(left, right.replace("respectively", ""))
            else:
                pairs = upgrade_walk(segment, list(by_name))
            for source, target in pairs:
                for s in resolve(source):
                    for t in resolve(target):
                        edges.append((s, t, f"Ausbau ({trait_name})", "Spiel"))
        after = GRANTS_AFTER.findall(text)
        for status, action in after:
            for producer in resolve(action):
                status_grants.setdefault(status.lower(), set()).add(producer)
        for m in GRANTS.finditer(text):
            if not any(m.group(1) == status for status, _ in after):
                status_grants.setdefault(m.group(1).lower(), set()).add(f"Eigenschaft {trait_name}")
    for ident in nodes:
        for kind, value in parse_texts(actions[ident]["text"]):
            if kind == "condition":
                edges.append((ident, value, "Bedingung (kein Status)", "Spiel"))
                continue
            if kind not in ("needs", "needs_not"):
                continue
            label = "braucht" if kind == "needs" else "darf nicht haben"
            producers = {p for p in status_grants.get(value.lower(), set()) if p != ident}
            targets = [t for t in resolve(value) if t != ident]
            if producers:
                for p in sorted(producers):
                    edges.append((ident, p, f"{label} {value}", "Spiel"))
            elif targets:
                for t in targets:
                    edges.append((ident, t, f"{label} {value}", "Spiel"))
            else:
                edges.append((ident, value, f"{label} {value} (Erzeuger nicht im Text)", "Spiel"))

    provides = {}
    code_settings = {}
    for ident in nodes:
        body = bodies.get(ident)
        if body is None:
            continue
        settings = settings_of(body)
        code_settings[ident] = settings
        for status in settings["StatusProvide"]:
            provides.setdefault(status, set()).add(ident)
    for ident, settings in code_settings.items():
        for status in settings["StatusNeed"]:
            for p in sorted(provides.get(status, {f"Status {status}"})):
                if p != ident:
                    edges.append((ident, p, f"StatusNeed {status}", "RSR"))
        for c in settings["ComboIds"]:
            edges.append((ident, c, "ComboIds", "RSR"))
        for r in set(settings["reads"]):
            if r != ident and r in nodes:
                edges.append((ident, r, "ActionCheck liest", "RSR"))
    for user, read, kind in sorted(rule_edges(rotation_code)):
        if user in nodes and read in nodes and user != read:
            edges.append((user, read, kind, "RSR"))

    # Use through another button: "A changes to B" or a trait upgrade, A cast directly - the cast
    # goes out on the adjusted id, so B is used whenever A is asked while B is the button.
    via = {}
    changed = True
    while changed:
        changed = False
        for s, t, kind, _layer in edges:
            if (kind == "Knopf wird zu" or kind.startswith("Ausbau")) and (s in direct or s in via) \
                    and t not in direct and t not in via:
                via[t] = s
                changed = True

    # The button change is reached only while the base action's CanUse passes. A base whose
    # StatusProvide holds the very status the changed action needs refuses exactly while the button
    # is changed (Improvisation -> Improvised Finish, Wildfire -> Detonator): the change is never cast.
    usage = {}
    for ident in nodes:
        siblings = [other for other in by_name.get(actions[ident]["name"].lower(), [])
                    if other != ident and other in direct]
        blocked = blocked_by_provide(actions[ident]["text"],
                                     code_settings.get(via[ident], {}).get("StatusProvide", [])) \
            if ident in via else None
        if ident in direct:
            usage[ident] = "direkt"
        elif blocked:
            usage[ident] = (f"ungenutzt — Knopfwechsel über {actions[via[ident]]['name']} gesperrt: "
                            f"deren StatusProvide enthält {blocked} (außer in den letzten "
                            f"StatusRefreshGcdCount GCDs des Status oder mit ShouldCheckStatus aus)")
        elif ident in via:
            usage[ident] = f"über {actions[via[ident]]['name']}"
        elif siblings:
            usage[ident] = f"über gleichnamige Aktion `{siblings[0]}`"
        elif ident in probed:
            usage[ident] = "nur geprüft (CanUse(out _))"
        elif ident in referenced:
            usage[ident] = "nur gelesen"
        else:
            usage[ident] = "ungenutzt"
        if usage[ident].startswith(("nur ", "ungenutzt")) and not blocked:
            text = actions[ident]["text"]
            # A container is the button that turns into others: the source of a "changes to" edge,
            # or a text that says so of itself. "※X changes to <this>" names this action as the
            # target and makes it no container.
            if any(s == ident and k == "Knopf wird zu" for s, _t, k, _l in edges) \
                    or re.search(r"(?:^|: |\. )(?:Action )?[Cc]hanges to|is determined by|may be followed by", text):
                usage[ident] += " — Behälter: der Knopf wird zu anderen Aktionen"
            elif "cannot be assigned to a hotbar" in text:
                usage[ident] += " — nicht zuweisbar: Begleiter oder Automatik"
    # The game names a condition, the code has neither StatusNeed nor ActionCheck: RSR then relies on
    # the game's own
    # usability answer, which BasicCheck only reads for a fixed list of refusal codes. Candidates,
    # not findings - whether that list covers the condition is not visible here.
    unchecked = []
    for ident in nodes:
        needs = [("nicht " if k == "needs_not" else "") + v
                 for k, v in parse_texts(actions[ident]["text"]) if k in ("needs", "needs_not")]
        settings = code_settings.get(ident, {})
        if needs and not settings.get("StatusNeed") and not settings.get("ActionCheck") \
                and usage[ident] == "direkt":
            unchecked.append((ident, needs))
    # Interplay and time (concept 14, "Wechselwirkungen und Zeit").
    rated = rated_ids()
    locks = central_channel_locks()
    # A window read through a Has<X> property of the base rotation (HasHypercharged) is the status it
    # tests.
    prop_status = dict(re.findall(r"public static bool (Has\w+)\s*=>\s*StatusHelper\.PlayerHasStatus\(true,\s*StatusID\.(\w+)\)",
                                  base_code))
    windows = {}
    for i in nodes:
        settings = code_settings.get(i, {})
        names = set(settings.get("window", []))
        for prop in settings.get("has_reads", []):
            if prop in prop_status:
                names.add(prop_status[prop])
        if names:
            windows[i] = names
    gated = gated_windows(rotation_code, windows)
    kinds = {i: kind_of(actions[i]["text"], actions[i]["id"] in rated) for i in nodes}
    blanks = {i: len(BLANK.findall(actions[i]["text"])) for i in nodes}
    facts = {i: interplay_of(actions[i]["text"]) for i in nodes}
    parsed = {i: parse_texts(actions[i]["text"]) for i in nodes}
    trait_facts = [(name, interplay_of(text)) for name, text in traits.get(job, [])]

    def producers_of(status):
        return sorted(p for p in status_grants.get(status.lower(), set()) if p in actions) or resolve(status)

    interplay = []  # (action, finding, other side)
    for i in nodes:
        f = dict()
        for k, v in facts[i]:
            f.setdefault(k, []).append(v)
        if "channel" in f:
            held = [f"{opt} ({'an' if on else 'aus'}) hält {path}"
                    + (" nur bei angekündigtem Flächenschaden" if area else "")
                    for opt, on, path, area in locks.get(i, []) if path != "Bewegung"]
            moving = [f"{opt} ({'an' if on else 'aus'}; wirkt nur mit PoslockCasting)"
                      for opt, on, path, _area in locks.get(i, []) if path == "Bewegung"]
            interplay.append((i, "endet bei jeder weiteren Aktion oder Bewegung (Kanal); "
                              + ("Aktionssperre: " + ", ".join(held) if held
                                 else "keine Aktionssperre: RSRs nächste Aktion beendet ihn")
                              + ("; Bewegungssperre: " + ", ".join(moving) if moving else ""), "jede Aktion"))
        # Only a defense or a heal that takes the GCD takes it from an attack; a mudra or a dance step
        # that takes it is part of the attack.
        if "gcd" in f and kinds[i] in ("Abwehr", "Heilung"):
            interplay.append((i, "belegt den GCD", "GCD-Angriffe"))
        # Barred by its own effect (Ley Lines under Ley Lines) is no reuse while active, not interplay.
        for status in f.get("barred_by", []):
            for p in producers_of(status) or [status]:
                if p != i:
                    interplay.append((i, f"nicht nutzbar unter {status}", p))
        for status in f.get("removes", []):
            for p in producers_of(status) or [status]:
                interplay.append((i, f"hebt {status} auf", p))
    for a, b, k, _l in edges:
        if k == "gemeinsame Abklingzeit" and a in kinds and b in kinds and kinds[a] != kinds[b] \
                and {kinds[a], kinds[b]} & {"Abwehr", "Heilung"}:
            interplay.append((a, f"gemeinsame Abklingzeit ({kinds[a]} / {kinds[b]})", b))

    extensions, stack_list, toggles = [], [], []
    produce, consume = {}, {}
    for i in nodes:
        for k, v in facts[i]:
            if k == "extends":
                extensions.append((i, *v))
            elif k == "stacks":
                stack_list.append((i, *v))
            elif k == "toggle":
                toggles.append(i)
            elif k == "produces":
                produce.setdefault(v, set()).add(i)
        for k, v in parsed[i]:
            if k == "costs":
                consume.setdefault(v, set()).add(i)
    for name, tf in trait_facts:
        for k, v in tf:
            if k == "produces":
                produce.setdefault(v, set()).add(f"Eigenschaft {name}")
    loops = find_loops({i: parsed[i] for i in nodes}, facts, produce, consume,
                       {i: actions[i]["name"] for i in nodes})

    return {
        "class": class_name, "files": [str(p.relative_to(ROOT)) for p in files if p.exists()],
        "kinds": kinds, "blanks": blanks, "interplay": interplay, "extensions": extensions,
        "stacks": stack_list, "toggles": toggles, "produce": produce, "consume": consume, "loops": loops,
        "gated": gated,
        "nodes": nodes, "edges": edges, "usage": usage, "unresolved": unresolved, "unchecked": unchecked,
        "limit_breaks": (limit_breaks or {}).get(job, []),
        "levels": {ident: level_of(ident, actions, job) for ident in nodes},
    }


def render_markdown(job, result, actions, stamp):
    usage = result["usage"]
    out = io.StringIO()
    w = out.write
    w(f"# {job} — Abhängigkeitsmatrix\n\n")
    w(f"Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am {stamp}; nicht von Hand "
      "bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.\n\n")
    w(f"- Basisrotation: `{result['class']}`\n")
    w(f"- Rotation: {', '.join('`' + f + '`' for f in result['files']) or 'keine'}\n")
    w(f"- Matrix als Tabelle: `{job}.csv`\n\n")

    counts = {}
    for u in usage.values():
        key = "über andere Aktion" if u.startswith("über") else u.split(" — ")[0].split(" (")[0]
        counts[key] = counts.get(key, 0) + 1
    w("## Nutzung\n\n")
    w(" · ".join(f"{k}: {v}" for k, v in sorted(counts.items())) + "\n\n")
    w("| Stufe | Aktion | Id | Art | Nutzung |\n|---|---|---|---|---|\n")
    order = [g for g, _ in GROUPS] + ["Job"]
    for ident in sorted(result["nodes"], key=lambda i: (order.index(result["levels"][i]), actions[i]["name"])):
        a = actions[ident]
        w(f"| {result['levels'][ident]} | {a['name']} (`{ident}`) | {a['id']} | {a['category']} | {usage[ident]} |\n")

    w("\n## Beziehungen\n\n")
    for layer, title in (("Spiel", "Aus den Wirktexten (Spiel)"), ("RSR", "Aus dem Code (RSR)")):
        w(f"### {title}\n\n")
        rows = sorted({(s, t, k) for s, t, k, l in result["edges"] if l == layer})
        if not rows:
            w("keine\n\n")
            continue
        w("| Aktion | Beziehung | zu |\n|---|---|---|\n")
        for s, t, k in rows:
            sn = actions[s]["name"] if s in actions else s
            tn = actions[t]["name"] if t in actions else t
            w(f"| {sn} | {k} | {tn} |\n")
        w("\n")

    def name(x):
        return actions[x]["name"] if x in actions else x

    w("## Wechselwirkungen und Zeit\n\n")
    w("Aus den Wirktexten; Art je Aktion: Abwehr (bewertet in `DefensiveValues` oder Wirktext), Heilung, "
      "Angriff, sonstige. Bewertung im Konzept 14.\n\n")
    w("### Abwehr, Angriff und Heilung beenden oder sperren einander\n\n")
    if result["interplay"]:
        w("| Aktion | Art | Befund | Gegenseite |\n|---|---|---|---|\n")
        rows = sorted({(name(a), result["kinds"].get(a, ""), finding, name(other))
                       for a, finding, other in result["interplay"]})
        for row in rows:
            w("| " + " | ".join(row) + " |\n")
    else:
        w("keine\n")
    w("\n### Verlängerung, Aufbau, Umschalten\n\n")
    if result["extensions"]:
        for row in sorted({(name(a), status, plus, cap) for a, status, plus, cap in result["extensions"]}):
            w(f"- {row[0]} verlängert {row[1]} um {row[2]} s, höchstens auf {row[3]} s\n")
    if result["stacks"]:
        for row in sorted({(name(a), n, status) for a, n, status in result["stacks"]}):
            w(f"- {row[0]}: {row[1]} Stapel {row[2]}\n")
    if result["toggles"]:
        w("- Umschalten (endet bei erneutem Einsatz): " + ", ".join(sorted(name(t) for t in result["toggles"])) + "\n")
    if not (result["extensions"] or result["stacks"] or result["toggles"]):
        w("keine\n")
    w("\n### Ressourcen: wer erzeugt, wer verbraucht\n\n")
    resources = sorted(set(result["produce"]) | set(result["consume"]))
    if resources:
        w("| Ressource | erzeugt von | verbraucht von |\n|---|---|---|\n")
        for r in resources:
            prod = ", ".join(sorted(name(x) for x in result["produce"].get(r, []))) or "— (nicht im Wirktext)"
            cons = ", ".join(sorted(name(x) for x in result["consume"].get(r, []))) or "—"
            w(f"| {r} | {prod} | {cons} |\n")
    else:
        w("keine\n")
    w("\n### Kandidaten für Selbsterhaltung\n\n")
    if result["loops"]:
        for a, why in sorted(set(result["loops"])):
            w(f"- {name(a)}: {why}\n")
    else:
        w("keine im Wirktext\n")
    w("\n### Fenster, deren Verbraucher an Bedingungen hängt\n\n")
    if result["gated"]:
        w("Jeder Aufruf der Aktion trägt eine eigene Bedingung; hält sie an, verfällt das Fenster "
          "(Konzept 14, „Werden die Fenster genutzt\"). Kandidaten, von Hand bewertet.\n\n")
        for a, n, fallback in result["gated"]:
            w(f"- {name(a)}: {n} Aufruf(e), " + ("mit Rückfall vor Ablauf" if fallback else "**ohne Rückfall vor Ablauf**") + "\n")
    else:
        w("keine\n")
    incomplete = sorted((a for a, n in result["blanks"].items() if n), key=name)
    w("\n### Unvollständige Beschreibungen\n\n")
    if incomplete:
        w("Der Wirktext lässt Werte aus, die eine Eigenschaft oder die Stufe setzt (Potenz, Dauer): "
          + ", ".join(f"{name(a)} ({result['blanks'][a]})" for a in incomplete) + "\n\n")
    else:
        w("keine\n\n")

    if result["limit_breaks"]:
        w("## Nicht in der Matrix: Limit Breaks\n\n")
        w("Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks "
          "(Konzept 05). " + ", ".join(f"`{x}`" for x in result["limit_breaks"]) + "\n\n")

    not_used = [i for i in result["nodes"] if usage[i].startswith(("ungenutzt", "nur "))]
    w("## Nicht direkt genutzt\n\n")
    if not not_used:
        w("keine\n")
    else:
        w("Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.\n\n")
        for i in sorted(not_used, key=lambda x: actions[x]["name"]):
            w(f"- {actions[i]['name']} (`{i}`, {actions[i]['category']}): {usage[i]}\n")
    if result["unchecked"]:
        w("\n## Abgleich Wirktext ↔ Code\n\n")
        w("Der Wirktext nennt eine Bedingung, die Basisrotation führt dafür weder `StatusNeed` noch "
          "`ActionCheck`. RSR verlässt sich "
          "dann auf die Nutzbarkeitsauskunft des Spiels, die `BasicCheck` nur für eine feste Liste von "
          "Ablehnungscodes liest. Kandidaten, keine Befunde.\n\n")
        for ident, needs in sorted(result["unchecked"], key=lambda x: actions[x[0]]["name"]):
            w(f"- {actions[ident]['name']} (`{ident}`): {'; '.join(needs)}\n")
    if result["unresolved"]:
        w("\n## Nicht aufgelöste Textverweise\n\n")
        for s, kind, value in result["unresolved"]:
            w(f"- {actions[s]['name']}: {kind} „{value}\" — kein gleichnamiges Aktionsobjekt dieses Jobs\n")
    return out.getvalue()


def render_csv(result, actions):
    nodes = result["nodes"]
    cell = {}
    for s, t, k, layer in result["edges"]:
        if s in nodes and t in nodes:
            code = {
                "Combo nach": "C", "Knopf wird zu": "K", "gemeinsame Abklingzeit": "A", "ComboIds": "c",
                "ActionCheck liest": "a", "Regel prüft": "r", "Regel sperrt vorher": "g",
            }.get(k, "U" if k.startswith("Ausbau") else "S" if k.startswith("braucht") else
                  "N" if k.startswith("darf nicht") else "s" if k.startswith("StatusNeed") else "?")
            cell.setdefault((s, t), set()).add(code)
    buf = io.StringIO()
    writer = csv.writer(buf, lineterminator="\n")
    writer.writerow(["Aktion \\ zu"] + [actions[n]["name"] for n in nodes])
    for s in nodes:
        writer.writerow([actions[s]["name"]] + ["".join(sorted(cell.get((s, t), ""))) for t in nodes])
    return buf.getvalue()


def self_test():
    text = ("Delivers an attack with a potency of 620. Combo Action: Hard Slash or Unleash Combo Potency: "
            "Can only be executed while under the effect of Delirium. ※Bloodspiller changes to Scarlet "
            "Delirium while under the effect of Delirium. Shares a recast timer with Edge of Shadow. "
            "Blood Gauge Cost: 50 Additional Effect: Grants Darkside")
    got = parse_texts(text)
    for expected in (("combo", "Hard Slash or Unleash"), ("needs", "Delirium"),
                     ("changes", ("Bloodspiller", "Scarlet Delirium")), ("shares", "Edge of Shadow"),
                     ("costs", "Blood"), ("grants", "Darkside")):
        if expected not in got:
            return f"{expected} not read from the constructed text: {got}"
    if ("needs", "Delirium. ※Bloodspiller changes to Scarlet Delirium while under the effect of Delirium") in got:
        return "a status requirement ran into the next sentence"
    walk = upgrade_walk("Dosis to Dosis II Phlegma to Phlegma II and Eukrasian Dosis to Eukrasian Dosis II",
                        ["dosis", "dosis ii", "phlegma", "phlegma ii", "eukrasian dosis", "eukrasian dosis ii"])
    if walk != [("dosis", "dosis ii"), ("phlegma", "phlegma ii"), ("eukrasian dosis", "eukrasian dosis ii")]:
        return f"an upgrade list without separators was read as {walk}"
    if upgrade_pairs("Flood of Darkness and Edge of Darkness", "Flood of Shadow and Edge of Shadow") != [
            ("Flood of Darkness", "Flood of Shadow"), ("Edge of Darkness", "Edge of Shadow")]:
        return "a list upgrade was not paired in order"
    if GRANTS_AFTER.findall("Grants the effect of Scorn after executing Living Shadow. Duration: 30s") != [
            ("Scorn", "Living Shadow")]:
        return "a status granted after executing an action was not tied to that action"
    if PROPERTY_ALIAS.findall("private protected sealed override IBaseAction TankStance => GritPvE;") != [
            ("TankStance", "GritPvE")]:
        return "a property naming an action was not read"
    if ("combo", "Combo Potency:") in parse_texts("Combo Action: Combo Potency: 300") \
            or [k for k, _ in parse_texts("Combo Action: Combo Potency: 300")].count("combo"):
        return "an empty combo name read the next field as a name"
    if ("combo", "Enchanted Moulinet") not in parse_texts(
            "Combo Action: Enchanted Moulinet Balance Gauge Cost: 15 Black Mana"):
        return "a combo name ran into the cost field"
    for text_, expected in (
            ("Grants Confiteor Ready Duration: 30s", ("grants", "Confiteor Ready")),
            ("Can only be executed while not under the effect of Subtractive Palette.",
             ("needs_not", "Subtractive Palette")),
            ("Can only be executed while less than five chakra are open.",
             ("condition", "less than five chakra are open")),
            ("Balance Gauge Cost: 15 Black Mana Balance Gauge Cost: 15 White Mana", ("costs", "Black Mana")),
            ("Restores 7% of maximum MP Addersgall Cost: 1", ("costs", "Addersgall")),
            ("Soul Voice Gauge Cost: 20", ("costs", "Soul Voice"))):
        if expected not in parse_texts(text_):
            return f"{text_!r} read as {parse_texts(text_)}, expected {expected}"
    if any(k == "costs" and v == "Balance" for k, v in parse_texts("Balance Gauge Cost: 15 Black Mana")):
        return "the balance gauge was read as a resource"
    trait = ("/// <see href=\"x\"><strong>Fire</strong></see> <i>PvE</i> (BLM) [141] [Spell]\n"
             "/// <para>Deals fire damage.</para>\n/// </summary>\npublic IBaseAction FirePvE => x;\n"
             "    /// <see href=\"y\"><strong>Aspect Mastery</strong></see> (BLM) [459]\n"
             "    /// <para>Upgrades Fire to Fire III.</para>\n    /// </summary>\n"
             "    public static IBaseTrait AspectMasteryTrait { get; } = new BaseTrait(459);\n")
    if [(n, t) for n, _j, t, _i in TRAIT.findall(trait)] != [("Aspect Mastery", "Upgrades Fire to Fire III.")]:
        return f"a trait ran across an action entry: {TRAIT.findall(trait)}"
    if set(CAST.findall("A1PvE.CanUse(out act) B1PvE.CanUse(out _) C1PvE.CanUse(out var act) "
                        "D1PvE.CanUse(out _, skipAoeCheck: true)")) != {"A1PvE", "C1PvE"}:
        return f"casts and probes not told apart: {CAST.findall('A1PvE.CanUse(out act) B1PvE.CanUse(out _)')}"
    guard = strip_code('''
        protected override bool CountDown(out IAction act)
        {
            if (CarbunclePvE.CanUse(out var act)) return act;
            if (RuinPvE.CanUse(out act)) return true;
        }
    ''')
    if any(k == "Regel sperrt vorher" for _u, _r, k in rule_edges(guard)):
        return "a priority cast with `out var act` was read as a guard"
    got = interplay_of(
        "Increases block rate to 100%. Duration: 18s Effect ends upon using another action or moving "
        "(including facing a different direction). Cancels auto-attack upon execution. Extends Darkside "
        "duration by 30s to a maximum of 60s. Grants 3 stacks of Delirium and Blood Weapon. Dispels Thrill "
        "of Battle and increasing damage absorbed. Cannot be executed while under the effect of Hammer Time. "
        "Restores 7% of maximum MP. Increases Soul Gauge by 50. Adds a Cartridge to your Powder Gauge.")
    for expected in (("channel", None), ("auto", None), ("extends", ("Darkside", 30, 60)),
                     ("stacks", (3, "Delirium")), ("removes", "Thrill of Battle"), ("barred_by", "Hammer Time"),
                     ("produces", "MP"), ("produces", "Soul"), ("produces", "Cartridge")):
        if expected not in got:
            return f"{expected} not read by interplay_of: {got}"
    if ("extends", ("Death's Design", 30, 60)) not in interplay_of(
            "Extends duration of Death's Design by 30s to a maximum of 60s."):
        return "an extension named after 'duration of' was not read"
    if len(BLANK.findall("Delivers an attack with a potency of . Cure Potency: Duration: s")) != 3:
        return f"blank values miscounted: {BLANK.findall('Delivers an attack with a potency of . Cure Potency: Duration: s')}"
    if trim_name("Requiescat Duration") != "Requiescat" or trim_name("Kazematoi Kazematoi") != "Kazematoi":
        return "field words or a repeated heading stayed in a name"
    if blocked_by_provide("Can only be executed while Improvisation is active.",
                          ["Improvisation", "RisingRhythm"]) != "Improvisation" \
            or blocked_by_provide("Can only be executed while under the effect of Wildfire.", ["Wildfire_1946"]) \
            != "Wildfire" or blocked_by_provide("Can only be executed while Improvisation is active.", ["Bind"]):
        return "a button change its base action blocks by StatusProvide was misread"
    got = channel_locks(
        "\t\tif (Service.Config.PldlockCasting && DataCenter.Job == Job.PLD && IsLastAction(ActionID.PassageOfArmsPvE))\n",
        "\t\tif (Service.Config.PldlockCasting && DataCenter.Job == Job.PLD && IsLastAction(ActionID.PassageOfArmsPvE)"
        " && DataCenter.MergedStatus.HasFlag(AutoStatus.DefenseArea))\n",
        "private static readonly bool _pldlockCasting = false;\n"
        "[UI(\"\", Action = ActionID.PassageOfArmsPvE, Parent = nameof(PoslockCasting))]\n"
        "\tpublic bool PosPassageOfArms { get; set; } = false;")
    if got.get("PassageOfArmsPvE") != [("PldlockCasting", False, "GCD", False),
                                       ("PldlockCasting", False, "Fähigkeit", True),
                                       ("PosPassageOfArms", False, "Bewegung", False)]:
        return f"channel locks misread: {got}"
    loops = find_loops(
        {"a": [("needs", "Enhanced Gallows"), ("grants", "Enhanced Gibbet")],
         "b": [("needs", "Enhanced Gibbet"), ("grants", "Enhanced Gallows")],
         "c": [("needs", "Darkside")], "d": []},
        {"c": [("extends", ("Darkside", 30, 60))]},
        {"Soul": {"d", "e"}, "Shroud": {"d"}}, {"Soul": {"d"}, "Shroud": {"e"}, "Kenki": {"f"}},
        {"b": "Gibbet", "e": "E"})
    for expected in (("a", "Status-Kreislauf mit Gibbet"), ("c", "braucht und erneuert darkside"),
                     ("d", "kostet und erzeugt Soul"), ("e", "Kreislauf Shroud → Soul mit d")):
        if expected not in loops:
            return f"loop {expected} not found: {loops}"
    if len(loops) != 4:
        return f"loops found where none are: {loops}"
    if ("produces", "Ninki") not in interplay_of("Dispels Shadow Walker increasing the Ninki Gauge by 50."):
        return "a gauge gain in the participle form was not read"
    if len(BLANK.findall("Dispels Thrill of Battle and increasing damage absorbed by 2%")) != 1 \
            or BLANK.findall("Dispels Shadow Walker increasing the Ninki Gauge by 50"):
        return "a status name left out of a dispel list was not counted, or a full one was"
    got = gated_windows(
        "protected override bool GeneralGCD(out IAction? act)\n{\n"
        "if (Outer)\n{\n if (LPvE.CanUse(out act)) { return true; }\n}\n"
        "if (StatusHelper.PlayerWillStatusEnd(3, true, StatusID.MReady))\n{\n if (MPvE.CanUse(out act)) { return true; }\n}\n"
        "if (APvE.CanUse(out act)) { return true; }\n"
        "if (HasNoMercy && BPvE.CanUse(out act)) { return true; }\n"
        "if ((HasBuff || StatusHelper.PlayerWillStatusEndGCD(1, 0, true, StatusID.CReady)) && CPvE.CanUse(out act)) { return true; }\n"
        "if (DPvE.CanUse(out act, skipAoeCheck: true))\n{\n if (IsBoss) { return true; }\n}\n"
        "if (FPvE.CanUse(out act) && Target != null)\n{\n if (IsBoss) { return true; }\n"
        " if (StatusHelper.PlayerWillStatusEndGCD(1, 0, true, StatusID.FReady)) { return true; }\n}\n"
        "if (EPvE.CanUse(out act, skipCastingCheck: A && ((B) || HasSwift))) { return true; }\n"
        "if (!HasSwift && GPvE.CanUse(out act)) { return true; }\n"
        "if (IsBoss && HPvE.CanUse(out act) || StatusHelper.PlayerWillStatusEndGCD(9, 0, true, StatusID.Other)) { return true; }\n"
        "if (Foo) { StatusHelper.PlayerWillStatusEnd(1, true, StatusID.KReady); }\nreturn IsBoss && KPvE.CanUse(out act);\n}\n",
        {"APvE": {"AReady"}, "BPvE": {"BReady"}, "CPvE": {"CReady"}, "DPvE": {"DReady"}, "EPvE": {"EReady"},
         "FPvE": {"FReady"}, "GPvE": {"GReady"}, "HPvE": {"HReady"}, "KPvE": {"KReady"}, "LPvE": {"LReady"},
         "MPvE": {"MReady"}})
    if got != [("BPvE", 1, False), ("CPvE", 1, True), ("DPvE", 1, False), ("FPvE", 1, True),
               ("GPvE", 1, False), ("HPvE", 1, False), ("KPvE", 1, False), ("LPvE", 1, False), ("MPvE", 1, True)]:
        return f"gated windows misread: {got}"
    if settings_of("setting.StatusNeed = [StatusID.ReadyToBreak]; setting.ActionCheck = () => HasReadyToReign "
                   "&& !DetonatorPvEReady && IsGarudaReady && StarryMusePvEReady;")["window"] != ["ReadyToBreak", "ReadyToReign"]:
        return "window statuses of a base setting misread"
    if trim_name("Fire Attunement Fire Attunement") != "Fire Attunement":
        return "a repeated heading of two words stayed in a name"
    blank_name = "Additional Effect: Grants 2 stacks of Duration: 30s"
    if len(BLANK.findall(blank_name)) != 1 or any(k == "stacks" for k, _ in interplay_of(blank_name)):
        return "a status name left out of the text was not counted as a blank"
    if kind_of("Delivers an attack with a potency of 300. Additional Effect: Restores own HP", False) != "Angriff":
        return "an attack with a heal as additional effect was read as a heal"
    if kind_of("Delivers an attack. Increases damage dealt by 5%", False) != "Angriff":
        return "a party damage buff was read as a defense"
    if kind_of("Creates a barrier around self", False) != "Abwehr" \
            or kind_of("Deals unaspected damage with a potency of 300", False) != "Angriff" \
            or kind_of("Restores target's HP. Cure Potency: 400", False) != "Heilung":
        return "action kinds are not told apart"
    # Numbered variants of an action (JinPvE_18807, LiturgyOfTheBellPvE_28509) are actions too.
    if PROPERTY.findall("public IBaseAction JinPvE_18807 => x;") != ["JinPvE_18807"] \
            or CAST.findall("JinPvE_18807.CanUse(out act, usedUp: true)") != ["JinPvE_18807"]:
        return "a numbered action variant was not read"
    if parse_texts("Can only be executed while Scorn is active.") != [("needs", "Scorn")]:
        return f"'while X is active' read as {parse_texts('Can only be executed while Scorn is active.')}"

    code = strip_code('''
        // if (CommentPvE.CanUse(out act)) return true;
        var s = "StringPvE.CanUse(";
        if (!InBurst && EdgePvE.Cooldown.IsCoolingDown && FloodPvE.CanUse(out act)) { }
        if (DeliriumPvE.CanUse(out act)) { }
    ''')
    if "CommentPvE" in code or "StringPvE" in code:
        return "a comment or a string was read as code"
    if set(CANUSE.findall(code)) != {"FloodPvE", "DeliriumPvE"}:
        return f"CanUse calls read as {set(CANUSE.findall(code))}"
    if rule_edges(code) != {("FloodPvE", "EdgePvE", "Regel prüft")}:
        return f"rule edges read as {rule_edges(code)}"
    nested = strip_code('''
        protected override bool Attack(out IAction act)
        {
            if (GuardPvE.Cooldown.IsCoolingDown) { return base.Attack(out act); }
            if (EarlierPvE.CanUse(out act)) { return true; }
            if (OuterPvE.EnoughLevel)
            {
                if (InnerPvE.CanUse(out act)) { return true; }
            }
        }
    ''')
    got = rule_edges(nested)
    if ("InnerPvE", "OuterPvE", "Regel prüft") not in got or ("InnerPvE", "GuardPvE", "Regel sperrt vorher") not in got \
            or any(r == "EarlierPvE" for _u, r, _k in got):
        return f"an enclosing condition or an earlier guard was not read: {got}"

    body = modify_bodies('''
        static partial void ModifyXPvE(ref ActionSetting setting)
        {
            setting.StatusNeed = [StatusID.Delirium_1972];
            setting.StatusProvide = [StatusID.Darkside];
            setting.ComboIds = [ActionID.HardSlashPvE];
            setting.ActionCheck = () => YPvE.Cooldown.IsCoolingDown;
        }
    ''')["XPvE"]
    s = settings_of(body)
    if s["StatusNeed"] != ["Delirium_1972"] or s["StatusProvide"] != ["Darkside"] \
            or s["ComboIds"] != ["HardSlashPvE"] or "YPvE" not in s["reads"]:
        return f"base settings read as {s}"
    return None


def generate():
    actions, job_actions, traits, general_actions, limit_breaks = load()
    if not actions or not job_actions:
        raise SystemExit("the sheets could not be read - the source generator format has changed")
    central_src = central_code()
    central = central_uses(central_src)
    stamp = date.today().isoformat()
    files = {}
    summary = []
    for job in sorted(job_actions):
        result = analyse(job, actions, job_actions, traits, general_actions, central, central_src, limit_breaks)
        files[f"{job}.md"] = render_markdown(job, result, actions, stamp)
        files[f"{job}.csv"] = render_csv(result, actions)
        counts = {}
        for u in result["usage"].values():
            key = "über andere Aktion" if u.startswith("über") else u.split(" — ")[0].split(" (")[0]
            counts[key] = counts.get(key, 0) + 1
        summary.append((job, len(result["nodes"]), counts,
                        sum(1 for n in result["blanks"].values() if n),
                        len(set(result["interplay"])), len(set(result["extensions"])), len(set(result["loops"]))))
    index = io.StringIO()
    index.write("# Abhängigkeitsmatrix je Job\n\n")
    index.write(f"Erzeugt am {stamp} von `.github/scripts/audit/generate_action_matrix.py`. Methode, "
                "Grenzen und Bewertung: `docs/rotation-flow/14-action-dependency-matrix.md`.\n\n")
    index.write("| Job | Aktionen | direkt | über andere Aktion | nur geprüft | nur gelesen | ungenutzt | "
                "unvollständig beschrieben | Wechselwirkungen | Verlängerungen | Selbsterhaltung |\n"
                "|---|---|---|---|---|---|---|---|---|---|---|\n")
    for job, n, c, blank, inter, ext, loop in summary:
        mark = " (begrenzter Job)" if job in LIMITED else ""
        index.write(f"| [{job}]({job}.md){mark} | {n} | {c.get('direkt', 0)} | {c.get('über andere Aktion', 0)} | "
                    f"{c.get('nur geprüft', 0)} | {c.get('nur gelesen', 0)} | {c.get('ungenutzt', 0)} | "
                    f"{blank} | {inter} | {ext} | {loop} |\n")
    files["README.md"] = index.getvalue()
    return files


def main():
    failure = self_test()
    if failure:
        print(f"self-test failed: {failure}")
        return 1
    print("self-test ok: combo, button change, shared recast, status need and grant, cost are read; "
          "comments and strings are not code; rule edges and base settings are read.")
    files = generate()
    if "--check" in sys.argv:
        stale = [name for name, content in files.items()
                 if not (OUTPUT / name).exists()
                 or re.sub(r"\d{4}-\d{2}-\d{2}", "", (OUTPUT / name).read_text(encoding="utf-8"))
                 != re.sub(r"\d{4}-\d{2}-\d{2}", "", content)]
        if stale:
            print("out of date: " + ", ".join(stale))
            return 1
        print(f"{len(files)} file(s) up to date.")
        return 0
    OUTPUT.mkdir(parents=True, exist_ok=True)
    for name, content in files.items():
        (OUTPUT / name).write_text(content, encoding="utf-8")
    print(f"{len(files)} file(s) written to {OUTPUT.relative_to(ROOT)}.")
    return 0


if __name__ == "__main__":
    sys.exit(main())

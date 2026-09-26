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
PROPERTY = re.compile(r"public IBaseAction (\w+PvE) =>")
TRAIT = re.compile(
    r"<strong>(.*?)</strong></see> \((\w+)\) \[\d+\]\s*\n\s*/// <para>(.*?)</para>\s*\n\s*/// </summary>\s*\n"
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
    r"[Gg]rants (?:the effect of |\d+ stacks? of |an? )?([A-Z][\w'’:-]*(?: [A-Z][\w'’:-]*)*)"
)
# A trait that grants a status when an action is executed: the action is the producer.
GRANTS_AFTER = re.compile(r"[Gg]rants the effect of ([A-Z][\w'’ -]*?) after executing ([A-Z][\w'’ -]*?)(?:\.| Duration)")
COST = re.compile(r"([A-Z][\w']*(?: [A-Z][\w']*)*?) (?:Gauge )?Cost: \d+")
UPGRADE = re.compile(r"[Uu]pgrades (.+?) to (.+?)(?: respectively| when|\.|$)")
# A property of the base rotation that names one action, cast by the central dispatch through the
# property: TankStance => GritPvE, Raise => AscendPvE.
PROPERTY_ALIAS = re.compile(r"IBaseAction\??\s+(\w+)\s*=>\s*(\w+PvE)\s*;")
ALIAS_CANUSE = re.compile(r"\b(\w+)\??\s*\.\s*CanUse\s*\(")

CODE_COMMENT = re.compile(r"//.*?$|/\*.*?\*/", re.S | re.M)
CODE_STRING = re.compile(r'\$?@?"(?:[^"\\]|\\.)*"')
CANUSE = re.compile(r"\b(\w+PvE)\s*\.\s*CanUse\s*\(")
IDENT = re.compile(r"\b(\w+PvE)\b")
MODIFY = re.compile(r"static partial void Modify(\w+PvE)\s*\(\s*ref ActionSetting setting\s*\)\s*\{")
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
        for name in split_names(m.group(1)):
            found.append(("needs", name))
    for m in NEEDS.finditer(text):
        if " is active" in m.group(1):
            continue
        for name in split_names(m.group(1)):
            found.append(("needs", name))
    for m in GRANTS.finditer(text):
        found.append(("grants", m.group(1)))
    for m in COST.finditer(text):
        found.append(("costs", m.group(1)))
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
        # A guard returns without casting: an if that casts in its own condition and returns true is
        # the priority order, not a guard over what follows.
        casts = re.search(r"CanUse\s*\(\s*out\s+act\b", cond) is not None
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
              "ActionCheck": "setting.ActionCheck" in body}
    for field in ("StatusNeed", "StatusProvide", "TargetStatusProvide", "TargetStatusNeed"):
        for m in re.finditer(r"setting\." + field + r"\s*=\s*(.*?);", body, re.S):
            key = "StatusNeed" if "Need" in field else "StatusProvide"
            result[key] += STATUS_ID.findall(m.group(1))
    for m in re.finditer(r"setting\.ComboIds\s*=\s*(.*?);", body, re.S):
        result["ComboIds"] += re.findall(r"ActionID\.(\w+PvE)", m.group(1))
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
        lbs = set(re.findall(r"IBaseAction LimitBreak[123] => (\w+PvE);", body))
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
    return set(CANUSE.findall(code))


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
    for body in bodies.values():
        base_outside = base_outside.replace(body, "")

    files = [REBORN / f for f in ROTATION_FILES.get(job, [])]
    rotation_code = "\n".join(strip_code(p.read_text(encoding="utf-8")) for p in files if p.exists())

    direct = set(CANUSE.findall(rotation_code)) | set(CANUSE.findall(base_outside))
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
            if kind != "needs":
                continue
            producers = status_grants.get(value.lower(), set())
            targets = resolve(value)
            if producers:
                for p in sorted(producers):
                    edges.append((ident, p, f"braucht {value}", "Spiel"))
            elif targets:
                for t in targets:
                    edges.append((ident, t, f"braucht {value}", "Spiel"))
            else:
                edges.append((ident, value, "braucht (Erzeuger nicht im Text)", "Spiel"))

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

    usage = {}
    for ident in nodes:
        if ident in direct:
            usage[ident] = "direkt"
        elif ident in via:
            usage[ident] = f"über {actions[via[ident]]['name']}"
        elif ident in referenced:
            usage[ident] = "nur gelesen"
        else:
            usage[ident] = "ungenutzt"
        if usage[ident] in ("nur gelesen", "ungenutzt"):
            text = actions[ident]["text"]
            if any(s == ident and k == "Knopf wird zu" for s, _t, k, _l in edges) \
                    or re.search(r"[Cc]hanges to|is determined by|may be followed by", text):
                usage[ident] += " — Behälter: der Knopf wird zu anderen Aktionen"
            elif "cannot be assigned to a hotbar" in text:
                usage[ident] += " — nicht zuweisbar: Begleiter oder Automatik"
    # The game names a condition, the code has neither StatusNeed nor ActionCheck: RSR then relies on
    # the game's own
    # usability answer, which BasicCheck only reads for a fixed list of refusal codes. Candidates,
    # not findings - whether that list covers the condition is not visible here.
    unchecked = []
    for ident in nodes:
        needs = [v for k, v in parse_texts(actions[ident]["text"]) if k == "needs"]
        settings = code_settings.get(ident, {})
        if needs and not settings.get("StatusNeed") and not settings.get("ActionCheck") \
                and usage[ident] == "direkt":
            unchecked.append((ident, needs))
    return {
        "class": class_name, "files": [str(p.relative_to(ROOT)) for p in files if p.exists()],
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
        key = "über anderen Knopf" if u.startswith("über") else u.split(" — ")[0]
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

    if result["limit_breaks"]:
        w("## Nicht in der Matrix: Limit Breaks\n\n")
        w("Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks "
          "(Konzept 05). " + ", ".join(f"`{x}`" for x in result["limit_breaks"]) + "\n\n")

    not_used = [i for i in result["nodes"] if usage[i].startswith(("ungenutzt", "nur gelesen"))]
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
                  "s" if k.startswith("StatusNeed") else "?")
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
            key = "über anderen Knopf" if u.startswith("über") else u.split(" — ")[0]
            counts[key] = counts.get(key, 0) + 1
        summary.append((job, len(result["nodes"]), counts))
    index = io.StringIO()
    index.write("# Abhängigkeitsmatrix je Job\n\n")
    index.write(f"Erzeugt am {stamp} von `.github/scripts/audit/generate_action_matrix.py`. Methode, "
                "Grenzen und Bewertung: `docs/rotation-flow/14-action-dependency-matrix.md`.\n\n")
    index.write("| Job | Aktionen | direkt | über anderen Knopf | nur gelesen | ungenutzt |\n|---|---|---|---|---|---|\n")
    for job, n, c in summary:
        mark = " (begrenzter Job)" if job in LIMITED else ""
        index.write(f"| [{job}]({job}.md){mark} | {n} | {c.get('direkt', 0)} | {c.get('über anderen Knopf', 0)} | "
                    f"{c.get('nur gelesen', 0)} | {c.get('ungenutzt', 0)} |\n")
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

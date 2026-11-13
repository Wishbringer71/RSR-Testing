#!/usr/bin/env python3
"""Every reader of the area, tankbuster and aimed-hit tables that would leave protection out asks whether the running game version
has confirmed the figure (concept 13, "Stufe 2", rule 11; his decision A451).

Since stage 2 a figure taken over from an earlier game version protects as before but saves nothing until a clean reading
of the running version confirms it. The readers that save are few and named in the concept's table; each must ask
`OtherConfiguration.AreaShareConfirmed`, `TankbusterConfirmed` or a value carried from them. A saver that does not ask
lets an outdated figure leave the first cast after a patch without the cover an unrated cast gets - nothing throws, the
fight just goes without it. And a new place that reads a table is not yet sorted into saving or protecting: reading the
tables is allowed only in the files whose readers the concept lists, so a new one is a finding until it is listed.

Checked, in every C# file outside .github/, bin/ and obj/:
- the tables `HostileCastingAreaPotential` and `TankbusterPotential` read only in READING_FILES (comments left out);
- each method in SAVERS holds its confirmation token in its body.
NOT SEEN: a saver reached through another method that reads the figure for it - the savers are listed by name, and a new
one is found only through the file rule or the concept's survey.

Usage: python3 .github/scripts/audit/check_learned_readers.py [--self-test]
"""

import re
import sys
from pathlib import Path

SKIPPED = ('.github', 'bin', 'obj')
TABLE = re.compile(r'\b(?:HostileCastingAreaPotential|TankbusterPotential|AimedHitPotential)\b(?!Gate)')
READING_FILES = {
    'RotationSolver/Basic/Configuration/OtherConfiguration.cs',
    'RotationSolver/Basic/Configuration/OtherConfiguration.Revision.cs',
    'RotationSolver/Basic/Configuration/TankbusterReading.cs',
    'RotationSolver/Basic/DataCenter.cs',
    'RotationSolver/Basic/Helpers/TankbusterTable.cs',
    'RotationSolver/Basic/Helpers/TankbusterForecast.cs',
    'RotationSolver/Watcher.cs',
    'RotationSolver/UI/DiagnosticsWindow.cs',
    'RotationSolver/UI/MainWindow/MainWindow_List.cs',
    'RotationSolver/UI/MainWindow/MainWindow_Learned.cs',
    'RotationSolver/UI/MainWindow/MainWindow_Debug.cs',
}
SAVERS = [  # (file, method name, token its body must hold)
    ('RotationSolver/Basic/DataCenter.cs', 'AreaCastIsWorthMitigating', 'AreaShareConfirmed'),
    ('RotationSolver/Basic/DataCenter.cs', 'BmrRaidwideIsSmallListedCast', 'AreaShareConfirmed'),
    ('RotationSolver/Basic/DataCenter.cs', 'AnnouncedHitIsSmall', '_announcedAreaShareConfirmed'),
    ('RotationSolver/Basic/Rotations/CustomRotation_Tankbuster.cs', 'HoldDefenceForTankbuster', 'Cast.Confirmed'),
    ('RotationSolver/Basic/Helpers/TankbusterHelp.cs', 'MeasuredHitCallsForNoHelp', 'TankbusterConfirmed'),
    ('RotationSolver/Basic/Helpers/TankbusterHelp.cs', 'AimedHitCallsForDefence', 'AimedHitConfirmed'),
]
COMMENT = re.compile(r'^\s*//')


def code_lines(text):
    for number, line in enumerate(text.splitlines(), 1):
        if COMMENT.match(line):
            continue
        yield number, line.split('//', 1)[0]


def body_of(text, method):
    """The body of the first member named `method` - a method or a property - by brace matching; None if absent."""
    match = re.search(rf'\b{re.escape(method)}\b\s*(?:\([^)]*\))?\s*\n?\s*\{{', text)
    if not match:
        return None
    depth, start = 0, match.end() - 1
    for i in range(start, len(text)):
        if text[i] == '{':
            depth += 1
        elif text[i] == '}':
            depth -= 1
            if depth == 0:
                return text[start:i + 1]
    return None


def class_file(path):
    """A partial part `Name.Topic.cs` stands for its class file `Name.cs` (A740, concept 24)."""
    folder, _, name = path.rpartition('/')
    stem = name.split('.', 1)[0]
    return (folder + '/' if folder else '') + stem + '.cs'


def class_text(files, path):
    """The text of a class file with its partial parts; None if the class file is absent."""
    if path not in files:
        return None
    return '\n'.join(text for part, text in sorted(files.items()) if class_file(part) == path)


def check(files):
    """files: {relative path: text}. Returns the findings."""
    found = []
    for path, text in files.items():
        if class_file(path) in READING_FILES:
            continue
        for number, code in code_lines(text):
            if TABLE.search(code):
                found.append(f'{path}:{number}: reads a learned table outside the sorted readers: {code.strip()}')
    for path, method, token in SAVERS:
        text = class_text(files, path)
        body = body_of(text, method) if text is not None else None
        if body is None:
            found.append(f'{path}: the saving reader {method} is not there - moved or renamed? Update SAVERS and the '
                         'concept\'s table')
        elif token not in '\n'.join(code for _, code in code_lines(body)):
            found.append(f'{path}: {method} saves on a learned figure without asking {token}')
    return found


def repository():
    root = Path(__file__).resolve().parents[3]
    files = {}
    for path in root.rglob('*.cs'):
        relative = path.relative_to(root)
        if any(part in SKIPPED for part in relative.parts):
            continue
        files[relative.as_posix()] = path.read_text(encoding='utf-8', errors='replace')
    return files


def tree(**overrides):
    savers = {
        'RotationSolver/Basic/DataCenter.cs': (
            'private static bool AreaCastIsWorthMitigating(uint id, IBattleChara c)\n{\n if (!OtherConfiguration.AreaShareConfirmed(id)) { return true; }\n}\n'
            'internal static bool BmrRaidwideIsSmallListedCast(out float l, out uint a)\n{\n if (!OtherConfiguration.AreaShareConfirmed(a)) { return false; }\n}\n'
            'public static bool AnnouncedHitIsSmall\n{\n get { if (!_announcedAreaShareConfirmed) { return false; } return true; }\n}\n'
            'var x = OtherConfiguration.HostileCastingAreaPotential;\n'),
        'RotationSolver/Basic/Rotations/CustomRotation_Tankbuster.cs':
            'private bool HoldDefenceForTankbuster()\n{\n if (!plan.Cast.Confirmed) { return false; }\n}\n',
        'RotationSolver/Basic/Helpers/TankbusterHelp.cs':
            'private static bool MeasuredHitCallsForNoHelp(IBattleChara t)\n{\n if (!OtherConfiguration.TankbusterConfirmed(a)) { return false; }\n}\n'
            'internal static bool AimedHitCallsForDefence(IBattleChara m, out string w)\n{\n var c = OtherConfiguration.AimedHitConfirmed(a);\n}\n',
    }
    savers.update(overrides)
    return savers


def self_test():
    assert check(tree()) == [], check(tree())
    # A saver moved into a partial part of its class is still found there, and the part reads as its class file (A740).
    moved = tree(**{'RotationSolver/Basic/DataCenter.cs': 'var x = OtherConfiguration.HostileCastingAreaPotential;\n',
                    'RotationSolver/Basic/DataCenter.AreaDamage.cs': tree()['RotationSolver/Basic/DataCenter.cs']})
    assert check(moved) == [], check(moved)
    # A saver without its question is found.
    bad = tree(**{'RotationSolver/Basic/Helpers/TankbusterHelp.cs':
                  'private static bool MeasuredHitCallsForNoHelp(IBattleChara t)\n{\n return share < line;\n}\n'})
    assert any('MeasuredHitCallsForNoHelp saves' in f for f in check(bad)), check(bad)
    # The question only in a comment does not count.
    commented = tree(**{'RotationSolver/Basic/Rotations/CustomRotation_Tankbuster.cs':
                        'private bool HoldDefenceForTankbuster()\n{\n // plan.Cast.Confirmed\n return true;\n}\n'})
    assert any('HoldDefenceForTankbuster saves' in f for f in check(commented)), check(commented)
    # A reader in a new file is found; a mention in a comment is not; the lock is not the table.
    new_file = tree(**{'RotationSolver/Basic/Helpers/NewRule.cs':
                       '// HostileCastingAreaPotential\nlock (OtherConfiguration.TankbusterPotentialGate) { }\n'
                       'if (OtherConfiguration.TankbusterPotential.TryGetValue(id, out var r)) { }\n'})
    findings = check(new_file)
    assert len(findings) == 1 and 'NewRule.cs:3' in findings[0], findings
    # A renamed saver is found.
    renamed = tree(**{'RotationSolver/Basic/DataCenter.cs': 'private static bool SizeRule(uint id) { return true; }\n'})
    assert sum('is not there' in f for f in check(renamed)) == 3, check(renamed)
    print('self-test ok: a saver without its question, a question only in a comment, a reader in a new file and a '
          'renamed saver are found; a comment and the lock are not')


def main():
    self_test()
    if '--self-test' in sys.argv:
        return 0
    findings = check(repository())
    for finding in findings:
        print(finding)
    if findings:
        print(f'{len(findings)} finding(s): sort the reader into saving or protecting (concept 13, "Stufe 2").')
        return 1
    print(f'{len(SAVERS)} saving readers ask for confirmation; the tables are read only in the sorted files.')
    return 0


if __name__ == '__main__':
    sys.exit(main())

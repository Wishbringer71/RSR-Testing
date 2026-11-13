#!/usr/bin/env python3
"""A new member the fork adds to a class of upstream stands in a partial file of its own, not in upstream's file.

Rule: `.claude/rules/code.md`, "Struktur und Kosten"; concept 24, "Fork-Code in eigenen Teildateien" (A740, A742).
Every fork line inside a file of upstream is merge logistics: an upstream change beside it can conflict at the next
sync, and whoever looks for a rule of the fork looks through upstream's file. So a member upstream does not know -
method, property, field, nested type - goes into `Name.Topic.cs`, the topic being the concept that carries it.

What stays in upstream's file, and is accepted here:
- changes inside upstream's own members (their declaration line is upstream's);
- static fields and properties with an initializer, and static constructors: the order of static initializers
  across partial parts is unspecified (C# specification, "Static field initialization"), so they stay beside the
  initializers they may depend on;
- the members listed in `fork_placement.json`, which stood there before this check (A742); a listed member that is
  no longer there is reported, so the list only shrinks.

A member is the fork's when its declaration line stands in neither the merge base with upstream/main nor
upstream/main's version of the file - no diff alignment involved. `ExtraRotations` (foreign rotations) and
`Configs.cs` (every setting in one class, upstream's pattern, read by the source generator) are left out.

    python3 .github/scripts/audit/check_fork_placement.py                    # check
    python3 .github/scripts/audit/check_fork_placement.py --write-baseline   # list what stands there now
"""
import json
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
LIST = Path(__file__).with_name('fork_placement.json')
SKIPPED_PARTS = ('ExtraRotations', 'bin', 'obj')
SKIPPED_FILES = {'RotationSolver/Basic/Configuration/Configs.cs'}
TYPE_DECL = re.compile(r'\b(class|struct|record|interface)\s+\w+')
STRING = re.compile(r'\$?@"(?:""|[^"])*"|\$?"(?:\\.|[^"\\])*"' + r"|'(?:\\.|[^'\\])'")


def run(*args):
    try:
        out = subprocess.run(args, cwd=ROOT, capture_output=True, text=True, check=True)
        return out.stdout
    except (subprocess.CalledProcessError, OSError):
        return None


def code_of(line):
    """The line without strings, chars and a trailing comment, for counting braces."""
    return STRING.sub('""', line).split('//', 1)[0]


def members(text):
    """The members of the top-level types of a file: (first line, last line, declaration line), 1-based. A member
    begins at its first comment, attribute or code line and ends at the line that brings the depth back to its type's
    body and ends with '}' or ';'. Preprocessor lines (#region) belong to no member."""
    lines = text.split('\n')
    found = []
    stack = []            # per open brace: 'type' or 'block'
    pending_type = False
    start = None
    for number, raw in enumerate(lines, 1):
        code = code_of(raw)
        stripped = raw.strip()
        if member_level(stack) and start is None and stripped and not stripped.startswith('#'):
            start = number
        if TYPE_DECL.search(code) and not code.rstrip().endswith(';'):
            pending_type = True
        for ch in code:
            if ch == '{':
                stack.append('type' if pending_type else 'block')
                pending_type = False
            elif ch == '}' and stack:
                stack.pop()
        tail = code.rstrip()
        if start is not None and member_level(stack) and (tail.endswith('}') or tail.endswith(';')):
            found.append((start, number, declaration(lines, start, number)))
            start = None
    return found


def member_level(stack):
    """Directly inside the body of a top-level type."""
    return bool(stack) and stack[-1] == 'type' and stack.count('type') == 1


def declaration(lines, first, last):
    for number in range(first, last + 1):
        stripped = lines[number - 1].strip()
        if stripped and not stripped.startswith('//') and not stripped.startswith('['):
            return ' '.join(stripped.split())
    return ''


def signature(line):
    """A member's name and type without its body or initializer: a change there is a change inside the member."""
    return re.split(r'=>|\{|(?<![=!<>])=(?![=>])', ' '.join(line.split()), maxsplit=1)[0].strip()


def name_of(decl):
    """The member's identifier: the word before '(' for a method, else the last word of its signature."""
    sig = signature(decl).rstrip(';').strip()
    head = sig.split('(', 1)[0].split()
    return head[-1] if head else ''


def must_stay(lines, first, last, decl):
    """Static fields and properties with an initializer, and static constructors, stay where they are."""
    if not re.search(r'\bstatic\b', decl):
        return False
    head = decl.split('=>', 1)[0]
    if re.match(r'(?:(?:private|internal|public|protected)\s+)*static\s+\w+\s*\(\s*\)', decl):
        return True                       # static constructor
    body = '\n'.join(lines[first - 1:last])
    if '(' in head.split('=', 1)[0] and not re.search(r'\{\s*get', body):
        return False                      # a method
    if '=>' in decl and not re.search(r'(?<![=!<>])=(?![=>])', head):
        return False                      # expression-bodied: no initializer
    return bool(re.search(r'(?<![=!<>])=(?![=>])', body.split('=>', 1)[0])) or bool(re.search(r'\}\s*=(?!>)', body))


def fork_members(path, text, upstream_texts):
    """The members of `text` whose declaration line no upstream version of the file holds, and that may move."""
    known = set()
    names = set()
    for upstream in upstream_texts:
        known |= {signature(l) for l in upstream.split('\n')}
        names |= {name_of(d) for _, _, d in members(upstream)}
    lines = text.split('\n')
    result = []
    for first, last, decl in members(text):
        # A member upstream has under this name, its signature changed by the fork, is upstream's member changed: it
        # stays. Price: a new overload of an upstream method passes unseen.
        if decl and signature(decl) not in known and name_of(decl) not in names and not must_stay(lines, first, last, decl):
            result.append(decl)
    return result


def check(current, baseline):
    """current: {path: [declarations]}; baseline: {(path, declaration)}. Returns the findings."""
    found = []
    seen = set()
    for path, decls in sorted(current.items()):
        for decl in decls:
            seen.add((path, decl))
            if (path, decl) not in baseline:
                stem = Path(path).stem
                found.append(f'{path}: new fork member in a file of upstream - move it to {stem}.<Topic>.cs: {decl}')
    for path, decl in sorted(baseline - seen):
        found.append(f'{path}: listed in fork_placement.json but no longer there - remove the entry: {decl}')
    return found


def self_test():
    upstream = 'namespace A;\n\ninternal static class Foo\n{\n\tpublic static int Old() => 1;\n\n\tprivate static int _x;\n}\n'
    added_method = upstream.replace('\tprivate static int _x;\n',
                                    '\tprivate static int _x;\n\n\t/// <summary>New.</summary>\n\tinternal static bool New(int a)\n\t{\n\t\treturn a > 1;\n\t}\n')
    assert fork_members('Foo.cs', added_method, [upstream]) == ['internal static bool New(int a)'], \
        fork_members('Foo.cs', added_method, [upstream])
    changed_inside = upstream.replace('=> 1;', '=> 2;')
    assert fork_members('Foo.cs', changed_inside, [upstream]) == [], 'a change inside upstream\'s member is upstream\'s'
    initializer = upstream.replace('\tprivate static int _x;\n', '\tprivate static int _x;\n\tprivate static readonly int[] Y = [1, 2];\n')
    assert fork_members('Foo.cs', initializer, [upstream]) == [], 'a static field with initializer stays'
    plain_field = upstream.replace('\tprivate static int _x;\n', '\tprivate static int _x;\n\tprivate static int _y;\n')
    assert fork_members('Foo.cs', plain_field, [upstream]) == ['private static int _y;'], 'a field without initializer moves'
    expression = upstream.replace('\tprivate static int _x;\n', '\tprivate static int _x;\n\tinternal static bool Z => _x > 0;\n')
    assert fork_members('Foo.cs', expression, [upstream]) == ['internal static bool Z => _x > 0;'], 'an expression body moves'
    block = 'namespace A\n{\n\tinternal class Bar\n\t{\n\t\tpublic void Old() { }\n\t\tpublic void New() { }\n\t}\n}\n'
    assert fork_members('Bar.cs', block, ['namespace A\n{\n\tinternal class Bar\n\t{\n\t\tpublic void Old() { }\n\t}\n}\n']) \
        == ['public void New() { }'], 'a block namespace is read too'
    nested = upstream.replace('\tprivate static int _x;\n', '\tprivate static int _x;\n\tprivate sealed class Inner\n\t{\n\t\tpublic int A;\n\t}\n')
    assert fork_members('Foo.cs', nested, [upstream]) == ['private sealed class Inner'], 'a nested type is one member'
    widened = upstream.replace('public static int Old() => 1;', 'public static int Old(bool fork = false) => 1;')
    assert fork_members('Foo.cs', widened, [upstream]) == [], 'a changed signature of upstream\'s member stays'
    region = upstream.replace('\tprivate static int _x;\n', '\t#region Fork\n\tprivate static int _x;\n\t#endregion\n')
    assert fork_members('Foo.cs', region, [upstream]) == [], 'a region line belongs to no member'
    findings = check({'Foo.cs': ['internal static bool New(int a)']}, set())
    assert findings and 'move it to Foo.<Topic>.cs' in findings[0], findings
    assert check({'Foo.cs': ['internal static bool New(int a)']}, {('Foo.cs', 'internal static bool New(int a)')}) == []
    stale = check({}, {('Foo.cs', 'gone()')})
    assert stale and 'no longer there' in stale[0], stale
    print('self-test ok: a new member in upstream\'s file is found, in a block namespace and as a nested type too; a '
          'change inside upstream\'s member or its signature, a static initializer and a region line are left alone; a listed member '
          'passes, a stale entry is reported')


def current_members(base):
    current = {}
    for name in (run('git', 'ls-files', 'RotationSolver*') or '').split():
        if not name.endswith('.cs') or name in SKIPPED_FILES or any(p in SKIPPED_PARTS for p in Path(name).parts):
            continue
        upstream_texts = [t for t in (run('git', 'show', f'{ref}:{name}') for ref in (base, 'upstream/main')) if t is not None]
        if not upstream_texts:
            continue                      # the fork's own file
        path = ROOT / name
        if not path.exists():
            continue
        decls = fork_members(name, path.read_text(encoding='utf-8-sig', errors='replace'), upstream_texts)
        if decls:
            current[name] = decls
    return current


def main(argv):
    self_test()
    base = (run('git', 'merge-base', 'upstream/main', 'HEAD') or '').strip()
    if not base:
        print('No merge base with upstream/main - fetch upstream main before this check (git fetch upstream main). '
              'Nothing was checked, and saying so is the result.')
        return 1
    current = current_members(base)
    if '--write-baseline' in argv:
        entries = [{'file': p, 'member': d} for p, ds in sorted(current.items()) for d in ds]
        LIST.write_text(json.dumps({'comment': 'Fork members that stood in files of upstream when check_fork_placement.py '
                                               'came in (A742). The list only shrinks: a new member goes into a partial '
                                               'file Name.Topic.cs.', 'entries': entries},
                                   ensure_ascii=False, indent=1) + '\n', encoding='utf-8')
        print(f'{len(entries)} member(s) listed in {LIST.name}')
        return 0
    baseline = {(e['file'], e['member']) for e in json.loads(LIST.read_text(encoding='utf-8'))['entries']}
    found = check(current, baseline)
    for line in found:
        print(line)
    if found:
        print(f'{len(found)} finding(s): a fork member goes into a partial file of its class (code.md, "Struktur und Kosten").')
        return 1
    print(f'{len(baseline)} listed fork member(s) in files of upstream, no new one.')
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))

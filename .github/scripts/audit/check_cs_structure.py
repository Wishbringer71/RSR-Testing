#!/usr/bin/env python3
"""Is every C# file still structurally intact?

This closes the class of defect where an edit leaves a file that no longer parses - and where the
only thing that notices is a Windows runner a minute and a half later. There is no local .NET in
this working environment, so a broken file travels to CI and the branch sits red until a round trip
finishes. The failure that prompted this check: two helper methods were moved to DataCenter, the
signatures went, the bodies stayed, and the file came back with thirty compiler errors that all
pointed at one wrong line.

Two checks, because one does not cover the other:

1. Brace balance. Counts { and } outside of comments, strings - verbatim, interpolated and raw
   included - and character literals. A file that does not close at zero cannot compile. This is
   what catches the leftover-body case, whose closing brace has no opener left.

2. Statements at member level. Inside a type, that depth holds declarations only: a `foreach`,
   `if`, `return` or `var` sitting there is a body without a signature. A move can leave one while
   the braces still balance out, which check 1 would then pass. Only lines that start a statement
   are examined - a line whose predecessor did not end in ; { or } is treated as a continuation and
   skipped, which keeps multi-line conditions and expression-bodied members out of it.

Neither check is a compiler. A file that passes can still be wrong in every way a type checker
would catch; what it can no longer be is unparseable. That is the whole point - the cheap half of
the answer, available in a second on the Linux job instead of after the Windows build.

Usage: python3 .github/scripts/audit/check_cs_structure.py
"""
import os
import sys

SKIP_DIRS = {'.git', 'obj', 'bin', 'node_modules', '__pycache__', 'Dalamud'}

# Keywords that can only ever open a statement, never a member declaration. `new` is out: it is a
# legal member modifier. `void`/`int` are out: they are return types.
STATEMENT_STARTERS = (
    'if', 'for', 'foreach', 'while', 'do', 'switch', 'return', 'throw', 'var',
    'break', 'continue', 'goto', 'lock', 'yield', 'else',
)


def strip_code(text):
    """Return the source with comments, strings and char literals blanked out.

    Blanked rather than removed so line numbers and columns survive.
    """
    out = []
    i, n = 0, len(text)
    while i < n:
        c = text[i]
        # Raw string literal: """ ... """ (C# 11), any fence length of three or more.
        if c == '"' and text.startswith('"""', i):
            fence = 0
            while i + fence < n and text[i + fence] == '"':
                fence += 1
            quote = '"' * fence
            end = text.find(quote, i + fence)
            if end == -1:
                end = n
            else:
                end += fence
            out.append(' ' if text[i:end].count('\n') == 0 else text[i:end].replace('"', ' '))
            # Keep the newlines inside so line numbers hold.
            out[-1] = ''.join('\n' if ch == '\n' else ' ' for ch in text[i:end])
            i = end
            continue
        # Verbatim string: @"..." where "" is an escaped quote.
        if c == '@' and i + 1 < n and text[i + 1] == '"':
            j = i + 2
            while j < n:
                if text[j] == '"':
                    if j + 1 < n and text[j + 1] == '"':
                        j += 2
                        continue
                    j += 1
                    break
                j += 1
            out.append(''.join('\n' if ch == '\n' else ' ' for ch in text[i:j]))
            i = j
            continue
        # Ordinary string or interpolated string. Braces inside an interpolation hole are code, but
        # they balance among themselves, so blanking the whole literal keeps the count right.
        if c == '"':
            j = i + 1
            while j < n:
                if text[j] == '\\':
                    j += 2
                    continue
                if text[j] == '"':
                    j += 1
                    break
                if text[j] == '\n':
                    break
                j += 1
            out.append(''.join('\n' if ch == '\n' else ' ' for ch in text[i:j]))
            i = j
            continue
        if c == "'":
            j = i + 1
            while j < n:
                if text[j] == '\\':
                    j += 2
                    continue
                if text[j] == "'":
                    j += 1
                    break
                if text[j] == '\n':
                    break
                j += 1
            out.append(' ' * (j - i))
            i = j
            continue
        if c == '/' and i + 1 < n and text[i + 1] == '/':
            j = text.find('\n', i)
            if j == -1:
                j = n
            out.append(' ' * (j - i))
            i = j
            continue
        if c == '/' and i + 1 < n and text[i + 1] == '*':
            j = text.find('*/', i + 2)
            j = n if j == -1 else j + 2
            out.append(''.join('\n' if ch == '\n' else ' ' for ch in text[i:j]))
            i = j
            continue
        out.append(c)
        i += 1
    return ''.join(out)


def member_depth(stripped):
    """Depth at which a type's own members sit: 1 for a file-scoped namespace, 2 for a block one."""
    for line in stripped.split('\n'):
        s = line.strip()
        if s.startswith('namespace '):
            return 1 if s.endswith(';') else 2
    return 1


def check(path):
    """Return a list of findings for one file."""
    try:
        text = open(path, encoding='utf-8-sig').read()
    except (OSError, UnicodeDecodeError) as exc:
        return ['%s: unreadable (%s)' % (path, exc)]

    stripped = strip_code(text)
    findings = []

    depth = 0
    lowest = 0
    lowest_line = 0
    lines = stripped.split('\n')
    depth_at_line_start = []
    for number, line in enumerate(lines, 1):
        depth_at_line_start.append(depth)
        for ch in line:
            if ch == '{':
                depth += 1
            elif ch == '}':
                depth -= 1
                if depth < lowest:
                    lowest, lowest_line = depth, number
    if depth != 0:
        findings.append('%s: braces do not balance - %+d at end of file%s' % (
            path, depth,
            '' if lowest >= 0 else ' (first went negative at line %d)' % lowest_line))
    elif lowest < 0:
        findings.append('%s: an unmatched closing brace at line %d' % (path, lowest_line))

    want = member_depth(stripped)
    raw = text.split('\n')
    previous = ''
    for number, line in enumerate(lines, 1):
        s = line.strip()
        if not s:
            continue
        starts_statement = previous == '' or previous.endswith((';', '{', '}'))
        if starts_statement and depth_at_line_start[number - 1] == want:
            first = s.split('(')[0].split()[0].rstrip(';').strip()
            if first in STATEMENT_STARTERS:
                findings.append('%s:%d: `%s` sits at member level - a body without a signature: %s'
                                % (path, number, first, raw[number - 1].strip()[:70]))
        previous = s
    return findings


def files():
    for root, dirs, names in os.walk('.'):
        dirs[:] = [d for d in dirs if d not in SKIP_DIRS]
        for name in names:
            if name.endswith('.cs'):
                yield os.path.join(root, name)[2:]


def self_test():
    import tempfile

    good = '''namespace X;

public class C
{
    // A brace in a comment { does not count, nor does one in a string "}".
    private const string S = "a { b } c";
    private static bool F()
    {
        foreach (var x in Y)
        {
            if (x) { return true; }
        }
        return false;
    }
}
'''
    # The exact shape that broke the build: signature moved away, body left behind.
    orphan = '''namespace X;

public class C
{
    private static bool F()
    {
        return G();
    }

        foreach (var member in members)
        {
            if (member == null) { continue; }
        }

        return false;
    }
}
'''
    # Braces balance out, and only the member-level check can see it.
    balanced_orphan = '''namespace X;

public class C
{
    private static bool F()
    {
        return G();
    }

    return false;
}
'''
    cases = [(good, 0), (orphan, None), (balanced_orphan, None)]
    for source, expected in cases:
        fd, path = tempfile.mkstemp(suffix='.cs')
        try:
            with os.fdopen(fd, 'w', encoding='utf-8') as fh:
                fh.write(source)
            found = check(path)
            if expected == 0 and found:
                raise AssertionError('clean file rejected: %s' % found)
            if expected is None and not found:
                raise AssertionError('a defect went unnoticed in:\n%s' % source)
        finally:
            os.unlink(path)
    print('self-test ok: clean file accepted, orphaned body caught by the brace count, '
          'orphaned body with balanced braces caught at member level\n')


def main():
    self_test()
    findings = []
    checked = 0
    for path in sorted(files()):
        checked += 1
        findings.extend(check(path))

    if findings:
        print('Structurally broken C# - these cannot compile:\n')
        for finding in findings:
            print('  %s' % finding)
        print('\n%d file(s) checked, %d finding(s).' % (checked, len(findings)))
        return 1

    print('%d C# file(s) checked, all structurally intact.' % checked)
    return 0


if __name__ == '__main__':
    sys.exit(main())

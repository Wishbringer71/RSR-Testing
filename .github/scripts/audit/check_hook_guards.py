#!/usr/bin/env python3
"""A method the game calls through a hook runs nothing but the original function outside a try that catches every
exception.

Requirement: concept 24, "Fehlertoleranz: Code, den das Spiel aufruft" (A794). A .NET exception that leaves a hook
detour crosses into the game's native code and closes the game - in a duty the party loses a member, in the Crucible
the run. A793: one overflow in a fork helper closed the game at the first action of every fight (14.11.2026). Guarding
the one line that failed left the next one open: the action manager's frame detour called fork-changed code
(CooldownDelayTweak, A753) before its try. So the rule holds for every detour as a whole, upstream's included, since a
fork change in any callee reaches the game through them.

What may stand outside a guarded try in a detour body: the original call (`hook!.Original(...)`, `hook?.Original`),
locals, parameters, literals, pointer field reads (`self->Field`, which throw no managed exception), `return`. Anything
that runs code - a call, a constructor, a member or property access other than `.Original` - stands inside a `try`
whose `catch` takes every exception (`catch`, `catch (Exception ...)`), or inside a `catch`. A `finally` is not
guarded: what it runs gets a try of its own. The original call is left out of the try on purpose: inside it, an
exception from another plugin's detour further down the chain would be caught and the call made a second time.

Detours are found where they are registered - `HookFrom...<T>(..., Name)` and `[EzHook(..., nameof(Name))]` - not by
their name; a registered detour whose body is not found fails the check, so a renamed method cannot drop out quietly.
Event handlers (Framework.Update, AddonLifecycle, ECommons' ActionEffect, IPC) are not detours: their sources catch
(concept 24, same section).

    python3 .github/scripts/audit/check_hook_guards.py              # check (runs the self-test first)
    python3 .github/scripts/audit/check_hook_guards.py --self-test
"""
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
SKIPPED_PARTS = ('bin', 'obj')
REGISTRATION = re.compile(r'\bHookFrom\w*\s*<[^;]*?,\s*(\w+)\s*\)')
EZHOOK = re.compile(r'\[EzHook\([^\]]*nameof\((\w+)\)')
STRING = re.compile(r'\$?@"(?:""|[^"])*"|\$?"(?:\\.|[^"\\])*"' + r"|'(?:\\.|[^'\\])'")
BLOCK_COMMENT = re.compile(r'/\*.*?\*/', re.S)
LINE_COMMENT = re.compile(r'//[^\n]*')
KEYWORDS = {'if', 'while', 'for', 'foreach', 'switch', 'catch', 'using', 'lock', 'return', 'nameof', 'typeof',
            'sizeof', 'default', 'when', 'fixed', 'stackalloc'}
CALL = re.compile(r'(?<![\w.])(\w+)\s*(?:<[\w\s,<>?*]*>)?\s*\(')
MEMBER = re.compile(r'(?:\?|!)?\.\s*(\w+)')
NEW = re.compile(r'\bnew\b')
DECLARATION = re.compile(r'\s*(?:(?:public|private|internal|protected|static|unsafe|override|virtual|async|extern|sealed)\s+)*'
                         r'[\w<>\[\],.*?]+')


def strip(text):
    """Code without comments and string contents, line breaks kept so positions map to lines."""
    text = BLOCK_COMMENT.sub(lambda m: re.sub(r'[^\n]', ' ', m.group()), text)
    text = STRING.sub(lambda m: '""' + ' ' * (len(m.group()) - 2), text)
    return LINE_COMMENT.sub(lambda m: ' ' * len(m.group()), text)


def detours(code):
    """The names a file registers as hook detours."""
    return set(REGISTRATION.findall(code)) | set(EZHOOK.findall(code))


def body(code, name):
    """(start, end) of the method body of `name` in stripped code: the braces, or `=> ...;`. None if not found."""
    for match in re.finditer(r'[\w>\]\*?]\s+' + re.escape(name) + r'\s*\(', code):
        line_start = code.rfind('\n', 0, match.start()) + 1
        if not DECLARATION.fullmatch(code[line_start:match.start() + 1]):
            continue  # a call or an assignment, not the declaration
        depth, i = 0, match.end() - 1
        while i < len(code):
            if code[i] == '(':
                depth += 1
            elif code[i] == ')':
                depth -= 1
                if depth == 0:
                    break
            i += 1
        rest = code[i + 1:]
        lead = len(rest) - len(rest.lstrip())
        start = i + 1 + lead
        if code.startswith('=>', start):
            return start + 2, code.index(';', start) + 1
        if not code.startswith('{', start):
            continue
        depth, j = 0, start
        while j < len(code):
            if code[j] == '{':
                depth += 1
            elif code[j] == '}':
                depth -= 1
                if depth == 0:
                    return start + 1, j
            j += 1
    return None


def guarded_spans(text):
    """Spans of `text` inside a try whose catch takes every exception, or inside any catch."""
    stack, spans, pending = [], [], None
    for match in re.finditer(r'\b(try|catch|finally)\b|[{}]', text):
        token = match.group()
        if token in ('try', 'catch', 'finally'):
            pending = (token, match.end())
            continue
        if token == '{':
            kind, header = ('block', '') if pending is None else (pending[0], text[pending[1]:match.start()])
            stack.append((kind, match.start(), header))
            pending = None
            continue
        if not stack:
            continue
        kind, start, header = stack.pop()
        spans.append((kind, start, match.end(), header))
    guarded = []
    for kind, start, end, _ in spans:
        if kind == 'catch':
            guarded.append((start, end))
        elif kind == 'try':
            for other_kind, other_start, _, header in spans:
                if other_kind != 'catch' or other_start < end:
                    continue
                between = text[end:other_start]
                if re.fullmatch(r'\s*catch\s*' + re.escape(header) + r'\s*', between) is None:
                    continue
                if header.strip() == '' or re.fullmatch(r'\s*\(\s*(System\.)?Exception\b[^)]*\)\s*(when\b.*)?', header, re.S):
                    guarded.append((start, end))
                break
    return guarded


def offences(text):
    """(offset, what) for code that runs outside a guarded try in a detour body."""
    guarded = guarded_spans(text)
    def inside(position):
        return any(start <= position < end for start, end in guarded)
    found = []
    for match in CALL.finditer(text):
        name = match.group(1)
        if name in KEYWORDS or name == 'Original' or inside(match.start()):
            continue
        found.append((match.start(), f'call {name}()'))
    for match in MEMBER.finditer(text):
        if match.group(1) == 'Original' or inside(match.start()):
            continue
        before = text[:match.start()].rstrip()
        if before.endswith('-'):  # a '->' read is a field of a native struct
            continue
        found.append((match.start(), f'member .{match.group(1)}'))
    for match in NEW.finditer(text):
        if not inside(match.start()):
            found.append((match.start(), 'constructor'))
    return found


def check_text(raw, path='<text>'):
    """Problems of one file: offences in its detours, and registered detours whose body is not found."""
    code = strip(raw)
    problems = []
    for name in sorted(detours(code)):
        span = body(code, name)
        if span is None:
            problems.append(f'{path}: detour {name} is registered, but its body was not found')
            continue
        start, end = span
        for offset, what in offences(code[start:end]):
            line = code.count('\n', 0, start + offset) + 1
            problems.append(f'{path}:{line}: {name}: {what} runs outside a try that catches every exception')
    return problems


def self_test():
    head = 'class C { void Init() { _h = Svc.Hook.HookFromAddress<D>(addr, MyDetour); }\n'
    good = head + '''private bool MyDetour(int a, Foo* self)
{
    var prev = self->Seq;
    uint? facing = null;
    try { facing = Gaze.Prepare(a); } catch (Exception ex) { PluginLog.Warning($"x {ex}"); }
    bool result;
    try { result = _h!.Original(a, self); }
    finally { try { Gaze.Restore(facing); } catch { } }
    if (self->Seq == prev) { return result; }
    try { Tweak.Note(self->Seq); } catch (Exception ex) when (ex is not null) { Log(ex); }
    return result;
}}'''
    assert check_text(good) == [], check_text(good)
    native = head + 'private bool MyDetour(Foo* self) { var g = self->GetGroup(1); return _h!.Original(self); }}'
    assert any('GetGroup' in p for p in check_text(native)), 'a native method call can run other plugins\' detours'
    unguarded = head + 'private bool MyDetour(int a) { Tweak.Start(a); return _h!.Original(a); }}'
    assert any('.Start' in p for p in check_text(unguarded)), 'a call before any try is found'
    in_finally = head + 'private bool MyDetour(int a) { try { return _h!.Original(a); } finally { Gaze.Restore(a); } }}'
    assert any('Restore' in p for p in check_text(in_finally)), 'a finally is not guarded'
    narrow = head + 'private bool MyDetour(int a) { try { Tweak.Start(a); } catch (InvalidOperationException) { } return _h!.Original(a); }}'
    assert any('Start' in p for p in check_text(narrow)), 'a catch of one exception type does not guard'
    member = head + 'private bool MyDetour(int a) { if (Player.Available) { } return _h!.Original(a); }}'
    assert any('.Available' in p for p in check_text(member)), 'a property read runs code too'
    created = head + 'private bool MyDetour(int a) { var x = new List<int>(); return _h!.Original(a); }}'
    assert any('constructor' in p for p in check_text(created)), 'a constructor runs code too'
    ez = 'class S { [EzHook("40 53", nameof(VfxDetour), true)] EzHook<D> h;\nIntPtr VfxDetour(char* a) { Note(a); return h!.Original(a); }}'
    assert any('VfxDetour' in p and 'Note' in p for p in check_text(ez)), 'an EzHook registration is read'
    expression = head + 'private static int MyDetour(nint a, uint f) => 0; }'
    assert check_text(expression) == [], 'an expression body without code is clean'
    missing = 'class C { void Init() { _h = Svc.Hook.HookFromSignature<D>("E8", GoneDetour); } }'
    assert any('not found' in p for p in check_text(missing)), 'a registered detour without a body fails'
    commented = head + 'private bool MyDetour(int a) { // Tweak.Start(a);\n var s = "Player.Available()"; return _h!.Original(a); }}'
    assert check_text(commented) == [], 'comments and strings are no code'
    print('self-test ok: calls, property reads and constructors outside a guarded try are found; a finally and a narrow '
          'catch do not guard; the original call, pointer reads, comments and strings pass; EzHook registrations are '
          'read; a registered detour without a body fails')


def main(argv):
    self_test()
    if argv[:1] == ['--self-test']:
        return 0
    problems, count = [], 0
    for path in sorted(ROOT.glob('RotationSolver*/**/*.cs')):
        if any(part in SKIPPED_PARTS for part in path.relative_to(ROOT).parts):
            continue
        raw = path.read_text(encoding='utf-8-sig')
        count += len(detours(strip(raw)))
        problems += check_text(raw, path.relative_to(ROOT).as_posix())
    if count == 0:
        print('no hook detour found - the registration pattern no longer matches')
        return 1
    if problems:
        print('\n'.join(problems))
        print(f'{len(problems)} place(s) where a fault would reach the game (concept 24, "Fehlertoleranz", A794)')
        return 1
    print(f'ok: {count} hook detours run nothing but the original call outside a guarded try')
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))

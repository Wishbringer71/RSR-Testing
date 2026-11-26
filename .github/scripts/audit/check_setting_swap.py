#!/usr/bin/env python3
"""Guard against swapping an action's shared setting for one call.

Four fork rules picked their target by saving an action's CanTarget, putting their own condition in its
place, calling CanUse and restoring the saved one in a finally block: Nascent Flash, the Dark Knight's
barrier on the lowest member, the sleep on the chosen enemy and the help for a tankbuster's target.
Upstream 7.5.8.11 (38afb1730) gave CanUse a target filter for exactly this and moved its own Nascent
Flash to it; the fork moved all four (concept 24, "Eine Zielbedingung je Aufruf geht als Zielfilter",
A971). Nothing fails when the swap comes back: it compiles and picks the same targets, and every merge
puts it against upstream's form again. Hence this check.

A swap is recognised by its restore: an assignment `X.Setting.P = local;` (or `setting.P = local;`)
whose right side is a plain local that the same file captured from the same setting member
(`var local = X.Setting.P;`). Composing a setting once in its Modify method
(`setting.ActionCheck = () => saved() && ...`) is not a swap and is left alone.

Left alone, by design: the foreign rotations the fork does not edit (layout.is_third_party).
"""

import re
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import layout  # noqa: E402

ROOTS = [Path('RotationSolver')]

# Strings first, so that a // or /* inside one does not hide the code after it; then comments. Newlines are kept.
TOKENS = re.compile(r'@"(?:[^"]|"")*"|\$?"(?:\\.|[^"\\\n])*"|\'(?:\\.|[^\'\\\n])\'|//[^\n]*|/\*.*?\*/', re.DOTALL)
# The saved value: `var x = ...` or a typed local (`Func<IBattleChara, bool> x = ...`).
SAVE = re.compile(r'(?:\bvar|[\w.]+(?:<[^;=()]*>)?\??)\s+(\w+)\s*=\s*(?:[\w.]+\.)?[Ss]etting\.(\w+)\s*;')


def strip_comments(text):
    """Blanks string and character literals and drops comments, keeping line numbers."""
    def blank(m):
        token = m.group(0)
        return '\n' * token.count('\n') if token.startswith('/') else '""' + '\n' * token.count('\n')
    return TOKENS.sub(blank, text)


ASSIGN = re.compile(r'(?:[\w.]+\.)?[Ss]etting\.(\w+)\s*=(?!=)\s*([^;]*);')


def check(text):
    """Returns the line numbers that restore a saved setting member after it was set to something else; empty means
    none does. Saving and writing the same value back without another value in between - a cache filled on first
    use, a switch written back from the window - is no swap. Statements are read over the whole text, so an
    assignment over several lines counts as one."""
    code = strip_comments(text)
    events = [(m.start(), 'save', m.group(1), m.group(2)) for m in SAVE.finditer(code)]
    events += [(m.start(), 'assign', m.group(2).strip(), m.group(1)) for m in ASSIGN.finditer(code)]
    events.sort()
    saved = set()     # (local, member) once captured
    replaced = set()  # (local, member) whose member was then set to something other than the local
    lines = []
    for position, kind, value, member in events:
        if kind == 'save':
            saved.add((value, member))
            replaced.discard((value, member))
            continue
        for local, saved_member in saved:
            if saved_member != member:
                continue
            if value == local:
                if (local, member) in replaced:
                    lines.append(code.count('\n', 0, position) + 1)
            else:
                replaced.add((local, member))
    return lines


def allowed(path):
    """Whole files left alone: the foreign rotations."""
    return path.is_file() and layout.is_third_party(path)


def self_test():
    """Constructed cases, because a silent pass is otherwise indistinguishable from a clean tree."""
    defects = {
        'CanTarget through a local setting': '''
            var setting = action.Setting;
            var canTarget = setting.CanTarget;
            setting.CanTarget = t => canTarget(t) && t.IsAlive;
            try { return action.CanUse(out act); }
            finally { setting.CanTarget = canTarget; }''',
        'TargetType on the action': '''
            var targetType = action.Setting.TargetType;
            action.Setting.TargetType = TargetType.HighHP;
            try { return action.CanUse(out act); }
            finally { action.Setting.TargetType = targetType; }''',
        'CanTarget set over several lines': '''
            var canTarget = setting.CanTarget;
            setting.CanTarget = t => canTarget(t)
                && t.GetHealthRatio() < ratio;
            try { return action.CanUse(out act); }
            finally { setting.CanTarget = canTarget; }''',
        'CanTarget saved in a typed local': '''
            Func<IBattleChara, bool> keep = action.Setting.CanTarget;
            action.Setting.CanTarget = t => keep(t) && t.IsAlive;
            action.Setting.CanTarget = keep;''',
        'ActionCheck on a named action': '''
            var check = NascentFlashPvE.Setting.ActionCheck;
            NascentFlashPvE.Setting.ActionCheck = () => false;
            NascentFlashPvE.Setting.ActionCheck = check;''',
    }
    for name, text in defects.items():
        if not check(text):
            raise AssertionError('a setting swap went unnoticed (%s)' % name)

    if check('var a = 1;\nvar c = setting.CanTarget;\nsetting.CanTarget = t => c(t);\nsetting.CanTarget = c;') != [4]:
        raise AssertionError('the line number of a finding is wrong')

    clean = '''
        static partial void ModifyHeatBlastPvE(ref ActionSetting setting)
        {
            var heatCheck = setting.ActionCheck;
            setting.ActionCheck = () => heatCheck() && IsOverheated;
        }
        return action.CanUse(out act, targetFilter: t => t.GameObjectId == chosenId);
        var statuses = action.Setting.StatusProvide;
        setting.TargetType = TargetType.Heal;
        // finally { setting.CanTarget = canTarget; }
        var other = setting.CanTarget;
        setting.TargetType = other;
        var comboActions = _action.Setting.ComboActionsCache;
        if (comboActions == null)
        {
            comboActions = baseCombo;
            _action.Setting.ComboActionsCache = comboActions;
        }
        var value = setting.Value;
        if (M3Widgets.RowSwitch(label, ref value)) { setting.Value = value; }
    '''
    if check(clean):
        raise AssertionError('a composition, a read, a filter, a comment or a different member was reported: %s'
                             % check(clean))
    if check('var s = "var c = setting.CanTarget; setting.CanTarget = c;";'):
        raise AssertionError('a swap inside a string was reported')

    if not allowed(Path('RotationSolver/Rotations/Magical/Pictomancer/BeirutaPCT.cs')):
        raise AssertionError('a foreign rotation was not left alone')
    if allowed(Path('RotationSolver/Rotations/Tank/Warrior/WAR_Reborn.cs')):
        raise AssertionError('a fork rotation was let through')

    print('self-test ok: a swap of CanTarget (also set over several lines), TargetType or ActionCheck through a\n'
          '  local (var or typed), the action or a named action is caught; a composition in Modify, a read, a target filter, a\n'
          '  comment, a string, a different member, a cache filled on first use and a switch written back are not;\n'
          '  only the foreign rotations are left alone')


def main():
    self_test()

    found = []
    seen = 0
    for root in ROOTS:
        if not root.exists():
            continue
        for path in sorted(root.rglob('*.cs')):
            if 'obj' in path.parts or 'bin' in path.parts or allowed(path):
                continue
            seen += 1
            found += ['%s:%d' % (path, line) for line in check(path.read_text(encoding='utf-8-sig'))]

    if seen == 0:
        print('no source file found - run from the repository root')
        return 1

    if found:
        print("an action's shared setting swapped for one call and restored - pass the condition as CanUse's "
              'targetFilter or targetOverride instead (concept 24, A971):')
        for f in found:
            print('  - %s' % f)
        return 1

    print('%d source files swap no shared action setting.' % seen)
    return 0


if __name__ == '__main__':
    sys.exit(main())

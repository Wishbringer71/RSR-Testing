#!/usr/bin/env python3
"""The hook input of the scripts in .claude/settings.json, read without waiting for ever.

Claude Code hands a hook its JSON on stdin and closes it. Run by hand or by the CI, stdin may be a pipe that never
sends and never closes; a plain read waits for ever there (found 27.10.2026: run_ci_checks.py hung on
session_brief.py, and KSP-MyContracts found stop_check.py and rule_inject.py built the same way). So the input is read
only when data is there within half a second; nothing or nothing but blanks reads as no hook call.

  hook_io.py --self-test  the three cases against a child process: open silent pipe, closed empty input, JSON
"""

import json
import select
import subprocess
import sys
import time


def read_hook_input(wait=0.5):
    """The hook's JSON as a dict, or None when no hook call is there (a terminal, a silent pipe, empty input)."""
    if sys.stdin is None or sys.stdin.isatty():
        return None
    try:
        if not select.select([sys.stdin], [], [], wait)[0]:
            return None
        text = sys.stdin.read()
    except (OSError, ValueError):
        return None
    if not text.strip():
        return None
    return json.loads(text)


def self_test():
    probe = ("import sys; sys.path.insert(0, %r); import hook_io; d = hook_io.read_hook_input(); "
             "print('none' if d is None else d.get('k'))" % str(__import__("pathlib").Path(__file__).resolve().parent))
    failed = []
    silent = subprocess.Popen([sys.executable, "-c", probe], stdin=subprocess.PIPE, stdout=subprocess.PIPE, text=True)
    start = time.monotonic()
    try:  # stdin stays open and silent while the child runs, as a pipe that never sends
        silent.wait(timeout=10)
        out = silent.stdout.read().strip()
    except subprocess.TimeoutExpired:
        silent.kill()
        out = "hung"
    finally:
        silent.stdin.close()
    if out != "none" or time.monotonic() - start > 5:
        failed.append(f"an open silent pipe reads as no hook call without waiting: got {out!r}")
    empty = subprocess.run([sys.executable, "-c", probe], stdin=subprocess.DEVNULL, capture_output=True, text=True,
                           timeout=10).stdout.strip()
    if empty != "none":
        failed.append(f"closed empty input reads as no hook call: got {empty!r}")
    given = subprocess.run([sys.executable, "-c", probe], input='{"k": "v"}', capture_output=True, text=True,
                           timeout=10).stdout.strip()
    if given != "v":
        failed.append(f"the hook's JSON is read: got {given!r}")
    if failed:
        print("self-test FAILED: " + "; ".join(failed))
        return 1
    print("self-test ok: open silent pipe and empty input read as no hook call, JSON is read")
    return 0


if __name__ == "__main__":
    sys.exit(self_test() if "--self-test" in sys.argv else 0)

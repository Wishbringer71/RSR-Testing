#!/usr/bin/env python3
"""The value of a command-line option of the audit scripts, refused with exit 2 when it is missing.

An option read as argv[argv.index(name) + 1] raises IndexError when it stands last, and takes the next option as its
value when another follows (`--remote --x`). Found in check_source_updates.py (87769bc) and then as a class in seven
scripts (issue #26, 16.11.2026); one function answers for all of them.

  cli_args.py --self-test  a given value, a missing one, an option as value, an absent option, several values
"""

import sys


def option_value(argv, name, default=None):
    """The value after name in argv; default when name is absent. Exits 2 with a usage line when it has none."""
    return option_values(argv, name, 1, [default])[0]


def option_values(argv, name, count, default=None):
    """The count values after name in argv; default when name is absent. Exits 2 when fewer stand there or one of
    them is another option."""
    if name not in argv:
        return default
    values = argv[argv.index(name) + 1:argv.index(name) + 1 + count]
    if len(values) < count or any(v.startswith("--") for v in values):
        sys.stderr.write(f"usage: {name} takes {count} value(s), got {values!r}\n")
        sys.exit(2)
    return values


def option_rest(argv, name):
    """Every value after name in argv, at least one; None when name is absent. Exits 2 when none follows."""
    if name not in argv:
        return None
    values = argv[argv.index(name) + 1:]
    if not values or values[0].startswith("--"):
        sys.stderr.write(f"usage: {name} takes at least one value\n")
        sys.exit(2)
    return values


def self_test():
    failed = []

    def exits(call):
        try:
            call()
        except SystemExit as ended:
            return ended.code
        return None

    if option_value(["--remote", "origin"], "--remote") != "origin":
        failed.append("a given value is read")
    if option_value(["--x"], "--remote", "upstream") != "upstream":
        failed.append("an absent option gives the default")
    if exits(lambda: option_value(["--remote"], "--remote")) != 2:
        failed.append("a missing value exits 2")
    if exits(lambda: option_value(["--remote", "--base", "x"], "--remote")) != 2:
        failed.append("another option taken as value exits 2")
    if option_values(["--record", "a/b", "c1"], "--record", 2) != ["a/b", "c1"]:
        failed.append("two values are read")
    if exits(lambda: option_values(["--record", "a/b"], "--record", 2)) != 2:
        failed.append("one value short exits 2")
    if option_rest(["--record", "x.md", "y.md"], "--record") != ["x.md", "y.md"] or option_rest([], "--record") is not None:
        failed.append("the rest after an option is read, absent gives None")
    if exits(lambda: option_rest(["--record"], "--record")) != 2:
        failed.append("an option without any value exits 2")
    if failed:
        print("self-test FAILED: " + "; ".join(failed))
        return 1
    print("self-test ok: values read, absent options give the default, a missing value or an option as value exits 2")
    return 0


if __name__ == "__main__":
    sys.exit(self_test() if "--self-test" in sys.argv else 0)

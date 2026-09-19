#!/usr/bin/env python3
"""Refuse fork commits whose author or committer address is not one this repository publishes
under.

Why this exists, and why it is not enough on its own
----------------------------------------------------
Six commits in September 2026 carried the repository owner's real name and his private address
into a public repository, because a `git -c user.name=... -c user.email=...` was placed in front
of each commit, overriding the repository's already correct configuration. He found them himself
on the commit page.

GitHub's own guard did not catch it. "Block command line pushes that expose my email" checks
whether the author address is a private email ON the pushing account. The address in question was
not listed on that account, so GitHub did not recognise it as his and let the push through - while
the credential in that environment authenticates AS him, so the commits arrived as his own pushes.
A protection that only covers addresses the account knows cannot cover an address supplied from
outside it.

The primary guard is therefore `.githooks/pre-commit`, which refuses the commit before it exists.
This script is the second line, for a clone that has no `core.hooksPath` set: it reports after the
fact, which is late - a commit is public and permanent, and a history rewrite does not reach the
commits GitHub keeps for a pull request. Late is still better than never, because it names the
commits that need cleaning while the number is small.

Scope
-----
Only the fork's own commits, `upstream/main..HEAD`. Everything reachable from upstream carries the
addresses of its own authors, which are none of this repository's business. If that range cannot
be resolved the script FAILS rather than passing: a silent zero-finding is indistinguishable from
a clean tree, and this class is exactly the one where that difference matters.
"""

import re
import subprocess
import sys

ALLOWED = re.compile(
    r"^(noreply@anthropic\.com"
    r"|noreply@github\.com"  # GitHub itself, as committer on a merge done on the web
    r"|[0-9]+\+[A-Za-z0-9_-]+@users\.noreply\.github\.com)$"
)

SEPARATOR = "\x1f"


def git(*args: str) -> str:
    result = subprocess.run(
        ["git", *args], capture_output=True, text=True, encoding="utf-8", errors="replace"
    )
    if result.returncode != 0:
        raise RuntimeError(f"git {' '.join(args)} failed: {result.stderr.strip()}")
    return result.stdout


def offending_addresses(line: str) -> list[str]:
    """Return the addresses on this log line that are not allowed."""
    parts = line.split(SEPARATOR)
    if len(parts) != 4:
        return []
    _, author_mail, committer_mail, _ = parts
    return [a for a in {author_mail, committer_mail} if a and not ALLOWED.match(a)]


def self_test() -> None:
    """Run against constructed lines, because a check that never fires proves nothing."""
    cases = [
        (f"abc123{SEPARATOR}noreply@anthropic.com{SEPARATOR}noreply@anthropic.com{SEPARATOR}msg",
         [], "the identity this repository publishes under"),
        (f"abc123{SEPARATOR}64041682+Wishbringer71@users.noreply.github.com"
         f"{SEPARATOR}64041682+Wishbringer71@users.noreply.github.com{SEPARATOR}msg",
         [], "a GitHub noreply address"),
        (f"abc123{SEPARATOR}64041682+Wishbringer71@users.noreply.github.com"
         f"{SEPARATOR}noreply@github.com{SEPARATOR}msg",
         [], "a web merge, where GitHub itself is the committer"),
        (f"abc123{SEPARATOR}private.person@example.com"
         f"{SEPARATOR}private.person@example.com{SEPARATOR}msg",
         ["private.person@example.com"], "a private address of a real person, the shape that actually leaked"),
        (f"abc123{SEPARATOR}noreply@anthropic.com"
         f"{SEPARATOR}someone@example.com{SEPARATOR}msg",
         ["someone@example.com"], "a clean author with a leaking committer"),
        (f"abc123{SEPARATOR}someone@example.com"
         f"{SEPARATOR}noreply@anthropic.com{SEPARATOR}msg",
         ["someone@example.com"], "a leaking author with a clean committer"),
        (f"abc123{SEPARATOR}noreply@anthropic.com.evil.example{SEPARATOR}"
         f"noreply@anthropic.com{SEPARATOR}msg",
         ["noreply@anthropic.com.evil.example"], "an address that merely starts with an allowed one"),
    ]
    for line, expected, what in cases:
        found = sorted(offending_addresses(line))
        if found != sorted(expected):
            print(f"SELF-TEST FAILED on {what}: expected {expected}, got {found}")
            sys.exit(2)
    print(
        "self-test ok: the published identities and GitHub's own merge committer are accepted;\n"
        "  a leaking author, a leaking committer, the address that actually leaked, and one\n"
        "  that merely starts with an allowed address are each caught"
    )


def main() -> int:
    self_test()
    print()

    try:
        git("rev-parse", "--verify", "--quiet", "upstream/main^{commit}")
    except RuntimeError:
        print("FAILED: upstream/main is not available, so the fork's own commits cannot be")
        print("  separated from upstream's. Add the upstream remote and fetch it before running")
        print("  this. Passing here would mean reporting a clean tree without having looked.")
        return 1

    log = git(
        "log",
        f"--format=%h{SEPARATOR}%ae{SEPARATOR}%ce{SEPARATOR}%s",
        "upstream/main..HEAD",
    )
    lines = [line for line in log.splitlines() if line.strip()]

    findings: list[tuple[str, str, str]] = []
    for line in lines:
        offenders = offending_addresses(line)
        if offenders:
            sha, _, _, subject = line.split(SEPARATOR)
            for address in sorted(offenders):
                findings.append((sha, address, subject))

    if findings:
        print(f"{len(findings)} commit address(es) that this repository does not publish under:")
        print()
        for sha, address, subject in findings:
            print(f"  {sha}  {address}")
            print(f"          {subject[:88]}")
        print()
        print("This repository is public. Such an address stays in every clone, and rewriting")
        print("history does not remove it from the commits GitHub keeps for a pull request.")
        print("Set core.hooksPath to .githooks so the next one is refused before it exists.")
        return 1

    print(f"{len(lines)} fork commit(s) checked against upstream/main.")
    print("Every author and committer address is one this repository publishes under.")
    return 0


if __name__ == "__main__":
    sys.exit(main())

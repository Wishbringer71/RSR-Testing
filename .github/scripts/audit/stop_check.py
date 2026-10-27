#!/usr/bin/env python3
"""Stop hook (.claude/settings.json). Two rules of CLAUDE.md stood as text only and were broken and rebuked several
times: a turn does not end on an announcement ("Bevor ich einen Zug beende"), and the chat is German ("Sprache und
Namen"; C130). This checks the final message of every turn against both, and against an empty one (a turn that ended
after a tool call without a result). Patterns and the empty case follow KerbalMissionDirector's tools/stop_guard.py;
a message that ends on a question is no announcement. On a finding it hands the reason back as
additionalContext and the turn goes on; on that continuation (stop_hook_active) it lets the turn end, so a wrong
finding costs one round and never a loop. It sees the final message only, not the messages between tool calls.

  stop_check.py              the hook: reads the hook input on stdin, prints its answer
  stop_check.py --self-test  constructed messages
  stop_check.py --transcript FILE  the final messages of a session transcript (.jsonl), each with its finding
"""

import json
import re
import sys

FENCE = re.compile(r"```.*?(?:```|\Z)|~~~.*?(?:~~~|\Z)", re.S)
INLINE = re.compile(r"`[^`\n]*`")
QUOTED = re.compile(r"„[^“\n]*“|\"[^\"\n]*\"|»[^«\n]*«")
LINKS = re.compile(r"https?://\S+|\]\([^)]*\)")
WORD = re.compile(r"[A-Za-zÄÖÜäöüß']+")

EN = set("the and is are to of with for this that will not be on it was were have has which from they these "
         "there would should could i'll let's don't doesn't it's".split())
DE = set("der die das und ist sind nicht mit für ich ein eine auf zu den dem im sich auch es wird werden wurde "
         "bei aus nach oder wie noch nur dass als wenn hat haben".split())

VERBS = (r"(?:prüfe|baue|schreibe|lese|starte|beginne|mache|setze|ändere|teste|gehe|hole|trage|führe|committe|"
         r"pushe|kümmere|arbeite|implementiere|untersuche|kläre|suche|messe|rechne|erhebe|ergänze|korrigiere)")
WHEN = r"(?:jetzt|nun|gleich|sofort|als\s+n[äa]chstes|anschlie(?:ß|ss)end|danach|im\s+n[äa]chsten\s+schritt)"
ANNOUNCE = [
    re.compile(r"\b(?:ich|wir)\s+werden?\b", re.I),
    re.compile(r"\bweiter\s+mit\b|\bn[äa]chste[rn]?\s+(?:punkt|schritt)\b|\bjetzt\s+(?:folgt|kommt)\b|"
               r"\bich\s+(?:beginne|fange|starte)\b|\bnext\s+(?:up|step|item)\b|\bmoving\s+on\s+to\b", re.I),
    re.compile(rf"\b{WHEN}\s+(?:\w+\s+){{0,3}}?{VERBS}\b", re.I),
    re.compile(rf"\b{VERBS}\s+ich\s+(?:\w+\s+){{0,3}}?{WHEN}\b", re.I),
    re.compile(r"\b(?:I'll|I will|let me|I'm going to|next,? I)\b", re.I),
]


def prose(message):
    """The message without code, quotes and links: what is written in the chat's own voice."""
    for pattern in (FENCE, INLINE, QUOTED, LINKS):
        message = pattern.sub(" ", message)
    return message


def findings(message):
    if not message.strip():
        return ["Der Zug endet ohne Ergebnis: Die letzte Nachricht ist leer, der Zug endete nach einem Werkzeugaufruf. "
                "CLAUDE.md, „Bevor ich einen Zug beende“: Ergebnis, Blocker oder eine Frage, die nur er beantworten kann."]
    out = []
    text = prose(message)
    words = [w.lower() for w in WORD.findall(text)]
    en, de = sum(w in EN for w in words), sum(w in DE for w in words)
    if en + de >= 8 and en > 1.5 * de:
        out.append(f"Die letzte Nachricht ist überwiegend Englisch ({en} englische gegen {de} deutsche Funktionswörter). "
                   f"CLAUDE.md, „Sprache und Namen“: Der Chat ist Deutsch, auch jede Zwischenmeldung.")
    blocks = [b.strip() for b in re.split(r"\n\s*\n", text) if b.strip()]
    last = blocks[-1] if blocks else ""
    hit = next((m.group(0) for p in ANNOUNCE for m in [p.search(last)] if m), None)
    if message.rstrip().endswith("?"):
        hit = None
    elif hit or message.rstrip().endswith(":"):
        what = f"„{hit}“" if hit else "einem Doppelpunkt, dem nichts folgt"
        out.append(f"Die letzte Nachricht endet mit einer Ankündigung ({what}). CLAUDE.md, „Bevor ich einen Zug "
                   f"beende“: Ein Zug endet mit einem Ergebnis, einem Blocker oder einer Frage, die nur er beantworten "
                   f"kann; angekündigte Arbeit beginnt im selben Zug.")
    return out


def hook(data):
    if data.get("stop_hook_active"):
        return None
    found = findings(data.get("last_assistant_message") or "")
    if not found:
        return None
    return {"hookSpecificOutput": {"hookEventName": "Stop", "additionalContext": "\n".join(found)}}


def transcript(path):
    """The last text of every assistant turn that ends before a message of the user, each with its findings."""
    entries = [json.loads(line) for line in open(path, encoding="utf-8") if line.strip()]
    last_text, answered, out = None, False, []
    for e in entries:
        msg = e.get("message") or {}
        content = msg.get("content")
        if e.get("type") == "assistant" and isinstance(content, list):
            answered = True
            texts = [c.get("text", "") for c in content if c.get("type") == "text"]
            if texts:
                last_text = texts[-1]
            if any(c.get("type") == "tool_use" for c in content):
                last_text = None
        elif e.get("type") == "user" and isinstance(content, (str, list)):
            is_tool_result = isinstance(content, list) and any(c.get("type") == "tool_result" for c in content)
            if not is_tool_result and answered:
                out.append((last_text or "", findings(last_text or "")))
                last_text, answered = None, False
    if answered:
        out.append((last_text or "", findings(last_text or "")))
    return out


def self_test():
    cases = {
        "a German result passes": ("Die Prüfung ist grün, der PR hat keine offenen Kommentare und wartet auf dich.", 0),
        "an announcement at the end is found": ("Der Regel-Hook steht.\n\nAls Nächstes prüfe ich die Tabellen.", 1),
        "'ich werde' is found": ("Das ist noch offen.\n\nIch werde das gleich erledigen.", 1),
        "a trailing colon is found": ("Ich prüfe die Dateien und sehe nach, was sich geändert hat:", 1),
        "an English message is found": ("The check is green and the pull request has no open comments. It will wait "
                                        "for the review, and there is nothing to do for this branch.", 1),
        "English in code and quotes is not counted": ("Hier ist der Text für den Release:\n\n```text\nThe check is "
                                                      "green and the pull request has no open comments. It will wait "
                                                      "for the review, and there is nothing for this.\n```", 0),
        "an announcement before the result is not the end": ("Ich prüfe jetzt die Tabellen.\n\nErgebnis: keine "
                                                             "Tabelle ohne Kopf, die Prüfung ist grün.", 0),
        "a waiting status passes": ("Der Build läuft noch. Das Ergebnis kommt als Ereignis über das PR-Abonnement.", 0),
        "an empty message is found": ("  \n", 1),
        "a perfect tense is no announcement (Retainer-Repricer)": ("Ich habe jetzt alle Tests grün.", 0),
        "'gleich' as a comparison is no announcement (Retainer-Repricer)": ("Der Preis liegt gleich dem günstigsten "
                                                                           "Angebot, und die Liste ist geprüft.", 0),
        "'weiter mit' is found": ("Gepusht.\n\nWeiter mit D18.", 1),
        "a question at the end is no announcement": ("Erledigt.\n\nSoll ich als Nächstes die Tabellen prüfen?", 0),
    }
    failed = [name for name, (msg, n) in cases.items() if len(findings(msg)) != n]
    if hook({"stop_hook_active": True, "last_assistant_message": "Als Nächstes prüfe ich das."}) is not None:
        failed.append("a continuation is let through")
    if hook({"last_assistant_message": "Als Nächstes prüfe ich das."}) is None:
        failed.append("the hook answers on a finding")
    if failed:
        print("self-test FAILED: " + "; ".join(failed))
        return 1
    print(f"self-test ok: {len(cases) + 2} cases - announcement at the end, trailing colon, English, code left out, "
          f"continuation let through")
    return 0


def main(argv):
    if "--self-test" in argv:
        return self_test()
    if "--transcript" in argv:
        for text, found in transcript(argv[argv.index("--transcript") + 1]):
            if found:
                print("---\n" + text[-300:].replace("\n", " ") + "\n  => " + " | ".join(f[:90] for f in found))
        return 0
    try:
        out = hook(json.load(sys.stdin))
    except Exception as ex:  # a broken hook must not hold the turn; exit 1 shows the error without blocking
        print(f"stop_check.py: {type(ex).__name__}: {ex}", file=sys.stderr)
        return 1
    if out:
        print(json.dumps(out, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))

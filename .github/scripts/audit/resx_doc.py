#!/usr/bin/env python3
"""The effect texts of the generated sheets (RotationSolver.SourceGenerators/Properties/*.resx) in one shape.

Upstream c79f584a5 (26.11.2026) writes each line of an effect text as a <para> of its own and escapes the apostrophe
("&amp;apos;"); before, one <para> held the whole text, its lines joined by a space, with a plain apostrophe. Every
reader here was written for the old shape and read one line of the text only - the heal-target check lost Rekindle's
"when HP falls below 75%", the defensive values found no figure at all, the action matrix lost its trait texts (A982).
normalize() gives every reader the old shape back, in one place: the <para> lines of one summary joined into one, the
empty ones dropped, the apostrophe plain. It takes the file as stored (escaped, "&lt;para&gt;") and after
html.unescape ("<para>").

  resx_doc.py --self-test   both shapes, escaped and unescaped, against the old form
"""

import re
import sys

PARA_LINE = re.compile(r"^(?P<lead>\s*///\s?)(?P<open>&lt;para&gt;|<para>)(?P<text>.*?)(?P<close>&lt;/para&gt;|</para>)\s*$")


def normalize(text):
    """The text with consecutive <para> lines of a doc comment joined into one, as the sheets were written before."""
    out = []
    run = []
    lead = open_tag = close_tag = ""
    newline = "\r\n" if "\r\n" in text else "\n"

    def flush():
        if run:
            joined = " ".join(part for part in run if part.strip())
            out.append(f"{lead}{open_tag}{joined}{close_tag}")
            run.clear()

    for line in text.split(newline):
        match = PARA_LINE.match(line)
        if match:
            if not run:
                lead, open_tag, close_tag = match.group("lead"), match.group("open"), match.group("close")
            run.append(match.group("text").strip())
            continue
        flush()
        out.append(line)
    flush()
    return newline.join(out).replace("&amp;apos;", "'").replace("&apos;", "'")


def self_test():
    new = ("/// &lt;summary&gt;\n"
           "/// &lt;see&gt;Rekindle&lt;/see&gt; (SMN) [25830] [Ability]\n"
           "/// &lt;para&gt;Restores own or target party member&amp;apos;s HP.&lt;/para&gt;\n"
           "/// &lt;para&gt;Cure Potency: 400&lt;/para&gt;\n"
           "/// &lt;para&gt;&lt;/para&gt;\n"
           "/// &lt;para&gt;Rekindle Effect: Healing over time when HP falls below 75%&lt;/para&gt;\n"
           "/// &lt;/summary&gt;\n"
           "RekindlePvE = 25830,\n")
    old = ("/// &lt;summary&gt;\n"
           "/// &lt;see&gt;Rekindle&lt;/see&gt; (SMN) [25830] [Ability]\n"
           "/// &lt;para&gt;Restores own or target party member's HP. Cure Potency: 400 Rekindle Effect: Healing over time "
           "when HP falls below 75%&lt;/para&gt;\n"
           "/// &lt;/summary&gt;\n"
           "RekindlePvE = 25830,\n")
    failed = []
    if normalize(new) != old:
        failed.append("the new escaped shape is joined into the old one")
    if normalize(old) != old:
        failed.append("the old shape stays as it is")
    unescaped_new = new.replace("&lt;", "<").replace("&gt;", ">").replace("&amp;apos;", "&apos;")
    unescaped_old = old.replace("&lt;", "<").replace("&gt;", ">")
    if normalize(unescaped_new) != unescaped_old:
        failed.append("the unescaped shape is joined as well")
    indented = "    /// <para>Delivers an attack.</para>\r\n    /// <para>Combo Action: Fast Blade</para>\r\n    /// </summary>\r\n"
    if normalize(indented) != "    /// <para>Delivers an attack. Combo Action: Fast Blade</para>\r\n    /// </summary>\r\n":
        failed.append("an indented block with Windows line ends keeps its indent and line ends")
    two = new + new.replace("25830", "25831")
    if normalize(two).count("&lt;para&gt;") != 2:
        failed.append("two summaries stay two")
    for item in failed:
        print("self-test failed:", item)
    if not failed:
        print("self-test ok: the new shape of the effect texts is joined into the old one, escaped and unescaped, the old "
              "shape and indents and line ends are kept, and summaries stay apart")
    return 1 if failed else 0


if __name__ == "__main__":
    if sys.argv[1:] == ["--self-test"]:
        sys.exit(self_test())
    print(__doc__)
    sys.exit(2)

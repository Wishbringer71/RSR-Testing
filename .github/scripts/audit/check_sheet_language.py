#!/usr/bin/env python3
"""A text of the game's sheets that the code compares with an English phrase is read from the English sheet (A547).

Dalamud's GetExcelSheet - and Service.GetSheet, which calls it without a language - returns the sheet in the client's
language. The owner plays with the German client. A method that reads a status or action text from such a sheet and looks
for an English phrase in it finds nothing for him, silently: HealReduction looked for "hp recovery" in the German status
texts, recognised no status, and the area heals counted every bearer of a lowered healing in full (concept 07, A544, A547).
The rest of the code asks for ClientLanguage.English or goes through ActionTooltip, which does.

Checked, in every C# file outside .github/, bin/ and obj/, method by method (a method is the text from one member
signature to the next): a method that gets a sheet without a language (GetSheet< or GetExcelSheet< on a line without
ClientLanguage), reads a row's text (.Description or .Name with ExtractText) and compares a string with a literal
(.Contains(" ... ")) is a finding.
NOT SEEN: a text handed from such a method to another that compares it - the check reads one method at a time.

Usage: python3 .github/scripts/audit/check_sheet_language.py [--self-test]
"""

import re
import sys
from pathlib import Path

SKIPPED = ('.github', 'bin', 'obj')
SIGNATURE = re.compile(r'^\t(?:(?:public|private|internal|protected|static|unsafe|override|virtual|async|partial|sealed|new)\s+)+'
                       r'[\w<>\[\],?. ]+\s+\w+\s*(?:<[^>]*>)?\s*\(')
SHEET = re.compile(r'\b(?:GetSheet|GetExcelSheet|GetSubrowExcelSheet)\s*<')
TEXT = re.compile(r'\.(?:Description|Name)\b[^;]*ExtractText\s*\(')
LITERAL_COMPARE = re.compile(r'\.Contains\s*\(\s*"')


def methods(lines):
    """Yields (first line number, lines) for each method-like block."""
    start, block = 0, []
    for number, line in enumerate(lines, 1):
        if SIGNATURE.match(line):
            if block:
                yield start, block
            start, block = number, [line]
        elif block:
            block.append(line)
    if block:
        yield start, block


def strip_comment(line):
    position = line.find('//')
    return line if position < 0 else line[:position]


def findings_in(text):
    found = []
    for start, block in methods(text.splitlines()):
        code = [strip_comment(line) for line in block]
        unlanguaged = any(SHEET.search(line) and 'ClientLanguage' not in line for line in code)
        reads_text = any(TEXT.search(line) for line in code)
        compares = any(LITERAL_COMPARE.search(line) for line in code)
        if unlanguaged and reads_text and compares:
            found.append(start)
    return found


def self_test():
    defect = (
        "\tprivate static HashSet<uint> BuildReducing()\n"
        "\t{\n"
        "\t\tvar sheet = Service.GetSheet<Lumina.Excel.Sheets.Status>();\n"
        "\t\tforeach (var row in sheet)\n"
        "\t\t{\n"
        "\t\t\tvar text = row.Description.ExtractText().ToLowerInvariant();\n"
        "\t\t\tif (text.Contains(\"hp recovery\")) { }\n"
        "\t\t}\n"
        "\t}\n")
    fixed = defect.replace("Service.GetSheet<Lumina.Excel.Sheets.Status>()",
                           "Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(ClientLanguage.English)")
    search = (
        "\tprivate void DrawList(string filter)\n"
        "\t{\n"
        "\t\tvar sheet = Service.GetSheet<Lumina.Excel.Sheets.Status>();\n"
        "\t\tvar name = sheet.GetRow(1).Name.ExtractText();\n"
        "\t\tif (name.Contains(filter)) { }\n"
        "\t}\n")
    commented = defect.replace("text.Contains(\"hp recovery\")", "true) { } // text.Contains(\"hp recovery\"")
    cases = [
        (defect, 1, 'a German client reads its own texts against an English phrase'),
        (fixed, 0, 'the English sheet'),
        (search, 0, 'a search with the player\'s own text'),
        (commented, 0, 'a phrase only in a comment'),
    ]
    for text, expected, what in cases:
        got = len(findings_in(text))
        if got != expected:
            print(f'SELF-TEST FAILED: {what}: {got} finding(s), expected {expected}')
            return False
    print(f'self-test ok: {len(cases)} cases - a client-language sheet compared with an English phrase is caught; the '
          'English sheet, a search with the player\'s text and a comment are left alone')
    return True


def main():
    if not self_test():
        return 1
    if '--self-test' in sys.argv:
        return 0

    root = Path(__file__).resolve().parents[3]
    findings = []
    for path in sorted(root.rglob('*.cs')):
        relative = path.relative_to(root)
        if any(part in SKIPPED for part in relative.parts) or path.name.endswith('.g.cs'):
            continue
        text = path.read_text(encoding='utf-8-sig', errors='replace')
        for line in findings_in(text):
            findings.append(f'{relative}:{line}')

    for finding in findings:
        print(f'CLIENT LANGUAGE  {finding}: reads a sheet text in the client\'s language and compares it with an English '
              'phrase - ask for ClientLanguage.English or use ActionTooltip')
    print(f'{len(findings)} method(s) comparing a client-language sheet text with an English phrase.')
    return 1 if findings else 0


if __name__ == '__main__':
    sys.exit(main())

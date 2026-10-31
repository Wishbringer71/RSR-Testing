#!/usr/bin/env python3
"""Does latest.zip hold the plugin, and only the plugin?

DalamudPackager zips every file below the output path unless it is given a list (its source, CreateZip). Two builds
put more there than the plugin: the release workflow builds with an output folder, which also receives the source
generator's dll, deps and pdb, and a local build keeps the files of earlier builds in bin/Release - the
RotationSolver.Basic.dll of before upstream 7.5.7.11 among them. Both reached a latest.zip (the release asset of
7.5.7.13+wsh1 carried the generator; his Visual Studio zip carried Basic; A474). RotationSolver/DalamudPackager.targets
now gives the packager the list; this check holds a built zip against that same list, and the list against the
libraries PruneOutputDlls keeps in RotationSolver.csproj, so a library added to the plugin without the list, or a list
that drifts from what the build leaves, fails the build that made it.

Usage: python3 .github/scripts/audit/check_plugin_zip.py <latest.zip> [--self-test]
Exit code 1 when the zip holds a file the list does not name or lacks one it names.
"""
import io
import re
import sys
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
TARGETS = ROOT / 'RotationSolver' / 'DalamudPackager.targets'
PROJECT = ROOT / 'RotationSolver' / 'RotationSolver.csproj'
ASSEMBLY = 'RotationSolver'
LIST = re.compile(r'<RsrPluginFiles>([^<]+)</RsrPluginFiles>')
PRUNE = re.compile(r'<Target Name="PruneOutputDlls".*?</Target>', re.S)
KEPT = re.compile(r"'%\(Filename\)' != '([^']+)'")


def expected(text, assembly=ASSEMBLY):
    match = LIST.search(text)
    if not match:
        raise SystemExit(f'{TARGETS}: no <RsrPluginFiles> list found')
    return {name.strip().replace('$(AssemblyName)', assembly) for name in match.group(1).split(';') if name.strip()}


def kept_libraries(project_text):
    """The libraries PruneOutputDlls keeps beside the plugin assembly: every name its condition spares."""
    target = PRUNE.search(project_text)
    if not target:
        raise SystemExit(f'{PROJECT}: no PruneOutputDlls target found')
    return {f'{name}.dll' for name in KEPT.findall(target.group(0))}


def list_misses_libraries(wanted, project_text):
    """Libraries the build keeps that the packager's list does not name - they would be missing from every zip."""
    return sorted(kept_libraries(project_text) - wanted)


def difference(names, wanted):
    """(files the zip holds that the list does not name, files the list names that the zip lacks)."""
    return sorted(set(names) - wanted), sorted(wanted - set(names))


def check(zip_path, wanted, quiet=False):
    with zipfile.ZipFile(zip_path) as archive:
        names = [info.filename for info in archive.infolist() if not info.is_dir()]
    extra, missing = difference(names, wanted)
    if quiet:
        return not extra and not missing
    for name in extra:
        print(f'{zip_path}: holds {name}, which is no part of the plugin')
    for name in missing:
        print(f'{zip_path}: lacks {name}')
    return not extra and not missing


def self_test():
    wanted = expected('<RsrPluginFiles>$(AssemblyName).dll;$(AssemblyName).json;ECommons.dll</RsrPluginFiles>')
    assert wanted == {'RotationSolver.dll', 'RotationSolver.json', 'ECommons.dll'}, wanted

    def make(names):
        buffer = io.BytesIO()
        with zipfile.ZipFile(buffer, 'w') as archive:
            for name in names:
                archive.writestr(name, b'x')
        buffer.seek(0)
        return buffer

    assert check(make(['RotationSolver.dll', 'RotationSolver.json', 'ECommons.dll']), wanted, quiet=True)
    # The two defects the list was made for: a stale Basic and the generator.
    assert not check(make(['RotationSolver.dll', 'RotationSolver.json', 'ECommons.dll', 'RotationSolver.Basic.dll']), wanted, quiet=True)
    assert not check(make(['RotationSolver.dll', 'RotationSolver.json', 'ECommons.dll',
                           'RotationSolver.SourceGenerators.dll']), wanted, quiet=True)
    # A list that names a file the build no longer leaves.
    assert not check(make(['RotationSolver.dll', 'RotationSolver.json']), wanted, quiet=True)
    # A library PruneOutputDlls keeps but the list forgets.
    prune = ('<Target Name="PruneOutputDlls"><ItemGroup><ReferenceCopyLocalPaths Remove="x" Condition="'
             "'%(Filename)' != 'ECommons' and '%(Filename)' != 'NewLib'\" /></ItemGroup></Target>")
    assert list_misses_libraries(wanted, prune) == ['NewLib.dll'], list_misses_libraries(wanted, prune)
    # The real list parses, names the plugin assembly and every library the project keeps.
    real = expected(TARGETS.read_text(encoding='utf-8'))
    assert 'RotationSolver.dll' in real and 'RotationSolver.json' in real, real
    assert not list_misses_libraries(real, PROJECT.read_text(encoding='utf-8-sig')), 'the list misses a kept library'
    print('self-test ok: an exact zip passes; a stale Basic, the generator, a missing file and a library the list '
          f'forgets are found; the list names {len(real)} files')


def main():
    self_test()
    paths = [arg for arg in sys.argv[1:] if not arg.startswith('--')]
    if '--self-test' in sys.argv or not paths:
        return 0
    wanted = expected(TARGETS.read_text(encoding='utf-8'))
    missed = list_misses_libraries(wanted, PROJECT.read_text(encoding='utf-8-sig'))
    for name in missed:
        print(f'{TARGETS}: PruneOutputDlls keeps {name}, the list does not name it')
    ok = not missed and all(check(path, wanted) for path in paths)
    if ok:
        print(f'{", ".join(paths)}: exactly the {len(wanted)} files of the plugin')
    return 0 if ok else 1


if __name__ == '__main__':
    sys.exit(main())

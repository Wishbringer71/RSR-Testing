#!/bin/bash
# Concept 24, "Referenzen fertig gebauter Bibliotheken gegen Dalamud" (A972): every reference of ECommons, RebornMaterial and
# RotationSolver into another assembly, checked against the Dalamud libraries by name and signature. Run after a sync that
# changes ECommons, RebornMaterial or the Dalamud version (skill upstream-sync, step 5), after cloud_build.sh.
#
#   .github/tools/refscan/run.sh               self-test, then the three libraries in bin/Release
#   .github/tools/refscan/run.sh --self-test   only the self-test
set -euo pipefail
root="$(cd "$(dirname "$0")/../../.." && pwd)"
here="$root/.github/tools/refscan"
export PATH="$HOME/.dotnet:$PATH" DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
dalamud="${DALAMUD_HOME:-$HOME/.xlcore/dalamud/Hooks/dev}"
work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT

dotnet build -c Release "$here/refscan.csproj" -nologo -v q -o "$work/tool" >/dev/null
scan="$work/tool/refscan.dll"

# Self-test: LibB calls eight members of LibA, among them an inherited one, a nested type's, a generic method and a generic
# type's; the second LibA no longer has Foo(int). Against the first nothing is missing and nothing is left unchecked,
# against the second exactly Foo.
for v in a1 a2; do cp -r "$here/selftest/$v" "$work/$v"; dotnet build -c Release "$work/$v/LibA.csproj" -nologo -v q -o "$work/$v/out" >/dev/null; done
cp -r "$here/selftest/b" "$work/b"
dotnet build -c Release "$work/b/LibB.csproj" -nologo -v q -o "$work/b/out" "-p:LibAPath=$work/a1/out/LibA.dll" >/dev/null
good="$(dotnet "$scan" "$work/b/out/LibB.dll" "$work/a1/out/LibA.dll" || true)"
bad="$(dotnet "$scan" "$work/b/out/LibB.dll" "$work/a2/out/LibA.dll" || true)"
[[ "$good" == *"8 member references into 1 provider assemblies checked, 0 unresolved, 0 with a parent not checked"* ]] \
	|| { echo "self-test failed: a complete library was not read clean:"; echo "$good"; exit 1; }
[[ "$bad" == *"1 unresolved"* && "$bad" == *"MISSING [LibA] LibA.C::Foo Void (Int32) g0"* ]] \
	|| { echo "self-test failed: the removed method was not the one finding:"; echo "$bad"; exit 1; }
echo "self-test ok: eight references across inheritance, nesting and generics resolve; a removed method is the one finding"
[ "${1:-}" = "--self-test" ] && exit 0

status=0
for lib in RebornMaterial ECommons RotationSolver; do
	dotnet "$scan" "$root/bin/Release/$lib.dll" "$dalamud" "$root/bin/Release" || status=1
done
exit $status

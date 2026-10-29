#!/bin/bash
# Runs the revision store's C# code outside the game against random runs of readings, saves, loads, patches, unreadable
# versions and files, deletions by hand and sessions of an older RSR - the runs of learned_store_model.py, against the
# code instead of the design (concept 13, "Stufe 1"). Needs the .NET SDK 10 and Newtonsoft.Json from the Dalamud
# distribution (.github/scripts/cloud_build.sh --fetch); not part of the CI, which has neither on its check runner.
#
#   run.sh               the runs; fails on a broken check
#   run.sh --self-test   each defect in prepare.py's MUTANTS put in: fails when one goes unseen
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
export PATH="$HOME/.dotnet:$PATH" DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT
cp "$here/revtest.csproj" "$here/Stubs.cs" "$here/Program.cs" "$work/"

build() {
	python3 "$here/prepare.py" "$work" "$@"
	dotnet build "$work/revtest.csproj" -c Release -v q -o "$work/bin" >"$work/build.log" 2>&1 || { cat "$work/build.log"; exit 2; }
}

if [ "${1:-}" = "--self-test" ]; then
	count="$(python3 -c "import sys; sys.path.insert(0, '$here'); import prepare; print(len(prepare.MUTANTS))")"
	for ((i = 0; i < count; i++)); do
		build --mutant "$i"
		if dotnet "$work/bin/revtest.dll" 150 >/dev/null; then
			echo "self-test FAILED: mutant $i went unseen"
			exit 1
		fi
	done
	echo "self-test ok: each of $count defects is caught"
	exit 0
fi

build
dotnet "$work/bin/revtest.dll" 300

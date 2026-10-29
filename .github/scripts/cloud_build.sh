#!/bin/bash
# Builds the plugin in a Linux cloud session, so a compiler error shows in the session instead of on the Windows
# runner of the CI. A fresh session has neither .NET nor Dalamud; this fetches what is missing - the .NET SDK 10 into
# $HOME/.dotnet, the Dalamud distribution into $HOME/.xlcore/dalamud/Hooks/dev, where the projects look on Linux
# (DalamudLibPath) - and builds RotationSolver in Release.
# Done by hand since A331 (22.10.2026); written down after Retainer-Repricer's tools/cloud-build.sh (27.10.2026, A423).
#
#   .github/scripts/cloud_build.sh            fetch what is missing, build
#   .github/scripts/cloud_build.sh --fetch    only fetch
set -euo pipefail
root="$(cd "$(dirname "$0")/../.." && pwd)"
export PATH="$HOME/.dotnet:$PATH" DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
dalamud="${DALAMUD_HOME:-$HOME/.xlcore/dalamud/Hooks/dev}"
work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT

if ! command -v dotnet >/dev/null || ! dotnet --list-sdks | grep -q '^10\.'; then
	echo "fetching the .NET SDK 10 into $HOME/.dotnet"
	curl -sSL https://dot.net/v1/dotnet-install.sh -o "$work/dotnet-install.sh"
	bash "$work/dotnet-install.sh" --channel 10.0 --install-dir "$HOME/.dotnet" >/dev/null
fi
if [ ! -f "$dalamud/Dalamud.dll" ]; then
	echo "fetching the Dalamud distribution into $dalamud"
	mkdir -p "$dalamud"
	curl -sSL https://goatcorp.github.io/dalamud-distrib/latest.zip -o "$work/dalamud.zip"
	unzip -q -o "$work/dalamud.zip" -d "$dalamud"
fi
echo "dotnet $(dotnet --version), Dalamud in $dalamud"
[ "${1:-}" = "--fetch" ] && exit 0

cd "$root"
dotnet build -c Release RotationSolver/RotationSolver.csproj "-p:SolutionDir=$root/" -nologo -v q

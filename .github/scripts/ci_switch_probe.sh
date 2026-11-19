#!/bin/bash
# Self-test of a compiler switch of the CI build (build.yaml, A893): the probe must build without the switch -
# else a failure with it proves nothing - and must fail with it, naming each expected error code.
#
#   ci_switch_probe.sh <probe dir> "<switch arguments>" <code>...
set -uo pipefail
probe="$1"; switch="$2"; shift 2
if ! dotnet build "$probe" -nologo; then
	echo "The probe does not build even without the switch; the self-test measures nothing."; exit 1
fi
# shellcheck disable=SC2086 # the switch arguments are words on purpose
out="$(dotnet build "$probe" -nologo --no-incremental $switch 2>&1)"; status=$?
echo "$out"
if [ "$status" -eq 0 ]; then echo "The switch '$switch' let the probe through."; exit 1; fi
for code in "$@"; do
	if ! grep -q "error $code" <<<"$out"; then echo "The probe failed, but not with error $code."; exit 1; fi
done
echo "The switch '$switch' fails the probe with $*."

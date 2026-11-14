# Sourced by pre-push: sets PYTHON to a Python that actually starts, else leaves it empty. On Windows "python3" may be
# missing or a Store placeholder that fails at once, and the hook would refuse the push for that, not for an identity
# (procedure from Retainer-Repricer, 14.11.2026; A792).
PYTHON=""
for candidate in "python3" "python" "py -3"; do
	if $candidate -c "import sys" >/dev/null 2>&1; then
		PYTHON="$candidate"
		break
	fi
done

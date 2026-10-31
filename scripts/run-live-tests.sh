#!/usr/bin/env bash
# Runs the whole HoloNET test suite (with coverage) against a fresh sandbox conductor of one Holochain version.
# Usage: scripts/run-live-tests.sh 0.7.1|0.7.0 [extra dotnet test args]
# holochain.exe always runs from %APPDATA%/NextGenSoftware/HoloNET/<version>/ (the copy HoloNET's embedded mode extracts),
# so Windows Firewall only ever asks once per version.
set -u
V="$1"; shift
REPO=$(cd "$(dirname "$0")/.." && pwd)
T="$REPO/.holochain-test"
TAG=${V//./}
HC="$REPO/NextGenSoftware.Holochain.HoloNET.Client/Resources/hc.exe"
[ "$V" = "0.7.0" ] && HC="$REPO/NextGenSoftware.Holochain.HoloNET.Client/Resources/hc-0.7.0.exe"
HOLOCHAIN="$APPDATA/NextGenSoftware/HoloNET/$V/holochain.exe"
PORT=65464
SB="$T/sb-$TAG"

# A fresh sandbox per test project: after the long Client run the conductor's in-process keystore can drop its connection
# ("ConnectionAborted" on generate_agent_pub_key / install_app), which would fail the next project for reasons outside HoloNET.
start_sandbox() {
  taskkill //F //IM holochain.exe >/dev/null 2>&1
  rm -rf "$SB"; mkdir -p "$SB"
  (cd "$SB" && echo "test-passphrase" | "$HC" sandbox --piped -H "$HOLOCHAIN" create --in-process-lair --root "$(cygpath -w "$SB")" network mem >/dev/null 2>&1)
  (cd "$SB" && echo "test-passphrase" | "$HC" sandbox --piped -H "$HOLOCHAIN" -f=$PORT run 0 > "$SB/conductor.log" 2>&1 &)
  for i in $(seq 1 60); do grep -q "Conductor launched" "$SB/conductor.log" 2>/dev/null && break; sleep 1; done
  grep -q "Conductor launched" "$SB/conductor.log" || { echo "conductor did not start"; cat "$SB/conductor.log"; exit 1; }
}

export HOLONET_LIVE_ADMIN_URI=ws://localhost:$PORT
export HOLONET_LIVE_HC_BIN="$(cygpath -w "$HC")"
export HOLONET_LIVE_HAPP_PATH="${HAPP_OVERRIDE:-$(cygpath -w "$T/happ-$TAG/oasis.happ")}"
export HOLONET_LIVE_GENERATED_HAPP_DIR="$(cygpath -w "$T/happ-$TAG-generated")"   # scripts/build-generated-happ.sh
export HOLONET_LIVE_GENERATED_SRC_DIR="$(cygpath -w "$T/generated-$TAG-src")"
export HOLONET_LIVE_CLONE_HAPP_PATH="$(cygpath -w "$T/happ-$TAG-clone/oasis-clone.happ")"
export HOLONET_LIVE_MEMPROOFS_HAPP_PATH="$(cygpath -w "$T/happ-$TAG-memproofs/oasis-memproofs.happ")"
filter() { if [ -n "${LIVE_VERBOSE:-}" ]; then grep -v "warning CS"; else grep -E "^\s*Failed |Error Message|Passed!|Failed!|error CS" -A1 | grep -v "^--"; fi; }

# HoloNET Client, ORM and API (one test project), then HoloNET Manager (WPF, its own project), then every example in
# examples/ (each must run to completion against the conductor). PROJECTS=client, manager or examples runs just one.
RC=0
P=${PROJECTS:-all}
if [ "$P" = "all" ] || [ "$P" = "client" ]; then
  start_sandbox
  (cd "$REPO/NextGenSoftware.Holochain.HoloNET.Client.Tests" && dotnet test -nologo --collect "XPlat Code Coverage" --results-directory "$T/coverage-$TAG" "$@" 2>&1 | filter; exit ${PIPESTATUS[0]}) || RC=1
fi
if [ "$P" = "all" ] || [ "$P" = "manager" ]; then
  start_sandbox
  (cd "$REPO/NextGenSoftware.Holochain.HoloNET.Manager.Tests" && dotnet test -nologo --collect "XPlat Code Coverage" --results-directory "$T/coverage-$TAG" "$@" 2>&1 | filter; exit ${PIPESTATUS[0]}) || RC=1
fi

if [ "$P" = "all" ] || [ "$P" = "examples" ]; then
  start_sandbox
  for ex in "$REPO"/examples/[0-9]*/; do
    [ -f "$ex/.no-auto-run" ] && continue   # e.g. the web site example, which runs until stopped (it has its own smoke test mode)
    name=$(basename "$ex")
    out=$(cd "$ex" && HOLONET_EXAMPLE_SMOKE=1 timeout 600 dotnet run 2>&1); code=$?
    [ -n "${EXAMPLES_VERBOSE:-}" ] && echo "$out"
    if [ $code -eq 0 ]; then echo "Example $name: passed"; else echo "Example $name: FAILED (exit $code)"; echo "$out" | tail -25; RC=1; fi
  done
fi

taskkill //F //IM holochain.exe >/dev/null 2>&1
exit $RC

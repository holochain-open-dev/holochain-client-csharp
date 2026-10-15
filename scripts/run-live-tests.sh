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

taskkill //F //IM holochain.exe >/dev/null 2>&1
rm -rf "$SB"; mkdir -p "$SB"
cd "$SB"
echo "test-passphrase" | "$HC" sandbox --piped -H "$HOLOCHAIN" create --in-process-lair --root "$(cygpath -w "$SB")" >/dev/null 2>&1
(echo "test-passphrase" | "$HC" sandbox --piped -H "$HOLOCHAIN" -f=$PORT run 0 > "$SB/conductor.log" 2>&1 &)
for i in $(seq 1 60); do grep -q "Conductor launched" "$SB/conductor.log" 2>/dev/null && break; sleep 1; done
grep -q "Conductor launched" "$SB/conductor.log" || { echo "conductor did not start"; cat "$SB/conductor.log"; exit 1; }

cd "$REPO/NextGenSoftware.Holochain.HoloNET.Client.Tests"
HOLONET_LIVE_ADMIN_URI=ws://localhost:$PORT \
HOLONET_LIVE_HAPP_PATH="$(cygpath -w "$T/happ-$TAG/oasis.happ")" \
HOLONET_LIVE_CLONE_HAPP_PATH="$(cygpath -w "$T/happ-$TAG-clone/oasis-clone.happ")" \
HOLONET_LIVE_MEMPROOFS_HAPP_PATH="$(cygpath -w "$T/happ-$TAG-memproofs/oasis-memproofs.happ")" \
dotnet test -nologo --collect "XPlat Code Coverage" --results-directory "$T/coverage-$TAG" "$@" 2>&1 | grep -E "^\s*Failed |Error Message|Passed!|Failed!|error CS" -A1 | grep -v "^--"
RC=${PIPESTATUS[0]}

taskkill //F //IM holochain.exe >/dev/null 2>&1
exit $RC

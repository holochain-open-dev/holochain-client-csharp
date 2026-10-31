#!/usr/bin/env bash
# Starts a throwaway Holochain conductor for the HoloNET examples, with its admin interface on ws://localhost:7777
# (HoloNET's default). Uses the hc / holochain binaries bundled with HoloNET on Windows, or hc / holochain on your PATH
# elsewhere (https://developer.holochain.org/get-started). Ctrl+C stops it; the sandbox is deleted on the next start.
#
# Usage: examples/start-conductor.sh [0.7.1|0.7.0] [port]
set -e
V=${1:-0.7.1}
PORT=${2:-7777}
HERE="$(cd "$(dirname "$0")" && pwd)"
RES="$HERE/../NextGenSoftware.Holochain.HoloNET.Client/Resources"
SB="$HERE/.sandbox-${V//./}"

if [ -f "$RES/hc.exe" ]; then
  if [ "$V" = "0.7.0" ]; then HC="$RES/hc-0.7.0.exe"; HOLOCHAIN="$RES/holochain-0.7.0.exe"; else HC="$RES/hc.exe"; HOLOCHAIN="$RES/holochain.exe"; fi
  W() { cygpath -w "$1" 2>/dev/null || echo "$1"; }
else
  HC=$(command -v hc) || { echo "hc not found: install Holochain $V (https://developer.holochain.org/get-started)"; exit 1; }
  HOLOCHAIN=$(command -v holochain) || { echo "holochain not found"; exit 1; }
  W() { echo "$1"; }
fi

rm -rf "$SB"; mkdir -p "$SB"
cd "$SB"
# An in-memory network: everything stays on this machine (agents on this conductor still reach each other).
echo "examples" | "$HC" sandbox --piped -H "$(W "$HOLOCHAIN")" create --in-process-lair --root "$(W "$SB")" network mem >/dev/null
echo "Holochain $V conductor starting; admin interface ws://localhost:$PORT (Ctrl+C to stop)."
echo "In another terminal:  dotnet run --project examples/01-Client"
echo "examples" | "$HC" sandbox --piped -H "$(W "$HOLOCHAIN")" -f=$PORT run 0

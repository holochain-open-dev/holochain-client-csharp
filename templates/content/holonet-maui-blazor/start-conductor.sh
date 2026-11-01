#!/usr/bin/env bash
# Starts a throwaway Holochain conductor with its admin interface on ws://localhost:7777 (HoloNET's default).
# Needs the hc and holochain binaries on your PATH: https://developer.holochain.org/get-started  (Holochain 0.7.0 or 0.7.1)
# Ctrl+C stops it; the sandbox folder is recreated on the next start.
set -e
PORT=${1:-7777}
HC=$(command -v hc) || { echo "hc not found: install Holochain 0.7.x (https://developer.holochain.org/get-started)"; exit 1; }
HOLOCHAIN=$(command -v holochain) || { echo "holochain not found"; exit 1; }
SB="$(cd "$(dirname "$0")" && pwd)/.sandbox"
rm -rf "$SB"; mkdir -p "$SB"; cd "$SB"
# An in-memory network: everything stays on this machine.
echo "app" | "$HC" sandbox --piped -H "$HOLOCHAIN" create --in-process-lair --root "$SB" network mem >/dev/null
echo "Holochain conductor starting; admin interface ws://localhost:$PORT (Ctrl+C to stop). In another terminal:  dotnet run"
echo "app" | "$HC" sandbox --piped -H "$HOLOCHAIN" -f=$PORT run 0

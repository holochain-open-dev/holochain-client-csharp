#!/usr/bin/env bash
# Generates the sample game hApp with HoloNET ORM's HolochainHappGenerator (what WEB5 STAR uses), compiles it with cargo
# (in WSL, wasm32 target) and packs it with the bundled hc, into .holochain-test/happ-<ver>-generated/holonet_game.happ.
# The live tests (LiveGeneratedHappTests) then drive it with HoloNET ORM on a real conductor.
# Usage: scripts/build-generated-happ.sh 0.7.1|0.7.0
set -e
V=${1:-0.7.1}
TAG=${V//./}
REPO="$(cd "$(dirname "$0")/.." && pwd)"
T="$REPO/.holochain-test"
SRC="$T/generated-$TAG-src"
D="$T/happ-$TAG-generated"
HC="$REPO/NextGenSoftware.Holochain.HoloNET.Client/Resources/hc.exe"

rm -rf "$SRC" "$D"
HOLONET_GENERATE_HAPP_TO="$(cygpath -w "$SRC")" HOLONET_GENERATE_HAPP_VERSION="$V" \
  dotnet test "$REPO/NextGenSoftware.Holochain.HoloNET.Client.Tests" --filter "FullyQualifiedName~WriteSampleHappForTheBuildScript" 2>&1 | grep -E "Passed!|Failed!|error" | tail -1
[ -f "$SRC/Cargo.toml" ] || { echo "generation failed"; exit 1; }

WSRC=$(wsl -e wslpath -a "$(cygpath -w "$SRC")" | tr -d '\r')
wsl -e bash -lc "cd '$WSRC' && CARGO_TARGET_DIR=\$HOME/holonet-generated-target-$TAG CARGO_TARGET_WASM32_UNKNOWN_UNKNOWN_LINKER=\$HOME/bin/wasm-ld RUSTFLAGS='--cfg getrandom_backend=\"custom\"' cargo build --release --target wasm32-unknown-unknown 2>&1 | grep -E '^(error|warning: unused)|Finished' | head -20"
W=$(wsl -e bash -lc "wslpath -w \$HOME/holonet-generated-target-$TAG/wasm32-unknown-unknown/release" | tr -d '\r')
[ -f "$W\\game.wasm" ] || { echo "cargo build failed"; exit 1; }

mkdir -p "$D"
cp "$W\\game.wasm" "$W\\game_integrity.wasm" "$D/"
sed -e 's|"../../../target/wasm32-unknown-unknown/release/|"|' "$SRC/dnas/holonet_game/workdir/dna.yaml" > "$D/dna.yaml"
sed -e 's|path: "../dnas/holonet_game/workdir/holonet_game.dna"|path: "holonet_game.dna"|' "$SRC/workdir/happ.yaml" > "$D/happ.yaml"
(cd "$D" && "$HC" dna pack . | tail -1 && "$HC" app pack . | tail -1)
rm -f "$D"/*.wasm
wsl -e bash -lc "rm -rf \$HOME/holonet-generated-target-$TAG"
ls "$D"/*.happ

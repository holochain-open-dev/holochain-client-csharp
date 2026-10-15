#!/usr/bin/env bash
# Builds the OASIS test hApps for one Holochain version into .holochain-test/happ-<ver>{,-clone,-memproofs}.
# Usage: scripts/build-test-happs.sh 0.7.1|0.7.0
#   0.7.1 builds OASIS-Holochain-hApp main; 0.7.0 builds its holochain-0.7.0-maintenance branch (the holochain-0.7.0 tag plus fixes) (via a temporary git worktree).
# The zomes are compiled in WSL (wasm32 toolchain); the target dir lives in WSL and is removed afterwards.
set -eu
V="$1"; TAG=${V//./}
REPO=$(cd "$(dirname "$0")/.." && pwd)
HAPP_SRC=/c/Source/OASIS-Holochain-hApp
T="$REPO/.holochain-test"
HC="$REPO/NextGenSoftware.Holochain.HoloNET.Client/Resources/hc.exe"
[ "$V" = "0.7.0" ] && HC="$REPO/NextGenSoftware.Holochain.HoloNET.Client/Resources/hc-0.7.0.exe"

SRC="$HAPP_SRC"
if [ "$V" = "0.7.0" ]; then
  SRC="$T/happ-src-070"
  git -C "$HAPP_SRC" worktree remove --force "$SRC" 2>/dev/null || true
  git -C "$HAPP_SRC" worktree add --detach "$SRC" holochain-0.7.0-maintenance >/dev/null
fi
WSRC=$(wsl -e wslpath -a "$(cygpath -w "$SRC")" | tr -d '\r')
wsl -e bash -lc "cd '$WSRC' && CARGO_TARGET_DIR=\$HOME/oasis-target-$TAG CARGO_TARGET_WASM32_UNKNOWN_UNKNOWN_LINKER=\$HOME/bin/wasm-ld RUSTFLAGS='--cfg getrandom_backend=\"custom\"' cargo build --release --target wasm32-unknown-unknown -p oasis -p oasis_integrity 2>&1 | tail -1"
W=$(wsl -e bash -lc "wslpath -w \$HOME/oasis-target-$TAG/wasm32-unknown-unknown/release" | tr -d '\r')

D="$T/happ-$TAG"
mkdir -p "$D"
cp "$W\oasis.wasm" "$W\oasis_integrity.wasm" "$D/"
sed -e 's|"../../../target/wasm32-unknown-unknown/release/|"|' "$SRC/dnas/oasis/workdir/dna.yaml" > "$D/dna.yaml"
sed -e 's|path: "../dnas/oasis/workdir/oasis.dna"|path: "oasis.dna"|' "$SRC/workdir/happ.yaml" > "$D/happ.yaml"
(cd "$D" && "$HC" dna pack . | tail -1 && "$HC" app pack . | tail -1)
for x in clone memproofs; do
  d="$T/happ-$TAG-$x"; mkdir -p "$d"; cp "$D/oasis.dna" "$d/"
  sed -e "0,/^name: oasis$/s//name: oasis-$x/" "$D/happ.yaml" > "$d/happ.yaml"  # the app name only, not the role
done
sed -i 's/clone_limit: 0/clone_limit: 3/' "$T/happ-$TAG-clone/happ.yaml"
sed -i 's/allow_deferred_memproofs: false/allow_deferred_memproofs: true/' "$T/happ-$TAG-memproofs/happ.yaml"
for x in clone memproofs; do (cd "$T/happ-$TAG-$x" && "$HC" app pack . | tail -1); done
rm -f "$D"/*.wasm
wsl -e bash -lc "rm -rf \$HOME/oasis-target-$TAG"
[ "$V" = "0.7.0" ] && git -C "$HAPP_SRC" worktree remove --force "$SRC"
ls "$T"/happ-$TAG*/*.happ

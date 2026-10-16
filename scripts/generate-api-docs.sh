#!/usr/bin/env bash
# Regenerates docs/api/*.md (every public type and member of Client, ORM, API and HyperNET, with their XML docs, falling
# back to a documented overload / sync-async twin / the README's member tables).
set -e
REPO="$(cd "$(dirname "$0")/.." && pwd)"
BIN="$(mktemp -d)"
trap 'rm -rf "$BIN"' EXIT
build() { dotnet build "$REPO/$1" -c Release -f "$2" -p:GenerateDocumentationFile=true -p:NoWarn=CS1591 -p:CopyLocalLockFileAssemblies=true -o "$(cygpath -w "$BIN" 2>/dev/null || echo "$BIN")" 2>&1 | grep -E " error |Build succeeded" | sort -u | head -3; }
build NextGenSoftware.Holochain.HoloNET.Client/NextGenSoftware.Holochain.HoloNET.Client.csproj net10.0
build NextGenSoftware.Holochain.HoloNET.ORM/NextGenSoftware.Holochain.HoloNET.ORM.csproj net10.0
build HoloNET-API/NextGenSoftware.Holochain.HoloNET.API/NextGenSoftware.Holochain.HoloNET.API.csproj net8.0
build NextGenSoftware.Holochain.HoloNET.HyperNET/NextGenSoftware.Holochain.HoloNET.HyperNET.csproj net10.0
W() { cygpath -w "$1" 2>/dev/null || echo "$1"; }
APIGEN_README="$(W "$REPO/README.md")" dotnet run --project "$REPO/scripts/apigen" -c Release -- "$(W "$BIN")" "$(W "$REPO/docs/api")" \
  NextGenSoftware.Holochain.HoloNET.Client "HoloNET Client" NextGenSoftware.Holochain.HoloNET.ORM "HoloNET ORM" \
  NextGenSoftware.Holochain.HoloNET.API "HoloNET API" NextGenSoftware.Holochain.HoloNET.HyperNET "HoloNET HyperNET"

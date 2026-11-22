# Starts a throwaway Holochain conductor with its admin interface on ws://localhost:7777 (HoloNET's default).
# Needs the hc and holochain binaries on your PATH: https://developer.holochain.org/get-started  (Holochain 0.7.0 or 0.7.1)
# Ctrl+C stops it; the sandbox folder is recreated on the next start.
param([int]$Port = 7777)
$ErrorActionPreference = 'Stop'
$hc = (Get-Command hc -ErrorAction Stop).Source
$holochain = (Get-Command holochain -ErrorAction Stop).Source
$sandbox = Join-Path $PSScriptRoot '.sandbox'
if (Test-Path $sandbox) { Remove-Item -Recurse -Force $sandbox }
New-Item -ItemType Directory -Path $sandbox | Out-Null
Push-Location $sandbox
try {
    # An in-memory network: everything stays on this machine.
    'app' | & $hc sandbox --piped -H $holochain create --in-process-lair --root $sandbox network mem | Out-Null
    Write-Host "Holochain conductor starting; admin interface ws://localhost:$Port (Ctrl+C to stop). In another terminal:  dotnet run"
    'app' | & $hc sandbox --piped -H $holochain -f=$Port run 0
}
finally { Pop-Location }

# Starts a throwaway Holochain conductor for the HoloNET examples, with its admin interface on ws://localhost:7777
# (HoloNET's default), using the hc / holochain binaries bundled with HoloNET. Ctrl+C stops it.
#
# Usage: .\examples\start-conductor.ps1 [-Version 0.7.1|0.7.0] [-Port 7777]
param([ValidateSet('0.7.1', '0.7.0')][string]$Version = '0.7.1', [int]$Port = 7777)
$ErrorActionPreference = 'Stop'
$res = Join-Path $PSScriptRoot '..\NextGenSoftware.Holochain.HoloNET.Client\Resources'
$hc = if ($Version -eq '0.7.0') { Join-Path $res 'hc-0.7.0.exe' } else { Join-Path $res 'hc.exe' }
$holochain = if ($Version -eq '0.7.0') { Join-Path $res 'holochain-0.7.0.exe' } else { Join-Path $res 'holochain.exe' }
$sandbox = Join-Path $PSScriptRoot (".sandbox-" + $Version.Replace('.', ''))

if (Test-Path $sandbox) { Remove-Item -Recurse -Force $sandbox }
New-Item -ItemType Directory -Path $sandbox | Out-Null
Push-Location $sandbox
try {
    # An in-memory network: everything stays on this machine (agents on this conductor still reach each other).
    'examples' | & $hc sandbox --piped -H $holochain create --in-process-lair --root $sandbox network mem | Out-Null
    Write-Host "Holochain $Version conductor starting; admin interface ws://localhost:$Port (Ctrl+C to stop)."
    Write-Host "In another terminal:  dotnet run --project examples/01-Client"
    'examples' | & $hc sandbox --piped -H $holochain -f=$Port run 0
}
finally { Pop-Location }

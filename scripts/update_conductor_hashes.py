#!/usr/bin/env python3
"""Rebuilds NextGenSoftware.Holochain.HoloNET.Client/Resources/ConductorHashes.json: the SHA-256 of every conductor (holochain) and dev tool (hc) binary
in Holochain's stable releases, as GitHub publishes them. HolochainConductorDownloader checks every download against this table.

    python scripts/update_conductor_hashes.py            (needs the GitHub CLI, logged in)

Run it when Holochain publishes a release you want HoloNET to download without relying on GitHub's digest at run time.
"""
import json, os, re, subprocess, sys

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'NextGenSoftware.Holochain.HoloNET.Client', 'Resources', 'ConductorHashes.json')
ASSET = re.compile(r'^(holochain|hc)-(x86_64|aarch64)-(pc-windows-msvc\.exe|unknown-linux-gnu|apple-darwin)$')

raw = subprocess.run(['gh', 'api', '--paginate', 'repos/holochain/holochain/releases?per_page=100'], capture_output=True, check=True).stdout.decode('utf-8')
# --paginate prints one JSON array per page back to back
releases, dec, i = [], json.JSONDecoder(), 0
while i < len(raw):
    while i < len(raw) and raw[i].isspace():
        i += 1
    if i >= len(raw):
        break
    page, i = dec.raw_decode(raw, i)
    releases += page

table = {}
for r in releases:
    if r.get('prerelease') or r.get('draft') or not r['tag_name'].startswith('holochain-'):
        continue
    version = r['tag_name'][len('holochain-'):]
    assets = {a['name']: a['digest'][7:] for a in r['assets'] if ASSET.match(a['name']) and (a.get('digest') or '').startswith('sha256:')}
    if assets:
        table[version] = dict(sorted(assets.items()))

table = dict(sorted(table.items(), key=lambda kv: [int(x) if x.isdigit() else x for x in re.split(r'[.-]', kv[0])]))
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    json.dump(table, f, indent=1)
    f.write('\n')
print(f'{len(table)} releases, {sum(len(v) for v in table.values())} binaries -> {os.path.normpath(OUT)}')

#!/usr/bin/env python3
"""Downloads the official Holochain conductors that the Linux and macOS Embedded packages bundle, and checks every one against the SHA-256 table in
NextGenSoftware.Holochain.HoloNET.Client/Resources/ConductorHashes.json (the same table the runtime downloader uses).

  python scripts/fetch_conductors.py                 every platform and version the packages need
  python scripts/fetch_conductors.py linux-x64       one platform (linux-x64 | linux-arm64 | osx-x64 | osx-arm64)

Files land in NextGenSoftware.Holochain.HoloNET.Client/Resources/conductors/<platform>/<tool>-<version> (git-ignored: they are downloaded at pack time, not committed).
The Windows binaries are committed in Resources/ and are not handled here.
"""
import hashlib, io, json, os, sys, urllib.request

REPO = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CLIENT = os.path.join(REPO, 'NextGenSoftware.Holochain.HoloNET.Client', 'Resources')
TRIPLES = {'linux-x64': 'x86_64-unknown-linux-gnu', 'linux-arm64': 'aarch64-unknown-linux-gnu', 'osx-x64': 'x86_64-apple-darwin', 'osx-arm64': 'aarch64-apple-darwin'}
VERSIONS = ['0.7.1', '0.7.0']
TOOLS = ['holochain', 'hc']


def target(platform, tool, version):
    return os.path.join(CLIENT, 'conductors', platform, f'{tool}-{version}')


def fetch(platform):
    hashes = json.load(io.open(os.path.join(CLIENT, 'ConductorHashes.json'), encoding='utf-8'))
    for version in VERSIONS:
        for tool in TOOLS:
            asset = f'{tool}-{TRIPLES[platform]}'
            want = hashes[version][asset]
            path = target(platform, tool, version)
            if os.path.exists(path) and hashlib.sha256(open(path, 'rb').read()).hexdigest() == want:
                continue
            os.makedirs(os.path.dirname(path), exist_ok=True)
            url = f'https://github.com/holochain/holochain/releases/download/holochain-{version}/{asset}'
            print('downloading', url, flush=True)
            data = urllib.request.urlopen(url).read()
            got = hashlib.sha256(data).hexdigest()
            if got != want:
                sys.exit(f'{asset} {version}: SHA-256 is {got}, expected {want}. Not using it.')
            open(path + '.tmp', 'wb').write(data)
            os.replace(path + '.tmp', path)


if __name__ == '__main__':
    platforms = sys.argv[1:] or list(TRIPLES)
    for p in platforms:
        if p not in TRIPLES:
            sys.exit('unknown platform ' + p)
        fetch(p)
    print('OK:', ', '.join(platforms))

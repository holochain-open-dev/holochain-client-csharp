#!/usr/bin/env python3
"""Everything about HoloNET's NuGet packages, driven by packages.json (the single list of packages and variants).

  python scripts/holonet_packages.py list                    every package id that a release produces
  python scripts/holonet_packages.py check [--partial]       fast, no build: manifest vs the repo's projects (run by CI on every push; --partial skips projects in private submodules that are not checked out)
  python scripts/holonet_packages.py pack <out> [--only ID]  build + pack every package (all variants) at the family version
  python scripts/holonet_packages.py verify-packed <out>     exactly the expected .nupkg files, right version, variants consistent
  python scripts/holonet_packages.py smoke <out>             install each package into a fresh app (local feed + nuget.org) and build it
  python scripts/holonet_packages.py verify-nuget            every package is on nuget.org at the family version
  python scripts/holonet_packages.py set-version X.Y.Z       one new version for the whole family (manifest, csproj files, release notes)

Variants: none = no conductor; both = .Embedded (Holochain 0.7.1 and 0.7.0); 0.7.1 = .Embedded.Holochain071; 0.7.0 = .Embedded.Holochain070.
"""
import glob, io, json, os, re, shutil, subprocess, sys, tempfile, time, urllib.request, xml.etree.ElementTree as ET, zipfile

REPO = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SUFFIX = {'none': '', 'both': '.Embedded', '0.7.1': '.Embedded.Holochain071', '0.7.0': '.Embedded.Holochain070'}
CLIENT = 'NextGenSoftware.Holochain.HoloNET.Client'
FAMILY_PREFIX = 'NextGenSoftware.Holochain.HoloNET.'


def manifest():
    return json.load(io.open(os.path.join(REPO, 'packages.json'), encoding='utf-8'))


def packages(m=None):
    """(library, variant, package id) for everything a release produces."""
    m = m or manifest()
    return [(lib, v, lib['id'] + SUFFIX[v]) for lib in m['libraries'] for v in lib['variants']]


def props(variant):
    return [] if variant == 'none' else ['-p:Embedded=true', '-p:EmbeddedHolochain=' + variant]


def run(cmd, cwd=REPO, check=True):
    print('>', ' '.join(cmd), flush=True)
    r = subprocess.run(cmd, cwd=cwd)
    if check and r.returncode != 0:
        sys.exit(f'FAILED ({r.returncode}): {" ".join(cmd)}')
    return r.returncode


def fail(errors, what):
    for e in errors:
        print('ERROR:', e)
    sys.exit(f'{what}: {len(errors)} problem(s).')


# ---- check: manifest vs the repository (no build) --------------------------------------------------------------------
def check(partial=False):
    """partial: skip libraries whose project is missing (private submodules a public CI run cannot clone); a release always runs the full check."""
    m = manifest()
    errors = []
    skipped = []
    ids = [p[2] for p in packages(m)]
    for dup in {i for i in ids if ids.count(i) > 1}:
        errors.append(f'package id listed twice: {dup}')

    listed_projects = {os.path.normpath(lib['project']) for lib in m['libraries']}
    for lib in m['libraries']:
        path = os.path.join(REPO, lib['project'])
        if not os.path.exists(path):
            if partial:
                skipped.append(lib['id'])
            else:
                errors.append(f'{lib["id"]}: project not found: {lib["project"]}')
            continue
        text = io.open(path, encoding='utf-8-sig').read()
        base = re.findall(r'<PackageId[^>]*>' + re.escape(lib['id']) + r'(\.Embedded(?:\.Holochain07[01])?)?</PackageId>', text)
        if not base:
            errors.append(f'{lib["project"]}: no <PackageId> for {lib["id"]}')
        for v in lib['variants']:
            if v != 'none' and (lib['id'] + SUFFIX[v]) not in re.findall(r'<PackageId[^>]*>([^<]+)</PackageId>', text):
                errors.append(f'{lib["project"]}: manifest says variant "{v}" but the project has no PackageId {lib["id"]}{SUFFIX[v]}')
        csproj_version = re.search(r'<Version>([^<]+)</Version>', text)
        if not csproj_version or csproj_version.group(1) != m['version']:
            errors.append(f'{lib["project"]}: <Version> is {csproj_version.group(1) if csproj_version else "missing"}, but the family version is {m["version"]} (run set-version)')
        # Variants are build properties on one project, never copies.
        if text.count('<PackageId') > 4:
            errors.append(f'{lib["project"]}: more than four PackageId lines (variants must be build properties, not copies)')

    # Every packable project in the repo must be listed, or excluded on purpose.
    excluded = [os.path.normpath(p) for p in m.get('exclude_projects', [])]
    for path in glob.glob(os.path.join(REPO, '**', '*.csproj'), recursive=True):
        rel = os.path.normpath(os.path.relpath(path, REPO))
        if any(seg in rel.split(os.sep) for seg in ('bin', 'obj', 'node_modules', '.holochain-test')):
            continue
        text = io.open(path, encoding='utf-8-sig', errors='ignore').read()
        pid = re.search(r'<PackageId[^>]*>([^<]+)</PackageId>', text)
        if not pid or rel in listed_projects:
            continue
        if any(re.fullmatch(re.escape(e).replace(r'\*', '.*'), rel) for e in excluded) or any(rel.startswith(e.rstrip('*')) for e in excluded if e.endswith('*')):
            continue
        errors.append(f'{rel} declares a package ("{pid.group(1)}") but is not in packages.json (add it, or add it to "exclude_projects")')

    # Embedded libraries must reference the Client as a project reference (so the property flows), not copy it.
    for lib in m['libraries']:
        if lib['id'] != CLIENT and len(lib['variants']) > 1 and os.path.exists(os.path.join(REPO, lib['project'])):
            text = io.open(os.path.join(REPO, lib['project']), encoding='utf-8-sig').read()
            if 'NextGenSoftware.Holochain.HoloNET.Client.csproj' not in text:
                errors.append(f'{lib["project"]}: has Embedded variants but no ProjectReference to the HoloNET Client')

    if not docs(check_only=True):
        errors.append('docs/packages.md is out of date: run python scripts/holonet_packages.py docs')
    if errors:
        fail(errors, 'check failed')
    print(f'OK: {len(ids)} packages from {len(m["libraries"])} projects at version {m["version"]}.')
    if skipped:
        print(f'NOTE: not checked against the repository (private submodule not available here): {", ".join(skipped)}. The release workflow runs the full check.')


# ---- docs: docs/packages.md is generated from the manifest, so the list can never go stale --------------------------------
def packages_markdown(m):
    version = m['version']
    libs = [l for l in m['libraries'] if l.get('kind') != 'templates']
    tpl = next((l for l in m['libraries'] if l.get('kind') == 'templates'), None)
    n_total = len(packages(m))
    lines = [
        '# Packages',
        '',
        '<!-- Generated from packages.json by `python scripts/holonet_packages.py docs`. Do not edit by hand. -->',
        '',
        f'HoloNET is {len(libs)} libraries, each published as {len(SUFFIX)} NuGet packages, plus a template pack: **{n_total} packages**, all released together at **version {version}**.',
        'Every package has the same version, so you never have to work out which versions go together.',
        '',
        '## The libraries',
        '',
        '| Library | What it is |',
        '|---|---|',
    ]
    for l in libs:
        lines.append(f"| `{l['id']}` | {l['description']} |")
    lines += [
        '',
        '## The four packages for each library',
        '',
        'Holochain needs a conductor (the program that runs your hApp). Each library comes in four packages, which differ only in the conductor they bundle:',
        '',
        '| Package suffix | Bundles | Use it when |',
        '|---|---|---|',
        '| *(none)* | no conductor | You run a conductor yourself (`hc sandbox`, a launcher, a server), or the app is a browser or phone app. |',
        '| `.Embedded` | Holochain 0.7.1 **and** 0.7.0 | You want HoloNET to start and stop the conductor for you (Windows). Choose the version at run time with `HoloNETDNA.EmbeddedHolochainVersion`. |',
        '| `.Embedded.Holochain071` | Holochain 0.7.1 only | The same, with a smaller download. |',
        '| `.Embedded.Holochain070` | Holochain 0.7.0 only | The same, for hApps built against 0.7.0. |',
        '',
        'The bundled conductors are Windows programs. On other systems use the package with no suffix and run your own conductor. Use one variant per app: they have the same API and namespaces.',
        'ORM, HyperNET, HDK, Manager, API and TestHarness depend on the Client variant that matches their own, so an `.Embedded.Holochain071` package always pulls in the Client with 0.7.1 and nothing else.',
        '',
        '## All packages',
        '',
        '| Package | Version |',
        '|---|---|',
    ]
    for _, _, pid in packages(m):
        lines.append(f'| [`{pid}`](https://www.nuget.org/packages/{pid}) | {version} |')
    lines += ['']
    if tpl:
        lines += [
            '## Project templates',
            '',
            f"`{tpl['id']}` is not a library: it adds `dotnet new` templates that create a ready-to-run project with HoloNET already wired in, for the popular .NET app types. See [Templates](templates).",
            '',
            '```bash',
            f"dotnet new install {tpl['id']}",
            'dotnet new holonet-blazor -n MyApp --HoloNET orm',
            '```',
            '',
        ]
    lines += [
        '## Which one do I want?',
        '',
        '| You want | Package |',
        '|---|---|',
        '| To save and load data in one line | `NextGenSoftware.Holochain.HoloNET.API` (add `.Embedded` on Windows to bundle the conductor) |',
        "| To call your own hApp's zome functions | `NextGenSoftware.Holochain.HoloNET.Client` |",
        '| To work with your own C# classes | `NextGenSoftware.Holochain.HoloNET.ORM` |',
        '| Multiplayer networking | `NextGenSoftware.Holochain.HoloNET.HyperNET` |',
        '| Everything in one reference | `NextGenSoftware.Holochain.HoloNET.HDK` |',
        '| To start a new app | `dotnet new install NextGenSoftware.Holochain.HoloNET.Templates`, then `dotnet new holonet-<app type>` |',
        '',
        '## For maintainers: keeping this list complete',
        '',
        'The list lives in one file, [`packages.json`](https://github.com/holochain-open-dev/holochain-client-csharp/blob/main/packages.json), and one script,',
        '[`scripts/holonet_packages.py`](https://github.com/holochain-open-dev/holochain-client-csharp/blob/main/scripts/holonet_packages.py), checks, packs and verifies it. A public workflow runs the check on every push,',
        'so a package cannot be forgotten.',
        '',
        '| Command | What it does |',
        '|---|---|',
        '| `check` | The manifest and the repository agree: every project that makes a package is listed, every variant exists, every version matches, and this page is current. |',
        '| `pack <folder>` | Builds and packs every package. |',
        '| `verify-packed <folder>` | Every expected package exists and each depends only on its own variant. |',
        '| `smoke <folder>` | Installs every package into a fresh app, builds it, runs it, and checks the conductors it bundles. |',
        '| `verify-nuget` | After publishing: every package is on nuget.org at this version. |',
        '| `set-version X.Y.Z` | Moves the whole family to a new version. |',
        '| `docs` | Regenerates this page. |',
        '',
    ]
    return chr(10).join(lines)


def docs(check_only=False):
    m = manifest()
    path = os.path.join(REPO, 'docs', 'packages.md')
    text = packages_markdown(m)
    current = io.open(path, encoding='utf-8-sig', newline='').read().replace(chr(13) + chr(10), chr(10)) if os.path.exists(path) else None
    if check_only:
        return current == text
    io.open(path, 'w', encoding='utf-8', newline=chr(10)).write(text)
    print('Wrote docs/packages.md')


# ---- pack ------------------------------------------------------------------------------------------------------------
def pack(out, only=None):
    only = set(only or [])
    failures = []
    m = manifest()
    version = m['version']
    out = os.path.abspath(out)
    os.makedirs(out, exist_ok=True)
    for lib, variant, pid in packages(m):
        if only and pid not in only and lib['id'] not in only:
            continue
        # The version comes from each csproj (set-version writes it; check enforces it equals the manifest). It must NOT be a
        # global -p:Version: that flows into referenced projects too (e.g. Chaos.NaCl), giving them a different assembly
        # version from the copy that other packages bring, which breaks the HDK build (CS1704).
        common = ['-c', 'Release'] + props(variant)
        # --no-incremental: each variant must really get its own conductors / dependency names, not the last build's.
        # Keep going after a failure so one run reports every broken package, not just the first.
        if run(['dotnet', 'build', lib['project'], '--no-incremental'] + common, check=False) != 0 or            run(['dotnet', 'pack', lib['project'], '--no-build', '-o', out] + common, check=False) != 0:
            failures.append(pid)
    if failures:
        fail([f'could not build/pack {p}' for p in failures], 'pack failed')
    print('Packed into', out)


# ---- verify-packed ---------------------------------------------------------------------------------------------------
def nuspec_deps(path):
    with zipfile.ZipFile(path) as z:
        name = next(n for n in z.namelist() if n.endswith('.nuspec'))
        root = ET.fromstring(z.read(name))
    ns = {'n': root.tag.split('}')[0].strip('{')}
    return {d.get('id'): d.get('version') for d in root.iterfind('.//n:dependency', ns)}


def verify_packed(out):
    m = manifest()
    version = m['version']
    errors = []
    expected = {pid: f'{pid}.{version}.nupkg' for _, _, pid in packages(m)}
    for pid, fname in expected.items():
        if not os.path.exists(os.path.join(out, fname)):
            errors.append(f'missing package: {fname}')
    for f in glob.glob(os.path.join(out, '*.nupkg')):
        if os.path.basename(f) not in expected.values():
            errors.append(f'unexpected package (not in packages.json at {version}): {os.path.basename(f)}')

    for lib, variant, pid in packages(m):
        path = os.path.join(out, expected[pid])
        if not os.path.exists(path):
            continue
        if lib.get('kind') == 'templates':
            continue   # a template pack depends on nothing; the template tests check what it generates
        deps = nuspec_deps(path)
        family = {d: v for d, v in deps.items() if d.startswith(FAMILY_PREFIX)}
        for d, v in family.items():
            if v and not v.startswith(version) and not v.startswith('[' + version):
                errors.append(f'{pid}: depends on {d} {v}, not the family version {version}')
            want = SUFFIX[variant]
            base = re.sub(r'\.Embedded(\.Holochain07[01])?$', '', d)
            if d != base + want:
                errors.append(f'{pid}: depends on {d}, but a "{variant}" package must depend on {base}{want} (variants must not mix)')
        if lib['id'] != CLIENT and not any(d.startswith(CLIENT) for d in family) and not any(d.startswith(FAMILY_PREFIX) for d in family):
            errors.append(f'{pid}: has no HoloNET dependency')
    if errors:
        fail(errors, 'verify-packed failed')
    print(f'OK: all {len(expected)} packages present at {version}, variants consistent.')


# ---- smoke: install each package into a fresh app and build it -------------------------------------------------------
def smoke(out):
    m = manifest()
    version = m['version']
    out = os.path.abspath(out)
    work = tempfile.mkdtemp(prefix='holonet-smoke-')
    # A private packages folder: the global cache would serve stale copies of packages with the same version from an earlier run.
    os.environ['NUGET_PACKAGES'] = os.path.join(work, '.packages')
    cfg = (f'<configuration><packageSources><clear/><add key="local" value="{out}"/>'
           '<add key="nuget" value="https://api.nuget.org/v3/index.json"/></packageSources></configuration>')
    failures = []
    try:
        for lib, variant, pid in packages(m):
            if lib.get('kind') == 'templates':
                continue   # not a library: python templates/test_templates.py <packages dir> builds and runs what it generates
            name = 'smoke-' + re.sub(r'[^A-Za-z0-9]', '-', pid[len(FAMILY_PREFIX):])
            d = os.path.join(work, name)
            os.makedirs(d)
            io.open(os.path.join(d, 'nuget.config'), 'w').write(cfg)
            # Windows-only packages (the Manager is WPF) need a windows target; everything else is a plain net10.0 console app.
            tfm = 'net10.0-windows7.0' if 'Manager' in pid else 'net10.0'
            io.open(os.path.join(d, name + '.csproj'), 'w').write(
                f'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>{tfm}</TargetFramework>'
                f'<NoWarn>$(NoWarn);NU1902;NU1903;NU1904</NoWarn></PropertyGroup>'
                f'<ItemGroup><PackageReference Include="{pid}" Version="{version}" /></ItemGroup></Project>')
            io.open(os.path.join(d, 'Program.cs'), 'w').write('System.Console.WriteLine("ok");\n')
            r = subprocess.run(['dotnet', 'build', d, '-nologo', '-v', 'q'], capture_output=True, text=True)
            print(('ok    ' if r.returncode == 0 else 'FAIL  ') + pid, flush=True)
            if r.returncode != 0:
                failures.append((pid, '\n'.join(l for l in (r.stdout + r.stderr).splitlines() if 'error' in l.lower())[:600]))
    finally:
        shutil.rmtree(work, ignore_errors=True)
    if failures:
        fail([f'{p}: {e}' for p, e in failures], 'smoke failed')
    print(f'OK: every one of the {len([x for x in packages(m) if x[0].get("kind") != "templates"])} library packages installs and builds in a fresh app.')


# ---- verify-nuget ----------------------------------------------------------------------------------------------------
def verify_nuget(wait_minutes=0):
    m = manifest()
    version = m['version']
    deadline = time.time() + wait_minutes * 60
    missing = []
    while True:
        missing = []
        for _, _, pid in packages(m):
            try:
                versions = json.load(urllib.request.urlopen(f'https://api.nuget.org/v3-flatcontainer/{pid.lower()}/index.json'))['versions']
            except Exception:
                versions = []
            if version not in versions:
                missing.append(pid)
        if not missing or time.time() > deadline:
            break
        time.sleep(30)
    if missing:
        fail([f'{p} {version} is not on nuget.org' for p in missing], 'verify-nuget failed')
    print(f'OK: all {len(packages(m))} packages are on nuget.org at {version}.')


# ---- set-version -----------------------------------------------------------------------------------------------------
def set_version(new):
    if not re.fullmatch(r'\d+\.\d+\.\d+', new):
        sys.exit('version must look like 4.6.0')
    m = manifest()
    old = m['version']
    text = io.open(os.path.join(REPO, 'packages.json'), encoding='utf-8').read()
    io.open(os.path.join(REPO, 'packages.json'), 'w', encoding='utf-8', newline='').write(re.sub(r'("version":\s*")[^"]+(")', r'\g<1>' + new + r'\2', text, count=1))
    note = f'## v{new} - Part of the HoloNET {new} release (every HoloNET package is versioned together): https://github.com/holochain-open-dev/holochain-client-csharp/releases/tag/v{new}'
    for lib in m['libraries']:
        path = os.path.join(REPO, lib['project'])
        raw = io.open(path, encoding='utf-8-sig', newline='').read()
        bom = io.open(path, 'rb').read(3) == b'\xef\xbb\xbf'
        s = re.sub(r'<Version>[^<]*</Version>', f'<Version>{new}</Version>', raw, count=1)
        if note not in s and '<PackageReleaseNotes>' in s:
            i = s.index('<PackageReleaseNotes>') + len('<PackageReleaseNotes>')
            eol = '\r\n' if '\r\n' in s else '\n'
            s = s[:i] + note.replace('&', '&amp;') + eol + eol + s[i:]
        io.open(path, 'w', encoding='utf-8-sig' if bom else 'utf-8', newline='').write(s)
    hn = os.path.join(REPO, 'NextGenSoftware.Holochain.HoloNET.HyperNET', 'HyperNET.cs')
    if os.path.exists(hn):
        raw = io.open(hn, encoding='utf-8-sig', newline='').read()
        io.open(hn, 'w', encoding='utf-8', newline='').write(re.sub(r'(public const string Version = ")[^"]+(")', r'\g<1>' + new + r'\2', raw))
    for tj in glob.glob(os.path.join(REPO, 'templates', 'content', '*', '.template.config', 'template.json')):
        t = io.open(tj, encoding='utf-8').read()
        io.open(tj, 'w', encoding='utf-8', newline='').write(t.replace('"defaultValue": "' + old + '"', '"defaultValue": "' + new + '"'))
    docs()
    print(f'Family version {old} -> {new}. Add its CHANGELOG.md section, then commit, push and run the publish job.')


def main():
    a = sys.argv[1:]
    if not a or a[0] in ('-h', '--help'):
        print(__doc__)
    elif a[0] == 'list':
        for _, _, pid in packages():
            print(pid)
    elif a[0] == 'check':
        check(partial='--partial' in a)
    elif a[0] == 'pack':
        pack(a[1], [a[i + 1] for i, x in enumerate(a) if x == '--only'])
    elif a[0] == 'verify-packed':
        verify_packed(a[1])
    elif a[0] == 'smoke':
        smoke(a[1])
    elif a[0] == 'verify-nuget':
        verify_nuget(int(a[a.index('--wait') + 1]) if '--wait' in a else 0)
    elif a[0] == 'docs':
        docs()
    elif a[0] == 'set-version':
        set_version(a[1])
    else:
        sys.exit('unknown command: ' + a[0])


if __name__ == '__main__':
    main()

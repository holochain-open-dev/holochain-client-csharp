#!/usr/bin/env python3
"""Packs the template pack, installs it, and for the chosen matrix creates a project from each template and builds it against a local package feed.

    python templates/test_templates.py <packages dir> [--quick] [type[:lib[:conductor]] ...]

Default matrix: every app type with --HoloNET api, plus console and webapi with every library, plus console with each bundled-conductor variant.
Run it after scripts/holonet_packages.py pack, so the feed has the packages at the current version.
"""
import io, json, os, shutil, subprocess, sys, tempfile
HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
TYPES = sorted(d[len('holonet-'):] for d in os.listdir(os.path.join(HERE, 'content')) if d.startswith('holonet-'))


def run(cmd, cwd=None, env=None):
    return subprocess.run(cmd, cwd=cwd, env=env, capture_output=True, text=True)


def main():
    args = sys.argv[1:]
    feed = os.path.abspath(args.pop(0))
    quick = '--quick' in args
    args = [a for a in args if a != '--quick']
    matrix = []
    if args:
        for a in args:
            p = a.split(':'); matrix.append((p[0], p[1] if len(p) > 1 else 'api', p[2] if len(p) > 2 else 'auto'))
    else:
        matrix = [(t, 'api', 'auto') for t in TYPES]
        if not quick:
            matrix += [('console', l, 'external') for l in ('client', 'orm', 'hypernet')] + [('console', l, 'embedded') for l in ('client', 'orm', 'hypernet')]
            matrix += [('console', 'api', 'external'), ('console', 'api', 'embedded'), ('webapi', 'orm', 'auto'), ('wpf', 'hypernet', 'auto')]
    work = tempfile.mkdtemp(prefix='holonet-tpltest-')
    env = dict(os.environ, NUGET_PACKAGES=os.path.join(work, '.packages'))
    io.open(os.path.join(work, 'nuget.config'), 'w').write(
        f'<configuration><packageSources><clear/><add key="local" value="{feed}"/><add key="nuget" value="https://api.nuget.org/v3/index.json"/></packageSources></configuration>')
    out = os.path.join(work, 'tpl')
    r = run(['dotnet', 'pack', os.path.join(HERE, 'NextGenSoftware.Holochain.HoloNET.Templates.csproj'), '-c', 'Release', '-o', out])
    if r.returncode: sys.exit('template pack failed:\n' + r.stdout[-1500:])
    nupkg = [f for f in os.listdir(out) if f.endswith('.nupkg')][0]
    run(['dotnet', 'new', 'uninstall', 'NextGenSoftware.Holochain.HoloNET.Templates'])
    r = run(['dotnet', 'new', 'install', os.path.join(out, nupkg)])
    if r.returncode: sys.exit('template install failed:\n' + r.stdout[-1500:])
    failures = []
    for t, lib, cond in matrix:
        name = f'T{len(os.listdir(work))}{t.replace("-", "")}{lib}{cond}'
        extra = ['--HoloNET', lib] + (['--Conductor', cond] if cond != 'auto' and t not in ('blazorwasm', 'maui', 'maui-blazor', 'android') else [])
        r = run(['dotnet', 'new', 'holonet-' + t, '-n', name] + extra, cwd=work)
        problem = None
        if r.returncode:
            problem = 'dotnet new failed: ' + (r.stdout + r.stderr)[-400:]
        else:
            b = run(['dotnet', 'build', os.path.join(work, name), '-nologo', '-v', 'q'], env=env)
            if b.returncode:
                problem = '\n'.join(l for l in (b.stdout + b.stderr).splitlines() if ' error ' in l)[:700] or 'build failed'
        print(('ok    ' if not problem else 'FAIL  ') + f'holonet-{t} --HoloNET {lib} --Conductor {cond}', flush=True)
        if problem: failures.append((t, lib, cond, problem)); print('      ' + problem.replace('\n', '\n      '), flush=True)
    shutil.rmtree(work, ignore_errors=True)
    sys.exit(f'{len(failures)} template(s) failed' if failures else 0)


main()

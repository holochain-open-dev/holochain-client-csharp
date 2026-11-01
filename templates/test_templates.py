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


# Multi-target apps are built one target at a time, so a missing workload for one (e.g. wasm-tools) is reported on its own.
FRAMEWORKS = {'uno': ['net10.0-desktop', 'net10.0-android']}


# What each library's demo prints when it works (HoloNETDemo.cs).
MARKERS = {'api': 'greeting = Hello, Holochain!', 'client': 'Created an avatar', 'orm': 'Updated and deleted.', 'hypernet': 'Message delivered.'}
CONSOLE_TYPES = ('console',)
WEB_PATHS = {'webapi': '/holonet', 'mvc': '/HoloNET', 'razor': '/HoloNET'}
DESKTOP_TYPES = ('wpf', 'winforms', 'avalonia', 'winui')


def kill_tree(proc):
    subprocess.run(['taskkill', '/T', '/F', '/PID', str(proc.pid)], capture_output=True)


def free_port():
    import socket
    with socket.socket() as sk:
        sk.bind(('127.0.0.1', 0))
        return sk.getsockname()[1]


def run_checks(t, lib, project, env):
    """Runs the built app and checks the HoloNET demo works. Needs the bundled conductor (Windows) or a running one on ws://localhost:7777."""
    import time, urllib.request
    marker = MARKERS[lib]
    if t in CONSOLE_TYPES:
        r = subprocess.run(['dotnet', 'run', '--project', project, '--no-build'], env=env, capture_output=True, text=True, timeout=300)
        return None if marker in r.stdout else 'output did not contain "' + marker + '":' + chr(10) + (r.stdout + r.stderr)[-500:]
    if t == 'worker':
        proc = subprocess.Popen(['dotnet', 'run', '--project', project, '--no-build'], env=env, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)
        lines, deadline = [], time.time() + 240
        import threading
        threading.Thread(target=lambda: [lines.append(l) for l in proc.stdout], daemon=True).start()
        while time.time() < deadline and marker not in ''.join(lines):
            time.sleep(1)
        kill_tree(proc)
        return None if marker in ''.join(lines) else 'log did not contain "' + marker + '":' + chr(10) + ''.join(lines)[-500:]
    if t in WEB_PATHS or t in ('blazor', 'razor', 'mvc', 'webapi'):
        port = free_port()
        proc = subprocess.Popen(['dotnet', 'run', '--project', project, '--no-build', '--urls', f'http://127.0.0.1:{port}'], env=env, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        try:
            path = WEB_PATHS.get(t, '/holonet')
            deadline, body = time.time() + 240, None
            while time.time() < deadline and body is None:
                try:
                    body = urllib.request.urlopen(f'http://127.0.0.1:{port}{path}', timeout=200).read().decode('utf-8', 'replace')
                except Exception as ex:
                    if t == 'blazor' and 'HTTP Error 404' not in str(ex): body = ''   # blazor's page needs a click: a 200 is enough
                    time.sleep(1)
            if t == 'blazor':
                return None if body is not None else 'page did not load'
            return None if body and marker in body else 'response did not contain "' + marker + '":' + chr(10) + str(body)[-500:]
        finally:
            kill_tree(proc)
    if t in DESKTOP_TYPES:
        exe_dir = os.path.join(project)
        proc = subprocess.Popen(['dotnet', 'run', '--project', project, '--no-build'], env=env, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        time.sleep(15)
        alive = proc.poll() is None
        kill_tree(proc)
        return None if alive else 'the app exited within 15 seconds of starting'
    return None


def check_unity(project, feed, env, work):
    """No Unity editor here: compile the demo file against .NET Standard 2.1 (what Unity uses) with the packages.config packages."""
    import re
    cfg = io.open(os.path.join(project, 'Assets', 'packages.config'), encoding='utf-8').read()
    cfg = re.sub(r'<!--#.*?-->', '', cfg)
    refs = ''.join(f'<PackageReference Include="{i}" Version="{v}" />' for i, v in re.findall(r'id="([^"]+)" version="([^"]+)"', cfg))
    lib = os.path.join(work, 'unity-compile')
    os.makedirs(lib, exist_ok=True)
    io.open(os.path.join(lib, 'u.csproj'), 'w').write(f'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>netstandard2.1</TargetFramework><LangVersion>latest</LangVersion></PropertyGroup><ItemGroup>{refs}<Compile Include="{os.path.join(project, "Assets", "HoloNET", "HoloNETDemo.cs")}" /></ItemGroup></Project>')
    b = run(['dotnet', 'build', lib, '-nologo', '-v', 'q'], env=env)
    return None if b.returncode == 0 else chr(10).join(l for l in (b.stdout + b.stderr).splitlines() if ' error ' in l)[:700]


def run(cmd, cwd=None, env=None):
    return subprocess.run(cmd, cwd=cwd, env=env, capture_output=True, text=True)


def main():
    args = sys.argv[1:]
    feed = os.path.abspath(args.pop(0))
    quick = '--quick' in args
    do_run = '--run' in args
    args = [a for a in args if a not in ('--quick', '--run')]
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
        extra = ['--HoloNET', lib] + (['--Conductor', cond] if cond != 'auto' and t not in ('blazorwasm', 'maui', 'maui-blazor', 'android', 'uno') else [])
        r = run(['dotnet', 'new', 'holonet-' + t, '-n', name] + extra, cwd=work)
        problem = None
        if r.returncode:
            problem = 'dotnet new failed: ' + (r.stdout + r.stderr)[-400:]
        else:
            if t == 'unity':
                problem = check_unity(os.path.join(work, name), feed, env, work)
        if not problem and not r.returncode and t != 'unity':
            for fw in FRAMEWORKS.get(t, [None]):
                b = run(['dotnet', 'build', os.path.join(work, name), '-nologo', '-v', 'q'] + (['-f', fw] if fw else []), env=env)
                if b.returncode:
                    problem = ((fw + ': ') if fw else '') + (chr(10).join(l for l in (b.stdout + b.stderr).splitlines() if ' error ' in l)[:700] or 'build failed')
                    break
        if not problem and do_run and t != 'unity' and not r.returncode:
            try:
                problem = run_checks(t, lib, os.path.join(work, name), env)
            except Exception as ex:
                problem = 'run check failed: ' + str(ex)
        print(('ok    ' if not problem else 'FAIL  ') + f'holonet-{t} --HoloNET {lib} --Conductor {cond}', flush=True)
        if problem: failures.append((t, lib, cond, problem)); print('      ' + problem.replace('\n', '\n      '), flush=True)
    shutil.rmtree(work, ignore_errors=True)
    sys.exit(f'{len(failures)} template(s) failed' if failures else 0)


main()

#!/usr/bin/env python3
"""Creates a project from the holonet-unity template, restores its packages with NuGetForUnity in a real Unity editor, and runs the HoloNET demo.

    python templates/test_unity.py <packages dir> [--HoloNET client|api|orm|hypernet] [--editor <Unity.exe>]

Needs a Unity 2021.3+ editor (default: the newest in C:\\Program Files\\Unity\\Hub\\Editor), git (Unity fetches NuGetForUnity with it), and the
template pack installed (dotnet new install <the Templates .nupkg>). Run it after scripts/holonet_packages.py pack, with no other conductor running.
"""
import glob, io, os, re, shutil, subprocess, sys, tempfile

MARKERS = {'api': 'greeting = Hello, Holochain!', 'client': 'Created an avatar', 'orm': 'Updated and deleted.', 'hypernet': 'Message delivered.'}

RESTORE = '''using System;
using System.Linq;
using UnityEditor;
public static class RestoreForTest
{
    static Type Find(string n) => AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch { return new Type[0]; } }).First(x => x.FullName == n);
    public static void Run()
    {
        Find("NugetForUnity.PackageRestorer").GetMethod("Restore").Invoke(null, new object[] { false });
        AssetDatabase.Refresh();
    }
}
'''

RUN = '''using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
public static class RunDemoForTest
{
    public static void Run()
    {
        // The same lookup HoloNETBehaviour (the template's component) does.
        var happ = Directory.GetFiles(Path.Combine(UnityEngine.Application.dataPath, "Packages"), "oasis.happ", SearchOption.AllDirectories);
        if (happ.Length > 0)
            HoloNETDemos.HoloNETDemo.HappPath = happ[0];
        string text;
        try { var t = Task.Run(() => HoloNETDemos.HoloNETDemo.RunAsync()); text = t.Wait(TimeSpan.FromMinutes(4)) ? t.Result : "TIMEOUT"; }
        catch (Exception ex) { text = "EXCEPTION " + ex; }
        File.WriteAllText("demo-result.txt", text);
        EditorApplication.Exit(0);
    }
}
'''


def unity(editor, project, method, log):
    """Runs the editor in batch mode; Unity.exe returns once it has quit."""
    cmd = [editor, '-batchmode', '-nographics', '-projectPath', project, '-logFile', log] + (['-executeMethod', method, '-quit'] if method else ['-quit'])
    subprocess.run(cmd, timeout=1500)
    return io.open(log, encoding='utf-8', errors='replace').read()


def main():
    args = sys.argv[1:]
    feed = os.path.abspath(args.pop(0))
    lib = args[args.index('--HoloNET') + 1] if '--HoloNET' in args else 'client'
    editors = sorted(glob.glob(r'C:\Program Files\Unity\Hub\Editor\*\Editor\Unity.exe'))
    editor = args[args.index('--editor') + 1] if '--editor' in args else (editors[0] if editors else None)
    if not editor:
        sys.exit('No Unity editor found; pass --editor <Unity.exe>')
    work = tempfile.mkdtemp(prefix='holonet-unity-')
    project = os.path.join(work, 'U1')
    r = subprocess.run(['dotnet', 'new', 'holonet-unity', '-n', 'U1', '--HoloNET', lib], cwd=work, capture_output=True, text=True)
    if r.returncode:
        sys.exit('dotnet new failed:\n' + r.stdout + r.stderr)
    os.makedirs(os.path.join(project, 'Assets', 'Editor'))
    io.open(os.path.join(project, 'Assets', 'Editor', 'RestoreForTest.cs'), 'w').write(RESTORE)
    # NuGetForUnity reads Assets/NuGet.config; it writes a default one on first load, so write ours first.
    io.open(os.path.join(project, 'Assets', 'NuGet.config'), 'w', encoding='utf-8').write(
        '<?xml version="1.0" encoding="utf-8"?><configuration><packageSources><clear />'
        f'<add key="local" value="{feed}" /><add key="nuget.org" value="https://api.nuget.org/v3/index.json" />'
        '</packageSources><activePackageSource><add key="All" value="(Aggregate source)" /></activePackageSource>'
        '<config><add key="packageInstallLocation" value="CustomWithinAssets" /><add key="repositoryPath" value="./Packages" />'
        '<add key="slimRestore" value="false" /></config></configuration>')
    # Keep the demo out of the first passes: it cannot compile until the packages are restored.
    demo = os.path.join(project, 'Assets', 'HoloNET')
    aside = os.path.join(work, 'HoloNET.aside')
    shutil.move(demo, aside)
    print('1/3 Unity imports NuGetForUnity (first run) ...', flush=True)
    unity(editor, project, None, os.path.join(work, 'import.log'))
    print('2/3 restoring the HoloNET packages ...', flush=True)
    unity(editor, project, 'RestoreForTest.Run', os.path.join(work, 'restore.log'))
    packages = os.path.join(project, 'Assets', 'Packages')
    if not os.path.isdir(packages) or not any(d.startswith('NextGenSoftware.Holochain.HoloNET') for d in os.listdir(packages)):
        sys.exit('FAIL: the packages were not restored; see ' + os.path.join(work, 'restore.log'))
    shutil.move(aside, demo)
    io.open(os.path.join(project, 'Assets', 'Editor', 'RunDemoForTest.cs'), 'w').write(RUN)
    print('3/3 compiling and running the demo ...', flush=True)
    log = unity(editor, project, 'RunDemoForTest.Run', os.path.join(work, 'run.log'))
    errors = sorted(set(re.findall(r'^.*error CS\d+.*$', log, re.M)))
    result = os.path.join(project, 'demo-result.txt')
    text = io.open(result, encoding='utf-8').read() if os.path.exists(result) else ''
    if errors or MARKERS[lib] not in text:
        print('FAIL' + (': compile errors:\n  ' + '\n  '.join(errors[:8]) if errors else ': the demo output did not contain "' + MARKERS[lib] + '":\n' + text[-600:]))
        print('Work folder kept: ' + work)
        sys.exit(1)
    shutil.rmtree(work, ignore_errors=True)
    print('ok    holonet-unity --HoloNET ' + lib + ' in ' + os.path.basename(os.path.dirname(os.path.dirname(editor))) + '\n' + text)


main()

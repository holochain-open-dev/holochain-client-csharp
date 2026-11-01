# HoloNETApp (Unity)

A Unity starter with [HoloNET](https://holonet.oasisomniverse.one) wired in (Unity 2021.3 or newer, which supports .NET Standard 2.1 and async/await).

This is an **Assets-only starter**, not a complete Unity project: create a new Unity project, then copy the `Assets` and `Packages` folders from here into it.
It has not been tested in the Unity editor; the HoloNET code (`HoloNETDemo.cs`) is compiled against .NET Standard 2.1, which Unity uses, by the template tests.

## Set up

1. Copy `Assets/` and `Packages/manifest.json` (merge the `dependencies` line if you already have a manifest) into your Unity project.
2. Let Unity import [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity). In **NuGet > Restore Packages** it installs the HoloNET packages listed in `Assets/packages.config`.
3. Add the `HoloNETBehaviour` component (Assets/HoloNET) to any GameObject and press Play.

## The conductor

Unity cannot start Holochain itself in the editor, so run a conductor on your machine first (`./start-conductor.sh` or `.\start-conductor.ps1`; ws://localhost:7777),
or, in a Windows standalone build, create this starter with `--Conductor embedded` to bundle it. The conductor installs the hApp from a path on *its* machine:
set `HoloNETDemo.HappPath` to where `oasis.happ` is (the HoloNET API package puts it in `Assets/Packages/NextGenSoftware.Holochain.HoloNET.API.*/`).

## Learn more

- [Documentation](https://docs.holonet.oasisomniverse.one) and [tutorials](https://learn.holonet.oasisomniverse.one)
- [All HoloNET packages and templates](https://docs.holonet.oasisomniverse.one/packages)

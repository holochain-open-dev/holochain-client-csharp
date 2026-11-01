# HoloNETApp (Unity)

A Unity starter with [HoloNET](https://holonet.oasisomniverse.one) wired in (Unity 2021.3 or newer, which supports .NET Standard 2.1 and async/await).

This is an **Assets-only starter**, not a complete Unity project: create a new Unity project, then copy the `Assets` and `Packages` folders from here into it.
It is tested in the Unity 2022.3 editor: the packages restore through NuGetForUnity, the scripts compile, and the demo runs against the bundled conductor.

## Set up

1. Copy `Assets/` and `Packages/manifest.json` (merge the `dependencies` line if you already have a manifest) into your Unity project.
2. Let Unity import [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity). In **NuGet > Restore Packages** it installs the HoloNET packages listed in `Assets/packages.config`.
3. Add the `HoloNETBehaviour` component (Assets/HoloNET) to any GameObject and press Play.

## The conductor

On Windows the default (`--Conductor embedded`) bundles the Holochain conductor: HoloNET starts and stops it for you, in the editor and in standalone builds.
On other platforms create the starter with `--Conductor external`, and run a conductor first (`./start-conductor.sh` or `.\start-conductor.ps1`; ws://localhost:7777).
`HoloNETBehaviour` finds `oasis.happ` (the HoloNET API package puts it under `Assets/Packages`); to use your own hApp set `HoloNETDemo.HappPath`.

## Learn more

- [Documentation](https://docs.holonet.oasisomniverse.one) and [tutorials](https://learn.holonet.oasisomniverse.one)
- [All HoloNET packages and templates](https://docs.holonet.oasisomniverse.one/packages)

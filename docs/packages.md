# Packages

<!-- Generated from packages.json by `python scripts/holonet_packages.py docs`. Do not edit by hand. -->

HoloNET is 7 libraries, each published as 4 NuGet packages, plus a template pack: **29 packages**, all released together at **version 4.6.0**.
Every package has the same version, so you never have to work out which versions go together.

## The libraries

| Library | What it is |
|---|---|
| `NextGenSoftware.Holochain.HoloNET.Client` | Connect to a Holochain conductor, install hApps, call zome functions and receive signals. Everything else builds on it. |
| `NextGenSoftware.Holochain.HoloNET.ORM` | Save, load, change and delete your own C# classes on Holochain, with change tracking and collections. Includes the Client. |
| `NextGenSoftware.Holochain.HoloNET.HyperNET` | Real-time player-to-player messaging for games and apps: zones, area of interest, reliable messages. Includes the Client. |
| `NextGenSoftware.Holochain.HoloNET.HDK` | Everything in one package: the Client, ORM and HyperNET. WEB5 STAR and HoloOASIS are not included yet because the STAR packages cannot be installed at the moment. |
| `NextGenSoftware.Holochain.HoloNET.Manager` | HoloNET Manager: a Windows (WPF) app and library for running a conductor and managing hApps. |
| `NextGenSoftware.Holochain.HoloNET.API` | The simple API: one line to save, load and delete data, share it with every agent and hear when it changes. Includes the Client and the OASIS hApp. |
| `NextGenSoftware.Holochain.HoloNET.Client.TestHarness` | A ready-to-run test harness that exercises the Client and ORM against a live conductor. |

## The four packages for each library

Holochain needs a conductor (the program that runs your hApp). Each library comes in four packages, which differ only in the conductor they bundle:

| Package suffix | Bundles | Use it when |
|---|---|---|
| *(none)* | no conductor | You run a conductor yourself (`hc sandbox`, a launcher, a server), or the app is a browser or phone app. |
| `.Embedded` | Holochain 0.7.1 **and** 0.7.0 | You want HoloNET to start and stop the conductor for you (Windows). Choose the version at run time with `HoloNETDNA.EmbeddedHolochainVersion`. |
| `.Embedded.Holochain071` | Holochain 0.7.1 only | The same, with a smaller download. |
| `.Embedded.Holochain070` | Holochain 0.7.0 only | The same, for hApps built against 0.7.0. |

The bundled conductors are Windows programs. On other systems use the package with no suffix and run your own conductor. Use one variant per app: they have the same API and namespaces.
ORM, HyperNET, HDK, Manager, API and TestHarness depend on the Client variant that matches their own, so an `.Embedded.Holochain071` package always pulls in the Client with 0.7.1 and nothing else.

## All packages

| Package | Version |
|---|---|
| [`NextGenSoftware.Holochain.HoloNET.Client`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.Client) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.Client.Embedded`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.Client.Embedded) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.Client.Embedded.Holochain071`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.Client.Embedded.Holochain071) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.Client.Embedded.Holochain070`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.Client.Embedded.Holochain070) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.ORM`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.ORM) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.ORM.Embedded`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.ORM.Embedded) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.ORM.Embedded.Holochain071`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.ORM.Embedded.Holochain071) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.ORM.Embedded.Holochain070`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.ORM.Embedded.Holochain070) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.HyperNET`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.HyperNET) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.HyperNET.Embedded`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.HyperNET.Embedded) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.HyperNET.Embedded.Holochain071`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.HyperNET.Embedded.Holochain071) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.HyperNET.Embedded.Holochain070`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.HyperNET.Embedded.Holochain070) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.HDK`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.HDK) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.HDK.Embedded`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.HDK.Embedded) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.HDK.Embedded.Holochain071`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.HDK.Embedded.Holochain071) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.HDK.Embedded.Holochain070`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.HDK.Embedded.Holochain070) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.Manager`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.Manager) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.Manager.Embedded`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.Manager.Embedded) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.Manager.Embedded.Holochain071`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.Manager.Embedded.Holochain071) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.Manager.Embedded.Holochain070`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.Manager.Embedded.Holochain070) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.API`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.API) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.API.Embedded`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.API.Embedded) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.API.Embedded.Holochain071`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.API.Embedded.Holochain071) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.API.Embedded.Holochain070`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.API.Embedded.Holochain070) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.Client.TestHarness`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.Client.TestHarness) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.Client.TestHarness.Embedded`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.Client.TestHarness.Embedded) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.Client.TestHarness.Embedded.Holochain071`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.Client.TestHarness.Embedded.Holochain071) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.Client.TestHarness.Embedded.Holochain070`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.Client.TestHarness.Embedded.Holochain070) | 4.6.0 |
| [`NextGenSoftware.Holochain.HoloNET.Templates`](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.Templates) | 4.6.0 |

## Project templates

`NextGenSoftware.Holochain.HoloNET.Templates` is not a library: it adds `dotnet new` templates that create a ready-to-run project with HoloNET already wired in, for the popular .NET app types. See [Templates](templates).

```bash
dotnet new install NextGenSoftware.Holochain.HoloNET.Templates
dotnet new holonet-blazor -n MyApp --HoloNET orm
```

## Which one do I want?

| You want | Package |
|---|---|
| To save and load data in one line | `NextGenSoftware.Holochain.HoloNET.API` (add `.Embedded` on Windows to bundle the conductor) |
| To call your own hApp's zome functions | `NextGenSoftware.Holochain.HoloNET.Client` |
| To work with your own C# classes | `NextGenSoftware.Holochain.HoloNET.ORM` |
| Multiplayer networking | `NextGenSoftware.Holochain.HoloNET.HyperNET` |
| Everything in one reference | `NextGenSoftware.Holochain.HoloNET.HDK` |
| To start a new app | `dotnet new install NextGenSoftware.Holochain.HoloNET.Templates`, then `dotnet new holonet-<app type>` |

## For maintainers: keeping this list complete

The list lives in one file, [`packages.json`](https://github.com/holochain-open-dev/holochain-client-csharp/blob/main/packages.json), and one script,
[`scripts/holonet_packages.py`](https://github.com/holochain-open-dev/holochain-client-csharp/blob/main/scripts/holonet_packages.py), checks, packs and verifies it. A public workflow runs the check on every push,
so a package cannot be forgotten.

| Command | What it does |
|---|---|
| `check` | The manifest and the repository agree: every project that makes a package is listed, every variant exists, every version matches, and this page is current. |
| `pack <folder>` | Builds and packs every package. |
| `verify-packed <folder>` | Every expected package exists and each depends only on its own variant. |
| `smoke <folder>` | Installs every package into a fresh app, builds it, runs it, and checks the conductors it bundles. |
| `verify-nuget` | After publishing: every package is on nuget.org at this version. |
| `set-version X.Y.Z` | Moves the whole family to a new version. |
| `docs` | Regenerates this page. |

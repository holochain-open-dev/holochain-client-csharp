# HoloNET documentation

HoloNET is the .NET and Unity client for [Holochain](https://holochain.org), plus a family of libraries built on it. Everything here is for **HoloNET 4.2+ on Holochain 0.7.0 and 0.7.1**.

## Start here

| If you want to... | Read |
|---|---|
| Pick a package and make your first zome call | [Getting started](getting-started.md) |
| Store and load data with no Rust and no setup | [HoloNET API guide](api-guide.md) |
| Map your C# classes onto your hApp's entries | [HoloNET ORM guide](orm-guide.md) |
| Talk to the conductor directly (admin, app, signals) | [HoloNET Client guide](client-guide.md) |
| Build multiplayer networking with no game server | [HyperNET guide](hypernet-guide.md) |
| Manage hApps and conductors from a desktop UI | [HoloNET Manager guide](manager-guide.md) |
| Generate a hApp (Rust and C#) from metadata | [STAR hApp generation guide](star-happ-generation.md) |
| Run the tests against real conductors | [Testing guide](../INTEGRATION_TESTING.md) |

## API reference

These pages list every public type and member, generated from the compiled assemblies and their XML docs.

- [HoloNET Client](api/Client.md): `HoloNETClientAdmin`, `HoloNETClientAppAgent`, `HoloNETDNA`, requests, responses and event args.
- [HoloNET ORM](api/ORM.md): `HoloNETEntryBase`, `HoloNETAuditEntryBase`, `HoloNETCollection`, `HoloNETObservableCollection`.
- [HoloNET API](api/API.md): `HoloNETAPI`, `HoloNETAPIHapp`, `HoloNETAPIResult`.
- [HoloNET HyperNET](api/HyperNET.md): `HyperNETSession`, `HyperNETService`, options, peers and events.

The [main README](../README.md) also has the long-form Client and ORM reference, with every event, method and property described in tables. The [CHANGELOG](../CHANGELOG.md) has the full history.

## How the pieces fit

```
 Your app / game / service
   │
   ├── HoloNET API        simplest: static key/value + object store, direct messages
   ├── HoloNET ORM        your classes ⇄ your hApp's entries (CRUD, change tracking, collections)
   ├── HoloNET HyperNET   P2P multiplayer: zones, area of interest, reliable messages
   │
   └── HoloNET Client     the conductor connection: admin API, app API, zome calls, signals
          │
          └── Holochain conductor (yours, or the one an .Embedded package bundles)

 WEB5 STAR             generates hApps (Rust zomes + C# on HoloNET) from metadata
 HoloNET Manager       desktop UI over all of the above
 HoloNET HDK           one package: Client + ORM + HyperNET + STAR + HoloOASIS
```

## Regenerating the API reference

The reference pages come from `scripts/apigen`. Build the libraries with XML docs, then run the generator:

```bash
bash scripts/generate-api-docs.sh
```

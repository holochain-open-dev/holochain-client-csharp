# Generating hApps with WEB5 STAR

WEB5 STAR (the OASIS low/no-code generator) can generate a complete Holochain hApp from a simple description of your data:

- the Rust integrity and coordinator zomes;
- the DNA and hApp manifests;
- a build script;
- the matching C# HoloNET ORM models.

You describe what you want in C#. STAR (through HoloOASIS and HoloNET ORM's `HolochainHappGenerator`) writes everything else.

What you get:

- **A Cargo workspace** for Holochain **0.7.1** (hdk 0.7.1 / hdi 0.8.1) or **0.7.0** (hdk 0.7.0 / hdi 0.8.0).
- **Per zome, an integrity zome** with your entry types, link types and a `validate` callback ready for your rules.
- **Per zome, a coordinator zome.** For every entry it has `create_<entry>`, `get_<entry>`, `update_<entry>`, `delete_<entry>` and `get_all_<entry>s`, plus anchored collections: `get_<entry>_collection`, `add_<entry>_to_collection`, `remove_<entry>_from_collection` and `batch_update_<entry>_collection`.
- **Manifests and a build script:** `dna.yaml`, `happ.yaml` and `build.sh`.
- **C# models:** one `HoloNETEntryBase` class per entry, with every field mapped, and a `NewCollection(client)` factory, so you can use the hApp from .NET straight away.

The zome function shapes are exactly the ones HoloNET ORM uses, so the generated models work against the generated hApp with no changes. This is tested on every HoloNET release:

1. A sample hApp is generated and compiled with cargo.
2. It's packed with `hc` and installed on real Holochain 0.7.0 and 0.7.1 conductors.
3. The generated C# models are compiled and used to create, load, update, delete and work with collections.

## 1. Describe your data (CelestialBody DNA)

A STAR CelestialBody DNA folder holds C# files. In them:

- each class deriving from `ZomeDNA` is a zome;
- each class nested inside it and deriving from `HolonDNA` is an entry type;
- each public property becomes a field.

```csharp
using NextGenSoftware.OASIS.STAR.DNA;

namespace MyGame.DNA
{
    public class Game : ZomeDNA
    {
        public class Player : HolonDNA
        {
            public string Name { get; set; }
            public int Level { get; set; }
            public long Score { get; set; }
            public bool IsOnline { get; set; }
            public double Rating { get; set; }
            public DateTime JoinedOn { get; set; }
        }

        public class GameItem : HolonDNA
        {
            public string Title { get; set; }
            public string Type { get; set; }
        }
    }
}
```

| C# type | Rust type | Generated C# property |
|---|---|---|
| `string` | `String` | `string` |
| `int` | `i32` | `int` |
| `long` | `i64` | `long` |
| `bool` | `bool` | `bool` |
| `double`, `float` | `f64` | `double` |
| `Guid`, `DateTime` | `String` | `string` (the value round-trips exactly) |

Names become snake_case in Rust (`IsOnline` becomes `is_online`), and Rust keywords are escaped (`Type` becomes `r#type`).

## 2. Generate

Make sure STAR's current storage provider is **HoloOASIS**: STAR generates native code for the current provider. Then light your OAPP:

```
star light
```

The wizard asks for:

- the OAPP name and description;
- the genesis type (Moon, Planet, Star, ..., or ZomesAndHolonsOnly);
- the CelestialBody DNA folder;
- the output folder.

For scripted builds, put the same answers in a JSON request:

```
star --non-interactive --json light ./LightRequest.json
```

The STAR docs (`Docs/Devs/STAR_CLI_NonInteractive.md` in the OASIS repo) describe the schema: `OAPPName`, `OAPPDescription`, `GenesisType`, `CelestialBodyDnaFolder`, `GenesisFolder`, `ProviderType: "HoloOASIS"`, ...

STAR writes its usual C# OAPP (the CelestialBody, zome and holon classes on the COSMIC ORM, which work with any OASIS provider). HoloOASIS adds the hApp in `<OAPP folder>/Rust`:

```
Rust/
  Cargo.toml
  build.sh
  README.md
  dnas/<oapp>/workdir/dna.yaml
  dnas/<oapp>/zomes/integrity/game_integrity/src/lib.rs
  dnas/<oapp>/zomes/coordinator/game/src/lib.rs
  workdir/happ.yaml
  CSharp/Player.cs
  CSharp/GameItem.cs
```

HoloOASIS targets 0.7.1 by default. Pass `"holochainVersion": "0.7.0"` in its native params to target 0.7.0.

## 3. Build the hApp

You need:

- Rust, with the `wasm32-unknown-unknown` target (`rustup target add wasm32-unknown-unknown`);
- Holochain's `hc`, the same version you're targeting.

HoloNET's Embedded packages bundle `hc.exe`.

```bash
cd <OAPP folder>/Rust
./build.sh          # cargo build --release --target wasm32-unknown-unknown, then hc dna pack + hc app pack
```

This produces `workdir/<oapp>.happ`.

## 4. Use it from .NET

Copy `CSharp/*.cs` into your project, then reference a HoloNET ORM package (`NextGenSoftware.Holochain.HoloNET.ORM.Embedded` starts its own conductor).

```csharp
var admin = new HoloNETClientAdmin(new HoloNETDNA { HolochainConductorMode = HolochainConductorModeEnum.UseEmbedded });
await admin.ConnectAsync();
var app = await admin.InstallEnableSignAttachAndConnectToHappAsync("my-game", @"Rust\workdir\my_game.happ", "my_game");

var player = new Player(app.HoloNETClientAppAgent) { Name = "Ada", Level = 1 };
await player.SaveAsync();

var guild = Player.NewCollection(app.HoloNETClientAppAgent);
await guild.LoadCollectionAsync("guild-42");
await guild.AddHoloNETEntryToCollectionAndSaveAsync(player, collectionAnchor: "guild-42");
```

See the [ORM guide](orm-guide.md) for everything the models can do.

## 5. Make it yours

The generated code is a starting point you own:

- **Validation.** Add your rules to `validate_entry` in the integrity zome, for example required fields, ranges or who may update what.
- **More zome functions.** Add them to the coordinator, such as lookups by a field or links between entries. Call them from C# with `CallZomeFunctionAsync`, or with `LoadByCustomFieldAsync` on the models.
- **Integrity changes create a new DNA.** Changing an integrity zome changes the DNA hash, so the result is a new network. Coordinator changes can be hot-swapped onto a running hApp with `UpdateCoordinatorsAsync`, or with **Update Coordinators** in [HoloNET Manager](manager-guide.md).

## Using the generator directly

You don't need STAR to use the generator; it's part of HoloNET ORM:

```csharp
using NextGenSoftware.Holochain.HoloNET.ORM.Generation;

var spec = new HappSpec
{
    Name = "my_game",
    CSharpNamespace = "MyGame",
    HolochainVersion = HappHolochainVersion.V0_7_1,
    Zomes =
    {
        new HappZomeSpec("Game",
            new HappEntrySpec("Player", new HappFieldSpec("Name", "string"), new HappFieldSpec("Level", "int")))
    }
};
HolochainHappGenerator.Generate(spec, @"C:\out\my_game");

// Or read the spec from a STAR CelestialBody DNA folder:
var fromDna = HolochainHappGenerator.FromStarDnaFolder(@"C:\MyGame\DNA", "my_game");
```

## Before HoloNET 4.4

Older STAR releases generated Rust from templates written for the pre-RSM Holochain HDK 0.0.50, which can't compile on any current Holochain. If you have code generated by those releases, regenerate it.

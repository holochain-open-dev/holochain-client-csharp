# Generate a hApp from your classes

**You'll build:** a complete Holochain 0.7 hApp (the Rust zomes, manifests and a build script) and the matching C# models, generated from a description of your data. You don't write any Rust.
**Time:** 20 minutes (plus the first Rust build). **You need:** [Rust](https://rustup.rs) with `rustup target add wasm32-unknown-unknown`, and Holochain's `hc` (bundled with HoloNET on Windows).
**Finished code:** [`examples/05-GenerateHapp`](https://github.com/holochain-open-dev/holochain-client-csharp/blob/main/examples/05-GenerateHapp/Program.cs) and its [STAR OAPP](https://github.com/holochain-open-dev/holochain-client-csharp/tree/main/examples/05-GenerateHapp/StarOAPP).

## 1. Describe your data

Write it as a WEB5 STAR CelestialBody DNA file: a class per zome, with a nested class per entry.

```csharp
public class Game : ZomeDNA
{
    public class Ship : HolonDNA
    {
        public string Name { get; set; }
        public double Speed { get; set; }
        public int Crew { get; set; }
        public bool IsDocked { get; set; }
    }

    public class Planet : HolonDNA
    {
        public string Name { get; set; }
        public long Population { get; set; }
    }
}
```

## 2a. Generate it with WEB5 STAR

1. With HoloOASIS as STAR's provider, and `"HolochainVersion": "0.7.1"` in its `OASIS_DNA.json`, light the OAPP:

   ```
   star light
   ```

2. Give it the folder with your DNA file. STAR writes the OAPP, plus the hApp in `<OAPP folder>/Rust`, from its editable Holochain 0.7 templates.

## 2b. ...or generate it from code

```bash
dotnet add package NextGenSoftware.Holochain.HoloNET.ORM
```

```csharp
using NextGenSoftware.Holochain.HoloNET.ORM.Generation;

HappSpec spec = HolochainHappGenerator.FromStarDnaFolder("DNA", "space_game", "SpaceGame");
HolochainHappGenerator.Generate(spec, "space_game");
```

## 3. Look at what you got

```
space_game/
  Cargo.toml  build.sh  README.md
  dnas/space_game/workdir/dna.yaml          workdir/happ.yaml
  dnas/space_game/zomes/integrity/game_integrity/src/lib.rs     <- Ship and Planet entry types, link types, validate()
  dnas/space_game/zomes/coordinator/game/src/lib.rs             <- create_ship, get_ship, update_ship, delete_ship,
                                                                   get_all_ships, get_ship_collection, ... (and for Planet)
  CSharp/Ship.cs  CSharp/Planet.cs                               <- HoloNET ORM models for them
```

## 4. Build and use it

```bash
cd space_game && ./build.sh        # cargo build to wasm, then hc dna pack + hc app pack
```

Install `workdir/space_game.happ` with HoloNET (role `space_game`), copy `CSharp/*.cs` into your app, and use the models exactly as in [Model your data with the ORM](03-model-your-data.md):

```csharp
var ship = new Ship(client) { Name = "Endeavour", Speed = 0.9, Crew = 12 };
await ship.SaveAsync();

var fleet = Ship.NewCollection(client);
await fleet.LoadCollectionAsync("fleet-1");
```

## What just happened

- **Coverage.** The generator wrote everything Holochain 0.7 needs for each entry: CRUD, a list of all entries, anchored collections, and a `validate` callback ready for your rules.
- **Matching models.** The C# models use the zome functions the generated hApp has, so they work together without changes.
- **It's yours.** Add validation rules, extra zome functions or links between entries. HoloNET's own tests generate this kind of hApp, compile it with cargo, and drive it with the ORM on real 0.7.0 and 0.7.1 conductors.

## Next

- The [STAR hApp generation guide](https://docs.holonet.oasisomniverse.one/star-happ-generation) covers the templates, the field types and choosing 0.7.0 or 0.7.1.

# Space Game: a WEB5 STAR OAPP with a generated Holochain hApp

This folder is everything WEB5 STAR needs to generate an OAPP:
- the C# code, on the COSMIC ORM, which works with any OASIS provider;
- with HoloOASIS as the provider, a complete **Holochain 0.7 hApp** and its **HoloNET ORM models**, built from STAR's
  Holochain07 templates.

| File | What it is |
|---|---|
| `CelestialBodyDNA/Game.cs` | The description: zome `Game` with entries `Ship` and `Planet` and their fields. |
| `LightRequest.json` | The answers to `star light`'s questions, so it runs without prompts. |

## Generate it with STAR

1. Install the STAR CLI.
2. Set the HoloOASIS section of its `OASIS_DNA.json`:
   - `"HolochainVersion": "0.7.1"` (or `"0.7.0"`);
   - optionally, `RustDNAHolochain07TemplateFolder`, if you keep your own copy of the templates.
3. Make HoloOASIS the current provider, because STAR generates native code for the current provider.
4. From this folder, run:

```
star --non-interactive --json light LightRequest.json
```

To answer the questions yourself instead, run `star light` and give the same values.

STAR writes the OAPP into `Generated/`, and the hApp into `Generated/Rust/`:
- `Cargo.toml`, `build.sh` and `README.md`;
- `dnas/space_game/workdir/dna.yaml` and `workdir/happ.yaml`;
- `dnas/space_game/zomes/integrity/game_integrity/` (entry and link types, validation);
- `dnas/space_game/zomes/coordinator/game/` (create, get, update, delete and list for each entry, plus collections);
- `CSharp/Ship.cs` and `CSharp/Planet.cs` (the HoloNET ORM models).

Build the hApp with `Generated/Rust/build.sh` (Rust with the `wasm32-unknown-unknown` target, and `hc`). Then install `workdir/space_game.happ` with HoloNET and use the models as in [example 02](../../02-ORM).

## Or without STAR

[`../Program.cs`](../Program.cs) reads this same DNA folder with HoloNET ORM's `HolochainHappGenerator.FromStarDnaFolder` and generates the same hApp from code. See the [STAR hApp generation guide](../../../docs/star-happ-generation.md).

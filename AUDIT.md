# HoloNET Client audit — Holochain 0.7.0 compatibility

**Date:** 2026-10-03 · **Package:** `NextGenSoftware.Holochain.HoloNET.Client` 4.1.0

## Summary

The 0.6.x and 0.7.0 "upgrades" changed version strings, docs and some type shapes, but nothing
had been run against a real conductor since Holochain 0.1.5 (2023). Checking the client against
the holochain-0.7.0 Rust source and a live 0.7.0 conductor found that **every admin and app call
failed**: the request/response envelope used `"data"` where the conductor expects `"value"`.
Behind that were several more bugs that would have surfaced one after another (cell decoding,
enable-app decoding, capability grants, status filters, clone cells, DNA manifests).

All the findings below are fixed. Admin calls now work end to end against a real 0.7.0 conductor. App-side
flows (install a hApp, connect to an app interface, zome calls, signals) have **not** been
run live yet. They need a built test hApp.

## Method

1. **Source comparison.** Each wire type was compared with its Rust definition at tag
   `holochain-0.7.0` (`holochain_conductor_api`, `holochain_types`, `holochain_zome_types`,
   `holochain_integrity_types`, `holochain_websocket`). Particular attention went to serde
   attributes: `tag`/`content`, `rename_all` and `deny_unknown_fields`.
2. **Live conductor.** The official holochain-0.7.0 Windows builds (SHA-256 checked against
   the GitHub release digests) were run in an `hc sandbox` on admin port 65464. HoloNET was then
   exercised through `LiveConductorTests` (see [INTEGRATION_TESTING.md](INTEGRATION_TESTING.md)).
3. **Wire-shape tests.** Unit tests now assert the exact JSON that is sent, not just that a value
   round-trips through HoloNET's own types. A round trip can't catch a key the conductor doesn't
   recognise.

## Findings

Severity: **Critical** = breaks every call, or a core flow, for every user. **High** = a feature is
unusable. **Medium** = wrong results in some cases. **Low** = quality or maintenance.

| # | Severity | Finding | Evidence | Status |
|---|---|---|---|---|
| 1 | Critical | The inner request/response envelope used `"data"`. Holochain 0.6.1 and 0.7.0 use `{"type","value"}` (`#[serde(tag="type", content="value")]` on `AdminRequest`/`AppRequest`/responses). The conductor answered every call with `error`. | `admin_interface.rs`, `app_interface.rs`; live test `Live_OldDataContentKey_IsRejectedByConductor` | Fixed |
| 2 | Critical | `CellInfo` expected `{"provisioned":{...}}`, but the conductor sends `{"type":"provisioned","value":{...}}`. Every cell decoded empty, no `CellId` was captured, and `InstallEnableSignAndAttachHapp` aborted. | `app_interface.rs` `CellInfo` | Fixed (`CellInfoFormatter`) |
| 3 | High | `EnableApp` decoded the pre-0.4 `{app, errors}` response. 0.7.0 returns `AppEnabled(AppInfo)`, so the enabled app's info was null. | `admin_interface.rs` `AdminResponse::AppEnabled` | Fixed |
| 4 | High | `CapAccess` and `GrantedFunctions` were sent externally tagged (`{"Assigned":…}`, `{"All":null}`). They are adjacently tagged, so capability grants and signing credentials could not be granted. This predates 0.7.0; 0.6.1 uses the same tagging. | `holochain_integrity_types/src/capability/grant.rs` (0.6.1 and 0.7.0) | Fixed (`CapAccess`, `GrantedFunctions`) |
| 5 | High | `CloneCellId` was sent as a raw string or CellId. It is `{"type":"clone_id"\|"dna_hash","value":…}` and has no CellId variant. | `holochain_zome_types/src/clone.rs` | Fixed (`CloneCellId.Normalize`; existing overloads still work) |
| 6 | High | `DnaManifest` used `manifest_version "1"`, top-level `network_seed`/`properties`/`zomes`, and `bundled`/`url` zome locations. 0.7.0 is `"0"` with `integrity`/`coordinator`, and every struct is `deny_unknown_fields`. An earlier pass in this upgrade had also invented `IntegrityZomeManifest`, `CoordinatorZomeManifest`, `origin_time` and `quantum_time`. | `dna_manifest.rs`, `dna_manifest_v0.rs` | Fixed. Breaking; invented types `[Obsolete]` |
| 7 | High | The DumpOpTimings callbacks never fired: the decoder had no case for `op_timings_dumped`. It also lacked `zome_called` and `storage_info`. | Decoder switch vs `AdminResponse`/`AppResponse` variants | Fixed |
| 8 | Medium | `AppStatusFilter` was sent as an integer, which rmp-serde reads as a variant index. `Running` (2) filtered by AwaitingMemproofs; `AwaitingMemproofs` (5) was invalid. `AwaitingRestore`/`Unrecoverable` were missing. | `AppStatusFilter` in `admin_interface.rs` | Fixed (sent as string) |
| 9 | Medium | `HoloNETDNAManager.LoadDNA` deserialized into an interface, threw, swallowed the exception, and always returned null. | Code inspection | Fixed |
| 10 | Medium | The bundled `holochain.exe`/`hc.exe` were 0.6.2, while package text said 0.7.0. | `holochain.exe --version` | Fixed (official 0.7.0 builds) |
| 11 | Low | `generate_agent_pub_key` was tagged with the grant request type. `throw ex;` lost stack traces. A `StreamWriter` was never disposed. | Code inspection | Fixed |
| 12 | Low | 35 TestHarness `bin/`/`obj/` files were tracked despite `.gitignore`. | `git ls-files` | Fixed (untracked) |

## Still open

| Item | Why it matters | Suggested next step |
|---|---|---|
| App-side flows not run live | Install, app-interface authentication, zome-call signing (`ZomeCallParamsSigned`), signals and full `AppInfo`/`CellInfo` decoding with real hashes are only source-checked or untested. | Build a minimal test hApp (INTEGRATION_TESTING.md §2) and add live tests for install → enable → attach → connect → zome call. |
| Types not yet compared with source | Signals, `Record`/`Action`/`Entry`, `AppManifest`, `AppBundle`, `DnaModifiers`, `AgentInfo`, `StorageInfo`, network stats/metrics, countersigning payloads, `AttachAppInterface` payload. | Same source-and-live method; the app-side live tests will exercise most of them. |
| Removed conductor APIs still exposed | 0.7.0 has no `RegisterDna` or `GraftRecords` request; calling them returns a conductor error. | Mark `RegisterDnaAsync`/`GraftRecordsAsync` `[Obsolete]`, or remove them in a major version. |
| Sync wrappers block on async (`.Result`) | Can deadlock on a UI `SynchronizationContext` (HoloNET Manager is WPF). | Use `ConfigureAwait(false)` throughout the async paths, or document that the sync methods aren't safe on UI threads. |
| Unawaited calls (CS4014) and unreachable code (CS0162) | Exceptions from fire-and-forget calls are lost. | Review each warning in `HoloNETClientBase`/`HoloNETClientAppBase`. |
| Other copies of `holochain.exe` | `Templates.MAUI/Resources/Raw` and `UnoApp.Mobile/Android/Resources` still bundle old binaries; a Windows `.exe` cannot run on Android anyway. | Remove them or replace them with platform-appropriate builds. |
| Pushes bypassed branch protection | Every push to `main` reported "Bypassed rule violations… must be made through a pull request". | Review these commits after the fact; use PRs from now on. |
| Uncommitted local changes not made in this audit | The client `.csproj` now multi-targets `netstandard2.1;net10.0`, and the ORM `.csproj` is modified. | Owner to review and commit or discard. |
| Earlier CHANGELOG entries | The "Post-upgrade polish" section describes the invented manifest types as verified. | Superseded by the "Wire-format corrections" section; can be edited down. |

## Test coverage after the audit

- **76 tests** in `NextGenSoftware.Holochain.HoloNET.Client.Tests`. All pass.
- **Wire-shape tests** check exact JSON for `ZomeManifest`, `DnaManifest`, `GrantedFunctions`,
  `CapAccess`, `CloneCellId` and `AppStatusFilter`, and decode conductor-shaped `CellInfo` bytes.
- **Live tests** (`LiveConductorTests`, opt-in through `HOLONET_LIVE_ADMIN_URI`) pass against a
  holochain 0.7.0 sandbox. They cover the raw envelope, rejection of the old envelope,
  `HoloNETClientAdmin` connect / generate key / list apps / list DNAs / status filter, and
  conductor errors surfacing as `IsError`.

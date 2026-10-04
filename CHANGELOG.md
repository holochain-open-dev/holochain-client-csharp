# Changelog

## Wire-format corrections (verified against holochain-0.7.0 source)

### Breaking changes
- **`DnaManifest`** now matches 0.7.0 `DnaManifestV0` (`deny_unknown_fields`): `manifest_version`
  defaults to `"0"`; `network_seed`, `properties` and the flat `zomes` list are removed (set
  `integrity.network_seed` / `integrity.properties`, and put zomes in `integrity.zomes` /
  `coordinator.zomes`). Code that set the removed members no longer compiles.
- **`ZomeManifest`**: `bundled` and `url` removed; `path` is the only location field.
- **`IntegrityManifest.zomes` / `CoordinatorManifest.zomes`** are `ZomeManifest[]`.
  `IntegrityZomeManifest` and `CoordinatorZomeManifest` (added earlier in 4.1.0 without
  verification) are `[Obsolete]`. They are not Holochain types.
- **`DesensitizedZomeCallCapGrant.functions`** is now `Dictionary<string, object>`.
- **`ListAppsRequest.status_filter`** is now a `string`. Use `AppStatusFilter.ToWireValue()`.

### Fixes
- **Request/response envelope (affected every call)**: `AdminRequest`/`AppRequest` and their
  responses are `{"type": ..., "value": ...}` in Holochain 0.6.1 and 0.7.0. HoloNET sent and read
  `"data"`, the pre-0.4 key, so the conductor returned `error` for every call. Confirmed
  against a live 0.7.0 conductor (`Live_OldDataContentKey_IsRejectedByConductor`).
- **Response routing**: added `zome_called`, `storage_info` and `op_timings_dumped`. Before this,
  the DumpOpTimings callbacks never fired. `generate_agent_pub_key` is no longer tagged as a
  capability grant.
- **`AppStatusFilter`** was sent as an integer index, which the conductor reads as a variant
  index (`Running` filtered by `AwaitingMemproofs`). It is now sent as `"enabled"`, `"disabled"`,
  `"awaiting_memproofs"`, `"awaiting_restore"` or `"unrecoverable"`. `Running`/`Stopped`/`Paused`
  are `[Obsolete]`.
- **`EnableApp` response**: 0.6.1/0.7.0 return `AppEnabled(AppInfo)`. HoloNET decoded the old
  `{app, errors}` shape, which left the enabled app's `AppInfo` (and so its `CellId`) null.
- **`HoloNETDNAManager.LoadDNA`** always returned `null` (it deserialized into an interface and
  swallowed the exception). Also fixed: `throw ex;` → `throw;`, and the `StreamWriter` is now disposed.
- **Bundled conductor**: `Resources/holochain.exe` and `hc.exe` are now the official
  holochain-0.7.0 Windows builds. Their SHA-256 hashes match the GitHub release digests.
- **Capability grants**: `CapAccess` and `GrantedFunctions` are serialized as
  `{"type": ..., "value": ...}`, as the conductor expects. HoloNET previously sent
  `{"Assigned": ...}` / `{"All": null}`. 0.6.1 used the same tagging, so this bug predates the
  0.7.0 upgrade. New `CapAccess` builder; `CapGrantAccessUnrestricted/Transferable/Assigned`
  are `[Obsolete]`.
- **Clone cells**: `CloneCellId` is sent as `{"type": "clone_id" | "dna_hash", "value": ...}`.
  The enum has no CellId variant. Existing overloads still accept a clone-id string, a DNA hash, or
  a CellId (its DNA hash is used) via `CloneCellId.Normalize`.
- **`CellInfo` decoding**: the conductor sends `{"type": "provisioned" | "cloned" | "stem",
  "value": {...}}`, but `CellInfo` expected `{"provisioned": {...}}`, so every cell decoded empty.
  `CellInfoType` came back `None`, no `CellId` was captured, and `InstallEnableSignAndAttachHapp`
  aborted with "CellType Is Not Provisioned". Fixed with `CellInfoFormatter`.
- Tests now assert the exact wire JSON for these types. 76 tests in total, including
  `LiveConductorTests` that run against a real conductor when `HOLONET_LIVE_ADMIN_URI` is set.

See [AUDIT.md](AUDIT.md) for the full audit, the evidence for each finding, and what remains open.

---

## Post-upgrade polish (v4.1.0 follow-on)

Further hardening and completeness work on top of the 0.7.0 upgrade:

- **`DesensitizedZomeCallCapGrant` and `CapAccessInfo` typed** (`Data/Admin/Responses/`):
  `CapGrantInfo.cap_grant` is now `DesensitizedZomeCallCapGrant` (typed) instead of `dynamic`.
  `CapAccessInfo` carries `access_type` (string) and optional `assignees` (AgentPubKey[]).
  Verified against `holochain_zome_types` 0.7.0.
- **DNA manifest restructured for 0.7.0** (`Data/Admin/Requests/Objects/`):
  - `IntegrityManifest` is now the correct container shape (`network_seed`, `origin_time`,
    `quantum_time`, `properties`, `zomes: IntegrityZomeManifest[]`). `network_seed` and
    `properties` commented out on `DnaManifest` (they moved to this container in 0.7.0).
  - `IntegrityZomeManifest` — new per-zome entry type (name, dylib, hash,
    `dependencies: ZomeDependency[]`, bundled/path/url, properties).
  - `CoordinatorManifest` — updated to `CoordinatorZomeManifest[]`.
  - `CoordinatorZomeManifest` — new per-zome entry type with `dependencies: string[]`
    (plain `ZomeName` strings, matching 0.7.0 Rust vs. `ZomeDependency[]` for integrity).
  - `DnaManifest.integrity` / `.coordinator` fixed from `[]` (array) to singular container
    objects. Legacy `zomes[]` retained for backwards compatibility.
- **`EnableCloneCellRequest.clone_cell_id`** doc comment updated with wire-format note (matches
  Disable/Delete treatment from the main 0.7.0 pass).
- **`.gitattributes`** added: `* text=auto`, CRLF for `.cs`/`.csproj`/`.sln`/`.md`,
  LF for JSON/YAML — silences persistent CRLF warnings on commit.
- **Test count: 55** — covers `DumpOpTimingsRequest` (with/without cursor), `IntegrityZomeManifest`,
  `CoordinatorZomeManifest` (verifies string deps), `DnaManifest` integrity+coordinator round-trip,
  `CapAccessInfo` (Unrestricted, Assigned), `DesensitizedZomeCallCapGrant`, `OpTimingsDump` ×3;
  all green.
- **TestHarness `v4.1.0`** — Summary/Description updated to Holochain 0.7.0; conductor paths
  configurable via `HOLONET_*` env vars noted in release notes.
- **Sibling packages (ORM, HDK, HyperNET, Manager) bumped to v4.1.0** with updated NuGet
  Summary/Description referencing Holochain 0.7.0 and a `v4.1.0` `PackageReleaseNotes` entry.

---

## Holochain 0.7.0 Wire Protocol Upgrade

Upgrades HoloNET's wire protocol from Holochain 0.6.1 to 0.7.0, verified against the real Rust
source in the [holochain](https://github.com/holochain/holochain) repo at tag `holochain-0.7.0`
(`holochain_conductor_api`, `holochain_types`, `holochain_zome_types`).

Key changes:

- **tx5/WebRTC transport removed**: `signal_url` and `webrtc_config` fields removed from
  `NetworkConfig` / `Kitsune2Config` (`Data/Config/Kitsune2Config.cs`). iroh (QUIC) is now
  the sole network backend. `Kitsune2Config` retains `BootstrapUrl`, `RelayUrl`,
  `TargetArcFactor`, and `AdvancedJson`.
- **`DnaStorageInfo` fields removed** (`Data/Admin/Responses/Objects/DnaStorageInfo.cs`):
  `authored_data_size`, `authored_data_size_on_disk`, `cache_data_size`, and
  `cache_data_size_on_disk` were removed from the conductor wire shape in 0.7.0. Only
  `dht_data_size`, `dht_data_size_on_disk`, and `used_by` remain.
- **`AppStatusFilter.AwaitingMemproofs`** added (`Enums/AppStatusFilter.cs`,
  `Enums/AppInfoStatusEnum.cs`, `Data/App/Responses/AppInfo.cs`): new app status introduced
  in 0.7.0 for apps that have been installed but are waiting for membrane proof submission.
- **`DnaDef` alias** (`Data/Admin/Responses/DnaDef.cs`): the Holochain JS client renamed
  `DnaDefinition` → `DnaDef` in 0.7.0. A `DnaDef` subclass is provided for forward
  compatibility while keeping the existing `DnaDefinition` name.
- **`DumpOpTimings` — new paginated API** (new in 0.7.0): wired in both the admin and app
  interfaces:
  - New request type `DumpOpTimingsRequest` (`Data/Admin/Requests/DumpOpTimingsRequest.cs`)
    with `dna_hash`, optional `cursor` (`OpTimingsCursor`), and optional `limit`.
  - New response types `OpTimingsDump`, `OpTimingDump`, `OpTimingsCursor`
    (`Data/Admin/Responses/OpTimingsDump.cs`).
  - `HoloNETRequestType.AdminDumpOpTimings` / `AppDumpOpTimings` and
    `HoloNETResponseType.AdminOpTimingsDumped` / `AppOpTimingsDumped` added.
  - `AdminOpTimingsDumpedCallBackEventArgs` / `AppOpTimingsDumpedCallBackEventArgs` added
    (`EventArgs/EventArgsAdmin.cs`).
  - `DumpOpTimingsAsync` / `DumpOpTimings` wired in `HoloNETClientAdmin` (admin interface:
    conductor fn `dump_op_timings`) and `HoloNETClientAppBase` (app interface).

---

## Holochain 0.6.1 Wire Protocol Upgrade

Earlier "version bump" work only updated version strings/comments without changing the actual
wire protocol, leaving HoloNET's real on-the-wire shapes at roughly Holochain 0.2.x-0.3.x while
docs claimed 0.5.6. This upgrade re-verifies and re-implements the wire protocol against the real
Holochain 0.6.1 Rust source (`holochain_conductor_api`, `holochain_types`, `holochain_zome_types`
at tag `holochain-0.6.1` on GitHub) so HoloNET's serialization actually matches a 0.6.1 conductor.

Key changes:

- **Zome call signing payload** (`Data/App/Requests/ZomeCall.cs`, `ZomeCallSigned.cs`): replaced
  the old flattened `cell_id_dna_hash` / `cell_id_agent_pub_key` byte arrays with a proper
  `CellId` tuple type (`Data/App/Requests/CellId.cs`) matching
  `holochain_zome_types::cell::CellId(DnaHash, AgentPubKey)`. Added
  `Data/App/Requests/ZomeCallParamsSigned.cs` mirroring the real 0.6.1 wire shape sent over the
  app websocket interface (`holochain_conductor_api::app_interface::ZomeCallParamsSigned { bytes,
  signature }`), used by `HoloNETClientAppBase` when constructing `call_zome` requests.
- **New Admin/App message types** added to `Enums/HoloNETRequestType.cs` /
  `HoloNETResponseType.cs` with corresponding request/response classes under
  `Data/Admin/Requests`, `Data/Admin/Responses`, `Data/App/Requests`, `Data/App/Responses`:
  RevokeZomeCallCapability, ListCapabilityGrants, PeerMetaInfo, IssueAppAuthenticationToken,
  RevokeAppAuthenticationToken, GetCompatibleCells, CreateCloneCell/EnableCloneCell/
  DisableCloneCell, countersigning, ListWasmHostFunctions, ProvideMemproofs.
- **Typed DumpNetworkStats/DumpFullState responses**: `DumpNetworkStatsResponse` (with nested
  transport stats) and `FullStateDumpedResponse`/`IFullStateDumpedResponse` give typed,
  deserialized access to conductor `dump_network_stats` / `dump_state` data, while the legacy
  raw-JSON string fields (`NetworkStatsDumpJSON`, `DumpedStateJSON`) are kept as an escape hatch
  for callers depending on the old shape.
- **`NetworkConfig`** (`Data/Config/NetworkConfig.cs`) added and wired into `HoloNETDNA` to carry
  `RequestTimeoutS`, matching the move of `request_timeout_s` from `ConductorConfig` directly to
  `ConductorConfig.network` (`NetworkConfig`) in Holochain 0.6.1.
- **`CallZomeOptions`** (`Data/App/Requests/CallZomeOptions.cs`) added as an optional trailing
  parameter on `CallZomeFunctionAsync`, allowing a per-call timeout override that falls back to
  `HoloNETDNA.NetworkConfig.RequestTimeoutS`. Existing overloads remain unchanged/backward
  compatible.
- **Removed dead scaffolding**: `InitializeIntegratedKeystoreAsync` /
  `InitializeCachingLayerAsync` / `InitializeWASMOptimizationAsync` and their `Task.Delay`-only
  helper methods (the "Holochain 0.5.6+ Enhanced Features" block in `HoloNETClientBase.cs`), plus
  the unused `KeystoreConfig`/`CacheConfig`/`WASMConfig` classes and the corresponding
  `HoloNETDNA`/`IHoloNETDNA` properties. None of this mapped to any real Holochain wire protocol
  or conductor config; it was pure unused stub code with no callers outside itself. `Kitsune2Config`
  and `QUICConfig` were removed for the same reason.
- **TestHarness / doc-comment cleanup**: updated hardcoded `holochain-0.1.5` conductor/happ paths
  in `NextGenSoftware.Holochain.HoloNET.Client.TestHarness/HoloNETTestHarness.cs` to `0.6.1`, and
  fixed `docs.rs/holochain_types/0.2.1` doc-comment links (in `InstallAppRequest.cs`,
  `CoordinatorSource.cs`) to point at `docs.rs/holochain_types/0.6.1`, which is the matching crate
  version for the `holochain-0.6.1` release.

### Not verified

- No authoritative Rust struct could be found for a `CallZomeOptions`-equivalent wire type in
  `holochain_zome_types` / `holochain_conductor_api` / `holochain_types` at tag `holochain-0.6.1`;
  `CallZomeOptions` here is a HoloNET-only client-side convenience type (see its doc comment),
  not a verified mirror of a Rust struct.

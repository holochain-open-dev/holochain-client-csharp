# Changelog

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

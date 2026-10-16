# Changelog

## v4.4.2 - 2026-10-16

HoloNET API 4.4.2. Everything else is unchanged.

- **HoloNET API: one-line deletes.** `DeleteDataAsync`, `DeleteObjectAsync`, `DeleteAllDataAsync` and `DeleteAllObjectsAsync`
  are safe straight after a restart (the store is loaded first, so the deleted key can't be merged back in). The API also
  remembers the hApp it was initialised with (`HoloNETAPIDNA`), so one-line calls keep using it after `ShutdownAsync` or
  a restart. Before, they silently switched back to the bundled OASIS hApp.

## v4.4.1 - 2026-10-16

HoloNET API, Manager and HDK 4.4.1. Client, ORM and HyperNET stay at 4.4.0, unchanged.

- **HDK is the full metapackage now:** HoloNET Client, ORM and HyperNET, plus WEB5 STAR 2.0.6 and HoloOASIS 2.0.6.
  STAR couldn't be installed from NuGet before, because IPFSOASIS 2.0.4 depended on `Ipfs.Http.Client` >= 0.42.0, which
  was never published. OASIS 2.0.6 fixes that: IPFSOASIS now ships its IPFS client inside the package, and Contracts,
  ArweaveOASIS and STAR.DNA (never published at 2.0.4) are now published. OASIS 2.0.6 also builds HoloOASIS against
  HoloNET 4.4.0, so STAR's hApp generation has the classes it needs. `-p:HdkIncludeStar=false` still gives a HoloNET-only HDK.
- **API and Manager bundle the current OASIS hApp,** with HyperNET. The previous bundled build predated it, so
  `HoloNETAPI.SendSignalAsync` (and HyperNET) failed with the default hApp. A live test now installs both bundled hApps
  on 0.7.0 and 0.7.1 and joins a HyperNET zone, so a stale bundled hApp can't ship again.
- **Every GitHub release has its packages attached:** each `.nupkg` plus a `.zip` of its contents, including
  v4.2.0-v4.4.0, Libraries v2.0.4 and the OASIS hApp. The OASIS `publish-holonet.yml` job's new `release_tag` input does
  this automatically.

## v4.4.0 - 2026-10-16 (Holochain 0.7.0 and 0.7.1)

Every HoloNET package is now 4.4.0: Client, ORM, API, HyperNET, Manager and HDK, including all the Embedded variants.

### hApp generation (WEB5 STAR + HoloNET ORM)
- WEB5 STAR now generates hApps that actually build on Holochain 0.7. Its old Rust templates were written for the pre-RSM
  hdk 0.0.50 (`#[zome]`, `hdk::commit_entry`, `ZomeApiResult`), so nothing it generated could compile on current Holochain.
- New STAR templates in `DNATemplates/RustDNATemplates/Holochain07`, filled by HoloOASIS's template engine. For each zome,
  they produce an integrity zome and a coordinator zome with create, get, update, delete and get-all for every entry, plus
  anchored collections. They also write `dna.yaml`, `happ.yaml`, `build.sh` and the matching HoloNET ORM C# models.
- Choose the version with the HoloOASIS `HolochainVersion` setting (`0.7.1` by default, or `0.7.0`).
- HoloNET ORM has its own code-based generator, `HolochainHappGenerator` (plus `FromStarDnaFolder`), for apps that don't use STAR.
- Tested end to end on both versions. A sample hApp is generated from the STAR templates and compiled with cargo. Its
  generated C# models are compiled with Roslyn and drive the hApp on real 0.7.0 and 0.7.1 conductors: create, load,
  update, change tracking, delete, and collection add, load, batch save and remove (`scripts/build-generated-happ.sh`,
  `LiveGeneratedHappTests`).

### HoloNET API
- **Direct messages.** `SendSignalAsync(message, toAgentPubKeys)` and the `OnSignalReceived` event send and receive
  text messages as remote signals, peer to peer, with nothing stored. `ReceiveSignal()` now returns the last message
  received. `SendSignal()` and `ReceiveSignal()` used to be placeholders; `SendSignal()` is now obsolete.
- XML docs for every public member.

### HoloNET Client
- `IHoloNETClientAdmin` now exposes `AddAgentInfoAsync(IEnumerable<string>)`. Before, only the obsolete pre-0.7
  overload was on the interface.
- More XML docs: `InstallEnableSignAndAttachHappEventArgs`, `HoloNETClientBase` and the admin events.

### HoloNET Manager
- The **Get Storage Info**, **Add Agent Info**, **Delete Clone Cell**, **Graft Records** and **Update Coordinators**
  buttons work now; they used to say "Coming Soon".
- **Dump State**, **Dump Full State**, **Dump Network Stats** and **Dump Network Metrics** now show their results.
  Before, the results went to events nothing listened to.

### HyperNET
- XML docs for every public member. STAR's `hypernet start [zone] | stop | status | config` commands now run on
  `HyperNETService`; they used to say "Coming Soon".

### Documentation
- New `docs/` folder:
  - guides: getting started, Client, ORM, API, HyperNET, Manager, and STAR hApp generation;
  - a generated API reference for every public type and member of Client, ORM, API and HyperNET
    (`scripts/generate-api-docs.sh`).
- The README's package list now covers the four Client/ORM variants, and the stale "Release Coming Soon" text is gone.

## v4.3.0 - 2026-10-16 (HyperNET + HDK)

### HoloNET HyperNET: P2P multiplayer networking, built out
HyperNET was a placeholder until now. It is now a working library, tested live with three players on Holochain 0.7.0 and 0.7.1:
- `HyperNETSession`: join, leave and change zones. Presence in a zone is a DHT link (the only thing stored); everything
  else is a direct remote signal between players, with no server in the middle.
- Area of interest: game traffic goes to the players within `AreaOfInterestRadius`, at most `MaxFullRatePeers` (the nearest)
  at full rate. The rest of the zone gets every `FarUpdateEvery`th position update. Moves smaller than `MinimumMoveToSend` aren't sent.
- Unreliable (fire and forget) and reliable messages (acknowledged, retried, de-duplicated, `OnReliableMessageFailed`),
  typed messages via MessagePack, `PingAsync` latency, `Stats`.
- The zone list is re-read every `PeerRefreshInterval` (5 s) to catch missed join signals, and a heartbeat every
  `HeartbeatInterval` (10 s) keeps idle players from hitting `PeerTimeout`.
- `HyperNETService` and `HyperNETDNA`: start, stop and status for a long-running process (what WEB5 STAR's `hypernet` commands drive).
- The OASIS hApp has the matching zome functions (`hypernet_join`, `hypernet_leave`, `hypernet_peers`, `hypernet_send`,
  `recv_remote_signal`, plus an unrestricted grant for `recv_remote_signal` created in `init`) on main and on `holochain-0.7.0-maintenance`.
  Two bugs found by the live tests are fixed there: `recv_remote_signal` took an `ExternIO`, which failed to decode every
  real remote signal, and a multi-target `send_remote_signal` reached only one of several agents on the same conductor
  (it now sends once per player).

### HDK
- `NextGenSoftware.Holochain.HoloNET.HDK` becomes a real metapackage: HoloNET Client, ORM, HyperNET, WEB5 STAR and HoloOASIS
  (HoloNET Manager ships on its own as a desktop app). For now it carries Client, ORM and HyperNET: STAR and HoloOASIS are
  behind `-p:HdkIncludeStar=true` until their NuGet packages install cleanly (STAR 2.0.3 needs `Ipfs.Http.Client` >= 0.42.0,
  which isn't on NuGet, and HoloOASIS 2.0.4 pins the never-published HoloNET.Client 4.1.0).

### Tests
- The live-test sandbox uses Holochain's in-memory network (offline; no public bootstrap or relay).

## v4.2.0 - 2026-10-16 (Holochain 0.7.0 and 0.7.1)

Published to NuGet as 4.2.0 for every HoloNET package (Client, Client.Embedded, ORM, ORM.Embedded, HDK, HyperNET,
Manager, and HoloNET API for the first time), with NextGenSoftware libraries 2.0.4. 4.1.0 was never published, so this
release includes everything below, newest first.

### Four Client / ORM packages
- `NextGenSoftware.Holochain.HoloNET.Client` / `.ORM`: no conductor (connect to your own).
- `.Embedded`: bundles Holochain 0.7.1 and 0.7.0 (choose with `HoloNETDNA.EmbeddedHolochainVersion`, default 0.7.1).
- `.Embedded.Holochain071` / `.Embedded.Holochain070`: one version only (about half the size). Asking a single-version
  package for the other version uses the one it has and logs a warning; `HoloNETClientBase.IsEmbeddedHolochainVersionBundled`
  says what a build carries. The conductors are now embedded directly (build property `EmbeddedHolochain=both|0.7.1|0.7.0`)
  instead of through Resources.resx.

### HoloNET API overloads
- New overloads take the hApp as a `HoloNETAPIHapp` (installed app id, .happ path, role). The old `(hAppId, hAppPath, hAppRole, ...)`
  string overloads are obsolete: with three or more leading strings C# could pick the zome/function overload instead
  (e.g. `SaveAllObjectsAsync(appId, path, role)` called `SaveAllObjectsAsync(zome, createFunction, updateFunction)`).


### HoloNET ORM, HoloNET API and HoloNET Manager working end to end on Holochain 0.7.0 and 0.7.1

All three are now tested against a real conductor of **both** versions with the OASIS hApp (live tests), plus a fake
conductor / in-memory hApp for the error paths. 1,758 Client/ORM/API tests and 9 Manager tests pass on 0.7.0 and 0.7.1;
line coverage: Client 93.1%, ORM 93.2%, API 93.9%, Manager logic 79% (the rest of the Manager is WPF window code). Before this pass none of them could save and reload data on 0.7.

#### HoloNET ORM (`HoloNETEntryBase`, `HoloNETAuditEntryBase`, `HoloNETCollection`, `HoloNETObservableCollection`)
- **Entries**: updates, `Load()` and `Delete()` used the EntryHash where Holochain needs the ActionHash, so every update
  failed; loading a hash with no record reported success; the `state` field and `IsChanged` made saved entries look
  changed; `PreviousVersionActionHash` was the source-chain previous action; `OrginalEntry` was always null.
- **Audit entries**: a new `Id` on every save, only the first modification recorded, culture-dependent dates,
  `NullReferenceException` when a tracking flag was off, delete wiped `DeletedDate`, no audit trail on 0.7.
- **Collections**: add/remove sent only the anchor (not which entry), the batch couldn't tell adds/removes/updates
  apart, loads never mapped records onto `T`, reloads duplicated entries, change detection broke after the first match.
  The zome contract is now defined (see `HoloNETCollectionCore`); the OASIS hApp has matching
  `get/add/remove/batch_update_avatar_collection` functions (coordinator only, DNA hash unchanged).
- Events were only wired when the ORM object connected the client itself, so a shared client's `OnError` was never
  forwarded and the observable collection never tracked changes. Constructors now all take `IHoloNETClientAppBase`.

#### HoloNET API
Rebuilt on the 0.7 OASIS hApp with the same public methods: one store entry (`StoreId`) holding Data and Objects as JSON;
created then updated by action hash; the installed app is reused (it used to uninstall it, losing the data, on every
start); errors reported (`OnError`, `LastError`, `Message`); loaded objects convert to `T`; `ShutdownAsync`; bundled
`oasis.happ` rebuilt for 0.7. Note: overloads like `SaveAllObjectsAsync(hAppId, hAppPath, hAppRole)` and
`SaveAllObjectsAsync(zome, createFn, updateFn)` take the same string types, so the hApp versions need a fourth argument.

#### HoloNET Manager
- Demo models used zome functions that no longer exist and sent no `id`/`username`; every loaded record opened its
  own conductor connection; the demo hApp path was relative (the conductor couldn't find it).
- App interface connections had no authentication token, which Holochain 0.6+ requires, so no app connection worked.
- Work queued from WebSocket events with `Dispatcher.CurrentDispatcher` never ran; shutdown re-subscribed handlers;
  closing connections modified the list being iterated; recycling reconnected another app's client; status updates to a
  closed popup threw; creating a client from the interface-attached event deadlocked; grant/attach error events were
  treated as success (it connected twice on 0.7.0); the shared entry's hash was saved as an entry hash and couldn't be
  loaded; reboot waited for clients that were already closed and could run twice. Bundled `oasis.happ` rebuilt for 0.7.
- Results: Manager tests 9/9 on both versions (boot, the HoloNET Entry/Collection demos incl. re-enable/reconnect/
  reinstall, the shared install → grant → attach → connect → use flow with the entry form, reboot, admin queries).
- New test project `NextGenSoftware.Holochain.HoloNET.Manager.Tests` drives the Manager headless: boot, the HoloNET
  Entry and Collection demos, and the install → grant → attach → connect → use flow.

#### HoloNET Client fixes found on the way
- **Sync methods called from inside a HoloNET event deadlocked** (since they were made to wait for the conductor):
  inside a conductor callback they now send and report through the matching `On...CallBack` event, as they used to.
- `IHoloNETClientAdmin` defaulted installs to *Unrestricted* grants while the class used *Assigned*; code holding the
  interface got grants Holochain 0.7 rejects (`BadCapGrant`). A test now checks every interface default.
- `uninstallhAppIfAlreadyInstalled: false` tried to install again; it now reuses the installed app.
- `ShutdownHoloNETAsync` on an already disconnected client never completed.
- `WaitTillReadyForZomeCallsAsync` hung forever when the agent key/DNA hash were unknown and retrieval was off.
- **Request ids weren't atomic**: two requests sent at once from different threads (an app call while a HoloNET event sends
  another) could get the same id, and a reply was matched to the wrong call.
- NextGenSoftware.WebSocket: the receive loop ran on the caller's UI thread, so in a desktop app any blocking call froze it.
- NextGenSoftware.Logging: every log line scanned the whole log folder, so with a few hundred files HoloNET calls timed
  out; the folder also grew without limit. The current file is now remembered and only `MaxLogFiles` (default 50) kept.

#### Tests and tools
- `scripts/run-live-tests.sh <version>` now runs the Client/ORM/API project and the Manager project (`PROJECTS=client`
  or `manager` for one; `HAPP_OVERRIDE` to try another hApp).
- `scripts/build-test-happs.sh <version>` builds the OASIS test hApps (0.7.0 from the hApp's `holochain-0.7.0-maintenance`
  branch: the `holochain-0.7.0` tag plus the collection functions).

### Full Holochain 0.7 coverage pass: 1,647 tests, 95% line coverage, ~40 bugs fixed

Every public request method, response type and connection path now has tests, against a real conductor on **both
Holochain 0.7.0 and 0.7.1** and against a fake conductor (`FakeConductor` in the test project) that produces exact 0.7
wire shapes, errors, malformed replies and dropped connections. Results: 1,637 tests pass on each version, plus 10
conductor auto-start tests; HoloNET.Client line coverage 95.1%, branch 85.4%. What isn't covered is listed at the end.

#### Bundled conductors
- The Embedded package now carries **Holochain 0.7.1 (default) and 0.7.0**; choose with
  `HoloNETDNA.EmbeddedHolochainVersion`. Each is extracted to `%APPDATA%\NextGenSoftware\HoloNET\<version>\`.

#### Holochain 0.7 API (completed)
- New: admin `dump_conductor_state`; app-interface `agent_info`, `dump_network_metrics`, `dump_network_stats`,
  `enable_app`; paging for `dump_state` (`SourceChainCursor`) and `dump_full_state` (`DhtOpsCursor`, `limit`).
- 0.7.1: direct-signal sender (`SignalCallBackEventArgs.FromAgentPubKey`, `IsDirectSignal`); system signals raised on the
  new `OnSystemSignalCallBack` (they were dropped).
- `hc sandbox call` no longer exists in 0.7: `RetrieveAgentPubKeyAndDnaHashFromSandbox` now uses `hc client call`.

#### Decoders that never matched the 0.7 wire format
`peer_meta_info` (always empty), DNA definitions (zome dependencies / wasm hash never read), the typed full state dump
(never deserialised), `AppInfo.AppStatus` (always `None` on 0.7), clone cells in `app_info` (decode failed),
countersigning session state ("no session" was an error), scalar zome results (`()`, bool, numbers, strings were
"unknown responses"), HoloHash strings of lengths other than 39 bytes.

#### Calls that hung forever (now fail with a message)
`get_dna_definition` errors; malformed `app_installed` / `app_info` / zome replies; a dropped connection, a call made
while disconnected, or a send that failed; a reply of an unexpected type (e.g. one renamed in a future Holochain);
`RetrieveAgentPubKeyAndDnaHashFromConductor` on an `app_info` error; the conductor ↔ sandbox fallback looping when both
failed; app clients with a token connecting in callback mode; a custom logger that throws.

#### Conductor start / stop
- `UseExternal` ignored `FullPathToExternalHolochainConductorBinary`; `hc` dev-tool mode used the wrong port flag;
  "only one conductor" never detected a running one; `DisconnectAsync` returned before auto-shutdown finished.
- **Shutting down "all" conductors killed every `rustc` process on the machine** (your own builds); "current only"
  killed every holochain/hc process. Both now stop only what they should, and the hidden PowerShell host too.
- Failed config creation still launched an empty PowerShell host; the config port rewrite could hit `report: none`.
- Start waits for the admin port instead of a fixed delay (faster, and no more timeouts under load).

#### Behaviour and API fixes
- Task continuations no longer run on the WebSocket receive thread (`RunContinuationsAsynchronously`): the live suite
  went from ~35 s to ~20 s, and reused request ids (cached zome results) no longer race.
- Sync wrappers: `GetAppInfo` returned a `Task`; several `dynamic` overloads (`CallZomeFunction`, clone cells,
  `MapEntryDataObject`) returned a `Task` or threw; some returned placeholders; `.Result`/`.Wait()` replaced. Sync
  `InstallEnableSignAttachAndConnectToHapp` defaulted to `Unrestricted` grants while async used `Assigned` (now both
  `Assigned`); added the missing sync `InstallEnableSignAndAttachHapp`.
- Entry mapping: `float`, nullable and enum properties, and non-English number formats; `Record.DateTime` is UTC (it
  applied a fixed −5 h offset).
- A null request id crashed `CallZomeFunction` overloads; the void `InstallApp` without an agent key did nothing; a zome
  string parameter that merely decoded as base64 (e.g. a username) was sent as bytes.
- Clients unsubscribe from `AppDomain.UnhandledException` on `Dispose` (they leaked); faulty log providers / loggers can't
  break calls (also fixed in NextGenSoftware.Logging and NextGenSoftware.WebSocket).

**Breaking:** `GetAppInfo()` (sync) returns `AppInfoCallBackEventArgs` instead of a `Task`; `ClonedCell.clone_id` is a
`string`; `DumpFullStateAsync`'s cursor is a `DhtOpsCursor` (the int overload is kept and ignores the value); the
`AppBundle` install overloads are obsolete (an AppBundle can't be packed for 0.7).

#### Not covered (and why)
About 200 lines are `catch` blocks around code that can no longer throw (after the fixes above) and are kept as
defensive handlers; the rest are the `ShowHolochainConductorWindow` branches (would open a visible window during tests),
generated resource code, and fallbacks made unreachable by the fixes. Removing them would reach 100% but make HoloNET
less robust, so they stay.

#### Running the tests
`scripts/run-live-tests.sh 0.7.1` (or `0.7.0`) starts a fresh sandbox and runs everything with coverage; conductor
auto-start tests need `HOLONET_LIVE_AUTOSTART=1` (and `HOLONET_LIVE_EMBEDDED=1` with `-p:Embedded=true`). See
INTEGRATION_TESTING.md. `scripts/classify_uncovered.py` lists what isn't covered.

### Holochain 0.7.0 and 0.7.1 both supported; direct signals

HoloNET now works against **both Holochain 0.7.0 and 0.7.1** conductors. The full live suite (96 tests) passes on each,
on fresh sandboxes.

- **Capability grants:** 0.7.1 changed the `grant_zome_call_capability` payload (`{tag, constraint, grant: {functions}}`
  instead of 0.7.0's `{tag, access, functions}`) with no compatibility shim, so each version rejects the other's shape.
  HoloNET sends the 0.7.1 shape, retries once with the 0.7.0 shape on a "Failed to deserialize request" error, and
  remembers which one the conductor accepts. Nothing changes for callers.
- **Grant listings** (`ListCapabilityGrantsAsync`) decode both versions. Read `AccessInfo`, `Functions` and the new
  `CapabilityType` (`"zome_call"` or `"direct_signal"`) rather than the raw `access` / `functions` fields.
- **New (0.7.1+):** `GrantDirectSignalCapabilityAsync` on the admin and app clients lets other agents send direct
  signals to a cell. On 0.7.0 it returns an error (the request doesn't exist there).
- **New:** `SendDirectSignalAsync(dnaHash, agents, signal, capSecret)` on the app client. `capSecret` is only sent when
  set, so it works on 0.7.0 too. On 0.7.1 a recipient only receives the signal if it granted direct-signal access; it
  arrives on the recipient's `OnSignalCallBack`.
- The generic `ok` response is now routed by request id (`send_direct_signal` vs `provide_memproofs`).

**hApps must be rebuilt for 0.7.1.** 0.7.1 changed the `CapGrant` type, so an integrity zome built against hdk 0.7.0 /
hdi 0.8.0 fails validation on a 0.7.1 conductor ("callback has invalid parameters"). Rebuild with **hdk 0.7.1 /
hdi 0.8.1**. The OASIS hApp main branch now targets 0.7.1; the tag `holochain-0.7.0` is the last 0.7.0 build.

### Synchronous methods now return the result (behaviour change)

**Breaking for anyone relying on the old behaviour.** HoloNET's synchronous methods (the ones without `Async`, e.g.
`ListApps()`, `AttachAppInterface()`, `Connect()`, `Disconnect()`, 66 in all) used to send the request and return an
empty placeholder straight away ("please wait for the ... event"); the real result only arrived through events. They now
**wait for the conductor and return its result**, which is what a synchronous .NET method is expected to do.

- Events (`OnAppsListedCallBack` etc.) are still raised, so event-based code keeps working; it just no longer gets an
  early return.
- They run on the thread pool and are awaited with `GetAwaiter().GetResult()`, so calling them from a UI or Unity main
  thread doesn't deadlock (a live test calls them on xunit's synchronisation context).
- Prefer the `Async` methods where you can. For fire-and-forget, call the `Async` method with
  `ConductorResponseCallBackMode.UseCallBackEvents`.

The previous line of each wrapper is kept commented out next to the new one.

### Coverage push: four more 0.7.0 bugs, shared-state fixes

New live tests (`LiveConductorCoverageTests`) for paths no test reached exposed real bugs. 93 tests; all pass
live against a 0.7.0 conductor on a fresh sandbox. HoloNET.Client coverage with the conductor: **54.9% lines,
49.6% branches** (it was 46.3% / 39.9%; offline-only it is 5.2%).

#### Fixes
- **`app_info` was rejected** ("Failed to deserialize request"): 0.7.0 `AppRequest::AppInfo` is a unit
  variant, but HoloNET sent the old `{ installed_app_id }` object. It now sends no value.
- **`app_info` responses crashed decoding**: a completed `TaskCompletionSource` was completed again and
  threw. All 51 `SetResult` calls are now `TrySetResult`, so duplicate or late responses can't crash
  response handling (and leave callers waiting).
- **`list_app_interfaces` crashed decoding**: 0.7.0 returns `AppInterfaceInfo { port, allowed_origins,
  installed_app_id }`, not bare ports, and an empty list threw in `Substring`. New
  `AppInterfacesListedCallBackEventArgs.AppInterfaces`; `WebSocketPorts` still filled.
- **The synchronous `CallZomeFunction` always returned null** (regression from the earlier deadlock fix):
  every overload passed `UseCallBackEvents`, and C# bound the caller's mode argument to a `dynamic`
  parameter. All sync overloads now go through one helper that honours the mode (default: wait) and
  runs on the thread pool, so a UI/Unity thread still can't deadlock.
- **All clients shared one HoloNETDNA**: `HoloNETClientBase.HoloNETDNA` read and wrote the static
  `HoloNETDNAManager.HoloNETDNA`, so creating any client replaced every other client's DNA (admin URI,
  InstalledAppId, DnaHash, AgentPubKey). Each client now keeps its own; a client given none still
  uses the shared, file-loaded default. `SaveDNA` saves the client's DNA.
- **Shared signing credentials weren't thread-safe**: concurrent clients threw in
  `Dictionary.TryInsert`. Now a `ConcurrentDictionary`.

- **`AddAgentInfoAsync` never completed**: its result handler checked the agent-info-*returned* request map
  (copy-paste), so the waiting call was never resolved. New overload takes the encoded strings that
  `GetAgentInfoAsync` returns (`AgentInfos`); the old `AgentInfo[]` overload is obsolete.
- **New `AddAdminInterfacesAsync`** (`add_admin_interfaces`): the decoder existed but there was no request method.
- **`ListCapabilityGrantsAsync` always returned no grants**: a Holochain 0.7.0 conductor bug
  (`capability_grant_info` skips *every* grant when `include_revoked` is false, not just revoked ones).
  HoloNET now always asks for all grants and drops revoked ones itself when `includeRevoked` is false. Already reported upstream
  ([holochain/holochain#5950](https://github.com/holochain/holochain/issues/5950)) and fixed in Holochain 0.7.1
  ([#5958](https://github.com/holochain/holochain/pull/5958)); the workaround is correct on both.

#### Tests
- Live tests now report as **skipped** without a conductor (they used to pass silently).
- `coverlet.collector` added: `dotnet test --collect "XPlat Code Coverage"`.
- Live test classes run one at a time (shared sandbox; concurrent installs hit conductor DB errors).
- `RegisterDna` is checked to fail cleanly on 0.7.0 (no such admin request; already obsolete).

### Remaining app/admin APIs verified live

Live tests now cover every API area against a real 0.7.0 conductor: clone cells (create,
disable, enable, delete), deferred membrane proofs (provide → enable), WASM host functions, op
timings on both interfaces, DNA definitions, capability grants, both state dumps, token
issue/revoke, and installing from bytes. 85 tests, 14 of them live.

#### Fixes
- **Signals were never delivered**: the dispatcher routed on the inner type (which is `app`,
  `app_direct` or `system` in 0.7.0, never `signal`), and the decoder expected the pre-0.4
  `{"App": [cell, data]}` shape with agent and DNA hash swapped. Signals now route on the outer
  `WireMessage` type, skip request-id matching, and decode `{"type", "value": {cell_id, zome_name,
  signal}}` (new `SignalValue`).
- **Tuple-variant requests** sent `{field: value}` objects where the conductor wants the bare value:
  countersigning (`Box<CellId>`), `revoke_app_authentication_token` (token as an int array),
  `get_compatible_cells` (`DnaHash`). `get_dna_definition` takes a `CellId` in 0.7.0; the DNA-hash
  overload now looks up a matching cell, and a `CellId` overload was added.
- **`dump_full_state`** was routed to the `dump_state` decoder and hung; it's now decoded properly,
  and `DumpedStateJSON` is always set (the typed `DumpedState` is best-effort).
- **Decoder errors no longer hang calls**: `HandleError(args, …)` threw (the default when nothing
  subscribes to `OnError`) before decoders raised their result. It now reports the error on the result.
- **`list_wasm_host_functions`** didn't unwrap the response envelope.
- **`DumpOpTimingsAsync`** was missing from `IHoloNETClientAppBase`.
- **Install from bytes**: new `InstallAppFromBytesAsync` (`AppBundleSource::Bytes`).
- **Embedded binaries**: the hc-tool path wrote the *conductor* binary out as `hc.exe` into the wrong
  variable, and extracted binaries were never refreshed after an upgrade (only written when
  missing). Now `Resources.hc` is used, and stale copies are re-extracted.
- **Large responses hung**: `DataHelper.ConvertBinaryDataToString` (NextGenSoftware-Libraries),
  called on every response, was O(n²); a full-state dump never finished. Fixed in that repo
  (`842296f`).
- **Feature-gated APIs**: countersigning (`unstable-countersigning`) and `GetCompatibleCells`
  (`unstable-migration`) don't exist in the standard 0.7.0 conductor. They're now documented on
  the methods, and they return a clean error instead of hanging.
- CS0162 (unreachable `break`) fixed; the libraries repo's CS4014/CS0162 warnings fixed.

---

### App flow verified live against Holochain 0.7.0

HoloNET has now run the full flow against a real 0.7.0 conductor and the 0.7.0 OASIS hApp
(`C:\Source\OASIS-Holochain-hApp\workdir\oasis.happ`, hdk 0.7.0): install → enable → grant
zome-call capability → attach app interface → issue token → authenticate → zome call.

#### Fixes
- **App authentication**: `InstallEnableSignAttachAndConnectToHapp` now issues an app
  authentication token and gives it to the app agent. Without one, the conductor closes the app
  socket on the first request.
- **Response payloads**: about 17 decoders deserialized the whole `{"type", "value"}` envelope as
  the payload, so their results were empty. Affected: auth token, clone cells, countersigning,
  peer meta info, op timings, capability grants, compatible cells, state dumps, storage info and
  others. New `DeserializeResponseValue` / `DeserializeResponseValueAsJson` helpers unwrap
  `value`.
- **Dispatch hang**: the dispatcher deserialized every payload as `dynamic` just to read `type`.
  Payloads keyed by hashes (e.g. `network_metrics_dumped`) throw under
  `MessagePackSecurity.UntrustedData`, so the response was never routed and the call hung. Dispatch
  now reads only `type`.
- **Conductor errors**: the message was read from `"data"` (it's `"value"`), so it was always lost.
  The base handler also threw before the per-request error event fired, which could hang the
  caller. It now logs, and callers get `IsError` with the conductor's message.
- **Auth token encoding**: `AppAuthenticationToken` is a Rust `Vec<u8>`, which goes on the wire as
  an integer array both when issued and when used to authenticate. Fixed with
  `ByteArrayAsIntArrayFormatter`.
- **Agent info**: the request takes `dna_hashes` (`cell_id` was ignored), and the response is a
  list of encoded strings. New `AgentInfos`; `AgentInfo` is `[Obsolete]`.
- **`dump_network_metrics`** sends `{dna_hash, include_dht_summary}`. Stats and metrics are
  exposed as JSON.
- **`install_app`** sends `source` / `roles_settings`. **`attach_app_interface`** sends the
  required `allowed_origins` (default `"*"`).
- **`RegisterDna` / `GraftRecords`** are `[Obsolete]`; 0.7.0 has no such admin requests.
- **Zome results with real records**: `null` entry fields (`Option::None`) threw a
  `NullReferenceException`, and decode errors called `HandleError` before the result event fired,
  so the zome call hung. Now null-safe and non-fatal; live test decodes real 0.7.0 avatar
  `Record`s down to entry fields.
- **Sync wrappers**: the 73 `XxxAsync(...).Result` wrappers now run via `Task.Run`, so they no
  longer deadlock on a UI `SynchronizationContext`. Unawaited calls (CS4014) go through
  `FireAndForget`, which logs failures.
- **Other bundled binaries**: the `holochain.exe` copies in `Templates.MAUI` and `UnoApp.Mobile`
  were 0.1.5 and are now the verified 0.7.0 build.
- 80 tests, including 11 live tests (`HOLONET_LIVE_ADMIN_URI`, plus `HOLONET_LIVE_HAPP_PATH` for
  the app flow).

---

### Wire-format corrections (verified against holochain-0.7.0 source)

#### Breaking changes
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

#### Fixes
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

### Post-upgrade polish (v4.1.0 follow-on)

Further hardening and completeness work on top of the 0.7.0 upgrade:

- **`DesensitizedZomeCallCapGrant` and `CapAccessInfo` typed** (`Data/Admin/Responses/`):
  `CapGrantInfo.cap_grant` is now `DesensitizedZomeCallCapGrant` (typed) instead of `dynamic`.
  `CapAccessInfo` carries `access_type` (string) and optional `assignees` (AgentPubKey[]).
  Verified against `holochain_zome_types` 0.7.0.
- **DNA manifest restructured** (`Data/Admin/Requests/Objects/`). *Correction:* this was written
  without checking the Rust source, and parts were wrong (`origin_time`/`quantum_time`,
  `IntegrityZomeManifest`/`CoordinatorZomeManifest`, string coordinator dependencies). The verified
  0.7.0 shape is in "Wire-format corrections" above; the invented types are `[Obsolete]`.
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

### Holochain 0.7.0 Wire Protocol Upgrade

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

## v4.0.0 - 2026-10-11 (Holochain 0.6)

### Holochain 0.6.1 Wire Protocol Upgrade

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

#### Not verified

- No authoritative Rust struct could be found for a `CallZomeOptions`-equivalent wire type in
  `holochain_zome_types` / `holochain_conductor_api` / `holochain_types` at tag `holochain-0.6.1`;
  `CallZomeOptions` here is a HoloNET-only client-side convenience type (see its doc comment),
  not a verified mirror of a Rust struct.

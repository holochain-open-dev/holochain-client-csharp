# HoloNET Client guide

The Client is the connection to a Holochain conductor. Everything else in HoloNET (ORM, API, HyperNET, Manager) is built on it. The [API reference](api/Client.md) lists every member, and the [main README](../README.md#holonetclient) describes each event, method and property in detail.

## The two clients

| Class | Connects to | Used for |
|---|---|---|
| `HoloNETClientAdmin` | the conductor's **admin** interface | installing and managing hApps, agents, interfaces, grants, tokens, diagnostics |
| `HoloNETClientAppAgent` | an **app** interface, for one installed app | zome calls, signals, clone cells, app info |

Both take an optional `HoloNETDNA` (the settings) and an optional logger in their constructors.

## Settings: HoloNETDNA

The most used settings:

| Setting | Default | Meaning |
|---|---|---|
| `HolochainConductorAdminURI` | `ws://localhost:7777` | The admin interface to connect to. |
| `HolochainConductorAppAgentURI` | `ws://localhost:8888` | The app interface (set for you by the install-and-connect helper). |
| `HolochainConductorMode` | `UseSystemGlobal` | `UseEmbedded` starts the conductor an `.Embedded` package bundles. `UseExternal` starts one at `FullPathToExternalHolochainConductorBinary`. `UseSystemGlobal` uses the `holochain` on your PATH. |
| `EmbeddedHolochainVersion` | `V0_7_1` | Which bundled conductor to start (dual-version Embedded package). |
| `AutoStartHolochainConductor` / `AutoShutdownHolochainConductor` | `true` | Start the conductor on connect and stop it on disconnect. Turn both off for a conductor you run yourself. |
| `AppAuthenticationToken` | none | The app-interface token (Holochain 0.6+). Issued for you by the helper. |
| `InstalledAppId`, `AgentPubKey`, `DnaHash`, `CellId` | found after connect | Which app and cell zome calls go to. |
| `LogToConsole`, `LogToFile`, `LogPath`, `FileLoggingMode` | on, AppData | Logging. |

`HoloNETDNAManager` loads and saves these as `HoloNETDNA.json` if you want them in a file.

## Connecting

```csharp
var admin = new HoloNETClientAdmin(new HoloNETDNA { HolochainConductorMode = HolochainConductorModeEnum.UseEmbedded });
HoloNETConnectedEventArgs connected = await admin.ConnectAsync();
```

**The one-call install.** `InstallEnableSignAttachAndConnectToHappAsync(appId, happPath, roleName)` does every step Holochain 0.7 needs:

1. Generates an agent key and installs the app. An app that's already installed is kept, with its agent, unless you pass `uninstallhAppIfAlreadyInstalled: true`.
2. Enables it.
3. Creates signing credentials and an **assigned** zome-call grant.
4. Attaches an app interface.
5. Issues an app authentication token.
6. Returns a connected `HoloNETClientAppAgent` in `HoloNETClientAppAgent`.

Use `InstallEnableSignAndAttachHappAsync` if you want to connect the app client yourself.

**Connecting an app client manually:**

```csharp
var token = await admin.IssueAppAuthenticationTokenAsync("my-app");
var client = new HoloNETClientAppAgent("my-app", agentPubKey, new HoloNETDNA
{
    AppAuthenticationToken = token.TokenIssued.token,
    AutoStartHolochainConductor = false
});
await client.ConnectAsync("my-app", "ws://localhost:" + appPort);
await client.WaitTillReadyForZomeCallsAsync();
```

## Zome calls

```csharp
ZomeFunctionCallBackEventArgs r = await client.CallZomeFunctionAsync("zome", "function", new { field = "value" });
```

- **What you pass.** `paramsObject` can be an anonymous object, a dictionary, a class or a primitive. It's serialised to MessagePack using its field names.
- **What comes back.** The decoded return value is in `ZomeReturnValue` and `ZomeReturnData`, and the record hash in `ZomeReturnHash`. `Records` holds decoded Holochain records. Pass `entryDataObjectTypeReturnedFromZome` to have entries mapped onto your type.
- **Errors.** `IsError` and `Message` tell you if the conductor or your zome returned an error. Calls never throw for zome errors.
- **Callbacks and ids.** Overloads take a callback and/or a request `id` so you can match responses yourself. `ConductorResponseCallBackMode.UseCallBackEvents` fires `OnZomeFunctionCallBack` instead of waiting.

## Signals

- **Emitted signals.** When your zome calls `emit_signal`, `client.OnSignalCallBack` fires. `e.RawSignalData.Data` holds the raw MessagePack bytes, and the decoded forms are on the event args.
- **Remote signals between agents.** These use `send_remote_signal` and `recv_remote_signal` in your zome. The OASIS hApp's `hypernet_*` functions are a ready-made example: [HyperNET](hypernet-guide.md) and [HoloNET API signals](api-guide.md#direct-messages) are built on them.
- **Direct signals between apps.** `GrantDirectSignalCapabilityAsync` and `SendDirectSignalAsync` send these.

## The admin API

All of these have `Async` and sync versions:

| Area | Methods |
|---|---|
| Agents | `GenerateAgentPubKeyAsync`, `GetAgentInfoAsync`, `AddAgentInfoAsync`, `GetPeerMetaInfoAsync` |
| Apps | `InstallAppAsync`, `InstallAppFromBytesAsync`, `UninstallAppAsync`, `EnableAppAsync`, `DisableAppAsync`, `ListAppsAsync`, `GetAppInfoAsync` |
| DNAs and cells | `RegisterDnaAsync`, `ListDnasAsync`, `ListCellIdsAsync`, `GetDnaDefinitionAsync`, `GetCompatibleCellsAsync`, `UpdateCoordinatorsAsync`, `DeleteCloneCellAsync`, `GraftRecordsAsync` |
| Interfaces | `AttachAppInterfaceAsync`, `ListInterfacesAsync`, `AddAdminInterfacesAsync` |
| Security | `AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync`, `RevokeZomeCallCapabilityAsync`, `ListCapabilityGrantsAsync`, `IssueAppAuthenticationTokenAsync`, `RevokeAppAuthenticationTokenAsync`, `GrantDirectSignalCapabilityAsync` |
| Diagnostics | `GetStorageInfoAsync`, `DumpStateAsync`, `DumpFullStateAsync`, `DumpConductorStateAsync`, `DumpNetworkStatsAsync`, `DumpNetworkMetricsAsync`, `DumpOpTimingsAsync` |

## The app API

| Area | Methods |
|---|---|
| Zome calls | `CallZomeFunctionAsync` (many overloads), `WaitTillReadyForZomeCallsAsync` |
| App info | `GetAppInfoAsync`, `RetrieveAgentPubKeyAndDnaHashAsync` |
| Clone cells | `CreateCloneCellAsync`, `EnableCloneCellAsync`, `DisableCloneCellAsync` |
| Membrane proofs | `ProvideMemproofsAsync` (for apps installed with deferred proofs) |
| Countersigning | `GetCountersigningSessionStateAsync`, `AbandonCountersigningSessionAsync`, `PublishCountersigningSessionAsync` |
| Network | `DumpNetworkStatsAsync`, `DumpNetworkMetricsAsync`, `GetAppPeerMetaInfoAsync` |
| Signals | `SendDirectSignalAsync`, plus the `OnSignalCallBack` event |

## Events

Every request also raises an event when its response arrives, for example `OnAppInstalledCallBack`, `OnZomeFunctionCallBack` and `OnSignalCallBack`. There are also lifecycle events: `OnConnected`, `OnDisconnected`, `OnError`, `OnReadyForZomeCalls`, `OnHolochainConductorStarting`, `OnHolochainConductorStarted`, `OnDataSent` and `OnDataReceived`. The README has [the complete list](../README.md#events).

## Shutting down

```csharp
await client.DisconnectAsync();
await admin.ShutdownHoloNETAsync(); // disconnects and stops the conductor HoloNET started (if AutoShutdownHolochainConductor)
```

## Threading and sync calls

- **Thread safety.** Request ids are allocated atomically, so one client can be used from several threads.
- **Sync calls inside events.** The sync wrappers (`CallZomeFunction`, `InstallApp`, ...) are safe to call from inside HoloNET's own event handlers: HoloNET detects this and avoids deadlocking.
- **UI apps.** HoloNET's receive loop runs off the UI thread. Marshal back to the UI thread yourself in WPF, WinForms or Unity.

## Unity

Use the netstandard2.1 build from the same NuGet packages. The HoloUnity notes are in the [main README](../README.md#holounity).

# HoloNET HyperNET API reference

Generated from `NextGenSoftware.Holochain.HoloNET.HyperNET` 4.4.0.0 (every public type and member, with its XML documentation where it has one). For guides and examples see the [documentation index](../README.md).

- **NextGenSoftware.Holochain.HoloNET.HyperNET**: [HyperNET](#nextgensoftware-holochain-holonet-hypernet-hypernet), [HyperNETDNA](#nextgensoftware-holochain-holonet-hypernet-hypernetdna), [HyperNETDNAManager](#nextgensoftware-holochain-holonet-hypernet-hypernetdnamanager), [HyperNETErrorEventArgs](#nextgensoftware-holochain-holonet-hypernet-hyperneterroreventargs), [HyperNETMessageEventArgs](#nextgensoftware-holochain-holonet-hypernet-hypernetmessageeventargs), [HyperNETOptions](#nextgensoftware-holochain-holonet-hypernet-hypernetoptions), [HyperNETPeer](#nextgensoftware-holochain-holonet-hypernet-hypernetpeer), [HyperNETPeerEventArgs](#nextgensoftware-holochain-holonet-hypernet-hypernetpeereventargs), [HyperNETReliableFailedEventArgs](#nextgensoftware-holochain-holonet-hypernet-hypernetreliablefailedeventargs), [HyperNETService](#nextgensoftware-holochain-holonet-hypernet-hypernetservice), [HyperNETSession](#nextgensoftware-holochain-holonet-hypernet-hypernetsession), [HyperNETStats](#nextgensoftware-holochain-holonet-hypernet-hypernetstats), [HyperNETTarget](#nextgensoftware-holochain-holonet-hypernet-hypernettarget), [HyperNETVector3](#nextgensoftware-holochain-holonet-hypernet-hypernetvector3)

## NextGenSoftware.Holochain.HoloNET.HyperNET

<a id="nextgensoftware-holochain-holonet-hypernet-hypernet"></a>
### HyperNET (static class)

HoloNET HyperNET: peer-to-peer networking for games on Holochain. Start with `HyperNETSession` (join a zone, share positions, send messages to the players around you) or `HyperNETService` (the whole thing from a config file, as STAR's "hypernet" commands use it).

| Member | Kind | Description |
|---|---|---|
| `const string Version` | field | This is the current version of the HolochainEntry. Each time it is saved with the Save method it will be incremented. **NOTE:** You will need to ensure you add a corresponding property in the rust entry struct for your hApp (pub version: i32), which will then map to this property. |

<a id="nextgensoftware-holochain-holonet-hypernet-hypernetdna"></a>
### HyperNETDNA (class)

HyperNET service settings (STAR's "hypernet config" edits this file).

| Member | Kind | Description |
|---|---|---|
| `string HappPath { get; set; }` | property | The .happ to install when InstalledAppId isn't installed yet (the OASIS hApp has the HyperNET zome functions). |
| `string HolochainConductorAdminURI { get; set; }` | property | The conductor's admin interface. Empty: HoloNET's default (its embedded conductor when using an Embedded package). |
| `string InstalledAppId { get; set; }` | property | The installed app id to use (installed from HappPath if missing; an existing install and its agent are kept). |
| `HyperNETOptions Options { get; set; }` | property | Session behaviour (area of interest, reliability, timeouts). |
| `string PlayerName { get; set; }` | property | Your display name in the zone. |
| `string RoleName { get; set; }` | property | The role in the hApp that has the HyperNET zome. |
| `string Zone { get; set; }` | property | The zone to join on start. |

<a id="nextgensoftware-holochain-holonet-hypernet-hypernetdnamanager"></a>
### HyperNETDNAManager (static class)

| Member | Kind | Description |
|---|---|---|
| `string HyperNETDNAPath { get; set; } (static)` | property | Where the settings live by default: %APPDATA%\NextGenSoftware\HoloNET\HyperNETDNA.json. |
| `static HyperNETDNA Load(string path = null)` | method | Load the settings (defaults if the file doesn't exist). |
| `static void Save(HyperNETDNA dna, string path = null)` | method | Save the settings (creating the folder if needed). |

<a id="nextgensoftware-holochain-holonet-hypernet-hyperneterroreventargs"></a>
### HyperNETErrorEventArgs (class)

Inherits / implements: `EventArgs`

| Member | Kind | Description |
|---|---|---|
| `Exception Exception { get; }` | property | The exception behind it, if there was one. |
| `string Message { get; }` | property | What went wrong. |

<a id="nextgensoftware-holochain-holonet-hypernet-hypernetmessageeventargs"></a>
### HyperNETMessageEventArgs (class)

Inherits / implements: `EventArgs`

| Member | Kind | Description |
|---|---|---|
| `string Channel { get; }` | property | The channel the sender used (your own label, e.g. "chat", "shot", "trade"). |
| `HyperNETPeer From { get; }` | property | The player who sent it. |
| `byte[] Payload { get; }` | property | The message bytes (use HyperNETSession.Deserialize for a typed message). |
| `bool Reliable { get; }` | property | True when it was sent reliably (acknowledged; delivered once). |
| `DateTime SentAt { get; }` | property | When the sender sent it (UTC, their clock). |

<a id="nextgensoftware-holochain-holonet-hypernet-hypernetoptions"></a>
### HyperNETOptions (class)

How a HyperNET session behaves. The defaults suit a typical action game.

| Member | Kind | Description |
|---|---|---|
| `float AreaOfInterestRadius { get; set; }` | property | Players further away than this aren't in your area of interest (in world units). |
| `int FarUpdateEvery { get; set; }` | property | Players in the zone but outside the area of interest (or beyond MaxFullRatePeers) get every Nth position update. |
| `TimeSpan HeartbeatInterval { get; set; }` | property | How often a tiny "still here" packet goes to everyone in the zone, so players who stand still don't hit PeerTimeout. Keep it well under PeerTimeout. Zero turns it off. |
| `int MaxFullRatePeers { get; set; }` | property | At most this many players (the nearest) get every update. In a crowd this caps how much each player sends, so a busy spot doesn't overload anyone's connection: the rest get updates less often (see FarUpdateEvery). |
| `float MinimumMoveToSend { get; set; }` | property | Only send a position update when you moved at least this far since the last one sent (0 = always). |
| `TimeSpan PeerRefreshInterval { get; set; }` | property | How often the zone's player list is re-read from the DHT, to pick up players whose "joined" signal was missed (signals aren't guaranteed) or who joined at the same moment as you. Zero turns it off. |
| `TimeSpan PeerTimeout { get; set; }` | property | A player not heard from for this long is treated as gone (OnPeerLeft) unless they rejoin. |
| `int ReliableMaxRetries { get; set; }` | property | ...up to this many times, then reported as failed (OnReliableMessageFailed). |
| `TimeSpan ReliableRetryInterval { get; set; }` | property | Reliable messages are re-sent if not acknowledged within this time... |
| `string ZomeName { get; set; }` | property | The zome the HyperNET functions live in (the OASIS hApp's "oasis" zome by default). |

<a id="nextgensoftware-holochain-holonet-hypernet-hypernetpeer"></a>
### HyperNETPeer (class)

Another player in your zone.

| Member | Kind | Description |
|---|---|---|
| `string AgentPubKey { get; }` | property | The player's agent public key (how HyperNET addresses them; pass it to SendAsync / PingAsync). |
| `bool IsInAreaOfInterest { get; }` | property | True when they're in your area of interest (updated whenever positions change). |
| `DateTime LastSeen { get; }` | property | When you last heard from them (UTC). |
| `double? LatencyMs { get; }` | property | The last measured round trip to them in milliseconds (null until pinged). |
| `string PlayerName { get; }` | property | The display name the player joined with. |
| `HyperNETVector3? Position { get; }` | property | Their last known position (null until they've sent one). |
| `string ToString()` | method | The player's name, key and (if known) position. |

<a id="nextgensoftware-holochain-holonet-hypernet-hypernetpeereventargs"></a>
### HyperNETPeerEventArgs (class)

Inherits / implements: `EventArgs`

| Member | Kind | Description |
|---|---|---|
| `HyperNETPeer Peer { get; }` | property | The player who joined, left or moved. |

<a id="nextgensoftware-holochain-holonet-hypernet-hypernetreliablefailedeventargs"></a>
### HyperNETReliableFailedEventArgs (class)

Inherits / implements: `EventArgs`

| Member | Kind | Description |
|---|---|---|
| `string Channel { get; }` | property | The channel of the message that failed. |
| `byte[] Payload { get; }` | property | The bytes of the message that failed. |
| `IReadOnlyList<string> UnacknowledgedAgentPubKeys { get; }` | property | The players that never acknowledged it. |

<a id="nextgensoftware-holochain-holonet-hypernet-hypernetservice"></a>
### HyperNETService (class)

The HyperNET service: connects to the conductor, installs the HyperNET hApp if needed (keeping an installed one and its agent), and joins the configured zone. What STAR's "hypernet start / stop / status" commands drive.

| Member | Kind | Description |
|---|---|---|
| `HyperNETService(HyperNETDNA dna = null)` | ctor | A service for these settings (or the saved ones from HyperNETDNAPath). |
| `IHoloNETClientAdmin Admin { get; }` | property | The admin connection while running. |
| `IHoloNETClientAppAgent Client { get; }` | property | The app connection the session uses while running. |
| `HyperNETDNA DNA { get; }` | property | The settings the service runs with. |
| `bool IsRunning { get; }` | property | True while connected and joined. |
| `string LastError { get; }` | property | Why the last StartAsync failed. |
| `HyperNETSession Session { get; }` | property | The zone session while running (subscribe to its events for messages and players). |
| `DateTime? StartedAt { get; }` | property | When it started (UTC), while running. |
| `Task<bool> StartAsync()` | method | Connect, install/attach the hApp if needed and join the zone. Returns false (with LastError) on failure; true at once if already running. |
| `string Status()` | method | A one-paragraph status (STAR's "hypernet status"). |
| `Task StopAsync()` | method | Leave the zone and disconnect. |

<a id="nextgensoftware-holochain-holonet-hypernet-hypernetsession"></a>
### HyperNETSession (class)

Your connection to one zone of the game world. Players in the zone talk to each other directly (Holochain remote signals, agent to agent, nothing stored), so there is no central server to become the bottleneck; and each player only exchanges game traffic with the players in their area of interest, so adding players elsewhere costs nothing. Zone membership is the only thing written to the DHT (once per zone change). Positions, game messages, acks and pings all travel as signals.

Inherits / implements: `IDisposable`

| Member | Kind | Description |
|---|---|---|
| `HyperNETSession(IHoloNETClientAppAgent client, string zone, string playerName, HyperNETOptions options = null)` | ctor | A session for one player (one connected app agent) in a zone. Call JoinAsync to enter it. |
| `bool IsJoined { get; }` | property | True between a successful JoinAsync and LeaveAsync. |
| `HyperNETOptions Options { get; }` | property | How the session behaves (area of interest, reliability, timeouts). |
| `IReadOnlyList<HyperNETPeer> Peers { get; }` | property | Everyone else in your zone. |
| `IReadOnlyList<HyperNETPeer> PeersInAreaOfInterest { get; }` | property | The players currently in your area of interest (nearest first). |
| `string PlayerName { get; }` | property | Your display name, sent to the other players. |
| `HyperNETVector3? Position { get; }` | property | Your last position (from UpdatePositionAsync); null until you set one. |
| `HyperNETStats Stats { get; }` | property | Traffic, reliability and latency counters for this session. |
| `string Zone { get; }` | property | The zone you're in (changes with ChangeZoneAsync). |
| `EventHandler<HyperNETErrorEventArgs> OnError` | event | Something failed (a zome call, a malformed signal); the session keeps running. |
| `EventHandler<HyperNETMessageEventArgs> OnMessage` | event | A game message arrived (reliable ones are raised once, even if re-sent). |
| `EventHandler<HyperNETPeerEventArgs> OnPeerJoined` | event | A player entered the zone (or was found by a zone refresh). |
| `EventHandler<HyperNETPeerEventArgs> OnPeerLeft` | event | A player left the zone, or wasn't heard from for PeerTimeout. |
| `EventHandler<HyperNETPeerEventArgs> OnPeerMoved` | event | A player sent a new position (Peer.Position). |
| `EventHandler<HyperNETReliableFailedEventArgs> OnReliableMessageFailed` | event | A reliable message wasn't acknowledged by everyone after ReliableMaxRetries. |
| `Task<bool> ChangeZoneAsync(string zone)` | method | Move to another zone (e.g. you crossed a zone boundary): leave this one, join the new one. |
| `static T Deserialize<T>(byte[] payload)` | method | Read a typed message sent with SendAsync<T> (MessagePack, contractless). |
| `void Dispose()` | method | Stop the background maintenance and stop listening for signals (call LeaveAsync first to tell the others). |
| `Task<bool> JoinAsync()` | method | Join the zone: announces you to the players already there and fetches who they are. |
| `Task LeaveAsync()` | method | Leave the zone (the others get told). |
| `Task<double?> PingAsync(string agentPubKey, TimeSpan? timeout = null)` | method | Measure the round trip to a player (also updates `LatencyMs` and the stats). |
| `Task RefreshPeersAsync()` | method | Refresh who's in the zone from the DHT (peers also announce themselves when they join, so this is only needed after missing signals, e.g. on reconnect). |
| `Task<int> SendAsync(string channel, byte[] payload, HyperNETTarget target = AreaOfInterest, bool reliable = false, IEnumerable<string> agentPubKeys = null)` | method | Send a game message. Unreliable (the default) is fire and forget: right for state that the next update replaces. Reliable messages are acknowledged and re-sent until they arrive (or OnReliableMessageFailed): use them for events that must not be lost (a trade, a hit, a chat line). |
| `Task<int> SendAsync<T>(string channel, T value, HyperNETTarget target = AreaOfInterest, bool reliable = false, IEnumerable<string> agentPubKeys = null)` | method | Send an object (serialised with MessagePack); the receiver reads it with `Deserialize``1`. |
| `Task UpdatePositionAsync(HyperNETVector3 position)` | method | Tell the other players where you are. The players in your area of interest get every update; the rest of the zone every `FarUpdateEvery`th one, so they still know roughly where you are. |

<a id="nextgensoftware-holochain-holonet-hypernet-hypernetstats"></a>
### HyperNETStats (class)

Counters for a session (what STAR's "hypernet status" shows).

| Member | Kind | Description |
|---|---|---|
| `double? AverageLatencyMs { get; }` | property | Average of the peers' last measured round trips, in milliseconds (null until pinged). |
| `long BytesReceived { get; }` | property | Payload bytes received. |
| `long BytesSent { get; }` | property | Payload bytes sent (counted once per recipient). |
| `long DuplicatesDropped { get; }` | property | Re-sent reliable messages that had already been delivered, so were dropped instead of raised twice. |
| `long MessagesReceived { get; }` | property | Packets received from other players. |
| `long MessagesSent { get; }` | property | Packets sent (one per recipient). |
| `long PositionUpdatesSkipped { get; }` | property | Position updates not sent because you moved less than MinimumMoveToSend. |
| `long ReliableFailures { get; }` | property | Reliable messages given up on after ReliableMaxRetries (each raised OnReliableMessageFailed). |
| `long ReliableRetries { get; }` | property | Times a reliable message was re-sent because not every recipient had acknowledged it yet. |
| `string ToString()` | method | All the counters on one line (what STAR's "hypernet status" prints). |

<a id="nextgensoftware-holochain-holonet-hypernet-hypernettarget"></a>
### HyperNETTarget (enum)

Who a message goes to.

| Value | Description |
|---|---|
| `AreaOfInterest` | The players in your area of interest: within `AreaOfInterestRadius`, the nearest `MaxFullRatePeers` of them. |
| `Zone` | Everyone in your zone. |
| `Specific` | Only the players you list. |

<a id="nextgensoftware-holochain-holonet-hypernet-hypernetvector3"></a>
### HyperNETVector3 (struct)

A position in the game world (any units; only distances between players matter to HyperNET).

| Member | Kind | Description |
|---|---|---|
| `HyperNETVector3(float x, float y, float z)` | ctor | A position from its three coordinates. |
| `float X { get; set; }` | property | The X coordinate. |
| `float Y { get; set; }` | property | The Y coordinate. |
| `float Z { get; set; }` | property | The Z coordinate. |
| `float DistanceTo(HyperNETVector3 other)` | method | The straight-line distance to another position. |
| `string ToString()` | method | The position as (X, Y, Z). |


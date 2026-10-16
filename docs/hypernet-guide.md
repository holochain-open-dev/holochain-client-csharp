# HoloNET HyperNET guide

HyperNET is P2P multiplayer networking for games (Unity, Unreal through .NET, or any .NET app) on Holochain 0.7.0 and 0.7.1. Players talk to each other directly over Holochain remote signals. There's no game server in the middle, so there's no server bottleneck, no server bill and no server-side player cap. Every member is listed in the [API reference](api/HyperNET.md).

```bash
dotnet add package NextGenSoftware.Holochain.HoloNET.HyperNET
```

## How it works

| Idea | What HyperNET does |
|---|---|
| **Zones** | The world is split into zones: a level, a region, a shard of an open world. Joining a zone writes one link to the DHT. That is the only thing HyperNET stores. |
| **Direct messages** | Positions, game events, acks and pings go straight from player to player as remote signals. Nothing is stored and nothing passes through a server. |
| **Area of interest** | Game traffic goes only to players within `AreaOfInterestRadius`, and only the nearest `MaxFullRatePeers` of them get every update. The rest of the zone gets every `FarUpdateEvery`th position update, so they still know roughly where you are. |
| **Reliability when you need it** | Messages are fire-and-forget by default, which suits state the next update replaces. Reliable messages are acknowledged, re-sent, delivered exactly once, or reported as failed. |
| **Presence** | Joins and leaves are announced by signal. The zone list is re-read from the DHT every `PeerRefreshInterval` to catch missed signals, and a heartbeat every `HeartbeatInterval` keeps idle players from timing out. |

Because of the area of interest, **each player's traffic depends on how crowded their surroundings are, not on how many players are online**. That's what lets the number of players grow without a central limit.

## A session

```csharp
using NextGenSoftware.Holochain.HoloNET.HyperNET;

// One connected app agent per player. The OASIS hApp includes the HyperNET zome functions.
var session = new HyperNETSession(client, zone: "forest-1", playerName: "Alice", new HyperNETOptions
{
    AreaOfInterestRadius = 100,
    MaxFullRatePeers = 64,
    FarUpdateEvery = 10
});

session.OnPeerJoined += (s, e) => SpawnAvatar(e.Peer.AgentPubKey, e.Peer.PlayerName);
session.OnPeerLeft   += (s, e) => RemoveAvatar(e.Peer.AgentPubKey);
session.OnPeerMoved  += (s, e) => MoveAvatar(e.Peer.AgentPubKey, e.Peer.Position.Value);
session.OnMessage    += (s, e) => HandleMessage(e.From, e.Channel, e.Payload, e.Reliable);
session.OnReliableMessageFailed += (s, e) => Log($"'{e.Channel}' not acknowledged by {e.UnacknowledgedAgentPubKeys.Count} players");
session.OnError      += (s, e) => Log(e.Message);

if (!await session.JoinAsync())
    return;

// Every tick: only sent if you moved at least MinimumMoveToSend, and only to the players who need it.
await session.UpdatePositionAsync(new HyperNETVector3(x, y, z));

// Game events go to your area of interest by default...
await session.SendAsync("shot", new byte[] { 1 });
// ...or to the whole zone...
await session.SendAsync("chat", Encoding.UTF8.GetBytes("hi all"), HyperNETTarget.Zone, reliable: true);
// ...or to specific players, as a typed object.
await session.SendAsync("trade", new Trade { Item = "sword", Qty = 1 }, HyperNETTarget.Specific, reliable: true, agentPubKeys: new[] { bob });

// The receiver reads a typed message with:
Trade trade = HyperNETSession.Deserialize<Trade>(e.Payload);

double? ms = await session.PingAsync(bob);       // round trip, also kept in Peer.LatencyMs and Stats.AverageLatencyMs

await session.ChangeZoneAsync("forest-2");        // crossing a zone boundary
await session.LeaveAsync();                       // the others are told
session.Dispose();
```

To find out who's around:

- `Peers` lists everyone in the zone.
- `PeersInAreaOfInterest` lists the nearby players, nearest first.
- Each `HyperNETPeer` has `PlayerName`, `AgentPubKey`, `Position`, `LastSeen`, `LatencyMs` and `IsInAreaOfInterest`.

## Options

| Option | Default | Meaning |
|---|---|---|
| `ZomeName` | `oasis` | The zome with the HyperNET functions. |
| `AreaOfInterestRadius` | 100 | Players further away than this (in world units) aren't in your area of interest. |
| `MaxFullRatePeers` | 64 | At most this many players (the nearest) get every update; this caps your traffic in a crowd. |
| `FarUpdateEvery` | 10 | Players outside your area of interest get every Nth position update. |
| `MinimumMoveToSend` | 0.01 | Smaller moves aren't sent. |
| `ReliableRetryInterval` | 500 ms | How long to wait for acks before re-sending. |
| `ReliableMaxRetries` | 5 | Then `OnReliableMessageFailed`. |
| `PeerRefreshInterval` | 5 s | Re-read the zone list from the DHT. Zero turns it off. |
| `HeartbeatInterval` | 10 s | Send a "still here" packet. Keep it well under `PeerTimeout`. Zero turns it off. |
| `PeerTimeout` | 30 s | A player not heard from for this long is treated as gone. |

`Stats` keeps the counters: messages and bytes sent and received, reliable retries and failures, duplicates dropped, position updates skipped and average latency.

## The service (long-running processes, STAR)

`HyperNETService` does the setup for you:

1. Connects to the conductor (the embedded one, if you use an `.Embedded` Client package).
2. Installs the hApp if needed, keeping an existing install and its agent.
3. Joins the zone from `HyperNETDNA`.

```csharp
var service = new HyperNETService(new HyperNETDNA { Zone = "lobby", PlayerName = "Alice", HappPath = "oasis.happ" });
if (await service.StartAsync())
    Console.WriteLine(service.Status());   // zone, players, area of interest, stats
service.Session.OnMessage += ...;
await service.StopAsync();
```

The settings are kept in `%APPDATA%\NextGenSoftware\HoloNET\HyperNETDNA.json`; use `HyperNETDNAManager.Load` and `Save` to read and write them. WEB5 STAR's commands use this service:

```
star hypernet start [zone]
star hypernet status
star hypernet stop
star hypernet config
```

## The zome functions

HyperNET needs these in your hApp. The OASIS hApp has them in `src/hypernet.rs`, which you can copy.

| Function | What it does |
|---|---|
| `hypernet_join({ session, player_name })` | Links you to the zone, tells the players already there, and returns the zone's player list. |
| `hypernet_leave(session)` | Removes your link and tells the others. |
| `hypernet_peers(session)` | Returns the zone's player list. |
| `hypernet_send({ session, payload, to })` | Sends `payload` to the agents in `to`, or to the whole zone if `to` is empty. Returns how many it went to. |
| `recv_remote_signal(signal: HyperNETSignal)` | Receives a signal and passes it to the client with `emit_signal`. |
| `init` | Creates an **unrestricted** capability grant for `recv_remote_signal`. Without it, nobody can deliver signals to you. |

Two Holochain details that are easy to get wrong:

- **Take the struct, not `ExternIO`.** Declare `recv_remote_signal` to take your signal struct. Holochain hands over the signal exactly as `send_remote_signal` encoded it, so an `ExternIO` parameter fails to decode every real signal.
- **Send once per target.** Call `send_remote_signal` separately for each recipient. On Holochain 0.7.1, a single call with several targets on the same conductor reached only one of them.

## The honest limits

- **It removes the server, not the network.** Latency is still the real distance between two players, and players behind strict NATs relay through a Holochain relay.
- **Crowds still cost the players inside them.** A huge crowd in one spot means more traffic for those players. `MaxFullRatePeers` caps it, so nobody gets flooded, at the cost of less frequent updates from the furthest of them.
- **Signals aren't guaranteed.** That's why reliable messages, zone refresh and heartbeats exist.
- **There's no server-side authority.** Anti-cheat has to come from the hApp's validation rules (for anything stored) or from players checking each other.

## Tested

The live tests run three players in one zone on real Holochain 0.7.0 and 0.7.1 conductors:

- joins and leaves;
- unreliable and reliable messages, including ack and exactly-once delivery;
- ping;
- area of interest, position updates and minimum-move skipping;
- zone changes;
- the service.

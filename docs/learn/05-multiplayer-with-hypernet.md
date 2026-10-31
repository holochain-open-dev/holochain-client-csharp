# Real-time multiplayer with HyperNET

**You'll build:** a game loop where players in a zone see each other move and exchange messages directly, peer to peer, with no game server. Only nearby players get your traffic.
**Time:** 25 minutes. **You need:** a running conductor, and the OASIS hApp (it has HyperNET's zome functions).
**Finished code:** [`examples/04-HyperNET`](https://github.com/holochain-open-dev/holochain-client-csharp/blob/main/examples/04-HyperNET/Program.cs), which runs three players in one process.

## 1. Add HyperNET

```bash
dotnet add package NextGenSoftware.Holochain.HoloNET.HyperNET
```

## 2. Join a zone

Each player is one Holochain agent: one app client. Connect it as in [the first tutorial](01-first-zome-call.md), then:

```csharp
using NextGenSoftware.Holochain.HoloNET.HyperNET;

var session = new HyperNETSession(client, zone: "forest-1", playerName: "Alice", new HyperNETOptions
{
    AreaOfInterestRadius = 100,   // only players this close get your game traffic
    MaxFullRatePeers = 64         // and at most the nearest 64 get every update
});

session.OnPeerJoined += (s, e) => Spawn(e.Peer);
session.OnPeerMoved  += (s, e) => Move(e.Peer, e.Peer.Position.Value);
session.OnMessage    += (s, e) => Handle(e.From, e.Channel, e.Payload);
await session.JoinAsync();
```

## 3. The game loop

```csharp
// Every tick: only sent if you moved, and only to the players who need it.
await session.UpdatePositionAsync(new HyperNETVector3(x, y, z));

// Game events go to your area of interest by default.
await session.SendAsync("shot", new byte[] { 1 });

// Things that must arrive are sent reliably: acknowledged, retried, delivered once.
await session.SendAsync("trade", new Trade { Item = "sword", Quantity = 1 }, HyperNETTarget.Specific,
    reliable: true, agentPubKeys: new[] { bobAgentPubKey });
```

To read a typed message, use `HyperNETSession.Deserialize<Trade>(e.Payload)`. To measure latency, use `await session.PingAsync(bobAgentPubKey)`.

## 4. Moving between zones

```csharp
await session.ChangeZoneAsync("forest-2");   // the players you leave are told; the new zone's players see you join
```

## What just happened

- **Presence.** Joining wrote one link to the DHT (your presence in the zone). Everything after that went straight from player to player.
- **Area of interest.** It keeps each player's traffic down to how crowded their surroundings are, not how many players are online. That's what lets a world grow without a central limit.
- **The honest limits.** Latency is still real network distance. Signals aren't guaranteed, which is why reliable messages exist. And with no server there's no server-side authority, so anti-cheat belongs in your hApp's validation rules.

## Next

- The [HyperNET guide](https://docs.holonet.oasisomniverse.one/hypernet-guide) covers every option, the service, and the zome functions to copy into your own hApp.
- [Generate a hApp](06-generate-a-happ.md) for your game's own data.

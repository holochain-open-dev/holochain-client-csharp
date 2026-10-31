// HoloNET example 04: HoloNET HyperNET - P2P multiplayer with no game server
//
// HyperNET lets players talk to each other directly over Holochain remote signals: zones, area of interest (you only send
// to the players near you), unreliable and reliable messages, ping, and stats. This example runs three players in one
// process (each its own installed app, so its own agent); in a real game each player runs your app on their own machine.
//
//   Alice is at (0,0,0), Bob is 10 units away (inside Alice's area of interest), Cara is 500 units away (outside it).
//
// Run: start a conductor (examples/start-conductor.sh), then   dotnet run --project examples/04-HyperNET
// Docs: https://docs.holonet.oasisomniverse.one/hypernet-guide  ·  API reference: docs/api/HyperNET.md

using HoloNET.Examples;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.HyperNET;

var admin = new HoloNETClientAdmin(ExampleSetup.ConductorDNA());
ExampleSetup.Check((await admin.ConnectAsync(ExampleSetup.AdminUri)).IsConnected, "connect to the conductor at " + ExampleSetup.AdminUri);

string zone = "forest-" + Guid.NewGuid().ToString("N").Substring(0, 6);
var options = new HyperNETOptions
{
    AreaOfInterestRadius = 50,   // players further than this aren't "near"
    MaxFullRatePeers = 64,       // at most this many (the nearest) get every update
    FarUpdateEvery = 1000        // everyone else gets every 1000th position update (huge here, to make the demo obvious)
};

var players = new List<(string AppId, HyperNETSession Session)>();
try
{
    // ---- Each player: an app agent and a session in the zone ------------------------------------------------------
    foreach (string name in new[] { "Alice", "Bob", "Cara" })
    {
        string appId = ExampleSetup.UniqueAppId("example-hypernet-" + name.ToLower());
        var installed = await admin.InstallEnableSignAttachAndConnectToHappAsync(appId, ExampleSetup.HappPath, "oasis");
        ExampleSetup.Check(installed.IsAppConnected, $"install {name}: {installed.Message}");

        var session = new HyperNETSession(installed.HoloNETClientAppAgent, zone, name, options);
        string me = name;
        // Everything arrives as events.
        session.OnPeerJoined += (s, e) => Console.WriteLine($"  [{me}] {e.Peer.PlayerName} joined");
        session.OnPeerLeft += (s, e) => Console.WriteLine($"  [{me}] {e.Peer.PlayerName} left");
        session.OnMessage += (s, e) => Console.WriteLine($"  [{me}] '{e.Channel}' from {e.From.PlayerName}{(e.Reliable ? " (reliable)" : "")}: {Describe(e)}");
        session.OnReliableMessageFailed += (s, e) => Console.WriteLine($"  [{me}] '{e.Channel}' was never acknowledged by {e.UnacknowledgedAgentPubKeys.Count} player(s)");
        session.OnError += (s, e) => Console.WriteLine($"  [{me}] error: {e.Message}");
        players.Add((appId, session));
    }
    var (alice, bob, cara) = (players[0].Session, players[1].Session, players[2].Session);

    // ---- Join the zone ---------------------------------------------------------------------------------------------
    ExampleSetup.Title("Join zone '" + zone + "'");
    foreach (var p in players)
        ExampleSetup.Check(await p.Session.JoinAsync(), "join");
    await Wait(() => players.All(p => p.Session.Peers.Count == 2), refresh: true);
    Console.WriteLine($"Alice sees: {string.Join(", ", alice.Peers.Select(p => p.PlayerName))}");

    // ---- Positions and area of interest ----------------------------------------------------------------------------
    ExampleSetup.Title("Positions and area of interest");
    await bob.UpdatePositionAsync(new HyperNETVector3(10, 0, 0));
    await cara.UpdatePositionAsync(new HyperNETVector3(500, 0, 0));
    await alice.UpdatePositionAsync(new HyperNETVector3(0, 0, 0));   // the first update goes to the whole zone
    await Wait(() => alice.Peers.All(p => p.Position.HasValue));
    foreach (var peer in alice.Peers)
        Console.WriteLine($"Alice sees {peer.PlayerName} at {peer.Position} - in her area of interest? {peer.IsInAreaOfInterest}");

    // ---- Messages ----------------------------------------------------------------------------------------------------
    ExampleSetup.Title("Messages");
    // Game traffic goes to your area of interest by default: Bob gets this, Cara (500 away) doesn't.
    await alice.SendAsync("shot", new byte[] { 1 });
    // To the whole zone.
    await alice.SendAsync("chat", System.Text.Encoding.UTF8.GetBytes("hello everyone"), HyperNETTarget.Zone);
    // To one player, as a typed object, reliably (acknowledged, retried until it arrives, delivered exactly once).
    await bob.SendAsync("trade", new Trade { Item = "sword", Quantity = 1 }, HyperNETTarget.Specific, reliable: true,
        agentPubKeys: new[] { cara.AgentPubKey });
    await Task.Delay(2000);

    // ---- Latency and stats -------------------------------------------------------------------------------------------
    ExampleSetup.Title("Ping and stats");
    double? ms = await alice.PingAsync(bob.AgentPubKey);
    Console.WriteLine($"Alice -> Bob round trip: {ms:0.0} ms");
    Console.WriteLine("Alice's stats: " + alice.Stats);

    // ---- Moving between zones ----------------------------------------------------------------------------------------
    ExampleSetup.Title("Bob crosses into the next zone");
    ExampleSetup.Check(await bob.ChangeZoneAsync(zone + "-east"), "change zone");
    await Wait(() => alice.Peers.Count == 1);
    Console.WriteLine($"Alice now sees: {string.Join(", ", alice.Peers.Select(p => p.PlayerName))}");
}
finally
{
    ExampleSetup.Title("Leave and clean up");
    foreach (var (appId, session) in players)
    {
        try { await session.LeaveAsync(); } catch { }
        session.Dispose();
        await admin.UninstallAppAsync(appId);
    }
    await admin.DisconnectAsync();
    Console.WriteLine("Done.");
}

// Typed messages are read with HyperNETSession.Deserialize<T>.
static string Describe(HyperNETMessageEventArgs e) => e.Channel switch
{
    "trade" => HyperNETSession.Deserialize<Trade>(e.Payload) is var t ? $"{t.Quantity} x {t.Item}" : "",
    "chat" => System.Text.Encoding.UTF8.GetString(e.Payload),
    _ => $"{e.Payload.Length} byte(s)"
};

// Wait (up to 30 s) for something to become true; optionally re-read the zone list while waiting (players that join at
// the same moment can miss each other's join signal; HyperNET also does this itself every PeerRefreshInterval).
async Task Wait(Func<bool> condition, bool refresh = false)
{
    for (int i = 0; i < 300 && !condition(); i++)
    {
        if (refresh && i % 20 == 19)
            foreach (var p in players) await p.Session.RefreshPeersAsync();
        await Task.Delay(100);
    }
    ExampleSetup.Check(condition(), "wait");
}

public class Trade
{
    public string Item { get; set; }
    public int Quantity { get; set; }
}

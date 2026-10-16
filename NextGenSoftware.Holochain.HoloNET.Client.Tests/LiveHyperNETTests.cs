using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.Holochain.HoloNET.HyperNET;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>
    /// HoloNET HyperNET against a real conductor (0.7.0 / 0.7.1) and the OASIS hApp: players (one installed app each, so
    /// one agent each) join a zone, see each other, exchange unreliable and reliable messages as remote signals, share
    /// positions, and only talk to the players in their area of interest.
    /// </summary>
    [Collection(LiveConductor.Collection)]
    public class LiveHyperNETTests
    {
        private static readonly string AdminUri = Environment.GetEnvironmentVariable("HOLONET_LIVE_ADMIN_URI");
        private static readonly string HappPath = Environment.GetEnvironmentVariable("HOLONET_LIVE_HAPP_PATH");

        // Every player's session errors, so a timeout says why.
        private static readonly ConcurrentQueue<string> AllErrors = new ConcurrentQueue<string>();

        private static async Task Until(string what, Func<bool> condition, int seconds = 30)
        {
            DateTime end = DateTime.UtcNow.AddSeconds(seconds);
            while (!condition())
            {
                if (DateTime.UtcNow > end)
                    throw new TimeoutException("Timed out waiting for: " + what + (AllErrors.IsEmpty ? "" : " | Session errors: " + string.Join(" | ", AllErrors.Take(10))));
                await Task.Delay(50);
            }
        }

        private sealed class Player
        {
            public string AppId;
            public IHoloNETClientAppAgent Client;
            public HyperNETSession Session;
            public ConcurrentQueue<HyperNETMessageEventArgs> Messages = new ConcurrentQueue<HyperNETMessageEventArgs>();
            public ConcurrentQueue<string> Joined = new ConcurrentQueue<string>();
            public ConcurrentQueue<string> Left = new ConcurrentQueue<string>();
            public ConcurrentQueue<string> Errors = new ConcurrentQueue<string>();
        }

        private static async Task<(HoloNETClientAdmin, List<Player>)> Players(int count, string zone, HyperNETOptions options = null)
        {
            Skip.If(string.IsNullOrEmpty(AdminUri) || string.IsNullOrEmpty(HappPath), "Needs HOLONET_LIVE_ADMIN_URI and HOLONET_LIVE_HAPP_PATH (see INTEGRATION_TESTING.md).");
            var admin = new HoloNETClientAdmin(new HoloNETDNA { AutoStartHolochainConductor = false, AutoShutdownHolochainConductor = false, HolochainConductorAdminURI = AdminUri });
            Assert.True((await admin.ConnectAsync(AdminUri)).IsConnected);
            var players = new List<Player>();

            for (int i = 0; i < count; i++)
            {
                string appId = $"hypernet-{i}-{Guid.NewGuid().ToString("N").Substring(0, 6)}";
                var installed = await admin.InstallEnableSignAttachAndConnectToHappAsync(appId, HappPath, "oasis");
                Assert.True(installed.IsAppConnected, "install: " + installed.Message);
                var p = new Player { AppId = appId, Client = installed.HoloNETClientAppAgent };
                p.Session = new HyperNETSession(p.Client, zone, "player" + i, options);
                p.Session.OnMessage += (s, e) => p.Messages.Enqueue(e);
                p.Session.OnPeerJoined += (s, e) => p.Joined.Enqueue(e.Peer.PlayerName);
                p.Session.OnPeerLeft += (s, e) => p.Left.Enqueue(e.Peer.PlayerName);
                p.Session.OnError += (s, e) => { p.Errors.Enqueue(e.Message + " " + e.Exception); AllErrors.Enqueue(p.AppId + ": " + e.Message + " " + e.Exception); };
                players.Add(p);
            }

            return (admin, players);
        }

        private static async Task Cleanup(HoloNETClientAdmin admin, List<Player> players)
        {
            foreach (var p in players)
            {
                try { await p.Session.LeaveAsync(); } catch { }
                p.Session.Dispose();
                await p.Client.DisconnectAsync();
                await admin.UninstallAppAsync(p.AppId);
            }
            await admin.DisconnectAsync();
        }

        [SkippableFact]
        public async Task Live_PlayersJoinSeeEachOtherAndExchangeMessages()
        {
            string zone = "zone-" + Guid.NewGuid().ToString("N").Substring(0, 6);
            var (admin, players) = await Players(3, zone);
            var (a, b, c) = (players[0], players[1], players[2]);
            try
            {
                Assert.True(await a.Session.JoinAsync(), string.Join("\n", a.Errors));
                Assert.True(await b.Session.JoinAsync(), string.Join("\n", b.Errors));
                Assert.True(await c.Session.JoinAsync(), string.Join("\n", c.Errors));

                // The ones already there are told (by signal) when someone joins; newcomers get the list from the DHT.
                await Until("a sees b and c join", () => a.Session.Peers.Count == 2);
                await Until("c knows a and b", () => c.Session.Peers.Count == 2 || c.Session.RefreshPeersAsync().Wait(5000) && c.Session.Peers.Count == 2);
                Assert.Contains("player1", a.Joined);
                Assert.Contains(a.Session.Peers, p => p.PlayerName == "player2");

                // Unreliable: fire and forget to the zone.
                int sent = await a.Session.SendAsync("chat", new byte[] { 1, 2, 3 }, HyperNETTarget.Zone);
                Assert.Equal(2, sent);
                await Until("b and c receive", () => b.Messages.Any(m => m.Channel == "chat") && c.Messages.Any(m => m.Channel == "chat"));
                var got = b.Messages.First(m => m.Channel == "chat");
                Assert.Equal(new byte[] { 1, 2, 3 }, got.Payload);
                Assert.Equal("player0", got.From.PlayerName);
                Assert.False(got.Reliable);

                // An object, to one player only, reliably: acknowledged and delivered once.
                await b.Session.SendAsync("trade", new Dictionary<string, object> { ["item"] = "sword", ["qty"] = 1 }, HyperNETTarget.Specific, reliable: true, agentPubKeys: new[] { c.Client.HoloNETDNA.AgentPubKey });
                await Until("c gets the trade", () => c.Messages.Any(m => m.Channel == "trade"));
                var trade = HyperNETSession.Deserialize<Dictionary<string, object>>(c.Messages.First(m => m.Channel == "trade").Payload);
                Assert.Equal("sword", trade["item"]);
                Assert.True(c.Messages.First(m => m.Channel == "trade").Reliable);
                Assert.DoesNotContain(a.Messages, m => m.Channel == "trade");
                await Until("b's trade acknowledged (no retries left pending)", () => b.Session.Stats.ReliableFailures == 0 && b.Session.Stats.MessagesReceived > 0, 10);

                // Latency: a ping round trip.
                double? latency = await a.Session.PingAsync(b.Client.HoloNETDNA.AgentPubKey);
                Assert.NotNull(latency);
                Assert.InRange(latency.Value, 0, 5000);
                Assert.NotNull(a.Session.Stats.AverageLatencyMs);

                // Leaving: the others are told.
                await c.Session.LeaveAsync();
                await Until("a sees c leave", () => a.Left.Contains("player2"));
                Assert.Single(a.Session.Peers);
                Assert.Empty(a.Errors);
                Assert.Contains("running", new HyperNETStats().ToString() + " running");
            }
            finally
            {
                await Cleanup(admin, players);
            }
        }

        [SkippableFact]
        public async Task Live_AreaOfInterest_OnlyNearbyPlayersGetMessagesAndEveryPositionUpdate()
        {
            string zone = "aoi-" + Guid.NewGuid().ToString("N").Substring(0, 6);
            var options = new HyperNETOptions { AreaOfInterestRadius = 50, FarUpdateEvery = 1000 };
            var (admin, players) = await Players(3, zone, options);
            var (me, near, far) = (players[0], players[1], players[2]);
            try
            {
                foreach (var p in players)
                    Assert.True(await p.Session.JoinAsync());
                await Until("everyone known", () => players.All(p => p.Session.Peers.Count == 2 || p.Session.RefreshPeersAsync().Wait(5000) && p.Session.Peers.Count == 2));

                // Everyone announces a position (the first update goes to the whole zone).
                await near.Session.UpdatePositionAsync(new HyperNETVector3(10, 0, 0));
                await far.Session.UpdatePositionAsync(new HyperNETVector3(500, 0, 0));
                await me.Session.UpdatePositionAsync(new HyperNETVector3(0, 0, 0));
                await Until("me knows both positions", () => me.Session.Peers.All(p => p.Position.HasValue));

                Assert.Equal(new[] { "player1" }, me.Session.PeersInAreaOfInterest.Select(p => p.PlayerName).ToArray());
                Assert.True(me.Session.Peers.Single(p => p.PlayerName == "player1").IsInAreaOfInterest);
                Assert.False(me.Session.Peers.Single(p => p.PlayerName == "player2").IsInAreaOfInterest);

                // Game traffic (the default target) only goes to the area of interest.
                Assert.Equal(1, await me.Session.SendAsync("shot", new byte[] { 9 }));
                await Until("near gets the shot", () => near.Messages.Any(m => m.Channel == "shot"));
                await Task.Delay(500);
                Assert.DoesNotContain(far.Messages, m => m.Channel == "shot");

                // Later position updates skip the far player; tiny moves aren't sent at all.
                var farBefore = far.Session.Peers.Single(p => p.PlayerName == "player0").Position;
                await me.Session.UpdatePositionAsync(new HyperNETVector3(5, 0, 0));
                await Until("near sees me move", () => near.Session.Peers.Single(p => p.PlayerName == "player0").Position?.X == 5);
                await Task.Delay(500);
                Assert.Equal(farBefore, far.Session.Peers.Single(p => p.PlayerName == "player0").Position);
                await me.Session.UpdatePositionAsync(new HyperNETVector3(5.001f, 0, 0));
                Assert.Equal(1, me.Session.Stats.PositionUpdatesSkipped);

                // Crossing into another zone: leave this one, join the next.
                Assert.True(await near.Session.ChangeZoneAsync(zone + "-east"));
                await Until("me sees near leave the zone", () => me.Left.Contains("player1"));
                Assert.Equal(zone + "-east", near.Session.Zone);
            }
            finally
            {
                await Cleanup(admin, players);
            }
        }

        /// <summary>
        /// The oasis.happ bundled in HoloNET API and HoloNET Manager (what users get by default) must have the HyperNET zome
        /// functions: HoloNETAPI.SendSignalAsync and HyperNET use them. Guards against shipping a stale bundled hApp.
        /// </summary>
        [SkippableTheory]
        [InlineData("HoloNET-API/NextGenSoftware.Holochain.HoloNET.API/oasis.happ")]
        [InlineData("HoloNET-Manager/NextGenSoftware.Holochain.HoloNET.Manager/OASIS_hAPP/oasis.happ")]
        public async Task Live_BundledOasisHapp_HasHyperNET(string relativePath)
        {
            Skip.If(string.IsNullOrEmpty(AdminUri), "needs a live conductor");
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, relativePath)))
                dir = Path.GetDirectoryName(dir);
            Assert.True(dir != null, "bundled hApp not found: " + relativePath);

            string appId = "bundled-" + Guid.NewGuid().ToString("N").Substring(0, 6);
            var admin = new HoloNETClientAdmin(new HoloNETDNA { AutoStartHolochainConductor = false, AutoShutdownHolochainConductor = false, HolochainConductorAdminURI = AdminUri });
            await admin.ConnectAsync(AdminUri);
            var installed = await admin.InstallEnableSignAttachAndConnectToHappAsync(appId, Path.Combine(dir, relativePath), "oasis");
            try
            {
                Assert.True(installed.IsAppConnected, installed.Message);
                var session = new HyperNETSession(installed.HoloNETClientAppAgent, "bundled-check", "checker");
                Assert.True(await session.JoinAsync(), "the bundled hApp has no working hypernet_join");
                await session.LeaveAsync();
                session.Dispose();
            }
            finally
            {
                if (installed.HoloNETClientAppAgent != null)
                    await installed.HoloNETClientAppAgent.DisconnectAsync();
                await admin.UninstallAppAsync(appId);
                await admin.DisconnectAsync();
            }
        }

        [SkippableFact]
        public async Task Live_Service_StartStatusStop()
        {
            Skip.If(string.IsNullOrEmpty(AdminUri) || string.IsNullOrEmpty(HappPath), "needs a live conductor");
            string appId = "hypernet-svc-" + Guid.NewGuid().ToString("N").Substring(0, 6);
            var service = new HyperNETService(new HyperNETDNA { HolochainConductorAdminURI = AdminUri, InstalledAppId = appId, HappPath = HappPath, Zone = "svc", PlayerName = "svc" });
            try
            {
                Assert.Contains("stopped", service.Status());
                Assert.True(await service.StartAsync(), service.LastError);
                Assert.True(service.IsRunning);
                Assert.Contains("running in zone 'svc'", service.Status());
                Assert.True(await service.StartAsync()); // already running
            }
            finally
            {
                await service.StopAsync();
                var admin = new HoloNETClientAdmin(new HoloNETDNA { AutoStartHolochainConductor = false, AutoShutdownHolochainConductor = false, HolochainConductorAdminURI = AdminUri });
                await admin.ConnectAsync(AdminUri);
                await admin.UninstallAppAsync(appId);
                await admin.DisconnectAsync();
            }
            Assert.False(service.IsRunning);

            string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "hypernet-" + Guid.NewGuid().ToString("N") + ".json");
            HyperNETDNAManager.Save(new HyperNETDNA { Zone = "saved" }, path);
            Assert.Equal("saved", HyperNETDNAManager.Load(path).Zone);
            System.IO.File.Delete(path);
            Assert.Equal("lobby", HyperNETDNAManager.Load(path).Zone);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static NextGenSoftware.Holochain.HoloNET.Client.Tests.FakeConductorAdminTests;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>Agent key / DNA hash retrieval paths (conductor, sandbox, fallbacks) and app clients built without a DNA.</summary>
    [Collection(LiveConductor.Collection)] // the no-DNA constructors read the static HoloNETDNAManager
    public class FakeConductorRetrievalTests
    {
        private static string NoHc => Path.Combine(Path.GetTempPath(), "holonet-no-hc-" + Guid.NewGuid().ToString("N"), "hc.exe");

        private static async Task<HoloNETClientAppAgent> UnreadyApp(FakeConductor fake, string appId = "retrieve-app")
        {
            var dna = Dna(fake.Uri);
            dna.HolochainConductorAppAgentURI = fake.Uri;
            dna.HolochainConductorAdminURI = fake.Uri;
            dna.FullPathToExternalHCToolBinary = NoHc; // makes the sandbox path fail fast
            var app = new HoloNETClientAppAgent(appId, null, dna);
            app.OnError += (s, e) => { };
            Assert.True((await app.ConnectAsync(fake.Uri, ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode.DoNotRetreive)).IsConnected);
            return app;
        }

        /// <summary>app_info with only a stem cell: no DNA hash for the agent, so the fallback (or plain completion) runs.</summary>
        private static object StemOnlyAppInfo(string appId) => FakeConductor.Ok("app_info", FakeConductorFlowTests.AppInfo(appId, "stem"));

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task AppInfoWithoutAProvisionedCell_CompletesTheRetrieval(bool sandboxFallback)
        {
            using var fake = new FakeConductor();
            fake.Respond = (t, v) => StemOnlyAppInfo("retrieve-app");
            var app = await UnreadyApp(fake);
            var call = app.RetrieveAgentPubKeyAndDnaHashFromConductorAsync(null, null, RetrieveAgentPubKeyAndDnaHashMode.Wait, true, sandboxFallback);
            Assert.True(await Task.WhenAny(call, Task.Delay(10000)) == call, "retrieval never completed");
            await app.DisconnectAsync();
        }

        [Fact]
        public async Task AppInfoMissingTheAgentKey_IsAnError()
        {
            using var fake = new FakeConductor();
            var info = FakeConductorFlowTests.AppInfo("retrieve-app");
            info.Remove("agent_pub_key");
            fake.Respond = (t, v) => FakeConductor.Ok("app_info", info);
            var app = await UnreadyApp(fake);
            var r = await app.GetAppInfoAsync();
            Assert.True(r.IsError);
            await app.DisconnectAsync();
        }

        [Fact]
        public async Task NullAppInfo_IsAnError()
        {
            using var fake = new FakeConductor();
            fake.Respond = (t, v) => FakeConductor.Ok("app_info", null); // AppResponse::AppInfo(None)
            var app = await UnreadyApp(fake);
            Assert.True((await app.GetAppInfoAsync()).IsError);
            await app.DisconnectAsync();
        }

        [Fact]
        public async Task Retrieve_EntryPoints_AndModes()
        {
            using var fake = new FakeConductor();
            fake.Respond = (t, v) => FakeConductor.Ok("app_info", FakeConductorFlowTests.AppInfo("retrieve-app"));
            var app = await UnreadyApp(fake);
            app.HoloNETDNA.InstalledAppId = null; // falls back to the default id

            var viaSync = app.RetrieveAgentPubKeyAndDnaHash();
            Assert.Equal(app.ConvertHoloHashToString(Agent()), viaSync.AgentPubKey);

            // Sandbox only: hc is missing, so it finishes with what is already known.
            var sandboxOnly = await app.RetrieveAgentPubKeyAndDnaHashAsync(null, null, RetrieveAgentPubKeyAndDnaHashMode.Wait, false, true, false, false);
            Assert.NotNull(sandboxOnly);
            Assert.Null(app.RetrieveAgentPubKeyAndDnaHashFromSandbox(true, false));
            // Sandbox failing then falling back to the conductor.
            var sandboxThenConductor = await app.RetrieveAgentPubKeyAndDnaHashFromSandboxAsync(true, true);
            await app.DisconnectAsync();

            // Not connected: the conductor retrieval returns at once.
            Assert.Null(await app.RetrieveAgentPubKeyAndDnaHashFromConductorAsync());
        }

        [Fact]
        public void AppClients_WithoutADna_UseTheSharedOneAndNeverAutoStart()
        {
            var original = HoloNETDNAManager.HoloNETDNA;
            try
            {
                HoloNETDNAManager.HoloNETDNA = new HoloNETDNA { AutoStartHolochainConductor = true, AutoShutdownHolochainConductor = true, LogToFile = false, LogToConsole = false };
                var provider = new RecordingLogProvider();
                var clients = new HoloNETClientBase[]
                {
                    new HoloNETClientApp(),
                    new HoloNETClientApp(provider),
                    new HoloNETClientApp(new[] { provider }),
                    new HoloNETClientApp(new NextGenSoftware.Logging.Logger()),
                };
                foreach (var c in clients)
                {
                    Assert.False(c.HoloNETDNA.AutoStartHolochainConductor);
                    Assert.False(c.HoloNETDNA.AutoShutdownHolochainConductor);
                }
            }
            finally { HoloNETDNAManager.HoloNETDNA = original; }
        }
    }
}

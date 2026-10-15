using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using Xunit;
using static NextGenSoftware.Holochain.HoloNET.Client.Tests.FakeConductorAdminTests;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>Exposes protected static helpers.</summary>
    public class HelperProbe : HoloNETClientAdmin
    {
        public HelperProbe() : base(Dna("ws://127.0.0.1:1")) { }
        public static void Extract(string path, byte[] bytes) => ExtractEmbeddedBinary(path, bytes);
    }

    public class SmallUnitTests
    {
        [Fact]
        public void HoloHashes_RoundTripForAnyLength_AndAcceptUnpaddedStrings()
        {
            var c = new HoloNETClientAdmin(Dna("ws://127.0.0.1:1"));
            foreach (int length in new[] { 32, 33, 34, 36, 39 })
            {
                byte[] bytes = Enumerable.Range(1, length).Select(i => (byte)i).ToArray();
                string s = c.ConvertHoloHashToString(bytes);
                Assert.Equal(bytes, c.ConvertHoloHashToBytes(s));
                Assert.Equal(bytes, c.ConvertHoloHashToBytes(s.TrimEnd('='))); // Holochain's format is unpadded
            }
            Assert.Null(c.ConvertHoloHashToBytes("u!!!not base64"));
            Assert.Null(c.ConvertHoloHashToString(null));
        }

        [Fact]
        public void Signature_RequiresSixtyFourBytes()
        {
            Assert.Equal(64, new Signature(new byte[64]).Value.Length);
            Assert.Throws<ArgumentException>(() => new Signature(new byte[10]));
        }

        [Fact]
        public void HolochainRustFieldName_Attribute()
        {
            var a = new HolochainRustFieldName("first_name");
            Assert.NotNull(a);
            Assert.NotNull(new HolochainRustFieldName("x", false));
        }

        [Fact]
        public void PublicStructs_PropertiesWork()
        {
            foreach (var t in typeof(HoloNETClientBase).Assembly.GetTypes().Where(t => t.IsPublic && t.IsValueType && !t.IsEnum && !t.IsPrimitive))
            {
                object instance = Activator.CreateInstance(t);
                foreach (var p in t.GetProperties().Where(p => p.GetIndexParameters().Length == 0))
                {
                    if (p.CanRead && p.GetGetMethod() != null) p.GetValue(instance);
                    if (p.CanWrite && p.GetSetMethod() != null)
                    {
                        object value = p.PropertyType == typeof(string) ? "s" : p.PropertyType.IsValueType ? Activator.CreateInstance(p.PropertyType) : null;
                        p.SetValue(instance, value);
                    }
                }
            }
        }

        [Fact]
        public void ByteArrayAsIntArrayFormatter_NilBothWays()
        {
            var f = new ByteArrayAsIntArrayFormatter();
            var buffer = new System.Buffers.ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(buffer);
            f.Serialize(ref writer, null, FakeConductor.Options);
            writer.Flush();
            var reader = new MessagePackReader(buffer.WrittenMemory);
            Assert.Null(f.Deserialize(ref reader, FakeConductor.Options));
        }

        [Fact]
        public void EnvelopeWithoutAType_ReadsNull()
        {
            byte[] env = MessagePackSerializer.Serialize(new Dictionary<string, object> { ["value"] = 1, ["other"] = "x" });
            Assert.Null(HoloNETClientBase.ReadResponseType(env));
        }

        [Fact]
        public void ConvertEntryValue_NullsAndDateTimeOffset()
        {
            Assert.Equal(0, HoloNETClientAppBase.ConvertEntryValue(null, typeof(int)));
            Assert.Null(HoloNETClientAppBase.ConvertEntryValue("null", typeof(int?)));
            Assert.Equal("null", HoloNETClientAppBase.ConvertEntryValue("null", typeof(string)));
            Assert.Equal(new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero), HoloNETClientAppBase.ConvertEntryValue("2026-01-02T03:04:05+00:00", typeof(DateTimeOffset)));
            Assert.Equal("null", HoloNETClientAppBase.ToInvariantString(null));
            Assert.Equal("1.5", HoloNETClientAppBase.ToInvariantString(1.5));
        }

        [Fact]
        public void ExtractEmbeddedBinary_WritesOnlyWhenMissingOrDifferent()
        {
            string dir = Path.Combine(Path.GetTempPath(), "holonet-extract-" + Guid.NewGuid().ToString("N"));
            string path = Path.Combine(dir, "sub", "bin.exe");
            try
            {
                HelperProbe.Extract(path, new byte[] { 1, 2, 3 });
                Assert.Equal(3, new FileInfo(path).Length);
                DateTime written = File.GetLastWriteTimeUtc(path);
                HelperProbe.Extract(path, new byte[] { 4, 5, 6 }); // same size: kept
                Assert.Equal(new byte[] { 1, 2, 3 }, File.ReadAllBytes(path));
                HelperProbe.Extract(path, new byte[] { 7, 8, 9, 10 }); // different size: replaced
                Assert.Equal(4, new FileInfo(path).Length);
            }
            finally { try { Directory.Delete(dir, true); } catch { } }
        }

        [Fact]
        public void SourceChainCursor_Forms() =>
            Assert.Equal(new[] { "ActionHash" }, ((Dictionary<string, object>)SourceChainCursor.ActionHash(Action())).Keys);

        [Fact]
        public void IsHoloNETDNALoaded_ReflectsTheManager() =>
            Assert.Equal(HoloNETDNAManager.IsLoaded, new HoloNETClientAdmin(Dna("ws://127.0.0.1:1")).IsHoloNETDNALoaded);

        [Fact]
        public void Clients_WithTheDefaultLoggerAlso_LogBothWays()
        {
            var provider = new RecordingLogProvider();
            var dna = Dna("ws://127.0.0.1:1");
            var a = new HoloNETClientAdmin(provider, true, dna);
            var b = new HoloNETClientAdmin(new[] { provider }, true, dna);
            Assert.Equal(2, a.Logger.LogProviders.Count);
            Assert.Equal(2, b.Logger.LogProviders.Count);
        }

        [Fact]
        public async Task DirectSignalGrant_EveryAccessType_AndAllowedOriginsForms()
        {
            var (fake, admin) = await ConnectAdmin();
            using (fake)
            {
                var seen = new List<byte[]>();
                fake.Respond = (t, v) =>
                {
                    if (t == "grant_direct_signal_capability") { seen.Add(v); return FakeConductor.Ok("direct_signal_capability_granted", Action()); }
                    return FakeConductor.Ok("app_interfaces_listed", new object[]
                    {
                        new Dictionary<string, object> { ["port"] = 1, ["allowed_origins"] = null, ["installed_app_id"] = null },
                        new Dictionary<string, object> { ["port"] = 2, ["allowed_origins"] = new[] { "a", "b" }, ["installed_app_id"] = "x" },
                        new Dictionary<string, object> { ["port"] = 3, ["allowed_origins"] = 7, ["installed_app_id"] = null },
                    });
                };
                var cell = new[] { Dna(), Agent() };
                Assert.False((await admin.GrantDirectSignalCapabilityAsync(cell, "t", CapGrantAccessType.Transferable, new byte[64])).IsError);
                Assert.False((await admin.GrantDirectSignalCapabilityAsync(cell, "t", CapGrantAccessType.Assigned, new byte[64], new[] { Agent(3) })).IsError);
                Assert.Contains("transferable", MessagePackSerializer.ConvertToJson(seen[0]));
                Assert.Contains("assigned", MessagePackSerializer.ConvertToJson(seen[1]));

                var interfaces = await admin.ListInterfacesAsync();
                Assert.False(interfaces.IsError, interfaces.Message);
                Assert.Null(interfaces.AppInterfaces[0].AllowedOrigins);
                Assert.Equal("a,b", interfaces.AppInterfaces[1].AllowedOrigins);
                Assert.Equal("7", interfaces.AppInterfaces[2].AllowedOrigins);
                await admin.DisconnectAsync();
            }
        }

        [Fact]
        public async Task CallbackModeAndObsoletePaths()
        {
            var (fake, admin) = await ConnectAdmin();
            using (fake)
            {
                admin.OnError += (s, e) => { };
                fake.Respond = (t, v) => FakeConductor.Ok("full_state_dumped", new Dictionary<string, object>());
#pragma warning disable CS0618
                Assert.True((await admin.InstallAppAsync("x", new AppBundle())).IsError); // AppBundle can't be packed
#pragma warning restore CS0618
                var withIntCursor = await admin.DumpFullStateAsync(new[] { Dna(), Agent() }, 5); // obsolete int cursor: ignored with a warning
                Assert.NotNull(withIntCursor);
                var shutdown = await admin.ShutdownHoloNETAsync(DisconnectedCallBackMode.UseCallBackEvents, ShutdownHolochainConductorsMode.DoNotShutdownAnyConductors);
                Assert.NotNull(shutdown);
            }

            using var appFake = new FakeConductor();
            var app = await FakeConductorAppErrorTests.ConnectApp(appFake);
            appFake.Respond = (t, v) => FakeConductor.Ok("list_wasm_host_functions", new[] { "f" });
            var fire = await app.ListWasmHostFunctionsAsync(ConductorResponseCallBackMode.UseCallBackEvents);
            Assert.False(fire.IsError);
            Assert.Contains("UseCallBackEvents", fire.Message);
            await app.DisconnectAsync();
            var whileDown = await app.ListWasmHostFunctionsAsync(ConductorResponseCallBackMode.UseCallBackEvents);
            Assert.True(whileDown.IsError, "a fire-and-forget request made while disconnected must report it");

            // GetCellIdAsync on an app with no agent key retrieves it first.
            appFake.Respond = (t, v) => FakeConductor.Ok("app_info", FakeConductorFlowTests.AppInfo("app"));
            var app2 = await FakeConductorAppErrorTests.ConnectApp(appFake);
            app2.HoloNETDNA.AgentPubKey = null;
            var cell = await app2.GetCellIdAsync();
            Assert.Equal(Agent(), cell[1]);
            var ready = await app2.WaitTillReadyForZomeCallsAsync();
            await app2.DisconnectAsync();
            app2.Dispose();
        }

        [Fact]
        public async Task RejectedAppToken_IsReported()
        {
            using var fake = new FakeConductor { RejectAuthentication = true };
            var dna = Dna(fake.Uri);
            dna.HolochainConductorAppAgentURI = fake.Uri;
            dna.AppAuthenticationToken = new byte[] { 1 };
            var app = new HoloNETClientAppAgent("app", null, dna);
            app.OnError += (s, e) => { };
            var r = await app.ConnectAsync(fake.Uri);
            Assert.True(r.IsError || !r.IsConnected, "a rejected token must not report a usable connection");
        }

        [Fact]
        public async Task InstallFlow_AppConnectFails_IsReported()
        {
            var (fake, admin) = await ConnectAdmin();
            using (fake)
            {
                admin.OnError += (s, e) => { };
                fake.Respond = (type, value) => type switch
                {
                    "list_apps" => FakeConductor.Ok("apps_listed", new object[0]),
                    "generate_agent_pub_key" => FakeConductor.Ok("agent_pub_key_generated", Agent()),
                    "install_app" => FakeConductor.Ok("app_installed", FakeConductorFlowTests.AppInfo("x", "provisioned", "disabled")),
                    "enable_app" => FakeConductor.Ok("app_enabled", FakeConductorFlowTests.AppInfo("x")),
                    "grant_zome_call_capability" => FakeConductor.Ok("zome_call_capability_granted", null),
                    "attach_app_interface" => FakeConductor.Ok("app_interface_attached", new Dictionary<string, object> { ["port"] = 1 }), // nothing listens on 1
                    "issue_app_authentication_token" => FakeConductor.Ok("app_authentication_token_issued", new Dictionary<string, object> { ["token"] = new[] { 1 }, ["expires_at"] = null }),
                    _ => FakeConductor.Error()
                };
                string happ = Path.GetTempFileName();
                try
                {
                    var r = await admin.InstallEnableSignAttachAndConnectToHappAsync("x", happ, "oasis");
                    Assert.False(r.IsAppConnected);
                    Assert.True(r.IsError);
                }
                finally { File.Delete(happ); }
                await admin.DisconnectAsync();
            }
        }
    }
}

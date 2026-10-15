using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NextGenSoftware.Logging;
using NextGenSoftware.Logging.Interfaces;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using Xunit;
using static NextGenSoftware.Holochain.HoloNET.Client.Tests.FakeConductorAdminTests;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>Records what HoloNET logs, to check a custom provider is really used.</summary>
    public sealed class RecordingLogProvider : ILogProvider
    {
        public List<string> Messages { get; } = new List<string>();
        public void Log(string message, LogType type, bool logToFile = true, bool logToConsole = true, bool showWorkingAnimation = false, bool logOnNewLine = true, bool insertExtraNewLineAfterLogMessage = false, int? indentLogMessagesBy = 1, bool nextMessageOnSameLine = false) { lock (Messages) Messages.Add(message); }
        public void Log(string message, LogType type, bool logToFile = true, bool logToConsole = true, ConsoleColor consoleColour = ConsoleColor.White, bool showWorkingAnimation = false, bool logOnNewLine = true, bool insertExtraNewLineAfterLogMessage = false, int? indentLogMessagesBy = 1, bool nextMessageOnSameLine = false) { lock (Messages) Messages.Add(message); }
        public void Shutdown() { }
    }

    /// <summary>HoloNETDNAManager load/save and every public client constructor. Shares the live collection: the manager is static.</summary>
    [Collection(LiveConductor.Collection)]
    public class ClientSetupTests
    {
        [Fact]
        public void DnaManager_SavesAndLoads_AndReportsBadInput()
        {
            string original = HoloNETDNAManager.HoloNETDNAPath;
            var originalDna = HoloNETDNAManager.HoloNETDNA;
            string dir = Path.Combine(Path.GetTempPath(), "HoloNET-dna-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            try
            {
                string path = Path.Combine(dir, "nested", "dna.json");
                var dna = new HoloNETDNA { InstalledAppId = "saved-app", HolochainConductorAdminURI = "ws://localhost:1111", EmbeddedHolochainVersion = EmbeddedHolochainVersion.V0_7_0 };
                Assert.True(HoloNETDNAManager.SaveDNA(path, dna));
                Assert.True(File.Exists(path));

                HoloNETDNAManager.HoloNETDNA = new HoloNETDNA();
                var loaded = HoloNETDNAManager.LoadDNA(path);
                Assert.Equal("saved-app", loaded.InstalledAppId);
                Assert.Equal(EmbeddedHolochainVersion.V0_7_0, loaded.EmbeddedHolochainVersion);
                Assert.True(HoloNETDNAManager.IsLoaded);

                // The parameterless overloads use the last path.
                Assert.True(HoloNETDNAManager.SaveDNA());
                Assert.Equal("saved-app", HoloNETDNAManager.LoadDNA().InstalledAppId);

                Assert.Null(HoloNETDNAManager.LoadDNA(Path.Combine(dir, "missing.json")));
                File.WriteAllText(Path.Combine(dir, "bad.json"), "{ not json");
                Assert.Null(HoloNETDNAManager.LoadDNA(Path.Combine(dir, "bad.json")));
                Assert.Throws<ArgumentNullException>(() => HoloNETDNAManager.LoadDNA(null));
                Assert.Throws<ArgumentNullException>(() => HoloNETDNAManager.SaveDNA(null, dna));
                Assert.Throws<ArgumentNullException>(() => HoloNETDNAManager.SaveDNA(path, null));
                Assert.False(HoloNETDNAManager.SaveDNA(Path.Combine(dir, "bad\0name.json"), dna));
            }
            finally
            {
                HoloNETDNAManager.HoloNETDNAPath = original;
                HoloNETDNAManager.HoloNETDNA = originalDna;
                try { Directory.Delete(dir, true); } catch { }
            }
        }

        [Fact]
        public void Client_LoadAndSaveDna_UseTheManagerPath()
        {
            string original = HoloNETDNAManager.HoloNETDNAPath;
            var originalDna = HoloNETDNAManager.HoloNETDNA;
            string dir = Path.Combine(Path.GetTempPath(), "HoloNET-dna-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            try
            {
                HoloNETDNAManager.HoloNETDNAPath = Path.Combine(dir, "dna.json");
                var admin = new HoloNETClientAdmin(Dna("ws://localhost:1"));
                admin.HoloNETDNA.InstalledAppId = "client-saved";
                Assert.True(admin.SaveDNA());

                var other = new HoloNETClientAdmin(Dna("ws://localhost:2"));
                Assert.Equal("client-saved", other.LoadDNA().InstalledAppId);
                Assert.Equal("client-saved", other.HoloNETDNA.InstalledAppId);
                // Loading into one client must not change another (each client has its own DNA).
                Assert.NotEqual("client-saved", new HoloNETClientAdmin(Dna("ws://localhost:3")).HoloNETDNA.InstalledAppId);
            }
            finally
            {
                HoloNETDNAManager.HoloNETDNAPath = original;
                HoloNETDNAManager.HoloNETDNA = originalDna;
                try { Directory.Delete(dir, true); } catch { }
            }
        }

        public static IEnumerable<object[]> ClientConstructors() =>
            new[] { typeof(HoloNETClientAdmin), typeof(HoloNETClientAppAgent), typeof(HoloNETClientApp) }
                .SelectMany(t => t.GetConstructors().Select(c => new object[] { t.Name, string.Join(",", c.GetParameters().Select(p => p.ParameterType.Name)) }));

        /// <summary>Every public client constructor builds a working client that logs through the given provider/logger and connects.</summary>
        [Theory]
        [MemberData(nameof(ClientConstructors))]
        public async Task EveryConstructor_BuildsAClientThatConnects(string clientType, string parameterTypes)
        {
            using var fake = new FakeConductor();
            Type type = new[] { typeof(HoloNETClientAdmin), typeof(HoloNETClientAppAgent), typeof(HoloNETClientApp) }.Single(t => t.Name == clientType);
            var ctor = type.GetConstructors().Single(c => string.Join(",", c.GetParameters().Select(p => p.ParameterType.Name)) == parameterTypes);

            var provider = new RecordingLogProvider();
            var dna = Dna(fake.Uri);
            dna.HolochainConductorAppAgentURI = fake.Uri;
            object[] args = ctor.GetParameters().Select<System.Reflection.ParameterInfo, object>(p =>
            {
                if (p.ParameterType == typeof(string)) return p.Name == "installedAppId" ? "app" : null;
                if (typeof(IHoloNETDNA).IsAssignableFrom(p.ParameterType)) return dna;
                if (p.ParameterType == typeof(ILogProvider)) return provider;
                if (p.ParameterType == typeof(IEnumerable<ILogProvider>)) return new ILogProvider[] { provider };
                if (p.ParameterType == typeof(ILogger)) { var logger = new Logger(); logger.AddLogProvider(provider); return logger; }
                if (p.ParameterType == typeof(bool)) return false;
                return p.HasDefaultValue ? p.DefaultValue : null;
            }).ToArray();

            var client = (HoloNETClientBase)ctor.Invoke(args);
            Assert.Equal(fake.Uri, client.HoloNETDNA.HolochainConductorAdminURI);
            var connected = await client.ConnectAsync(fake.Uri, ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode.DoNotRetreive);
            Assert.True(connected.IsConnected, connected.Message);
            await client.DisconnectAsync();

            // The client's Logger must route to the injected provider / logger.
            client.Logger.Log("probe-message", LogType.Info);
            if (ctor.GetParameters().Any(p => p.ParameterType == typeof(ILogProvider) || p.ParameterType == typeof(IEnumerable<ILogProvider>) || p.ParameterType == typeof(ILogger)))
                lock (provider.Messages) Assert.Contains("probe-message", provider.Messages);
        }
    }
}

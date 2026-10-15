using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>Admin API against <see cref="FakeConductor"/>: every request's success decode and error path.</summary>
    public class FakeConductorAdminTests
    {
        internal static byte[] Hash(byte prefix2, byte fill = 7)
        {
            var h = Enumerable.Repeat(fill, 39).ToArray();
            h[0] = 0x84; h[1] = prefix2; h[2] = 0x24;
            return h;
        }

        internal static byte[] Agent(byte fill = 1) => Hash(0x20, fill);
        internal static byte[] Dna(byte fill = 2) => Hash(0x2d, fill);
        internal static byte[] Action(byte fill = 3) => Hash(0x29, fill);

        internal static HoloNETDNA Dna(string uri) => new HoloNETDNA
        {
            AutoStartHolochainConductor = false,
            AutoShutdownHolochainConductor = false,
            HolochainConductorAdminURI = uri,
            LogToFile = false,
            LogToConsole = false
        };

        internal static async Task<(FakeConductor, HoloNETClientAdmin)> ConnectAdmin()
        {
            var fake = new FakeConductor();
            var admin = new HoloNETClientAdmin(Dna(fake.Uri));
            var connected = await admin.ConnectAsync(fake.Uri);
            Assert.True(connected.IsConnected, connected.Message);
            return (fake, admin);
        }

        [Fact]
        public async Task GenerateAgentPubKey_DecodesTheKey_AndReportsConductorErrors()
        {
            var (fake, admin) = await ConnectAdmin();
            using (fake)
            {
                fake.Respond = (type, value) => { Assert.Equal("generate_agent_pub_key", type); return FakeConductor.Ok("agent_pub_key_generated", Agent()); };
                var ok = await admin.GenerateAgentPubKeyAsync();
                Assert.False(ok.IsError, ok.Message);
                Assert.False(string.IsNullOrEmpty(ok.AgentPubKey));

                fake.Respond = (type, value) => FakeConductor.Error();
                var failed = await admin.GenerateAgentPubKeyAsync();
                Assert.True(failed.IsError);
                Assert.Contains("fake conductor error", failed.Message);
                await admin.DisconnectAsync();
            }
        }
    }
}

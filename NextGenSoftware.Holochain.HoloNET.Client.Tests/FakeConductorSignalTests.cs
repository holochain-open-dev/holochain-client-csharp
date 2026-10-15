using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;
using Xunit;
using static NextGenSoftware.Holochain.HoloNET.Client.Tests.FakeConductorAdminTests;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>Signals in the exact Holochain 0.7.0 / 0.7.1 wire shapes (holochain_types::signal::Signal).</summary>
    public class FakeConductorSignalTests
    {
        private static async Task<SignalCallBackEventArgs> Receive(object signal, bool system = false)
        {
            using var fake = new FakeConductor();
            var app = await FakeConductorAppErrorTests.ConnectApp(fake);
            var got = new TaskCompletionSource<SignalCallBackEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (system)
            {
                app.OnSystemSignalCallBack += (s, e) => got.TrySetResult(e);
                app.OnSignalCallBack += (s, e) => got.TrySetException(new Exception("system signal raised as an app signal"));
            }
            else
                app.OnSignalCallBack += (s, e) => got.TrySetResult(e);
            await fake.SendSignalAsync(signal);
            var done = await Task.WhenAny(got.Task, Task.Delay(5000));
            Assert.True(done == got.Task, "signal not delivered");
            await app.DisconnectAsync();
            return got.Task.Result;
        }

        [Fact]
        public async Task ZomeSignal_DecodesCellZomeAndPayload()
        {
            byte[] payload = MessagePackSerializer.Serialize(new Dictionary<string, object> { ["message"] = "hi", ["n"] = 3 });
            var e = await Receive(FakeConductor.Ok("app", new Dictionary<string, object>
            {
                ["cell_id"] = new object[] { Dna(), Agent() },
                ["zome_name"] = "oasis",
                ["signal"] = payload
            }));
            Assert.False(e.IsError, e.Message);
            Assert.Equal(SignalType.App, e.SignalType);
            Assert.False(e.IsDirectSignal);
            Assert.Equal("hi", e.SignalData["message"]);
            Assert.Null(e.FromAgentPubKey);
            Assert.False(string.IsNullOrEmpty(e.DnaHash));
        }

        [Fact]
        public async Task DirectSignal_Holochain070_NoSender_IntArrayPayload()
        {
            var e = await Receive(FakeConductor.Ok("app_direct", new Dictionary<string, object>
            {
                ["cell_id"] = new object[] { Dna(), Agent() },
                ["signal"] = new[] { 1, 2, 3 }
            }));
            Assert.False(e.IsError, e.Message);
            Assert.True(e.IsDirectSignal);
            Assert.Null(e.FromAgentPubKey);
            Assert.Equal(new byte[] { 1, 2, 3 }, e.RawSignalData.Data);
        }

        [Fact]
        public async Task DirectSignal_Holochain071_WithSender_NonMsgPackPayload()
        {
            byte[] opaque = { 0xc1, 0xff, 0x00, 0x42 }; // 0xc1 is never valid msgpack
            var e = await Receive(FakeConductor.Ok("app_direct", new Dictionary<string, object>
            {
                ["cell_id"] = new object[] { Dna(), Agent() },
                ["from_agent"] = Agent(5),
                ["signal"] = opaque.Select(b => (int)b).ToArray()
            }));
            Assert.False(e.IsError, e.Message);
            Assert.True(e.IsDirectSignal);
            Assert.Equal(new HoloNETClientAppAgent("x", null, Dna("ws://x")).ConvertHoloHashToString(Agent(5)), e.FromAgentPubKey);
            Assert.Equal(opaque, e.RawSignalData.Data);
            Assert.Equal(opaque, (byte[])e.SignalData["value"]);
        }

        [Fact]
        public async Task SystemSignal_ReportsTheVariant()
        {
            var e = await Receive(FakeConductor.Ok("system", new Dictionary<string, object>
            {
                ["type"] = "successful_countersigning",
                ["value"] = Hash(0x21)
            }), system: true);
            Assert.False(e.IsError, e.Message);
            Assert.Equal(SignalType.System, e.SignalType);
            Assert.Equal("successful_countersigning", e.SystemSignalType);
        }
    }
}

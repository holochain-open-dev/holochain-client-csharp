using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using Xunit;
using static NextGenSoftware.Holochain.HoloNET.Client.Tests.FakeConductorAdminTests;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>Zome-call edge cases: unusual return payloads, Update records, preconditions, timeouts and signing.</summary>
    public class FakeConductorZomeEdgeTests
    {
        private static async Task GrantCredentials()
        {
            var (adminFake, admin) = await ConnectAdmin();
            using (adminFake)
            {
                adminFake.Respond = (t, v) => FakeConductor.Ok("zome_call_capability_granted", null);
                await admin.AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(new[] { Dna(), Agent() }, CapGrantAccessType.Unrestricted, GrantedFunctionsType.All, null);
                await admin.DisconnectAsync();
            }
        }

        private static byte[] Raw(Action<MessagePackWriter> write)
        {
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(buffer);
            write(writer);
            writer.Flush();
            return buffer.WrittenMemory.ToArray();
        }

        private static async Task<ZomeFunctionCallBackEventArgs> CallWith(object zomeCalledValue, Type entryType = null, object entryObject = null)
        {
            await GrantCredentials();
            using var fake = new FakeConductor();
            var app = await FakeConductorAppErrorTests.ConnectApp(fake);
            app.OnError += (s, e) => { };
            fake.Respond = (t, v) => FakeConductor.Ok("zome_called", zomeCalledValue);
            ZomeFunctionCallBackEventArgs r = entryObject != null
                ? await app.CallZomeFunctionAsync("oasis", "fn", new { x = 1 }, entryObject)
                : await app.CallZomeFunctionAsync("oasis", "fn", new { x = 1 }, entryType);
            await app.DisconnectAsync();
            return r;
        }

        [Fact]
        public async Task ZomeCalledValueThatIsNotExternIO_IsAnError()
        {
            var r = await CallWith(12345); // the value must be msgpack bytes (ExternIO)
            Assert.True(r.IsError, "message: " + r.Message);
        }

        [Fact]
        public async Task TimestampReturnValue_IsReportedAsUnknown()
        {
            var r = await CallWith(Raw(w => w.Write(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc))));
            Assert.True(r.IsError, "message: " + r.Message);
        }

        [Fact]
        public async Task UnknownExtensionReturnValue_IsAnError()
        {
            var r = await CallWith(Raw(w => w.WriteExtensionFormat(new ExtensionResult(42, new byte[] { 1, 2 }))));
            Assert.True(r.IsError);
        }

        [Fact]
        public async Task UpdateRecord_FillsTheOriginalAddresses_AndSkipsBadFields()
        {
            var record = FakeConductorZomeCallTests.Record(FakeConductorZomeCallTests.EntryFields());
            var content = (Dictionary<string, object>)((Dictionary<string, object>)((Dictionary<string, object>)record["signed_action"])["hashed"])["content"];
            content["type"] = "Update";
            content["original_action_address"] = Action(5);
            content["original_entry_address"] = Hash(0x21, 6);
            content["action_seq"] = "not-a-number"; // skipped, not fatal
            content[(object)7 is int ? "weird" : "x"] = new Dictionary<object, object> { [7] = "int key" }; // non-string key: skipped
            var r = await CallWith(MessagePackSerializer.Serialize(record), typeof(TestEntry));
            Assert.False(r.IsError, r.Message);
            var rec = Assert.Single(r.Records);
            Assert.StartsWith("u", rec.OriginalActionAddress);
            Assert.StartsWith("u", rec.OriginalEntryAddress);
        }

        [Fact]
        public async Task ListOfRecords_MappedOntoAGivenEntryObject()
        {
            var list = new object[] { FakeConductorZomeCallTests.Record(FakeConductorZomeCallTests.EntryFields()), "not a record" };
            var r = await CallWith(MessagePackSerializer.Serialize(list), null, new TestEntry());
            Assert.False(r.IsError, r.Message);
            Assert.Equal("Ada", ((TestEntry)Assert.Single(r.Records).EntryDataObject).Name);
        }

        [Fact]
        public async Task Preconditions_NotReady_NoDnaHash_NoAgentKey()
        {
            using var fake = new FakeConductor();
            var dna = Dna(fake.Uri);
            dna.HolochainConductorAppAgentURI = fake.Uri;
            var app = new HoloNETClientAppAgent("app", null, dna);
            app.OnError += (s, e) => { };
            Assert.True((await app.ConnectAsync(fake.Uri, ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode.DoNotRetreive)).IsConnected);

            // Not ready, callback mode: refused straight away.
            var notReady = await app.CallZomeFunctionAsync(null, "oasis", "fn", null, new { }, true, false, ConductorResponseCallBackMode.UseCallBackEvents);
            Assert.True(notReady.IsError);
            Assert.Contains("not ready", notReady.Message);
            await app.DisconnectAsync();

            var app2 = await FakeConductorAppErrorTests.ConnectApp(fake);
            app2.OnError += (s, e) => { };
            app2.HoloNETDNA.DnaHash = null;
            Assert.Contains("DnaHash cannot be empty", (await app2.CallZomeFunctionAsync("oasis", "fn", new { })).Message);
            app2.HoloNETDNA.DnaHash = app2.ConvertHoloHashToString(Dna());
            app2.HoloNETDNA.AgentPubKey = null;
            Assert.Contains("AgentPubKey cannot be empty", (await app2.CallZomeFunctionAsync("oasis", "fn", new { })).Message);
            await app2.DisconnectAsync();
        }

        [Fact]
        public async Task NoSigningCredentials_IsAnError()
        {
            using var fake = new FakeConductor();
            var app = await FakeConductorAppErrorTests.ConnectApp(fake);
            app.OnError += (s, e) => { };
            app.HoloNETDNA.AgentPubKey = app.ConvertHoloHashToString(Agent(201)); // a cell nobody authorised
            var r = await app.CallZomeFunctionAsync("oasis", "fn", new { });
            Assert.True(r.IsError);
            Assert.Contains("no signing credentials", r.Message);
            await app.DisconnectAsync();
        }

        [Fact]
        public async Task Timeout_UseCallBackEvents_HashParam_Callback_AndReconnect()
        {
            await GrantCredentials();
            using var fake = new FakeConductor();
            var app = await FakeConductorAppErrorTests.ConnectApp(fake);
            app.OnError += (s, e) => { };

            fake.Respond = (t, v) => null; // never answers
            var timedOut = await app.CallZomeFunctionAsync(null, "oasis", "fn", null, new { }, true, false, ConductorResponseCallBackMode.WaitForHolochainConductorResponse, new CallZomeOptions { TimeoutSeconds = 1 });
            Assert.True(timedOut.IsError);
            Assert.Contains("timed out", timedOut.Message);

            fake.Respond = (t, v) => FakeConductor.Ok("zome_called", MessagePackSerializer.Serialize(true));
            var fireAndForget = await app.CallZomeFunctionAsync(null, "oasis", "fn", null, new { }, true, false, ConductorResponseCallBackMode.UseCallBackEvents);
            Assert.False(fireAndForget.IsError);
            Assert.Contains("UseCallBackEvents", fireAndForget.Message);

            // A HoloHash string parameter is sent as bytes; any other string (even one that decodes as base64) as-is.
            var sentPayloads = new List<string>();
            fake.Respond = (t, v) => { sentPayloads.Add(MessagePack.MessagePackSerializer.ConvertToJson(v)); return FakeConductor.Ok("zome_called", MessagePackSerializer.Serialize(true)); };
            Assert.False((await app.CallZomeFunctionAsync("oasis", "fn", app.ConvertHoloHashToString(Action()))).IsError);
            Assert.False((await app.CallZomeFunctionAsync("oasis", "fn", "user12345678")).IsError);
            Assert.False((await app.CallZomeFunctionAsync("oasis", "fn", "plain text")).IsError);
            Assert.True(sentPayloads.Count >= 3);

            // Cached result delivered to a callback.
            ZomeFunctionCallBackEventArgs fromCallback = null;
            await app.CallZomeFunctionAsync("424242", "oasis", "fn", (s, e) => { }, new { }, true, true, ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
            await app.CallZomeFunctionAsync("424242", "oasis", "fn", (s, e) => fromCallback = e, new { }, true, true, ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
            Assert.NotNull(fromCallback);

            // After a disconnect the next zome call reconnects by itself.
            await app.DisconnectAsync();
            Assert.False((await app.CallZomeFunctionAsync("oasis", "fn", new { })).IsError);
            await app.DisconnectAsync();
        }
    }
}

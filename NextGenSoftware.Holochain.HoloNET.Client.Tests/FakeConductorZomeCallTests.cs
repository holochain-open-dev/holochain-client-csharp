using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using Xunit;
using static NextGenSoftware.Holochain.HoloNET.Client.Tests.FakeConductorAdminTests;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    public enum TestColour { Red, Green }

    /// <summary>An entry with one property of every type MapEntryDataObject converts.</summary>
    public class TestEntry
    {
        [HolochainRustFieldName("name")] public string Name { get; set; }
        [HolochainRustFieldName("age")] public int Age { get; set; }
        [HolochainRustFieldName("height")] public float Height { get; set; }
        [HolochainRustFieldName("weight")] public double Weight { get; set; }
        [HolochainRustFieldName("balance")] public decimal Balance { get; set; }
        [HolochainRustFieldName("active")] public bool Active { get; set; }
        [HolochainRustFieldName("id")] public Guid Id { get; set; }
        [HolochainRustFieldName("born")] public DateTime Born { get; set; }
        [HolochainRustFieldName("colour")] public TestColour Colour { get; set; }
        [HolochainRustFieldName("score")] public int? Score { get; set; }
        [HolochainRustFieldName("missing")] public int? Missing { get; set; }
        [HolochainRustFieldName("big")] public long Big { get; set; }
        [HolochainRustFieldName("count")] public uint Count { get; set; }
        [HolochainRustFieldName("initial")] public char Initial { get; set; }
        [HolochainRustFieldName("flags")] public byte Flags { get; set; }
        public string NotMapped { get; set; }
    }

    /// <summary>Zome calls through <see cref="FakeConductor"/>: every Holochain 0.7 return shape and every overload.</summary>
    public class FakeConductorZomeCallTests
    {
        private static readonly Guid TheId = Guid.Parse("6f1c7e2a-1111-4222-8333-944455556666");

        internal static Dictionary<string, object> EntryFields() => new Dictionary<string, object>
        {
            ["name"] = "Ada", ["age"] = 36, ["height"] = 1.5, ["weight"] = 60.25, ["balance"] = "12.5", ["active"] = true,
            ["id"] = TheId.ToString(), ["born"] = "1815-12-10T00:00:00", ["colour"] = "Green", ["score"] = 7, ["big"] = 9000000000L,
            ["count"] = 3, ["initial"] = "A", ["flags"] = 5
        };

        /// <summary>A Holochain 0.7 Record: { signed_action: { hashed: { content, hash }, signature }, entry: { Present: Entry::App } }.</summary>
        internal static Dictionary<string, object> Record(Dictionary<string, object> fields, byte fill = 3) => new Dictionary<string, object>
        {
            ["signed_action"] = new Dictionary<string, object>
            {
                ["hashed"] = new Dictionary<string, object>
                {
                    ["content"] = new Dictionary<string, object>
                    {
                        ["type"] = "Create", ["author"] = Agent(), ["timestamp"] = 1792029232652253L, ["action_seq"] = 5,
                        ["prev_action"] = Action(9), ["entry_type"] = new Dictionary<string, object> { ["App"] = new Dictionary<string, object> { ["entry_index"] = 0, ["zome_index"] = 0, ["visibility"] = "Public" } },
                        ["entry_hash"] = Hash(0x21), ["weight"] = new Dictionary<string, object> { ["bucket_id"] = 0, ["units"] = 0, ["rate_bytes"] = 0 }
                    },
                    ["hash"] = Action(fill)
                },
                ["signature"] = new byte[64]
            },
            ["entry"] = new Dictionary<string, object> { ["Present"] = new Dictionary<string, object> { ["entry_type"] = "App", ["entry"] = MessagePackSerializer.Serialize(fields) } }
        };

        /// <summary>A connected app plus signing credentials for its cell (granted through the fake admin interface).</summary>
        internal static async Task<(FakeConductor, HoloNETClientAppAgent)> Ready()
        {
            var (adminFake, admin) = await ConnectAdmin();
            using (adminFake)
            {
                adminFake.Respond = (t, v) => FakeConductor.Ok("zome_call_capability_granted", null);
                var granted = await admin.AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(new[] { Dna(), Agent() }, CapGrantAccessType.Unrestricted, GrantedFunctionsType.All, null);
                Assert.False(granted.IsError, granted.Message);
                await admin.DisconnectAsync();
            }
            var fake = new FakeConductor();
            return (fake, await FakeConductorAppErrorTests.ConnectApp(fake));
        }

        private static async Task<ZomeFunctionCallBackEventArgs> Call(object returnValue, Type entryType = null)
        {
            var (fake, app) = await Ready();
            using (fake)
            {
                fake.Respond = (t, v) => { Assert.Equal("call_zome", t); return FakeConductor.Ok("zome_called", MessagePackSerializer.Serialize(returnValue)); };
                var r = await app.CallZomeFunctionAsync("oasis", "fn", new { x = 1 }, entryType);
                await app.DisconnectAsync();
                return r;
            }
        }

        [Fact]
        public async Task SingleRecord_DecodesRecordFields_AndMapsEveryPropertyType()
        {
            var r = await Call(Record(EntryFields()), typeof(TestEntry));
            Assert.False(r.IsError, r.Message);
            var record = Assert.Single(r.Records);
            Assert.Equal(5, record.ActionSequence);
            Assert.Equal(2026, record.DateTime.Year);
            Assert.Equal(DateTimeKind.Utc, record.DateTime.Kind);
            var e = Assert.IsType<TestEntry>(record.EntryDataObject);
            Assert.Equal("Ada", e.Name);
            Assert.Equal(36, e.Age);
            Assert.Equal(1.5f, e.Height);
            Assert.Equal(60.25, e.Weight);
            Assert.Equal(12.5m, e.Balance);
            Assert.True(e.Active);
            Assert.Equal(TheId, e.Id);
            Assert.Equal(new DateTime(1815, 12, 10), e.Born);
            Assert.Equal(TestColour.Green, e.Colour);
            Assert.Equal(7, e.Score);
            Assert.Null(e.Missing);
            Assert.Equal(9000000000L, e.Big);
            Assert.Equal(3u, e.Count);
            Assert.Equal('A', e.Initial);
            Assert.Equal((byte)5, e.Flags);
        }

        [Fact]
        public async Task SingleRecord_NumbersRoundTripOnANonEnglishCulture()
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
            try
            {
                var r = await Call(Record(EntryFields()), typeof(TestEntry));
                var e = Assert.IsType<TestEntry>(Assert.Single(r.Records).EntryDataObject);
                Assert.Equal(60.25, e.Weight);
                Assert.Equal(1.5f, e.Height);
            }
            finally { System.Globalization.CultureInfo.CurrentCulture = culture; }
        }

        [Fact]
        public async Task ListOfRecords_EachMapped()
        {
            var r = await Call(new object[] { Record(EntryFields(), 3), Record(EntryFields(), 4) }, typeof(TestEntry));
            Assert.False(r.IsError, r.Message);
            Assert.Equal(2, r.Records.Count);
            Assert.All(r.Records, rec => Assert.Equal("Ada", ((TestEntry)rec.EntryDataObject).Name));
        }

        [Fact]
        public async Task ActionHash_IsReturnedAsHash()
        {
            var r = await Call(Action());
            Assert.False(r.IsError, r.Message);
            Assert.StartsWith("u", r.ZomeReturnHash);
        }

        [Theory]
        [InlineData("unit")]
        [InlineData("bool")]
        [InlineData("number")]
        [InlineData("string")]
        public async Task Scalars_AreReturnedNotErrors(string kind)
        {
            object value = kind switch { "unit" => null, "bool" => true, "number" => 42, _ => "hello" };
            var r = await Call(value);
            Assert.False(r.IsError, r.Message);
            if (value == null) Assert.Null(r.ZomeReturnValue);
            else Assert.Equal(Convert.ToString(value), Convert.ToString(r.ZomeReturnValue));
        }

        [Fact]
        public async Task ListOfHashes_IsAvailableAsTheReturnValue()
        {
            var r = await Call(new object[] { Action(3), Action(4) });
            Assert.False(r.IsError, r.Message);
            Assert.Equal(2, ((object[])r.ZomeReturnValue).Length);
        }

        [Fact]
        public async Task ConductorError_OnZomeCall_IsReported()
        {
            var (fake, app) = await Ready();
            using (fake)
            {
                fake.Respond = (t, v) => FakeConductor.Error("ribosome_error", "zome panicked");
                var r = await app.CallZomeFunctionAsync("oasis", "fn", new { x = 1 });
                Assert.True(r.IsError);
                Assert.Contains("zome panicked", r.Message);
                await app.DisconnectAsync();
            }
        }

        /// <summary>
        /// Every public CallZomeFunction / CallZomeFunctionAsync overload, with plausible arguments, must reach the
        /// conductor and decode the record (sync overloads now wait for the result).
        /// </summary>
        public static IEnumerable<object[]> Overloads() =>
            typeof(HoloNETClientAppBase).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == "CallZomeFunction" || m.Name == "CallZomeFunctionAsync")
                .Select(m => new object[] { m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")" });

        [Theory]
        [MemberData(nameof(Overloads))]
        public async Task EveryOverload_CallsAndDecodes(string signature)
        {
            var method = typeof(HoloNETClientAppBase).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(m => (m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")") == signature);
            var (fake, app) = await Ready();
            using (fake)
            {
                fake.Respond = (t, v) => FakeConductor.Ok("zome_called", MessagePackSerializer.Serialize(Record(EntryFields())));
                ZomeFunctionCallBackEventArgs fromCallback = null;
                var args = method.GetParameters().Select(p => Argument(p, app, e => fromCallback = e)).ToArray();

                object returned = method.Invoke(app, args);
                ZomeFunctionCallBackEventArgs result = returned is Task<ZomeFunctionCallBackEventArgs> task ? await task : (ZomeFunctionCallBackEventArgs)returned;

                Assert.False(result.IsError, signature + ": " + result.Message);
                Assert.Equal("Ada", Assert.Single(result.Records).EntryKeyValuePairs["name"]);
                if (method.GetParameters().Any(p => p.ParameterType == typeof(HoloNETClientAppBase.ZomeFunctionCallBack)))
                {
                    for (int i = 0; i < 50 && fromCallback == null; i++) await Task.Delay(20);
                    Assert.NotNull(fromCallback);
                }
                await app.DisconnectAsync();
            }
        }

        private static object Argument(ParameterInfo p, HoloNETClientAppAgent app, Action<ZomeFunctionCallBackEventArgs> onCallback)
        {
            switch (p.Name)
            {
                case "id": return null;
                case "zome": return "oasis";
                case "function": return "fn";
                case "paramsObject": return new { x = 1 };
                case "callback": return new HoloNETClientAppBase.ZomeFunctionCallBack((s, e) => onCallback(e));
                case "entryDataObjectTypeReturnedFromZome": return typeof(TestEntry);
                case "entryDataObjectReturnedFromZome": return new TestEntry();
                case "zomeResultCallBackMode": return ConductorResponseCallBackMode.WaitForHolochainConductorResponse;
            }
            if (p.ParameterType == typeof(bool)) return p.HasDefaultValue ? p.DefaultValue : true;
            return p.HasDefaultValue ? p.DefaultValue : null;
        }
    }
}

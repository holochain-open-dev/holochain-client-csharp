using System;
using System.Linq;
using MessagePack;
using MessagePack.Resolvers;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>
    /// Group (c): round-trip tests for HoloNET's config classes (NetworkConfig, Kitsune2Config,
    /// KeystoreConfig in its 3 enum variants), plus default-value lock-in tests for the
    /// intentionally-inert QUICConfig/WASMConfig placeholders.
    ///
    /// NOTE: NetworkConfig/Kitsune2Config/KeystoreConfig are plain POCOs - they are NOT decorated
    /// with [MessagePackObject]/[Key] like the wire-protocol request/response types, and are NOT
    /// currently sent directly over the websocket wire by HoloNET (RequestTimeoutS etc. are
    /// consumed client-side only). The production code's
    /// MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData)
    /// (StandardResolver) therefore CANNOT serialize these types at all - it throws
    /// FormatterNotRegisteredException for any type lacking a [MessagePackObject] contract or a
    /// built-in formatter, confirmed by running this against the real options. Since these classes
    /// are not part of the actual wire contract, these tests instead use
    /// ContractlessStandardResolver (MessagePack's reflection-based fallback for plain POCOs) so
    /// the fields can still be meaningfully round-tripped and the documented defaults locked in.
    /// </summary>
    public class ConfigSerializationTests
    {
        // NOT the production wire-path options (those cannot serialize plain, non-contract POCOs
        // like these config classes at all - see class-level NOTE above). Used here only so these
        // config POCOs can be round-tripped for testing purposes.
        private static readonly MessagePackSerializerOptions Options =
            ContractlessStandardResolver.Options.WithSecurity(MessagePackSecurity.UntrustedData);

        [Fact]
        public void NetworkConfig_RoundTrips()
        {
            var original = new NetworkConfig { RequestTimeoutS = 120 };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<NetworkConfig>(bytes, Options);

            Assert.Equal(original.RequestTimeoutS, result.RequestTimeoutS);
        }

        [Fact]
        public void NetworkConfig_DefaultValue_Is60Seconds()
        {
            var config = new NetworkConfig();
            Assert.Equal(60, config.RequestTimeoutS);
        }

        [Fact]
        public void Kitsune2Config_RoundTrips_AllFields()
        {
            // SignalUrl and WebrtcConfigJson were removed in Holochain 0.7.0 (tx5/WebRTC dropped).
            var original = new Kitsune2Config
            {
                BootstrapUrl = "https://bootstrap.example.org",
                RelayUrl = "https://relay.example.org",
                TargetArcFactor = 0,
                AdvancedJson = "{\"tuning\":true}"
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<Kitsune2Config>(bytes, Options);

            Assert.Equal(original.BootstrapUrl, result.BootstrapUrl);
            Assert.Equal(original.RelayUrl, result.RelayUrl);
            Assert.Equal(original.TargetArcFactor, result.TargetArcFactor);
            Assert.Equal(original.AdvancedJson, result.AdvancedJson);
        }

        [Fact]
        public void Kitsune2Config_Defaults_MatchDocumentedValues()
        {
            var config = new Kitsune2Config();

            Assert.Equal("https://dev-test-bootstrap2.holochain.org", config.BootstrapUrl);
            Assert.Equal("https://use1-1.relay.n0.iroh-canary.iroh.link./", config.RelayUrl);
            Assert.Equal(1u, config.TargetArcFactor);
            Assert.Null(config.AdvancedJson);
        }

        [Fact]
        public void KeystoreConfig_RoundTrips_DangerTestKeystoreVariant()
        {
            var original = new KeystoreConfig
            {
                Type = KeystoreConfigType.DangerTestKeystore,
                ConnectionUrl = "",
                LairRoot = null
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<KeystoreConfig>(bytes, Options);

            Assert.Equal(KeystoreConfigType.DangerTestKeystore, result.Type);
            Assert.Equal(original.ConnectionUrl, result.ConnectionUrl);
            Assert.Null(result.LairRoot);
        }

        [Fact]
        public void KeystoreConfig_RoundTrips_LairServerVariant()
        {
            var original = new KeystoreConfig
            {
                Type = KeystoreConfigType.LairServer,
                ConnectionUrl = "unix:///path/to/lair/socket?k=abc123",
                LairRoot = null
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<KeystoreConfig>(bytes, Options);

            Assert.Equal(KeystoreConfigType.LairServer, result.Type);
            Assert.Equal(original.ConnectionUrl, result.ConnectionUrl);
        }

        [Fact]
        public void KeystoreConfig_RoundTrips_LairServerInProcVariant()
        {
            var original = new KeystoreConfig
            {
                Type = KeystoreConfigType.LairServerInProc,
                ConnectionUrl = "",
                LairRoot = "/home/user/.config/holochain/ks"
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<KeystoreConfig>(bytes, Options);

            Assert.Equal(KeystoreConfigType.LairServerInProc, result.Type);
            Assert.Equal(original.LairRoot, result.LairRoot);
        }

        [Fact]
        public void KeystoreConfig_DefaultType_IsLairServerInProc()
        {
            var config = new KeystoreConfig();
            Assert.Equal(KeystoreConfigType.LairServerInProc, config.Type);
        }

        [Fact]
        public void QUICConfig_DefaultEnabled_IsFalse()
        {
            // QUICConfig is an intentionally-inert placeholder — this test locks in the default.
            Assert.False(new QUICConfig().Enabled);
        }

        [Fact]
        public void WASMConfig_DefaultEnabled_IsFalse()
        {
            // WASMConfig is an intentionally-inert placeholder — this test locks in the default.
            Assert.False(new WASMConfig().Enabled);
        }

        // ── DnaStorageInfo (Holochain 0.7.0) ────────────────────────────────────────────────────

        [Fact]
        public void DnaStorageInfo_RoundTrips_RemainingFields()
        {
            // authored_data_size / cache_data_size were removed in 0.7.0.
            // Verify the remaining fields survive a MessagePack round-trip.
            var original = new DnaStorageInfo
            {
                dht_data_size = 1_234_567,
                dht_data_size_on_disk = 2_345_678,
                used_by = "test-app"
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, MessagePackSerializerOptions.Standard);
            var result = MessagePackSerializer.Deserialize<DnaStorageInfo>(bytes, MessagePackSerializerOptions.Standard);

            Assert.Equal(original.dht_data_size, result.dht_data_size);
            Assert.Equal(original.dht_data_size_on_disk, result.dht_data_size_on_disk);
            Assert.Equal(original.used_by, result.used_by);
        }

        [Fact]
        public void DnaStorageInfo_DoesNotHaveRemovedFields()
        {
            // Compile-time guard: ensure the 0.7.0-removed fields are gone from the type.
            var type = typeof(DnaStorageInfo);
            Assert.Null(type.GetProperty("authored_data_size"));
            Assert.Null(type.GetProperty("authored_data_size_on_disk"));
            Assert.Null(type.GetProperty("cache_data_size"));
            Assert.Null(type.GetProperty("cache_data_size_on_disk"));
        }

        // ── AppStatusFilter / AwaitingMemproofs (Holochain 0.7.0) ───────────────────────────────

        [Fact]
        public void AppStatusFilter_ContainsAwaitingMemproofs()
        {
            // Lock in the new enum variant added in 0.7.0.
            Assert.True(Enum.IsDefined(typeof(AppStatusFilter), "AwaitingMemproofs"));
        }

        [Fact]
        public void AppStatusFilter_AwaitingMemproofs_HasDistinctValue()
        {
            // Ensure no accidental duplicate ordinal.
            var values = Enum.GetValues(typeof(AppStatusFilter)).Cast<AppStatusFilter>().ToList();
            var distinct = values.Distinct().ToList();
            Assert.Equal(values.Count, distinct.Count);
        }

        [Theory]
        [InlineData(AppStatusFilter.Enabled, "enabled")]
        [InlineData(AppStatusFilter.Disabled, "disabled")]
        [InlineData(AppStatusFilter.AwaitingMemproofs, "awaiting_memproofs")]
        [InlineData(AppStatusFilter.AwaitingRestore, "awaiting_restore")]
        [InlineData(AppStatusFilter.Unrecoverable, "unrecoverable")]
        [InlineData(AppStatusFilter.All, null)]
        public void AppStatusFilter_SentAsSnakeCaseString(AppStatusFilter filter, string expected)
        {
            Assert.Equal(expected, filter.ToWireValue());
        }

        [Fact]
        public void AppInfoStatusEnum_ContainsAwaitingMemproofs()
        {
            Assert.True(Enum.IsDefined(typeof(AppInfoStatusEnum), "AwaitingMemproofs"));
        }

        // ── DumpOpTimingsRequest (Holochain 0.7.0) ──────────────────────────────────────────────

        [Fact]
        public void DumpOpTimingsRequest_RoundTrips_WithoutCursor()
        {
            var original = new DumpOpTimingsRequest
            {
                dna_hash = new byte[] { 0x01, 0x02, 0x03 },
                cursor = null,
                limit = 100u
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, MessagePackSerializerOptions.Standard);
            var result = MessagePackSerializer.Deserialize<DumpOpTimingsRequest>(bytes, MessagePackSerializerOptions.Standard);

            Assert.Equal(original.dna_hash, result.dna_hash);
            Assert.Null(result.cursor);
            Assert.Equal(100u, result.limit);
        }

        [Fact]
        public void DumpOpTimingsRequest_RoundTrips_WithCursor()
        {
            var original = new DumpOpTimingsRequest
            {
                dna_hash = new byte[] { 0xAB },
                cursor = new OpTimingsCursor { when_received = 999L, hash = new byte[] { 0xFF } },
                limit = null
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, MessagePackSerializerOptions.Standard);
            var result = MessagePackSerializer.Deserialize<DumpOpTimingsRequest>(bytes, MessagePackSerializerOptions.Standard);

            Assert.NotNull(result.cursor);
            Assert.Equal(999L, result.cursor.when_received);
            Assert.Null(result.limit);
        }

        // ── IntegrityManifest / CoordinatorManifest / DnaManifest (Holochain 0.7.0) ───────────

        // These assert the exact wire keys: DnaManifestV0, IntegrityManifest, CoordinatorManifest
        // and ZomeManifest are all deny_unknown_fields in Holochain 0.7.0.
        private static string Json<T>(T value) =>
            MessagePackSerializer.ConvertToJson(MessagePackSerializer.Serialize(value, MessagePackSerializerOptions.Standard));

        [Fact]
        public void ZomeManifest_WireKeys_MatchHolochain070()
        {
            var zome = new NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects.ZomeManifest
            {
                name = "z", path = "z.wasm",
                dependencies = new[] { new NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects.ZomeDependency { name = "i" } }
            };

            Assert.Equal("{\"name\":\"z\",\"hash\":null,\"path\":\"z.wasm\",\"dependencies\":[{\"name\":\"i\"}]}", Json(zome));
        }

        [Fact]
        public void DnaManifest_WireKeys_MatchHolochain070()
        {
            var manifest = new NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects.DnaManifest
            {
                name = "d",
                integrity = new NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects.IntegrityManifest { network_seed = "s", zomes = new NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects.ZomeManifest[0] },
                coordinator = new NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects.CoordinatorManifest { zomes = new NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects.ZomeManifest[0] }
            };

            Assert.Equal(
                "{\"manifest_version\":\"0\",\"name\":\"d\",\"integrity\":{\"network_seed\":\"s\",\"properties\":null,\"zomes\":[]},\"coordinator\":{\"zomes\":[]}}",
                Json(manifest));
        }

        [Fact]
        public void GrantedFunctions_UseAdjacentTagging()
        {
            Assert.Equal("{\"type\":\"all\"}", Json(NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects.GrantedFunctions.All().Functions));
            Assert.Equal(
                "{\"type\":\"listed\",\"value\":[[\"zome\",\"fn\"]]}",
                Json(NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects.GrantedFunctions.Listed(new System.Collections.Generic.List<(string, string)> { ("zome", "fn") }).Functions));
        }

        [Fact]
        public void CapAccess_UsesAdjacentTagging()
        {
            Assert.Equal("{\"type\":\"unrestricted\"}", Json(NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects.CapAccess.Unrestricted()));
            Assert.StartsWith("{\"type\":\"transferable\",\"value\":{\"secret\":", Json(NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects.CapAccess.Transferable(new byte[] { 1 })));
            Assert.StartsWith("{\"type\":\"assigned\",\"value\":{\"secret\":", Json(NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects.CapAccess.Assigned(new byte[] { 1 }, new[] { new byte[] { 2 } })));
        }

        [Fact]
        public void CellInfo_DecodesAdjacentlyTaggedConductorShape()
        {
            byte[] wire = MessagePackSerializer.ConvertFromJson(
                "[{\"type\":\"provisioned\",\"value\":{\"cell_id\":null,\"dna_modifiers\":null,\"name\":\"main\"}}," +
                " {\"type\":\"stem\",\"value\":{\"original_dna_hash\":null,\"dna_modifiers\":null,\"name\":null}}]");

            var cells = MessagePackSerializer.Deserialize<System.Collections.Generic.List<NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects.CellInfo>>(
                wire, MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData));

            Assert.Equal(CellInfoType.Provisioned, cells[0].CellInfoType);
            Assert.Equal("main", cells[0].Provisioned.name);
            Assert.Equal(CellInfoType.Stem, cells[1].CellInfoType);
        }

        [Fact]
        public void CloneCellId_Normalize_ProducesTaggedForms()
        {
            Assert.Equal("{\"type\":\"clone_id\",\"value\":\"role.0\"}", Json(CloneCellId.Normalize("role.0")));
            Assert.StartsWith("{\"type\":\"dna_hash\",\"value\":", Json(CloneCellId.Normalize(new byte[] { 9 })));

            var fromCellId = (System.Collections.Generic.Dictionary<string, object>)CloneCellId.Normalize(new[] { new byte[] { 7 }, new byte[] { 8 } });
            Assert.Equal("dna_hash", fromCellId["type"]);
            Assert.Equal(new byte[] { 7 }, fromCellId["value"]);
        }

        // ── OpTimingsDump / OpTimingDump / OpTimingsCursor (Holochain 0.7.0) ───────────────────

        [Fact]
        public void OpTimingsCursor_RoundTrips()
        {
            var original = new OpTimingsCursor
            {
                when_received = 1_700_000_000_000_000L,
                hash = new byte[] { 0x01, 0x02, 0x03 }
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, MessagePackSerializerOptions.Standard);
            var result = MessagePackSerializer.Deserialize<OpTimingsCursor>(bytes, MessagePackSerializerOptions.Standard);

            Assert.Equal(original.when_received, result.when_received);
            Assert.Equal(original.hash, result.hash);
        }

        [Fact]
        public void OpTimingDump_RoundTrips()
        {
            var original = new OpTimingDump
            {
                op_hash = new byte[] { 0xAB, 0xCD },
                when_received = 1_700_000_000_000_000L,
                when_integrated = 1_700_000_001_000_000L,
                abandoned_at = null,
                validation_status = "Valid",
                locally_validated = true
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, MessagePackSerializerOptions.Standard);
            var result = MessagePackSerializer.Deserialize<OpTimingDump>(bytes, MessagePackSerializerOptions.Standard);

            Assert.Equal(original.op_hash, result.op_hash);
            Assert.Equal(original.when_received, result.when_received);
            Assert.Equal(original.when_integrated, result.when_integrated);
            Assert.Null(result.abandoned_at);
            Assert.Equal("Valid", result.validation_status);
            Assert.True(result.locally_validated);
        }

        [Fact]
        public void OpTimingsDump_RoundTrips_WithCursor()
        {
            var original = new OpTimingsDump
            {
                timings = new System.Collections.Generic.List<OpTimingDump>
                {
                    new OpTimingDump { op_hash = new byte[] { 1 }, when_received = 100L }
                },
                cursor = new OpTimingsCursor { when_received = 100L, hash = new byte[] { 1 } }
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, MessagePackSerializerOptions.Standard);
            var result = MessagePackSerializer.Deserialize<OpTimingsDump>(bytes, MessagePackSerializerOptions.Standard);

            Assert.Single(result.timings);
            Assert.Equal(100L, result.timings[0].when_received);
            Assert.NotNull(result.cursor);
            Assert.Equal(100L, result.cursor.when_received);
        }

        // ── CapAccessInfo / DesensitizedZomeCallCapGrant (Holochain 0.7.0) ──────────────────────

        [Fact]
        public void CapAccessInfo_RoundTrips_Unrestricted()
        {
            var original = new CapAccessInfo { access_type = "Unrestricted", assignees = null };

            byte[] bytes = MessagePackSerializer.Serialize(original, MessagePackSerializerOptions.Standard);
            var result = MessagePackSerializer.Deserialize<CapAccessInfo>(bytes, MessagePackSerializerOptions.Standard);

            Assert.Equal("Unrestricted", result.access_type);
            Assert.Null(result.assignees);
        }

        [Fact]
        public void CapAccessInfo_RoundTrips_Assigned()
        {
            var agent = new byte[] { 1, 2, 3, 4 };
            var original = new CapAccessInfo
            {
                access_type = "Assigned",
                assignees = new[] { agent }
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, MessagePackSerializerOptions.Standard);
            var result = MessagePackSerializer.Deserialize<CapAccessInfo>(bytes, MessagePackSerializerOptions.Standard);

            Assert.Equal("Assigned", result.access_type);
            Assert.Single(result.assignees);
            Assert.Equal(agent, result.assignees[0]);
        }

        [Fact]
        public void DesensitizedZomeCallCapGrant_RoundTrips()
        {
            var original = new DesensitizedZomeCallCapGrant
            {
                tag = "my-grant",
                access = new CapAccessInfo { access_type = "Transferable", assignees = null },
                functions = null
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, MessagePackSerializerOptions.Standard);
            var result = MessagePackSerializer.Deserialize<DesensitizedZomeCallCapGrant>(bytes, MessagePackSerializerOptions.Standard);

            Assert.Equal("my-grant", result.tag);
            Assert.Equal("Transferable", result.access.access_type);
        }
    }
}

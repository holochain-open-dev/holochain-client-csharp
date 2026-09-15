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
            // QUICConfig is an intentionally-inert placeholder with no real Holochain wire
            // equivalent as of 0.6.1 (see QUICConfig.cs comments) - this test exists purely to
            // lock in the documented default-disabled behaviour.
            Assert.False(new QUICConfig().Enabled);
        }

        [Fact]
        public void WASMConfig_DefaultEnabled_IsFalse()
        {
            // WASMConfig is an intentionally-inert placeholder with no real Holochain wire
            // equivalent as of 0.6.1 (see WASMConfig.cs comments) - this test exists purely to
            // lock in the documented default-disabled behaviour.
            Assert.False(new WASMConfig().Enabled);
        }
    }
}

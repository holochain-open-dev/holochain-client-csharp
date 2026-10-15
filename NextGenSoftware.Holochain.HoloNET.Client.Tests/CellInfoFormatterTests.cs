using System;
using System.Collections.Generic;
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using NextGenSoftware.Holochain.HoloNET.Client.Data.App.Responses.Objects;
using Xunit;
using static NextGenSoftware.Holochain.HoloNET.Client.Tests.FakeConductorAdminTests;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>holochain_conductor_api::CellInfo ({"type": provisioned | cloned | stem, "value": ...}) in both directions.</summary>
    public class CellInfoFormatterTests
    {
        private static readonly MessagePackSerializerOptions Options = FakeConductor.Options;
        private static Dictionary<string, object> Modifiers => new Dictionary<string, object> { ["network_seed"] = "seed", ["properties"] = new byte[] { 0xc0 } };

        private static CellInfo FromWire(string type, object value) =>
            MessagePackSerializer.Deserialize<CellInfo>(MessagePackSerializer.Serialize(new Dictionary<string, object> { ["value"] = value, ["type"] = type }), Options);

        [Fact]
        public void Provisioned_DecodesAndRoundTrips()
        {
            var info = FromWire("provisioned", new Dictionary<string, object> { ["cell_id"] = new object[] { Dna(), Agent() }, ["dna_modifiers"] = Modifiers, ["name"] = "oasis" });
            Assert.Equal(CellInfoType.Provisioned, info.CellInfoType);
            Assert.Equal("oasis", info.Provisioned.name);
            var back = MessagePackSerializer.Deserialize<CellInfo>(MessagePackSerializer.Serialize(info, Options), Options);
            Assert.Equal(CellInfoType.Provisioned, back.CellInfoType);
            Assert.Equal(info.Provisioned.cell_id[0], back.Provisioned.cell_id[0]);
        }

        [Fact]
        public void Cloned_CloneIdIsAString()
        {
            var info = FromWire("cloned", new Dictionary<string, object>
            {
                ["cell_id"] = new object[] { Dna(), Agent() }, ["clone_id"] = "oasis.0", ["original_dna_hash"] = Dna(9),
                ["dna_modifiers"] = Modifiers, ["name"] = "c", ["enabled"] = true
            });
            Assert.Equal(CellInfoType.Cloned, info.CellInfoType);
            Assert.Equal("oasis.0", info.Cloned.clone_id);
            var back = MessagePackSerializer.Deserialize<CellInfo>(MessagePackSerializer.Serialize(info, Options), Options);
            Assert.Equal("oasis.0", back.Cloned.clone_id);
            Assert.True(back.Cloned.enabled);
        }

        [Fact]
        public void Stem_DecodesAndRoundTrips()
        {
            var info = FromWire("stem", new Dictionary<string, object> { ["original_dna_hash"] = Dna(), ["dna_modifiers"] = Modifiers, ["name"] = null });
            Assert.Equal(CellInfoType.Stem, info.CellInfoType);
            var back = MessagePackSerializer.Deserialize<CellInfo>(MessagePackSerializer.Serialize(info, Options), Options);
            Assert.Equal(CellInfoType.Stem, back.CellInfoType);
        }

        [Fact]
        public void NilUnknownAndEmpty()
        {
            Assert.Null(MessagePackSerializer.Deserialize<CellInfo>(MessagePackSerializer.Serialize<CellInfo>(null, Options), Options));
            Assert.Equal(CellInfoType.None, FromWire("future_variant", new Dictionary<string, object>()).CellInfoType);
            Assert.Equal(CellInfoType.None, MessagePackSerializer.Deserialize<CellInfo>(MessagePackSerializer.Serialize(new Dictionary<string, object> { ["type"] = "provisioned", ["extra"] = 1 }), Options).CellInfoType);
            var ex = Assert.Throws<MessagePackSerializationException>(() => MessagePackSerializer.Serialize(new CellInfo(), Options));
            Assert.IsType<InvalidOperationException>(ex.InnerException);
        }
    }
}

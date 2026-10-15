using System.Collections.Generic;
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>
    /// Decodes a list_capability_grants response captured from a real Holochain 0.7.0 conductor
    /// (AppCapGrantInfo = Vec&lt;(CellId, Vec&lt;CapGrantInfo&gt;)&gt;).
    /// </summary>
    public class CapabilityGrantsWireTests
    {
        private static readonly MessagePackSerializerOptions Options =
            MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);

        // Hashes shortened to fixed bytes; structure exactly as the conductor sent it.
        private static byte[] Envelope()
        {
            var dna = new byte[39]; dna[0] = 0x84; dna[1] = 0x2d;
            var agent = new byte[39]; agent[0] = 0x84; agent[1] = 0x20;
            var action = new byte[39]; action[0] = 0x84; action[1] = 0x29;
            var grant = new Dictionary<string, object>
            {
                ["cap_grant"] = new Dictionary<string, object>
                {
                    ["tag"] = "zome-call-signing-key",
                    ["access"] = new Dictionary<string, object> { ["access_type"] = "assigned", ["assignees"] = new object[] { agent } },
                    ["functions"] = new Dictionary<string, object> { ["type"] = "all" }
                },
                ["action_hash"] = action,
                ["created_at"] = 1792029232652253L,
                ["revoked_at"] = null
            };
            var value = new object[] { new object[] { new object[] { dna, agent }, new object[] { grant } } };
            return MessagePackSerializer.Serialize(new Dictionary<string, object>
            {
                ["type"] = "capability_grants_info",
                ["value"] = value
            }, Options);
        }

        [Fact]
        public void CapabilityGrantsInfo_DecodesCellAndGrant()
        {
            var cells = HoloNETClientBase.DeserializeResponseValue<CellCapGrantInfo[]>(Envelope(), Options);

            var cell = Assert.Single(cells);
            Assert.Equal(39, cell.cell_id.dna_hash.Length);
            var grant = Assert.Single(cell.cap_grants);
            Assert.Equal("zome-call-signing-key", grant.cap_grant.tag);
            Assert.Equal("assigned", grant.cap_grant.access.access_type);
            Assert.Single(grant.cap_grant.access.assignees);
            Assert.Equal(39, grant.action_hash.Length);
            Assert.Null(grant.revoked_at);
        }
    }
}

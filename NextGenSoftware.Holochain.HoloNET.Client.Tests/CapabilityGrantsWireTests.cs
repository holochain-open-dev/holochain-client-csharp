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
        private static byte[] Envelope() => Envelope(holochain071: false);

        /// <summary>Holochain 0.7.0 grant: { tag, access, functions }. 0.7.1: { tag, constraint, capability }.</summary>
        private static byte[] Envelope(bool holochain071)
        {
            var dna = new byte[39]; dna[0] = 0x84; dna[1] = 0x2d;
            var agent = new byte[39]; agent[0] = 0x84; agent[1] = 0x20;
            var action = new byte[39]; action[0] = 0x84; action[1] = 0x29;
            var access = new Dictionary<string, object> { ["access_type"] = "assigned", ["assignees"] = new object[] { agent } };
            var functions = new Dictionary<string, object> { ["type"] = "all" };
            var capGrant = holochain071
                ? new Dictionary<string, object>
                {
                    ["tag"] = "zome-call-signing-key",
                    ["constraint"] = access,
                    ["capability"] = new Dictionary<string, object> { ["type"] = "zome_call", ["value"] = new Dictionary<string, object> { ["functions"] = functions } }
                }
                : new Dictionary<string, object> { ["tag"] = "zome-call-signing-key", ["access"] = access, ["functions"] = functions };
            var grant = new Dictionary<string, object>
            {
                ["cap_grant"] = capGrant,
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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CapabilityGrantsInfo_DecodesCellAndGrant_OnHolochain070And071(bool holochain071)
        {
            var cells = HoloNETClientBase.DeserializeResponseValue<CellCapGrantInfo[]>(Envelope(holochain071), Options);

            var cell = Assert.Single(cells);
            Assert.Equal(39, cell.cell_id.dna_hash.Length);
            var grant = Assert.Single(cell.cap_grants);
            Assert.Equal("zome-call-signing-key", grant.cap_grant.tag);
            Assert.Equal("assigned", grant.cap_grant.AccessInfo.access_type);
            Assert.Single(grant.cap_grant.AccessInfo.assignees);
            Assert.Equal("all", grant.cap_grant.Functions["type"]);
            Assert.Equal("zome_call", grant.cap_grant.CapabilityType);
            Assert.Equal(39, grant.action_hash.Length);
            Assert.Null(grant.revoked_at);
        }
    }
}

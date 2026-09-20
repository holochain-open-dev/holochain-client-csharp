using System.Collections.Generic;
using MessagePack;
using Xunit;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>
    /// Group (b): round-trip serialization tests for the 10 newly-wired Admin/App request/response
    /// types added in the Holochain 0.7.0 upgrade, using the exact same MessagePackSerializerOptions
    /// used on the real send/receive path.
    /// </summary>
    public class NewAdminAppTypesSerializationTests
    {
        private static readonly MessagePackSerializerOptions Options =
            MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);

        private static byte[] MakeHashLikeBytes(byte seed)
        {
            var bytes = new byte[39];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = (byte)(seed + i);
            return bytes;
        }

        [Fact]
        public void RevokeZomeCallCapabilityRequest_RoundTrips()
        {
            var original = new RevokeZomeCallCapabilityRequest
            {
                action_hash = MakeHashLikeBytes(5),
                cell_id = new CellId(MakeHashLikeBytes(1), MakeHashLikeBytes(50))
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<RevokeZomeCallCapabilityRequest>(bytes, Options);

            Assert.Equal(original.action_hash, result.action_hash);
            Assert.Equal(original.cell_id.dna_hash, result.cell_id.dna_hash);
            Assert.Equal(original.cell_id.agent_pub_key, result.cell_id.agent_pub_key);
        }

        [Fact]
        public void ListCapabilityGrantsRequest_RoundTrips()
        {
            var original = new ListCapabilityGrantsRequest
            {
                installed_app_id = "my_test_app",
                include_revoked = true
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<ListCapabilityGrantsRequest>(bytes, Options);

            Assert.Equal(original.installed_app_id, result.installed_app_id);
            Assert.Equal(original.include_revoked, result.include_revoked);
        }

        [Fact]
        public void PeerMetaInfoRequest_RoundTrips()
        {
            var original = new PeerMetaInfoRequest
            {
                url = "wss://peer.example.org/123",
                dna_hashes = new List<byte[]> { MakeHashLikeBytes(1), MakeHashLikeBytes(2) }
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<PeerMetaInfoRequest>(bytes, Options);

            Assert.Equal(original.url, result.url);
            Assert.Equal(original.dna_hashes.Count, result.dna_hashes.Count);
            Assert.Equal(original.dna_hashes[0], result.dna_hashes[0]);
            Assert.Equal(original.dna_hashes[1], result.dna_hashes[1]);
        }

        [Fact]
        public void PeerMetaInfoRequest_RoundTrips_WithNullDnaHashes()
        {
            var original = new PeerMetaInfoRequest
            {
                url = "wss://peer.example.org/123",
                dna_hashes = null
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<PeerMetaInfoRequest>(bytes, Options);

            Assert.Equal(original.url, result.url);
            Assert.Null(result.dna_hashes);
        }

        [Fact]
        public void PeerMetaInfoResponse_RoundTrips()
        {
            var original = new PeerMetaInfoResponse
            {
                dna_hash_peer_meta = new Dictionary<string, Dictionary<string, dynamic>>
                {
                    ["dna_hash_1"] = new Dictionary<string, dynamic>
                    {
                        ["wss://peer1.example.org"] = "opaque_meta_value_1"
                    }
                }
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<PeerMetaInfoResponse>(bytes, Options);

            Assert.Single(result.dna_hash_peer_meta);
            Assert.True(result.dna_hash_peer_meta.ContainsKey("dna_hash_1"));
            Assert.True(result.dna_hash_peer_meta["dna_hash_1"].ContainsKey("wss://peer1.example.org"));
            Assert.Equal("opaque_meta_value_1", (string)result.dna_hash_peer_meta["dna_hash_1"]["wss://peer1.example.org"]);
        }

        [Fact]
        public void IssueAppAuthenticationTokenRequest_RoundTrips()
        {
            var original = new IssueAppAuthenticationTokenRequest
            {
                installed_app_id = "my_test_app",
                expiry_seconds = 3600UL,
                single_use = false
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<IssueAppAuthenticationTokenRequest>(bytes, Options);

            Assert.Equal(original.installed_app_id, result.installed_app_id);
            Assert.Equal(original.expiry_seconds, result.expiry_seconds);
            Assert.Equal(original.single_use, result.single_use);
        }

        [Fact]
        public void AppAuthenticationTokenIssuedResponse_RoundTrips()
        {
            var original = new AppAuthenticationTokenIssuedResponse
            {
                token = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 },
                expires_at = 1735689600000000L
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<AppAuthenticationTokenIssuedResponse>(bytes, Options);

            Assert.Equal(original.token, result.token);
            Assert.Equal(original.expires_at, result.expires_at);
        }

        [Fact]
        public void AppAuthenticationTokenIssuedResponse_RoundTrips_WithNullExpiresAt()
        {
            var original = new AppAuthenticationTokenIssuedResponse
            {
                token = new byte[] { 9, 9, 9 },
                expires_at = null
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<AppAuthenticationTokenIssuedResponse>(bytes, Options);

            Assert.Equal(original.token, result.token);
            Assert.Null(result.expires_at);
        }

        [Fact]
        public void RevokeAppAuthenticationTokenRequest_RoundTrips()
        {
            var original = new RevokeAppAuthenticationTokenRequest
            {
                token = new byte[] { 1, 2, 3, 4, 5 }
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<RevokeAppAuthenticationTokenRequest>(bytes, Options);

            Assert.Equal(original.token, result.token);
        }

        [Fact]
        public void GetCompatibleCellsRequest_RoundTrips()
        {
            var original = new GetCompatibleCellsRequest
            {
                dna_hash = MakeHashLikeBytes(3)
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<GetCompatibleCellsRequest>(bytes, Options);

            Assert.Equal(original.dna_hash, result.dna_hash);
        }

        [Fact]
        public void CompatibleCellsResponse_RoundTrips()
        {
            // AdminResponse::CompatibleCells is BTreeSet<(InstalledAppId, BTreeSet<CellId>)> on the
            // Rust side; HoloNET models the response as an array of AppCompatibleCells entries.
            var original = new AppCompatibleCells[]
            {
                new AppCompatibleCells
                {
                    installed_app_id = "app_one",
                    cell_ids = new List<CellId>
                    {
                        new CellId(MakeHashLikeBytes(1), MakeHashLikeBytes(50)),
                        new CellId(MakeHashLikeBytes(2), MakeHashLikeBytes(60))
                    }
                }
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<AppCompatibleCells[]>(bytes, Options);

            Assert.Single(result);
            Assert.Equal(original[0].installed_app_id, result[0].installed_app_id);
            Assert.Equal(original[0].cell_ids.Count, result[0].cell_ids.Count);
            Assert.Equal(original[0].cell_ids[0].dna_hash, result[0].cell_ids[0].dna_hash);
            Assert.Equal(original[0].cell_ids[1].agent_pub_key, result[0].cell_ids[1].agent_pub_key);
        }

        [Fact]
        public void CreateCloneCellRequest_RoundTrips()
        {
            var original = new CreateCloneCellRequest
            {
                role_name = "my_role",
                modifiers = new Dictionary<object, object> { ["network_seed"] = "seed123" },
                membrane_proof = new byte[] { 1, 2, 3 },
                name = "my_clone"
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<CreateCloneCellRequest>(bytes, Options);

            Assert.Equal(original.role_name, result.role_name);
            Assert.Equal(original.membrane_proof, result.membrane_proof);
            Assert.Equal(original.name, result.name);
            Assert.NotNull(result.modifiers);
        }

        [Fact]
        public void CreateCloneCellRequest_RoundTrips_WithNullOptionalFields()
        {
            var original = new CreateCloneCellRequest
            {
                role_name = "my_role",
                modifiers = null,
                membrane_proof = null,
                name = null
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<CreateCloneCellRequest>(bytes, Options);

            Assert.Equal(original.role_name, result.role_name);
            Assert.Null(result.membrane_proof);
            Assert.Null(result.name);
        }

        [Fact]
        public void ClonedCellResponse_RoundTrips()
        {
            var original = new ClonedCellResponse
            {
                cell_id = new CellId(MakeHashLikeBytes(1), MakeHashLikeBytes(50)),
                clone_id = "my_role.0",
                original_dna_hash = MakeHashLikeBytes(7),
                dna_modifiers = new Dictionary<object, object> { ["network_seed"] = "seed123" },
                name = "my_clone",
                enabled = true
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<ClonedCellResponse>(bytes, Options);

            Assert.Equal(original.cell_id.dna_hash, result.cell_id.dna_hash);
            Assert.Equal(original.cell_id.agent_pub_key, result.cell_id.agent_pub_key);
            Assert.Equal(original.clone_id, result.clone_id);
            Assert.Equal(original.original_dna_hash, result.original_dna_hash);
            Assert.Equal(original.name, result.name);
            Assert.Equal(original.enabled, result.enabled);
        }

        [Fact]
        public void DisableCloneCellRequest_RoundTrips_WithCloneIdString()
        {
            var original = new DisableCloneCellRequest
            {
                clone_cell_id = "my_role.0"
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<DisableCloneCellRequest>(bytes, Options);

            Assert.Equal("my_role.0", (string)result.clone_cell_id);
        }

        [Fact]
        public void CountersigningCellIdRequest_RoundTrips()
        {
            var original = new CountersigningCellIdRequest
            {
                cell_id = new CellId(MakeHashLikeBytes(1), MakeHashLikeBytes(50))
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<CountersigningCellIdRequest>(bytes, Options);

            Assert.Equal(original.cell_id.dna_hash, result.cell_id.dna_hash);
            Assert.Equal(original.cell_id.agent_pub_key, result.cell_id.agent_pub_key);
        }

        [Fact]
        public void CountersigningSessionStateResponse_RoundTrips()
        {
            var original = new CountersigningSessionStateResponse
            {
                session_state = "Accepted"
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<CountersigningSessionStateResponse>(bytes, Options);

            Assert.Equal("Accepted", (string)result.session_state);
        }

        [Fact]
        public void CountersigningSessionStateResponse_RoundTrips_WithNullState()
        {
            var original = new CountersigningSessionStateResponse
            {
                session_state = null
            };

            byte[] bytes = MessagePackSerializer.Serialize(original, Options);
            var result = MessagePackSerializer.Deserialize<CountersigningSessionStateResponse>(bytes, Options);

            Assert.Null(result.session_state);
        }

        [Fact]
        public void ProvideMemproofsRequest_RoundTrips()
        {
            // ProvideMemproofsRequest is not [MessagePackObject]-decorated itself; it exposes an
            // implicit conversion to Dictionary<string, byte[]>, which is the shape that actually
            // gets serialized on the wire (see ProvideMemproofsRequest.cs).
            var original = new ProvideMemproofsRequest
            {
                MembraneProofs = new Dictionary<string, byte[]>
                {
                    ["role_one"] = new byte[] { 1, 2, 3 },
                    ["role_two"] = new byte[] { 4, 5, 6 }
                }
            };

            Dictionary<string, byte[]> wireShape = original;
            byte[] bytes = MessagePackSerializer.Serialize(wireShape, Options);
            var result = MessagePackSerializer.Deserialize<Dictionary<string, byte[]>>(bytes, Options);

            Assert.Equal(2, result.Count);
            Assert.Equal(original.MembraneProofs["role_one"], result["role_one"]);
            Assert.Equal(original.MembraneProofs["role_two"], result["role_two"]);
        }
    }
}

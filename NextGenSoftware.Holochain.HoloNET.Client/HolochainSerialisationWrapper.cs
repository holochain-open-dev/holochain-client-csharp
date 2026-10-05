using MessagePack;
using System;
using System.Security.Cryptography;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public struct ZomeCallUnsigned
    {
        public byte[] provenance;
        public byte[] cell_id_dna_hash;
        public byte[] cell_id_agent_pub_key;
        public string zome_name;
        public string fn_name;
        public byte[] cap_secret;
        public byte[] payload;
        public byte[] nonce;
        public long expires_at;
    }

    /// <summary>
    /// Implements Holochain 0.7's ZomeCallParams::serialize_and_hash contract in managed code.
    /// This is the sole signing path so desktop, Unity and mobile clients sign identical
    /// canonical bytes without a platform-specific native serialization wrapper.
    /// </summary>
    public static class HolochainSerialisationWrapper
    {
        internal const int DataToSignLength = 64;

        internal static byte[] serialize_for_signing(ZomeCallUnsigned zomeCallUnsigned)
        {
            Validate(zomeCallUnsigned);
            var call = new ZomeCall
            {
                provenance = zomeCallUnsigned.provenance,
                cell_id = new CellId(zomeCallUnsigned.cell_id_dna_hash, zomeCallUnsigned.cell_id_agent_pub_key),
                zome_name = zomeCallUnsigned.zome_name,
                fn_name = zomeCallUnsigned.fn_name,
                cap_secret = zomeCallUnsigned.cap_secret,
                payload = zomeCallUnsigned.payload,
                nonce = zomeCallUnsigned.nonce,
                expires_at = zomeCallUnsigned.expires_at
            };

            return MessagePackSerializer.Serialize(
                call,
                MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData));
        }

        internal static void call_get_data_to_sign(byte[] data, ZomeCallUnsigned zomeCallUnsigned)
        {
            if (data == null || data.Length != DataToSignLength)
                throw new ArgumentException(
                    $"The Holochain 0.7 data-to-sign buffer must be {DataToSignLength} bytes.",
                    nameof(data));

            using (var sha512 = SHA512.Create())
            {
                var hash = sha512.ComputeHash(serialize_for_signing(zomeCallUnsigned));
                Buffer.BlockCopy(hash, 0, data, 0, DataToSignLength);
            }
        }

        private static void Validate(ZomeCallUnsigned call)
        {
            if (call.provenance == null) throw new ArgumentNullException(nameof(call.provenance));
            if (call.cell_id_dna_hash == null) throw new ArgumentNullException(nameof(call.cell_id_dna_hash));
            if (call.cell_id_agent_pub_key == null) throw new ArgumentNullException(nameof(call.cell_id_agent_pub_key));
            if (string.IsNullOrWhiteSpace(call.zome_name)) throw new ArgumentException("A zome name is required.", nameof(call.zome_name));
            if (string.IsNullOrWhiteSpace(call.fn_name)) throw new ArgumentException("A function name is required.", nameof(call.fn_name));
            if (call.payload == null) throw new ArgumentNullException(nameof(call.payload));
            if (call.nonce == null || call.nonce.Length != 32) throw new ArgumentException("A 32-byte nonce is required.", nameof(call.nonce));
        }
    }
}

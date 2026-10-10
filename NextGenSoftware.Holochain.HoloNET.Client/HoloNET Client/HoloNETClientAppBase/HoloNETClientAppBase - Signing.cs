using System;
using Chaos.NaCl;
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public abstract partial class HoloNETClientAppBase
    {
        /// <summary>
        /// Optional external signer. When set, every zome call is signed by it instead of by the local
        /// credentials from AuthorizeSigningCredentials (e.g. the shared Android runtime's signZomeCall).
        /// </summary>
        public IZomeCallSigner ZomeCallSigner { get; set; }

        /// <summary>
        /// Signs with the credentials AuthorizeSigningCredentials stored for the call's cell, or returns
        /// null when none have been authorized.
        /// </summary>
        public ZomeCallParamsSigned SignZomeCallWithAuthorizedCredentials(ZomeCallToSign call)
        {
            string cellId = $"{ConvertHoloHashToString(call.CellIdAgentPubKey)}:{ConvertHoloHashToString(call.CellIdDnaHash)}";

            return _signingCredentialsForCell.TryGetValue(cellId, out SigningCredentials credentials) && credentials != null
                ? SignZomeCallLocally(call, credentials)
                : null;
        }

        /// <summary>
        /// Signs a zome call with locally held credentials (the default when ZomeCallSigner is unset).
        /// Public so a custom signer can delegate to it.
        /// </summary>
        public static ZomeCallParamsSigned SignZomeCallLocally(ZomeCallToSign call, SigningCredentials credentials)
        {
            ZomeCallUnsigned payload = new ZomeCallUnsigned()
            {
                cap_secret = credentials.CapSecret,
                cell_id_agent_pub_key = call.CellIdAgentPubKey,
                cell_id_dna_hash = call.CellIdDnaHash,
                fn_name = call.FnName,
                zome_name = call.ZomeName,
                payload = call.Payload,
                provenance = credentials.SigningKey,
                nonce = call.Nonce,
                expires_at = call.ExpiresAt
            };

            // Holochain 0.7 signs SHA-512(canonical MessagePack(ZomeCallParams)).
            byte[] hash = new byte[HolochainSerialisationWrapper.DataToSignLength];
            HolochainSerialisationWrapper.call_get_data_to_sign(hash, payload);

            byte[] sig = Ed25519.Sign(hash, credentials.KeyPair.PrivateKey);

            ZomeCallSigned signedPayload = new ZomeCallSigned()
            {
                cap_secret = payload.cap_secret,
                cell_id = new CellId(payload.cell_id_dna_hash, payload.cell_id_agent_pub_key),
                fn_name = payload.fn_name,
                zome_name = payload.zome_name,
                payload = payload.payload,
                provenance = payload.provenance,
                nonce = payload.nonce,
                expires_at = payload.expires_at,
                signature = sig[0..64]
            };

            // AppRequest::CallZome takes ZomeCallParamsSigned { bytes, signature }, where bytes is the
            // msgpack encoding of the unsigned ZomeCall fields (see ZomeCallParamsSigned.cs).
            byte[] unsignedBytes = MessagePackSerializer.Serialize<ZomeCall>(signedPayload, MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.None));
            return new ZomeCallParamsSigned(unsignedBytes, signedPayload.signature);
        }
    }
}

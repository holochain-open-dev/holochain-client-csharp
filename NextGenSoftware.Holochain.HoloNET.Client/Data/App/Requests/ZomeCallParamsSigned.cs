using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors holochain_conductor_api::app_interface::ZomeCallParamsSigned (Holochain 0.7.0).
    /// This is the actual struct sent for the `call_zome` AppRequest on the app websocket
    /// interface as of 0.6.1: `AppRequest::CallZome(Box&lt;ZomeCallParamsSigned&gt;)`.
    ///
    /// pub struct ZomeCallParamsSigned {
    ///     /// Bytes of the serialized zome call payload (holochain_serialized_bytes::encode of
    ///     /// the unsigned ZomeCallParams/ZomeCall fields).
    ///     pub bytes: ExternIO,
    ///     /// Signature over the (sha2-512) hash of `bytes`, signed by the provenance agent key.
    ///     pub signature: Signature,
    /// }
    ///
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/app_interface.rs
    /// </summary>
    [MessagePackObject]
    public class ZomeCallParamsSigned
    {
        [Key("bytes")]
        public byte[] bytes { get; set; }

        [Key("signature")]
        public byte[] signature { get; set; }

        public ZomeCallParamsSigned() { }

        public ZomeCallParamsSigned(byte[] bytes, byte[] signature)
        {
            this.bytes = bytes;
            this.signature = signature;
        }
    }
}

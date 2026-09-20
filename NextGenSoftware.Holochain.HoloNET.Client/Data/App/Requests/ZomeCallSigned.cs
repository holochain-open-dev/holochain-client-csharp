using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// NOTE: As of Holochain 0.6.1 the wire-level signed zome call sent over the app
    /// websocket interface is no longer a flattened struct with a trailing signature field.
    /// It is `holochain_conductor_api::app_interface::ZomeCallParamsSigned`:
    ///
    /// pub struct ZomeCallParamsSigned {
    ///     pub bytes: ExternIO,   // holochain_serialized_bytes::encode(&ZomeCallParams)
    ///     pub signature: Signature,
    /// }
    ///
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/app_interface.rs
    ///
    /// This class is kept (mirroring the previous flattened shape built from <see cref="ZomeCall"/>)
    /// for backward compatibility with any code/tests that reference it, but it is no longer what
    /// gets sent on the wire - see <see cref="ZomeCallParamsSigned"/> for the real 0.6.1 wire shape,
    /// which HoloNETClientAppBase now uses when constructing the `call_zome` request.
    /// </summary>
    [MessagePackObject]
    public class ZomeCallSigned : ZomeCall
    {
        [Key("signature")]
        public byte[] signature { get; set; }
    }
}
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors the payload of AppRequest::ProvideMemproofs(MemproofMap) (Holochain 0.7.0):
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/app_interface.rs
    ///
    /// NOTE: `MemproofMap`'s precise Rust definition could not be conclusively located/verified
    /// at the time of writing. Based on its usage as "a membrane proof per role name" elsewhere
    /// in the Holochain app installation flow (see InstallAppRequest's `membrane_proofs: HashMap
    /// &lt;RoleName, MembraneProof&gt;` for the analogous shape used at install time), it is modelled
    /// here as a plain role-name-keyed dictionary of raw membrane proof bytes (MessagePack
    /// serializes Dictionary&lt;string, byte[]&gt; as a map natively, matching the Rust map shape).
    /// If the real MemproofMap turns out to differ, only this type alias needs updating.
    /// </summary>
    public class ProvideMemproofsRequest
    {
        public static implicit operator Dictionary<string, byte[]>(ProvideMemproofsRequest r) => r?.MembraneProofs;
        public Dictionary<string, byte[]> MembraneProofs { get; set; } = new Dictionary<string, byte[]>();
    }
}

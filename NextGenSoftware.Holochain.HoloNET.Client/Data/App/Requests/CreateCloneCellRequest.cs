using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors holochain_types::app::CreateCloneCellPayload, used as the payload for
    /// AppRequest::CreateCloneCell (Holochain 0.7.0):
    ///
    /// pub struct CreateCloneCellPayload {
    ///     pub role_name: RoleName,                          // String
    ///     pub modifiers: DnaModifiersOpt&lt;YamlProperties&gt;,
    ///     pub membrane_proof: Option&lt;MembraneProof&gt;,         // Option&lt;Arc&lt;SerializedBytes&gt;&gt;
    ///     pub name: Option&lt;String&gt;,
    /// }
    /// https://docs.rs/holochain_types/0.7.0/holochain_types/app/struct.CreateCloneCellPayload.html
    ///
    /// Verified against holochain_types/src/app.rs at holochain-0.7.0 — all four fields match.
    /// `DnaModifiersOpt&lt;YamlProperties&gt;` is represented as `dynamic` since its sub-fields
    /// (network_seed, properties, origin_time, quantum_time) are each individually Optional;
    /// callers can supply a plain anonymous object/dictionary with only the fields they need.
    /// </summary>
    [MessagePackObject]
    public class CreateCloneCellRequest
    {
        [Key("role_name")]
        public string role_name { get; set; }

        [Key("modifiers")]
        public dynamic modifiers { get; set; }

        [Key("membrane_proof")]
        public byte[] membrane_proof { get; set; }

        [Key("name")]
        public string name { get; set; }
    }
}

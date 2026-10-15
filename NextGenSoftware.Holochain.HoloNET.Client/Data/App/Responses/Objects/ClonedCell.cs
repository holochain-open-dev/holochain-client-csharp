
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.AppManifest;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.App.Responses.Objects
{
    [MessagePackObject]
    public class ClonedCell : ICell
    {
        [Key("cell_id")]
        public byte[][] cell_id { get; set; }

        // holochain_zome_types CloneId is a String newtype ("oasis.0") on the wire, not a {role_name} struct, so app_info
        // for an app with a clone cell failed to decode.
        //[Key("clone_id")]
        //public CloneId clone_id { get; set; }

        [Key("clone_id")]
        public string clone_id { get; set; }

        [Key("original_dna_hash")]
        public byte[] original_dna_hash { get; set; }

        [Key("dna_modifiers")]
        public DnaModifiers dna_modifiers { get; set; } //DnaModifiers

        [Key("name")]
        public string name { get; set; }

        [Key("enabled")]
        public bool enabled { get; set; }
    }
}

using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public struct ClonedCell : ICell
    {
        [Key("cell_id")]
        public byte[][] cell_id { get; set; }

        [Key("clone_id")]
        public CloneId clone_id { get; set; }

        [Key("original_dna_hash")]
        public byte[] original_dna_hash { get; set; }

        [Key("dna_modifiers")]
        public byte[] dna_modifiers { get; set; } //DnaModifiers

        [Key("name")]
        public string name { get; set; }

        [Key("enabled")]
        public bool enabled { get; set; }
    }
}
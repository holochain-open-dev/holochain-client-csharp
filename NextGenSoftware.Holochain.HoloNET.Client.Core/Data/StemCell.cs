
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Enums;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public struct StemCell
    {
        [Key("original_dna_hash")]
        public byte[] original_dna_hash { get; set; }

        [Key("dna_modifiers")]
        public byte[] dna_modifiers { get; set; } //DnaModifiers

        [Key("name")]
        public OptionType name { get; set; } // pub name: Option<String>,
    }
}
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data
{
    public struct ProvisionedCell
    {
        [Key("cell_id")]
        public byte[] cell_id { get; set; }

        [Key("dna_modifiers")]
        public string dna_modifiers { get; set; } //pub dna_modifiers: DnaModifiers,

        [Key("name")]
        public string name { get; set; }
    }
}

namespace NextGenSoftware.Holochain.HoloNET.Client.Interfaces
{
    public interface ICell
    {
        public byte[] dna_modifiers { get; set; } //DnaModifiers

        public string name { get; set; } // pub name: Option<String>,
    }
}
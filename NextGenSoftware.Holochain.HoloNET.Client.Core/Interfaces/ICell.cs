
using NextGenSoftware.Holochain.HoloNET.Client.Data.AppManifest;

namespace NextGenSoftware.Holochain.HoloNET.Client.Interfaces
{
    public interface ICell
    {
        public DnaModifiers dna_modifiers { get; set; } //DnaModifiers

        public string name { get; set; } // pub name: Option<String>,
    }
}
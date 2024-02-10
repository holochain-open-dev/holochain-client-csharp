
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.AppManifest;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class DnaDefinitionResponse : IDnaDefinitionResponse
    {
        [Key("name")]
        public string name { get; set; }

        [Key("modifiers")]
        public DnaModifiers modifiers { get; set; }

        [Key("integrity_zomes")]
        public ZomeDefinition[] integrity_zomes { get; set; }

        [Key("integrity_zomes")]
        public ZomeDefinition[] coordinator_zomes { get; set; }
    }
}

using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class DnaDefinitionResponseDetail //: IDnaDefinitionResponse
    {
        [Key("name")]
        public string name { get; set; }

        [Key("modifiers")]
        //public DnaModifiers modifiers { get; set; }
        public object modifiers { get; set; }

        [Key("integrity_zomes")]
        //public ZomeDefinition[] integrity_zomes { get; set; }
        public object integrity_zomes { get; set; }

        [Key("coordinator_zomes")]
        //public ZomeDefinition[] coordinator_zomes { get; set; }
        public object coordinator_zomes { get; set; }
    }
}
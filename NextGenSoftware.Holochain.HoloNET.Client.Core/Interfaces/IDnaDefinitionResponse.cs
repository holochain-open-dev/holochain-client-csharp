using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.AppManifest;

namespace NextGenSoftware.Holochain.HoloNET.Client.Interfaces
{
    public interface IDnaDefinitionResponse
    {
        ZomeDefinition[] coordinator_zomes { get; set; }
        ZomeDefinition[] integrity_zomes { get; set; }
        DnaModifiers modifiers { get; set; }
        string name { get; set; }
    }
}
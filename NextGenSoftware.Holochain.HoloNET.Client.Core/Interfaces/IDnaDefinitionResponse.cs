
namespace NextGenSoftware.Holochain.HoloNET.Client.Interfaces
{
    public interface IDnaDefinitionResponse
    {
        public string type { get; set; }
        public IDnaDefinitionResponseDetail data { get; set; }
    }
}
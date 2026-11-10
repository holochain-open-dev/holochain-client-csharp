using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF.Objects
{
    public class ConnectToAppAgentClientResult
    {
        public IHoloNETClientAppAgent AppAgentClient { get; set; }
        public ConnectToAppAgentClientResponseType ResponseType { get; set; }
    }
}
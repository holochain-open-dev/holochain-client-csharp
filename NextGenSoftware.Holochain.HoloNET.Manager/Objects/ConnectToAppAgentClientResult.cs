using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.Holochain.HoloNET.Manager.Enums;

namespace NextGenSoftware.Holochain.HoloNET.Manager.Objects
{
    public class ConnectToAppAgentClientResult
    {
        public IHoloNETClientAppAgent AppAgentClient { get; set; }
        public ConnectToAppAgentClientResponseType ResponseType { get; set; }
    }
}
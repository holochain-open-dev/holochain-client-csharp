
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    public class ConnectToAppAgentClientResult
    {
        public HoloNETClient AppAgentClient { get; set; }
        public ConnectToAppAgentClientResponseType ResponseType { get; set; }
    }
}
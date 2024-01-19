﻿using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF.Objects
{
    public class ConnectToAppAgentClientResult
    {
        public HoloNETClientAppAgent AppAgentClient { get; set; }
        public ConnectToAppAgentClientResponseType ResponseType { get; set; }
    }
}
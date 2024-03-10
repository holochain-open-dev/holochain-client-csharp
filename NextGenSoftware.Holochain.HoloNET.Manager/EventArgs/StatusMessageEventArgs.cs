using System;
using NextGenSoftware.Holochain.HoloNET.Manager.Enums;
using NextGenSoftware.Holochain.HoloNET.Manager.UserControls;

namespace NextGenSoftware.Holochain.HoloNET.Manager
{
    public class StatusMessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public StatusMessageType Type { get; set; } = StatusMessageType.Information;
        public bool ShowSpinner { get; set; } = false;
        public ucHoloNETEntry ucHoloNETEntry { get; set; }
    }
}
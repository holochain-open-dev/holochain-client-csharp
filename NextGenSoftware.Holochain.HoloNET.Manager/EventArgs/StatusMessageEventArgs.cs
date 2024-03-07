using System;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.UserControls;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    public class StatusMessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public StatusMessageType Type { get; set; } = StatusMessageType.Information;
        public bool ShowSpinner { get; set; } = false;
        public ucHoloNETEntry ucHoloNETEntry { get; set; }
    }
}
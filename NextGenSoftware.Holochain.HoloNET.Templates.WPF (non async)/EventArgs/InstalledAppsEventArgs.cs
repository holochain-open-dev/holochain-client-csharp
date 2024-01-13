using System;
using System.Collections.Generic;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Models;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    public class InstalledAppsEventArgs : EventArgs
    {
        public List<InstalledApp> InstalledApps { get; set; }
    }
}
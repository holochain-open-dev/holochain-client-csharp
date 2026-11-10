using System;
using System.Collections.Generic;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    public class InstalledAppsEventArgs : EventArgs
    {
        public List<IInstalledApp> InstalledApps { get; set; }
    }
}
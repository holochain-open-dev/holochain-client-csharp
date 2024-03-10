using System;
using System.Collections.Generic;
using NextGenSoftware.Holochain.HoloNET.Manager.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Manager
{
    public class InstalledAppsEventArgs : EventArgs
    {
        public List<IInstalledApp> InstalledApps { get; set; }
    }
}
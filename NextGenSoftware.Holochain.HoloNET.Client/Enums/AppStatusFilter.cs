

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public enum AppStatusFilter //May need to convert to lowecase string for each value such as enabled, disabled, etc in HoloNETClient when setting params in HoloNETAdminListAppsRequest etc.
    {
        Enabled,
        Disabled,
        Running,
        Stopped,
        Paused,
        // New in Holochain 0.7.0 - app installed but awaiting membrane proof submission
        AwaitingMemproofs,
        All
    }
}


/*
export enum AppStatusFilter {
  Enabled = "enabled",
  Disabled = "disabled",
  Running = "running",
  Stopped = "stopped",
  Paused = "paused",
}
*/
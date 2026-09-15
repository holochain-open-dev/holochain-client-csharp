

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public enum AppInfoStatusEnum
    {
        Paused,
        Disabled,
        Running,
        // New in Holochain 0.7.0 - installed but awaiting membrane proof submission before enabling
        AwaitingMemproofs,
        //RunningAndAttached,
        None
    }
}
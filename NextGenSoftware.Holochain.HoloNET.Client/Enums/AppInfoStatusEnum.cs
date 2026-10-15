

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
        None,
        // Holochain 0.7: the app's cells are being restored from the DHT.
        AwaitingRestore,
        // Holochain 0.7: a cell received a warrant (chain fork / integrity) and can't run; see AppStatusReason.
        Unrecoverable
    }
}
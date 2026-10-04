using System;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors holochain_conductor_api::AppStatusFilter (Holochain 0.7.0):
    /// Enabled, Disabled, AwaitingMemproofs, AwaitingRestore, Unrecoverable.
    /// Sent on the wire as its snake_case name via ToWireValue(), never as an integer:
    /// rmp-serde accepts integers as variant indexes, so the C# ordinal would select the wrong filter.
    /// </summary>
    public enum AppStatusFilter
    {
        Enabled,
        Disabled,
        [Obsolete("Not a Holochain 0.7.0 status; sent as \"enabled\".")]
        Running,
        [Obsolete("Not a Holochain 0.7.0 status; sent as \"disabled\".")]
        Stopped,
        [Obsolete("Not a Holochain 0.7.0 status; sent as \"disabled\".")]
        Paused,
        AwaitingMemproofs,
        All,
        AwaitingRestore,
        Unrecoverable
    }

    public static class AppStatusFilterExtensions
    {
        /// <summary>Returns the conductor's wire value, or null for All (no filter).</summary>
        public static string ToWireValue(this AppStatusFilter filter)
        {
#pragma warning disable CS0618
            switch (filter)
            {
                case AppStatusFilter.Enabled:
                case AppStatusFilter.Running:
                    return "enabled";
                case AppStatusFilter.Disabled:
                case AppStatusFilter.Stopped:
                case AppStatusFilter.Paused:
                    return "disabled";
                case AppStatusFilter.AwaitingMemproofs:
                    return "awaiting_memproofs";
                case AppStatusFilter.AwaitingRestore:
                    return "awaiting_restore";
                case AppStatusFilter.Unrecoverable:
                    return "unrecoverable";
                default:
                    return null;
            }
#pragma warning restore CS0618
        }
    }
}

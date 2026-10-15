using System.Collections.Generic;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// grant_direct_signal_capability payload (Holochain 0.7.1+, admin and app interfaces):
    /// <c>{ cell_id, tag, constraint: GrantConstraint }</c>, where GrantConstraint is adjacently tagged
    /// ({"type": "unrestricted"} / {"type": "transferable", "value": {secret}} / {"type": "assigned", "value": {secret, assignees}}),
    /// the same shape CapAccess builds.
    /// </summary>
    internal static class DirectSignalGrants
    {
        public static Dictionary<string, object> Request(byte[][] cellId, string tag, CapGrantAccessType access, byte[] secret, byte[][] assignees)
        {
            Dictionary<string, object> constraint = access switch
            {
                CapGrantAccessType.Transferable => CapAccess.Transferable(secret),
                CapGrantAccessType.Assigned => CapAccess.Assigned(secret, assignees ?? new byte[0][]),
                _ => CapAccess.Unrestricted()
            };

            return new Dictionary<string, object>
            {
                ["cell_id"] = cellId,
                ["tag"] = tag ?? "direct-signal",
                ["constraint"] = constraint
            };
        }
    }
}

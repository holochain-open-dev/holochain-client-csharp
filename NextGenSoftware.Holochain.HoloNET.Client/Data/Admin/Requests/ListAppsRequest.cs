
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests
{
    [MessagePackObject]
    public class ListAppsRequest
    {
        // Snake_case wire name from AppStatusFilter.ToWireValue(), or null for no filter.
        // Was AppStatusFilter?, which MessagePack sent as an ordinal that rmp-serde read as a
        // variant index — e.g. Running (2) filtered by AwaitingMemproofs.
        [Key("status_filter")]
        public string status_filter { get; set; }
    }
}

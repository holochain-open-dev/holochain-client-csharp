using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests
{
    [MessagePackObject]
    public class UpdateCoordinatorsRequest
    {
        [Key("cell_id")]
        public CellId CellId { get; set; }

        [Key("source")]
        public CoordinatorSource Source { get; set; }
    }

    [MessagePackObject]
    public class CoordinatorSource
    {
        [Key("type")]
        public string Type { get; set; }

        [Key("value")]
        public object Value { get; set; }
    }
}

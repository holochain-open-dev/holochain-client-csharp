
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    [System.Obsolete("Serialises as externally-tagged {\"Variant\": ...}; the conductor expects {\"type\", \"value\"}. Use CapAccess instead.")]
    [MessagePackObject]
    public class CapGrantAccessUnrestricted
    {
        [Key("Unrestricted")]
        public object Unrestricted { get; set; }
    }
}

using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    [System.Obsolete("Serialises as externally-tagged {\"Variant\": ...}; the conductor expects {\"type\", \"value\"}. Use CapAccess instead.")]
    [MessagePackObject]
    public class CapGrantAccessTransferable
    {
        [Key("Transferable")]
        public CapGrantAccessTransferableDetails Transferable { get; set; }
    }
}
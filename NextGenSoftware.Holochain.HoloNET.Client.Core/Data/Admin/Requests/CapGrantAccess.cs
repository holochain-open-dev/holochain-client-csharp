
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class CapGrantAccess
    {
        [Key("Assigned")]
        public CapGrantAccessAssigned Assigned { get; set; }
    }
}
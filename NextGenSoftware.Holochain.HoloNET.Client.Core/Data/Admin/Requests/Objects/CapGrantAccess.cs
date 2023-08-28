
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    [MessagePackObject]
    public class CapGrantAccess
    {
        [Key("Assigned")]
        public CapGrantAccessAssigned Assigned { get; set; }
    }
}
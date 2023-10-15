
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    [MessagePackObject]
    public class CapGrantAccess
    {
        //[Key("Unrestricted")]
        //public CapGrantAccessUnrestricted Unrestricted { get; set; }

        //[Key("Transferable")]
        //public CapGrantAccessTransferable Transferable { get; set; }

        [Key("Assigned")]
        public CapGrantAccessAssigned Assigned { get; set; }
    }
}
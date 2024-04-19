
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.Holochain.HoloNET.ORM.Entries;
using System;

namespace NextGenSoftware.Holochain.HoloNET.Client.TestHarness
{
    /// <summary>
    /// This example passes in a HoloNETClient instance that is shared with other classes/objects such as the Holon class/object. Only use this if you have more than one class extending HoloNETAuditEntryBaseClass/HoloNETEntryBaseClass otherwise use the Single Class Example.
    /// </summary>
    public class AvatarMultiple : HoloNETAuditEntryBase
    {
        public AvatarMultiple(IHoloNETClientAppAgent holonNETClient) : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", holonNETClient) { }

        [HolochainRustFieldName("first_name")]
        public string FirstName { get; set; }

        [HolochainRustFieldName("last_name")]
        public string LastName { get; set; }

        [HolochainRustFieldName("email")]
        public string Email { get; set; }

        [HolochainRustFieldName("dob")]
        public DateTime DOB { get; set; }
    }
}
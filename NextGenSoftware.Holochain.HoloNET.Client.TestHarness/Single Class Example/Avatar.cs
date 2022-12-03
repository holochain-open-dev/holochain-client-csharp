
using System;

namespace NextGenSoftware.Holochain.HoloNET.Client.TestHarness
{
    public class Avatar : HoloNETAuditEntryBaseClass
    {
        public Avatar() : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar") { }
        public Avatar(HoloNETConfig holoNETConfig) : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", holoNETConfig) { }

        [HolochainPropertyName("first_name")]
        public string FirstName { get; set; }

        [HolochainPropertyName("last_name")]
        public string LastName { get; set; }

        [HolochainPropertyName("email")]
        public string Email { get; set; }

        [HolochainPropertyName("dob")]
        public DateTime DOB { get; set; }
    }
}
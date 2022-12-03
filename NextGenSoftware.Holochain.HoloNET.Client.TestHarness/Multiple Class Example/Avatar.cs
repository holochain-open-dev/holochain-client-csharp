
using System;

namespace NextGenSoftware.Holochain.HoloNET.Client.TestHarness
{
    public class AvatarMultiple : HoloNETAuditEntryBaseClass
    {
        public AvatarMultiple(HoloNETClient holonNETClient) : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", holonNETClient) { }

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
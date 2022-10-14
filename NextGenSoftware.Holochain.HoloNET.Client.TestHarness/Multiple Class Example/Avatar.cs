
using System;

namespace NextGenSoftware.Holochain.HoloNET.Client.TestHarness
{
    public class AvatarMultiple : HoloNETEntryBaseClass
    {
        public AvatarMultiple(HoloNETClient holonNETClient) : base("oasis", "get_avatar_entry", "create_avatar_entry", "update_avatar_entry", "delete_avatar_entry", holonNETClient) { }

        [HolochainPropertyName("id")]
        public Guid Id { get; set; }

        [HolochainPropertyName("first_name")]
        public string FirstName { get; set; }

        [HolochainPropertyName("last_name")]
        public string LastName { get; set; }

        [HolochainPropertyName("email")]
        public string Email { get; set; }

        [HolochainPropertyName("dob")]
        public DateTime DOB { get; set; }

        [HolochainPropertyName("created_date")]
        public DateTime CreatedDate { get; set; }

        [HolochainPropertyName("created_by")]
        public Guid CreatedBy { get; set; }

        [HolochainPropertyName("modified_date")]
        public DateTime ModifiedDate { get; set; }

        [HolochainPropertyName("modified_by")]
        public Guid ModifiedBy { get; set; }

        [HolochainPropertyName("deleted_date")]
        public DateTime DeletedDate { get; set; }

        [HolochainPropertyName("deleted_by")]
        public Guid DeletedBy { get; set; }

        [HolochainPropertyName("is_active")]
        public bool IsActive { get; set; }
    }
}
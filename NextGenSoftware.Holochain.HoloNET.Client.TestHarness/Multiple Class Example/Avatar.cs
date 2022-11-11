
using System;

namespace NextGenSoftware.Holochain.HoloNET.Client.TestHarness
{
    public class AvatarMultiple : HoloNETAuditEntryBaseClass
    {
        public AvatarMultiple(HoloNETClient holonNETClient) : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", holonNETClient) { }

        //[HolochainPropertyName("id")]
        //public Guid Id { get; set; }

        [HolochainPropertyName("first_name")]
        public string FirstName { get; set; }

        [HolochainPropertyName("last_name")]
        public string LastName { get; set; }

        [HolochainPropertyName("email")]
        public string Email { get; set; }

        [HolochainPropertyName("dob")]
        public DateTime DOB { get; set; }

        /*
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
        public bool IsActive { get; set; }*/
    }
}
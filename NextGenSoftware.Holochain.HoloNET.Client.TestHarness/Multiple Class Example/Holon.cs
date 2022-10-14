
using System;

namespace NextGenSoftware.Holochain.HoloNET.Client.TestHarness
{
    public class Holon : HoloNETEntryBaseClass
    {
        public Holon(HoloNETClient holonNETClient) : base("oasis", "get_holon_entry", "create_holon_entry", "update_holon_entry", "delete_holon_entry", holonNETClient) { }

        [HolochainPropertyName("id")]
        public Guid Id { get; set; }


        [HolochainPropertyName("parent_id")]
        public Guid ParentId { get; set; }

        [HolochainPropertyName("name")]
        public string Name { get; set; }

        [HolochainPropertyName("description")]
        public string Description { get; set; }

        [HolochainPropertyName("type")]
        public string Type { get; set; }

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
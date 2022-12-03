
using System;

namespace NextGenSoftware.Holochain.HoloNET.Client.TestHarness
{
    public class Holon : HoloNETAuditEntryBaseClass
    {
        public Holon(HoloNETClient holonNETClient) : base("oasis", "get_holon_entry", "create_holon_entry", "update_holon_entry", "delete_holon_entry", holonNETClient) { }

        [HolochainPropertyName("parent_id")]
        public Guid ParentId { get; set; }

        [HolochainPropertyName("name")]
        public string Name { get; set; }

        [HolochainPropertyName("description")]
        public string Description { get; set; }

        [HolochainPropertyName("type")]
        public string Type { get; set; }
    }
}
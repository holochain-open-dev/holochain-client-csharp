
using System;

namespace NextGenSoftware.Holochain.HoloNET.Client.TestHarness
{
    /// <summary>
    /// This example passes in a HoloNETClient instance that is shared with other classes/objects such as the AvatarMultiple class/object. Only use this if you have more than one class extending HoloNETAuditEntryBaseClass/HoloNETEntryBaseClass otherwise use the Single Class Example.
    /// </summary>
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
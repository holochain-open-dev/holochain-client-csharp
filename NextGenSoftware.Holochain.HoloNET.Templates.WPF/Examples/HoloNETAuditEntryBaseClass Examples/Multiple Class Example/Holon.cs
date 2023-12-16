
using NextGenSoftware.Holochain.HoloNET.Client;
using System;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    /// <summary>
    /// This example passes in a HoloNETClient instance that is shared with other classes/objects such as the AvatarMultiple class/object. Only use this if you have more than one class extending HoloNETAuditEntryBaseClass/HoloNETEntryBaseClass otherwise use the Single Class Example.
    /// </summary>
    public class Holon : HoloNETAuditEntryBase
    {
        public Holon(HoloNETClient holoNETDNA) : base("oasis", "get_holon_entry", "create_holon_entry", "update_holon_entry", "delete_holon_entry", holoNETDNA) { }

        [HolochainRustFieldName("parent_id")]
        public Guid ParentId { get; set; }

        [HolochainRustFieldName("name")]
        public string Name { get; set; }

        [HolochainRustFieldName("description")]
        public string Description { get; set; }

        [HolochainRustFieldName("type")]
        public string Type { get; set; }
    }
}
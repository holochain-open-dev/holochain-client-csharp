
using NextGenSoftware.Holochain.HoloNET.Client;
using System;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    /// <summary>
    /// This example passes in a HoloNETClient instance that is shared with other classes/objects such as the AvatarMultiple class/object. Only use this if you have more than one class extending HoloNETAuditEntryBaseClass/HoloNETEntryBaseClass otherwise use the Single Class Example.
    /// </summary>
    public class Holon : HoloNETAuditEntryBaseClass
    {
        public Holon(HoloNETClient holoNETDNA) : base("oasis", "get_holon_entry", "create_holon_entry", "update_holon_entry", "delete_holon_entry", holoNETDNA) { }

        [HolochainFieldName("parent_id")]
        public Guid ParentId { get; set; }

        [HolochainFieldName("name")]
        public string Name { get; set; }

        [HolochainFieldName("description")]
        public string Description { get; set; }

        [HolochainFieldName("type")]
        public string Type { get; set; }
    }
}
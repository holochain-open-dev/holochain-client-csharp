
using System;

namespace NextGenSoftware.Holochain.HoloNET.Client.TestHarness
{
    /// <summary>
    /// This example is to be used with the CallZomeFunction overloads on HoloNETClient that take either a type of the EntryDataObject (Type entryDataObjectTypeReturnedFromZome) to map the zome function returned data onto or the actual instance of a dynamic object (dynamic entryDataObjectReturnedFromZome) to map onto.
    /// </summary>
    public class AvatarEntryDataObject
    {
        /// <summary>
        /// GUID Id that is consistent across multiple versions of the entry (each version has a different hash).
        /// </summary>
        [HolochainRustFieldName("id")]
        public Guid Id { get; set; }

        [HolochainRustFieldName("first_name")]
        public string FirstName { get; set; }

        [HolochainRustFieldName("last_name")]
        public string LastName { get; set; }

        [HolochainRustFieldName("email")]
        public string Email { get; set; }

        [HolochainRustFieldName("dob")]
        public DateTime DOB { get; set; }

        /// <summary>
        /// The date the entry was created.
        /// </summary>
        [HolochainRustFieldName("created_date")]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The AgentId who created the entry.
        /// </summary>
        [HolochainRustFieldName("created_by")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// The date the entry was last modified.
        /// </summary>
        [HolochainRustFieldName("modified_date")]
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// The AgentId who modifed the entry.
        /// </summary>
        [HolochainRustFieldName("modified_by")]
        public string ModifiedBy { get; set; }

        /// <summary>
        /// The date the entry was soft deleted.
        /// </summary>
        [HolochainRustFieldName("deleted_date")]
        public DateTime DeletedDate { get; set; }

        /// <summary>
        /// The AgentId who deleted the entry.
        /// </summary>
        [HolochainRustFieldName("deleted_by")]
        public string DeletedBy { get; set; }

        /// <summary>
        /// Flag showing the whether this entry is active or not.
        /// </summary>
        [HolochainRustFieldName("is_active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// The current version of the entry.
        /// </summary>
        [HolochainRustFieldName("version")]
        public int Version { get; set; } = 1;
    }
}
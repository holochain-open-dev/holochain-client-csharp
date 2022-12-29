
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

        /// <summary>
        /// The date the entry was created.
        /// </summary>
        [HolochainPropertyName("created_date")]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The AgentId who created the entry.
        /// </summary>
        [HolochainPropertyName("created_by")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// The date the entry was last modified.
        /// </summary>
        [HolochainPropertyName("modified_date")]
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// The AgentId who modifed the entry.
        /// </summary>
        [HolochainPropertyName("modified_by")]
        public string ModifiedBy { get; set; }

        /// <summary>
        /// The date the entry was soft deleted.
        /// </summary>
        [HolochainPropertyName("deleted_date")]
        public DateTime DeletedDate { get; set; }

        /// <summary>
        /// The AgentId who deleted the entry.
        /// </summary>
        [HolochainPropertyName("deleted_by")]
        public string DeletedBy { get; set; }

        /// <summary>
        /// Flag showing the whether this entry is active or not.
        /// </summary>
        [HolochainPropertyName("is_active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// The current version of the entry.
        /// </summary>
        [HolochainPropertyName("version")]
        public int Version { get; set; } = 1;
    }
}
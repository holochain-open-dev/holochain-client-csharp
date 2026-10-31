// The entry models for example 02. Each maps onto a struct in the OASIS hApp's Rust zome: the base constructor names the
// zome and its load / create / update / delete functions, and [HolochainRustFieldName] maps each property onto a field.
// (WEB5 STAR, or HoloNET ORM's HolochainHappGenerator, can generate models like these AND the Rust for them: example 05.)

using NextGenSoftware.Holochain.HoloNET.Client;              // HolochainRustFieldName
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;   // IHoloNETClientAppBase
using NextGenSoftware.Holochain.HoloNET.ORM.Collections;
using NextGenSoftware.Holochain.HoloNET.ORM.Entries;

namespace HoloNET.Examples.ORM
{
    /// <summary>The OASIS hApp's avatar entry.</summary>
    public class Avatar : HoloNETEntryBase
    {
        private const string Zome = "oasis";

        /// <summary>For collections (they connect the entries themselves).</summary>
        public Avatar() : base(Zome, "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", createHoloNETClientConnection: false, autoCallInitialize: false) { }

        /// <summary>Use an existing app connection.</summary>
        public Avatar(IHoloNETClientAppBase client) : base(Zome, "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", client) { }

        [HolochainRustFieldName("id")] public string Id { get; set; }
        [HolochainRustFieldName("username")] public string Username { get; set; }
        [HolochainRustFieldName("email")] public string Email { get; set; }
        [HolochainRustFieldName("first_name")] public string FirstName { get; set; }
        [HolochainRustFieldName("karma")] public long Karma { get; set; }

        /// <summary>A collection of avatars linked from an anchor (the hApp's avatar collection functions).</summary>
        public static HoloNETCollection<Avatar> NewGuild(IHoloNETClientAppBase client) =>
            new HoloNETCollection<Avatar>(Zome, "get_avatar_collection", "add_avatar_to_collection", "remove_avatar_from_collection", client, "batch_update_avatar_collection");
    }

    /// <summary>
    /// The same entry through HoloNETAuditEntryBase, which adds and fills in Id, Version, IsActive, CreatedDate / CreatedBy,
    /// ModifiedDate / ModifiedBy and DeletedDate / DeletedBy (the Rust struct needs the matching fields; the OASIS avatar
    /// stores any extra fields it is given).
    /// </summary>
    public class AuditedAvatar : HoloNETAuditEntryBase
    {
        public AuditedAvatar() : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", createHoloNETClientConnection: false, autoCallInitialize: false) { }
        public AuditedAvatar(IHoloNETClientAppBase client) : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", client) { }

        [HolochainRustFieldName("username")] public string Username { get; set; }
        [HolochainRustFieldName("email")] public string Email { get; set; }
    }
}

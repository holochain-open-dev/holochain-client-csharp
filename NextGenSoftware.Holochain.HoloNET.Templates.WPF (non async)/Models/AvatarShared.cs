using System;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.ORM.Entries;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF.Models
{
    /// <summary>
    /// This example passes in a HoloNETClient instance that is shared with other classes/objects that extend from HoloNETAuditEntryBaseClass/HoloNETEntryBaseClass. Only use this if you have more than one class extending HoloNETAuditEntryBaseClass/HoloNETEntryBaseClass otherwise use the Avatar example.
    /// </summary>
    public class AvatarShared : HoloNETAuditEntryBase
    {
        private string _firstName = "";

        //This constructor will create an empty object with no internal HoloNETClient connection.
        public AvatarShared() : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", false) { }

        //This constructor takes a shared HoloNETClient connection as a param.
        public AvatarShared(HoloNETClient holoNETclient) : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", holoNETclient) { }

        //[HolochainFieldName("first_name")]
        //public string FirstName { get; set; }

        [HolochainRustFieldName("first_name")]
        public string FirstName
        {
            get
            {
                return _firstName;
            }

            set
            {
                if (_firstName != value)
                {
                    // IsChanged = true;
                    _firstName = value;
                }
            }
        }

        [HolochainRustFieldName("last_name")]
        public string LastName { get; set; }

        [HolochainRustFieldName("email")]
        public string Email { get; set; }

        [HolochainRustFieldName("dob")]
        public DateTime DOB { get; set; }
    }
}
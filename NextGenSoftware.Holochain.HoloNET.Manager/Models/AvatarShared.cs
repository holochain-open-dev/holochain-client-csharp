using System;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.Holochain.HoloNET.ORM.Entries;
using NextGenSoftware.Holochain.HoloNET.Manager.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Manager.Models
{
    /// <summary>
    /// This example passes in a HoloNETClient instance that is shared with other classes/objects that extend from HoloNETAuditEntryBaseClass/HoloNETEntryBaseClass. Only use this if you have more than one class extending HoloNETAuditEntryBaseClass/HoloNETEntryBaseClass otherwise use the Avatar example.
    /// </summary>
    public class AvatarShared : HoloNETAuditEntryBase, IAvatarShared
    {
        private string _firstName = "";

        //This constructor will create an empty object with no internal HoloNETClient connection.
        public AvatarShared() : base("oasis", "get_avatar", "create_avatar", "update_avatar", "delete_avatar", false) { }

        //This constructor takes a shared HoloNETClient connection as a param.
        public AvatarShared(IHoloNETClientAppAgent holoNETclient) : base("oasis", "get_avatar", "create_avatar", "update_avatar", "delete_avatar", holoNETclient) { }

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
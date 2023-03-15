
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextGenSoftware.Holochain.HoloNET.Client.TestHarness
{
    /// <summary>
    /// This example creates its own internal instance of the HoloNETClient, you should only use this if you will be extending only one HoloNETAuditEntryBaseClass/HoloNETEntryBaseClass otherwise use the Multiple Class Example.
    /// </summary>
    public class Avatar : HoloNETAuditEntryBaseClass
    {
        public Avatar() : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", false, false, false) { }
        public Avatar(HoloNETConfig holoNETConfig) : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", holoNETConfig, false, false, false) { }

        [HolochainFieldName("first_name")]
        public string FirstName { get; set; }

        [HolochainFieldName("last_name")]
        public string LastName { get; set; }

        [HolochainFieldName("email")]
        public string Email { get; set; }

        [HolochainFieldName("dob")]
        public DateTime DOB { get; set; }

        public override Task<ZomeFunctionCallBackEventArgs> SaveAsync(Dictionary<string, string> customDataKeyValuePair = null, Dictionary<string, bool> holochainPropertiesIsEnabledKeyValuePair = null)
        {
            if (holochainPropertiesIsEnabledKeyValuePair == null)
                holochainPropertiesIsEnabledKeyValuePair = new Dictionary<string, bool>();

            holochainPropertiesIsEnabledKeyValuePair["DOB"] = false;
            holochainPropertiesIsEnabledKeyValuePair["Email"] = false;

            return base.SaveAsync(customDataKeyValuePair, holochainPropertiesIsEnabledKeyValuePair);
        }
    }
}
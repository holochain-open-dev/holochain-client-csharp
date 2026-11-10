using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.Holochain.HoloNET.ORM.Entries;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF.Models
{
    /// <summary>
    /// This example creates its own internal instance of the HoloNETClient, you should only use this if you will be extending only one HoloNETAuditEntryBaseClass/HoloNETEntryBaseClass otherwise use the AvatarShared Example.
    /// </summary>
    public class Avatar : HoloNETAuditEntryBase, IAvatar
    {
        // Example of how you can disable the 3 different audit options (you can of course only disable one of two of them).
        //public Avatar() : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", false, false, false) { }
        //public Avatar(HoloNETConfig holoNETConfig) : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", holoNETConfig, false, false, false) { }
        public Avatar() : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar") { }
        public Avatar(IHoloNETDNA holoNETDNA) : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", true, holoNETDNA) { }


        [HolochainRustFieldName("first_name")]
        public string FirstName { get; set; }

        [HolochainRustFieldName("last_name")]
        public string LastName { get; set; }

        [HolochainRustFieldName("email")]
        public string Email { get; set; }

        [HolochainRustFieldName("dob")]
        public DateTime DOB { get; set; }

        public override Task<ZomeFunctionCallBackEventArgs> SaveAsync(Dictionary<string, string> customDataKeyValuePair = null, Dictionary<string, bool> holochainFieldsIsEnabledKeyValuePair = null, bool cachePropertyInfos = true, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            //Example of how to disable various holochain fields/ properties so the data is omitted from the data sent to the zome function.
            //if (holochainFieldsIsEnabledKeyValuePair == null)
            //    holochainFieldsIsEnabledKeyValuePair = new Dictionary<string, bool>();

            //holochainFieldsIsEnabledKeyValuePair["DOB"] = false;
            //holochainFieldsIsEnabledKeyValuePair["Email"] = false;

            ////Below is an example of how you can send custom data to the zome function:
            //if (customDataKeyValuePair == null)
            //    customDataKeyValuePair = new Dictionary<string, string>();

            //customDataKeyValuePair["dynamic data"] = "dynamic";
            //customDataKeyValuePair["some other data"] = "data";

            return base.SaveAsync(customDataKeyValuePair, holochainFieldsIsEnabledKeyValuePair, cachePropertyInfos, useReflectionToMapKeyValuePairResponseOntoEntryDataObject);
        }

        public override ZomeFunctionCallBackEventArgs Save(Dictionary<string, string> customDataKeyValuePair = null, Dictionary<string, bool> holochainFieldsIsEnabledKeyValuePair = null, bool cachePropertyInfos = true, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            //Example of how to disable various holochain fields/properties so the data is omitted from the data sent to the zome function.
            //if (holochainFieldsIsEnabledKeyValuePair == null)
            //    holochainFieldsIsEnabledKeyValuePair = new Dictionary<string, bool>();

            //holochainFieldsIsEnabledKeyValuePair["DOB"] = false;
            //holochainFieldsIsEnabledKeyValuePair["Email"] = false;

            //Below is an example of how you can send custom data to the zome function:
            //if (customDataKeyValuePair == null)
            //    customDataKeyValuePair = new Dictionary<string, string>();

            //customDataKeyValuePair["dynamic data"] = "dynamic";
            //customDataKeyValuePair["some other data"] = "data";

            return base.Save(customDataKeyValuePair, holochainFieldsIsEnabledKeyValuePair, cachePropertyInfos, useReflectionToMapKeyValuePairResponseOntoEntryDataObject);
        }
    }
}
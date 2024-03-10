using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.ORM.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Manager.Interfaces
{
    public interface IAvatar : IHoloNETAuditEntryBase
    {
        DateTime DOB { get; set; }
        string Email { get; set; }  
        string FirstName { get; set; }
        string LastName { get; set; }

        ZomeFunctionCallBackEventArgs Save(Dictionary<string, string> customDataKeyValuePair = null, Dictionary<string, bool> holochainFieldsIsEnabledKeyValuePair = null, bool cachePropertyInfos = true, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ZomeFunctionCallBackEventArgs> SaveAsync(Dictionary<string, string> customDataKeyValuePair = null, Dictionary<string, bool> holochainFieldsIsEnabledKeyValuePair = null, bool cachePropertyInfos = true, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
    }
}
using System;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public interface IHoloNETAuditEntryBase : IHoloNETEntryBase
    {
        List<HoloNETAuditEntry> AuditEntries { get; set; }
        string CreatedBy { get; set; }
        DateTime CreatedDate { get; set; }
        string DeletedBy { get; set; }
        DateTime DeletedDate { get; set; }
        Guid Id { get; set; }
        bool IsActive { get; set; }
        bool IsAuditAgentCreateModifyDeleteFieldsEnabled { get; set; }
        bool IsAuditTrackingEnabled { get; set; }
        bool IsVersionTrackingEnabled { get; set; }
        string ModifiedBy { get; set; }
        DateTime ModifiedDate { get; set; }
        int Version { get; set; }

        //ZomeFunctionCallBackEventArgs Delete();
        //ZomeFunctionCallBackEventArgs Delete(string entryHash);
        //Task<ZomeFunctionCallBackEventArgs> DeleteAsync();
        //Task<ZomeFunctionCallBackEventArgs> DeleteAsync(string entryHash);
        //ZomeFunctionCallBackEventArgs Save(Dictionary<string, string> customDataKeyValuePair = null, Dictionary<string, bool> holochainFieldsIsEnabledKeyValuePair = null, bool cachePropertyInfos = true);
        //Task<ZomeFunctionCallBackEventArgs> SaveAsync(Dictionary<string, string> customDataKeyValuePair = null, Dictionary<string, bool> holochainFieldsIsEnabledKeyValuePair = null, bool cachePropertyInfos = true);
    }
}
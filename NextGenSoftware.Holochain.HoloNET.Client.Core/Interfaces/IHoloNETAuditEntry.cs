using System;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public interface IHoloNETAuditEntry
    {
        DateTime DateTime { get; set; }
        string EntryHash { get; set; }
        HoloNETAuditEntryType Type { get; set; }
    }
}
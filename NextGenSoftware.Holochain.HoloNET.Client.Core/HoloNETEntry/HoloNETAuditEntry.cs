using System;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class HoloNETAuditEntry
    {
        public string EntryHash { get; set; }
        public DateTime DateTime { get; set; }
        public HoloNETAuditEntryType Type { get; set; } 
    }
}

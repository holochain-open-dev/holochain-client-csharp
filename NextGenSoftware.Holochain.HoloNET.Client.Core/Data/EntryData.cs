

using System;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class EntryData
    {
        public string Author { get; set; }
        public string EntryHash { get; set; }
        public string PreviousHash { get; set; }
        public long TimestampRust { get; set; }
        public DateTime Timestamp { get; set; }
        public string Type { get; set; }
        public int ActionSequence { get; set; }
        public string OriginalActionAddress { get; set; }
        public string OriginalEntryAddress { get; set; }
        public byte[] Bytes { get; set; }
        public string BytesString { get; set; }
        public Dictionary<string, object> Entry { get; set; }
        public dynamic EntryDataObject { get; set; }
    }
}
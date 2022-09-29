

using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class EntryData
    {
        public byte[] Bytes { get; set; }
        public string BytesString { get; set; }
        public Dictionary<string, object> Entry { get; set; }    
    }
}
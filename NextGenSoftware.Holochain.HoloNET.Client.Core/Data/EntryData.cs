

using System;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class EntryData
    {
        public string Author { get; set; }
        public string Hash { get; set; } //the true EntryHash (is Hash in the data returned from the Holochain Conductor
        public string EntryHash { get; set; } //Have no idea what this is?! lol
        public string PreviousHash { get; set; } //The previous Hash.
        public string Signature { get; set; } //Not sure what this is?
        public long Timestamp { get; set; }
        public DateTime DateTime { get; set; } //Converted from the Unix timestamp above (returned from the Holochain Conductor).
        public string Type { get; set; } //Create/Update/Delete
        public string EntryType { get; set; } //App/Not sure what other types there are?
        public int ActionSequence { get; set; }
        public string OriginalActionAddress { get; set; } //Is the original Hash.
        public string OriginalEntryAddress { get; set; } //Is the original EntryHash.
        public byte[] Bytes { get; set; }
        public string BytesString { get; set; }
        public Dictionary<string, object> Entry { get; set; }
        public dynamic EntryDataObject { get; set; }
    }
}
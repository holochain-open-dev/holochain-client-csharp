﻿
using System;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class EntryData
    {
        /// <summary>
        /// The author of the entry.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// The true EntryHash (is Hash in the meta data returned from the Holochain Conductor)
        /// </summary>
        public string Hash { get; set; } 

        /// <summary>
        /// Unknown - still investigating...
        /// </summary>
        public string EntryHash { get; set; } 

        /// <summary>
        /// The previous Hash (EntryHash).
        /// </summary>
        public string PreviousHash { get; set; }

        /// <summary>
        /// The signature of the entry.
        /// </summary>
        public string Signature { get; set; } //Not sure what this is?

        /// <summary>
        /// The Unix timestamp (returned from the Holochain Conductor)
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// Converted from the Unix timestamp above (returned from the Holochain Conductor).
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Create/Update/Delete
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// //App/Not sure what other types there are?
        /// </summary>
        public string EntryType { get; set; } 

        /// <summary>
        /// The Action Sequence for this entry.
        /// </summary>
        public int ActionSequence { get; set; }

        /// <summary>
        /// Is the original Hash for the entry.
        /// </summary>
        public string OriginalActionAddress { get; set; }

        /// <summary>
        /// Is the original EntryHash for the entry.
        /// </summary>
        public string OriginalEntryAddress { get; set; }

        /// <summary>
        /// The raw bytes returned from the Holochain Conductor for the entry.
        /// </summary>
        public byte[] Bytes { get; set; }

        /// <summary>
        /// The raw bytes returned from the Holochain Conductor for the entry formatted as a string making logging etc easier.
        /// </summary>
        public string BytesString { get; set; }

        /// <summary>
        /// A key/value pair/dictionary containing the entry data itself.
        /// </summary>
        public Dictionary<string, object> Entry { get; set; }

        /// <summary>
        /// A dynamic object constructed from the entry data.
        /// </summary>
        public dynamic EntryDataObject { get; set; }
    }
}
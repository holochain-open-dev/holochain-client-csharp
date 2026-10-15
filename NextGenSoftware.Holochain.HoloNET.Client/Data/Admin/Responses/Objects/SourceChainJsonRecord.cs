
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class SourceChainJsonRecord
    {
        // Holochain 0.7 SourceChainDumpRecord { signature, action_address, action, entry }: the signature is 64 bytes,
        // and action / entry were missing.
        //[Key("signature")]
        //public Signature[] signature { get; set; }

        [Key("signature")]
        public byte[] signature { get; set; }

        [Key("action_address")]
        public byte[] action_address { get; set; }

        /// <summary>The action as decoded msgpack (its "type" says which kind).</summary>
        [Key("action")]
        public object action { get; set; }

        /// <summary>The entry (tagged by "entry_type"), or null.</summary>
        [Key("entry")]
        public object entry { get; set; }
    }
}
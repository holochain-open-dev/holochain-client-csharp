
using MessagePack;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// One entry in the errors array returned by enable_app.
    /// Wire format: [CellId, error_string] where CellId is [DnaHash, AgentPubKey].
    /// </summary>
    [MessagePackObject]
    public class EnableAppError
    {
        [Key(0)]
        public byte[][] CellId { get; set; }

        [Key(1)]
        public string Error { get; set; }
    }

    [MessagePackObject]
    public class EnableAppResponseDetails
    {
        [Key("app")]
        public AppInfo app { get; set; }

        [Key("errors")]
        public List<EnableAppError> errors { get; set; }
    }
}

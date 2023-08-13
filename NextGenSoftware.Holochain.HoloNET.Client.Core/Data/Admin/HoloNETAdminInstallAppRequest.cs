
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class HoloNETAdminInstallAppRequest
    {
        [Key("agent_key")]
        public byte[] agent_key { get; set; }

        [Key("installed_app_id")]
        public string installed_app_id { get; set; }

        [Key("membrane_proofs")]
        public byte[] membrane_proofs { get; set; }

        [Key("network_seed")]
        public byte[] network_seed { get; set; }
    }
}
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Core
{
    [MessagePackObject]
    public struct HoloNETDataAppInfoCall
    {
        [Key("installed_app_id")]
        public string installed_app_id { get; set; }

        [Key("cell_data")]
        public InstalledCell[] cell_data { get; set; }

        [Key("status")]
        public InstalledAppInfoStatus status { get; set; }
    }
}
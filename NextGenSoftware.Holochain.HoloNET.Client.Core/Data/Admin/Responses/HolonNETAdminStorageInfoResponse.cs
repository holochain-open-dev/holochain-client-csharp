
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public struct HolonNETAdminStorageInfoResponse
    {
        [Key("blobs")]
        public DnaStorageBlob[] blobs { get; set; }
    }
}
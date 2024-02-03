
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class StorageInfoResponse
    {
        [Key("blobs")]
        public DnaStorageBlob[] blobs { get; set; }
    }
}
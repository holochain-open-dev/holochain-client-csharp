
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class DnaStorageInfo
    {
        // authored_data_size / authored_data_size_on_disk removed in Holochain 0.7.0:
        // source-chain data now lives in the per-DNA DHT database and is counted in dht_data_size.
        // cache_data_size / cache_data_size_on_disk also removed in 0.7.0.

        [Key("dht_data_size")]
        public int dht_data_size { get; set; }

        [Key("dht_data_size_on_disk")]
        public int dht_data_size_on_disk { get; set; }

        [Key("used_by")]
        public string used_by { get; set; } //InstalledAppId
    }
}

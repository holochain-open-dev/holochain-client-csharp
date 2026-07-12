
using MessagePack;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    /// <summary>
    /// Mirrors the Holochain GrantedFunctions tagged union.
    /// Wire format (MessagePack map): {"All": null} or {"Listed": [[zome, fn], ...]}
    /// </summary>
    [MessagePackObject]
    public class GrantedFunctions
    {
        // String-keyed so MessagePack serialises "All"/"Listed" rather than int 0/1.
        [Key("functions")]
        public Dictionary<string, object> Functions { get; set; }

        public static GrantedFunctions All()
        {
            return new GrantedFunctions
            {
                Functions = new Dictionary<string, object> { { "All", null } }
            };
        }

        public static GrantedFunctions Listed(List<(string zome, string fn)> grants)
        {
            var list = new List<object[]>();
            foreach (var (zome, fn) in grants)
                list.Add(new object[] { zome, fn });

            return new GrantedFunctions
            {
                Functions = new Dictionary<string, object> { { "Listed", list } }
            };
        }
    }
}

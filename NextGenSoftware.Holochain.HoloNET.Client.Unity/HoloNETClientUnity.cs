
using NextGenSoftware.Holochain.HoloNET.Client.Core;
using NextGenSoftware.WebSocket;

namespace NextGenSoftware.Holochain.HoloNET.Client.Unity
{
    public class HoloNETClientUnity : HoloNETClient
    {
        public HoloNETClientUnity(string holochainConductorURI, ILogger logger, HolochainVersion version = HolochainVersion.RSM) : base(holochainConductorURI, logger, version)
        {

        }

        //public HoloNETClientUnity(string holochainConductorURI, HolochainVersion version = HolochainVersion.RSM) : base(holochainConductorURI, new NLogger(), version)
        //{
        //    //TODO: Add Unity Compat Logger Here (hopefully the Unity NLogger Download/Asset I found)
        //    // this.Logger = new NLogger();
        //    //this.Logger = new DumbyLogger();
        //}
    }
}

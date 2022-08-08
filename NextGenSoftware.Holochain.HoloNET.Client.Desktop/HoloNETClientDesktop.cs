
using NextGenSoftware.Holochain.HoloNET.Client.Core;
using NextGenSoftware.WebSocket;

namespace NextGenSoftware.Holochain.HoloNET.Client.Desktop
{
    public class HoloNETClientDesktop : HoloNETClient
    {
        public HoloNETClientDesktop(string holochainConductorURI, ILogger logger, HolochainVersion version = HolochainVersion.RSM) : base(holochainConductorURI, logger, version)
        {

        }

        public HoloNETClientDesktop(string holochainConductorURI, HolochainVersion version = HolochainVersion.RSM) : base(holochainConductorURI, new NLogger(), version)
        {

        }
    }
}

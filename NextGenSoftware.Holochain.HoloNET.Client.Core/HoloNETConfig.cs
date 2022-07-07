
using NextGenSoftware.WebSocket;

namespace NextGenSoftware.Holochain.HoloNET.Client.Core
{
    public class HoloNETConfig
    {
        //public string AgentPubKey { get; set; } = "AgentPubKey(uhCAk3R-gBvP0KUclYxiHo-j0g29Kv3D-mF3aE7LNKhj5Lyf4qchy)";
        //public string HoloHash { get; set; } = "DnaHash(uhC0kVlEK-_3ODBfCW7p2uz9RCp_lfqBlJn7eEvJVIeInBJSI5sfR)";
        //public string AgentPubKey { get; set; } = "uhCAk3R-gBvP0KUclYxiHo-j0g29Kv3D-mF3aE7LNKhj5Lyf4qchy";
        //public string HoloHash { get; set; } = "uhC0kVlEK-_3ODBfCW7p2uz9RCp_lfqBlJn7eEvJVIeInBJSI5sfR";
        public string AgentPubKey { get; set; } = "hCAkto0UVRCevjag3nce/k7v3TwWbF17rSlpDpBsxN7AVOyFCN7A";
        public string DnaHash { get; set; } = "hC0kr7hZGm8V/KwtQzRR8yHPOZqvM7WRDcVlk1oz8HAEC5hovS7s"; //_ =/   - = + and drop the u prefix.
        public string FullPathToExternalHolochainConductor { get; set; }
        public string FullPathToHolochainAppDNA { get; set; }
        public int SecondsToWaitForHolochainConductorToStart { get; set; } = 5;
        public bool AutoStartConductor { get; set; } = true;
        public bool AutoShutdownConductor { get; set; } = true;
        public ErrorHandlingBehaviour ErrorHandlingBehaviour { get; set; } = ErrorHandlingBehaviour.OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent;
    }
}
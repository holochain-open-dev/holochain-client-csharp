
namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class HoloNETConfig
    {
        //public string AgentPubKey { get; set; } = "uhCAkto0UVRCevjag3nce_k7v3TwWbF17rSlpDpBsxN7AVOyFCN7A";
        //public string DnaHash { get; set; } = "uhC0kr7hZGm8V_KwtQzRR8yHPOZqvM7WRDcVlk1oz8HAEC5hovS7s";
        public string AgentPubKey { get; set; } = "";
        public string DnaHash { get; set; } = "";
        public string FullPathToHapp { get; set; }
        public string FullPathToExternalHolochainConductor { get; set; }
        public string FullPathToHolochainAppDNA { get; set; }
        public int SecondsToWaitForHolochainConductorToStart { get; set; } = 5;
        public bool AutoStartConductor { get; set; } = true;
        public bool AutoShutdownConductor { get; set; } = true;
        public ErrorHandlingBehaviour ErrorHandlingBehaviour { get; set; } = ErrorHandlingBehaviour.OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent;
    }
}
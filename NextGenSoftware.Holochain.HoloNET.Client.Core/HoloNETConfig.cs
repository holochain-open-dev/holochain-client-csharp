
using NextGenSoftware.Logging;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class HoloNETConfig
    {
        public string AgentPubKey { get; set; } = "";
        public string DnaHash { get; set; } = "";
        public string FullPathToHappFolder { get; set; }
        public string FullPathToExternalHolochainConductorBinary { get; set; } //= "HolochainBinaries\\holochain.exe";
        public string FullPathToExternalHCToolBinary { get; set; } //= "HolochainBinaries\\hc.exe";
        //public string FullPathToHolochainAppDNA { get; set; }
        public int SecondsToWaitForHolochainConductorToStart { get; set; } = 7;
        public bool AutoStartHolochainConductor { get; set; } = true;
        public bool ShowHolochainConductorWindow { get; set; } = false;
        public bool AutoShutdownHolochainConductor { get; set; } = true;
        public bool ShutDownALLHolochainConductors { get; set; } = false;

        public LoggingMode LoggingMode
        {
            get
            {
                return LogConfig.LoggingMode;
            }
            set
            {
                LogConfig.LoggingMode = value;
            }
        }
        public ErrorHandlingBehaviour ErrorHandlingBehaviour { get; set; } = ErrorHandlingBehaviour.OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent;
    }
}
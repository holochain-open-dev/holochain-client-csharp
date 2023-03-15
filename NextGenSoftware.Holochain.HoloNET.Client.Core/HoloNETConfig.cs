
using NextGenSoftware.Logging;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class HoloNETConfig
    {
        /// <summary>
        /// The AgentPubKey to use for Zome calls. If this is not set then HoloNET will automatically retrieve this along with the DnaHash after it connects (if the Connect method defaults are not overriden).
        /// </summary>
        public string AgentPubKey { get; set; } = "";

        /// <summary>
        /// The DnaHash to use for Zome calls. If this is not set then HoloNET will automatically retrieve this along with the AgentPubKey after it connects (if the Connect method defaults are not overriden).
        /// </summary>
        public string DnaHash { get; set; } = "";

        /// <summary>
        /// The full path to the root of the hApp that HoloNET will start the Holochain Conductor (currenty uses hc.exe) with and then make zome calls to.
        /// </summary>
        public string FullPathToRootHappFolder { get; set; }

        /// <summary>
        /// The full path to the compiled hApp that HoloNET will start the Holochain Conductor (currenty uses hc.exe) with and then make zome calls to.
        /// </summary>
        public string FullPathToCompiledHappFolder { get; set; }

        /// <summary>
        /// Tells HoloNET how to auto-start the Holochain Conductor. It can be one of the following values: `UseExternal` - Will use the hc.exe specified in the `FullPathToExternalHCToolBinary` property if `HolochainConductorToUse` property is set to `Hc`. It will use the holochain.exe specified in the `FullPathToExternalHolochainConductorBinary` property if `HolochainConductorToUse` property is set to `Holochain`. If `HolochainConductorMode` is set to `UseEmbedded` then it will use the embdedded/integrated hc.exe/holochain.exe if the app is using the [NextGenSoftware.Holochain.HoloNET.Client.Embedded](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.Client.Embedded) package, otherwise it will throw an exception. Finally, if `HolochainConductorMode` is set to `UseSystemGlobal` (default), then it will automatically use the installed version of hc.exe & holochain.exe on the target machine.
        /// </summary>
        public HolochainConductorModeEnum HolochainConductorMode { get; set; } = HolochainConductorModeEnum.UseSystemGlobal;

        /// <summary>
        /// This is the Holochain Conductor to use for the auto-start Holochain Conductor feature. It can be either `Holochain` or `Hc`.
        /// </summary>
        public string FullPathToExternalHolochainConductorBinary { get; set; } //= "HolochainBinaries\\holochain.exe";

        /// <summary>
        /// The full path to the Holochain Conductor exe (holochain.exe) that HoloNET will auto-start if `HolochainConductorToUse` is set to `Holochain`.
        /// </summary>
        public string FullPathToExternalHCToolBinary { get; set; } //= "HolochainBinaries\\hc.exe";

        //public string FullPathToHolochainAppDNA { get; set; }

        /// <summary>
        /// The seconds to wait for the Holochain Conductor to start before attempting to connect to it.
        /// </summary>
        public int SecondsToWaitForHolochainConductorToStart { get; set; } = 7;

        /// <summary>
        /// Set this to true if you with HoloNET to auto-start the Holochain Conductor defined in the `FullPathToExternalHolochainConductorBinary parameter if `HolochainConductorToUse` is `Holochain`, otherwise if it`s `Hc` then it will use `FullPathToExternalHCToolBinary`. Default is true.
        /// </summary>
        public bool AutoStartHolochainConductor { get; set; } = true;

        /// <summary>
        /// Set this to true if you wish HoloNET to show the Holochain Conductor window whilst it is starting it (will be left open until the conductor is automatically shutdown again when HoloNET disconects if `AutoShutdownHolochainConductor` is true.)
        /// </summary>
        public bool ShowHolochainConductorWindow { get; set; } = false;

        /// <summary>
        /// Set this to true if you wish HoloNET to auto-shutdown the Holochain Conductor after it disconnects. Default is true.
        /// </summary>
        public bool AutoShutdownHolochainConductor { get; set; } = true;

        /// <summary>
        /// Set this to true if you wish HoloNET to auto-shutdown ALL Holochain Conductors after it disconnects. Default is false. Set this to true if you wish to make sure there are none left running to prevent memory leaks. You can also of course manually call the ShutDownAllConductors if you wish.
        /// </summary>
        public bool ShutDownALLHolochainConductors { get; set; } = false;

        /// <summary>
        /// This is the Holochain Conductor to use for the auto-start Holochain Conductor feature. It can be either `Holochain` or `Hc`.
        /// </summary>
        public HolochainConductorEnum HolochainConductorToUse { get; set; } = HolochainConductorEnum.HcDevTool; //Will soon default to HolochainProductionConductor once figured out how to pass a hApp to it! ;-)

        /// <summary>
        /// Set this to true if you wish HoloNET to allow only ONE Holochain Conductor to run at a time. The default is false.
        /// </summary>
        public bool OnlyAllowOneHolochainConductorToRunAtATime { get; set; } = false;

        /// <summary>
        /// This passes through to the static LogConfig.LoggingMode property in [NextGenSoftware.Logging](https://www.nuget.org/packages/NextGenSoftware.Logging) package. It can be either `WarningsErrorsInfoAndDebug`, `WarningsErrorsAndInfo`, `WarningsAndErrors` or `ErrorsOnly`.
        /// </summary>
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

        /// <summary>
        /// An enum that specifies what to do when anm error occurs. The options are: `AlwaysThrowExceptionOnError`, `OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent` & `NeverThrowExceptions`). The default is `OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent` meaning it will only throw an error if the `OnError` event has not been subscribed to. This delegates error handling to the caller. If no event has been subscribed then HoloNETClient will throw an error. `AlwaysThrowExceptionOnError` will always throw an error even if the `OnError` event has been subscribed to. The `NeverThrowException` enum option will never throw an error even if the `OnError` event has not been subscribed to. Regardless of what enum is selected, the error will always be logged using whatever ILogger`s have been injected into the constructor or set on the static Logging.Loggers property.
        /// </summary>
        public ErrorHandlingBehaviour ErrorHandlingBehaviour { get; set; } = ErrorHandlingBehaviour.OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent;
    }
}
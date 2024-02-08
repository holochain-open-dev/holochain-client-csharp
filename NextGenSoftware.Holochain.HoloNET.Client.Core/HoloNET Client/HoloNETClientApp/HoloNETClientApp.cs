using System.Collections.Generic;
using NextGenSoftware.Logging;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public partial class HoloNETClientApp : HoloNETClientAppBase
    {
        /// <summary>
        /// This constructor uses the built-in DefaultLogger and the settings contained in the HoloNETDNA.
        /// </summary>
        /// <param name="holoNETDNA">The HoloNETDNA you wish to use for this connection (optional). If this is not passed in then it will use the default HoloNETDNA defined in the HoloNETDNA property.</param>
        public HoloNETClientApp(HoloNETDNA holoNETDNA = null) : base(holoNETDNA)
        {
            ////if (holoNETDNA == null)
            ////    HoloNETDNA = new HoloNETDNA() { AutoStartHolochainConductor = false, AutoShutdownHolochainConductor = false };

            //if (holoNETDNA == null)
            //{
            //    //Will load the HoloNETDNA from disk if there is a HoloNET_DNA.json file and then default to not starting or shutting down the conductor (because the admin takes care of this).
            //    HoloNETDNA.AutoStartHolochainConductor = false;
            //    HoloNETDNA.AutoShutdownHolochainConductor = false;
            //}
            //else
            //    HoloNETDNA = holoNETDNA;
        }

        /// <summary>
        /// This constructor allows you to inject in (DI) your own implementation (logProvider) of the ILogProvider interface, which will be added to the Logger.LogProviders collection. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA.
        /// </summary>
        /// <param name="logProvider">The implementation of the ILogProvider interface (custom logProvider).</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom logger injected in.</param>
        /// <param name="holoNETDNA">The HoloNETDNA you wish to use for this connection (optional). If this is not passed in then it will use the default HoloNETDNA defined in the HoloNETDNA property.</param>
        public HoloNETClientApp(ILogProvider logProvider, bool alsoUseDefaultLogger = false, HoloNETDNA holoNETDNA = null) : base(logProvider, alsoUseDefaultLogger, holoNETDNA)
        {
            ////if (holoNETDNA == null)
            ////    HoloNETDNA = new HoloNETDNA() { AutoStartHolochainConductor = false, AutoShutdownHolochainConductor = false };

            //if (holoNETDNA == null)
            //{
            //    //Will load the HoloNETDNA from disk if there is a HoloNET_DNA.json file and then default to not starting or shutting down the conductor (because the admin takes care of this).
            //    HoloNETDNA.AutoStartHolochainConductor = false;
            //    HoloNETDNA.AutoShutdownHolochainConductor = false;
            //}
            //else
            //    HoloNETDNA = holoNETDNA;
        }

        /// <summary>
        /// This constructor allows you to inject in (DI) multiple implementations (logProviders) of the ILogProvider interface, which will be added to the Logger.LogProviders collection. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA.
        /// </summary>
        /// <param name="logProviders">The implementations of the ILogProvider interface (custom logProviders).</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom loggers injected in.</param>
        /// <param name="holoNETDNA">The HoloNETDNA you wish to use for this connection (optional). If this is not passed in then it will use the default HoloNETDNA defined in the HoloNETDNA property.</param>
        public HoloNETClientApp(IEnumerable<ILogProvider> logProviders, bool alsoUseDefaultLogger = false, HoloNETDNA holoNETDNA = null) : base(logProviders, alsoUseDefaultLogger, holoNETDNA)
        {
            ////if (holoNETDNA == null)
            ////    HoloNETDNA = new HoloNETDNA() { AutoStartHolochainConductor = false, AutoShutdownHolochainConductor = false };
            ////else
            ////    HoloNETDNA = holoNETDNA;

            //if (holoNETDNA == null)
            //{
            //    //Will load the HoloNETDNA from disk if there is a HoloNET_DNA.json file and then default to not starting or shutting down the conductor (because the admin takes care of this).
            //    HoloNETDNA.AutoStartHolochainConductor = false;
            //    HoloNETDNA.AutoShutdownHolochainConductor= false;
            //}
            //else
            //    HoloNETDNA = holoNETDNA;
        }

        /// <summary>
        /// This constructor allows you to inject in (DI) a Logger instance (which could contain multiple logProviders). This will then override the default Logger found on the Logger property. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA.
        /// </summary>
        /// <param name="logger">The logger instance to use.</param>
        /// <param name="holoNETDNA">The HoloNETDNA you wish to use for this connection (optional). If this is not passed in then it will use the default HoloNETDNA defined in the HoloNETDNA property.</param>
        public HoloNETClientApp(Logger logger, HoloNETDNA holoNETDNA = null) : base(logger, holoNETDNA)
        {
            ////if (holoNETDNA == null)
            ////   HoloNETDNA = new HoloNETDNA() { AutoStartHolochainConductor = false, AutoShutdownHolochainConductor = false };

            //if (holoNETDNA == null)
            //{
            //    //Will load the HoloNETDNA from disk if there is a HoloNET_DNA.json file and then default to not starting or shutting down the conductor (because the admin takes care of this).
            //    HoloNETDNA.AutoStartHolochainConductor = false;
            //    HoloNETDNA.AutoShutdownHolochainConductor = false;
            //}
            //else
            //    HoloNETDNA = holoNETDNA;
        }
    }
}
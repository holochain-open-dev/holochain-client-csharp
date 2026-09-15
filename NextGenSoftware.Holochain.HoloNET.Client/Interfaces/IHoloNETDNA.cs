using System;
using NextGenSoftware.Logging;

namespace NextGenSoftware.Holochain.HoloNET.Client.Interfaces
{
    public interface IHoloNETDNA
    {
        bool InsertExtraNewLineAfterLogMessage { get; set; }
        int IndentLogMessagesBy { get; set; } 
        string AgentPubKey { get; set; }
        bool AutoShutdownHolochainConductor { get; set; }
        bool AutoStartHolochainConductor { get; set; }
        byte[][] CellId { get; set; }
        ConsoleColor DebugColour { get; set; }
        string DnaHash { get; set; }
        EnforceRequestToResponseIdMatchingBehaviour EnforceRequestToResponseIdMatchingBehaviour { get; set; }
        ConsoleColor ErrorColour { get; set; }
        ErrorHandlingBehaviour ErrorHandlingBehaviour { get; set; }
        string FullPathToCompiledHappFolder { get; set; }
        string FullPathToExternalHCToolBinary { get; set; }
        string FullPathToExternalHolochainConductorBinary { get; set; }
        string FullPathToRootHappFolder { get; set; }
        string HolochainConductorAdminURI { get; set; }
        string HolochainConductorAppAgentURI { get; set; }
        string HolochainConductorConfigPath { get; set; }
        HolochainConductorModeEnum HolochainConductorMode { get; set; }
        HolochainConductorEnum HolochainConductorToUse { get; set; }
        ConsoleColor InfoColour { get; set; }
        string InstalledAppId { get; set; }
        string LogFileName { get; set; }
        LoggingMode FileLoggingMode { get; set; }
        LoggingMode ConsoleLoggingMode { get; set; }
        string LogPath { get; set; }
        bool LogToConsole { get; set; }
        bool LogToFile { get; set; }
        int MaxLogFileSize { get; set; }
        int NumberOfRetriesToLogToFile { get; set; }
        bool OnlyAllowOneHolochainConductorToRunAtATime { get; set; }
        int RetryLoggingToFileEverySeconds { get; set; }
        int SecondsToWaitForHolochainConductorToStart { get; set; }
        bool ShowColouredLogs { get; set; }
        bool ShowHolochainConductorWindow { get; set; }
        bool ShutDownALLHolochainConductors { get; set; }
        ConsoleColor WarningColour { get; set; }

        /// <summary>
        /// Network configuration (Holochain 0.6.1 ConductorConfig.network). Currently used by
        /// HoloNET to set the default RequestTimeoutS used for zome calls / websocket requests.
        /// </summary>
        NetworkConfig NetworkConfig { get; set; }

        /// <summary>
        /// Kitsune2 networking sub-fields of ConductorConfig.network not already covered by
        /// NetworkConfig (bootstrap/relay URLs, target_arc_factor, advanced kitsune2 JSON;
        /// signal_url and webrtc_config removed in Holochain 0.7.0). Not yet wired through to any websocket/conductor call.
        /// </summary>
        Kitsune2Config Kitsune2Config { get; set; }

        /// <summary>
        /// QUIC transport configuration. NOT VERIFIED against any real Holochain conductor-facing
        /// config - kept as a placeholder only; has no effect on conductor behaviour.
        /// </summary>
        QUICConfig QUICConfig { get; set; }

        /// <summary>
        /// Keystore connection configuration, mirroring holochain_conductor_api's KeystoreConfig.
        /// Not yet wired through to actual conductor config generation/admin API - out of scope here.
        /// </summary>
        KeystoreConfig KeystoreConfig { get; set; }

        /// <summary>
        /// WASM-related configuration. NOT VERIFIED against any real Holochain conductor-facing
        /// config - kept as a placeholder only; has no effect on conductor behaviour.
        /// </summary>
        WASMConfig WASMConfig { get; set; }

        /// <summary>
        /// HoloNET client-side cache configuration (HoloNET-specific concept, not a Rust mirror).
        /// Not yet consumed by any caching implementation in HoloNET.
        /// </summary>
        CacheConfig CacheConfig { get; set; }
    }
}
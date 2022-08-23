//using NextGenSoftware.Logging;

//namespace NextGenSoftware.Holochain.HoloNET.Client.Embedded
//{
//    public class HoloNETClientEmbedded
//    {
//        public HoloNETClientEmbedded(string holochainConductorURI, bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red)
//        {
//            Logging.Logging.Loggers.Add(new DefaultLogger(logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour));
//            Init(holochainConductorURI);
//        }

//        public HoloNETClientEmbedded(string holochainConductorURI, ILogger logger)
//        {
//            Logging.Logging.Loggers.Add(logger);
//            Init(holochainConductorURI);
//        }

//        public HoloNETClientEmbedded(string holochainConductorURI, IEnumerable<ILogger> loggers)
//        {
//            Logging.Logging.Loggers = new List<ILogger>(loggers);
//            Init(holochainConductorURI);
//        }
//    }
//}
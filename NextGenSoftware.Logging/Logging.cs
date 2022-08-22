
using System;
using System.Collections.Generic;

namespace NextGenSoftware.Logging
{
    public static class Logging
    {
        public static List<ILogger> Loggers { get; set; } = new List<ILogger>();

        public static void Log(string message, LogType type)
        {
            foreach (ILogger logger in Loggers)
                logger.Log(message, type);
        }

        public static void Log(string message, LogType type, ConsoleColor consoleColour, bool showWorkingAnimation = false)
        {
            foreach (ILogger logger in Loggers)
                logger.Log(message, type, consoleColour, showWorkingAnimation);
        }

        public static void Log(string message, LogType type, bool showWorkingAnimation = false)
        {
            foreach (ILogger logger in Loggers)
                logger.Log(message, type, showWorkingAnimation);
        }

        public static bool ContinueLogging(LogType type)
        {
            if (type == LogType.Info && !(LogConfig.LoggingMode == LoggingMode.WarningsErrorsInfoAndDebug || LogConfig.LoggingMode == LoggingMode.WarningsErrorsAndInfo))
                return false;

            if (type == LogType.Debug && LogConfig.LoggingMode != LoggingMode.WarningsErrorsInfoAndDebug)
                return false;

            if (type == LogType.Warning && !(LogConfig.LoggingMode == LoggingMode.WarningsErrorsInfoAndDebug || LogConfig.LoggingMode == LoggingMode.WarningsErrorsAndInfo || LogConfig.LoggingMode == LoggingMode.WarningsAndErrors))
                return false;

            return true;
        }
    }
}

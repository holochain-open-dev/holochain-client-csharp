
namespace NextGenSoftware.Logging
{
    public static class LogProcessor
    {
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

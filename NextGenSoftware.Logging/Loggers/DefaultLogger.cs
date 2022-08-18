
using System;
using System.IO;

namespace NextGenSoftware.Logging
{
    public class DefaultLogger : ILogger
    {
        public DefaultLogger(bool logToConsole = true, bool logToFile = true, string pathToLogFile = "Logs", string logFileName = "Log.txt", bool addAdditionalSpaceAfterEachLogEntry = false)
        {
            LogDirectory = pathToLogFile;
            LogFileName = logFileName;
            LogToConsole = logToConsole;
            LogToFile = logToFile;
            AddAdditionalSpaceAfterEachLogEntry = addAdditionalSpaceAfterEachLogEntry;
        }

        public string LogDirectory { get; set; }
        public string LogFileName { get; set; }
        public bool LogToConsole { get; set; }
        public bool LogToFile { get; set; }
        public bool AddAdditionalSpaceAfterEachLogEntry { get; set; } = false;

        public void Log(string message, LogType type)
        {
            if (LogProcessor.ContinueLogging(type))
            {
                string logMessage = $"{DateTime.Now} {type}: {message}";

                if (AddAdditionalSpaceAfterEachLogEntry)
                    logMessage = String.Concat(logMessage, "\n");

                if (LogToConsole)
                    Console.WriteLine(logMessage);

                if (LogToFile)
                {
                    if (!string.IsNullOrEmpty(LogDirectory) && !Directory.Exists(LogDirectory))
                        Directory.CreateDirectory(LogDirectory);

                    if (!AddAdditionalSpaceAfterEachLogEntry)
                        logMessage = String.Concat(logMessage, "\n");

                    File.AppendAllText(String.Concat(LogDirectory, "\\", LogFileName), logMessage);
                }
            }
        }
    }
}
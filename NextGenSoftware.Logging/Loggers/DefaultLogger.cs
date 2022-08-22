using System;
using System.IO;
using NextGenSoftware.CLI.Engine;

namespace NextGenSoftware.Logging
{
    public class DefaultLogger : ILogger
    {
        public DefaultLogger(bool logToConsole = true, bool logToFile = true, string pathToLogFile = "Logs", string logFileName = "Log.txt", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red)
        {
            LogDirectory = pathToLogFile;
            LogFileName = logFileName;
            LogToConsole = logToConsole;
            LogToFile = logToFile;
            AddAdditionalSpaceAfterEachLogEntry = addAdditionalSpaceAfterEachLogEntry;
            ShowColouredLogs = showColouredLogs;
            DebugColour = debugColour;
            InfoColour = infoColour;
            ErrorColour = errorColour;
            WarningColour = warningColour;
        }

        public string LogDirectory { get; set; }
        public string LogFileName { get; set; }
        public bool LogToConsole { get; set; }
        public bool LogToFile { get; set; }
        public bool AddAdditionalSpaceAfterEachLogEntry { get; set; } = false;
        public static bool ShowColouredLogs { get; set; } = true;
        public static ConsoleColor DebugColour { get; set; } = ConsoleColor.White;
        public static ConsoleColor InfoColour { get; set; } = ConsoleColor.Green;
        public static ConsoleColor WarningColour { get; set; } = ConsoleColor.Yellow;
        public static ConsoleColor ErrorColour { get; set; } = ConsoleColor.Red;

        public void Log(string message, LogType type, bool showWorkingAnimation = false)
        {
            if (ShowColouredLogs)
            {
                switch (type)
                {
                    case LogType.Debug:
                        Log(message, type, DebugColour, showWorkingAnimation);
                        break;

                    case LogType.Info:
                        Log(message, type, InfoColour, showWorkingAnimation);
                        break;

                    case LogType.Warning:
                        Log(message, type, WarningColour, showWorkingAnimation);
                        break;

                    case LogType.Error:
                        Log(message, type, ErrorColour, showWorkingAnimation);
                        break;
                }
            }
            else
                Log(message, type, ConsoleColor.White, showWorkingAnimation);
        }

        public void Log(string message, LogType type, ConsoleColor consoleColour, bool showWorkingAnimation = false)
        {
            if (Logging.ContinueLogging(type))
            {
                string logMessage = $"{DateTime.Now} {type}: {message}";

                if (AddAdditionalSpaceAfterEachLogEntry)
                    logMessage = String.Concat(logMessage, "\n");

                if (LogToConsole)
                {
                    if (showWorkingAnimation)
                        CLIEngine.ShowWorkingMessage(message, consoleColour, false, 0);
                    else
                        CLIEngine.ShowMessage(message, consoleColour, false, false, 0);
                }

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
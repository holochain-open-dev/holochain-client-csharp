
using System;

namespace NextGenSoftware.Logging
{
    public interface ILogger
    {
        void Log(string message, LogType type, bool showWorkingAnimation = false);
        void Log(string message, LogType type, ConsoleColor consoleColour, bool showWorkingAnimation = false);
    }
}
using System;

namespace NextGenSoftware.Logging
{
    public class NLogger : ILogger
    {
        public NLogger()
        {

        }
        public void Log(string message, LogType type, bool showWorkingAnimation = false)
        {
            //Log here.
        }

        public void Log(string message, LogType type, ConsoleColor consoleColour, bool showWorkingAnimation = false)
        {
            Log(message, type, showWorkingAnimation);
        }
    }
}
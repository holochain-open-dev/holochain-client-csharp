using NextGenSoftware.WebSocket;
using System;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message, LogType type)
        {
            if (type != LogType.Debug && type != LogType.Info)
                Console.WriteLine($"{type}: {message}");
            else
                Console.WriteLine(message);
        }
    }
}

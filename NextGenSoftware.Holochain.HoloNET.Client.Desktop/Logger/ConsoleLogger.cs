using NextGenSoftware.WebSocket;
using System;

namespace NextGenSoftware.Holochain.HoloNET.Client.Desktop
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message, LogType type)
        {
            Console.WriteLine($"{type}: {message}");
        }
    }
}

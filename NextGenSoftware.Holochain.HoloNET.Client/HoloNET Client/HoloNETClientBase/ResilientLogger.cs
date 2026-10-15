using System;
using System.Collections.Generic;
using NextGenSoftware.Logging;
using NextGenSoftware.Logging.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Wraps the logger a client is given so that a faulty custom ILogger can't break HoloNET: an exception from it used to
    /// escape HoloNET's WebSocket handlers (dropping the connection) and its request paths (skipping the send). Every call
    /// still reaches the wrapped logger.
    /// </summary>
    internal sealed class ResilientLogger : ILogger
    {
        public ResilientLogger(ILogger inner) => Inner = inner;

        public ILogger Inner { get; }

        public List<ILogProvider> LogProviders
        {
            get => Inner.LogProviders;
            set => Inner.LogProviders = value;
        }

        public void AddLogProvider(ILogProvider logProvider) => Inner.AddLogProvider(logProvider);

        public void AddLogProviders(IEnumerable<ILogProvider> logProviders) => Inner.AddLogProviders(logProviders);

        public void Log(string message, LogType type, bool logToFile = true, bool logToConsole = true, bool showWorkingAnimation = false, bool noLineBreaks = false, bool insertExtraNewLineAfterLogMessage = false, int? indentLogMessagesBy = 1, bool nextMessageOnSameLine = false)
        {
            try { Inner.Log(message, type, logToFile, logToConsole, showWorkingAnimation, noLineBreaks, insertExtraNewLineAfterLogMessage, indentLogMessagesBy, nextMessageOnSameLine); }
            catch { }
        }

        public void Log(string message, LogType type, ConsoleColor consoleColour, bool logToFile = true, bool logToConsole = true, bool showWorkingAnimation = false, bool noLineBreaks = false, bool insertExtraNewLineAfterLogMessage = false, int? indentLogMessagesBy = 1, bool nextMessageOnSameLine = false)
        {
            try { Inner.Log(message, type, consoleColour, logToFile, logToConsole, showWorkingAnimation, noLineBreaks, insertExtraNewLineAfterLogMessage, indentLogMessagesBy, nextMessageOnSameLine); }
            catch { }
        }
    }
}

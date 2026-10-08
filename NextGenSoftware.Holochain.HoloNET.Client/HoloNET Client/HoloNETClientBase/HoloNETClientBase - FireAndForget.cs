using System.Threading.Tasks;
using NextGenSoftware.Logging;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public abstract partial class HoloNETClientBase
    {
        /// <summary>
        /// Runs a task without awaiting it, but logs any failure instead of losing it (CS4014).
        /// Used where callers are event handlers or sync APIs that intentionally don't wait.
        /// </summary>
        protected void FireAndForget(Task task, string context)
        {
            task?.ContinueWith(t =>
                Logger.Log($"Error in {context}: {t.Exception?.GetBaseException()}", LogType.Error),
                TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}

using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>
    /// Live-conductor test classes share one sandbox conductor, which isn't reliable under concurrent installs
    /// (database StateMutationError / dropped connections), so xunit runs them one after another.
    /// </summary>
    [CollectionDefinition(LiveConductor.Collection, DisableParallelization = true)]
    public class LiveConductor
    {
        public const string Collection = "Live Holochain conductor";
    }
}

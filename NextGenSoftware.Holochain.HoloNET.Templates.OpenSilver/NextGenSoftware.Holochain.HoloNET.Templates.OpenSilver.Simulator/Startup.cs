using OpenSilver.Simulator;
using System;

namespace NextGenSoftware.Holochain.HoloNET.Templates.OpenSilver.Simulator
{
    internal static class Startup
    {
        [STAThread]
        static int Main(string[] args)
        {
            return SimulatorLauncher.Start(typeof(App));
        }
    }
}

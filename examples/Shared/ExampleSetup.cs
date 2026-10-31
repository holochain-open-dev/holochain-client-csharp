// Shared by every HoloNET example: where the Holochain conductor is, and which hApp to use.
//
// Every example connects to a running Holochain 0.7.0 / 0.7.1 conductor. Start one with
//   examples/start-conductor.sh        (Git Bash / Linux / macOS)   or   examples\start-conductor.ps1   (PowerShell)
// which runs the hc.exe bundled with HoloNET in a throwaway sandbox on admin port 7777 (HoloNET's default), or point
// the examples at your own with the HOLONET_ADMIN_URI environment variable (e.g. ws://localhost:65464).
//
// (To have HoloNET start a conductor itself instead, reference an .Embedded package such as
//  NextGenSoftware.Holochain.HoloNET.Client.Embedded and set HoloNETDNA.HolochainConductorMode = UseEmbedded.)

using System;
using System.IO;
using NextGenSoftware.Holochain.HoloNET.Client;

namespace HoloNET.Examples
{
    public static class ExampleSetup
    {
        /// <summary>The conductor's admin interface: HOLONET_ADMIN_URI (or the test runner's HOLONET_LIVE_ADMIN_URI), else HoloNET's default.</summary>
        public static string AdminUri =>
            Environment.GetEnvironmentVariable("HOLONET_ADMIN_URI")
            ?? Environment.GetEnvironmentVariable("HOLONET_LIVE_ADMIN_URI")
            ?? "ws://localhost:7777";

        /// <summary>
        /// The OASIS hApp: HOLONET_HAPP_PATH if set, otherwise the oasis.happ that HoloNET API ships (copied next to the
        /// example when it builds). It has avatar/holon entries, ORM collections and HyperNET's zome functions.
        /// </summary>
        public static string HappPath =>
            Environment.GetEnvironmentVariable("HOLONET_HAPP_PATH")
            ?? Environment.GetEnvironmentVariable("HOLONET_LIVE_HAPP_PATH")
            ?? Path.Combine(AppContext.BaseDirectory, "oasis.happ");

        /// <summary>Settings for an admin client that uses the conductor you started (rather than starting or stopping one).</summary>
        public static HoloNETDNA ConductorDNA() => new HoloNETDNA
        {
            HolochainConductorAdminURI = AdminUri,
            AutoStartHolochainConductor = false,
            AutoShutdownHolochainConductor = false,
            LogToConsole = false   // keep the example output readable; set true to watch every message to and from the conductor
        };

        /// <summary>A unique installed-app id, so running an example twice never collides with the last run.</summary>
        public static string UniqueAppId(string prefix) => prefix + "-" + Guid.NewGuid().ToString("N").Substring(0, 6);

        public static void Title(string text)
        {
            Console.WriteLine();
            Console.WriteLine("== " + text);
        }

        /// <summary>Stop the example with a clear message when something it needs failed.</summary>
        public static void Check(bool ok, string what)
        {
            if (!ok)
                throw new InvalidOperationException("Example step failed: " + what);
        }
    }
}

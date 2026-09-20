using System;
using System.IO;

namespace NextGenSoftware.Holochain.HoloNET.Client.TestHarness
{
    /// <summary>
    /// Resolves TestHarness path constants from environment variables, falling back to
    /// relative paths under the current working directory.  Set any of these env vars
    /// before launching the harness to point at your local Holochain build:
    ///
    ///   HOLONET_HC_ADMIN_URI        e.g. ws://localhost:65464
    ///   HOLONET_HC_APP_URI          e.g. ws://localhost:8888
    ///   HOLONET_OASIS_HAPP_PATH     full path to oasis.happ
    ///   HOLONET_OASIS_HAPP_FOLDER   directory containing oasis.happ
    ///   HOLONET_OASIS_DNA_PATH      full path to oasis.dna
    ///   HOLONET_NUMBERS_HAPP_PATH   full path to numbers happ
    /// </summary>
    internal static class HoloNETTestHarnessConfig
    {
        public static readonly string HcAdminURI =
            Env("HOLONET_HC_ADMIN_URI", "ws://localhost:65464");

        public static readonly string HcAppURI =
            Env("HOLONET_HC_APP_URI", "ws://localhost:8888");

        public static readonly string OasisHappPath =
            Env("HOLONET_OASIS_HAPP_PATH",
                Path.Combine("happs", "oasis", "BUILD", "happ", "oasis.happ"));

        public static readonly string OasisHappFolder =
            Env("HOLONET_OASIS_HAPP_FOLDER",
                Path.Combine("happs", "oasis", "BUILD", "happ"));

        public static readonly string OasisDnaPath =
            Env("HOLONET_OASIS_DNA_PATH",
                Path.Combine("happs", "oasis", "BUILD", "dna", "oasis.dna"));

        public static readonly string NumbersHappPath =
            Env("HOLONET_NUMBERS_HAPP_PATH",
                Path.Combine("hApps", "happ-build-tutorial-develop", "workdir", "happ"));

        private static string Env(string key, string fallback) =>
            Environment.GetEnvironmentVariable(key) is { Length: > 0 } v ? v : fallback;
    }
}

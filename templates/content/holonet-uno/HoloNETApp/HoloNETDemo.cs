// HoloNET wiring for this app. Every screen, page or endpoint in the app calls HoloNETDemo.RunAsync() and shows the text it returns,
// so the only HoloNET code you need to read (and replace with your own) is in this one file.
//
// This app uses HoloNET HOLONET_LIBRARY_NAME.
// Guide:      https://docs.holonet.oasisomniverse.one
// Tutorials:  https://learn.holonet.oasisomniverse.one
// Templates:  https://docs.holonet.oasisomniverse.one/templates
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client;
//#if (UseApi)
using NextGenSoftware.Holochain.HoloNET.API;
//#endif
//#if (UseOrm)
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.Holochain.HoloNET.ORM.Entries;
//#endif
//#if (UseHyperNet)
using NextGenSoftware.Holochain.HoloNET.HyperNET;
//#endif

namespace HoloNETDemos
{
    public static class HoloNETDemo
    {
        /// <summary>Where your Holochain conductor's admin interface is (used when the conductor is not bundled in the app).</summary>
        public static string AdminUri { get; set; } = "ws://localhost:7777";

        /// <summary>The hApp to install. The HoloNET API package copies oasis.happ next to your app; point this at your own .happ to use that instead.</summary>
        public static string HappPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "oasis.happ");

        /// <summary>The HoloNET settings. With a bundled conductor HoloNET starts and stops Holochain for you; otherwise it uses the one at AdminUri.</summary>
        public static HoloNETDNA CreateDNA()
        {
            var dna = new HoloNETDNA { LogToConsole = false };
//#if (true)
            dna.HolochainConductorAdminURI = AdminUri;
//#if (true)
            dna.AutoStartHolochainConductor = false;     // you start the conductor yourself (see the start-conductor script)
            dna.AutoShutdownHolochainConductor = false;
//#else
            // Nothing bundled and no conductor of your own: HoloNET starts the Holochain on this computer, or downloads the official one the first time (HoloNETDNA.AutoDownloadConductor).
//#endif
//#endif
            return dna;
        }

//#if (PhoneRuntime)
#if ANDROID || IOS
        /// <summary>On Android and iOS: use the Holochain runtime on the phone (Android: the shared Holochain Service Runtime when installed, otherwise a conductor built into
        /// the app; iOS: a conductor linked into the app) instead of a conductor you run. Set to false to connect to AdminUri as before. Needs the runtime libraries (see README.md).</summary>
        public static bool UsePhoneRuntime { get; set; } = true;

        /// <summary>The installed app id used with the phone's runtime.</summary>
        public static string PhoneAppId { get; set; } = "oasis";

        /// <summary>True when the runtime libraries are part of this app.</summary>
        public static bool PhoneRuntimeAvailable =>
#if ANDROID
            HoloNETAndroidRuntime.IsBridgeAvailable;
#else
            HoloNETIosRuntime.IsBridgeAvailable;
#endif

        private static int _phonePort;
        private static byte[] _phoneToken;
        private static string _phoneMode;
        private static bool _phoneStatusHooked;

        /// <summary>Starts the phone's Holochain runtime, installs the hApp and returns an app connection whose zome calls the runtime signs.</summary>
        public static async Task<HoloNETClientAppAgent> ConnectToPhoneRuntimeAsync(StringBuilder log)
        {
            if (_phoneToken == null)
            {
#if ANDROID
                if (!_phoneStatusHooked)
                {
                    _phoneStatusHooked = true;
                    // "awaiting_approval" means: the user must approve Holochain access in the notification the shared runtime posted.
                    HoloNETAndroidRuntime.StatusChanged += (status, message) => System.Diagnostics.Debug.WriteLine("[holochain runtime] " + status + ": " + message);
                }
                var session = await HoloNETAndroidRuntime.StartAsync(HoloNETAndroidRuntime.LoadHapp(HappPath), PhoneAppId);
                _phoneMode = session.Mode;
#else
                var session = await HoloNETIosRuntime.StartAsync(HoloNETIosRuntime.LoadHapp(HappPath), PhoneAppId);
                _phoneMode = "embedded";
#endif
                _phonePort = session.Port;
                _phoneToken = session.AuthenticationToken;
            }
            log.AppendLine("Holochain runtime on this phone: " + _phoneMode + " (app interface port " + _phonePort + ").");

            var dna = CreateDNA();
            dna.HolochainConductorAppAgentURI = "ws://127.0.0.1:" + _phonePort;
            dna.AppAuthenticationToken = _phoneToken;
            dna.InstalledAppId = PhoneAppId;
            dna.AutoStartHolochainConductor = false;
            dna.AutoShutdownHolochainConductor = false;
            var client = new HoloNETClientAppAgent(PhoneAppId, null, dna) { ZomeCallSigner = new PhoneZomeCallSigner() };
            var connected = await client.ConnectAsync(PhoneAppId, new Uri(dna.HolochainConductorAppAgentURI),
                retrieveAgentPubKeyAndDnaHashFromSandbox: false, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails: false);
            if (connected == null || !connected.IsConnected)
                throw new InvalidOperationException("Could not connect to the Holochain runtime on this phone: " + connected?.Message);
            return client;
        }

        /// <summary>Signs zome calls through the runtime on this phone.</summary>
        private sealed class PhoneZomeCallSigner : IZomeCallSigner
        {
            public Task<ZomeCallParamsSigned> SignZomeCallAsync(ZomeCallToSign call) =>
#if ANDROID
                new AndroidRuntimeZomeCallSigner().SignZomeCallAsync(call);
#else
                new IosRuntimeZomeCallSigner().SignZomeCallAsync(call);
#endif
        }
#endif
//#endif

//#if (UseApi)
        private static bool _configured;

        /// <summary>HoloNET API: one line each to save, load and delete data, share it, and hear when it changes.</summary>
        public static async Task<string> RunAsync()
        {
            var log = new StringBuilder();
            try
            {
//#if (PhoneRuntime)
#if ANDROID || IOS
                if (!_configured && UsePhoneRuntime && PhoneRuntimeAvailable)
                {
                    // On a phone the runtime installs the hApp and signs, so the API uses an app connection (no admin access).
                    await HoloNETAPI.UseAppClientAsync(await ConnectToPhoneRuntimeAsync(log));
                    _configured = true;
                }
#endif
//#endif
                if (!_configured) { HoloNETAPI.Configure(CreateDNA()); _configured = true; }

                await HoloNETAPI.SaveDataAsync("greeting", "Hello, Holochain!");
                log.AppendLine("greeting = " + (await HoloNETAPI.LoadDataAsync("greeting")).Result);

                await HoloNETAPI.SaveDataAsync("visits", 1);                       // any type: numbers, bools, dates, Guids, enums
                log.AppendLine("visits = " + (await HoloNETAPI.LoadDataAsync<int>("visits")).Result);

                await HoloNETAPI.SaveSharedDataAsync("leaderboard", "ada", "9001");   // shared with every agent in the app
                log.AppendLine("ada's score = " + (await HoloNETAPI.LoadSharedDataAsync<int>("leaderboard", "ada")).Result);

                var missing = await HoloNETAPI.LoadDataAsync("no-such-key");          // nothing throws: check IsSuccess / Message
                log.AppendLine("missing key: IsSuccess = " + missing.IsSuccess);
            }
            catch (Exception ex) { log.AppendLine("HoloNET error: " + ex.Message); }
            return log.ToString();
        }
//#endif
//#if (UseClient)

        /// <summary>HoloNET Client: connect to the conductor, install a hApp, make a zome call and uninstall again.</summary>
        public static async Task<string> RunAsync()
        {
//#if (PhoneRuntime)
#if ANDROID || IOS
            if (UsePhoneRuntime && PhoneRuntimeAvailable)
                return await RunOnPhoneRuntimeAsync();
#endif
//#endif
            var log = new StringBuilder();
            HoloNETClientAdmin admin = null;
            string appId = null;
            try
            {
                admin = new HoloNETClientAdmin(CreateDNA());
                var connected = await admin.ConnectAsync(AdminUri);
                if (!connected.IsConnected) return "Could not connect to the conductor: " + connected.Message;
                log.AppendLine("Connected to the conductor.");

                // One call: agent key, install, enable, signing credentials, app interface and an app client.
                appId = "holonet-app-" + Guid.NewGuid().ToString("N")[..6];
                var installed = await admin.InstallEnableSignAttachAndConnectToHappAsync(appId, HappPath, "oasis");
                if (!installed.IsAppConnected) return log + "Could not install the hApp: " + installed.Message;
                log.AppendLine("Agent " + installed.AgentPubKey);

                // A zome call. Parameters are serialised by field name, so they match the Rust struct.
                var created = await installed.HoloNETClientAppAgent.CallZomeFunctionAsync("oasis", "create_entry_avatar", new System.Collections.Generic.Dictionary<string, object>
                {
                    ["id"] = Guid.NewGuid().ToString(), ["username"] = "ada", ["email"] = "ada@example.com", ["first_name"] = "Ada"
                });
                log.AppendLine(created.IsError ? "Zome call failed: " + created.Message : "Created an avatar, ActionHash " + created.Records[0].ActionHash);
                await installed.HoloNETClientAppAgent.DisconnectAsync();
            }
            catch (Exception ex) { log.AppendLine("HoloNET error: " + ex.Message); }
            finally
            {
                if (admin != null)
                {
                    if (appId != null) await admin.UninstallAppAsync(appId);
                    await admin.DisconnectAsync();
                }
            }
            return log.ToString();
        }
//#if (PhoneRuntime)
#if ANDROID || IOS
        // The same demo on the phone's runtime: there is no admin connection, so the hApp is installed by the runtime and the call is signed by it.
        private static async Task<string> RunOnPhoneRuntimeAsync()
        {
            var log = new StringBuilder();
            try
            {
                var client = await ConnectToPhoneRuntimeAsync(log);
                var created = await client.CallZomeFunctionAsync("oasis", "create_entry_avatar", new System.Collections.Generic.Dictionary<string, object>
                {
                    ["id"] = Guid.NewGuid().ToString(), ["username"] = "ada", ["email"] = "ada@example.com", ["first_name"] = "Ada"
                });
                log.AppendLine(created.IsError ? "Zome call failed: " + created.Message : "Created an avatar, ActionHash " + created.Records[0].ActionHash);
                await client.DisconnectAsync();
            }
            catch (Exception ex) { log.AppendLine("HoloNET error: " + ex.Message); }
            return log.ToString();
        }
#endif
//#endif
//#endif
//#if (UseOrm)

        /// <summary>An entry model: it maps onto a struct in the hApp's Rust zome. The base constructor names the zome and its load / create / update / delete functions.</summary>
        public class Avatar : HoloNETEntryBase
        {
            public Avatar(IHoloNETClientAppBase client) : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", client) { }

            [HolochainRustFieldName("id")] public string Id { get; set; }
            [HolochainRustFieldName("username")] public string Username { get; set; }
            [HolochainRustFieldName("email")] public string Email { get; set; }
            [HolochainRustFieldName("first_name")] public string FirstName { get; set; }
            [HolochainRustFieldName("karma")] public long Karma { get; set; }
        }

        /// <summary>HoloNET ORM: save, load, change and delete your own C# classes.</summary>
        public static async Task<string> RunAsync()
        {
//#if (PhoneRuntime)
#if ANDROID || IOS
            if (UsePhoneRuntime && PhoneRuntimeAvailable)
                return await RunOnPhoneRuntimeAsync();
#endif
//#endif
            var log = new StringBuilder();
            HoloNETClientAdmin admin = null;
            IHoloNETClientAppAgent client = null;
            string appId = null;
            try
            {
                admin = new HoloNETClientAdmin(CreateDNA());
                var connected = await admin.ConnectAsync(AdminUri);
                if (!connected.IsConnected) return "Could not connect to the conductor: " + connected.Message;

                appId = "holonet-app-" + Guid.NewGuid().ToString("N")[..6];
                var installed = await admin.InstallEnableSignAttachAndConnectToHappAsync(appId, HappPath, "oasis");
                if (!installed.IsAppConnected) return "Could not install the hApp: " + installed.Message;
                client = installed.HoloNETClientAppAgent;

                var ada = new Avatar(client) { Id = Guid.NewGuid().ToString(), Username = "ada", Email = "ada@example.com", FirstName = "Ada", Karma = 7 };
                var saved = await ada.SaveAsync();                    // no ActionHash yet, so this creates it
                log.AppendLine(saved.IsError ? "Save failed: " + saved.Message : "Saved " + ada.FirstName + ", ActionHash " + ada.ActionHash);

                var loaded = new Avatar(client);
                await loaded.LoadAsync(ada.ActionHash);               // always load by ActionHash
                loaded.FirstName = "Grace";                           // change tracking: HoloNET compares with what was loaded
                log.AppendLine("Loaded " + loaded.Username + "; edited, changed? " + loaded.HasEntryChanged());
                await loaded.SaveAsync();                             // has an ActionHash, so this updates
                await loaded.DeleteAsync();
                log.AppendLine("Updated and deleted.");
            }
            catch (Exception ex) { log.AppendLine("HoloNET error: " + ex.Message); }
            finally
            {
                if (client != null) await client.DisconnectAsync();
                if (admin != null)
                {
                    if (appId != null) await admin.UninstallAppAsync(appId);
                    await admin.DisconnectAsync();
                }
            }
            return log.ToString();
        }
//#if (PhoneRuntime)
#if ANDROID || IOS
        // The same demo on the phone's runtime: there is no admin connection, so the hApp is installed by the runtime and the calls are signed by it.
        private static async Task<string> RunOnPhoneRuntimeAsync()
        {
            var log = new StringBuilder();
            IHoloNETClientAppAgent client = null;
            try
            {
                client = await ConnectToPhoneRuntimeAsync(log);
                var ada = new Avatar(client) { Id = Guid.NewGuid().ToString(), Username = "ada", Email = "ada@example.com", FirstName = "Ada", Karma = 7 };
                var saved = await ada.SaveAsync();
                log.AppendLine(saved.IsError ? "Save failed: " + saved.Message : "Saved " + ada.FirstName + ", ActionHash " + ada.ActionHash);
                var loaded = new Avatar(client);
                await loaded.LoadAsync(ada.ActionHash);
                loaded.FirstName = "Grace";
                log.AppendLine("Loaded " + loaded.Username + "; edited, changed? " + loaded.HasEntryChanged());
                await loaded.SaveAsync();
                await loaded.DeleteAsync();
                log.AppendLine("Updated and deleted.");
            }
            catch (Exception ex) { log.AppendLine("HoloNET error: " + ex.Message); }
            finally { if (client != null) await client.DisconnectAsync(); }
            return log.ToString();
        }
#endif
//#endif
//#endif
//#if (UseHyperNet)

        /// <summary>HoloNET HyperNET: two players join a zone, one sends a message and the other receives it.</summary>
        public static async Task<string> RunAsync()
        {
//#if (PhoneRuntime)
#if ANDROID || IOS
            if (UsePhoneRuntime && PhoneRuntimeAvailable)
                return await RunOnPhoneRuntimeAsync();
#endif
//#endif
            var log = new StringBuilder();
            HoloNETClientAdmin admin = null;
            var ids = new System.Collections.Generic.List<string>();
            var sessions = new System.Collections.Generic.List<HyperNETSession>();
            try
            {
                admin = new HoloNETClientAdmin(CreateDNA());
                var connected = await admin.ConnectAsync(AdminUri);
                if (!connected.IsConnected) return "Could not connect to the conductor: " + connected.Message;

                string zone = "zone-" + Guid.NewGuid().ToString("N")[..6];
                bool received = false;
                foreach (string name in new[] { "Alice", "Bob" })
                {
                    string appId = "holonet-" + name.ToLower() + "-" + Guid.NewGuid().ToString("N")[..6];
                    var installed = await admin.InstallEnableSignAttachAndConnectToHappAsync(appId, HappPath, "oasis");
                    if (!installed.IsAppConnected) return "Could not install the hApp for " + name + ": " + installed.Message;
                    ids.Add(appId);

                    var session = new HyperNETSession(installed.HoloNETClientAppAgent, zone, name, new HyperNETOptions());
                    string me = name;
                    session.OnPeerJoined += (s, e) => log.AppendLine($"[{me}] {e.Peer.PlayerName} joined");
                    session.OnMessage += (s, e) => { log.AppendLine($"[{me}] '{e.Channel}' from {e.From.PlayerName}: {Encoding.UTF8.GetString(e.Payload)}"); received = true; };
                    sessions.Add(session);
                    await session.JoinAsync();
                }

                for (int i = 0; i < 100 && sessions.Any(s => s.Peers.Count < 1); i++)
                {
                    if (i % 20 == 19) foreach (var s in sessions) await s.RefreshPeersAsync();
                    await Task.Delay(100);
                }

                await sessions[0].SendAsync("chat", Encoding.UTF8.GetBytes("hello from Alice"), HyperNETTarget.Zone);
                for (int i = 0; i < 100 && !received; i++) await Task.Delay(100);
                log.AppendLine(received ? "Message delivered." : "No message arrived.");
            }
            catch (Exception ex) { log.AppendLine("HoloNET error: " + ex.Message); }
            finally
            {
                foreach (var s in sessions) { try { await s.LeaveAsync(); } catch { } s.Dispose(); }
                if (admin != null)
                {
                    foreach (string id in ids) await admin.UninstallAppAsync(id);
                    await admin.DisconnectAsync();
                }
            }
            return log.ToString();
        }
//#if (PhoneRuntime)
#if ANDROID || IOS
        // On a phone the runtime holds one app (one player), so this joins a zone as one player and lists who else is there.
        private static async Task<string> RunOnPhoneRuntimeAsync()
        {
            var log = new StringBuilder();
            HyperNETSession session = null;
            try
            {
                var client = await ConnectToPhoneRuntimeAsync(log);
                session = new HyperNETSession(client, "zone-phone", "Player", new HyperNETOptions());
                session.OnPeerJoined += (s, e) => log.AppendLine(e.Peer.PlayerName + " joined");
                await session.JoinAsync();
                await Task.Delay(3000);
                await session.RefreshPeersAsync();
                log.AppendLine("Joined the zone; " + session.Peers.Count + " other player(s) here.");
            }
            catch (Exception ex) { log.AppendLine("HoloNET error: " + ex.Message); }
            finally { if (session != null) { try { await session.LeaveAsync(); } catch { } session.Dispose(); } }
            return log.ToString();
        }
#endif
//#endif
//#endif
    }
}

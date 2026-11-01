// The four demos the sandbox page can run, one per HoloNET library. They are the same code the dotnet new templates use (templates/shared/HoloNETDemo.cs),
// run on the sandbox's conductor, and return the text the page shows. Nothing a visitor sends is ever executed: a demo is chosen by name only.
using System.Text;
using NextGenSoftware.Holochain.HoloNET.API;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.Holochain.HoloNET.HyperNET;
using NextGenSoftware.Holochain.HoloNET.ORM.Entries;

namespace HoloNETSandbox
{
    public class Avatar : HoloNETEntryBase
    {
        public Avatar(IHoloNETClientAppBase client) : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", client) { }

        [HolochainRustFieldName("id")] public string Id { get; set; }
        [HolochainRustFieldName("username")] public string Username { get; set; }
        [HolochainRustFieldName("email")] public string Email { get; set; }
        [HolochainRustFieldName("first_name")] public string FirstName { get; set; }
        [HolochainRustFieldName("karma")] public long Karma { get; set; }
    }

    public static class Demos
    {
        public static readonly string[] Names = { "api", "client", "orm", "hypernet" };

        public static string Code(string demo) => demo switch
        {
            "api" => "await HoloNETAPI.SaveDataAsync(\"greeting\", \"Hello, Holochain!\");\nstring greeting = (await HoloNETAPI.LoadDataAsync(\"greeting\")).Result;\nawait HoloNETAPI.SaveSharedDataAsync(\"leaderboard\", \"ada\", \"9001\");",
            "client" => "var admin = new HoloNETClientAdmin(dna);\nawait admin.ConnectAsync(adminUri);\nvar installed = await admin.InstallEnableSignAttachAndConnectToHappAsync(appId, happPath, \"oasis\");\nvar created = await installed.HoloNETClientAppAgent.CallZomeFunctionAsync(\"oasis\", \"create_entry_avatar\", new Dictionary<string, object> { [\"username\"] = \"ada\" });",
            "orm" => "var ada = new Avatar(client) { Username = \"ada\", FirstName = \"Ada\" };\nawait ada.SaveAsync();      // creates it\nada.FirstName = \"Grace\";   // change tracking\nawait ada.SaveAsync();      // updates it\nawait ada.DeleteAsync();",
            "hypernet" => "var session = new HyperNETSession(client, zone, \"Alice\", new HyperNETOptions());\nsession.OnMessage += (s, e) => Console.WriteLine(e.From.PlayerName);\nawait session.JoinAsync();\nawait session.SendAsync(\"chat\", bytes, HyperNETTarget.Zone);",
            _ => null
        };

        public static HoloNETDNA CreateDNA(string adminUri) => new HoloNETDNA
        {
            LogToConsole = false,
            HolochainConductorAdminURI = adminUri,
            AutoStartHolochainConductor = false,
            AutoShutdownHolochainConductor = false
        };

        public static async Task<string> RunAsync(string demo, string adminUri, string happPath)
        {
            switch (demo)
            {
                case "api": return await ApiAsync();
                case "client": return await ClientAsync(adminUri, happPath);
                case "orm": return await OrmAsync(adminUri, happPath);
                case "hypernet": return await HyperNetAsync(adminUri, happPath);
                default: return "Unknown demo.";
            }
        }

        // The API demo uses its own keys, separate from the visitors' keys (they are prefixed per session).
        private static async Task<string> ApiAsync()
        {
            var log = new StringBuilder();
            string key = "demo-" + Guid.NewGuid().ToString("N")[..6];
            await HoloNETAPI.SaveDataAsync(key + "-greeting", "Hello, Holochain!");
            log.AppendLine("greeting = " + (await HoloNETAPI.LoadDataAsync(key + "-greeting")).Result);
            await HoloNETAPI.SaveDataAsync(key + "-visits", 1);
            log.AppendLine("visits = " + (await HoloNETAPI.LoadDataAsync<int>(key + "-visits")).Result);
            await HoloNETAPI.SaveSharedDataAsync("sandbox-demo", key, "9001");
            log.AppendLine("shared score = " + (await HoloNETAPI.LoadSharedDataAsync<int>("sandbox-demo", key)).Result);
            var missing = await HoloNETAPI.LoadDataAsync(key + "-no-such-key");
            log.AppendLine("missing key: IsSuccess = " + missing.IsSuccess);
            await HoloNETAPI.DeleteDataAsync(key + "-greeting");
            await HoloNETAPI.DeleteDataAsync(key + "-visits");
            await HoloNETAPI.DeleteSharedDataAsync("sandbox-demo", key);
            return log.ToString();
        }

        private static async Task<string> ClientAsync(string adminUri, string happPath)
        {
            var log = new StringBuilder();
            HoloNETClientAdmin admin = null;
            string appId = null;
            try
            {
                admin = new HoloNETClientAdmin(CreateDNA(adminUri));
                var connected = await admin.ConnectAsync(adminUri);
                if (!connected.IsConnected) return "Could not connect to the conductor: " + connected.Message;
                log.AppendLine("Connected to the conductor.");
                appId = "sbx-client-" + Guid.NewGuid().ToString("N")[..6];
                var installed = await admin.InstallEnableSignAttachAndConnectToHappAsync(appId, happPath, "oasis");
                if (!installed.IsAppConnected) return log + "Could not install the hApp: " + installed.Message;
                log.AppendLine("Agent " + installed.AgentPubKey);
                var created = await installed.HoloNETClientAppAgent.CallZomeFunctionAsync("oasis", "create_entry_avatar", new Dictionary<string, object>
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

        private static async Task<string> OrmAsync(string adminUri, string happPath)
        {
            var log = new StringBuilder();
            HoloNETClientAdmin admin = null;
            IHoloNETClientAppAgent client = null;
            string appId = null;
            try
            {
                admin = new HoloNETClientAdmin(CreateDNA(adminUri));
                var connected = await admin.ConnectAsync(adminUri);
                if (!connected.IsConnected) return "Could not connect to the conductor: " + connected.Message;
                appId = "sbx-orm-" + Guid.NewGuid().ToString("N")[..6];
                var installed = await admin.InstallEnableSignAttachAndConnectToHappAsync(appId, happPath, "oasis");
                if (!installed.IsAppConnected) return "Could not install the hApp: " + installed.Message;
                client = installed.HoloNETClientAppAgent;

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

        private static async Task<string> HyperNetAsync(string adminUri, string happPath)
        {
            var log = new StringBuilder();
            HoloNETClientAdmin admin = null;
            var ids = new List<string>();
            var sessions = new List<HyperNETSession>();
            try
            {
                admin = new HoloNETClientAdmin(CreateDNA(adminUri));
                var connected = await admin.ConnectAsync(adminUri);
                if (!connected.IsConnected) return "Could not connect to the conductor: " + connected.Message;
                string zone = "zone-" + Guid.NewGuid().ToString("N")[..6];
                bool received = false;
                foreach (string name in new[] { "Alice", "Bob" })
                {
                    string appId = "sbx-hn-" + name.ToLower() + "-" + Guid.NewGuid().ToString("N")[..6];
                    var installed = await admin.InstallEnableSignAttachAndConnectToHappAsync(appId, happPath, "oasis");
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
    }
}

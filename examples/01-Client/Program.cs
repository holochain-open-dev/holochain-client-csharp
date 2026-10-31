// HoloNET example 01: HoloNET Client
//
// The Client is the connection to a Holochain conductor; everything else in HoloNET is built on it. This example:
//   1. connects to the conductor's admin interface (HoloNETClientAdmin),
//   2. installs, enables, signs and attaches the OASIS hApp and connects an app client - all in one call,
//   3. calls zome functions (create and read an entry),
//   4. receives a signal sent by another agent,
//   5. uses a few admin calls (list apps, storage info, network stats),
//   6. uninstalls everything and disconnects.
//
// Run: start a conductor (examples/start-conductor.sh), then   dotnet run --project examples/01-Client
// Docs: https://docs.holonet.oasisomniverse.one/client-guide  ·  API reference: docs/api/Client.md

using System.Collections.Concurrent;
using HoloNET.Examples;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;

// ---- 1. Connect to the conductor's admin interface ---------------------------------------------------------------
ExampleSetup.Title("1. Connect the admin client to " + ExampleSetup.AdminUri);

// HoloNETDNA holds every setting. ExampleSetup.ConductorDNA() points at the conductor you started and tells HoloNET not
// to start or stop one itself.
var admin = new HoloNETClientAdmin(ExampleSetup.ConductorDNA());
HoloNETConnectedEventArgs connected = await admin.ConnectAsync(ExampleSetup.AdminUri);
ExampleSetup.Check(connected.IsConnected, "connect to the conductor: " + connected.Message);
Console.WriteLine("Connected.");

// ---- 2. Install the hApp and connect an app client: one call -----------------------------------------------------
// InstallEnableSignAttachAndConnectToHappAsync does every step Holochain 0.7 needs: generate an agent key, install the
// .happ, enable it, create signing credentials + an Assigned zome-call capability grant, attach an app interface,
// issue an app authentication token, and connect a HoloNETClientAppAgent to it.
string appId = ExampleSetup.UniqueAppId("example-client");
ExampleSetup.Title("2. Install " + Path.GetFileName(ExampleSetup.HappPath) + " as '" + appId + "'");
InstallEnableSignAttachAndConnectToHappEventArgs installed = await admin.InstallEnableSignAttachAndConnectToHappAsync(appId, ExampleSetup.HappPath, "oasis");
ExampleSetup.Check(installed.IsAppConnected, "install the hApp: " + installed.Message);
IHoloNETClientAppAgent client = installed.HoloNETClientAppAgent;
Console.WriteLine($"Agent {installed.AgentPubKey}");
Console.WriteLine($"DNA   {installed.DnaHash}  (app interface on port {installed.AttachedOnPort})");

// A second agent (its own installed app), so we can show signals between agents in step 4.
string peerId = ExampleSetup.UniqueAppId("example-client-peer");
var peerInstall = await admin.InstallEnableSignAttachAndConnectToHappAsync(peerId, ExampleSetup.HappPath, "oasis");
ExampleSetup.Check(peerInstall.IsAppConnected, "install the second agent: " + peerInstall.Message);
IHoloNETClientAppAgent peer = peerInstall.HoloNETClientAppAgent;

try
{
    // ---- 3. Zome calls -----------------------------------------------------------------------------------------------
    // CallZomeFunctionAsync(zome, function, params). Params can be an anonymous object, a dictionary or a class; HoloNET
    // serialises it to MessagePack using its field names, so they must match the Rust struct's fields.
    ExampleSetup.Title("3. Zome calls: create an avatar entry, then read it back");
    ZomeFunctionCallBackEventArgs created = await client.CallZomeFunctionAsync("oasis", "create_entry_avatar", new Dictionary<string, object>
    {
        ["id"] = Guid.NewGuid().ToString(),
        ["username"] = "ada",
        ["email"] = "ada@example.com",
        ["first_name"] = "Ada"
    });
    // Zome errors never throw: check IsError / Message.
    ExampleSetup.Check(!created.IsError, "create_entry_avatar: " + created.Message);

    // The new record's ActionHash: what Holochain 0.7 uses to read, update and delete it.
    string actionHash = created.Records[0].ActionHash;
    Console.WriteLine("Created, ActionHash " + actionHash);

    // Hashes travel as raw bytes; ConvertHoloHashToBytes turns the string form back into them.
    ZomeFunctionCallBackEventArgs read = await client.CallZomeFunctionAsync("oasis", "get_entry_avatar", client.ConvertHoloHashToBytes(actionHash));
    ExampleSetup.Check(!read.IsError, "get_entry_avatar: " + read.Message);
    // Records[0].EntryKeyValuePairs has the decoded entry fields.
    Console.WriteLine("Read back: username = " + read.Records[0].EntryKeyValuePairs["username"] + ", first_name = " + read.Records[0].EntryKeyValuePairs["first_name"]);

    // ---- 4. Signals --------------------------------------------------------------------------------------------------
    // A zome emits signals with emit_signal; HoloNET raises OnSignalCallBack. Here the peer joins a HyperNET zone (a link
    // other agents can find) and our client sends it a remote signal with the OASIS hApp's hypernet_send function; the
    // peer's zome receives it in recv_remote_signal and emits it to the peer's HoloNET client.
    ExampleSetup.Title("4. Signals between agents");
    var received = new ConcurrentQueue<SignalCallBackEventArgs>();
    peer.OnSignalCallBack += (sender, e) => received.Enqueue(e);
    await peer.CallZomeFunctionAsync("oasis", "hypernet_join", new Dictionary<string, object> { ["session"] = "example-zone", ["player_name"] = "peer" });
    await client.CallZomeFunctionAsync("oasis", "hypernet_send", new Dictionary<string, object>
    {
        ["session"] = "example-zone",
        ["payload"] = System.Text.Encoding.UTF8.GetBytes("hello from the first agent"),
        ["to"] = null   // null: everyone in the zone
    });

    for (int i = 0; i < 100 && received.IsEmpty; i++)
        await Task.Delay(100);
    ExampleSetup.Check(!received.IsEmpty, "receive the signal");
    // RawSignalData.Data has the signal's MessagePack bytes (here: the OASIS hApp's HyperNET signal struct).
    Console.WriteLine($"The second agent received a signal ({received.First().RawSignalData.Data.Length} bytes).");

    // ---- 5. Admin calls ----------------------------------------------------------------------------------------------
    ExampleSetup.Title("5. Admin calls");
    var apps = await admin.ListAppsAsync(AppStatusFilter.Enabled);
    Console.WriteLine("Enabled apps: " + string.Join(", ", apps.Apps.Select(a => a.installed_app_id)));

    var storage = await admin.GetStorageInfoAsync();
    if (!storage.IsError && storage.StorageInfoResponse?.blobs != null)
        foreach (var blob in storage.StorageInfoResponse.blobs.Where(b => b.dna != null))
            Console.WriteLine($"DNA storage: {blob.dna.dht_data_size} bytes of DHT data");

    var stats = await admin.DumpNetworkStatsAsync();
    Console.WriteLine("Network stats: " + (stats.IsError ? stats.Message : "returned (" + (stats.NetworkStatsDumpJSON?.Length ?? 0) + " chars of JSON)"));
}
finally
{
    // ---- 6. Clean up -------------------------------------------------------------------------------------------------
    ExampleSetup.Title("6. Uninstall and disconnect");
    await client.DisconnectAsync();
    await peer.DisconnectAsync();
    await admin.UninstallAppAsync(appId);
    await admin.UninstallAppAsync(peerId);
    await admin.DisconnectAsync();
    Console.WriteLine("Done.");
}

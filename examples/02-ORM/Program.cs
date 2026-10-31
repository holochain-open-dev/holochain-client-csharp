// HoloNET example 02: HoloNET ORM
//
// The ORM maps your C# classes onto your hApp's entries: call SaveAsync / LoadAsync / DeleteAsync on an object and HoloNET
// makes the zome calls, tracks changes and keeps the hashes. Collections do the same for lists of entries.
// This example: an entry model, create / load / update (change tracking) / delete, an audit entry (created/modified by,
// versions), and a collection (load, add, remove, save every change as one batch).
//
// Run: start a conductor (examples/start-conductor.sh), then   dotnet run --project examples/02-ORM
// Docs: https://docs.holonet.oasisomniverse.one/orm-guide  ·  API reference: docs/api/ORM.md

using HoloNET.Examples;
using HoloNET.Examples.ORM;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.Holochain.HoloNET.ORM.Collections;

// One app connection, shared by every entry and collection (rather than each creating its own).
var admin = new HoloNETClientAdmin(ExampleSetup.ConductorDNA());
ExampleSetup.Check((await admin.ConnectAsync(ExampleSetup.AdminUri)).IsConnected, "connect to the conductor at " + ExampleSetup.AdminUri);
string appId = ExampleSetup.UniqueAppId("example-orm");
var installed = await admin.InstallEnableSignAttachAndConnectToHappAsync(appId, ExampleSetup.HappPath, "oasis");
ExampleSetup.Check(installed.IsAppConnected, "install the hApp: " + installed.Message);
IHoloNETClientAppAgent client = installed.HoloNETClientAppAgent;

try
{
    // ---- An entry: create, load, update, delete ------------------------------------------------------------------
    ExampleSetup.Title("Entry: create, load, update, delete");

    var ada = new Avatar(client) { Id = Guid.NewGuid().ToString(), Username = "ada", Email = "ada@example.com", FirstName = "Ada", Karma = 7 };
    var created = await ada.SaveAsync();              // no ActionHash yet, so this creates it
    ExampleSetup.Check(!created.IsError, "create: " + created.Message);
    Console.WriteLine($"Created {ada.FirstName}: ActionHash {ada.ActionHash}, state {ada.State}");

    var loaded = new Avatar(client);
    ExampleSetup.Check(!(await loaded.LoadAsync(ada.ActionHash)).IsError, "load");   // always load by ActionHash
    Console.WriteLine($"Loaded {loaded.FirstName} (karma {loaded.Karma}); changed? {loaded.HasEntryChanged()}");

    loaded.FirstName = "Grace";                         // change tracking: HoloNET compares with what was loaded
    loaded.Karma = 8;
    Console.WriteLine($"Edited; changed? {loaded.HasEntryChanged()}");
    ExampleSetup.Check(!(await loaded.SaveAsync()).IsError, "update");               // has an ActionHash, so this updates
    Console.WriteLine($"Updated: new ActionHash {loaded.ActionHash}, previous version {loaded.PreviousVersionActionHash}");

    ExampleSetup.Check(!(await loaded.DeleteAsync()).IsError, "delete");
    Console.WriteLine("Deleted.");

    // ---- An audit entry: who created / modified it, and when -------------------------------------------------------
    ExampleSetup.Title("Audit entry");
    var audited = new AuditedAvatar(client) { Username = "audited", Email = "a@example.com" };
    ExampleSetup.Check(!(await audited.SaveAsync()).IsError, "audit create");
    audited.Email = "b@example.com";
    ExampleSetup.Check(!(await audited.SaveAsync()).IsError, "audit update");
    Console.WriteLine($"Id {audited.Id}, version {audited.Version}, created {audited.CreatedDate:u} by {Short(audited.CreatedBy)}, modified {audited.ModifiedDate:u}");

    // ---- A collection: a list of entries linked from an anchor ------------------------------------------------------
    ExampleSetup.Title("Collection");
    string guildAnchor = "guild-" + Guid.NewGuid().ToString("N").Substring(0, 6);
    HoloNETCollection<Avatar> guild = Avatar.NewGuild(client);
    ExampleSetup.Check(!(await guild.LoadCollectionAsync(guildAnchor)).IsError, "load the (empty) guild");

    // One at a time: each add saves the entry and links it.
    var bob = new Avatar { Id = Guid.NewGuid().ToString(), Username = "bob", Email = "bob@example.com", FirstName = "Bob" };
    ExampleSetup.Check(!(await guild.AddHoloNETEntryToCollectionAndSaveAsync(bob, collectionAnchor: guildAnchor)).IsError, "add Bob");

    // Or change the list freely and save every change at once - adds, edits and removes - as one zome call.
    guild.Add(new Avatar { Id = Guid.NewGuid().ToString(), Username = "cara", Email = "cara@example.com", FirstName = "Cara" });
    guild.Single(a => a.FirstName == "Bob").Karma = 42;
    var saved = await guild.SaveAllChangesAsync(saveAsOneBatchOperation: true, collectionAnchor: guildAnchor);
    ExampleSetup.Check(!saved.IsError, "save all changes: " + saved.Message);

    // Another view of the same collection, straight from the DHT.
    var reloaded = Avatar.NewGuild(client);
    ExampleSetup.Check(!(await reloaded.LoadCollectionAsync(guildAnchor)).IsError, "reload the guild");
    foreach (Avatar member in reloaded.OrderBy(a => a.FirstName))
        Console.WriteLine($"  {member.FirstName} (karma {member.Karma})");

    var cara = reloaded.Single(a => a.FirstName == "Cara");
    ExampleSetup.Check(!(await reloaded.RemoveHoloNETEntryFromCollectionAndSaveAsync(cara, guildAnchor)).IsError, "remove Cara");
    Console.WriteLine($"Removed Cara; {reloaded.Count} member(s) left.");

    // (HoloNETObservableCollection<T> is the same, as an ObservableCollection for WPF / MAUI / Uno data binding.)
}
finally
{
    await client.DisconnectAsync();
    await admin.UninstallAppAsync(appId);
    await admin.DisconnectAsync();
    Console.WriteLine("Done.");
}

static string Short(string key) => string.IsNullOrEmpty(key) ? "-" : key.Substring(0, Math.Min(12, key.Length)) + "...";

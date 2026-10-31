# Model your data with the ORM

**You'll build:** a C# class that maps onto a hApp entry: save it, load it, change it (with change tracking), delete it, and keep lists of them in collections.
**Time:** 20 minutes. **You need:** a running conductor, and the OASIS hApp (`oasis.happ`) next to your project.
**Finished code:** [`examples/02-ORM`](https://github.com/holochain-open-dev/holochain-client-csharp/blob/main/examples/02-ORM/Program.cs).

## 1. Create the project

```bash
dotnet new console -n Guild
cd Guild
dotnet add package NextGenSoftware.Holochain.HoloNET.ORM
```

## 2. Describe the entry

The OASIS hApp has an avatar entry and four zome functions for it. Map a class onto them:

```csharp
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.Holochain.HoloNET.ORM.Entries;

public class Avatar : HoloNETEntryBase
{
    public Avatar() : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar",
        createHoloNETClientConnection: false, autoCallInitialize: false) { }

    public Avatar(IHoloNETClientAppBase client)
        : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", client) { }

    [HolochainRustFieldName("id")] public string Id { get; set; }
    [HolochainRustFieldName("username")] public string Username { get; set; }
    [HolochainRustFieldName("email")] public string Email { get; set; }
    [HolochainRustFieldName("first_name")] public string FirstName { get; set; }
}
```

- **The constructor** names the zome and its load, create, update and delete functions.
- **`[HolochainRustFieldName]`** maps each property onto a field of the Rust struct.

## 3. Save, load, change, delete

Connect the same way as in [the first tutorial](01-first-zome-call.md), then:

```csharp
var ada = new Avatar(client) { Id = Guid.NewGuid().ToString(), Username = "ada", Email = "ada@example.com", FirstName = "Ada" };
await ada.SaveAsync();                          // creates it; ada.ActionHash is set

var loaded = new Avatar(client);
await loaded.LoadAsync(ada.ActionHash);

loaded.FirstName = "Grace";
Console.WriteLine(loaded.HasEntryChanged());    // True: the ORM tracks changes
await loaded.SaveAsync();                       // updates it (new ActionHash, PreviousVersionActionHash = the old one)

await loaded.DeleteAsync();
```

## 4. Keep lists: collections

A collection is a list of entries linked from an **anchor**, such as one guild's members:

```csharp
using NextGenSoftware.Holochain.HoloNET.ORM.Collections;

var guild = new HoloNETCollection<Avatar>("oasis", "get_avatar_collection", "add_avatar_to_collection",
    "remove_avatar_from_collection", client, "batch_update_avatar_collection");
await guild.LoadCollectionAsync("guild-42");

guild.Add(new Avatar { Id = Guid.NewGuid().ToString(), Username = "bob", Email = "bob@example.com", FirstName = "Bob" });
guild.Add(new Avatar { Id = Guid.NewGuid().ToString(), Username = "cara", Email = "cara@example.com", FirstName = "Cara" });
await guild.SaveAllChangesAsync(saveAsOneBatchOperation: true, collectionAnchor: "guild-42");   // one zome call
```

Load `guild-42` from another app, or another machine, and Bob and Cara are there.

## What just happened

- **The ORM** turned your object into the zome calls the hApp expects, and kept the hashes for you.
- **Change tracking.** It compared the entry with what was loaded, so `SaveAsync` knew it was an update.
- **Collections.** They track adds, removes and edits, and can save them all in one go.

## Next

- [Build a live web site on Holochain](04-a-live-site-on-holochain.md).
- [Generate a hApp](06-generate-a-happ.md) for your own classes, so the Rust is written for you.
- The [ORM guide](https://docs.holonet.oasisomniverse.one/orm-guide) covers audit entries, observable collections and the Rust side.

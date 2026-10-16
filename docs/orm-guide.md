# HoloNET ORM guide

The ORM maps your C# classes onto your hApp's entries. You call `SaveAsync`, `LoadAsync` and `DeleteAsync` on an object, and HoloNET makes the zome calls, tracks changes and keeps the hashes. Collections do the same for lists of entries.

- Packages: `NextGenSoftware.Holochain.HoloNET.ORM`, with `.Embedded`, `.Embedded.Holochain071` and `.Embedded.Holochain070` variants.
- Every member: [API reference](api/ORM.md). The README describes [HoloNETEntryBase](../README.md#holonetentrybaseclass), [HoloNETAuditEntryBase](../README.md#holonetauditentrybaseclass), [HoloNETCollection](../README.md#holonetcollection) and [HoloNETObservableCollection](../README.md#holonetobservablecollection) in full.

Every example below is taken from the live tests, which run against real Holochain 0.7.0 and 0.7.1 conductors and the OASIS hApp.

## 1. An entry model

```csharp
using NextGenSoftware.Holochain.HoloNET.Client;             // HolochainRustFieldName
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;  // IHoloNETClientAppBase
using NextGenSoftware.Holochain.HoloNET.ORM.Entries;

public class Avatar : HoloNETEntryBase
{
    // zome name, then the load / create / update / delete zome functions; then the client to use.
    public Avatar(IHoloNETClientAppBase client)
        : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", client) { }

    [HolochainRustFieldName("id")]         public string Id { get; set; }
    [HolochainRustFieldName("username")]   public string Username { get; set; }
    [HolochainRustFieldName("email")]      public string Email { get; set; }
    [HolochainRustFieldName("first_name")] public string FirstName { get; set; }
    [HolochainRustFieldName("karma")]      public long Karma { get; set; }
}
```

`[HolochainRustFieldName]` maps each property onto a field of your Rust struct. Properties without it aren't sent.

There are other constructors as well:

- **Your own connection.** Pass `createHoloNETClientConnection: true` with a `HoloNETDNA`, and the entry creates and connects its own client.
- **Construct now, connect later.** Pass `autoCallInitialize: false`, then call `InitializeAsync` yourself.

## 2. The Rust side

The ORM expects these shapes. The OASIS hApp's avatar functions are a working reference.

```rust
#[hdk_extern] pub fn create_entry_avatar(avatar: Avatar) -> ExternResult<Record>
#[hdk_extern] pub fn get_entry_avatar(hash: ActionHash) -> ExternResult<Option<Record>>
#[hdk_extern] pub fn update_entry_avatar(input: UpdateAvatarInput) -> ExternResult<Record>   // { original_action_hash, updated_entry }
#[hdk_extern] pub fn delete_entry_avatar(hash: ActionHash) -> ExternResult<Record>
```

Always pass the **ActionHash** (`entry.ActionHash`), not the EntryHash, to load, update and delete.

## 3. Create, load, update, delete

```csharp
var avatar = new Avatar(client) { Id = Guid.NewGuid().ToString(), Username = "ada", Email = "ada@example.com", FirstName = "Ada", Karma = 7 };

var created = await avatar.SaveAsync();          // create (no ActionHash yet)
// avatar.ActionHash and avatar.EntryHash are set; avatar.State == HoloNETEntryState.NoChanges

var loaded = new Avatar(client);
await loaded.LoadAsync(avatar.ActionHash);        // loaded.FirstName == "Ada"

loaded.FirstName = "Grace";                       // loaded.HasEntryChanged() == true
await loaded.SaveAsync();                         // update: new ActionHash, PreviousVersionActionHash = the old one

await loaded.DeleteAsync();
```

- **Errors.** Every call returns a result with `IsError` and `Message`, and also raises `OnSaved`, `OnLoaded`, `OnDeleted` or `OnError`.
- **No exceptions.** Zome errors never throw.
- **Loading by a field.** `LoadByCustomFieldAsync` and `DeleteByCustomFieldAsync` call a zome function that looks the entry up by a field, for example `get_avatar_by_id`.

### Change tracking

| Member | Meaning |
|---|---|
| `HasEntryChanged()` / `IsChanged` | True when a property differs from what was last loaded or saved. |
| `State` | `NoChanges`, `Updated`, `AddedToCollection`, ... (`HoloNETEntryState`). |
| `OrginalEntry` / `OrginalKeyValuePairs` | The values as last loaded or saved. |
| `PreviousVersionActionHash` | The version before the last update. |

## 4. Audit entries

Extend `HoloNETAuditEntryBase` instead to get these fields, filled in for you:

- `Id`, `Version` and `IsActive`.
- `CreatedDate` and `CreatedBy`, `ModifiedDate` and `ModifiedBy`, `DeletedDate` and `DeletedBy`.
- `AuditEntries`, the list of changes.

```csharp
public class AuditedAvatar : HoloNETAuditEntryBase
{
    public AuditedAvatar(IHoloNETClientAppBase client)
        : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", client) { }

    [HolochainRustFieldName("username")] public string Username { get; set; }
    [HolochainRustFieldName("email")]    public string Email { get; set; }
}
```

Your Rust struct needs the matching fields (`id`, `version`, `created_date`, `created_by`, `modified_date`, `modified_by`, `deleted_date`, `deleted_by`, `is_active`). Turn individual features off with `IsVersionTrackingEnabled`, `IsAuditTrackingEnabled`, `IsActiveFlagEnabled`, `IsGenerateUniqueGuidIdEnabled` and `IsAuditAgentCreateModifyDeleteFieldsEnabled`.

## 5. Collections

A collection is a list of entries linked from an anchor, such as "all avatars in this guild".

```csharp
var guild = new HoloNETCollection<Avatar>(
    "oasis", "get_avatar_collection", "add_avatar_to_collection", "remove_avatar_from_collection",
    client, "batch_update_avatar_collection");

await guild.LoadCollectionAsync("guild-42");

// One at a time:
await guild.AddHoloNETEntryToCollectionAndSaveAsync(newAvatar, collectionAnchor: "guild-42");
await guild.RemoveHoloNETEntryFromCollectionAndSaveAsync(oldAvatar, "guild-42");

// Or change the list and save every change at once (adds, removes and edits), optionally as one zome call:
guild.Add(another);
guild.Remove(someone);
guild[0].FirstName = "Changed";
var saved = await guild.SaveAllChangesAsync(saveAsOneBatchOperation: true, collectionAnchor: "guild-42");
```

The collection zome functions take these shapes:

```rust
#[hdk_extern] pub fn get_avatar_collection(collection_anchor: String) -> ExternResult<Vec<Record>>
#[hdk_extern] pub fn add_avatar_to_collection(input: AvatarCollectionInput) -> ExternResult<()>        // { collection_anchor, entry_action_hash }
#[hdk_extern] pub fn remove_avatar_from_collection(input: AvatarCollectionInput) -> ExternResult<()>
#[hdk_extern] pub fn batch_update_avatar_collection(input: AvatarCollectionBatchInput) -> ExternResult<Vec<Record>>
//   { collection_anchor, added: Vec<Avatar>, removed: Vec<ActionHash>, updated: Vec<UpdateAvatarInput> }
```

The collection also tracks its own state:

- `EntriesAddedSinceLastSaved` and `EntriesRemovedSinceLastSaved` hold what changed.
- `IsChanges` is true while there are unsaved changes.
- Events fire for each step: `OnCollectionLoaded`, `OnCollectionSaved`, `OnHoloNETEntryAddedToCollection`, `OnHoloNETEntryRemovedFromCollection`.

`HoloNETObservableCollection<T>` is the same, but an `ObservableCollection` for WPF, MAUI and Uno data binding.

## 6. Tips

- **One client per agent.** Share one `IHoloNETClientAppAgent` between your entries and collections, rather than letting each create its own connection.
- **Version coverage.** The ORM is tested end to end on Holochain 0.7.0 and 0.7.1 (line coverage 93%).
- **Generating models.** WEB5 STAR can generate both the C# models and the Rust zome from one description. See the [STAR hApp generation guide](star-happ-generation.md).

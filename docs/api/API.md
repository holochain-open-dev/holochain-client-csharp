# HoloNET API API reference

Generated from `NextGenSoftware.Holochain.HoloNET.API` 4.6.0.0 (every public type and member, with its XML documentation where it has one). For guides and examples see the [documentation index](../README.md).

- **NextGenSoftware.Holochain.HoloNET.API**: [HoloNETAPI](#nextgensoftware-holochain-holonet-api-holonetapi), [HoloNETAPIDataChangedEventArgs](#nextgensoftware-holochain-holonet-api-holonetapidatachangedeventargs), [HoloNETAPIDNA](#nextgensoftware-holochain-holonet-api-holonetapidna), [HoloNETAPIDNAManager](#nextgensoftware-holochain-holonet-api-holonetapidnamanager), [HoloNETAPIHapp](#nextgensoftware-holochain-holonet-api-holonetapihapp), [HoloNETAPIResult&lt;T&gt;](#nextgensoftware-holochain-holonet-api-holonetapiresult-1), [HoloNETAPISignalEventArgs](#nextgensoftware-holochain-holonet-api-holonetapisignaleventargs)
- **NextGenSoftware.Holochain.HoloNET.API.Interfaces**: [IHoloNETAPIDNA](#nextgensoftware-holochain-holonet-api-interfaces-iholonetapidna)

## NextGenSoftware.Holochain.HoloNET.API

<a id="nextgensoftware-holochain-holonet-api-holonetapi"></a>
### HoloNETAPI (static class)

A simple key/value and object store on Holochain. Everything saved (Data and Objects) lives in one entry of the OASIS hApp, an avatar whose id is `StoreId`, under the fields holonet_api_data and holonet_api_objects (JSON). The entry's action hash is remembered in HoloNETAPIDNA.EntryHash so later saves update it.

| Member | Kind | Description |
|---|---|---|
| `IHoloNETAPIDNA HoloNETAPIDNA { get; } (static)` | property | The API's settings (store id, entry hashes), loaded from and saved to HoloNETAPIDNA.json. |
| `IHoloNETClientAppAgent HoloNETClient { get; } (static)` | property | The app connection the API uses once initialised (null before). |
| `IHoloNETClientAdmin HoloNETClientAdmin { get; } (static)` | property | The admin connection the API uses (installs the hApp and issues the app token). |
| `bool IsInitialized { get; } (static)` | property | True once HoloNET is connected and the hApp is ready. |
| `bool IsPerKeyStorage { get; } (static)` | property | True when the per-key store is in use (the default; HoloNETAPIDNA.StorageMode = "SingleEntry" keeps the old one-entry store). |
| `string LastError { get; } (static)` | property | The last error message, or null. |
| `string StoreId { get; } (static)` | property | The id of the entry the store is kept in: HoloNETAPIDNA.StoreId, or one derived from this agent's key. |
| `static Dictionary<string, string> Data` | field | The key/value store as last loaded or saved (SaveAllDataAsync saves it as a whole). |
| `static Dictionary<string, object> Objects` | field | The object store as last loaded or saved (SaveAllObjectsAsync saves it as a whole). |
| `EventHandler<HoloNETAPIDataChangedEventArgs> OnDataChanged` | event | Raised when another agent changes shared data in a namespace you subscribed to (SubscribeToChangesAsync). |
| `EventHandler<string> OnError` | event | Raised for every error (they used to be silently dropped). |
| `EventHandler<HoloNETAPISignalEventArgs> OnSignalReceived` | event | Raised when another agent sends this agent a message with SendSignalAsync: (sender's agent pub key, message). |
| `static void Configure(HoloNETDNA holoNETDNA)` | method | Set the HoloNET settings the API uses: which conductor (HolochainConductorAdminURI, or HolochainConductorMode = UseEmbedded with an .Embedded package), whether HoloNET starts / stops it, and logging (LogToConsole, LogToFile...). Call before the first API call (or after ShutdownAsync); the app connection the API makes inherits the same logging. |
| `static Task<HoloNETAPIResult<bool>> DeleteAllDataAsync(string zome = "oasis", string loadFunction = "get_avatar_by_id", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Delete every value (the objects are kept) and save the store. Result is true if there was anything to delete. |
| `static Task<HoloNETAPIResult<bool>> DeleteAllObjectsAsync(string zome = "oasis", string loadFunction = "get_avatar_by_id", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Delete every object (the values are kept) and save the store. Result is true if there was anything to delete. |
| `static HoloNETAPIResult<bool> DeleteData(string key)` | method | Sync version of DeleteDataAsync. |
| `static Task<HoloNETAPIResult<bool>> DeleteDataAsync(HoloNETAPIHapp hApp, string key)` | method | Delete one value and save the store, using this hApp. |
| `static Task<HoloNETAPIResult<bool>> DeleteDataAsync(string key, string zome = "oasis", string loadFunction = "get_avatar_by_id", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Delete one value and save the store. Result is true if the key existed. |
| `static HoloNETAPIResult<bool> DeleteObject(string key)` | method | Sync version of DeleteObjectAsync. |
| `static Task<HoloNETAPIResult<bool>> DeleteObjectAsync(HoloNETAPIHapp hApp, string key)` | method | Delete one object and save the store, using this hApp. |
| `static Task<HoloNETAPIResult<bool>> DeleteObjectAsync(string key, string zome = "oasis", string loadFunction = "get_avatar_by_id", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Delete one object and save the store. Result is true if the key existed. |
| `static HoloNETAPIResult<bool> DeleteSharedData(string ns, string key)` | method | Sync version of DeleteSharedDataAsync. |
| `static Task<HoloNETAPIResult<bool>> DeleteSharedDataAsync(string ns, string key)` | method | Delete a shared value. Subscribers get OnDataChanged (IsDeleted). |
| `static Task<HoloNETAPIResult<bool>> DeleteSharedObjectAsync(string ns, string key)` | method | Delete a shared object. Subscribers get OnDataChanged (IsDeleted). |
| `static HoloNETAPIResult<bool> Exists(string key, bool objects = false)` | method | Sync version of ExistsAsync. |
| `static Task<HoloNETAPIResult<bool>> ExistsAsync(string key, bool objects = false)` | method | True when a value (or an object, when objects is true) is saved under this key. |
| `static Task<bool> InitHoloNETAsync()` | method | Connect and install/attach the hApp the API was last initialised with (remembered in HoloNETAPIDNA), or the bundled OASIS hApp. Every one-line call does this for you on first use. Returns false with LastError on failure. |
| `static Task<bool> InitHoloNETAsync(HoloNETAPIHapp hApp)` | method | Connect and install/attach this hApp (installed app id, .happ path, role). |
| `static Task<bool> InitHoloNETAsync(string hAppId, string hAppPath, string hAppRole)` | method | Connect and install/attach the given hApp. Obsolete: use the HoloNETAPIHapp overload. |
| `static HoloNETAPIResult<List<string>> ListKeys(bool objects = false)` | method | Sync version of ListKeysAsync. |
| `static Task<HoloNETAPIResult<List<string>>> ListKeysAsync(bool objects = false)` | method | The keys saved in your store (values, or objects when objects is true). |
| `static Task<HoloNETAPIResult<List<string>>> ListSharedKeysAsync(string ns, bool objects = false)` | method | The shared keys in a namespace (values, or objects when objects is true). |
| `static Task<HoloNETAPIResult<Dictionary<string, string>>> LoadAllDataAsync(string zome = "oasis", string function = "get_avatar_by_id")` | method | Load the whole key/value store from the DHT into Data. |
| `static Task<HoloNETAPIResult<Dictionary<string, string>>> LoadAllDataAsync(HoloNETAPIHapp hApp, string zome = "oasis", string function = "get_avatar_by_id")` | method | Load the whole key/value store, using this hApp. |
| `static Task<HoloNETAPIResult<Dictionary<string, string>>> LoadAllDataAsync(string hAppId, string hAppPath, string hAppRole, string key, bool useCache = true, string zome = "oasis", string function = "get_avatar_by_id")` | method | Load the whole key/value store from the DHT into Data. **(obsolete)** |
| `static Task<HoloNETAPIResult<Dictionary<string, object>>> LoadAllObjectsAsync(string zome = "oasis", string function = "get_avatar_by_id")` | method | Load the whole object store from the DHT into Objects. |
| `static Task<HoloNETAPIResult<Dictionary<string, object>>> LoadAllObjectsAsync(HoloNETAPIHapp hApp, string zome = "oasis", string function = "get_avatar_by_id")` | method | Load the whole object store, using this hApp. |
| `static Task<HoloNETAPIResult<Dictionary<string, object>>> LoadAllObjectsAsync(string hAppId, string hAppPath, string hAppRole, string key, bool useCache = true, string zome = "oasis", string function = "get_avatar_by_id")` | method | Load the whole object store from the DHT into Objects. **(obsolete)** |
| `static Task<HoloNETAPIResult<Dictionary<string, string>>> LoadAllSharedDataAsync(string ns)` | method | Every shared value in a namespace. |
| `static HoloNETAPIResult<string> LoadData(string key, bool useCache = true)` | method | Sync version of LoadDataAsync. |
| `static HoloNETAPIResult<T> LoadData<T>(string key, bool useCache = true)` | method | Sync version of LoadDataAsync<T>. |
| `static Task<HoloNETAPIResult<T>> LoadDataAsync<T>(string key, bool useCache = true)` | method | Load a value converted to T (numbers, bools, dates, Guids, enums, or JSON for anything else). |
| `static Task<HoloNETAPIResult<string>> LoadDataAsync(string key, bool useCache = true, string zome = "oasis", string function = "get_avatar_by_id")` | method | Load one value by key (from the cache unless useCache is false). |
| `static Task<HoloNETAPIResult<string>> LoadDataAsync(HoloNETAPIHapp hApp, string key, bool useCache = true, string zome = "oasis", string function = "get_avatar_by_id")` | method | Load one value by key, using this hApp. |
| `static Task<HoloNETAPIResult<string>> LoadDataAsync(string hAppId, string hAppPath, string hAppRole, string key, bool useCache = true, string zome = "oasis", string function = "get_avatar_by_id")` | method | Load one value by key (from the cache unless useCache is false). **(obsolete)** |
| `static HoloNETAPIResult<T> LoadObject<T>(string key, bool useCache = true)` | method | Sync version of LoadObjectAsync<T>. |
| `static Task<HoloNETAPIResult<T>> LoadObjectAsync<T>(string key, bool useCache = true, string zome = "oasis", string function = "get_avatar_by_id")` | method | Load one object by key, deserialised as T (from the cache unless useCache is false). |
| `static Task<HoloNETAPIResult<T>> LoadObjectAsync<T>(HoloNETAPIHapp hApp, string key, bool useCache = true, string zome = "oasis", string function = "get_avatar_by_id")` | method | Load one object by key, using this hApp. |
| `static Task<HoloNETAPIResult<T>> LoadObjectAsync<T>(string hAppId, string hAppPath, string hAppRole, string key, bool useCache = true, string zome = "oasis", string function = "get_avatar_by_id")` | method | Load a value converted to T (numbers, bools, dates, Guids, enums, or JSON for anything else). **(obsolete)** |
| `static HoloNETAPIResult<string> LoadSharedData(string ns, string key)` | method | Sync version of LoadSharedDataAsync. |
| `static Task<HoloNETAPIResult<string>> LoadSharedDataAsync(string ns, string key)` | method | Load a shared value (always from the DHT: anyone may have changed it). |
| `static Task<HoloNETAPIResult<T>> LoadSharedDataAsync<T>(string ns, string key)` | method | Load a shared value converted to T. |
| `static Task<HoloNETAPIResult<T>> LoadSharedObjectAsync<T>(string ns, string key)` | method | Load a shared object as T. |
| `static string ReceiveSignal()` | method | The last message received through OnSignalReceived ("" if none yet). |
| `static Task<HoloNETAPIResult<bool>> SaveAllDataAsync(string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Save the whole key/value store (Data) to the DHT. |
| `static Task<HoloNETAPIResult<bool>> SaveAllDataAsync(HoloNETAPIHapp hApp, string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Save the whole key/value store, using this hApp. |
| `static Task<HoloNETAPIResult<bool>> SaveAllDataAsync(string hAppId, string hAppPath, string hAppRole, string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Save the whole key/value store (Data) to the DHT. **(obsolete)** |
| `static Task<HoloNETAPIResult<bool>> SaveAllObjectsAsync(string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Save the whole object store (Objects) to the DHT. |
| `static Task<HoloNETAPIResult<bool>> SaveAllObjectsAsync(HoloNETAPIHapp hApp, string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Save the whole object store, using this hApp. |
| `static Task<HoloNETAPIResult<bool>> SaveAllObjectsAsync(string hAppId, string hAppPath, string hAppRole, string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Save the whole object store (Objects) to the DHT. **(obsolete)** |
| `static HoloNETAPIResult<bool> SaveData(string key, string value)` | method | Sync version of SaveDataAsync. |
| `static Task<HoloNETAPIResult<bool>> SaveDataAsync<T>(string key, T value)` | method | Save any value as text (numbers and dates in the invariant culture, so they load back the same everywhere). |
| `static Task<HoloNETAPIResult<bool>> SaveDataAsync(string key, string value, string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Set one value and save the store. |
| `static Task<HoloNETAPIResult<bool>> SaveDataAsync(HoloNETAPIHapp hApp, string key, string value, string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Set one value and save the store, using this hApp. |
| `static Task<HoloNETAPIResult<bool>> SaveDataAsync(string hAppId, string hAppPath, string hAppRole, string key, string value, string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Set one value and save the store. **(obsolete)** |
| `static HoloNETAPIResult<bool> SaveObject(string key, object value)` | method | Sync version of SaveObjectAsync. |
| `static Task<HoloNETAPIResult<bool>> SaveObjectAsync(string key, object objectToSave, string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Set one object (serialised as JSON) and save the store. |
| `static Task<HoloNETAPIResult<bool>> SaveObjectAsync(HoloNETAPIHapp hApp, string key, object objectToSave, string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Set one object and save the store, using this hApp. |
| `static Task<HoloNETAPIResult<bool>> SaveObjectAsync(string hAppId, string hAppPath, string hAppRole, string key, object objectToSave, string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Set one object (serialised as JSON) and save the store. **(obsolete)** |
| `static HoloNETAPIResult<bool> SaveSharedData(string ns, string key, string value)` | method | Sync version of SaveSharedDataAsync. |
| `static Task<HoloNETAPIResult<bool>> SaveSharedDataAsync(string ns, string key, string value)` | method | Save a value every agent in the app can read (in this namespace). Subscribers get OnDataChanged. |
| `static Task<HoloNETAPIResult<bool>> SaveSharedObjectAsync(string ns, string key, object value)` | method | Save an object (as JSON) every agent in the app can read (in this namespace). Subscribers get OnDataChanged. |
| `static bool SendSignal()` | method | Kept for compatibility: sends nothing (it never did). Use SendSignalAsync. **(obsolete)** |
| `static HoloNETAPIResult<int> SendSignal(string message, IEnumerable<string> toAgentPubKeys)` | method | Sync version of SendSignalAsync. |
| `static Task<HoloNETAPIResult<int>> SendSignalAsync(string message, IEnumerable<string> toAgentPubKeys, string zome = "oasis")` | method | Send a text message straight to other agents (Holochain remote signals: delivered peer to peer, nothing stored, not guaranteed). They receive it through OnSignalReceived. Returns how many agents it was sent to. |
| `static HoloNETAPIResult<int> SendSignalToAll(string message)` | method | Sync version of SendSignalToAllAsync. |
| `static Task<HoloNETAPIResult<int>> SendSignalToAllAsync(string message)` | method | Send a message to every agent using the HoloNET API in this app (remote signals; not stored, not guaranteed). Result: how many it went to. |
| `static Task ShutdownAsync()` | method | Disconnects the app and admin clients and forgets the cached store, so the next call initialises again. |
| `static Task<HoloNETAPIResult<bool>> SubscribeToChangesAsync(string ns)` | method | Get OnDataChanged whenever another agent changes shared data in this namespace. Subscribers find each other through the hApp (a zone link, like HyperNET), and changes are announced to them as remote signals. |
| `static Task<HoloNETAPIResult<bool>> UnsubscribeFromChangesAsync(string ns)` | method | Stop getting OnDataChanged for this namespace. |
| `static void UseConductor(string adminUri, bool logToConsole = true)` | method | Use a conductor you run yourself (HoloNET won't start or stop it), optionally without console logging. |

<a id="nextgensoftware-holochain-holonet-api-holonetapidatachangedeventargs"></a>
### HoloNETAPIDataChangedEventArgs (class)

Shared data changed (HoloNETAPI.OnDataChanged).

Inherits / implements: `EventArgs`

| Member | Kind | Description |
|---|---|---|
| `string ByAgentPubKey { get; set; }` | property | Who changed it. |
| `bool IsDeleted { get; set; }` | property | True when it was deleted. |
| `bool IsObject { get; set; }` | property | True for an object (SaveSharedObjectAsync), false for a value. |
| `string Key { get; set; }` | property | The key that changed. |
| `string Namespace { get; set; }` | property | The namespace it changed in. |

<a id="nextgensoftware-holochain-holonet-api-holonetapidna"></a>
### HoloNETAPIDNA (class)

Inherits / implements: `IHoloNETAPIDNA`

| Member | Kind | Description |
|---|---|---|
| `string EntryHash { get; set; }` | property | This is the hash generated by the Holochain Conductor for the Holochain Entry that the HoloNETEntryBaseClass maps onto. This will be empty until after the first call to the Save method. |
| `string HappPath { get; set; }` | property | The .happ that hApp is installed from. |
| `string InstalledAppId { get; set; }` | property | The hApp the API last initialised with (so one-line calls after a restart use it again). Empty: the bundled OASIS hApp. |
| `bool MigratedToPerKey { get; set; }` | property | True once a store saved by an older version (one entry) has been copied into per-key storage. |
| `string RoleName { get; set; }` | property | The role (DNA) in that hApp the store lives in. |
| `string StorageMode { get; set; }` | property | "PerKey" (the default: each key is its own entry) or "SingleEntry" (the whole store in one entry, as before 4.5). |
| `string StoreId { get; set; }` | property | The id of the entry the store is kept in. Empty: one derived from the agent's key. |

<a id="nextgensoftware-holochain-holonet-api-holonetapidnamanager"></a>
### HoloNETAPIDNAManager (static class)

| Member | Kind | Description |
|---|---|---|
| `IHoloNETAPIDNA HoloNETAPIDNA { get; set; } (static)` | property | The HoloNET API settings in use. |
| `bool IsLoaded { get; } (static)` | property | True once the settings have been loaded. |
| `static string HoloNETAPIDNAPath` | field | Where HoloNETAPIDNA.json is read from and saved to. |
| `static IHoloNETAPIDNA LoadDNA()` | method | Load the settings from HoloNETDNA.json (defaults if it doesn't exist). |
| `static IHoloNETAPIDNA LoadDNA(string holoNETDNAAPIPath)` | method | Load the settings from HoloNETDNA.json (defaults if it doesn't exist). |
| `static bool SaveDNA()` | method | Save the settings to HoloNETDNA.json. |
| `static bool SaveDNA(string holoNETAPIDNAPath, IHoloNETAPIDNA holoNETAPIDNA)` | method | Save the settings to HoloNETDNA.json. |

<a id="nextgensoftware-holochain-holonet-api-holonetapihapp"></a>
### HoloNETAPIHapp (class)

The hApp HoloNET API installs (if needed) and connects to: its installed app id, the path to its .happ bundle and its role name. Pass one to the HoloNETAPI overloads that take it to use a hApp other than the default OASIS one. (The older overloads that took these as three strings were ambiguous with the zome/function overloads, see HoloNETAPI.)

| Member | Kind | Description |
|---|---|---|
| `HoloNETAPIHapp()` | ctor | The default: the bundled OASIS hApp. |
| `HoloNETAPIHapp(string installedAppId, string hAppPath, string roleName)` | ctor | A specific hApp. |
| `string HappPath { get; set; }` | property | The .happ file to install it from. |
| `string InstalledAppId { get; set; }` | property | The installed app id (installed from HappPath if missing). |
| `string RoleName { get; set; }` | property | The role (DNA) in the hApp that has the store's zome functions. |

<a id="nextgensoftware-holochain-holonet-api-holonetapiresult-1"></a>
### HoloNETAPIResult&lt;T&gt; (class)

The outcome of a HoloNET API call.

| Member | Kind | Description |
|---|---|---|
| `bool IsSuccess { get; set; }` | property | True when the call worked. |
| `string Message { get; set; }` | property | Why it failed (when not IsSuccess). |
| `T Result { get; set; }` | property | The value (when IsSuccess). |
| `ZomeFunctionCallBackEventArgs ZomeCallResult { get; set; }` | property | The underlying zome call, if one was made. |

<a id="nextgensoftware-holochain-holonet-api-holonetapisignaleventargs"></a>
### HoloNETAPISignalEventArgs (class)

A message another agent sent with HoloNETAPI.SendSignalAsync.

Inherits / implements: `EventArgs`

| Member | Kind | Description |
|---|---|---|
| `string FromAgentPubKey { get; set; }` | property | The sender's agent public key (send a reply to it). |
| `string Message { get; set; }` | property | The message text. |

## NextGenSoftware.Holochain.HoloNET.API.Interfaces

<a id="nextgensoftware-holochain-holonet-api-interfaces-iholonetapidna"></a>
### IHoloNETAPIDNA (interface)

| Member | Kind | Description |
|---|---|---|
| `string EntryHash { get; set; }` | property | This is the hash generated by the Holochain Conductor for the Holochain Entry that the HoloNETEntryBaseClass maps onto. This will be empty until after the first call to the Save method. |
| `string HappPath { get; set; }` | property | The .happ that hApp is installed from. |
| `string InstalledAppId { get; set; }` | property | The installed app id. |
| `bool MigratedToPerKey { get; set; }` | property | True once a store saved by an older version (one entry) has been copied into per-key storage. |
| `string RoleName { get; set; }` | property | The role (DNA) in that hApp the store lives in. |
| `string StorageMode { get; set; }` | property | "PerKey" (the default: each key is its own entry) or "SingleEntry" (the whole store in one entry, as before 4.5). |
| `string StoreId { get; set; }` | property | The id of the entry the store is kept in. Empty: one derived from the agent's key. |


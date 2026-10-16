# HoloNET API API reference

Generated from `NextGenSoftware.Holochain.HoloNET.API` 4.4.1.0 (every public type and member, with its XML documentation where it has one). For guides and examples see the [documentation index](../README.md).

- **NextGenSoftware.Holochain.HoloNET.API**: [HoloNETAPI](#nextgensoftware-holochain-holonet-api-holonetapi), [HoloNETAPIDNA](#nextgensoftware-holochain-holonet-api-holonetapidna), [HoloNETAPIDNAManager](#nextgensoftware-holochain-holonet-api-holonetapidnamanager), [HoloNETAPIHapp](#nextgensoftware-holochain-holonet-api-holonetapihapp), [HoloNETAPIResult&lt;T&gt;](#nextgensoftware-holochain-holonet-api-holonetapiresult-1), [HoloNETAPISignalEventArgs](#nextgensoftware-holochain-holonet-api-holonetapisignaleventargs)
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
| `string LastError { get; } (static)` | property | The last error message, or null. |
| `string StoreId { get; } (static)` | property | The id of the entry the store is kept in: HoloNETAPIDNA.StoreId, or one derived from this agent's key. |
| `static Dictionary<string, string> Data` | field | The key/value store as last loaded or saved (SaveAllDataAsync saves it as a whole). |
| `static Dictionary<string, object> Objects` | field | The object store as last loaded or saved (SaveAllObjectsAsync saves it as a whole). |
| `EventHandler<string> OnError` | event | Raised for every error (they used to be silently dropped). |
| `EventHandler<HoloNETAPISignalEventArgs> OnSignalReceived` | event | Raised when another agent sends this agent a message with SendSignalAsync: (sender's agent pub key, message). |
| `static Task<bool> InitHoloNETAsync()` | method | Connect and install/attach the default hApp (the bundled OASIS hApp). Returns false with LastError on failure. |
| `static Task<bool> InitHoloNETAsync(HoloNETAPIHapp hApp)` | method | Connect and install/attach this hApp (installed app id, .happ path, role). |
| `static Task<bool> InitHoloNETAsync(string hAppId, string hAppPath, string hAppRole)` | method | Connect and install/attach the given hApp. Obsolete: use the HoloNETAPIHapp overload. |
| `static Task<HoloNETAPIResult<Dictionary<string, string>>> LoadAllDataAsync(string zome = "oasis", string function = "get_avatar_by_id")` | method | Load the whole key/value store from the DHT into Data. |
| `static Task<HoloNETAPIResult<Dictionary<string, string>>> LoadAllDataAsync(HoloNETAPIHapp hApp, string zome = "oasis", string function = "get_avatar_by_id")` | method | Load the whole key/value store, using this hApp. |
| `static Task<HoloNETAPIResult<Dictionary<string, string>>> LoadAllDataAsync(string hAppId, string hAppPath, string hAppRole, string key, bool useCache = true, string zome = "oasis", string function = "get_avatar_by_id")` | method | Load the whole key/value store from the DHT into Data. **(obsolete)** |
| `static Task<HoloNETAPIResult<Dictionary<string, object>>> LoadAllObjectsAsync(string zome = "oasis", string function = "get_avatar_by_id")` | method | Load the whole object store from the DHT into Objects. |
| `static Task<HoloNETAPIResult<Dictionary<string, object>>> LoadAllObjectsAsync(HoloNETAPIHapp hApp, string zome = "oasis", string function = "get_avatar_by_id")` | method | Load the whole object store, using this hApp. |
| `static Task<HoloNETAPIResult<Dictionary<string, object>>> LoadAllObjectsAsync(string hAppId, string hAppPath, string hAppRole, string key, bool useCache = true, string zome = "oasis", string function = "get_avatar_by_id")` | method | Load the whole object store from the DHT into Objects. **(obsolete)** |
| `static Task<HoloNETAPIResult<string>> LoadDataAsync(string key, bool useCache = true, string zome = "oasis", string function = "get_avatar_by_id")` | method | Load one value by key (from the cache unless useCache is false). |
| `static Task<HoloNETAPIResult<string>> LoadDataAsync(HoloNETAPIHapp hApp, string key, bool useCache = true, string zome = "oasis", string function = "get_avatar_by_id")` | method | Load one value by key, using this hApp. |
| `static Task<HoloNETAPIResult<string>> LoadDataAsync(string hAppId, string hAppPath, string hAppRole, string key, bool useCache = true, string zome = "oasis", string function = "get_avatar_by_id")` | method | Load one value by key (from the cache unless useCache is false). **(obsolete)** |
| `static Task<HoloNETAPIResult<T>> LoadObjectAsync<T>(string key, bool useCache = true, string zome = "oasis", string function = "get_avatar_by_id")` | method | Load one object by key, deserialised as T (from the cache unless useCache is false). |
| `static Task<HoloNETAPIResult<T>> LoadObjectAsync<T>(HoloNETAPIHapp hApp, string key, bool useCache = true, string zome = "oasis", string function = "get_avatar_by_id")` | method | Load one object by key, using this hApp. |
| `static Task<HoloNETAPIResult<T>> LoadObjectAsync<T>(string hAppId, string hAppPath, string hAppRole, string key, bool useCache = true, string zome = "oasis", string function = "get_avatar_by_id")` | method | Load one object by key, deserialised as T (from the cache unless useCache is false). **(obsolete)** |
| `static string ReceiveSignal()` | method | The last message received through OnSignalReceived ("" if none yet). |
| `static Task<HoloNETAPIResult<bool>> SaveAllDataAsync(string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Save the whole key/value store (Data) to the DHT. |
| `static Task<HoloNETAPIResult<bool>> SaveAllDataAsync(HoloNETAPIHapp hApp, string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Save the whole key/value store, using this hApp. |
| `static Task<HoloNETAPIResult<bool>> SaveAllDataAsync(string hAppId, string hAppPath, string hAppRole, string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Save the whole key/value store (Data) to the DHT. **(obsolete)** |
| `static Task<HoloNETAPIResult<bool>> SaveAllObjectsAsync(string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Save the whole object store (Objects) to the DHT. |
| `static Task<HoloNETAPIResult<bool>> SaveAllObjectsAsync(HoloNETAPIHapp hApp, string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Save the whole object store, using this hApp. |
| `static Task<HoloNETAPIResult<bool>> SaveAllObjectsAsync(string hAppId, string hAppPath, string hAppRole, string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Save the whole object store (Objects) to the DHT. **(obsolete)** |
| `static Task<HoloNETAPIResult<bool>> SaveDataAsync(string key, string value, string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Set one value and save the store. |
| `static Task<HoloNETAPIResult<bool>> SaveDataAsync(HoloNETAPIHapp hApp, string key, string value, string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Set one value and save the store, using this hApp. |
| `static Task<HoloNETAPIResult<bool>> SaveDataAsync(string hAppId, string hAppPath, string hAppRole, string key, string value, string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Set one value and save the store. **(obsolete)** |
| `static Task<HoloNETAPIResult<bool>> SaveObjectAsync(string key, object objectToSave, string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Set one object (serialised as JSON) and save the store. |
| `static Task<HoloNETAPIResult<bool>> SaveObjectAsync(HoloNETAPIHapp hApp, string key, object objectToSave, string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Set one object and save the store, using this hApp. |
| `static Task<HoloNETAPIResult<bool>> SaveObjectAsync(string hAppId, string hAppPath, string hAppRole, string key, object objectToSave, string zome = "oasis", string createFunction = "create_entry_avatar", string updateFunction = "update_entry_avatar")` | method | Set one object (serialised as JSON) and save the store. **(obsolete)** |
| `static bool SendSignal()` | method | Kept for compatibility: sends nothing (it never did). Use SendSignalAsync. **(obsolete)** |
| `static Task<HoloNETAPIResult<int>> SendSignalAsync(string message, IEnumerable<string> toAgentPubKeys, string zome = "oasis")` | method | Send a text message straight to other agents (Holochain remote signals: delivered peer to peer, nothing stored, not guaranteed). They receive it through OnSignalReceived. Returns how many agents it was sent to. |
| `static Task ShutdownAsync()` | method | Disconnects the app and admin clients and forgets the cached store, so the next call initialises again. |

<a id="nextgensoftware-holochain-holonet-api-holonetapidna"></a>
### HoloNETAPIDNA (class)

Inherits / implements: `IHoloNETAPIDNA`

| Member | Kind | Description |
|---|---|---|
| `string EntryHash { get; set; }` | property | This is the hash generated by the Holochain Conductor for the Holochain Entry that the HoloNETEntryBaseClass maps onto. This will be empty until after the first call to the Save method. |
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
| `string StoreId { get; set; }` | property | The id of the entry the store is kept in. Empty: one derived from the agent's key. |


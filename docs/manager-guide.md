# HoloNET Manager guide

HoloNET Manager is a Windows desktop app (WPF) for running a Holochain conductor and managing hApps. It shows off what HoloNET Client and ORM can do, and its backend and UI components can be reused in your own .NET hApps. It works with Holochain 0.7.0 and 0.7.1.

## Getting it

- **Installer:** download it from the [HoloNET GitHub releases](https://github.com/holochain-open-dev/holochain-client-csharp/releases).
- **NuGet:** `NextGenSoftware.Holochain.HoloNET.Manager`, if you want to reuse its components.
- **Source:** [HoloNET-Manager](https://github.com/NextGenSoftwareUK/HoloNET-Manager). Open the solution and run `NextGenSoftware.Holochain.HoloNET.Manager`.

## First run

On start, the Manager:

1. Starts its embedded Holochain conductor.
2. Connects the admin client.
3. Lists the installed hApps.

The status bar and the log panel (**Show Logs**) show what it's doing. To use a conductor you already run, see [Settings](#settings).

## The main window

**Installed hApps** shows the hApps installed in the conductor:

- their status (enabled, disabled, paused);
- the app interface port;
- the agent and DNA;
- whether the Manager is connected to them.

Select a hApp to use the buttons that act on it.

| Button | What it does |
|---|---|
| **Install hApp** | Pick a `.happ` file. The Manager installs it, enables it, grants signing, attaches an interface, issues the app token and connects. |
| **Enable / Disable / Uninstall** | Changes the selected hApp's state. |
| **Refresh hApps** | Re-lists what's installed. |
| **Call Zome Function** | Calls a zome function on the selected hApp and shows the result. |
| **List DNAs / List Cell Ids / List Attached Interfaces** | Shows what the conductor has. |
| **Get DNA Definition** | Shows the selected DNA's integrity and coordinator zomes, with their dependencies and wasm hashes. |
| **Get Agent Info** | Shows the selected cell's agent infos (the peer records other conductors use to reach you). |
| **Add Agent Info** | Adds agent infos to the conductor's peer store, for example ones copied from another conductor's Get Agent Info. Pick a text file with one encoded agent info per line. |
| **Get Storage Info** | Shows how much disk each app's databases use. |
| **Dump State / Dump Full State** | Shows the selected cell's source chain and DHT state as JSON (for debugging). |
| **Dump Network Stats / Dump Network Metrics** | Shows the conductor's networking statistics. |
| **Update Coordinators** | Hot-swaps the selected DNA's coordinator zomes from a coordinator bundle, without changing its integrity zomes or data. |
| **Graft Records** | Grafts records (a JSON array, for example saved from Dump State) onto the selected cell's source chain, with validation. This is an advanced recovery tool. |
| **Delete Clone Cell** | Deletes the selected clone cell, after asking you to confirm. Holochain requires the clone to be disabled first. |
| **Reboot** | Restarts the conductor and reconnects. |
| **Disconnect** | Disconnects from the selected hApp. |

The results of these calls appear in the log panel. **Clear Log** clears it, and **Show Detailed Logs** adds the raw requests and responses.

## The ORM demos

These run against the bundled OASIS demo hApp:

- **HoloNET Entry** demonstrates `HoloNETEntryBase`: create, load, update and delete an avatar, with change tracking.
- **HoloNET Collection** demonstrates `HoloNETObservableCollection`: a data-bound list of avatars in a collection, which you can add to, remove from, edit and save all at once.
- **View Data Entries** browses what's been saved.

These demos are the quickest way to see what the [ORM](orm-guide.md) does before you write code.

## Settings

- **Show Advanced Admin Tools.** The diagnostic and recovery buttons (agent info, storage, dumps, coordinators, graft records, clone cells) are hidden until you tick this.
- **Conductor settings.** The popup sets whether the conductor starts with the Manager (**Auto Start Holochain Conductor**), whether its console window shows (**Show Holochain Conductor Window**) and whether it stops when you close the Manager (**Auto Shutdown Holochain Conductor**).
- **Open DNA.** Opens `HoloNETDNA.json` to change anything else, for example the admin URI if you'd rather use a conductor you already run.
- **Shared connections.** The **Shared Connection** column shows which hApps share one app connection and which have their own.

## Tested

The Manager has live UI and logic tests against real 0.7.0 and 0.7.1 conductors (`NextGenSoftware.Holochain.HoloNET.Manager.Tests`). They cover installing, connecting, the ORM demos, reboot and shared and separate connections. See the [Testing guide](../INTEGRATION_TESTING.md).

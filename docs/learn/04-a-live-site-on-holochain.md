# A live web site on Holochain

**You'll build:** an ASP.NET Core site with a guestbook and a leaderboard that every visitor shares, stored on Holochain and updated instantly in every open browser.
**Time:** 25 minutes. **You need:** a running conductor.
**Finished code:** [`examples/06-WebSite`](https://github.com/holochain-open-dev/holochain-client-csharp/blob/main/examples/06-WebSite/Program.cs).

## 1. Create the site

```bash
dotnet new web -n Guestbook
cd Guestbook
dotnet add package NextGenSoftware.Holochain.HoloNET.API
```

## 2. Shared data: everyone sees it

Your own HoloNET API data is private to your agent. **Shared data** lives in a *namespace* that every agent in the app can read and write:

```csharp
using NextGenSoftware.Holochain.HoloNET.API;

HoloNETAPI.UseConductor("ws://localhost:7777", logToConsole: false);
var app = WebApplication.Create(args);

app.MapPost("/api/guestbook", async (Entry e) =>
{
    string key = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
    await HoloNETAPI.SaveSharedObjectAsync("guestbook", key, e);     // stored on Holochain
    return Results.Ok();
});

app.MapGet("/api/guestbook", async () =>
{
    var keys = (await HoloNETAPI.ListSharedKeysAsync("guestbook", objects: true)).Result;
    var entries = new List<Entry>();
    foreach (var key in keys.OrderByDescending(k => k))
        entries.Add((await HoloNETAPI.LoadSharedObjectAsync<Entry>("guestbook", key)).Result);
    return entries;
});

app.Run();

public record Entry(string Name, string Message);
```

## 3. Live updates: hear when anyone changes it

Subscribe to the namespace. `OnDataChanged` fires whenever another agent changes it, whether that's another copy of your site or another app on Holochain. Push that to the browsers with Server-Sent Events:

```csharp
await HoloNETAPI.SubscribeToChangesAsync("guestbook");
HoloNETAPI.OnDataChanged += (s, e) => NotifyBrowsers(e.Namespace);   // e.Key, e.ByAgentPubKey, e.IsDeleted
```

The finished example has the whole Server-Sent Events endpoint and a page that re-loads the guestbook whenever it hears `guestbook`, so you can copy it from there.

## 4. Run it twice

Start it twice (on two ports, or on two machines in the same Holochain network) and open both pages. Sign the guestbook in one, and it appears in the other.

## What just happened

- **No database server.** Each copy of the site runs its own Holochain agent, and they share the namespace through the hApp's DHT.
- **Notifications** travel directly from agent to agent as Holochain remote signals.
- **Anyone can write** to a namespace. If only some agents should be allowed to change something, add validation rules to your own hApp.

## Next

- [Real-time multiplayer with HyperNET](05-multiplayer-with-hypernet.md), for traffic too fast for the DHT.
- The [API guide](https://docs.holonet.oasisomniverse.one/api-guide) covers shared objects, broadcasts and messages.

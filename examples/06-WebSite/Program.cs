// HoloNET example 06: a web site on Holochain - a live guestbook and leaderboard
//
// An ASP.NET Core site whose data lives on Holochain through HoloNET API shared data: every visitor (and every copy of
// the site, on any machine in the same Holochain network) sees the same guestbook and leaderboard. When anyone changes
// them, HoloNET API's change notifications push the update to every open browser instantly (Server-Sent Events).
//
//   GET  /                     the page (wwwroot/index.html)
//   GET  /api/guestbook        every message
//   POST /api/guestbook        { "name": "...", "message": "..." }   - one line: HoloNETAPI.SaveSharedObjectAsync
//   GET  /api/leaderboard      the scores
//   POST /api/score            { "name": "...", "score": 123 }
//   GET  /api/events           live updates (OnDataChanged -> text/event-stream)
//
// Run: start a conductor (examples/start-conductor.sh), then   dotnet run --project examples/06-WebSite   and open the URL it prints.
// Docs: https://docs.holonet.oasisomniverse.one/api-guide#shared-data

using System.Collections.Concurrent;
using System.Text.Json;
using HoloNET.Examples;
using NextGenSoftware.Holochain.HoloNET.API;

const string Guestbook = "guestbook";     // shared-data namespaces
const string Leaderboard = "leaderboard";
bool smoke = Environment.GetEnvironmentVariable("HOLONET_EXAMPLE_SMOKE") == "1";

// ---- HoloNET API: the conductor you started, this site's own app id, and change notifications ----------------------
HoloNETAPI.UseConductor(ExampleSetup.AdminUri, logToConsole: false);
string appId = smoke ? ExampleSetup.UniqueAppId("example-site") : "holonet-example-site";
HoloNETAPIDNAManager.HoloNETAPIDNAPath = Path.Combine(smoke ? Path.GetTempPath() : AppContext.BaseDirectory, appId + ".json");
ExampleSetup.Check(await HoloNETAPI.InitHoloNETAsync(new HoloNETAPIHapp(appId, ExampleSetup.HappPath, "oasis")), "initialise HoloNET API: " + HoloNETAPI.LastError);
await HoloNETAPI.SubscribeToChangesAsync(Guestbook);
await HoloNETAPI.SubscribeToChangesAsync(Leaderboard);

// Every open browser has a queue; a change (from this site or any other agent) is pushed to all of them.
var browsers = new ConcurrentDictionary<Guid, BlockingCollection<string>>();
void Push(string what) { foreach (var q in browsers.Values) q.TryAdd(what); }
HoloNETAPI.OnDataChanged += (s, e) => Push(e.Namespace);   // another agent changed something

// ---- The site ----------------------------------------------------------------------------------------------------
var builder = WebApplication.CreateBuilder(args);
builder.Logging.SetMinimumLevel(LogLevel.Warning);
if (smoke) builder.WebHost.UseUrls("http://127.0.0.1:0");
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

// The guestbook: one shared object per message, keyed by time so it sorts naturally.
app.MapGet("/api/guestbook", async () =>
{
    var keys = (await HoloNETAPI.ListSharedKeysAsync(Guestbook, objects: true)).Result ?? new List<string>();
    var entries = new List<GuestbookEntry>();
    foreach (string key in keys.OrderByDescending(k => k).Take(50))
        if ((await HoloNETAPI.LoadSharedObjectAsync<GuestbookEntry>(Guestbook, key)).Result is GuestbookEntry entry)
            entries.Add(entry);
    return Results.Ok(entries);
});

app.MapPost("/api/guestbook", async (GuestbookEntry entry) =>
{
    if (string.IsNullOrWhiteSpace(entry.Name) || string.IsNullOrWhiteSpace(entry.Message))
        return Results.BadRequest("name and message are required");
    entry.At = DateTime.UtcNow;
    string key = entry.At.ToString("yyyyMMddHHmmssfff") + "-" + Guid.NewGuid().ToString("N").Substring(0, 4);
    var saved = await HoloNETAPI.SaveSharedObjectAsync(Guestbook, key, entry);   // the one line that stores it on Holochain
    Push(Guestbook);
    return saved.IsSuccess ? Results.Ok() : Results.Problem(saved.Message);
});

// The leaderboard: one shared value per player (their best score).
app.MapGet("/api/leaderboard", async () =>
{
    var scores = (await HoloNETAPI.LoadAllSharedDataAsync(Leaderboard)).Result ?? new Dictionary<string, string>();
    return Results.Ok(scores.Select(kv => new { name = kv.Key, score = long.TryParse(kv.Value, out long v) ? v : 0 }).OrderByDescending(s => s.score));
});

app.MapPost("/api/score", async (ScoreSubmission s) =>
{
    if (string.IsNullOrWhiteSpace(s.Name))
        return Results.BadRequest("name is required");
    var best = await HoloNETAPI.LoadSharedDataAsync<long>(Leaderboard, s.Name);
    if (best.IsSuccess && best.Result >= s.Score)
        return Results.Ok(new { kept = best.Result });
    var saved = await HoloNETAPI.SaveSharedDataAsync(Leaderboard, s.Name, s.Score.ToString());
    Push(Leaderboard);
    return saved.IsSuccess ? Results.Ok(new { kept = s.Score }) : Results.Problem(saved.Message);
});

// Live updates: Server-Sent Events, one "data: <namespace>" line per change.
app.MapGet("/api/events", async (HttpContext http) =>
{
    http.Response.Headers.ContentType = "text/event-stream";
    var id = Guid.NewGuid();
    var queue = browsers[id] = new BlockingCollection<string>();
    try
    {
        await http.Response.WriteAsync("data: hello\n\n");
        await http.Response.Body.FlushAsync();
        while (!http.RequestAborted.IsCancellationRequested)
        {
            if (queue.TryTake(out string what, 1000, http.RequestAborted))
            {
                await http.Response.WriteAsync($"data: {what}\n\n");
                await http.Response.Body.FlushAsync();
            }
        }
    }
    catch (OperationCanceledException) { }
    finally { browsers.TryRemove(id, out _); }
});

if (!smoke)
{
    app.Lifetime.ApplicationStarted.Register(() => Console.WriteLine("HoloNET example site running at: " + string.Join(", ", app.Urls)));
    await app.RunAsync();
    await HoloNETAPI.ShutdownAsync();
    return;
}

// ---- Smoke test (HOLONET_EXAMPLE_SMOKE=1, used by scripts/run-live-tests.sh): run the site, use it, stop ---------
await app.StartAsync();
string url = app.Urls.First();
using var http = new HttpClient { BaseAddress = new Uri(url) };
try
{
    ExampleSetup.Check((await http.GetStringAsync("/")).Contains("HoloNET"), "serve the page");
    ExampleSetup.Check((await http.PostAsJsonAsync("/api/guestbook", new GuestbookEntry { Name = "Ada", Message = "Hello from Holochain" })).IsSuccessStatusCode, "post to the guestbook");
    var entries = JsonSerializer.Deserialize<List<GuestbookEntry>>(await http.GetStringAsync("/api/guestbook"), new JsonSerializerOptions(JsonSerializerDefaults.Web));
    ExampleSetup.Check(entries.Any(e => e.Message == "Hello from Holochain"), "read the guestbook back");
    ExampleSetup.Check((await http.PostAsJsonAsync("/api/score", new ScoreSubmission { Name = "Ada", Score = 42 })).IsSuccessStatusCode, "post a score");
    ExampleSetup.Check((await http.GetStringAsync("/api/leaderboard")).Contains("42"), "read the leaderboard");
    Console.WriteLine($"Smoke test passed: {entries.Count} guestbook entr{(entries.Count == 1 ? "y" : "ies")}, leaderboard has Ada at 42.");
}
finally
{
    await app.StopAsync();
    await HoloNETAPI.ShutdownAsync();
    var admin = new NextGenSoftware.Holochain.HoloNET.Client.HoloNETClientAdmin(ExampleSetup.ConductorDNA());
    if ((await admin.ConnectAsync(ExampleSetup.AdminUri)).IsConnected) { await admin.UninstallAppAsync(appId); await admin.DisconnectAsync(); }
    try { File.Delete(HoloNETAPIDNAManager.HoloNETAPIDNAPath); } catch { }
}

public class GuestbookEntry
{
    public string Name { get; set; }
    public string Message { get; set; }
    public DateTime At { get; set; }
}

public class ScoreSubmission
{
    public string Name { get; set; }
    public long Score { get; set; }
}

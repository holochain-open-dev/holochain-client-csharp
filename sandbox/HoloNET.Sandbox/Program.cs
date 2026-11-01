// The HoloNET sandbox service. The page at sandbox.holonet.oasisomniverse.one calls it, and it serves the same page itself when you run it locally.
//
// What it lets a visitor do, with the HoloNET API on a shared Holochain conductor:
//   - a private key-value store (each visitor's keys are prefixed with their session id, so visitors never see each other's data),
//   - a shared message board every visitor reads and writes (HoloNET API shared data),
//   - one-click demos of the API, Client, ORM and HyperNET (Demos.cs).
// It never runs code a visitor sends: every operation is fixed here, with limits on key names, sizes, counts and request rates.
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading.RateLimiting;
using HoloNETSandbox;
using NextGenSoftware.Holochain.HoloNET.API;

const int MaxKeysPerSession = 25, MaxValueLength = 500, MaxBoardMessages = 100, SessionIdleMinutes = 30;
string adminUri = Environment.GetEnvironmentVariable("HOLONET_ADMIN_URI") ?? "ws://localhost:7777";
string happPath = Environment.GetEnvironmentVariable("HOLONET_HAPP_PATH") ?? Path.Combine(AppContext.BaseDirectory, "oasis.happ");
string[] origins = (Environment.GetEnvironmentVariable("SANDBOX_ORIGINS") ?? "https://sandbox.holonet.oasisomniverse.one").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .SetIsOriginAllowed(origin => origins.Contains(origin) || new Uri(origin).IsLoopback)
    .AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    string Ip(HttpContext c) => c.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    o.AddPolicy("api", c => RateLimitPartition.GetFixedWindowLimiter(Ip(c), _ => new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromMinutes(1) }));
    o.AddPolicy("demo", c => RateLimitPartition.GetFixedWindowLimiter(Ip(c), _ => new FixedWindowRateLimiterOptions { PermitLimit = 6, Window = TimeSpan.FromMinutes(1) }));
});

var app = builder.Build();
app.UseCors();
app.UseRateLimiter();
app.UseDefaultFiles();
app.UseStaticFiles();

HoloNETAPI.UseConductor(adminUri, logToConsole: false);
var sessions = new ConcurrentDictionary<string, SandboxSession>();
var apiLock = new SemaphoreSlim(1, 1);        // the HoloNET API is one static client: one call at a time keeps it simple and safe
var demoSlots = new SemaphoreSlim(2, 2);      // at most two demos at once: each installs apps on the conductor
var keyPattern = new Regex("^[A-Za-z0-9._-]{1,40}$", RegexOptions.Compiled);
var sessionPattern = new Regex("^[a-f0-9]{16}$", RegexOptions.Compiled);

SandboxSession Touch(string sid)
{
    if (!sessionPattern.IsMatch(sid ?? "") || !sessions.TryGetValue(sid, out var s)) return null;
    s.LastUsed = DateTime.UtcNow;
    return s;
}

// ---- status and sessions -----------------------------------------------------------------------------------------------
app.MapGet("/api/status", async () =>
{
    bool ready = false; string message = null;
    await apiLock.WaitAsync();
    try { ready = (await HoloNETAPI.ExistsAsync("sandbox-ping")).IsSuccess; message = ready ? null : "The conductor did not answer."; }
    catch (Exception ex) { message = ex.Message; }
    finally { apiLock.Release(); }
    return Results.Json(new { ready, message, sessions = sessions.Count, limits = new { maxKeysPerSession = MaxKeysPerSession, maxValueLength = MaxValueLength, sessionIdleMinutes = SessionIdleMinutes, maxBoardMessages = MaxBoardMessages } });
}).RequireRateLimiting("api");

app.MapPost("/api/session", () =>
{
    if (sessions.Count >= 500) return Results.Json(new { error = "The sandbox is busy. Try again in a few minutes." }, statusCode: 503);
    string sid = Guid.NewGuid().ToString("N")[..16];
    sessions[sid] = new SandboxSession();
    return Results.Json(new { sessionId = sid, idleMinutes = SessionIdleMinutes });
}).RequireRateLimiting("api");

// ---- a visitor's private store -----------------------------------------------------------------------------------------
app.MapGet("/api/store/{sid}", async (string sid) =>
{
    var s = Touch(sid);
    if (s == null) return Results.Json(new { error = "Unknown or expired session. Start a new one." }, statusCode: 404);
    var items = new List<object>();
    await apiLock.WaitAsync();
    try
    {
        foreach (string key in s.Keys.OrderBy(k => k))
        {
            var r = await HoloNETAPI.LoadDataAsync(sid + "-" + key);
            items.Add(new { key, value = r.IsSuccess ? r.Result : null });
        }
    }
    finally { apiLock.Release(); }
    return Results.Json(new { items });
}).RequireRateLimiting("api");

app.MapPut("/api/store/{sid}/{key}", async (string sid, string key, SaveRequest body) =>
{
    var s = Touch(sid);
    if (s == null) return Results.Json(new { error = "Unknown or expired session. Start a new one." }, statusCode: 404);
    if (!keyPattern.IsMatch(key ?? "")) return Results.Json(new { error = "A key is 1 to 40 letters, digits, dots, dashes or underscores." }, statusCode: 400);
    string value = body?.Value ?? "";
    if (value.Length > MaxValueLength) return Results.Json(new { error = $"A value can be at most {MaxValueLength} characters." }, statusCode: 400);
    if (!s.Keys.Contains(key) && s.Keys.Count >= MaxKeysPerSession) return Results.Json(new { error = $"The sandbox keeps at most {MaxKeysPerSession} keys each." }, statusCode: 400);
    await apiLock.WaitAsync();
    try
    {
        var r = await HoloNETAPI.SaveDataAsync(sid + "-" + key, value);
        if (!r.IsSuccess) return Results.Json(new { error = r.Message }, statusCode: 502);
        s.Keys.Add(key);
    }
    finally { apiLock.Release(); }
    return Results.Json(new { saved = key });
}).RequireRateLimiting("api");

app.MapDelete("/api/store/{sid}/{key}", async (string sid, string key) =>
{
    var s = Touch(sid);
    if (s == null) return Results.Json(new { error = "Unknown or expired session. Start a new one." }, statusCode: 404);
    if (!keyPattern.IsMatch(key ?? "") || !s.Keys.Contains(key)) return Results.Json(new { error = "No such key." }, statusCode: 404);
    await apiLock.WaitAsync();
    try { await HoloNETAPI.DeleteDataAsync(sid + "-" + key); s.Keys.Remove(key); }
    finally { apiLock.Release(); }
    return Results.Json(new { deleted = key });
}).RequireRateLimiting("api");

// ---- the shared board (HoloNET API shared data: every visitor reads and writes it) ---------------------------------------
const string Board = "sandbox-board";
app.MapGet("/api/board", async () =>
{
    var messages = new List<object>();
    await apiLock.WaitAsync();
    try
    {
        var keys = (await HoloNETAPI.ListSharedKeysAsync(Board)).Result ?? new List<string>();
        foreach (string key in keys.OrderByDescending(k => k).Take(20))
        {
            string value = (await HoloNETAPI.LoadSharedDataAsync(Board, key)).Result ?? "";
            int bar = value.IndexOf('|');
            messages.Add(new { id = key, name = bar > 0 ? value[..bar] : "anonymous", text = bar >= 0 ? value[(bar + 1)..] : value });
        }
    }
    finally { apiLock.Release(); }
    return Results.Json(new { messages });
}).RequireRateLimiting("api");

app.MapPost("/api/board", async (BoardPost post) =>
{
    string name = (post?.Name ?? "").Trim(), text = (post?.Text ?? "").Trim();
    if (text.Length == 0 || text.Length > 280) return Results.Json(new { error = "A message is 1 to 280 characters." }, statusCode: 400);
    if (name.Length == 0) name = "anonymous";
    name = new string(name.Where(ch => ch != '|' && !char.IsControl(ch)).Take(24).ToArray());
    text = new string(text.Where(ch => !char.IsControl(ch)).ToArray());
    string key = DateTime.UtcNow.Ticks.ToString("D19") + "-" + Guid.NewGuid().ToString("N")[..4];
    await apiLock.WaitAsync();
    try
    {
        var r = await HoloNETAPI.SaveSharedDataAsync(Board, key, name + "|" + text);
        if (!r.IsSuccess) return Results.Json(new { error = r.Message }, statusCode: 502);
        var keys = (await HoloNETAPI.ListSharedKeysAsync(Board)).Result ?? new List<string>();
        foreach (string old in keys.OrderBy(k => k).Take(Math.Max(0, keys.Count - MaxBoardMessages)))
            await HoloNETAPI.DeleteSharedDataAsync(Board, old);
    }
    finally { apiLock.Release(); }
    return Results.Json(new { posted = key });
}).RequireRateLimiting("api");

// ---- the demos ----------------------------------------------------------------------------------------------------------------
app.MapGet("/api/demos", () => Results.Json(Demos.Names.Select(n => new { name = n, code = Demos.Code(n) }))).RequireRateLimiting("api");
app.MapPost("/api/demos/{name}/run", async (string name) =>
{
    if (!Demos.Names.Contains(name)) return Results.Json(new { error = "Unknown demo." }, statusCode: 404);
    if (!await demoSlots.WaitAsync(TimeSpan.FromSeconds(1))) return Results.Json(new { error = "Two demos are already running. Try again in a moment." }, statusCode: 429);
    try
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        var work = name == "api" ? RunApiDemo() : Demos.RunAsync(name, adminUri, happPath);
        var done = await Task.WhenAny(work, Task.Delay(Timeout.Infinite, timeout.Token));
        return done == work ? Results.Json(new { demo = name, output = await work }) : Results.Json(new { error = "The demo took too long." }, statusCode: 504);
    }
    finally { demoSlots.Release(); }

    async Task<string> RunApiDemo()
    {
        await apiLock.WaitAsync();
        try { return await Demos.RunAsync("api", adminUri, happPath); }
        finally { apiLock.Release(); }
    }
}).RequireRateLimiting("demo");

// ---- tidy up: delete the keys of sessions that have been idle -------------------------------------------------------------------
_ = Task.Run(async () =>
{
    while (true)
    {
        await Task.Delay(TimeSpan.FromMinutes(1));
        foreach (var (sid, s) in sessions.ToArray())
        {
            if (DateTime.UtcNow - s.LastUsed < TimeSpan.FromMinutes(SessionIdleMinutes)) continue;
            await apiLock.WaitAsync();
            try { foreach (string key in s.Keys.ToArray()) await HoloNETAPI.DeleteDataAsync(sid + "-" + key); }
            catch { }
            finally { apiLock.Release(); }
            sessions.TryRemove(sid, out _);
        }
    }
});

app.Run();

record SaveRequest(string Value);
record BoardPost(string Name, string Text);

class SandboxSession
{
    public DateTime LastUsed = DateTime.UtcNow;
    public HashSet<string> Keys = new();
}

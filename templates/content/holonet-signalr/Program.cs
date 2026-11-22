using HoloNETApp;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

// Browsers connect to /holonet-hub (see wwwroot/index.html).
app.MapHub<HoloNETHub>("/holonet-hub");

app.Run();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// GET /holonet runs the HoloNET demo (HoloNETDemo.cs) and returns what happened.
app.MapGet("/holonet", async () => Results.Text(await HoloNETDemos.HoloNETDemo.RunAsync()));

app.Run();

using HoloNETApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

// GET /holonet runs the HoloNET demo (HoloNETDemo.cs) and returns what happened. Your gRPC services (Services/) can call HoloNETDemo the same way.
app.MapGet("/holonet", async () => Results.Text(await HoloNETDemos.HoloNETDemo.RunAsync()));

app.Run();

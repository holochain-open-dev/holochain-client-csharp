var builder = DistributedApplication.CreateBuilder(args);

// The HoloNET service (../HoloNETApp). Add more services, databases or containers here and give them a reference to it.
builder.AddProject<Projects.HoloNETApp>("holonet");

builder.Build().Run();

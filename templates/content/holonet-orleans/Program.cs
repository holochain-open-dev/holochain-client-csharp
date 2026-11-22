using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HoloNETApp;

// An Orleans silo with a grain that uses HoloNET. Add more silos (or a client) to spread grains across machines.
using var host = Host.CreateDefaultBuilder(args)
    .UseOrleans(silo => silo.UseLocalhostClustering())
    .Build();
await host.StartAsync();

var grain = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IHoloNETGrain>("demo");
Console.WriteLine(await grain.RunDemoAsync());

await host.StopAsync();

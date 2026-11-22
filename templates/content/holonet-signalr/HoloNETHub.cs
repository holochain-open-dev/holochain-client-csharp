using Microsoft.AspNetCore.SignalR;

namespace HoloNETApp;

/// <summary>A SignalR hub that gives browsers (or any SignalR client) a live connection to Holochain through HoloNET.
/// To push Holochain signals to every browser, inject IHubContext&lt;HoloNETHub&gt; where you receive them and call Clients.All.SendAsync("HoloNETSignal", ...).</summary>
public class HoloNETHub : Hub
{
    // Runs the HoloNET demo (HoloNETDemo.cs) and returns what happened.
    public Task<string> RunDemo() => HoloNETDemos.HoloNETDemo.RunAsync();

    // Sends a message to every connected browser.
    public Task Broadcast(string message) => Clients.All.SendAsync("HoloNETMessage", Context.ConnectionId, message);
}

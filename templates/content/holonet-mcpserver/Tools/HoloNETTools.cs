using System.ComponentModel;
using ModelContextProtocol.Server;

/// <summary>MCP tools that let an AI client (Claude, Copilot, ...) use Holochain through HoloNET. Replace the demo with your own HoloNET calls (see HoloNETDemo.cs).</summary>
internal class HoloNETTools
{
    [McpServerTool]
    [Description("Runs the HoloNET demo: connects to a Holochain conductor, installs the hApp, and saves, loads and changes some data. Returns what happened.")]
    public Task<string> RunHoloNETDemo() => HoloNETDemos.HoloNETDemo.RunAsync();
}

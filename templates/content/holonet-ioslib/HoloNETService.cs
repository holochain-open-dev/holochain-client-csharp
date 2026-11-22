namespace HoloNETApp;

/// <summary>Reference this library from any app to use HoloNET. Replace the demo call with your own HoloNET code (see HoloNETDemo.cs).</summary>
public static class HoloNETService
{
    /// <summary>Runs the HoloNET demo (HoloNETDemo.cs) and returns what happened.</summary>
    public static Task<string> RunDemoAsync() => HoloNETDemos.HoloNETDemo.RunAsync();
}

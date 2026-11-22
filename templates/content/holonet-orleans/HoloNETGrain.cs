namespace HoloNETApp;

/// <summary>A grain that runs the HoloNET demo (HoloNETDemo.cs). Replace it with grains that wrap your own HoloNET calls.</summary>
public class HoloNETGrain : Grain, IHoloNETGrain
{
    public Task<string> RunDemoAsync() => HoloNETDemos.HoloNETDemo.RunAsync();
}

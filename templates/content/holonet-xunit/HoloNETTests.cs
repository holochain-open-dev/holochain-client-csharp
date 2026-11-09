namespace HoloNETApp;

public class HoloNETTests
{
    // An integration test: it starts (or connects to) a conductor, so it needs Holochain available (see README.md).
    // Replace the demo with the HoloNET calls your own code makes.
    [Fact]
    public async Task HoloNETDemoRuns()
    {
        var result = await HoloNETDemos.HoloNETDemo.RunAsync();
        Assert.False(string.IsNullOrWhiteSpace(result));
    }
}

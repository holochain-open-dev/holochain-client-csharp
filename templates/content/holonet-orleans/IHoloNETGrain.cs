namespace HoloNETApp;

public interface IHoloNETGrain : IGrainWithStringKey
{
    Task<string> RunDemoAsync();
}

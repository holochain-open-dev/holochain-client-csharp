namespace HoloNETApp;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Runs the HoloNET demo (HoloNETDemo.cs) once when the service starts, then idles. Replace it with your own background work.
        logger.LogInformation("HoloNET demo:\n{result}", await HoloNETDemos.HoloNETDemo.RunAsync());
        while (!stoppingToken.IsCancellationRequested)
            await Task.Delay(1000, stoppingToken);
    }
}

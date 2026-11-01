namespace HoloNETApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnRunClicked(object? sender, EventArgs e)
    {
        RunBtn.IsEnabled = false;
        OutputLabel.Text = "Running...";
        OutputLabel.Text = await HoloNETDemos.HoloNETDemo.RunAsync();
        RunBtn.IsEnabled = true;
    }
}

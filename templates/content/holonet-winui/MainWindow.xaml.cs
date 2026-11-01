using Microsoft.UI.Xaml;

namespace HoloNETApp;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void RunButton_Click(object sender, RoutedEventArgs e)
    {
        RunButton.IsEnabled = false;
        Output.Text = "Running...";
        Output.Text = await HoloNETDemos.HoloNETDemo.RunAsync();
        RunButton.IsEnabled = true;
    }
}

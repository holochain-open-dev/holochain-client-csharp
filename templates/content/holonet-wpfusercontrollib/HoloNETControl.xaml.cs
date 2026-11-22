using System.Windows;
using System.Windows.Controls;

namespace HoloNETApp;

public partial class HoloNETControl : UserControl
{
    public HoloNETControl()
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

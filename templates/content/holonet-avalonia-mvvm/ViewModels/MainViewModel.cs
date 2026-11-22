using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HoloNETApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Output { get; set; } = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RunCommand))]
    public partial bool IsBusy { get; set; }

    // Runs the HoloNET demo (HoloNETDemo.cs) and shows what happened.
    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task RunAsync()
    {
        IsBusy = true;
        Output = "Running...";
        Output = await HoloNETDemos.HoloNETDemo.RunAsync();
        IsBusy = false;
    }

    private bool CanRun() => !IsBusy;
}

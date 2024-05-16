
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using NextGenSoftware.Holochain.HoloNET.Manager.Managers;

namespace NextGenSoftware.Holochain.HoloNET.Manager.UserControls
{
    /// <summary>
    /// Interaction logic for ucHoloNETMetaData.xaml
    /// </summary>
    public partial class ucAboutPopup : UserControl
    {
        public ucAboutPopup()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            PopupManager.CurrentPopup = null;
            this.Visibility = Visibility.Collapsed;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            // for .NET Core you need to add UseShellExecute = true
            // see https://learn.microsoft.com/dotnet/api/system.diagnostics.processstartinfo.useshellexecute#property-value
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
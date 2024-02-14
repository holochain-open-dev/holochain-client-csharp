using System.Windows;
using System.Windows.Controls;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Managers;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF.UserControls
{
    /// <summary>
    /// Interaction logic for ucHoloNETMetaData.xaml
    /// </summary>
    public partial class ucInstallAppPopup : UserControl
    {
        public ucInstallAppPopup()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void btnPopupInstallhAppOk_Click(object sender, RoutedEventArgs e)
        {
            if (txthAppName.Text == "")
            {
                lblInstallAppErrorMessage.Text = "Please enter the hApp name";
                lblInstallAppErrorMessage.Visibility = Visibility.Visible;
            }
            else if (txthAppName.Text == HoloNETManager.Instance.HoloNETEntryDemoAppId)
            {
                lblInstallAppErrorMessage.Text = "Sorry that name is reserved for the HoloNETEntry (Inernal Connection) popup.";
                lblInstallAppErrorMessage.Visibility = Visibility.Visible;
            }
            else if (txthAppName.Text == HoloNETManager.Instance.HoloNETCollectionDemoAppId)
            {
                lblInstallAppErrorMessage.Text = "Sorry that name is reserved for the HoloNET Collection (Inernal Connection) popup.";
                lblInstallAppErrorMessage.Visibility = Visibility.Visible;
            }
            else
            {
                lblInstallAppErrorMessage.Visibility = Visibility.Collapsed;
                this.Visibility = Visibility.Collapsed;
                Dispatcher.InvokeAsync(async () => await HoloNETManager.Instance.InstallApp(txthAppName.Text, HoloNETManager.Instance.InstallingAppParams.InstallinghAppPath));
            }
        }

        private void btnPopupInstallhAppCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            lblInstallAppErrorMessage.Visibility = Visibility.Collapsed;
        }
    }
}
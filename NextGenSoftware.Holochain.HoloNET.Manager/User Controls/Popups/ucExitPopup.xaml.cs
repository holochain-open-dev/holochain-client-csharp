
using System.Windows;
using System.Windows.Controls;
using NextGenSoftware.Holochain.HoloNET.Manager.Managers;

namespace NextGenSoftware.Holochain.HoloNET.Manager.UserControls
{
    /// <summary>
    /// Interaction logic for ucHoloNETMetaData.xaml
    /// </summary>
    public partial class ucExitPopup : UserControl
    {
        public ucExitPopup()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void btnYes_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnNo_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PopupManager.CurrentPopup = null;
            this.Visibility = Visibility.Collapsed;
        }
    }
}
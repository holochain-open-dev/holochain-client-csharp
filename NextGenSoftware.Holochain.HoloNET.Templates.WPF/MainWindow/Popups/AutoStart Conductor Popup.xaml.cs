using System;
using System.Windows;
using System.Windows.Media;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private void btnAdminURIPopupCancel_Click(object sender, RoutedEventArgs e)
        //{
        //    popupAdminURI.Visibility = Visibility.Collapsed;
        //    Application.Current.Shutdown();
        //}

        //private void btnAdminURIPopupOK_Click(object sender, RoutedEventArgs e)
        //{
        //    txtSecondsToWaitForConductorToStartError.Visibility = Visibility.Hidden;

        //    if (string.IsNullOrEmpty(txtAdminURI.Text))
        //        lblAdminURIError.Visibility = Visibility.Visible;

        //    //else if (!Uri.IsWellFormedUriString(txtAdminURI.Text, UriKind.Absolute))
        //    //{
        //    //    //lblAdminURIError.Visibility = Visibility.Visible;
        //    //}

        //    else
        //    {
        //        try
        //        {
        //            int number = Convert.ToInt32(txtSecondsToWaitForConductorToStart.Text);

        //            lblAdminURIError.Visibility = Visibility.Collapsed;
        //            popupAdminURI.Visibility = Visibility.Collapsed;

        //            _holoNETClientAdmin.HoloNETDNA.HolochainConductorAdminURI = txtAdminURI.Text;
        //            _holoNETClientAdmin.HoloNETDNA.AutoStartHolochainConductor = chkAutoStartConductor.IsChecked.Value;
        //            _holoNETClientAdmin.HoloNETDNA.AutoShutdownHolochainConductor = chkAutoShutdownConductor.IsChecked.Value;
        //            _holoNETClientAdmin.HoloNETDNA.ShowHolochainConductorWindow = chkShowConductorWindow.IsChecked.Value;
        //            _holoNETClientAdmin.HoloNETDNA.SecondsToWaitForHolochainConductorToStart = Convert.ToInt32(txtSecondsToWaitForConductorToStart.Text);
        //            _holoNETClientAdmin.SaveDNA();

        //            ConnectAdmin();
        //        }
        //        catch (Exception ex)
        //        {
        //            txtSecondsToWaitForConductorToStartError.Visibility = Visibility.Visible;
        //        }
        //    }
        //}

        //private void chkAutoStartConductor_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (chkAutoShutdownConductor != null)
        //        chkAutoShutdownConductor.IsEnabled = true;

        //    if (chkShowConductorWindow != null)
        //        chkShowConductorWindow.IsEnabled = true;

        //    if (txtSecondsToWaitForConductorToStart != null)
        //        txtSecondsToWaitForConductorToStart.IsEnabled = true;

        //    if (lblSecondsToWaitForConductorToStart != null)
        //    {
        //        lblSecondsToWaitForConductorToStart.IsEnabled = true;
        //        lblSecondsToWaitForConductorToStart.Foreground = Brushes.WhiteSmoke;
        //    }
        //}

        //private void chkAutoStartConductor_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    if (chkAutoShutdownConductor != null)
        //        chkAutoShutdownConductor.IsEnabled = false;

        //    if (chkShowConductorWindow != null)
        //        chkShowConductorWindow.IsEnabled = false;

        //    if (txtSecondsToWaitForConductorToStart != null)
        //        txtSecondsToWaitForConductorToStart.IsEnabled = false;

        //    if (lblSecondsToWaitForConductorToStart != null)
        //    {
        //        lblSecondsToWaitForConductorToStart.IsEnabled = false;
        //        lblSecondsToWaitForConductorToStart.Foreground = Brushes.Gray;
        //    }

        //    if (txtSecondsToWaitForConductorToStartError != null)
        //        txtSecondsToWaitForConductorToStartError.Visibility = Visibility.Hidden;
        //}
    }
}
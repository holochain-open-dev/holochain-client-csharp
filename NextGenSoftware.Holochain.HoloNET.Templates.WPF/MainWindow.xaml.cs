using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Interfaces;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Managers;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Models;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.UserControls;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Unloaded += MainWindow_Unloaded;
        }

        private void InitUI()
        {
            ucConductorSettingsPopup.txtAdminURI.Text = HoloNETManager.Instance.HoloNETClientAdmin.HoloNETDNA.HolochainConductorAdminURI;
            ucConductorSettingsPopup.chkAutoStartConductor.IsChecked = HoloNETManager.Instance.HoloNETClientAdmin.HoloNETDNA.AutoStartHolochainConductor;
            ucConductorSettingsPopup.chkAutoShutdownConductor.IsChecked = HoloNETManager.Instance.HoloNETClientAdmin.HoloNETDNA.AutoShutdownHolochainConductor;
            ucConductorSettingsPopup.chkShowConductorWindow.IsChecked = HoloNETManager.Instance.HoloNETClientAdmin.HoloNETDNA.ShowHolochainConductorWindow;
            ucConductorSettingsPopup.txtSecondsToWaitForConductorToStart.Text = HoloNETManager.Instance.HoloNETClientAdmin.HoloNETDNA.SecondsToWaitForHolochainConductorToStart.ToString();
        }

        private void ShowStatusMessage(string message, StatusMessageType type = StatusMessageType.Information, bool showSpinner = false, ucHoloNETEntry ucHoloNETEntry = null)
        {
            txtStatus.Text = $"{message}";

            switch (type)
            {
                case StatusMessageType.Success:
                    txtStatus.Foreground = Brushes.LightGreen;
                    break;

                case StatusMessageType.Information:
                    txtStatus.Foreground = Brushes.White;
                    break;

                case StatusMessageType.Error:
                    txtStatus.Foreground = Brushes.LightSalmon;
                    break;

                case StatusMessageType.Warning:
                    txtStatus.Foreground = Brushes.Yellow;
                    break;
            }

            if (showSpinner)
                spinner.Visibility = Visibility.Visible;
            else
                spinner.Visibility = Visibility.Hidden;

            if (ucHoloNETEntry != null)
                ucHoloNETEntry.ShowStatusMessage(message, type, showSpinner);

            sbAnimateStatus.Begin();
        }

        private void LogMessage(string message)
        {
            lstOutput.Items.Add(message);

            if (VisualTreeHelper.GetChildrenCount(lstOutput) > 0)
            {
                Border border = (Border)VisualTreeHelper.GetChild(lstOutput, 0);
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HoloNETManager.Instance.OnLogMessage += HoloNETManager_OnLogMessage;
            HoloNETManager.Instance.OnStatusMessage += HoloNETManager_OnStatusMessage;
            HoloNETManager.Instance.OnNumberOfClientConnectionsChanged += HoloNETManager_OnNumberOfClientConnectionsChanged;
            HoloNETManager.Instance.OnInstalledAppsChanged += HoloNETManager_OnInstalledAppsChanged;

            HoloNETManager.Instance.BootHoloNETManager();
            InitUI();
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            HoloNETManager.Instance.ShutdownHoloNETManager();
            HoloNETManager.Instance.OnLogMessage -= HoloNETManager_OnLogMessage;
            HoloNETManager.Instance.OnStatusMessage -= HoloNETManager_OnStatusMessage;
            HoloNETManager.Instance.OnNumberOfClientConnectionsChanged -= HoloNETManager_OnNumberOfClientConnectionsChanged;
            HoloNETManager.Instance.OnInstalledAppsChanged -= HoloNETManager_OnInstalledAppsChanged;
        }

        private void HoloNETManager_OnInstalledAppsChanged(object sender, InstalledAppsEventArgs e)
        {
            //If the UI used does not support binding like WPF/XAML does then you will need to un-comment the line below to manually refresh the grid.
            //gridHapps.ItemsSource = e.InstalledApps;
        }

        private void HoloNETManager_OnNumberOfClientConnectionsChanged(object sender, NumberOfClientConnectionsEventArgs e)
        {
            txtConnections.Text = $"Client Connections: {e.NumberOfConnections}";
            sbAnimateConnections.Begin();
        }

        private void HoloNETManager_OnStatusMessage(object sender, StatusMessageEventArgs e)
        {
            ShowStatusMessage(e.Message, e.Type, e.ShowSpinner, e.ucHoloNETEntry);
        }

        private void HoloNETManager_OnLogMessage(object sender, LogMessageEventArgs e)
        {
            LogMessage(e.Message);
        }

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "hApp files (*.hApp)|*.hApp";

            if (openFileDialog.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(openFileDialog.FileName)) 
                {
                    HoloNETManager.Instance.InstallingAppParams.InstallinghAppPath = openFileDialog.FileName;
                    FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                    string[] parts = fileInfo.Name.Split('.');

                    HoloNETManager.Instance.InstallingAppParams.InstallinghAppName = parts[0];
                    ucInstallAppPopup.txthAppName.Text = HoloNETManager.Instance.InstallingAppParams.InstallinghAppName;
                    ucInstallAppPopup.Visibility = Visibility.Visible;
                }
            }
        }

        private void btnRefreshInstalledhApps_Click(object sender, RoutedEventArgs e)
        {
            HoloNETManager.Instance.ListHapps();
        }

        private void btnListDNAs_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("ADMIN: Listing DNAs...");
            ShowStatusMessage($"Listing DNAs...");
            HoloNETManager.Instance.HoloNETClientAdmin.ListDnas();
        }

        private void btnListCellIds_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("ADMIN: Listing Cell Ids...");
            ShowStatusMessage($"Listing Cell Ids...", StatusMessageType.Information, true);
            HoloNETManager.Instance.HoloNETClientAdmin.ListCellIds();
        }

        private void btnListAttachedInterfaces_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("ADMIN: Listing Attached Interfaces...");
            ShowStatusMessage($"Listing Attached Interfaces...", StatusMessageType.Information, true);
            HoloNETManager.Instance.HoloNETClientAdmin.ListInterfaces();
        }

        private void btnGetAgentInfo_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("ADMIN: Getting Agent Info...");
            ShowStatusMessage($"Getting Agent Info...", StatusMessageType.Information, true);
            HoloNETManager.Instance.HoloNETClientAdmin.GetAgentInfo();
        }

        private void btnGetStorageInfo_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("ADMIN: Getting Storage Info...");
            ShowStatusMessage($"Getting Storage Info...", StatusMessageType.Information, true);
            HoloNETManager.Instance.HoloNETClientAdmin.GetStorageInfo();
        }

        private void btnDumpNetWorkStats_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("ADMIN: Dumping Network Stats...");
            ShowStatusMessage($"Dumping Network Stats....", StatusMessageType.Information, true);
            HoloNETManager.Instance.HoloNETClientAdmin.DumpNetworkStats();
        }

        private void btnDumpNetworkMetrics_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("ADMIN: Dumping Network Metrics...");
            ShowStatusMessage($"Dumping Network Metrics....", StatusMessageType.Information, true);
            HoloNETManager.Instance.HoloNETClientAdmin.DumpNetworkMetrics();
        }

        private void btnDumpState_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("ADMIN: Dump State...");
            ShowStatusMessage($"Dump State....", StatusMessageType.Information, true);
            HoloNETManager.Instance.HoloNETClientAdmin.DumpState();
        }

        private void btnFullDump_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("ADMIN: Dump Full State...");
            ShowStatusMessage($"Dump Full State....", StatusMessageType.Information, true);
            HoloNETManager.Instance.HoloNETClientAdmin.DumpFullState();
            //HoloNETManager.Instance.HoloNETClientAdmin.GetDnaDefinition();

            //HoloNETManager.Instance.HoloNETClientAdmin.AddAgentInfo
            //HoloNETManager.Instance.HoloNETClientAdmin.DeleteCloneCellAsync()
            //HoloNETManager.Instance.HoloNETClientAdmin.GraftRecords()
            //HoloNETManager.Instance.HoloNETClientAdmin.UpdateCoordinators
        }

        //private void btnGetCellId_Click(object sender, RoutedEventArgs e)
        //{
        //    if (HoloNETManager.Instance != null && HoloNETManager.Instance.CurrentApp != null)
        //    {
        //        LogMessage("ADMIN: Getting Cell Id...");
        //        ShowStatusMessage($"Getting Cell Id....", StatusMessageType.Information, true);
        //        HoloNETManager.Instance.HoloNETClientAdmin.GetCellId();
        //    }
        //}

        private void btnGetDNADefinition_Click(object sender, RoutedEventArgs e)
        {
            if (HoloNETManager.Instance != null && HoloNETManager.Instance.CurrentApp != null)
            {
                LogMessage("ADMIN: Getting DNA Defintion...");
                ShowStatusMessage($"Getting DNA Defintion....", StatusMessageType.Information, true);
                HoloNETManager.Instance.HoloNETClientAdmin.GetDnaDefinition(HoloNETManager.Instance.CurrentApp.DnaHash);
            }
        }

        private void btnShowLog_Click(object sender, RoutedEventArgs e)
        {
            //Process.Start("notepad.exe", "Logs\\HoloNET.log");
            Process.Start("notepad.exe", $"{HoloNETDNAManager.HoloNETDNA.LogPath}\\{HoloNETDNAManager.HoloNETDNA.LogFileName}");
        }

        private void btnClearLog_Click(object sender, RoutedEventArgs e)
        {
            lstOutput.Items.Clear();
        }

        private void btnReboot_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Rebooting...");
            ShowStatusMessage($"Rebooting...");
            HoloNETManager.Instance.Reboot();
        }

        private void btnHoloNETEntry_Click(object sender, RoutedEventArgs e)
        {
            PopupManager.CurrentPopup = ucHoloNETEntryPopup;
            ucHoloNETEntryPopup.Visibility = Visibility.Visible;
        }

        private void btnHoloNETCollection_Click(object sender, RoutedEventArgs e)
        {
            PopupManager.CurrentPopup = ucHoloNETCollectionPopup;
            ucHoloNETCollectionPopup.Visibility = Visibility.Visible;
        }

        private void btnEnableApp_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source != null)
            {
                Button? button = e.Source as Button;

                if (button != null)
                {
                    IInstalledApp? app = button.DataContext as IInstalledApp;

                    if (app != null)
                    {
                        LogMessage($"ADMIN: Enabling {app.Name} hApp...");
                        ShowStatusMessage($"Enabling {app.Name} hApp...", StatusMessageType.Information, true);
                        HoloNETManager.Instance.HoloNETClientAdmin.EnablelApp(app.Name);
                    }
                }
            }
        }

        private void btnDisableApp_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source != null)
            {
                Button? button = e.Source as Button;

                if (button != null)
                {
                    InstalledApp? app = button.DataContext as InstalledApp;

                    if (app != null)
                    {
                        LogMessage($"ADMIN: Disabling {app.Name} hApp...");
                        ShowStatusMessage($"Disabling {app.Name} hApp...", StatusMessageType.Information, true);
                        HoloNETManager.Instance.HoloNETClientAdmin.DisableApp(app.Name);
                    }
                }
            }
        }

        private void btnUninstallApp_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source != null)
            {
                Button? button = e.Source as Button;

                if (button != null)
                {
                    InstalledApp? app = button.DataContext as InstalledApp;

                    if (app != null)
                    {
                        LogMessage($"ADMIN: Uninstalling {app.Name} hApp...");
                        ShowStatusMessage($"Uninstalling {app.Name} hApp...", StatusMessageType.Information, true);
                        HoloNETManager.Instance.HoloNETClientAdmin.UninstallApp(app.Name);
                    }
                }
            }
        }

        private void btnDisconnectClient_Click(object sender, RoutedEventArgs e)
        {
            IInstalledApp app = gridHapps.SelectedItem as IInstalledApp;

            if (app != null)
            {
                Dispatcher.InvokeAsync(async () =>
                {
                    if (await HoloNETManager.Instance.DisconnectAsync(app))
                    {
                        Button btnDisconnectClient = sender as Button;

                        if (btnDisconnectClient != null)
                            btnDisconnectClient.IsEnabled = false;
                    }
                });
            }
        }

        private void btnCallZomeFunction_Click(object sender, RoutedEventArgs e)
        {
            PopupManager.CurrentPopup = ucCallZomePopup;
            ucCallZomePopup.Visibility = Visibility.Visible;
        }

        private void btnViewDataEntries_Click(object sender, RoutedEventArgs e)
        {
            PopupManager.CurrentPopup = ucHoloNETEntryAndCollectionSharedPopup;
            ucHoloNETEntryAndCollectionSharedPopup.Visibility = Visibility.Visible;
        }

        private void gridLog_LayoutUpdated(object sender, EventArgs e)
        {
            if (gridLog.ActualHeight - 60 > 0)
                lstOutput.Height = gridLog.ActualHeight - 60;
        }

        private void chkShowDetailedMessages_Checked(object sender, RoutedEventArgs e)
        {
            HoloNETManager.Instance.ShowDetailedLogMessages = true;
        }

        private void chkShowDetailedMessages_Unchecked(object sender, RoutedEventArgs e)
        {
            HoloNETManager.Instance.ShowDetailedLogMessages = false;
        }

        private void gridHapps_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (PopupManager.CurrentPopup == null)
                HoloNETManager.Instance.CurrentApp = gridHapps.SelectedItem as InstalledApp;
        }
    }
}
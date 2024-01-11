using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Models;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.UserControls;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // private const string _holoNETEntryDemoHappPath = @"E:\hc\holochain-holochain-0.1.5\happs\oasis\BUILD\happ\oasis.happ";
        // //private const string _holoNETCollectionDemoHappPath = @"E:\hc\holochain-holochain-0.1.5\happs\oasis-holonet-collection\BUILD\happ\oasis.happ";
        // private const string _holoNETCollectionDemoHappPath = @"E:\hc\holochain-holochain-0.1.5\happs\oasis\BUILD\happ\oasis.happ";
        // private const string _holoNETEntryDemoAppId = "oasis-holonet-entry-demo-app";
        // private const string _holoNETCollectionDemoAppId = "oasis-holonet-collection-demo-app";
        // private const string _role_name = "oasis";
        // private HoloNETClient? _holoNETClientAdmin;
        // private List<HoloNETClient> _holoNETappClients = new List<HoloNETClient>();
        // private bool _rebooting = false;
        // private bool _adminDisconnected = false;
        //// private bool _appDisconnected = false;
        // private string _installinghAppName = "";
        // private string _installinghAppPath = "";
        // private byte[][] _installingAppCellId = null;
        // dynamic _paramsObject = null;
        // private int _clientsToDisconnect = 0;
        // private int _clientsDisconnected = 0;
        // private Avatar _holoNETEntry;
        // private Dictionary<string, AvatarShared> _holoNETEntryShared = new Dictionary<string, AvatarShared>();
        // private ClientOperation _clientOperation;
        // private ushort _appAgentClientPort = 0;
        // private bool _removeClientConnectionFromPoolAfterDisconnect = true;
        // private bool _initHoloNETEntryDemo = false;
        // private bool _showAppsListedInLog = true;
        //private AvatarShared _currentAvatar;

        //public ObservableCollection<InstalledApp> InstalledApps { get; set; } = new ObservableCollection<InstalledApp>();
        //public Dictionary<string, HoloNETObservableCollection<AvatarShared>> HoloNETEntriesShared { get; set; } = new Dictionary<string, HoloNETObservableCollection<AvatarShared>>();
        //public HoloNETObservableCollection<Avatar> HoloNETEntries { get; set; }
        //public InstalledApp CurrentApp { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Unloaded += MainWindow_Unloaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitUI();
            HoloNETManager.Instance.BootHoloNETManager();
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            HoloNETManager.Instance.ShutdownHoloNETManager();

            if (ucHoloNETCollectionEntryInternal != null)
            {
                ucHoloNETCollectionEntryInternal.txtHoloNETEntryFirstName.TextChanged -= TxtHoloNETEntryInternalFirstName_TextChanged;
                ucHoloNETCollectionEntryInternal.txtHoloNETEntryLastName.TextChanged -= TxtHoloNETEntryInternalLastName_TextChanged;
                ucHoloNETCollectionEntryInternal.txtHoloNETEntryDOB.TextChanged -= TxtHoloNETEntryInternalDOB_TextChanged;
                ucHoloNETCollectionEntryInternal.txtHoloNETEntryEmail.TextChanged -= TxtHoloNETEntryInternalEmail_TextChanged;
            }

            if (ucHoloNETCollectionEntry != null)
            {
                ucHoloNETCollectionEntry.txtHoloNETEntryFirstName.TextChanged -= TxtHoloNETEntryFirstName_TextChanged;
                ucHoloNETCollectionEntry.txtHoloNETEntryLastName.TextChanged -= TxtHoloNETEntryLastName_TextChanged;
                ucHoloNETCollectionEntry.txtHoloNETEntryDOB.TextChanged -= TxtHoloNETEntryDOB_TextChanged;
                ucHoloNETCollectionEntry.txtHoloNETEntryEmail.TextChanged -= TxtHoloNETEntryEmail_TextChanged;
            }
        }

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "hApp files (*.hApp)|*.hApp";

            if (openFileDialog.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(openFileDialog.FileName)) 
                {
                    HoloNETManager.Instance.InstallinghAppPath = openFileDialog.FileName;
                    FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                    string[] parts = fileInfo.Name.Split('.');
                    HoloNETManager.Instance.InstallinghAppName = parts[0];
                    InputTextBox.Text = HoloNETManager.Instance.InstallinghAppName;
                    popupInstallhApp.Visibility = Visibility.Visible;
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
            HoloNETManager.Instance.HoloNETClientAdmin.AdminListApps(AppStatusFilter.All);
        }

        private void btnListCellIds_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("ADMIN: Listing Cell Ids...");
            ShowStatusMessage($"Listing Cell Ids...", StatusMessageType.Information, true);
            HoloNETManager.Instance.HoloNETClientAdmin.AdminListCellIds();
        }

        private void btnListAttachedInterfaces_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("ADMIN: Listing Attached Interfaces...");
            ShowStatusMessage($"Listing Attached Interfaces...", StatusMessageType.Information, true);
            HoloNETManager.Instance.HoloNETClientAdmin.AdminListInterfaces();
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

        private void btnEnableApp_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source != null)
            {
                Button? button = e.Source as Button;

                if (button != null)
                {
                    InstalledApp? app = button.DataContext as InstalledApp;

                    if (app != null)
                    {
                        LogMessage($"ADMIN: Enabling {app.Name} hApp...");
                        ShowStatusMessage($"Enabling {app.Name} hApp...", StatusMessageType.Information, true);
                        HoloNETManager.Instance.HoloNETClientAdmin.AdminEnablelApp(app.Name);
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
                        HoloNETManager.Instance.HoloNETClientAdmin.AdminDisableApp(app.Name);
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
                        HoloNETManager.Instance.HoloNETClientAdmin.AdminUninstallApp(app.Name);
                    }
                }
            }
        }

        private void btnDisconnectClient_Click(object sender, RoutedEventArgs e)
        {
            InstalledApp app = gridHapps.SelectedItem as InstalledApp;

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

        private void gridLog_LayoutUpdated(object sender, EventArgs e)
        {
            if (gridLog.ActualHeight - 60 > 0)
                lstOutput.Height = gridLog.ActualHeight - 60;
        }

        private void InitUI()
        {
            ucHoloNETCollectionEntry.txtHoloNETEntryFirstName.TextChanged += TxtHoloNETEntryFirstName_TextChanged;
            ucHoloNETCollectionEntry.txtHoloNETEntryLastName.TextChanged += TxtHoloNETEntryLastName_TextChanged;
            ucHoloNETCollectionEntry.txtHoloNETEntryDOB.TextChanged += TxtHoloNETEntryDOB_TextChanged;
            ucHoloNETCollectionEntry.txtHoloNETEntryEmail.TextChanged += TxtHoloNETEntryEmail_TextChanged;

            ucHoloNETCollectionEntryInternal.txtHoloNETEntryFirstName.TextChanged += TxtHoloNETEntryInternalFirstName_TextChanged;
            ucHoloNETCollectionEntryInternal.txtHoloNETEntryLastName.TextChanged += TxtHoloNETEntryInternalLastName_TextChanged;
            ucHoloNETCollectionEntryInternal.txtHoloNETEntryDOB.TextChanged += TxtHoloNETEntryInternalDOB_TextChanged;
            ucHoloNETCollectionEntryInternal.txtHoloNETEntryEmail.TextChanged += TxtHoloNETEntryInternalEmail_TextChanged;

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
            HoloNETManager.Instance.CurrentApp = gridHapps.SelectedItem as InstalledApp;
        }
    }
}
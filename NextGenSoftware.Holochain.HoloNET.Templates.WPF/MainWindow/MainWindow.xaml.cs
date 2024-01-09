using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.ORM.Collections;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Models;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string _holoNETEntryDemoHappPath = @"E:\hc\holochain-holochain-0.1.5\happs\oasis\BUILD\happ\oasis.happ";
        //private const string _holoNETCollectionDemoHappPath = @"E:\hc\holochain-holochain-0.1.5\happs\oasis-holonet-collection\BUILD\happ\oasis.happ";
        private const string _holoNETCollectionDemoHappPath = @"E:\hc\holochain-holochain-0.1.5\happs\oasis\BUILD\happ\oasis.happ";
        private const string _holoNETEntryDemoAppId = "oasis-holonet-entry-demo-app";
        private const string _holoNETCollectionDemoAppId = "oasis-holonet-collection-demo-app";
        private const string _role_name = "oasis";
        private HoloNETClient? _holoNETClientAdmin;
        private List<HoloNETClient> _holoNETappClients = new List<HoloNETClient>();
        private bool _rebooting = false;
        private bool _adminDisconnected = false;
       // private bool _appDisconnected = false;
        private string _installinghAppName = "";
        private string _installinghAppPath = "";
        private byte[][] _installingAppCellId = null;
        dynamic _paramsObject = null;
        private int _clientsToDisconnect = 0;
        private int _clientsDisconnected = 0;
        private Avatar _holoNETEntry;
        private Dictionary<string, AvatarShared> _holoNETEntryShared = new Dictionary<string, AvatarShared>();
        private ClientOperation _clientOperation;
        private ushort _appAgentClientPort = 0;
        private bool _removeClientConnectionFromPoolAfterDisconnect = true;
        private bool _initHoloNETEntryDemo = false;
        private bool _showAppsListedInLog = true;
        private AvatarShared _currentAvatar;

        public ObservableCollection<InstalledApp> InstalledApps { get; set; } = new ObservableCollection<InstalledApp>();
        public Dictionary<string, HoloNETObservableCollection<AvatarShared>> HoloNETEntriesShared { get; set; } = new Dictionary<string, HoloNETObservableCollection<AvatarShared>>();
        public HoloNETObservableCollection<Avatar> HoloNETEntries { get; set; }
        public InstalledApp CurrentApp { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Unloaded += MainWindow_Unloaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            if (HoloNETEntries != null)
            {
                HoloNETEntries.OnClosed -= HoloNETEntries_OnClosed;
                HoloNETEntries.OnCollectionLoaded -= HoloNETEntries_OnCollectionLoaded;
                HoloNETEntries.OnCollectionSaved -= HoloNETEntries_OnCollectionSaved;
                HoloNETEntries.OnError -= HoloNETEntries_OnError;
                HoloNETEntries.OnHoloNETEntryAddedToCollection -= HoloNETEntries_OnHoloNETEntryAddedToCollection;
                HoloNETEntries.OnHoloNETEntryRemovedFromCollection -= HoloNETEntries_OnHoloNETEntryRemovedFromCollection;
            }

            if (HoloNETEntriesShared != null)
            {
                foreach (string key in HoloNETEntriesShared.Keys)
                {
                    HoloNETEntriesShared[key].OnInitialized -= HoloNETEntriesShared_OnInitialized;
                    HoloNETEntriesShared[key].OnCollectionLoaded -= HoloNETEntriesShared_OnCollectionLoaded;
                    HoloNETEntriesShared[key].OnCollectionSaved -= HoloNETEntriesShared_OnCollectionSaved;
                    HoloNETEntriesShared[key].OnHoloNETEntryAddedToCollection -= HoloNETEntriesShared_OnHoloNETEntryAddedToCollection;
                    HoloNETEntriesShared[key].OnHoloNETEntryRemovedFromCollection -= HoloNETEntriesShared_OnHoloNETEntryRemovedFromCollection;
                    HoloNETEntriesShared[key].OnClosed -= HoloNETEntriesShared_OnClosed;
                    HoloNETEntriesShared[key].OnError -= HoloNETEntriesShared_OnError;
                }
            }

            if (_holoNETEntryShared != null)
            {
                foreach (string key in _holoNETEntryShared.Keys)
                {
                    _holoNETEntryShared[key].OnInitialized += _holoNETEntryShared_OnInitialized;
                    _holoNETEntryShared[key].OnLoaded += _holoNETEntryShared_OnLoaded;
                    _holoNETEntryShared[key].OnClosed += _holoNETEntryShared_OnClosed;
                    _holoNETEntryShared[key].OnSaved += _holoNETEntryShared_OnSaved;
                    _holoNETEntryShared[key].OnDeleted += _holoNETEntryShared_OnDeleted;
                    _holoNETEntryShared[key].OnError += _holoNETEntryShared_OnError;
                }
            }

            if (_holoNETEntry != null)
            {
                _holoNETEntry.OnInitialized -= _holoNETEntry_OnInitialized;
                _holoNETEntry.OnLoaded -= _holoNETEntry_OnLoaded;
                _holoNETEntry.OnSaved -= _holoNETEntry_OnSaved;
                _holoNETEntry.OnDeleted -= _holoNETEntry_OnDeleted;
                _holoNETEntry.OnClosed -= _holoNETEntry_OnClosed;
                _holoNETEntry.OnError -= _holoNETEntry_OnError;
            }

            if (ucHoloNETCollectionEntry != null)
            {
                ucHoloNETCollectionEntry.txtHoloNETEntryFirstName.TextChanged -= TxtHoloNETEntryFirstName_TextChanged;
                ucHoloNETCollectionEntry.txtHoloNETEntryLastName.TextChanged -= TxtHoloNETEntryLastName_TextChanged;
                ucHoloNETCollectionEntry.txtHoloNETEntryDOB.TextChanged -= TxtHoloNETEntryDOB_TextChanged;
                ucHoloNETCollectionEntry.txtHoloNETEntryEmail.TextChanged -= TxtHoloNETEntryEmail_TextChanged;
            }
            
            CloseAllConnections();
        }

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "hApp files (*.hApp)|*.hApp";

            if (openFileDialog.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(openFileDialog.FileName)) 
                {
                    _installinghAppPath = openFileDialog.FileName;
                    FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                    string[] parts = fileInfo.Name.Split('.');
                    _installinghAppName = parts[0];
                    InputTextBox.Text = _installinghAppName;
                    popupInstallhApp.Visibility = Visibility.Visible;
                }
            }
        }

        private void btnRefreshInstalledhApps_Click(object sender, RoutedEventArgs e)
        {
            ListHapps();
        }

        private void btnListDNAs_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("ADMIN: Listing DNAs...");
            ShowStatusMessage($"Listing DNAs...");
            _holoNETClientAdmin.AdminListApps(AppStatusFilter.All);
        }

        private void btnListCellIds_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("ADMIN: Listing Cell Ids...");
            ShowStatusMessage($"Listing Cell Ids...", StatusMessageType.Information, true);
            _holoNETClientAdmin.AdminListCellIds();
        }

        private void btnListAttachedInterfaces_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("ADMIN: Listing Attached Interfaces...");
            ShowStatusMessage($"Listing Attached Interfaces...", StatusMessageType.Information, true);
            _holoNETClientAdmin.AdminListInterfaces();
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

            _clientsToDisconnect = _holoNETappClients.Count;
            _rebooting = true;
            CloseAllConnections();
        }

        private void gridLog_LayoutUpdated(object sender, EventArgs e)
        {
            if (gridLog.ActualHeight - 60 > 0)
                lstOutput.Height = gridLog.ActualHeight - 60;
        }
    }
}
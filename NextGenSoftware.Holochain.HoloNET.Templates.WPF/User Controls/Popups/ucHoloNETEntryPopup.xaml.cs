using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.ORM.Entries;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Managers;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF.UserControls
{
    /// <summary>
    /// Interaction logic for ucHoloNETMetaData.xaml
    /// </summary>
    public partial class ucHoloNETEntryPopup : UserControl
    {
        public ucHoloNETEntryPopup()
        {
            InitializeComponent();
            DataContext = this;
            this.IsVisibleChanged += UcHoloNETEntryPopup_IsVisibleChanged;
        }

        private void UcHoloNETEntryPopup_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
                Dispatcher.InvokeAsync(async () => { await InitPopupAsync(); });
        }

        private void ShowHoloNETEntryTab()
        {
            btnViewHoloNETEntry.IsEnabled = false;
            btnViewHoloNETEntryMetaData.IsEnabled = true;
            btnViewHoloNETEntryInfo.IsEnabled = true;
            ucHoloNETEntryMetaData.Visibility = Visibility.Collapsed;
            ucHoloNETEntry.Visibility = Visibility.Visible;
            stkpnlInfoHoloNETEntry.Visibility = Visibility.Collapsed;
        }

        private void ShowHoloNETEntryMetaDataTab()
        {
            btnViewHoloNETEntry.IsEnabled = true;
            btnViewHoloNETEntryMetaData.IsEnabled = false;
            btnViewHoloNETEntryInfo.IsEnabled = true;
            ucHoloNETEntryMetaData.Visibility = Visibility.Visible;
            ucHoloNETEntry.Visibility = Visibility.Collapsed;
            stkpnlInfoHoloNETEntry.Visibility = Visibility.Collapsed;
        }

        private void ShowHoloNETEntryInfoTab()
        {
            btnViewHoloNETEntry.IsEnabled = true;
            btnViewHoloNETEntryMetaData.IsEnabled = true;
            btnViewHoloNETEntryInfo.IsEnabled = false;
            ucHoloNETEntryMetaData.Visibility = Visibility.Collapsed;
            ucHoloNETEntry.Visibility = Visibility.Collapsed;
            stkpnlInfoHoloNETEntry.Visibility = Visibility.Visible;
        }

        private async Task InitPopupAsync()
        {
            ShowHoloNETEntryTab();
            HoloNETEntryUIManager.CurrentHoloNETEntryUI = ucHoloNETEntry;
            ZomeFunctionCallBackEventArgs result = await HoloNETManager.Instance.LoadHoloNETEntryAsync();

            if (!result.IsError)
            {
                if (!result.IsWarning)
                {
                    HoloNETManager.Instance.ShowStatusMessage($"HoloNET Entry (Internal Connection) Loaded.", StatusMessageType.Success, false, ucHoloNETEntry);
                    HoloNETManager.Instance.LogMessage($"APP: HoloNET Entry (Internal Connection) Loaded. {GetHoloNETEntryMetaData(HoloNETManager.Instance.HoloNETEntry)}");

                    ucHoloNETEntry.DataContext = HoloNETManager.Instance.HoloNETEntry;
                    ucHoloNETEntryMetaData.DataContext = HoloNETManager.Instance.HoloNETEntry;
                    //RefreshHoloNETEntryMetaData(HoloNETManager.Instance.HoloNETEntry, ucHoloNETEntryMetaData);
                }
                else
                {
                    HoloNETManager.Instance.ShowStatusMessage($"HoloNET Entry (Internal Connection) Not Loaded.", StatusMessageType.Warning, false, ucHoloNETEntry);
                    HoloNETManager.Instance.LogMessage($"APP: HoloNET Entry (Internal Connection) Not Loaded. Reason: {result.Message}");
                }
            }
            else
            {
                ucHoloNETEntry.ShowStatusMessage("Error Occured Loading HoloNET Entry (Internal Connection).", StatusMessageType.Error);
                HoloNETManager.Instance.ShowStatusMessage($"Error Occured HoloNET Loading Entry (Internal Connection): {result.Message}", StatusMessageType.Error, false);
                HoloNETManager.Instance.LogMessage($"APP: Error Occured Loading HoloNET Entry (Internal Connection): {result.Message}");
            }
        }

        private void RefreshHoloNETEntryMetaData(HoloNETAuditEntryBase holoNETEntry, ucHoloNETEntryMetaData userControl)
        {
            userControl.DataContext = holoNETEntry;


            //holoNETEntry.Id = Guid.NewGuid();
            //holoNETEntry.IsActive = true;
            //holoNETEntry.Version = 1;
            //holoNETEntry.CreatedBy = "David";
            //holoNETEntry.CreatedDate = DateTime.Now;
            //holoNETEntry.ModifiedBy = "David";
            //holoNETEntry.ModifiedDate = DateTime.Now;
            //holoNETEntry.DeletedBy = "David";
            //holoNETEntry.DeletedDate = DateTime.Now;
            //holoNETEntry.EntryHash = Guid.NewGuid().ToString();

            //userControl.Id = holoNETEntry.Id.ToString();
            //userControl.IsActive = holoNETEntry.IsActive == true ? "true" : "false";
            //userControl.Version = holoNETEntry.Version.ToString();
            //userControl.CreatedBy = holoNETEntry.CreatedBy.ToString();
            //userControl.CreatedDate = holoNETEntry.CreatedDate.ToShortDateString();
            //userControl.ModifiedBy = holoNETEntry.ModifiedBy.ToString();
            //userControl.ModifiedDate = holoNETEntry.ModifiedDate.ToShortDateString();
            //userControl.DeletedBy = holoNETEntry.DeletedBy.ToString();
            //userControl.DeletedDate = holoNETEntry.DeletedDate.ToShortDateString();
            //userControl.EntryHash = holoNETEntry.EntryHash;

            //userControl.EntryHash = holoNETEntry.EntryHash;
            //userControl.PreviousVersionEntryHash = holoNETEntry.PreviousVersionEntryHash;
            //userControl.Hash = holoNETEntry.EntryData.Hash;
            //userControl.ActionSequence = holoNETEntry.EntryData.ActionSequence.ToString();
            //userControl.EntryType = holoNETEntry.EntryData.EntryType;
            //userControl.OriginalActionAddress = holoNETEntry.EntryData.OriginalActionAddress;
            //userControl.OriginalEntryAddress = holoNETEntry.EntryData.OriginalEntryAddress;
            //userControl.Signature = holoNETEntry.EntryData.Signature;
            //userControl.Timestamp = holoNETEntry.EntryData.Timestamp.ToString();
            //userControl.Type = holoNETEntry.EntryData.Type;
        }

        private string GetHoloNETEntryMetaData(HoloNETAuditEntryBase entry)
        {
            string metaData = $"EntryHash: {HoloNETManager.Instance.HoloNETEntry.EntryHash}, Created By: {HoloNETManager.Instance.HoloNETEntry.CreatedBy}, Created Date: {HoloNETManager.Instance.HoloNETEntry.CreatedDate}, Modified By: {HoloNETManager.Instance.HoloNETEntry.ModifiedBy}, Modified Date: {HoloNETManager.Instance.HoloNETEntry.ModifiedDate}, Version: {HoloNETManager.Instance.HoloNETEntry.Version}, Previous Version Hash: {HoloNETManager.Instance.HoloNETEntry.PreviousVersionEntryHash}, Id: {HoloNETManager.Instance.HoloNETEntry.Id}, IsActive: {HoloNETManager.Instance.HoloNETEntry.IsActive}";

            if (entry.EntryData != null)
                metaData = $"{metaData}, Action Sequence: {HoloNETManager.Instance.HoloNETEntry.EntryData.ActionSequence}, EntryType: {HoloNETManager.Instance.HoloNETEntry.EntryData.EntryType}, Hash: {HoloNETManager.Instance.HoloNETEntry.EntryData.Hash}, OriginalActionAddress: {HoloNETManager.Instance.HoloNETEntry.EntryData.OriginalActionAddress}, OriginalEntryAddress: {HoloNETManager.Instance.HoloNETEntry.EntryData.OriginalEntryAddress}, Signature: {HoloNETManager.Instance.HoloNETEntry.EntryData.Signature}, TimeStamp: {HoloNETManager.Instance.HoloNETEntry.EntryData.Timestamp}, Type: {HoloNETManager.Instance.HoloNETEntry.EntryData.Type}";

            return metaData;
        }
 

        private void btnViewHoloNETEntry_Click(object sender, RoutedEventArgs e)
        {
            ShowHoloNETEntryTab();
        }

        private void btnViewHoloNETEntryMetaData_Click(object sender, RoutedEventArgs e)
        {
            ShowHoloNETEntryMetaDataTab();
        }

        private void btnViewHoloNETEntryInfo_Click(object sender, RoutedEventArgs e)
        {
            ShowHoloNETEntryInfoTab();
        }

        private void btnHoloNETEntryPopupSave_Click(object sender, RoutedEventArgs e)
        {
            if (ucHoloNETEntry.Validate())
            {
                Dispatcher.InvokeAsync(async () =>
                {
                    ZomeFunctionCallBackEventArgs result = await HoloNETManager.Instance.SaveHoloNETEntryAsync();

                    if (result != null && !result.IsError)
                    {
                        RefreshHoloNETEntryMetaData(HoloNETManager.Instance.HoloNETEntry, ucHoloNETEntryMetaData);

                        HoloNETManager.Instance.ShowStatusMessage($"HoloNET Entry (Internal Connection) Saved.", StatusMessageType.Success, false, ucHoloNETEntry);
                        HoloNETManager.Instance.LogMessage($"APP: HoloNET Entry (Internal Connection) Saved. {GetHoloNETEntryMetaData(HoloNETManager.Instance.HoloNETEntry)}");
                        this.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        ucHoloNETEntry.ShowStatusMessage("Error Occured Saving HoloNET Entry (Internal Connection).", StatusMessageType.Error);
                        HoloNETManager.Instance.ShowStatusMessage($"Error Occured Saving HoloNET Entry (Internal Connection): {result.Message}", StatusMessageType.Error, false);
                        HoloNETManager.Instance.LogMessage($"APP: Error Occured Saving HoloNET Entry (Internal Connection): {result.Message}");
                    }
                });
            }
            else
                ShowHoloNETEntryTab();
        }

        private void btnHoloNETEntryPopupDelete_Click(object sender, RoutedEventArgs e)
        {
            ucHoloNETEntry.HideStatusMessage();

            Dispatcher.InvokeAsync(async () =>
            {
                ZomeFunctionCallBackEventArgs result = await HoloNETManager.Instance.DeleteHoloNETEntryAsync();

                if (result != null && !result.IsError)
                {
                    RefreshHoloNETEntryMetaData(HoloNETManager.Instance.HoloNETEntry, ucHoloNETEntryMetaData);

                    HoloNETManager.Instance.ShowStatusMessage($"HoloNET Entry (Internal Connection) Deleted.", StatusMessageType.Success, false, ucHoloNETEntry);
                    HoloNETManager.Instance.LogMessage($"APP: HoloNET Entry (Internal Connection) Deleted. {GetHoloNETEntryMetaData(HoloNETManager.Instance.HoloNETEntry)}");
                    this.Visibility = Visibility.Hidden;
                }
                else
                {
                    ucHoloNETEntry.ShowStatusMessage("Error Occured Deleting HoloNET Entry (Internal Connection)", StatusMessageType.Error);
                    HoloNETManager.Instance.ShowStatusMessage($"Error Occured Deleting HoloNET Entry (Internal Connection): {result.Message}", StatusMessageType.Error, false);
                    HoloNETManager.Instance.LogMessage($"APP: Error Occured Deleting HoloNET Entry (Internal Connection): {result.Message}");
                }
            });
        }

        private void btnHoloNETEntryPopupCancel_Click(object sender, RoutedEventArgs e)
        {
            ucHoloNETEntry.HideStatusMessage();
            this.Visibility = Visibility.Hidden;
            PopupManager.CurrentPopup = null;
        }
    }
}
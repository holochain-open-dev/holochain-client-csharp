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

        private async Task InitPopupAsync()
        {
            ShowHoloNETEntryTab();
            HoloNETEntryUIManager.CurrentHoloNETEntryUI = ucHoloNETEntry;

            //This extra check here will not normally be needed in a normal hApp (because you will not have the admin UI allowing you to uninstall, disable or disconnect).
            //But for extra defencive coding to be on the safe side you can of course double check! ;-)
            bool showAlreadyInitMessage = true;

            if (HoloNETManager.Instance.HoloNETEntry != null)
                showAlreadyInitMessage = await HoloNETManager.Instance.CheckIfDemoAppReadyAsync(true);

            //If we intend to re-use an object then we can store it globally so we only need to init once...
            if (HoloNETManager.Instance.HoloNETEntry == null)
            {
                HoloNETManager.Instance.LogMessage("APP: Initializing HoloNET Entry...");
                HoloNETManager.Instance.ShowStatusMessage("Initializing HoloNET Entry...", StatusMessageType.Information, true, ucHoloNETEntry);

                // If the HoloNET Entry is null then we need to init it now...
                // Will init the HoloNET Entry which includes installing and enabling the app, signing credentials, attaching the app interface, then finally creating and connecting to the internal instance of the HoloNETClient.
                await HoloNETManager.Instance.InitHoloNETEntry();
            }
            else if (showAlreadyInitMessage)
            {
                HoloNETManager.Instance.ShowStatusMessage($"APP: HoloNET Entry Already Initialized.", StatusMessageType.Information, false, ucHoloNETEntry);
                HoloNETManager.Instance.LogMessage($"HoloNET Entry Already Initialized..");
            }

            //Non async way.
            //If you use Load, Save or Delete non-async versions you will need to wait for the OnInitialized event to fire before calling.
            //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...

            //HoloNETManager.Instance.ShowStatusMessage($"APP: Loading HoloNET Data Entry...", StatusMessageType.Information, true);
            //HoloNETManager.Instance.LogMessage($"APP: Loading HoloNET Data Entry...");

            //HoloNETManager.Instance.HoloNETEntry.Load(HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarEntryHash); //For this OnLoaded event handler above is required //TODO: Check if this works without waiting for OnInitialized event!


            //TODO: TEMP! REMOVE AFTER!
            RefreshHoloNETEntryMetaData(HoloNETManager.Instance.HoloNETEntry, ucHoloNETEntryMetaData);


            //Async way.
            if (HoloNETManager.Instance.HoloNETEntry != null && HoloNETEntryDNAManager.HoloNETEntryDNA != null && !string.IsNullOrEmpty(HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarEntryHash))
            {
                HoloNETManager.Instance.ShowStatusMessage($"APP: Loading HoloNET Data Entry...", StatusMessageType.Information, true, ucHoloNETEntry);
                HoloNETManager.Instance.LogMessage($"APP: Loading HoloNET Data Entry...");

                //LoadAsync (as well as SaveAsync & DeleteAsync) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                ZomeFunctionCallBackEventArgs result = await HoloNETManager.Instance.HoloNETEntry.LoadAsync(HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarEntryHash); //No event handlers are needed.

                if (result.IsCallSuccessful && !result.IsError)
                {
                    HoloNETManager.Instance.ShowStatusMessage($"APP: HoloNET Entry Loaded.", StatusMessageType.Success, false, ucHoloNETEntry);
                    HoloNETManager.Instance.LogMessage($"APP: HoloNET Entry Loaded. {GetHoloNETEntryMetaData(HoloNETManager.Instance.HoloNETEntry)}");

                    ucHoloNETEntry.DataContext = HoloNETManager.Instance.HoloNETEntry;
                    RefreshHoloNETEntryMetaData(HoloNETManager.Instance.HoloNETEntry, ucHoloNETEntryMetaData);
                }
                else
                {
                    ucHoloNETEntry.ShowStatusMessage(result.Message, StatusMessageType.Error);
                    HoloNETManager.Instance.ShowStatusMessage($"APP: Error Occured Loading Entry: {result.Message}", StatusMessageType.Error, false, ucHoloNETEntry);
                    HoloNETManager.Instance.LogMessage($"APP: Error Occured Loading Entry: {result.Message}");
                }
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
            return $"EntryHash: {HoloNETManager.Instance.HoloNETEntry.EntryHash}, Created By: {HoloNETManager.Instance.HoloNETEntry.CreatedBy}, Created Date: {HoloNETManager.Instance.HoloNETEntry.CreatedDate}, Modified By: {HoloNETManager.Instance.HoloNETEntry.ModifiedBy}, Modified Date: {HoloNETManager.Instance.HoloNETEntry.ModifiedDate}, Version: {HoloNETManager.Instance.HoloNETEntry.Version}, Previous Version Hash: {HoloNETManager.Instance.HoloNETEntry.PreviousVersionEntryHash}, Id: {HoloNETManager.Instance.HoloNETEntry.Id}, IsActive: {HoloNETManager.Instance.HoloNETEntry.IsActive}, Action Sequence: {HoloNETManager.Instance.HoloNETEntry.EntryData.ActionSequence}, EntryType: {HoloNETManager.Instance.HoloNETEntry.EntryData.EntryType}, Hash: {HoloNETManager.Instance.HoloNETEntry.EntryData.Hash}, OriginalActionAddress: {HoloNETManager.Instance.HoloNETEntry.EntryData.OriginalActionAddress}, OriginalEntryAddress: {HoloNETManager.Instance.HoloNETEntry.EntryData.OriginalEntryAddress}, Signature: {HoloNETManager.Instance.HoloNETEntry.EntryData.Signature}, TimeStamp: {HoloNETManager.Instance.HoloNETEntry.EntryData.Timestamp}, Type: {HoloNETManager.Instance.HoloNETEntry.EntryData.Type}";
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
                    ///In case it has not been init properly when the popup was opened (it should have been though!)
                    if (HoloNETManager.Instance.HoloNETEntry == null)
                    {
                        HoloNETManager.Instance.LogMessage("APP: Initializing HoloNET Entry...");
                        HoloNETManager.Instance.ShowStatusMessage("Initializing HoloNET Entry...", StatusMessageType.Information, true);

                        // If the HoloNET Entry is null then we need to init it now...
                        // Will init the HoloNET Entry which includes installing and enabling the app, signing credentials, attaching the app interface, then finally creating and connecting to the internal instance of the HoloNETClient.
                        await HoloNETManager.Instance.InitHoloNETEntry();
                    }

                    //Non async way.
                    //If you use Load, Save or Delete non-async versions you will need to wait for the OnInitialized event to fire before calling.
                    //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...

                    //HoloNETManager.Instance.ShowStatusMessage($"APP: Saving HoloNET Data Entry...", StatusMessageType.Information, true);
                    //HoloNETManager.Instance.LogMessage($"APP: Saving HoloNET Data Entry...");

                    //string[] parts = txtHoloNETEntryDOB.Text.Split('/');

                    //HoloNETManager.Instance.HoloNETEntry.FirstName = txtHoloNETEntryFirstName.Text;
                    //HoloNETManager.Instance.HoloNETEntry.LastName = txtHoloNETEntryLastName.Text;
                    //HoloNETManager.Instance.HoloNETEntry.DOB = new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0]));
                    //HoloNETManager.Instance.HoloNETEntry.Email = txtHoloNETEntryEmail.Text;

                    //HoloNETManager.Instance.HoloNETEntry.Save(); //For this OnSaved event handler above is required (only if you want to see what the result was!) //TODO: Check if this works without waiting for OnInitialized event!

                    //Async way.
                    if (HoloNETManager.Instance.HoloNETEntry != null)
                    {
                        HoloNETManager.Instance.ShowStatusMessage($"APP: Saving HoloNET Data Entry...", StatusMessageType.Information, true);
                        HoloNETManager.Instance.LogMessage($"APP: Saving HoloNET Data Entry...");

                        HoloNETManager.Instance.HoloNETEntry.FirstName = ucHoloNETEntry.FirstName;
                        HoloNETManager.Instance.HoloNETEntry.LastName = ucHoloNETEntry.LastName;
                        HoloNETManager.Instance.HoloNETEntry.DOB = ucHoloNETEntry.DOBDateTime;
                        HoloNETManager.Instance.HoloNETEntry.Email = ucHoloNETEntry.Email;

                        //SaveAsync (as well as LoadAsync and DeleteAsync) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                        ZomeFunctionCallBackEventArgs result = await HoloNETManager.Instance.HoloNETEntry.SaveAsync(); //No event handlers are needed.

                        if (result.IsCallSuccessful && !result.IsError)
                        {
                            //Persist the entryhash so the next time we start the app we can re-load the entry from the hash.
                            HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarEntryHash = HoloNETManager.Instance.HoloNETEntry.EntryHash;
                            HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarEntryHash = result.Entries[0].EntryHash; //We can also get the entryHash from the callback eventargs.
                            HoloNETEntryDNAManager.SaveDNA();

                            RefreshHoloNETEntryMetaData(HoloNETManager.Instance.HoloNETEntry, ucHoloNETEntryMetaData);

                            HoloNETManager.Instance.ShowStatusMessage($"APP: HoloNET Entry Saved.", StatusMessageType.Success);
                            HoloNETManager.Instance.LogMessage($"APP: HoloNET Entry Saved. {GetHoloNETEntryMetaData(HoloNETManager.Instance.HoloNETEntry)}");
                            this.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            ucHoloNETEntry.ShowStatusMessage(result.Message, StatusMessageType.Error);
                            HoloNETManager.Instance.ShowStatusMessage($"APP: Error Occured Saving Entry: {result.Message}", StatusMessageType.Error);
                            HoloNETManager.Instance.LogMessage($"APP: Error Occured Saving Entry: {result.Message}");
                        }
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
                ///In case it has not been init properly when the popup was opened (it should have been though!)
                if (HoloNETManager.Instance.HoloNETEntry == null)
                {
                    HoloNETManager.Instance.LogMessage("APP: Initializing HoloNET Entry...");
                    HoloNETManager.Instance.ShowStatusMessage("Initializing HoloNET Entry...", StatusMessageType.Information, true);

                    // If the HoloNET Entry is null then we need to init it now...
                    // Will init the HoloNET Entry which includes installing and enabling the app, signing credentials, attaching the app interface, then finally creating and connecting to the internal instance of the HoloNETClient.
                    await HoloNETManager.Instance.InitHoloNETEntry();
                }

                //Non async way.
                //If you use Load, Save or Delete non-async versions you will need to wait for the OnInitialized event to fire before calling.
                //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...

                //HoloNETManager.Instance.ShowStatusMessage($"APP: Deleting HoloNET Data Entry...", StatusMessageType.Information, true);
                //HoloNETManager.Instance.LogMessage($"APP: Deleting HoloNET Data Entry...");

                //HoloNETManager.Instance.HoloNETEntry.Delete(); //For this OnDeleted event handler above is required (only if you want to see what the result was!) //TODO: Check if this works without waiting for OnInitialized event!

                //Async way.
                if (HoloNETManager.Instance.HoloNETEntry != null)
                {
                    HoloNETManager.Instance.ShowStatusMessage($"APP: Deleting HoloNET Data Entry...", StatusMessageType.Information, true);
                    HoloNETManager.Instance.LogMessage($"APP: Deleting HoloNET Data Entry...");

                    //DeleteAsync (as well as LoadAsync and SaveAsync) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                    ZomeFunctionCallBackEventArgs result = await HoloNETManager.Instance.HoloNETEntry.DeleteAsync(); //No event handlers are needed.

                    if (result.IsCallSuccessful && !result.IsError)
                    {
                        //Persist the entryhash so the next time we start the app we can re-load the entry from the hash.
                        HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarEntryHash = HoloNETManager.Instance.HoloNETEntry.EntryHash;
                        HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarEntryHash = result.Entries[0].EntryHash; //We can also get the entryHash from the callback eventargs.
                        HoloNETEntryDNAManager.SaveDNA();

                        RefreshHoloNETEntryMetaData(HoloNETManager.Instance.HoloNETEntry, ucHoloNETEntryMetaData);

                        HoloNETManager.Instance.ShowStatusMessage($"APP: HoloNET Entry Deleted.", StatusMessageType.Success);
                        HoloNETManager.Instance.LogMessage($"APP: HoloNET Entry Deleted. {GetHoloNETEntryMetaData(HoloNETManager.Instance.HoloNETEntry)}");
                        this.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        ucHoloNETEntry.ShowStatusMessage(result.Message, StatusMessageType.Error);
                        HoloNETManager.Instance.ShowStatusMessage($"APP: Error Occured Deleting Entry: {result.Message}", StatusMessageType.Error);
                        HoloNETManager.Instance.LogMessage($"APP: Error Occured Deleting Entry: {result.Message}");
                    }
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
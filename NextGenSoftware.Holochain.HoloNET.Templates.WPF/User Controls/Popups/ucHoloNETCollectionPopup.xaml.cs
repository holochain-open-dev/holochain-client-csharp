using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Managers;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Models;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF.UserControls
{
    /// <summary>
    /// Interaction logic for ucHoloNETMetaData.xaml
    /// </summary>
    public partial class ucHoloNETCollectionPopup : UserControl
    {
        public ucHoloNETCollectionPopup()
        {
            InitializeComponent();
            DataContext = this;
            this.Loaded += UcHoloNETCollectionPopup_Loaded;
            this.IsVisibleChanged += UcHoloNETCollectionPopup_IsVisibleChanged;
            this.Unloaded += UcHoloNETCollectionPopup_Unloaded;
        }

        private void UcHoloNETCollectionPopup_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
                Dispatcher.InvokeAsync(async () => { await InitPopupAsync(); });
        }

        private async Task InitPopupAsync()
        {
            ShowHoloNETCollectionTab();
            HoloNETEntryUIManager.CurrentHoloNETEntryUI = ucHoloNETCollectionEntryInternal;

            //This extra check here will not normally be needed in a normal hApp (because you will not have the admin UI allowing you to uninstall, disable or disconnect).
            //But for extra defencive coding to be on the safe side you can of course double check! ;-)
            bool showAlreadyInitMessage = true;

            if (HoloNETManager.Instance.HoloNETEntries != null)
                showAlreadyInitMessage = await HoloNETManager.Instance.CheckIfDemoAppReadyAsync(false);

            //If we intend to re-use an object then we can store it globally so we only need to init once...
            if (HoloNETManager.Instance.HoloNETEntries == null)
            {
                HoloNETManager.Instance.LogMessage("APP: Initializing HoloNET Collection...");
                HoloNETManager.Instance.ShowStatusMessage("Initializing HoloNET Collection...", StatusMessageType.Information, true, ucHoloNETCollectionEntryInternal);

                // If the HoloNET Collection is null then we need to init it now...
                // Will init the HoloNET Collection which includes installing and enabling the app, signing credentials, attaching the app interface, then finally creating and connecting to the internal instance of the HoloNETClient.
                await HoloNETManager.Instance.InitHoloNETCollectionAsync();
            }
            else if (showAlreadyInitMessage)
            {
                HoloNETManager.Instance.ShowStatusMessage($"APP: HoloNET Collection Already Initialized.", StatusMessageType.Information, false, ucHoloNETCollectionEntryInternal);
                HoloNETManager.Instance.LogMessage($"HoloNET Collection Already Initialized..");
            }

            //Non async way.
            //If you use LoadCollection or SaveAllChanges non-async versions you will need to wait for the OnInitialized event to fire before calling.
            //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...

            //HoloNETManager.Instance.HoloNETEntries.LoadCollection(); //For this OnCollectionLoaded event handler above is required //TODO: Check if this works without waiting for OnInitialized event!

            //Async way.
            if (HoloNETManager.Instance.HoloNETEntries != null)
            {
                HoloNETManager.Instance.ShowStatusMessage($"APP: Loading HoloNET Collection...", StatusMessageType.Information, true, ucHoloNETCollectionEntryInternal);
                HoloNETManager.Instance.LogMessage($"APP: Loading HoloNET Collection...");

                //LoadCollectionAsync will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                HoloNETCollectionLoadedResult<Avatar> result = await HoloNETManager.Instance.HoloNETEntries.LoadCollectionAsync(); //No event handlers are needed.

                if (!result.IsError)
                {
                    HoloNETManager.Instance.ShowStatusMessage($"APP: HoloNET Collection Loaded.", StatusMessageType.Success, false, ucHoloNETCollectionEntryInternal);
                    HoloNETManager.Instance.LogMessage($"APP: HoloNET Collection Loaded.");

                    gridDataEntriesInternal.ItemsSource = HoloNETManager.Instance.HoloNETEntries;
                }
                else
                {
                    //ucHoloNETCollectionEntryInternal.HoloNETManager.Instance.ShowStatusMessage(result.Message, StatusMessageType.Error);
                    HoloNETManager.Instance.ShowStatusMessage($"APP: Error Occured Loading HoloNET Collection: {result.Message}", StatusMessageType.Error, false, ucHoloNETCollectionEntryInternal);
                    HoloNETManager.Instance.LogMessage($"APP: Error Occured Loading HoloNET Collection: {result.Message}");
                }
            }
        }

        private void ShowHoloNETCollectionTab()
        {
            btnViewHoloNETCollection.IsEnabled = false;
            btnViewHoloNETCollectionInfo.IsEnabled = true;
            stkpnlHoloNETCollectionInternal.Visibility = Visibility.Visible;
            stkpnlInfoHoloNETCollection.Visibility = Visibility.Collapsed;
        }

        private void ShowInfoTab()
        {
            btnViewHoloNETCollection.IsEnabled = true;
            btnViewHoloNETCollectionInfo.IsEnabled = false;
            stkpnlHoloNETCollectionInternal.Visibility = Visibility.Collapsed;
            stkpnlInfoHoloNETCollection.Visibility = Visibility.Visible;
        }

        private void CheckForChangesInHoloNETCollection()
        {
            if (HoloNETManager.Instance.HoloNETEntries != null && HoloNETManager.Instance.HoloNETEntries.IsChanges)
                btnHoloNETCollectionPopupSaveChanges.IsEnabled = true;
            else
                btnHoloNETCollectionPopupSaveChanges.IsEnabled = false;
        }

        private void UcHoloNETCollectionPopup_Loaded(object sender, RoutedEventArgs e)
        {
            ucHoloNETCollectionEntryInternal.txtHoloNETEntryFirstName.TextChanged += TxtHoloNETEntryInternalFirstName_TextChanged;
            ucHoloNETCollectionEntryInternal.txtHoloNETEntryLastName.TextChanged += TxtHoloNETEntryInternalLastName_TextChanged;
            ucHoloNETCollectionEntryInternal.txtHoloNETEntryDOB.TextChanged += TxtHoloNETEntryInternalDOB_TextChanged;
            ucHoloNETCollectionEntryInternal.txtHoloNETEntryEmail.TextChanged += TxtHoloNETEntryInternalEmail_TextChanged;
        }

        private void UcHoloNETCollectionPopup_Unloaded(object sender, RoutedEventArgs e)
        {
            ucHoloNETCollectionEntryInternal.txtHoloNETEntryFirstName.TextChanged -= TxtHoloNETEntryInternalFirstName_TextChanged;
            ucHoloNETCollectionEntryInternal.txtHoloNETEntryLastName.TextChanged -= TxtHoloNETEntryInternalLastName_TextChanged;
            ucHoloNETCollectionEntryInternal.txtHoloNETEntryDOB.TextChanged -= TxtHoloNETEntryInternalDOB_TextChanged;
            ucHoloNETCollectionEntryInternal.txtHoloNETEntryEmail.TextChanged -= TxtHoloNETEntryInternalEmail_TextChanged;
        }

        private void btnViewHoloNETCollection_Click(object sender, RoutedEventArgs e)
        {
            ShowHoloNETCollectionTab();
        }

        private void btnViewHoloNETCollectionInfo_Click(object sender, RoutedEventArgs e)
        {
            ShowInfoTab();
        }

        private void btnHoloNETCollectionPopupClose_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            PopupManager.CurrentPopup = null;
        }

        private void btnHoloNETCollectionPopupRemoveEntryFromCollection_Click(object sender, RoutedEventArgs e)
        {
            Avatar avatar = gridDataEntriesInternal.SelectedItem as Avatar;

            if (avatar != null)
            {
                //btnHoloNETCollectionPopupAddEntryToCollection.IsEnabled = false;
                btnHoloNETCollectionPopupRemoveEntryFromCollection.IsEnabled = true;

                //Remove the item from the list(we could re-load the list from hc here but it is more efficient to just remove it from the in -memory collection).
                int index = -1;
                for (int i = 0; i < HoloNETManager.Instance.HoloNETEntries.Count; i++)
                {
                    if (HoloNETManager.Instance.HoloNETEntries[i] != null && HoloNETManager.Instance.HoloNETEntries[i].Id == avatar.Id)
                    {
                        index = i;
                        break;
                    }
                }

                if (index > -1)
                {
                    HoloNETManager.Instance.HoloNETEntries.RemoveAt(index);
                    gridDataEntriesInternal.ItemsSource = null;
                    gridDataEntriesInternal.ItemsSource = HoloNETManager.Instance.HoloNETEntries;
                    btnHoloNETCollectionPopupRemoveEntryFromCollection.IsEnabled = false;

                    //This would normally be needed if the UI did not support binding like WPF/XAML does.
                    //CheckForChangesInHoloNETCollection();
                }
            }
        }

        private void btnHoloNETCollectionPopupAddEntryToCollection_Click(object sender, RoutedEventArgs e)
        {
            if (ucHoloNETCollectionEntryInternal.Validate())
            {
                HoloNETManager.Instance.HoloNETEntries.Add(new Avatar()
                {
                    Id = Guid.NewGuid(),
                    FirstName = ucHoloNETCollectionEntryInternal.txtHoloNETEntryFirstName.Text,
                    LastName = ucHoloNETCollectionEntryInternal.txtHoloNETEntryLastName.Text,
                    DOB = ucHoloNETCollectionEntryInternal.DOBDateTime,
                    Email = ucHoloNETCollectionEntryInternal.txtHoloNETEntryEmail.Text,
                });

                gridDataEntriesInternal.ItemsSource = HoloNETManager.Instance.HoloNETEntries;
                btnHoloNETCollectionPopupSaveChanges.IsEnabled = true;
            }
            else
                ShowHoloNETCollectionTab();
        }

        private void btnHoloNETCollectionPopupSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync(async () =>
            {
                HoloNETManager.Instance.ShowStatusMessage($"Saving Changes For Collection...", StatusMessageType.Information, true);
                HoloNETManager.Instance.LogMessage($"APP: Saving Changes For Collection...");

                //Will save all changes made to the collection (add, remove and updates).
                HoloNETCollectionSavedResult result = await HoloNETManager.Instance.HoloNETEntries.SaveAllChangesAsync();

                if (result != null && !result.IsError)
                {
                    HoloNETManager.Instance.ShowStatusMessage($"Changes Saved For Collection.", StatusMessageType.Success, false);
                    HoloNETManager.Instance.LogMessage($"APP: Changes Saved For Collection.");
                    btnHoloNETCollectionPopupSaveChanges.IsEnabled = false;

                    gridDataEntriesInternal.ItemsSource = null;
                    gridDataEntriesInternal.ItemsSource = HoloNETManager.Instance.HoloNETEntries;

                    ucHoloNETCollectionEntryInternal.FirstName = "";
                    ucHoloNETCollectionEntryInternal.LastName = "";
                    ucHoloNETCollectionEntryInternal.DOB = "";
                    ucHoloNETCollectionEntryInternal.Email = "";
                }
                else
                {
                    ucHoloNETCollectionEntryInternal.ShowStatusMessage(result.Message, StatusMessageType.Error);
                    HoloNETManager.Instance.ShowStatusMessage($"Error Occured Saving Changes For Collection: {result.Message}", StatusMessageType.Error);
                    HoloNETManager.Instance.LogMessage($"APP: Error Occured Saving Changes For Collection: {result.Message}");

                    //TODO:TEMP, REMOVE AFTER!
                    gridDataEntriesInternal.ItemsSource = null;
                    gridDataEntriesInternal.ItemsSource = HoloNETManager.Instance.HoloNETEntries;

                    ucHoloNETCollectionEntryInternal.FirstName = "";
                    ucHoloNETCollectionEntryInternal.LastName = "";
                    ucHoloNETCollectionEntryInternal.DOB = "";
                    ucHoloNETCollectionEntryInternal.Email = "";
                }
            });
        }

        private void gridDataEntriesInternal_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            Avatar currentAvatar = gridDataEntriesInternal.SelectedItem as Avatar;

            if (currentAvatar != null)
                btnHoloNETCollectionPopupRemoveEntryFromCollection.IsEnabled = true;
            else
                btnHoloNETCollectionPopupRemoveEntryFromCollection.IsEnabled = false;
        }

        private void TxtHoloNETEntryInternalEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            //This would normally be needed if the UI did not support binding like WPF/XAML does.
            CheckForChangesInHoloNETCollection();
        }

        private void TxtHoloNETEntryInternalDOB_TextChanged(object sender, TextChangedEventArgs e)
        {
            //This would normally be needed if the UI did not support binding like WPF/XAML does.
            CheckForChangesInHoloNETCollection();
        }

        private void TxtHoloNETEntryInternalLastName_TextChanged(object sender, TextChangedEventArgs e)
        {
            //This would normally be needed if the UI did not support binding like WPF/XAML does.
            CheckForChangesInHoloNETCollection();
        }

        private void TxtHoloNETEntryInternalFirstName_TextChanged(object sender, TextChangedEventArgs e)
        {
            //This would normally be needed if the UI did not support binding like WPF/XAML does.
            CheckForChangesInHoloNETCollection();
        }
    }
}
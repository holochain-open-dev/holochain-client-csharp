using System;
using System.Windows;
using System.Windows.Controls;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.ORM.Entries;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Managers;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Models;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Objects;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF.UserControls
{
    /// <summary>
    /// Interaction logic for ucHoloNETMetaData.xaml
    /// </summary>
    public partial class ucHoloNETEntryAndCollectionSharedPopup : UserControl
    {
        public ucHoloNETEntryAndCollectionSharedPopup()
        {
            InitializeComponent();
            DataContext = this;
            this.Loaded += UcHoloNETEntryAndCollectionSharedPopup_Loaded;
            this.Unloaded += UcHoloNETEntryAndCollectionSharedPopup_Unloaded;
            this.IsVisibleChanged += UcHoloNETEntryAndCollectionSharedPopup_IsVisibleChanged;
        }

        private void UcHoloNETEntryAndCollectionSharedPopup_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
               InitPopup();
        }

        private void UcHoloNETEntryAndCollectionSharedPopup_Loaded(object sender, RoutedEventArgs e)
        {
            ucHoloNETCollectionEntryShared.txtHoloNETEntryFirstName.TextChanged += TxtHoloNETEntryFirstName_TextChanged;
            ucHoloNETCollectionEntryShared.txtHoloNETEntryLastName.TextChanged += TxtHoloNETEntryLastName_TextChanged;
            ucHoloNETCollectionEntryShared.txtHoloNETEntryDOB.TextChanged += TxtHoloNETEntryDOB_TextChanged;
            ucHoloNETCollectionEntryShared.txtHoloNETEntryEmail.TextChanged += TxtHoloNETEntryEmail_TextChanged;
        }

        private void UcHoloNETEntryAndCollectionSharedPopup_Unloaded(object sender, RoutedEventArgs e)
        {
            ucHoloNETCollectionEntryShared.txtHoloNETEntryFirstName.TextChanged -= TxtHoloNETEntryFirstName_TextChanged;
            ucHoloNETCollectionEntryShared.txtHoloNETEntryLastName.TextChanged -= TxtHoloNETEntryLastName_TextChanged;
            ucHoloNETCollectionEntryShared.txtHoloNETEntryDOB.TextChanged -= TxtHoloNETEntryDOB_TextChanged;
            ucHoloNETCollectionEntryShared.txtHoloNETEntryEmail.TextChanged -= TxtHoloNETEntryEmail_TextChanged;
        }

        private void InitPopup()
        {
            ShowHoloNETEntrySharedTab();
            ucHoloNETEntryShared.HideStatusMessage();
            ucHoloNETCollectionEntryShared.HideStatusMessage();

            ConnectToAppAgentClientResult result = HoloNETManager.Instance.ConnectToAppAgentClient();

            if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                HoloNETManager.Instance.LoadHoloNETEntryShared(result.AppAgentClient);
            else
                HoloNETManager.Instance.ClientOperation = ClientOperation.LoadHoloNETEntryShared;
        }

        public void ShowHoloNETEntrySharedTab()
        {
            HoloNETEntryUIManager.CurrentHoloNETEntryUI = ucHoloNETEntryShared;
            btnShowHoloNETCollection.IsEnabled = true;
            btnShowHoloNETEntryShared.IsEnabled = false;
            btnHoloNETEntrySharedInfo.IsEnabled = true;
            stkpnlHoloNETEntryShared.Visibility = Visibility.Visible;
            stkpnlHoloNETCollection.Visibility = Visibility.Collapsed;
            stkpnlInfoHoloNETEntryShared.Visibility = Visibility.Collapsed;
            btnHoloNETEntrySharedPopupSave.Visibility = Visibility.Visible;
            btnDataEntriesPopupAddEntryToCollection.Visibility = Visibility.Collapsed;
            btnDataEntriesPopupRemoveEntryFromCollection.Visibility = Visibility.Collapsed;
            btnDataEntriesPopupUpdateEntryInCollection.Visibility = Visibility.Collapsed;
        }

        private void ShowHoloNETCollectionSharedTab()
        {
            HoloNETEntryUIManager.CurrentHoloNETEntryUI = ucHoloNETCollectionEntryShared;
            btnShowHoloNETCollection.IsEnabled = false;
            btnShowHoloNETEntryShared.IsEnabled = true;
            btnHoloNETEntrySharedInfo.IsEnabled = true;
            stkpnlHoloNETEntryShared.Visibility = Visibility.Collapsed;
            stkpnlHoloNETCollection.Visibility = Visibility.Visible;
            stkpnlInfoHoloNETEntryShared.Visibility = Visibility.Collapsed;
            btnHoloNETEntrySharedPopupSave.Visibility = Visibility.Collapsed;
            btnDataEntriesPopupAddEntryToCollection.Visibility = Visibility.Visible;
            btnDataEntriesPopupRemoveEntryFromCollection.Visibility = Visibility.Visible;
            btnDataEntriesPopupUpdateEntryInCollection.Visibility = Visibility.Visible;

            ConnectToAppAgentClientResult result = HoloNETManager.Instance.ConnectToAppAgentClient();

            if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                HoloNETManager.Instance.LoadCollection(result.AppAgentClient);
            else
                HoloNETManager.Instance.ClientOperation = ClientOperation.LoadHoloNETCollection;
        }

        private void ShowHoloNETEntrySharedInfoTab()
        {
            btnShowHoloNETCollection.IsEnabled = true;
            btnShowHoloNETEntryShared.IsEnabled = true;
            btnHoloNETEntrySharedInfo.IsEnabled = false;
            stkpnlHoloNETEntryShared.Visibility = Visibility.Collapsed;
            stkpnlHoloNETCollection.Visibility = Visibility.Collapsed;
            stkpnlInfoHoloNETEntryShared.Visibility = Visibility.Visible;
            btnHoloNETEntrySharedPopupSave.Visibility = Visibility.Collapsed;
            btnDataEntriesPopupAddEntryToCollection.Visibility = Visibility.Collapsed;
            btnDataEntriesPopupRemoveEntryFromCollection.Visibility = Visibility.Collapsed;
            btnDataEntriesPopupUpdateEntryInCollection.Visibility = Visibility.Collapsed;
        }

        private void CheckForChanges()
        {
            if (HoloNETManager.Instance.CurrentAvatar != null)
            {
                btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = false;

                if (ucHoloNETCollectionEntryShared.txtHoloNETEntryFirstName.Text != HoloNETManager.Instance.CurrentAvatar.FirstName)
                    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = true;

                if (ucHoloNETCollectionEntryShared.txtHoloNETEntryLastName.Text != HoloNETManager.Instance.CurrentAvatar.LastName)
                    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = true;

                if (ucHoloNETCollectionEntryShared.txtHoloNETEntryDOB.Text != HoloNETManager.Instance.CurrentAvatar.DOB.ToShortDateString())
                    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = true;

                if (ucHoloNETCollectionEntryShared.txtHoloNETEntryEmail.Text != HoloNETManager.Instance.CurrentAvatar.Email)
                    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = true;
            }
        }

        private void btnShowHoloNETCollection_Click(object sender, RoutedEventArgs e)
        {
            ShowHoloNETCollectionSharedTab();
        }

        private void btnShowHoloNETEntryShared_Click(object sender, RoutedEventArgs e)
        {
            ShowHoloNETEntrySharedTab();
        }

        private void btnHoloNETEntrySharedInfo_Click(object sender, RoutedEventArgs e)
        {
            ShowHoloNETEntrySharedInfoTab();
        }

        private void btnHoloNETEntrySharedPopupSave_Click(object sender, RoutedEventArgs e)
        {
            if (ucHoloNETEntryShared.Validate())
            {
                ConnectToAppAgentClientResult result = HoloNETManager.Instance.ConnectToAppAgentClient();

                if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                    HoloNETManager.Instance.SaveHoloNETEntryShared(result.AppAgentClient, ucHoloNETEntryShared.FirstName, ucHoloNETEntryShared.LastName, ucHoloNETEntryShared.DOBDateTime, ucHoloNETEntryShared.Email);
                else
                    HoloNETManager.Instance.ClientOperation = ClientOperation.SaveHoloNETEntryShared;
            }
        }

        private void btnDataEntriesPopupClose_Click(object sender, RoutedEventArgs e)
        {
            ucHoloNETCollectionEntryShared.HideStatusMessage();
            this.Visibility = Visibility.Collapsed;
            PopupManager.CurrentPopup = null;
        }

        private void btnDataEntriesPopupUpdateEntryInCollection_Click(object sender, RoutedEventArgs e)
        {
            if (HoloNETManager.Instance.CurrentAvatar != null)
            {
                Dispatcher.InvokeAsync(async () =>
                {
                    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = false;
                    btnDataEntriesPopupRemoveEntryFromCollection.IsEnabled = true;

                    HoloNETManager.Instance.CurrentAvatar.FirstName = ucHoloNETCollectionEntryShared.FirstName;
                    HoloNETManager.Instance.CurrentAvatar.LastName = ucHoloNETCollectionEntryShared.LastName;
                    HoloNETManager.Instance.CurrentAvatar.Email = ucHoloNETCollectionEntryShared.Email;
                    HoloNETManager.Instance.CurrentAvatar.DOB = ucHoloNETCollectionEntryShared.DOBDateTime;

                    HoloNETManager.Instance.ShowStatusMessage($"Updating HoloNETEntry In Collection...", StatusMessageType.Information, true);
                    HoloNETManager.Instance.LogMessage($"APP: Updating HoloNETEntry In Collection...");

                    HoloNETCollectionSavedResult result = await HoloNETManager.Instance.HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].SaveAllChangesAsync();

                    if (result != null && !result.IsError)
                    {
                        HoloNETManager.Instance.ShowStatusMessage($"HoloNETEntry Updated In Collection.", StatusMessageType.Success, false);
                        HoloNETManager.Instance.LogMessage($"APP: HoloNETEntry Updated In Collection.");

                        gridDataEntries.ItemsSource = null;
                        gridDataEntries.ItemsSource = HoloNETManager.Instance.HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name];

                        ucHoloNETCollectionEntryShared.FirstName = "";
                        ucHoloNETCollectionEntryShared.LastName = "";
                        ucHoloNETCollectionEntryShared.DOB = "";
                        ucHoloNETCollectionEntryShared.Email = "";
                    }
                    else
                    {
                        ucHoloNETCollectionEntryShared.ShowStatusMessage(result.Message, StatusMessageType.Error);
                        HoloNETManager.Instance.ShowStatusMessage($"Error Occured Updating HoloNETEntry In Collection: {result.Message}", StatusMessageType.Error);
                        HoloNETManager.Instance.LogMessage($"APP: Error Occured Updating HoloNETEntry In Collection: {result.Message}");

                        //TODO:TEMP, REMOVE AFTER!
                        gridDataEntries.ItemsSource = null;
                        gridDataEntries.ItemsSource = HoloNETManager.Instance.HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name];

                        ucHoloNETCollectionEntryShared.FirstName = "";
                        ucHoloNETCollectionEntryShared.LastName = "";
                        ucHoloNETCollectionEntryShared.DOB = "";
                        ucHoloNETCollectionEntryShared.Email = "";
                    }
                });
            }
        }

        private void btnDataEntriesPopupAddEntryToCollection_Click(object sender, RoutedEventArgs e)
        {
            if (ucHoloNETCollectionEntryShared.Validate())
            {
                //lblNewEntryValidationErrors.Visibility = Visibility.Hidden;
                ConnectToAppAgentClientResult result = HoloNETManager.Instance.ConnectToAppAgentClient();

                if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                {
                    HoloNETManager.Instance.AddHoloNETEntryToCollection(result.AppAgentClient, ucHoloNETCollectionEntryShared.FirstName, ucHoloNETCollectionEntryShared.LastName, ucHoloNETCollectionEntryShared.DOBDateTime, ucHoloNETCollectionEntryShared.Email);

                    //We could alternatively save the entry first and then add it to the collection afterwards but this is 2 round trips to the conductor whereas AddHoloNETEntryToCollectionAndSave is only 1 and will automatically save the entry before adding it to the collection.
                    //SaveHoloNETEntry(result.AppAgentClient, txtFirstName.Text, txtLastName.Text, txtDOB.Text, txtEmail.Text);
                }
                else
                    HoloNETManager.Instance.ClientOperation = ClientOperation.AddHoloNETEntryToCollection;
            }
        }

        private void btnDataEntriesPopupRemoveEntryFromCollection_Click(object sender, RoutedEventArgs e)
        {
            AvatarShared avatar = gridDataEntries.SelectedItem as AvatarShared;

            if (avatar != null)
            {
                Dispatcher.InvokeAsync(async () =>
                {
                    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = false;
                    btnDataEntriesPopupRemoveEntryFromCollection.IsEnabled = true;

                    HoloNETManager.Instance.ShowStatusMessage($"Removing HoloNETEntry From Collection...", StatusMessageType.Information, true);
                    HoloNETManager.Instance.LogMessage($"APP: Removing HoloNETEntry From Collection...");

                    ZomeFunctionCallBackEventArgs result = await HoloNETManager.Instance.HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].RemoveHoloNETEntryFromCollectionAndSaveAsync(avatar);

                    if (result != null && !result.IsError)
                    {
                        HoloNETManager.Instance.ShowStatusMessage($"HoloNETEntry Removed From Collection.", StatusMessageType.Success, false);
                        HoloNETManager.Instance.LogMessage($"APP: HoloNETEntry Removed From Collection.");

                        ucHoloNETCollectionEntryShared.FirstName = "";
                        ucHoloNETCollectionEntryShared.LastName = "";
                        ucHoloNETCollectionEntryShared.DOB = "";
                        ucHoloNETCollectionEntryShared.Email = "";

                        //Remove the item from the list (we could re-load the list from hc here but it is more efficient to just remove it from the in-memory collection).
                        //int index = -1;
                        //for (int i = 0; i < HoloNETEntries[CurrentApp.Name].Count; i++)
                        //{
                        //    if (HoloNETEntries[CurrentApp.Name][i] != null && HoloNETEntries[CurrentApp.Name][i].Id == avatar.Id)
                        //    {
                        //        index = i;
                        //        break;
                        //    }
                        //}

                        //if (index > -1)
                        //{
                        //    HoloNETEntries[CurrentApp.Name].RemoveAt(index);
                        //    gridDataEntries.ItemsSource = null;
                        //    gridDataEntries.ItemsSource = HoloNETEntries[CurrentApp.Name];
                        //    btnDataEntriesPopupRemoveEntryFromCollection.IsEnabled = false;
                        //    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = false;
                        //}
                    }
                    else
                    {
                        ucHoloNETCollectionEntryShared.FirstName = "";
                        ucHoloNETCollectionEntryShared.LastName = "";
                        ucHoloNETCollectionEntryShared.DOB = "";
                        ucHoloNETCollectionEntryShared.Email = "";

                        ucHoloNETCollectionEntryShared.ShowStatusMessage(result.Message, StatusMessageType.Error);
                        HoloNETManager.Instance.ShowStatusMessage($"Error Occured Removing HoloNETEntry From Collection: {result.Message}", StatusMessageType.Error);
                        HoloNETManager.Instance.LogMessage($"APP: Error Occured Removing HoloNETEntry From Collection: {result.Message}");
                    }
                });
            }
        }

        private void gridDataEntries_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            HoloNETManager.Instance.CurrentAvatar = gridDataEntries.SelectedItem as AvatarShared;

            if (HoloNETManager.Instance.CurrentAvatar != null)
            {
                btnDataEntriesPopupRemoveEntryFromCollection.IsEnabled = true;
                btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = false;

                ucHoloNETCollectionEntryShared.FirstName = HoloNETManager.Instance.CurrentAvatar.FirstName;
                ucHoloNETCollectionEntryShared.LastName = HoloNETManager.Instance.CurrentAvatar.LastName;
                ucHoloNETCollectionEntryShared.DOB = HoloNETManager.Instance.CurrentAvatar.DOB.ToShortDateString();
                ucHoloNETCollectionEntryShared.Email = HoloNETManager.Instance.CurrentAvatar.Email;
            }
            else
            {
                btnDataEntriesPopupRemoveEntryFromCollection.IsEnabled = false;
                btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = false;
            }
        }

        private void TxtHoloNETEntryEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckForChanges();
        }

        private void TxtHoloNETEntryDOB_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckForChanges();
        }

        private void TxtHoloNETEntryLastName_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckForChanges();
        }

        private void TxtHoloNETEntryFirstName_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckForChanges();
        }
    }
}
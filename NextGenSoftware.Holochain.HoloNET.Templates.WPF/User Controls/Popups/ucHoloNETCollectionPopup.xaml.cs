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
                Dispatcher.InvokeAsync(async () => await InitPopupAsync());
        }

        private async Task InitPopupAsync()
        {
            ShowHoloNETCollectionTab();
            HoloNETEntryUIManager.CurrentHoloNETEntryUI = ucHoloNETCollectionEntryInternal;
            HoloNETCollectionLoadedResult<Avatar> result = await HoloNETManager.Instance.LoadCollectionAsync();

            if (!result.IsError)
            {
                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Collection Loaded (Internal Connection).", StatusMessageType.Success, false, ucHoloNETCollectionEntryInternal);
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Collection Loaded (Internal Connection).");
                gridDataEntriesInternal.ItemsSource = HoloNETManager.Instance.HoloNETEntries;
            }
            else
            {
                ucHoloNETCollectionEntryInternal.ShowStatusMessage("Error Occured Loading HoloNET Collection (Internal Connection).", StatusMessageType.Error);
                HoloNETManager.Instance.ShowStatusMessage($"Error Occured Loading HoloNET Collection (Internal Connection): {result.Message}", StatusMessageType.Error, false);
                HoloNETManager.Instance.LogMessage($"APP: Error Occured Loading HoloNET Collection (Internal Connection): {result.Message}");
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
                //Will save all changes made to the collection (add, remove and updates).
                HoloNETCollectionSavedResult result = await HoloNETManager.Instance.SaveCollectionAsync();

                if (result != null && !result.IsError)
                {
                    HoloNETManager.Instance.ShowStatusMessage($"Changes Saved For HoloNET Collection (Internal Connection).", StatusMessageType.Success, false, ucHoloNETCollectionEntryInternal);
                    HoloNETManager.Instance.LogMessage($"APP: Changes Saved For HoloNET Collection (Internal Connection).");
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
                    ucHoloNETCollectionEntryInternal.ShowStatusMessage("Error Occured Saving Changes For HoloNET Collection (Internal Connection).", StatusMessageType.Error);
                    HoloNETManager.Instance.ShowStatusMessage($"Error Occured Saving Changes For HoloNET Collection (Internal Connection): {result.Message}", StatusMessageType.Error, false);
                    HoloNETManager.Instance.LogMessage($"APP: Error Occured Saving Changes For HoloNET Collection (Internal Connection): {result.Message}");

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
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NextGenSoftware.Holochain.HoloNET.Client;
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
        private bool _initCollection = false;

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
                Dispatcher.InvokeAsync(async () => await InitPopupAsync());
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

        private async Task InitPopupAsync()
        {
            ShowHoloNETEntrySharedTab();
            ucHoloNETEntryShared.HideStatusMessage();
            ucHoloNETCollectionEntryShared.HideStatusMessage();

            await LoadHoloNETEntryAsync();
        }

        private async Task LoadHoloNETEntryAsync()
        {
            ConnectToAppAgentClientResult connectedResult = HoloNETManager.Instance.ConnectToAppAgentClient();

            if (connectedResult.ResponseType == ConnectToAppAgentClientResponseType.Connected)
            {
                ZomeFunctionCallBackEventArgs result = await HoloNETManager.Instance.LoadHoloNETEntrySharedAsync(connectedResult.AppAgentClient);

                if (!result.IsError)
                {
                    HoloNETManager.Instance.ShowStatusMessage($"HoloNET Entry (Shared Connection) Loaded.", StatusMessageType.Success, false, ucHoloNETEntryShared);
                    HoloNETManager.Instance.LogMessage($"APP: HoloNET Entry (Shared Connection) Loaded.");

                    ucHoloNETEntryShared.DataContext = HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name];
                    ucHoloNETEntrySharedMetaData.DataContext = HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name];
                }
                else
                {
                    HoloNETManager.Instance.ShowStatusMessage($"Error Occured Loading HoloNET Entry (Shared Connection). Reason: {result.Message}", StatusMessageType.Error, false, ucHoloNETEntryShared);
                    HoloNETManager.Instance.LogMessage($"APP: Error Occured Loading HoloNET Entry (Shared Connection). Reason: {result.Message}");
                }
            }
            else
                HoloNETManager.Instance.ClientOperation = ClientOperation.LoadHoloNETEntryShared;
        }

        private async Task LoadHoloNETCollectionAsync()
        {
            ConnectToAppAgentClientResult connectedResult = HoloNETManager.Instance.ConnectToAppAgentClient();

            if (connectedResult.ResponseType == ConnectToAppAgentClientResponseType.Connected)
            {
                HoloNETCollectionLoadedResult<AvatarShared> result = await HoloNETManager.Instance.LoadCollectionAsync(connectedResult.AppAgentClient);

                if (!result.IsError)
                {
                    HoloNETManager.Instance.ShowStatusMessage($"HoloNET Collection Loaded.", StatusMessageType.Error, false, ucHoloNETCollectionEntryShared);
                    HoloNETManager.Instance.LogMessage($"APP: HoloNET Collection Loaded.");

                    //gridDataEntries.ItemsSource = result.Entries; //Can set it using this or line below.
                    gridDataEntries.ItemsSource = HoloNETManager.Instance.HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name];
                }
                else
                {
                    MockData();

                    HoloNETManager.Instance.ShowStatusMessage($"Error Occured Loading HoloNET Collection.", StatusMessageType.Error, false, ucHoloNETCollectionEntryShared);
                    HoloNETManager.Instance.LogMessage($"APP: Error Occured Loading HoloNET Collection. Reason: {result.Message}");
                }

                _initCollection = true;
            }
            else
                HoloNETManager.Instance.ClientOperation = ClientOperation.LoadHoloNETCollectionShared;
        }

        private void ShowHoloNETEntrySharedTab()
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

            if (!_initCollection)
                Dispatcher.InvokeAsync(async () => await LoadHoloNETCollectionAsync());
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
            Dispatcher.InvokeAsync(async () =>
            {
                ConnectToAppAgentClientResult connectedResult = HoloNETManager.Instance.ConnectToAppAgentClient();

                if (ucHoloNETEntryShared.Validate() && connectedResult.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                {
                    ZomeFunctionCallBackEventArgs result = await HoloNETManager.Instance.SaveHoloNETEntrySharedAsync(connectedResult.AppAgentClient, ucHoloNETEntryShared.FirstName, ucHoloNETEntryShared.LastName, ucHoloNETEntryShared.DOBDateTime, ucHoloNETEntryShared.Email);

                    if (!result.IsError)
                    {
                        HoloNETManager.Instance.ShowStatusMessage($"HoloNET Entry (Shared Connection) Saved.", StatusMessageType.Success, false, ucHoloNETEntryShared);
                        HoloNETManager.Instance.LogMessage($"APP: HoloNET Entry (Shared Connection) Saved.");
                    }
                    else
                    {
                        HoloNETManager.Instance.ShowStatusMessage($"Error Occured Saving HoloNET Entry (Shared Connection): {result.Message}", StatusMessageType.Error, false, ucHoloNETEntryShared);
                        HoloNETManager.Instance.LogMessage($"APP: Error Occured Saving HoloNET Entry (Shared Connection): {result.Message}");
                    }
                }
                else
                    HoloNETManager.Instance.ClientOperation = ClientOperation.SaveHoloNETEntryShared;
            });
        }

        private void btnDataEntriesPopupUpdateEntryInCollection_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync(async () =>
            {
                ConnectToAppAgentClientResult connectedResult = HoloNETManager.Instance.ConnectToAppAgentClient();

                if (connectedResult.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                { 
                    if (HoloNETManager.Instance.CurrentAvatar != null)
                    {
                        btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = false;
                        btnDataEntriesPopupRemoveEntryFromCollection.IsEnabled = true;

                        HoloNETManager.Instance.CurrentAvatar.FirstName = ucHoloNETCollectionEntryShared.FirstName;
                        HoloNETManager.Instance.CurrentAvatar.LastName = ucHoloNETCollectionEntryShared.LastName;
                        HoloNETManager.Instance.CurrentAvatar.Email = ucHoloNETCollectionEntryShared.Email;
                        HoloNETManager.Instance.CurrentAvatar.DOB = ucHoloNETCollectionEntryShared.DOBDateTime;

                        HoloNETCollectionSavedResult result = await HoloNETManager.Instance.UpdateHoloNETEntryInCollectionAsync(connectedResult.AppAgentClient);

                        if (result != null && !result.IsError)
                        {
                            HoloNETManager.Instance.ShowStatusMessage($"HoloNETEntry Updated In Collection.", StatusMessageType.Success, false, ucHoloNETCollectionEntryShared);
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
                            HoloNETManager.Instance.ShowStatusMessage($"Error Occured Updating HoloNETEntry In Collection: {result.Message}", StatusMessageType.Error, false, ucHoloNETCollectionEntryShared);
                            HoloNETManager.Instance.LogMessage($"APP: Error Occured Updating HoloNETEntry In Collection: {result.Message}");

                            //TODO:TEMP, REMOVE AFTER!
                            gridDataEntries.ItemsSource = null;
                            gridDataEntries.ItemsSource = HoloNETManager.Instance.HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name];

                            ucHoloNETCollectionEntryShared.FirstName = "";
                            ucHoloNETCollectionEntryShared.LastName = "";
                            ucHoloNETCollectionEntryShared.DOB = "";
                            ucHoloNETCollectionEntryShared.Email = "";
                        }
                    }
                }
                else
                    HoloNETManager.Instance.ClientOperation = ClientOperation.UpdateHoloNETEntryInCollectionShared;
            });
        }

        private void btnDataEntriesPopupAddEntryToCollection_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync(async () =>
            {
                if (ucHoloNETCollectionEntryShared.Validate())
                {
                    ConnectToAppAgentClientResult connectedResult = HoloNETManager.Instance.ConnectToAppAgentClient();

                    if (connectedResult.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                    { 
                        ZomeFunctionCallBackEventArgs result = await HoloNETManager.Instance.AddHoloNETEntryToCollectionAsync(connectedResult.AppAgentClient, ucHoloNETCollectionEntryShared.FirstName, ucHoloNETCollectionEntryShared.LastName, ucHoloNETCollectionEntryShared.DOBDateTime, ucHoloNETCollectionEntryShared.Email);

                        //We could alternatively save the entry first and then add it to the collection afterwards but this is 2 round trips to the conductor whereas AddHoloNETEntryToCollectionAndSave is only 1 and will automatically save the entry before adding it to the collection.
                        //SaveHoloNETEntry(result.AppAgentClient, txtFirstName.Text, txtLastName.Text, txtDOB.Text, txtEmail.Text);

                        if (!result.IsError)
                        {
                            gridDataEntries.ItemsSource = HoloNETManager.Instance.HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name];

                            HoloNETEntryUIManager.CurrentHoloNETEntryUI.FirstName = "";
                            HoloNETEntryUIManager.CurrentHoloNETEntryUI.LastName = "";
                            HoloNETEntryUIManager.CurrentHoloNETEntryUI.DOB = "";
                            HoloNETEntryUIManager.CurrentHoloNETEntryUI.Email = "";

                            HoloNETManager.Instance.ShowStatusMessage($"HoloNET Entry Added To Collection.", StatusMessageType.Success, false, ucHoloNETCollectionEntryShared);
                            HoloNETManager.Instance.LogMessage($"APP: HoloNET Entry Added To Collection.");
                        }
                        else
                        {
                            //TODO: TEMP! REMOVE AFTER!
                            gridDataEntries.ItemsSource = HoloNETManager.Instance.HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name];

                            //TODO: TEMP! REMOVE AFTER!
                            HoloNETEntryUIManager.CurrentHoloNETEntryUI.FirstName = "";
                            HoloNETEntryUIManager.CurrentHoloNETEntryUI.LastName = "";
                            HoloNETEntryUIManager.CurrentHoloNETEntryUI.DOB = "";
                            HoloNETEntryUIManager.CurrentHoloNETEntryUI.Email = "";

                            HoloNETManager.Instance.ShowStatusMessage($"Error Occured Adding HoloNET Entry To Collection.", StatusMessageType.Error, false, ucHoloNETCollectionEntryShared);
                            HoloNETManager.Instance.LogMessage($"APP: Error Occured Adding HoloNET Entry To Collection. Reason: {result.Message}");
                        }
                    }
                    else
                        HoloNETManager.Instance.ClientOperation = ClientOperation.AddHoloNETEntryToCollectionShared;
                }
            });
        }

        private void btnDataEntriesPopupRemoveEntryFromCollection_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync(async () =>
            {
                ConnectToAppAgentClientResult connectedResult = HoloNETManager.Instance.ConnectToAppAgentClient();

                if (connectedResult.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                { 
                    AvatarShared avatar = gridDataEntries.SelectedItem as AvatarShared;

                    if (avatar != null)
                    {
                        btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = false;
                        btnDataEntriesPopupRemoveEntryFromCollection.IsEnabled = true;

                        ZomeFunctionCallBackEventArgs result = await HoloNETManager.Instance.RemoveHoloNETEntryFromCollectionAsync(connectedResult.AppAgentClient, avatar);
    
                        if (result != null && !result.IsError)
                        {
                            HoloNETManager.Instance.ShowStatusMessage($"HoloNETEntry Removed From Collection.", StatusMessageType.Success, false, ucHoloNETCollectionEntryShared);
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

                            HoloNETManager.Instance.ShowStatusMessage($"Error Occured Removing HoloNETEntry From Collection: {result.Message}", StatusMessageType.Error, false, ucHoloNETCollectionEntryShared);
                            HoloNETManager.Instance.LogMessage($"APP: Error Occured Removing HoloNETEntry From Collection: {result.Message}");
                        }
                    }
                }
                else
                    HoloNETManager.Instance.ClientOperation = ClientOperation.RemoveHoloNETEntryFromCollectionShared;
            });
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

        private void btnDataEntriesPopupClose_Click(object sender, RoutedEventArgs e)
        {
            ucHoloNETCollectionEntryShared.HideStatusMessage();
            this.Visibility = Visibility.Collapsed;
            PopupManager.CurrentPopup = null;
        }

        //TODO: TEMP UNTIL REAL DATA IS RETURNED! REMOVE AFTER!
        private void MockData()
        {
            if (HoloNETManager.Instance.HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name] != null && HoloNETManager.Instance.HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].Count == 0)
            {
                if (HoloNETManager.Instance.CurrentApp.Name == HoloNETManager.Instance.HoloNETCollectionDemoAppId)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        AvatarShared avatar1 = new AvatarShared()
                        {
                            Id = Guid.NewGuid(),
                            FirstName = "David",
                            LastName = "Ellams",
                            Email = "davidellams@hotmail.com",
                            DOB = new DateTime(1980, 4, 11)
                        };

                        //TODO: REMOVE AFTER, TEMP TILL GET ZOMECALLS WORKING AGAIN!
                        avatar1.MockData();
                        HoloNETManager.Instance.HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].Add(avatar1);
                    }

                    AvatarShared avatar = new AvatarShared()
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "James",
                        LastName = "Ellams",
                        Email = "davidellams@hotmail.com",
                        DOB = new DateTime(1980, 4, 11)
                    };

                    //TODO: REMOVE AFTER, TEMP TILL GET ZOMECALLS WORKING AGAIN!
                    avatar.MockData();
                    HoloNETManager.Instance.HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].Add(avatar);

                    avatar = new AvatarShared()
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "Noah",
                        LastName = "Ellams",
                        Email = "davidellams@hotmail.com",
                        DOB = new DateTime(1980, 4, 11)
                    };

                    //TODO: REMOVE AFTER, TEMP TILL GET ZOMECALLS WORKING AGAIN!
                    avatar.MockData();
                    HoloNETManager.Instance.HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].Add(avatar);
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        AvatarShared avatar = new AvatarShared()
                        {
                            Id = Guid.NewGuid(),
                            FirstName = "Elba",
                            LastName = "Ellams",
                            Email = "davidellams@hotmail.com",
                            DOB = new DateTime(1980, 4, 11)
                        };

                        //TODO: REMOVE AFTER, TEMP TILL GET ZOMECALLS WORKING AGAIN!
                        avatar.MockData();
                        HoloNETManager.Instance.HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].Add(avatar);

                        avatar = new AvatarShared()
                        {
                            Id = Guid.NewGuid(),
                            FirstName = "David",
                            LastName = "Ellams",
                            Email = "davidellams@hotmail.com",
                            DOB = new DateTime(1980, 4, 11)
                        };

                        //TODO: REMOVE AFTER, TEMP TILL GET ZOMECALLS WORKING AGAIN!
                        avatar.MockData();
                        HoloNETManager.Instance.HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].Add(avatar);

                        avatar = new AvatarShared()
                        {
                            Id = Guid.NewGuid(),
                            FirstName = "James",
                            LastName = "Ellams",
                            Email = "davidellams@hotmail.com",
                            DOB = new DateTime(1980, 4, 11)
                        };

                        //TODO: REMOVE AFTER, TEMP TILL GET ZOMECALLS WORKING AGAIN!
                        avatar.MockData();
                        HoloNETManager.Instance.HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].Add(avatar);

                        avatar = new AvatarShared()
                        {
                            Id = Guid.NewGuid(),
                            FirstName = "Noah",
                            LastName = "Ellams",
                            Email = "davidellams@hotmail.com",
                            DOB = new DateTime(1980, 4, 11)
                        };

                        //TODO: REMOVE AFTER, TEMP TILL GET ZOMECALLS WORKING AGAIN!
                        avatar.MockData();
                        HoloNETManager.Instance.HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].Add(avatar);
                    }
                }
            }

            gridDataEntries.ItemsSource = HoloNETManager.Instance.HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name];
        }
    }
}
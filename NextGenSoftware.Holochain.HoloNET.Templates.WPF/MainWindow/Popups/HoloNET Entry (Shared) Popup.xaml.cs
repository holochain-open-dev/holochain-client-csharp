using System;
using System.Windows;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.ORM.Collections;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private void InitHoloNETEntryShared(HoloNETClient client)
        {
            LogMessage("APP: Initializing HoloNET Entry (Shared)...");
            ShowStatusMessage("Initializing HoloNET Entry (Shared)...", StatusMessageType.Information, true);

            if (!_holoNETEntryShared.ContainsKey(CurrentApp.Name) || (_holoNETEntryShared.ContainsKey(CurrentApp.Name) && _holoNETEntryShared[CurrentApp.Name] == null))
            {
                _holoNETEntryShared[CurrentApp.Name] = new AvatarShared(client);

                //If we are using SaveAsync (or LoadAsync) we do not need to worry about any events such as OnSaved if you don't need them.
                _holoNETEntryShared[CurrentApp.Name].OnInitialized += _holoNETEntryShared_OnInitialized;
                _holoNETEntryShared[CurrentApp.Name].OnLoaded += _holoNETEntryShared_OnLoaded;
                _holoNETEntryShared[CurrentApp.Name].OnClosed += _holoNETEntryShared_OnClosed;
                _holoNETEntryShared[CurrentApp.Name].OnSaved += _holoNETEntryShared_OnSaved;
                _holoNETEntryShared[CurrentApp.Name].OnDeleted += _holoNETEntryShared_OnDeleted;
                _holoNETEntryShared[CurrentApp.Name].OnError += _holoNETEntryShared_OnError;
            }
            else
                _holoNETEntryShared[CurrentApp.Name].HoloNETClient = client;
        }

        private void LoadHoloNETEntryShared(HoloNETClient client)
        {
            if (!_holoNETEntryShared.ContainsKey(CurrentApp.Name)
                || (_holoNETEntryShared.ContainsKey(CurrentApp.Name) && _holoNETEntryShared[CurrentApp.Name] == null)
                || (_holoNETEntryShared.ContainsKey(CurrentApp.Name) && _holoNETEntryShared[CurrentApp.Name] != null && !_holoNETEntryShared[CurrentApp.Name].IsInitialized))
                InitHoloNETEntryShared(client);

            else if (_holoNETEntryShared[CurrentApp.Name].HoloNETClient.EndPoint != client.EndPoint)
                _holoNETEntryShared[CurrentApp.Name].HoloNETClient = client;
            else
            {
                ShowStatusMessage($"HoloNET Entry (Shared) Already Initialized.", StatusMessageType.Success, false);
                LogMessage($"APP: HoloNET Entry (Shared) Already Initialized..");
            }

            if (_holoNETEntryShared.ContainsKey(CurrentApp.Name) && _holoNETEntryShared[CurrentApp.Name] != null)
            {
                //ShowStatusMessage($"APP: Loading HoloNET Entry Shared...", StatusMessageType.Information, true);
                //LogMessage($"APP: Loading HoloNET Entry Shared...");

                //Non-async way (you need to use the OnLoaded event to know when the entry has loaded.
                //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...
                //_holoNETEntryShared[CurrentApp.Name].Load(); 

                //Async way.
                Dispatcher.InvokeAsync(async () =>
                {
                    if (!string.IsNullOrEmpty(HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarSharedEntryHash))
                    {
                        ShowStatusMessage($"APP: Loading HoloNET Entry Shared...", StatusMessageType.Information, true, ucHoloNETEntryShared);
                        LogMessage($"Loading HoloNET Entry Shared...");

                        //LoadAsync (as well as all other async methods) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                        ZomeFunctionCallBackEventArgs result = await _holoNETEntryShared[CurrentApp.Name].LoadAsync(HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarSharedEntryHash); //No event handlers are needed but you can still use if you like.
                        HandleHoloNETEntrySharedLoaded(result);
                    }
                });
            }
        }

        private void SaveHoloNETEntryShared(HoloNETClient client, string firstName, string lastName, DateTime dob, string email)
        {
            //If we intend to re-use an object then we can store it globally so we only need to init once...
            if (_holoNETEntryShared[CurrentApp.Name] == null || (_holoNETEntryShared[CurrentApp.Name] != null && !_holoNETEntryShared[CurrentApp.Name].IsInitialized))
                InitHoloNETEntryShared(client);

            else if (_holoNETEntryShared[CurrentApp.Name].HoloNETClient.EndPoint != client.EndPoint)
                _holoNETEntryShared[CurrentApp.Name].HoloNETClient = client;

            if (_holoNETEntryShared != null && _holoNETEntryShared[CurrentApp.Name].IsInitialized)
            {
                _holoNETEntryShared[CurrentApp.Name].FirstName = firstName;
                _holoNETEntryShared[CurrentApp.Name].LastName = lastName;
                _holoNETEntryShared[CurrentApp.Name].DOB = dob; //new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0]));
                _holoNETEntryShared[CurrentApp.Name].Email = email;

                //Non async way.
                //If you use Load or Save non-async versions you will need to wait for the OnInitialized event to fire before calling.
                //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...
                //_holoNETEntry.Save(); //For this OnSaved event handler above is required (only if you want to see what the result was!) //TODO: Check if this works without waiting for OnInitialized event!

                //Async way.
                Dispatcher.InvokeAsync(async () =>
                {
                    ShowStatusMessage($"Saving HoloNET Data Entry (Shared)...", StatusMessageType.Information, true, ucHoloNETEntryShared);
                    LogMessage($"APP: Saving HoloNET Data Entry (Shared)...");

                    //SaveAsync (as well as LoadAsync) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                    ZomeFunctionCallBackEventArgs result = await _holoNETEntryShared[CurrentApp.Name].SaveAsync(); //No event handlers are needed.
                    HandleHoloNETEntrySharedSaved(result);
                });
            }
        }

        private void HandleHoloNETEntrySharedSaved(ZomeFunctionCallBackEventArgs result)
        {
            //TEMP to test!
            //HoloNETEntries.AddHoloNETEntryToCollectionAndSave(_holoNETEntry);

            if (result.IsCallSuccessful && !result.IsError)
            {
                ShowStatusMessage($"HoloNET Entry (Shared) Saved.", StatusMessageType.Success, false, ucHoloNETEntryShared);
                LogMessage($"APP: HoloNET Entry (Shared) Saved.");

                //Can use either of the lines below to get the EntryHash for the new entry.
                HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarSharedEntryHash = result.Entries[0].EntryHash;
                HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarSharedEntryHash = _holoNETEntryShared[CurrentApp.Name].EntryHash;
                HoloNETEntryDNAManager.SaveDNA();

                //Will add the entry to the collection and then persist the change to the hc/rust/happ code.
                //TODO: Dont think we need to call Save() on the entry before calling this method because this method will automatically save the entry and then add it to the collection. It can also of course add an existing entry to the collection. The same applies to the SaveCollection method below.
                //HoloNETEntries.AddHoloNETEntryToCollectionAndSave(result.Entries[0].EntryDataObject); 

                //Allows you to batch add/remove multiple entries to the collection and then persist the changes to the hc/rust/happ code in one go.
                //HoloNETEntries.Add(_holoNETEntry); //Will only add the entry to the collection in memory (it will NOT persist to hc/rust/happ code until SaveCollection is called.
                //HoloNETEntries.SaveCollection(); //Will look for any changes since the last time this method was called (includes entries added/removed from the collection as well as any changes made to entries themselves). This can invoke multiple events including OnHoloNETEntryAddedToCollection, OnHoloNETEntryRemovedFromCollection & OnHoloNETEntriesUpdated (if any changes were made to the entries themselves))/
            }
            else
            {
                ucHoloNETEntryShared.ShowStatusMessage(result.Message, StatusMessageType.Error);
                ShowStatusMessage($"Error occured saving entry (Shared): {result.Message}", StatusMessageType.Error);
                LogMessage($"APP: Error occured saving entry (Shared): {result.Message}");
            }
        }
        private void ShowHoloNETEntrySharedTab()
        {
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

            ConnectToAppAgentClientResult result = ConnectToAppAgentClient();

            if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                LoadCollection(result.AppAgentClient);
            else
                _clientOperation = ClientOperation.LoadHoloNETCollection;
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

        private void HandleHoloNETEntrySharedLoaded(ZomeFunctionCallBackEventArgs result)
        {
            //TODO: TEMP, REMOVE AFTER!
            _holoNETEntryShared[CurrentApp.Name].Id = Guid.NewGuid();
            _holoNETEntryShared[CurrentApp.Name].CreatedBy = "David";
            _holoNETEntryShared[CurrentApp.Name].CreatedDate = DateTime.Now;
            _holoNETEntryShared[CurrentApp.Name].EntryHash = Guid.NewGuid().ToString();
            _holoNETEntryShared[CurrentApp.Name].PreviousVersionEntryHash = Guid.NewGuid().ToString();

            ucHoloNETEntryShared.DataContext = _holoNETEntryShared[CurrentApp.Name];
            RefreshHoloNETEntryMetaData(_holoNETEntryShared[CurrentApp.Name], ucHoloNETEntrySharedMetaData);

            if (result.IsCallSuccessful && !result.IsError)
            {
                ShowStatusMessage($"HoloNET Entry Shared Loaded.", StatusMessageType.Success, false, ucHoloNETEntryShared);
                LogMessage($"APP: HoloNET Entry Shared Loaded.");

                ucHoloNETEntryShared.DataContext = _holoNETEntryShared[CurrentApp.Name];
                RefreshHoloNETEntryMetaData(_holoNETEntryShared[CurrentApp.Name], ucHoloNETEntrySharedMetaData);
            }
            else
            {
                ucHoloNETEntryShared.ShowStatusMessage(result.Message, StatusMessageType.Error);
                //ShowStatusMessage($"Error Occured Loading HoloNET Entry Shared.", StatusMessageType.Error, false, ucHoloNETEntryShared);
                LogMessage($"APP: Error Occured Loading HoloNET Entry Shared. Reason: {result.Message}");
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

        private void btnViewDataEntries_Click(object sender, RoutedEventArgs e)
        {
            ShowHoloNETEntrySharedTab();
            ucHoloNETEntryShared.HideStatusMessage();
            ucHoloNETCollectionEntry.HideStatusMessage();

            popupDataEntries.Visibility = Visibility.Visible;
            ConnectToAppAgentClientResult result = ConnectToAppAgentClient();

            if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                LoadHoloNETEntryShared(result.AppAgentClient);
            else
                _clientOperation = ClientOperation.LoadHoloNETEntryShared;
        }

        private void btnHoloNETEntrySharedPopupSave_Click(object sender, RoutedEventArgs e)
        {
            if (ucHoloNETEntryShared.Validate())
            {
                ConnectToAppAgentClientResult result = ConnectToAppAgentClient();

                if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                    SaveHoloNETEntryShared(result.AppAgentClient, ucHoloNETEntryShared.FirstName, ucHoloNETEntryShared.LastName, ucHoloNETEntryShared.DOBDateTime, ucHoloNETEntryShared.Email);
                else
                    _clientOperation = ClientOperation.SaveHoloNETEntryShared;
            }
        }

        private void btnDataEntriesPopupClose_Click(object sender, RoutedEventArgs e)
        {
            //lblNewEntryValidationErrors.Visibility = Visibility.Hidden;
            ucHoloNETCollectionEntry.HideStatusMessage();
            popupDataEntries.Visibility = Visibility.Collapsed;
        }
    }
}
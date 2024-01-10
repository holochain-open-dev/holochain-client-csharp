using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.ORM.Collections;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    /// <summary>
    /// //NOTE: EVERY method on HoloNETClient can be called either async or non-async, in these examples we are using a mixture of async and non-async. Normally you would use async because it is less code and easier to follow but we wanted to test and demo both versions (and show how you would use non async as well as async versions)...
    /// </summary>
    public partial class MainWindow : Window
    {
        private void InitHoloNETCollection(HoloNETClient client)
        {
            LogMessage("APP: Initializing HoloNET Collection...");
            ShowStatusMessage("Initializing HoloNET Collection...", StatusMessageType.Information, true);

            if (!HoloNETEntriesShared.ContainsKey(CurrentApp.Name) || (HoloNETEntriesShared.ContainsKey(CurrentApp.Name) && HoloNETEntriesShared[CurrentApp.Name] == null))
            {
                HoloNETEntriesShared[CurrentApp.Name] = new HoloNETObservableCollection<AvatarShared>("oasis", "load_avatars", "add_avatar", "remove_avatar", client, "update_avatars");
                HoloNETEntriesShared[CurrentApp.Name].OnInitialized += HoloNETEntriesShared_OnInitialized;
                HoloNETEntriesShared[CurrentApp.Name].OnCollectionLoaded += HoloNETEntriesShared_OnCollectionLoaded;
                HoloNETEntriesShared[CurrentApp.Name].OnCollectionSaved += HoloNETEntriesShared_OnCollectionSaved;
                HoloNETEntriesShared[CurrentApp.Name].CollectionChanged += HoloNETEntriesShared_CollectionChanged;
                HoloNETEntriesShared[CurrentApp.Name].OnHoloNETEntryAddedToCollection += HoloNETEntriesShared_OnHoloNETEntryAddedToCollection;
                HoloNETEntriesShared[CurrentApp.Name].OnHoloNETEntryRemovedFromCollection += HoloNETEntriesShared_OnHoloNETEntryRemovedFromCollection;
                HoloNETEntriesShared[CurrentApp.Name].OnClosed += HoloNETEntriesShared_OnClosed;
                HoloNETEntriesShared[CurrentApp.Name].OnError += HoloNETEntriesShared_OnError;
            }
            else
                HoloNETEntriesShared[CurrentApp.Name].HoloNETClient = client;
        }

        private void LoadCollection(HoloNETClient client)
        {
            if (!HoloNETEntriesShared.ContainsKey(CurrentApp.Name)
                || (HoloNETEntriesShared.ContainsKey(CurrentApp.Name) && HoloNETEntriesShared[CurrentApp.Name] == null)
                || (HoloNETEntriesShared.ContainsKey(CurrentApp.Name) && HoloNETEntriesShared[CurrentApp.Name] != null && !HoloNETEntriesShared[CurrentApp.Name].IsInitialized))
                InitHoloNETCollection(client);

            else if (HoloNETEntriesShared[CurrentApp.Name].HoloNETClient.EndPoint != client.EndPoint)
                HoloNETEntriesShared[CurrentApp.Name].HoloNETClient = client;

            else
            {
                ShowStatusMessage($"HoloNET Collection Already Initialized.", StatusMessageType.Success, false);
                LogMessage($"APP: HoloNET Collection Already Initialized..");
            }

            if (HoloNETEntriesShared.ContainsKey(CurrentApp.Name) && HoloNETEntriesShared[CurrentApp.Name] != null)
            {
                //ShowStatusMessage($"APP: Loading HoloNET Collection...", StatusMessageType.Information, true);
                //LogMessage($"APP: Loading HoloNET Collection...");

                //Non-async way (you need to use the OnCollectionLoaded event to know when the collection has loaded and receive the data/collection.
                //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...
                //HoloNETEntries[CurrentApp.Name].LoadCollection(); 

                //Async way.
                Dispatcher.InvokeAsync(async () =>
                {
                    ShowStatusMessage($"Loading HoloNET Collection...", StatusMessageType.Information, true, ucHoloNETCollectionEntry);
                    LogMessage($"APP: Loading HoloNET Collection...");

                    //LoadCollectionAsync (as well as all other async methods) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                    HoloNETCollectionLoadedResult<AvatarShared> result = await HoloNETEntriesShared[CurrentApp.Name].LoadCollectionAsync(); //No event handlers are needed but you can still use if you like.
                    HandleHoloNETCollectionLoaded(result);
                });
            }
        }

        private void HandleHoloNETCollectionLoaded(HoloNETCollectionLoadedResult<AvatarShared> result)
        {
            //TODO: TEMP UNTIL REAL DATA IS RETURNED! REMOVE AFTER!
            if (HoloNETEntriesShared[CurrentApp.Name] != null && HoloNETEntriesShared[CurrentApp.Name].Count == 0)
            {
                if (CurrentApp.Name == _holoNETCollectionDemoAppId)
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
                        HoloNETEntriesShared[CurrentApp.Name].Add(avatar1);
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
                    HoloNETEntriesShared[CurrentApp.Name].Add(avatar);

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
                    HoloNETEntriesShared[CurrentApp.Name].Add(avatar);
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
                        HoloNETEntriesShared[CurrentApp.Name].Add(avatar);

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
                        HoloNETEntriesShared[CurrentApp.Name].Add(avatar);

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
                        HoloNETEntriesShared[CurrentApp.Name].Add(avatar);

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
                        HoloNETEntriesShared[CurrentApp.Name].Add(avatar);
                    }
                }
            }

            gridDataEntries.ItemsSource = HoloNETEntriesShared[CurrentApp.Name];

            if (!result.IsError)
            {
                ShowStatusMessage($"HoloNET Collection Loaded.", StatusMessageType.Error);
                LogMessage($"APP: HoloNET Collection Loaded.");
                //gridDataEntries.ItemsSource = result.Entries; //Can set it using this or line below.
                gridDataEntries.ItemsSource = HoloNETEntriesShared[CurrentApp.Name];
            }
            else
            {
                ucHoloNETCollectionEntry.ShowStatusMessage(result.Message, StatusMessageType.Error);
                ShowStatusMessage($"Error Occured Loading HoloNET Collection.", StatusMessageType.Error);
                LogMessage($"APP: Error Occured Loading HoloNET Collection. Reason: {result.Message}");
            }
        }

        private void AddHoloNETEntryToCollection(HoloNETClient client, string firstName, string lastName, DateTime dob, string email)
        {
            if (HoloNETEntriesShared == null || (HoloNETEntriesShared != null && !HoloNETEntriesShared[CurrentApp.Name].IsInitialized))
                InitHoloNETCollection(client);

            else if (HoloNETEntriesShared[CurrentApp.Name].HoloNETClient.EndPoint != client.EndPoint)
                HoloNETEntriesShared[CurrentApp.Name].HoloNETClient = client;

            if (HoloNETEntriesShared != null && HoloNETEntriesShared[CurrentApp.Name].IsInitialized)
            {
                //ShowStatusMessage($"APP: Adding HoloNET Entry To Collection...", StatusMessageType.Information, true);
                //LogMessage($"APP: Adding HoloNET Entry To Collection...");

                //Non-async way (you need to use the OnCollectionLoaded event to know when the collection has loaded and receive the data/collection.
                //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...
                //HoloNETEntries.AddHoloNETEntryToCollectionAndSave(new AvatarMultiple()
                //{
                //    FirstName = firstName,
                //    LastName = lastName,
                //    DOB = new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0])),
                //    Email = email
                //});

                //Async way.
                Dispatcher.InvokeAsync(async () =>
                {
                    ShowStatusMessage($"Adding HoloNET Entry To Collection...", StatusMessageType.Information, true, ucHoloNETCollectionEntry);
                    LogMessage($"APP: Adding HoloNET Entry To Collection...");

                    //Will add the entry to the collection and then persist the change to the hc/rust/happ code.
                    //We don't need to call Save() on the entry before calling this method because this method will automatically save the entry and then add it to the collection. It can also of course add an existing entry to the collection. The same applies to the SaveCollection method below.
                    ZomeFunctionCallBackEventArgs result = await HoloNETEntriesShared[CurrentApp.Name].AddHoloNETEntryToCollectionAndSaveAsync(new AvatarShared()
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        DOB = dob, //new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0])),
                        Email = email
                    });

                    HandleHoloNETEntryAddedToCollection(result);

                    //Allows you to batch add/remove multiple entries to the collection and then persist the changes to the hc/rust/happ code in one go.
                    //Will only add the entry to the collection in memory (it will NOT persist to hc/rust/happ code until SaveCollection is called.
                    //HoloNETEntries.Add(new AvatarMultiple()
                    //{
                    //    FirstName = firstName,
                    //    LastName = lastName,
                    //    DOB = new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0])),
                    //    Email = email
                    //});

                    //Will look for any changes since the last time this method was called (includes entries added/removed from the collection as well as any changes made to entries themselves). This can invoke multiple events including OnHoloNETEntryAddedToCollection, OnHoloNETEntryRemovedFromCollection & OnHoloNETEntriesUpdated (if any changes were made to the entries themselves))/
                    //HoloNETEntries.SaveCollection(); 
                });
            }
        }

        private void HandleHoloNETEntryAddedToCollection(ZomeFunctionCallBackEventArgs result)
        {
            if (result.IsCallSuccessful && !result.IsError)
            {
                gridDataEntries.ItemsSource = null;
                gridDataEntries.ItemsSource = HoloNETEntriesShared[CurrentApp.Name];

                ucHoloNETCollectionEntry.FirstName = "";
                ucHoloNETCollectionEntry.LastName = "";
                ucHoloNETCollectionEntry.DOB = "";
                ucHoloNETCollectionEntry.Email = "";

                ShowStatusMessage($"HoloNET Entry Added To Collection.", StatusMessageType.Success, false, ucHoloNETCollectionEntry);
                LogMessage($"APP: HoloNET Entry Added To Collection.");
            }
            else
            {
                //TODO: TEMP! REMOVE AFTER!
                gridDataEntries.ItemsSource = null;
                gridDataEntries.ItemsSource = HoloNETEntriesShared[CurrentApp.Name];

                //TODO: TEMP! REMOVE AFTER!
                ucHoloNETCollectionEntry.FirstName = "";
                ucHoloNETCollectionEntry.LastName = "";
                ucHoloNETCollectionEntry.DOB = "";
                ucHoloNETCollectionEntry.Email = "";

                ucHoloNETCollectionEntry.ShowStatusMessage(result.Message, StatusMessageType.Error);
                ShowStatusMessage($"Error Occured Adding HoloNET Entry To Collection.", StatusMessageType.Error);
                LogMessage($"APP: Error Occured Adding HoloNET Entry To Collection. Reason: {result.Message}");
            }
        }

        private void CheckForChanges()
        {
            if (_currentAvatar != null)
            {
                btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = false;

                if (ucHoloNETCollectionEntry.txtHoloNETEntryFirstName.Text != _currentAvatar.FirstName)
                    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = true;

                if (ucHoloNETCollectionEntry.txtHoloNETEntryLastName.Text != _currentAvatar.LastName)
                    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = true;

                if (ucHoloNETCollectionEntry.txtHoloNETEntryDOB.Text != _currentAvatar.DOB.ToShortDateString())
                    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = true;

                if (ucHoloNETCollectionEntry.txtHoloNETEntryEmail.Text != _currentAvatar.Email)
                    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = true;
            }
        }

        private void btnDataEntriesPopupUpdateEntryInCollection_Click(object sender, RoutedEventArgs e)
        {
            //AvatarShared avatar = gridDataEntries.SelectedItem as AvatarShared;

            if (_currentAvatar != null)
            {
                Dispatcher.InvokeAsync(async () =>
                {
                    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = false;
                    btnDataEntriesPopupRemoveEntryFromCollection.IsEnabled = true;

                    _currentAvatar.FirstName = ucHoloNETCollectionEntry.FirstName;
                    _currentAvatar.LastName = ucHoloNETCollectionEntry.LastName;
                    _currentAvatar.Email = ucHoloNETCollectionEntry.Email;
                    _currentAvatar.DOB = ucHoloNETCollectionEntry.DOBDateTime;

                    ShowStatusMessage($"Updating HoloNETEntry In Collection...", StatusMessageType.Information, true);
                    LogMessage($"APP: Updating HoloNETEntry In Collection...");

                    HoloNETCollectionSavedResult result = await HoloNETEntriesShared[CurrentApp.Name].SaveAllChangesAsync();

                    if (result != null && !result.IsError)
                    {
                        ShowStatusMessage($"HoloNETEntry Updated In Collection.", StatusMessageType.Success, false);
                        LogMessage($"APP: HoloNETEntry Updated In Collection.");

                        gridDataEntries.ItemsSource = null;
                        gridDataEntries.ItemsSource = HoloNETEntriesShared[CurrentApp.Name];

                        ucHoloNETCollectionEntry.FirstName = "";
                        ucHoloNETCollectionEntry.LastName = "";
                        ucHoloNETCollectionEntry.DOB = "";
                        ucHoloNETCollectionEntry.Email = "";
                    }
                    else
                    {
                        ucHoloNETCollectionEntry.ShowStatusMessage(result.Message, StatusMessageType.Error);
                        ShowStatusMessage($"Error Occured Updating HoloNETEntry In Collection: {result.Message}", StatusMessageType.Error);
                        LogMessage($"APP: Error Occured Updating HoloNETEntry In Collection: {result.Message}");

                        //TODO:TEMP, REMOVE AFTER!
                        gridDataEntries.ItemsSource = null;
                        gridDataEntries.ItemsSource = HoloNETEntriesShared[CurrentApp.Name];

                        ucHoloNETCollectionEntry.FirstName = "";
                        ucHoloNETCollectionEntry.LastName = "";
                        ucHoloNETCollectionEntry.DOB = "";
                        ucHoloNETCollectionEntry.Email = "";
                    }
                });
            }
        }

        private void btnDataEntriesPopupAddEntryToCollection_Click(object sender, RoutedEventArgs e)
        {
            if (ucHoloNETCollectionEntry.Validate())
            {
                //lblNewEntryValidationErrors.Visibility = Visibility.Hidden;
                ConnectToAppAgentClientResult result = ConnectToAppAgentClient();

                if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                {
                    AddHoloNETEntryToCollection(result.AppAgentClient, ucHoloNETCollectionEntry.FirstName, ucHoloNETCollectionEntry.LastName, ucHoloNETCollectionEntry.DOBDateTime, ucHoloNETCollectionEntry.Email);

                    //We could alternatively save the entry first and then add it to the collection afterwards but this is 2 round trips to the conductor whereas AddHoloNETEntryToCollectionAndSave is only 1 and will automatically save the entry before adding it to the collection.
                    //SaveHoloNETEntry(result.AppAgentClient, txtFirstName.Text, txtLastName.Text, txtDOB.Text, txtEmail.Text);
                }
                else
                    _clientOperation = ClientOperation.AddHoloNETEntryToCollection;
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

                    ShowStatusMessage($"Removing HoloNETEntry From Collection...", StatusMessageType.Information, true);
                    LogMessage($"APP: Removing HoloNETEntry From Collection...");

                    ZomeFunctionCallBackEventArgs result = await HoloNETEntriesShared[CurrentApp.Name].RemoveHoloNETEntryFromCollectionAndSaveAsync(avatar);

                    if (result != null && !result.IsError)
                    {
                        ShowStatusMessage($"HoloNETEntry Removed From Collection.", StatusMessageType.Success, false);
                        LogMessage($"APP: HoloNETEntry Removed From Collection.");

                        ucHoloNETCollectionEntry.FirstName = "";
                        ucHoloNETCollectionEntry.LastName = "";
                        ucHoloNETCollectionEntry.DOB = "";
                        ucHoloNETCollectionEntry.Email = "";

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
                        ucHoloNETCollectionEntry.FirstName = "";
                        ucHoloNETCollectionEntry.LastName = "";
                        ucHoloNETCollectionEntry.DOB = "";
                        ucHoloNETCollectionEntry.Email = "";

                        ucHoloNETCollectionEntry.ShowStatusMessage(result.Message, StatusMessageType.Error);
                        ShowStatusMessage($"Error Occured Removing HoloNETEntry From Collection: {result.Message}", StatusMessageType.Error);
                        LogMessage($"APP: Error Occured Removing HoloNETEntry From Collection: {result.Message}");
                    }
                });
            }
        }

        private void gridDataEntries_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            _currentAvatar = gridDataEntries.SelectedItem as AvatarShared;

            if (_currentAvatar != null)
            {
                btnDataEntriesPopupRemoveEntryFromCollection.IsEnabled = true;
                btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = false;

                ucHoloNETCollectionEntry.FirstName = _currentAvatar.FirstName;
                ucHoloNETCollectionEntry.LastName = _currentAvatar.LastName;
                ucHoloNETCollectionEntry.DOB = _currentAvatar.DOB.ToShortDateString();
                ucHoloNETCollectionEntry.Email = _currentAvatar.Email;
            }
            else
            {
                btnDataEntriesPopupRemoveEntryFromCollection.IsEnabled = false;
                btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = false;
            }
        }

        private void HoloNETEntriesShared_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Collection Initialized", StatusMessageType.Success);
                LogMessage($"APP: HoloNET Collection Initialized.");
            }
        }

        private void HoloNETEntriesShared_OnCollectionLoaded(object sender, HoloNETCollectionLoadedResult<AvatarShared> e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Collection Loaded", StatusMessageType.Success);
                LogMessage($"APP: HoloNET Collection Loaded: {GetEntryInfo(e.ZomeFunctionCallBackEventArgs)}");
            }
        }

        private void HoloNETEntriesShared_OnCollectionSaved(object sender, HoloNETCollectionSavedResult e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Data Entries Updated (Collection Updated)", StatusMessageType.Success);
                LogMessage($"APP: HoloNET Data Entries Updated (Collection Updated).");
            }
        }

        private void HoloNETEntriesShared_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            string msg = $"HoloNET Collection Changed. Action: {Enum.GetName(typeof(NotifyCollectionChangedAction), e.Action)}, New Items: {(e.NewItems != null ? e.NewItems.Count : 0)}, Old Items: {(e.OldItems != null ? e.OldItems.Count : 0)}";
            LogMessage($"APP: {msg}");
            ShowStatusMessage(msg, StatusMessageType.Information, true);
        }

        private void HoloNETEntriesShared_OnHoloNETEntryAddedToCollection(object sender, ZomeFunctionCallBackEventArgs e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Data Entry Added To Collection", StatusMessageType.Success);
                LogMessage($"APP: HoloNET Data Entry Added To Collection: {GetEntryInfo(e)}");
            }
        }

        private void HoloNETEntriesShared_OnHoloNETEntryRemovedFromCollection(object sender, ZomeFunctionCallBackEventArgs e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Data Entry Removed From Collection", StatusMessageType.Success);
                LogMessage($"APP: HoloNET Data Entry Removed From Collection: {GetEntryInfo(e)}");
            }
        }

        private void HoloNETEntriesShared_OnClosed(object sender, HoloNETShutdownEventArgs e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Collection Closed", StatusMessageType.Success);
                LogMessage($"APP: HoloNET Collection Closed.");
            }
        }

        private void HoloNETEntriesShared_OnError(object sender, HoloNETErrorEventArgs e)
        {
            ShowStatusMessage($"HoloNET Collection Error", StatusMessageType.Error);
            LogMessage($"APP: HoloNET Collection Error: {e.Reason}");
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
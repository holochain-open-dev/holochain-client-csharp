using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.ORM.Collections;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF.UserControls
{
    /// <summary>
    /// Interaction logic for ucHoloNETMetaData.xaml
    /// </summary>
    public partial class ucHoloNETEntryAndCollectionSharedPopup : UserControl
    {
        private Dictionary<string, AvatarShared> _holoNETEntryShared = new Dictionary<string, AvatarShared>();
        private Dictionary<string, HoloNETObservableCollection<AvatarShared>> HoloNETEntriesShared { get; set; } = new Dictionary<string, HoloNETObservableCollection<AvatarShared>>();

        public ucHoloNETEntryAndCollectionSharedPopup()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void InitHoloNETEntryShared(HoloNETClient client)
        {
            HoloNETManager.Instance.LogMessage("APP: Initializing HoloNET Entry (Shared)...");
            HoloNETManager.Instance.ShowStatusMessage("Initializing HoloNET Entry (Shared)...", StatusMessageType.Information, true);

            if (!_holoNETEntryShared.ContainsKey(HoloNETManager.Instance.CurrentApp.Name) || (_holoNETEntryShared.ContainsKey(HoloNETManager.Instance.CurrentApp.Name) && _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name] == null))
            {
                _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name] = new AvatarShared(client);

                //If we are using SaveAsync (or LoadAsync) we do not need to worry about any events such as OnSaved if you don't need them.
                _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].OnInitialized += _holoNETEntryShared_OnInitialized;
                _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].OnLoaded += _holoNETEntryShared_OnLoaded;
                _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].OnSaved += _holoNETEntryShared_OnSaved;
                _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].OnDeleted += _holoNETEntryShared_OnDeleted;
                _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].OnClosed += _holoNETEntryShared_OnClosed;
                _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].OnError += _holoNETEntryShared_OnError;
            }
            else
                _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].HoloNETClient = client;
        }

        private void LoadHoloNETEntryShared(HoloNETClient client)
        {
            if (!_holoNETEntryShared.ContainsKey(HoloNETManager.Instance.CurrentApp.Name)
                || (_holoNETEntryShared.ContainsKey(HoloNETManager.Instance.CurrentApp.Name) && _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name] == null)
                || (_holoNETEntryShared.ContainsKey(HoloNETManager.Instance.CurrentApp.Name) && _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name] != null && !_holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].IsInitialized))
                InitHoloNETEntryShared(client);

            else if (_holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].HoloNETClient.EndPoint != client.EndPoint)
                _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].HoloNETClient = client;
            else
            {
                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Entry (Shared) Already Initialized.", StatusMessageType.Success, false);
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Entry (Shared) Already Initialized..");
            }

            if (_holoNETEntryShared.ContainsKey(HoloNETManager.Instance.CurrentApp.Name) && _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name] != null)
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
                        HoloNETManager.Instance.ShowStatusMessage($"APP: Loading HoloNET Entry Shared...", StatusMessageType.Information, true, ucHoloNETEntryShared);
                        HoloNETManager.Instance.LogMessage($"Loading HoloNET Entry Shared...");

                        //LoadAsync (as well as all other async methods) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                        ZomeFunctionCallBackEventArgs result = await _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].LoadAsync(HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarSharedEntryHash); //No event handlers are needed but you can still use if you like.
                        HandleHoloNETEntrySharedLoaded(result);
                    }
                });
            }
        }

        private void SaveHoloNETEntryShared(HoloNETClient client, string firstName, string lastName, DateTime dob, string email)
        {
            //If we intend to re-use an object then we can store it globally so we only need to init once...
            if (_holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name] == null || (_holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name] != null && !_holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].IsInitialized))
                InitHoloNETEntryShared(client);

            else if (_holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].HoloNETClient.EndPoint != client.EndPoint)
                _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].HoloNETClient = client;

            if (_holoNETEntryShared != null && _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].IsInitialized)
            {
                _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].FirstName = firstName;
                _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].LastName = lastName;
                _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].DOB = dob; //new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0]));
                _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].Email = email;

                //Non async way.
                //If you use Load or Save non-async versions you will need to wait for the OnInitialized event to fire before calling.
                //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...
                //_holoNETEntry.Save(); //For this OnSaved event handler above is required (only if you want to see what the result was!) //TODO: Check if this works without waiting for OnInitialized event!

                //Async way.
                Dispatcher.InvokeAsync(async () =>
                {
                    HoloNETManager.Instance.ShowStatusMessage($"Saving HoloNET Data Entry (Shared)...", StatusMessageType.Information, true, ucHoloNETEntryShared);
                    HoloNETManager.Instance.LogMessage($"APP: Saving HoloNET Data Entry (Shared)...");

                    //SaveAsync (as well as LoadAsync) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                    ZomeFunctionCallBackEventArgs result = await _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].SaveAsync(); //No event handlers are needed.
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
                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Entry (Shared) Saved.", StatusMessageType.Success, false, ucHoloNETEntryShared);
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Entry (Shared) Saved.");

                //Can use either of the lines below to get the EntryHash for the new entry.
                HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarSharedEntryHash = result.Entries[0].EntryHash;
                HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarSharedEntryHash = _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].EntryHash;
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
                HoloNETManager.Instance.ShowStatusMessage($"Error occured saving entry (Shared): {result.Message}", StatusMessageType.Error);
                HoloNETManager.Instance.LogMessage($"APP: Error occured saving entry (Shared): {result.Message}");
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

            ConnectToAppAgentClientResult result = HoloNETManager.Instance.ConnectToAppAgentClient();

            if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                LoadCollection(result.AppAgentClient);
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

        private void HandleHoloNETEntrySharedLoaded(ZomeFunctionCallBackEventArgs result)
        {
            //TODO: TEMP, REMOVE AFTER!
            _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].Id = Guid.NewGuid();
            _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].CreatedBy = "David";
            _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].CreatedDate = DateTime.Now;
            _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].EntryHash = Guid.NewGuid().ToString();
            _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].PreviousVersionEntryHash = Guid.NewGuid().ToString();

            ucHoloNETEntryShared.DataContext = _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name];
            RefreshHoloNETEntryMetaData(_holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name], ucHoloNETEntrySharedMetaData);

            if (result.IsCallSuccessful && !result.IsError)
            {
                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Entry Shared Loaded.", StatusMessageType.Success, false, ucHoloNETEntryShared);
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Entry Shared Loaded.");

                ucHoloNETEntryShared.DataContext = _holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name];
                RefreshHoloNETEntryMetaData(_holoNETEntryShared[HoloNETManager.Instance.CurrentApp.Name], ucHoloNETEntrySharedMetaData);
            }
            else
            {
                ucHoloNETEntryShared.ShowStatusMessage(result.Message, StatusMessageType.Error);
                //ShowStatusMessage($"Error Occured Loading HoloNET Entry Shared.", StatusMessageType.Error, false, ucHoloNETEntryShared);
                HoloNETManager.Instance.LogMessage($"APP: Error Occured Loading HoloNET Entry Shared. Reason: {result.Message}");
            }
        }

        private void InitHoloNETCollection(HoloNETClient client)
        {
            HoloNETManager.Instance.LogMessage("APP: Initializing HoloNET Collection...");
            HoloNETManager.Instance.ShowStatusMessage("Initializing HoloNET Collection...", StatusMessageType.Information, true);

            if (!HoloNETEntriesShared.ContainsKey(HoloNETManager.Instance.CurrentApp.Name) || (HoloNETEntriesShared.ContainsKey(HoloNETManager.Instance.CurrentApp.Name) && HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name] == null))
            {
                HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name] = new HoloNETObservableCollection<AvatarShared>("oasis", "load_avatars", "add_avatar", "remove_avatar", client, "update_avatars");
                HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].OnInitialized += HoloNETEntriesShared_OnInitialized;
                HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].OnCollectionLoaded += HoloNETEntriesShared_OnCollectionLoaded;
                HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].OnCollectionSaved += HoloNETEntriesShared_OnCollectionSaved;
                HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].CollectionChanged += HoloNETEntriesShared_CollectionChanged;
                HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].OnHoloNETEntryAddedToCollection += HoloNETEntriesShared_OnHoloNETEntryAddedToCollection;
                HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].OnHoloNETEntryRemovedFromCollection += HoloNETEntriesShared_OnHoloNETEntryRemovedFromCollection;
                HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].OnClosed += HoloNETEntriesShared_OnClosed;
                HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].OnError += HoloNETEntriesShared_OnError;
            }
            else
                HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].HoloNETClient = client;
        }

        private void LoadCollection(HoloNETClient client)
        {
            if (!HoloNETEntriesShared.ContainsKey(HoloNETManager.Instance.CurrentApp.Name)
                || (HoloNETEntriesShared.ContainsKey(HoloNETManager.Instance.CurrentApp.Name) && HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name] == null)
                || (HoloNETEntriesShared.ContainsKey(HoloNETManager.Instance.CurrentApp.Name) && HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name] != null && !HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].IsInitialized))
                InitHoloNETCollection(client);

            else if (HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].HoloNETClient.EndPoint != client.EndPoint)
                HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].HoloNETClient = client;

            else
            {
                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Collection Already Initialized.", StatusMessageType.Success, false);
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Collection Already Initialized..");
            }

            if (HoloNETEntriesShared.ContainsKey(HoloNETManager.Instance.CurrentApp.Name) && HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name] != null)
            {
                //ShowStatusMessage($"APP: Loading HoloNET Collection...", StatusMessageType.Information, true);
                //LogMessage($"APP: Loading HoloNET Collection...");

                //Non-async way (you need to use the OnCollectionLoaded event to know when the collection has loaded and receive the data/collection.
                //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...
                //HoloNETEntries[CurrentApp.Name].LoadCollection(); 

                //Async way.
                Dispatcher.InvokeAsync(async () =>
                {
                    HoloNETManager.Instance.ShowStatusMessage($"Loading HoloNET Collection...", StatusMessageType.Information, true, ucHoloNETCollectionEntry);
                    HoloNETManager.Instance.LogMessage($"APP: Loading HoloNET Collection...");

                    //LoadCollectionAsync (as well as all other async methods) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                    HoloNETCollectionLoadedResult<AvatarShared> result = await HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].LoadCollectionAsync(); //No event handlers are needed but you can still use if you like.
                    HandleHoloNETCollectionLoaded(result);
                });
            }
        }

        private void HandleHoloNETCollectionLoaded(HoloNETCollectionLoadedResult<AvatarShared> result)
        {
            //TODO: TEMP UNTIL REAL DATA IS RETURNED! REMOVE AFTER!
            if (HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name] != null && HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].Count == 0)
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
                        HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].Add(avatar1);
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
                    HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].Add(avatar);

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
                    HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].Add(avatar);
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
                        HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].Add(avatar);

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
                        HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].Add(avatar);

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
                        HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].Add(avatar);

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
                        HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].Add(avatar);
                    }
                }
            }

            gridDataEntries.ItemsSource = HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name];

            if (!result.IsError)
            {
                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Collection Loaded.", StatusMessageType.Error);
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Collection Loaded.");
                //gridDataEntries.ItemsSource = result.Entries; //Can set it using this or line below.
                gridDataEntries.ItemsSource = HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name];
            }
            else
            {
                ucHoloNETCollectionEntry.ShowStatusMessage(result.Message, StatusMessageType.Error);
                HoloNETManager.Instance.ShowStatusMessage($"Error Occured Loading HoloNET Collection.", StatusMessageType.Error);
                HoloNETManager.Instance.LogMessage($"APP: Error Occured Loading HoloNET Collection. Reason: {result.Message}");
            }
        }

        private void AddHoloNETEntryToCollection(HoloNETClient client, string firstName, string lastName, DateTime dob, string email)
        {
            if (HoloNETEntriesShared == null || (HoloNETEntriesShared != null && !HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].IsInitialized))
                InitHoloNETCollection(client);

            else if (HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].HoloNETClient.EndPoint != client.EndPoint)
                HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].HoloNETClient = client;

            if (HoloNETEntriesShared != null && HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].IsInitialized)
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
                    HoloNETManager.Instance.ShowStatusMessage($"Adding HoloNET Entry To Collection...", StatusMessageType.Information, true, ucHoloNETCollectionEntry);
                    HoloNETManager.Instance.LogMessage($"APP: Adding HoloNET Entry To Collection...");

                    //Will add the entry to the collection and then persist the change to the hc/rust/happ code.
                    //We don't need to call Save() on the entry before calling this method because this method will automatically save the entry and then add it to the collection. It can also of course add an existing entry to the collection. The same applies to the SaveCollection method below.
                    ZomeFunctionCallBackEventArgs result = await HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].AddHoloNETEntryToCollectionAndSaveAsync(new AvatarShared()
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
                gridDataEntries.ItemsSource = HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name];

                ucHoloNETCollectionEntry.FirstName = "";
                ucHoloNETCollectionEntry.LastName = "";
                ucHoloNETCollectionEntry.DOB = "";
                ucHoloNETCollectionEntry.Email = "";

                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Entry Added To Collection.", StatusMessageType.Success, false, ucHoloNETCollectionEntry);
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Entry Added To Collection.");
            }
            else
            {
                //TODO: TEMP! REMOVE AFTER!
                gridDataEntries.ItemsSource = null;
                gridDataEntries.ItemsSource = HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name];

                //TODO: TEMP! REMOVE AFTER!
                ucHoloNETCollectionEntry.FirstName = "";
                ucHoloNETCollectionEntry.LastName = "";
                ucHoloNETCollectionEntry.DOB = "";
                ucHoloNETCollectionEntry.Email = "";

                ucHoloNETCollectionEntry.ShowStatusMessage(result.Message, StatusMessageType.Error);
                HoloNETManager.Instance.ShowStatusMessage($"Error Occured Adding HoloNET Entry To Collection.", StatusMessageType.Error);
                HoloNETManager.Instance.LogMessage($"APP: Error Occured Adding HoloNET Entry To Collection. Reason: {result.Message}");
            }
        }

        private void CheckForChanges()
        {
            if (HoloNETManager.Instance.CurrentAvatar != null)
            {
                btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = false;

                if (ucHoloNETCollectionEntry.txtHoloNETEntryFirstName.Text != HoloNETManager.Instance.CurrentAvatar.FirstName)
                    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = true;

                if (ucHoloNETCollectionEntry.txtHoloNETEntryLastName.Text != HoloNETManager.Instance.CurrentAvatar.LastName)
                    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = true;

                if (ucHoloNETCollectionEntry.txtHoloNETEntryDOB.Text != HoloNETManager.Instance.CurrentAvatar.DOB.ToShortDateString())
                    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = true;

                if (ucHoloNETCollectionEntry.txtHoloNETEntryEmail.Text != HoloNETManager.Instance.CurrentAvatar.Email)
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

        private void btnViewDataEntries_Click(object sender, RoutedEventArgs e)
        {
            ShowHoloNETEntrySharedTab();
            ucHoloNETEntryShared.HideStatusMessage();
            ucHoloNETCollectionEntry.HideStatusMessage();

            popupDataEntries.Visibility = Visibility.Visible;
            ConnectToAppAgentClientResult result = HoloNETManager.Instance.ConnectToAppAgentClient();

            if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                LoadHoloNETEntryShared(result.AppAgentClient);
            else
                HoloNETManager.Instance.ClientOperation = ClientOperation.LoadHoloNETEntryShared;
        }

        private void btnHoloNETEntrySharedPopupSave_Click(object sender, RoutedEventArgs e)
        {
            if (ucHoloNETEntryShared.Validate())
            {
                ConnectToAppAgentClientResult result = HoloNETManager.Instance.ConnectToAppAgentClient();

                if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                    SaveHoloNETEntryShared(result.AppAgentClient, ucHoloNETEntryShared.FirstName, ucHoloNETEntryShared.LastName, ucHoloNETEntryShared.DOBDateTime, ucHoloNETEntryShared.Email);
                else
                    HoloNETManager.Instance.ClientOperation = ClientOperation.SaveHoloNETEntryShared;
            }
        }

        private void btnDataEntriesPopupClose_Click(object sender, RoutedEventArgs e)
        {
            //lblNewEntryValidationErrors.Visibility = Visibility.Hidden;
            ucHoloNETCollectionEntry.HideStatusMessage();
            popupDataEntries.Visibility = Visibility.Collapsed;
        }

        private void btnDataEntriesPopupUpdateEntryInCollection_Click(object sender, RoutedEventArgs e)
        {
            //AvatarShared avatar = gridDataEntries.SelectedItem as AvatarShared;

            if (HoloNETManager.Instance.CurrentAvatar != null)
            {
                Dispatcher.InvokeAsync(async () =>
                {
                    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = false;
                    btnDataEntriesPopupRemoveEntryFromCollection.IsEnabled = true;

                    HoloNETManager.Instance.CurrentAvatar.FirstName = ucHoloNETCollectionEntry.FirstName;
                    HoloNETManager.Instance.CurrentAvatar.LastName = ucHoloNETCollectionEntry.LastName;
                    HoloNETManager.Instance.CurrentAvatar.Email = ucHoloNETCollectionEntry.Email;
                    HoloNETManager.Instance.CurrentAvatar.DOB = ucHoloNETCollectionEntry.DOBDateTime;

                    HoloNETManager.Instance.ShowStatusMessage($"Updating HoloNETEntry In Collection...", StatusMessageType.Information, true);
                    HoloNETManager.Instance.LogMessage($"APP: Updating HoloNETEntry In Collection...");

                    HoloNETCollectionSavedResult result = await HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].SaveAllChangesAsync();

                    if (result != null && !result.IsError)
                    {
                        HoloNETManager.Instance.ShowStatusMessage($"HoloNETEntry Updated In Collection.", StatusMessageType.Success, false);
                        HoloNETManager.Instance.LogMessage($"APP: HoloNETEntry Updated In Collection.");

                        gridDataEntries.ItemsSource = null;
                        gridDataEntries.ItemsSource = HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name];

                        ucHoloNETCollectionEntry.FirstName = "";
                        ucHoloNETCollectionEntry.LastName = "";
                        ucHoloNETCollectionEntry.DOB = "";
                        ucHoloNETCollectionEntry.Email = "";
                    }
                    else
                    {
                        ucHoloNETCollectionEntry.ShowStatusMessage(result.Message, StatusMessageType.Error);
                        HoloNETManager.Instance.ShowStatusMessage($"Error Occured Updating HoloNETEntry In Collection: {result.Message}", StatusMessageType.Error);
                        HoloNETManager.Instance.LogMessage($"APP: Error Occured Updating HoloNETEntry In Collection: {result.Message}");

                        //TODO:TEMP, REMOVE AFTER!
                        gridDataEntries.ItemsSource = null;
                        gridDataEntries.ItemsSource = HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name];

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
                ConnectToAppAgentClientResult result = HoloNETManager.Instance.ConnectToAppAgentClient();

                if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                {
                    AddHoloNETEntryToCollection(result.AppAgentClient, ucHoloNETCollectionEntry.FirstName, ucHoloNETCollectionEntry.LastName, ucHoloNETCollectionEntry.DOBDateTime, ucHoloNETCollectionEntry.Email);

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

                    ZomeFunctionCallBackEventArgs result = await HoloNETEntriesShared[HoloNETManager.Instance.CurrentApp.Name].RemoveHoloNETEntryFromCollectionAndSaveAsync(avatar);

                    if (result != null && !result.IsError)
                    {
                        HoloNETManager.Instance.ShowStatusMessage($"HoloNETEntry Removed From Collection.", StatusMessageType.Success, false);
                        HoloNETManager.Instance.LogMessage($"APP: HoloNETEntry Removed From Collection.");

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

                ucHoloNETCollectionEntry.FirstName = HoloNETManager.Instance.CurrentAvatar.FirstName;
                ucHoloNETCollectionEntry.LastName = HoloNETManager.Instance.CurrentAvatar.LastName;
                ucHoloNETCollectionEntry.DOB = HoloNETManager.Instance.CurrentAvatar.DOB.ToShortDateString();
                ucHoloNETCollectionEntry.Email = HoloNETManager.Instance.CurrentAvatar.Email;
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

        private void _holoNETEntryShared_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            HoloNETManager.Instance.ShowStatusMessage($"HoloNET Data Entry (Shared) Initialized", StatusMessageType.Success);
            HoloNETManager.Instance.LogMessage($"APP: HoloNET Data Entry (Shared) Initialized.");
        }

        private void _holoNETEntryShared_OnLoaded(object sender, ZomeFunctionCallBackEventArgs e)
        {
            HoloNETManager.Instance.ShowStatusMessage($"HoloNET Data Entry (Shared) Loaded", StatusMessageType.Success);
            HoloNETManager.Instance.LogMessage($"APP: HoloNET Data Entry (Shared) Loaded: {HoloNETManager.Instance.GetEntryInfo(e)}");
        }

        private void _holoNETEntryShared_OnSaved(object sender, ZomeFunctionCallBackEventArgs e)
        {
            HoloNETManager.Instance.ShowStatusMessage($"HoloNET Data Entry (Shared) Saved", StatusMessageType.Success);
            HoloNETManager.Instance.LogMessage($"APP: HoloNET Data Entry (Shared) Saved: {HoloNETManager.Instance.GetEntryInfo(e)}");
        }

        private void _holoNETEntryShared_OnDeleted(object sender, ZomeFunctionCallBackEventArgs e)
        {
            HoloNETManager.Instance.ShowStatusMessage($"HoloNET Data Entry (Shared) Deleted", StatusMessageType.Success);
            HoloNETManager.Instance.LogMessage($"APP: HoloNET Data Entry (Shared) Deleted: {HoloNETManager.Instance.GetEntryInfo(e)}");
        }

        private void _holoNETEntryShared_OnClosed(object sender, HoloNETShutdownEventArgs e)
        {
            HoloNETManager.Instance.ShowStatusMessage($"HoloNET Data Entry (Shared) Closed", StatusMessageType.Success);

            if (e.HolochainConductorsShutdownEventArgs != null)
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Data Entry (Shared) Closed: Number Of Holochain Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown}, Number Of Hc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, Number Of Rustc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}");
            else
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Data Entry (Shared) Closed: Number Of Holochain Exe Instances Shutdown: 0, Number Of Hc Exe Instances Shutdown: 0, Number Of Rustc Exe Instances Shutdown: 0");
        }

        private void _holoNETEntryShared_OnError(object sender, HoloNETErrorEventArgs e)
        {
            HoloNETManager.Instance.ShowStatusMessage($"HoloNET Data Entry (Shared) Error", StatusMessageType.Error);
            HoloNETManager.Instance.LogMessage($"APP: HoloNET Data Entry (Shared) Error: {e.Reason}");
        }

        private void HoloNETEntriesShared_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            if (!e.IsError)
            {
                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Collection Initialized", StatusMessageType.Success);
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Collection Initialized.");
            }
        }

        private void HoloNETEntriesShared_OnCollectionLoaded(object sender, HoloNETCollectionLoadedResult<AvatarShared> e)
        {
            if (!e.IsError)
            {
                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Collection Loaded", StatusMessageType.Success);
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Collection Loaded: {HoloNETManager.Instance.GetEntryInfo(e.ZomeFunctionCallBackEventArgs)}");
            }
        }

        private void HoloNETEntriesShared_OnCollectionSaved(object sender, HoloNETCollectionSavedResult e)
        {
            if (!e.IsError)
            {
                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Data Entries Updated (Collection Updated)", StatusMessageType.Success);
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Data Entries Updated (Collection Updated).");
            }
        }

        private void HoloNETEntriesShared_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            string msg = $"HoloNET Collection Changed. Action: {Enum.GetName(typeof(NotifyCollectionChangedAction), e.Action)}, New Items: {(e.NewItems != null ? e.NewItems.Count : 0)}, Old Items: {(e.OldItems != null ? e.OldItems.Count : 0)}";
            HoloNETManager.Instance.LogMessage($"APP: {msg}");
            HoloNETManager.Instance.ShowStatusMessage(msg, StatusMessageType.Information, true);
        }

        private void HoloNETEntriesShared_OnHoloNETEntryAddedToCollection(object sender, ZomeFunctionCallBackEventArgs e)
        {
            if (!e.IsError)
            {
                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Data Entry Added To Collection", StatusMessageType.Success);
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Data Entry Added To Collection: {HoloNETManager.Instance.GetEntryInfo(e)}");
            }
        }

        private void HoloNETEntriesShared_OnHoloNETEntryRemovedFromCollection(object sender, ZomeFunctionCallBackEventArgs e)
        {
            if (!e.IsError)
            {
                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Data Entry Removed From Collection", StatusMessageType.Success);
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Data Entry Removed From Collection: {HoloNETManager.Instance.GetEntryInfo(e)}");
            }
        }

        private void HoloNETEntriesShared_OnClosed(object sender, HoloNETShutdownEventArgs e)
        {
            if (!e.IsError)
            {
                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Collection Closed", StatusMessageType.Success);
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Collection Closed.");
            }
        }

        private void HoloNETEntriesShared_OnError(object sender, HoloNETErrorEventArgs e)
        {
            HoloNETManager.Instance.ShowStatusMessage($"HoloNET Collection Error", StatusMessageType.Error);
            HoloNETManager.Instance.LogMessage($"APP: HoloNET Collection Error: {e.Reason}");
        }
    }
}
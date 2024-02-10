using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.Holochain.HoloNET.ORM.Collections;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Interfaces;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Models;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF.Managers
{
    public partial class HoloNETManager : IHoloNETManager
    {
        public void InitHoloNETCollection(IHoloNETClientAppAgent client)
        {
            LogMessage("APP: Initializing HoloNET Collection (Shared Connection)...");
            ShowStatusMessage("Initializing HoloNET Collection (Shared Connection)...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

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

            LogMessage("APP: HoloNET Collection (Shared Connection) Initialized.");
            ShowStatusMessage("HoloNET Collection (Shared Connection) Initialized.", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
        }

        public async Task<HoloNETCollectionLoadedResult<AvatarShared>> LoadCollectionAsync(IHoloNETClientAppAgent client)
        {
            HoloNETCollectionLoadedResult<AvatarShared> result = new HoloNETCollectionLoadedResult<AvatarShared>();
            CheckIfHoloNETCollectionSharedInitOK(client);

            if (HoloNETEntriesShared.ContainsKey(CurrentApp.Name) && HoloNETEntriesShared[CurrentApp.Name] != null)
            {
                ShowStatusMessage($"Loading HoloNET Collection...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: Loading HoloNET Collection...");

                // Non-async way (you need to use the OnCollectionLoaded event to know when the collection has loaded and receive the data/collection..
                //HoloNETEntries[CurrentApp.Name].LoadCollection(); 

                // LoadCollectionAsync (as well as all other async methods) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                result = await HoloNETEntriesShared[CurrentApp.Name].LoadCollectionAsync(); //No event handlers are needed but you can still use if you like.
            }
            else
            {
                result.IsError = true;
                result.Message = "HoloNETCollection (Shared Connection) Failed To Initialize";
            }

            return result;
        }

        public async Task<ZomeFunctionCallBackEventArgs> AddHoloNETEntryToCollectionAsync(IHoloNETClientAppAgent client, string firstName, string lastName, DateTime dob, string email)
        {
            ZomeFunctionCallBackEventArgs result = new ZomeFunctionCallBackEventArgs();
            CheckIfHoloNETCollectionSharedInitOK(client);

            if (HoloNETEntriesShared != null && HoloNETEntriesShared[CurrentApp.Name].IsInitialized)
            {
                ShowStatusMessage($"Adding HoloNET Entry To Collection...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: Adding HoloNET Entry To Collection...");

                // Non-async way (you need to use the OnCollectionLoaded event to know when the collection has loaded and receive the data/collection.
                //HoloNETEntries.AddHoloNETEntryToCollectionAndSave(new AvatarMultiple()
                //{
                //    FirstName = firstName,
                //    LastName = lastName,
                //    DOB = new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0])),
                //    Email = email
                //});

                // Async version
                // Will add the entry to the collection and then persist the change to the hc/rust/happ code.
                // We don't need to call Save() on the entry before calling this method because this method will automatically save the entry and then add it to the collection. It can also of course add an existing entry to the collection. The same applies to the SaveCollection method below.
                result = await HoloNETEntriesShared[CurrentApp.Name].AddHoloNETEntryToCollectionAndSaveAsync(new AvatarShared()
                {
                    FirstName = firstName,
                    LastName = lastName,
                    DOB = dob, //new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0])),
                    Email = email
                });

                //HandleHoloNETEntryAddedToCollection(result);

                // Allows you to batch add/remove multiple entries to the collection and then persist the changes to the hc/rust/happ code in one go.
                // Will only add the entry to the collection in memory (it will NOT persist to hc/rust/happ code until SaveCollection is called.
                // HoloNETEntries.Add(new AvatarMultiple()
                //{
                //    FirstName = firstName,
                //    LastName = lastName,
                //    DOB = new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0])),
                //    Email = email
                //});

                // Will look for any changes since the last time this method was called (includes entries added/removed from the collection as well as any changes made to entries themselves).
                // HoloNETEntries.SaveCollection(); 

                // HINT: Look in HoloNETManager - HoloNETCollection.cs for example using SaveCollection.
            }
            else
            {
                result.IsError = true;
                result.Message = "HoloNETCollection (Shared Connection) Failed To Initialize";
            }

            return result;
        }

        public async Task<ZomeFunctionCallBackEventArgs> RemoveHoloNETEntryFromCollectionAsync(IHoloNETClientAppAgent client, IAvatarShared avatar)
        {
            ZomeFunctionCallBackEventArgs result = new ZomeFunctionCallBackEventArgs();
            CheckIfHoloNETCollectionSharedInitOK(client);

            if (HoloNETEntriesShared != null && HoloNETEntriesShared[CurrentApp.Name].IsInitialized)
            {
                ShowStatusMessage($"Removing HoloNETEntry From Collection...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: Removing HoloNETEntry From Collection...");

                result = await HoloNETEntriesShared[CurrentApp.Name].RemoveHoloNETEntryFromCollectionAndSaveAsync((AvatarShared)avatar);
            }
            else
            {
                result.IsError = true;
                result.Message = "HoloNETCollection (Shared Connection) Failed To Initialize";
            }

            return result;
        }

        public async Task<HoloNETCollectionSavedResult> UpdateHoloNETEntryInCollectionAsync(IHoloNETClientAppAgent client)
        {
            HoloNETCollectionSavedResult result = new HoloNETCollectionSavedResult();
            CheckIfHoloNETCollectionSharedInitOK(client);

            if (HoloNETEntriesShared != null && HoloNETEntriesShared[CurrentApp.Name].IsInitialized)
            {
                ShowStatusMessage($"Updating HoloNETEntry In Collection...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: Updating HoloNETEntry In Collection...");

                result = await HoloNETEntriesShared[CurrentApp.Name].SaveAllChangesAsync();
            }
            else
            {
                result.IsError = true;
                result.Message = "HoloNETCollection (Shared Connection) Failed To Initialize";
            }

            return result;
        }

        private void CheckIfHoloNETCollectionSharedInitOK(IHoloNETClientAppAgent client)
        {
            ShowStatusMessage($"Checking If HoloNET Collection (Shared Connection) Already Initialized...", StatusMessageType.Information, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
            LogMessage($"APP: Checking If HoloNET Collection (Shared Connection) Already Initialized...");

            // In case it has not been init properly when the popup was opened (it should have been though!)
            if (HoloNETEntriesShared == null ||
                (HoloNETEntriesShared != null && !HoloNETEntriesShared.ContainsKey(CurrentApp.Name)) ||
                (HoloNETEntriesShared != null && HoloNETEntriesShared.ContainsKey(CurrentApp.Name) && !HoloNETEntriesShared[CurrentApp.Name].IsInitialized))
                    InitHoloNETCollection(client);

            // Otherwise if the Endpoint does not match then set the correct internal HoloNETClient now.
            else if (HoloNETEntriesShared != null
                && HoloNETEntriesShared.ContainsKey(CurrentApp.Name)
                && HoloNETEntriesShared[CurrentApp.Name] != null
                && HoloNETEntriesShared[CurrentApp.Name].HoloNETClient != null
                && HoloNETEntriesShared[CurrentApp.Name].HoloNETClient.EndPoint != client.EndPoint)
                    HoloNETEntriesShared[CurrentApp.Name].HoloNETClient = client;

            else
            {
                ShowStatusMessage($"HoloNET Collection (Shared Connection) Already Initialized.", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: HoloNET Collection (Shared Connection) Already Initialized..");
            }
        }

        private void HoloNETEntriesShared_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Collection Initialized", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: HoloNET Collection Initialized.");
            }
        }

        private void HoloNETEntriesShared_OnCollectionLoaded(object sender, HoloNETCollectionLoadedResult<AvatarShared> e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Collection Loaded", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: HoloNET Collection Loaded: {GetEntryInfo(e.ZomeFunctionCallBackEventArgs)}");
            }
        }

        private void HoloNETEntriesShared_OnCollectionSaved(object sender, HoloNETCollectionSavedResult e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Data Entries Updated (Collection Updated)", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
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
                ShowStatusMessage($"HoloNET Data Entry Added To Collection", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: HoloNET Data Entry Added To Collection: {GetEntryInfo(e)}");
            }
        }

        private void HoloNETEntriesShared_OnHoloNETEntryRemovedFromCollection(object sender, ZomeFunctionCallBackEventArgs e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Data Entry Removed From Collection", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: HoloNET Data Entry Removed From Collection: {GetEntryInfo(e)}");
            }
        }

        private void HoloNETEntriesShared_OnClosed(object sender, HoloNETShutdownEventArgs e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Collection Closed", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: HoloNET Collection Closed.");
            }
        }

        private void HoloNETEntriesShared_OnError(object sender, HoloNETErrorEventArgs e)
        {
            ShowStatusMessage($"HoloNET Collection Error", StatusMessageType.Error, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
            LogMessage($"APP: HoloNET Collection Error: {e.Reason}");
        }
    }
}
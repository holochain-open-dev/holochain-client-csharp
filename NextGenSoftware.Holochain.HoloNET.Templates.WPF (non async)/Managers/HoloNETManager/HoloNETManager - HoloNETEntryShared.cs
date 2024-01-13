using System;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.ORM.Entries;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Models;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.UserControls;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF.Managers
{
    public partial class HoloNETManager
    {
        public void InitHoloNETEntryShared(HoloNETClient client)
        {
            HoloNETManager.Instance.LogMessage("APP: Initializing HoloNET Entry (Shared)...");
            HoloNETManager.Instance.ShowStatusMessage("Initializing HoloNET Entry (Shared)...", StatusMessageType.Information, true);

            if (!HoloNETManager.Instance.HoloNETEntryShared.ContainsKey(HoloNETManager.Instance.CurrentApp.Name) || (HoloNETManager.Instance.HoloNETEntryShared.ContainsKey(HoloNETManager.Instance.CurrentApp.Name) && HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name] == null))
            {
                HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name] = new AvatarShared(client);

                //If we are using SaveAsync (or LoadAsync) we do not need to worry about any events such as OnSaved if you don't need them.
                HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].OnInitialized += _holoNETEntryShared_OnInitialized;
                HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].OnLoaded += _holoNETEntryShared_OnLoaded;
                HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].OnSaved += _holoNETEntryShared_OnSaved;
                HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].OnDeleted += _holoNETEntryShared_OnDeleted;
                HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].OnClosed += _holoNETEntryShared_OnClosed;
                HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].OnError += _holoNETEntryShared_OnError;
            }
            else
                HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].HoloNETClient = client;
        }

        public async Task LoadHoloNETEntryShared(HoloNETClient client)
        {
            if (!HoloNETManager.Instance.HoloNETEntryShared.ContainsKey(HoloNETManager.Instance.CurrentApp.Name)
                || (HoloNETManager.Instance.HoloNETEntryShared.ContainsKey(HoloNETManager.Instance.CurrentApp.Name) && HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name] == null)
                || (HoloNETManager.Instance.HoloNETEntryShared.ContainsKey(HoloNETManager.Instance.CurrentApp.Name) && HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name] != null && !HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].IsInitialized))
                HoloNETManager.Instance.InitHoloNETEntryShared(client);

            else if (HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].HoloNETClient.EndPoint != client.EndPoint)
                HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].HoloNETClient = client;
            else
            {
                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Entry (Shared) Already Initialized.", StatusMessageType.Success, false);
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Entry (Shared) Already Initialized..");
            }

            if (HoloNETManager.Instance.HoloNETEntryShared.ContainsKey(HoloNETManager.Instance.CurrentApp.Name) && HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name] != null)
            {
                if (!string.IsNullOrEmpty(HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarSharedEntryHash))
                {
                    HoloNETManager.Instance.ShowStatusMessage($"APP: Loading HoloNET Entry Shared...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                    HoloNETManager.Instance.LogMessage($"Loading HoloNET Entry Shared...");

                    //Non-async way (you need to use the OnLoaded event to know when the entry has loaded.
                    //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...
                    //HoloNETManager.Instance.HoloNETEntryShared[CurrentApp.Name].Load(); 

                    //LoadAsync (as well as all other async methods) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                    ZomeFunctionCallBackEventArgs result = await HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].LoadAsync(HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarSharedEntryHash); //No event handlers are needed but you can still use if you like.
                    HandleHoloNETEntrySharedLoaded(result);
                }
            }
        }

        public async Task SaveHoloNETEntryShared(HoloNETClient client, string firstName, string lastName, DateTime dob, string email)
        {
            //If we intend to re-use an object then we can store it globally so we only need to init once...
            if (HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name] == null || (HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name] != null && !HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].IsInitialized))
                HoloNETManager.Instance.InitHoloNETEntryShared(client);

            else if (HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].HoloNETClient.EndPoint != client.EndPoint)
                HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].HoloNETClient = client;

            if (HoloNETManager.Instance.HoloNETEntryShared != null && HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].IsInitialized)
            {
                HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].FirstName = firstName;
                HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].LastName = lastName;
                HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].DOB = dob;
                HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].Email = email;

                HoloNETManager.Instance.ShowStatusMessage($"Saving HoloNET Data Entry (Shared)...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                HoloNETManager.Instance.LogMessage($"APP: Saving HoloNET Data Entry (Shared)...");

                //Non async way.
                //If you use Load or Save non-async versions you will need to wait for the OnInitialized event to fire before calling.
                //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...
                //_holoNETEntry.Save(); //For this OnSaved event handler above is required (only if you want to see what the result was!) //TODO: Check if this works without waiting for OnInitialized event!

                //SaveAsync (as well as LoadAsync) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                ZomeFunctionCallBackEventArgs result = await HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].SaveAsync(); //No event handlers are needed.
                HandleHoloNETEntrySharedSaved(result);
            }
        }

        private void HandleHoloNETEntrySharedLoaded(ZomeFunctionCallBackEventArgs result)
        {
            //TODO: TEMP, REMOVE AFTER!
            //HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].Id = Guid.NewGuid();
            //HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].CreatedBy = "David";
            //HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].CreatedDate = DateTime.Now;
            //HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].EntryHash = Guid.NewGuid().ToString();
            //HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].PreviousVersionEntryHash = Guid.NewGuid().ToString();

            if (result.IsCallSuccessful && !result.IsError)
            {
                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Entry Shared Loaded.", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Entry Shared Loaded.");

                HoloNETEntryUIManager.CurrentHoloNETEntryUI.DataContext = HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name];
                ucHoloNETEntryAndCollectionSharedPopup ucHoloNETEntryAndCollectionSharedPopup = PopupManager.CurrentPopup as ucHoloNETEntryAndCollectionSharedPopup;

                if (ucHoloNETEntryAndCollectionSharedPopup != null)
                    RefreshHoloNETEntryMetaData(HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name], ucHoloNETEntryAndCollectionSharedPopup.ucHoloNETEntrySharedMetaData);
            }
            else
            {
                HoloNETEntryUIManager.CurrentHoloNETEntryUI.ShowStatusMessage(result.Message, StatusMessageType.Error);
                HoloNETManager.Instance.LogMessage($"APP: Error Occured Loading HoloNET Entry Shared. Reason: {result.Message}");
            }
        }

        private void HandleHoloNETEntrySharedSaved(ZomeFunctionCallBackEventArgs result)
        {
            if (result.IsCallSuccessful && !result.IsError)
            {
                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Entry (Shared) Saved.", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Entry (Shared) Saved.");

                //Can use either of the lines below to get the EntryHash for the new entry.
                HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarSharedEntryHash = result.Entries[0].EntryHash;
                HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarSharedEntryHash = HoloNETManager.Instance.HoloNETEntryShared[HoloNETManager.Instance.CurrentApp.Name].EntryHash;
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
                HoloNETEntryUIManager.CurrentHoloNETEntryUI.ShowStatusMessage(result.Message, StatusMessageType.Error);
                HoloNETManager.Instance.ShowStatusMessage($"Error occured saving entry (Shared): {result.Message}", StatusMessageType.Error);
                HoloNETManager.Instance.LogMessage($"APP: Error occured saving entry (Shared): {result.Message}");
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
    }
}
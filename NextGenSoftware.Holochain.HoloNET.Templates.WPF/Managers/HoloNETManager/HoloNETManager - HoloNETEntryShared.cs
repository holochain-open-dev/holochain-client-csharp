using System;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Models;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF.Managers
{
    public partial class HoloNETManager
    {
        public void InitHoloNETEntryShared(HoloNETClientAppAgent client)
        {
            LogMessage("APP: Initializing HoloNET Entry (Shared Connection)...");
            ShowStatusMessage("Initializing HoloNET Entry (Shared Connection)...", StatusMessageType.Information, true);

            if (!HoloNETEntryShared.ContainsKey(CurrentApp.Name) || (HoloNETEntryShared.ContainsKey(CurrentApp.Name) && HoloNETEntryShared[CurrentApp.Name] == null))
            {
                HoloNETEntryShared[CurrentApp.Name] = new AvatarShared(client);

                //If we are using SaveAsync (or LoadAsync) we do not need to worry about any events such as OnSaved if you don't need them.
                HoloNETEntryShared[CurrentApp.Name].OnInitialized += _holoNETEntryShared_OnInitialized;
                HoloNETEntryShared[CurrentApp.Name].OnLoaded += _holoNETEntryShared_OnLoaded;
                HoloNETEntryShared[CurrentApp.Name].OnSaved += _holoNETEntryShared_OnSaved;
                HoloNETEntryShared[CurrentApp.Name].OnDeleted += _holoNETEntryShared_OnDeleted;
                HoloNETEntryShared[CurrentApp.Name].OnClosed += _holoNETEntryShared_OnClosed;
                HoloNETEntryShared[CurrentApp.Name].OnError += _holoNETEntryShared_OnError;
            }
            else
                HoloNETEntryShared[CurrentApp.Name].HoloNETClient = client;
        }

        public async Task<ZomeFunctionCallBackEventArgs> LoadHoloNETEntrySharedAsync(HoloNETClientAppAgent client)
        {
            ZomeFunctionCallBackEventArgs result = new ZomeFunctionCallBackEventArgs();
            CheckIfHoloNETEntrySharedInitOK(client);

            if (HoloNETEntryShared.ContainsKey(CurrentApp.Name) && HoloNETEntryShared[CurrentApp.Name] != null)
            {
                if (!string.IsNullOrEmpty(HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarSharedEntryHash))
                {
                    ShowStatusMessage($"APP: Loading HoloNET Entry (Shared Connection)...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                    LogMessage($"Loading HoloNET Entry (Shared Connection)...");

                    // Non-async way (you need to use the OnLoaded event to know when the entry has loaded.
                    // For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...
                    //HoloNETEntryShared[CurrentApp.Name].Load(); 

                    // Async way
                    // LoadAsync (as well as all other async methods) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                    result = await HoloNETEntryShared[CurrentApp.Name].LoadAsync(HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarSharedEntryHash); //No event handlers are needed but you can still use if you like.
                }
            }
            else
            {
                result.IsError = true;
                result.Message = "HoloNET Entry (Shared Connection) Failed To Initialize";
            }

            return result;
        }

        public async Task<ZomeFunctionCallBackEventArgs> SaveHoloNETEntrySharedAsync(HoloNETClientAppAgent client, string firstName, string lastName, DateTime dob, string email)
        {
            ZomeFunctionCallBackEventArgs result = new ZomeFunctionCallBackEventArgs();
            CheckIfHoloNETEntrySharedInitOK(client);

            if (HoloNETEntryShared != null && HoloNETEntryShared[CurrentApp.Name].IsInitialized)
            {
                HoloNETEntryShared[CurrentApp.Name].FirstName = firstName;
                HoloNETEntryShared[CurrentApp.Name].LastName = lastName;
                HoloNETEntryShared[CurrentApp.Name].DOB = dob;
                HoloNETEntryShared[CurrentApp.Name].Email = email;

                ShowStatusMessage($"Saving HoloNET Data Entry (Shared Connection)...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: Saving HoloNET Data Entry (Shared Connection)...");

                // Non async way.
                // If you use Load or Save non-async versions you will need to wait for the OnInitialized event to fire before calling.
                //_holoNETEntry.Save(); //For this OnSaved event handler above is required (only if you want to see what the result was!).

                // Async way.
                // SaveAsync (as well as LoadAsync) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                result = await HoloNETEntryShared[CurrentApp.Name].SaveAsync(); //No event handlers are needed.

                if (!result.IsError)
                {
                    // Can use either of the lines below to get the EntryHash for the new entry.
                    HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarSharedEntryHash = result.Entries[0].EntryHash;
                    //HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarSharedEntryHash = HoloNETEntryShared[CurrentApp.Name].EntryHash;
                    HoloNETEntryDNAManager.SaveDNA();
                }
            }

            return result;
        }

        private void CheckIfHoloNETEntrySharedInitOK(HoloNETClientAppAgent client)
        {
            ShowStatusMessage($"Checking If HoloNET Entry (Shared Connection) Already Initialized...", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
            LogMessage($"APP: Checking If HoloNET Entry (Shared Connection) Already Initialized...");

            // In case it has not been init properly when the popup was opened (it should have been though!)
            if (HoloNETEntryShared == null ||
                (HoloNETEntryShared != null && !HoloNETEntryShared.ContainsKey(CurrentApp.Name)) ||
                (HoloNETEntryShared != null && HoloNETEntryShared.ContainsKey(CurrentApp.Name) && !HoloNETEntryShared[CurrentApp.Name].IsInitialized))
                InitHoloNETCollection(client);

            // Otherwise if the Endpoint does not match then set the correct internal HoloNETClient now.
            else if (HoloNETEntryShared != null
                && HoloNETEntryShared.ContainsKey(CurrentApp.Name)
                && HoloNETEntryShared[CurrentApp.Name] != null
                && HoloNETEntryShared[CurrentApp.Name].HoloNETClient != null
                && HoloNETEntryShared[CurrentApp.Name].HoloNETClient.EndPoint != client.EndPoint)
                    HoloNETEntryShared[CurrentApp.Name].HoloNETClient = client;

            else
            {
                ShowStatusMessage($"HoloNET Entry (Shared Connection) Already Initialized.", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: HoloNET Entry (Shared Connection) Already Initialized..");
            }
        }

        private void _holoNETEntryShared_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry (Shared) Initialized", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
            LogMessage($"APP: HoloNET Data Entry (Shared) Initialized.");
        }

        private void _holoNETEntryShared_OnLoaded(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry (Shared) Loaded", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
            LogMessage($"APP: HoloNET Data Entry (Shared) Loaded: {GetEntryInfo(e)}");
        }

        private void _holoNETEntryShared_OnSaved(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry (Shared) Saved", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
            LogMessage($"APP: HoloNET Data Entry (Shared) Saved: {GetEntryInfo(e)}");
        }

        private void _holoNETEntryShared_OnDeleted(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry (Shared) Deleted", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
            LogMessage($"APP: HoloNET Data Entry (Shared) Deleted: {GetEntryInfo(e)}");
        }

        private void _holoNETEntryShared_OnClosed(object sender, HoloNETShutdownEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry (Shared) Closed", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

            if (e.HolochainConductorsShutdownEventArgs != null)
                LogMessage($"APP: HoloNET Data Entry (Shared) Closed: Number Of Holochain Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown}, Number Of Hc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, Number Of Rustc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}");
            else
                LogMessage($"APP: HoloNET Data Entry (Shared) Closed: Number Of Holochain Exe Instances Shutdown: 0, Number Of Hc Exe Instances Shutdown: 0, Number Of Rustc Exe Instances Shutdown: 0");
        }

        private void _holoNETEntryShared_OnError(object sender, HoloNETErrorEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry (Shared) Error", StatusMessageType.Error, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
            LogMessage($"APP: HoloNET Data Entry (Shared) Error: {e.Reason}");
        }
    }
}
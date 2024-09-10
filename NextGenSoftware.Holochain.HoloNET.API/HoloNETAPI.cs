using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using System.Text.Json;

namespace NextGenSoftware.Holochain.HoloNET.API
{
    public static class HoloNETAPI
    {
        private const string HOLONET_API_HAPP_ID = "holonet_api";
        private const string HOLONET_API_APP_PATH = "holonet_api.happ";
        private const string HOLONET_API_HAPP_ROLE = "holonet_api";

        public static IHoloNETClientAdmin HoloNETClientAdmin { get; private set; } = new HoloNETClientAdmin();
        public static IHoloNETClientAppAgent HoloNETClient { get; private set; }

        public static bool IsInitialized { get; private set; }

        public static async Task<bool> InitHoloNETAsync()
        {
            return await InitHoloNETAsync(HOLONET_API_HAPP_ID, HOLONET_API_APP_PATH, HOLONET_API_HAPP_ROLE);
        }

        public static async Task<bool> InitHoloNETAsync(string hAppId, string hAppPath, string hAppRole)
        {
            bool adminConnected = false;
            string errorMessage = "Error Occured In HoloNETAPI.InitHoloNETAsync method. Reason: ";
            bool holoNETReady = false;

            try
            {
                HoloNETClientAdmin.OnError += HoloNETClientAdmin_OnError;

                if (HoloNETClientAdmin.State == System.Net.WebSockets.WebSocketState.Open)
                    adminConnected = true;

                else if (!HoloNETClientAdmin.IsConnecting)
                {
                    HoloNETConnectedEventArgs adminConnectResult = await HoloNETClientAdmin.ConnectAsync();

                    if (adminConnectResult != null && !adminConnectResult.IsError && adminConnectResult.IsConnected)
                        adminConnected = true;
                    else
                        HandleError($"{errorMessage}Error Occured Connecting To HoloNETClientAdmin EndPoint {HoloNETClientAdmin.EndPoint.AbsoluteUri}. Reason: {adminConnectResult.Message}");
                }

                if (adminConnected)
                {
                    if (HoloNETClient == null)
                    {
                        InstallEnableSignAttachAndConnectToHappEventArgs installedAppResult = await HoloNETClientAdmin.InstallEnableSignAttachAndConnectToHappAsync(hAppId, hAppPath, hAppRole);

                        if (installedAppResult != null && installedAppResult.IsSuccess && !installedAppResult.IsError)
                        {
                            HoloNETClient = installedAppResult.HoloNETClientAppAgent;
                            holoNETReady = true;
                        }
                        else
                            HandleError($"{errorMessage}Error Occured Calling InstallEnableSignAttachAndConnectToHappAsync On HoloNETClientAppAgent EndPoint {HoloNETClientAdmin.EndPoint.AbsoluteUri}. Reason: {installedAppResult.Message}");
                    }
                    else if (HoloNETClient.State != System.Net.WebSockets.WebSocketState.Open)
                    {
                        HoloNETConnectedEventArgs connectedResult = await HoloNETClient.ConnectAsync();

                        if (connectedResult != null && !connectedResult.IsError && connectedResult.IsConnected)
                            holoNETReady = true;
                        else
                            HandleError($"{errorMessage}Error Occured Connecting To HoloNETClientAppAgent EndPoint {HoloNETClient.EndPoint.AbsoluteUri}. Reason: {connectedResult.Message}");
                    }
                }

                if (HoloNETClient != null)
                    HoloNETClient.OnError += HoloNETClient_OnError;
            }
            catch (Exception e)
            {
                HandleError($"{errorMessage}{e}");
            }

            IsInitialized = holoNETReady;
            return holoNETReady;
        }

        public static async Task<string> LoadDataAsync(string key, string zome = "holonet_api", string function = "load_data")
        {
            string result = "";

            if (!IsInitialized)
                await InitHoloNETAsync();

            ZomeFunctionCallBackEventArgs callZomeResult =  await HoloNETClient.CallZomeFunctionAsync(zome, function, key);

            if (callZomeResult != null && !callZomeResult.IsError && callZomeResult.ZomeReturnData != null && callZomeResult.ZomeReturnData.ContainsKey("result") && callZomeResult.ZomeReturnData["result"] != null)
                result = callZomeResult.ZomeReturnData["result"].ToString();
            else
                HandleError($"Error Occured In HoloNETAPI.LoadDataAsync. Reason: {callZomeResult.Message}");

            return result;
        }

        public static async Task<string> LoadDataAsync(string hAppId, string hAppPath, string hAppRole, string key, string zome = "holonet_api", string function = "load_data")
        {
            if (!IsInitialized)
                await InitHoloNETAsync(hAppId, hAppPath, hAppRole);

            return await LoadDataAsync(key, zome, function);
        }

        public static async Task<T> LoadObjectAsync<T>(string key, string zome = "holonet_api", string function = "load_data")
        {
            dynamic result = "";

            if (!IsInitialized)
                await InitHoloNETAsync();

            ZomeFunctionCallBackEventArgs callZomeResult = await HoloNETClient.CallZomeFunctionAsync(zome, function, key);

            if (callZomeResult != null && !callZomeResult.IsError && callZomeResult.ZomeReturnData != null && callZomeResult.ZomeReturnData.ContainsKey("result") && callZomeResult.ZomeReturnData["result"] != null)
            {
                string json = callZomeResult.ZomeReturnData["result"].ToString();
                result = JsonSerializer.Deserialize<T>(json);
            }
            else
                HandleError($"Error Occured In HoloNETAPI.LoadObjectAsync. Reason: {callZomeResult.Message}");

            return result;
        }

        public static async Task<T> LoadObjectAsync<T>(string hAppId, string hAppPath, string hAppRole, string key, string zome = "holonet_api", string function = "load_data")
        {
            if (!IsInitialized)
                await InitHoloNETAsync(hAppId, hAppPath, hAppRole);

            return await LoadObjectAsync<T>(key, zome, function);
        }

        public static async Task<bool> SaveDataAsync(string key, string value, string zome = "holonet_api", string function = "save_data")
        {
            bool result = false;

            if (!IsInitialized)
                await InitHoloNETAsync();

            ZomeFunctionCallBackEventArgs callZomeResult = await HoloNETClient.CallZomeFunctionAsync(zome, function, new { key, value });

            if (callZomeResult != null && !callZomeResult.IsError && callZomeResult.ZomeReturnData != null && callZomeResult.ZomeReturnData.ContainsKey("result") && callZomeResult.ZomeReturnData["result"] != null)
                result = true;
            else
                HandleError($"Error Occured In HoloNETAPI.SaveDataAsync. Reason: {callZomeResult.Message}");

            return result;
        }

        public static async Task<bool> SaveDataAsync(string hAppId, string hAppPath, string hAppRole, string key, string value, string zome = "holonet_api", string function = "load_data")
        {
            if (!IsInitialized)
                await InitHoloNETAsync(hAppId, hAppPath, hAppRole);

            return await SaveDataAsync(key, value, zome, function);
        }

        public static async Task<bool> SaveObjectAsync(string key, dynamic objectToSave, string zome = "holonet_api", string function = "save_data")
        {
            bool result = false;

            if (!IsInitialized)
                await InitHoloNETAsync();

            string json = JsonSerializer.Serialize(objectToSave);
            ZomeFunctionCallBackEventArgs callZomeResult = await HoloNETClient.CallZomeFunctionAsync(zome, function, new { key, json });

            if (callZomeResult != null && !callZomeResult.IsError && callZomeResult.ZomeReturnData != null && callZomeResult.ZomeReturnData.ContainsKey("result") && callZomeResult.ZomeReturnData["result"] != null)
                result = true;
            else
                HandleError($"Error Occured In HoloNETAPI.SaveDataAsync. Reason: {callZomeResult.Message}");

            return result;
        }

        public static async Task<bool> SaveObjectAsync(string hAppId, string hAppPath, string hAppRole, string key, dynamic objectToSave, string zome = "holonet_api", string function = "load_data")
        {
            if (!IsInitialized)
                await InitHoloNETAsync(hAppId, hAppPath, hAppRole);

            return await SaveObjectAsync(key, objectToSave, zome, function);
        }

        public static bool SendSignal()
        {

            return true;
        }

        public static string ReceiveSignal()
        {

            return "";
        }

        private static void HoloNETClient_OnError(object sender, HoloNETErrorEventArgs e)
        {
            HandleError($"Error Occured In HoloNETAPI.HoloNETClient_OnError Event Handler: {e.Reason}");
        }

        private static void HoloNETClientAdmin_OnError(object sender, HoloNETErrorEventArgs e)
        {
            HandleError($"Error Occured In HoloNETAPI.HoloNETClientAdmin_OnError Event Handler: {e.Reason}");
        }

        private static void HandleError(string errorMessage)
        {
            //TODO: Add error handling/logging here.
        }
    }
}

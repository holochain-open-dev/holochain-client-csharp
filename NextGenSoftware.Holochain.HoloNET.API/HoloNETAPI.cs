using System.Text.Json;
using NextGenSoftware.Holochain.HoloNET.API.Interfaces;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;

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
        public static Dictionary<string, string> Data = new Dictionary<string, string>();

        public static IHoloNETAPIDNA HoloNETAPIDNA
        {
            get
            {
                return HoloNETAPIDNAManager.HoloNETAPIDNA;
            }
        }

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
                HoloNETAPIDNAManager.LoadDNA();
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

        public static async Task<HoloNETAPIResult<string>> LoadDataAsync(string key, bool useCache = true, string zome = "holonet_api", string function = "load_data")
        {
            HoloNETAPIResult<string> result = new HoloNETAPIResult<string>();

            if (!IsInitialized)
                await InitHoloNETAsync();

            if (useCache && Data != null && Data.ContainsKey(key))
            {
                result.Result = Data[key];
                result.IsSuccess = true;
            }
            else
            {
                result.ZomeCallResult = await HoloNETClient.CallZomeFunctionAsync(zome, function, key);

                if (result.ZomeCallResult != null && !result.ZomeCallResult.IsError && result.ZomeCallResult.ZomeReturnData != null && result.ZomeCallResult.ZomeReturnData.ContainsKey("result") && result.ZomeCallResult.ZomeReturnData["result"] != null)
                {
                    Data = JsonSerializer.Deserialize<Dictionary<string, string>>(result.ZomeCallResult.ZomeReturnData["result"].ToString());
                    result.Result = Data[key];
                    //result.Result = result.ZomeCallResult.ZomeReturnData["result"].ToString();
                    result.IsSuccess = true;
                }
                else
                    HandleError($"Error Occured In HoloNETAPI.LoadDataAsync. Reason: {result.ZomeCallResult.Message}");
            }

            return SetHoloNETAPIResult(result);
        }

        public static async Task<HoloNETAPIResult<string>> LoadDataAsync(string hAppId, string hAppPath, string hAppRole, string key, bool useCache = true, string zome = "holonet_api", string function = "load_data")
        {
            if (!IsInitialized)
                await InitHoloNETAsync(hAppId, hAppPath, hAppRole);

            return await LoadDataAsync(key, useCache, zome, function);
        }

        public static async Task<HoloNETAPIResult<T>> LoadObjectAsync<T>(string key, bool useCache = true, string zome = "holonet_api", string function = "get_latest_data") //where T : new()
        {
            HoloNETAPIResult<T> result = new HoloNETAPIResult<T>();

            if (!IsInitialized)
                await InitHoloNETAsync();

            result.ZomeCallResult = await HoloNETClient.CallZomeFunctionAsync(zome, function, key);

            if (result.ZomeCallResult != null && !result.ZomeCallResult.IsError && result.ZomeCallResult.ZomeReturnData != null && result.ZomeCallResult.ZomeReturnData.ContainsKey("result") && result.ZomeCallResult.ZomeReturnData["result"] != null)
            {
                string json = result.ZomeCallResult.ZomeReturnData["result"].ToString();
                result.Result = JsonSerializer.Deserialize<T>(json);
            }
            else
                HandleError($"Error Occured In HoloNETAPI.LoadObjectAsync. Reason: {result.ZomeCallResult.Message}");

            return SetHoloNETAPIResult(result);
        }

        public static async Task<HoloNETAPIResult<T>> LoadObjectAsync<T>(string hAppId, string hAppPath, string hAppRole, string key, bool useCache = true, string zome = "holonet_api", string function = "get_latest_data")
        {
            if (!IsInitialized)
                await InitHoloNETAsync(hAppId, hAppPath, hAppRole);

            return await LoadObjectAsync<T>(key, useCache, zome, function);
        }


        public static async Task<HoloNETAPIResult<bool>> SaveDataAsync(string zome = "holonet_api", string createFunction = "create_data", string updateFunction = "update_data")
        {
            HoloNETAPIResult<bool> result = new HoloNETAPIResult<bool>();
            string zomeFunction = "";

            if (!IsInitialized)
                await InitHoloNETAsync();

            if (string.IsNullOrEmpty(HoloNETAPIDNA.EntryHash))
                zomeFunction = createFunction;
            else
                zomeFunction = updateFunction;

            result.ZomeCallResult = await HoloNETClient.CallZomeFunctionAsync(zome, zomeFunction, JsonSerializer.Serialize(Data));

            if (result.ZomeCallResult != null && !result.ZomeCallResult.IsError && result.ZomeCallResult.ZomeReturnData != null && result.ZomeCallResult.ZomeReturnData.ContainsKey("result") && result.ZomeCallResult.ZomeReturnData["result"] != null)
                result.Result = true;
            else
                HandleError($"Error Occured In HoloNETAPI.SaveDataAsync. Reason: {result.ZomeCallResult.Message}");

            return SetHoloNETAPIResult(result);
        }

        public static async Task<HoloNETAPIResult<bool>> SaveDataAsync(string hAppId, string hAppPath, string hAppRole, string zome = "holonet_api", string createFunction = "create_data", string updateFunction = "update_data")
        {
            if (!IsInitialized)
                await InitHoloNETAsync(hAppId, hAppPath, hAppRole);

            return await SaveDataAsync(zome, createFunction, updateFunction);
        }

        //public static async Task<HoloNETAPIResult<bool>> SaveDataAsync(string key, string value, string zome = "holonet_api", string function = "save_data")
        public static async Task<HoloNETAPIResult<bool>> SaveKeyValuePairAsync(string key, string value, string zome = "holonet_api", string createFunction = "create_data", string updateFunction = "update_data")
        {
            Data[key] = value;
            return await SaveDataAsync(zome, createFunction, updateFunction);
        }

        public static async Task<HoloNETAPIResult<bool>> SaveKeyValuePairAsync(string hAppId, string hAppPath, string hAppRole, string key, string value, string zome = "holonet_api", string createFunction = "create_data", string updateFunction = "update_data")
        {
            Data[key] = value;
            return await SaveDataAsync(hAppId, hAppPath, hAppRole, zome, createFunction, updateFunction);
        }

        public static async Task<HoloNETAPIResult<bool>> SaveObjectAsync(string key, dynamic objectToSave, string zome = "holonet_api", string createFunction = "create_object_data", string updateFunction = "update_object_data")
        {
            HoloNETAPIResult<bool> result = new HoloNETAPIResult<bool>();
            string zomeFunction = "";

            if (!IsInitialized)
                await InitHoloNETAsync();

            if (string.IsNullOrEmpty(HoloNETAPIDNA.EntryHash))
                zomeFunction = createFunction;
            else
                zomeFunction = updateFunction;

            string json = JsonSerializer.Serialize(objectToSave);
            result.ZomeCallResult = await HoloNETClient.CallZomeFunctionAsync(zome, zomeFunction, new { key, json });

            if (result.Result != null && !result.ZomeCallResult.IsError && result.ZomeCallResult.ZomeReturnData != null && result.ZomeCallResult.ZomeReturnData.ContainsKey("result") && result.ZomeCallResult.ZomeReturnData["result"] != null)
                result.Result = true;
            else
                HandleError($"Error Occured In HoloNETAPI.SaveObjectAsync. Reason: {result.ZomeCallResult.Message}");

            return SetHoloNETAPIResult(result);
        }

        public static async Task<HoloNETAPIResult<bool>> SaveObjectAsync(string hAppId, string hAppPath, string hAppRole, string key, dynamic objectToSave, string zome = "holonet_api", string createFunction = "create_object_data", string updateFunction = "update_object_data")
        {
            if (!IsInitialized)
                await InitHoloNETAsync(hAppId, hAppPath, hAppRole);

            return await SaveObjectAsync(key, objectToSave, zome, createFunction, updateFunction);
        }

        public static bool SendSignal()
        {

            return true;
        }

        public static string ReceiveSignal()
        {

            return "";
        }

        private static HoloNETAPIResult<T> SetHoloNETAPIResult<T>(HoloNETAPIResult<T> result)
        {
            result.IsSuccess = !result.ZomeCallResult.IsError;
            result.Message = result.ZomeCallResult.Message;
            return result;
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

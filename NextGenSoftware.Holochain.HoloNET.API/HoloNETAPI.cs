using System.Text.Json;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.API.Interfaces;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.API
{
    public static class HoloNETAPI
    {
        //private const string HOLONET_API_HAPP_ID = "holonet_api";
        //private const string HOLONET_API_APP_PATH = "holonet_api.happ";
        //private const string HOLONET_API_HAPP_ROLE = "holonet_api_role";
        //private const string HOLONET_API_ZOME_NAME = "holonet_api";
        //private const string HOLONET_API_LOAD_DATA_FUNCTION = "get_latest_data";
        //private const string HOLONET_API_CREATE_DATA_FUNCTION = "create_data";
        //private const string HOLONET_API_UPDATE_DATA_FUNCTION = "update_data";
        //private const string HOLONET_API_CREATE_OBJECT_FUNCTION = "create_data"; //"create_object"; //TODO: Will update happ so object and data are stored in seperate entries rather than one.
        //private const string HOLONET_API_UPDATE_OBJECT_FUNCTION = "update_data"; //"update_object";
        //private const string HOLONET_API_JSON_DATA_FIELD = "dataJson";
        //private const string HOLONET_API_JSON_OBJECT_FIELD = "objectsJson";

        private const string HOLONET_API_HAPP_ID = "oasis";
        private const string HOLONET_API_APP_PATH = "oasis.happ";
        private const string HOLONET_API_HAPP_ROLE = "oasis";
        private const string HOLONET_API_ZOME_NAME = "oasis";
        private const string HOLONET_API_LOAD_DATA_FUNCTION = "get_avatar";
        private const string HOLONET_API_CREATE_DATA_FUNCTION = "create_avatar";
        private const string HOLONET_API_UPDATE_DATA_FUNCTION = "update_avatar";
        private const string HOLONET_API_CREATE_OBJECT_FUNCTION = "create_data"; //"create_object"; //TODO: Will update happ so object and data are stored in seperate entries rather than one.
        private const string HOLONET_API_UPDATE_OBJECT_FUNCTION = "update_data"; //"update_object";
        //private const string HOLONET_API_JSON_DATA_FIELD = "meta_data";
        private const string HOLONET_API_JSON_DATA_FIELD = "first_name";
        //private const string HOLONET_API_JSON_OBJECT_FIELD = "meta_data";
        private const string HOLONET_API_JSON_OBJECT_FIELD = "last_name";

        public static IHoloNETClientAdmin HoloNETClientAdmin { get; private set; } = new HoloNETClientAdmin();
        public static IHoloNETClientAppAgent HoloNETClient { get; private set; }
        public static bool IsInitialized { get; private set; }
        public static Dictionary<string, string> Data = new Dictionary<string, string>();
        public static Dictionary<string, object> Objects = new Dictionary<string, object>();

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

                HoloNETClientAdmin.HoloNETDNA.ShowHolochainConductorWindow = true;

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

        public static async Task<HoloNETAPIResult<Dictionary<string, string>>> LoadAllDataAsync(string zome = HOLONET_API_ZOME_NAME, string function = HOLONET_API_LOAD_DATA_FUNCTION)
        {
            HoloNETAPIResult<Dictionary<string, string>> result = new HoloNETAPIResult<Dictionary<string, string>>();

            if (!IsInitialized)
                await InitHoloNETAsync();

            result.ZomeCallResult = await HoloNETClient.CallZomeFunctionAsync(zome, function, null);

            if (result.ZomeCallResult != null && !result.ZomeCallResult.IsError && result.ZomeCallResult.ZomeReturnData != null && result.ZomeCallResult.ZomeReturnData.ContainsKey(HOLONET_API_JSON_DATA_FIELD) && result.ZomeCallResult.ZomeReturnData[HOLONET_API_JSON_DATA_FIELD] != null)
            {
                Data = JsonSerializer.Deserialize<Dictionary<string, string>>(result.ZomeCallResult.ZomeReturnData[HOLONET_API_JSON_DATA_FIELD].ToString());
                result.Result = Data;
                result.IsSuccess = true;
            }
            else
                HandleError($"Error Occured In HoloNETAPI.LoadAllDataAsync. Reason: {result.ZomeCallResult.Message}");
            
            return SetHoloNETAPIResult(result);
        }

        public static async Task<HoloNETAPIResult<Dictionary<string, string>>> LoadAllDataAsync(string hAppId, string hAppPath, string hAppRole, string key, bool useCache = true, string zome = "holonet_api", string function = HOLONET_API_LOAD_DATA_FUNCTION)
        {
            if (!IsInitialized)
                await InitHoloNETAsync(hAppId, hAppPath, hAppRole);

            return await LoadAllDataAsync(zome, function);
        }

        public static async Task<HoloNETAPIResult<Dictionary<string, object>>> LoadAllObjectsAsync(string zome = HOLONET_API_ZOME_NAME, string function = HOLONET_API_LOAD_DATA_FUNCTION)
        {
            HoloNETAPIResult<Dictionary<string, object>> result = new HoloNETAPIResult<Dictionary<string, object>>();

            if (!IsInitialized)
                await InitHoloNETAsync();

            result.ZomeCallResult = await HoloNETClient.CallZomeFunctionAsync(zome, function, null);

            if (result.ZomeCallResult != null && !result.ZomeCallResult.IsError && result.ZomeCallResult.ZomeReturnData != null && result.ZomeCallResult.ZomeReturnData.ContainsKey(HOLONET_API_JSON_OBJECT_FIELD) && result.ZomeCallResult.ZomeReturnData[HOLONET_API_JSON_OBJECT_FIELD] != null)
            {
                Objects = JsonSerializer.Deserialize<Dictionary<string, object>>(result.ZomeCallResult.ZomeReturnData[HOLONET_API_JSON_OBJECT_FIELD].ToString());
                result.Result = Objects;
                result.IsSuccess = true;
            }
            else
                HandleError($"Error Occured In HoloNETAPI.LoadAllObjectsAsync. Reason: {result.ZomeCallResult.Message}");

            return SetHoloNETAPIResult(result);
        }

        public static async Task<HoloNETAPIResult<Dictionary<string, object>>> LoadAllObjectsAsync(string hAppId, string hAppPath, string hAppRole, string key, bool useCache = true, string zome = "holonet_api", string function = HOLONET_API_LOAD_DATA_FUNCTION)
        {
            if (!IsInitialized)
                await InitHoloNETAsync(hAppId, hAppPath, hAppRole);

            return await LoadAllObjectsAsync(zome, function);
        }

        public static async Task<HoloNETAPIResult<string>> LoadDataAsync(string key, bool useCache = true, string zome = HOLONET_API_ZOME_NAME, string function = HOLONET_API_LOAD_DATA_FUNCTION)
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
                HoloNETAPIResult<Dictionary<string, string>> allDataResult =  await LoadAllDataAsync();

                if (allDataResult != null && allDataResult.IsSuccess)
                {
                    result.Result = allDataResult.Result[key];
                    result.IsSuccess = true;
                }
                else
                    HandleError($"Error Occured In HoloNETAPI.LoadDataAsync. Reason: {allDataResult.Message}");

                result.IsSuccess = allDataResult.IsSuccess;
                result.Message = allDataResult.Message;
            }

            return result;
        }

        public static async Task<HoloNETAPIResult<string>> LoadDataAsync(string hAppId, string hAppPath, string hAppRole, string key, bool useCache = true, string zome = HOLONET_API_ZOME_NAME, string function = HOLONET_API_LOAD_DATA_FUNCTION)
        {
            if (!IsInitialized)
                await InitHoloNETAsync(hAppId, hAppPath, hAppRole);

            return await LoadDataAsync(key, useCache, zome, function);
        }

        public static async Task<HoloNETAPIResult<T>> LoadObjectAsync<T>(string key, bool useCache = true, string zome = HOLONET_API_ZOME_NAME, string function = HOLONET_API_LOAD_DATA_FUNCTION) //where T : new()
        {
            HoloNETAPIResult<T> result = new HoloNETAPIResult<T>();

            if (!IsInitialized)
                await InitHoloNETAsync();

            if (useCache && Objects != null && Objects.ContainsKey(key))
            {
                result.Result = (T)Objects[key];
                result.IsSuccess = true;
            }
            else
            {
                HoloNETAPIResult<Dictionary<string, object>> allObjectsResult = await LoadAllObjectsAsync();

                if (allObjectsResult != null && allObjectsResult.IsSuccess)
                {
                    result.Result = (T)allObjectsResult.Result[key];
                    result.IsSuccess = true;
                }
                else
                    HandleError($"Error Occured In HoloNETAPI.LoadObjectAsync. Reason: {allObjectsResult.Message}");

                result.IsSuccess = allObjectsResult.IsSuccess;
                result.Message = allObjectsResult.Message;
            }

            return result;
        }

        public static async Task<HoloNETAPIResult<T>> LoadObjectAsync<T>(string hAppId, string hAppPath, string hAppRole, string key, bool useCache = true, string zome = HOLONET_API_ZOME_NAME, string function = HOLONET_API_LOAD_DATA_FUNCTION)
        {
            if (!IsInitialized)
                await InitHoloNETAsync(hAppId, hAppPath, hAppRole);

            return await LoadObjectAsync<T>(key, useCache, zome, function);
        }

        public static async Task<HoloNETAPIResult<bool>> SaveAllDataAsync(string zome = HOLONET_API_ZOME_NAME, string createFunction = HOLONET_API_CREATE_DATA_FUNCTION, string updateFunction = HOLONET_API_UPDATE_DATA_FUNCTION)
        {
            HoloNETAPIResult<bool> result = new HoloNETAPIResult<bool>();
            string zomeFunction = "";

            if (!IsInitialized)
                await InitHoloNETAsync();

            if (string.IsNullOrEmpty(HoloNETAPIDNA.EntryHash))
                zomeFunction = createFunction;
            else
                zomeFunction = updateFunction;

            //TODO: Will generate a new happ soon where Data and Objects are in seperate entries rather than in the same one as different props.
            //result.ZomeCallResult = await HoloNETClient.CallZomeFunctionAsync(zome, zomeFunction, new { dataJson = JsonSerializer.Serialize(Data), objectsJson = JsonSerializer.Serialize(Objects) });
            result.ZomeCallResult = await HoloNETClient.CallZomeFunctionAsync(zome, zomeFunction, new 
            { 
                first_name = JsonSerializer.Serialize(Data), 
                last_name = JsonSerializer.Serialize(Objects),
                email = "",
                dob = ""
            });

            if (result.ZomeCallResult != null && !result.ZomeCallResult.IsError && result.ZomeCallResult.ZomeReturnData != null && result.ZomeCallResult.ZomeReturnData.ContainsKey("result") && result.ZomeCallResult.ZomeReturnData["result"] != null)
                result.Result = true;
            else
                HandleError($"Error Occured In HoloNETAPI.SaveDataAsync. Reason: {result.ZomeCallResult.Message}");

            return SetHoloNETAPIResult(result);
        }

        public static async Task<HoloNETAPIResult<bool>> SaveAllDataAsync(string hAppId, string hAppPath, string hAppRole, string zome = HOLONET_API_ZOME_NAME, string createFunction = HOLONET_API_CREATE_DATA_FUNCTION, string updateFunction = HOLONET_API_UPDATE_DATA_FUNCTION)
        {
            if (!IsInitialized)
                await InitHoloNETAsync(hAppId, hAppPath, hAppRole);

            return await SaveAllDataAsync(zome, createFunction, updateFunction);
        }

        public static async Task<HoloNETAPIResult<bool>> SaveDataAsync(string key, string value, string zome = HOLONET_API_ZOME_NAME, string createFunction = HOLONET_API_CREATE_DATA_FUNCTION, string updateFunction = HOLONET_API_UPDATE_DATA_FUNCTION)
        {
            //TODO: In future we can make this ONLY save this keyvalue pair instead of saving the full dictionary!
            Data[key] = value;
            return await SaveAllDataAsync(zome, createFunction, updateFunction);
        }

        public static async Task<HoloNETAPIResult<bool>> SaveDataAsync(string hAppId, string hAppPath, string hAppRole, string key, string value, string zome = HOLONET_API_ZOME_NAME, string createFunction = HOLONET_API_CREATE_DATA_FUNCTION, string updateFunction = HOLONET_API_UPDATE_DATA_FUNCTION)
        {
            //TODO: In future we can make this ONLY save this keyvalue pair instead of saving the full dictionary!
            Data[key] = value;
            return await SaveAllDataAsync(hAppId, hAppPath, hAppRole, zome, createFunction, updateFunction);
        }

        public static async Task<HoloNETAPIResult<bool>> SaveAllObjectsAsync(string zome = HOLONET_API_ZOME_NAME, string createFunction = HOLONET_API_CREATE_OBJECT_FUNCTION, string updateFunction = HOLONET_API_UPDATE_OBJECT_FUNCTION)
        {
            HoloNETAPIResult<bool> result = new HoloNETAPIResult<bool>();
            string zomeFunction = "";

            if (!IsInitialized)
                await InitHoloNETAsync();

            if (string.IsNullOrEmpty(HoloNETAPIDNA.EntryHash))
                zomeFunction = createFunction;
            else
                zomeFunction = updateFunction;

            //TODO: Will generate a new happ soon where Data and Objects are in seperate entries rather than in the same one as different props.
            result.ZomeCallResult = await HoloNETClient.CallZomeFunctionAsync(zome, zomeFunction, new { dataJson = JsonSerializer.Serialize(Data), objectsJson = JsonSerializer.Serialize(Objects) });

            if (result.Result != null && !result.ZomeCallResult.IsError && result.ZomeCallResult.ZomeReturnData != null && result.ZomeCallResult.ZomeReturnData.ContainsKey("result") && result.ZomeCallResult.ZomeReturnData["result"] != null)
                result.Result = true;
            else
                HandleError($"Error Occured In HoloNETAPI.SaveObjectAsync. Reason: {result.ZomeCallResult.Message}");

            return SetHoloNETAPIResult(result);
        }

        public static async Task<HoloNETAPIResult<bool>> SaveAllObjectsAsync(string hAppId, string hAppPath, string hAppRole, string zome = HOLONET_API_ZOME_NAME, string createFunction = HOLONET_API_CREATE_OBJECT_FUNCTION, string updateFunction = HOLONET_API_UPDATE_OBJECT_FUNCTION)
        {
            if (!IsInitialized)
                await InitHoloNETAsync(hAppId, hAppPath, hAppRole);

            return await SaveObjectAsync(zome, createFunction, updateFunction);
        }

        public static async Task<HoloNETAPIResult<bool>> SaveObjectAsync(string key, dynamic objectToSave, string zome = HOLONET_API_ZOME_NAME, string createFunction = HOLONET_API_CREATE_OBJECT_FUNCTION, string updateFunction = HOLONET_API_UPDATE_OBJECT_FUNCTION)
        {
            //TODO: In future we can make this ONLY save this keyvalue pair instead of saving the full dictionary!
            Objects[key] = objectToSave;//JsonSerializer.Serialize(objectToSave);
            return await SaveAllObjectsAsync(zome, createFunction, updateFunction);
        }

        public static async Task<HoloNETAPIResult<bool>> SaveObjectAsync(string hAppId, string hAppPath, string hAppRole, string key, dynamic objectToSave, string zome = HOLONET_API_ZOME_NAME, string createFunction = HOLONET_API_CREATE_OBJECT_FUNCTION, string updateFunction = HOLONET_API_UPDATE_OBJECT_FUNCTION)
        {
            //TODO: In future we can make this ONLY save this keyvalue pair instead of saving the full dictionary!
            Objects[key] = objectToSave; //JsonSerializer.Serialize(objectToSave);
            return await SaveAllObjectsAsync(hAppId, hAppPath, hAppRole, zome, createFunction, updateFunction);
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
            if (result.ZomeCallResult != null)
            {
                result.IsSuccess = !result.ZomeCallResult.IsError;
                result.Message = result.ZomeCallResult.Message;
            }

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

using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using MessagePack;
using NextGenSoftware.Logging;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public abstract partial class HoloNETClientBase : IHoloNETClientBase
    {

        /// <summary>
        /// This method allows you to send your own raw request to holochain. This method raises the OnDataRecived event once it has received a response from the Holochain conductor.
        /// </summary>
        /// <param name="id">The id of the request to send to the Holochain Conductor. This will be matched to the id in the response received from the Holochain Conductor.</param>
        /// <param name="holoNETData">The raw data packet you wish to send to the Holochain conductor.</param>
        /// <returns></returns>
        public virtual async Task SendHoloNETRequestAsync(HoloNETData holoNETData, HoloNETRequestType requestType, string id = "")
        {
            await SendHoloNETRequestAsync(MessagePackSerializer.Serialize(holoNETData), requestType, id);
        }

        /// <summary>
        /// This method allows you to send your own raw request to holochain. This method raises the OnDataRecived event once it has received a response from the Holochain conductor.
        /// </summary>
        /// <param name="id">The id of the request to send to the Holochain Conductor. This will be matched to the id in the response received from the Holochain Conductor.</param>
        /// <param name="holoNETData">The raw data packet you wish to send to the Holochain conductor.</param>
        public virtual void SendHoloNETRequest(HoloNETData holoNETData, HoloNETRequestType requestType, string id = "")
        {
            FireAndForget(SendHoloNETRequestAsync(holoNETData, requestType, id), "SendHoloNETRequest");
        }

        /// <summary>
        /// This method allows you to send your own raw request to holochain. This method raises the OnDataRecived event once it has received a response from the Holochain conductor.
        /// </summary>
        /// <param name="id">The id of the request to send to the Holochain Conductor. This will be matched to the id in the response received from the Holochain Conductor.</param>
        /// <param name="holoNETData">The raw data packet you wish to send to the Holochain conductor.</param>
        public virtual async Task SendHoloNETRequestAsync(byte[] data, HoloNETRequestType requestType, string id = "")
        {
            await TrySendHoloNETRequestAsync(data, requestType, id);
        }

        /// <summary>
        /// Sends the request and reports whether it was actually sent. SendHoloNETRequestAsync swallowed every failure (including
        /// one in its own logging, which skipped the send), so a caller waiting for the reply waited forever.
        /// </summary>
        protected virtual async Task<bool> TrySendHoloNETRequestAsync(byte[] data, HoloNETRequestType requestType, string id = "")
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    id = GetRequestId();

                HoloNETRequest request = new HoloNETRequest()
                {
                    id = Convert.ToUInt64(id),
                    type = "request",
                    data = data
                };

                if (HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour != EnforceRequestToResponseIdMatchingBehaviour.Ignore)
                    _pendingRequests.Add(id);

                _requestTypeLookup[id] = requestType;

                if (WebSocket.State != WebSocketState.Open)
                    return false;

                try { Logger.Log("Sending HoloNET Request to Holochain Conductor...", LogType.Info, true); } catch { }
                //await WebSocket.SendRawDataAsync(MessagePackSerializer.Serialize(request));
                // SendRawDataAsync returned normally even when nothing was sent; TrySendRawDataAsync says so.
                if (!await WebSocket.TrySendRawDataAsync(MessagePackSerializer.Serialize(request)))
                    return false;
                try { Logger.Log("HoloNET Request Successfully Sent To Holochain Conductor.", LogType.Info, false); } catch { }
                return true;
            }
            catch (Exception ex)
            {
                ReportError("Error occurred in HoloNETClient.SendHoloNETRequest method.", ex);
                return false;
            }
        }

        // Previous body of SendHoloNETRequestAsync(byte[]), kept for reference:
//        private async Task SendHoloNETRequestAsyncOld(byte[] data, HoloNETRequestType requestType, string id = "")
//        {
//            try
//            {
//                if (string.IsNullOrEmpty(id))
//                    id = GetRequestId();

//                HoloNETRequest request = new HoloNETRequest()
//                {
//                    id = Convert.ToUInt64(id),
//                    type = "request",
//                    data = data
//                };

//                if (HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour != EnforceRequestToResponseIdMatchingBehaviour.Ignore)
//                    _pendingRequests.Add(id);

//                _requestTypeLookup[id] = requestType;

//                if (WebSocket.State == WebSocketState.Open)
//                {
//                    Logger.Log("Sending HoloNET Request to Holochain Conductor...", LogType.Info, true);
//                    await WebSocket.SendRawDataAsync(MessagePackSerializer.Serialize(request)); //This is the fastest and most popular .NET MessagePack Serializer.
//                    //await WebSocket.UnityWebSocket.Send(MessagePackSerializer.Serialize(request));
//                    Logger.Log("HoloNET Request Successfully Sent To Holochain Conductor.", LogType.Info, false);
//                }
//            }
//            catch (Exception ex)
//            {
//                HandleError("Error occurred in HoloNETClient.SendHoloNETRequest method.", ex);
//            }
//        }

        /// <summary>
        /// This method allows you to send your own raw request to holochain. This method raises the OnDataRecived event once it has received a response from the Holochain conductor.
        /// </summary>
        /// <param name="id">The id of the request to send to the Holochain Conductor. This will be matched to the id in the response received from the Holochain Conductor.</param>
        /// <param name="holoNETData">The raw data packet you wish to send to the Holochain conductor.</param>
        public virtual void SendHoloNETRequest(byte[] data, HoloNETRequestType requestType, string id = "")
        {
            FireAndForget(SendHoloNETRequestAsync(data, requestType, id), "SendHoloNETRequest");
        }

        protected virtual string GetRequestId()
        {
            // Was "_currentId++; return _currentId.ToString();": requests sent from two threads at once (e.g. one from a HoloNET
            // event while the app sends another) could get the same id, and the replies were then matched to the wrong call.
            //_currentId++;
            //return _currentId.ToString();
            return System.Threading.Interlocked.Increment(ref _currentId).ToString();
        }
    }
}
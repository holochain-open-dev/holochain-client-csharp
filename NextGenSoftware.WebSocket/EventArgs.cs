using System;
using System.Net.WebSockets;

namespace NextGenSoftware.WebSocket
{
    public class ConnectedEventArgs : EventArgs
    {
        public string EndPoint { get; set; }
    }

    public class DisconnectedEventArgs : EventArgs
    {
        public string EndPoint { get; set; }
        public string Reason { get; set; }
    }

    public class WebSocketErrorEventArgs : EventArgs
    {
        public string EndPoint { get; set; }
        public string Reason { get; set; }
        public Exception ErrorDetails { get; set; }
    }
    public class DataReceivedEventArgs : CallBackBaseEventArgsWithId
    {
        public DataReceivedEventArgs(string id, string endPoint, bool isCallSuccessful, byte[] rawBinaryData, string rawJSONData, WebSocketReceiveResult webSocketResult) : base(id, endPoint, isCallSuccessful, rawBinaryData, rawJSONData, webSocketResult)
        {
            
        }
    }
    
    public abstract class CallBackBaseEventArgs : EventArgs
    {
        public CallBackBaseEventArgs(string endPoint, bool isCallSuccessful, byte[] rawBinaryData, string rawJSONData, WebSocketReceiveResult webSocketResult)
        {
            EndPoint = endPoint;
            IsCallSuccessful = isCallSuccessful;
            RawJSONData = rawJSONData;
            RawBinaryData = rawBinaryData;
            WebSocketResult = webSocketResult;
        }

        public string EndPoint { get; private set; }
        public bool IsCallSuccessful { get; private set; }
        public string RawJSONData { get; private set; }
        public byte[] RawBinaryData { get; private set; }
        public WebSocketReceiveResult WebSocketResult { get; private set; }
    }

    public abstract class CallBackBaseEventArgsWithId : CallBackBaseEventArgs
    {
        public CallBackBaseEventArgsWithId(string id, string endPoint, bool isCallSuccessful, byte[] rawBinaryData, string rawJSONData, WebSocketReceiveResult webSocketResult) : base(endPoint, isCallSuccessful, rawBinaryData, rawJSONData, webSocketResult)
        {
            Id = id;
        }

        public string Id { get; private set; }
    }
}

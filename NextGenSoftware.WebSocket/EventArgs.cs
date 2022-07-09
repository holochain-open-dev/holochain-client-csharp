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
    public class DataReceivedEventArgs : EventArgs
    {
        public string EndPoint { get; set; }
        public string RawJSONData { get; set; }
        public byte[] RawBinaryData { get; set; }
        public WebSocketReceiveResult WebSocketResult { get; set; }
        //public bool IsConductorDebugInfo { get; set; }
    }
    
    public abstract class CallBackBaseEventArgs : EventArgs
    {
        public CallBackBaseEventArgs(string id, string endPoint, bool isCallSuccessful, byte[] rawBinaryData, string rawJSONData, WebSocketReceiveResult webSocketResult)
        {
            Id = id;
            EndPoint = endPoint;
            IsCallSuccessful = isCallSuccessful;
            RawJSONData = rawJSONData;
            RawBinaryData = rawBinaryData;
            WebSocketResult = webSocketResult;
        }

        public string Id { get; private set; }
        public string EndPoint { get; private set; }
        public bool IsCallSuccessful { get; private set; }
        public string RawJSONData { get; private set; }
        public byte[] RawBinaryData { get; private set; }
        public WebSocketReceiveResult WebSocketResult { get; private set; }
    }
}

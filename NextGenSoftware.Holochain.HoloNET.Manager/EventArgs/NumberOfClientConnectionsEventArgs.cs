using System;
namespace NextGenSoftware.Holochain.HoloNET.Manager
{
    public class NumberOfClientConnectionsEventArgs : EventArgs
    {
        public int NumberOfConnections { get; set; }
    }
}
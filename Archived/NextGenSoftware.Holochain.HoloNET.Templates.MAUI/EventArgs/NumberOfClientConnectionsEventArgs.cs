using System;
namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    public class NumberOfClientConnectionsEventArgs : EventArgs
    {
        public int NumberOfConnections { get; set; }
    }
}
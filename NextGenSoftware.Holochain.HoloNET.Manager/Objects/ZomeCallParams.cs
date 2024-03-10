namespace NextGenSoftware.Holochain.HoloNET.Manager.Objects
{
    public class ZomeCallParams
    {
        public string ZomeName { get; set; }
        public string ZomeFunction { get; set; }
        public dynamic ParamsObjectForZomeCall { get; set; }
    }
}
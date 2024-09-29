using NextGenSoftware.Holochain.HoloNET.Client;

namespace NextGenSoftware.Holochain.HoloNET.API
{
    public class HoloNETAPIResult<T>
    {
        public T Result { get; set; }
        public ZomeFunctionCallBackEventArgs ZomeCallResult { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
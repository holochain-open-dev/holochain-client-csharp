using NextGenSoftware.Holochain.HoloNET.ORM;
using NextGenSoftware.WebSocket;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class HoloNETCollectionSavedResult : CallBackBaseEventArgs
    {
        public List<HoloNETEntryBase> EntiesSaved { get; set; } = new List<HoloNETEntryBase>();
        public List<HoloNETEntryBase> EntiesAdded { get; set; } = new List<HoloNETEntryBase>();
        public List<HoloNETEntryBase> EntiesRemoved { get; set; } = new List<HoloNETEntryBase>();
        public List<HoloNETEntryBase> EntiesSaveErrors { get; set; } = new List<HoloNETEntryBase>();
        public List<ZomeFunctionCallBackEventArgs> ConductorResponses { get; set; } = new List<ZomeFunctionCallBackEventArgs>();
        public List<string> ErrorMessages = new List<string>();
    }

    public class HoloNETCollectionLoadedResult<T> : CallBackBaseEventArgs where T : HoloNETEntryBase
    {
        public List<T> EntriesLoaded {  get; set; }
        public ZomeFunctionCallBackEventArgs ZomeFunctionCallBackEventArgs { get; set; }
    }
}
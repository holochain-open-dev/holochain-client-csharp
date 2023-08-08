
using NextGenSoftware.Holochain.HoloNET.Client.Data;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public enum CellInfoType
    {
        Provisioned, //ProvisionedCell
        Cloned, //ClonedCell 
        Stem  //StemCell 
    }

    //public struct CellInfoType
    //{
    //    public ICell Cell { get ;set; }

    //    //public ProvisionedCell Provisioned { get; set; }
    //    //public ClonedCell Cloned { get; set; }
    //    //public StemCell Stem { get; set; }
    //}
}
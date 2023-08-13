
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    //public enum CellInfoType
    //{
    //    Provisioned, //ProvisionedCell
    //    Cloned, //ClonedCell 
    //    Stem  //StemCell 
    //}

    [MessagePackObject]
    public struct CellInfoType
    {
        //public ICell Cell { get; set; }

        //Dictionary<string, ICell> Cell { get;set; }

        [Key("Provisioned")]
        public ProvisionedCell Provisioned { get; set; }

        [Key("Cloned")]
        public ClonedCell Cloned { get; set; }

        [Key("Stem")]
        public StemCell Stem { get; set; }
    }
}
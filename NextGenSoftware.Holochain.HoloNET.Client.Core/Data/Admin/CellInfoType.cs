
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin
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

        [Key("provisioned")]
        public ProvisionedCell provisioned { get; set; }

        [Key("cloned")]
        public ClonedCell cloned { get; set; }

        [Key("stem")]
        public StemCell stem { get; set; }
    }
}
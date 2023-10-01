﻿
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests
{
    [MessagePackObject]
    public class HoloNETAdminListDnasRequest
    {
        [Key("status_filter")]
        public AppStatusFilter status_filter { get; set; }
    }
}
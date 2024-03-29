﻿using System;
using NextGenSoftware.Holochain.HoloNET.ORM.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF.Interfaces
{
    public interface IAvatarShared : IHoloNETAuditEntryBase
    {
        DateTime DOB { get; set; }
        string Email { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
    }
}
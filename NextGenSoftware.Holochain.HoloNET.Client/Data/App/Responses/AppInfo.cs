using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.AppManifest;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class AppInfo : IAppInfo
    {
        [Key("installed_app_id")]
        public string installed_app_id { get; set; }

        [Key("cell_info")]
        public Dictionary<string, List<CellInfo>> cell_info { get; set; }

        [Key("agent_pub_key")]
        public byte[] agent_pub_key { get; set; }

        [IgnoreMember]
        public string AgentPubKey { get; set; }

        [IgnoreMember]
        public string DnaHash { get; set; }

        [IgnoreMember]
        public byte[][] CellId { get; set; }

        [Key("status")]
        public Dictionary<string, object> status { get; set; }
        //public AppInfoStatus status { get; set; }
        //public object status { get; set; }

        [IgnoreMember]
        public AppInfoStatusEnum AppStatus
        {
            get
            {
                // Holochain 0.7 AppStatus is adjacently tagged: {"type": "enabled" | "disabled" | "awaiting_memproofs" |
                // "awaiting_restore" | "unrecoverable", "value": ...}. The keyed form below is the pre-0.7 shape, so on 0.7 this
                // always returned None. "enabled" maps to Running.
                if (status != null && status.TryGetValue("type", out object statusType) && statusType is string type)
                {
                    status.TryGetValue("value", out object statusValue);
                    switch (type)
                    {
                        case "enabled":
                            AppStatusReason = null;
                            return AppInfoStatusEnum.Running;

                        case "disabled":
                            AppStatusReason = DisabledReason(statusValue);
                            return AppInfoStatusEnum.Disabled;

                        case "awaiting_memproofs":
                            return AppInfoStatusEnum.AwaitingMemproofs;

                        case "awaiting_restore":
                            return AppInfoStatusEnum.AwaitingRestore;

                        case "unrecoverable":
                            // Unrecoverable(CellId, UnrecoverableCellReason): the reason is {"type": "chain_fork_warrant" | ...}.
                            if (statusValue is object[] parts && parts.Length > 1 && parts[1] is Dictionary<object, object> reason && reason.TryGetValue("type", out object reasonType))
                                AppStatusReason = reasonType?.ToString();
                            return AppInfoStatusEnum.Unrecoverable;
                    }
                }

                if (status != null)
                {
                    if (status.ContainsKey("running"))
                        return AppInfoStatusEnum.Running;

                    else if (status.ContainsKey("paused"))
                    {
                        Dictionary<object, object> pausedDict = status["paused"] as Dictionary<object, object>;

                        if (pausedDict != null && pausedDict.ContainsKey("reason"))
                        {
                            Dictionary<object, object> reasonDict = pausedDict["reason"] as Dictionary<object, object>;

                            if (reasonDict != null && reasonDict.ContainsKey("error"))
                                AppStatusReason = reasonDict["error"].ToString();
                        }

                        return AppInfoStatusEnum.Paused;
                    }

                    else if (status.ContainsKey("disabled"))
                    {
                        Dictionary<object, object> disabledDict = status["disabled"] as Dictionary<object, object>;

                        if (disabledDict != null && disabledDict.ContainsKey("reason"))
                        {
                            Dictionary<object, object> reasonDict = disabledDict["reason"] as Dictionary<object, object>;

                            if (reasonDict != null && reasonDict.ContainsKey("never_started"))
                                AppStatusReason = "Never Started";

                            else if (reasonDict != null && reasonDict.ContainsKey("user"))
                                AppStatusReason = "User";

                            else if (reasonDict != null && reasonDict.ContainsKey("error"))
                                AppStatusReason = reasonDict["error"].ToString();
                        }

                        return AppInfoStatusEnum.Disabled;
                    }

                    // New in Holochain 0.7.0
                    else if (status.ContainsKey("awaiting_memproofs"))
                        return AppInfoStatusEnum.AwaitingMemproofs;
                }

                return AppInfoStatusEnum.None;
            }
        }

        [IgnoreMember]
        public string AppStatusReason { get; private set; }

        private static string DisabledReason(object value)
        {
            if (value is Dictionary<object, object> reason && reason.TryGetValue("type", out object reasonType))
            {
                switch (reasonType?.ToString())
                {
                    case "never_started": return "Never Started";
                    case "not_started_after_providing_memproofs": return "Not Started After Providing Memproofs";
                    case "user": return "User";
                    case "error": return reason.TryGetValue("value", out object error) ? error?.ToString() : "Error";
                }
            }
            return null;
        }


        [Key("manifest")]
        public AppManifest manifest { get; set; }
    }
}
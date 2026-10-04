
using MessagePack;
using System;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests
{
    /// <summary>
    /// Mirrors AdminRequest::AttachAppInterface (Holochain 0.7.0):
    /// { port: Option&lt;u16&gt;, danger_bind_addr: Option&lt;String&gt;, allowed_origins: AllowedOrigins,
    ///   installed_app_id: Option&lt;InstalledAppId&gt; }.
    /// allowed_origins is required: the conductor rejects a request without it
    /// ("Failed to deserialize request", confirmed against a live 0.7.0 conductor).
    /// </summary>
    [MessagePackObject]
    public class AttachAppInterfaceRequest
    {
        [Key("port")]
        public UInt16? port { get; set; }

        [Key("danger_bind_addr")]
        public string danger_bind_addr { get; set; }

        /// <summary>"*" for any origin, or a comma-separated list of allowed origins.</summary>
        [Key("allowed_origins")]
        public string allowed_origins { get; set; } = "*";

        /// <summary>Restrict the interface to one app, or null for all apps.</summary>
        [Key("installed_app_id")]
        public string installed_app_id { get; set; }
    }
}

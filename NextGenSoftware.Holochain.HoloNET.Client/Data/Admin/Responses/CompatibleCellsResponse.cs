using System.Collections.Generic;
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors holochain_conductor_api::admin_interface::AdminResponse::CompatibleCells
    /// (Holochain 0.6.1):
    ///
    /// #[cfg(feature = "unstable-migration")]
    /// pub type CompatibleCells = BTreeSet&lt;(InstalledAppId, BTreeSet&lt;CellId&gt;)&gt;;
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/admin_interface.rs
    ///
    /// NOTE: This is gated behind the `unstable-migration` feature flag in Holochain 0.6.1 and
    /// may not be present/enabled on all conductor builds.
    /// </summary>
    [MessagePackObject]
    public class AppCompatibleCells
    {
        [Key(0)]
        public string installed_app_id { get; set; }

        [Key(1)]
        public List<CellId> cell_ids { get; set; }
    }
}

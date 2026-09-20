
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests
{
    [MessagePackObject]
    public class DeleteCloneCellRequest //Same as DisableCloneCellRequest on App API.
    {
        /// <summary>
        /// The app id that the clone cell belongs to
        /// </summary>
        [Key("app_id")]
        public string app_id { get; set; }

        /// <summary>
        /// The CloneCellId identifying the clone cell — either a clone id string
        /// (e.g. "role_name.0") or a CellId byte[][] tuple.
        /// Wire: {"CloneId": "..."} or {"CellId": [[dna_hash], [agent_key]]}.
        /// Kept as dynamic because MessagePack cannot transparently handle this Rust tagged enum.
        /// </summary>
        [Key("clone_cell_id")]
        public dynamic clone_cell_id { get; set; }
    }
}


//export interface DisableCloneCellRequest
//{
//    /**
//     * The app id that the clone cell belongs to
//     */
//    app_id: InstalledAppId;
//  /**
//   * The clone id or cell id of the clone cell
//   */
//  clone_cell_id: RoleName | CellId; //RoleName is a string and CellId is a byte[][]
//}
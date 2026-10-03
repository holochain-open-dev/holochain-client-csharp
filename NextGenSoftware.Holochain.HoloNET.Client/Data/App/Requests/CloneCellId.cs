using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Builds the wire form of holochain_zome_types::clone::CloneCellId (Holochain 0.7.0).
    /// Rust: #[serde(tag = "type", content = "value", rename_all = "snake_case")]
    /// Wire: {"type": "clone_id", "value": "role_name.0"} or {"type": "dna_hash", "value": bytes}
    /// Pass the result as clone_cell_id on Enable/Disable/DeleteCloneCellRequest.
    /// </summary>
    public static class CloneCellId
    {
        public static Dictionary<string, object> FromCloneId(string cloneId) =>
            new Dictionary<string, object> { { "type", "clone_id" }, { "value", cloneId } };

        public static Dictionary<string, object> FromDnaHash(byte[] dnaHash) =>
            new Dictionary<string, object> { { "type", "dna_hash" }, { "value", dnaHash } };

        /// <summary>
        /// Accepts the legacy inputs callers already pass and returns the tagged wire form:
        /// string → clone_id; byte[] → dna_hash; byte[][] CellId → dna_hash of cellId[0]
        /// (0.7.0 has no CellId variant; a clone cell's DNA hash identifies it within the app).
        /// An already-built dictionary is passed through unchanged.
        /// </summary>
        public static object Normalize(object cloneCellId) => cloneCellId switch
        {
            string cloneId => FromCloneId(cloneId),
            byte[][] cellId => FromDnaHash(cellId[0]),
            byte[] dnaHash => FromDnaHash(dnaHash),
            _ => cloneCellId
        };
    }
}

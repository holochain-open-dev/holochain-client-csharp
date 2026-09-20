namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors holochain_conductor_api::config::conductor::keystore_config::KeystoreConfig from
    /// Holochain 0.6.1. This defines how the conductor connects to a keystore (lair-keystore),
    /// NOT a client-configurable KDF algorithm/keystore-type as a previous, unverified pass on
    /// this file invented (KDFAlgorithm = "PBKDF2", Type = KeystoreType.FileSystem, HSM support,
    /// key rotation/backup/recovery settings, etc. - none of that exists in the real Rust struct).
    ///
    /// The real type is a serde tagged enum with exactly three variants:
    /// DangerTestKeystore, LairServer { connection_url }, LairServerInProc { lair_root }.
    /// There is no KDF, password, key rotation, backup, recovery, or HSM configuration exposed
    /// here at all - lair-keystore manages its own passphrase handling via the conductor CLI
    /// entrypoint, not via this config struct.
    ///
    /// Verified against:
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/config/conductor/keystore_config.rs
    /// </summary>
    public class KeystoreConfig
    {
        /// <summary>
        /// Which keystore connection mode to use. Mirrors the Rust enum's serde "type" tag
        /// (rename_all = "snake_case"): danger_test_keystore, lair_server, lair_server_in_proc.
        /// Default mirrors Rust's Default impl: LairServerInProc.
        /// </summary>
        public KeystoreConfigType Type { get; set; } = KeystoreConfigType.LairServerInProc;

        /// <summary>
        /// Only used when Type == LairServer. The "connectionUrl" as defined in the target
        /// lair-keystore-config.yaml (also obtainable by running `lair-keystore url`).
        /// Mirrors LairServer.connection_url: url2::Url2.
        /// </summary>
        public string ConnectionUrl { get; set; } = "";

        /// <summary>
        /// Only used when Type == LairServerInProc. The "lair_root" path, i.e. the directory
        /// containing the lair-keystore-config.yaml file. If null/empty, the conductor defaults
        /// to [environment_path]/ks. Mirrors LairServerInProc.lair_root: Option&lt;KeystorePath&gt;.
        /// </summary>
        public string LairRoot { get; set; } = null;
    }

    /// <summary>
    /// Mirrors the variants of holochain_conductor_api's KeystoreConfig enum (Holochain 0.7.0).
    /// </summary>
    public enum KeystoreConfigType
    {
        /// <summary>
        /// Uses a test keystore instead of lair, generating publicly accessible private keys.
        /// DO NOT USE IN PRODUCTION - mirrors Rust's DangerTestKeystore variant.
        /// </summary>
        DangerTestKeystore,

        /// <summary>
        /// Connect to an external lair-keystore process via ConnectionUrl.
        /// </summary>
        LairServer,

        /// <summary>
        /// Run a lair-keystore server in-process, optionally rooted at LairRoot.
        /// </summary>
        LairServerInProc
    }
}

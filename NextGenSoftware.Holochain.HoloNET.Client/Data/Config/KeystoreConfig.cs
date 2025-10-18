using System;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Configuration for integrated keystore in Holochain 0.5.6+
    /// </summary>
    public class KeystoreConfig
    {
        /// <summary>
        /// Enable integrated keystore. Default is true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Keystore type to use.
        /// </summary>
        public KeystoreType Type { get; set; } = KeystoreType.FileSystem;

        /// <summary>
        /// Keystore path for file system keystore.
        /// </summary>
        public string Path { get; set; } = "";

        /// <summary>
        /// Keystore password for encryption.
        /// </summary>
        public string Password { get; set; } = "";

        /// <summary>
        /// Enable key derivation function (KDF).
        /// </summary>
        public bool EnableKDF { get; set; } = true;

        /// <summary>
        /// KDF algorithm to use.
        /// </summary>
        public string KDFAlgorithm { get; set; } = "PBKDF2";

        /// <summary>
        /// KDF iterations for key derivation.
        /// </summary>
        public int KDFIterations { get; set; } = 100000;

        /// <summary>
        /// Enable key rotation.
        /// </summary>
        public bool EnableKeyRotation { get; set; } = true;

        /// <summary>
        /// Key rotation interval in days.
        /// </summary>
        public int KeyRotationIntervalDays { get; set; } = 90;

        /// <summary>
        /// Maximum number of keys to keep.
        /// </summary>
        public int MaxKeys { get; set; } = 10;

        /// <summary>
        /// Enable key backup.
        /// </summary>
        public bool EnableKeyBackup { get; set; } = true;

        /// <summary>
        /// Backup path for keys.
        /// </summary>
        public string BackupPath { get; set; } = "";

        /// <summary>
        /// Enable key recovery.
        /// </summary>
        public bool EnableKeyRecovery { get; set; } = true;

        /// <summary>
        /// Recovery phrase for key recovery.
        /// </summary>
        public string RecoveryPhrase { get; set; } = "";

        /// <summary>
        /// Enable hardware security module (HSM) support.
        /// </summary>
        public bool EnableHSM { get; set; } = false;

        /// <summary>
        /// HSM configuration.
        /// </summary>
        public HSMConfig HSM { get; set; } = new HSMConfig();

        /// <summary>
        /// Key generation configuration.
        /// </summary>
        public KeyGenerationConfig KeyGeneration { get; set; } = new KeyGenerationConfig();

        /// <summary>
        /// Key storage configuration.
        /// </summary>
        public KeyStorageConfig KeyStorage { get; set; } = new KeyStorageConfig();
    }

    /// <summary>
    /// Keystore types available.
    /// </summary>
    public enum KeystoreType
    {
        FileSystem,
        Memory,
        Database,
        HSM,
        Cloud
    }

    /// <summary>
    /// HSM configuration for hardware security modules.
    /// </summary>
    public class HSMConfig
    {
        /// <summary>
        /// HSM provider name.
        /// </summary>
        public string Provider { get; set; } = "";

        /// <summary>
        /// HSM slot number.
        /// </summary>
        public int Slot { get; set; } = 0;

        /// <summary>
        /// HSM PIN for authentication.
        /// </summary>
        public string PIN { get; set; } = "";

        /// <summary>
        /// HSM certificate label.
        /// </summary>
        public string CertificateLabel { get; set; } = "";

        /// <summary>
        /// HSM private key label.
        /// </summary>
        public string PrivateKeyLabel { get; set; } = "";
    }

    /// <summary>
    /// Key generation configuration.
    /// </summary>
    public class KeyGenerationConfig
    {
        /// <summary>
        /// Key algorithm to use.
        /// </summary>
        public string Algorithm { get; set; } = "ed25519";

        /// <summary>
        /// Key size in bits.
        /// </summary>
        public int KeySize { get; set; } = 256;

        /// <summary>
        /// Enable deterministic key generation.
        /// </summary>
        public bool EnableDeterministic { get; set; } = true;

        /// <summary>
        /// Seed for deterministic key generation.
        /// </summary>
        public string Seed { get; set; } = "";

        /// <summary>
        /// Enable key derivation from master key.
        /// </summary>
        public bool EnableKeyDerivation { get; set; } = true;

        /// <summary>
        /// Key derivation path.
        /// </summary>
        public string DerivationPath { get; set; } = "m/44'/0'/0'/0";
    }

    /// <summary>
    /// Key storage configuration.
    /// </summary>
    public class KeyStorageConfig
    {
        /// <summary>
        /// Storage type for keys.
        /// </summary>
        public KeyStorageType Type { get; set; } = KeyStorageType.Encrypted;

        /// <summary>
        /// Enable key compression.
        /// </summary>
        public bool EnableCompression { get; set; } = true;

        /// <summary>
        /// Enable key indexing.
        /// </summary>
        public bool EnableIndexing { get; set; } = true;

        /// <summary>
        /// Maximum key age in days.
        /// </summary>
        public int MaxKeyAgeDays { get; set; } = 365;

        /// <summary>
        /// Enable key archival.
        /// </summary>
        public bool EnableArchival { get; set; } = true;

        /// <summary>
        /// Archive path for old keys.
        /// </summary>
        public string ArchivePath { get; set; } = "";
    }

    /// <summary>
    /// Key storage types available.
    /// </summary>
    public enum KeyStorageType
    {
        Plain,
        Encrypted,
        Compressed,
        Indexed
    }
}

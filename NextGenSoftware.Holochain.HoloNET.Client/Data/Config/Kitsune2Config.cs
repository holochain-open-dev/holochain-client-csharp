using System;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Configuration for Kitsune2 networking in Holochain 0.5.6+
    /// </summary>
    public class Kitsune2Config
    {
        /// <summary>
        /// Enable Kitsune2 networking. Default is true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Network topology configuration for Kitsune2.
        /// </summary>
        public NetworkTopologyConfig NetworkTopology { get; set; } = new NetworkTopologyConfig();

        /// <summary>
        /// Bootstrap nodes for network discovery.
        /// </summary>
        public List<string> BootstrapNodes { get; set; } = new List<string>();

        /// <summary>
        /// Network space configuration.
        /// </summary>
        public string NetworkSpace { get; set; } = "default";

        /// <summary>
        /// Agent configuration for Kitsune2.
        /// </summary>
        public AgentConfig Agent { get; set; } = new AgentConfig();

        /// <summary>
        /// Signature configuration for Kitsune2.
        /// </summary>
        public SignatureConfig Signature { get; set; } = new SignatureConfig();

        /// <summary>
        /// Transport configuration for Kitsune2.
        /// </summary>
        public TransportConfig Transport { get; set; } = new TransportConfig();
    }

    /// <summary>
    /// Network topology configuration for Kitsune2.
    /// </summary>
    public class NetworkTopologyConfig
    {
        /// <summary>
        /// Maximum number of connections.
        /// </summary>
        public int MaxConnections { get; set; } = 50;

        /// <summary>
        /// Minimum number of connections.
        /// </summary>
        public int MinConnections { get; set; } = 5;

        /// <summary>
        /// Connection timeout in milliseconds.
        /// </summary>
        public int ConnectionTimeoutMs { get; set; } = 30000;

        /// <summary>
        /// Keep-alive interval in milliseconds.
        /// </summary>
        public int KeepAliveIntervalMs { get; set; } = 30000;
    }

    /// <summary>
    /// Agent configuration for Kitsune2.
    /// </summary>
    public class AgentConfig
    {
        /// <summary>
        /// Agent public key.
        /// </summary>
        public string PublicKey { get; set; } = "";

        /// <summary>
        /// Agent private key (encrypted).
        /// </summary>
        public string PrivateKey { get; set; } = "";

        /// <summary>
        /// Agent signature algorithm.
        /// </summary>
        public string SignatureAlgorithm { get; set; } = "ed25519";
    }

    /// <summary>
    /// Signature configuration for Kitsune2.
    /// </summary>
    public class SignatureConfig
    {
        /// <summary>
        /// Signature algorithm to use.
        /// </summary>
        public string Algorithm { get; set; } = "ed25519";

        /// <summary>
        /// Signature verification enabled.
        /// </summary>
        public bool VerificationEnabled { get; set; } = true;

        /// <summary>
        /// Signature timeout in milliseconds.
        /// </summary>
        public int TimeoutMs { get; set; } = 5000;
    }

    /// <summary>
    /// Transport configuration for Kitsune2.
    /// </summary>
    public class TransportConfig
    {
        /// <summary>
        /// Transport protocol to use.
        /// </summary>
        public string Protocol { get; set; } = "quic";

        /// <summary>
        /// Transport port.
        /// </summary>
        public int Port { get; set; } = 0; // Auto-assign

        /// <summary>
        /// Transport bind address.
        /// </summary>
        public string BindAddress { get; set; } = "0.0.0.0";

        /// <summary>
        /// Transport timeout in milliseconds.
        /// </summary>
        public int TimeoutMs { get; set; } = 30000;
    }
}

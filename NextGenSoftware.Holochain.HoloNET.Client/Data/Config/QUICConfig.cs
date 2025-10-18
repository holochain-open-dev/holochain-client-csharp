using System;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Configuration for QUIC protocol in Holochain 0.5.6+
    /// </summary>
    public class QUICConfig
    {
        /// <summary>
        /// Enable QUIC protocol. Default is true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// QUIC version to use.
        /// </summary>
        public string Version { get; set; } = "1";

        /// <summary>
        /// Maximum number of concurrent streams.
        /// </summary>
        public int MaxConcurrentStreams { get; set; } = 100;

        /// <summary>
        /// Maximum stream data in bytes.
        /// </summary>
        public long MaxStreamData { get; set; } = 1024 * 1024; // 1MB

        /// <summary>
        /// Maximum connection data in bytes.
        /// </summary>
        public long MaxConnectionData { get; set; } = 10 * 1024 * 1024; // 10MB

        /// <summary>
        /// Connection timeout in milliseconds.
        /// </summary>
        public int ConnectionTimeoutMs { get; set; } = 30000;

        /// <summary>
        /// Handshake timeout in milliseconds.
        /// </summary>
        public int HandshakeTimeoutMs { get; set; } = 10000;

        /// <summary>
        /// Idle timeout in milliseconds.
        /// </summary>
        public int IdleTimeoutMs { get; set; } = 300000; // 5 minutes

        /// <summary>
        /// Keep-alive interval in milliseconds.
        /// </summary>
        public int KeepAliveIntervalMs { get; set; } = 30000;

        /// <summary>
        /// Maximum packet size in bytes.
        /// </summary>
        public int MaxPacketSize { get; set; } = 1350;

        /// <summary>
        /// Initial congestion window size.
        /// </summary>
        public int InitialCongestionWindow { get; set; } = 10;

        /// <summary>
        /// Maximum congestion window size.
        /// </summary>
        public int MaxCongestionWindow { get; set; } = 200;

        /// <summary>
        /// Enable 0-RTT (zero round-trip time) resumption.
        /// </summary>
        public bool EnableZeroRTT { get; set; } = true;

        /// <summary>
        /// Enable connection migration.
        /// </summary>
        public bool EnableConnectionMigration { get; set; } = true;

        /// <summary>
        /// Enable multipath support.
        /// </summary>
        public bool EnableMultipath { get; set; } = false;

        /// <summary>
        /// TLS configuration for QUIC.
        /// </summary>
        public TLSConfig TLS { get; set; } = new TLSConfig();

        /// <summary>
        /// Transport configuration for QUIC.
        /// </summary>
        public QUICTransportConfig Transport { get; set; } = new QUICTransportConfig();
    }

    /// <summary>
    /// TLS configuration for QUIC.
    /// </summary>
    public class TLSConfig
    {
        /// <summary>
        /// TLS version to use.
        /// </summary>
        public string Version { get; set; } = "1.3";

        /// <summary>
        /// Certificate file path.
        /// </summary>
        public string CertificateFile { get; set; } = "";

        /// <summary>
        /// Private key file path.
        /// </summary>
        public string PrivateKeyFile { get; set; } = "";

        /// <summary>
        /// CA certificate file path.
        /// </summary>
        public string CACertificateFile { get; set; } = "";

        /// <summary>
        /// Enable client certificate verification.
        /// </summary>
        public bool VerifyClientCertificates { get; set; } = true;

        /// <summary>
        /// Enable server certificate verification.
        /// </summary>
        public bool VerifyServerCertificates { get; set; } = true;
    }

    /// <summary>
    /// Transport configuration for QUIC.
    /// </summary>
    public class QUICTransportConfig
    {
        /// <summary>
        /// Transport port.
        /// </summary>
        public int Port { get; set; } = 0; // Auto-assign

        /// <summary>
        /// Bind address.
        /// </summary>
        public string BindAddress { get; set; } = "0.0.0.0";

        /// <summary>
        /// Maximum number of connections.
        /// </summary>
        public int MaxConnections { get; set; } = 1000;

        /// <summary>
        /// Connection timeout in milliseconds.
        /// </summary>
        public int ConnectionTimeoutMs { get; set; } = 30000;

        /// <summary>
        /// Send buffer size in bytes.
        /// </summary>
        public int SendBufferSize { get; set; } = 64 * 1024; // 64KB

        /// <summary>
        /// Receive buffer size in bytes.
        /// </summary>
        public int ReceiveBufferSize { get; set; } = 64 * 1024; // 64KB
    }
}

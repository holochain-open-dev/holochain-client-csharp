using System;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Configuration for caching layer in Holochain 0.5.6+
    /// </summary>
    public class CacheConfig
    {
        /// <summary>
        /// Enable caching layer. Default is true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Cache type to use.
        /// </summary>
        public CacheType Type { get; set; } = CacheType.Memory;

        /// <summary>
        /// Maximum cache size in bytes.
        /// </summary>
        public long MaxSizeBytes { get; set; } = 100 * 1024 * 1024; // 100MB

        /// <summary>
        /// Maximum number of cache entries.
        /// </summary>
        public int MaxEntries { get; set; } = 10000;

        /// <summary>
        /// Default cache TTL in seconds.
        /// </summary>
        public int DefaultTTLSeconds { get; set; } = 3600; // 1 hour

        /// <summary>
        /// Default cache duration in minutes.
        /// </summary>
        public int DefaultCacheDurationMinutes { get; set; } = 60; // 1 hour

        /// <summary>
        /// Cache eviction policy.
        /// </summary>
        public CacheEvictionPolicy EvictionPolicy { get; set; } = CacheEvictionPolicy.LRU;

        /// <summary>
        /// Enable cache compression.
        /// </summary>
        public bool EnableCompression { get; set; } = true;

        /// <summary>
        /// Compression algorithm to use.
        /// </summary>
        public string CompressionAlgorithm { get; set; } = "gzip";

        /// <summary>
        /// Compression level (1-9).
        /// </summary>
        public int CompressionLevel { get; set; } = 6;

        /// <summary>
        /// Enable cache statistics.
        /// </summary>
        public bool EnableStatistics { get; set; } = true;

        /// <summary>
        /// Cache statistics collection interval in seconds.
        /// </summary>
        public int StatisticsIntervalSeconds { get; set; } = 60;

        /// <summary>
        /// Enable cache warming.
        /// </summary>
        public bool EnableWarming { get; set; } = true;

        /// <summary>
        /// Cache warming configuration.
        /// </summary>
        public CacheWarmingConfig Warming { get; set; } = new CacheWarmingConfig();

        /// <summary>
        /// Cache persistence configuration.
        /// </summary>
        public CachePersistenceConfig Persistence { get; set; } = new CachePersistenceConfig();

        /// <summary>
        /// Cache distribution configuration.
        /// </summary>
        public CacheDistributionConfig Distribution { get; set; } = new CacheDistributionConfig();
    }

    /// <summary>
    /// Cache types available.
    /// </summary>
    public enum CacheType
    {
        Memory,
        FileSystem,
        Database,
        Redis,
        Distributed
    }

    /// <summary>
    /// Cache eviction policies available.
    /// </summary>
    public enum CacheEvictionPolicy
    {
        LRU,        // Least Recently Used
        LFU,        // Least Frequently Used
        FIFO,       // First In, First Out
        TTL,        // Time To Live
        Random      // Random
    }

    /// <summary>
    /// Cache warming configuration.
    /// </summary>
    public class CacheWarmingConfig
    {
        /// <summary>
        /// Enable cache warming.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Warming strategy to use.
        /// </summary>
        public CacheWarmingStrategy Strategy { get; set; } = CacheWarmingStrategy.Preload;

        /// <summary>
        /// Warming interval in seconds.
        /// </summary>
        public int IntervalSeconds { get; set; } = 300; // 5 minutes

        /// <summary>
        /// Maximum number of items to warm.
        /// </summary>
        public int MaxItems { get; set; } = 1000;

        /// <summary>
        /// Warming timeout in seconds.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Enable parallel warming.
        /// </summary>
        public bool EnableParallel { get; set; } = true;

        /// <summary>
        /// Maximum parallel warming threads.
        /// </summary>
        public int MaxParallelThreads { get; set; } = 5;
    }

    /// <summary>
    /// Cache warming strategies available.
    /// </summary>
    public enum CacheWarmingStrategy
    {
        Preload,        // Preload frequently accessed items
        Predictive,     // Predict and preload likely needed items
        Scheduled,      // Schedule warming at specific times
        OnDemand        // Warm on demand
    }

    /// <summary>
    /// Cache persistence configuration.
    /// </summary>
    public class CachePersistenceConfig
    {
        /// <summary>
        /// Enable cache persistence.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Persistence type.
        /// </summary>
        public CachePersistenceType Type { get; set; } = CachePersistenceType.FileSystem;

        /// <summary>
        /// Persistence path.
        /// </summary>
        public string Path { get; set; } = "";

        /// <summary>
        /// Persistence interval in seconds.
        /// </summary>
        public int IntervalSeconds { get; set; } = 60; // 1 minute

        /// <summary>
        /// Enable compression for persistence.
        /// </summary>
        public bool EnableCompression { get; set; } = true;

        /// <summary>
        /// Maximum persistence file size in bytes.
        /// </summary>
        public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB

        /// <summary>
        /// Maximum number of persistence files.
        /// </summary>
        public int MaxFiles { get; set; } = 10;
    }

    /// <summary>
    /// Cache persistence types available.
    /// </summary>
    public enum CachePersistenceType
    {
        FileSystem,
        Database,
        Cloud,
        Memory
    }

    /// <summary>
    /// Cache distribution configuration.
    /// </summary>
    public class CacheDistributionConfig
    {
        /// <summary>
        /// Enable cache distribution.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Distribution type.
        /// </summary>
        public CacheDistributionType Type { get; set; } = CacheDistributionType.Replication;

        /// <summary>
        /// Distribution nodes.
        /// </summary>
        public List<string> Nodes { get; set; } = new List<string>();

        /// <summary>
        /// Distribution replication factor.
        /// </summary>
        public int ReplicationFactor { get; set; } = 3;

        /// <summary>
        /// Distribution consistency level.
        /// </summary>
        public CacheConsistencyLevel ConsistencyLevel { get; set; } = CacheConsistencyLevel.Eventual;

        /// <summary>
        /// Distribution timeout in seconds.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Enable distribution encryption.
        /// </summary>
        public bool EnableEncryption { get; set; } = true;
    }

    /// <summary>
    /// Cache distribution types available.
    /// </summary>
    public enum CacheDistributionType
    {
        Replication,
        Sharding,
        ConsistentHashing,
        Ring
    }

    /// <summary>
    /// Cache consistency levels available.
    /// </summary>
    public enum CacheConsistencyLevel
    {
        Strong,
        Eventual,
        Weak
    }
}

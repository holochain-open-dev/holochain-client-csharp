using System;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// This is a HoloNET-CLIENT-SIDE caching layer config, NOT a mirror of any Holochain Rust
    /// conductor struct. Researched holochain_conductor_api's ConductorConfig
    /// (https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/config/conductor.rs)
    /// and confirmed there is no generic "cache" concept exposed there at all - the conductor has
    /// an internal wasm compilation cache (see WASMConfig.cs in this directory) and SQLite-backed
    /// databases, but nothing resembling a configurable client-side response/object cache.
    ///
    /// This class is therefore honestly relabelled: it is a legitimate, HoloNET-specific local
    /// cache configuration (e.g. for caching zome call results or DNA metadata client-side), not a
    /// fabricated mirror of a non-existent Rust struct. Its fields are deliberately kept simple
    /// (TTL, max size/entries, eviction policy) since this is purely a client-side concern that
    /// HoloNET is free to define as it sees fit. The previous version of this file contained
    /// several over-elaborate, unused nested configs (warming, persistence, distribution/sharding,
    /// HSM-style encryption) that implied integration with non-existent distributed-cache
    /// infrastructure; those have been removed as speculative scaffolding pending a real use case.
    /// </summary>
    public class CacheConfig
    {
        /// <summary>
        /// Enable the client-side cache. Default is true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Maximum number of entries to retain in the cache before eviction.
        /// </summary>
        public int MaxEntries { get; set; } = 1000;

        /// <summary>
        /// Default time-to-live for cached entries, in seconds.
        /// </summary>
        public int DefaultTTLSeconds { get; set; } = 300; // 5 minutes

        /// <summary>
        /// Eviction policy applied once MaxEntries is exceeded.
        /// </summary>
        public CacheEvictionPolicy EvictionPolicy { get; set; } = CacheEvictionPolicy.LRU;
    }

    /// <summary>
    /// Eviction policies for the HoloNET client-side cache (client concept only, no Rust mirror).
    /// </summary>
    public enum CacheEvictionPolicy
    {
        /// <summary>Least Recently Used.</summary>
        LRU,

        /// <summary>Least Frequently Used.</summary>
        LFU,

        /// <summary>First In, First Out.</summary>
        FIFO
    }
}

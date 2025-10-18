using System;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Configuration for WASM optimization in Holochain 0.5.6+
    /// </summary>
    public class WASMConfig
    {
        /// <summary>
        /// Enable WASM optimization. Default is true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// WASM optimization level.
        /// </summary>
        public WASMOptimizationLevel OptimizationLevel { get; set; } = WASMOptimizationLevel.Balanced;

        /// <summary>
        /// Enable WASM compilation caching.
        /// </summary>
        public bool EnableCompilationCaching { get; set; } = true;

        /// <summary>
        /// WASM compilation cache path.
        /// </summary>
        public string CompilationCachePath { get; set; } = "";

        /// <summary>
        /// Maximum WASM module size in bytes.
        /// </summary>
        public long MaxModuleSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB

        /// <summary>
        /// Maximum WASM memory size in bytes.
        /// </summary>
        public long MaxMemorySizeBytes { get; set; } = 100 * 1024 * 1024; // 100MB

        /// <summary>
        /// Maximum WASM stack size in bytes.
        /// </summary>
        public long MaxStackSizeBytes { get; set; } = 1024 * 1024; // 1MB

        /// <summary>
        /// Enable WASM garbage collection.
        /// </summary>
        public bool EnableGarbageCollection { get; set; } = true;

        /// <summary>
        /// WASM garbage collection interval in milliseconds.
        /// </summary>
        public int GCIntervalMs { get; set; } = 1000; // 1 second

        /// <summary>
        /// Enable WASM memory optimization.
        /// </summary>
        public bool EnableMemoryOptimization { get; set; } = true;

        /// <summary>
        /// Enable WASM execution optimization.
        /// </summary>
        public bool EnableExecutionOptimization { get; set; } = true;

        /// <summary>
        /// Enable WASM parallel execution.
        /// </summary>
        public bool EnableParallelExecution { get; set; } = true;

        /// <summary>
        /// Maximum parallel WASM threads.
        /// </summary>
        public int MaxParallelThreads { get; set; } = 4;

        /// <summary>
        /// Enable WASM profiling.
        /// </summary>
        public bool EnableProfiling { get; set; } = false;

        /// <summary>
        /// WASM profiling configuration.
        /// </summary>
        public WASMProfilingConfig Profiling { get; set; } = new WASMProfilingConfig();

        /// <summary>
        /// WASM security configuration.
        /// </summary>
        public WASMSecurityConfig Security { get; set; } = new WASMSecurityConfig();

        /// <summary>
        /// WASM performance configuration.
        /// </summary>
        public WASMPerformanceConfig Performance { get; set; } = new WASMPerformanceConfig();
    }

    /// <summary>
    /// WASM optimization levels available.
    /// </summary>
    public enum WASMOptimizationLevel
    {
        None,           // No optimization
        Basic,          // Basic optimization
        Balanced,       // Balanced optimization
        Aggressive,     // Aggressive optimization
        Maximum         // Maximum optimization
    }

    /// <summary>
    /// WASM profiling configuration.
    /// </summary>
    public class WASMProfilingConfig
    {
        /// <summary>
        /// Enable WASM profiling.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Profiling output path.
        /// </summary>
        public string OutputPath { get; set; } = "";

        /// <summary>
        /// Profiling format.
        /// </summary>
        public WASMProfilingFormat Format { get; set; } = WASMProfilingFormat.JSON;

        /// <summary>
        /// Profiling sampling rate (0.0 to 1.0).
        /// </summary>
        public double SamplingRate { get; set; } = 0.1; // 10%

        /// <summary>
        /// Profiling duration in seconds.
        /// </summary>
        public int DurationSeconds { get; set; } = 60; // 1 minute

        /// <summary>
        /// Enable function-level profiling.
        /// </summary>
        public bool EnableFunctionProfiling { get; set; } = true;

        /// <summary>
        /// Enable memory profiling.
        /// </summary>
        public bool EnableMemoryProfiling { get; set; } = true;

        /// <summary>
        /// Enable execution profiling.
        /// </summary>
        public bool EnableExecutionProfiling { get; set; } = true;
    }

    /// <summary>
    /// WASM profiling formats available.
    /// </summary>
    public enum WASMProfilingFormat
    {
        JSON,
        CSV,
        Binary,
        Text
    }

    /// <summary>
    /// WASM security configuration.
    /// </summary>
    public class WASMSecurityConfig
    {
        /// <summary>
        /// Enable WASM security sandbox.
        /// </summary>
        public bool EnableSandbox { get; set; } = true;

        /// <summary>
        /// Enable WASM memory protection.
        /// </summary>
        public bool EnableMemoryProtection { get; set; } = true;

        /// <summary>
        /// Enable WASM execution protection.
        /// </summary>
        public bool EnableExecutionProtection { get; set; } = true;

        /// <summary>
        /// Enable WASM module validation.
        /// </summary>
        public bool EnableModuleValidation { get; set; } = true;

        /// <summary>
        /// Enable WASM signature verification.
        /// </summary>
        public bool EnableSignatureVerification { get; set; } = true;

        /// <summary>
        /// WASM security policy.
        /// </summary>
        public WASMSecurityPolicy SecurityPolicy { get; set; } = WASMSecurityPolicy.Strict;

        /// <summary>
        /// Allowed WASM imports.
        /// </summary>
        public List<string> AllowedImports { get; set; } = new List<string>();

        /// <summary>
        /// Blocked WASM imports.
        /// </summary>
        public List<string> BlockedImports { get; set; } = new List<string>();

        /// <summary>
        /// Maximum WASM execution time in milliseconds.
        /// </summary>
        public int MaxExecutionTimeMs { get; set; } = 30000; // 30 seconds
    }

    /// <summary>
    /// WASM security policies available.
    /// </summary>
    public enum WASMSecurityPolicy
    {
        Permissive,     // Allow most operations
        Balanced,       // Balanced security
        Strict,         // Strict security
        Maximum         // Maximum security
    }

    /// <summary>
    /// WASM performance configuration.
    /// </summary>
    public class WASMPerformanceConfig
    {
        /// <summary>
        /// Enable WASM JIT compilation.
        /// </summary>
        public bool EnableJIT { get; set; } = true;

        /// <summary>
        /// Enable WASM AOT compilation.
        /// </summary>
        public bool EnableAOT { get; set; } = false;

        /// <summary>
        /// Enable WASM tiered compilation.
        /// </summary>
        public bool EnableTieredCompilation { get; set; } = true;

        /// <summary>
        /// WASM compilation threshold.
        /// </summary>
        public int CompilationThreshold { get; set; } = 1000;

        /// <summary>
        /// Enable WASM inlining.
        /// </summary>
        public bool EnableInlining { get; set; } = true;

        /// <summary>
        /// Maximum inlining depth.
        /// </summary>
        public int MaxInliningDepth { get; set; } = 3;

        /// <summary>
        /// Enable WASM loop optimization.
        /// </summary>
        public bool EnableLoopOptimization { get; set; } = true;

        /// <summary>
        /// Enable WASM dead code elimination.
        /// </summary>
        public bool EnableDeadCodeElimination { get; set; } = true;

        /// <summary>
        /// Enable WASM constant folding.
        /// </summary>
        public bool EnableConstantFolding { get; set; } = true;

        /// <summary>
        /// Enable WASM register allocation optimization.
        /// </summary>
        public bool EnableRegisterAllocation { get; set; } = true;

        /// <summary>
        /// WASM performance monitoring.
        /// </summary>
        public WASMPerformanceMonitoringConfig Monitoring { get; set; } = new WASMPerformanceMonitoringConfig();
    }

    /// <summary>
    /// WASM performance monitoring configuration.
    /// </summary>
    public class WASMPerformanceMonitoringConfig
    {
        /// <summary>
        /// Enable performance monitoring.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Monitoring interval in milliseconds.
        /// </summary>
        public int IntervalMs { get; set; } = 1000; // 1 second

        /// <summary>
        /// Enable execution time monitoring.
        /// </summary>
        public bool EnableExecutionTimeMonitoring { get; set; } = true;

        /// <summary>
        /// Enable memory usage monitoring.
        /// </summary>
        public bool EnableMemoryUsageMonitoring { get; set; } = true;

        /// <summary>
        /// Enable CPU usage monitoring.
        /// </summary>
        public bool EnableCPUUsageMonitoring { get; set; } = true;

        /// <summary>
        /// Enable cache hit rate monitoring.
        /// </summary>
        public bool EnableCacheHitRateMonitoring { get; set; } = true;

        /// <summary>
        /// Performance monitoring output path.
        /// </summary>
        public string OutputPath { get; set; } = "";

        /// <summary>
        /// Performance monitoring format.
        /// </summary>
        public WASMProfilingFormat Format { get; set; } = WASMProfilingFormat.JSON;
    }
}

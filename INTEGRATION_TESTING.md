# Integration Testing Plan — Holochain 0.7.0 Conductor

This document describes what would be required to add **real integration tests** that
exercise `NextGenSoftware.Holochain.HoloNET.Client` against a live Holochain 0.7.0
conductor process, as a follow-up to the unit tests in
`NextGenSoftware.Holochain.HoloNET.Client.Tests` (which only cover serialization
round-trips and do not require a conductor).

This is a plan only. No conductor install or live-process work has been attempted as
part of producing this document — see "Feasibility" at the end.

## 1. Installing the `holochain` conductor binary (0.7.0)

Three realistic options, roughly in order of CI-friendliness:

1. **Prebuilt binary from the GitHub release.** The `holochain/holochain` repo publishes
   release artifacts tagged `holochain-0.7.0`. Download the binary for the target OS/arch
   from the GitHub Releases page for that tag and put it on `PATH` (or reference its full
   path from the test fixture). This is the most CI-friendly option since it avoids a Rust
   toolchain entirely, but the artifact name/availability per-platform should be confirmed
   at https://github.com/holochain/holochain/releases when this is actually attempted.

2. **`cargo install holochain --version 0.7.0`.** Requires a working Rust toolchain
   (`rustup`) and takes several minutes to compile from source. Most portable across
   platforms (anywhere Rust can run) but slow and adds a Rust toolchain as a CI
   dependency.

3. **Nix / Holonix dev shell.** The Holochain project publishes a `flake.nix` /
   Holonix shell that pins all of `holochain`, `hc` (the dev CLI), and `lair-keystore`
   to compatible versions for a given release. This is the approach the Holochain
   project itself uses for its own CI and is the most reliable way to get a *known-good,
   version-matched* set of binaries (avoids subtle incompatibilities between a manually
   acquired `holochain` binary and `lair-keystore`/`hc` versions). Requires Nix installed
   on the CI runner.

Whichever method is used, two binaries are actually required at runtime:
- `holochain` (the conductor itself)
- `lair-keystore` (the keystore process `holochain` shells out to / connects to —
  see `KeystoreConfig.cs` in this repo for the `LairServerInProc`/`LairServer` modes
  HoloNET's config models already (not yet wired to a real conductor launch)).

## 2. Minimal test hApp / DNA

A real `.happ` bundle is needed to install via the Admin API. Options:
- Use Holochain's own `hello_world` or `dummy` example hApp from the `holochain/holochain`
  repo's `crates/test_utils/wasm` fixtures, if pre-built `.happ`/`.wasm` artifacts are
  published — these are intentionally trivial (e.g. a single zome with a no-op or
  echo function) and are what Holochain's own integration tests use.
- Alternatively, build a minimal custom DNA with `hc` (the Holochain dev CLI, included
  in the Holonix shell) from a one-zome scaffold (`hc scaffold` can generate a trivial
  zome). This requires the Rust + WASM toolchain (`cargo build --release --target
  wasm32-unknown-unknown`) to compile the zome, then `hc dna pack` / `hc app pack` to
  bundle it.
- Either way, the resulting `.happ` file path needs to be checked into the test repo
  (as a small binary fixture) or built as a pre-test step, not downloaded at test-run
  time from an untrusted source.

## 3. Starting the conductor with a known admin port

A minimal sandbox conductor config (`conductor-config.yaml` or generated via `hc sandbox`)
needs:
- `admin_interfaces` with a `websocket.port` set to a known or dynamically-allocated
  port (port 0 lets the OS pick a free port, but then the actual bound port must be
  read back from the conductor's stdout/log — `hc sandbox` prints this).
- A `data_root_path` pointed at a temp directory per test run (so test runs don't
  collide or leave state behind).
- A `network` block — for a fully offline/local-only integration test, the
  `bootstrap_url`/`signal_url` can point at a local mock or simply rely on
  single-node operation if the test doesn't need real peer-to-peer gossip.

The easiest path is actually `hc sandbox generate` + `hc sandbox run`, which handles
conductor-config generation, lair-keystore setup, and admin port assignment
automatically — wrapping this CLI rather than hand-rolling a YAML config is the
pragmatic choice for a first integration-test pass.

## 4. CI-friendly xUnit integration test harness shape

A `[CollectionDefinition]`/`IAsyncLifetime`-based xUnit fixture, conceptually:

```csharp
public class HolochainConductorFixture : IAsyncLifetime
{
    private Process _conductorProcess;
    public int AdminPort { get; private set; }
    public string InstalledAppId { get; } = "integration-test-app";

    public async Task InitializeAsync()
    {
        // 1. Locate the holochain/lair-keystore/hc binaries (env var or PATH).
        // 2. Run `hc sandbox generate --directory <tempdir> <path-to-happ>` to create
        //    a sandboxed conductor + install the test hApp in one step, OR run
        //    `hc sandbox create` then issue InstallAppAsync via HoloNETClientAdmin
        //    after the conductor is up (closer to what real HoloNET consumers do).
        // 3. Launch `holochain` (or `hc sandbox run`) as a child Process, redirecting
        //    stdout/stderr to a buffer.
        // 4. Parse stdout for the actual bound admin websocket port (sandbox tooling
        //    prints "Listening on port: N" or similar - exact string must be confirmed
        //    against the real 0.7.0 CLI output when this is implemented).
        // 5. Poll/retry connecting a HoloNETClientAdmin instance to
        //    ws://127.0.0.1:{port} until it succeeds or a timeout elapses (the
        //    conductor takes a moment to bind the socket after process start).
    }

    public async Task DisposeAsync()
    {
        // Gracefully send SIGTERM/CloseMainWindow, then kill if it doesn't exit within
        // a few seconds. Clean up the temp data_root_path directory.
        _conductorProcess?.Kill(entireProcessTree: true);
    }
}

[CollectionDefinition("HolochainConductor")]
public class HolochainConductorCollection : ICollectionFixture<HolochainConductorFixture> { }

[Collection("HolochainConductor")]
public class AdminApiIntegrationTests
{
    private readonly HolochainConductorFixture _fixture;
    public AdminApiIntegrationTests(HolochainConductorFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GenerateAgentPubKeyAsync_ReturnsValidKey() { /* connect, call, assert */ }

    [Fact]
    public async Task InstallApp_Enable_AttachAppInterface_FullFlow_Succeeds() { /* ... */ }

    [Fact]
    public async Task DumpNetworkStatsAsync_ReturnsParsedResponse() { /* exercises the new
        typed DumpNetworkStatsResponse added in the 0.7.0 upgrade against a real conductor */ }
}
```

Key properties this harness needs that the unit tests deliberately don't exercise:
- **Process lifecycle management** — starting/stopping a real OS process per test
  collection (not per test, to keep runtime reasonable), with robust cleanup even on
  test failure/cancellation (xUnit's `IAsyncLifetime.DisposeAsync` plus a
  `try`/`finally` `Kill(entireProcessTree: true)`).
- **Readiness polling** — the admin websocket isn't immediately available the instant
  the process starts; tests need a retry-with-backoff connect loop rather than a fixed
  sleep.
- **Test isolation** — a fresh `data_root_path` (temp directory) per test run so state
  from one run doesn't leak into the next; delete it in teardown.
- **CI environment gating** — these tests should be skippable (e.g. an xUnit
  `[Trait("Category", "Integration")]` + a CI flag, or checking for the `holochain`
  binary on `PATH` in a static `[Fact(Skip = ...)]` condition) so that `dotnet test`
  in normal dev/CI runs (which only have the binary-free unit test project) doesn't
  fail trying to launch a conductor that isn't installed.

## 5. What this would actually validate that unit tests can't

- That the real conductor accepts HoloNET's exact wire bytes for `ZomeCallParamsSigned`,
  the new Admin/App request types, etc. — the unit tests verify *internal consistency*
  (HoloNET serializes and deserializes its own types correctly) but cannot prove the
  *real* Holochain 0.7.0 conductor accepts those exact bytes without a live round trip.
- Real signature verification (a real conductor-generated `AgentPubKey` + a real
  Ed25519 signature over the real `ZomeCallParams` bytes) versus the unit tests' use of
  arbitrary placeholder byte arrays.
- Real error responses from the conductor for the new 0.7.0-only Admin/App calls (e.g.
  whether `GetCompatibleCells` is actually enabled by default or gated behind a feature
  flag in a given build — this was flagged as uncertain during the original upgrade).

## Feasibility note

I have **not** attempted to install or run a Holochain conductor as part of producing
this plan, per instruction. Realistically:

- Downloading a prebuilt 0.7.0 binary or running `cargo install holochain --version
  0.7.0` requires outbound internet access and (for the cargo path) a multi-minute
  build; both are plausible to attempt with the tools available in this environment,
  but neither has been attempted here since it wasn't requested as an action — only
  the plan was.
- The Nix/Holonix path additionally requires Nix to be installed, which is a heavier,
  more deliberate environment change.
- **This is something the user should explicitly approve before it's attempted** —
  installing a multi-hundred-MB Rust toolchain or Holochain binary, or running a long
  `cargo install`, is a meaningful environment change with real time/disk cost, not a
  small step to take silently as part of a "write tests" task. If the user wants this
  done, the recommended next step is the **prebuilt-binary route** (option 1 above) as
  the fastest, lowest-footprint way to get a working conductor binary for a first
  integration-test attempt, falling back to Nix/Holonix only if version-compatibility
  issues with `lair-keystore` show up.

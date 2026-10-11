# HoloNET 4.1.0 release: remaining steps

**Status (2026-10-11):** paused, waiting on two GitHub secrets. Everything else is ready.

## Done

- Code: Holochain 0.7.0 support, verified against a live conductor (see [CHANGELOG.md](CHANGELOG.md) and [AUDIT.md](AUDIT.md)).
- Versions: Client 4.1.0; ORM **4.1.3** (NuGet already had 4.1.1 and 4.1.2 from another pipeline); HDK, HyperNET and Manager 4.1.0; HoloNET-API 4.0.0 (first NuGet publish).
- GitHub release [v4.0.0](https://github.com/holochain-open-dev/holochain-client-csharp/releases/tag/v4.0.0) backfilled at `c236a4f` to match NuGet.
- The NextGenSoftwareUK fork's `main` matches this repo's `main`.

## Where it runs

Publishing runs from **[NextGenSoftwareUK/holochain-client-csharp](https://github.com/NextGenSoftwareUK/holochain-client-csharp)**,
workflow **Publish NuGet Packages** (manual "Run workflow"), not from `holochain-open-dev`. That keeps the token
for our private submodule repos out of the shared org.

## Steps

1. **Add the NuGet API key** to the fork (from <https://www.nuget.org/account/apikeys>, with Push permission for
   `NextGenSoftware.Holochain.HoloNET.*`):
   ```bash
   gh secret set NUGET_API_KEY -R NextGenSoftwareUK/holochain-client-csharp
   ```
2. **Add a token for the private submodules.** HoloNET-ORM, HDK, HyperNET and API have been private since
   2026-08-21, so the workflow's checkout needs one. Create a fine-grained token at
   <https://github.com/settings/personal-access-tokens/new>: resource owner **NextGenSoftwareUK**, those four
   repositories, **Contents: Read-only**. Then:
   ```bash
   gh secret set PRIVATE_SUBMODULE_PAT -R NextGenSoftwareUK/holochain-client-csharp
   ```
3. **Make sure the fork is up to date** with this repo's `main`, then run the publish:
   ```bash
   gh workflow run publish-nuget.yml --ref main -R NextGenSoftwareUK/holochain-client-csharp
   ```
   The workflow uses `--skip-duplicate`, so re-running is safe.
4. **Check the packages are live on NuGet:** Client and Client.Embedded 4.1.0, ORM and ORM.Embedded 4.1.3,
   HDK/HyperNET/Manager 4.1.0, API 4.0.0.
5. **Create the v4.1.0 GitHub release** at the published commit and mark it **latest**. Release notes: summarise
   the CHANGELOG sections "Remaining app/admin APIs verified live", "App flow verified live against Holochain
   0.7.0" and "Wire-format corrections", list the breaking changes, and link the NuGet packages. No Claude
   attribution.
6. **Delete this file** once the release is out.

## Notes

- The earlier attempt to publish from `holochain-open-dev` (run 38108402563) failed for the same reason:
  `PRIVATE_SUBMODULE_PAT` doesn't exist there.
- Local `backup/pre-attribution-cleanup` branches (old pre-cleanup history, never pushed) exist in this repo,
  its four submodules and NextGenSoftware-Libraries. Delete them once you're happy with the cleanup.

# CritterCensus ONI Mod

Oxygen Not Included mod (work in progress).

## Local build setup

Build settings follow the `DuplicantSurveillance` layout.

1. Copy `CritterCensus/Local.Build.props.example` to `CritterCensus/Local.Build.props` (git-ignored).
2. Set `GameManagedPath` (`OxygenNotIncluded_Data\Managed`) and `ModInstallPath` (e.g. `...\mods\local\CritterCensus`).
   Alternatively set the environment variables `ONI_GAME_MANAGED_PATH` / `ONI_MOD_INSTALL_PATH`.
3. `dotnet build` — the DLL, pdb, `mod.yaml` and `mod_info.yaml` are copied to `ModInstallPath` after each build.

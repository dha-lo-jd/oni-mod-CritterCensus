# CritterCensus ONI Mod

Oxygen Not Included mod (work in progress).

## Local build setup

Build settings follow the `DuplicantSurveillance` layout.

1. Copy `CritterCensus/Local.Build.props.example` to `CritterCensus/Local.Build.props` (git-ignored).
2. Set `GameManagedPath` (`OxygenNotIncluded_Data\Managed`) and `ModInstallPath` (e.g. `...\mods\local\CritterCensus`).
   Alternatively set the environment variables `ONI_GAME_MANAGED_PATH` / `ONI_MOD_INSTALL_PATH`.
3. `dotnet build` — Debug builds copy the DLL, pdb, `mod.yaml`, `mod_info.yaml`, `translations/*.po` and
   `ModAssets/*.png` to `ModInstallPath`. Release builds (`dotnet build -c Release`) do not touch `ModInstallPath`;
   they create the distribution folder `release/CritterCensus` (DLL without pdb, `mod.yaml`, `mod_info.yaml`,
   `Workshop/preview.png`, translations and `ModAssets`).

# StateSmith CLI Change Log
Note that this is a change log for the CLI tool, not the StateSmith library.

They have different version numbers.

# Releases
Test/interim releases are not documented here.

---

## wip
### Change (minor)
- `run` message "No .csx scripts found to run." now white instead of red.
    - no longer considered an error as we have diagram only projects.

---

## [0.9.3]
### Fix (minor)
- `run` command for "diagram-only" projects now prints the proper path for created files.
  - https://github.com/StateSmith/StateSmith/issues/345
  - Was not entirely fixed in 0.9.2 as intended (it included current directory).

---

## [0.9.2]
### Change (minor)
- `run` command for "diagram-only" projects now prints the proper path for created files.
    - https://github.com/StateSmith/StateSmith/issues/345

### Add
- Add ability for toml config to disable simulator file generation.

---

## [0.9.1]
### Change (minor)
- `create` command now asks if .csx file is desired. Defaults to "no" based on latest diagram-only work.
- `run` command now checks if diagrams specify `SmRunnerSettings.transpilerId` using toml if `--lang` is not specified.
- PlantUML is now the default diagram type for new users.
- PlantUML and drawio templates now include toml config that is updated based on target language.

### Upgrade StateSmith library version to 0.11.1
#### Add
- Add plantuml support for `note on link`
  - https://github.com/StateSmith/StateSmith/issues/343

---

## [0.9.0]
### Add
- `run` command now supports running diagrams with `--lang` option.
    - https://github.com/StateSmith/StateSmith/issues/285
    - Supports diagram based toml settings
        - https://github.com/StateSmith/StateSmith/issues/335
- `run` command detects broken drawio.svg files
    - https://github.com/StateSmith/StateSmith/issues/341
- `run` command supports `-v` verbose mode.
    - most grey text moved to verbose mode.
    - now also prints non-matching and intentionally ignored .csx/diagram files.

### Upgrade StateSmith library version to 0.11.0
#### Add
- Add diagram based toml config for `RenderConfig` and `SmRunner.Settings`
    - Useful for plantuml as well which previously never had diagram based render config support.
    - https://github.com/StateSmith/StateSmith/issues/335
- simulator - show implicit `do` edge trigger explicitly
    - https://github.com/StateSmith/StateSmith/issues/316
- Add default expansions for variables and functions.
    - https://github.com/StateSmith/StateSmith/issues/284
- Add convenience helpers for `SmTransformer`.

#### Fix
- simulator - prevent user diagram settings that could mess up generated simulation.
    - https://github.com/StateSmith/StateSmith/issues/337

---

## [0.8.2]
### Fixed
- Fix update check showing unlisted packages when it shouldn't
    - https://github.com/StateSmith/StateSmith/issues/269

---

## [0.8.1]
### Added
- Main menu option to check for updates
- Slight message improvements (print manifest success, etc)

---

## [0.8.0]
### Fixed
- fix generated vscode launch.json file
    - https://github.com/StateSmith/StateSmith/issues/263
- fix cursor missing after CTRL+C exiting menu/wizard
    - https://github.com/StateSmith/StateSmith/issues/256
- fix typo in `create-update-info.json`
    - `LastestStateSmithLibStableVersion` -> `LatestStateSmithLibStableVersion`
### Added
- Added tool settings for checking for tool updates
    - https://github.com/StateSmith/StateSmith/issues/249

---

## [0.7.2]
### Fixed
- Fix settings directory sometimes blank on WSL2
    - https://github.com/StateSmith/StateSmith/issues/255
### Changed
- Removed `--print-storage-paths` option from `create` verb
    - It now prints storage path always.

---

## [0.7.1]
### Added
- Support for `run` command.
- Support for `setup` command.

### Fixed
- Fixed detect if written/output files changed for skip detection
    - https://github.com/StateSmith/StateSmith/issues/246

---

## [0.6.6]
### Fixed
- Fixed user settings surviving tool upgrade.
    - On windows: `C:\Users\user\AppData\Roaming\StateSmith.Cli\`
    - On unix/mac: `~/.config/StateSmith.Cli/`
    - https://github.com/StateSmith/StateSmith/issues/244
### Added
- Updated language templates with common features needed.
- Initial support for `run` command.

--- 

## [0.6.4]
### Changed (minor)
- updated to use latest StateSmith library version 0.9.12

---

## [0.6.3]
### Added
- `create` - wizard defaults to use latest ss version bundled with CLI tool.
- `run` - print message saying not ready yet.

# StateSmith CLI Change Log
Note that this is a change log for the CLI tool, not the StateSmith library.

They have different version numbers.

# Releases
Test/interim releases are not documented here.

## [0.13.0]
### Added
- `create` command - add support for `java` language
  - https://github.com/StateSmith/StateSmith/issues/393

### Upgrade StateSmith library version to [0.14.0]
#### Added
- Add `Java` transpiler (only supports algorithm `Balanced2` right now)
  - https://github.com/StateSmith/StateSmith/issues/393
- Add optimization to comment out unreachable behaviors in generated code
  - https://github.com/StateSmith/StateSmith/issues/394

---

## [0.12.2]
### Upgrade StateSmith library version to `0.13.2`
#### Added
- `Balanced2` - remove un-necessary root state exit handler.
- `Balanced2` - add protection against infinite loop in the event of memory corruption.
  - https://github.com/StateSmith/StateSmith/issues/391
- `Balanced2` - add `"First ancestor handler for this event"` comment.

---

## [0.12.1]
### Upgrade StateSmith library version to `0.13.1`
#### Added
- `Balanced2` - add optimization for single event state machines
  - Add note assuming `event_id` parameter is valid for dispatch event function.
  - https://github.com/StateSmith/StateSmith/issues/388
- `Balanced2` - smarter code gen (remove unused func type)

---

## [0.12.0]
### Upgrade StateSmith library version to `0.13.0`
#### Added
- NEW ALGORITHM: `Balanced2` - a variant of `Balanced1`. Instead of dynamically updating function pointers, `Balanced2` uses a more traditional switch/case mapping.
  - `Balanced2` is the new default algorithm.
  - `Balanced1` is still available for use.
  - https://github.com/StateSmith/StateSmith/issues/384

#### Changed
- new algorithm `Balanced2` is now the default algorithm. See above.

---

## [0.11.2]
### Upgrade StateSmith library version to `0.12.2-alpha`
#### Added
- C/C++ - Add more render config settings
  - `RenderConfig.C.HFileTopPostIncludeGuard`
  - `RenderConfig.C.HFileBottomPreIncludeGuard`
  - `RenderConfig.C.HFileBottom`
  - `RenderConfig.C.CFileBottom`
  - https://github.com/StateSmith/StateSmith/issues/385
- Console print StateSmith lib version when running: `StateSmith lib ver - X.Y.Z-tag+build`
  - https://github.com/StateSmith/StateSmith/issues/186

#### Changed (minor)
- C/C++ - moved user includes to after StateSmith includes.
  - slight tidy to top of generated files

---

## [0.11.1]
### Add
- `create` command - toml template adds `RenderConfig.C.IncludeGuardLabel` setting.
    - https://github.com/StateSmith/StateSmith/issues/112

### Upgrade StateSmith library version to 0.12.1-alpha
#### Contributors
- @diorcety
  - https://github.com/StateSmith/StateSmith/pull/376/

#### Added
- C/C++ - Add support for standard `#ifdef` include guards (@diorcety)
  - https://github.com/StateSmith/StateSmith/issues/112
- C/C++ - Add support for old/odd compilers (@diorcety)
  - allow using `int` instead of `bool` for `bool` type.
  - remove extra trailing comma in enum declarations.
  - https://github.com/StateSmith/StateSmith/pull/376/

---

## [0.11.0]
### Add
- Add new `draw.io` template with many new features.
    - allow declaring state machine without nesting children
    - https://github.com/StateSmith/StateSmith/issues/359

### Fixed (minor)
- Fix tool update check to consider alpha builds in comparison.
    - https://github.com/StateSmith/StateSmith/issues/351

### Change (minor)
- PlantUML - `create` templates modified to move initial transition below state definitions.
    - this will help new users in a few situations.
- PlantUML StateSmith file detection regex changed from `startuml\s+\w+` to `@startuml[ \t]+\S+`
    - this supports https://github.com/StateSmith/StateSmith/issues/330 and is more precise.
- `create` - use toml templates with minimal settings instead of nearly full settings.

### Upgrade StateSmith library version to 0.12.0
#### Fixed (minor)
- plantuml - allow line comments before `@startuml`
  - https://github.com/StateSmith/StateSmith/issues/352
- simulator - style $initial_state nodes as a black circle (@emmby)
  - https://github.com/StateSmith/StateSmith/issues/294
- improve exception printing & wording around propagate exceptions
  - https://github.com/StateSmith/StateSmith/issues/375
- plantuml - support character escape sequences properly
  - https://github.com/StateSmith/StateSmith/issues/369

#### Added
- draw.io - allow multiple pages.
  - https://github.com/StateSmith/StateSmith/issues/78
- draw.io - allow declaring state machine without nesting.
  - https://github.com/StateSmith/StateSmith/issues/359
- allow declaring state machine name from `{fileName}`.
  - https://github.com/StateSmith/StateSmith/issues/330
- draw.io - add parse error location in exception message with help link.
  - https://github.com/StateSmith/StateSmith/issues/353
  - https://github.com/StateSmith/StateSmith/issues/354
- simulator - always show action code (even if blank) for non-transition behaviors.
  - https://github.com/StateSmith/StateSmith/issues/355
- improve draw.io disconnected edge error info
  - https://github.com/StateSmith/StateSmith/issues/378
- plantuml - support text align escape sequences
  - https://github.com/StateSmith/StateSmith/issues/362
- grammar - allow division operator `/` in guard and action code
  - https://github.com/StateSmith/StateSmith/issues/230
- simulator - improve guard evaluation dialog
  - https://github.com/StateSmith/StateSmith/issues/381

#### Changed (minor)
- Slight change to draw.io xml parsing to allow for error location information.
  - https://github.com/StateSmith/StateSmith/issues/353

---

## [0.10.0]
### Add
- Add prebuilt binaries for Windows, Linux, and Mac. No need to install dotnet anything unless you want .csx support.
  File sizes are around 145 MB, but may be reduced to around 90 MB in the future with IL trimming.
    - `win-x64`, `win-x86`, `win-arm64`
    - `osx-x64` (minimum macOS version is 10.12 Sierra)
    - `osx-arm64`
    - `linux-x64`, `linux-arm`, `linux-arm64`
    - `linux-musl-x64`, `linux-musl-arm64`
- Add ability to run diagrams even if `dotnet-script` is not installed.
- Add `run` CLI option `--no-csx` to skip running .csx scripts.
    - Useful if `dotnet-script` is not installed.
- Various CLI improvements.
    - Add top level `--help`.
    - Add `--version` to show version of CLI tool.
    - Parse enumeration values as case insensitive.
    - List allowed enumeration values.
- Add `run` CLI options for propagating and dumping exceptions to file.
    - https://github.com/StateSmith/StateSmith/issues/348

### Change (minor)
- Print `run` message `No .csx scripts found to run.` in default color.
    - no longer considered an error as we have diagram only projects.

### Fix (minor)
- CLI now sets process return code properly for CLI argument errors.
    - Also set if `run --help` or the like is used (cli lib side effect).
- Fix some printed error messages that ran across screen for Windows.
    - terminal lib (when in Windows) needed \r to return to start of line.
- Will stop running after first diagram-only (no-csx) failure instead of continuing.
    - This is consistent with .csx script running behavior and sets return code properly.
- Fix stack traces when diagram failed to parse.

### Upgrade StateSmith library version to 0.11.2-alpha
#### Fixed
- Fix error reporting for user injected code (via .csx) that has errors in it.
  - https://github.com/StateSmith/StateSmith/issues/283
- Fix error reporting when pre-parsing diagram for settings.
- Fix stack traces when pre-parsing diagram for settings.

#### Changed (minor)
- Modified PreDiagramSettingsReader to not run validations on diagram when reading settings.
  - https://github.com/StateSmith/StateSmith/issues/349
- Slight change to error reporting for better user experience.
- Reword exception detail message to be non-csx specific.
  - https://github.com/StateSmith/StateSmith/issues/348

#### Added
- Add help URL for user StateSmith grammar mistakes.
  - https://github.com/StateSmith/StateSmith/issues/174



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

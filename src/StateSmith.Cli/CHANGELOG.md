# StateSmith CLI Change Log
Note that this is a change log for the CLI tool, not the StateSmith library.

They have different version numbers.

# Releases
Test/interim releases are not documented here.

---

## [WIP]
### Fixed
- fix generated vscode launch.json file
    https://github.com/StateSmith/StateSmith/issues/263

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

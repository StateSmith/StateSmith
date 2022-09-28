# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [WIP]
### Added
- detect yEd hidden edges https://github.com/StateSmith/StateSmith/issues/29
- Better error reporting regarding diagram edge IDs.
- Improve PlantUML parsing https://github.com/StateSmith/StateSmith/issues/21
  - `notes`, and improve parsing of malformed `skinparam` blocks.


## [0.5.6-alpha]
### Added
- Support [PlantUML input](./docs/plantuml-input.md) as an alternative to yEd input.
  Input file extension must be one of ".pu", ".puml", ".plantuml"
  https://github.com/StateSmith/StateSmith/issues/21

## [0.5.5-alpha]
### Fixed
- Fix generated comment regarding marking event as handled when not for a transition

## [0.5.4-alpha]
### Added
- Support `entry` and `exit` points https://github.com/StateSmith/StateSmith/issues/3
- Initial support for `$PARENT_ALIAS` nodes. https://github.com/StateSmith/StateSmith/issues/2
- Improve exception error messages

## [0.5.3-alpha]
### Fixed
- Fix generated comment about exiting to LCA name
- Fix `FinishCodeBlock()` when `BracesOnNewLines = false;`

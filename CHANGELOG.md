# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [WIP]
### Added
- grammar - support backtick strings and other ASCII symbols https://github.com/StateSmith/StateSmith/issues/42

## [0.5.9-alpha]
### Added
- Runner - set process exit code to -1 on failure.
- Runner - output additional exception details to `<diagram_file_path>.err.txt`.
  This is useful for generic assert like exceptions that don't yet have useful error messages.
  https://github.com/StateSmith/StateSmith/issues/38
- validation - new exception: `State machines must have exactly 1 initial state. Actual count: 0.`
- validation - helpful error messages for when state machine design is not found.
- Add Describe() methods for Behavior and Vertex.
- Add `TracingModder` class and experimental `Runner.postParentAliasValidation()` callback that allows graph modification.

### Fixed
- Root initial transition actions are now output. Had been ignored previously. Other initial transition actions were output properly. https://github.com/StateSmith/StateSmith/issues/47
- Parent to child transition now exits current child first. https://github.com/StateSmith/StateSmith/issues/46
- Parent to self transition now exits current child first. https://github.com/StateSmith/StateSmith/issues/48

---

## [0.5.7-alpha]
### Added
- detect yEd hidden edges https://github.com/StateSmith/StateSmith/issues/29
- Better error reporting regarding diagram edge IDs.
- Improve PlantUML parsing https://github.com/StateSmith/StateSmith/issues/21
  - `notes`, and improve parsing of malformed `skinparam` blocks.
- Prevent antlr4 error output to console.

### Fixed
- Now throws useful lexer stage exceptions instead of printing them and not recognizing the failure. https://github.com/StateSmith/StateSmith/issues/31 

---

## [0.5.6-alpha]
### Added
- Support [PlantUML input](./docs/plantuml-input.md) as an alternative to yEd input.
  Input file extension must be one of ".pu", ".puml", ".plantuml"
  https://github.com/StateSmith/StateSmith/issues/21

---

## [0.5.5-alpha]
### Fixed
- Fix generated comment regarding marking event as handled when not for a transition

---

## [0.5.4-alpha]
### Added
- Support `entry` and `exit` points https://github.com/StateSmith/StateSmith/issues/3
- Initial support for `$PARENT_ALIAS` nodes. https://github.com/StateSmith/StateSmith/issues/2
- Improve exception error messages

---

## [0.5.3-alpha]
### Fixed
- Fix generated comment about exiting to LCA name
- Fix `FinishCodeBlock()` when `BracesOnNewLines = false;`

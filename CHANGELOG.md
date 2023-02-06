# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## WIP
### Fixed
- draw.io - convert HTML breaks `<br>` to new line characters.
- PrefixingModder - improve regex match accuracy.

### Added
- allow defining render config in diagram
  - https://github.com/StateSmith/StateSmith/issues/23
- Add `AutoExpandedVars` functionality to make it easier to add variables.
  - https://github.com/StateSmith/StateSmith/issues/91
- Add experimental `TriggerModHelper`.

### Minor changes (probably no one effected)
- Renamed new class `DefaultSmTransformer` to `StandardSmTransformer`.
- Simplified transformer step registration.
- Renamed `$cmd` to `$mod` to better reflect the intent. These are commands or directives to modify the state machine.

---

## [0.7.6-alpha]
### Added
- added `SmTransformer` pipeline that allows custom user state machine transformation!
- added back ability to support designs with multiple state machines via `RunnerSettings.stateMachineName`.
- exceptions are now only dumped to file if explicitly enabled.
  - https://github.com/StateSmith/StateSmith/issues/82

### ! BREAKING-CHANGES !
- renamed namespaces that will affect user code generation scripts:
  - `using StateSmith.Compiling;` ---> `using StateSmith.SmGraph;`
  - `using StateSmith.compiling;` ---> `using StateSmith.SmGraph;`
  - `using StateSmith.output` ---> `using StateSmith.Output`

---

## [0.7.5-alpha]
### Added
- draw.io - detect when group isn't shown correctly
  - https://github.com/StateSmith/StateSmith/issues/81
- nuget package - Include documentation and source code for intellisense/debugging.
  - https://github.com/StateSmith/StateSmith/issues/80

---

## [0.7.4-alpha]
### Added
- `RunnerSettings.filePathPrintBase` to control how file paths are printed.
  - Required when C# scripts (.csx) are used.
  - https://github.com/StateSmith/StateSmith/issues/79

---

## [0.7.3-alpha]
### Added
- draw.io - add support for draw.io plugin and new drawing style where a state can have its text event handlers as a child text node instead of its label #77.
  - see `src/StateSmithTest/test-input/drawio/Design1Sm.drawio.svg` or expand section below
  - https://github.com/StateSmith/StateSmith/issues/77
  - child text must have matching style elements (in any order): `fillColor=none;gradientColor=none;strokeColor=none;resizable=0;movable=0;deletable=0;rotatable=0;`
    - this is already done if you use the plugin https://github.com/StateSmith/StateSmith-drawio-plugin

<details><summary>click here to see Design1Sm.drawio.svg</summary>

![Design1Sm.drawio.svg](src/StateSmithTest/test-input/drawio/Design1Sm.drawio.svg)
</details>

---

## [0.7.2-alpha]
### Added
- draw.io - embedded images inside of a draw.io diagram are now ignored. Issue #77.
  - images are sometimes embedded in state machine diagrams for documentation purposes.
- draw.io - treat null labels as blank instead of throwing exception. Null labels don't normally occur. Issue #77.
- improved `$notes` validations.

---

## [0.7.1-alpha]
### Added
- Add initial support for draw.io files as alternative to yEd files. https://github.com/StateSmith/StateSmith/issues/77
  - Supported file extensions:
    - `.drawio.svg` Recommended as design file is a valid svg image that can be used in markdown and other files!
    - `.drawio`
    - `.dio`

---

## [0.7.0-alpha]
### Changed
- Generated .h file now automatically includes `<stdint.h>` (required for history states).

### Added
- Add history. https://github.com/StateSmith/StateSmith/issues/63
  - Deep history functionality supported via history continue nodes.
  - plantuml supports history states and history continue nodes.
- Add `$prefix` methods.
  - Experimental feature to help with duplicate state names. https://github.com/StateSmith/StateSmith/issues/65

### Fixed
  - Pseudo state transition de-duplication now considers initial state parent incoming transition count too.
  - Improved exit handling optimization.

---

## [0.6.1-alpha]
### Added
- Improve generated code clarity by showing step 1,2,3... and other small things.
- Improve generated code clarity by removing `if (true)` for behaviors with no guard clause.
- Improve generated code by nulling `ancestor_event_handler` only when actually needed. https://github.com/StateSmith/StateSmith/issues/14
- Throw helpful exception message when duplicate state name used. Previously relied on c compiler and user to catch the problem.
```
VertexValidationException: Duplicate state name `OPTION` also used by state `Statemachine{LaserTagMenu1Sm}.State{MENUS_GROUP}.State{MAIN_MENU}.State{OPTION}`.
    Vertex
    Path: Statemachine{LaserTagMenu1Sm}.State{MENUS_GROUP}.State{MM_SHOW_INFO}.State{OPTION}
    Diagram Id: n0::n3::n2::n2
    Children count: 0
    Behaviors count: 3
    Incoming transitions count: 2
```

---

## [0.6.0-alpha]
### Fixed
- Fix parent self transition involving initial state incorrect https://github.com/StateSmith/StateSmith/issues/49
- StateSmith grammar fix for parsing 'e()'. https://github.com/StateSmith/StateSmith/issues/60

### Changed
- Transition actions are now run after states are exited (instead of before) https://github.com/StateSmith/StateSmith/issues/6
- Reserved `else` "trigger" name for `else` functionality https://github.com/StateSmith/StateSmith/issues/59

### Added
- Support `$choice` pseudo states/points https://github.com/StateSmith/StateSmith/issues/40
  - supported in PlantUML as well.
- Support multiple transitions for initial states, entry points, exit points. https://github.com/StateSmith/StateSmith/issues/40
  - Allow multiple transitions from initial state.
  - Initial state must have a default transition (always true).
  - Allow incoming transitions to initial state.
  - Allow multiple transitions from entry point.
  - Entry point must have a default transition (always true).
  - Allow incoming transitions to entry point.
  - Allow multiple transitions from exit point.
  - Exit point must have a default transition (always true).
- Allow specifying `else` on transitions https://github.com/StateSmith/StateSmith/issues/59
- Optimize pseudo state transitions to avoid code duplication.
  - Especially important for when History states are eventually implemented as the code de-duplication
  savings can rack up big there.
- Improve entry and exit point validations.
  - Detect duplicate labels in the same scope.
  - Allow exit points to target parent state. Same as a self transition on parent.
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

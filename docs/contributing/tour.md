# A brief tour of the codebase

This page describes various good starting points to look at in the code if you're trying to get started making contributions to StateSmith.

## Notable classes in StateSmith

#### Runner/[SmRunner](/src/StateSmith/Runner/SmRunner.cs)


### PlantUML 

#### Input/PlantUML/[PlantUmlWalker](/src/StateSmith/Input/PlantUML/PlantUMLWalker.cs)

#### [PlantUML.g4](/antlr/PlantUML.g4)
The grammar for parsing PlantUML. Is converted to `XXX` by `compile.sh`





## Notable classes in StateSmith.Cli

### `ss.cli run` classes

#### Run/[DiagramRunner](/src/StateSmith.Cli/Run/DiagramRunner.cs)
Processes diagrams of different formats and transpiles them into code using `SmRunner`

#### Run/[CsxRunner](/src/StateSmith.Cli/Run/CsxRunner.cs)
Processes CSX diagrams

#### Run/[RunInfoStore](/src/StateSmith.Cli/Run/RunInfoStore.cs)
Maintains state about processing between runs, for things like incremental runs.


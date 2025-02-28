# Advanced: Running StateSmith via CSX and apps

You can use the StateSmith library from within any dotnet program, including dotnet CSX files. This allows you greater flexibility in customizing StateSmith behavior.

## Generating code from CSX

Place the following in a file named `lightbulb.csx`
```c#
{% include_relative lightbulb.csx %}
```

Put a state machine in `lightbulb.puml`:
```
{% include_relative lightbulb.puml %}
```

Now run the script to generate your code:
```
% dotnet-script lightbulb.csx

StateSmith lib ver - 0.17.3+594ad33398ce411f61e512b23481f63147af5845
StateSmith Runner - Compiling file: `lightbulb.puml` (no state machine name specified).
StateSmith Runner - State machine `lightbulb` selected.
StateSmith Runner - Writing to file `lightbulb.js`
StateSmith Runner - Finished normally.
```

Alternately, you can also run the script from `ss.cli`
```
mike@mac csx % ss.cli run -h
Using settings directory: /Users/mike/Library/Application Support/StateSmith.Cli
StateSmith.Cli 0.17.5+6ec341a3c93edf598f2ca7c31f61af85857928de


── Run ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
Reading run info from: /var/folders/4k/tf_42jmj3fx85x6q81cchc1m0000gn/T/StateSmith.Cli/runinfo.2aa3a7dba341731215ffaf1695ad386d.json
Expected file missing `lightbulb.js`. Code gen needed.
Running script: `lightbulb.csx`

StateSmith lib ver - 0.17.3+594ad33398ce411f61e512b23481f63147af5845
StateSmith Runner - Compiling file: `lightbulb.puml` (no state machine name specified).
StateSmith Runner - State machine `lightbulb` selected.
StateSmith Runner - Writing to file `lightbulb.js`
StateSmith Runner - Finished normally.

Finished running scripts.

Finished running diagrams.
Run info stored in /var/folders/4k/tf_42jmj3fx85x6q81cchc1m0000gn/T/StateSmith.Cli/runinfo.2aa3a7dba341731215ffaf1695ad386d.json

```

# Using StateSmith in CSX and apps

You can use the StateSmith library from within any dotnet program, including CSX files.

## Generating code from CSX

Place the following in a file named `lightbulb.csx`
```
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



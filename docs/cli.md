---
title: Command line reference
layout: default
nav_order: 7
---

# Command Line Reference

## statesmith

The `statesmith` tool is a simple command-line tool that turns one or more state machine diagrams into generated source code in the language of your choice. It is designed for integrating with build systems and CI/CD pipelines and for light developer needs.

```
statesmith [--flags] file1 [file2 ...]

Reads one or more state machine descriptions from the input file list
and generates code in the target language that implements that
state machine.

    --help   This message.
    --lang   One or more comma separate languages, eg. "--lang=js,ts". Defauts to 'js'
             js (JavaScript)
             ts (TypeScript)
             java
             c (alias to c99)
             c99
             cpp
             python
             csharp
             svg (image-only generation)

     -o      Output directory. Defaults to same directory as the source file.

```

## ss.cli

The `ss.cli` tool is an expanded CLI that is meant for more sophisticated interactive needs. `ss.cli` can run CSX scripts, generate new project templates, watch for changes to files and automatically recompile, check for updates, etc.

```
Usage:

  run       Run StateSmith code generation.

  create    Create a new StateSmith project from template.

  setup     Set up vscode for StateSmith & csx files.


To get help for a specific verb, use the command name followed by --help
The `create` verb currently has no options and runs a wizard.


RUN

  -h, --here                Runs code generation in this directory.

  -r, --recursive           Recursive. Can't use with -i.

  -w, --watch               Watch input files for changes.

  -x, --exclude             Glob patterns to exclude

  -i, --include             Glob patterns to include. ex: `**/src/*.csx`. Can't
                            use with -r.

  -b, --rebuild             Ensures code generation is run. Ignores change
                            detection.

  -u, --up                  Searches upwards for manifest file.

  --menu                    Shows a terminal GUI with a choice menu. Don't use
                            with other options.

  --lang                    Specifies programming language for transpiler.
                            Ignored for csx files. Valid values: Default, C99,
                            Cpp, CSharp, JavaScript, Java, Python, TypeScript,
                            NotYetSet

  --no-sim-gen              Disables simulation .html file generation. Ignored
                            for csx files.

  --no-csx                  Disables running csx files (useful if dotnet-script
                            is not installed).

  --no-ask                  Prevents tool from prompting you. Good for CI/CD.

  --propagate-exceptions    Useful for troubleshooting. Exceptions will
                            propagate out of SmRunner with original stack trace
                            instead of being summarized and printed. Ignored for
                            .csx files.

  --dump-errors-to-file     Useful for troubleshooting. Exception stack traces
                            will be written to file. Ignored if
                            'propagate-exceptions' is set. Ignored for .csx
                            files.

  -v, --verbose             Enables verbose info printing.



SETUP

  --vscode-all              Set up vscode all.

  --vscode-drawio-plugin    Set up vscode drawio extension with StateSmith
                            plugin.

  --vscode-csx              Set up vscode for C# script debugging and
                            intellisense.

  -v, --verbose             Enables verbose info printing.

```
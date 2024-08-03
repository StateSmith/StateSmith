using CommandLine;
using StateSmith.Cli.Utils;
using StateSmith.Runner;
using System.Collections.Generic;

namespace StateSmith.Cli.Run;

// https://github.com/commandlineparser/commandline
// https://github.com/commandlineparser/commandline/wiki/Mutually-Exclusive-Options
// TODO find a better name
[Verb("run-lite", HelpText = Description)]
public class RunLiteOptions
{
    public const string Description = "Run StateSmith code generation.";

    [Option(HelpText = "Specifies programming language for transpiler. Ignored for csx files.")]
    public TranspilerId Lang { get; set; } = TranspilerId.NotYetSet;

    // TODO remove the need to specify --files, just assume any unprocessed args are files
    [Option("files", HelpText = "Files to process. Can be .csx or diagram files.")]
    public IEnumerable<string> Files {get;set;} //sequence

    [Option("no-sim-gen", HelpText = "Disables simulation .html file generation. Ignored for csx files.")]
    public bool NoSimGen { get; set; } = false;

    [Option('w', "watch", HelpText = "Watch for changes. Continue watching for changes after initial run and reprocess any files as they change.")]
    public bool Watch { get; set; } = false;

    [Option('v', "verbose", HelpText = "Enables verbose info printing.")]
    public bool Verbose { get; set; } = false;

    public DiagramOptions GetDiagramOptions()
    {
        return new DiagramOptions(Lang, NoSimGen);
    }

}


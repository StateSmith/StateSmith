using CommandLine;
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

    [Option(Required=true, HelpText = "Specifies programming language for transpiler. Ignored for csx files.")]
    public TranspilerId Lang { get; set; } = TranspilerId.NotYetSet;

    [Option('w', "watch", HelpText = "Watch for changes. Continue watching for changes after initial run and reprocess any files as they change.")]
    public bool Watch { get; set; } = false;

    [Option('v', "verbose", HelpText = "Enables verbose info printing.")]
    public bool Verbose { get; set; } = false;

    [Option("no-sim-gen", HelpText = "Disables simulation .html file generation. Ignored for csx files.")]
    public bool NoSimGen { get; set; } = false;

    [Value(0)]
    public IList<string> Files { get; set; } = new List<string>();


    public DiagramOptions GetDiagramOptions()
    {
        return new DiagramOptions(Lang, NoSimGen);
    }

}


using CommandLine;
using StateSmith.Runner;
using System.Collections.Generic;

namespace StateSmith.Exe;

// https://github.com/commandlineparser/commandline
// https://github.com/commandlineparser/commandline/wiki/Mutually-Exclusive-Options
public class CommandOptions
{
    public const string Description = "Run StateSmith code generation.";


    [Option(HelpText = "Specifies programming language for transpiler. Ignored for csx files.")]
    public TranspilerId Lang { get; set; } = TranspilerId.NotYetSet;

    [Option("no-sim-gen", HelpText = "Disables simulation .html file generation. Ignored for csx files.")]
    public bool NoSimGen { get; set; } = false;


    [Option('v', "verbose", HelpText = "Enables verbose info printing.")]
    public bool Verbose { get; set; } = false;

}


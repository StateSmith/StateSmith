using CommandLine;
using StateSmith.Runner;
using System.Collections.Generic;

namespace StateSmith.Exe;

// https://github.com/commandlineparser/commandline
// https://github.com/commandlineparser/commandline/wiki/Mutually-Exclusive-Options
public class ProgramOptions
{
    // TODO how to make a description appear in the output?
    public const string Description = "Run StateSmith code generation.";


    [Option(HelpText = "Specifies output language.")]
    public TranspilerId Lang { get; set; } = TranspilerId.JavaScript;

    [Option("no-sim-gen", HelpText = "Disables simulation .html file generation.")]
    public bool NoSimGen { get; set; } = false;


    [Option('v', "verbose", HelpText = "Enables verbose info printing.")]
    public bool Verbose { get; set; } = false;


  [Value(0)]
  public IEnumerable<string> Files { get; set; } = new List<string>();

}


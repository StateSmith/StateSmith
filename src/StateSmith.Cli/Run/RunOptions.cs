using CommandLine;
using StateSmith.Cli.Utils;
using StateSmith.Runner;
using System.Collections.Generic;

namespace StateSmith.Cli.Run;

// https://github.com/commandlineparser/commandline
// https://github.com/commandlineparser/commandline/wiki/Mutually-Exclusive-Options
[Verb("run", HelpText = Description)]
public class RunOptions
{
    public const string Description = "Run StateSmith code generation.";

    [Option('h', "here", HelpText = "Runs code generation in this directory.")]
    public bool Here { get; set; } = false;

    [Option('r', "recursive", SetName = "recursive", HelpText = "Recursive. Can't use with -i.")]
    public bool Recursive { get; set; } = false;

    [Option('w', "watch", HelpText = "Watch input files for changes.")]
    public bool Watch { get; set; } = false;

    [Option('x', "exclude", HelpText = "Glob patterns to exclude")]
    public IEnumerable<string> ExcludePatterns { get; set; } = new List<string>();
    // note: we don't put this in a SetName because it's not mutually exclusive with -r

    [Option('i', "include", SetName = "include", HelpText = "Glob patterns to include. ex: `**/src/*.csx`. Can't use with -r.")]
    public IEnumerable<string> IncludePatterns { get; set; } = new List<string>();

    [Option('b', "rebuild", HelpText = "Ensures code generation is run. Ignores change detection.")]
    public bool Rebuild { get; set; } = false;

    [Option('u', "up", SetName = "up", HelpText = "Searches upwards for manifest file.")]
    public bool Up { get; set; } = false;

    [Option(SetName = "menu", HelpText = "Shows a terminal GUI with a choice menu. Don't use with other options.")]
    public bool Menu { get; set; } = false;

    [Option(HelpText = "Specifies programming language for transpiler. Ignored for csx files.")]
    public TranspilerId Lang { get; set; } = TranspilerId.NotYetSet;

    [Option("no-sim-gen", HelpText = "Disables simulation .html file generation. Ignored for csx files.")]
    public bool NoSimGen { get; set; } = false;

    [Option("no-csx", HelpText = $"Disables running csx files (useful if {DotnetScriptProgram.Name} is not installed).")]
    public bool NoCsx { get; set; } = false;

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/420
    /// </summary>
    [Option("no-ask", HelpText = $"Prevents tool from prompting you. Good for CI/CD.")]
    public bool NoAsk { get; set; } = false;

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/348
    /// </summary>
    [Option("propagate-exceptions", HelpText = "Useful for troubleshooting. Exceptions will propagate out of SmRunner with original stack trace " +
        "instead of being summarized and printed. Ignored for .csx files.")]
    public bool PropagateExceptions { get; set; } = false;

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/348
    /// </summary>
    [Option("dump-errors-to-file", HelpText = "Useful for troubleshooting. Exception stack traces will be written to file. " +
        "Ignored if 'propagate-exceptions' is set. Ignored for .csx files.")]
    public bool DumpErrorsToFile { get; set; } = false;

    [Option('v', "verbose", HelpText = "Enables verbose info printing.")]
    public bool Verbose { get; set; } = false;

    [Option("config-files", HelpText = "External toml config file(s) to use for settings. Paths relative to current terminal directory. https://github.com/StateSmith/StateSmith/issues/428")]
    public IList<string> ConfigFiles { get; set; } = new List<string>();

    [Value(0, MetaName = "Optional specific files", HelpText = "Use when you want to target specific files without directory scanning.")]
    public IList<string> SpecificFiles { get; set; } = new List<string>();

    public RunHandlerOptions GetRunHandlerOptions(string currentDirectory)
    {
        return new RunHandlerOptions(currentDirectory: currentDirectory)
        {
            Verbose = Verbose,
            NoCsx = NoCsx,
            PropagateExceptions = PropagateExceptions,
            DumpErrorsToFile = DumpErrorsToFile,
            Rebuild = Rebuild,
            Watch = Watch,
            ConfigFiles = ConfigFiles,
        };
    }

    public DiagramOptions GetDiagramOptions()
    {
        return new DiagramOptions(Lang, NoSimGen);
    }

}


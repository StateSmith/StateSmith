using CommandLine;
using System.Collections.Generic;

namespace StateSmith.Cli.Run;

[Verb("run", HelpText = "Not ready yet. Runs the StateSmith code generation.")]
public class RunOptions
{
    [Option('h', "here", HelpText = "Runs ")]
    public bool Here { get; set; } = false;

    [Option('b', "rebuild", HelpText = "Ensures code generation is run. Ignores change detection.")]
    public bool Rebuild { get; set; } = false;

    [Option('u', "up", SetName = "up", HelpText = "Searches upwards for manifest file. Can't use with -p.")]
    public bool Up { get; set; } = false;

    [Option('r', "recursive", SetName = "recursive", HelpText = "Recursive. Can't use with -i.")]
    public bool Recursive { get; set; } = false;

    [Option('x', "exclude", HelpText = "Glob patterns to exclude")]
    public IEnumerable<string> ExcludePatterns { get; set; } = new List<string>();

    [Option('i', "include", SetName = "include", HelpText = "Glob patterns to include. ex: `**/src/*.csx`. Can't use with -r.")]
    public IEnumerable<string> IncludePatterns { get; set; } = new List<string>();

    [Option(SetName = "menu", HelpText = "Shows a terminal GUI with a choice menu. Don't use with other options.")]
    public bool Menu { get; set; } = false;
}

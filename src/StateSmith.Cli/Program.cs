using CommandLine;
using System;
using StateSmith.Cli.Create;
using CommandLine.Text;
using System.Diagnostics;
using StateSmith.Cli.Run;
using System.Collections.Generic;
using StateSmith.Common;
using System.IO;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("StateSmith.CliTest")]

namespace StateSmith.Cli;

/// <summary>
/// StateSmithUI create
/// </summary>
class Program
{
    // https://github.com/commandlineparser/commandline
    // https://github.com/commandlineparser/commandline/wiki/Mutually-Exclusive-Options
    // This lib isn't perfect. It isn't very strict. You can do `-cx` and it will still work (there is no `x` command). But it's good enough for now.
    // https://github.com/commandlineparser/commandline/issues/818

    [Verb("create", HelpText = "Creates a new StateSmith project from template.")]
    class CreateOptions
    {
        [Option("print-storage-paths", HelpText = "Shows the paths where data/settings is stored.")]
        public bool PrintDataSettingsPaths { get; set; }
    }

    [Verb("run", HelpText = "Not ready yet. Runs the StateSmith code generation.")]
    class RunOptions
    {
        [Option('p', "path", SetName = "normal", HelpText = "Path to manifest file or directory with manifest file. Can't use with -u.")]
        public string PathToDirOrManifest { get; set; } = "";

        [Option('b', "rebuild", HelpText = "Ensures code generation is run. Ignores change detection.")]
        public bool Rebuild { get; set; } = false;

        [Option('u', "up", SetName="up", HelpText = "Searches upwards for manifest file. Can't use with -p.")]
        public bool Up { get; set; } = false;

        [Option(SetName = "choose", HelpText = "Shows a terminal GUI with choices. Don't use with other options.")]
        public bool Choose { get; set; } = false;

        //[Option(HelpText = "Allows experimenting with run functionality. Not advised yet.")]
        //public bool AllowExperiment { get; set; } = false;
    }

    [Verb("run-csx", HelpText = "Not ready yet.")]
    class RunCsxOptions
    {
        [Option('b', "rebuild", HelpText = "Ensures code generation is run. Ignores change detection.")]
        public bool Rebuild { get; set; } = false;

        [Option('r', "recursive", SetName = "recursive", HelpText = "Recursive. Can't use with -i.")]
        public bool Recursive { get; set; } = false;

        [Option('x', "exclude-paths", HelpText = "Glob patterns to exclude")]
        public IEnumerable<string> ExcludePatterns { get; set; } = new List<string>();

        [Option('i', "include", SetName = "include", HelpText = "Glob patterns to include. ex: `**/src/*.csx`. Can't use with -r.")]
        public IEnumerable<string> IncludePatterns { get; set; } = new List<string>();
    }

    // TODOLOW - help setup vscode script intellisense
    // TODOLOW - setup vscode with StateSmith plugin for draw.io extension
    // TODOLOW - colorize drawio file

    static void Main(string[] args)
    {
        if (Debugger.IsAttached)
        {
            //new CreateUi().Run();
        }
        //args = new[] { "run-here", "--help" };

        var parserResult = Parser.Default.ParseArguments<CreateOptions, RunOptions, RunCsxOptions>(args);

        Manifest? manifest = new ManifestPersistance(Environment.CurrentDirectory).Read();

        parserResult.MapResult(
            (CreateOptions opts) =>
            {
                var createUi = new CreateUi();

                if (opts.PrintDataSettingsPaths)
                    createUi.PrintPersistencePaths();

                createUi.Run();

                return 0;
            },
            (RunOptions opts) =>
            {
                Console.WriteLine($"Not ready yet.");
                return 0;

                //var runHandler = new RunHandler(manifest, Environment.CurrentDirectory);
                //runHandler.SetForceRebuild(opts.Rebuild);
                //// FIXME!
                //return 0;
            },
            (RunCsxOptions opts) =>
            {
                Console.WriteLine($"Not ready yet.");
                return 0;
            },
            errs =>
            {
                //PrintHelp(parserResult); // already printed by the lib
                return 1;
            }
        );
    }

    private static void PrintHelp(ParserResult<object> parserResult)
    {
        var helpText = HelpText.AutoBuild(parserResult, h =>
        {
            h.AutoHelp = false;     // hides --help
            h.AutoVersion = false;  // hides --version
            return HelpText.DefaultParsingErrorsHandler(parserResult, h);
        }, e => e);
        Console.WriteLine(helpText);
    }
}

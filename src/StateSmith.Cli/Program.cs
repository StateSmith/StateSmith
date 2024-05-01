using CommandLine;
using System;
using StateSmith.Cli.Create;
using CommandLine.Text;
using System.Diagnostics;
using StateSmith.Cli.Run;
using StateSmith.Common;
using System.IO;
using Spectre.Console;

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
    public class CreateOptions
    {
        [Option("print-storage-paths", HelpText = "Shows the paths where data/settings is stored.")]
        public bool PrintDataSettingsPaths { get; set; }
    }



    // TODOLOW - help setup vscode script intellisense
    // TODOLOW - setup vscode with StateSmith plugin for draw.io extension
    // TODOLOW - colorize drawio file

    static void Main(string[] args)
    {
        IAnsiConsole _console = AnsiConsole.Console;

        if (Debugger.IsAttached)
        {
            //new CreateUi().Run();
        }
        //args = new[] { "run-here", "--help" };

        try
        {
            ParseCommandsAndRun(args, _console);
        }
        catch (Exception ex)
        {
            _console.WriteException(ex);
            Environment.ExitCode = 1;
        }
        finally
        {
            _console.WriteLine("");
            _console.WriteLine("");
        }
    }

    private static void ParseCommandsAndRun(string[] args, IAnsiConsole _console)
    {
        var parserResult = Parser.Default.ParseArguments<CreateOptions, RunOptions>(args);

        parserResult.MapResult(
            (CreateOptions opts) =>
            {
                var createUi = new CreateUi();
                createUi.SetConsole(_console);

                if (opts.PrintDataSettingsPaths)
                    createUi.PrintPersistencePaths();

                createUi.Run();

                return 0;
            },
            (RunOptions opts) =>
            {
                var runUi = new RunUi(opts);
                runUi.SetConsole(_console);
                return runUi.HandleRunCommand();
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

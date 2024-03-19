using CommandLine;
using System;
using StateSmith.Cli.Create;
using CommandLine.Text;
using System.Diagnostics;

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

    [Verb("run", HelpText = "Runs the StateSmith code generation for a diagram. Not implemented yet.")]
    class DrawioUpdateOptions
    {
        [Option('i', "input_digram", Required = true, HelpText = "The name of the diagram to generate code for.")]
        public bool InputDiagram { get; set; }
    }

    // TODOLOW - help setup vscode script intellisense
    // TODOLOW - setup vscode with StateSmith plugin for draw.io extension
    // TODOLOW - colorize drawio file

    static void Main(string[] args)
    {
        if (Debugger.IsAttached)
        {
            new CreateUi().Run();
        }

        var parserResult = Parser.Default.ParseArguments<CreateOptions, DrawioUpdateOptions>(args);

        parserResult.MapResult(
            (CreateOptions opts) =>
            {
                var createUi = new CreateUi();

                if (opts.PrintDataSettingsPaths)
                    createUi.PrintPersistencePaths();

                createUi.Run();

                return 0;
            },
            (DrawioUpdateOptions opts) =>
            {
                Console.WriteLine("Run. Not implemented yet.");
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

using CommandLine;
using System;
using StateSmith.Cli.Create;
using CommandLine.Text;
using System.Diagnostics;
using StateSmith.Cli.Run;
using Spectre.Console;
using StateSmith.Cli.Setup;
using System.Linq;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("StateSmith.CliTest")]

namespace StateSmith.Cli;

/// <summary>
/// StateSmithUI create
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        IAnsiConsole _console = AnsiConsole.Console;

        // Fix for cursor not showing after ctrl-c exiting the program
        // https://github.com/StateSmith/StateSmith/issues/256
        Console.CancelKeyPress += delegate {
            AnsiConsole.Console.Cursor.Show();
        };

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
        var parser = new Parser(settings =>
        {
            settings.HelpWriter = null;
            settings.IgnoreUnknownArguments = false;
            settings.AutoVersion = false;
            settings.AutoHelp = false;
        });

        var parserResult = parser.ParseArguments<RunOptions, CreateOptions, SetupOptions>(args);

        parserResult.MapResult(
            (RunOptions opts) =>
            {
                PrintVersionInfo(_console);
                var runUi = new RunUi(opts, _console);
                return runUi.HandleRunCommand();
            },
            (CreateOptions opts) =>
            {
                PrintVersionInfo(_console);
                var createUi = new CreateUi(_console);
                createUi.Run();
                return 0;
            },
            (SetupOptions opts) =>
            {
                PrintVersionInfo(_console);
                var ui = new SetupUi(opts, _console);
                return ui.Run();
            },
            errs =>
            {
                PrintHelp(parserResult, _console);
                if (errs.Count() == 1 && errs.First().Tag == ErrorType.NoVerbSelectedError)
                {
                    return ProvideMenu(_console);
                }
                return 1;
            }
        );
    }

    private static int ProvideMenu(IAnsiConsole _console)
    {
        _console.MarkupLine("[cyan]No command verb was specified (see above).[/]");

        const string run = RunOptions.Description;
        const string create = CreateOptions.Description;
        const string setup = SetupOptions.Description;

        string choice = _console.Prompt(new SelectionPrompt<string>()
            .Title($"What did you want to do?")
            .AddChoices(new[] {
                run,
                create,
                setup
            }));

        switch (choice)
        {
            case run:
                var runUi = new RunUi(new(), _console);
                return runUi.HandleRunCommand();

            case create:
                var createUi = new CreateUi(_console);
                createUi.Run();
                return 0;

            case setup:
                var setupUi = new SetupUi(new(), _console);
                return setupUi.Run();
        }

        return 0;
    }

    private static void PrintVersionInfo(IAnsiConsole _console)
    {
        _console.WriteLine(HeadingInfo.Default);
    }

    private static void PrintHelp(ParserResult<object> parserResult, IAnsiConsole _console)
    {
        var helpText = HelpText.AutoBuild(parserResult, h =>
        {
            h.AutoHelp = false;
            h.AutoVersion = false;
            h.Copyright = "";
            return HelpText.DefaultParsingErrorsHandler(parserResult, h);
        }, e => e);

        _console.WriteLine(helpText);
    }
}

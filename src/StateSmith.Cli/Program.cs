using CommandLine;
using System;
using StateSmith.Cli.Create;
using CommandLine.Text;
using System.Diagnostics;
using StateSmith.Cli.Run;
using Spectre.Console;
using StateSmith.Cli.Setup;
using System.Linq;
using StateSmith.Cli.Data;
using StateSmith.Cli.Utils;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("StateSmith.CliTest")]

namespace StateSmith.Cli;

/// <summary>
/// StateSmithUI create
/// </summary>
class Program
{
    IAnsiConsole _console = AnsiConsole.Console;
    DataPaths _settingsPaths;

    public Program()
    {
        _settingsPaths = new DataPaths(_console);
    }

    static void Main(string[] args)
    {
        var program = new Program();
        program.Run(args);
    }

    private void Run(string[] args)
    {
        // Fix for cursor not showing after ctrl-c exiting the program
        // https://github.com/StateSmith/StateSmith/issues/256
        Console.CancelKeyPress += delegate
        {
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

    private void ParseCommandsAndRun(string[] args, IAnsiConsole _console)
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
                PreRunNoArgError(_console);
                var runUi = new RunUi(opts, _console);
                return runUi.HandleRunCommand();
            },
            (CreateOptions opts) =>
            {
                PreRunNoArgError(_console);
                var createUi = new CreateUi(_console, _settingsPaths);
                createUi.Run();
                return 0;
            },
            (SetupOptions opts) =>
            {
                PreRunNoArgError(_console);
                var ui = new SetupUi(opts, _console);
                return ui.Run();
            },
            errs =>
            {
                PrintHelp(parserResult, _console);
                if (errs.Count() == 1 && errs.First().Tag == ErrorType.NoVerbSelectedError)
                {
                    TryCheckForUpdates();
                    return ProvideMenu();
                }
                return 1;
            }
        );
    }

    private int ProvideMenu()
    {
        UiHelper.AddSectionLeftHeader(_console, "Main Menu");
        _console.MarkupLine("[cyan]No command verb was specified (see above).[/]");

        const string run = RunOptions.Description;
        const string create = CreateOptions.Description;
        const string setup = SetupOptions.Description;
        const string checkForToolUpdate = "Check for tool update";

        string choice = _console.Prompt(new SelectionPrompt<string>()
            .Title($"What did you want to do?")
            .AddChoices(new[] {
                run,
                create,
                setup,
                checkForToolUpdate
            }));

        switch (choice)
        {
            case run:
                var runUi = new RunUi(new(), _console);
                return runUi.HandleRunCommand();

            case create:
                var createUi = new CreateUi(_console, _settingsPaths);
                createUi.Run();
                return 0;

            case setup:
                var setupUi = new SetupUi(new(), _console);
                return setupUi.Run();

            case checkForToolUpdate:
                ToolUpdateChecker updateChecker = new(_console, _settingsPaths);
                updateChecker.CheckForUpdates(pauseForKeyboardEnter: false);
                return 0;
        }

        return 0;
    }

    private void PreRunNoArgError(IAnsiConsole _console)
    {
        _console.WriteLine(HeadingInfo.Default);
        TryCheckForUpdates();
    }

    private void TryCheckForUpdates()
    {
        ToolSettingsLoader loader = new(_console, _settingsPaths);
        loader.LoadOrAskUser();

        ToolUpdateChecker updateChecker = new(_console, _settingsPaths);
        updateChecker.AskToCheckIfTime(loader.GetToolSettings());

        bool printed = loader.Printed || updateChecker.Printed;
        if (printed)
            _console.WriteLine("\n");
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

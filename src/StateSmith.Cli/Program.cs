using CommandLine;
using System;
using StateSmith.Cli.Create;
using CommandLine.Text;
using StateSmith.Cli.Run;
using Spectre.Console;
using StateSmith.Cli.Setup;
using System.Linq;
using StateSmith.Cli.Data;
using StateSmith.Cli.Utils;
using StateSmith.Cli.VersionUtils;
using StateSmith.Output;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("StateSmith.CliTest")]

namespace StateSmith.Cli;

/// <summary>
/// StateSmithUI create
/// </summary>
public class Program
{
    IAnsiConsole _console = AnsiConsole.Console;
    CliArgsParser _cliArgsParser = new();
    DataPaths _settingsPaths;
    string _currentDirectory;

    public Program(string currentDirectory)
    {
        _settingsPaths = new DataPaths(_console);
        this._currentDirectory = currentDirectory;
    }

    public static void Main(string[] args)
    {
        var program = new Program(currentDirectory: Environment.CurrentDirectory);
        program.Run(args);
    }

    public static string GetSemVersionString()
    {
        return LibVersionInfo.GetVersionInfoString(typeof(Program).Assembly);
    }

    public void Run(string[] args)
    {
        // Fix for cursor not showing after ctrl-c exiting the program
        // https://github.com/StateSmith/StateSmith/issues/256
        Console.CancelKeyPress += delegate
        {
            AnsiConsole.Console.Cursor.Show();
        };

        int resultCode;

        try
        {
            resultCode = ParseCommandsAndRun(args, _console);
        }
        catch (Exception ex)
        {
            _console.WriteException(ex);
            resultCode = 1;
        }
        finally
        {
            _console.WriteLine("");
            _console.WriteLine("");
        }

        Environment.ExitCode = resultCode;
    }

    internal int ParseCommandsAndRun(string[] args, IAnsiConsole _console)
    {
        var parserResult = _cliArgsParser.Parse(args);

        int resultCode = parserResult.MapResult(
            (RunOptions opts) =>
            {
                PreRunNoArgError(_console);
                var runUi = new RunUi(opts, _console, currentDirectory: _currentDirectory);
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
                var ui = new SetupUi(opts, _console, currentDirectory: _currentDirectory);
                return ui.Run();
            },
            errs =>
            {
                if (errs.Count() == 1 && errs.IsVersion())
                {
                    _console.WriteLine(HeadingInfo.Default);
                    return 0;
                }

                //if (errs.Count() == 1 && errs.IsHelp())   // this breaks `ss.cli run --help`
                if (args.Length == 1 && args[0] == "--help")
                {
                    _console.WriteLine(HeadingInfo.Default);
                    _console.WriteLine();
                    _console.WriteLine(_cliArgsParser.GetUsage());
                    return 0;
                }

                if (errs.Count() == 1 && errs.First().Tag == ErrorType.NoVerbSelectedError)
                {
                    _console.WriteLine(HeadingInfo.Default);
                    _console.WriteLine();
                    _console.WriteLine(_cliArgsParser.GetUsage());
                    TryCheckForUpdates();
                    return ProvideMenu();
                }

                _console.WriteLine(_cliArgsParser.GetErrorHelp(parserResult, errs));
                return 1;
            }
        );

        return resultCode;
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
                var runUi = new RunUi(new(), _console, currentDirectory: _currentDirectory);
                return runUi.HandleRunCommand();

            case create:
                var createUi = new CreateUi(_console, _settingsPaths);
                createUi.Run();
                return 0;

            case setup:
                var setupUi = new SetupUi(new(), _console, currentDirectory: _currentDirectory);
                return setupUi.Run();

            case checkForToolUpdate:
                ToolUpdateChecker updateChecker = new(_console, _settingsPaths, new ThisAssemblySemVerProvider());
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

        ToolUpdateChecker updateChecker = new(_console, _settingsPaths, new ThisAssemblySemVerProvider());
        updateChecker.AskToCheckIfTime(loader.GetToolSettings());

        bool printed = loader.Printed || updateChecker.Printed;
        if (printed)
            _console.WriteLine("\n");
    }
}

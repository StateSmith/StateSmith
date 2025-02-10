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


namespace StateSmith.Binary;

public class Program
{
    IAnsiConsole _console = AnsiConsole.Console;
    CliArgsParser _cliArgsParser = new();
    string _currentDirectory;

    public Program(string currentDirectory)
    {
        this._currentDirectory = currentDirectory;
    }

    public static void Main(string[] args)
    {
        var program = new Program(currentDirectory: Environment.CurrentDirectory);
        program.Run(args);
    }


    public void Run(string[] args)
    {

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
                PreRunNoArgError(_console, noAsk: opts.NoAsk);
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


}

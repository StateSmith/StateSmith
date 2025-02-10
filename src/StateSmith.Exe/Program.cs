using CommandLine;
using System;
using CommandLine.Text;
using Spectre.Console;
using System.Linq;
using StateSmith.Output;
using System.Buffers.Text;


namespace StateSmith.Exe;

public class Program
{
    static IAnsiConsole _console = AnsiConsole.Console;
    
    public Program()
    {
    }

    public static void Main(string[] args)
    {        
        if (args.Length == 0)
          args = new [] { "--help" };

        try
        {
            var program = new Program();
            var parseResult = Program.ParseCommands(args, Program._console, program);
            if (parseResult != 0)
            {
                Environment.ExitCode = parseResult;
                return;
            }
            program.Run();
            Environment.ExitCode = 0;
        }
        catch (Exception ex)
        {
            _console.WriteException(ex);
            Environment.ExitCode = 1;
        }
    }


    public void Run()
    {

    }

    static internal int ParseCommands(string[] args, IAnsiConsole _console, Program program)
    {
        var parser = new Parser(settings =>
        {
            settings.HelpWriter = null;
            settings.IgnoreUnknownArguments = false;
            settings.AutoVersion = true;
            settings.AutoHelp = false;
            settings.CaseInsensitiveEnumValues = true;
        });

        var parserResult = parser.ParseArguments<CommandOptions>(args);

        int resultCode = parserResult.MapResult(
            (CommandOptions opts) =>
            {
                // TODO set program parameters
                return 0;
            },
            errs =>
            {
                PrintUsage( parserResult, _console);
                return 1;
            }
        );

        return resultCode;
    }


    static internal void PrintUsage( ParserResult<CommandOptions> parserResult, IAnsiConsole console)
    {
        console.WriteLine(HelpText.AutoBuild(parserResult));
    }
}

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
    internal static IAnsiConsole _console = AnsiConsole.Console;
    
    internal ProgramOptions _options  = new ProgramOptions();

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
            var parseResult = program.ParseCommands(args, Program._console);
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
        new DiagramRunner(_console,_options).Run(_options.Files.ToList());
    }

    internal int ParseCommands(string[] args, IAnsiConsole _console)
    {
        var parser = new Parser(settings =>
        {
            settings.HelpWriter = null;
            settings.IgnoreUnknownArguments = false;
            settings.AutoVersion = true;
            settings.AutoHelp = false;
            settings.CaseInsensitiveEnumValues = true;
        });

        var parserResult = parser.ParseArguments<ProgramOptions>(args);

        int resultCode = parserResult.MapResult(
            (ProgramOptions opts) =>
            {
                _options = opts; // set the options in the program
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


    static internal void PrintUsage( ParserResult<ProgramOptions> parserResult, IAnsiConsole console)
    {
        console.WriteLine(HelpText.AutoBuild(parserResult, h=> {
            h.Heading = "\nStateSmith - a state machine diagram tool.\nUsage: statesmith [options] file1 [file2...]\n";
            return h;
        }, e=>e));
    }
}

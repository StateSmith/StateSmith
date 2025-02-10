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
        try
        {
            var program = new Program();
            var parseResult = Program.ParseCommands(args, Program._console, program);
            program.Run();
            Environment.ExitCode = parseResult;
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
                PrintUsage(_console);
                return 1;
            }
        );

        return resultCode;
    }


    static internal void PrintUsage(IAnsiConsole console)
    {
        // TODO
        // var helpText = new HelpText
        // {
        //     Heading = new Heading("Usage:", new Style(foreground: Color.Green)),
        //     AdditionalNewLineAfterOption = false,
        //     AddDashesToOption = true
        // };
        // helpText.AddOptions(new CommandOptions());
        // console.WriteLine(helpText);
    }
}

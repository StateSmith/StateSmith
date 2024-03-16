using CommandLine;
using System;
using StateSmithCli.Create;
using CommandLine.Text;

namespace StateSmithCli.Cli;



/// <summary>
/// StateSmithUI --create
/// </summary>
class StateSmithCli
{
    // https://github.com/commandlineparser/commandline
    // https://github.com/commandlineparser/commandline/wiki/Mutually-Exclusive-Options
    // This lib isn't perfect. It isn't very strict. You can do `-cx` and it will still work (there is no `x` command). But it's good enough for now.
    // https://github.com/commandlineparser/commandline/issues/818

    [Verb("create", HelpText = "Creates a new StateSmith project from template.")]
    class CreateOptions
    {
        //[Option('c', "create", HelpText = "Creates a new StateSmith project from template.")]
        //public bool Create { get; set; }
    }

    [Verb("run", HelpText = "Runs the StateSmith project.")]
    class DrawioUpdateOptions
    {
        [Option("fileName", Required = true, HelpText = "The name of the file to run.")]
        public bool Run { get; set; }
    }


    static void Main(string[] args)
    {
        var parserResult = Parser.Default.ParseArguments<CreateOptions, DrawioUpdateOptions>(args);

        parserResult.MapResult( 
            (CreateOptions opts) => {
                new CreateUi().Run();
                return 0;
            },
            (DrawioUpdateOptions opts) =>
            {
                Console.WriteLine("Run");
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

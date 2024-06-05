using CommandLine;
using StateSmith.Cli.Create;
using StateSmith.Cli.Run;
using StateSmith.Cli.Setup;

namespace StateSmith.Cli;

public class CliArgsParser
{
    public ParserResult<object> Parse(string[] args)
    {
        var parser = new Parser(settings =>
        {
            settings.HelpWriter = null;
            settings.IgnoreUnknownArguments = false;
            settings.AutoVersion = false;
            settings.AutoHelp = false;
        });

        var parserResult = parser.ParseArguments<RunOptions, CreateOptions, SetupOptions>(args);
        return parserResult;
    }
}

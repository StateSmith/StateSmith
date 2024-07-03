using CommandLine;
using CommandLine.Text;
using Spectre.Console;
using StateSmith.Cli.Create;
using StateSmith.Cli.Run;
using StateSmith.Cli.Setup;
using System.Collections.Generic;
using System.Linq;

namespace StateSmith.Cli;

/// <summary>
/// The CLI arguments parser library is really helpful, but gets awkward when command verbs are used.
/// We may switch at some point.
/// 
/// When verbs are involved `--help` and `--version` are treated as errors. You can't mix verbs and global options.
/// Try not to change too much here as it requires lots of manual testing (we should add unit tests...).
/// </summary>
public class CliArgsParser
{
    public ParserResult<object> Parse(string[] args)
    {
        var parser = new Parser(settings =>
        {
            settings.HelpWriter = null;
            settings.IgnoreUnknownArguments = false;
            settings.AutoVersion = true;
            settings.AutoHelp = false;
            settings.CaseInsensitiveEnumValues = true;
        });

        var parserResult = parser.ParseArguments<RunOptions, CreateOptions, SetupOptions>(args);
        return parserResult;
    }

    public string GetUsage()
    {
        var parserResult = Parse(new string[] { });

        var helpText = HelpText.AutoBuild(parserResult, h =>
        {
            h.AutoHelp = false;
            h.AutoVersion = false;
            h.Copyright = "";
            h.Heading = "Usage:";
            return h;
        }, e => e,
        verbsIndex: true);

        helpText.AddPostOptionsText("To get help for a specific verb, use the command name followed by --help");
        helpText.AddPostOptionsText("The `create` verb currently has no options and runs a wizard.");

        return helpText.ToString();
    }

    /// <summary>
    /// This also provides help for specific commands like `ss.cli run --help`.
    /// </summary>
    /// <param name="parserResult"></param>
    /// <returns></returns>
    public string GetErrorHelp(ParserResult<object> parserResult, IEnumerable<Error> errs)
    {
        var helpText = HelpText.AutoBuild(parserResult, h =>
        {
            h.AutoHelp = false;
            h.AutoVersion = false;
            h.Copyright = "";
            h.AddEnumValuesToHelpText = true;
            return HelpText.DefaultParsingErrorsHandler(parserResult, h);
        }, e => e);

        var str = helpText.ToString();

        if (errs.Count() == 1 && errs.First().Tag == ErrorType.BadVerbSelectedError)
        {
            str += "\n\n" + GetUsage();
        }

        return str.ToString();
    }
}

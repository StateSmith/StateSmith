using System.Text;
using System.Text.RegularExpressions;

#nullable enable

namespace StateSmith.Output.Gil;

/// <summary>
/// Getting the whitespace right when transpiling to C is hard.
/// This class helps.
/// </summary>
public class PostProcessor
{
    public const string echoLineMarker = "//>>>>>ECHO:";
    public const string trimBlankLinesMarker = ">>>>>>>>trimBlankLinesMarker<<<<<<<<<<<<<<";
    public const string trimHorizontalWhiteSpaceMarker = ">>>>>>>>trimHorizontalWhiteSpaceMarker<<<<<<<<<<<<<<";
    public const string rmLeft2Marker = "<<<<<rm2<<<<<";
    public const string rmRight2Marker = ">>>>>rm2>>>>>";
    public const string rmIdentifierStart = "____rmIdentifierStart____";
    public const string dummy = "____rmDummy____";

    private static readonly Regex trimBlankLinesRegex;
    private static readonly Regex trimHwsRegex;
    private static readonly Regex rmLeft2Regex;
    private static readonly Regex rmRight2Regex;
    private static readonly Regex rmIdentifierStartRegex;

    static PostProcessor()
    {
        var eolPattern = @"(?:\r\n|\r|\n)";
        var anyPattern = @"[\s\S]";

        trimBlankLinesRegex = new Regex(@$"(?xm)
            ^ \s* {trimBlankLinesMarker} .* {eolPattern}
            (?:
                ^ [ \t]* {eolPattern}
            )*
        ");

        trimHwsRegex = new Regex(@$"(?xm)
            [ \t]* {trimHorizontalWhiteSpaceMarker} [ \t]*
        ");

        rmIdentifierStartRegex = new Regex(@$"(?xm)
            \b \w* {rmIdentifierStart} (\w+) \b
        ");

        rmLeft2Regex = new Regex(@$"(?x) {anyPattern} {anyPattern} {rmLeft2Marker} ");
        rmRight2Regex = new Regex(@$"(?x) {rmRight2Marker} {anyPattern} {anyPattern}");
    }

    public static string RmCommentStart => $"/*{PostProcessor.rmLeft2Marker}";
    public static string RmCommentEnd => $"{PostProcessor.rmRight2Marker}*/";

    public static string RmCommentOut(string code)
    {
        code = code.Replace("*/", $"*{rmRight2Marker}##/");

        return $"{RmCommentStart}{code}{RmCommentEnd}";
    }

    public static string PostProcess(string str)
    {
        str = trimBlankLinesRegex.Replace(str, "");
        str = trimHwsRegex.Replace(str, "");
        str = rmLeft2Regex.Replace(str, "");
        str = rmRight2Regex.Replace(str, "");
        str = rmIdentifierStartRegex.Replace(str, "$1");
        str = str.Replace(echoLineMarker, "");
        str = str.Replace(dummy, "");

        return str;
    }

    public static void PostProcess(StringBuilder sb)
    {
        var input = sb.ToString();
        sb.Clear();
        var output = PostProcess(input);
        sb.Append(output);
    }
}

#nullable enable

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StateSmith.Output;

// todo_low - move to StateSmith.Common namespace

public class StringUtils
{
    public static string DeIndent(string str)
    {
        var match = Regex.Match(str, @"^\s*?([ \t]+)\S");

        if (!match.Success)
        {
            return str;
        }

        var indent = match.Groups[1];

        var r = new Regex("^" + indent, RegexOptions.Multiline);

        var output = r.Replace(str, "");

        return output;
    }

    public static string RemoveAnyIndent(string str)
    {
        var r = new Regex(@"^[ \t]+", RegexOptions.Multiline);

        var output = r.Replace(str, "");

        return output;
    }

    public static string Indent(string str, string indent)
    {
        var result = new Regex("^", RegexOptions.Multiline);
        var output = result.Replace(str, indent);
        return output;
    }

    public static string[] SplitIntoLinesOrEmpty(string str)
    {
        var lines = Regex.Split(str, @"\r\n|\r|\n");    // TODOLOW compile common regex

        if (lines.Length == 1 && lines[0] == String.Empty)
            return Array.Empty<string>();

        return lines;
    }

    public static string ReplaceNewLineChars(string str, string replacment)
    {
        // TODOLOW compile common regex
        return Regex.Replace(str, @"\r\n|\r|\n", replacment);
    }

    public static string ConvertToSlashNLines(string str)
    {
        // TODOLOW compile common regex
        return Regex.Replace(str, @"\r\n|\r", "\n");
    }

    public static string DeIndentTrim(string str)
    {
        var output = DeIndent(str);
        output = TrimLeadingHsAndFirstNewline(output);
        output = TrimTrailingNewLineAndHs(output);

        return output;
    }

    public static string RemoveAnyIndentAndTrim(string str)
    {
        var output = RemoveAnyIndent(str);
        output = TrimLeadingHsAndFirstNewline(output);
        output = TrimTrailingNewLineAndHs(output);

        return output;
    }

    public static string FindIndent(string str)
    {
        var regex = new Regex(@"(?m)^([ \t]+)\z"); // \z is end of string
        string indent = regex.Match(str).Groups[1].Value;

        return indent;
    }

    /// <summary>
    /// See unit tests for examples. If no indent is found, returns empty string.
    /// </summary>
    /// <param name="sb"></param>
    /// <returns></returns>
    public static string FindLastIndent(StringBuilder sb)
    {
        int endExclusive = sb.Length;
        int start = sb.Length - 1;

        // walk backwards from end of string
        while (start >= 0)
        {
            char c = sb[start];
            if (c == ' ' || c == '\t')
            {
                start--;
            }
            else if (c == '\n' || c == '\r')
            {
                break;
            }
            else
            {
                endExclusive = start;
                start--;
            }
        }

        start++;

        if (start == endExclusive)
        {
            return "";
        }

        return sb.ToString(start, endExclusive - start);
    }

    /// <summary>
    /// Trims trailing new line and horizontal white space
    /// </summary>
    /// <param name="output"></param>
    /// <returns></returns>
    private static string TrimTrailingNewLineAndHs(string output)
    {
        output = Regex.Replace(output, @"(\r\n|\r|\n)[ \t]*$", "");
        return output;
    }

    /// <summary>
    /// Trims leading horizontal space and first new line
    /// </summary>
    /// <param name="output"></param>
    /// <returns></returns>
    private static string TrimLeadingHsAndFirstNewline(string output)
    {
        output = Regex.Replace(output, @"^[ \t]*(\r\n|\r|\n)", "");
        return output;
    }

    public static string EscapeCharsForString(string str)
    {
        if (str == null) return null;

        str = StringUtils.ReplaceNewLineChars(str, "\\n");
        str = Regex.Replace(str, @"(\\|"")", "\\$1");
        return str;
    }

    internal static string RemoveEverythingBefore(string str, string match, bool requireMatch = false)
    {
        var index = str.LastIndexOf(match);
        if (index < 0)
        {
            if (requireMatch)
                throw new ArgumentException($"match `{match}` not found");
            else
                return str;
        }

        return str.Substring(index + match.Length);
    }

    internal static string MaybeTruncateWithEllipsis(string str, int maxLength)
    {
        if (str.Length > maxLength)
        {
            return str.Substring(0, maxLength) + "…";
        }

        return str;
    }

    internal static string RemoveCCodeComments(string code, bool keepLineEnding = false)
    {
        var regex = new Regex(@"(?x)
                // .* (?<lineEnding> \r\n | \r | \n | $ )
                |
                /[*]
                [\s\S]*? # anything, lazy
                [*]/
            ");

        var result = regex.Replace(code, (m) => {
            if (keepLineEnding)
                return m.Groups["lineEnding"].Value;
            return "";
        });

        return result;
    }

    internal static string RemoveJsStrings(string code)
    {
        var regex = new Regex(@"(?x)
                # double quoted string
                "" (?: \\. | [^""] )* ""
                |
                # single quoted string
                ' (?: \\. | [^'] )* '
                |
                # template string
                ` (?: \\. | [^`] )* `
            ");

        var result = regex.Replace(code, "");
        return result;
    }

    internal static string RemovePythonStringsAndComments(string code)
    {
        // have to match all elements that can affect each other at the same time.
        // For example, a string literal can contain a comment, and a comment can contain a string literal.
        var regex = new Regex(@"(?x)
                # string literal
                ""
                (?:
                    \\. # escape
                    |
                    [\s\S] *? # anything, lazy
                )
                ""
                |
                """""" # triple double quotes
                [\s\S] *? # anything, lazy
                """"""
                |
                \# [^\r\n]*
            ");

        var result = regex.Replace(code, "");
        return result;
    }



    internal static string AppendWithNewlineIfNeeded(string str, string toAppend)
    {
        AppendInPlaceWithNewlineIfNeeded(ref str, toAppend);
        return str;
    }

    internal static void AppendInPlaceWithNewlineIfNeeded(ref string str, string toAppend)
    {
        if (str.Length > 0 && toAppend.Length > 0)
        {
            str += "\n";
        }

        str += toAppend;
    }

    public static string SnakeCaseToCamelCase(string snakeCaseName)
    {
        var regex = new Regex(@"(?x) _ (?<letterAfterUnderscore> [a-z] ) ");
        var newName = regex.Replace(snakeCaseName, (Match m) => m.Groups["letterAfterUnderscore"].Value.ToUpper());

        return newName;
    }

    public static string SnakeCaseToPascalCase(string snakeCaseName)
    {
        var regex = new Regex(@"(?x)
        ( ^ \s* | _ ) # either start of input or underscore
        (?<letterToUpperCase> [a-zA-Z] ) ");
        var newName = regex.Replace(snakeCaseName, (Match m) => m.Groups["letterToUpperCase"].Value.ToUpper());

        return newName;
    }

    public static string ReadLineXFromString(int lineNumber, string input)
    {
        using var tempReader = new StringReader(input);

        for (int i = 0; i < lineNumber - 1; i++)
        {
            tempReader.ReadLine();
        }

        var lineContents = tempReader.ReadLine() ?? $"<failed reading line {lineNumber} from string>";
        return lineContents;
    }

    /// <summary>
    /// Line numbers are 1-based. Trimmed message subject to change.
    /// </summary>
    /// <returns></returns>
    public static string BackTickQuoteLimitedString(string str, int maxLineLength = 100)
    {
        if (str.Length > maxLineLength)
        {
            str = str[..maxLineLength];
            str = $"`{str}` (trimmed to {maxLineLength} chars)";
        }
        else
        {
            str = $"`{str}`";
        }

        return str;
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace StateSmith.Output
{
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

        public static string[] SplitIntoLines(string str)
        {
            var lines = Regex.Split(str, @"\r\n|\r|\n");    // TODOLOW compile common regex
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

        internal static string EscapeCharsForString(string str)
        {
            if (str == null) return null;

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

        internal static string RemoveCCodeComments(string code)
        {
            var regex = new Regex(@"(?x)
                // .* [\r\n]+
                |
                /[*]
                [\s\S]*? # anything, lazy
                [*]/
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
    }
}

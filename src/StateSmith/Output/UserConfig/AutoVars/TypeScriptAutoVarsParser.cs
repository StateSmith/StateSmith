#nullable enable

using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace StateSmith.Output.UserConfig.AutoVars;

/// <summary>
/// TypeScript auto vars parser. TypeScript is super flexible with its types, so this parser is a bit more lenient.
/// NOTE! We require fields to end with a semicolon.
/// </summary>
public class TypeScriptAutoVarsParser : IAutoVarsParser
{
    public List<string> ParseIdentifiers(string code)
    {
        var result = new List<string>();

        // todolow - need to remove comments and strings at the same time as they can interfere with each other.
        code = StringUtils.RemoveCCodeComments(code);
        code = StringUtils.RemoveJsStrings(code);

        // support formats like:
        //      public count: number = 0;
        //      count2;
        //      count3;
        var regex = new Regex(@"(?x)
            (?:
                \s*
                \w*? # optional access modifier like `public`
                \s*
                (?<identifier>  [_a-zA-Z] \w* )  # must start with a letter or underscore

                (?: # optional type
                    \s*
                    :
                    \s*
                    [^;=]*?
                )?

                (?: # optional initial value
                    \s*
                    =
                    \s*
                    [^;=]+?
                )?
                \s*
                ; # ender
            )+
        ");

        foreach (Match match in regex.Matches(code).Cast<Match>())
        {
            result.AddRange(match.Groups["identifier"].Captures.Select(c => c.Value));
        }

        return result;
    }
}

// earlier attempt at regex
//var regex = new Regex(@"(?x)
//            (?:
//                \s*
//                \w*? # optional access modifier like `public`
//                \s*
//                (?<identifier>  [_a-zA-Z]+ )

//                (?: # optional type
//                    \s*
//                    :
//                    .*? \w .*?
//                )?
//                (?: # optional initial value
//                    \s*
//                    =
//                    \s*
//                    (?:
//                        # double quoted string
//                        "" (?: \\. | [^""] )* ""
//                        |
//                        # single quoted string
//                        ' (?: \\. | [^'] )* '
//                        |
//                        # template string
//                        ` (?: \\. | [^`] )* `
//                        |
//                        [^;]*?
//                    )
//                )?
//                [ \t]*
//                (?: # ender
//                    [;] | [\r\n]+ | $
//                )
//            )+
//        ");

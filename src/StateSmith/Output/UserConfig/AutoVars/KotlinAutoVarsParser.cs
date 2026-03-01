#nullable enable

using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace StateSmith.Output.UserConfig.AutoVars;

public class KotlinAutoVarsParser : IAutoVarsParser
{
    public List<string> ParseIdentifiers(string code)
    {
        var result = new List<string>();

        code = StringUtils.RemoveCCodeComments(code);

        // support formats like:
        //      var count: Int = 0
        //      var text = ""
        //      var last: Int?
        var regex = new Regex(@"(?x)
            (?:
                \s*

                var
                \s+
                (?<identifier>  [_a-zA-Z]  \w* )

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
            )+
        ");

        foreach (Match match in regex.Matches(code).Cast<Match>())
        {
            result.AddRange(match.Groups["identifier"].Captures.Select(c => c.Value));
        }

        return result;
    }
}

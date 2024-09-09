#nullable enable

using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace StateSmith.Output.UserConfig;

public class PythonAutoVarsParser : IAutoVarsParser
{
    public List<string> ParseIdentifiers(string code)
    {
        var result = new List<string>();

        code = StringUtils.RemovePythonStringsAndComments(code);

        // require format like: `self.some_var`
        var regex = new Regex(@"(?x)
            (?:
                \s*
                self[.]
                (?<identifier>  [_a-zA-Z]  \w* )
            )+
        ");

        foreach (Match match in regex.Matches(code).Cast<Match>())
        {
            result.AddRange(match.Groups["identifier"].Captures.Select(c => c.Value));
        }

        return result;
    }
}

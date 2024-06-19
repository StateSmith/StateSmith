#nullable enable

using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace StateSmith.Output.UserConfig;

public class JsAutoVarsParser : IAutoVarsParser
{
    public List<string> ParseIdentifiers(string code)
    {
        var result = new List<string>();

        code = StringUtils.RemoveCCodeComments(code);

        if (code.Contains('{') || code.Contains('}'))
        {
            throw new ArgumentException($"Detected '{{' or '}}' in {nameof(RenderConfigBaseVars.AutoExpandedVars)} section. That section parsing is currently too dumb to handle anonymous structures." +
                "You'll have to use the regular variables and expansions fields of the render config. https://github.com/StateSmith/StateSmith/issues/91 .");
        }

        // require format like: `count : undefined,`
        var regex = new Regex(@"(?x)
            (?:
                \s*
                (?<identifier>  [_a-zA-Z]  \w* )
                \s*
                [:]
                [^,]+
                (?:
                    $ | [,]
                )
            )+
        ");

        foreach (Match match in regex.Matches(code).Cast<Match>())
        {
            result.AddRange(match.Groups["identifier"].Captures.Select(c => c.Value));
        }

        return result;
    }
}

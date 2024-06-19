#nullable enable

using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace StateSmith.Output.UserConfig;

public class CLikeAutoVarsParser : IAutoVarsParser
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

        // see it here: https://www.debuggex.com/r/KenQI-k9hRYdksb5
        var regex = new Regex(@"(?x)
            [^;]+?
            \b
            (?<identifier>  [_a-zA-Z]  \w* )
            (?: # multiple variables on one line
                \s*
                ,
                [*\s]* # could have leading space or pointer asterisks
                (?<identifier>  [_a-zA-Z]  \w* )
                \s*
                # array of any dimension
                (?:
                    \[ [^]]+ \] \s*
                )*
            )*
            \s*

            (?:
                (?:
                    __ \w+ # double underscore for things like GCC's `__attribute__`
                    |
                    [[] # for array
                    |
                    [)] # for function pointer
                    |
                    [:] # for bit field
                )
                [^;]*
            )?
            ;
        ");

        foreach (Match match in regex.Matches(code).Cast<Match>())
        {
            result.AddRange(match.Groups["identifier"].Captures.Select(c => c.Value));
        }

        return result;
    }
}

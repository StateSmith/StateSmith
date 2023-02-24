#nullable enable

using StateSmith.Input.Expansions;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace StateSmith.Output.UserConfig;

public class AutoExpandedVarsProcessor
{
    private readonly RenderConfigVars renderConfig;
    private readonly Expander expander;
    private readonly IExpansionVarsPathProvider expansionVarsPathProvider;

    public AutoExpandedVarsProcessor(RenderConfigVars renderConfig, Expander expander, IExpansionVarsPathProvider expansionVarsPathProvider)
    {
        this.renderConfig = renderConfig;
        this.expander = expander;
        this.expansionVarsPathProvider = expansionVarsPathProvider;
    }

    public void AddExpansions()
    {
        string toParse = renderConfig.AutoExpandedVars;
        var varsPath = expansionVarsPathProvider.ExpansionVarsPath;

        var identifiers = ParseIdentifiers(toParse);

        foreach (var identifier in identifiers)
        {
            expander.AddVariableExpansion(identifier, varsPath + identifier);
        }

        StringUtils.AppendInPlaceWithNewlineIfNeeded(ref renderConfig.VariableDeclarations, renderConfig.AutoExpandedVars);
    }

    public static List<string> ParseIdentifiers(string code)
    {
        var result = new List<string>();

        code = StringUtils.RemoveCCodeComments(code);

        if (code.Contains('{') || code.Contains('}'))
        {
            throw new ArgumentException($"Detected '{{' or '}}' in {nameof(RenderConfigVars.AutoExpandedVars)} section. That section parsing is currently too dumb to handle anonymous structures." +
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

#nullable enable

using StateSmith.Output;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using StateSmith.Common;
using System;

namespace StateSmith.SmGraph.TriggerMap;

/// <summary>
/// Parses an trigger map like `ALL => *`.
/// https://github.com/StateSmith/StateSmith/issues/161
/// </summary>
public class TriggerMapParser
{
    private static readonly Regex ruleRegex = new(@"(?x)
                (?: \G | \A )  # start of input or last match
                \s*
                (?<name> [$\w]+ )
                \s*  =>  \s*
                (?:
                    (?<triggerMatcher>
                        [$\w*]+ # exact or wild card matcher.
                        |
                        / .* / # JavaScript like regex matcher. Don't worry about patterns like `/ev\/blah/` as it couldn't match an event name anyway
                    )
                    \s*  ,?  \s*
                )+
                \s*
                (\r|\n|\r\n|\z)
                |
                (?<err>
                    [\s\S]+
                )
        ");

    public static Dictionary<string, TriggerMapRule> MatchLines(string mapText)
    {
        Dictionary<string, TriggerMapRule> mappings = new(StringComparer.OrdinalIgnoreCase);
        mapText = StringUtils.RemoveCCodeComments(mapText, keepLineEnding: true);

        if (mapText.Trim().Length == 0)
            return mappings;

        var matches = ruleRegex.Matches(mapText);

        foreach (Match match in matches.Cast<Match>())
        {
            var name = match.Groups["name"].Value;

            if (TriggerHelper.IsEnterExitTrigger(name))
            {
                throw new ArgumentException($"Trigger map failed parsing rule {mappings.Count + 1}. Rule name `{name}` can't match reserved enter or exit triggers.");
            }

            if (TriggerHelper.IsDoEvent(name))
            {
                throw new ArgumentException($"Trigger map failed parsing rule {mappings.Count + 1}. Rule name `{name}` can't match reserved `do` event.");
            }

            Group errGroup = match.Groups["err"];
            if (errGroup.Success)
            {
                var errText = GetSubStringUpToLength(errGroup.Value, 0, idealLength: 50);
                throw new ArgumentException($"Trigger map failed parsing rule {mappings.Count + 1}. Invalid input: `{errText}`");
            }

            var triggerMatchers = new HashSet<string>(match.Groups["triggerMatcher"].Captures.Select(c => c.Value).ToList());
            var rule = new TriggerMapRule()
            {
                name = name,
                triggerMatchers = triggerMatchers,
            };

            mappings.Add(name, rule);
        }

        return mappings;
    }

    private static string GetSubStringUpToLength(string mapText, int startIndex, int idealLength = 30)
    {
        int maxLength = mapText.Length - startIndex;
        int length = Math.Min(idealLength, maxLength);
        var errText = mapText.Substring(startIndex, length);

        if (length != idealLength)
        {
            errText += "<snip>";
        }

        return errText;
    }
}

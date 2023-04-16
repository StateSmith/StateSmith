#nullable enable

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;

namespace StateSmith.SmGraph.TriggerMap;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/161
/// </summary>
public class TriggerMapper
{
    internal Dictionary<string, TriggerMapRule> mappingRules = new(StringComparer.OrdinalIgnoreCase);

    public void Setup(string mappingText, IEnumerable<string> triggers)
    {
        mappingRules = TriggerMapParser.MatchLines(mappingText);
        PrepareMappings(mappingRules, triggers);
    }

    public List<string> MapTriggers(IEnumerable<string> triggers)
    {
        HashSet<string> result = new(StringComparer.OrdinalIgnoreCase);

        foreach (var trigger in triggers)
        {
            if (mappingRules.TryGetValue(trigger, out var rule))
            {
                foreach (var matcher in rule.triggerMatchers)
                {
                    result.Add(matcher);
                }
            }
            else
            {
                result.Add(trigger);
            }
        }

        return result.ToList();
    }

    public static void PrepareMappings(Dictionary<string, TriggerMapRule> mappings, IEnumerable<string> triggers)
    {
        var nonMappedTriggers = triggers.Where(trigger => !mappings.ContainsKey(trigger));

        ProcessRegularExpressions(mappings, nonMappedTriggers);
        ProcessRuleReferences(mappings);
    }

    /// <summary>
    /// Allows a simple (non-regex/wildcard) triggerMatcher to reference another replacement rule.
    /// <code>
    /// UPx => UP_PRESS, UP_HELD
    /// INPUT => UPx, MOUSE_CLICK
    /// </code>
    /// </summary>
    /// <param name="mappings"></param>
    private static void ProcessRuleReferences(Dictionary<string, TriggerMapRule> mappings)
    {
        foreach (TriggerMapRule rule in mappings.Values)
        {
            foreach (TriggerMapRule otherRule in mappings.Values)
            {
                if (rule != otherRule && otherRule.triggerMatchers.Contains(rule.name))
                {
                    otherRule.triggerMatchers.Remove(rule.name);
                    foreach (var item in rule.triggerMatchers)
                    {
                        otherRule.triggerMatchers.Add(item);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Some of the mapping replacements are regular expressions or use wild cards (that are converted to regular expressions).
    /// This call expands the regular expressions to the matched trigger.
    /// </summary>
    /// <param name="mappings"></param>
    /// <param name="nonMappedTriggers">Triggers found in the diagram that are not mappings.</param>
    public static void ProcessRegularExpressions(Dictionary<string, TriggerMapRule> mappings, IEnumerable<string> nonMappedTriggers)
    {
        var jsRegex = new Regex("/(.+)/");

        foreach (TriggerMapRule rule in mappings.Values)
        {
            foreach (var triggerMatcher in rule.triggerMatchers.ToList()) // ToList() so that we can modify original while iterating over it
            {
                Regex? regex = null;

                var match = jsRegex.Match(triggerMatcher);
                if (match.Success)
                {
                    string regexContents = match.Groups[1].Value;
                    regex = TryCompilingRegex(rule, triggerMatcher, regexContents);
                }
                else if (triggerMatcher.Contains('*'))
                {
                    string regexContents = triggerMatcher.Replace("*", "%star%");
                    regexContents = Regex.Escape(regexContents);
                    regexContents = regexContents.Replace("%star%", ".*");
                    regex = TryCompilingRegex(rule, triggerMatcher, regexContents);
                }

                if (regex != null)
                {
                    foreach (var trigger in nonMappedTriggers)
                    {
                        if (regex.IsMatch(trigger))
                        {
                            rule.triggerMatchers.Add(trigger);
                        }
                    }

                    rule.triggerMatchers.Remove(triggerMatcher);
                }
            }
        }
    }

    private static Regex TryCompilingRegex(TriggerMapRule mapping, string item, string regexContents)
    {
        Regex regex;
        try
        {
            regex = new Regex(@"\A" + regexContents + @"\z", RegexOptions.IgnoreCase);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Invalid Trigger Mapping `{mapping.name} => {item}`", ex);
        }

        return regex;
    }
}

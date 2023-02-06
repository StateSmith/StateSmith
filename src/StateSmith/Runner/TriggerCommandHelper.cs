using StateSmith.SmGraph;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// spell-checker: ignore modder

#nullable enable

namespace StateSmith.Runner;

public class TriggerCommandHelper
{
    public static IEnumerable<Behavior> GetCommandBehaviors(Vertex v)
    {
        return v.GetBehaviorsWithTrigger("$cmd");
    }

    private static List<Behavior> GetCommandBehaviors(Vertex v, Regex matcher)
    {
        List<Behavior> result = new();

        foreach (var b in GetCommandBehaviors(v))
        {
            if (matcher.IsMatch(b.actionCode))
            {
                result.Add(b);
            }
        }

        return result;
    }

    public static List<Match> PopCommandBehaviors(Vertex v, Regex matcher)
    {
        List<Match> result = new();

        for (int i = 0; i < v.Behaviors.Count; i++) // can't use foreach because we are modifying the collection
        {
            var b = v.Behaviors[i];
            var match = matcher.Match(b.actionCode);
            if (match.Success)
            {
                result.Add(match);
                v.RemoveBehaviorAndUnlink(b);
                i--; // because we removed a behavior
            }
        }

        return result;
    }
}

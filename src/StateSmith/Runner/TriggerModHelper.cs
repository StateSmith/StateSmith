using StateSmith.SmGraph;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// spell-checker: ignore modder

#nullable enable

namespace StateSmith.Runner;

/// <summary>
/// Experimental status. Can be used, but API may change.
/// </summary>
public class TriggerModHelper
{
    public static IEnumerable<Behavior> GetModBehaviors(Vertex v)
    {
        return v.GetBehaviorsWithTrigger("$mod");
    }

    public static List<Behavior> GetModBehaviors(Vertex v, Regex matcher)
    {
        List<Behavior> result = new();

        foreach (var b in GetModBehaviors(v))
        {
            if (matcher.IsMatch(b.actionCode))
            {
                result.Add(b);
            }
        }

        return result;
    }

    public static List<Match> PopModBehaviors(Vertex v, Regex matcher)
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

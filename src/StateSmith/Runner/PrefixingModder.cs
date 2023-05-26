using StateSmith.SmGraph.Visitors;
using StateSmith.SmGraph;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// spell-checker: ignore modder

#nullable enable

namespace StateSmith.Runner;

/// <summary>
/// Currently uses rather simple regular expressions for matching.
/// </summary>
public class PrefixingModder : VertexVisitor
{
    private static readonly Regex autoPrefixRegex = new(@"\bprefix[.]auto\(\s*\)");
    private static readonly Regex addPrefixRegex = new(@"\bprefix[.]add\(\s*(\w+)\s*\)");
    private static readonly Regex setPrefixRegex = new(@"\bprefix[.]set\(\s*(\w+)\s*\)");

    private Stack<string> prefixStack = new();

    public static void Process(StateMachine sm)
    {
        new PrefixingModder().Visit(sm);
    }

    public PrefixingModder()
    {
        prefixStack.Push(""); // dummy
    }

    public override void Visit(Vertex v)
    {
        if (v is NamedVertex namedVertex)
        {
            HandleNamedVertex(namedVertex);
        }
    }

    private void HandleNamedVertex(NamedVertex state)
    {
        var activePrefix = prefixStack.Peek();
        state.Rename($"{activePrefix}{state.Name}");    // may rename to the same if no prefix
        
        string foundPrefix = GetPrefix(state, activePrefix);

        prefixStack.Push(foundPrefix);
        VisitChildren(state);
        prefixStack.Pop();
    }

    private static string GetPrefix(NamedVertex state, string prefix)
    {
        foreach (var b in TriggerModHelper.GetModBehaviors(state))
        {
            string? newPrefix = MaybeGetPrefixFromBehavior(state, b, prefix);

            if (newPrefix != null)
            {
                state.RemoveBehaviorAndUnlink(b);
                prefix = newPrefix;
                break;
            }
        }

        return prefix;
    }

    private static string? MaybeGetPrefixFromBehavior(NamedVertex state, Behavior b, string prefix)
    {
        string actionCode = b.actionCode;
        Match match;

        match = autoPrefixRegex.Match(actionCode);
        if (match.Success)
        {
            return state.Name + "__"; // note that state name may have already been prefixed by parent at this point.
        }

        match = addPrefixRegex.Match(actionCode);
        if (match.Success)
        {
            return prefix + match.Groups[1] + "__";
        }

        match = setPrefixRegex.Match(actionCode);
        if (match.Success)
        {
            return match.Groups[1] + "__";
        }

        return null;
    }
}

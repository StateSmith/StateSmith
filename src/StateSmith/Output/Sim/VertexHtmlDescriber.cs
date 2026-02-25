#nullable enable

using StateSmith.SmGraph;
using System.Text;
using System.Linq;
using System.Web;
using System;

namespace StateSmith.Output.Sim;

public class VertexHtmlDescriber
{
    public Func<string, string> codeFilter = (s) => s;

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/525
    /// </summary>
    public bool lowerDiagramDetail = false;

    /// <summary>
    /// This MUST be done before simulation tracing behaviors are added.
    /// https://github.com/StateSmith/StateSmith/issues/523
    /// </summary>
    public string BuildVertexHtml(NamedVertex namedVertex, bool showTransitionText)
    {
        StringBuilder sb = new();
        
        Vertex? currentVertex = namedVertex;
        while (currentVertex != null)
        {
            sb.Append("<tr><td>");

            bool isLeafState = currentVertex == namedVertex;

            string prefix = (!isLeafState) ? "⇡ Parent " : "";

            sb.Append($"<h3>{prefix}Vertex: {VertexName(currentVertex)}, <b>Diagram Id:</b> {Syntax("vertex-id", currentVertex.DiagramId)}</h3>");

            if (isLeafState)
            {
                StringBuilder metaSb = new();

                if (currentVertex.Children.Any())
                {
                    metaSb.Append("Children: ");

                    foreach (var child in currentVertex.Children)
                    {
                        metaSb.Append($"{VertexName(child)}, ");
                    }
                    metaSb.Length -= 2; // remove trailing ", "
                    metaSb.Append('\n');
                    sb.Append(HtmlSpan("vertex-meta", metaSb.ToString()));
                }
            }

            foreach (var behavior in currentVertex.Behaviors)
            {
                sb.Append($"{BuildBehaviorHtml(behavior, showTransitionText:showTransitionText)}\n");
            }

            sb.Append("</td></tr>");
            currentVertex = currentVertex.Parent;
        }
            
        return sb.ToString().ReplaceLineEndings("<br>");
    }

    public static string VertexName(Vertex currentVertex)
    {
        return Syntax("vertex-name", HttpUtility.HtmlEncode(Vertex.Describe(currentVertex)));
    }

    public static string ActionCode(string code)
    {
        return Syntax("action-code", code);
    }

    public static string Syntax(string className, string code)
    {
        return HtmlSpan($"syntax-{className}", code);
    }

    public static string HtmlSpan(string className, string code)
    {
        return $"<span class='{className}'>{code}</span>";
    }

    public string BuildBehaviorHtml(Behavior b, bool showTransitionText)
    {
        string result = "";
        string joiner = "";

        if (b.order == Behavior.ELSE_ORDER)
        {
            result += Syntax("order", "else");
            joiner = " ";
        }
        else if (b.order != Behavior.DEFAULT_ORDER)
        {
            result += joiner;
            result += Syntax("order", b.order + ".");
            joiner = " ";
        }

        {
            string prefix = "", postfix = "";
            if (b.Triggers.Count > 1)
            {
                prefix = Syntax("trigger-punctuation", "(");
                postfix = Syntax("trigger-punctuation", ")");
            }

            result += prefix + joiner;

            bool needsComma = false;
            for (int i = 0; i < b.Triggers.Count; i++)
            {
                string trigger = b.Triggers[i];

                if (needsComma)
                {
                    result += Syntax("trigger-punctuation", ", ");
                }
                result += Syntax("trigger-name", trigger);
                needsComma = true;
            }

            result += postfix;
            joiner = " ";
        }

        if (b.HasGuardCode())
        {
            result += joiner + Syntax("guard-bracket", "[") + Syntax("guard-code", HttpUtility.HtmlEncode(codeFilter(b.guardCode))) + Syntax("guard-bracket", "]");
            joiner = " ";
        }

        bool alwaysShowActionCode = !b.HasTransition(); // See https://github.com/StateSmith/StateSmith/issues/355
        if (alwaysShowActionCode || b.HasActionCode())
        {
            string actionCode = b.actionCode;
            if (lowerDiagramDetail)
            {
                actionCode = "...";
            }

            result += joiner + Syntax("action-start-end", "/ { ") + ActionCode(HttpUtility.HtmlEncode(codeFilter(actionCode))) + Syntax("action-start-end", " }");
            joiner = " ";
        }

        if (b.HasViaExit())
        {
            result += joiner + Syntax("via-exit-entry", "via exit " + b.viaExit);
            joiner = " ";
        }

        if (b.HasViaEntry())
        {
            result += joiner + Syntax("via-exit-entry", "via entry " + b.viaEntry);
            joiner = " ";
        }

        if (showTransitionText && b.TransitionTarget != null)
        {
            result += joiner;
            result += Syntax("transition-arrow", "--> ");
            result += Syntax("transition-to", "TransitionTo(") + VertexName(b.TransitionTarget) + Syntax("transition-to", ")");
        }

        result = result.ReplaceLineEndings("<br>");
        return result;
    }
}

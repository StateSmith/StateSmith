using StateSmith.SmGraph;
using StateSmith.SmGraph.Visitors;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace StateSmith.Output.Sim;

class MermaidGenerator : IVertexVisitor
{
    int indentLevel = 0;
    StringBuilder sb = new();
    MermaidEdgeTracker mermaidEdgeTracker;
    BehaviorDescriber behaviorDescriber;

    /// <summary>
    /// We print this as a replacement for newlines in behavior descriptions,
    /// then the mermaid special characters are escaped, and finally we replace
    /// this token with a newline character in the final output.
    /// Without this, new lines in user action code end up as `#92;n` instead of `\n` in the mermaid output.
    /// </summary>
    const string LINE_BREAK_TOKEN = "__LINE_BREAK__";

    public MermaidGenerator(MermaidEdgeTracker edgeOrderTracker)
    {
        this.mermaidEdgeTracker = edgeOrderTracker;
        behaviorDescriber = new(singleLineFormat: true, newLine: LINE_BREAK_TOKEN);
        behaviorDescriber.describeTransition = false; // we don't want `TransitionTo(state)` printed in mermaid labels
    }

    public void RenderAll(StateMachine sm)
    {
        sm.Accept(this);
        RenderEdges(sm);
    }

    public string GetMermaidCode()
    {
        return sb.ToString();
    }

    public void Visit(StateMachine v)
    {
        AppendIndentedLine("stateDiagram");
        indentLevel--; // don't indent the state machine contents
        VisitChildren(v);
    }

    public void Visit(State v)
    {
        if (v.Children.Count <= 0)
        {
            VisitLeafState(v);
        }
        else
        {
            VisitCompoundState(v);
        }
    }

    private void VisitCompoundState(State v)
    {
        AppendIndentedLine($"state {v.Name} {{");
        // FIXME - add behavior code here when supported by mermaid
        // https://github.com/StateSmith/StateSmith/issues/268#issuecomment-2111432194
        VisitChildren(v);
        AppendIndentedLine("}");
    }

    private void VisitLeafState(State v)
    {
        string name = v.Name;
        AppendIndentedLine(name);
        AppendIndentedLine($"{name} : {name}");
        foreach (var b in v.NonTransitionBehaviors())
        {
            // always show action code for https://github.com/StateSmith/StateSmith/issues/355
            string behaviorText = BehaviorToMermaidLabel(b, alwaysShowActionCode: true);
            AppendIndentedLine($"{name} : {behaviorText}");
        }
    }

    public void Visit(InitialState initialState)
    {
        string initialStateId = MakeVertexDiagramId(initialState);

        bool showInitialStateDot = false;
        // disabled for now because we then need to change edge highlighting.
        // @emmby: can we we potentially style the `$initial_state` so that it looks somewhat like an initial state black filled circle? Thoughts?

        if (showInitialStateDot)
        {
            // Mermaid and PlantUML don't have a syntax that allows transitions to an initial state.
            // If you do `someState --> [*]`, it means transition to a final state.
            // StateSmith, however, does allow transitions to initial states so we add a dummy state to represent the initial state.
            AppendIndentedLine($"[*] --> {initialStateId}");
            mermaidEdgeTracker.AdvanceId();  // we skip this "work around" edge for now. We can improve this later.
        }

        AppendIndentedLine($"state \"$initial_state\" as {initialStateId}");
    }

    public void Visit(ChoicePoint v)
    {
        AppendIndentedLine($"state {MakeVertexDiagramId(v)} <<choice>>");
    }

    public void Visit(EntryPoint v)
    {
        AppendIndentedLine($"state \"$entry_pt.{v.label}\" as {MakeVertexDiagramId(v)}");
    }

    public void Visit(ExitPoint v)
    {
        AppendIndentedLine($"state \"$exit_pt.{v.label}\" as {MakeVertexDiagramId(v)}");
    }

    public void Visit(HistoryVertex v)
    {
        AppendIndentedLine($"state \"$H\" as {MakeVertexDiagramId(v)}");
    }

    public void Visit(HistoryContinueVertex v)
    {
        AppendIndentedLine($"state \"$HC\" as {MakeVertexDiagramId(v)}");
    }


    public void RenderEdges(StateMachine sm)
    {
        sm.VisitRecursively((Vertex v) =>
        {
            string vertexDiagramId = MakeVertexDiagramId(v);

            foreach (var behavior in v.Behaviors)
            {
                // transition behaviors
                if (behavior.TransitionTarget != null)
                {
                    Append($"{vertexDiagramId} --> {MakeVertexDiagramId(behavior.TransitionTarget)}");

                    // only append edge label if behavior text is not empty to avoid Mermaid parse errors
                    string behaviorText = BehaviorToMermaidLabel(behavior);
                    if (!string.IsNullOrWhiteSpace(behaviorText))
                    {
                        Append($" : {behaviorText}");
                    }
                    AppendLine();

                    mermaidEdgeTracker.AddEdge(behavior);
                }
            }
        });
    }

    public string BehaviorToMermaidLabel(Behavior behavior, bool alwaysShowActionCode = false)
    {
        var behaviorText = behaviorDescriber.Describe(behavior, alwaysShowActionCode: alwaysShowActionCode);
        behaviorText = MermaidEscape(behaviorText);
        behaviorText = behaviorText.Replace(LINE_BREAK_TOKEN, "\\n");
        return behaviorText;
    }

    public static string MakeVertexDiagramId(Vertex v)
    {
        switch (v)
        {
            case NamedVertex namedVertex:
                return namedVertex.Name;
            default:
                // see https://github.com/StateSmith/StateSmith/blob/04955e5df7d5eb6654a048dccb35d6402751e4c6/src/StateSmithTest/VertexDescribeTests.cs
                return Vertex.Describe(v).Replace("<", "(").Replace(">", ")");
        }
    }

    // TODO handle #
    // You can't naively add # to the list of characters because # and ; will interfere with each other
    public static string MermaidEscape(string text)
    {
        foreach (char c in ":;\\{}".ToCharArray())
        {
            text = text.Replace(c.ToString(), $"#{(int)c};");
        }
        return text;
    }

    private void Append(string message)
    {
        sb.Append(message);
    }

    private void AppendLine(string message = "")
    {
        sb.AppendLine(message);

        // add an extra blank line so that git diffs work on individual lines instead of giant text blocks
        sb.AppendLine();
    }

    private void AppendIndentedLine(string message)
    {
        for (int i = 0; i < indentLevel; i++)
            sb.Append("        ");

        AppendLine(message);
    }

    private void VisitChildren(Vertex v)
    {
        indentLevel++;
        foreach (var child in v.Children)
        {
            child.Accept(this);
        }
        indentLevel--;
    }

    public void Visit(RenderConfigVertex v)
    {
        // just ignore render config and any children
    }

    public void Visit(ConfigOptionVertex v)
    {
        // just ignore config option and any children
    }

    // orthogonal states are not yet implemented, but will be one day
    public void Visit(OrthoState v)
    {
        throw new NotImplementedException();
    }

    public void Visit(NotesVertex v)
    {
        // just ignore notes and any children
    }

    public void Visit(NamedVertex v)
    {
        throw new NotImplementedException(); // should not be called
    }

    public void Visit(Vertex v)
    {
        throw new NotImplementedException(); // should not be called
    }
}

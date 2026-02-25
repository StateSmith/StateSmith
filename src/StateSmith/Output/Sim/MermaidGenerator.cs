using StateSmith.SmGraph;
using StateSmith.SmGraph.Visitors;
using System;
using System.Text;

namespace StateSmith.Output.Sim;

class MermaidGenerator : IVertexVisitor
{
    int indentLevel = 0;
    StringBuilder sb = new();
    MermaidEdgeTracker mermaidEdgeTracker;

    /// <summary>
    /// 
    /// </summary>
    public bool ellipsizeActionCode = false;

    public MermaidGenerator(MermaidEdgeTracker edgeOrderTracker)
    {
        this.mermaidEdgeTracker = edgeOrderTracker;
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
        AppendIndentedLine($"state \"{VertexHtmlDescriber.VertexName(v)}\" as {v.Name} {{");
        // FIXME - add behavior code here when supported by mermaid
        // https://github.com/StateSmith/StateSmith/issues/268#issuecomment-2111432194
        VisitChildren(v);
        AppendIndentedLine("}");
    }

    private void VisitLeafState(State v)
    {
        string name = v.Name;
        AppendIndentedLine(name);
        AppendIndentedLine($"{name} : {VertexHtmlDescriber.VertexName(v)}");
        bool ellipsized = false;

        foreach (var b in v.NonTransitionBehaviors())
        {
            if (ellipsizeActionCode)
            {
                ellipsized = true;
            }
            else
            {
                string behaviorText = BehaviorToMermaidLabel(b);
                AppendIndentedLine($"{name} : {behaviorText}");
            }
        }

        if (ellipsized)
        {
            AppendIndentedLine($"{name} : ...");
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

        AppendIndentedLine($"%% Initial state name as \".\" so that it fits in black circle shape.", extraLine: false);
        AppendIndentedLine($"%% See https://github.com/StateSmith/StateSmith/issues/404", extraLine: false);
        AppendIndentedLine($"state \".\" as {initialStateId}");
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

    public string BehaviorToMermaidLabel(Behavior behavior)
    {
        VertexHtmlDescriber vertexHtmlDescriber = new();
        vertexHtmlDescriber.codeFilter = MermaidEscape;

        var behaviorText = vertexHtmlDescriber.BuildBehaviorHtml(behavior, showTransitionText:false);
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

    public static string MermaidEscape(string text)
    {
        // replace ';' and ':' with look a likes as Mermaid is really picky about this
        text = text.Replace(":", "꞉");
        text = text.Replace(";", ";");
        
        foreach (char c in "#\\{}<>".ToCharArray())
        {
            // kinda strange that we don't prefix with `&` like HTML, but that's how mermaid works
            text = text.Replace(c.ToString(), $"#{(int)c};");
        }

        return text;
    }

    private void Append(string message)
    {
        sb.Append(message);
    }

    private void AppendLine(string message = "", bool extraLine = true)
    {
        sb.AppendLine(message);

        if (extraLine)
        {
            // add an extra blank line so that git diffs work on individual lines instead of giant text blocks
            sb.AppendLine();
        }
    }

    private void AppendIndentedLine(string message, bool extraLine = true)
    {
        for (int i = 0; i < indentLevel; i++)
            sb.Append("        ");

        AppendLine(message, extraLine);
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

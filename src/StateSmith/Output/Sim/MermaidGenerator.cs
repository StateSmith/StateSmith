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
        AppendLn("stateDiagram");
        AppendLn("classDef active fill:yellow,stroke-width:2px;");
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
        AppendLn($"state {v.Name} {{");
        // FIXME - add behavior code here when supported by mermaid
        // https://github.com/StateSmith/StateSmith/issues/268#issuecomment-2111432194
        VisitChildren(v);
        AppendLn("}");
    }

    private void VisitLeafState(State v)
    {
        string name = v.Name;
        AppendLn(name);
        AppendLn($"{name} : {name}");
        foreach (var b in v.Behaviors.Where(b => b.TransitionTarget == null))
        {
            string text = b.ToString();
            text = MermaidEscape(text);
            AppendLn($"{name} : {text}");
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
            AppendLn($"[*] --> {initialStateId}");
            mermaidEdgeTracker.AdvanceId();  // we skip this "work around" edge for now. We can improve this later.
        }

        AppendLn($"state \"$initial_state\" as {initialStateId}");
    }

    public void Visit(ChoicePoint v)
    {
        AppendLn($"state {MakeVertexDiagramId(v)} <<choice>>");
    }

    public void Visit(EntryPoint v)
    {
        AppendLn($"state \"$entry_pt.{v.label}\" as {MakeVertexDiagramId(v)}");
    }

    public void Visit(ExitPoint v)
    {
        AppendLn($"state \"$exit_pt.{v.label}\" as {MakeVertexDiagramId(v)}");
    }

    public void Visit(HistoryVertex v)
    {
        AppendLn($"state \"$H\" as {MakeVertexDiagramId(v)}");
    }

    public void Visit(HistoryContinueVertex v)
    {
        AppendLn($"state \"$HC\" as {MakeVertexDiagramId(v)}");
    }


    public void RenderEdges(StateMachine sm)
    {
        sm.VisitRecursively((Vertex v) =>
        {
            string vertexDiagramId = MakeVertexDiagramId(v);

            foreach (var behavior in v.Behaviors)
            {
                if (behavior.TransitionTarget != null)
                {
                    var behaviorText = behavior.ToString();
                    behaviorText = Regex.Replace(behaviorText, @"\s*TransitionTo[(].*[)]", ""); // bit of a hack to remove the `TransitionTo(SOME_STATE)` text
                    behaviorText = MermaidEscape(behaviorText);
                    sb.Append($"{vertexDiagramId} --> {MakeVertexDiagramId(behavior.TransitionTarget)}");
                    
                    // only append edge label if behavior text is not empty to avoid Mermaid parse errors
                    if (!string.IsNullOrWhiteSpace(behaviorText))
                    {
                        sb.Append($" : {behaviorText}");
                    }
                    sb.AppendLine();

                    mermaidEdgeTracker.AddEdge(behavior);
                }
            }
        });
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

    private void AppendLn(string message)
    {
        for (int i = 0; i < indentLevel; i++)
            sb.Append("        ");

        sb.AppendLine(message);
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

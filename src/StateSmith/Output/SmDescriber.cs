#nullable enable

using StateSmith.Common;
using StateSmith.SmGraph;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StateSmith.Output;

public class SmDescriber
{
    protected TextWriter writer;

    public SmDescriber(TextWriter writer)
    {
        this.writer = writer;
    }

    public void Describe(StateMachine stateMachine)
    {
        BehaviorDescriber behaviorDescriber = new(singleLineFormat: false, multilinePrefix: "    ");

        // sort vertices by diagram id so that they are always in the same order
        SortedDictionary<string, Vertex> vertices = new();

        stateMachine.VisitRecursively(v =>
        {
            vertices.Add(v.DiagramId, v);
        });

        bool prependSeparator = false;
        foreach (var v in vertices.Values)
        {
            OutputForVertex(behaviorDescriber, v, prependSeparator:prependSeparator);
            prependSeparator = true;
        }
    }

    private void OutputForVertex(BehaviorDescriber behaviorDescriber, Vertex v, bool prependSeparator)
    {
        if (prependSeparator)
        {
            WriteLine($"\n");
        }   

        WriteLine($"Vertex: {Vertex.Describe(v)}");
        WriteLine($"=================");

        if (v.Parent != null)
        {
            WriteLine($"Parent: {Vertex.Describe(v.Parent)}");
        }
        
        WriteLine($"Type: {v.GetType().Name}");
        WriteLine($"Diagram Id: {v.DiagramId}");

        InitialState? initialState = v.Children.OfType<InitialState>().FirstOrDefault();
        if (initialState != null)
        {
            WriteLine($"Initial State: {Vertex.Describe(initialState)}");
        }

        WriteLine($"Behaviors:");
        OutputBehaviors(writer, behaviorDescriber, v);

        var ancestor = v.Parent;
        if (v is NamedVertex)
        {
            while (ancestor != null)
            {
                MaybeOutputAncestorBehaviors(behaviorDescriber, ancestor);
                ancestor = ancestor.Parent;
            }
        }

    }

    private void MaybeOutputAncestorBehaviors(BehaviorDescriber behaviorDescriber, Vertex ancestor)
    {
        StringBuilder sb = new();
        OutputBehaviors(new StringWriter(sb), behaviorDescriber, ancestor, eventsOnly: true);

        if (sb.Length == 0)
        {
            return;
        }

        WriteLine($"\n    =========== from ancestor {Vertex.Describe(ancestor)} ===========\n");
        Write(sb.ToString());
    }

    private static void OutputBehaviors(TextWriter textWriter, BehaviorDescriber behaviorDescriber, Vertex v, bool eventsOnly = false)
    {
        bool first = true;

        foreach (var behavior in v.Behaviors)
        {
            if (eventsOnly && !TriggerHelper.HasAnEventTrigger(behavior))
            {
                continue;
            }

            if (!first)
            {
                textWriter.Write($"\n");
            }

            textWriter.Write($"{behaviorDescriber.Describe(behavior)}\n");

            first = false;
        }
    }

    protected void WriteLine(string text)
    {
        Write(text);
        writer.Write("\n");
    }

    protected void Write(string text)
    {
        writer.Write(text);
    }
}


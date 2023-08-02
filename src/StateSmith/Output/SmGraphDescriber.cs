#nullable enable

using StateSmith.Common;
using StateSmith.SmGraph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StateSmith.Output;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/200
/// </summary>
public class SmGraphDescriber : IDisposable
{
    protected string indentStr = "    ";
    protected TextWriter writer;
    protected bool prependSeparator = false;
    protected bool outputAncestorHandlers = false;

    public SmGraphDescriber(TextWriter writer)
    {
        this.writer = writer;
    }

    public void SetOutputAncestorHandlers(bool outputAncestorHandlers)
    {
        this.outputAncestorHandlers = outputAncestorHandlers;
    }

    public static void DescribeToFile(Vertex vertex, string filePath)
    {
        using var writer = new StreamWriter(filePath);
        var describer = new SmGraphDescriber(writer);
        describer.Describe(vertex);
    }

    public void OutputHeader(string header)
    {
        WriteLine($"{header}");
        WriteLine($"==========================================================="); // this is a markdown H1
        WriteLine("");
    }

    // describe array of vertices
    public void Describe(IEnumerable<Vertex> vertices)
    {
        foreach (var v in vertices)
        {
            Describe(v);
        }
    }

    public void Describe(Vertex vertex)
    {
        BehaviorDescriber behaviorDescriber = new(singleLineFormat: false, indent: indentStr);

        // sort vertices by diagram id so that they are always in the same order
        SortedDictionary<string, Vertex> vertices = new();

        vertex.VisitRecursively(v =>
        {
            vertices.Add(v.DiagramId, v);
        });

        foreach (var v in vertices.Values)
        {
            OutputForVertex(behaviorDescriber, v, prependSeparator: prependSeparator);
            prependSeparator = true;
        }
    }

    public void ClearSeparatorFlag()
    {
        prependSeparator = false;
    }

    private void OutputForVertex(BehaviorDescriber behaviorDescriber, Vertex v, bool prependSeparator)
    {
        if (prependSeparator)
        {
            WriteLine($"\n");
        }

        WriteLine($"Vertex: {DescribeVertex(v)}");
        WriteLine($"-----------------------------------------"); // this is a markdown H2

        if (v.Parent != null)
        {
            WriteLine($"- Parent: {DescribeVertex(v.Parent)}");
        }

        WriteLine($"- Type: {v.GetType().Name}");
        WriteLine($"- Diagram Id: {v.DiagramId}");

        if (v is ConfigOptionVertex configOptionVertex)
        {
            WriteLine($"\n### Option Content:");
            WriteLine(Indent(configOptionVertex.value));
        }
        else if (v is RenderConfigVertex)
        {
            // nothing to add here
        }
        else if (v is NotesVertex notesVertex)
        {
            WriteLine($"\n### Notes Content:");
            WriteLine(Indent(notesVertex.notes));
        }
        else
        {
            InitialState? initialState = v.Children.OfType<InitialState>().FirstOrDefault();
            if (initialState != null)
            {
                WriteLine($"- Initial State: {DescribeVertex(initialState)}");
            }

            WriteLine($"\n### Behaviors");
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
    }

    private static string DescribeVertex(Vertex v)
    {
        string description = Vertex.Describe(v);
        description = description.Replace("<", @"\<"); // for markdown
        return description;
    }

    internal void SetTextWriter(TextWriter writer)
    {
        this.writer = writer;
    }

    private string Indent(string str)
    {
        return StringUtils.Indent(str, indentStr);
    }

    private void MaybeOutputAncestorBehaviors(BehaviorDescriber behaviorDescriber, Vertex ancestor)
    {
        if (!outputAncestorHandlers)
            return;

        StringBuilder sb = new();
        OutputBehaviors(new StringWriter(sb), behaviorDescriber, ancestor, eventsOnly: true);

        if (sb.Length == 0)
        {
            return;
        }

        WriteLine($"\n    =========== from ancestor {DescribeVertex(ancestor)} ===========\n");
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

    public void WriteLine(string text)
    {
        Write(text);
        writer.Write("\n");
    }

    public void Write(string text)
    {
        writer.Write(text);
    }

    public void Dispose() => writer.Dispose();
}


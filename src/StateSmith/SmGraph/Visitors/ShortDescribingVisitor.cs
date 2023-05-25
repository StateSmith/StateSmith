#nullable enable

using System.Text;

namespace StateSmith.SmGraph.Visitors;

public class ShortDescribingVisitor : VertexVisitor // Note! This class should not use any convenience visitors that group types (should be explicit).
{
    protected StringBuilder stringBuilder;

    public ShortDescribingVisitor()
    {
        stringBuilder = new StringBuilder();
    }

    public ShortDescribingVisitor(StringBuilder stringBuilder)
    {
        this.stringBuilder = stringBuilder;
    }

    public static void Describe(StringBuilder stringBuilder, Vertex vertex)
    {
        var visitor = new ShortDescribingVisitor(stringBuilder);
        vertex.Accept(visitor);
    }

    public static string Describe(Vertex vertex)
    {
        var sb = new StringBuilder();
        Describe(sb, vertex);
        return sb.ToString();
    }

    //----------------------------------------------------------------------------------

    protected void Append(string str)
    {
        stringBuilder.Append(str);
    }

    public override void Visit(StateMachine v)
    {
        //Append($"$STATEMACHINE({v.Name})");   // we should transition to this eventually. Maybe something like `$STATEMACHINE(MySm).ROOT`
        Append($"ROOT");
    }

    public override void Visit(State v)
    {
        Append(v.Name);
    }

    public override void Visit(OrthoState v)
    {
        Append($"$ORTHO({v.Name})");
    }

    public override void Visit(Vertex v)
    {
        Append($"Vertex({v.GetType().Name})");  // This shouldn't happen, but doesn't hurt just in case we miss something in the future
    }

    public override void Visit(RenderConfigVertex v)
    {
        Append("$RenderConfig");
    }

    public override void Visit(ConfigOptionVertex v)
    {
        Append($"$CONFIG({v.name})");
    }

    public override void Visit(NamedVertex v)
    {
        Append($"NamedVertex({v.Name})"); // shouldn't happen, but doesn't hurt
    }

    public override void Visit(NotesVertex v)
    {
        Append("$NOTES");
    }

    //---------------------------------------------------------------------------------------------------

    protected void AppendRelativeToParent(Vertex v, string str)
    {
        Append($"{Vertex.Describe(v.Parent)}.{str}");
    }

    public override void Visit(InitialState v)
    {
        AppendRelativeToParent(v, $"InitialState");
    }

    public override void Visit(EntryPoint v)
    {
        AppendRelativeToParent(v, $"EntryPoint({v.label})");
    }

    public override void Visit(ExitPoint v)
    {
        AppendRelativeToParent(v, $"ExitPoint({v.label})");
    }

    public override void Visit(ChoicePoint v)
    {
        AppendRelativeToParent(v, $"ChoicePoint({v.label})");
    }

    public override void Visit(HistoryVertex v)
    {
        AppendRelativeToParent(v, $"History");
    }

    public override void Visit(HistoryContinueVertex v)
    {
        AppendRelativeToParent(v, $"HistoryContinue");
    }
}

#nullable enable

using System.Text;

namespace StateSmith.SmGraph.Visitors;

public class ShortDescribingVisitor : IVertexVisitor // Note! This class should not use any convenience visitors that group types (should be explicit).
{
    protected StringBuilder stringBuilder;
    protected bool skipParentForRelative;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stringBuilder"></param>
    /// <param name="skipParentForRelative">Useful when describing an ancestry chain like `ROOT.S1.S2.InitialState`</param>
    public ShortDescribingVisitor(StringBuilder stringBuilder, bool skipParentForRelative = false)
    {
        this.stringBuilder = stringBuilder;
        this.skipParentForRelative = skipParentForRelative;
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

    public string PopString()
    {
        var str = stringBuilder.ToString();
        stringBuilder.Clear();
        return str;
    }

    public void AppendDescription(Vertex vertex)
    {
        vertex.Accept(this);
    }

    //----------------------------------------------------------------------------------

    protected void Append(string str)
    {
        stringBuilder.Append(str);
    }

    void IVertexVisitor.Visit(StateMachine v)
    {
        //Append($"$STATEMACHINE({v.Name})");   // we should transition to this eventually. Maybe something like `$STATEMACHINE(MySm).ROOT`
        Append($"ROOT");
    }

    void IVertexVisitor.Visit(State v)
    {
        Append(v.Name);
    }

    void IVertexVisitor.Visit(OrthoState v)
    {
        Append($"<Ortho>({v.Name})");
    }

    void IVertexVisitor.Visit(Vertex v)
    {
        Append($"<Vertex>({v.GetType().Name})");  // This shouldn't happen, but doesn't hurt just in case we miss something in the future
    }

    void IVertexVisitor.Visit(RenderConfigVertex v)
    {
        Append("<RenderConfig>");
    }

    void IVertexVisitor.Visit(ConfigOptionVertex v)
    {
        Append($"<Config>({v.name})");
    }

    void IVertexVisitor.Visit(NamedVertex v)
    {
        Append($"<NamedVertex>({v.Name})"); // shouldn't happen, but doesn't hurt
    }

    void IVertexVisitor.Visit(NotesVertex v)
    {
        Append("<Notes>");
    }

    //---------------------------------------------------------------------------------------------------

    protected void AppendRelativeToParent(Vertex v, string str)
    {
        if (!skipParentForRelative)
            Append(Vertex.Describe(v.Parent) + ".");

        Append(str);
    }

    void IVertexVisitor.Visit(InitialState v)
    {
        AppendRelativeToParent(v, $"<InitialState>");
    }

    void IVertexVisitor.Visit(EntryPoint v)
    {
        AppendRelativeToParent(v, $"<EntryPoint>({v.label})");
    }

    void IVertexVisitor.Visit(ExitPoint v)
    {
        AppendRelativeToParent(v, $"<ExitPoint>({v.label})");
    }

    void IVertexVisitor.Visit(ChoicePoint v)
    {
        AppendRelativeToParent(v, $"<ChoicePoint>({v.label})");
    }

    void IVertexVisitor.Visit(HistoryVertex v)
    {
        AppendRelativeToParent(v, $"<History>");
    }

    void IVertexVisitor.Visit(HistoryContinueVertex v)
    {
        AppendRelativeToParent(v, $"<HistoryContinue>");
    }
}

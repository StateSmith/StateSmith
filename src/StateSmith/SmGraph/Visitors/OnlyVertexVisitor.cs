namespace StateSmith.SmGraph.Visitors;

public class OnlyVertexVisitor : VertexVisitor
{
    public override void Visit(NamedVertex v)
    {
        Visit((Vertex)v);
    }

    public override void Visit(State v)
    {
        Visit((Vertex)v);
    }

    public override void Visit(OrthoState v)
    {
        Visit((Vertex)v);
    }
    public override void Visit(StateMachine v)
    {
        Visit((Vertex)v);
    }
    public override void Visit(NotesVertex v)
    {
        Visit((Vertex)v);
    }
    public override void Visit(InitialState v)
    {
        Visit((Vertex)v);
    }

    public override void Visit(Vertex v)
    {
        Visit((Vertex)v);
    }

    public override void Visit(ChoicePoint v)
    {
        Visit((Vertex)v);
    }

    public override void Visit(EntryPoint v)
    {
        Visit((Vertex)v);
    }

    public override void Visit(ExitPoint v)
    {
        Visit((Vertex)v);
    }

    public override void Visit(HistoryVertex v)
    {
        Visit((Vertex)v);
    }

    public override void Visit(HistoryContinueVertex v)
    {
        Visit((Vertex)v);
    }

    public override void Visit(RenderConfigVertex v)
    {
        Visit((Vertex)v);
    }

    public override void Visit(ConfigOptionVertex v)
    {
        Visit((Vertex)v);
    }
}

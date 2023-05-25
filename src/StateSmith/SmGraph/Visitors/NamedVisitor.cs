namespace StateSmith.SmGraph.Visitors;

public class NamedVisitor : OnlyVertexVisitor
{
    public override void Visit(State v)
    {
        Visit((NamedVertex)v);
    }

    public override void Visit(OrthoState v)
    {
        Visit((NamedVertex)v);
    }

    public override void Visit(StateMachine v)
    {
        Visit((NamedVertex)v);
    }
}

using StateSmith.Common;
using StateSmith.SmGraph.Visitors;

namespace StateSmith.SmGraph;

public class DefaultToDoEventVisitor : NamedVisitor
{
    public static void Process(Vertex v)
    {
        var processor = new DefaultToDoEventVisitor();
        v.Accept(processor);
    }

    public override void Visit(NamedVertex v)
    {
        foreach (var b in v.Behaviors)
        {
            if (b.HasImplicitDoTriggerOnly())
            {
                b._triggers.Add(TriggerHelper.TRIGGER_DO);
            }
        }

        VisitVertexChildren(v, this);
    }

    public override void Visit(Vertex v)
    {
        //throw new System.NotImplementedException("should not happen");
    }

    public override void Visit(NotesVertex v)
    {
        //throw new System.NotImplementedException();
    }

    public override void Visit(InitialState v)
    {
        //throw new System.NotImplementedException();
    }
}

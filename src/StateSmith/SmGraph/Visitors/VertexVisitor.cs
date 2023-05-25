using System.Collections.Generic;

namespace StateSmith.SmGraph.Visitors;

public class VertexVisitor : IVertexVisitor
{
    public virtual void Visit(Vertex v)
    {
        // do nothing...
    }

    public virtual void Visit(NamedVertex v)
    {
        Visit((Vertex)v);
    }

    public virtual void Visit(State v)
    {
        Visit((Vertex)v);
    }

    public virtual void Visit(OrthoState v)
    {
        Visit((Vertex)v);
    }
    public virtual void Visit(StateMachine v)
    {
        Visit((Vertex)v);
    }
    public virtual void Visit(NotesVertex v)
    {
        Visit((Vertex)v);
    }
    public virtual void Visit(InitialState v)
    {
        Visit((Vertex)v);
    }

    public virtual void Visit(ChoicePoint v)
    {
        Visit((Vertex)v);
    }

    public virtual void Visit(EntryPoint v)
    {
        Visit((Vertex)v);
    }

    public virtual void Visit(ExitPoint v)
    {
        Visit((Vertex)v);
    }

    public virtual void Visit(HistoryVertex v)
    {
        Visit((Vertex)v);
    }

    public virtual void Visit(HistoryContinueVertex v)
    {
        Visit((Vertex)v);
    }

    public virtual void Visit(RenderConfigVertex v)
    {
        Visit((Vertex)v);
    }

    public virtual void Visit(ConfigOptionVertex v)
    {
        Visit((Vertex)v);
    }

    //-------------------------------------------------------------------------------------

    public static void VisitVertexChildren(Vertex v, IVertexVisitor visitor)
    {
        //copy list so that we can remove children while iterating.
        //TODO Not ideal. What if larger scale changes made? requires thought.
        var childrenCopy = new List<Vertex>(v.Children);
        foreach (var child in childrenCopy)
        {
            child.Accept(visitor);
        }
    }

    public virtual void VisitChildren(Vertex v)
    {
        VisitVertexChildren(v, this);
    }
}

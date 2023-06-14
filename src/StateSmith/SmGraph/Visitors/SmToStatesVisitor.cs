#nullable enable

using StateSmith.SmGraph.Validation;
using System.Collections.Generic;
using System.Linq;

namespace StateSmith.SmGraph.Visitors;

public class SmToNamedVerticesVisitor : VertexVisitor
{
    /// <summary>
    /// Includes regular states, super states, orthogonal states (some day) and state machines
    /// </summary>
    public List<NamedVertex> namedVertices = new();

    // helps make state machine graph less susceptible to yEd xml editing changes.
    public void VisitChildrenInOrderOfName(Vertex v)
    {
        var sortedNamedKids = v.NamedChildren().OrderBy(x => x.Name).ToList();

        foreach (var child in sortedNamedKids)
        {
            child.Accept(this);
        }
    }

    public override void Visit(State v)
    {
        namedVertices.Add(v);
        VisitChildrenInOrderOfName(v);
    }

    public override void Visit(StateMachine v)
    {
        if (v.Parent != null)
        {
            throw new VertexValidationException(v, "State machines cannot be nested, yet. See https://github.com/adamfk/StateSmith/issues/7");
        }
        namedVertices.Add(v);
        VisitChildrenInOrderOfName(v);
    }

    public override void Visit(OrthoState v)
    {
        throw new VertexValidationException(v, "Ortho states not implemented, yet. TODO create a github issues to track this.");
        // namedVertices.Add(v);
    }

    public override void Visit(Vertex v)
    {
        //VisitChildrenInOrderOfName(v);
        throw new VertexValidationException(v, "Unexpected visit. TODO create a github issues to track this.");
    }

    public override void Visit(NamedVertex v)
    {
        throw new VertexValidationException(v, "Unexpected visit. TODO create a github issues to track this.");
    }

    public override void Visit(NotesVertex v)
    {
        // don't visit anything else in a NotesVertex. It's like any contained states are commented out
    }

    public override void Visit(InitialState v)
    {
        // ignore initial states
    }
}

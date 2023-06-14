#nullable enable
using System.Linq;
using StateSmith.SmGraph.Visitors;

namespace StateSmith.SmGraph.Validation;

public class SpecificVertexValidator : VertexVisitor
{
    public override void Visit(InitialState initialState)
    {
        InitialStateValidator.Validate(initialState);
    }

    public override void Visit(HistoryVertex v)
    {
        HistoryStateValidator.ValidateAfterTransforming(v);
    }

    public override void Visit(HistoryContinueVertex v)
    {
        throw new VertexValidationException(v, "History Continue vertex should have been removed by processing already.");
    }

    public override void Visit(Vertex v)
    {
        throw new System.NotImplementedException();
    }

    public override void Visit(NamedVertex v)
    {
        throw new System.NotImplementedException();
    }

    public override void Visit(State v)
    {
        VisitChildren(v);
    }

    public override void Visit(OrthoState v)
    {
        throw new System.NotImplementedException();
    }

    public override void Visit(StateMachine v)
    {
        if (v.Parent != null)
        {
            throw new VertexValidationException(v, "State machines cannot be nested, yet. See https://github.com/adamfk/StateSmith/issues/7");
        }

        // require initial state
        int initialStateCount = v.Children.OfType<InitialState>().Count();
        if (initialStateCount != 1)
        {
            throw new VertexValidationException(v, $"State machines must have exactly 1 initial state. Actual count: {initialStateCount}.");
        }

        VisitChildren(v);
    }

    /// <summary>
    /// <see cref="NotesProcessor.ValidateAndRemoveNotes(StateMachine)"/>
    /// </summary>
    /// <param name="v"></param>
    public override void Visit(NotesVertex v)
    {
        // see method comment
    }

    public override void Visit(ChoicePoint v)
    {
        ChoicePointValidator.Validate(v);
    }

    public override void Visit(EntryPoint entryPoint)
    {
        EntryPointValidator.Validate(entryPoint);
    }

    public override void Visit(ExitPoint v)
    {
        ExitPointValidator.Validate(v);
    }
}

#nullable enable

using System.Collections.Generic;
using System.Linq;

namespace StateSmith.SmGraph;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/190
/// </summary>
public class NotesProcessor
{
    /// <summary>
    /// Contains Notes vertices or vertices inside of a Notes vertice.
    /// </summary>
    readonly HashSet<Vertex> notedVertices = new();

    List<NotesVertex> topLevelNotes = new();

    public static void Process(StateMachine sm)
    {
        var processor = new NotesProcessor();
        processor.ValidateAndRemoveNotes(sm);
    }

    public void ValidateAndRemoveNotes(StateMachine sm)
    {
        ValidateNotesWithoutRemoving(sm);
        RemoveTopLevelNotes();
    }

    // only use for unit testing
    internal void ValidateNotesWithoutRemoving(StateMachine sm)
    {
        FindAllNotedVertices(sm);
        LookForIllegalTransitionsToNotes(sm);
    }

    private void LookForIllegalTransitionsToNotes(StateMachine sm)
    {
        sm.VisitRecursively((vertex, context) =>
        {
            if (vertex is NotesVertex)
            {
                context.SkipChildren();
                return;
            }

            foreach (var b in vertex.Behaviors)
            {
                if (b.TransitionTarget != null && notedVertices.Contains(b.TransitionTarget))
                    throw new VertexValidationException(vertex, "Illegal transition to Notes.");
            }
        });
    }

    private void FindAllNotedVertices(StateMachine sm)
    {
        sm.VisitRecursively((vertex, context) =>
        {
            if (vertex is NotesVertex notesVertex)
            {
                topLevelNotes.Add(notesVertex);
                context.SkipChildren();
                vertex.VisitRecursively(v => { notedVertices.Add(v); });
                return;
            }
        });
    }

    private void RemoveTopLevelNotes()
    {
        foreach (var v in topLevelNotes)
        {
            // We know there are no illegal transitions at this point because of validations, but
            // there might be transitions between top level notes we have to worry about.
            foreach(var t in v.IncomingTransitions.ToList()) // ToList() to create copy as it is modified
            {
                t.OwningVertex.RemoveBehaviorAndUnlink(t);
            }
            v.RemoveSelf();
        }
    }

}

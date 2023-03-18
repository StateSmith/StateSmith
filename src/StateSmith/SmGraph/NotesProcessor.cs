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
    readonly List<NotesVertex> topLevelNotes = new();

    public static void Process(StateMachine sm)
    {
        var processor = new NotesProcessor();
        processor.ValidateAndRemoveNotes(sm);
    }

    public void ValidateAndRemoveNotes(StateMachine sm)
    {
        FindAllNotedVertices(sm);
        RemoveTopLevelNotes();
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
        // Temp list so that we don't modify collections we are iterating over.
        // Probably faster than copying each vertex's behavior list pre-emptively as well.
        List<Behavior> transitionsToRemove = new();

        // Gather up all transition behaviors to remove.
        // Only transitions from "non-noted" vertices to noted vertices need to be removed.
        foreach (var noted in notedVertices)
        {
            transitionsToRemove.AddRange(FindNonNotedTransitionsToNoted(noted));
        }

        Behavior.RemoveBehaviorsAndUnlink(transitionsToRemove);

        foreach (var v in topLevelNotes)
        {
            v.ForceRemoveSelf(); // auto removes any incoming transitions. helpful incase a noted vertex has an edge to a top level note.
        }
    }

    private IEnumerable<Behavior> FindNonNotedTransitionsToNoted(Vertex noted)
    {
        return noted.IncomingTransitions.Where(transition => IsNonNoted(transition.OwningVertex));
    }

    private bool IsNonNoted(Vertex v)
    {
        return v.IsContainedBy(notedVertices) == false;
    }
}

#nullable enable

using System.Collections.Generic;

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

    public static void Process(StateMachine sm)
    {
        var processor = new NotesProcessor();
        processor.ValidateAndRemoveNotes(sm);
    }

    public void ValidateAndRemoveNotes(StateMachine sm)
    {
        FindAllNotedVertices(sm);
        RemoveNoted();
    }

    private void FindAllNotedVertices(StateMachine sm)
    {
        sm.VisitRecursively((vertex, context) =>
        {
            if (vertex is NotesVertex notesVertex)
            {
                context.SkipChildren();
                vertex.VisitRecursively(v => { notedVertices.Add(v); });
                return;
            }
        });
    }

    private void RemoveNoted()
    {
        foreach (var v in notedVertices)
        {
            v.ForceRemoveSelf(); // auto removes any incoming transitions
        }
    }
}

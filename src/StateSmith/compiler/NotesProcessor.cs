using StateSmith.compiler.Visitors;
using System.Collections.Generic;
using StateSmith.compiler;

#nullable enable

namespace StateSmith.Compiling;

public class NotesProcessor : OnlyVertexVisitor
{
    int noteNestingCount = 0;
    HashSet<Vertex> notedVertices = new();
    HashSet<Vertex> nonNotedVertices = new();
    List<NotesVertex> topNotesVertices = new();

    public void ValidateAndRemoveNotes(Statemachine sm)
    {
        Visit(sm);

        EnsureNoInvalidTransitionsToNotesVertex();
        EnsureNoInvalidTransitionsFromNotesVertex();
        RemoveTopLevelNotes();
    }

    private void RemoveTopLevelNotes()
    {
        foreach (var v in topNotesVertices)
        {
            v.RemoveSelf();
        }
    }

    private void EnsureNoInvalidTransitionsToNotesVertex()
    {
        // loop over noted vertices because there are probably way fewer of them
        foreach (var v in notedVertices)
        {
            foreach (var transition in v.IncomingTransitions)
            {
                if (nonNotedVertices.Contains(transition.OwningVertex))
                {
                    throw new BehaviorValidationException(transition, "Transition from non-notes section to notes section detected.");
                }
            }
        }
    }

    private void EnsureNoInvalidTransitionsFromNotesVertex()
    {
        // loop over noted vertices because there are probably way fewer of them than non-noted vertices
        foreach (var v in notedVertices)
        {
            foreach (var transition in v.TransitionBehaviors())
            {
                if (nonNotedVertices.Contains(transition.TransitionTarget!))
                {
                    throw new BehaviorValidationException(transition, "Transition from notes section to non-notes section detected.");
                }
            }
        }
    }

    public override void Visit(Vertex v)
    {
        if (noteNestingCount == 0)
        {
            nonNotedVertices.Add(v);
        }
        else
        {
            notedVertices.Add(v);
        }

        VisitChildren(v);
    }

    public override void Visit(NotesVertex v)
    {
        noteNestingCount++;

        if (v.Behaviors.Count > 0)
        {
            throw new VertexValidationException(v, "Notes vertices cannot have any behaviors");
        }

        Visit((Vertex)v);

        noteNestingCount--;

        if (noteNestingCount == 0)
        {
            topNotesVertices.Add(v);
        }
    }
}

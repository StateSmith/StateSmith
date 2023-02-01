using StateSmith.compiler.Visitors;
using System.Collections.Generic;
using StateSmith.compiler;
using System.Linq;

#nullable enable

namespace StateSmith.Compiling;

public class NotesProcessor : OnlyVertexVisitor
{
    int noteNestingCount = 0;
    HashSet<Vertex> notedVertices = new();
    HashSet<Vertex> nonNotedVertices = new();
    List<NotesVertex> topNotesVertices = new();

    public static void Process(Statemachine sm)
    {
        var processor = new NotesProcessor();
        processor.ValidateAndRemoveNotes(sm);
        sm.UpdateNamedDescendantsMapping();
    }

    public void ValidateAndRemoveNotes(Statemachine sm)
    {
        ValidateNotesWithoutRemoving(sm);
        RemoveTopLevelNotes();
    }

    // only use for unit testing
    internal void ValidateNotesWithoutRemoving(Statemachine sm)
    {
        Visit(sm);
        EnsureNoInvalidTransitionsToNotesVertex();
        EnsureNoInvalidTransitionsFromNotesVertex();
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
                List<Vertex> all = GetAllVerticesInvolvedInTransition(transition);

                foreach (var part in all)
                {
                    if (nonNotedVertices.Contains(part))
                    {
                        throw new BehaviorValidationException(transition, "Transition from non-notes section to notes section detected.");
                    }
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
                // this section of code can be simpler as we don't have to worry about transitions to other noted states as
                // EnsureNoInvalidTransitionsToNotesVertex() will catch that.
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

        if (v.IncomingTransitions.Count > 0)
        {
            throw new VertexValidationException(v, "Notes vertices cannot have any incoming transitions");
        }

        Visit((Vertex)v);

        noteNestingCount--;

        if (noteNestingCount == 0)
        {
            topNotesVertices.Add(v);
        }
    }

    private static List<Vertex> GetAllVerticesInvolvedInTransition(Behavior transition)
    {
        var path = transition.FindTransitionPath();
        var all = path.toExit.ToList();
        all.AddRange(path.toEnter);

        if (path.leastCommonAncestor != null)
            all.Add(path.leastCommonAncestor);

        return all;
    }
}

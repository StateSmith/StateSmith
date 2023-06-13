#nullable enable
using StateSmith.Common;
using StateSmith.SmGraph.Visitors;
using System.Linq;

namespace StateSmith.SmGraph.Validation;

public class VertexValidator : VertexVisitor
{
    readonly PseudoLoopDetector pseudoLoopDetector = new();

    public override void Visit(Vertex v)
    {
        pseudoLoopDetector.CheckVertexBehaviors(v);

        foreach (var b in v.Behaviors)
        {
            ValidateBehavior(b);
        }

        VisitChildren(v);
    }

    private static void ValidateBehavior(Behavior b)
    {
        if (b.HasTransition() == false)
        {
            if (b.viaEntry != null || b.viaExit != null)
            {
                throw new VertexValidationException(b.OwningVertex, "via entry/exit can only be specified on transitions");
            }
        }
        else
        {
            ValidateViaEntry(b);
            ValidateViaExit(b);

            foreach (var trigger in b.Triggers)
            {
                if (TriggerHelper.IsEnterExitTrigger(trigger))
                {
                    throw new BehaviorValidationException(b, "A transition behavior cannot have an enter or exit trigger.");
                }
            }
        }

        DetectYedHiddenEdges(b);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/29
    /// </summary>
    /// <param name="b"></param>
    /// <exception cref="BehaviorValidationException"></exception>
    private static void DetectYedHiddenEdges(Behavior b)
    {
        if (b.IsBlankTransition() == false)
        {
            return;
        }

        if (b.TransitionTarget == b.OwningVertex)
        {
            throw new BehaviorValidationException(b, "yEd hidden self-to-self edge detected. See https://github.com/StateSmith/StateSmith/issues/29 .");
        }

        if (b.TransitionTarget.ContainsVertex(b.OwningVertex))
        {
            throw new BehaviorValidationException(b, "yEd hidden edge to ancestor detected. See https://github.com/StateSmith/StateSmith/issues/29 .");
        }
    }

    private static void ValidateViaExit(Behavior b)
    {
        string? exitLabel = b.viaExit;
        if (exitLabel == null)
        {
            return;
        }

        // find exit point with matching label
        var matchingExitPoints = b.OwningVertex.Children.OfType<ExitPoint>().Where(point => point.label == exitLabel);

        switch (matchingExitPoints.Count())
        {
            case 0: throw new BehaviorValidationException(b, $"no matching exit point found with label `{exitLabel}`.");
            case 1: break; // happy path
            default: throw new BehaviorValidationException(b, $"multiple matching exit points found with label `{exitLabel}`.");
        }
    }

    private static void ValidateViaEntry(Behavior b)
    {
        b.TransitionTarget.ThrowIfNull();

        string? entryLabel = b.viaEntry;
        if (entryLabel == null)
        {
            return;
        }

        var matchingEntryPoints = b.TransitionTarget.Children.OfType<EntryPoint>().Where(point => point.label == entryLabel);

        switch (matchingEntryPoints.Count())
        {
            case 0: throw new BehaviorValidationException(b, $"no matching entry point found with label `{entryLabel}`.");
            case 1: break; // happy path
            default: throw new BehaviorValidationException(b, $"multiple matching entry points found with label `{entryLabel}`.");
        }
    }

    public override void Visit(NotesVertex v)
    {
        // ignore
    }
}

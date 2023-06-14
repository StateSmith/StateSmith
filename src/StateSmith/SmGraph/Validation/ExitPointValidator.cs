#nullable enable
using System.Linq;

namespace StateSmith.SmGraph.Validation;

public class ExitPointValidator
{
    public static void ValidateBeforeTransformation(ExitPoint exitPoint)
    {
        PseudoStateValidator.ValidateParentAndNoChildren(exitPoint);
    }

    public static void Validate(ExitPoint exitPoint)
    {
        PseudoStateValidator.ValidateParentAndNoChildren(exitPoint);

        if (exitPoint.IncomingTransitions.Count == 0)
        {
            throw new VertexValidationException(exitPoint, "An exit point must at least one incoming transition (for now).");
        }

        PseudoStateValidator.ValidateExitingBehaviors(exitPoint);

        var duplicateLabelCount = exitPoint.SiblingsOfMyType().Where(v => v.label == exitPoint.label).Count();
        if (duplicateLabelCount > 0)
        {
            throw new VertexValidationException(exitPoint, $"Found {duplicateLabelCount} exit point(s) with duplicate label `{exitPoint.label}` within the same immediate parent.");
        }
    }
}

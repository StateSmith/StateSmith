#nullable enable
using System.Linq;

namespace StateSmith.SmGraph.Validation;

public class HistoryStateValidator
{
    public static void ValidateBeforeTransforming(HistoryVertex v)
    {
        PseudoStateValidator.ValidateParentAndNoChildren(v);
        PseudoStateValidator.ValidateBehaviors(v);

        var siblingCount = v.SiblingsOfMyType().Count();
        if (siblingCount > 0)
        {
            throw new VertexValidationException(v, $"Only 1 history vertex is allowed in a state. Found {siblingCount + 1}.");
        }

        if (v.Behaviors.Count() != 1)
        {
            throw new VertexValidationException(v, $"history vertex must only have a single default transition. Found {v.Behaviors.Count} behaviors.");
        }

        PseudoStateValidator.ValidateEnteringBehaviors(v);
    }

    public static void ValidateAfterTransforming(HistoryVertex v)
    {
        PseudoStateValidator.ValidateParentAndNoChildren(v);
        PseudoStateValidator.ValidateBehaviors(v);
        PseudoStateValidator.ValidateEnteringBehaviors(v);
    }

    //----------------------------------------------------------

    public static void ValidateBeforeTransforming(HistoryContinueVertex v)
    {
        PseudoStateValidator.ValidateParentAndNoChildren(v);

        int count = v.Behaviors.Count();
        if (count > 0)
        {
            throw new VertexValidationException(v, $"A {nameof(HistoryContinueVertex)} cannot have any behaviors. Found {count}.");
        }
    }

}


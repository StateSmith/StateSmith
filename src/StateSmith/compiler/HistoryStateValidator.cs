using System.Linq;
using StateSmith.compiler;

namespace StateSmith.Compiling;

public class HistoryStateValidator
{
    public static void Validate(HistoryVertex v)
    {
        PseudoStateValidator.ValidateParentAndNoChildren(v);

        var siblingCount = v.SiblingsOfMyType().Count();
        if (siblingCount > 0)
        {
            throw new VertexValidationException(v, $"Only 1 history vertex is allowed in a state. Found {siblingCount+1}.");
        }

        PseudoStateValidator.ValidateEnteringBehaviors(v);
    }
}

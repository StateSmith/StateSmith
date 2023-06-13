#nullable enable
using System.Linq;

namespace StateSmith.SmGraph.Validation;

public class EntryPointValidator
{
    public static void ValidateBeforeTransformation(EntryPoint v)
    {
        PseudoStateValidator.ValidateParentAndNoChildren(v);
    }

    public static void Validate(EntryPoint state)
    {
        PseudoStateValidator.ValidateParentAndNoChildren(state);
        PseudoStateValidator.ValidateEnteringBehaviors(state);

        var duplicateLabelCount = state.SiblingsOfMyType().Where(v => v.label == state.label).Count();
        if (duplicateLabelCount > 0)
        {
            throw new VertexValidationException(state, $"Found {duplicateLabelCount} entry point(s) with duplicate label `{state.label}` within the same immediate parent.");
        }
    }
}

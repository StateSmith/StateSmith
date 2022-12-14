namespace StateSmith.Compiling;

public class ExitPointValidator
{
    public static void Validate(ExitPoint v)
    {
        PseudoStateValidator.ValidateParentAndNoChildren(v);

        if (v.IncomingTransitions.Count == 0)
        {
            throw new VertexValidationException(v, "An exit point must at least one incoming transition (for now).");
        }

        PseudoStateValidator.ValdiateExitingBehaviors(v);
    }
}
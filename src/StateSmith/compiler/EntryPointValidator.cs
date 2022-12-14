namespace StateSmith.Compiling;

public class EntryPointValidator
{
    public static void Validate(EntryPoint state)
    {
        PseudoStateValidator.Validate(state);
        PseudoStateValidator.ValdiateEnteringBehaviors(state);
    }
}
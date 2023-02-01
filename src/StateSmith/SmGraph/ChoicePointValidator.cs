namespace StateSmith.SmGraph;

public class ChoicePointValidator
{
    public static void Validate(ChoicePoint state)
    {
        PseudoStateValidator.ValidateParentAndNoChildren(state);
        PseudoStateValidator.ValidateBehaviors(state);
    }
}


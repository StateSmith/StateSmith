#nullable enable
using System.Linq;

namespace StateSmith.SmGraph.Validation;

public class InitialStateValidator
{
    public static void Validate(InitialState initialState)
    {
        PseudoStateValidator.ValidateParentAndNoChildren(initialState);

        var parent = initialState.NonNullParent;

        // Ensure that containing state only has a single initial state.
        // This seems like it might be inefficient, but it isn't because we throw if more than 1 initial state
        // so any additional initial states are not visited.
        // This approach also has the added benefit of being simpler to implement. Instead of StateMachine, State, OrthoState all
        // implementing the check top down, we can do the check in one place with 1/3rd of the test code.
        var count = parent.Children<InitialState>().Count();
        if (count > 1)
        {
            throw new VertexValidationException(parent, $"A state can only have a single initial state, not {count}.");
        }

        PseudoStateValidator.ValidateEnteringBehaviors(initialState);
    }
}

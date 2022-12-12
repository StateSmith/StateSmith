using System.Linq;
using StateSmith.compiler;

namespace StateSmith.Compiling;

public class InitialStateValidator
{
    public static void Validate(InitialState initialState)
    {
        if (initialState.Children.Count > 0)
        {
            throw new VertexValidationException(initialState, "Initial state vertices cannot contain children.");
        }

        var parent = initialState.Parent;
        if (parent == null)
        {
            throw new VertexValidationException(initialState, "Initial states must have a parent state.");
        }

        // Ensure that containing state only has a single initial state.
        // This seems like it might be inefficient, but it isn't because we throw if more than 1 initial state
        // so any additional initial states are not visited.
        // This approach also has the added benefit of being simpler to implement. Instead of StateMachine, State, OrthoState all
        // implementing the check top down, we can do the check in one place with 1/3rd of the test code.
        var siblingCount = parent.Children<InitialState>().Count();
        if (siblingCount > 1)
        {
            throw new VertexValidationException(parent, $"A state can only have a single initial state, not {siblingCount}.");
        }

        //if (initialState.IncomingTransitions.Count > 0)
        //{
        //    throw new VertexValidationException(initialState, "Initial states cannot have any incoming transitions for now"); //todolow
        //}

        bool hasDefaultTransition = false;

        foreach (var b in initialState.Behaviors)
        {
            ValidateBehavior(initialState, parent: parent, b);

            if (IsSuitableForDefaultTransition(b))
            {
                hasDefaultTransition = true;
            }
        }

        if (!hasDefaultTransition)
        {
            throw new VertexValidationException(initialState, "initial state must have at least one transition without a guard clause (default transition). https://github.com/adamfk/StateSmith/issues/8");
        }
    }

    private static bool IsSuitableForDefaultTransition(Behavior b)
    {
        return b.HasTransition() && (b.HasGuardCode() == false || b.guardCode.Trim() == "true");
    }

    private static void ValidateBehavior(InitialState initialState, Vertex parent, Behavior behavior)
    {
        if (behavior.TransitionTarget == null)
        {
            throw new VertexValidationException(initialState, "Initial states must have a transition target");
        }

        if (behavior.HasAtLeastOneTrigger())
        {
            throw new VertexValidationException(initialState, "Initial states cannot have triggers"); //todolow create example of using guard to check current event
        }

        if (!parent.ContainsVertex(behavior.TransitionTarget))
        {
            throw new VertexValidationException(initialState, "Initial state transition must remain within parent");
        }

        if (behavior.TransitionTarget == parent)
        {
            throw new VertexValidationException(initialState, "Initial state transition cannot target parent");
        }

        if (behavior.TransitionTarget == initialState)
        {
            throw new VertexValidationException(initialState, "Initial state transition cannot be to self");
        }
    }
}

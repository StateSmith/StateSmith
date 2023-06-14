#nullable enable
using System;

namespace StateSmith.SmGraph.Validation;

public class PseudoStateValidator
{
    public static bool IsSuitableForDefaultTransition(Behavior b)
    {
        return b.HasTransition() && (b.HasGuardCode() == false || b.guardCode.Trim() == "true");
    }

    public static void ValidateEnteringBehaviors(PseudoStateVertex state)
    {
        bool hasDefaultTransition = false;
        var parent = state.NonNullParent;

        foreach (var b in state.Behaviors)
        {
            ValidateEnteringBehavior(state, parent: parent, b);

            if (IsSuitableForDefaultTransition(b))
            {
                hasDefaultTransition = true;
            }
        }

        if (!hasDefaultTransition)
        {
            throw new VertexValidationException(state, $"{GetTypeNameForMessage(state)} must have at least one transition without a guard clause (default transition). https://github.com/StateSmith/StateSmith/issues/59");
        }
    }

    public static void ValidateBehaviors(PseudoStateVertex state)
    {
        bool hasDefaultTransition = false;
        var parent = state.NonNullParent;

        foreach (var b in state.Behaviors)
        {
            ValidateBehavior(state, parent: parent, b);

            if (IsSuitableForDefaultTransition(b))
            {
                hasDefaultTransition = true;
            }
        }

        if (!hasDefaultTransition)
        {
            throw new VertexValidationException(state, $"{GetTypeNameForMessage(state)} must have at least one transition without a guard clause (default transition). https://github.com/StateSmith/StateSmith/issues/59");
        }
    }

    public static void ValidateExitingBehaviors(PseudoStateVertex state)
    {
        bool hasDefaultTransition = false;
        var parent = state.NonNullParent;

        foreach (var b in state.Behaviors)
        {
            ValidateExitingBehavior(state, parent: parent, b);

            if (IsSuitableForDefaultTransition(b))
            {
                hasDefaultTransition = true;
            }
        }

        if (!hasDefaultTransition)
        {
            throw new VertexValidationException(state, $"{GetTypeNameForMessage(state)} must have at least one transition without a guard clause (default transition). https://github.com/StateSmith/StateSmith/issues/59");
        }
    }

    public static void ValidateEnteringBehavior(PseudoStateVertex state, Vertex parent, Behavior behavior)
    {
        ValidateBehavior(state, parent, behavior);

        if (behavior.TransitionTarget == parent)
        {
            throw new VertexValidationException(state, $"{GetTypeNameForMessage(state)} transition cannot target parent");
        }

        if (!parent.ContainsVertex(behavior.TransitionTarget))
        {
            throw new VertexValidationException(state, $"{GetTypeNameForMessage(state)} transition must remain within parent");
        }
    }

    public static void ValidateExitingBehavior(PseudoStateVertex state, Vertex parent, Behavior behavior)
    {
        ValidateBehavior(state, parent, behavior);

        if (parent.ContainsVertex(behavior.TransitionTarget) && parent != behavior.TransitionTarget)
        {
            throw new VertexValidationException(state, $"{GetTypeNameForMessage(state)} transition must not target within parent");
        }
    }

    private static void ValidateBehavior(PseudoStateVertex state, Vertex parent, Behavior behavior)
    {
        if (behavior.TransitionTarget == null)
        {
            throw new VertexValidationException(state, $"{GetTypeNameForMessage(state)}s must have a transition target");
        }

        if (behavior.HasAtLeastOneTrigger())
        {
            throw new VertexValidationException(state, $"{GetTypeNameForMessage(state)}s cannot have triggers"); //todolow create example of using guard to check current event
        }

        if (behavior.TransitionTarget == state)
        {
            throw new VertexValidationException(state, $"{GetTypeNameForMessage(state)} transition cannot be to self");
        }
    }

    public static void ValidateParentAndNoChildren(PseudoStateVertex state)
    {
        if (state.Parent == null)
        {
            throw new VertexValidationException(state, $"{GetTypeNameForMessage(state)}s must have a parent state.");
        }

        if (state.Children.Count > 0)
        {
            throw new VertexValidationException(state, $"{GetTypeNameForMessage(state)} vertices cannot contain children.");
        }
    }

    public static string GetTypeNameForMessage(PseudoStateVertex state)
    {
        return state switch
        {
            InitialState => "Initial state",
            EntryPoint => "Entry point",
            ExitPoint => "Exit point",
            ChoicePoint => "Choice point",
            HistoryVertex => "History state",
            HistoryContinueVertex => "History continue vertex",
            _ => throw new ArgumentException("unsupported type: " + state.GetType().FullName),
        };
    }
}


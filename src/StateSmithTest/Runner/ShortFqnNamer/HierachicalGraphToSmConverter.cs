using StateSmith.SmGraph;
using System.Collections.Generic;
using System.Linq;

namespace StateSmithTest.Runner.ShortFqnNamer;

/// <summary>
/// Used to convert a graph where arrows denote ancestors into a standard HSM graph.
/// </summary>
public class HierachicalGraphToSmConverter
{
    public static void Convert(StateMachine sm)
    {
        List<State> states = new();

        // need to copy all states to a list because we will be modifying the collection
        sm.VisitTypeRecursively<State>(vertex => {
            states.Add(vertex);
        });

        foreach (var state in states)
        {
            foreach (var edge in state.IncomingTransitions.ToList())
            {
                edge.OwningVertex.ChangeParent(state);
                edge.OwningVertex.RemoveBehaviorAndUnlink(edge);
            }
        }

        sm.AddChild(new InitialState()).AddTransitionTo(sm.Children[0]); // just for validation
    }
}

using System.Collections.Generic;
using FluentAssertions;
using StateSmith.Compiling;

namespace StateSmithTest
{
    public static class VertexTestHelpers
    {
        public static void ExpectBehavior(Behavior behavior, string diagramId = null, Vertex owningVertex = null, Vertex transitionTarget = null,
                    string actionCode = null, string guardCode = null, List<string> triggers = null, double order = Behavior.DEFAULT_ORDER)
        {
            behavior.DiagramId.Should().Be(diagramId);
            behavior.OwningVertex.Should().Be(owningVertex);
            behavior.TransitionTarget.Should().Be(transitionTarget);
            behavior.actionCode.Should().Be(actionCode);
            behavior.guardCode.Should().Be(guardCode);

            triggers = triggers ?? new List<string>();
            behavior.triggers.Should().BeEquivalentTo(triggers);

            behavior.order.Should().Be(order);
        }

        public static List<string> TriggerList(params string[] triggers)
        {
            return new List<string>(triggers);
        }
    }
}

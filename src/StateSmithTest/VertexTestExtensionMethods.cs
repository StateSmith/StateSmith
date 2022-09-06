using System.Collections.Generic;
using StateSmith.Compiling;
using FluentAssertions;

namespace StateSmithTest
{
    public static class VertexTestExtensionMethods
    {
        public static void ShouldBeExactly(this Behavior behavior, string diagramId = null, Vertex owningVertex = null, Vertex transitionTarget = null,
                    string actionCode = null, string guardCode = null, List<string> triggers = null, double order = Behavior.DEFAULT_ORDER)
        {
            VertexTestHelpers.ExpectBehavior(behavior, diagramId: diagramId, owningVertex: owningVertex, transitionTarget: transitionTarget,
                actionCode: actionCode, guardCode: guardCode, triggers: triggers, order: order);
        }

        public static void ShouldHaveNoIncomingTransitions(this Vertex vertex)
        {
            vertex.IncomingTransitions.Should().BeEmpty();
        }
    }
}

using System.Collections.Generic;
using StateSmith.Compiling;
using FluentAssertions;

namespace StateSmithTest
{
    public static class VertexTestExtensionMethods
    {
        public static void ShouldBeExactly(this Behavior behavior, string diagramId = null, Vertex owningVertex = null, Vertex transitionTarget = null,
                    string actionCode = "", string guardCode = "", List<string> triggers = null, double order = Behavior.DEFAULT_ORDER)
        {
            VertexTestHelper.ExpectBehaviorExact(behavior, diagramId: diagramId, owningVertex: owningVertex, transitionTarget: transitionTarget,
                actionCode: actionCode, guardCode: guardCode, triggers: triggers, order: order);
        }

        public static void ShouldBe(this Behavior behavior, VertexTestHelper matcher, string diagramId = null, Vertex owningVertex = null, Vertex transitionTarget = null,
                    string actionCode = "", string guardCode = "", List<string> triggers = null, double order = Behavior.DEFAULT_ORDER)
        {
            matcher.ExpectBehavior(behavior, diagramId: diagramId, owningVertex: owningVertex, transitionTarget: transitionTarget,
                actionCode: actionCode, guardCode: guardCode, triggers: triggers, order: order);
        }


        public static void ShouldHaveNoIncomingTransitions(this Vertex vertex)
        {
            vertex.IncomingTransitions.Should().BeEmpty();
        }
    }
}

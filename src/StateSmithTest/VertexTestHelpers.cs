using System;
using System.Collections.Generic;
using FluentAssertions;
using StateSmith.Compiling;
using FABehaviorMatcherFunc = System.Func<FluentAssertions.Equivalency.EquivalencyAssertionOptions<StateSmith.Compiling.Behavior>, FluentAssertions.Equivalency.EquivalencyAssertionOptions<StateSmith.Compiling.Behavior>>;

namespace StateSmithTest
{
    public class BehaviorMatchingFlags
    {
        public bool diagramId = false;
        public bool owningVertex = false;
        public bool transitionTarget = true;
        public bool actionCode = true;
        public bool guardCode = true;
        public bool triggers = true;
        public bool order = false;
    }

    public class VertexTestHelper
    {
        public BehaviorMatchingFlags matchingFlags = new();
        
        public void ExpectBehavior(Behavior behavior, string diagramId = null, Vertex owningVertex = null, Vertex transitionTarget = null,
                    string actionCode = "", string guardCode = "", List<string> triggers = null, double order = Behavior.DEFAULT_ORDER)
        {
            if (matchingFlags.diagramId)
                behavior.DiagramId.Should().Be(diagramId);

            if (matchingFlags.owningVertex)
                behavior.OwningVertex.Should().Be(owningVertex);

            if (matchingFlags.transitionTarget)
                behavior.TransitionTarget.Should().Be(transitionTarget);

            if (matchingFlags.actionCode)
                behavior.actionCode.Should().Be(actionCode);

            if (matchingFlags.guardCode)
                behavior.guardCode.Should().Be(guardCode);

            if (matchingFlags.triggers)
            {
                triggers = triggers ?? new List<string>();
                behavior.triggers.Should().BeEquivalentTo(triggers);
            }

            if (matchingFlags.order)
                behavior.order.Should().Be(order);
        }

        public static FABehaviorMatcherFunc BuildFluentAssertionBehaviorMatcher(bool diagramId = false, bool owningVertex = false, bool transitionTarget = true, bool actionCode = true, bool guardCode = true, bool triggers = true, bool order = false)
        {
            FABehaviorMatcherFunc config
                = options =>
                {
                    var result = options;
                    if (diagramId) { result = result.Including(o => o.DiagramId); }
                    if (owningVertex) { result = result.Including(o => o.OwningVertex); }
                    if (transitionTarget) { result = result.Including(o => o.TransitionTarget); }
                    if (actionCode) { result = result.Including(o => o.actionCode); }
                    if (guardCode) { result = result.Including(o => o.guardCode); }
                    if (triggers) { result = result.Including(o => o.triggers); }
                    if (order) { result = result.Including(o => o.order); }

                    return result;
                };
            return config;
        }

        public static void ExpectBehaviorExact(Behavior behavior, string diagramId = null, Vertex owningVertex = null, Vertex transitionTarget = null,
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

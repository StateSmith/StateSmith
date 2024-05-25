using System.Collections.Generic;
using StateSmith.SmGraph;
using FluentAssertions;
using System;
#nullable enable

namespace StateSmithTest
{
    public static class VertexTestExtensionMethods
    {
        /// <summary>
        /// Used to indicate that a parameter should not be checked.
        /// </summary>
        private const string StringNotSpecified = "___ignore___";

        /// <summary>
        /// String parameters are checked if specified (even if null).
        /// <paramref name="owningVertex"/> is checked if not null or if <paramref name="forceOwningVertexCheck"/> set.
        /// <paramref name="triggers"/> is checked if not null or if <paramref name="forceTriggersCheck"/> set.
        /// <paramref name="transitionTarget"/> and <paramref name="order"/> are always checked.
        /// </summary>
        /// <param name="behavior"></param>
        /// <param name="diagramId">Checked if specified.</param>
        /// <param name="actionCode">Checked if specified.</param>
        /// <param name="guardCode">Checked if specified.</param>
        /// <param name="owningVertex">Checked if not null.</param>
        /// <param name="triggers">Checked if not null.</param>
        /// <param name="transitionTarget">Always checked. Defaults to null.</param>
        /// <param name="order">Always checked. Defaults to Behavior.DEFAULT_ORDER</param>
        public static void ShouldBeExactly(this Behavior behavior, 
            string diagramId = StringNotSpecified,
            string actionCode = StringNotSpecified,
            string guardCode = StringNotSpecified,
            Vertex? owningVertex = null,
            bool forceOwningVertexCheck = false,
            List<string>? triggers = null,
            bool forceTriggersCheck = false,
            Vertex? transitionTarget = null,
            double order = Behavior.DEFAULT_ORDER)
        {
            FABehaviorMatcherBuilder builder = new();

            Behavior expectedBehavior = new(owningVertex: owningVertex ?? new State("dummy"), transitionTarget: transitionTarget, diagramId: diagramId)
            {
                DiagramId = diagramId,
                actionCode = actionCode,
                guardCode = guardCode,
                order = order
            };
            expectedBehavior._triggers.AddRange(triggers ?? []);

            builder.guardCode = guardCode != StringNotSpecified;
            builder.actionCode = actionCode != StringNotSpecified;
            builder.diagramId = diagramId != StringNotSpecified;
            builder.owningVertex = owningVertex != null || forceOwningVertexCheck;
            builder.triggers = triggers != null || forceTriggersCheck;
            builder.order = true;
            builder.transitionTarget = true;

            var matcher = builder.Build();

            behavior.Should().BeEquivalentTo(expectedBehavior, matcher);
        }

        public static void ShouldHaveNoIncomingTransitions(this Vertex vertex)
        {
            vertex.IncomingTransitions.Should().BeEmpty();
        }
    }
}

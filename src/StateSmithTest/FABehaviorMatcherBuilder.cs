using FABehaviorMatcherFunc = System.Func<FluentAssertions.Equivalency.EquivalencyAssertionOptions<StateSmith.SmGraph.Behavior>, FluentAssertions.Equivalency.EquivalencyAssertionOptions<StateSmith.SmGraph.Behavior>>;

namespace StateSmithTest;

/// <summary>
/// Fluent Assertions Behavior Matcher Builder
/// </summary>
public class FABehaviorMatcherBuilder
{
    public bool diagramId = false;
    public bool owningVertex = false;
    public bool transitionTarget = true;
    public bool actionCode = true;
    public bool guardCode = true;
    public bool triggers = true;
    public bool order = false;

    public static FABehaviorMatcherFunc Build(bool diagramId = false, bool owningVertex = false, bool transitionTarget = true, bool actionCode = true, bool guardCode = true, bool triggers = true, bool order = false)
    {
        FABehaviorMatcherBuilder builder = new()
        {
            diagramId = diagramId,
            owningVertex = owningVertex,
            transitionTarget = transitionTarget,
            actionCode = actionCode,
            guardCode = guardCode,
            triggers = triggers,
            order = order
        };

        return builder.Build();
    }

    public FABehaviorMatcherFunc Build()
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
                if (triggers) { result = result.Including(o => o._triggers); }
                if (order) { result = result.Including(o => o.order); }

                return result;
            };
        return config;
    }
}

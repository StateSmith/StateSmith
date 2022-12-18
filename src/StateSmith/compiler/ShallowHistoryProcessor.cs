using StateSmith.compiler.Visitors;
using StateSmith.compiler;
using StateSmith.Common;
using System.Linq;

namespace StateSmith.Compiling
{
    public class ShallowHistoryProcessor : DummyVertexVisitor
    {
        Statemachine sm;

        public ShallowHistoryProcessor(Statemachine sm)
        {
            this.sm = sm;
        }

        public override void Visit(ShallowHistoryVertex historyState)
        {
            sm.historyStates.Add(historyState);
            historyState.stateTrackingVarName = $"{historyState.ParentState.Name}_history_state_tracking_var_name___$$$$"; // will be changed later on with expansions

            int i = 1; // save 0 for default
            var stateSiblings = historyState.Siblings<NamedVertex>().ToList();

            // historyState.Behaviors.Single().order = Behavior.ELSE_ORDER;
            Behavior defaultTransition = historyState.Behaviors.Single();
            defaultTransition.guardCode = $"{historyState.stateTrackingVarName} == 0";

            foreach (var sibling in stateSiblings)
            {
                var index = i;
                bool isDefaultTransition = sibling == defaultTransition.TransitionTarget;
                if (isDefaultTransition)
                {
                    index = 0;
                }
                else
                {
                    i++;
                }

                {
                    Behavior trackingBehavior = new Behavior(trigger:TriggerHelper.TRIGGER_ENTER, actionCode: $"{historyState.stateTrackingVarName} = {index};");
	                sibling.AddBehavior(trackingBehavior, 0);
                }

                if (!isDefaultTransition)
                {
                    Behavior transitionBehavior = new Behavior(guardCode: $"{historyState.stateTrackingVarName} == {index}", transitionTarget: sibling);
                    historyState.AddBehavior(transitionBehavior);
                }
            }

            historyState.trackedStateCount = i;
        }
    }
}

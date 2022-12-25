using StateSmith.compiler.Visitors;
using StateSmith.compiler;
using StateSmith.Common;
using System.Linq;
using System.Collections.Generic;

#nullable enable

namespace StateSmith.Compiling
{
    public class HistoryProcessor
    {
        Statemachine sm;

        public HistoryProcessor(Statemachine sm)
        {
            this.sm = sm;
        }

        public void Process()
        {
            Visit(sm);
        }

        private void Visit(NamedVertex v)
        {
            // process history continue vertex children first
            var hc = v.SingleChildOrNull<HistoryContinueVertex>();
            ProcessHistoryContinue(hc);

            var h = v.SingleChildOrNull<HistoryVertex>();
            ProcessHistory(h);

            foreach (var c in v.Children<NamedVertex>())
            {
                Visit(c);
            }

            PostProcessHistoryContinue(hc); // has to be done after kid states are processed as nested History Continue states may access this one
        }

        private static void ProcessHistoryContinue(HistoryContinueVertex? hc)
        {
            if (hc == null)
            {
                return;
            }

            var parent = hc.NonNullParent;

            var ancestorHc = parent.SingleSiblingOrNull<HistoryContinueVertex>();
            if (ancestorHc != null)
            {
                hc.historyVertices.AddRange(ancestorHc.historyVertices);
            }

            var ancestorHistory = parent.SingleSiblingOrNull<HistoryVertex>();
            if (ancestorHistory != null)
            {
                hc.historyVertices.Add(ancestorHistory);
            }

            if (hc.historyVertices.Count == 0)
            {
                throw new VertexValidationException(hc, $"HistoryContinue vertex expects to find a History and/or HistoryContinue vertex two levels up.");
            }
        }

        private void PostProcessHistoryContinue(HistoryContinueVertex? hc)
        {
            if (hc == null)
            {
                return;
            }

            var statesToTrack = hc.Siblings<NamedVertex>().ToList();

            foreach (var h in hc.historyVertices)
            {
                AddTrackingBehaviors(h, null, statesToTrack);
            }

            hc.NonNullParent.RemoveChild(hc);
        }

        private void ProcessHistory(HistoryVertex? historyState)
        {
            if (historyState == null)
            {
                return;
            }

            sm.historyStates.Add(historyState);
            historyState.stateTrackingVarName = $"{historyState.ParentState.Name}_history_state_tracking_var_name___$$$$"; // will be changed later on with expansions
            Behavior defaultTransition = historyState.Behaviors.Single();
            defaultTransition.order = Behavior.ELSE_ORDER;

            var statesToTrack = historyState.Siblings<NamedVertex>().ToList();
            AddTrackingBehaviors(historyState, defaultTransition, statesToTrack);
        }

        private static void AddTrackingBehaviors(HistoryVertex historyState, Behavior? defaultTransition, List<NamedVertex> statesToTrack)
        {
            foreach (var stateToTrack in statesToTrack)
            {
                var index = historyState.Behaviors.Count;
                bool isDefaultTransition = stateToTrack == defaultTransition?.TransitionTarget;
                if (isDefaultTransition)
                {
                    index = 0;
                }

                {
                    Behavior trackingBehavior = new Behavior(trigger: TriggerHelper.TRIGGER_ENTER, actionCode: $"{historyState.stateTrackingVarName} = {index};");
                    stateToTrack.AddBehavior(trackingBehavior);
                }

                if (!isDefaultTransition)
                {
                    Behavior transitionBehavior = new Behavior(guardCode: $"{historyState.stateTrackingVarName} == {index}", transitionTarget: stateToTrack);
                    historyState.AddBehavior(transitionBehavior);
                }
            }
        }
    }
}

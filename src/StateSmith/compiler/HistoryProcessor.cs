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

        public static void ValidateHistoryState(HistoryVertex h)
        {
            int count = h.TransitionBehaviors().Count();
            if (count > 1)
            {
                throw new VertexValidationException(h, $"A history state can only have a single drawn transition for its default. Found {count}.");
            }
        }

        private void Visit(NamedVertex v)
        {
            // process history continue vertex children first
            var hc = v.SingleChildOrNull<HistoryContinueVertex>();
            if (hc != null)
            {
                ProcessHistoryContinue(hc);
            }

            var h = v.SingleChildOrNull<HistoryVertex>();
            ProcessHistory(h);

            foreach (var c in v.Children<NamedVertex>())
            {
                Visit(c);
            }

            if (hc != null)
            {
                // has to be done after kid states are visited as nested History Continue states may access this one
                hc.RemoveSelf();
            }
        }

        private void ProcessHistoryContinue(HistoryContinueVertex hc)
        {
            HistoryStateValidator.ValidateBeforeTransforming(hc);

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

            AddHistoryContinueBehaviors(hc);
        }

        private void AddHistoryContinueBehaviors(HistoryContinueVertex hc)
        {
            var statesToTrack = hc.Siblings<NamedVertex>().ToList();

            foreach (var h in hc.historyVertices)
            {
                AddTrackingBehaviors(h, null, statesToTrack);
            }
        }

        private void ProcessHistory(HistoryVertex? historyState)
        {
            if (historyState == null)
            {
                return;
            }

            HistoryStateValidator.ValidateBeforeTransforming(historyState);

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
                    Behavior enterTrackingBehavior = new Behavior(trigger: TriggerHelper.TRIGGER_ENTER, actionCode: $"{historyState.stateTrackingVarName} = {index};");
                    stateToTrack.AddBehavior(enterTrackingBehavior);
                }

                if (!isDefaultTransition)
                {
                    Behavior historyTransitionBehavior = new Behavior(guardCode: $"{historyState.stateTrackingVarName} == {index}", transitionTarget: stateToTrack);
                    historyState.AddBehavior(historyTransitionBehavior);
                }
            }
        }
    }
}

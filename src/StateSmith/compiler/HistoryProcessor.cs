using StateSmith.compiler.Visitors;
using StateSmith.compiler;
using StateSmith.Common;
using System.Linq;
using System.Collections.Generic;

#nullable enable

namespace StateSmith.Compiling
{
    public class HistoryProcessor : DummyVertexVisitor
    {
        Statemachine sm;
        List<HistoryContinueVertex> historyContinueVertices = new();


        public HistoryProcessor(Statemachine sm)
        {
            this.sm = sm;
        }

        public void PostProcess()
        {
            foreach (var hc in historyContinueVertices)
            {
                PostProcessHistoryContinue(hc);
            }
        }

        public void PostProcessHistoryContinue(HistoryContinueVertex hc)
        {
            var statesToTrack = hc.Siblings<NamedVertex>().ToList();

            foreach (var h in hc.historyVertices)
            {
                HistoryProcessor.TrackStates(h, null, statesToTrack);
            }

            hc.ParentState.RemoveChild(hc);
        }

        public override void Visit(HistoryVertex historyState)
        {
            sm.historyStates.Add(historyState);
            historyState.stateTrackingVarName = $"{historyState.ParentState.Name}_history_state_tracking_var_name___$$$$"; // will be changed later on with expansions
            Behavior defaultTransition = historyState.Behaviors.Single();
            defaultTransition.order = Behavior.ELSE_ORDER;

            var statesToTrack = historyState.Siblings<NamedVertex>().ToList();
            TrackStates(historyState, defaultTransition, statesToTrack);
        }

        public static void TrackStates(HistoryVertex historyState, Behavior? defaultTransition, List<NamedVertex> statesToTrack)
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
                    stateToTrack.AddBehavior(trackingBehavior, 0);
                }

                if (!isDefaultTransition)
                {
                    Behavior transitionBehavior = new Behavior(guardCode: $"{historyState.stateTrackingVarName} == {index}", transitionTarget: stateToTrack);
                    historyState.AddBehavior(transitionBehavior);
                }
            }
        }

        public override void Visit(HistoryContinueVertex hc)
        {
            FindTrackingHistoryStates(hc, hc);
            historyContinueVertices.Add(hc);
        }

        private static void FindTrackingHistoryStates(HistoryContinueVertex hc, Vertex? node)
        {
            node = node?.Parent?.Parent;    // consider change to single parent up and then search siblings

            while (node != null)
            {
                hc.historyVertices.AddRange(node.Children<HistoryVertex>());

                // fixme. stop upwards recursion if no history or history continue found.

                foreach (var v in node.Children<HistoryContinueVertex>())
                {
                    FindTrackingHistoryStates(hc, v);
                }

                node = node?.Parent?.Parent;
            }
        }
    }
}

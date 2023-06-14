using StateSmith.Common;
using System.Linq;
using System.Collections.Generic;
using StateSmith.Output.Algos.Balanced1;
using StateSmith.Output;
using System.Text.RegularExpressions;
using StateSmith.Output.Gil;
using StateSmith.SmGraph.Validation;

#nullable enable

namespace StateSmith.SmGraph;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/63
/// </summary>
public class HistoryProcessor
{
    readonly NameMangler mangler;
    readonly IExpansionVarsPathProvider expansionVarsPathProvider;
    private StateMachine? sm;

    public HistoryProcessor(NameMangler mangler, IExpansionVarsPathProvider expansionVarsPathProvider)
    {
        this.mangler = mangler;
        this.expansionVarsPathProvider = expansionVarsPathProvider;
    }

    public void Process(StateMachine sm)
    {
        this.sm = sm;
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
        var statesToTrack = GetStatesToTrack(hc);

        foreach (var h in hc.historyVertices)
        {
            AddTrackingBehaviors(h, null, statesToTrack);
        }
    }

    private static List<NamedVertex> GetStatesToTrack(Vertex v)
    {
        return v.Siblings<NamedVertex>().Where(s => s.IncomingTransitions.Any()).ToList();
    }

    private void ProcessHistory(HistoryVertex? historyState)
    {
        if (historyState == null)
        {
            return;
        }

        HistoryStateValidator.ValidateBeforeTransforming(historyState);

        sm.ThrowIfNull().historyStates.Add(historyState);
        historyState.stateTrackingVarName = mangler.HistoryVarName(historyState);
        Behavior defaultTransition = historyState.Behaviors.Single();
        defaultTransition.order = Behavior.ELSE_ORDER;

        var statesToTrack = GetStatesToTrack(historyState);
        AddTrackingBehaviors(historyState, defaultTransition, statesToTrack);
    }

    private void AddTrackingBehaviors(HistoryVertex historyState, Behavior? defaultTransition, List<NamedVertex> statesToTrack)
    {
        foreach (var stateToTrack in statesToTrack)
        {
            bool isDefaultTransition = stateToTrack == defaultTransition?.TransitionTarget && defaultTransition.HasActionCode() == false;

            string expansionVarsPath = expansionVarsPathProvider.ExpansionVarsPath;
            expansionVarsPath = new Regex(@"(\w+).*?(\w+)").Replace(expansionVarsPath, "this.$2"); // this converts `sm->vars` (if C99 selected) to `this.vars` for GIL code.

            string enumName = mangler.HistoryVarEnumType(historyState);
            string enumValueName = enumName + "." + mangler.HistoryVarEnumValue(historyState, stateToTrack);

            {
                string actionCode = $"{expansionVarsPath}{historyState.stateTrackingVarName} = {enumValueName};";
                actionCode = GilCreationHelper.MarkAsGilExpansionCode(actionCode);

                Behavior enterTrackingBehavior = new(trigger: TriggerHelper.TRIGGER_ENTER, actionCode: actionCode);
                stateToTrack.AddBehavior(enterTrackingBehavior);
            }

            if (!isDefaultTransition)
            {
                string guardCode = $"{expansionVarsPath}{historyState.stateTrackingVarName} == {enumValueName}";
                guardCode = GilCreationHelper.MarkAsGilExpansionCode(guardCode);

                Behavior historyTransitionBehavior = new(guardCode: guardCode, transitionTarget: stateToTrack);
                historyState.AddBehavior(historyTransitionBehavior);
            }
        }
    }
}

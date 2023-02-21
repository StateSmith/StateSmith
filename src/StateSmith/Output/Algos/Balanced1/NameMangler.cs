using StateSmith.Common;
using StateSmith.SmGraph;
using System.Text.RegularExpressions;
using System;

#nullable enable

namespace StateSmith.Output.Algos.Balanced1;

public class NameMangler
{
    protected StateMachine? sm;

    public NameMangler()
    {

    }

    public NameMangler(StateMachine sm)
    {
        this.sm = sm;
    }

    public void SetStateMachine(StateMachine sm)
    {
        this.sm = sm;
    }

    public virtual string SmName => sm.ThrowIfNull().Name;

    //--------------------------------------------------------
    //--------------------------------------------------------
    //--------------------------------------------------------


    public virtual string SmEventEnum => $"EventId";

    /// <summary>
    /// Set to `__attribute__((packed)) ` for gcc if you want smallest possible enum
    /// </summary>
    public virtual string SmEventEnumAttribute => $"";

    public virtual string SmEventEnumValue(string evt) => $"{evt.ToUpper()}";

    public virtual string SmEventEnumCount => $"{SmEventEnum}Count";

    //--------------------------------------------------------
    //--------------------------------------------------------
    //--------------------------------------------------------


    #region StateEnum

    public virtual string SmStateEnum => $"StateId";

    /// <summary>
    /// Set to `__attribute__((packed)) ` for gcc if you want smallest possible enum
    /// </summary>
    public virtual string SmStateEnumAttribute => $"";

    public virtual string SmStateEnumValue(NamedVertex namedVertex)
    {
        string stateName = SmStateName(namedVertex);
        return stateName.ToUpper();
    }

    public virtual string SmStateName(NamedVertex namedVertex)
    {
        return namedVertex.Parent == null ? "ROOT" : namedVertex.Name.ToUpper();
    }

    public virtual string SmStateEnumCount => $"{SmStateEnum}Count";

    #endregion StateEnum

    //--------------------------------------------------------
    //--------------------------------------------------------
    //--------------------------------------------------------

    public virtual string SmStructName => SmName;

    public virtual string SmStructTypedefName => SmStructName;

    public virtual string SmFuncTypedef => $"Func";

    public virtual string SmFuncStart => $"start";
    public virtual string SmFuncDispatchEvent => $"dispatch_event";

    public virtual string SmFuncExitUpTo => $"exit_up_to_state_handler";

    public virtual string SmFuncToString => $"state_id_to_string";
    public virtual string SmStateToString(NamedVertex state) => $"{SmStateName(state).ToUpper()}";

    public virtual string HistoryVarName(HistoryVertex historyVertex) => $"{historyVertex.ParentState!.Name}_history";
    public virtual string HistoryVarEnumName(HistoryVertex historyVertex) => $"{historyVertex.ParentState!.Name}_HistoryId";
    public virtual string HistoryVarEnumValueName(HistoryVertex historyVertex, Vertex transitionTarget)
    {
        var description = Vertex.Describe(transitionTarget);
        description = Regex.Replace(description, @"[().]", "");
        return description;
    }

    public virtual string SmFuncTriggerHandler(NamedVertex state, string triggerName)
    {
        return $"{SmStateName(state)}_{triggerName.ToLower()}";
    }
}

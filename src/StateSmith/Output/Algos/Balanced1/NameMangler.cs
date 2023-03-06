using StateSmith.Common;
using StateSmith.SmGraph;
using System.Text.RegularExpressions;

#nullable enable

namespace StateSmith.Output.Algos.Balanced1;

public class NameMangler
{
    protected StateMachine? _sm;
    public StateMachine Sm => _sm.ThrowIfNull();

    public NameMangler()
    {

    }

    public NameMangler(StateMachine sm)
    {
        this._sm = sm;
    }

    public void SetStateMachine(StateMachine sm)
    {
        this._sm = sm;
    }

    /// <summary>
    /// File name without dot or extension
    /// </summary>
    public virtual string BaseFileName => Sm.Name;

    //--------------------------------------------------------
    //--------------------------------------------------------
    //--------------------------------------------------------


    public virtual string SmEventEnumType => "EventId";

    /// <summary>
    /// FIXME: support with post processing markers. Set to `__attribute__((packed)) ` for gcc if you want smallest possible enum
    /// </summary>
    public virtual string SmEventEnumAttribute => "";

    public virtual string SmEventEnumValue(string evt) => evt.ToUpper();

    public virtual string SmEventEnumCount => $"{SmEventEnumType}Count";

    /// <summary>
    /// The name of the function that translates a state machine event ID to a string literal.
    /// </summary>
    public virtual string SmEventIdToStringFuncName => "event_id_to_string";

    /// <summary>
    /// Stringifies an event ID for <see cref="SmEventIdToStringFuncName"/>.
    /// </summary>
    public virtual string SmEventIdToString(string eventId) => SmEventEnumValue(eventId);

    //--------------------------------------------------------
    //--------------------------------------------------------
    //--------------------------------------------------------

    #region StateEnum

    public virtual string SmStateEnumType => "StateId";

    /// <summary>
    /// FIXME: support with post processing markers. Set to `__attribute__((packed)) ` for gcc if you want smallest possible enum
    /// </summary>
    public virtual string SmStateEnumAttribute => "";

    public virtual string SmStateEnumValue(NamedVertex namedVertex)
    {
        string stateName = SmStateName(namedVertex);
        return stateName.ToUpper();
    }

    public virtual string SmStateName(NamedVertex namedVertex)
    {
        return namedVertex.Parent == null ? "ROOT" : namedVertex.Name.ToUpper();
    }

    public virtual string SmStateEnumCount => $"{SmStateEnumType}Count";

    /// <summary>
    /// The name of the function that translates a state machine state ID to a string literal.
    /// </summary>
    public virtual string SmStateIdToStringFuncName => "state_id_to_string";

    /// <summary>
    /// Stringifies an state ID for <see cref="SmStateIdToStringFuncName"/>.
    /// </summary>
    public virtual string SmStateToString(NamedVertex state) => SmStateName(state).ToUpper();

    #endregion StateEnum

    //--------------------------------------------------------
    //--------------------------------------------------------
    //--------------------------------------------------------

    public virtual string SmTypeName => Sm.Name;

    public virtual string SmHandlerFuncType => "Func";

    public virtual string SmStartFuncName => "start";
    public virtual string SmDispatchEventFuncName => "dispatch_event";

    public virtual string SmExitUpToFuncName => "exit_up_to_state_handler";

    //--------------------------------------------------------
    //--------------------------------------------------------
    //--------------------------------------------------------

    public virtual string HistoryVarName(HistoryVertex historyVertex) => $"{historyVertex.ParentState!.Name}_history";
    public virtual string HistoryVarEnumType(HistoryVertex historyVertex) => $"{historyVertex.ParentState!.Name}_HistoryId";
    public virtual string HistoryVarEnumValue(HistoryVertex historyVertex, Vertex transitionTarget)
    {
        var description = Vertex.Describe(transitionTarget);
        description = Regex.Replace(description, @"[().]", "");
        return description;
    }

    public virtual string SmTriggerHandlerFuncName(NamedVertex state, string triggerName)
    {
        return $"{SmStateName(state)}_{triggerName.ToLower()}";
    }

    //------------------------------------------------------

    /// <summary>
    /// Name of constructor function. Only used for langauges (like 'C') that lack actual constructors.
    /// </summary>
    public virtual string SmFuncCtor => "ctor"; // this needs to be changed
}

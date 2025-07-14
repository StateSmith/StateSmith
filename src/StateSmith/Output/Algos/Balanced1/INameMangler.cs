
using StateSmith.SmGraph;

namespace StateSmith.Output.Algos.Balanced1;

public interface INameMangler
{
    StateMachine Sm { get; }
    void SetStateMachine(StateMachine sm);
    string BaseFileName { get; }
    string SmEventEnumType { get; }
    string SmEventEnumValue(string evt);
    string SmEventEnumCount { get; }
    string SmEventIdToStringFuncName { get; }
    string SmEventIdToString(string eventId);
    string SmStateEnumType { get; }
    string SmStateIdVarName { get; }
    string SmStateEnumValue(NamedVertex namedVertex);
    string SmQualifiedStateEnumValue(NamedVertex namedVertex);
    string SmStateName(NamedVertex namedVertex);
    string SmStateEnumCount { get; }
    string SmStateIdToStringFuncName { get; }
    string SmStateToString(NamedVertex state);
    string SmTypeName { get; }
    string SmHandlerFuncType { get; }
    string SmStartFuncName { get; }
    string SmDispatchEventFuncName { get; }
    string SmExitUpToFuncName { get; }
    string HistoryVarNamePostfix { get; }
    string HistoryVarName(HistoryVertex historyVertex);
    string HistoryVarEnumTypePostfix { get; }
    string HistoryVarEnumType(HistoryVertex historyVertex);
    string HistoryVarEnumValue(HistoryVertex historyVertex, Vertex transitionTarget);
    string SmTriggerHandlerFuncName(NamedVertex state, string triggerName);
    string SmFuncCtor { get; }
    string SmAncestorEventHandlerVarName { get; }
    string SmCurrentEventHandlersVarName { get; }
    string SmCurrentStateExitHandlerVarName { get; }
    string MangleVarName(string snakeCaseName);
}

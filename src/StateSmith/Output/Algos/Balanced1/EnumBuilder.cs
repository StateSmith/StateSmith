using StateSmith.SmGraph;
using System.Collections.Generic;
using System.Linq;
using StateSmith.Common;
using StateSmith.Runner;

#nullable enable

namespace StateSmith.Output.Algos.Balanced1;

public class EnumBuilder
{
    readonly NameMangler mangler;
    readonly IStateMachineProvider stateMachineProvider;

    public EnumBuilder(NameMangler mangler, IStateMachineProvider stateMachineProvider)
    {
        this.mangler = mangler;
        this.stateMachineProvider = stateMachineProvider;
    }

    public void OutputEventIdCode(OutputFile file)
    {
        file.Append($"public enum {mangler.SmEventEnumType}");
        file.StartCodeBlock();
        List<string> nonDoEvents = GetNonDoEvents(out var hadDoEvent);

        int enumOffset = 0;

        if (hadDoEvent)
        {
            enumOffset = 1;
            file.AppendLine($"{mangler.SmEventEnumValue(TriggerHelper.TRIGGER_DO)} = 0, // The `do` event is special. State event handlers do not consume this event (ancestors all get it too) unless a transition occurs.");
        }

        for (int i = 0; i < nonDoEvents.Count; i++)
        {
            string evt = nonDoEvents[i];
            file.AppendLine($"{mangler.SmEventEnumValue(evt)} = {i + enumOffset},");
        }

        file.FinishCodeBlock("");
        file.AppendLine();

        OutputEventIdCount(file, nonDoEvents.Count + enumOffset);
    }

    StateMachine GetSm()
    {
        return stateMachineProvider.GetStateMachine();
    }

    private List<string> GetNonDoEvents(out bool hadDoEvent)
    {
        var nonDoEvents = GetSm().GetEventListCopy();
        hadDoEvent = nonDoEvents.RemoveAll((e) => TriggerHelper.IsDoEvent(e)) > 0;
        return nonDoEvents;
    }

    protected void OutputEventIdCount(OutputFile file, int count)
    {
        var enumValueName = mangler.SmEventEnumCount;
        OutputEnumCount(file, enumValueName, count);
    }

    public void OutputStateIdCode(OutputFile file)
    {
        string smName = GetSm().Name;

        file.Append($"public enum {mangler.SmStateEnumType}");
        file.StartCodeBlock();

        var namedVertices = GetSm().GetNamedVerticesCopy();
        for (int i = 0; i < namedVertices.Count; i++)
        {
            NamedVertex namedVertex = namedVertices[i];
            file.AppendLine($"{mangler.SmStateEnumValue(namedVertex)} = {i},");
        }

        file.FinishCodeBlock("");
        file.AppendLine();

        OutputStateIdCount(file, smName, namedVertices.Count);
    }

    protected void OutputStateIdCount(OutputFile file, string smName, int count)
    {
        var enumValueName = mangler.SmStateEnumCount;
        OutputEnumCount(file, enumValueName, count);
    }

    public void OutputHistoryIdCode(OutputFile file, HistoryVertex historyVertex)
    {
        file.Append($"public enum {mangler.HistoryVarEnumType(historyVertex)}");
        file.StartCodeBlock();

        // default behavior is the last one
        Behavior defaultBehavior = historyVertex.Behaviors.Last();
        file.AppendLine($"{mangler.HistoryVarEnumValue(historyVertex, defaultBehavior.TransitionTarget.ThrowIfNull())} = 0, // default transition");

        for (int i = 0; i < historyVertex.Behaviors.Count - 1; i++)
        {
            var b = historyVertex.Behaviors[i];
            file.AppendLine($"{mangler.HistoryVarEnumValue(historyVertex, b.TransitionTarget.ThrowIfNull())} = {i + 1},");
        }

        file.FinishCodeBlock("");
        file.FinishLine();
    }

    private static void OutputEnumCount(OutputFile file, string enumValueName, int count)
    {
        file.AppendLine($"public const int {enumValueName} = {count};");
    }
}

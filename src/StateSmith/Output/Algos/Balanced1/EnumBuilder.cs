using StateSmith.SmGraph;
using System.Collections.Generic;
using System.Linq;
using StateSmith.Common;
using StateSmith.Runner;

#nullable enable

namespace StateSmith.Output.Algos.Balanced1;

public class EnumBuilder
{
    private readonly NameMangler mangler;
    private readonly IStateMachineProvider stateMachineProvider;
    private readonly AlgoBalanced1Settings settings;

    public EnumBuilder(NameMangler mangler, IStateMachineProvider stateMachineProvider, AlgoBalanced1Settings settings)
    {
        this.mangler = mangler;
        this.stateMachineProvider = stateMachineProvider;
        this.settings = settings;
    }

    public void OutputEventIdCode(OutputFile file)
    {
        file.AppendIndented($"public enum {mangler.SmEventEnumType}");
        file.StartCodeBlock();
        List<string> nonDoEvents = GetNonDoEvents(out var hadDoEvent);

        int enumOffset = 0;

        if (hadDoEvent)
        {
            enumOffset = 1;
            file.AppendIndentedLine($"{mangler.SmEventEnumValue(TriggerHelper.TRIGGER_DO)} = 0, // The `do` event is special. State event handlers do not consume this event (ancestors all get it too) unless a transition occurs.");
        }

        for (int i = 0; i < nonDoEvents.Count; i++)
        {
            string evt = nonDoEvents[i];
            file.AppendIndentedLine($"{mangler.SmEventEnumValue(evt)} = {i + enumOffset},");
        }

        file.FinishCodeBlock("");
        file.AppendIndentedLine();

        if (settings.outputEnumMemberCount)
        {
            OutputEventIdCount(file, nonDoEvents.Count + enumOffset);
        }
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
        SubtreeIdMapper mapper = new();
        Dictionary<NamedVertex, SubtreeIdMapper.SubtreeData> subtreeDataMap = mapper.MapSubtree(GetSm());

        file.AppendIndented($"public enum {mangler.SmStateEnumType}");
        file.StartCodeBlock();

        List<NamedVertex> orderedNamedVertices = GetSm().GetOrderedNamedVerticesCopy();
        foreach (var namedVertex in orderedNamedVertices)
        {
            var subtreeData = subtreeDataMap[namedVertex];
            file.AppendIndentedLine($"{mangler.SmStateEnumValue(namedVertex)} = {subtreeData.id},");
        }

        file.FinishCodeBlock("");
        file.AppendIndentedLine();

        OutputStateIdCount(file, orderedNamedVertices.Count);
        OutputSubtreeEndIds(file, orderedNamedVertices, subtreeDataMap);
    }

    protected void OutputStateIdCount(OutputFile file, int count)
    {
        if (!settings.outputEnumMemberCount)
            return;

        var enumValueName = mangler.SmStateEnumCount;
        OutputEnumCount(file, enumValueName, count);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/538
    /// </summary>
    protected void OutputSubtreeEndIds(OutputFile file, List<NamedVertex> orderedVertices, Dictionary<NamedVertex, SubtreeIdMapper.SubtreeData> subtreeDataMap)
    {
        if (!settings.outputSubtreeEndIds)
            return;
        
        file.AppendIndentedLine($"");
        file.AppendIndentedLine($"// Subtree meta data generation can be disabled in settings.");
        file.AppendIndentedLine($"// Details: https://github.com/StateSmith/StateSmith/issues/538");

        foreach (var namedVertex in orderedVertices)
        {
            var subtreeData = subtreeDataMap[namedVertex];
            // file.AppendIndentedLine($"public const int {mangler.SmStateEnumValue(namedVertex)}_SubtreeEndId = (int){mangler.SmStateEnumType}.{mangler.SmStateEnumValue(subtreeData.subtreeEndVertex)};"); // this is nicer, but doesn't work with all languages
            file.AppendIndentedLine($"public const int {mangler.SmStateEnumValue(namedVertex)}_SubtreeEndId = {subtreeData.subtreeEndId};  // State '{namedVertex.Name}' subtree extends from itself (id: {subtreeData.id}) to state '{subtreeData.subtreeEndVertex.Name}' (id: {subtreeData.subtreeEndId})");
        }
    }

    public void OutputHistoryIdCode(OutputFile file, HistoryVertex historyVertex)
    {
        file.AppendIndented($"public enum {mangler.HistoryVarEnumType(historyVertex)}");
        file.StartCodeBlock();

        // default behavior is the last one
        Behavior defaultBehavior = historyVertex.Behaviors.Last();
        file.AppendIndentedLine($"{mangler.HistoryVarEnumValue(historyVertex, defaultBehavior.TransitionTarget.ThrowIfNull())} = 0, // default transition");

        for (int i = 0; i < historyVertex.Behaviors.Count - 1; i++)
        {
            var b = historyVertex.Behaviors[i];
            file.AppendIndentedLine($"{mangler.HistoryVarEnumValue(historyVertex, b.TransitionTarget.ThrowIfNull())} = {i + 1},");
        }

        file.FinishCodeBlock("");
        file.FinishLine();
    }

    private static void OutputEnumCount(OutputFile file, string enumValueName, int count)
    {
        file.AppendIndentedLine($"public const int {enumValueName} = {count};");
    }
}

using StateSmith.SmGraph;
using System.Linq;
using StateSmith.Common;
using StateSmith.Runner;
using StateSmith.Output.Gil;
using System.Globalization;

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

        int i = 0;
        EventMapping eventMapping = GetSm()._eventMapping.ThrowIfNull();
        foreach (var eventName in eventMapping.OrderedSanitizedEvents)
        {
            AppendEventEnumMember(file, ref i, eventMapping, eventName);
        }

        file.FinishCodeBlock("");
        file.AppendIndentedLine();

        if (settings.outputEnumMemberCount)
        {
            OutputEventIdCount(file, eventMapping.OrderedSanitizedEvents.Count);
        }
    }

    private void AppendEventEnumMember(OutputFile file, ref int i, EventMapping eventMapping, string eventName)
    {
        string enumValueText = GenerateEnumValueText(i, eventMapping, eventName);
        i++;

        file.AppendIndented($"{mangler.SmEventEnumValue(eventName)} = {enumValueText},");

        if (TriggerHelper.IsDoEvent(eventName))
        {
            file.AppendWithoutIndent($" // The `do` event is special. State event handlers do not consume this event (ancestors all get it too) unless a transition occurs.");
        }
        file.FinishLine();
    }

    private static string GenerateEnumValueText(int i, EventMapping eventMapping, string eventName)
    {
        string enumValueText;

        // This may be a number or an external user specified value like "SYSTEM_EV1" or "SystemIds::Ev1".
        // https://github.com/StateSmith/StateSmith/issues/470
        var maybeExternalValue = eventMapping.GetEventValue(eventName);

        // We could always use the post-processor, but I'd like to avoid it if possible.
        // Would like some better testing before enabling this everywhere. Will maybe remove in the future.
        bool isSimpleNumber = int.TryParse(maybeExternalValue, NumberStyles.None, CultureInfo.InvariantCulture, out var _);
        if (isSimpleNumber)
        {
            enumValueText = maybeExternalValue; // it's a number, so we can use it directly.
        }
        else
        {
            // Event value is an external user specified value. We need to use the post-processor
            // because the transpiler wants to see valid C# enum values.
            enumValueText = $"{PostProcessor.AddCommentToReplaceNextWordWith(maybeExternalValue)}{i}";
            // We use `i` above because transpiler still wants to see valid and unique enum values.
            // The `i` gets replaced by the post-processor with the actual value from the event mapping.
        }

        return enumValueText;
    }

    StateMachine GetSm()
    {
        return stateMachineProvider.GetStateMachine();
    }

    protected void OutputEventIdCount(OutputFile file, int count)
    {
        var enumValueName = mangler.SmEventEnumCount;
        OutputEnumCount(file, enumValueName, count);
    }

    public void OutputStateIdCode(OutputFile file)
    {
        string smName = GetSm().Name;

        file.AppendIndented($"public enum {mangler.SmStateEnumType}");
        file.StartCodeBlock();

        var namedVertices = GetSm().GetNamedVerticesCopy();
        for (int i = 0; i < namedVertices.Count; i++)
        {
            NamedVertex namedVertex = namedVertices[i];
            file.AppendIndentedLine($"{mangler.SmStateEnumValue(namedVertex)} = {i},");
        }

        file.FinishCodeBlock("");
        file.AppendIndentedLine();

        OutputStateIdCount(file, smName, namedVertices.Count);
    }

    protected void OutputStateIdCount(OutputFile file, string smName, int count)
    {
        if (!settings.outputEnumMemberCount)
            return;

        var enumValueName = mangler.SmStateEnumCount;
        OutputEnumCount(file, enumValueName, count);
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

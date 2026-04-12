#nullable enable

using StateSmith.SmGraph;

namespace StateSmith.Output.Algos.Balanced1;

public class AlgoEventIdToString : IAlgoEventIdToString
{
    protected readonly NameMangler mangler;

    public AlgoEventIdToString(NameMangler mangler)
    {
        this.mangler = mangler;
    }

    public void CreateEventIdToStringFunction(OutputFile file, StateMachine sm)
    {
        file.AppendIndentedLine("// Thread safe. This function can be disabled with `outputEventIdToStringFunction` setting.");
        file.AppendIndented($"public static string {mangler.SmEventIdToStringFuncName}({mangler.SmEventEnumType} id)");
        file.StartCodeBlock();
        {
            file.AppendIndented("switch (id)");
            file.StartCodeBlock();
            {
                foreach (var eventId in sm.GetEventListCopy())
                {
                    file.AppendIndentedLine($"case {mangler.SmEventEnumType}.{mangler.SmEventEnumValue(eventId)}: return \"{mangler.SmEventIdToString(eventId)}\";");
                }
                file.AppendIndentedLine("default: return \"?\";");
            }
            file.FinishCodeBlock(forceNewLine: true);
        }
        file.FinishCodeBlock(forceNewLine: true);
        file.AppendIndentedLine();
    }
}

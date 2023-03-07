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
        file.AppendLine("// Thread safe.");
        file.Append($"public static string {mangler.SmEventIdToStringFuncName}({mangler.SmEventEnumType} id)");
        file.StartCodeBlock();
        {
            file.Append("switch (id)");
            file.StartCodeBlock();
            {
                foreach (var eventId in sm.GetEventListCopy())
                {
                    file.AppendLine($"case {mangler.SmEventEnumType}.{mangler.SmEventEnumValue(eventId)}: return \"{mangler.SmEventIdToString(eventId)}\";");
                }
                file.AppendLine("default: return \"?\";");
            }
            file.FinishCodeBlock(forceNewLine: true);
        }
        file.FinishCodeBlock(forceNewLine: true);
        file.AppendLine();
    }
}

#nullable enable

using StateSmith.SmGraph;

namespace StateSmith.Output.Algos.Balanced1;

public class AlgoEventIdFuncGenerator : IAlgoEventIdFuncGenerator
{
    protected readonly NameMangler mangler;

    public AlgoEventIdFuncGenerator(NameMangler mangler)
    {
        this.mangler = mangler;
    }

    public void CreateEventIdToStringFunction(OutputFile file, StateMachine sm)
    {
        file.AppendIndentedLine("// Thread safe.");
        file.AppendIndentedLine("// There is a setting available to disable generating this function.");
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

    public void CreateEventIdIsValidFunction(OutputFile file, StateMachine sm)
    {
        file.AppendIndentedLine("// Thread safe.");
        file.AppendIndentedLine("// There is a setting available to disable generating this function.");
        file.AppendIndented($"public static bool {mangler.SmEventIdIsValidFuncName}({mangler.SmEventEnumType} id)");
        file.StartCodeBlock();
        {
            file.AppendIndented("switch (id)");
            file.StartCodeBlock();
            {
                foreach (var eventId in sm.GetEventListCopy())
                {
                    file.AppendIndentedLine($"case {mangler.SmEventEnumType}.{mangler.SmEventEnumValue(eventId)}: return true;");
                }
                file.AppendIndentedLine("default: return false;");
            }
            file.FinishCodeBlock(forceNewLine: true);
        }
        file.FinishCodeBlock(forceNewLine: true);
        file.AppendIndentedLine();
    }
}

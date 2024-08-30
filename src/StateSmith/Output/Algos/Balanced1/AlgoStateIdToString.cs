#nullable enable

using StateSmith.SmGraph;

namespace StateSmith.Output.Algos.Balanced1;

public class AlgoStateIdToString : IAlgoStateIdToString
{
    protected readonly NameMangler mangler;

    public AlgoStateIdToString(NameMangler mangler)
    {
        this.mangler = mangler;
    }

    public void CreateStateIdToStringFunction(OutputFile file, StateMachine sm)
    {
        file.AppendLine("// Thread safe.");
        file.Append($"public static string {mangler.SmStateIdToStringFuncName}({mangler.SmStateEnumType} id)");
        file.StartCodeBlock();
        {
            file.Append("switch (id)");
            file.StartCodeBlock();
            {
                foreach (var state in sm.GetNamedVerticesCopy())
                {
                    file.AppendLine($"case {mangler.SmQualifiedStateEnumValue(state)}: return \"{mangler.SmStateToString(state)}\";");
                }
                file.AppendLine("default: return \"?\";");
            }
            file.FinishCodeBlock(forceNewLine: true);
        }
        file.FinishCodeBlock(forceNewLine: true);
        file.AppendLine();
    }
}

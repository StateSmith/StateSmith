#nullable enable

using StateSmith.SmGraph;

namespace StateSmith.Output.Algos.Balanced1;

public class AlgoStateIdToString : IAlgoStateIdToString
{
    protected readonly INameMangler mangler;

    public AlgoStateIdToString(INameMangler mangler)
    {
        this.mangler = mangler;
    }

    public void CreateStateIdToStringFunction(OutputFile file, StateMachine sm)
    {
        file.AppendIndentedLine("// Thread safe.");
        file.AppendIndented($"public static string {mangler.SmStateIdToStringFuncName}({mangler.SmStateEnumType} id)");
        file.StartCodeBlock();
        {
            file.AppendIndented("switch (id)");
            file.StartCodeBlock();
            {
                foreach (var state in sm.GetNamedVerticesCopy())
                {
                    file.AppendIndentedLine($"case {mangler.SmQualifiedStateEnumValue(state)}: return \"{mangler.SmStateToString(state)}\";");
                }
                file.AppendIndentedLine("default: return \"?\";");
            }
            file.FinishCodeBlock(forceNewLine: true);
        }
        file.FinishCodeBlock(forceNewLine: true);
        file.AppendIndentedLine();
    }
}

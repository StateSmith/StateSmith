#nullable enable

using StateSmith.SmGraph;

namespace StateSmith.Output.Algos.Balanced1;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/535
/// </summary>
public class GetParentIdFunctionGen
{
    public static void Generate(OutputFile file, StateMachine sm, NameMangler mangler)
    {
        file.AppendIndentedLine("// Returns the parent state for a given state. Returns ROOT if input has no parent.");
        file.AppendIndentedLine("// Thread safe. This function can be disabled with `outputGetParentIdFunction` setting.");
        file.AppendIndented($"public static {mangler.SmStateEnumType} {mangler.SmGetParentIdFuncName}({mangler.SmStateEnumType} id)");
        file.StartCodeBlock();
        {
            file.AppendIndented("switch (id)");
            file.StartCodeBlock();
            {
                foreach (var state in sm.GetOrderedNamedVerticesCopy())
                {
                    // the ROOT state will return itself as a parent
                    NamedVertex parent = state.ParentState ?? sm;
                    file.AppendIndentedLine($"case {mangler.SmQualifiedStateEnumValue(state)}: return {mangler.SmQualifiedStateEnumValue(parent)};");
                }
                file.AppendIndentedLine($"default: return {mangler.SmQualifiedStateEnumValue(sm)};");
            }
            file.FinishCodeBlock(forceNewLine: true);
        }
        file.FinishCodeBlock(forceNewLine: true);
        file.AppendIndentedLine();
    }
}

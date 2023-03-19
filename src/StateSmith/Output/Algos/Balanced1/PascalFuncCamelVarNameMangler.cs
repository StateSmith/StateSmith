#nullable enable

namespace StateSmith.Output.Algos.Balanced1;

public class PascalFuncCamelVarNameMangler : AbstractMangler
{
    public override string MangleVarName(string snakeCaseName) => StringUtils.SnakeCaseToCamelCase(snakeCaseName);
    public override string MangleFuncName(string originalName) => StringUtils.SnakeCaseToPascalCase(originalName);
}

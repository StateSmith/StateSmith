#nullable enable

using Microsoft.CodeAnalysis.CSharp.Syntax;
using StateSmith;
using StateSmith.Output.C99BalancedCoder1;
using System.Reflection.Metadata;

namespace StateSmith.Output.Gil;

public class GilAlgoHelper
{
    public const string GilNoEmitPrefix = "____GilNoEmit_";
    public const string GilNoEmitEchoStringBoolFuncName = GilNoEmitPrefix + "echoStringBool";

    public static void AppendGilHelpersFuncs(OutputFile file)
    {
        file.AppendLine($"public bool {GilNoEmitEchoStringBoolFuncName}(string toEcho) {{ return true; }}");
    }

    public static string WrapRawCodeWithBoolReturn(string codeToWrap)
    {
        codeToWrap = Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(codeToWrap, quote: true);
        return $"{GilNoEmitEchoStringBoolFuncName}({codeToWrap})";
    }

    public static bool IsGilNoEmit(string identifierName)
    {
        return identifierName.StartsWith(GilNoEmitPrefix);
    }

    public static bool IsGilNoEmit(MethodDeclarationSyntax node)
    {
        return IsGilNoEmit(node.Identifier.ValueText);
    }
}

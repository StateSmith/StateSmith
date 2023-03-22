#nullable enable

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StateSmith.Output.Gil;

public class GilCreationHelper
{
    public const string GilNoEmitPrefix = "____GilNoEmit_";
    public const string GilNoEmitEchoStringBoolFuncName = GilNoEmitPrefix + "echoStringBool";
    public const string GilUnusedVarFuncName = GilNoEmitPrefix + "GilUnusedVar";
    public const string GilFileTopClassName = GilNoEmitPrefix + "FileTop";

    public static void AppendGilHelpersFuncs(OutputFile file)
    {
        file.AppendLine($"public static bool {GilNoEmitEchoStringBoolFuncName}(string toEcho) {{ return true; }}");
        file.AppendLine($"public static void {GilUnusedVarFuncName}(object unusedVar) {{ }}");
    }

    public static string MarkVarAsUnused(string varName)
    {
        return $"{GilUnusedVarFuncName}({varName});";
    }

    /// <summary>
    /// currently can only be called once.
    /// </summary>
    public static void AddFileTopComment(OutputFile file, string gilCommentCode)
    {
        file.AppendLine(gilCommentCode);
        file.AppendLine($"public class {GilFileTopClassName} {{ }}");
    }

    public static string WrapRawCodeWithBoolReturn(string codeToWrap)
    {
        codeToWrap = SymbolDisplay.FormatLiteral(codeToWrap, quote: true);
        return $"{GilNoEmitEchoStringBoolFuncName}({codeToWrap})";
    }
}

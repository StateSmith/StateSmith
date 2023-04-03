#nullable enable

using Microsoft.CodeAnalysis.CSharp;

namespace StateSmith.Output.Gil;

public class GilCreationHelper
{
    /// <summary>
    /// Helps prevent clashing with code to transpile.
    /// </summary>
    public const string GilPrefix = "____GilNoEmit_";

    /// <summary>
    /// The string passed to this function will be unescaped and then emitted without the transpiler getting to see it.
    /// This is useful for outputting user code that isn't valid GIL/C# code that the transpiler would otherwise error on.
    /// </summary>
    public const string GilEchoStringBoolReturnFuncName = GilPrefix + "EchoStringBool";

    /// <summary>
    /// Causes transpiler to visit a variable number of arguments passed to this function. Has a fake bool return so
    /// that it can be used where a bool is expected (like in guard code).
    /// </summary>
    public const string GilVisitVarArgsBoolReturnFuncName = GilPrefix + "VarArgsToBool";

    /// <summary>
    /// Gives any interested transpilers a chance to mark a variable as unused.
    /// </summary>
    public const string GilUnusedVarFuncName = GilPrefix + "UnusedVar";

    /// <summary>
    /// Allows us to output a GIL comment at the top of the file for the transpiler to handle.
    /// This is not the same as the RenderConfig FileTop. Transpilers output that directly.
    /// </summary>
    public const string GilFileTopClassName = GilPrefix + "FileTop";

    /// <summary>
    /// `$gil(code...)`
    /// </summary>
    public const string GilExpansionMarkerFuncName = "$gil";

    public static void AppendGilHelpersFuncs(OutputFile file)
    {
        file.AppendLine($"public static bool {GilEchoStringBoolReturnFuncName}(string toEcho) {{ return true; }}");
        file.AppendLine($"public static bool {GilVisitVarArgsBoolReturnFuncName}(params object[] args) {{ return true; }}");
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

    /// <summary>
    /// This is useful for outputting user code that isn't valid GIL/C# code that the transpiler would otherwise error on.
    /// </summary>
    public static string WrapRawCodeWithBoolReturn(string codeToWrap)
    {
        codeToWrap = SymbolDisplay.FormatLiteral(codeToWrap, quote: true);
        return $"{GilEchoStringBoolReturnFuncName}({codeToWrap})";
    }

    public static string MarkAsGilExpansionCode(string code)
    {
        return $"{GilExpansionMarkerFuncName}({code})";
    }
}

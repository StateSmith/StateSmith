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
    public const string GilFuncName_EchoStringBool = GilPrefix + "EchoStringBool";

    /// <summary>
    /// The string passed to this function will be unescaped and then emitted without the transpiler getting to see it.
    /// This is useful for outputting user code that isn't valid GIL/C# code that the transpiler would otherwise error on.
    /// </summary>
    public const string GilFuncName_EchoStringStatement = GilPrefix + "EchoStringVoid";

    /// <summary>
    /// Causes transpiler to visit a variable number of arguments passed to this function. Has a fake bool return so
    /// that it can be used where a bool is expected (like in guard code).
    /// </summary>
    public const string GilFuncName_VarArgsToBool = GilPrefix + "VarArgsToBool";

    /// <summary>
    /// Gives any interested transpilers a chance to mark a variable as unused.
    /// </summary>
    public const string GilFuncName_UnusedVar = GilPrefix + "UnusedVar";

    /// <summary>
    /// Allows us to output a GIL comment at the top of the file for the transpiler to handle.
    /// This is not the same as the RenderConfig FileTop. Transpilers output that directly.
    /// </summary>
    public const string GilClassName_FileTop = GilPrefix + "FileTop";

    /// <summary>
    /// `$gil(code...)`
    /// </summary>
    public const string GilExpansionMarkerFuncName = "$gil";

    public static void AppendGilHelpersFuncs(OutputFile file)
    {
        file.AppendIndentedLine($"public static bool {GilFuncName_EchoStringBool}(string toEcho) {{ return true; }}");
        file.AppendIndentedLine($"public static void {GilFuncName_EchoStringStatement}(string toEcho) {{ }}");
        file.AppendIndentedLine($"public static bool {GilFuncName_VarArgsToBool}(params object[] args) {{ return true; }}");
        file.AppendIndentedLine($"public static void {GilFuncName_UnusedVar}(object unusedVar) {{ }}");
    }

    public static string MarkVarAsUnused(string varName)
    {
        return $"{GilFuncName_UnusedVar}({varName});";
    }

    /// <summary>
    /// currently can only be called once.
    /// </summary>
    public static void AddFileTopComment(OutputFile file, string gilCommentCode)
    {
        file.AppendIndentedLine(gilCommentCode);
        file.AppendIndentedLine($"public class {GilClassName_FileTop} {{ }}");
    }

    /// <summary>
    /// This is useful for outputting user code that isn't valid GIL/C# code that the transpiler would otherwise error on.
    /// </summary>
    public static string WrapRawCodeWithBoolReturn(string codeToWrap)
    {
        var escapedQuoted = SymbolDisplay.FormatLiteral(codeToWrap, quote: true);
        return $"{GilFuncName_EchoStringBool}({escapedQuoted})";
    }

    /// <summary>
    /// This is useful for outputting user code that isn't valid GIL/C# code that the transpiler would otherwise error on.
    /// </summary>
    public static string WrapRawCodeWithVoidReturn(string codeToWrap)
    {
        string result = "";
        string joiner = "";

        // split code into multiple lines
        string[] lines = StringUtils.SplitIntoLinesOrEmpty(codeToWrap);

        foreach (var line in lines)
        {
            result += joiner;
            var escapedQuoted = SymbolDisplay.FormatLiteral(line, quote: true);
            result += $"{GilFuncName_EchoStringStatement}({escapedQuoted});";
            joiner = "\n";
        }
        
        return result;
    }

    public static string MarkAsGilExpansionCode(string code)
    {
        return $"{GilExpansionMarkerFuncName}({code})";
    }
}

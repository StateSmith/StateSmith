#nullable enable

using System;
using StateSmith.SmGraph;
using StateSmith.Input.Antlr4;
using StateSmith.Output.Gil;
using System.Text.RegularExpressions;

namespace StateSmith.Input.Expansions;

// See https://github.com/StateSmith/StateSmith/wiki/Z:-mixing-GIL-and-user-code
public class WrappingExpander
{
    public const string XML_FINAL_CODE_TAG_NAME = "finalCode";
    public const string XML_FINAL_CODE_TAG = $"<{XML_FINAL_CODE_TAG_NAME}>";
    public const string XML_GIL_CODE_TAG_NAME = "gilCode";
    public const string XML_GIL_CODE_TAG = $"<{XML_GIL_CODE_TAG_NAME}>";
    public const string XML_END_CODE_TAG = "</code>";

    private readonly Expander expander;
    private static readonly Regex codeTypeSplittingRegex = new($@"<(?<type>{XML_FINAL_CODE_TAG_NAME}|{XML_GIL_CODE_TAG_NAME})>(?<value>[\s\S]*?){XML_END_CODE_TAG}"); // MUST be lazy match to prevent skipping over multiple matches

    public WrappingExpander(Expander expander)
    {
        this.expander = expander;
    }

    /// <summary>
    /// Handles something like `$gil(code)`
    /// </summary>
    /// <param name="code">Code that should be transpiled as GIL code. It was everything inside `$gil(code)` parenthesis.</param>
    /// <returns></returns>
    public static string HandleGilFunctionCode(string code)
    {
        return $"{XML_END_CODE_TAG}{XML_GIL_CODE_TAG}{code}{XML_END_CODE_TAG}{XML_FINAL_CODE_TAG}";
    }

    public string ExpandWrapGuardCode(Behavior b)
    {
        return ExpandWrapCode(b.guardCode, isGuard: true);
    }

    public string ExpandWrapActionCode(Behavior b)
    {
        return ExpandWrapCode(b.actionCode, isGuard: false);
    }

    public string ExpandWrapCode(string code, bool isGuard)
    {
        string expanded = ExpandCode(code);

        expanded = $"{XML_FINAL_CODE_TAG}{expanded}{XML_END_CODE_TAG}";

        if (isGuard)
            return WrapExpandedForGuard(expanded);

        return WrapExpandedForAction(expanded);
    }

    public static string WrapExpandedForGuard(string expanded)
    {
        int count = 0;
        var wrappedArgs = codeTypeSplittingRegex.Replace(expanded, (match) =>
        {
            string codeType = match.Groups["type"].Value;
            string value = match.Groups["value"].Value;
            if (value.Length == 0)
                return "";

            string partResult = "";
            count++;

            if (count > 1)
                partResult += ",";

            if (codeType == XML_FINAL_CODE_TAG_NAME)
                partResult += GilCreationHelper.WrapRawCodeWithBoolReturn(value);
            else if (codeType == XML_GIL_CODE_TAG_NAME)
                partResult += value;
            else
                throw new ArgumentException($"unknown code type `{codeType}`");

            return partResult;
        });

        var result = GilCreationHelper.GilVisitVarArgsBoolReturnFuncName + "(" + wrappedArgs + ")";
        return result;
    }

    public static string WrapExpandedForAction(string expanded)
    {
        var result = codeTypeSplittingRegex.Replace(expanded, (match) =>
        {
            string codeType = match.Groups["type"].Value;
            string value = match.Groups["value"].Value;
            if (value.Length == 0)
                return "";

            string partResult = "";

            if (codeType == XML_FINAL_CODE_TAG_NAME)
                partResult += PostProcessor.RmCommentOut(value);
            else if (codeType == XML_GIL_CODE_TAG_NAME)
                partResult += value;
            else
                throw new ArgumentException($"unknown code type `{codeType}`");

            return partResult;
        });

        return result;
    }

    public string ExpandCode(string code)
    {
        return ExpandingVisitor.ParseAndExpandCode(expander, code);
    }
}

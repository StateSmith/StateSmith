#nullable enable

using StateSmith.Output.UserConfig;
using System;
using System.Text.RegularExpressions;

namespace StateSmith.Output.Gil.C99;

// spell-checker: ignore customizer

public class GilToC99Customizer : IGilToC99Customizer
{
    protected RenderConfigCVars renderConfigCVars;
    protected IOutputInfo outputInfo;

    public GilToC99Customizer(RenderConfigCVars renderConfigCVars, IOutputInfo outputInfo)
    {
        this.renderConfigCVars = renderConfigCVars;
        this.outputInfo = outputInfo;
    }

    public Func<string> HFileNameBuilder = () => throw new InvalidOperationException("IGilToC99Customizer.Setup() needs to be called");
    public Func<string> CFileNameBuilder = () => throw new InvalidOperationException("IGilToC99Customizer.Setup() needs to be called");
    public Func<string> HGuardBuilder = () => throw new InvalidOperationException("IGilToC99Customizer.Setup() needs to be called");
    public Func<string, string> EnumDeclarationBuilder = (string enumName) => $"typedef enum {enumName}";

    public void Setup()
    {
        HFileNameBuilder = () => $"{outputInfo.BaseFileName}{renderConfigCVars.HFileExtension}";
        CFileNameBuilder = () => $"{outputInfo.BaseFileName}{renderConfigCVars.CFileExtension}";
        HGuardBuilder = () => $"{outputInfo.BaseFileName.ToUpper()}_H";

        SupportEnumDeclarer();
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/185
    /// </summary>
    private void SupportEnumDeclarer()
    {
        string enumDeclarerStr = renderConfigCVars.CEnumDeclarer;
        if (enumDeclarerStr.Length > 0)
        {
            EnumDeclarationBuilder = (string enumName) =>
            {
                var regex = new Regex(@"[{]\s*enumName\s*[}]");
                var enumDeclaration = regex.Replace(enumDeclarerStr, enumName);
                return enumDeclaration;
            };
        }
    }

    string IGilToC99Customizer.MakeHFileName() => HFileNameBuilder();
    string IGilToC99Customizer.MakeCFileName() => CFileNameBuilder();
    string IGilToC99Customizer.MakeHGuard() => HGuardBuilder();
    string IGilToC99Customizer.MakeEnumDeclaration(string enumName) => EnumDeclarationBuilder(enumName);
}

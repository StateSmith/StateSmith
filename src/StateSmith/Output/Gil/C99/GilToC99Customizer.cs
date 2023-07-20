#nullable enable

using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using System;
using System.Text.RegularExpressions;

namespace StateSmith.Output.Gil.C99;

// spell-checker: ignore customizer

public class GilToC99Customizer : IGilToC99Customizer
{
    protected RenderConfigCVars renderConfigCVars;
    protected ISmBaseFileNameProvider baseFileNameProvider;

    public GilToC99Customizer(RenderConfigCVars renderConfigCVars, ISmBaseFileNameProvider baseFileNameProvider)
    {
        this.renderConfigCVars = renderConfigCVars;
        this.baseFileNameProvider = baseFileNameProvider;
    }

    public Func<ISmBaseFileNameProvider, string> HFileNameBuilder = (_) => throw new InvalidOperationException("IGilToC99Customizer.Setup() needs to be called");
    public Func<ISmBaseFileNameProvider, string> CFileNameBuilder = (_) => throw new InvalidOperationException("IGilToC99Customizer.Setup() needs to be called");
    public Func<string, string> EnumDeclarationBuilder = (string enumName) => $"typedef enum {enumName}";

    public void Setup()
    {
        HFileNameBuilder = (ISmBaseFileNameProvider nameProvider) => $"{nameProvider.Get()}{renderConfigCVars.HFileExtension}";
        CFileNameBuilder = (ISmBaseFileNameProvider nameProvider) => $"{nameProvider.Get()}{renderConfigCVars.CFileExtension}";

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

    string IGilToC99Customizer.MakeHFileName() => HFileNameBuilder(baseFileNameProvider);
    string IGilToC99Customizer.MakeCFileName() => CFileNameBuilder(baseFileNameProvider);
    string IGilToC99Customizer.MakeEnumDeclaration(string enumName) => EnumDeclarationBuilder(enumName);
}

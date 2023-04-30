#nullable enable

using StateSmith.Common;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.SmGraph;
using System;
using System.Text.RegularExpressions;

namespace StateSmith.Output.Gil.C99;

// spell-checker: ignore customizer

public class GilToC99Customizer : IGilToC99Customizer
{
    protected StateMachineProvider stateMachineProvider;
    protected StateMachine Sm => stateMachineProvider.GetStateMachine().ThrowIfNull();
    protected RenderConfigCVars renderConfigCVars;

    public GilToC99Customizer(StateMachineProvider stateMachineProvider, RenderConfigCVars renderConfigCVars)
    {
        this.stateMachineProvider = stateMachineProvider;
        this.renderConfigCVars = renderConfigCVars;
    }

    public Func<StateMachine, string> HFileNameBuilder = (StateMachine sm) => throw new InvalidOperationException("IGilToC99Customizer.Setup() needs to be called");
    public Func<StateMachine, string> CFileNameBuilder = (StateMachine sm) => throw new InvalidOperationException("IGilToC99Customizer.Setup() needs to be called");
    public Func<string, string> EnumDeclarationBuilder = (string enumName) => $"typedef enum {enumName}";

    public void Setup()
    {
        HFileNameBuilder = (StateMachine sm) => $"{sm.Name}{renderConfigCVars.HFileExtension}";
        CFileNameBuilder = (StateMachine sm) => $"{sm.Name}{renderConfigCVars.CFileExtension}";

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

    string IGilToC99Customizer.MakeHFileName() => HFileNameBuilder(Sm);
    string IGilToC99Customizer.MakeCFileName() => CFileNameBuilder(Sm);
    string IGilToC99Customizer.MakeEnumDeclaration(string enumName) => EnumDeclarationBuilder(enumName);
}

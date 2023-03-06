#nullable enable

using StateSmith.Common;
using StateSmith.Runner;
using StateSmith.SmGraph;
using System;

namespace StateSmith.Output.Gil.C99;

// spell-checker: ignore customizer

public class GilToC99Customizer : IGilToC99Customizer
{
    protected StateMachineProvider stateMachineProvider;
    protected StateMachine Sm => stateMachineProvider.GetStateMachine().ThrowIfNull();

    public GilToC99Customizer(StateMachine sm) : this(new StateMachineProvider(sm)) { }

    public GilToC99Customizer(StateMachineProvider stateMachineProvider)
    {
        this.stateMachineProvider = stateMachineProvider;
    }

    public Func<StateMachine, string> HFileNameBuilder = (StateMachine sm) => $"{sm.Name}.h";
    public Func<StateMachine, string> CFileNameBuilder = (StateMachine sm) => $"{sm.Name}.c";
    public Func<string, string> EnumDeclarationBuilder = (string enumName) => $"typedef enum {enumName}";

    string IGilToC99Customizer.MakeHFileName() => HFileNameBuilder(Sm);
    string IGilToC99Customizer.MakeCFileName() => CFileNameBuilder(Sm);
    string IGilToC99Customizer.MakeEnumDeclaration(string enumName) => EnumDeclarationBuilder(enumName);
}

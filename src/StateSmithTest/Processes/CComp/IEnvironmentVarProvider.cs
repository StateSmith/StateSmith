#nullable enable

namespace StateSmithTest.Processes.CComp;

public interface IEnvironmentVarProvider
{
    string? CompilerId { get; }
    string? CompilerPath { get; }
}

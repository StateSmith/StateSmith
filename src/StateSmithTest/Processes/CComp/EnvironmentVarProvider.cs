#nullable enable

using System;

namespace StateSmithTest.Processes.CComp;

public class EnvironmentVarProvider : IEnvironmentVarProvider
{
    // SST stands for StateSmith Test
    public string? CompilerId => GetVar("SST_C_COMP_ID");

    // SST stands for StateSmith Test
    public string? CompilerPath => GetVar("SST_C_COMP_PATH");

    public static string? GetVar(string varName)
    {
        string? value;

        var isRunningInVS = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VisualStudioEdition"));

        if (isRunningInVS)
        {
            // In Visual Studio, the process scope is not updated when the environment variables are changed.
            // So, we need to read the environment variables from the registry.
            value = Environment.GetEnvironmentVariable(varName, EnvironmentVariableTarget.User);
        }
        else
        {
            value = Environment.GetEnvironmentVariable(varName, EnvironmentVariableTarget.Process);

            if (string.IsNullOrEmpty(value))
            {
                // It looks like EnvironmentVariableTarget.User vars are not available for Unix processes.
                // On Windows, the user vars are only updated after the terminal is restarted.
                value = Environment.GetEnvironmentVariable(varName, EnvironmentVariableTarget.User);
            }
        }

        return value;
    }
}

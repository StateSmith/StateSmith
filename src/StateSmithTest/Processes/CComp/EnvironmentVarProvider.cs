#nullable enable

using System;

namespace StateSmithTest.Processes.CComp;

public class EnvironmentVarProvider : IEnvironmentVarProvider
{
    // SST stands for StateSmith Test
    // NOTE: we use User scope to avoid needing to restart Visual Studio. https://stackoverflow.com/a/41410599/7331858
    public string? CompilerId => Environment.GetEnvironmentVariable("SST_C_COMP_ID", EnvironmentVariableTarget.User);

    // SST stands for StateSmith Test
    // NOTE: we use User scope to avoid needing to restart Visual Studio. https://stackoverflow.com/a/41410599/7331858
    public string? CompilerPath => Environment.GetEnvironmentVariable("SST_C_COMP_PATH", EnvironmentVariableTarget.User);
}

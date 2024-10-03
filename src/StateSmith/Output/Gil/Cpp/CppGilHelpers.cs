#nullable enable

using StateSmith.Output.Gil.C99;
using StateSmith.Output.UserConfig;

namespace StateSmith.Output.Gil.Cpp;

public class CppGilHelpers
{
    public readonly RenderConfigCppVars renderConfigCpp;
    public readonly RenderConfigBaseVars renderConfig;
    public readonly RoslynCompiler roslynCompiler;
    public readonly IncludeGuardProvider includeGuardProvider;
    public readonly CodeStyleSettings codeStyleSettings;
    public readonly IOutputInfo outputInfo;

    public CppGilHelpers(RenderConfigCppVars renderConfigCpp, RenderConfigBaseVars renderConfig, RoslynCompiler roslynCompiler, IncludeGuardProvider includeGuardProvider, IOutputInfo outputInfo, CodeStyleSettings codeStyleSettings)
    {
        this.renderConfigCpp = renderConfigCpp;
        this.renderConfig = renderConfig;
        this.roslynCompiler = roslynCompiler;
        this.includeGuardProvider = includeGuardProvider;
        this.outputInfo = outputInfo;
        this.codeStyleSettings = codeStyleSettings;
    }
}
